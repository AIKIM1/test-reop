/*************************************************************************************
 Created Date : 2020.09.09
      Creator : 정문교
   Decription : 전극 공정진척
--------------------------------------------------------------------------------------
 [Change History]
 2020.09.09  정문교 : Initial Created.
 2021.02.11  정문교 : 시생산설정/해제 버튼 추가
 2021.02.19  정문교 : Half Slitting 공정 추가
 2021.07.01  조영대 : Slitting, 절연 Coter, 표면검사, TAPING 추가
 2021.08.12  김지은 : 시생산샘플설정/해제 기능 추가
 2021.09.13  조영대 : 전극콤보 공정별 비사용 공통코드에 따라 숨김처리
 2021.10.14  김지은 : 무지부방향설정 신규팝업화면 적용(공동코드 유무에 따라 구/신 팝업 선택하여 띄움)
 2021.10.25  조영대 : 권취 방향 변경 팝업 추가
 2021.12.15  강동희 : 2차 Slitting 공정진척DRB 화면 개발
 2022.03.11  김지은 : 권한에 따라 설비 변경 불가 기능 추가
 2022.03.12  김지은 : TEST CUT 실적반영 기능 추가
 2022.06.21  김지은 : 재와인딩 공정진척 장착부 변경 시 설비 재조회
 2022.06.22  윤기업 : RollMap 적용 
 2022.11.02  윤기업 : ROLLPRESS ROLLMAP 수불여부 반영
 2022.11.10  오화백 : 투입랏이 FastTrack 일 경우,  완성랏이 QA 대상랏일 경우 경고 문구 추가
 2022.11.17  오화백 : Re-Winding 공정진척시 대기재공조회 팝업 추가, 롤프레스/슬리터 설비에서 고객인증 그룹 기준정보 팝업 추가
 2022.11.23  오화백   물류반송조건설정 팝업 추가
 2022.12.27  신광희 : GM 롤프레스 공정인 경우 적재율에 따른 UI 표시 기능 추가  
 2023.01.11  윤기업 : GM 롤맵 수불 적용 - Coater 장비 조회시 [EQPT_RSLT_CODE] ='A' 주석처리 ==> 원복 (2023-01-13 롤맵수불 - 자동실적으로 처리)
 2023.01.12  윤기업 : GM 롤맵 수불 적용 - ROLLMAP TAG_SECTION/최외곽 폐기 수정 후 불량/LOSS/물품청구 재조회
 2023.02.02  윤기업 : GM 롤맵 수불 적용 - TEST CUT 기능 적용
 2023.03.14  정재홍 : [C20220812-000226] - COATER pop-up alarm improvement
 2023.05.30  양영재 : [E20230501-000010] - 동별 공통 코드에 따라 롤프레스 대Lot 품질 정보 필수 입력 SKIP
 2023.05.31  정재홍 : [E20230412-001093] - [ESGM] 공정진척 실적 자동화 (롤프레스,재와인딩기)
 2023.07.14  김태우 : NFF DAM_MIXING 추가
 2023.08.17  김태우 : NFF 슬리팅 전극 등급 정보 확인
 2023.08.24  장영철 : GM2 예외로직 추가 (G674 조건 추가)
 2023.09.05  임재형 : 롤맵 홀드 조건 및 노트 추가
 2023.09.18  김태우 : NFF 슬리팅 전극 등급 정보 확인 보류
 2023.09.23  정재홍 : [E20230825-001646] - 재와인딩 합권취 문제 개선 건 (Validation 기능 추가]
 2023.10.30  김태우 : NFF 슬리팅 전극 등급 정보가 없어도 설비완료는 되지만,  작업완료는 등급 정보가 있어야 함.
 2023.11.24  정재홍 : [E20231030-000234] - R/P 공정 실적확정시 무지부 방향 설정 미등록 Validation 
 2024.04.30  나순민 : [E20240403-001187] [ESWA PI] Operator name multiple showing
 2024.05.22  백상우 : [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부
 2024.07.25  이원열 : [E20240712-001591] 공정진척 & 실적확정 투입lot 조회 validation 추가(대체자제)
 2024.08.28  조성근 : [E20240827-000372] 롤프레스 롤맵 수불 적용
 2024.11.29  이동주 : E20240904-000991 [MES팀] 모델 버전별 반제품/설비 CP revision으로 설비 및 레시피를 운영하기 위한 MES 개선 요청 건(CatchUp)
 2025.03.20  이민형 : [MI2_OSS_0094] 시생산 설정/해제 버튼 클릭 시 Validation Skip 기능 추가
 2025.03.20  이민형 : [MI2_OSS_0218] 시생산 설정/해제 버튼 클릭 시 Validation Skip 기능 추가 한 부분 롤백
 2025.05.02  이민형 : [MI2_OSS_0223] Carrier를 재 매핑하도록 알람 메시지 추가
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.ELEC003.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Threading;

namespace LGC.GMES.MES.ELEC003
{
    public partial class ELEC003_001 : IWorkArea
    {
        #region Declaration

        public UcElectrodeCommand UcElectrodeCommand { get; set; }
        public UcElectrodeEquipment UcElectrodeEquipment { get; set; }
        public UcElectrodeProductLot UcElectrodeProductLot { get; set; }
        public UcElectrodeProductionResult_Mixing UcResult_Mixing { get; set; }
        public UcElectrodeProductionResult_CoatingAuto UcResult_CoatingAuto { get; set; }
        public UcElectrodeProductionResult_InsCoating UcResult_InsCoating { get; set; }
        public UcElectrodeProductionResult_HalfSlitting UcResult_HalfSlitting { get; set; }
        public UcElectrodeProductionResult_RollPressing UcResult_RollPressing { get; set; }
        public UcElectrodeProductionResult_Slitting UcResult_Slitting { get; set; }
        public UcElectrodeProductionResult_ReWinding UcResult_ReWinding { get; set; }
        public UcElectrodeProductionResult_ReWinder UcResult_ReWinder { get; set; }
        public UcElectrodeProductionResult_Taping UcResult_Taping { get; set; }
        public UcElectrodeProductionResult_HeatTreatment UcResult_HeatTreatment { get; set; }

        public UcElectrodeProductionResult_TwoSlitting UcResult_TwoSlitting { get; set; } //20211215 2차 Slitting 공정진척DRB 화면 개발

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private C1DataGrid DgEquipment { get; set; }
        private C1DataGrid DgProductLot { get; set; }

        private string _equipmentSegmentCode;
        private string _equipmentSegmentName;
        private string _processCode;
        private string _processName;
        private string _equipmentCode;
        private string _equipmentName;
        private string _treeEquipmentCode;
        private string _treeEquipmentName;
        private string _productLot;

        private string _reWindingProcess;
        private string _HalfSlitSide;   // 무지부 방향
        private string _RollDirctn;   // 권취 방향
        private string _ldrLotIdentBasCode;
        private string _unldrLotIdentBasCode;
        private string _labelPassHoldFlag = string.Empty;

        private string _TopLot = string.Empty;
        private string _BackLot = string.Empty;
        private string _MtrlID = string.Empty;

        private string _isPostingHold;
        private bool _isLastBatch = false;

        DataRowView _dvProductLot;
        private bool _isEquipmentClick = false;

        // Roll Map
        private bool _isOriginRollMapEquipment = false;
        private bool _isRollMapEquipment = false;
        private bool _isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        private bool _isRollMapLot = false;
        private bool _isRollMapSBL = false;
        private bool _isRollMapDivReportEqpt = false; //롤맵 수집항목 분할보고 설비체크

        private System.Windows.Threading.DispatcherTimer _timer = null;
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherRackRateTimer = new System.Windows.Threading.DispatcherTimer();


        private bool _isRackRateMode = false;

        private DataTable dtSlurry = null;

        private enum SetRackState
        {
            Normal,
            Warning,
            Danger
        }

        private SetRackState _rackState;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        // 2022.1.7. ESNJ 1동 생산시스템 구축. 실적 완료 후 ProductLot에 Lot 정보가 사라져 전극 이력카드 출력이 되지 않는 오류 수정
        private string _txtEndLotId = string.Empty;
        private string _txtEndLotCutId = string.Empty;

        // 코터 롤맵 Hold 기능 추가
        Dictionary<string, string> holdLotClassCode = new Dictionary<string, string>();

        // [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        private bool _isELEC_MTRL_LOT_VALID_YN = false;

        #endregion

        #region Initialize
        public ELEC003_001()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            SetComboBox();
            ///////////////////////////////// Re-Winding 공정 체크
            SetReWindingProcess();
            ///////////////////////////////// Load, Unload 체크
            SetIdentInfo();

            // 전극콤보 사용유무 체크
            CheckUsePolarity(_processCode);

            InitializeUserControls();
            SetEventInUserControls();
            SeProcessInUserControls();
            TimerSetting();

            UcElectrodeCommand.btnRollMap.IsEnabled = false;
        }

        private void SetComboBox()
        {
            // 라인
            SetEquipmentSegmentCombo(cboEquipmentSegment);

            // 공정
            SetProcessCombo(cboProcess);

            // 극성
            CommonCombo _combo = new CommonCombo();

            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboPolarity, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            // 설비
            SetEquipmentCombo(cboEquipment);

            // Top/Back
            cboCoatSide.SetCommonCode("COAT_SIDE_TYPE", string.Empty, CommonCombo.ComboStatus.NONE, true, ComboBoxExtension.InCodeType.Colon);

            // 자동조회
            String[] sFilter3 = { "DRB_REFRESH_TERM" };
            _combo.SetCombo(cboAutoSearch, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE_WITHOUT_CODE");

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboPolarity.SelectedValueChanged += cboPolarity_SelectedValueChanged;
            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
            cboCoatSide.SelectedValueChanged += cboCoatSide_SelectedValueChanged;
            cboAutoSearch.SelectedValueChanged += cboAutoSearch_SelectedValueChanged;

            if (string.Equals(cboProcess.SelectedValue?.GetString(), Process.ROLL_PRESSING) && (LoginInfo.CFG_SHOP_ID.Equals("G672") || LoginInfo.CFG_SHOP_ID.Equals("G674"))) // GM2추가    2023-08-24  장영철
            {
                SelectRackRate();
            }
        }

        private void InitializeUserControls()
        {
            UcElectrodeCommand = grdCommand.Children[0] as UcElectrodeCommand;
            UcElectrodeEquipment = grdEquipment.Children[0] as UcElectrodeEquipment;
            UcElectrodeProductLot = grdProduct.Children[0] as UcElectrodeProductLot;

            ///////////////////////////////////////// 공정별 실적 User Control
            UcResult_Mixing = grdProductionResult_Mixing.Children[0] as UcElectrodeProductionResult_Mixing;
            UcResult_CoatingAuto = grdProductionResult_Coating.Children[0] as UcElectrodeProductionResult_CoatingAuto;
            UcResult_InsCoating = grdProductionResult_InsCoating.Children[0] as UcElectrodeProductionResult_InsCoating;
            UcResult_HalfSlitting = grdProductionResult_HalfSlitting.Children[0] as UcElectrodeProductionResult_HalfSlitting;
            UcResult_RollPressing = grdProductionResult_RollPressing.Children[0] as UcElectrodeProductionResult_RollPressing;
            UcResult_Slitting = grdProductionResult_Slitting.Children[0] as UcElectrodeProductionResult_Slitting;
            UcResult_ReWinding = grdProductionResult_ReWinding.Children[0] as UcElectrodeProductionResult_ReWinding;
            UcResult_ReWinder = grdProductionResult_ReWinder.Children[0] as UcElectrodeProductionResult_ReWinder;
            UcResult_Taping = grdProductionResult_Taping.Children[0] as UcElectrodeProductionResult_Taping;
            UcResult_HeatTreatment = grdProductionResult_HeatTreatment.Children[0] as UcElectrodeProductionResult_HeatTreatment;
            UcResult_TwoSlitting = grdProductionResult_TwoSlitting.Children[0] as UcElectrodeProductionResult_TwoSlitting; //20211215 2차 Slitting 공정진척DRB 화면 개발

            if (UcElectrodeCommand != null)
            {
                UcElectrodeCommand.UcParentControl = this;
                UcElectrodeCommand.FrameOperation = FrameOperation;
                UcElectrodeCommand.ProcessCode = _processCode;
                UcElectrodeCommand.FrameOperation = FrameOperation;
                UcElectrodeCommand.SetButtonVisibility(true, _reWindingProcess == "Y" ? true : false);
                UcElectrodeCommand.SetButtonExtraVisibility(true, _reWindingProcess == "Y" ? true : false);
                UcElectrodeCommand.ApplyPermissions();
            }

            if (UcElectrodeEquipment != null)
            {
                UcElectrodeEquipment.UcParentControl = this;
                UcElectrodeEquipment.FrameOperation = FrameOperation;
                UcElectrodeEquipment.DgEquipment.MouseLeftButtonUp += DgEquipment_MouseLeftButtonUp;
                UcElectrodeEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
                UcElectrodeEquipment.ProcessCode = _processCode;
                UcElectrodeEquipment.EquipmentCode = _equipmentCode;

                DgEquipment = UcElectrodeEquipment.dgEquipment;
                SetEquipmentTree();
            }

            if (UcElectrodeProductLot != null)
            {
                UcElectrodeProductLot.UcParentControl = this;
                UcElectrodeProductLot.FrameOperation = FrameOperation;
                UcElectrodeProductLot.DgProductLot.MouseLeftButtonUp += DgProductLot_MouseLeftButtonUp;
                UcElectrodeProductLot.DgProductLot.PreviewMouseDoubleClick += DgProductLot_PreviewMouseDoubleClick;
                UcElectrodeProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
                UcElectrodeProductLot.ProcessCode = _processCode;
                UcElectrodeProductLot.EquipmentCode = _equipmentCode;
                if (cboCoatSide.Visibility.Equals(Visibility.Visible))
                {
                    UcElectrodeProductLot.CoatSide = cboCoatSide.GetStringValue();
                }
                else
                {
                    UcElectrodeProductLot.CoatSide = null;
                }
                UcElectrodeProductLot.SetControlVisibility();

                DgProductLot = UcElectrodeProductLot.dgProductLot;
                SetProductLotList();

                if (cboEquipment.SelectedItems.Count() > 0)
                {
                    UcElectrodeProductLot.txtSelectEquipment.Text = _equipmentCode;
                    UcElectrodeProductLot.txtSelectEquipmentName.Text = _equipmentName;
                }

                // 무지부 방향 조회(동별 공통 : MNG_SLITTING_SIDE_AREA)
                if (UcElectrodeCommand.IsManageSlittingSide)
                {
                    GetWorkHalfSlittingSide();
                }

                UcElectrodeProductLot.CommandButtonClick += UcElectrodeProductLot_CommandButtonClick;
            }

            ///////////////////////////////////////// 공정별 실적 User Control
            if (UcResult_Mixing != null)
            {
                UcResult_Mixing.FrameOperation = FrameOperation;
                UcResult_Mixing.ApplyPermissions();
            }
            if (UcResult_CoatingAuto != null)
            {
                UcResult_CoatingAuto.FrameOperation = FrameOperation;
                UcResult_CoatingAuto.ButtonSaveRegDefectLane.Click += ButtonSaveRegDefectLane_Click;
                UcResult_CoatingAuto.ApplyPermissions();
            }
            if (UcResult_InsCoating != null)
            {
                UcResult_InsCoating.FrameOperation = FrameOperation;
                UcResult_InsCoating.ButtonSaveRegDefectLane.Click += ButtonSaveRegDefectLane_Click;
                UcResult_InsCoating.ApplyPermissions();
            }
            if (UcResult_HalfSlitting != null)
            {
                UcResult_HalfSlitting.FrameOperation = FrameOperation;
                UcResult_HalfSlitting.ApplyPermissions();
            }
            if (UcResult_RollPressing != null)
            {
                UcResult_RollPressing.FrameOperation = FrameOperation;
                UcResult_RollPressing.ApplyPermissions();
            }
            if (UcResult_Slitting != null)
            {
                UcResult_Slitting.FrameOperation = FrameOperation;
                UcResult_Slitting.ApplyPermissions();
            }
            if (UcResult_ReWinding != null)
            {
                UcResult_ReWinding.FrameOperation = FrameOperation;
                UcResult_ReWinding.ApplyPermissions();
            }
            if (UcResult_ReWinder != null)
            {
                UcResult_ReWinder.FrameOperation = FrameOperation;
                UcResult_ReWinder.ApplyPermissions();
            }
            if (UcResult_Taping != null)
            {
                UcResult_Taping.FrameOperation = FrameOperation;
                UcResult_Taping.ApplyPermissions();
            }
            if (UcResult_HeatTreatment != null)
            {
                UcResult_HeatTreatment.FrameOperation = FrameOperation;
                UcResult_HeatTreatment.ApplyPermissions();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            if (UcResult_TwoSlitting != null)
            {
                UcResult_TwoSlitting.FrameOperation = FrameOperation;
                UcResult_TwoSlitting.ApplyPermissions();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
        }

        private void SetEventInUserControls()
        {
            if (UcElectrodeCommand != null)
            {
                UcElectrodeCommand.btnExtra.MouseLeave += ButtonExtra_MouseLeave;

                UcElectrodeCommand.btnManualMode.Click += ButtonManualMode_Click;                               // 수작업모드
                UcElectrodeCommand.btnEqptIssue.Click += ButtonEqptIssue_Click;                                 // 설비특이사항
                //UcElectrodeCommand.btnFinalCut.Click += ButtonFinalCut_Click;                                 // F/Cut변경
                UcElectrodeCommand.btnCleanLot.Click += ButtonCleanLot_Click;                                   // 대LOT정리
                UcElectrodeCommand.btnCancelFCut.Click += ButtonCancelFCut_Click;                               // LOT종료취소
                UcElectrodeCommand.btnCancelDelete.Click += ButtonCancelDelete_Click;                           // 삭제Lot생성
                UcElectrodeCommand.btnCut.Click += ButtonCut_Click;                                             // Cut
                UcElectrodeCommand.btnInvoiceMaterial.Click += ButtonInvoiceMaterial_Click;                     // 투입요청서
                UcElectrodeCommand.btnEqptCond.Click += ButtonEqptCond_Click;                                   // 작업조건등록
                UcElectrodeCommand.btnMixConfirm.Click += ButtonMixConfirm_Click;                               // 자주검사등록
                UcElectrodeCommand.btnSamplingProd.Click += ButtonSamplingProd_Click;                           // R/P 샘플링 제품등록
                UcElectrodeCommand.btnProcReturn.Click += ButtonProcReturn_Click;                               // R/P 대기 변경
                UcElectrodeCommand.btnSamplingProdT1.Click += ButtonSamplingProdT1_Click;                       // 샘플링 제품등록
                UcElectrodeCommand.btnMixerTankInfo.Click += ButtonMixerTankInfo_Click;                         // Slurry정보
                UcElectrodeCommand.btnReservation.Click += ButtonReservation_Click;                             // W/O 예약
                UcElectrodeCommand.btnFoil.Click += ButtonFoil_Click;                                           // FOIL 관리
                UcElectrodeCommand.btnSlurryConf.Click += ButtonSlurryConf_Click;                               // Slurry 물성정보
                UcElectrodeCommand.btnStartCoaterCut.Click += ButtonStartCoaterCut_Click;                       // 코터 임의 Cut 생성
                //UcElectrodeCommand.btnMovetoHalf.Click += ButtonMovetoHalf_Click;                             // 하프슬리터 이동
                UcElectrodeCommand.btnWorkHalfSlitSide.Click += ButtonWorkHalfSlitSide_Click;                   // 무지부 방향설정
                UcElectrodeCommand.btnEmSectionRollDirctn.Click += ButtonEmSectionRollDirctn_Click;                   // 권취방향변경
                UcElectrodeCommand.btnLogisStat.Click += ButtonLogisStat_Click;                                 // 물류반송현황
                UcElectrodeCommand.btnSkidTypeSettingByPort.Click += ButtonSkidTypeSettingByPort_Click;         // Port별 Skid Type
                UcElectrodeCommand.btnSlBatch.Click += ButtonSlBatch_Click;                                     // 목시관리필요Lot 등록
                UcElectrodeCommand.btnCustomer.Click += ButtonbtnCustomer_Click;                                 // 고객인증그룹조회
                UcElectrodeCommand.btnSearchWaitingWork.Click += ButtonbtnSearchWaitingWork_Click;               // Verification 재공대기조회 팝업 


                UcElectrodeCommand.btnScrapLot.Click += ButtonScrapLot_Click;                                   // Scrap Lot 재생성 등록
                UcElectrodeCommand.btnWebBreak.Click += ButtonWebBreak_Click;                                   // 단선추가
                UcElectrodeCommand.btnPilotProdMode.Click += ButtonPilotProdMode_Click;                         // 시생산설정/해제
                UcElectrodeCommand.btnPilotProdSPMode.Click += ButtonPilotProdSPMode_Click;                     // 시생산샘플설정/해제
                UcElectrodeCommand.btnShipmentModel.Click += ButtonShipmentModel_Click;                         // 출하모델
                UcElectrodeCommand.btnRollMapInputMaterial.Click += ButtonRollMapInputMaterial_Click;           // 자재투입이력[Foil/Slurry]

                UcElectrodeCommand.btnMove.Click += ButtonMove_Click;                                           // 이동
                UcElectrodeCommand.btnMoveCancel.Click += ButtonMoveCancel_Click;                               // 이동취소
                UcElectrodeCommand.btnInput.Click += ButtonInput_Click;                                         // 추가투입
                UcElectrodeCommand.btnInputCancel.Click += ButtonInputCancel_Click;                             // 추가투입취소
                UcElectrodeCommand.btnStart.Click += ButtonStart_Click;                                         // 작업시작
                UcElectrodeCommand.btnCancel.Click += ButtonCancel_Click;                                       // 시작취소
                UcElectrodeCommand.btnEqptEnd.Click += ButtonEqptEnd_Click;                                     // 장비완료
                UcElectrodeCommand.btnEqptEndCancel.Click += ButtonEqptEndCancel_Click;                         // 장비완료취소
                UcElectrodeCommand.btnEndCancel.Click += ButtonEndCancel_Click;                                 // 실적확정취소(코터 자동실적 완성 취소)

                UcElectrodeCommand.btnProductList.Click += ButtonProductList_Click;                             // 공정 홈 화면전환
                UcElectrodeCommand.btnProductionSchedule.Click += ButtonProductionSchedule_Click;               // 생산 Schedule 조회
                UcElectrodeCommand.btnDispatch.Click += ButtonDispatch_Click;                                   // 공정이동(실적확정)
                UcElectrodeCommand.btnCard.Click += ButtonCard_Click;                                           // 이력카드발행
                UcElectrodeCommand.btnBarcodeLabel.Click += ButtonBarcodeLabel_Click;                           // 바코드발행
                UcElectrodeCommand.btnPrint.Click += ButtonPrint_Click;                                         // 발행
                UcElectrodeCommand.btnRollMap.Click += ButtonRollMap_Click;                                     // RollMap


                UcElectrodeCommand.btnReturnCondition.Click += ButtonbtnReturnCondition_Click;                  // 물류반송조건팝업

                //UcElectrodeCommand.btnUpdateLaneNo.Click += ButtonbtnUpdateLaneNo_Click;               // 슬리팅 레인 번호 변경 버튼 추가 

                UcElectrodeCommand.btnSlurryManualOutput.Click += ButtonbtnSlurryManualOutput_Click;    //nathan 2023.12.20 믹서 코터 배치연계 
                UcElectrodeCommand.btnSlurryManualInput.Click += ButtonbtnSlurryManualInput_Click;      //nathan 2023.12.20 믹서 코터 배치연계 
                UcElectrodeCommand.btnSlurryBufferManualInit.Click += ButtonbtnSlurryBufferManualInit_Click;  //minylee 2024.02.14 믹서 코터 배치 연계 고도화, 버퍼수동초기화

            }
        }

        private void SeProcessInUserControls()
        {
            if (_processCode.Equals(Process.INS_COATING))
            {
                lblCoatSide.Visibility = Visibility.Visible;
                cboCoatSide.Visibility = Visibility.Visible;
                cboEquipment.SetValue(Grid.ColumnSpanProperty, 1);
            }
            else
            {
                lblCoatSide.Visibility = Visibility.Collapsed;
                cboCoatSide.Visibility = Visibility.Collapsed;
                cboEquipment.SetValue(Grid.ColumnSpanProperty, 3);
            }

            if (GetEqptChgBlock(_processCode).Equals("Y"))
            {
                cboEquipmentSegment.IsEnabled = false;
                cboProcess.IsEnabled = false;
                cboPolarity.IsEnabled = false;
                cboCoatSide.IsEnabled = false;
                cboEquipment.IsEnabled = false;
            }
            else
            {
                cboEquipmentSegment.IsEnabled = true;
                cboProcess.IsEnabled = true;
                cboPolarity.IsEnabled = true;
                cboCoatSide.IsEnabled = true;
                cboEquipment.IsEnabled = true;
            }

            // RollMap
            SetRollMapEquipment();
        }

        private void TimerSetting()
        {

            if (_dispatcherRackRateTimer != null)
            {
                _dispatcherRackRateTimer.Tick -= DispatcherRackRateTimer_Tick;
                _dispatcherRackRateTimer.Tick += DispatcherRackRateTimer_Tick;
                _dispatcherRackRateTimer.Interval = new TimeSpan(0, 0, 60);
                _dispatcherRackRateTimer.Start();
            }

            if (cboAutoSearch.SelectedValue == null || string.IsNullOrWhiteSpace(cboAutoSearch.SelectedValue.ToString()))
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            _timer = new System.Windows.Threading.DispatcherTimer();
            int interval = Convert.ToInt32(cboAutoSearch.SelectedValue);

            _timer.Interval = TimeSpan.FromSeconds(interval);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        #endregion

        #region Event
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

            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);

            this.Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_dispatcherRackRateTimer?.Stop();
        }

        /// <summary>
        /// 라인
        /// </summary>
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlEquipmentSegment();
            SetIdentInfo();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // 공정 
            SetProcessCombo(cboProcess);
            // Clear
            SetControlClear(true);

            HideAllRackRateMode();
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlProcess();
            SetIdentInfo();

            // 전극콤보 사용유무 체크
            CheckUsePolarity(Util.NVC(e.NewValue));

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // 설비 
            SetEquipmentCombo(cboEquipment);

            UcElectrodeCommand.btnRollMap.IsEnabled = false;

            if (string.Equals(cboProcess.SelectedValue?.GetString(), Process.ROLL_PRESSING) && (LoginInfo.CFG_SHOP_ID.Equals("G672") || LoginInfo.CFG_SHOP_ID.Equals("G674")))  // GM2 공장 추가    2023-08-24  장영철
            {
                SelectRackRate();
            }

            HideAllRackRateMode();
        }

        /// <summary>
        /// 극성
        /// </summary>
        private void cboPolarity_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            // 설비 cboPolarity
            SetEquipmentCombo(cboEquipment);

            //cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
        }

        /// <summary>
        /// 설비 
        /// </summary>
        private void cboEquipment_SelectionChanged(object sender, EventArgs e)
        {
            // 메인화면으로
            ButtonProductList_Click(null, null);

            SetEquipment("C");
        }

        private void cboCoatSide_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 메인화면으로
            ButtonProductList_Click(null, null);

            SetEquipment("C");
        }

        /// <summary>
        /// 자동조회
        /// </summary>
        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            TimerSetting();
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ButtonProductList_Click(null, null);

            SetEquipment("L");

            ////////////////////////////////////////////////////////////// Product List, Lot 실적 등록에 따른 조회
            if (grdProduct.Visibility == Visibility.Collapsed)
            {
                // 공정 변경 여부 체크
                if (SelectLotWip() == false)
                {
                    // {%1} LOT은 다음 공정으로 이동 되어 생산 리스트로 화면이 전환 됩니다.
                    Util.MessageValidation("SFU7352", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (grdProductionResult_Mixing.Visibility == Visibility.Visible)
                            {
                                UcResult_Mixing.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_Coating.Visibility == Visibility.Visible)
                            {
                                UcResult_CoatingAuto.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_InsCoating.Visibility == Visibility.Visible)
                            {
                                UcResult_InsCoating.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_HalfSlitting.Visibility == Visibility.Visible)
                            {
                                UcResult_HalfSlitting.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_RollPressing.Visibility == Visibility.Visible)
                            {
                                UcResult_RollPressing.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_Slitting.Visibility == Visibility.Visible)
                            {
                                UcResult_Slitting.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_ReWinding.Visibility == Visibility.Visible)
                            {
                                UcResult_ReWinding.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_ReWinder.Visibility == Visibility.Visible)
                            {
                                UcResult_ReWinder.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_Taping.Visibility == Visibility.Visible)
                            {
                                UcResult_Taping.bProductionUpdate = false;
                            }
                            else if (grdProductionResult_HeatTreatment.Visibility == Visibility.Visible)
                            {
                                UcResult_HeatTreatment.bProductionUpdate = false;
                            }
                            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
                            else if (grdProductionResult_TwoSlitting.Visibility == Visibility.Visible)
                            {
                                UcResult_TwoSlitting.bProductionUpdate = false;
                            }
                            //20211215 2차 Slitting 공정진척DRB 화면 개발 END

                            ButtonProductList_Click(null, null);

                            SelectProductLot(string.Empty);
                            SetProductLotList();
                        }
                    }, _dvProductLot["LOTID"].ToString());
                }
                else
                {
                    // Lot 실적 정보 조회
                    if (_processCode == Process.MIXING ||
                        _processCode == Process.PRE_MIXING ||
                        _processCode == Process.BS ||
                        _processCode == Process.CMC ||
                        _processCode == Process.InsulationMixing)
                    {
                        UcResult_Mixing.SelectProductionResult();
                    }
                    else if (_processCode == Process.COATING)
                    {
                        UcResult_CoatingAuto.SelectProductionResult();
                    }
                    else if (_processCode == Process.INS_COATING)
                    {
                        UcResult_InsCoating.SelectProductionResult();
                    }
                    else if (_processCode == Process.HALF_SLITTING)
                    {
                        UcResult_HalfSlitting.SelectProductionResult();
                    }
                    else if (_processCode == Process.SLITTING)
                    {
                        UcResult_Slitting.SelectProductionResult();
                    }
                    //20211215 2차 Slitting 공정진척DRB 화면 개발 START
                    else if (_processCode == Process.TWO_SLITTING)
                    {
                        UcResult_TwoSlitting.SelectProductionResult();
                    }
                    //20211215 2차 Slitting 공정진척DRB 화면 개발 END
                    else if (_processCode == Process.ROLL_PRESSING)
                    {
                        UcResult_RollPressing.SelectProductionResult();
                    }
                    else if (_processCode == Process.REWINDER)
                    {
                        UcResult_ReWinder.SelectProductionResult();
                    }
                    else if (_processCode == Process.TAPING)
                    {
                        UcResult_Taping.SelectProductionResult();
                    }
                    else if (_processCode == Process.HEAT_TREATMENT)
                    {
                        UcResult_HeatTreatment.SelectProductionResult();
                    }
                    else if (_reWindingProcess == "Y")
                    {
                        UcResult_ReWinding.SelectProductionResult();
                    }
                }
            }
        }

        /// <summary>
        /// 설비 선택 
        /// </summary>
        private void DgEquipment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = DgEquipment.GetCellFromPoint(pnt);

            if (cell != null) return;

            if (DgEquipment.CurrentCell == null) return;

            if (DgEquipment.CurrentCell.Row.IsMouseOver == false) return;

            // 설비 선택은 Product 리스트가 Visible인 경우
            if (grdProduct.Visibility == Visibility.Collapsed) return;

            string Equipment = DgEquipment.CurrentCell.Row.DataItem.ToString();

            if (string.IsNullOrWhiteSpace(Equipment) || Equipment.Split(':').Length < 2)
            {
                _treeEquipmentCode = string.Empty;
                _treeEquipmentName = string.Empty;
            }
            else
            {
                try
                {
                    this.Cursor = Cursors.Wait;

                    ///////////////////////////////////////////////////////////////////// 선택 설비로 정렬
                    _isEquipmentClick = true;

                    _treeEquipmentCode = Equipment.Split(':')[0].Trim();
                    _treeEquipmentName = Equipment.Split(':')[1].Trim();

                    SetEquipment("T");
                    SetUserControlEquipmentDataTable();

                    // Re-Winding 공정은 리스트에 설비가 없다.
                    if (_reWindingProcess == "Y") return;

                    if (UcElectrodeProductLot.chkWait.IsChecked == true) return;

                    // Product Lot 정렬
                    DataTable dt = DataTableConverter.Convert(DgProductLot.ItemsSource);
                    dt.Select().ToList<DataRow>().ForEach(r => r["CHK"] = 0);

                    if (dt == null || dt.Rows.Count == 0) return;

                    DataRow[] drSelect = dt.Select("EQPTID = '" + _treeEquipmentCode + "'");

                    // 해당 설비가 생산 Lot 정보가 없는 경우
                    if (drSelect.Length == 0)
                    {
                        DgProductLot.Refresh();
                        return;
                    }

                    _isEquipmentClick = false;

                    DataRow[] drNoneSelect = dt.Select("EQPTID <> '" + _treeEquipmentCode + "'");
                    DataTable dtSort = dt.Clone();

                    for (int row = 0; row < drSelect.Length; row++)
                    {
                        dtSort.ImportRow(drSelect[row]);
                    }

                    for (int row = 0; row < drNoneSelect.Length; row++)
                    {
                        dtSort.ImportRow(drNoneSelect[row]);
                    }

                    Util.GridSetData(DgProductLot, dtSort, null, true);

                    //////////////////////////////////////////////////////////////////////
                    this.Cursor = Cursors.Arrow;
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);

                if (grdProduct.Visibility == Visibility.Visible)
                {
                    string SelectLotID = _dvProductLot == null ? null : _dvProductLot["LOTID"].ToString();
                    SetProductLotList(SelectLotID);       // Product Lot 조회
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;

                    if (grdProduct.Visibility == Visibility.Visible)
                    {
                        if (LoginInfo.CFG_SHOP_ID.Equals("G672") || LoginInfo.CFG_SHOP_ID.Equals("G674"))   // GM2 추가   2023-08-24  장영철
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

        /// <summary>
        /// 생산(완성) Lot 선택 
        /// </summary>
        private void DgProductLot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null) return;

            RadioButton rb = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;

            if (DgProductLot.CurrentCell.Row.IsMouseOver == false) return;

            ////////////////////////////////////////////
            _isEquipmentClick = false;

            SetProductLotSelect(rb);
        }

        private void DgProductLot_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null) return;

            RadioButton rb = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;

            if (_dvProductLot == null) return;

            //if (DgProductLot.CurrentCell.Row.IsMouseOver == false) return;

            ////////////////////////////////////////////
            //_isEquipmentClick = false;

            //SetProductLotSelect(rb);

            ////////////////////////////////////////////////////////////////////// 화면 전환
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            //if ((!_processCode.Equals(ElectrodeProcesses.SLITTING) && dg.CurrentCell.Column.Name.ToString() == "LOTID") ||
            //    (_processCode.Equals(ElectrodeProcesses.SLITTING) && dg.CurrentCell.Column.Name.ToString() == "CUT_ID"))
            if (((!_processCode.Equals(ElectrodeProcesses.SLITTING) && !_processCode.Equals(ElectrodeProcesses.TWO_SLITTING)) && dg.CurrentCell.Column.Name.ToString() == "LOTID") ||
                ((_processCode.Equals(ElectrodeProcesses.SLITTING) || _processCode.Equals(ElectrodeProcesses.TWO_SLITTING)) && dg.CurrentCell.Column.Name.ToString() == "CUT_ID"))
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            {
                if (_reWindingProcess != "Y" && _dvProductLot["WIPSTAT"].ToString() == Wip_State.WAIT)
                {
                    // Re-Winding 공정은 대기 상태에서 전환 가능.
                    Util.MessageValidation("SFU2063");     // 재공상태를 확인해주세요.
                    return;
                }

                //if (String.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
                //{
                //    // Re-Winding 공정은 설비 선택후.
                //    Util.MessageValidation("SFU1673");      // 설비를 선택 하세요.
                //    return;
                //}

                //this.Cursor = Cursors.Wait;
                dg.CurrentCell.Presenter.Cursor = Cursors.Wait;

                UcElectrodeCommand.SetButtonVisibility(false, _reWindingProcess == "Y" ? true : false);
                UcElectrodeCommand.SetButtonExtraVisibility(false, _reWindingProcess == "Y" ? true : false);

                HideAllRackRateMode();
                grdProduct.Visibility = Visibility.Collapsed;

                ///////////////////////// User Control Visibility
                SetUserControlProductionResultVisibility();

                //this.Cursor = Cursors.Arrow;
                dg.CurrentCell.Presenter.Cursor = Cursors.Hand;
            }
            //else if (dg.CurrentCell.Column.Name.ToString() == "WIPQTY")
            //{
            //    // 재와인딩 공정만 merge 팝업 ghcnf
            //    if (_reWindingProcess == "Y")
            //    {
            //        PopupMergeLotList();
            //    }
            //}

        }

        private void UcElectrodeProductLot_CommandButtonClick(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "btnStart": ButtonStart_Click(sender, null); break;
                case "btnCancel": ButtonCancel_Click(sender, null); break;
                case "btnMove": ButtonMove_Click(sender, null); break;
                case "btnMoveCancel": ButtonMoveCancel_Click(sender, null); break;
                case "btnInput": ButtonInput_Click(sender, null); break;
                case "btnInputCancel": ButtonInputCancel_Click(sender, null); break;
                case "btnEqptEnd": ButtonEqptEnd_Click(sender, null); break;
                case "btnEqptEndCancel": ButtonEqptEndCancel_Click(sender, null); break;
                case "btnEndCancel": ButtonEndCancel_Click(sender, null); break;
                case "btnProductionSchedule": ButtonProductionSchedule_Click(sender, null); break;
                case "btnProductList": ButtonProductList_Click(sender, null); break;
                case "btnWebBreak": ButtonWebBreak_Click(sender, null); break;
                case "btnPilotProdMode": ButtonPilotProdMode_Click(sender, null); break;
                case "btnShipmentModel": ButtonShipmentModel_Click(sender, null); break;
                case "btnDispatch": ButtonDispatch_Click(sender, null); break;
                case "btnCard": ButtonCard_Click(sender, null); break;
                case "btnBarcodeLabel": ButtonBarcodeLabel_Click(sender, null); break;
                case "btnPrint": ButtonPrint_Click(sender, null); break;

                case "btnExtra_MouseLeave": ButtonExtra_MouseLeave(sender, null); break;
                case "btnManualMode": ButtonManualMode_Click(sender, null); break;
                case "btnEqptIssue": ButtonEqptIssue_Click(sender, null); break;
                case "btnCleanLot": ButtonCleanLot_Click(sender, null); break;
                case "btnCancelFCut": ButtonCancelFCut_Click(sender, null); break;
                case "btnCancelDelete": ButtonCancelDelete_Click(sender, null); break;
                case "btnCut": ButtonCut_Click(sender, null); break;
                case "btnInvoiceMaterial": ButtonInvoiceMaterial_Click(sender, null); break;
                case "btnEqptCond": ButtonEqptCond_Click(sender, null); break;
                case "btnMixConfirm": ButtonMixConfirm_Click(sender, null); break;
                case "btnSamplingProd": ButtonSamplingProd_Click(sender, null); break;
                case "btnProcReturn": ButtonProcReturn_Click(sender, null); break;
                case "btnSamplingProdT1": ButtonSamplingProdT1_Click(sender, null); break;
                case "btnMixerTankInfo": ButtonMixerTankInfo_Click(sender, null); break;
                case "btnReservation": ButtonReservation_Click(sender, null); break;
                case "btnFoil": ButtonFoil_Click(sender, null); break;
                case "btnSlurryConf": ButtonSlurryConf_Click(sender, null); break;
                case "btnStartCoaterCut": ButtonStartCoaterCut_Click(sender, null); break;
                case "btnWorkHalfSlitSide": ButtonWorkHalfSlitSide_Click(sender, null); break;
                case "btnEmSectionRollDirctn": ButtonEmSectionRollDirctn_Click(sender, null); break;
                case "btnLogisStat": ButtonLogisStat_Click(sender, null); break;
                case "btnSkidTypeSettingByPort": ButtonSkidTypeSettingByPort_Click(sender, null); break;
                case "btnSlBatch": ButtonSlBatch_Click(sender, null); break;
                case "btnScrapLot": ButtonScrapLot_Click(sender, null); break;
                case "btnSaveRegDefectLane": ButtonSaveRegDefectLane_Click(sender, null); break;
                case "btnRollMapInputMaterial": ButtonRollMapInputMaterial_Click(sender, null); break;
            }
        }

        /// <summary>
        /// 작업시작
        /// </summary>
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (_processCode == Process.MIXING ||
                _processCode == Process.PRE_MIXING ||
                _processCode == Process.BS ||
                _processCode == Process.CMC ||
                _processCode == Process.InsulationMixing ||
                _processCode == Process.DAM_MIXING
                )
            {
                // Mixing, 선분산 Mixing, Binder Solution, CMC Solution, DAM MIXING
                PopupStartMixing();
            }
            else if (_processCode == Process.COATING)
            {
                PopupStartCoating();
            }
            else if (_processCode == Process.INS_COATING)
            {
                PopupStartInsCoating();
            }
            else if (_processCode == Process.HALF_SLITTING)
            {
                PopupStartHalfSlitting();
            }
            else if (_processCode == Process.SLITTING)
            {
                PopupStartSlitting();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (_processCode == Process.TWO_SLITTING)
            {
                PopupStartTwoSlitting();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else if (_processCode == Process.ROLL_PRESSING)
            {
                PopupStartRollPressing();
            }
            else if (_processCode == Process.REWINDER)
            {
                PopupStartReWinder();
            }
            else if (_processCode == Process.TAPING)
            {
                PopupStartTaping();
            }
            else if (_processCode == Process.HEAT_TREATMENT)
            {
                PopupStartHeatTreatment();
            }
            else if (_reWindingProcess == "Y")
            {
                PopupStartReWinding();
            }
        }

        /// <summary>
        /// 시작취소
        /// </summary>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_reWindingProcess == "Y")
            {
                if (!ValidationCancelReWinding()) return;
            }
            else
            {
                if (!ValidationCancel()) return;
            }

            // 선택된 LOT을 작업 취소하시겠습니까? 
            Util.MessageConfirm("SFU3151", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (_reWindingProcess == "Y")
                        CancelProcessReWinding();
                    else if (_processCode == Process.HALF_SLITTING)
                        CancelProcessHalfSlutting();
                    else if (_processCode == Process.SLITTING)
                        CancelProcessSlitting();
                    //20211215 2차 Slitting 공정진척DRB 화면 개발 START
                    else if (_processCode == Process.TWO_SLITTING)
                        CancelProcessTwoSlitting();
                    //20211215 2차 Slitting 공정진척DRB 화면 개발 END
                    else
                        CancelProcess();
                }
            });
        }

        /// <summary>
        /// 이동
        /// </summary>
        private void ButtonMove_Click(object sender, RoutedEventArgs e)
        {
            PopupMove(true);
        }

        /// <summary>
        /// 이동취소
        /// </summary>
        private void ButtonMoveCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupMove(false);
        }

        /// <summary>
        /// 투입
        /// </summary>
        private void ButtonInput_Click(object sender, RoutedEventArgs e)
        {
            PopupInput(true);
        }

        /// <summary>
        /// 투입취소
        /// </summary>
        private void ButtonInputCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupInput(false);
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void ButtonEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckButtonPermissionGroupByBtnGroupID("EQPT_END_W")) return;

            if (_processCode == Process.MIXING ||
                _processCode == Process.PRE_MIXING ||
                _processCode == Process.BS ||
                _processCode == Process.CMC ||
                _processCode == Process.InsulationMixing ||
                _processCode == Process.DAM_MIXING
                )
            {
                PopupEqptEndMixing();
            }
            else if (_processCode == Process.COATING)
            {
                PopupEqptEndCoating();
            }
            else if (_processCode == Process.INS_COATING)
            {
                PopupEqptEndInsCoating();
            }
            else if (_reWindingProcess == "Y")
            {
                EqptEndProcessRewinder();
            }
            else if (_processCode == Process.HALF_SLITTING)
            {
                PopupEqptEndHalfSlitting();
            }
            else if (_processCode == Process.SLITTING)
            {
                PopupEqptEndSlitting();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (_processCode == Process.TWO_SLITTING)
            {
                PopupEqptEndTwoSlitting();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else if (_processCode == Process.ROLL_PRESSING)
            {
                PopupEqptEndRollPressing();
            }
            else if (_processCode == Process.REWINDER)
            {
                PopupEqptEndReWinder();
            }
            else if (_processCode == Process.TAPING)
            {
                PopupEqptEndTaping();
            }
            else if (_processCode == Process.HEAT_TREATMENT)
            {
                PopupEqptEndHeatTreatment();
            }
        }

        /// <summary>
        /// 장비완료취소
        /// </summary>
        private void ButtonEqptEndCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptEndCancel()) return;

            // 선택된 LOT을 장비 완료 취소 하시겠습니까? 투입된 LOT의 LOT/CARRIER 실물정보가 변경이 없다면 CAR-LOT 매핑을 진행해 주세요.
            Util.MessageConfirm("SFU5190", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (_reWindingProcess == "Y")
                        EqptEndCancelProcessRewinder();
                    else
                        EqptEndCancelProcess();
                }
            });
        }

        /// <summary>
        /// 실적확정취소
        /// </summary>
        private void ButtonEndCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEndCancel()) return;

            // 해당 확정 처리 된 Lot을 실적취소하시겠습니까?
            Util.MessageConfirm("SFU5147", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EqptEndCancelProcess();
                }
            });
        }

        /// <summary>
        /// 생산 Schedule 조회 => 미정의
        /// </summary>
        private void ButtonProductionSchedule_Click(object sender, RoutedEventArgs e)
        {
        }

        public void ButtonProductList_Click(object sender, RoutedEventArgs e)
        {
            UcElectrodeCommand.SetButtonVisibility(true, _reWindingProcess == "Y" ? true : false);
            UcElectrodeCommand.SetButtonExtraVisibility(true, _reWindingProcess == "Y" ? true : false);

            grdProduct.Visibility = Visibility.Visible;

            ///////////////////// 실적 User Control Visibility 및 Product List 갱신
            bool Refresh = false;

            if (grdProductionResult_Mixing.Visibility == Visibility.Visible)
            {
                grdProductionResult_Mixing.Visibility = Visibility.Collapsed;

                Refresh = UcResult_Mixing.bProductionUpdate;
                UcResult_Mixing.bProductionUpdate = false;
            }
            else if (grdProductionResult_Coating.Visibility == Visibility.Visible)
            {
                grdProductionResult_Coating.Visibility = Visibility.Collapsed;

                Refresh = UcResult_CoatingAuto.bProductionUpdate;
                UcResult_CoatingAuto.bProductionUpdate = false;
            }
            else if (grdProductionResult_InsCoating.Visibility == Visibility.Visible)
            {
                grdProductionResult_InsCoating.Visibility = Visibility.Collapsed;

                Refresh = UcResult_InsCoating.bProductionUpdate;
                UcResult_InsCoating.bProductionUpdate = false;
            }
            else if (grdProductionResult_HalfSlitting.Visibility == Visibility.Visible)
            {
                grdProductionResult_HalfSlitting.Visibility = Visibility.Collapsed;

                Refresh = UcResult_HalfSlitting.bProductionUpdate;
                UcResult_HalfSlitting.bProductionUpdate = false;
            }
            else if (grdProductionResult_RollPressing.Visibility == Visibility.Visible)
            {
                grdProductionResult_RollPressing.Visibility = Visibility.Collapsed;

                Refresh = UcResult_RollPressing.bProductionUpdate;
                UcResult_RollPressing.bProductionUpdate = false;
            }
            else if (grdProductionResult_Slitting.Visibility == Visibility.Visible)
            {
                grdProductionResult_Slitting.Visibility = Visibility.Collapsed;

                Refresh = UcResult_Slitting.bProductionUpdate;
                UcResult_Slitting.bProductionUpdate = false;
            }
            else if (grdProductionResult_ReWinding.Visibility == Visibility.Visible)
            {
                grdProductionResult_ReWinding.Visibility = Visibility.Collapsed;

                Refresh = UcResult_ReWinding.bProductionUpdate;
                UcResult_ReWinding.bProductionUpdate = false;
                UcResult_ReWinding.dgProductResult.EndEdit();
            }
            else if (grdProductionResult_ReWinder.Visibility == Visibility.Visible)
            {
                grdProductionResult_ReWinder.Visibility = Visibility.Collapsed;

                Refresh = UcResult_ReWinder.bProductionUpdate;
                UcResult_ReWinder.bProductionUpdate = false;
            }
            else if (grdProductionResult_Taping.Visibility == Visibility.Visible)
            {
                grdProductionResult_Taping.Visibility = Visibility.Collapsed;

                Refresh = UcResult_Taping.bProductionUpdate;
                UcResult_Taping.bProductionUpdate = false;
            }
            else if (grdProductionResult_HeatTreatment.Visibility == Visibility.Visible)
            {
                grdProductionResult_HeatTreatment.Visibility = Visibility.Collapsed;

                Refresh = UcResult_HeatTreatment.bProductionUpdate;
                UcResult_HeatTreatment.bProductionUpdate = false;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (grdProductionResult_TwoSlitting.Visibility == Visibility.Visible)
            {
                grdProductionResult_TwoSlitting.Visibility = Visibility.Collapsed;

                Refresh = UcResult_TwoSlitting.bProductionUpdate;
                UcResult_TwoSlitting.bProductionUpdate = false;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END

            ///////////////////////////////////////////////////////// 실적 또는 불량/LOSS/물품청구 변경시 생산 Lot 재조회
            if (Refresh)
            {
                string SelectLotID = _dvProductLot["LOTID"].ToString();
                SelectProductLot(SelectLotID);
                SetProductLotList(SelectLotID);
            }
        }

        /// <summary>
        /// 단선추가
        /// </summary>
        private void ButtonWebBreak_Click(object sender, RoutedEventArgs e)
        {
            PopupWebBreak();
        }

        /// <summary>
        /// 시생산설정/해제
        /// </summary>
        private void ButtonPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPilotProdMode()) return;

            string sHypen = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
            DataRow[] drPilot = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '0' AND HYPHEN = '" + sHypen + "'");

            string sMessage = string.Empty;
            //bool bMode;
            string sToMode = string.Empty;

            if (drPilot.Length == 0 || drPilot[0]["INPUT_YN"].ToString() == "N")
            {
                // [%1]을 시생산 설정하시겠습니까?
                sMessage = "SFU8303";
                //bMode = true;
                sToMode = "PILOT";
            }
            else
            {
                // [%1]을 시생산 해제하시겠습니까?
                sMessage = "SFU8304";
                //bMode = false;
                sToMode = string.Empty;
            }

            Util.MessageConfirm(sMessage, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 모드 변경
                    SavePilotProdMode(sToMode);
                }
            }, _equipmentName);

        }

        /// <summary>
        /// 2021.08.12 : 시생산샘플설정/해제 기능 추가
        /// 시생산샘플설정/해제
        /// </summary>
        private void ButtonPilotProdSPMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPilotProdMode()) return;

            string sHypen = ObjectDic.Instance.GetObjectName("PILOT_SMPL_PROD");
            DataRow[] drPilotSP = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '0' AND HYPHEN = '" + sHypen + "'");

            string sMessage = string.Empty;
            string sToMode = string.Empty;

            if (drPilotSP.Length == 0 || drPilotSP[0]["INPUT_YN"].ToString() == "N")
            {
                sMessage = "SFU8189";    //[%1]을 시생산샘플 설정하시겠습니까?
                sToMode = "PILOT_S";
            }
            else
            {
                sMessage = "SFU8188";    //[%1]을 시생산샘플 해제하시겠습니까?
                sToMode = string.Empty;
            }

            Util.MessageConfirm(sMessage, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 모드 변경
                    SavePilotProdMode(sToMode);
                }
            }, _equipmentName);
        }

        /// <summary>
        /// 출하모델
        /// </summary>
        protected void ButtonShipmentModel_Click(object sender, EventArgs e)
        {
            CMM_ELEC_SHIPMENTMODEL popup = new CMM_ELEC_SHIPMENTMODEL();
            popup.FrameOperation = FrameOperation;

            //if (EQUIPMENT_COMBO.SelectedIndex > 0) //&& !string.IsNullOrEmpty(sWOID)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _processCode;

                C1WindowExtension.SetParameters(popup, Parameters);

                Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                popup.CenterOnScreen();
            }
        }

        /// <summary>
        /// 실적확정(공정이동)
        /// </summary>
        private void ButtonDispatch_Click(object sender, RoutedEventArgs e)
        {
            if (_processCode == Process.COATING)
            {
                if (!CheckButtonPermissionGroupByBtnGroupID("MOVE_W")) return;
            }
            else
            {
                if (!CheckButtonPermissionGroupByBtnGroupID("CONFIRM_W")) return;
            }

            // 설비 재조회(작업조,작업자 갱신)
            SelectWorkUser();

            ///////////////////////////////////////////////  Validation
            if (_processCode == Process.MIXING ||
                _processCode == Process.PRE_MIXING ||
                _processCode == Process.BS ||
                _processCode == Process.CMC ||
                _processCode == Process.InsulationMixing ||
                _processCode == Process.DAM_MIXING)
            {
                // [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부 Start
                // 원재료 Lot 누락 Validation 기능 적용 Check
                CheckUseElecMtrlLotValidation();

                if (_isELEC_MTRL_LOT_VALID_YN)
                {
                    //미 투입자재 존재 유무 Check
                    if (CheckMissedElecMtrlLot())
                    {
                        //미투입 자재 PopUp
                        PopupInputMaterial();
                        return;
                    }
                }
                // [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부 End

                if (!ValidationDispatchMixing()) return;
            }
            else if (_processCode == Process.COATING)
            {
                if (!ValidationDispatchCoating()) return;
            }
            else if (_processCode == Process.INS_COATING)
            {
                if (!ValidationDispatchInsCoating()) return;
            }
            else if (_processCode == Process.HALF_SLITTING)
            {
                if (!ValidationDispatchHalfSlitting()) return;
            }
            else if (_processCode == Process.SLITTING)
            {
                if (!ValidationDispatchSlitting()) return;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (_processCode == Process.TWO_SLITTING)
            {
                if (!ValidationDispatchTwoSlitting()) return;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else if (_processCode == Process.ROLL_PRESSING)
            {
                if (!ValidationDispatchRollPressing()) return;
            }
            else if (_processCode == Process.REWINDER)
            {
                if (!ValidationDispatchReWinder()) return;
            }
            else if (_processCode == Process.TAPING)
            {
                if (!ValidationDispatchTaping()) return;
            }
            else if (_processCode == Process.HEAT_TREATMENT)
            {
                if (!ValidationDispatchHeatTreatment()) return;
            }
            else if (_reWindingProcess == "Y")
            {
                UcResult_ReWinding.dgProductResult.EndEdit();

                if (!ValidationDispatchReWinding()) return;
            }

            ///////////////////////////////////////////////  실적확정
            CheckLabelPassHold(() =>
            {
                CheckAuthValidation(() =>
                {
                    CheckSpecOutHold(() =>
                    {
                        if (_processCode == Process.MIXING ||
                            _processCode == Process.PRE_MIXING ||
                            _processCode == Process.BS ||
                            _processCode == Process.CMC ||
                            _processCode == Process.InsulationMixing ||
                            _processCode == Process.HALF_SLITTING ||
                            _processCode == Process.INS_COATING ||
                            _processCode == Process.SLITTING ||
                            _processCode == Process.TWO_SLITTING || //20211215 2차 Slitting 공정진척DRB 화면 개발
                            _processCode == Process.REWINDER ||
                            _processCode == Process.TAPING ||
                            _processCode == Process.HEAT_TREATMENT ||
                            _processCode == Process.DAM_MIXING)
                        {
                            ConfirmCheck();
                        }
                        else if (_processCode == Process.COATING)
                        {
                            // 2023.10.10 조성근- 롤맵 홀드는 수동 선택이 아닌, 자동 선택 ( E20231005-000782 )
                            //DataTable dtHold = GetRollMapHold(_processCode);
                            //if (IsRollMapEquipment() && CommonVerify.HasTableRow(dtHold))
                            //{
                            //    //롤맵 Hold 기능 추가
                            //    holdLotClassCode.Clear();
                            //    if (dtHold.Columns.Contains("ADJ_LOTID") && dtHold.Columns.Contains("HOLD_CLASS_CODE"))
                            //    {
                            //        holdLotClassCode.Add(dtHold.Rows[0]["ADJ_LOTID"].ToString(), dtHold.Rows[0]["HOLD_CLASS_CODE"].ToString());
                            //    }

                            //    #region # Roll-Map Hold 팝업 호출
                            //    CMM_ROLLMAP_HOLD rollMapHoldPopup = new CMM_ROLLMAP_HOLD();
                            //    rollMapHoldPopup.FrameOperation = FrameOperation;

                            //    if (rollMapHoldPopup != null)
                            //    {
                            //        object[] parameters = new object[10];
                            //        parameters[0] = _processCode;
                            //        parameters[1] = _equipmentCode;
                            //        parameters[2] = Util.NVC(_dvProductLot["LOTID"]);
                            //        parameters[3] = Util.NVC(_dvProductLot["WIPSEQ"]);
                            //        parameters[4] = _equipmentName;
                            //        parameters[5] = GetVersion();
                            //        C1WindowExtension.SetParameters(rollMapHoldPopup, parameters);

                            //        rollMapHoldPopup.Closed += new EventHandler(popupRollMapHold_Closed);
                            //        this.Dispatcher.BeginInvoke(new Action(() => rollMapHoldPopup.ShowModal()));
                            //    }
                            //    #endregion
                            //}
                            //else                            

                            if (_isRollMapEquipment && _isRollMapLot && _isRollMapDivReportEqpt)
                            {
                                //황석동 2024.12.01 롤맵 코터 수불 1.5V - 불량수량 변경 감지
                                //여기서 변경 되었는지 확인
                                if (UcResult_CoatingAuto.CheckDefectLen() == false)
                                {
                                    Util.MessageValidation("SFU10033");
                                    return;
                                }
                                else
                                {
                                    PopupElecHold();
                                }
                            }
                            else
                            {
                                PopupElecHold();
                            }

                        }
                        else if (_processCode == Process.ROLL_PRESSING)
                        {
                            // 2023.10.10 조성근- 롤맵 홀드는 수동 선택이 아닌, 자동 선택 ( E20231005-000782 )
                            //DataTable dtHold = GetRollMapHold(_processCode);
                            //// 롤프레스 공정 수불 미적용 인 경우에도 롤맵 홀드 팝업 호출 요청(ESGM)
                            //if (IsRollMapEquipment() && CommonVerify.HasTableRow(dtHold))
                            //{
                            //    //롤맵 Hold 기능 추가
                            //    holdLotClassCode.Clear();
                            //    if (dtHold.Columns.Contains("ADJ_LOTID"))
                            //    {
                            //        holdLotClassCode.Add(dtHold.Rows[0]["ADJ_LOTID"].ToString(), "E3000");
                            //    }

                            //    #region # Roll-Map Hold 팝업 호출
                            //    CMM_ROLLMAP_HOLD rollMapHoldPopup = new CMM_ROLLMAP_HOLD();
                            //    rollMapHoldPopup.FrameOperation = FrameOperation;

                            //    if (rollMapHoldPopup != null)
                            //    {
                            //        object[] parameters = new object[10];
                            //        parameters[0] = _processCode;
                            //        parameters[1] = _equipmentCode;
                            //        parameters[2] = Util.NVC(_dvProductLot["LOTID"]);
                            //        parameters[3] = Util.NVC(_dvProductLot["WIPSEQ"]);
                            //        parameters[4] = _equipmentName;
                            //        parameters[5] = GetVersion();
                            //        C1WindowExtension.SetParameters(rollMapHoldPopup, parameters);

                            //        rollMapHoldPopup.Closed += new EventHandler(popupRollMapHold_Closed);
                            //        this.Dispatcher.BeginInvoke(new Action(() => rollMapHoldPopup.ShowModal()));
                            //    }
                            //    #endregion
                            //}
                            //else
                            //조성근 2024.08.28 롤프레스 롤맵 실적 적용
                            if (_isRollMapEquipment && _isRollMapLot && _isRollMapDivReportEqpt)
                            {
                                RollmapConfirmCheck();
                            }
                            else
                            {
                                ConfirmCheck();
                            }
                        }
                        else if (_reWindingProcess == "Y")
                        {
                            PopupConfirmUser();
                        }

                    });
                });
            });
        }
        //조성근 2024.08.28 롤프레스 롤맵 실적 적용
        private void RollmapConfirmCheck()
        {
            try
            {
                //1. RESNCODE확인 --DA_PRD_SEL_RM_RPT_DEFECT_RP_CHK     
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("WIPSEQ", typeof(decimal));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = _dvProductLot["LOTID"].ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                dr["WIPSEQ"] = _dvProductLot["WIPSEQ"].ToString();
                dr["PROCID"] = _processCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_DEFECT_RP_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (dtResult.Rows[i]["RESNCODE"].ToString().IsEmpty())
                        {
                            Util.MessageValidation("SFU10006", dtResult.Rows[i]["EQPT_MEASR_PSTN_ID"].ToString());
                            return;
                        }

                    }
                }

                //조성근 2024.10.10 롤맵 롤프레스 수불 - 최외곽폐기 변경 감지
                //여기서 변경 되었는지 확인
                if (UcResult_RollPressing.CheckOutSideDefectLen() == false)
                {
                    Util.MessageValidation("SFU10008");
                    return;
                }

                if(UcResult_RollPressing.CheckDefectByActID() == false)
                {
                    Util.MessageValidation("SFU10038");
                    return;
                }

                //2. 최외곽폐기 수집여부 확인
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LOTID", typeof(string));
                RQSTDT2.Columns.Add("WIPSEQ", typeof(decimal));
                RQSTDT2.Columns.Add("ROLLMAP_ADJ_INFO_TYPE_CODE", typeof(string));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LOTID"] = _dvProductLot["LOTID"].ToString();
                dr2["WIPSEQ"] = _dvProductLot["WIPSEQ"].ToString();
                dr2["ROLLMAP_ADJ_INFO_TYPE_CODE"] = "CONFIRM_OUTSIDE_SCRAP";
                RQSTDT2.Rows.Add(dr2);

                DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_ADJ_INFO", "RQSTDT", "RSLTDT", RQSTDT2);

                if (dtResult2 != null && dtResult2.Rows.Count > 0)
                {
                    PopupConfirmUser();
                }
                else
                {
                    Util.MessageConfirm("SFU10007", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            PopupConfirmUser();
                        }
                    });
                }
                return;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                //오류가 나더라도 공정진행은 계속되도록...
                PopupConfirmUser();
            }
        }

        /// <summary>
        /// 이력카드
        /// </summary>
        private void ButtonCard_Click(object sender, RoutedEventArgs e)
        {
            PopupReport();
        }

        /// <summary>
        /// 바코드발행
        /// </summary>
        private void ButtonBarcodeLabel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBarcodeLabel()) return;

            DataTable printDT = GetPrintCount();

            if (printDT.Rows.Count > 0 && Util.NVC_Decimal(printDT.Rows[0]["PRT_COUNT1"]) > 0)
            {
                // 이미 해당 공정에서 발행된 Lot인데 재 발행하시겠습니까?
                Util.MessageConfirm("SFU3463", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            int iSamplingCount;

                            // 2021.12.30. ESNJ 1동 생산시스템 구축. 기존 Slitting 공정진척의 바코드 발행 로직과 동일하게 Slitting Lot 개수만큼 출력하도록 전극공전진척에 추가. 다른공정은 기존 로직 유지
                            if (string.Equals(_processCode, Process.SLITTING) || string.Equals(_processCode, Process.TWO_SLITTING))
                            {
                                DataTable LabelDT = null;

                                if (string.Equals(_processCode, Process.SLITTING))
                                    LabelDT = DataTableConverter.Convert(UcResult_Slitting.dgProductResult.ItemsSource);
                                else
                                    LabelDT = DataTableConverter.Convert(UcResult_TwoSlitting.dgProductResult.ItemsSource);

                                DataTable sampleDT = new DataTable();
                                sampleDT.Columns.Add("CUT_ID", typeof(string));
                                sampleDT.Columns.Add("LOTID", typeof(string));
                                sampleDT.Columns.Add("COMPANY", typeof(string));
                                DataRow dRow = null;

                                foreach (DataRow _iRow in LabelDT.Rows)
                                {
                                    iSamplingCount = 0;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }
                                    for (int i = 0; i < iSamplingCount; i++)
                                    {
                                        dRow = sampleDT.NewRow();
                                        dRow["CUT_ID"] = _iRow["CUT_ID"];
                                        dRow["LOTID"] = _iRow["LOTID"];
                                        dRow["COMPANY"] = i > sCompany.Length - 1 ? "" : sCompany[i];
                                        sampleDT.Rows.Add(dRow);
                                    }
                                }

                                var sortdt = sampleDT.AsEnumerable().OrderBy(x => x.Field<string>("CUT_ID") + x.Field<string>("COMPANY")).CopyToDataTable();

                                for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                    for (int i = 0; i < sortdt.Rows.Count; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(sortdt.DefaultView[i]["LOTID"]), _processCode, Util.NVC(sortdt.DefaultView[i]["COMPANY"]));
                            }
                            else
                            {
                                for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                                {
                                    iSamplingCount = 1;
                                    string[] sCompany = null;
                                    foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(_dvProductLot["LOTID"].ToString()))
                                    {
                                        iSamplingCount = Util.NVC_Int(items.Key);
                                        sCompany = Util.NVC(items.Value).Split(',');
                                    }

                                    for (int i = 0; i < iSamplingCount; i++)
                                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, _dvProductLot["LOTID"].ToString(), _processCode, i > sCompany.Length - 1 ? "" : sCompany[i]);
                                }
                            }
                        }
                        catch (Exception ex) { Util.MessageException(ex); }
                    }
                });
            }
            else
            {
                try
                {
                    int iSamplingCount;

                    // 2021.12.30. ESNJ 1동 생산시스템 구축. 기존 Slitting 공정진척의 바코드 발행 로직과 동일하게 Slitting Lot 개수만큼 출력하도록 전극공전진척에 추가. 다른공정은 기존 로직 유지
                    if (string.Equals(_processCode, Process.SLITTING) || string.Equals(_processCode, Process.TWO_SLITTING))
                    {
                        DataTable LabelDT = null;

                        if (string.Equals(_processCode, Process.SLITTING))
                            LabelDT = DataTableConverter.Convert(UcResult_Slitting.dgProductResult.ItemsSource);
                        else
                            LabelDT = DataTableConverter.Convert(UcResult_TwoSlitting.dgProductResult.ItemsSource);

                        DataTable sampleDT = new DataTable();
                        sampleDT.Columns.Add("CUT_ID", typeof(string));
                        sampleDT.Columns.Add("LOTID", typeof(string));
                        sampleDT.Columns.Add("COMPANY", typeof(string));
                        DataRow dRow = null;

                        foreach (DataRow _iRow in LabelDT.Rows)
                        {
                            iSamplingCount = 0;
                            string[] sCompany = null;
                            foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                            {
                                iSamplingCount = Util.NVC_Int(items.Key);
                                sCompany = Util.NVC(items.Value).Split(',');
                            }
                            for (int i = 0; i < iSamplingCount; i++)
                            {
                                dRow = sampleDT.NewRow();
                                dRow["CUT_ID"] = _iRow["CUT_ID"];
                                dRow["LOTID"] = _iRow["LOTID"];
                                dRow["COMPANY"] = i > sCompany.Length - 1 ? "" : sCompany[i];
                                sampleDT.Rows.Add(dRow);
                            }
                        }

                        var sortdt = sampleDT.AsEnumerable().OrderBy(x => x.Field<string>("CUT_ID") + x.Field<string>("COMPANY")).CopyToDataTable();

                        for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                            for (int i = 0; i < sortdt.Rows.Count; i++)
                                Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(sortdt.DefaultView[i]["LOTID"]), _processCode, Util.NVC(sortdt.DefaultView[i]["COMPANY"]));
                    }
                    else
                        Util.PrintLabel_Elec(FrameOperation, loadingIndicator, _dvProductLot["LOTID"].ToString(), _processCode);
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }
        }

        /// <summary>
        /// 발행
        /// </summary>
        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            PopupPrint();
        }

        /// <summary>
        /// RollMap 
        /// </summary>
        protected void ButtonRollMap_Click(object sender, EventArgs e)
        {
            string strVersion = string.Empty;
            int intLaneQty = 0;

            // 버전정보 체크
            strVersion = GetVersion();
            if (string.IsNullOrEmpty(strVersion)) return;

            // Lane Qty
            intLaneQty = GetLaneQty();

            // Roll Map 호출 
            string MAINFORMPATH = "LGC.GMES.MES.COM001";
            string MAINFORMNAME;

            string processCode;
            string equipmentCode;
            string equipmentName;
            string lotCode;
            string wipSeq;

            if (_dvProductLot == null)
            {
                UcElectrodeCommand.btnRollMap.IsEnabled = false;
                return;
            }

            if (string.Equals(_processCode, Process.ROLL_PRESSING))
            {
                if (string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.PROC))
                {
                    MAINFORMNAME = "COM001_RM_CHART_CT";
                    lotCode = string.Empty;
                    wipSeq = "1";
                    processCode = Process.COATING;
                    equipmentName = string.Empty;

                    DataTable dtRollMapInputLot = GetRollMapInputLotCode(Util.NVC(_dvProductLot["LOTID"]));

                    if (CommonVerify.HasTableRow(dtRollMapInputLot))
                    {
                        lotCode = dtRollMapInputLot.Rows[0]["INPUT_LOTID"].GetString();

                        DataTable dtEquipment = GetEquipmentCode(lotCode, wipSeq);

                        if (CommonVerify.HasTableRow(dtEquipment))
                        {
                            equipmentCode = dtEquipment.Rows[0]["EQPTID"].GetString();
                        }
                        else
                        {
                            equipmentCode = string.Empty;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    MAINFORMNAME = "COM001_RM_CHART_RP";
                    lotCode = Util.NVC(_dvProductLot["LOTID"]);
                    wipSeq = Util.NVC(_dvProductLot["WIPSEQ"]);
                    processCode = _processCode;
                    equipmentCode = _equipmentCode;
                    equipmentName = _equipmentName;
                }
            }
            else if (string.Equals(_processCode, Process.COATING))
            {
                MAINFORMNAME = "COM001_RM_CHART_CT";
                lotCode = Util.NVC(_dvProductLot["LOTID"]);
                wipSeq = Util.NVC(_dvProductLot["WIPSEQ"]);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName;
            }
            else if (string.Equals(_processCode, Process.SLIT_REWINDING) || string.Equals(_processCode, Process.REWINDING))
            {
                MAINFORMNAME = "COM001_RM_CHART_RW";
                lotCode = Util.NVC(_dvProductLot["LOTID"]);
                wipSeq = Util.NVC(_dvProductLot["WIPSEQ"]);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName;
            }
            else if (string.Equals(_processCode, Process.SLITTING))
            {
                MAINFORMNAME = "COM001_RM_CHART_SL";
                lotCode = Util.NVC(_dvProductLot["LOTID"]);
                wipSeq = Util.NVC(_dvProductLot["WIPSEQ"]);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName;
            }
            else if (string.Equals(_processCode, Process.TWO_SLITTING))
            {
                MAINFORMNAME = "COM001_ROLLMAP_TWOSLITTING";
                lotCode = Util.NVC(_dvProductLot["LOTID"]);
                wipSeq = Util.NVC(_dvProductLot["WIPSEQ"]);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName;
            }
            else
            {
                MAINFORMNAME = "COM001_ROLLMAP_COMMON";
                lotCode = Util.NVC(_dvProductLot["LOTID"]);
                wipSeq = Util.NVC(_dvProductLot["WIPSEQ"]);
                processCode = _processCode;
                equipmentCode = _equipmentCode;
                equipmentName = _equipmentName;
            }

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] Parameters = new object[10];
            Parameters[0] = processCode;
            Parameters[1] = _equipmentSegmentCode;
            Parameters[2] = equipmentCode;
            Parameters[3] = lotCode;
            Parameters[4] = wipSeq;
            Parameters[5] = intLaneQty;
            Parameters[6] = equipmentName;
            Parameters[7] = strVersion;

            C1Window popupRollMap = obj as C1Window;
            popupRollMap.Closed += new EventHandler(PopupRollMap_Closed);
            C1WindowExtension.SetParameters(popupRollMap, Parameters);
            if (popupRollMap != null)
            {
                popupRollMap.ShowModal();
                popupRollMap.CenterOnScreen();
            }
        }


        /// <summary>
        /// 고객인증팝업
        /// </summary>
        protected void ButtonbtnCustomer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return;
            }
            CMM_ELEC_CUSTOMER_GROUP_SEARCH wndMixerTankInfo = new CMM_ELEC_CUSTOMER_GROUP_SEARCH { FrameOperation = FrameOperation };

            if (wndMixerTankInfo != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];
                Parameters[0] = _equipmentSegmentCode.ToString();
                Parameters[1] = _equipmentCode.ToString();
                Parameters[2] = _processCode;
                C1WindowExtension.SetParameters(wndMixerTankInfo, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => wndMixerTankInfo.ShowModal()));
            }

        }


        /// <summary>
        /// Verification 대기재공조회
        /// </summary>
        protected void ButtonbtnSearchWaitingWork_Click(object sender, EventArgs e)
        {

            try
            {
                // 라인을선택하세요
                if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
                {
                    Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                    return;
                }
                if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
                {
                    Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                    return;
                }
                ELEC003_WAITWORK popWaitingWipLot = new ELEC003_WAITWORK() { FrameOperation = FrameOperation }; ;

                if (popWaitingWipLot != null)
                {
                    UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                    object[] Parameters = new object[1];
                    Parameters[0] = _equipmentSegmentCode.ToString();
                    C1WindowExtension.SetParameters(popWaitingWipLot, Parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => popWaitingWipLot.ShowModal()));
                }

                //object[] Parameters = new object[1];
                //Parameters[0] = cboEquipmentSegment.SelectedValue;

                //C1WindowExtension.SetParameters(popWaitingWipLot, Parameters);

                //popWaitingWipLot.Closed += new EventHandler(popWaitingWipLot_Closed);

                //this.Dispatcher.BeginInvoke(new Action(() => popWaitingWipLot.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

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
                    if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
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

                    //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
                    if (_util.IsCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(_processCode, Process.ROLL_PRESSING))
                    {
                        CMM_CONDITION_RETURN_ESWA popReturn = new CMM_CONDITION_RETURN_ESWA() { FrameOperation = FrameOperation }; ;
                        if (popReturn != null)
                        {
                            UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                            object[] Parameters = new object[7];
                            Parameters[0] = _equipmentCode.ToString(); //설비코드
                            Parameters[1] = UcElectrodeProductLot.txtSelectEquipmentName.Text; // 설비코드명
                            Parameters[2] = dt.Rows[0]["PRJT_NAME"].ToString(); //PJT
                            Parameters[3] = dt.Rows[0]["PROD_VER_CODE"].ToString(); //Version
                            Parameters[4] = _processCode; //공정
                            Parameters[5] = "E"; //전극 화면에서 호출
                            Parameters[6] = dt.Rows[0]["WOID"].ToString(); //WOID

                            C1WindowExtension.SetParameters(popReturn, Parameters);
                            popReturn.Closed += new EventHandler(popReturn_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => popReturn.ShowModal()));
                        }
                    }
                    else
                    {
                        CMM_CONDITION_RETURN popCondition = new CMM_CONDITION_RETURN() { FrameOperation = FrameOperation }; ;
                        if (popCondition != null)
                        {
                            UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                            object[] Parameters = new object[7];
                            Parameters[0] = _equipmentCode.ToString(); //설비코드
                            Parameters[1] = UcElectrodeProductLot.txtSelectEquipmentName.Text; // 설비코드명
                            Parameters[2] = dt.Rows[0]["PRJT_NAME"].ToString(); //PJT
                            Parameters[3] = dt.Rows[0]["PROD_VER_CODE"].ToString(); //Version
                            Parameters[4] = _processCode; //공정
                            Parameters[5] = "E"; //전극 화면에서 호출
                            Parameters[6] = dt.Rows[0]["WOID"].ToString(); //WOID
                            C1WindowExtension.SetParameters(popCondition, Parameters);
                            popCondition.Closed += new EventHandler(popCondition_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => popCondition.ShowModal()));
                        }
                    }



                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void popCondition_Closed(object sender, EventArgs e)
        {
            CMM_CONDITION_RETURN popup = sender as CMM_CONDITION_RETURN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 무지부 조회
                GetWorkHalfSlittingSide();

                //ButtonProductList_Click(null, null);

                //// 생산 Lot 재조회
                //SelectProductLot(string.Empty);
                //SetProductLotList();

                // 설비 재조회
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        private void popReturn_Closed(object sender, EventArgs e)
        {
            CMM_CONDITION_RETURN_ESWA popup = new CMM_CONDITION_RETURN_ESWA();
            if (popup != null)
            {
                // 무지부 조회
                GetWorkHalfSlittingSide();

                // 설비 재조회
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        #region =============================추가 버튼 Event
        private void ButtonExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            C1DropDownButton btn = sender as C1DropDownButton;
            if (btn != null) btn.IsDropDownOpen = false;
        }

        /// <summary>
        /// 수작업모드
        /// </summary>
        private void ButtonManualMode_Click(object sender, RoutedEventArgs e)
        {
            // 작업시작,시작취소, 장비완료, 장비완료취소에 관한 별도 권한 관리시 수정
            // 수작업모드 : 작업시작, 시작취소, 장비완료취소 수작업으로 가능하게 버튼 활성화
        }

        /// <summary>
        /// 설비특이사항
        /// </summary>
        private void ButtonEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            PopupEqptIssue();
        }

        /// <summary>
        /// 대LOT정리
        /// </summary>
        private void ButtonCleanLot_Click(object sender, RoutedEventArgs e)
        {
            PopupCleanLot();
        }

        /// <summary>
        /// LOT종료취소
        /// </summary>
        private void ButtonCancelFCut_Click(object sender, RoutedEventArgs e)
        {
            PopupCancelFCut();
        }

        /// <summary>
        /// 삭제Lot생성
        /// </summary>
        private void ButtonCancelDelete_Click(object sender, RoutedEventArgs e)
        {
            PopupCancelDelete();
        }

        /// <summary>
        /// Cut
        /// </summary>
        private void ButtonCut_Click(object sender, RoutedEventArgs e)
        {
            PopupCut();
        }

        /// <summary>
        /// 투입요청서
        /// </summary>
        private void ButtonInvoiceMaterial_Click(object sender, RoutedEventArgs e)
        {
            ELEC003_002 wndInvoiceMaterial = new ELEC003_002();
            wndInvoiceMaterial.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = Util.NVC(_processCode);
            parameters[1] = Util.NVC(_equipmentCode);

            FrameOperation.OpenMenu("SFU010020012", true, parameters);
        }

        /// <summary>
        /// 작업조건등록
        /// </summary>
        private void ButtonEqptCond_Click(object sender, RoutedEventArgs e)
        {
            PopupEqptCond();
        }

        /// <summary>
        /// 자주검사등록
        /// </summary>
        private void ButtonMixConfirm_Click(object sender, RoutedEventArgs e)
        {
            PopupMixConfirm();
        }

        /// <summary>
        /// R/P 샘플링 제품등록
        /// </summary>
        private void ButtonSamplingProd_Click(object sender, RoutedEventArgs e)
        {
            PopupSamplingProd();
        }

        /// <summary>
        /// R/P 대기 변경
        /// </summary>
        private void ButtonProcReturn_Click(object sender, RoutedEventArgs e)
        {
            PopupProcReturn();
        }

        /// <summary>
        /// 샘플링 제품등록
        /// </summary>
        private void ButtonSamplingProdT1_Click(object sender, RoutedEventArgs e)
        {
            PopupSamplingProdT1();
        }

        /// <summary>
        /// Slurry 정보
        /// </summary>
        private void ButtonMixerTankInfo_Click(object sender, RoutedEventArgs e)
        {
            PopupMixerTankInfo();
        }

        /// <summary>
        /// W/O 예약
        /// </summary>
        private void ButtonReservation_Click(object sender, RoutedEventArgs e)
        {
            PopupReservation();
        }

        /// <summary>
        /// FOIL 관리
        /// </summary>
        private void ButtonFoil_Click(object sender, RoutedEventArgs e)
        {
            PopupFoil();
        }

        /// <summary>
        /// Slurry 물성정보
        /// </summary>
        private void ButtonSlurryConf_Click(object sender, RoutedEventArgs e)
        {
            PopupSlurryConf();
        }

        /// <summary>
        /// 코터 임의 cut생성
        /// </summary>
        private void ButtonStartCoaterCut_Click(object sender, RoutedEventArgs e)
        {
            PopupStartCoaterCut();
        }

        /// <summary>
        /// 무지부 방향설정
        /// </summary>
        private void ButtonWorkHalfSlitSide_Click(object sender, RoutedEventArgs e)
        {
            PopupWorkHalfSlitSide();
        }

        /// <summary>
        /// 권취 방향 변경
        /// </summary>
        private void ButtonEmSectionRollDirctn_Click(object sender, RoutedEventArgs e)
        {
            PopupEmSectionRollDirctn();
        }

        /// <summary>
        /// 물류반송현황
        /// </summary>
        private void ButtonLogisStat_Click(object sender, RoutedEventArgs e)
        {
            PopupLogisStat();
        }

        /// <summary>
        /// Port별 Skid Type 설정
        /// </summary>
        private void ButtonSkidTypeSettingByPort_Click(object sender, RoutedEventArgs e)
        {
            PopupSkidTypeSettingByPort();
        }

        /// <summary>
        /// 목시관리필요Lot 등록  
        /// </summary>
        private void ButtonSlBatch_Click(object sender, RoutedEventArgs e)
        {
            PopupSlBatch();
        }

        /// <summary>
        /// Scrap Lot 재생성  
        /// </summary>
        private void ButtonScrapLot_Click(object sender, RoutedEventArgs e)
        {
            PopupScrapLot();
        }

        protected virtual void ButtonRollMapInputMaterial_Click(object sender, RoutedEventArgs e)
        {
            PopupRollMapInputMaterial();
        }

        /// <summary>
        /// 버퍼 수동 초기화
        /// </summary>
        private void ButtonbtnSlurryBufferManualInit_Click(object sender, RoutedEventArgs e)
        {
            PopupSlurryBufferManualInit();
        }
        //protected virtual void ButtonbtnUpdateLaneNo_Click(object sender, RoutedEventArgs e)
        //{
        //    PopupUpdateLaneNo();
        //}
        #endregion

        #region =============================Coating 
        /// <summary>
        /// 전수불량Lane등록
        /// </summary>
        private void ButtonSaveRegDefectLane_Click(object sender, RoutedEventArgs e)
        {
            PopupDfctLanePancake();
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        #region =============================공통
        /// <summary>
        /// 라인
        /// </summary>
        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
        }

        /// <summary>
        /// 설비
        /// </summary>
        private void SetEquipmentCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPT_RSLT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();
                dr["ELTR_TYPE_CODE"] = string.IsNullOrWhiteSpace(cboPolarity.SelectedValue.ToString()) ? null : cboPolarity.SelectedValue.ToString();
                if (_processCode == Process.COATING)
                {
                    dr["COATER_EQPT_TYPE_CODE"] = "DSC";
                    // ROLLMAP 수불인 경우 'M' - 자동실적 방지 [2023-01-11]
                    dr["EQPT_RSLT_CODE"] = "A";                   // (*)설비 확정 여부 [A : 자동, M : 수동]
                }
                //else if (_processCode == Process.ROLL_PRESSING)
                //{
                //    dr["EQPT_RSLT_CODE"] = "A";                   // (*)설비 확정 여부 [A : 자동, M : 수동]
                //}

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO_DRB", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetReWindingProcess()
        {
            try
            {
                _reWindingProcess = string.Empty;

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));
                inTable.Columns.Add("CMCDIUSE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "REWINDING_PROCID";
                newRow["CMCODE"] = _processCode;
                newRow["CMCDIUSE"] = "Y";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _reWindingProcess = "Y";
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetIdentInfo()
        {
            try
            {
                _ldrLotIdentBasCode = string.Empty;
                _unldrLotIdentBasCode = string.Empty;

                //// 재 와인딩 공정인 경우 제외
                //if (_reWindingProcess == "Y") return;

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = _equipmentSegmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _ldrLotIdentBasCode = dtResult.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _unldrLotIdentBasCode = dtResult.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 권한 그룹별 권한 체크
        /// </summary>
        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID)
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
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    if (sBtnGrpID == "EQPT_END_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("장비완료");
                    else if (sBtnGrpID == "CONFIRM_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("실적확정");
                    else
                        objectmessage = ObjectDic.Instance.GetObjectName("공정이동");

                    Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private string GetEqptChgBlock(string sProcId)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = sProcId;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROC_EQPT_CHG_BLOCK_YN", "RQSTDT", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0]["BLOCK_YN"].ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 진행중인 Lot 검색
        /// </summary>
        private string SelectProcLotID()
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

                if (dtResult != null && dtResult.Rows.Count > 0)
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

        /// <summary>
        /// LOT 재공 정보 조회
        /// </summary>
        private bool SelectLotWip()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return _dvProductLot["PROCID"].ToString() == dtResult.Rows[0]["PROCID"].ToString();
                }

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        /// <summary>
        /// 시생산설정/해제 가능 여부 체크
        /// </summary>
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
                dtRow["EQPTID"] = _equipmentCode;
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

        /// <summary>
        /// 2021.08.12 : 시생산샘플설정/해제 기능 추가
        /// param bool에서 string으로 변경
        /// </summary>
        /// <param name="sMode"></param>
        /// <returns></returns>
        private void SavePilotProdMode(string sMode)
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
                newRow["PILOT_PRDC_MODE"] = sMode;  //bMode ? "PILOT" : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_PILOT_PRODUCTION_MODE", "INDATA", null, inTable);

                Util.MessageInfo("SFU1166");    // 변경되었습니다.

                // 설비 리스트 재조회
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 시작취소 : 재와인딩
        /// </summary>
        private void CancelProcessReWinding()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROCID"] = _processCode; ;
                newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                newRow["WIPSEQ"] = _dvProductLot["WIPSEQ"].ToString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);
                ////////////////////////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_RW_DRB", "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    //Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    ButtonProductList_Click(null, null);

                    // 생산 Lot 재조회
                    SelectProductLot(string.Empty);
                    SetProductLotList();

                    // 설비 재조회
                    UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 시작취소 HALF_SLITTING
        /// </summary>
        private void CancelProcessHalfSlutting()
        {
            try
            {
                string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_HS";

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                DataRow[] dr = DataTableConverter.Convert(DgProductLot.ItemsSource).Select("LOTID_PR = '" + _dvProductLot["LOTID_PR"].ToString() + "' And  CUT_ID = '" + _dvProductLot["CUT_ID"].ToString() + "'");

                if (dr.Length != 2)
                {
                    Util.MessageInfo("SFU1695");     // 시작 취소 에러
                    return;
                }

                foreach (DataRow row in dr)
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["EQPTID"] = _equipmentCode;
                    newRow["LOTID"] = row["LOTID"].ToString();
                    newRow["INPUT_LOTID"] = row["LOTID_PR"].ToString();
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    inTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    //Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    ButtonProductList_Click(null, null);

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

        /// <summary>
        /// 시작취소
        /// </summary>
        private void CancelProcessSlitting()
        {
            try
            {
                DataTable inLotTable = new DataTable("RQSTDT");
                inLotTable.Columns.Add("LOTID_PR", typeof(string));
                inLotTable.Columns.Add("LOTID", typeof(string));
                inLotTable.Columns.Add("LANGID", typeof(string));
                inLotTable.Columns.Add("PROCID", typeof(string));
                inLotTable.Columns.Add("WIPSTAT", typeof(string));
                inLotTable.Columns.Add("CUT_ID", typeof(string));

                DataRow newRow = inLotTable.NewRow();
                newRow["LOTID_PR"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                newRow["LOTID"] = null;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["WIPSTAT"] = Util.NVC(_dvProductLot["WIPSTAT"]);
                newRow["CUT_ID"] = Util.NVC(_dvProductLot["CUT_ID"]);
                inLotTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RUNLOT_SL", "RQSTDT", "RSLTDT", inLotTable);
                if (dtResult == null) return;

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUT_CSTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                foreach (DataRow drLot in dtResult.Rows)
                {
                    DataRow newLotRow = inTable.NewRow();
                    newLotRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newLotRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newLotRow["EQPTID"] = _equipmentCode;
                    newLotRow["LOTID"] = Util.NVC(drLot["LOTID"]);

                    if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
                    {
                        if (dtResult.Columns["CSTID"] != null)
                        {
                            newLotRow["CSTID"] = Util.NVC(drLot["CSTID"]);
                        }
                    }

                    if (_unldrLotIdentBasCode == "CST_ID" || _unldrLotIdentBasCode == "RF_ID")
                    {
                        if (dtResult.Columns["OUT_CSTID"] != null)
                        {
                            newLotRow["OUT_CSTID"] = Util.NVC(drLot["OUT_CSTID"]);
                        }
                    }
                    newLotRow["USERID"] = LoginInfo.USERID;
                    newLotRow["INPUT_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);

                    inTable.Rows.Add(newLotRow);
                }


                #region Parent Lot
                DataTable InLotdataTable = inDataSet.Tables.Add("IN_PRLOT");
                InLotdataTable.Columns.Add("PR_LOTID", typeof(string));
                InLotdataTable.Columns.Add("CUT_ID", typeof(string));
                InLotdataTable.Columns.Add("CSTID", typeof(string));

                DataRow inLotDataRow = InLotdataTable.NewRow();
                inLotDataRow["PR_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                inLotDataRow["CUT_ID"] = Util.NVC(_dvProductLot["CUT_ID"]);
                if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
                {
                    inLotDataRow["CSTID"] = Util.NVC(_dvProductLot["CSTID"]);
                }
                InLotdataTable.Rows.Add(inLotDataRow);
                #endregion

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_SL", "INDATA,IN_PRLOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    ButtonProductList_Click(null, null);

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

        //20211215 2차 Slitting 공정진척DRB 화면 개발 START
        /// <summary>
        /// 시작취소
        /// </summary>
        private void CancelProcessTwoSlitting()
        {
            try
            {
                DataTable inLotTable = new DataTable("RQSTDT");
                inLotTable.Columns.Add("LOTID_PR", typeof(string));
                inLotTable.Columns.Add("LOTID", typeof(string));
                inLotTable.Columns.Add("LANGID", typeof(string));
                inLotTable.Columns.Add("PROCID", typeof(string));
                inLotTable.Columns.Add("WIPSTAT", typeof(string));
                inLotTable.Columns.Add("CUT_ID", typeof(string));

                DataRow newRow = inLotTable.NewRow();
                newRow["LOTID_PR"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                newRow["LOTID"] = null;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["WIPSTAT"] = Util.NVC(_dvProductLot["WIPSTAT"]);
                newRow["CUT_ID"] = Util.NVC(_dvProductLot["CUT_ID"]);
                inLotTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RUNLOT_SL", "RQSTDT", "RSLTDT", inLotTable);
                if (dtResult == null) return;

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUT_CSTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                foreach (DataRow drLot in dtResult.Rows)
                {
                    DataRow newLotRow = inTable.NewRow();
                    newLotRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newLotRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newLotRow["EQPTID"] = _equipmentCode;
                    newLotRow["LOTID"] = Util.NVC(drLot["LOTID"]);

                    if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
                    {
                        if (dtResult.Columns["CSTID"] != null)
                        {
                            newLotRow["CSTID"] = Util.NVC(drLot["CSTID"]);
                        }
                    }

                    if (_unldrLotIdentBasCode == "CST_ID" || _unldrLotIdentBasCode == "RF_ID")
                    {
                        if (dtResult.Columns["OUT_CSTID"] != null)
                        {
                            newLotRow["OUT_CSTID"] = Util.NVC(drLot["OUT_CSTID"]);
                        }
                    }
                    newLotRow["USERID"] = LoginInfo.USERID;
                    newLotRow["INPUT_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);

                    inTable.Rows.Add(newLotRow);
                }


                #region Parent Lot
                DataTable InLotdataTable = inDataSet.Tables.Add("IN_PRLOT");
                InLotdataTable.Columns.Add("PR_LOTID", typeof(string));
                InLotdataTable.Columns.Add("CUT_ID", typeof(string));
                InLotdataTable.Columns.Add("CSTID", typeof(string));

                DataRow inLotDataRow = InLotdataTable.NewRow();
                inLotDataRow["PR_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                inLotDataRow["CUT_ID"] = Util.NVC(_dvProductLot["CUT_ID"]);
                if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
                {
                    inLotDataRow["CSTID"] = Util.NVC(_dvProductLot["CSTID"]);
                }
                InLotdataTable.Rows.Add(inLotDataRow);
                #endregion

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_SL", "INDATA,IN_PRLOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    ButtonProductList_Click(null, null);

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
        //20211215 2차 Slitting 공정진척DRB 화면 개발 END
        /// <summary>
        /// 시작취소
        /// </summary>
        private void CancelProcess()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (_processCode == Process.MIXING)
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_MX";
                if (_processCode == Process.PRE_MIXING ||
                    _processCode == Process.BS ||
                    _processCode == Process.CMC ||
                    _processCode == Process.InsulationMixing ||
                    _processCode == Process.DAM_MIXING)
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_PM";
                else if (_processCode == Process.COATING)
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_CT";
                else if (_processCode == Process.INS_COATING)
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_IC";
                else if (_processCode == Process.ROLL_PRESSING)
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_RP";
                else if (_processCode == Process.REWINDER)
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_SI";
                else if (_processCode == Process.TAPING)
                    bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_TP";

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUT_CSTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                newRow["USERID"] = LoginInfo.USERID;

                //////////////////////////////////////////////////////////
                if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
                {
                    newRow["CSTID"] = Util.NVC(_dvProductLot["CSTID"]);
                }
                if (_unldrLotIdentBasCode == "CST_ID" || _unldrLotIdentBasCode == "RF_ID")
                {
                    newRow["OUT_CSTID"] = Util.NVC(_dvProductLot["OUT_CSTID"]);
                }
                newRow["INPUT_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                ////////////////////////////////////////////////////////////////////////////////////////////////
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    //Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    ButtonProductList_Click(null, null);

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

        /// <summary>
        /// 이동취소
        /// </summary>
        private void MoveCancelProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID_FR", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID_FR"] = _processCode;
                newRow["NOTE"] = "";
                inTable.Rows.Add(newRow);

                DataTable inLot = inDataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));

                newRow = inLot.NewRow();
                newRow["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                inLot.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOCATE_LOT_MOVECANCEL_RW_DRB", "INDATA,IN_LOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1766");     // 이동완료

                    ButtonProductList_Click(null, null);

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

        /// <summary>
        /// 장비완료취소
        /// </summary>
        private void EqptEndCancelProcess()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (_processCode == Process.MIXING ||
                    _processCode == Process.PRE_MIXING ||
                    _processCode == Process.BS ||
                    _processCode == Process.CMC ||
                    _processCode == Process.InsulationMixing ||
                    _processCode == Process.HALF_SLITTING ||
                    _processCode == Process.ROLL_PRESSING ||
                    _processCode == Process.INS_COATING ||
                    _processCode == Process.SLITTING ||
                    _processCode == Process.TWO_SLITTING || //20211215 2차 Slitting 공정진척DRB 화면 개발
                    _processCode == Process.REWINDER ||
                    _processCode == Process.TAPING ||
                    _processCode == Process.HEAT_TREATMENT ||
                    _processCode == Process.DAM_MIXING)
                    bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_ELTR";
                else if (_processCode == Process.COATING)
                    bizRuleName = "BR_PRD_REG_CANCEL_END_LOT_ELTR";

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable InLot = inDataSet.Tables.Add("INLOT");
                InLot.Columns.Add("LOTID", typeof(string));
                InLot.Columns.Add("CUT_ID", typeof(string));
                InLot.Columns.Add("WIPNOTE", typeof(string));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = InLot.NewRow();
                //if (_processCode == Process.HALF_SLITTING || _processCode == Process.SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
                if (_processCode == Process.HALF_SLITTING || _processCode == Process.SLITTING || _processCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
                    newRow["CUT_ID"] = _dvProductLot["CUT_ID"].ToString();
                else
                    newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                InLot.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    ButtonProductList_Click(null, null);

                    // 생산 Lot 재조회
                    SelectProductLot(_dvProductLot["LOTID"].ToString());
                    SetProductLotList(_dvProductLot["LOTID"].ToString());

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 장비완료 : 재와인딩
        /// </summary>
        private void EqptEndProcessRewinder()
        {
            try
            {
                if (!ValidationEqptEndRewinder()) return;

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("IN_EQP");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("INPUT_LOTID", typeof(string));
                InInput.Columns.Add("EQPT_END_QTY", typeof(decimal));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(UcResult_ReWinding.dgProductResult.ItemsSource);

                foreach (DataRow row in dt.Rows)
                {
                    newRow = InInput.NewRow();
                    newRow["INPUT_LOTID"] = row["INPUT_LOTID"];
                    newRow["EQPT_END_QTY"] = row["EQPT_END_QTY"];
                    InInput.Rows.Add(newRow);
                }

                // 무지부/권취 두 방향 모두 사용하는 AREA에선 재와인딩 완공 시 무지부/권취 방향 저장할 수 있도록 함
                if (UcResult_ReWinding.bSideRollDirctnUse)
                {
                    inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                    inTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                    inTable.Rows[0]["HALF_SLIT_SIDE"] = UcResult_ReWinding.txtSSWD.Tag.ToString().Substring(0, 1);
                    inTable.Rows[0]["EM_SECTION_ROLL_DIRCTN"] = UcResult_ReWinding.txtSSWD.Tag.ToString().Substring(1, 1);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_RW_DRB", "IN_EQP,IN_INPUT", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    ButtonProductList_Click(null, null);

                    // 생산 Lot 재조회
                    SelectProductLot(_dvProductLot["LOTID"].ToString());
                    SetProductLotList(_dvProductLot["LOTID"].ToString());

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 장비완료취소 : 재와인딩
        /// </summary>
        private void EqptEndCancelProcessRewinder()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(Decimal));
                inTable.Columns.Add("USERID", typeof(string));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROCID"] = _processCode;
                newRow["LOTID"] = _dvProductLot["LOTID"];
                newRow["WIPSEQ"] = _dvProductLot["WIPSEQ"];
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_EQPT_END_LOT_RW_DRB", "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");     // 정상 처리 되었습니다.

                    ButtonProductList_Click(null, null);

                    // 생산 Lot 재조회
                    SelectProductLot(_dvProductLot["LOTID"].ToString());
                    SetProductLotList(_dvProductLot["LOTID"].ToString());

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 실적 확정전 작업자 재조회
        /// </summary>
        private void SelectWorkUser()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WRK_USER_DRB", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataTable dt = DataTableConverter.Convert(DgEquipment.ItemsSource);

                    foreach (C1.WPF.DataGrid.DataGridRow row in DgEquipment.Rows)
                    {
                        if (row.Type == DataGridRowType.Item)
                        {
                            if (DataTableConverter.GetValue(row.DataItem, "EQPTID").ToString() == dtResult.Rows[0]["EQPTID"].ToString() &&
                                DataTableConverter.GetValue(row.DataItem, "SEQ").ToString() == dtResult.Rows[0]["SEQ"].ToString())
                            {
                                DataTableConverter.SetValue(row.DataItem, "SHFT_ID", dtResult.Rows[0]["SHFT_ID"]);
                                DataTableConverter.SetValue(row.DataItem, "WRK_USERID", dtResult.Rows[0]["WRK_USERID"]);
                                DataTableConverter.SetValue(row.DataItem, "WRK_STRT_DTTM", dtResult.Rows[0]["WRK_STRT_DTTM"]);
                                DataTableConverter.SetValue(row.DataItem, "WRK_END_DTTM", dtResult.Rows[0]["WRK_END_DTTM"]);
                                DataTableConverter.SetValue(row.DataItem, "INPUT_YN", dtResult.Rows[0]["INPUT_YN"]);
                                DataTableConverter.SetValue(row.DataItem, "VAL001", dtResult.Rows[0]["VAL001"]);
                                DataTableConverter.SetValue(row.DataItem, "VAL002", dtResult.Rows[0]["VAL002"]);
                                DataTableConverter.SetValue(row.DataItem, "VAL006", dtResult.Rows[0]["VAL006"]);

                                break;
                            }
                        }
                    }

                    DgEquipment.Refresh();
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// W/O 자동 변경  
        /// </summary>
        private void AutoChangeWorkOrder()
        {
            try
            {
                DataRow[] dr = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

                DataSet inDataSet = new DataSet();
                DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));
                inEQP.Columns.Add("WO_DETL_ID", typeof(string));

                /////////////////////////////////////////////////////////////////////// INDATA SET
                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                // 2021.10.29. 한종현. Parameter VAL004 -> VAL009로 변경
                //newRow["WO_DETL_ID"] = dr[0]["VAL004"].ToString();
                newRow["WO_DETL_ID"] = dr[0]["VAL009"].ToString();
                inEQP.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_AUTOCHANGE_WO_DETL_ID_ELEC", "IN_EQP", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 설비 재조회
                        //SetEquipment("C");
                        UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 바코드 발행에서 사용
        /// </summary>
        private string GetLotProdVerCode()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _dvProductLot["LOTID"];
                inTable.Rows.Add(newRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "INDATA", "RSLTDT", inTable);

                if (result != null && result.Rows.Count > 0)
                    return Util.NVC(result.Rows[0]["PROD_VER_CODE"]);
            }
            catch (Exception ex) { }

            return "";
        }

        /// <summary>
        /// 바코드 발행에서 사용
        /// </summary>
        private DataTable GetPrintCount()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _dvProductLot["LOTID"];
                newRow["PROCID"] = _processCode;
                inTable.Rows.Add(newRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_COUNT", "INDATA", "RSLTDT", inTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        /// <summary>
        /// 실제 확정 작업자 저장
        /// </summary>
        private void SaveRealWorker(string sWrokerName)
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WORKER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                C1DataGrid Remark = UcResult_Slitting.dgRemark;
                DataTable dt = DataTableConverter.Convert(Remark.ItemsSource);

                //CSR E20240403-001187 [ESWA PI] Operator name multiple showing 작업자 정보 저장 처리
                if (dt.Rows.Count > 1 && _processCode.Equals(Process.SLITTING))
                {
                    for (int i = 1; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(dt.Rows[i]["LOTID"]) != null)
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);
                            newRow["WORKER_NAME"] = sWrokerName;
                            newRow["USERID"] = LoginInfo.USERID;

                            inTable.Rows.Add(newRow);
                        }
                    }
                }
                else
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["LOTID"] = _dvProductLot["LOTID"];
                    newRow["WORKER_NAME"] = sWrokerName;
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORYATTR_REAL_WORKER", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private Dictionary<int, string> CheckFinalCutLot(C1DataGrid dg, decimal dExceedLengthQty, string sUnit)
        {
            // 자동 FINAL CUT 여부 [2017-01-13]
            // 0 : Confirm 여부, 1 : Final Cut 여부, 2  : Loss 처리 여부, 3 : Hold 표시 여부, 4 : Confirm Message, 5 : 잔량[팝업], 6 : 잔량[길이부족차감] 
            DataTable inTable = new DataTable();
            inTable.Columns.Add("PR_LOTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));

            DataRow indata = inTable.NewRow();
            indata["PR_LOTID"] = _dvProductLot["LOTID_PR"];
            indata["LOTID"] = _dvProductLot["LOTID"];
            indata["PROCID"] = _processCode;

            inTable.Rows.Add(indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUTLOT_ELEC", "INDATA", "RSLTDT", inTable);

            if (dt.Rows.Count <= 1)
            {
                Util.MessageValidation("SFU1707");  //실적 확정할 대상이 없습니다.
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            string sLotID = string.Empty;
            decimal iOutQty = 0;         // 생산 수량
            decimal iTotQty = 0;         // 투입 LOT 수량
            decimal iResQty = 0;         // 투입 LOT 처리 이후 최종 수량
            bool isFinalOper = false;   // 마지막 CUT 여부

            // 투입 Lot 수량 저장
            iOutQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[dg.TopRows.Count].DataItem, "INPUTQTY"));   // 생산수량
            iTotQty = Util.NVC_Decimal(dt.Rows[0]["WIPQTY"]);

            // 실적확정해야 할 1번째 LOT이 처음 확정할 LOT인지 체크
            DataRow[] rows = dt.Select("CUT_SEQNO = 1");
            if (rows.Length == ((dg.Rows.Count - dg.BottomRows.Count) - dg.TopRows.Count))
            {
                bool isDupplicate = false;
                for (int i = 0 + dg.TopRows.Count; i < (dg.Rows.Count - dg.BottomRows.Count); i++)
                {

                    for (int j = 0; j < rows.Length; j++)
                    {
                        if (string.Equals(rows[j]["LOTID"], DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID")))
                        {
                            isDupplicate = true;
                            break;
                        }
                    }
                }
                if (isDupplicate == false)
                {
                    Util.MessageValidation("SFU2898", new object[] { rows[0]["LOTID"] });   //LOT[{%1}]을 먼저 실적 확정 하세요.
                    return new Dictionary<int, string> { { 0, bool.FalseString } };
                }
            }
            else
            {
                Util.MessageValidation("SFU2898", new object[] { rows[0]["LOTID"] });   //LOT[{%1}]을 먼저 실적 확정 하세요.
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            // 마지막 실적 확정인지 체크
            if (dt.Select("CUT_SEQNO > 1").Length == 0)
                isFinalOper = true;

            ///////////////////////////////////////////////////////////////
            if (_processCode == Process.HALF_SLITTING)
                iResQty = Util.NVC_Decimal(UcResult_HalfSlitting.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            else if (_processCode == Process.ROLL_PRESSING)
                iResQty = Util.NVC_Decimal(UcResult_RollPressing.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            else if (_processCode == Process.SLITTING)
                iResQty = Util.NVC_Decimal(UcResult_Slitting.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (_processCode == Process.TWO_SLITTING)
                iResQty = Util.NVC_Decimal(UcResult_TwoSlitting.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else if (_processCode == Process.INS_COATING)
                iResQty = Util.NVC_Decimal(UcResult_InsCoating.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            else if (_processCode == Process.REWINDER)
                iResQty = Util.NVC_Decimal(UcResult_ReWinder.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            else if (_processCode == Process.TAPING)
                iResQty = Util.NVC_Decimal(UcResult_Taping.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            else if (_processCode == Process.HEAT_TREATMENT)
                iResQty = Util.NVC_Decimal(UcResult_HeatTreatment.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));
            else
                iResQty = Util.NVC_Decimal(UcResult_RollPressing.GetUnitFormatted(iTotQty - (iOutQty - dExceedLengthQty)));

            // 길이초과로 등록된 수량 감안해서 -수량인 경우 체크
            if (iResQty < 0)
            {
                Util.MessageValidation("SFU1614");
                return new Dictionary<int, string> { { 0, bool.FalseString } };
            }

            if (isFinalOper)
            {
                if (iResQty > 0)
                {
                    return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.FalseString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                        { 4, MessageDic.Instance.GetMessage("SFU1964", new object[] { iResQty + sUnit }) }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };     // 투입LOT 잔량 {0}가 대기됩니다.\n실적확정 하시겠습니까?
                }
                else
                {
                    return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.TrueString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1965") }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };     // 투입LOT 잔량이 없습니다.\r\n실적확정하시겠습니까?
                }
            }
            else
            {
                if (iResQty > 0)
                {
                    return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.FalseString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1963", new object[] { iResQty + sUnit }) }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };     // 투입LOT 잔량 {0}이 남게 됩니다.\r\n실적확정하시겠습니까?
                }
                else
                {
                    if (_isRollMapEquipment == true)
                    {
                        // ROLLMAP 실적 처리의 경우 다음LOT이 존재하고 투입LOT 잔량이 0인 경우도 확정 가능하도록 수정
                        return new Dictionary<int, string> { { 0, bool.TrueString }, { 1, bool.TrueString }, { 2, bool.FalseString }, { 3, bool.FalseString },
                            { 4, MessageDic.Instance.GetMessage("SFU1965") }, { 5, Util.NVC(iResQty) }, { 6, Util.NVC(iResQty) } };     //투입LOT 잔량이 없습니다.\r\n실적확정하시겠습니까?
                    }
                    else
                    {
                        Util.MessageValidation("SFU1483");     // 다음 완성LOT이 존재하여 실적확정 불가합니다.
                        return new Dictionary<int, string> { { 0, bool.FalseString } };
                    }
                }
            }
        }

        #region -----------------------------RollMap Biz

        private DataTable GetEquipmentCode(string lotId, string wipSeq)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = inTable.NewRow();
            //dr["EQPTID"] = equipmentCode;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIPHISTORY", "RQSTDT", "RSLTDT", inTable);
        }

        private DataTable GetRollMapInputLotCode(string lotId)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = 1;
            dr["EQPT_MEASR_PSTN_ID"] = "UW";
            inTable.Rows.Add(dr);

            if (_isRollMapDivReportEqpt)    //E20250115-001100
            {
                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_BAS_CLCT_INFO", "RQSTDT", "RSLTDT", inTable);
            }

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_COLLECT_INFO", "RQSTDT", "RSLTDT", inTable);
        }

        private bool IsEquipmentAttr(string sEqptID)
        {
            try
            {
                DataRow[] dr = Util.getEquipmentAttr(sEqptID).Select();
                if (dr?.Length > 0)
                {
                    if (string.Equals(Util.NVC(dr[0]["ROLLMAP_EQPT_FLAG"]), "Y"))
                        return true;
                }
            }
            catch (Exception ex) { }

            return false;
        }

        private bool IsRollMapResultApply()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _processCode;
                dr["EQSGID"] = _equipmentSegmentCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null & dtResult.Rows.Count > 0)
                    if (string.Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void SetRollMapLotAttribute(string sLotID)
        {
            try
            {
                bool isRollMapModeChange = false;

                if (_isOriginRollMapEquipment == true)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("LOTID", typeof(string));

                    DataRow row = dt.NewRow();
                    row["LOTID"] = sLotID;
                    dt.Rows.Add(row);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);

                    if (result != null && result.Rows.Count > 0)
                    {
                        // 실적 연계 여부에 따라 처리 변경 [2021-10-18]
                        if (_isRollMapResultLink == true)
                        {
                            if (string.Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                            {
                                isRollMapModeChange = _isRollMapEquipment == false ? true : false;
                                _isRollMapEquipment = true;
                            }
                            else
                            {
                                isRollMapModeChange = _isRollMapEquipment == true ? true : false;
                                _isRollMapEquipment = false;
                            }

                            if (isRollMapModeChange == true)
                            {
                                VisibleRollMapMode();
                            }
                        }
                        else
                        {
                            _isRollMapEquipment = false;
                            VisibleRollMapMode();

                            if (string.Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                            {
                                UcElectrodeCommand.btnRollMap.Visibility = Visibility.Visible;
                                if (string.Equals(_processCode, Process.COATING))
                                {
                                    UcElectrodeCommand.btnRollMapInputMaterial.Visibility = Visibility.Visible; // 2021.08.10 Visible 처리
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private bool IsRollMapEquipment()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_LOTATTR";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow row = inTable.NewRow();
                row["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                inTable.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(result))
                {
                    if (Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SelectRollMapLot()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_ROLLMAP_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                dr["WIPSEQ"] = Util.NVC(_dvProductLot["WIPSEQ"]);
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["ROLLMAP_LOT_YN"].Equals("Y"))
                    {
                        _isRollMapLot = true;
                    }
                    else
                    {
                        _isRollMapLot = false;
                    }

                    if (dt.Rows[0]["ROLLMAP_SBL_YN"].Equals("Y"))
                    {
                        _isRollMapSBL = true;
                    }
                    else
                    {
                        _isRollMapSBL = false;
                    }
                }
                else
                {
                    _isRollMapLot = false;
                    _isRollMapSBL = false;
                }
            }
            catch (Exception)
            {
                _isRollMapLot = false;
                _isRollMapSBL = false;
            }
        }

        private bool SelectCheckDivReportEqpt()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_RM_COM_DIV_REPORT_EQPT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["DIV_EQPT_YN"].Equals("Y"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private DataTable GetRollMapHold(string processCode)
        {
            string bizRuleName = string.Equals(processCode, Process.ROLL_PRESSING) ? "DA_PRD_SEL_ROLLMAP_RP_HOLD" : "DA_PRD_SEL_ROLLMAP_CT_HOLD";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            decimal dcmWIPSEQ;
            decimal.TryParse(Util.NVC(_dvProductLot["WIPSEQ"]), out dcmWIPSEQ);

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
            dr["WIPSEQ"] = dcmWIPSEQ;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }
        #endregion

        #region [적재율 관련 메소드]
        private void SelectRackRate()
        {
            if (string.Equals(_processCode, Process.ROLL_PRESSING))
            {
                //if (string.IsNullOrEmpty(UcElectrodeProductLot.txtSelectEquipment.Text)) return;

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
                dr["EQGRID"] = "PCW";   //팬케잌 창고
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        //롤프레스 양극 : ROL-NND (+) STK #2
                        const string cathodeEquipmentCode = "U1ESTO14202";

                        var querycathode = (from t in bizResult.AsEnumerable()
                                            where t.Field<string>("EQPTID") == cathodeEquipmentCode
                                            select new
                                            {
                                                EquipmentCode = t.Field<string>("EQPTID"),
                                                RackRate = t.Field<decimal>("RACK_RATE"),
                                                EquipmentName = t.Field<string>("EQPTNAME")
                                            }).FirstOrDefault();

                        if (querycathode != null)
                        {
                            _rackState = RackRateDifferenceValue(querycathode.EquipmentCode, querycathode.RackRate);
                            string rackRateText = RackRateDifferenceValueText(querycathode.EquipmentCode, querycathode.RackRate);
                            ShowRackRateMode(_rackState, querycathode.EquipmentName, rackRateText + "%", "C");
                        }

                        //롤프레스 음극 : ROL-NND (-) STK #2
                        const string anodeEquipmentCode = "U1ESTO14102";

                        var queryAnode = (from t in bizResult.AsEnumerable()
                                          where t.Field<string>("EQPTID") == anodeEquipmentCode
                                          select new
                                          {
                                              EquipmentCode = t.Field<string>("EQPTID"),
                                              RackRate = t.Field<decimal>("RACK_RATE"),
                                              EquipmentName = t.Field<string>("EQPTNAME"),
                                          }).FirstOrDefault();

                        if (queryAnode != null)
                        {
                            _rackState = RackRateDifferenceValue(queryAnode.EquipmentCode, queryAnode.RackRate);
                            string rackRateText = RackRateDifferenceValueText(queryAnode.EquipmentCode, queryAnode.RackRate);
                            ShowRackRateMode(_rackState, queryAnode.EquipmentName, rackRateText + "%", "A");
                        }
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
            if (LoginInfo.CFG_SHOP_ID.Equals("G672") || LoginInfo.CFG_SHOP_ID.Equals("G674"))   //GM2추가 2023-08-24  장영철
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 4; i > 2; i--)
                    {
                        if (UcElectrodeProductLot.ProductLotContents.RowDefinitions[i].Height.Value <= 0) continue;

                        GridLengthAnimation gla = new GridLengthAnimation
                        {
                            From = new GridLength(0.1, GridUnitType.Star),
                            To = new GridLength(0, GridUnitType.Star),
                            AccelerationRatio = 0.8,
                            DecelerationRatio = 0.2,
                            Duration = new TimeSpan(0, 0, 0, 0, 500)
                        };
                        gla.Completed += HideTestAnimationCompleted;
                        UcElectrodeProductLot.ProductLotContents.RowDefinitions[i].BeginAnimation(RowDefinition.HeightProperty, gla);
                    }
                }));

                _isRackRateMode = false;
            }
        }

        private void HideRackRateMode()
        {
            if (LoginInfo.CFG_SHOP_ID.Equals("G672") || LoginInfo.CFG_SHOP_ID.Equals("G674"))   // GM2 추가   2023-08-24  장영철
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 4; i > 2; i--)
                    {
                        if (UcElectrodeProductLot.ProductLotContents.RowDefinitions[i].Height.Value <= 0) continue;

                        GridLengthAnimation gla = new GridLengthAnimation
                        {
                            From = new GridLength(0.1, GridUnitType.Star),
                            To = new GridLength(0, GridUnitType.Star),
                            AccelerationRatio = 0.8,
                            DecelerationRatio = 0.2,
                            Duration = new TimeSpan(0, 0, 0, 0, 500)
                        };
                        gla.Completed += HideTestAnimationCompleted;
                        UcElectrodeProductLot.ProductLotContents.RowDefinitions[i].BeginAnimation(RowDefinition.HeightProperty, gla);

                        if (i == 3)
                        {
                            UcElectrodeProductLot.recCathodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                            UcElectrodeProductLot.txtCathodeRackRateMode.Text = string.Empty;
                        }
                        else
                        {
                            UcElectrodeProductLot.recAnodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                            UcElectrodeProductLot.txtAnodeRackRateMode.Text = string.Empty;
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
            if (i == 4)
            {
                UcElectrodeProductLot.recCathodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                UcElectrodeProductLot.txtCathodeRackRateMode.Text = string.Empty;
            }
            else
            {
                UcElectrodeProductLot.recAnodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                UcElectrodeProductLot.txtAnodeRackRateMode.Text = string.Empty;
            }
        }

        private void ShowRackRateMode(SetRackState rackState, string equipmentName, string rackRate, string polarityCode)
        {

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                int gridrowIndex;
                if (polarityCode.Equals("C"))
                    gridrowIndex = 3;
                else
                    gridrowIndex = 4;

                if (UcElectrodeProductLot.ProductLotContents.RowDefinitions[gridrowIndex].Height.Value > 0)
                {
                    //강제로 그리드 Row Height 설정
                    GridLengthAnimation lengthAnimation = new GridLengthAnimation
                    {
                        From = new GridLength(0.1, GridUnitType.Star),
                        To = new GridLength(0, GridUnitType.Star),
                        AccelerationRatio = 0.8,
                        DecelerationRatio = 0.2,
                        Duration = new TimeSpan(0, 0, 0, 0, 500)
                    };
                    UcElectrodeProductLot.ProductLotContents.RowDefinitions[gridrowIndex].BeginAnimation(RowDefinition.HeightProperty, lengthAnimation);
                }

                //if (UcElectrodeProductLot.ProductLotContents.RowDefinitions[gridrowIndex].Height.Value > 0 || rackState == SetRackState.Normal) return;
                if (rackState == SetRackState.Normal) return;

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
                    UcElectrodeProductLot.txtCathodeRackRateMode.Text = GetMessage(messageCode, parameters);
                }
                else
                {
                    UcElectrodeProductLot.txtAnodeRackRateMode.Text = GetMessage(messageCode, parameters);
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

                UcElectrodeProductLot.ProductLotContents.RowDefinitions[gridrowIndex].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            _isRackRateMode = true;
        }

        private void ShowCathodeAnimationCompleted(SetRackState rackState)
        {
            string name;
            if (rackState == SetRackState.Danger)
            {
                UcElectrodeProductLot.recCathodeRackRateMode.Fill = redBrush;
                name = "redBrush";
            }
            else if (rackState == SetRackState.Warning)
            {
                UcElectrodeProductLot.recCathodeRackRateMode.Fill = yellowBrush;
                name = "yellowBrush";
            }
            else
            {
                UcElectrodeProductLot.recCathodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
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
                UcElectrodeProductLot.recAnodeRackRateMode.Fill = redBrush;
                name = "redBrush";
            }
            else if (rackState == SetRackState.Warning)
            {
                UcElectrodeProductLot.recAnodeRackRateMode.Fill = yellowBrush;
                name = "yellowBrush";
            }
            else
            {
                UcElectrodeProductLot.recAnodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
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

        #endregion

        #region =============================Mixing

        private void CheckLabelPassHold(Action callback)
        {
            try
            {
                //라벨링 패스 기능은 기존에도 활성화 되어 있었음
                _labelPassHoldFlag = "Y";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("CMCDTYPE");
                inTable.Columns.Add("CMCODE");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "LABEL_PASS_HOLD_CHECK";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

                if (rslt != null && rslt.Rows.Count > 0)
                {
                    if (!ValidLabelPass())
                    {
                        // Labeling Pass를 진행한 이력이 있습니다. 홀드하시겠습니까?
                        Util.MessageConfirm("SFU8218", result =>
                        {
                            //홀드
                            //전극의 모든 실적확정 로직에 플래그를 넣어서 Y면 자주검사스펙 홀드를 통과하도록하고 N이면 넘긴다
                            //MMD 동별 자주검사에서 자동홀드여부도 Y로 바꿔야 한다.
                            //또한 홀드를 걸 항목에 대해서만 LSL USL을 등록한다.
                            if (result == MessageBoxResult.OK)
                            {
                                _labelPassHoldFlag = "Y";
                            }
                            else
                            {
                                _labelPassHoldFlag = "N";
                            }
                            callback();
                        });
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
            catch (Exception ex)
            {
            }
        }

        private bool ValidLabelPass()
        {
            bool bRet = true;
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LOTID");
            inTable.Columns.Add("EQPTID");

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = Util.NVC(_dvProductLot["CUT_ID"]);
            newRow["EQPTID"] = _equipmentCode;
            inTable.Rows.Add(newRow);

            DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DATA_ECLCT", "INDATA", "OUTDATA", inTable);

            if (rslt != null && rslt.Rows.Count > 0)
            {
                bRet = false;
            }
            else
            {
                bRet = true;
            }
            return bRet;
        }

        private DataTable GetConfirmAuthVaildation()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["EQSGID"] = _equipmentSegmentCode;
            newRow["PROCID"] = _processCode;
            newRow["EQPTID"] = _equipmentCode;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CNFM_AUTH", "INDATA", "RSLTDT", inTable);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                // 인증 여부
                bool isAuthConfirm = true;

                // Input용 데이터 테이블 ( INPUTVALUE : 비교할 대상 값, CHK_VALUE1 : SPEC1, CHK_VALUE2 : SPEC2 )
                DataTable inputTable = new DataTable();
                inputTable.Columns.Add("CHK_VALUE1", typeof(decimal));
                inputTable.Columns.Add("CHK_VALUE2", typeof(decimal));
                inputTable.Columns.Add("INPUTVALUE", typeof(decimal));

                foreach (DataRow row in dtResult.Rows)
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
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(UcResult_Mixing.txtGoodQty.Value);
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);

                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "CNFM_PROD_QTY_LIMIT"))
                    {
                        inputTable.Rows[0]["INPUTVALUE"] = Util.NVC_Decimal(UcResult_Mixing.txtProductionQty.Value);
                        isAuthConfirm = CheckLimitValue(Util.NVC(row["CHK_TYPE_CODE"]), inputTable);
                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "QA_SPEC_LIMIT"))
                    {
                        // USL,LSL 제한 체크
                        isAuthConfirm = ValidQualitySpec("Auth");
                    }
                    else if (string.Equals(row["RSLT_CNFM_TYPE_CODE"], "QA_AVG_LIMIT"))
                    {
                        // 품질평균값 제한 체크

                    }

                    // 인증이 필요한 경우 전체 정보 전달
                    if (isAuthConfirm == false)
                    {
                        DataTable outTable = dtResult.Clone();
                        // MES 2.0 ItemArray 위치 오류 Patch
                        //outTable.Rows.Add(row.ItemArray);
                        outTable.AddDataRow(row);
                        return outTable;
                    }
                }
            }
            return new DataTable();
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dtResult.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        private DataTable GetMixerPreData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PRODID"] = Util.NVC(_dvProductLot["PRODID"]);
                inTable.Rows.Add(newRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MIXER_BEFORE_QTY", "INDATA", "RSLTDT", inTable);

                return result;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private Dictionary<int, string> getSamplingLabelInfo(string sLotID)
        {
            //if ((string.Equals(_processCode, Process.ROLL_PRESSING) || (string.Equals(_processCode, Process.SLITTING))) && string.Equals(getQAInspectFlag(sLotID), "Y")) //20211215 2차 Slitting 공정진척DRB 화면 개발
            if ((string.Equals(_processCode, Process.ROLL_PRESSING) || (string.Equals(_processCode, Process.SLITTING) || string.Equals(_processCode, Process.TWO_SLITTING))) && string.Equals(getQAInspectFlag(sLotID), "Y")) //20211215 2차 Slitting 공정진척DRB 화면 개발
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PROCID"] = _processCode;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SAMPLE_CHK_LOT_T1", "INDATA", "OUT_DATA", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                    return new Dictionary<int, string> { { Util.NVC_Int(dtMain.Rows[0]["OUT_PRINTCNT"]), Util.NVC(dtMain.Rows[0]["OUT_COMPANY"]) } };
            }

            return new Dictionary<int, string> { { 1, string.Empty } };
        }

        private string getQAInspectFlag(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count == 1)
                    return Util.NVC(string.Equals(_processCode, Process.ROLL_PRESSING) ? result.Rows[0]["QA_INSP_TRGT_FLAG"] : result.Rows[0]["SLIT_QA_INSP_TRGT_FLAG"]);
            }
            catch (Exception ex) { }

            return "";
        }

        private void ConfirmProcessMixing()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴 ?????????????????????????????
                string topInfo = SetConfirmTopInfoMixing();

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                // 이전 확정 생산LOT[%1]의 수량 체크
                if (string.IsNullOrWhiteSpace(addMessage))
                {
                    addMessage = SetConfirmQtyMessage();
                }
                else
                {
                    addMessage += SetConfirmQtyMessage();
                }

                // REMARK 필요 정보 취합 [2017-05-24]
                Dictionary<string, string> remarkInfo = GetRemarkConvert(UcResult_Mixing.dgRemark);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // 실적 확정 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1706"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // DEFECT 자동 저장
                        UcResult_Mixing.SaveDefect(UcResult_Mixing.dgWipReason, true);

                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));
                        if (_processCode == Process.PRE_MIXING)
                            inTable.Columns.Add("MILL_COUNT", typeof(string));
                        else if (_processCode == Process.MIXING)
                            inTable.Columns.Add("LAST_FLAG", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("INPUTQTY", typeof(decimal));
                        inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLot.Columns.Add("RESNQTY", typeof(decimal));
                        ///////////////////////////////////////////////////////////////////////////////

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["PROD_VER_CODE"] = UcResult_Mixing.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(UcResult_Mixing.dgRemark.Rows[1].DataItem, "LOTID"))];
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["WRK_USERID"] = workUserID;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_Mixing.txtWorkDate.Tag);
                        if (_processCode == Process.PRE_MIXING)
                            newRow["MILL_COUNT"] = Util.NVC_Int(UcResult_Mixing.txtBeadMillCount.Value);
                        else if (_processCode == Process.MIXING)
                            newRow["LAST_FLAG"] = _isLastBatch == true ? "Y" : "N";

                        inTable.Rows.Add(newRow);

                        newRow = inLot.NewRow();
                        newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                        newRow["INPUTQTY"] = Util.NVC_Decimal(UcResult_Mixing.txtProductionQty.Value);
                        newRow["OUTPUTQTY"] = Util.NVC_Decimal(UcResult_Mixing.txtGoodQty.Value);
                        newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_Mixing.dgProductResult.Rows[0].DataItem, "DEFECT_SUM"));
                        inLot.Rows.Add(newRow);

                        ShowLoadingIndicator();

                        string bizRuleName = string.Empty;
                        if (_processCode == Process.PRE_MIXING)
                            bizRuleName = "BR_PRD_REG_END_LOT_PM";
                        else if (_processCode == Process.BS)
                            bizRuleName = "BR_PRD_REG_END_LOT_BS";
                        else if (_processCode == Process.CMC)
                            bizRuleName = "BR_PRD_REG_END_LOT_CMC";
                        else if (_processCode == Process.InsulationMixing)
                            bizRuleName = "BR_PRD_REG_END_LOT_INSULT_MX";
                        else if (_processCode == Process.DAM_MIXING)
                            bizRuleName = "BR_PRD_REG_END_LOT_DAM_MX";
                        else
                            bizRuleName = "BR_PRD_REG_END_LOT_MX";

                        new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //// 정상 처리 되었습니다
                                //Util.MessageInfo("SFU1889");

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // W/O 자동 변경
                                if (_processCode == Process.MIXING || _processCode == Process.PRE_MIXING)
                                    AutoChangeWorkOrder();

                                // 라벨자동발행
                                LabelAuto();

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region =============================Coating
        /// <summary>
        /// Coating 실적 확정
        /// </summary>
        private void ConfirmProcessCoating()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_CoatingAuto.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // 다음 공정으로 이송 하시겠습니까?
                Util.MessageConfirm("SFU4257", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        ////////////////////////////////////////////////////// DEFECT 자동 저장
                        UcResult_CoatingAuto.SaveDefect(UcResult_CoatingAuto.dgWipReasonTop, true);
                        UcResult_CoatingAuto.SaveDefect(UcResult_CoatingAuto.dgWipReasonBack, true);

                        // RollMap Defect 좌표 반영
                        if (_isRollMapEquipment)
                        {
                            UcResult_CoatingAuto.SaveDefectForRollMap(true);
                        }


                        ////////////////////////////////////////////////////// 실적 저장
                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("PROCID", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIP_NOTE", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("REPROC_YN", typeof(string));           // 추가압연여부
                        inTable.Columns.Add("FINAL_CUT_FLAG", typeof(string));      // 코터 공정 FINAL CUT여부

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("INPUTQTY", typeof(decimal));
                        inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLot.Columns.Add("RESNQTY", typeof(decimal));
                        inLot.Columns.Add("TOPLOSSQTY", typeof(decimal));
                        inLot.Columns.Add("HOLD_YN", typeof(string));
                        ///////////////////////////////////////////////////////////////////////////////
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["PROCID"] = _processCode;
                        newRow["SHIFT"] = shift;
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["WIP_NOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgrmk.Rows[1].DataItem, "LOTID"))];
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["FINAL_CUT_FLAG"] = UcResult_CoatingAuto.chkFinalCut.IsChecked == true ? "Y" : "N";

                        inTable.Rows.Add(newRow);

                        C1DataGrid dg = UcResult_CoatingAuto.dgProductResult;

                        for (int i = 0; i < dg.GetRowCount(); i++)
                        {
                            if (!string.Equals(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                            {
                                newRow = inLot.NewRow();

                                newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                                newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUT_BACK_QTY"));
                                newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "GOODQTY"));
                                newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "DTL_BACK_DEFECT_SUM"));
                                newRow["TOPLOSSQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUT_TOP_QTY"));
                                newRow["HOLD_YN"] = _isPostingHold;
                                inLot.Rows.Add(newRow);
                            }
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_NEXT_PROC_MOVE", "INDATA,INLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //// 정상 처리 되었습니다
                                //Util.MessageInfo("SFU1889");

                                //// W/O 자동 변경을 위한 BIZ 호출 [MO등록해서 쓰는것들은 이제 사용 안함 2017-10-13]
                                //if (string.Equals(_processCode, Process.PRE_MIXING) || string.Equals(_processCode, Process.MIXING) || string.Equals(_processCode, Process.SRS_MIXING) ||
                                //    string.Equals(_processCode, Process.COATING) || string.Equals(_processCode, Process.SRS_COATING))
                                //{
                                //    // W/O 자동 변경
                                //    AutoChangeWorkOrder();
                                // 라벨자동발행
                                LabelAuto();
                                //}

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================InsCoating
        /// <summary>
        /// InsCoating 실적 확정
        /// </summary>
        private void ConfirmProcessInsCoating()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴
                string topInfo = SetConfirmTopInfo(UcResult_InsCoating.dgProductResult, UcResult_InsCoating.txtUnit.Text);

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_InsCoating.dgProductResult, UcResult_InsCoating.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_InsCoating.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용
                // PRE_MIXING,MIXING,COATING,SRS_MIXING,SRS_COATING 이외의 것 적용
                Dictionary<int, string> finalCutInfo = null;
                finalCutInfo = CheckFinalCutLot(UcResult_InsCoating.dgProductResult, UcResult_InsCoating.ExceedLengthQty, UcResult_InsCoating.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // DEFECT 자동 저장
                        UcResult_InsCoating.SaveDefect(UcResult_InsCoating.dgWipReasonTop, true);
                        UcResult_InsCoating.SaveDefect(UcResult_InsCoating.dgWipReasonBack, true);

                        double goodQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_InsCoating.dgProductResult.Rows[UcResult_InsCoating.dgProductResult.TopRows.Count].DataItem, "GOODQTY"));
                        double lossQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_InsCoating.dgProductResult.Rows[UcResult_InsCoating.dgProductResult.TopRows.Count].DataItem, "LOSSQTY"));

                        // 실적 저장
                        DataSet inDataSet = new DataSet();

                        // INDATA =====================================================
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("LOTID", typeof(string));
                        inTable.Columns.Add("INPUTQTY", typeof(double));
                        inTable.Columns.Add("OUTPUTQTY", typeof(double));
                        inTable.Columns.Add("RESNQTY", typeof(double));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                        newRow["INPUTQTY"] = Util.NVC(_dvProductLot["INPUTQTY"]);
                        newRow["OUTPUTQTY"] = goodQty;
                        newRow["RESNQTY"] = lossQty;
                        newRow["PROD_VER_CODE"] = UcResult_InsCoating.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgrmk.Rows[1].DataItem, "LOTID"))];
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["WRK_USERID"] = workUserID;
                        newRow["COAT_SIDE_TYPE"] = cboCoatSide.GetBindValue(); ;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_InsCoating.txtWorkDate.Tag);
                        inTable.Rows.Add(newRow);


                        // IN_INPUT =========================================================
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = _dvProductLot["LOTID"].ToString();
                        inInput.Rows.Add(newRow);

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_IC", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //// 정상 처리 되었습니다
                                //Util.MessageInfo("SFU1889");

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================Half Slitting
        /// <summary>
        /// 실적 확정
        /// </summary>
        private void ConfirmProcessHalfSlitting()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴 ?????????????????????????????
                string topInfo = SetConfirmTopInfo(UcResult_HalfSlitting.dgProductResult, UcResult_HalfSlitting.txtUnit.Text);

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_HalfSlitting.dgProductResult, UcResult_HalfSlitting.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // H/S공정 설비양품량 > 확정양품량 일 경우 인폼 추가 [2018-01-17]
                double eqptQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_HalfSlitting.dgProductResult.Rows[UcResult_HalfSlitting.dgProductResult.TopRows.Count].DataItem, "EQPT_END_QTY"));
                double goodQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_HalfSlitting.dgProductResult.Rows[UcResult_HalfSlitting.dgProductResult.TopRows.Count].DataItem, "GOODQTY"));
                if (eqptQty > 0 && eqptQty > goodQty)
                    addMessage += string.IsNullOrEmpty(addMessage) ? MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty }) : "\n" + MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty });  //설비수량[%1]이 양품량[%2]보다 많습니다. 불량/LOSS수량등 입력되지 않았는지 확인 바랍니다.

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_HalfSlitting.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용 ( 2017-01-17 ) CR-16
                Dictionary<int, string> finalCutInfo = null;

                finalCutInfo = CheckFinalCutLot(UcResult_HalfSlitting.dgProductResult, UcResult_HalfSlitting.ExceedLengthQty, UcResult_HalfSlitting.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //////////////////////////////////////////////////////// DEFECT 자동 저장
                        UcResult_HalfSlitting.SaveDefect(UcResult_HalfSlitting.dgWipReason, true);

                        //// 자동 Loss 처리 여부
                        //if (bool.Parse(finalCutInfo[2]) == true)
                        //{
                        //    if (UcResult_HalfSlitting.SetLossLot(UcResult_HalfSlitting.dgWipReason, UcResult_HalfSlitting.ItemCodeLenLack, Util.NVC_Decimal(finalCutInfo[5])) == false)
                        //        return;

                        //    UcResult_HalfSlitting.SaveDefect(UcResult_HalfSlitting.dgWipReason, true);
                        //}

                        ////////////////////////////////////////////////////// 실적 저장
                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("CUTYN", typeof(string));
                        inTable.Columns.Add("REMAINQTY", typeof(string));
                        inTable.Columns.Add("LANE_QTY", typeof(string));
                        inTable.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("INPUTQTY", typeof(decimal));
                        inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLot.Columns.Add("RESNQTY", typeof(decimal));
                        inLot.Columns.Add("WIPNOTE", typeof(string));
                        /////////////////////////////////////////////////////////////////////////////// INDATA
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["PROD_VER_CODE"] = UcResult_HalfSlitting.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        //newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgrmk.Rows[1].DataItem, "LOTID"))];
                        newRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        newRow["REMAINQTY"] = finalCutInfo[6];

                        newRow["LANE_QTY"] = UcResult_HalfSlitting.txtLaneQty.Value;
                        newRow["LANE_PTN_QTY"] = UcResult_HalfSlitting.txtLanePatternQty.Value;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_HalfSlitting.txtWorkDate.Tag);
                        inTable.Rows.Add(newRow);

                        /////////////////////////////////////////////////////////////////////////////// IN_INPUT
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = _dvProductLot["LOTID_PR"].ToString();
                        inInput.Rows.Add(newRow);

                        /////////////////////////////////////////////////////////////////////////////// inLot
                        C1DataGrid dg = UcResult_HalfSlitting.dgProductResult;

                        for (int i = dg.TopRows.Count; i < (dg.Rows.Count - dg.BottomRows.Count); i++)
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID"));
                            newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i].DataItem, "INPUTQTY"));
                            newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i].DataItem, "GOODQTY"));
                            newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOSSQTY"));
                            newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(UcResult_HalfSlitting.dgRemark.Rows[i - 1].DataItem, "LOTID"))];
                            inLot.Rows.Add(newRow);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_HS_UI", "INDATA,IN_INPUT,IN_LOT,IN_OUTLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //// 정상 처리 되었습니다
                                //Util.MessageInfo("SFU1889");

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================Roll Pressing
        private void GetWorkHalfSlittingSide()
        {
            try
            {
                //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
                if (_util.IsCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(_processCode, Process.ROLL_PRESSING))
                {
                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = _equipmentCode;
                    inDataTable.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_WRK_HALF_SLIT_SIDE_RP_ESWA", "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Text = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"]);
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"]);
                        }
                        else
                        {
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Text = string.Empty;
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag = null;
                        }
                    });
                }
                else
                {
                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));

                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = _equipmentCode;
                    inDataTable.Rows.Add(dr);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_WRK_HALF_SLIT_SIDE", "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Text = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"]);
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"]);
                        }
                        else
                        {
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Text = string.Empty;
                            UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag = null;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckRollQASampling()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPATTR_QAFLAG", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1382");     // LOT이 WIPATTR테이블에 없습니다. 
                    return false;
                }

                if (Convert.ToString(result.Rows[0]["QA_INSP_TRGT_FLAG"]).Equals("Y"))
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("LANGID", typeof(string));
                    IndataTable.Columns.Add("AREAID", typeof(string));
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("LOTID", typeof(string));
                    IndataTable.Columns.Add("WIPSEQ", typeof(string));
                    IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                    Indata["PROCID"] = _processCode;
                    Indata["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                    Indata["WIPSEQ"] = Util.NVC(_dvProductLot["WIPSEQ"]);

                    Indata["CLCT_PONT_CODE"] = null;
                    IndataTable.Rows.Add(Indata);

                    result = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count == 0)
                        result = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count > 0)
                    {
                        if (result.Select("INSP_ITEM_ID = 'E3000-0001'").Count() > 0)
                        {
                            DataRow[] inspRows = result.Select("INSP_ITEM_ID = 'E3000-0001'").Count() > 0 ? result.Select("INSP_ITEM_ID = 'E3000-0001'") : result.Select("INSP_ITEM_ID = 'SI516'");

                            foreach (DataRow inspRow in inspRows)
                            {
                                if (inspRow["CLCTVAL01"].Equals("") && !Util.NVC(inspRow["CLSS_NAME1"]).Contains("HG"))
                                {
                                    Util.MessageValidation("SFU1603");     // 샘플링 진행/마우저 두께를 모두 기입 바랍니다.
                                    return false;
                                }
                            }
                        }

                        if (result.Select("INSP_ITEM_ID = 'SI022'").Count() > 0)
                        {
                            DataRow[] inspRows = result.Select("INSP_ITEM_ID = 'SI022'").Count() > 0 ? result.Select("INSP_ITEM_ID = 'SI022'") : result.Select("INSP_ITEM_ID = 'SI516'");

                            foreach (DataRow inspRow in inspRows)
                            {
                                if (inspRow["CLCTVAL01"].Equals("") && !Util.NVC(inspRow["CLSS_NAME1"]).Contains("HG"))
                                {
                                    Util.MessageValidation("SFU1603");     // 샘플링 진행/마우저 두께를 모두 기입 바랍니다.
                                    return false;
                                }
                            }
                        }
                    }
                }

                //2024.10.17. 김영국 - HD1 불필요한 로직으로 주석 처리함.
                //if (!CheckValidInspectionSpec())
                //    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return true;
        }

        private bool CheckValidInspectionSpec()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));

            DataRow row = dt.NewRow();
            row["LANGID"] = LoginInfo.LANGID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INSP_ITEM_SPEC", "INDATA", "RSLTDT", dt);
            if (result.Rows.Count != 0)
            {
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    if (Convert.ToString(result.Rows[i]["CLCTVAL01"]).Equals("") || Util.NVC_Decimal(result.Rows[i]["CLCTVAL01"]) == 0)
                    {
                        Util.MessageValidation("SFU2886", new object[] { Util.NVC(result.Rows[i]["INSP_CLCTNAME"]) });     // {%1} 품질 값을 넣어주세요
                        return false;
                    }
                }
            }
            return true;
        }

        private DataTable GetMergeInfo(string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotID;
                Indata["PROCID"] = _processCode;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_HIST", "INDATA", "RSLTDT", IndataTable);

                return result;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return new DataTable();
        }

        /// <summary>
        /// RollPressing 실적 확정
        /// </summary>
        private void ConfirmProcessRollPressing()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴 ?????????????????????????????
                string topInfo = SetConfirmTopInfo(UcResult_RollPressing.dgProductResult, UcResult_RollPressing.txtUnit.Text);

                // 롤프레스 설비완공:소진완료->실적확정:잔량처리 시 validation
                string sReturnVal = SetConfirmRsnQty(UcResult_RollPressing.dgProductLotInfo, UcResult_RollPressing.txtRemainQty.Value);
                if (!string.IsNullOrEmpty(sReturnVal))
                    topInfo = topInfo + ((string.IsNullOrEmpty(topInfo)) ? "" : "\r\n\r\n") + sReturnVal;

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_RollPressing.dgProductResult, UcResult_RollPressing.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                //FastTrack 및 QA대상 LOT 일 경우  체크
                string sFastTrackChk = UcResult_RollPressing.sFastTrackChk;
                string sQaTagetChk = UcResult_RollPressing.sQaTagetChk;

                if (sFastTrackChk == "Y" && sQaTagetChk == string.Empty)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = "Fast Track : " + MessageDic.Instance.GetMessage("SFU8527"); // 이 Lot은 패스트트랙 요청받은 롤입니다. 압연시 품질 문제가 있을경우 상위자에게 보고하세요
                    else
                        addMessage += "\nFast Track : " + MessageDic.Instance.GetMessage("SFU8527"); // 이 Lot은 패스트트랙 요청받은 롤입니다. 압연시 품질 문제가 있을경우 상위자에게 보고하세요
                }
                if (sQaTagetChk == "Y" && sFastTrackChk == string.Empty)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = "Sampling : " + MessageDic.Instance.GetMessage("SFU8528"); // 이 롤은 샘플 lot입니다. QA샘플 채취하세요.
                    else
                        addMessage += "\nSampling : " + MessageDic.Instance.GetMessage("SFU8528"); // 이 롤은 샘플 lot입니다. QA샘플 채취하세요.
                }
                if (sQaTagetChk == "Y" && sFastTrackChk == "Y")
                {
                    if (string.IsNullOrEmpty(addMessage))
                    {
                        addMessage = "Fast Track : " + MessageDic.Instance.GetMessage("SFU8527");
                        addMessage += "\nSampling : " + MessageDic.Instance.GetMessage("SFU8528");
                    }
                    else
                    {
                        addMessage += "\nFast Track : " + MessageDic.Instance.GetMessage("SFU8527");
                        addMessage += "\nSampling : " + MessageDic.Instance.GetMessage("SFU8528");
                    }
                }
                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_RollPressing.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용 ( 2017-01-17 ) CR-16
                Dictionary<int, string> finalCutInfo = null;

                finalCutInfo = CheckFinalCutLot(UcResult_RollPressing.dgProductResult, UcResult_RollPressing.ExceedLengthQty, UcResult_RollPressing.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //////////////////////////////////////////////////////// DEFECT 자동 저장
                        UcResult_RollPressing.SaveDefect(UcResult_RollPressing.dgWipReason, true);

                        // RollMap Defect 좌표 반영
                        if (_isRollMapEquipment && _isRollMapLot)
                        {
                            UcResult_RollPressing.SaveDefectForRollMap(true);

                            if (UcResult_RollPressing.CheckDefectByActID() == false)
                            {
                                Util.MessageValidation("SFU10038");
                                return;
                            }
                        }
                        //// 자동 Loss 처리 여부
                        //if (bool.Parse(finalCutInfo[2]) == true)
                        //{
                        //    if (UcResult_RollPressing.SetLossLot(UcResult_RollPressing.dgWipReason, UcResult_RollPressing.ItemCodeLenLack, Util.NVC_Decimal(finalCutInfo[5])) == false)
                        //        return;

                        //    UcResult_RollPressing.SaveDefect(UcResult_RollPressing.dgWipReason, true);
                        //}

                        ////////////////////////////////////////////////////// 실적 저장
                        DataSet inDataSet = new DataSet();
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("CUTYN", typeof(string));
                        inTable.Columns.Add("REMAINQTY", typeof(string));
                        inTable.Columns.Add("REPROC_YN", typeof(string));           // 추가압연여부
                        inTable.Columns.Add("LANE_QTY", typeof(string));
                        inTable.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));
                        inTable.Columns.Add("HOLD_YN", typeof(string));

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("INPUTQTY", typeof(decimal));
                        inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLot.Columns.Add("RESNQTY", typeof(decimal));
                        /////////////////////////////////////////////////////////////////////////////// INDATA
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["PROD_VER_CODE"] = UcResult_RollPressing.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgrmk.Rows[1].DataItem, "LOTID"))];
                        newRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        newRow["REMAINQTY"] = finalCutInfo[6];
                        newRow["REPROC_YN"] = UcResult_RollPressing.chkExtraPress.IsChecked == true ? "Y" : "N";

                        newRow["LANE_QTY"] = UcResult_RollPressing.txtLaneQty.Value;
                        newRow["LANE_PTN_QTY"] = 1;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_RollPressing.txtWorkDate.Tag);
                        newRow["HOLD_YN"] = _isPostingHold;
                        inTable.Rows.Add(newRow);

                        /////////////////////////////////////////////////////////////////////////////// IN_INPUT
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = _dvProductLot["LOTID_PR"].ToString();
                        inInput.Rows.Add(newRow);

                        /////////////////////////////////////////////////////////////////////////////// inLot
                        C1DataGrid dg = UcResult_RollPressing.dgProductResult;

                        for (int i = 0; i < dg.GetRowCount(); i++)
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                            newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUTQTY"));
                            newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "GOODQTY"));
                            newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "LOSSQTY"));
                            inLot.Rows.Add(newRow);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_RP_UI", "INDATA,IN_INPUT,IN_LOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //// 정상 처리 되었습니다
                                //Util.MessageInfo("SFU1889");

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================ReWinder

        private void ConfirmProcessReWinder()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴
                string topInfo = SetConfirmTopInfo(UcResult_ReWinder.dgProductResult, UcResult_ReWinder.txtUnit.Text);

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_ReWinder.dgProductResult, UcResult_ReWinder.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_ReWinder.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용
                // PRE_MIXING,MIXING,COATING,SRS_MIXING,SRS_COATING 이외의 것 적용
                Dictionary<int, string> finalCutInfo = null;
                finalCutInfo = CheckFinalCutLot(UcResult_ReWinder.dgProductResult, UcResult_ReWinder.ExceedLengthQty, UcResult_ReWinder.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // DEFECT 자동 저장
                        UcResult_ReWinder.SaveDefect(UcResult_ReWinder.dgWipReason, true);

                        // 실적 저장
                        DataSet inDataSet = new DataSet();

                        // INDATA =====================================================
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("CUTYN", typeof(string));
                        inTable.Columns.Add("REMAINQTY", typeof(string));
                        inTable.Columns.Add("LANE_QTY", typeof(string));
                        inTable.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["PROD_VER_CODE"] = UcResult_ReWinder.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgrmk.Rows[1].DataItem, "LOTID"))];
                        newRow["USERID"] = LoginInfo.USERID;

                        newRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부

                        newRow["REMAINQTY"] = finalCutInfo[6];
                        newRow["LANE_QTY"] = UcResult_ReWinder.txtLaneQty.Value;
                        newRow["LANE_PTN_QTY"] = 1;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_ReWinder.txtWorkDate.Tag);
                        inTable.Rows.Add(newRow);


                        // IN_INPUT =========================================================                        
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = _dvProductLot["LOTID_PR"].ToString();
                        inInput.Rows.Add(newRow);


                        // INLOT =========================================================
                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("INPUTQTY", typeof(decimal));
                        inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLot.Columns.Add("RESNQTY", typeof(decimal));

                        C1DataGrid dg = UcResult_ReWinder.dgProductResult;
                        for (int i = 0; i < dg.GetRowCount(); i++)
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                            newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUTQTY"));
                            newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "GOODQTY"));
                            newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "LOSSQTY"));
                            inLot.Rows.Add(newRow);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_SI_UI", "INDATA,IN_INPUT,IN_LOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================Slitting
        private void ConfirmProcessSlitting()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴 ?????????????????????????????
                string topInfo = SetConfirmTopInfo(UcResult_Slitting.dgProductResult, UcResult_Slitting.txtUnit.Text);

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_Slitting.dgProductResult, UcResult_Slitting.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // H/S공정 설비양품량 > 확정양품량 일 경우 인폼 추가 [2018-01-17]
                double eqptQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_Slitting.dgProductResult.Rows[UcResult_Slitting.dgProductResult.TopRows.Count].DataItem, "EQPT_END_QTY"));
                double goodQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_Slitting.dgProductResult.Rows[UcResult_Slitting.dgProductResult.TopRows.Count].DataItem, "GOODQTY"));
                if (eqptQty > 0 && eqptQty > goodQty)
                    addMessage += string.IsNullOrEmpty(addMessage) ? MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty }) : "\n" + MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty });  //설비수량[%1]이 양품량[%2]보다 많습니다. 불량/LOSS수량등 입력되지 않았는지 확인 바랍니다.

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_Slitting.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용 ( 2017-01-17 ) CR-16
                // PRE_MIXING,MIXING,COATING,SRS_MIXING,SRS_COATING 이외의 것 적용
                Dictionary<int, string> finalCutInfo = null;
                finalCutInfo = CheckFinalCutLot(UcResult_Slitting.dgProductResult, UcResult_Slitting.ExceedLengthQty, UcResult_Slitting.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // DEFECT 자동 저장
                        UcResult_Slitting.SaveDefect(UcResult_Slitting.dgWipReason, true);

                        DataSet inDataSet = new DataSet();

                        // INDATA =======================================================
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("CUTYN", typeof(string));
                        inTable.Columns.Add("REMAINQTY", typeof(string));
                        inTable.Columns.Add("LANE_QTY", typeof(string));
                        inTable.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["PROD_VER_CODE"] = UcResult_Slitting.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        newRow["REMAINQTY"] = finalCutInfo[6];
                        newRow["LANE_QTY"] = 1;
                        newRow["LANE_PTN_QTY"] = 1;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_Slitting.txtWorkDate.Tag);
                        inTable.Rows.Add(newRow);


                        // IN_CUTLOT =======================================================
                        DataTable inLotDetail = inDataSet.Tables.Add("IN_CUTLOT");
                        inLotDetail.Columns.Add("CUT_ID", typeof(string));
                        inLotDetail.Columns.Add("INPUTQTY", typeof(decimal));
                        inLotDetail.Columns.Add("LABEL_PASS_HOLD_FLAG", typeof(string));

                        DataRow inLotDetailDataRow = null;
                        inLotDetailDataRow = inLotDetail.NewRow();
                        inLotDetailDataRow["CUT_ID"] = Util.NVC(_dvProductLot["CUT_ID"]);

                        // 2021.12.03. 한종현. ESNJ 1동 생산시스템 1단계
                        // 슬리팅 공정진척 진행 시 생산량 대신 모Lot 투입량으로 처리되는 오류 수정
                        if (bool.Parse(finalCutInfo[2]) == true)
                            //inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(_dvProductLot["INPUTQTY"]) + Convert.ToDouble(finalCutInfo[5]);
                            inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(UcResult_Slitting.dgProductResult.Rows[UcResult_Slitting.dgProductResult.TopRows.Count].DataItem, "INPUTQTY")) + Convert.ToDouble(finalCutInfo[5]);
                        else
                            //inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(_dvProductLot["INPUTQTY"]);
                            inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(UcResult_Slitting.dgProductResult.Rows[UcResult_Slitting.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDetailDataRow["LABEL_PASS_HOLD_FLAG"] = string.IsNullOrEmpty(_labelPassHoldFlag) ? "Y" : _labelPassHoldFlag;
                        inLotDetail.Rows.Add(inLotDetailDataRow);


                        // IN_PRLOT =======================================================
                        DataTable inPLotDataTable = inDataSet.Tables.Add("IN_PRLOT");
                        inPLotDataTable.Columns.Add("LOTID", typeof(string));
                        inPLotDataTable.Columns.Add("OUTQTY", typeof(string));
                        inPLotDataTable.Columns.Add("CUTYN", typeof(string));

                        DataRow inPLotDataRow = inPLotDataTable.NewRow();
                        inPLotDataRow["LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                        inPLotDataRow["OUTQTY"] = finalCutInfo[6];
                        inPLotDataRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        inPLotDataTable.Rows.Add(inPLotDataRow);


                        // IN_INPUT =======================================================
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }


                        // IN_INPUT =======================================================
                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                        inInput.Rows.Add(newRow);


                        // IN_OUTLOT =======================================================
                        DataTable inRemartTable = inDataSet.Tables.Add("IN_OUTLOT");
                        inRemartTable.Columns.Add("OUT_LOTID", typeof(string));
                        inRemartTable.Columns.Add("WIPNOTE", typeof(string));

                        DataTable dt = ((DataView)UcResult_Slitting.dgRemark.ItemsSource).Table;
                        for (int i = 1; i < dt.Rows.Count; i++)
                        {
                            DataRow row = inRemartTable.NewRow();
                            row["OUT_LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);
                            row["WIPNOTE"] = remarkInfo[Util.NVC(dt.Rows[i]["LOTID"])];
                            inDataSet.Tables["IN_OUTLOT"].Rows.Add(row);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CUTID_SL", "INDATA,IN_CUTLOT,IN_PRLOT,IN_INPUT,IN_OUTLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================Taping
        private void ConfirmProcessTaping()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴 ?????????????????????????????
                string topInfo = SetConfirmTopInfo(UcResult_Taping.dgProductResult, UcResult_Taping.txtUnit.Text);

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_Taping.dgProductResult, UcResult_Taping.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_Taping.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용 ( 2017-01-17 ) CR-16
                Dictionary<int, string> finalCutInfo = null;

                finalCutInfo = CheckFinalCutLot(UcResult_Taping.dgProductResult, UcResult_Taping.ExceedLengthQty, UcResult_Taping.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // DEFECT 자동 저장
                        UcResult_Taping.SaveDefect(UcResult_Taping.dgWipReason, true);

                        // 실적 저장
                        DataSet inDataSet = new DataSet();

                        // INDATA
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("CUTYN", typeof(string));
                        inTable.Columns.Add("REMAINQTY", typeof(string));
                        inTable.Columns.Add("LANE_QTY", typeof(string));
                        inTable.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["PROD_VER_CODE"] = UcResult_Taping.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgrmk.Rows[1].DataItem, "LOTID"))];
                        newRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        newRow["REMAINQTY"] = finalCutInfo[6];
                        newRow["LANE_QTY"] = UcResult_Taping.txtLaneQty.Value;
                        newRow["LANE_PTN_QTY"] = 1;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_Taping.txtWorkDate.Tag);
                        inTable.Rows.Add(newRow);

                        // IN_INPUT
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = _dvProductLot["LOTID_PR"].ToString();
                        inInput.Rows.Add(newRow);

                        // INLOT
                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("INPUTQTY", typeof(decimal));
                        inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLot.Columns.Add("RESNQTY", typeof(decimal));

                        C1DataGrid dg = UcResult_Taping.dgProductResult;

                        for (int i = 0; i < dg.GetRowCount(); i++)
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                            newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUTQTY"));
                            newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "GOODQTY"));
                            newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "LOSSQTY"));
                            inLot.Rows.Add(newRow);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_TP_UI", "INDATA,IN_INPUT,IN_LOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================Heat Treatment
        private void ConfirmProcessHeatTreatment()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴 ?????????????????????????????
                string topInfo = SetConfirmTopInfo(UcResult_HeatTreatment.dgProductResult, UcResult_HeatTreatment.txtUnit.Text);

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_HeatTreatment.dgProductResult, UcResult_HeatTreatment.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_HeatTreatment.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용 ( 2017-01-17 ) CR-16
                Dictionary<int, string> finalCutInfo = null;

                finalCutInfo = CheckFinalCutLot(UcResult_HeatTreatment.dgProductResult, UcResult_HeatTreatment.ExceedLengthQty, UcResult_HeatTreatment.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // DEFECT 자동 저장
                        UcResult_HeatTreatment.SaveDefect(UcResult_HeatTreatment.dgWipReason, true);

                        // 실적 저장
                        DataSet inDataSet = new DataSet();

                        // INDATA
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("CUTYN", typeof(string));
                        inTable.Columns.Add("REMAINQTY", typeof(string));
                        inTable.Columns.Add("LANE_QTY", typeof(string));
                        inTable.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["PROD_VER_CODE"] = UcResult_HeatTreatment.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["WIPNOTE"] = remarkInfo[Util.NVC(DataTableConverter.GetValue(dgrmk.Rows[1].DataItem, "LOTID"))];
                        newRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        newRow["REMAINQTY"] = finalCutInfo[6];
                        newRow["LANE_QTY"] = UcResult_HeatTreatment.txtLaneQty.Value;
                        newRow["LANE_PTN_QTY"] = 1;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_HeatTreatment.txtWorkDate.Tag);
                        inTable.Rows.Add(newRow);

                        // IN_INPUT
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = _dvProductLot["LOTID_PR"].ToString();
                        inInput.Rows.Add(newRow);

                        // INLOT
                        DataTable inLot = inDataSet.Tables.Add("INLOT");
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("INPUTQTY", typeof(decimal));
                        inLot.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inLot.Columns.Add("RESNQTY", typeof(decimal));

                        C1DataGrid dg = UcResult_HeatTreatment.dgProductResult;

                        for (int i = 0; i < dg.GetRowCount(); i++)
                        {
                            newRow = inLot.NewRow();
                            newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                            newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUTQTY"));
                            newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "GOODQTY"));
                            newRow["RESNQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "LOSSQTY"));
                            inLot.Rows.Add(newRow);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_HT_UI", "INDATA,IN_INPUT,IN_LOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region =============================재와인딩
        /// <summary>
        /// ReWinding( 실적 확정
        /// </summary>
        private void ConfirmProcessReWinding()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // ESGM 재와인딩 실적확정시 잔량이 남아 신규 LOT 발번 되는 경우 작업자가 잔량 LOT생성 여부 확인 할수 있도록 메세지 반영 (2022-07-04)
                DataRow[] drInputLot = DataTableConverter.Convert(UcResult_ReWinding.dgProductResult.ItemsSource).Select("INPUT_LOTID = '" + _dvProductLot["LOTID"] + "'");
                decimal InputQty = Util.NVC_Decimal(drInputLot[0]["INPUT_QTY"]);
                decimal confirmQty = Util.NVC_Decimal(drInputLot[0]["CNFM_QTY"]);
                decimal remainQty = InputQty - confirmQty;

                string addMessage = string.Empty;
                if (remainQty > 0)
                {
                    // CSR : E20230412-001093 - [ESGM] 공정진척 실적 자동화 (롤프레스,재와인딩기)
                    string[] sAttr = { "" };
                    if (_util.IsAreaCommoncodeAttrUse("PROD_PROC_AUTO_CALC", _processCode, sAttr) && (string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.EQPT_END)))
                    {
                        Util.MessageValidation("SFU8910");     // 언와인더 잔량이 남았습니다. 확인해주세요.
                        return;
                    }
                    else
                    {
                        //신규 MESSAGE : 잔량이 1% m 남아서 신규 잔량 LOT이 생성 됩니다. 계속 진행 하시겠습니까?
                        addMessage = MessageDic.Instance.GetMessage("SFU8505", remainQty.ToString());
                    }
                }

                // 실적 확정 하시겠습니까?
                //Util.MessageConfirm("SFU1706", (sresult) =>
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1706"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        ////////////////////////////////////////////////////// DEFECT 자동 저장
                        UcResult_ReWinding.SaveDefect(UcResult_ReWinding.dgWipReason, true);
                        // RollMap Defect 좌표 반영
                        if (_isRollMapEquipment)
                        {
                            UcResult_ReWinding.SaveDefectForRollMap(true);
                        }

                        DataSet inDataSet = new DataSet();
                        DataTable inData = inDataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("WIPSEQ", typeof(decimal));
                        inData.Columns.Add("SHIFT", typeof(string));
                        inData.Columns.Add("WRK_USERID", typeof(string));
                        inData.Columns.Add("WRK_USER_NAME", typeof(string));
                        inData.Columns.Add("INPUTQTY", typeof(decimal));
                        inData.Columns.Add("OUTPUTQTY", typeof(decimal));
                        inData.Columns.Add("OUTPUTQTY2", typeof(decimal));
                        inData.Columns.Add("RESNQTY", typeof(decimal));
                        inData.Columns.Add("USERID", typeof(string));

                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        //inInput.Columns.Add("INPUT_SEQNO", typeof(Int64));
                        inInput.Columns.Add("INPUT_SEQNO", typeof(string)); // 20204.10.21. 김영국 - INPUT_SEQ 타입 변경 INT64 -
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));
                        inInput.Columns.Add("CNFM_QTY", typeof(decimal));
                        ///////////////////////////////////////////////////////////////////////////////
                        int LaneQty = string.IsNullOrWhiteSpace(_dvProductLot["LANE_QTY"].ToString()) ? 1 : int.Parse(_dvProductLot["LANE_QTY"].ToString());
                        int LanePtnQty = string.IsNullOrWhiteSpace(_dvProductLot["LANE_PTN_QTY"].ToString()) ? 1 : int.Parse(_dvProductLot["LANE_PTN_QTY"].ToString());

                        DataRow newRow = inData.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["PROCID"] = _processCode;
                        newRow["LOTID"] = _dvProductLot["LOTID"];
                        newRow["WIPSEQ"] = _dvProductLot["WIPSEQ"];
                        newRow["SHIFT"] = shift;
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["INPUTQTY"] = UcResult_ReWinding.txtProductionQty.Value;
                        newRow["OUTPUTQTY"] = UcResult_ReWinding.txtGoodQty.Value;
                        newRow["OUTPUTQTY2"] = UcResult_ReWinding.txtGoodQty.Value * LaneQty * LanePtnQty;
                        newRow["RESNQTY"] = UcResult_ReWinding.txtDefectQty.Value;
                        newRow["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(newRow);

                        DataTable dt = DataTableConverter.Convert(UcResult_ReWinding.dgProductResult.ItemsSource);

                        foreach (DataRow row in dt.Rows)
                        {
                            newRow = inInput.NewRow();
                            newRow["INPUT_SEQNO"] = row["INPUT_SEQNO"];
                            newRow["INPUT_LOTID"] = row["INPUT_LOTID"];
                            newRow["CNFM_QTY"] = row["CNFM_QTY"];
                            inInput.Rows.Add(newRow);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_RW_DRB", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //// 정상 처리 되었습니다
                                //Util.MessageInfo("SFU1889");

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();

                                // 설비 재조회
                                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);
                    }
                }, false, false, string.Empty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //20211215 2차 Slitting 공정진척DRB 화면 개발 START
        #region =============================Slitting
        private void ConfirmProcessTwoSlitting()
        {
            try
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");
                string shift = drShift[0]["SHFT_ID"].ToString();
                string workUserID = drShift[0]["WRK_USERID"].ToString();
                string workUserName = drShift[0]["VAL002"].ToString();

                // END TIME을 현재 시간으로 넣기 위하여 DB에서 조회
                string sEndTime = GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss");

                // TOP 정보 추가 (생산량과 설비완료수량 차이가 30이상인 경우 확인) => SRS 코터/슬리터 제외 요청 들어옴 ?????????????????????????????
                string topInfo = SetConfirmTopInfo(UcResult_TwoSlitting.dgProductResult, UcResult_TwoSlitting.txtUnit.Text);

                // 설비 LOSS 체크
                string addMessage = SetConfirmEqpLossMessage();

                if (ValidLaneWrongQty(UcResult_TwoSlitting.dgProductResult, UcResult_TwoSlitting.txtLaneQty.Value) == false)
                {
                    if (string.IsNullOrEmpty(addMessage))
                        addMessage = MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                    else
                        addMessage += "\n" + MessageDic.Instance.GetMessage("SFU3470"); // 알림 : 현재 LANE수가 1입니다.
                }

                // H/S공정 설비양품량 > 확정양품량 일 경우 인폼 추가 [2018-01-17]
                double eqptQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_TwoSlitting.dgProductResult.Rows[UcResult_TwoSlitting.dgProductResult.TopRows.Count].DataItem, "EQPT_END_QTY"));
                double goodQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_TwoSlitting.dgProductResult.Rows[UcResult_TwoSlitting.dgProductResult.TopRows.Count].DataItem, "GOODQTY"));
                if (eqptQty > 0 && eqptQty > goodQty)
                    addMessage += string.IsNullOrEmpty(addMessage) ? MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty }) : "\n" + MessageDic.Instance.GetMessage("SFU4474", new object[] { eqptQty, goodQty });  //설비수량[%1]이 양품량[%2]보다 많습니다. 불량/LOSS수량등 입력되지 않았는지 확인 바랍니다.

                // REMARK 필요 정보 취합 [2017-05-24]
                C1DataGrid dgrmk = UcResult_TwoSlitting.dgRemark;

                Dictionary<string, string> remarkInfo = GetRemarkConvert(dgrmk);
                if (remarkInfo.Count == 0)
                {
                    Util.MessageValidation("SFU3484");     // 특이사항 정보를 확인 바랍니다.
                    return;
                }

                // COATER BACK을 포함한 COATER 이후 공정은 해당 로직 적용 ( 2017-01-17 ) CR-16
                // PRE_MIXING,MIXING,COATING,SRS_MIXING,SRS_COATING 이외의 것 적용
                Dictionary<int, string> finalCutInfo = null;
                finalCutInfo = CheckFinalCutLot(UcResult_TwoSlitting.dgProductResult, UcResult_TwoSlitting.ExceedLengthQty, UcResult_TwoSlitting.txtUnit.Text);
                if (bool.Parse(finalCutInfo[0]) == false)
                    return;

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(finalCutInfo[4], bool.Parse(finalCutInfo[3]), false, ObjectDic.Instance.GetObjectName("HOLD여부"), addMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result, isHold) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // DEFECT 자동 저장
                        UcResult_TwoSlitting.SaveDefect(UcResult_TwoSlitting.dgWipReason, true);

                        DataSet inDataSet = new DataSet();

                        // INDATA =======================================================
                        DataTable inTable = inDataSet.Tables.Add("INDATA");
                        inTable.Columns.Add("SRCTYPE", typeof(string));
                        inTable.Columns.Add("IFMODE", typeof(string));
                        inTable.Columns.Add("EQPTID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));
                        inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                        inTable.Columns.Add("SHIFT", typeof(string));
                        inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));
                        inTable.Columns.Add("WRK_USERID", typeof(string));
                        inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                        inTable.Columns.Add("WIPNOTE", typeof(string));
                        inTable.Columns.Add("CUTYN", typeof(string));
                        inTable.Columns.Add("REMAINQTY", typeof(string));
                        inTable.Columns.Add("LANE_QTY", typeof(string));
                        inTable.Columns.Add("LANE_PTN_QTY", typeof(string));
                        inTable.Columns.Add("CALDATE", typeof(DateTime));

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["PROD_VER_CODE"] = UcResult_TwoSlitting.txtVersion.Text;
                        newRow["SHIFT"] = shift;
                        newRow["WIPDTTM_ED"] = Convert.ToDateTime(sEndTime);
                        newRow["WRK_USERID"] = workUserID;
                        newRow["WRK_USER_NAME"] = workUserName;
                        newRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        newRow["REMAINQTY"] = finalCutInfo[6];
                        newRow["LANE_QTY"] = 1;
                        newRow["LANE_PTN_QTY"] = 1;
                        newRow["CALDATE"] = Convert.ToDateTime(UcResult_TwoSlitting.txtWorkDate.Tag);
                        inTable.Rows.Add(newRow);


                        // IN_CUTLOT =======================================================
                        DataTable inLotDetail = inDataSet.Tables.Add("IN_CUTLOT");
                        inLotDetail.Columns.Add("CUT_ID", typeof(string));
                        inLotDetail.Columns.Add("INPUTQTY", typeof(decimal));
                        inLotDetail.Columns.Add("LABEL_PASS_HOLD_FLAG", typeof(string));

                        DataRow inLotDetailDataRow = null;
                        inLotDetailDataRow = inLotDetail.NewRow();
                        inLotDetailDataRow["CUT_ID"] = Util.NVC(_dvProductLot["CUT_ID"]);

                        // 2021.12.03. 한종현. ESNJ 1동 생산시스템 1단계
                        // 슬리팅 공정진척 진행 시 생산량 대신 모Lot 투입량으로 처리되는 오류 수정
                        if (bool.Parse(finalCutInfo[2]) == true)
                            //inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(_dvProductLot["INPUTQTY"]) + Convert.ToDouble(finalCutInfo[5]);
                            inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(UcResult_TwoSlitting.dgProductResult.Rows[UcResult_TwoSlitting.dgProductResult.TopRows.Count].DataItem, "INPUTQTY")) + Convert.ToDouble(finalCutInfo[5]);
                        else
                            //inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(_dvProductLot["INPUTQTY"]);
                            inLotDetailDataRow["INPUTQTY"] = Convert.ToDouble(DataTableConverter.GetValue(UcResult_TwoSlitting.dgProductResult.Rows[UcResult_TwoSlitting.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));
                        inLotDetailDataRow["LABEL_PASS_HOLD_FLAG"] = string.IsNullOrEmpty(_labelPassHoldFlag) ? "Y" : _labelPassHoldFlag;
                        inLotDetail.Rows.Add(inLotDetailDataRow);


                        // IN_PRLOT =======================================================
                        DataTable inPLotDataTable = inDataSet.Tables.Add("IN_PRLOT");
                        inPLotDataTable.Columns.Add("LOTID", typeof(string));
                        inPLotDataTable.Columns.Add("OUTQTY", typeof(string));
                        inPLotDataTable.Columns.Add("CUTYN", typeof(string));

                        DataRow inPLotDataRow = inPLotDataTable.NewRow();
                        inPLotDataRow["LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                        inPLotDataRow["OUTQTY"] = finalCutInfo[6];
                        inPLotDataRow["CUTYN"] = bool.Parse(finalCutInfo[1]) == true ? "N" : "Y";    // FINAL CUT 여부
                        inPLotDataTable.Rows.Add(inPLotDataRow);


                        // IN_INPUT =======================================================
                        DataTable eqpt_mount = new DataTable();
                        eqpt_mount.Columns.Add("EQPTID", typeof(string));

                        DataRow eqpt_mount_row = eqpt_mount.NewRow();
                        eqpt_mount_row["EQPTID"] = _equipmentCode;
                        eqpt_mount.Rows.Add(eqpt_mount_row);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "RQSTDT", "RSLTDT", eqpt_mount);

                        if (EQPT_PSTN_ID.Rows.Count < 0)
                        {
                            Util.MessageValidation("SFU1398");     // MMD에 설비 투입을 입력해주세요.
                            return;
                        }


                        // IN_INPUT =======================================================
                        DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInput.Columns.Add("INPUT_LOTID", typeof(string));

                        newRow = inInput.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                        inInput.Rows.Add(newRow);


                        // IN_OUTLOT =======================================================
                        DataTable inRemartTable = inDataSet.Tables.Add("IN_OUTLOT");
                        inRemartTable.Columns.Add("OUT_LOTID", typeof(string));
                        inRemartTable.Columns.Add("WIPNOTE", typeof(string));

                        DataTable dt = ((DataView)UcResult_TwoSlitting.dgRemark.ItemsSource).Table;
                        for (int i = 1; i < dt.Rows.Count; i++)
                        {
                            DataRow row = inRemartTable.NewRow();
                            row["OUT_LOTID"] = Util.NVC(dt.Rows[i]["LOTID"]);
                            row["WIPNOTE"] = remarkInfo[Util.NVC(dt.Rows[i]["LOTID"])];
                            inDataSet.Tables["IN_OUTLOT"].Rows.Add(row);
                        }

                        ShowLoadingIndicator();

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_LOT_CUTID_SL", "INDATA,IN_CUTLOT,IN_PRLOT,IN_INPUT,IN_OUTLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                HiddenLoadingIndicator();

                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // 라벨자동발행
                                LabelAuto();

                                // 화면 전환
                                ButtonProductList_Click(null, null);

                                // 생산 Lot 재조회
                                SelectProductLot(string.Empty);
                                SetProductLotList();
                            }
                            catch (Exception ex)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(ex);
                            }
                        }, inDataSet);

                    }
                }, false, Util.NVC_Decimal(finalCutInfo[5]) > 0 ? true : false, topInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        //20211215 2차 Slitting 공정진척DRB 화면 개발 END
        #endregion

        #region [Func]
        #region =============================공통
        private void ApplyPermissions()
        {
            if (UcElectrodeCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }

        private void SetControlClear(bool bClearequipmen = false)
        {
            if (bClearequipmen)
            {
                _equipmentCode = string.Empty;
                _equipmentName = string.Empty;
            }
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            _productLot = string.Empty;
            _isLastBatch = false;

            _dvProductLot = null;

            // 대기 Lot 조회
            UcElectrodeProductLot.chkWait.IsChecked = false;
            UcElectrodeProductLot.chkWait.Visibility = Visibility.Collapsed;

            Util.gridClear(UcElectrodeEquipment.DgEquipment);
            Util.gridClear(UcElectrodeProductLot.DgProductLot);

            UcElectrodeProductLot.txtSelectEquipment.Text = string.Empty;
            UcElectrodeProductLot.txtSelectEquipmentName.Text = string.Empty;
            UcElectrodeProductLot.txtWorkHalfSlittingSide.Text = string.Empty;
            UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag = null;
            UcElectrodeProductLot.txtSelectLot.Text = string.Empty;

            //HideAllRackRateMode();
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
            _processCode = cboProcess.SelectedValue.ToString();
            _processName = cboProcess.Text;

            // 공정별 컨트롤 설정
            SeProcessInUserControls();

            ///////////////////////////////// Re-Winding 공정 체크
            SetReWindingProcess();

            UcElectrodeCommand.ProcessCode = string.IsNullOrWhiteSpace(_processCode) ? LoginInfo.CFG_PROC_ID : _processCode;
            if (DgProductLot.Visibility == Visibility.Visible)
                UcElectrodeCommand.SetButtonExtraVisibility(true, _reWindingProcess == "Y" ? true : false);
            else
                UcElectrodeCommand.SetButtonExtraVisibility(false, _reWindingProcess == "Y" ? true : false);

            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SetUserControlEquipment()
        {
            UcElectrodeEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
            UcElectrodeEquipment.ProcessCode = _processCode;
            UcElectrodeEquipment.ProcessName = _processName;
            UcElectrodeEquipment.EquipmentCode = cboEquipment.SelectedItemsToString;
        }

        private void SetUserControlProductLot()
        {
            UcElectrodeProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
            UcElectrodeProductLot.ProcessCode = _processCode;
            UcElectrodeProductLot.ProcessName = _processName;
            UcElectrodeProductLot.EquipmentCode = cboEquipment.SelectedItemsToString;
            if (cboCoatSide.Visibility.Equals(Visibility.Visible))
            {
                UcElectrodeProductLot.CoatSide = cboCoatSide.GetStringValue();
            }
            else
            {
                UcElectrodeProductLot.CoatSide = null;
            }
            UcElectrodeProductLot.ReWindingProcess = _reWindingProcess;
            UcElectrodeProductLot.LdrLotIdentBasCode = _ldrLotIdentBasCode;
            UcElectrodeProductLot.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
            UcElectrodeProductLot.SetControlVisibility();

            UcElectrodeCommand.btnRollMap.IsEnabled = false;
        }

        private void SetUserControlProductionResultVisibility()
        {
            grdProductionResult_Mixing.Visibility = Visibility.Collapsed;
            grdProductionResult_Coating.Visibility = Visibility.Collapsed;
            grdProductionResult_HalfSlitting.Visibility = Visibility.Collapsed;
            grdProductionResult_RollPressing.Visibility = Visibility.Collapsed;
            grdProductionResult_ReWinding.Visibility = Visibility.Collapsed;

            if (_processCode == Process.MIXING ||
                _processCode == Process.PRE_MIXING ||
                _processCode == Process.BS ||
                _processCode == Process.CMC ||
                _processCode == Process.InsulationMixing ||
                _processCode == Process.DAM_MIXING)
            {
                // Mixing, 선분산 Mixing, Binder Solution, CMC Solution
                grdProductionResult_Mixing.Visibility = Visibility.Visible;
            }
            else if (_processCode == Process.COATING)
            {
                grdProductionResult_Coating.Visibility = Visibility.Visible;
            }
            else if (_processCode == Process.INS_COATING)
            {
                grdProductionResult_InsCoating.Visibility = Visibility.Visible;
            }
            else if (_processCode == Process.HALF_SLITTING)
            {
                grdProductionResult_HalfSlitting.Visibility = Visibility.Visible;
            }
            else if (_processCode == Process.SLITTING)
            {
                grdProductionResult_Slitting.Visibility = Visibility.Visible;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (_processCode == Process.TWO_SLITTING)
            {
                grdProductionResult_TwoSlitting.Visibility = Visibility.Visible;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else if (_processCode == Process.ROLL_PRESSING)
            {
                grdProductionResult_RollPressing.Visibility = Visibility.Visible;
            }
            else if (_processCode == Process.REWINDER)
            {
                grdProductionResult_ReWinder.Visibility = Visibility.Visible;
            }
            else if (_processCode == Process.TAPING)
            {
                grdProductionResult_Taping.Visibility = Visibility.Visible;
            }
            else if (_processCode == Process.HEAT_TREATMENT)
            {
                grdProductionResult_HeatTreatment.Visibility = Visibility.Visible;
            }
            else if (_reWindingProcess == "Y")
            {
                grdProductionResult_ReWinding.Visibility = Visibility.Visible;
            }

            SetUserControlProductionResult();
        }

        private void SetUserControlProductionResult()
        {
            if (_processCode == Process.MIXING ||
                _processCode == Process.PRE_MIXING ||
                _processCode == Process.BS ||
                _processCode == Process.CMC ||
                _processCode == Process.InsulationMixing ||
                _processCode == Process.DAM_MIXING
                )
            {
                UcResult_Mixing.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_Mixing.ProcessCode = _processCode;
                UcResult_Mixing.ProcessName = _processName;
                UcResult_Mixing.EquipmentCode = _equipmentCode;
                UcResult_Mixing.EquipmentName = _equipmentName;
                UcResult_Mixing.DvProductLot = _dvProductLot;
                SetUserControlEquipmentDataTable();
                UcResult_Mixing.SelectProductionResult();
            }
            else if (_processCode == Process.COATING)
            {
                UcResult_CoatingAuto.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_CoatingAuto.ProcessCode = _processCode;
                UcResult_CoatingAuto.ProcessName = _processName;
                UcResult_CoatingAuto.EquipmentCode = _equipmentCode;
                UcResult_CoatingAuto.EquipmentName = _equipmentName;
                UcResult_CoatingAuto.DvProductLot = _dvProductLot;
                UcResult_CoatingAuto.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_CoatingAuto.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                UcResult_CoatingAuto.IsRollMapEquipment = _isRollMapEquipment;       // RollMap 적용여부
                UcResult_CoatingAuto.IsRollMapLot = _isRollMapLot;                  // RollMap Lot 여부
                UcResult_CoatingAuto.IsRollMapSBL = _isRollMapSBL;

                SetUserControlEquipmentDataTable();
                UcResult_CoatingAuto.SelectProductionResult();
            }
            else if (_processCode == Process.INS_COATING)
            {
                UcResult_InsCoating.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_InsCoating.ProcessCode = _processCode;
                UcResult_InsCoating.ProcessName = _processName;
                UcResult_InsCoating.EquipmentCode = _equipmentCode;
                UcResult_InsCoating.EquipmentName = _equipmentName;
                UcResult_InsCoating.DvProductLot = _dvProductLot;
                UcResult_InsCoating.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_InsCoating.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                UcResult_InsCoating.CoatSide = cboCoatSide.GetStringValue();
                SetUserControlEquipmentDataTable();
                UcResult_InsCoating.SelectProductionResult();
            }
            else if (_processCode == Process.HALF_SLITTING)
            {
                UcResult_HalfSlitting.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_HalfSlitting.ProcessCode = _processCode;
                UcResult_HalfSlitting.ProcessName = _processName;
                UcResult_HalfSlitting.EquipmentCode = _equipmentCode;
                UcResult_HalfSlitting.EquipmentName = _equipmentName;
                UcResult_HalfSlitting.DvProductLot = _dvProductLot;
                UcResult_HalfSlitting.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_HalfSlitting.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                SetUserControlEquipmentDataTable();
                UcResult_HalfSlitting.SelectProductionResult();
            }
            else if (_processCode == Process.SLITTING)
            {
                UcResult_Slitting.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_Slitting.ProcessCode = _processCode;
                UcResult_Slitting.ProcessName = _processName;
                UcResult_Slitting.EquipmentCode = _equipmentCode;
                UcResult_Slitting.EquipmentName = _equipmentName;
                UcResult_Slitting.DvProductLot = _dvProductLot;
                UcResult_Slitting.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_Slitting.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                UcResult_Slitting.IsRollMapEquipment = _isRollMapEquipment;       // RollMap 적용여부
                UcResult_Slitting.IsRollMapLot = _isRollMapLot;                 // RollMap Lot 여부
                SetUserControlEquipmentDataTable();
                UcResult_Slitting.SelectProductionResult();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (_processCode == Process.TWO_SLITTING)
            {
                UcResult_TwoSlitting.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_TwoSlitting.ProcessCode = _processCode;
                UcResult_TwoSlitting.ProcessName = _processName;
                UcResult_TwoSlitting.EquipmentCode = _equipmentCode;
                UcResult_TwoSlitting.EquipmentName = _equipmentName;
                UcResult_TwoSlitting.DvProductLot = _dvProductLot;
                UcResult_TwoSlitting.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_TwoSlitting.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                UcResult_TwoSlitting.IsRollMapEquipment = _isRollMapEquipment;      // RollMap 적용여부
                UcResult_TwoSlitting.IsRollMapLot = _isRollMapLot;                 // RollMap Lot 여부
                SetUserControlEquipmentDataTable();
                UcResult_TwoSlitting.SelectProductionResult();
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else if (_processCode == Process.ROLL_PRESSING)
            {
                UcResult_RollPressing.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_RollPressing.ProcessCode = _processCode;
                UcResult_RollPressing.ProcessName = _processName;
                UcResult_RollPressing.EquipmentCode = _equipmentCode;
                UcResult_RollPressing.EquipmentName = _equipmentName;
                UcResult_RollPressing.DvProductLot = _dvProductLot;
                UcResult_RollPressing.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_RollPressing.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                UcResult_RollPressing.IsRollMapEquipment = _isRollMapEquipment;      // RollMap 적용여부
                UcResult_RollPressing.IsRollMapLot = _isRollMapLot;      // RollMap Lot 여부
                SetUserControlEquipmentDataTable();
                UcResult_RollPressing.SelectProductionResult();
            }
            else if (_processCode == Process.REWINDER)
            {
                UcResult_ReWinder.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_ReWinder.ProcessCode = _processCode;
                UcResult_ReWinder.ProcessName = _processName;
                UcResult_ReWinder.EquipmentCode = _equipmentCode;
                UcResult_ReWinder.EquipmentName = _equipmentName;
                UcResult_ReWinder.DvProductLot = _dvProductLot;
                UcResult_ReWinder.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_ReWinder.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                SetUserControlEquipmentDataTable();
                UcResult_ReWinder.SelectProductionResult();
            }
            else if (_processCode == Process.TAPING)
            {
                UcResult_Taping.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_Taping.ProcessCode = _processCode;
                UcResult_Taping.ProcessName = _processName;
                UcResult_Taping.EquipmentCode = _equipmentCode;
                UcResult_Taping.EquipmentName = _equipmentName;
                UcResult_Taping.DvProductLot = _dvProductLot;
                UcResult_Taping.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_Taping.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                SetUserControlEquipmentDataTable();
                UcResult_Taping.SelectProductionResult();
            }
            else if (_processCode == Process.HEAT_TREATMENT)
            {
                UcResult_HeatTreatment.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_HeatTreatment.ProcessCode = _processCode;
                UcResult_HeatTreatment.ProcessName = _processName;
                UcResult_HeatTreatment.EquipmentCode = _equipmentCode;
                UcResult_HeatTreatment.EquipmentName = _equipmentName;
                UcResult_HeatTreatment.DvProductLot = _dvProductLot;
                UcResult_HeatTreatment.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcResult_HeatTreatment.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                SetUserControlEquipmentDataTable();
                UcResult_HeatTreatment.SelectProductionResult();
            }
            else if (_reWindingProcess == "Y")
            {
                UcResult_ReWinding.EquipmentSegmentCode = _equipmentSegmentCode;
                UcResult_ReWinding.ProcessCode = _processCode;
                UcResult_ReWinding.ProcessName = _processName;
                UcResult_ReWinding.EquipmentCode = _equipmentCode;
                UcResult_ReWinding.EquipmentName = _equipmentName;
                UcResult_ReWinding.DvProductLot = _dvProductLot;
                UcResult_ReWinding.IsRollMapEquipment = _isRollMapEquipment;     // RollMap 적용여부
                UcResult_ReWinding.IsRollMapLot = _isRollMapLot;     // RollMap Lot 여부
                SetUserControlEquipmentDataTable();
                UcResult_ReWinding.SelectProductionResult();
            }
        }

        public void SetUserControlEquipmentDataTable()
        {
            //// 선택 설비가 없다면 return
            //if (string.IsNullOrWhiteSpace(_equipmentCode)) return;

            DataTable dt = new DataTable();
            DataTable dtWO = new DataTable();

            if (DgEquipment.ItemsSource == null || DgEquipment.GetRowCount() == 0 || _reWindingProcess == "Y")
            {
                dt = null;
                dtWO = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_equipmentCode))
                    dt = null;
                else
                    dt = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "'").CopyToDataTable();

                dtWO = !DataTableConverter.Convert(DgEquipment.ItemsSource).Select("SEQ = 11").Any() ? null : DataTableConverter.Convert(DgEquipment.ItemsSource).Select("SEQ = 11").CopyToDataTable();
            }

            UcElectrodeProductLot.DtEquipment = dtWO;

            if (_processCode == Process.MIXING ||
                _processCode == Process.PRE_MIXING ||
                _processCode == Process.BS ||
                _processCode == Process.CMC ||
                _processCode == Process.InsulationMixing ||
                _processCode == Process.DAM_MIXING)
            {
                UcResult_Mixing.DtEquipment = dt;
            }
            else if (_processCode == Process.COATING)
            {
                UcResult_CoatingAuto.DtEquipment = dt;
            }
            else if (_processCode == Process.INS_COATING)
            {
                UcResult_InsCoating.DtEquipment = dt;
            }
            else if (_processCode == Process.HALF_SLITTING)
            {
                UcResult_HalfSlitting.DtEquipment = dt;
            }
            else if (_processCode == Process.SLITTING)
            {
                UcResult_Slitting.DtEquipment = dt;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 START
            else if (_processCode == Process.TWO_SLITTING)
            {
                UcResult_TwoSlitting.DtEquipment = dt;
            }
            //20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else if (_processCode == Process.ROLL_PRESSING)
            {
                UcResult_RollPressing.DtEquipment = dt;
            }
            else if (_processCode == Process.REWINDER)
            {
                UcResult_ReWinder.DtEquipment = dt;
            }
            else if (_processCode == Process.TAPING)
            {
                UcResult_Taping.DtEquipment = dt;
            }
            else if (_processCode == Process.HEAT_TREATMENT)
            {
                UcResult_HeatTreatment.DtEquipment = dt;
            }
            else if (_reWindingProcess == "Y")
            {
                UcResult_ReWinding.DtEquipment = dt;
            }

            //// ROLL_PRESSING 공정 대기 체크시
            //if (_processCode == Process.ROLL_PRESSING && UcElectrodeProductLot.chkWait.IsChecked == true)
            //{
            //    UcElectrodeProductLot.OnCheckBoxChecked(null, null);
            //}

        }

        public void SetProductLotSelect(RadioButton rb)
        {
            ///////////////////////////////////////////////////////// 설비 Tree 선택시 Return 
            if (_isEquipmentClick)
            {
                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                _isEquipmentClick = false;
                return;
            }
            /////////////////////////////////////////////////////////

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

            for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                else
                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
            }

            // row 색 바꾸기
            DgProductLot.SelectedIndex = idx;

            ////////////////////////////////////////////////////////////////////////////////
            _dvProductLot = DgProductLot.Rows[idx].DataItem as DataRowView;

            if (_dvProductLot == null) return;

            //// Re-Winding 공정은 리스트에 설비가 없다.
            //if (_reWindingProcess != "Y")
            //{
            if (_reWindingProcess != "Y" && _dvProductLot["WIPSTAT"].ToString() == Wip_State.WAIT)
                SelectEquipment(_equipmentCode, _equipmentName);
            else
                SelectEquipment(_dvProductLot["EQPTID"].ToString(), _dvProductLot["EQPTNAME"].ToString());
            //}
            SetRollMapEquipment();
            SelectProductLot(_dvProductLot["LOTID"].ToString());

        }

        private void SetEquipment(string sSelect, bool bProductList = true)
        {
            // 생산 Lot List가 Visible인 경우만 조회(실적 입력시는 설비만 재조회 한다.)
            if (grdProduct.Visibility == Visibility.Collapsed)
            {
                if (cboEquipment.SelectedItems.Count() > 0)
                {
                    UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
                return;
            }

            switch (sSelect)
            {
                ////////////////////////////////// 조회, 설비 콤보 변경
                case "L":
                case "C":
                    if (string.IsNullOrWhiteSpace(cboEquipment.SelectedItemsToString))
                    {
                        SetControlClear(true);

                        SetUserControlEquipment();
                        SetUserControlProductLot();
                        // 설비 Setting
                        SetUserControlEquipmentDataTable();

                        // Product Lot 조회
                        if (bProductList)
                            UcElectrodeProductLot.SelectProductList(null, _reWindingProcess);

                        break;
                    }

                    // 설비 Tree 조회
                    SetEquipmentTree();

                    ////////////////////////
                    if (cboEquipment.SelectedItems.Count() == 1)
                    {
                        DataTable dt = ((System.Data.DataView)cboEquipment.ItemsSource).Table;
                        DataRow[] dr = dt.Select("CBO_CODE ='" + cboEquipment.SelectedItemsToString + "'");

                        SelectEquipment(cboEquipment.SelectedItemsToString, dr.Length == 0 ? null : dr[0]["CBO_NAME"].ToString());
                    }
                    else
                    {
                        SelectEquipment(cboEquipment.SelectedItemsToString, null);
                    }

                    // Product Lot Setting
                    SetProductLotList();

                    break;
                ///////////////////////////////// 설비 Tree Click
                case "T":
                    _equipmentCode = _treeEquipmentCode;
                    _equipmentName = _treeEquipmentName;

                    SelectEquipment(_equipmentCode, _equipmentName);

                    if (_dvProductLot == null || _dvProductLot["WIPSTAT"].ToString() != Wip_State.WAIT)
                        SelectProductLot(string.Empty);

                    break;

                // Product Lot Click
                case "P":
                    break;
            }

        }

        private void SetEquipmentTree()
        {
            // Clear
            SetControlClear();

            if (cboEquipment.SelectedItems.Count() > 0)
            {
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        private void SetProductLotList(string LotID = null)
        {
            UcElectrodeProductLot.SetControlVisibility();

            if (cboEquipment.SelectedItems.Count() > 0)
            {
                SetUserControlProductLot();
                UcElectrodeProductLot.SelectProductList(LotID, _reWindingProcess);
            }
        }

        private void SelectEquipment(string EqptID, string EqptName)
        {
            if (EqptID.Split(',').Length == 1)
            {
                _equipmentCode = EqptID;
                _equipmentName = EqptName;
                UcElectrodeProductLot.txtSelectEquipment.Text = _equipmentCode;
                UcElectrodeProductLot.txtSelectEquipmentName.Text = _equipmentName;

                // 무지부 방향 조회(동별 공통 : MNG_SLITTING_SIDE_AREA)
                if (UcElectrodeCommand.IsManageSlittingSide)
                {
                    GetWorkHalfSlittingSide();
                }
            }
            else
            {
                _equipmentCode = string.Empty;
                _equipmentName = string.Empty;
                UcElectrodeProductLot.txtSelectEquipment.Text = _equipmentCode;
                UcElectrodeProductLot.txtSelectEquipmentName.Text = _equipmentName;
                //HideAllRackRateMode();
            }

            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SelectProductLot(string LotID)
        {
            if (string.IsNullOrWhiteSpace(LotID))
            {
                _dvProductLot = null;
                _isLastBatch = false;
                _isPostingHold = string.Empty;
            }

            _productLot = LotID;

            UcElectrodeProductLot.txtSelectLot.Text = _productLot;

            SetRollMapLotAttribute(_productLot);
            SelectRollMapLot();


            //RollMap Button Enabled
            if (string.Equals(_processCode, Process.ROLL_PRESSING))
            {
                if (!(_dvProductLot == null) && (string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.EQPT_END) || string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.PROC)))
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = true;
                }
                else
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = false;
                }
            }
            else if (string.Equals(_processCode, Process.COATING))
            {
                if (!(_dvProductLot == null) && (string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.EQPT_END) || string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.END)))
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = true;
                }
                else
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = false;
                }
            }
            else if (string.Equals(_processCode, Process.REWINDING) || string.Equals(_processCode, Process.SLIT_REWINDING))
            {
                if (!(_dvProductLot == null) && (string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.EQPT_END) || string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.END)))
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = true;
                }
                else
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = false;
                }
            }
            else if (string.Equals(_processCode, Process.TWO_SLITTING) || string.Equals(_processCode, Process.SLITTING))
            {
                if (!(_dvProductLot == null) && (string.Equals(Util.NVC(_dvProductLot["WIPSTAT"]), Wip_State.EQPT_END)))
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = true;
                }
                else
                {
                    UcElectrodeCommand.btnRollMap.IsEnabled = false;
                }
            }

        }

        private void LabelAuto()
        {
            int iSamplingCount;

            if (LoginInfo.CFG_LABEL_AUTO.Equals("Y"))
            {
                //int iSamplingCount;

                // 2022.1.7. ESNJ 1동 생산시스템 구축. 실적 완료 후 Label 자동 출력 시 슬리터의 경우 기존 슬리터 공정진척과 동일하게 수정. 다른공정은 기존 로직 유지.
                //if (string.Equals(_processCode, Process.SLITTING) || string.Equals(_processCode, Process.TWO_SLITTING))
                if (string.Equals(_processCode, Process.SLITTING) || string.Equals(_processCode, "E4200"))
                {
                    DataTable LabelDT = null;

                    if (string.Equals(_processCode, Process.SLITTING))
                        LabelDT = DataTableConverter.Convert(UcResult_Slitting.dgProductResult.ItemsSource);
                    //else
                    //    LabelDT = DataTableConverter.Convert(UcResult_TwoSlitting.dgProductResult.ItemsSource);

                    DataTable sampleDT = new DataTable();
                    sampleDT.Columns.Add("CUT_ID", typeof(string));
                    sampleDT.Columns.Add("LOTID", typeof(string));
                    sampleDT.Columns.Add("COMPANY", typeof(string));
                    DataRow dRow = null;

                    foreach (DataRow _iRow in LabelDT.Rows)
                    {
                        iSamplingCount = 0;
                        string[] sCompany = null;
                        foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_iRow["LOTID"])))
                        {
                            iSamplingCount = Util.NVC_Int(items.Key);
                            sCompany = Util.NVC(items.Value).Split(',');
                        }
                        for (int i = 0; i < iSamplingCount; i++)
                        {
                            dRow = sampleDT.NewRow();
                            dRow["CUT_ID"] = _iRow["CUT_ID"];
                            dRow["LOTID"] = _iRow["LOTID"];
                            dRow["COMPANY"] = i > sCompany.Length - 1 ? "" : sCompany[i];
                            sampleDT.Rows.Add(dRow);
                        }
                    }

                    var sortdt = sampleDT.AsEnumerable().OrderBy(x => x.Field<string>("CUT_ID") + x.Field<string>("COMPANY")).CopyToDataTable();

                    for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                        for (int i = 0; i < sortdt.Rows.Count; i++)
                            Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(sortdt.DefaultView[i]["LOTID"]), _processCode, Util.NVC(sortdt.DefaultView[i]["COMPANY"]));
                }
                else
                {
                    for (int ii = 0; ii < LoginInfo.CFG_LABEL_COPIES; ii++)
                    {
                        iSamplingCount = 1;
                        string[] sCompany = null;

                        foreach (KeyValuePair<int, string> items in getSamplingLabelInfo(Util.NVC(_dvProductLot["LOTID"])))
                        {
                            iSamplingCount = Util.NVC_Int(items.Key);
                            sCompany = Util.NVC(items.Value).Split(',');
                        }

                        for (int i = 0; i < iSamplingCount; i++)
                            // 2022.1.7. ESNJ 1동 생산시스템 구축. Process가 Coating으로 고정되어 있어 수정
                            //Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_dvProductLot["LOTID"]), Process.COATING, i > sCompany.Length - 1 ? "" : sCompany[i]);
                            Util.PrintLabel_Elec(FrameOperation, loadingIndicator, Util.NVC(_dvProductLot["LOTID"]), _processCode, i > sCompany.Length - 1 ? "" : sCompany[i]);
                    }
                }
            }

            if (_processCode != Process.PRE_MIXING)
            {
                // 2022.1.7. ESNJ 1동 생산시스템 구축. 실적 완료 후 ProductLot에 Lot 정보가 사라져 전극 이력카드 출력이 되지 않는 오류 수정  
                _txtEndLotId = string.Empty;
                _txtEndLotCutId = string.Empty;
                _txtEndLotId = Util.NVC(_dvProductLot["LOTID"].ToString());
                _txtEndLotCutId = Util.NVC(_dvProductLot["CUT_ID"].ToString());

                // CSR : [C20220812-000226] COATER popup alarm improvement
                // Label MessageBox 정상처리 클릭 후 실행해야 함
                if (string.Equals(_processCode, Process.COATING))
                {
                    dtSlurry = null;
                    dtSlurry = GetSlurryInfo(Util.NVC(_dvProductLot["LOTID"].ToString()));
                }

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        if (string.Equals(LoginInfo.CFG_CARD_POPUP, "Y") || string.Equals(LoginInfo.CFG_CARD_AUTO, "Y"))
                            PopupPrintHistoryCard();
                        else
                        {
                            // CSR : [C20220812-000226] COATER popup alarm improvement
                            // Slurry Popup실행
                            if (_util.IsCommonCodeUse("COATER_SLURRY_POPUP", LoginInfo.CFG_AREA_ID) && string.Equals(_processCode, Process.COATING))
                            {
                                if (dtSlurry != null && dtSlurry.Rows.Count > 0)
                                    SetCoaterSlurryPopup(dtSlurry);
                            }
                        }
                    }
                });
            }
        }

        private string SetConfirmTopInfoMixing()
        {
            decimal prodQty = Util.NVC_Decimal(UcResult_Mixing.txtProductionQty.Value);
            decimal eqptEndQty = Util.NVC_Decimal(UcResult_Mixing.txtEquipmentQty.Value);

            if (Math.Abs((prodQty - eqptEndQty)) >= 30)
                return MessageDic.Instance.GetMessage("SFU3468", UcResult_Mixing.txtUnit.Text);  // 설비 양품량과 생산량 차이가 30M이상입니다.\n실물 수량이 정말 맞습니까?

            return string.Empty;
        }

        private string SetConfirmTopInfo(C1DataGrid dg, string sUnit)
        {
            decimal prodQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[dg.TopRows.Count].DataItem, "INPUTQTY"));
            decimal eqptEndQty = Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[dg.TopRows.Count].DataItem, "EQPT_END_QTY"));

            if (Math.Abs((prodQty - eqptEndQty)) >= 30)
                return MessageDic.Instance.GetMessage("SFU3468", sUnit);  // 설비 양품량과 생산량 차이가 30%1이상입니다.\n실물 수량이 정말 맞습니까?

            return string.Empty;
        }

        /// <summary>
        /// 롤프레스 설비완공:소진완료->실적확정:잔량처리 시 validation
        /// 공통코드 : CST_EMPTY_CUT_YN_AREA_RP 사용중인 동에서만 시행함
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="sUnit"></param>
        /// <returns></returns>
        private string SetConfirmRsnQty(C1DataGrid dg, double dRemainQty)
        {
            if (!_util.IsCommonCodeUse("CST_EMPTY_CUT_YN_AREA_RP", LoginInfo.CFG_AREA_ID))
                return string.Empty;

            string sInputCstID = DataTableConverter.GetValue(dg.Rows[dg.TopRows.Count].DataItem, "CSTID").Nvc(); //투입 보빈ID

            #region [설비완공:소진완료 후 실적확정:잔량배출 시 처리]

            // 소진완료->잔량배출 변경 시 해당 Lot 마지막으로 사용된 보빈을 자동으로 매핑해주지 못하는 이유는
            // 설비완공-실적확정까지 시간이 소요되어 자동물류로 STK 입고되었을 경우 MES상에서 LOT-CST 매핑을 하면 물류관리 정보와 충돌이 일어날 수 있기 때문에
            // 사용자 실물 확인 후 수동으로 연계하는것으로 프로세스 정의함
            if (string.IsNullOrEmpty(sInputCstID) && dRemainQty > 0)
                return MessageDic.Instance.GetMessage("SFU3817");  // 설비완공 시 투입Lot 소진완료되어 보빈 연계해제된 Lot입니다.\r\n잔량배출 할 경우 투입Lot에 보빈을 수동연계해야합니다.\r\n(특이작업 >> Carrior-Lot 정보관리)

            #endregion

            return string.Empty;
        }

        private Dictionary<string, string> GetRemarkConvert(C1DataGrid dg)
        {
            Dictionary<string, string> remarkInfo = new Dictionary<string, string>();
            if (dg.Rows.Count > 0)
            {
                System.Text.StringBuilder sRemark = new System.Text.StringBuilder();
                for (int i = 1; i < dg.Rows.Count; i++)
                {
                    sRemark.Clear();

                    // 1. 특이사항
                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 2. 공통특이사항
                    //if (_processCode == Process.HALF_SLITTING || _processCode == Process.SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
                    if (_processCode == Process.HALF_SLITTING || _processCode == Process.SLITTING || _processCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
                        sRemark.Append(Util.NVC(DataTableConverter.GetValue(dg.Rows[0].DataItem, "REMARK")));
                    sRemark.Append("|");

                    // 롤맵 Hold 기능 추가 Start
                    if (Process.COATING.Equals(_processCode) || Process.ROLL_PRESSING.Equals(_processCode))
                    {
                        if (!holdLotClassCode.IsNullOrEmpty() && holdLotClassCode.Count > 0)
                        {
                            string lotId = _dvProductLot["LOTID"].ToString();
                            string holdClassCode = holdLotClassCode[lotId];
                            string holdMessage = string.Empty;

                            holdMessage = GetMessageWithSubstitution(holdClassCode.Trim(), lotId);
                            sRemark.Append(holdMessage);
                            sRemark.Append("|");
                        }
                    }

                    // 롤맵 Hold 기능 추가 End

                    // 3. 조정횟수
                    if (_processCode == Process.COATING)
                    {
                        C1DataGrid wipReason_T = UcResult_CoatingAuto.dgWipReasonTop;
                        C1DataGrid wipReason_B = UcResult_CoatingAuto.dgWipReasonBack;

                        if (wipReason_T.Visibility == Visibility.Visible && wipReason_T.Columns["COUNTQTY"] != null)
                            for (int j = 0; j < wipReason_T.Rows.Count; j++)
                                if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(wipReason_T.Rows[j].DataItem, "COUNTQTY"))) &&
                                        Util.NVC_Decimal(DataTableConverter.GetValue(wipReason_T.Rows[j].DataItem, "COUNTQTY")) > 0)
                                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(wipReason_T.Rows[j].DataItem, "RESNNAME")) + " : " +
                                        Util.NVC(DataTableConverter.GetValue(wipReason_T.Rows[j].DataItem, "COUNTQTY")) + ",");

                        if (wipReason_B.Visibility == Visibility.Visible && wipReason_B.Columns["COUNTQTY"] != null)
                            for (int j = 0; j < wipReason_B.Rows.Count; j++)
                                if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(wipReason_B.Rows[j].DataItem, "COUNTQTY"))) &&
                                        Util.NVC_Decimal(DataTableConverter.GetValue(wipReason_B.Rows[j].DataItem, "COUNTQTY")) > 0)
                                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(wipReason_B.Rows[j].DataItem, "RESNNAME")) + " : " +
                                        Util.NVC(DataTableConverter.GetValue(wipReason_B.Rows[j].DataItem, "COUNTQTY")) + ",");
                    }
                    else
                    {
                        C1DataGrid wipReason;

                        if (_processCode == Process.MIXING ||
                            _processCode == Process.PRE_MIXING ||
                            _processCode == Process.BS ||
                            _processCode == Process.CMC ||
                            _processCode == Process.InsulationMixing ||
                            _processCode == Process.DAM_MIXING)
                        {
                            wipReason = UcResult_Mixing.dgWipReason;
                        }
                        else if (_processCode == Process.HALF_SLITTING)
                        {
                            wipReason = UcResult_HalfSlitting.dgWipReason;
                        }
                        else if (_processCode == Process.ROLL_PRESSING)
                        {
                            wipReason = UcResult_RollPressing.dgWipReason;
                        }
                        else if (_processCode == Process.INS_COATING)
                        {
                            wipReason = UcResult_InsCoating.dgWipReasonTop;
                        }
                        else if (_processCode == Process.SLITTING)
                        {
                            wipReason = UcResult_Slitting.dgWipReason;
                        }
                        //20211215 2차 Slitting 공정진척DRB 화면 개발 START
                        else if (_processCode == Process.TWO_SLITTING)
                        {
                            wipReason = UcResult_TwoSlitting.dgWipReason;
                        }
                        //20211215 2차 Slitting 공정진척DRB 화면 개발 END
                        else if (_processCode == Process.REWINDER)
                        {
                            wipReason = UcResult_ReWinder.dgWipReason;
                        }
                        else if (_processCode == Process.TAPING)
                        {
                            wipReason = UcResult_Taping.dgWipReason;
                        }
                        else if (_processCode == Process.HEAT_TREATMENT)
                        {
                            wipReason = UcResult_HeatTreatment.dgWipReason;
                        }
                        else //if (_reWindingProcess == "Y")
                        {
                            wipReason = UcResult_ReWinding.dgWipReason;
                        }

                        if (wipReason.Visibility == Visibility.Visible && wipReason.Columns["COUNTQTY"] != null)
                            for (int j = 0; j < wipReason.Rows.Count; j++)
                                if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(wipReason.Rows[j].DataItem, "COUNTQTY"))) &&
                                        Util.NVC_Decimal(DataTableConverter.GetValue(wipReason.Rows[j].DataItem, "COUNTQTY")) > 0)
                                    sRemark.Append(Util.NVC(DataTableConverter.GetValue(wipReason.Rows[j].DataItem, "RESNNAME")) + " : " +
                                        Util.NVC(DataTableConverter.GetValue(wipReason.Rows[j].DataItem, "COUNTQTY")) + ",");

                    }

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);

                    sRemark.Append("|");

                    // 4. 압연횟수
                    if (string.Equals(_processCode, Process.ROLL_PRESSING))
                        sRemark.Append(_dvProductLot["ROLLPRESS_SEQNO"].ToString());
                    sRemark.Append("|");

                    // 5.색지정보
                    if (string.Equals(_processCode, Process.ROLL_PRESSING))
                    {
                        for (int j = 0; j < UcResult_RollPressing.dgColor.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(UcResult_RollPressing.dgColor.Rows[j].DataItem, "CLCTVAL01"))) &&
                                Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_RollPressing.dgColor.Rows[j].DataItem, "CLCTVAL01")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(UcResult_RollPressing.dgColor.Rows[j].DataItem, "CLCTNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(UcResult_RollPressing.dgColor.Rows[j].DataItem, "CLCTVAL01")) + ",");
                    }
                    else if (string.Equals(_processCode, Process.TAPING))
                    {
                        for (int j = 0; j < UcResult_Taping.dgColor.Rows.Count; j++)
                            if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(UcResult_Taping.dgColor.Rows[j].DataItem, "CLCTVAL01"))) &&
                                Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_Taping.dgColor.Rows[j].DataItem, "CLCTVAL01")) > 0)
                                sRemark.Append(Util.NVC(DataTableConverter.GetValue(UcResult_Taping.dgColor.Rows[j].DataItem, "CLCTNAME")) + " : " +
                                    Util.NVC(DataTableConverter.GetValue(UcResult_Taping.dgColor.Rows[j].DataItem, "CLCTVAL01")) + ",");
                    }

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);
                    sRemark.Append("|");

                    // 6.합권이력
                    if (string.Equals(_processCode, Process.HALF_SLITTING))
                    {
                        DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(UcResult_HalfSlitting.dgRemark.Rows[i].DataItem, "LOTID")));
                        if (mergeInfo.Rows.Count > 0)
                            foreach (DataRow row in mergeInfo.Rows)
                                sRemark.Append(Util.NVC(row["LOTID"]) + " : " + UcResult_HalfSlitting.GetUnitFormatted(row["LOT_QTY"].ToString()) + UcResult_HalfSlitting.txtUnit.Text + ",");
                    }
                    else if (string.Equals(_processCode, Process.ROLL_PRESSING))
                    {
                        DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(UcResult_RollPressing.dgRemark.Rows[i].DataItem, "LOTID")));
                        if (mergeInfo.Rows.Count > 0)
                            foreach (DataRow row in mergeInfo.Rows)
                                sRemark.Append(Util.NVC(row["LOTID"]) + " : " + UcResult_RollPressing.GetUnitFormatted(row["LOT_QTY"].ToString()) + UcResult_RollPressing.txtUnit.Text + ",");
                    }
                    else if (string.Equals(_processCode, Process.SLITTING))
                    {
                        DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(UcResult_Slitting.dgRemark.Rows[i].DataItem, "LOTID")));
                        if (mergeInfo.Rows.Count > 0)
                            foreach (DataRow row in mergeInfo.Rows)
                                sRemark.Append(Util.NVC(row["LOTID"]) + " : " + UcResult_Slitting.GetUnitFormatted(row["LOT_QTY"].ToString()) + UcResult_Slitting.txtUnit.Text + ",");
                    }
                    //20211215 2차 Slitting 공정진척DRB 화면 개발 START
                    else if (string.Equals(_processCode, Process.TWO_SLITTING))
                    {
                        DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(UcResult_TwoSlitting.dgRemark.Rows[i].DataItem, "LOTID")));
                        if (mergeInfo.Rows.Count > 0)
                            foreach (DataRow row in mergeInfo.Rows)
                                sRemark.Append(Util.NVC(row["LOTID"]) + " : " + UcResult_TwoSlitting.GetUnitFormatted(row["LOT_QTY"].ToString()) + UcResult_TwoSlitting.txtUnit.Text + ",");
                    }
                    //20211215 2차 Slitting 공정진척DRB 화면 개발 END
                    else if (string.Equals(_processCode, Process.REWINDER))
                    {
                        DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(UcResult_ReWinder.dgRemark.Rows[i].DataItem, "LOTID")));
                        if (mergeInfo.Rows.Count > 0)
                            foreach (DataRow row in mergeInfo.Rows)
                                sRemark.Append(Util.NVC(row["LOTID"]) + " : " + UcResult_ReWinder.GetUnitFormatted(row["LOT_QTY"].ToString()) + UcResult_ReWinder.txtUnit.Text + ",");
                    }
                    else if (string.Equals(_processCode, Process.TAPING))
                    {
                        DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(UcResult_Taping.dgRemark.Rows[i].DataItem, "LOTID")));
                        if (mergeInfo.Rows.Count > 0)
                            foreach (DataRow row in mergeInfo.Rows)
                                sRemark.Append(Util.NVC(row["LOTID"]) + " : " + UcResult_Taping.GetUnitFormatted(row["LOT_QTY"].ToString()) + UcResult_Taping.txtUnit.Text + ",");
                    }
                    else if (string.Equals(_processCode, Process.HEAT_TREATMENT))
                    {
                        DataTable mergeInfo = GetMergeInfo(Util.NVC(DataTableConverter.GetValue(UcResult_HeatTreatment.dgRemark.Rows[i].DataItem, "LOTID")));
                        if (mergeInfo.Rows.Count > 0)
                            foreach (DataRow row in mergeInfo.Rows)
                                sRemark.Append(Util.NVC(row["LOTID"]) + " : " + UcResult_HeatTreatment.GetUnitFormatted(row["LOT_QTY"].ToString()) + UcResult_HeatTreatment.txtUnit.Text + ",");
                    }

                    if (string.Equals(sRemark.ToString().Substring(sRemark.ToString().Length - 1, 1), ","))
                        sRemark.Remove(sRemark.ToString().Length - 1, 1);

                    remarkInfo.Add(Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID")), sRemark.ToString());
                }
            }
            return remarkInfo;
        }

        public void OutSideProductLotList(string LotID)
        {
            ButtonProductList_Click(null, null);

            // 생산 Lot 재조회
            SelectProductLot(LotID);
            SetProductLotList(LotID);
        }

        public void SetInputUseAuthority(string GrpCode)
        {
            if (UcElectrodeEquipment == null)
                return;

            UcElectrodeEquipment.SetPermissionPerButton(GrpCode);
        }

        public void Get(string LotID)
        {
            ButtonProductList_Click(null, null);

            // 생산 Lot 재조회
            SelectProductLot(LotID);
            SetProductLotList(LotID);
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

        private void CheckUsePolarity(string processCode)
        {
            try
            {
                lblPolarity.Visibility = Visibility.Visible;
                cboPolarity.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("CMCDTYPE");
                inTable.Columns.Add("CMCODE");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "ELTR_TYPE_USE_PROCESS_SKIP";
                newRow["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable rslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

                if (rslt != null && rslt.Rows.Count > 0)
                {
                    if (Util.NVC(rslt.Rows[0]["ATTRIBUTE1"]).IndexOf(processCode) > -1)
                    {
                        lblPolarity.Visibility = Visibility.Collapsed;
                        cboPolarity.Visibility = Visibility.Collapsed;
                        cboPolarity.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        #region -----------------------------RollMap Func

        /// <summary>
        /// Version Info
        /// </summary>
        private string GetVersion()
        {
            string strVersion = string.Empty;
            strVersion = Util.NVC(_dvProductLot["PROD_VER_CODE"]);

            // Version check
            if (_processCode == Process.COATING)
            {
                if (string.IsNullOrEmpty(strVersion) && string.IsNullOrEmpty(UcResult_CoatingAuto.txtVersion.Text.Trim()))
                {
                    Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                    return string.Empty;
                }
                else
                {
                    // Product Version Change Case
                    if (!string.IsNullOrEmpty(UcResult_CoatingAuto.txtVersion.Text.Trim())
                        && !string.Equals(strVersion, Util.NVC(UcResult_CoatingAuto.txtVersion.Text.Trim())))
                    {
                        strVersion = UcResult_CoatingAuto.txtVersion.Text.Trim();
                    }
                }
            }
            else if (_processCode == Process.ROLL_PRESSING)
            {
                if (string.IsNullOrEmpty(strVersion) && string.IsNullOrEmpty(UcResult_RollPressing.txtVersion.Text.Trim()))
                {
                    Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                    return string.Empty;
                }
                else
                {
                    // Product Version Change Case
                    if (!string.IsNullOrEmpty(UcResult_RollPressing.txtVersion.Text.Trim())
                        && !string.Equals(strVersion, Util.NVC(UcResult_RollPressing.txtVersion.Text.Trim())))
                    {
                        strVersion = UcResult_RollPressing.txtVersion.Text.Trim();
                    }
                }
            }
            else if (_processCode == Process.REWINDING || _processCode == Process.SLIT_REWINDING)
            {
                if (string.IsNullOrEmpty(strVersion) && string.IsNullOrEmpty(UcResult_ReWinding.txtVersion.Text.Trim()))
                {
                    Util.MessageValidation("SFU1218");  //Version 정보를 입력 하세요.
                    return string.Empty;
                }
                else
                {
                    // Product Version Change Case
                    if (!string.IsNullOrEmpty(UcResult_ReWinding.txtVersion.Text.Trim())
                        && !string.Equals(strVersion, Util.NVC(UcResult_ReWinding.txtVersion.Text.Trim())))
                    {
                        strVersion = UcResult_ReWinding.txtVersion.Text.Trim();
                    }
                }
            }

            return strVersion;
        }

        /// <summary>
        /// Lane Qty
        /// </summary>
        private int GetLaneQty()
        {
            int intLaneQty = 0;
            // Version check
            if (_processCode == Process.COATING)
            {
                intLaneQty = Util.NVC_Int(UcResult_CoatingAuto.txtLaneQty.Value);
            }
            else if (_processCode == Process.INS_COATING)
            {
                intLaneQty = Util.NVC_Int(UcResult_InsCoating.txtLaneQty.Value);
            }
            else if (_processCode == Process.HALF_SLITTING)
            {
                intLaneQty = Util.NVC_Int(UcResult_HalfSlitting.txtLaneQty.Value);
            }
            else if (_processCode == Process.SLITTING)
            {
                intLaneQty = Util.NVC_Int(UcResult_Slitting.txtLaneQty.Value);
            }
            else if (_processCode == Process.ROLL_PRESSING)
            {
                intLaneQty = Util.NVC_Int(UcResult_RollPressing.txtLaneQty.Value);
            }
            else if (_processCode == Process.REWINDER)
            {
                intLaneQty = Util.NVC_Int(UcResult_ReWinder.txtLaneQty.Value);
            }

            return intLaneQty;
        }

        /// <summary>
        /// RollMap Visible 
        /// </summary>
        private void VisibleRollMapMode()
        {
            if (_isRollMapEquipment)
            {
                UcElectrodeCommand.btnRollMap.Visibility = Visibility.Visible;
                if (string.Equals(_processCode, Process.COATING))
                {
                    UcElectrodeCommand.btnRollMapInputMaterial.Visibility = Visibility.Visible; // 2021.08.10 Visible 처리
                }
            }
            else
            {
                UcElectrodeCommand.btnRollMap.Visibility = Visibility.Collapsed;
                UcElectrodeCommand.btnRollMapInputMaterial.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// RollMap 대상장비 설정
        /// </summary>
        private void SetRollMapEquipment()
        {
            _isRollMapResultLink = IsRollMapResultApply();
            _isRollMapEquipment = IsEquipmentAttr(_equipmentCode);
            _isOriginRollMapEquipment = _isRollMapEquipment;
            _isRollMapDivReportEqpt = SelectCheckDivReportEqpt();

            // # Roll Map 대상설비에 따른 컨트롤 정의
            VisibleRollMapMode();

            // 롤맵 설비는 롤 방향 Tab 안보이도록 해달라는 요청 반영 [2021-10-18]
            //if (isRollMapEquipment)
            //    (grdDataCollect.Children[0] as UcDataCollect).tiRollDirection.Visibility = Visibility.Collapsed;
            //else
            //    (grdDataCollect.Children[0] as UcDataCollect).tiRollDirection.Visibility = Visibility.Visible;
        }
        #endregion 

        #endregion

        #region =============================Mixing
        private void CheckSpecOutHold(Action callback)
        {
            try
            {
                //자동보류여부에 체크되어 있으면 무조건 홀드를 건다.
                if (!ValidQualitySpec("Hold"))
                {
                    //자동HOLD되도록 설정된 품질검사 결과가 기준치를 만족하지 못했습니다. 완성랏이 홀드됩니다. 계속하시겠습니까?
                    Util.MessageConfirm("SFU8185", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            callback();
                        }
                    });
                }
                else
                {
                    if (!ValidQualitySpecExists())
                    {
                        //LSL, USL 미설정되어 계속 진행할 경우 완성LOT이 HOLD처리 됩니다. 계속하시겠습니까?
                        Util.MessageConfirm("SFU8186", result =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                callback();
                            }
                        });
                    }
                    else
                    {
                        callback();
                    }
                }
            }
            catch (Exception ex)
            {
            }
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

        private string SetConfirmEqpLossMessage()
        {
            // 설비 LOSS 체크
            DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

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
                    return MessageDic.Instance.GetMessage("SFU3501", new object[] { sLossInfo });       // 미입력한 설비 Loss가 존재합니다. 확정하시겠습니까? %1
                }
            }

            return string.Empty;
        }

        private string SetConfirmQtyMessage()
        {
            DataTable dt = GetMixerPreData();
            if (dt.Rows.Count > 0)
            {
                decimal goodQty = Util.NVC_Decimal(UcResult_Mixing.txtGoodQty.Value);
                decimal mixOutQty = string.IsNullOrEmpty(Util.NVC(dt.Rows[0]["WIPQTY_ED"])) ? 0 : Util.NVC_Decimal(dt.Rows[0]["WIPQTY_ED"]);

                if (Math.Abs(goodQty - mixOutQty) > 10)
                {
                    return MessageDic.Instance.GetMessage("SFU3602", Util.NVC(dt.Rows[0]["LOTID"])); // 이전 확정 생산LOT[%1]의 수량보다 ±10KG 초과하였습니다.
                }
            }

            return string.Empty;
        }
        #endregion

        #region =============================Coating

        private decimal GetDiffWebBreakQty(C1DataGrid dataGrid, string sActId, string sResnPosition)
        {
            // 요청으로 0.5반영 로직 제거 (실적 전송 시만 TOP은 0.5로 변경)
            decimal dSumQty = 0;

            if (dataGrid.Rows.Count > 0)
                for (int i = 0; i < dataGrid.GetRowCount(); i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY"))))
                        if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RSLT_EXCL_FLAG")), "Y"))
                            if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "WEB_BREAK_FLAG"), "Y"))
                                if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "ACTID"), sActId))
                                    dSumQty += (Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "FRST_AUTO_RSLT_RESNQTY")) -
                                        Util.NVC_Decimal(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, "RESNQTY")));
            //* (string.Equals(sResnPosition, "TOP") ? 0.5M : 1M));
            return dSumQty;
        }

        #endregion

        #region =============================Roll Pressing
        #endregion

        #region =============================재와인딩
        #endregion

        #endregion

        #region[[Validation]
        #region =============================공통
        private bool ValidationCancel()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1938");     // 취소할 LOT을 선택하세요.
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() != Wip_State.PROC)
            {
                Util.MessageValidation("SFU2957");     // 진행중인 작업을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEqptEnd()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() != "PROC")
            {
                Util.MessageValidation("SFU2957");     // 진행중인 작업을 선택하세요.
                return false;
            }

            return true;
        }
        private bool ValidationEndCancel()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1938");     // 취소할 LOT을 선택하세요.
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() != Wip_State.END)
            {
                Util.MessageValidation("SFU5146", new object[] { _dvProductLot["LOTID"] });     // 해당 Lot[%1]은 실적확정 상태가 아니라 취소 할 수 없습니다.
                return false;
            }

            ///////////////////////////////////// 차후 권한 설정
            //if (_util.IsCommonCodeUse("ELEC_CNFM_CANCEL_USER", LoginInfo.USERID) == false)
            //{
            //    Util.MessageValidation("SFU5148", new object[] { LoginInfo.USERID });     // 해당 USER[%1]는 실적취소할 권한이 없습니다. (시스템 담당자에게 문의 바랍니다.)
            //    return false;
            //}

            return true;
        }

        private bool ValidQualityRequired(C1DataGrid dg)
        {
            if (dg.Visibility == Visibility.Visible && dg.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(dg.ItemsSource).DefaultView;
                view.RowFilter = "MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    bool isValid = false;
                    DataRow[] filterRows = DataTableConverter.Convert(dg.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if (!string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) && !string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString()))
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (isValid == false)
                    {
                        Util.MessageValidation("SFU3601", sItemName);     // 해당 품질정보[%1]는 필수값이기 때문에 한 항목이라도 측정값의 입력이 필요합니다.
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidQualitySpecRequired(C1DataGrid dg)
        {
            if (dg.Rows.Count > 0)
            {
                DataView view = DataTableConverter.Convert(dg.ItemsSource).DefaultView;
                view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
                DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

                foreach (DataRow row in dt.Rows)
                {
                    string sItemName = string.Empty;
                    DataRow[] filterRows = DataTableConverter.Convert(dg.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
                    foreach (DataRow subRow in filterRows)
                    {
                        sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
                        if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
                        {
                            Util.MessageValidation("SFU4985", sItemName);     // 해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
                            return false;
                        }
                    }
                }
            }

            //if (QUALITY2_GRID.Visibility == Visibility.Visible && QUALITY2_GRID.Rows.Count > 0)
            //{
            //    DataView view = DataTableConverter.Convert(QUALITY2_GRID.ItemsSource).DefaultView;
            //    view.RowFilter = "SPEC_USE_MAND_INSP_ITEM_FLAG = 'Y'";
            //    DataTable dt = view.ToTable(true, "INSP_ITEM_ID");

            //    foreach (DataRow row in dt.Rows)
            //    {
            //        string sItemName = string.Empty;
            //        string itemName = string.Empty;
            //        DataRow[] filterRows = DataTableConverter.Convert(QUALITY2_GRID.ItemsSource).Select("INSP_ITEM_ID = '" + Util.NVC(row["INSP_ITEM_ID"]) + "'");
            //        foreach (DataRow subRow in filterRows)
            //        {
            //            sItemName = Util.NVC(subRow["INSP_ITEM_NAME"]);
            //            if ((!string.IsNullOrEmpty(Util.NVC(subRow["USL"])) || !string.IsNullOrEmpty(Util.NVC(subRow["LSL"]))) && (string.IsNullOrEmpty(Util.NVC(subRow["CLCTVAL01"])) || string.Equals(Util.NVC(subRow["CLCTVAL01"]), Double.NaN.ToString())))
            //            {
            //                Util.MessageValidation("SFU4985", sItemName);     // 해당 품질정보[%1]는 상/하한 값이 존재하는 경우 측정값이 필수로 지정되어 있어 측정값 입력이 필요합니다.
            //                return false;
            //            }
            //        }
            //    }
            //}
            return true;
        }

        private bool ValidDataCollect(bool ChangeQuality, bool ChangeMaterial, bool ChangeRemark)
        {
            if (ChangeQuality)
            {
                Util.MessageValidation("SFU1999");     // 품질 정보를 저장하세요.
                return false;
            }

            // 변경 여부 체크 안함
            if (_processCode == Process.MIXING ||
                _processCode == Process.PRE_MIXING ||
                _processCode == Process.BS ||
                _processCode == Process.CMC ||
                _processCode == Process.InsulationMixing ||
                _processCode == Process.INS_COATING ||
                _processCode == Process.COATING ||
                _processCode == Process.DAM_MIXING)
            {
                if (ChangeMaterial)
                {
                    Util.MessageValidation("SFU1818");     // 자재 정보를 저장하세요.
                    return false;
                }
            }

            if (ChangeRemark)
            {
                Util.MessageValidation("SFU2977");     // 특이사항 정보를 저장하세요.
                return false;
            }

            return true;
        }

        private bool ValidationWebBreak()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationPilotProdMode()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            string sProcLotID = string.Empty;
            if (CheckProcWip(out sProcLotID))
            {
                Util.MessageValidation("SFU3199", sProcLotID); // 진행중인 LOT이 있습니다. LOT ID : {% 1}
                return false;
            }

            return true;
        }

        private bool ValidationBarcodeLabel()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(GetLotProdVerCode()))
            {
                Util.MessageValidation("SFU4561");     // 생산실적 화면의 저장버튼 클릭 후(버전 정보 저장) 바코드 출력 하시기 바랍니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003");     // 프린트 환경 설정값이 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationReport()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationPrint()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEqptIssue()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationCancelFCut()
        {
            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationCancelDelete()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationCut()
        {
            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEqptCond()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1632");     // 선택된 LOT이 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationMixConfirm()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationSamplingProdT1()
        {
            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationMixerTankInfo()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1490");     // 대LOT을 선택하십시오.
                return false;
            }

            return true;
        }

        private bool ValidationReservation()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationStartCoaterCut()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1490");     // 대LOT을 선택하십시오.
                return false;
            }

            return true;
        }

        private bool ValidationWorkHalfSlitSide()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEmSectionRollDirctn()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationLogisStat()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationSlBatch()
        {
            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationScrapLot()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");      // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationRollMapLotNextProcessMove()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_ROLLMAP_LOT_NEXT_PROC_MOVE";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                dr["WIPSEQ"] = Util.NVC(_dvProductLot["WIPSEQ"]);
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", null, inTable);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidateProductQty(C1DataGrid dg)
        {
            if (dg.GetRowCount() > 0)
            {
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUTQTY")) <= 0)
                    {
                        Util.MessageValidation("SFU1617");     // 생산수량을 확인하십시오.
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ValidWorkTime(string StartDateTime, string EndDateTime)
        {
            if (string.IsNullOrEmpty(StartDateTime))
            {
                Util.MessageValidation("SFU1696");     // 시작시간 정보가 없습니다.
                return false;
            }

            double totalMin = (Convert.ToDateTime(EndDateTime) - Convert.ToDateTime(StartDateTime)).TotalMinutes;

            if (totalMin < 0)
            {
                Util.MessageValidation("SFU1219");     // 가동시간을 확인 하세요.
                return false;
            }
            return true;
        }

        private bool ValidOverProdQty(C1DataGrid dg)
        {
            // 2021.10.22. 한종현. requsted by 이문철 주관. ESNJ 1동 생산시스템 구축 1단계 프로젝트. 
            // 실적 확정 시, 슬리터의 경우 생산량과 불량수를 비교할 때 INPUTQTY * LANE 를 생산량으로 계산
            double inputQty = double.NaN;
            //if (_processCode == Process.SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            if (_processCode == Process.SLITTING || _processCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
                inputQty = Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[dg.TopRows.Count].DataItem, "GOODQTY"));
            else
                inputQty = Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[dg.TopRows.Count].DataItem, "INPUTQTY"));

            double lossQty = Convert.ToDouble(DataTableConverter.GetValue(dg.Rows[dg.TopRows.Count].DataItem, "LOSSQTY"));

            if (inputQty < lossQty)
            {
                Util.MessageValidation("SFU3236");
                return false;
            }
            return true;
        }



        #endregion

        #region =============================Mixing
        private bool ValidationStartMixing()
        {
            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidationDispatchMixing()
        {
            DataRow[] dr = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (dr.Length == 0 || string.IsNullOrWhiteSpace(dr[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU8109");     // WO를 선택해주세요.
                return false;
            }

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_Mixing.CheckConfirmLot() == false)
            {
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() != "EQPT_END")
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            // 생산수량 Min, Max 체크
            if (!ValidateInputQtyLimit()) return false;

            // 생산수량 입력 여부 체크
            if (!ValidateProductQty()) return false;

            // 작업시간 체크
            if (!ValidWorkTimeMixing()) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            //// 작업자
            //if (_drShift.Length == 0 || string.IsNullOrWhiteSpace(_drShift[0]["WRK_USERID"].ToString()))
            //{
            //    
            //    Util.MessageValidation("SFU1843");     // 작업자를 입력 해 주세요.
            //    return false;
            //}

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQtyMixing()) return false;

            // 선분산, BS, CMC제외
            if (_processCode == Process.MIXING)
            {
                if (string.IsNullOrEmpty(UcResult_Mixing.txtVersion.Text.Trim()))
                {
                    Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                    return false;
                }
            }

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_Mixing.dgQuality)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_Mixing.dgQuality)) return false;

            if (!ValidDataCollect(UcResult_Mixing.bChangeQuality, UcResult_Mixing.bChangeMaterial, UcResult_Mixing.bChangeRemark)) return false;

            return true;
        }

        private bool ValidateProductQty()
        {
            if (UcResult_Mixing.txtProductionQty.Value.ToString() == double.NaN.ToString() || UcResult_Mixing.txtProductionQty.Value <= 0)
            {
                Util.MessageValidation("SFU1617");     // 생산수량을 확인하십시오.
                return false;
            }
            return true;
        }

        private bool ValidateInputQtyLimit()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["EQPTID"] = _equipmentCode;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXER_INPUT_VALID", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    // CWA요청으로 Validation 조건 변경 [2019-06-02]
                    decimal inputQty = Util.NVC_Decimal(UcResult_Mixing.txtProductionQty.Value);

                    if (!string.IsNullOrWhiteSpace(Util.NVC(result.Rows[0]["LOT_PROD_MAX_QTY"])) && !string.IsNullOrWhiteSpace(Util.NVC(result.Rows[0]["LOT_PROD_MIN_QTY"])))
                    {
                        if (Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MAX_QTY"]) < inputQty || Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MIN_QTY"]) > inputQty)
                        {
                            Util.MessageValidation("SFU3359");     // 입력 가능한 생산량 범위를 벗어났습니다.
                            return false;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(Util.NVC(result.Rows[0]["LOT_PROD_MAX_QTY"])))
                    {
                        if (Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MAX_QTY"]) < inputQty)
                        {
                            Util.MessageValidation("SFU3359");     // 입력 가능한 생산량 범위를 벗어났습니다.
                            return false;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(Util.NVC(result.Rows[0]["LOT_PROD_MIN_QTY"])))
                    {
                        if (Util.NVC_Decimal(result.Rows[0]["LOT_PROD_MIN_QTY"]) > inputQty)
                        {
                            Util.MessageValidation("SFU3359");     // 입력 가능한 생산량 범위를 벗어났습니다.
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

        private bool ValidWorkTimeMixing()
        {
            if (string.IsNullOrEmpty(UcResult_Mixing.txtStartDateTime.Text))
            {
                Util.MessageValidation("SFU1696");     // 시작시간 정보가 없습니다.
                return false;
            }

            double totalMin = (Convert.ToDateTime(UcResult_Mixing.txtEndDateTime.Text) - Convert.ToDateTime(UcResult_Mixing.txtStartDateTime.Text)).TotalMinutes;

            if (totalMin < 0)
            {
                Util.MessageValidation("SFU1219");     // 가동시간을 확인 하세요.
                return false;
            }
            return true;
        }

        private bool ValidOverProdQtyMixing()
        {
            double inputQty = Convert.ToDouble(UcResult_Mixing.txtProductionQty.Value);
            double lossQty = Convert.ToDouble(DataTableConverter.GetValue(UcResult_Mixing.dgProductResult.Rows[0].DataItem, "DEFECT_SUM"));

            if (inputQty < lossQty)
            {
                Util.MessageValidation("SFU3236");     // 불량수량의 합이 생산량 초과 [불량 입력 수량 확인 후 재입력]
                return false;
            }
            return true;
        }

        private bool ValidQualitySpec(string validType)
        {
            bool bRet = true;
            try
            {
                DataTable qualityList = UcResult_Mixing.dgQuality.ItemsSource == null ? null : ((DataView)UcResult_Mixing.dgQuality.ItemsSource).ToTable();
                //DataTable qualityList2 = QUALITY2_GRID.ItemsSource == null ? null : ((DataView)QUALITY2_GRID.ItemsSource).ToTable();
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;

                if (qualityList != null && qualityList.Rows.Count > 0)
                {
                    for (int i = 0; i < qualityList.Rows.Count; i++)
                    {
                        LSL = qualityList.Rows[i]["LSL"].ToString();
                        USL = qualityList.Rows[i]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList.Rows[i]["AUTO_HOLD_FLAG"].ToString();

                        //yield LSL USL => 또 팝업 => 예를 누르면 실적확정되면서 무조건 홀드

                        if (!qualityList.Rows[i]["CLCTVAL01"].ToString().Equals("NaN"))
                        {
                            CLCTVAL = qualityList.Rows[i]["CLCTVAL01"].ToString();
                        }

                        if (!String.IsNullOrWhiteSpace(LSL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            //validType이 Hold면 자동보류여부를 체크하고
                            //아니면 체크 안하고
                            if (Util.NVC_Int(LSL) > 0 && Util.NVC_Int(LSL) > Util.NVC_Int(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                        if (!String.IsNullOrWhiteSpace(USL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                        {
                            if (Util.NVC_Int(USL) > 0 && Util.NVC_Int(USL) < Util.NVC_Int(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                            {
                                bRet = false;
                            }
                        }
                    }
                }
                //if (qualityList2 != null && qualityList2.Rows.Count > 0)
                //{
                //    for (int j = 0; j < qualityList2.Rows.Count; j++)
                //    {
                //        LSL = qualityList2.Rows[j]["LSL"].ToString();
                //        USL = qualityList2.Rows[j]["USL"].ToString();
                //        AUTO_HOLD_FLAG = qualityList2.Rows[j]["AUTO_HOLD_FLAG"].ToString();

                //        if (!qualityList2.Rows[j]["CLCTVAL01"].ToString().Equals("NaN"))
                //        {
                //            CLCTVAL = qualityList2.Rows[j]["CLCTVAL01"].ToString();
                //        }

                //        if (!String.IsNullOrWhiteSpace(LSL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                //        {
                //            if (Util.NVC_Int(LSL) > 0 && Util.NVC_Int(LSL) > Util.NVC_Int(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                //            {
                //                bRet = false;
                //            }
                //        }
                //        if (!String.IsNullOrWhiteSpace(USL) && !String.IsNullOrWhiteSpace(CLCTVAL))
                //        {
                //            if (Util.NVC_Int(USL) > 0 && Util.NVC_Int(USL) < Util.NVC_Int(CLCTVAL) && ((validType.Equals("Hold") && AUTO_HOLD_FLAG.Equals("Y")) || validType.Equals("Auth")))
                //            {
                //                bRet = false;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                bRet = true;
            }

            return bRet;
        }

        private bool ValidQualitySpecExists()
        {
            bool bRet = true;
            try
            {
                DataTable qualityList = UcResult_Mixing.dgQuality.ItemsSource == null ? null : ((DataView)UcResult_Mixing.dgQuality.ItemsSource).ToTable();
                //DataTable qualityList2 = QUALITY2_GRID.ItemsSource == null ? null : ((DataView)QUALITY2_GRID.ItemsSource).ToTable();
                string LSL = string.Empty;
                string USL = string.Empty;
                string AUTO_HOLD_FLAG = string.Empty;
                string CLCTVAL = string.Empty;

                if (qualityList != null && qualityList.Rows.Count > 0)
                {
                    for (int i = 0; i < qualityList.Rows.Count; i++)
                    {
                        LSL = qualityList.Rows[i]["LSL"].ToString();
                        USL = qualityList.Rows[i]["USL"].ToString();
                        AUTO_HOLD_FLAG = qualityList.Rows[i]["AUTO_HOLD_FLAG"].ToString();

                        if (AUTO_HOLD_FLAG.Equals("Y"))
                        {
                            if (string.IsNullOrWhiteSpace(LSL) || string.IsNullOrWhiteSpace(USL))
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                }
                //if (qualityList2 != null && qualityList2.Rows.Count > 0)
                //{
                //    for (int j = 0; j < qualityList2.Rows.Count; j++)
                //    {
                //        LSL = qualityList2.Rows[j]["LSL"].ToString();
                //        USL = qualityList2.Rows[j]["USL"].ToString();
                //        AUTO_HOLD_FLAG = qualityList2.Rows[j]["AUTO_HOLD_FLAG"].ToString();

                //        if (AUTO_HOLD_FLAG.Equals("Y"))
                //        {
                //            if (string.IsNullOrWhiteSpace(LSL) || string.IsNullOrWhiteSpace(USL))
                //            {
                //                bRet = false;
                //                break;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                bRet = true;
            }

            return bRet;
        }

        #endregion

        #region =============================Coating
        private bool ValidationStartCoating()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidationDispatchCoating()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 확정 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.END))
            {
                Util.MessageValidation("SFU5131");     // 공정이동 대상 Lot 선택 오류 [선택한 Lot이 완공상태 인지 확인 후 처리]
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_CoatingAuto.CheckConfirmLot() == false)
            {
                return false;
            }

            // TEST CUT 실적반영 여부
            // ROLLMAP 수불인 경우 TEST CUT 실적반영 여부 체크하지 않음 ==> 원복 (2023-01-31)
            //if (!_isRollMapEquipment && !UcResult_CoatingAuto.CheckTestCut()) return false;
            if (!UcResult_CoatingAuto.CheckTestCut()) return false;

            // 양품량 체크
            if (!IsValidGoodQty(UcResult_CoatingAuto.dgProductResult)) return false;

            // 생산수량 체크
            if (!IsValidLimitQty(UcResult_CoatingAuto.dgProductResult)) return false;

            // TOP LOSS 수량 CHECK
            // ROLLMAP 수불인 경우 TOP LOSS 수량 CHECK 하지 않음
            if (!_isRollMapEquipment && !IsValidTopQty(UcResult_CoatingAuto.dgProductResult)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            //// 작업자
            //if (_drShift.Length == 0 || string.IsNullOrWhiteSpace(_drShift[0]["WRK_USERID"].ToString()))
            //{
            //    
            //    Util.MessageValidation("SFU1843");     // 작업자를 입력 해 주세요.
            //    return false;
            //}

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_CoatingAuto.dgQualityTop)) return false;
            if (!ValidQualityRequired(UcResult_CoatingAuto.dgQualityBack)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_CoatingAuto.dgQualityTop)) return false;
            if (!ValidQualitySpecRequired(UcResult_CoatingAuto.dgQualityBack)) return false;

            if (!ValidDataCollect(UcResult_CoatingAuto.bChangeQuality, UcResult_CoatingAuto.bChangeMaterial, UcResult_CoatingAuto.bChangeRemark)) return false;

            //롤맵 LOT 공정이동시 불량,LOSS 코드 입력 여부 체크
            if (_isRollMapEquipment && _isRollMapLot)
            {
                if (!ValidationRollMapLotNextProcessMove())
                    return false;
            }

            //Slurry Input Lot 확인 인터락
            string[] sAttrbute = { _processCode };
            if (_util.IsAreaCommoncodeAttrUse("EQPT_CURR_MOUNT_INPUT_LOT", "USE_YN", sAttrbute))
            {
                if (!IsChickSlurryInEQPT())
                {
                    return false;
                }
            }


            return true;
        }

        private void ValidateCarrierCTUnloader()
        {
            try
            {
                if (_unldrLotIdentBasCode == "CST_ID" || _unldrLotIdentBasCode == "RF_ID")
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("LOTID", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LOTID"] = _dvProductLot["LOTID"];
                    IndataTable.Rows.Add(Indata);

                    DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "INDATA", "RSLTDT", IndataTable);

                    if (result.Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(result.Rows[0]["CSTID"].ToString()))
                        {
                            PopupElecCstRelation();
                        }
                        else
                        {
                            PopupConfirmUser();
                        }
                    }
                }
                else
                {
                    PopupConfirmUser();
                }
            }
            catch (Exception e)
            {
                Util.MessageException(e);
                return;
            }
            return;
        }

        private bool IsValidGoodQty(C1DataGrid dg)
        {
            if (dg.GetRowCount() > 0)
            {
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "GOODQTY")) < 0)
                        {
                            Util.MessageValidation("SFU5129");  //양품량이 0보다 작습니다.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool IsValidLimitQty(C1DataGrid dg)
        {
            if (dg.GetRowCount() > 0)
            {
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (!string.Equals(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "CHG_BLOCK_FLAG"), "Y"))
                    {
                        if (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "INPUT_BACK_QTY")) !=
                            (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "GOODQTY")) +
                            Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i + dg.TopRows.Count].DataItem, "DTL_BACK_DEFECT_SUM"))))
                        {
                            Util.MessageValidation("SFU1617");  //생산수량을 확인하십시오.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool IsValidTopQty(C1DataGrid dg)
        {
            if (dg.GetRowCount() > 0)
            {
                // 단선수량이 투입량에 반영되면서 TOP LOSS 체크 로직 변경 [2019-12-04]
                if ((Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[dg.Rows.Count - 1].DataItem, "INPUT_TOP_QTY")) + GetDiffWebBreakQty(UcResult_CoatingAuto.dgWipReasonTop, "DEFECT_LOT", "TOP")) != 0)
                {
                    Util.MessageValidation("SFU5130");     // Top의 수량 차이가 0이 되도록 불량/Loss 수량 수정 바랍니다.
                    return false;
                }
            }
            return true;
        }

        private bool ValidationCleanLot()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }
        #endregion

        #region =============================InsCoating
        private bool ValidationStartInsCoating()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidationDispatchInsCoating()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 확정 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");  //실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_InsCoating.CheckConfirmLot() == false)
            {
                return false;
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_InsCoating.dgProductResult)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_InsCoating.dgQualityTop)) return false;
            if (!ValidQualityRequired(UcResult_InsCoating.dgQualityBack)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_InsCoating.dgQualityTop)) return false;
            if (!ValidQualitySpecRequired(UcResult_InsCoating.dgQualityBack)) return false;

            if (!ValidDataCollect(UcResult_InsCoating.bChangeQuality, UcResult_InsCoating.bChangeMaterial, UcResult_InsCoating.bChangeRemark)) return false;

            return true;
        }

        #endregion

        #region =============================Half Slitting
        private bool ValidationMove()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationStartHalfSlitting()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidationDispatchHalfSlitting()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 설비완공 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_HalfSlitting.CheckConfirmLot() == false)
            {
                return false;
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_HalfSlitting.dgProductResult)) return false;

            if (string.IsNullOrEmpty(UcResult_HalfSlitting.txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcResult_HalfSlitting.txtLaneQty.Value)) || UcResult_HalfSlitting.txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");     // Lane 정보가 없습니다
                return false;
            }

            // 작업시간 체크
            if (!ValidWorkTime(UcResult_HalfSlitting.txtStartDateTime.Text, UcResult_HalfSlitting.txtEndDateTime.Text)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            //// 작업자
            //if (_drShift.Length == 0 || string.IsNullOrWhiteSpace(_drShift[0]["WRK_USERID"].ToString()))
            //{
            //    
            //    Util.MessageValidation("SFU1843");     // 작업자를 입력 해 주세요.
            //    return false;
            //}

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQty(UcResult_HalfSlitting.dgProductResult)) return false;

            if (!ValidConfirmQtyHalfSlitting()) return false;

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_HalfSlitting.dgQuality)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_HalfSlitting.dgQuality)) return false;

            if (!ValidDataCollect(UcResult_HalfSlitting.bChangeQuality, false, UcResult_HalfSlitting.bChangeRemark)) return false;

            return true;
        }

        private bool ValidConfirmQtyHalfSlitting()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_HalfSlitting.dgProductResult.Rows[UcResult_HalfSlitting.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            if (UcResult_HalfSlitting.ExceedLengthQty > 0)
            {
                // 투입량 = (생산량 - 길이초과) 여야 확정 가능
                if (Util.NVC_Decimal(UcResult_HalfSlitting.txtParentQty.Value) != (inputQty - UcResult_HalfSlitting.ExceedLengthQty))
                {
                    Util.MessageValidation("SFU3417");     // 길이초과 입력 시 잔량이 0이어야 합니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidLaneWrongQty(C1DataGrid dg, double dLaneQty)
        {
            if (_processCode == Process.HALF_SLITTING || _processCode == Process.SLITTING || _processCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발 START//20211215 2차 Slitting 공정진척DRB 화면 개발
            {
                for (int i = dg.TopRows.Count; i < (dg.Rows.Count - dg.BottomRows.Count); i++)
                    if (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LANE_QTY")) == 1)
                        return false;
            }
            ////20211215 2차 Slitting 공정진척DRB 화면 개발 START
            //else if (_processCode == Process.TWO_SLITTING)
            //{
            //    for (int i = dg.TopRows.Count; i < (dg.Rows.Count - dg.BottomRows.Count); i++)
            //        if (Util.NVC_Decimal(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LANE_QTY2")) == 1)
            //            return false;
            //}
            ////20211215 2차 Slitting 공정진척DRB 화면 개발 END
            else
            {
                if (dLaneQty == 1)
                    return false;
            }

            return true;
        }
        #endregion

        #region =============================Slitting
        private bool ValidationStartSlitting()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidConfirmQtySlitting()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_Slitting.dgProductResult.Rows[UcResult_Slitting.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            if (UcResult_Slitting.ExceedLengthQty > 0)
            {
                // 투입량 = (생산량 - 길이초과) 여야 확정 가능
                if (Util.NVC_Decimal(UcResult_Slitting.txtParentQty.Value) != (inputQty - UcResult_Slitting.ExceedLengthQty))
                {
                    Util.MessageValidation("SFU3417");     // 길이초과 입력 시 잔량이 0이어야 합니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDispatchSlitting()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_Slitting.CheckConfirmLot() == false)
            {
                return false;
            }

            // 설비완공 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (string.IsNullOrEmpty(UcResult_Slitting.txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcResult_Slitting.txtLaneQty.Value)) || UcResult_Slitting.txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");     // Lane 정보가 없습니다
                return false;
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_Slitting.dgProductResult)) return false;

            // 작업시간 체크
            if (!ValidWorkTime(UcResult_Slitting.txtStartDateTime.Text, UcResult_Slitting.txtEndDateTime.Text)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQty(UcResult_Slitting.dgProductResult)) return false;

            if (!ValidConfirmQtySlitting()) return false;

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_Slitting.dgQuality)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_Slitting.dgQuality)) return false;

            if (!ValidDataCollect(UcResult_Slitting.bChangeQuality, false, UcResult_Slitting.bChangeRemark)) return false;

            if (!CheckRollQASampling()) return false;

            if (!Validation_Slit_GRD_EqptEnd()) return false;     //NFF 슬리팅 전극 등급 정보 확인

            return true;
        }

        #endregion

        #region =============================Roll Pressing
        private bool ValidationPopupDfctLanePancake()
        {
            if (_dvProductLot["WIPSTAT"].ToString() != Wip_State.END)
            {
                Util.MessageValidation("SFU3723");     // 작업 가능한 상태가 아닙니다.
                return false;
            }

            return true;
        }

        private bool ValidationStartRollPressing()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidConfirmQtyRollPressing()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_RollPressing.dgProductResult.Rows[UcResult_RollPressing.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            if (UcResult_RollPressing.ExceedLengthQty > 0)
            {
                // 투입량 = (생산량 - 길이초과) 여야 확정 가능
                if (Util.NVC_Decimal(UcResult_RollPressing.txtParentQty.Value) != (inputQty - UcResult_RollPressing.ExceedLengthQty))
                {
                    Util.MessageValidation("SFU3417");     // 길이초과 입력 시 잔량이 0이어야 합니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDispatchRollPressing()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            // 05.30 추가 -> 동별 공통 코드에 따라 롤프레스 대Lot 품질 정보 필수 입력 SKIP
            DataTable dt1 = new DataTable();
            dt1.Columns.Add("LANGID", typeof(string));
            dt1.Columns.Add("AREAID", typeof(string));
            dt1.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr1 = dt1.NewRow();
            dr1["LANGID"] = LoginInfo.LANGID;
            dr1["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr1["COM_TYPE_CODE"] = "QC_ITEM_SPEC_CHECK_EXCEPTION_FLAG";
            dt1.Rows.Add(dr1);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR", "RQSTDT", "RSLTDT", dt1);

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_RollPressing.CheckConfirmLot() == false)
            {
                return false;
            }

            // 설비완공 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (string.IsNullOrEmpty(UcResult_RollPressing.txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcResult_RollPressing.txtLaneQty.Value)) || UcResult_RollPressing.txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");     // Lane 정보가 없습니다
                return false;
            }

            // CSR : [E20231030-000234] - 실적확정시 무지부 방향 설정 미등록 Validation
            string[] sAttrb = { "" };
            if (_util.IsAreaCommoncodeAttrUse("NON_COATED_NOT_SET", _processCode, sAttrb))
            {
                if (UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag == null || string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtWorkHalfSlittingSide.Text))
                {
                    Util.MessageValidation("SFU8924");     // 무지부 방향 설정이 미등록 상태입니다. 등록 후 실행 바랍니다.
                    return false;
                }
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_RollPressing.dgProductResult)) return false;

            // 작업시간 체크
            if (!ValidWorkTime(UcResult_RollPressing.txtStartDateTime.Text, UcResult_RollPressing.txtEndDateTime.Text)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            //// 작업자
            //if (_drShift.Length == 0 || string.IsNullOrWhiteSpace(_drShift[0]["WRK_USERID"].ToString()))
            //{
            //    
            //    Util.MessageValidation("SFU1843");     // 작업자를 입력 해 주세요.
            //    return false;
            //}

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQty(UcResult_RollPressing.dgProductResult)) return false;

            if (!ValidConfirmQtyRollPressing()) return false;

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_RollPressing.dgQuality)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_RollPressing.dgQuality)) return false;

            if (!ValidDataCollect(UcResult_RollPressing.bChangeQuality, false, UcResult_RollPressing.bChangeRemark)) return false;

            // 05.30 추가 -> 동별 공통 코드에 따라 롤프레스 대Lot 품질 정보 필수 입력 SKIP, 사용여부 N이면 품질 검사 진행
            if (dtResult.Rows.Count == 0)
                if (!CheckRollQASampling()) return false;



            return true;
        }
        #endregion

        #region =============================ReWinder

        private bool ValidationStartReWinder()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidConfirmQtyReWinder()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_ReWinder.dgProductResult.Rows[UcResult_ReWinder.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            if (UcResult_ReWinder.ExceedLengthQty > 0)
            {
                // 투입량 = (생산량 - 길이초과) 여야 확정 가능
                if (Util.NVC_Decimal(UcResult_ReWinder.txtParentQty.Value) != (inputQty - UcResult_ReWinder.ExceedLengthQty))
                {
                    Util.MessageValidation("SFU3417");     // 길이초과 입력 시 잔량이 0이어야 합니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDispatchReWinder()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_ReWinder.CheckConfirmLot() == false)
            {
                return false;
            }

            // 설비완공 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (string.IsNullOrEmpty(UcResult_ReWinder.txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcResult_ReWinder.txtLaneQty.Value)) || UcResult_ReWinder.txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");     // Lane 정보가 없습니다
                return false;
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_ReWinder.dgProductResult)) return false;

            // 작업시간 체크
            if (!ValidWorkTime(UcResult_ReWinder.txtStartDateTime.Text, UcResult_ReWinder.txtEndDateTime.Text)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQty(UcResult_ReWinder.dgProductResult)) return false;

            if (!ValidConfirmQtyReWinder()) return false;

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_ReWinder.dgQuality)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_ReWinder.dgQuality)) return false;

            if (!ValidDataCollect(UcResult_ReWinder.bChangeQuality, false, UcResult_ReWinder.bChangeRemark)) return false;

            if (!CheckRollQASampling()) return false;

            return true;
        }
        #endregion

        #region =============================재와인딩
        private bool ValidationMergeLotList()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationStartReWinding()
        {
            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1223");     // 라인을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            return true;
        }

        private bool ValidationCancelReWinding()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1938");     // 취소할 LOT을 선택하세요.
                return false;
            }

            return true;
        }

        private bool ValidationEqptEndRewinder()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() != "PROC")
            {
                Util.MessageValidation("SFU2957");     // 진행중인 작업을 선택하세요.
                return false;
            }

            if (UcResult_ReWinding.txtProductionQty.Value == double.NaN || UcResult_ReWinding.txtProductionQty.Value == 0)
            {
                Util.MessageValidation("SFU1609");     // 생산량을 입력하십시오.
                return false;
            }

            if (UcResult_ReWinding.bSideRollDirctnUse && (UcResult_ReWinding.txtSSWD.Tag == null || string.IsNullOrEmpty(UcResult_ReWinding.txtSSWD.Text)))
            {
                Util.MessageValidation("SFU6030");  // 무지부 방향을 선택하세요.
                return false;
            }

            #region E20230825-001646

            //재와인딩 공정 생산실적 
            DataRow[] drRWProd = DataTableConverter.Convert(UcResult_ReWinding.dgProductResult.ItemsSource).Select("INPUT_LOTID = '" + _dvProductLot["LOTID"] + "'");

            //UI 재고수량
            decimal InputQty = Util.NVC_Decimal(drRWProd[0]["INPUT_QTY"]);

            DataTable dtMerg = new DataTable();
            dtMerg.Columns.Add("PROCID", typeof(string));
            dtMerg.Columns.Add("EQPTID", typeof(string));
            dtMerg.Columns.Add("LOTID", typeof(string));
            dtMerg.Columns.Add("WIPSEQ", typeof(string));
            dtMerg.Columns.Add("INPUT_LOT_STAT_CODE", typeof(string));
            dtMerg.Columns.Add("EQPT_END_APPLY_FLAG", typeof(string));

            DataRow drRow = dtMerg.NewRow();
            drRow["PROCID"] = _processCode;
            drRow["EQPTID"] = _equipmentCode;
            drRow["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
            drRow["WIPSEQ"] = Util.NVC(_dvProductLot["WIPSEQ"]);
            drRow["INPUT_LOT_STAT_CODE"] = "PROC";

            dtMerg.Rows.Add(drRow);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_RW_MERGE_DATA_CHK", "RQSTDT", "RSLTDT", dtMerg);

            if (dtRslt.Rows.Count > 0 && dtRslt != null)
            {
                if (dtRslt.Rows[0]["RSLT_FLAG"].ToString() == "NG")
                {
                    Util.MessageValidation("SFU8916");  // 재공수량과 재와인딩 이력 수량 차이 발생. 합권취소 후 다시 합권하세요
                    return false;
                }

                //Wip 재고수량
                decimal Wipqty = Util.NVC_Decimal(dtRslt.Rows[0]["WIPQTY"]);

                if (InputQty != Wipqty)
                {
                    Util.MessageValidation("SFU8915");  // 합권취 이력이 존재합니다. 돌아가기 실행 후 LOT를 다시 선택바랍니다.
                    return false;
                }
            }

            #endregion

            return true;
        }

        private bool ValidationEqptEndCancel()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1938");     // 취소할 LOT을 선택하세요.
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() == "WAIT")
            {
                Util.MessageValidation("SFU1939");     // 취소 할 수 있는 상태가 아닙니다.
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() == "PROC")
            {

                Util.MessageValidation("SFU3464");     // 진행중인 LOT은 장비완료취소 할 수 없습니다. [진행중인 LOT은 시작취소 버튼으로 작업취소 바랍니다.]
                return false;
            }

            ///////////////////////////////////// 차후 권한 설정
            //if (_util.IsCommonCodeUse("ELEC_CNFM_CANCEL_USER", LoginInfo.USERID) == false)
            //{
            //    Util.MessageValidation("SFU5148", new object[] { LoginInfo.USERID });  //해당 USER[%1]는 실적취소할 권한이 없습니다. (시스템 담당자에게 문의 바랍니다.)
            //    return false;
            //}

            return true;
        }


        private bool ValidationDispatchReWinding()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() != "EQPT_END")
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            //// 작업자
            //if (_drShift.Length == 0 || string.IsNullOrWhiteSpace(_drShift[0]["WRK_USERID"].ToString()))
            //{
            //    
            //    Util.MessageValidation("SFU1843");     // 작업자를 입력 해 주세요.
            //    return false;
            //}

            if (UcResult_ReWinding.txtProductionQty.Value == double.NaN || UcResult_ReWinding.txtProductionQty.Value == 0)
            {
                Util.MessageValidation("SFU1609");     // 생산량을 입력하십시오.
                return false;
            }

            if (!ValidDataCollect(UcResult_ReWinding.bChangeQuality, false, UcResult_ReWinding.bChangeRemark)) return false;

            return true;
        }

        private bool ValidationInput()
        {
            if (string.IsNullOrWhiteSpace(_equipmentSegmentCode))
            {
                Util.MessageValidation("SFU1255");     // 라인을 선택 하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                Util.MessageValidation("SFU1459");     // 공정을 선택하세요.
                return false;
            }

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            return true;
        }

        #endregion

        #region =============================Taping

        private bool ValidationStartTaping()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidConfirmQtyTaping()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_Taping.dgProductResult.Rows[UcResult_Taping.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            if (UcResult_Taping.ExceedLengthQty > 0)
            {
                // 투입량 = (생산량 - 길이초과) 여야 확정 가능
                if (Util.NVC_Decimal(UcResult_Taping.txtParentQty.Value) != (inputQty - UcResult_Taping.ExceedLengthQty))
                {
                    Util.MessageValidation("SFU3417");     // 길이초과 입력 시 잔량이 0이어야 합니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDispatchTaping()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_Taping.CheckConfirmLot() == false)
            {
                return false;
            }

            // 설비완공 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (string.IsNullOrEmpty(UcResult_Taping.txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcResult_Taping.txtLaneQty.Value)) || UcResult_Taping.txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");     // Lane 정보가 없습니다
                return false;
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_Taping.dgProductResult)) return false;

            // 작업시간 체크
            if (!ValidWorkTime(UcResult_Taping.txtStartDateTime.Text, UcResult_Taping.txtEndDateTime.Text)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQty(UcResult_Taping.dgProductResult)) return false;

            if (!ValidConfirmQtyTaping()) return false;

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_Taping.dgQuality)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_Taping.dgQuality)) return false;

            if (!ValidDataCollect(UcResult_Taping.bChangeQuality, false, UcResult_Taping.bChangeRemark)) return false;

            if (!CheckRollQASampling()) return false;

            return true;
        }
        #endregion

        #region =============================Heat Treatment

        private bool ValidationStartHeatTreatment()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidConfirmQtyHeatTreatment()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_HeatTreatment.dgProductResult.Rows[UcResult_HeatTreatment.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            if (UcResult_HeatTreatment.ExceedLengthQty > 0)
            {
                // 투입량 = (생산량 - 길이초과) 여야 확정 가능
                if (Util.NVC_Decimal(UcResult_HeatTreatment.txtParentQty.Value) != (inputQty - UcResult_HeatTreatment.ExceedLengthQty))
                {
                    Util.MessageValidation("SFU3417");     // 길이초과 입력 시 잔량이 0이어야 합니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDispatchHeatTreatment()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_HeatTreatment.CheckConfirmLot() == false)
            {
                return false;
            }

            // 설비완공 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (string.IsNullOrEmpty(UcResult_HeatTreatment.txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcResult_HeatTreatment.txtLaneQty.Value)) || UcResult_HeatTreatment.txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");     // Lane 정보가 없습니다
                return false;
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_HeatTreatment.dgProductResult)) return false;

            // 작업시간 체크
            if (!ValidWorkTime(UcResult_HeatTreatment.txtStartDateTime.Text, UcResult_HeatTreatment.txtEndDateTime.Text)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQty(UcResult_HeatTreatment.dgProductResult)) return false;

            if (!ValidConfirmQtyHeatTreatment()) return false;

            if (!ValidDataCollect(UcResult_HeatTreatment.bChangeQuality, false, UcResult_HeatTreatment.bChangeRemark)) return false;

            if (!CheckRollQASampling()) return false;

            return true;
        }
        #endregion

        //20211215 2차 Slitting 공정진척DRB 화면 개발 START
        #region =============================TwoSlitting
        private bool ValidationStartTwoSlitting()
        {
            if (string.IsNullOrWhiteSpace(UcElectrodeProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // 설비를 선택 하세요.
                return false;
            }

            // W/O 체크
            DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (drWO.Length == 0 || string.IsNullOrWhiteSpace(drWO[0]["VAL004"].ToString()))
            {
                Util.MessageValidation("SFU1436");     // W/O 선택 후 작업시작하세요
                return false;
            }

            return true;
        }

        private bool ValidConfirmQtyTwoSlitting()
        {
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(UcResult_TwoSlitting.dgProductResult.Rows[UcResult_TwoSlitting.dgProductResult.TopRows.Count].DataItem, "INPUTQTY"));

            if (UcResult_TwoSlitting.ExceedLengthQty > 0)
            {
                // 투입량 = (생산량 - 길이초과) 여야 확정 가능
                if (Util.NVC_Decimal(UcResult_TwoSlitting.txtParentQty.Value) != (inputQty - UcResult_TwoSlitting.ExceedLengthQty))
                {
                    Util.MessageValidation("SFU3417");     // 길이초과 입력 시 잔량이 0이어야 합니다.
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDispatchTwoSlitting()
        {
            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1381");     // Lot을 선택하세요.
                return false;
            }

            // 진행LOT이 실적 확정 완료이면, 실적확정, 불량/Loss저장, 품질검사 저장 전체 방어 [2018-11-16]
            if (UcResult_TwoSlitting.CheckConfirmLot() == false)
            {
                return false;
            }

            // 설비완공 상태일 경우만 공정 이동 처리 가능
            if (!string.Equals(_dvProductLot["WIPSTAT"], Wip_State.EQPT_END))
            {
                Util.MessageValidation("SFU3194");     // 실적확정 Lot 선택 오류 [선택한 Lot이 장비완료상태 인지 확인 후 처리]
                return false;
            }

            if (string.IsNullOrEmpty(UcResult_TwoSlitting.txtVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1218");     // Version 정보를 입력 하세요.
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcResult_TwoSlitting.txtLaneQty.Value)) || UcResult_TwoSlitting.txtLaneQty.Value < 1)
            {
                Util.MessageValidation("SFU1351");     // Lane 정보가 없습니다
                return false;
            }

            // 생산수량 체크
            if (!ValidateProductQty(UcResult_TwoSlitting.dgProductResult)) return false;

            // 작업시간 체크
            if (!ValidWorkTime(UcResult_TwoSlitting.txtStartDateTime.Text, UcResult_TwoSlitting.txtEndDateTime.Text)) return false;

            // 작업조
            if (drShift.Length == 0 || string.IsNullOrWhiteSpace(drShift[0]["SHFT_ID"].ToString()))
            {
                Util.MessageValidation("SFU1845");     // 작업조를 입력하세요.
                return false;
            }

            // 생산량 < 불량수량SUM
            if (!ValidOverProdQty(UcResult_TwoSlitting.dgProductResult)) return false;

            if (!ValidConfirmQtyTwoSlitting()) return false;

            // 품질 정보 필수 입력 체크
            if (!ValidQualityRequired(UcResult_TwoSlitting.dgQuality)) return false;

            // 품질 정보 상/하한 값이 존재하는 경우 측정값 필수 입력
            if (!ValidQualitySpecRequired(UcResult_TwoSlitting.dgQuality)) return false;

            if (!ValidDataCollect(UcResult_TwoSlitting.bChangeQuality, false, UcResult_TwoSlitting.bChangeRemark)) return false;

            if (!CheckRollQASampling()) return false;

            return true;
        }
        #endregion
        //20211215 2차 Slitting 공정진척DRB 화면 개발 END
        #endregion

        #region [팝업]
        #region =============================공통
        private void PopupConfirmUser()
        {
            ELEC003_CONFIRM_USER popupConfirmUser = new ELEC003_CONFIRM_USER();
            popupConfirmUser.FrameOperation = FrameOperation;

            if (popupConfirmUser != null)
            {
                DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                object[] Parameters = new object[11];
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _processName;
                Parameters[3] = _equipmentCode;
                Parameters[4] = _equipmentName;
                Parameters[5] = _dvProductLot["LOTID"].ToString();
                Parameters[6] = drShift[0]["SHFT_ID"].ToString();
                Parameters[7] = drShift[0]["WRK_STRT_DTTM"].ToString();
                Parameters[8] = drShift[0]["WRK_END_DTTM"].ToString();
                Parameters[9] = drShift[0]["WRK_USERID"].ToString();
                Parameters[10] = drShift[0]["VAL002"].ToString();

                C1WindowExtension.SetParameters(popupConfirmUser, Parameters);

                popupConfirmUser.Closed += new EventHandler(PopupConfirmUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupConfirmUser.ShowModal()));
                popupConfirmUser.CenterOnScreen();
            }
        }

        private void PopupConfirmUser_Closed(object sender, EventArgs e)
        {
            ELEC003_CONFIRM_USER popup = sender as ELEC003_CONFIRM_USER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                SaveRealWorker(popup.ConfirmkUserName);

                if (_processCode == Process.MIXING ||
                    _processCode == Process.PRE_MIXING ||
                    _processCode == Process.BS ||
                    _processCode == Process.CMC ||
                    _processCode == Process.InsulationMixing ||
                    _processCode == Process.DAM_MIXING)
                    ConfirmProcessMixing();
                else if (_processCode == Process.COATING)
                    ConfirmProcessCoating();
                else if (_processCode == Process.INS_COATING)
                    ConfirmProcessInsCoating();
                else if (_processCode == Process.HALF_SLITTING)
                    ConfirmProcessHalfSlitting();
                else if (_processCode == Process.SLITTING)
                    ConfirmProcessSlitting();
                //20211215 2차 Slitting 공정진척DRB 화면 개발 START
                else if (_processCode == Process.TWO_SLITTING)
                    ConfirmProcessTwoSlitting();
                //20211215 2차 Slitting 공정진척DRB 화면 개발 EMD
                else if (_processCode == Process.ROLL_PRESSING)
                    ConfirmProcessRollPressing();
                else if (_processCode == Process.REWINDER)
                    ConfirmProcessReWinder();
                else if (_processCode == Process.TAPING)
                    ConfirmProcessTaping();
                else if (_processCode == Process.HEAT_TREATMENT)
                    ConfirmProcessHeatTreatment();
                else if (_reWindingProcess == "Y")
                    ConfirmProcessReWinding();
            }
        }

        /// <summary>
        /// 단선추가
        /// </summary>
        private void PopupWebBreak()
        {
            if (!ValidationWebBreak()) return;

            ELEC003_WEB_BREAK popupWebBreak = new ELEC003_WEB_BREAK();
            popupWebBreak.FrameOperation = FrameOperation;

            if (popupWebBreak != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _dvProductLot["LOTID"].ToString();

                C1WindowExtension.SetParameters(popupWebBreak, Parameters);

                popupWebBreak.Closed += new EventHandler(PopupWebBreak_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupWebBreak.ShowModal()));
                popupWebBreak.CenterOnScreen();
            }
        }

        private void PopupWebBreak_Closed(object sender, EventArgs e)
        {
            ELEC003_WEB_BREAK popup = sender as ELEC003_WEB_BREAK;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(string.Empty);
                SetProductLotList();
            }
        }

        private void PopupReport()
        {
            if (!ValidationReport()) return;

            CMM_ELEC_REPORT2 popupReport = new CMM_ELEC_REPORT2();
            popupReport.FrameOperation = FrameOperation;

            if (popupReport != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = _dvProductLot["LOTID"].ToString();
                Parameters[1] = _processCode;

                C1WindowExtension.SetParameters(popupReport, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupReport.ShowModal()));
                popupReport.CenterOnScreen();
            }
        }

        private void PopupPrint()
        {
            if (!ValidationPrint()) return;

            CMM_ELEC_BARCODE popupPrint = new CMM_ELEC_BARCODE();
            popupPrint.FrameOperation = FrameOperation;

            if (popupPrint != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _equipmentCode;

                C1WindowExtension.SetParameters(popupPrint, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupPrint.ShowModal()));
                popupPrint.CenterOnScreen();
            }
        }

        /// <summary>
        /// 설비특이사항
        /// </summary>
        private void PopupEqptIssue()
        {
            if (!ValidationEqptIssue()) return;

            CMM_COM_EQPCOMMENT popupEqptIssue = new CMM_COM_EQPCOMMENT { FrameOperation = FrameOperation };

            DataRow[] dr = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ ='2'");

            if (popupEqptIssue != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _equipmentCode;
                Parameters[2] = _processCode;
                Parameters[3] = _dvProductLot == null ? "" : _dvProductLot["LOTID"].ToString();
                Parameters[4] = _dvProductLot == null ? "" : _dvProductLot["WIPSEQ"].ToString();
                Parameters[5] = _equipmentName;
                Parameters[6] = Util.NVC(dr[0]["VAL001"]);
                Parameters[7] = Util.NVC(dr[0]["SHFT_ID"]);
                Parameters[8] = Util.NVC(dr[0]["VAL002"]);
                Parameters[9] = Util.NVC(dr[0]["WRK_USERID"]);

                C1WindowExtension.SetParameters(popupEqptIssue, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupEqptIssue.ShowModal()));
            }
        }

        /// <summary>
        /// 대Lot정리
        /// </summary>
        private void PopupCleanLot()
        {
            if (!ValidationCancelDelete()) return;

            ELEC003_LARGELOT_DELETE popupCleanLot = new ELEC003_LARGELOT_DELETE { FrameOperation = FrameOperation };

            if (popupCleanLot != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[3];
                Parameters[0] = _processCode;
                Parameters[1] = _equipmentCode;
                Parameters[2] = _equipmentName;

                C1WindowExtension.SetParameters(popupCleanLot, Parameters);
                popupCleanLot.Closed += new EventHandler(PopupCleanLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupCleanLot.ShowModal()));
            }
        }
        private void PopupCleanLot_Closed(object sender, EventArgs e)
        {
            ELEC003_LARGELOT_DELETE popup = sender as ELEC003_LARGELOT_DELETE;
            if (popup.bSaveConfirm == true)
            {
                // 설비 List 재조회
                //SetEquipment("C");
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        /// <summary>
        /// LOT종료취소
        /// </summary>
        private void PopupCancelFCut()
        {
            if (!ValidationCancelFCut()) return;

            CMM_ELEC_CANCEL_FCUT popupCancelFCut = new CMM_ELEC_CANCEL_FCUT { FrameOperation = FrameOperation };

            if (popupCancelFCut != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[2];
                Parameters[0] = _processCode;
                Parameters[1] = "N";                                // 단면 = Y", 양면 = "N"

                C1WindowExtension.SetParameters(popupCancelFCut, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupCancelFCut.ShowModal()));
            }

        }

        /// <summary>
        /// 삭제Lot생성
        /// </summary>
        private void PopupCancelDelete()
        {
            if (!ValidationCancelDelete()) return;

            CMM_ELEC_CANCEL_DELETE_LOT popupCancelDelete = new CMM_ELEC_CANCEL_DELETE_LOT { FrameOperation = FrameOperation };

            if (popupCancelDelete != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[3];
                Parameters[0] = _processCode;
                Parameters[1] = _equipmentSegmentCode;
                Parameters[2] = _equipmentCode;

                C1WindowExtension.SetParameters(popupCancelDelete, Parameters);
                popupCancelDelete.Closed += new EventHandler(PopupCancelDelete_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupCancelDelete.ShowModal()));
            }
        }
        private void PopupCancelDelete_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_CANCEL_DELETE_LOT popup = sender as CMM_ELEC_CANCEL_DELETE_LOT;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            if (_dvProductLot == null) return;

            // 생산 Lot 재조회
            string SelectLotID = _dvProductLot["LOTID"].ToString();

            SelectProductLot(SelectLotID);
            SetProductLotList(SelectLotID);
            //}
        }

        /// <summary>
        /// Cut
        /// </summary>
        private void PopupCut()
        {
            if (!ValidationCut()) return;

            CMM_ELEC_LOTCUT popupCut = new CMM_ELEC_LOTCUT { FrameOperation = FrameOperation };

            if (popupCut != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[1];
                Parameters[0] = _processCode;

                C1WindowExtension.SetParameters(popupCut, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupCut.ShowModal()));
            }
        }

        /// <summary>
        /// 작업조건등록
        /// </summary>
        private void PopupEqptCond()
        {
            if (!ValidationEqptCond()) return;

            CMM_ELEC_EQPT_COND popupEqptCond = new CMM_ELEC_EQPT_COND { FrameOperation = FrameOperation };

            //선택된 LOT
            //var SelectRow = DgProductLot.Selection.SelectedRows.ToList();

            if (popupEqptCond != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[4];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _equipmentName;
                Parameters[2] = _processCode;
                Parameters[3] = _dvProductLot["LOTID"].ToString();

                C1WindowExtension.SetParameters(popupEqptCond, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupEqptCond.ShowModal()));
            }
        }

        /// <summary>
        /// 자주검사등록
        /// </summary>
        private void PopupMixConfirm()
        {
            if (!ValidationMixConfirm()) return;

            CMM_COM_ELEC_MIXCONFIRM popupMixConfirm = new CMM_COM_ELEC_MIXCONFIRM { FrameOperation = FrameOperation };

            if (popupMixConfirm != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[6];
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _equipmentCode;
                Parameters[2] = _processCode;
                Parameters[3] = "";
                Parameters[4] = "";
                Parameters[5] = _equipmentName;

                C1WindowExtension.SetParameters(popupMixConfirm, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupMixConfirm.ShowModal()));
            }

        }

        /// <summary>
        /// R/P 샘플링 제품등록
        /// </summary>
        private void PopupSamplingProd()
        {
            CMM_ELEC_RP_SAMPLING popupSamplingProd = new CMM_ELEC_RP_SAMPLING { FrameOperation = FrameOperation };

            if (popupSamplingProd != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                C1WindowExtension.SetParameters(popupSamplingProd, null);

                this.Dispatcher.BeginInvoke(new Action(() => popupSamplingProd.ShowModal()));
            }
        }

        /// <summary>
        /// R/P 샘플링 제품등록
        /// </summary>
        private void PopupProcReturn()
        {
            CMM_ELEC_RP_RETURN popupProcReturn = new CMM_ELEC_RP_RETURN { FrameOperation = FrameOperation };

            if (popupProcReturn != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                C1WindowExtension.SetParameters(popupProcReturn, null);

                this.Dispatcher.BeginInvoke(new Action(() => popupProcReturn.ShowModal()));
            }
        }

        /// <summary>
        /// 샘플링 제품등록
        /// </summary>
        private void PopupSamplingProdT1()
        {
            if (!ValidationSamplingProdT1()) return;

            CMM_ELEC_SAMPLING popupSamplingProdT1 = new CMM_ELEC_SAMPLING { FrameOperation = FrameOperation };

            if (popupSamplingProdT1 != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[1];
                Parameters[0] = _processCode;

                C1WindowExtension.SetParameters(popupSamplingProdT1, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupSamplingProdT1.ShowModal()));
            }
        }

        /// <summary>  
        /// Slurry정보
        /// </summary>
        private void PopupMixerTankInfo()
        {
            if (!ValidationMixerTankInfo()) return;

            CMM_ELEC_MIXER_TANK_INFO popupMixerTankInfo = new CMM_ELEC_MIXER_TANK_INFO { FrameOperation = FrameOperation };

            //// 선택된 LOT
            //var SelectRow = DgProductLot.Selection.SelectedRows.ToList();

            DataRow[] drWorkOrder = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");

            if (popupMixerTankInfo != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _equipmentCode;
                Parameters[2] = _processCode;
                Parameters[3] = _dvProductLot == null ? "" : _dvProductLot["LOTID"].ToString();
                Parameters[4] = _dvProductLot == null ? "" : _dvProductLot["WIPSEQ"].ToString();
                Parameters[5] = drWorkOrder.Length > 0 ? drWorkOrder[0]["VAL007"].GetString() : null;    // Prod Id
                Parameters[6] = null;   // Material Id

                C1WindowExtension.SetParameters(popupMixerTankInfo, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupMixerTankInfo.ShowModal()));
            }
        }

        /// <summary>  
        /// W/O 예약
        /// </summary>
        private void PopupReservation()
        {
            if (!ValidationReservation()) return;

            CMM_WORKORDER_DRB WO = new CMM_WORKORDER_DRB();
            WO._EqptSegment = _equipmentSegmentCode;
            WO._ProcID = _processCode;
            WO._EqptID = _equipmentCode;

            WO.GetWorkOrdeCallBack(() =>
            {
                DataTable dt = WO.WOTable;

                if (dt == null || dt.Rows.Count == 0)
                {
                    // %1가 없습니다.
                    Util.MessageValidation("SFU1444", ObjectDic.Instance.GetObjectName("Work Order"));
                    return;
                }

                CMM_COM_ELEC_RESERVATION popupReservation = new CMM_COM_ELEC_RESERVATION { FrameOperation = FrameOperation };

                if (popupReservation != null)
                {
                    UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                    object[] Parameters = new object[10];
                    Parameters[0] = dt.Rows[0]["PRJT_NAME"].ToString();
                    Parameters[1] = dt.Rows[0]["PROD_VER_CODE"].ToString();
                    Parameters[2] = dt.Rows[0]["WOID"].ToString();
                    Parameters[3] = dt.Rows[0]["PRODID"].ToString();
                    Parameters[4] = dt.Rows[0]["ELECTYPE"].ToString();
                    Parameters[5] = dt.Rows[0]["LOTYNAME"].ToString();
                    Parameters[6] = dt.Rows[0]["EQPTID"].ToString();
                    Parameters[7] = _equipmentSegmentCode;
                    Parameters[8] = _processCode;
                    Parameters[9] = dt;

                    C1WindowExtension.SetParameters(popupReservation, Parameters);
                    popupReservation.Closed += new EventHandler(PopupReservation_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => popupReservation.ShowModal()));
                }
            });

        }

        private void PopupReservation_Closed(object sender, EventArgs e)
        {
            CMM_COM_ELEC_RESERVATION popup = sender as CMM_COM_ELEC_RESERVATION;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비 List 재조회
                //SetEquipment("C");
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        /// <summary>  
        /// FOIL 관리
        /// </summary>
        private void PopupFoil()
        {
            CMM_COM_FOIL popupFoil = new CMM_COM_FOIL { FrameOperation = FrameOperation };

            if (popupFoil != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                this.Dispatcher.BeginInvoke(new Action(() => popupFoil.ShowModal()));
            }
        }

        /// <summary>  
        /// Slurry 물성정보
        /// </summary>
        private void PopupSlurryConf()
        {
            CMM_ELEC_SLURRY_CONF popupSlurryConf = new CMM_ELEC_SLURRY_CONF { FrameOperation = FrameOperation };

            if (popupSlurryConf != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.CFG_AREA_ID;

                C1WindowExtension.SetParameters(popupSlurryConf, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupSlurryConf.ShowModal()));
            }
        }

        /// <summary>
        /// 코터 임의 Cut 생성
        /// </summary>
        private void PopupStartCoaterCut()
        {
            if (!ValidationStartCoaterCut()) return;

            CMM_ELEC_START_COAT_LOT popupStartCoaterCut = new CMM_ELEC_START_COAT_LOT { FrameOperation = FrameOperation };

            //// 선택된 대 LOT
            //var SelectRow = DgProductLot.Selection.SelectedRows.ToList();

            if (popupStartCoaterCut != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[5];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _equipmentName;
                // 2021.12.28. 코터 임의 CUT 생성 오류 수정 - LOTID -> CUTID 변경
                //Parameters[2] = _dvProductLot["LOTID"].ToString();
                Parameters[2] = _dvProductLot["LOTID_LARGE"].ToString();
                Parameters[3] = false;                              // 단면 = Y", 양면 = "N"
                Parameters[4] = "";                                 // Top, Back

                C1WindowExtension.SetParameters(popupStartCoaterCut, Parameters);

                popupStartCoaterCut.Closed += new EventHandler(PopupStartCoaterCut_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartCoaterCut.ShowModal()));
            }
        }

        private void PopupStartCoaterCut_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_START_COAT_LOT popup = sender as CMM_ELEC_START_COAT_LOT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                SelectProductLot(string.Empty);
                // 생산 Lot 
                SetProductLotList();

                // DataTable dt = DataTableConverter.Convert(DgProductLot.ItemsSource);
                // returnLargeLotID, returnOutLotID
                // 기존 대 LOT에 체크 ??
            }
        }

        /// <summary>
        /// 무지부 방향설정
        /// </summary>
        private void PopupWorkHalfSlitSide()
        {
            if (!ValidationWorkHalfSlitSide()) return;

            //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
            if (_util.IsCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(_processCode, Process.ROLL_PRESSING))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ESWA popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING_ESWA { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag;

                    C1WindowExtension.SetParameters(popupWorkHalfSlitSide, Parameters);

                    popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
                }
            }
            // 무지부/권취 방향 2종류 모두 사용하는 AREA 일 경우
            else if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag;

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
                    UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = UcElectrodeProductLot.txtWorkHalfSlittingSide.Tag;

                    C1WindowExtension.SetParameters(popupWorkHalfSlitSide, Parameters);

                    popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
                }
            }
        }

        private void PopupWorkHalfSlitSide_Closed(object sender, EventArgs e)
        {
            //CSR : E20230210-000354 - [ESWA] 롤프레스 자동 연결을 위한 언와인더 권출 방향 투입 로직 개선 건
            if (_util.IsCommonCodeUse("UNCOATED_UNWINDING_DIRECTION", LoginInfo.CFG_AREA_ID) && string.Equals(_processCode, Process.ROLL_PRESSING))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ESWA popup = sender as CMM_ELEC_WORK_HALF_SLITTING_ESWA;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                    UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
            // 무지부/권취 방향 2종류 모두 사용하는 AREA 일 경우
            else if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popup = sender as CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                    UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popup = sender as CMM_ELEC_WORK_HALF_SLITTING;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // 무지부 조회
                    GetWorkHalfSlittingSide();
                    UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
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
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[1];
                Parameters[0] = _equipmentCode;

                C1WindowExtension.SetParameters(popupEmSectionRollDirctn, Parameters);

                popupEmSectionRollDirctn.Closed += new EventHandler(PopupEmSectionRollDirctn_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEmSectionRollDirctn.ShowModal()));
            }
        }

        private void PopupEmSectionRollDirctn_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_EM_SECTION_ROLL_DIRCTN popup = sender as CMM_ELEC_EM_SECTION_ROLL_DIRCTN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        /// <summary>
        /// 물류반송현황
        /// </summary>
        private void PopupLogisStat()
        {
            if (!ValidationLogisStat()) return;

            CMM_ELEC_LOGIS_STAT popupLogisStat = new CMM_ELEC_LOGIS_STAT { FrameOperation = FrameOperation };

            //// 선택된 대 LOT
            //var SelectRow = DgProductLot.Selection.SelectedRows.ToList();

            if (popupLogisStat != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[1];
                Parameters[0] = _equipmentCode;

                C1WindowExtension.SetParameters(popupLogisStat, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupLogisStat.ShowModal()));
            }
        }

        /// <summary>
        /// Port별 Skid Type 설정
        /// </summary>
        private void PopupSkidTypeSettingByPort()
        {
            CMM_ELEC_SKIDTYPE_SETTING_PORT popupSkidTypeSettingByPort = new CMM_ELEC_SKIDTYPE_SETTING_PORT { FrameOperation = FrameOperation };

            if (popupSkidTypeSettingByPort != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.CFG_AREA_ID;

                C1WindowExtension.SetParameters(popupSkidTypeSettingByPort, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupSkidTypeSettingByPort.ShowModal()));
            }
        }

        /// <summary>
        /// 목시관리필요Lot 등록 
        /// </summary>
        private void PopupSlBatch()
        {
            if (!ValidationSlBatch()) return;

            CMM_ELEC_SLBATCH popupSlBatch = new CMM_ELEC_SLBATCH { FrameOperation = FrameOperation };

            if (popupSlBatch != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[1];
                Parameters[0] = _processCode;

                C1WindowExtension.SetParameters(popupSlBatch, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupSlBatch.ShowModal()));
            }
        }

        /// <summary>
        /// Scrap Lot 재생성 
        /// </summary>
        private void PopupScrapLot()
        {
            if (!ValidationScrapLot()) return;

            CMM_ELEC_RECREATE_SCRAP_LOT popupScrapLot = new CMM_ELEC_RECREATE_SCRAP_LOT { FrameOperation = FrameOperation };

            if (popupScrapLot != null)
            {
                UcElectrodeCommand.btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[3];
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _equipmentCode;
                Parameters[2] = _processCode;

                C1WindowExtension.SetParameters(popupScrapLot, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupScrapLot.ShowModal()));
            }

        }

        private void PopupPrintHistoryCard()
        {
            // 2022.1.7. ESNJ 1동 생산시스템 구축. 믹서의 경우 구분하여 출력 하는 기존 로직대로 추가
            if (string.Equals(_processCode, Process.MIXING))
            {
                LGC.GMES.MES.CMM001.CMM_ELEC_REPORT3 wndPopup1 = new LGC.GMES.MES.CMM001.CMM_ELEC_REPORT3();
                wndPopup1.FrameOperation = FrameOperation;

                if (wndPopup1 != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = _txtEndLotId; //LOT ID
                    Parameters[1] = _processCode; //PROCESS ID
                    Parameters[2] = "Y";    // 실적확정 여부

                    C1WindowExtension.SetParameters(wndPopup1, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup1.ShowModal()));
                }
            }
            else
            {
                CMM_ELEC_REPORT2 popupPrintHistoryCard = new CMM_ELEC_REPORT2();
                popupPrintHistoryCard.FrameOperation = FrameOperation;

                if (popupPrintHistoryCard != null)
                {
                    UcElectrodeCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[4];

                    // 2022.1.7. ESNJ 1동 생산시스템 구축. Process 가 Coating으로 고정, 실적 완료 후 ProductLot이 사라져 해당 항목을 가져오지 못하여 전극 이력카드 출력이 되지 않는 오류 수정    
                    //Parameters[0] = _dvProductLot["LOTID"].ToString(); //LOT ID
                    //Parameters[1] = Process.COATING; //PROCESS ID
                    //Parameters[2] = string.Empty;
                    //Parameters[3] = "Y";    // 실적확정 여부

                    Parameters[0] = _txtEndLotId; //LOT ID
                    Parameters[1] = _processCode; //PROCESS ID

                    if ((string.Equals(_processCode, Process.SLITTING) || string.Equals(_processCode, "E4200")) && !string.IsNullOrEmpty(Util.NVC(_txtEndLotCutId)))
                        Parameters[2] = Util.NVC(_txtEndLotCutId);
                    else
                        Parameters[2] = string.Empty;

                    Parameters[3] = "Y";    // 실적확정 여부

                    C1WindowExtension.SetParameters(popupPrintHistoryCard, Parameters);
                    popupPrintHistoryCard.Closed += new EventHandler(PopupPrintHistoryCard_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupPrintHistoryCard.ShowModal()));
                }
            }
        }

        private void PopupPrintHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_REPORT2 popupPrintHistoryCard = new CMM_ELEC_REPORT2();
            // CSR : [C20220812-000226] COATER popup alarm improvement
            // Slurry Popup실행
            if (popupPrintHistoryCard != null)
            {
                //if (popupPrintHistoryCard.DialogResult == MessageBoxResult.OK)
                //{
                if (_util.IsCommonCodeUse("COATER_SLURRY_POPUP", LoginInfo.CFG_AREA_ID) && string.Equals(_processCode, Process.COATING))
                {
                    SetCoaterSlurryPopup(dtSlurry);
                }
                //}
            }
        }
        #region -----------------------------RollMap Popup
        /// <summary>
        /// RollMap Closed
        /// </summary>
        private void PopupRollMap_Closed(object sender, EventArgs e)
        {
            ///////////////////// ROLLMAP 실적 수정시 불량/LOSS/물품청구 재조회

            if (grdProductionResult_Coating.Visibility == Visibility.Visible)
            {
                SetUserControlProductionResult();
            }
            else if (grdProductionResult_RollPressing.Visibility == Visibility.Visible)
            {
                //SetUserControlProductionResult();
            }
        }

        /// <summary>
        /// RollMapInputMaterial
        /// </summary>
        private void PopupRollMapInputMaterial()
        {
            if (string.IsNullOrEmpty(_equipmentCode))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

            LGC.GMES.MES.CMM001.Popup.CMM_ELEC_MTRL_INPUT_INFO poupRollMapInputMaterial = new LGC.GMES.MES.CMM001.Popup.CMM_ELEC_MTRL_INPUT_INFO();
            poupRollMapInputMaterial.FrameOperation = FrameOperation;

            if (poupRollMapInputMaterial != null)
            {
                (grdCommand.Children[0] as UcElectrodeCommand).btnExtra.IsDropDownOpen = false;
                object[] Parameters = new object[10];
                Parameters[0] = _equipmentSegmentCode;
                Parameters[1] = _equipmentCode;
                Parameters[2] = _processCode;
                Parameters[3] = iRow < 0 ? "" : Util.NVC(_dvProductLot["LOTID"].ToString());

                C1WindowExtension.SetParameters(poupRollMapInputMaterial, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => poupRollMapInputMaterial.ShowModal()));
            }
        }

        /// <summary>
        /// Popup RollMap Hold Closed 
        /// </summary>
        private void popupRollMapHold_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_HOLD rollMapHoldPopup = sender as CMM_ROLLMAP_HOLD;
            if (rollMapHoldPopup.DialogResult == MessageBoxResult.OK)
            {
                _isPostingHold = rollMapHoldPopup.HoldCheck;
                ValidateCarrierCTUnloader();
            }
        }

        /// <summary>
        /// Popup 버퍼 수동 초기화
        /// </summary>
        private void PopupSlurryBufferManualInit()
        {
            try
            {
                if (string.IsNullOrEmpty(_equipmentCode))
                {
                    Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                    return;
                }

                CMM_ELEC_COATER_SLURRY_BUFFER_INIT popup = new CMM_ELEC_COATER_SLURRY_BUFFER_INIT { FrameOperation = FrameOperation };

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = LoginInfo.CFG_AREA_ID;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    if (popup != null)
                    {
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        ///// <summary>
        ///// PopupUpdateLaneNo
        ///// </summary>
        //private void PopupUpdateLaneNo()
        //{
        //    CMM_COM_UPDATE_LANENO popUpdateLaneNo = new CMM_COM_UPDATE_LANENO();
        //    popUpdateLaneNo.FrameOperation = FrameOperation;

        //    object[] parameters = new object[1];
        //    parameters[0] = _processCode;
        //    C1WindowExtension.SetParameters(popUpdateLaneNo, parameters);

        //    popUpdateLaneNo.Closed += popupUpdateLaneNo_Closed;
        //    Dispatcher.BeginInvoke(new Action(() => popUpdateLaneNo.ShowModal()));
        //}

        ///// <summary>
        ///// popupRollMapHold_Closed
        ///// </summary>
        //private void popupUpdateLaneNo_Closed(object sender, EventArgs e)
        //{
        //    CMM_COM_UPDATE_LANENO popUpdateLaneNo = sender as CMM_COM_UPDATE_LANENO;
        //    if (popUpdateLaneNo != null && popUpdateLaneNo.DialogResult == MessageBoxResult.OK)
        //    {
        //    }
        //}
        #endregion

        #endregion

        #region =============================Mixing
        /// <summary>
        /// 작업시작
        /// </summary>
        private void PopupStartMixing()
        {
            if (!ValidationStartMixing()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQPTID", _equipmentCode);
            dicParam.Add("ELEC", ""); //극성정보

            ELEC003_LOTSTART_MIXING popupStartMixing = new ELEC003_LOTSTART_MIXING(dicParam);
            popupStartMixing.FrameOperation = FrameOperation;

            if (popupStartMixing != null)
            {
                popupStartMixing.Closed += new EventHandler(PopupStartMixing_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartMixing.ShowModal()));
                popupStartMixing.CenterOnScreen();
            }
        }

        private void PopupStartMixing_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_MIXING popup = sender as ELEC003_LOTSTART_MIXING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndMixing()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndMixing = new CMM_COM_EQPT_END();
            popupEqptEndMixing.FrameOperation = FrameOperation;

            if (popupEqptEndMixing != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _dvProductLot["LOTID"].ToString();
                Parameters[3] = UcResult_Mixing.txtProductionQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_Mixing.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = string.Empty;
                Parameters[6] = string.Empty;                                                                              // Util.NVC(txtParentQty_M.Value);

                C1WindowExtension.SetParameters(popupEqptEndMixing, Parameters);

                popupEqptEndMixing.Closed += new EventHandler(PopupEqptEndMixing_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndMixing.ShowModal()));
                popupEqptEndMixing.CenterOnScreen();
            }
        }

        private void PopupEqptEndMixing_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }

        private void ConfirmCheck()
        {
            if (_processCode != Process.MIXING)
            {
                // 선분산믹서, B/S, CMC , Roll Pressing
                PopupConfirmUser();
                return;
            }

            LGC.GMES.MES.CMM001.CMM_ELEC_MIXER_BATCH mixerConfirm = new CMM_ELEC_MIXER_BATCH();
            mixerConfirm.FrameOperation = FrameOperation;
            if (mixerConfirm != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Util.NVC(_dvProductLot["PRJT_NAME"]);
                Parameters[1] = Util.NVC(UcResult_Mixing.txtVersion.Text);
                // 2019-08-28 오화백  LOTID, EQPTID 정보 파라미터 추가
                Parameters[2] = Util.NVC(_dvProductLot["LOTID"]);
                Parameters[3] = _equipmentCode;
                C1WindowExtension.SetParameters(mixerConfirm, Parameters);

                mixerConfirm.Closed += new EventHandler(OnCloseMixerConfirm);
                this.Dispatcher.BeginInvoke(new Action(() => mixerConfirm.ShowModal()));
            }
        }

        private void OnCloseMixerConfirm(object sender, EventArgs e)
        {
            CMM_ELEC_MIXER_BATCH window = sender as CMM_ELEC_MIXER_BATCH;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                string sRemark = Util.NVC(DataTableConverter.GetValue(UcResult_Mixing.dgRemark.Rows[1].DataItem, "REMARK"));
                DataTableConverter.SetValue(UcResult_Mixing.dgRemark.Rows[1].DataItem, "REMARK", window._ConfirmMsg + sRemark);

                _isLastBatch = window._IsLastBatch;
                PopupConfirmUser();
            }
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
                        CMM_COM_AUTH_CONFIRM authConfirm = new CMM_COM_AUTH_CONFIRM();
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
            CMM_COM_AUTH_CONFIRM window = sender as CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                CheckSpecOutHold(() => { ConfirmCheck(); });
            }
        }

        #endregion

        #region =============================Coating
        private void PopupStartCoating()
        {
            if (!ValidationStartCoating()) return;

            string sLargeLotID = string.Empty;
            string sWoID = string.Empty;

            if (_dvProductLot != null)
            {
                sLargeLotID = _dvProductLot["LOTID_LARGE"].ToString();
                sWoID = _dvProductLot["WO_DETL_ID"].ToString();
            }

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("EQPTID", _equipmentCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);
            dicParam.Add("LARGELOT", sLargeLotID);
            dicParam.Add("WODETIL", sWoID);
            dicParam.Add("COATSIDE", "");
            dicParam.Add("SINGL", "");
            dicParam.Add("LOTID_PR", "");

            ELEC003_LOTSTART_COATING popupStartCoating = new ELEC003_LOTSTART_COATING(dicParam);
            popupStartCoating.FrameOperation = FrameOperation;
            popupStartCoating.IsSingleCoater = false;

            if (popupStartCoating != null)
            {
                popupStartCoating.Closed += new EventHandler(PopupStartCoating_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartCoating.ShowModal()));
                popupStartCoating.CenterOnScreen();
            }
        }

        private void PopupStartCoating_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_COATING popup = sender as ELEC003_LOTSTART_COATING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 대Lot 생성시 설비 조회
                if (popup.LargeLotCreate)
                {
                    //SetEquipment("C", false);
                    UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }

                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndCoating()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END_COATER popupEqptEndCoating = new CMM_COM_EQPT_END_COATER();
            popupEqptEndCoating.FrameOperation = FrameOperation;

            if (popupEqptEndCoating != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _dvProductLot["LOTID"].ToString();
                Parameters[3] = UcResult_CoatingAuto.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_CoatingAuto.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_CoatingAuto.txtGoodQty.Value.ToString();
                Parameters[7] = UcResult_CoatingAuto.chkFinalCut.IsChecked == true ? "Y" : "N";

                C1WindowExtension.SetParameters(popupEqptEndCoating, Parameters);

                popupEqptEndCoating.Closed += new EventHandler(PopupEqptEndCoating_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndCoating.ShowModal()));
                popupEqptEndCoating.CenterOnScreen();
            }
        }

        private void PopupEqptEndCoating_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END_COATER popup = sender as CMM_COM_EQPT_END_COATER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }

        /// <summary>
        /// 공정이동
        /// </summary>
        private void PopupElecHold()
        {
            CMM_ELEC_HOLD_YN popupElecHold = new CMM_ELEC_HOLD_YN();
            popupElecHold.FrameOperation = FrameOperation;

            if (popupElecHold != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _dvProductLot["LOTID"].ToString();

                C1WindowExtension.SetParameters(popupElecHold, Parameters);

                popupElecHold.Closed += new EventHandler(PopupElecHold_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupElecHold.ShowModal()));
                popupElecHold.CenterOnScreen();
            }
        }

        private void PopupElecHold_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_HOLD_YN popup = sender as CMM_ELEC_HOLD_YN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _isPostingHold = popup.HOLDYNCHK;
                ValidateCarrierCTUnloader();
            }
        }

        private void PopupElecCstRelation()
        {
            CMM_ELEC_CST_RELATION popupElecCstRelation = new CMM_ELEC_CST_RELATION();
            popupElecCstRelation.FrameOperation = FrameOperation;

            if (popupElecCstRelation != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = _dvProductLot["LOTID"].ToString();
                Parameters[1] = _equipmentCode;

                C1WindowExtension.SetParameters(popupElecCstRelation, Parameters);

                popupElecCstRelation.Closed += new EventHandler(PopupElecCstRelation_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupElecCstRelation.ShowModal()));
                popupElecCstRelation.CenterOnScreen();
            }
        }

        private void PopupElecCstRelation_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_CST_RELATION popup = sender as CMM_ELEC_CST_RELATION;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                PopupConfirmUser();
            }
        }

        #endregion

        #region =============================InsCoating
        private void PopupStartInsCoating()
        {
            if (!ValidationStartInsCoating()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);
            dicParam.Add("EQPTID", _equipmentCode);
            dicParam.Add("COAT_SIDE_TYPE", cboCoatSide.GetStringValue());

            ELEC003_LOTSTART_INSCOATING popupStartInsCoating = new ELEC003_LOTSTART_INSCOATING(dicParam);
            popupStartInsCoating.FrameOperation = FrameOperation;

            if (popupStartInsCoating != null)
            {
                popupStartInsCoating.Closed += new EventHandler(PopupStartInsCoating_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartInsCoating.ShowModal()));
                popupStartInsCoating.CenterOnScreen();
            }
        }

        private void PopupStartInsCoating_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_INSCOATING popup = sender as ELEC003_LOTSTART_INSCOATING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }
        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndInsCoating()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndInsCoating = new CMM_COM_EQPT_END();
            popupEqptEndInsCoating.FrameOperation = FrameOperation;

            string endLotID = "";

            for (int i = 0; i < UcResult_InsCoating.dgProductResult.Rows.Count; i++)
            {
                if (UcResult_InsCoating.dgProductResult.Rows[i].DataItem != null)
                {
                    if (!DataTableConverter.GetValue(UcResult_InsCoating.dgProductResult.Rows[i].DataItem, "LOTID").Equals(""))  //_Shift
                    {
                        endLotID = DataTableConverter.GetValue(UcResult_InsCoating.dgProductResult.Rows[i].DataItem, "LOTID").ToString() + "," + endLotID;
                    }
                }
            }

            if (popupEqptEndInsCoating != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = endLotID;
                Parameters[3] = UcResult_InsCoating.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_InsCoating.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_InsCoating.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndInsCoating, Parameters);

                popupEqptEndInsCoating.Closed += new EventHandler(PopupEqptEndInsCoating_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndInsCoating.ShowModal()));
                popupEqptEndInsCoating.CenterOnScreen();
            }
        }

        private void PopupEqptEndInsCoating_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }
        #endregion

        #region =============================Half Slitting
        /// <summary>
        /// 이동, 이동취소
        /// </summary>
        private void PopupMove(bool bMove)
        {
            if (!ValidationMove()) return;

            ELEC003_MOVE_CANCEL_HALFSLITTING popupMove = new ELEC003_MOVE_CANCEL_HALFSLITTING();
            popupMove.FrameOperation = FrameOperation;

            if (popupMove != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = _processCode;
                Parameters[1] = _equipmentSegmentCode;
                Parameters[2] = bMove ? "Y" : "N";                // 이동 Y, 이동취소 N

                C1WindowExtension.SetParameters(popupMove, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupMove.ShowModal()));
                popupMove.CenterOnScreen();
            }
        }

        private void PopupStartHalfSlitting()
        {
            if (!ValidationStartHalfSlitting()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);
            dicParam.Add("EQPTID", _equipmentCode);

            ELEC003_LOTSTART_HALFSLITTING popupStartHalfSlitting = new ELEC003_LOTSTART_HALFSLITTING(dicParam);
            popupStartHalfSlitting.FrameOperation = FrameOperation;

            if (popupStartHalfSlitting != null)
            {
                popupStartHalfSlitting.Closed += new EventHandler(PopupStartHalfSlitting_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartHalfSlitting.ShowModal()));
                popupStartHalfSlitting.CenterOnScreen();
            }
        }

        private void PopupStartHalfSlitting_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_HALFSLITTING popup = sender as ELEC003_LOTSTART_HALFSLITTING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndHalfSlitting()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndHalfSlitting = new CMM_COM_EQPT_END();
            popupEqptEndHalfSlitting.FrameOperation = FrameOperation;

            string endLotID = "";

            for (int i = 0; i < UcResult_HalfSlitting.dgProductResult.Rows.Count; i++)
            {
                if (UcResult_HalfSlitting.dgProductResult.Rows[i].DataItem != null)
                {
                    if (!DataTableConverter.GetValue(UcResult_HalfSlitting.dgProductResult.Rows[i].DataItem, "LOTID").Equals(""))  //_Shift
                    {
                        endLotID = DataTableConverter.GetValue(UcResult_HalfSlitting.dgProductResult.Rows[i].DataItem, "LOTID").ToString() + "," + endLotID;
                    }
                }
            }

            if (popupEqptEndHalfSlitting != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = endLotID;
                Parameters[3] = UcResult_HalfSlitting.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_HalfSlitting.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_HalfSlitting.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndHalfSlitting, Parameters);

                popupEqptEndHalfSlitting.Closed += new EventHandler(PopupEqptEndHalfSlitting_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndHalfSlitting.ShowModal()));
                popupEqptEndHalfSlitting.CenterOnScreen();
            }
        }

        private void PopupEqptEndHalfSlitting_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }

        #endregion

        #region =============================Slitting
        private void PopupStartSlitting()
        {
            if (!ValidationStartSlitting()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);
            dicParam.Add("EQPTID", _equipmentCode);

            ELEC003_LOTSTART_SLITTING popupStartSlitting = new ELEC003_LOTSTART_SLITTING(dicParam);
            popupStartSlitting.FrameOperation = FrameOperation;

            if (popupStartSlitting != null)
            {
                popupStartSlitting.Closed += new EventHandler(PopupStartSlitting_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartSlitting.ShowModal()));
                popupStartSlitting.CenterOnScreen();
            }
        }

        private void PopupStartSlitting_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_SLITTING popup = sender as ELEC003_LOTSTART_SLITTING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }
        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndSlitting()
        {
            if (!ValidationEqptEnd()) return;

            //if (!Validation_Slit_GRD_EqptEnd()) return;     //슬리팅 전극 등급 정보 확인

            CMM_COM_EQPT_END popupEqptEndSlitting = new CMM_COM_EQPT_END();
            popupEqptEndSlitting.FrameOperation = FrameOperation;

            string endLotID = "";

            for (int i = 0; i < UcResult_Slitting.dgProductResult.Rows.Count; i++)
            {
                if (UcResult_Slitting.dgProductResult.Rows[i].DataItem != null)
                {
                    if (!DataTableConverter.GetValue(UcResult_Slitting.dgProductResult.Rows[i].DataItem, "LOTID").Equals(""))  //_Shift
                    {
                        endLotID = DataTableConverter.GetValue(UcResult_Slitting.dgProductResult.Rows[i].DataItem, "LOTID").ToString() + "," + endLotID;
                    }
                }
            }

            if (popupEqptEndSlitting != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = endLotID;
                Parameters[3] = UcResult_Slitting.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_Slitting.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_Slitting.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndSlitting, Parameters);

                popupEqptEndSlitting.Closed += new EventHandler(PopupEqptEndSlitting_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndSlitting.ShowModal()));
                popupEqptEndSlitting.CenterOnScreen();
            }
        }

        private void PopupEqptEndSlitting_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }
        #endregion

        #region =============================Roll Pressing
        private void PopupDfctLanePancake()
        {
            if (!ValidationPopupDfctLanePancake()) return;

            CMM_ELEC_REG_DFCT_LANE_PANCAKE popupDfctLanePancake = new CMM_ELEC_REG_DFCT_LANE_PANCAKE { FrameOperation = FrameOperation };

            if (popupDfctLanePancake != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Util.NVC(_dvProductLot["LOTID"]);
                C1WindowExtension.SetParameters(popupDfctLanePancake, Parameters);

                popupDfctLanePancake.Closed += new EventHandler(PopupDfctLanePancake_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupDfctLanePancake.ShowModal()));
            }
        }

        private void PopupDfctLanePancake_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_REG_DFCT_LANE_PANCAKE popup = sender as CMM_ELEC_REG_DFCT_LANE_PANCAKE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 라벨자동발행
                LabelAuto();

                // 화면 전환
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(Util.NVC(_dvProductLot["LOTID"]));
                SetProductLotList();
            }
        }

        private void PopupStartRollPressing()
        {
            if (!ValidationStartRollPressing()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQPTID", _equipmentCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);

            //if (_dvProductLot != null)
            //    dicParam.Add("LOTID", _dvProductLot["LOTID_PR"].ToString());

            DataRow[] dr = DataTableConverter.Convert(DgProductLot.ItemsSource).Select("CHK = 1");
            if (dr.Length > 0)
                dicParam.Add("LOTID", dr[0]["LOTID_PR"].ToString());

            ELEC003_LOTSTART_ROLLPRESSING popupStartRollPressing = new ELEC003_LOTSTART_ROLLPRESSING(dicParam);
            popupStartRollPressing.FrameOperation = FrameOperation;

            if (popupStartRollPressing != null)
            {
                popupStartRollPressing.Closed += new EventHandler(PopupStartRollPressing_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartRollPressing.ShowModal()));
                popupStartRollPressing.CenterOnScreen();
            }
        }

        private void PopupStartRollPressing_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_ROLLPRESSING popup = sender as ELEC003_LOTSTART_ROLLPRESSING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcElectrodeProductLot.chkWait.Checked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.IsChecked = false;
                UcElectrodeProductLot.chkWait.Checked += UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked += UcElectrodeProductLot.chkWait_Checked;

                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }

        private void PopupStartReWinding()
        {
            if (!ValidationStartReWinding()) return;

            ELEC003_LOTSTART_REWINDING popupStartReWinding = new ELEC003_LOTSTART_REWINDING();
            popupStartReWinding.FrameOperation = FrameOperation;

            if (popupStartReWinding != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = _processCode;
                Parameters[1] = _equipmentSegmentCode;
                Parameters[2] = _equipmentCode;
                Parameters[3] = _equipmentName;
                C1WindowExtension.SetParameters(popupStartReWinding, Parameters);

                popupStartReWinding.Closed += new EventHandler(PopupStartReWinding_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartReWinding.ShowModal()));
                popupStartReWinding.CenterOnScreen();
            }
        }

        private void PopupStartReWinding_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_REWINDING popup = sender as ELEC003_LOTSTART_REWINDING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 대기 Lot 재조회
                SelectProductLot(null);
                SetProductLotList(null);

                // 설비 재조회
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndRollPressing()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndRollPressing = new CMM_COM_EQPT_END();
            popupEqptEndRollPressing.FrameOperation = FrameOperation;

            if (popupEqptEndRollPressing != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _dvProductLot["LOTID"].ToString();
                Parameters[3] = UcResult_RollPressing.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_RollPressing.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_RollPressing.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndRollPressing, Parameters);

                popupEqptEndRollPressing.Closed += new EventHandler(PopupEqptEndRollPressing_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndRollPressing.ShowModal()));
                popupEqptEndRollPressing.CenterOnScreen();
            }
        }

        private void PopupEqptEndRollPressing_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }
        #endregion

        #region =============================ReWinder


        private void PopupStartReWinder()
        {
            if (!ValidationStartReWinder()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQPTID", _equipmentCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);

            //if (_dvProductLot != null)
            //    dicParam.Add("LOTID", _dvProductLot["LOTID_PR"].ToString());

            DataRow[] dr = DataTableConverter.Convert(DgProductLot.ItemsSource).Select("CHK = 1");
            if (dr.Length > 0)
                dicParam.Add("LOTID", dr[0]["LOTID_PR"].ToString());

            ELEC003_LOTSTART_REWINDER popupStartReWinder = new ELEC003_LOTSTART_REWINDER(dicParam);
            popupStartReWinder.FrameOperation = FrameOperation;

            if (popupStartReWinder != null)
            {
                popupStartReWinder.Closed += new EventHandler(PopupStartReWinder_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartReWinder.ShowModal()));
                popupStartReWinder.CenterOnScreen();
            }
        }

        private void PopupStartReWinder_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_REWINDER popup = sender as ELEC003_LOTSTART_REWINDER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcElectrodeProductLot.chkWait.Checked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.IsChecked = false;
                UcElectrodeProductLot.chkWait.Checked += UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked += UcElectrodeProductLot.chkWait_Checked;

                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndReWinder()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndReWinder = new CMM_COM_EQPT_END();
            popupEqptEndReWinder.FrameOperation = FrameOperation;

            if (popupEqptEndReWinder != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _dvProductLot["LOTID"].ToString();
                Parameters[3] = UcResult_ReWinder.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_ReWinder.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_ReWinder.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndReWinder, Parameters);

                popupEqptEndReWinder.Closed += new EventHandler(PopupEqptEndReWinder_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndReWinder.ShowModal()));
                popupEqptEndReWinder.CenterOnScreen();
            }
        }

        private void PopupEqptEndReWinder_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }
        #endregion

        #region =============================Taping
        private void PopupStartTaping()
        {
            if (!ValidationStartTaping()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQPTID", _equipmentCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);

            DataRow[] dr = DataTableConverter.Convert(DgProductLot.ItemsSource).Select("CHK = 1");
            if (dr.Length > 0)
                dicParam.Add("LOTID", dr[0]["LOTID_PR"].ToString());

            ELEC003_LOTSTART_TAPING popupStartTaping = new ELEC003_LOTSTART_TAPING(dicParam);
            popupStartTaping.FrameOperation = FrameOperation;

            if (popupStartTaping != null)
            {
                popupStartTaping.Closed += new EventHandler(PopupStartTaping_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartTaping.ShowModal()));
                popupStartTaping.CenterOnScreen();
            }
        }

        private void PopupStartTaping_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_TAPING popup = sender as ELEC003_LOTSTART_TAPING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcElectrodeProductLot.chkWait.Checked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.IsChecked = false;
                UcElectrodeProductLot.chkWait.Checked += UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked += UcElectrodeProductLot.chkWait_Checked;

                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndTaping()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndTaping = new CMM_COM_EQPT_END();
            popupEqptEndTaping.FrameOperation = FrameOperation;

            if (popupEqptEndTaping != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _dvProductLot["LOTID"].ToString();
                Parameters[3] = UcResult_Taping.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_Taping.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_Taping.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndTaping, Parameters);

                popupEqptEndTaping.Closed += new EventHandler(PopupEqptEndTaping_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndTaping.ShowModal()));
                popupEqptEndTaping.CenterOnScreen();
            }
        }

        private void PopupEqptEndTaping_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }
        #endregion

        #region =============================Heat Treatment
        private void PopupStartHeatTreatment()
        {
            if (!ValidationStartHeatTreatment()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQPTID", _equipmentCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);

            DataRow[] dr = DataTableConverter.Convert(DgProductLot.ItemsSource).Select("CHK = 1");
            if (dr.Length > 0)
                dicParam.Add("LOTID", dr[0]["LOTID_PR"].ToString());

            ELEC003_LOTSTART_HEATTREATMENT popupStartHeatTreatment = new ELEC003_LOTSTART_HEATTREATMENT(dicParam);
            popupStartHeatTreatment.FrameOperation = FrameOperation;

            if (popupStartHeatTreatment != null)
            {
                popupStartHeatTreatment.Closed += new EventHandler(PopupStartHeatTreatment_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartHeatTreatment.ShowModal()));
                popupStartHeatTreatment.CenterOnScreen();
            }
        }

        private void PopupStartHeatTreatment_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_HEATTREATMENT popup = sender as ELEC003_LOTSTART_HEATTREATMENT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcElectrodeProductLot.chkWait.Checked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked -= UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.IsChecked = false;
                UcElectrodeProductLot.chkWait.Checked += UcElectrodeProductLot.chkWait_Checked;
                UcElectrodeProductLot.chkWait.Unchecked += UcElectrodeProductLot.chkWait_Checked;

                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }

        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndHeatTreatment()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndHeatTreatment = new CMM_COM_EQPT_END();
            popupEqptEndHeatTreatment.FrameOperation = FrameOperation;

            if (popupEqptEndHeatTreatment != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = _dvProductLot["LOTID"].ToString();
                Parameters[3] = UcResult_HeatTreatment.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_HeatTreatment.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_HeatTreatment.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndHeatTreatment, Parameters);

                popupEqptEndHeatTreatment.Closed += new EventHandler(PopupEqptEndHeatTreatment_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndHeatTreatment.ShowModal()));
                popupEqptEndHeatTreatment.CenterOnScreen();
            }
        }

        private void PopupEqptEndHeatTreatment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }
        #endregion

        #region =============================재와인딩
        /// <summary>
        /// Merge Lot List =========================> 사용 안함
        /// </summary>
        private void PopupMergeLotList()
        {
            if (!ValidationMergeLotList()) return;

            ELEC003_REWINDING_MERGE_LOT popupMergeLotList = new ELEC003_REWINDING_MERGE_LOT();
            popupMergeLotList.FrameOperation = FrameOperation;

            if (popupMergeLotList != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = _dvProductLot["LOTID"].ToString();

                C1WindowExtension.SetParameters(popupMergeLotList, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popupMergeLotList.ShowModal()));
                popupMergeLotList.CenterOnScreen();
            }
        }

        /// <summary>
        /// 투입, 투입취소
        /// </summary>
        private void PopupInput(bool bInput)
        {
            if (!ValidationInput()) return;

            ELEC003_INPUT_CANCEL_REWINDING popupInput = new ELEC003_INPUT_CANCEL_REWINDING();
            popupInput.FrameOperation = FrameOperation;

            if (popupInput != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = _processCode;
                Parameters[1] = _equipmentSegmentCode;
                Parameters[2] = _equipmentCode;
                Parameters[3] = _equipmentName;
                Parameters[4] = _dvProductLot["LOTID"].ToString();
                Parameters[5] = _dvProductLot["WIPSEQ"].ToString();
                Parameters[6] = bInput ? "Y" : "N";                // 투입 Y, 투입취소 N
                Parameters[7] = _ldrLotIdentBasCode;

                C1WindowExtension.SetParameters(popupInput, Parameters);

                popupInput.Closed += new EventHandler(PopupInput_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupInput.ShowModal()));
                popupInput.CenterOnScreen();
            }
        }

        private void PopupInput_Closed(object sender, EventArgs e)
        {
            ELEC003_INPUT_CANCEL_REWINDING popup = sender as ELEC003_INPUT_CANCEL_REWINDING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비 재조회
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        #endregion

        //20211215 2차 Slitting 공정진척DRB 화면 개발 START
        #region =============================Slitting
        private void PopupStartTwoSlitting()
        {
            if (!ValidationStartTwoSlitting()) return;

            Dictionary<string, string> dicParam = new Dictionary<string, string>();

            dicParam.Add("PROCID", _processCode);
            dicParam.Add("EQSGID", _equipmentSegmentCode);
            dicParam.Add("EQPTID", _equipmentCode);

            ELEC003_LOTSTART_SLITTING popupStartTwoSlitting = new ELEC003_LOTSTART_SLITTING(dicParam);
            popupStartTwoSlitting.FrameOperation = FrameOperation;

            if (popupStartTwoSlitting != null)
            {
                popupStartTwoSlitting.Closed += new EventHandler(PopupStartTwoSlitting_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupStartTwoSlitting.ShowModal()));
                popupStartTwoSlitting.CenterOnScreen();
            }
        }

        private void PopupStartTwoSlitting_Closed(object sender, EventArgs e)
        {
            ELEC003_LOTSTART_SLITTING popup = sender as ELEC003_LOTSTART_SLITTING;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // 설비의 진행중인 Lot 검색
                string StartLotID = SelectProcLotID();

                // 생산 Lot 재조회
                SelectProductLot(StartLotID);
                SetProductLotList(StartLotID);
            }
        }
        /// <summary>
        /// 장비완료
        /// </summary>
        private void PopupEqptEndTwoSlitting()
        {
            if (!ValidationEqptEnd()) return;

            CMM_COM_EQPT_END popupEqptEndTwoSlitting = new CMM_COM_EQPT_END();
            popupEqptEndTwoSlitting.FrameOperation = FrameOperation;

            string endLotID = "";

            for (int i = 0; i < UcResult_TwoSlitting.dgProductResult.Rows.Count; i++)
            {
                if (UcResult_TwoSlitting.dgProductResult.Rows[i].DataItem != null)
                {
                    if (!DataTableConverter.GetValue(UcResult_TwoSlitting.dgProductResult.Rows[i].DataItem, "LOTID").Equals(""))  //_Shift
                    {
                        endLotID = DataTableConverter.GetValue(UcResult_TwoSlitting.dgProductResult.Rows[i].DataItem, "LOTID").ToString() + "," + endLotID;
                    }
                }
            }

            if (popupEqptEndTwoSlitting != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = _equipmentCode;
                Parameters[1] = _processCode;
                Parameters[2] = endLotID;
                Parameters[3] = UcResult_TwoSlitting.txtInputQty.Value.ToString();
                Parameters[4] = Util.NVC(UcResult_TwoSlitting.txtStartDateTime.Text);                               // 시작시간 추가
                Parameters[5] = _dvProductLot["CUT_ID"].ToString();
                Parameters[6] = UcResult_TwoSlitting.txtParentQty.Value.ToString();

                C1WindowExtension.SetParameters(popupEqptEndTwoSlitting, Parameters);

                popupEqptEndTwoSlitting.Closed += new EventHandler(PopupEqptEndTwoSlitting_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEqptEndTwoSlitting.ShowModal()));
                popupEqptEndTwoSlitting.CenterOnScreen();
            }
        }

        private void PopupEqptEndTwoSlitting_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPT_END popup = sender as CMM_COM_EQPT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ButtonProductList_Click(null, null);

                // 생산 Lot 재조회
                SelectProductLot(_dvProductLot["LOTID"].ToString());
                SetProductLotList(_dvProductLot["LOTID"].ToString());
            }
        }

        #endregion
        //20211215 2차 Slitting 공정진척DRB 화면 개발 END

        #region =============================Coater Slurry Popup
        private DataTable GetSlurryInfo(string sCotLotID)
        {
            try
            {
                DataRow[] drWO = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '11'");
                DataRow[] drSlryTop = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '5'");
                DataRow[] drSlryBack = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '6'");

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("EQPTID", typeof(string));
                dtIndata.Columns.Add("SLURRY_LOTID1", typeof(string));
                dtIndata.Columns.Add("SLURRY_LOTID2", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("PRODID", typeof(string));
                dtIndata.Columns.Add("COT_LOTID", typeof(string));
                dtIndata.Columns.Add("WOID", typeof(string));
                dtIndata.Columns.Add("WIDE_ROLL_FLAG", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["EQPTID"] = _equipmentCode;
                dr["SLURRY_LOTID1"] = drSlryTop[0]["VAL002"].ToString();
                dr["SLURRY_LOTID2"] = drSlryBack[0]["VAL002"].ToString();
                dr["MTRLID"] = drSlryTop[0]["MTRLID1"].ToString();
                dr["PRODID"] = drWO[0]["VAL007"].ToString();
                dr["COT_LOTID"] = sCotLotID;
                dr["WOID"] = _dvProductLot["WO_DETL_ID"].ToString();
                dr["WIDE_ROLL_FLAG"] = drWO[0]["WIDE_ROLL_FLAG"].ToString();
                dtIndata.Rows.Add(dr);

                return dtIndata;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void SetCoaterSlurryPopup(DataTable dtSlurry)
        {
            // CSR : [C20220812-000226] - COATER pop-up alarm improvemen
            string sSlryTop = string.Empty;
            string sSlryBack = string.Empty;
            string sChkSlurry = string.Empty;
            string sMessage = string.Empty;

            int iChkFlag = 0;

            bool isPopup = false; //Popup 진행여부

            MessageBoxButton Msgbtn = new MessageBoxButton();

            // 선택Lot 미존재 Slurry Popup 미실행
            if (dtSlurry == null)
            {
                return;
            }

            // W/O 미존재 Slurry Popup 미실행
            if (string.IsNullOrEmpty(dtSlurry.Rows[0]["WOID"].ToString()))
            {
                return;
            }

            sSlryTop = dtSlurry.Rows[0]["SLURRY_LOTID1"].ToString();
            sSlryBack = dtSlurry.Rows[0]["SLURRY_LOTID2"].ToString();

            // Slurry 데이터 없으면 미처리
            if (string.IsNullOrEmpty(sSlryTop))
            {
                return;
            }

            if (string.IsNullOrEmpty(sSlryBack))
                sSlryBack = sSlryTop;

            DataTable dtIndata = new DataTable();
            dtIndata.Columns.Add("AREAID", typeof(string));
            dtIndata.Columns.Add("EQPTID", typeof(string));
            dtIndata.Columns.Add("SLURRY_LOTID1", typeof(string));
            dtIndata.Columns.Add("SLURRY_LOTID2", typeof(string));
            dtIndata.Columns.Add("MTRLID", typeof(string));
            dtIndata.Columns.Add("PRODID", typeof(string));
            dtIndata.Columns.Add("COT_LOTID", typeof(string));
            dtIndata.Columns.Add("USERID", typeof(string));

            DataRow dr = dtIndata.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["EQPTID"] = _equipmentCode;
            dr["SLURRY_LOTID1"] = sSlryTop;
            dr["SLURRY_LOTID2"] = sSlryBack;
            dr["MTRLID"] = dtSlurry.Rows[0]["MTRLID"].ToString();
            dr["PRODID"] = dtSlurry.Rows[0]["PRODID"].ToString();
            dr["COT_LOTID"] = dtSlurry.Rows[0]["COT_LOTID"].ToString(); ;
            dr["USERID"] = LoginInfo.USERID;

            dtIndata.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_COATER_SLURRY_SUM", "RQSTDT", "RSLTDT", dtIndata);

            // 조회 데이터가 없으면 Popup 실행하지 않음
            if (dtRslt == null || dtRslt.Rows.Count == 0)
            {
                return;
            }

            // Slurry 재고량과 Coater 총 생산량 비교 값
            for (int i = 0; i < dtRslt.Rows.Count; i++)
            {
                if (dtRslt.Rows[i]["MIX_COA_YN"].ToString() == "Y")
                {
                    sChkSlurry = dtRslt.Rows[i]["LOTID"].ToString();
                    isPopup = true;

                    iChkFlag++;
                }
            }

            if (!isPopup) return;

            // Top & Back 둘다 Slurry 양품량 보다 Coater 생산량이 초과면 메시지만 표기
            if (iChkFlag == 2)
            {
                sMessage = MessageDic.Instance.GetMessage("SFU8471");
                Msgbtn = MessageBoxButton.OK;
            }
            else
            {
                string sLotID = string.Empty;
                if (sSlryTop == sChkSlurry)
                    if (!string.IsNullOrEmpty(sSlryTop) && !string.IsNullOrEmpty(sSlryBack))
                        sLotID = "SLURRY ( " + sChkSlurry + " )";
                    else
                        sLotID = "TOP ( " + sChkSlurry + " )";
                else
                    sLotID = "BACK ( " + sChkSlurry + " )";

                sMessage = MessageDic.Instance.GetMessage("SFU8470", new object[] { sLotID });
                Msgbtn = MessageBoxButton.OKCancel;
            }

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(sMessage, null, "Confirm", Msgbtn, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (iChkFlag != 2)
                    {
                        CMM_ELEC_SLURRY popup = new CMM_ELEC_SLURRY();
                        popup.FrameOperation = FrameOperation;

                        object[] Parameters = new object[8];
                        Parameters[0] = _processCode;
                        Parameters[1] = _equipmentSegmentCode;
                        Parameters[2] = dtSlurry.Rows[0]["WOID"].ToString();
                        Parameters[3] = _equipmentCode;
                        Parameters[4] = 0;
                        Parameters[5] = (sSlryTop == sChkSlurry) ? sSlryTop : sSlryBack;
                        Parameters[6] = "N";
                        Parameters[7] = dtSlurry.Rows[0]["WIDE_ROLL_FLAG"].ToString();

                        C1WindowExtension.SetParameters(popup, Parameters);
                        popup.Closed += new EventHandler(PopupSlurry_Closed);
                        Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                        popup.CenterOnScreen();
                    }
                }
            });
        }

        private void PopupSlurry_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_SLURRY popup = sender as CMM_ELEC_SLURRY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    _TopLot = popup._ReturnLotID;
                    _MtrlID = popup._ReturnPRODID;

                    if (popup._IsAllConfirm == true)
                    {
                        _BackLot = popup._ReturnLotID;
                    }

                    // Slurry만 장착 처리
                    if (popup._IsAllConfirm == true)
                        SaveMountChange(false, popup._IsSlurryTerm);
                    else
                        SaveMountChange(false, popup._IsSlurryTerm, false, popup._ReturnPosition == 0 ? true : false, popup._ReturnPosition == 1 ? true : false);
                }
            }
        }

        private void SaveMountChange(bool IsCurrentFoil = true, bool IsSlurryTerm = false, bool IsCoreTerm = false, bool IsTopSlurryChange = true, bool IsBackSlurryChange = true, bool IsAcoreChange = true, bool IsBcoreChange = true)
        {
            DataTable mountDt = GetCurrentMount(Util.NVC(_equipmentCode));

            BizDataSet bizRule = new BizDataSet();
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inProduct = indataSet.Tables.Add("INPUT_LOT");
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inProduct.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inProduct.Columns.Add("MTRLID", typeof(string));
            inProduct.Columns.Add("INPUT_LOTID", typeof(string));
            inProduct.Columns.Add("TERM_FLAG", typeof(string));

            DataTable inTable = indataSet.Tables["INDATA"];
            DataRow newRow = inTable.NewRow();
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            newRow["EQPTID"] = _equipmentCode;
            newRow["USERID"] = LoginInfo.USERID;

            inTable.Rows.Add(newRow);
            Grid grid = null;

            DataTable inMaterial = indataSet.Tables["INPUT_LOT"];

            // SET SLURRY
            DataRow[] rows = mountDt.Copy().Select("PRDT_CLSS_CODE = 'ASL'");
            if (rows.Length <= 0)
            {
                Util.MessageValidation("SFU2988", new object[] { _equipmentCode });  //해당 설비({%1})의 등록된 Slurry정보가 존재하지 않습니다.
                return;
            }

            if (_TopLot.Length > 0 && rows.Length > 0 && IsTopSlurryChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[0]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrEmpty(_TopLot) ? "A" : "S";
                newRow["MTRLID"] = _MtrlID;
                newRow["INPUT_LOTID"] = _TopLot;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            if (_BackLot.Length > 0 && rows.Length > 1 && IsBackSlurryChange == true)
            {
                newRow = null;
                newRow = inMaterial.NewRow();

                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(rows[1]["EQPT_MOUNT_PSTN_ID"]);
                newRow["EQPT_MOUNT_PSTN_STATE"] = !string.IsNullOrEmpty(_BackLot) ? "A" : "S";
                newRow["MTRLID"] = _MtrlID;
                newRow["INPUT_LOTID"] = _BackLot;
                newRow["TERM_FLAG"] = IsSlurryTerm ? "Y" : string.Empty;

                inMaterial.Rows.Add(newRow);
            }

            if (inMaterial.Rows.Count == 0)
                return;

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_USE_MTRL_LOT_CT", "INDATA,INPUT_LOT", null, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                UcElectrodeEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }, indataSet);
        }

        /// <summary>
        /// 설비 장착정보 조회
        /// </summary>
        private DataTable GetCurrentMount(string sEqptID)
        {
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = sEqptID;
                inTable.Rows.Add(newRow);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }
        #endregion

        #region 설비 등급 정보 저장

        private bool Validation_Slit_GRD_EqptEnd()
        {
            if (IsElectrodeGradeInfo())
            {
                try
                {
                    DataTable inTable = new DataTable { TableName = "RQSTDT" };
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("CUT_ID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CUT_ID"] = _dvProductLot["CUT_ID"].ToString();
                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GRD_JUDG_LOT_LANE_RESULT_OUT", "RQSTDT", "RSLTDT", inTable);

                    if (dtResult.Rows.Count > 0)
                    {
                        foreach (DataRow drow in dtResult.Rows)
                        {
                            if (string.IsNullOrEmpty(drow["GRD_JUDG_CODE"].ToString()))
                            {
                                Util.MessageValidation("SFU9333");//전극 등급 판정값이 없습니다.
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Util.MessageValidation("SFU9333");//전극 등급 판정값이 없습니다.
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return false;
                }
            }
            return true;
        }

        private bool IsElectrodeGradeInfo()
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "GRD_JUDG_DISP_AREA";
                dr["COM_CODE"] = _processCode;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                foreach (DataRow drow in dtResult.Rows)
                {
                    if (drow["ATTR1"].Equals("SAVE_CHECK"))
                    {
                        if (CommonVerify.HasTableRow(dtResult))
                            return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }
        #endregion

        //[E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        #region =========================== Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        /// <summary>
        /// Mixer 원재료 Lot 누락 Validation 기능 적용 여부
        /// </summary>
        private void CheckUseElecMtrlLotValidation()
        {
            try
            {
                _isELEC_MTRL_LOT_VALID_YN = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ELEC_MTRL_LOT_VALID_YN";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        if (dtResult.Rows[0]["ATTRIBUTE1"].ToString().Equals("Y"))
                            _isELEC_MTRL_LOT_VALID_YN = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// Mixer 원재료 Lot 누락 존재 유무 Check
        /// </summary>
        /// <returns></returns>
        private bool CheckMissedElecMtrlLot()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("WOID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = Util.NVC(_dvProductLot["LOTID"]);
                dr["WOID"] = Util.NVC(_dvProductLot["WOID"]);
                dr["EQPTID"] = _equipmentCode.ToString();
                dr["PRODID"] = Util.NVC(_dvProductLot["PRODID"]);
                dr["PROCID"] = _processCode.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_LOT_MISSED", "RQSTDT", "RSLTDT", RQSTDT);

                //미 투입자재 존재하면
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
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
        private void PopupInputMaterial()
        {

            CMM_ELEC_MISSED_MTRL_INPUT_LOT popupInputMaterial = new CMM_ELEC_MISSED_MTRL_INPUT_LOT { FrameOperation = FrameOperation };

            if (popupInputMaterial != null)
            {
                // [E20240712-001591]
                //object[] Parameters = new object[5];
                object[] Parameters = new object[6];

                Parameters[0] = Util.NVC(_dvProductLot["LOTID"]);
                Parameters[1] = Util.NVC(_dvProductLot["WOID"]);
                Parameters[2] = _equipmentCode.ToString();
                Parameters[3] = _processCode.ToString();
                Parameters[4] = Util.NVC(_dvProductLot["PRODID"]);
                // [E20240712-001591]
                Parameters[5] = "D";

                C1WindowExtension.SetParameters(popupInputMaterial, Parameters);

                popupInputMaterial.Closed += new EventHandler(PopupInputMaterial_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupInputMaterial.ShowModal()));
                popupInputMaterial.BringToFront();

            }
        }

        private void PopupInputMaterial_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_MISSED_MTRL_INPUT_LOT popup = sender as CMM_ELEC_MISSED_MTRL_INPUT_LOT;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                if (popup.bSaveConfirm == true) // PopUp에서 투입자재 저장을 했을 경우 투입자재 재조회
                {
                    UcResult_Mixing.SelectInputMaterialResult();
                }
            }
        }

        #endregion

        #endregion

        private string GetMessageWithSubstitution(string messageId, params object[] parameters)
        {
            DataTable dtMessage = GetMessageFromCommonCode(messageId);
            string message = string.Empty;

            if (CommonVerify.HasTableRow(dtMessage))
            {
                message = dtMessage.Rows[0]["CMCDNAME"].ToString();
            }

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

        private DataTable GetMessageFromCommonCode(string messageId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_HOLD_CONDITION_MSG";
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                var resultCmcode = (from t in dtResult.AsEnumerable()
                                    where messageId.Equals(t.Field<string>("CMCODE"))
                                    orderby t.Field<decimal>("CMCDSEQ") ascending
                                    select t);
                if (resultCmcode.Any())
                {
                    return resultCmcode.CopyToDataTable();
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }
        #endregion

        //nathan 2023.12.20 믹서 코터 배치연계 - start
        protected void ButtonbtnSlurryManualOutput_Click(object sender, EventArgs e)
        {

            try
            {
                CMM_ELEC_COATER_MANUAL_DRANE popup = new CMM_ELEC_COATER_MANUAL_DRANE { FrameOperation = FrameOperation };

                if (popup != null)
                {
                    object[] Parameters = new object[10];
                    //Parameters[0] = Util.GetCondition(cboAreaSupply);
                    //Parameters[1] = "A1ECOT001";
                    //popupSlurryMove.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                    C1WindowExtension.SetParameters(popup, Parameters);

                    if (popup != null)
                    {
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        protected void ButtonbtnSlurryManualInput_Click(object sender, EventArgs e)
        {

            try
            {
                CMM_ELEC_COATER_MANUAL_INPUT popup = new CMM_ELEC_COATER_MANUAL_INPUT { FrameOperation = FrameOperation };

                if (popup != null)
                {
                    object[] Parameters = new object[10];
                    //Parameters[0] = Util.GetCondition(cboAreaSupply);
                    //Parameters[1] = "A1ECOT001";
                    //popupSlurryMove.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                    C1WindowExtension.SetParameters(popup, Parameters);

                    if (popup != null)
                    {
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //nathan 2023.12.20 믹서 코터 배치연계 - end

        private bool IsChickSlurryInEQPT()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _equipmentCode;
                IndataTable.Rows.Add(Indata);

                DataTable OutdataTable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_SLURRY", "INDATA", "OUTDATA", IndataTable);

                for (int i = 0; i < OutdataTable.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(OutdataTable.Rows[i]["INPUT_LOTID"].ToString()))
                    {
                        Util.MessageValidation("SFU2988", _equipmentCode);
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return true;
        }
    }
}
