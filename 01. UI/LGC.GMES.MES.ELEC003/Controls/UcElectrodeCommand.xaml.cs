/*************************************************************************************
 Created Date : 2020.09.09
      Creator : 정문교
   Decription : 전극 공정진척 - 설비 Tree 
--------------------------------------------------------------------------------------
 [Change History]
 2020.09.09  정문교 : Initial Created.
 2021.02.11  정문교 : 시생산설정/해제 버튼 추가
 2021.07.01  조영대 : Slitting, 절연 Coter, 표면검사, TAPING 추가
 2021.08.12  김지은 : 시생산샘플설정/해제 기능 추가
 2021.10.25  조영대 : 권취 방향 변경 팝업 추가
 2021.12.15  강동희 : 2차 Slitting 공정진척DRB 화면 개발
 2022.02.16  김지은 : 시생산 샘플 버튼 권한부여 추가
 2022.06.19  윤기업 : RollMap 적용 
 2022.11.16  오화백 : 물류반송조건설정 버튼 및 대기재공조회 버튼 추가
 2023.07.26  김태우 : NFF DAM 믹서 E0430 추가
 2025.02.03  백상우 : [MES2.0] BS/CMC/Pre-Mix공정 작업조건등록 버튼 숨김처리/ Coating 공정 Scrap Lot 재생성 버튼 숨김 처리
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Reflection;
using System.Data;

namespace LGC.GMES.MES.ELEC003.Controls
{
    /// <summary>
    /// UcElectrodeCommand.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcElectrodeCommand : UserControl
    {
        #region Properties
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;
   
        public string ProcessCode { get; set; }

        private bool isManageSlittingSide = false;
        public bool IsManageSlittingSide
        {
            get { return isManageSlittingSide; }
            set { isManageSlittingSide = value; }
        }

        #endregion Properties

        public UcElectrodeCommand()
        {
            InitializeComponent();
        }
        
        public void SetButtonExtraVisibility(bool Main, bool ReWindingProcess = false)
        {
            btnExtra.Visibility = Visibility.Visible;                            // 추가 기능 버튼

            btnManualMode.Visibility = Visibility.Collapsed;                     // 수작업모드
            btnEqptIssue.Visibility = Visibility.Collapsed;                      // 설비특이사항
            btnFinalCut.Visibility = Visibility.Collapsed;                       // F/Cut변경
            btnCleanLot.Visibility = Visibility.Collapsed;                       // 대LOT삭제
            btnCancelFCut.Visibility = Visibility.Collapsed;                     // LOT종료취소
            btnCancelDelete.Visibility = Visibility.Collapsed;                   // 삭제Lot생성
            btnCut.Visibility = Visibility.Collapsed;                            // Cut
            btnInvoiceMaterial.Visibility = Visibility.Collapsed;                // 투입요청서
            btnEqptCond.Visibility = Visibility.Collapsed;                       // 작업조건등록
            btnMixConfirm.Visibility = Visibility.Collapsed;                     // 자주검사등록
            btnSamplingProd.Visibility = Visibility.Collapsed;                   // R/P 샘플링 제품등록
            btnProcReturn.Visibility = Visibility.Collapsed;                     // R/P 대기 변경
            btnSamplingProdT1.Visibility = Visibility.Collapsed;                 // 샘플링 제품등록
            btnMixerTankInfo.Visibility = Visibility.Collapsed;                  // Slurry정보
            btnReservation.Visibility = Visibility.Collapsed;                    // W/O 예약
            btnFoil.Visibility = Visibility.Collapsed;                           // FOIL 관리
            btnSlurryConf.Visibility = Visibility.Collapsed;                     // Slurry 물성정보
            btnStartCoaterCut.Visibility = Visibility.Collapsed;                 // 코터 임의 Cut 생성
            btnMovetoHalf.Visibility = Visibility.Collapsed;                     // 하프슬리터 이동
            btnWorkHalfSlitSide.Visibility = Visibility.Collapsed;               // 무지부 방향설정
            btnEmSectionRollDirctn.Visibility = Visibility.Collapsed;            // 권취방향변경
            btnLogisStat.Visibility = Visibility.Collapsed;                      // 물류반송현황
            btnSkidTypeSettingByPort.Visibility = Visibility.Collapsed;          // Port별 Skid Type 설정
            btnSlBatch.Visibility = Visibility.Collapsed;                        // 목시관리필요Lot 등록   
            btnCustomer.Visibility = Visibility.Collapsed;                       // 고객인증그룹조회
            btnScrapLot.Visibility = Visibility.Collapsed;                       // Scrap Lot 재생성
            btnWebBreak.Visibility = Visibility.Collapsed;                       // 단선추가
            btnPilotProdMode.Visibility = Visibility.Collapsed;                  // 시생산설정/해제
            btnPilotProdSPMode.Visibility = Visibility.Collapsed;                // 시생산샘플설정/해제
            btnShipmentModel.Visibility = Visibility.Collapsed;                  // 출하모델

            btnMove.Visibility = Visibility.Collapsed;                           // 이동
            btnMoveCancel.Visibility = Visibility.Collapsed;                     // 이동취소
            btnInput.Visibility = Visibility.Collapsed;                          // 추가투입
            btnInputCancel.Visibility = Visibility.Collapsed;                    // 추가투입취소
            btnStart.Visibility = Visibility.Collapsed;                          // 작업시작
            btnCancel.Visibility = Visibility.Collapsed;                         // 시작취소
            btnEqptEnd.Visibility = Visibility.Collapsed;                        // 장비완료

            btnEqptEndCancel.Visibility = Visibility.Collapsed;                  // 장비완료취소
            btnEndCancel.Visibility = Visibility.Collapsed;                      // 실적확정취소

            btnReturnCondition.Visibility = Visibility.Collapsed;                // 물류반송조건설정
            btnSearchWaitingWork.Visibility = Visibility.Collapsed;              // 대기재공조회

            btnSlurryManualOutput.Visibility = Visibility.Collapsed;        //슬러리수동배출   //nathan 2023.12.20 믹서 코터 배치연계 
            btnSlurryManualInput.Visibility = Visibility.Collapsed;        //슬러리수동재투입   //nathan 2023.12.20 믹서 코터 배치연계 

            btnSlurryBufferManualInit.Visibility = Visibility.Collapsed;                   //버퍼 수동 초기화 //minylee 2024.2.14 믹서 코터 배치 연계 고도화

            //btnUpdateLaneNo.Visibility = Visibility.Collapsed;                   // 슬리팅 레인 번호 변경

            if (Main)
            {
                // 무지부 방향 설정
                isManageSlittingSide = IsAreaCommonCodeUse("MNG_SLITTING_SIDE_AREA", ProcessCode);
                if (isManageSlittingSide)
                {
                    btnWorkHalfSlitSide.Visibility = Visibility.Visible;

                    // 권취 방향 변경
                    if (IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
                    {
                        btnEmSectionRollDirctn.Visibility = Visibility.Visible;
                    }
                }
            }

            if (ProcessCode == Process.MIXING)
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelFCut.Visibility = Visibility.Visible;                   // LOT종료취소
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnInvoiceMaterial.Visibility = Visibility.Visible;              // 투입요청서
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnMixConfirm.Visibility = Visibility.Visible;                   // 자주검사등록
                    //btnSlurryConf.Visibility = Visibility.Visible;                 // Slurry 물성정보 - MES2.0 숨김처리(이상권 책임 요청)
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소
                    if(IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }


                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }

            }
            else if (ProcessCode == Process.PRE_MIXING)
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelFCut.Visibility = Visibility.Visible;                   // LOT종료취소
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnInvoiceMaterial.Visibility = Visibility.Visible;              // 투입요청서
                    //btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록 
                    btnMixConfirm.Visibility = Visibility.Visible;                   // 자주검사등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }

            }
            else if (ProcessCode == Process.BS || ProcessCode == Process.CMC || ProcessCode == Process.InsulationMixing || ProcessCode == Process.DAM_MIXING)
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    if (ProcessCode == Process.InsulationMixing || ProcessCode == Process.DAM_MIXING)
                    {
                        btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    }
                    else
                    {
                        btnEqptCond.Visibility = Visibility.Collapsed;                   // 작업조건등록 숨김
                    }
                    btnMixConfirm.Visibility = Visibility.Visible;                   // 자주검사등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }

            }
            else if (ProcessCode == Process.COATING)
            {
                if (Main)
                {
                    //btnManualMode.Visibility = Visibility.Visible;                   // 수작업모드 : 작업시작, 시작취소, 장비완료취소 수작업으로 가능하게 버튼 활성화
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCleanLot.Visibility = Visibility.Visible;                     // 대LOT삭제
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnCancelFCut.Visibility = Visibility.Visible;                   // LOT종료취소
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnMixConfirm.Visibility = Visibility.Visible;                   // 자주검사등록
                    btnMixerTankInfo.Visibility = Visibility.Visible;                // Slurry정보
                    btnReservation.Visibility = Visibility.Visible;                  // W/O 예약
                  //btnFoil.Visibility = Visibility.Visible;                         // FOIL 관리
                    btnStartCoaterCut.Visibility = Visibility.Visible;               // 코터 임의 Cut 생성
                    btnLogisStat.Visibility = Visibility.Visible;                    // 물류반송현황
                    //btnScrapLot.Visibility = Visibility.Visible;                     // Scrap Lot 재생성
                    btnWebBreak.Visibility = Visibility.Visible;                     // 단선추가
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    btnSlurryManualOutput.Visibility = Visibility.Visible;        //슬러리수동배출   //nathan 2023.12.20 믹서 코터 배치연계 
                    btnSlurryManualInput.Visibility = Visibility.Visible;        //슬러리수동재투입   //nathan 2023.12.20 믹서 코터 배치연계 

                    btnSlurryBufferManualInit.Visibility = Visibility.Visible;      //버퍼 수동 초기화 //minylee 2024.2.14 믹서 코터 배치 연계 고도화

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEndCancel.Visibility = Visibility.Visible;                    // 실적확정취소
                }
            }
            else if (ProcessCode == Process.INS_COATING)
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnCancelFCut.Visibility = Visibility.Visible;                   // LOT종료취소
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEndCancel.Visibility = Visibility.Visible;                    // 실적확정취소
                }
            }
            else if (ProcessCode == Process.HALF_SLITTING)
            {
                if (Main)
                {
                    //btnManualMode.Visibility = Visibility.Visible;                   // 수작업모드 : 작업시작, 시작취소, 장비완료취소 수작업으로 가능하게 버튼 활성화
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnCut.Visibility = Visibility.Visible;                          // Cut
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnMove.Visibility = Visibility.Visible;                         // 이동
                    btnMoveCancel.Visibility = Visibility.Visible;                   // 이동취소
                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if (ChkCustomer())
                    {
                        btnCustomer.Visibility = Visibility.Visible;                     // 고객인증 팝업
                    }

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }
            }
            //else if (ProcessCode == Process.SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            else if (ProcessCode == Process.SLITTING || ProcessCode == Process.TWO_SLITTING) //20211215 2차 Slitting 공정진척DRB 화면 개발
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    // 적용대상 동은 오창 자동차 전극
                    if (LoginInfo.CFG_AREA_ID.Equals(ElectrodeAreas.E6) || LoginInfo.CFG_AREA_ID.Equals(ElectrodeAreas.E5))
                    {
                        btnSlBatch.Visibility = Visibility.Visible;
                    }

                    // 적용대상 동은 CNB 전극1동, CWA 전극2동
                    if (LoginInfo.CFG_AREA_ID.Equals(ElectrodeAreas.EC) || LoginInfo.CFG_AREA_ID.Equals(ElectrodeAreas.ED))
                    {
                        btnSkidTypeSettingByPort.Visibility = Visibility.Visible;
                    }

                    // 출하모델 적용대상 동
                    if (IsCommonCodeUse("SHIPMENTMODEL", LoginInfo.CFG_AREA_ID))
                    {
                        btnShipmentModel.Visibility = Visibility.Visible;
                    }

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if(ChkCustomer())
                    {
                        btnCustomer.Visibility = Visibility.Visible;                     // 고객인증 팝업
                    }

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }
            }
            else if (ProcessCode == Process.ROLL_PRESSING)
            {
                if (Main)
                {
                    //btnManualMode.Visibility = Visibility.Visible;                   // 수작업모드 : 작업시작, 시작취소, 장비완료취소 수작업으로 가능하게 버튼 활성화
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnCut.Visibility = Visibility.Visible;                          // Cut
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnProcReturn.Visibility = Visibility.Visible;                   // R/P 대기 변경
                    //btnWorkHalfSlitSide.Visibility = Visibility.Visible;             // 무지부 방향설정  // 무지부 방향설정 버튼은 동별 공통코드로 관리함 : MNG_SLITTING_SIDE_AREA
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소


                    if (ChkCustomer())
                    {
                        btnCustomer.Visibility = Visibility.Visible;                     // 고객인증 팝업
                    }

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }

                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }
            }
            else if (ProcessCode == Process.REWINDER)
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    //btnCut.Visibility = Visibility.Visible;                          // Cut
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }

                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }
            }
            else if (ProcessCode == Process.TAPING)
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }
            }
            else if (ProcessCode == Process.HEAT_TREATMENT)
            {
                if (Main)
                {
                    btnEqptIssue.Visibility = Visibility.Visible;                    // 설비특이사항
                    btnCancelDelete.Visibility = Visibility.Visible;                 // 삭제Lot생성
                    btnEqptCond.Visibility = Visibility.Visible;                     // 작업조건등록
                    btnPilotProdMode.Visibility = Visibility.Visible;                // 시생산설정/해제
                    btnPilotProdSPMode.Visibility = Visibility.Visible;              // 시생산샘플설정/해제

                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }
            }
            else if (ReWindingProcess == true)
            {
                if (Main)
                {
                    btnInput.Visibility = Visibility.Visible;                        // 추가투입
                    btnInputCancel.Visibility = Visibility.Visible;                  // 추가투입취소
                    btnStart.Visibility = Visibility.Visible;                        // 작업시작
                    btnCancel.Visibility = Visibility.Visible;                       // 시작취소
                    if (ChkVerification())
                    {
                        btnSearchWaitingWork.Visibility = Visibility.Visible;            // 대기재공조회 
                    }

                    if (IsReturnConditionVisibility(ProcessCode))
                    {
                        btnReturnCondition.Visibility = Visibility.Visible;                // 물류반송조건설정
                    }
                }
                else
                {
                    btnEqptEnd.Visibility = Visibility.Visible;                      // 장비완료
                    btnEqptEndCancel.Visibility = Visibility.Visible;                // 장비완료취소
                }

            }

            //if (IsUpdateLaneNoVisibility(ProcessCode) == true)
            //{
            //    btnUpdateLaneNo.Visibility = Visibility.Visible;                   // 슬리팅 레인 번호 변경
            //}

            // 공정 변경에 따른 버튼 권한 조회
            GetButtonPermissionGroup(Main);
        }

        public void SetButtonVisibility(bool Main, bool ReWindingProcess = false)
        {
            if (Main)
            {
                //btnEqptEnd.IsEnabled = false;

                //btnProductionSchedule.Visibility = Visibility.Visible;
                btnProductionSchedule.Visibility = Visibility.Collapsed;
                btnProductList.Visibility = Visibility.Collapsed;
                btnDispatch.Visibility = Visibility.Collapsed;
                btnCard.Visibility = Visibility.Collapsed;                           // 이력카드발행
                btnBarcodeLabel.Visibility = Visibility.Collapsed;
                //btnPrint.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (ReWindingProcess)
                {
                    btnProductionSchedule.Visibility = Visibility.Collapsed;
                    btnProductList.Visibility = Visibility.Visible;

                    btnDispatch.Visibility = Visibility.Visible;
                    btnCard.Visibility = Visibility.Visible;                  
                    btnPrint.Visibility = Visibility.Visible;
                }
                else
                {
                    //btnEqptEnd.IsEnabled = true;

                    btnProductionSchedule.Visibility = Visibility.Collapsed;
                    btnProductList.Visibility = Visibility.Visible;
                    btnDispatch.Visibility = Visibility.Visible;

                    if (ProcessCode == Process.PRE_MIXING || ProcessCode == Process.MIXING || ProcessCode == Process.BS || ProcessCode == Process.CMC || ProcessCode == Process.InsulationMixing || ProcessCode == Process.DAM_MIXING)
                        btnBarcodeLabel.Visibility = Visibility.Collapsed;
                    else
                        btnBarcodeLabel.Visibility = Visibility.Visible;

                    btnPrint.Visibility = Visibility.Visible;
                }
            }

            //////////////////////////////////// 
            if (ProcessCode == Process.COATING)
            {
                btnDispatch.Content = ObjectDic.Instance.GetObjectName("공정이동");
            }
            else
            {
                btnDispatch.Content = ObjectDic.Instance.GetObjectName("실적확정");
            }
        }

        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnManualMode);
            listAuth.Add(btnEqptIssue);
            listAuth.Add(btnFinalCut);
            listAuth.Add(btnCleanLot);
            listAuth.Add(btnCancelFCut);
            listAuth.Add(btnCancelDelete);
            listAuth.Add(btnCut);
            listAuth.Add(btnInvoiceMaterial);
            listAuth.Add(btnEqptCond);
            listAuth.Add(btnMixConfirm);
            listAuth.Add(btnSamplingProd);
            listAuth.Add(btnProcReturn);
            listAuth.Add(btnSamplingProdT1);
            //listAuth.Add(btnMixerTankInfo);
            listAuth.Add(btnReservation);
            listAuth.Add(btnFoil);
            //listAuth.Add(btnSlurryConf);
            listAuth.Add(btnStartCoaterCut);
            //listAuth.Add(btnMovetoHalf);
            //listAuth.Add(btnWorkHalfSlitSide);
            listAuth.Add(btnSkidTypeSettingByPort);
            listAuth.Add(btnSlBatch);
            listAuth.Add(btnCustomer);
            listAuth.Add(btnScrapLot);
            listAuth.Add(btnWebBreak);
            listAuth.Add(btnPilotProdMode);
            listAuth.Add(btnPilotProdSPMode);
            listAuth.Add(btnShipmentModel);

            listAuth.Add(btnMove);
            listAuth.Add(btnMoveCancel);
            listAuth.Add(btnInput);
            listAuth.Add(btnInputCancel);
            listAuth.Add(btnStart);
            listAuth.Add(btnCancel);
            listAuth.Add(btnEqptEnd);
            listAuth.Add(btnEqptEndCancel);
            listAuth.Add(btnEndCancel);
            //listAuth.Add(btnProductionSchedule);
            //listAuth.Add(btnProductList);
            listAuth.Add(btnDispatch);
            //listAuth.Add(btnCard);
            //listAuth.Add(btnBarcodeLabel);
            //listAuth.Add(btnPrint);      
            listAuth.Add(btnSlurryManualOutput);        //nathan 2023.12.20 믹서 코터 배치연계 
            listAuth.Add(btnSlurryManualInput);         //nathan 2023.12.20 믹서 코터 배치연계 
            listAuth.Add(btnSlurryBufferManualInit);    //minylee 2024.2.14 믹서 코터 배치 연계 고도화

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 권한 그룹에 따른 설정
        /// </summary>
        private void GetButtonPermissionGroup(bool Main)
        {
            try
            {
                InitializeButtonPermissionGroup();

                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID; //"CNSSOBAKTOP"; //
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = ProcessCode;
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
                                case "INPUT_W":               // 투입 사용 권한
                                    SetInputUseAuthority(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                    break;
                                case "LOTSTART_W":            // 작업시작 사용 권한
                                    if (Main)
                                        btnStart.Visibility = Visibility.Visible;
                                    break;
                                case "CANCEL_LOTSTART_W":     // 작업시작 취소 권한
                                    if (Main)
                                        btnCancel.Visibility = Visibility.Visible;
                                    break;
                                case "CANCEL_EQPT_END_W":     // 설비완공 취소 권한
                                    if (Main == false)
                                        btnEqptEndCancel.Visibility = Visibility.Visible;
                                    break;
                                case "CANCEL_END_W":          // 자동실적 확정취소 권한
                                    if (Main == false)
                                        btnEndCancel.Visibility = Visibility.Visible;
                                    break;
                                case "PILOT_W":    // 시생산 샘플 설정 권한
                                    if (Main)
                                        btnPilotProdSPMode.Visibility = Visibility.Visible;
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
        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                btnStart.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Collapsed;
                btnEqptEndCancel.Visibility = Visibility.Collapsed;
                btnEndCancel.Visibility = Visibility.Collapsed;
                btnPilotProdSPMode.Visibility = Visibility.Collapsed;

                SetInputUseAuthority(null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 투입 권한 
        /// </summary>
        protected virtual void SetInputUseAuthority(string GrpCode)
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("SetInputUseAuthority");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                parameterArrys[0] = GrpCode;

                methodInfo.Invoke(UcParentControl, parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private bool IsCommonCodeUse(string sCmcdType, string sAreaid)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCmcdType;
                dr["CMCODE"] = sAreaid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
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
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = sCodeName;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        /// <summary>
        /// 고객인증 조회 버튼
        /// </summary>
        private bool ChkCustomer()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "CUSTOMER_VERIFICATION_CHK_AREA";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }


        /// <summary>
        /// Verification 대기 LOT 조회 버튼
        /// </summary>
        private bool ChkVerification()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "CUSTOMER_VERIFICATION_CHK_AREA";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE2"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }



  
        /// <summary>
        /// 물류반송 조건설정 버튼
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        private bool IsReturnConditionVisibility(string ProcessID)
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
                dr["COM_TYPE_CODE"] = "LOGIS_COND_BY_PROC";
                dr["COM_CODE"] = ProcessID;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
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

        ///// <summary>
        ///// 슬리팅 레인 번호 변경 버튼
        ///// </summary>
        //private bool IsUpdateLaneNoVisibility(string sProcessID)
        //{
        //    const string sBizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";
        //    try
        //    {
        //        DataTable dtInTable = new DataTable("INDATA");
        //        dtInTable.Columns.Add("LANGID", typeof(string));
        //        dtInTable.Columns.Add("AREAID", typeof(string));
        //        dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
        //        dtInTable.Columns.Add("COM_CODE", typeof(string));
        //        dtInTable.Columns.Add("USE_FLAG", typeof(string));
        //        DataRow dr = dtInTable.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
        //        dr["COM_TYPE_CODE"] = "LANE_NO_CHG_BY_PROC";
        //        dr["COM_CODE"] = sProcessID;
        //        dr["USE_FLAG"] = "Y";
        //        dtInTable.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "INDATA", "OUTDATA", dtInTable);

        //        if (CommonVerify.HasTableRow(dtResult))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        return false;
        //    }
        //}


    }
}