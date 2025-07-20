/*************************************************************************************
 Created Date : 2016.09.12
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.12  INS 김동일K : Initial Created.
  2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
  2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
  2023.05.17  안유수 : E20230427-001043 ESMI 공정PC GMES 계정으로 접근 시 투입 LOT 종료취소 버튼 비활성화 처리
  2023.07.17  안유수 : E20230707-001494 ESMI법인의 경우, 추가 기능 - 불량정보관리 버튼 숨김 처리
  2023.11.22  안유수 : E20231006-001025 특별 TARY 적용 관련 사유 콤보박스 추가 및 GROUP ID 발번 기능 추가
  2024.02.14  김용군 : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_007.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_007 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private bool bSetAutoSelTime = false;

        private bool bTestMode = false;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        private string _BottmMsgMode = string.Empty;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush myAnimatedBrush = new SolidColorBrush(Colors.Fuchsia);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        private object _BOTTOM_MSG_TYPE;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherTimer_QC = new System.Windows.Threading.DispatcherTimer();

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();
        private UC_IN_OUTPUT winInput = null;
        
        private struct PRV_VALUES
        {
            public string sPrvTray;

            public PRV_VALUES(string sTray)
            {
                sPrvTray = sTray;
            }
        }

        private PRV_VALUES _PRV_VLAUES = new PRV_VALUES("");

        private static class _BRUSH
        {
            public static readonly string RED = "redBrush";
            public static readonly string FUCHSIA = "myAnimatedBrush";
            public static readonly string YELLOW = "yellowBrush";          
        }

        private enum BOTTOM_MSG_TYPE
        {
            TEST_MODE,
            SHUTDOWN_MODE,
            QC_NO_INPUT_ALARM
        }
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
        public ASSY001_007()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 특별 TRAY  사유 Combo
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboOutTraySplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");

            if (cboOutTraySplReason != null && cboOutTraySplReason.Items != null && cboOutTraySplReason.Items.Count > 0)
                cboOutTraySplReason.SelectedIndex = 0;


            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { cboEquipment };
            //ESMI-A4동 6Line 제외처리
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);
            if (IsCmiExceptLine())
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "ESMI_A4_EXCEPT_LINEID");
            }
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);
            }

            String[] sFilter1 = { Process.PACKAGING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT_NEW");
            // 2017.07.24  Lee. D. R : 해당라인에 설비가 1개인 경우는 자동선택 될수 있도록 수정
            // EQUIPMENT => EQUIPMENT_NEW 신규 생성

            // 자동 조회 시간 Combo
            String[] sFilter4 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;

        }
        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.RegisterName(_BRUSH.RED, redBrush);
            this.RegisterName(_BRUSH.FUCHSIA, myAnimatedBrush);
            this.RegisterName(_BRUSH.YELLOW, yellowBrush);

            InitCombo();

            rdoTraceUse.IsChecked = true;
            rdoTraceNotUse.IsEnabled = false;   // 오창 자동차는 모두 trace 모드..

            // 생산 반제품 조회 Timer
            if (dispatcherTimer != null)
            {
                int iSec = 0;

                if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                //dispatcherTimer.Start();
            }

            // 인장강도 측정여부 조회 타이머
            if (dispatcherTimer_QC != null)
            {
                int iQCSec = 60;

                dispatcherTimer_QC.Tick += dispatcherTimer_QC_Tick;
                dispatcherTimer_QC.Interval = new TimeSpan(0, 0, iQCSec);
            }

            btnReworkCell.Visibility = ((LoginInfo.CFG_AREA_ID.Equals("A5") || LoginInfo.CFG_AREA_ID.Equals("A6")) ? Visibility.Visible : Visibility.Collapsed);  //폴란드 재작업 CELL 등록.
            HideBottomMsgArea();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //GetTestMode();

            ApplyPermissions();

            SetWorkOrderWindow();

            SetInputWindow();

            #region ESMI 공정PC GMES 계정 접근 시 투입 LOT 종료취소 버튼 비활성화
            if (LoginInfo.CFG_SHOP_ID.Equals("G382"))
            {
                btnDefect.Visibility = Visibility.Collapsed;

                if (LoginInfo.USERTYPE == "P")
                {
                    btnCancelTerm.Visibility = Visibility.Collapsed;
                    btnCancelTermMulti.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnCancelTerm.Visibility = Visibility.Visible;
                    btnCancelTermMulti.Visibility = Visibility.Visible;
                }
            }
            #endregion

            // 생산 반제품 조회 Timer Start.
            if (dispatcherTimer != null)
                dispatcherTimer.Start();

            if (CheckUseQCAlarm())
            {
                if (dispatcherTimer_QC != null)
                    dispatcherTimer_QC.Start();
            }
            else
            {
                if (dispatcherTimer_QC != null)
                    dispatcherTimer_QC.Stop();
            }


            if (AuthCheck())
            {
                btnCancelConfirm.Visibility = Visibility.Visible;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
            {
                HiddenLoadingIndicator();
                return;
            }

            // 기준정보 조회
            GetLotIdentBasCode();            
            SetInOutCtrlByLotIdentBasCode();    // Lot 식별 기준 코드에 따른 컨트롤 변경.

            GetWorkOrder();
            GetProductLot();

            GetEqptWrkInfo();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            ASSY001_007_RUNSTART wndRunStart = new ASSY001_007_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = _LDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.Closed += new EventHandler(wndRunStart_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                grdMain.Children.Add(wndRunStart);
                wndRunStart.BringToFront();
            }
        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunComplete())
                return;

            CMM_ASSY_EQPEND wndPop = new CMM_ASSY_EQPEND();
            wndPop.FrameOperation = FrameOperation;

            if (wndPop != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = "N";    // Stacking.
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPDTTM_ST_ORG"));
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "DTTM_NOW"));
                C1WindowExtension.SetParameters(wndPop, Parameters);

                wndPop.Closed += new EventHandler(wndEqpEnd_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                grdMain.Children.Add(wndPop);
                wndPop.BringToFront();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirm())
                return;

            if (!CanInputComplete())
                return;

            #region 작업조건/품질정보 입력 여부
            string _ValueToLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

            // 작업조건 등록 여부
            if (Util.EQPTCondition(Process.PACKAGING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnEqptCond_Click(null, null);
                return;
            }
            // 품질정보 등록 여부
            if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, Process.PACKAGING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnQualityInput_Click(null, null);
                return;
            }
            #endregion

            // 설비 Loss 등록 여부 체크
            DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.PACKAGING);

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
                        ConfirmProcess();
                    }
                }, param);
            }
            else
            {
                ConfirmProcess();
            }
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_007_WAITLOT wndWait = new ASSY001_007_WAITLOT();
            wndWait.FrameOperation = FrameOperation;

            if (wndWait != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.PACKAGING;
                C1WindowExtension.SetParameters(wndWait, Parameters);

                wndWait.Closed += new EventHandler(wndWait_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndWait.ShowModal()));
                grdMain.Children.Add(wndWait);
                wndWait.BringToFront();
            }
        }

        private void btnWaitLotDelete_Click(object sender, RoutedEventArgs e)
        {

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            try
            {
                CMM_ASSY_WAITLOT_DELETE popWait = new CMM_ASSY_WAITLOT_DELETE { FrameOperation = FrameOperation };

                object[] parameters = new object[4];
                parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                parameters[1] = cboEquipment.SelectedValue.ToString();
                parameters[2] = Process.PACKAGING;
                parameters[3] = ProcessType.Packaging;
                C1WindowExtension.SetParameters(popWait, parameters);

                popWait.Closed += new EventHandler(popWait_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => popWait.ShowModal()));
                grdMain.Children.Add(popWait);
                popWait.BringToFront();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popWait_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WAITLOT_DELETE pop = sender as CMM_ASSY_WAITLOT_DELETE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(pop);
        }

        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
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
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                Parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                Parameters[6] = txtShift.Text; // 작업조명
                Parameters[7] = txtShift.Tag; // 작업조코드
                Parameters[8] = txtWorker.Text; // 작업자명
                Parameters[9] = txtWorker.Tag; // 작업자 ID

                C1WindowExtension.SetParameters(wndEqpComment, Parameters);

                wndEqpComment.Closed += new EventHandler(wndEqpComment_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
                grdMain.Children.Add(wndEqpComment);
                wndEqpComment.BringToFront();
            }
        }

        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {
            if (!CanQuality())
                return;

            CMM_ASSY_QUALITY2 wndPopup = new CMM_ASSY_QUALITY2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Process.PACKAGING;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                //   Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODID"));

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndQuality_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void btnAbnormal_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_007_ABNORMAL wndPopup = new ASSY001_007_ABNORMAL();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Process.PACKAGING;
                Parameters[1] = cboEquipment.SelectedValue;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndAbnormal_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //if (winWorkOrder == null)
            //    return;

            //winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            //winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            //winWorkOrder.PROCID = Process.NOTCHING;

            //winWorkOrder.ClearWorkOrderInfo();

            //ClearControls();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();

                ClearControls();

                string sEqsgID = string.Empty;
                string sEqptID = string.Empty;

                if (cboEquipmentSegment.SelectedValue != null)
                    sEqsgID = cboEquipmentSegment.SelectedValue.ToString();
                else
                    sEqsgID = "";

                if (cboEquipment.SelectedValue != null)
                    sEqptID = cboEquipment.SelectedValue.ToString();
                else
                    sEqptID = "";

                if (winWorkOrder != null)
                {
                    winWorkOrder.EQPTSEGMENT = sEqsgID;
                    winWorkOrder.EQPTID = sEqptID;
                    winWorkOrder.PROCID = Process.PACKAGING;

                    winWorkOrder.ClearWorkOrderInfo();
                }

                if (winInput != null)
                {
                    winInput.ChangeEquipment(sEqsgID, sEqptID);
                }

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

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                }

                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 포장 테스트를 위한 가상 Tray 정보 생성 임시 로직.
        private void btnTestFullTrayCreate_Click(object sender, RoutedEventArgs e)
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정보생성 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1887", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (cboEquipment == null || cboEquipment.SelectedValue == null)
                                return;

                            int iPrdRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK"); // 조립 lot row

                            if (iPrdRow < 0)
                                return;

                            if (txtTestFullTrayID.Text.Length != 10)
                            {
                                //Util.AlertInfo("생성할 Tray ID를 입력하세요.");
                                Util.MessageValidation("SFU1626");
                                return;
                            }

                            if (!Util.CheckDecimal(txtTestFullTrayCnt.Text, 0))
                            {
                                //Util.AlertInfo("생성 수량을 입력하세요.");
                                Util.MessageValidation("SFU1620");
                                return;
                            }

                            int iCnt = int.Parse(txtTestFullTrayCnt.Text); // 총 tray 생성 수

                            if (!Util.CheckDecimal(txtTestFullTrayID.Text.Substring(4, 6), 0))
                                return;

                            int iStartNum = int.Parse(txtTestFullTrayID.Text.Substring(4, 6)); // 시작 tray 넘버

                            int iTrayTotCnt = GetCstCellQtyInfo(txtTestFullTrayID.Text.Substring(0, 4));

                            if (iTrayTotCnt == 0) iTrayTotCnt = 50;   // tray 내 cell 수량.

                            string sTmptrayID = ""; // tray id
                            string sTrayOutLot = "";    // tray id 에 대한 db key id
                            string sPkgLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iPrdRow].DataItem, "LOTID"));

                            ShowLoadingIndicator();

                            for (int i = 0; i < iCnt; i++)
                            {
                                sTmptrayID = txtTestFullTrayID.Text.Substring(0, 4) + (iStartNum + i).ToString("000000");

                                // Tray 생성.
                                DataSet indataSet = _Biz.GetBR_PRD_REG_CREATE_TRAY_CL();

                                DataTable inTable = indataSet.Tables["IN_EQP"];

                                DataRow newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                                newRow["PROD_LOTID"] = sPkgLot;
                                newRow["OUT_LOTID"] = ""; // TRAY MAPPING LOT
                                newRow["TRAYID"] = sTmptrayID;
                                newRow["WO_DETL_ID"] = null;
                                newRow["USERID"] = LoginInfo.USERID;

                                inTable.Rows.Add(newRow);
                                //newRow = null;

                                //DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                                //newRow = inMtrlTable.NewRow();
                                //newRow["INPUT_LOTID"] = "";
                                //newRow["MTRLID"] = "";
                                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";

                                //inMtrlTable.Rows.Add(newRow);

                                newRow = null;

                                DataTable inSpcl = indataSet.Tables["IN_SPCL"];
                                newRow = inSpcl.NewRow();
                                newRow["SPCL_CST_GNRT_FLAG"] = "N";
                                newRow["SPCL_CST_NOTE"] = "";
                                newRow["SPCL_CST_RSNCODE"] = "";

                                inSpcl.Rows.Add(newRow);

                                try
                                {
                                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_OUT_LOT_CL", "IN_EQP,IN_INPUT,IN_SPCL", "OUT_LOT", indataSet);

                                    if (dsRslt != null && dsRslt.Tables.Contains("OUT_LOT") && dsRslt.Tables["OUT_LOT"].Rows.Count > 0)
                                    {
                                        sTrayOutLot = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["OUT_LOTID"]);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(ex);
                                    return;
                                }


                                // Cell 생성.

                                string sTmpCellID = sPkgLot.Substring(3, 5) + sPkgLot.Substring(9, 1);
                                int iRow = 0;
                                int iLocation = 0;

                                // 해당 LOT의 MAX SEQ 조회.
                                DataTable inTmpTable = new DataTable();
                                inTmpTable.Columns.Add("LOTID", typeof(string));
                                inTmpTable.Columns.Add("OUT_LOTID", typeof(string));
                                inTmpTable.Columns.Add("TRAYID", typeof(string));
                                inTmpTable.Columns.Add("CELLID", typeof(string));

                                DataRow newTmpRow = inTmpTable.NewRow();
                                newTmpRow["LOTID"] = sPkgLot;
                                //newTmpRow["OUT_LOTID"] = _OutLotID;
                                //newTmpRow["TRAYID"] = _TrayID;
                                //newTmpRow["CELLID"] = "";

                                inTmpTable.Rows.Add(newTmpRow);

                                try
                                {

                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CELL_SEQ_IN_TRAY", "INDATA", "OUTDATA", inTmpTable);

                                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                                        iRow = Util.NVC(dtRslt.Rows[0]["MAXSEQ"]).Equals("") ? 0 : Convert.ToInt32(Util.NVC(dtRslt.Rows[0]["MAXSEQ"]));
                                }
                                catch (Exception ex)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(ex);
                                    return;
                                }

                                indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL();
                                newRow = null;

                                inTable = indataSet.Tables["IN_EQP"];
                                newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                                newRow["PROD_LOTID"] = sPkgLot;
                                newRow["OUT_LOTID"] = sTrayOutLot;
                                newRow["CSTID"] = sTmptrayID;
                                newRow["USERID"] = LoginInfo.USERID;

                                inTable.Rows.Add(newRow);
                                newRow = null;

                                DataTable inSublotTable = indataSet.Tables["IN_CST"];

                                for (int j = 1; j <= iTrayTotCnt; j++)
                                {
                                    iLocation = iLocation + 1;

                                    newRow = inSublotTable.NewRow();
                                    newRow["SUBLOTID"] = sTmpCellID + (iRow + j).ToString("0000");
                                    newRow["CSTSLOT"] = iLocation.ToString();

                                    inSublotTable.Rows.Add(newRow);
                                }

                                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL", "IN_EQP,IN_CST", null, indataSet);
                            }

                            HiddenLoadingIndicator();
                            GetOutTray();
                        }
                    });
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }));
        }

        private void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (grdTestFullTrayCreate != null && grdTestFullTrayCreate.Visibility == Visibility.Collapsed)
                {
                    grdTestFullTrayCreate.Visibility = Visibility.Visible;
                }
                else if (grdTestFullTrayCreate != null && grdTestFullTrayCreate.Visibility == Visibility.Visible)
                {
                    grdTestFullTrayCreate.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnTestFullTrayAllConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("일괄확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1794", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (dgOut == null || dgOut.Rows.Count < 1)
                                return;

                            ShowLoadingIndicator();

                            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                                {
                                    try
                                    {
                                        DataSet indataSet = _Biz.GetBR_PRD_REG_TRAY_CONFIRM_CL();

                                        DataTable inTable = indataSet.Tables["IN_EQP"];
                                        DataRow newRow = inTable.NewRow();
                                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                        newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                                        newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                                        newRow["USERID"] = LoginInfo.USERID;

                                        inTable.Rows.Add(newRow);
                                        newRow = null;

                                        DataTable inCst = indataSet.Tables["IN_CST"];
                                        newRow = inCst.NewRow();

                                        newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "OUT_LOTID"));
                                        newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CELLQTY")));
                                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "TRAYID"));
                                        newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "SPECIALYN"));
                                        newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "SPECIALDESC"));
                                        newRow["SPCL_CST_RSNCODE"] = "";

                                        inCst.Rows.Add(newRow);

                                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_OUT_LOT_CL", "IN_EQP,IN_CST", null, indataSet);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                    }
                                }
                            }

                            GetOutTray();

                            HiddenLoadingIndicator();
                        }
                    });
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }));
        }

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDefectInfo())
                return;

            CMM_ASSY_DEFECT wndDefect = new CMM_ASSY_DEFECT();
            wndDefect.FrameOperation = FrameOperation;

            if (wndDefect != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = "N";    // Stacking.
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WO_DETL_ID"));

                C1WindowExtension.SetParameters(wndDefect, Parameters);

                wndDefect.Closed += new EventHandler(wndDefect_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndDefect.ShowModal()));
                grdMain.Children.Add(wndDefect);
                wndDefect.BringToFront();
            }
        }

        private void dgOut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgOut_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void cboAutoSearchOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOut != null && cboAutoSearchOut.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                    if (iSec == 0 && bSetAutoSelTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        bSetAutoSelTime = true;
                        return;
                    }

                    dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    dispatcherTimer.Start();

                    if (bSetAutoSelTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        Util.MessageInfo("SFU1605", cboAutoSearchOut.SelectedValue.ToString());
                    }

                    bSetAutoSelTime = true;
                }
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
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                C1WindowExtension.SetParameters(wndEqptCond, Parameters);

                wndEqptCond.Closed += new EventHandler(wndEqptCond_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
                grdMain.Children.Add(wndEqptCond);
                wndEqptCond.BringToFront();
            }
        }

        private void btnCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");  // 라인을 선택 하세요.
                return;
            }

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                CMM_ASSY_CANCEL_TERM_CST wndCancelTerm = new CMM_ASSY_CANCEL_TERM_CST();
                wndCancelTerm.FrameOperation = FrameOperation;

                if (wndCancelTerm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Process.PACKAGING;

                    C1WindowExtension.SetParameters(wndCancelTerm, Parameters);

                    wndCancelTerm.Closed += new EventHandler(wndCancelTermCST_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
                    grdMain.Children.Add(wndCancelTerm);
                    wndCancelTerm.BringToFront();
                }
            }
            else
            {
                CMM_ASSY_CANCEL_TERM wndCancelTerm = new CMM_ASSY_CANCEL_TERM();
                wndCancelTerm.FrameOperation = FrameOperation;

                if (wndCancelTerm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Process.PACKAGING;

                    C1WindowExtension.SetParameters(wndCancelTerm, Parameters);

                    wndCancelTerm.Closed += new EventHandler(wndCancelTerm_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
                    grdMain.Children.Add(wndCancelTerm);
                    wndCancelTerm.BringToFront();
                }
            }
        }

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
                Parameters[3] = Process.PACKAGING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);  //EQPTID 추가 
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void btnTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTestMode()) return;

            if (bTestMode)
            {
                SetTestMode(false);
                GetTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // 테스트 Run이 되면 실적처리가 되지 않습니다. 테스트 Run 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetTestMode(true);
                        GetTestMode();
                    }
                });
            }
        }

        private void btnQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!CanQuality())
                return;
            
            CMM_COM_QUALITY wndQualityInput = new CMM_COM_QUALITY();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Process.PACKAGING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[3] = Util.NVC(cboEquipment.Text);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInput_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                //grdMain.Children.Add(wndQualityInput);
                //wndQualityInput.BringToFront();
            }
        }

        private void btnQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            CMM_ASSY_QUALITY_PKG wndPopup = new CMM_ASSY_QUALITY_PKG();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = Process.PACKAGING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndQualityRslt_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                //foreach (System.Windows.UIElement child in grdMain.Children)
                //{
                //    if (child.GetType() == typeof(CMM_ASSY_QUALITY_PKG))
                //    {
                //        grdMain.Children.Remove(child);
                //        break;
                //    }
                //}

                //grdMain.Children.Add(wndPopup);
                //wndPopup.BringToFront();
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
                Parameters[1] = Process.PACKAGING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndEqptCondSearch, Parameters);

                wndEqptCondSearch.Closed += new EventHandler(wndEqptCondSearch_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCondSearch.ShowModal()));
            }
        }

        private void wndEqptCondSearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND_SEARCH window = sender as CMM_ASSY_EQPT_COND_SEARCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
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
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgProductLot.SelectedIndex = idx;

                // 상세 정보 조회
                ProdListClickedProcess(idx);

                this.Dispatcher.BeginInvoke(new Action(() => dispatcherTimer_QC_Tick(this.dispatcherTimer_QC, null)));
            }
        }
        #endregion

        #region [투입]

        #endregion

        #region [생산]

        private void dgOut_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        // 이전 값 저장.
                                        _PRV_VLAUES.sPrvTray = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));

                                        SetOutTrayButtonEnable(e.Cell.Row);

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;

                                        _PRV_VLAUES.sPrvTray = "";

                                        // 확정 시 저장, 삭제 버튼 비활성화
                                        SetOutTrayButtonEnable(null);
                                    }
                                }
                                break;
                        }

                        if (dgOut.CurrentCell != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                        else if (dgOut.Rows.Count > 0)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);

                    }
                }
            }));
        }

        private void dgOut_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            //if (e.Row.Index < 0)
            //    return;

            //if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
            //{
            //    e.EditingElement.IsEnabled = false;
            //}
        }

        private void dgOut_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN") ||
                    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                {
                    e.Cancel = true;
                    //dgOut.BeginEdit(e.Row.Index, 4);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayConfirm())
                return;

            TrayConfirmProcess();

            // 특별 Tray 정보 조회.
            //GetSpecialTrayInfo();
        }

        private void btnOutDel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayDelete())
                return;

            //string sMsg = "삭제 하시겠습니까?";
            string messageCode = "SFU1230";
            string sCellQty = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "CELLQTY"));
            double dCellQty = 0;

            if (!sCellQty.Equals(""))
                double.TryParse(sCellQty, out dCellQty);

            if (!sCellQty.Equals("") && dCellQty != 0)
            {
                //sMsg = "Cell 수량이 존재 합니다.\n그래도 삭제 하시겠습니까?";
                messageCode = "SFU1320";
            }

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    TrayDelete();
                }
            });
        }

        private void btnOutDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayDefect())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량 Tray로 등록 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1229", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //SetBadTray();
                }
            });
        }

        private void btnOutCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateTray())
                return;

            // 특별 Tray 정보 Check.
            string sSamePkgLot = "Y";
            string messageCode = string.Empty;

            DataTable dtRslt = GetSpecialTrayInfo();
            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                string sSpclProdLot = Util.NVC(dtRslt.Rows[0]["SPCL_PROD_LOTID"]);
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (!sSpclProdLot.Equals("") && iRow >= 0)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")).Equals(sSpclProdLot))
                    {
                        sSamePkgLot = "N";

                        //sMsg = "선택한 조립 LOT과 특별 TRAY로 설정된 조립 LOT이 다릅니다.";
                        messageCode = "SFU1665";
                    }
                }
            }

            if (string.IsNullOrEmpty(messageCode))
            {
                ASSY001_007_TRAY_CREATE wndTrayCreate = new ASSY001_007_TRAY_CREATE();
                wndTrayCreate.FrameOperation = FrameOperation;

                if (wndTrayCreate != null)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    Parameters[4] = (bool)rdoTraceUse.IsChecked ? "Y" : "N";
                    Parameters[5] = "";//cboTrayType.SelectedValue.ToString();
                    Parameters[6] = sSamePkgLot;

                    C1WindowExtension.SetParameters(wndTrayCreate, Parameters);

                    wndTrayCreate.Closed += new EventHandler(wndTrayCreate_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndTrayCreate.ShowModal()));
                    grdMain.Children.Add(wndTrayCreate);
                    wndTrayCreate.BringToFront();
                }
            }
            else
            {
                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ASSY001_007_TRAY_CREATE wndTrayCreate = new ASSY001_007_TRAY_CREATE();
                        wndTrayCreate.FrameOperation = FrameOperation;

                        if (wndTrayCreate != null)
                        {
                            object[] Parameters = new object[7];
                            Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[1] = cboEquipment.SelectedValue.ToString();
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                            Parameters[4] = (bool)rdoTraceUse.IsChecked ? "Y" : "N";
                            Parameters[5] = "";// cboTrayType.SelectedValue.ToString();
                            Parameters[6] = sSamePkgLot;

                            C1WindowExtension.SetParameters(wndTrayCreate, Parameters);

                            wndTrayCreate.Closed += new EventHandler(wndTrayCreate_Closed);

                            // 팝업 화면 숨겨지는 문제 수정.
                            //this.Dispatcher.BeginInvoke(new Action(() => wndTrayCreate.ShowModal()));
                            grdMain.Children.Add(wndTrayCreate);
                            wndTrayCreate.BringToFront();
                        }
                    }
                });
            }
        }

        private void btnOutCell_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChangeCell())
                return;

            ChangeCellInfo();
        }

        private void btnOutMove_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayMove())
                return;

            ASSY001_007_TRAY_MOVE wndTrayMove = new ASSY001_007_TRAY_MOVE();
            wndTrayMove.FrameOperation = FrameOperation;

            if (wndTrayMove != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "TRAYID")).Replace("\0", "");

                C1WindowExtension.SetParameters(wndTrayMove, Parameters);

                wndTrayMove.Closed += new EventHandler(wndTrayMove_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndTrayMove.ShowModal()));
                grdMain.Children.Add(wndTrayMove);
                wndTrayMove.BringToFront();
            }
        }

        private void btnOutConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirmCancel())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetTrayConfirmCancel();
                }
            });
        }

        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveTray())
                return;

            SaveTray();
        }

        private void rdoTraceNotUse_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOut == null)
                return;

            if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                dgOut.Columns["CELLQTY"].IsReadOnly = false;
            else
                dgOut.Columns["CELLQTY"].IsReadOnly = true;
        }

        private void rdoTraceUse_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOut == null)
                return;

            if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                dgOut.Columns["CELLQTY"].IsReadOnly = true;
            else
                dgOut.Columns["CELLQTY"].IsReadOnly = false;
        }

        private void btnOutTraySplSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanOutTraySplSave())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("적용 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetSpecialTray();
                }
            });
        }

        #region Hold 등록 팝업   
        private void btnOutTraySplHold_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChangeCell())
                return;

            TrayHoldPopUp("TRAY");
        }

        private void TrayHoldPopUp(string holdTrgtCode)
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

            if (idx < 0)
                return;

            string sTrayQty = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CST_CELL_QTY"));
            string trayID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID")).Replace("\0", "");
            string outLOTID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));
            string sProdID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "PRODID"));
            
            ASSY001_007_TRAYHOLD wndTrayHoldList = new ASSY001_007_TRAYHOLD();
            wndTrayHoldList.FrameOperation = FrameOperation;

            if (wndTrayHoldList != null)
            {
                object[] Parameters = new object[10];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[4] = trayID;
                Parameters[5] = sTrayQty;
                Parameters[6] = outLOTID;
                Parameters[7] = sProdID;
                Parameters[8] = holdTrgtCode;
                Parameters[9] = "HOLD";  // HOLD 생성 : "HOLD", HOLD 해제 : "UNHOLD"

                C1WindowExtension.SetParameters(wndTrayHoldList, Parameters);

                wndTrayHoldList.Closed += new EventHandler(wndTrayHoldList_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndTrayHoldList.ShowModal()));
                grdMain.Children.Add(wndTrayHoldList);
                wndTrayHoldList.BringToFront();
            }         
        }

        private void chkOutTraySpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutTrayReamrk != null)
            {
                txtOutTrayReamrk.Text = "";
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        public void GetProductLot(bool bSelPrv = true, string sNewLot = "")
        {
            try
            {
                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                {
                    HiddenLoadingIndicator();
                    return;
                }

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                    // 착공 후 조회 시..
                    if (sNewLot != null && !sNewLot.Equals(""))
                        sPrvLot = sNewLot;
                }

                ClearControls();

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIPINFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.PACKAGING;
                //newRow["WOID"] = Util.NVC(drWorkOrderInfo["WOID"]);
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                //newRow["WIPSTAT"] = "WAIT,PROC,END";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgProductLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgProductLot, searchResult, FrameOperation, true);

                        if (!sPrvLot.Equals("") && bSelPrv)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgProductLot.SelectedIndex = idx;

                                ProdListClickedProcess(idx);
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInputLotList();
                                GetWaitBoxList();
                            }
                        }
                        else
                        {
                            if (searchResult.Rows.Count > 0)
                            {
                                int iRowRun = _Util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)
                                {
                                    iRowRun = 0;
                                    if (dgProductLot.TopRows.Count > 0)
                                        iRowRun = dgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;

                                    ProdListClickedProcess(iRowRun);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;

                                    ProdListClickedProcess(iRowRun);
                                }
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInputLotList();
                                GetWaitBoxList();
                            }
                        }

                        // 2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
                        GetLossCnt();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );

                GetEIOInfo();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetEIOInfo()
        {
            //try
            //{
            //    ShowLoadingIndicator();

            //    DataTable inTable = _Biz.GetDA_PRD_SEL_EIOINFO_CL();

            //    DataRow newRow = inTable.NewRow();
            //    newRow["EQPTID"] = cboEquipment.SelectedValue;

            //    inTable.Rows.Add(newRow);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("", "INDATA", "OUTDATA", inTable);

            //    if (dtResult != null && dtResult.Rows.Count > 0)
            //    {
            //        string sTraceYN = Util.NVC(dtResult.Rows[0]["CELLTRACEYN"]);
            //        string sTrayQty = Util.NVC(dtResult.Rows[0]["TRAYCELLSTNDQTY"]);

            //        rdoTraceUse.IsChecked = sTraceYN.Equals("Y") ? true : false;
            //        rdoTraceNotUse.IsChecked = sTraceYN.Equals("Y") ? true : false;

            //        cboTrayType.Text = sTrayQty;
            //    }

            //    HiddenLoadingIndicator();
            //}
            //catch (Exception ex)
            //{
            //    HiddenLoadingIndicator();
            //    Util.AlertByBiz("", ex.Message, ex.ToString());
            //}
        }

        private void GetOutTray()
        {
            try
            {
                // Tray 관련 버튼 처리.
                SetOutTrayButtonEnable(null);

                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                //dgOut.ItemsSource = DataTableConverter.Convert(searchResult);
                Util.GridSetData(dgOut, searchResult, FrameOperation, true);

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                (dgOut.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt.Copy());

                //new ClientProxy().ExecuteService("COR_SEL_LANGUAGE", null, "RSLTDT", null, (langResult, langEx) =>
                //{
                //    (dgOut.Columns["ComboColumn"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(langResult);
                //});
      
                GetOutTrayRSNCODE(); //사유 콤보박스 Setting
                GetSpecialTrayInfo();

                if (!_PRV_VLAUES.sPrvTray.Equals(""))
                {
                    int idx = _Util.GetDataGridRowIndex(dgOut, "OUT_LOTID", _PRV_VLAUES.sPrvTray);

                    if (idx >= 0)
                    {
                        DataTableConverter.SetValue(dgOut.Rows[idx].DataItem, "CHK", true);

                        dgOut.ScrollIntoView(idx, dgOut.Columns["CHK"].Index);

                        // Tray 관련 버튼 처리.
                        SetOutTrayButtonEnable(dgOut.Rows[idx]);

                        dgOut.CurrentCell = dgOut.GetCell(idx, dgOut.Columns.Count - 1);
                    }
                    else
                    {
                        if (dgOut.CurrentCell != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                        else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                    }
                }
                else
                {
                    if (dgOut.CurrentCell != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                    else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetOutTrayRSNCODE()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SPCL_RSNCODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dr2 = dtResult.NewRow();
                dr2["CBO_CODE"] = "";
                dr2["CBO_NAME"] = "";
                dtResult.Rows.InsertAt(dr2, 0);

                (dgOut.Columns["CBO_SPCL_RSNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTrayInfo(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "TRAYID"));

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";
                        sMsg = "SFU3045";   // TRAY가 미확정 상태가 아닙니다.
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }

                // CNJ ESS 또는 CNA     여기 비즈룰 수정해야 함! 조회하는 테이블을 TB_SFC_EQPT_DATA_CLCT 로 바꿔야함.
                //if (LoginInfo.CFG_SHOP_ID.Equals("G184") || LoginInfo.CFG_SHOP_ID.Equals("G451"))
                //{
                //    DataTable inTable2 = new DataTable();
                //    inTable2.Columns.Add("PROD_LOTID", typeof(string));
                //    inTable2.Columns.Add("OUT_LOTID", typeof(string));
                //    inTable2.Columns.Add("SUBLOTID", typeof(string));

                //    DataRow newRow2 = inTable2.NewRow();
                //    newRow2["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                //    newRow2["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "OUT_LOTID"));
                //    newRow2["SUBLOTID"] = null;

                //    inTable2.Rows.Add(newRow2);

                //    DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_EL_CELL_NO_DATA", "INDATA", "OUTDATA", inTable2);
                //    if (dtResult2 != null && dtResult2.Rows.Count > 0)
                //    {
                //        sRet = "NG";
                //        sMsg = "SFU3054";      // 주액 정보가 없습니다.
                //    }
                //}

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                sRet = "EXCEPTION";
                sMsg = ex.Message;
            }
        }

        private void TrayConfirm()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_TRAY_CONFIRM_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inCst = indataSet.Tables["IN_CST"];
                newRow = inCst.NewRow();

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY")));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID"));
                newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "SPECIALYN"));
                newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "SPECIALDESC"));
                newRow["SPCL_CST_RSNCODE"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "SPECIALRSNCODE")); 

                inCst.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_CL", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();

                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void TrayDelete()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_TRAY_DEL_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "OUT_LOTID"));
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "TRAYID"));
                newRow["WO_DETL_ID"] = null;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_DELETE_OUT_LOT_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetTrayConfirmCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_TRAY_CONFIRM_CNL_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inCst = indataSet.Tables["IN_CST"];
                newRow = inCst.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "OUT_LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "TRAYID"));

                inCst.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CNFM_CANCEL_OUT_LOT_CL", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveTray()
        {
            try
            {
                dgOut.EndEdit();

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

                string specYN = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "SPECIALYN"));
                string SpecDesc = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "SPECIALDESC"));
                string SpecRsnCode = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "SPECIALRSNCODE"));

                if (specYN.Equals("Y"))
                {
                    if (SpecDesc == "")
                    {
                        //Util.Alert("특별관리내역을 입력하세요.");
                        Util.MessageValidation("SFU1990");
                        return;
                    }

                    if (string.IsNullOrEmpty(SpecRsnCode))
                    {
                        //Util.Alert("사유를 선택하세요.");
                        Util.MessageValidation("SFU1593");
                        return;
                    }
                }
                else if (specYN.Equals("N"))
                {
                    if (SpecDesc != "")
                    {
                        //Util.Alert("특별관리내역을 삭제하세요.");
                        Util.MessageValidation("SFU1989");
                        return;
                    }

                    if (!string.IsNullOrEmpty(SpecRsnCode))
                    {
                        //Util.Alert("사유를 삭제하세요.");
                        Util.MessageValidation("SFU8674");
                        return;
                    }                
                }

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_TRAY_SAVE_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inLot = indataSet.Tables["IN_LOT"];
                DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;
                    // Tray 정보 DataTable             
                    newRow = inLot.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID"));
                    if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                        newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY")));


                    inLot.Rows.Add(newRow);
                    newRow = null;

                    // 특별 Tray DataTable                
                    newRow = inSpcl.NewRow();
                    newRow["SPCL_CST_GNRT_FLAG"] = specYN;
                    newRow["SPCL_CST_NOTE"] = SpecDesc;
                    newRow["SPCL_CST_RSNCODE"] = specYN.Equals("Y") ? SpecRsnCode : "";

                    inSpcl.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_CL", "IN_EQP,IN_LOT,IN_SPCL", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetSpecialTray()
        {
            try
            {
                string sRsnCode = cboOutTraySplReason.SelectedValue == null ? "" : cboOutTraySplReason.SelectedValue.ToString();

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["SPCL_LOT_GNRT_FLAG"] = (bool)chkOutTraySpl.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = (bool)chkOutTraySpl.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = txtOutTrayReamrk.Text;
                newRow["SPCL_PROD_LOTID"] = (bool)chkOutTraySpl.IsChecked ? Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID")) : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_SPCL_CST_PKG", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        // 특별 Tray 정보 조회.
                        GetSpecialTrayInfo();

                        if ((bool)chkOutTraySpl.IsChecked)
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Visible;
                            ColorAnimationInSpecialTray();
                        }
                        else
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetSpecialTrayInfo()
        {
            try
            {
                if (cboEquipment == null || cboEquipment.SelectedValue == null)
                    return null;

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_LOT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    chkOutTraySpl.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                    txtOutTrayReamrk.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                    if (cboOutTraySplReason != null && cboOutTraySplReason.Items != null && cboOutTraySplReason.Items.Count > 0 && cboOutTraySplReason.Items.CurrentItem != null)
                    {
                        DataView dtview = (cboOutTraySplReason.Items.CurrentItem as DataRowView).DataView;
                        if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])))
                                {
                                    cboOutTraySplReason.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboOutTraySplReason.Items.Count > 0)
                                cboOutTraySplReason.SelectedIndex = 0;
                        }
                    }

                    //cboOutTraySplReason.SelectedValue = Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"]);

                    if ((bool)chkOutTraySpl.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                        SpcLotID.Text = Util.NVC(dtResult.Rows[0]["SPCL_PROD_LOTID"]);
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    chkOutTraySpl.IsChecked = false;
                    txtOutTrayReamrk.Text = "";

                    if (cboOutTraySplReason.Items.Count > 0)
                        cboOutTraySplReason.SelectedIndex = 0;

                    if ((bool)chkOutTraySpl.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }

                //HiddenLoadingIndicator();

                return dtResult;
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private int GetCstCellQtyInfo(string sTrayType)
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_CST_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID.Equals("S2") ? "A2" : LoginInfo.CFG_AREA_ID; // LNS는 2동 정보로 처리.
                newRow["CST_TYPE_CODE"] = sTrayType;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CST_TYPE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    iCnt = int.Parse(Util.NVC(dtResult.Rows[0]["CST_CELL_QTY"]));
                }

                HiddenLoadingIndicator();

                return iCnt;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return 0;
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
                        //Util.Alert("선택된 W/O가 없습니다.");
                        Util.MessageValidation("SFU1635");
                    }
                    else if (Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK"]).Equals("N"))
                    {
                        bRet = false;
                        //Util.Alert("선택된 W/O가 없습니다.");
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
                HiddenLoadingIndicator();
            }
        }

        private bool GetErpSendInfo(string sLotID, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();

                bool bRet = false;
                DataTable inTable = _Biz.GetDA_PRD_SEL_ERP_SEND_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    // 'S' 가 아닌 경우는 삭제 가능.
                    if (!Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("S")) // S : ERP 전송 중 , P : ERP 전송 대기, Y : ERP 전송 완료
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
                HiddenLoadingIndicator();
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
                newRow["PROCID"] = Process.PACKAGING;

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
                HiddenLoadingIndicator();
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
                HiddenLoadingIndicator();
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
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.PACKAGING;

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
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
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
                HiddenLoadingIndicator();
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    HideBottomMsgArea();
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
                        bTestMode = true;
                        ShowBottomMsgArea("테스트모드사용중", BOTTOM_MSG_TYPE.TEST_MODE);
                    }
                    else
                    {
                        bTestMode = false;
                        HideBottomMsgArea();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
        }

        public void OnCurrInCancel(DataTable inMtrl)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_IN_CANCEL_CL();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                // 취소는 biz에서 모랏 찾음.
                newRow["PROD_LOTID"] = ""; //Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                foreach (DataRow sourcerow in inMtrl.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);

                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_INPUT_LOT_CL", "INDATA,IN_INPUT", null, indataSet);

                    inInputTable.Clear();
                }

                GetProductLot();

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void OnCurrInComplete(DataTable inMtrl)
        {
            try
            {
                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return;

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_IN_COMPLETE_CL();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                foreach (DataRow sourcerow in inMtrl.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_LOT_CL", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public bool OnCurrAutoInputLot(string sInputLot, string sPstnID, string sInMtrlID, string sInQty)
        {
            try
            {

                if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    return false;

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_BASKET_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                newRow = null;

                DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                newRow = inMtrlTable.NewRow();
                newRow["INPUT_LOTID"] = sInputLot;
                //newRow["MTRLID"] = sInMtrlID; // 자동투입 biz 변경.
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                //newRow["ACTQTY"] = Util.NVC(sInQty).Equals("") ? 0 : Convert.ToDecimal(sInQty); // 자동투입 biz 변경.
                newRow["ACTQTY"] = 0;

                inMtrlTable.Rows.Add(newRow);


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_CL", "IN_EQP,IN_INPUT", null, indataSet);

                HiddenLoadingIndicator();

                GetProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
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

        private bool ChkInputHistCnt(string sLotid, string sWipSeq)
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = sLotid;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_MTRL_HIST_CNT", "INDATA", "OUTDATA", inTable);

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
                HiddenLoadingIndicator();
            }
        }

        private bool ChkOutTrayCnt(string sLotid, string sWipSeq)
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = sLotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_CNT", "INDATA", "OUTDATA", inTable);

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
                HiddenLoadingIndicator();
            }
        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_CL", "INDATA", null, inTable, (bizResult, bizException) =>
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
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool ChkEqpLossInfo()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOSS_INPUT_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("NOT_SAVE_LOSS_CNT"))
                    {
                        int iCnt = Util.NVC_Int(dtRslt.Rows[0]["NOT_SAVE_LOSS_CNT"]);

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
                HiddenLoadingIndicator();
            }
        }

        private bool ChkConfirmOutTray()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                if (searchResult != null)
                {
                    DataRow[] drs = searchResult.Select("FORM_MOVE_STAT_CODE = 'WAIT'");
                    if (drs.Length > 0)
                        bRet = true;
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
                HiddenLoadingIndicator();
            }
        }

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
                dtRow["PROCID"] = Process.PACKAGING;
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
                //HiddenLoadingIndicator();
            }
        }

        private bool CheckInputQCData()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));                
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_DATA_INPUT_CHK", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && Util.NVC(dtRslt.Rows[0]["DATA_INPUT_YN"]).Equals("N"))
                {
                    bRet = true;
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
                HiddenLoadingIndicator();
            }
        }

        private bool CheckUseQCAlarm()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = "TENSILE_STRENGTH_INPUT_CHK";
                newRow["COM_CODE"] = "CHK_FLAG";

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && Util.NVC(dtRslt.Rows[0]["ATTR1"]).ToUpper().Equals("Y"))
                {
                    if (dispatcherTimer_QC != null)
                    {
                        dispatcherTimer_QC.Stop();

                        int iSec = 0;
                        int.TryParse(Util.NVC(dtRslt.Rows[0]["ATTR2"]), out iSec);

                        if (iSec < 5)
                        {
                            iSec = 60;
                        }

                        dispatcherTimer_QC.Interval = new TimeSpan(0, 0, iSec);
                        dispatcherTimer_QC.Start();

                        if (FrameOperation != null)
                            FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + "Tensile strength check interval : " + iSec.ToString() + " second.");
                    }
                    bRet = true;
                }
                else
                {
                    if (FrameOperation != null)
                        FrameOperation.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + "Not use tensile strength check.");
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
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Validation]
        private bool CanInputComplete()
        {
            bool bRet = true;

            // 실적확정 대상 Lot 에 물린 바구니 확인
            if (winInput != null)
            {
                try
                {
                    ShowLoadingIndicator();

                    DataTable inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue); ;
                    inTable.Rows.Add(newRow);

                    DataTable dtCurrIn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_IN_LOT_LIST", "INDATA", "OUTDATA", inTable);
                    if (dtCurrIn.Rows != null && dtCurrIn.Rows.Count > 0)
                    {
                        string prodLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        DataRow[] drs = dtCurrIn.Select("MOUNT_MTRL_TYPE_CODE = 'PROD' AND WIPSTAT = 'PROC' AND PROD_LOTID = '" + prodLotID + "'");
                        if (drs.Length > 0)
                        {
                            bRet = false;

                            ASSY001_007_INPUT_OBJECT wndInputObject = new ASSY001_007_INPUT_OBJECT();
                            wndInputObject.FrameOperation = FrameOperation;
                            if (wndInputObject != null)
                            {
                                object[] Parameters = new object[5];
                                Parameters[0] = cboEquipment.SelectedValue.ToString();
                                Parameters[1] = Process.PACKAGING;
                                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                                Parameters[3] = "";
                                Parameters[4] = drs.CopyToDataTable();
                                C1WindowExtension.SetParameters(wndInputObject, Parameters);

                                wndInputObject.Closed += new EventHandler(wndInputObject_Closed);

                                // 팝업 화면 숨겨지는 문제 수정.
                                //this.Dispatcher.BeginInvoke(new Action(() => wndInputObject.ShowModal()));
                                grdMain.Children.Add(wndInputObject);
                                wndInputObject.BringToFront();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);

                    bRet = false;
                    return bRet;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }

            return bRet;
        }

        private bool CanRunComplete()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("장비완료 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU1866");
                return bRet;
            }

            // 투입 완료 여부 체크는 validation 없애도록 의사결정 됨. => 투입된 조립랏인 경우는 체크 처리 하도록 변경.
            // 투입 바구니 완료 여부 체크
            if (winInput != null)
            {
                if (!winInput.CanPkgConfirm(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"))))
                    return bRet;
            }

            bRet = true;
            return bRet;
        }

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

            bRet = true;
            return bRet;
        }

        private bool CanRunStart()
        {
            bool bRet = false;

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            if (!CheckSelWOInfo())
            {
                //Util.Alert("선택된 W/O가 없습니다.");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanConfirm()
        {
            bool bRet = false;

            string sLine = LoginInfo.CFG_EQSG_ID.Substring(3, 1);
            // 남경 SF 라인이 아닌경우에 투입 바구니 확인
            //if (sLine != "F")
            //{
            //    if (dgIn.Rows.Count - dgIn.BottomRows.Count == 0)
            //    {
            //        Util.Alert("투입한 바구니 이력이 없습니다.");
            //        return bRet;
            //    }
            //}

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return bRet;
            }

            #region 투입 바구니 체크 -> CanInputComplete() 에서 확인 하도록 함.
            //// 투입 완료 여부 체크는 validation 없애도록 의사결정 됨. => 투입된 조립랏인 경우는 체크 처리
            //// 투입 바구니 완료 여부 체크
            //if (winInput != null)
            //{
            //    if (!winInput.CanPkgConfirm(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"))))
            //        return bRet;
            //}
            #endregion


            if (ChkConfirmOutTray())
            {
                Util.MessageValidation("SFU1250");  //확정되지 않은 Tray를 확정 해 주세요.
                return bRet;
            }

            //for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            //    {
            //        //Util.Alert("확정되지 않은 Tray를 확정 해 주세요.");
            //        Util.MessageValidation("SFU1250");
            //        return bRet;
            //    }
            //}

            // 설비 Loss 등록 여부 체크
            //if (ChkEqpLossInfo())
            //{
            //    // 설비 LOSS 정보가 등록되지 않았습니다. 등록 후 실적확정 하세요.
            //    Util.MessageValidation("SFU3501");
            //    return bRet;
            //}


            if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            {
                DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();

                int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                if (prevCheck < 0 || nextCheck > 0)
                {
                    Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
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

            bRet = true;
            return bRet;
        }

        private bool CanQuality()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanTrayConfirm()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return bRet;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY")), out dTmp))
            {
                if (dTmp == 0)
                {
                    //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                    Util.MessageValidation("SFU1685");
                    return bRet;
                }
            }
            else
            {
                //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                Util.MessageValidation("SFU1687");
                return bRet;
            }

            string sRet = string.Empty;
            string sMsg = string.Empty;
            // Tray 현재 작업중인지 여부 확인.
            GetTrayInfo(out sRet, out sMsg);
            if (sRet.Equals("NG"))
            {
                Util.MessageValidation(sMsg);
                return bRet;
            }
            else if (sRet.Equals("EXCEPTION"))
                return bRet;

            bRet = true;
            return bRet;
        }

        private bool CanTrayDelete()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // 삭제 biz 에서 fcs 체크
            //// FCS Check...            
            //if (!GetFCSTrayCheck(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "TRAYID")).Replace("\0", "")))
            //{
            //    Util.Alert("FORMATION에 TRAY ID가 작업중입니다. 활성화에 문의하세요.");
            //    return bRet;
            //}



            //// ERP 전송 여부 확인.            
            //if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[iRow].DataItem, "OUT_LOTID")),
            //                    Util.NVC(DataTableConverter.GetValue(dgOut.Rows[iRow].DataItem, "WIPSEQ"))))
            //{
            //    Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[iRow].DataItem, "TRAYID")));
            //    return bRet;
            //}


            bRet = true;
            return bRet;
        }

        private bool CanTrayDefect()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanCreateTray()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanTrayMove()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }
            bRet = true;
            return bRet;
        }

        private bool CanConfirmCancel()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanSaveTray()
        {
            bool bRet = false;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanChangeCell()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanOutTraySplSave()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            if (chkOutTraySpl.IsChecked.HasValue && (bool)chkOutTraySpl.IsChecked)
            {
                if (cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return bRet;
                }

                if (txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return bRet;
                }
            }
            else
            {
                if (!txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanDefectInfo()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
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
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
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

            string sLotid = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
            string sWipSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));

            // 투입 이력 정보 존재여부 확인            
            if (ChkInputHistCnt(sLotid, sWipSeq))
            {
                Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
                return bRet;
            }

            // 완성 이력 정보 존재여부 확인
            if (ChkOutTrayCnt(sLotid, sWipSeq))
            {
                Util.MessageValidation("SFU3438");   // 생산Tray가 존재하여 취소할 수 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [PopUp Event]
        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            ASSY001_007_RUNSTART window = sender as ASSY001_007_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true, window.NEW_PROD_LOT);
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            ASSY001_007_CONFIRM window = sender as ASSY001_007_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot();                
            }

            GetProductLot();
            GetEqptWrkInfo();

            this.grdMain.Children.Remove(window);
        }

        private void wndWait_Closed(object sender, EventArgs e)
        {
            ASSY001_007_WAITLOT window = sender as ASSY001_007_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        private void wndTrayCreate_Closed(object sender, EventArgs e)
        {
            ASSY001_007_TRAY_CREATE window = sender as ASSY001_007_TRAY_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                // tray 생성 후 trace 모드인 경우는 cell 팝업 호출.
                if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                {
                    ASSY001_007_CELL_LIST wndCellList = new ASSY001_007_CELL_LIST();
                    wndCellList.FrameOperation = FrameOperation;

                    if (wndCellList != null)
                    {
                        object[] Parameters = new object[8];
                        Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[1] = cboEquipment.SelectedValue.ToString();
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                        Parameters[4] = Util.NVC(window.CREATE_TRAYID);
                        Parameters[5] = Util.NVC(window.CREATE_TRAY_QTY);
                        Parameters[6] = Util.NVC(window.CREATE_OUT_LOT);
                        Parameters[7] = false;  // View Mode. (Read Only)

                        C1WindowExtension.SetParameters(wndCellList, Parameters);

                        wndCellList.Closed += new EventHandler(wndCellList_Closed);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
                        //grdMain.Children.Add(wndCellList);
                        //wndCellList.BringToFront();
                    }
                }

                GetProductLot();
                //GetOutTray();
            }

            this.grdMain.Children.Remove(window);
        }

        private void wndTrayMove_Closed(object sender, EventArgs e)
        {
            ASSY001_007_TRAY_MOVE window = sender as ASSY001_007_TRAY_MOVE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetOutTray();
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndCellList_Closed(object sender, EventArgs e)
        {
            ASSY001_007_CELL_LIST window = sender as ASSY001_007_CELL_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetWorkOrder(); // 작지 생산수량 정보 재조회.

            GetProductLot();

            this.grdMain.Children.Remove(window);
        }

        private void wndTrayHoldList_Closed(object sender, EventArgs e)
        {
            ASSY001_007_TRAYHOLD window = sender as ASSY001_007_TRAYHOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetWorkOrder(); // 작지 생산수량 정보 재조회.

            GetProductLot();

            this.grdMain.Children.Remove(window);
        }

        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        private void wndDefect_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DEFECT window = sender as CMM_ASSY_DEFECT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetProductLot();

            this.grdMain.Children.Remove(window);
        }

        private void wndEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND window = sender as CMM_ASSY_EQPT_COND;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        private void wndCancelTerm_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        private void wndCancelTermCST_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM_CST window = sender as CMM_ASSY_CANCEL_TERM_CST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                //txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                //txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                //txtWorker.Tag = Util.NVC(wndPopup.USERID);
                //txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                //txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                //txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);                

                GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(wndPopup);
        }

        private void wndEqpEnd_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPEND window = sender as CMM_ASSY_EQPEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndQuality_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY2 window = sender as CMM_ASSY_QUALITY2;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndAbnormal_Closed(object sender, EventArgs e)
        {
            ASSY001_007_ABNORMAL window = sender as ASSY001_007_ABNORMAL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_COM_QUALITY window = sender as CMM_COM_QUALITY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                this.Dispatcher.BeginInvoke(new Action(() => dispatcherTimer_QC_Tick(this.dispatcherTimer_QC, null)));
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndQualityInput_New_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP window = sender as CMM_COM_SELF_INSP;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                this.Dispatcher.BeginInvoke(new Action(() => dispatcherTimer_QC_Tick(this.dispatcherTimer_QC, null)));
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        private void wndInputObject_Closed(object sender, EventArgs e)
        {
            ASSY001_007_INPUT_OBJECT window = sender as ASSY001_007_INPUT_OBJECT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                this.grdMain.Children.Remove(window);

                try
                {
                    ShowLoadingIndicator();

                    DataTable inTable = _Biz.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue); ;
                    inTable.Rows.Add(newRow);

                    DataTable dtCurrIn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_IN_LOT_LIST", "INDATA", "OUTDATA", inTable);
                    if (dtCurrIn.Rows != null && dtCurrIn.Rows.Count > 0)
                    {
                        string prodLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        DataRow[] drs = dtCurrIn.Select("MOUNT_MTRL_TYPE_CODE = 'PROD' AND WIPSTAT = 'PROC' AND PROD_LOTID = '" + prodLotID + "'");
                        if (drs.Length > 0)
                        {
                            object[] parameters = new object[2];
                            parameters[0] = Util.NVC(drs[0]["EQPT_MOUNT_PSTN_NAME"]);
                            parameters[1] = Util.NVC(drs[0]["INPUT_LOTID"]);

                            Util.MessageValidation("SFU1282", parameters);
                            return;
                        }

                    }

                    ConfirmProcess();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }
            else
            {
                this.grdMain.Children.Remove(window);
            }
        }

        private void wndTrayClean_Closed(object sender, EventArgs e)
        {
            ASSY001_007_TRAY_CLEANING_MGT window = sender as ASSY001_007_TRAY_CLEANING_MGT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOutCreate);
            listAuth.Add(btnOutDel);
            listAuth.Add(btnOutConfirmCancel);
            listAuth.Add(btnOutConfirm);
            listAuth.Add(btnOutSave);
            listAuth.Add(btnOutTraySplSave);
            listAuth.Add(btnTestFullTrayCreate);
            listAuth.Add(btnTestFullTrayAllConfirm);

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

        private void SetInputWindow()
        {
            if (grdInput.Children.Count == 0)
            {
                string sEqsg = "";
                string sEqpt = "";
                if (cboEquipmentSegment != null && cboEquipmentSegment.SelectedValue != null)
                    sEqsg = cboEquipmentSegment.SelectedValue.ToString();
                if (cboEquipment != null && cboEquipment.SelectedValue != null)
                    sEqpt = cboEquipment.SelectedValue.ToString();

                if (winInput == null)
                    winInput = new UC_IN_OUTPUT(Process.PACKAGING, sEqsg, sEqpt);

                winInput.FrameOperation = FrameOperation;

                winInput._UCParent = this;
                grdInput.Children.Add(winInput);

                // Lot 식별 기준 코드에 따른 컨트롤 변경.
                SetInOutCtrlByLotIdentBasCode();
            }
        }

        public void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.PACKAGING;

            winWorkOrder.GetWorkOrder();
        }

        private void GetInputAllInfo(int iRow)
        {
            if (winInput == null)
                return;

            if (iRow < 0 || iRow > dgProductLot.Rows.Count)
                return;

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROD_LOTID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
            winInput.PROD_WIPSEQ = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
            winInput.PROD_WOID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WOID"));
            winInput.PROD_WODTLID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WO_DETL_ID"));

            winInput.SearchAll();
        }

        private void GetCurrInputLotList()
        {
            if (winInput == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1)
            {
                winInput.PROD_LOTID = "";
                winInput.PROD_WIPSEQ = "";
                winInput.PROD_WOID = "";
                winInput.PROD_WODTLID = "";
                winInput.PROD_LOT_STAT = "";
            }

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROCID = Process.PACKAGING;

            winInput.GetCurrInList();
        }
        private void GetWaitBoxList()
        {
            if (winInput == null)
                return;

            if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1)
            {
                winInput.PROD_LOTID = "";
                winInput.PROD_WIPSEQ = "";
                winInput.PROD_WOID = "";
                winInput.PROD_WODTLID = "";
                winInput.PROD_LOT_STAT = "";
            }

            winInput.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winInput.EQPTID = cboEquipment.SelectedValue.ToString();
            winInput.PROCID = Process.PACKAGING;

            winInput.GetWaitBox();
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.PACKAGING;
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

        private DataRow GetSelectWorkOrderInfo()
        {
            if (winWorkOrder == null)
                return null;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.PACKAGING;

            return winWorkOrder.GetSelectWorkOrderRow();
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
                ClearDetailControls();

                if (winInput != null)
                {
                    winInput.ClearDataGrid();
                }

                bRet = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                bRet = false;
            }
            return bRet;
        }

        private void ClearDetailControls()
        {
            Util.gridClear(dgOut);
        }

        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            ClearDetailControls();

            if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", iRow))
            {
                return;
            }

            GetInputAllInfo(iRow);

            ProcessDetail(dgProductLot.Rows[iRow].DataItem);
        }

        private void ProcessDetail(object obj)
        {
            DataRowView rowview = (obj as DataRowView);

            if (rowview == null)
                return;

            GetOutTray();
        }

        private void TrayConfirmProcess()
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

            if (idx < 0)
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU2044", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    TrayConfirm();
                }
            });

        }

        private void ChangeCellInfo()
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

            if (idx < 0)
                return;

            string sTrayQty = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CST_CELL_QTY"));//cboTrayType.SelectedValue == null ? "25" : cboTrayType.SelectedValue.ToString();
            string trayID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID")).Replace("\0", "");
            string outLOTID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));

            ASSY001_007_CELL_LIST wndCellList = new ASSY001_007_CELL_LIST();
            wndCellList.FrameOperation = FrameOperation;

            if (wndCellList != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[4] = trayID;
                Parameters[5] = sTrayQty;
                Parameters[6] = outLOTID;
                Parameters[7] = false;  // View Mode. (Read Only)

                C1WindowExtension.SetParameters(wndCellList, Parameters);

                wndCellList.Closed += new EventHandler(wndCellList_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
                //grdMain.Children.Add(wndCellList);
                //wndCellList.BringToFront();
            }


            //string strTrayPrefix = string.Empty;

            //if (eqptID.Trim() == "N1APKG417")
            //{
            //    strTrayPrefix = "LBCD";
            //    if (trayID.Substring(0, 4) == "LJTD")
            //    {
            //        strTrayPrefix = "LJTD";
            //    }

            //}
            //else if (eqptID.Trim() == "N1APKG418")
            //{
            //    strTrayPrefix = "LBDD";
            //    if (trayID.Substring(0, 4) == "LJUD")
            //    {
            //        strTrayPrefix = "LJUD";
            //    }
            //}
            //else if (eqptID.Trim() == "N1APKG419")
            //{
            //    strTrayPrefix = "LBED";
            //    if (trayID.Substring(0, 4) == "LJVD")
            //    {
            //        strTrayPrefix = "LJVD";
            //    }
            //}
            //else if (eqptID.Trim() == "N1APKG420")
            //{
            //    strTrayPrefix = "LBFD";
            //    if (trayID.Substring(0, 4) == "LJWD")
            //    {
            //        strTrayPrefix = "LJWD";
            //    }
            //}

            //if (strTrayPrefix != "" && trayID.IndexOf(strTrayPrefix) == 0)
            //{
            //    frmCNCellIDList8.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //}
            //else
            //{
            //    switch (cboTrayType.SelectedValue.ToString())
            //    {
            //        case "43":
            //            frmCNCellIDList9.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "88":
            //            frmCNCellIDList.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "108":
            //            frmCNCellIDList2.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "151":
            //            frmCNCellIDList3.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "66":
            //            frmCNCellIDList4.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "110":
            //            frmCNCellIDList5.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "128":
            //            frmCNCellIDList6.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "132":
            //            frmCNCellIDList7.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        case "64":
            //            frmCNCellIDList10.Show(gPackageLot, eqptID, trayID, trayConfirm, chkCurrentModel);
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }

        private void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    // 확정 시 저장, 삭제 버튼 비활성화
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN ")) // 활성화입고
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                }
                else
                {
                    btnOutCreate.IsEnabled = true;
                    btnOutDel.IsEnabled = true;
                    btnOutConfirmCancel.IsEnabled = true;
                    btnOutConfirm.IsEnabled = true;
                    btnOutCell.IsEnabled = true;
                    btnOutSave.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    if (dgProductLot == null || dgProductLot.Rows.Count < 1)
                        return;

                    GetOutTray();
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

        /// <summary>
        /// 작업지시 공통 UserControl에서 작업지시 선택/선택취소 시 메인 화면 정보 재조회 처리
        /// 설비 선택하면 자동 조회 처리 기능 추가로 작지 변경시에도 자동 조회 되도록 구현.
        /// </summary>
        public void GetAllInfoFromChild()
        {
            GetProductLot();
        }

        private void ColorAnimationInRectangle()
        {
            try
            {
                if (_BottmMsgMode.Equals(_BRUSH.RED))
                {
                    recTestMode.Fill = redBrush;
                }
                else if (_BottmMsgMode.Equals(_BRUSH.YELLOW))
                {
                    recTestMode.Fill = yellowBrush;
                }
                else
                {
                    recTestMode.Fill = redBrush;
                    _BottmMsgMode = _BRUSH.RED;
                }

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1.0;
                opacityAnimation.To = 0.0;
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
                opacityAnimation.AutoReverse = true;
                opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTargetName(opacityAnimation, _BottmMsgMode);
                Storyboard.SetTargetProperty(
                    opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

                mouseLeftButtonDownStoryboard.Begin(this);
            }
            catch (Exception ex)
            {
            }
        }

        private void showBottomAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle();
        }
        
        private void ColorAnimationInSpecialTray()
        {
            recSpcTray.Fill = myAnimatedBrush;
            recSpcLot.Fill = myAnimatedBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, _BRUSH.FUCHSIA);
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        private void ConfirmProcess()
        {
            //if (!CheckInputEqptCond())
            if (CheckModelChange() && !CheckInputEqptCond())
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU2817", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ASSY001_007_CONFIRM wndConfirm = new ASSY001_007_CONFIRM();
                        wndConfirm.FrameOperation = FrameOperation;

                        if (wndConfirm != null)
                        {
                            object[] Parameters = new object[13];
                            Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[1] = cboEquipment.SelectedValue.ToString();
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                            Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

                            Parameters[5] = Util.NVC(txtShift.Text); //= Util.NVC(wndPopup.SHIFTNAME);
                            Parameters[6] = Util.NVC(txtShift.Tag); //= Util.NVC(wndPopup.SHIFTCODE);
                            Parameters[7] = Util.NVC(txtWorker.Text); //= Util.NVC(wndPopup.USERNAME);
                            Parameters[8] = Util.NVC(txtWorker.Tag); //= Util.NVC(wndPopup.USERID);                            
                            Parameters[9] = Util.NVC(txtShiftStartTime.Text); //= Util.NVC(wndPopup.WRKSTRTTIME);
                            Parameters[10] = Util.NVC(txtShiftEndTime.Text); //= Util.NVC(wndPopup.WRKENDTTIME);

                            Parameters[11] = _LDR_LOT_IDENT_BAS_CODE;
                            Parameters[12] = _UNLDR_LOT_IDENT_BAS_CODE;

                            C1WindowExtension.SetParameters(wndConfirm, Parameters);

                            wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                            // 팝업 화면 숨겨지는 문제 수정.
                            //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                            grdMain.Children.Add(wndConfirm);
                            wndConfirm.BringToFront();
                        }
                    }
                });
            }
            else
            {
                ASSY001_007_CONFIRM wndConfirm = new ASSY001_007_CONFIRM();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[13];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

                    Parameters[5] = Util.NVC(txtShift.Text); //= Util.NVC(wndPopup.SHIFTNAME);
                    Parameters[6] = Util.NVC(txtShift.Tag); //= Util.NVC(wndPopup.SHIFTCODE);
                    Parameters[7] = Util.NVC(txtWorker.Text); //= Util.NVC(wndPopup.USERNAME);
                    Parameters[8] = Util.NVC(txtWorker.Tag); //= Util.NVC(wndPopup.USERID);                            
                    Parameters[9] = Util.NVC(txtShiftStartTime.Text); //= Util.NVC(wndPopup.WRKSTRTTIME);
                    Parameters[10] = Util.NVC(txtShiftEndTime.Text); //= Util.NVC(wndPopup.WRKENDTTIME);

                    Parameters[11] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[12] = _UNLDR_LOT_IDENT_BAS_CODE;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
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

        private void SetInOutCtrlByLotIdentBasCode()
        {
            try
            {
                // Loader
                if (winInput != null)
                {
                    winInput.LDR_LOT_IDENT_BAS_CODE = _LDR_LOT_IDENT_BAS_CODE;

                    winInput.SetCtrlByLotIdentBasCode();
                }


                // Unloader                
                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dispatcherTimer_QC_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                bool bUseCheck = CheckUseQCAlarm();
                try
                {
                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    if (!bUseCheck)
                    {
                        if (dpcTmr != null)
                            dpcTmr.Stop();

                        GetTestMode();
                        return;
                    }

                    // 0이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    if (dgProductLot == null || dgProductLot.Rows.Count - dgProductLot.TopRows.Count < 1 ||
                        _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 1)
                        return;

                    if (CheckInputQCData())
                    {
                        // 인장강도 입력 Notice..
                        ShowBottomMsgArea("인장강도를입력하세요.", BOTTOM_MSG_TYPE.QC_NO_INPUT_ALARM);
                    }
                    else
                    {
                        //if (!bTestMode)
                        //    HideBottomMsgArea();

                        GetTestMode();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr != null && dpcTmr.Interval.TotalSeconds > 0 && bUseCheck)
                        dpcTmr.Start();
                }
            }));
        }

        private void ShowBottomMsgArea(string sMsg, BOTTOM_MSG_TYPE _TYPE)
        {
            try
            {
                txtBottomMsg.Text = ObjectDic.Instance.GetObjectName(sMsg);
                _BOTTOM_MSG_TYPE = _TYPE;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    switch (_TYPE)
                    {
                        case BOTTOM_MSG_TYPE.TEST_MODE:
                            _BottmMsgMode = _BRUSH.RED;
                            break;
                        case BOTTOM_MSG_TYPE.SHUTDOWN_MODE:
                            _BottmMsgMode = _BRUSH.YELLOW;
                            break;
                        case BOTTOM_MSG_TYPE.QC_NO_INPUT_ALARM:
                            _BottmMsgMode = _BRUSH.YELLOW;
                            break;
                        default:
                            _BottmMsgMode = _BRUSH.RED;
                            break;
                    }
                   
                    if (MainContents.RowDefinitions[2].Height.Value > 0)
                    {
                        ColorAnimationInRectangle();
                        return;
                    }

                    MainContents.RowDefinitions[1].Height = new GridLength(8);
                    LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                    gla.From = new GridLength(0, GridUnitType.Star);
                    gla.To = new GridLength(1, GridUnitType.Star);
                    gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                    gla.Completed += showBottomAnimationCompleted;
                    MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
                    
                }));                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HideBottomMsgArea()
        {
            _BOTTOM_MSG_TYPE = null;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideBottomAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            //bTestMode = false;
        }

        private void HideBottomAnimationCompleted(object sender, EventArgs e)
        {

        }
        #endregion

        #endregion

        private void btnReworkCell_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_007_Rework_CELL_LIST wndConfirm = new ASSY001_007_Rework_CELL_LIST();

            wndConfirm.FrameOperation = FrameOperation;

            if (wndConfirm != null)
            {
                //object[] Parameters = new object[11];
                //Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                //Parameters[1] = cboEquipment.SelectedValue.ToString();
                //Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                //Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                //Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

                //Parameters[5] = Util.NVC(txtShift.Text); //= Util.NVC(wndPopup.SHIFTNAME);
                //Parameters[6] = Util.NVC(txtShift.Tag); //= Util.NVC(wndPopup.SHIFTCODE);
                //Parameters[7] = Util.NVC(txtWorker.Text); //= Util.NVC(wndPopup.USERNAME);
                //Parameters[8] = Util.NVC(txtWorker.Tag); //= Util.NVC(wndPopup.USERID);                            
                //Parameters[9] = Util.NVC(txtShiftStartTime.Text); //= Util.NVC(wndPopup.WRKSTRTTIME);
                //Parameters[10] = Util.NVC(txtShiftEndTime.Text); //= Util.NVC(wndPopup.WRKENDTTIME);

                //C1WindowExtension.SetParameters(wndConfirm, Parameters);

                //wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                grdMain.Children.Add(wndConfirm);
                wndConfirm.BringToFront();
            }

        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                #region EDC BCR Reading Info
                if (winInput != null)
                {
                    winInput.EQPTSEGMENT = Util.NVC(cboEquipmentSegment.SelectedValue);

                    if (!(Util.NVC(cboEquipmentSegment.SelectedValue).Equals("SELECT") || Util.NVC(cboEquipmentSegment.SelectedValue).Equals("")))
                        winInput.CheckEDCVisibity();
                }
                #endregion

                if (cboEquipmentSegment.SelectedIndex > 0 && cboEquipmentSegment.Items.Count > cboEquipmentSegment.SelectedIndex)
                {
                    if (Util.NVC((cboEquipmentSegment.Items[cboEquipmentSegment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        GetLotIdentBasCode();

                        // Lot 식별 기준 코드에 따른 컨트롤 변경.
                        SetInOutCtrlByLotIdentBasCode();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!CanMerge())
                return;

            ASSY001_OUTLOT_MERGE wndMerge = new ASSY001_OUTLOT_MERGE();
            wndMerge.FrameOperation = FrameOperation;

            if (wndMerge != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = cboEquipmentSegment.Text;
                Parameters[4] = _LDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndMerge, Parameters);

                wndMerge.Closed += new EventHandler(wndMerge_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndMerge.ShowModal()));
            }
        }

        private bool CanMerge()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("설비를 선택 하세요.");
            //    Util.MessageValidation("SFU1673");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private void wndMerge_Closed(object sender, EventArgs e)
        {
            ASSY001_OUTLOT_MERGE window = sender as ASSY001_OUTLOT_MERGE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                return;

            this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
        }

        private void btnRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

            // 장비완료 여부 체크
            for (int i = dgProductLot.TopRows.Count; i < dgProductLot.Rows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", i)) continue;

                if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT")).Equals("EQPT_END"))
                {
                    Util.MessageValidation("SFU1864");  // 장비완료 상태의 LOT이 아닙니다.
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private void RunCompleteCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                for (int i = dgProductLot.TopRows.Count; i < dgProductLot.Rows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", i)) continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_EQPT_END_LOT", "INDATA", null, inTable, (bizResult, bizException) =>
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
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.dispatcherTimer != null)
                    this.dispatcherTimer.Stop();

                if (this.dispatcherTimer_QC != null)
                    this.dispatcherTimer_QC.Stop();

                //this.dispatcherTimer = null;
                //this.dispatcherTimer_QC = null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnQualityInput_New_Click(object sender, RoutedEventArgs e)
        {
            if (!CanQuality())
                return;

            CMM_COM_SELF_INSP wndQualityInput = new CMM_COM_SELF_INSP();
            wndQualityInput.FrameOperation = FrameOperation;

            if (wndQualityInput != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Process.PACKAGING;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[3] = Util.NVC(cboEquipment.Text);
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                wndQualityInput.Closed += new EventHandler(wndQualityInput_New_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                //grdMain.Children.Add(wndQualityInput);
                //wndQualityInput.BringToFront();
            }
        }

        private void txtBottomMsg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_BOTTOM_MSG_TYPE == null) return;

                if (_BOTTOM_MSG_TYPE.GetType() == typeof(BOTTOM_MSG_TYPE) && (BOTTOM_MSG_TYPE)_BOTTOM_MSG_TYPE == BOTTOM_MSG_TYPE.QC_NO_INPUT_ALARM)
                {
                    if (!CanQuality())
                        return;

                    CMM_COM_QUALITY wndQualityInput = new CMM_COM_QUALITY();
                    wndQualityInput.FrameOperation = FrameOperation;

                    if (wndQualityInput != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = Process.PACKAGING;
                        Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                        Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                        Parameters[3] = Util.NVC(cboEquipment.Text);
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                        C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                        wndQualityInput.Closed += new EventHandler(wndQualityInput_Closed);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                        //grdMain.Children.Add(wndQualityInput);
                        //wndQualityInput.BringToFront();
                    }
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void recTestMode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_BOTTOM_MSG_TYPE == null) return;

                if (_BOTTOM_MSG_TYPE.GetType() == typeof(BOTTOM_MSG_TYPE) && (BOTTOM_MSG_TYPE)_BOTTOM_MSG_TYPE == BOTTOM_MSG_TYPE.QC_NO_INPUT_ALARM)
                {
                    if (!CanQuality())
                        return;

                    CMM_COM_QUALITY wndQualityInput = new CMM_COM_QUALITY();
                    wndQualityInput.FrameOperation = FrameOperation;

                    if (wndQualityInput != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = Process.PACKAGING;
                        Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                        Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                        Parameters[3] = Util.NVC(cboEquipment.Text);
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));

                        C1WindowExtension.SetParameters(wndQualityInput, Parameters);

                        wndQualityInput.Closed += new EventHandler(wndQualityInput_Closed);

                        // 팝업 화면 숨겨지는 문제 수정.
                        this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
                        //grdMain.Children.Add(wndQualityInput);
                        //wndQualityInput.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void recTestMode_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                recTestMode.Cursor = null;

                if (_BOTTOM_MSG_TYPE == null) return;

                if (_BOTTOM_MSG_TYPE.GetType() == typeof(BOTTOM_MSG_TYPE) && (BOTTOM_MSG_TYPE)_BOTTOM_MSG_TYPE == BOTTOM_MSG_TYPE.QC_NO_INPUT_ALARM)
                {
                    recTestMode.Cursor = Cursors.Hand;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool AuthCheck()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "PROD_RSLT_MGMT,MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void btnCancelConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //Util.Alert("라인을 선택 하세요.");
                    Util.MessageValidation("SFU1223");
                    return;
                }

                if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //Util.Alert("설비를 선택 하세요.");
                    Util.MessageValidation("SFU1673");
                    return;
                }

                CMM_ASSY_CANCEL_CONFIRM_PROD wndCancelConfrim = new CMM_ASSY_CANCEL_CONFIRM_PROD();
                wndCancelConfrim.FrameOperation = FrameOperation;

                if (wndCancelConfrim != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = Process.PACKAGING;
                    Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                    Parameters[3] = Util.NVC(cboEquipment.Text);

                    C1WindowExtension.SetParameters(wndCancelConfrim, Parameters);

                    wndCancelConfrim.Closed += new EventHandler(wndCancelConfrim_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndCancelConfrim.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndCancelConfrim_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_CONFIRM_PROD window = sender as CMM_ASSY_CANCEL_CONFIRM_PROD;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                return;

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                return;

            GetProductLot();
            GetEqptWrkInfo();
        }

        private void btnFormationHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
                    {
                        //Util.Alert("선택된 항목이 없습니다.");
                        Util.MessageValidation("SFU1651");
                        return;
                    }

                    CMM_ASSY_FCS_HOLD wndFCSHold = new CMM_ASSY_FCS_HOLD();
                    wndFCSHold.FrameOperation = FrameOperation;

                    if (wndFCSHold != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = Process.PACKAGING;
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                       
                        C1WindowExtension.SetParameters(wndFCSHold, Parameters);

                        wndFCSHold.Closed += new EventHandler(wndFCSHold_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndFCSHold.ShowModal()));
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndFCSHold_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_FCS_HOLD window = sender as CMM_ASSY_FCS_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                GetEqptWrkInfo();
            }
        }

        private void btnCancelTermMulti_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_CANCEL_TERM_MULTI wndCancelTermMulti = new CMM_ASSY_CANCEL_TERM_MULTI();
            wndCancelTermMulti.FrameOperation = FrameOperation;

            if (wndCancelTermMulti != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Process.PACKAGING;

                C1WindowExtension.SetParameters(wndCancelTermMulti, Parameters);

                wndCancelTermMulti.Closed += new EventHandler(wndCancelTermMulti_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));
                grdMain.Children.Add(wndCancelTermMulti);
                wndCancelTermMulti.BringToFront();
            }


            //for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            //{
            //    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;


            //    DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));

            //    if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


            //    Dictionary<string, string> dicParam = new Dictionary<string, string>();

            //    //폴딩
            //    dicParam.Add("reportName", "Fold");
            //    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
            //    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
            //    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
            //    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
            //    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
            //    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
            //    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
            //    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
            //    dicParam.Add("TITLEX", "BASKET ID");

            //    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

            //    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
            //    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

            //    dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.

            //    dicList.Add(dicParam);

            //}
        }

        private void btnTrayCleaningMgt_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_007_TRAY_CLEANING_MGT wndTrayClean = new ASSY001_007_TRAY_CLEANING_MGT();
            wndTrayClean.FrameOperation = FrameOperation;

            if (wndTrayClean != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.PACKAGING;
                C1WindowExtension.SetParameters(wndTrayClean, Parameters);

                wndTrayClean.Closed += new EventHandler(wndTrayClean_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndWait.ShowModal()));
                grdMain.Children.Add(wndTrayClean);
                wndTrayClean.BringToFront();
            }
        }

        private void wndCancelTermMulti_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM_MULTI window = sender as CMM_ASSY_CANCEL_TERM_MULTI;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(window);
        }

        //ESMI 1동(A4) 1~5 Line 만 조회
        private bool IsCmiExceptLine()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_EXCEPT_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }
}
#endregion