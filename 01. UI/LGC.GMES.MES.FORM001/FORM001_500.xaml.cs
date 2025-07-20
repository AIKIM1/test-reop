/*************************************************************************************
 Created Date : 2018.09.17
      Creator : 
   Decription : 자동차 활성화 후공정 - 설비 불량 Lot 관리
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
    public partial class FORM001_500 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FORM001_500()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            btnPrint.Visibility = Visibility.Collapsed;

            //dtpDateFromLot.SelectedDateTime = DateTime.Now.AddDays(-7);
            //dtpDateToLot.SelectedDateTime = DateTime.Now;
            //dtpDateFromHis.SelectedDateTime = DateTime.Now.AddDays(-7);
            //dtpDateToHis.SelectedDateTime = DateTime.Now;
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

            // 설비 불량 Lot 정보
            //동
            C1ComboBox[] cboAreaLotChild = { cboEquipmentSegmentLot };
            _combo.SetCombo(cboAreaLot, CommonCombo.ComboStatus.NONE, cbChild: cboAreaLotChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentLotParent = { cboAreaLot };
            C1ComboBox[] cboEquipmentSegmentLotChild = { cboProcessLot };
            _combo.SetCombo(cboEquipmentSegmentLot, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_AUTO", cbChild: cboEquipmentSegmentLotChild, cbParent: cboEquipmentSegmentLotParent);

            //공정
            C1ComboBox[] cboProcessLotParent = { cboEquipmentSegmentLot };
            _combo.SetCombo(cboProcessLot, CommonCombo.ComboStatus.ALL, sCase: "PROCESS",  cbParent: cboProcessLotParent);

            // 설비 불량 Lot 생성 이력
            //동
            C1ComboBox[] cboAreaHisChild = { cboEquipmentSegmentHis };
            _combo.SetCombo(cboAreaHis, CommonCombo.ComboStatus.NONE, cbChild: cboAreaHisChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentHisParent = { cboAreaHis };
            C1ComboBox[] cboEquipmentSegmentHisChild = { cboProcessHis };
            _combo.SetCombo(cboEquipmentSegmentHis, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_AUTO", cbChild: cboEquipmentSegmentHisChild, cbParent: cboEquipmentSegmentHisParent);

            //공정
            C1ComboBox[] cboProcessHisParent = { cboEquipmentSegmentHis };
            _combo.SetCombo(cboProcessHis, CommonCombo.ComboStatus.ALL, sCase: "PROCESS", cbParent: cboProcessHisParent);

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearchLot);
            //listAuth.Add(btnSearchHis);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        #region [설비 불량 Lot 정보]
        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchLotProcess(false, tb);
            }
        }

        private void btnSearchLot_Click(object sender, RoutedEventArgs e)
        {
            SearchLotProcess(true);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint())
                return;

            DataRow[] dr = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select("CHK = 1");

            PopupPrint(dr);
        }

        private void dgDefectLotChoice_Checked(object sender, RoutedEventArgs e)
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
                        dgDefectLot.SelectedIndex = idx;

                        // Cell 정보 조회
                        SearchCellProcess(Util.NVC(drv.Row["LOTID"].ToString()), Util.NVC(drv.Row["LOTID_RT"].ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [설비 불량 Lot 생성 이력]
        private void txtHis_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox tb = sender as TextBox;

                SearchHistoryProcess(false, tb);
            }
        }

        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            SearchHistoryProcess(true);
        }

        private void dgHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("CELLQTY"))
                    {
                        if (Util.NVC_Int(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CELLQTY").GetString()).Equals(-1))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        private void dgDefectHistoryChoice_Checked(object sender, RoutedEventArgs e)
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
                        dgHistory.SelectedIndex = idx;

                        // Cell 정보 조회
                        DataTable dtMin = DataTableConverter.Convert(dgHistory.ItemsSource).Select("LOTID = '" + drv.Row["LOTID"].ToString() + "' And ACTID = '" + drv.Row["ACTID"].ToString() + "'").CopyToDataTable();

                        var MinMax = from dt in dtMin.AsEnumerable()
                                     group dt by dt.Field<string>("LOTID") into grp
                                     select new
                                     {
                                         Min = grp.Min(T => T.Field<string>("ACTDTTM_MIN")),
                                         Max = grp.Max(T => T.Field<string>("ACTDTTM"))
                                     };

                        foreach (var data in MinMax)
                        {
                            SearchCellHistoryProcess(data.Min, data.Max, Util.NVC(drv.Row["LOTID"].ToString())
                                                   , Util.NVC(drv.Row["ACTID"].ToString())
                                                   , Util.NVC(drv.Row["LOTID_RT"].ToString()));
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

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 설비 불량 Lot 조회
        /// </summary>
        private void SearchLotProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if ((dtpDateToLot.SelectedDateTime - dtpDateFromLot.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtSubLotIDLot"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("CELL ID"));
                            return;
                        }
                        else if (tb.Name.Equals("txtProdidLot"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("제품"));
                            return;
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량명"));
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
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("RESNNAME", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromLot);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToLot);

                if (!string.IsNullOrWhiteSpace(txtSubLotIDLot.Text))
                {
                    newRow["SUBLOTID"] = txtSubLotIDLot.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtResnNameLot.Text))
                {
                    newRow["RESNNAME"] = txtResnNameLot.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtProdidLot.Text))
                {
                    newRow["PRODID"] = txtProdidLot.Text;
                }
                else
                { 
                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegmentLot, MessageDic.Instance.GetMessage("SFU1223"));
                    if (newRow["EQSGID"].Equals("")) return;

                    newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcessLot.SelectedValue.ToString()) ? null : cboProcessLot.SelectedValue.ToString();
                }
                inTable.Rows.Add(newRow);

                // Cell 정보 Clear
                Util.gridClear(dgDefectCell);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_DEFECT_LOT_LIST_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgDefectLot, bizResult, FrameOperation, true);

                        // Cell Count(중복제거)
                        DataTable dt = bizResult.DefaultView.ToTable(true, "LOTID", "WIPQTY");
                        decimal CellCount = dt.AsEnumerable().Sum(r => r.Field<decimal>("WIPQTY"));

                        DataGridAggregate.SetAggregateFunctions(dgDefectLot.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = CellCount.ToString("###,###") } });
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
        /// 설비 불량 Cell 조회
        /// </summary>
        private void SearchCellProcess(string DefectLot, string AssyLot)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = DefectLot;
                newRow["LOTID_RT"] = AssyLot;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_DEFECT_LOT_CELL_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgDefectCell, bizResult, FrameOperation, true);
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
        /// 설비 불량 Lot 생성 이력
        /// </summary>
        private void SearchHistoryProcess(bool buttonClick, TextBox tb = null)
        {
            try
            {
                if ((dtpDateToLot.SelectedDateTime - dtpDateFromLot.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (!buttonClick)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        if (tb.Name.Equals("txtSubLotIDHis"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("CELL ID"));
                            return;
                        }
                        else if (tb.Name.Equals("txtProdidHis"))
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("제품"));
                            return;
                        }
                        else
                        {
                            // %1이 입력되지 않았습니다.
                            Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("불량명"));
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
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("RESNNAME", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROM_DATE"] = Util.GetCondition(dtpDateFromHis);
                newRow["TO_DATE"] = Util.GetCondition(dtpDateToHis);

                if (!string.IsNullOrWhiteSpace(txtSubLotIDHis.Text))
                {
                    newRow["SUBLOTID"] = txtSubLotIDHis.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtResnNameHis.Text))
                {
                    newRow["RESNNAME"] = txtResnNameHis.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtProdidHis.Text))
                {
                    newRow["PRODID"] = txtProdidHis.Text;
                }
                else
                {
                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHis, MessageDic.Instance.GetMessage("SFU1223"));
                    if (newRow["EQSGID"].Equals("")) return;

                    newRow["PROCID"] = string.IsNullOrWhiteSpace(cboProcessHis.SelectedValue.ToString()) ? null : cboProcessHis.SelectedValue.ToString();
                }
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_DEFECT_LOT_HIST_AUTO", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, FrameOperation, true);

                        //// Cell Count(중복제거)
                        //DataTable dt = bizResult.DefaultView.ToTable(true, "LOTID", "WIPQTY");
                        //decimal CellCount = dt.AsEnumerable().Sum(r => r.Field<decimal>("WIPQTY"));

                        //DataGridAggregate.SetAggregateFunctions(dgHistory.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = CellCount.ToString("###,###") } });
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
        /// 설비 불량 이력 Cell 조회
        /// </summary>
        private void SearchCellHistoryProcess(string FromDT, string ToDT, string DefectLot, string ActID, string AssyLot)
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

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_ACTDTTM"] = FromDT;
                newRow["TO_ACTDTTM"] = ToDT;
                newRow["LOTID"] = DefectLot;
                newRow["RESNGRID"] = "DEFECT_EQPOUT_AUTO";
                newRow["ACTID"] = ActID;
                newRow["LOTID_RT"] = AssyLot;
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

        #region [Validation]
        private bool ValidationPrint()
        {
            int rowChkCount = DataTableConverter.Convert(dgDefectLot.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (rowChkCount == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        #endregion

        #region [팝업]
        /// <summary>
        /// Sheet발행 팝업
        /// </summary>
        private void PopupPrint(DataRow[] drPrint)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupPrint.FrameOperation = this.FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = "";
            parameters[1] = "";
            parameters[2] = "";   // ButtonCertSelect.Tag;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupPrint, parameters);

            popupPrint.Closed += new EventHandler(popupPrint_Closed);
            grdMain.Children.Add(popupPrint);
            popupPrint.BringToFront();
        }

        private void popupPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //DataTableConverter.SetValue(dgCreate.Rows[_ResnGridRow].DataItem, "ASSYLOT", popup.a);
            }

            grdMain.Children.Remove(popup);
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

        #endregion

    }
}
