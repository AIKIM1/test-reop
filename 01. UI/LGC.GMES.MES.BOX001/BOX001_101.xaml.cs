/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_101 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _PROCID = Process.CHR;

        Util _util = new Util();
        public BOX001_101()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo(); 

            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[]{LoginInfo.CFG_AREA_ID}, cbChild: new C1ComboBox[] {cboEquipment_Search }, sCase: "LINE_CP");
            _combo.SetCombo(cboEquipment_Search, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { _PROCID }, sCase: "cboEquipment");
            _combo.SetCombo(cboWorkType_Search, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "PACK_WRK_TYPE_CODE1" }, sCase: "COMMCODE_WITHOUT_CODE");

            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { string.Empty, _PROCID }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            _combo.SetCombo(cboInBox, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "INBOX_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            _combo.SetCombo(cboWorkType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "PACK_WRK_TYPE_CODE1" }, sCase: "COMMCODE_WITHOUT_CODE");

            SetGridColumnCombo(dgInbox, "PRDT_GRD_CODE", "PRDT_GRD_CODE");
            SetGridColumnCombo(dgInbox, "PRDT_GRD_LEVEL", "PRDT_GRD_LEVEL");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }        
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
        
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = _PROCID;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

                //grdMain.Children.Add(wndPopup);
                //wndPopup.BringToFront();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
            // this.grdMain.Children.Remove(wndPopup);
        }

        private void btnShift_Main_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboLine.SelectedValue);
                Parameters[3] = _PROCID;
                Parameters[4] = Util.NVC(txtShift_Main.Tag);
                Parameters[5] = Util.NVC(txtWorker_Main.Tag);
                Parameters[6] = Util.NVC(cboEquipment_Search.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Main_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void wndShift_Main_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift_Main.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift_Main.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker_Main.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker_Main.Tag = Util.NVC(wndPopup.USERID);              
            }
            this.grdMain.Children.Remove(wndPopup);
        }
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtShift_Main.Text))
            {
                Util.MessageValidation("SFU1845");
                return;
            }

            if (string.IsNullOrEmpty(txtWorker_Main.Text))
            {
                Util.MessageValidation("SFU1843");
                return;
            }

            BOX001_101_RUNSTART popup = new BOX001.BOX001_101_RUNSTART();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[13];
                Parameters[0] = _PROCID;                                
                Parameters[1] = Util.NVC(cboEquipment_Search.SelectedValue);
                Parameters[2] = string.Empty; //작업구분
                Parameters[3] = string.Empty; //inbox종류
                Parameters[4] = string.Empty; //모델lot
                Parameters[5] = string.Empty; //prjt
                Parameters[6] = string.Empty; //prodid
                Parameters[7] = string.Empty; // 출하처id
                Parameters[8] = string.Empty; // 출하처name
                Parameters[9] = txtShift_Main.Tag; // 작업조id
                Parameters[10] = txtShift_Main.Text; // 작업조name
                Parameters[11] = txtWorker_Main.Tag; // 작업자id
                Parameters[12] = txtWorker_Main.Text; // 작업자name

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puRunStart_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        
        private void btnShip_To_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIP_TO popup = new CMM001.Popup.CMM_SHIP_TO();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puShipTo_Closed);

                //grdMain.Children.Add(popup);
                //popup.BringToFront();
                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void puShipTo_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIP_TO popup = sender as CMM001.Popup.CMM_SHIP_TO;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtShip_To.Text = popup.SHIPTO_NAME;
                btnShip_To.Tag = popup.SHIPTO_ID;
            }
            //this.grdMain.Children.Remove(popup);
        }
        private void puRunStart_Closed(object sender, EventArgs e)
        {
            BOX001_101_RUNSTART popup = sender as BOX001_101_RUNSTART;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Search();
                _util.SetDataGridCheck(dgSearchResult, "CHK", "BOXID", popup.BOXID);
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : DA_PRD_SEL_INPALLET_FM
        /// </summary>
        private void Search()
        {
            try
            {
                Clear();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _PROCID;

                if (!string.IsNullOrWhiteSpace(txtLotID_Search.Text))
                {
                    dr["PKG_LOTID"] = txtLotID_Search.Text;
                }
                else
                {
                    dr["EQSGID"] = (string)cboLine.SelectedValue;
                    dr["EQPTID"] = (string)cboEquipment_Search.SelectedValue;
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPALLET_INFO_FM", "RQSTDT", "RSLTDT", RQSTDT);
                if(!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);

                if (dgSearchResult.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["DEFECT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        private void btnAdd_Inbox_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
            dtInfo.Rows.Add();
            dtInfo.Rows[dtInfo.Rows.Count - 1]["YN"] = "Y";
            Util.GridSetData(dgInbox, dtInfo, FrameOperation);
        }
        /// <summary>
        /// [등급수량] 저장 
        ///  BR_PRD_REG_PACKING_INBOX_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Inbox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift_Main.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int row = _util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK");

                if (row < 0)
                {
                    return;
                }
                DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
                
                List<DataRow> drInfo = dtInfo?.Select("YN = 'Y'").ToList();

                if (drInfo == null || drInfo.Count <= 0)
                {
                    return;
                }                               

                string sBoxID = Util.NVC(DataTableConverter.GetValue(dgSearchResult.Rows[row].DataItem, "BOXID"));
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("TOTAL_QTY");
                inDataTable.Columns.Add("USERID");
                
                DataRow newRow = inDataTable.NewRow();
                newRow["BOXID"] = sBoxID;
                newRow["TOTAL_QTY"] = Util.NVC_Int(dtInfo.Compute("SUM(TOTAL_QTY)", ""));
                newRow["USERID"] = txtWorker_Main.Tag;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("PRDT_GRD_CODE");
                inBoxTable.Columns.Add("PRDT_GRD_LEVEL");
                inBoxTable.Columns.Add("TOTAL_QTY");

                foreach (DataRow dr in drInfo)
                {
                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = dr["BOXID"];
                    newRow["PRDT_GRD_CODE"] = dr["PRDT_GRD_CODE"];
                    newRow["PRDT_GRD_LEVEL"] = dr["PRDT_GRD_LEVEL"];
                    newRow["TOTAL_QTY"] = dr["TOTAL_QTY"];
                    inBoxTable.Rows.Add(newRow);
                }               

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PACKING_INBOX_FM", "INDATA,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }                      

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Search();
                        _util.SetDataGridCheck(dgSearchResult, "CHK", "BOXID", sBoxID);
                        setDetailInfo(_util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK"));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
       
        }

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSearchResult_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgSearchResult.CurrentRow == null || dgSearchResult.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgSearchResult.CurrentColumn.Name == "CHK")
                {
                    string chkValue = Util.NVC(dgSearchResult.GetCell(dgSearchResult.CurrentRow.Index, dgSearchResult.Columns["CHK"].Index).Value);
                   

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SetGridColumnCombo(C1.WPF.DataGrid.C1DataGrid grid, string columnName, string sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                (grid.Columns[columnName] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setDetailInfo(int rIdx)
        {
            try
            {
                Clear();               

                string chkValue = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["CHK"].Index).Value);

                if (chkValue == bool.TrueString)
                {
                    string sProdId = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PRODID"].Index).Value);
                    string sProject = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PROJECT"].Index).Value);
                    string sMdlLot = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["MDLLOT_ID"].Index).Value);
                    string sEqsgId = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PACK_EQSGID"].Index).Value);
                    string sEqptId = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PACK_EQPTID"].Index).Value);
                    string sWrkType = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PACK_WRK_TYPE_CODE"].Index).Value);
                    string sLotId = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PKG_LOTID"].Index).Value);
                    string sPackDttm = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PACKDTTM"].Index).Value);
                    string sShipTo_Id = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["SHIPTO_ID"].Index).Value);
                    string sShiftName = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["SHFT_NAME"].Index).Value);
                    string sActUser = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["ACTUSER"].Index).Value);
                    string sActUserName = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["ACTUSERNAME"].Index).Value);
                    string sShipTo_Name = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["SHIPTO_NAME"].Index).Value);
                    string sInboxType = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["INBOX_TYPE"].Index).Value);
                    string sNote = Util.NVC(dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["PACK_NOTE"].Index).Value);

                    Util.gridClear(dgInbox);

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("INPALLETID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["INPALLETID"] = dgSearchResult.GetCell(rIdx, dgSearchResult.Columns["BOXID"].Index).Text;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INBOX_INFO_FM", "RQSTDT", "RSLTDT", RQSTDT);
                    DataColumn dc = new DataColumn("YN");
                    dc.DefaultValue = "N";
                    dtResult.Columns.Add(dc);

                    Util.GridSetData(dgInbox, dtResult, FrameOperation);

                    if (dgInbox.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["BOXID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                        DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["PRDT_GRD_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                        DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["PRDT_GRD_LEVEL"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                        DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                        cboEquipment.SelectedValue = sEqptId;
                        txtDate.Text = sPackDttm;
                        cboWorkType.SelectedValue = sWrkType;
                        cboInBox.SelectedValue = sInboxType;
                        txtMDLLOT.Text = sMdlLot;
                        txtProjectName.Text = sProject;
                        txtShip_To.Text = sShipTo_Name;
                        btnShip_To.Tag = sShipTo_Id;
                        txtLotID.Text = sLotId;
                        txtShift.Text = sShiftName;
                        txtWorker.Text = sActUserName;
                        txtWorker.Tag = sActUser;

                        new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text = sNote;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void dgSearchResult_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Column.Name != "CHK")
                    return;

                for (int row = 0; row < dgSearchResult.Rows.Count - dgSearchResult.BottomRows.Count; row++)
                {
                    if (e.Cell.Row.Index != row)
                        (dgSearchResult.GetCell(row, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                }

                setDetailInfo(e.Cell.Row.Index);              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgInbox_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                int iRow = e.Cell.Row.Index;
                int iCol = dgInbox.Columns["YN"].Index;
                dgInbox.GetCell(iRow, iCol).Value = "Y";

                DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
                string sChkData = string.Empty;

                if (!string.IsNullOrWhiteSpace(Util.NVC(dtInfo.Rows[iRow]["PRDT_GRD_CODE"])) && !string.IsNullOrWhiteSpace(Util.NVC(dtInfo.Rows[iRow]["PRDT_GRD_LEVEL"])))
                {
                    dtInfo.Rows[iRow]["PRDT_GRD"] = sChkData = (string)dtInfo.Rows[iRow]["PRDT_GRD_CODE"] + "-" + dtInfo.Rows[iRow]["PRDT_GRD_LEVEL"];                  
                }
                if (!string.IsNullOrWhiteSpace(sChkData) && dtInfo.Select("PRDT_GRD = '" + sChkData + "'").Count() > 1)
                {                   
                    dtInfo.Rows[iRow]["PRDT_GRD"] = dtInfo.Rows[iRow][e.Cell.Column.Name]  = null;
                    Util.MessageValidation("SFU3605",new object[] {sChkData});  //동일한 등급-레벨 [%1]이 존재합니다
                }
                Util.GridSetData(dgInbox, dtInfo, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDelete_Inbox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift_Main.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                DataRow dr = dtInfo?.Select("CHK = 'True'").FirstOrDefault();

                if (dr == null)
                {
                    return;
                }

                Button btn = sender as Button;
                int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

                if (string.IsNullOrWhiteSpace(((DataRowView)btn.DataContext).Row["BOXID"].ToString()))
                {
                    DataTable dt = DataTableConverter.Convert(dgInbox.ItemsSource);
                    dt.Rows[iRow].Delete();
                    Util.GridSetData(dgInbox, dt, FrameOperation);
                }
                else
                {

                    DataSet indataSet = new DataSet();

                    DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                    DataRow newRow = inBoxTable.NewRow();
                    inBoxTable.Columns.Add("BOXID");
                    inBoxTable.Columns.Add("USERID");
                    inBoxTable.Columns.Add("LANGID");

                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = DataTableConverter.GetValue(dgInbox.Rows[iRow].DataItem, "BOXID");
                    newRow["USERID"] = txtWorker_Main.Tag;
                    newRow["LANGID"] = LoginInfo.LANGID;

                    inBoxTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_INBOX_FM", "INBOX", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            Search();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    }, indataSet);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void Clear()
        {
            Util.gridClear(dgInbox);

            txtNote.Document.Blocks.Clear();
            cboEquipment.SelectedIndex = 0;
            txtDate.Text = string.Empty;
            cboWorkType.SelectedIndex = 0;
            cboInBox.SelectedIndex = 0;
            txtMDLLOT.Text = string.Empty;
            txtProjectName.Text = string.Empty;
            txtShip_To.Text = string.Empty;
            btnShip_To.Tag = string.Empty;
            txtLotID.Text = string.Empty;
            txtShift.Text = string.Empty;
            txtWorker.Text = string.Empty;
            txtWorker.Tag = string.Empty;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd);
                string sNote = textRange.Text.Substring(0, textRange.Text.LastIndexOf(System.Environment.NewLine));

                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                DataRow dr = dtInfo?.Select("CHK = 'True'").FirstOrDefault();
                
                if (dr == null)
                {
                    return;
                }

                string sBoxID = Util.NVC(dr["BOXID"]);
                DataSet indataSet = new DataSet();
               
                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("EQPTID");
                inBoxTable.Columns.Add("PACK_WRK_TYPE_CODE");
                inBoxTable.Columns.Add("INBOX_TYPE");
                inBoxTable.Columns.Add("PKG_LOTID");
                inBoxTable.Columns.Add("SHIPTO_ID");
                inBoxTable.Columns.Add("PACK_NOTE");
                inBoxTable.Columns.Add("SHFT_ID");
                inBoxTable.Columns.Add("USERID");
                inBoxTable.Columns.Add("USERNAME");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = sBoxID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;  
                newRow["PACK_WRK_TYPE_CODE"] = cboWorkType.SelectedValue;
                newRow["INBOX_TYPE"] = cboInBox.SelectedValue;
                newRow["PKG_LOTID"] = txtLotID.Text;
                newRow["SHIPTO_ID"] = btnShip_To.Tag;
                newRow["PACK_NOTE"] = sNote;
                newRow["SHFT_ID"] = txtShift.Tag;
                newRow["USERID"] = txtWorker.Tag;
                newRow["USERNAME"] = txtWorker.Text;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_INPALLET_FM", "INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Search();
                        _util.SetDataGridCheck(dgSearchResult, "CHK", "BOXID", sBoxID);
                        setDetailInfo(_util.GetDataGridCheckFirstRowIndex(dgSearchResult,"CHK"));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift_Main.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                DataRow dr = dtInfo?.Select("CHK = 'True'").FirstOrDefault();

                if (dr == null)
                {
                    return;
                }

                DataSet indataSet = new DataSet();

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("BOXID");            
                inBoxTable.Columns.Add("USERID");
                inBoxTable.Columns.Add("LAGNID");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = dr["BOXID"];           
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["LAGNID"] = LoginInfo.LANGID;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CANCEL_INPALLET_FM", "INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Search();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// 실적확정 버튼 클릭시 이벤트
        /// Biz : BR_PRD_REG_END_INPALLET_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift_Main.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                DataRow dr = dtInfo?.Select("CHK = 'True'").FirstOrDefault();

                if (dr == null)
                {
                    return;
                }

                string sBoxID = Util.NVC(dr["BOXID"]);
                DataSet indataSet = new DataSet();

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("USERID");
                inBoxTable.Columns.Add("LANGID");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = sBoxID;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPALLET_FM", "INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Search();
                        _util.SetDataGridCheck(dgSearchResult, "CHK", "BOXID", sBoxID);
                        setDetailInfo(_util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK"));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 포장출고 버튼 클릭시 이벤트
        /// Biz : BR_PRD_REG_SHIP_INPALLET_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift_Main.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                DataRow dr = dtInfo?.Select("CHK = 'True'").FirstOrDefault();

                if (dr == null)
                {
                    return;
                }
                string sBoxID = Util.NVC(dr["BOXID"]);

                DataSet indataSet = new DataSet();

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("USERID");
                inBoxTable.Columns.Add("LANGID");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = sBoxID;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SHIP_INPALLET_FM", "INBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Search();
                        _util.SetDataGridCheck(dgSearchResult, "CHK", "BOXID", sBoxID);
                        setDetailInfo(_util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK"));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 포장출고 버튼 클릭시 이벤트
        /// Biz : BR_PRD_REG_CANCEL_SHIP_INPALLET_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancelShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift_Main.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                DataRow dr = dtInfo?.Select("CHK = 'True'").FirstOrDefault();

                if (dr == null)
                {
                    return;
                }
                string sBoxID = Util.NVC(dr["BOXID"]);
                DataSet indataSet = new DataSet();

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                DataRow newRow = inBoxTable.NewRow();
                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("USERID");
                inBoxTable.Columns.Add("LANGID");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = sBoxID;
                newRow["USERID"] = txtWorker_Main.Tag;
                newRow["LANGID"] = LoginInfo.LANGID;

                inBoxTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SHIP_INPALLET_FM", "INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Search();
                        _util.SetDataGridCheck(dgSearchResult, "CHK", "BOXID", sBoxID);
                      //  setDetailInfo(_util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK"));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 포장출고 버튼 클릭시 이벤트
        /// Biz : BR_PRD_GET_INPALLET_LABEL_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtShift_Main.Text))
                {
                    Util.MessageValidation("SFU1845");
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker_Main.Text))
                {
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int row = _util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK");

                if (row < 0)
                {
                    return;
                }

                string sBoxID = Util.NVC(dgSearchResult.GetCell(row, dgSearchResult.Columns["BOXID"].Index).Value);

                DataSet inDataSet = new DataSet();
                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LANGID", typeof(string));
                inBox.Columns.Add("BOXID", typeof(string));

                DataRow dr = inBox.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBoxID;
                inBox.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INPALLET_LABEL_FM", "INBOX", "OUTBOX,OUTLOT", (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    // Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    BOX001_101_REPORT _print = new BOX001_101_REPORT();
                    _print.FrameOperation = this.FrameOperation;

                    if (_print != null)
                    {
                        // SET PARAMETER
                        object[] Parameters = new object[2];
                        Parameters[0] = result;
                        Parameters[1] = txtWorker_Main.Text;

                        C1WindowExtension.SetParameters(_print, Parameters);

                        _print.ShowModal();
                       
                    }

                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgSearchResult_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
