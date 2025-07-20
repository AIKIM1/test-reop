/*************************************************************************************
 Created Date : 2019.04.15
      Creator : INS 김동일K
   Decription : CWA3동 증설 - Packaging 공정진척 화면 (ASSY0001.ASSY001_007 2019/04/15 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.15  INS 김동일K : Initial Created.
  2019.09.02  INS 김동일K : 공정진척(조립) > 패키지 생산반제품에 외관검사 판정 정보 추가 및 Cell 관리 팝업 외관검사 목록 추가
  2019.09.25  LG CNS 김대근 : 금형관리 탭 추가
  2020.01.20  INS 김동일 : 실적확정 취소 기능 추가
  2020.05.12  김동일 : [C20200511-000024] 작업조, 작업자 등록 변경
  2020.06.03  김동일 : [C20200602-000207] [CWA PI] 활성화 트레이 조회 기능 추가 (SI 진행 건)
  2020.08.21  신광희 : [C20200814-000102] 시생산 재공 표기 추가 및 추가기능 – 시생산 모드 설정/해제 Validation 주석처리
  2021.02.02  신광희   동별 공통코드 항목에 RE_INPUT_DFCT_CODE 등록된 동만 불량/Loss/물품청구 그리드 재투입 수량 및 재투입 정보의 불량 항목 보이도록 수정 함.
  2023.11.22  안유수 : E20231006-001025 특별 TARY 적용 관련 사유 콤보박스 추가 및 GROUP ID 발번 기능 추가
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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_007.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_007 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private bool bSetAutoSelTime = false;

        private bool bTestMode = false;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        // 전체 적용 여부 CCB 결과 없음. 지급 요청 건으로 하드 코딩.
        private List<string> _SELECT_USER_MODE_AREA = new List<string>(new string[] { "A7", "A9" });   // 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]

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
        public ASSY004_007()
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


            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.PACKAGING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_PROC");

            String[] sFilter1 = { Process.PACKAGING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT_NEW");

            // 자동 조회 시간 Combo
            String[] sFilter4 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;

        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                grdOutTranBtn.Visibility = Visibility.Collapsed;
                grdSpclTranBtn.Visibility = Visibility.Collapsed;
                btnRunStart.Visibility = Visibility.Collapsed;
                btnCancelConfirm.Visibility = Visibility.Collapsed;

                if (winInput == null)
                    return;

                winInput.InitializePermissionPerInputButton();
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
            
            HideBottomMsgArea();

            // 동별 공통코드 항목에 RE_INPUT_DFCT_CODE 등록된 동만 보이도록 처리
            if (CheckReInputDefectCode())
            {
                dgReInput.Columns["RESNNAME"].Visibility = Visibility.Visible;
                dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
            }
            else
            {
                dgReInput.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            SetWorkOrderWindow();

            SetInputWindow();

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
            
            #region [버튼 권한 적용에 따른 처리]
            GetButtonPermissionGroup();
            #endregion

            Logger.Instance.WriteLine("[btnTrayType]", LogCategory.UI, new object[] { btnTrayType.ToString(), btnTrayType.Content.ToString() });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPilotProdMode();

            if (!CanSearch())
            {
                HideLoadingIndicator();
                return;
            }

            // 기준정보 조회
            GetLotIdentBasCode();
            SetInOutCtrlByLotIdentBasCode();    // Lot 식별 기준 코드에 따른 컨트롤 변경.

            GetWorkOrder();
            GetProductLot();

            GetWrokCalander();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            ASSY004_COM_RUNSTART wndRunStart = new ASSY004_COM_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.PACKAGING;
                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.Closed += new EventHandler(wndRunStart_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
            }
        }

        private void btnRunComplete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunComplete())
                return;

            if (!ChkEqptCnfmType("EQPT_END_W"))
                return;

            ASSY004_COM_EQPTEND wndPop = new ASSY004_COM_EQPTEND();
            wndPop.FrameOperation = FrameOperation;

            if (wndPop != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = Process.PACKAGING;
                Parameters[1] = cboEquipmentSegment.SelectedValue;
                Parameters[2] = cboEquipment.SelectedValue;

                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
                Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;
                Parameters[7] = CheckProdQtyChgPermission();
                Parameters[8] = "N";

                C1WindowExtension.SetParameters(wndPop, Parameters);

                wndPop.Closed += new EventHandler(wndEqpEnd_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPop.ShowModal()));
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // WorkCalander 체크.
            GetWrokCalander();
            
            if (!CheckCanConfirm())
                return;

            if (!ChkEqptCnfmType("CONFIRM_W"))
                return;

            //if (!CanInputComplete())
            //    return;

            #region 불량 저장 체크
            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878", (action) =>
                    {
                        if (tbDefect.Visibility == Visibility.Visible)
                            tbDefect.IsSelected = true;
                    });
                    return;
                }
            }
            #endregion

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
            ASSY004_COM_WAITLOT wndWait = new ASSY004_COM_WAITLOT();
            wndWait.FrameOperation = FrameOperation;

            if (wndWait != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;
                C1WindowExtension.SetParameters(wndWait, Parameters);

                wndWait.Closed += new EventHandler(wndWait_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWait.ShowModal()));
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

                this.Dispatcher.BeginInvoke(new Action(() => popWait.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

                this.Dispatcher.BeginInvoke(new Action(() => wndEqpComment.ShowModal()));
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
            //winWorkOrder.PROCID = Process.PACKAGING;

            //winWorkOrder.ClearWorkOrderInfo();

            //ClearControls();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();
                GetPilotProdMode();

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

                    //if (Util.NVC(e.Cell.Column.Name).Equals("TRAYID"))
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //}
                    //else
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //}
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

        private void dgOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    if (sender == null) return;

            //    C1DataGrid dataGrid = (sender as C1DataGrid);
            //    Point pnt = e.GetPosition(null);
            //    C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

            //    if (cell != null)
            //    {
            //        if (cell.Column.Name == "TRAYID")
            //        {
            //            if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
            //            {
            //                CMM_ASSY_CELL_INFO wndCell = new CMM_ASSY_CELL_INFO();
            //                wndCell.FrameOperation = FrameOperation;

            //                if (wndCell != null)
            //                {
            //                    object[] Parameters = new object[2];
            //                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name));
            //                    Parameters[1] = "";// Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "CSTID"));

            //                    C1WindowExtension.SetParameters(wndCell, Parameters);

            //                    wndCell.Closed += new EventHandler(wndCell_Closed);

            //                    this.Dispatcher.BeginInvoke(new Action(() => wndCell.ShowModal()));
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
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

                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
            }
        }
                
        private void btnQualityInput_Click(object sender, RoutedEventArgs e)
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
                
                this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
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
                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCondSearch.ShowModal()));
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

            ASSY004_OUTLOT_MERGE wndMerge = new ASSY004_OUTLOT_MERGE();
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

        private void recTestMode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_BOTTOM_MSG_TYPE == null) return;

                if (_BOTTOM_MSG_TYPE.GetType() == typeof(BOTTOM_MSG_TYPE) && (BOTTOM_MSG_TYPE)_BOTTOM_MSG_TYPE == BOTTOM_MSG_TYPE.QC_NO_INPUT_ALARM)
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

                        this.Dispatcher.BeginInvoke(new Action(() => wndQualityInput.ShowModal()));
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
                Parameters[2] = Process.PACKAGING;

                C1WindowExtension.SetParameters(wndWipNote, Parameters);

                wndWipNote.Closed += new EventHandler(wndWipNote_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWipNote.ShowModal()));
            }
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

                //Tool 투입 이력 조회
                GetInputHistTool();
                this.Dispatcher.BeginInvoke(new Action(() => dispatcherTimer_QC_Tick(this.dispatcherTimer_QC, null)));
            }
        }

        private void dgProductLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell.Presenter == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null) return;

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTTYPE")), "X"))
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
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
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
                ASSY004_007_TRAY_CREATE wndTrayCreate = new ASSY004_007_TRAY_CREATE();
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

                    this.Dispatcher.BeginInvoke(new Action(() => wndTrayCreate.ShowModal()));
                }
            }
            else
            {
                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ASSY004_007_TRAY_CREATE wndTrayCreate = new ASSY004_007_TRAY_CREATE();
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

                            this.Dispatcher.BeginInvoke(new Action(() => wndTrayCreate.ShowModal()));
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
        
        private void btnOutConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirmCancel())
                return;

            //취소 하시겠습니까?
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

            //적용 하시겠습니까?
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetSpecialTray();
                }
            });
        }

        private void chkOutTraySpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutTrayReamrk != null)
            {
                txtOutTrayReamrk.Text = "";
            }
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //불량정보를 저장하시겠습니까?            
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("RESNQTY"))
                {
                    string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    if (sFlag == "Y")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //C1TabControl c1TabControl = sender as C1TabControl;
            //if (c1TabControl.IsLoaded)
            //{
            //    string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

            //    if (string.Equals(tabItem, "tbOutProduct"))
            //    {
            //        grdTrayLegend.Visibility = Visibility.Visible;
            //        grdDfctLegend.Visibility = Visibility.Collapsed;
            //    }
            //    else if (string.Equals(tabItem, "tbDefect"))
            //    {
            //        grdTrayLegend.Visibility = Visibility.Collapsed;
            //        grdDfctLegend.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        grdTrayLegend.Visibility = Visibility.Collapsed;
            //        grdDfctLegend.Visibility = Visibility.Collapsed;
            //    }
            //}
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

        private void btnDfctCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (idx < 0) return;

                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "DFCT_QTY_CHG_BLOCK_FLAG")), "Y"))
                    return;

                CMM_ASSY_DFCT_CELL_REG wndDfctCellReg = new CMM_ASSY_DFCT_CELL_REG();
                wndDfctCellReg.FrameOperation = FrameOperation;

                if (wndDfctCellReg != null)
                {
                    object[] Parameters = new object[7];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
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
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        public void GetProductLot(bool bSelPrv = true, string sNewLot = "")
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
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

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_CL_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                        //2019.11.01 김대근 : Tool투입이력 조회
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

                GetEIOInfo();
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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

            //    HideLoadingIndicator();
            //}
            //catch (Exception ex)
            //{
            //    HideLoadingIndicator();
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

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL_L", "INDATA", "OUTDATA", inTable);
                
                Util.GridSetData(dgOut, searchResult, FrameOperation, true);
                
                if (searchResult?.Copy()?.Select("PKG_VISUAL_EQPT_JUDG_VALUE <> ''")?.Length > 0)
                    dgOut.Columns["PKG_VISUAL_EQPT_JUDG_VALUE"].Visibility = Visibility.Visible;
                else
                    dgOut.Columns["PKG_VISUAL_EQPT_JUDG_VALUE"].Visibility = Visibility.Collapsed;

                if (searchResult?.Copy()?.Select("PKG_VISUAL_MANL_JUDG_VALUE <> ''")?.Length > 0)
                    dgOut.Columns["PKG_VISUAL_MANL_JUDG_VALUE"].Visibility = Visibility.Visible;
                else
                    dgOut.Columns["PKG_VISUAL_MANL_JUDG_VALUE"].Visibility = Visibility.Collapsed;

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                (dgOut.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt.Copy());

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
                HideLoadingIndicator();
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

                HideLoadingIndicator();
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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
                        HideLoadingIndicator();

                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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
                        HideLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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

                //HideLoadingIndicator();

                return dtResult;
            }
            catch (Exception ex)
            {
                //HideLoadingIndicator();
                Util.MessageException(ex);
                return null;
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
                HideLoadingIndicator();
            }
        }

        private bool CheckReInputDefectCode()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["COM_TYPE_CODE"] = "RE_INPUT_DFCT_CODE";
            inTable.Rows.Add(newRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                return true;
            }
            else
            {
                return false;
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
                Indata["PROCID"] = Process.PACKAGING;

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
                //HideLoadingIndicator();
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
                HideLoadingIndicator();
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
                        HideLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("INPUT_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];

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
                newRow["INPUT_ID"] = sInputLot;
                //newRow["MTRLID"] = sInMtrlID; // 자동투입 biz 변경.
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                //newRow["ACTQTY"] = Util.NVC(sInQty).Equals("") ? 0 : Convert.ToDecimal(sInQty); // 자동투입 biz 변경.
                //newRow["ACTQTY"] = 0;

                inMtrlTable.Rows.Add(newRow);


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_CL_L", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

                HideLoadingIndicator();

                GetProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");

                return true;
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
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
                HideLoadingIndicator();
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
                HideLoadingIndicator();
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
                HideLoadingIndicator();
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
                HideLoadingIndicator();
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
                //HideLoadingIndicator();
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
                HideLoadingIndicator();
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
                HideLoadingIndicator();
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
                                    SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                    break;
                                case "OUTPUT_W": // 생산반제품 사용 권한
                                    grdOutTranBtn.Visibility = Visibility.Visible;
                                    grdSpclTranBtn.Visibility = Visibility.Visible;
                                    break;
                                case "LOTSTART_W": // 작업시작 사용 권한
                                    btnRunStart.Visibility = Visibility.Visible;
                                    break;
                                case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                    btnCancelConfirm.Visibility = Visibility.Visible;
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

        private void SetDefect()
        {
            try
            {
                dgDefect.EndEdit();

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (idx < 0) return;

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    //newRow["A_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "A_TYPE_DFCT_QTY")));
                    //newRow["C_TYPE_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "C_TYPE_DFCT_QTY")));

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetProductLot();
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
                Util.MessageException(ex);

                HideLoadingIndicator();
            }
        }

        private void GetDefectInfo()
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDefect, searchResult, null, false);
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
                Util.MessageException(ex);

                HideLoadingIndicator();
            }

        }

        public void GetEqptLoss()
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0) return;

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
                newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));

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

                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, false);

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

                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetReInputInfo()
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_LOT_EQPT_RE_INPUT_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgReInput, searchResult, FrameOperation, false);                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                            ASSY004_007_INPUT_OBJECT wndInputObject = new ASSY004_007_INPUT_OBJECT();
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

                                this.Dispatcher.BeginInvoke(new Action(() => wndInputObject.ShowModal()));
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
                    HideLoadingIndicator();
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

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878", (action) =>
                    {
                        if (tbDefect.Visibility == Visibility.Visible)
                            tbDefect.IsSelected = true;
                    });
                    return bRet;
                }
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

            foreach (C1.WPF.DataGrid.DataGridRow row in dgDefect.Rows)
            {
                double dRsn, dOrgRsn = 0;

                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                if (dRsn != dOrgRsn)
                {
                    // 저장하지 않은 불량 정보가 있습니다.
                    Util.MessageValidation("SFU1878", (action) =>
                    {
                        if (tbDefect.Visibility == Visibility.Visible)
                            tbDefect.IsSelected = true;
                    });
                    return bRet;
                }
            }

            if (txtShift.Text.Trim().Equals("") || txtWorker.Text.Trim().Equals(""))
            {
                // 등록된 월력정보가 없습니다.
                Util.MessageValidation("SFU6040");
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

        private bool CanSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                Util.MessageValidation("SFU1578");      //불량 항목이 없습니다.
                return false;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
        #endregion

        #region [PopUp Event]
        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_RUNSTART window = sender as ASSY004_COM_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true, window.NEW_PROD_LOT);
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_CONFIRM window = sender as ASSY004_COM_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot();                
            }

            GetProductLot();            
        }

        private void wndWait_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_WAITLOT window = sender as ASSY004_COM_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndTrayCreate_Closed(object sender, EventArgs e)
        {
            ASSY004_007_TRAY_CREATE window = sender as ASSY004_007_TRAY_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                // tray 생성 후 trace 모드인 경우는 cell 팝업 호출.
                if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                {
                    ASSY004_007_CELL_LIST wndCellList = new ASSY004_007_CELL_LIST();
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
        }
        
        private void wndCellList_Closed(object sender, EventArgs e)
        {
            ASSY004_007_CELL_LIST window = sender as ASSY004_007_CELL_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetWorkOrder(); // 작지 생산수량 정보 재조회.

            GetProductLot();
        }

        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
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
        
        private void wndEqpEnd_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_EQPTEND window = sender as ASSY004_COM_EQPTEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }
        
        private void wndQualityInput_New_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP window = sender as CMM_COM_SELF_INSP;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                this.Dispatcher.BeginInvoke(new Action(() => dispatcherTimer_QC_Tick(this.dispatcherTimer_QC, null)));
            }
        }
        
        private void wndInputObject_Closed(object sender, EventArgs e)
        {
            ASSY004_007_INPUT_OBJECT window = sender as ASSY004_007_INPUT_OBJECT;
            if (window.DialogResult == MessageBoxResult.OK)
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
                    HideLoadingIndicator();
                }
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
                //GetEqptLoss();
            }
        }

        private void wndMerge_Closed(object sender, EventArgs e)
        {
            ASSY004_OUTLOT_MERGE window = sender as ASSY004_OUTLOT_MERGE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                return;

            this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
        }

        private void popWait_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WAITLOT_DELETE pop = sender as CMM_ASSY_WAITLOT_DELETE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndEqptCondSearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND_SEARCH window = sender as CMM_ASSY_EQPT_COND_SEARCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CELL_INFO window = sender as CMM_ASSY_CELL_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndFCSHold_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_FCS_HOLD window = sender as CMM_ASSY_FCS_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnEqptCond);
            listAuth.Add(btnQualityInput);
            listAuth.Add(btnEqpRemark);
            listAuth.Add(btnRunComplete);
            listAuth.Add(btnConfirm);
            //listAuth.Add(btnWaitLot);
            listAuth.Add(btnEqptCondSearch);
            listAuth.Add(btnMerge);
            listAuth.Add(btnWipNote);
            listAuth.Add(btnRunStart);
            listAuth.Add(btnCancelConfirm);
            listAuth.Add(btnTrayType);

            listAuth.Add(btnOutCreate);
            listAuth.Add(btnOutDel);
            listAuth.Add(btnOutConfirm);
            listAuth.Add(btnOutConfirmCancel);
            listAuth.Add(btnOutCell);
            listAuth.Add(btnOutSave);
            listAuth.Add(btnOutTraySplSave);
            listAuth.Add(btnDefectSave);
            listAuth.Add(btnFormationHold);


            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetInputHistTool()
        {
            if (winInput == null)
                return;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            string prodLotID = string.Empty;

            if (idx > -1)
                prodLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

            winInput.GetInputHistTool(prodLotID, Util.NVC(cboEquipment.SelectedValue));
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
            Util.gridClear(dgDefect);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgReInput);
        }

        private void ProdListClickedProcess(int iRow)
        {
            #region [버튼 권한 적용에 따른 처리]
            GetButtonPermissionGroup();
            #endregion

            if (iRow < 0)
                return;

            ClearDetailControls();

            if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", iRow))
            {
                return;
            }

            GetInputAllInfo(iRow);

            GetOutTray();
            GetDefectInfo();
            GetEqptLoss();

            if (tbReInput.Visibility == Visibility.Visible)
                GetReInputInfo();
        }

        private void TrayConfirmProcess()
        {
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

            if (idx < 0)
                return;

            //확정 하시겠습니까?
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

            ASSY004_007_CELL_LIST wndCellList = new ASSY004_007_CELL_LIST();
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
                
                this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
            }            
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

        public void HideLoadingIndicator()
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
                //해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?
                Util.MessageConfirm("SFU2817", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ASSY004_COM_CONFIRM wndConfirm = new ASSY004_COM_CONFIRM();
                        wndConfirm.FrameOperation = FrameOperation;

                        if (wndConfirm != null)
                        {
                            object[] Parameters = new object[13];
                            Parameters[0] = Process.PACKAGING;
                            Parameters[1] = cboEquipmentSegment.SelectedValue;
                            Parameters[2] = cboEquipment.SelectedValue;

                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                            Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                            Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
                            Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;

                            Parameters[7] = Util.NVC(txtShift.Text);
                            Parameters[8] = Util.NVC(txtShift.Tag);
                            Parameters[9] = Util.NVC(txtWorker.Text);
                            Parameters[10] = Util.NVC(txtWorker.Tag);
                            Parameters[11] = CheckProdQtyChgPermission();
                            Parameters[12] = "N";

                            C1WindowExtension.SetParameters(wndConfirm, Parameters);

                            wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                        }
                    }
                });
            }
            else
            {
                ASSY004_COM_CONFIRM wndConfirm = new ASSY004_COM_CONFIRM();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[13];
                    Parameters[0] = Process.PACKAGING;
                    Parameters[1] = cboEquipmentSegment.SelectedValue;
                    Parameters[2] = cboEquipment.SelectedValue;

                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    Parameters[5] = _LDR_LOT_IDENT_BAS_CODE;
                    Parameters[6] = _UNLDR_LOT_IDENT_BAS_CODE;

                    Parameters[7] = Util.NVC(txtShift.Text);
                    Parameters[8] = Util.NVC(txtShift.Tag);
                    Parameters[9] = Util.NVC(txtWorker.Text);
                    Parameters[10] = Util.NVC(txtWorker.Tag);
                    Parameters[11] = CheckProdQtyChgPermission();
                    Parameters[12] = "N";

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
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

        private void SetPermissionPerInputButton(string sBtnPermissionGrpCode)
        {
            if (winInput == null)
                return;

            winInput.SetPermissionPerButton(sBtnPermissionGrpCode);
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

                    wndCancelConfrim.Closed += new EventHandler(wndCancelConfirm_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndCancelConfrim.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndCancelConfirm_Closed(object sender, EventArgs e)
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
        }

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
                Parameters[3] = Process.PACKAGING;
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
                Indata["PROCID"] = Process.PACKAGING;

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

        private void btnTrayInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_PKG_TRAY_INFO wndTrayInfo = new CMM_PKG_TRAY_INFO();
                wndTrayInfo.FrameOperation = FrameOperation;

                if (wndTrayInfo != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Process.PACKAGING;

                    C1WindowExtension.SetParameters(wndTrayInfo, Parameters);

                    wndTrayInfo.Closed += new EventHandler(wndTrayInfo_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndTrayInfo.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void wndTrayInfo_Closed(object sender, EventArgs e)
        {
            CMM_PKG_TRAY_INFO window = sender as CMM_PKG_TRAY_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnTrayType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Logger.Instance.WriteLine("[object sender, RoutedEventArgs e]", LogCategory.UI, new object[] { e.GetString(), sender.GetString(), btnTrayType.ToString() });

                //Button btn = sender as Button;
                //if (btn == null || e == null || e.Source == null || e.RoutedEvent == null)
                //{
                //    return;
                //}

                //string selectedEqptId = cboEquipment.SelectedValue == null ? string.Empty : cboEquipment.SelectedValue.ToString();
                //Logger.Instance.WriteLine("[selectedEqptId]", LogCategory.UI, new object[] { selectedEqptId });

                //if (string.IsNullOrEmpty(selectedEqptId) || selectedEqptId.Equals("SELECT"))
                //{
                //    //설비를 선택하세요
                //    Util.MessageValidation("SFU1153");
                //    return;
                //}
                //Logger.Instance.WriteLine("[after validation eqptid]", LogCategory.UI, new object[] { selectedEqptId });

                //CMM_FORM_TRAY_TYPE wndTrayType = new CMM_FORM_TRAY_TYPE();
                //Logger.Instance.WriteLine("[before open popup]", LogCategory.UI, new object[] { wndTrayType.GetString() });

                //if (wndTrayType != null)
                //{
                //    wndTrayType.FrameOperation = FrameOperation;
                //    object[] parameters = new object[1];
                //    parameters[0] = selectedEqptId;

                //    C1WindowExtension.SetParameters(wndTrayType, parameters);
                //    Logger.Instance.WriteLine("[after set parameters]", LogCategory.UI, new object[] { wndTrayType.GetString() });

                //    wndTrayType.Closed -= wndTrayType_Closed;
                //    wndTrayType.Closed += wndTrayType_Closed;

                //    wndTrayType.ShowModal();
                //    Logger.Instance.WriteLine("[after open popup]", LogCategory.UI, new object[] { wndTrayType.GetString() });
                //    wndTrayType.CenterOnScreen();
                //    Logger.Instance.WriteLine("[after center popup]", LogCategory.UI, new object[] { wndTrayType.GetString() });
                //}

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
                ASSY004_007_FORMATION_REQUEST_BLOCK Format_Request = new ASSY004_007_FORMATION_REQUEST_BLOCK();
                Format_Request.FrameOperation = FrameOperation;

                if (Format_Request != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = cboEquipment.SelectedValue.ToString();

                    C1WindowExtension.SetParameters(Format_Request, Parameters);

                    Format_Request.Closed += new EventHandler(Format_Request_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => Format_Request.ShowModal()));
                }




            }
            catch (Exception ex)
            {
                //Logger.Instance.WriteLine("[Exception]", LogCategory.UI, new object[] { ex.Message.ToString(), ex.TargetSite.ToString(), ex.Source.ToString(), ex.InnerException.ToString(), ex.StackTrace.ToString() });
                Util.MessageException(ex);
            }
        }

        private void Format_Request_Closed(object sender, EventArgs e)
        {
            ASSY004_007_FORMATION_REQUEST_BLOCK window = sender as ASSY004_007_FORMATION_REQUEST_BLOCK;
            if (window.DialogResult == MessageBoxResult.OK)
            {
              
            }
        }
    }
}
