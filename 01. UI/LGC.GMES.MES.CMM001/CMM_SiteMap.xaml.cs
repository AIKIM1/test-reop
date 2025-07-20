/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_SiteMap : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable dtSiteMap_MES = new DataTable();
        DataTable dtSiteMap_MMD = new DataTable();
        DataRow newRow = null;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_SiteMap()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            Initialize_MES();
            Initialize_MMD();
        }

        private void Initialize_MES()
        {
            dtSiteMap_MES = new DataTable();
            dtSiteMap_MES.Columns.Add("MENUNAME", typeof(string));
            dtSiteMap_MES.Columns.Add("FORMID", typeof(string));
            dtSiteMap_MES.Columns.Add("NAMESPACE", typeof(string));
            dtSiteMap_MES.Columns.Add("CHECKEND", typeof(string));
            dtSiteMap_MES.Columns.Add("PROGEND", typeof(string));
            dtSiteMap_MES.Columns.Add("ACCESS_FLAG", typeof(string));
            dtSiteMap_MES.Columns.Add("USER", typeof(string));

            List<object[]> menulist_MES = new List<object[]>();

            #region MES DataRow Add =================================================================================================
            menulist_MES.Add(new object[] { "공정 외관검사 입력", "ASSY001_033", "LGC.GMES.MES.ASSY001", "2017-05-23", "", "W", "고현영" });
            menulist_MES.Add(new object[] { "SoundTest", "BOX001_000", "LGC.GMES.MES.BOX001", "2016-11-18", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "공정진척(전극)_롤프레스 공정진척", "ELEC001_BAS_010", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(전극)_테이핑 공정진척", "ELEC001_BAS_017", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공정진척(전극)_리와인더 공정진척", "ELEC001_BAS_012", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(전극)_백와인더 공정진척", "ELEC001_BAS_013", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "SRS 믹서 공정진척", "ELEC001_016", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "SRS이송탱크", "ELEC001_001", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "SRS 코터 공정진척", "ELEC001_107", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "SRS 슬리터 공정진척", "ELEC001_114", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "선분산 믹서 공정진척", "ELEC001_005", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            //************************************************************************************************************************************
            menulist_MES.Add(new object[] { "공정진척(전극)_코터 공정진척(단면코팅)", "PGM_GUI_014", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(전극)_믹서 공정진척(NON BASEFORM)", "ELEC001_NON_BAS_006", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(전극)_코터 공정진척(NON BASEFORM)", "ELEC001_NON_BAS_007", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(전극)_슬리터 공정진척(NON BASEFORM)", "ELEC001_NON_BAS_014", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "SRS 믹서 공정진척(NON BASEFORM)", "ELEC001_NON_BAS_016", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "SRS 코터 공정진척(NON BASEFORM)", "ELEC001_NON_BAS_107", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "SRS 슬리터 공정진척(NON BASEFORM)", "ELEC001_NON_BAS_114", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "테이핑 공정진척(NON BASEFORM)", "ELEC001_017", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "리와인더 공정진척(NON BASEFORM)", "ELEC001_012", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "백와인더 공정진척(NON BASEFORM)", "ELEC001_013", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "절연코터 공정진척", "PGM_GUI_205", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척믹서(BATCH ORDERM)", "ELEC001_NON_BAS_006", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "생산계획_생산계획 조회", "PGM_GUI_001", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "생산계획_생산계획 조정", "PGM_GUI_002", "LGC.GMES.MES.ProtoType04", "2016-07-27", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "생산계획_작업지시", "PGM_GUI_003", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "생산계획_작업지시_마감", "PGM_GUI_004", "LGC.GMES.MES.ProtoType04", "2016-07-27", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "ERP 이전전기 확인", "COM001_030", "LGC.GMES.MES.COM001", "2016-08-08", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(SRS)_SRS포장출고", "PGM_GUI_006", "LGC.GMES.MES.BOX001", "2016-08-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(전극)_믹서원자재 투입요청서", "PGM_GUI_007", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(전극)_믹서원자재 수동투입", "PGM_GUI_008", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(전극)_믹서원자재 Batch 추적", "PGM_GUI_009", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(전극)_믹서원자재 투입이력 조회", "PGM_GUI_010", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(전극)_LOT 예약", "PGM_GUI_011", "LGC.GMES.MES.ProtoType04", "2016-08-12", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "LOT 예약(노칭)", "PGM_GUI_198", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(전극)_슬리터 공정진척_TEST", "ELEC001_014", "LGC.GMES.MES.ELEC001", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(전극)_하프 슬리터 공정진척", "PGM_GUI_020", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(조립)_노칭 공정진척", "ASSY001_001", "LGC.GMES.MES.ASSY001", "2016-08-04", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "공정진척(조립)_V/D 공정진척", "ASSY001_002", "LGC.GMES.MES.ASSY001", "2016-08-09", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공정진척(조립)_V/D QA 대상 LOT조회", "ASSY001_003", "LGC.GMES.MES.ASSY001", "2016-08-05", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(조립)_라미 공정진척", "ASSY001_004", "LGC.GMES.MES.ASSY001", "2016-08-01", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "공정진척(조립)_폴딩 공정진척", "ASSY001_005", "LGC.GMES.MES.ASSY001", "2016-08-08", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "공정진척(조립)_패키징 공정진척", "ASSY001_007", "LGC.GMES.MES.ASSY001", "2016-08-04", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "특이작업_투입LOT 종료 취소(노칭)", "ASSY001_021", "LGC.GMES.MES.ASSY001", "2016-08-04", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "공정진척(조립)_와인더 공정진척", "ASSY002_001", "LGC.GMES.MES.ASSY002", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(조립)_Ass'y 공정진척", "ASSY002_002", "LGC.GMES.MES.ASSY002", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(조립)_와싱 공정진척", "ASSY002_003", "LGC.GMES.MES.ASSY002", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(조립)_SRC 공정진척", "PGM_GUI_032", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "공정진척(조립)_STP 공정진척", "PGM_GUI_033", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "공정진척(조립)_SSC Bi_cell 공정진척", "PGM_GUI_034", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "공정진척(조립)_SSC Folded_cell 공정진척", "PGM_GUI_035", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_그레이더 실적등록", "PGM_GUI_036", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_디가스 공정진척", "PGM_GUI_037", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_특성측정기 공정진척", "PGM_GUI_038", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_Baking 공정진척", "PGM_GUI_039", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_TCO 공정진척", "PGM_GUI_040", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_DSF 투입전 재공관리", "PGM_GUI_041", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_DSF 공정진척", "PGM_GUI_042", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_DSF 불량등록", "PGM_GUI_043", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_DSF 양품등록", "PGM_GUI_044", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_DSF 확대경 검사", "PGM_GUI_045", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "공정진척(활성화)_TCO ETC", "PGM_GUI_046", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "백광영" });
            menulist_MES.Add(new object[] { "공정진척(포장)_포장 공정진척", "PGM_GUI_047", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(포장)_포장 실적보고(개별Box 구성)", "PGM_GUI_048", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(포장)_포장 실적보고(Pallet 수동 구성)", "PGM_GUI_049", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(포장)_CELL 교체 처리", "PGM_GUI_050", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(포장)_OutBox포장", "PGM_GUI_051", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이동렬" });
            menulist_MES.Add(new object[] { "공정진척(포장)_QA 출하검사 의뢰", "PGM_GUI_052", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "공정진척(포장)_반품 PALLET 조회", "PGM_GUI_056", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "이동렬" });
            menulist_MES.Add(new object[] { "공정진척(포장)_기간별 Pallet 확정 이력 정보 조회", "PGM_GUI_057", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "공정진척(자동포장)_재발행(신형)", "PGM_GUI_058", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공정진척(자동포장)_재발행(구형)", "PGM_GUI_059", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공정진척(자동포장)_SBD InBox", "PGM_GUI_060", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공정진척(자동포장)_TTI InBox", "PGM_GUI_061", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공정진척(자동포장)_수동인쇄", "PGM_GUI_062", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공정진척(자동포장)_자동인쇄", "PGM_GUI_063", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_Mophie Label", "PGM_GUI_064", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_SBD Label", "PGM_GUI_065", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_TTI Label", "PGM_GUI_066", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_Motorola Mat Label", "PGM_GUI_067", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_BOSH Shipping note", "PGM_GUI_068", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_BOSH Mat Label", "PGM_GUI_069", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_LG전자 PALLET 라벨 발행", "ASSY001_012", "LGC.GMES.MES.ASSY001", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_HLGP PALLET 라벨 발행", "ASSY001_013", "LGC.GMES.MES.ASSY001", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_FORD PALLET 라벨 발행", "ASSY001_014", "LGC.GMES.MES.ASSY001", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_GM PALLET 라벨 발행", "ASSY001_015", "LGC.GMES.MES.ASSY001", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "라벨발행_Daimler PALLET 라벨 발행", "ASSY001_016", "LGC.GMES.MES.ASSY001", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "작업지시선택 - PACK", "PACK001_000", "LGC.GMES.MES.PACK001", "2016-08-03", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_Pack 공정진척", "PACK001_001", "LGC.GMES.MES.PACK001", "2016-08-03", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_LOT 이력", "PACK001_075", "LGC.GMES.MES.PACK001", "2016-08-05", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_Pack 수리/재작업 공정", "PACK001_004", "LGC.GMES.MES.PACK001", "2016-08-05", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_Pack 폐기 공정", "PACK001_023", "LGC.GMES.MES.PACK001", "", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_Pack 공정진척(자동발행)", "PACK001_002", "LGC.GMES.MES.PACK001", "2016-08-08", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_PILOT공정 일괄 완공", "PGM_GUI_079", "LGC.GMES.MES.ProtoType04", "2016-08-10", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_Pack 포장 BOX ID 발행", "PGM_GUI_080", "LGC.GMES.MES.ProtoType04", "2016-08-11", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_Pack 포장 박스라벨 발행", "PGM_GUI_081", "LGC.GMES.MES.ProtoType04", "2016-08-16", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_자동 Pack 포장2", "PACK001_003", "LGC.GMES.MES.PACK001", "2016-08-08", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "Pack 반품", "PACK001_024", "LGC.GMES.MES.PACK001", "2016-08-08", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "Pack 반품 재작업", "PACK001_025", "LGC.GMES.MES.PACK001", "2016-08-08", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "QA 출하검사 의뢰(Pack)", "PACK001_026", "LGC.GMES.MES.PACK001", "2016-08-08", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_315H CMA포장", "PGM_GUI_083", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            //menulist_MES.Add(new object[] { "공정진척(Pack)_SAIC Label 발행",                       "PGM_GUI_084", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "공정진척(Pack)_SMA Box 바코드 발행",                   "PGM_GUI_085", "LGC.GMES.MES.ProtoType04" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_Porshe 바코드 발행", "PGM_GUI_086", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_9호기 Hv Test Report", "PGM_GUI_087", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_일괄 폐기 처리", "PGM_GUI_088", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack)_불량유형변경", "PGM_GUI_089", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack 포장)_BMA 포장 관리", "PGM_GUI_090", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack 포장)_수동 Pack 포장", "PGM_GUI_091", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            menulist_MES.Add(new object[] { "공정진척(Pack 포장)_CMA 포장 관리", "PGM_GUI_092", "LGC.GMES.MES.ProtoType04" });
            menulist_MES.Add(new object[] { "공정진척(Pack 포장)_포장 관리(CNA Pack)", "PGM_GUI_093", "LGC.GMES.MES.ProtoType04" });
            menulist_MES.Add(new object[] { "공정진척(Pack 포장)_포장 이력 조회", "PGM_GUI_094", "LGC.GMES.MES.ProtoType04", "2016-09-09", "", "W", "장만철" });
            //menulist_MES.Add(new object[] { "작업일지_바인더 작업일지",                             "PGM_GUI_095", "LGC.GMES.MES.ProtoType04", "          ", "W", "박상철" });
            //menulist_MES.Add(new object[] { "작업일지_CMC 작업일지",                                "PGM_GUI_096", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_믹서 작업일지",                               "PGM_GUI_097", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_코터 작업일지",                               "PGM_GUI_098", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_롤프레스 작업일지",                           "PGM_GUI_099", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_재와인더 작업일지",                           "PGM_GUI_100", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_백와인더 작업일지",                           "PGM_GUI_101", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_테이핑 작업일지",                             "PGM_GUI_102", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_하프슬리터 작업일지",                         "PGM_GUI_103", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_슬리터 작업일지",                             "PGM_GUI_104", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_노칭 작업일지",                               "PGM_GUI_105", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_라미 작업일지",                               "PGM_GUI_106", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_폴딩 작업일지",                               "PGM_GUI_107", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_패키징 작업일지",                             "PGM_GUI_108", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_와인더/각형) 작업일지",                       "PGM_GUI_109", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_와인더(원형) 품질체크",                       "PGM_GUI_110", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_와인더(각형) 품질체크",                       "PGM_GUI_111", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_Ass'y(원/각형) 작업일지",                     "PGM_GUI_112", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_Ass'y(원/각형) 작업보고서",                   "PGM_GUI_113", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_Ass'y(각형) 품질체크",                        "PGM_GUI_114", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_와싱(원형) 작업일지",                         "PGM_GUI_115", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_패키징 품질체크",                             "PGM_GUI_116", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_와싱(원형) 작업보고서",                       "PGM_GUI_117", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_포장 작업일지",                               "PGM_GUI_118", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_C&B 작업일지",                                "PGM_GUI_119", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_활성화 작업일지",                             "PGM_GUI_120", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_디가스 작업일지",                             "PGM_GUI_121", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "작업일지_작업일지 모니터링",                           "PGM_GUI_122", "LGC.GMES.MES.ProtoType04", "          ", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_Lot 홀딩/해제", "COM001_006", "LGC.GMES.MES.COM001", "2016-07-27", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_Lot 폐기", "PGM_GUI_124", "LGC.GMES.MES.ProtoType04", "2016-08-10", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_Lot 정보 변경", "COM001_008", "LGC.GMES.MES.COM001", "2016-07-27", "", "W", "박상철" });
            //menulist_MES.Add(new object[] { "특이작업_Lot 이력 관리(Pack)",                         "PGM_GUI_126", "LGC.GMES.MES.ProtoType04", "          ", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_공정별 생산실적 수정", "COM001_009", "LGC.GMES.MES.COM001", "2016-08-11", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_SubLot 라인 이동[라미~패키징]", "PGM_GUI_128", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "특이작업_투입Lot 종료 취소[노칭]", "PGM_GUI_129", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "특이작업_DSF 대차관리", "PGM_GUI_130", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_공TRAY 입고 설정", "COM001_010", "LGC.GMES.MES.COM001", "2016-07-27", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_재고실사", "COM001_011", "LGC.GMES.MES.COM001", "2016-07-27", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "특이작업_근무자그룹-근무자 Mapping", "COM001_061", "LGC.GMES.MES.COM001", "2017-01-14", "", "", "김지은" });
            //menulist_MES.Add(new object[] { "특이작업_전산/실사 재고비교",                          "PGM_GUI_133", "LGC.GMES.MES.ProtoType04" });
            menulist_MES.Add(new object[] { "설비Loss_설비LOSS등록", "COM001_014", "LGC.GMES.MES.COM001", "2016-07-27", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "설비Loss_설비LOSS현황", "COM001_015", "LGC.GMES.MES.COM001", "2016-07-27", "", "2016-08-24", "박상철" });
            menulist_MES.Add(new object[] { "정보조회_Lot 정보 조회", "COM001_016", "LGC.GMES.MES.COM001", "2016-07-27", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "정보조회_Cell 이력 조회", "PGM_GUI_137", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            //menulist_MES.Add(new object[] { "정보조회_Lot별 생산이력 정보 조회",                    "PGM_GUI_138", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "정보조회_실적 조회",                                   "PGM_GUI_139", "LGC.GMES.MES.ProtoType04" });
            menulist_MES.Add(new object[] { "정보조회_생산실적 변경 이력 조회", "PGM_GUI_140", "LGC.GMES.MES.ProtoType04", "2016-07-27", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "정보조회_EIF Log 조회", "PGM_GUI_141", "LGC.GMES.MES.ProtoType04", "2016-08-08", "", "W", "정현식" });
            //menulist_MES.Add(new object[] { "정보조회_제품별검사값조회 ",                           "PGM_GUI_142", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "입출고관리_공정창고 위치관리",                         "PGM_GUI_143", "LGC.GMES.MES.ProtoType04" });
            menulist_MES.Add(new object[] { "입출고관리_공정 창고 모니터링", "COM001_025", "LGC.GMES.MES.COM001", "2016-08-09", "", "W", "이동렬" });
            menulist_MES.Add(new object[] { "입출고관리_Pancake 입고", "PGM_GUI_145", "LGC.GMES.MES.ProtoType04", "2016-08-05", "", "W", "김동일" });
            menulist_MES.Add(new object[] { "입출고관리_점보롤 입고", "PGM_GUI_146", "LGC.GMES.MES.ProtoType04", "", "", "이하나" });
            //menulist_MES.Add(new object[] { "입출고관리_공정창고 입고",                             "PGM_GUI_147", "LGC.GMES.MES.ProtoType04" });
            //menulist_MES.Add(new object[] { "입출고관리_공정창고 출고",                             "PGM_GUI_148", "LGC.GMES.MES.ProtoType04" });
            menulist_MES.Add(new object[] { "입출고관리_전극창고 재고조회", "PGM_GUI_149", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "이동렬" });
            menulist_MES.Add(new object[] { "입출고관리_자재 입고(Pack)", "PACK001_019", "LGC.GMES.MES.PACK001", "2016-08-09", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "입출고관리_Cell등록", "PACK001_020", "LGC.GMES.MES.PACK001", "          ", "", "W", "정현식" });
            menulist_MES.Add(new object[] { "창고이동_전극창고 출고", "PGM_GUI_152", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이동렬" });
            menulist_MES.Add(new object[] { "창고이동_전극창고 입고", "PGM_GUI_153", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이동렬" });
            menulist_MES.Add(new object[] { "PDA(선분산)_원재료 투입", "PGM_GUI_156", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "PDA(선분산)_탱크 ID 확인", "PGM_GUI_157", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "PDA_SRS포장팔레트 구성", "PGM_GUI_158", "LGC.GMES.MES.ProtoType04", "2016-08-09", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_믹서원자재 투입요청서조회", "PGM_GUI_159", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_믹서원자재 투입", "PGM_GUI_160", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(믹서원자재)_믹서원자재 투입보류", "PGM_GUI_161", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_LOT 예약", "PGM_GUI_162", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_LOT 예약 취소", "PGM_GUI_163", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_V/D 실적확정", "PGM_GUI_164", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PD(재고실사)_재고실사(PDA)", "PGM_GUI_165", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_DSF 투입전 재공 조사", "PGM_GUI_166", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_Pancake 입고", "PGM_GUI_167", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_창고내 Skid 변경", "PGM_GUI_168", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_점보롤 입고", "PGM_GUI_169", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_점보롤 입고 취소", "PGM_GUI_170", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA_전극창고 출고", "PGM_GUI_171", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(창고이동)_창고이동(출고)", "PGM_GUI_172", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(창고이동)_전극창고 입고", "PGM_GUI_173", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(창고이동)_창고이동(입고)", "PGM_GUI_174", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(환경설정)_기본설정", "PGM_GUI_175", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(환경설정)_로그인(PDA)", "PGM_GUI_176", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(포장재)_포장재 폐기", "PGM_GUI_177", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(포장재)_Pallet Write(포장)", "PGM_GUI_178", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "PDA(포장재)_Tray Write(포장)", "PGM_GUI_179", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "PDA(포장)_Cell 정보 조회(포장)", "PGM_GUI_180", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "PDA(포장)_불량 Cell 교체", "PGM_GUI_181", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "PDA(포장)_불량 Cell 이력 조회", "PGM_GUI_182", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(포장)_Tag 정보 변경", "PGM_GUI_183", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(포장)_Tray 체결", "PGM_GUI_184", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "임종훈" });
            menulist_MES.Add(new object[] { "PDA(DSF)_대차 인수인계", "PGM_GUI_185", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "PDA(DSF)_대차 인수", "PGM_GUI_186", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이하나" });
            menulist_MES.Add(new object[] { "시스템_로그인", "PGM_GUI_187", "LGC.GMES.MES.ProtoType04", "", "", "", "정규성" });
            menulist_MES.Add(new object[] { "시스템_공지사항", "PGM_GUI_188", "LGC.GMES.MES.ProtoType04", "", "", "", "정규성" });
            menulist_MES.Add(new object[] { "시스템_매뉴얼", "PGM_GUI_189", "LGC.GMES.MES.ProtoType04", "", "", "", "정규성" });
            menulist_MES.Add(new object[] { "시스템_MES 비상연락망", "PGM_GUI_190", "LGC.GMES.MES.ProtoType04", "", "", "", "정규성" });
            menulist_MES.Add(new object[] { "시스템_환경설정", "PGM_GUI_191", "LGC.GMES.MES.ProtoType04", "", "", "", "정규성" });
            menulist_MES.Add(new object[] { "시스템_포트설정", "PGM_GUI_192", "LGC.GMES.MES.ProtoType04", "", "", "", "정규성" });
            menulist_MES.Add(new object[] { "시스템_PC관리", "PGM_GUI_193", "LGC.GMES.MES.ProtoType04", "", "", "", "정규성" });
            menulist_MES.Add(new object[] { "ERP 전송확인", "COM001_029", "LGC.GMES.MES.COM001", "2016-08-09", "W", "", "박상철" });
            menulist_MES.Add(new object[] { "공정모니터링", "COM001_031", "LGC.GMES.MES.COM001", "2016-08-09", "W", "", "박상철" });
            menulist_MES.Add(new object[] { "폴딩(인계)", "ASSY001_020", "LGC.GMES.MES.ASSY001", "2016-08-09", "W", "", "박상철" });
            menulist_MES.Add(new object[] { "공정진척(조립)_Stacking 공정진척", "ASSY001_006", "LGC.GMES.MES.ASSY001", "", "W", "", "김동일" });
            menulist_MES.Add(new object[] { "절연코터 공정진척", "PGM_GUI_205", "LGC.GMES.MES.ProtoType04", "2016-08-08", "W", "", "백광영" });
            menulist_MES.Add(new object[] { "스펙 및 제어값 조회", "PGM_GUI_206", "LGC.GMES.MES.ProtoType04", "2016-09-09", "W", "", "장만철" });
            menulist_MES.Add(new object[] { "Cell 홀딩/해제", "PGM_GUI_207", "LGC.GMES.MES.ProtoType04", "2016-09-09", "W", "", "장만철" });
            menulist_MES.Add(new object[] { "월력 관리", "COM001_033", "LGC.GMES.MES.COM001", "          ", "W", "", "정현식" });
            menulist_MES.Add(new object[] { "공정 자주검사", "COM001_034", "LGC.GMES.MES.COM001", "          ", "W", "", "박상철" });
            menulist_MES.Add(new object[] { "재공조회", "PGM_GUI_211", "LGC.GMES.MES.ProtoType04", "2016-08-09", "W", "", "정현식" });
            menulist_MES.Add(new object[] { "Lot 정보 조회", "PGM_GUI_212", "LGC.GMES.MES.ProtoType04", "2016-08-09", "W", "", "정현식" });
            menulist_MES.Add(new object[] { "불량 자재 처리", "PGM_GUI_213", "LGC.GMES.MES.ProtoType04", "2016-09-09", "W", "", "장만철" });
            menulist_MES.Add(new object[] { "Cell 반품", "PGM_GUI_214", "LGC.GMES.MES.ProtoType04", "          ", "W", "", "정현식" });
            menulist_MES.Add(new object[] { "정보조회_재공현황 조회", "PGM_GUI_300", "LGC.GMES.MES.ProtoType04", "          ", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "승인요청목록", "COM001_035", "LGC.GMES.MES.COM001", "", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "결재목록", "COM001_036", "LGC.GMES.MES.COM001", "", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "공정진척(전극)_NISSAN향 RANID저장", "BOX001_002", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "공정진척(전극)_NISSAN향 프린트 이력", "BOX001_003", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "포장(전극)_Nissan향 Box 포장", "BOX001_004", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "VD예약", "COM001_005", "LGC.GMES.MES.COM001", "2016-10-08", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "믹서생산계획", "COM001_003", "LGC.GMES.MES.COM001", "2017-01-18", "", "", "김지은" });
            menulist_MES.Add(new object[] { "공정진척(포장)_포장 출고 및 이력 조회", "BOX001_011", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "공정진척(포장)_포장 출고 취소 및 이력 조회(WMS)", "BOX001_012", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "공정진척(포장)_포장 재작업 / 이력 조회  및 복구", "BOX001_013", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "포장(전극)_전극 포장 및 출고", "BOX001_016", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "동간이동(인계)", "COM001_027", "LGC.GMES.MES.COM001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "동간이동(인수)", "COM001_028", "LGC.GMES.MES.COM001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "생산계획_동간이동 계획 조회", "COM001_038", "LGC.GMES.MES.COM001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "SHOP간이동(인수)", "COM001_039", "LGC.GMES.MES.COM001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "SHOP간이동(인계)", "COM001_040", "LGC.GMES.MES.COM001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "CELL 반품", "BOX001_017", "LGC.GMES.MES.BOX001", "2016-08-09", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "대차 모니터링", "COM001_032", "LGC.GMES.MES.COM001", "2016-09-22", "", "W", "김광호" });
            menulist_MES.Add(new object[] { "믹서실적 조회", "COM001_044", "LGC.GMES.MES.COM001", "2016-11-16", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "공정별 생산실적 조회", "COM001_045", "LGC.GMES.MES.COM001", "2016-11-16", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "믹서원자재 라벨발행", "ELEC001_019", "LGC.GMES.MES.ELEC001", "2016-11-18", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "믹서원자재 자재LOT 입력", "ELEC001_020", "LGC.GMES.MES.ELEC001", "2016-11-18", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "믹서원자재 LOT 추적", "ELEC001_021", "LGC.GMES.MES.ELEC001", "2016-11-18", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "전극 반품 확정", "BOX001_025", "LGC.GMES.MES.BOX001", "2016-11-18", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "포장 개별 구성", "BOX001_026", "LGC.GMES.MES.BOX001", "2017-01-24", "", "W", "이슬아" });
            menulist_MES.Add(new object[] { "재와인더", "ELEC001_115", "LGC.GMES.MES.ELEC001", "2016-09-28", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "재공현황조회", "COM001_018", "LGC.GMES.MES.COM001", "2016-11-08", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "테스트모드관리", "COM001_042", "LGC.GMES.MES.COM001", "2016-11-08", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "생산실적 변경 이력조회", "COM001_019", "LGC.GMES.MES.COM001", "2016-11-08", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "외주전극", "BOX001_023", "LGC.GMES.MES.BOX001", "2016-11-18", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "설비 별 투입자재 조회", "COM001_059", "LGC.GMES.MES.COM001", "2016-11-21", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "ERP실적확인2", "COM001_046", "LGC.GMES.MES.COM001", "2016-11-21", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "게시판", "COM001_047", "LGC.GMES.MES.COM001", "2016-11-21", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "공지사항", "COM001_048", "LGC.GMES.MES.COM001", "2016-11-21", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "실적조회PIVOT", "COM001_049", "LGC.GMES.MES.COM001", "2016-11-21", "", "W", "이성식" });
            menulist_MES.Add(new object[] { "합권취", "COM001_043", "LGC.GMES.MES.COM001", "2016-12-12", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "라미이송", "ASSY001_022", "LGC.GMES.MES.ASSY001", "2016-12-19", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "VD재작업", "ASSY001_023", "LGC.GMES.MES.ASSY001", "2016-12-19", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "N5출고", "BOX001_024", "LGC.GMES.MES.BOX001", "2016-12-26", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "ERP 전송확인", "COM001_050", "LGC.GMES.MES.COM001", "2016-01-04", "", "", "이성식" });
            menulist_MES.Add(new object[] { "VDQA대상LOT조회수정", "ASSY001_024", "LGC.GMES.MES.ASSY001", "2017-01-13", "", "", "이진선" });
            menulist_MES.Add(new object[] { "선분산믹서저장탱크구성", "COM001_051", "LGC.GMES.MES.COM001", "2017-01-17", "", "", "이동렬" });
            menulist_MES.Add(new object[] { "선분산포장출고", "COM001_052", "LGC.GMES.MES.COM001", "2017-01-19", "", "", "이진선" });
            menulist_MES.Add(new object[] { "전극입고", "BOX001_022", "LGC.GMES.MES.BOX001", "2017-01-24", "", "W", "이동렬" });
            menulist_MES.Add(new object[] { "자주검사 조회", "COM001_053", "LGC.GMES.MES.COM001", "2016-11-16", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "라미이송(수정)", "ASSY001_025", "LGC.GMES.MES.ASSY001", "2017-01-25", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "전극이력카드", "ELEC001_005", "LGC.GMES.MES.ELEC001", "2017-02-01", "", "W", "박상철" });
            menulist_MES.Add(new object[] { "선분산출고(동간이동로직)", "COM001_066", "LGC.GMES.MES.COM001", "2017-02-01", "", "W", "이진선" });
            menulist_MES.Add(new object[] { "LGCWA 입출고관리_공정 창고 모니터링", "COM001_073", "LGC.GMES.MES.COM001", "2017-04-17", "", "W", "윤세진" });
            menulist_MES.Add(new object[] { "LGCWA V/D설비예약", "ASSY001_031", "LGC.GMES.MES.ASSY001", "2017-04-17", "", "W", "윤세진" });
            menulist_MES.Add(new object[] { "V/D QA 대상 LOT조회", "ASSY001_032", "LGC.GMES.MES.ASSY001", "2017-04-17", "", "W", "윤세진" });
            menulist_MES.Add(new object[] { "Grader", "ASSY001_034", "LGC.GMES.MES.ASSY001", "2017-06-29", "", "W", "오화백" });
            menulist_MES.Add(new object[] { "초소형 그룹관리", "COM001_087", "LGC.GMES.MES.COM001", "2017-06-29", "", "W", "오화백" });
            menulist_MES.Add(new object[] { "등외품 Lot 등록", "COM001_092", "LGC.GMES.MES.COM001", "2017-07-17", "", "W", "오화백" });
            menulist_MES.Add(new object[] { "재작업 대상 재공 이동", "COM001_093", "LGC.GMES.MES.COM001", "2017-07-19", "", "W", "오화백" });
            menulist_MES.Add(new object[] { "Pallet 병합/분할/수량변경", "COM001_094", "LGC.GMES.MES.COM001", "2017-07-24", "", "W", "오화백" });
            menulist_MES.Add(new object[] { "QMS 검사 의뢰", "COM001_095", "LGC.GMES.MES.COM001", "2017-07-24", "", "W", "오화백" });
            //COM001_060
            #endregion

            foreach (object[] item in menulist_MES)
            {
                newRow = dtSiteMap_MES.NewRow();
                newRow.ItemArray = item;
                dtSiteMap_MES.Rows.Add(newRow);
            }

            dgSiteMap_MES.ItemsSource = DataTableConverter.Convert(dtSiteMap_MES);
        }

        private void Initialize_MMD()
        {
            dtSiteMap_MMD = new DataTable();
            dtSiteMap_MMD.Columns.Add("MENUNAME", typeof(string));
            dtSiteMap_MMD.Columns.Add("FORMID", typeof(string));
            dtSiteMap_MMD.Columns.Add("NAMESPACE", typeof(string));
            dtSiteMap_MMD.Columns.Add("CHECKEND", typeof(string));
            dtSiteMap_MMD.Columns.Add("ACCESS_FLAG", typeof(string));
            dtSiteMap_MMD.Columns.Add("USER", typeof(string));

            List<object[]> menulist_MMD = new List<object[]>();

            #region MES DataRow Add =================================================================================================

            menulist_MMD.Add(new object[] { "Basic Code", "Basic_Code", "LGC.GMES.MES.MMD001", "          ", "W", "이영준" });
            menulist_MMD.Add(new object[] { "Basic Code2", "Basic_Code2", "LGC.GMES.MES.MMD001", "        ", "W", "이영준" });
            menulist_MMD.Add(new object[] { "Plant", "PGM_MMD_A001", "LGC.GMES.MES.MMD001", "          ", "W", "이영준" });
            menulist_MMD.Add(new object[] { "공장 동(AREA)", "PGM_MMD_A002", "LGC.GMES.MES.MMD001", "          ", "W", "이영준" });
            #endregion

            foreach (object[] item in menulist_MMD)
            {
                newRow = dtSiteMap_MMD.NewRow();
                newRow.ItemArray = item;
                dtSiteMap_MMD.Rows.Add(newRow);
            }

            dgSiteMap_MMD.ItemsSource = DataTableConverter.Convert(dtSiteMap_MMD);
        }

        #endregion

        #region Event

        private void Run_Button_MES_Click(object sender, RoutedEventArgs e)
        {
            ExecuteOpenPage_MES(sender);
        }

        private void Run_Button_MMD_Click(object sender, RoutedEventArgs e)
        {
            ExecuteOpenPage_MMD(sender);
        }

        private void Image_Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(Convert.ToString(row.Row.ItemArray[0]), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        }

        #endregion

        #region Mehod

        private void ExecuteOpenPage_MES(object sender)
        {
            Button btn = sender as Button;

            C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
            System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
            foreach (var item in ilist)
            {
                C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                if (presenter != null)
                {
                    row = presenter.Row;
                }
            }
            dgSiteMap_MES.SelectedItem = row.DataItem;
            object selectedItem = dgSiteMap_MES.SelectedItem;
            FrameOperation.OpenDummyMenu(selectedItem);
        }

        private void ExecuteOpenPage_MMD(object sender)
        {
            Button btn = sender as Button;

            C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
            System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
            foreach (var item in ilist)
            {
                C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                if (presenter != null)
                {
                    row = presenter.Row;
                }
            }
            dgSiteMap_MMD.SelectedItem = row.DataItem;
            object selectedItem = dgSiteMap_MMD.SelectedItem;
            FrameOperation.OpenDummyMenu(selectedItem);
        }

        #endregion
    }
}