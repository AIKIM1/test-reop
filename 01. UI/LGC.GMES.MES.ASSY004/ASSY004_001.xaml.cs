/*************************************************************************************
 Created Date : 2019.04.11
      Creator : INS 김동일K
   Decription : CWA3동 증설 - Notching 공정진척 화면 (ASSY0001.ASSY001_001 2019/04/11 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.11  INS 김동일K : Initial Created.
  2019.09.25  LG CNS 김대근 : 금형관리 탭 추가
  2020.05.12  김동일 : [C20200511-000024] 작업조, 작업자 등록 변경
  2020.08.21  신광희 : [C20200814-000102] 시생산 재공 표기 추가 및 추가기능 – 시생산 모드 설정/해제 Validation 주석처리
  2021.06.23  김동일 : C20210514-000019 설정의 설비정보로 극성 및 설비 콤보 자동 선택 기능 추가 건
**************************************************************************************/


using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_001.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_001 : UserControl, IWorkArea
    {

        #region Declaration & Constructor

        private string sCaldate = string.Empty;
        private System.DateTime dtCaldate;

        private bool bTestMode = false;

        // 2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
        private bool bLoaded = true;
        private bool bchkWait = true;
        private bool bchkRun = true;
        private bool bchkEqpEnd = true;

        private bool bWndConfirmJobEnd = true;  //로직이 중복으로 타는 현상 발생해 방지하기 위해

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        CurrentLotInfo _CurrentLotInfo = new CurrentLotInfo();

        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();
        private COM001_314_HIST winInputHistTool = new COM001_314_HIST();

        //2019.02.20 오화백 RF_ID 투입부, 배출부 RFID  
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty; //투입부
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; //배출부

        // 전체 적용 여부 CCB 결과 없음. 지급 요청 건으로 하드 코딩.
        private List<string> _SELECT_USER_MODE_AREA = new List<string>(new string[] { "A7","A9" });   // 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY004_001()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.NOTCHING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_PROC");

            String[] sFilter3 = { "ELEC_TYPE", GetBasicPolarity() };
            C1ComboBox[] cboLineChild2 = { cboEquipment };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.NONE, cbChild: cboLineChild2, sFilter: sFilter3, sCase: "POLARITY");

            String[] sFilter2 = { Process.NOTCHING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboElecType };  //C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboElecType };
            C1ComboBox[] cboChild = { cboMountPstsID };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbChild: cboChild, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_NT_AUTO_CFG");

            // 투입위치 코드
            String[] sFilter4 = { "PROD" };
            C1ComboBox[] cboMountPstParent = { cboEquipment };
            _combo.SetCombo(cboMountPstsID, CommonCombo.ComboStatus.NONE, cbParent: cboMountPstParent, sFilter: sFilter4, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");


            #region EDC BCR Reading Info
            // 자동 조회 시간 Combo
            String[] sFilter9 = { "EDC_BCD_RATE_INTERVAL" };
            _combo.SetCombo(cboEdcAutoSearch, CommonCombo.ComboStatus.NA, sFilter: sFilter9, sCase: "COMMCODE");

            if (cboEdcAutoSearch != null && cboEdcAutoSearch.Items != null && cboEdcAutoSearch.Items.Count > 0)
                cboEdcAutoSearch.SelectedIndex = cboEdcAutoSearch.Items.Count - 1;

            #endregion
        }

        private void InitFaultyDataGrid(bool bClearAll = false)
        {
            if (bClearAll)
            {
                Util.gridClear(dgFaulty);

                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    if (Util.NVC(dgFaulty.Columns[i].Name).ToString().StartsWith("DEFECTQTY"))
                    {
                        dgFaulty.Columns.RemoveAt(i);
                    }
                }
            }
            else
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    if (Util.NVC(dgFaulty.Columns[i].Name).ToString().StartsWith("DEFECTQTY"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgFaulty.ItemsSource);
                        if (dt.Columns.Count > i)
                            if (dt.Columns[i].ColumnName.Equals(dgFaulty.Columns[i].Name))
                                dt.Columns.RemoveAt(i);
                        
                        dgFaulty.Columns.RemoveAt(i);
                    }
                }
            }
        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                btnRunStart.Visibility = Visibility.Collapsed;
                btnRunCancel.Visibility = Visibility.Collapsed;
                btnRunCompleteCancel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event

        #region [Main Window]

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();

            this.RegisterName("redBrush", redBrush);

            HideTestMode();


            #region EDC BCR Reading Info
            this.RegisterName("BCR_WARNING", edcBrush);

            if (dspTmr_EdcWarn != null)
            {
                int iSec = 0;

                if (cboEdcAutoSearch != null && cboEdcAutoSearch.SelectedValue != null && !cboEdcAutoSearch.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboEdcAutoSearch.SelectedValue.ToString());

                dspTmr_EdcWarn.Tick -= dspTmr_EdcWarn_Tick;
                dspTmr_EdcWarn.Tick += dspTmr_EdcWarn_Tick;
                dspTmr_EdcWarn.Interval = new TimeSpan(0, 0, iSec);
                //dispatcherTimer.Start();
            }
            #endregion
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            SetWorkOrderWindow();

            SetInputHistToolWindow();

            //if (chkWait != null && chkWait.IsChecked.HasValue)
            //    chkWait.IsChecked = true;
            //if (chkRun != null && chkRun.IsChecked.HasValue)
            //    chkRun.IsChecked = true;
            //if (chkEqpEnd != null && chkEqpEnd.IsChecked.HasValue)
            //    chkEqpEnd.IsChecked = true;

            #region [버튼 권한 적용에 따른 처리]            
            GetButtonPermissionGroup();
            #endregion

            #region EDC BCR Reading Info
            CheckEDCVisibity();
            #endregion

            chkWait.Checked -= chkWait_Checked;
            chkWait.Unchecked -= chkWait_Unchecked;
            chkRun.Checked -= chkRun_Checked;
            chkRun.Unchecked -= chkRun_Unchecked;
            chkEqpEnd.Checked -= chkEqpEnd_Checked;
            chkEqpEnd.Unchecked -= chkEqpEnd_Unchecked;

            chkWait.Checked += chkWait_Checked;
            chkWait.Unchecked += chkWait_Unchecked;
            chkRun.Checked += chkRun_Checked;
            chkRun.Unchecked += chkRun_Unchecked;
            chkEqpEnd.Checked += chkEqpEnd_Checked;
            chkEqpEnd.Unchecked += chkEqpEnd_Unchecked;

            bLoaded = false;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPilotProdMode();

            if (!CanSearch())
            {
                HideLoadingIndicator();
                return;
            }

            //2019.02.20 오화백 RF_ID 투입부, 배출부 여부 체크
            GetLotIdentBasCode();

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                dgDetail.Columns["CSTID"].Visibility = Visibility.Visible;                
                dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgDetail.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                dgProductLot.Columns["PR_CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgProductLot.Columns["PR_CSTID"].Visibility = Visibility.Collapsed;
            }

            GetButtonPermissionGroup();

            GetWorkOrder();
            GetProductLot();

            GetWrokCalander();

        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            ASSY004_001_RUNSTART wndRunStart = new ASSY004_001_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                object[] Parameters = new object[9];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.NOTCHING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "PR_LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "PR_CSTID"));
                Parameters[5] = cboMountPstsID.SelectedValue;
                Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "MO_PRODID"));
                Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPQTY_M_EA"));

                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.Closed += new EventHandler(wndRunStart_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
            }
        }
        
        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanRunComplete())
                    return;

                if (!ChkEqptCnfmType("EQPT_END_W"))
                    return;

                ASSY004_001_EQPTEND wndEqpend = new ASSY004_001_EQPTEND();
                wndEqpend.FrameOperation = FrameOperation;

                if (wndEqpend != null)
                {
                    object[] Parameters = new object[9];
                    Parameters[0] = Process.NOTCHING;
                    Parameters[1] = cboEquipmentSegment.SelectedValue.ToString(); 
                    Parameters[2] = cboEquipment.SelectedValue.ToString();
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
                    Parameters[7] = cboMountPstsID != null && cboMountPstsID.SelectedValue != null ? cboMountPstsID.SelectedValue.ToString() : "";
                    Parameters[8] = CheckProdQtyChgPermission();

                    C1WindowExtension.SetParameters(wndEqpend, Parameters);

                    wndEqpend.Closed += new EventHandler(wndEqpend_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndEqpend.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnConfirm.IsEnabled = false;
                
                // WorkCalander 체크.
                GetWrokCalander();
                
                if (!CheckCanConfirm())
                    return;

                if (!ChkEqptCnfmType("CONFIRM_W"))
                    return;

                #region 작업조건/품질정보 입력 여부
                string _ValueToLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                // 작업조건 등록 여부
                if (Util.EQPTCondition(Process.NOTCHING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
                {
                    btnEqptCond_Click(null, null);
                    return;
                }
                // 품질정보 등록 여부
                if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, Process.NOTCHING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
                {
                    btnQualityInput_Click(null, null);
                    return;
                }
                #endregion

                // 설비 Loss 등록 여부 체크
                DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.NOTCHING);

                if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                {
                    string sInfo = string.Empty;
                    string sLossInfo = string.Empty;

                    for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                    {
                        sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                        sLossInfo = sLossInfo + "\n" + sInfo;
                    }

                    object[] param = new object[] { sLossInfo };

                    // 미입력한 설비 Loss가 존재합니다. 확정하시겠습니까? %1
                    Util.MessageConfirm("SFU3501", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Confirm_Process();
                        }
                    }, param);
                }
                else
                {
                    Confirm_Process();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnConfirm.IsEnabled = true;
            }
        }

        private void btnRemarkHist_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return;
            }

            ASSY004_001_LOTCOMMENTHIST wndLotCommentHist = new ASSY004_001_LOTCOMMENTHIST();
            wndLotCommentHist.FrameOperation = FrameOperation;

            if (wndLotCommentHist != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                C1WindowExtension.SetParameters(wndLotCommentHist, Parameters);

                wndLotCommentHist.Closed += new EventHandler(wndLotCommentHist_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndLotCommentHist.ShowModal()));
            }
        }

        private void btnEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                //설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

            CMM_COM_EQPCOMMENT wndEqpComment = new CMM_COM_EQPCOMMENT();
            wndEqpComment.FrameOperation = FrameOperation;

            if (wndEqpComment != null)
            {
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.NOTCHING;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                Parameters[6] = txtShift.Text; // 작업조명
                Parameters[7] = txtShift.Tag; // 작업조코드
                Parameters[8] = txtWorker.Text; // 작업자명
                Parameters[9] = txtWorker.Tag; // 작업자 ID

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
            }
        }
        
        //2019.04.22 김대근 수정
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //인쇄할 항목이 없는 경우 발행 팝업 출력.
            if (!CanPrintPopup())
                return;

            ASSY004_001_HIST wndPrint = new ASSY004_001_HIST();
            wndPrint.FrameOperation = FrameOperation;

            if (wndPrint != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Process.NOTCHING;
                Parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[2] = cboEquipment.SelectedValue.ToString();
                //_UNLDR코드를 wndPrint로 보낸다.
                Parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndPrint, Parameters);

                wndPrint.Closed += new EventHandler(wndPrint_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndPrint.ShowModal()));
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }
             
        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();
                GetPilotProdMode();


                if (cboEquipment.SelectedIndex == 0)
                {
                    if (winWorkOrder != null)
                    {
                        winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
                        winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
                        winWorkOrder.PROCID = Process.NOTCHING;

                        winWorkOrder.ClearWorkOrderInfo();
                    }
                }

                ClearControls();

                txtWorker.Text = "";
                txtWorker.Tag = "";
                txtShift.Text = "";
                txtShift.Tag = "";
                txtShiftStartTime.Text = "";
                txtShiftEndTime.Text = "";
                txtShiftDateTime.Text = "";
                txtLossCnt.Text = "";
                txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);

                // 설비 선택 시 자동 조회 처리
                if (cboEquipment.SelectedIndex > 0 && cboEquipment.Items.Count > cboEquipment.SelectedIndex)
                {
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        ShowLoadingIndicator();

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            btnSearch_Click(null, null);
                        }));
                    }
                }

                #region EDC BCR Reading Info
                if (tbEDCBCRInfo.Visibility == Visibility.Visible)
                {
                    GetBcrReadingRate(true);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEqptCondInfo())
                return;

            CMM_ASSY_EQPT_COND wndEqptCond = new CMM_ASSY_EQPT_COND();
            wndEqptCond.FrameOperation = FrameOperation;

            if (wndEqptCond != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.NOTCHING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                C1WindowExtension.SetParameters(wndEqptCond, Parameters);

                wndEqptCond.Closed += new EventHandler(wndEqptCond_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
            }
        }

        private void btnEqptCondSearch_Click(object sender, RoutedEventArgs e)
        {

            if (!CanSearch())
                return;

            CMM_ASSY_EQPT_COND_SEARCH wndEqptCondSearch = new CMM_ASSY_EQPT_COND_SEARCH();
            wndEqptCondSearch.FrameOperation = FrameOperation;

            if (wndEqptCondSearch != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = Process.NOTCHING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndEqptCondSearch, Parameters);

                wndEqptCondSearch.Closed += new EventHandler(wndEqptCondSearch_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCondSearch.ShowModal()));
            }
        }

        private void cboEqptDfctLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null)
                return;

            GetEqpFaultyData();
        }
        
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (sCaldate.Equals("")) return;

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    // 선택할 수 없습니다.
                    Util.MessageValidation("SFU1669");  
                    //e.Handled = false;
                    return;
                }
            }));
        }

        //2019.04.24 김대근 / 현재 PROCID를 wndWait로 전달
        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY004_COM_WAITLOT wndWait = new ASSY004_COM_WAITLOT();
            wndWait.FrameOperation = FrameOperation;

            if (wndWait != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.NOTCHING;
                Parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;
                C1WindowExtension.SetParameters(wndWait, Parameters);

                wndWait.Closed += new EventHandler(wndWait_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWait.ShowModal()));
            }
        }
        
        private void btnQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            CMM_COM_SELF_INSP wndQualityInput = new CMM_COM_SELF_INSP();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = Process.NOTCHING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[3] = Util.NVC(cboEquipment.Text);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInput_New_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
            }
        }
        
        /// <summary>
        /// 2019.02.20 오화백 RF_ID 투입부, 배출부 여부 라인콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                #region EDC BCR Reading Info
                CheckEDCVisibity();
                #endregion

                GetLotIdentBasCode();

                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    if (dgDetail.Columns.Contains("CSTID"))
                        dgDetail.Columns["CSTID"].Visibility = Visibility.Visible;

                    dgProductLot.Columns["CSTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgDetail.Columns.Contains("CSTID"))
                        dgDetail.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    dgProductLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                }

                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    dgProductLot.Columns["PR_CSTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgProductLot.Columns["PR_CSTID"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWipNote_Click(object sender, RoutedEventArgs e)
        {
            if (Util.GetCondition(cboEquipmentSegment).Equals(""))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return;
            }
            if (Util.GetCondition(cboEquipment).Equals(""))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1153");
                return;
            }

            CMM_COM_WIP_NOTE wndWipNote = new CMM_COM_WIP_NOTE();
            wndWipNote.FrameOperation = FrameOperation;

            if (wndWipNote != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.NOTCHING;

                C1WindowExtension.SetParameters(wndWipNote, Parameters);

                wndWipNote.Closed += new EventHandler(wndWipNote_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWipNote.ShowModal()));
            }
        }

        private void btnDfctCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgDetail.ItemsSource);

                if (!CommonVerify.HasTableRow(dtTmp)) return;

                Dictionary<string, string> dList = new Dictionary<string, string>();

                foreach (DataRow dr in dtTmp.Rows)
                {
                    dList.Add(Util.NVC(dr["LOTID"]), Util.NVC(dr["WIPSEQ"]));
                }
                
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "DFCT_QTY_CHG_BLOCK_FLAG")), "Y"))
                    return;

                CMM_ASSY_DFCT_CELL_REG wndDfctCellReg = new CMM_ASSY_DFCT_CELL_REG();
                wndDfctCellReg.FrameOperation = FrameOperation;

                if (wndDfctCellReg != null)
                {
                    object[] Parameters = new object[7];

                    Parameters[0] = dList;
                    Parameters[1] = dList;
                    Parameters[2] = cboEquipmentSegment.SelectedValue;
                    Parameters[3] = cboEquipment.SelectedValue;
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNCODE"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "ACTID"));
                    Parameters[6] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNNAME"));

                    C1WindowExtension.SetParameters(wndDfctCellReg, Parameters);

                    wndDfctCellReg.Closed += new EventHandler(wndDfctCellReg_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndDfctCellReg.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAutoHoldHist_Click(object sender, RoutedEventArgs e)
        {
            if (Util.GetCondition(cboEquipmentSegment).Equals(""))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1223");
                return;
            }
            //if (Util.GetCondition(cboEquipment).Equals(""))
            //{
            //    // 설비를 선택하세요.
            //    Util.MessageValidation("SFU1153");
            //    return;
            //}

            ASSY004_COM_CHG_HOLD_RESN wndAutoHoldHist = new ASSY004_COM_CHG_HOLD_RESN();
            wndAutoHoldHist.FrameOperation = FrameOperation;

            if (wndAutoHoldHist != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.NOTCHING;

                C1WindowExtension.SetParameters(wndAutoHoldHist, Parameters);

                wndAutoHoldHist.Closed += new EventHandler(wndAutoHoldHist_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndAutoHoldHist.ShowModal()));
            }
        }

        private void btnRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCompleteCancelRun())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunCompleteCancel();
                }
            });
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCancelRun())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        #endregion

        #region [작업대상]

        private void dgProductLot_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    _CurrentLotInfo.resetCurrentLotInfo();
                                    ClearDetailData();

                                    if (!SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row))
                                        return;

                                    ProdListClickedProcess(e.Cell.Row.Index);

                                    //SetEnabled(dg.Rows[e.Cell.Row.Index].DataItem);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = e.Cell.Row.Index;
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    _CurrentLotInfo.resetCurrentLotInfo();

                                    ClearDetailData();

                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    SetCheckProdListSameChildSeq(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Row, true);
                                }
                                GetInputHistTool();
                                break;
                        }

                        if (dgProductLot.CurrentCell != null)
                            dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.CurrentCell.Row.Index, dgProductLot.Columns.Count - 1);
                        else if (dgProductLot.Rows.Count > 0)
                            dgProductLot.CurrentCell = dgProductLot.GetCell(dgProductLot.Rows.Count, dgProductLot.Columns.Count - 1);
                    }
                }
            }));
        }

        private void chkWait_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgProductLot);
                    ClearLotInfo();
                    ClearDetailData();

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkWait == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }
                        ));
                    }
                }
            }
        }

        private void chkRun_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgProductLot);
                    ClearLotInfo();
                    ClearDetailData();

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkRun == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }
                        ));
                    }
                }
            }
        }

        private void chkEqpEnd_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked.HasValue)
            {
                if ((bool)(sender as CheckBox).IsChecked)
                {
                    Util.gridClear(dgProductLot);
                    ClearLotInfo();
                    ClearDetailData();

                    // 상태 변경 시 자동 조회
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (bchkEqpEnd == false)
                            {
                                btnSearch_Click(null, null);
                            }
                        }
                        ));
                    }
                }
            }
        }

        private void chkWait_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkWait = false;
            }
        }

        private void chkRun_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkRun = false;
            }
        }

        private void chkEqpEnd_Unchecked(object sender, RoutedEventArgs e)
        {
            // 상태 변경 시 자동 조회
            if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                bchkEqpEnd = false;
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTTYPE")).Equals("X"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2424"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgProductLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        #endregion

        #region [실적확인]        

        /// <summary>
        /// 2019.02.20 오화백 RF_ID 투입부, 배출부 여부 Carrier관리 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDetail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDetail.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CSTID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_CST_HIST wndHist = new CMM_CST_HIST();
                            wndHist.FrameOperation = FrameOperation;

                            if (wndHist != null)
                            {
                                object[] Parameters = new object[1];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[cell.Row.Index].DataItem, cell.Column.Name));

                                C1WindowExtension.SetParameters(wndHist, Parameters);

                                wndHist.Closed += new EventHandler(wndHist_Closed);
                                
                                this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                            }
                        }
                    }
                    //else if (cell.Column.Name == "LOTID")
                    //{
                    //    if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                    //    {
                    //        CMM_ASSY_CELL_INFO wndCell = new CMM_ASSY_CELL_INFO();
                    //        wndCell.FrameOperation = FrameOperation;

                    //        if (wndCell != null)
                    //        {
                    //            object[] Parameters = new object[2];
                    //            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[cell.Row.Index].DataItem, cell.Column.Name));
                    //            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[cell.Row.Index].DataItem, "CSTID"));

                    //            C1WindowExtension.SetParameters(wndCell, Parameters);

                    //            wndCell.Closed += new EventHandler(wndCell_Closed);

                    //            this.Dispatcher.BeginInvoke(new Action(() => wndCell.ShowModal()));
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        
        /// <summary>
        /// 2019.02.20 오화백 RF_ID 투입부, 배출부 여부 생산실적 리스트의 CSTID의 링크색 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgDetail.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (Util.NVC(e.Cell.Column.Name).Equals("CSTID")) // || Util.NVC(e.Cell.Column.Name).Equals("LOTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        #endregion

        #region [Tabs]
        private void btnSaveFaulty_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonVerify.HasDataGridRow(dgFaulty))
            {
                //불량항목이 없습니다.
                Util.MessageValidation("SFU1588");
                return;
            }

            // Lot 상태 체크
            if (GetWipStat().Equals(Wip_State.END) || GetWipStat().Equals(""))
            {
                Util.MessageValidation("SFU2063"); // 재공상태를 확인해주세요.
                return;
            }

            //불량정보를 저장 하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetFaulty();
                }
            });
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboRemarkLot.SelectedIndex < 0)
            {
                //LOT정보가 없습니다.
                Util.MessageValidation("SFU1386");
                return;
            }

            //저장하시겠습니까
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetRemark();
                }
            });
        }

        private void cboRemarkLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null)
                return;

            GetRemark();
        }

        private void dgFaulty_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (!e.Cell.Column.Name.StartsWith("DEFECTQTY"))
                return;

            double dfct, loss, charge_prd;

            double sum = SumDefectQty(dgFaulty, e.Cell.Column.Name, out dfct, out loss, out charge_prd);

            double totSum = sum;

            if (!_CurrentLotInfo.STATUS.Equals("WAIT"))
            {
                int iRow = -1;
                double inputqty = 0;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    if (e.Cell.Column.Header.ToString().IndexOf(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"))) >= 0)
                    {
                        inputqty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
                        iRow = i;
                        break;
                    }
                }

                if (iRow < 0) return;

                if (inputqty < totSum)
                {
                    //생산량보다 불량이 크게 입력 될 수 없습니다.
                    Util.MessageValidation("SFU1608");

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);

                    sum = SumDefectQty(dgFaulty, e.Cell.Column.Name, out dfct, out loss, out charge_prd);
                    totSum = sum;

                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DFCT_SUM", totSum);
                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_DEFECT_LOT", dfct);
                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_LOSS_LOT", loss);
                    DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_CHARGE_PROD_LOT", charge_prd);

                    SetLotInfoCalc();
                    return;
                }

                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DFCT_SUM", totSum);

                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_DEFECT_LOT", dfct);
                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_LOSS_LOT", loss);
                DataTableConverter.SetValue(dgDetail.Rows[iRow].DataItem, "DTL_CHARGE_PROD_LOT", charge_prd);

                SetLotInfoCalc();
            }
        }

        private void btnSearchEqpFaulty_Click(object sender, RoutedEventArgs e)
        {
            if (dgDetail == null || dgDetail.Rows.Count < 1)
                return;

            GetEqpFaultyData();
        }

        private void dgFaulty_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    string sColName = "DEFECTQTY" + (i + 1).ToString();
                    if (e.Column.Name.Equals(sColName))
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(e.Cell.Column.Name).Equals("ACTNAME"))
                    {
                        //if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "RSLT_EXCL_FLAG")).Equals("Y"))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}
                        //else
                        //{
                            string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                            if (sFlag == "Y")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                            else
                            {
                                e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                        //}
                    }
                }
            }));
        }

        private void dgFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqpFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(e.Cell.Column.Name).Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // 합계 컬럼 색 변경.
                        if (!Util.NVC(e.Cell.Column.Name).Equals("EQPT_DFCT_SUM_GR_NAME") &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_DFCT_GR_SUM_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgEqpFaulty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_ASSY_EQPT_DFCT_CELL_INFO wndEqptDfctCell = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
                            wndEqptDfctCell.FrameOperation = FrameOperation;

                            if (wndEqptDfctCell != null)
                            {
                                object[] Parameters = new object[3];

                                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                                if (idx < 0) return;

                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                                Parameters[1] = cboEquipment.SelectedValue;
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID"));

                                C1WindowExtension.SetParameters(wndEqptDfctCell, Parameters);

                                wndEqptDfctCell.Closed += new EventHandler(wndEqptDfctCell_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndEqptDfctCell.ShowModal()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqpFaulty_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgEqpFaulty.TopRows.Count; i < dgEqpFaulty.Rows.Count; i++)
                {
                    if (dgEqpFaulty.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") ||
                           Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
                        {
                            if (bStrt)
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                                idxS = i;

                                if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                    bStrt = false;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        public void GetProductLot(bool bSelPrv = true)
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                _CurrentLotInfo.resetCurrentLotInfo();

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                }

                ClearControls();

                ShowLoadingIndicator();

                string sInQuery = string.Empty;

                if (chkWait.IsChecked.HasValue && (bool)chkWait.IsChecked)
                    sInQuery = "WAIT";

                if (chkRun.IsChecked.HasValue && (bool)chkRun.IsChecked)
                {
                    if (sInQuery.Equals(""))
                        sInQuery = "PROC";
                    else
                        sInQuery = sInQuery + ",PROC";
                }

                if (chkEqpEnd.IsChecked.HasValue && (bool)chkEqpEnd.IsChecked)
                {
                    if (sInQuery.Equals(""))
                        sInQuery = "EQPT_END";
                    else
                        sInQuery = sInQuery + ",EQPT_END";
                }

                if (bLoaded == true)
                {
                    sInQuery = "WAIT,PROC,EQPT_END";
                }

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["WIPSTAT"] = sInQuery;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_NT_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (!sPrvLot.Equals("") && bSelPrv)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);

                                SetCheckProdListSameChildSeq(dgProductLot.Rows[idx]);

                                //row 색 바꾸기
                                dgProductLot.SelectedIndex = idx;

                                ProdListClickedProcess(idx);
                                
                                dgProductLot.CurrentCell = dgProductLot.GetCell(idx, dgProductLot.Columns.Count - 1);
                            }
                        }
                        else
                        {
                            if (searchResult.Rows.Count > 0)
                            {
                                int iRowRun = _Util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)  // 진행중인 경우에만 자동 체크 처리.
                                {

                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    SetCheckProdListSameChildSeq(dgProductLot.Rows[iRowRun], true);

                                    ProdListClickedProcess(iRowRun);

                                    dgProductLot.CurrentCell = dgProductLot.GetCell(iRowRun, dgProductLot.Columns.Count - 1);
                                }
                            }

                            if (dgProductLot.Rows.Count > 0)
                            {
                                dgProductLot.CurrentCell = dgProductLot.GetCell(0, dgProductLot.Columns.Count - 1);
                            }
                        }

                        // 2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
                        GetLossCnt();
                        GetInputHistTool();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetDetailData()
        {
            try
            {
                ShowLoadingIndicator();

                int iGrpSeq = 0;
                int.TryParse(_CurrentLotInfo.CHILD_GR_SEQNO, out iGrpSeq);

                DataTable inTable = _Biz.GetDA_PRD_SEL_CHILDINFO_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _CurrentLotInfo.PR_LOTID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CHILD_GR_SEQNO"] = iGrpSeq;
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_INFO_BY_PR_NT_L", "INDATA", "OUTDATA", inTable);
                
                Util.GridSetData(dgDetail, searchResult, FrameOperation, true);

                SetRemarkCombo();

                SetEqptDfctCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }

        }

        private void GetFaultyData()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PROD_SEL_ACTVITYREASON_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["EQPTID"] = cboEquipment.SelectedValue == null ? "" : cboEquipment.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROCACTRSN_CODE_L", "INDATA", "OUTDATA", inTable);
                
                Util.GridSetData(dgFaulty, searchResult, FrameOperation, false);

                // Defect Column 생성..
                if (dgDetail.Rows.Count - dgDetail.TopRows.Count > 0)
                {
                    InitFaultyDataGrid();

                    for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                    {
                        string sColName = "DEFECTQTY" + (i + 1).ToString();

                        // 불량 수량 컬럼 위치 변경.
                        int iColIdx = 0;

                        iColIdx = dgFaulty.Columns["COST_CNTR_NAME"].Index;

                        Util.SetGridColumnNumeric(dgFaulty, sColName, null, Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, iColIdx, "#,##0");  // 부품 항목 앞으로 위치 이동.

                        if (dgFaulty.Columns.Contains(sColName))
                        {
                            (dgFaulty.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Minimum = 0;
                            (dgFaulty.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                            (dgFaulty.Columns[sColName] as C1.WPF.DataGrid.DataGridNumericColumn).EditOnSelection = true;
                        }

                        if (dgFaulty.Rows.Count == 0) continue;

                        DataTable dt = GetFaultyDataByLot(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")));

                        BindingDataGrid(dgFaulty, dt, sColName);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private DataTable GetFaultyDataByLot(string sLotID, string sWipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("WIPSEQ", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow newRow = RQSTDT.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipseq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                RQSTDT.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_L", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetEqpFaultyData()
        {
            try
            {
                if (cboEqptDfctLot.SelectedValue == null || cboEqptDfctLot.SelectedValue.ToString().Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = cboEqptDfctLot.SelectedValue;
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", cboEqptDfctLot.SelectedValue.ToString())].DataItem, "WIPSEQ"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, true);

                        dgEqpFaulty.MergingCells -= dgEqpFaulty_MergingCells;
                        dgEqpFaulty.MergingCells += dgEqpFaulty_MergingCells;

                        if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                            dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                        else
                            dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetRemark()
        {
            try
            {
                if (cboRemarkLot.SelectedValue == null || cboRemarkLot.SelectedValue.ToString().Equals(""))
                    return;

                ShowLoadingIndicator();

                rtxRemark.Document.Blocks.Clear();

                DataTable inTable = _Biz.GetDA_PRD_SEL_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = cboRemarkLot.SelectedValue;
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", cboRemarkLot.SelectedValue.ToString())].DataItem, "WIPSEQ"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count > 0 && !Util.NVC(searchResult.Rows[0]["WIP_NOTE"]).Equals(""))
                            rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetRemark()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_UPD_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(cboRemarkLot.SelectedValue);
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", cboRemarkLot.SelectedValue.ToString())].DataItem, "WIPSEQ"));
                newRow["WIP_NOTE"] = new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //저장 되었습니다.
                        Util.MessageInfo("SFU1270");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private void SetFaulty(bool bMsgShow = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgFaulty.EndEdit();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable inDEFECT_LOT = indataSet.Tables.Add("INRESN");
                inDEFECT_LOT.Columns.Add("LOTID", typeof(string));
                inDEFECT_LOT.Columns.Add("WIPSEQ", typeof(string));
                inDEFECT_LOT.Columns.Add("ACTID", typeof(string));
                inDEFECT_LOT.Columns.Add("RESNCODE", typeof(string));
                inDEFECT_LOT.Columns.Add("RESNQTY", typeof(double));
                inDEFECT_LOT.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDEFECT_LOT.Columns.Add("PROCID_CAUSE", typeof(string));
                inDEFECT_LOT.Columns.Add("RESNNOTE", typeof(string));
                inDEFECT_LOT.Columns.Add("DFCT_TAG_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("LANE_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("LANE_PTN_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("COST_CNTR_ID", typeof(string));
                inDEFECT_LOT.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(newRow);

                for (int i = 0; i < dgFaulty.Columns.Count; i++)
                {
                    if (Util.NVC(dgFaulty.Columns[i].Name).StartsWith("DEFECTQTY"))
                    {
                        string sLot = dgFaulty.Columns[i].Header.ToString().Replace("[#]", "").Trim();
                        string sWipSeq = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[_Util.GetDataGridRowIndex(dgDetail, "LOTID", sLot)].DataItem, "WIPSEQ"));
                        string sColName = dgFaulty.Columns[i].Name.ToString();

                        if (string.IsNullOrEmpty(sWipSeq))
                        {
                            return;
                        }

                        for (int j = 0; j < dgFaulty.Rows.Count - dgFaulty.BottomRows.Count; j++)
                        {
                            newRow = null;

                            newRow = inDEFECT_LOT.NewRow();
                            newRow["LOTID"] = sLot;
                            newRow["WIPSEQ"] = sWipSeq;
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTID"));
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "RESNCODE"));
                            newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, sColName)).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, sColName)));
                            newRow["RESNCODE_CAUSE"] = "";
                            newRow["PROCID_CAUSE"] = "";
                            newRow["RESNNOTE"] = "";
                            //newRow["DFCT_TAG_QTY"] = 0;
                            newRow["LANE_QTY"] = 1;
                            newRow["LANE_PTN_QTY"] = 1;

                            if (Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                                newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "COST_CNTR_ID"));
                            else
                                newRow["COST_CNTR_ID"] = "";

                            newRow["A_TYPE_DFCT_QTY"] = 0;
                            newRow["C_TYPE_DFCT_QTY"] = 0;

                            inDEFECT_LOT.Rows.Add(newRow);
                        }
                    }
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    HideLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL_NT", "INDATA,INRESN", null, indataSet);

                if (bMsgShow)
                {
                    Util.MessageInfo("SFU1275");
                    GetFaultyData();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }
        
        private string GetMaxChildGRPSeq(string sPLot)
        {
            if (sPLot.Equals(""))
                return "";

            string sRet = string.Empty;

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MAXCHILDGRSEQ();

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = sPLot;

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CHILDGRSEQ", "INDATA", "OUTDATA", inTable);

                if (searchResult != null && searchResult.Rows.Count > 0)
                    sRet = Util.NVC(searchResult.Rows[0]["CHILD_GR_SEQNO"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private DataTable GetChildEqpQty(string sPR_Lot, string sLot)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = sPR_Lot;
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPTQTY_NT_L", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness            
                newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE

                newRow["NT_WAIT_YN"] = "N"; // 대기 팬케익 재발행 여부.

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        sOutLBCD = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq, string sLBCD)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = sLBCD;
                newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["PRT_ITEM03"] = "NOTCHED PANCAKE";
                //newRow["PRT_ITEM04"] = "";
                //newRow["PRT_ITEM05"] = "";
                //newRow["PRT_ITEM06"] = "";
                //newRow["PRT_ITEM07"] = "";
                //newRow["PRT_ITEM08"] = "";
                //newRow["PRT_ITEM09"] = "";
                //newRow["PRT_ITEM10"] = "";
                //newRow["PRT_ITEM11"] = "";
                //newRow["PRT_ITEM12"] = "";
                //newRow["PRT_ITEM13"] = "";
                //newRow["PRT_ITEM14"] = "";
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void AutoPrint(bool bQASample)
        {
            try
            {
                ShowLoadingIndicator();
                // ZPL 파일 저장 여부 
                string _EQPT_CELL_PRINT_FLAG = string.Empty;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    DataTable inTable = _Biz.GetDA_PRD_SEL_PRINT_YN();

                    DataRow newRow = inTable.NewRow();

                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")));

                    inTable.Rows.Add(newRow);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_PRT_CHK", "INDATA", "OUTDATA", inTable);


                    //  파일 저장 분기 처리

                    DataTable inTable2 = new DataTable();
                    inTable2.Columns.Add("EQPTID", typeof(string));

                    DataRow newRow2 = inTable2.NewRow();
                    newRow2["EQPTID"] = cboEquipment.SelectedValue;

                    inTable2.Rows.Add(newRow2);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR_CELL_ID_PRT_FLAG", "INDATA", "OUTDATA", inTable2);

                    if (dtResult?.Rows?.Count > 0)
                        _EQPT_CELL_PRINT_FLAG = dtResult.Rows[0]["CELL_ID_PRT_FLAG"].ToString();


                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {

                        if (!Util.NVC(dtRslt.Rows[0]["PROC_LABEL_PRT_FLAG"]).Equals("Y"))
                        {
                            // 프린터 정보 조회
                            string sPrt = string.Empty;
                            string sRes = string.Empty;
                            string sCopy = string.Empty;
                            string sXpos = string.Empty;
                            string sYpos = string.Empty;
                            string sDark = string.Empty;
                            string sLBCD = string.Empty;    // 리턴 라벨 타입 코드
                            string sEqpt = string.Empty;
                            DataRow drPrtInfo = null;

                            // 2017-07-04 Lee. D. R
                            // Line별 라벨 독립 발행 기능
                            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                            {
                                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                                return;
                            }
                            else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                            {
                                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                    return;
                            }
                            else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                            {
                                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                                {
                                    if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(cboEquipment.SelectedValue.ToString()))
                                    {
                                        sPrt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                        sRes = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                        sCopy = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                        sXpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                        sYpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                        sDark = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                        sEqpt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                                        drPrtInfo = dr;
                                    }
                                }

                                if (sEqpt.Equals(""))
                                {
                                    Util.MessageValidation("SFU3615"); //프린터 환경설정에 설비 정보를 확인하세요.
                                    return;
                                }
                            }

                            if (bQASample)
                                sCopy = Convert.ToString(Convert.ToInt32(sCopy) + 1);

                            string sZPL = GetPrintInfo(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);
                            string sLot = Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));


                            if (sZPL.Equals(""))
                            {
                                Util.MessageValidation("SFU1498");
                                return;
                            }

                            if (_EQPT_CELL_PRINT_FLAG.Equals("Y"))
                            {
                                Util.SendZplBarcode(sLot, sZPL);
                            }
                            else
                            {
                                if (sZPL.StartsWith("0,"))  // ZPL 정상 코드 확인.
                                {
                                    if (PrintLabel(sZPL.Substring(2), drPrtInfo))
                                        SetLabelPrtHist(sZPL.Substring(2), drPrtInfo, Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "WIPSEQ")), sLBCD);
                                }
                                else
                                {
                                    Util.MessageValidation(sZPL.Substring(2));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetMinChildGrSeq(out string sLot, out int iMinChildSeq)
        {
            try
            {
                sLot = "";
                iMinChildSeq = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MIN_CHILD_GR_SEQ_NT();

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.NOTCHING;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIN_CHILD_GR_SEQ_BY_PROD_LOT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    iMinChildSeq = Util.NVC(dtRslt.Rows[0]["MIN_CHILD_GR_SEQNO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["MIN_CHILD_GR_SEQNO"]));
                }
            }
            catch (Exception ex)
            {
                sLot = "";
                iMinChildSeq = 0;
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetMaxChildGrSeq(out string sLot, out int iMaxChildSeq)
        {
            try
            {
                sLot = "";
                iMaxChildSeq = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MIN_CHILD_GR_SEQ_NT();

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.NOTCHING;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CHILD_GR_SEQ_BY_PROD_LOT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sLot = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    iMaxChildSeq = Util.NVC(dtRslt.Rows[0]["MAX_CHILD_GR_SEQNO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["MAX_CHILD_GR_SEQNO"]));
                }
            }
            catch (Exception ex)
            {
                sLot = "";
                iMaxChildSeq = 0;
                Util.MessageException(ex);
            }
            finally
            {
                HideLoadingIndicator();
            }
        }
        
        private bool CheckSelWOInfo()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SET_WORKORDER_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_WORKORDER_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals(""))
                    {
                        bRet = false;
                        //선택된 W/O가 없습니다.
                        Util.MessageValidation("SFU1635");
                    }
                    else if (Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK"]).Equals("N"))
                    {
                        bRet = false;
                        //선택된 W/O가 없습니다.
                        Util.MessageValidation("SFU1635");
                    }
                    else
                    {
                        bRet = true;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private bool CheckModelChange()
        {
            bool bRet = true;

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LAST_EQP_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("PRJT_NAME"))
                    {
                        string productProjectName = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRJT_NAME"));
                        string serchProjectName = Util.NVC(dtRslt.Rows[0]["PRJT_NAME"]);
                        if (serchProjectName.Length > 1 && serchProjectName.Equals(productProjectName))
                            bRet = false;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private bool CheckInputEqptCond()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_EQPT_CLCT_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetWrokCalander()
        {
            try
            {
                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;                
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.NOTCHING;
                
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORK_CALENDAR_WRKR_INFO", "RQSTDT", "RSLTDT", IndataTable);
                
                if (result.Rows.Count > 0)
                {
                    txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                    txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                    txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                    txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                    txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                    txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                    txtShiftDateTime.Text = Util.NVC(result.Rows[0]["WRK_DTTM_FR_TO"]);

                    #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
                    if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
                        btnShift.Visibility = Visibility.Collapsed;
                    #endregion
                }
                else
                {
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    txtShiftDateTime.Text = string.Empty;

                    #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
                    if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
                    {
                        btnShift.Visibility = Visibility.Visible;
                        GetEqptWrkInfo();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                //HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private bool SetTestMode(bool bTestMode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_EQP_REG_TESTMODE();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = bTestMode ? "TEST" : "ON";
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_EQP_REG_EQPT_OPMODE", "IN_EQP", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        HideTestMode();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HideLoadingIndicator();
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void RunCompleteCancel()
        {
            try
            {
                ShowLoadingIndicator();

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "PR_LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_EQPT_END_LOT_NT_L", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 2019.02.20 오화백 RF_ID 투입부, 배출부 여부 
        /// </summary>
        private void GetLotIdentBasCode()
        {
            try
            {
                _LDR_LOT_IDENT_BAS_CODE = "";
                _UNLDR_LOT_IDENT_BAS_CODE = "";

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.NOTCHING;
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("LDR_LOT_IDENT_BAS_CODE"))
                        _LDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"]);

                    if (dtRslt.Columns.Contains("UNLDR_LOT_IDENT_BAS_CODE"))
                        _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HideLoadingIndicator();
            }
        }

        private bool ChkEqptCnfmType(string sBtnGrpID)
        {
            try
            {
                bool bRet = false;

                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return bRet;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);

                HideLoadingIndicator();

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("RSLT_CNFM_TYPE") && Util.NVC(dtRslt.Rows[0]["RSLT_CNFM_TYPE"]).Equals("A"))
                    {
                        if (!CheckButtonPermissionGroupByBtnGroupID(sBtnGrpID))  // 버튼 권한 체크.
                        {
                            //해당 설비는 자동실적확정 설비 입니다.
                            Util.MessageValidation("SFU6034");
                            return bRet;
                        }
                    }
                    else
                    {
                        if (!CheckButtonPermissionGroupByBtnGroupID(sBtnGrpID))  // 버튼 권한 체크.
                        {
                            Util.MessageValidation("SFU3520", LoginInfo.USERID, sBtnGrpID.Equals("EQPT_END_W") ? ObjectDic.Instance.GetObjectName("장비완료") : ObjectDic.Instance.GetObjectName("실적확정")); // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                            return bRet;
                        }
                    }
                }

                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HideLoadingIndicator();
                return false;
            }
        }

        private void GetButtonPermissionGroup()
        {
            try
            {
                InitializeButtonPermissionGroup();

                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = this.GetType().Name;    // 화면ID

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("BTN_PMS_GRP_CODE"))
                    {
                        foreach (DataRow drTmp in dtRslt.Rows)
                        {
                            if (drTmp == null) continue;

                            switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                            {
                                case "INPUT_W": // 투입 사용 권한
                                case "WAIT_W": // 대기 사용 권한
                                case "INPUTHIST_W": // 투입이력 사용 권한
                                    //SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                    break;
                                case "OUTPUT_W": // 생산반제품 사용 권한
                                    //grdOutTranBtn.Visibility = Visibility.Visible;
                                    break;
                                case "LOTSTART_W": // 작업시작 사용 권한
                                    btnRunStart.Visibility = Visibility.Visible;
                                    break;
                                case "CANCEL_LOTSTART_W": // 작업시작 취소 권한
                                    btnRunCancel.Visibility = Visibility.Visible;
                                    break;
                                case "CANCEL_EQPT_END_W": // 장비완료 취소 권한
                                    btnRunCompleteCancel.Visibility = Visibility.Visible;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HideLoadingIndicator();
            }
        }

        private bool CheckProdQtyChgPermission()
        {
            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = this.GetType().Name;    // 화면ID

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("BTN_PMS_GRP_CODE"))
                    {
                        foreach (DataRow drTmp in dtRslt.Rows)
                        {
                            if (drTmp == null) continue;

                            switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                            {
                                case "PROD_QTY_CHG_W": // 수량 변경 권한
                                    bRet = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckCanConfirm()
        {
            try
            {
                bool bRet = false;

                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return bRet;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID")).Equals(""))
                {
                    //Util.Alert("실적확인할 수 있는 상태가 아닙니다.");
                    Util.MessageValidation("SFU1714");
                    return bRet;
                }
                
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.GetCondition(cboEquipment);
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_AUTO_CONFIRM_LOT", "INDATA", "OUTDATA", inDataTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["RESULT_VALUE"]).Equals("OK"))
                    {
                        bRet = true;
                    }
                    else
                    {
                        Util.MessageValidation(Util.NVC(dtRslt.Rows[0]["RESULT_CODE"]));
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return false;
            }
        }

        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID)
        {
            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = this.GetType().Name;    // 화면ID

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataTable inOutput = indataSet.Tables.Add("IN_OUTPUT");
                inOutput.Columns.Add("OUTPUT_LOTID", typeof(string));
                inOutput.Columns.Add("OUTPUT_CSTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();                
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                inDataTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "PR_LOTID"));

                inInput.Rows.Add(newRow);

                newRow = inOutput.NewRow();

                newRow["OUTPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["OUTPUT_CSTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "CSTID"));

                inOutput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_NT_CSTID", "IN_EQP,IN_INPUT,IN_OUTPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetWipStat()
        {
            try
            {
                string sRet = string.Empty;

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0)
                {
                    return sRet;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", inTable);

                HideLoadingIndicator();

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                    if (dtRslt.Columns.Contains("WIPSTAT"))
                        sRet = Util.NVC(dtRslt.Rows[0]["WIPSTAT"]);
                
                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HideLoadingIndicator();
                return "";
            }
        }

        private string GetBasicPolarity()
        {
            try
            {
                string sRet = "";
                
                if (Util.NVC(LoginInfo.CFG_EQPT_ID).Equals("") || Util.NVC(LoginInfo.CFG_PROC_ID) != Process.NOTCHING)
                {
                    return sRet;
                }
                
                DataTable inTable = new DataTable();

                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = Util.NVC(LoginInfo.CFG_EQPT_ID);

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VWEQUIPMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("ELTR_TYPE_CODE"))
                    sRet = Util.NVC(dtRslt.Rows[0]["ELTR_TYPE_CODE"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        #endregion

        #region [Validation]

        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            if (bLoaded == false)
            {
                if (!(bool)chkRun.IsChecked && !(bool)chkEqpEnd.IsChecked && !(bool)chkWait.IsChecked)
                {
                    //Util.Alert("LOT 상태 선택 조건을 하나 이상 선택하세요.");
                    Util.MessageValidation("SFU1370");
                    return bRet;
                }
            }

            if (cboElecType.SelectedIndex < 0)
            {
                //Util.Alert("극성을 선택 하세요.");
                Util.MessageValidation("SFU1467");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanStartRun()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
            {
                Util.MessageValidation("SFU1492");
                return bRet;
            }

            if (cboMountPstsID.SelectedValue == null || cboMountPstsID.SelectedValue.ToString().Equals(""))
            {
                //Util.Alert("반제품 투입위치 기준정보가 없습니다.");
                Util.MessageValidation("SFU1543");
                return bRet;
            }
            
            if (!CheckSelWOInfo())
            {
                //Util.Alert("선택된 W/O가 없습니다.");
                return bRet;
            }

            #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
            //if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
            //{
            //    // WorkCalander 체크.
            //    GetWrokCalander();

            //    if (txtWorker.Text.Trim().Equals(""))
            //    {
            //        Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
            //        return bRet;
            //    }

            //    if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            //    {
            //        DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
            //        DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
            //        DateTime systemDateTime = GetSystemTime();

            //        int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
            //        int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

            //        if (prevCheck < 0 || nextCheck > 0)
            //        {
            //            Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
            //            txtWorker.Text = string.Empty;
            //            txtWorker.Tag = string.Empty;
            //            txtShift.Text = string.Empty;
            //            txtShift.Tag = string.Empty;
            //            txtShiftStartTime.Text = string.Empty;
            //            txtShiftEndTime.Text = string.Empty;
            //            txtShiftDateTime.Text = string.Empty;
            //            return bRet;
            //        }
            //    }
            //}
            #endregion

            bRet = true;

            return bRet;
        }
        
        private bool CanRunComplete()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            string sWipStat = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

            if (!sWipStat.Equals("PROC"))
            {
                Util.MessageValidation("SFU1866");
                return bRet;
            }

            #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
            if (_SELECT_USER_MODE_AREA.Contains(LoginInfo.CFG_AREA_ID))
            {
                // WorkCalander 체크.
                GetWrokCalander();

                if (txtWorker.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
                    return bRet;
                }

                if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
                {
                    DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                    DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                    DateTime systemDateTime = GetSystemTime();

                    int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                    int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                    if (prevCheck < 0 || nextCheck > 0)
                    {
                        Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
                        txtWorker.Text = string.Empty;
                        txtWorker.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                        txtShift.Tag = string.Empty;
                        txtShiftStartTime.Text = string.Empty;
                        txtShiftEndTime.Text = string.Empty;
                        txtShiftDateTime.Text = string.Empty;
                        return bRet;
                    }
                }
            }
            #endregion

            bRet = true;

            return bRet;
        }
        
        private bool CanPrintPopup()
        {
            bool bRet = false;
            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                //Util.Alert("프린트 환경 설정값이 없습니다.");
                Util.MessageValidation("SFU2003");
                return bRet;
            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageInfo("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        
        private bool CanEqptCondInfo()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageInfo("SFU1673");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIP_TYPE_CODE")).Equals("OUT"))
            {
                //Util.Alert("{0} LOT은 공정조건을 입력할 수 있는 상태가 아닙니다.", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID")));
                Util.MessageValidation("SFU3086", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID")));
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanTestMode()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanCompleteCancelRun()
        {
            bool bRet = false;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                Util.MessageValidation("SFU1864");  // 장비완료 상태의 LOT이 아닙니다.
                return bRet;
            }

            string sPRLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int iCut = 0;

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            // 최종 작업 lot 부터 취소 가능.
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(sPRLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) > iCut)
                        {
                            //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            // Max CUT DB 확인.
            string sTmp = "";
            int iTmpMinSeq = 0;

            sTmp = GetMaxChildGRPSeq(sPRLot);

            int.TryParse(sTmp, out iTmpMinSeq);

            if (iTmpMinSeq > 0 && iTmpMinSeq > iCut)
            {
                //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.");
                Util.MessageValidation("SFU1790");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanCancelRun()
        {
            bool bRet = false;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return bRet;
            }

            string sPRLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int iCut = 0;

            if (!int.TryParse(sChildSeq, out iCut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            // 최종 작업 lot 부터 취소 가능.
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(sPRLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) > iCut)
                        {
                            //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            // Max CUT DB 확인.
            string sTmp = "";
            int iTmpMinSeq = 0;

            sTmp = GetMaxChildGRPSeq(sPRLot);

            int.TryParse(sTmp, out iTmpMinSeq);

            if (iTmpMinSeq > 0 && iTmpMinSeq > iCut)
            {
                //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.");
                Util.MessageValidation("SFU1790");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        
        #endregion

        #region [PopUp Event]

        /// <summary>
        /// 2019-02-25 RF_ID CST 이력조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndHist_Closed(object sender, EventArgs e)
        {
            CMM_CST_HIST window = sender as CMM_CST_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndEqpend_Closed(object sender, EventArgs e)
        {
            ASSY004_001_EQPTEND window = sender as ASSY004_001_EQPTEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndLotCommentHist_Closed(object sender, EventArgs e)
        {
            ASSY004_001_LOTCOMMENTHIST window = sender as ASSY004_001_LOTCOMMENTHIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        
        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            ASSY004_001_RUNSTART window = sender as ASSY004_001_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }
                
        private void wndPrint_Closed(object sender, EventArgs e)
        {
            ASSY004_001_HIST window = sender as ASSY004_001_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND window = sender as CMM_ASSY_EQPT_COND;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        
        private void wndWait_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_WAITLOT window = sender as ASSY004_COM_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            ASSY004_001_CONFIRM window = sender as ASSY004_001_CONFIRM;

            try
            {
                if (window.DialogResult == MessageBoxResult.OK && bWndConfirmJobEnd == true)
                {
                    // 발행 여부 체크 및 미발행 시 자동 발행 처리
                    AutoPrint(window.CHECKED);

                    GetWorkOrder(); // 작지 생산수량 정보 재조회.
                    GetProductLot();                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                bWndConfirmJobEnd = true;
            }
        }
        
        private void wndEqptCondSearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND_SEARCH window = sender as CMM_ASSY_EQPT_COND_SEARCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndQualityInput_New_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP window = sender as CMM_COM_SELF_INSP;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }
        
        private void wndEqptDfctCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO window = sender as CMM_ASSY_EQPT_DFCT_CELL_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndWipNote_Closed(object sender, EventArgs e)
        {
            CMM_COM_WIP_NOTE window = sender as CMM_COM_WIP_NOTE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndDfctCellReg_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DFCT_CELL_REG window = sender as CMM_ASSY_DFCT_CELL_REG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //GetEqpFaultyData();
            }
        }

        private void wndCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CELL_INFO window = sender as CMM_ASSY_CELL_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndAutoHoldHist_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_CHG_HOLD_RESN window = sender as ASSY004_COM_CHG_HOLD_RESN;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }
        
        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnQualityInput);
            listAuth.Add(btnEqptIssue);
            listAuth.Add(btnRunComplete);
            listAuth.Add(btnConfirm);
            listAuth.Add(btnPrint);
            //listAuth.Add(btnWaitLot);
            listAuth.Add(btnRemarkHist);
            listAuth.Add(btnEqptCondSearch);
            listAuth.Add(btnEqptCond);
            listAuth.Add(btnWipNote);
            listAuth.Add(btnRunStart);

            listAuth.Add(btnSaveFaulty);
            listAuth.Add(btnSaveRemark);
            listAuth.Add(btnSearchEqpFaulty);

            listAuth.Add(btnRunCancel);
            listAuth.Add(btnRunCompleteCancel);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                winWorkOrder.FrameOperation = FrameOperation;

                winWorkOrder._UCParent = this;
                grdWorkOrder.Children.Add(winWorkOrder);
            }
        }

        private void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.NOTCHING;

            winWorkOrder.GetWorkOrder();
        }

        //2019.11.01 김대근 추가
        private void SetInputHistToolWindow()
        {
            if(grdInputHistTool.Children.Count == 0)
            {
                winInputHistTool.FrameOperation = FrameOperation;

                winInputHistTool._UCParent = this;
                grdInputHistTool.Children.Add(winInputHistTool);
            }
        }

        private void GetInputHistTool()
        {
            if (winInputHistTool == null)
                return;

            winInputHistTool.EQPTID = cboEquipment.SelectedValue.ToString();

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            string prodLotID = string.Empty;
            if (idx > -1)
                prodLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "PR_LOTID"));

            winInputHistTool.PROD_LOTID = prodLotID;
            winInputHistTool.GetInputHistTool();
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.NOTCHING;
                sEqsg = cboEquipmentSegment.SelectedIndex >= 0 ? cboEquipmentSegment.SelectedValue.ToString() : "";
                sEqpt = cboEquipment.SelectedIndex >= 0 ? cboEquipment.SelectedValue.ToString() : "";

                return true;
            }
            catch (Exception ex)
            {
                sProc = "";
                sEqsg = "";
                sEqpt = "";
                return false;
                throw ex;
            }

        }

        /// <summary>
        /// Main Windows Data Clear 처리
        /// UC_WORKORDER 컨트롤에서 Main Window Data Clear
        /// </summary>
        /// <returns>Clear 완료 여부</returns>
        public bool ClearControls()
        {
            bool bRet = false;

            try
            {
                Util.gridClear(dgProductLot);
                ClearDetailData();

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private void Confirm_Process()
        {
            if (CheckModelChange() && !CheckInputEqptCond())
            {
                //해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?
                Util.MessageConfirm("SFU2817", (result2) =>
                {
                    if (result2 == MessageBoxResult.OK)
                    {
                        ConfirmProcess();
                    }
                });
            }
            else
            {
                ConfirmProcess();
            }
        }

        private bool SetCheckProdListSameChildSeq(C1.WPF.DataGrid.DataGridRow dataitem, bool bUncheckAll = false)
        {
            if (dataitem == null || dataitem.Index < 0 || dataitem.DataItem == null)
                return false;

            string sInputLot = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "PR_LOTID"));
            string sChildSeq = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "CHILD_GR_SEQNO"));
            string sChildLot = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "LOTID"));

            // 모두 Uncheck 처리 및 동일 자랏의 경우는 Check 처리.
            for (int i = 0; i < dgProductLot.Rows.Count; i++)
            {
                if (dataitem.Index != i)
                {
                    if (sInputLot == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "PR_LOTID")) &&
                        sChildSeq == Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")) &&
                        !Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")).Equals(""))
                    {
                        if (sChildLot.Equals(""))
                        {
                            if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                            {
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                            }
                            DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                        }
                        else
                        {
                            if (bUncheckAll)
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                {
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                }
                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                                dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                                (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                {
                                    (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                                }
                                DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", true);
                            }
                        }

                    }
                    else
                    {
                        if (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter != null &&
                            dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content != null &&
                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                        {
                            (dgProductLot.GetCell(i, dgProductLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                        }
                        DataTableConverter.SetValue(dgProductLot.Rows[i].DataItem, "CHK", false);
                    }
                }
            }

            return true;
        }

        private void ProcessDetail(object obj)
        {
            DataRowView rowview = (obj as DataRowView);

            if (rowview == null)
            {
                //Util.Alert("LOT 상세정보가 잘못 되어 있습니다.");
                Util.MessageValidation("SFU1215");
                return;
            }

            _CurrentLotInfo.LOTID = Util.NVC(rowview["LOTID"]);
            _CurrentLotInfo.PR_LOTID = Util.NVC(rowview["PR_LOTID"]);
            _CurrentLotInfo.WIPSEQ = Util.NVC(rowview["WIPSEQ"]);
            _CurrentLotInfo.INPUTQTY = Util.NVC(rowview["WIPQTY"]);
            _CurrentLotInfo.WORKORDER = Util.NVC(rowview["WOID"]); // 장비완료 process 추가로 수정.
            _CurrentLotInfo.STATUS = Util.NVC(rowview["WIPSTAT"]);
            _CurrentLotInfo.STATUSNAME = Util.NVC(rowview["WIPSNAME"]);
            _CurrentLotInfo.WORKDATE = Util.NVC(rowview["CALDATE_LOT"]);
            _CurrentLotInfo.STARTTIME_CHAR = Util.NVC(rowview["WIPDTTM_ST"]);
            //_CurrentLotInfo.REMARK = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, ""));
            _CurrentLotInfo.PRODID = Util.NVC(rowview["PRODID"]);
            _CurrentLotInfo.VERSION = Util.NVC(rowview["PROD_VER_CODE"]);
            _CurrentLotInfo.ENDTIME_CHAR = Util.NVC(rowview["EQPT_END_DTTM"]);
            _CurrentLotInfo.CHILD_GR_SEQNO = Util.NVC(rowview["CHILD_GR_SEQNO"]);
            _CurrentLotInfo.WIP_TYPE_CODE = Util.NVC(rowview["WIP_TYPE_CODE"]);

            _CurrentLotInfo.STARTTIME_CHAR_ORG = Util.NVC(rowview["WIPDTTM_ST_ORG"]);
            _CurrentLotInfo.ENDTIME_CHAR_ORG = Util.NVC(rowview["EQPT_END_DTTM_ORG"]);


            // Caldate Lot의 Caldate로...
            if (Util.NVC(rowview["CALDATE_LOT"]).Trim().Equals(""))
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"])).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"]));

                sCaldate = Util.NVC(rowview["NOW_CALDATE_YMD"]);
                dtCaldate = Convert.ToDateTime(Util.NVC(rowview["NOW_CALDATE"]));
            }
            else
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"])).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"]));

                sCaldate = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"])).ToString("yyyyMMdd");
                dtCaldate = Convert.ToDateTime(Util.NVC(rowview["CALDATE_LOT"]));
            }
            
            string sLot = "";
            if (!_CurrentLotInfo.STATUS.Equals("WAIT"))
            {
                if (_CurrentLotInfo.STATUS.Equals("EQPT_END"))
                {
                    FillLotInfo();
                    sLot = Util.NVC(rowview["LOTID"]);                    
                }
                else if (_CurrentLotInfo.STATUS.Equals("PROC") && _CurrentLotInfo.WIP_TYPE_CODE.Equals("OUT"))
                {
                    //FillLotInfo(); // 장비완료 process 추가로 수정.
                    sLot = Util.NVC(rowview["LOTID"]);
                }
                else
                {
                    sLot = Util.NVC(rowview["PR_LOTID"]);
                }

                GetDetailData();

                GetFaultyData();

                SetLotInfoCalc();

                if (_CurrentLotInfo.STATUS.Equals("EQPT_END"))// || (_CurrentLotInfo.STATUS.Equals("PROC") && _CurrentLotInfo.WIP_TYPE_CODE.Equals("OUT"))) // 장비완료 process 추가로 수정.
                {
                    SetParentQty();
                }
            }
        }

        private void FillLotInfo()
        {
            /******************** 상세 정보 Set... ******************/
            txtWorkorder.Text = _CurrentLotInfo.WORKORDER;
            txtLotStatus.Text = _CurrentLotInfo.STATUSNAME;
            txtStartTime.Text = _CurrentLotInfo.STARTTIME_CHAR;
            txtEndTime.Text = _CurrentLotInfo.ENDTIME_CHAR;

            if (!_CurrentLotInfo.STARTTIME_CHAR_ORG.Equals("") && !_CurrentLotInfo.ENDTIME_CHAR_ORG.Equals(""))
            {
                DateTime dTmpEnd;
                DateTime dTmpStart;

                if (DateTime.TryParse(_CurrentLotInfo.ENDTIME_CHAR_ORG, out dTmpEnd) && DateTime.TryParse(_CurrentLotInfo.STARTTIME_CHAR_ORG, out dTmpStart))
                    txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString();
            }
        }

        private void ClearLotInfo()
        {
            sCaldate = "";

            //txtParentQty.Text = "";
            txtParent2.Text = "";

            //txtParentQty_M.Text = "";
            txtParent2_M.Text = "";

            txtWorkorder.Text = "";
            txtLotStatus.Text = "";
            txtStartTime.Text = "";
            txtEndTime.Text = "";
            txtWorkMinute.Text = "";
        }

        private void SetLotInfoCalc()
        {
            //if (dgDetail.ItemsSource == null)
            //    return;

            //double inputQty = 0;
            //double goodQty = 0;
            //double lossQty = 0;

            //for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
            //{
            //    inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
            //    lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")).Equals("") ? "0" : Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));
            //    goodQty = inputQty - lossQty;

            //    DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", goodQty);
            //}
        }

        private double SumDefectQty(C1.WPF.DataGrid.C1DataGrid dg, string sColName, out double dfct, out double loss, out double charge_prd)
        {
            double sum = 0;
            dfct = 0;
            loss = 0;
            charge_prd = 0;

            if (!dg.Columns.Contains(sColName))
                return sum;

            for (int i = 0; i < dg.Rows.Count - dg.Rows.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")).Equals("N"))  // 실적 제외 여부 확인.
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("DEFECT_LOT"))
                        dfct += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                    else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("LOSS_LOT"))
                        loss += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                    else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                        charge_prd += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());

                    sum += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                }
            }
            return sum;
        }

        private void BindingDataGrid(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dtRslt, string sColName)
        {
            DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);

            if (!dt.Columns.Contains(sColName))
            {
                dt.Columns.Add(sColName, typeof(int));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][sColName] = 0;
                }
            }

            if (dtRslt != null)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                    {
                        if (dtRslt.Rows[j]["ACTID"].Equals(dt.Rows[k]["ACTID"]) &&
                            dtRslt.Rows[j]["RESNCODE"].Equals(dt.Rows[k]["RESNCODE"]))
                        {
                            dt.Rows[k][sColName] = dtRslt.Rows[j]["RESNQTY"];

                            if (dt.Columns.Contains("RESNNOTE") && dtRslt.Columns.Contains("RESNNOTE"))
                            {
                                dt.Rows[k]["RESNNOTE"] = dtRslt.Rows[j]["RESNNOTE"];
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][sColName] = 0;
                }
            }

            dataGrid.BeginEdit();
            //dataGrid.ItemsSource = DataTableConverter.Convert(dt);
            Util.GridSetData(dataGrid, dt, FrameOperation, false);

            C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dataGrid.Columns[sColName]
                , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

            dataGrid.EndEdit();

        }

        private void ClearDetailData()
        {
            Util.gridClear(dgDetail);

            InitFaultyDataGrid(bClearAll: true);

            Util.gridClear(dgEqpFaulty); 

            ClearLotInfo();

            rtxRemark.Document.Blocks.Clear();

            cboRemarkLot.ItemsSource = null;
            cboRemarkLot.Text = "";

            cboEqptDfctLot.ItemsSource = null;
            cboEqptDfctLot.Text = "";
        }

        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            ClearLotInfo();

            if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", iRow))
            {
                return;
            }

            ProcessDetail(dgProductLot.Rows[iRow].DataItem);
        }

        private void SetRemarkCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    dt.Rows.Add(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"), DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                }
                cboRemarkLot.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboRemarkLot.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqptDfctCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
                {
                    dt.Rows.Add(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"), DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "LOTID"));
                }
                cboEqptDfctLot.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboEqptDfctLot.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetInputQty()
        {
            //if (!Util.CheckDecimal(txtInputQty.Text, 0))
            //{
            //    txtInputQty.Text = "";
            //    return;
            //}

            //if (dgDetail.Rows.Count - dgDetail.TopRows.Count < 1)
            //    return;


            //// 모랏 재공수량과 자랏의 생산수량 정보 비교.
            //if (!txtParentQty.Text.Equals("") && !txtParent2.Text.Equals(""))
            //{
            //    double iInputQty = double.Parse(txtInputQty.Text);
            //    double iParentWipQty = double.Parse(txtParentQty.Text);

            //    if (iInputQty > iParentWipQty)
            //    {
            //        // 생산량 입력 후 엔터 시 처리 방법.
            //        // 1. 마지막 완성LOT : 차이수량 만큼 길이초과 처리.
            //        // 2. 마지막 완성LOT 아니면 : 생산량이 투입량을 초과할 수 없다.

            //        // 1. 마지막 완성 LOT 체크
            //        int iCut = 0;
            //        int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            //        if (idx < 0)
            //            return;

            //        bool bFndAnotherChild = false;
            //        string sChildSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "CHILD_GR_SEQNO"));

            //        if (!int.TryParse(sChildSeq, out iCut))
            //        {
            //            //Util.Alert("CUT이 숫자가 아닙니다.");
            //            //return bRet;
            //        }

            //        string sTmpLot = "";
            //        int iTmpMaxSeq = 0;


            //        GetMaxChildGrSeq(out sTmpLot, out iTmpMaxSeq);

            //        if (iTmpMaxSeq > 0 && iTmpMaxSeq > iCut)
            //        {
            //            bFndAnotherChild = true;
            //        }

            //        // 2. 마지막 완성랏이 아닌 경우 : 생산량이 투입량을 초과할 수 없습니다.
            //        if (bFndAnotherChild)
            //        {
            //            //Util.Alert("생산량이 투입량을 초과할 수 없습니다.");
            //            Util.MessageValidation("SFU1614");
            //            txtInputQty.Text = "";
            //            return;
            //        }
            //        else // 3. 마지막 완성랏인 경우 : 차이수량 길이초과 자동 등록
            //        {
            //            Util.MessageConfirm("SFU1921", (result) =>
            //            {
            //                if (result == MessageBoxResult.OK)
            //                {
            //                    SaveLoss(iInputQty - iParentWipQty, "PLUS", true);

            //                    double inputQty = double.Parse(txtInputQty.Text);
            //                    //double inputQty = 0;
            //                    double lossQty = 0;

            //                    for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
            //                    {
            //                        //inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
            //                        lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

            //                        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "INPUTQTY", inputQty);
            //                        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
            //                    }

            //                    txtInputQty.Text = "";

            //                    SetParentRemainQty();
            //                }
            //                else
            //                {
            //                    txtInputQty.Text = "";
            //                    return;
            //                }
            //            }, (iInputQty - iParentWipQty).ToString(CultureInfo.InvariantCulture));

            //        }
            //    }
            //    else
            //    {
            //        double inputQty = double.Parse(txtInputQty.Text);
            //        //double inputQty = 0;
            //        double lossQty = 0;

            //        for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
            //        {
            //            //inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
            //            lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

            //            DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "INPUTQTY", inputQty);
            //            DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
            //        }

            //        txtInputQty.Text = "";

            //        SetParentRemainQty();
            //    }
            //}
            //else
            //{
            //    double inputQty = double.Parse(txtInputQty.Text);
            //    //double inputQty = 0;
            //    double lossQty = 0;

            //    for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count; i++)
            //    {
            //        //inputQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "INPUTQTY")));
            //        lossQty = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[i].DataItem, "DFCT_SUM")));

            //        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "INPUTQTY", inputQty);
            //        DataTableConverter.SetValue(dgDetail.Rows[i].DataItem, "GOODQTY", inputQty - lossQty);
            //    }

            //    txtInputQty.Text = "";

            //    SetParentRemainQty();
            //}
        }

        private void SetParentRemainQty(string sParentQtyEA, string sParentQtyM)
        {
            try
            {
                if (dgDetail.Rows.Count - dgDetail.TopRows.Count > 0)
                {
                    if (double.Parse(sParentQtyEA) - double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.TopRows.Count].DataItem, "INPUTQTY"))) < 0)
                    {
                        txtParent2.Text = "0";
                        txtParent2_M.Text = "0";
                    }
                    else
                    {
                        // 반올림 처리.
                        txtParent2.Text = Math.Round((double.Parse(sParentQtyEA) - double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.TopRows.Count].DataItem, "INPUTQTY"))))).ToString("#,##0");

                        // 잔량 Meter 로 계산 필요.... 컬럼 없음.
                        int iPRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                        if (iPRow >= 0)
                        {
                            string sTmp = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iPRow].DataItem, "PTN_LEN"));
                            if (sTmp.Equals(""))
                            {
                                txtParent2_M.Text = txtParent2.Text;
                            }
                            else
                            {
                                double dTmp = 0;
                                double.TryParse(sTmp, out dTmp);
                                double dInputEa = double.Parse(Util.NVC(DataTableConverter.GetValue(dgDetail.Rows[dgDetail.TopRows.Count].DataItem, "INPUTQTY")));
                                // EA = 재공(M) / 패턴길이
                                // M = 재공(EA) * 패턴길이
                                txtParent2_M.Text = Math.Round((double.Parse(sParentQtyM) - (dInputEa * dTmp))).ToString("#,##0");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SetParentQty()
        {
            DataTable dt = GetChildEqpQty(_CurrentLotInfo.PR_LOTID, _CurrentLotInfo.LOTID);

            if (dt != null && dt.Rows.Count > 0)
            {
                if (Util.NVC(dt.Rows[0]["EQPT_UNMNT_TYPE_CODE"]).Equals("R"))
                {
                    tbParentQtyInfo.Visibility = Visibility.Visible;
                    txtParent2_M.Visibility = Visibility.Visible;
                    txtParent2.Visibility = Visibility.Visible;
                }
                else
                {
                    tbParentQtyInfo.Visibility = Visibility.Collapsed;
                    txtParent2_M.Visibility = Visibility.Collapsed;
                    txtParent2.Visibility = Visibility.Collapsed;

                    return;
                }

                string dTmp = "0";
                string dTmp2 = "0";

                dTmp = Util.NVC(dt.Rows[0]["WIPQTY_EA"]).Equals("") ? "0" : double.Parse(Util.NVC(dt.Rows[0]["WIPQTY_EA"])).ToString("#,##0");
                dTmp2 = Util.NVC(dt.Rows[0]["WIPQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(dt.Rows[0]["WIPQTY"])).ToString("#,##0");

                //txtParentQty.Text = dTmp.ToString();
                //txtParentQty_M.Text = dTmp2.ToString();

                SetParentRemainQty(dTmp, dTmp2);
            }
        }

        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(sZPL, cboEquipment.SelectedValue.ToString());
                    }

                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void ConfirmProcess()
        {
            try
            {
                ASSY004_001_CONFIRM wndConfirm = new ASSY004_001_CONFIRM();

                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx < 0) return;

                    object[] Parameters = new object[14];
                    Parameters[0] = Process.NOTCHING;
                    Parameters[1] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[2] = cboEquipment.SelectedValue.ToString();
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
                    Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
                    Parameters[7] = cboMountPstsID != null && cboMountPstsID.SelectedValue != null ? cboMountPstsID.SelectedValue.ToString() : "";

                    Parameters[8] = Util.NVC(txtShift.Text);
                    Parameters[9] = Util.NVC(txtShift.Tag);
                    Parameters[10] = Util.NVC(txtWorker.Text);
                    Parameters[11] = Util.NVC(txtWorker.Tag);
                    Parameters[12] = CheckProdQtyChgPermission();
                    Parameters[13] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "QA_INSP_TRGT_FLAG")); 

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed -= new EventHandler(wndConfirm_Closed);
                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ColorAnimationInredRectangle(System.Windows.Shapes.Rectangle rect)
        {
            rect.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showTestAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInredRectangle(recTestMode);
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 2017.07.20  Lee. D. R 
        /// 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
        /// </summary>
        private void GetLossCnt()
        {
            try
            {
                DataTable dtEqpLossCnt = Util.Get_EqpLossCnt(cboEquipment.SelectedValue.ToString());

                if (dtEqpLossCnt != null && dtEqpLossCnt.Rows.Count > 0)
                {
                    txtLossCnt.Text = Util.NVC(dtEqpLossCnt.Rows[0]["CNT"]);

                    if (Util.NVC_Int(dtEqpLossCnt.Rows[0]["CNT"]) > 0)
                    {
                        txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.LightPink);
                    }
                    else
                    {
                        txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);
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

        private void btnPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPilotProdMode()) return;

            string sMsg = string.Empty;
            bool bMode = GetPilotProdMode();
            
            if (bMode)
                sMsg = "SFU2875";
            else
                sMsg = "SFU2875";

            Util.MessageConfirm(sMsg, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetPilotProdMode(!bMode);
                    GetPilotProdMode();

                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                }
            });
        }

        private bool CanPilotProdMode()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //string sProcLotID = string.Empty;
            //if (CheckProcWip(out sProcLotID))
            //{
            //    Util.MessageValidation("SFU3199", sProcLotID); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private bool CheckProcWip(out string sProcLotID)
        {
            sProcLotID = "";

            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                dtRow["WIPSTAT"] = Wip_State.PROC;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sProcLotID = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool GetPilotProdMode()
        {
            try
            {
                bool bRet = false;

                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || Util.NVC(cboEquipment.SelectedValue).Trim().Equals("SELECT"))
                {
                    HidePilotProdMode();
                    return bRet;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("PILOT_PROD_MODE"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["PILOT_PROD_MODE"]).Equals("Y"))
                    {
                        ShowPilotProdMode();
                        bRet = true;
                    }
                    else
                    {
                        HidePilotProdMode();
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool SetPilotProdMode(bool bMode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PILOT_PRDC_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PILOT_PRDC_MODE"] = bMode ? "PILOT" : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_PILOT_PRODUCTION_MODE", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void HidePilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void ShowPilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Visible;
                //txtPilotProdMode.Text = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
                ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void btnSecCutOff_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
            {
                //Util.MessageValidation(""); // 대기 Lot은 입력할 수 없습니다.
                return;
            }

            // Lot 상태 체크
            if (GetWipStat().Equals(Wip_State.END) || GetWipStat().Equals(""))
            {
                Util.MessageValidation("SFU2063"); // 재공상태를 확인해주세요.
                return;
            }


            ASSY004_001_EQPT_SECT_CUTOFF wndCutOff = new ASSY004_001_EQPT_SECT_CUTOFF();
            wndCutOff.FrameOperation = FrameOperation;

            if (wndCutOff != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipment.SelectedValue;
                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
                Parameters[3] = cboEquipmentSegment.SelectedValue;
                Parameters[4] = cboEquipmentSegment.Text;

                C1WindowExtension.SetParameters(wndCutOff, Parameters);

                wndCutOff.Closed += new EventHandler(wndCutOff_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndCutOff.ShowModal()));
            }
        }

        private void wndCutOff_Closed(object sender, EventArgs e)
        {
            ASSY004_001_EQPT_SECT_CUTOFF window = sender as ASSY004_001_EQPT_SECT_CUTOFF;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }




        #region EDC BCR Reading Info
        private bool bSetEdcAutoSelTime = false;
        private Storyboard sbBcrWarning = new Storyboard();
        private SolidColorBrush edcBrush = new SolidColorBrush(Colors.DarkOrange);

        private System.Windows.Threading.DispatcherTimer dspTmr_EdcWarn = new System.Windows.Threading.DispatcherTimer();

        private void HideBcrWarning()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdBcrWarning.Visibility = Visibility.Collapsed;
                //sbBcrWarning.Stop();

                if (grdBcrWarning.RowDefinitions[1].Height.Value <= 0) return;
                grdBcrWarning.RowDefinitions[0].Height = new GridLength(0);
                grdBcrWarning.RowDefinitions[2].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 300);
                gla.Completed += HideLengthAnimationCompleted;
                //gla.FillBehavior = FillBehavior.HoldEnd;
                grdBcrWarning.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));
        }

        private void ShowBcrWarning()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdBcrWarning.Visibility = Visibility.Visible;
                //BcrWarnAnimationInRectangle(recBcrWarning);

                //if (grdBcrWarning.RowDefinitions[1].Height.Value > 0) return;

                grdBcrWarning.RowDefinitions[0].Height = new GridLength(5);
                grdBcrWarning.RowDefinitions[2].Height = new GridLength(5);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 300);
                gla.Completed += ShowLengthAnimationCompleted;
                //gla.FillBehavior = FillBehavior.Stop;
                grdBcrWarning.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));
        }

        private void ShowLengthAnimationCompleted(object sender, EventArgs e)
        {
            grdBcrWarning.Visibility = Visibility.Visible;
            BcrWarnAnimationInRectangle(recBcrWarning);
        }

        private void HideLengthAnimationCompleted(object sender, EventArgs e)
        {
            grdBcrWarning.Visibility = Visibility.Collapsed;
            sbBcrWarning.Stop();
        }

        private void BcrWarnAnimationInRectangle(System.Windows.Shapes.Rectangle rec)
        {
            rec.Fill = edcBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(1.1);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "BCR_WARNING");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));

            sbBcrWarning.Children.Add(opacityAnimation);
            sbBcrWarning.Begin(this);
        }

        private void GetBcrReadingRate(bool bChgEqptID)
        {
            try
            {
                if (Util.NVC(cboEquipment.SelectedValue).Equals("") || Util.NVC(cboEquipment.SelectedValue).IndexOf("SELECT") >= 0)
                {
                    HideBcrWarning();
                    Util.gridClear(dgEDCInfo);
                    return;
                }

                Edc_LoadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EDC_EQPT_BCR_READ_RATE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Select("WARN_DISP = 'Y'").Length > 0)
                        {
                            ShowBcrWarning();

                            if (bChgEqptID)
                                tbEDCBCRInfo.IsSelected = true;
                        }
                        else
                            HideBcrWarning();

                        Util.GridSetData(dgEDCInfo, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        Edc_LoadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                Edc_LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dspTmr_EdcWarn_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (tbEDCBCRInfo.Visibility != Visibility.Visible) return;

                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    GetBcrReadingRate(false);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr != null && dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));

        }

        private void cboEdcAutoSearch_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dspTmr_EdcWarn != null)
                {
                    dspTmr_EdcWarn.Stop();

                    int iSec = 0;

                    if (cboEdcAutoSearch != null && cboEdcAutoSearch.SelectedValue != null && !cboEdcAutoSearch.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboEdcAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && bSetEdcAutoSelTime)
                    {
                        dspTmr_EdcWarn.Interval = new TimeSpan(0, 0, iSec);
                        return;
                    }

                    if (iSec == 0)
                    {
                        bSetEdcAutoSelTime = true;
                        return;
                    }

                    dspTmr_EdcWarn.Interval = new TimeSpan(0, 0, iSec);
                    dspTmr_EdcWarn.Start();

                    bSetEdcAutoSelTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckEDCVisibity()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = Process.NOTCHING;
                dtRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("BCD_READ_RATE_DISP_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["BCD_READ_RATE_DISP_FLAG"]).Equals("Y"))
                    {
                        tbEDCBCRInfo.Visibility = Visibility.Visible;

                        if (dspTmr_EdcWarn != null && dspTmr_EdcWarn.Interval.TotalSeconds > 0)
                            dspTmr_EdcWarn.Start();
                    }
                    else
                    {
                        tbEDCBCRInfo.Visibility = Visibility.Collapsed;
                        HideBcrWarning();
                        if (dspTmr_EdcWarn != null)
                            dspTmr_EdcWarn.Stop();
                    }
                }
                else
                {
                    tbEDCBCRInfo.Visibility = Visibility.Collapsed;
                    HideBcrWarning();
                    if (dspTmr_EdcWarn != null)
                        dspTmr_EdcWarn.Stop();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dspTmr_EdcWarn != null)
                    this.dspTmr_EdcWarn.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = new CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.NOTCHING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
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

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                {
                    if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                    {
                        txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                    }
                    else
                    {
                        txtShiftStartTime.Text = string.Empty;
                    }

                    if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                    {
                        txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                    }
                    else
                    {
                        txtShiftEndTime.Text = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                    {
                        txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                    }
                    else
                    {
                        txtShiftDateTime.Text = string.Empty;
                    }

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

                    if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                    {
                        txtShift.Tag = string.Empty;
                        txtShift.Text = string.Empty;
                    }
                    else
                    {
                        txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                        txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                    }
                }
                else
                {
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
