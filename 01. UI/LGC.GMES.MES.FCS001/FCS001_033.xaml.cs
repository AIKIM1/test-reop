/*************************************************************************************
 Created Date : 2020.10.15
      Creator : Dooly
   Decription : Lot Hold 및 특성 투입여부
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.15  DEVELOPER : Initial Created.
  2023.06.29  박승렬 : E20230629-001095 / "특성투입가능설정" 버튼 클릭 시 이벤트 발생 조건에 검사의뢰 ID 추가


       


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_033 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        private string _SHOPID = string.Empty;
        public FCS001_033()
        {
            InitializeComponent();
            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            ////동
            //C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            //_combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.ALL, cbChild: cboAreaChild);

            ////Login 한 AREA Setting
            //cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            ////라인
            //C1ComboBox[] cboLineParent = { cboArea };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, cbParent: cboLineParent);

            //라인
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            //모델
            C1ComboBox[] cboModelParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            string[] sFilter = { "RELEASE_YN" };
            _combo.SetCombo(cboRelease, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //날짜
            dtpFrom.SelectedDateTime = System.DateTime.Now.AddDays(-1);

        }
        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnEolInputYN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sMsgID = string.Empty;
                Button btnEolInput = sender as Button;
                if (btnEolInput != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                    if (string.Equals(btnEolInput.Name, "btnEolInputYN"))
                    {
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("HOLD_ID", typeof(string));
                        RQSTDT.Columns.Add("FORM_UNHOLD_FLAG", typeof(string));
                        RQSTDT.Columns.Add("USERID", typeof(string));
                        RQSTDT.Columns.Add("INSP_REQ_ID", typeof(string)); // 특성투입여부 변경 시 검사의뢰 ID 조건 추가_0629 

                        DataRow dr = RQSTDT.NewRow();
                        dr["HOLD_ID"] = Util.NVC(dataRow.Row["HOLD_ID"]);
                        //dr["HOLD_ID"] = Util.NVC(dataRow.Row["MDF_ID"]);
                        dr["USERID"] = LoginInfo.USERID;
                        dr["INSP_REQ_ID"] = Util.NVC(dataRow.Row["INSP_REQ_ID"]); // 특성투입여부 변경 시 검사의뢰 ID 조건 추가_0629 

                        if ((Util.NVC(dataRow.Row["FORM_UNHOLD_FLAG"]) == "Y") || (Util.NVC(dataRow.Row["UNHOLD_FLAG"]) == "Y"))
                        {
                            dr["FORM_UNHOLD_FLAG"] = "N";
                            sMsgID = "FM_ME_0332";  //특성 작업 가능 설정을 취소하시겠습니까?
                        }
                        else
                        {
                            dr["FORM_UNHOLD_FLAG"] = "Y";
                            sMsgID = "FM_ME_0247";  //특성 작업이 가능하도록 변경하시겠습니까?
                        }
                        RQSTDT.Rows.Add(dr);

                        //요청하시겠습니까?
                        Util.MessageConfirm(sMsgID, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ShowLoadingIndicator();
                                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_UPD_TC_LOT_HOLD_PACK_FCS", "RQSTDT", "RSLTDT", RQSTDT);
                                GetList();
                            }
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgLotHoldOCV_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //특성 투입 가능 설정 버튼 Control
                if (e.Cell.Column.Name.Equals("EOLINPUT"))
                {
                    //Button btn = dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as Button;

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "UNHOLD_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.IsEnabled = false;
                        //btn.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        e.Cell.Presenter.IsEnabled = true;
                        //btn.Visibility = Visibility.Visible;
                    }
                }
            }));
        }

        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("HOLD_ID", typeof(string));
                RQSTDT.Columns.Add("UNHOLD_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true); 
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true); 
                dr["HOLD_ID"] = Util.GetCondition(txtHoldId);
                dr["UNHOLD_FLAG"] = Util.GetCondition(cboRelease, bAllNull: true);
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOTHOLD_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgLotHoldOCV, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

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


    }
}
