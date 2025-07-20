/*************************************************************************************************************
 Created Date : 2020-12-19
      Creator : KANG DONG HEE
   Decription : 상온/출하 Aging 예약
--------------------------------------------------------------------------------------------------------------
 [Change History]
  2020.10.19  NAME : Initial Created
  2021.04.01  KDH : 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
  2021.04.04  KDH : PKG Lot List 조회 수정
  2021.04.11  KDH : 조회건수 첫번째 데이타 자동 선택
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2021.04.19  KDH : 데이타 Check 상태를 인지 못하는 현상 수정
  2021.01.04  PSM : CHECK 상태 확인 지점 수정 (LoadedCellPresenter => 조회 함수 내)
  2022.07.10  JYD : 체크박스 선택시 작업 Unit 같은거 다 체크되는데 Equipment.EQPTID+작업 Unit 으로 수정
  2022.07.15  최도훈 : 사이트마다 차기 공정에 차이가 있어, 동별 공통코드로 변경할 수 있도록 수정.
  2022.08.26  조영대 : Hold된 Tray와 일반 Tray가 2단 적재되어 있을 경우 예약불가 Validation
  2022.10.13  조영대 : 특별관리 체크를 특별관리 내용이 아닌 특별관리 플레그로 판별
  2022.11.03  강동희 : 범례 체크박스 Default로 UnCheck 상태로 설정 및 검색 조건으로 포함하여 조회
  2022.11.14  최완영 : 생산라인 ComboBox를 필수 선택 항목으로  변경 (ALL을  최하단에 추가)
  2022.12.12  최완영 : 생산라인 ALL 일경우  NULL  처리 추가 
  2022.12.14  조영대 : UI Event Log 수정(USER_IP, PC_NAME, MENUID)
  2022.12.21  최완영 : 조회조건  LOT TYPE  추가
  2023.01.03  조영대 : Lot Hold, 특별관리 출하금지 Validation 수정
  2023.01.06  최완영 : 조회조건  LOT TYPE  Default 값 양산 설정 
  2022.01.31  조영대 : Validation 추가
  2022.04.04  김수용 : 보정dOCV 계산 항목 미수신 EOL 투입 제한 기능 추가
  2023.07.31  최도훈 : 생산라인 ALL 삭제(미사용)
  2023.09.21  조영대 : 예약 및 예약취소 처리시 10개씩 분리 처리.
  2023.09.22  이의철 : Route select 팝업 버튼 추가
  2023.09.22  이의철 : TRAY 상대판정 추가
  2023.10.21  조영대 : Aging Type 선택시 기본 차기공정 자동선택, Route 콤보 차기공정 연결
  2023.10.28  이의철 : E등급 수량 한계 초과  
  2023.12.14  손동혁 : ALL_CHK 체크박스 (Tray 조회 기능 전부 체크 선택 추가)
  2023.12.15  이의철 : S/C 상태가 작동중이 아니면 리스트에서 제외
  2024.01.11  박수미 : OCV 측정일 편차 검사 Validation 추가
  2024.01.19  최도훈 : 특별관리여부 공통코드 변경
  2024.02.01  박수미 : 포장 Hold > Cell Hold 된 Lot 색인 기능 추가
  2024.02.05  이지은 : [RTD물류동] 반송지시가 내려간 트레이 리스트 표시,  예약취소 버튼 클릭시 반송지시 캔슬 추가
  2024.02.27  배준호 : 최종 수동판정 여부(수동 판정 대기) 추가
  2024.03.04  형준우 : 고온Aging 단 정보 추가 (안수종 팀장 요청사항)
  2024.03.07  이지은 : Biz Exception 처리 오류 수정- 배포를 위해 잠시 고온Aging 단 정보 추가 주석처리
  2024.03.13  배준호 : 최종 수동판정 여부(수동 판정 대기) EOL일때만 처리
  2024.03.14  이현승 : Lot 유형 선택에 따라 PKG Lot ID 필터링 반영
  2024.03.14  배준호 : 예약 취소 시 Validation 무시하고 처리하도록 수정 및 수동판정대기 2단 Tray Validation 추가
  2024.04.22  이지은 : 수동 예약 이력 Tab 추가
  2024.06.11  남형희 : E20240507-000384 예약자 컬럼 추가
  2024.07.09  김용준 : 조회조건 '상온Aging' 선택 시 Next Op 'Degas', '출하 Aging' 선택 시 Next Op 'EOL'부터 보이게 수정 - E20240708-000244
  2024.08.30  박태용 : 최초 화면 진입시 포장 홀드 체크박스 True -> False 변경, 상온/출하 Aging 화면 예약탭의 cboModel ComboStatus를 ALL -> SELECT_ALL 변경, 필수값으로 변경
  2024.11.26  최도훈 : (Box/Jig) 예약이력 화면 null 처리 추가
  **************************************************************************************************************/
#define SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS001
{
    public enum AgingReserv
    {
        NH_AGING_REV = 0,
        NP_AGING_REV_BOX_JIG = 1
    }

    public partial class FCS001_031 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private AgingReserv MenuKind;
        private DataTable tempTable = new DataTable();

        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        private string NEXT_PROC = string.Empty;
        private Dictionary<string, string> DEFAULT_NEXT_PROC = null;

        private bool bFCS001_031_ROUTE_LIST = false; //Route select 팝업 버튼 추가
        private bool bFCS001_031_TRAY_JUDG = false;  //TRAY 상대판정 추가
        private bool bFCS001_031_E_GR_PASS_LIMIT = false; //E등급 수량 한계 초과
        private bool bFCS001_031_INTEROCK_ALL_CHECK = false; //Tray 조회 기능 전부 선택 추가
        private bool bFCS001_031_SC_STAT_CHECK = false;//S/C 상태가 작동중이 아니면 리스트에서 제외
        private bool bFCS001_031_TRF_YN = false;//RTD 물류동이 아니면 제외
        private bool bFCS001_031_MNL_JUDG_STANDBY = false;//수동 판정 대기 추가

        Util _Util = new Util();

        public FCS001_031()
        {
            InitializeComponent();
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                //사용자 권한별로 버튼 숨기기
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnRoute);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //여기까지 사용자 권한별로 버튼 숨기기

                switch (FrameOperation.MENUID.ToString())
                {
                    case "SFU010710130": //상온/출하 Aging 예약
                        MenuKind = AgingReserv.NH_AGING_REV;
                        break;
                    case "SFU010710140": //상온/Pre Aging 예약(Box/Jig)
                        MenuKind = AgingReserv.NP_AGING_REV_BOX_JIG;
                        break;
                }
                //FCS001_031_INTEROCK_ALL_CHECK
                bFCS001_031_INTEROCK_ALL_CHECK = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_031_INTEROCK_ALL_CHECK");

                if (bFCS001_031_INTEROCK_ALL_CHECK)
                {
                    chkAllJudg.Visibility = Visibility.Visible;
                    lblAllJudg.Visibility = Visibility.Visible;
                }
                else
                {
                    chkAllJudg.Visibility = Visibility.Collapsed;
                    lblAllJudg.Visibility = Visibility.Collapsed;
                }
                //Route select 팝업 버튼 추가
                bFCS001_031_ROUTE_LIST = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_031_ROUTE_LIST");

                //TRAY 상대판정 추가
                bFCS001_031_TRAY_JUDG = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_031_TRAY_JUDG");

                //E등급 수량 한계 초과
                bFCS001_031_E_GR_PASS_LIMIT = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_031_E_GR_PASS_LIMIT");

                //S/C 상태가 작동중이 아니면 리스트에서 제외
                bFCS001_031_SC_STAT_CHECK = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_031_SC_STAT_CHECK");

                //RTD 물류동이 아니면 제외 (GMES 전환동 제외)
                bFCS001_031_TRF_YN = _Util.IsAreaCommonCodeUse("FORM_SITE_BASE_INFO", "FORMLGS_USE_FLAG");

                //수동 판정 대기 추가
                bFCS001_031_MNL_JUDG_STANDBY = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_031_MNL_JUDG_STANDBY");

                //Control Setting
                InitControl();

                //Combo Setting
                InitCombo();


                this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                // 확인 후 수정
                btnSearch.ToolTip = ObjectDic.Instance.GetObjectName("UC_0004"); //※ 상온/출하 Aging 작업조건이\r\n최대(999999분)로 설정된 Tray와\r\n차기공정이 Degas/특성측정기인\r\nTray만 조회됩니다.

                cboLotId.Visibility = Visibility.Visible;
                cboLotId.IsEnabled = true;
                txtLotID.Visibility = Visibility.Collapsed;
                txtLotID.IsEnabled = false;

                cboLotId2.Visibility = Visibility.Visible;
                cboLotId2.IsEnabled = true;
                txtLotID2.Visibility = Visibility.Collapsed;
                txtLotID2.IsEnabled = false;

                //Route select 팝업 버튼 추가
                if (bFCS001_031_ROUTE_LIST.Equals(true))
                {
                    this.btnRoute.Visibility = Visibility.Visible;
                }
                else
                {
                    this.btnRoute.Visibility = Visibility.Hidden;
                }

                //TRAY 상대판정 추가
                if (bFCS001_031_TRAY_JUDG.Equals(true))
                {
                    chkTrayJudg.Visibility = Visibility.Visible;
                    lblTrayJudg.Visibility = Visibility.Visible;
                }
                else
                {
                    chkTrayJudg.Visibility = Visibility.Hidden;
                    lblTrayJudg.Visibility = Visibility.Hidden;
                }

                //E등급 수량 한계 초과
                if (bFCS001_031_E_GR_PASS_LIMIT.Equals(true))
                {
                    chkEGRPassLimit.Visibility = Visibility.Visible;
                    lblEGRPassLimit.Visibility = Visibility.Visible;
                }
                else
                {
                    chkEGRPassLimit.Visibility = Visibility.Hidden;
                    lblEGRPassLimit.Visibility = Visibility.Hidden;
                }

                if (bFCS001_031_TRF_YN.Equals(false))
                {
                    chkTrfCmd.Visibility = Visibility.Visible;
                    lblTrfCmd.Visibility = Visibility.Visible;
                }
                else
                {
                    chkTrfCmd.Visibility = Visibility.Collapsed;
                    lblTrfCmd.Visibility = Visibility.Collapsed;
                }

                //수동 판정 대기 추가
                if (bFCS001_031_MNL_JUDG_STANDBY.Equals(true))
                {
                    chkMNL_JUDG_STANDBY.Visibility = Visibility.Visible;
                    lblMNL_JUDG_STANDBY.Visibility = Visibility.Visible;
                }
                else
                {
                    chkMNL_JUDG_STANDBY.Visibility = Visibility.Collapsed;
                    lblMNL_JUDG_STANDBY.Visibility = Visibility.Collapsed;
                }

            }
            else if (MenuKind == AgingReserv.NP_AGING_REV_BOX_JIG)
            {
                cboLotId.Visibility = Visibility.Collapsed;
                cboLotId.IsEnabled = false;
                txtLotID.Visibility = Visibility.Visible;
                txtLotID.IsEnabled = true;
                cboLotId2.Visibility = Visibility.Collapsed;
                cboLotId2.IsEnabled = false;
                txtLotID2.Visibility = Visibility.Visible;
                txtLotID2.IsEnabled = true;

                //Route select 팝업 버튼 추가
                this.btnRoute.Visibility = Visibility.Hidden;

                //TRAY 상대판정 추가
                chkTrayJudg.Visibility = Visibility.Hidden;
                lblTrayJudg.Visibility = Visibility.Hidden;

                //E등급 수량 한계 초과
                chkEGRPassLimit.Visibility = Visibility.Hidden;
                lblEGRPassLimit.Visibility = Visibility.Hidden;
            }

        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                // 상온/출하 Aging 예약 - Aging Type 선택에 따라 NEXTOP 순서 변경
                C1ComboBox[] cboAgingTypeChild = { cboNextOp, cboLotId };
                C1ComboBox[] cboAgingTypeChild2 = { cboNextOp2, cboLotId2 };
                string[] sFilterAgingType = { "EQPT_GR_TYPE_CODE", "3,7" };
                _combo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterAgingType, cbChild: cboAgingTypeChild);
                _combo.SetCombo(cboAgingType2, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterAgingType, cbChild: cboAgingTypeChild2);
            }
            else
            {
                string[] sFilterAgingType = { "EQPT_GR_TYPE_CODE", "3,9" };
                _combo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterAgingType);
                _combo.SetCombo(cboAgingType2, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterAgingType);
            }


            // 사이트마다 차기 공정에 차이가 있어, 동별 공통코드로 변경할 수 있도록 수정.
            GetAgingNextProc(MenuKind.ToString());

            string[] sFilterToOp = null;
            if (string.IsNullOrWhiteSpace(NEXT_PROC))
            {
                sFilterToOp = new string[] { "EQPT_GR_TYPE_CODE", (MenuKind == AgingReserv.NH_AGING_REV) ? "5,D" : "J,1,D" };
            }
            else
            {
                sFilterToOp = new string[] { "EQPT_GR_TYPE_CODE", NEXT_PROC };
            }

            // 상온 / 출하 Aging 예약 -Aging Type 선택에 따라 NEXTOP 순서 변경
            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboNextOpParent = { cboAgingType };
                C1ComboBox[] cboNextOpParent2 = { cboAgingType2 };
                C1ComboBox[] cboNextOpChild = { cboLotId, cboRoute };
                C1ComboBox[] cboNextOpChild2 = { cboLotId2, cboRoute2 };
                _combo.SetCombo(cboNextOp, CommonCombo_Form.ComboStatus.NONE, sCase: "AGINGTYPE_NEXTOP", sFilter: sFilterToOp, cbChild: cboNextOpChild, cbParent: cboNextOpParent);
                _combo.SetCombo(cboNextOp2, CommonCombo_Form.ComboStatus.NONE, sCase: "AGINGTYPE_NEXTOP", sFilter: sFilterToOp, cbChild: cboNextOpChild2, cbParent: cboNextOpParent2);
            }
            else
            {
                _combo.SetCombo(cboNextOp, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterToOp);
                _combo.SetCombo(cboNextOp2, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterToOp);
            }


            string[] sFilter2 = { "COMBO_AGING_ISS_PRIORITY" }; // 기준정보 추가요청중.
            _combo.SetCombo(cboPriority, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilter2);

            string[] sFilterResv = { "FORM_GMES_USE_ACTID", "REQUEST_AGING_MANUAL_ISS,CANCEL_AGING_MANUAL_ISS" };
            _combo.SetCombo(cboActID, CommonCombo_Form.ComboStatus.NONE, sCase: "AGINGKIND", sFilter: sFilterResv);

            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboLineChild = { cboModel, cboLotId };
                C1ComboBox[] cboLineChild2 = { cboModel2, cboLotId2 };
                _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild);
                _combo.SetCombo(cboLine2, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild2);
            }
            else
            {
                C1ComboBox[] cboLineChild = { cboModel };
                C1ComboBox[] cboLineChild2 = { cboModel2 };
                _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild);
                _combo.SetCombo(cboLine2, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild2);
            }

            C1ComboBox[] cboModelParent = { cboLine };
            C1ComboBox[] cboModelParent2 = { cboLine2 };
            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboModelChild = { cboRoute, cboLotId };
                C1ComboBox[] cboModelChild2 = { cboRoute2, cboLotId2 };
                _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.SELECT_ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);
                _combo.SetCombo(cboModel2, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild2, cbParent: cboModelParent2);
            }
            else
            {
                C1ComboBox[] cboModelChild = { cboRoute };
                C1ComboBox[] cboModelChild2 = { cboRoute2 };
                _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);
                _combo.SetCombo(cboModel2, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild2, cbParent: cboModelParent2);
            }

            C1ComboBox[] cboRouteParent = { cboLine, cboModel, cboNextOp };
            C1ComboBox[] cboRouteParent2 = { cboLine2, cboModel2, cboNextOp2 };
            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                //Route select 팝업 버튼 추가
                if (this.bFCS001_031_ROUTE_LIST.Equals(true))
                {
                    C1ComboBox[] cboRouteChild = { cboOper, cboLotId };
                    C1ComboBox[] cboRouteChild2 = { cboOper2, cboLotId2 };
                    _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_RESV", cbParent: cboRouteParent, cbChild: cboRouteChild);
                    _combo.SetCombo(cboRoute2, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_RESV", cbParent: cboRouteParent2, cbChild: cboRouteChild2);
                }
                else
                {
                    C1ComboBox[] cboRouteChild = { cboOper, cboLotId };
                    C1ComboBox[] cboRouteChild2 = { cboOper2, cboLotId2 };
                    _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_NEXTOP", cbParent: cboRouteParent, cbChild: cboRouteChild);
                    _combo.SetCombo(cboRoute2, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_NEXTOP", cbParent: cboRouteParent2, cbChild: cboRouteChild2);
                }
            }
            else
            {
                C1ComboBox[] cboRouteChild = { cboOper };
                C1ComboBox[] cboRouteChild2 = { cboOper2 };
                _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_NEXTOP", cbParent: cboRouteParent, cbChild: cboRouteChild);
                _combo.SetCombo(cboRoute2, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_NEXTOP", cbParent: cboRouteParent2, cbChild: cboRouteChild2);
            }

            // Lot 유형
            //2023.01.06   LotType  default  양산  
            string[] sFilterLotType = { "P" };
            _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.SELECT, sCase: "LOTTYPE", sFilter: sFilterLotType); //2022.12.21 Lot 유형 검색조건 추가
            _combo.SetCombo(cboLotType2, CommonCombo_Form.ComboStatus.SELECT, sCase: "LOTTYPE", sFilter: sFilterLotType);
            
            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboLotParent = { cboLine, cboModel, cboRoute, cboSpecial, cboAgingType, cboNextOp, cboLotType };
                C1ComboBox[] cboLotParent2 = { cboLine2, cboModel2, cboRoute2, cboSpecial2, cboAgingType2, cboNextOp2, cboLotType2 };
                //_combo.SetCombo(cboLotId, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTID", cbParent: cboLotParent); //2021.04.04  KDH : PKG Lot List 조회 수정
                _combo.SetCombo(cboLotId, CommonCombo_Form.ComboStatus.ALL, sCase: "LOT", cbParent: cboLotParent); //2021.04.04  KDH : PKG Lot List 조회 수정
                _combo.SetCombo(cboLotId2, CommonCombo_Form.ComboStatus.ALL, sCase: "LOT", cbParent: cboLotParent2);
            }

            C1ComboBox[] cboOperParent = { cboRoute };
            C1ComboBox[] cboOperParent2 = { cboRoute2 };
            string[] sFilterOper = { (MenuKind == AgingReserv.NH_AGING_REV) ? "3,7" : "3,9" };
            _combo.SetCombo(cboOper, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP_MAX_END_TIME", cbParent: cboOperParent, sFilter: sFilterOper);
            _combo.SetCombo(cboOper2, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP_MAX_END_TIME", cbParent: cboOperParent2, sFilter: sFilterOper);

            string[] sFilter = { "SPCL_FLAG" };
            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboSpecialChild = { cboLotId };
                C1ComboBox[] cboSpecialChild2 = { cboLotId2 };
                _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter, cbChild: cboSpecialChild);
                _combo.SetCombo(cboSpecial2, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter, cbChild: cboSpecialChild2);
            }
            else
            {
                _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter);
                _combo.SetCombo(cboSpecial2, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter);
            }

            DataTable dtSearchCnt = new DataTable();
            dtSearchCnt.Columns.Add("CBO_CODE", typeof(string));
            dtSearchCnt.Columns.Add("CBO_NAME", typeof(string));
            for (int i = 1; i < 11; i++)
            {
                DataRow dr = dtSearchCnt.NewRow();
                dr["CBO_CODE"] = (i * 10).ToString();
                dr["CBO_NAME"] = (i * 10).ToString();
                dtSearchCnt.Rows.Add(dr);
            }
            DataRow drAll = dtSearchCnt.NewRow();
            drAll["CBO_CODE"] = "99999";
            drAll["CBO_NAME"] = "-ALL-";
            dtSearchCnt.Rows.Add(drAll);

            cboSearchChount.DisplayMemberPath = "CBO_NAME";
            cboSearchChount.SelectedValuePath = "CBO_CODE";
            cboSearchChount.ItemsSource = DataTableConverter.Convert(dtSearchCnt);
            cboSearchChount.SelectedIndex = 0; //2021.04.11 조회건수 첫번째 데이타 자동 선택

            // 차기 공정 기본 설정
            if (DEFAULT_NEXT_PROC != null && DEFAULT_NEXT_PROC.ContainsKey(EQPT_GR_TYPE_CODE))
            {
                cboNextOp.SelectedValue = DEFAULT_NEXT_PROC[EQPT_GR_TYPE_CODE];
            }

            //이력 날짜 조회 조건 Setting
            dtpFromDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = System.DateTime.Now;
        }

        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingType);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetAgingNextProc(string menuKind)
        {
            try
            {
                NEXT_PROC = string.Empty;
                DEFAULT_NEXT_PROC = new Dictionary<string, string>();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "AGING_NEXT_PROC";
                dr["COM_CODE"] = menuKind;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    NEXT_PROC = Util.NVC(row["ATTR1"]);

                    // 차기공정 기본 설정.
                    if (!string.IsNullOrEmpty(Util.NVC(row["ATTR2"])))
                    {
                        string[] nextProc = NEXT_PROC.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] aging = Util.NVC(row["ATTR2"]).Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (nextProc.Length > 0 && aging.Length > 0 && nextProc.Length == aging.Length)
                        {
                            for (int inx = 0; inx < aging.Length; inx++)
                            {
                                if (!DEFAULT_NEXT_PROC.ContainsKey(aging[inx]))
                                {
                                    DEFAULT_NEXT_PROC.Add(aging[inx], nextProc[inx]);
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

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetList2();
        }

        private void btnReservation_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();
            if (!dgAgingReserv.IsCheckedRow("CHK"))
            {
                Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                return;
            }

            string sMSG = string.Empty;
            if ((sender as Button).Name.Equals("btnReservation"))
            {
                sMSG = "FM_ME_0312"; //예약하시겠습니까?                
            }
            else
            {
                sMSG = "FM_ME_0313"; // 예약취소하시겠습니까?
            }

            //상태를 변경하시겠습니까?
            Util.MessageConfirm(sMSG, (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sCancel = string.Empty;
                        string sMsg = string.Empty;
                        if ((sender as Button).Name.Equals("btnReservation"))
                        {
                            sCancel = "N";
                            sMsg = "FM_ME_0014";  //{0}개의 Tray를 예약완료하였습니다.
                        }
                        else
                        {
                            sCancel = "Y";
                            sMsg = "FM_ME_0301";  //{0}개의 Tray를 예약취소하였습니다.
                        }

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("IFMODE", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                        dtRqst.Columns.Add("UNITID", typeof(string));
                        dtRqst.Columns.Add("CANCEL_YN", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("MENUID", typeof(string));
                        dtRqst.Columns.Add("USER_IP", typeof(string));
                        dtRqst.Columns.Add("PC_NAME", typeof(string));
                        dtRqst.Columns.Add("AREAID", typeof(string));

                        //2021.04.19 데이타 Check 상태를 인지 못하는 현상 수정 START
                        DataTable dtAR = DataTableConverter.Convert(dgAgingReserv.ItemsSource);

                        foreach (DataRow drar in dtAR.Rows)
                        {
                            if (Util.NVC(drar["CHK"]).Equals("True"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["LOTID"] = Util.NVC(drar["LOTID"]);
                                dr["UNITID"] = Util.NVC(drar["EQPTID"]);
                                dr["CANCEL_YN"] = sCancel;
                                dr["USERID"] = LoginInfo.USERID;
                                dr["MENUID"] = LoginInfo.CFG_MENUID;
                                dr["USER_IP"] = LoginInfo.USER_IP;
                                dr["PC_NAME"] = LoginInfo.PC_NAME;
                                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                dtRqst.Rows.Add(dr);
                            }
                        }
                        //2021.04.19 데이타 Check 상태를 인지 못하는 현상 수정 END

                        if (dtRqst.Rows.Count == 0)
                        {
                            Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                            return;
                        }

                        // 백그라운드 실행 (xProgress_WorkProcess)
                        object[] argument = new object[2] { dtRqst, sMsg };
                        xProgress.Percent = 0;
                        xProgress.Visibility = Visibility.Visible;
                        xProgress.RunWorker(argument);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });

        }

        /*private void cboAgingType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            //GetCommonCode();
            EQPT_GR_TYPE_CODE = cboAgingType.SelectedValue.ToString();
            string[] sFilter = { EQPT_GR_TYPE_CODE, null };
            _combo.SetCombo(cboSCLine, CommonCombo_Form.ComboStatus.ALL, sCase: "SCLINE", sFilter: sFilter);
            if (cboSCLine.Items.Count > 0)
            {
                cboSCLine.SelectedIndex = 0;
            }
        }
        */

        private void cboPriority_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.GetCondition(cboPriority).Equals("5"))
            {
                btnReservation.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Collapsed;
                Util.gridClear(dgAgingReserv);
            }
            else
            {
                btnReservation.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Visible;
                Util.gridClear(dgAgingReserv);
            }
        }

        private void cboActID_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.GetCondition(cboActID).Equals("5"))
            {
                Util.gridClear(dgAgingReserv2);
            }
            else
            {
                Util.gridClear(dgAgingReserv2);
            }
        }

        private void dgAgingReserv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgAgingReserv.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Column.Name.Equals("CSTID"))
                    {
                        //Tray 정보조회 화면 연계
                        object[] parameters = new object[6];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "CSTID"));
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "LOTID"));
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = string.Empty;
                        this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAgingReserv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //동일한 설비 체크하기
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAgingReserv.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                if (cell.Row.Index < 2)
                {
                    return;
                }

                dgAgingReserv.ClearValidation();

                bool bCheck = bool.Parse(Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "CHK")));
                bCheck = bCheck.Equals(false) ? true : false;

                string sEqpID = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "MACHINE_EQPTID"));

                for (int i = 0; i < dgAgingReserv.Rows.Count; i++)
                {
                    if (i > 1)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[i].DataItem, "MACHINE_EQPTID")).Equals(sEqpID))
                        {
                            if (dgAgingReserv.Rows[i].Presenter != null && dgAgingReserv.Rows[i].Presenter[dgAgingReserv.Columns["CHK"]].IsEnabled)
                            {
                                DataTableConverter.SetValue(dgAgingReserv.Rows[i].DataItem, "CHK", bCheck);

                                if (bCheck)
                                {
                                    string chkValidation = CheckReserveValidation(Util.NVC(dgAgingReserv.GetValue(i, "LOTID")));
                                    string msg = string.Empty;
                                    if (chkValidation.Contains("HOLD"))
                                    {
                                        msg += MessageDic.Instance.GetMessage("FM_ME_0449");     //Hold 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.

                                    }
                                    if (chkValidation.Contains("SPECIAL"))
                                    {
                                        if (!msg.Equals(string.Empty)) msg += "\r\n";
                                        msg += MessageDic.Instance.GetMessage("FM_ME_0464");    //특별(출하금지) 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                                    }
                                    if (chkValidation.Contains("OCV"))
                                    {
                                        if (!msg.Equals(string.Empty)) msg += "\r\n";
                                        msg += MessageDic.Instance.GetMessage("FM_ME_0519");     //OCV 측정일 편차 초과된 Tray와 일반 Tray가 2단 적재되어 있을 경우 예약할 수 없습니다.
                                    }
                                    if (chkValidation.Contains("FIN_MNL_JUDG"))
                                    {
                                        if (!msg.Equals(string.Empty)) msg += "\r\n";
                                        msg += MessageDic.Instance.GetMessage("FM_ME_0584");     //OCV 측정일 편차 초과된 Tray와 일반 Tray가 2단 적재되어 있을 경우 예약할 수 없습니다.
                                    }
                                    if (!chkValidation.Equals(string.Empty) && !msg.Equals(string.Empty))
                                    {
                                        Util.MessageValidation(msg);
                                    }
                                    /*
                                    switch (CheckReserveValidation(Util.NVC(dgAgingReserv.GetValue(i, "LOTID"))))
                                    {
                                        case "BOTH":
                                            //Hold 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                                            //특별(출하금지) 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                                            string msg = MessageDic.Instance.GetMessage("FM_ME_0449") + "\r\n" + MessageDic.Instance.GetMessage("FM_ME_0464");
                                            Util.MessageValidation(msg);
                                            break;
                                        case "HOLD":
                                            //Hold 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                                            Util.MessageValidation("FM_ME_0449");
                                            break;
                                        case "SPECIAL":
                                            //특별(출하금지) 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                                            Util.MessageValidation("FM_ME_0464");
                                            break;
                                    }
                                    */
                                }

                            }
                        }
                    }
                }


            }
        }

        private void dgAgingReserv2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          
        }


        private void dgAgingReserv_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                    string sEQPTID = Util.NVC(dr.Row["EQPTID"]);
                    string sHIGH_TEMP_Z_PSTN = Util.NVC(dr.Row["HIGH_TEMP_Z_PSTN"]);
                    string sLOT_HOLD_YN = Util.NVC(dr.Row["LOT_HOLD_YN"]);
                    string sSPECIAL_YN = Util.NVC(dr.Row["SPCL_FLAG"]);
                    string sSPECIAL_DESC = Util.NVC(dr.Row["SPECIAL_DESC"]);
                    string sSPECIAL_SHIP = Util.NVC(dr.Row["SPECIAL_SHIP"]);
                    string sOVER_MINTIME = Util.NVC(dr.Row["OVER_MINTIME"]);
                    string sDOCV_RCV_RSLT = Util.NVC(dr.Row["DOCV_JUDG_RSLT"]); //20230315 suyong.kim dOCV2 판정 출하예약 InterLock
                    string sAGING_OUT_PRIORITY = Util.GetCondition(cboPriority);

                    //TRAY 상대판정 추가
                    string sA_TJUDGE_CHECK_YN = string.Empty;
                    string sZ_TJUDGE_CHECK_YN = string.Empty;
                    if (bFCS001_031_TRAY_JUDG.Equals(true))
                    {
                        sA_TJUDGE_CHECK_YN = Util.NVC(dr.Row["A_TJUDGE_CHECK_YN"]); //TRAY 상대판정 추가
                        sZ_TJUDGE_CHECK_YN = Util.NVC(dr.Row["Z_TJUDGE_CHECK_YN"]);
                    }

                    //E등급 수량 한계 초과
                    string sE_GR_PASS_LIMIT = string.Empty;
                    if (bFCS001_031_E_GR_PASS_LIMIT.Equals(true))
                    {
                        sE_GR_PASS_LIMIT = Util.NVC(dr.Row["E_GR_PASS_LIMIT"]); //E등급 수량 한계 초과
                    }

                    // OCV 측정일 편차 초과
                    string sOCV_OVER_FLAG = Util.NVC(dr.Row["OCV_OVER_FLAG"]);

                    // Cell 포장 HOLD 추가
                    string sPackHold = Util.NVC(dr.Row["PACK_HOLD_YN"]);

                    //2024.02.13 반송 지시 여부 추가
                    string sTrfCmd = Util.NVC(dr.Row["TRF_YN"]);

                    //2024.02.27 최종 수동판정 여부(수동 판정 대기)
                    string sFIN_MNL_JUDG_YN = Util.NVC(dr.Row["FIN_MNL_JUDG_YN"]);

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    e.Cell.Presenter.BorderBrush = Brushes.LightGray;
                    e.Cell.Presenter.BorderThickness = new Thickness(0.5, 0, 0, 0.5);
                    ///////////////////////////////////////////////////////////////////////////////////

                    // 2024.02.13 반송 지시 존재 여부
                    if (sTrfCmd.Equals("Y"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            //e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aqua);
                    }

                    /* 2015-03-18   정종덕   CSR ID:2563313] 자동차 전지 저전압 한계불량 초과 Lot 자동 Hold 요청의  件
                     * 변경 내용 :  LOT_HOLD  트레이 표시
                     */
                    if (sLOT_HOLD_YN.Equals("Y"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            if (sAGING_OUT_PRIORITY.Equals("8"))
                            {
                                e.Cell.Presenter.IsEnabled = true;
                                //2021-05-11
                                // DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK_FLAG", "Y");
                            }
                            else
                            {
                                e.Cell.Presenter.IsEnabled = false;
                                //2021-05-11
                                // DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK_FLAG", "N");
                            }
                            // e.Cell.Presenter.IsEnabled = sAGING_OUT_PRIORITY.Equals("8") ? true : false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightSalmon);
                    }

                    if (!string.IsNullOrEmpty(sSPECIAL_DESC))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    //200114 KJE: 특별Tray 출하금지
                    if (sSPECIAL_SHIP.Equals("N"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            if (sAGING_OUT_PRIORITY.Equals("8"))
                                e.Cell.Presenter.IsEnabled = true;
                            else
                                e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Plum);
                    }

                    DataRow[] drSelect = tempTable.Select("LOW_VOLT_JUDGE_YN = 'N' AND EQPTID='" + sEQPTID + "'");
                    if (drSelect.Length > 0)
                    {
                        if (e.Cell.Column.Name.ToString() == "CHK")
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightSlateGray);
                    }

                    //200203 : KJE 최소시간 미충족 Tray 출하 금지
                    if (sOVER_MINTIME.Equals("N"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }

                    //20230315 suyong.kim dOCV2 판정 출하예약 InterLock
                    if (sDOCV_RCV_RSLT.Equals("N"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gold);
                    }

                    if (e.Cell.Column.Name.ToString().Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold; //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }


                    //TRAY 상대판정 추가
                    if (bFCS001_031_TRAY_JUDG.Equals(true))
                    {
                        //A_TJUDGE_CHECK_YN, Z_TJUDGE_CHECK_YN 
                        if (sA_TJUDGE_CHECK_YN.Equals("N") || sZ_TJUDGE_CHECK_YN.Equals("N"))
                        {
                            if (e.Cell.Column.Name.ToString().Equals("CSTID"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            }
                            if (e.Cell.Column.Name.ToString().Equals("CHK"))
                            {
                                e.Cell.Presenter.IsEnabled = false;
                            }
                        }
                    }

                    //E등급 수량 한계 초과
                    if (bFCS001_031_E_GR_PASS_LIMIT.Equals(true))
                    {
                        if (sE_GR_PASS_LIMIT.Equals("Y"))
                        {
                            if (e.Cell.Column.Name.ToString().Equals("CSTID"))
                            {
                                //e.Cell.Presenter.Background = new SolidColorBrush(Colors.SkyBlue);
                                e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x9B, 0xC1, 0xE6)); //#FF9BC1E6                                

                            }
                            if (e.Cell.Column.Name.ToString().Equals("CHK"))
                            {
                                e.Cell.Presenter.IsEnabled = false;
                            }
                        }
                    }

                    // OCV 측정일 편차 초과
                    if (sOCV_OVER_FLAG.Equals("Y"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Pink);
                    }

                    // Cell 포장  HOLD 추가
                    if (sPackHold.Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.SaddleBrown);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }

                    //2024.02.27 최종 수동판정 여부(수동 판정 대기)
                    if (bFCS001_031_MNL_JUDG_STANDBY.Equals(true))
                    {
                        if (cboNextOp.SelectedValue.Equals("5"))
                        {
                            if (sFIN_MNL_JUDG_YN.Equals("N"))
                            {
                                if (e.Cell.Column.Name.ToString().Equals("CHK"))
                                {
                                    if (sAGING_OUT_PRIORITY.Equals("8"))
                                        e.Cell.Presenter.IsEnabled = true;
                                    else
                                        e.Cell.Presenter.IsEnabled = false;
                                }
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGreen);
                            }
                        }
                    }

                    //2024.03.04 고온Aging 단 정보 추가 (디가스 전만 표기) - 추후 확인
                    //if (string.IsNullOrEmpty(sHIGH_TEMP_Z_PSTN))
                    //{
                    //    if (e.Cell.Column.Name.ToString().Equals("HIGH_TEMP_Z_PSTN"))
                    //    {
                    //        e.Cell.Column.Visibility = Visibility.Collapsed;
                    //    }
                    //}
                    //else
                    //{
                    //    if (e.Cell.Column.Name.ToString().Equals("HIGH_TEMP_Z_PSTN"))
                    //    {
                    //        e.Cell.Column.Visibility = Visibility.Visible;
                    //    }
                    //}
                }
            }));

        }

        private void dgAgingReserv2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    e.Cell.Presenter.BorderBrush = Brushes.LightGray;
                    e.Cell.Presenter.BorderThickness = new Thickness(0.5, 0, 0, 0.5);
                    ///////////////////////////////////////////////////////////////////////////////////

                }
            }));

        }


        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            dgAgingReserv.ClearValidation();

            //Util.DataGridCheckAllChecked(dgAgingReserv);
            //2021-05-11 Lot Hold, 특별관리 출하금지 등의 사유로 전체 선택 Validation 추가
            for (int i = dgAgingReserv.TopRows.Count; i < dgAgingReserv.Rows.Count - 1; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[i].DataItem, "CHK_FLAG")).Equals("Y"))
                {
                    if (dgAgingReserv.Rows[i].IsEditable.Equals(true))
                    {
                        DataTableConverter.SetValue(dgAgingReserv.Rows[i].DataItem, "CHK", true);
                    }
                }
            }

            bool validHold = false;
            bool validSpecial = false;
            bool validocv = false;
            int checkCount = 0;
            string lotList = string.Empty;

            foreach (DataRow drChk in dgAgingReserv.GetCheckedDataRow("CHK"))
            {
                lotList += Util.NVC(drChk["LOTID"]) + ",";

                checkCount++;
                if (checkCount > 200)
                {
                    string chkValidation = CheckReserveValidation(lotList);
                    if (chkValidation.Contains("HOLD"))
                    {
                        validHold = true;
                    }
                    if (chkValidation.Contains("SPECIAL"))
                    {
                        validSpecial = true;

                    }
                    if (chkValidation.Contains("OCV"))
                    {
                        validocv = true;
                    }
                    /*
                    switch (CheckReserveValidation(lotList))
                    {
                        case "BOTH":
                            validHold = true;
                            validSpecial = true;
                            break;
                        case "HOLD":
                            validHold = true;
                            break;
                        case "SPECIAL":
                            validSpecial = true;
                            break;
                    }
                    */

                    checkCount = 0;
                    lotList = string.Empty;
                }
            }
            string chkValidation2 = CheckReserveValidation(lotList);
            if (chkValidation2.Contains("HOLD"))
            {
                validHold = true;
            }
            if (chkValidation2.Contains("SPECIAL"))
            {
                validSpecial = true;

            }
            if (chkValidation2.Contains("OCV"))
            {
                validocv = true;
            }
            /*switch (CheckReserveValidation(lotList))
            {
                case "BOTH":
                    validHold = true;
                    validSpecial = true;
                    break;
                case "HOLD":
                    validHold = true;
                    break;
                case "SPECIAL":
                    validSpecial = true;
                    break;
            }*/
            string msg = string.Empty;
            if (validHold) msg = MessageDic.Instance.GetMessage("FM_ME_0449");
            if (validSpecial)
            {
                if (!msg.Equals(string.Empty)) msg += "\r\n";
                msg += MessageDic.Instance.GetMessage("FM_ME_0464");
            }
            if (validocv)
            {
                if (!msg.Equals(string.Empty)) msg += "\r\n";
                msg += MessageDic.Instance.GetMessage("FM_ME_0519");
            }

            if (!msg.Equals(string.Empty))
            {
                Util.MessageValidation(msg);
            }

            /*

            if (validHold && validSpecial)
            {
                //Hold 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                //특별(출하금지) 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                string msg = MessageDic.Instance.GetMessage("FM_ME_0449") + "\r\n" + MessageDic.Instance.GetMessage("FM_ME_0464");
                Util.MessageValidation(msg);
            }
            else if (validHold)
            {
                //Hold 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                Util.MessageValidation("FM_ME_0449");
            }
            else if (validSpecial)
            {
                //특별(출하금지) 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                Util.MessageValidation("FM_ME_0464");
            }*/
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgAgingReserv);
        }

        //Route 버튼 기능 추가
        private void chkRoute_Checked(object sender, RoutedEventArgs e)
        {
            this.btnRoute.IsEnabled = true;
        }

        private void chkRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            this.btnRoute.IsEnabled = false;
        }

        private void btnRoute_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            if (cboLine.GetBindValue() == null)
            {
                cboLine.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0044"), true); //Line을 선택해주세요.   
                return;
            }

            try
            {
                string sMDLLOT_ID = Util.GetCondition(cboModel, bAllNull: true);
                string sEQSGID = Util.GetCondition(cboLine, bAllNull: true);

                Load_FCS001_031_ROUTE_LIST(sMDLLOT_ID, sEQSGID);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void Load_FCS001_031_ROUTE_LIST(string sMDLLOT_ID, string sEQSGID)
        {

            //FCS001_026_STATUS_CHANGE fcs001_026_status_change = new FCS001_026_STATUS_CHANGE();
            //fcs001_026_status_change.FrameOperation = FrameOperation;
            //if (fcs001_026_status_change != null)
            //{
            //    object[] Parameters = new object[1];
            //    Parameters[0] = sTrayList;
            //    C1WindowExtension.SetParameters(fcs001_026_status_change, Parameters);

            //    fcs001_026_status_change.Closed += new EventHandler(wndPopup_Closed);

            //    // 팝업 화면 숨겨지는 문제 수정.
            //    this.Dispatcher.BeginInvoke(new Action(() => fcs001_026_status_change.ShowModal()));
            //}

            FCS001_031_ROUTE_LIST RouteList = new FCS001_031_ROUTE_LIST();
            RouteList.FrameOperation = FrameOperation;

            if (RouteList != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = sMDLLOT_ID;
                Parameters[1] = sEQSGID;
                C1WindowExtension.SetParameters(RouteList, Parameters);
                //this.FrameOperation.OpenMenuFORM("SFU010710072", "FCS001_031_ROUTE_LIST", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Route List"), true, Parameters);                

                RouteList.Closed += new EventHandler(wndPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => RouteList.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS001_031_ROUTE_LIST window = sender as FCS001_031_ROUTE_LIST;
            //GetList();
            this.grdMain.Children.Remove(window);
        }

        private void btnPlan_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;


            try
            {
                Load_FP_DAILY_PLAN(LoginInfo.CFG_AREA_ID);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void Load_FP_DAILY_PLAN(string sAREAID)
        {
            FCS001_031_FP_PLAN RouteList = new FCS001_031_FP_PLAN();
            RouteList.FrameOperation = FrameOperation;

            if (RouteList != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = sAREAID;
                C1WindowExtension.SetParameters(RouteList, Parameters);
                //this.FrameOperation.OpenMenuFORM("SFU010710072", "FCS001_031_ROUTE_LIST", "LGC.GMES.MES.FCS001", ObjectDic.Instance.GetObjectName("Route List"), true, Parameters);                

                RouteList.Closed += new EventHandler(fpPopup_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => RouteList.ShowModal()));
            }
        }

        private void fpPopup_Closed(object sender, EventArgs e)
        {
            FCS001_031_FP_PLAN window = sender as FCS001_031_FP_PLAN;
            //GetList();
            this.grdMain.Children.Remove(window);
        }

        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            /* 10개씩 분리 처리
             * LOT ID 가 중복해서 들어가면 안되어 체크 후 처리.
             * 같은 UNIT 에서 분리되어 안들어가게 처리.
             */
            try
            {
                object[] arguments = e.Arguments as object[];

                List<string> processTrayId = new List<string>();

                DataTable dtRqstAll = arguments[0] as DataTable;

                DataTable dtRqst = dtRqstAll.Clone();

                int reservCnt = 0;
                int addCnt = 0;
                foreach (DataRow dataRow in dtRqstAll.Rows)
                {
                    if (processTrayId.Contains(Util.NVC(dataRow["LOTID"]))) continue;

                    addCnt++;
                    // MES 2.0 ItemArray 위치 오류 Patch
                    //dtRqst.Rows.Add(dataRow.ItemArray);
                    dtRqst.AddDataRow(dataRow);
                    processTrayId.Add(Util.NVC(dataRow["LOTID"]));

                    // 해당 LOTID 의 같은 UNIT 가 있으면 같이 포함.
                    List<DataRow> chkRows = dtRqstAll.AsEnumerable()
                        .Where(w => w.Field<string>("UNITID").Equals(Util.NVC(dataRow["UNITID"])) && !w.Field<string>("LOTID").Equals(Util.NVC(dataRow["LOTID"])))
                        .ToList();
                    if (chkRows != null && chkRows.Count > 0)
                    {
                        foreach (DataRow addRow in chkRows)
                        {
                            if (processTrayId.Contains(Util.NVC(addRow["LOTID"]))) continue;

                            addCnt++;
                            // MES 2.0 ItemArray 위치 오류 Patch
                            //dtRqst.Rows.Add(addRow.ItemArray);
                            dtRqst.AddDataRow(addRow);
                            processTrayId.Add(Util.NVC(addRow["LOTID"]));
                        }
                    }

                    if (dtRqst.Rows.Count >= 10 || addCnt >= dtRqstAll.Rows.Count)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_AGING_UNLOAD_RESERVATION", "INDATA", "OUTDATA", dtRqst, menuid: FrameOperation.MENUID.ToString());
                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            reservCnt += Util.NVC_Int(dtRslt.Rows[0]["RESER_CNT"]);
                        }
                        else
                        {
                            return Util.NVC(dtRslt.Rows[0]["MSGNAME"]);
                        }

                        e.Worker.ReportProgress(Convert.ToInt16((double)addCnt / (double)dtRqstAll.Rows.Count * 100));

                        dtRqst.Rows.Clear();
                    }
                }
                return reservCnt;
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                xProgress.Percent = percent;
                xProgress.ProgressText = "Working...";
                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                xProgress.Visibility = Visibility.Collapsed;

                object[] arguments = e.Arguments as object[];
                string sMsg = arguments[1] as string;

                if (e.Result != null && e.Result is int)
                {
                    int reservCnt = Util.NVC_Int(e.Result);

                    Util.AlertInfo(sMsg, new string[] { reservCnt.ToString() });
                }
                else if (e.Result != null && e.Result is Exception)
                {
                    Exception ex = e.Result as Exception;
                    Util.MessageException(ex);

                    //string msg = MessageDic.Instance.GetMessage("FM_ME_0202") + "\r\n\r\n" + Util.NVC(e.Result);
                    //Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }
                else
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202");
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        private string CheckReserveValidation(string lotIdList)
        {
            try
            {
                string returnValue = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTLIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTLIST"] = lotIdList;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_UNLOAD_RESERVATION_UI_HOLD_CHECK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && Util.GetCondition(cboPriority).Equals("5"))//예약대기 상태일때만 확인
                {
                    int holdCount = dtResult.AsEnumerable().Where(r => r["QMS_HOLD_FLAG"].Equals("Y")).Count();
                    int specialCount = dtResult.AsEnumerable().Where(r => r["SPCL_SHIP_HOLD_FLAG"].Equals("Y")).Count();
                    int ocvoverCount = dtResult.AsEnumerable().Where(r => r["OCV_HOLD_FLAG"].Equals("Y")).Count();
                    int finjudgCount = dtResult.AsEnumerable().Where(r => r["FIN_MNL_JUDG_FLAG"].Equals("Y")).Count();

                    if (holdCount > 0) returnValue += "HOLD";
                    if (specialCount > 0) returnValue += "SPECIAL";
                    if (ocvoverCount > 0) returnValue += "OCV";
                    if (finjudgCount > 0) returnValue += "FIN_MNL_JUDG";
                    // if (holdCount > 0 && specialCount > 0) returnValue = "BOTH";

                    if (holdCount > 0 || specialCount > 0 || ocvoverCount > 0 || finjudgCount > 0)
                    {
                        foreach (DataRow drLot in dtResult.Rows)
                        {
                            for (int row = 0; row < dgAgingReserv.Rows.Count; row++)
                            {
                                if (dgAgingReserv.GetValue(row, "CSTID") == null) continue;

                                string cstID = Util.NVC(dgAgingReserv.GetValue(row, "CSTID"));

                                if (cstID.Equals(Util.NVC(drLot["CSTID_IN"])) || cstID.Equals(Util.NVC(drLot["CSTID"])))
                                {
                                    // 체크 해제
                                    dgAgingReserv.SetValue(row, "CHK", false);

                                    int holdCstCount = dtResult.AsEnumerable().Where(r => (r["CSTID"].Equals(cstID) || r["CSTID_IN"].Equals(cstID)) && r["QMS_HOLD_FLAG"].Equals("Y")).Count();
                                    int specialCstCount = dtResult.AsEnumerable().Where(r => (r["CSTID"].Equals(cstID) || r["CSTID_IN"].Equals(cstID)) && r["SPCL_SHIP_HOLD_FLAG"].Equals("Y")).Count();
                                    int ocvCstCount = dtResult.AsEnumerable().Where(r => (r["CSTID"].Equals(cstID) || r["CSTID_IN"].Equals(cstID)) && r["OCV_HOLD_FLAG"].Equals("Y")).Count();
                                    int judgCstCount = dtResult.AsEnumerable().Where(r => (r["CSTID"].Equals(cstID) || r["CSTID_IN"].Equals(cstID)) && r["FIN_MNL_JUDG_FLAG"].Equals("Y")).Count();

                                    string msg = string.Empty;
                                    if (holdCstCount > 0) msg = MessageDic.Instance.GetMessage("FM_ME_0449");
                                    if (specialCstCount > 0)
                                    {
                                        if (!msg.Equals(string.Empty)) msg += "\r\n";
                                        msg += MessageDic.Instance.GetMessage("FM_ME_0464");
                                    }
                                    if (ocvCstCount > 0)
                                    {
                                        if (!msg.Equals(string.Empty)) msg += "\r\n";
                                        msg += MessageDic.Instance.GetMessage("FM_ME_0519");
                                    }
                                    if (judgCstCount > 0)
                                    {
                                        if (!msg.Equals(string.Empty)) msg += "\r\n";
                                        msg += MessageDic.Instance.GetMessage("FM_ME_0584");
                                    }
                                    dgAgingReserv.SetRowValidation(row, msg);
                                }
                            }
                        }
                    }
                }


                return returnValue;

            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }

            return string.Empty; ;
        }

        private void GetList()
        {
            try
            {
                this.ClearValidation();

                if (cboLine.GetBindValue() == null)
                {
                    cboLine.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0044"), true);
                    return;
                }

                if (cboModel.GetBindValue() == null && cboModel.Text != "-ALL-" )
                {
                    cboModel.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0129"), true);
                    return;
                }

                if (cboLotType.GetBindValue() == null)
                {
                    cboLotType.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0050"), true);
                    return;
                }

                Util.gridClear(dgAgingReserv);
                tempTable = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));
                dtRqst.Columns.Add("AGING_ISS_PRIORITY_NO", typeof(string));
                dtRqst.Columns.Add("TO_PROCID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("TO_PROC_FIX", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtRqst.Columns.Add("ABNORM_FLAG", typeof(string));

                dtRqst.Columns.Add("LOTTYPE", typeof(string));  //2022.12.21  LotType  조회조건 추가

                if (bFCS001_031_ROUTE_LIST.Equals(true))
                {
                    dtRqst.Columns.Add("USE_FLAG", typeof(string)); //Route select 팝업 버튼 추가
                }

                if (bFCS001_031_SC_STAT_CHECK.Equals(true))
                {
                    dtRqst.Columns.Add("EIOSTAT", typeof(string)); //S/C 상태가 작동중이 아니면 리스트에서 제외
                }

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);

                dr["LOTTYPE"] = Util.GetCondition(cboLotType, sMsg: "FM_ME_0050");  //2022.12.21  LotType  조회조건 추가

                if (MenuKind == AgingReserv.NH_AGING_REV)
                {
                    dr["PROD_LOTID"] = Util.GetCondition(cboLotId, bAllNull: true);
                    dr["TO_PROCID"] = string.IsNullOrEmpty(NEXT_PROC) ? "5,D" : NEXT_PROC;
                }
                else if (MenuKind == AgingReserv.NP_AGING_REV_BOX_JIG)
                {
                    dr["PROD_LOTID"] = Util.GetCondition(txtLotID, bAllNull: true);
                    dr["TO_PROCID"] = string.IsNullOrEmpty(NEXT_PROC) ? "J,1" : NEXT_PROC;
                }

                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["AGING_ISS_PRIORITY_NO"] = Util.GetCondition(cboPriority);
                dr["PROC_GR_CODE"] = Util.GetCondition(cboAgingType, sMsg: "FM_ME_0336"); //Aging 유형을 선택해주세요.
                dr["TO_PROC_FIX"] = Util.GetCondition(cboNextOp, sMsg: "FM_ME_0338"); //차기공정을 선택해주세요.
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = Util.GetCondition(cboSCLine, bAllNull: true);

                dr["WIPSTAT"] = "PROC";
                dr["ISS_RSV_FLAG"] = "N";
                dr["ABNORM_FLAG"] = "N";

                if (bFCS001_031_ROUTE_LIST.Equals(true))
                {
                    dr["USE_FLAG"] = "Y";  //Route select 팝업 버튼 추가
                }
                if (bFCS001_031_SC_STAT_CHECK.Equals(true))
                {
                    dr["EIOSTAT"] = "F,R,U,W";  //S/C 상태가 작동중이 아니면 리스트에서 제외, FCS 기준 T 상태면 제외
                }

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                new ClientProxy().ExecuteService("BR_GET_AGING_CAN_UNLOAD_TRAY", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            DataTable dtRsltCnt = result.Clone();

                            DataTable dtTable = GetNewData(result);

                            if (dtTable == null || dtTable.Rows.Count == 0)
                            {
                                return;
                            }

                            for (int i = 0; i < Convert.ToUInt32(Util.GetCondition(cboSearchChount)) && i < dtTable.Rows.Count; i++)
                            {
                                dtRsltCnt.ImportRow(dtTable.Rows[i]);
                            }

                            //2021-05-11 전체선택 체크 시 체크 불가능한 Row 선별을 위하여 Flag 컬럼 추가
                            dtRsltCnt.Columns.Add("CHK_FLAG");
                            for (int i = 0; i < dtRsltCnt.Rows.Count; i++)
                            {

                                //Row별 check Flag 확인

                                string sEQPTID = Util.NVC(dtRsltCnt.Rows[i]["EQPTID"]);
                                string sLOT_HOLD_YN = Util.NVC(dtRsltCnt.Rows[i]["LOT_HOLD_YN"]);
                                string sSPECIAL_YN = Util.NVC(dtRsltCnt.Rows[i]["SPCL_FLAG"]);
                                string sSPECIAL_SHIP = Util.NVC(dtRsltCnt.Rows[i]["SPECIAL_SHIP"]);
                                string sOVER_MINTIME = Util.NVC(dtRsltCnt.Rows[i]["OVER_MINTIME"]);
                                string sAGING_OUT_PRIORITY = Util.GetCondition(cboPriority);
                                string sOCV_OVER_FLAG = Util.NVC(dtRsltCnt.Rows[i]["OCV_OVER_FLAG"]);
                                string sFIN_MNL_JUDG_YN = Util.NVC(dtRsltCnt.Rows[i]["FIN_MNL_JUDG_YN"]);

                                dtRsltCnt.Rows[i]["CHK_FLAG"] = "Y"; // Y : 선택 가능, N : 선택 불가능

                                //E20240507-000384 예약자 컬럼 추가
                                if (dtRsltCnt.Rows[i]["RESVDTTM"].ToString().Length == 0)
                                {
                                    dtRsltCnt.Rows[i]["UPDUSER"] = "";
                                }

                                /* 2015-03-18   정종덕   CSR ID:2563313] 자동차 전지 저전압 한계불량 초과 Lot 자동 Hold 요청의  件
                                 * 변경 내용 :  LOT_HOLD  트레이 표시
                                 */
                                if (sAGING_OUT_PRIORITY.Equals("5")) //예약 대기일때만 전체선택 불가
                                {
                                    if (sLOT_HOLD_YN.Equals("Y"))
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                    }
                                    //특별관리 트레이 출하금지 선택 시 출하금지
                                    if (sSPECIAL_SHIP.Equals("N"))
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                    }

                                    //최소시간 미충족시 출하금지
                                    if (sOVER_MINTIME.Equals("N"))
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                    }

                                    //저전압
                                    if (dtRsltCnt.Rows[i]["LOW_VOLT_JUDGE_YN"].Equals("N"))
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                    }

                                    //20230315 suyong.kim dOCV2 판정 출하예약 InterLock
                                    if (dtRsltCnt.Rows[i]["DOCV_JUDG_RSLT"].Equals("N"))
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                    }

                                    //TRAY 상대판정 추가
                                    if (bFCS001_031_TRAY_JUDG.Equals(true))
                                    {
                                        if (dtRsltCnt.Rows[i]["A_TJUDGE_CHECK_YN"].Equals("N") || dtRsltCnt.Rows[i]["Z_TJUDGE_CHECK_YN"].Equals("N"))
                                        {
                                            dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                        }
                                    }

                                    //E등급 수량 한계 초과
                                    if (bFCS001_031_E_GR_PASS_LIMIT.Equals(true))
                                    {
                                        if (dtRsltCnt.Rows[i]["E_GR_PASS_LIMIT"].Equals("Y"))
                                        {
                                            dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                        }
                                    }

                                    //OCV 측정일 편차 초과
                                    if (sOCV_OVER_FLAG.Equals("Y"))
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                    }

                                    if (bFCS001_031_MNL_JUDG_STANDBY.Equals(true))
                                    {
                                        if (cboNextOp.SelectedValue.Equals("5")) //EOL만
                                        {
                                            //최종 수동판정 여부(수동 판정 대기)
                                            if (sFIN_MNL_JUDG_YN.Equals("N"))
                                            {
                                                dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                            }
                                        }
                                    }
                                }
                            }

                            tempTable = dtRsltCnt.Copy();

                            Util.GridSetData(dgAgingReserv, dtRsltCnt, FrameOperation, true);
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
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

        private void GetList2()
        {
            try
            {
                this.ClearValidation();

                if (cboLine2.GetBindValue() == null)
                {
                    cboLine2.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0044"), true);
                    return;
                }

                if (cboLotType2.GetBindValue() == null)
                {
                    cboLotType2.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0050"), true);
                    return;
                }

                if ((dtpToDate.SelectedDateTime.Date - dtpFromDate.SelectedDateTime.Date).Days >= 7)
                {
                    Util.Alert("FM_ME_0231"); //조회기간은 7일을 초과할 수 없습니다.
                    return;
                }

                Util.gridClear(dgAgingReserv2);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                //dtRqst.Columns.Add("TO_PROCID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("TO_PROC_FIX", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtRqst.Columns.Add("ABNORM_FLAG", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));  

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_TIME"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                dr["TO_TIME"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpToTime.DateTime.Value.ToString("HH:mm:59");

                dr["EQSGID"] = Util.GetCondition(cboLine2, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel2, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute2, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper2, bAllNull: true);
                if(MenuKind == AgingReserv.NH_AGING_REV)
                {
                    dr["PROD_LOTID"] = Util.GetCondition(cboLotId2, bAllNull: true);
                }
                else
                {
                    dr["PROD_LOTID"] = !string.IsNullOrWhiteSpace(txtLotID2.Text) ? txtLotID2.Text : null;
                }

                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial2, bAllNull: true);
                dr["ACTID"] = Util.GetCondition(cboActID);
                //dr["TO_PROCID"] = string.IsNullOrEmpty(NEXT_PROC) ? "5,D" : NEXT_PROC;
                dr["PROC_GR_CODE"] = Util.GetCondition(cboAgingType2, sMsg: "FM_ME_0336"); //Aging 유형을 선택해주세요.
                dr["TO_PROC_FIX"] = Util.GetCondition(cboNextOp2, sMsg: "FM_ME_0338"); //차기공정을 선택해주세요.
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = Util.GetCondition(cboSCLine2, bAllNull: true);
                dr["ISS_RSV_FLAG"] = "N";
                dr["ABNORM_FLAG"] = "N";
                dr["LOTTYPE"] = Util.GetCondition(cboLotType2, sMsg: "FM_ME_0050");  //2022.12.21  LotType  조회조건 추가

                dtRqst.Rows.Add(dr);

                //dgAgingReserv.ExecuteService("DA_SEL_AGING_MANUAL_RESERVATION_HIST", "RQSTDT", "RSLTDT", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_AGING_MANUAL_RESERVATION_HIST", "RQSTDT", "OUTDATA", dtRqst);

                Util.GridSetData(dgAgingReserv2, dtRslt, this.FrameOperation, false);

            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

        #endregion


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

        private void dgAgingReserv_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    e.Cell.Presenter.IsEnabled = true;
                }
            }
        }

        private void dgAgingReserv2_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    e.Cell.Presenter.IsEnabled = true;
                }
            }
        }

        private void cboAgingType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            //GetCommonCode();
            EQPT_GR_TYPE_CODE = e.NewValue.ToString();
            string[] sFilter = { EQPT_GR_TYPE_CODE, null };
            _combo.SetCombo(cboSCLine, CommonCombo_Form.ComboStatus.ALL, sCase: "SCLINE", sFilter: sFilter);
            if (cboSCLine.Items.Count > 0)
            {
                cboSCLine.SelectedIndex = 0;
            }

            // 차기공정 기본 설정(AGING_NEXT_PROC Attribute1, Attribute2 매칭)
            if (DEFAULT_NEXT_PROC != null && DEFAULT_NEXT_PROC.ContainsKey(EQPT_GR_TYPE_CODE))
            {
                cboNextOp.SelectedValue = DEFAULT_NEXT_PROC[EQPT_GR_TYPE_CODE];
            }            
        }

        private void cboAgingType2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            //GetCommonCode();
            EQPT_GR_TYPE_CODE = e.NewValue.ToString();
            string[] sFilter = { EQPT_GR_TYPE_CODE, null };
            _combo.SetCombo(cboSCLine2, CommonCombo_Form.ComboStatus.ALL, sCase: "SCLINE", sFilter: sFilter);
            if (cboSCLine2.Items.Count > 0)
            {
                cboSCLine2.SelectedIndex = 0;
            }

            // 차기공정 기본 설정(AGING_NEXT_PROC Attribute1, Attribute2 매칭)
            if (DEFAULT_NEXT_PROC != null && DEFAULT_NEXT_PROC.ContainsKey(EQPT_GR_TYPE_CODE))
            {
                cboNextOp2.SelectedValue = DEFAULT_NEXT_PROC[EQPT_GR_TYPE_CODE];
            }
        }

        private void cboLotType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboLotParent = { cboLine, cboModel, cboRoute, cboSpecial, cboAgingType, cboNextOp, cboLotType };
                _combo.SetCombo(cboLotId, CommonCombo_Form.ComboStatus.ALL, sCase: "LOT", cbParent: cboLotParent); //2024.03.14 LH& : Lot 유형 선택에 따른 PKG Lot ID 필터링 반영
            }
        }

        private void cboLotType2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboLotParent = { cboLine2, cboModel2, cboRoute2, cboSpecial2, cboAgingType2, cboNextOp2, cboLotType2 };
                _combo.SetCombo(cboLotId2, CommonCombo_Form.ComboStatus.ALL, sCase: "LOT", cbParent: cboLotParent); //2024.03.14 LH& : Lot 유형 선택에 따른 PKG Lot ID 필터링 반영
            }
        }

        private DataTable GetNewData(DataTable dt)
        {
            DataTable dtNewData = new DataTable();
            try
            {
                if (dt.Rows.Count > 0)
                {
                    dtNewData = dt.Copy();

                    if (chkLotHold.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("LOT_HOLD_YN <> 'Y'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkLowVolt.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("LOW_VOLT_JUDGE_YN <> 'N'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkSpecial.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("ISNULL(SPECIAL_DESC, '') = ''");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkSpecialShipBan.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("SPECIAL_SHIP <> 'N'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkMinTime.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("OVER_MINTIME <> 'N'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkFitDocv.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("DOCV_JUDG_RSLT <> 'N'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    //TRAY 상대판정 추가
                    if (bFCS001_031_TRAY_JUDG.Equals(true))
                    {
                        if (chkTrayJudg.IsChecked.Equals(false))
                        {
                            if (dtNewData != null && dtNewData.Rows.Count > 0)
                            {
                                DataRow[] drSelect = dtNewData.Select("A_TJUDGE_CHECK_YN <> 'N'  and Z_TJUDGE_CHECK_YN <> 'N'");
                                if (drSelect.Length > 0)
                                {
                                    dtNewData = drSelect.CopyToDataTable();
                                }
                                else
                                {
                                    dtNewData = null;
                                }
                            }
                        }
                    }

                    //E등급 수량 한계 초과
                    if (bFCS001_031_E_GR_PASS_LIMIT.Equals(true))
                    {
                        if (this.chkEGRPassLimit.IsChecked.Equals(false))
                        {
                            if (dtNewData != null && dtNewData.Rows.Count > 0)
                            {
                                DataRow[] drSelect = dtNewData.Select("E_GR_PASS_LIMIT <> 'Y'");
                                if (drSelect.Length > 0)
                                {
                                    dtNewData = drSelect.CopyToDataTable();
                                }
                                else
                                {
                                    dtNewData = null;
                                }
                            }
                        }
                    }

                    // OCV 측정일 편차 초과
                    if (chkOCVOver.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("OCV_OVER_FLAG <> 'Y'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    //Cel 포장 Hold, 추가

                    if (chkPackHold.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("PACK_HOLD_YN <> 'Y'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    // OCV 측정일 편차 초과
                    if (chkTrfCmd.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("TRF_YN <> 'Y'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    //수동 판정 대기 추가
                    if (bFCS001_031_MNL_JUDG_STANDBY.Equals(true))
                    {
                        if (cboNextOp.SelectedValue.Equals("5"))
                        {
                            if (chkMNL_JUDG_STANDBY.IsChecked.Equals(false))
                            {
                                if (dtNewData != null && dtNewData.Rows.Count > 0)
                                {
                                    DataRow[] drSelect = dtNewData.Select("FIN_MNL_JUDG_YN <> 'N'");
                                    if (drSelect.Length > 0)
                                    {
                                        dtNewData = drSelect.CopyToDataTable();
                                    }
                                    else
                                    {
                                        dtNewData = null;
                                    }
                                }
                            }
                        }
                    }
                }

                return dtNewData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void chkAllJudg_Checked(object sender, RoutedEventArgs e)
        {
            chkTrayJudg.IsChecked = true;
            chkFitDocv.IsChecked = true;
            chkLotHold.IsChecked = true;
            chkLowVolt.IsChecked = true;
            chkEGRPassLimit.IsChecked = true;
            chkSpecial.IsChecked = true;
            chkMinTime.IsChecked = true;
            chkSpecialShipBan.IsChecked = true;
            chkOCVOver.IsChecked = true;
            chkPackHold.IsChecked = true;


        }

        private void chkAllJudg_Unchecked(object sender, RoutedEventArgs e)
        {
            chkTrayJudg.IsChecked = false;
            chkFitDocv.IsChecked = false;
            chkLotHold.IsChecked = false;
            chkLowVolt.IsChecked = false;
            chkEGRPassLimit.IsChecked = false;
            chkSpecial.IsChecked = false;
            chkMinTime.IsChecked = false;
            chkSpecialShipBan.IsChecked = false;
            chkOCVOver.IsChecked = false;
            chkPackHold.IsChecked = false;
        }

    }
}
