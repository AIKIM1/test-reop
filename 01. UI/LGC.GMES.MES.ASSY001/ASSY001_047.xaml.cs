/*************************************************************************************
 Created Date : 2019.04.15
      Creator : INS 김동일K
   Decription : CWA3동 증설 - Lamination 공정진척 화면 (ASSY0001.ASSY001_004 2019/04/15 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.04.15  INS 김동일K : Initial Created.
  2019.09.25  LG CNS 김대근 : 금형관리 탭 추가
  2020.01.20  INS 김동일 : 실적확정 취소 기능 추가
  2020.05.12  김동일 : [C20200511-000024] 작업조, 작업자 등록 변경
  2020.05.12  김동일 : [C20200219-000381] [폴란드증설통합3동 PJT] 라미공정 투입 LOT 자공정 LOSS 상세 등록 관리
  2020.08.21  신광희 : [C20200814-000102] 시생산 재공 표기 추가 및 추가기능 – 시생산 모드 설정/해제 Validation 주석처리
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

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_047.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_047 : UserControl, IWorkArea
    { 
        #region Declaration & Constructor
        private bool bSetAutoSelTime = false;
        private bool bTestMode = false;
        private bool bMagzinePrintVisible = false;
        private string _EQPT_CONF_TYPE = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty;

        // 전체 적용 여부 CCB 결과 없음. 지급 요청 건으로 하드 코딩.
        private List<string> _SELECT_USER_MODE_AREA = new List<string>(new string[] { "A1", "A2", "A7", "A9" });   // 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]

        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private UC_WORKORDER_LINE winWorkOrder = new UC_WORKORDER_LINE();
        private UC_IN_OUTPUT_L winInput = null;
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
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
        public ASSY001_047()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.LAMINATION };
            C1ComboBox[] cboLineChild = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_PROC");

            String[] sFilter1 = { Process.LAMINATION, "AZS" };      //20220411 sinjigun22 "AZS" add
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };

            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter1, sCase: "EQUIPMENT_NEW");

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
                btnRunStart.Visibility = Visibility.Collapsed;
                btnCancelConfirm.Visibility = Visibility.Collapsed;
                grdOutTranPrint.Visibility = Visibility.Collapsed;

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
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Timer Stop.
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
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

            #region [버튼 권한 적용에 따른 처리]
            GetButtonPermissionGroup();
            #endregion

            #region # 매거진 발행 권한처리
            //if (_Util.IsCommonCodeUse("LNS_MAGAZINE_PRINT_AREA", LoginInfo.CFG_AREA_ID) == true)
            //{
            //    bMagzinePrintVisible = true;
            //    grdOutTranPrint.Visibility = Visibility.Visible;
            //    btnOutPrint.Visibility = Visibility.Visible;
            //    dgOut.Columns["PRINT_YN_NAME"].Visibility = Visibility.Visible;
            //}                
            //else
            //{
            //    bMagzinePrintVisible = false;
            //    grdOutTranPrint.Visibility = Visibility.Collapsed;
            //    btnOutPrint.Visibility = Visibility.Collapsed;
            //    dgOut.Columns["PRINT_YN_NAME"].Visibility = Visibility.Collapsed;
            //}
            #endregion

            if (LoginInfo.CFG_AREA_ID.Equals("A9") && dgProductLot.Columns.Contains("PRODNAME"))
                dgProductLot.Columns["PRODNAME"].Visibility = Visibility.Visible;
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
            this.RegisterName("greenBrush", greenBrush);

            HideTestMode();
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
            //GetEqptWrkInfo();
        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            ASSY001_COM_RUNSTART wndRunStart = new ASSY001_COM_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.LAMINATION;
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

            ASSY001_COM_EQPTEND wndPop = new ASSY001_COM_EQPTEND();
            wndPop.FrameOperation = FrameOperation;

            if (wndPop != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = Process.LAMINATION;
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
            if (Util.EQPTCondition(Process.LAMINATION, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnEqptCond_Click(null, null);
                return;
            }
            // 품질정보 등록 여부
            if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, Process.LAMINATION, Util.NVC(cboEquipmentSegment.SelectedValue), Util.NVC(cboEquipment.SelectedValue), _ValueToLotID))
            {
                btnQualityInput_Click(null, null);
                return;
            }
            #endregion

            // 설비 Loss 등록 여부 체크
            DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(cboEquipment.SelectedValue.ToString(), Process.LAMINATION);

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
        
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        //2019.04.24 김대근 / 현재 공정을 wndWait로 전달
        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            ASSY001_COM_WAITLOT wndWait = new ASSY001_COM_WAITLOT();
            wndWait.FrameOperation = FrameOperation;

            if (wndWait != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.LAMINATION;
                Parameters[3] = _UNLDR_LOT_IDENT_BAS_CODE;
                C1WindowExtension.SetParameters(wndWait, Parameters);

                wndWait.Closed += new EventHandler(wndWait_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWait.ShowModal()));
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
                Parameters[2] = Process.LAMINATION;
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
            //winWorkOrder.PROCID = Process.NOTCHING;

            //winWorkOrder.ClearWorkOrderInfo();

            //ClearControls();
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetTestMode();
                GetPilotProdMode();
                GetEqptConfType();

                GetSpclProdMode();

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
                    winWorkOrder.PROCID = Process.LAMINATION;

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
        
        private void dgOut_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":                            
                            break;
                        case "CSTID":
                            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                            {
                                dgOut.EndEdit();
                                SetCarrierMapping(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTID")));
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOut_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;


                if ((Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["CHK"]).Equals("0") && e.Column.Name.Equals("CSTID")) || (Util.NVC((e.Row.DataItem as System.Data.DataRowView).Row["CSTID"]).Trim().IndexOf("NOREAD") < 0 && e.Column.Name.Equals("CSTID")))
                {
                    e.Cancel = true;
                    //dgOut.BeginEdit(e.Row.Index, 4);
                }

                if (InputMethod.Current != null)
                    InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
            }
            catch (Exception ex)
            {
            }
        }

        private void dgOut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        if (Util.NVC(e.Cell.Column.Name).Equals("LOTID"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        if (bMagzinePrintVisible)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
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
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
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
                Parameters[2] = Process.LAMINATION;
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
                Parameters[1] = Process.LAMINATION;
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
                Parameters[0] = Process.LAMINATION;
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

                GetLotIdentBasCode();

                // Lot 식별 기준 코드에 따른 컨트롤 변경.
                SetInOutCtrlByLotIdentBasCode();
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
                Parameters[2] = Process.LAMINATION;

                C1WindowExtension.SetParameters(wndWipNote, Parameters);

                wndWipNote.Closed += new EventHandler(wndWipNote_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWipNote.ShowModal()));
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

            ASSY001_COM_CHG_HOLD_RESN wndAutoHoldHist = new ASSY001_COM_CHG_HOLD_RESN();
            wndAutoHoldHist.FrameOperation = FrameOperation;

            if (wndAutoHoldHist != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.LAMINATION;

                C1WindowExtension.SetParameters(wndAutoHoldHist, Parameters);

                wndAutoHoldHist.Closed += new EventHandler(wndAutoHoldHist_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndAutoHoldHist.ShowModal()));
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

        private void btnOutAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!CanAddOutMagazine())
                return;

            //생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOutMagazine();
                }
            });
        }

        private void btnOutDel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelOutMagazine())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            //삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        OutDelete();
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

        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutMagazine())
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
                        OutSave();
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

        private void txtOutCnt_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutCnt.Text, 0))
                {
                    txtOutCnt.Text = "";
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

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            MainContents.RowDefinitions[0].Height = new GridLength(0);
            MainContents.RowDefinitions[2].Height = new GridLength(0);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star); ;
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            MainContents.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            MainContents.RowDefinitions[0].Height = new GridLength(5);
            MainContents.RowDefinitions[2].Height = new GridLength(5);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0, GridUnitType.Star);
            gla.To = new GridLength(1, GridUnitType.Star);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            MainContents.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void txtOutCa_KeyUp(object sender, KeyEventArgs e)
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

        private void dgOut_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        public void GetProductLot(bool bSelPrv = true)
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
                }

                ClearControls();

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIPINFO_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.LAMINATION;
                //newRow["WOID"] = Util.NVC(drWorkOrderInfo["WOID"]);
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                //newRow["WIPSTAT"] = "WAIT,PROC,END";

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_LM_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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
                                GetWaitPancakeList();
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
                                GetWaitPancakeList();
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

        private void GetOutMagazine(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(rowview["LOTID"]);

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_LM_L", "INDATA", "OUTDATA", inTable);
                
                Util.GridSetData(dgOut, searchResult, FrameOperation, true);
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
        
        private void CreateOutMagazine()
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
                //dr["USERID"] = txtUserNameCr.Tag.ToString();
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_CHK_LOT_LABEL_PRT_RESTRCT", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ShowLoadingIndicator();

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        //inDataTable.Columns.Add("LANGID", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                        //inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));                        
                        inDataTable.Columns.Add("OUTPUTQTY", typeof(int));                        
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                        inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInputTable.Columns.Add("INPUT_LOTID", typeof(string));

                        DataTable inTable = indataSet.Tables["INDATA"];
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        //newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();                        
                        newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        //newRow["OUT_LOTID"] = sNewOutLot;
                        newRow["CSTID"] = txtOutCa.Text;                       
                        newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutCnt.Text);                       
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);
                        newRow = null;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_LM_L", "INDATA,IN_INPUT", "OUTDATA", (searchResult, searchException) =>
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
                                txtOutCa.Text = "";
                                txtOutCnt.Text = "";

                                //정상 처리 되었습니다.
                                Util.MessageInfoAutoClosing("SFU1275");

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

        private void OutDelete()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_MAGAZINE_LM();

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

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                    newRow = input_LOT.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));

                    input_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_LM", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
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

                        //정상 처리 되었습니다.
                        Util.MessageValidation("SFU1275");
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

        private void OutDeleteConfirm()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_MAGAZINE_LM();

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

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                    {
                        newRow = input_LOT.NewRow();
                        newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));

                        input_LOT.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DELETE_OUT_LOT_LM", "INDATA,IN_INPUT", "OUT_LOT", indataSet);
                
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

        private void OutSave()
        {
            try
            {
                ShowLoadingIndicator();

                dgOut.EndEdit();

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

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                    newRow = outLotTable.NewRow();

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                    newRow["OUT_LOT_WIPSEQ"] = DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSEQ");
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"));

                    outLotTable.Rows.Add(newRow);
                }

                if (outLotTable.Rows.Count < 1)
                    return;

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_OUT_LOT_QTY_L", "IN_DATA,IN_OUTLOT", null, indataSet);


                GetWorkOrder(); // 작지 생산수량 정보 재조회.
                GetProductLot();

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (idx >= 0)
                {
                    GetOutMagazine(dgProductLot.Rows[idx].DataItem as DataRowView);
                }
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void OutSaveEndState()
        {
            try
            {
                ShowLoadingIndicator();

                dgOut.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_MODIFY_LOT();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                    if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSTAT")).Equals("END"))
                    {
                        newRow = outLotTable.NewRow();

                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                        newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSEQ")));
                        newRow["WIPQTY_ED"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")));

                        outLotTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MODIFY_LOT_WIPQTY", "INDATA,IN_INPUT", null, indataSet);

                GetWorkOrder(); // 작지 생산수량 정보 재조회.
                GetProductLot();

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (idx >= 0)
                {
                    GetOutMagazine(dgProductLot.Rows[idx].DataItem as DataRowView);
                }
            }
            finally
            {
                HideLoadingIndicator();
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
                    if (!Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("S")) // P : ERP 전송 중 , Y : ERP 전송 완료
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

        /// <summary>
        /// 2017.07.11: Unload 식별구분이 CST_ID인 경우에는 라벨 발행여부 확인 안함.(폴란드)
        /// </summary>
        /// <returns></returns>
        private bool CheckLabelPrint()
        {
            try
            {
                bool bValue = true;

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.LAMINATION;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "RQSTDT", "RSLDT", dtRQSTDT);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString().Trim().ToUpper().Equals("CST_ID") && LoginInfo.CFG_AREA_ID == "A5")
                    bValue = false;

                return bValue;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return true;
            }

        }

        private int GetNotPrintCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_NOT_DISPATCH_CNT();

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_PRT_LABEL_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["NOT_PRT_LABEL_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOT_PRT_LABEL_CNT"]));
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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_DISPATCH_CNT_WITHOUT_PROC", "INDATA", "OUTDATA", inTable);

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
                newRow["PROCID"] = Process.LAMINATION;

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
                Indata["PROCID"] = Process.LAMINATION;
                
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

        private void GetEqptConfType()
        {
            try
            {
                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    _EQPT_CONF_TYPE = "";
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = cboEquipment.SelectedValue;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_CONF_TYPE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("EQPT_CONF_TYPE"))
                {
                    _EQPT_CONF_TYPE = Util.NVC(dtRslt.Rows[0]["EQPT_CONF_TYPE"]);
                }
            }
            catch (Exception ex)
            {
                _EQPT_CONF_TYPE = "";
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
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (iRow < 0)
                    return;

                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_LM();

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

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
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

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1275");
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

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_LM_L", "INDATA,IN_INPUT", "OUT_LOT", indataSet);

                GetProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");

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
                dtRow["PROCID"] = Process.LAMINATION;
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

                //20220615 AUDIT로 인해 임시 설정 - 김영환
                _LDR_LOT_IDENT_BAS_CODE = "RF_ID";
                _UNLDR_LOT_IDENT_BAS_CODE = "RF_ID";
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
        
        private int GetNoreadCNT(DataTable inTable)
        {
            try
            {
                int iCnt = 0;

                if (inTable?.Rows?.Count < 1) return iCnt;

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOREAD_CST_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["NOREAD_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOREAD_CNT"]));
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

        private void SetCarrierMapping(string sLotID, string sCstID)
        {
            try
            {
                if (sCstID.ToUpper().Equals("NOREAD")) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLotID;
                newRow["CSTID"] = sCstID;
                newRow["PROCID"] = Process.LAMINATION;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CSTID_USING_RFID", "INDATA", null, inTable);

                // 변경되었습니다.   
                Util.MessageInfo("PSS9097", (msgResult) =>
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                    {
                        GetOutMagazine(dgProductLot.Rows[idx].DataItem as DataRowView);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (msgResult) =>
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                    {
                        GetOutMagazine(dgProductLot.Rows[idx].DataItem as DataRowView);
                    }
                });
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
                dr["PROCID"] = Process.LAMINATION;
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
                                case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                    btnCancelConfirm.Visibility = Visibility.Visible;
                                    break;
                                case "OUTPUT_W_C1":         // 발행 사용권한
                                    bMagzinePrintVisible = true;
                                    grdOutTranPrint.Visibility = Visibility.Visible;
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

        private void GetDefectInfo(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

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
                newRow["PROCID"] = Process.LAMINATION;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
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

        public void GetEqptLoss(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);

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

        private bool CanRunStart()
        {
            bool bRet = false;

            if (_Util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC") > -1)
            {
                //Util.Alert("장비에 진행 중 인 LOT이 존재 합니다.");
                Util.MessageValidation("SFU1863");
                return bRet;
            }

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

        private bool CanAddOutMagazine()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (txtOutCnt.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutCnt.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutCnt.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutCnt.SelectAll();
                return bRet;
            }

            if ((Convert.ToDecimal(txtOutCnt.Text) % 1) > 0)
            {
                //Util.Alert("소수점 입력은 불가능 합니다. 수량을 확인해 주세요.");
                Util.MessageValidation("SFU2342");
                txtOutCnt.SelectAll();
                return bRet;
            }

            // LOT TYPE 에 따른 VALIDATION 처리            
            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
            {
                // MC/HC 인 경우만 체크. 
                if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODUCT_LEVEL2_CODE")).Equals("BC") &&
                   !Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODUCT_LEVEL2_CODE")).Equals("MC"))
                {
                    if (txtOutCa.Text.Equals(""))
                    {
                        //Util.Alert("카세트ID를 입력 하세요.");
                        Util.MessageValidation("SFU6051");
                        txtOutCa.Focus();
                        return bRet;
                    }
                }
            }

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (txtOutCa.Text.Trim().Equals(""))
                {
                    //Inline설비 && Mono Type일 경우 카세트ID 입력에 관한 조건 무시
                    if (!(Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODUCT_LEVEL2_CODE")).Equals("MC") &&
                      _EQPT_CONF_TYPE.Equals("INLINE")))
                    {

                        //Util.Alert("카세트ID를 입력 하세요.");
                        Util.MessageValidation("SFU6051");
                        txtOutCa.Focus();
                        return bRet;
                    }
                }
            }
            
            if (LoginInfo.CFG_AREA_ID == "A5" && !CheckedUseCassette())  //Cassette 중복여부 체크.
            {
                txtOutCa.SelectAll();
                return bRet;
            }
            bRet = true;
            return bRet;
        }
        
        public bool CanDelOutMagazine()
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

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")),
                                    Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool CanSaveOutMagazine()
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

            bool bFnd = false;
            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                bFnd = true;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
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
            ASSY001_COM_RUNSTART window = sender as ASSY001_COM_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
            }
        }
        
        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            ASSY001_COM_CONFIRM window = sender as ASSY001_COM_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
            }
        }
        
        private void wndWait_Closed(object sender, EventArgs e)
        {
            ASSY001_COM_WAITLOT window = sender as ASSY001_COM_WAITLOT;
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
            ASSY001_COM_EQPTEND window = sender as ASSY001_COM_EQPTEND;
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

        private void wndWipNote_Closed(object sender, EventArgs e)
        {
            CMM_COM_WIP_NOTE window = sender as CMM_COM_WIP_NOTE;
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

        private void wndCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CELL_INFO window = sender as CMM_ASSY_CELL_INFO;
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

        private void wndDfctCellReg_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DFCT_CELL_REG window = sender as CMM_ASSY_DFCT_CELL_REG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                //if (idx < 0) return;

                //GetEqptLoss((DataRowView)dgProductLot.Rows[idx].DataItem);
            }
        }

        private void wndAutoHoldHist_Closed(object sender, EventArgs e)
        {
            ASSY001_COM_CHG_HOLD_RESN window = sender as ASSY001_COM_CHG_HOLD_RESN;
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
            listAuth.Add(btnWipNote);
            listAuth.Add(btnRunStart);
            listAuth.Add(btnCancelConfirm);
            listAuth.Add(btnSpclProdMode);

            listAuth.Add(btnOutAdd);
            listAuth.Add(btnOutDel);
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
                    winInput = new UC_IN_OUTPUT_L(Process.LAMINATION, sEqsg, sEqpt);

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
            winWorkOrder.PROCID = Process.LAMINATION;

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
            winInput.PROCID = Process.LAMINATION;

            winInput.GetCurrInList();
        }

        private void GetWaitPancakeList()
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
            winInput.PROCID = Process.LAMINATION;
            winInput.GetWaitPancake();
        }

        public bool GetSearchConditions(out string sProc, out string sEqsg, out string sEqpt)
        {
            try
            {
                sProc = Process.LAMINATION;
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
            Util.gridClear(dgInputLoss);
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

            ProcessDetail(dgProductLot.Rows[iRow].DataItem);
        }

        private void ProcessDetail(object obj)
        {
            DataRowView rowview = (obj as DataRowView);

            if (rowview == null)
                return;

            GetOutMagazine(rowview);
            GetDefectInfo(rowview);
            GetEqptLoss(rowview);
            GetEqptSectCutOffList(rowview);
        }

        private void ConfirmProcess()
        {
            ASSY001_COM_CONFIRM wndConfirm = new ASSY001_COM_CONFIRM();
            wndConfirm.FrameOperation = FrameOperation;

            if (wndConfirm != null)
            {
                object[] Parameters = new object[13];
                Parameters[0] = Process.LAMINATION;
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

                    int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                    if (iRow < 0)
                        return;

                    if (dgProductLot.Rows[iRow] != null && dgProductLot.Rows[iRow].DataItem != null)
                        GetOutMagazine((dgProductLot.Rows[iRow].DataItem as DataRowView));

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
                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("CST_ID") || _UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    if (dgOut.Columns.Contains("CSTID"))
                        dgOut.Columns["CSTID"].Visibility = Visibility.Visible;

                    lblCstID.Visibility = Visibility.Visible;
                    txtOutCa.Visibility = Visibility.Visible;

                    if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {

                    }
                }
                else
                {
                    if (dgOut.Columns.Contains("CSTID"))
                        dgOut.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    lblCstID.Visibility = Visibility.Collapsed;
                    txtOutCa.Visibility = Visibility.Collapsed;
                }                
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

        private void CreateOutProdWithOutMsg()
        {
            if (!CanAddOutMagazine())
                return;

            CreateOutMagazine();
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
                    Parameters[0] = Process.LAMINATION;
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

        #region 특별관리 설정/해제
        private void btnSpclProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSpclProdMode()) return;
            
            bool bMode = GetSpclProdMode();
            
            //적용 하시겠습니까?
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetSpclProdMode(!bMode);
                    GetSpclProdMode();
                }
            });
        }

        private bool CanSpclProdMode()
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

            string sEqptDspName = string.Empty;
            sProcLotID = string.Empty;
            
            if (CheckLoaderEqptProcWip(out sEqptDspName, out sProcLotID))
            {
                Util.MessageValidation("SFU3747", sEqptDspName, sProcLotID); // 작업오류 : 로더 설비를 공유하는 [%1] 설비에 진행중인 LOT [%2] 이 있습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CheckLoaderEqptProcWip(out string sEqptDspName ,out string sProcLotID)
        {
            sEqptDspName = "";
            sProcLotID = "";

            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));                

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_WIP_BY_LDR_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sEqptDspName = Util.NVC(dtRslt.Rows[0]["EQPTDSPNAME"]);
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

        private bool GetSpclProdMode()
        {
            try
            {
                bool bRet = false;

                if (cboEquipment == null || cboEquipment.SelectedIndex < 0 || Util.NVC(cboEquipment.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideSpclProdMode();
                    return bRet;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("SPCL_LOT_GNRT_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y"))
                    {
                        ShowSpclProdMode();
                        bRet = true;
                    }
                    else
                    {
                        HideSpclProdMode();
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

        private bool SetSpclProdMode(bool bMode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SPCL_LOT_GNRT_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["SPCL_LOT_GNRT_FLAG"] = bMode ? "Y" : "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_SPCL_LOT_INPUT", "INDATA", null, inTable);

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

        private void HideSpclProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdSpclProd.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void ShowSpclProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdSpclProd.Visibility = Visibility.Visible;
                //txtPilotProdMode.Text = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
                ColorAnimationInSpecialLot();
            }));
        }

        private void ColorAnimationInSpecialLot()
        {
            recSpclProdMode.Fill = greenBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "greenBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
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
                Parameters[3] = Process.LAMINATION;
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
                Indata["PROCID"] = Process.LAMINATION;

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

        private void btnSaveInputLoss_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (idx < 0) return;

                string sLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("SECTION_ID", typeof(string));
                inDataTable.Columns.Add("OCCR_COUNT", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = dgInputLoss.TopRows.Count; i < dgInputLoss.Rows.Count - dgInputLoss.BottomRows.Count; i++)
                {
                    DataRow newRow = inDataTable.NewRow();

                    decimal dCnt = 0;
                    decimal.TryParse(Util.NVC(DataTableConverter.GetValue(dgInputLoss.Rows[i].DataItem, "OCCR_COUNT")), out dCnt);

                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                    newRow["LOTID"] = sLot;
                    newRow["SECTION_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputLoss.Rows[i].DataItem, "SECTION_ID"));
                    newRow["OCCR_COUNT"] = dCnt;
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);
                }

                if (inDataTable.Rows.Count < 1)
                {
                    HideLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_SECTION_CUTOFF", "INDATA", null, inDataTable, (bizResult, bizException) =>
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
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                HideLoadingIndicator();
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                int iTimes = 0;

                int.TryParse(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT")), out iTimes);

                DataTableConverter.SetValue(bt.DataContext, "OCCR_COUNT", ++iTimes);


                //파단수량 및 전체 수량을 다시 계산함
                DataTableConverter.SetValue(bt.DataContext, "CUT_EA", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SECTION_EA"))) * Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT"))));

                DataTable dtList = DataTableConverter.Convert(dgInputLoss.ItemsSource);
                Util.GridSetData(dgInputLoss, dtList, FrameOperation, false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                int iTimes = 0;

                int.TryParse(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT")), out iTimes);

                DataTableConverter.SetValue(bt.DataContext, "OCCR_COUNT", iTimes < 1 ? 0 : --iTimes);

                //파단수량 및 전체 수량을 다시 계산함
                DataTableConverter.SetValue(bt.DataContext, "CUT_EA", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SECTION_EA"))) * Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT"))));

                DataTable dtList = DataTableConverter.Convert(dgInputLoss.ItemsSource);
                Util.GridSetData(dgInputLoss, dtList, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetEqptSectCutOffList(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                //inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                //newRow["WIPSEQ"] = _Wipseq;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_EQPT_SECTION_CUTOFF", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //2020-01-25 OHB 파단수 계산
                        if (searchResult.Rows.Count > 0)
                        {
                            searchResult.Columns.Add(new DataColumn("CUT_EA", typeof(decimal)));

                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {
                                searchResult.Rows[i]["CUT_EA"] = Convert.ToDecimal(searchResult.Rows[i]["SECTION_EA"]) * Convert.ToDecimal(searchResult.Rows[i]["OCCR_COUNT"]);
                            }

                        }
                        Util.GridSetData(dgInputLoss, searchResult, FrameOperation, false);
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

        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrintOutMagazine())
                return;

            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            Util.MessageConfirm("SFU1237", (result) =>
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                        {
                            try
                            {
                                btnOutPrint.IsEnabled = false;

                                using (ThermalPrint thmrPrt = new ThermalPrint())
                                {
                                    thmrPrt.Print(sEqsgID: Util.NVC(cboEquipmentSegment.SelectedValue),
                                                  sEqptID: Util.NVC(cboEquipment.SelectedValue),
                                                  sProcID: Process.LAMINATION,
                                                  inData: GetGroupPrintInfo(),
                                                  iType: THERMAL_PRT_TYPE.COM_OUT_RFID_GRP,
                                                  iPrtCnt: 1,
                                                  bSavePrtHist: true,
                                                  bDispatch: true);


                                    GetOutMagazine(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem as DataRowView);

                                    if (chkAll != null && chkAll.IsChecked.HasValue)
                                        chkAll.IsChecked = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                btnOutPrint.IsEnabled = true;
                            }
                        }
                        else
                        {
                            btnOutPrint.IsEnabled = false;

                            List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                            {
                                if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                                DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")));

                                if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                                //라미
                                dicParam.Add("reportName", "Lami"); //dicParam.Add("reportName", "Fold");
                                dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                                dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                                dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                                dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                                dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                                dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                                dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                                dicParam.Add("TITLEX", "MAGAZINE ID");

                                dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                                dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.

                                dicList.Add(dicParam);
                            }

                            CMM_THERMAL_PRINT_LAMI print = new CMM_THERMAL_PRINT_LAMI();
                            print.FrameOperation = FrameOperation;

                            if (print != null)
                            {
                                object[] Parameters = new object[7];
                                Parameters[0] = dicList;
                                Parameters[1] = Process.LAMINATION;
                                Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                                Parameters[3] = cboEquipment.SelectedValue.ToString();
                                Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                                Parameters[5] = "Y";   // 디스패치 처리.
                                Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                                C1WindowExtension.SetParameters(print, Parameters);

                                print.Closed += new EventHandler(print_Closed);

                                print.Show();
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
                    // Timer Start.
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();

                    btnOutPrint.IsEnabled = true;
                }
            });
        }

        private DataTable GetGroupPrintInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                string sTmp = string.Empty;

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                    if (sTmp.Length < 1)
                        sTmp = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                    else
                        sTmp = sTmp + "," + Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));
                }

                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sTmp;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_GRP_PRT_INFO_FD", "INDATA", "OUTDATA", inTable);
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


        private bool CanPrintOutMagazine()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("라미LOT이 선택되지 않았습니다.");
                Util.MessageValidation("SFU1519");
                return bRet;
            }

            if (_Util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    //Util.Alert("진행중인 매거진은 발행할 수 없습니다.");
                    Util.MessageValidation("SFU1919");
                    return bRet;
                }

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return bRet;
                }

                if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                {
                    using (DataTable dtTmp = new DataTable())
                    {
                        dtTmp.Columns.Add("PR_LOTID", typeof(string));
                        dtTmp.Columns.Add("LOTID", typeof(string));

                        DataRow drTmp = dtTmp.NewRow();
                        drTmp["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        drTmp["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID"));

                        dtTmp.Rows.Add(drTmp);

                        if (GetNoreadCNT(dtTmp) > 0)
                        {
                            // 캐리어가 맵핑되지 않은 Lot은 발행할 수 없습니다. (LOT : %1)
                            Util.MessageValidation("SFU4934", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")));
                            return bRet;
                        }
                    }
                }
            }

            bRet = true;
            return bRet;
        }
        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
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

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_LAMI window = sender as CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            GetOutMagazine(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem as DataRowView);

            if (chkAll != null && chkAll.IsChecked.HasValue)
                chkAll.IsChecked = false;
        }
    }
}
