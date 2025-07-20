/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 믹서원자재 투입요청서
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.10.13  양영재   [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 재고량, 설정값 저장 기능 추가
  2023.11.06  양영재   [E20231027-001625] - 배치 비율(%) 입력에 따른 설정값 계산 및 투입요청서 상세정보에 재고량, 투입 후 이론중량, 설정값 컬럼 추가
  2023.12.18  양영재   [E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경
   
 [미적용 사항]
     1. 화면 종료 시 확인정보 Table Update 처리 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_002 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        CommonCombo _combo = new CommonCombo();
        string BatchOrdID = string.Empty;
        string WO_ID = string.Empty;
        string ReqID = string.Empty;
        string REQ_DTTM = string.Empty;
        string _EQPTID = string.Empty;
        string _HopperID = "";
        string _MtrlID = "";
        string _ProdID = string.Empty;
        string _ProcChkEqptID = string.Empty;
        DataSet inDataSet = null;
        DataTable IndataTable = null;
        DataTable ReqPrintTag = new DataTable();
        DataTable ReqPrintTagDetl = new DataTable();
        public DataTable dtColumnLang;
        DataTable ChkKeepData = new DataTable();
        bool HopperFlag = false;
        bool InputFlag = false;
        Util _Util = new Util();
        bool DETAIL_POPUP = false;
        bool HopperMtrlFlag = false;
        int Select = 0;
        private System.Windows.Threading.DispatcherTimer _Timer = new System.Windows.Threading.DispatcherTimer();
        private SoundPlayer player;
        bool uses_PNTR_WRKR_TYPE = false;
        int rowIndex = 0; // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
        int? curr_input_percent = null; // [E20231027-001625]건 추가
        int? before_input_percent = null; // [E20231027-001625]건 추가

        public ELEC001_002()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            //WPF 는 컨트롤 초기화를 Loaded 이벤트에서 해야합니다
            //가끔 컨트롤이 정상 초기화 되지 않을 경우가 있습니다

            _Timer.Tick += dispatcherTimer_Tick;
            _Timer.Interval = TimeSpan.FromSeconds(10);

            InitControls();
            InitCombo();
            ApplyPermissions();
            //Set_Combo_BatchCode(cboBatchCode);

        }

        #endregion


        #region Initialize

        public string PRODID { get; set; }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDelete);
            listAuth.Add(btnRequest);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitControls()
        {

            //ldpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
            //ldpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ldpDate();
            dtpDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            // 절연믹싱 투입요청 가능한 동만 표시하도록 설정 [2019-08-13]
            if (IsCommonCode("INS_MIXING_INPUT_REQ_USE_AREA", LoginInfo.CFG_AREA_ID) == false)
                btnJCMixing.Visibility = Visibility.Collapsed;


            // 오창일 경우, 협력사 작업자 유형에 따른 사용 가능 기능 분리 [2020-06-16]
            if (IsCommonCode("SUBCONT_SHOP", LoginInfo.CFG_SHOP_ID))
                uses_PNTR_WRKR_TYPE = true;

            if (uses_PNTR_WRKR_TYPE)
            {
                dgRequestList.Columns["APPR_REQ_RSLT_NAME"].Visibility = Visibility.Visible;

                if (LoginInfo.PNTR_WRKR_TYPE.Equals("DLGT"))    // 현장대리인
                {
                    btnAccept.Visibility = Visibility.Visible;
                    btnReject.Visibility = Visibility.Visible;
                    btnDelete.Visibility = Visibility.Collapsed;
                    btnJCMixing.Visibility = Visibility.Collapsed;

                    chkRequest.Visibility = Visibility.Collapsed;

                    txbWorker.Visibility = Visibility.Hidden;
                    txtWorker.Visibility = Visibility.Hidden;
                    btnReqUser.Visibility = Visibility.Hidden;
                    txbRemark.Visibility = Visibility.Hidden;
                    txtRemark.Visibility = Visibility.Hidden;
                    btnRequest.Visibility = Visibility.Hidden;

                    _Timer.Tick -= dispatcherTimer_Tick;
                    _Timer.Tick += dispatcherTimer_Tick_Delegate;
                    _Timer.Interval = TimeSpan.FromSeconds(20);
                    _Timer.Start();

                    //_Timer.Stop();
                    //_Timer.Tick -= dispatcherTimer_Tick_Delegate;
                }
                else if (LoginInfo.PNTR_WRKR_TYPE.Equals("WRKR"))   // 협력사직원
                {
                    btnAccept.Visibility = Visibility.Collapsed;
                    btnReject.Visibility = Visibility.Collapsed;
                    btnJCMixing.Visibility = Visibility.Collapsed;

                    chkRequest.Visibility = Visibility.Visible;

                    txbWorker.Visibility = Visibility.Hidden;
                    txtWorker.Visibility = Visibility.Hidden;
                    btnReqUser.Visibility = Visibility.Hidden;
                    txbRemark.Visibility = Visibility.Hidden;
                    txtRemark.Visibility = Visibility.Hidden;
                    btnRequest.Visibility = Visibility.Hidden;

                    btnDelete.Visibility = Visibility.Collapsed;
                }
                else                                                // 화학직원 (원청)
                {
                    btnAccept.Visibility = Visibility.Collapsed;
                    btnReject.Visibility = Visibility.Collapsed;

                    chkRequest.Visibility = Visibility.Collapsed;

                    txbWorker.Visibility = Visibility.Visible;
                    txtWorker.Visibility = Visibility.Visible;
                    btnReqUser.Visibility = Visibility.Visible;
                    txbRemark.Visibility = Visibility.Visible;
                    txtRemark.Visibility = Visibility.Visible;
                    btnRequest.Visibility = Visibility.Visible;
                }
            }
            // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
            if (IsAreaCommonCodeUse("INPUT_REQ_STCK_MANAGE_USE_FLAG"))
            {

                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgProductLot.Columns)
                {
                    switch (dgc.Name.ToString())
                    {
                        case "Delete":
                            dgc.Visibility = Visibility.Collapsed;
                            break;
                        case "MTRL_QTY":
                            dgc.Visibility = Visibility.Visible; //E20230907-001465  2023.10.18 수정 Collapsed->Visible
                            break;
                        case "STCK_QTY":
                            dgc.Visibility = Visibility.Visible;
                            break;
                        case "INPUT_AFTER_THRY_WEIGHT":
                            dgc.Visibility = Visibility.Visible;
                            break;
                        case "SET_QTY":
                            dgc.Visibility = Visibility.Visible;
                            break;
                    }
                }
                // [E20231027-001625]건 추가
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgRequestDetailList.Columns)
                {
                    switch (dgc.Name.ToString())
                    {
                        case "MTRL_QTY":
                            dgc.Visibility = Visibility.Visible; //E20230907-001465  2023.10.18 수정 Collapsed->Visible
                            break;
                        case "STCK_QTY":
                            dgc.Visibility = Visibility.Visible;
                            break;
                        case "INPUT_AFTER_THRY_WEIGHT":
                            dgc.Visibility = Visibility.Visible;
                            break;
                        case "SET_QTY":
                            dgc.Visibility = Visibility.Visible;
                            break;
                    }
                }
                batchPercent.Visibility = Visibility.Visible;
                txtBatchPercent.Visibility = Visibility.Visible;
                txtBatchPercent.Text = Convert.ToString(100);
                requestHist.Visibility = Visibility.Visible; //[E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경
            }

        }

        private void SetEquipment(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_LIST_SUBCONT_DLGT_EQPT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow drALL = dtResult.NewRow();
                drALL["CBO_NAME"] = "-ALL-";
                drALL["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drALL, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void ldpDate()
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_AREA_DATE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    ldpDateFrom.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    ldpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    //Util.MessageException(Exception);
                    return;
                }
                ldpDateFrom.Text = result.Rows[0][0].ToString();
                ldpDateTo.Text = result.Rows[0][0].ToString();
            }
            );
        }


        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            //String[] sFilter2 = { cboEquipmentSegment.SelectedValue.ToString(), LoginInfo.CFG_PROC_ID, null };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);
            if (uses_PNTR_WRKR_TYPE && LoginInfo.PNTR_WRKR_TYPE.Equals("DLGT"))
            {
                SetEquipment(EQUIPMENT);
            }
        }

        private void Set_Combo_BatchCode(C1ComboBox cbo)
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["LANGID"] = LoginInfo.LANGID;
            drnewrow["CMCDTYPE"] = "BTCH_LINK_CODE";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_BAS_SEL_COMMONCODE", Exception.Message, Exception.ToString());
                    return;
                }
                DataRow dRow = result.NewRow();
                dRow["CBO_NAME"] = "-SELECT-";
                dRow["CBO_CODE"] = "";
                result.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(result);
                cbo.SelectedIndex = 0;
            }
            );
        }

        #endregion


        #region Event

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipmentSegment.SelectedValue) != string.Empty)
            {
                //20210205 로그인 공정 정보에 상관없이 콤보박스 세팅되도록 변경
                //if (LoginInfo.CFG_PROC_ID.Equals(Process.MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.SRS_MIXING) || LoginInfo.CFG_PROC_ID.Equals(Process.PRE_MIXING))
                //{

                String[] sFilter2 = { cboEquipmentSegment.SelectedValue.ToString(), "E0500,E1000", null };//2017-08-14 권병훈C 요청으로 LoginInfo.CFG_PROC_ID -> "E0500,E1000" 수정
                //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment};
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);
                //cboEquipment.SelectedIndex = 0;
                _combo.SetCombo(EQUIPMENT, CommonCombo.ComboStatus.ALL, sFilter: sFilter2);
                EQUIPMENT.SelectedIndex = 0;

                // HOPPER MTRL USE FLAG 갱신 처리 [2018-03-21]
                IsHopperMtrlChk();

                //}
            }
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipment.SelectedValue) != string.Empty && Util.NVC(cboEquipment.SelectedValue) != "-SELECT-")
            {
                _EQPTID = Util.NVC(cboEquipment.SelectedValue.ToString().Trim());
                //btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
                //Util.gridClear(dgRequestList);
                //Util.gridClear(dgRequestDetailList);
            }
            else if (cboEquipment.Items.Count <= 1 && cboEquipment.SelectedIndex < 1)
            {
                Util.AlertInfo("SFU2017");  //해당 라인으로 설정 후 설비가 출력됩니다.
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
                Util.gridClear(dgRequestList);
                Util.gridClear(dgRequestDetailList);
                return;
            }

        }

        private void btnCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count < 1 || cboEquipment.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }
            ELEC001_002_INPUT_INFO _ReqDetail = new ELEC001_002_INPUT_INFO();
            _ReqDetail.FrameOperation = FrameOperation;
            if (_ReqDetail != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _EQPTID;
                C1WindowExtension.SetParameters(_ReqDetail, Parameters);

                _ReqDetail.Closed += new EventHandler(InputInfo_Closed);
                _ReqDetail.ShowModal();
                _ReqDetail.CenterOnScreen();
            }
        }

        private void btnRingblower_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count < 1 || cboEquipment.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }
            ELEC001_002_INPUT_CHK _InputCheck = new ELEC001_002_INPUT_CHK();
            _InputCheck.FrameOperation = FrameOperation;
            if (_InputCheck != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _EQPTID;
                C1WindowExtension.SetParameters(_InputCheck, Parameters);

                _InputCheck.Closed += new EventHandler(InputCheck_Closed);
                _InputCheck.ShowModal();
                _InputCheck.CenterOnScreen();
            }
        }

        private void dgWorkOrder_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e) //C1.WPF.DataGrid.DataGridMergingCellsEventArgs e, DataGridCellEventArgs a
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (dgWorkOrder.Rows.Count <= 0)
                {
                    return;
                }
                int x = 0;
                int x1 = 0;
                for (int i = x1; i < dgWorkOrder.GetRowCount(); i++)
                {
                    if (Util.NVC(dgWorkOrder.GetCell(x, dgWorkOrder.Columns["BTCH_ORD_ID"].Index).Value) == Util.NVC(dgWorkOrder.GetCell(i, dgWorkOrder.Columns["BTCH_ORD_ID"].Index).Value))
                    {
                        x1 = i;
                    }
                    else
                    {
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)0), dgWorkOrder.GetCell((int)x1, (int)0)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)2), dgWorkOrder.GetCell((int)x1, (int)2)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)3), dgWorkOrder.GetCell((int)x1, (int)3)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)4), dgWorkOrder.GetCell((int)x1, (int)4)));
                        e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)5), dgWorkOrder.GetCell((int)x1, (int)5)));
                        x = x1 + 1;
                        i = x1;
                    }
                }
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)0), dgWorkOrder.GetCell((int)x1, (int)0)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)2), dgWorkOrder.GetCell((int)x1, (int)2)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)3), dgWorkOrder.GetCell((int)x1, (int)3)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)4), dgWorkOrder.GetCell((int)x1, (int)4)));
                e.Merge(new DataGridCellsRange(dgWorkOrder.GetCell((int)x, (int)5), dgWorkOrder.GetCell((int)x1, (int)5)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count == 0)
            {
                Util.MessageValidation("SFU2016");   //해당 라인에 설비가 존재하지 않습니다.
                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
                Util.gridClear(dgRequestList);
                Util.gridClear(dgRequestDetailList);
                return;
            }
            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            if (cboEquipment.Items.Count < 1 || cboEquipment.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }

            GetWorkOrder();//좌측상단 작업지시 리스트 조회
            GetEqptWrkInfo();//우측상단 작업자 조회
            btnSearchList.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearchList));//좌측하단 투입요청서 리스트 조회
            GetMtrlHistInfo(); //[E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경
        }

        private void btnSearchList_Click(object sender, RoutedEventArgs e)
        {
            //if (cboEquipment.Items.Count == 0)
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
            //    return;
            //}
            //if (cboEquipment.Text.Equals("-SELECT-"))
            //{
            //    Util.MessageValidation("SFU1673");  //설비를 선택하세요
            //    return;
            //}

            GetRequestList();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DelRequestList();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!chkRequest.IsChecked.Value)
            {
                DETAIL_POPUP = true;
            }
            if (DETAIL_POPUP)
            {
                if (!LoginInfo.PNTR_WRKR_TYPE.Equals("DLGT"))
                {
                    this.PlaySound();
                }
                return;
            }
            if (!string.IsNullOrEmpty(GetRequestCount()))
            {
                ELEC001_002_DETAIL _ReqDetail = new ELEC001_002_DETAIL();
                _ReqDetail.FrameOperation = FrameOperation;

                if (_ReqDetail != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = GetRequestCount();
                    //Parameters[1] = _EQPTID;
                    C1WindowExtension.SetParameters(_ReqDetail, Parameters);

                    _ReqDetail.Closed += new EventHandler(Detail_Closed);
                    _ReqDetail.ShowModal();
                    _ReqDetail.CenterOnScreen();

                    DETAIL_POPUP = true;
                    this.PlaySound();
                }
            }
            /*
            if (uses_PNTR_WRKR_TYPE)
            {
                GetRequestList();
            }
            */
        }

        private void dispatcherTimer_Tick_Delegate(object sender, EventArgs e)
        {
            GetRequestList();

            if (!string.IsNullOrEmpty(GetRequestCount_Dlgt()))
            {
                this.PlaySound();
            }
        }

        private void PlaySound()
        {
            player = new System.Media.SoundPlayer(LGC.GMES.MES.ELEC001.Properties.Resources.InputBois);
            this.player.Play();
        }

        private void chkRequest_Checked(object sender, RoutedEventArgs e)
        {
            _Timer.Tick += dispatcherTimer_Tick;
            _Timer.Interval = TimeSpan.FromSeconds(10);
            _Timer.Start();
        }

        private void chkRequest_Unchecked(object sender, RoutedEventArgs e)
        {
            _Timer.Stop();
            DETAIL_POPUP = false;
            _Timer.Tick -= dispatcherTimer_Tick;
        }

        private void dgProductLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                bool bchk = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));
                rowIndex = e.Row.Index; // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가

                if (bchk == true)
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "MTRL_QTY")
                        {
                            e.Cancel = false;
                        }
                    }
                    /*
                    if (e.Column is C1.WPF.DataGrid.DataGridComboBoxColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "HOPPER_ID")
                        {
                            e.Cancel = false;

                        }
                    }
                    */
                }
                else
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "MTRL_QTY")
                        {
                            e.Cancel = true;    // Editing 불가능
                        }
                        // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
                        if (Convert.ToString(e.Column.Name) == "STCK_QTY")
                        {
                            e.Cancel = true;
                        }
                        if (Convert.ToString(e.Column.Name) == "SET_QTY")
                        {
                            e.Cancel = true;
                        }
                        if (Convert.ToString(e.Column.Name) == "INPUT_AFTER_THRY_WEIGHT")
                        {
                            e.Cancel = true;
                        }
                    }
                    /*
                    if (e.Column is C1.WPF.DataGrid.DataGridComboBoxColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "HOPPER_ID")
                        {
                            e.Cancel = true;    // Editing 불가능
                        }
                    }
                    */
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProductLot.ItemsSource == null || dgProductLot.Rows.Count == 0)
                    return;

                if (dgProductLot.CurrentRow.DataItem == null)
                    return;
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                DataTable dt = ((DataView)dgProductLot.ItemsSource).Table;

                if (DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "CHK").ToString().Equals("1"))
                {
                    dgProductLot.SelectedIndex = rowIndex;

                    // HOPPER 체크 시 ITEM 갱신되도록 추가 [2018-03-23]
                    C1.WPF.DataGrid.DataGridCell gridCell = dgProductLot.GetCell(rowIndex, dgProductLot.Columns["HOPPER_ID"].Index) as C1.WPF.DataGrid.DataGridCell;

                    if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                    {
                        C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;

                        if (combo != null)
                            SetGridCboItem(combo, _EQPTID, Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "MTRLID")));
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void chkMtrl_Checked(object sender, RoutedEventArgs e)
        {
            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count == 0)
                return;

            if (dgWorkOrder.CurrentRow.DataItem == null)
                return;
            if (BatchOrdID == null || BatchOrdID == "")
                return;
            //btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
            GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[Select].DataItem, "WOID")), BatchOrdID);
        }

        private void chkMtrl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count == 0)
                return;

            if (dgWorkOrder.CurrentRow.DataItem == null)
                return;
            if (BatchOrdID == null || BatchOrdID == "")
                return;
            //btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
            GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[Select].DataItem, "WOID")), BatchOrdID);
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    Select = idx;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                    //row 색 바꾸기
                    dgWorkOrder.SelectedIndex = idx;
                    BatchOrdID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "BTCH_ORD_ID"));
                    _ProdID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                    _ProcChkEqptID = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "EQPTID"));

                    GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID")), BatchOrdID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProductLot.Rows.Count < 1 || dgProductLot.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU1833");  //자재정보가 없습니다.
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtWorker.Text) || string.IsNullOrWhiteSpace((string)txtWorker.Tag))
                {
                    // 요청자를 선택해 주세요.
                    Util.MessageValidation("SFU3467");
                    return;
                }

                dgProductLot.EndEdit();
                inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("BTCH_ORD_ID", typeof(string));
                inDataTable.Columns.Add("CMC_BINDER_FLAG", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["WOID"] = WO_ID;
                inDataRow["NOTE"] = Util.NVC(txtRemark.Text.ToString());
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["REQ_USERID"] = (string)txtWorker.Tag;
                inDataRow["BTCH_ORD_ID"] = BatchOrdID;
                inDataRow["CMC_BINDER_FLAG"] = this.chkMtrl.IsChecked == true ? "Y" : "N";
                inDataTable.Rows.Add(inDataRow);

                DataTable inMtrlid = _Mtrlid();
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return;
                }
                if (!InputFlag)
                {
                    return;
                }
                if (!HopperFlag)
                {
                    Util.MessageValidation("SFU2035");  //호퍼를 선택하세요.
                    return;
                }                
                //투입요청 하시겠습니까?
                Util.MessageConfirm("SFU1974", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RMTRL_INPUT_REQ_PROC", "INDATA,INDTLDATA", null, (result, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.AlertByBiz("BR_PRD_REG_RMTRL_INPUT_REQ_PROC", ex.Message, ex.ToString());
                                    return;
                                }
                                // [E20231027-001625]건 추가
                                if (IsAreaCommonCodeUse("INPUT_REQ_STCK_MANAGE_USE_FLAG"))
                                {
                                    mngtHist_Save();
                                }
                                Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                                dgProductLot.EndEdit(true);
                                dgWorkOrder.EndEdit(true);
                                btnSearchList.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearchList));
                                //Util.gridClear(dgProductLot);
                                ClearGrid();
                                this.txtRemark.Clear();
                                this.txtWorker.Clear();
                                this.txtWorker.Tag = string.Empty;
                                //dgProductLot.ItemsSource = DataTableConverter.Convert(ChkKeepData);
                                GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[Select].DataItem, "WOID")), BatchOrdID);
                            }
                            catch (Exception ErrEx)
                            {
                                Util.MessageException(ErrEx);
                            }
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void ClearGrid()
        {
            try
            {
                for (int i = 0; i < dgProductLot.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "MTRL_QTY", string.Empty);
                    // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "STCK_QTY", 0.0);   //E20230907-001465 2023.10.18 수정
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "INPUT_AFTER_THRY_WEIGHT", 0.0);    //E20230907-001465 2023.10.18 수정
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "SET_QTY", 0.0);    //E20230907-001465 2023.10.18 수정
                    DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "HOPPER_ID", "");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void ldpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }

            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }

        private void ldpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }
            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgRequestList, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }
                dtColumnLang = new DataTable();

                dtColumnLang.Columns.Add("믹서투입요청서", typeof(string));
                dtColumnLang.Columns.Add("담당자", typeof(string));
                dtColumnLang.Columns.Add("요청서번호", typeof(string));
                dtColumnLang.Columns.Add("요청이력", typeof(string));
                dtColumnLang.Columns.Add("자재상세정보", typeof(string));
                dtColumnLang.Columns.Add("요청일자", typeof(string));
                dtColumnLang.Columns.Add("프로젝트명", typeof(string));
                dtColumnLang.Columns.Add("요청작업자", typeof(string));
                dtColumnLang.Columns.Add("요청장비", typeof(string));
                dtColumnLang.Columns.Add("특이사항", typeof(string));
                dtColumnLang.Columns.Add("자재", typeof(string));
                dtColumnLang.Columns.Add("자재규격", typeof(string));
                dtColumnLang.Columns.Add("요청중량", typeof(string));
                dtColumnLang.Columns.Add("호퍼", typeof(string));

                DataRow drCrad = null;

                drCrad = dtColumnLang.NewRow();

                drCrad.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("믹서투입요청서"),
                                                  ObjectDic.Instance.GetObjectName("담당자"),
                                                  ObjectDic.Instance.GetObjectName("요청서번호"),
                                                  ObjectDic.Instance.GetObjectName("요청이력"),
                                                  ObjectDic.Instance.GetObjectName("자재상세정보"),
                                                  ObjectDic.Instance.GetObjectName("요청일자"),
                                                  ObjectDic.Instance.GetObjectName("프로젝트명"),
                                                  ObjectDic.Instance.GetObjectName("요청작업자"),
                                                  ObjectDic.Instance.GetObjectName("요청장비"),
                                                  ObjectDic.Instance.GetObjectName("특이사항"),
                                                  ObjectDic.Instance.GetObjectName("자재"),
                                                  ObjectDic.Instance.GetObjectName("자재규격"),
                                                  ObjectDic.Instance.GetObjectName("요청중량"),
                                                  ObjectDic.Instance.GetObjectName("호퍼")
                                               };

                dtColumnLang.Rows.Add(drCrad);

                ReqPrintTagDetl = new DataTable();
                ReqPrintTagDetl = CreateReqDetlPrint(ReqPrintTagDetl);

                LGC.GMES.MES.ELEC001.Report_Multi rs = new LGC.GMES.MES.ELEC001.Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[4];
                    Parameters[0] = "MReqList_Print";
                    Parameters[1] = ReqPrintTag;
                    Parameters[2] = ReqPrintTagDetl;
                    Parameters[3] = dtColumnLang;

                    C1WindowExtension.SetParameters(rs, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgProductLot_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가 ,  IN_PUT_QTY -> MTRL_QTY   2023.10.18 수정
            if ((Convert.ToString(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "MTRL_QTY")) != ""))
            {
                decimal a = System.Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "STCK_QTY")), 3) + System.Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgProductLot.Rows[rowIndex].DataItem, "MTRL_QTY")), 3);
                DataTableConverter.SetValue(dgProductLot.Rows[rowIndex].DataItem, "INPUT_AFTER_THRY_WEIGHT", a);
            }
            /*
            if (dgProductLot.CurrentRow == null || dgProductLot.SelectedIndex == -1)
            {
                return;
            }
            try
            {
                if (e.Cell.Column.Name.Equals("HOPPER_ID"))
                {
                    C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                    if (dg.GetRowCount() == 0) return;

                    DataTable dtTmp = DataTableConverter.Convert(dg.ItemsSource);
                    _HopperID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "HOPPER_ID"));
                    _MtrlID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "MTRLID"));

                    //같은 자재, 같은 호퍼 투입 불가능
                    for (int icnt = 0; icnt < dgProductLot.Rows.Count; icnt++)
                    {
                        if (icnt != dgProductLot.SelectedIndex) //현재 선택한 row 제외
                        {
                            if (dtTmp.Rows[icnt]["HOPPER_ID"].ToString() == _HopperID)//dtTmp.Rows[icnt]["MTRLID"].ToString() == _MtrlID && //자재상관없이 같은 호퍼는 선택 불가능
                            {
                                //같은 호퍼ID를 선택하셨습니다.
                                Util.MessageInfo("SFU2852", (result) =>
                                {
                                    if (result == MessageBoxResult.OK || result == MessageBoxResult.Cancel)
                                    {
                                        DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "HOPPER_ID", "");
                                    }
                                });
                                return;
                            }
                        }
                    }

                    if (FinalInputMtrlCheck(_HopperID, _MtrlID))
                    {
                        //이미 투입요청된 자재입니다. \r\n다시 요청하시겠습니까? -> 해당 HOPPER에 잔량이 존재합니다. 초기화하고 다시 투입하시겠습니까?
                        Util.MessageConfirm("SFU1778", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "Y");

                                // DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK", false);
                                // CANCEL시 초기화 하는 로직 제거
                                //DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "HOPPER_ID", "");
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "N");
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            */
        }

        private void dgReqListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgRequestList.SelectedIndex = idx;
                GetRequestDetail(Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "REQ_ID")));
                ReqID = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "REQ_ID"));

                //요청서 프린트 
                ReqPrintTag = new DataTable();
                ReqPrintTag = CreateReqPrint(ReqPrintTag);
                ReqPrintTag.Rows[0]["REQ_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "REQ_DTTM"));
                ReqPrintTag.Rows[0]["REQ_ID"] = ReqID;
                ReqPrintTag.Rows[0]["REQ_ID1"] = ReqID;
                ReqPrintTag.Rows[0]["USERNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "USERNAME"));
                ReqPrintTag.Rows[0]["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "NOTE"));
                txtDetailRemark.Text = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "NOTE"));
                ReqPrintTag.Rows[0]["EPQTID"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "EQPTNAME"));
                ReqPrintTag.Rows[0]["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[idx].DataItem, "PRJT_NAME"));
            }
        }

        private void Detail_Closed(object sender, EventArgs e)
        {
            ELEC001_002_DETAIL window = sender as ELEC001_002_DETAIL;
            if (window.DialogResult == (MessageBoxResult.OK) || window.DialogResult == (MessageBoxResult.Cancel))
            {
                _Timer.Start();
                DETAIL_POPUP = false;
                this.player.Stop();
            }
        }

        private void InputInfo_Closed(object sender, EventArgs e)
        {
            ELEC001_002_INPUT_INFO window = sender as ELEC001_002_INPUT_INFO;
            if (window.DialogResult == (MessageBoxResult.OK) || window.DialogResult == (MessageBoxResult.Cancel))
            {
            }
        }

        private void InputCheck_Closed(object sender, EventArgs e)
        {
            ELEC001_002_INPUT_CHK window = sender as ELEC001_002_INPUT_CHK;
            if (window.DialogResult == (MessageBoxResult.OK) || window.DialogResult == (MessageBoxResult.Cancel))
            {
            }
        }

        private bool FinalInputMtrlCheck(string HopperID, string MtrlID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("HOPPER_ID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                //IndataTable.Columns.Add("BTCH_ORD_ID", typeof(string));
                //IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _EQPTID;
                Indata["HOPPER_ID"] = HopperID;
                Indata["PRODID"] = _ProdID;
                //Indata["BTCH_ORD_ID"] = BatchOrdID;
                //Indata["MTRLID"] = MtrlID;
                IndataTable.Rows.Add(Indata);

                //DA_BAS_SEL_TB_SFC_MIXER_FINL_INPUT_MTRL -> BR_PRD_CHK_FINL_INPUT_MTRL
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_FINL_INPUT_MTRL", "INDATA", "OUTDATA", IndataTable);

                if (dtMain.Rows[0]["CHK_FLAG"].ToString() == "N")
                {
                    DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "N");
                    return false;
                }
                else
                {
                    //DataTableConverter.SetValue(dgProductLot.Rows[dgProductLot.SelectedIndex].DataItem, "CHK_FLAG", "Y");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // (C20200724-000246) 원재료 오투입 방지를 위한 알람팝업 적용
        private bool IsFinalMaterialCheck(string HopperID, string MtrlID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("HOPPER_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["HOPPER_ID"] = HopperID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_MIXER_FINL_INPUT_MTRL", "INDATA", "RSLTDT", IndataTable);

                //투입이력 없음
                if (result.Rows.Count == 0)
                    return true;
                //동일투입자재
                foreach (DataRow row in result.Rows)
                    if (string.Equals(MtrlID, row["MTRLID"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnRemain_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_ELEC_TERM_CANCEL_PREMIX wndPopup = new LGC.GMES.MES.CMM001.CMM_ELEC_TERM_CANCEL_PREMIX();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                C1WindowExtension.SetParameters(wndPopup, null);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                if (string.Equals(e.Cell.Column.Name, "HOPPER_ID"))
                                {
                                    C1ComboBox combo = e.Cell.Presenter.Content as C1ComboBox;
                                    if (combo != null)
                                    {
                                        combo.IsDropDownOpenChanged -= dgProductLot_DropDownOpenChanged;
                                        combo.IsDropDownOpenChanged += dgProductLot_DropDownOpenChanged;

                                        combo.SelectionCommitted -= dgProductLot_SelectedCommited;
                                        combo.SelectionCommitted += dgProductLot_SelectedCommited;
                                    }
                                }
                            }
                        }
                    }
                }));
            }
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
        }

        private void dgProductLot_DropDownOpenChanged(object sender, PropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue == true)
            {
                C1ComboBox combo = sender as C1ComboBox;
                if (combo != null)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(dgProductLot.Rows[((DataGridCellPresenter)combo.Parent).Cell.Row.Index].DataItem, "CHK")) == false)
                    {
                        combo.IsDropDownOpen = false;
                        Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                        return;
                    }
                }
            }
        }

        private void dgProductLot_SelectedCommited(object sender, PropertyChangedEventArgs<object> e)
        {
            int iSelectIdx = -1;
            C1ComboBox comboBox = sender as C1ComboBox;
            if (comboBox != null)
            {
                DataGridCellPresenter cellPresenter = comboBox.Parent as DataGridCellPresenter;
                if (cellPresenter != null && cellPresenter.Cell != null && cellPresenter.Cell.Row != null)
                    iSelectIdx = cellPresenter.Cell.Row.Index;
            }

            if (iSelectIdx < 0)
                return;


            if (dgProductLot.CurrentRow == null || iSelectIdx < 0 || string.IsNullOrEmpty(Util.NVC(e.NewValue)))
                return;

            try
            {
                if (dgProductLot.GetRowCount() == 0)
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgProductLot.ItemsSource);
                _HopperID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID"));
                _MtrlID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iSelectIdx].DataItem, "MTRLID"));

                //같은 자재, 같은 호퍼 투입 불가능
                for (int icnt = 0; icnt < dgProductLot.Rows.Count; icnt++)
                {
                    if (icnt != iSelectIdx) //현재 선택한 row 제외
                    {
                        if (dtTmp.Rows[icnt]["HOPPER_ID"].ToString() == _HopperID)//dtTmp.Rows[icnt]["MTRLID"].ToString() == _MtrlID && //자재상관없이 같은 호퍼는 선택 불가능
                        {
                            //같은 호퍼ID를 선택하셨습니다.
                            Util.MessageInfo("SFU2852", (result) =>
                            {
                                if (result == MessageBoxResult.OK || result == MessageBoxResult.Cancel)
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID", "");
                                }
                            });
                            return;
                        }
                    }
                }
                // (C20200724-000246) 원재료 오투입 방지를 위한 알람팝업 적용 (이전호퍼 투입자재 변경의 경우)
                if (!IsFinalMaterialCheck(_HopperID, _MtrlID))
                {
                    // 투입자재를 변경하시겠습니까? 호퍼와 자재를 확인해주십시오
                    Util.MessageConfirm("SFU8228", (result) =>
                    {
                        if (result != MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "MTRL_QTY", string.Empty);
                            // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "STCK_QTY", 0.0);      //E20230907-001465 2023.10.18 수정
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "INPUT_AFTER_THRY_WEIGHT", 0.0);       //E20230907-001465 2023.10.18 수정
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "SET_QTY", 0.0);       //E20230907-001465 2023.10.18 수정
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "HOPPER_ID", "");
                        }
                    });
                    return;
                }

                if (FinalInputMtrlCheck(_HopperID, _MtrlID))
                {
                    //이미 투입요청된 자재입니다. \r\n다시 요청하시겠습니까? -> 해당 HOPPER에 잔량이 존재합니다. 초기화하고 다시 투입하시겠습니까?
                    Util.MessageConfirm("SFU1778", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK_FLAG", "Y");
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgProductLot.Rows[iSelectIdx].DataItem, "CHK_FLAG", "N");
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = ((DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                DataView tempView = dgProductLot.ItemsSource as DataView;
                DataTable tempTable = tempView.ToTable().Copy();

                if (idx != 0)
                {
                    tempTable.Rows[idx - 1]["RowSequenceNo"] = idx;
                    tempTable.Rows[idx]["RowSequenceNo"] = idx - 1;

                    DataTable newTable = tempTable.Select("", "RowSequenceNo").CopyToDataTable();
                    Util.GridSetData(dgProductLot, newTable, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = ((DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                DataView tempView = dgProductLot.ItemsSource as DataView;
                DataTable tempTable = tempView.ToTable().Copy();

                if (idx != tempTable.Rows.Count - 1)
                {
                    tempTable.Rows[idx + 1]["RowSequenceNo"] = idx;
                    tempTable.Rows[idx]["RowSequenceNo"] = idx + 1;

                    DataTable newTable = tempTable.Select("", "RowSequenceNo").CopyToDataTable();
                    Util.GridSetData(dgProductLot, tempTable.Select("", "RowSequenceNo").CopyToDataTable(), FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // [E20231027-001625]건 추가
        private void txtBatchPercent_KeyDown(object sender, KeyEventArgs e)
        {
            int d;
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(Util.NVC(txtBatchPercent.Text)))
                {
                    // 배치 비율이 입력되지 않았습니다.
                    Util.MessageValidation("SFU9903");
                    return;
                }
                else
                {
                    if(int.TryParse(txtBatchPercent.Text, out d))
                    {
                        curr_input_percent = int.Parse(Util.NVC(txtBatchPercent.Text));

                        if (curr_input_percent < 50 || curr_input_percent > 110)
                        {
                            // 배치 비율은 50% ~ 110%까지 입력 가능합니다.
                            Util.MessageValidation("SFU9904");
                            txtBatchPercent.Clear();
                            return;
                        }
                        else
                        {
                            if (before_input_percent != curr_input_percent) // 배치 비율 변경했을 경우
                            {
                                if (string.IsNullOrEmpty(BatchOrdID)) 
                                {
                                    Util.MessageValidation("SFU9909");
                                    return;
                                }

                                if (curr_input_percent.Equals(100))
                                {
                                    GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[Select].DataItem, "WOID")), BatchOrdID);
                                    before_input_percent = curr_input_percent;
                                }
                                else
                                {
                                    GetWOMaterial(Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[Select].DataItem, "WOID")), BatchOrdID);
                                    refreshSET_QTY(); // 배치 비율에 따른 설정값 불러오기
                                    before_input_percent = curr_input_percent;
                                }
                            }
                        }
                    }
                    else
                    {
                        Util.MessageValidation("SFU9906");
                        txtBatchPercent.Clear();
                        return;
                    }                    
                }
            }
        }

        #endregion


        #region Mehod

        private void GetWorkOrder()
        {
            try
            {
                bool isProc = false;

                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                {
                    isProc = true;
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                }
                else
                    dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

                Util.gridClear(dgWorkOrder);
                Util.gridClear(dgProductLot);
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MES_CLOSE_FLAG", typeof(string));
                IndataTable.Columns.Add("STDT", typeof(string));
                IndataTable.Columns.Add("EDDT", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());

                if (isProc == false)
                    Indata["EQPTID"] = _EQPTID;

                Indata["MES_CLOSE_FLAG"] = "N";
                Indata["STDT"] = Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd");
                Indata["EDDT"] = Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd");
                Indata["PROCID"] = LoginInfo.CFG_PROC_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(isProc ? "DA_PRD_SEL_MIXMTRL_PROC_WORKORDER" : "DA_PRD_SEL_MIXMTRL_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                dgWorkOrder.BeginEdit();

                Util.GridSetData(dgWorkOrder, dtMain, FrameOperation, true);
                dgWorkOrder.EndEdit();
                dgWorkOrder.MergingCells -= dgWorkOrder_MergingCells;
                dgWorkOrder.MergingCells += dgWorkOrder_MergingCells;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.NOTCHING;

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
                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ReChecked(int iRow)
        {
            if (iRow < 0)
                return;

            if (dgWorkOrder.ItemsSource == null || dgWorkOrder.Rows.Count - dgWorkOrder.BottomRows.Count < iRow)
                return;

            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[iRow].DataItem, "CHK")).Equals("1"))
            {
                dgWorkOrder.SelectedIndex = iRow;
            }
        }

        private void GetWOMaterial(string WOID, string BTCH_ORD_ID)
        {
            try
            {
                WO_ID = WOID;
                Util.gridClear(dgProductLot);
                this.txtRemark.Clear();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("BATCHORDID", typeof(string));
                IndataTable.Columns.Add("FLAG", typeof(string));
                IndataTable.Columns.Add("ALL_FLAG", typeof(string));
                IndataTable.Columns.Add("SHOP_ID", typeof(string));
                IndataTable.Columns.Add("AREA_ID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.AcceptChanges();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["BATCHORDID"] = BatchOrdID;
                Indata["FLAG"] = this.chkMtrl.IsChecked == true ? "Y" : null;
                Indata["ALL_FLAG"] = this.chkMtrl.IsChecked == false ? "Y" : null;
                Indata["SHOP_ID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREA_ID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQPTID"] = chkProc.IsChecked == true ? _ProcChkEqptID : _EQPTID;

                IndataTable.Rows.Add(Indata);

                string sBizName = string.Empty;
                if (LoginInfo.CFG_SHOP_ID == "G183" || _ProdID.Substring(3, 2).Equals("CA"))
                {
                    sBizName = "DA_PRD_SEL_BATCHORDID_MATERIAL_LIST";
                }
                else
                {
                    sBizName = "DA_PRD_SEL_BATCHORDID_MATERIAL_ANODE";
                }

                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WOMATERIAL", "INDATA", "RSLTDT", IndataTable);
                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BATCHORDID_MATERIAL_LIST", "INDATA", "RSLTDT", IndataTable); //DA_PRD_SEL_BATCHORDID_MATERIAL -> DA_PRD_SEL_BATCHORDID_MATERIAL_LIST

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", IndataTable, RowSequenceNo: true);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }

                // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("BTCH_ORD_ID", typeof(string));  // [E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경

                DataRow drnewrow = RQSTDT.NewRow();
                drnewrow["EQPTID"] = _EQPTID;
                drnewrow["BTCH_ORD_ID"] = BatchOrdID; // [E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경

                RQSTDT.Rows.Add(drnewrow);

                // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
                DataTable dtSub = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_INPUT_MIX_STCK_MNGT", "RQSTDT", "RSLTDT", RQSTDT, RowSequenceNo: true);

                dtMain.Columns.Add("STCK_QTY", typeof(decimal));
                dtMain.Columns.Add("SET_QTY", typeof(decimal));
                dtMain.Columns.Add("INPUT_AFTER_THRY_WEIGHT", typeof(decimal));

                foreach (DataRow drow in dtMain.Rows)
                {
                    drow["STCK_QTY"] = 0.0;
                    drow["SET_QTY"] = 0.0;                    
                }

                for (int i = 0; i < dtMain.Rows.Count; i++)
                {
                    string mtrlid = Util.NVC(dtMain.Rows[i]["MTRLID"]);

                    for (int j = 0; j < dtSub.Rows.Count; j++)
                    {
                        if (mtrlid.Equals(Util.NVC(dtSub.Rows[j]["MTRLID"])))
                        {
                            // [E20231027-001625]건 추가
                            if (curr_input_percent.HasValue && curr_input_percent == before_input_percent) // [E20231027-001625]건 추가
                            {
                                dtMain.Rows[i]["SET_QTY"] = Convert.ToDecimal(Util.NVC(dtSub.Rows[j]["SET_QTY"])) * curr_input_percent / 100;
                            }
                            else 
                            {
                                dtMain.Rows[i]["SET_QTY"] = Convert.ToDecimal(Util.NVC(dtSub.Rows[j]["SET_QTY"]));
                            }
                            // [E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경
                            dtMain.Rows[i]["STCK_QTY"] = Convert.ToDecimal(Util.NVC(dtSub.Rows[j]["INPUT_AFTER_THRY_WEIGHT"])) - Convert.ToDecimal(Util.NVC(dtMain.Rows[i]["SET_QTY"]));
                            dtMain.Rows[i]["INPUT_AFTER_THRY_WEIGHT"] = Convert.ToDecimal(Util.NVC(dtMain.Rows[i]["STCK_QTY"])); // [E20231027-001625]건 추가
                            break;
                        }
                    }
                }

                Util.GridSetData(dgProductLot, dtMain, FrameOperation, true);
                //dgProductLot.ItemsSource = DataTableConverter.Convert(dtMain);

                //SetGridCboItem(dgProductLot.Columns["HOPPER_ID"], _EQPTID);

                GridStyleSetting(dgProductLot);

                ChkKeepData = dtMain;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GridStyleSetting(C1DataGrid dataGrid)
        {
            if (dataGrid.Columns.Contains("MTGRNAME"))
            {
                var iMtgName = dataGrid.Columns["MTGRNAME"].Index;

                for (int i = 0; i < dataGrid.Rows.Count; i++)
                {
                    if (Util.NVC(dataGrid.GetCell(i, iMtgName).Value).Equals("Others"))
                    {
                        if (dataGrid.Rows[i].Presenter != null)
                        {
                            dataGrid.Rows[i].Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                }
            }
        }

        private DataTable _Mtrlid()
        {
            IndataTable = inDataSet.Tables.Add("INDTLDATA");
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("MTRL_QTY", typeof(string));
            IndataTable.Columns.Add("HOPPER_ID", typeof(string));
            IndataTable.Columns.Add("CHK_FLAG", typeof(string));

            dgProductLot.EndEdit();
            DataTable dtTop = DataTableConverter.Convert(dgProductLot.ItemsSource);

            foreach (DataRow _iRow in dtTop.Rows)
            {
                if (_iRow["CHK"].Equals(1))
                {
                    if (_iRow["MTRL_QTY"].Equals("") || _iRow["MTRL_QTY"].Equals("0"))
                    {
                        //투입요청수량을 입력 하세요.
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1978"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        Util.MessageInfo("SFU1978", (result) =>
                        {
                            Keyboard.Focus(dgProductLot.CurrentCell.DataGrid);
                        });
                        InputFlag = false;
                        return null;
                    }
                    if (Double.Parse(Util.NVC(_iRow["MTRL_QTY"])) < 0)
                    {
                        Util.MessageValidation("SFU1977");   //투입요청수량은 정수만 입력 하세요.
                        InputFlag = false;
                        return null;
                    }
                    else
                    {
                        InputFlag = true;
                    }
                    if (_iRow["HOPPER_ID"].Equals(""))
                    {
                        HopperFlag = false;
                        return null;
                    }
                    else
                    {
                        HopperFlag = true;
                    }

                    IndataTable.ImportRow(_iRow);
                    HopperFlag = true;
                    InputFlag = true;
                }
            }
            return IndataTable;
        }

        private void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sEQPTID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_HOPPER_CBO", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count == 0) { return; }
                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetGridCboItem(C1ComboBox combo, string sEQPTID, string sMTRLID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEQPTID;
                Indata["MTRLID"] = sMTRLID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(HopperMtrlFlag == true ? "DA_BAS_SEL_HOPPER_TANK_MTRL_SET_CBO" : "DA_BAS_SEL_HOPPER_CBO", "INDATA", "RSLTDT", IndataTable);
                //if (dtMain.Rows.Count == 0) { return; }
                combo.ItemsSource = DataTableConverter.Convert(dtMain);

                if (combo != null && combo.Items.Count == 1)
                {
                    DataTable distinctTable = ((DataView)dgProductLot.ItemsSource).Table.DefaultView.ToTable(true, "HOPPER_ID");
                    foreach (DataRow row in distinctTable.Rows)
                        if (string.Equals(row["HOPPER_ID"], dtMain.Rows[0][0]))
                            return;

                    combo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetRequestList()
        {
            try
            {
                string sBizrule = string.Empty;
                if (uses_PNTR_WRKR_TYPE)
                {
                    if (LoginInfo.PNTR_WRKR_TYPE.Equals("DLGT"))
                    {
                        sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST_SUBCONT_DLGT";
                    }
                    else if (LoginInfo.PNTR_WRKR_TYPE.Equals("WRKR"))
                    {
                        sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST_SUBCONT_WRKR";
                    }
                    else
                    {
                        sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST_SUBCONT";
                    }
                }
                else
                {
                    sBizrule = "DA_PRD_SEL_MIXMTRL_REQUEST_LIST";
                }

                if (cboEquipment.Items.Count < 1)
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요
                    return;
                }
                Util.gridClear(dgRequestList);
                Util.gridClear(dgRequestDetailList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("REQ_DATE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["REQ_DATE"] = Convert.ToDateTime(dtpDate.SelectedDateTime).ToString("yyyyMMdd");     //dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQPTID"] = Util.NVC(EQUIPMENT.SelectedValue.ToString()) == "" ? null : Util.NVC(EQUIPMENT.SelectedValue.ToString());
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizrule, "INDATA", "RSLTDT", IndataTable);
                if (dtMain == null)
                {
                    return;
                }
                // 투입요청서 완료포함 여부 (C20200317-000002)
                if (!(bool)chkCompleted.IsChecked)
                {
                    DataView view = dtMain.AsDataView();
                    view.RowFilter = "CMCODE <> 'C'";
                    DataTable dt = view.ToTable();
                    Util.GridSetData(dgRequestList, dt, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgRequestList, dtMain, FrameOperation, true);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void DelRequestList()
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgRequestList, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1641");  //선택된 요청서가 없습니다.
                    return;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["REQ_ID"] = ReqID;
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                //삭제처리 하시겠습니까?
                Util.MessageConfirm("SFU1259", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_RMTRL_INPUT_REQ_PROC", "INDATA", null, IndataTable);
                            GetRequestList();
                            if (dtMain != null)
                            {
                                return;
                            }
                            ReqID = string.Empty;
                        }
                        catch (Exception ex2)
                        {
                            Util.MessageException(ex2);
                            return;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable CreateReqPrint(DataTable dt)
        {
            try
            {
                dt.Columns.Add("REQ_DTTM", typeof(string));
                dt.Columns.Add("REQ_ID", typeof(string));
                dt.Columns.Add("REQ_ID1", typeof(string));
                dt.Columns.Add("USERNAME", typeof(string));
                dt.Columns.Add("NOTE", typeof(string));
                dt.Columns.Add("EPQTID", typeof(string));
                dt.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = dt.NewRow();
                dr["REQ_DTTM"] = string.Empty;
                dr["REQ_ID"] = string.Empty;
                dr["REQ_ID1"] = string.Empty;
                dr["USERNAME"] = string.Empty;
                dr["NOTE"] = string.Empty;
                dr["EPQTID"] = string.Empty;
                dr["PRJT_NAME"] = string.Empty;
                dt.Rows.Add(dr);
                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetRequestDetail(string sReqID)
        {
            try
            {
                Util.gridClear(dgRequestDetailList);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                //IndataTable.Columns.Add("REQ_DATE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["REQ_ID"] = sReqID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                //Indata["REQ_DATE"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_DETAIL", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgRequestDetailList, dtMain, FrameOperation, true);
                //dgRequestDetailList.ItemsSource = DataTableConverter.Convert(dtMain);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable CreateReqDetlPrint(DataTable dt)
        {
            try
            {
                if (dgRequestDetailList.Rows.Count > 10)
                {
                    Util.AlertInfo("SFU1824");  //자재가 10개이상일 경우 MES팀에 연락주시기바랍니다. (투입요청서 레포트 줄 추가 해야함)
                    return null;
                }
                for (int i = 0; i < dgRequestDetailList.Rows.Count; i++)
                {
                    dt.Columns.Add("MTRLID_" + i, typeof(string));
                    dt.Columns.Add("MTRLDESC_" + i, typeof(string));
                    dt.Columns.Add("QTY_" + i, typeof(decimal));
                    dt.Columns.Add("HOPPER_" + i, typeof(string));
                }

                //DataRow dr = dt.NewRow();
                DataRow dr = null;
                dr = dt.NewRow();
                for (int i = 0; i < dgRequestDetailList.Rows.Count; i++)
                {
                    dr["MTRLID_" + i] = Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "MTRLID"));
                    dr["MTRLDESC_" + i] = Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "MTRLDESC"));
                    dr["QTY_" + i] = System.Math.Round(Convert.ToDecimal(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "MTRL_QTY")), 3);
                    dr["HOPPER_" + i] = Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "HOPPER")) + "[" + Util.NVC(DataTableConverter.GetValue(dgRequestDetailList.Rows[i].DataItem, "HOPPER_ID")) + "]";
                }
                dt.Rows.Add(dr);
                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private string GetRequestCount()
        {
            try
            {
                string sBizRule = uses_PNTR_WRKR_TYPE ? "DA_PRD_SEL_MIXMTRL_REQUEST_POPUP_SUBCONT" : "DA_PRD_SEL_MIXMTRL_REQUEST_POPUP";
                string _ValueToFind = string.Empty;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizRule, "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    _ValueToFind = dtMain.Rows[0]["REQ_ID"].ToString();
                }
                return _ValueToFind;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private string GetRequestCount_Dlgt()
        {
            try
            {
                string sBizRule = "DA_PRD_SEL_MIXMTRL_REQUEST_POPUP_SUBCONT_DLGT";
                string _ValueToFind = string.Empty;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizRule, "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    _ValueToFind = dtMain.Rows[0]["REQ_ID"].ToString();
                }
                return _ValueToFind;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void IsHopperMtrlChk()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = LoginInfo.CFG_PROC_ID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (string.Equals(dtResult.Rows[0]["MIXER_HOPPER_MTRL_USE_FLAG"], "Y"))
                    {
                        HopperMtrlFlag = true;
                        return;
                    }
                }
            }
            catch (Exception ex) { }

            HopperMtrlFlag = false;
        }

        private bool IsCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCDIUSE"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }


        public T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null)
                return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null)
                        break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtWorker.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = wndPerson.USERNAME;
                txtWorker.Tag = wndPerson.USERID;
            }
        }

        private void GetMtrlHistInfo() // [E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경
        {
            try
            {
                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("EQPTID", typeof(string));
                

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _EQPTID;

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_INPUT_MIX_STCK_MNGT_BY_EQPTID", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgMtrlHist, result, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        // 절연믹싱 추가 요청() [2019-08-08]
        private void btnJCMixing_Click(object sender, RoutedEventArgs e)
        {
            ELEC001_002_INPUT_JC_MIXING jcMixing = new ELEC001_002_INPUT_JC_MIXING();
            jcMixing.FrameOperation = FrameOperation;
            if (jcMixing != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = ldpDateFrom.SelectedDateTime;
                Parameters[1] = ldpDateTo.SelectedDateTime;
                Parameters[2] = cboEquipmentSegment.SelectedValue;

                C1WindowExtension.SetParameters(jcMixing, Parameters);

                jcMixing.Closed += new EventHandler(OnClosejcMixing);
                this.Dispatcher.BeginInvoke(new Action(() => jcMixing.ShowModal()));
            }
        }

        private void OnClosejcMixing(object sender, EventArgs e)
        {
            ELEC001_002_INPUT_JC_MIXING window = new ELEC001_002_INPUT_JC_MIXING();
            if (window.DialogResult == MessageBoxResult.OK)
                GetRequestList();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU2878", (sresult) => //승인하시겠습니까?
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dtRQSTDT = new DataTable();
                        dtRQSTDT.TableName = "RQSTDT";
                        dtRQSTDT.Columns.Add("REQ_NO", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_USERID", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_RSLT_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_NOTE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_BIZ_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("LANGID", typeof(string));

                        DataRow drnewrow = dtRQSTDT.NewRow();
                        drnewrow["REQ_NO"] = ReqID;
                        drnewrow["APPR_USERID"] = LoginInfo.USERID;
                        drnewrow["APPR_RSLT_CODE"] = "APP";
                        drnewrow["APPR_NOTE"] = String.Empty;
                        drnewrow["APPR_BIZ_CODE"] = "RMTRL_INPUT";
                        drnewrow["LANGID"] = LoginInfo.LANGID;

                        dtRQSTDT.Rows.Add(drnewrow);

                        new ClientProxy().ExecuteService("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.AlertByBiz("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", Exception.Message, Exception.ToString());
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                            GetRequestList();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU2866", (sresult) => //반려하시겠습니까?
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dtRQSTDT = new DataTable();
                        dtRQSTDT.TableName = "RQSTDT";
                        dtRQSTDT.Columns.Add("REQ_NO", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_USERID", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_RSLT_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_NOTE", typeof(string));
                        dtRQSTDT.Columns.Add("APPR_BIZ_CODE", typeof(string));
                        dtRQSTDT.Columns.Add("LANGID", typeof(string));

                        DataRow drnewrow = dtRQSTDT.NewRow();
                        drnewrow["REQ_NO"] = ReqID;
                        drnewrow["APPR_USERID"] = LoginInfo.USERID;
                        drnewrow["APPR_RSLT_CODE"] = "REJ";
                        drnewrow["APPR_NOTE"] = String.Empty;
                        drnewrow["APPR_BIZ_CODE"] = "RMTRL_INPUT";
                        drnewrow["LANGID"] = LoginInfo.LANGID;

                        dtRQSTDT.Rows.Add(drnewrow);

                        new ClientProxy().ExecuteService("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                        {
                            if (Exception != null)
                            {
                                Util.MessageException(Exception);
                                Util.AlertByBiz("BR_PRD_UPD_APPR_SUBCONT_RMTRL_INPUT", Exception.Message, Exception.ToString());
                                return;
                            }
                            Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                            GetRequestList();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        GetRequestList();
                    }
                }
            });
        }
        // [E20230907-001465] - 재고량, 요청중량, 투입 후 이론중량, 설정값 컬럼 추가 및 저장 기능 추가
        private bool IsAreaCommonCodeUse(string sComeCodeType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                //RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                //dr["COM_CODE"] = sComeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }
        // [E20231027-001625]건 추가
        private void refreshSET_QTY() 
        {
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                decimal before_input_qty = Convert.ToDecimal(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "SET_QTY")) + Convert.ToDecimal(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "STCK_QTY")); //[E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경              
                decimal current_set_qty = Convert.ToDecimal(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "SET_QTY"));
                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "SET_QTY", (current_set_qty * curr_input_percent / 100));    //E20230907-001465 2023.10.18 수정                
                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "STCK_QTY", before_input_qty - Convert.ToDecimal(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "SET_QTY")));    //[E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경              
                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "INPUT_AFTER_THRY_WEIGHT", Convert.ToDecimal(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "STCK_QTY")));    //[E20231129-001107] - 투입 히스토리 및 재고 값 산출 변경              
            }
        }

        private void mngtHist_Save()
        {
            inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("WOID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow inDataRow = null;

            inDataRow = inDataTable.NewRow();
            inDataRow["EQPTID"] = _EQPTID;
            inDataRow["WOID"] = WO_ID;
            inDataRow["USERID"] = LoginInfo.USERID;

            inDataTable.Rows.Add(inDataRow);


            DataTable inDataTable1 = inDataSet.Tables.Add("INMTRLDATA");
            inDataTable1.Columns.Add("MTRLID", typeof(string)); // 양영재
            inDataTable1.Columns.Add("HOPPER_ID", typeof(string)); // 양영재
            inDataTable1.Columns.Add("STCK_QTY", typeof(decimal)); // 양영재
            inDataTable1.Columns.Add("INPUT_AFTER_THRY_WEIGHT", typeof(decimal));
            inDataTable1.Columns.Add("SET_QTY", typeof(decimal));

            dgProductLot.EndEdit();
            DataTable dtTop = DataTableConverter.Convert(dgProductLot.ItemsSource);

            foreach (DataRow _iRow in dtTop.Rows)
            {
                if (_iRow["CHK"].Equals(1))
                {
                    if (_iRow["MTRL_QTY"].Equals("") || _iRow["MTRL_QTY"].Equals("0"))
                    {
                        //투입요청수량을 입력 하세요.
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1978"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        Util.MessageInfo("SFU1978", (result) =>
                        {
                            Keyboard.Focus(dgProductLot.CurrentCell.DataGrid);
                        });
                        InputFlag = false;
                        return;
                    }
                    if (Double.Parse(Util.NVC(_iRow["MTRL_QTY"])) < 0)
                    {
                        Util.MessageValidation("SFU1977");   //투입요청수량은 정수만 입력 하세요.
                        InputFlag = false;
                        return;
                    }
                    else
                    {
                        InputFlag = true;
                    }
                    if (_iRow["HOPPER_ID"].Equals(""))
                    {
                        HopperFlag = false;
                        return;
                    }
                    else
                    {
                        HopperFlag = true;
                    }

                    inDataTable1.ImportRow(_iRow);
                }
            }

            new ClientProxy().ExecuteService_Multi("BR_MAT_REG_INPUT_MIX_STCK_MNGT_HIST", "INDATA,INMTRLDATA", null, (result, ex) =>
            {
                try
                {
                    if (ex != null)
                    {
                        Util.AlertByBiz("BR_MAT_REG_INPUT_MIX_STCK_MNGT_HIST", ex.Message, ex.ToString());
                        return;
                    }
                }
                catch (Exception ErrEx)
                {
                    Util.MessageException(ErrEx);
                }
            }, inDataSet);
        }
    }
}