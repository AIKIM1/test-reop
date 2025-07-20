/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 이영준S
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_027 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _util = new Util();

        public BOX001_027()
        {
            InitializeComponent();
        }
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

            //if (cboEqpt != null && cboEqpt.SelectedValue != null && !cboEqpt.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    GetEqptWrkInfo();
            //}
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEqpt };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { Process.CELL_BOXING };
            C1ComboBox[] cboEquipmentParent = { cboLine };  //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboElecType };
           // C1ComboBox[] cboChild = { cboMountPstsID };
            _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT");

            _combo.SetCombo(cboState, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "BOXSTAT" }, sCase: "COMMCODE_WITHOUT_CODE");

            _combo.SetCombo(cboGrade, CommonCombo.ComboStatus.NONE, sFilter: new string[] { "PRDT_GRD_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
            _combo.SetCombo(cboLevel, CommonCombo.ComboStatus.NONE, sFilter: new string[] { "PRDT_GRD_LEVEL" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            txtBoxQty.Value = 100;
        }
        #endregion

        #region Events

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
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

            //Validation 추가 
            BOX001_027_RUNSTART popUp = new BOX001_027_RUNSTART { FrameOperation = FrameOperation };
            if (popUp != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = Process.CELL_BOXING; // 
                Parameters[1] = cboLine.SelectedValue; 
                Parameters[2] = cboEqpt.SelectedValue; // popup창에서 validation 함
                Parameters[3] =  txtShift_Main.Tag; // 작업조id
                Parameters[4] = txtShift_Main.Text; // 작업조name
                Parameters[5] = txtWorker_Main.Tag; // 작업자id
                Parameters[6] = txtWorker_Main.Text; // 작업자name
                C1WindowExtension.SetParameters(popUp, Parameters);
                popUp.ShowModal();
                popUp.CenterOnScreen();
                popUp.Closed += new EventHandler(runStart_Closed);
            } 
        }
        private void btnShift_Main_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = new CMM_SHIFT_USER2();
            shiftPopup.FrameOperation = this.FrameOperation;

            if (shiftPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboLine.SelectedValue);
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift_Main.Tag);
                Parameters[5] = Util.NVC(txtWorker_Main.Tag);
                Parameters[6] = Util.NVC(cboEqpt.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(shiftPopup);
                shiftPopup.BringToFront();
            }
        }
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = sender as CMM_SHIFT_USER2;

            if (shiftPopup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(shiftPopup);
        }
        private void runStart_Closed(object sender, EventArgs e)
        {
            BOX001_027_RUNSTART popUp = sender as BOX001_027_RUNSTART;
            if(popUp.DialogResult == MessageBoxResult.OK)
            {
                GetPalletList();
            }
            this.grdMain.Children.Remove(popUp);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            GetPalletList();
        }

        #endregion


        private void GetEqptWrkInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEqpt.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                Indata["PROCID"] = Process.CELL_BOXING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            //if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            //{
                            //    txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            //}
                            //else
                            //{
                            //    txtShiftStartTime.Text = string.Empty;
                            //}

                            //if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            //{
                            //    txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            //}
                            //else
                            //{
                            //    txtShiftEndTime.Text = string.Empty;
                            //}

                            //if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            //{
                            //    txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            //}
                            //else
                            //{
                            //    txtShiftDateTime.Text = string.Empty;
                            //}

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker_Main.Text = string.Empty;
                                txtWorker_Main.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker_Main.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker_Main.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift_Main.Tag = string.Empty;
                                txtShift_Main.Text = string.Empty;
                            }
                            else
                            {
                                txtShift_Main.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift_Main.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker_Main.Text = string.Empty;
                            txtWorker_Main.Tag = string.Empty;
                            txtShift_Main.Text = string.Empty;
                            txtShift_Main.Tag = string.Empty;
                            //txtShiftStartTime.Text = string.Empty;
                            //txtShiftEndTime.Text = string.Empty;
                            //txtShiftDateTime.Text = string.Empty;
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
                });
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
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnOutAdd);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }


        private bool CanSearch()
        {
            bool bRet = false;

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEqpt.SelectedIndex < 0 || cboEqpt.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void GetPalletList()
        {
            try
            {
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("EQPTID");
                RQSTDT.Columns.Add("FROM_DTTM");
                RQSTDT.Columns.Add("TO_DTTM");
                RQSTDT.Columns.Add("BOXSTAT");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEqpt.SelectedValue);
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                dr["BOXSTAT"] = Util.NVC(cboState.SelectedValue) == "" ? null: Util.NVC(cboState.SelectedValue);

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_PACKING_LIST_FOR_1ST_MP", "RQSTDT", "RSLTDT", RQSTDT, (RSLTDT, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (!RQSTDT.Columns.Contains("CHK"))
                        RQSTDT = _util.gridCheckColumnAdd(RQSTDT, "CHK");

                    Util.GridSetData(dgSearchResult, RSLTDT, FrameOperation, true);

                  

                    if (dgSearchResult.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["DEFECT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    }
                });
            }
            catch(Exception ex)
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
        private void btnBoxLabelPrint_Click(object sender, RoutedEventArgs e)
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

            BOX001_027_INBOX_LABEL popup = new BOX001_027_INBOX_LABEL();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = Process.CELL_BOXING;
                Parameters[1] = Util.NVC(cboEqpt.SelectedValue);
                Parameters[2] = txtWorker_Main.Tag; // 작업자id
             
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puInboxLabel_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        private void puInboxLabel_Closed(object sender, EventArgs e)
        {
            BOX001_027_INBOX_LABEL popup = sender as BOX001_027_INBOX_LABEL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }

        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
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

            // 입력된 BOXID 체크

            DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
            dtInfo.Rows.Add();
            dtInfo.Rows[dtInfo.Rows.Count - 1]["YN"] = "Y";
            Util.GridSetData(dgInbox, dtInfo, FrameOperation);
        }

        private void btnShip_To_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
