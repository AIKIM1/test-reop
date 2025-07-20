/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.03.08  용하옥    
  2017.03.14  정문교      생산실적 조회 화면 코터공정에서 Slurry 탭 제거 바랍니다
  2017.04.27  정문교      라미, 폴딩 감열지 발행시 DISPATCH_YN 가 N인 경우 자동 DISPATCH 추가
  2017.11.21  INS염규범   GEMS FOLDING 실적 마감 단어 수정 요청 CSR20171011_02178  - 기존 _StackingYN 의경우 설비 선택옵션에서 결정되기 때문에, ALL 일경우 사용 불가능 
  2018.10.01  이대근      [CSR ID:3805352] 생산실적 조회(생산Lot 기준) 
  2019.05.17  이상훈      [C20190415_74474] x-ray pallet 라벨 재 발행 기능 추가
  2019.07.16  김동일      폴란드 1,2 동 라미 공정진척 3LOSS 적용에 따른 수정
  2020.01.06  정문교      품질정보, 색지정보, 불량태그 조회시 동정보 조건을 LoginInfo.CFG_AREA_ID -> LOT의 동 정보로 변경
  2020.03.11  김동일      재공이력 정보 조회 기능 추가.
  2020.05.08  김태균      ROLL PRESS 공정에서도 TAG 수를 보여줘야 하는 AREA 체크 추가. CSRC20200508-000359
  2020.05.21  김태균      COATING 공정에서 HALF SIDE(신규)를 보여 줘야 하는 AREA 추가 C20200519-000286
  2020.06.18  정문교      C20200610-000491 - 등급 정보 칼럼 추가 
  2020.07.01  정문교      C20200630-000328 - 설비 수집 TAG 수 칼럼 추가 
  2021.08.18  정종원      ROLLMAP 버튼 활성화 기준 변경 (설비 -> LOT 단위)
  2021.08.20  정재홍      C20210720-000108 - INPUT_SECTION_ROLL_DIRCT, EM_SECTION_ROLL_DIRCTN 칼럼 추가
  2021.08.23  김지은      [GM JV Proj.]조회조건-생산구분 추가
  2021.09.06  오화백      FastTrack 컬럼 추가
  2021.09.09  신광희      롤맵 팝업 차트 버전 파라메터 추가
  2021.11.02  신광희      롤맵 팝업 호출 시 EquipmentName -> EquipmentName + [EquipmentId] 로 표기되도록 수정
  2021.12.01  정재홍      C20211114-000011 Rewinding Process 전극생산Site Column 추가
  2022.03.24  서용호      C20220110-000251 슬라이딩 측정값, 롤방향 컬럼 추가
  2022.04.28  정재홍      C20220414-000238 - 절연 자주검사 설비값 컬럼 추가 (품질정보)
  2022.05.11  정재홍      C20220406-000241 - 설비불량정보 TAB 불량검출수량 컬럼 추가
  2022.06.13  김지은      COATING, ROLL PRESS공정에서 CSTID 보여주도록 추가
  2022.06.23  정재홍      C20220117-000174 - 롤프레스 공정 파단횟수 컬럼 추가 / 재와인딩 공정시 슬리터 Tag 컬럼 표기
  2022.06.24  이호섭      C20220622-000541 - 2동 생산 실적 화면 내외수 변경 전/후 비교화면 추가 요청건(롤프레스 공정 추가)
  2022.11.04  강호운      C20221107-000542 - LASER_ABLATION 공정추가
  2022.11.25  방민재      C20221006-000307 - 동별 CT검사 CHECK 항목 구분
  2023.02.21  윤기업      롤맵 수불 조회 버튼 추가
  2023.03.08  김대현      품질정보탭 측정값이 상/하한값 범위를 벗어난 경우 배경색 Red적용
  2023.04.07  김동일      E20230311-000012 - 조립 CT 공정 바구니 감열지 재발행 기능 추가
  2023.05.09  김대현      세부정보 Filter 적용 후 Lot 선택시 Filter 내용 유지
  2023.08.11  김태우      NFF 슬리팅 E4000 공정 등급 정보 수정 기능 추가
  2023.09.19  김태우      NFF 슬리팅.등급 데이터 유/무 에 따른 처리. 저장버튼, ROW색상등
  2023.09.25  이병윤      SHOPID기준 F030 의  와인더(A2000) 공정인 경우 전극 등급 정보탭 추가
  2023.10.10  연현정      E20231005-000636 : 품질 정보 AVG_VAL 추가
  2023.10.13  성민식      E20230330-001350 : ESNJ 라미, 폴딩 공정 완성Lot 탭 REMARK 컬럼 Visible
  2023.10.16  주동석      와인딩 공정일경우 대차ID VISIBLE
  2023.10.25  강성묵      E20231005-000636 : 품질 정보 AVG_VAL 측정 데이터가 존재하는 항목만 계산
  2023.10.31  김도형      [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
  2023.11.07  김도형      [E20231031-000604] 생산실적조뢰(생산Lot기준) 모델 조회조건 TEXTBOX 기능 삭제 처리
  2023.11.07  윤지해      E20230828-000349 CT 공정 작업유형 컬럼 Visible (2023.11.14 배포 예정)
  2023.11.22  김태우      NFF 슬리팅. 전극등급정보탭 판정등급 안나오는 현상 수정
  2023.11.24  안유수      E20231006-001025 패키지공정 특별관리 Tray Group ID - SPCL_LOT_GR_CODE 컬럼 추가
  2023.12.07  김태우      NFF 슬리팅. 전극등급정보탭 판정등급 수정기능 삭제.
  2023.12.12  윤지해      E20231211-000182 조회용 표준 공구 ID 추가
  2023.12.26  황재원      E20231107-000486 ESNJ 라미,폴딩 공정 실적확정(완성Lot tab) 특이사항 컬럼 Visible
  2024.01.19  오수현      E20230901-001504 ESL 및 대차ID 자동리딩 적용 - MMD 와인딩 대차아이디 보기 여부의 기준 변경(동->라인)
  2024.01.25  오수현      E20230901-001504 ESL 및 대차ID 자동리딩 적용 - MMD 와인딩 대차아이디 보기 여부의 기준 변경(동->라인)
  2024.02.14  김용군      E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
  2024.02.22  김한        조회 버튼 클릭 시 기존 롤맵실적조회 버튼 Visibility 수정 추가
  2024.03.18  조범모      [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
  2024.04.19  김동일      E20240214-000850 : 라미 공정 실적 조회 시 투입 음/양 구분 조회 요청 건
  2024.05.03  조범모      E20240428-001991 : TWS 로딩량 추적관리 업무개선 요청 건
  2024.05.07  백상우      [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Main 믹서/Sub 믹서 생산실적 조회 화면 표준화 요청건
  2024.06.12  양영재      [E20240508-000904] : 검색 조건에 LOT TYPE 추가 및 LOT 탭에서만 검색 조건 보이도록 수정
  2024.06.13  백상우      [E20240607-001447] : Mixer 원재료 Lot 투입 이력 개선 요청건 -> 절연액Mixing과 DAM Mixing도 투입자재Tab 활성화
  2024.07.22  조범모      [E20240715-000063] 슬리팅 후 해당 LOT별 TWS Data 확인
  2024.08.30  유명환      [E20240619-000917] Loding lebel 추가
  2024.09.04  유명환      [E20240619-000917] 원통형 조립 실제투입수량 및 설비호기 추가 표시 요청
  2024.09.04  조성근    : [E20240827-000372] - 롤프레스 롤맵 실적 수정팝업 추가
  2025.03.10  이민형    : [HD_OSS_0060] 소문자 인식 가능하도록 변경
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_045 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string _StackingYN = string.Empty;
        string _AREATYPE = "";
        string _AREAID = "";
        string _PROCID = "";
        string _EQSGID = "";
        string _EQPTID = "";
        string _LOTID = "";
        string _WIPSEQ = "";
        string _LANEPTNQTY = "";
        string _ORGQTY = "";
        string _UNITCODE = "";
        string _WIP_NOTE = "";
        string _LANE = "";
        string _EQPTNAME = "";

        //투입 언마운트 타입 코드.
        string _INPUT_LOT_UNMOUNT_TYPE_CODE = "";

        //FOLDING & STACKING 구분자
        string _EQGRID = string.Empty;

        bool _bLoad = true;

        //동별 CT검사 CHECK 항목 구분
        private string _workCalbuttonAuthYN = string.Empty; //월력관리/버튼권한 사용여부
        private string _unidentifiedLossYN = string.Empty;  //미확인LOSS 사용여부
        private string _reworkBoxTypeYN = string.Empty;     //재작업 BOX 생성시 작업유형 REWORK 확인
        private string _pkgInputCheckYN = string.Empty;     //PKG 투입시 재작업 완성 LOT CHECK 사용여부
        private string _qmsDefectCodeYN = string.Empty;     //QMS 불량코드 사용여부
        private string _IncomeShortConnectLossApplyFlag = string.Empty;     // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot


        private readonly Util _util = new Util();

        List<string> _MColumns1;
        List<string> _MColumns2;

        private BizDataSet _Biz = new BizDataSet();

        //[E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 
        bool isTWS_LOADING_TRACKING = false;

        private bool _InputPolarityFlag = false;

        public COM001_045()
        {
            InitializeComponent();

            InitCombo();
            InitColumnsList();          // 단위 환산 체크시 칼럼 Visible 

            _IncomeShortConnectLossApplyFlag = GetIncomeShortConnectLossApplyFlag(); // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
              
            GetAreaType(cboProcess.SelectedValue.ToString());
            AreaCheck(cboProcess.SelectedValue.ToString());
            SetProcessNumFormat(cboProcess.SelectedValue.ToString());

            //20210906 오화백 : FastTrack 적용여부 체크
            if (ChkFastTrackOWNER())
            {
                dgLotList.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;

            }             

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            //ESMI-A4동 6Line 제외처리
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);
            if (IsCmiExceptLine())
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent, sCase: "ESMI_A4_EXCEPT_LINEID");
            }
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);
            }

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboEquipment };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, null, cboProcessParent);

            if (cboProcess.Items.Count < 1)
                SetProcess();

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //경로흐름
            string[] sFilter = { "FLOWTYPE" };
            _combo.SetCombo(cboFlowType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // Top/Back
            String[] sFilter3 = { "COAT_SIDE_TYPE" };
            _combo.SetCombo(cboTopBack, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

            // 극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            // 시장유형
            string[] sFilterMKType = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMKTtype, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterMKType);

            // 2021.08.23 : 생산구분 추가
            // 생산구분
            string[] sFilterProdDiv = { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilterProdDiv);

            SetLotType();

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;

            //// 모델 AutoComplete
            //GetModel();

        }

        #endregion

        private void InitColumnsList()
        {
            _MColumns1 = new List<string>();
            _MColumns1.Add("EQPT_END_QTY");
            _MColumns1.Add("INPUT_QTY");
            _MColumns1.Add("WIPQTY_ED");
            _MColumns1.Add("CNFM_DFCT_QTY");
            _MColumns1.Add("CNFM_LOSS_QTY");
            _MColumns1.Add("CNFM_PRDT_REQ_QTY");
            _MColumns1.Add("LENGTH_EXCEED");
            _MColumns1.Add("WIPQTY2_ED");
            _MColumns1.Add("CNFM_DFCT_QTY2");
            _MColumns1.Add("CNFM_LOSS_QTY2");
            _MColumns1.Add("CNFM_PRDT_REQ_QTY2");
            _MColumns1.Add("LENGTH_EXCEED2");

            _MColumns2 = new List<string>();
            _MColumns2.Add("EQPT_END_QTY_EA");
            _MColumns2.Add("INPUT_QTY_EA");
            _MColumns2.Add("WIPQTY_ED_EA");
            _MColumns2.Add("CNFM_DFCT_QTY_EA");
            _MColumns2.Add("CNFM_LOSS_QTY_EA");
            _MColumns2.Add("CNFM_PRDT_REQ_QTY_EA");
            _MColumns2.Add("LENGTH_EXCEED_EA");
            _MColumns2.Add("WIPQTY2_ED_EA");
            _MColumns2.Add("CNFM_DFCT_QTY2_EA");
            _MColumns2.Add("CNFM_LOSS_QTY2_EA");
            _MColumns2.Add("CNFM_PRDT_REQ_QTY2_EA");
            _MColumns2.Add("LENGTH_EXCEED2_EA");
        }

        /// <summary>
        /// 20210906 오화백 FastTrack 적용 공장 체크
        /// </summary>
        private bool ChkFastTrackOWNER()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "FAST_TRACK_OWNER";
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

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            //2019.09.25 김대근 : CNB전극이고 CT/RP/ST에서만 CarrierID를 보여주도록 했음
            //2022.06.13 김지은 : 동정보에 물류 동 그룹 Code 가 적용된 AREA만 CARRIER ID 보여주도록 수정함
            SetCarrierVisible();

            // 전극 등급 표시여부
            EltrGrdCodeColumnVisible();

            // 동별 CT검사 CHECK 항목 구분
            GetAreaByCheckData();

            // SHOPID[F030],Area[M9],ProcessParent[A2000]인 경우 전극 등급 정보 탭 보여주기
            VoltGradeInfoVisible();

            InputPolarityChkBoxVisible();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 2023.10.25 강성묵 E20231005-000636 : 품질 정보 AVG_VAL 측정 데이터가 존재하는 항목만 계산
            if (cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING) == true)
            {
                dgColAvgValue.Visibility = Visibility.Visible;
            }
            else
            {
                dgColAvgValue.Visibility = Visibility.Collapsed;
            }

            ClearValue();
            GetLotList();
        }
        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                //cboProcess.SelectedItemChanged -= CboProcess_SelectedItemChanged;
                SetProcess();

                IsElectrodeGradeInfo();
                //cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
                //SetEquipment();
            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                SetEquipment();

                Util.gridClear(dgLotList);
                Util.gridClear(dgModelList);
                ClearValue();

                GetAreaType(cboProcess.SelectedValue.ToString());
                AreaCheck(cboProcess.SelectedValue.ToString());
                SetProcessNumFormat(cboProcess.SelectedValue.ToString());

                //2019.09.25 김대근 : CNB전극이고 CT/RP/ST에서만 CarrierID를 보여주도록 했음
                //2022.06.13 김지은 : 동정보에 물류 동 그룹 Code 가 적용된 AREA만 CARRIER ID 보여주도록 수정함
                SetCarrierVisible();

                // SHOPID[F030],Area[M9],ProcessParent[A2000]인 경우 전극 등급 정보 탭 보여주기
                VoltGradeInfoVisible();

                InputPolarityChkBoxVisible();
            }
        }
        #endregion

        #region [동] - 조회 조건
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 전극 등급 표시여부
            EltrGrdCodeColumnVisible();
            // SHOPID[F030],Area[M9],ProcessParent[A2000]인 경우 전극 등급 정보 탭 보여주기
            VoltGradeInfoVisible();

            InputPolarityChkBoxVisible();            
        }
        #endregion

        #region [작업일] - 조회 조건
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                // 조회 버튼 클릭시로 변경
                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                //{
                //    // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "31");

                //    dtpDateFrom.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged -= dtpDate_SelectedDataTimeChanged;

                //    //dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
                //    if (LGCdp.Name.Equals("dtpDateTo"))
                //        dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+30);
                //    else
                //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-30);

                //    dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //    dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }

                //// To 일자 변경시 From일자 1일자로 변경
                //if (LGCdp.Name.Equals("dtpDateTo"))
                //{
                //    dtpDateFrom.SelectedDateTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, 1);
                //}

            }
        }
        #endregion

        #region [LOT] - 조회 조건
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [모델] - 조회 조건
        private void txtModlId_GotFocus(object sender, RoutedEventArgs e)
        {
            _bLoad = false; // [E20231031-000604] 생산실적조뢰(생산Lot기준) 모델 조회조건 TEXTBOX 기능 삭제 처리

            // 모델 AutoComplete
            if (_bLoad)
            {
                GetModel();
                _bLoad = false;
            }
        }
        #endregion

        #region [프로젝트] - 조회 조건
        private void txtPrjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [M수량환산] - 조회 조건
        private void chkPtnLen_Checked(object sender, RoutedEventArgs e)
        {
            // Visibility.Collapsed
            foreach (string str in _MColumns1)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Collapsed;
            }

            // Visibility.Visible
            foreach (string str in _MColumns2)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Visible;
            }

            // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
            if (cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING) || cboProcess.SelectedValue.Equals(Process.SLITTING))
            {
                if (string.Equals(_IncomeShortConnectLossApplyFlag, "Y"))
                {
                    dgLotList.Columns["CONNECTION_LOSS_ROLL"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INCOME_SHORTAGE_ROLL"].Visibility = Visibility.Collapsed;

                    dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"].Visibility = Visibility.Visible;
                    dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"].Visibility = Visibility.Visible;
                }
            }
        }

        private void chkPtnLen_Unchecked(object sender, RoutedEventArgs e)
        {
            // Visibility.Visible
            foreach (string str in _MColumns1)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Visible;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Visible;
            }

            // Visibility.Collapsed
            foreach (string str in _MColumns2)
            {
                if (dgLotList.Columns.Contains(str))
                    dgLotList.Columns[str].Visibility = Visibility.Collapsed;

                if (dgModelList.Columns.Contains(str))
                    dgModelList.Columns[str].Visibility = Visibility.Collapsed;
            }

            // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
            if (cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING) || cboProcess.SelectedValue.Equals(Process.SLITTING))
            {
                if (string.Equals(_IncomeShortConnectLossApplyFlag, "Y"))
                {
                    dgLotList.Columns["CONNECTION_LOSS_ROLL"].Visibility = Visibility.Visible;
                    dgLotList.Columns["INCOME_SHORTAGE_ROLL"].Visibility = Visibility.Visible;

                    dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"].Visibility = Visibility.Collapsed;
                }
            }

        }
        #endregion

        #region [모델합산] - 조회 조건
        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 1;
        }

        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 0;
        }
        #endregion

        #region [작업대상 목록 에서 선택]
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            //if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").ToString().Equals("0")) // 2024.10.11. 김영국 - CHK값이 DB에서 넘어올때 Long Type으로 넘어오는 문제가 있어 String 비교함.
            {
                //체크시 처리될 로직
                string sLotId = DataTableConverter.GetValue(rb.DataContext, "LOTID").ToString();
                string sDate = DataTableConverter.GetValue(rb.DataContext, "STARTDTTM").ToString();
                int iWipSeq = Convert.ToInt16(DataTableConverter.GetValue(rb.DataContext, "WIPSEQ"));
                string sEqptID = DataTableConverter.GetValue(rb.DataContext, "EQPTID").ToString();

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                ClearValue();
                SetValue(rb.DataContext);
                SetProcessNumFormat(_PROCID);
                GetDefectInfo();

                if (_AREATYPE.Equals("A") && !_PROCID.Equals(Process.NOTCHING))
                {
                    GetSubLot();
                }


                GetInputHistory();
                GetQuality();
                GetColor();
                GetInputMaterial();
                GetEqpFaultyData();
                GetSlurry();
                ////GetRemark();
                GetTWS_Load();

                if (cboProcess.SelectedValue.Equals(Process.REWINDER) || cboProcess.SelectedValue.Equals(Process.LASER_ABLATION) || cboProcess.SelectedValue.Equals(Process.BACK_WINDER))
                    GetDefectTagList(sLotId, iWipSeq);

                // Slitter Vision Image 추가( 2017-01-05 )
                //if (cboProcess.SelectedValue.Equals(Process.SLITTING))
                //    GetVisionImage(sLotId, sDate);

                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    GetHalfProductList();

                    if (TabReInput.Visibility == Visibility.Visible)
                    {
                        GetDefectReInputList();
                    }
                }

                if (_PROCID.Equals(Process.PACKAGING))
                {
                    GetTrayLotByTime();
                }


                ProcCheck(_PROCID, sEqptID);

                #region # RollMap 설비 
                /* if (IsEquipmentAttr(sEqptID))
                    btnRollMap.Visibility = Visibility.Visible;
                else
                    btnRollMap.Visibility = Visibility.Collapsed; */

                if (IsRollMapLotAttribute(sLotId))
                    btnRollMap.Visibility = Visibility.Visible;
                else
                    btnRollMap.Visibility = Visibility.Collapsed;

                if (btnRollMap.Visibility == Visibility.Visible && IsRollMapResultApply() && IsRollMapLot())
                    btnRollMapUpdate.Visibility = Visibility.Visible;
                else
                    btnRollMapUpdate.Visibility = Visibility.Collapsed;

                // 전극 등급 정보 
                if (tiElectrodeGradeInfo.Visibility.Equals(Visibility.Visible)) SelectElectrodeGradeInfo(sLotId, iWipSeq.GetString());

                // SHOPID기준 F030 의  와인더(A2000) 공정인 경우 전극 등급 정보텝 추가
                if (cTabVoltGradeInfo.Visibility.Equals(Visibility.Visible))
                {
                    SelectVoltGradeInfo(sLotId, sEqptID);
                }
                #endregion
            }
        }
        #endregion

        public KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] FilterCondition(C1DataGrid grid)
        {
            KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filters = new KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[grid.FilteredColumns.Length];

            for (int i = 0; i < grid.FilteredColumns.Length; i++)
            {
                List<DataGridFilterInfo> filterInfoList = new List<DataGridFilterInfo>();
                DataGridFilterState filterState = new DataGridFilterState();

                filterInfoList.Add(new DataGridFilterInfo() {
                    FilterOperation = grid.FilteredColumns[i].FilterState.FilterInfo[0].FilterOperation,
                    FilterType = grid.FilteredColumns[i].FilterState.FilterInfo[0].FilterType,
                    Value = grid.FilteredColumns[i].FilterState.FilterInfo[0].Value
                });
                filterState.FilterInfo = filterInfoList;

                filters[i] = new KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>(grid.Columns[grid.FilteredColumns[i].Name], filterState);
            }

            return filters;
        }

        #region [품질정보조회]
        private void btnQualityInfo_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return;
            }

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("")) //SELECT
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

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
        private void wndQualityRslt_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG window = sender as CMM_ASSY_QUALITY_PKG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(window);
        }

        #endregion

        #region [완성LOT 탭] - dgSubLot_LoadedCellPresenter, dgSubLot_MouseDoubleClick(셀정보팝업), print_Button_Click(재발행)
        private void dgSubLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (cboProcess.SelectedValue.Equals(Process.PACKAGING) && e.Cell.Column.Name.Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));

        }
        private void dgSubLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgSubLot.CurrentRow != null && dgSubLot.CurrentColumn.Name.Equals("CSTID"))
                {
                    COM001_045_CELL wndPopup = new COM001_045_CELL();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[4];
                        Parameters[0] = _LOTID;
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSubLot.CurrentRow.DataItem, "LOTID"));
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSubLot.CurrentRow.DataItem, "CSTID")); ;

                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        grdMain.Children.Add(wndPopup);
                        wndPopup.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            String sBoxID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

            if (!sBoxID.Equals(""))
            {
                // 발행..
                DataTable dtRslt = _PROCID.Equals(Process.CT_INSP) ? GetThermalPaperPrintingInfo_WO(sBoxID) : GetThermalPaperPrintingInfo(sBoxID);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                    return;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                if (_PROCID.Equals(Process.LAMINATION))
                {
                    dicParam.Add("reportName", "Lami");
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
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.
                    dicList.Add(dicParam);

                    CMM_THERMAL_PRINT_LAMI printlami = new CMM_THERMAL_PRINT_LAMI();
                    printlami.FrameOperation = FrameOperation;

                    if (printlami != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.LAMINATION;
                        Parameters[2] = _EQSGID;
                        Parameters[3] = _EQPTID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.
                        Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                        C1WindowExtension.SetParameters(printlami, Parameters);
                        printlami.Show();
                    }

                }
                else if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    int iCopys = 2;

                    if (LoginInfo.CFG_THERMAL_COPIES > 0)
                    {
                        iCopys = LoginInfo.CFG_THERMAL_COPIES;
                    }

                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");
                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수
                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.
                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD printfold = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    printfold.FrameOperation = FrameOperation;

                    if (printfold != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.STACKING_FOLDING;
                        Parameters[2] = _EQSGID;
                        Parameters[3] = _EQPTID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(printfold, Parameters);
                        printfold.ShowModal();
                    }

                }
                ///[C20190415_74474] x-ray 재발행 버튼 추가
                else if (_PROCID.Equals(Process.XRAY_REWORK))
                {
                    //발행하시겠습니까?
                    Util.MessageConfirm("SFU2873", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            xRayPalletPrint(Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        }
                    });
                }
                else if (_PROCID.Equals(Process.CT_INSP))
                {
                    // 폴딩 양식과 동일 요청.
                    int iCopys = 2;

                    if (LoginInfo.CFG_THERMAL_COPIES > 0)
                    {
                        iCopys = LoginInfo.CFG_THERMAL_COPIES;
                    }

                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");
                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수
                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.

                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD printfold = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    printfold.FrameOperation = FrameOperation;

                    if (printfold != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.CT_INSP;
                        Parameters[2] = _EQSGID;
                        Parameters[3] = _EQPTID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "N";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(printfold, Parameters);
                        printfold.ShowModal();
                    }
                }
            }
        }
        #endregion

        #region [탭 선택 변경]
        private void tbcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("Lot"))
            {
                chkModel.IsChecked = false;
                cboLotType.Visibility = Visibility.Visible;
                cboLotTypeTitle.Visibility = Visibility.Visible;
            }
            else
            {
                chkModel.IsChecked = true;
                cboLotType.Visibility = Visibility.Collapsed;
                cboLotTypeTitle.Visibility = Visibility.Collapsed;
            }
                

        }
        #endregion

        #region [Rollmap]
        private void btnRollMap_Click(object sender, RoutedEventArgs e)
        {

            string lotUnitCode = string.Empty;
            // LOT 단위 ID -> EA : 원통형 롤맵 UI 팝업 호출 , M 기존 롤맵 UI 팝업 호출
            lotUnitCode = GetLotUnitCode();

            // Roll Map 호출 
            string mainFormPath = "LGC.GMES.MES.COM001";
            string mainFormName = string.Empty;

            if (string.Equals(_PROCID, Process.COATING))
            {
                //mainFormName = "COM001_ROLLMAP_COATER";
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_COATER" : "COM001_RM_CHART_CT";
            }
            else if (string.Equals(_PROCID, Process.ROLL_PRESSING))
            {
                //mainFormName = "COM001_ROLLMAP_ROLLPRESS_NEW";
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_ROLLPRESS" : "COM001_RM_CHART_RP";
            }
            else if (string.Equals(_PROCID, Process.SLITTING))
            {
                //mainFormName = "COM001_ROLLMAP_SLITTING";
                mainFormName = lotUnitCode == "EA" ? "COM001_ROLLMAP_MOBILE_SLITTING" : "COM001_RM_CHART_SL";
            }
            else if (string.Equals(_PROCID, Process.TWO_SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_TWOSLITTING";
            }
            else if (string.Equals(_PROCID, Process.HALF_SLITTING))
            {
                mainFormName = "COM001_ROLLMAP_MOBILE_HALFSLITTING";
            }
            else if (string.Equals(_PROCID, Process.SLIT_REWINDING) || string.Equals(_PROCID, Process.REWINDING))
            {
                mainFormName = "COM001_RM_CHART_RW";
            }
            else
            {
                return;
            }

            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom("ClientBin\\" + mainFormPath + ".dll");
            Type targetType = asm.GetType(mainFormPath + "." + mainFormName);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workrollmap = obj as IWorkArea;
            workrollmap.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _PROCID;
            parameters[1] = _EQSGID;
            parameters[2] = _EQPTID;
            parameters[3] = _LOTID;
            parameters[4] = _WIPSEQ;
            parameters[5] = _LANE;
            parameters[6] = _EQPTNAME + " [" + _EQPTID + "]";
            parameters[7] = txtProdVerCode.Text;

            C1Window popup = obj as C1Window;
            C1WindowExtension.SetParameters(popup, parameters);
            if (popup != null)
            {
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void btnRollMapUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] Parameters = new object[11];
                Parameters[0] = _PROCID;
                Parameters[1] = _EQSGID;
                Parameters[2] = _EQPTID;
                Parameters[3] = _LOTID;
                Parameters[4] = _WIPSEQ;
                Parameters[5] = _LANE;
                Parameters[6] = _EQPTNAME + " [" + _EQPTID + "]";
                Parameters[7] = txtProdVerCode.Text;
                Parameters[8] = "Y"; //Test Cut Visible false
                Parameters[9] = "Y"; //Search Mode True
                Parameters[10] = "COM001_045"; //호출화면

                // RollMap 실적 수정 Popup Call
                if (_PROCID == Process.COATING)
                {
                    CMM_RM_CT_RESULT popupRollMapUpdate = new CMM_RM_CT_RESULT { FrameOperation = FrameOperation };

                    if (popupRollMapUpdate != null)
                    {


                        C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                        if (popupRollMapUpdate != null)
                        {
                            popupRollMapUpdate.ShowModal();
                            popupRollMapUpdate.CenterOnScreen();
                        }
                    }
                }
                else if (_PROCID == Process.ROLL_PRESSING)
                {
                    CMM_RM_RP_RESULT popupRollMapUpdate = new CMM_RM_RP_RESULT { FrameOperation = FrameOperation };

                    if (popupRollMapUpdate != null)
                    {
                        C1WindowExtension.SetParameters(popupRollMapUpdate, Parameters);

                        if (popupRollMapUpdate != null)
                        {
                            popupRollMapUpdate.ShowModal();
                            popupRollMapUpdate.CenterOnScreen();
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

        private void dgElectrodeGradeInfo_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        gradejudgmentCode = x.Field<string>("GRD_JUDG_CLSS_CODE")
                    }).Select(g => new
                    {
                        GradejudgmentCode = g.Key.gradejudgmentCode,
                        Count = g.Count()
                    }).ToList();

                    string previewGradejudgmentCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {

                        foreach (var item in query)
                        {
                            int rowIndex = i;
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "GRD_JUDG_CLSS_CODE").GetString() == item.GradejudgmentCode && previewGradejudgmentCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "GRD_JUDG_CLSS_CODE").GetString())
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["GRD_AVG_VALUE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["GRD_AVG_VALUE"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["GRD_JUDG_CODE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["GRD_JUDG_CODE"].Index)));
                            }
                        }

                        previewGradejudgmentCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "GRD_JUDG_CLSS_CODE").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        #region [### 공정의 AreaType ###]
        public void GetAreaType(string sProcID)
        {
            try
            {
                ShowLoadingIndicator();

                _AREATYPE = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PCSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = sProcID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENTPROCESS", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                    _AREATYPE = dtRslt.Rows[0]["PCSGID"].ToString();
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
        #endregion

        #region [### 실적 일자로 조회 ###]
        public void GetCaldate()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
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
        #endregion

        #region [동별 전극 등급 Visible]  
        private void EltrGrdCodeColumnVisible()
        {
            try
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString() == "SELECT")
                {
                    dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgLotList.Columns.Contains("ELTR_GRD_CODE"))
                        dgLotList.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [동별, 공정별 Carrier ID Visible]
        /// <summary>
        /// 동 속성 : 물류 동 그룹 Code가 존재하며, 공정이 Coating, Roll Press, Slitting 일 때만 CSTID 컬럼 Visible
        /// </summary>
        private void SetCarrierVisible()
        {
            try
            {
                dgLotList.Columns["PR_CSTID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["CSTID"].Visibility = Visibility.Collapsed;

                if (cboArea.SelectedValue != null && !cboArea.SelectedValue.ToString().Equals("SELECT") && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.ToString().Equals("SELECT"))
                {
                    DataTable inTable = new DataTable();
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    inTable.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_LOGIS_GROUP_CBO", "RQSTDT", "RSLTDT", inTable);

                    if (dtRslt != null && dtRslt.Rows.Count > 0)
                    {
                        if (cboProcess.SelectedValue.Equals(Process.COATING))
                        {
                            dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                        }
                        else if (cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING)
                                || cboProcess.SelectedValue.Equals(Process.SLITTING))
                        {
                            dgLotList.Columns["PR_CSTID"].Visibility = Visibility.Visible;
                            dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                        }
                    }

                    if (LoginInfo.CFG_AREA_ID == "A3" && cboProcess.SelectedValue.Equals(Process.NOTCHING))
                        dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;

                    if (cboProcess.SelectedValue.Equals(Process.WINDING) && SetAssyCSTIDView())
                    {
                        dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [### 작업대상 조회 ###]
        public void GetLotList()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (string.Equals(GetAreaType(), "E"))
                {
                    WIP_BCD_PRT_COUNT.Visibility = Visibility.Visible;
                    WIP_BCD_PRT_EXEC_COUNT.Visibility = Visibility.Visible;
                }

                //2022-12-29 오화백  동 :EP 추가 
                if ((cboArea.SelectedValue.Equals("E5") || cboArea.SelectedValue.Equals("EP")) && cboProcess.SelectedValue.Equals(Process.COATING))
                {
                    dgLotList.Columns["EQPT_DFCT_TOTAL_QTY"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["EQPT_DFCT_TOTAL_QTY"].Visibility = Visibility.Collapsed;
                }

                ShowLoadingIndicator();
                DoEvents();

                bool bLot = false;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AREATYPE", typeof(string));
                dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("TOPBACK", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));
                dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.08.23 : 생산구분 추가
                dtRqst.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                if (dr["AREAID"].Equals("")) return;

                ////dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "라인을선택하세요.");
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));
                //if (dr["EQSGID"].Equals("")) return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;

                //dr["PROCID"] = Util.GetCondition(cboProcess, "공정을선택하세요.");
                dr["PROCID"] = Util.GetCondition(cboProcess, MessageDic.Instance.GetMessage("SFU1459"));
                if (dr["PROCID"].Equals("")) return;

                //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                string sEqptID = Util.GetCondition(cboEquipment);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(sEqptID) ? null : sEqptID;

                dr["FLOWTYPE"] = Util.GetCondition(cboFlowType, bAllNull: true);

                if (cboTopBack.Visibility.Equals(Visibility.Visible))
                    dr["TOPBACK"] = Util.GetCondition(cboTopBack, bAllNull: true);
                if (cboElecType.Visibility.Equals(Visibility.Visible))
                    dr["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType, bAllNull: true);

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["AREATYPE"] = _AREATYPE;

                string sLotType = Util.GetCondition(cboLotType);
                dr["LOTTYPE"] = string.IsNullOrWhiteSpace(sLotType) ? null : sLotType;

                if (cboProcess.SelectedValue.Equals(Process.SLIT_REWINDING) || cboProcess.SelectedValue.Equals(Process.SLITTING) || cboProcess.SelectedValue.Equals(Process.HEAT_TREATMENT) || cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING))
                {
                    FRST_MKT.Visibility = Visibility.Visible;
                }
                else
                {
                    FRST_MKT.Visibility = Visibility.Collapsed;
                }

                // 기존 컬럼에 소진량, 잔량, LOSS 등이 있으나 WINDING 에서 구하는 방식과 맞지 않아 WINDING 용으로 신규 컬럼 추가함
                if (cboProcess.SelectedValue.Equals(Process.WINDING))
                {
                    dgInputHist.Columns["INPUT_QTY"].Header = new object[] { ObjectDic.Instance.GetObjectName("투입량"), ObjectDic.Instance.GetObjectName("투입량") };
                    dgInputHist.Columns["USED_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["REMAIN_QTY"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["LOSS_QTY"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgInputHist.Columns["INPUT_QTY"].Header = new object[] { ObjectDic.Instance.GetObjectName("소진량"), ObjectDic.Instance.GetObjectName("소진량") };
                    dgInputHist.Columns["USED_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["REMAIN_QTY"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["LOSS_QTY"].Visibility = Visibility.Collapsed;
                }

                if (!string.IsNullOrWhiteSpace(txtPRLOTID.Text))
                {
                    dr["PR_LOTID"] = Util.GetCondition(txtPRLOTID);
                }
                else if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                {
                    txtLotId.Text = txtLotId.Text.ToUpper();
                    dr["LOTID"] = Util.GetCondition(txtLotId);
                    bLot = true;
                }

                if (!string.IsNullOrWhiteSpace(txtModlId.Text))
                    dr["MODLID"] = txtModlId.Text;

                if (!string.IsNullOrWhiteSpace(txtProdId.Text))
                    dr["PRODID"] = txtProdId.Text;

                if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                if (chkProc.IsChecked == false)
                    dr["RUNYN"] = "Y";

                dr["MKT_TYPE_CODE"] = Util.GetCondition(cboMKTtype, bAllNull: true);
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);    // 2021.08.23 : 생산구분 추가

                dtRqst.Rows.Add(dr);
                
                string sBizName = string.Empty;

                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("Lot"))
                {
                    if (cboProcess.SelectedValue.ToString().Equals(Process.ASSEMBLY) || cboProcess.SelectedValue.ToString().Equals(Process.WASHING))
                        sBizName = "DA_PRD_SEL_LOT_LIST_MOBILE";
                    else
                        sBizName = "DA_PRD_SEL_LOT_LIST";

                    // [E20240214-000850] 라미 공정 투입 음/양 구분 조회 요청 건                    
                    _InputPolarityFlag = false;
                    if (chkPolarity.Visibility == Visibility.Visible && chkPolarity.IsChecked == true && Util.GetCondition(cboProcess).Equals(Process.LAMINATION))
                    {
                        sBizName = "DA_PRD_SEL_LOT_LIST_PLRT";
                        _InputPolarityFlag = true;
                    }
                }
                else
                {
                    if (cboProcess.SelectedValue.ToString().Equals(Process.ASSEMBLY) || cboProcess.SelectedValue.ToString().Equals(Process.WASHING))
                        sBizName = "DA_PRD_SEL_LOT_LIST_MODEL_MOBILE";
                    else
                        sBizName = "DA_PRD_SEL_LOT_LIST_MODEL";
                }

                //DataSet ds = new DataSet();
                //ds.Tables.Add(dtRqst);
                //string xml = ds.GetXml();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("Lot"))
                {
                    SetGridPolarity();

                    dgLotList.MergingCells -= dgLotList_MergingCells;

                    Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);
                    
                    dgLotList.MergingCells += dgLotList_MergingCells;                    
                }
                else
                    Util.GridSetData(dgModelList, dtRslt, FrameOperation, true);

                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("Lot") &&
                    dtRslt != null && dtRslt.Rows.Count > 0 && bLot == true)
                {
                    _AREATYPE = dtRslt.Rows[0]["AREATYPE"].ToString();
                    AreaCheck(dtRslt.Rows[0]["PROCID"].ToString());
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

        #endregion

        #region [### 불량/Loss/물품청구 조회 ###]
        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                /* AZS 설비 불량/Loss항목 변경을 위한 준비 시작 */
                bool bAZS = false;
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersDefect = FilterCondition(dgDefect);  //2023.05.09 김대현

                DataTable inComTable = new DataTable();
                inComTable.Columns.Add("CMCODE", typeof(string));
                inComTable.Columns.Add("CMCDTYPE", typeof(string));
                inComTable.Columns.Add("LANGID", typeof(string));

                DataRow newComRow = inComTable.NewRow();
                newComRow["CMCODE"] = _EQPTID;
                newComRow["CMCDTYPE"] = "EQPT_EXCEPT_WORK_CALENDAR";
                newComRow["LANGID"] = LoginInfo.LANGID;
                inComTable.Rows.Add(newComRow);

                DataTable dtCom = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", inComTable);

                if (dtCom.Rows.Count > 0)
                    bAZS = true;
                /* AZS 설비 불량/Loss항목 변경을 위한 준비 끝 */

                string BizNAme = string.Empty;
                if (bAZS)
                    BizNAme = "DA_QCA_SEL_AZSREASON_L";
                else if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                    BizNAme = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";
                else
                    //C20210222-000365 불량/Loss항목 표준화 적용 DA_QCA_SEL_WIPRESONCOLLECT_INFO -> BR_PRD_SEL_WIPRESONCOLLECT_INFO 변경
                    BizNAme = "BR_PRD_SEL_WIPRESONCOLLECT_INFO";

                // 동별 CT검사 CHECK 항목 구분
                if (_PROCID.Equals(Process.CT_INSP) && _qmsDefectCodeYN == "N")
                    BizNAme = "BR_PRD_SEL_WIPRESONCOLLECT_INFO";

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = _AREAID;
                newRow["PROCID"] = _PROCID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    if (dgDefect.Columns.Contains("RESNNAME"))
                        dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                    if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                        dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                    if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                        dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;

                    DataSet ds = new DataSet();
                    ds.Tables.Add(inTable);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(BizNAme, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                    if (CommonVerify.HasTableInDataSet(dsResult))
                    {
                        //'AP' 동별 / 공정별
                        //'LP' 라인 / 공정별
                        dgDefect.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                        dgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                    }


                }
                else if (_PROCID.Equals(Process.CT_INSP) && _qmsDefectCodeYN == "N")
                {
                    dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizNAme, "INDATA", "OUTDATA", inTable);
                    Util.GridSetData(dgDefect, dtResult, null);

                    if ((dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("DFCT_SYS_TYPE") && Util.NVC(dtResult.Rows[0]["DFCT_SYS_TYPE"]).Equals("Q")))
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    dgDefect.Columns["CLSS_NAME1"].Visibility = Visibility.Collapsed;

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(BizNAme, "INDATA", "OUTDATA", inTable);
                    Util.GridSetData(dgDefect, dtResult, null);

                    if (bAZS || (dtResult?.Rows?.Count > 0 && dtResult.Columns.Contains("DFCT_SYS_TYPE") && Util.NVC(dtResult.Rows[0]["DFCT_SYS_TYPE"]).Equals("Q")))
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (dgDefect.Columns.Contains("RESNNAME"))
                            dgDefect.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                        if (dgDefect.Columns.Contains("DFCT_CODE_DETL_NAME"))
                            dgDefect.Columns["DFCT_CODE_DETL_NAME"].Visibility = Visibility.Visible;
                        if (dgDefect.Columns.Contains("DFCT_PART_NAME"))
                            dgDefect.Columns["DFCT_PART_NAME"].Visibility = Visibility.Visible;
                    }
                }


                // Folding의 경우 'FOLDED CELL' 으로 표기 STACKING 일 경우 'STAKCING CELL' 으로 표기
                // 그외 다른 공정일경우 '수량'으로 표기
                if (_PROCID.Equals(Process.STACKING_FOLDING))
                {
                    chkFoldingStacking();

                    if (_EQGRID == "STK")
                    {
                        dgDefect.Columns["RESNQTY"].Header = "STACKING CELL";
                    }
                    else
                    {
                        dgDefect.Columns["RESNQTY"].Header = "FOLDED CELL";
                    }
                }
                else
                {
                    dgDefect.Columns["RESNQTY"].Header = ObjectDic.Instance.GetObjectName("수량");
                }

                if (_StackingYN.Equals("Y"))
                {

                }

                dgDefect.FilterBy(filtersDefect);   //2023.05.09 김대현
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
        #endregion

        #region [### 설비불량정보 조회 ###]
        private void GetEqpFaultyData() //string sLot, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersEqpFaulty = FilterCondition(dgEqpFaulty);    //2023.05.09 김대현

                string sBizRule = string.Empty;
                if (_PROCID.Equals(Process.WINDING) || _PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WASHING))
                {
                    sBizRule = "DA_EQP_SEL_EQPTDFCT_INFO_HIST";
                }
                else
                {
                    sBizRule = "DA_EQP_SEL_EQPTDFCT_INFO";
                }

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EQPTID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizRule, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgEqpFaulty.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, true);

                        dgEqpFaulty.FilterBy(filtersEqpFaulty); //2023.05.09 김대현
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
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 품질정보 조회 ###]
        private void GetQuality()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersQualityInfo = FilterCondition(dgQualityInfo);    //2023.05.09 김대현

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(Int16));

                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WINDING))
                {
                    dtRqst.Columns.Add("CLCT_PONT_CODE", typeof(string));
                    dtRqst.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                    dtRqst.Columns.Add("CLCTITEM_CLSS3", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("CLCT_BAS_CODE", typeof(string));
                }

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREAID"] = _AREAID;
                dr["PROCID"] = _PROCID;
                dr["LOTID"] = _LOTID;
                dr["WIPSEQ"] = _WIPSEQ;

                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WINDING))
                {
                    dr["EQPTID"] = _EQPTID;
                }
                dtRqst.Rows.Add(dr);

                string BizName = string.Empty;
                if (_PROCID.Equals(Process.ASSEMBLY) || _PROCID.Equals(Process.WINDING))
                    BizName = "DA_QCA_SEL_SELF_INSP_CLCTITEM_LOT";
                else
                    BizName = "DA_QCA_SEL_WIPDATACOLLECT_LOT_PROC_END";

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizName, "INDATA", "OUTDATA", dtRqst);

                // 2023.10.25 강성묵 E20231005-000636 : 품질 정보 AVG_VAL 측정 데이터가 존재하는 항목만 계산
                if (cboProcess.SelectedValue.Equals(Process.ROLL_PRESSING) == true)
                {
                    dgColAvgValue.Visibility = Visibility.Visible;

                    //<-------2023.10.10  연현정 E20231005-000636 : 품질 정보 AVG_VAL 추가 Start

                    double sum = 0.00;
                    // 2023.10.25 강성묵 E20231005-000636 : 품질 정보 AVG_VAL 측정 데이터가 존재하는 항목만 계산
                    int iSumItemCount = 0;
                    foreach (DataRow row in dtRslt.Rows)
                    {
                        double value = 0.0;
                        try
                        {
                            //숫자일 경우만 Sum
                            if (!string.IsNullOrEmpty(Util.NVC(row["CLCTVAL01"])) && double.TryParse(Util.NVC(row["CLCTVAL01"]), out value))
                            {
                                sum += Convert.ToDouble(row["CLCTVAL01"]);
                                // 2023.10.25 강성묵 E20231005-000636 : 품질 정보 AVG_VAL 측정 데이터가 존재하는 항목만 계산
                                iSumItemCount++;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                            //throw;
                        }
                    }

                    // 2023.10.25 강성묵 E20231005-000636 : 품질 정보 AVG_VAL 측정 데이터가 존재하는 항목만 계산
                    //double dAvgVal = sum / dtRslt.Rows.Count;   //average
                    double dAvgVal = 0;
                    if (iSumItemCount > 0)
                    {
                        dAvgVal = sum / iSumItemCount;
                    }

                    DataRow drNew = dtRslt.NewRow();
                    drNew["CLCTNAME"] = ObjectDic.Instance.GetObjectName("AVG_VALUE");
                    drNew["CLCTVAL01"] = string.Format("{0:0.00}", dAvgVal);
                    drNew["COMBOVISIBLE"] = "Collapsed";
                    dtRslt.Rows.Add(drNew);
                    //2023.10.10  연현정 E20231005-000636 : 품질 정보 AVG_VAL 추가 End ---------->
                }

                //Util.gridClear(dgQualityInfo);
                //dgQualityInfo.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgQualityInfo, dtRslt, null);


                dgQualityInfo.FilterBy(filtersQualityInfo); //2023.05.09 김대현
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
        #endregion

        #region [### 색지정보 조회 ###]
        private void GetColor()
        {
            try
            {
                if (_PROCID != Process.ROLL_PRESSING)
                    return;

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREAID"] = _AREAID;
                dr["LOTID"] = _LOTID;
                dr["EQPTID"] = _EQPTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_COLORTAG_LOT", "INDATA", "RSLTDT", dtRqst);

                Util.GridSetData(dgColor, dtRslt, FrameOperation);
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
        #endregion

        #region [### 완성LOT 조회 ###]
        private void GetSubLot()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersSubLot = FilterCondition(dgSubLot);  //2023.05.09 김대현

                string sBizName = string.Empty;

                sBizName = "DA_PRD_SEL_EDIT_SUBLOT_LIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _LOTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgLotList);
                //dgLotList.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgSubLot, dtRslt, FrameOperation);

                dgSubLot.FilterBy(filtersSubLot);   //2023.05.09 김대현
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
        #endregion

        #region [### 투입이력 조회 ###]
        private void GetInputHistory()
        {
            try
            {
                if (!cTabInputHalf.Visibility.Equals(Visibility.Visible)) return;

                ShowLoadingIndicator();
                DoEvents();
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersInputHist = FilterCondition(dgInputHist);    //2023.05.09 김대현

                // 투입 언마운트 타입 조회
                _INPUT_LOT_UNMOUNT_TYPE_CODE = GetInputUnMountType();

                string sBizName = string.Empty;

                if (_INPUT_LOT_UNMOUNT_TYPE_CODE.Equals("UNMOUNT"))             // 3Loss 이력 저장 타입.
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END_3LOSS";
                else if (_INPUT_LOT_UNMOUNT_TYPE_CODE.Equals("UNMOUNT_LOSS"))   // CNB, CWA3동 3Loss 타입.
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END_L";
                else
                    sBizName = "DA_PRD_SEL_INPUT_MTRL_HIST_END";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["PROD_WIPSEQ"] = _WIPSEQ;
                newRow["INPUT_LOTID"] = null;
                newRow["EQPT_MOUNT_PSTN_ID"] = null;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            //Util.AlertByBiz("DA_PRD_SEL_INPUT_MTRL_HIST", searchException.Message, searchException.ToString());
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(searchException.Message, searchException.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            Util.MessageException(searchException);
                            return;
                        }

                        switch (_INPUT_LOT_UNMOUNT_TYPE_CODE)
                        {
                            case "UNMOUNT":         // 3Loss 이력 저장 타입.
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Visible;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Visible;

                                break;
                            case "UNMOUNT_LOSS":    // CNB, CWA3동 3Loss 타입.
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY")
                                   && (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.PACKAGING)))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY")
                                   && (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION)))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY")
                                   && (_PROCID.Equals(Process.NOTCHING) || _PROCID.Equals(Process.LAMINATION) || _PROCID.Equals(Process.STACKING_FOLDING)))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Collapsed;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Visible;

                                break;
                            default:
                                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("RMN_QTY"))
                                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Collapsed;

                                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Collapsed;

                                break;
                        }

                        Util.GridSetData(dgInputHist, searchResult, FrameOperation, true);

                        dgInputHist.FilterBy(filtersInputHist); //2023.05.09 김대현
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
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
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 투입자재 조회 ###]
        private void GetInputMaterial()
        {
            try
            {
                if (!cTabInputMaterial.Visibility.Equals(Visibility.Visible)) return;

                ShowLoadingIndicator();
                DoEvents();

                string sBizName = string.Empty;

                if (_PROCID.Equals(Process.MIXING) || _PROCID.Equals(Process.PRE_MIXING) || _PROCID.Equals(Process.SRS_MIXING))  //MIXER전체공정 적용 확인 
                    sBizName = "DA_PRD_SEL_CONSUME_MATERIAL_SUMMARY";
                else
                    sBizName = "DA_PRD_SEL_CONSUME_MATERIAL";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                if (_PROCID.Equals(Process.MIXING) || _PROCID.Equals(Process.PRE_MIXING) || _PROCID.Equals(Process.SRS_MIXING))  //MIXER전체공정 적용 확인 
                    dtRqst.Columns.Add("WOID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = _LOTID;
                if (_PROCID.Equals(Process.MIXING) || _PROCID.Equals(Process.PRE_MIXING) || _PROCID.Equals(Process.SRS_MIXING))  //MIXER전체공정 적용 확인 
                    dr["WOID"] = txtWorkorder.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgMaterial, dtRslt, FrameOperation);

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
        #endregion

        #region [### 불량태그 조회 ###]
        private void GetDefectTagList(string sLotID, int iWipSeq)
        {
            try
            {
                if (cTabDefectTag.Visibility != Visibility.Visible)
                    return;

                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIPSEQ", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["AREAID"] = _AREAID;
                dr["LOTID"] = sLotID;
                dr["WIPSEQ"] = Util.NVC(iWipSeq);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUALITY_DEFECT_TAG_WIP", "INDATA", "RSLTDT", dtRqst);

                Util.GridSetData(dgDefectTag, dtRslt, FrameOperation);
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
        #endregion

        #region [### 슬러리 조회 ###]
        private void GetSlurry()
        {
            try
            {
                if (!cTabSlurry.Visibility.Equals(Visibility.Visible)) return;

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = _LOTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPMTRL_CT", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSlurry, dtRslt, FrameOperation);

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
        #endregion

        #region[### 특이사항 조회 ###]
        private void GetRemark()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                rtxRemark.Document.Blocks.Clear();

                DataTable inTable = _Biz.GetDA_PRD_SEL_LOT_REMARK();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            //Util.AlertByBiz("DA_PRD_SEL_LOT_NOTE", searchException.Message, searchException.ToString());
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(searchException.Message, searchException.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Rows.Count > 0 && !Util.NVC(searchResult.Rows[0]["WIP_NOTE"]).Equals(""))
                            rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        //Logger.Instance.WriteLine(Logger.OPERATION_R + "GetProductLot", Logger.MESSAGE_OPERATION_END);
                    }
                }
                );
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
        #endregion

        #region [### 불량/Loss/물품청구 - ATYPE, CTYPE 가져오기 ###]
        private void GetMBOMInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_MBOM();

                DataRow newRow = inTable.NewRow();
                newRow["WO_DETL_ID"] = txtWorkorderDetail.Text;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                inTable.Rows.Add(newRow);

                bool bAZS = false;

                new ClientProxy().ExecuteService("DA_PRD_SEL_MBOM_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                        {
                            Util.MessageValidation("SFU1941");      //타입별 불량 기준정보가 존재하지 않습니다.
                            return;
                        }
                        else
                        {
                            if (_StackingYN.Equals("Y"))
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("NC"))
                                    {
                                        //AZS 제품
                                        if (Util.NVC(searchResult.Rows[i]["PRDT_CLSS_CODE"]).Equals("AN"))
                                        {
                                            txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }
                                        else if (Util.NVC(searchResult.Rows[i]["PRDT_CLSS_CODE"]).Equals("CA"))
                                        {
                                            txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }

                                        bAZS = true;
                                    }
                                    else
                                    {
                                        if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("HC"))
                                        {
                                            txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }
                                        else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL2_CODE"]).Equals("MC"))
                                        {
                                            txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < searchResult.Rows.Count; i++)
                                {
                                    if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("AT"))
                                    {
                                        txtInAType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                    else if (Util.NVC(searchResult.Rows[i]["PRODUCT_LEVEL3_CODE"]).Equals("CT"))
                                    {
                                        txtInCType.Text = double.Parse(Util.NVC(searchResult.Rows[i]["PROC_INPUT_CNT"])).ToString();
                                    }
                                }
                            }

                            if (bAZS)
                            {
                                tbAType.Text = "AN";
                                tbCType.Text = "CA";

                                if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                                    dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "AN";
                                if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                                    dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "CA";

                                dgDefect.Columns["RESNQTY"].Header = "AZS CELL";

                                if (txtInAType.Text.Equals(""))
                                {
                                    Util.MessageValidation("AN 불량 기준정보가 존재하지 않습니다.");      //AN 불량 기준정보가 존재하지 않습니다.
                                    return;
                                }

                                if (txtInCType.Text.Equals(""))
                                {
                                    Util.MessageValidation("CA 불량 기준정보가 존재하지 않습니다.");      //CA 불량 기준정보가 존재하지 않습니다.
                                    return;
                                }
                            }
                            else
                            {
                                if (txtInAType.Text.Equals(""))
                                {
                                    if (_StackingYN.Equals("Y"))
                                        Util.MessageValidation("SFU1337");      //HALF TYPE 불량 기준정보가 존재하지 않습니다.
                                    else
                                        Util.MessageValidation("SFU1306");        //ATYPE 불량 기준정보가 존재하지 않습니다.

                                    return;
                                }

                                if (txtInCType.Text.Equals(""))
                                {
                                    if (_StackingYN.Equals("Y"))
                                        Util.MessageValidation("SFU1401");     //MONO TYPE 불량 기준정보가 존재하지 않습니다.
                                    else
                                        Util.MessageValidation("SFU1326");        //CTYPE 불량 기준정보가 존재하지 않습니다.

                                    return;
                                }
                            }

                            //gDfctAQty = double.Parse(txtInAType.Text);
                            //gDfctCQty = double.Parse(txtInCType.Text);
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
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 재발행 ###]
        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AREAID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", ex.Message, ex.ToString());
                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetThermalPaperPrintingInfo_WO(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;
                newRow["PROCID"] = _PROCID;
                newRow["PROD_LOTID"] = _LOTID.Equals("") ? null : _LOTID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_WITHOUT_WO", "INDATA", "OUTDATA", inTable);

                return dtRslt;
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

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                //newRow["LABEL_CODE"] = "LBL0001";
                //newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = "2";
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                //newRow["PRT_ITEM03"] = "";
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

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_LABEL_PRINT_HIST", ex.Message, ex.ToString());
                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }

        /// [C20190415_74474] x-ray 재발행 버튼 추가
        private void xRayPalletPrint(string sPalletId)
        {
            try
            {

                //string palletId = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                const string bizRuleName = "DA_PRD_SEL_PALLET_RUNCARD_DATA_ASSY_XR";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PALLET_ID", typeof(string));
                DataRow indata = inDataTable.NewRow();
                indata["PALLET_ID"] = sPalletId;
                inDataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        CMM_ASSY_PALLET_PRINT poopupPallet = new CMM_ASSY_PALLET_PRINT { FrameOperation = this.FrameOperation };
                        object[] parameters = new object[3];
                        parameters[0] = result;
                        parameters[1] = sPalletId;
                        parameters[2] = Process.XRAY_REWORK;
                        C1WindowExtension.SetParameters(poopupPallet, parameters);
                        poopupPallet.Closed += new EventHandler(poopupPallet_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => poopupPallet.ShowModal()));
                    }
                    else
                    {
                        //데이터가 없습니다.
                        Util.MessageValidation("SFU1498");
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupPallet_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PALLET_PRINT popup = sender as CMM_ASSY_PALLET_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //GetOutPallet();
            }
        }
        #endregion

        #region [### 투입 반제품 조회 ###]
        private void GetHalfProductList()
        {
            try
            {
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersInHalfProduct = FilterCondition(dgInHalfProduct);    //2023.05.09 김대현

                Util.gridClear(dgInHalfProduct);

                string bizRuleName = string.Equals(_PROCID, Process.ASSEMBLY) ? "DA_PRD_SEL_INPUT_HALFPROD_AS" : "DA_PRD_SEL_INPUT_HALFPROD_WS";
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
                dr["MTRLTYPE"] = "PROD";
                dr["EQPTID"] = _EQPTID;
                dr["PROD_LOTID"] = _LOTID;

                indataTable.Rows.Add(dr);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(indataTable);
                //string xml = ds.GetXml();

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
                dgInHalfProduct.ItemsSource = DataTableConverter.Convert(dt);

                dgInHalfProduct.FilterBy(filtersInHalfProduct); //2023.05.09 김대현
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[### 로딩 인라인 데이터(TWS) 조회 ###] - [E20240206-000574]
        private void GetTWS_Load()
        {
            try
            {
                if (!cTabTWS_Loading.Visibility.Equals(Visibility.Visible)) return;

                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = _LOTID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_COATING_TWS_LOAD_MEASR", "INDATA", "OUTDATA", dtRqst);

                //단위 컬럼 추가
                dtRslt.Columns.Add("UNIT", typeof(string));

                Util.GridSetData(dgTWS_Loading, dtRslt, FrameOperation);

                
                foreach (C1.WPF.DataGrid.DataGridRow _iRow in dgTWS_Loading.Rows)
                {
                    _iRow.DataItem.SetValue("UNIT", "㎎/25㎠");
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
        #endregion

        #region[### 조회 조건 모델 조회 ###]
        private void GetModel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL", "INDATA", "OUTDATA", inTable);

                foreach (DataRow r in dtRslt.Rows)
                {
                    string displayString = r["MODLID"].ToString(); //표시 텍스트
                    //string[] keywordString = new string[r["MODLID"].ToString().Length - 1]; //검색 필요 최소 글자수(Threshold)가 2이므로 두 글자씩 묶어서 배열(이진선은 총 세 글자이고 '이진'과 '진선'의 2개의 묶음으로 나눌 수 있으므로 배열의 Count는 2가 된다)로 던져야 검색 가능.
                    string keywordString;


                    //for (int i = 0; i < displayString.Length - 1; i++)
                    //keywordString[i] = displayString.Substring(i, txtModlId.Threshold); //Threshold 만큼 잘라서 배열에 담는다 (위 주석 참조)

                    keywordString = displayString;

                    //// [E20231031-000604] 생산실적조뢰(생산Lot기준) 모델 조회조건 TEXTBOX 기능 삭제 처리
                    //txtModlId.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString)); //표시 텍스트와 검색어 텍스트(배열)를 AutoCompleteTextBox의 Item에 추가한다.
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
        #endregion

        #region [Process 정보 가져오기]
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [FOLDING & STACKING 구분]
        //2017.11.21  INS염규범 GEMS FOLDING 실적 마감 단어 수정 요청 CSR20171011_02178
        private void chkFoldingStacking()
        {

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["LOTID"] = _LOTID;

            inDataTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_EQGRID_BYLOTID", "INDATA", "OUTDATA", inDataTable, (dtResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    _EQGRID = dtResult.Rows[0]["EQGRID"].ToString();

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
        #endregion

        #region [전극 조립 구분]
        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }
        #endregion

        #region [투입 Unmount 타입 조회]
        private string GetInputUnMountType()
        {
            try
            {
                string sRet = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.NVC(_EQPTID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult?.Rows?.Count > 0)
                    if (dtResult.Columns.Contains("INPUT_LOT_UNMOUNT_TYPE_CODE"))
                        sRet = Util.NVC(dtResult.Rows[0]["INPUT_LOT_UNMOUNT_TYPE_CODE"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
        #endregion

        private string GetLotUnitCode()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_VW_LOT";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = _LOTID;
                inTable.Rows.Add(dr);
                DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(result))
                {
                    return result.Rows[0]["LOTUID"].GetString();
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

        #endregion

        #region [길이부족(Roll),연결Loss(Rol) 조회결과항목 적용여부]
        // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
        private string GetIncomeShortConnectLossApplyFlag()
        {
            string sIncomeShortConnectLossApplyFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "COM_TYPE_CODE";
            sCmCode = "INCOME_SHORT_CONNECT_LOSS_APPLY_FLAG"; //  길이부족_연결LOSS_적용여부

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sIncomeShortConnectLossApplyFlag = "Y";

                }
                else
                {
                    sIncomeShortConnectLossApplyFlag = "N";
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex); 
                sIncomeShortConnectLossApplyFlag = "N";
            }

            return sIncomeShortConnectLossApplyFlag;

        }
        #endregion

        #region WINDING 공정의 대차ID 항목 표기 여부
        private bool SetAssyCSTIDView()
        {
            if (String.IsNullOrEmpty(Util.NVC(cboEquipmentSegment.SelectedValue.ToString())))
            {
                return false;
            }

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "ASSY_CSTID_VIEW";
            //dr["CBO_CODE"] = cboArea.SelectedValue.ToString();
            dr["CBO_CODE"] = cboEquipmentSegment.SelectedValue.ToString(); // E20230901-001504
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
        #endregion 

        #region [Func]
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

        #region [초기화]
        private void ClearValue()
        {
            txtSelectLot.Text = "";
            txtWorkorder.Text = "";
            txtWorkorderDetail.Text = "";

            txtWorkdate.Text = "";
            txtShift.Text = "";
            txtStartTime.Text = "";
            txtWorker.Text = "";
            txtInputQty.Value = 0;
            txtOutQty.Value = 0;
            txtDefectQty.Value = 0;
            txtLossQty.Value = 0;
            txtPrdtReqQty.Value = 0;
            txtProdVerCode.Text = "";
            txtLaneQty.Text = "";
            txtEndDttm.Text = "";
            txtNote.Text = "";
            txtWipWrkTypeCOde.Text = "";

            _AREAID = "";
            _PROCID = "";
            _EQSGID = "";
            _EQPTID = "";
            _LOTID = "";
            _WIPSEQ = "";
            _LANEPTNQTY = "";
            _ORGQTY = "";
            _WIP_NOTE = "";
            _StackingYN = "";
            _LANE = "";
            _EQPTNAME = "";

            txtInAType.Text = "";
            txtInCType.Text = "";

            Util.gridClear(dgDefect);
            Util.gridClear(dgQualityInfo);
            Util.gridClear(dgEqpFaulty);
            Util.gridClear(dgColor);
            Util.gridClear(dgSubLot);
            Util.gridClear(dgInputHist);
            Util.gridClear(dgDefectTag);
            Util.gridClear(dgTrayInfo);

            //[E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건 
            Util.gridClear(dgTWS_Loading);

            if (!cboProcess.SelectedValue.ToString().Equals(Process.STACKING_FOLDING))
            {
                grdMBomTypeCnt.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdMBomTypeCnt.Visibility = Visibility.Visible;
            }

            #region 투입 이력 컬럼 View 초기화.
            if (dgInputHist.Visibility == Visibility.Visible)
            {
                if (dgInputHist.Columns.Contains("PRE_PROC_LOSS_QTY"))
                    dgInputHist.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("FIX_LOSS_QTY"))
                    dgInputHist.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("CURR_PROC_LOSS_QTY"))
                    dgInputHist.Columns["CURR_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("RMN_QTY"))
                    dgInputHist.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                if (dgInputHist.Columns.Contains("EQPT_INPUT_PTN_QTY"))
                    dgInputHist.Columns["EQPT_INPUT_PTN_QTY"].Visibility = Visibility.Collapsed;

                if (dgInputHist.Columns.Contains("INPUT_QTY"))
                    dgInputHist.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                if (dgInputHist.Columns.Contains("WIPQTY_IN"))
                    dgInputHist.Columns["WIPQTY_IN"].Visibility = Visibility.Collapsed;
            }
            #endregion
            btnRollMap.Visibility = Visibility.Collapsed;
            btnRollMapUpdate.Visibility = Visibility.Collapsed;
            //[E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Main 믹서/Sub 믹서 생산실적 조회 화면 표준화 요청건
            Util.gridClear(dgMaterial);
        }
        #endregion

        #region 탭, Grid Column Visibility
        private void AreaCheck(string sProcID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                //ROLL PRESS 공정일때도 TAG수를 보여줘야 하는 AREA인지 체크(CSRC20200508-000359)
                bool bTagView = RollPressTagCountViewCheck();

                chkPtnLen.IsChecked = false;
                TabReInput.Visibility = Visibility.Collapsed;
                tbInputDiffQty.Visibility = Visibility.Collapsed;
                txtInputDiffQty.Visibility = Visibility.Collapsed;
                tbLengthExceedQty.Visibility = Visibility.Collapsed;
                txtLengthExceedQty.Visibility = Visibility.Collapsed;
                tbLaneQty.Visibility = Visibility.Collapsed;
                tbProdVerCode.Visibility = Visibility.Collapsed;
                txtLaneQty.Visibility = Visibility.Collapsed;
                txtProdVerCode.Visibility = Visibility.Collapsed;
                cboTopBackTiltle.Visibility = Visibility.Collapsed;
                cboTopBack.Visibility = Visibility.Collapsed;
                cboElecTypeTiltle.Visibility = Visibility.Collapsed;
                cboElecType.Visibility = Visibility.Collapsed;

                tbWipWrkTypeCode.Visibility = Visibility.Collapsed;
                txtWipWrkTypeCOde.Visibility = Visibility.Collapsed;

                cboElecType.Visibility = Visibility.Collapsed;
                cTabDefectTag.Visibility = Visibility.Collapsed;
                cTabImage.Visibility = Visibility.Collapsed;
                cTabEqptDefect.Visibility = Visibility.Collapsed;
                cTabColor.Visibility = Visibility.Collapsed;
                cTabTrayTime.Visibility = Visibility.Collapsed;

                #region ################ 주석 #########################
                //dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY2_ED"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["EQPT_END_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["SRS1_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["SRS2_QTY"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["ROLLPRESS_COUNT"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Collapsed;

                //dgLotList.Columns["EQPT_END_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["INPUT_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY_ED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["WIPQTY2_ED_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_DFCT_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_LOSS_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"].Visibility = Visibility.Collapsed;
                //dgLotList.Columns["LENGTH_EXCEED2_EA"].Visibility = Visibility.Collapsed;
                #endregion #######################################

                List<string> ColumnsVisibility = new List<string>();
                ColumnsVisibility.Add("EQPTID_COATER");
                ColumnsVisibility.Add("EQPT_END_PSTN_ID");
                ColumnsVisibility.Add("INPUT_DIFF_QTY");
                ColumnsVisibility.Add("PR_LOTID");
                ColumnsVisibility.Add("WIPQTY2_ED");
                ColumnsVisibility.Add("CNFM_LOSS_QTY2");
                ColumnsVisibility.Add("CNFM_DFCT_QTY2");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY2");
                ColumnsVisibility.Add("PROD_VER_CODE");
                ColumnsVisibility.Add("EQPT_END_QTY");
                ColumnsVisibility.Add("SRS1_QTY");
                ColumnsVisibility.Add("SRS2_QTY");
                ColumnsVisibility.Add("COUNTQTY");
                ColumnsVisibility.Add("ROLLPRESS_COUNT");
                ColumnsVisibility.Add("LENGTH_EXCEED");
                ColumnsVisibility.Add("LENGTH_EXCEED2");
                ColumnsVisibility.Add("TAG_QTY");
                ColumnsVisibility.Add("EQP_TAG_QTY");

                ColumnsVisibility.Add("EQPT_END_QTY_EA");
                ColumnsVisibility.Add("INPUT_QTY_EA");
                ColumnsVisibility.Add("WIPQTY_ED_EA");
                ColumnsVisibility.Add("CNFM_DFCT_QTY_EA");
                ColumnsVisibility.Add("CNFM_LOSS_QTY_EA");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY_EA");
                ColumnsVisibility.Add("LENGTH_EXCEED_EA");
                ColumnsVisibility.Add("WIPQTY2_ED_EA");
                ColumnsVisibility.Add("CNFM_DFCT_QTY2_EA");
                ColumnsVisibility.Add("CNFM_LOSS_QTY2_EA");
                ColumnsVisibility.Add("CNFM_PRDT_REQ_QTY2_EA");
                ColumnsVisibility.Add("LENGTH_EXCEED2_EA");
                ColumnsVisibility.Add("FOIL_LOTID");
                ColumnsVisibility.Add("AUTO_STOP_FLAG");

                // 원형, 조소형 관련
                ColumnsVisibility.Add("DIFF_QTY");
                ColumnsVisibility.Add("INPUT_DIFF_QTY_WS");
                ColumnsVisibility.Add("WIPQTY_END_WS");
                ColumnsVisibility.Add("DFCT_QTY_WS");
                ColumnsVisibility.Add("LOSS_QTY_WS");
                ColumnsVisibility.Add("REQ_QTY_WS");
                ColumnsVisibility.Add("BOXQTY_IN");
                ColumnsVisibility.Add("BOXQTY");
                ColumnsVisibility.Add("WINDING_RUNCARD_ID");
                ColumnsVisibility.Add("LOTID_AS");
                ColumnsVisibility.Add("MODL_CHG_FRST_PROD_LOT_FLAG");
                ColumnsVisibility.Add("WIP_WRK_TYPE_CODE_DESC");
                /////////////////////////////////////////////////
                ColumnsVisibility.Add("AFTER_PUNCH_CUTOFF_COUNT");
                ColumnsVisibility.Add("REWINDING_DFCT_TAG_QTY");
                ColumnsVisibility.Add("THICKNESS_VALUE");
                ColumnsVisibility.Add("WIDTH_VALUE");
                
                //C20200519-000286
                dgLotList.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;

                // Visibility.Collapsed
                foreach (string str in ColumnsVisibility)
                {
                    if (dgLotList.Columns.Contains(str))
                        dgLotList.Columns[str].Visibility = Visibility.Collapsed;

                    if (dgModelList.Columns.Contains(str))
                        dgModelList.Columns[str].Visibility = Visibility.Collapsed;
                }

                dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Collapsed;
                dgDefect.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Collapsed;

                tbAType.Visibility = Visibility.Collapsed;
                tbCType.Visibility = Visibility.Collapsed;

                dgSubLot.Columns["PRINT"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPCL_RSNCODE"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["SPCL_LOT_GR_CODE"].Visibility = Visibility.Collapsed;
                dgSubLot.Columns["FORM_MOVE_STAT_CODE_NAME"].Visibility = Visibility.Collapsed;

                dgInputHist.Columns["CSTID"].Visibility = Visibility.Collapsed;
                //dgInputHist.Columns["SCAN_LOTID"].Visibility = Visibility.Collapsed;

                cTabInputMaterial.Visibility = Visibility.Collapsed;
                cTabSlurry.Visibility = Visibility.Collapsed;
                cTabHalf.Visibility = Visibility.Collapsed;
                cTabWipNote.Visibility = Visibility.Collapsed;

                btnQualityInfo.Visibility = Visibility.Collapsed;

                //if (sProcID.Equals(Process.PACKAGING))
                //    tbInqty.Text = ObjectDic.Instance.GetObjectName("투입량");
                //else
                //    tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량");

                dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");
                dgLotList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("생산량");

                dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");
                dgModelList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("생산량");

                if (sProcID.Equals(Process.PACKAGING))
                {
                    dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");
                    tbInqty.Text = ObjectDic.Instance.GetObjectName("투입량");

                    dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");

                    cTabTrayTime.Visibility = Visibility.Visible;
                }
                else
                {
                    if (((_AREATYPE.Equals("E") && !sProcID.Equals(Process.MIXING)) && (_AREATYPE.Equals("E") && !sProcID.Equals(Process.COATING)))
                         ||
                        (_AREATYPE.Equals("A") && sProcID.Equals(Process.NOTCHING)))
                    {
                        dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량(투입)");
                        dgLotList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("생산량(투입)");
                        tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량(투입)");

                        dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량(투입)");
                        dgModelList.Columns["INPUT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("생산량(투입)");
                    }
                    else
                    {
                        tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량");
                    }
                }


                if (!_AREATYPE.Equals("A"))
                {
                    chkPtnLen.IsEnabled = true;
                    dgLotList.Columns["EQPT_END_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["JUDG_NAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    chkPtnLen.IsEnabled = false;
                }

                if (_AREATYPE.Equals("A") && !sProcID.Equals(Process.NOTCHING)) //반제품 탭 보여주기
                {
                    cTabHalf.Visibility = Visibility.Visible;
                    dgInputHist.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;

                    dgLotList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량");
                    dgLotList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량");
                    dgLotList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구");

                    dgModelList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량");
                    dgModelList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량");
                    dgModelList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구");
                }
                else
                {
                    dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["WIPQTY2_ED"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;

                    dgModelList.Columns["WIPQTY2_ED"].Visibility = Visibility.Visible;
                    dgModelList.Columns["CNFM_LOSS_QTY2"].Visibility = Visibility.Visible;
                    dgModelList.Columns["CNFM_DFCT_QTY2"].Visibility = Visibility.Visible;
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY2"].Visibility = Visibility.Visible;

                    dgLotList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량(Roll)");
                    dgLotList.Columns["WIPQTY2_ED"].Header = ObjectDic.Instance.GetObjectName("양품량(Lane)");
                    dgLotList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량(Roll)");
                    dgLotList.Columns["CNFM_DFCT_QTY2"].Header = ObjectDic.Instance.GetObjectName("불량량(Lane)");
                    dgLotList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Roll)");
                    dgLotList.Columns["CNFM_LOSS_QTY2"].Header = ObjectDic.Instance.GetObjectName(@"LOSS량(Lane)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구(Roll)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2"].Header = ObjectDic.Instance.GetObjectName("물품청구(Lane)");

                    dgLotList.Columns["WIPQTY_ED_EA"].Header = ObjectDic.Instance.GetObjectName("양품량(Roll)");
                    dgLotList.Columns["WIPQTY2_ED_EA"].Header = ObjectDic.Instance.GetObjectName("양품량(Lane)");
                    dgLotList.Columns["CNFM_DFCT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("불량량(Roll)");
                    dgLotList.Columns["CNFM_DFCT_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("불량량(Lane)");
                    dgLotList.Columns["CNFM_LOSS_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Roll)");
                    dgLotList.Columns["CNFM_LOSS_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Lane)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("물품청구(Roll)");
                    dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("물품청구(Lane)");

                    dgModelList.Columns["WIPQTY_ED"].Header = ObjectDic.Instance.GetObjectName("양품량(Roll)");
                    dgModelList.Columns["WIPQTY2_ED"].Header = ObjectDic.Instance.GetObjectName("양품량(Lane)");
                    dgModelList.Columns["CNFM_DFCT_QTY"].Header = ObjectDic.Instance.GetObjectName("불량량(Roll)");
                    dgModelList.Columns["CNFM_DFCT_QTY2"].Header = ObjectDic.Instance.GetObjectName("불량량(Lane)");
                    dgModelList.Columns["CNFM_LOSS_QTY"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Roll)");
                    dgModelList.Columns["CNFM_LOSS_QTY2"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Lane)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY"].Header = ObjectDic.Instance.GetObjectName("물품청구(Roll)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY2"].Header = ObjectDic.Instance.GetObjectName("물품청구(Lane)");

                    dgModelList.Columns["WIPQTY_ED_EA"].Header = ObjectDic.Instance.GetObjectName("양품량(Roll)");
                    dgModelList.Columns["WIPQTY2_ED_EA"].Header = ObjectDic.Instance.GetObjectName("양품량(Lane)");
                    dgModelList.Columns["CNFM_DFCT_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("불량량(Roll)");
                    dgModelList.Columns["CNFM_DFCT_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("불량량(Lane)");
                    dgModelList.Columns["CNFM_LOSS_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Roll)");
                    dgModelList.Columns["CNFM_LOSS_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("LOSS량(Lane)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY_EA"].Header = ObjectDic.Instance.GetObjectName("물품청구(Roll)");
                    dgModelList.Columns["CNFM_PRDT_REQ_QTY2_EA"].Header = ObjectDic.Instance.GetObjectName("물품청구(Lane)");

                }

                dgLotList.Columns["MOLD_ID"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["STD_TOOL_ID"].Visibility = Visibility.Collapsed; // 2023.12.12 윤지해 E20231211-000182 표준공구ID 숨김처리
                dgLotList.Columns["MOLD_USE_COUNT"].Visibility = Visibility.Collapsed;

                if (sProcID.Equals(Process.NOTCHING))
                {
                    cTabHalf.Visibility = Visibility.Collapsed;
                    dgLotList.Columns["EQPT_END_PSTN_ID"].Visibility = Visibility.Visible;

                    dgLotList.Columns["MOLD_ID"].Visibility = Visibility.Visible;
                    dgLotList.Columns["STD_TOOL_ID"].Visibility = Visibility.Visible; // 2023.12.12 윤지해 E20231211-000182 표준공구ID 숨김처리
                    dgLotList.Columns["MOLD_USE_COUNT"].Visibility = Visibility.Visible;

                    if (IsAreaCommonCodeUse("CELL_TWS_REEL_NO_RULE",null))
                        dgLotList.Columns["LOADING_LEVEL"].Visibility = Visibility.Visible;
                    else
                        dgLotList.Columns["LOADING_LEVEL"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgLotList.Columns["LOADING_LEVEL"].Visibility = Visibility.Collapsed;
                }

                if (sProcID.Equals(Process.LAMINATION))
                {
                    dgSubLot.Columns["PRINT"].Visibility = Visibility.Visible;
                    #region
                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("카세트ID");
					#endregion

					//E20230330 - 001350 : ESNJ 라미, 폴딩 공정 완성Lot 탭 REMARK 컬럼 Visible
					//E20231107-000486 : ESNJ 라미,폴딩 공정 실적확정(완성Lot Tab) 특이사항 컬럼 Visible
					if (LoginInfo.CFG_SHOP_ID == "G182")
                    {
                        dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Visible;
						dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Visible;
					}
                }

                /// [C20190415_74474] x-ray 재발행 버튼 추가
                if (sProcID.Equals(Process.XRAY_REWORK))
                {
                    dgSubLot.Columns["PRINT"].Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.STACKING_FOLDING))
                {
                    #region
                    dgSubLot.Columns["CSTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("카세트ID");
                    #endregion
                    dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;
                    txtWipWrkTypeCOde.Visibility = Visibility.Visible;
                    tbWipWrkTypeCode.Visibility = Visibility.Visible;

					//E20230330-001350 : ESNJ 라미, 폴딩 공정 완성Lot 탭 REMARK 컬럼 Visible
					//E20231107-000486 : ESNJ 라미,폴딩 공정 실적확정(완성Lot Tab) 특이사항 컬럼 Visible
					if (LoginInfo.CFG_SHOP_ID == "G182")
                    {
                        dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Visible;
						dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Visible;
					}
                }

                if (sProcID.Equals(Process.CT_INSP))
                {
                    dgSubLot.Columns["PRINT"].Visibility = Visibility.Visible;
                    dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;   // 2023.11.07 윤지해 E20230828-000349 작업유형 추가
                }

                if (sProcID.Equals(Process.PACKAGING))
                {
                    btnQualityInfo.Visibility = Visibility.Visible;

                    dgLotList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["WIP_WRK_TYPE_CODE_DESC"].Visibility = Visibility.Visible;

                    dgModelList.Columns["INPUT_DIFF_QTY"].Visibility = Visibility.Visible;

                    dgSubLot.Columns["SPECIALYN"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["SPCL_RSNCODE"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["SPECIALDESC"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["SPCL_LOT_GR_CODE"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["FORM_MOVE_STAT_CODE_NAME"].Visibility = Visibility.Visible;

                    dgSubLot.Columns["LOTID"].Visibility = Visibility.Collapsed;
                    dgSubLot.Columns["PRINT_YN"].Visibility = Visibility.Collapsed;
                    dgSubLot.Columns["DISPATCH_YN"].Visibility = Visibility.Collapsed;

                    tbInputDiffQty.Visibility = Visibility.Visible;
                    txtInputDiffQty.Visibility = Visibility.Visible;

                    cTabQualityInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgSubLot.Columns["LOTID"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["PRINT_YN"].Visibility = Visibility.Visible;
                    dgSubLot.Columns["DISPATCH_YN"].Visibility = Visibility.Visible;
                    cTabQualityInfo.Visibility = Visibility.Visible;
                    #region
                    dgSubLot.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Tray ID");
                    #endregion
                }

                // C20211114-000011, Rewinding Process 전극생산Site
                //if (sProcID.Equals(Process.ROLL_PRESSING) || sProcID.Equals(Process.SLITTING) || sProcID.Equals(Process.SLIT_REWINDING))
                //{
                //    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible;
                //}

                if (sProcID.Equals(Process.ROLL_PRESSING))
                {

                    dgLotList.Columns["ROLLPRESS_COUNT"].Visibility = Visibility.Visible;
                    dgLotList.Columns["AUTO_STOP_FLAG"].Visibility = Visibility.Visible;    // Auto Stop 추가 [2018-09-19]
                    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible; // C20211114-000011, Rewinding Process 전극생산Site
                    dgLotList.Columns["AFTER_PUNCH_CUTOFF_COUNT"].Visibility = Visibility.Visible; // CSR : C20220117-000174 [2022.06.23]

                    cTabColor.Visibility = Visibility.Visible;

                    if (bTagView)
                    {
                        dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                        dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;

                        // 폴란드 전극인 경우만 보임
                        if (LoginInfo.CFG_SHOP_ID == "G482")
                            dgLotList.Columns["EQP_TAG_QTY"].Visibility = Visibility.Visible;
                    }

                    // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
                    if (string.Equals(_IncomeShortConnectLossApplyFlag, "Y"))
                    {
                        if (chkPtnLen.IsChecked == true) // [M수량환산]
                        {
                            dgLotList.Columns["CONNECTION_LOSS_ROLL"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL"].Visibility = Visibility.Collapsed;

                            dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgLotList.Columns["CONNECTION_LOSS_ROLL"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL"].Visibility = Visibility.Visible;

                            dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"].Visibility = Visibility.Collapsed;
                        }
                    }
                }

                if (sProcID.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible; // C20211114-000011, Rewinding Process 전극생산Site
                    dgLotList.Columns["AFTER_PUNCH_CUTOFF_COUNT"].Visibility = Visibility.Visible; // CSR : C20220117-000174 [2022.06.23]
                    
                    dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;

                    // 폴란드 전극인 경우만 보임
                    if (LoginInfo.CFG_SHOP_ID == "G482")
                        dgLotList.Columns["EQP_TAG_QTY"].Visibility = Visibility.Visible;

                    if (IsElectrodeGradeInfo())
                    {
                        tiElectrodeGradeInfo.Visibility = Visibility.Visible;
                        dgLotList.Columns["THICKNESS_VALUE"].Visibility = Visibility.Visible;
                        dgLotList.Columns["WIDTH_VALUE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tiElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                        dgLotList.Columns["THICKNESS_VALUE"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["WIDTH_VALUE"].Visibility = Visibility.Collapsed;
                    }

                    // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
                    if (string.Equals(_IncomeShortConnectLossApplyFlag, "Y"))
                    {
                        if (chkPtnLen.IsChecked == true) // [M수량환산]
                        {
                            dgLotList.Columns["CONNECTION_LOSS_ROLL"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL"].Visibility = Visibility.Collapsed;

                            dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgLotList.Columns["CONNECTION_LOSS_ROLL"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL"].Visibility = Visibility.Visible;

                            dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"].Visibility = Visibility.Collapsed;
                            dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"].Visibility = Visibility.Collapsed;
                        }
                    } 
                }

                // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
                if (!sProcID.Equals(Process.ROLL_PRESSING) && !sProcID.Equals(Process.SLITTING))
                {
                    dgLotList.Columns["CONNECTION_LOSS_ROLL"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INCOME_SHORTAGE_ROLL"].Visibility = Visibility.Collapsed;

                    dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"].Visibility = Visibility.Collapsed;
                }

                    if (sProcID.Equals(Process.SLIT_REWINDING))
                {
                    dgLotList.Columns["EQPTID_COATER"].Visibility = Visibility.Visible; // C20211114-000011, Rewinding Process 전극생산Site
                    dgLotList.Columns["REWINDING_DFCT_TAG_QTY"].Visibility = Visibility.Visible;  // CSR : C20220117-000174 [2022.06.23]
                }

                if (sProcID.Equals(Process.MIXING))
                {
                    cTabInputMaterial.Visibility = Visibility.Visible;
                }

                // Vision Image는 Sliiter 공정에서만 보여지게 추가 ( 2017-01-05 )
                if (sProcID.Equals(Process.COATING))
                {
                    dgLotList.Columns["FOIL_LOTID"].Visibility = Visibility.Visible;

                    cTabInputMaterial.Visibility = Visibility.Visible;
                    //cTabSlurry.Visibility = Visibility.Visible;

                    tbLaneQty.Visibility = Visibility.Visible;
                    tbProdVerCode.Visibility = Visibility.Visible;
                    txtLaneQty.Visibility = Visibility.Visible;
                    txtProdVerCode.Visibility = Visibility.Visible;

                    //dgInputHist.Columns["SCAN_LOTID"].Visibility = Visibility.Visible;

                    //C20200519-000286
                    if (AreaViewCheck("HS_SIDE_VIEW_AREA", LoginInfo.CFG_AREA_ID))
                        dgLotList.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;

                }

                if (sProcID.Equals(Process.COATING) || sProcID.Equals(Process.INS_COATING) || sProcID.Equals(Process.INS_SLIT_COATING))
                {
                    cboTopBackTiltle.Visibility = Visibility.Visible;
                    cboTopBack.Visibility = Visibility.Visible;

                    //dgInputHist.Columns["SCAN_LOTID"].Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.REWINDER) || sProcID.Equals(Process.LASER_ABLATION) || sProcID.Equals(Process.BACK_WINDER))
                    cTabDefectTag.Visibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(sProcID) && !sProcID.Substring(0, 1).Equals("E"))
                    cTabEqptDefect.Visibility = Visibility.Visible;

                if (sProcID.Equals(Process.SRS_MIXING) ||
                    sProcID.Equals(Process.SRS_COATING) ||
                    sProcID.Equals(Process.SRS_SLITTING) ||
                    sProcID.Equals(Process.SRS_BOXING))
                {
                    dgLotList.Columns["SRS1_QTY"].Visibility = Visibility.Visible;
                    dgLotList.Columns["SRS2_QTY"].Visibility = Visibility.Visible;
                }


                // CWA단선횟수 관리요청으로 추가 [2019-04-22]
                if (sProcID.Equals(Process.COATING) ||
                     sProcID.Equals(Process.ROLL_PRESSING) ||
                     sProcID.Equals(Process.HALF_SLITTING) ||
                     sProcID.Equals(Process.SLITTING))
                {
                    if (LoginInfo.CFG_SHOP_ID.Equals("A041") || LoginInfo.CFG_SHOP_ID.Equals("A011"))
                    {
                        dgLotList.Columns["COUNTQTY2"].Visibility = Visibility.Visible;
                        dgLotList.Columns["COUNTQTY"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgLotList.Columns["COUNTQTY"].Visibility = Visibility.Visible;
                        dgLotList.Columns["COUNTQTY2"].Visibility = Visibility.Collapsed;
                    }
                }

                // 길이초과 칼럼은 전극에서 MIXING제외 조립에선 NOTCHING만
                if (((_AREATYPE.Equals("E") && !sProcID.Equals(Process.MIXING)) && (_AREATYPE.Equals("E") && !sProcID.Equals(Process.COATING)))
                 ||
                (_AREATYPE.Equals("A") && sProcID.Equals(Process.NOTCHING)))
                {
                    tbLengthExceedQty.Text = ObjectDic.Instance.GetObjectName("길이초과");
                    tbLengthExceedQty.Visibility = Visibility.Visible;
                    txtLengthExceedQty.Visibility = Visibility.Visible;

                    dgLotList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
                    dgLotList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Visible;

                    dgModelList.Columns["LENGTH_EXCEED"].Visibility = Visibility.Visible;
                    dgModelList.Columns["LENGTH_EXCEED2"].Visibility = Visibility.Visible;
                }

                if (_AREATYPE.Equals("E"))
                {
                    cboElecTypeTiltle.Visibility = Visibility.Visible;
                    cboElecType.Visibility = Visibility.Visible;
                }

                // 원형, 초소형 투입 반제품 탭 
                if (sProcID.Equals(Process.WINDING) || sProcID.Equals(Process.ASSEMBLY) || sProcID.Equals(Process.WASHING))
                {
                    cTabEqptDefect.Visibility = Visibility.Visible;

                    if (sProcID.Equals(Process.WINDING))
                    {
                        if (SetAssyCSTIDView())
                        {
                            dgLotList.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("대차ID");
                            dgLotList.Columns["CSTID"].Visibility = Visibility.Visible;
                        }

                        dgLotList.Columns["WINDING_RUNCARD_ID"].Visibility = Visibility.Visible;
                        dgLotList.Columns["LOTID_AS"].Visibility = Visibility.Visible;
                        dgLotList.Columns["MODL_CHG_FRST_PROD_LOT_FLAG"].Visibility = Visibility.Visible;

                        cTabInputHalfProduct.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        cTabInputHalfProduct.Visibility = Visibility.Visible;
                        dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Collapsed;
                        dgInHalfProduct.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                        dgInHalfProduct.Columns["EQPT_INPUT_QTY"].Visibility = Visibility.Collapsed;

                        if (sProcID.Equals(Process.ASSEMBLY))
                        {
                            cTabHalf.Visibility = Visibility.Collapsed;
                            dgInHalfProduct.Columns["WN_LOTID"].Visibility = Visibility.Visible;
                            dgInHalfProduct.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                            dgInHalfProduct.Columns["EQPT_INPUT_QTY"].Visibility = Visibility.Visible;

                            dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");
                            dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("투입량");
                            tbInqty.Text = ObjectDic.Instance.GetObjectName("투입량");

                            tbInputDiffQty.Visibility = Visibility.Visible;
                            txtInputDiffQty.Visibility = Visibility.Visible;

                            tbLengthExceedQty.Text = ObjectDic.Instance.GetObjectName("재투입");
                            tbLengthExceedQty.Visibility = Visibility.Visible;
                            txtLengthExceedQty.Visibility = Visibility.Visible;


                            dgLotList.Columns["DIFF_QTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["INPUT_DIFF_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["WIPQTY_END_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["DFCT_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["LOSS_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["REQ_QTY_WS"].Visibility = Visibility.Visible;
                            dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;

                            dgModelList.Columns["DIFF_QTY"].Visibility = Visibility.Visible;
                            dgModelList.Columns["INPUT_DIFF_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["WIPQTY_END_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["DFCT_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["LOSS_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["REQ_QTY_WS"].Visibility = Visibility.Visible;
                            dgModelList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgLotList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");
                            dgModelList.Columns["INPUT_QTY"].Header = ObjectDic.Instance.GetObjectName("생산량");
                            tbInqty.Text = ObjectDic.Instance.GetObjectName("생산량");

                            dgLotList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                            dgLotList.Columns["BOXQTY_IN"].Visibility = Visibility.Visible;

                            dgModelList.Columns["BOXQTY"].Visibility = Visibility.Visible;
                            dgModelList.Columns["BOXQTY_IN"].Visibility = Visibility.Visible;
                        }

                    }
                }
                else
                {
                    cTabEqptDefect.Visibility = Visibility.Visible;
                    cTabInputHalfProduct.Visibility = Visibility.Collapsed;
                }

                if (sProcID.Equals(Process.NOTCHING_REWINDING))
                {
                    cTabEqptDefect.Visibility = Visibility.Collapsed;
                    cTabHalf.Visibility = Visibility.Collapsed;
                    cTabQualityInfo.Visibility = Visibility.Collapsed;
                    cTabInputHalf.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cTabInputHalf.Visibility = Visibility.Visible;
                }

                if (sProcID.Equals(Process.TWO_SLITTING))
                {
                    if (IsElectrodeGradeInfo())
                    {
                        tiElectrodeGradeInfo.Visibility = Visibility.Visible;
                        dgLotList.Columns["THICKNESS_VALUE"].Visibility = Visibility.Visible;
                        dgLotList.Columns["WIDTH_VALUE"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tiElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                        dgLotList.Columns["THICKNESS_VALUE"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["WIDTH_VALUE"].Visibility = Visibility.Collapsed;
                    }
                }


                // CSR : C20210720-000108 
                if (IsAreaCommonCodeUse("MNG_ROLL_DIR_AREA", sProcID))
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Visible;
                    dgLotList.Columns["EM_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["EM_SECTION_ROLL_DIRCTN"].Visibility = Visibility.Collapsed;
                }

                // C20210928-000539 / 재와인딩 NG TAG수 칼럼 추가
                if (IsAreaCommonCodeUse("REMARK_NG_TAG_COL_USE", sProcID))
                {
                    dgLotList.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                    dgDefect.Columns["TAG_QTY"].Visibility = Visibility.Visible;
                }

                string sBizname = string.Empty;
                string[] sEqptAttr = { LoginInfo.CFG_AREA_ID, sProcID };
                switch (sProcID)
                {
                    case "E2000":
                        sBizname = "COATER_QLTY_EQPT_VISIBLE";
                        break;
                    case "E4000":
                        sBizname = "DEFECT_COLUMN_VISIBILITY";
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(sBizname) && IsCommoncodeAttrUse(sBizname, null, sEqptAttr))
                {
                    // C20220308-000577 - 절연 자주검사 설비값 [2022-04-27]
                    if (string.Equals(sProcID, Process.COATING))
                        dgQualityInfo.Columns["EQPT_VALUE"].Visibility = Visibility.Visible;
                    else
                        dgQualityInfo.Columns["EQPT_VALUE"].Visibility = Visibility.Collapsed;

                    // CSR : C20220406-000241
                    if (string.Equals(sProcID, Process.SLITTING))
                        dgEqpFaulty.Columns["QLTY_DFCT_QTY"].Visibility = Visibility.Visible;
                    else
                        dgEqpFaulty.Columns["QLTY_DFCT_QTY"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgQualityInfo.Columns["EQPT_VALUE"].Visibility = Visibility.Collapsed;
                    dgEqpFaulty.Columns["QLTY_DFCT_QTY"].Visibility = Visibility.Collapsed;
                }
                // 2023-08-11 김태우. 공정 등급 정보 슬리팅에서만 보여지도록 추가 
                if (IsElectrodeGradeInfo())
                {
                    tiElectrodeGradeInfo.Visibility = Visibility.Visible;
                    //dgLotList.Columns["THICKNESS_VALUE"].Visibility = Visibility.Visible;
                    //dgLotList.Columns["WIDTH_VALUE"].Visibility = Visibility.Visible;

                    //SetGridGrdCombo(dgElectrodeGradeInfo.Columns["GRD_JUDG_CODE"]);
                }
                else
                {
                    tiElectrodeGradeInfo.Visibility = Visibility.Collapsed;
                    //dgLotList.Columns["THICKNESS_VALUE"].Visibility = Visibility.Collapsed;
                    //dgLotList.Columns["WIDTH_VALUE"].Visibility = Visibility.Collapsed;
                }

                #region [E20240206-000574] 로딩 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건 
                CheckAreaUseTWS(cboArea.SelectedValue.ToString());
                SetVisibleObject_TWS(sProcID);

                #endregion

                #region [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Main 믹서/Sub 믹서 생산실적 조회 화면 표준화 요청건
                SetVisibleObject_MixResultStd(sProcID);
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool IsCommoncodeAttrUse(string sCodeType, string sCmCode, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTRIBUTE1", "ATTRIBUTE2", "ATTRIBUTE3", "ATTRIBUTE4", "ATTRIBUTE5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = (sCmCode == string.Empty ? null : sCmCode);
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTES", "RQSTDT", "RSLTDT", RQSTDT);

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

        private bool RollPressTagCountViewCheck()
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "ROLLPRESS_TAG_VIEW_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool AreaViewCheck(string sCMCDTYPE, string sAREAID)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCMCDTYPE;
                dr["CMCODE"] = sAREAID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
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
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["COM_TYPE_CODE"] = "GRD_JUDG_DISP_AREA";
                dr["COM_CODE"] = cboProcess.SelectedValue.ToString();   //E4000
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            return false;
        }
        private void ProcCheck(string sProcID, string sEqptID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                if (cboProcess.SelectedValue.Equals(Process.STACKING_FOLDING))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("EQPTID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["EQPTID"] = sEqptID;
                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQGRID", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows[0]["EQGRID"].ToString().Equals("FOL"))
                        _StackingYN = "N";
                    else
                        _StackingYN = "Y";

                    dgSubLot.Columns["PRINT"].Visibility = Visibility.Visible;
                    grdMBomTypeCnt.Visibility = Visibility.Visible;

                    if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                        dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Visible;
                    if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                        dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Visible;
                    if (dgDefect != null && dgDefect.Columns.Contains("DFCT_QTY_DDT_RATE"))
                        dgDefect.Columns["DFCT_QTY_DDT_RATE"].Visibility = Visibility.Visible;

                    tbAType.Visibility = Visibility.Visible;
                    tbCType.Visibility = Visibility.Visible;

                    if (_StackingYN.Equals("Y"))
                    {
                        tbAType.Text = "HALFTYPE";
                        tbCType.Text = "MONOTYPE";

                        if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                            dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "HALFTYPE";
                        if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                            dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "MONOTYPE";
                    }
                    else
                    {
                        tbAType.Text = "ATYPE";
                        tbCType.Text = "CTYPE";

                        if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                            dgDefect.Columns["A_TYPE_DFCT_QTY"].Header = "ATYPE";
                        if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                            dgDefect.Columns["C_TYPE_DFCT_QTY"].Header = "CTYPE";
                        if (dgDefect != null && dgDefect.Columns.Contains("DFCT_QTY_DDT_RATE"))
                            dgDefect.Columns["DFCT_QTY_DDT_RATE"].Header = ObjectDic.Instance.GetObjectName("비율");
                    }

                    GetMBOMInfo();
                }
                else
                {
                    grdMBomTypeCnt.Visibility = Visibility.Collapsed;

                    if (dgDefect != null && dgDefect.Columns.Contains("A_TYPE_DFCT_QTY"))
                        dgDefect.Columns["A_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                    if (dgDefect != null && dgDefect.Columns.Contains("C_TYPE_DFCT_QTY"))
                        dgDefect.Columns["C_TYPE_DFCT_QTY"].Visibility = Visibility.Collapsed;
                    if (dgDefect != null && dgDefect.Columns.Contains("DFCT_QTY_DDT_RATE"))
                        dgDefect.Columns["DFCT_QTY_DDT_RATE"].Header = ObjectDic.Instance.GetObjectName("비율");

                    tbAType.Visibility = Visibility.Collapsed;
                    tbCType.Visibility = Visibility.Collapsed;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 공정별 숫자 Format
        private void SetProcessNumFormat(string sProcid)
        {
            // 숫자값 Format 
            string sFormat = string.Empty;

            if (sProcid.Equals(Process.MIXING))
            {
                // MIXING
                sFormat = "###,##0.###";
            }
            else if (_AREATYPE.Equals("A"))
            {
                // 조립
                sFormat = "###,##0";
            }
            else
            {
                // 전극
                if (_UNITCODE.Equals("EA"))
                    sFormat = "###,##0.##";
                else
                    sFormat = "###,##0.#";
            }

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_END_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY2_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_DIFF_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED2"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["A_TYPE_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["C_TYPE_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgDefect.Columns["RESNQTY"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgEqpFaulty.Columns["DFCT_QTY"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_END_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INPUT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["WIPQTY2_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["LENGTH_EXCEED2_EA"])).Format = sFormat;

            // [E20230927-000893] [LGESWA PI Team] Add Connection Loss and Income shortage column to Search Production Result by Lot
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CONNECTION_LOSS_ROLL"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CONNECTION_LOSS_ROLL_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INCOME_SHORTAGE_ROLL"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["INCOME_SHORTAGE_ROLL_EA"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["PRE_PROC_INPUT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["EQPT_INPUT_QTY"])).Format = sFormat;
            
            txtInputQty.Format = sFormat;
            txtOutQty.Format = sFormat;
            txtDefectQty.Format = sFormat;
            txtLossQty.Format = sFormat;
            txtPrdtReqQty.Format = sFormat;
            txtInputDiffQty.Format = sFormat;
            txtLengthExceedQty.Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["INPUT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY2_ED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY2"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["INPUT_DIFF_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED2"])).Format = sFormat;

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["INPUT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["WIPQTY2_ED_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_DFCT_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_LOSS_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["CNFM_PRDT_REQ_QTY2_EA"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgModelList.Columns["LENGTH_EXCEED2_EA"])).Format = sFormat;

        }
        #endregion

        #region [값셋팅]
        private void SetValue(object oContext)
        {
            txtSelectLot.Text = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID")); ;
            txtWorkorder.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WOID"));
            txtWorkorderDetail.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WO_DETL_ID"));
            //txtLotStatus.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSNAME"));
            txtWorkdate.Text = Util.NVC(DataTableConverter.GetValue(oContext, "CALDATE"));
            txtShift.Text = Util.NVC(DataTableConverter.GetValue(oContext, "SHFT_NAME"));
            txtShift.Tag = Util.NVC(DataTableConverter.GetValue(oContext, "SHIFT"));
            txtStartTime.Text = Util.NVC(DataTableConverter.GetValue(oContext, "STARTDTTM"));
            txtWorker.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WRK_USER_NAME"));
            txtEndDttm.Text = Util.NVC(DataTableConverter.GetValue(oContext, "ENDDTTM"));
            txtInputQty.Value = string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_QTY"))) == true ? 0 : Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_QTY")));
            txtOutQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "WIPQTY_ED")));
            txtDefectQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_DFCT_QTY")));
            txtLossQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_LOSS_QTY")));
            txtPrdtReqQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "CNFM_PRDT_REQ_QTY")));
            txtWipWrkTypeCOde.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_WRK_TYPE_CODE_DESC"));

            if (Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.ASSEMBLY) || Util.NVC(DataTableConverter.GetValue(oContext, "PROCID")).Equals(Process.WASHING))
            {
                txtInputDiffQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "DIFF_QTY")));
                txtLengthExceedQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_DIFF_QTY")));
                txtLaneQty.Text = string.Empty;

                if (string.Equals(DataTableConverter.GetValue(oContext, "PROCID").GetString(), Process.ASSEMBLY))
                {
                    if (!string.IsNullOrEmpty(DataTableConverter.GetValue(oContext, "PROCID").GetString()))
                    {
                        if (GetReInputReasonApplyFlag(DataTableConverter.GetValue(oContext, "EQPTID").GetString()) == "Y")
                        {
                            TabReInput.Visibility = Visibility.Visible;
                            txtInputDiffQty.IsEnabled = false;
                        }
                        else
                        {
                            txtInputDiffQty.IsEnabled = true;
                            TabReInput.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        txtInputDiffQty.IsEnabled = true;
                        TabReInput.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                txtInputDiffQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "INPUT_DIFF_QTY")));
                txtLengthExceedQty.Value = Double.Parse(Util.NVC(DataTableConverter.GetValue(oContext, "LENGTH_EXCEED")));
                txtLaneQty.Text = Util.NVC_NUMBER(DataTableConverter.GetValue(oContext, "LANE_QTY"));
            }

            txtProdVerCode.Text = Util.NVC(DataTableConverter.GetValue(oContext, "PROD_VER_CODE"));

            //txtNote.Text = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));

            _AREAID = Util.NVC(DataTableConverter.GetValue(oContext, "AREAID"));
            _PROCID = Util.NVC(DataTableConverter.GetValue(oContext, "PROCID"));
            _EQSGID = Util.NVC(DataTableConverter.GetValue(oContext, "EQSGID"));
            _EQPTID = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTID"));
            _LOTID = Util.NVC(DataTableConverter.GetValue(oContext, "LOTID"));
            _WIPSEQ = Util.NVC(DataTableConverter.GetValue(oContext, "WIPSEQ"));
            _LANEPTNQTY = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_PTN_QTY"));
            ////_ORGQTY = Util.NVC(DataTableConverter.GetValue(oContext, "ORG_QTY"));
            _WIP_NOTE = Util.NVC(DataTableConverter.GetValue(oContext, "WIP_NOTE"));
            _LANE = Util.NVC(DataTableConverter.GetValue(oContext, "LANE_QTY"));
            _EQPTNAME = Util.NVC(DataTableConverter.GetValue(oContext, "EQPTNAME"));

            txtNote.Text = GetWipNote();

            //txtWorkorder.FontWeight = FontWeights.Normal;
            //txtLotStatus.FontWeight = FontWeights.Normal;
            //txtWorkdate.FontWeight = FontWeights.Normal;
            //txtShift.FontWeight = FontWeights.Normal;
            //txtStartTime.FontWeight = FontWeights.Normal;
            //txtWorker.FontWeight = FontWeights.Normal;
            //ldpDatePicker.FontWeight = FontWeights.Normal;
            //txtOutQty.FontWeight = FontWeights.Normal;
            //txtDefectQty.FontWeight = FontWeights.Normal;
            //txtLossQty.FontWeight = FontWeights.Normal;
            //txtPrdtReqQty.FontWeight = FontWeights.Normal;
            //txtNote.FontWeight = FontWeights.Normal;
        }
        #endregion

        #region [VISION IMAGE]
        private void GetVisionImage(string sLotId, string sDate)
        {
            try
            {
                // 추후 정보 가져오는 Biz추가예정
                if (string.IsNullOrEmpty(sDate))
                    return;

                // TEST용
                string address = "165.244.114.238";
                string userId = "mesmgr";
                string password = "mesmgr@2010";

                string rootPath = string.Format(@"ftp://{0}/", new object[] { address }) + @"test111/";
                rootPath += Convert.ToDateTime(sDate).ToString("yyyyMMdd") + @"/" + sLotId.Substring(0, 9) + "0" + sLotId.Substring(9, 1) + @"/";

                /*
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    return GetRequestImageList(rootPath, userId, password);
                }).ContinueWith((x) => {
                    Application.Current.Dispatcher.Invoke((Action)(delegate
                    {
                        ImageListView.ItemsSource = new List<ImageSource>(x.Result);
                    }));
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
                */
                ImageListView.ItemsSource = new List<ImageSource>();
                ImageListView.ItemsSource = Util.GetRequestImageList(rootPath, userId, password);
            }
            catch { /* Nothing */ }
        }

        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private string GetWipNote()
        {
            string sReturn;
            string[] sWipNote = _WIP_NOTE.Split('|');

            if (sWipNote.Length == 0)
            {
                sReturn = _WIP_NOTE;
            }
            else
            {
                sReturn = sWipNote[0];
            }
            return sReturn;
        }

        private string GetReInputReasonApplyFlag(string equipmentCode)
        {
            const string bizRuleName = "DA_PRD_SEL_REINPUT_RSN_APPLY_FLAG";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = equipmentCode;
            inTable.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            return CommonVerify.HasTableRow(dtResult) ? dtResult.Rows[0][0].GetString() : string.Empty;
        }

        private void GetDefectReInputList()
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_FOR_REINPUT";
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersDefectReInput = FilterCondition(dgDefectReInput);    //2023.05.09 김대현
                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();
                inTable.TableName = "INDATA";

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _PROCID;
                newRow["LOTID"] = _LOTID;
                newRow["WIPSEQ"] = _WIPSEQ;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = _EQPTID;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' 동별 / 공정별
                    //'LP' 라인 / 공정별
                    dgDefectReInput.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    dgDefectReInput.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                    dgDefectReInput.FilterBy(filtersDefectReInput); //2023.05.09 김대현
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetTrayLotByTime()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _LOTID;
                dt.Rows.Add(dr);
                KeyValuePair<C1.WPF.DataGrid.DataGridColumn, DataGridFilterState>[] filtersTrayInfo = FilterCondition(dgTrayInfo);  //2023.05.09 김대현

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EDIT_SUBLOT_QTY_BY_TIME", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgTrayInfo, result, FrameOperation, true);

                dgTrayInfo.FilterBy(filtersTrayInfo);   //2023.05.09 김대현
            }
            catch (Exception ex)
            {

            }
        }

        #region # Roll Map
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

        private bool IsRollMapLotAttribute(string sLotID)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = sLotID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                    if (string.Equals(result.Rows[0]["ROLLMAP_APPLY_FLAG"], "Y"))
                        return true;
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
                dr["PROCID"] = _PROCID;
                dr["EQSGID"] = _EQSGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null & dtResult.Rows.Count > 0)
                    if (string.Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private bool IsRollMapLot()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_ROLLMAP_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = _LOTID;
                dr["WIPSEQ"] = _WIPSEQ;
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["ROLLMAP_LOT_YN"].Equals("Y"))
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



        private void SelectElectrodeGradeInfo(string lotId, string wipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;
                dr["WIPSEQ"] = wipseq;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_GRD_JUDG_LOT", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgElectrodeGradeInfo, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region
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

        private void GetAreaByCheckData()
        {
            _workCalbuttonAuthYN = string.Empty; //월력관리/버튼권한 사용여부
            _unidentifiedLossYN = string.Empty;  //미확인LOSS 사용여부
            _reworkBoxTypeYN = string.Empty;     //재작업 BOX 생성시 작업유형 REWORK 확인
            _pkgInputCheckYN = string.Empty;     //PKG 투입시 재작업 완성 LOT CHECK 사용여부
            _qmsDefectCodeYN = string.Empty;     //QMS 불량코드 사용여부

            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CT_INSP_AREA_BY_CHECK";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _workCalbuttonAuthYN = dtResult.Rows[0]["ATTRIBUTE1"].GetString();
                    _unidentifiedLossYN = dtResult.Rows[0]["ATTRIBUTE2"].GetString();
                    _reworkBoxTypeYN = dtResult.Rows[0]["ATTRIBUTE3"].GetString();
                    _pkgInputCheckYN = dtResult.Rows[0]["ATTRIBUTE4"].GetString();
                    _qmsDefectCodeYN = dtResult.Rows[0]["ATTRIBUTE5"].GetString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        private void SetLotType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTYPE_FO", "INDATA", "OUTDATA", inTable);

                cboLotType.DisplayMemberPath = "CBO_NAME";
                cboLotType.SelectedValuePath = "CBO_CODE";

                DataRow dr = dtResult.NewRow();
                dr["CBO_NAME"] = "-ALL-";
                dr["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(dr, 0);

                cboLotType.ItemsSource = dtResult.Copy().AsDataView();

                cboLotType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        private void dgLotList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (Util.NVC(dg.CurrentColumn.Name).Equals("WIPHOLD") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y"
                    || Util.NVC(dg.CurrentColumn.Name).Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "WIPHOLD").ToString() == "Y")
                {
                    ShowHoldDetail(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID")));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void ShowHoldDetail(string pLotid)
        {
            COM001_018_HOLD_DETL wndRunStart = new COM001_018_HOLD_DETL();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = pLotid;
                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.ShowModal();
            }
        }

        private void btnHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                CMM001.Popup.CMM_COM_WIPHIST_HIST wndHist = new CMM001.Popup.CMM_COM_WIPHIST_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[2];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);

                    wndHist.Closed += new EventHandler(wndHist_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_COM_WIPHIST_HIST window = sender as CMM001.Popup.CMM_COM_WIPHIST_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgQualityInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg != null)
            {
                dg.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(e.Cell.Column.Name, "LSL") || string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                            }
                            else if (string.Equals(e.Cell.Column.Name, "CLCTVAL01"))
                            {
                                string sValue = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCTVAL01"));
                                string sLSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LSL"));
                                string sUSL = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "USL"));


                                if (sValue != null && !string.IsNullOrWhiteSpace(Util.NVC(sValue)) && !string.Equals(Util.NVC(sValue), Double.NaN.ToString()))
                                {
                                    if (sLSL != "" && Util.NVC_Decimal(sValue) < Util.NVC_Decimal(sLSL))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    }

                                    else if (sUSL != "" && Util.NVC_Decimal(sValue) > Util.NVC_Decimal(sUSL))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                    }
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }

                            }
                            else
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }));
            }
        }

        private void dgQualityInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg == null) return;

            dg.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e != null && e.Cell != null && e.Cell.Presenter != null)
                        {
                            e.Cell.Presenter.Background = null;

                            if (!string.Equals(e.Cell.Column.Name, "LSL") && !string.Equals(e.Cell.Column.Name, "USL"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                }
            }));
        }

        private void SetGridGrdCombo(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MATC_WINDING";
            dr["ATTRIBUTE1"] = "MATC_GRD";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);


            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);

        }

        private void SelectVoltGradeInfo(string lotId, string eqptid)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;
                dr["EQPTID"] = eqptid;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_GEL_INPUT_PANCAKE_GRD_JUDG_HIST", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgVoltGradeInfo, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void VoltGradeInfoVisible()
        {
            try
            {
                // SHOPID[F030],Area[M9],ProcessParent[A2000]인 경우 전극 등급 정보 탭 보여주기
                string areaId = Util.GetCondition(cboArea);
                string procId = Util.GetCondition(cboProcess);
                if(areaId.Equals("M9") && procId.Equals(Process.WINDING))
                {
                    cTabVoltGradeInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    cTabVoltGradeInfo.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //ESMI-A4동 6 Line 제외처리
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

        #region [E20240206-000574] 인라인 데이터(TWS) 연동을 위한 ESNJ MES(전극,조립) 시스템 기능 추가 요청 건
        private void CheckAreaUseTWS(string sArea)
        {
            try
            {
                isTWS_LOADING_TRACKING = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sArea;
                dr["COM_TYPE_CODE"] = "TWS_LOADING_TRACKING_YN";
                dr["COM_CODE"] = "TWS_LOADING_TRACKING_YN";
                dr["USE_FLAG"] = 'Y';

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0]["ATTR1"].ToString().Equals("Y") || dtResult.Rows[0]["ATTR1"].ToString().Equals("P"))
                        isTWS_LOADING_TRACKING = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetVisibleObject_TWS(string sProcID)
        {
            if (isTWS_LOADING_TRACKING == true)
            {
                if (sProcID.Left(1).Equals("E"))
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN1"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN2"].Visibility = Visibility.Visible;
                    dgLotList.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Visible;
                    dgLotList.Columns["INPUT_SECTION_ROLL_DIRCTN2"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["EM_SECTION_ROLL_DIRCTN2"].Visibility = Visibility.Collapsed;

                    if (sProcID.Equals(Process.SLITTING) || sProcID.Equals(Process.SLIT_REWINDING) || sProcID.Equals(Process.HEAT_TREATMENT))
                    {
                        dgLotList.Columns["LOADING_L"].Visibility = Visibility.Visible;
                        dgLotList.Columns["LOADING_R"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgLotList.Columns["LOADING_L"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["LOADING_R"].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN1"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN2"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["LOADING_L"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["LOADING_R"].Visibility = Visibility.Collapsed;
                }

                if (sProcID.Equals(Process.COATING))
                    cTabTWS_Loading.Visibility = Visibility.Visible;   //코팅 TWS 적재
                else
                    cTabTWS_Loading.Visibility = Visibility.Collapsed;

                if (sProcID.Equals(Process.NOTCHING))
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN1"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN1"].Visibility = Visibility.Collapsed;
                }

                if (sProcID.Equals(Process.NOTCHING) || sProcID.Equals(Process.ZZS))
                    dgInputHist.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Visible;
                else
                    dgInputHist.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Collapsed;
            }
            else
            {
                cTabTWS_Loading.Visibility = Visibility.Collapsed;   //코팅 TWS 로딩량

                dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN1"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_SECTION_ROLL_LANE_DIRCTN2"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["EM_SECTION_ROLL_LANE_DIRCTN"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["LOADING_L"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["LOADING_R"].Visibility = Visibility.Collapsed;
                dgInputHist.Columns["LOAD_WEIGHT1"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["INPUT_SECTION_ROLL_DIRCTN2"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["EM_SECTION_ROLL_DIRCTN2"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [E20240214-000850] 라미 공정 투입 음/양 구분 조회 요청 건

        private void chkPolarity_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkPolarity_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void InputPolarityChkBoxVisible()
        {
            try
            {
                string sAreaID = Util.GetCondition(cboArea);
                string procId = Util.GetCondition(cboProcess);

                if (procId.Equals(Process.LAMINATION))
                {
                    chkPolarity.Visibility = Visibility.Visible;                    
                }
                else
                {
                    chkPolarity.Visibility = Visibility.Collapsed;
                    chkPolarity.IsChecked = false;

                    if (dgLotList.Columns.Contains("PRDT_CLSS_NAME"))
                        dgLotList.Columns["PRDT_CLSS_NAME"].Visibility = Visibility.Collapsed;
                    if (dgLotList.Columns.Contains("PRE_PROC_INPUT_QTY"))
                        dgLotList.Columns["PRE_PROC_INPUT_QTY"].Visibility = Visibility.Collapsed;
                    if (dgLotList.Columns.Contains("EQPT_INPUT_QTY"))
                        dgLotList.Columns["EQPT_INPUT_QTY"].Visibility = Visibility.Collapsed;

                    dgLotList.AlternatingRowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF9F9F9"));
                    dgLotList.Rows[dgLotList.Rows.Count - 1].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                chkPolarity.Visibility = Visibility.Collapsed;
                chkPolarity.IsChecked = false;
            }
        }

        private void dgLotList_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (!_InputPolarityFlag) return;

                C1DataGrid dg = sender as C1DataGrid;

                int rowCount = 0;
                for (int row = 0; row < (dg.Rows.Count - dg.TopRows.Count) - 1; row++)
                {
                    rowCount++;

                    if (rowCount % 2 != 0)
                    {
                        for (int col = 0; col < dg.Columns.Count; col++)
                        {
                            if (
                                    dg.Columns[col].Name.Equals("PRDT_CLSS_NAME") ||
                                    dg.Columns[col].Name.Equals("PRE_PROC_INPUT_QTY") ||
                                    dg.Columns[col].Name.Equals("EQPT_INPUT_QTY"))
                            {
                                continue;
                            }
                            else
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(row + dg.TopRows.Count, col), dg.GetCell(row + dg.TopRows.Count + 1, col)));
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

        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (!_InputPolarityFlag) return;

                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                    if (panel == null && panel.Children == null && panel.Children.Count < 1) return;

                    ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                    if (e.Cell.Column.Index == dg.Columns["PRDT_CLSS_NAME"].Index)
                    {
                        if (e.Cell.Row.Index == (dg.Rows.Count - 2))
                        {
                            if (cboProcess.SelectedValue == null)
                                presenter.Content = string.Empty;                            
                            else if (cboProcess.SelectedValue.ToString() == Process.LAMINATION)
                                presenter.Content = ObjectDic.Instance.GetObjectName("음극");
                            else
                                presenter.Content = ObjectDic.Instance.GetObjectName("EA");
                        }
                        else if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                        {
                            if (cboProcess.SelectedValue == null)
                                presenter.Content = string.Empty;                            
                            else if (cboProcess.SelectedValue.ToString() == Process.LAMINATION)
                                presenter.Content = ObjectDic.Instance.GetObjectName("양극");                            
                        }
                    }
                    else if (e.Cell.Column.Index >= dg.Columns["PRE_PROC_INPUT_QTY"].Index || e.Cell.Column.Index < dg.Columns["LANE_QTY"].Index)
                    {
                        if (cboProcess.SelectedValue.ToString() != Process.LAMINATION) return;

                        if (dg.GetRowCount() > 0)
                        {
                            if (e.Cell.Row.Index == (dg.Rows.Count - 2))
                            {
                                presenter.Content = SetGridFormatBottomRow(e.Cell.Column.Name, SumGridColumnDecimal(e.Cell.Column.Name, 1));
                            }
                            else if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                            {
                                presenter.Content = SetGridFormatBottomRow(e.Cell.Column.Name, SumGridColumnDecimal(e.Cell.Column.Name, 2));
                            }
                        }

                    }
                }

            }));
        }

        private decimal SumGridColumnDecimal(string ColumnName, int SortNo)
        {
            DataRow[] dr;
            if (DataTableConverter.Convert(dgLotList.ItemsSource)?.Columns?.Contains("SORT_NO") == false)
                dr = DataTableConverter.Convert(dgLotList.ItemsSource).Select();
            else
                dr = DataTableConverter.Convert(dgLotList.ItemsSource).Select("SORT_NO = " + SortNo + "");

            if (dr.Length == 0) return 0;

            return dr.AsEnumerable().Sum(r => r.GetValue(ColumnName).GetDecimal());
        }

        private string SetGridFormatBottomRow(string ColumnName, object obj)
        {
            string sFormat = string.Empty;
            double dFormat = 0;

            sFormat = "{0:###,##0}";

            if (Double.TryParse(Util.NVC(obj), out dFormat))
                return String.Format(sFormat, dFormat);

            return string.Empty;
        }

        private void SetGridPolarity()
        {
            try
            {
                if (chkPolarity?.Visibility == Visibility.Visible && chkPolarity?.IsChecked == true && Util.GetCondition(cboProcess).Equals(Process.LAMINATION))
                {
                    if (dgLotList?.Columns?.Contains("PRDT_CLSS_NAME") == true)
                        dgLotList.Columns["PRDT_CLSS_NAME"].Visibility = Visibility.Visible;
                    if (dgLotList?.Columns?.Contains("PRE_PROC_INPUT_QTY") == true)
                        dgLotList.Columns["PRE_PROC_INPUT_QTY"].Visibility = Visibility.Visible;
                    if (dgLotList?.Columns?.Contains("EQPT_INPUT_QTY") == true)
                        dgLotList.Columns["EQPT_INPUT_QTY"].Visibility = Visibility.Visible;
                    
                    dgLotList.AlternatingRowBackground = null;
                    dgLotList.Rows[dgLotList.Rows.Count - 1].Visibility = Visibility.Visible;                
                }
                else
                {
                    if (dgLotList?.Columns?.Contains("PRDT_CLSS_NAME") == true)
                        dgLotList.Columns["PRDT_CLSS_NAME"].Visibility = Visibility.Collapsed;
                    if (dgLotList?.Columns?.Contains("PRE_PROC_INPUT_QTY") == true)
                        dgLotList.Columns["PRE_PROC_INPUT_QTY"].Visibility = Visibility.Collapsed;
                    if (dgLotList?.Columns?.Contains("EQPT_INPUT_QTY") == true)
                        dgLotList.Columns["EQPT_INPUT_QTY"].Visibility = Visibility.Collapsed;
                    
                    dgLotList.AlternatingRowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF9F9F9"));
                    dgLotList.Rows[dgLotList.Rows.Count - 1].Visibility = Visibility.Collapsed;                    
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region [E20240502-001076] Mixer 원재료 Tracking 기능 개선 : Main 믹서/Sub 믹서 생산실적 조회 화면 표준화 요청건
        private void SetVisibleObject_MixResultStd(string sProcID)
        {

            if (sProcID.Left(1).Equals("E"))
            {
                // [E20240607-001447] : Mixer 원재료 Lot 투입 이력 개선 요청건 -> 절연액Mixing과 DAM Mixing도 투입자재Tab 활성화
                if (sProcID.Equals(Process.BS) || sProcID.Equals(Process.CMC) || sProcID.Equals(Process.PRE_MIXING) || sProcID.Equals(Process.MIXING) ||
                    sProcID.Equals(Process.InsulationMixing) || sProcID.Equals(Process.DAM_MIXING))
                {
                    cTabInputMaterial.Visibility = Visibility.Visible;
                    dgInputHist.Columns["EQPT_MOUNT_PSTN_NAME"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["HOPPER_ID"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["REQ_ID"].Visibility = Visibility.Visible;
                }
                else if (sProcID.Equals(Process.COATING))
                {
                    cTabInputMaterial.Visibility = Visibility.Visible;
                    dgInputHist.Columns["EQPT_MOUNT_PSTN_NAME"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["HOPPER_ID"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["REQ_ID"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    cTabInputMaterial.Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["EQPT_MOUNT_PSTN_NAME"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["HOPPER_ID"].Visibility = Visibility.Collapsed;
                    dgInputHist.Columns["REQ_ID"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                cTabInputMaterial.Visibility = Visibility.Collapsed;
                dgInputHist.Columns["EQPT_MOUNT_PSTN_NAME"].Visibility = Visibility.Visible;
                dgInputHist.Columns["HOPPER_ID"].Visibility = Visibility.Collapsed;
                dgInputHist.Columns["REQ_ID"].Visibility = Visibility.Collapsed;

            }

        }
        #endregion
    }
}