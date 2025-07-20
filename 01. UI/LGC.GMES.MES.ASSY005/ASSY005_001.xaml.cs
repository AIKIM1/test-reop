/*************************************************************************************
 Created Date : 2020.10.16
      Creator : 신광희
   Decription : 조립 공정진척 (CNB2 동) 메인
--------------------------------------------------------------------------------------
 [Change History]
 2021.08.12  김지은 : 시생산샘플설정/해제 기능 추가
 2021.09.01  조영대 : Process.STACKING_FOLDING 의 경우 설비그룹 콤보 추가
 2021.10.25  김지은 : 무지부방향설정 추가(공동코드 유무에 따라 구/신 팝업 선택하여 띄움)
 2021.10.27  조영대 : 권취 방향 변경 팝업 추가
 2022.12.27  신광희 : GM NND 공정인 경우 적재율에 따른 UI 표시 기능 추가
 2023.02.24  김용군 : ESHM 증설-AZS E-Cuttor 설비 및 AZS Stacking 설비 대응  
 2023.03.20  강성묵 : 슬리팅 레인 번호 추가
 2023.06.29  주동석 : NND Unwinder 무지부/권취 방향 저장
 2023.08.24  장영철 : GM2 예외로직 추가 (G673 조건 추가)
 2023.11.21  문혜림 : E20231115-000860 라미 공정 불량/LOSS/물품청구 저장 버튼 숨김 처리
 2024.01.02  장영철 : 시생산 설정/해제 진행중 LOT Validation 전공정 반영ㅠ
 2024.01.23  남재현 : Package 공정 시 특별 관리 여부 컬럼 SPCL_MNGT_FLAG 이용.
 2024.01.31  박성진 : E20240124-000934 조립공정진척 설비 적재율 조회 극성 관련 오류 수정 및 극성 콤보박스 값 변경할때마다 해당하는 값 조회하도록 수정 
 2024.02.20  윤지해 : E20240130-000700 노칭 공정일 경우 전극/NND 공정 NG TAG수 비교 추가
 2024.02.21  김용군 : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
 2024.06.26  김용군 : E20240626-000751 ESMI1동 조립(A4) 의 경우 동별 공통코드 체크하여 MES SFU Configurtion에서 셋팅된 라인정보가 아닌 '조립공정진척-6Line' 화면에 라인 콤보박스에 선택된 라인정보로 조회되게 수정
 2024.09.04  김동일 : E20240806-000371 Lamination 공정의 적재율 메시지 로직 추가
 2025.05.20  천진수 : ESHG 증설 조립공정진척 DNC공정추가 
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
using LGC.GMES.MES.ASSY005.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY005
{
    public partial class ASSY005_001 : IWorkArea
    {
        #region Declaration


        public UcAssemblyCommand UcAssemblyCommand { get; set; }
        public UcAssemblyEquipment UcAssemblyEquipment { get; set; }
        public UcAssemblyProductLot UcAssemblyProductLot { get; set; }
        public UcAssemblyProductionResult UcAssemblyProductionResult { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private C1DataGrid DgEquipment { get; set; }
        public C1DataGrid DgProductLot { get; set; }

        private string _equipmentSegmentCode;
        private string _equipmentSegmentName;
        private string _processCode;
        private string _processName;
        private string _equipmentCode;
        private string _equipmentName;
        private string _treeEquipmentCode;
        private string _treeEquipmentName;
        private string _productLot;
        private string _equipmentMountPositionCode;
        private string sAttr2;

        private string _ldrLotIdentBasCode;
        private string _unldrLotIdentBasCode;
        private string _labelPassHoldFlag = string.Empty;
        private bool _isPopupConfirmJobEnd = true;  //로직이 중복으로 타는 현상 발생해 방지하기 위해
        private bool _isAutoSelectTime = false;
        private List<string> _selectUserModeAreaList = new List<string>(new string[] { "A7", "A9" });   // 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherRackRateTimer = new System.Windows.Threading.DispatcherTimer();

        DataRowView _dvProductLot;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private bool _isRackRateMode = false;

        //SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);


        private enum SetEquipmentType
        {
            SearchButton,
            EquipmentComboChange,
            EquipmentTreeClick,
            ProductLotClick
        }

        private enum SetRackState
        {
            Normal,
            Warning,
            Danger
        }

        private SetRackState _rackState;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public ASSY005_001()
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

            // NND 공정진척인 경우만 극성을 보여줌.
            SetPolarityByProcess();

            if (_dispatcherMainTimer != null)
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.GetString()))
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            }

            if (_dispatcherRackRateTimer != null)
            {
                _dispatcherRackRateTimer.Tick -= DispatcherRackRateTimer_Tick;
                _dispatcherRackRateTimer.Tick += DispatcherRackRateTimer_Tick;
                _dispatcherRackRateTimer.Interval = new TimeSpan(0, 0, 60);
                _dispatcherRackRateTimer.Start();
            }
        }

        private void SetComboBox()
        {
            // 라인
            SetEquipmentSegmentCombo(cboEquipmentSegment);

            // 공정
            SetProcessCombo(cboProcess);

            // 설비 그룹
            SetEquipmentGroup();

            // 극성
            SetPolarityCombo(cboPolarity);

            // 설비
            SetEquipmentCombo(cboEquipment);

            // 자동조회
            SetAutoSearchCombo(cboAutoSearch);

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboPolarity.SelectedValueChanged += cboPolarity_SelectedValueChanged;
            cboEqpGrp.SelectedValueChanged += cboEqpGrp_SelectedValueChanged;
            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            if ((string.Equals(cboProcess.SelectedValue?.GetString(), Process.NOTCHING) || string.Equals(cboProcess.SelectedValue?.GetString(), Process.LAMINATION)) && (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673")))   //GM2추가 2023-08-24  장영철
            {
                SelectRackRate();
            }
        }

        private void InitializeUserControls()
        {
            UcAssemblyCommand = grdCommand.Children[0] as UcAssemblyCommand;
            UcAssemblyEquipment = grdEquipment.Children[0] as UcAssemblyEquipment;
            UcAssemblyProductLot = grdProduct.Children[0] as UcAssemblyProductLot;
            UcAssemblyProductionResult = grdProductionResult.Children[0] as UcAssemblyProductionResult;

            SetIdentInfo();

            if (UcAssemblyCommand != null)
            {
                UcAssemblyCommand.FrameOperation = FrameOperation;
                UcAssemblyCommand.ProcessCode = _processCode;
                UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);
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
                UcAssemblyEquipment.LdrLotIdentBasCode = _ldrLotIdentBasCode;
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
                UcAssemblyProductLot.cboColor.SelectedIndex = 0;

                UcAssemblyProductLot.dgProductLot.PreviewMouseDoubleClick += DgProductLot_PreviewMouseDoubleClick;
                UcAssemblyProductLot.dgProductLotCommon.PreviewMouseDoubleClick += DgProductLot_PreviewMouseDoubleClick;
                UcAssemblyProductLot.dgProductLot.MouseLeftButtonUp += DgProductLot_MouseLeftButtonUp;
                UcAssemblyProductLot.dgProductLotCommon.MouseLeftButtonUp += DgProductLot_MouseLeftButtonUp;

                DgProductLot = _processCode == Process.NOTCHING ? UcAssemblyProductLot.dgProductLot : UcAssemblyProductLot.dgProductLotCommon;

                UcAssemblyProductLot.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcAssemblyProductLot.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                SetProductLotList();

                // 강성묵 : 20230404 레인번호 추가
                GetSlittingLaneNo();
            }

            if (UcAssemblyProductionResult != null)
            {
                UcAssemblyProductionResult.FrameOperation = FrameOperation;
                UcAssemblyProductionResult.EquipmentGroupCode = cboEqpGrp.GetStringValue(); // STACKING, FOLDING 구분을 위한 그룹 추가
                UcAssemblyProductionResult.ApplyPermissions();
            }
        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                if (_processCode == Process.NOTCHING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnRunCancel.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnRunCompleteCancel.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC공정신설
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.btnDefectSave.Visibility = Visibility.Collapsed; // 2023.11.21 E20231115-000860 문혜림 추가
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.STACKING_FOLDING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.PACKAGING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutPackagingTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdSpclTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }

                // 김용군 ESHM AZS_ECUTTER, AZS_STACKING 공정 추가에 따른 대응
                else if (_processCode == Process.AZS_ECUTTER)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.AZS_STACKING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }

                UcAssemblyEquipment.IsInputUseAuthority = false;
                UcAssemblyEquipment.IsWaitUseAuthority = false;
                UcAssemblyEquipment.IsInputHistoryAuthority = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEventInUserControls()
        {
            if (UcAssemblyCommand != null)
            {
                UcAssemblyCommand.btnWaitLot.Click += ButtonWaitLot_Click;                       //대기LOT조회
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

                UcAssemblyCommand.btnReturnCondition.Click += ButtonbtnReturnCondition_Click;        // 물류반송조건팝업
                // UcAssemblyCommand.btnUpdateLaneNo.Click += ButtonBtnUpdateLaneNo_Click;              // 슬리팅 레인 번호 변경 팝업
            }
        }

        private void SetProcessInUserControls()
        {
        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //this.RegisterName("redBrush", redBrush);
            //this.RegisterName("yellowBrush", yellowBrush);
            //HideRackRateMode();
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

            //ESMI 1동 조립(A4) 의 경우 동별 공통코드 체크하여 MES SFU Configurtion에서 셋팅된 라인정보가 아닌 '조립공정진척-6Line' 화면에 라인 콤보박스에 선택된 라인정보로 조회되게 수정
            if (IsEsmiLineCheck())
            {
                cboEquipmentSegment_SelectedValueChanged(null, null);
            }

            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);

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
            SetIdentInfo();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // 공정 
            SetProcessCombo(cboProcess);
            // Clear
            SetControlClear();

            HideAllRackRateMode();
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
            SetIdentInfo();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // Process.STACKING_FOLDING 공정일때 설비 그룹설정
            SetEquipmentGroup();

            // 설비 
            SetEquipmentCombo(cboEquipment);

            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);

            // ProductLot 영역의 컨트롤 Visibility 속성 정의
            UcAssemblyProductLot.SetControlVisibility();

            // 공정 변경에 따른 버튼 권한 조회
            GetButtonPermissionGroup();

            // NND 공정진척인 경우 극성을 보여주지 않음.
            SetPolarityByProcess();

            // Clear
            SetControlClear();

            HideAllRackRateMode();

            if ((string.Equals(cboProcess.SelectedValue?.GetString(), Process.NOTCHING) || string.Equals(cboProcess.SelectedValue?.GetString(), Process.LAMINATION)) && (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673")))   //GM2추가 2023-08-24  장영철
            {
                SelectRackRate();
            }

            // 강성묵 : 20230404 레인번호 추가
            GetSlittingLaneNo();

            // 2023.11.21 E20231115-000860 문혜림 : 라미 공정 불량/LOSS/물품청구 저장 버튼 숨김
            if (!string.Equals(cboProcess.SelectedValue?.GetString(), Process.LAMINATION)) UcAssemblyProductionResult.btnDefectSave.Visibility = Visibility.Visible;
        }

        private void cboPolarity_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            // 아래 SelectRackRate 호출 전 애니매이션 초기화
            // ShowRackRateMode 함수 내 gridrowIndex 설정값으로 인하여 idx < 4 설정
            for (int idx = 0; idx < 4; idx++)
                UcAssemblyProductLot.ProductLotContents.RowDefinitions[idx].BeginAnimation(RowDefinition.HeightProperty, null);

            // 극성 콤보박스 값 변경할때마다 조회되는 값 새로 load
            SelectRackRate();

            // 설비 cboPolarity
            SetEquipmentCombo(cboEquipment);

            // Clear
            SetControlClear();

            SetEquipment(SetEquipmentType.EquipmentComboChange);
            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
        }

        private void cboEqpGrp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            // 설비
            SetEquipmentCombo(cboEquipment);

            // Clear
            SetControlClear();

            SetEquipment(SetEquipmentType.EquipmentComboChange);
            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.IsEnabled = false;
            UcAssemblyProductLot.IsSearchResult = false;

            SetIdentInfo();
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

                DgProductLot = _processCode == Process.NOTCHING ? UcAssemblyProductLot.dgProductLot : UcAssemblyProductLot.dgProductLotCommon;

                // Product Lot 정렬
                DataTable dt = DataTableConverter.Convert(DgProductLot.ItemsSource);

                if (dt == null || dt.Rows.Count == 0) return;

                DataRow[] drSelect = dt.Select("EQPTID = '" + _treeEquipmentCode + "'");

                //해당 설비가 생산 Lot 정보가 없는 경우
                if (drSelect.Length == 0)
                {
                    if (_processCode == Process.NOTCHING)
                    {
                        DgProductLot.Refresh();
                        return;
                    }
                    else
                    {
                        // Sort 처리
                        dt = (from t in dt.AsEnumerable()
                              orderby t.Field<string>("EQPTID") ascending, t.Field<string>("WIPSTAT") descending, t.Field<string>("PR_LOTID") descending
                              select t).CopyToDataTable();

                        dt.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                        dt.AcceptChanges();

                        UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = false;
                        Util.GridSetData(DgProductLot, dt, null, true);
                        UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = true;
                        return;
                    }
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

                /*
                DataRow[] drNoneSelectEquipmentNull = dt.Select(" ISNULL(EQPTID,'') = '' ");
                DataRow[] drNoneSelect = dt.Select(" EQPTID <> '" + _treeEquipmentCode + "' AND ISNULL(EQPTID,'') <> '' ");

                DataTable dtSort = dt.Clone();

                for (int row = 0; row < drSelect.Length; row++)
                {
                    dtSort.ImportRow(drSelect[row]);
                }

                for (int row = 0; row < drNoneSelect.Length ; row++)
                {
                    dtSort.ImportRow(drNoneSelect[row]);
                }

                for (int row = 0; row < drNoneSelectEquipmentNull.Length; row++)
                {
                    dtSort.ImportRow(drNoneSelectEquipmentNull[row]);
                }

                // Equipment TreeClick 시 체크된 라디오 버튼 비활성화 처리
                dtSort.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dtSort.AcceptChanges();
                */

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

            if (!string.Equals(_processCode, Process.NOTCHING) && string.IsNullOrEmpty(drv["LOTID"].GetString())) return;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);

            if (dg.CurrentCell.Column.Name == "LOTID" && !string.IsNullOrEmpty(drv["LOTID"].GetString()))
            {
                //this.Cursor = Cursors.Wait;
                dg.CurrentCell.Presenter.Cursor = Cursors.Wait;

                HideAllRackRateMode();
                UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductionResult : UcAssemblyCommand.ButtonVisibilityType.CommonProductionResult);

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

            if (!string.Equals(_processCode, Process.NOTCHING) && string.IsNullOrEmpty(drv["LOTID"].GetString())) return;

            if (dg.Name == "dgProductLot")
                DgProductLot = UcAssemblyProductLot.dgProductLot;
            else if (dg.Name == "dgProductLotCommon")
                DgProductLot = UcAssemblyProductLot.dgProductLotCommon;

            RadioButton rb = DgProductLot.GetCell(DgProductLot.CurrentCell.Row.Index, DgProductLot.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;

            // 선택된 LOT을 작업 취소하시겠습니까?
            Util.MessageConfirm("SFU3151", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelProcess();
                }
            });
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UcAssemblyCommand.btnConfirm.IsEnabled = false;

                // WorkCalander 체크.
                GetWorkCalander();

                if (!ValidationConfirm()) return;
                if (!CheckEquipmentConfirmType("CONFIRM_W")) return;

                if (_processCode == Process.NOTCHING)
                {
                    // 작업조건/품질정보 입력 여부를 확인하여 미입력 시 등록 화면 호출 처리
                    string valueToLotId = _dvProductLot["LOTID"].GetString();

                    // 작업조건 등록 여부
                    if (Util.EQPTCondition(_processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonEqptCondSearch_Click(null, null);
                        return;
                    }

                    // 품질정보 등록 여부
                    if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, _processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonQualityInput_Click(null, null);
                        return;
                    }

                    // 설비 Loss 등록 여부 체크
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);
                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string info = string.Empty;
                        string lossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            info = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            lossInfo = lossInfo + "\n" + info;
                        }

                        object[] param = new object[] { lossInfo };

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
                else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)        // 20250428 ESHG DNC공정신설
                {
                    #region 불량 저장 체크
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // 저장하지 않은 불량 정보가 있습니다.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    #region 작업조건/품질정보 입력 여부
                    //string _ValueToLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    string valueToLotId = _dvProductLot["LOTID"].GetString();

                    // 작업조건 등록 여부
                    if (Util.EQPTCondition(_processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonEqptCond_Click(null, null);
                        return;
                    }
                    // 품질정보 등록 여부
                    if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, _processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonQualityInput_Click(null, null);
                        return;
                    }
                    #endregion

                    // 설비 Loss 등록 여부 체크
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string info = string.Empty;
                        string lossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            info = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            lossInfo = lossInfo + "\n" + info;
                        }

                        object[] param = new object[] { lossInfo };

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
                else if (_processCode == Process.STACKING_FOLDING)
                {
                    #region 불량 저장 체크
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // 저장하지 않은 불량 정보가 있습니다.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    // 설비 Loss 등록 여부 체크
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

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
                else if (_processCode == Process.PACKAGING)
                {
                    #region 불량 저장 체크
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // 저장하지 않은 불량 정보가 있습니다.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    #region 작업조건/품질정보 입력 여부
                    string valueToLotId = _dvProductLot["LOTID"].GetString();

                    // 작업조건 등록 여부
                    if (Util.EQPTCondition(_processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonEqptCond_Click(null, null);
                        return;
                    }
                    // 품질정보 등록 여부
                    if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, _processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonQualityInput_Click(null, null);
                        return;
                    }

                    #endregion

                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

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
                // 김용군 ESHM AZS_ECUTTER, AZS_STACKING 공정 추가에 따른 대응
                else if (_processCode == Process.AZS_ECUTTER)
                {
                    #region 불량 저장 체크
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // 저장하지 않은 불량 정보가 있습니다.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    // 설비 Loss 등록 여부 체크
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

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
                else if (_processCode == Process.AZS_STACKING)
                {
                    #region 불량 저장 체크
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // 저장하지 않은 불량 정보가 있습니다.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    // 설비 Loss 등록 여부 체크
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                UcAssemblyCommand.btnConfirm.IsEnabled = true;
            }
        }

        private void ButtonWaitLot_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationWaitLot()) return;

            PopupWaitLot();
        }

        private void ButtonRemarkHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemarkHistory()) return;

            PopupRemarkHistory();
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

        private void ButtonReworkMove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");


            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            CMM_COM_EQPCOMMENT popupEquipmentComment = new CMM_COM_EQPCOMMENT();
            popupEquipmentComment.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = idx < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID"));//_dvProductLot["LOTID"].GetString();
            parameters[4] = idx < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSEQ"));//_dvProductLot["WIPSEQ"].GetString();
            parameters[5] = UcAssemblyProductLot.txtSelectEquipmentName.Text;
            parameters[6] = Util.NVC(drShift[0]["VAL001"]);     //txtShift.Text; // 작업조명
            parameters[7] = Util.NVC(drShift[0]["SHFT_ID"]);    //txtShift.Tag; // 작업조코드
            parameters[8] = Util.NVC(drShift[0]["VAL002"]);     //txtWorker.Text; // 작업자명
            parameters[9] = Util.NVC(drShift[0]["WRK_USERID"]); //txtWorker.Tag; // 작업자 ID

            C1WindowExtension.SetParameters(popupEquipmentComment, parameters);
            //popupEquipmentComment.Closed += new EventHandler(popupEquipmentComment_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupEquipmentComment.ShowModal()));
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
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popup = sender as CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN;
            
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                    UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popup = sender as CMM_ELEC_WORK_HALF_SLITTING;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                    UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
        }

        /// <summary>
        /// 권취방향변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEmSectionRollDirctn_Click(object sender, RoutedEventArgs e)
        {
            PopupEmSectionRollDirctn();
        }

        private void PopupEmSectionRollDirctn_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_EM_SECTION_ROLL_DIRCTN popup = sender as CMM_ELEC_EM_SECTION_ROLL_DIRCTN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        private void ButtonRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart()) return;

            PopupRunStart();
        }

        //시작취소 버튼은 NND 공정진척인 경우에만 존재 함.
        private void ButtonRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunCancel()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });

        }

        private void ButtonRunComplete_Click(object sendeer, RoutedEventArgs e)
        {
            if (!ValidationRunComplete()) return;
            if (!CheckEquipmentConfirmType("EQPT_END_W")) return;

            PopupEquipmentEnd();
        }

        private void ButtonRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCompleteCancelRun()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunCompleteCancel();
                }
            });
        }

        private void ButtonCancelConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelConfirm()) return;

            PopupCancelConfirm();
        }

        private void ButtonSpclProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSpclProdMode()) return;

            bool isMode = GetSpclProdMode();

            string messageCode;

            if (isMode)
                messageCode = "SFU8309";    //[%1]을 특별관리 해제하시겠습니까?
            else
                messageCode = "SFU8308";    //[%1]을 특별관리 설정하시겠습니까?

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (SetSpclProdMode(!isMode) == false)
                    {
                        return;
                    }
                    Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                }
            }, UcAssemblyProductLot.txtSelectEquipmentName.Text);

        }

        private void ButtonMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLogMarge()) return;

            PopupLotMerge();
        }

        private void ButtonChgCarrier_Click(object sender, RoutedEventArgs e)
        {
            PopupChangeCarrier();
        }

        private void ButtonTrayInfo_Click(object sender, RoutedEventArgs e)
        {
            PopupTrayInfo();
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

        private void ButtonPilotProdSPMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPilotProdMode()) return;

            //bool isMode = GetPilotProdMode();
            string sFromMode = GetPilotProdMode();
            string messageCode;
            string sToMode = string.Empty;

            if (sFromMode.Equals("PILOT_S"))
            {
                messageCode = "SFU8188";    //[%1]을 시생산샘플 해제하시겠습니까?
                sToMode = string.Empty;
            }
            else
            {
                messageCode = "SFU8189";    //[%1]을 시생산샘플 설정하시겠습니까?
                sToMode = "PILOT_S";
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

        private void ButtonRework_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRework()) return;

            PopupReWork();
        }

        private void ButtonQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualityInput()) return;

            PopupQualityInput();
        }

        private void ButtonProductLot_Click(object sender, RoutedEventArgs e)
        {
            if (UcAssemblyProductionResult.IsProductLotRefreshFlag)
            {
                if (_dvProductLot != null)
                {
                    UcAssemblyProductLot.SelectProductList(_dvProductLot["LOTID"].GetString());
                }
                else
                {
                    UcAssemblyProductLot.SelectProductList();
                }

            }


            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);

            // 공정 변경에 따른 버튼 권한 조회
            GetButtonPermissionGroup();

            grdProduct.Visibility = Visibility.Visible;
            grdProductionResult.Visibility = Visibility.Collapsed;

            //if (UcAssemblyProductionResult.IsProductLotRefreshFlag)
            //    UcAssemblyProductLot.SelectProductList(_dvProductLot["LOTID"].GetString());

            UcAssemblyProductionResult.IsProductLotRefreshFlag = false;
            UcAssemblyProductionResult.InitializeControls();
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;
            PopupPrint();
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
                        UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
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

        private void DispatcherRackRateTimer_Tick(object sender, EventArgs e)
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

                    if (grdProduct.Visibility == Visibility.Visible)
                    {
                        if (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673"))   //GM2추가 2023-08-24  장영철
                        {
                            HideRackRateMode();
                            SelectRackRate();
                        }
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

        // <summary>
        /// 물류반송조건설정 팝업
        /// </summary>
        protected void ButtonbtnReturnCondition_Click(object sender, EventArgs e)
        {

            try
            {
                CMM_WORKORDER_DRB WO = new CMM_WORKORDER_DRB();

                WO._EqptSegment = _equipmentSegmentCode;
                WO._ProcID = _processCode;
                WO._EqptID = _equipmentCode;

                WO.GetWorkOrdeCallBack(() =>
                {
                    if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
                    {
                        Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                        return;
                    }
                    DataTable dt = WO.WOTable;

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        // %1가 없습니다.
                        Util.MessageValidation("SFU1444", ObjectDic.Instance.GetObjectName("Work Order"));
                        return;
                    }

                    CMM_CONDITION_RETURN popCondition = new CMM_CONDITION_RETURN() { FrameOperation = FrameOperation }; ;
                    if (popCondition != null)
                    {
                        UcAssemblyCommand.btnExtra.IsDropDownOpen = false;
                        object[] Parameters = new object[7];
                        Parameters[0] = _equipmentCode.ToString(); //설비코드
                        Parameters[1] = UcAssemblyProductLot.txtSelectEquipmentName.Text; // 설비코드명
                        Parameters[2] = dt.Rows[0]["PRJT_NAME"].ToString(); //PJT
                        Parameters[3] = dt.Rows[0]["PROD_VER_CODE"].ToString(); //Version
                        Parameters[4] = _processCode; //공정
                        Parameters[5] = "A"; //조립 화면에서 호출
                        Parameters[6] = dt.Rows[0]["WOID"].ToString();
                        C1WindowExtension.SetParameters(popCondition, Parameters);

                        popCondition.Closed += new EventHandler(popCondition_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => popCondition.ShowModal()));
                    }

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //// <summary>
        ///// 슬리팅 레인 번호 변경 팝업
        ///// </summary>
        //protected void ButtonBtnUpdateLaneNo_Click(object sender, EventArgs e)
        //{
        //    if (!ValidationUpdateLaneNo()) return;

        //    PopupUpdateLaneNo();
        //}

        private void popCondition_Closed(object sender, EventArgs e)
        {
            CMM_CONDITION_RETURN popup = sender as CMM_CONDITION_RETURN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 강성묵 : 20230404 레인번호 추가
                GetSlittingLaneNo();

                // 무지부 조회
                GetWorkHalfSlittingSide();
                UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                //string processLotId = SelectProcessLotId();

                ////SetProductLotList();
                //// 생산 Lot 재조회
                //SelectProductLot(processLotId);
                //SetProductLotList(processLotId);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private static void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            //ESMI-A4동 1~5 Line 제외처리
            //const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string bizRuleName = string.Empty;

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
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FIRST_PRIORITY_LINE_CBO";
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

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

            //A5000 : Notching,  A7000	A7000 : Lamination, A8000	A8000 : Stacking & Folding, A9000	A9000 : Packaging
            // string[] assemblyProcess = new string[] { "A5000", "A7000", "A7400", "A8400", "A8000", "A9000" };
            string[] assemblyProcess = new string[] { "A5000", "A5700", "A7000", "A7400", "A8400", "A8000", "A9000" };  // 20250428 ESHG DNC공정신설

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
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
            //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void SetPolarityCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELEC_TYPE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetAutoSearchCombo(C1ComboBox cbo)
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
                // 김용군 E-Cuttor 설비그룹 선택시 설비 셋팅
                if (_processCode == Process.AZS_ECUTTER)
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("EQSGID", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                    dr["PROCID"] = cboProcess.SelectedValue?.ToString();

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ECUTTER_CBO", "RQSTDT", "RSLTDT", inTable);

                    if (dtResult.Rows.Count != 0)
                    {
                        mcb.isAllUsed = false;
                        if (dtResult.Rows.Count == 1)
                        {
                            mcb.ItemsSource = DataTableConverter.Convert(dtResult);
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
                else
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("EQSGID", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("EQGRID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                    //dr["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();
                    dr["PROCID"] = cboProcess.SelectedValue?.ToString();

                    if (_processCode == Process.NOTCHING)
                    {
                        dr["ELTR_TYPE_CODE"] = string.IsNullOrWhiteSpace(cboPolarity.SelectedValue?.ToString()) ? null : cboPolarity.SelectedValue.ToString();
                    }

                    if (_processCode == Process.STACKING_FOLDING && cboEqpGrp.GetBindValue() != null)
                    {
                        dr["EQGRID"] = cboEqpGrp.GetBindValue();
                    }

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_CBO", "RQSTDT", "RSLTDT", inTable);

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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentGroup()
        {
            try
            {
                if (cboProcess.SelectedValue?.GetString() == Process.STACKING_FOLDING)
                {
                    cboEqpGrp.SelectedValueChanged -= cboEqpGrp_SelectedValueChanged;

                    string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "PROCID" };
                    string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID,
                    cboEquipmentSegment.GetStringValue(), cboProcess.GetStringValue() };
                    cboEqpGrp.SetDataComboItem("DA_BAS_SEL_EQUIPMENTGROUP_BY_PROCID_CBO", arrColumn, arrCondition);

                    cboEqpGrp.SelectedValueChanged += cboEqpGrp_SelectedValueChanged;

                    if (cboEqpGrp.Items.Count > 1)
                    {
                        tbEqpGrp.Visibility = Visibility.Visible;
                        cboEqpGrp.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tbEqpGrp.Visibility = Visibility.Collapsed;
                        cboEqpGrp.Visibility = Visibility.Collapsed;
                    }

                    tbEqpGrp.Visibility = Visibility.Visible;
                    cboEqpGrp.Visibility = Visibility.Visible;
                }
                else
                {
                    cboEqpGrp.ItemsSource = null;

                    tbEqpGrp.Visibility = Visibility.Collapsed;
                    cboEqpGrp.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetIdentInfo()
        {
            const string bizRuleName = "DA_EQP_SEL_LOT_IDENT_BAS_CODE";
            try
            {
                _ldrLotIdentBasCode = string.Empty;
                _unldrLotIdentBasCode = string.Empty;

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = _equipmentSegmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    _ldrLotIdentBasCode = dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _unldrLotIdentBasCode = dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
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
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID; //"CNSSOBAKTOP"; //
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = _processCode;
                dtRow["EQGRID"] = _processCode == Process.STACKING_FOLDING ? "STK" : null;  // STACKING, FOLDING 공정진척의 경우 동일한 PROCID를 사용함으로, EQGRID 로 분기처리가 필요 함.
                inTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("BTN_PMS_GRP_CODE"))
                    {
                        foreach (DataRow drTmp in dtResult.Rows)
                        {
                            if (drTmp == null) continue;

                            if (_processCode == Process.NOTCHING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // 투입 사용 권한
                                    case "WAIT_W": // 대기 사용 권한
                                    case "INPUTHIST_W": // 투입이력 사용 권한 TODO : CNB2동 조립공정진척의 경우 기존화면과 상이하여 투입, 대기, 투입이력에 권한 재정의 필요 함.
                                        //SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // 생산반제품 사용 권한
                                        //grdOutTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // 작업시작 사용 권한
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_LOTSTART_W": // 작업시작 취소 권한
                                        UcAssemblyCommand.btnRunCancel.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_EQPT_END_W": // 장비완료 취소 권한
                                        UcAssemblyCommand.btnRunCompleteCancel.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // 시생산 샘플 설정 권한
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }

                            else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC공정신설
                            {
                                UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;   // 20250428 ESHG DNC공정신설
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // 투입 사용 권한
                                    case "WAIT_W": // 대기 사용 권한
                                    case "INPUTHIST_W": // 투입이력 사용 권한
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"])); //TODO : AS-IS 상에는 탭의 컨트롤의 권한 부여를 하였으나 현재는 그리드에 적용됨으로 추후 변경 작업이 필요 함.
                                        break;
                                    case "OUTPUT_W": // 생산반제품 사용 권한
                                        UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // 작업시작 사용 권한
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // 발행 사용권한
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // 시생산 샘플 설정 권한
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            else if (_processCode == Process.STACKING_FOLDING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // 투입 사용 권한
                                    case "WAIT_W": // 대기 사용 권한
                                    case "INPUTHIST_W": // 투입이력 사용 권한
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // 생산반제품 사용 권한
                                        UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // 작업시작 사용 권한
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // 발행 사용권한
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // 시생산 샘플 설정 권한
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            else if (_processCode == Process.PACKAGING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // 투입 사용 권한
                                    case "WAIT_W": // 대기 사용 권한
                                    case "INPUTHIST_W": // 투입이력 사용 권한
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // 생산반제품 사용 권한
                                        UcAssemblyProductionResult.grdOutPackagingTranBtn.Visibility = Visibility.Visible;
                                        UcAssemblyProductionResult.grdSpclTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // 작업시작 사용 권한
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // 시생산 샘플 설정 권한
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            // 김용군 AZS_ECUTTER, AZS_STACKING 공정 권한
                            else if (_processCode == Process.AZS_ECUTTER)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // 투입 사용 권한
                                    case "WAIT_W": // 대기 사용 권한
                                    case "INPUTHIST_W": // 투입이력 사용 권한
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"])); //TODO : AS-IS 상에는 탭의 컨트롤의 권한 부여를 하였으나 현재는 그리드에 적용됨으로 추후 변경 작업이 필요 함.
                                        break;
                                    case "OUTPUT_W": // 생산반제품 사용 권한
                                        UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // 작업시작 사용 권한
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // 발행 사용권한
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // 시생산 샘플 설정 권한
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            else if (_processCode == Process.AZS_STACKING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // 투입 사용 권한
                                    case "WAIT_W": // 대기 사용 권한
                                    case "INPUTHIST_W": // 투입이력 사용 권한
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // 생산반제품 사용 권한
                                        UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // 작업시작 사용 권한
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // 확정취소 사용 권한
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // 발행 사용권한
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // 시생산 샘플 설정 권한
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
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

        }

        private void SetPermissionPerInputButton(string sBtnPermissionGrpCode)
        {
            if (UcAssemblyEquipment == null)
                return;

            UcAssemblyEquipment.SetPermissionPerButton(sBtnPermissionGrpCode);
        }

        private string GetMaxChildGRPSeq(string parentLot)
        {
            if (string.IsNullOrEmpty(parentLot)) return string.Empty;

            try
            {
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PR_LOTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = parentLot;
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CHILDGRSEQ", "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    return Util.NVC(searchResult.Rows[0]["CHILD_GR_SEQNO"]);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private void GetWorkCalander()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode; //Process.NOTCHING;
                inTable.Rows.Add(newRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORK_CALENDAR_WRKR_INFO", "RQSTDT", "RSLTDT", inTable);

                if (result.Rows.Count > 0)
                {
                    //DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                    dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                    {
                        r["SHFT_ID"] = result.Rows[0]["SHFT_ID"];
                        r["VAL001"] = result.Rows[0]["SHFT_NAME"];
                        r["WRK_USERID"] = result.Rows[0]["WRK_USERID"];
                        r["VAL002"] = result.Rows[0]["WRK_USERNAME"];
                        r["WRK_STRT_DTTM"] = result.Rows[0]["WRK_STRT_DTTM"];
                        r["WRK_END_DTTM"] = result.Rows[0]["WRK_END_DTTM"];
                    });
                    dt.AcceptChanges();

                }
                else
                {
                    DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                    dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                    {
                        r["SHFT_ID"] = string.Empty;
                        r["VAL001"] = string.Empty;
                        r["WRK_USERID"] = string.Empty;
                        r["VAL002"] = string.Empty;
                        r["WRK_STRT_DTTM"] = string.Empty;
                        r["WRK_END_DTTM"] = string.Empty;
                    });
                    dt.AcceptChanges();

                    #region 작업조,작업자 등록 화면 변경 요청 건 [C20200511-000024]
                    if (_selectUserModeAreaList.Contains(LoginInfo.CFG_AREA_ID))
                    {
                        GetEquipmentWorkInfo();
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

        private void SelectEquipmentMountPosition()
        {
            _equipmentMountPositionCode = string.Empty;

            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO_L";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["MOUNT_MTRL_TYPE_CODE"] = "PROD";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _equipmentMountPositionCode = dtResult.Rows[0]["CBO_CODE"].GetString();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
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

        private void CancelProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUT_CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                ///////////////////////////////////////////////////////////// Process.SLITTING || Process.SRS_SLITTING 사용
                DataTable InPrLot = inDataSet.Tables.Add("IN_PRLOT");
                InPrLot.Columns.Add("PR_LOTID", typeof(string));
                InPrLot.Columns.Add("CUT_ID", typeof(string));
                InPrLot.Columns.Add("CSTID", typeof(string));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Lot 정보 그리드 만큼 Loop ??

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
                {
                    if (_dvProductLot["CSTID"] != null)
                    {
                        //CSTID가 NULL인 경우에는 오류가 발생하여 Util.NVC를 사용함.
                        newRow["CSTID"] = Util.NVC(_dvProductLot["CSTID"]);
                    }
                }
                if (_unldrLotIdentBasCode == "CST_ID" || _unldrLotIdentBasCode == "RF_ID")
                {
                    if (_dvProductLot["OUT_CSTID"] != null)
                    {
                        newRow["OUT_CSTID"] = Util.NVC(_dvProductLot["OUT_CSTID"]);
                    }
                }
                newRow["USERID"] = LoginInfo.USERID;
                newRow["INPUT_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_MX", "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    // 정상 처리 되었습니다.
                    Util.MessageInfo("SFU1275");

                    // 생산 Lot 재조회
                    SelectProductLot(string.Empty);
                    SetProductLotList();

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

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
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PR_LOTID"));
                inInput.Rows.Add(newRow);

                newRow = inOutput.NewRow();
                newRow["OUTPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID"));
                newRow["OUTPUT_CSTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "CSTID"));
                inOutput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_NT_CSTID", "IN_EQP,IN_INPUT,IN_OUTPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
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
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void RunCompleteCancel()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_NT_L";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["INPUT_LOTID"] = _dvProductLot["PR_LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
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

        private void Confirm_Process()
        {

            if (CheckModelChange() && !CheckInputEqptCond())
            {
                //해당 LOT에 작업조건이 등록되지 않았습니다.\n실적확정 하시겠습니까?
                Util.MessageConfirm("SFU2817", (result2) =>
                {
                    if (result2 == MessageBoxResult.OK)
                    {
                        //ConfirmProcess();
                        CheckNgTag();   // 2024.02.20 윤지해 E20240130-000700 전극/노칭(NND) 공정 NG TAG 수량 비교 팝업
                    }
                });
            }
            else
            {
                //ConfirmProcess();
                CheckNgTag();   // 2024.02.20 윤지해 E20240130-000700 전극/노칭(NND) 공정 NG TAG 수량 비교 팝업
            }
        }

        /// <summary>
        /// 2021.08.12 : 시생산샘플설정/해제 기능 추가
        /// return bool에서 string으로 변경
        /// </summary>
        /// <returns></returns>
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

        private bool GetSpclProdMode()
        {
            try
            {
                if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
                    return false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(dr);
                //2024.01.23  남재현: Package 공정 시 특별 관리 여부 컬럼 SPCL_MNGT_FLAG 이용.
                if (_processCode == Process.PACKAGING)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_MNGT_FLAG", "INDATA", "OUTDATA", inTable);
                    if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("SPCL_MNGT_FLAG"))
                    {
                        if (Util.NVC(dtRslt.Rows[0]["SPCL_MNGT_FLAG"]).Equals("Y"))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR", "INDATA", "OUTDATA", inTable);

                    if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("SPCL_LOT_GNRT_FLAG"))
                    {
                        if (Util.NVC(dtRslt.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y"))
                        {
                            return true;
                        }
                    }
                }
                return false;
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
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SPCL_LOT_GNRT_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["SPCL_LOT_GNRT_FLAG"] = bMode ? "Y" : "N";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_SPCL_LOT_INPUT", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetEquipmentWorkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["PROCID"] = _processCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
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
                            DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                            dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                            {
                                r["SHFT_ID"] = result.Rows[0]["SHFT_ID"];
                                r["VAL001"] = result.Rows[0]["SHFT_NAME"];
                                r["WRK_USERID"] = result.Rows[0]["WRK_USERID"];
                                r["VAL002"] = result.Rows[0]["WRK_USERNAME"];
                                r["WRK_STRT_DTTM"] = result.Rows[0]["WRK_STRT_DTTM"];
                                r["WRK_END_DTTM"] = result.Rows[0]["WRK_END_DTTM"];
                            });
                            dt.AcceptChanges();
                        }
                        else
                        {
                            DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                            dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                            {
                                r["SHFT_ID"] = string.Empty;
                                r["VAL001"] = string.Empty;
                                r["WRK_USERID"] = string.Empty;
                                r["VAL002"] = string.Empty;
                                r["WRK_STRT_DTTM"] = string.Empty;
                                r["WRK_END_DTTM"] = string.Empty;
                            });
                            dt.AcceptChanges();
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
                Util.MessageException(ex);
            }
        }

        private void AutoPrint(bool bQASample)
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

                        // 2017-07-04 Lee. D. R
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

                        if (bQASample)
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

        private bool CheckModelChange()
        {
            bool bRet = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROCID"] = _processCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LAST_EQP_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("PRJT_NAME"))
                    {
                        string productProjectName = _dvProductLot["PRJT_NAME"].GetString();
                        string searchProjectName = Util.NVC(dtResult.Rows[0]["PRJT_NAME"]);
                        if (searchProjectName.Length > 1 && searchProjectName.Equals(productProjectName))
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
        }

        private bool CheckInputEqptCond()
        {
            try
            {
                bool bRet = false;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_EQPT_CLCT_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_CNT", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtResult.Rows[0]["CNT"]);
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
        }

        private bool CheckProdQtyChangePermission()
        {
            try
            {
                bool bRet = false;

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

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
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

        // 2024.02.20 윤지해 E20240130-000700 전극/노칭(NND) 공정 NG TAG 수량 비교 팝업
        private void CheckNgTag()
        {
            if (string.Equals(_processCode, Process.NOTCHING))
            {
                const string bizRuleName = "BR_PRD_CHK_NG_TAG_CNT";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["OUT_LOTID"] = _dvProductLot["LOTID"].GetString();
                dr["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                object[] parameters = new object[2];
                parameters[0] = dtRslt.Rows[0]["NND_DFCT_QTY"];
                parameters[1] = dtRslt.Rows[0]["ELEC_DFCT_QTY"];

                if (dtRslt.Rows[0]["RSLT_FLAG"].Equals("Y"))
                {
                    Util.MessageValidation("SFU3676", (action) =>
                    {
                        ConfirmProcess();
                    }, parameters);
                }
                else
                {
                    ConfirmProcess();
                }
            }
            else
            {
                ConfirmProcess();
            }
        }

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            /*
            if (UcElectrodeCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
            */
        }

        private void SetPolarityByProcess()
        {
            if (cboProcess.SelectedValue?.GetString() == Process.NOTCHING)
            {
                tbPolarity.Visibility = Visibility.Visible;
                cboPolarity.Visibility = Visibility.Visible;
                UcAssemblyProductLot.chkWait.Visibility = Visibility.Visible;
                UcAssemblyProductLot.chkWait.IsChecked = false;

            }
            else
            {
                tbPolarity.Visibility = Visibility.Collapsed;
                cboPolarity.Visibility = Visibility.Collapsed;
                UcAssemblyProductLot.chkWait.Visibility = Visibility.Collapsed;
            }

            if (UcAssemblyProductLot.cboColor.Items.Count > 0)
                UcAssemblyProductLot.cboColor.SelectedIndex = 0;
        }

        private void SetControlClear()
        {
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            _productLot = string.Empty;
            _equipmentMountPositionCode = string.Empty;
            _dvProductLot = null;

            Util.gridClear(UcAssemblyEquipment.DgEquipment);

            //Util.gridClear(UcAssemblyProductLot.DgProductLot);
            //Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            //Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);

            //UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            //UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            //UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
        }

        private void SetControlClearByProcess()
        {
            Util.gridClear(UcAssemblyEquipment.DgEquipment);
            Util.gridClear(UcAssemblyProductLot.dgProductLot);
            Util.gridClear(UcAssemblyProductLot.dgProductLotCommon);
            Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);
            UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
            UcAssemblyProductionResult.SetControlClear();

            // 강성묵 : 20230404 레인번호 추가
            UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;
        }

        private void SetEquipmentTreeControlClear()
        {
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            Util.gridClear(UcAssemblyEquipment.DgEquipment);
        }

        private void SetEquipmentProductLotControlClear()
        {
            Util.gridClear(UcAssemblyProductLot.DgProductLot);
            Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);

            UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            UcAssemblyProductLot.txtSelectLot.Text = string.Empty;

            // 강성묵 : 20230404 레인번호 추가
            UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;
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
            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);

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
            UcAssemblyEquipment.EquipmentGroupCode = cboEqpGrp.GetStringValue(); // STACKING, FOLDING 구분을 위한 그룹 추가

            if (_dvProductLot != null)
            {
                UcAssemblyEquipment.ProductLotID = _dvProductLot["LOTID"].GetString();
                UcAssemblyEquipment.SelectEquipmentCode = _dvProductLot["EQPTID"].GetString();
            }
            else
            {
                UcAssemblyEquipment.ProductLotID = string.Empty;
                UcAssemblyEquipment.SelectEquipmentCode = string.Empty;
            }
        }

        private void SetUserControlProductLot()
        {
            UcAssemblyProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyProductLot.ProcessCode = _processCode;
            UcAssemblyProductLot.ProcessName = _processName;
            UcAssemblyProductLot.EquipmentCode = cboEquipment.SelectedItemsToString;
            // 설비영역 선택에 따른 UcAssemblyProductLot UserControl에 EquipmentCode 전달 수정 함.
            //UcAssemblyProductLot.EquipmentCode = _equipmentCode;
            UcAssemblyProductLot.LdrLotIdentBasCode = _ldrLotIdentBasCode;
            UcAssemblyProductLot.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
        }

        public void SetUserControlProductionResult()
        {
            if (string.IsNullOrWhiteSpace(_equipmentCode) || _equipmentCode.Split(',').Length > 1) return;

            DataTable dt = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "'").CopyToDataTable();

            UcAssemblyProductionResult.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyProductionResult.EquipmentSegmentName = _equipmentSegmentName;
            UcAssemblyProductionResult.ProcessCode = _processCode;
            UcAssemblyProductionResult.ProcessName = _processName;
            UcAssemblyProductionResult.EquipmentCode = _equipmentCode;
            UcAssemblyProductionResult.EquipmentName = _equipmentName;
            UcAssemblyProductionResult.DtEquipment = dt;
            UcAssemblyProductionResult.DvProductLot = _dvProductLot;
            UcAssemblyProductionResult.LdrLotIdentBasCode = _ldrLotIdentBasCode;
            UcAssemblyProductionResult.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
            UcAssemblyProductionResult.EquipmentGroupCode = cboEqpGrp.GetStringValue(); // STACKING, FOLDING 구분을 위한 그룹 추가
        }

        public void SetProductLotSelect(RadioButton rb)
        {
            int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

            for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
            }

            DgProductLot = UcAssemblyProductLot.DgProductLot;

            DgProductLot.SelectedIndex = idx;

            // 라디오 버튼 선택에 따른 DataRowView 생성
            _dvProductLot = DgProductLot.Rows[idx].DataItem as DataRowView;
            if (_dvProductLot == null) return;

            if (_processCode == Process.NOTCHING)
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
                            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductionResult : UcAssemblyCommand.ButtonVisibilityType.CommonProductionResult);

                            // 공정 변경에 따른 버튼 권한 조회
                            GetButtonPermissionGroup();

                            grdProduct.Visibility = Visibility.Collapsed;
                            grdProductionResult.Visibility = Visibility.Visible;
                            _equipmentCode = _dvProductLot["EQPTID"].GetString();
                            //SetUserControlProductionResult();
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

        private void SetEquipmentTree()
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
            //if (grdProduct.Visibility == Visibility.Collapsed) return;

            UcAssemblyProductLot.SetControlVisibility();

            if (cboEquipment.SelectedItems.Any())
            {
                SetUserControlProductLot();
                UcAssemblyProductLot.SelectProductList(processLotId);
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

                // 무지부 방향 조회(동별 공통 : MNG_SLITTING_SIDE_AREA)
                if (UcAssemblyCommand.IsManageSlittingSide)
                {
                    GetWorkHalfSlittingSide();
                }

                SelectEquipmentMountPosition();

                // 강성묵 : 20230404 레인번호 추가
                GetSlittingLaneNo();
            }
            else
            {
                if (grdProduct.Visibility == Visibility.Visible)
                {
                    UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
                    UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
                    UcAssemblyProductLot.txtSelectLot.Text = string.Empty;

                    // 강성묵 : 20230404 레인번호 추가
                    UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;
                }
            }
            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SelectProductLot(string productLotId)
        {
            if (string.IsNullOrEmpty(productLotId))
            {
                if (_processCode == Process.NOTCHING)
                {
                    if (_util.GetDataGridFirstRowIndexByCheck(DgProductLot, "CHK") < 0)
                        _dvProductLot = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(productLotId)) _dvProductLot = null;
                }
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

        private void ConfirmProcess()
        {
            try
            {
                if (_processCode == Process.NOTCHING)
                {
                    ASSY005_001_CONFIRM popupConfirm = new ASSY005_001_CONFIRM();
                    popupConfirm.FrameOperation = FrameOperation;

                    DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    object[] parameters = new object[14];
                    parameters[0] = Process.NOTCHING;
                    parameters[1] = _equipmentSegmentCode;
                    parameters[2] = _equipmentCode;
                    parameters[3] = _dvProductLot["LOTID"].GetString();
                    parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                    parameters[5] = _ldrLotIdentBasCode;
                    parameters[6] = _unldrLotIdentBasCode;
                    parameters[7] = _equipmentMountPositionCode;

                    parameters[8] = Util.NVC(drShift[0]["VAL001"]);         //Util.NVC(txtShift.Text);
                    parameters[9] = Util.NVC(drShift[0]["SHFT_ID"]);        //Util.NVC(txtShift.Tag);
                    parameters[10] = Util.NVC(drShift[0]["VAL002"]);        //Util.NVC(txtWorker.Text);
                    parameters[11] = Util.NVC(drShift[0]["WRK_USERID"]);    //Util.NVC(txtWorker.Tag);
                    parameters[12] = CheckProdQtyChangePermission();
                    parameters[13] = _dvProductLot["QA_INSP_TRGT_FLAG"].GetString();

                    C1WindowExtension.SetParameters(popupConfirm, parameters);

                    popupConfirm.Closed -= popupConfirm_Closed;
                    popupConfirm.Closed += popupConfirm_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupConfirm.ShowModal()));
                }
                else
                {

                    ASSY005_COM_CONFIRM popupConfirm = new ASSY005_COM_CONFIRM { FrameOperation = FrameOperation };

                    DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    object[] parameters = new object[13];
                    parameters[0] = _processCode;
                    parameters[1] = _equipmentSegmentCode;
                    parameters[2] = _equipmentCode;
                    parameters[3] = _dvProductLot["LOTID"].GetString();
                    parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                    parameters[5] = _ldrLotIdentBasCode;
                    parameters[6] = _unldrLotIdentBasCode;
                    parameters[7] = Util.NVC(drShift[0]["VAL001"]);         //Util.NVC(txtShift.Text);
                    parameters[8] = Util.NVC(drShift[0]["SHFT_ID"]);        //Util.NVC(txtShift.Tag);
                    parameters[9] = Util.NVC(drShift[0]["VAL002"]);         //Util.NVC(txtWorker.Text);
                    parameters[10] = Util.NVC(drShift[0]["WRK_USERID"]);    //Util.NVC(txtWorker.Tag);
                    parameters[11] = CheckProdQtyChangePermission();
                    parameters[12] = "N";

                    C1WindowExtension.SetParameters(popupConfirm, parameters);
                    popupConfirm.Closed += popupConfirm_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupConfirm.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void SelectRackRate()
        {
            if (string.Equals(_processCode, Process.NOTCHING))
            {
                //if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text)) return;

                const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_PER_ELTR_TYPE_STO";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "NPW";   //노칭완성창고
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        if (cboPolarity.SelectedIndex != 2)
                        {
                            //NND 양극 : NND-LNS (+) STO #1
                            const string cathodeEquipmentCode = "U1ASTO13201";

                            var querycathode = (from t in bizResult.AsEnumerable()
                                                where t.Field<string>("EQPTID") == cathodeEquipmentCode
                                                select new
                                                {
                                                    EquipmentCode = t.Field<string>("EQPTID"),
                                                    RackRate = t.Field<long>("RACK_RATE"),
                                                    EquipmentName = t.Field<string>("EQPTNAME")
                                                }).FirstOrDefault();

                            if (querycathode != null)
                            {
                                _rackState = RackRateDifferenceValue(querycathode.EquipmentCode, querycathode.RackRate);
                                string rackRateText = RackRateDifferenceValueText(querycathode.EquipmentCode, querycathode.RackRate);

                                ShowRackRateMode(_rackState, querycathode.EquipmentName, rackRateText + "%", "C");
                            }
                        }
                        if (cboPolarity.SelectedIndex != 1)
                        {
                            //NND 음극 : NND - LNS(-) STO #1
                            const string anodeEquipmentCode = "U1ASTO13101";

                            var queryAnode = (from t in bizResult.AsEnumerable()
                                              where t.Field<string>("EQPTID") == anodeEquipmentCode
                                              select new
                                              {
                                                  EquipmentCode = t.Field<string>("EQPTID"),
                                                  RackRate = t.Field<long>("RACK_RATE"),
                                                  EquipmentName = t.Field<string>("EQPTNAME"),
                                              }).FirstOrDefault();

                            // querycathode -> queryAnode 변경
                            if (queryAnode != null)
                            {
                                _rackState = RackRateDifferenceValue(queryAnode.EquipmentCode, queryAnode.RackRate);
                                string rackRateText = RackRateDifferenceValueText(queryAnode.EquipmentCode, queryAnode.RackRate);

                                ShowRackRateMode(_rackState, queryAnode.EquipmentName, rackRateText + "%", "A");
                            }
                        }
                    }
                });
            }
            else if (string.Equals(_processCode, Process.DNC) || string.Equals(_processCode, Process.LAMINATION))       // 20250428 ESHG DNC공정신설 
            {
                HideAllRackRateMode();

                const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_PER_ELTR_TYPE_STO";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "LWW";   //라미완성창고
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        string sDangerMsgEqptName = string.Empty;
                        string sDangerMsgRackRate = string.Empty;
                        string sWarningMsgEqptName = string.Empty;
                        string sWarningMsgRackRate = string.Empty;

                        for (int idx = 0; idx < bizResult.Rows.Count; idx++)
                        {
                            if (bizResult.Columns.Contains("EQPTID") && bizResult.Columns.Contains("EQPTNAME") && bizResult.Columns.Contains("RACK_RATE"))
                            {
                                string sEqptCode = Util.NVC(bizResult.Rows[idx]["EQPTID"]);
                                string sEqptName = Util.NVC(bizResult.Rows[idx]["EQPTNAME"]);
                                decimal dRackRate = Util.NVC_Decimal(bizResult.Rows[idx]["RACK_RATE"]);

                                _rackState = RackRateDifferenceValue(sEqptCode, dRackRate);
                                string rackRateText = RackRateDifferenceValueText(sEqptCode, dRackRate);
                                if (_rackState == SetRackState.Danger)
                                {
                                    sDangerMsgEqptName = string.IsNullOrEmpty(sDangerMsgEqptName) ? sEqptName : sDangerMsgEqptName + ", " + sEqptName;
                                    sDangerMsgRackRate = string.IsNullOrEmpty(sDangerMsgRackRate) ? rackRateText + "%" : sDangerMsgRackRate + ", " + rackRateText + "%";
                                }
                                else if (_rackState == SetRackState.Warning)
                                {
                                    sWarningMsgEqptName = string.IsNullOrEmpty(sWarningMsgEqptName) ? sEqptName : sWarningMsgEqptName + ", " + sEqptName;
                                    sWarningMsgRackRate = string.IsNullOrEmpty(sWarningMsgRackRate) ? rackRateText + "%" : sWarningMsgRackRate + ", " + rackRateText + "%";
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(sDangerMsgEqptName))
                            ShowRackRateMode(SetRackState.Danger, sDangerMsgEqptName, sDangerMsgRackRate, "C");

                        if (!string.IsNullOrEmpty(sWarningMsgEqptName))
                            ShowRackRateMode(SetRackState.Warning, sWarningMsgEqptName, sWarningMsgRackRate, "A");
                    }
                });
            }
        }

        private SetRackState RackRateDifferenceValue(string equipmentCode, decimal rackRate)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_PRDT_WH";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("WH_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["WH_ID"] = equipmentCode;
            inTable.Rows.Add(dr);

            DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            if (CommonVerify.HasTableRow(bizResult))
            {
                if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 5 <= rackRate)
                {
                    return SetRackState.Danger;
                }
                else if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 10 <= rackRate)
                {
                    return SetRackState.Warning;
                }
                else
                {
                    return SetRackState.Normal;
                }
            }

            return SetRackState.Normal;
        }

        private string RackRateDifferenceValueText(string equipmentCode, decimal rackRate)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_PRDT_WH";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("WH_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["WH_ID"] = equipmentCode;
            inTable.Rows.Add(dr);

            DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            if (CommonVerify.HasTableRow(bizResult))
            {
                if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 5 <= rackRate)
                {
                    return $"{(bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 5):###,###,###,##0.##}";
                }
                else if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 10 <= rackRate)
                {
                    return $"{(bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 10):###,###,###,##0.##}";
                }
                else
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }


        private void HideAllRackRateMode()
        {
            if (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673"))   //GM2추가 2023-08-24  장영철
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 3; i > 1; i--)
                    {
                        if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[i].Height.Value <= 0) continue;

                        GridLengthAnimation gla = new GridLengthAnimation
                        {
                            From = new GridLength(0.1, GridUnitType.Star),
                            To = new GridLength(0, GridUnitType.Star),
                            AccelerationRatio = 0.8,
                            DecelerationRatio = 0.2,
                            Duration = new TimeSpan(0, 0, 0, 0, 500)
                        };
                        gla.Completed += HideTestAnimationCompleted;
                        UcAssemblyProductLot.ProductLotContents.RowDefinitions[i].BeginAnimation(RowDefinition.HeightProperty, gla);
                    }
                    _isRackRateMode = false;
                }));
            }
        }

        private void HideRackRateMode()
        {
            if (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673"))   // GM2 추가   2023-08-24  장영철
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 3; i > 1; i--)
                    {
                        if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[i].Height.Value > 0)
                        {
                            GridLengthAnimation gla = new GridLengthAnimation
                            {
                                From = new GridLength(0.1, GridUnitType.Star),
                                To = new GridLength(0, GridUnitType.Star),
                                AccelerationRatio = 0.8,
                                DecelerationRatio = 0.2,
                                Duration = new TimeSpan(0, 0, 0, 0, 500)
                            };
                            gla.Completed += HideTestAnimationCompleted;
                            UcAssemblyProductLot.ProductLotContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
                        }
                    }
                    _isRackRateMode = false;
                }));
            }
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void HideRackRateAnimationCompleted(int i)
        {
            if (i == 3)
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                UcAssemblyProductLot.txtCathodeRackRateMode.Text = string.Empty;
            }
            else
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                UcAssemblyProductLot.txtAnodeRackRateMode.Text = string.Empty;
            }
        }

        private void ShowRackRateMode(SetRackState rackState, string equipmentName, string rackRate, string polarityCode)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                int gridrowIndex;
                if (polarityCode.Equals("C"))
                    gridrowIndex = 2;
                else
                    gridrowIndex = 3;

                if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].Height.Value > 0)
                {
                    //그리드 Row Height 강제 설정
                    GridLengthAnimation lengthAnimation = new GridLengthAnimation
                    {
                        From = new GridLength(0.1, GridUnitType.Star),
                        To = new GridLength(0, GridUnitType.Star),
                        AccelerationRatio = 0.8,
                        DecelerationRatio = 0.2,
                        Duration = new TimeSpan(0, 0, 0, 0, 500)
                    };
                    UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].BeginAnimation(RowDefinition.HeightProperty, lengthAnimation);
                }

                if (rackState == SetRackState.Normal) return;
                //if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].Height.Value > 0) return;

                string messageCode = string.Empty;

                if (rackState == SetRackState.Danger)
                    messageCode = "SFU8891";
                else if (rackState == SetRackState.Warning)
                    messageCode = "SFU8890";

                object[] parameters = new object[2];
                parameters[0] = equipmentName;
                parameters[1] = rackRate;

                if (polarityCode.Equals("C"))
                {
                    UcAssemblyProductLot.txtCathodeRackRateMode.Text = GetMessage(messageCode, parameters);
                }
                else
                {
                    UcAssemblyProductLot.txtAnodeRackRateMode.Text = GetMessage(messageCode, parameters);
                }

                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(0.1, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };

                if (polarityCode.Equals("C"))
                {
                    gla.Completed += (sender, e) => { ShowCathodeAnimationCompleted(rackState); };
                }
                else
                {
                    gla.Completed += (sender, e) => { ShowAnodeAnimationCompleted(rackState); };
                }

                UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].BeginAnimation(RowDefinition.HeightProperty, gla);

            }));

            _isRackRateMode = true;
        }

        private void ShowCathodeAnimationCompleted(SetRackState rackState)
        {
            string name;
            if (rackState == SetRackState.Danger)
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = redBrush;
                name = "redBrush";
            }
            else if (rackState == SetRackState.Warning)
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = yellowBrush;
                name = "yellowBrush";
            }
            else
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                name = string.Empty;
                return;
            }

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                //Duration = TimeSpan.FromSeconds(0.8),
                Duration = TimeSpan.FromSeconds(2.0),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, name);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);
            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private void ShowAnodeAnimationCompleted(SetRackState rackState)
        {
            string name;
            if (rackState == SetRackState.Danger)
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = redBrush;
                name = "redBrush";
            }
            else if (rackState == SetRackState.Warning)
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = yellowBrush;
                name = "yellowBrush";
            }
            else
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                name = string.Empty;
                return;
            }

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                //Duration = TimeSpan.FromSeconds(0.8),
                Duration = TimeSpan.FromSeconds(2.0),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, name);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);
            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private string GetMessage(string messageId, params object[] parameters)
        {
            if (string.IsNullOrEmpty(messageId)) return string.Empty;

            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            return message;
        }


        #endregion

        #region[[Validation]

        private bool ValidationWaitLot()
        {
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

            return true;
        }

        private bool ValidationRemarkHistory()
        {

            // NND 공정진척에서만 사용 함.
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
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
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_processCode == Process.NOTCHING)
            {
                if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIP_TYPE_CODE")).Equals("OUT"))
                {
                    Util.MessageValidation("SFU3086", Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "PR_LOTID")));
                    return false;
                }
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

        private bool ValidationRunStart()
        {
            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (!CheckSelectWorkOrderInfo())
            {
                return false;
            }

            if (_processCode == Process.NOTCHING)
            {
                if (string.IsNullOrEmpty(_equipmentSegmentCode))
                {
                    // 라인을 선택 하세요.
                    Util.MessageValidation("SFU1255");
                    return false;
                }
                /*
                if (_dvProductLot == null)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }
                
                if (_dvProductLot["WIPSTAT"].GetString() != "WAIT")
                {
                    // 대기 LOT을 선택해주세요
                    Util.MessageValidation("SFU1492");
                    return false;
                }
                */

                int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
                {
                    Util.MessageValidation("SFU1492");
                    return false;
                }


                if (string.IsNullOrEmpty(_equipmentMountPositionCode))
                {
                    //Util.Alert("반제품 투입위치 기준정보가 없습니다.");
                    Util.MessageValidation("SFU1543");
                    return false;
                }
            }
            else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC공정신설
            {
                DataTable dt = ((DataView)DgProductLot.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("EQPTID") == UcAssemblyProductLot.txtSelectEquipment.Text
                                   && t.Field<string>("WIPSTAT") == "PROC"
                             select t).ToList();

                if (query.Any())
                {
                    //"장비에 진행 중 인 LOT이 존재 합니다."
                    Util.MessageValidation("SFU1863");
                    return false;
                }
            }

            else if (_processCode == Process.STACKING_FOLDING)
            {
            }
            else if (_processCode == Process.PACKAGING)
            {
            }

            return true;
        }

        private bool ValidationRunCancel()
        {
            //if (_dvProductLot == null)
            //{
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            //if (_dvProductLot["WIPSTAT"].GetString() != "PROC")
            //{
            //    Util.MessageValidation("SFU1846");
            //    return false;
            //}

            //string parentLot = _dvProductLot["PR_LOTID"].GetString();
            //string childSeq = _dvProductLot["CHILD_GR_SEQNO"].GetString();

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

            return true;
        }

        private bool ValidationRunComplete()
        {
            // 장비완료 Validation
            if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
            {   //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_dvProductLot["WIPSTAT"].GetString() != "PROC")
            {
                //장비완료 할 수 있는 LOT상태가 아닙니다.
                Util.MessageValidation("SFU1866");
                return false;
            }

            if (_processCode == Process.NOTCHING)
            {
                if (_selectUserModeAreaList.Contains(LoginInfo.CFG_AREA_ID))
                {
                    GetWorkCalander();

                    DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    if (string.IsNullOrEmpty(drShift[0]["VAL001"].GetString()))
                    {
                        Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.
                        return false;
                    }

                    if (!string.IsNullOrEmpty(drShift[0]["WRK_END_DTTM"].GetString()))
                    {
                        DateTime shiftStartDateTime = Convert.ToDateTime(drShift[0]["WRK_STRT_DTTM"].GetString());
                        DateTime shiftEndDateTime = Convert.ToDateTime(drShift[0]["WRK_END_DTTM"].GetString());
                        DateTime systemDateTime = GetSystemTime();

                        int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                        int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                        if (prevCheck < 0 || nextCheck > 0)
                        {
                            Util.MessageValidation("SFU3752"); // 입력오류 : 입력된 작업자 정보가 없습니다. 월력정보를 등록 하거나 작업자를 선택 하세요.

                            DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                            dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                            {
                                r["SHFT_ID"] = string.Empty;            //txtShift.Text        
                                r["VAL001"] = string.Empty;             //txtShift.Tag
                                r["WRK_USERID"] = string.Empty;         //txtWorker.Text
                                r["VAL002"] = string.Empty;             //txtWorker.Tag
                                r["WRK_STRT_DTTM"] = string.Empty;      //txtShiftStartTime.Text
                                r["WRK_END_DTTM"] = string.Empty;       //txtShiftEndTime.Text
                            });
                            dt.AcceptChanges();

                            return false;
                        }
                    }
                }
            }
            else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC공정신설
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // 저장하지 않은 불량 정보가 있습니다.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            else if (_processCode == Process.STACKING_FOLDING)
            {
                // 투입위치 중 mono 매거진 진행중 존재 여부 체크. 로직이 AS-IS에 있으나
                // SHOPID 오창 자동차 조립(A040), 오창 ESS 조립(A050)에만 해당되며, CNB2동은 관계가 없으므로 해당 내용을 반영하지 않음. CanStkConfirm

                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // 저장하지 않은 불량 정보가 있습니다.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            // 김용군 ESHM AZS_ECUTTER, AZS_STACKING 공정 추가에 따른 대응
            else if (_processCode == Process.AZS_ECUTTER)
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // 저장하지 않은 불량 정보가 있습니다.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            else if (_processCode == Process.AZS_STACKING)
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // 저장하지 않은 불량 정보가 있습니다.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            else if (_processCode == Process.PACKAGING)
            {
                //투입 바구니 완료 여부 체크 AS-IS winInput.CanPkgConfirm
                if (!ValidationPackagingConfirm(_dvProductLot["LOTID"].GetString()))
                    return false;

                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // 저장하지 않은 불량 정보가 있습니다.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidationCompleteCancelRun()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_dvProductLot["WIPSTAT"].GetString() != "EQPT_END")
            {
                // 장비완료 상태의 LOT이 아닙니다.
                Util.MessageValidation("SFU1864");
                return false;
            }

            string parentLot = _dvProductLot["PR_LOTID"].GetString();
            string childSeq = _dvProductLot["CHILD_GR_SEQNO"].GetString();

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
                            //Util.Alert("이후 CUT이 존재하여 취소할 수 없습니다.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "LOTID")));
                            return false;
                        }
                    }
                }
            }

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

        private bool ValidationSpclProdMode()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            string processLotId = string.Empty;
            if (ValidationProcessWip(out processLotId))
            {
                Util.MessageValidation("SFU3199", processLotId); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
                return false;
            }

            string equipmentDisplayName = string.Empty;
            processLotId = string.Empty;

            if (ValidationLoaderEquipmentProcessWip(out equipmentDisplayName, out processLotId))
            {
                Util.MessageValidation("SFU3747", equipmentDisplayName, processLotId); // 작업오류 : 로더 설비를 공유하는 [%1] 설비에 진행중인 LOT [%2] 이 있습니다.
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

            //if (_processCode == Process.STACKING_FOLDING) --> 전공정 진행중 LOT Validation (GM2 증설 Pjt 장영철)
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

        private bool ValidationRework()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            return true;
        }

        private bool ValidationQualityInput()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationPrint()
        {
            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                //Util.Alert("프린트 환경 설정값이 없습니다.");
                Util.MessageValidation("SFU2003");
                return false;
            }

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageInfo("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationLogMarge()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {
            /*
            if (_dvProductLot == null)
            {
                // 취소할 LOT을 선택하세요.
                Util.MessageValidation("SFU1938"); 
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() == "WAIT")
            {
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() == "EQPT_END")
            {
                // 설비 완공 LOT은 취소할 수 없습니다.
                Util.MessageValidation("SFU1671");  
                return false;
            }
            */

            return true;
        }

        private bool ValidationConfirm()
        {
            try
            {

                if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
                {
                    Util.MessageValidation("SFU1651");
                    return false;
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
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_AUTO_CONFIRM_LOT", "INDATA", "OUTDATA", inDataTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (Util.NVC(dtRslt.Rows[0]["RESULT_VALUE"]).Equals("OK"))
                    {
                        return true;
                    }
                    else
                    {
                        Util.MessageValidation(Util.NVC(dtRslt.Rows[0]["RESULT_CODE"]));
                        return false;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidationLoaderEquipmentProcessWip(out string equipmentDisplayName, out string processLotId)
        {
            equipmentDisplayName = string.Empty;
            processLotId = string.Empty;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_WIP_BY_LDR_EQPTID", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    equipmentDisplayName = Util.NVC(dtRslt.Rows[0]["EQPTDSPNAME"]);
                    processLotId = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidationPackagingConfirm(string lotId)
        {
            const string bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_L";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = lotId;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["MOUNT_MTRL_TYPE_CODE"].GetString() == "PROD")
                    {
                        if (dtResult.Rows[i]["WIPSTAT"].GetString() == "PROC" && dtResult.Rows[i]["PROD_LOTID"].GetString() == lotId)
                        {
                            object[] parameters = new object[2];
                            parameters[0] = dtResult.Rows[i]["EQPT_MOUNT_PSTN_NAME"].GetString();
                            parameters[1] = dtResult.Rows[i]["INPUT_LOTID"].GetString();
                            //[{0}] 위치에 투입완료되지 않은 바구니[{1}]가 존재 합니다.
                            Util.MessageValidation("SFU1282", parameters);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool ValidationProcessWip(out string processLotId)
        {
            processLotId = string.Empty;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                dtRow["WIPSTAT"] = Wip_State.PROC;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    processLotId = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


        }

        //private bool ValidationUpdateLaneNo()
        //{
        //    return true;
        //}

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


        private void PopupWaitLot()
        {
            ASSY005_COM_WAITLOT popWaitLot = new ASSY005_COM_WAITLOT();
            popWaitLot.FrameOperation = FrameOperation;

            if (_processCode == Process.STACKING_FOLDING)
            {
                object[] parameters = new object[5];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = Process.STACKING_FOLDING;
                //parameters[3] = _unldrLotIdentBasCode;
                parameters[3] = _ldrLotIdentBasCode;
                // stacking과 folding이 하나의 ProcID로 통합되어 있음.
                //그러나 현재 분리를 시켜야 하기 때문에 구분하여 값을 따로 준다.
                parameters[4] = EquipmentGroup.STACKING;
                C1WindowExtension.SetParameters(popWaitLot, parameters);
                popWaitLot.Closed += popWaitLot_Closed;
                Dispatcher.BeginInvoke(new Action(() => popWaitLot.ShowModal()));
            }
            else
            {
                object[] parameters = new object[4];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _processCode;
                //parameters[3] = _unldrLotIdentBasCode;
                parameters[3] = _ldrLotIdentBasCode;
                C1WindowExtension.SetParameters(popWaitLot, parameters);

                popWaitLot.Closed += popWaitLot_Closed;
                Dispatcher.BeginInvoke(new Action(() => popWaitLot.ShowModal()));
            }
        }

        private void popWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY005_COM_WAITLOT popup = sender as ASSY005_COM_WAITLOT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void PopupRemarkHistory()
        {
            ASSY005_001_LOTCOMMENTHIST popLotCommentHistory = new ASSY005_001_LOTCOMMENTHIST();
            popLotCommentHistory.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "PR_LOTID"));//_dvProductLot["PR_LOTID"].ToString();
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ"));//_dvProductLot["WIPSEQ"].ToString();
            C1WindowExtension.SetParameters(popLotCommentHistory, parameters);

            popLotCommentHistory.Closed += popLotCommentHistory_Closed;
            Dispatcher.BeginInvoke(new Action(() => popLotCommentHistory.ShowModal()));
        }

        private void popLotCommentHistory_Closed(object sender, EventArgs e)
        {
            ASSY005_001_LOTCOMMENTHIST popup = sender as ASSY005_001_LOTCOMMENTHIST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

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
            CMM_ASSY_EQPT_COND popEqptCond = new CMM_ASSY_EQPT_COND();
            popEqptCond.FrameOperation = FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));//_dvProductLot["LOTID"].ToString();
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ")); //_dvProductLot["WIPSEQ"].ToString();
            parameters[5] = UcAssemblyProductLot.txtSelectEquipmentName.Text;

            C1WindowExtension.SetParameters(popEqptCond, parameters);

            popEqptCond.Closed += popEqptCond_Closed;
            Dispatcher.BeginInvoke(new Action(() => popEqptCond.ShowModal()));
        }

        private void popEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND popup = sender as CMM_ASSY_EQPT_COND;
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
            if (_processCode == Process.NOTCHING)
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

                ASSY005_001_RUNSTART popupRunStart = new ASSY005_001_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[10];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _processCode;
                parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PR_LOTID"));//_dvProductLot["PR_LOTID"].ToString();
                parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PR_CSTID"));//_dvProductLot["PR_CSTID"].ToString();
                parameters[5] = _equipmentMountPositionCode;
                parameters[6] = _unldrLotIdentBasCode;
                parameters[7] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "MO_PRODID"));//_dvProductLot["MO_PRODID"].ToString();
                parameters[8] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPQTY_M_EA")); //_dvProductLot["WIPQTY_M_EA"].ToString();
                parameters[9] = equipmentName;// UcAssemblyProductLot.txtSelectEquipmentName.Text;
                C1WindowExtension.SetParameters(popupRunStart, parameters);

                popupRunStart.Closed += popupRunStart_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            }
            else
            {
                ASSY005_COM_RUNSTART popupRunStart = new ASSY005_COM_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[3];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _processCode;
                C1WindowExtension.SetParameters(popupRunStart, parameters);
                popupRunStart.Closed += popupRunStart_Closed;

                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            }
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            if (_processCode == Process.NOTCHING)
            {
                ASSY005_001_RUNSTART pop = sender as ASSY005_001_RUNSTART;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();

                    //SetProductLotList();
                    // 생산 Lot 재조회
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
            else
            {
                ASSY005_COM_RUNSTART pop = sender as ASSY005_COM_RUNSTART;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    //ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                    //SetProductLotList();
                    string processLotId = SelectProcessLotId();

                    //SetProductLotList();
                    // 생산 Lot 재조회
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
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[3] = UcAssemblyProductLot.txtSelectEquipmentName.Text;

            C1WindowExtension.SetParameters(popupCancelConfrim, parameters);

            popupCancelConfrim.Closed += popupCancelConfrim_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCancelConfrim.ShowModal()));
        }

        private void popupCancelConfrim_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_CONFIRM_PROD pop = sender as CMM_ASSY_CANCEL_CONFIRM_PROD;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //SetProductLotList();
            }
            if (string.IsNullOrEmpty(_equipmentSegmentCode) || string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text)) return;
            SetProductLotList();
        }

        private void PopupLotMerge()
        {
            ASSY005_OUTLOT_MERGE popLotMerge = new ASSY005_OUTLOT_MERGE();
            popLotMerge.FrameOperation = FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = _equipmentSegmentName;
            parameters[4] = _ldrLotIdentBasCode;

            C1WindowExtension.SetParameters(popLotMerge, parameters);
            popLotMerge.Closed += popLotMerge_Closed;
            Dispatcher.BeginInvoke(new Action(() => popLotMerge.ShowModal()));
        }

        private void popLotMerge_Closed(object sender, EventArgs e)
        {
            ASSY005_OUTLOT_MERGE pop = sender as ASSY005_OUTLOT_MERGE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text)) return;

            Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
        }

        private void PopupChangeCarrier()
        {
            CMM_COM_CHG_CARRIER popupChangeCarrier = new CMM_COM_CHG_CARRIER();
            popupChangeCarrier.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = _processCode; //Process.STACKING_FOLDING;
            parameters[1] = "N";

            C1WindowExtension.SetParameters(popupChangeCarrier, parameters);
            popupChangeCarrier.Closed += popupChangeCarrier_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupChangeCarrier.ShowModal()));
        }

        private void popupChangeCarrier_Closed(object sender, EventArgs e)
        {
            CMM_COM_CHG_CARRIER pop = sender as CMM_COM_CHG_CARRIER;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupTrayInfo()
        {
            CMM_PKG_TRAY_INFO popupTrayInfo = new CMM_PKG_TRAY_INFO();
            popupTrayInfo.FrameOperation = FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = _processCode; //Process.PACKAGING;
            C1WindowExtension.SetParameters(popupTrayInfo, parameters);

            popupTrayInfo.Closed += popupTrayInfo_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTrayInfo.ShowModal()));
        }

        private void popupTrayInfo_Closed(object sender, EventArgs e)
        {
            CMM_PKG_TRAY_INFO pop = sender as CMM_PKG_TRAY_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupReWork()
        {
            ASSY005_001_REWORK popupRework = new ASSY005_001_REWORK();
            popupRework.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = Process.VD_LMN;
            parameters[3] = _unldrLotIdentBasCode;

            C1WindowExtension.SetParameters(popupRework, parameters);
            popupRework.Closed += popupRework_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupRework.ShowModal()));
        }

        private void popupRework_Closed(object sender, EventArgs e)
        {
            ASSY005_001_REWORK pop = sender as ASSY005_001_REWORK;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
            }
        }

        private void PopupQualityInput()
        {
            CMM_COM_SELF_INSP popupQualityInput = new CMM_COM_SELF_INSP();
            popupQualityInput.FrameOperation = FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[3] = UcAssemblyProductLot.txtSelectEquipmentName.Text;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupQualityInput, parameters);
            popupQualityInput.Closed += popupQualityInput_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupQualityInput.ShowModal()));
        }

        private void popupQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP pop = sender as CMM_COM_SELF_INSP;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                // TODO : AS-IS Packaging 공정진척에는 동별 공통코드에 인장강도입력체크여부를 관리하는 로직이 있으나,
                // 오창자동차1동, 오창자동차2동에 국한되어 있으며, CNB2동 조립공정진척에는 해당사항이 없을것으로 판단되어 해당 로직은 추가하지 않는다. 
            }
        }

        private void PopupEquipmentEnd()
        {
            if (_processCode == Process.NOTCHING)
            {
                ASSY005_001_EQPTEND popupEquipmentEnd = new ASSY005_001_EQPTEND { FrameOperation = FrameOperation };

                object[] parameters = new object[9];
                parameters[0] = _processCode;
                parameters[1] = _equipmentSegmentCode;
                parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[3] = _dvProductLot["LOTID"].GetString();
                parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                parameters[5] = _ldrLotIdentBasCode;
                parameters[6] = _unldrLotIdentBasCode;
                parameters[7] = _equipmentMountPositionCode;
                parameters[8] = CheckProdQtyChangePermission();

                C1WindowExtension.SetParameters(popupEquipmentEnd, parameters);
                popupEquipmentEnd.Closed += popupEquipmentEnd_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupEquipmentEnd.ShowModal()));
            }
            else
            {
                ASSY005_COM_EQPTEND popupEquipmentEnd = new ASSY005_COM_EQPTEND { FrameOperation = FrameOperation };

                object[] parameters = new object[9];
                parameters[0] = _processCode;
                parameters[1] = _equipmentSegmentCode;
                parameters[2] = _equipmentCode;
                parameters[3] = _dvProductLot["LOTID"].GetString();
                parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                parameters[5] = _ldrLotIdentBasCode;
                parameters[6] = _unldrLotIdentBasCode;
                parameters[7] = CheckProdQtyChangePermission();
                parameters[8] = "N";
                C1WindowExtension.SetParameters(popupEquipmentEnd, parameters);
                popupEquipmentEnd.Closed += popupEquipmentEnd_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupEquipmentEnd.ShowModal()));
            }
        }

        private void popupEquipmentEnd_Closed(object sender, EventArgs e)
        {
            if (_processCode == Process.NOTCHING)
            {
                ASSY005_001_EQPTEND pop = sender as ASSY005_001_EQPTEND;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                    SetProductLotList();
                }
            }
            else
            {
                ASSY005_COM_EQPTEND pop = sender as ASSY005_COM_EQPTEND;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                    SetProductLotList();
                }
            }
        }

        private void PopupPrint()
        {

            ASSY005_001_HIST popupPrint = new ASSY005_001_HIST();
            popupPrint.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = _equipmentCode;
            //_UNLDR코드를 popupPrint 보낸다.
            parameters[3] = _unldrLotIdentBasCode;
            C1WindowExtension.SetParameters(popupPrint, parameters);
            popupPrint.Closed += popupPrint_Closed;

            Dispatcher.BeginInvoke(new Action(() => popupPrint.ShowModal()));
        }

        private void popupPrint_Closed(object sender, EventArgs e)
        {
            ASSY005_001_HIST pop = sender as ASSY005_001_HIST;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void popupConfirm_Closed(object sender, EventArgs e)
        {
            try
            {
                if (_processCode == Process.NOTCHING)
                {
                    ASSY005_001_CONFIRM pop = sender as ASSY005_001_CONFIRM;
                    if (pop != null && pop.DialogResult == MessageBoxResult.OK && _isPopupConfirmJobEnd)
                    {
                        // 발행 여부 체크 및 미발행 시 자동 발행 처리
                        AutoPrint(pop.CHECKED);

                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                    }
                }
                else
                {
                    ASSY005_COM_CONFIRM pop = sender as ASSY005_COM_CONFIRM;
                    if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                    {
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                _isPopupConfirmJobEnd = true;
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
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[3];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag;
                    Parameters[2] = _processCode;

                    C1WindowExtension.SetParameters(popupWorkHalfSlitSide, Parameters);

                    popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
                }
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag;

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

        private void GetWorkHalfSlittingSide()
        {
            try
            {
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(dr);

                string sBizrule = string.Empty;
                
                string sWorkHalfSlittingSideName = string.Empty;
                string sWorkHalfSlittingSide = string.Empty;
                sAttr2 = string.Empty;

                DataTable dtUWND = GetCommonUWND();
                sAttr2 = (dtUWND != null && dtUWND.Rows.Count > 0) ? dtUWND.Rows[0]["ATTR2"].ToString() : "N";

                if (sAttr2 == "Y")
                    sBizrule = "DA_PRD_SEL_CURR_MOUNT_MTRL_WRK_HALF_SLIT_SIDE";
                else
                    sBizrule = "DA_PRD_SEL_WRK_HALF_SLIT_SIDE";

                new ClientProxy().ExecuteService(sBizrule, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        if (sAttr2 == "Y")
                        {
                            for (int i = 0; i <= dtUWND.Rows.Count; i++)
                            {
                                sWorkHalfSlittingSideName += i > 0 ? ", " : string.Empty;
                                sWorkHalfSlittingSide += i > 0 ? "," : string.Empty;

                                sWorkHalfSlittingSideName += bizResult.Rows[i]["WRK_HALF_SLIT_SIDE_NAME"].ToString();
                                sWorkHalfSlittingSide += bizResult.Rows[i]["WRK_HALF_SLIT_SIDE"].ToString();
                            }

                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Text = Util.NVC(sWorkHalfSlittingSideName);
                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag = Util.NVC(sWorkHalfSlittingSide);
                        }
                        else
                        {
                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Text = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"]);
                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"]);
                        }
                    }
                    else
                    {
                        UcAssemblyProductLot.txtWorkHalfSlittingSide.Text = string.Empty;
                        UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        /// <summary>
        /// 권취 방향 변경
        /// </summary>
        private void PopupEmSectionRollDirctn()
        {
            if (!ValidationEmSectionRollDirctn()) return;

            CMM_ELEC_EM_SECTION_ROLL_DIRCTN popupEmSectionRollDirctn = new CMM_ELEC_EM_SECTION_ROLL_DIRCTN { FrameOperation = FrameOperation };
            if (popupEmSectionRollDirctn != null)
            {
                popupEmSectionRollDirctn.FrameOperation = FrameOperation;
                UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[1];
                Parameters[0] = _equipmentCode;

                C1WindowExtension.SetParameters(popupEmSectionRollDirctn, Parameters);

                popupEmSectionRollDirctn.Closed += new EventHandler(PopupEmSectionRollDirctn_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEmSectionRollDirctn.ShowModal()));
            }
        }

        private bool ValidationEmSectionRollDirctn()
        {
            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private void GetSlittingLaneNo()
        {
            try
            {
                UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;

                if (UcAssemblyProductLot.txtSlittingLaneNo.Visibility != Visibility.Visible)
                {
                    return;
                }

                if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
                {
                    return;
                }

                DataTable dtInDataTable = new DataTable("INDATA");
                dtInDataTable.Columns.Add("LANGID", typeof(string));
                dtInDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtInDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                dtInDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_EQPT_INPUT_LANE_INFO", "RQSTDT", "RSLTDT", dtInDataTable, (dtBizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(dtBizResult))
                    {
                        if (dtBizResult.Columns["LANE_ID"] != null)
                        {
                            string sLineNo = "";
                            foreach (DataRow drLaneNo in dtBizResult.Rows)
                            {
                                if (drLaneNo != null && drLaneNo["LANE_ID"] != null)
                                {
                                    if (sLineNo.Length > 0)
                                    {
                                        sLineNo += ", ";
                                    }

                                    sLineNo += Util.NVC(drLaneNo["LANE_ID"]);
                                }
                            }

                            UcAssemblyProductLot.txtSlittingLaneNo.Text = sLineNo;
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void PopupUpdateLaneNo()
        //{
        //    CMM_COM_UPDATE_LANENO popUpdateLaneNo = new CMM_COM_UPDATE_LANENO();
        //    popUpdateLaneNo.FrameOperation = FrameOperation;

        //    object[] parameters = new object[1];
        //    parameters[0] = _processCode;
        //    C1WindowExtension.SetParameters(popUpdateLaneNo, parameters);

        //    popUpdateLaneNo.Closed += popUpdateLaneNo_Closed;
        //    Dispatcher.BeginInvoke(new Action(() => popUpdateLaneNo.ShowModal()));
        //}

        //private void popUpdateLaneNo_Closed(object sender, EventArgs e)
        //{
        //    CMM_COM_UPDATE_LANENO popup = sender as CMM_COM_UPDATE_LANENO;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //    }
        //}

        //ESMI 1동 조립(A4) 의 경우 동별 공통코드 체크하여 MES SFU Configurtion에서 셋팅된 라인정보가 아닌 '조립공정진척-6Line' 화면에 라인 콤보박스에 선택된 라인정보로 조회되게 수정
        private bool IsEsmiLineCheck()
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
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
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
        #endregion

        #endregion

    }
}
