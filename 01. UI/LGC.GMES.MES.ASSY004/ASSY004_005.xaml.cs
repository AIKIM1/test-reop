/*************************************************************************************
 Created Date : 2019.04.15
      Creator : INS 김동일K
   Decription : CWA3동 증설 - Folding 공정진척 화면 (ASSY0001.ASSY001_005 2019/04/15 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.15  INS 김동일K : Initial Created.
  2019.09.25  LG CNS 김대근 : 금형관리 탭 추가
  2020.01.20  INS 김동일 : 실적확정 취소 기능 추가 (CNB 요구에 의해 해당 기능 제거)
  2020.05.12  김동일 : [C20200511-000024] 작업조, 작업자 등록 변경
  2020.08.21  신광희 : [C20200814-000102] 시생산 재공 표기 추가
  2021.05.06  김동일 : [C20210427-000125] 실적 확정 시 PKG 공급 Carrier Tab에 정보가 존재하지 않는 경우에 Inform 처리
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_005.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_005 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private bool bSetAutoSelTime = false;
        private bool bTestMode = false;
        private string _LABEL_PRT_RESTRCT_FLAG = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        // 전체 적용 여부 CCB 결과 없음. 지급 요청 건으로 하드 코딩.
        private List<string> _SELECT_USER_MODE_AREA = new List<string>(new string[] { "A7" });   // 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();
        private UC_IN_OUTPUT winInput = null;

        #endregion

        #region Initialize
        public ASSY004_005()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.STACKING_FOLDING, EquipmentGroup.FOLDING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_BY_EQGRID");

            String[] sFilter2 = { Process.STACKING_FOLDING, EquipmentGroup.FOLDING };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2, sCase: "EQUIPMENT_BY_EQSGID_NEW");

            // 자동 조회 시간 Combo
            String[] sFilter3 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboAutoSearchOut != null && cboAutoSearchOut.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;
        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                grdOutTranBtn.Visibility = Visibility.Collapsed;
                grdOutPkgSupplyTranBtn.Visibility = Visibility.Collapsed;
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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            SetWorkOrderWindow();

            SetInputWindow();

            // 생산 반제품 조회 Timer Start.
            if (dispatcherTimer != null)
                dispatcherTimer.Start();

            #region [버튼 권한 적용에 따른 처리]
            GetButtonPermissionGroup();
            #endregion
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();

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

            this.RegisterName("redBrush", redBrush);

            HideTestMode();

            //SetUserCheckFlag();
            txtUserNameCr.Text = string.Empty;
            txtUserNameCr.Tag = string.Empty;
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
            CheckUnldrShrFlag();

            SetInOutCtrlByLotIdentBasCode();    // Lot 식별 기준 코드에 따른 컨트롤 변경.

            GetWorkOrder();
            GetProductLot();

            GetWrokCalander();
            SetUserCheckFlag();
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;
            
            ASSY004_COM_RUNSTART wndRunStart = new ASSY004_COM_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.STACKING_FOLDING;
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
                Parameters[0] = Process.STACKING_FOLDING;
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
            if (Util.EQPTCondition(Process.STACKING_FOLDING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnEqptCond_Click(null, null);
                return;
            }
            // 품질정보 등록 여부
            if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, Process.STACKING_FOLDING, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnQualityInput_Click(null, null);
                return;
            }
            #endregion

            // PKG 공급 Carrier 확인
            if (tbPkgSupplyCarrier.Visibility == Visibility.Visible && dgOutPkgSupply.GetRowCount() < 1)
            {
                // PKG 공급 Caiirer 정보가 없습니다. 실적 확인을 하시겠습니까?
                Util.MessageConfirm("SFU3793", (result2) =>
                {
                    if (result2 == MessageBoxResult.OK)
                    {
                        // 설비 Loss 등록 여부 체크
                        DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.STACKING_FOLDING);

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
                });
            }
            else
            {
                // 설비 Loss 등록 여부 체크
                DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.STACKING_FOLDING);

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
        }
        
        //2019.04.29 김대근 / 현재 공정을 wndWait로 전달
        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY004_COM_WAITLOT wndWaitLot = new ASSY004_COM_WAITLOT();
            wndWaitLot.FrameOperation = FrameOperation;

            if (wndWaitLot != null)
            {
                //다른 Window와 Parameters를 맞추기 위하여 1->2로 증가시켜줌
                object[] Parameters = new object[5];
                Parameters[0] = Util.GetCondition(cboEquipmentSegment);
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;
                //stacking과 folding이 하나의 ProcID로 통합되어 있음.
                //그러나 현재 분리를 시켜야 하기 때문에 구분하여 값을 따로 준다.
                Parameters[4] = EquipmentGroup.FOLDING;
                C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                wndWaitLot.Closed += new EventHandler(wndWaitLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
            }
        }
        
        private void btnEqpRemark_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.Equals("") || cboEquipment.SelectedValue.Equals("SELECT"))
            {
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
                Parameters[2] = Process.STACKING_FOLDING;
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

        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //if (winWorkOrder == null)
            //    return;

            //winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            //winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            //winWorkOrder.PROCID = Process.STACKING_FOLDING;

            //winWorkOrder.ClearWorkOrderInfo();


            //if (winInput == null)
            //    return;

            //winInput.ChangeEquipment(cboEquipmentSegment.SelectedValue.ToString(), cboEquipment.SelectedValue.ToString());

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
                    winWorkOrder.PROCID = Process.STACKING_FOLDING;

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

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                        SetUserCheckFlag();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutCa_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    System.Threading.Thread.Sleep(300);

                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        // 접근권한이 없습니다.
                        Util.MessageValidation("10042", (action) =>
                        {
                            txtOutCa.Text = "";
                            txtOutCa.Focus();
                        });

                        return;
                    }

                    CreateOutProdWithOutMsg();
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
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = cboEquipment.Text;

                C1WindowExtension.SetParameters(wndEqptCond, Parameters);

                wndEqptCond.Closed += new EventHandler(wndEqptCond_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
            }
        }
        
        private void dgOutProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(e.Cell.Column.Name).Equals("LOTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgOutProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                Parameters[1] = Process.STACKING_FOLDING;
                Parameters[2] = cboEquipment.SelectedValue;
                Parameters[3] = cboEquipmentSegment.Text.ToString();
                Parameters[4] = cboEquipment.Text.ToString();

                C1WindowExtension.SetParameters(wndEqptCondSearch, Parameters);

                wndEqptCondSearch.Closed += new EventHandler(wndEqptCondSearch_Closed);
                
                this.Dispatcher.BeginInvoke(new Action(() => wndEqptCondSearch.ShowModal()));
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
                Parameters[0] = Process.STACKING_FOLDING;
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Timer Stop.
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
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
                Parameters[2] = Process.STACKING_FOLDING;
                Parameters[3] = cboEquipmentSegment.Text;
                Parameters[4] = _LDR_LOT_IDENT_BAS_CODE;

                C1WindowExtension.SetParameters(wndMerge, Parameters);

                wndMerge.Closed += new EventHandler(wndMerge_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndMerge.ShowModal()));
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
                Parameters[2] = Process.STACKING_FOLDING;

                C1WindowExtension.SetParameters(wndWipNote, Parameters);

                wndWipNote.Closed += new EventHandler(wndWipNote_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWipNote.ShowModal()));
            }
        }

        private void btnChgCarrier_Click(object sender, RoutedEventArgs e)
        {
            CMM_COM_CHG_CARRIER wndChgCst = new CMM_COM_CHG_CARRIER();
            wndChgCst.FrameOperation = FrameOperation;

            if (wndChgCst != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = Process.STACKING_FOLDING;
                Parameters[1] = "N";

                C1WindowExtension.SetParameters(wndChgCst, Parameters);

                wndChgCst.Closed += new EventHandler(wndChgCst_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndChgCst.ShowModal()));
            }
        }
        #endregion

        #region [작업대상]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
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

        private void btnCreateBOX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCreateBox())
                    return;

                if (CheckFirstOutProduction())
                {
                    //"생산 수량 %1 개로 생성 하시겠습니까?"
                    Util.MessageConfirm("SFU4888", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CreateOutBox();
                        }
                    }, txtOutBoxQty.Text);
                }
                else
                {
                    //"생성 하시겠습니까?"
                    Util.MessageConfirm("SFU1621", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CreateOutBox();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnDeleteBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDeleteBox())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        DeleteOutBox();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();
                }
            });
        }

        private void txtOutBoxQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutBoxQty.Text, 0))
                {
                    txtOutBoxQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (txtOutCa.Visibility == Visibility.Visible)
                        txtOutCa.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutBox())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            //저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        SaveOutBox();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();
                }
            });
        }
        
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtOutCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_ASSY_CELL_INFO wndCell = new CMM_ASSY_CELL_INFO();
                            wndCell.FrameOperation = FrameOperation;

                            if (wndCell != null)
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name));
                                Parameters[1] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "CSTID"));

                                C1WindowExtension.SetParameters(wndCell, Parameters);

                                wndCell.Closed += new EventHandler(wndCell_Closed);

                                this.Dispatcher.BeginInvoke(new Action(() => wndCell.ShowModal()));
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
                        dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
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
                        Util.MessageValidation("SFU1605", cboAutoSearchOut.SelectedValue.ToString());
                    }

                    bSetAutoSelTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                    //FrameOperation.PrintFrameMessage(DateTime.Now.ToLongTimeString() + ">>" + dpcTmr.Interval.TotalSeconds.ToString());

                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    if (dgProductLot == null || dgProductLot.GetRowCount() < 1)
                        return;

                    GetOutProduct();

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
            //        grdDfctLegend.Visibility = Visibility.Collapsed;
            //    }
            //    else if (string.Equals(tabItem, "tbDefect"))
            //    {
            //        grdDfctLegend.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        grdDfctLegend.Visibility = Visibility.Collapsed;
            //    }
            //}
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


        private void txtOutPkgSupplyBoxQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutPkgSupplyBoxQty.Text, 0))
                {
                    txtOutPkgSupplyBoxQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (txtOutPkgSupplyCa.Visibility == Visibility.Visible)
                        txtOutPkgSupplyCa.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutPkgSupplyCa_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        // 접근권한이 없습니다.
                        Util.MessageValidation("10042", (action) =>
                        {
                            txtOutPkgSupplyCa.Text = "";
                            txtOutPkgSupplyCa.Focus();
                        });

                        return;
                    }

                    CreatePkgSupplyBoxWithOutMsg();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutPkgSupplyCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutPkgSupplyCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutPkgSupplyCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCreatePkgSupplyBOX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCreatePkgSupplyBox())
                    return;

                //"생성 하시겠습니까?"
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreatePkgSupplyBox();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDeletePkgSupplyBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDeletePkgSupplyBox())
                return;

            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeletePkgSupplyBox();
                }
            });
        }

        private void btnOutPkgSupplySave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutPkgSupplyBox())
                return;

            //저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SavePkgSupplyBox();
                }
            });
        }

        private void dgOutPkgSupply_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }

            //    if (e.Cell.Row.Type == DataGridRowType.Item)
            //    {
            //        if (Util.NVC(e.Cell.Column.Name).Equals("LOTID"))
            //        {
            //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
            //        }
            //        else
            //        {
            //            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

        private void dgOutPkgSupply_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter != null)
            //    {
            //        if (e.Cell.Row.Type == DataGridRowType.Item)
            //        {
            //            e.Cell.Presenter.Background = null;
            //            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
            //        }
            //    }
            //}));
        }

        private void dgOutPkgSupply_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        #region [작업대상 변경]

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

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIPINFO_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                //newRow["WOID"] = Util.NVC(drWorkOrderInfo["WOID"]);
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                //newRow["WIPSTAT"] = "WAIT,PROC,END";
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_FD_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                                GetWaitMagazineList();
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
                                GetWaitMagazineList();
                            }
                        }

                        // 2017.07.20  Lee. D. R : 공정진척 UI 에 당일 등록 필요 설비 Loss Cnt 표시
                        GetLossCnt();
                        //2019.11.01 김대근 : Tool투입이력 조회
                        GetInputHistTool();

                        if (txtOutCa.Visibility == Visibility.Visible)
                            txtOutCa.Focus();
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

        private void GetOutProduct()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_FD_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutProduct, searchResult, FrameOperation, false);

                if (dgOutProduct.GetRowCount() > 0)
                {
                    double dQty = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[0].DataItem, "WIPQTY")));
                    txtOutBoxQty.Text = Convert.ToString(dQty);
                }
                else
                {
                    txtOutBoxQty.Text = "";
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

        private void GetPkgSupplyProduct()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_UNLOADER_FD_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutPkgSupply, searchResult, FrameOperation, false);

                if (dgOutPkgSupply.GetRowCount() > 0)
                {
                    double dQty = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[0].DataItem, "WIPQTY")));
                    txtOutPkgSupplyBoxQty.Text = Convert.ToString(dQty);
                }
                else
                {
                    txtOutPkgSupplyBoxQty.Text = "";
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

        /// <summary>
        /// 2017.06.28 Add by Kim Joonphil
        /// CST 사용 가능 확인 (폴란드에서 사용)
        /// </summary>
        /// <returns></returns>
        private bool CheckedUseCassette()
        {
            try
            {
                DataSet IndataSet = new DataSet();

                DataTable dtIN_EQP = IndataSet.Tables.Add("IN_EQP");
                dtIN_EQP.Columns.Add("SRCTYPE", typeof(string));
                dtIN_EQP.Columns.Add("IFMODE", typeof(string));
                dtIN_EQP.Columns.Add("CSTID", typeof(string));
                dtIN_EQP.Columns.Add("WIP_TYPE_CODE", typeof(string));

                DataRow dr = dtIN_EQP.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["CSTID"] = Util.NVC(txtOutCa.Text.Trim());
                dr["WIP_TYPE_CODE"] = "OUT";
                dtIN_EQP.Rows.Add(dr);


                DataTable dtIN_INPUT = IndataSet.Tables.Add("IN_INPUT");
                dtIN_INPUT.Columns.Add("LANGID", typeof(string));
                dtIN_INPUT.Columns.Add("PROCID", typeof(string));
                dtIN_INPUT.Columns.Add("EQSGID", typeof(string));

                dr = dtIN_INPUT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.STACKING_FOLDING;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                dtIN_INPUT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_CST_MAPPING_DUP", "IN_EQP,IN_INPUT", null, IndataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }

        private bool CheckUnldrShrFlag()
        {
            try
            {
                bool bRet = false;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = IndataTable.NewRow();
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                IndataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", IndataTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["UNLDR_SHR_FLAG"]).Equals("Y"))
                    {
                        tbPkgSupplyCarrier.Visibility = Visibility.Visible;
                        bRet = true;
                    }
                    else
                        tbPkgSupplyCarrier.Visibility = Visibility.Collapsed;
                }
                else
                    tbPkgSupplyCarrier.Visibility = Visibility.Collapsed;

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void CreateOutBox()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("IFMODE", typeof(String));
                RQSTDT.Columns.Add("EQPTID", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));
                RQSTDT.Columns.Add("PROD_LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                dr["USERID"] = _LABEL_PRT_RESTRCT_FLAG == "Y" ? txtUserNameCr.Tag.ToString() : LoginInfo.USERID;
                dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_CHK_LOT_LABEL_PRT_RESTRCT", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException, (msgResult) =>
                            {
                                if (txtOutCa.Visibility == Visibility.Visible)
                                {
                                    txtOutCa.Text = "";
                                    txtOutCa.Focus();
                                }
                            });

                            return;
                        }
                        
                        ShowLoadingIndicator();

                        DataSet indataSet = new DataSet();

                        DataTable inTable = indataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("LANGID", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("PROD_LOTID", typeof(string));
                        //inTable.Columns.Add("OUT_LOTID", typeof(string));
                        inTable.Columns.Add("CSTID", typeof(string));
                        inTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inTable.Columns.Add("USERID", typeof(string));

                        inTable = indataSet.Tables.Add("IN_INPUT");
                        inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inTable.Columns.Add("INPUT_LOTID", typeof(string));

                        inTable = indataSet.Tables["INDATA"];
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                        //newRow["OUT_LOTID"] = sNewOutLot;
                        newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        newRow["CSTID"] = Util.NVC(txtOutCa.Text.Trim());
                        newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutBoxQty.Text);                        
                        newRow["USERID"] = _LABEL_PRT_RESTRCT_FLAG == "Y" ? txtUserNameCr.Tag.ToString() : LoginInfo.USERID;

                        inTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_FD_L", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException, (msgResult) =>
                                    {
                                        if (txtOutCa.Visibility == Visibility.Visible)
                                        {
                                            txtOutCa.Text = "";
                                            txtOutCa.Focus();
                                        }
                                    });

                                    return;
                                }

                                GetWorkOrder(); // 작지 생산수량 정보 재조회.
                                GetProductLot();

                                //int idx = _Util.GetDataGridRowIndex(dgOutProduct, "LOTID", sNewOutLot);
                                //if (idx >= 0)
                                //    DataTableConverter.SetValue(dgOutProduct.Rows[idx].DataItem, "CHK", true);

                                Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.
                                txtOutCa.Text = "";

                                if (txtOutCa.Visibility == Visibility.Visible)
                                    txtOutCa.Focus();
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
                        Util.MessageException(ex);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DeleteOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_BOX_ST();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = input_LOT.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));

                    input_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_FD_L", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
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

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
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
        
        private void SaveOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable out_LOT = indataSet.Tables.Add("IN_OUTLOT");
                out_LOT.Columns.Add("OUT_LOTID", typeof(string));
                out_LOT.Columns.Add("OUT_LOT_WIPSEQ", typeof(decimal));
                out_LOT.Columns.Add("OUTPUT_QTY", typeof(decimal));
                out_LOT.Columns.Add("CSTID", typeof(string));
                out_LOT.Columns.Add("BONUSQTY", typeof(decimal));

                DataTable inTable = indataSet.Tables["IN_DATA"];
                DataRow newRow = inTable.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_OUTLOT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = outLotTable.NewRow();

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                    newRow["OUT_LOT_WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ")));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "CSTID"));
                    newRow["BONUSQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY")));

                    outLotTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_QTY_L", "IN_DATA,IN_OUTLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            GetOutProduct();
                            return;
                        }

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();
                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
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

        private int GetNotDispatchCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_NOT_DISPATCH_CNT();

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_DISPATCH_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
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
                newRow["PROCID"] = Process.STACKING_FOLDING;

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
                Indata["PROCID"] = Process.STACKING_FOLDING;

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

        public void OnCurrInDelete(DataTable inMtrl)
        {
            try
            {

                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_COMPLETE_LM();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
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

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_INPUT_IN_LOT_FD", "INDATA,IN_INPUT", null, indataSet);

                inInputTable.Clear();

                GetProductLot();

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void OnCurrInCancel(DataTable inMtrl)
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (iRow < 0)
                    return;

                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_FD();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")); // biz에서 찾음.

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

                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT", "INDATA,IN_INPUT", null, indataSet);

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

                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_COMPLETE_LM();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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

        public bool OnCurrAutoInputLot(string sInputLot, string sPstnID)
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
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_ID", typeof(string));

                inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                inTable = indataSet.Tables["IN_INPUT"];
                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sPstnID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_ID"] = sInputLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_FD_L", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

                HideLoadingIndicator();

                GetProductLot();
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

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
                dtRow["PROCID"] = Process.STACKING_FOLDING;
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
        
        private bool CheckFirstOutProduction()
        {
            try
            {
                bool bRet = false;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_CNT_BY_PR_LOTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("OUT_PROD_CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["OUT_PROD_CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt < 1)
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
        
        private int GetOutTermCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_TERM_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["TERM_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["TERM_CNT"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
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
                                    break;
                                case "LOTSTART_W": // 작업시작 사용 권한
                                    btnRunStart.Visibility = Visibility.Visible;
                                    break;
                                case "OUTPUT_PKG_SUPPLY_W":    // 패키지 공급캐리어 사용 권한
                                    grdOutPkgSupplyTranBtn.Visibility = Visibility.Visible;
                                    break;
                                //case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                //    btnCancelConfirm.Visibility = Visibility.Visible;
                                //    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                // 재작업 Lot 이면 생산 반제품 기능 제공.
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx >= 0)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "IRREGL_PROD_LOT_TYPE_CODE")).Equals("R"))
                    {
                        grdOutTranBtn.Visibility = Visibility.Visible;
                        grdOutPkgSupplyTranBtn.Visibility = Visibility.Visible;
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
                newRow["PROCID"] = Process.STACKING_FOLDING;
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

        private void CreatePkgSupplyBox()
        {
            try
            {
                ShowLoadingIndicator();
                
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                //inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                //newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["CSTID"] = Util.NVC(txtOutPkgSupplyCa.Text.Trim());
                newRow["OUTPUT_QTY"] = Convert.ToDecimal(txtOutPkgSupplyBoxQty.Text); 
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_OUT_LOT_UNLOADER_FD_L", "IN_EQP", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException, (msgResult) =>
                            {
                                if (txtOutPkgSupplyCa.Visibility == Visibility.Visible)
                                {
                                    txtOutPkgSupplyCa.Text = "";
                                    txtOutPkgSupplyCa.Focus();
                                }
                            });

                            return;
                        }

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();

                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.
                        txtOutPkgSupplyCa.Text = "";

                        if (txtOutPkgSupplyCa.Visibility == Visibility.Visible)
                            txtOutPkgSupplyCa.Focus();
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

        private void DeletePkgSupplyBox()
        {
            try
            {
                ShowLoadingIndicator();
                
                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));


                inDataTable = indataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                inDataTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;

                    newRow = inDataTable.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID"));

                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_UNLOADER_FD_L", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
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

        private void SavePkgSupplyBox()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                inDataTable = indataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                inDataTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;

                    newRow = inDataTable.NewRow();

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID"));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPQTY")));

                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_OUT_LOT_UNLOADER_FD_L", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            GetPkgSupplyProduct();
                            return;
                        }

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();
                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
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

        private string GetWipSeq(string sLotID)
        {
            try
            {
                string sRet = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LOTID"] = sLotID;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sRet = Util.NVC(dtRslt.Rows[0]["WIPSEQ"]);
                }

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

            bRet = true;
            return bRet;
        }

        private bool CanStartRun()
        {
            bool bRet = false;

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            for (int i = 0; i < dgProductLot.Rows.Count - dgProductLot.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    //Util.Alert("작업중인 LOT이 존재 합니다.");
                    Util.MessageValidation("SFU1847");
                    return bRet;
                }
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
        
        private bool CanCreateBox()
        {
            bool bRet = false;
            string returnMsg = string.Empty;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (txtOutBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutBoxQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutBoxQty.SelectAll();
                return bRet;
            }

            if ((Convert.ToDecimal(txtOutBoxQty.Text) % 1) > 0)
            {
                //Util.Alert("소수점 입력은 불가능 합니다. 수량을 확인해 주세요.");
                Util.MessageValidation("SFU2342");
                txtOutBoxQty.SelectAll();
                return bRet;
            }

            if (!CheckedUseCassette())  //Cassette 중복여부 체크. 2017.06.28 Add by Kim Joonphil
            {
                txtOutCa.SelectAll();
                return bRet;
            }

            if (_LABEL_PRT_RESTRCT_FLAG == "Y")
            {
                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserNameCr.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    txtUserNameCr.Focus();
                    return bRet;
                }
            }

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (!CheckUnldrShrFlag())
                {
                    //카세트ID 10자리 확인 [2018.03.14]
                    if (txtOutCa.Text.Length != 10)
                    {
                        Util.MessageValidation("SFU4571");
                        txtOutCa.Text = "";
                        txtOutCa.Focus();
                        return bRet;
                    }
                }
            }

            bRet = true;
            return bRet;
        }
        
        private bool CanDeleteBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")),
                                    Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private bool CanSaveOutBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return bRet;
                }

                // 양품수량(전체수량 - 재작업수량) 이 음수 여부 체크
                double dBonusQty = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDouble(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "BONUS_QTY"));
                if ((dTmp - dBonusQty) < 0)
                {
                    Util.MessageValidation("SFU1721");  // 양품량은 음수가 될 수 없습니다.값을 맞게 변경하세요.
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

        private bool CanCreatePkgSupplyBox()
        {
            bool bRet = false;
            string returnMsg = string.Empty;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (txtOutPkgSupplyBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutPkgSupplyBoxQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutPkgSupplyBoxQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutPkgSupplyBoxQty.SelectAll();
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanDeletePkgSupplyBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOutPkgSupply, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;
                
                if (Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPSEQ")) != GetWipSeq(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID")))) //Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIP_WIPSEQ")))
                {
                    Util.MessageValidation("SFU5145", Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID")), ObjectDic.Instance.GetObjectName("삭제")); // 해당 Lot[%1]은 %2 처리할 수 없습니다.
                    return bRet;
                }
            }            

            bRet = true;

            return bRet;
        }

        private bool CanSaveOutPkgSupplyBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutPkgSupply, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return bRet;
                }

                // 양품수량(전체수량 - 재작업수량) 이 음수 여부 체크
                double dBonusQty = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDouble(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "BONUS_QTY"));
                if ((dTmp - dBonusQty) < 0)
                {
                    Util.MessageValidation("SFU1721");  // 양품량은 음수가 될 수 없습니다.값을 맞게 변경하세요.
                    return bRet;
                }


                if (Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPSEQ")) != GetWipSeq(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID"))))  //Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIP_WIPSEQ")))
                {
                    Util.MessageValidation("SFU5145", Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID")), ObjectDic.Instance.GetObjectName("저장")); // 해당 Lot[%1]은 %2 처리할 수 없습니다.
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
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
                GetProductLot();
            }           
        }

        private void wndWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY004_COM_WAITLOT window = sender as ASSY004_COM_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
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

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;
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

        private void wndCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CELL_INFO window = sender as CMM_ASSY_CELL_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
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

        private void wndChgCst_Closed(object sender, EventArgs e)
        {
            CMM_COM_CHG_CARRIER window = sender as CMM_COM_CHG_CARRIER;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #region [Func]

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

            listAuth.Add(btnCreateBOX);
            listAuth.Add(btnDeleteBOX);
            listAuth.Add(btnOutSave);
            listAuth.Add(btnDefectSave);

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
                    winInput = new UC_IN_OUTPUT(Process.STACKING_FOLDING, sEqsg, sEqpt);

                winInput.FrameOperation = FrameOperation;

                winInput._UCParent = this;
                grdInput.Children.Add(winInput);

                // Lot 식별 기준 코드에 따른 컨트롤 변경.
                SetInOutCtrlByLotIdentBasCode();
            }
        }

        private void Confirm_Process()
        {
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
                            Parameters[0] = Process.STACKING_FOLDING;
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
                    Parameters[0] = Process.STACKING_FOLDING;
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

        private void SetUserCheckFlag()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRQSTDT.NewRow();
                dr["PROCID"] = Process.STACKING_FOLDING;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_LABEL_FLAG", "RQSTDT", "RSLDT", dtRQSTDT);

                if (dtRslt.Rows[0]["LABEL_PRT_RESTRCT_FLAG"].ToString().Trim().ToUpper().Equals("Y"))
                {
                    //grdCreater.Visibility = Visibility.Visible;
                    txtCreater.Visibility = Visibility.Visible;
                    txtUserNameCr.Visibility = Visibility.Visible;
                    btnUserCr.Visibility = Visibility.Visible;

                    _LABEL_PRT_RESTRCT_FLAG = "Y";
                }
                else
                {
                    //grdCreater.Visibility = Visibility.Collapsed;
                    txtCreater.Visibility = Visibility.Collapsed;
                    txtUserNameCr.Visibility = Visibility.Collapsed;
                    btnUserCr.Visibility = Visibility.Collapsed;

                    _LABEL_PRT_RESTRCT_FLAG = "N";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetWorkOrder()
        {
            if (winWorkOrder == null)
                return;

            winWorkOrder.EQPTSEGMENT = cboEquipmentSegment.SelectedValue.ToString();
            winWorkOrder.EQPTID = cboEquipment.SelectedValue.ToString();
            winWorkOrder.PROCID = Process.STACKING_FOLDING;

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
            winInput.PROCID = Process.STACKING_FOLDING;

            winInput.GetCurrInList();
        }

        private void GetWaitMagazineList()
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
            winInput.PROCID = Process.STACKING_FOLDING;

            winInput.GetWaitMagazine();
        }
        
        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.STACKING_FOLDING;
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
            Util.gridClear(dgOutProduct);
            Util.gridClear(dgDefect);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgOutPkgSupply); 

            txtOutBoxQty.Text = "";
            txtOutPkgSupplyBoxQty.Text = "";
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

            GetOutProduct();
            GetDefectInfo();
            GetEqptLoss();

            GetPkgSupplyProduct();
        }
        

        /// <summary>
        /// 작업지시 공통 UserControl에서 작업지시 선택/선택취소 시 메인 화면 정보 재조회 처리
        /// 설비 선택하면 자동 조회 처리 기능 추가로 작지 변경시에도 자동 조회 되도록 구현.
        /// </summary>
        public void GetAllInfoFromChild()
        {
            GetProductLot();
        }

        private void ColorAnimationInredRectangle(System.Windows.Shapes.Rectangle rec)
        {
            rec.Fill = redBrush;

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
                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
                {
                    if (dgOutProduct.Columns.Contains("CSTID"))
                        dgOutProduct.Columns["CSTID"].Visibility = Visibility.Visible;

                    if (dgOutPkgSupply.Columns.Contains("CSTID"))
                        dgOutPkgSupply.Columns["CSTID"].Visibility = Visibility.Visible;

                    lblCST.Visibility = Visibility.Visible;
                    txtOutCa.Visibility = Visibility.Visible;

                    lblOutPkgSupplyCST.Visibility = Visibility.Visible;
                    txtOutPkgSupplyCa.Visibility = Visibility.Visible;
                    //chkAutoPrint.IsChecked = true;
                    //chkAutoPrint.Visibility = Visibility.Visible;

                    SetGridData(dgOutProduct, false);
                    SetGridData(dgOutPkgSupply, false);
                }
                else if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    if (dgOutProduct.Columns.Contains("CSTID"))
                        dgOutProduct.Columns["CSTID"].Visibility = Visibility.Visible;

                    if (dgOutPkgSupply.Columns.Contains("CSTID"))
                        dgOutPkgSupply.Columns["CSTID"].Visibility = Visibility.Visible;

                    lblCST.Visibility = Visibility.Visible;
                    txtOutCa.Visibility = Visibility.Visible;

                    lblOutPkgSupplyCST.Visibility = Visibility.Visible;
                    txtOutPkgSupplyCa.Visibility = Visibility.Visible;
                    //chkAutoPrint.IsChecked = true;
                    //chkAutoPrint.Visibility = Visibility.Collapsed;

                    SetGridData(dgOutProduct, true);
                    SetGridData(dgOutPkgSupply, true);
                }
                else
                {
                    if (dgOutProduct.Columns.Contains("CSTID"))
                        dgOutProduct.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    if (dgOutPkgSupply.Columns.Contains("CSTID"))
                        dgOutPkgSupply.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    lblCST.Visibility = Visibility.Collapsed;
                    txtOutCa.Visibility = Visibility.Collapsed;

                    lblOutPkgSupplyCST.Visibility = Visibility.Collapsed;
                    txtOutPkgSupplyCa.Visibility = Visibility.Collapsed;
                    //chkAutoPrint.IsChecked = true;
                    //chkAutoPrint.Visibility = Visibility.Visible;

                    SetGridData(dgOutProduct, false);
                    SetGridData(dgOutPkgSupply, false);
                }

                // RFID 관련 숨김 기능...
                //btnOutPrint_Org.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPermissionPerInputButton(string sBtnPermissionGrpCode)
        {
            if (winInput == null)
                return;

            winInput.SetPermissionPerButton(sBtnPermissionGrpCode);
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserNameCr.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
            }
        }

        public void SetGridData(C1DataGrid dg, bool bRfid)
        {
            try
            {
                if (bRfid)
                {
                    dg.Columns["CHK"].DisplayIndex = 0;
                    dg.Columns["ROWNUM"].DisplayIndex = 1;
                    dg.Columns["LOTID"].DisplayIndex = 2;
                    dg.Columns["WIPSEQ"].DisplayIndex = 4;
                    dg.Columns["PRODID"].DisplayIndex = 5;

                    dg.Columns["WIPQTY"].DisplayIndex = 6;
                    dg.Columns["BONUS_QTY"].DisplayIndex = 7;
                    dg.Columns["UNIT_CODE"].DisplayIndex = 8;
                    dg.Columns["PRINT_YN"].DisplayIndex = 9;
                    dg.Columns["PRINT_YN_NAME"].DisplayIndex = 10;

                    dg.Columns["WIP_WRK_TYPE_CODE"].DisplayIndex = 11;
                    dg.Columns["WIP_WRK_TYPE_CODE_DESC"].DisplayIndex = 12;
                    dg.Columns["LOTDTTM_CR"].DisplayIndex = 13;
                    dg.Columns["CSTID"].DisplayIndex = 3;
                    dg.Columns["DISPATCH_YN"].DisplayIndex = 14;

                    dg.Columns["INSUSERNAME"].DisplayIndex = 15;

                    //dg.FrozenColumnCount = 5;
                }
                else
                {
                    dg.Columns["CHK"].DisplayIndex = 0;
                    dg.Columns["ROWNUM"].DisplayIndex = 1;
                    dg.Columns["LOTID"].DisplayIndex = 2;
                    dg.Columns["WIPSEQ"].DisplayIndex = 3;
                    dg.Columns["PRODID"].DisplayIndex = 4;

                    dg.Columns["WIPQTY"].DisplayIndex = 5;
                    dg.Columns["BONUS_QTY"].DisplayIndex = 6;
                    dg.Columns["UNIT_CODE"].DisplayIndex = 7;
                    dg.Columns["PRINT_YN"].DisplayIndex = 8;
                    dg.Columns["PRINT_YN_NAME"].DisplayIndex = 9;

                    dg.Columns["WIP_WRK_TYPE_CODE"].DisplayIndex = 10;
                    dg.Columns["WIP_WRK_TYPE_CODE_DESC"].DisplayIndex = 11;
                    dg.Columns["LOTDTTM_CR"].DisplayIndex = 12;
                    dg.Columns["CSTID"].DisplayIndex = 13;
                    dg.Columns["DISPATCH_YN"].DisplayIndex = 14;

                    dg.Columns["INSUSERNAME"].DisplayIndex = 15;

                    //dg.FrozenColumnCount = 5;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CreateOutProdWithOutMsg()
        {
            if (!CanCreateBox())
                return;

            if (CheckFirstOutProduction())
            {
                //"생산 수량 %1 개로 생성 하시겠습니까?"
                Util.MessageConfirm("SFU4888", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateOutBox();
                    }
                }, txtOutBoxQty.Text);
            }
            else
            {
                CreateOutBox();
            }
        }

        private void CreatePkgSupplyBoxWithOutMsg()
        {
            try
            {
                if (!CanCreatePkgSupplyBox())
                    return;

                CreatePkgSupplyBox();
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

            string sProcLotID = string.Empty;
            if (CheckProcWip(out sProcLotID))
            {
                Util.MessageValidation("SFU3199", sProcLotID); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
                return bRet;
            }

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
                    Parameters[0] = Process.STACKING_FOLDING;
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
                Parameters[3] = Process.STACKING_FOLDING;
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
                Indata["PROCID"] = Process.STACKING_FOLDING;

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
