/*************************************************************************************
 Created Date : 2017.06.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.15  INS 김동일K : Initial Created.
  2023.02.16  김린겸 E20230207-000170 PKG NG interception V2
  2023.06.25  조영대      : 설비 Loss Level 2 Code 사용 체크 및 변환
  2023.11.27  오수현        E20231013-001274 동별 적합성 체크 예외 여부 조회하여 버튼 hidden 여부 설정
  2004.05.31  이병윤        E20240528-000578 NG Cell 추가, 삭제시 Caution 팝업 호출
  2024.08.08  유명환        E20240715-000211 Cell관리 삭제부분 한건처리에서 다중 처리되도록 수정
  2024.08.23  이병윤        E20240704-001582 ZZS/PKG HOLD 추가
  2024.09.02  이병윤        E20240802-001557 EDC 데이터 요청   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
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
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_007.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_007 : UserControl, IWorkArea
    {   
        #region Declaration & Constructor
        private bool bSetAutoSelTime = false;
        private bool bTestMode = false;
        private string sTestModeType = string.Empty;
        private bool bCellTraceMode = false;

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow); 
        SolidColorBrush myAnimatedBrush = new SolidColorBrush(Colors.Fuchsia);

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE(); 
        private UC_IN_OUTPUT_MOBILE winInput = null;
        ASSY003_OUTLOT_MERGE wndMerge;

        // E20240528-000578 NG Cell 추가 Visible 여부
        private bool bCellNg = false;

        private struct PRV_VALUES
        {
            public string sPrvTray;

            public PRV_VALUES(string sTray)
            {
                sPrvTray = sTray;
            }
        }

        private PRV_VALUES _PRV_VLAUES = new PRV_VALUES("");

        #region Popup 처리 로직 변경
        ASSY003_007_WAITLOT wndWaitLot;
        ASSY003_007_BOX_IN wndBoxIn;
        CMM_ASSY_QUALITY_PKG wndQualitySearch;
        CMM_ASSY_DEFECT wndDefect;
        CMM_ASSY_CANCEL_TERM wndCancelTerm;
        CMM_ASSY_PU_EQPT_COND wndEqptCond;
        CMM_COM_SELF_INSP wndQualityInput;
        CMM_COM_EQPCOMMENT wndEqpComment;
        ASSY003_007_RUNSTART wndRunStart;
        CMM_ASSY_EQPEND wndEqpEnd;
        ASSY003_007_CONFIRM wndConfirm;
        CMM_SHIFT_USER2 wndShiftUser;

        ASSY003_007_TRAY_CREATE wndTrayCreate;
        //ASSY003_007_CELL_LIST wndCellList;
        ASSY003_007_INPUT_OBJECT wndInputObject;
        ASSY003_007_TRAY_MOVE wndTrayMove;
        WND_CPROD_TRANSFER  wndCProduct;

        CMM_ASSY_CANCEL_CONFIRM_PROD wndCancelConfrim;
        #endregion
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
        public ASSY003_007()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 특별 TRAY  사유 Combo
            String[] sFilter3 = { "SPCL_RSNCODE" };
            _combo.SetCombo(cboOutTraySplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");

            //String[] sFilter3 = { LoginInfo.CFG_AREA_ID, "SPCL_RSNCODE" };
            //_combo.SetCombo(cboOutTraySplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "AREA_COMMON_CODE");

            if (cboOutTraySplReason != null && cboOutTraySplReason.Items != null && cboOutTraySplReason.Items.Count > 0)
                cboOutTraySplReason.SelectedIndex = 0;


            String[] sFilter = { LoginInfo.CFG_AREA_ID, null, Process.PACKAGING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "cboEquipmentSegmentAssy");

            String[] sFilter1 = { Process.PACKAGING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT_MAIN_LEVEL");

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
            this.RegisterName("redBrush", redBrush);
            this.RegisterName("myAnimatedBrush", myAnimatedBrush);
            this.RegisterName("yellowBrush", yellowBrush); 


            InitCombo();

            // Cell Trace Mode 설정            
            //if (bCellTraceMode)
            //{
            //    rdoTraceUse.IsChecked = true;
            //    rdoTraceNotUse.IsEnabled = false;
            //}
            //else
            //{
            //    rdoTraceUse.IsEnabled = false;
            //    rdoTraceNotUse.IsChecked = true;
            //}

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

            //HideTestMode();

            // E20240528-000578 NG Cell 추가 Visible 여부
            if(LoginInfo.SYSID.Equals("GMES-S-N5") || LoginInfo.SYSID.Equals("GMES-S-N6"))
            {
                bCellNg = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region Popup 처리 로직 변경
            if (wndWaitLot != null)
                wndWaitLot.BringToFront();

            if (wndQualitySearch != null)
                wndQualitySearch.BringToFront();

            if (wndDefect != null)
                wndDefect.BringToFront();

            if (wndCancelTerm != null)
                wndCancelTerm.BringToFront();

            if (wndEqptCond != null)
                wndEqptCond.BringToFront();

            if (wndQualityInput != null)
                wndQualityInput.BringToFront();

            if (wndEqpComment != null)
                wndEqpComment.BringToFront();

            if (wndRunStart != null)
                wndRunStart.BringToFront();

            if (wndEqpEnd != null)
                wndEqpEnd.BringToFront();

            if (wndConfirm != null)
                wndConfirm.BringToFront();

            if (wndShiftUser != null)
                wndShiftUser.BringToFront();

            if (wndTrayCreate != null)
                wndTrayCreate.BringToFront();

            //if (wndCellList != null)
            //    wndCellList.BringToFront();

            if (wndInputObject != null)
                wndInputObject.BringToFront();

            if (wndTrayMove != null)
                wndTrayMove.BringToFront();

            if (wndBoxIn != null)
                wndBoxIn.BringToFront();

            if (wndCancelConfrim != null)
                wndCancelConfrim.BringToFront();
            #endregion

                //GetTestMode();

            ApplyPermissions();

            //// Cell Trace Mode 설정 
            //GetCellTraceFlag();

            SetWorkOrderWindow();

            SetInputWindow();

            // 생산 반제품 조회 Timer Start.
            if (dispatcherTimer != null)
                dispatcherTimer.Start();

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

            GetWorkOrder();
            GetProductLot();

            GetEqptWrkInfo();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            if (wndRunStart != null)
                wndRunStart = null;

            wndRunStart = new ASSY003_007_RUNSTART();
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

            if (wndEqpEnd != null)
                wndEqpEnd = null;

            wndEqpEnd = new CMM_ASSY_EQPEND();
            wndEqpEnd.FrameOperation = FrameOperation;

            if (wndEqpEnd != null)
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
                C1WindowExtension.SetParameters(wndEqpEnd, Parameters);

                wndEqpEnd.Closed += new EventHandler(wndEqpEnd_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqpEnd.ShowModal()));                
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanConfirm())
                {
                    return;
                }

                if (ChkInLotMustComplete() == false)
                {
                    Util.MessageValidation("SFU6022");  //투입LOT을 투입완료하거나 잔량처리 해주세요.
                    return;
                }

                // Loss 입력 여부 체크.
                DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.PACKAGING);

                if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                {
                    string sInfo = string.Empty;
                    string sLossInfo = string.Empty;

                    for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                    {
                        sInfo = Util.NVC(dtEqpLossInfo.Rows[iCnt]["JOBDATE"]) + " : " + Util.NVC(dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"]);
                        sLossInfo = sLossInfo + "\n" + sInfo;
                    }

                    object[] param = new object[] { sLossInfo };

                    // 미입력한 설비 Loss가 존재합니다. 확정하시겠습니까? %1
                    Util.MessageConfirm("SFU3501", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (!CanInputComplete())
                                return;

                            ConfirmProcess();
                        }
                    }, param);
                }
                else
                {
                    if (!CanInputComplete())
                        return;

                    ConfirmProcess();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            if (wndWaitLot != null)
                wndWaitLot = null;

            wndWaitLot = new ASSY003_007_WAITLOT();
            wndWaitLot.FrameOperation = FrameOperation;

            if (wndWaitLot != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.PACKAGING;
                C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                wndWaitLot.Closed += new EventHandler(wndWait_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
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

            if (wndEqpComment != null)
                wndEqpComment = null;

            wndEqpComment = new CMM_COM_EQPCOMMENT();
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
            //ASSY003_007_ABNORMAL wndPopup = new ASSY003_007_ABNORMAL();
            //wndPopup.FrameOperation = FrameOperation;

            //if (wndPopup != null)
            //{
            //    object[] Parameters = new object[4];
            //    Parameters[0] = Process.PACKAGING;
            //    Parameters[1] = cboEquipment.SelectedValue;

            //    C1WindowExtension.SetParameters(wndPopup, Parameters);

            //    wndPopup.Closed += new EventHandler(wndAbnormal_Closed);

            //    // 팝업 화면 숨겨지는 문제 수정.
            //    //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            //    grdMain.Children.Add(wndPopup);
            //    wndPopup.BringToFront();
            //}
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
            Util.MessageInfo("사용 불가 합니다.");
            return;

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
                                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_OUT_LOT_CL_S", "IN_EQP,IN_INPUT,IN_SPCL", "OUT_LOT", indataSet);

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

                                indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S();
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

                                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S", "IN_EQP,IN_CST,IN_EL", null, indataSet);
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
                            if (dgOut == null || dgOut.Rows.Count - dgOut.TopRows.Count - dgOut.BottomRows.Count < 1)
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

                                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_OUT_LOT_CL_S", "IN_EQP,IN_CST", null, indataSet);
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

            if (wndDefect != null)
                wndDefect = null;

            wndDefect = new CMM_ASSY_DEFECT();
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
                
                this.Dispatcher.BeginInvoke(new Action(() => wndDefect.ShowModal()));                
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

        private void btnCProdBoxIn_Click(object sender, RoutedEventArgs e)
        {

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

            if (wndEqptCond != null)
                wndEqptCond = null;

            wndEqptCond = new CMM_ASSY_PU_EQPT_COND();
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

        private void btnCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            if (wndCancelTerm != null)
                wndCancelTerm = null;

            wndCancelTerm = new CMM_ASSY_CANCEL_TERM();
            wndCancelTerm.FrameOperation = FrameOperation;

            if (wndCancelTerm != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Process.PACKAGING;

                C1WindowExtension.SetParameters(wndCancelTerm, Parameters);

                wndCancelTerm.Closed += new EventHandler(wndCancelTerm_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndCancelTerm.ShowModal()));                
            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            if (wndShiftUser != null)
                wndShiftUser = null;

            wndShiftUser = new CMM_SHIFT_USER2();
            wndShiftUser.FrameOperation = this.FrameOperation;

            if (wndShiftUser != null)
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

                C1WindowExtension.SetParameters(wndShiftUser, Parameters);

                wndShiftUser.Closed += new EventHandler(wndShift_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndShiftUser.ShowModal()));
            }
        }

        private void btnTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTestMode()) return;

            if (bTestMode)
            {
                //if (SetTestMode(bMode: false, sMode: "TEST"))
                //    HideTestMode();
                SetTestMode(false);
                GetTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // 테스트 Run이 되면 실적처리가 되지 않습니다. 테스트 Run 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

                        //if (SetTestMode(bMode: true, sMode: "TEST"))
                        //    ShowTestMode();
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

            if (wndQualitySearch != null)
                wndQualitySearch = null;

            wndQualitySearch = new CMM_ASSY_QUALITY_PKG();
            wndQualitySearch.FrameOperation = FrameOperation;

            if (wndQualitySearch != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = Process.PACKAGING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndQualitySearch, Parameters);

                wndQualitySearch.Closed += new EventHandler(wndQualityRslt_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQualitySearch.ShowModal()));
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

        private void btnBoxIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanBoxIn()) return;

                if (wndBoxIn != null)
                    wndBoxIn = null;

                wndBoxIn = new ASSY003_007_BOX_IN();
                wndBoxIn.FrameOperation = FrameOperation;

                if (wndBoxIn != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Process.PACKAGING;
                    C1WindowExtension.SetParameters(wndBoxIn, Parameters);

                    wndBoxIn.Closed += new EventHandler(wndBoxIn_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndBoxIn.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndBoxIn_Closed(object sender, EventArgs e)
        {
            wndBoxIn = null;
            ASSY003_007_BOX_IN window = sender as ASSY003_007_BOX_IN;
            if (window.DialogResult == MessageBoxResult.OK)
            {

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
                    //else
                    //{
                    //    switch (Convert.ToString(e.Cell.Column.Name))
                    //    {
                    //        case "CELLQTY":

                    //            int iColIdx = dg.Columns["CHK"].Index;
                    //            C1.WPF.DataGrid.DataGridCell dgTmpCell = dg.GetCell(e.Cell.Row.Index, iColIdx); // Checkbox

                    //            chk = dgTmpCell.Presenter.Content as CheckBox;
                    //            if (chk != null)
                    //            {
                    //                if (dgTmpCell.Presenter != null &&
                    //                    dgTmpCell.Presenter.Content != null &&
                    //                    (dgTmpCell.Presenter.Content as CheckBox) != null &&
                    //                    (dgTmpCell.Presenter.Content as CheckBox).IsChecked.HasValue &&
                    //                    !(bool)(dgTmpCell.Presenter.Content as CheckBox).IsChecked)
                    //                {
                    //                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    //                    {
                    //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                    //                        chk.IsChecked = true;

                    //                        // 이전 값 저장.
                    //                        _PRV_VLAUES.sPrvTray = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));

                    //                        SetOutTrayButtonEnable(e.Cell.Row);

                    //                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                    //                        {
                    //                            if (e.Cell.Row.Index != idx)
                    //                            {
                    //                                if (dg.GetCell(idx, iColIdx).Presenter != null &&
                    //                                    dg.GetCell(idx, iColIdx).Presenter.Content != null &&
                    //                                    (dg.GetCell(idx, iColIdx).Presenter.Content as CheckBox) != null)
                    //                                {
                    //                                    (dg.GetCell(idx, iColIdx).Presenter.Content as CheckBox).IsChecked = false;
                    //                                }
                    //                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //                else if (dgTmpCell.Presenter != null &&
                    //                         dgTmpCell.Presenter.Content != null &&
                    //                         (dgTmpCell.Presenter.Content as CheckBox) != null &&
                    //                         (dgTmpCell.Presenter.Content as CheckBox).IsChecked.HasValue &&
                    //                         (bool)(dgTmpCell.Presenter.Content as CheckBox).IsChecked)
                    //                {
                    //                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    //                    {
                    //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                    //                        chk.IsChecked = false;

                    //                        _PRV_VLAUES.sPrvTray = "";

                    //                        // 확정 시 저장, 삭제 버튼 비활성화
                    //                        SetOutTrayButtonEnable(null);
                    //                    }
                    //                }
                    //            }
                    //            break;
                    //    }

                    //    if (dgOut.CurrentCell != null)
                    //        dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                    //    else if (dgOut.Rows.Count > 0)
                    //        dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                    //}
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

        private void dgOut_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null || e.Cell.Row == null || e.Cell.Row.DataItem == null || e.Cell.Column == null)
                    return;

                if (e.Cell.Column.Name.Equals("CELLQTY") ||
                    e.Cell.Column.Name.Equals("SPECIALDESC") ||
                    e.Cell.Column.Name.Equals("CBO_SPCL")
                   )
                {
                    C1DataGrid dg = e.Cell.DataGrid;

                    int iColIdx = dg.Columns["CHK"].Index;
                    C1.WPF.DataGrid.DataGridCell dgTmpCell = dg.GetCell(e.Cell.Row.Index, iColIdx); // Checkbox

                    CheckBox chk = dgTmpCell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        if (dgTmpCell.Presenter != null &&
                            dgTmpCell.Presenter.Content != null &&
                            (dgTmpCell.Presenter.Content as CheckBox) != null &&
                            (dgTmpCell.Presenter.Content as CheckBox).IsChecked.HasValue &&
                            !(bool)(dgTmpCell.Presenter.Content as CheckBox).IsChecked)
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
                                        if (dg.GetCell(idx, iColIdx).Presenter != null &&
                                            dg.GetCell(idx, iColIdx).Presenter.Content != null &&
                                            (dg.GetCell(idx, iColIdx).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, iColIdx).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["OUT_LOTID"].Index);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns["OUT_LOTID"].Index);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgOut.EndEdit();

                if (!CanTrayConfirm())
                    return;

                TrayConfirmProcess();

                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                if (wndTrayCreate != null)
                    wndTrayCreate = null;

                wndTrayCreate = new ASSY003_007_TRAY_CREATE();
                wndTrayCreate.FrameOperation = FrameOperation;

                if (wndTrayCreate != null)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    Parameters[4] = bCellTraceMode ? "Y" : "N";// (bool)rdoTraceUse.IsChecked ? "Y" : "N";
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
                        if (wndTrayCreate != null)
                            wndTrayCreate = null;

                        wndTrayCreate = new ASSY003_007_TRAY_CREATE();
                        wndTrayCreate.FrameOperation = FrameOperation;

                        if (wndTrayCreate != null)
                        {
                            object[] Parameters = new object[7];
                            Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                            Parameters[1] = cboEquipment.SelectedValue.ToString();
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                            Parameters[4] = bCellTraceMode ? "Y" : "N";// (bool)rdoTraceUse.IsChecked ? "Y" : "N";
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

        private void btnOutMove_Click(object sender, RoutedEventArgs e)
        {
            if (!CanTrayMove())
                return;

            if (wndTrayMove != null)
                wndTrayMove = null;

            wndTrayMove = new ASSY003_007_TRAY_MOVE();
            wndTrayMove.FrameOperation = FrameOperation;

            if (wndTrayMove != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "TRAYID")).Replace("\0", "");
                Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "OUT_LOTID"));


                C1WindowExtension.SetParameters(wndTrayMove, Parameters);

                wndTrayMove.Closed += new EventHandler(wndTrayMove_Closed);
                                
                this.Dispatcher.BeginInvoke(new Action(() => wndTrayMove.ShowModal()));
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

        //private void rdoTraceNotUse_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (dgOut == null)
        //        return;

        //    if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
        //    {
        //        dgOut.Columns["CELLQTY"].IsReadOnly = false;
        //        txtTraceMode.Text = "(Not Trace mode)";
        //    }
        //    else
        //    {
        //        dgOut.Columns["CELLQTY"].IsReadOnly = true;
        //        txtTraceMode.Text = "(Trace mode)";
        //    }
        //}

        //private void rdoTraceUse_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (dgOut == null)
        //        return;

        //    if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
        //    {
        //        dgOut.Columns["CELLQTY"].IsReadOnly = true;
        //        txtTraceMode.Text = "(Trace mode)";
        //    }
        //    else
        //    {
        //        dgOut.Columns["CELLQTY"].IsReadOnly = false;
        //        txtTraceMode.Text = "(Not Trace mode)";
        //    }            
        //}

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

        private void chkOutTraySpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutTrayReamrk != null)
            {
                txtOutTrayReamrk.Text = "";
            }
        }
        #endregion


        private void txtTrayID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    CreateTray();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtTrayID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtTrayID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtTrayQty.Text, 0))
                {
                    txtTrayQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    CreateTray();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreateTray()
        {
            try
            {
                if (!CanCreateTrayByTrayID())
                    return;

                //Util.MessageConfirm("SFU1621", result =>
                //{
                //    if (result == MessageBoxResult.OK)
                //    {
                string sCreateTrayID, sCreateTrayQty, sCreateOutLot;
                if (CreateTray(out sCreateOutLot, out sCreateTrayID, out sCreateTrayQty))
                {
                    //셀 추적용
                    if (bCellTraceMode)
                    {
                        ShowCellInfoPopup(sCreateTrayID, sCreateOutLot, sCreateTrayQty);

                        //if (wndCellList != null)
                        //    wndCellList = null;

                        //wndCellList = new ASSY003_007_CELL_LIST();
                        //wndCellList.FrameOperation = FrameOperation;

                        //if (wndCellList != null)
                        //{
                        //    object[] Parameters = new object[8];
                        //    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                        //    Parameters[1] = cboEquipment.SelectedValue.ToString();
                        //    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        //    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                        //    Parameters[4] = Util.NVC(sCreateTrayID);
                        //    Parameters[5] = Util.NVC(sCreateTrayQty);
                        //    Parameters[6] = Util.NVC(sCreateOutLot);
                        //    Parameters[7] = false;  // View Mode. (Read Only)

                        //    C1WindowExtension.SetParameters(wndCellList, Parameters);

                        //    wndCellList.Closed += new EventHandler(wndCellList_Closed);

                        //    wndCellList.WindowState = C1WindowState.Maximized;

                        //    // 팝업 화면 숨겨지는 문제 수정.
                        //    this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
                        //    //grdMain.Children.Add(wndCellList);
                        //    //wndCellList.BringToFront();
                        //}
                    }
                    //else
                    //{
                    //    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    //}

                    txtTrayID.Text = "";
                }

                GetProductLot();
                //    }
                //});
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCPrdTransfer())
                return;

            if (wndCProduct != null)
                wndCProduct = null;

            wndCProduct = new WND_CPROD_TRANSFER();
            wndCProduct.FrameOperation = FrameOperation;

            if (wndCProduct != null)
            {
                object[] Parameters = new object[6];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipmentSegment.Text);
                Parameters[2] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[3] = Process.PACKAGING;
                Parameters[4] = EquipmentGroup.PACKAGING;
                Parameters[5] = Util.NVC(cboEquipment.Text);

                C1WindowExtension.SetParameters(wndCProduct, Parameters);

                wndCProduct.Closed += new EventHandler(wndCProduct_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndCProduct.ShowModal()));
            }
        }
        
        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!CanMerge())
                return;

            if (wndMerge != null)
                wndMerge = null;

            wndMerge = new ASSY003_OUTLOT_MERGE();
            wndMerge.FrameOperation = FrameOperation;

            if (wndMerge != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = cboEquipmentSegment.Text;

                C1WindowExtension.SetParameters(wndMerge, Parameters);

                wndMerge.Closed += new EventHandler(wndMerge_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndMerge.ShowModal()));
            }
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
                    newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID"));
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_EQPT_END_LOT_CL_S", "INDATA", null, inTable, (bizResult, bizException) =>
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

        private void btnScheduledShutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanScheduledShutdownMode()) return;

                if (bTestMode)
                {
                    SetTestMode(false, bShutdownMode: true);
                    GetTestMode();
                }
                else
                {
                    Util.MessageConfirm("SFU4460", (result) => // 계획정지를 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

                            SetTestMode(true, bShutdownMode: true);
                            GetTestMode();
                        }
                    });
                }
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

            if (wndQualityInput != null)
                wndQualityInput = null;

            wndQualityInput = new CMM_COM_SELF_INSP();
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

        private void dgOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                if (dgOut.CurrentCell.Column.Name.Equals("FORM_MOVE_STAT_CODE_NAME"))
                {

                    if (Util.NVC(DataTableConverter.GetValue(dgOut.CurrentCell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        if (AuthCheck())
                        {
                            Util.MessageConfirm("SFU1243", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    SetTrayConfirmCancelAdmin(Util.NVC(DataTableConverter.GetValue(dgOut.CurrentCell.Row.DataItem, "OUT_LOTID")),
                                        Util.NVC(DataTableConverter.GetValue(dgOut.CurrentCell.Row.DataItem, "TRAYID")));
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                if (wndCancelConfrim != null)
                    wndCancelConfrim = null;

                wndCancelConfrim = new CMM_ASSY_CANCEL_CONFIRM_PROD();
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

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            ASSY003_005_CHANGE wndChange = new ASSY003_005_CHANGE();
            wndChange.FrameOperation = FrameOperation;

            if (wndChange != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIP_WRK_TYPE_CODE"));
                Parameters[4] = Process.PACKAGING;

                C1WindowExtension.SetParameters(wndChange, Parameters);

                wndChange.Closed += new EventHandler(wndChange_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                grdMain.Children.Add(wndChange);
                wndChange.BringToFront();
            }
        }
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

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_CL_NJ", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                if (!bCellTraceMode)
                {
                    newRow["CELL_TRACE_FLAG"] = "N";
                }

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL_NJ", "INDATA", "OUTDATA", inTable);

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
                newRow["SPCL_CST_RSNCODE"] = "";

                inCst.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_CL_S", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
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

                new ClientProxy().ExecuteService("BR_PRD_REG_DELETE_OUT_LOT_CL_S", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CNFM_CANCEL_OUT_LOT_CL_S", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
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
                if (specYN.Equals("Y"))
                {
                    if (SpecDesc == "")
                    {
                        //Util.Alert("특별관리내역을 입력하세요.");
                        Util.MessageValidation("SFU1990");
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
                }

                ShowLoadingIndicator();

                string sBizName = "BR_PRD_REG_UPD_OUT_LOT_CL_S";// bCellTraceMode ? "BR_PRD_REG_UPD_OUT_LOT_CL" : "BR_PRD_REG_UPD_OUT_LOT_CL_S";

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
                    if(!bCellTraceMode) // if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                        newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CELLQTY")));


                    inLot.Rows.Add(newRow);
                    newRow = null;

                    // 특별 Tray DataTable                
                    newRow = inSpcl.NewRow();
                    newRow["SPCL_CST_GNRT_FLAG"] = specYN;
                    newRow["SPCL_CST_NOTE"] = SpecDesc;
                    newRow["SPCL_CST_RSNCODE"] = "";

                    inSpcl.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_LOT,IN_SPCL", null, (searchResult, searchException) =>
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
                newRow["SRCTYPE"] = Process.PACKAGING;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["SPCL_LOT_GNRT_FLAG"] = (bool)chkOutTraySpl.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = (bool)chkOutTraySpl.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = txtOutTrayReamrk.Text;
                newRow["SPCL_PROD_LOTID"] = (bool)chkOutTraySpl.IsChecked ? Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID")) : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_SPCL_CST", "INDATA", null, inTable, (searchResult, searchException) =>
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

        private bool SetTestMode(bool bOn, bool bShutdownMode = false)
        {
            try
            {
                string sBizName = string.Empty;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_MODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (bShutdownMode)
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE_LOSS";

                    newRow["IFMODE"] = "ON";
                    newRow["UI_LOSS_MODE"] = bOn ? "ON" : "OFF";
                    newRow["UI_LOSS_CODE"] = bOn ? Util.ConvertEqptLossLevel2Change("LC003") : ""; // 계획정지 loss 코드.
                }
                else
                {
                    sBizName = "BR_EQP_REG_EQPT_OPMODE";

                    newRow["IFMODE"] = bOn ? "TEST" : "ON";
                }

                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(sBizName, "IN_EQP", null, inTable);

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
                if (cboEquipment == null || cboEquipment.SelectedValue == null) return;
                if (Util.NVC(cboEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO_S", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE") && dtRslt.Columns.Contains("MODE_TYPE") && dtRslt.Columns.Contains("SCHEDULED_SHUTDOWN"))
                {
                    sTestModeType = Util.NVC(dtRslt.Rows[0]["MODE_TYPE"]);

                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        //HideTestMode();

                        if (Util.NVC(dtRslt.Rows[0]["SCHEDULED_SHUTDOWN"]).Equals("Y"))
                        {
                            ShowScheduledShutdown();
                        }
                        else
                        {
                            HideScheduledShutdown();
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
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_LOT_CL_S", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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


                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_CL_S", "IN_EQP,IN_INPUT", null, indataSet);

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

        private void GetCellTraceFlag(string sProdID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));                

                DataRow newRow = inTable.NewRow();                
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PRODID"] = sProdID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CELL_ID_MNGT_FLAG_CL", "INDATA", "OUTDATA", inTable);
                
                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CELL_ID_MNGT_FLAG") && Util.NVC(dtRslt.Rows[0]["CELL_ID_MNGT_FLAG"]).Equals("Y"))
                    {
                        bCellTraceMode = true;

                        if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                            dgOut.Columns["CELLQTY"].IsReadOnly = true;

                        txtTraceMode.Text = "(Trace mode)";

                        btnOutCell.IsEnabled = true;
                    }
                    else
                    {                        
                        bCellTraceMode = false;

                        if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                            dgOut.Columns["CELLQTY"].IsReadOnly = false;

                        txtTraceMode.Text = "(Not Trace mode)";

                        btnOutCell.IsEnabled = false;
                    }
                }
                else
                {
                    bCellTraceMode = true;

                    if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                        dgOut.Columns["CELLQTY"].IsReadOnly = true;

                    txtTraceMode.Text = "(Trace mode)";
                    btnOutCell.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                bCellTraceMode = true;

                if (dgOut != null && dgOut.Columns.Contains("CELLQTY"))
                    dgOut.Columns["CELLQTY"].IsReadOnly = true;

                txtTraceMode.Text = "(Trace mode)";
                btnOutCell.IsEnabled = true;

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CreateTray(out string sCreateOutLot, out string sCreateTrayID, out string sCreateTrayQty)
        {
            sCreateOutLot = "";
            sCreateTrayID = "";
            sCreateTrayQty = "";

            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_CREATE_TRAY_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = ""; // TRAY MAPPING LOT
                newRow["TRAYID"] = txtTrayID.Text;
                newRow["WO_DETL_ID"] = null;
                newRow["USERID"] = LoginInfo.USERID;
                if (!bCellTraceMode)
                {
                    decimal dTmp = 0;
                    decimal.TryParse(txtTrayQty.Text, out dTmp);
                    newRow["CELL_QTY"] = dTmp;
                }

                inTable.Rows.Add(newRow);

                //string sRsnCode = cboOutTraySplReason.SelectedValue == null ? "" : cboOutTraySplReason.SelectedValue.ToString();

                DataTable inSpcl = indataSet.Tables["IN_SPCL"];
                //newRow = inSpcl.NewRow();
                //newRow["SPCL_CST_GNRT_FLAG"] = (bool)chkOutTraySpl.IsChecked ? "Y" : "N";
                //newRow["SPCL_CST_NOTE"] = txtOutTrayReamrk.Text;
                //newRow["SPCL_CST_RSNCODE"] = (bool)chkOutTraySpl.IsChecked ? sRsnCode : "";

                //inSpcl.Rows.Add(newRow);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_OUT_LOT_CL_S", "IN_EQP,IN_INPUT,IN_SPCL", "OUT_LOT", indataSet);

                if (dsRslt != null && dsRslt.Tables.Contains("OUT_LOT") && dsRslt.Tables["OUT_LOT"].Rows.Count > 0)
                {
                    if (dsRslt.Tables["OUT_LOT"].Columns.Contains("OUT_LOTID"))
                        sCreateOutLot = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["OUT_LOTID"]);
                    else
                        sCreateOutLot = "";

                    if (dsRslt.Tables["OUT_LOT"].Columns.Contains("CST_CAPA_QTY"))
                        sCreateTrayQty = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["CST_CAPA_QTY"]);
                    else
                        sCreateTrayQty = "";

                    sCreateTrayID = txtTrayID.Text;
                }

                HiddenLoadingIndicator();

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool GetExcptTime(out double dHour)
        {
            dHour = 0;

            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = Process.PACKAGING;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_WORK_EXCP_TIME", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("EXCP_TIME"))
                {
                    if (!double.TryParse(Util.NVC(dtRslt.Rows[0]["EXCP_TIME"]), out dHour))
                        dHour = 0;

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
                dr["AUTHID"] = "MESADMIN";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT);
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

        private void SetTrayConfirmCancelAdmin(string sOutLot, string sTrayID)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inCst = indataSet.Tables.Add("IN_CST");
                inCst.Columns.Add("OUT_LOTID", typeof(string));
                inCst.Columns.Add("CSTID", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;
                
                newRow = inCst.NewRow();
                newRow["OUT_LOTID"] = sOutLot;
                newRow["CSTID"] = sTrayID;

                inCst.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CNFM_CANCEL_OUT_LOT_CL_ADMIN_S", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
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
        
        #endregion

        #region [Validation]
        private bool CanCPrdTransfer()
        {
            bool bRet = false;

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //if (txtShift.Text.Trim().Equals(""))
            //{
            //    //Util.Alert("작업조를 선택 하세요.");
            //    Util.MessageValidation("SFU1844");
            //    return bRet;
            //}

            //if (txtWorker.Text.Trim().Equals(""))
            //{
            //    //Util.Alert("작업자를 선택 하세요.");
            //    Util.MessageValidation("SFU1842");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

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

                            if (wndInputObject != null)
                            {
                                wndInputObject.BringToFront();
                            }
                            else
                            {
                                wndInputObject = new ASSY003_007_INPUT_OBJECT();
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

            //string sLine = LoginInfo.CFG_EQSG_ID.Substring(3, 1);
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

                // 공통코드에 등록된 시간 내의 경우에는 초기화 하지 않도록..
                double dHour = 0;
                if (GetExcptTime(out dHour))
                {
                    if (dHour > 0)
                    {
                        shiftStartDateTime = shiftStartDateTime.AddHours(-dHour);
                        shiftEndDateTime = shiftEndDateTime.AddHours(-dHour);
                    }
                }

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

        private bool ChkInLotMustComplete()
        {
            bool bRet = true;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            newRow["PROCID"] = Process.PACKAGING;
            newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

            inTable.Rows.Add(newRow);

            DataTable dtResult = null;

            dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_CURR_IN_LOT_LIST", "INDATA", "OUTDATA", inTable);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                for (int inx = 0; inx < dtResult.Rows.Count; inx++)
                {
                    if (string.IsNullOrEmpty(Util.NVC(dtResult.Rows[inx]["INPUT_LOTID"])) == false)
                    {
                        bRet = false;
                        break;
                    }
                }
            }
            else
            {
                bRet = true;
            }

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

            //if (((DataRowView)dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem).Row.RowState == DataRowState.Modified)
            if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "CELLQTY")).Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "HIDDEN_CELLQTY"))))
            {
                Util.MessageValidation("SFU4038");   // 변경된 데이터가 존재합니다. 먼저 저장한 후 다시 시도하세요.
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
            
            if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                Util.MessageValidation("SFU3721"); // Tray 이동이 가능한 상태가 아닙니다.
                return bRet;
            }

            //if (((DataRowView)dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem).Row.RowState == DataRowState.Modified)
            if(!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "CELLQTY")).Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "HIDDEN_CELLQTY"))))
            {
                Util.MessageValidation("SFU4038");   // 변경된 데이터가 존재합니다. 먼저 저장한 후 다시 시도하세요.
                return bRet;
            }

            double iCellCnt = 0;
            double.TryParse(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "CELLQTY")), out iCellCnt);

            if (iCellCnt < 1)
            {
                Util.MessageValidation("SFU3063"); // 수량이 없습니다.
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

            //if (bScheduledShutdown)
            //{
            //    Util.MessageValidation("SFU4464"); // 계획정지중 입니다. 계획정지를 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}
            
            bRet = true;
            return bRet;
        }

        private bool CanScheduledShutdownMode()
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

            //if (bTestMode)
            //{
            //    Util.MessageValidation("SFU4465"); // 테스트 Run 중 입니다. 테스트 Run 해제 후 다시 시도해 주세요.
            //    return bRet;
            //}

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

        private bool CanCreateTrayByTrayID()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                txtTrayID.Text = "";
                return bRet;
            }

            string trayID = txtTrayID.Text.ToUpper();
            //정규식표현 false=>영문과숫자이외문자열비허용
            bool chk = System.Text.RegularExpressions.Regex.IsMatch(trayID, @"^[a-zA-Z0-9]+$");

            if (!chk)
            {
                //Util.Alert("입력한 ID ({0}) 에 특수문자가 존재하여 생성할 수 없습니다.", trayID);
                Util.MessageValidation("SFU1811", trayID);
                txtTrayID.Text = "";
                return bRet;
            }
            if (!bCellTraceMode && (txtTrayQty.Text.Equals("0") || txtTrayQty.Text.Trim().Length < 1))
            {
                //Util.Alert("수량을 입력하세요.");
                Util.MessageValidation("SFU1684");
                txtTrayQty.Focus();
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
                    //Util.Alert("특이사항을 입력 하세요.");
                    Util.MessageValidation("SFU1992");
                    return bRet;
                }
            }
            else
            {
                if (!txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("특이사항을 삭제 하세요.");
                    Util.MessageValidation("SFU1991");
                    return bRet;
                }
            }

            // 생성 비즈에서 체크 하므로 주석.
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetTrayValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    Util.Alert(sMsg);
            //    return bRet;
            //}


            // FCS Check...            
            //if (!GetFCSTrayCheck(trayID))
            //{
            //    Util.Alert("FORMATION에 TRAY ID가 작업중입니다. 활성화에 문의하세요.");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }
        
        private bool CanBoxIn()
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

        #endregion

        #region [PopUp Event]
        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            wndRunStart = null;
            ASSY003_007_RUNSTART window = sender as ASSY003_007_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true, window.NEW_PROD_LOT);
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            wndConfirm = null;
            ASSY003_007_CONFIRM window = sender as ASSY003_007_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot();                
            }

            GetProductLot();
            GetEqptWrkInfo();
        }

        private void wndWait_Closed(object sender, EventArgs e)
        {
            wndWaitLot = null;
            ASSY003_007_WAITLOT window = sender as ASSY003_007_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndTrayCreate_Closed(object sender, EventArgs e)
        {
            wndTrayCreate = null;

            ASSY003_007_TRAY_CREATE window = sender as ASSY003_007_TRAY_CREATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                // tray 생성 후 trace 모드인 경우는 cell 팝업 호출.
                if(bCellTraceMode) // if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                {
                    ShowCellInfoPopup(Util.NVC(window.CREATE_TRAYID), Util.NVC(window.CREATE_OUT_LOT), Util.NVC(window.CREATE_TRAY_QTY));

                    //if (wndCellList != null)
                    //    wndCellList = null;

                    //wndCellList = new ASSY003_007_CELL_LIST();
                    //wndCellList.FrameOperation = FrameOperation;

                    //if (wndCellList != null)
                    //{
                    //    object[] Parameters = new object[8];
                    //    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                    //    Parameters[1] = cboEquipment.SelectedValue.ToString();
                    //    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    //    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                    //    Parameters[4] = Util.NVC(window.CREATE_TRAYID);
                    //    Parameters[5] = Util.NVC(window.CREATE_TRAY_QTY);
                    //    Parameters[6] = Util.NVC(window.CREATE_OUT_LOT);
                    //    Parameters[7] = false;  // View Mode. (Read Only)

                    //    C1WindowExtension.SetParameters(wndCellList, Parameters);

                    //    wndCellList.Closed += new EventHandler(wndCellList_Closed);

                    //    wndCellList.WindowState = C1WindowState.Maximized;

                    //    // 팝업 화면 숨겨지는 문제 수정.
                    //    this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
                    //    //grdMain.Children.Add(wndCellList);
                    //    //wndCellList.BringToFront();
                    //}
                }

                GetProductLot();
                //GetOutTray();
            }
        }

        //private void wndTrayMove_Closed(object sender, EventArgs e)
        //{
        //    ASSY003_007_TRAY_MOVE window = sender as ASSY003_007_TRAY_MOVE;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //        GetOutTray();
        //    }
        //    this.grdMain.Children.Remove(window);
        //}

        private void wndCellList_Closed(object sender, EventArgs e)
        {
            //wndCellList = null;
            ASSY003_007_CELL_LIST window = sender as ASSY003_007_CELL_LIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            //System.Diagnostics.Debug.WriteLine("After close cell pop up : {0} Bytes.", GC.GetTotalMemory(false));

            GetWorkOrder(); // 작지 생산수량 정보 재조회.

            GetProductLot();
        }

        private void wndEqpComment_Closed(object sender, EventArgs e)
        {
            wndEqpComment = null;
            CMM_COM_EQPCOMMENT window = sender as CMM_COM_EQPCOMMENT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndDefect_Closed(object sender, EventArgs e)
        {
            wndDefect = null;
            CMM_ASSY_DEFECT window = sender as CMM_ASSY_DEFECT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetProductLot();
        }

        private void wndEqptCond_Closed(object sender, EventArgs e)
        {
            wndEqptCond = null;
            CMM_ASSY_PU_EQPT_COND window = sender as CMM_ASSY_PU_EQPT_COND;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndCancelTerm_Closed(object sender, EventArgs e)
        {
            wndCancelTerm = null;
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            wndShiftUser = null;
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
        }

        private void wndEqpEnd_Closed(object sender, EventArgs e)
        {
            wndEqpEnd = null;
            CMM_ASSY_EQPEND window = sender as CMM_ASSY_EQPEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
        }

        private void wndQuality_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY2 window = sender as CMM_ASSY_QUALITY2;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        //private void wndAbnormal_Closed(object sender, EventArgs e)
        //{
        //    ASSY003_007_ABNORMAL window = sender as ASSY003_007_ABNORMAL;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //    }
        //    this.grdMain.Children.Remove(window);
        //}

        private void wndQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_COM_QUALITY window = sender as CMM_COM_QUALITY;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndQualityInput_New_Closed(object sender, EventArgs e)
        {
            wndQualityInput = null;
            CMM_COM_SELF_INSP window = sender as CMM_COM_SELF_INSP;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            wndQualitySearch = null;
            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void wndInputObject_Closed(object sender, EventArgs e)
        {
            wndInputObject = null;
            ASSY003_007_INPUT_OBJECT window = sender as ASSY003_007_INPUT_OBJECT;
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
                    HiddenLoadingIndicator();
                }
            }
        }

        private void wndTrayMove_Closed(object sender, EventArgs e)
        {
            wndTrayMove = null;
            ASSY003_007_TRAY_MOVE window = sender as ASSY003_007_TRAY_MOVE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetOutTray();
            }
        }

        private void wndCProduct_Closed(object sender, EventArgs e)
        {
            wndCProduct = null;
            WND_CPROD_TRANSFER window = sender as WND_CPROD_TRANSFER;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void wndMerge_Closed(object sender, EventArgs e)
        {
            wndMerge = null;

            ASSY003_OUTLOT_MERGE window = sender as ASSY003_OUTLOT_MERGE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            btnSearch_Click(null, null);
        }

        private void wndCancelConfrim_Closed(object sender, EventArgs e)
        {
            wndCancelConfrim = null;
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

        private void wndChange_Closed(object sender, EventArgs e)
        {
            ASSY003_005_CHANGE window = sender as ASSY003_005_CHANGE;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                GetOutTray();
            }

            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnOutCreate);
            listAuth.Add(btnOutDel);
            listAuth.Add(btnOutConfirmCancel);
            listAuth.Add(btnOutConfirm);
            listAuth.Add(btnOutSave);
            listAuth.Add(btnOutTraySplSave);
            listAuth.Add(btnTestFullTrayCreate);
            listAuth.Add(btnTestFullTrayAllConfirm);
            listAuth.Add(btnOutMove);

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
                    winInput = new UC_IN_OUTPUT_MOBILE(Process.PACKAGING, sEqsg, sEqpt);

                winInput.FrameOperation = FrameOperation;

                winInput._UCParent = this;
                grdInput.Children.Add(winInput);

                SetControlButton();
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

            // Cell Trace Mode 설정
            string sTmpProd = "";
            if (iRow < dgProductLot.Rows.Count)
                sTmpProd = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "PRODID"));

            GetCellTraceFlag(sTmpProd);

            GetInputAllInfo(iRow);

            ProcessDetail(dgProductLot.Rows[iRow].DataItem);

            // Tray 모드에 따른 Tray 생성 Control Enable 처리.
            if (bCellTraceMode)
            {
                txtTrayID.IsEnabled = true;
                txtTrayQty.IsEnabled = false;
            }
            else
            {
                txtTrayID.IsEnabled = true;
                //txtTrayQty.Text = "";
                txtTrayQty.IsEnabled = true;
            }
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

            //System.Diagnostics.Debug.WriteLine("Before open cell pop up : {0} Bytes.", GC.GetTotalMemory(false));

            ShowCellInfoPopup(trayID, outLOTID, sTrayQty);

            //if (wndCellList != null)
            //    wndCellList = null;

            //wndCellList = new ASSY003_007_CELL_LIST();
            //wndCellList.FrameOperation = FrameOperation;

            //if (wndCellList != null)
            //{
            //    object[] Parameters = new object[8];
            //    Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
            //    Parameters[1] = cboEquipment.SelectedValue.ToString();
            //    Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
            //    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
            //    Parameters[4] = trayID;
            //    Parameters[5] = sTrayQty;
            //    Parameters[6] = outLOTID;
            //    Parameters[7] = false;  // View Mode. (Read Only)

            //    C1WindowExtension.SetParameters(wndCellList, Parameters);

            //    wndCellList.Closed += new EventHandler(wndCellList_Closed);

            //    //int tmpTrayQty = 0;
            //    //int.TryParse(sTrayQty, out tmpTrayQty);

            //    //if (tmpTrayQty > 100)
            //        wndCellList.WindowState = C1WindowState.Maximized;

            //    // 팝업 화면 숨겨지는 문제 수정.
            //    this.Dispatcher.BeginInvoke(new Action(() => wndCellList.ShowModal()));
            //    //grdMain.Children.Add(wndCellList);
            //    //wndCellList.BringToFront();
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
                        //btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                        btnOutMove.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        //btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                        btnOutMove.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN ")) // 활성화입고
                    {
                        //btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                        btnOutMove.IsEnabled = false;
                    }
                    else
                    {
                        //btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                        btnOutMove.IsEnabled = true;
                    }
                }
                else
                {
                    //btnOutCreate.IsEnabled = true;
                    btnOutDel.IsEnabled = true;
                    btnOutConfirmCancel.IsEnabled = true;
                    btnOutConfirm.IsEnabled = true;
                    btnOutCell.IsEnabled = true;
                    btnOutSave.IsEnabled = true;
                    btnOutMove.IsEnabled = true;
                }

                // Cell 추적 여부에 따른 Cell ID 버튼 활성/비활성화
                if (bCellTraceMode)
                {
                    btnOutCell.IsEnabled = true;
                }
                else
                {
                    btnOutCell.IsEnabled = false;
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
        
        private void ColorAnimationInSpecialTray()
        {
            recSpcTray.Fill = myAnimatedBrush;

            //ColorAnimation mouseEnterColorAnimation = new ColorAnimation();
            //mouseEnterColorAnimation.To = Colors.Aqua;
            //mouseEnterColorAnimation.Duration = TimeSpan.FromSeconds(1);
            //mouseEnterColorAnimation.RepeatBehavior = RepeatBehavior.Forever;
            //Storyboard.SetTargetName(mouseEnterColorAnimation, "myAnimatedBrush");
            //Storyboard.SetTargetProperty(
            //    mouseEnterColorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));
            //Storyboard storyboard = new Storyboard();
            //storyboard.Children.Add(mouseEnterColorAnimation);
            //storyboard.Begin(this);

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "myAnimatedBrush");
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
                        if (wndConfirm != null)
                            wndConfirm = null;

                        wndConfirm = new ASSY003_007_CONFIRM();
                        wndConfirm.FrameOperation = FrameOperation;

                        if (wndConfirm != null)
                        {
                            object[] Parameters = new object[11];
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

                            C1WindowExtension.SetParameters(wndConfirm, Parameters);

                            wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                            
                            this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));                            
                        }
                    }
                });
            }
            else
            {
                if (wndConfirm != null)
                    wndConfirm = null;

                wndConfirm = new ASSY003_007_CONFIRM();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[11];
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
                txtLossCnt.Text = "";
                txtLossCnt.Background = new System.Windows.Media.SolidColorBrush(Colors.WhiteSmoke);

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

        #region 계획정지, 테스트 Run
        private void HideScheduledShutdown()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += HideScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            bTestMode = false;

        }

        private void ShowScheduledShutdown()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bScheduledShutdown) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(false);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[2].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            bTestMode = true;
        }

        private void showScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(false);
        }

        private void HideScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void ColorAnimationInRectangle(bool bTest)
        {
            try
            {
                string sname = string.Empty;
                if (bTest)
                {
                    recTestMode.Fill = redBrush;
                    sname = "redBrush";
                }
                else
                {
                    recTestMode.Fill = yellowBrush;
                    sname = "yellowBrush";
                }

                DoubleAnimation opacityAnimation = new DoubleAnimation();
                opacityAnimation.From = 1.0;
                opacityAnimation.To = 0.0;
                opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
                opacityAnimation.AutoReverse = true;
                opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTargetName(opacityAnimation, sname);
                Storyboard.SetTargetProperty(
                    opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

                mouseLeftButtonDownStoryboard.Begin(this);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[2].Height.Value > 0)
                {
                    ColorAnimationInRectangle(true);
                    return;
                }

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
            ColorAnimationInRectangle(true);
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }
        #endregion

        #endregion

        #endregion


        #region [Cell Info Pop up..(Memory leak)]
        #region [Declaration & Constructor]
        private string _TrayID = string.Empty;
        private string _TrayQty = string.Empty;
        private string _OutLotID = string.Empty;
        private bool _CompatibilityChkExceptFlag = false;
        private bool _ELUseYN = false;
        private Brush[,] originColorMap = null;


        private bool _ShowSlotNo = false;
        private bool _OnlyView = false;

        // 주액 USL, LSL 기준정보
        private string _EL_WEIGHT_LSL = string.Empty;
        private string _EL_WEIGHT_USL = string.Empty;
        private string _EL_AFTER_WEIGHT_LSL = string.Empty;
        private string _EL_AFTER_WEIGHT_USL = string.Empty;
        private string _EL_BEFORE_WEIGHT_LSL = string.Empty;
        private string _EL_BEFORE_WEIGHT_USL = string.Empty;


        ASSY003_007_CELLID_RULE wndCellIDRule = null;

        private static class TRAY_SHAPE
        {
            public static string CELL_TYPE = string.Empty;  // CELL TYPE
            public static int ROW_NUM = 0;  // 총 ROW 수
            public static int COL_NUM = 0;  // 총 COL 수
            public static bool EMPTY_SLOT = false;  // 빈 슬롯 존재 여부
            public static bool ZIGZAG = false;  // COL 별 지그재그 배치 여부
            public static string[] EMPTY_SLOT_LIST = null;  // 빈 슬롯 컬럼 LIST
            public static int MERGE_START_COL_NUM = 0; // 머지 시작 컬럼 넘버
            public static string[] DISPLAY_LIST = null; // Cell 영역에 표시할 Data List
            public static char[] DISP_SEPARATOR; // 표시 영역 구분자
        }

        public bool bExistLayOutInfo = false;
        private bool bViewAll = false;

        #endregion

        #region [Initialize]
        private void InitializeControls()
        {
            int iTrayQty = 0;
            if (!_TrayQty.Equals("") && int.TryParse(_TrayQty, out iTrayQty))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                for (int i = 0; i < iTrayQty; i++)
                {
                    dt.Rows.Add((i + 1).ToString(), (i + 1).ToString());
                }

                cboTrayLocation.ItemsSource = dt.Copy().AsDataView();
                if (dt.Rows.Count > 0)
                    cboTrayLocation.SelectedIndex = 0;
            }

            //rdoAuto.IsChecked = true;
            cboTrayLocation.IsEnabled = false;

            // 적합성 체크 Visibility
            if (!_CompatibilityChkExceptFlag)
            {
                btnCheckElJudge.Visibility = Visibility.Visible;

                if (!_ELUseYN)
                {
                    btnCheckElJudge.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                btnCheckElJudge.Visibility = Visibility.Collapsed;
            }

            // 주액 관려 여부에 따른 컨트롤 Hidden 처리
            if (_ELUseYN)
            {
                //rdoManual.IsEnabled = true;
                tblEl.Visibility = Visibility.Visible;
                txtEl.Visibility = Visibility.Visible;
                //tblElMinMax.Visibility = Visibility.Visible;
                tblBeforeWeight.Visibility = Visibility.Visible;
                txtBeforeWeight.Visibility = Visibility.Visible;
                //tblBeforeWeightMinMax.Visibility = Visibility.Visible;
                tblAfterWeight.Visibility = Visibility.Visible;
                txtAfterWeight.Visibility = Visibility.Visible;
                //tblAfterWeightMinMax.Visibility = Visibility.Visible;
                tblHeader.Visibility = Visibility.Collapsed;
                txtHeader.Visibility = Visibility.Collapsed;
                tblPosition.Visibility = Visibility.Visible;
                txtPosition.Visibility = Visibility.Visible;
                tblJudge.Visibility = Visibility.Visible;
                txtJudge.Visibility = Visibility.Visible;
            }
            else
            {
                //rdoManual.IsEnabled = false;
                tblEl.Visibility = Visibility.Collapsed;
                txtEl.Visibility = Visibility.Collapsed;
                tblElMinMax.Visibility = Visibility.Collapsed;
                tblBeforeWeight.Visibility = Visibility.Collapsed;
                txtBeforeWeight.Visibility = Visibility.Collapsed;
                tblBeforeWeightMinMax.Visibility = Visibility.Collapsed;
                tblAfterWeight.Visibility = Visibility.Collapsed;
                txtAfterWeight.Visibility = Visibility.Collapsed;
                tblAfterWeightMinMax.Visibility = Visibility.Collapsed;
                tblHeader.Visibility = Visibility.Collapsed;
                txtHeader.Visibility = Visibility.Collapsed;
                tblPosition.Visibility = Visibility.Collapsed;
                txtPosition.Visibility = Visibility.Collapsed;
                tblJudge.Visibility = Visibility.Collapsed;
                txtJudge.Visibility = Visibility.Collapsed;
            }


            // View Mode 인 경우 모두 Disable 처리.
            if (_OnlyView)
            {
                txtCellId.IsReadOnly = true;
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                dgDupList.IsReadOnly = true;

                //rdoManual.IsEnabled = true;

                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;

                btnCheckElJudge.Visibility = Visibility.Collapsed;
                btnOutRangeDelAll.Visibility = Visibility.Collapsed;

                this.tbTitle.Text = ObjectDic.Instance.GetObjectName("TRAY별CELLID관리") + " (Read Only)";
            }
        }

        private void InitializeGrid()
        {
            try
            {
                if (string.IsNullOrEmpty(TRAY_SHAPE.CELL_TYPE))
                    return;

                DataTable dtTemp = new DataTable();

                if (TRAY_SHAPE.ZIGZAG) // zigzag 모양
                {
                    int width = 200;

                    if (TRAY_SHAPE.COL_NUM > 6)
                        width = 80;
                    else if (TRAY_SHAPE.COL_NUM > 5)
                        width = 115;
                    else if (TRAY_SHAPE.COL_NUM > 4)
                        width = 125;
                    else if (TRAY_SHAPE.COL_NUM > 3)
                        width = 150;

                    dtTemp.Columns.Add("NO");

                    int ascii = 65; // ascii => "A"

                    for (int i = 0; i < TRAY_SHAPE.COL_NUM; i++)
                    {
                        int iSBN = (ascii + i);

                        string sTmp = Char.ConvertFromUtf32(iSBN);

                        if (!dgCell.Columns.Contains(sTmp + "_SLOTNO"))
                            Util.SetGridColumnText(dgCell, sTmp + "_SLOTNO", null, "NO.", true, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 20, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp))
                            Util.SetGridColumnText(dgCell, sTmp, null, sTmp, false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, width, HorizontalAlignment.Center, Visibility.Visible);
                        if (!dgCell.Columns.Contains(sTmp + "_JUDGE"))
                            Util.SetGridColumnText(dgCell, sTmp + "_JUDGE", null, sTmp + "_JUDGE", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_LOC"))
                            Util.SetGridColumnText(dgCell, sTmp + "_LOC", null, sTmp + "_LOC", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_CELLID"))
                            Util.SetGridColumnText(dgCell, sTmp + "_CELLID", null, sTmp + "_CELLID", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);


                        dtTemp.Columns.Add(sTmp + "_SLOTNO");
                        dtTemp.Columns.Add(sTmp);
                        dtTemp.Columns.Add(sTmp + "_JUDGE");
                        dtTemp.Columns.Add(sTmp + "_LOC");
                        dtTemp.Columns.Add(sTmp + "_CELLID");

                        dgCell.Columns[sTmp].MaxWidth = 220;
                    }

                    // 빈 Cell 정보 Set.
                    for (int i = 0; i < TRAY_SHAPE.ROW_NUM; i++)
                    {
                        DataRow dtRow = dtTemp.NewRow();

                        dtTemp.Rows.Add(dtRow);
                    }

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();

                    // alternating row color 삭제
                    dgCell.RowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
                    dgCell.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);

                    SetZigZagGridInfo();

                    dgCell.MergingCells -= dgCell_MergingCells;
                    dgCell.MergingCells += dgCell_MergingCells;

                    //MergingCells();
                }
                else // 정상 모양
                {
                    int width = 200;

                    if (TRAY_SHAPE.COL_NUM > 6)
                        width = 95;
                    else if (TRAY_SHAPE.COL_NUM > 5)
                        width = 100;
                    else if (TRAY_SHAPE.COL_NUM > 4)
                        width = 125;
                    else if (TRAY_SHAPE.COL_NUM > 3)
                        width = 150;

                    dtTemp.Columns.Add("NO");

                    int ascii = 65; // ascii => "A"

                    for (int i = 0; i < TRAY_SHAPE.COL_NUM; i++)
                    {
                        int iSBN = (ascii + i);

                        string sTmp = Char.ConvertFromUtf32(iSBN);

                        if (!dgCell.Columns.Contains(sTmp + "_SLOTNO"))
                            Util.SetGridColumnText(dgCell, sTmp + "_SLOTNO", null, "NO.", true, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 20, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp))
                            Util.SetGridColumnText(dgCell, sTmp, null, sTmp, false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, width, HorizontalAlignment.Center, Visibility.Visible);
                        if (!dgCell.Columns.Contains(sTmp + "_JUDGE"))
                            Util.SetGridColumnText(dgCell, sTmp + "_JUDGE", null, sTmp + "_JUDGE", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_LOC"))
                            Util.SetGridColumnText(dgCell, sTmp + "_LOC", null, sTmp + "_LOC", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_CELLID"))
                            Util.SetGridColumnText(dgCell, sTmp + "_CELLID", null, sTmp + "_CELLID", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);

                        dtTemp.Columns.Add(sTmp + "_SLOTNO");
                        dtTemp.Columns.Add(sTmp);
                        dtTemp.Columns.Add(sTmp + "_JUDGE");
                        dtTemp.Columns.Add(sTmp + "_LOC");
                        dtTemp.Columns.Add(sTmp + "_CELLID");

                        dgCell.Columns[sTmp].MaxWidth = 220;
                    }

                    // Row Add.
                    for (int i = 0; i < TRAY_SHAPE.ROW_NUM; i++)
                    {
                        DataRow dtRow = dtTemp.NewRow();

                        dtRow["NO"] = (i + 1).ToString();

                        dtTemp.Rows.Add(dtRow);
                    }

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();

                    // LOCATION 정보 SET.
                    int iLocIdx = 1;    // 실제 Cell Location Number 변수.
                    int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                    {
                        for (int i = 0; i < dgCell.Rows.Count; i++)
                        {
                            // 빈 슬롯 번호 확인.
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                            {
                                if (TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                                {
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_JUDGE"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_JUDGE", "EMPT_SLOT");
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_LOC"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC", "EMPT_SLOT");
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_CELLID"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_CELLID", "");
                                }
                                else
                                {
                                    if (!Util.NVC(dgCell.Columns[j].Name).Equals("NO") &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                                }
                            }
                            else
                            {
                                if (!Util.NVC(dgCell.Columns[j].Name).Equals("NO") &&
                                    !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                            }

                            // Location 정보 설정
                            if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_LOC") >= 0 &&
                                !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                            {
                                // Cell 에 Location 값 처리.
                                string sOrgView = Util.NVC(dgCell.Columns[j].Name).Replace("_LOC", "");
                                if (dgCell.Columns.Contains(sOrgView))
                                {
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, sOrgView, iLocIdx.ToString());
                                }

                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, iLocIdx.ToString());
                                iLocIdx++;


                                // Location 정보 Set (View 용)
                                if (dgCell.Columns.Contains(sOrgView + "_SLOTNO") &&
                                    dgCell.Columns.Contains(sOrgView + "_LOC"))
                                {
                                    string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, sOrgView + "_LOC"));

                                    if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, sOrgView + "_SLOTNO", sTmpLocValue);
                                }
                            }
                            else if (Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                            {

                            }
                            else
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                            }

                            // View 컬럼 기준으로 슬롯 넘버링 처리.
                            if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                                iTmpIdx++;

                            //// 25 ROW 넘어가는 경우 ROW HEIGHT 조정
                            //if (dgCell.Rows.Count > 25)
                            //    dgCell.Rows[i].Height = new C1.WPF.DataGrid.DataGridLength(22);
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

        #region [Event]
        private void rdoCellID_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                {
                    // Display Set.
                    SetTrayDisplayList(new string[] { "CELLID" });

                    // 조회
                    SetCellInfo(bLoad: false, bSameLoc: false, bChgNexRow: false);
                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void dgCell_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null)
                    return;

                // LOCATION 정보 SET.
                int iLocIdx = 1; // 실제 Loc Number
                int iAsciiIdx = 0; // 실제 Col Index
                int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                int iMergIdx = string.IsNullOrEmpty(TRAY_SHAPE.MERGE_START_COL_NUM.ToString()) ? 1 : TRAY_SHAPE.MERGE_START_COL_NUM;   // 0 row 부터 머지 처리할 index 변수.
                string sTmpColName = "";


                for (int idxCol = 0; idxCol < dg.Columns.Count; idxCol++)
                {
                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if ((Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 || Util.NVC(dg.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0) && !Util.NVC(dg.Columns[idxCol].Name).Equals("NO"))
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iAsciiIdx++;
                        }
                    }
                    //if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") < 0)
                    //{
                    //    sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                    //    if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                    //    {
                    //        iAsciiIdx++;
                    //    }
                    //}

                    for (int idxRow = 0; idxRow < dg.Rows.Count; idxRow++)
                    {
                        // Row Number 설정
                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") >= 0)
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null && TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                            {
                                if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                            }
                            else
                            {
                                if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    //iRowNum = iRowNum + 1;
                                }
                            }
                        }
                        else
                        {
                            if (iMergIdx % 2 == 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                            {
                                // Cell Merge 처리.
                                if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                }
                                else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                }
                            }
                            else
                            {
                                // View Column Cell Merge 처리.
                                if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 ||
                                    Util.NVC(dg.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0) // View 용 slot no 머지 처리.
                                {
                                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                                    {
                                        e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    }
                                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                                    {
                                        e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    }
                                }
                            }
                        }

                        // View 컬럼 기준으로 슬롯 넘버링 처리.
                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0)
                        {
                            iTmpIdx++;
                        }

                        dg.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                    }

                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iMergIdx++;
                        }
                    }
                }

                #region [빈슬롯 cell number 변경 전 로직 주석..]
                //C1DataGrid dg = sender as C1DataGrid;

                //if (dg == null)
                //    return;

                //int iRowNum = 1;

                //// LOCATION 정보 SET.
                //int iLocIdx = 1; // 실제 Loc Number
                //int iAsciiIdx = 0; // 실제 Col Index
                //string sTmpColName = "";

                //for (int idxCol = 0; idxCol < dg.Columns.Count; idxCol++)
                //{
                //    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                //    if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") < 0)
                //    {
                //        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                //        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                //            iAsciiIdx++;
                //    }

                //    for (int idxRow = 0; idxRow < dg.Rows.Count; idxRow++)
                //    {
                //        // Row Number 설정
                //        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") >= 0)
                //        {
                //            DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, "NO", (iRowNum).ToString());

                //            if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                //                e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //            else
                //                iRowNum = iRowNum + 1;                           
                //        }
                //        else
                //        {
                //            if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                //                DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, "");


                //            int iTmp = 0;

                //            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                //            {
                //                int.TryParse(TRAY_SHAPE.EMPTY_SLOT_LIST[0].ToString(), out iTmp);
                //            }

                //            if (iTmp % 2 != 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                //            {
                //                // Location Number 설정.
                //                if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                //                        !Util.NVC(DataTableConverter.GetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                //                {
                //                    if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                    else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                }

                //                // Cell Merge 처리.
                //                if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //                else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //            }
                //            else
                //            {
                //                // Location Number 설정.
                //                if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                //                        !Util.NVC(DataTableConverter.GetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                //                {
                //                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                }

                //                // Cell Merge 처리.
                //                if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //                else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //            }

                //            //// Cell Merge 처리.
                //            //if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                //            //{
                //            //    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //            //}
                //            //else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //            //{
                //            //    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //            //}
                //        }

                //        dg.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        //Keyboard.IsKeyDown(Key.F3) &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        // 자동차동..
                        if (!LoginInfo.CFG_AREA_ID.StartsWith("A"))
                            return;

                        // ReadOnly
                        if (_OnlyView) return;

                        ShowLoadingIndicator();

                        //if (winTray25 == null)
                        //    return;

                        //C1DataGrid dgTray = winTray25.GetTrayGrdInfo();

                        //if (winTray == null)
                        //    return;

                        C1DataGrid dgTray = null;

                        //if (winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                        //    dgTray = (winTray as PKG_TRAY_MOBILE).GetTrayGrdInfo();
                        dgTray = dgCell;

                        if (dgTray == null)
                            return;

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (txtLotId.Text.Length == 10)
                            {
                                string sTmpCellID = txtLotId.Text.Substring(3, 5) + txtLotId.Text.Substring(9, 1);
                                int iRow = 0;
                                int iLocation = 0;
                                int iColCnt = 0;    // 컬럼 수

                                // 해당 LOT의 MAX SEQ 조회.
                                DataTable inTmpTable = new DataTable();
                                inTmpTable.Columns.Add("LOTID", typeof(string));
                                inTmpTable.Columns.Add("OUT_LOTID", typeof(string));
                                inTmpTable.Columns.Add("TRAYID", typeof(string));
                                inTmpTable.Columns.Add("CELLID", typeof(string));

                                DataRow newTmpRow = inTmpTable.NewRow();
                                newTmpRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                                //newTmpRow["OUT_LOTID"] = _OutLotID;
                                //newTmpRow["TRAYID"] = _TrayID;
                                //newTmpRow["CELLID"] = "";

                                inTmpTable.Rows.Add(newTmpRow);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CELL_SEQ_IN_TRAY", "INDATA", "OUTDATA", inTmpTable);

                                if (dtRslt != null && dtRslt.Rows.Count > 0)
                                    iRow = Util.NVC(dtRslt.Rows[0]["MAXSEQ"]).Equals("") ? 0 : Convert.ToInt32(Util.NVC(dtRslt.Rows[0]["MAXSEQ"]));

                                for (int i = 0; i < dgTray.Columns.Count; i++)
                                {
                                    if (!dgTray.Columns[i].Name.Equals("NO") &&
                                        !dgTray.Columns[i].Name.EndsWith("_JUDGE") &&
                                        !dgTray.Columns[i].Name.EndsWith("_LOC"))
                                    {
                                        // 여러 컬럼인 경우 계산.
                                        iLocation = dgTray.Rows.Count * iColCnt;
                                        iColCnt = iColCnt + 1;

                                        for (int j = 0; j < dgTray.Rows.Count; j++)
                                        {
                                            iLocation = iLocation + 1;
                                            iRow = iRow + 1;

                                            string sTmpCell = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name));
                                            string sTmpLoc = Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name + "_LOC"));

                                            if ((sTmpCell.Equals("") || sTmpCell.Equals(sTmpLoc)) &&
                                                !Util.NVC(DataTableConverter.GetValue(dgTray.Rows[j].DataItem, dgTray.Columns[i].Name + "_JUDGE")).Equals("EMPT_SLOT"))
                                            {
                                                try
                                                {
                                                    DataSet indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S();

                                                    DataTable inTable = indataSet.Tables["IN_EQP"];

                                                    DataRow newRow = inTable.NewRow();
                                                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                                                    newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                                                    newRow["OUT_LOTID"] = _OutLotID;
                                                    newRow["CSTID"] = _TrayID;
                                                    newRow["USERID"] = LoginInfo.USERID;

                                                    inTable.Rows.Add(newRow);
                                                    newRow = null;

                                                    DataTable inSublotTable = indataSet.Tables["IN_CST"];
                                                    newRow = inSublotTable.NewRow();
                                                    newRow["SUBLOTID"] = sTmpCellID + iRow.ToString("0000");
                                                    newRow["CSTSLOT"] = iLocation.ToString();

                                                    inSublotTable.Rows.Add(newRow);

                                                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S", "IN_EQP,IN_CST,IN_EL", null, indataSet);

                                                    System.Threading.Thread.Sleep(300);
                                                }
                                                catch (Exception ex)
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }

                                SearchTrayWindow();
                                GetTrayInfo();
                            }
                        }));
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
            }));
        }

        private void tbxCellId_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbxCellId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //C1.WPF.DataGrid.C1DataGrid dgdCell = (C1.WPF.DataGrid.C1DataGrid)winTray.FindName("dgCell");
                string[] colNameArr = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                ClearControl();

                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    for (int j = 2; j < dgCell.Columns.Count; j++)
                    {
                        if (dgCell[i, j].Presenter != null)
                        {
                            //dgCell[i, j].Presenter.Background = originColorMap[i, j];
                            dgCell[i, j].Presenter.IsSelected = false;
                            for (int k = 0; k < colNameArr.Length; k++)
                            {
                                if (tbxCellId.Text == dgCell[i, j].Text &&
                                    dgCell.Columns[j].Name == colNameArr[k] && dgCell[i, j].Text == dgCell[i, j + 3].Text)
                                {
                                    dgCell.CurrentCell = dgCell[i, j];
                                    dgCell[i, j].Presenter.IsSelected = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void chkViewSlotNo_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked.HasValue)
                {
                    bool bShowSlotNo = (bool)(sender as CheckBox).IsChecked ? true : false;

                    //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    //    (winTray as PKG_TRAY_MOBILE).ShowSlotNoColumns(bShowSlotNo);
                    //}
                    ShowSlotNoColumns(bShowSlotNo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkViewSlotNo_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as CheckBox).IsChecked.HasValue)
                {
                    bool bShowSlotNo = (bool)(sender as CheckBox).IsChecked ? true : false;

                    //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    //    (winTray as PKG_TRAY_MOBILE).ShowSlotNoColumns(bShowSlotNo);
                    //}
                    ShowSlotNoColumns(bShowSlotNo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCheckElJudge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("IN_EQP");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EL_SPEC_CHK_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.PACKAGING;

                //if (Util.NVC(_EL_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_WEIGHT_USL).Length > 0 &&
                //    Util.NVC(_EL_AFTER_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_AFTER_WEIGHT_USL).Length > 0 &&
                //    Util.NVC(_EL_BEFORE_WEIGHT_LSL).Length > 0 &&
                //    Util.NVC(_EL_BEFORE_WEIGHT_USL).Length > 0 )
                //    newRow["EL_SPEC_CHK_FLAG"] = "Y";
                //else
                newRow["EL_SPEC_CHK_FLAG"] = "N";

                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PKG_CELL_CALC_EL_SPEC", "IN_EQP", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        btnSearchCellInfo.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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

        private void btnSearchCellInfo_Click(object sender, RoutedEventArgs e)
        {
            SearchTrayWindow(bChgNexRow: false);

            GetTrayInfo();

            GetOutRangeCellList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = MessageBoxResult.Cancel;
            try
            {
                this.ClearAllCellInfo();

                grCellList.Visibility = Visibility.Collapsed;

                if (this.Parent != null && this.Parent.GetType() == typeof(ContentControl) &&
                    (this.Parent as ContentControl).Parent != null && (this.Parent as ContentControl).Parent.GetType() == typeof(Grid))
                {
                    UIElementCollection uiec = ((this.Parent as ContentControl).Parent as Grid).Children;

                    if (uiec != null)
                    {
                        foreach (System.Windows.UIElement child in uiec)
                        {
                            if (child.GetType() == typeof(Button))
                            {
                                if ((child as Button).Name.ToUpper().Equals("BTNCLOSE"))
                                {
                                    child.Visibility = Visibility.Visible;
                                }
                                break;
                            }
                        }
                    }
                }

                GetWorkOrder(); // 작지 생산수량 정보 재조회.

                GetProductLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCell_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    if (e.Cell.Row.Index >= 0 && e.Cell.Column.Index > 0)
                    {
                        if (Util.NVC(e.Cell.Column.Name).IndexOf("_") >= 0 || Util.NVC(e.Cell.Column.Name).IndexOf("NO") >= 0) // Hidden Column 인 경우.
                            return;

                        if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_JUDGE")).Equals("EMPT_SLOT")) // 빈 슬롯인 경우..
                            return;

                        string sCell = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_CELLID")); //dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value == null ? "" : dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value.ToString();
                        string sLoc = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_LOC"));


                        if (sCell.IndexOf(new string(TRAY_SHAPE.DISP_SEPARATOR)) >= 0)
                        {
                            string[] sSplList = sCell.Split(TRAY_SHAPE.DISP_SEPARATOR);

                            if (TRAY_SHAPE.DISPLAY_LIST != null && TRAY_SHAPE.DISPLAY_LIST.Length > 0)
                            {
                                int index = Array.FindIndex(TRAY_SHAPE.DISPLAY_LIST, s => s.Contains("CELLID"));
                                if (index >= 0 && index < sSplList.Length)
                                {
                                    sCell = sSplList[index];
                                }
                                else
                                {
                                    sCell = sSplList[0];
                                }
                            }
                            else
                            {
                                sCell = sSplList[0];
                            }
                        }

                        GetCellInfo(sCell.Equals(sLoc) ? "" : sCell, e.Cell.Row.Index, e.Cell.Column.Index, sLoc);
                    }
                }
            }));
        }

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
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
                    if (e.Cell.Column.Name.Equals("NO"))
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        return;
                    }

                    if (e.Cell.Column.Name.IndexOf("_SLOTNO") > 0)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        string sOrgView = Util.NVC(e.Cell.Column.Name).Replace("_SLOTNO", "");
                        if (dgCell.Columns.Contains(sOrgView + "_JUDGE"))
                        {
                            string sTmpJudge = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, sOrgView + "_JUDGE"));

                            if (sTmpJudge.Equals("EMPT_SLOT"))    // Tray 내 빈 슬롯
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                        }

                        return;
                    }

                    if (!dataGrid.Columns.Contains(e.Cell.Column.Name + "_JUDGE"))
                        return;

                    string sJudge = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name + "_JUDGE"));

                    if (sJudge.Equals("SC")) // SC : 특수문자 포함 (Include Special Character) => Cell ID 형식 오류
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("NR")) // NR : 읽을 수 없음 (No Read)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("DL")) // DL : 자리수 상이 (Different ID Length)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D941C5"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("ID")) // ID : ID 중복 (ID Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("PD")) // PD : Tray Location 중복 (Position Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    /* E20240528-000578 : NG Cell 하이라이트 표시 추가[AC,UT,TH,TA,FI 및 N5,6동 소형조립만 사용 */
                    else if (sJudge.Equals("AC") && bCellNg == true) // ACOH : ACOH
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#376092"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("UT") && bCellNg == true) // UT : UTAP
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#558ED5"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("TH") && bCellNg == true) // TH : Thickness
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#C6D9F1"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("TA") && bCellNg == true) // TA : TAPE
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#632523"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("FI") && bCellNg == true) // FI : FILM
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#948A54"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("HD") && bCellNg == true) // HD : ZZS/PKG HOLD
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("NI")) // NI : 주액량 정보 없음 (No Information)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("EMPT_SLOT"))    // Tray 내 빈 슬롯
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        // 아무것도 없는 경우에는 기본 Base 폰트 색 변경.
                        if (e.Cell.Column.Name.IndexOf("_") < 0 && dataGrid.Columns.Contains(e.Cell.Column.Name + "_LOC") && !e.Cell.Column.Name.Equals("NO"))
                        {
                            string sLocVal = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name + "_LOC"));
                            string sCellVal = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name));

                            if (!sLocVal.Equals("EMPT_SLOT") && sCellVal.Trim().Equals(sLocVal)) //sCellVal.Trim().Equals(""))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#B0B0B0"));
                                //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                            }
                            else if (sLocVal.Equals("EMPT_SLOT"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                        }
                    }
                }
            }));
        }

        private void dgCell_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                }
            }));
        }

        private void dgCell_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.D)
                {
                    C1.WPF.DataGrid.C1DataGrid grd = (sender as C1.WPF.DataGrid.C1DataGrid);
                    if (grd != null &&
                        grd.CurrentCell != null &&
                        grd.CurrentCell.Column != null &&
                        !grd.CurrentCell.Column.Name.Equals("NO") &&
                        grd.CurrentCell.Column.Name.IndexOf("_") < 0)
                    {
                        //남경 로직.. OP 사용 로직인지 확인 필요..
                        //DeleteBtnCall();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.LeftButton == MouseButtonState.Released &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    {
                        //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                        //{
                        bool bShowSlotNo = chkViewSlotNo.IsChecked.HasValue && (bool)chkViewSlotNo.IsChecked ? true : false;
                        ShowHideAllColumns(bShowSlotNo);
                        //}
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
            }));
        }

        private void tbCellID_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        //Keyboard.IsKeyDown(Key.F3) &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    txtCellId.IsReadOnly = false;
                    txtEl.IsReadOnly = false;
                    txtBeforeWeight.IsReadOnly = false;
                    txtAfterWeight.IsReadOnly = false;
                    txtHeader.IsReadOnly = false;
                    txtPosition.IsReadOnly = false;
                    txtJudge.IsReadOnly = true;
                    cboTrayLocation.IsEnabled = true;

                    btnSave.IsEnabled = true;
                    btnDelete.IsEnabled = true;

                    btnCheckElJudge.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoELWeight_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                {
                    //if (winTray != null && winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    // Display Set.
                    SetTrayDisplayList(new string[] { "EL_WEIGHT" });

                    // 조회
                    SetCellInfo(bLoad: false, bSameLoc: false, bChgNexRow: false);
                    GetTrayInfo();


                    //rdoManual.IsEnabled = true;
                    //tblEl.Visibility = Visibility.Visible;
                    //txtEl.Visibility = Visibility.Visible;
                    //tblBeforeWeight.Visibility = Visibility.Visible;
                    //txtBeforeWeight.Visibility = Visibility.Visible;
                    //tblAfterWeight.Visibility = Visibility.Visible;
                    //txtAfterWeight.Visibility = Visibility.Visible;
                    //tblHeader.Visibility = Visibility.Visible;
                    //txtHeader.Visibility = Visibility.Visible;
                    //tblPosition.Visibility = Visibility.Visible;
                    //txtPosition.Visibility = Visibility.Visible;
                    //tblJudge.Visibility = Visibility.Visible;
                    //txtJudge.Visibility = Visibility.Visible;
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tbCellIDRuleTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    wndCellIDRule = new ASSY003_007_CELLID_RULE();

                    wndCellIDRule.FrameOperation = FrameOperation;

                    if (wndCellIDRule != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                        C1WindowExtension.SetParameters(wndCellIDRule, Parameters);

                        wndCellIDRule.Closed += new EventHandler(wndCellIDRule_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndCellIDRule.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCellId_KeyUp(object sender, KeyEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            // 권한 없으면 Skip.
            if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                return;

            if (e.Key == Key.Enter) // && rdoAuto.IsChecked.HasValue && (bool)rdoAuto.IsChecked)
            {
                if (!CanCellScan())
                    return;


                if (SaveCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                }
            }
        }

        private void txtCellId_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCellId == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCellId, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtEl_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (e.Key == Key.Enter)
                {
                    if (!CanCellJudge())
                        return;

                    FL_Judge_Check();
                }

                if (!ChkDouble(txtEl.Text, false, -1))
                {
                    txtEl.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBeforeWeight_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (e.Key == Key.Enter)
                {
                    if (!CanCellJudge())
                        return;

                    FL_Judge_Check();
                }

                if (!ChkDouble(txtBeforeWeight.Text, false, -1))
                {
                    txtBeforeWeight.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtAfterWeight_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (e.Key == Key.Enter)
                {
                    if (!CanCellJudge())
                        return;

                    FL_Judge_Check();
                }

                if (!ChkDouble(txtAfterWeight.Text, false, -1))
                {
                    txtAfterWeight.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtHeader_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (!ChkDouble(txtHeader.Text, false, -1))
                {
                    txtHeader.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPosition_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // ReadOnly
                if (_OnlyView) return;

                if (e.Key == Key.Enter)
                {
                    if (!CanCellJudge())
                        return;

                    FL_Judge_Check();
                }

                //if (!ChkDouble(txtPosition.Text, false, -1))
                //{
                //    txtPosition.Text = "";
                //    return;
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtJudge_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            FL_Judge_Check();

            if (!CanCellModify())
                return;

            if (SaveCell())
            {
                SearchTrayWindow();

                GetTrayInfo();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // ReadOnly
            if (_OnlyView) return;

            // E20240528-000578 : N5,6동 소형조립인경우 삭제팝업 호출 분기처리
            if(bCellNg == true)
            {
                CheckDeleteCell();
            }   
            else
            {
                if (!CanDelete())
                    return;

                if (DeleteCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                }
            }
        }

        private void btnDupDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDelete())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgDupList == null || dgDupList.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            if (DeleteCell(Util.NVC(dtRow["SUBLOTID"])))
                            {
                                SearchTrayWindow(false, true);

                                GetTrayInfo();

                                GetDupLocList(Util.NVC(dtRow["CSTSLOT"]));
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutRangeDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDeleteOutRange())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgOutRangeList == null || dgOutRangeList.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            if (DeleteCell(Util.NVC(dtRow["SUBLOTID"])))
                            {
                                GetTrayInfo();
                                GetOutRangeCellList();
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutRangeDelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 권한 없으면 Skip.
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                // ReadOnly
                if (_OnlyView) return;

                if (!CanDeleteOutRange())
                    return;

                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgOutRangeList == null || dgOutRangeList.Rows.Count < 1)
                            return;

                        for (int i = 0; i < dgOutRangeList.Rows.Count; i++)
                        {
                            DeleteCell(Util.NVC(DataTableConverter.GetValue(dgOutRangeList.Rows[i].DataItem, "SUBLOTID")));
                        }

                        GetTrayInfo();
                        GetOutRangeCellList();

                        Util.MessageInfo("SFU1273");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Method]
        #region [Func]
        private void ShowCellInfoPopup(string sTrayID, string sOutLOTID, string sTrayQty)
        {
            try
            {
                this.ClearAllCellInfo();

                if (sTrayID.Equals("")) return;
                if (sOutLOTID.Equals("")) return;

                _TrayID = sTrayID;
                _TrayQty = sTrayQty;
                _OutLotID = sOutLOTID;

                grCellList.Visibility = Visibility.Visible;

                if (this.Parent != null && this.Parent.GetType() == typeof(ContentControl) &&
                    (this.Parent as ContentControl).Parent != null && (this.Parent as ContentControl).Parent.GetType() == typeof(Grid))
                {
                    UIElementCollection uiec = ((this.Parent as ContentControl).Parent as Grid).Children;

                    if (uiec != null)
                    {
                        foreach (System.Windows.UIElement child in uiec)
                        {
                            if (child.GetType() == typeof(Button))
                            {
                                if ((child as Button).Name.ToUpper().Equals("BTNCLOSE"))
                                {
                                    child.Visibility = Visibility.Collapsed;
                                }
                                break;
                            }
                        }
                    }
                }

                grdDupList.Visibility = Visibility.Collapsed;
                grdOutRangeList.Visibility = Visibility.Collapsed;

                // Slot No. 표시 처리
                //_ShowSlotNo = true;
                //if (chkViewSlotNo != null && chkViewSlotNo.IsChecked.HasValue)
                //    chkViewSlotNo.IsChecked = true; 

                chkViewSlotNo.Visibility = Visibility.Visible;


                ApplyPermissions();

                // 기본 Display 설정            
                if (rdoCellID != null && rdoCellID.IsChecked != null)
                {
                    rdoCellID.Checked -= rdoCellID_Checked;
                    rdoCellID.IsChecked = true;
                    rdoCellID.Checked += rdoCellID_Checked;
                }

                // E20231013-001274 적합성 체크 예외 여부
                GetCompatibilityChkExceptFlag();
                
                // 주액 DATA 관리 여부
                GetElDataMngtFlag();

                InitializeControls();

                SetTrayWindow();
                //setTryLoction();
                SetBasicInfo();

                ChangeMode("ALL");  // 컨트롤 모두 View 처리...


                // Tray Grid Set.
                InitializeGrid();

                SetCellInfo(true, false, true);

                if (_ShowSlotNo)
                {
                    ShowSlotNoColumns(true);
                }
                else
                    ShowSlotNoColumns(false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearAllCellInfo()
        {
            try
            {
                // 내부 사용 변수
                _TrayID = string.Empty;
                _TrayQty = string.Empty;
                _OutLotID = string.Empty;

                _CompatibilityChkExceptFlag = false;
                _ELUseYN = false;
                originColorMap = null;

                _ShowSlotNo = false;
                _OnlyView = false;

                // 주액 USL, LSL 기준정보
                _EL_WEIGHT_LSL = string.Empty;
                _EL_WEIGHT_USL = string.Empty;
                _EL_AFTER_WEIGHT_LSL = string.Empty;
                _EL_AFTER_WEIGHT_USL = string.Empty;
                _EL_BEFORE_WEIGHT_LSL = string.Empty;
                _EL_BEFORE_WEIGHT_USL = string.Empty;

                bExistLayOutInfo = false;
                bViewAll = false;

                TRAY_SHAPE.CELL_TYPE = string.Empty;
                TRAY_SHAPE.ROW_NUM = 0;
                TRAY_SHAPE.COL_NUM = 0;
                TRAY_SHAPE.EMPTY_SLOT = false;
                TRAY_SHAPE.ZIGZAG = false;
                TRAY_SHAPE.EMPTY_SLOT_LIST = null;
                TRAY_SHAPE.MERGE_START_COL_NUM = 0;
                TRAY_SHAPE.DISPLAY_LIST = null;
                TRAY_SHAPE.DISP_SEPARATOR = null;

                // 화면 Display Control
                Util.gridClear(dgCell);
                Util.gridClear(dgDupList);
                Util.gridClear(dgOutRangeList);

                tbxCellId.Text = "";
                txtLotId.Text = "";
                txtTrayId.Text = "";
                txtCellCnt.Text = "";
                txtDefaultWeight.Text = "";
                txtCellId.Text = "";
                txtEl.Text = "";
                tblElMinMax.Text = "";
                txtBeforeWeight.Text = "";
                tblBeforeWeightMinMax.Text = "";
                txtAfterWeight.Text = "";
                tblAfterWeightMinMax.Text = "";
                txtHeader.Text = "";
                txtPosition.Text = "";
                txtJudge.Text = "";


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTrayWindow()
        {
            // Tray Layout 정보 조회.
            GetTrayLayoutInfo();
        }

        private void SetZigZagGridInfo()
        {
            try
            {
                if (dgCell == null)
                    return;

                int iRowNum = 1;

                // LOCATION 정보 SET.
                int iLocIdx = 1; // 실제 Loc Number
                int iAsciiIdx = 0; // 실제 Col Index
                int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                int iMergIdx = string.IsNullOrEmpty(TRAY_SHAPE.MERGE_START_COL_NUM.ToString()) ? 1 : TRAY_SHAPE.MERGE_START_COL_NUM;   // 0 row 부터 머지 처리할 index 변수.
                string sTmpColName = "";


                for (int idxCol = 0; idxCol < dgCell.Columns.Count; idxCol++)
                {
                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iAsciiIdx++;
                            //iMergIdx++;
                        }
                    }

                    for (int idxRow = 0; idxRow < dgCell.Rows.Count; idxRow++)
                    {
                        // 빈 슬롯 번호 확인하여 Empty Slot 설정.
                        if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                            {
                                if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[idxCol].Name) + "_JUDGE"))
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name) + "_JUDGE", "EMPT_SLOT");
                                if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[idxCol].Name) + "_LOC"))
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name) + "_LOC", "EMPT_SLOT");
                            }
                            else
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT") &&
                                    !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                                {
                                    // Location 정보 다음 Cell index 값 Set 후 아래 로직에서 Reset 되는 문제로...
                                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", ""))).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name))) &&
                                        Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_SLOTNO") < 0
                                        )
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                }
                            }
                        }
                        else
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT") &&
                                !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                            {
                                // Location 정보 다음 Cell index 값 Set 후 아래 로직에서 Reset 되는 문제로...
                                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_LOC") >= 0 &&
                                    !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", ""))).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name))) &&
                                    Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_SLOTNO") < 0
                                   )
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                            }
                        }


                        // Row Number 설정
                        if (Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null && TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))  // Empty slot
                            {
                                DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name), "");
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name), (iRowNum).ToString());

                                if (idxRow % 2 != 0 && idxRow != dgCell.Rows.Count - 1)
                                {
                                    iRowNum = iRowNum + 1;
                                }
                            }
                        }
                        else
                        {
                            if (iMergIdx % 2 == 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                            {
                                // Location Number 설정.
                                if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                                {
                                    if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }
                                    else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dgCell.Rows.Count - 1)
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }


                                    // Location 정보 Set (View 용)
                                    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");

                                    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                    {
                                        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", sTmpLocValue);
                                        else
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", "");
                                    }
                                }
                                //// Location 정보 Set (View 용)
                                //else if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0)
                                //{
                                //    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_SLOTNO", "");

                                //    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                //        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                //    {
                                //        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                //        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, sTmpLocValue);
                                //        else
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                //    }
                                //}
                            }
                            else
                            {
                                // Location Number 설정.
                                if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                                {
                                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)  // A Column
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }


                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }
                                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dgCell.Rows.Count - 1) // B Column
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }


                                    // Location 정보 Set (View 용)
                                    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");

                                    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                    {
                                        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", sTmpLocValue);
                                        else
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", "");
                                    }
                                }
                                //// Location 정보 Set (View 용)
                                //else if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0)
                                //{
                                //    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_SLOTNO", "");

                                //    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                //        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                //    {
                                //        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                //        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, sTmpLocValue);
                                //        else
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                //    }
                                //}
                            }
                        }


                        // View 컬럼 기준으로 슬롯 넘버링 처리.
                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                        {
                            iTmpIdx++;
                        }

                        dgCell.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                    }

                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iMergIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetTrayDisplayList(string[] strList)
        {
            TRAY_SHAPE.DISPLAY_LIST = strList == null ? new string[] { "CELLID" } : strList;
        }

        public void SetCellInfo(bool bLoad, bool bSameLoc, bool bChgNexRow)
        {
            try
            {
                if (string.IsNullOrEmpty(TRAY_SHAPE.CELL_TYPE))
                    return;

                ShowLoadingIndicator();

                int iRow = 0;
                int iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1;

                if (!bLoad)
                    GetNextCellPos(out iRow, out iCol, bSameLoc: bSameLoc, bChgNexRow: bChgNexRow);

                ClearCellInfo();

                // Cell List 조회.
                DataTable dtResult = GetTrayCellList();

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (!Util.NVC(dtResult.Rows[i]["LOCATION"]).Equals(""))
                        {
                            int iTmpLoc = 0;
                            if (int.TryParse(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iTmpLoc))
                            {
                                // Grid 내에 해당 Location 좌표 조회
                                int iFndRow, iFndCol;
                                string sViewColName;

                                //FindLocXY(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iFndRow, out iFndCol, out sViewColName);
                                FindLocXYByLinq(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iFndRow, out iFndCol, out sViewColName);

                                if (!sViewColName.Equals("") &&
                                    dgCell.Columns.Contains(sViewColName) &&
                                    dgCell.Columns.Contains(sViewColName + "_JUDGE") &&
                                    iFndRow > -1)
                                {
                                    // OK 가 아닌 경우에는 DATA SET 후 화면 색 표시 처리.
                                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE")).Equals("") ||
                                        Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE")).Equals("OK"))
                                    {
                                        string sTmpDisp = ""; // Util.NVC(dtResult.Rows[i]["CELLID"]); 


                                        if (TRAY_SHAPE.DISPLAY_LIST != null)
                                        {
                                            for (int iDsp = 0; iDsp < TRAY_SHAPE.DISPLAY_LIST.Length; iDsp++)
                                            {
                                                if (dtResult.Columns.Contains(Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])))
                                                {
                                                    if (sTmpDisp.Equals(""))
                                                        sTmpDisp = Util.NVC(dtResult.Rows[i][Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])]);
                                                    else
                                                        sTmpDisp = sTmpDisp + new string(TRAY_SHAPE.DISP_SEPARATOR) + Util.NVC(dtResult.Rows[i][Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])]);
                                                }
                                                else
                                                {
                                                    if (sTmpDisp.Equals(""))
                                                        sTmpDisp = Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp]);
                                                    else
                                                        sTmpDisp = sTmpDisp + new string(TRAY_SHAPE.DISP_SEPARATOR) + Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (dtResult.Columns.Contains("CELLID"))
                                                sTmpDisp = Util.NVC(dtResult.Rows[i]["CELLID"]);
                                        }

                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName, sTmpDisp);
                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE", Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_CELLID", Util.NVC(dtResult.Rows[i]["CELLID"]));

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > iFndRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName, sTmpDisp);
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName + "_JUDGE", Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName + "_CELLID", Util.NVC(dtResult.Rows[i]["CELLID"]));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!bLoad)
                {
                    DataTable dtTemp = DataTableConverter.Convert(dgCell.ItemsSource);

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();
                }

                if (dgCell.Rows.Count > iRow && dgCell.Columns.Count > iCol && iRow > -1 && iCol > -1)
                {
                    Util.SetDataGridCurrentCell(dgCell, dgCell[iRow, iCol]);

                    dgCell.CurrentCell = dgCell.GetCell(iRow, iCol);
                    dgCell.ScrollIntoView(iRow, iCol);

                    if (Util.NVC(dgCell.Columns[iCol].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[iCol].Name).Equals("NO") &&
                        dgCell.Columns.Contains(Util.NVC(dgCell.Columns[iCol].Name) + "_LOC"))
                    {
                        //loadcellpresenter 콜을 위해 itemsouce 다시 set 시 current cell 오류로.. index로 직접 콜 하도록 변경.
                        string sCell = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, Util.NVC(dgCell.Columns[iCol].Name) + "_CELLID")); //dgCell.GetCell(iRow, iCol).Value == null ? "" : dgCell.GetCell(iRow, iCol).Value.ToString();
                        string sLoc = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, Util.NVC(dgCell.Columns[iCol].Name) + "_LOC"));

                        // 첫 Row 가 Empty slot 이면 Current Cell 1번 설정.
                        if (iRow == 0 && sLoc.Equals("EMPT_SLOT"))
                        {
                            int iFirstRow = Util.gridFindDataRow(ref dgCell, dgCell.Columns[iCol].Name + "_SLOTNO", "1", true);

                            if (iFirstRow > 0)
                            {
                                iRow = iFirstRow;
                                Util.SetDataGridCurrentCell(dgCell, dgCell[iRow, iCol]);

                                dgCell.CurrentCell = dgCell.GetCell(iRow, iCol);
                                dgCell.ScrollIntoView(iRow, iCol);
                            }
                        }

                        if (sCell.IndexOf(new string(TRAY_SHAPE.DISP_SEPARATOR)) >= 0)
                        {
                            string[] sSplList = sCell.Split(TRAY_SHAPE.DISP_SEPARATOR);

                            if (TRAY_SHAPE.DISPLAY_LIST != null && TRAY_SHAPE.DISPLAY_LIST.Length > 0)
                            {
                                int index = Array.FindIndex(TRAY_SHAPE.DISPLAY_LIST, s => s.Contains("CELLID"));
                                if (index >= 0 && index < sSplList.Length)
                                {
                                    sCell = sSplList[index];
                                }
                                else
                                {
                                    sCell = sSplList[0];
                                }
                            }
                            else
                            {
                                sCell = sSplList[0];
                            }
                        }

                        GetCellInfo(sCell.Equals(sLoc) ? "" : sCell, iRow, iCol, sLoc);
                    }
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

        private void GetNextCellPos(out int iRow, out int iCol, bool bSameLoc = false, bool bChgNexRow = true)
        {
            if (dgCell.CurrentCell != null)
            {
                if (dgCell.CurrentCell.Row != null)
                {
                    if (bSameLoc)   // 동일 로케이션 
                    {
                        iRow = dgCell.CurrentCell.Row.Index < 0 ? 0 : dgCell.CurrentCell.Row.Index;
                        iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                    }
                    else
                    {
                        string sTmpColName = Util.NVC(dgCell.CurrentCell.Column.Name);

                        if (sTmpColName.IndexOf("_") < 0 && !sTmpColName.Equals("NO") && dgCell.Columns.Contains(sTmpColName + "_LOC"))
                        {
                            string sPrvLoc = Util.NVC(DataTableConverter.GetValue(dgCell.CurrentCell.Row.DataItem, sTmpColName + "_LOC"));

                            int iTmp = 0;
                            //int iFndRow, iFndCol;
                            string sViewColName;

                            int.TryParse(sPrvLoc, out iTmp);

                            //FindLocXY((iTmp + 1).ToString(), out iRow, out iCol, out sViewColName);
                            FindLocXYByLinq((iTmp + 1).ToString(), out iRow, out iCol, out sViewColName);

                            if (!sViewColName.Equals(""))
                                iCol = dgCell.Columns[sViewColName].Index < 0 ? iCol : dgCell.Columns[sViewColName].Index;

                            // 동일 Row 유지 확인
                            if (!bChgNexRow)
                            {
                                int iTmpPrvRow = 0;
                                int iTmpPrvCol = 0;
                                string sTmpViewColName = "";

                                FindLocXYByLinq((iTmp).ToString(), out iTmpPrvRow, out iTmpPrvCol, out sTmpViewColName);

                                iRow = iTmpPrvRow;
                            }
                        }
                        else
                        {
                            iRow = 0;
                            iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1;
                        }
                    }
                }
                else
                {
                    iRow = 0;

                    if (dgCell.CurrentCell.Column != null)
                        iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                    else
                        iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1; ;
                }
            }
            else
            {
                iRow = 0;
                iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1; ;
            }
        }

        private void ClearCellInfo()
        {
            try
            {
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                    {
                        if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                        {
                            if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_LOC") &&
                                !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC")).Equals("EMPT_SLOT"))
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC")));
                            else
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                        else if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_JUDGE") >= 0)  // 판정 컬럼 초기화
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name))).Equals("EMPT_SLOT"))
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                        else if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_CELLID") >= 0)  // CELL ID 컬럼 초기화
                        {
                            DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                    }
                }

                //ParentClearInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetBasicInfo()
        {
            txtLotId.Text = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
            txtTrayId.Text = _TrayID;

            GetTrayInfo();

            GetOutRangeCellList();

            // 주액 MIN MAX 관리 안하기로 하여 주석..
            //GetElJudgeInfo();
        }

        private void SetOriginColorMap()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (dgCell == null) return;

                //C1.WPF.DataGrid.C1DataGrid dgdCell = (C1.WPF.DataGrid.C1DataGrid)winTray.FindName("dgCell");
                originColorMap = new Brush[dgCell.Rows.Count, dgCell.Columns.Count];

                for (int i = 0; i < dgCell.Rows.Count; i++)
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                        if (dgCell[i, j].Presenter != null)
                            originColorMap[i, j] = dgCell[i, j].Presenter.Background;
            })
            );
        }

        private void FindLocXYByLinq(string sFindText, out int iFndRow, out int iFndCol, out string sViewColName)
        {
            iFndRow = -1;
            iFndCol = -1;
            sViewColName = "";

            DataTable dt = ((DataView)dgCell.ItemsSource).Table;
            DataRow row;

            for (int col = 0; col < dgCell.Columns.Count; col++)
            {
                if (Util.NVC(dgCell.Columns[col].Name).IndexOf("_LOC") < 0) continue;

                row = (from t in dt.AsEnumerable()
                       where (t.Field<string>(Util.NVC(dgCell.Columns[col].Name)) == sFindText)
                       select t).FirstOrDefault();

                if (row != null)
                {
                    //idx = dt.Rows.IndexOf(row) + 1;
                    iFndRow = dt.Rows.IndexOf(row);
                    iFndCol = dgCell.Columns[col].Index;

                    sViewColName = Util.NVC(dgCell.Columns[col].Name).Replace("_LOC", "");
                    return;
                }
            }
        }

        private void ChangeMode(string sMode)
        {
            if (sMode.Equals("ALL"))
            {
                txtCellId.IsReadOnly = false;
                //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
                txtEl.IsReadOnly = false;
                txtBeforeWeight.IsReadOnly = false;
                txtAfterWeight.IsReadOnly = false;
                txtHeader.IsReadOnly = false;
                txtPosition.IsReadOnly = false;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;
            }
            else if (sMode.Equals("AUTO"))
            {
                txtCellId.IsReadOnly = false;
                //txtCellId.Background = new SolidColorBrush(Colors.Transparent);
                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = true;
            }
            else
            {
                txtCellId.IsReadOnly = true;
                txtEl.IsReadOnly = false;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = false;
                txtHeader.IsReadOnly = false;
                txtPosition.IsReadOnly = false;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;
            }


            if (!bExistLayOutInfo)
            {
                SetLayoutNone();
            }
            else
            {
                //txtCellId.IsReadOnly = false;
                //txtEl.IsReadOnly = false;
                //txtBeforeWeight.IsReadOnly = false;
                //txtAfterWeight.IsReadOnly = false;
                //txtHeader.IsReadOnly = false;
                //txtPosition.IsReadOnly = false;
                //txtJudge.IsReadOnly = true;
                //cboTrayLocation.IsEnabled = true;

                btnSave.IsEnabled = true;
                btnDelete.IsEnabled = true;

                btnCheckElJudge.IsEnabled = true;
            }
        }

        public void SetLayoutNone()
        {
            try
            {
                txtCellId.IsReadOnly = true;
                txtEl.IsReadOnly = true;
                txtBeforeWeight.IsReadOnly = true;
                txtAfterWeight.IsReadOnly = true;
                txtHeader.IsReadOnly = true;
                txtPosition.IsReadOnly = true;
                txtJudge.IsReadOnly = true;
                cboTrayLocation.IsEnabled = false;

                btnSave.IsEnabled = false;
                btnDelete.IsEnabled = false;
                btnOutRangeDelAll.IsEnabled = false;

                btnCheckElJudge.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckLocationDup()
        {
            try
            {
                if (cboTrayLocation == null || cboTrayLocation.SelectedValue == null)
                    return;

                DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

                if (dtTmp == null || dtTmp.Rows.Count < 1 || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0") || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("1"))
                {
                    grdDupList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grdDupList.Visibility = Visibility.Visible;
                    grdOutRangeList.Visibility = Visibility.Collapsed;

                    GetDupLocList(cboTrayLocation.SelectedValue.ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchTrayWindow(bool bLoad = false, bool bSameLoc = false, bool bChgNexRow = true)
        {
            SetCellInfo(bLoad, bSameLoc, bChgNexRow);
        }

        private void ClearControl()
        {
            txtDefaultWeight.Text = "";
            txtCellId.Text = "";
            txtEl.Text = "";
            txtBeforeWeight.Text = "";
            txtAfterWeight.Text = "";
        }

        public void ShowSlotNoColumns(bool bShowSlotNo)
        {
            try
            {
                if (dgCell == null)
                    return;

                bool bFirsNo = true;
                for (int i = 0; i < dgCell.Columns.Count; i++)
                {
                    if (dgCell.Columns[i].Name.IndexOf("_SLOTNO") >= 0)
                    {
                        if (bFirsNo)
                        {
                            dgCell.Columns[i].Visibility = Visibility.Visible;
                            bFirsNo = false;
                        }
                        else
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns[i].Visibility = Visibility.Visible;

                                if (dgCell.Columns.Contains("NO"))
                                    dgCell.Columns["NO"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;

                                if (dgCell.Columns.Contains("NO"))
                                    dgCell.Columns["NO"].Visibility = Visibility.Collapsed; //Visibility.Visible;
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

        public void ShowHideAllColumns(bool bShowSlotNo)
        {
            if (dgCell == null)
                return;

            if (bViewAll)
                bViewAll = false;
            else
                bViewAll = true;

            for (int i = 0; i < dgCell.Columns.Count; i++)
            {
                if (bViewAll)
                {
                    dgCell.Columns[i].Visibility = Visibility.Visible;
                }
                else
                {
                    if (Util.NVC(dgCell.Columns[i].Name).Length > 2)
                    {
                        if (dgCell.Columns.Contains("NO"))
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns["NO"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                dgCell.Columns["NO"].Visibility = Visibility.Visible;
                            }
                        }

                        if (dgCell.Columns[i].Name.IndexOf("_SLOTNO") >= 0)
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns[i].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            if (dgCell.Columns[i].Visibility == Visibility.Visible)
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;
                            else if (dgCell.Columns[i].Visibility == Visibility.Collapsed)
                                dgCell.Columns[i].Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void wndCellIDRule_Closed(object sender, EventArgs e)
        {
            ASSY003_007_CELLID_RULE window = sender as ASSY003_007_CELLID_RULE;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            wndCellIDRule = null;
        }

        private bool ChkDouble(string str, bool bUseMin, double dMinValue)
        {
            try
            {
                bool bRet = false;

                if (str.Trim().Equals(""))
                    return bRet;

                if (str.Trim().Equals("-"))
                    return true;

                double value;
                if (!double.TryParse(str, out value))
                {
                    //숫자필드에 부적절한 값이 입력 되었습니다.
                    Util.MessageValidation("SFU2914");
                    return bRet;
                }
                if (bUseMin && value < dMinValue)
                {
                    //숫자필드에 허용되지 않는 값이 입력 되었습니다.
                    Util.MessageValidation("SFU2915");
                    return bRet;
                }

                bRet = true;

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public static void SetTrayShape(string sCellType, int iRowCnt, int iColCnt,
                                        bool bEmptySlot, bool bZigZag, string[] emptySlotList,
                                        int iMergeStartCol = 0, string[] displayList = null, char[] dispSeparator = null)
        {
            TRAY_SHAPE.CELL_TYPE = sCellType;  // CELL TYPE
            TRAY_SHAPE.ROW_NUM = bZigZag ? iRowCnt * 2 : iRowCnt;  // 총 ROW 수 (zigzag이면 Merge를 위해 2배로 생성)
            TRAY_SHAPE.COL_NUM = iColCnt;  // 총 COL 수
            TRAY_SHAPE.EMPTY_SLOT = bEmptySlot;  // 빈 슬롯 존재 여부
            TRAY_SHAPE.ZIGZAG = bZigZag;  // COL 별 지그재그 배치 여부
            TRAY_SHAPE.EMPTY_SLOT_LIST = emptySlotList;  // 빈 슬롯 번호 LIST
            TRAY_SHAPE.MERGE_START_COL_NUM = iMergeStartCol;    // 머지 시작 컬럼 번호.
            TRAY_SHAPE.DISPLAY_LIST = displayList == null ? new string[] { "CELLID" } : displayList;  // Cell 영역에 표시할 Data List
            TRAY_SHAPE.DISP_SEPARATOR = dispSeparator == null ? new char[] { ',' } : dispSeparator; // 표시 영역 구분자
        }
        #endregion

        #region [BizRule]
        private void GetTrayLayoutInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CST_TYPE_CODE"] = _TrayID.Length > 4 ? _TrayID.Substring(0, 4) : _TrayID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CST_LAYOUT_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    bExistLayOutInfo = true;

                    int iRowNum = Util.NVC(dtRslt.Rows[0]["ROW_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_NUM"]));
                    int iColNum = Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]));
                    string sRowTypeCode = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]);
                    string[] sEmptySlotList = Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    int iRowMergeStrtCol = Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]));
                    string[] sDispInfo = Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    char[] cDispDelimeter = Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).Equals("") ? null : Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).ToCharArray();
                    bool bEmptySlot = sEmptySlotList != null && sEmptySlotList.Length > 0 ? true : false;
                    bool bZigZag = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]).Equals("Z") ? true : false;

                    SetTrayShape(Util.NVC(dtRslt.Rows[0]["CST_CELL_QTY"]), iRowNum, iColNum, bEmptySlot, bZigZag, sEmptySlotList, iMergeStartCol: iRowMergeStrtCol, displayList: sDispInfo, dispSeparator: cDispDelimeter);
                }
                else
                {
                    // 남경인 경우 AREA 로 없으면 AREA 없이 CST_TYPE_CODE 코드로만 조회 후 첫번째 LAYOUT 표시 요청에 의한 코드 추가.
                    GetTrayLayoutInfoWithArea();

                    //bExistLayOutInfo = false;

                    //SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                    //// 데이터 오류 [캐리어 레이아웃 기준정보 누락 - PI팀에 데이터 확인 요청]
                    //Util.MessageValidation("SFU3630");                    
                }
            }
            catch (Exception ex)
            {
                bExistLayOutInfo = false;

                SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTrayLayoutInfoWithArea()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CST_TYPE_CODE"] = _TrayID.Length > 4 ? _TrayID.Substring(0, 4) : _TrayID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CST_LAYOUT_INFO_WITHOUT_AREAID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    bExistLayOutInfo = true;

                    int iRowNum = Util.NVC(dtRslt.Rows[0]["ROW_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_NUM"]));
                    int iColNum = Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]));
                    string sRowTypeCode = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]);
                    string[] sEmptySlotList = Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    int iRowMergeStrtCol = Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]));
                    string[] sDispInfo = Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    char[] cDispDelimeter = Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).Equals("") ? null : Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).ToCharArray();
                    bool bEmptySlot = sEmptySlotList != null && sEmptySlotList.Length > 0 ? true : false;
                    bool bZigZag = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]).Equals("Z") ? true : false;

                    SetTrayShape(Util.NVC(dtRslt.Rows[0]["CST_CELL_QTY"]), iRowNum, iColNum, bEmptySlot, bZigZag, sEmptySlotList, iMergeStartCol: iRowMergeStrtCol, displayList: sDispInfo, dispSeparator: cDispDelimeter);
                }
                else
                {
                    bExistLayOutInfo = false;

                    SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                    // 데이터 오류 [캐리어 레이아웃 기준정보 누락 - PI팀에 데이터 확인 요청]
                    Util.MessageValidation("SFU3630");
                }
            }
            catch (Exception ex)
            {
                bExistLayOutInfo = false;

                SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetCompatibilityChkExceptFlag()
        {
            try
            {
                ShowLoadingIndicator();
                
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = "COMPATIBILITY_CHK_EXCEPT"; // 적합성 체크 예외
                newRow["COM_CODE"] = "EXCEPT_FLAG";

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    _CompatibilityChkExceptFlag = true;
                }
                else
                {
                    _CompatibilityChkExceptFlag = false;
                }
            }
            catch (Exception ex)
            {
                _CompatibilityChkExceptFlag = false;

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetElDataMngtFlag()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EL_DATA_MNGT_FLAG_CL", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("EL_DATA_USE_YN") && Util.NVC(dtRslt.Rows[0]["EL_DATA_USE_YN"]).Equals("Y"))
                    {
                        _ELUseYN = true;
                    }
                    else
                    {
                        _ELUseYN = false;
                    }
                }
                else
                {
                    _ELUseYN = false;
                }
            }
            catch (Exception ex)
            {
                _ELUseYN = false;

                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        public DataTable GetTrayCellList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_TRAY_CELL_LIST();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                // E20240528-000578 NG Cell 추가는 N5,6동 파우치만 적용
                string bzName = "DA_PRD_SEL_CELL_LIST";
                if(bCellNg == true)
                {
                    bzName = "DA_PRD_SEL_CELL_LIST_NJ"; //
                }

                return new ClientProxy().ExecuteServiceSync(bzName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTrayInfo()
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
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            txtCellCnt.Text = Double.Parse(Util.NVC(searchResult.Rows[0]["CELLQTY"])).ToString();
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetOutRangeCellList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CST_TYPE_CODE"] = _TrayID.Substring(0, 4);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_OUT_OF_RANGE_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgOutRangeList.ItemsSource = DataTableConverter.Convert(searchResult);

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            grdDupList.Visibility = Visibility.Collapsed;
                            grdOutRangeList.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            grdDupList.Visibility = Visibility.Collapsed;
                            grdOutRangeList.Visibility = Visibility.Collapsed;
                        }

                        //SetOriginColorMap();
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

        public void GetCellInfo(string sCellID, int iRow, int iCol, string sLoc = "")
        {
            try
            {
                ShowLoadingIndicator();

                txtCellId.Text = sCellID;

                // 주액 정보
                txtEl.Text = "";
                txtBeforeWeight.Text = "";
                txtAfterWeight.Text = "";
                txtHeader.Text = "";
                txtPosition.Text = "";
                txtJudge.Text = "";

                DataTable inTable = _Biz.GetDA_PRD_SEL_CELL_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["TRAYID"] = _TrayID;
                newRow["CELLID"] = sCellID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_INFO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtEl.Text = dtResult.Columns.Contains("EL_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_WEIGHT"]) : "";
                    txtBeforeWeight.Text = dtResult.Columns.Contains("EL_PRE_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_PRE_WEIGHT"]) : "";
                    txtAfterWeight.Text = dtResult.Columns.Contains("EL_AFTER_WEIGHT") ? Util.NVC(dtResult.Rows[0]["EL_AFTER_WEIGHT"]) : "";
                    txtHeader.Text = dtResult.Columns.Contains("HEADER") ? Util.NVC(dtResult.Rows[0]["HEADER"]) : "";
                    txtPosition.Text = dtResult.Columns.Contains("EL_PSTN") ? Util.NVC(dtResult.Rows[0]["EL_PSTN"]) : "";
                    txtJudge.Text = dtResult.Columns.Contains("EL_JUDG_VALUE") ? Util.NVC(dtResult.Rows[0]["EL_JUDG_VALUE"]) : "";

                    int iLoc = 0;
                    int.TryParse(Util.NVC(dtResult.Rows[0]["LOCATION"]), out iLoc);

                    //if(cboTrayLocation.Items.Contains(iLoc))
                    //    cboTrayLocation.SelectedValue = iLoc;
                    if (cboTrayLocation.Items.Count >= iLoc)
                    {
                        cboTrayLocation.SelectedIndex = iLoc - 1;

                        //if (winTray25 == null)
                        //    return;

                        //winTray25.SetNexPos(iLoc - 1);
                    }


                    // 해당 위치에 Cell 정보 조회 후 중복건인경우 중복 List View 처리.
                    CheckLocationDup();
                }
                else
                {
                    //if (winTray.GetType() == typeof(PKG_TRAY_MOBILE))
                    //{
                    int iLoc = 0;

                    int.TryParse(sLoc, out iLoc);

                    if (cboTrayLocation.Items.Count >= iLoc)
                    {
                        if (iLoc == 0)
                            cboTrayLocation.SelectedValue = 1;
                        else
                            cboTrayLocation.SelectedValue = iLoc;

                        //if (winTray25 == null)
                        //    return;

                        //winTray25.SetNexPos(iLoc);
                    }
                    //}
                    //else
                    //{
                    //    int iMaxRow = 25;

                    //    int iLoc = 0;
                    //    iLoc = iRow + (iCol > 1 ? iMaxRow : 0);

                    //    if (cboTrayLocation.Items.Count >= iLoc)
                    //    {
                    //        cboTrayLocation.SelectedIndex = iLoc;

                    //        //if (winTray25 == null)
                    //        //    return;

                    //        //winTray25.SetNexPos(iLoc);
                    //    }
                    //}

                    grdDupList.Visibility = Visibility.Collapsed;
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetCellLocCount(string sLoc)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_SEL_TRAY_LOCATION_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["CSTSLOT"] = sLoc;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_LOCATION_CNT", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                return dtRslt;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetDupLocList(string sLoc)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_SEL_DUP_TRAY_LOCATION_LIST();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["CSTSLOT"] = sLoc;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_DUP_TRAY_LOCATION_LIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgDupList.ItemsSource = DataTableConverter.Convert(searchResult);
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

        private bool SaveCell()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S();

                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["CSTID"] = _TrayID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_CST"];
                newRow = inSublotTable.NewRow();
                newRow["SUBLOTID"] = txtCellId.Text.Trim();
                newRow["CSTSLOT"] = cboTrayLocation.SelectedValue.ToString();

                inSublotTable.Rows.Add(newRow);

                if (_ELUseYN)
                {
                    DataTable inElTable = indataSet.Tables["IN_EL"];
                    newRow = inElTable.NewRow();
                    newRow["SUBLOTID"] = txtCellId.Text.Trim();
                    newRow["EL_PRE_WEIGHT"] = txtBeforeWeight.Text.Trim();
                    newRow["EL_AFTER_WEIGHT"] = txtAfterWeight.Text.Trim();
                    newRow["EL_WEIGHT"] = txtEl.Text.Trim();
                    newRow["EL_PSTN"] = txtPosition.Text.Trim();
                    newRow["EL_JUDG_VALUE"] = txtJudge.Text.Trim();

                    inElTable.Rows.Add(newRow);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL_S_UI", "IN_EQP,IN_CST,IN_EL", "OUTDATA", indataSet);

                if (dsRslt != null && dsRslt.Tables.Count > 0 && dsRslt.Tables["OUTDATA"] != null && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string sRet = string.Empty;
                    string sMsg = string.Empty;

                    if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("OK"))
                    {
                        sRet = "OK";
                        sMsg = "";// Util.NVC(dtResult.Rows[0][1]);
                    }
                    else
                    {
                        sRet = "NG";

                        if (dsRslt.Tables["OUTDATA"].Columns.Contains("NG_MSG"))
                            sMsg = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSG"]).Trim();
                        else
                            sMsg = "";

                        //if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("SC"))
                        //{
                        //    sMsg = "SFU3049";  // 특수문자가 포함되어 있습니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NR"))
                        //{
                        //    sMsg = "SFU3050";    // 읽을 수 없습니다
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("DL"))
                        //{
                        //    sMsg = "SFU3051";    // CELL ID 자리수가 잘못 되었습니다
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("ID"))
                        //{
                        //    sMsg = "SFU3052";  // CELL ID 가 이미 존재 합니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("PD"))
                        //{
                        //    sMsg = "SFU3053";   // 동일한 위치에 이미 CELL ID가 존재 합니다.
                        //}
                        //else if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["SUBLOT_CHK_CODE"]).Equals("NI"))
                        //{
                        //    sMsg = "SFU3054";  // 주액 정보가 없습니다.
                        //}
                        //else
                        //{
                        //    sMsg = "";
                        //} 

                    }

                    if (sRet.Equals("NG"))
                    {
                        //Util.Alert(sMsg);
                        Util.MessageValidation(sMsg);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool FL_Judge_Check()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inEqpTable = indataSet.Tables.Add("IN_EQP");
                inEqpTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inEqpTable.NewRow();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                inEqpTable.Rows.Add(newRow);

                if (_ELUseYN)
                {
                    DataTable inElTable = indataSet.Tables.Add("IN_EL");
                    inElTable.Columns.Add("SUBLOTID", typeof(string));
                    inElTable.Columns.Add("EL_PRE_WEIGHT", typeof(string));
                    inElTable.Columns.Add("EL_AFTER_WEIGHT", typeof(string));
                    inElTable.Columns.Add("EL_WEIGHT", typeof(string));
                    inElTable.Columns.Add("EL_PSTN", typeof(string));
                    inElTable.Columns.Add("EL_JUDG_VALUE", typeof(string));

                    newRow = inElTable.NewRow();
                    newRow["SUBLOTID"] = txtCellId.Text.Trim();
                    newRow["EL_PRE_WEIGHT"] = txtBeforeWeight.Text.Trim();
                    newRow["EL_AFTER_WEIGHT"] = txtAfterWeight.Text.Trim();
                    newRow["EL_WEIGHT"] = txtEl.Text.Trim();
                    newRow["EL_PSTN"] = txtPosition.Text.Trim();
                    newRow["EL_JUDG_VALUE"] = txtJudge.Text.Trim();
                    inElTable.Rows.Add(newRow);
                }

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_CELL_CL_EL_S_UI", "IN_EQP,IN_EL", "OUTDATA", indataSet);

                if (dsRslt != null && dsRslt.Tables.Count > 0 && dsRslt.Tables["OUTDATA"] != null && dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    string sRet = string.Empty;
                    string sMsg = string.Empty;

                    if (Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["JUDGE"]).Equals("OK"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";

                        if (dsRslt.Tables["OUTDATA"].Columns.Contains("NG_MSG"))
                            sMsg = Util.NVC(dsRslt.Tables["OUTDATA"].Rows[0]["NG_MSG"]).Trim();
                        else
                            sMsg = "";
                    }

                    txtJudge.Text = sRet;

                    if (sRet.Equals("NG"))
                    {
                        //Util.Alert(sMsg);
                        Util.MessageValidation(sMsg);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool ChkTrayStatWait()
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
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["TRAYID"] = _TrayID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private bool DeleteCell(string sDelCell = "", string sDelCells = "")
        {
            try
            {
                bool isMultdelCell = false;
                string[] sDelCellTargets = null;

                if (!string.IsNullOrEmpty(sDelCells))
                {
                    sDelCellTargets = sDelCells.Split(',');
                    isMultdelCell = true;
                }

                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_SUBLOT_CL();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = _OutLotID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inSublotTable = indataSet.Tables["IN_CST"];

                if (isMultdelCell)
                {
                    for (int i = 0; i < sDelCellTargets.Length; i++)
                    {
                        newRow = null;
                        newRow = inSublotTable.NewRow();
                        newRow["CSTID"] = _TrayID;
                        newRow["SUBLOTID"] = sDelCell.Equals("") ? sDelCellTargets[i].Trim() : sDelCell;

                        inSublotTable.Rows.Clear();
                        inSublotTable.Rows.Add(newRow);
                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_SUBLOT_CL", "INDATA,IN_CST", null, indataSet);
                    }
                }
                else
                {
                    newRow = inSublotTable.NewRow();
                    newRow["CSTID"] = _TrayID;
                    newRow["SUBLOTID"] = sDelCell.Equals("") ? txtCellId.Text.Trim() : sDelCell;

                    inSublotTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_SUBLOT_CL", "INDATA,IN_CST", null, indataSet);
                }

                HiddenLoadingIndicator();
                return true;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region [Validation]
        private bool CanCellJudge()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            double dTmpInput;
            // 주액량 정보입력 여부 체크.
            if (txtEl.Text.Trim().Equals("") || (double.TryParse(txtEl.Text, out dTmpInput) && dTmpInput <= 0))
            {
                //Util.MessageValidation("SFU4451"); // 주액량 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }

            if (txtBeforeWeight.Text.Trim().Equals("") || (double.TryParse(txtBeforeWeight.Text, out dTmpInput) && dTmpInput <= 0))
            {
                //Util.MessageValidation("SFU4452"); // 주액전 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }

            if (txtAfterWeight.Text.Trim().Equals("") || (double.TryParse(txtAfterWeight.Text, out dTmpInput) && dTmpInput <= 0))
            {
                //Util.MessageValidation("SFU4453"); // 주액후 값이 잘못 되었습니다. 다시 입력하세요.
                return bRet;
            }

            ////Header 길이 체크.
            if (txtPosition.Text.Trim().Equals("") || txtPosition.Text.Trim().Length > 1)
            {
                //Util.MessageValidation("SFU4450"); // 해더 정보는 1자리로 입력하세요.
                //return bRet;
                txtPosition.Text = "0";
            }

            bRet = true;
            return bRet;
        }

        private bool CanCellScan()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            //if (txtCellId.Text.Trim().Length != 10)
            //{
            //    //Util.Alert("CELL ID 길이가 잘못 되었습니다.");
            //    Util.MessageValidation("SFU1318");
            //    return bRet;
            //}

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // 주액 저장시 신규 Cell ID 인지 수정 Cell ID 인지 구분 불가로 해당 validation 저장 Biz에서 처리 하도록 변경.
            //// Cell ID 특수문자 등 체크.            
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetSubLotValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    //Util.Alert(sMsg);
            //    Util.MessageValidation(sMsg);
            //    return bRet;
            //}
            //else if (sRet.Equals("EXCEPTION"))
            //    return bRet;

            //// 해당 Location 존재 여부 체크.            
            //DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            //if (dtTmp == null || dtTmp.Rows.Count < 1 || !dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            //{
            //    //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 있습니다.\nCell을 먼저 삭제하세요.", cboTrayLocation.SelectedValue.ToString());
            //    Util.MessageValidation("SFU2032", cboTrayLocation.SelectedValue.ToString());
            //    return bRet;
            //}


            bRet = true;
            return bRet;
        }

        private bool CanCellModify()
        {
            bool bRet = false;

            if (txtCellId.Text.Trim().Equals(""))
            {
                //Util.Alert("CELL ID를 입력 하세요.");
                Util.MessageValidation("SFU1319");
                return bRet;
            }

            //if (txtCellId.Text.Trim().Length != 10)
            //{
            //    //Util.Alert("CELL ID 길이가 잘못 되었습니다.");
            //    Util.MessageValidation("SFU1318");
            //    return bRet;
            //}

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // 주액 저장시 신규 Cell ID 인지 수정 Cell ID 인지 구분 불가로 해당 validation 저장 Biz에서 처리 하도록 변경.
            //// Cell ID 특수문자 등 체크.            
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetSubLotValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    //Util.Alert(sMsg);
            //    Util.MessageValidation(sMsg);
            //    return bRet;
            //}
            //else if (sRet.Equals("EXCEPTION"))
            //    return bRet;

            //// 해당 Location 존재 여부 체크.            
            //DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            //if (dtTmp == null || dtTmp.Rows.Count < 1 || !dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            //{
            //    //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 있습니다.\nCell을 먼저 삭제하세요.", cboTrayLocation.SelectedValue.ToString());
            //    Util.MessageValidation("SFU2032", cboTrayLocation.SelectedValue.ToString());
            //    return bRet;
            //}

            if (txtJudge.Text.Trim().Equals("NG") || txtJudge.Text.Trim().Equals("0") || string.IsNullOrEmpty(txtJudge.Text) || string.IsNullOrWhiteSpace(txtJudge.Text))
            {
                txtJudge.Text = "";
                return bRet;
            }

            // 주액 정보 Min ~ Max 값 확인
            if (_ELUseYN)
            {
                double dTmpLSL, dTmpUSL, dTmpInput;

                if (double.TryParse(_EL_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_WEIGHT_USL, out dTmpUSL))
                {
                    if (double.TryParse(txtEl.Text, out dTmpInput))
                    {
                        if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                        {
                            Util.MessageValidation("SFU4280"); // 입력한 주액 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                            return bRet;
                        }
                    }
                }

                if (double.TryParse(_EL_BEFORE_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_BEFORE_WEIGHT_USL, out dTmpUSL))
                {
                    if (double.TryParse(txtBeforeWeight.Text, out dTmpInput))
                    {
                        if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                        {
                            Util.MessageValidation("SFU4281"); // 입력한 주액전 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                            return bRet;
                        }
                    }
                }

                if (double.TryParse(_EL_AFTER_WEIGHT_LSL, out dTmpLSL) && double.TryParse(_EL_AFTER_WEIGHT_USL, out dTmpUSL))
                {
                    if (double.TryParse(txtAfterWeight.Text, out dTmpInput))
                    {
                        if (dTmpLSL > dTmpInput || dTmpUSL < dTmpInput)
                        {
                            Util.MessageValidation("SFU4282"); // 입력한 주액후 값이 상/하한 범위를 벗어 났습니다. 다시 입력 하세요.
                            return bRet;
                        }
                    }
                }


                // 주액량 정보입력 여부 체크.
                if (txtEl.Text.Trim().Equals("") || (double.TryParse(txtEl.Text, out dTmpInput) && dTmpInput <= 0))
                {
                    Util.MessageValidation("SFU4451"); // 주액량 값이 잘못 되었습니다. 다시 입력하세요.
                    return bRet;
                }

                if (txtBeforeWeight.Text.Trim().Equals("") || (double.TryParse(txtBeforeWeight.Text, out dTmpInput) && dTmpInput <= 0))
                {
                    Util.MessageValidation("SFU4452"); // 주액전 값이 잘못 되었습니다. 다시 입력하세요.
                    return bRet;
                }

                if (txtAfterWeight.Text.Trim().Equals("") || (double.TryParse(txtAfterWeight.Text, out dTmpInput) && dTmpInput <= 0))
                {
                    Util.MessageValidation("SFU4453"); // 주액후 값이 잘못 되었습니다. 다시 입력하세요.
                    return bRet;
                }


                //Header 길이 체크.
                if (txtPosition.Text.Trim().Equals("") || txtPosition.Text.Trim().Length > 1)
                {
                    Util.MessageValidation("SFU4450"); // 해더 정보는 1자리로 입력하세요.
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanDelete()
        {
            bool bRet = false;

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            // CELL 확정 여부 체크.

            // 해당 Location 존재 여부 체크.            
            DataTable dtTmp = GetCellLocCount(cboTrayLocation.SelectedValue.ToString());

            if (dtTmp == null || dtTmp.Rows.Count < 1 || dtTmp.Rows[0]["CELLCNT"].ToString().Equals("0"))
            {
                //Util.Alert("현재위치(Cell Location : {0}) 의 Cell 정보가 없습니다.", cboTrayLocation.SelectedValue.ToString());
                Util.MessageValidation("SFU2031", cboTrayLocation.SelectedValue.ToString());
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanDeleteOutRange()
        {
            bool bRet = false;

            // Tray 확정 여부 체크.
            if (!ChkTrayStatWait())
            {
                //Util.Alert("Tray 상태가 미확정 상태가 아닙니다.");
                Util.MessageValidation("SFU1431");
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #endregion
        #endregion

        private void SetControlButton()
        {
            try
            {
                btnCancelTerm.Visibility = Visibility.Collapsed; //투입LOT종료취소

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("USERID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH", "INDATA", "OUTDATA", dt);

                if (dtResult?.Rows?.Count > 0)
                {
                    DataRow[] searchedRow = dtResult.Select("(AUTHID = 'MESADMIN' AND USE_FLAG = 'Y') OR (AUTHID = 'PROD_RSLT_MGMT_NJ' AND USE_FLAG = 'Y')");
                    if (searchedRow.Length > 0)
                    {
                        btnCancelTerm.Visibility = Visibility.Visible; //투입LOT종료취소
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void CheckDeleteCell()
        {
            try
            {
                string cellId = txtCellId.Text.Trim();
                ASSY003_007_CELL_DEL wndConfirm = new ASSY003_007_CELL_DEL();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = cellId;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Cell_Closed);
                    //grdMain.Children.Add(wndConfirm);
                    //wndConfirm.ShowModal();
                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
                /*
                if (popColumnVisible != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = this;
                    Parameters[1] = originalConfigInfos;
                    Parameters[2] = isRowCountView;

                    ControlsLibrary.C1WindowExtension.SetParameters(popColumnVisible, Parameters);

                    popColumnVisible.Closed += new EventHandler(popColumnVisible_Closed);
                    popColumnVisible.ShowModal();
                }
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void wndConfirm_Cell_Closed(object sender, EventArgs e)
        {
            ASSY003_007_CELL_DEL window = sender as ASSY003_007_CELL_DEL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (!CanDelete())
                    return;

                if (DeleteCell())
                {
                    SearchTrayWindow();

                    GetTrayInfo();
                }
            }
            grdMain.Children.Remove(window);
        }

        private void btnEdc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU9933", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //EDC 데이터 요청
                        callEdcData();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void callEdcData()
        {
            try
            {
                DataTable inTable = _Biz.GetDA_PRD_SEL_TRAY_CELL_LIST();
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["OUT_LOTID"]  = _OutLotID;
                newRow["EQPTID"]     = Util.NVC(cboEquipment.SelectedValue);
                newRow["TRAYID"]     = _TrayID;
                newRow["AREAID"]     = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);
                new ClientProxy().ExecuteServiceSync("BR_PRD_GET_EDC_ZZS_PKG_DATA", "INDATA", null, inTable);
                // 요청되었습니다.
                Util.MessageInfo("SFU1747");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    
    }
}
