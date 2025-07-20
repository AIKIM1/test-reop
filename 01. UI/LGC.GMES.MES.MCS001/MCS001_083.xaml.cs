/*************************************************************************************
 Created Date : 2022.07.11
      Creator : 오화백
   Decription : STK 재공현황 LOT (신규공장 : NB(전극2, 조립2), GM, ESWA(전극 3, 조립4)
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.11  오화백 : Initial Created.    
  2023.04.07  오화백 : 창고 적재 현황 + 창고 재공현황 
  2023.05.24  오화백 : 폴란드 조립4동 Stacking 완성 창고일 경우  극성정보 파라미터는 Null로 넘기도록 수정
  2023.06.14  김대현 : ESNJ 9동 조립으로 접속했을때 동 콤보박스에 전극만 보이도록 수정
  2023.07.24  오화백 : 극성선택후  Loayout 초기화 문제 수정, 전체 집계 클릭시 극성 조건 추가 
  2023.09.14  주동석 : 공 Carrier 탭 팬케이크 창고일 경우 숨김 처리
  2023.10.23  오화백 : GMES 및 CIMS MISMACHING  문제로  금지단 제외 로직 주석 처리 및 금지단 탭 추가
  2023.12.15  김태우 : 와인더대기창고(WWW) 추가
  2024.04.01  배현우 : WWW, JTW 창고 적재 현황 수정,입고LOT, 공 Carrier REP_CSTID 컬럼 추가
  2025.03.20  조범모 : 라미 창고 집계 조회시 데이터 없어도 무한로딩 이미지 해제
  2025.04.10  조범모 : Layout 탭에 보빈ID 검색 기능 수정 (랙정보와 보빈ID 매핑소스의 주석제거)
  2025.04.25  오화백 : 라미 창고 현황 정보 나오지 않아서 수정 - 라인 752 
  2025.05.08  이민형 : 공 Carrierid 표시
  2025.05.09  이민형 : 창고 적재 현황 Grid 수정
  2025.05.19  이민형 : W/O관련 인자값 추가 된 부분 롤백처리
  2025.05.21  이민형 : 불량수량(NG MARK) 컬럼 추가 코드값 처리로 변경
  2025.06.12  이민형 : [MI2_OSS_313] 공Carrier 극성별로 분류해서 표시
  2025.06.12  이민형 : [MI2_OSS_342] ATTR4->ATTR5로 변경
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_029.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_083 : UserControl, IWorkArea
    {

    
        #region Declaration & Constructor 

    
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //private readonly 
        private readonly Util _util = new Util();
        private string _selectedEquipmentCode;
        private string _selectedLotElectrodeTypeCode; //제품에대한 극성정보 
        private string _selectedStkElectrodeTypeCode; //STK, 설비에 대한 극성정보
        private string _selectedProjectName;
        private string _selectedWipHold;
        private string _selectedQmsHold;
        private string _selectedLotIdByRackInfo;
        private string _selectedSkIdIdByRackInfo;
        private string _selectedBobbinIdByRackInfo;
        private string _ElectrodeType_Chk; // 조회조건 극성 선택여부 


        private string _selectedRackIdByRackInfo;
        private DataTable _dtWareHouseCapacity;

        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;
        private bool _isGradeJudgmentDisplay;
        private int _maxRowCount;
        private int _maxColumnCount;

        private DataTable _dtRackInfo;
        private UcRackLayout[][] _ucRackLayout1;
        private UcRackLayout[][] _ucRackLayout2;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        private bool _LotConf = false;

        private string sCurtrode = string.Empty;
                
        Util _Util = new Util();
        //스키드 사용여부
        private string _Skid_Use_Chk;
        private enum SearchType
        {
            Tab,
            MultiSelectionBox
        }
        /// <summary>
        /// 생성자
        /// </summary>
        public MCS001_083()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _isGradeJudgmentDisplay = IsGradeJudgmentDisplay();
        }
        /// <summary>
        /// 화면 로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeGrid();
            InitializeCombo();
            Chk_Skid_Use();
            MakeRackInfoTable();
            MakeWareHouseCapacityTable();
            TimerSetting();
            // 스태킹 완성 창고 여부
            if (LoginInfo.CFG_AREA_ID == "AC" || LoginInfo.CFG_AREA_ID == "EJ")
            {
                dgStatusbyWorkorder.Columns["QTY_AF_NT_SWW"].Visibility = Visibility.Visible;
                dgHold.Columns["STK_FOL_CNT"].Visibility = Visibility.Visible;
            }
            else
            {
                dgStatusbyWorkorder.Columns["QTY_AF_NT_SWW"].Visibility = Visibility.Collapsed;
                dgHold.Columns["STK_FOL_CNT"].Visibility = Visibility.Collapsed;
            }
            //HOLD LIST 탭 
            if (ChkHoldTabView())
            {
                tabHoldList.Visibility = Visibility.Visible;
            }
            //STO그룹별 적재수량 셋팅 탭
            if (ChkNNDLoadQtySettingTabView())
            {
                tabLoadQty.Visibility = Visibility.Visible;
            }


            Loaded -= UserControl_Loaded;
            C1TabControl.SelectionChanged += C1TabControl_SelectionChanged;

            if (cboStockerType.SelectedValue.GetString() == "NWW")
            {
                dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                dgCapacitySummary.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;

                if (ChkIQCView())
                {
                    // dgProductSummary.Columns["IQC_NG_QTY"].Visibility = Visibility.Visible;
                    dgProduct.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Visible;
                }
                dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;

                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }
                //STO그룹별 적재수량 셋팅 탭
                if (ChkNNDLoadQtySettingTabView())
                {
                    tabLoadQty.Visibility = Visibility.Visible;
                }

            }

            _isLoaded = true;
        }
        /// <summary>
        /// 화면 UNLOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
            _monitorTimer.IsEnabled = false; // 2024.11.06. 김영국 - 화면 종료 시 Timer의 동작 비활성화.
            _monitorTimer.Stop();
        }
        /// <summary>
        /// 콤보초기화
        /// </summary>
        private void InitializeCombo()
        {
            // Area 콤보박스
            SetAreaCombo(cboArea);

            // 창고유형 콤보박스
            SetStockerTypeCombo(cboStockerType);

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);


            //라인아이디 콤보
            SetDataGridEqsgidCombo(dgLineLoadQty.Columns["EQPTID"]);

            //사용 여부 Set
            SetDataGridUseFlagCombo(dgLineLoadQty.Columns["USE_FLAG"]);
        }

        /// <summary>
        /// RACK 정보 DataTable
        /// </summary>
        private void MakeRackInfoTable()
        {
            _dtRackInfo = new DataTable();
            _dtRackInfo.Columns.Add("RACK_ID", typeof(string));
            _dtRackInfo.Columns.Add("STATUS", typeof(string));
            _dtRackInfo.Columns.Add("PRJT_NAME", typeof(string));
            _dtRackInfo.Columns.Add("LOTID", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTPROD", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTPROD_NAME", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTID", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTPROD", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTPROD_NAME", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTID", typeof(string));
            _dtRackInfo.Columns.Add("WIPHOLD", typeof(string));
            _dtRackInfo.Columns.Add("CST_DFCT_FLAG", typeof(string));
            _dtRackInfo.Columns.Add("SKID_GUBUN", typeof(string));
            _dtRackInfo.Columns.Add("COLOR_GUBUN", typeof(string));
            _dtRackInfo.Columns.Add("COLOR", typeof(string));
            _dtRackInfo.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
            _dtRackInfo.Columns.Add("HOLD_FLAG", typeof(string));
            _dtRackInfo.Columns.Add("SEQ", typeof(int));
        }
        /// <summary>
        /// 창고재공현황 DataTable
        /// </summary>
        private void MakeWareHouseCapacityTable()
        {
            _dtWareHouseCapacity = new DataTable();
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_NAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTID", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTNAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("PRJT_NAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("RACK_MAX", typeof(decimal));          // 용량
            _dtWareHouseCapacity.Columns.Add("LOT_QTY", typeof(decimal));           // 가용수량
            _dtWareHouseCapacity.Columns.Add("LOT_HOLD_QTY", typeof(decimal));      // HOLD수량
            _dtWareHouseCapacity.Columns.Add("LOT_HOLD_QMS_QTY", typeof(decimal));  // QMS HOLD 수량
            _dtWareHouseCapacity.Columns.Add("BBN_U_QTY", typeof(decimal));     // 실Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_UM_QTY", typeof(decimal));    // 반대극성Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_E_QTY", typeof(decimal));     // 공Carrier수
            _dtWareHouseCapacity.Columns.Add("ERROR_QTY", typeof(decimal));     // 오류Carrier수
            _dtWareHouseCapacity.Columns.Add("ABNORM_QTY", typeof(decimal));    // 정보불일치수
            _dtWareHouseCapacity.Columns.Add("PROHIBIT_QTY", typeof(decimal));    // 금지단수
            _dtWareHouseCapacity.Columns.Add("RACK_RATE", typeof(double));      // 적재율
            _dtWareHouseCapacity.Columns.Add("RACK_QTY", typeof(decimal));      // 총Carrier수(실+공)
        }

        /// <summary>
        /// GRID 초기화
        /// </summary>
        private void InitializeGrid()
        {
            if (_isGradeJudgmentDisplay)
            {
                dgProduct.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
            }
            //FastTrack 적용여부 체크
            if (ChkFastTrackOWNER())
            {
                dgProduct.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
            }

        }

        #endregion

        #region Event

        #region 동정보 콤보 박스 이벤트 : cboArea_SelectedValueChanged()
        /// <summary>
        /// 동콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            SetStockerTypeCombo(cboStockerType);

            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }
        #endregion

        #region 창고 타입 콤보박스 이벤트 : cboStockerType_SelectedValueChanged()
        /// <summary>
        /// 창고타입 콤보박스 이벤트 - 창고 타입에 따라 컬럼 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
            //DisplayInit();
            //불량태그수
            dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
            //무지부, 권취방향
            dgProduct.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["ROLL_DIRECTION"].Visibility = Visibility.Collapsed;
            //생산설비(COATING) 
            dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Collapsed;
            //QA 검사결과, QA Hold 여부, QA 검사비고
            dgProduct.Columns["JUDG_TYPE"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Collapsed;
            //VD검사결과
            dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
            //IQC 검사결과
            dgProduct.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Collapsed;
            //투입LOT
            dgProduct.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
            //FAST TRACK
            dgProduct.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Collapsed;
            //극성
            dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;
            //SKID 
            dgProduct.Columns["SKID_ID"].Visibility = Visibility.Collapsed;
            dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Collapsed;

            //케리어 세정 여부, PET 권취여부
            dgCarrierList.Columns["CST_CLEAN_NAME"].Visibility = Visibility.Visible;
            dgCarrierList.Columns["PET_WINDING_CMPL_NAME"].Visibility = Visibility.Visible;

            // 집계 극성정보
            dgCapacitySummary.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;



            //EmptyCarrier 그룹
            dgEmptyCarrier.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;


            //스태킹 완성창고일 경우  컬럼명 변경
            dgProduct.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
            dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("보빈 ID");
            dgCarrierList.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("보빈 ID");
            dgCarrierList.Columns["CARRIERID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
            dgAbNormalCarrier.Columns["CARRIERID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");

            LeftAreaLami.Visibility = Visibility.Collapsed;
            LeftArea.Visibility = Visibility.Visible;
            tabStatusbyWorkorder.Visibility = Visibility.Collapsed;
            tabHoldList.Visibility = Visibility.Collapsed; //2021-05-19 OHB
            tabLoadQty.Visibility = Visibility.Collapsed;


            if (IsTabStatusbyWorkorderVisibility(SearchType.Tab, cboStockerType.SelectedValue.GetString()))
            {
                tabStatusbyWorkorder.Visibility = Visibility.Visible;
                //HOLD LIST 탭 
                if (ChkHoldTabView())
                {
                    tabHoldList.Visibility = Visibility.Visible;
                }
            }
            
            if(IsProcNGmarkVisibility(cboStockerType.SelectedValue.GetString()))
            {
                dgProduct.Columns["DFCT_QTY"].Visibility = Visibility.Visible;
            }
            else
            {
                dgProduct.Columns["DFCT_QTY"].Visibility = Visibility.Collapsed;
            }            

            tabEmptyCarrier.Visibility = Visibility.Visible;
            dgCapacitySummary.Columns["BBN_E_QTY"].Visibility = Visibility.Visible;

            _LotConf = false;

            // 노칭대기창고
            if (cboStockerType.SelectedValue.GetString() == "NWW")
            {
                dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;              
                dgCapacitySummary.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;

                if (ChkIQCView())
                {
                    // dgProductSummary.Columns["IQC_NG_QTY"].Visibility = Visibility.Visible;
                    dgProduct.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Visible;
                }
                dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;

                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }
                //STO그룹별 적재수량 셋팅 탭
                if (ChkNNDLoadQtySettingTabView())
                {
                    tabLoadQty.Visibility = Visibility.Visible;
                }

            }
            //노칭 완성창고
            else if (cboStockerType.SelectedValue.GetString() == "NPW")
            {                

                if (cboElectrodeType.Items.Count > 1)
                {
                    dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                    dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                    dgProduct.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                    dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;                   
                    dgCapacitySummary.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;


                }
                else
                {
                    dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                    dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                    dgProduct.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                    LeftAreaLami.Visibility = Visibility.Visible;
                    LeftArea.Visibility = Visibility.Collapsed;
                }

                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }

              
            }
            //라미 대기창고
            else if (cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                dgProduct.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
           
                LeftAreaLami.Visibility = Visibility.Visible;
                LeftArea.Visibility = Visibility.Collapsed;

                dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                if (ChkDefectTag())
                    dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                dgEmptyCarrier.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;

                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }

                dgLamiCapacitySummary.Columns["EMPTY_QTY"].Visibility = Visibility.Collapsed;
                dgLamiCapacitySummary.Columns["EMPTY_CST_QTY_N"].Visibility = Visibility.Collapsed; 
            }
            //스태킹 완성 창고
            else if (cboStockerType.SelectedValue.GetString() == "SWW")
            {
                dgProduct.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                dgEmptyCarrier.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;

                //스태킹 완성창고일 경우  컬럼명 변경
                dgProduct.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("Group Carrier ID");
                dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
          
                dgCarrierList.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                dgCarrierList.Columns["CARRIERID"].Header = ObjectDic.Instance.GetObjectName("Group Carrier ID");
                dgAbNormalCarrier.Columns["CARRIERID"].Header = ObjectDic.Instance.GetObjectName("Group Carrier ID");

                dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;

                dgCarrierList.Columns["CST_CLEAN_NAME"].Visibility = Visibility.Collapsed;
                dgCarrierList.Columns["PET_WINDING_CMPL_NAME"].Visibility = Visibility.Collapsed;
            }

            //점보롤 창고
            else if (cboStockerType.SelectedValue.GetString() == "JRW")
            {
                //LeftAreaLami.Visibility = Visibility.Visible; 
                //LeftArea.Visibility     = Visibility.Collapsed;

                dgProduct.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                dgProduct.Columns["ROLL_DIRECTION"].Visibility = Visibility.Visible;
                dgProduct.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgProduct.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgProduct.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
             
                dgCapacitySummary.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;


                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }

            }
            else if (cboStockerType.SelectedValue.GetString() == "PCW")
            {
                dgProduct.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgProduct.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
              
                dgCapacitySummary.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;


                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.Columns.Add("LANGID", typeof(string));
                RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

                DataRow row = RQSTDT1.NewRow();
                row["LANGID"] = LoginInfo.USERID;
                row["CMCDTYPE"] = "REP_LOT_USE_AREA";
                row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT1.Rows.Add(row);

                DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

                if (dtCommon != null)
                {
                    if (dtCommon.Rows.Count > 0)
                    {
                        // 주동석 팬케이크 창고일 경우 공 케리어 탭 숨김
                        tabEmptyCarrier.Visibility = Visibility.Collapsed;
                        dgCapacitySummary.Columns["BBN_E_QTY"].Visibility = Visibility.Collapsed;

                        _LotConf = true;
                        dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("대표 LOTID");
                        tblBobbond.Text = "대표 LOTID";
                    }
                    else
                    {
                        _LotConf = false;
                        dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("보빈 ID");
                        tblBobbond.Text = "보빈 ID";
                    }
                }
            }
            else if (cboStockerType.SelectedValue.GetString() == "WWW") //WND WAITED WAREHOUSE
            {
                LeftAreaLami.Visibility = Visibility.Visible;
                LeftArea.Visibility = Visibility.Collapsed;

                dgProduct.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgProduct.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                
                dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                dgProduct.Columns["BOBBIN_ID"].Visibility = Visibility.Collapsed;

                dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_C"].Visibility = Visibility.Visible;
                dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_A"].Visibility = Visibility.Visible;
                dgLamiCapacitySummary.Columns["RACK_QTY"].Visibility = Visibility.Visible;
                dgLamiCapacitySummary.Columns["EMPTY_QTY"].Visibility = Visibility.Collapsed;
                //dgLamiCapacitySummary.Columns["RACK_MAX"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("용량") }.ToList();
                //dgLamiCapacitySummary.Columns["ERROR_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("비정상 Rack") }.ToList();
                //dgLamiCapacitySummary.Columns["ABNORM_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("정보불일치수") }.ToList();
                //dgLamiCapacitySummary.Columns["PROHIBIT_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("금지단 수") }.ToList();
                //dgLamiCapacitySummary.Columns["RACK_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("사용중 Rack") }.ToList();
                //dgLamiCapacitySummary.Columns["RACK_RATE"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("적재율(%)") }.ToList();

                //dgLamiCapacitySummary.Columns["LOT_QTY_C"].Header = new string[] { ObjectDic.Instance.GetObjectName("양극 LOT"), ObjectDic.Instance.GetObjectName("양품") }.ToList();
                //dgLamiCapacitySummary.Columns["HOLD_QTY_C"].Header = new string[] { ObjectDic.Instance.GetObjectName("양극 LOT"), ObjectDic.Instance.GetObjectName("MES HOLD") }.ToList();
                //dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_C"].Header = new string[] { ObjectDic.Instance.GetObjectName("양극 LOT"), ObjectDic.Instance.GetObjectName("QMS HOLD") }.ToList();
                //dgLamiCapacitySummary.Columns["LOT_QTY_A"].Header = new string[] { ObjectDic.Instance.GetObjectName("음극 LOT"), ObjectDic.Instance.GetObjectName("양품") }.ToList();
                //dgLamiCapacitySummary.Columns["HOLD_QTY_A"].Header = new string[] { ObjectDic.Instance.GetObjectName("음극 LOT"), ObjectDic.Instance.GetObjectName("MES HOLD") }.ToList();
                //dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_A"].Header = new string[] { ObjectDic.Instance.GetObjectName("음극 LOT"), ObjectDic.Instance.GetObjectName("QMS HOLD") }.ToList();

                //dgLamiCapacitySummary.Columns["RACK_MAX"].DisplayIndex = 1;
                //dgLamiCapacitySummary.Columns["ERROR_QTY"].DisplayIndex = 2;
                //dgLamiCapacitySummary.Columns["ABNORM_QTY"].DisplayIndex = 3;
                //dgLamiCapacitySummary.Columns["PROHIBIT_QTY"].DisplayIndex = 4;
                //dgLamiCapacitySummary.Columns["RACK_QTY"].DisplayIndex = 5;
                //dgLamiCapacitySummary.Columns["RACK_RATE"].DisplayIndex = 6;
                //dgLamiCapacitySummary.Columns["PRJT_NAME"].DisplayIndex = 7;
                //dgLamiCapacitySummary.Columns["LOT_QTY_C"].DisplayIndex = 8;
                //dgLamiCapacitySummary.Columns["HOLD_QTY_C"].DisplayIndex = 9;
                //dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_C"].DisplayIndex = 10;
                //dgLamiCapacitySummary.Columns["LOT_QTY_A"].DisplayIndex = 11;
                //dgLamiCapacitySummary.Columns["HOLD_QTY_A"].DisplayIndex = 12;
                //dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_A"].DisplayIndex = 13;
                //dgLamiCapacitySummary.Columns["EMPTY_QTY"].DisplayIndex = 14;

                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.Columns.Add("LANGID", typeof(string));
                RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

                DataRow row = RQSTDT1.NewRow();
                row["LANGID"] = LoginInfo.USERID;
                row["CMCDTYPE"] = "REP_LOT_USE_AREA";
                row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT1.Rows.Add(row);

                DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

                if (dtCommon != null)
                {
                    if (dtCommon.Rows.Count > 0)
                    {
                        // 주동석 팬케이크 창고일 경우 공 케리어 탭 숨김
                        tabEmptyCarrier.Visibility = Visibility.Collapsed;
                        dgCapacitySummary.Columns["BBN_E_QTY"].Visibility = Visibility.Collapsed;

                        _LotConf = true;
                        dgProduct.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("대표 LOTID");
                        tblBobbond.Text = "대표 LOTID";
                    }
                    else
                    {
                        _LotConf = false;
                        dgProduct.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                        tblBobbond.Text = "Carrier ID";
                    }
                }
            }
            else if (cboStockerType.SelectedValue.GetString() == "JTW") //JELLY ROLL TRAY WAREHOUSE
            {
                LeftAreaLami.Visibility = Visibility.Collapsed;
                LeftArea.Visibility = Visibility.Visible;

                dgProduct.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                dgProduct.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                dgProduct.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                dgProduct.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;
                dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
               
                dgCapacitySummary.Columns["RACK_QTY"].Visibility = Visibility.Visible;
                dgCapacitySummary.Columns["BBN_UM_QTY"].Visibility = Visibility.Collapsed;


                dgProduct.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("REP_CSTID");
                dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("CSTID");
                dgCarrierList.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("CSTID");

                //dgCapacitySummary.Columns["RACK_MAX"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("용량") }.ToList();
                //dgCapacitySummary.Columns["ERROR_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("비정상 Rack") }.ToList();
                //dgCapacitySummary.Columns["ABNORM_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("정보불일치수") }.ToList();
                //dgCapacitySummary.Columns["PROHIBIT_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("금지단 수") }.ToList();
                //dgCapacitySummary.Columns["RACK_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("사용중 Rack") }.ToList();
                //dgCapacitySummary.Columns["RACK_RATE"].Header = new string[] { ObjectDic.Instance.GetObjectName("Rack"), ObjectDic.Instance.GetObjectName("적재율(%)") }.ToList();

                //dgCapacitySummary.Columns["LOT_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Carrier"), ObjectDic.Instance.GetObjectName("양품") }.ToList();
                //dgCapacitySummary.Columns["LOT_HOLD_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Carrier"), ObjectDic.Instance.GetObjectName("MES HOLD") }.ToList();
                //dgCapacitySummary.Columns["LOT_HOLD_QMS_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Carrier"), ObjectDic.Instance.GetObjectName("QMS HOLD") }.ToList();
                //dgCapacitySummary.Columns["BBN_E_QTY"].Header = new string[] { ObjectDic.Instance.GetObjectName("Carrier"), ObjectDic.Instance.GetObjectName("공Carrier수") }.ToList();
               
                //dgCapacitySummary.Columns["EQPTNAME"].DisplayIndex = 3;
                //dgCapacitySummary.Columns["RACK_MAX"].DisplayIndex = 4;
                //dgCapacitySummary.Columns["ERROR_QTY"].DisplayIndex = 5;
                //dgCapacitySummary.Columns["ABNORM_QTY"].DisplayIndex = 6;
                //dgCapacitySummary.Columns["PROHIBIT_QTY"].DisplayIndex = 7;
                //dgCapacitySummary.Columns["RACK_QTY"].DisplayIndex = 8;
                //dgCapacitySummary.Columns["RACK_RATE"].DisplayIndex = 9;
                //dgCapacitySummary.Columns["PRJT_NAME"].DisplayIndex = 10;
                //dgCapacitySummary.Columns["LOT_QTY"].DisplayIndex = 11;
                //dgCapacitySummary.Columns["LOT_HOLD_QTY"].DisplayIndex = 12;
                //dgCapacitySummary.Columns["LOT_HOLD_QMS_QTY"].DisplayIndex = 13;
                //dgCapacitySummary.Columns["BBN_E_QTY"].DisplayIndex = 14;
                //dgCapacitySummary.Columns["BBN_U_QTY"].DisplayIndex = 15;
                //dgCapacitySummary.Columns["BBN_UM_QTY"].DisplayIndex = 15;
                

                if (_Skid_Use_Chk == "Y")
                {
                    dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
                    dgCarrierList.Columns["CARRIERID"].Visibility = Visibility.Visible;
                }

                //DataTable RQSTDT1 = new DataTable();
                //RQSTDT1.Columns.Add("LANGID", typeof(string));
                //RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
                //RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

                //DataRow row = RQSTDT1.NewRow();
                //row["LANGID"] = LoginInfo.USERID;
                //row["CMCDTYPE"] = "REP_LOT_USE_AREA";
                //row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

                //RQSTDT1.Rows.Add(row);

                //DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

                //if (dtCommon != null)
                //{
                //    if (dtCommon.Rows.Count > 0)
                //    {
                //        // 주동석 팬케이크 창고일 경우 공 케리어 탭 숨김
                //        tabEmptyCarrier.Visibility = Visibility.Collapsed;
                //        dgCapacitySummary.Columns["BBN_E_QTY"].Visibility = Visibility.Collapsed;

                //        _LotConf = true;
                //        dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("대표 LOTID");
                //        tblBobbond.Text = "대표 LOTID";
                //    }
                //    else
                //    {
                //        _LotConf = false;
                //        dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("보빈 ID");
                //        tblBobbond.Text = "보빈 ID";
                //    }
                //}
            }
        }
        #endregion

        #region 전극 콤보 이벤트 : cboElectrodeType_SelectedValueChanged()
        /// <summary>
        /// 전극 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }
        #endregion

        #region 창고 콤보 이벤트 : cboStocker_SelectedValueChanged()
        /// <summary>
        /// 창고 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }
        #endregion

        #region 조회버튼  이벤트 : btnSearch_Click()
        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            _ElectrodeType_Chk = cboElectrodeType.SelectedValue == null ? string.Empty : cboElectrodeType.SelectedValue.ToString();
            //극성 정보 존재 및 스태킹 완성창고
            //폴란드 조립 4동일 경우 노칭완성 창고에서는 극성을 관리 하지 않음
            if (cboStockerType.SelectedValue.GetString() == "LWW")
            //if (cboElectrodeType.Items.Count > 1 || cboStockerType.SelectedValue.GetString() == "SWW" || cboStockerType.SelectedValue.GetString() == "JTW")
            {
                SelectWareHouseLamiSummary();
                
            }
            else
            {
                SelectWareHouseCapacitySummary();
            }
            // 노칭 WO별 창고적재 수량 정보 여부
            if (tabStatusbyWorkorder.Visibility == Visibility.Visible)
            {
                SelectStatusbyWorkorder();
            }
            // 노칭 WO별 창고적재 HOLD 전극 여부
            if (tabHoldList.Visibility == Visibility.Visible)
            {
                SelectHold();
            }
            if (tabLoadQty.Visibility == Visibility.Visible)
            {
                SearchLoadQtyList();
            }

        }
        #endregion

        #region 창고 적재현황 리스트 이벤트 : dgCapacitySummary_()

        /// <summary>
        /// 창고 적재현황 클릭 - 노칭대기, 노칭완성, 스태킹완성
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null
                   || cell.Column.Name.Equals("ELTR_TYPE_NAME")
                   || cell.Column.Name.Equals("RACK_MAX")
                   || cell.Column.Name.Equals("BBN_U_QTY")
                   || cell.Column.Name.Equals("RACK_RATE")
                   )
                {
                    return;
                }

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedLotElectrodeTypeCode = null;
                _selectedStkElectrodeTypeCode = null;
                _selectedLotIdByRackInfo = null;
                _selectedSkIdIdByRackInfo = null;
                _selectedBobbinIdByRackInfo = null;
                _selectedRackIdByRackInfo = null;

                if (cell.Column.Name.Equals("EQPTNAME") || (cell.Column.Name.Equals("ELTR_TYPE_NAME") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                {

                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }


                    if ((_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ")) || cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }

                    Util.gridClear(dgProduct);

                    if (cell.Column.Name.Equals("EQPTNAME") || (cell.Column.Name.Equals("ELTR_TYPE_NAME") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        _selectedWipHold = null;
                        _selectedProjectName = null;

                        if (tabLayout.IsSelected && (cell.Column.Name.Equals("EQPTNAME") || cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                        {

                            ShowLoadingIndicator();
                            DoEvents();

                            SelectMaxxyz();
                            ReSetLayoutUserControl();

                            if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                            {
                                SelectRackInfo();
                            }
                            else
                            {
                                HiddenLoadingIndicator();
                            }


                        }
                        else
                        {
                            SelectWareHouseProductList((table, ex) =>
                            {
                                tabProduct.IsSelected = true;
                                Util.GridSetData(dgProduct, table, null, true);
                                HiddenLoadingIndicator();
                            });
                        }

                    }
                    else
                    {
                        tabProduct.IsSelected = true;
                    }

                }
                else if (cell.Column.Name.Equals("PRJT_NAME")) // PJT 클릭시
                {
                    if (DataTableConverter.GetValue(drv, "PRJT_NAME").GetString() == string.Empty)
                        return;

                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                    _selectedStkElectrodeTypeCode = null;
                    _selectedLotElectrodeTypeCode = null;
                    _selectedWipHold = null;
                    _selectedQmsHold = null;
                    tabProduct.IsSelected = true;
                    SelectWareHouseProductList(dgProduct);
                }
                else if (cell.Column.Name.Equals("LOT_QTY") || cell.Column.Name.Equals("RACK_QTY")) // 양품수량 클릭시
                {


                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if ((_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ")) || cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }

                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString() == string.Empty ? null : DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                    _selectedStkElectrodeTypeCode = null;
                    _selectedWipHold = "N";
                    _selectedQmsHold = "N";
                    tabProduct.IsSelected = true;
                    SelectWareHouseProductList(dgProduct);
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QTY")) // 불량수량 클릭시 [MES]
                {


                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if ((_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ")) || cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }

                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString() == string.Empty ? null : DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                    _selectedStkElectrodeTypeCode = null;
                    _selectedWipHold = "Y";
                    _selectedQmsHold = null;
                    tabProduct.IsSelected = true;
                    SelectWareHouseProductList(dgProduct);
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QMS_QTY")) // QMS불량수량 클릭시
                {


                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }
                    if ((_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ")) || cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString() == string.Empty ? null : DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                    _selectedStkElectrodeTypeCode = null;
                    _selectedQmsHold = "Y";
                    _selectedWipHold = "N";
                    tabProduct.IsSelected = true;
                    SelectWareHouseProductList(dgProduct);
                }
                else if (cell.Column.Name.Equals("BBN_E_QTY")) //공Carrier수
                {
                    //_selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if ((_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ")) || cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }

                    tabEmptyCarrier.IsSelected = true;
                    SelectWareHouseEmptyCarrier(sCurtrode);
                    SelectWareHouseEmptyCarrierList(dgCarrierList, sCurtrode);
                }
                else if (cell.Column.Name.Equals("ERROR_QTY")) //비정상 Rack
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString(), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }

                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList(dgErrorCarrier);
                }
                else if (cell.Column.Name.Equals("ABNORM_QTY")) //정보불일치수
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString(), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }

                    tabAbNormalCarrier.IsSelected = true;
                    SelectAbNormalCarrierList(dgAbNormalCarrier);
                }
                else if (cell.Column.Name.Equals("PROHIBIT_QTY")) //금지단수
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString(), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }

                    tabProHibit.IsSelected = true;
                    SelectProHibitList(dgProHibit);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 컬럼에 대한 색깔 표시 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "EQPTNAME") || string.Equals(e.Cell.Column.Name, "PRJT_NAME") || string.Equals(e.Cell.Column.Name, "LOT_QTY") || string.Equals(e.Cell.Column.Name, "LOT_HOLD_QTY") || string.Equals(e.Cell.Column.Name, "LOT_HOLD_QMS_QTY") || string.Equals(e.Cell.Column.Name, "BBN_E_QTY") || string.Equals(e.Cell.Column.Name, "ERROR_QTY") || string.Equals(e.Cell.Column.Name, "ABNORM_QTY") || string.Equals(e.Cell.Column.Name, "PROHIBIT_QTY") || string.Equals(e.Cell.Column.Name, "RACK_QTY"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (string.Equals(e.Cell.Column.Name, "BBN_UM_QTY") && Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BBN_UM_QTY")) > 0)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }


                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")), "XXXXXXXXXX") && !string.Equals(e.Cell.Column.Name, "ELTR_TYPE_NAME"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Aqua");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }
        /// <summary>
        /// 컬럼에 대한 색깔 표시 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        /// <summary>
        /// 극성 클럼을 기준으로 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_MergingCells(object sender, DataGridMergingCellsEventArgs e)
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
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE")
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        Count = g.Count()
                    }).ToList();

                    string previewElectrodeTypeCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index + 1)));
                        }
                        else
                        {
                            foreach (var item in query)
                            {
                                int rowIndex = i;
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString() == item.ElectrodeTypeCode && previewElectrodeTypeCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ELTR_TYPE_NAME"].Index)));
                                }
                            }
                        }
                        previewElectrodeTypeCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString();
                    }
                    DataTable dt1 = ((DataView)dg.ItemsSource).Table;
                    var query1 = dt.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE"),
                        equipmentCode = x.Field<string>("EQPTID")
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        EquipmentCode = g.Key.equipmentCode,
                        Count = g.Count()
                    }).OrderBy(o => o.ElectrodeTypeCode).ThenBy(t => t.EquipmentCode).ToList();

                    string previewEquipmentCode = string.Empty;

                    for (int j = 0; j < dg.Rows.Count; j++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString() == "XXXXXXXXXX" ||
                            DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString() == "ZZZZZZZZZZ")
                        {
                            continue;
                        }

                        foreach (var item in query1)
                        {
                            int rowIndex = j;

                            if (DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString() == item.EquipmentCode
                                && previewEquipmentCode != DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString()
                                && DataTableConverter.GetValue(dg.Rows[j].DataItem, "ELTR_TYPE_CODE").GetString() == item.ElectrodeTypeCode
                                )
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["EQPTNAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["RACK_MAX"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_MAX"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["BBN_U_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["BBN_U_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["BBN_E_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["BBN_E_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["BBN_UM_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["BBN_UM_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["ERROR_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ERROR_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["ABNORM_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ABNORM_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["PROHIBIT_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PROHIBIT_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["RACK_RATE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_RATE"].Index)));
                            }
                        }
                        previewEquipmentCode = DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString();
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 창고 적재현황 리스트 이벤트 (라미대기창고) : dgLamiCapacitySummary_()
        /// <summary>
        /// 창고 적재현황 클릭 - 라미대기창고
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLamiCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null
                  || cell.Column.Name.Equals("RACK_MAX")
                  || cell.Column.Name.Equals("RACK_RATE")
                   )

                {
                    return;
                }

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedStkElectrodeTypeCode = null;
                _selectedLotElectrodeTypeCode = null;
                _selectedEquipmentCode = null;
                _selectedProjectName = null;
                _selectedWipHold = null;
                _selectedQmsHold = null;

                _selectedLotIdByRackInfo = null;
                _selectedSkIdIdByRackInfo = null;
                _selectedBobbinIdByRackInfo = null;
                _selectedRackIdByRackInfo = null;

                if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                    string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                {
                    _selectedEquipmentCode = null;
                    _selectedProjectName = null;
                }
                else
                {
                    _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                }

                if (cell.Column.Name.Equals("EMPTY_CST_QTY_A"))
                {
                    tabEmptyCarrier.IsSelected = true;
                    sCurtrode = "A";
                    SelectWareHouseEmptyCarrier(sCurtrode);
                    SelectWareHouseEmptyCarrierList(dgCarrierList, sCurtrode);
                }
                else if (cell.Column.Name.Equals("EMPTY_CST_QTY_C"))
                {
                    tabEmptyCarrier.IsSelected = true;
                    sCurtrode = "C";
                    SelectWareHouseEmptyCarrier(sCurtrode);
                    SelectWareHouseEmptyCarrierList(dgCarrierList, sCurtrode);
                }
                else if (cell.Column.Name.Equals("ERROR_QTY"))
                {
                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList(dgErrorCarrier);
                }
                else if (cell.Column.Name.Equals("ABNORM_QTY"))
                {
                    tabAbNormalCarrier.IsSelected = true;
                    SelectAbNormalCarrierList(dgAbNormalCarrier);
                }
                else if (cell.Column.Name.Equals("PROHIBIT_QTY"))
                {
                    tabProHibit.IsSelected = true;
                    SelectProHibitList(dgProHibit);
                }
                // MES 2.0 - 박병성 책임 요청으로 코드 추가 Start
                else if (cell.Column.Name.Equals("LOT_HOLD_QMS_QTY")) // QMS불량수량 클릭시
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }
                    if ((_ElectrodeType_Chk == string.Empty && string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ")) || cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") ? _ElectrodeType_Chk : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                        // _selectedLotElectrodeTypeCode 값이 "" 공란으로 들어 올경우 파라미터가 있는것으로 넘어가 값이 다르게 나와 null 로 수정함 2023.10.05 주동석
                        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(_selectedLotElectrodeTypeCode) ? null : _selectedLotElectrodeTypeCode;
                    }
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString() == string.Empty ? null : DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                    _selectedStkElectrodeTypeCode = null;
                    _selectedQmsHold = "Y";
                    _selectedWipHold = "N";
                    tabProduct.IsSelected = true;
                    SelectWareHouseProductList(dgProduct);
                }
                // MES 2.0 - 박병성 책임 요청으로 코드 추가 End
                else
                {
                    if (cell.Column.Name.Equals("EQPTNAME") || cell.Column.Name.Equals("RACK_MAX_QTY") || cell.Column.Name.Equals("RACK_RATE"))
                    {
                        _selectedProjectName = null;
                    }

                    else if (cell.Column.Name.Equals("LOT_QTY_C"))
                    {
                        _selectedLotElectrodeTypeCode = "C";
                        _selectedWipHold = "N";
                    }
                    else if (cell.Column.Name.Equals("HOLD_QTY_C"))
                    {
                        _selectedLotElectrodeTypeCode = "C";
                        _selectedWipHold = "Y";
                    }
                    else if (cell.Column.Name.Equals("LOT_QTY_A"))
                    {
                        _selectedLotElectrodeTypeCode = "A";
                        _selectedWipHold = "N";
                    }
                    else if (cell.Column.Name.Equals("HOLD_QTY_A"))
                    {
                        _selectedLotElectrodeTypeCode = "A";
                        _selectedWipHold = "Y";
                    }

                    if (tabLayout.IsSelected && (cell.Column.Name.Equals("EQPTNAME") || cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        ShowLoadingIndicator();
                        DoEvents();

                        SelectMaxxyz();
                        ReSetLayoutUserControl();

                        if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                        {
                            SelectRackInfo();
                        }
                        else
                        {
                            HiddenLoadingIndicator();
                        }

                    }
                    else
                    {
                        Util.gridClear(dgProduct);
                        tabProduct.IsSelected = true;
                        SelectWareHouseProductList(dgProduct);
                    }
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 컬럼에 대한 색깔 표시 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLamiCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (string.Equals(e.Cell.Column.Name, "EQPTNAME") || string.Equals(e.Cell.Column.Name, "PRJT_NAME") || string.Equals(e.Cell.Column.Name, "LOT_QTY_C") 
                        || string.Equals(e.Cell.Column.Name, "HOLD_QTY_C") || string.Equals(e.Cell.Column.Name, "LOT_QTY_A") || string.Equals(e.Cell.Column.Name, "HOLD_QTY_A")                         
                        || string.Equals(e.Cell.Column.Name, "ERROR_QTY") || string.Equals(e.Cell.Column.Name, "ABNORM_QTY") || string.Equals(e.Cell.Column.Name, "PROHIBIT_QTY") 
                        || string.Equals(e.Cell.Column.Name, "RACK_QTY")  || string.Equals(e.Cell.Column.Name, "HOLD_QMS_QTY_C") || string.Equals(e.Cell.Column.Name, "HOLD_QMS_QTY_A")
                        || string.Equals(e.Cell.Column.Name, "EMPTY_CST_QTY_A") || string.Equals(e.Cell.Column.Name, "EMPTY_CST_QTY_C")
                       )
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }


                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));

            dataGrid.AllColumnsWidthAuto();
        }
        /// <summary>
        /// 컬럼에 대한 색깔 표시 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLamiCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        /// <summary>
        /// 극성 클럼을 기준으로 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLamiCapacitySummary_MergingCells(object sender, DataGridMergingCellsEventArgs e)
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
                        equipmentCode = x.Field<string>("EQPTID")
                    }).Select(g => new
                    {
                        EquipmentCode = g.Key.equipmentCode,
                        Count = g.Count()
                    }).ToList();

                    string previewEquipmentCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTNAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                        {
                            //e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPTNAME"].Index), dg.GetCell(i, dg.Columns["EQPTNAME"].Index + 1)));
                        }
                        else
                        {
                            foreach (var item in query)
                            {
                                int rowIndex = i;
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString() == item.EquipmentCode && previewEquipmentCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPTNAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_MAX"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_MAX"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["HOLD_QMS_QTY_A"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["HOLD_QMS_QTY_A"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["HOLD_QMS_QTY_C"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["HOLD_QMS_QTY_C"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ERROR_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ERROR_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ABNORM_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ABNORM_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PROHIBIT_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PROHIBIT_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_RATE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_RATE"].Index)));
                                }
                            }
                        }
                        previewEquipmentCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 창고 재공 현황 리스트 이벤트 : dgProductSummary_()

        ///// <summary>
        ///// 창고재공조회 그리드 클릭
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void dgProductSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        C1DataGrid dg = sender as C1DataGrid;
        //        if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

        //        Point pnt = e.GetPosition(null);
        //        C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

        //        if (cell == null
        //            || cell.Column.Name.Equals("WIP_HOLD_RATE")
        //            || cell.Column.Name.Equals("WIP_QTY")
        //            || cell.Column.Name.Equals("WIP_HOLD_QTY")
        //            || cell.Column.Name.Equals("WIP_HOLD_QMS_QTY"))

        //        {
        //            return;
        //        }


        //        int rowIdx = cell.Row.Index;
        //        DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
        //        if (drv == null) return;

        //        _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
        //        _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE_LOT").GetString()) ? null : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE_LOT").GetString();
        //        _selectedStkElectrodeTypeCode = null;
        //        _selectedQmsHold = null;

        //        if (cell.Column.Name.Equals("LOT_QTY") || cell.Column.Name.Equals("WIP_QTY"))
        //        {
        //            _selectedWipHold = "N";
        //            _selectedQmsHold = "N";
        //        }
        //        else if (cell.Column.Name.Equals("LOT_HOLD_QTY") || cell.Column.Name.Equals("WIP_HOLD_QTY"))
        //        {
        //            _selectedWipHold = "Y";
        //            _selectedQmsHold = null;
        //        }
        //        else if (cell.Column.Name.Equals("LOT_HOLD_QMS_QTY") || cell.Column.Name.Equals("WIP_HOLD_QMS_QTY"))
        //        {
        //            _selectedQmsHold = "Y";
        //            _selectedWipHold = "N";
        //        }
        //        else if (cell.Column.Name.Equals("PRJT_NAME"))
        //        {
        //            _selectedLotElectrodeTypeCode = null;
        //            _selectedWipHold = null;
        //        }

        //        tabProduct.IsSelected = true;
        //        SelectWareHouseProductList(dgProduct);

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        ///// <summary>
        ///// 컬럼에 대한 색깔 표시 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void dgProductSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    if (sender == null) return;

        //    C1DataGrid dataGrid = sender as C1DataGrid;

        //    dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (e.Cell.Presenter == null) return;


        //        if (string.Equals(e.Cell.Column.Name, "PRJT_NAME")
        //            || string.Equals(e.Cell.Column.Name, "ELTR_TYPE_NAME")
        //            || string.Equals(e.Cell.Column.Name, "LOT_QTY")
        //            || string.Equals(e.Cell.Column.Name, "LOT_HOLD_QTY")
        //            || string.Equals(e.Cell.Column.Name, "LOT_HOLD_QMS_QTY"))
        //        {
        //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
        //            e.Cell.Presenter.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            if (string.Equals(e.Cell.Column.Name, "WIP_HOLD_RATE"))
        //            {
        //                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_HOLD_RATE").GetDecimal() >= 20)
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
        //                }
        //                else
        //                {
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
        //                }
        //            }
        //            else
        //            {
        //                e.Cell.Presenter.FontWeight = FontWeights.Normal;
        //                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //            }
        //        }
        //    }));
        //}
        ///// <summary>
        ///// 컬럼에 대한 색깔 표시 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void dgProductSummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    if (sender == null) return;

        //    C1DataGrid dataGrid = sender as C1DataGrid;
        //    if (e.Cell.Presenter != null)
        //    {
        //        if (e.Cell.Row.Type == DataGridRowType.Item)
        //        {
        //            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Cell.Presenter.FontWeight = FontWeights.Normal;
        //            e.Cell.Presenter.Background = null;
        //        }
        //    }
        //}


        #endregion

        #region 입고 LOT 리스트 이벤트 : dgProduct_()

        /// <summary>
        /// 입고 LOT 이벤트 (랏정보조회 화면 이동)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("LOTID"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID"));
                    this.FrameOperation.OpenMenu("SFU010160050", true, parameters);
                }

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("RACK_ID"))
                {

                    MCS001_083_RACK_INFO popupRackInfo = new MCS001_083_RACK_INFO { FrameOperation = FrameOperation };
                    object[] parameters = new object[5];
                    parameters[0] = cboArea.SelectedValue;
                    parameters[1] = cboStockerType.SelectedValue;
                    parameters[2] = LoginInfo.CFG_AREA_ID;
                    parameters[3] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "EQPTID"));
                    parameters[4] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "RACK_ID"));
                    C1WindowExtension.SetParameters(popupRackInfo, parameters);

                    popupRackInfo.Closed += popupRackInfo_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupRackInfo.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// RACK 정보 LIST 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupRackInfo_Closed(object sender, EventArgs e)
        {
            MCS001_083_RACK_INFO popup = sender as MCS001_083_RACK_INFO;
            if (popup != null)
            {

            }
        }
        /// <summary>
        /// 경과일수에 대한 색깔표시 (범례참조)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "RACK_ID" && cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                    if (_isscrollToHorizontalOffset)
                    {
                        dataGrid.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                    }
                }
                else
                {

                }
            }));
        }
        /// <summary>
        /// 경과일수에 대한 색깔표시 (범례참조)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgProduct_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isscrollToHorizontalOffset = false;
        }

        /// <summary>
        /// 실 Carrier 리스트 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgProduct.TopRows.Count; i < dgProduct.Rows.Count; i++)
                {

                    if (dgProduct.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgProduct.Rows[i].DataItem, "BOBBIN_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgProduct.Rows[i].DataItem, "BOBBIN_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgProduct.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["ROW_NUM"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["ROW_NUM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["EQPTNAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["RACK_ID"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["RACK_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["RACK_NAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["RACK_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["CSTINDTTM"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["CSTINDTTM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["EQSGNAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["EQSGNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["BOBBIN_ID"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["BOBBIN_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["RACK_STAT_NAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["RACK_STAT_NAME"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["ROW_NUM"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["ROW_NUM"].Index)));
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["EQPTNAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["RACK_ID"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["RACK_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["RACK_NAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["RACK_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["CSTINDTTM"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["CSTINDTTM"].Index)));
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["EQSGNAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["EQSGNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["BOBBIN_ID"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["BOBBIN_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgProduct.GetCell(idxS, dgProduct.Columns["RACK_STAT_NAME"].Index), dgProduct.GetCell(idxE, dgProduct.Columns["RACK_STAT_NAME"].Index)));
                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgProduct.Rows[i].DataItem, "BOBBIN_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region 공 Carrier LOT 리스트 이벤트 :dgCarrierList_()

        /// <summary>
        /// 공 Carrier 리스트 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCarrierList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
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
                        CarrierID = x.Field<string>("CARRIERID")
                    }).Select(g => new
                    {
                        GroupCarrierID = g.Key.CarrierID,
                        Count = g.Count()
                    }).ToList();

                    string GroupCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        foreach (var item in query)
                        {
                            int rowIndex = i;
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "CARRIERID").GetString() == item.GroupCarrierID && GroupCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "CARRIERID").GetString())
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_STAT_NAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_STAT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ABNORM_TRF_RSN_NAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ABNORM_TRF_RSN_NAME"].Index)));
                            }
                        }
                        GroupCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "CARRIERID").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region NND W/O별 현황 리스트 이벤트 : dgStatusbyWorkorder_()

        /// <summary>
        /// 리스트 이벤트 (랏 리스트 정보 조회 팝업) 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStatusbyWorkorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || cell.Column.Name.Equals("EQSGNAME") || cell.Column.Name.Equals("PRJT_NAME") || cell.Column.Name.Equals("EQSGID")) return;
                if (cell.Text.Replace(",", "").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_083_LOTLIST popupLotlist = new MCS001_083_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[4];
                parameters[0] = LoginInfo.CFG_AREA_ID;
                parameters[1] = DataTableConverter.GetValue(drv, "EQSGID").GetString();
                parameters[2] = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                parameters[3] = cell.Column.Name;
                //parameters[4] = DataTableConverter.GetValue(drv, "WO_DETL_ID").GetString();  // 추가 부분
                //parameters[5] = DataTableConverter.GetValue(drv, "DEMAND_TYPE_NAME").GetString();
                //parameters[6] = DataTableConverter.GetValue(drv, "ALTERNATE_MATERIAL_SPEC_ID").GetString();
                C1WindowExtension.SetParameters(popupLotlist, parameters);

                popupLotlist.Closed += popupLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// NND W/O별 현황 리스트 색 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStatusbyWorkorder_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "EQSGNAME") || string.Equals(e.Cell.Column.Name, "PRJT_NAME"))
                {
                    return;
                }
                else if (string.Equals(e.Cell.Column.Name, "FOL_EQSG_MODEL_QTY") || string.Equals(e.Cell.Column.Name, "FOL_MODEL_QTY")) // PKG Input Try
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1000)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1)
                        {
                            e.Cell.Presenter.Cursor = Cursors.Arrow;
                        }
                        else
                        {
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }

                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 1000 && DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 2000)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
                else if (string.Equals(e.Cell.Column.Name, "AN_QTY_AF_NT_NPW") || string.Equals(e.Cell.Column.Name, "CA_QTY_AF_NT_NPW")) //After NND Stocker 
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 5)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1)
                        {
                            e.Cell.Presenter.Cursor = Cursors.Arrow;
                        }
                        else
                        {
                            e.Cell.Presenter.Cursor = Cursors.Hand;
                        }
                    }
                }
                else
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() > 0 && DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 6)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        /// <summary>
        /// NND W/O별 현황 리스트 색 표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStatusbyWorkorder_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        /// <summary>
        /// 랏 리스트 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupLotlist_Closed(object sender, EventArgs e)
        {
            MCS001_083_LOTLIST popup = sender as MCS001_083_LOTLIST;
            if (popup != null)
            {

            }
        }
        #endregion

        #region Hold 재고 현황 리스트 이벤트 : dgHold_()

        /// <summary>
        /// Hold 재고 현황 리스트 이벤트 :  Hold Lot 리스트 조회 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgHold_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || cell.Column.Name.Equals("EQSGNAME") || cell.Column.Name.Equals("PRJT_NAME") || cell.Column.Name.Equals("EQSGID")) return;
                if (cell.Text.Replace(",", "").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_083_HOLD_LOTLIST popupHoldLotlist = new MCS001_083_HOLD_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[4];
                parameters[0] = LoginInfo.CFG_AREA_ID;
                parameters[1] = DataTableConverter.GetValue(drv, "EQSGID").GetString();
                parameters[2] = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                parameters[3] = cell.Column.Name;
                C1WindowExtension.SetParameters(popupHoldLotlist, parameters);

                popupHoldLotlist.Closed += popupHoldLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupHoldLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 색깔표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgHold_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "EQSGNAME") || string.Equals(e.Cell.Column.Name, "PRJT_NAME"))
                {
                    return;
                }

                else
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() > 4)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        /// <summary>
        /// 색깔표시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgHold_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        /// <summary>
        /// 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void popupHoldLotlist_Closed(object sender, EventArgs e)
        {
            MCS001_083_HOLD_LOTLIST popup = sender as MCS001_083_HOLD_LOTLIST;
            if (popup != null)
            {

            }
        }

        #endregion

        #region Tab 이벤트 : C1TabControl_SelectionChanged()
        /// <summary>
        /// Tab 이벤트 : LayOut 정보 재 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CommonVerify.HasDataGridRow(dgProduct))
                {
                    _isscrollToHorizontalOffset = true;
                    _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
                }

                string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

                if (string.Equals(tabItem, "tabLayout"))
                {
                    if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                    {
                        ShowLoadingIndicator();
                        DoEvents();

                        SelectMaxxyz();
                        ReSetLayoutUserControl();
                        SelectRackInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region UserControl 체크 이벤트  : UcRackLayout1_Checked(), UcRackLayout2_Checked()

        /// <summary>
        /// UserControl 체크시 상세정보 조회 : 1열
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout1_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            _selectedRackIdByRackInfo = null;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            if (rackLayout.IsChecked)
            {
                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout1[row][col];

                        if (!Equals(rackLayout, ucRackLayout))
                        {
                            ucRackLayout.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (ucRackLayout.IsChecked)
                            ucRackLayout.IsChecked = false;
                    }
                }

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
                }

                _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
                _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
                _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;
                _selectedRackIdByRackInfo = string.IsNullOrEmpty(rackLayout.RackId) ? null : rackLayout.RackId;

                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackLayout.RackId;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["PRJT_NAME"] = rackLayout.ProjectName;
                dr["LOTID"] = rackLayout.LotId;
                dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
                dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["WIPHOLD"] = rackLayout.WipHold;
                dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
                dr["SKID_GUBUN"] = rackLayout.SkidType;
                dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
                dr["COLOR"] = rackLayout.LegendColor;
                dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
                dr["HOLD_FLAG"] = rackLayout.HoldFlag;
                dr["SEQ"] = maxSeq;
                _dtRackInfo.Rows.Add(dr);

                if (rackLayout.LegendColorType == "4")
                {
                    _selectedSkIdIdByRackInfo = null;
                }
                else if (rackLayout.LegendColorType == "5")
                {
                    _selectedBobbinIdByRackInfo = null;
                }

                GetLayoutGridColumns(rackLayout.LegendColorType);
                SelectLayoutGrid(rackLayout.LegendColorType);
            }
            else
            {

                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// UserControl 체크시 상세정보 조회 : 2열
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout2_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            _selectedRackIdByRackInfo = null;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            if (rackLayout.IsChecked)
            {
                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (!Equals(rackLayout, ucRackLayout))
                        {
                            ucRackLayout.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackStair = _ucRackLayout1[row][col];

                        if (ucRackStair.IsChecked)
                            ucRackStair.IsChecked = false;
                    }
                }

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
                }

                _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
                _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
                _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;
                _selectedRackIdByRackInfo = string.IsNullOrEmpty(rackLayout.RackId) ? null : rackLayout.RackId;

                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackLayout.RackId;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["PRJT_NAME"] = rackLayout.ProjectName;
                dr["LOTID"] = rackLayout.LotId;
                dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
                dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["WIPHOLD"] = rackLayout.WipHold;
                dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
                dr["SKID_GUBUN"] = rackLayout.SkidType;
                dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
                dr["COLOR"] = rackLayout.LegendColor;
                dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
                dr["HOLD_FLAG"] = rackLayout.HoldFlag;
                dr["SEQ"] = maxSeq;
                _dtRackInfo.Rows.Add(dr);
                GetLayoutGridColumns(rackLayout.LegendColorType);
                SelectLayoutGrid(rackLayout.LegendColorType);
            }
            else
            {

                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }
        }
        #endregion

        #region Layout 탭에서  LOTID 및 보빈ID 텍스트 박스 이벤트 : textBox_KeyDown()
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (string.IsNullOrEmpty(textBox.Text.Trim())) return;

            if (textBox.Name == "txtLotId")
            {
                txtBobbinId.Text = string.Empty;
            }
            else
            {
                txtLotId.Text = string.Empty;
            }


            if (e.Key == Key.Enter)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();

                UnCheckedAllRackLayout();

                for (int r = 0; r < grdRackstair1.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair1.ColumnDefinitions.Count; c++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout1[r][c];

                        doubleAnimation.From = ucRackLayout.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                        doubleAnimation.AutoReverse = true;

                        string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.BobbinCarrierCode;

                        if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            SetScrollToHorizontalOffset(scrollViewer1, c);
                            ucRackLayout.IsChecked = true;
                            CheckUcRackLayout(ucRackLayout);
                            ucRackLayout.BeginAnimation(HeightProperty, doubleAnimation);

                            return;
                        }
                    }
                }

                for (int r = 0; r < grdRackstair2.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair2.ColumnDefinitions.Count; c++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[r][c];

                        doubleAnimation.From = ucRackLayout.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        doubleAnimation.AutoReverse = true;

                        string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.BobbinCarrierCode;

                        if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            SetScrollToHorizontalOffset(scrollViewer2, c);
                            ucRackLayout.IsChecked = true;
                            CheckUcRackLayout(ucRackLayout);
                            ucRackLayout.BeginAnimation(HeightProperty, doubleAnimation);

                            return;
                        }
                    }
                }
            }
        }
        #endregion

        #region  타이머 콤보박스 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// 타이머 콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Splitter 이벤트 : Splitter_DragStarted(), Splitter_DragCompleted()
        private void Splitter_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            GridSplitter splitter = (GridSplitter)sender;

            try
            {
                C1DataGrid dataGrid = (LeftArea.Visibility == Visibility.Visible) ? dgCapacitySummary : dgLamiCapacitySummary;
                double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);

                if (ContentsRow.ColumnDefinitions[0].Width.Value > sumWidth)
                {
                    ContentsRow.ColumnDefinitions[0].Width = new GridLength(sumWidth + splitter.ActualWidth);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 타이머 실행 이벤트 : _dispatcherTimer_Tick()
        /// <summary>
        /// 타이머 실행 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    if (tabLoadQty.IsSelected == false)
                    {
                        Time_Re_Search();
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

        #region Rack 상세정보 리스트 머지 : dgRackInfo_MergingCells()
        /// <summary>
        /// Rack 상세 리스트 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRackInfo_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgRackInfo.TopRows.Count; i < dgRackInfo.Rows.Count; i++)
                {

                    if (dgRackInfo.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgRackInfo.Rows[i].DataItem, "CARRIERID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgRackInfo.Rows[i].DataItem, "CARRIERID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgRackInfo.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }

                                    e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["ROW_NUM"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["ROW_NUM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["EQPTNAME"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["RACK_NAME"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["RACK_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["CSTINDTTM"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["CSTINDTTM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["EQSGNAME"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["EQSGNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["SKID_ID"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["SKID_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["RACK_ID"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["RACK_ID"].Index)));


                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["ROW_NUM"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["ROW_NUM"].Index)));
                                e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["EQPTNAME"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["RACK_NAME"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["RACK_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["CSTINDTTM"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["CSTINDTTM"].Index)));
                                e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["EQSGNAME"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["EQSGNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["SKID_ID"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["SKID_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgRackInfo.GetCell(idxS, dgRackInfo.Columns["RACK_ID"].Index), dgRackInfo.GetCell(idxE, dgRackInfo.Columns["RACK_ID"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgRackInfo.Rows[i].DataItem, "CARRIERID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }


        #endregion

        #region NND STO 그룹별 적재수량 - 셋팅된 수량 리스트 이벤트 : dgStocGr_()
        /// <summary>
        /// STO 그룹별 적재수량 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStocGr_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgStocGr.TopRows.Count; i < dgStocGr.Rows.Count; i++)
                {

                    if (dgStocGr.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgStocGr.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CHK"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CAPA_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CAPA_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["SAFE_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["SAFE_QTY"].Index)));

                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CHK"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CAPA_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CAPA_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["SAFE_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["SAFE_QTY"].Index)));


                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }

                idxS = 0;
                idxE = 0;
                bStrt = false;
                sTmpLvCd = string.Empty;
                sTmpTOTALQTY = string.Empty;
                for (int i = dgStocGr.TopRows.Count; i < dgStocGr.Rows.Count; i++)
                {

                    if (dgStocGr.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")) + Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "EQPTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if ((Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")) + Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "EQPTID"))).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgStocGr.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["EQPTNAME"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["EQPTNAME"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["EQPTNAME"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["EQPTNAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")) + Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "EQPTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
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

        /// <summary>
        /// STO 그룹별 적재수량 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStocGrChoice_Checked(object sender, RoutedEventArgs e)
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

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgStocGr.SelectedIndex = idx;
                txtStck_Gr.Text = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "STOC_STCK_GR_NAME"));
                txtStck_Gr.Tag = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "STOC_STCK_GR_ID"));
                txtSafeQty.Text = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "SAFE_QTY"));
                GetLineLoadQty(Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "STOC_STCK_GR_ID")));

            }
        }


        #endregion

        #region NND STO 그룹별 적재수량 - 셋팅할 적재수량 리스트 이벤트 :  dgLineLoadQty_()

        /// <summary>
        /// 수정대상이 아닌 데이터 Enable 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLineLoadQty_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    return;
                }

                // 기존 데이터 일경우 안전재고비율만 수정 가능
                else if (DataTableConverter.GetValue(e.Row.DataItem, "TYPE").ToString() == "Y")
                {
                    if (e.Column.Name.Equals("EQPTID") ||
                        e.Column.Name.Equals("LOAD_QTY")
                       )
                        e.Cancel = true;

                    return;
                }
                else
                    e.Cancel = false;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 수정대상 리스트 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLineLoadQty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "CHK"))
                {
                    return;
                }
                else if (string.Equals(e.Cell.Column.Name, "EQPTID"))
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TYPE").GetString() == "Y")
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
                else if (string.Equals(e.Cell.Column.Name, "LOAD_QTY"))
                {
                    if (!String.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TYPE").GetString()))
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }

                }
                else if (string.Equals(e.Cell.Column.Name, "LOAD_RATE") || string.Equals(e.Cell.Column.Name, "USE_FLAG"))
                {
                    if (!String.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TYPE").GetString()))
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }

                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                }
            }));
        }

        /// <summary>
        /// 데이터 수정시 비율에 대한 수량 계산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLineLoadQty_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

                if (e.Cell.Column.Name.Equals("LOAD_RATE") || e.Cell.Column.Name.Equals("USE_FLAG") || e.Cell.Column.Name.Equals("EQPTID"))
                {
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", 1);
                }
                if (e.Cell.Column.Name.Equals("LOAD_RATE"))
                {
                    decimal _TOTLOADQTY = 0;

                    _TOTLOADQTY = Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOAD_RATE").GetString()) * Convert.ToDecimal(txtSafeQty.Text) / 100;
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "LOAD_QTY", _TOTLOADQTY);
                }
                dgLineLoadQty.EndEdit();
                dgLineLoadQty.Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 전체 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgLineLoadQty);
        }
        /// <summary>
        /// 전체 선택 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgLineLoadQty);
        }

        #endregion

        #region NND STO 그룹별 적재수량 - 리스트 ROW 추가 : btnAdd_Click()
        /// <summary>
        /// ROW 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgLineLoadQty);
        }

        #endregion

        #region NND STO 그룹별 적재수량 - 리스트 ROW 삭제 : btnDelete_Click()
        /// <summary>
        /// ROW 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgLineLoadQty);
        }

        #endregion

        #region NND STO 그룹별 적재수량 - 저장 : btnSave_Click()
        /// <summary>
        /// 저장 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;
            SaveData();
        }


        #endregion

        #endregion

        #region Method


        #region 타이머 재조회 : Time_Re_Search()
        /// <summary>
        /// 창고 적재현황 조회
        /// </summary>
        private void Time_Re_Search()
        {
            //극성 정보 존재 및 스태킹 완성창고
            //폴란드 조립 4동일 경우 노칭완성 창고에서는 극성을 관리 하지 않음
            if (cboElectrodeType.Items.Count > 1 || cboStockerType.SelectedValue.GetString() == "SWW")
            {
                SelectWareHouseCapacitySummary();
            }

            else
            {
                SelectWareHouseLamiSummary();
            }

            // 입고LOT 조회
            if (tabProduct.Visibility == Visibility.Visible)
            {
                SelectWareHouseProductList(dgProduct);
            }
            // 공Carrier 조회
            if (tabEmptyCarrier.Visibility == Visibility.Visible)
            {
                SelectWareHouseEmptyCarrier(sCurtrode);
                SelectWareHouseEmptyCarrierList(dgCarrierList, sCurtrode);
            }
            // 비정상 Rack 조회
            if (tabErrorCarrier.Visibility == Visibility.Visible)
            {
                SelectErrorCarrierList(dgErrorCarrier);
            }
            // 정보불일치 조회
            if (tabAbNormalCarrier.Visibility == Visibility.Visible)
            {
                SelectAbNormalCarrierList(dgAbNormalCarrier);
            }
            // 금지단리스트 조회
            if (tabAbNormalCarrier.Visibility == Visibility.Visible)
            {
                SelectProHibitList(dgProHibit);
            }
            // LayOut 조회
            if (tabLayout.Visibility == Visibility.Visible)
            {
                ShowLoadingIndicator();
                DoEvents();

                SelectMaxxyz();
                ReSetLayoutUserControl();

                if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                {
                    SelectRackInfo();
                }
                else
                {
                    HiddenLoadingIndicator();
                }
            }
            // 노칭 WO별 창고적재 수량 정보 여부
            if (tabStatusbyWorkorder.Visibility == Visibility.Visible)
            {
                SelectStatusbyWorkorder();
            }

            // 노칭 WO별 창고적재 HOLD 전극 여부
            if (tabHoldList.Visibility == Visibility.Visible)
            {
                SelectHold();
            }
            if (tabLoadQty.Visibility == Visibility.Visible)
            {
                SearchLoadQtyList();
            }
        }


        #endregion

        #region 창고 적재 현황 조회 : SelectWareHouseCapacitySummary()
        /// <summary>
        /// 창고 적재현황 조회
        /// </summary>
        private void SelectWareHouseCapacitySummary()
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_PER_WAREHOUSE_PRODUCT";

            try
            {


                DataTable dtResult = new DataTable();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);
                ShowLoadingIndicator();
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (bizResult.Rows.Count > 0)
                {
                    var queryBase = bizResult.AsEnumerable()
                    .Select(row => new
                    {
                        ElectrodeTypeCode = row.Field<string>("ELTR_TYPE_CODE"),
                        ElectrodeTypeName = row.Field<string>("ELTR_TYPE_NAME"),
                        EquipmentCode = row.Field<string>("EQPTID"),
                        EquipmentName = row.Field<string>("EQPTNAME"),
                        RackMax = row.Field<long>("RACK_MAX"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        RealCarrierCount = row.Field<long>("BBN_U_QTY"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        EmptyCarrierCount = row.Field<long>("BBN_E_QTY"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        OppositeCarrierCount = row.Field<long>("BBN_UM_QTY"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        ErrorCarrierCount = row.Field<long>("ERROR_QTY"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        AbnormalCount = row.Field<long>("ABNORM_QTY"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        ProHibitCount = row.Field<long>("PROHIBIT_QTY"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        SumCarrierCount = row.Field<long>("RACK_QTY"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        RackRate = row.Field<double>("RACK_RATE"), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                    }).Distinct();
                    // 극성별 소계
                    
                    var query = bizResult.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE"),
                        electrodeTypeName = x.Field<string>("ELTR_TYPE_NAME"),
                    }).Select(g => new
                    {

                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        ElectrodeTypeName = g.Key.electrodeTypeName,
                        RackMax = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.RackMax).Sum(),
                        RealCarrierCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.RealCarrierCount).Sum(),
                        EmptyCarrierCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.EmptyCarrierCount).Sum(),
                        OppositeCarrierCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.OppositeCarrierCount).Sum(),
                        ErrorCarrierCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.ErrorCarrierCount).Sum(),
                        AbnormalCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.AbnormalCount).Sum(),
                        ProHibitCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.ProHibitCount).Sum(),
                        SumCarrierCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.SumCarrierCount).Sum(),
                        RackRate = GetRackRate(queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.SumCarrierCount).Sum(), queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.RackMax).Sum()),
                        //LotCount = g.Sum(x => x.Field<decimal>("LOT_QTY")),
                        //HoldCount = g.Sum(x => x.Field<decimal>("LOT_HOLD_QTY")),
                        //QMSHoldCount = g.Sum(x => x.Field<decimal>("LOT_HOLD_QMS_QTY")),
                        LotCount = g.Sum(x => x.Field<long>("LOT_QTY")),  // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        HoldCount = g.Sum(x => x.Field<long>("LOT_HOLD_QTY")), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        QMSHoldCount = g.Sum(x => x.Field<long>("LOT_HOLD_QMS_QTY")), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        ProjectName = "",
                        EquipmentCode = "XXXXXXXXXX",
                        EquipmentName = g.Key.electrodeTypeName + "  " + ObjectDic.Instance.GetObjectName("소계"),
                        Count = g.Count()
                    }).ToList();

                    // 합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new
                    { }).Select(g => new
                    {
                        ElectrodeTypeCode = "ZZZZZZZZZZ",
                        ElectrodeTypeName = ObjectDic.Instance.GetObjectName("합계"),
                        RackMax = queryBase.AsQueryable().Select(s => s.RackMax).Sum(),
                        //LotCount = g.Sum(x => x.Field<decimal>("LOT_QTY")),
                        //HoldCount = g.Sum(x => x.Field<decimal>("LOT_HOLD_QTY")),
                        //QMSHoldCount = g.Sum(x => x.Field<decimal>("LOT_HOLD_QMS_QTY")),
                        LotCount = g.Sum(x => x.Field<long>("LOT_QTY")), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        HoldCount = g.Sum(x => x.Field<long>("LOT_HOLD_QTY")), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        QMSHoldCount = g.Sum(x => x.Field<long>("LOT_HOLD_QMS_QTY")), // 2024.10.23. 김영국 - DB Type형식 불일치로 Type변경.Decimal -> long
                        RealCarrierCount = queryBase.AsQueryable().Select(s => s.RealCarrierCount).Sum(),
                        EmptyCarrierCount = queryBase.AsQueryable().Select(s => s.EmptyCarrierCount).Sum(),
                        OppositeCarrierCount = queryBase.AsQueryable().Select(s => s.OppositeCarrierCount).Sum(),
                        ErrorCarrierCount = queryBase.AsQueryable().Select(s => s.ErrorCarrierCount).Sum(),
                        AbnormalCount = queryBase.AsQueryable().Select(s => s.AbnormalCount).Sum(),
                        ProHibitCount = queryBase.AsQueryable().Select(s => s.ProHibitCount).Sum(),
                        SumCarrierCount = queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum(),
                        RackRate = GetRackRate(queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum(), queryBase.AsQueryable().Select(s => s.RackMax).Sum()),
                        ProjectName = "",
                        EquipmentCode = "ZZZZZZZZZZ",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        Count = g.Count()
                    }).ToList();


                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        _dtWareHouseCapacity.Clear();
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            DataRow newRow = _dtWareHouseCapacity.NewRow();
                            newRow["ELTR_TYPE_CODE"] = bizResult.Rows[i]["ELTR_TYPE_CODE"];
                            newRow["ELTR_TYPE_NAME"] = bizResult.Rows[i]["ELTR_TYPE_NAME"];
                            newRow["EQPTID"] = bizResult.Rows[i]["EQPTID"];
                            newRow["EQPTNAME"] = bizResult.Rows[i]["EQPTNAME"];
                            newRow["PRJT_NAME"] = bizResult.Rows[i]["PRJT_NAME"];
                            newRow["RACK_MAX"] = bizResult.Rows[i]["RACK_MAX"];
                            newRow["LOT_QTY"] = bizResult.Rows[i]["LOT_QTY"];
                            newRow["LOT_HOLD_QTY"] = bizResult.Rows[i]["LOT_HOLD_QTY"];
                            newRow["LOT_HOLD_QMS_QTY"] = bizResult.Rows[i]["LOT_HOLD_QMS_QTY"];
                            newRow["BBN_U_QTY"] = bizResult.Rows[i]["BBN_U_QTY"];
                            newRow["BBN_E_QTY"] = bizResult.Rows[i]["BBN_E_QTY"];
                            newRow["BBN_UM_QTY"] = bizResult.Rows[i]["BBN_UM_QTY"];
                            newRow["ERROR_QTY"] = bizResult.Rows[i]["ERROR_QTY"];
                            newRow["ABNORM_QTY"] = bizResult.Rows[i]["ABNORM_QTY"];
                            newRow["PROHIBIT_QTY"] = bizResult.Rows[i]["PROHIBIT_QTY"];
                            newRow["RACK_RATE"] = bizResult.Rows[i]["RACK_RATE"];
                            newRow["RACK_QTY"] = bizResult.Rows[i]["RACK_QTY"];
                            _dtWareHouseCapacity.Rows.Add(newRow);
                        }

                        if (query.Any() && cboStockerType.SelectedValue.GetString() != "JTW")
                        {
                            foreach (var item in query)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["PRJT_NAME"] = item.ProjectName;
                                newRow["RACK_MAX"] = item.RackMax;
                                newRow["LOT_QTY"] = item.LotCount;
                                newRow["LOT_HOLD_QTY"] = item.HoldCount;
                                newRow["LOT_HOLD_QMS_QTY"] = item.QMSHoldCount;
                                newRow["BBN_U_QTY"] = item.RealCarrierCount;
                                newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                                newRow["BBN_UM_QTY"] = item.OppositeCarrierCount;
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["PROHIBIT_QTY"] = item.ProHibitCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }

                        if (querySum.Any())
                        {
                            foreach (var item in querySum)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["PRJT_NAME"] = item.ProjectName;
                                newRow["RACK_MAX"] = item.RackMax;
                                newRow["LOT_QTY"] = item.LotCount;
                                newRow["LOT_HOLD_QTY"] = item.HoldCount;
                                newRow["LOT_HOLD_QMS_QTY"] = item.QMSHoldCount;
                                newRow["BBN_U_QTY"] = item.RealCarrierCount;
                                newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                                newRow["BBN_UM_QTY"] = item.OppositeCarrierCount;
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["PROHIBIT_QTY"] = item.ProHibitCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }
                    }

                    if (CommonVerify.HasTableRow(_dtWareHouseCapacity))
                    {
                        dtResult = (from t in _dtWareHouseCapacity.AsEnumerable()
                                    orderby t.Field<string>("ELTR_TYPE_CODE") ascending, t.Field<string>("EQPTID")
                                    select t).CopyToDataTable();
                    }
                    else
                    {
                        dtResult = bizResult;
                    }
                    Util.GridSetData(dgCapacitySummary, dtResult, null, true);

                }


                HiddenLoadingIndicator();


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 창고 적재 현황 조회 (라미) : SelectWareHouseLamiSummary()
        /// <summary>
        /// 라미 창고 집계 조회
        /// </summary>
        private void SelectWareHouseLamiSummary()
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_PER_NON_ELTR_TYPE_STO";
            try
            {
                Util.gridClear(dgLamiCapacitySummary);
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);


                ShowLoadingIndicator();
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (bizResult.Rows.Count > 0)
                {
                    // 라미대기 창고 인 경우 합계를 구하기 위해서 용량, 공Carrier, 오류Carrier, 적재율을 Distinct 처리 해야 함.
                    var queryBase = bizResult.AsEnumerable()
                        .Select(row => new
                        {
                            EquipmentCode = row.Field<string>("EQPTID"),
                            EquipmentName = row.Field<string>("EQPTNAME"),
                            RackMaxQty = row.Field<double>("RACK_MAX"),
                            //EmptyQty = row.Field<double>("EMPTY_QTY"),
                            ErrorQty = row.Field<double>("ERROR_QTY"),
                            AbNormalQty = row.Field<double>("ABNORM_QTY"),
                            ProhibitQty = row.Field<double>("PROHIBIT_QTY"),
                            SumCarrierCount = row.Field<double>("RACK_QTY"),
                            RackRate = row.Field<double>("RACK_RATE")

                        }).Distinct();

                    //합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        EquipmentCode = "ZZZZZZZZZZ",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        ProjectName = "",
                        LotCountCathode = g.Sum(x => x.Field<double>("LOT_QTY_C")),
                        HoldCountCathode = g.Sum(x => x.Field<double>("HOLD_QTY_C")),
                        QmsHoldCountCathode = g.Sum(x => x.Field<double>("HOLD_QMS_QTY_C")),
                        LotCountAnode = g.Sum(x => x.Field<double>("LOT_QTY_A")),
                        HoldCountAnode = g.Sum(x => x.Field<double>("HOLD_QTY_A")),
                        QmsHoldCountAnode = g.Sum(x => x.Field<double>("HOLD_QMS_QTY_A")),
                        EmptyCstCountAnode = g.Sum(x => x.Field<double>("EMPTY_CST_QTY_A")),
                        EmptyCstCountCathode = g.Sum(x => x.Field<double>("EMPTY_CST_QTY_C")),
                        ErrorCount = queryBase.AsQueryable().Select(s => s.ErrorQty).Sum(),
                        RackMaxCount = queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum(),
                        SumCarrierCount = g.Sum(x => x.Field<double>("RACK_QTY")),                        
                        AbNormalQty = queryBase.AsQueryable().Select(s => s.AbNormalQty).Sum(),
                        ProhibitQty = queryBase.AsQueryable().Select(s => s.ProhibitQty).Sum(),
                        RackRate = GetRackRate(queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum() + queryBase.AsQueryable().Select(s => s.ProhibitQty).Sum(), queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum()),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (querySum != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = querySum.EquipmentCode;
                        newRow["EQPTNAME"] = querySum.EquipmentName;
                        newRow["PRJT_NAME"] = querySum.ProjectName;
                        newRow["RACK_MAX"] = querySum.RackMaxCount;
                        newRow["LOT_QTY_C"] = querySum.LotCountCathode;
                        newRow["HOLD_QTY_C"] = querySum.HoldCountCathode;
                        newRow["LOT_QTY_A"] = querySum.LotCountAnode;
                        newRow["HOLD_QTY_A"] = querySum.HoldCountAnode;
                        newRow["EMPTY_CST_QTY_A"] = querySum.EmptyCstCountAnode;
                        newRow["EMPTY_CST_QTY_C"] = querySum.EmptyCstCountCathode;
                        newRow["ERROR_QTY"] = querySum.ErrorCount;
                        newRow["ABNORM_QTY"] = querySum.AbNormalQty;
                        newRow["PROHIBIT_QTY"] = querySum.ProhibitQty;
                        newRow["RACK_RATE"] = querySum.RackRate;
                        newRow["RACK_QTY"] = querySum.SumCarrierCount;
                        newRow["HOLD_QMS_QTY_C"] = querySum.QmsHoldCountCathode;
                        newRow["HOLD_QMS_QTY_A"] = querySum.QmsHoldCountAnode;
                        bizResult.Rows.Add(newRow);
                    }
                    Util.GridSetData(dgLamiCapacitySummary, bizResult, null, true);
                    //Util.GridSetData(dgCapacitySummary, bizResult, null, true);
                    HiddenLoadingIndicator();
                }
                else
                {
                    HiddenLoadingIndicator();
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 입고 LOT 조회  : SelectWareHouseProductList(), SelectWareHouseProductList()
        /// <summary>
        /// 입고LOT 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectWareHouseProductList(C1DataGrid dg)
        {

            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST";

            //string bizRuleName = "DA_MHS_SEL_LOT_CONF_LIST_NORM_USING_CST";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE_LOT", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("QMS_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));


                DataRow dr = inTable.NewRow();

                if (dg.Name == "dgRackInfo")
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue;
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                    dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                    //dr["LOTID"] = _selectedLotIdByRackInfo;
                    dr["EQPTID"] = _selectedEquipmentCode;
                    dr["RACK_ID"] = _selectedRackIdByRackInfo;
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue;
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                    dr["ELTR_TYPE_CODE_LOT"] = _selectedLotElectrodeTypeCode;
                    dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                    dr["EQPTID"] = _selectedEquipmentCode;
                    dr["PRJT_NAME"] = _selectedProjectName;
                    dr["WIPHOLD"] = _selectedWipHold;
                    dr["QMS_HOLD_FLAG"] = _selectedQmsHold;
                    dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTID"] = _selectedLotIdByRackInfo;


                }
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }


                    if (bizResult.Rows.Count > 0)
                    {
                        // 금지단 ROW는 삭제
                        //bizResult.Select("RACK_STAT_CODE = 'DISABLE'").ToList<DataRow>().ForEach(row => row.Delete());
                        //bizResult.AcceptChanges();

                        DataTable GrTray = bizResult.Clone();

                        List<string> sIdList = bizResult.AsEnumerable().Select(c => c.Field<string>("SKID_ID")).Distinct().ToList();

                        Int32 _Rownum = 1;
                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = GrTray.NewRow();
                            drIndata["ROW_NUM"] = _Rownum;
                            drIndata["SKID_ID"] = id;
                            GrTray.Rows.Add(drIndata);
                            _Rownum = _Rownum + 1;
                        }

                        for (int i = 0; i < GrTray.Rows.Count; i++)
                        {

                            for (int j = 0; j < bizResult.Rows.Count; j++)
                            {
                                if (GrTray.Rows[i]["SKID_ID"].ToString() == bizResult.Rows[j]["SKID_ID"].ToString())
                                {
                                    bizResult.Rows[j]["ROW_NUM"] = GrTray.Rows[i]["ROW_NUM"];
                                }
                            }
                        }
                    }

                    Util.GridSetData(dg, bizResult, null, true);

                    if (cboStockerType.SelectedValue.ToString() == "SWW")
                    {
                        if (dg.Name == "dgRackInfo")
                        {
                            dgRackInfo.MergingCells -= dgRackInfo_MergingCells;
                            dgRackInfo.MergingCells += dgRackInfo_MergingCells;
                        }
                        else
                        {
                            dgProduct.MergingCells -= dgProduct_MergingCells;
                            dgProduct.MergingCells += dgProduct_MergingCells;
                        }

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
        /// 입고LOT 조회 : 장고 적재현황 클릭시 조회
        /// </summary>
        /// <param name="actionCompleted"></param>
        private void SelectWareHouseProductList(Action<DataTable, Exception> actionCompleted = null)
        {


            string bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE_LOT", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE_LOT"] = _selectedLotElectrodeTypeCode;
                dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["PRJT_NAME"] = _selectedProjectName;
                dr["WIPHOLD"] = _selectedWipHold;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {

                        // 금지단 ROW는 삭제
                        //bizResult.Select("RACK_STAT_CODE = 'DISABLE'").ToList<DataRow>().ForEach(row => row.Delete());
                        //bizResult.AcceptChanges();
                        DataTable GrTray = bizResult.Clone();

                        List<string> sIdList = bizResult.AsEnumerable().Select(c => c.Field<string>("SKID_ID")).Distinct().ToList();

                        Int32 _Rownum = 1;
                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = GrTray.NewRow();
                            drIndata["ROW_NUM"] = _Rownum;
                            drIndata["SKID_ID"] = id;
                            GrTray.Rows.Add(drIndata);
                            _Rownum = _Rownum + 1;
                        }

                        for (int i = 0; i < GrTray.Rows.Count; i++)
                        {

                            for (int j = 0; j < bizResult.Rows.Count; j++)
                            {
                                if (GrTray.Rows[i]["SKID_ID"].ToString() == bizResult.Rows[j]["SKID_ID"].ToString())
                                {
                                    bizResult.Rows[j]["ROW_NUM"] = GrTray.Rows[i]["ROW_NUM"];
                                }
                            }
                        }
                    }
                    actionCompleted?.Invoke(bizResult, bizException);

                    if (cboStockerType.SelectedValue.ToString() == "SWW")
                    {

                        dgProduct.MergingCells -= dgProduct_MergingCells;
                        dgProduct.MergingCells += dgProduct_MergingCells;
                    }

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 공 Carrier 정보 조회 : SelectWareHouseEmptyCarrier(), SelectWareHouseEmptyCarrierList()

        /// <summary>
        /// 공 Carrier 그룹 조회
        /// </summary>
        private void SelectWareHouseEmptyCarrier()
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_EMPTY_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEmptyCarrier, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseEmptyCarrier(string sCST_ELTR_CODE)
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_EMPTY_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("CST_ELTR_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["CST_ELTR_CODE"] = sCST_ELTR_CODE;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEmptyCarrier, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 공 Carrier 리스트 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectWareHouseEmptyCarrierList(C1DataGrid dg)
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_EMPTY_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("SKID_ID", typeof(string));
                //inTable.Columns.Add("BOBBIN_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["SKID_ID"] = _selectedSkIdIdByRackInfo;
                // dr["BOBBIN_ID"] = _selectedBobbinIdByRackInfo;
                dr["RACK_ID"] = _selectedRackIdByRackInfo;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {

                        // 금지단 ROW는 삭제
                        //bizResult.Select("RACK_STAT_CODE = 'DISABLE'").ToList<DataRow>().ForEach(row => row.Delete());
                        //bizResult.AcceptChanges();

                        DataTable GrTray = bizResult.Clone();

                        List<string> sIdList = bizResult.AsEnumerable().Select(c => c.Field<string>("SKID_ID")).Distinct().ToList();

                        Int32 _Rownum = 1;
                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = GrTray.NewRow();
                            drIndata["ROW_NUM"] = _Rownum;
                            drIndata["SKID_ID"] = id;
                            GrTray.Rows.Add(drIndata);
                            _Rownum = _Rownum + 1;
                        }

                        for (int i = 0; i < GrTray.Rows.Count; i++)
                        {

                            for (int j = 0; j < bizResult.Rows.Count; j++)
                            {
                                if (GrTray.Rows[i]["SKID_ID"].ToString() == bizResult.Rows[j]["SKID_ID"].ToString())
                                {
                                    bizResult.Rows[j]["ROW_NUM"] = GrTray.Rows[i]["ROW_NUM"];
                                }
                            }
                        }

                        Util.GridSetData(dg, bizResult, null, true);

                        if (cboStockerType.SelectedValue.ToString() == "SWW")
                        {
                            if (dg.Name == "dgRackInfo")
                            {
                                string[] sColumnName = new string[] { "EQPTNAME", "RACK_ID", "RACK_NAME", "ROW_NUM", "CSTINDTTM", "CARRIERID" };
                                _Util.SetDataGridMergeExtensionCol(dgRackInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            }
                            else
                            {
                                dgCarrierList.MergingCells -= dgCarrierList_MergingCells;
                                dgCarrierList.MergingCells += dgCarrierList_MergingCells;
                            }

                        }

                    }


                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseEmptyCarrierList(C1DataGrid dg, string sCST_ELTR_CODE)
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_EMPTY_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("SKID_ID", typeof(string));
                //inTable.Columns.Add("BOBBIN_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("CST_ELTR_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["SKID_ID"] = _selectedSkIdIdByRackInfo;
                // dr["BOBBIN_ID"] = _selectedBobbinIdByRackInfo;
                dr["RACK_ID"] = _selectedRackIdByRackInfo;
                dr["CST_ELTR_CODE"] = sCST_ELTR_CODE;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {

                        // 금지단 ROW는 삭제
                        //bizResult.Select("RACK_STAT_CODE = 'DISABLE'").ToList<DataRow>().ForEach(row => row.Delete());
                        //bizResult.AcceptChanges();

                        DataTable GrTray = bizResult.Clone();

                        List<string> sIdList = bizResult.AsEnumerable().Select(c => c.Field<string>("SKID_ID")).Distinct().ToList();

                        Int32 _Rownum = 1;
                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = GrTray.NewRow();
                            drIndata["ROW_NUM"] = _Rownum;
                            drIndata["SKID_ID"] = id;
                            GrTray.Rows.Add(drIndata);
                            _Rownum = _Rownum + 1;
                        }

                        for (int i = 0; i < GrTray.Rows.Count; i++)
                        {

                            for (int j = 0; j < bizResult.Rows.Count; j++)
                            {
                                if (GrTray.Rows[i]["SKID_ID"].ToString() == bizResult.Rows[j]["SKID_ID"].ToString())
                                {
                                    bizResult.Rows[j]["ROW_NUM"] = GrTray.Rows[i]["ROW_NUM"];
                                }
                            }
                        }

                        Util.GridSetData(dg, bizResult, null, true);

                        if (cboStockerType.SelectedValue.ToString() == "SWW")
                        {
                            if (dg.Name == "dgRackInfo")
                            {
                                string[] sColumnName = new string[] { "EQPTNAME", "RACK_ID", "RACK_NAME", "ROW_NUM", "CSTINDTTM", "CARRIERID" };
                                _Util.SetDataGridMergeExtensionCol(dgRackInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            }
                            else
                            {
                                dgCarrierList.MergingCells -= dgCarrierList_MergingCells;
                                dgCarrierList.MergingCells += dgCarrierList_MergingCells;
                            }

                        }

                    }


                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 비정상 Rack 조회 : SelectErrorCarrierList()

        /// <summary>
        /// 비정상 RACK 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectErrorCarrierList(C1DataGrid dg)
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_ABNORM_RACK";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["RACK_ID"] = _selectedRackIdByRackInfo;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 정보불일치 리스트 조회 : SelectAbNormalCarrierList()
        /// <summary>
        /// 정보불일치
        /// </summary>
        /// <param name="dg"></param>
        private void SelectAbNormalCarrierList(C1DataGrid dg)
        {
            //string bizRuleName = string.Empty;

            //if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW"&& cboArea.SelectedValue.ToString() == "A7")
            //{
            //    bizRuleName = "BR_MCS_SEL_WAREHOUSE_ABNORM_LIST_NWW";
            //}
            //else
            //{
            //    bizRuleName = "BR_MCS_SEL_WAREHOUSE_ABNORM_LIST";
            //}

            //const string bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_ABNORM_CST";

            const string bizRuleName =  "BR_MHS_SEL_STO_INVENT_LIST_ABNORM_CST";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["RACK_ID"] = _selectedRackIdByRackInfo;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    // 금지단 ROW는 삭제
                    //bizResult.Select("RACK_STAT_CODE = 'DISABLE'").ToList<DataRow>().ForEach(row => row.Delete());
                    //bizResult.AcceptChanges();
                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 금지단 리스트 조회 : SelectAbNormalCarrierList()
        /// <summary>
        /// 금지단 리스트 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectProHibitList(C1DataGrid dg)
        {

            //const string bizRuleName = "BR_MHS_SEL_STO_INVENT_PROHIBIT_CST";


            const string bizRuleName =  "BR_MHS_SEL_STO_INVENT_PROHIBIT_CST";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion



        #region 창고별 WO별 실적 조회 : SelectStatusbyWorkorder()
        /// <summary>
        // 창고별 WO별 실적 조회
        /// </summary>
        private void SelectStatusbyWorkorder()
        {
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SUMMARY_BY_ASSY_WO";
            const string bizRuleName = "BR_MHS_SEL_WAREHOUSE_SUMMARY_BY_ASSY_WO";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                //inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgStatusbyWorkorder, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 재고 HOLD 현황 조회 : SelectHold()
        /// <summary>
        ///  재고 HOLD 현황 조회
        /// </summary>
        private void SelectHold()
        {
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SUMMARY_BY_HOLD";
            const string bizRuleName = "BR_MHS_SEL_WAREHOUSE_SUMMARY_BY_HOLD";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgHold, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region LayOut Tab 모니터링 
        /// <summary>
        /// Layout 정보 조회 바인딩
        /// </summary>
        private void SelectRackInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                //const string bizRuleName = "BR_MHS_SEL_WAREHOUSE_RACK_LAYOUT";

                const string bizRuleName =  "BR_MHS_SEL_WAREHOUSE_RACK_LAYOUT";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(dr);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                HideAndClearAllRack();
                Util.gridClear(dgRackInfo);

                if (CommonVerify.HasTableRow(bizResult))
                {
                    foreach (DataRow item in bizResult.Rows)
                    {
                        int x = GetXPosition(item["Z_PSTN"].ToString());
                        int y = int.Parse(item["Y_PSTN"].ToString()) - 1;

                        UcRackLayout ucRackLayout = item["X_PSTN"].ToString() == "1" ? _ucRackLayout1[x][y] : _ucRackLayout2[x][y];

                        if (ucRackLayout == null) continue;

                        ucRackLayout.RackId = item["RACK_ID"].GetString();
                        ucRackLayout.Row = int.Parse(item["Z_PSTN"].GetString());
                        ucRackLayout.Col = int.Parse(item["Y_PSTN"].GetString());
                        ucRackLayout.Stair = int.Parse(item["X_PSTN"].GetString());
                        ucRackLayout.RackStateCode = item["STSTUS"].GetString();
                        ucRackLayout.ProjectName = item["PRJT_NAME"].GetString();
                        ucRackLayout.LotId = item["LOTID"].GetString();
                        //ucRackLayout.SkidCarrierProductCode = item["SD_CSTPROD"].GetString();
                        //ucRackLayout.SkidCarrierProductName = item["SD_CSTPROD_NAME"].GetString();
                        ucRackLayout.SkidCarrierCode = item["SD_CSTID"].GetString();
                        //ucRackLayout.BobbinCarrierProductCode = item["BB_CSTPROD"].GetString();
                        //ucRackLayout.BobbinCarrierProductName = item["BB_CSTPROD_NAME"].GetString();
                        ucRackLayout.BobbinCarrierCode = item["BB_CSTID"].GetString();
                        ucRackLayout.WipHold = item["WIPHOLD"].GetString();
                        //ucRackLayout.CarrierDefectFlag = item["CST_DFCT_FLAG"].GetString();
                        ucRackLayout.LegendColor = item["COLOR"].GetString();
                        //ucRackLayout.SkidType = item["SKID_GUBUN"].GetString();
                        ucRackLayout.AbnormalTransferReasonCode = item["ABNORM_TRF_RSN_CODE"].GetString();
                        ucRackLayout.LegendColorType = item["COLOR_GUBUN"].GetString();
                        ucRackLayout.HoldFlag = item["HOLD_FLAG"].GetString();
                        ucRackLayout.Visibility = Visibility.Visible;
                    }
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// LayOut 탭 USER Control 포지션 위치
        /// </summary>
        /// <param name="zPosition"></param>
        /// <returns></returns>
        private int GetXPosition(string zPosition)
        {
            int xposition = _maxRowCount == Convert.ToInt16(zPosition) ? 0 : _maxRowCount - Convert.ToInt16(zPosition);
            return xposition;
        }

        /// <summary>
        /// LayOut 탭 연, 단 맥스값
        /// </summary>
        private void SelectMaxxyz()
        {
            try
            {
                const string bizRuleName = "DA_MHS_SEL_RACK_MAX_XYZ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _selectedEquipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    if (string.IsNullOrEmpty(searchResult.Rows[0][2].GetString()) || string.IsNullOrEmpty(searchResult.Rows[0][1].GetString()))
                    {
                        _maxRowCount = 0;
                        _maxColumnCount = 0;
                        return;
                    }

                    _maxRowCount = Convert.ToInt32(searchResult.Rows[0][2].GetString());
                    _maxColumnCount = Convert.ToInt32(searchResult.Rows[0][1].GetString());
                }
                else
                {
                    _maxRowCount = 0;
                    _maxColumnCount = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// LayOut 탭 UserControl 초기화
        /// </summary>
        private void InitializeRackUserControl()
        {
            grdColumn1.Children.Clear();
            grdColumn2.Children.Clear();

            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            if (grdColumn1.ColumnDefinitions.Count > 0) grdColumn1.ColumnDefinitions.Clear();
            if (grdColumn1.RowDefinitions.Count > 0) grdColumn1.RowDefinitions.Clear();

            if (grdColumn2.ColumnDefinitions.Count > 0) grdColumn2.ColumnDefinitions.Clear();
            if (grdColumn2.RowDefinitions.Count > 0) grdColumn2.RowDefinitions.Clear();

            if (grdStair1.ColumnDefinitions.Count > 0) grdStair1.ColumnDefinitions.Clear();
            if (grdStair1.RowDefinitions.Count > 0) grdStair1.RowDefinitions.Clear();

            if (grdStair2.ColumnDefinitions.Count > 0) grdStair2.ColumnDefinitions.Clear();
            if (grdStair2.RowDefinitions.Count > 0) grdStair2.RowDefinitions.Clear();

            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();
        }

        /// <summary>
        /// LayOut 탭  Column 정의 
        /// </summary>
        private void MakeColumnDefinition()
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            int colIndex = 0;

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock2, colIndex);
                grdStair2.Children.Add(textBlock2);
                colIndex++;
            }
        }

        /// <summary>
        /// LayOut 탭 Row 정의
        /// </summary>
        private void MakeRowDefinition()
        {
            // 단 Row 생성
            ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                grdColumn1.RowDefinitions.Add(rowDefinition1);
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdColumn2.RowDefinitions.Add(rowDefinition2);
            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                TextBlock textBlock1 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.SetValue(Grid.RowProperty, i);
                textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock1.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn1.Children.Add(textBlock1);

                TextBlock textBlock2 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.SetValue(Grid.RowProperty, i);
                textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock2.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn2.Children.Add(textBlock2);
            }
        }

        /// <summary>
        /// LayOut 탭 UserControl 위치 정의
        /// </summary>
        private void PrepareRackStair()
        {
            _ucRackLayout1 = new UcRackLayout[_maxRowCount][];
            _ucRackLayout2 = new UcRackLayout[_maxRowCount][];

            for (int r = 0; r < _ucRackLayout1.Length; r++)
            {
                _ucRackLayout1[r] = new UcRackLayout[_maxColumnCount];
                _ucRackLayout2[r] = new UcRackLayout[_maxColumnCount];

                for (int c = 0; c < _ucRackLayout1[r].Length; c++)
                {
                    UcRackLayout ucRackLayout1 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout1.Checked += UcRackLayout1_Checked;
                    //ucPancakeRackStair1.Click += UcRackStair1_Click;
                    //ucPancakeRackStair1.DoubleClick += UcRackStair1_DoubleClick;
                    _ucRackLayout1[r][c] = ucRackLayout1;
                }

                for (int c = 0; c < _ucRackLayout2[r].Length; c++)
                {
                    UcRackLayout ucRackLayout2 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout2.Checked += UcRackLayout2_Checked;
                    //ucPancakeRackStair2.Click += UcRackStair2_Click;
                    //ucPancakeRackStair2.DoubleClick += UcRackStair2_DoubleClick;
                    _ucRackLayout2[r][c] = ucRackLayout2;
                }
            }

        }
        /// <summary>
        /// LayOut 탭 UserControl 길이 셋팅
        /// </summary>
        private void PrepareRackStairLayout()
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();

            BrushConverter converter = new BrushConverter();
            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);

                Border border = new Border();
                if (i == _maxColumnCount - 1)
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair1.Children.Add(border);

                Border border1 = new Border();
                if (i == _maxColumnCount - 1)
                {
                    border1.SetValue(Grid.RowProperty, 0);
                    border1.SetValue(Grid.ColumnProperty, i);
                    border1.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border1.SetValue(Grid.RowProperty, 0);
                    border1.SetValue(Grid.ColumnProperty, i);
                    border1.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }

                grdRackstair2.Children.Add(border1);

            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);

                Border border = new Border();
                if (i == _maxRowCount - 1)
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair1.Children.Add(border);


                Border border1 = new Border();
                if (i == _maxRowCount - 1)
                {
                    border1.SetValue(Grid.RowProperty, i);
                    border1.SetValue(Grid.ColumnProperty, 0);
                    border1.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border1.SetValue(Grid.RowProperty, i);
                    border1.SetValue(Grid.ColumnProperty, 0);
                    border1.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair2.Children.Add(border1);
            }

            for (int row = 0; row < _maxRowCount; row++)
            {
                for (int col = 0; col < _maxColumnCount; col++)
                {
                    Grid.SetRow(_ucRackLayout1[row][col], row);
                    Grid.SetColumn(_ucRackLayout1[row][col], col);
                    grdRackstair1.Children.Add(_ucRackLayout1[row][col]);

                    Grid.SetRow(_ucRackLayout2[row][col], row);
                    Grid.SetColumn(_ucRackLayout2[row][col], col);
                    grdRackstair2.Children.Add(_ucRackLayout2[row][col]);
                }
            }
        }
        /// <summary>
        /// LayOut 탭 UserControl 재정의
        /// </summary>
        private void ReSetLayoutUserControl()
        {
            _dtRackInfo.Clear();
            InitializeRackUserControl();

            MakeRowDefinition();
            MakeColumnDefinition();
            PrepareRackStair();
            PrepareRackStairLayout();
        }

        /// <summary>
        /// LayOut 탭 상단 Grid 컬럼 셋팅
        /// </summary>
        /// <param name="type"></param>
        private void GetLayoutGridColumns(string type)
        {
            for (int i = dgRackInfo.Columns.Count - 1; i >= 0; i--)
            {
                dgRackInfo.Columns.RemoveAt(i);
            }

            dgRackInfo.Refresh();

            switch (type)
            {
                case "1":
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "NO",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_ID",
                        Header = ObjectDic.Instance.GetObjectName("Rack ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Rack"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "UPDDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("UPDDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CARRIERID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CARRIERID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.IsReadOnly = true;
                    break;
                case "3":
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "NO",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_ID",
                        Header = ObjectDic.Instance.GetObjectName("Rack ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Rack"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "UPDDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("UPDDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });


                    dgRackInfo.IsReadOnly = true;
                    break;

                case "4":
                case "5":
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ROW_NUM",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_ID",
                        Header = ObjectDic.Instance.GetObjectName("Rack ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("RACK명"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTINDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("CSTINDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CARRIERID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CARRIERID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "BOBBIN_ID",
                        Header = ObjectDic.Instance.GetObjectName("보빈 ID"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTPROD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("사용자재"),
                        Binding = new Binding() { Path = new PropertyPath("CSTPROD_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTPROD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Carrier유형"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_TYPE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    //_util.SetDataGridMergeExtensionCol(dgRackInfo, new string[] { "ELTR_TYPE_NAME"}, DataGridMergeMode.VERTICALHIERARCHI);
                    dgRackInfo.IsReadOnly = true;

                    dgRackInfo.Columns["CARRIERID"].Visibility = Visibility.Collapsed;

                    // 노칭대기창고
                    if (cboStockerType.SelectedValue.GetString() == "NWW")
                    {
                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["CARRIERID"].Visibility = Visibility.Visible;
                        }


                    }
                    else if (cboStockerType.SelectedValue.GetString() == "NPW")
                    {
                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["CARRIERID"].Visibility = Visibility.Visible;
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "LWW")
                    {
                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["CARRIERID"].Visibility = Visibility.Visible;
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        dgRackInfo.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                        dgRackInfo.Columns["CARRIERID"].Header = ObjectDic.Instance.GetObjectName("Group Carrier ID");
                        dgRackInfo.Columns["CARRIERID"].Visibility = Visibility.Visible;
                    }

                    //점보롤 창고
                    else if (cboStockerType.SelectedValue.GetString() == "JRW")
                    {
                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["CARRIERID"].Visibility = Visibility.Visible;
                        }

                    }
                    else if (cboStockerType.SelectedValue.GetString() == "PCW")
                    {
                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["CARRIERID"].Visibility = Visibility.Visible;
                        }

                    }

                    break;


                case "2":

                    dgRackInfo.Columns.Add(new DataGridNumericColumn()
                    {
                        Name = "ROW_NUM",
                        Header = ObjectDic.Instance.GetObjectName("순위"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new DataGridNumericColumn()
                    {
                        Name = "FAST_TRACK_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("FastTrack"),
                        Binding = new Binding() { Path = new PropertyPath("FAST_TRACK_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("RACK명"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTINDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("CSTINDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQSGNAME",
                        Header = ObjectDic.Instance.GetObjectName("LINE"),
                        Binding = new Binding() { Path = new PropertyPath("EQSGNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SKID_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("SKID_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "BOBBIN_ID",
                        Header = ObjectDic.Instance.GetObjectName("보빈 ID"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });


                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "LOTID",
                        Header = ObjectDic.Instance.GetObjectName("LOT ID"),
                        Binding = new Binding() { Path = new PropertyPath("LOTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "INPUT_LOTID",
                        Header = ObjectDic.Instance.GetObjectName("투입LOT"),
                        Binding = new Binding() { Path = new PropertyPath("INPUT_LOTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "LOTYNAME",
                        Header = ObjectDic.Instance.GetObjectName("LOT유형"),
                        Binding = new Binding() { Path = new PropertyPath("LOTYNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = "WIPQTY",
                        Header = ObjectDic.Instance.GetObjectName("수량"),
                        Binding = new Binding() { Path = new PropertyPath("WIPQTY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Format = "#,##0",
                        HorizontalAlignment = HorizontalAlignment.Right
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "UNIT_CODE",
                        Header = ObjectDic.Instance.GetObjectName("단위"),
                        Binding = new Binding() { Path = new PropertyPath("UNIT_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRJT_NAME",
                        Header = ObjectDic.Instance.GetObjectName("프로젝트명"),
                        Binding = new Binding() { Path = new PropertyPath("PRJT_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HALF_SLIT_SIDE",
                        Header = ObjectDic.Instance.GetObjectName("무지부"),
                        Binding = new Binding() { Path = new PropertyPath("HALF_SLIT_SIDE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ROLL_DIRECTION",
                        Header = ObjectDic.Instance.GetObjectName("권취방향"),
                        Binding = new Binding() { Path = new PropertyPath("ROLL_DIRECTION"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ELTR_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("극성"),
                        Binding = new Binding() { Path = new PropertyPath("ELTR_TYPE_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRODID",
                        Header = ObjectDic.Instance.GetObjectName("제품"),
                        Binding = new Binding() { Path = new PropertyPath("PRODID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });


                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "VLD_DATE",
                        Header = ObjectDic.Instance.GetObjectName("유효일자"),
                        Binding = new Binding() { Path = new PropertyPath("VLD_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PAST_DAY",
                        Header = ObjectDic.Instance.GetObjectName("경과일수"),
                        Binding = new Binding() { Path = new PropertyPath("PAST_DAY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PROD_VER_CODE",
                        Header = ObjectDic.Instance.GetObjectName("버전"),
                        Binding = new Binding() { Path = new PropertyPath("PROD_VER_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ELTR_GRD_CODE",
                        Header = ObjectDic.Instance.GetObjectName("ELTR_GRD_CODE"),
                        Binding = new Binding() { Path = new PropertyPath("ELTR_GRD_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "IQC_JUDGEMENT",
                        Header = ObjectDic.Instance.GetObjectName("IQC 검사결과"),
                        Binding = new Binding() { Path = new PropertyPath("IQC_JUDGEMENT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "COATING_NAME",
                        Header = ObjectDic.Instance.GetObjectName("생산설비(COATING)"),
                        Binding = new Binding() { Path = new PropertyPath("COATING_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "JUDG_TYPE",
                        Header = ObjectDic.Instance.GetObjectName("QA 검사결과"),
                        Binding = new Binding() { Path = new PropertyPath("JUDG_TYPE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "QMS_HOLD_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("QA Hold 여부"),
                        Binding = new Binding() { Path = new PropertyPath("QMS_HOLD_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "FINL_JUDG_NOTE",
                        Header = ObjectDic.Instance.GetObjectName("QA 검사비고"),
                        Binding = new Binding() { Path = new PropertyPath("FINL_JUDG_NOTE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "VD_QA_RESULT",
                        Header = ObjectDic.Instance.GetObjectName("VD검사결과"),
                        Binding = new Binding() { Path = new PropertyPath("VD_QA_RESULT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SPCL_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("특별관리여부"),
                        Binding = new Binding() { Path = new PropertyPath("SPCL_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RSV_EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("목적지 설비명"),
                        Binding = new Binding() { Path = new PropertyPath("RSV_EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "WIPHOLD",
                        Header = ObjectDic.Instance.GetObjectName("HOLD 여부"),
                        Binding = new Binding() { Path = new PropertyPath("WIPHOLD"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD사유"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_NOTE",
                        Header = ObjectDic.Instance.GetObjectName("HOLD비고"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_NOTE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_DTTM",
                        Header = ObjectDic.Instance.GetObjectName("HOLD시간"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_DTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_USERNAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD등록자"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_USERNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ACTION_USERNAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD담당자"),
                        Binding = new Binding() { Path = new PropertyPath("ACTION_USERNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_HOLD_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("설비 보류 유형 코드"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_HOLD_TYPE_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_HOLD_CNFM_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("설비 보류 확인 여부"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_HOLD_CNFM_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = "DFCT_TAG_QTY",
                        Header = ObjectDic.Instance.GetObjectName("불량태그수"),
                        Binding = new Binding() { Path = new PropertyPath("DFCT_TAG_QTY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Format = "#,##0",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.IsReadOnly = true;

                    // 극성 콤보박스
                    //SetElectrodeTypeCombo(cboElectrodeType);
                    //cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;

                    //불량태그수
                    dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;

                    //FAST TRACK
                    dgRackInfo.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Collapsed;
                    //무지부, 권취방향
                    dgRackInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["ROLL_DIRECTION"].Visibility = Visibility.Collapsed;
                    //생산설비(COATING) 
                    dgRackInfo.Columns["COATING_NAME"].Visibility = Visibility.Collapsed;
                    //QA 검사결과, QA Hold 여부, QA 검사비고
                    dgRackInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Collapsed;
                    //VD검사결과
                    dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
                    //IQC 검사결과
                    dgRackInfo.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Collapsed;
                    //투입LOT
                    dgRackInfo.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;

                    //극성
                    dgRackInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;
                    //SKID 
                    dgRackInfo.Columns["SKID_ID"].Visibility = Visibility.Collapsed;

                    // 집계 극성정보
                    dgRackInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Collapsed;

                    //스태킹 완성창고일 경우  컬럼명 변경
                    dgRackInfo.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                    dgRackInfo.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("보빈 ID");

                    // 노칭대기창고
                    if (cboStockerType.SelectedValue.GetString() == "NWW")
                    {
                        dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                        if (ChkIQCView())
                        {
                            dgRackInfo.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Visible;
                        }
                        dgRackInfo.Columns["COATING_NAME"].Visibility = Visibility.Visible;

                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["SKID_ID"].Visibility = Visibility.Visible;
                        }


                    }
                    else if (cboStockerType.SelectedValue.GetString() == "NPW")
                    {
                        if (cboElectrodeType.Items.Count > 1)
                        {
                            dgRackInfo.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                            dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                            dgRackInfo.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                            dgRackInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgRackInfo.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                            dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                            dgRackInfo.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                        }

                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["SKID_ID"].Visibility = Visibility.Visible;
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "LWW")
                    {
                        dgRackInfo.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;

                        dgRackInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        if (ChkDefectTag())
                            dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;

                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["SKID_ID"].Visibility = Visibility.Visible;
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "SWW")
                    {
                        dgRackInfo.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;

                        //스태킹 완성창고일 경우  컬럼명 변경
                        dgRackInfo.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("Group Carrier ID");
                        dgRackInfo.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                        dgRackInfo.Columns["SKID_ID"].Visibility = Visibility.Visible;


                    }

                    //점보롤 창고
                    else if (cboStockerType.SelectedValue.GetString() == "JRW")
                    {
                        dgRackInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["ROLL_DIRECTION"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;

                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["SKID_ID"].Visibility = Visibility.Visible;

                        }

                    }
                    else if (cboStockerType.SelectedValue.GetString() == "PCW")
                    {
                        dgRackInfo.Columns["JUDG_TYPE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["FINL_JUDG_NOTE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["COATING_NAME"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["ELTR_TYPE_NAME"].Visibility = Visibility.Visible;

                        if (_Skid_Use_Chk == "Y")
                        {
                            dgRackInfo.Columns["SKID_ID"].Visibility = Visibility.Visible;

                        }

                    }

                    break;

            }
        }
        /// <summary>
        /// LayOut 탭 상단 Grid에 대한 
        /// </summary>
        /// <param name="type"></param>
        private void SelectLayoutGrid(string type)
        {
            switch (type)
            {
                case "1":   // 정보불일치
                    SelectAbNormalCarrierList(dgRackInfo);
                    break;
                case "2":   // 실보빈(LOT존재)
                    SelectWareHouseProductList(dgRackInfo);
                    break;
                case "3":   // 비정상Rack
                    SelectErrorCarrierList(dgRackInfo);
                    break;
                case "4":   // 공 Carrier
                case "5":
                    SelectWareHouseEmptyCarrierList(dgRackInfo);
                    break;
            }
        }

        /// <summary>
        /// LayOut 탭  UserControl 숨김 및 초기화
        /// </summary>
        private void HideAndClearAllRack()
        {
            for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout1[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout1[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout2[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout2[row][col].Clear();
                }
            }
        }

        /// <summary>
        /// LayOut 탭  전체 UserControl 체크해제
        /// </summary>
        private void UnCheckedAllRackLayout()
        {
            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout1[rowIndex][colIndex];

                    if (ucRackLayout.IsChecked)
                    {
                        ucRackLayout.IsChecked = false;
                        UnCheckUcRackLayout(ucRackLayout);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout2[rowIndex][colIndex];

                    if (ucRackLayout.IsChecked)
                    {
                        ucRackLayout.IsChecked = false;
                        UnCheckUcRackLayout(ucRackLayout);
                    }
                }
            }
        }
        /// <summary>
        /// LayOut 탭  UserControl 체크해제
        /// </summary>
        private void UnCheckUcRackLayout(UcRackLayout ucRackLayout)
        {
            DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID = '" + ucRackLayout.RackId + "'");
            foreach (DataRow row in selectedRow)
            {
                _dtRackInfo.Rows.Remove(row);
            }
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }
        /// <summary>
        /// LayOut 탭  UserControl 체크
        /// </summary>
        private void CheckUcRackLayout(UcRackLayout rackLayout)
        {

            if (CommonVerify.HasTableRow(_dtRackInfo))
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }

            _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
            _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
            _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;
            _selectedRackIdByRackInfo = string.IsNullOrEmpty(rackLayout.RackId) ? null : rackLayout.RackId;

            int maxSeq = 1;
            DataRow dr = _dtRackInfo.NewRow();
            dr["RACK_ID"] = rackLayout.RackId;
            dr["STATUS"] = rackLayout.RackStateCode;
            dr["PRJT_NAME"] = rackLayout.ProjectName;
            dr["LOTID"] = rackLayout.LotId;
            dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
            dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
            dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
            dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
            dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
            dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
            dr["WIPHOLD"] = rackLayout.WipHold;
            dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
            dr["SKID_GUBUN"] = rackLayout.SkidType;
            dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
            dr["COLOR"] = rackLayout.LegendColor;
            dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
            dr["HOLD_FLAG"] = rackLayout.HoldFlag;
            dr["SEQ"] = maxSeq;
            _dtRackInfo.Rows.Add(dr);

            if (rackLayout.LegendColorType == "4")
            {
                _selectedSkIdIdByRackInfo = null;
            }
            else if (rackLayout.LegendColorType == "5")
            {
                _selectedBobbinIdByRackInfo = null;
            }

            GetLayoutGridColumns(rackLayout.LegendColorType);
            SelectLayoutGrid(rackLayout.LegendColorType);

        }

        #endregion

        #region 동 정보 콤보 박스  : SetAreaCombo()
        /// <summary>
        /// 동 정보 콤보 박스
        /// </summary>
        /// <param name="cbo"></param>
        private void SetAreaCombo(C1ComboBox cbo)
        {
            //2023.06.14 김대현
            //const string bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_CBO";
            string bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_CBO";
            string selectedValue = LoginInfo.CFG_AREA_ID;
            if (GetElecOnlyUseArea() == true)
            {
                bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_ELEC_CBO";
                selectedValue = null;
            }
            //2023.06.14 김대현
            string[] arrColumn = { "LANGID", "AREAID" };

            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, selectedValue);
        }
        #endregion

        #region ELEC_ONLY_USE_AREA 조회
        private bool GetElecOnlyUseArea()
        {
            bool chk = false;
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("CMCODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ELEC_ONLY_USE_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            RQSTDT.Rows.Add(dr);

            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

            if (Result != null && Result.Rows.Count > 0)
            {
                chk = true;
            }

            return chk;
        }
        #endregion

        #region 창고 유형 콤보 박스 : SetStockerTypeCombo()
        /// <summary>
        /// 창고 유형 콤보 박스
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {

            const string bizRuleName = "BR_MHS_SEL_AREA_EQUIPMENT_GROUP";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "CFG_AREA_ID", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), "Y", null, LoginInfo.CFG_AREA_ID, "AREA_EQUIPMENT_GROUP" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region  창고 콤보 박스 : SetStockerCombo()
        /// <summary>
        ///  창고 콤보 박스
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
            string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

            const string bizRuleName = "BR_MHS_SEL_AREA_EQUIPMENT_ELTRTYPE";
            string[] arrColumn = { "LANGID", "AREAID", "CFG_AREA_ID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), LoginInfo.CFG_AREA_ID, stockerType, electrodeType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

        }
        #endregion

        #region  극성 콤보 박스 : SetElectrodeTypeCombo()
        /// <summary>
        /// 극성 콤보 박스
        /// </summary>
        /// <param name="cbo"></param>
        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MHS_SEL_ELTR_TYPE_LIST_PER_EQGR";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.ToString(), cboStockerType.SelectedValue.ToString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region FastTrack 적용 공장 체크 :  ChkFastTrackOWNER()
        /// <summary>
        /// FastTrack 적용 공장 체크
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
        #endregion

        #region HOLD LIST TAB 확인 여부 : ChkHoldTabView()
        /// <summary>
        /// HOLD LIST TAB 확인 여부
        /// </summary>
        private bool ChkHoldTabView()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "AREA_HOLD_TAB_VIEW";
            dr["CBO_CODE"] = cboArea.SelectedValue.ToString();
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

        #region NND STO 그룹별 적재수량설정 TAB 확인 여부 : ChkNNDLoadQtySettingTabView()
        /// <summary>
        /// HOLD LIST TAB 확인 여부
        /// </summary>
        private bool ChkNNDLoadQtySettingTabView()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "AREA_HOLD_TAB_VIEW";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE4"].ToString() == "Y")
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

        #region IQC 컬럼 확인 여부 : ChkIQCView()
        /// <summary>
        /// IQC 컬럼 확인 여부
        /// </summary>
        private bool ChkIQCView()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "AREA_HOLD_TAB_VIEW";
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
        #endregion

        #region 불량태그수 확인여부 :  ChkDefectTag()
        /// <summary>
        /// 불량태그수 확인여부
        /// </summary>
        private bool ChkDefectTag()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "AREA_HOLD_TAB_VIEW";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE3"].ToString() == "Y")
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

        #region 전극 등급 판정 항목 코드 : IsGradeJudgmentDisplay()
        /// <summary>
        /// 전극 등급 판정 항목 코드  동별 공통 코드관리
        /// </summary>
        /// <returns></returns>
        private bool IsGradeJudgmentDisplay()
        {
            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region 타이머 셋팅 : TimerSetting()
        /// <summary>
        /// 타이머 셋팅
        /// </summary>
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();
            }
        }
        #endregion

        #region NND W/O별 현황 탭 사용여부 : IsTabStatusbyWorkorderVisibility()
        /// <summary>
        /// NND W/O별 현황 탭 사용 여부 
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="stockerTypeCode"></param>
        /// <returns></returns>
        private bool IsTabStatusbyWorkorderVisibility(SearchType searchType, string stockerTypeCode)
        {

            if (string.IsNullOrEmpty(stockerTypeCode)) return false;

            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "AREA_EQUIPMENT_GROUP_UI";
                dr["COM_CODE"] = stockerTypeCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (searchType == SearchType.Tab)
                    {
                        if (dtResult.Rows[0]["ATTR1"].GetString() == "Y")
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
                        if (dtResult.Rows[0]["ATTR2"].GetString() == "Y")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
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

        private bool IsProcNGmarkVisibility(string stockerTypeCode)
        {

            if (string.IsNullOrEmpty(stockerTypeCode)) return false;

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
                dr["COM_TYPE_CODE"] = "AREA_EQUIPMENT_GROUP_UI";
                dr["COM_CODE"] = stockerTypeCode;
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["ATTR5"].GetString() == "Y")
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region 프로그래스바 : ShowLoadingIndicator(), HiddenLoadingIndicator()

        /// <summary>
        /// 프로그래스바 보이기
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// 프로그래스바 숨기기
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Do 이벤트
        /// <summary>
        /// DoEvent
        /// </summary>
        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }
        #endregion

        #region Scroll 셋팅 : SetScrollToHorizontalOffset()
        /// <summary>
        ///  Scroll 셋팅
        /// </summary>
        private void SetScrollToHorizontalOffset(ScrollViewer scrollViewer, int colIndex)
        {
            double averageScrollWidth = scrollViewer.ActualWidth / _maxColumnCount;
            scrollViewer.ScrollToHorizontalOffset(Math.Abs(colIndex) * averageScrollWidth);
        }
        #endregion

        #region 컨트롤 초기화 : ClearControl()
        /// <summary>
        /// 컨트롤 초기화
        /// </summary>
        private void ClearControl()
        {
            _selectedEquipmentCode = string.Empty;
            _selectedLotElectrodeTypeCode = string.Empty;
            _selectedStkElectrodeTypeCode = string.Empty;
            _selectedProjectName = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _selectedLotIdByRackInfo = string.Empty;
            _selectedSkIdIdByRackInfo = string.Empty;
            _selectedBobbinIdByRackInfo = string.Empty;
            _selectedRackIdByRackInfo = string.Empty;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;
            _ElectrodeType_Chk = string.Empty;

            _dtWareHouseCapacity?.Clear();

            Util.gridClear(dgCapacitySummary);
            Util.gridClear(dgLamiCapacitySummary);
            Util.gridClear(dgProduct);
            Util.gridClear(dgEmptyCarrier);
            Util.gridClear(dgCarrierList);
            Util.gridClear(dgErrorCarrier);
            Util.gridClear(dgAbNormalCarrier);
            Util.gridClear(dgRackInfo);
            Util.gridClear(dgProHibit);
            InitializeRackUserControl();
        }
        /// <summary>
        #endregion

        #region 적재율 계산  : GetRackRate()
        /// <summary>
        /// 적재율 계산 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private double GetRackRate(double x, double y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x / y * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        #endregion

        #region 스키드 사용 체크 : Chk_Skid_Use()
        /// <summary>
        /// 스키드 사용 체크
        /// </summary>
        private void Chk_Skid_Use()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));
            dt.Columns.Add("ATTRIBUTE1", typeof(string));


            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "MHS_UNCHECK_SKID_BOBBIN_MAPPING";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dr["ATTRIBUTE1"] = "Y";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTE1", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0)
            {
                _Skid_Use_Chk = "Y";
            }
            else
            {
                _Skid_Use_Chk = "N";

            }



        }

        #endregion

        #region NND STO 그룹별 적재수량 - 노칭설비 조회 : SetDataGridEqsgidCombo()
        /// <summary>
        /// 노칭 설비 조회 
        /// </summary>
        /// <param name="dgcol"></param>
        private void SetDataGridEqsgidCombo(C1.WPF.DataGrid.DataGridColumn dgcol)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_AREA_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            //string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE2" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, Process.NOTCHING };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, dgcol, selectedValueText, displayMemberText);
        }

        #endregion

        #region NND STO 그룹별 적재수량 - 사용여부 조회 : SetDataGridUseFlagCombo()
        /// <summary>
        /// 사용여부조회
        /// </summary>
        /// <param name="dgcol"></param>
        private static void SetDataGridUseFlagCombo(C1.WPF.DataGrid.DataGridColumn dgcol)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "CMCDTYPE", "LANGID" };
            string[] arrCondition = { "IUSE", LoginInfo.LANGID };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, dgcol, selectedValueText, displayMemberText);
        }

        #endregion

        #region NND STO 그룹별 적재수량 - 노칭설비정보 리스트에 반영 : SetDataGridComboItem()
        /// <summary>
        /// 리스트에 콤보박스 정보 반영(노칭설비)
        /// </summary>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="dgcol"></param>
        /// <param name="selectedValueText"></param>
        /// <param name="displayMemberText"></param>
        public void SetDataGridComboItem(string bizRuleName, string[] arrColumn, string[] arrCondition, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                        inDataTable.Columns.Add(col, typeof(string));

                    DataRow dr = inDataTable.NewRow();

                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = dtBinding.Copy().AsDataView();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        #endregion

        #region NND STO 그룹별 적재수량 - 그룹별 셋팅된 수량 조회 : SearchLoadQtyList()

        // <summary>
        ///  그룹별 셋팅된 수량조회
        /// </summary>
        private void SearchLoadQtyList()
        {
            Util.gridClear(dgStocGr);
            Util.gridClear(dgLineLoadQty);

            txtStck_Gr.Text = string.Empty;
            txtStck_Gr.Tag = null;
            txtSafeQty.Text = string.Empty;

            const string bizRuleName = "DA_MHS_SEL_MIX_STOCK_LOAD_QTY_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgStocGr, bizResult, null, true);

                    dgStocGr.MergingCells -= dgStocGr_MergingCells;
                    dgStocGr.MergingCells += dgStocGr_MergingCells;

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region NND STO 그룹별 적재수량 - 선택된 그룹에 대한 셋팅된수량 조회 : GetLineLoadQty()
        /// <summary>
        /// 선택된 그룹에 대한 안전재고 수량 조회
        /// </summary>
        /// <param name="sReqID"></param>
        private void GetLineLoadQty(string sReqID)
        {
            try
            {
                Util.gridClear(dgLineLoadQty);


                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("STOC_STCK_GR_ID", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["STOC_STCK_GR_ID"] = sReqID;

                //Indata["REQ_DATE"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_MIX_STOCK_LINE_LOAD_QTY_LIST", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        dtMain.Rows[i]["LOAD_QTY"] = Convert.ToDecimal(dtMain.Rows[i]["LOAD_RATE"].ToString()) * Convert.ToDecimal(txtSafeQty.Text) / 100;

                    }

                }


                Util.GridSetData(dgLineLoadQty, dtMain, FrameOperation, true);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        #endregion

        #region NND STO 그룹별 적재수량 - ROW 추가 및 삭제 : DataGridRowAdd(), DataGridRowRemove()

        /// <summary>
        /// ROW 추가
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {

                if (txtStck_Gr.Text == string.Empty) return;

                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr2 = dt.NewRow();
                dr2["CHK"] = true;
                dr2["USE_FLAG"] = "Y";
                dr2["EQPTID"] = "";
                dr2["LOAD_RATE"] = 0;
                dr2["LOAD_QTY"] = 0;
                dr2["TYPE"] = "N";
                dt.Rows.Add(dr2);
                dt.AcceptChanges();

                dg.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        /// <summary>
        /// ROW 삭제
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                List<DataRow> drInfo = dt.Select("CHK = 1")?.ToList();
                foreach (DataRow dr in drInfo)
                {
                    if (dr["TYPE"].Equals("N"))
                    {
                        dt.Rows.Remove(dr);
                    }
                }
                Util.GridSetData(dg, dt, FrameOperation, true);
                dgLineLoadQty.EndEdit();
                dgLineLoadQty.Refresh();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        #endregion

        #region NND STO 그룹별 적재수량 - 저장 전 Validation : ValidationSave()

        private bool ValidationSave()
        {
            if (!CommonVerify.HasDataGridRow(dgLineLoadQty))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            DataRow[] drChk = Util.gridGetChecked(ref dgLineLoadQty, "CHK");

            if (drChk.Length <= 0)
            {
                Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                return false;
            }


            foreach (C1.WPF.DataGrid.DataGridRow row in dgLineLoadQty.Rows)
            {
                if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1"))
                {
                    //라인 체크
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString()))
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("설비"));
                        return false;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "LOAD_RATE").GetString()) || Convert.ToDecimal(DataTableConverter.GetValue(row.DataItem, "LOAD_RATE").GetString()) == 0)
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("적재설정비율(%)"));
                        return false;
                    }

                }
            }
            DataTable dt = new DataTable();
            dt = DataTableConverter.Convert(dgLineLoadQty.ItemsSource);
            decimal _totloadqty = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                _totloadqty = Convert.ToDecimal(dt.Rows[i]["LOAD_RATE"].ToString()) + _totloadqty;
            }

            if (_totloadqty > 100)
            {
                //적재설정비율이 100%를 넘었습니다.
                Util.MessageValidation("SUF4968");
                return false;
            }


            return true;
        }

        #endregion

        #region NND STO 그룹별 적재수량 - 신규 셋팅된 안전수량 저장 : SaveData()
        private void SaveData()
        {
            try
            {
                const string bizRuleName = "DA_MHS_SEL_MIX_STOCK_LOAD_QTY_MNGT";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("STOC_STCK_GR_ID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("LOAD_QTY", typeof(decimal));
                inDataTable.Columns.Add("LOAD_RATE", typeof(decimal));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SYSDATE", typeof(DateTime));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgLineLoadQty.Rows)
                {
                    if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1"))
                    {
                        // 신규ROW에서 사용여부 N인것은 제외
                        if (!(Util.NVC(DataTableConverter.GetValue(row.DataItem, "TYPE")).GetString() == "N" && Util.NVC(DataTableConverter.GetValue(row.DataItem, "USE_FLAG")).GetString() == "N"))
                        {

                            DataRow dr = inDataTable.NewRow();
                            dr["STOC_STCK_GR_ID"] = txtStck_Gr.Tag.ToString();
                            dr["EQSGID"] = "-";
                            dr["EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                            dr["PRODID"] = "-";
                            dr["LOAD_QTY"] = 0;
                            dr["LOAD_RATE"] = DataTableConverter.GetValue(row.DataItem, "LOAD_RATE").GetString();
                            dr["USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "USE_FLAG").GetString();
                            dr["USERID"] = LoginInfo.USERID;
                            dr["SYSDATE"] = System.DateTime.Now;
                            inDataTable.Rows.Add(dr);
                        }
                    }
                }

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                        SearchLoadQtyList();
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
        private void DisplayInit()
        {
            //  dgProduct.Columns["SKID_ID"].Visibility = Visibility.Visible;
            //  dgCapacitySummary.Columns["RACK_QTY"].Visibility = Visibility.Visible;
            //  dgCapacitySummary.Columns["BBN_UM_QTY"].Visibility = Visibility.Collapsed;


            // dgProduct.Columns["SKID_ID"].Header = ObjectDic.Instance.GetObjectName("REP_CSTID");
            //  dgProduct.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("CSTID");
            //dgCarrierList.Columns["BOBBIN_ID"].Header = ObjectDic.Instance.GetObjectName("CSTID");
            dgCapacitySummary.Columns["RACK_QTY"].Visibility = Visibility.Collapsed;
            dgCapacitySummary.Columns["RACK_MAX"].Header = ObjectDic.Instance.GetObjectName("용량");
            dgCapacitySummary.Columns["ERROR_QTY"].Header = ObjectDic.Instance.GetObjectName("비정상 Rack");
            dgCapacitySummary.Columns["ABNORM_QTY"].Header = ObjectDic.Instance.GetObjectName("정보불일치수");
            dgCapacitySummary.Columns["PROHIBIT_QTY"].Header = ObjectDic.Instance.GetObjectName("금지단 수");
            dgCapacitySummary.Columns["RACK_QTY"].Header = ObjectDic.Instance.GetObjectName("총Carrier수(실+공)");
            dgCapacitySummary.Columns["RACK_RATE"].Header =  ObjectDic.Instance.GetObjectName("적재율(%)") ;

            dgCapacitySummary.Columns["LOT_QTY"].Header = ObjectDic.Instance.GetObjectName("양품");
            dgCapacitySummary.Columns["LOT_HOLD_QTY"].Header = ObjectDic.Instance.GetObjectName("MES HOLD");
            dgCapacitySummary.Columns["LOT_HOLD_QMS_QTY"].Header = ObjectDic.Instance.GetObjectName("QMS HOLD");
            dgCapacitySummary.Columns["BBN_E_QTY"].Header = ObjectDic.Instance.GetObjectName("공Carrier수");

            dgCapacitySummary.Columns["ELTR_TYPE_CODE"].DisplayIndex = 0;
            dgCapacitySummary.Columns["ELTR_TYPE_NAME"].DisplayIndex = 1;
            dgCapacitySummary.Columns["EQPTNAME"].DisplayIndex = 2;
            dgCapacitySummary.Columns["RACK_MAX"].DisplayIndex = 3;
            dgCapacitySummary.Columns["PRJT_NAME"].DisplayIndex = 4;
            dgCapacitySummary.Columns["LOT_QTY"].DisplayIndex = 5;
            dgCapacitySummary.Columns["LOT_HOLD_QTY"].DisplayIndex = 6;
            dgCapacitySummary.Columns["LOT_HOLD_QMS_QTY"].DisplayIndex = 7;
            dgCapacitySummary.Columns["BBN_U_QTY"].DisplayIndex = 8;
            dgCapacitySummary.Columns["BBN_E_QTY"].DisplayIndex = 9;
            dgCapacitySummary.Columns["BBN_UM_QTY"].DisplayIndex = 10;
            dgCapacitySummary.Columns["ERROR_QTY"].DisplayIndex = 11;
            dgCapacitySummary.Columns["ABNORM_QTY"].DisplayIndex = 12;
            dgCapacitySummary.Columns["PROHIBIT_QTY"].DisplayIndex = 13;
            dgCapacitySummary.Columns["RACK_RATE"].DisplayIndex = 14;
            dgCapacitySummary.Columns["RACK_QTY"].DisplayIndex = 15;

            dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_C"].Visibility = Visibility.Visible;
            dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_A"].Visibility = Visibility.Visible;
            dgLamiCapacitySummary.Columns["RACK_QTY"].Visibility = Visibility.Visible;
            dgLamiCapacitySummary.Columns["EMPTY_QTY"].Visibility = Visibility.Collapsed;
            dgLamiCapacitySummary.Columns["RACK_MAX"].Header = ObjectDic.Instance.GetObjectName("용량");
            dgLamiCapacitySummary.Columns["ERROR_QTY"].Header = ObjectDic.Instance.GetObjectName("비정상 Rack");
            dgLamiCapacitySummary.Columns["ABNORM_QTY"].Header = ObjectDic.Instance.GetObjectName("정보불일치수") ;
            dgLamiCapacitySummary.Columns["PROHIBIT_QTY"].Header = ObjectDic.Instance.GetObjectName("금지단 수") ;
            dgLamiCapacitySummary.Columns["RACK_QTY"].Header = ObjectDic.Instance.GetObjectName("사용중 Rack") ;
            dgLamiCapacitySummary.Columns["RACK_RATE"].Header = ObjectDic.Instance.GetObjectName("적재율(%)") ;

            dgLamiCapacitySummary.Columns["LOT_QTY_C"].Header =  ObjectDic.Instance.GetObjectName("양극양품_") ;
            dgLamiCapacitySummary.Columns["HOLD_QTY_C"].Header =  ObjectDic.Instance.GetObjectName("양극HOLD_") ;
            dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_C"].Header = ObjectDic.Instance.GetObjectName("QMS HOLD") ;
            dgLamiCapacitySummary.Columns["LOT_QTY_A"].Header = ObjectDic.Instance.GetObjectName("음극양품_") ;
            dgLamiCapacitySummary.Columns["HOLD_QTY_A"].Header = ObjectDic.Instance.GetObjectName("음극HOLD_") ;
            dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_A"].Header = ObjectDic.Instance.GetObjectName("QMS HOLD") ;


            dgLamiCapacitySummary.Columns["EQPTNAME"].DisplayIndex = 0;
            dgLamiCapacitySummary.Columns["RACK_MAX"].DisplayIndex = 1;
            dgLamiCapacitySummary.Columns["PRJT_NAME"].DisplayIndex = 2;
            dgLamiCapacitySummary.Columns["LOT_QTY_C"].DisplayIndex = 3;
            dgLamiCapacitySummary.Columns["HOLD_QTY_C"].DisplayIndex = 4;
            dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_C"].DisplayIndex = 5;
            dgLamiCapacitySummary.Columns["LOT_QTY_A"].DisplayIndex = 6;
            dgLamiCapacitySummary.Columns["HOLD_QTY_A"].DisplayIndex = 7;
            dgLamiCapacitySummary.Columns["HOLD_QMS_QTY_A"].DisplayIndex = 8;
            dgLamiCapacitySummary.Columns["EMPTY_QTY"].DisplayIndex = 9;
            dgLamiCapacitySummary.Columns["ERROR_QTY"].DisplayIndex = 10;
            dgLamiCapacitySummary.Columns["ABNORM_QTY"].DisplayIndex = 11;
            dgLamiCapacitySummary.Columns["PROHIBIT_QTY"].DisplayIndex = 12;
            dgLamiCapacitySummary.Columns["RACK_QTY"].DisplayIndex = 13;
            dgLamiCapacitySummary.Columns["RACK_RATE"].DisplayIndex = 14;
            dgLamiCapacitySummary.Columns["EQPTID"].DisplayIndex = 15;

        }

        #endregion


        #endregion
    }
}
