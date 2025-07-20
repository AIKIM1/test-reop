/*************************************************************************************
 Created Date : 2018.09.18
      Creator : 
   Decription : 자동차 활성화 후공정 - 불량 Lot 자동 차감 처리
--------------------------------------------------------------------------------------
 [Change History]


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_502 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_502()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            //dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            //dtpDateTo.SelectedDateTime = DateTime.Now;
        }
        private void SetControl()
        {
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_AUTO", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbParent: cboProcessParent);

            //구분
            string[] sFilter = { "FORM_WIP_TYPE_CODE" };
            _combo.SetCombo(cboGubun, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchProcess(false, tb);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchProcess(true);
        }

        private void dgGoodChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        // row 색 바꾸기
                        dgList.SelectedIndex = idx;

                        // Cell 정보 조회
                        DataTable dtMin = DataTableConverter.Convert(dgList.ItemsSource).Select("LOTID = '" + drv.Row["LOTID"].ToString() + "' And ACTID = '" + drv.Row["ACTID"].ToString() + "'").CopyToDataTable();

                        var MinMax = from dt in dtMin.AsEnumerable()
                                     group dt by dt.Field<string>("LOTID") into grp
                                     select new
                                     {
                                         Min = grp.Min(T => T.Field<string>("ACTDTTM_MIN")),
                                         Max = grp.Max(T => T.Field<string>("ACTDTTM"))
                                     };

                        foreach (var data in MinMax)
                        {
                            SearchCellProcess(data.Min, data.Max, Util.NVC(drv.Row["LOTID"].ToString())
                                            , Util.NVC(drv.Row["ACTID"].ToString())
                                             , Util.NVC(drv.Row["LOTID_RT"].ToString())
                                            , Util.NVC(drv.Row["FORM_WIP_TYPE_CODE"].ToString()));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 양품화 Lot 조회
        /// </summary>
        private void SearchProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtProdid"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("제품"));
                            return;
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량LOT"));
                            return;
                        }
                    }
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("FORM_WIP_TYPE_CODE", typeof(string));
                inTable.Columns.Add("DEFECT_LOT", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateTo);

                if (!string.IsNullOrWhiteSpace(txtDefectLot.Text))
                {
                    newRow["DEFECT_LOT"] = txtDefectLot.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtProdid.Text))
                {
                    newRow["PRODID"] = txtProdid.Text;
                    newRow["FORM_WIP_TYPE_CODE"] = string.IsNullOrWhiteSpace(cboGubun.SelectedValue.ToString()) ? null : cboGubun.SelectedValue.ToString();
                }
                else
                {
                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                    if (newRow["EQSGID"].Equals("")) return;

                    newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcess.SelectedValue.ToString()) ? null : cboProcess.SelectedValue.ToString();
                    newRow["FORM_WIP_TYPE_CODE"] = string.IsNullOrWhiteSpace(cboGubun.SelectedValue.ToString()) ? null : cboGubun.SelectedValue.ToString();

                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_FORM_GOOD_LIST_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 양품  이력 Cell 조회
        /// </summary>
        private void SearchCellProcess(string FromDT, string ToDT, string DefectLot, string ActID, string AssyLot, string FormTypeCode)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FR_ACTDTTM", typeof(string));
                inTable.Columns.Add("TO_ACTDTTM", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("RESNGRID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));
                inTable.Columns.Add("FORM_WIP_TYPE_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_ACTDTTM"] = FromDT;
                newRow["TO_ACTDTTM"] = ToDT;
                newRow["LOTID"] = DefectLot;
                newRow["RESNGRID"] = "DEFECT_EQPOUT_AUTO,DEFECT_SURFACE_AUTO";
                newRow["ACTID"] = ActID;
                newRow["LOTID_RT"] = AssyLot;
                newRow["FORM_WIP_TYPE_CODE"] = FormTypeCode;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_DEFECT_CELL_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistoryCell, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #endregion

    }
}
