/*************************************************************************************
 Created Date : 2021.12.07
      Creator : 신광희
   Decription : 소형 조립 공정진척(NFF) 메인
--------------------------------------------------------------------------------------
 [Change History]
 2021.12.07  신광희 : Initial Created.
 2022.04.26  김태균 : 환경설정에서 VD를 선택했을 경우, Winding 선택되게 하고 자동조회 가능토록 함
 2022.06.02  배현우 : 추가작업 - Tray 재구성 기능 추가
 2023.02.08  배현우 : 와인딩 실적 확정시 wipqty 수량이 0으로 초기화 (수량 원복 로직 주석처리)
 2023.10.25  김용군 : 오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
 2023.12.05  배현우 : 와인더 투입위치별 무지부 권취방향 설정 기능 추가 (추가기능)
 2024.01.16  배현우 : ZTZ 설비완공시 tmpProdQty 값이 없으면 설비완공수량으로 반영하도록 수
 2024.03.14  남기운 : 허용비율 초과시 사유 등록 프로세서 추가
 2024.04.24  배현우 : Winder 자동 실적 확정 모드 설정/해제 기능 추가
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.ASSY006.Controls;
using LGC.GMES.MES.ASSY006.Popup;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Threading;
using System.Threading.Tasks;
using LGC.GMES.MES.ASSY003;

namespace LGC.GMES.MES.ASSY006
{
    public partial class ASSY006_001 : IWorkArea
    {
        #region Declaration


        public UcAssemblyCommand UcAssemblyCommand { get; set; }
        public UcAssemblyEquipment UcAssemblyEquipment { get; set; }
        public UcAssemblyProductLot UcAssemblyProductLot { get; set; }
        public UcAssemblyProductionResult UcAssemblyProductionResult { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private C1DataGrid DgEquipment { get; set; }
        private C1DataGrid DgProductLot { get; set; }
        //소형 조립 ZTZ(A5600) 설비 대응
        private C1DataGrid DgProductLotZtz { get; set; }
        private C1DataGrid DgProductQty { get; set; }
        private C1DataGrid DgRemarkInfo { get; set; }
   
        private string _equipmentSegmentCode;
        private string _equipmentSegmentName;
        private string _processCode;
        private string _processName;
        private string _equipmentCode;
        private string _equipmentName;
        private string _treeEquipmentCode;
        private string _treeEquipmentName;
        private string _productLot;

        private bool _isAutoSelectTime = false;
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();
        DataRowView _dvProductLot;
        //
        ASSY006_TRAY_RECONF celTrayReconf;

        //ZZS 추가
        ASSY003_023_WAITLOT wndWaitLot;
        CMM_ASSY_QUALITY_PKG wndQualitySearch;

        //PACKAGING 추가
        WND_CPROD_TRANSFER wndCProduct;
        ASSY003_007_BOX_IN wndBoxIn;
        ASSY003_OUTLOT_MERGE wndMerge;
        private UC_IN_OUTPUT_MOBILE winInput = null;
        ASSY003_007_INPUT_OBJECT wndInputObject;
        ASSY003_007_CONFIRM wndConfirm;
        ASSY003_023_CONFIRM wndConfirmZZS;
        ASSY003_007_RUNSTART wndRunStart;

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
        ASSY006_ZTZ_WAITLOT wndWaitLotZtz;

        //허용비율 초과 사유
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;
        bool _bInputErpRate = false;
        bool _isLastBatch = false;
        private bool _isAutoConfirmMode;

        //previous
        private struct PreviousValues
        {
            public string PreviousProdLotId;
            public decimal? PreviousReInputQty;

            public PreviousValues(decimal? reInputQty, string prodLotId)
            {
                PreviousReInputQty = reInputQty;
                PreviousProdLotId = prodLotId;
            }
        }

        private PreviousValues _previousValues = new PreviousValues(null, string.Empty);

        private enum SetEquipmentType
        {
            SearchButton,
            EquipmentComboChange,
            EquipmentTreeClick,
            ProductLotClick
        }

        public IFrameOperation FrameOperation { get; set; }

        #endregion

        #region Initialize
        public ASSY006_001()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            SetComboBox();
            InitializeUserControls();
            SetEventInUserControls();
            SetProcessInUserControls();

            // Setting 정보에 설정된 라인, 공정, 설비 등으로 화면 오픈에 따른 초기 설정 
            GetButtonPermissionGroup();

            if (_dispatcherMainTimer != null)
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.GetString()))
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

        private void SetComboBox()
        {
            // 라인
            SetEquipmentSegmentCombo(cboEquipmentSegment);

            // 공정
            SetProcessCombo(cboProcess);

            // 설비
            SetEquipmentCombo(cboEquipment);

            // 자동조회
            SetAutoSearchCombo(cboAutoSearch);

            // 이벤트 생성
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
        }

        private void InitializeUserControls()
        {
            UcAssemblyCommand = grdCommand.Children[0] as UcAssemblyCommand;
            UcAssemblyEquipment = grdEquipment.Children[0] as UcAssemblyEquipment;
            UcAssemblyProductLot = grdProduct.Children[0] as UcAssemblyProductLot;
            UcAssemblyProductionResult = grdProductionResult.Children[0] as UcAssemblyProductionResult;

            if (UcAssemblyCommand != null)
            {
                UcAssemblyCommand.FrameOperation = FrameOperation;
                UcAssemblyCommand.ProcessCode = _processCode;
                UcAssemblyCommand.SetButtonVisibility(UcAssemblyCommand.ButtonVisibilityType.ProductLot);
                UcAssemblyCommand.ApplyPermissions();
            }


            if (UcAssemblyEquipment != null)
            {
                UcAssemblyEquipment.UcParentControl = this;
                UcAssemblyEquipment.FrameOperation = FrameOperation;
                UcAssemblyEquipment.DgEquipment.MouseLeftButtonUp += DgEquipment_MouseLeftButtonUp;
                UcAssemblyEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
                UcAssemblyEquipment.ProcessCode = _processCode;
                UcAssemblyEquipment.EquipmentCode = _equipmentCode;
                DgEquipment = UcAssemblyEquipment.dgEquipment;
                SetEquipmentTree();
            }

            if (UcAssemblyProductLot != null)
            {
                UcAssemblyProductLot.UcParentControl = this;
                UcAssemblyProductLot.FrameOperation = FrameOperation;
                UcAssemblyProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
                UcAssemblyProductLot.ProcessCode = _processCode;
                UcAssemblyProductLot.EquipmentCode = _equipmentCode;
                UcAssemblyProductLot.txtSelectEquipment.Text = _equipmentCode;
                UcAssemblyProductLot.txtSelectEquipmentName.Text = _equipmentName;
                UcAssemblyProductLot.SetControlVisibility();
                //UcAssemblyProductLot.cboColor.SelectedIndex = 0;

                UcAssemblyProductLot.dgProductLot.PreviewMouseDoubleClick += DgProductLot_PreviewMouseDoubleClick;
                UcAssemblyProductLot.dgProductLot.MouseLeftButtonUp += DgProductLot_MouseLeftButtonUp;

                DgProductLot = UcAssemblyProductLot.dgProductLot;

                //소형 조립 ZTZ(A5600) 설비 대응
                UcAssemblyProductLot.dgProductLotZtz.PreviewMouseDoubleClick += DgProductLotZtz_PreviewMouseDoubleClick;
                UcAssemblyProductLot.dgProductLotZtz.MouseLeftButtonUp += DgProductLotZtz_MouseLeftButtonUp;

                DgProductLot = UcAssemblyProductLot.dgProductLotZtz;
                DgProductQty = UcAssemblyProductionResult.dgDefectDetail;
                DgRemarkInfo = UcAssemblyProductionResult.dgRemark;

                SetProductLotList();
            }

            if (UcAssemblyProductionResult != null)
            {
                UcAssemblyProductionResult.UcParentControl = this;
                UcAssemblyProductionResult.FrameOperation = FrameOperation;
                UcAssemblyProductionResult.ApplyPermissions();
                UcAssemblyProductionResult.btnSaveWipHistory.Click += ButtonSaveWipHistory_Click;
            }
        }

        private void InitializeButtonPermissionGroup()
        {
            //TODO 공정별 구분이 필요 할수 있음
            UcAssemblyEquipment.IsInputUseAuthority = false;
            UcAssemblyEquipment.IsWaitUseAuthority = false;
            UcAssemblyEquipment.IsInputHistoryAuthority = false;

        }

        private void SetEventInUserControls()
        {

            if (UcAssemblyCommand != null)
            {
                UcAssemblyCommand.btnProductLot.Click += ButtonProductLot_Click;                 //Product Lot

                UcAssemblyCommand.btnWaitPancake.Click += ButtonWaitPancake_Click;               //대기Pancake조회
                UcAssemblyCommand.btnWindingLot.Click += ButtonWindingLot_Click;                 //Winding LOT 조회
                UcAssemblyCommand.btnWipNote.Click += ButtonWipNote_Click;                       //특이사항관리
                UcAssemblyCommand.btnEqptCondSearch.Click += ButtonEqptCondSearch_Click;         //작업조건조회
                UcAssemblyCommand.btnRunStart.Click += ButtonRunStart_Click;                     //작업시작
                UcAssemblyCommand.btnCancel.Click += ButtonCancel_Click;                         //시작취소
                UcAssemblyCommand.btnCancelConfirm.Click += ButtonCancelConfirm_Click;           //확정취소
                UcAssemblyCommand.btnCancelTerm.Click += ButtonCancelTerm_Click;                 //투입LOT종료취소
                UcAssemblyCommand.btnCancelTermSepa.Click += ButtonCancelTermSepa_Click;
                UcAssemblyCommand.btnEditEqptQty.Click += ButtonEditEqptQty_Click;               //설비투입수량수정

                UcAssemblyCommand.btnEqptEnd.Click += ButtonEqptEnd_Click;                       //장비완료
                UcAssemblyCommand.btnEqptEndCancel.Click += ButtonEqptEndCancel_Click;           //장비완료취소

                UcAssemblyCommand.btnEqptCondMain.Click += ButtonEqptCond_Click;                 //작업조건등록
                UcAssemblyCommand.btnQualityInput.Click += ButtonQualityInput_Click;             //품질정보입력

                UcAssemblyCommand.btnEqptIssue.Click += ButtonEqptIssue_Click;                   //인수인계노트
                UcAssemblyCommand.btnConfirm.Click += ButtonConfirm_Click;                       //실적확정
                UcAssemblyCommand.btnHistoryCard.Click += ButtonHistoryCard_Click;               //이력카드

                UcAssemblyCommand.btnBoxPrint.Click += ButtonBoxPrint_Click;                     //BOX발행
                UcAssemblyCommand.btnTrayLotChange.Click += ButtonTrayLotChange_Click;           //Tray LOT 변경
                UcAssemblyCommand.btnLastCellNo.Click += ButtonLastCellNo_Click;                 //Final Cell ID 
                UcAssemblyCommand.btnCellDetailInfo.Click += ButtonCellDetailInfo_Click;         //CELL정보조회

                UcAssemblyCommand.btnPilotProdMode.Click += ButtonPilotProdMode_Click;           //시생산설정/해제
                UcAssemblyCommand.btnTrayReconf.Click += ButtonTrayReconf_Click;                 //Tray 재구성

                //ZZS 추가
                UcAssemblyCommand.btnWaitLot.Click += btnWaitLot_Click;                          //대기LOT조회
                UcAssemblyCommand.btnQualitySearch.Click += btnQualitySearch_Click;              //품질정보조회
                //UcAssemblyCommand.btnScheduledShutdown.Click += btnScheduledShutdown_Click;      //계획정지

                //PACKAGING 추가
                UcAssemblyCommand.btnCProduct.Click += btnCProduct_Click;                          //C생산인계
                UcAssemblyCommand.btnBoxIn.Click += btnBoxIn_Click;                          //C생산BOX인수
                UcAssemblyCommand.btnMerge.Click += btnMerge_Click;                          //LOTMERGE

                /*
                UcAssemblyCommand.btnRemarkHistory.Click += ButtonRemarkHistory_Click;           //특이사항이력(전공정)
                UcAssemblyCommand.btnEqptCondSearch.Click += ButtonEqptCondSearch_Click;         //작업조건조회
                UcAssemblyCommand.btnEqptCond.Click += ButtonEqptCond_Click;                     //작업조건등록
                UcAssemblyCommand.btnEqptCondMain.Click += ButtonEqptCond_Click;                 //작업조건등록
                UcAssemblyCommand.btnWipNote.Click += ButtonWipNote_Click;                       //특이사항관리
                UcAssemblyCommand.btnReworkMove.Click += ButtonReworkMove_Click;                 //재작업대기LOT이동
                UcAssemblyCommand.btnEqptIssue.Click += ButtonEqptIssue_Click;                   //인수인계노트
                UcAssemblyCommand.btnWorkHalfSlitSide.Click += ButtonWorkHalfSlitSide_Click;     //무지부 방향설정
                UcAssemblyCommand.btnEmSectionRollDirctn.Click += ButtonEmSectionRollDirctn_Click; //권취방향변경
                
                UcAssemblyCommand.btnRunStart.Click += ButtonRunStart_Click;                     //작업시작
                UcAssemblyCommand.btnRunCancel.Click += ButtonRunCancel_Click;                   //시작취소
                UcAssemblyCommand.btnRunComplete.Click += ButtonRunComplete_Click;               //장비완료
                UcAssemblyCommand.btnRunCompleteCancel.Click += ButtonRunCompleteCancel_Click;   //장비완료취소
                UcAssemblyCommand.btnCancelConfirm.Click += ButtonCancelConfirm_Click;           //확정취소
                UcAssemblyCommand.btnSpclProdMode.Click += ButtonSpclProdMode_Click;             //특별관리설정/해제
                UcAssemblyCommand.btnMerge.Click += ButtonMerge_Click;                           //LOT Merge
                UcAssemblyCommand.btnChgCarrier.Click += ButtonChgCarrier_Click;                 //Carrier 교체
                UcAssemblyCommand.btnTrayInfo.Click += ButtonTrayInfo_Click;                     //활성화 트레이 조회
                UcAssemblyCommand.btnPilotProdMode.Click += ButtonPilotProdMode_Click;           //시생산설정/해제
                UcAssemblyCommand.btnPilotProdSPMode.Click += ButtonPilotProdSPMode_Click;       //시생산샘플설정/해제
                UcAssemblyCommand.btnRework.Click += ButtonRework_Click;                         //재작업대기LOT이동

                UcAssemblyCommand.btnQualityInput.Click += ButtonQualityInput_Click;                 //품질정보입력
                UcAssemblyCommand.btnProductLot.Click += ButtonProductLot_Click;                     //Product Lot
                UcAssemblyCommand.btnConfirm.Click += ButtonConfirm_Click;                           //실적확정
                UcAssemblyCommand.btnPrint.Click += ButtonPrint_Click;                               //바코드발행
                */

                //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                UcAssemblyCommand.btnPrint.Click += ButtonPrint_Click;                               //바코드발행
                //오창 2산단 NFF MP 라인 대응
                UcAssemblyCommand.btnWorkHalfSlitSide.Click += ButtonWorkHalfSlitSide_Click;     //무지부 방향설정
                UcAssemblyCommand.btnAutoRsltCnfm.Click += ButtonAutoRsltCnfmMode_Click;
            }

        }


        private void SetProcessInUserControls()
        {
        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _equipmentSegmentCode = LoginInfo.CFG_EQSG_ID;
            _equipmentSegmentName = LoginInfo.CFG_EQSG_NAME;
            _processCode = LoginInfo.CFG_PROC_ID;
            _processName = LoginInfo.CFG_PROC_NAME;
            _equipmentCode = LoginInfo.CFG_EQPT_ID;
            _equipmentName = LoginInfo.CFG_EQPT_NAME;

            ApplyPermissions();
            Initialize();

            Loaded -= UserControl_Loaded;
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //UcPolymerFormProductionResult.DispatcherTimer?.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlEquipmentSegment();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // 공정 
            SetProcessCombo(cboProcess);
            // Clear
            SetControlClear();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && string.IsNullOrEmpty(cboProcess.SelectedValue?.GetString())) return;

            if (cboProcess?.SelectedValue == null || string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()))
            {
                SetControlClearByProcess();
                return;
            }

            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlProcess();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // 설비 
            SetEquipmentCombo(cboEquipment);

            UcAssemblyCommand.SetButtonVisibility(UcAssemblyCommand.ButtonVisibilityType.ProductLot);

            // ProductLot 영역의 컨트롤 Visibility 속성 정의
            UcAssemblyProductLot.SetControlVisibility();

            // 공정 변경에 따른 버튼 권한 조회
            GetButtonPermissionGroup();

            // Clear
            SetControlClear();

        }

        private void cboEquipment_SelectionChanged(object sender, EventArgs e)
        {
            SetControlClear();
            SetEquipment(SetEquipmentType.EquipmentComboChange);
            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherMainTimer != null)
                {
                    _dispatcherMainTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearch?.SelectedValue != null && !cboAutoSearch.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                        // 자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU8170");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherMainTimer.Start();

                    if (_isAutoSelectTime)
                    {
                        // 자동조회  %1초로 변경 되었습니다.
                        Util.MessageValidation("SFU5127", cboAutoSearch?.SelectedValue?.ToString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //if (wndWaitLot != null)
            //    wndWaitLot = null;

            //wndWaitLot = new ASSY003_023_WAITLOT();            
            //wndWaitLot.FrameOperation = FrameOperation;

            //if (wndWaitLot != null)
            //{
            //    object[] Parameters = new object[2];
            //    Parameters[0] = _equipmentSegmentCode;
            //    Parameters[1] = _equipmentCode;
            //    C1WindowExtension.SetParameters(wndWaitLot, Parameters);

            //    wndWaitLot.Closed += new EventHandler(wndWait_Closed);

            //    this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
            //}
            if (string.Equals(_processCode, Process.ZTZ))
            {
                if (wndWaitLotZtz != null)
                    wndWaitLotZtz = null;

                wndWaitLotZtz = new ASSY006_ZTZ_WAITLOT();
                wndWaitLotZtz.FrameOperation = FrameOperation;

                if (wndWaitLotZtz != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    C1WindowExtension.SetParameters(wndWaitLotZtz, Parameters);

                    wndWaitLotZtz.Closed += new EventHandler(wndWait_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndWaitLotZtz.ShowModal()));

                }
            }
            else
            {
                if (wndWaitLot != null)
                    wndWaitLot = null;

                wndWaitLot = new ASSY003_023_WAITLOT();
                wndWaitLot.FrameOperation = FrameOperation;

                if (wndWaitLot != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                    wndWaitLot.Closed += new EventHandler(wndWait_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
                }
            }
        }

        private void wndWait_Closed(object sender, EventArgs e)
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //wndWaitLot = null;
            //ASSY003_023_WAITLOT window = sender as ASSY003_023_WAITLOT;

            //if (window.DialogResult == MessageBoxResult.OK)
            //{

            //}
            if (string.Equals(_processCode, Process.ZTZ))
            {
                wndWaitLotZtz = null;
                ASSY006_ZTZ_WAITLOT window = sender as ASSY006_ZTZ_WAITLOT;

                if (window.DialogResult == MessageBoxResult.OK)
                {

                }
            }
            else
            {
                wndWaitLot = null;
                ASSY003_023_WAITLOT window = sender as ASSY003_023_WAITLOT;

                if (window.DialogResult == MessageBoxResult.OK)
                {

                }
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
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _equipmentSegmentName;
                Parameters[2] = _equipmentCode;
                Parameters[3] = Process.PACKAGING;
                Parameters[4] = EquipmentGroup.PACKAGING;
                Parameters[5] = _equipmentName;
                
                C1WindowExtension.SetParameters(wndCProduct, Parameters);

                wndCProduct.Closed += new EventHandler(wndCProduct_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndCProduct.ShowModal()));
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

        private bool CanCPrdTransfer()
        {
            bool bRet = false;

            if (Util.NVC(_equipmentCode).Equals("") || Util.NVC(_equipmentCode).Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
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
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
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

        private bool CanBoxIn()
        {
            bool bRet = false;

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (Util.NVC(_equipmentCode).Equals("") || Util.NVC(_equipmentCode).Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
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
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _equipmentCode;
                Parameters[2] = Process.PACKAGING;
                Parameters[3] = cboEquipmentSegment.Text;

                C1WindowExtension.SetParameters(wndMerge, Parameters);

                wndMerge.Closed += new EventHandler(wndMerge_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndMerge.ShowModal()));
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

        private bool CanMerge()
        {
            bool bRet = false;

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            bRet = true;
            return bRet;
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
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = Process.ZZS;
                Parameters[2] = _equipmentCode;
                Parameters[3] = _equipmentSegmentName;
                Parameters[4] = _equipmentName;

                C1WindowExtension.SetParameters(wndQualitySearch, Parameters);

                wndQualitySearch.Closed += new EventHandler(wndQualityRslt_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndQualitySearch.ShowModal()));
            }
        }

        private bool CanSearch()
        {
            bool bRet = false;

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (Util.NVC(_equipmentCode).Equals("") || Util.NVC(_equipmentCode).Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            wndQualitySearch = null;
            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.IsEnabled = false;
            UcAssemblyProductLot.IsSearchResult = false;

            SetEquipment(SetEquipmentType.SearchButton);

            Task<bool> task = WaitCallback();
            task.ContinueWith(_ =>
            {
                Thread.Sleep(500);
                btnSearch.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DgEquipment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (DgEquipment == null || DgEquipment.CurrentCell == null) return;
            if (grdProduct.Visibility == Visibility.Collapsed) return;

            string equipment = DgEquipment.CurrentCell.Row.DataItem.ToString();

            if (string.IsNullOrWhiteSpace(equipment) || equipment.Split(':').Length < 2)
            {
                _treeEquipmentCode = string.Empty;
                _treeEquipmentName = string.Empty;
            }
            else
            {
                _treeEquipmentCode = equipment.Split(':')[0].Trim();
                _treeEquipmentName = equipment.Split(':')[1].Trim();

                SetEquipment(SetEquipmentType.EquipmentTreeClick);

                // Product Lot 정렬
                DataTable dt = DataTableConverter.Convert(DgProductLot.ItemsSource);

                if (dt == null || dt.Rows.Count == 0) return;

                DataRow[] drSelect = dt.Select("EQPTID = '" + _treeEquipmentCode + "'");

                //해당 설비가 생산 Lot 정보가 없는 경우
                if (drSelect.Length == 0)
                {

                    // Sort 처리
                    dt = (from t in dt.AsEnumerable()
                          orderby t.Field<string>("EQPTID") ascending, t.Field<string>("WIPSTAT") descending, t.Field<string>("PR_LOTID") descending
                          select t).CopyToDataTable();

                    dt.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                    dt.AcceptChanges();

                    Util.GridSetData(DgProductLot, dt, null, true);
                    return;
                }

                var queryNoneEquipment = dt.AsEnumerable().Where(o => string.IsNullOrEmpty(o.Field<string>("EQPTID"))).ToList();
                DataTable dtNoneEquipment = queryNoneEquipment.Any() ? queryNoneEquipment.CopyToDataTable() : dt.Clone();

                var querySelect = dt.AsEnumerable().Where(o => o.Field<string>("EQPTID") == _treeEquipmentCode).ToList();
                DataTable dtSelect = querySelect.Any() ? querySelect.OrderBy(y => y.Field<string>("EQPTID")).ThenByDescending(z => z.Field<string>("WIPSTAT")).CopyToDataTable() : dt.Clone();

                var queryNoneSelect = dt.AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Field<string>("EQPTID")) && x.Field<string>("EQPTID") != _treeEquipmentCode).ToList();
                DataTable dtNoneSelect = queryNoneSelect.Any() ? queryNoneSelect.OrderBy(y => y.Field<string>("EQPTID")).ThenByDescending(z => z.Field<string>("WIPSTAT")).CopyToDataTable() : dt.Clone();

                DataTable dtSort = dt.Clone();
                dtSort.Merge(dtSelect);
                dtSort.Merge(dtNoneSelect);
                dtSort.Merge(dtNoneEquipment);
                dtSort.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                dtSort.AcceptChanges();

                UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = false;
                Util.GridSetData(DgProductLot, dtSort, null, true);
                UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = true;
            }
        }

        private void DgProductLot_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg != null && (dg.CurrentCell == null || dg.CurrentCell.Row == null)) return;

            // +, - 선택에 따른 라디오버튼 체크가 비정상 작동을 하는 경우가 발생하여 TOGGLEKEY 컬럼 영역 선택시엔 라디오 버튼 체크 배제 처리 함.
            if (dg.CurrentCell == null || dg.CurrentCell.Column.Name == "TOGGLEKEY") return;
            if (dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter == null) return;

            RadioButton rb = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

            int rowIdx = dg.CurrentCell.Row.Index;
            DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            if (string.IsNullOrEmpty(drv["LOTID"].GetString())) return;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);

            if (dg.CurrentCell.Column.Name == "LOTID" && !string.IsNullOrEmpty(drv["LOTID"].GetString()))
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                if (drShift.Length > 0)
                {
                    UcAssemblyProductionResult.WorkerId = drShift[0]["WRK_USERID"].GetString();
                    UcAssemblyProductionResult.ShiftId = drShift[0]["SHFT_ID"].GetString();
                    UcAssemblyProductionResult.WorkerName = drShift[0]["VAL002"].GetString();
                }

                //this.Cursor = Cursors.Wait;
                dg.CurrentCell.Presenter.Cursor = Cursors.Wait;

                UcAssemblyCommand.SetButtonVisibility(UcAssemblyCommand.ButtonVisibilityType.ProductionResult);

                GetButtonPermissionGroup();

                grdProduct.Visibility = Visibility.Collapsed;
                grdProductionResult.Visibility = Visibility.Visible;

                SetUserControlProductionResult();
                UcAssemblyProductionResult.SelectProductionResult();

                dg.CurrentCell.Presenter.Cursor = Cursors.Hand;
            }
        }

        private void DgProductLot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg?.CurrentCell == null || dg.CurrentCell.Row == null) return;
            if (dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter == null) return;

            // +, - 선택에 따른 라디오버튼 체크가 비정상 작동을 하는 경우가 발생하여 TOGGLEKEY 컬럼 영역 선택시엔 라디오 버튼 체크 배제 처리 함.
            if (dg.CurrentCell.Column.Name == "TOGGLEKEY") return;

            int rowIdx = dg.CurrentCell.Row.Index;
            DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            DgProductLot = UcAssemblyProductLot.dgProductLot;
            RadioButton rb = DgProductLot.GetCell(DgProductLot.CurrentCell.Row.Index, DgProductLot.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelRun()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        /// <summary>
        /// /////////////////////////////////////////////
        /// 
        /// 
        /// 
        /// </summary>
        private void BR_PRD_REG_PERMIT_RATE_OVER_HIST()
        {
            try
            {
                DataTable lotTab = _dtRet_Data.DefaultView.ToTable(true, new string[] { "LOTID", "WIPSEQ" });
                string sLot = "";

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));


                DataTable inRESN = indataSet.Tables.Add("IN_RESN");
                inRESN.Columns.Add("PERMIT_RATE", typeof(decimal));
                inRESN.Columns.Add("ACTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(decimal));
                inRESN.Columns.Add("OVER_QTY", typeof(decimal));
                inRESN.Columns.Add("REQ_USERID", typeof(string));
                inRESN.Columns.Add("REQ_DEPTID", typeof(string));
                inRESN.Columns.Add("DIFF_RSN_CODE", typeof(string));
                inRESN.Columns.Add("NOTE", typeof(string));

                for (int j = 0; j < lotTab.Rows.Count; j++)
                {

                    inTable.Rows.Clear();
                    inRESN.Rows.Clear();
                    sLot = lotTab.Rows[j]["LOTID"].ToString();

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;  //cboEquipment.SelectedValue.ToString();
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = sLot;
                    newRow["WIPSEQ"] = lotTab.Rows[j]["WIPSEQ"].ToString();
                    inTable.Rows.Add(newRow);
                    newRow = null;


                    for (int i = 0; i < _dtRet_Data.Rows.Count; i++)
                    {
                        if (sLot.Equals(_dtRet_Data.Rows[i]["LOTID"].ToString()))
                        {
                            newRow = inRESN.NewRow();
                            newRow["PERMIT_RATE"] = Convert.ToDecimal(_dtRet_Data.Rows[i]["PERMIT_RATE"].ToString());
                            newRow["ACTID"] = _dtRet_Data.Rows[i]["ACTID"].ToString();
                            newRow["RESNCODE"] = _dtRet_Data.Rows[i]["RESNCODE"].ToString();
                            newRow["RESNQTY"] = _dtRet_Data.Rows[i]["RESNQTY"].ToString();
                            newRow["OVER_QTY"] = _dtRet_Data.Rows[i]["OVER_QTY"].ToString();
                            newRow["REQ_USERID"] = _sUserID;
                            newRow["REQ_DEPTID"] = _sDepID;
                            newRow["DIFF_RSN_CODE"] = _dtRet_Data.Rows[i]["SPCL_RSNCODE"].ToString();
                            newRow["NOTE"] = _dtRet_Data.Rows[i]["RESNNOTE"].ToString();
                            inRESN.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PERMIT_RATE_OVER_HIST", "INDATA,IN_LOT", null, (bizResult, bizException) =>
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

                        }
                    }, indataSet);
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// ////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //???
        private void popRermitRate_Closed(object sender, EventArgs e)
        {

            CMM_PERMIT_RATE popRermitRate = sender as CMM_PERMIT_RATE;
            if (popRermitRate != null && popRermitRate.DialogResult == MessageBoxResult.OK)
            {
                _dtRet_Data.Clear();
                _dtRet_Data = popRermitRate.PERMIT_RATE.Copy();
                _sUserID = popRermitRate.UserID;
                _sDepID = popRermitRate.DeptID;
                _bInputErpRate = true;



                //////////////////////////////
                //확정 처리

                if (string.Equals(_processCode, Process.WINDING))
                {
                    ConfirmProcessWinding_Rate();
                }
                else if (string.Equals(_processCode, Process.ASSEMBLY))
                {
                    ConfirmProcessAssembly_Rate();
                }
                else if (string.Equals(_processCode, Process.ZZS))
                {
                   // Confirm_ProcessZZS_Rate();
                }
                else if (string.Equals(_processCode, Process.PACKAGING))
                {
                    //ConfirmProcessPKG_Rate();
                }
                else if (string.Equals(_processCode, Process.ZTZ))
                {
                    ConfirmProcess_Rate(_isLastBatch);
                } else
                {
                    ConfirmProcessWashing_Rate();
                }


                /////////////////////////////////////
                //Rate저장 Biz 호출

            }
            foreach (Grid grid in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (grid.Name == "grdMain")
                {
                    //  grid.Children.Remove(pop);
                    break;
                }
            }

        }



        private double GetLotINPUTQTY(string sLotID)
        {
            double dInputQty = 0;
            if (string.Equals(_processCode, Process.WINDING))
            {
                dInputQty = Util.NVC_Int(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY"));
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                dInputQty = Util.NVC_Int(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY"));
            }
            else if (string.Equals(_processCode, Process.ZZS))
            {
                dInputQty = Util.NVC_Int(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY"));
            }
            else if (string.Equals(_processCode, Process.PACKAGING))
            {
                dInputQty = Util.NVC_Int(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY"));
            }
            else if (string.Equals(_processCode, Process.ZTZ))
            {
                dInputQty = Util.NVC_Int(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY"));
            }
            return dInputQty;
        }


        private bool PERMIT_RATE_input(string sLotID, string sWipSeq)
        {
            bool bFlag = false;
            try
            {

                DataTable dtTmp = new DataTable();

                if (string.Equals(_processCode, Process.ZZS) || string.Equals(_processCode, Process.PACKAGING))
                {
                    dtTmp = DataTableConverter.Convert(UcAssemblyProductionResult.dgDefectZZS.ItemsSource);
                }
                //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                else if (string.Equals(_processCode, Process.ZTZ))
                {
                    dtTmp = DataTableConverter.Convert(UcAssemblyProductionResult.dgDefectZtz.ItemsSource);
                }
                else
                {
                    dtTmp = DataTableConverter.Convert(UcAssemblyProductionResult.dgDefect.ItemsSource);
                } 
                ////////생산수량을 가지고 온다
                double goodQty = GetLotINPUTQTY(sLotID);

                if (goodQty <= 0)
                    return bFlag;


                DataTable data = new DataTable();
                data.Columns.Add("LOTID", typeof(string));
                data.Columns.Add("WIPSEQ", typeof(string));
                data.Columns.Add("ACTID", typeof(string));
                data.Columns.Add("ACTNAME", typeof(string));
                data.Columns.Add("RESNCODE", typeof(string));

                data.Columns.Add("RESNNAME", typeof(string));
                data.Columns.Add("DFCT_CODE_DETL_NAME", typeof(string));
                data.Columns.Add("RESNQTY", typeof(string));
                data.Columns.Add("PERMIT_RATE", typeof(string));
                data.Columns.Add("OVER_QTY", typeof(string));
                data.Columns.Add("SPCL_RSNCODE", typeof(string));
                data.Columns.Add("SPCL_RSNCODE_NAME", typeof(string));
                data.Columns.Add("RESNNOTE", typeof(string));


                decimal dRate = 0;
                decimal dQty = 0;
                decimal dAllowQty = 0;
                decimal OverQty = 0;

                for (int j = 0; j < dtTmp.Rows.Count; j++)
                {
                    dRate = Util.NVC_Decimal(dtTmp.Rows[j]["PERMIT_RATE"].ToString());


                    // dQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNQTY")); //불량수량      
                    // string ss = DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNNAME").ToString(); //불량수량      
                    //등록된 Rate가 0보다 큰것인 것만 적용
                    if (dRate > 0)
                    {
                        //dQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNQTY")); //불량수량                         

                        dQty = Util.NVC_Decimal(dtTmp.Rows[j]["RESNQTY"].ToString());//불량수량    

                        dAllowQty = Convert.ToDecimal(goodQty) * dRate / 100;
                        if (dAllowQty < dQty)
                        {
                            OverQty = dQty - dAllowQty;
                            OverQty = Math.Ceiling(OverQty); //소수점 첫자리 올림

                            DataRow newRow = data.NewRow();
                            newRow["LOTID"] = sLotID; //필수
                            newRow["WIPSEQ"] = sWipSeq; //필수
                            newRow["ACTID"] = Util.NVC(dtTmp.Rows[j]["ACTID"].ToString());// Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTID")); //필수
                            newRow["ACTNAME"] = Util.NVC(dtTmp.Rows[j]["ACTNAME"].ToString()); //Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTNAME")); //필수
                            newRow["RESNCODE"] = Util.NVC(dtTmp.Rows[j]["RESNCODE"].ToString()); // Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNCODE")); //필수
                            newRow["RESNNAME"] = Util.NVC(dtTmp.Rows[j]["RESNNAME"].ToString());  //Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNNAME")); //필수
                            newRow["DFCT_CODE_DETL_NAME"] = "";
                            newRow["RESNQTY"] = dQty.ToString("G29"); //필수
                            newRow["PERMIT_RATE"] = dRate.ToString("0.00");  //필수 0제거 
                            newRow["OVER_QTY"] = OverQty.ToString("G29"); //(dQty - dAllowQty).ToString("0.000"); //소수점 3자리
                            newRow["SPCL_RSNCODE"] = "";
                            newRow["SPCL_RSNCODE_NAME"] = "";
                            newRow["RESNNOTE"] = "";
                            data.Rows.Add(newRow);
                        }
                    }
                }

                //등록 할 정보가 있으면 
                if (data.Rows.Count > 0)
                {

                    CMM_PERMIT_RATE popRermitRate = new CMM_PERMIT_RATE { FrameOperation = FrameOperation };
                    object[] parameters = new object[2];
                    parameters[0] = sLotID;
                    parameters[1] = data;
                    C1WindowExtension.SetParameters(popRermitRate, parameters);

                    popRermitRate.Closed += popRermitRate_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popRermitRate.ShowModal()));

                    bFlag = true;
                }
              
                return bFlag;
                ///////////////////////////////////////////////                  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bFlag;
            }

        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            UcAssemblyProductionResult.dgDefectDetail.EndEdit();
            UcAssemblyProductionResult.dgDefectDetail.EndEditRow(true);


            if (!ValidationConfirm()) return;

            if (string.Equals(_processCode, Process.WINDING))
            {
                if (!ValidationInspectionLot()) return;
                if (!ValidationInspectionTime()) return;

                if (DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() == 0)
                {
                    //양품량이 0입니다. 그래도 실적확정 하시겠습니까?
                    Util.MessageConfirm("SFU4497", result1 =>
                    {
                        if (result1 == MessageBoxResult.OK)
                        {
                            ConfirmProcessWinding();
                        }
                    });
                }
                else
                {
                    //실적 확정 하시겠습니까?
                    Util.MessageConfirm("SFU1706", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ConfirmProcessWinding();
                        }
                    });
                }
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                Util.MessageConfirm("SFU1706", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmProcessAssembly();
                    }
                });
            }
            else if (string.Equals(_processCode, Process.ZZS))
            {
                // Loss 입력 여부 체크.
                DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, Process.ZZS);

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
                            Confirm_ProcessZZS();
                        }
                    }, param);
                }
                else
                {
                    Confirm_ProcessZZS();
                }
            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(_processCode, Process.ZTZ))
            {
                if (UcAssemblyProductionResult.textInputQtyZtz.Value > 0)
                    return;

                // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
                if (ValidConfirmLotCheck() == false)
                    return;

                // 설비완공 상태에서만 실적확정 가능하도록 변경 [2017-03-02]
                if (!string.Equals(_dvProductLot["WIPSTAT"].GetString(), "EQPT_END"))
                {
                    Util.MessageValidation("SFU3194");  //실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                    return;
                }

                if (!ValidateConfirm())
                {
                    return;
                }

                CheckAuthValidation(() =>
                {
                    ConfirmCheck();
                });
            }
            else if (string.Equals(_processCode, Process.PACKAGING))
            {
                // Loss 입력 여부 체크.
                DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, Process.PACKAGING);

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

                            ConfirmProcessPKG();
                        }
                    }, param);
                }
                else
                {
                    if (!CanInputComplete())
                        return;

                    ConfirmProcessPKG();
                }
            }
            else
            {
                Util.MessageConfirm("SFU1706", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmProcessWashing();
                    }
                });
            }
        }

        private void ButtonWaitPancake_Click(object sender, RoutedEventArgs e)
        {
            PopupWaitPancake(sender);
        }

        private void ButtonWindingLot_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWindingLot()) return;

            PopupWindingLot();
        }

        private void ButtonEqptCondSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptCondSearch()) return;

            PopupEqptCondSearch();
        }

        private void ButtonEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptCond()) return;

            PopupEqptCond();
        }

        private void ButtonWipNote_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWipNote()) return;

            PopupWipNote();
        }


        private void ButtonEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEquipmentIssue()) return;

            PopupEquipmentIssue();
        }

        private void ButtonHistoryCard_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationHistoryCard()) return;

            PopupHistoryCard();
        }

        private void ButtonBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_processCode)) return;
            PopupBoxPrint();
        }

        private void ButtonTrayLotChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayMove()) return;
            PopupTrayLotChange();
        }

        private void ButtonSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveWipHistory()) return;
            SaveWipHistory();
        }

        private void ButtonRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart()) return;

            PopupRunStart();
        }

        private void ButtonEqptEnd_Click(object sendeer, RoutedEventArgs e)
        {
            if (!ValidationEquipmentEnd()) return;

            PopupEquipmentEnd();
        }

        private void ButtonEqptEndCancel_Click(object sendeer, RoutedEventArgs e)
        {
            if (!ValidationEquipmentEndCancel()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EquipmentEndCancel();
                }
            });
        }

        private void ButtonCancelConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelConfirm()) return;

            PopupCancelConfirm();
        }

        private void ButtonCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            PopupCancelTerm();
        }

        private void ButtonCancelTermSepa_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelTermSepa()) return;
            PopupCancelTermSepa();
        }

        private void ButtonEditEqptQty_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEditEquipmentQty()) return;

            PopupEditEquipmentQty();
        }

        private void ButtonQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualityInput()) return;

            PopupQualityInput();
        }

        private void ButtonLastCellNo_Click(object sender, RoutedEventArgs e)
        {
            PopupLastCellNo();
        }

        private void ButtonCellDetailInfo_Click(object sender, RoutedEventArgs e)
        {
            PopupCellDetailInfo();
        }

        private void ButtonPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPilotProdMode()) return;

            //bool isMode = GetPilotProdMode();
            string sFromMode = GetPilotProdMode();
            string messageCode;
            string sToMode = string.Empty;

            if (sFromMode.Equals("PILOT"))
            {
                messageCode = "SFU8304";    //[%1]을 시생산 해제하시겠습니까?
                sToMode = string.Empty;
            }
            else
            {
                messageCode = "SFU8303";    //[%1]을 시생산 설정하시겠습니까?
                sToMode = "PILOT";
            }

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (SetPilotProdMode(sToMode) == false)
                    {
                        return;
                    }
                    Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                }
            }, UcAssemblyProductLot.txtSelectEquipmentName.Text);
        }

        private void ButtonProductLot_Click(object sender, RoutedEventArgs e)
        {
            string sFromMode = GetPilotProdMode();

            if (UcAssemblyProductionResult.IsProductLotRefreshFlag)
            {
                if (_dvProductLot != null)
                {
                    UcAssemblyProductLot.SelectProductList(_dvProductLot["LOTID"].GetString(), sFromMode);
                }
                else
                {
                    UcAssemblyProductLot.SelectProductList(null, sFromMode);
                }
            }

            UcAssemblyCommand.SetButtonVisibility(UcAssemblyCommand.ButtonVisibilityType.ProductLot);

            // 공정 변경에 따른 버튼 권한 조회
            GetButtonPermissionGroup();

            grdProduct.Visibility = Visibility.Visible;
            grdProductionResult.Visibility = Visibility.Collapsed;

            UcAssemblyProductionResult.IsProductLotRefreshFlag = false;
            UcAssemblyProductionResult.IsSelectedAll = false;
            UcAssemblyProductionResult.InitializeControls();
        }

        private void ButtonTrayReconf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanWaitTrayReconf())
                    return;

                if (celTrayReconf != null)
                    celTrayReconf = null;

                celTrayReconf = new ASSY006_TRAY_RECONF();
                celTrayReconf.FrameOperation = FrameOperation;

                if (celTrayReconf != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = _equipmentSegmentName;
                    Parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                    //Parameters[2] = _dvProductLot["LOTID"].GetString();
                    Parameters[3] = _processCode;
                    Parameters[4] = "";

                    Console.WriteLine(Parameters[0] + " " + Parameters[1] + " " + Parameters[2] + " " + Parameters[3] + " " + Parameters[4]);
                    C1WindowExtension.SetParameters(celTrayReconf, Parameters);

                    celTrayReconf.Closed += new EventHandler(celTrayReconf_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => celTrayReconf.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void celTrayReconf_Closed(object sender, EventArgs e)
        {
            celTrayReconf = null;
            ASSY006_TRAY_RECONF window = sender as ASSY006_TRAY_RECONF;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            string processLotId = SelectProcessLotId();
            SelectProductLot(processLotId);
        }



        private bool CanWaitTrayReconf()
        {
            bool bRet = false;

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (Util.NVC(_processCode).Equals("") || Util.NVC(_processCode).Equals("SELECT"))
            {
                //Util.Alert("공정을 선택 하세요.");
                Util.MessageValidation("SFU1459");
                return bRet;
            }
            
            if (Util.NVC(_equipmentCode).Equals("") || Util.NVC(_equipmentCode).Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            bRet = true;

            return bRet;
        }
        private void DispatcherMainTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;

                    // 설비 Tree 조회
                    SetUserControlEquipment();

                    if (cboEquipment.SelectedItems.Any())
                    {
                        //UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                    }

                    // Product Lot 영역 조회
                    if (grdProduct.Visibility == Visibility.Visible)
                    {
                        SetProductLotList();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }


        #endregion

        #region Mehod


        private static void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetProcessCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dtRow = inTable.NewRow();
            dtRow["LANGID"] = LoginInfo.LANGID;
            dtRow["EQSGID"] = cboEquipmentSegment.SelectedValue ?? null;
            inTable.Rows.Add(dtRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            //NFF -> A2000 : Winding,  A3000 : Assembly, A4000 : Washing
            //ZZS -> A8200 : ZZS, A9000 : Package
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //string[] assemblyProcess = new string[] { "A2000", "A3000", "A4000", "A8200", "A9000" };
            string[] assemblyProcess = new string[] { "A2000", "A3000", "A4000", "A5600", "A8200", "A9000" };


            if (CommonVerify.HasTableRow(dtResult))
            {
                dtResult.AsEnumerable().Where(r => !assemblyProcess.Contains(r.Field<string>("CBO_CODE"))).ToList().ForEach(row => row.Delete());
                dtResult.AcceptChanges();
            }

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                    _processCode = cboProcess?.SelectedValue?.ToString();
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
        }

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응_Start
        private string GetZzsProcId()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ZTZ_PROCESS_CODE_CBO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    return Util.NVC(dtRslt.Rows[0]["PROCID"]);
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }
        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응_End

        private static void SetAutoSearchCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "DRB_REFRESH_TERM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NA, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetEquipmentCombo(MultiSelectionBox mcb)
        {
            try
            {
                //const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
                const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_EQPTLEVEL_CBO";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));

                //inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                //inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                dr["PROCID"] = cboProcess.SelectedValue?.ToString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int row = 0; row < dtResult.Rows.Count; row++)
                        {
                            if (dtResult.Rows[row]["CBO_CODE"].ToString() == LoginInfo.CFG_EQPT_ID)
                            {
                                mcb.Check(row);
                            }
                        }
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckSelectWorkOrderInfo()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("EQPTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_WORKORDER_INFO", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (string.IsNullOrEmpty(dtRslt.Rows[0]["WO_DETL_ID"].GetString()))
                    {
                        //선택된 W/O가 없습니다.
                        Util.MessageValidation("SFU1635");
                        return false;
                    }

                    if (dtRslt.Rows[0]["WO_STAT_CHK"].GetString() == "N")
                    {
                        //선택된 W/O가 없습니다.
                        Util.MessageValidation("SFU1635");
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private DataRow SelectWorkOrderInfo()
        {
            const string bizRuleName = "DA_PRD_SEL_ASSY_WORKORDER_EIOATTR_DRB";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("EQPTID", typeof(string));
            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                return dtResult.Rows[0];
            }
            else
            {
                return null;
            }
        }

        private void GetButtonPermissionGroup()
        {
            if (!Util.pageAuthCheck(FrameOperation.AUTHORITY)) return;

            InitializeButtonPermissionGroup();

            DataTable inTable = new DataTable();
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));

            DataRow dtRow = inTable.NewRow();
            dtRow["USERID"] = LoginInfo.USERID;
            dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            //dtRow["AREAID"] = "M9"; //TODO 테스트 이후 삭제 예정
            dtRow["PROCID"] = _processCode;
            dtRow["EQGRID"] = null;
            inTable.Rows.Add(dtRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow dr in dtResult.Rows)
                {
                    if (dr == null) continue;

                    switch (Util.NVC(dr["BTN_PMS_GRP_CODE"]))
                    {
                        case "INPUT_W":         // 투입 사용 권한
                        case "WAIT_W":          // 대기 사용 권한
                        case "INPUTHIST_W":     // 투입이력 사용 권한
                            SetPermissionPerInputButton(Util.NVC(dr["BTN_PMS_GRP_CODE"]));
                            break;
                    }
                }
            }
        }

        private void SetPermissionPerInputButton(string buttonPermissionGroupCode)
        {
            if (UcAssemblyEquipment == null) return;
            UcAssemblyEquipment.SetPermissionPerButton(buttonPermissionGroupCode);
        }

        private void CancelRun()
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            if (string.Equals(_processCode, Process.ZTZ))
            {
                CancelRunZtz();
            }
            else
            {
                try
                {
                    string bizRuleName;
                    if (string.Equals(_processCode, Process.WINDING))
                        bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_WN";
                    else if (string.Equals(_processCode, Process.ASSEMBLY))
                        bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_AS";
                    else if (string.Equals(_processCode, Process.ZZS))
                        bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_CL";
                    else
                        bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_WS";

                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = _equipmentCode;
                    newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                    {
                        HiddenLoadingIndicator();
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void EquipmentEndCancel()
        {
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            if (string.Equals(_processCode, Process.ZTZ))
            {
                CancelEqpEndZtz();
            }
            else
            {
                try
                {
                    ShowLoadingIndicator();

                    string bizRuleName;

                    if (string.Equals(_processCode, Process.WINDING))
                        bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_WN";
                    else if (string.Equals(_processCode, Process.ASSEMBLY))
                        bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_PROD_LOT_AS";
                    else if (string.Equals(_processCode, Process.ZZS))
                        bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_CL_S";
                    else
                        bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_WS";

                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("SRCTYPE", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("IFMODE", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));

                    if (string.Equals(_processCode, Process.WINDING))
                    {
                        inTable.Columns.Add("LOTID", typeof(string));
                        inTable.Columns.Add("INPUT_LOTID", typeof(string));
                    }
                    else if (string.Equals(_processCode, Process.ASSEMBLY))
                    {
                        inTable.Columns.Add("PROD_LOTID", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                    }
                    else if (string.Equals(_processCode, Process.ZZS))
                    {
                        inTable.Columns.Add("LOTID", typeof(string));
                    }
                    else
                    {
                        inTable.Columns.Add("LOTID", typeof(string));
                    }

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = _equipmentCode;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["USERID"] = LoginInfo.USERID;

                    if (string.Equals(_processCode, Process.WINDING))
                    {
                        newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                        newRow["INPUT_LOTID"] = null;
                    }
                    else if (string.Equals(_processCode, Process.ASSEMBLY))
                    {
                        newRow["PROD_LOTID"] = _dvProductLot["LOTID"].ToString(); //Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                        newRow["WIPNOTE"] = null;
                    }
                    else if (string.Equals(_processCode, Process.ZZS))
                    {
                        newRow["LOTID"] = _dvProductLot["LOTID"].ToString(); //Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                    }
                    else
                    {
                        newRow["LOTID"] = _dvProductLot["LOTID"].ToString(); //Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                    }

                    inTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                            SetProductLotList();

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
        }

        private void ApplyPermissions()
        {
            var ucAssyCommand = grdCommand.Children[0] as UcAssemblyCommand;
            if (ucAssyCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {   // 작업시작, 시작취소, 실적확정
                    //ucAssyCommand.ButtonStart,
                    //ucAssyCommand.ButtonCancel,
                    //ucAssyCommand.ButtonConfirm
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }

        private void SetControlClear()
        {
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            _productLot = string.Empty;
            _dvProductLot = null;

            Util.gridClear(UcAssemblyEquipment.DgEquipment);
        }

        private void SetControlClearByProcess()
        {
            Util.gridClear(UcAssemblyEquipment.DgEquipment);
            Util.gridClear(UcAssemblyProductLot.dgProductLot);
            Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);
            UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
            UcAssemblyProductionResult.SetControlClear();

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            Util.gridClear(UcAssemblyProductLot.dgProductLotZtz);
        }

        private void SetEquipmentTreeControlClear()
        {
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            Util.gridClear(UcAssemblyEquipment.DgEquipment);
        }

        private void SetEquipmentProductLotControlClear()
        {
            Util.gridClear(UcAssemblyProductLot.dgProductLot);
            Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);

            UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
        }

        private void SetUserControlEquipmentSegment()
        {
            _equipmentSegmentCode = cboEquipmentSegment.SelectedValue.ToString();
            _equipmentSegmentName = cboEquipmentSegment.Text;

            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SetUserControlProcess()
        {
            _processCode = cboProcess?.SelectedValue?.ToString();
            if (cboProcess != null) _processName = cboProcess.Text;

            UcAssemblyCommand.ProcessCode = string.IsNullOrEmpty(_processCode) ? LoginInfo.CFG_PROC_ID : _processCode;
            UcAssemblyCommand.SetButtonVisibility(UcAssemblyCommand.ButtonVisibilityType.ProductLot);

            // 공정 변경에 따른 버튼 권한 조회
            GetButtonPermissionGroup();

            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SetUserControlEquipment()
        {
            UcAssemblyEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyEquipment.ProcessCode = _processCode;
            UcAssemblyEquipment.ProcessName = _processName;
            UcAssemblyEquipment.EquipmentCode = cboEquipment.SelectedItemsToString;

            if (_dvProductLot != null)
            {
                UcAssemblyEquipment.ProductLotId = _dvProductLot["LOTID"].GetString();
                UcAssemblyEquipment.SelectEquipmentCode = _dvProductLot["EQPTID"].GetString();
                //UcAssemblyEquipment.ProdWorkOrderId = _dvProductLot["WOID"].GetString();
            }
            else
            {
                UcAssemblyEquipment.ProductLotId = string.Empty;
                UcAssemblyEquipment.SelectEquipmentCode = string.Empty;
                //UcAssemblyEquipment.ProdWorkOrderId = string.Empty;
            }
        }

        private void SetUserControlProductLot()
        {
            UcAssemblyProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyProductLot.ProcessCode = _processCode;
            UcAssemblyProductLot.ProcessName = _processName;
            UcAssemblyProductLot.EquipmentCode = cboEquipment.SelectedItemsToString;
        }

        public void SetUserControlProductionResult()
        {
            if (string.IsNullOrWhiteSpace(_equipmentCode) || _equipmentCode.Split(',').Length > 1) return;

            DataTable dt = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "'").CopyToDataTable();

            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
            if (drShift.Length > 0)
            {
                UcAssemblyProductionResult.WorkerId = drShift[0]["WRK_USERID"].GetString();
                UcAssemblyProductionResult.ShiftId = drShift[0]["SHFT_ID"].GetString();
                UcAssemblyProductionResult.WorkerName = drShift[0]["VAL002"].GetString();
            }

            UcAssemblyProductionResult.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyProductionResult.EquipmentSegmentName = _equipmentSegmentName;
            UcAssemblyProductionResult.ProcessCode = _processCode;
            UcAssemblyProductionResult.ProcessName = _processName;
            UcAssemblyProductionResult.EquipmentCode = _equipmentCode;
            UcAssemblyProductionResult.EquipmentName = _equipmentName;
            UcAssemblyProductionResult.DtEquipment = dt;
            UcAssemblyProductionResult.DvProductLot = _dvProductLot;
        }

        public void SetProductLotSelect(RadioButton rb)
        {

            int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

            for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
            }

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //DgProductLot = UcAssemblyProductLot.dgProductLot;
            if (string.Equals(_processCode, Process.ZTZ))
            {
                DgProductLot = UcAssemblyProductLot.dgProductLotZtz;
            }
            else
            {
                DgProductLot = UcAssemblyProductLot.dgProductLot;
            }

            DgProductLot.SelectedIndex = idx;

            // 라디오 버튼 선택에 따른 DataRowView 생성
            _dvProductLot = DgProductLot.Rows[idx].DataItem as DataRowView;
            if (_dvProductLot == null) return;

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            if (string.Equals(_processCode, Process.ZTZ))
            {
                if (string.IsNullOrEmpty(_dvProductLot["EQPTID"].GetString()) && !string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
                {
                    SelectEquipment(UcAssemblyProductLot.txtSelectEquipment.Text, _dvProductLot["EQPTNAME"].ToString());
                }
                else
                {
                    SelectEquipment(_dvProductLot["EQPTID"].ToString(), _dvProductLot["EQPTNAME"].ToString());
                }
            }
            else
            {
                SelectEquipment(_dvProductLot["EQPTID"].ToString(), _dvProductLot["EQPTNAME"].ToString());
            }
            
            SelectProductLot(_dvProductLot["LOTID"].ToString());
        }

        private void SetEquipment(SetEquipmentType equipmentType)
        {

            switch (equipmentType)
            {
                case SetEquipmentType.SearchButton:

                    UcAssemblyProductLot.txtSelectLot.Text = string.Empty;

                    if (string.IsNullOrEmpty(cboEquipment.SelectedItemsToString))
                    {
                        UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
                        UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
                        UcAssemblyProductLot.IsSearchResult = true;
                        break;
                    }

                    SelectEquipment(cboEquipment.SelectedItemsToString,
                        cboEquipment.SelectedItems.Count == 1 ? GetEquipmentName(cboEquipment) : null);

                    // 설비 Tree 조회
                    SetEquipmentTree();

                    // 1. grdProduct 활성화 시  
                    if (grdProductionResult.Visibility == Visibility.Visible)
                    {
                        btnSearch.IsEnabled = true;
                        UcAssemblyProductLot.IsSearchResult = true;

                        if (!IsCurrentProcessByLotId())
                        {
                            Util.MessageInfo("SFU7352", (result) =>
                            {
                                // Product Lot 화면 전환 및 조회
                                ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                                SetProductLotList();
                            }
                                , _dvProductLot["LOTID"].GetString());
                        }
                        else
                        {
                            // UcAssemblyProductionResult 조회
                            UcAssemblyCommand.SetButtonVisibility(UcAssemblyCommand.ButtonVisibilityType.ProductionResult);

                            // 공정 변경에 따른 버튼 권한 조회
                            GetButtonPermissionGroup();

                            grdProduct.Visibility = Visibility.Collapsed;
                            grdProductionResult.Visibility = Visibility.Visible;
                            _equipmentCode = _dvProductLot["EQPTID"].GetString();
                            UcAssemblyProductionResult.SelectProductionResult();
                        }
                    }
                    else
                    {
                        // Product Lot 조회
                        SetProductLotList();
                    }

                    break;

                case SetEquipmentType.EquipmentComboChange:

                    SetEquipmentProductLotControlClear();

                    if (string.IsNullOrEmpty(cboEquipment.SelectedItemsToString))
                    {
                        UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
                        UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
                        UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
                        break;
                    }

                    //SelectEquipment(cboEquipment.SelectedItemsToString, null);
                    SelectEquipment(cboEquipment.SelectedItemsToString,
                        cboEquipment.SelectedItems.Count == 1 ? GetEquipmentName(cboEquipment) : null);

                    // 설비 Tree 조회
                    SetEquipmentTree();

                    // Product Lot 조회
                    SetProductLotList();
                    break;

                case SetEquipmentType.EquipmentTreeClick:

                    DgEquipment.SelectedBackground = new SolidColorBrush(Colors.Transparent);
                    _equipmentCode = _treeEquipmentCode;
                    _equipmentName = _treeEquipmentName;
                    SelectEquipment(_equipmentCode, _equipmentName);

                    if (grdProduct.Visibility == Visibility.Visible)
                    {
                        SelectProductLot(string.Empty);
                    }
                    break;

                case SetEquipmentType.ProductLotClick:

                    break;
            }
        }

        public void SetEquipmentTree()
        {
            // Clear
            //SetControlClear();
            SetEquipmentTreeControlClear();

            if (cboEquipment.SelectedItems.Any())
            {
                UcAssemblyEquipment.ChangeEquipment(_processCode, _equipmentCode);
            }
        }

        private void SetProductLotList(string processLotId = null)
        {
            UcAssemblyProductLot.SetControlVisibility();

            if (cboEquipment.SelectedItems.Any())
            {
                SetUserControlProductLot();
                string sFromMode = GetPilotProdMode();
                UcAssemblyProductLot.SelectProductList(processLotId, sFromMode);
            }
        }

        /// <summary>
        /// 2021.08.12 : 시생산샘플설정/해제 기능 추가
        /// param bool에서 string으로 변경
        /// </summary>
        /// <param name="sMode"></param>
        /// <returns></returns>
        private bool SetPilotProdMode(string sMode)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PILOT_PRDC_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PILOT_PRDC_MODE"] = sMode; //isMode ? "PILOT" : "";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_PILOT_PRODUCTION_MODE", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097"); // 변경되었습니다.
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public void SelectProductLotList()
        {
            if (grdProduct.Visibility == Visibility.Collapsed)
                UcAssemblyProductionResult.IsProductLotRefreshFlag = true;
            else
            {
                SetProductLotList();
            }
        }

        private void SelectEquipment(string equipmentCode, string equipmentName)
        {
            _equipmentCode = equipmentCode;
            _equipmentName = equipmentName;

            if (equipmentCode.Split(',').Length == 1)
            {
                UcAssemblyProductLot.txtSelectEquipment.Text = _equipmentCode;
                UcAssemblyProductLot.txtSelectEquipmentName.Text = _equipmentName;

                if (string.IsNullOrEmpty(_equipmentName))
                {
                    equipmentName = DgEquipment?.CurrentCell?.Row?.DataItem?.GetString();

                    if (!string.IsNullOrEmpty(equipmentName))
                    {
                        equipmentName = equipmentName.Replace("System.Data.DataRowView", string.Empty);

                        if (equipmentName.Split(':').Length == 2)
                        {
                            equipmentName = equipmentName.Split(':')[1].Trim();
                            UcAssemblyProductLot.txtSelectEquipmentName.Text = equipmentName;
                        }
                    }
                }

                //SelectEquipmentMountPosition();
            }
            else
            {
                if (grdProduct.Visibility == Visibility.Visible)
                {
                    UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
                    UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
                    UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
                }
            }
            SetUserControlEquipment();
            SetUserControlProductLot();

        }

        private void SelectProductLot(string productLotId)
        {
            if (string.IsNullOrEmpty(productLotId))
          {
                _dvProductLot = null;
            }

            _productLot = productLotId;
            UcAssemblyProductLot.txtSelectLot.Text = _productLot;
        }

        private string SelectProcessLotId()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["WIPSTAT"] = Wip_State.PROC;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_EQPT_PROC_LOT_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    return dtResult.Rows[0]["LOTID"].ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private string GetPilotProdMode()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                //if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("PILOT_PROD_MODE"))
                if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("EQPT_OPER_MODE"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["EQPT_OPER_MODE"])))
                    {
                        //ShowPilotProdMode();
                        return Util.NVC(dtRslt.Rows[0]["EQPT_OPER_MODE"]);
                    }
                    else
                    {
                        //HidePilotProdMode();
                        return string.Empty;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private void GetAutoConfirmMode()
        {
            try
            {
                if (string.IsNullOrEmpty(_equipmentCode)) return;
                

                DataTable inTable = _bizDataSet.GetDA_EQP_SEL_AUTO_CONFIRMMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = _equipmentCode;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_AUTO_RSLT_CNFM", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 &&  dtRslt.Columns.Contains("AUTO_RSLT_CNFM_FLAG"))
                {
                    
                  if (Util.NVC(dtRslt.Rows[0]["AUTO_RSLT_CNFM_FLAG"]).Equals("Y"))
                  {
                        _isAutoConfirmMode = true;
                  }
                  else
                  {
                        _isAutoConfirmMode = false;
                  }
                  
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private static string GetEquipmentName(MultiSelectionBox msb)
        {
            DataTable dt = ((DataView)msb.ItemsSource).Table;

            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("CBO_CODE") == msb.SelectedItemsToString
                         select new
                         {
                             EquipmentName = t.Field<string>("CBO_NAME")
                         }).FirstOrDefault();

            return query?.EquipmentName;
        }

        private string GetFormIdByProcess(string processCode)
        {
            string returnFormId = string.Empty;

            switch (processCode)
            {
                case "A5000":
                    returnFormId = "ASSY004_001";
                    break;
                case "A7000":
                    returnFormId = "ASSY004_004";
                    break;
                case "A8000":
                    returnFormId = "ASSY004_006";
                    break;
                case "A9000":
                    returnFormId = "ASSY004_007";
                    break;
                default:
                    returnFormId = "ASSY004_001";
                    break;
            }

            return returnFormId;
        }

        private void ConfirmProcessWinding()
        {
            try
            {
                SaveDefectBeforeConfirm();
                /////////////////////////////////////////////////////////////////////////
                //Rate 관련 팝업
                //완료 처리 하기 전에 팝업 표시
                _bInputErpRate = false;
                _dtRet_Data.Clear();
                _sUserID = string.Empty;
                _sDepID = string.Empty;
                if (PERMIT_RATE_input(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString()))
                {
                    return;
                }
                ///////////////////////////////////////////////////////////////////////////////

                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");               
                const string bizRuleName = "BR_PRD_REG_END_LOT_WNS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _dvProductLot["LOTID"].GetString(); //_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["INPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                newRow["OUTPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["RESNQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = Util.NVC(drShift[0]["SHFT_ID"]);          //UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = null;                                   //new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USERID"] = Util.NVC(drShift[0]["WRK_USERID"]);  //UcAssyShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = Util.NVC(drShift[0]["VAL002"]);   // UcAssyShift.TextWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
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
                        Thread.Sleep(500);

                        if (string.Equals(_processCode, Process.WINDING) && DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() > 0)
                        {   // Winding 이력카드 출력
                            PrintHistoryCard(_dvProductLot["LOTID"].GetString());
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

         private void ConfirmProcessWinding_Rate()
        {
            try
            {
                //SaveDefectBeforeConfirm();

                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                const string bizRuleName = "BR_PRD_REG_END_LOT_WNS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _dvProductLot["LOTID"].GetString(); //_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["INPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                newRow["OUTPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["RESNQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = Util.NVC(drShift[0]["SHFT_ID"]);          //UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = null;                                   //new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USERID"] = Util.NVC(drShift[0]["WRK_USERID"]);  //UcAssyShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = Util.NVC(drShift[0]["VAL002"]);   // UcAssyShift.TextWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }


                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        Thread.Sleep(500);

                        if (string.Equals(_processCode, Process.WINDING) && DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() > 0)
                        {   // Winding 이력카드 출력
                            PrintHistoryCard(_dvProductLot["LOTID"].GetString());
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    

        private void PrintHistoryCard(string lotId)
        {
            try
            {
                const string bizRuleName = "BR_PRD_SEL_WINDING_RUNCARD_WNS";
                DataSet ds = _bizDataSet.GetBR_PRD_SEL_WINDING_RUNCARD_WN();
                DataTable indataTable = ds.Tables["IN_DATA"];
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = lotId;
                indataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_DATA", "OUT_DATA,OUT_ELEC,OUT_DFCT,OUT_SEPA,OUT_TRAY", (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        CMM_ASSY_WINDERCARD_PRINT poopupHistoryCard = new CMM_ASSY_WINDERCARD_PRINT { FrameOperation = FrameOperation };
                        object[] parameters = new object[5];
                        parameters[0] = result;
                        parameters[1] = true;       //ASSY002_010 _isSmallType true; 
                        parameters[2] = lotId;
                        parameters[3] = _processCode;
                        parameters[4] = true;
                        C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                        poopupHistoryCard.Closed += poopupHistoryCard_Closed;
                        Dispatcher.BeginInvoke(new Action(() => poopupHistoryCard.ShowModal()));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WINDERCARD_PRINT popup = sender as CMM_ASSY_WINDERCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void ConfirmProcessAssembly()
        {
            try
            {
                SaveDefectBeforeConfirm();

                /////////////////////////////////////////////////////////////////////////
                //Rate 관련 팝업
                //완료 처리 하기 전에 팝업 표시
                _bInputErpRate = false;
                _dtRet_Data.Clear();
                _sUserID = string.Empty;
                _sDepID = string.Empty;
                if (PERMIT_RATE_input(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString()))
                {
                    return;
                }
                ///////////////////////////////////////////////////////////////////////////////


                ShowLoadingIndicator();
                string workUserId, workUserName, shift;

                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                if (drShift.Length > 0)
                {
                    shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                    workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                    workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                }
                else
                {
                    shift = string.Empty;           //작업조코드
                    workUserId = string.Empty;      //작업자 ID 
                    workUserName = string.Empty;    //작업자명
                }

                const string bizRuleName = "BR_PRD_REG_END_LOT_AS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_AS();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["INPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                newRow["OUTPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["RESNQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = shift;
                //newRow["WIPNOTE"] = new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USERID"] = workUserId;
                newRow["WRK_USER_NAME"] = workUserName;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                newRow["INPUT_DIFF_QTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "REINPUTQTY").GetDouble();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
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
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
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

        private void ConfirmProcessAssembly_Rate()
        {
            try
            {
                //SaveDefectBeforeConfirm();
                ShowLoadingIndicator();
                string workUserId, workUserName, shift;

                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                if (drShift.Length > 0)
                {
                    shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                    workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                    workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                }
                else
                {
                    shift = string.Empty;           //작업조코드
                    workUserId = string.Empty;      //작업자 ID 
                    workUserName = string.Empty;    //작업자명
                }

                const string bizRuleName = "BR_PRD_REG_END_LOT_AS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_AS();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["INPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                newRow["OUTPUTQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["RESNQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = shift;
                //newRow["WIPNOTE"] = new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USERID"] = workUserId;
                newRow["WRK_USER_NAME"] = workUserName;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                newRow["INPUT_DIFF_QTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "REINPUTQTY").GetDouble();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
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

        private void ConfirmProcessWashing()
        {
            try
            {
                SaveDefectBeforeConfirm();

                /////////////////////////////////////////////////////////////////////////
                //Rate 관련 팝업
                //완료 처리 하기 전에 팝업 표시
                _bInputErpRate = false;
                _dtRet_Data.Clear();
                _sUserID = string.Empty;
                _sDepID = string.Empty;
                if (PERMIT_RATE_input(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString()))
                {
                    return;
                }
                ///////////////////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();
                string workUserId, workUserName, shift;

                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                if (drShift.Length > 0)
                {
                    shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                    workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                    workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                }
                else
                {
                    shift = string.Empty;           //작업조코드
                    workUserId = string.Empty;      //작업자 ID 
                    workUserName = string.Empty;    //작업자명
                }

                const string bizRuleName = "BR_PRD_REG_END_LOT_WS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WS();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["INPUT_QTY"] = 0;
                newRow["OUTPUT_QTY"] = 0;
                newRow["RESNQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = shift;
                newRow["WIPNOTE"] = null; //new TextRange(UcAssyProduction.TextRemark.Document.ContentStart, UcAssyProduction.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USER_NAME"] = workUserName;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
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
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
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
                Util.MessageException(ex);
            }
        }


        private void ConfirmProcessWashing_Rate()
        {
            try
            {
                //SaveDefectBeforeConfirm();
                ShowLoadingIndicator();

                string workUserId, workUserName, shift;

                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                if (drShift.Length > 0)
                {
                    shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                    workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                    workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                }
                else
                {
                    shift = string.Empty;           //작업조코드
                    workUserId = string.Empty;      //작업자 ID 
                    workUserName = string.Empty;    //작업자명
                }

                const string bizRuleName = "BR_PRD_REG_END_LOT_WS";

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WS();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["INPUT_QTY"] = 0;
                newRow["OUTPUT_QTY"] = 0;
                newRow["RESNQTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = shift;
                newRow["WIPNOTE"] = null; //new TextRange(UcAssyProduction.TextRemark.Document.ContentStart, UcAssyProduction.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USER_NAME"] = workUserName;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
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
                Util.MessageException(ex);
            }
        }

        private void SaveDefectBeforeConfirm()
        {
            string selectedLotId = _dvProductLot["LOTID"].GetString();
            string selectedWipSeq = _dvProductLot["WIPSEQ"].GetString();

            if (string.Equals(_processCode, Process.WINDING))
            {
                UcAssemblyProductionResult.SaveDefectBeforeConfirm();
                UcAssemblyProductionResult.SetProductionResultDetailGrid();
                //UcAssemblyProductionResult.GetDefectInfo(_dvProductLot); 
                UcAssemblyProductionResult.GetProductCellList(UcAssemblyProductionResult.dgProdCellWinding, false);
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                if (_previousValues.PreviousProdLotId == selectedLotId)
                {
                    if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
                    {
                        _previousValues.PreviousReInputQty = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "REINPUTQTY").GetDecimal();
                    }
                }
                else
                {
                    _previousValues.PreviousProdLotId = selectedLotId;
                    _previousValues.PreviousReInputQty = null;
                }

                UcAssemblyProductionResult.SaveDefectBeforeConfirm();
                /*
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                CalculateDefectQty();
                */
                UcAssemblyProductionResult.SetProductionResultDetailGrid();
                UcAssemblyProductionResult.GetDefectInfo(_dvProductLot);
                CalculateDefectQty();
            }
            else if (string.Equals(_processCode, Process.WASHING))
            {
                UcAssemblyProductionResult.SaveDefectBeforeConfirm();
                UcAssemblyProductionResult.SetProductionResultDetailGrid();
                UcAssemblyProductionResult.GetDefectInfo(_dvProductLot);
                CalculateDefectQty();
            }
        }



        public void CalculateDefectQty()
        {
            #region [Winding 실적상세 수량계산]
            if (string.Equals(_processCode, Process.WINDING))
            {
                // 양품수량
                double totalCellQty = 0;

                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgProdCellWinding))
                {
                    DataTable dt = ((DataView)UcAssemblyProductionResult.dgProdCellWinding.ItemsSource).Table;
                    totalCellQty = dt.AsEnumerable().Sum(s => s.Field<double>("CELLQTY"));
                }

                SumDefectQty();

                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
                {
                    if (UcAssemblyProductionResult.dgDefectDetail.Rows.Count - UcAssemblyProductionResult.dgDefectDetail.TopRows.Count > 0)
                    {
                        double defect = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                        double loss = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                        double chargeprd = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                        double defectQty = defect + loss + chargeprd;

                        double goodQty = (double)totalCellQty;
                        double outputQty = goodQty + defectQty;
                        double eqptQty = 0;
                        double inputQtyByType = GetInputQtyByApplyTypeCode().GetDouble();

                        // 와인더 공정진척의 경우 설비투입수량을 구할때 Product Lot의 EQPTQTY 값을 가져와서 계산함에 이에 EQPTQTY의 변경값이 있을 수 있어 재 조회 처리 함.
                        if (UcAssemblyProductionResult.IsSelectedAll)
                        {
                            string sFromMode = GetPilotProdMode();
                            UcAssemblyProductLot.SelectProductList(_dvProductLot["LOTID"].GetString(), sFromMode);
                        }

                        int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                        if (rowIndex > 0)
                        {
                            eqptQty = DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "EQPT_END_QTY").GetDouble();

                            string equipmentInputQty = DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "EQPT_END_QTY_M_EA").GetString();

                            if (!string.IsNullOrEmpty(equipmentInputQty))
                            {
                                string[] equipmentInputQtyString = equipmentInputQty.Split('/');


                                string firstText = equipmentInputQtyString[0].Trim();           //M
                                string lastText = (eqptQty + inputQtyByType).GetString();       //EA

                                DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "EQPTQTY_M_EA", firstText + "/" + lastText);
                            }
                            else
                            {
                                DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "EQPTQTY_M_EA", "0/0");
                            }

                            DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "EQPTQTY", (eqptQty + inputQtyByType).GetString());
                        }

                        //양품수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                        UcAssemblyProductionResult.textGoodQty.Value = goodQty;
                        //생산수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                        UcAssemblyProductionResult.textInputQty.Value = outputQty;
                        //투입수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", outputQty);
                        //추가불량
                        if (Math.Abs((eqptQty + inputQtyByType) - (defectQty + goodQty)) > 0)
                        {
                            UcAssemblyProductionResult.txtAssyResultQty.Text = ((eqptQty + inputQtyByType) - (defectQty + goodQty)).ToString("##,###");
                            UcAssemblyProductionResult.txtAssyResultQty.FontWeight = FontWeights.Bold;
                            UcAssemblyProductionResult.txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            UcAssemblyProductionResult.txtAssyResultQty.Text = "0";
                            UcAssemblyProductionResult.txtAssyResultQty.FontWeight = FontWeights.Normal;
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                            if (convertFromString != null)
                                UcAssemblyProductionResult.txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
            }
            #endregion

            #region  [Assembly 실적상세 수량계산]
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                double reInputlQty = 0;
                if (Math.Abs(GetReInputQty()) > 0) reInputlQty = GetReInputQty().GetDouble();

                SumDefectQty();

                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
                {
                    if (UcAssemblyProductionResult.dgDefectDetail.Rows.Count - UcAssemblyProductionResult.dgDefectDetail.TopRows.Count > 0)
                    {
                        if (
                                CommonVerify.HasDataGridRow(DgProductLot)
                                && _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") >= 0
                                && UcAssemblyProductionResult.tabReInput.Visibility == Visibility.Collapsed
                            )
                        {
                            if (_previousValues.PreviousReInputQty != null && Math.Abs(_previousValues.PreviousReInputQty.GetDecimal()) > 0)
                            {
                                reInputlQty = _previousValues.PreviousReInputQty.GetDouble();
                            }
                        }

                        double inputQty = GetInputQty().GetDouble();
                        double goodQty = GetGoodQty().GetDouble();
                        double boxQty = GetBoxQty().GetDouble();

                        double defect = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                        double loss = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                        double chargeprd = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();

                        double defectQty = defect + loss + chargeprd;
                        //double defectQty = Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")));

                        //투입수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", inputQty);
                        UcAssemblyProductionResult.textInputQty.Value = inputQty;

                        //양품수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                        UcAssemblyProductionResult.textGoodQty.Value = goodQty;
                        //재투입수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "REINPUTQTY", reInputlQty);
                        //박스수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "BOXQTY", boxQty);

                        //if (Math.Abs((inputQty + reInputlQty) - (goodQty + defectQty)) > 0)
                        if (Math.Abs((inputQty + reInputlQty) - (goodQty + defectQty + boxQty)) > 0)
                        {
                            UcAssemblyProductionResult.txtAssyResultQty.Text = ((inputQty + reInputlQty) - (goodQty + defectQty + boxQty)).ToString("##,###");
                            UcAssemblyProductionResult.txtAssyResultQty.FontWeight = FontWeights.Bold;
                            UcAssemblyProductionResult.txtAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            UcAssemblyProductionResult.txtAssyResultQty.Text = "0";
                            UcAssemblyProductionResult.txtAssyResultQty.FontWeight = FontWeights.Normal;
                            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                            if (convertFromString != null)
                                UcAssemblyProductionResult.txtAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
            }
            #endregion

            #region  [ZZS 실적상세 수량계산]
            else if (string.Equals(_processCode, Process.ZZS))
            {
                // 양품수량
                Decimal totalCellQty = 0;

                if (UcAssemblyProductionResult.dgOutProduct.ItemsSource != null)
                {
                    if (((DataView)UcAssemblyProductionResult.dgOutProduct.ItemsSource).Table.Rows.Count > 0)
                    {
                        DataTable dt = ((DataView)UcAssemblyProductionResult.dgOutProduct.ItemsSource).Table;
                        totalCellQty = dt.AsEnumerable().Sum(s => s.Field<Decimal>("WIPQTY"));
                    }
                }
                
                SumDefectQty();

                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
                {
                    if (UcAssemblyProductionResult.dgDefectDetail.Rows.Count - UcAssemblyProductionResult.dgDefectDetail.TopRows.Count > 0)
                    {
                        double defect = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                        double loss = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                        double chargeprd = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                        double defectQty = defect + loss + chargeprd;

                        double goodQty = (double)totalCellQty;
                        double eqptQty = GetEqptQtyQty().GetDouble();
                        double outputQty = goodQty + defectQty;

                        //양품수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                        UcAssemblyProductionResult.textGoodQty.Value = goodQty;
                        //생산수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                        UcAssemblyProductionResult.textInputQty.Value = outputQty;
                        //설비투입수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "EQPTQTY", eqptQty);
                    }
                }

            }
            #endregion
            #region  [Packsging 실적상세 수량계산]
            else if (string.Equals(_processCode, Process.PACKAGING))
            {
                // 양품수량
                Decimal totalCellQty = 0;

                if (UcAssemblyProductionResult.dgOutPKG.ItemsSource != null)
                {
                    if (((DataView)UcAssemblyProductionResult.dgOutPKG.ItemsSource).Table.Rows.Count > 0)
                    {
                        DataTable dt = ((DataView)UcAssemblyProductionResult.dgOutPKG.ItemsSource).Table;
                        totalCellQty = dt.AsEnumerable().Sum(s => s.Field<Decimal>("CELLQTY"));
                    }
                }
                
                SumDefectQty();

                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
                {
                    if (UcAssemblyProductionResult.dgDefectDetail.Rows.Count - UcAssemblyProductionResult.dgDefectDetail.TopRows.Count > 0)
                    {
                        double defect = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                        double loss = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                        double chargeprd = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                        double defectQty = defect + loss + chargeprd;

                        double goodQty = (double)totalCellQty;
                        double eqptQty = GetEqptQtyQty().GetDouble();
                        double outputQty = goodQty + defectQty;

                        //양품수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                        UcAssemblyProductionResult.textGoodQty.Value = goodQty;
                        //생산수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                        UcAssemblyProductionResult.textInputQty.Value = outputQty;
                        //설비투입수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "EQPTQTY", eqptQty);
                    }
                }

            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(_processCode, Process.ZTZ))
            {
               
                SumDefectQty();
                //GetTmpProdQty();

                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
                {
                    if (UcAssemblyProductionResult.dgDefectDetail.Rows.Count - UcAssemblyProductionResult.dgDefectDetail.TopRows.Count > 0)
                    {
                        double defect = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                        double loss = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                        double chargeprd = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                        double outputQty = Convert.ToDouble(UcAssemblyProductionResult.DvProductLot["INPUTQTY"]).GetDouble();
                        double tmpOutputQty = GetTmpProdQty(); 
                       

                        if (tmpOutputQty != 0)
                            outputQty = tmpOutputQty;


                        double defectQty = defect + loss + chargeprd;
                        double goodQty = outputQty - defectQty;
                        double eqptQty = GetEqptQtyQty().GetDouble();
 
                        //양품수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                        UcAssemblyProductionResult.textGoodQty.Value = goodQty;
                        //생산수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                        UcAssemblyProductionResult.textInputQty.Value = outputQty;
                        //설비투입수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "EQPTQTY", eqptQty);
                    }
                }

            }
            #endregion
            #region  [Washing 실적상세 수량계산]
            else
            {
                double totalCellQty = 0;

                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgOut))
                {
                    DataTable dt = ((DataView)UcAssemblyProductionResult.dgOut.ItemsSource).Table;
                    totalCellQty = dt.AsEnumerable().Sum(s => s.Field<double>("CELLQTY"));
                }
                SumDefectQty();
                if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
                {
                    if (UcAssemblyProductionResult.dgDefectDetail.Rows.Count - UcAssemblyProductionResult.dgDefectDetail.TopRows.Count > 0)
                    {
                        double defect = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                        double loss = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                        double chargeprd = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                        double defectQty = defect + loss + chargeprd;

                        double goodQty = totalCellQty;
                        double boxQty = GetBoxQty().GetDouble();
                        double outputQty = goodQty + defectQty + boxQty;

                        //양품수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                        UcAssemblyProductionResult.textGoodQty.Value = goodQty;
                        //생산수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                        UcAssemblyProductionResult.textInputQty.Value = outputQty;
                        //투입수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", outputQty);
                        //BOX수량
                        DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "BOXQTY", boxQty);
                    }
                }
            }
            #endregion

        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtTmp = new DataTable();
                
                if (string.Equals(_processCode, Process.ZZS) || string.Equals(_processCode, Process.PACKAGING))
                {
                    dtTmp = DataTableConverter.Convert(UcAssemblyProductionResult.dgDefectZZS.ItemsSource);
                }
                //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
                else if (string.Equals(_processCode, Process.ZTZ))
                {
                    dtTmp = DataTableConverter.Convert(UcAssemblyProductionResult.dgDefectZtz.ItemsSource);
                }
                else
                {
                    dtTmp = DataTableConverter.Convert(UcAssemblyProductionResult.dgDefect.ItemsSource);
                }

                if (CommonVerify.HasTableRow(dtTmp))
                {

                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString()));
                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").GetString()));
                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString()));
                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString()));
                }
                else
                {
                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", 0);
                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                    DataTableConverter.SetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SaveWipHistory()
        {
            ShowLoadingIndicator();

            string bizRuleName;

            if (string.Equals(_processCode, Process.WINDING))
                bizRuleName = "BR_ACT_REG_SAVE_LOT";
            else if (string.Equals(_processCode, Process.ASSEMBLY))
                bizRuleName = "BR_PRD_REG_SAVE_LOT_AS";
            else
                bizRuleName = "BR_ACT_REG_SAVE_LOT";

            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));

            if (string.Equals(_processCode, Process.ASSEMBLY))
                inDataTable.Columns.Add("INPUT_DIFF_QTY", typeof(decimal));

            string workUserId, workUserName, shift;
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
            if (drShift.Length > 0)
            {
                shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                workUserName = drShift[0]["VAL002"].GetString();    //작업자명
            }
            else
            {
                shift = string.Empty;           //작업조코드
                workUserId = string.Empty;      //작업자 ID 
                workUserName = string.Empty;    //작업자명
            }

            string prodVersion;

            if (string.Equals(_processCode, Process.WINDING))
                prodVersion = null;
            else
            {
                prodVersion = UcAssemblyProductionResult.txtProdVerCode.Text;
            }

            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = _dvProductLot["LOTID"].GetString(); //_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            dr["PROD_VER_CODE"] = prodVersion;
            dr["SHIFT"] = shift;
            dr["WIPNOTE"] = null; //new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
            dr["WRK_USERID"] = workUserId;
            dr["WRK_USER_NAME"] = workUserName;
            dr["PROD_QTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
            dr["USERID"] = LoginInfo.USERID;
            if (string.Equals(_processCode, Process.ASSEMBLY))
                dr["INPUT_DIFF_QTY"] = DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "REINPUTQTY").GetDecimal();

            inDataTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    UcAssemblyProductionResult.SetProductionResultDetailGrid();
                    UcAssemblyProductionResult.GetDefectInfo(_dvProductLot);
                    if (!string.Equals(_processCode, Process.WINDING))
                        CalculateDefectQty();

                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    //GetProductLot();
                    //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            });
        }

        private double GetInputQtyByApplyTypeCode()
        {
            double qty = 0;

            if (CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefect))
            {
                DataTable dt = ((DataView)UcAssemblyProductionResult.dgDefect.ItemsSource).Table;
                double qtyPlus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "PLUS").Sum(s => s.Field<double>("RESNQTY"));
                double qtyMinus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "MINUS").Sum(s => s.Field<double>("RESNQTY"));

                return qtyPlus - qtyMinus;
            }
            return qty;
        }

        private decimal GetInputQty()
        {
            decimal returnInputQty;

            if (!CommonVerify.HasDataGridRow(DgProductLot) || _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            string bizRuleName = string.Equals(_processCode, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_HALFPROD_AS" : "DA_PRD_SEL_INPUT_HALFPROD_WS";
            const string materialType = "PROD";

            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LANGID", typeof(string));
            indataTable.Columns.Add("LOTID", typeof(string));
            indataTable.Columns.Add("MTRLTYPE", typeof(string));
            indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            indataTable.Columns.Add("EQPTID", typeof(string));
            indataTable.Columns.Add("PROD_LOTID", typeof(string));
            DataRow dr = indataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = string.Empty;
            dr["MTRLTYPE"] = materialType;
            dr["EQPT_MOUNT_PSTN_ID"] = null;
            dr["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
            dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK",true).Field<string>("LOTID").GetString();
            indataTable.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);

            if (CommonVerify.HasTableRow(dt))
            {
                returnInputQty = Convert.ToDecimal(dt.AsEnumerable().Sum(s => s.Field<double>("INPUT_QTY")));
            }
            else
            {
                returnInputQty = 0;
            }
            return returnInputQty;
        }

        private decimal GetReInputQty()
        {
            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            if (UcAssemblyProductionResult.tabReInput.Visibility == Visibility.Visible)
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_FOR_REINPUT";
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();
                inTable.TableName = "INDATA";

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    return CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]) ? dsResult.Tables["OUTDATA"].AsEnumerable().Sum(s => s.Field<decimal>("RESNQTY")) : 0;
                }
                return 0;
            }
            else
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_DIFF_QTY";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(Int32));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK", true).Field<string>("LOTID").GetString();
                dr["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK", true).Field<Int64>("WIPSEQ").GetInt();
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                return CommonVerify.HasTableRow(searchResult) ? searchResult.Rows[0]["INPUT_DIFF_QTY"].GetDecimal() : 0;
            }
        }

        private double GetGoodQty()
        {
            double returnGoodQty;

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            const string bizRuleName = "DA_PRD_SEL_WASHING_LOT_RSLT";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK",true).Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            returnGoodQty = CommonVerify.HasTableRow(searchResult) ? searchResult.AsEnumerable().Sum(s => s.Field<double>("WIPQTY_INPUT")) : 0;
            return returnGoodQty;
        }

        private decimal GetBoxQty()
        {
            decimal returnBoxQty;

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            const string bizRuleName = "DA_PRD_SEL_WAIT_BOX_LIST_WS";
            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = indataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK",true).Field<string>("LOTID").GetString();
            indataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", indataTable);
            if (CommonVerify.HasTableRow(searchResult))
            {
                returnBoxQty = searchResult.AsEnumerable().Sum(s => s.Field<Decimal>("WIPQTY"));
            }
            else
            {
                returnBoxQty = 0;
            }
            return returnBoxQty;
        }

        private decimal GetEqptQtyQty()
        {
            decimal returnEqptQty;

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            const string bizRuleName = "DA_BAS_SEL_WIPATTR";
            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = indataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            indataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", indataTable);

            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            //returnEqptQty = Util.NVC_Decimal(searchResult.Rows[0]["EQPT_INPUT_QTY"]);
            if (string.Equals(_processCode, Process.ZTZ))
            {
                returnEqptQty = Util.NVC_Decimal(searchResult.Rows[0]["EQPT_END_QTY"]);
            }
            else
            {
                returnEqptQty = Util.NVC_Decimal(searchResult.Rows[0]["EQPT_INPUT_QTY"]);
            }


            return returnEqptQty;
        }

        private string GetConfirmDate()
        {
            string confirmDate;

            const string bizRuleName = "DA_PRD_SEL_CONFIRM_LOT_INFO";
            string prodLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK", true).Field<string>("LOTID").GetString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = prodLotId;
            dr["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).Trim()))
            {
                confirmDate = Util.NVC(searchResult.Rows[0]["NOW_CALDATE"]).GetString();
            }
            else
            {
                confirmDate = Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).GetString();
            }

            return confirmDate;
        }

        private bool PrintLabel(string zplCode, DataRow drPrtInfo)
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
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zplCode);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(zplCode, _equipmentCode);
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
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zplCode);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zplCode);
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

        private bool CanInputComplete()
        {
            bool bRet = true;

            // 실적확정 대상 Lot 에 물린 바구니 확인
            if (winInput != null)
            {
                try
                {
                    ShowLoadingIndicator();

                    DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["EQPTID"] = Util.NVC(_equipmentCode); ;
                    inTable.Rows.Add(newRow);

                    DataTable dtCurrIn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_IN_LOT_LIST", "INDATA", "OUTDATA", inTable);
                    if (dtCurrIn.Rows != null && dtCurrIn.Rows.Count > 0)
                    {
                        string prodLotID = _dvProductLot["LOTID"].GetString();
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
                                    Parameters[0] = _equipmentCode;
                                    Parameters[1] = Process.PACKAGING;
                                    Parameters[2] = _dvProductLot["LOTID"].GetString(); 
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

        private void wndInputObject_Closed(object sender, EventArgs e)
        {
            wndInputObject = null;
            ASSY003_007_INPUT_OBJECT window = sender as ASSY003_007_INPUT_OBJECT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                try
                {
                    ShowLoadingIndicator();

                    DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                    DataRow newRow = inTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["EQPTID"] = Util.NVC(_equipmentCode); ;
                    inTable.Rows.Add(newRow);

                    DataTable dtCurrIn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_IN_LOT_LIST", "INDATA", "OUTDATA", inTable);
                    if (dtCurrIn.Rows != null && dtCurrIn.Rows.Count > 0)
                    {
                        string prodLotID = _dvProductLot["LOTID"].GetString();
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

                    ConfirmProcessPKG();
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

        private void ConfirmProcessPKG()
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

                        string workUserId, workUserName, shift;
                        string shiftStartTime = string.Empty;
                        string shiftEndTime = string.Empty;
                        DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                        if (drShift.Length > 0)
                        {
                            shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                            workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                            workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                        }
                        else
                        {
                            shift = string.Empty;           //작업조코드
                            workUserId = string.Empty;      //작업자 ID 
                            workUserName = string.Empty;    //작업자명
                        }

                        if (wndConfirm != null)
                        {
                            object[] Parameters = new object[11];
                            Parameters[0] = _equipmentSegmentCode;
                            Parameters[1] = _equipmentCode;
                            Parameters[2] = _dvProductLot["LOTID"].GetString();
                            Parameters[3] = _dvProductLot["WIPSEQ"].GetString();
                            Parameters[4] = _dvProductLot["WIPSTAT"].GetString();

                            Parameters[5] = shift; //= Util.NVC(wndPopup.SHIFTNAME);
                            Parameters[6] = shift; //= Util.NVC(wndPopup.SHIFTCODE);
                            Parameters[7] = workUserName; //= Util.NVC(wndPopup.USERNAME);
                            Parameters[8] = workUserId; //= Util.NVC(wndPopup.USERID);                            
                            Parameters[9] = shiftStartTime; //= Util.NVC(wndPopup.WRKSTRTTIME);
                            Parameters[10] = shiftEndTime; //= Util.NVC(wndPopup.WRKENDTTIME);

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

                string workUserId, workUserName, shift;
                string shiftStartTime = string.Empty;
                string shiftEndTime = string.Empty;

                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                if (drShift.Length > 0)
                {
                    shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                    workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                    workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                }
                else
                {
                    shift = string.Empty;           //작업조코드
                    workUserId = string.Empty;      //작업자 ID 
                    workUserName = string.Empty;    //작업자명
                }

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[11];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    Parameters[2] = _dvProductLot["LOTID"].GetString();
                    Parameters[3] = _dvProductLot["WIPSEQ"].GetString();
                    Parameters[4] = _dvProductLot["WIPSTAT"].GetString();

                    Parameters[5] = shift; //= Util.NVC(wndPopup.SHIFTNAME);
                    Parameters[6] = shift; //= Util.NVC(wndPopup.SHIFTCODE);
                    Parameters[7] = workUserName; //= Util.NVC(wndPopup.USERNAME);
                    Parameters[8] = workUserId; //= Util.NVC(wndPopup.USERID);                               
                    Parameters[9] = shiftStartTime; //= Util.NVC(wndPopup.WRKSTRTTIME);
                    Parameters[10] = shiftEndTime; //= Util.NVC(wndPopup.WRKENDTTIME);

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                }
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
                SetProductLotList();
            }

            //GetProductLot();
            //GetEqptWrkInfo();
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
                newRow["EQSGID"] = Util.NVC(_equipmentSegmentCode);
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROCID"] = Process.PACKAGING;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LAST_EQP_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("PRJT_NAME"))
                    {

                        string productProjectName = _dvProductLot["PRJT_NAME"].GetString(); 
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

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_EQPT_CLCT_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();

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

        private void Confirm_ProcessZZS()
        {
            try
            {
                if (CheckModelChange() && !CheckInputEqptCond())
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU2817", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (wndConfirmZZS != null)
                                wndConfirmZZS = null;

                            wndConfirmZZS = new ASSY003_023_CONFIRM();
                            wndConfirmZZS.FrameOperation = FrameOperation;

                            string workUserId, workUserName, shift;
                            string shiftStartTime = string.Empty;
                            string shiftEndTime = string.Empty;

                            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                            if (drShift.Length > 0)
                            {
                                shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                                workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                                workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                            }
                            else
                            {
                                shift = string.Empty;           //작업조코드
                                workUserId = string.Empty;      //작업자 ID 
                                workUserName = string.Empty;    //작업자명
                            }

                            if (wndConfirmZZS != null)
                            {
                                object[] Parameters = new object[11];
                                Parameters[0] = _equipmentSegmentCode;
                                Parameters[1] = _equipmentCode;
                                Parameters[2] = _dvProductLot["LOTID"].GetString();
                                Parameters[3] = _dvProductLot["WIPSEQ"].GetString();
                                Parameters[4] = _dvProductLot["WIPSTAT"].GetString();

                                Parameters[5] = shift; //= Util.NVC(wndPopup.SHIFTNAME);
                                Parameters[6] = shift; //= Util.NVC(wndPopup.SHIFTCODE);
                                Parameters[7] = workUserName; //= Util.NVC(wndPopup.USERNAME);
                                Parameters[8] = workUserId; //= Util.NVC(wndPopup.USERID);                               
                                Parameters[9] = shiftStartTime; //= Util.NVC(wndPopup.WRKSTRTTIME);
                                Parameters[10] = shiftEndTime; //= Util.NVC(wndPopup.WRKENDTTIME);
                                C1WindowExtension.SetParameters(wndConfirmZZS, Parameters);

                                wndConfirmZZS.Closed += new EventHandler(wndConfirmZZS_Closed);
                                this.Dispatcher.BeginInvoke(new Action(() => wndConfirmZZS.ShowModal()));
                            }
                        }
                    });
                }
                else
                {
                    if (wndConfirmZZS != null)
                        wndConfirmZZS = null;

                    string workUserId, workUserName, shift;
                    string shiftStartTime = string.Empty;
                    string shiftEndTime = string.Empty;

                    DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                    if (drShift.Length > 0)
                    {
                        shift = drShift[0]["SHFT_ID"].GetString();          //작업조코드
                        workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                        workUserName = drShift[0]["VAL002"].GetString();    //작업자명
                    }
                    else
                    {
                        shift = string.Empty;           //작업조코드
                        workUserId = string.Empty;      //작업자 ID 
                        workUserName = string.Empty;    //작업자명
                    }

                    wndConfirmZZS = new ASSY003_023_CONFIRM();
                    wndConfirmZZS.FrameOperation = FrameOperation;

                    if (wndConfirmZZS != null)
                    {
                        object[] Parameters = new object[11];
                        Parameters[0] = _equipmentSegmentCode;
                        Parameters[1] = _equipmentCode;
                        Parameters[2] = _dvProductLot["LOTID"].GetString();
                        Parameters[3] = _dvProductLot["WIPSEQ"].GetString();
                        Parameters[4] = _dvProductLot["WIPSTAT"].GetString();

                        Parameters[5] = shift; //= Util.NVC(wndPopup.SHIFTNAME);
                        Parameters[6] = shift; //= Util.NVC(wndPopup.SHIFTCODE);
                        Parameters[7] = workUserName; //= Util.NVC(wndPopup.USERNAME);
                        Parameters[8] = workUserId; //= Util.NVC(wndPopup.USERID);                               
                        Parameters[9] = shiftStartTime; //= Util.NVC(wndPopup.WRKSTRTTIME);
                        Parameters[10] = shiftEndTime; //= Util.NVC(wndPopup.WRKENDTTIME);
                        C1WindowExtension.SetParameters(wndConfirmZZS, Parameters);
                        C1WindowExtension.SetParameters(wndConfirmZZS, Parameters);

                        wndConfirmZZS.Closed += new EventHandler(wndConfirmZZS_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => wndConfirmZZS.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndConfirmZZS_Closed(object sender, EventArgs e)
        {
            wndConfirm = null;
            ASSY003_023_CONFIRM window = sender as ASSY003_023_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot();
                SetProductLotList();
            }

            //GetEqptWrkInfo();
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

        #region[[Validation]

        private bool ValidationWaitLot()
        {
            /*
            if (string.IsNullOrEmpty(_processCode))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }
            */
            return true;
        }

        private bool ValidationWindingLot()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }
            return true;
        }

        private bool ValidationEqptCondSearch()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationEqptCond()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationWipNote()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }
            return true;
        }

        private bool ValidationHistoryCard()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }
            return true;
        }

        private bool ValidationRunStart()
        {
            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (!string.Equals(_processCode, Process.WASHING))
            {
                DataRow[] drWorkOrder = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '1'");

                if (drWorkOrder.Length == 0 || string.IsNullOrWhiteSpace(drWorkOrder[0]["VAL002"].ToString()))
                {
                    Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                    return false;
                }

                if (!CheckSelectWorkOrderInfo())
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidationRunCancel()
        {
            /*
            int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            string parentLot = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string childSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int cut = 0;

            if (!int.TryParse(childSeq, out cut))
            {
                //Util.Alert("CUT이 숫자가 아닙니다.");
                //return bRet;
            }

            // 최종 작업 lot 부터 취소 가능.
            for (int i = 0; i < UcAssemblyProductLot.dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(parentLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) > cut)
                        {
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "LOTID")));
                            return false;
                        }
                    }
                }
            }

            // Max CUT DB 확인.
            string maxSeq = string.Empty;
            int tempMinSeq = 0;

            maxSeq = GetMaxChildGRPSeq(parentLot);
            int.TryParse(maxSeq, out tempMinSeq);

            if (tempMinSeq > 0 && tempMinSeq > cut)
            {
                //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.");
                Util.MessageValidation("SFU1790");
                return false;
            }
            */
            return true;
        }

        private bool ValidationEquipmentEnd()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_dvProductLot["WIPSTAT"].GetString() != "PROC")
            {
                //장비완료 할 수 있는 LOT상태가 아닙니다.
                Util.MessageValidation("SFU1866");
                return false;
            }

            return true;
        }

        private bool ValidationEquipmentEndCancel()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_dvProductLot["WIPSTAT"].GetString() != "EQPT_END")
            {
                Util.MessageValidation("SFU1864");  // 장비완료 상태의 LOT이 아닙니다.
                return false;
            }

            return true;
        }

        private bool ValidationCancelConfirm()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationCancelTermSepa()
        {

            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationEditEquipmentQty()
        {
            if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationQualityInput()
        {
            //if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
            {
                //"선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            return true;
        }

        private bool ValidationCancelRun()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            if (string.Equals(_processCode, Process.ASSEMBLY) || string.Equals(_processCode, Process.WASHING))
            {
                // 투입 이력 정보 존재여부 확인            
                if (CheckInputHistoryInfo(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString()))
                {
                    Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
                    return false;
                }

                if (string.Equals(_processCode, Process.WASHING))
                {
                    // 완성 이력 정보 존재여부 확인
                    if (CheckOutTrayInfo(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString()))
                    {
                        Util.MessageValidation("SFU3438");   // 생산Tray가 존재하여 취소할 수 없습니다.
                        return false;
                    }
                }

            }

            if (string.Equals(_processCode, Process.ZZS))
            {
                // 투입 이력 정보 존재여부 확인            
                if (ChkInputHistCnt(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString()))
                {
                    Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
                    return false;
                }

                // 완성 이력 정보 존재여부 확인
                if (ChkOutTrayCnt(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString()))
                {
                    Util.MessageValidation("SFU3438");   // 생산Tray가 존재하여 취소할 수 없습니다.
                    return false;
                }
            }

                return true;
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
                newRow["EQPTID"] = _equipmentCode;
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
                newRow["PROCID"] = Process.ZZS;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = _equipmentCode;
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

        private bool ValidationConfirm()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID").GetString()))
            {
                //실적확정 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1717");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return false;
            }

            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (drShift.Length < 1 || string.IsNullOrEmpty(drShift[0]["VAL001"].GetString()))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(drShift[0]["VAL002"].GetString()))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            //추가불량 오류 : 추가불량 수량을 재 확인하신 후 반영(저장)해 주세요.
            if (string.Equals(_processCode, Process.WINDING))
            {
                if (Math.Abs(UcAssemblyProductionResult.txtAssyResultQty.Text.GetDecimal()) > 0)
                {
                    //추가불량 오류 : 추가불량 수량을 재 확인하신 후 반영(저장)해 주세요.
                    Util.MessageValidation("SFU3665");
                    return false;
                }
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {

                //if (DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetString() == "0")
                //{
                //    //양품 수량을 확인하십시오.
                //    Util.MessageValidation("SFU1722");
                //    return false;
                //}

                // 실적확정 이전에 양품수량 합계의 변동사항이 있는지 체크한다
                if (Math.Abs(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble() - GetGoodQty().GetDouble()) > 0)
                {
                    //양품수량 변경내역이 존재 합니다. 재 조회 하신 후 실적확정 하세요.
                    Util.MessageValidation("SFU4111");
                    return false;
                }

                if (Math.Abs(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "BOXQTY").GetDouble() - GetBoxQty().GetDouble()) > 0)
                {
                    //BOX수량 변경내역이 존재 합니다. 재 조회 하신 후 실적확정 하세요.
                    Util.MessageValidation("SFU4112");
                    return false;
                }

                if (Math.Abs(UcAssemblyProductionResult.txtAssyResultQty.Text.GetDecimal()) > 0)
                {
                    //차이수량이 존재하여 실적 확정이 불가 합니다.\r\n생산실적을 재 확인 해주세요.
                    Util.MessageValidation("SFU3701");
                    return false;
                }

            }
            else if (string.Equals(_processCode, Process.PACKAGING))
            {
                if (ChkConfirmOutTray())
                {
                    Util.MessageValidation("SFU1250");  //확정되지 않은 Tray를 확정 해 주세요.
                    return false;
                }

                if (ChkInLotMustComplete() == false)
                {
                    Util.MessageValidation("SFU6022");  //투입LOT을 투입완료하거나 잔량처리 해주세요.
                    return false;
                }
            }
            else if (string.Equals(_processCode, Process.ZZS))
            {
                if (!CanConfirmWithOutPrint())
                {
                    return false;
                }

                if (ChkInLotMustCompleteZZS() == false)
                {
                    Util.MessageValidation("SFU6022");  //투입LOT을 투입완료하거나 잔량처리 해주세요.
                    return false;
                }

            }
            else
            {
                //if (DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[UcAssemblyProductionResult.dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetString() == "0")
                //{
                //    //양품 수량을 확인하십시오.
                //    Util.MessageValidation("SFU1722");
                //    return false;
                //}
            }

            return true;
        }

        private bool ChkInLotMustCompleteZZS()
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
            newRow["PROCID"] = Process.ZZS;
            newRow["EQPTID"] = _equipmentCode;

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

        private bool CanConfirmWithOutPrint()
        {
            bool bRet = false;

            // 생산 반제품이 존재하는 경우에만 실적확정 가능 하도록 수정.
            DataTable dt = DataTableConverter.Convert(UcAssemblyProductionResult.dgOutProduct.ItemsSource);
            DataRow[] dr1 = dt.Select("WIPSTAT <> 'PROC'");
            if (dr1.Length < 1)
            {
                // 생산된 매거진이 존재하지 않아 확정할 수 없습니다.\n\r매거진 생성 후 확정 하시기 바랍니다.
                Util.MessageValidation("SFU3516");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool ChkConfirmOutTray()
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["EQPTID"] = _equipmentCode;
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
            newRow["EQPTID"] = _equipmentCode;

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

        private bool ValidationInspectionLot()
        {
            const string bizRuleName = "DA_QCA_CHK_MAND_INSP_ITEM_RESULT_LOT";

            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PROCID"] = _processCode;
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK", true).Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
            {
                Util.MessageInfo("SFU3669", dtResult.Rows[0]["CLCTNAME"].GetString());
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ValidationInspectionTime()
        {

            const string bizRuleName = "BR_QCA_CHK_MAND_INSP_ITEM_RESULT_TIME";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PROCID"] = _processCode;
            dr["EQPTID"] = _equipmentCode; //UcAssemblyProductLot.txtSelectEquipment.Text;
            dr["LOTID"] = _dvProductLot["LOTID"].GetString(); //_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
            {
                String stnMsg = dtResult.Rows[0]["CLCTNAME1"].GetString();
                if (string.IsNullOrEmpty(dtResult.Rows[0]["CLCTNAME2"].GetString()) == false)
                {
                    stnMsg = stnMsg + "-" + dtResult.Rows[0]["CLCTNAME2"].GetString();
                }
                if (string.IsNullOrEmpty(dtResult.Rows[0]["CLCTNAME3"].GetString()) == false)
                {
                    stnMsg = stnMsg + "-" + dtResult.Rows[0]["CLCTNAME3"].GetString();
                }
                Util.MessageInfo("SFU5079", stnMsg); //Time 단위 필수 검사 항목 입력 필요 : [%1]
                return false;
            }
            else
            {
                //return string.Empty;
                return true;
            }
        }

        private bool ValidationSaveWipHistory()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(UcAssemblyProductionResult.dgDefectDetail))
            {
                Util.MessageValidation("SFU3552");
                return false;
            }

            return true;
        }

        private bool ValidationEquipmentIssue()
        {
            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }
            return true;
        }

        private bool ValidationTrayMove()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationPilotProdMode()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //if (_processCode == Process.STACKING_FOLDING)
            //{
                string processLotId;
                if (CheckProcessWip(out processLotId))
                {
                    Util.MessageValidation("SFU3199", processLotId); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
                    return false;
                }
            //}

            return true;
        }

        private bool CheckProcessWip(out string processLotId)
        {
            processLotId = string.Empty;

            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                dtRow["WIPSTAT"] = Wip_State.PROC;
                inTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    processLotId = Util.NVC(dtResult.Rows[0]["LOTID"]);
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

        private bool CheckEquipmentConfirmType(string buttonGroupId)
        {
            try
            {
                if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
                {
                    //선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("RSLT_CNFM_TYPE") && Util.NVC(dtResult.Rows[0]["RSLT_CNFM_TYPE"]).Equals("A"))
                    {
                        if (!CheckButtonPermissionGroupByBtnGroupId(buttonGroupId))  // 버튼 권한 체크.
                        {
                            //해당 설비는 자동실적확정 설비 입니다.
                            Util.MessageValidation("SFU6034");
                            return false;
                        }
                    }
                    else
                    {
                        if (!CheckButtonPermissionGroupByBtnGroupId(buttonGroupId))  // 버튼 권한 체크.
                        {
                            Util.MessageValidation("SFU3520", LoginInfo.USERID, buttonGroupId.Equals("EQPT_END_W") ? ObjectDic.Instance.GetObjectName("장비완료") : ObjectDic.Instance.GetObjectName("실적확정")); // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckButtonPermissionGroupByBtnGroupId(string buttonGroupId)
        {
            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = _processCode;
                dtRow["EQGRID"] = _processCode == Process.STACKING_FOLDING ? "STK" : null;  // STACKING, FOLDING 공정진척의 경우 동일한 PROCID를 사용함으로, EQGRID 로 분기처리가 필요 함.
                inTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    DataRow[] drs = dtResult.Select("BTN_PMS_GRP_CODE = '" + buttonGroupId + "'");
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

        private bool IsCurrentProcessByLotId()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow dtRow = inTable.NewRow();
            dtRow["LOTID"] = _dvProductLot["LOTID"].GetString();
            inTable.Rows.Add(dtRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["PROCID"].GetString() == _dvProductLot["PROCID"].ToString())
                {
                    return _dvProductLot["PROCID"].ToString() == dtResult.Rows[0]["PROCID"].ToString();
                }
            }
            return false;
        }

        private bool CheckInputHistoryInfo(string lotId, string wipSeq)
        {
            bool bRet = false;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST_CNT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt;
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

        }

        private bool CheckOutTrayInfo(string lotid, string wipSeq)
        {
            bool bRet = false;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_CNT_WS";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROD_LOTID"] = lotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt;
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
        }

        private async Task<bool> WaitCallback()
        {
            bool succeeded = false;
            while (!succeeded)
            {
                if (UcAssemblyProductLot.IsSearchResult) succeeded = true;
                await System.Threading.Tasks.Task.Delay(500);
            }

            return true;
        }

        #endregion

        #region [팝업]

        private void PopupEqptCondSearch()
        {

            CMM_ASSY_EQPT_COND_SEARCH popEqptCondSearch = new CMM_ASSY_EQPT_COND_SEARCH();
            popEqptCondSearch.FrameOperation = FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = _processCode;
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[3] = _equipmentSegmentName;
            parameters[4] = UcAssemblyProductLot.txtSelectEquipmentName.Text;

            C1WindowExtension.SetParameters(popEqptCondSearch, parameters);

            popEqptCondSearch.Closed += popEqptCondSearch_Closed;
            Dispatcher.BeginInvoke(new Action(() => popEqptCondSearch.ShowModal()));

        }

        private void popEqptCondSearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND_SEARCH popup = sender as CMM_ASSY_EQPT_COND_SEARCH;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupEqptCond()
        {
            CMM_ASSY_PU_EQPT_COND popupEqptCond = new CMM_ASSY_PU_EQPT_COND { FrameOperation = FrameOperation };
            object[] parameters = new object[6];
            parameters[0] = _equipmentSegmentCode; ;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ"));
            parameters[5] = UcAssemblyProductLot.txtSelectEquipmentName.Text;
            C1WindowExtension.SetParameters(popupEqptCond, parameters);

            popupEqptCond.Closed += popupEqptCond_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupEqptCond.ShowModal()));
        }

        private void popupEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PU_EQPT_COND popup = sender as CMM_ASSY_PU_EQPT_COND;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupWipNote()
        {
            CMM_COM_WIP_NOTE popWipNote = new CMM_COM_WIP_NOTE();
            popWipNote.FrameOperation = FrameOperation;

            object[] parameters = new object[3];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            C1WindowExtension.SetParameters(popWipNote, parameters);

            popWipNote.Closed += popWipNote_Closed;
            Dispatcher.BeginInvoke(new Action(() => popWipNote.ShowModal()));
        }

        private void popWipNote_Closed(object sender, EventArgs e)
        {
            CMM_COM_WIP_NOTE popup = sender as CMM_COM_WIP_NOTE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupRunStart()
        {
            if (string.Equals(_processCode, Process.WINDING))
            {
                ASSY006_WN_RUNSTART popupRunStart = new ASSY006_WN_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[6];
                parameters[0] = _processCode;
                parameters[1] = _equipmentSegmentCode;
                parameters[2] = _equipmentCode;
                parameters[3] = _equipmentName;
                parameters[4] = string.Empty;
                // Set Work Order Parameter
                parameters[5] = SelectWorkOrderInfo();
                C1WindowExtension.SetParameters(popupRunStart, parameters);

                popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                ASSY006_AS_RUNSTART popupRunStart = new ASSY006_AS_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[6];
                parameters[0] = _processCode;
                parameters[1] = _equipmentSegmentCode;
                parameters[2] = _equipmentCode;
                parameters[3] = _equipmentName;
                parameters[4] = string.Empty;
                // Set Work Order Parameter
                parameters[5] = SelectWorkOrderInfo();
                C1WindowExtension.SetParameters(popupRunStart, parameters);

                popupRunStart.Closed += popupRunStart_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            }
            else if (string.Equals(_processCode, Process.ZZS))
            {
                ASSY003_023_RUNSTART wndRunStart = new ASSY003_023_RUNSTART();
                wndRunStart.FrameOperation = FrameOperation;

                if (wndRunStart != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    C1WindowExtension.SetParameters(wndRunStart, Parameters);

                    wndRunStart.Closed += new EventHandler(popupRunStart_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                }
            }
            else if (string.Equals(_processCode, Process.PACKAGING))
            {
                if (wndRunStart != null)
                    wndRunStart = null;

                wndRunStart = new ASSY003_007_RUNSTART();
                wndRunStart.FrameOperation = FrameOperation;

                if (wndRunStart != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    Parameters[2] = Process.PACKAGING;
                    C1WindowExtension.SetParameters(wndRunStart, Parameters);

                    wndRunStart.Closed += new EventHandler(popupRunStart_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                }
            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(_processCode, Process.ZTZ))
            {
                string equipmentName = UcAssemblyProductLot.txtSelectEquipmentName.Text;

                if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipmentName.Text))
                {
                    equipmentName = DgEquipment?.CurrentCell?.Row?.DataItem?.GetString();

                    if (!string.IsNullOrEmpty(equipmentName))
                    {
                        equipmentName = equipmentName.Replace("System.Data.DataRowView", string.Empty);

                        if (equipmentName.Split(':').Length == 2)
                        {
                            equipmentName = equipmentName.Split(':')[1].Trim();
                        }
                    }
                }

                int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");
                
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
                {
                    Util.MessageValidation("SFU1220");
                    return;
                }

                ASSY006_ZTZ_RUNSTART popupRunStart = new ASSY006_ZTZ_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[10];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _processCode;
                parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PR_LOTID"));//_dvProductLot["PR_LOTID"].ToString();
                parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "CSTID"));//_dvProductLot["PR_CSTID"].ToString();
                //parameters[5] = _equipmentMountPositionCode;
                //parameters[6] = _unldrLotIdentBasCode;
                parameters[5] = null;
                parameters[6] = null;
                parameters[7] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PRODID"));//_dvProductLot["MO_PRODID"].ToString();
                parameters[8] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPQTY_M_EA")); //_dvProductLot["WIPQTY_M_EA"].ToString();
                parameters[9] = equipmentName;// UcAssemblyProductLot.txtSelectEquipmentName.Text;
                C1WindowExtension.SetParameters(popupRunStart, parameters);

                popupRunStart.Closed += popupRunStart_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));

                UcAssemblyProductLot.chkWait.IsChecked = false;
            }
            else
            {
                ASSY006_WS_RUNSTART popupRunStart = new ASSY006_WS_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[6];
                parameters[0] = _processCode;
                parameters[1] = _equipmentSegmentCode;
                parameters[2] = _equipmentCode;
                parameters[3] = _equipmentName;
                parameters[4] = string.Empty;
                parameters[5] = null;
                C1WindowExtension.SetParameters(popupRunStart, parameters);
                popupRunStart.Closed += popupRunStart_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            }
        }
        
        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            if (string.Equals(_processCode, Process.WINDING))
            {
                ASSY006_WN_RUNSTART pop = sender as ASSY006_WN_RUNSTART;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();

                    //SetProductLotList();
                    // 생산 Lot 재조회
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                ASSY006_AS_RUNSTART popup = sender as ASSY006_AS_RUNSTART;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
            else if (string.Equals(_processCode, Process.ZZS))
            {
                ASSY003_023_RUNSTART window = sender as ASSY003_023_RUNSTART;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
            else if (string.Equals(_processCode, Process.PACKAGING))
            {
                wndRunStart = null;
                ASSY003_007_RUNSTART window = sender as ASSY003_007_RUNSTART;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(_processCode, Process.ZTZ))
            {
                ASSY006_ZTZ_RUNSTART window = sender as ASSY006_ZTZ_RUNSTART;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
            else
            {
                ASSY006_WS_RUNSTART popup = sender as ASSY006_WS_RUNSTART;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
        }

        private void PopupCancelConfirm()
        {
            CMM_ASSY_CANCEL_CONFIRM_PROD popupCancelConfrim = new CMM_ASSY_CANCEL_CONFIRM_PROD();
            popupCancelConfrim.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = _equipmentCode;
            parameters[3] = _equipmentName;

            C1WindowExtension.SetParameters(popupCancelConfrim, parameters);

            popupCancelConfrim.Closed += popupCancelConfrim_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCancelConfrim.ShowModal()));
        }

        private void popupCancelConfrim_Closed(object sender, EventArgs e)
        {
            /*
            CMM_ASSY_CANCEL_CONFIRM_PROD pop = sender as CMM_ASSY_CANCEL_CONFIRM_PROD;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //SetProductLotList();
            }
            if (string.IsNullOrEmpty(_equipmentSegmentCode) || string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text) ) return;
            SetProductLotList();
            */
        }

        private void PopupCancelTerm()
        {
            CMM_ASSY_CANCEL_TERM popupCanCelTerm = new CMM_ASSY_CANCEL_TERM { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = _processCode;
            C1WindowExtension.SetParameters(popupCanCelTerm, parameters);

            popupCanCelTerm.Closed += popupCanCelTerm_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCanCelTerm.ShowModal()));
        }

        private void popupCanCelTerm_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupCancelTermSepa()
        {
            CMM_ASSY_CANCEL_TERM_SEPA popupCanCelTermSepa = new CMM_ASSY_CANCEL_TERM_SEPA { FrameOperation = FrameOperation };

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = _processName;

            parameters[3] = _equipmentName;
            C1WindowExtension.SetParameters(popupCanCelTermSepa, parameters);

            popupCanCelTermSepa.Closed += popupCanCelTermSepa_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCanCelTermSepa.ShowModal()));
        }

        private void popupCanCelTermSepa_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
        }



        private void PopupEditEquipmentQty()
        {
            CMM_ASSY_EQPT_INPUT_QTY popupEqutInputQty = new CMM_ASSY_EQPT_INPUT_QTY { FrameOperation = FrameOperation };

            object[] parameters = new object[2];
            parameters[0] = _dvProductLot["LOTID"].GetString();
            parameters[1] = _dvProductLot["EQPT_END_QTY"].GetInt();
            C1WindowExtension.SetParameters(popupEqutInputQty, parameters);
            popupEqutInputQty.Closed += popupEqutInputQty_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupEqutInputQty.ShowModal()));
        }

        private void popupEqutInputQty_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_INPUT_QTY popup = sender as CMM_ASSY_EQPT_INPUT_QTY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
                ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                SetProductLotList();
            }
        }

        private void PopupQualityInput()
        {
            CMM_COM_SELF_INSP popupSelfInspection = new CMM_COM_SELF_INSP
            {
                FrameOperation = FrameOperation
            };

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;       // Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = UcAssemblyProductLot.txtSelectEquipmentName.Text;   //Util.NVC(ComboEquipment.Text);
            parameters[4] = _dvProductLot["LOTID"].GetString();                 //Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[5] = _dvProductLot["WIPSEQ"].GetString();                // Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupSelfInspection, parameters);
            popupSelfInspection.Closed += popupSelfInspection_Closed;

            Dispatcher.BeginInvoke(new Action(() => popupSelfInspection.ShowModal()));
        }

        private void popupSelfInspection_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP pop = sender as CMM_COM_SELF_INSP;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupEquipmentEnd()
        {
            bool isSmallType;

            if (string.Equals(_processCode, Process.ZZS))
            {
                CMM_ASSY_EQPEND wndEqpEnd;

                wndEqpEnd = new CMM_ASSY_EQPEND();
                wndEqpEnd.FrameOperation = FrameOperation;

                if (wndEqpEnd != null)
                {
                    object[] Parameters = new object[8];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    Parameters[2] = Process.ZZS;
                    Parameters[3] = _dvProductLot["LOTID"].GetString();
                    Parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                    Parameters[5] = "N";    // Stacking.
                    Parameters[6] = _dvProductLot["WIPDTTM_ST_ORG"].GetString();
                    Parameters[7] = _dvProductLot["DTTM_NOW"].GetString();
                    C1WindowExtension.SetParameters(wndEqpEnd, Parameters);

                    wndEqpEnd.Closed += new EventHandler(wndEqpEnd_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndEqpEnd.ShowModal()));
                }

            }           
            if (string.Equals(_processCode, Process.PACKAGING))
            {
                CMM_ASSY_EQPEND wndEqpEnd;

                wndEqpEnd = new CMM_ASSY_EQPEND();
                wndEqpEnd.FrameOperation = FrameOperation;

                if (wndEqpEnd != null)
                {
                    object[] Parameters = new object[8];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    Parameters[2] = Process.PACKAGING;
                    Parameters[3] = _dvProductLot["LOTID"].GetString();
                    Parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                    Parameters[5] = "N";    // Stacking.
                    Parameters[6] = _dvProductLot["WIPDTTM_ST_ORG"].GetString();
                    Parameters[7] = _dvProductLot["DTTM_NOW"].GetString();
                    C1WindowExtension.SetParameters(wndEqpEnd, Parameters);

                    wndEqpEnd.Closed += new EventHandler(wndEqpEnd_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndEqpEnd.ShowModal()));
                }

            }
            //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
            else if (string.Equals(_processCode, Process.ZTZ))
            {
                CMM_ASSY_EQPEND_ZTZ wndEqpEnd;

                wndEqpEnd = new CMM_ASSY_EQPEND_ZTZ();
                wndEqpEnd.FrameOperation = FrameOperation;

                if (wndEqpEnd != null)
                {
                    object[] Parameters = new object[16];
                    Parameters[0] = _equipmentSegmentCode;
                    Parameters[1] = _equipmentCode;
                    Parameters[2] = Process.ZTZ;
                    Parameters[3] = _dvProductLot["PR_LOTID"].GetString();
                    Parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                    Parameters[5] = "N";    // Stacking.
                    Parameters[6] = _dvProductLot["WIPDTTM_ST_ORG"].GetString();
                    Parameters[7] = GetCurrentTime().GetString();
                    Parameters[8] = Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[4].DataItem, "EQPT_MOUNT_PSTN_ID"));
                    Parameters[9] = "A";
                    Parameters[10]= _dvProductLot["PR_LOTID"].GetString();
                    Parameters[11] = Util.NVC(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[2].DataItem, "OUTPUTQTY"));
                    Parameters[12] = 0;
                    Parameters[13] = Util.NVC(DataTableConverter.GetValue(UcAssemblyProductionResult.dgDefectDetail.Rows[2].DataItem, "OUTPUTQTY"));
                    Parameters[14] = 0;
                    Parameters[15] = _dvProductLot["FINAL_CUT_FLAG"].GetString();
                    C1WindowExtension.SetParameters(wndEqpEnd, Parameters);

                    wndEqpEnd.Closed += new EventHandler(wndEqpEndZtz_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndEqpEnd.ShowModal()));
                }

            }
            else
            {
                if (string.Equals(_processCode, Process.WINDING))
                    isSmallType = true;
                else if (string.Equals(_processCode, Process.ASSEMBLY))
                    isSmallType = false;
                else
                    isSmallType = false;


                CMM_ASSY_EQUIPMENT_END popupEqpEnd = new CMM_ASSY_EQUIPMENT_END { FrameOperation = FrameOperation };
                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                object[] parameters = new object[7];
                parameters[0] = _processCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _dvProductLot["LOTID"].GetString(); // Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
                parameters[3] = _dvProductLot["WIPDTTM_ST_ORG"].GetString(); //Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPDTTM_ST_ORG"));
                parameters[4] = _dvProductLot["DTTM_NOW"].GetString(); //Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "DTTM_NOW"));
                parameters[5] = isSmallType;
                parameters[6] = false;  // 재작업 여부
                C1WindowExtension.SetParameters(popupEqpEnd, parameters);

                popupEqpEnd.Closed += new EventHandler(popupEqpEnd_Closed);
                Dispatcher.BeginInvoke(new Action(() => popupEqpEnd.ShowModal()));
            }
        }

        private void wndEqpEnd_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPEND window = sender as CMM_ASSY_EQPEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                SetProductLotList();
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
        }

        private void popupEqpEnd_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQUIPMENT_END popup = sender as CMM_ASSY_EQUIPMENT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                SetProductLotList();
            }
        }

        private void PopupEquipmentIssue()
        {
            string shiftId, shiftName, workUserId, workUserName;

            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
            if (drShift.Length > 0)
            {
                shiftId = drShift[0]["SHFT_ID"].GetString();        //작업조코드
                shiftName = drShift[0]["VAL001"].GetString();       //작업조명
                workUserId = drShift[0]["WRK_USERID"].GetString();  //작업자 ID 
                workUserName = drShift[0]["VAL002"].GetString();    //작업자명
            }
            else
            {
                shiftId = string.Empty;         //작업조코드
                shiftName = string.Empty;       //작업조명
                workUserId = string.Empty;      //작업자 ID 
                workUserName = string.Empty;    //작업자명
            }

            CMM_COM_EQPCOMMENT popupEquipmentComment = new CMM_COM_EQPCOMMENT();
            popupEquipmentComment.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = _dvProductLot["LOTID"].GetString();
            parameters[4] = _dvProductLot["WIPSEQ"].GetString();
            parameters[5] = UcAssemblyProductLot.txtSelectEquipmentName.Text;
            parameters[6] = shiftName;      //txtShift.Text; // 작업조명
            parameters[7] = shiftId;        //txtShift.Tag; // 작업조코드
            parameters[8] = workUserName;   //txtWorker.Text; // 작업자명
            parameters[9] = workUserId;     //txtWorker.Tag; // 작업자 ID

            C1WindowExtension.SetParameters(popupEquipmentComment, parameters);
            Dispatcher.BeginInvoke(new Action(() => popupEquipmentComment.ShowModal()));
        }

        private void PopupWindingLot()
        {
            CMM_ASSY_WINDING_LOT_INFO popupWindingLotInfo = new CMM_ASSY_WINDING_LOT_INFO { FrameOperation = FrameOperation };

            DataTable dtEquipment = DataTableConverter.Convert(cboEquipment.ItemsSource);
            DataRow newRow = dtEquipment.NewRow();
            dtEquipment.Rows.InsertAt(newRow, 0);

            object[] parameters = new object[8];
            parameters[0] = dtEquipment; // 기존 공정에서 사용할때에는 SELECT 값이 있음 동일하게 하기 위해 강제로 맞춤
            parameters[1] = string.Empty;
            parameters[2] = DataTableConverter.Convert(cboEquipmentSegment.ItemsSource);
            parameters[3] = string.Empty;
            parameters[4] = _processCode;
            parameters[5] = false;   //Process.ASSEMBLY 공정진척은 _isSmallType = false;

            parameters[6] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[7] = cboEquipmentSegment.SelectedValue;

            C1WindowExtension.SetParameters(popupWindingLotInfo, parameters);
            popupWindingLotInfo.Closed += new EventHandler(popupWindingLotInfo_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupWindingLotInfo.ShowModal()));
        }

        private void popupWindingLotInfo_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WINDING_LOT_INFO popup = sender as CMM_ASSY_WINDING_LOT_INFO;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void PopupBoxPrint()
        {
            CMM_ASSY_BOX_HISTORYCARD popBoxPrint = new CMM_ASSY_BOX_HISTORYCARD { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = _processCode;

            C1WindowExtension.SetParameters(popBoxPrint, parameters);

            popBoxPrint.Closed += popBoxPrint_Closed;
            Dispatcher.BeginInvoke(new Action(() => popBoxPrint.ShowModal()));
        }

        private void popBoxPrint_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_BOX_HISTORYCARD pop = sender as CMM_ASSY_BOX_HISTORYCARD;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupHistoryCard()
        {
            bool isSmallType;
            if (string.Equals(_processCode, Process.WINDING))
                isSmallType = true;
            else if (string.Equals(_processCode, Process.ASSEMBLY))
                isSmallType = false;
            else
                isSmallType = false;

            CMM_ASSY_HISTORYCARD popupHistoryCard = new CMM_ASSY_HISTORYCARD { FrameOperation = FrameOperation };

            object[] parameters = new object[7];
            parameters[0] = DataTableConverter.Convert(cboEquipment.ItemsSource);
            parameters[1] = string.Empty;
            parameters[2] = DataTableConverter.Convert(cboEquipmentSegment.ItemsSource);
            parameters[3] = Util.NVC(cboEquipmentSegment.SelectedIndex);
            parameters[4] = _processCode;
            parameters[5] = isSmallType;
            parameters[6] = UcAssemblyProductLot.txtSelectEquipment.Text;

            C1WindowExtension.SetParameters(popupHistoryCard, parameters);
            popupHistoryCard.Closed += popupHistoryCard_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupHistoryCard.ShowModal()));
        }

        private void popupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_HISTORYCARD popup = sender as CMM_ASSY_HISTORYCARD;
        }

        private void PopupWaitPancake(object sender)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return;
            }

            string workOrder;

            DataRow[] drWorkOrder = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '1'");
            if (drWorkOrder.Length > 0)
                workOrder = drWorkOrder[0]["CSTID"].ToString();
            else
                workOrder = string.Empty;

            // A음극, C양극
            string electrodeCode = string.Empty;
            string limitCount = string.Empty;
            string flag = string.Empty;
            string inputLotId = string.Empty;

            if (btn.Name == "btnWaitPancake")
            {
                flag = "A";
            }
            else if (btn.Name == "btnProductWaitLot")
            {
                flag = "C";
            }

            bool isSmallType;

            if (string.Equals(_processCode, Process.WINDING))
                isSmallType = true;
            else if (string.Equals(_processCode, Process.ASSEMBLY))
                isSmallType = false;
            else
                isSmallType = false;

            CMM_WAITING_PANCAKE_SEARCH popupWaitingPancake = new CMM_WAITING_PANCAKE_SEARCH { FrameOperation = FrameOperation };
            object[] parameters = new object[11];
            parameters[0] = workOrder;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = _processCode;
            parameters[3] = electrodeCode;
            parameters[4] = limitCount;
            parameters[5] = inputLotId;
            parameters[6] = flag;
            parameters[7] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[8] = string.Empty;
            parameters[9] = isSmallType;
            parameters[10] = "N";
            C1WindowExtension.SetParameters(popupWaitingPancake, parameters);
            popupWaitingPancake.Closed += popupWaitingPancake_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupWaitingPancake.ShowModal()));
        }

        private void popupWaitingPancake_Closed(object sender, EventArgs e)
        {
            CMM_WAITING_PANCAKE_SEARCH popup = sender as CMM_WAITING_PANCAKE_SEARCH;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupTrayLotChange()
        {
            CMM_ASSY_TRAYLOT_CHANGE popTraychange = new CMM_ASSY_TRAYLOT_CHANGE { FrameOperation = FrameOperation };
            _dispatcherMainTimer?.Stop();

            object[] parameters = new object[3];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;

            C1WindowExtension.SetParameters(popTraychange, parameters);
            popTraychange.Closed += popTraychange_Closed;

            grdMain.Children.Add(popTraychange);
            popTraychange.BringToFront();
        }

        private void popTraychange_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_TRAYLOT_CHANGE pop = sender as CMM_ASSY_TRAYLOT_CHANGE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //GetProductLot();
            }
            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
            SetProductLotList();

            if (_dispatcherMainTimer != null && _dispatcherMainTimer.Interval.TotalSeconds > 0)
                _dispatcherMainTimer.Start();

        }

        private void PopupLastCellNo()
        {
            CMM_ASSY_CELL_NO_LAST popupLastCell = new CMM_ASSY_CELL_NO_LAST { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            if (_dvProductLot != null && !string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
                parameters[0] = _dvProductLot["LOTID"].GetString().Substring(0, 10);
            else
                parameters[0] = "";

            C1WindowExtension.SetParameters(popupLastCell, parameters);
            Dispatcher.BeginInvoke(new Action(() => popupLastCell.ShowModal()));
        }

        private void PopupCellDetailInfo()
        {
            CMM_ASSY_CELL_ID_DETAIL popupCellInfo = new CMM_ASSY_CELL_ID_DETAIL { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = "";

            C1WindowExtension.SetParameters(popupCellInfo, parameters);
            Dispatcher.BeginInvoke(new Action(() => popupCellInfo.ShowModal()));
        }


        //
        private void DgProductLotZtz_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg != null && (dg.CurrentCell == null || dg.CurrentCell.Row == null)) return;

            // +, - 선택에 따른 라디오버튼 체크가 비정상 작동을 하는 경우가 발생하여 TOGGLEKEY 컬럼 영역 선택시엔 라디오 버튼 체크 배제 처리 함.
            if (dg.CurrentCell == null || dg.CurrentCell.Column.Name == "TOGGLEKEY") return;
            if (dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter == null) return;

            RadioButton rb = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

            int rowIdx = dg.CurrentCell.Row.Index;
            DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            if (string.IsNullOrEmpty(drv["LOTID"].GetString())) return;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);

            if (dg.CurrentCell.Column.Name == "LOTID" && !string.IsNullOrEmpty(drv["LOTID"].GetString()))
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                if (drShift.Length > 0)
                {
                    UcAssemblyProductionResult.WorkerId = drShift[0]["WRK_USERID"].GetString();
                    UcAssemblyProductionResult.ShiftId = drShift[0]["SHFT_ID"].GetString();
                    UcAssemblyProductionResult.WorkerName = drShift[0]["VAL002"].GetString();
                }

                dg.CurrentCell.Presenter.Cursor = Cursors.Wait;

                UcAssemblyCommand.SetButtonVisibility(UcAssemblyCommand.ButtonVisibilityType.ProductionResult);

                GetButtonPermissionGroup();

                grdProduct.Visibility = Visibility.Collapsed;
                grdProductionResult.Visibility = Visibility.Visible;

                SetUserControlProductionResult();
                UcAssemblyProductionResult.SelectProductionResult();

                dg.CurrentCell.Presenter.Cursor = Cursors.Hand;
            }
        }

        private void DgProductLotZtz_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg?.CurrentCell == null || dg.CurrentCell.Row == null) return;
            if (dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter == null) return;

            // +, - 선택에 따른 라디오버튼 체크가 비정상 작동을 하는 경우가 발생하여 TOGGLEKEY 컬럼 영역 선택시엔 라디오 버튼 체크 배제 처리 함.
            if (dg.CurrentCell.Column.Name == "TOGGLEKEY") return;

            int rowIdx = dg.CurrentCell.Row.Index;
            DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            DgProductLot = UcAssemblyProductLot.dgProductLotZtz;
            RadioButton rb = DgProductLot.GetCell(DgProductLot.CurrentCell.Row.Index, DgProductLot.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);
        }        

        //오창 2산단 소형 조립 ZTZ(A5600) 설비 대응
        private double GetTmpProdQty()
        {
            double returnTmpProdQty = 0;

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            const string bizRuleName = "DA_PRD_SEL_TMP_PROD_QTY";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            returnTmpProdQty = CommonVerify.HasTableRow(searchResult) ? searchResult.Rows[0]["TMP_PROD_QTY"].GetDouble() : 0;

            return returnTmpProdQty;
        }

        private void wndEqpEndZtz_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPEND_ZTZ window = sender as CMM_ASSY_EQPEND_ZTZ;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                SetProductLotList();
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
        }

        private bool ValidConfirmLotCheck()
        {
            //productReult.데이타

            int iRow = new Util().GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return false;
            }

            try
            {
                string sLotID = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));

                if (string.IsNullOrEmpty(sLotID))
                {
                    Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                    return false;
                }

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", IndataTable);

                if (dt != null && dt.Rows.Count > 0 && (string.Equals(INOUT_TYPE.IN, dt.Rows[0]["WIP_TYPE_CODE"]) || string.Equals(INOUT_TYPE.INOUT, dt.Rows[0]["WIP_TYPE_CODE"])))
                {
                    Util.MessageValidation("SFU5066");  // 이미 실적 확정 된 LOT입니다.
                    return false;
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }

        bool ValidateConfirm()
        {
            if (!ValidateProductQty())
                return false;

            if (!ValidWorkTime())
                return false;

            if (!ValidShift())
                return false;

            if (!ValidOperator())
                return false;

            if (!ValidOverProdQty())
                return false;

            return true;
        }

        private bool ValidWorkTime()
        {
            int iRow = new Util().GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPDTTM_ST"))))
                {
                    Util.MessageValidation("SFU1696");  //시작시간 정보가 없습니다.
                    return false;
                }

                double totalMin = (Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPDTTM_ED"))) - Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPDTTM_ST")))).TotalMinutes;

                if (totalMin < 0)
                {
                    Util.MessageValidation("SFU1219");  //가동시간을 확인 하세요.
                    return false;
                }

                return true;
                
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
            
        }

        bool ValidateProductQty()
        {
            DataTable dt = DataTableConverter.Convert(DgProductQty.ItemsSource);
            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Util.NVC_Decimal(dt.Rows[i]["OUTPUTQTY"]) <= 0)
                {
                    Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                    return false;
                }
            }
            return true;
        }

        private bool ValidShift()
        {

            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            try
            {
                if (string.IsNullOrEmpty(Util.NVC(drShift[0]["SHFT_ID"])))
                {
                    Util.MessageValidation("SFU1845");  //작업조를 입력하세요
                    return false;
                }               

                return true;

            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }

        private bool ValidOperator()
        {
            try
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[3].DataItem, "WRK_USERID"))))
                {
                    Util.MessageValidation("SFU1843");  //작업자를 입력 해 주세요.
                    return false;
                }

                return true;

            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }

        private bool ValidOverProdQty()
        {
            double inputQty = Convert.ToDouble(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "OUTPUTQTY"));
            double lossQty = Convert.ToDouble(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "DTL_LOSS"));

            if (inputQty < lossQty)
            {
                Util.MessageValidation("SFU3236");
                return false;
            }
            return true;
        }

        private void CheckAuthValidation(Action callback)
        {
            try
            {
                // AD 인증 기능 추가 [2019-08-21]
                DataTable confirmDt = GetConfirmAuthVaildation();

                if (confirmDt != null && confirmDt.Rows.Count > 0)
                {
                    // 강제 인터락 체크 (이거는 공용 메세지로 공유하니 필요 시 MES MESSAGE 코드 별도 추가 필요)
                    if (string.Equals(confirmDt.Rows[0]["VALIDATION_FLAG"], "Y"))
                    {
                        // 실적확정은 자동 Interlock 기능에 의하여 보류 되었습니다. [%1]
                        Util.MessageValidation("SFU5125", new object[] { Util.NVC(confirmDt.Rows[0]["RSLT_CNFM_TYPE_CODE"]) });
                        return;
                    }

                    // AD 인증 체크
                    if (string.Equals(confirmDt.Rows[0]["AD_CHK_FLAG"], "Y"))
                    {
                        LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
                        authConfirm.FrameOperation = FrameOperation;
                        authConfirm.sContents = Util.NVC(confirmDt.Rows[0]["DISP_MSG"]);
                        if (authConfirm != null)
                        {
                            // SBC AD 인증
                            if (string.Equals(confirmDt.Rows[0]["AD_CHK_TYPE_CODE"], "SBC_AD"))
                            {
                                object[] Parameters = new object[1];
                                Parameters[0] = Util.NVC(confirmDt.Rows[0]["AUTHID"]);

                                C1WindowExtension.SetParameters(authConfirm, Parameters);

                            }
                            else if (string.Equals(confirmDt.Rows[0]["AD_CHK_TYPE_CODE"], "LGCHEM_AD"))
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = Util.NVC(confirmDt.Rows[0]["AUTHID"]);
                                Parameters[1] = "lgchem.com";

                                C1WindowExtension.SetParameters(authConfirm, Parameters);
                            }
                           
                            authConfirm.Closed += new EventHandler(OnCloseAuthConfirm);
                            this.Dispatcher.BeginInvoke(new Action(() => authConfirm.ShowModal()));
                        }
                    }
                    else
                    {
                        callback();
                    }
                }
                else
                {
                    callback();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void OnCloseAuthConfirm(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                ConfirmCheck();
            }
        }

        private DataTable GetConfirmAuthVaildation()
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SHOPID", typeof(string));
            IndataTable.Columns.Add("AREAID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            Indata["EQSGID"] = Util.NVC(_equipmentSegmentCode);
            Indata["PROCID"] = Process.ZTZ;
            Indata["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CNFM_AUTH", "INDATA", "RSLTDT", IndataTable);

            if (dt != null && dt.Rows.Count > 0)
            {
                // 인증 여부
                bool isAuthConfirm = true;

                // Input용 데이터 테이블 ( INPUTVALUE : 비교할 대상 값, CHK_VALUE1 : SPEC1, CHK_VALUE2 : SPEC2 )
                DataTable inputTable = new DataTable();
                inputTable.Columns.Add("CHK_VALUE1", typeof(decimal));
                inputTable.Columns.Add("CHK_VALUE2", typeof(decimal));
                inputTable.Columns.Add("INPUTVALUE", typeof(decimal));

                foreach (DataRow row in dt.Rows)
                {
                    inputTable.Clear();

                    if (!string.IsNullOrEmpty(Util.NVC(row["CHK_VALUE1"])) || !string.IsNullOrEmpty(Util.NVC(row["CHK_VALUE2"])))
                    {
                        DataRow dataRow = inputTable.NewRow();
                        dataRow["CHK_VALUE1"] = row["CHK_VALUE1"];
                        dataRow["CHK_VALUE2"] = row["CHK_VALUE2"];
                        inputTable.Rows.Add(dataRow);
                    }

                    if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "CNFM_GOOD_QTY_LIMIT"))
                    {
                        // 양품량 기준 체크
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "GOODQTY"));
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);

                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "CNFM_PROD_QTY_LIMIT"))
                    {
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "INPUTQTY"));
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);
                    }                                      

                    // 인증이 필요한 경우 전체 정보 전달
                    if (isAuthConfirm == false)
                    {
                        DataTable outTable = dt.Clone();
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //outTable.Rows.Add(row.ItemArray);
                        outTable.AddDataRow(row);
                        return outTable;
                    }
                }
            }
            return new DataTable();
        }

        private bool CheckLimitValue(string sCheckType, DataTable inputTable)
        {
            foreach (DataRow row in inputTable.Rows)
            {
                switch (sCheckType)
                {
                    case "LOWER":           // SPEC LOWER
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "UPPER":           // SPEC UPPER
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "BOTH":            // SPEC IN
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE1"]) &&
                            Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE2"]))
                            return false;

                        break;

                    case "NOT_BOTH":        // SPEC OUT
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) < Util.NVC_Decimal(row["CHK_VALUE1"]) &&
                            Util.NVC_Decimal(row["INPUTVALUE"]) > Util.NVC_Decimal(row["CHK_VALUE2"]))
                            return false;

                        break;
                    case "VALUE":
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) == Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    case "NOT_VAULE":
                        if (Util.NVC_Decimal(row["INPUTVALUE"]) != Util.NVC_Decimal(row["CHK_VALUE1"]))
                            return false;

                        break;
                    default:
                        break;
                }
            }
            return true;
        }        

        private void ConfirmCheck()
        {
            ConfirmProcess();
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        private string GetUserName()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("USERNAME", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["USERNAME"] = Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[3].DataItem, "WRK_USERID"));
            dr["LANGID"] = LoginInfo.LANGID;

            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

            if (dtRslt.Rows.Count > 0)
            {
                return dtRslt.Rows[0]["USERNAME"].ToString();
            }
            else
            {
                Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[3].DataItem, "WRK_USERID"));
            }

            return dtRslt.Rows[0]["USERNAME"].ToString();
        }

        private Dictionary<string, string> GetRemarkConvert()
        {
            Dictionary<string, string> remarkInfo = new Dictionary<string, string>();

            if (DgRemarkInfo.Rows.Count > 0)
            {
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int i = 1; i < DgRemarkInfo.Rows.Count; i++)
                {
                    sRemark.Clear();

                    // 1. 특이사항
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(DgRemarkInfo.Rows[i].DataItem, "REMARK")));
                    sRemark.Append("|");

                    remarkInfo.Add(Util.NVC(DataTableConverter.GetValue(DgRemarkInfo.Rows[i].DataItem, "LOTID")), sRemark.ToString());
                }
            }
            return remarkInfo;
        }


        void ConfirmProcess(bool isLastBatch = false)
        {
            string listData = string.Empty;

            // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
            string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

            // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴
            string topInfo = string.Empty;

            if (string.Equals(_processCode, Process.ZTZ))
            {
                decimal prodQty = Util.NVC_Decimal(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "OUTPUTQTY"));
                decimal eqptEndQty = Util.NVC_Decimal(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "EQPTQTY"));

                if (Math.Abs((prodQty - eqptEndQty)) >= 30)
                    topInfo = MessageDic.Instance.GetMessage("SFU3468", UcAssemblyProductionResult.txtUnit.Text);  // 설비 양품량과 생산량 차이가 30M이상입니다.\n실물 수량이 정말 맞습니까?
            }

            // LANE수 정보 표시 (믹서는 이전 확정 수량 VALIDATION)
            string addMessage = string.Empty;

            // 설비 LOSS 체크
            DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(UcAssemblyProductLot.txtSelectEquipment.Text, Process.ZTZ);

            if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0)
            {
                int iLossCnt = Util.NVC_Int(dtEqpLossInfo.Rows[0]["CNT"]);

                if (iLossCnt > 0)
                {
                    string sInfo = string.Empty;
                    string sLossInfo = string.Empty;

                    for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                    {
                        sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                        sLossInfo = sLossInfo + "\n" + sInfo;
                    }
                    addMessage = MessageDic.Instance.GetMessage("SFU3501", new object[] { sLossInfo });
                }
            }           

            // REMARK 필요 정보 취합
            Dictionary<string, string> remarkInfo = GetRemarkConvert();
            if (remarkInfo.Count == 0)
            {
                Util.MessageValidation("SFU3484"); // 특이사항 정보를 확인 바랍니다.
                return;
            }

            Dictionary<int, string> finalCutInfo = null;
            if (string.Equals(_processCode, Process.ZTZ))
            {
                finalCutInfo = CheckFinalCutLot();
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;
            }


            /////////////////////////////////////////////////////////////////////////
            //Rate 관련 팝업
            //완료 처리 하기 전에 팝업 표시
            _bInputErpRate = false;
            _dtRet_Data.Clear();
            _sUserID = string.Empty;
            _sDepID = string.Empty;
            string sLotID = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("PR_LOTID").GetString();
            string sWipSeq = string.Empty;

            _isLastBatch = isLastBatch;

            if (PERMIT_RATE_input(sLotID, sWipSeq))
            {
                return;
            }
            ///////////////////////////////////////////////////////////////////////////////

            if (string.Equals(_processCode, Process.ZTZ))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE
                        
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROD_VER_CODE", typeof(string));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WIPNOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("CUTYN", typeof(string));
                        inData.Columns.Add("REMAINQTY", typeof(string));
                        inData.Columns.Add("LANE_QTY", typeof(int));
                        inData.Columns.Add("LANE_PTN_QTY", typeof(int));
                        inData.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = "OFF";
                        row["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                        row["PROD_VER_CODE"] = "";
                        row["SHIFT"] = Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[3].DataItem, "SHFT_ID"));
                        row["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        row["WRK_USER_NAME"] = GetUserName();
                        row["WRK_USERID"] = Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[3].DataItem, "WRK_USERID"));
                        row["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(DgRemarkInfo.Rows[1].DataItem, "LOTID"))];
                        row["USERID"] = LoginInfo.USERID;
                        row["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";
                        row["REMAINQTY"] = finalCutInfo[6];
                        row["LANE_QTY"] = "1";
                        row["LANE_PTN_QTY"] = "1";
                        row["CALDATE"] = GetCurrentTime();

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable input = indataSet.Tables.Add("IN_INPUT");
                        input.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        input.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        input.Columns.Add("INPUT_LOTID", typeof(string));

                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count <= 0)
                        {
                            Util.MessageValidation("SFU1398");  //MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        row = input.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        row["EQPT_MOUNT_PSTN_STATE"] = "A";
                        row["INPUT_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("PR_LOTID").GetString();
                        input.Rows.Add(row);

                        DataTable InLot = indataSet.Tables.Add("INLOT");
                        InLot.Columns.Add("LOTID", typeof(string));
                        InLot.Columns.Add("INPUTQTY", typeof(string));
                        InLot.Columns.Add("OUTPUTQTY", typeof(string));
                        InLot.Columns.Add("RESNQTY", typeof(string));

                        for (int i = DgProductQty.TopRows.Count; i < (DgProductQty.Rows.Count - DgProductQty.BottomRows.Count); i++)
                        {
                            row = InLot.NewRow();
                            row["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                            row["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(DgProductQty.Rows[i].DataItem, "INPUTQTY"));
                            row["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(DgProductQty.Rows[i].DataItem, "GOODQTY"));
                            row["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(DgProductQty.Rows[i].DataItem, "DEFECTQTY"));

                            indataSet.Tables["INLOT"].Rows.Add(row);
                        }
                        #endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_ASSY_LS", "INDATA,IN_INPUT,IN_LOT", null, (sResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                        }, indataSet);

                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                        btnSearch_Click(null, null);
                        Util.MessageInfo("SFU1270");
                        
                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }                              
        }


        void ConfirmProcess_Rate(bool isLastBatch = false)
        {
            string listData = string.Empty;

            // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
            string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

            // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴
            string topInfo = string.Empty;

            if (string.Equals(_processCode, Process.ZTZ))
            {
                decimal prodQty = Util.NVC_Decimal(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "OUTPUTQTY"));
                decimal eqptEndQty = Util.NVC_Decimal(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "EQPTQTY"));

                if (Math.Abs((prodQty - eqptEndQty)) >= 30)
                    topInfo = MessageDic.Instance.GetMessage("SFU3468", UcAssemblyProductionResult.txtUnit.Text);  // 설비 양품량과 생산량 차이가 30M이상입니다.\n실물 수량이 정말 맞습니까?
            }

            // LANE수 정보 표시 (믹서는 이전 확정 수량 VALIDATION)
            string addMessage = string.Empty;

            // 설비 LOSS 체크
            DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(UcAssemblyProductLot.txtSelectEquipment.Text, Process.ZTZ);

            if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0)
            {
                int iLossCnt = Util.NVC_Int(dtEqpLossInfo.Rows[0]["CNT"]);

                if (iLossCnt > 0)
                {
                    string sInfo = string.Empty;
                    string sLossInfo = string.Empty;

                    for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                    {
                        sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                        sLossInfo = sLossInfo + "\n" + sInfo;
                    }
                    addMessage = MessageDic.Instance.GetMessage("SFU3501", new object[] { sLossInfo });
                }
            }

            // REMARK 필요 정보 취합
            Dictionary<string, string> remarkInfo = GetRemarkConvert();
            if (remarkInfo.Count == 0)
            {
                Util.MessageValidation("SFU3484"); // 특이사항 정보를 확인 바랍니다.
                return;
            }

            Dictionary<int, string> finalCutInfo = null;
            if (string.Equals(_processCode, Process.ZTZ))
            {
                finalCutInfo = CheckFinalCutLot();
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;
            }

            if (string.Equals(_processCode, Process.ZTZ))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        #region CONFIRM MESSAGE

                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROD_VER_CODE", typeof(string));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WIPNOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("CUTYN", typeof(string));
                        inData.Columns.Add("REMAINQTY", typeof(string));
                        inData.Columns.Add("LANE_QTY", typeof(int));
                        inData.Columns.Add("LANE_PTN_QTY", typeof(int));
                        inData.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = "OFF";
                        row["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                        row["PROD_VER_CODE"] = "";
                        row["SHIFT"] = Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[3].DataItem, "SHFT_ID"));
                        row["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        row["WRK_USER_NAME"] = GetUserName();
                        row["WRK_USERID"] = Util.NVC(DataTableConverter.GetValue(DgEquipment.Rows[3].DataItem, "WRK_USERID"));
                        row["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(DgRemarkInfo.Rows[1].DataItem, "LOTID"))];
                        row["USERID"] = LoginInfo.USERID;
                        row["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";
                        row["REMAINQTY"] = finalCutInfo[6];
                        row["LANE_QTY"] = "1";
                        row["LANE_PTN_QTY"] = "1";
                        row["CALDATE"] = GetCurrentTime();

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable input = indataSet.Tables.Add("IN_INPUT");
                        input.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        input.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        input.Columns.Add("INPUT_LOTID", typeof(string));

                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count <= 0)
                        {
                            Util.MessageValidation("SFU1398");  //MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        row = input.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        row["EQPT_MOUNT_PSTN_STATE"] = "A";
                        row["INPUT_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("PR_LOTID").GetString();
                        input.Rows.Add(row);

                        DataTable InLot = indataSet.Tables.Add("INLOT");
                        InLot.Columns.Add("LOTID", typeof(string));
                        InLot.Columns.Add("INPUTQTY", typeof(string));
                        InLot.Columns.Add("OUTPUTQTY", typeof(string));
                        InLot.Columns.Add("RESNQTY", typeof(string));

                        for (int i = DgProductQty.TopRows.Count; i < (DgProductQty.Rows.Count - DgProductQty.BottomRows.Count); i++)
                        {
                            row = InLot.NewRow();
                            row["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                            row["INPUTQTY"] = Util.NVC(DataTableConverter.GetValue(DgProductQty.Rows[i].DataItem, "INPUTQTY"));
                            row["OUTPUTQTY"] = Util.NVC(DataTableConverter.GetValue(DgProductQty.Rows[i].DataItem, "GOODQTY"));
                            row["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(DgProductQty.Rows[i].DataItem, "DEFECTQTY"));

                            indataSet.Tables["INLOT"].Rows.Add(row);
                        }
                        #endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_ASSY_LS", "INDATA,IN_INPUT,IN_LOT", null, (sResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            // ERP 불량 비율 Rate 저장
                            if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                            {
                                BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                            }

                        }, indataSet);

                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                        btnSearch_Click(null, null);
                        Util.MessageInfo("SFU1270");

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
        }

        private Dictionary<int, string> CheckFinalCutLot()
        {
            // 자동 FINAL CUT 여부
            // 0 : Confirm 여부, 1 : Final Cut 여부, 2  : Loss 처리 여부, 3 : Hold 표시 여부, 4 : Confirm Message, 5 : 잔량[팝업], 6 : 잔량[길이부족차감] 
            DataTable inTable = new DataTable();
            inTable.Columns.Add("PR_LOTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["PR_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("PR_LOTID").GetString();
            indata["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            indata["PROCID"] = Process.ZTZ;

            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUTLOT_ELEC", "INDATA", "RSLTDT", inTable);

            if (dt.Rows.Count <= 1)
            {
                Util.MessageValidation("SFU1707");  //실적 확정할 대상이 없습니다.
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            string sLotID = string.Empty;
            string sLackMesg = string.Empty;  //길이부족 메시지

            decimal iOutQty = 0;         // 생산 수량
            decimal iTotQty = 0;         // 투입 LOT 수량
            decimal iResQty = 0;         // 투입 LOT 처리 이후 최종 수량
            
            // 투입 Lot 수량 저장
            iOutQty = Util.NVC_Decimal(DataTableConverter.GetValue(DgProductQty.Rows[DgProductQty.TopRows.Count].DataItem, "OUTPUTQTY"));   // 생산수량
            iTotQty = Util.NVC_Decimal(dt.Rows[0]["WIPQTY"]);           

            iResQty = Util.NVC_Decimal(GetUnitFormatted(iTotQty - iOutQty));
            string tLotID = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            decimal tSeq = 0;

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["LOTID"].Equals(tLotID))
                    tSeq = dt.Rows[i]["CHILD_GR_SEQNO"].SafeToDecimal();
            }

            for(int i=1; i < dt.Rows.Count; i++)
            {
                if((dt.Rows[i]["WIPSTAT"].Equals("EQPT_END") || dt.Rows[i]["WIPSTAT"].Equals("PROC")) && dt.Rows[i]["CHILD_GR_SEQNO"].SafeToDecimal() < tSeq)
                {
                    Util.MessageValidation("SFU2898", new object[] { dt.Rows[i]["LOTID"] });   //LOT[{%1}]을 먼저 실적 확정 하세요.
                    return new Dictionary<int, string> { { 0, bool.FalseString } };
                }
            }
            
           
            if (iResQty < 0)
            {
                Util.MessageValidation("SFU1614");
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            if (iResQty > 0)
            {
                return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.FalseString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1963", new object[] { iResQty + UcAssemblyProductionResult.txtUnit.Text }) + "\r" + sLackMesg }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };   //투입LOT 잔량 {0}가 대기됩니다.\n실적확정 하시겠습니까?
            }
            else
            {
                return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.TrueString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1965") + "\r" + sLackMesg }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };     //투입LOT 잔량이 없습니다.\r\n실적확정하시겠습니까?       
            }
        }

        private string GetUnitFormatted(object obj)
        {
            string sValue = Util.NVC(obj);
            string sFormatted = string.Empty;
            double dFormat = 0;

            switch (UcAssemblyProductionResult.txtUnit.Text)
            {
                case "KG":
                    sFormatted = "{0:#,##0.000}";
                    break;

                case "M":
                    sFormatted = "{0:#,##0.00}";
                    break;

                case "EA":
                default:
                    sFormatted = "{0:#,##0}";
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
                return String.Format(sFormatted, 0);

            if (Double.TryParse(sValue, out dFormat))
                return String.Format(sFormatted, dFormat);

            return String.Format(sFormatted, 0);
        }

        private void CancelRunZtz()
        {
            try
            {
                string bizRuleName;

                bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_SI";
              
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["INPUT_LOTID"] = _dvProductLot["LOTID_PR"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                    SetProductLotList();
                    Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CancelEqpEndZtz()
        {
            try
            {
                if (_dvProductLot["WIPSTAT"].GetString().Equals("WAIT"))
                    return;

                if (_dvProductLot["WIPSTAT"].GetString().Equals("PROC"))
                {
                    Util.MessageValidation("SFU3464");  //진행중인 LOT은 장비완료취소 할 수 없습니다. [진행중인 LOT은 시작취소 버튼으로 작업취소 바랍니다.]
                    return;
                }

                //선택된 LOT을 작업 취소하시겠습니까?
                Util.MessageConfirm("SFU3151", (sResult) =>
                {
                    if (sResult == MessageBoxResult.OK)
                    {
                        DataSet inDataSet = new DataSet();

                        DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("PROCID", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        inDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        inDataRow["PROCID"] = Process.ZTZ;
                        inDataRow["EQPTID"] = _dvProductLot["EQPTID"].ToString();
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable InLotdataTable = inDataSet.Tables.Add("INLOT");
                        InLotdataTable.Columns.Add("LOTID", typeof(string));
                        InLotdataTable.Columns.Add("CUT_ID", typeof(string));
                        InLotdataTable.Columns.Add("WIPNOTE", typeof(string));

                        DataRow inLotDataRow = InLotdataTable.NewRow();
                        inLotDataRow["LOTID"] = _dvProductLot["LOTID"].GetString();

                        InLotdataTable.Rows.Add(inLotDataRow);

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_EQPT_END_LOT_ELTR", "INDATA,INLOT", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                            SetProductLotList();
                            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        }, inDataSet);
                    }
                });
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_processCode)) return;
            AutoPrint();
        }

        private void AutoPrint()
        {
            try
            {
                string equipmentCellPrintFlag = string.Empty;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_PRINT_YN();
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = string.IsNullOrEmpty(_dvProductLot["WIPSEQ"].GetString()) ? 1 : _dvProductLot["WIPSEQ"].GetInt();
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_PRT_CHK", "INDATA", "OUTDATA", inTable);

                //  파일 저장 분기 처리
                DataTable inTable2 = new DataTable();
                inTable2.Columns.Add("EQPTID", typeof(string));

                DataRow newRow2 = inTable2.NewRow();
                newRow2["EQPTID"] = _equipmentCode;
                inTable2.Rows.Add(newRow2);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR_CELL_ID_PRT_FLAG", "INDATA", "OUTDATA", inTable2);

                if (CommonVerify.HasTableRow(dtResult))
                    equipmentCellPrintFlag = dtResult.Rows[0]["CELL_ID_PRT_FLAG"].ToString();

                if (CommonVerify.HasTableRow(dtRslt))
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

                        // Line별 라벨 독립 발행 기능
                        if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                            return;
                        }
                        else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                        {
                            if (!_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                return;
                        }
                        else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                        {
                            foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            {
                                if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(_equipmentCode))
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

                        sCopy = Convert.ToString(Convert.ToInt32(sCopy) + 1);

                        string zplCode = GetPrintInfo(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString(), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);
                        string sLot = _dvProductLot["LOTID"].GetString();


                        if (zplCode.Equals(""))
                        {
                            Util.MessageValidation("SFU1498");
                            return;
                        }

                        if (equipmentCellPrintFlag.Equals("Y"))
                        {
                            Util.SendZplBarcode(sLot, zplCode);
                        }
                        else
                        {
                            if (zplCode.StartsWith("0,"))  // ZPL 정상 코드 확인.
                            {
                                if (PrintLabel(zplCode.Substring(2), drPrtInfo))
                                    SetLabelPrtHist(zplCode.Substring(2), drPrtInfo, _dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString(), sLBCD);
                            }
                            else
                            {
                                Util.MessageValidation(zplCode.Substring(2));
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

        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";
            try
            {

                DataTable inTable = _bizDataSet.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = _equipmentCode;
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

                if (CommonVerify.HasTableRow(dtResult))
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
        }

        private void SetLabelPrtHist(string zplCode, DataRow drPrtInfo, string sLot, string wipseq, string labelCode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = labelCode;
                newRow["LABEL_ZPL_CNTT"] = zplCode;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = wipseq;
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
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 무지부 방향설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonWorkHalfSlitSide_Click(object sender, RoutedEventArgs e)
        {
            PopupWorkHalfSlitSide();
        }

        private void PopupWorkHalfSlitSide_Closed(object sender, EventArgs e)
        {
            // 무지부/권취 방향 2종류 모두 사용하는 AREA 일 경우
            if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN popup = sender as ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN;

                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
            else
            {
                ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN popup = sender as ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
        }

        

        /// <summary>
        /// 무지부 방향설정
        /// </summary>
        private void PopupWorkHalfSlitSide()
        {
            if (!ValidationWorkHalfSlitSide()) return;

            // 무지부/권취 방향 2종류 모두 사용하는 AREA 일 경우
            if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN popupWorkHalfSlitSide = new ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[3];
                    Parameters[0] = _equipmentCode;
                    //Parameters[1] = UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag;
                    Parameters[2] = _processCode;

                    C1WindowExtension.SetParameters(popupWorkHalfSlitSide, Parameters);

                    popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
                }
            }
            else
            {
                ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN popupWorkHalfSlitSide = new ASSY006_WRK_HALF_SLITTING_ROLL_DIRCTN { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentCode;
                    //Parameters[1] = UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag;

                    C1WindowExtension.SetParameters(popupWorkHalfSlitSide, Parameters);

                    popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
                }
            }
        }

        private bool ValidationWorkHalfSlitSide()
        {
            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        

        /// <summary>
        /// 공통코드에 등록된 무지부방향 항목 불러오기
        /// </summary>
        /// <returns></returns>
        private DataTable GetCommonUWND()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "MNG_SLITTING_SIDE_AREA";
                dr["COM_CODE"] = _processCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void SetAutoConfirMode(bool bOn)
        {
            try
            {
                string bizRuleName = "BR_PRD_REG_EIOATTR_AUTO_RSLT_CNFM_FLAG";
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string)); //꼭 필요한가 확인 필요
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTO_RSLT_CNFM_FLAG", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = "ON";
                newRow["AUTO_RSLT_CNFM_FLAG"] = bOn ? "Y" : "N";

                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                Util.MessageInfo("PSS9097");    // 변경되었습니다.
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

        private void ButtonAutoRsltCnfmMode_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return;
            }

            GetAutoConfirmMode();
            string messageCode;
            if (!_isAutoConfirmMode)
            {
               
                messageCode = "SFU8440";    //자동실적확정모드 설정하시겠습니까?
                    
                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetAutoConfirMode(true);
                        Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                    }
                }, UcAssemblyProductLot.txtSelectEquipmentName.Text);
            }
            else
            {
                messageCode = "SFU8441";    //자동실적확정모드 해제하시겠습니까?

                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetAutoConfirMode(false);
                        Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                    }
                }, UcAssemblyProductLot.txtSelectEquipmentName.Text);
            }
        }



        #endregion


        #endregion

    }
}