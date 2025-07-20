/*************************************************************************************
 Created Date : 2027.07.30
      Creator : 조성근
   Decription : 롤프레스 롤맵 수정(실적조회) 팝업
--------------------------------------------------------------------------------------
 [Change History]
   2024.09.24  김지호 : [E20240905-001389] TAG_SECTION 툴팁내 불량 명칭 및 총 거리값 표시되도록 수정  
   2024.10.16  김지호 : [E20240830-001396] 특이작업, 시스템관리 Modify RollMap Reason Code 미맵핑 시 Validation 기능 추가
   2024.10.11  조성근 : [E20241011-001158] 길이초과, 길이부족 제거
   2024.11.01  조성근 : [E20241101-001339] TAG_DEFECT_MES 추가, TEST CUT 추가
   2024.11.21  조성근 : [E20250107-000829] 조회용으로 팝업시에 TEST CUT 버튼 숨기기, 그리드하단에 불량합계 추가, TagSectionSingle Tooltip에 불량명추가 
   2024.12.17  이민영 : [E20241126-000434] 코터 웹게이지 MIN/MAX/대표값 추가로 설정된 설비별 툴팁 표시되도록 수정
   2025.01.02  조성근 : [E20250107-000829] [롤맵] 롤프레스 실적 자동화 관련 개선 건
   2025.03.23  조성근   [E20250324-000244] 롤프레스 롤맵 수불 개선건
   2025.04.01  조성근   [E20250401-000324] 롤프레스 롤맵 수불 개선건
   2025.04.04  조성근 : [E20250325-000705] 롤프레스 롤맵 아웃피드 스크랩 추가
   2025.05.09  이창희 : [E20250509-001114] [롤맵] 전극공정진척 - Modifiy RollMap Tab 구성 변경 건.
   2025.05.27  조성근   [E20241216-000153] 롤맵 슬리터_롤맵 불량, Loss 자동 입력 개선
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System.Data;
using System;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using C1.WPF;
using System.Windows.Media;
using C1.WPF.C1Chart;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using Action = System.Action;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Windows.Input;
using System.Diagnostics;
using System.Text;
using System.Windows.Interop;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// 롤프레스 롤맵 팝업
    /// </summary>
    public partial class CMM_RM_RP_RESULT : C1Window, IWorkArea
    {
        #region Variables

        #region Struct (WinParams)
        public struct WinParams
        {
            public string processCode;
            public string equipmentSegmentCode;
            public string equipmentCode;
            public string lotId;
            public string wipSeq;
            public string laneQty;
            public string equipmentName;
            public string version;
        }
        #endregion

        public WinParams wParams;
        public bool IsUpdated { get; set; }
        private readonly Util _util = new Util();
        private DataTable _dtOutsideEtcClctType;
        private DataTable _dtOutsideClctType;
        private DataTable _dtEtcLossClctType;
        private DataTable _dtOutsideComCode;
        private DataTable _dtEtcLossComCode;
        private DataTable _dtOutsideEqptMeasrPstnID;
        private DataTable _dtEtcLossEqptMeasrPstnID;
        private DataTable _dtLevelComCode;
        private DataTable _dtLevelSection;
        private DataSet _dsLevelReasonCode;
        private string _strOutsideEtcEqptPstnID;
        private string _strOutsideEqptPstnID;
        private string _strEtcLossEqptPstnID;
        private DataTable _dtReason;
        private bool _isShowBtnDeft = false;	//2023-10-05 롤맵 불량 비교버튼
        private DataRow _drADJInfo_NonArea; //nathan 2024.01.17 롤맵용 테스트 컷 적용

        private bool _isMinMaxEqpt = false;  //2024-12-17 웹게이지 MIN/MAX 툴팁 표시여부(설비기준)

        private bool _isSelectedTagSection;
        private double _selectedStartSection = 0;
        private double _selectedEndSection = 0;
        private double _selectedchartPosition = 0;
        private string _callClass = "";
        private C1Chart chartInput = null;
        private string _EnableChangeFPIQty = "Y";
        private double _defaltFPIQty = 0;
        private bool _bUseFPI = true;

        private string strReadOnlyColor = "#FFF3F0F0";
        private string _strSectionDefect = ObjectDic.Instance.GetObjectName("Section Defect");
        private string _strScrapSection = ObjectDic.Instance.GetObjectName("SCRAP_SECTION");

        #region Enum (CoordinateType / ChartConfigurationType/ChartDisplayType)
        /// <summary>
        /// 보정이전(절대좌표) 보정이후(상태좌표)
        /// </summary>
        private enum CoordinateType
        {
            /// <summary>
            /// 상태좌표(보정이후)
            /// </summary>
            RelativeCoordinates,
            /// <summary>
            /// 절대좌표(보정이전)
            /// </summary>
            Absolutecoordinates
        }
        /// <summary>
        /// 투입LOT, 완성LOT 구분
        /// </summary>
        private enum ChartConfigurationType
        {
            Input,
            Output
        }
        /// <summary>
        /// 측정데이터에서 차트에 표시할 설비측정위치
        /// </summary>
        private enum ChartDisplayType
        {
            MarkLabel,
            TagSectionStart,
            TagSectionEnd,
            Mark,
            //TagVisionTop,
            //TagVisionBack,
            Material,
            Sample,
            TagSpot,
            //SurfaceTop,
            //SurfaceBack,
            //OverLayVision,
            RewinderScrap
        }

        #endregion

        private CoordinateType coordinateType;

        /// <summary>
        /// ROLLMAP_COLORMAP_SPEC(로딩량/두께 등 스펙 컬러)
        /// </summary>
        private DataTable dtLineLegend;
        /// <summary>
        /// ROLLMAP_LANE_SYMBOL_COLOR(레인 심볼 컬러)
        /// </summary>
        private DataTable dtLaneLegend;

        private DataTable dtLane;
        private DataTable dtDefectOutput;
        private DataTable dtDefectInput;
        private DataTable dtBaseDefectOutput;
        private DataTable dtBaseDefectInput;
        private DataTable dtOutGauge;
        private DataTable dt2DBarcodeInfo;

        /// <summary>
        /// 롤맵 수불여부
        /// </summary>
        private bool isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        /// <summary>
        /// 롤맵 LOT 여부
        /// </summary>
        private bool isRollMapLot;
        /// <summary>
        /// INPUT LOT 기준 길이
        /// </summary>
        private double xInputMaxLength;
        /// <summary>
        /// OUTPUT LOT 기준 길이
        /// </summary>
        private double xOutputMaxLength;

        private bool _isSearchMode; // Search Mode

        /// <summary>
        /// 수동으로 등록된 Scrap 
        /// </summary>
        AlarmZone dsscrapSection = new AlarmZone();
        #endregion

        #region Properties

        #region IWorkArea
        public IFrameOperation FrameOperation { get; set; }
        #endregion

        #endregion

        #region Constructor
        public CMM_RM_RP_RESULT()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods

        #region InitSetting
        /// <summary>
        /// 로드시 최초 설정
        /// </summary>
        private void InitSetting()
        {
            #region 1.Parameters
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)//11개
            {
                wParams = new WinParams();
                wParams.processCode = Util.NVC(parameters[0]);
                wParams.equipmentSegmentCode = Util.NVC(parameters[1]);
                wParams.equipmentCode = Util.NVC(parameters[2]);
                wParams.lotId = Util.NVC(parameters[3]);
                wParams.wipSeq = Util.NVC(parameters[4]);
                wParams.laneQty = Util.NVC(parameters[5]);
                wParams.equipmentName = Util.NVC(parameters[6]);
                wParams.version = Util.NVC(parameters[7]);
                //_isTestCutVisible = (Util.NVC(parameters[8])).Equals("Y") ? true : false;
                _isSearchMode = (Util.NVC(parameters[9])).Equals("Y") ? true : false;
                _callClass = Util.NVC(parameters[10]);
            }

            if (_isSearchMode) //수정불가능모드
            {
                btnDelete.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;

                btnDeleteSingle.Visibility = Visibility.Collapsed;
                btnSaveSingle.Visibility = Visibility.Collapsed;
                btnAddSingle.Visibility = Visibility.Collapsed;

                btnDelOutsideScrap.Visibility = Visibility.Collapsed;
                btnSaveOutsideScrap.Visibility = Visibility.Collapsed;
                btnAddOutsideScrap.Visibility = Visibility.Collapsed;

                btnDelScrapSection.Visibility = Visibility.Collapsed;
                btnSaveScrapSection.Visibility = Visibility.Collapsed;
                btnAddScrapSection.Visibility = Visibility.Collapsed;

                foreach (var column in dgOutsideScrap.Columns)
                {
                    column.IsReadOnly = true;
                }

                foreach (var column in dgTagDefectMes.Columns)
                {
                    column.IsReadOnly = true;
                }

                foreach (var column in dgTagSectionSingle.Columns)
                {
                    column.IsReadOnly = true;
                }

                foreach (var column in dgLotInfo.Columns)
                {
                    column.IsReadOnly = true;
                }

                foreach (var column in dgScrapSection.Columns)
                {
                    column.IsReadOnly = true;
                }                

                cbTestCutLoss.IsEnabled = false;
                txtGoodQty.IsEnabled = false;
                rdoTCApplyY_FPI.IsEnabled = false;
                rdoTCApplyN_FPI.IsEnabled = false;
                rdoTCApplyY_TESTCUT.IsEnabled = false;
                rdoTCApplyN_TESTCUT.IsEnabled = false;

            }
            else
            {
                btnDelete.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                btnRollMapPositionEdit.Visibility = Visibility.Visible;

                btnDeleteSingle.Visibility = Visibility.Visible;
                btnSaveSingle.Visibility = Visibility.Visible;
                btnAddSingle.Visibility = Visibility.Visible;

                btnDelOutsideScrap.Visibility = Visibility.Visible;
                btnSaveOutsideScrap.Visibility = Visibility.Visible;
                btnAddOutsideScrap.Visibility = Visibility.Visible;

                btnDelScrapSection.Visibility = Visibility.Visible;
                btnSaveScrapSection.Visibility = Visibility.Visible;
                btnAddScrapSection.Visibility = Visibility.Visible;
            }

            btnDefect.Visibility = Visibility.Collapsed;    //2023-10-05 롤맵 불량 비교버튼
            bShowDeftButton(); //2023-10-05 롤맵 불량 비교버튼
            #endregion

            #region 2.Windows Size
            var interopHelper = new WindowInteropHelper(System.Windows.Application.Current.MainWindow);
            var activeScreen = System.Windows.Forms.Screen.FromHandle(interopHelper.Handle);

            ROLLMAP.Width = activeScreen.WorkingArea.Width - 100;
            ROLLMAP.Height = activeScreen.WorkingArea.Height - 100;
            //ROLLMAP.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;

            #endregion

            #region 3.WinParams(LOT,설비)
            txtLot.Text = wParams.lotId;
            txtEquipmentName.Text = wParams.equipmentName;
            #endregion

            #region 4.좌표계(절대/상대)
            if (rdoRelativeCoordinates.IsChecked == true)
            {
                coordinateType = CoordinateType.RelativeCoordinates;
            }
            else
            {
                coordinateType = CoordinateType.Absolutecoordinates;
            }
            #endregion

            #region 5.IN/OUT Chart X ScrollBar
            chartOutput.View.AxisX.ScrollBar = new AxisScrollBar();
            #endregion

            #region 6.GetCommonCode(ROLLMAP_COLORMAP_SPEC) : 로딩량/두께 등 스펙 컬러
            dtLineLegend = new DataTable();
            dtLineLegend.Columns.Add("NO", typeof(int));
            dtLineLegend.Columns.Add("COLORMAP", typeof(string));
            dtLineLegend.Columns.Add("CMCODE", typeof(string));
            dtLineLegend.Columns.Add("VALUE", typeof(string));
            dtLineLegend.Columns.Add("GROUP", typeof(string));
            dtLineLegend.Columns.Add("SHAPE", typeof(string));

            // 로딩량 평균
            DataTable dtColorMapSpec = GetCommonCode("ROLLMAP_COLORMAP_SPEC");

            if (CommonVerify.HasTableRow(dtColorMapSpec))
            {
                string[] exceptionCode = new string[] { "OK", "NG" };

                dtColorMapSpec = (from t in dtColorMapSpec.AsEnumerable()
                                  where !exceptionCode.Contains(t.Field<string>("CMCODE"))
                                  orderby t.GetValue("CMCDSEQ").GetInt() ascending
                                  select t).CopyToDataTable();

                foreach (DataRow row in dtColorMapSpec.Rows)
                {
                    dtLineLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCODE"].GetString(), row["CMCDNAME"].GetString(), "LOAD", "RECT");
                }
            }
            #endregion

            #region 7.GetCommonCode(ROLLMAP_LANE_SYMBOL_COLOR)
            // Lane 범례 색상 
            dtLaneLegend = new DataTable();
            dtLaneLegend.Columns.Add("LANE_NO", typeof(string));
            dtLaneLegend.Columns.Add("COLORMAP", typeof(string));

            DataTable dtLaneSymbolColor = GetCommonCode("ROLLMAP_LANE_SYMBOL_COLOR");

            if (CommonVerify.HasTableRow(dtLaneSymbolColor))
            {
                foreach (DataRow row in dtLaneSymbolColor.Rows)
                {
                    dtLaneLegend.Rows.Add(row["CMCODE"].GetString(), row["ATTRIBUTE1"].GetString());
                }
            }
            #endregion

            #region 8.Lane 정보, IN/OUT 불량정보
            dtLane = new DataTable();
            dtLane.Columns.Add("LANE_NO", typeof(string));

            dtDefectOutput = new DataTable();
            dtDefectOutput.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            dtDefectOutput.Columns.Add("ABBR_NAME", typeof(string));
            dtDefectOutput.Columns.Add("COLORMAP", typeof(string));

            dtDefectInput = dtDefectOutput.Copy();
            #endregion

            #region 9.GetAreaCommonCode(ROLLMAP_HEADER_COND_VISIBILITY) : Header 조건 Visibility 설정
            DataTable dtV = GetAreaCommonCode("ROLLMAP_HEADER_COND_VISIBILITY", wParams.processCode);

            if (CommonVerify.HasTableRow(dtV))
            {
                bool bMeasure = string.Equals(dtV.Rows[0]["ATTR1"].GetString(), "Y");//계측기(로딩량/두께)
                bool bMapExpress = string.Equals(dtV.Rows[0]["ATTR2"].GetString(), "Y");//맵표현 설정(요약/LANE 선택)
                bool bColorMap = string.Equals(dtV.Rows[0]["ATTR3"].GetString(), "Y");//색지도 표현(Grade 블록/평균값 상세)

                gdMeasurementradio.Visibility = bMeasure ? Visibility.Visible : Visibility.Collapsed;
                gdMapExpress.Visibility = bMapExpress ? Visibility.Visible : Visibility.Collapsed;
                gdColorMap.Visibility = bColorMap ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                gdMeasurementradio.Visibility = Visibility.Collapsed;
                gdMapExpress.Visibility = Visibility.Collapsed;
                gdColorMap.Visibility = Visibility.Collapsed;
            }

            #endregion
        }

        private void bShowDeftButton() //2023-10-05 롤맵 불량 비교버튼
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_DEF_COMP_RP";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                _isShowBtnDeft = true;
            }
            else
            {
                _isShowBtnDeft = false;
            }
        }

        private void SetEqptDetailInfoByCommoncode(string eqptid)
        {
            //공통코드 ROLLMAP_EQPT_DETAIL_INFO
            // - 속성1 : Y일 경우 코터 차트의 1Lane을 위쪽으로 표시
            // - 속성2 : Y일 경우 MIN/MAX/REP 값 툴팁 표시 사용설비
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

            try
            {
                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("CMCODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ROLLMAP_EQPT_DETAIL_INFO";
                dr["CMCODE"] = eqptid;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (string.Equals("Y", dtResult.Rows[0]["ATTRIBUTE2"].GetString()))
                        _isMinMaxEqpt = true;
                }
            }
            catch (Exception)
            {
                ;
            }
        }
        #endregion

        #region SetMsbRollMapLaneCheck
        /// <summary>
        /// 맵표현설정(요약) 선택시, 하단 Lane 선택 정보 체크변경
        /// </summary>
        private void SetMsbRollMapLaneCheck()
        {
            //C : 양극
            //A : 음극
            //if (string.IsNullOrEmpty(cboRollMapExpressSummary.SelectedValue.GetString()) || string.IsNullOrEmpty(msbRollMapLane.SelectedItems.GetString()))
            if (string.IsNullOrEmpty(cboRollMapExpressSummary.SelectedValue.GetString()))
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(cboRollMapExpressSummary.ItemsSource);
            DataRow dr = dt.Rows[cboRollMapExpressSummary.SelectedIndex];

            DataTable dtRollMapLane = DataTableConverter.Convert(msbRollMapLane.ItemsSource);

            string[] laneArray = null;

            if (CommonVerify.HasTableRow(dtRollMapLane))
            {
                laneArray = dr["ATTRIBUTE1"].GetString().Split(',');
                //laneArray = dr["LaneDescription"].GetString().Split(',');
            }

            if (laneArray != null)
            {
                msbRollMapLane.UncheckAll();

                for (int i = 0; i < laneArray.Length; i++)
                {
                    foreach (MultiSelectionBoxItem msbitem in msbRollMapLane.MultiSelectionBoxSource)
                    {
                        if (DataTableConverter.GetValue(msbitem.Item, "CMCODE").GetString().Equals(laneArray[i]))
                        {
                            msbitem.IsChecked = true;
                        }
                    }
                }
            }
        }
        #endregion

        #region SetRollMapExpressionAndLines
        /// <summary>
        /// 상단 조회조건(맵표현설정:요약/Lane선택) 설정
        /// </summary>
        private void SetRollMapExpressionAndLines()
        {
            try
            {
                DataTable dtExp = GetAreaCommonCode("ROLLMAP_EXPRESS_SUMMARY");
                DataTable dtLane = GetAreaCommonCode("ROLLMAP_LANE");

                if (CommonVerify.HasTableRow(dtExp) && CommonVerify.HasTableRow(dtLane))
                {
                    DataTable dtMulti = new DataTable();
                    dtMulti.Columns.Add("CMCDNAME", typeof(string));
                    dtMulti.Columns.Add("CMCODE", typeof(string));
                    dtMulti.Columns.Add("ATTRIBUTE1", typeof(string));

                    #region 1.맵표현설정(레인선택)
                    foreach (DataRow drLane in dtLane.Rows)
                    {
                        DataRow drNew = dtMulti.NewRow();
                        drNew["CMCODE"] = drLane["COM_CODE"];
                        drNew["CMCDNAME"] = drLane["COM_CODE_NAME"];
                        dtMulti.Rows.Add(drNew);
                    }
                    msbRollMapLane.ItemsSource = DataTableConverter.Convert(dtMulti);
                    #endregion

                    #region 2.맵표현설정(요약)
                    DataTable dtCbo = dtMulti.Clone();

                    foreach (DataRow drExp in dtExp.Rows)
                    {
                        DataRow drNew = dtCbo.NewRow();
                        drNew["CMCODE"] = drExp["COM_CODE"];
                        drNew["CMCDNAME"] = drExp["COM_CODE_NAME"];
                        drNew["ATTRIBUTE1"] = drExp["ATTR1"];
                        dtCbo.Rows.Add(drNew);
                    }

                    cboRollMapExpressSummary.ItemsSource = DataTableConverter.Convert(dtCbo);
                    #endregion

                    cboRollMapExpressSummary.SelectedIndex = cboRollMapExpressSummary.Items.Count - 1;//전체 Lane 
                    //SetMsbRollMapLaneCheck();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region SetRollMapLaneRange
        /// <summary>
        /// 상단 조회조건(맵표현설정:요약/Lane선택) 설정
        /// </summary>
        /// <returns></returns>
        private DataTable SetRollMapLaneRange()
        {
            /**************************************************************************
             * 1.소형(2170) 롤맵화면에서는 함수를 이용해서, 맵표현설정 정보 가져오는 방법과
             *                             COMMONCODE 이용해서 정보 가져오는 방법 2개 사용.
             * 2.NT 소형 파우치에서는 아직 요약정보 결정도 안되었고,
             *   가져오는 방법도 미정이라서, 테스트시는 LANE_LIST 파라미터 수동으로 입력해서 진행할 것
             **************************************************************************/

            MultiSelectionBox msb = msbRollMapLane;

            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_LANE_CY";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WIPSEQ", typeof(decimal));
            dt.Columns.Add("IN_OUT", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = wParams.equipmentCode;
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = wParams.wipSeq;
            dr["IN_OUT"] = "2";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                #region No.1 Lane 별 맵 선택 MultiSelectionBox 바인딩(하단)
                var query = (from t in dtResult.AsEnumerable()
                             where t.Field<string>("LANE").Equals("TOT")
                             select new { LaneDescription = t.Field<string>("LANE_DESC") }).FirstOrDefault();

                if (query != null)
                {
                    DataTable dtRollMapLane = new DataTable();
                    dtRollMapLane.Columns.Add("CMCDNAME", typeof(string));
                    dtRollMapLane.Columns.Add("CMCODE", typeof(string));

                    string[] laneList = query.LaneDescription.Split(',');

                    for (int j = 0; j < laneList.Length; j++)
                    {
                        DataRow newRow = dtRollMapLane.NewRow();
                        newRow["CMCODE"] = laneList[j];
                        newRow["CMCDNAME"] = @"L" + laneList[j].GetString();
                        dtRollMapLane.Rows.Add(newRow);
                    }

                    msbRollMapLane.ItemsSource = DataTableConverter.Convert(dtRollMapLane);
                }
                #endregion

                #region No.2 맵 표현 요약 ComboBox 바인딩(상단) - 이벤트 처리 위해 나중에 바인딩
                var queryExpressSummary = (from t in dtResult.AsEnumerable()
                                           where !t.Field<string>("LANE").Equals("TOT") //TOT 제외
                                           select new
                                           {
                                               CMCODE = t.Field<string>("LANE")
                                             ,
                                               CMCDNAME = t.Field<string>("LANE")
                                             ,
                                               LaneDescription = t.Field<string>("LANE_DESC")
                                           }).ToList();

                if (queryExpressSummary.Any())
                {
                    DataTable dtExpressSummary = new DataTable();
                    dtExpressSummary.Columns.Add("CMCDNAME", typeof(string));
                    dtExpressSummary.Columns.Add("CMCODE", typeof(string));
                    dtExpressSummary.Columns.Add("LaneDescription", typeof(string));

                    foreach (var item in queryExpressSummary)
                    {
                        DataRow newRow = dtExpressSummary.NewRow();
                        newRow["CMCODE"] = item.CMCODE;
                        newRow["CMCDNAME"] = item.CMCODE;
                        newRow["LaneDescription"] = item.LaneDescription;
                        dtExpressSummary.Rows.Add(newRow);
                    }

                    cboRollMapExpressSummary.ItemsSource = DataTableConverter.Convert(dtExpressSummary);

                    if (cboRollMapExpressSummary.SelectedIndex < 0)
                    {
                        cboRollMapExpressSummary.SelectedIndex = 0;
                    }

                }
                #endregion

                dtResult = dtResult.AsEnumerable()
                  .Where(row => row.Field<String>("LANE") != "TOT")
                  .CopyToDataTable();

            }

            return dtResult;

        }
        #endregion

        #region InitializeCombo
        /// <summary>
        /// 로드시 상단조건 Combo 일괄설정 (1.맵표현설정:요약/Lane선택) 
        /// </summary>
        private void InitializeCombo()
        {
            //SetRollMapLaneRange();

            if (gdMapExpress.Visibility == Visibility.Visible) //맵표현설정 사용시에만 레인정보 가져온다.
            {
                SetRollMapExpressionAndLines();
            }

            DataTable dtResult = GetAreaCommonCode("ROLLMAP_SBL_APPLY_DATE", wParams.equipmentCode);

            SetShowHideTagSectionTab();

            if (tiTagSection.Visibility == Visibility.Collapsed)
            {
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;
            }

            InitializeDataGridCombo();

            if (_isShowBtnDeft == true) btnDefect.Visibility = Visibility.Visible;
            else btnDefect.Visibility = Visibility.Collapsed;

            SetUseScrapSection();

            cbTestCutLoss.ItemsSource = StatusAddLocal(_dtReason, CommonCombo.ComboStatus.NONE, "RESNCODE", "RESNNAME").Copy().AsDataView();
            cbTestCutLoss.SelectedIndex = 0;

            //SelectTestCut(dgTestCut);
            btnRollMapTestCut.Visibility = Visibility.Visible;
            tiTestcut_NonArea.Visibility = Visibility.Visible;
            btnSaveTestCut_NonArea.IsEnabled = false;
            if (_isSearchMode) //수정불가능모드
            {
                btnRollMapTestCut.Visibility = Visibility.Collapsed;
                btnSaveTestCut_NonArea.Visibility = Visibility.Collapsed;
            }




            //nathan 2024.01.17 롤맵용 테스트 컷 적용 - end


        }

        private void SetShowHideTagSectionTab()
        {
            bool bHide = false;

            DataTable dtResult = GetAreaCommonCode("ROLLMAP_CONSTANTS", "HIDE_TAG_SECTION_TAB_RP");
            if (CommonVerify.HasTableRow(dtResult) == true)
            {
                string[] ARR_ATTR1 = dtResult.Rows[0]["ATTR1"].ToString().Split(',');
                foreach (string ATTR1 in ARR_ATTR1)
                {
                    if (ATTR1 == _callClass)
                    {
                        bHide = true;
                    }
                }
            }

            DataTable dtSBL = GetAreaCommonCode("ROLLMAP_SBL_APPLY_DATE", wParams.equipmentCode);

            if (bHide == true)
            {
                tiOutsideScrap.Visibility = Visibility.Visible;
                tiTestcut_NonArea.Visibility = Visibility.Visible;
                tiTagDefectMes.Visibility = Visibility.Collapsed;
                tiTagSectionSingle.Visibility = Visibility.Collapsed;
                tiTagSection.Visibility = Visibility.Collapsed;
            }
            else  //특이작업, 실적관리자, 생산실적조회
            {
                tiOutsideScrap.Visibility = Visibility.Visible;
                tiTestcut_NonArea.Visibility = Visibility.Visible;
                if (CommonVerify.HasTableRow(dtSBL))
                {
                    if (string.Equals("Y", dtSBL.Rows[0]["ATTR4"].GetString()))
                    {
                        tiTagDefectMes.Visibility = Visibility.Visible;
                        tiTagSectionSingle.Visibility = Visibility.Collapsed;
                    }
                    else if (dtSBL.Rows[0]["ATTR4"].GetString().Right(1) == "B")
                    {
                        tiTagDefectMes.Visibility = Visibility.Visible;
                        tiTagSectionSingle.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tiTagDefectMes.Visibility = Visibility.Collapsed;
                        tiTagSectionSingle.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    tiTagDefectMes.Visibility = Visibility.Collapsed;
                    tiTagSectionSingle.Visibility = Visibility.Visible;
                }
            }
        }

        private void InitializeDataGridCombo()
        {
            GetReasonCode();

            SetDataGridComboItemLocal(_dtReason, dgOutsideScrap.Columns["RESNCODE"], CommonCombo.ComboStatus.NONE);
            SetDataGridComboItemLocal(_dtReason, dgTagDefectMes.Columns["RESNCODE"], CommonCombo.ComboStatus.NONE);
            SetDataGridComboItemLocal(_dtReason, dgTestCut_NonArea.Columns["ADJ_ATTR2_VALUE"], CommonCombo.ComboStatus.NONE);

            {// 구간불량  LEVEL
                _dtLevelSection = new DataTable("LEVEL");
                _dtLevelSection.Columns.Add("CBO_CODE", typeof(string));
                _dtLevelSection.Columns.Add("CBO_NAME", typeof(string));
                DataRow newRow = _dtLevelSection.NewRow();
                newRow["CBO_CODE"] = _strSectionDefect;
                newRow["CBO_NAME"] = _strSectionDefect;
                _dtLevelSection.Rows.Add(newRow);
                SetDataGridComboItemLocal(_dtLevelSection, dgLotInfo.Columns["LEVEL"], CommonCombo.ComboStatus.NONE);
                SetDataGridComboItemLocal(_dtLevelSection, dgTagSectionSingle.Columns["LEVEL"], CommonCombo.ComboStatus.NONE);
            }


            {// 구간불량 수집유형
                DataTable dtResult = GetEqptClctTypeCombo("TAG_SECTION");
                SetDataGridComboItemLocal(dtResult, dgLotInfo.Columns["ROLLMAP_CLCT_TYPE"], CommonCombo.ComboStatus.NONE);
            }
            {// 구간불량 수집항목 TAG_SECTION
                DataTable dtResult = GetEqptMeasrPstnCombo("TAG_SECTION");
                SetDataGridComboItemLocal(dtResult, dgLotInfo.Columns["EQPT_MEASR_PSTN_ID"], CommonCombo.ComboStatus.NONE);
            }
            {// 구간불량 활동유형
                DataTable dtResult = GetEqptDefectCode("TAG_SECTION", null);
                List<string> strList = new List<string>();
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string strData = dtResult.Rows[i]["RESNCODE"].ToString();
                    strList.Insert(i, strData);
                }
                var query = _dtReason.AsEnumerable().Where(o => strList.Contains(o.Field<string>("RESNCODE"))).ToList();
                DataTable dtResn = query.Any() ? query.CopyToDataTable() : _dtReason.Clone();
                SetDataGridComboItemLocal(dtResn, dgLotInfo.Columns["RESNCODE"], CommonCombo.ComboStatus.NONE);
            }
            {// 싱글 수집유형
                DataTable dtResult = GetEqptClctTypeCombo("TAG_SECTION_SINGLE");
                SetDataGridComboItemLocal(dtResult, dgTagSectionSingle.Columns["ROLLMAP_CLCT_TYPE"], CommonCombo.ComboStatus.NONE);
            }
            {// 싱글 수집항목 TAG_SECTION_SINGLE
                DataTable dtResult = GetEqptMeasrPstnCombo("TAG_SECTION_SINGLE");
                SetDataGridComboItemLocal(dtResult, dgTagSectionSingle.Columns["EQPT_MEASR_PSTN_ID"], CommonCombo.ComboStatus.NONE);
            }
            {// 싱글 활동유형
                DataTable dtResult = GetEqptDefectCode("TAG_SECTION_SINGLE", null);
                List<string> strList = new List<string>();
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string strData = dtResult.Rows[i]["RESNCODE"].ToString();
                    strList.Insert(i, strData);
                }
                var query = _dtReason.AsEnumerable().Where(o => strList.Contains(o.Field<string>("RESNCODE"))).ToList();

                DataTable dtResn = query.Any() ? query.CopyToDataTable() : _dtReason.Clone();
                SetDataGridComboItemLocal(dtResn, dgTagSectionSingle.Columns["RESNCODE"], CommonCombo.ComboStatus.NONE);
            }
            {// ScrapSection  LEVEL
                _dtLevelSection = new DataTable("LEVEL");
                _dtLevelSection.Columns.Add("CBO_CODE", typeof(string));
                _dtLevelSection.Columns.Add("CBO_NAME", typeof(string));
                DataRow newRow = _dtLevelSection.NewRow();
                newRow["CBO_CODE"] = _strScrapSection;
                newRow["CBO_NAME"] = _strScrapSection;
                _dtLevelSection.Rows.Add(newRow);
                SetDataGridComboItemLocal(_dtLevelSection, dgScrapSection.Columns["LEVEL"], CommonCombo.ComboStatus.NONE);
            }
            {// ScrapSection 수집유형
                DataTable dtResult = GetEqptClctTypeCombo("SCRAP_SECTION");
                SetDataGridComboItemLocal(dtResult, dgScrapSection.Columns["ROLLMAP_CLCT_TYPE"], CommonCombo.ComboStatus.NONE);
            }
            {// ScrapSection 수집항목 TAG_SECTION
                DataTable dtResult = GetEqptMeasrPstnCombo("SCRAP_SECTION");
                SetDataGridComboItemLocal(dtResult, dgScrapSection.Columns["EQPT_MEASR_PSTN_ID"], CommonCombo.ComboStatus.NONE);
            }
            {// ScrapSection 활동유형
                DataTable dtResult = GetEqptDefectCode("SCRAP_SECTION", null);
                List<string> strList = new List<string>();
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string strData = dtResult.Rows[i]["RESNCODE"].ToString();
                    strList.Insert(i, strData);
                }
                var query = _dtReason.AsEnumerable().Where(o => strList.Contains(o.Field<string>("RESNCODE"))).ToList();
                DataTable dtResn = query.Any() ? query.CopyToDataTable() : _dtReason.Clone();
                SetDataGridComboItemLocal(dtResn, dgScrapSection.Columns["RESNCODE"], CommonCombo.ComboStatus.NONE);
            }
            {// 최외곽폐기 + 기타
                { // 최외곽폐기 수집항목
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["COM_TYPE_CODE"] = "ROLLMAP_RP_AUTO_RSLT_BAS_QTY";
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                    _dtOutsideComCode = dtResult.Copy();

                    if (dtResult.Rows.Count != 0)
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            if (i == 0) _strOutsideEqptPstnID = dtResult.Rows[i]["COM_CODE"].ToString();
                            else _strOutsideEqptPstnID = _strOutsideEqptPstnID + "," + dtResult.Rows[i]["COM_CODE"].ToString();
                        }
                    }
                }
                { // 최외과폐기 수집항목 2
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                    RQSTDT.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["USE_FLAG"] = "Y";
                    dr["EQPT_MEASR_PSTN_ID"] = _strOutsideEqptPstnID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_EQPT_MEASR_PSTN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    _dtOutsideEqptMeasrPstnID = dtResult.Copy();

                }
                { // 기타불량 수집항목
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["COM_TYPE_CODE"] = "ROLLMAP_ETC_LOSS_RP";
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                    _dtEtcLossComCode = dtResult.Copy();

                    if (dtResult.Rows.Count != 0)
                    {
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            if (i == 0) _strEtcLossEqptPstnID = dtResult.Rows[i]["COM_CODE"].ToString();
                            else _strEtcLossEqptPstnID = _strEtcLossEqptPstnID + "," + dtResult.Rows[i]["COM_CODE"].ToString();
                        }
                    }
                }
                {// 최외과폐기 수집항목 2
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                    RQSTDT.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["USE_FLAG"] = "Y";
                    dr["EQPT_MEASR_PSTN_ID"] = _strEtcLossEqptPstnID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_EQPT_MEASR_PSTN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    _dtEtcLossEqptMeasrPstnID = dtResult.Copy();
                }

                _dtLevelComCode = new DataTable("LEVEL");
                _dtLevelComCode.Columns.Add("CBO_CODE", typeof(string));
                _dtLevelComCode.Columns.Add("CBO_NAME", typeof(string));
                _dtLevelComCode.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                for (int i = 0; i < _dtOutsideComCode.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(_dtOutsideComCode.Rows[i]["ATTR3"].ToString()))
                    {
                        _dtOutsideComCode.Rows[i]["ATTR3"] = _dtOutsideComCode.Rows[i]["COM_CODE"];
                    }

                    string strMeasrPstnID = _dtOutsideComCode.Rows[i]["COM_CODE"].ToString();
                    string strCode = _dtOutsideComCode.Rows[i]["ATTR3"].ToString();
                    var query = _dtLevelComCode.AsEnumerable().Where(o => o.Field<string>("CBO_CODE").Equals(strCode)).ToList();
                    DataTable dtLevel = query.Any() ? query.CopyToDataTable() : _dtLevelComCode.Clone();
                    if (CommonVerify.HasTableRow(dtLevel) == true)
                    {
                        for (int j = 0; j < _dtLevelComCode.Rows.Count; j++)
                        {
                            if (_dtLevelComCode.Rows[j]["CBO_CODE"].ToString() == strCode)
                            {
                                _dtLevelComCode.Rows[j]["EQPT_MEASR_PSTN_ID"] = _dtLevelComCode.Rows[j]["EQPT_MEASR_PSTN_ID"].ToString() + "," + strMeasrPstnID;
                            }
                        }
                    }
                    else
                    {
                        DataRow newRow = _dtLevelComCode.NewRow();
                        newRow["CBO_CODE"] = strCode;
                        newRow["CBO_NAME"] = strCode;
                        newRow["EQPT_MEASR_PSTN_ID"] = strMeasrPstnID;
                        _dtLevelComCode.Rows.Add(newRow);
                    }
                }
                for (int i = 0; i < _dtEtcLossComCode.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(_dtEtcLossComCode.Rows[i]["ATTR3"].ToString()))
                    {
                        _dtEtcLossComCode.Rows[i]["ATTR3"] = _dtEtcLossComCode.Rows[i]["COM_CODE"];
                    }
                    string strCode = _dtEtcLossComCode.Rows[i]["ATTR3"].ToString();
                    string strMeasrPstnID = _dtEtcLossComCode.Rows[i]["COM_CODE"].ToString();
                    var query = _dtLevelComCode.AsEnumerable().Where(o => o.Field<string>("CBO_CODE").Equals(strCode)).ToList();
                    DataTable dtLevel = query.Any() ? query.CopyToDataTable() : _dtLevelComCode.Clone();
                    if (CommonVerify.HasTableRow(dtLevel) == true)
                    {
                        for (int j = 0; j < _dtLevelComCode.Rows.Count; j++)
                        {
                            if (_dtLevelComCode.Rows[j]["CBO_CODE"].ToString() == strCode)
                            {
                                _dtLevelComCode.Rows[j]["EQPT_MEASR_PSTN_ID"] = _dtLevelComCode.Rows[j]["EQPT_MEASR_PSTN_ID"].ToString() + "," + strMeasrPstnID;
                            }
                        }
                    }
                    else
                    {
                        DataRow newRow = _dtLevelComCode.NewRow();
                        newRow["CBO_CODE"] = strCode;
                        newRow["CBO_NAME"] = strCode;
                        newRow["EQPT_MEASR_PSTN_ID"] = strMeasrPstnID;
                        _dtLevelComCode.Rows.Add(newRow);
                    }
                }

                SetDataGridComboItemLocal(_dtLevelComCode, dgOutsideScrap.Columns["LEVEL"], CommonCombo.ComboStatus.NONE);

                SetLevelReason();


                if (_strOutsideEqptPstnID == "") _strOutsideEtcEqptPstnID = _strEtcLossEqptPstnID;
                else if (_strEtcLossEqptPstnID == "") _strOutsideEtcEqptPstnID = _strOutsideEqptPstnID;
                else _strOutsideEtcEqptPstnID = _strOutsideEqptPstnID + "," + _strEtcLossEqptPstnID;

                C1DataGrid dg = dgOutsideScrap;
                const string bizRuleName = "DA_PRD_SEL_RM_RPT_EQPT_MEASR_PSTN_CBO";
                string[] arrColumn = { "LANGID", "USE_FLAG", "EQPT_MEASR_PSTN_ID" };
                string[] arrCondition = { LoginInfo.LANGID, "Y", _strOutsideEtcEqptPstnID };
                string selectedValueText = dg.Columns["EQPT_MEASR_PSTN_ID"].SelectedValuePath();
                string displayMemberText = dg.Columns["EQPT_MEASR_PSTN_ID"].DisplayMemberPath();
                CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, dg.Columns["EQPT_MEASR_PSTN_ID"], selectedValueText, displayMemberText);
            }
            {//최외곽 폐기 + 기타 수집유형
                _dtOutsideEtcClctType = GetEqptClctTypeCombo(_strOutsideEtcEqptPstnID);
                SetDataGridComboItemLocal(_dtOutsideEtcClctType, dgOutsideScrap.Columns["ROLLMAP_CLCT_TYPE"], CommonCombo.ComboStatus.NONE);
            }
            {
                _dtOutsideClctType = GetEqptClctTypeCombo(_strOutsideEqptPstnID);   // 최외곽 폐기 수집유형
            }

        }

        private void GetReasonCode()
        {
            const string bizRuleName = "BR_PRD_SEL_WIPRESONCOLLECT_INFO";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("RESNPOSITION", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LANGID"] = LoginInfo.LANGID;
            inData["AREAID"] = LoginInfo.CFG_AREA_ID;
            inData["PROCID"] = wParams.processCode;
            inData["EQPTID"] = wParams.equipmentCode;
            inData["LOTID"] = txtLot.Text;
            inData["WIPSEQ"] = wParams.wipSeq;
            inData["ACTID"] = "LOSS_LOT,DEFECT_LOT,CHARGE_PROD_LOT";
            //inData["RESNPOSITION"] = "DEFECT_BACK";
            inDataTable.Rows.Add(inData);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            string[] exceptionCode = new string[] { "LENGTH_LACK", "LENGTH_EXCEED" };
            var queryResult = dtResult.AsEnumerable().Where(o => !exceptionCode.Contains(o.Field<string>("PRCS_ITEM_CODE"))).ToList();

            _dtReason = queryResult.Any() ? queryResult.CopyToDataTable() : dtResult.Clone();
        }

        private DataTable GetEqptClctTypeCombo(string strPstnID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = wParams.equipmentCode;
            dr["PROCID"] = wParams.processCode;
            dr["EQPT_MEASR_PSTN_ID"] = strPstnID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_DEFECT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
        }

        private DataTable GetEqptMeasrPstnCombo(string strPstnID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("USE_FLAG", typeof(string));
            RQSTDT.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["USE_FLAG"] = "Y";
            dr["EQPT_MEASR_PSTN_ID"] = strPstnID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_EQPT_MEASR_PSTN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
        }

        private void SetLevelReason()
        {
            if (CommonVerify.HasTableRow(_dtLevelComCode) == false) return;

            _dsLevelReasonCode = new DataSet();

            for (int i = 0; i < _dtLevelComCode.Rows.Count; i++)
            {
                string strLEVEL = _dtLevelComCode.Rows[i]["CBO_CODE"].GetString();
                string strPstn = _dtLevelComCode.Rows[i]["EQPT_MEASR_PSTN_ID"].GetString();

                DataTable IndataTable = new DataTable(strLEVEL);

                DataTable dtResult = GetEqptDefectCode(strPstn, null);
                IndataTable = dtResult.Copy();
                IndataTable.TableName = strLEVEL;

                //IndataTable = GetEqptDefectCode(strPstn, null);

                for (int j = 0; j < _dtOutsideComCode.Rows.Count; j++)
                {
                    if (strLEVEL != _dtOutsideComCode.Rows[j]["ATTR3"].ToString()) continue;
                    if (_dtOutsideComCode.Rows[j]["ATTR1"].ToString() == "") continue;

                    DataRow Indata = IndataTable.NewRow();
                    Indata["RESNCODE"] = _dtOutsideComCode.Rows[j]["ATTR1"].ToString();
                    IndataTable.Rows.Add(Indata);
                }

                _dsLevelReasonCode.Tables.Add(IndataTable);


            }
        }

        private DataTable GetEqptDefectCode(string strPstnID, string strResnCode)
        {
            const string bizRuleName = "DA_PRD_SEL_RM_COM_EQPT_INSP_DFCT_CODE";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USE_FLAG", typeof(string));
            inDataTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("DFCT_AUTO_APPLY_FLAG", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LANGID"] = LoginInfo.LANGID;
            inData["PROCID"] = wParams.processCode;
            inData["EQPTID"] = wParams.equipmentCode;
            inData["USE_FLAG"] = "Y";
            inData["DFCT_AUTO_APPLY_FLAG"] = "Y";
            inData["EQPT_MEASR_PSTN_ID"] = strPstnID;
            inData["RESNCODE"] = strResnCode;


            inDataTable.Rows.Add(inData);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            return dtResult;
        }

        private void SetDataGridComboItemLocal(DataTable dtValue, C1.WPF.DataGrid.DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            try
            {
                string selectedValueText = dgcol.SelectedValuePath();
                string displayMemberText = dgcol.DisplayMemberPath();

                DataTable dtBinding = dtValue.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = StatusAddLocal(dtBinding, status, selectedValueText, displayMemberText).Copy().AsDataView();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable StatusAddLocal(DataTable dt, CommonCombo.ComboStatus cs, string selectedValueText, string displayMemberText)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[selectedValueText] = null;
                    dr[displayMemberText] = "-ALL-";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[selectedValueText] = "SELECT";
                    dr[displayMemberText] = "-SELECT-";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[selectedValueText] = string.Empty;
                    dr[displayMemberText] = "-N/A-";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NONE:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(cs), cs, null);
            }

            return dt;
        }



        #endregion

        #region InitializeControls
        /// <summary>
        /// 조회시 컨트롤 초기화 및 DataTable 초기화
        /// </summary>
        private void InitializeControls()
        {

            if (grdOutputDefectLegend.ColumnDefinitions.Count > 0) grdOutputDefectLegend.ColumnDefinitions.Clear();
            if (grdOutputDefectLegend.RowDefinitions.Count > 0) grdOutputDefectLegend.RowDefinitions.Clear();
            grdOutputDefectLegend.Children.Clear();

            tbOutputCore.Text = string.Empty;

            tbOutputProdQty.Text = string.Empty;
            tbOutputGoodQty.Text = string.Empty;

            tbOutputTopupper.Text = string.Empty;
            tbOutputToplower.Text = string.Empty;
            tbOutputBackupper.Text = string.Empty;
            tbOutputBacklower.Text = string.Empty;
            tbOutputRwEnd.Text = string.Empty;

            isRollMapLot = SelectRollMapLot(wParams.lotId, wParams.wipSeq);
            isRollMapResultLink = IsRollMapResultApply(wParams.processCode, wParams.equipmentSegmentCode);

            Util.gridClear(dgLotInfo);
            Util.gridClear(dgTagSectionSingle);
            Util.gridClear(dgOutsideScrap);
            Util.gridClear(dgScrapSection);
            Util.gridClear(dgTagDefectMes);



        }
        #endregion

        #region InitializeDataTables
        /// <summary>
        /// 불량정보, 측정정보, 레인정보 DataTable 초기화 
        /// </summary>
        private void InitializeDataTables()
        {

            if (dtDefectOutput != null && dtDefectOutput.Rows.Count > 0) { dtDefectOutput.Clear(); }
            if (dtDefectInput != null && dtDefectInput.Rows.Count > 0) { dtDefectInput.Clear(); }
            if (dtBaseDefectInput != null && dtBaseDefectInput.Rows.Count > 0) { dtBaseDefectInput.Clear(); }
            if (dtBaseDefectOutput != null && dtBaseDefectOutput.Rows.Count > 0) { dtBaseDefectOutput.Clear(); }
            if (dtOutGauge != null && dtOutGauge.Rows.Count > 0) { dtOutGauge.Clear(); }
            if (dtLane != null && dtLane.Rows.Count > 0) { dtLane.Clear(); }
        }
        #endregion

        #region Get2DBarcodeInfo
        /// <summary>
        /// LOTID 기준 해당 설비(EQUIPMENTATTR(S99)) 2D 바코드 정보와 LOTATTR(LANE_QTY) 정보 조회
        /// </summary>
        /// <param name="lotId"></param>
        /// <param name="eqptId">사용안함</param>
        /// <returns></returns>
        private DataTable Get2DBarcodeInfo(string lotId, string eqptId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotId;
            dr["EQPTID"] = eqptId;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_RM_RPT_DFCT_2D_BCR_STD", "RQSTDT", "RSLTDT", dt);
        }
        #endregion

        #region GetInOutProcessMaxLength
        /// <summary>
        /// IN/OUT LOTID 기준 1.공정 2.길이 3.생산량 4.양품량 정보 조회 및 셋팅
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void GetInOutProcessMaxLength(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
                         {
                             ProcessName = t.Field<string>("PROCNAME"),
                             EndPosition = t.GetValue("END_PSTN").GetDecimal(),   // t.Field<decimal>("END_PSTN"),
                             ProdQty = t.GetValue("INPUT_QTY").GetDecimal(),  //t.Field<decimal?>("INPUT_QTY"),
                             GoodQty = t.GetValue("WIPQTY_ED").GetDecimal() //t.Field<decimal?>("WIPQTY_ED")
                         }).FirstOrDefault();

            if (query != null)
            {
                tbOutput.Text = query.ProcessName;
                xOutputMaxLength = query.EndPosition.GetDouble();
            }
        }
        #endregion

        #region InitializeChartView
        /// <summary>
        /// 모든 차트 초기화 작업(X,Y축 {MajorGridStrokeThickness/MinorGridStrokeThickness} {MajorTickThickness/MinorTickThickness} {Foreground})
        /// </summary>
        /// <param name="c1Chart"></param>
        private void InitializeChartView(C1Chart c1Chart)
        {
            c1Chart.View.AxisX.MajorGridStrokeThickness = 0;
            c1Chart.View.AxisX.MinorGridStrokeThickness = 0;
            c1Chart.View.AxisY.MajorGridStrokeThickness = 0;
            c1Chart.View.AxisY.MinorGridStrokeThickness = 0;
            c1Chart.View.AxisX.MajorTickThickness = 0;
            c1Chart.View.AxisX.MinorTickThickness = 0;
            c1Chart.View.AxisY.MajorTickThickness = 0;
            c1Chart.View.AxisY.MinorTickThickness = 0;
            c1Chart.View.AxisY.Foreground = new SolidColorBrush(Colors.Transparent);


            if (c1Chart.Name == "chartInput" || c1Chart.Name == "chartOutput")
            {
                double majorUnit;
                double length = c1Chart.Name == "chartInput" ? xInputMaxLength : xOutputMaxLength;

                if (length < 11)
                {
                    majorUnit = 1;
                }
                else if (length < 101)
                {
                    majorUnit = 10;
                }
                else
                {
                    majorUnit = Math.Round(length / 200) * 10;
                }

                c1Chart.View.AxisX.MinorTickOverlap = 20;
                c1Chart.View.AxisX.MajorGridStrokeDashes = new DoubleCollection { 3, 2 };
                c1Chart.View.AxisX.AutoMax = false;
                c1Chart.View.AxisX.AutoMin = false;
                c1Chart.View.AxisX.MajorUnit = majorUnit;//X축 좌표 간격
                c1Chart.View.AxisX.AnnoFormat = "#,##0";
            }
            else
            {
                c1Chart.View.AxisX.Foreground = new SolidColorBrush(Colors.Transparent);
            }
        }
        #endregion

        #region InitializeChart
        /// <summary>
        /// IN/OUT 차트 초기화(1.X축 범위(0 ~ 길이) 2.Y축 범위(-40 ~ 100)
        /// </summary>
        /// <param name="chart"></param>
        private void InitializeChart(C1Chart chart)
        {
            double maxLength = 10;

            if (string.Equals(chart.Name, "chartInput"))
            {
                maxLength = xInputMaxLength.Equals(0) ? 10 : xInputMaxLength;
            }
            else
            {
                maxLength = xOutputMaxLength.Equals(0) ? 10 : xOutputMaxLength;
            }

            chart.View.AxisX.Min = 0;
            chart.View.AxisY.Min = -40;
            //chart.View.AxisY.Min = chart.Name == "chartInput" ? -10 : -30; // TODO 수정 필요 함. 차트 하단의 태그.. 등의 표시로  AxisY Min 값을 설정 함.
            chart.View.AxisX.Max = maxLength;
            chart.View.AxisY.Max = 100;
            InitializeChartView(chart);
        }
        #endregion

        #region DrawStartEndYAxis
        /// <summary>
        /// chartInput/chartOutput 상단 영역에 길이(파란색), LOT_CUT(가위), LOT_MERGE 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawStartEndYAxis(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            #region 1.chartInput, chartOutput 상단 RW 길이(파란색) 표시
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select
                            new
                            {
                                StartPosition = t.GetValue("STRT_PSTN").GetDecimal(),  //t.Field<decimal?>("STRT_PSTN"),
                                EndPosition = t.GetValue("END_PSTN").GetDecimal() // t.Field<decimal>("END_PSTN")
                            }).FirstOrDefault();

            if (query != null)
            {
                DataTable dtLength = new DataTable();
                dtLength.Columns.Add("RAW_END_PSTN", typeof(string));
                dtLength.Columns.Add("SOURCE_Y_PSTN", typeof(double));
                dtLength.Columns.Add("FONTSIZE", typeof(double));

                DataRow newRow = dtLength.NewRow();
                newRow["RAW_END_PSTN"] = "0";
                newRow["SOURCE_Y_PSTN"] = 100;
                newRow["FONTSIZE"] = 11;
                dtLength.Rows.Add(newRow);

                if (chartConfigurationType == ChartConfigurationType.Output)
                {
                    //tbOutputRwEnd
                    tbOutputRwEnd.Text = $"{query.EndPosition.GetDouble():###,###,###,##0.##}";
                }
                else
                {
                    DataRow newLength = dtLength.NewRow();
                    newLength["RAW_END_PSTN"] = $"{query.EndPosition:###,###,###,##0.##}";
                    newLength["SOURCE_Y_PSTN"] = 100;
                    newLength["FONTSIZE"] = 20;
                    dtLength.Rows.Add(newLength);
                }

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtLength);
                ds.XValueBinding = new Binding("RAW_END_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartLength"] as DataTemplate;   //chartRewinderLength
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Cursor = Cursors.Hand;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };

                chart.Data.Children.Add(ds);
            }
            #endregion

            #region 2.chartInput, chartOutput 차트 상단영역 LOT Cut(가위) 표시
            var querycut = (from t in dt.AsEnumerable()
                            where t.Field<string>("EQPT_MEASR_PSTN_ID") == "CUT_PSTN"
                            select
                            new
                            {
                                StartPosition = t.GetValue("CUM_STRT_PSTN").GetDecimal(), // t.Field<decimal?>("CUM_STRT_PSTN"),
                                EndPosition = t.GetValue("CUM_END_PSTN").GetDecimal() // t.Field<decimal>("CUM_END_PSTN")
                            }).ToList();

            if (querycut.Any())
            {
                DataTable dtLotCut = new DataTable();
                dtLotCut.Columns.Add(new DataColumn() { ColumnName = "RAW_END_PSTN", DataType = typeof(double) });
                dtLotCut.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });
                dtLotCut.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (var item in querycut)
                {
                    DataRow newLotCut = dtLotCut.NewRow();
                    newLotCut["RAW_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                    newLotCut["SOURCE_Y_PSTN"] = 97;
                    newLotCut["TOOLTIP"] = ObjectDic.Instance.GetObjectName("CUT_SPLIT") + " : " + $"{item.EndPosition:###,###,###,##0.##}";
                    dtLotCut.Rows.Add(newLotCut);
                }

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtLotCut);
                ds.XValueBinding = new Binding("RAW_END_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartLotCut"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Cursor = Cursors.Hand;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                chart.Data.Children.Add(ds);
            }
            #endregion

            #region 3.chartInput, chartOutput 차트 영역 상단 LOT MergeLot(-><-) 영역 표시
            var queryMergeLot = (from t in dt.AsEnumerable()
                                 where t.Field<string>("EQPT_MEASR_PSTN_ID") == "MERGE_PSTN"
                                 select
                                 new
                                 {
                                     InputLotId = t.Field<string>("INPUT_LOTID"),
                                     StartPosition = t.GetValue("CUM_STRT_PSTN").GetDecimal(), // t.Field<decimal>("CUM_STRT_PSTN"),
                                     EndPosition = t.GetValue("CUM_END_PSTN").GetDecimal() //t.Field<decimal>("CUM_END_PSTN")
                                 }).ToList();

            if (queryMergeLot.Any())
            {
                DataTable dtMergeLot = new DataTable();
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_LOTID", DataType = typeof(string) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_STRT_PSTN", DataType = typeof(double) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_END_PSTN", DataType = typeof(double) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "Y_PSTN", DataType = typeof(double) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_VISIVILITY", DataType = typeof(string) }); //Collapsed, Visible

                foreach (var item in queryMergeLot)
                {
                    DataRow drMergeLot = dtMergeLot.NewRow();
                    drMergeLot["ADJ_LOTID"] = item.InputLotId;
                    drMergeLot["ADJ_STRT_PSTN"] = $"{item.StartPosition:###,###,###,##0.##}";
                    drMergeLot["ADJ_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                    drMergeLot["Y_PSTN"] = 100;//차트 상단
                    drMergeLot["TOOLTIP"] = "LOT ID : " + item.InputLotId + " ( " + $"{item.StartPosition.GetDouble():###,###,###,##0.##}" + " ~ " + $"{item.EndPosition.GetDouble():###,###,###,##0.##}" + ")";
                    drMergeLot["ADJ_VISIVILITY"] = "Visible";
                    dtMergeLot.Rows.Add(drMergeLot);
                }

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtMergeLot);
                ds.XValueBinding = new Binding("ADJ_END_PSTN");
                ds.ValueBinding = new Binding("Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartMerge"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Cursor = Cursors.Hand;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                chart.Data.Children.Add(ds);
            }
            #endregion
        }
        #endregion


        #region DrawLaneText
        /// <summary>
        /// chartInput/chartOutput 영역에 Lane 텍스트 표시
        /// </summary>
        /// <param name="firstlaneRate">레인 Y축 시작 위치(100 기준)</param>
        /// <param name="lastlaneRate">레인 Y축 종료 위치(100 기준)</param>
        /// <param name="laneNo">레인</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawLaneText(double firstlaneRate, double lastlaneRate, string laneNo, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart;
            double startPosition;

            if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            {
                c1Chart = chartInput;
                startPosition = xInputMaxLength - (xInputMaxLength * 0.01);   //chartInput 레인 텍스트 X축 시작위치
            }
            else
            {
                c1Chart = chartOutput;
                startPosition = xOutputMaxLength - (xOutputMaxLength * 0.01); //chartOutput 레인 텍스트 X축 시작위치
            }
            DataTable dtLane = new DataTable();
            dtLane.Columns.Add("SOURCE_STRT_PSTN", typeof(string));
            dtLane.Columns.Add("SOURCE_Y_PSTN", typeof(string));
            dtLane.Columns.Add("LANEINFO", typeof(string));

            DataRow dr = dtLane.NewRow();
            dr["SOURCE_STRT_PSTN"] = startPosition;
            dr["SOURCE_Y_PSTN"] = (firstlaneRate + lastlaneRate) / 2 - 1;//레인 Y축 중간 위치
            dr["LANEINFO"] = "Lane " + laneNo;
            dtLane.Rows.Add(dr);

            XYDataSeries dsLane = new XYDataSeries();
            dsLane.ItemsSource = DataTableConverter.Convert(dtLane);
            dsLane.XValueBinding = new Binding("SOURCE_STRT_PSTN");
            dsLane.ValueBinding = new Binding("SOURCE_Y_PSTN");
            dsLane.ChartType = ChartType.XYPlot;
            dsLane.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsLane.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsLane.PointLabelTemplate = grdMain.Resources["chartLane"] as DataTemplate;//레인 텍스트
            dsLane.Margin = new Thickness(0, 0, 0, 0);

            dsLane.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };

            c1Chart.Data.Children.Add(dsLane);
        }
        #endregion

        #region DrawLane
        /// <summary>
        /// chartInput/chartOutput Lane 표시 (무지부 유지부 색상 표현 및 Top, Back의 Lane 표시) 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawLane(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            var convertFromString = ColorConverter.ConvertFromString("#D5D5D5"); //무지부 색상

            double axisXnear = 0;                                                                                             //x축 시작점
            double axisXfar = (ChartConfigurationType.Input == chartConfigurationType) ? xInputMaxLength : xOutputMaxLength; //x축 종료점

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Plain")
                {
                    #region 무지부 Lane 지정(AlarmZone : 영역)
                    AlarmZone alarmZone = new AlarmZone();
                    alarmZone.Near = axisXnear;
                    alarmZone.Far = axisXfar;
                    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                    alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    chart.Data.Children.Add(alarmZone);
                    #endregion
                }
                else if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Coat")
                {
                    #region 유지부 Lane 지정(AlarmZone : 영역[투명])
                    AlarmZone alarmZone = new AlarmZone();
                    alarmZone.Near = axisXnear;
                    alarmZone.Far = axisXfar;
                    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                    alarmZone.ConnectionStroke = new SolidColorBrush(Colors.Transparent);
                    alarmZone.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    chart.Data.Children.Add(alarmZone);
                    #endregion
                }
            }

            #region Lane 텍스트 차트에 표시
            var queryLane = (from t in dt.AsEnumerable()
                             where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX"
                             select new
                             {
                                 LaneNo = t.Field<string>("LANE_NO_CUR")
                             })
                             .Distinct().ToList();

            //int laneCount = queryLane.Count;

            if (dtLane != null && dtLane.Rows.Count > 0)
            {
                dtLane.Clear();
            }

            if (queryLane.Any())
            {
                foreach (var item in queryLane)
                {
                    DataRow dr = dtLane.NewRow();
                    dr["LANE_NO"] = item.LaneNo;
                    dtLane.Rows.Add(dr);
                }
            }

            var queryTopBack = (from t in dt.AsEnumerable()
                                select new
                                {
                                    TopBackFlag = t.Field<string>("T_B_FLAG")
                                })
                                .Distinct().ToList();

            //int topBackCount = queryTopBack.Count;

            //Top, Back 표현
            if (queryTopBack.Any())
            {
                foreach (var item in queryTopBack)
                {
                    #region Top, Back별 Lane 텍스트 표시
                    if (queryLane.Any())
                    {
                        foreach (var itemLane in queryLane)
                        {
                            var queryTopBackLaneRate = dt.AsEnumerable().Where(x => x.Field<string>("T_B_FLAG") == item.TopBackFlag
                                                                                 && x.Field<string>("LANE_NO_CUR") == itemLane.LaneNo
                                                                                 && x.Field<string>("LOC2") != "XX")
                                                                        .GroupBy(g => new {
                                                                            TopBackFlag = g.Field<string>("T_B_FLAG")
                                                                                              ,
                                                                            LaneNo = g.Field<string>("LANE_NO_CUR")
                                                                        })
                                                                        .Select(s => new
                                                                        {
                                                                            TopBackFlag = s.Key.TopBackFlag,
                                                                            LaneNo = s.Key.LaneNo,
                                                                            FirstTopBackLaneRate = s.Min(z => z.GetValue("Y_PSTN_STRT_RATE").GetDecimal()),
                                                                            LastTopBackLaneRate = s.Max(z => z.GetValue("Y_PSTN_END_RATE").GetDecimal())
                                                                        }).FirstOrDefault();

                            if (queryTopBackLaneRate != null)
                            {
                                DrawLaneText(queryTopBackLaneRate.FirstTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LastTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LaneNo, chartConfigurationType);
                            }

                        }
                    }
                    #endregion
                }
            }
            #endregion
        }
        #endregion

        #region DrawWinderDirection
        /// <summary>
        /// chartOutput 상단 차트에 UW/RW 정보 표시
        /// </summary>
        /// <param name="dtLength">레인정보</param>
        private void DrawWinderDirection(DataTable dtLength)
        {
            #region 차트 초기화
            chart.View.AxisX.MajorGridStrokeThickness = 0;
            chart.View.AxisX.MinorGridStrokeThickness = 0;
            chart.View.AxisY.MajorGridStrokeThickness = 0;
            chart.View.AxisY.MinorGridStrokeThickness = 0;
            //chart.View.AxisX.LogBase = 3;
            chart.View.AxisX.MinorTickOverlap = 30;
            chart.View.AxisX.MajorGridStrokeDashes = new DoubleCollection { 3, 2 };
            // hide all tick marks
            chart.View.AxisX.MajorTickThickness = 0;
            chart.View.AxisX.MinorTickThickness = 0;
            chart.View.AxisY.MajorTickThickness = 0;
            chart.View.AxisY.MinorTickThickness = 0;
            //
            chart.View.AxisX.AutoMax = false;
            chart.View.AxisX.AutoMin = false;

            // hide axis labels
            chart.View.AxisY.Foreground = new SolidColorBrush(Colors.Transparent);
            chart.View.AxisX.Foreground = new SolidColorBrush(Colors.Transparent);
            chart.View.Margin = new Thickness(2, 1, 2, 1);
            #endregion

            chart.View.AxisY.Min = 0;
            chart.View.AxisY.Max = 100;
            chart.View.AxisX.Min = 0;
            chart.View.AxisX.Max = dtLength.AsEnumerable().ToList().Max(r => r["TOTAL_WIDTH2"].GetDouble()).GetDouble();

            #region 권출/권취 차트 바인딩 DataTable
            DataTable dt = new DataTable();
            dt.Columns.Add("SOURCE_END_PSTN", typeof(double));
            dt.Columns.Add("ARROW_PSTN", typeof(double));
            dt.Columns.Add("SOURCE_Y_PSTN", typeof(double));
            dt.Columns.Add("ARROW_Y_PSTN", typeof(double));
            dt.Columns.Add("COLORMAP", typeof(string));
            dt.Columns.Add("CIRCLENAME", typeof(string));
            dt.Columns.Add("WND_DIRCTN", typeof(string));
            dt.Columns.Add("TOOLTIP", typeof(string));
            #endregion

            double arrowYPosition;
            double arrowPosition;
            double arrowValue = dtLength.AsEnumerable().ToList().Max(r => r["TOTAL_WIDTH2"].GetDouble()).GetDouble() * 0.025;

            #region 권취(RW) 정보 생성
            //Rewinder 권취 방향 (RW_DIRCTN "1" 이면 상권취 "0" 이면 하권취)
            //(코터인 경우, 2층 구조로 작업자 관점에서 권취정보가 반대. RW_DIRCTN "1" 이면 하권취 "0" 이면 상권취)
            //arrowYPosition = dtLength.Rows[0]["RW_DIRCTN"].GetInt() == 1 ? 60 : 0;    //화살표 Y축 위치(변경되어야 할 소스)
            arrowYPosition = dtLength.Rows[0]["RW_DIRCTN"].GetInt() == 1 ? 0 : 60;      //화살표 Y축 위치(기존소스)
            arrowPosition = dtLength.Rows[0]["TOTAL_WIDTH2"].GetDouble() - arrowValue; //RW Circle 보다 앞쪽 위치

            DataRow dr = dt.NewRow();
            dr["SOURCE_END_PSTN"] = dtLength.Rows[0]["TOTAL_WIDTH2"];
            dr["ARROW_PSTN"] = arrowPosition;
            dr["SOURCE_Y_PSTN"] = 0;
            dr["ARROW_Y_PSTN"] = arrowYPosition;
            dr["CIRCLENAME"] = "RW";
            dr["COLORMAP"] = "#000000";
            dt.Rows.Add(dr);
            #endregion

            #region 권출(UW) 정보 생성
            //UnWinder 권출 방향 (UW_DIRCTN "1" 이면 상권출 "0" 이면 하권출)
            arrowYPosition = dtLength.Rows[0]["UW_DIRCTN"].GetInt() == 1 ? 60 : 0; //화살표 Y축 위치
            arrowPosition = arrowValue;                                           //UW Circle 보다 뒷쪽 위치

            DataRow newRow = dt.NewRow();
            newRow["SOURCE_END_PSTN"] = 0;
            newRow["ARROW_PSTN"] = arrowPosition;
            newRow["SOURCE_Y_PSTN"] = 0;
            newRow["ARROW_Y_PSTN"] = arrowYPosition;
            newRow["CIRCLENAME"] = "UW";
            newRow["COLORMAP"] = "#000000";
            dt.Rows.Add(newRow);
            #endregion

            #region Circle(UW/RW) 차트 추가
            XYDataSeries ds = new XYDataSeries();
            ds.ItemsSource = DataTableConverter.Convert(dt);
            ds.XValueBinding = new Binding("SOURCE_END_PSTN");
            ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
            ds.ChartType = ChartType.XYPlot;
            ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
            ds.PointLabelTemplate = grdMain.Resources["chartCircle"] as DataTemplate;
            ds.Margin = new Thickness(0, 0, 0, 0);

            ds.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
                pe.Size = new Size(50, 50);
            };
            chart.Data.Children.Add(ds);
            #endregion

            #region 화살표(->) 차트 추가
            XYDataSeries dsArrowRight = new XYDataSeries();
            dsArrowRight.ItemsSource = DataTableConverter.Convert(dt);
            dsArrowRight.XValueBinding = new Binding("ARROW_PSTN");
            dsArrowRight.ValueBinding = new Binding("ARROW_Y_PSTN");
            dsArrowRight.ChartType = ChartType.XYPlot;
            dsArrowRight.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsArrowRight.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsArrowRight.PointLabelTemplate = grdMain.Resources["arrowRight"] as DataTemplate;

            dsArrowRight.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };
            chart.Data.Children.Add(dsArrowRight);
            #endregion
        }
        #endregion

        #region DisplayTabLocation
        /// <summary>
        /// Tab 정보 표시
        /// </summary>
        /// <param name="dtTab">레인정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DisplayTabLocation(DataTable dtTab, ChartConfigurationType chartConfigurationType)
        {
            if (CommonVerify.HasTableRow(dtTab) && dtTab.Columns.Contains("TAB"))
            {
                bool isupper = false;
                bool islower = false;

                if (dtTab.AsEnumerable().Any(x => !string.IsNullOrEmpty(x.Field<string>("TAB"))
                                               && x.Field<string>("TAB") == "1"
                                               && x.Field<string>("COATING_PATTERN") == "Plain"))
                {
                    /********************************************************************
                     * 1. 유지부(Coat)에 Tab 있는 경우 제외. 무지부(Plain)에만 존재해야 함.
                     * 2. Lane 이 다수인 경우 제외? 
                     * 3. 유지부 상단 하단에 Tab 값이 1 인경우 upper, lower 모두 표시                     
                     *******************************************************************/
                    var query = (from t in dtTab.AsEnumerable()
                                 where !string.IsNullOrEmpty(t.Field<string>("TAB"))
                                      && t.Field<string>("TAB") == "1"
                                      && t.Field<string>("COATING_PATTERN") == "Plain"
                                 select new
                                 {
                                     LaneNo = t.Field<string>("LANE_NO_CUR")
                                   ,
                                     Tab = t.Field<string>("TAB")
                                 })
                                 .Distinct().ToList();

                    if (query.Count > 1)
                    {
                        // Lane 이 다수인 경우 Tab 표시하지 않음.
                        return;
                    }
                    else
                    {
                        var queryTab = (from t in dtTab.AsEnumerable()
                                        where !string.IsNullOrEmpty(t.Field<string>("TAB"))
                                           && t.Field<string>("TAB") == "1"
                                           && t.Field<string>("COATING_PATTERN") == "Plain"
                                        select t).ToList();

                        if (queryTab.Any())
                        {
                            foreach (var item in queryTab)
                            {
                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString()
                                                               && x.Field<string>("COATING_PATTERN") == "Coat"
                                                               && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString()
                                                               && x.GetValue("CNT").GetInt() < item["CNT"].GetInt()))
                                {
                                    islower = true;
                                }

                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString()
                                                               && x.Field<string>("COATING_PATTERN") == "Coat"
                                                               && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString()
                                                               && x.GetValue("CNT").GetInt() > item["CNT"].GetInt()))
                                {
                                    isupper = true;
                                }
                            }

                            if (isupper)
                            {
                                tbOutputTopupper.Text = "Tab";
                                tbOutputBackupper.Text = "Tab";
                            }
                            if (islower)
                            {
                                tbOutputToplower.Text = "Tab";
                                tbOutputBacklower.Text = "Tab";
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }
        #endregion

        #region DrawPetScrap
        /// <summary>
        /// PET(이음매), SCRAP 시작위치 표시(역삼각형)
        /// </summary>
        /// <param name="dtPetScrap"></param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawPetScrap(DataTable dtPetScrap, ChartConfigurationType chartConfigurationType)
        {
            // 롤프레스 공정에서만 PET, SCRAP 호출 함.
            if (chartConfigurationType == ChartConfigurationType.Input) return;

            double lowerExtent = 0;
            double upperExtent = 100;

            DataTable dt = new DataTable();
            dt.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            dt.Columns.Add("RAW_STRT_PSTN", typeof(double));
            dt.Columns.Add("RAW_END_PSTN", typeof(double));
            dt.Columns.Add("Y_PSTN", typeof(double));
            dt.Columns.Add("COLORMAP", typeof(string));
            dt.Columns.Add("TOOLTIP", typeof(string));
            dt.Columns.Add("TAG", typeof(string));

            foreach (DataRow row in dtPetScrap.Rows)
            {
                var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));

                string colorMap = row["COLORMAP"].GetString();

                string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] "
                               + ObjectDic.Instance.GetObjectName("위치") + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m "
                               + ObjectDic.Instance.GetObjectName("길이") + $"{row["WND_LEN"]:###,###,###,##0.##}" + "m";
                string tag = row["EQPT_MEASR_PSTN_ID"].GetString()
                               + ";" + row["ADJ_STRT_PSTN"].GetString() + ";" + row["ADJ_END_PSTN"].GetString()
                               + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString()
                               + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                chartOutput.Data.Children.Add(new XYDataSeries()
                {
                    ChartType = ChartType.Line,
                    XValuesSource = new[] { row["ADJ_STRT_PSTN"], row["ADJ_STRT_PSTN"] },
                    ValuesSource = new double[] { lowerExtent, upperExtent },
                    ConnectionStroke = convertFromString,
                    ConnectionStrokeThickness = 3,
                    ConnectionFill = convertFromString,
                });

                dt.Rows.Add(row["EQPT_MEASR_PSTN_ID"], row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"], 97, colorMap, content, tag);
            }

            if (CommonVerify.HasTableRow(dt))
            {
                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dt);
                dsPetScrap.XValueBinding = new Binding("RAW_STRT_PSTN");
                dsPetScrap.ValueBinding = new Binding("Y_PSTN");
                dsPetScrap.ChartType = ChartType.XYPlot;
                dsPetScrap.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.SymbolFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.PointLabelTemplate = grdMain.Resources["chartPetScrap"] as DataTemplate;
                dsPetScrap.Margin = new Thickness(0, 0, 0, 0);

                dsPetScrap.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };

                chartOutput.Data.Children.Add(dsPetScrap);
            }

        }
        #endregion

        #region DrawRewinderScrap
        /// <summary>
        /// RW 이후 수동 SCRAP(마지막 위치를 선으로 표시하고, 위에 역삼각형 표시)
        /// </summary>
        /// <param name="dtRewinderScrap"></param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawRewinderScrap(DataTable dtRewinderScrap, ChartConfigurationType chartConfigurationType)
        {
            //롤프레스 이후로 제한
            if (chartConfigurationType == ChartConfigurationType.Input) return;

            if (CommonVerify.HasTableRow(dtRewinderScrap))
            {
                dtRewinderScrap.Columns.Add("TOOLTIP", typeof(string));
                dtRewinderScrap.Columns.Add("TAG", typeof(string));
                dtRewinderScrap.Columns.Add("Y_PSTN", typeof(double));

                double lowerExtent = 0;
                double upperExtent = 100;

                foreach (DataRow row in dtRewinderScrap.Rows)
                {
                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));

                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString()
                                   + ";" + row["SOURCE_ADJ_STRT_PSTN"].GetString() + ";" + row["SOURCE_ADJ_END_PSTN"].GetString()
                                   + ";" + row["ADJ_LOTID"].GetString()
                                   + ";" + row["ADJ_WIPSEQ"].GetString()
                                   + ";" + row["CLCT_SEQNO"].GetString()
                                   + ";" + row["ROLLMAP_CLCT_TYPE"].GetString()
                                   + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString()
                                   + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m"
                                   + " ~ "
                                   + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + " ]";

                    row["TOOLTIP"] = toolTip;
                    row["TAG"] = tag;
                    row["Y_PSTN"] = 97;

                    chartOutput.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["SOURCE_ADJ_END_PSTN"], row["SOURCE_ADJ_END_PSTN"] },
                        ValuesSource = new double[] { lowerExtent, upperExtent },
                        ConnectionStroke = convertFromString,
                        ConnectionStrokeThickness = 3,
                        ConnectionFill = convertFromString,
                    });
                }

                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dtRewinderScrap);
                dsPetScrap.XValueBinding = new Binding("SOURCE_ADJ_END_PSTN");
                dsPetScrap.ValueBinding = new Binding("Y_PSTN");
                dsPetScrap.ChartType = ChartType.XYPlot;
                dsPetScrap.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.SymbolFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.PointLabelTemplate = grdMain.Resources["chartPetScrap"] as DataTemplate;
                dsPetScrap.Margin = new Thickness(0, 0, 0, 0);

                dsPetScrap.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };

                chartOutput.Data.Children.Add(dsPetScrap);
            }
        }
        #endregion

        #region MakeTableForDisplay
        /// <summary>
        /// chartInput, chartOutput에 바인딩 전 ChartDisplayType타입에 따른 Y좌표 생성 및 ToolTip 생성
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartDisplayType">MarkLabel/TagSectionStart/TagSectionEnd/TagSpot/RewinderScrap</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        /// <returns></returns>
        private DataTable MakeTableForDisplay(DataTable dt, ChartDisplayType chartDisplayType, ChartConfigurationType chartConfigurationType)
        {
            var dtBinding = dt.Copy();

            if (!CommonVerify.HasTableRow(dt)) return dtBinding;

            if (chartDisplayType == ChartDisplayType.MarkLabel)
            {
                #region ChartDisplayType.MarkLabel (SOURCE_ADJ_Y_PSTN = -16)
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = -16;
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[" + row["ROLLMAP_CLCT_TYPE"] + "],"
                                             + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";
                }
                #endregion
            }
            else if (chartDisplayType == ChartDisplayType.TagSpot)
            {
                #region ChartDisplayType.TagSpot (SOURCE_ADJ_Y_PSTN = -40)
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    //row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -8 : -30;
                    row["SOURCE_ADJ_Y_PSTN"] = -40;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + " : " + row["ABBR_NAME"] + " [" + sourceStartPosition + "m]";
                    row["TAG"] = null;
                }
                #endregion
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionStart || chartDisplayType == ChartDisplayType.TagSectionEnd)
            {
                #region ChartDisplayType.TagSectionStart / ChartDisplayType.TagSectionEnd
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });//데이터 원천이 코터는 불량데이터, 롤프레스는 측정 데이터라 컬럼 틀림
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "BORDERTAG", DataType = typeof(string) });

                #region [AS-IS] NFF 추가로직(EQPT_MEASR_PSTN_ID(TAG_SECTION)인 데이터에서 해당 TAG_SECTION가 아닌 데이터를 LOOP 돌며 처리하고 있었음)
                /*                

                ////TAG_SECTION 2D BCR Count 추출(TAG_SECTION + MRK_2D_BCD_STR + ADJ_LANE_NO)
                var query2DBcrWithCountInfoToList =
                    (from t in dt.AsEnumerable()
                     where ("TAG_SECTION".Equals(t.Field<string>("EQPT_MEASR_PSTN_ID")) 
                        && t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty() 
                        && t.Field<string>("ADJ_LANE_NO").IsNotEmpty())
                     select new
                     {
                         Mark2DBarCodeStr = t.Field<string>("MRK_2D_BCD_STR"),
                         AdjStrtPstn      = t.Field<decimal>("ADJ_STRT_PSTN"),
                         AdjEndPstn       = t.Field<decimal>("ADJ_END_PSTN")
                     }
                    )
                    .GroupBy(n => new { n.Mark2DBarCodeStr, n.AdjStrtPstn, n.AdjEndPstn })
                    .Select(n => new
                                {
                                    Mark2DBarCode            = n.Key.Mark2DBarCodeStr,
                                    Mark2DBarCodeAdjStrtPstn = n.Key.AdjStrtPstn,
                                    Mark2DBarCodeAdjEndPstn  = n.Key.AdjEndPstn,
                                    Mark2DBarCodeCnt         = n.Count()
                                }).ToList();

                //MRK_2D_BCD_STR 별 시작위치,종료위치,갯수 정보 수집 
                Dictionary<string, List<Tuple<int, decimal, decimal>>> query2DBcrWithCountInfoToDict = new Dictionary<string, List<Tuple<int, decimal, decimal>>>();

                query2DBcrWithCountInfoToList.ForEach(n =>
                {
                    if (query2DBcrWithCountInfoToDict.ContainsKey(n.Mark2DBarCode))
                    {
                        query2DBcrWithCountInfoToDict[n.Mark2DBarCode].Add(new Tuple<int, decimal, decimal>(n.Mark2DBarCodeCnt, n.Mark2DBarCodeAdjStrtPstn, n.Mark2DBarCodeAdjEndPstn));
                    }
                    else
                    {
                        query2DBcrWithCountInfoToDict[n.Mark2DBarCode] = new List<Tuple<int, decimal, decimal>> { new Tuple<int, decimal, decimal>(n.Mark2DBarCodeCnt, n.Mark2DBarCodeAdjStrtPstn, n.Mark2DBarCodeAdjEndPstn) };
                    }

                });

                int laneQty = 0;
                string dfct2DBcrStd = "";

                if (CommonVerify.HasTableRow(dt2DBarcodeInfo))
                {
                    laneQty      = Convert.ToUInt16(dt2DBarcodeInfo.Rows[0]["LANE_QTY"]);
                    dfct2DBcrStd = dt2DBarcodeInfo.Rows[0]["DFCT_2D_BCR_STD"].GetString();
                }

                //// TAG_SECTION 제외하고 MRK_2D_BCD_STR 와 LANE_NO 정보가 있는 정보
                var queryDtBinding = (from t in dtBinding.AsEnumerable()
                                      where !("TAG_SECTION".Equals(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                         && t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty()
                                         && t.Field<string>("LANE_NO").IsNotEmpty())
                                      select t).ToList();

                //foreach (DataRow row in dtBinding.Rows)
                foreach (DataRow row in queryDtBinding)
                {
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -22 : -31;
                    row["TAG"]           = chartConfigurationType == ChartConfigurationType.Input ? null : $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" 
                                                                                                         + ";" 
                                                                                                         + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" 
                                                                                                         + ";" 
                                                                                                         + row["CLCT_SEQNO"].GetString() 
                                                                                                         + ";" 
                                                                                                         + row["ROLLMAP_CLCT_TYPE"].GetString();

                    if ("TAG_SECTION".Equals(row["EQPT_MEASR_PSTN_ID"])
                        && "L002".Equals(dfct2DBcrStd.Trim()) //L001: Normal, L002: NFF, L003: 소형(2170)
                        && !"".Equals(row["MRK_2D_BCD_STR"].ToString().Trim()))
                    {
                        #region [진입불가] queryDtBinding row는 TAG_SECTION을 제외한 정보라서 진입 불가
                        if (query2DBcrWithCountInfoToDict.Any())
                        {
                            string mark2DBarcode = row["MRK_2D_BCD_STR"].ToString();
                            decimal adjStrtPstn  = Convert.ToDecimal(row["ADJ_STRT_PSTN"]);
                            decimal adjEndPstn   = Convert.ToDecimal(row["ADJ_END_PSTN"]);

                            List<Tuple<int, decimal, decimal>> mark2DBarcodeCountInfoList = query2DBcrWithCountInfoToDict[mark2DBarcode];

                            mark2DBarcodeCountInfoList.ForEach(t =>
                            {
                                if (laneQty == t.Item1 && adjStrtPstn == t.Item2 && adjEndPstn == t.Item3)
                                {
                                    //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() 
                                                   + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" 
                                                   + ", " 
                                                   + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                                }
                                else
                                {
                                    //// MRK_2D_BCD_STR이 존재할 경우 그룹핑하여 LANE_QTY와 비교
                                    String laneInfo = row["MRK_2D_BCD_STR"].ToString().Trim().Substring(0, 2);

                                    row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() 
                                                   + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" 
                                                   + ", " 
                                                   + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                                }
                            });
                        }
                        #endregion
                    }
                    else
                    {

                        row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() 
                                       + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" 
                                       + ", " 
                                       + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                    }

                }
                 */
                #endregion

                #region [TO-BE] NFF 추가로직
                //2D 바코드 정보와 Lane수 정보가 있는 경우, 정보수집
                var queryBarcodeInfoList = (from t in dtBinding.AsEnumerable()
                                            where t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty()
                                               && t.Field<string>("ADJ_LANE_NO").IsNotEmpty()
                                            select new
                                            {
                                                MarkBarcodeString = t.Field<string>("MRK_2D_BCD_STR"),
                                                StartPosition = t.GetValue("ADJ_STRT_PSTN").GetDecimal(), // t.Field<decimal>("ADJ_STRT_PSTN"),
                                                EndPosition = t.GetValue("ADJ_END_PSTN").GetDecimal() //t.Field<decimal>("ADJ_END_PSTN")
                                            }).GroupBy(x => new
                                            {
                                                x.MarkBarcodeString,
                                                x.StartPosition,
                                                x.EndPosition,
                                            }).Select(g => new
                                            {
                                                Mark2DBarCode = g.Key.MarkBarcodeString,
                                                Mark2DBarCodeStartPosition = g.Key.StartPosition,
                                                Mark2DBarCodeEndPosition = g.Key.EndPosition,
                                                Mark2DBarCodeCount = g.Count()
                                            }).ToList();

                int laneQty = 0;
                string dfct2DBcrStd = "";

                if (CommonVerify.HasTableRow(dt2DBarcodeInfo))
                {
                    laneQty = Convert.ToUInt16(dt2DBarcodeInfo.Rows[0]["LANE_QTY"]);
                    dfct2DBcrStd = dt2DBarcodeInfo.Rows[0]["DFCT_2D_BCR_STD"].GetString();
                }

                foreach (DataRow row in dtBinding.Rows)//전체 데이터 대상중에서, NFF 2D 바코드 정보가 있는 경우에 추가적인 정보 변경작업
                {
                    row["BORDERTAG"] = row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["TAG_AUTO_FLAG"].GetString();
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -22 : -31;
                    row["TAG"] = $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}"
                               + ";"
                               + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}"
                               + ";"
                               + row["CLCT_SEQNO"].GetString()
                               + ";"
                               + row["ROLLMAP_CLCT_TYPE"].GetString();

                    //L001: Normal, L002: NFF, L003: 소형(2170)
                    if (string.Equals(dfct2DBcrStd, "L002") && !string.IsNullOrEmpty(row["MRK_2D_BCD_STR"].GetString().Trim()))
                    {
                        if (queryBarcodeInfoList.Any())
                        {
                            foreach (var item in queryBarcodeInfoList)
                            {
                                if (laneQty == item.Mark2DBarCodeCount
                                    && row["ADJ_STRT_PSTN"].GetDecimal() == item.Mark2DBarCodeStartPosition
                                    && row["ADJ_END_PSTN"].GetDecimal() == item.Mark2DBarCodeEndPosition)
                                {
                                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString()
                                                   + "["
                                                   + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m"
                                                   + " ~ "
                                                   + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m"
                                                   + ", "
                                                   + row["TAG_AUTO_FLAG_NAME"].GetString()
                                                   + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                                }
                                else
                                {
                                    string laneInfo = row["MRK_2D_BCD_STR"].GetString().Trim().Substring(0, 2);
                                    row["TOOLTIP"] = "(" + laneInfo + ")"
                                                   + row["EQPT_MEASR_PSTN_NAME"].GetString()
                                                   + "["
                                                   + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m"
                                                   + " ~ "
                                                   + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m"
                                                   + ", "
                                                   + row["TAG_AUTO_FLAG_NAME"].GetString()
                                                   + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                                }
                            }
                        }
                    }
                    else
                    {
                        if (row.Table.Columns.Contains("DFCT_NAME"))
                        {
                            row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString()
                                        + "["
                                        + $"{Convert.ToDouble(row["SOURCE_ADJ_END_PSTN"]) - Convert.ToDouble(row["SOURCE_ADJ_STRT_PSTN"]):###,###,###,##0.##}"
                                        + "m"
                                        + ", "
                                        + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}"
                                        + "m"
                                        + " ~ "
                                        + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}"
                                        + "m"
                                        + ", "
                                        + row["TAG_AUTO_FLAG_NAME"].GetString()
                                        + ", "
                                        + row["DFCT_NAME"].GetString()
                                        + "]";
                        }
                        else
                        {
                            // MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                            row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString()
                                           + "["
                                           + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m"
                                           + " ~ "
                                           + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m"
                                           + ", "
                                           + row["TAG_AUTO_FLAG_NAME"].GetString()
                                           + " ]";
                        }

                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                    }

                    /*
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();
                    row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                    */
                }
                #endregion

                #endregion
            }

            dtBinding.AcceptChanges();

            return dtBinding;
        }
        #endregion

        #region DrawDefectTagSection
        /// <summary>
        /// TAG_SECTION NG 마킹 표시(Start(S), End(E) 두개로 표현)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawDefectTagSection(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;
            Binding bd = Equals(chartConfigurationType, ChartConfigurationType.Input) ? new Binding("ADJ_END_PSTN") : new Binding("ADJ_STRT_PSTN");

            for (int x = 0; x < 2; x++)// Start, End 이미지 두번의 표현으로 for문 사용
            {
                DataTable dtTag = MakeTableForDisplay(dt, x == 0 ? ChartDisplayType.TagSectionStart : ChartDisplayType.TagSectionEnd, chartConfigurationType);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTag);

                #region 투입 LOT 차트에 표시되는 TAG_SECTION 항목의 START/END 표시가 반대로 표기되는 부분 확인 필요
                //[2024-04-15]AS-IS
                ds.XValueBinding = (x == 0) ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");
                //[2024-04-15]TO-BE
                //if (Equals(chartConfigurationType, ChartConfigurationType.Input))
                //{
                //    ds.XValueBinding = (x == 0) ? new Binding("ADJ_END_PSTN") : new Binding("ADJ_STRT_PSTN");
                //}
                //else
                //{
                //    ds.XValueBinding = (x == 0) ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");
                //}
                #endregion

                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartSectionTag"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Tag = "TAG_SECTION";

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                c1Chart.Data.Children.Add(ds);
            }
        }
        #endregion

        #region DrawGauge
        /// <summary>
        /// Input, Output 차트 영역에 측정데이터(두께/게이지) 표시 (PET, SCRAP, RW_SCRAP, TAG_SECTION 여기에서 따로 표시함.)
        /// </summary>
        /// <param name="dt">측정데이터</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawGauge(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            //두께 차트 영역에 Gauge 데이터 외의 설비 측정 위치 아이디는 제외하여 표시 함.
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "TAG_SECTION", "SCRAP_SECTION" };

            var queryGauge = dt.AsEnumerable().Where(o => !gaugeExceptions.Contains(o.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList();

            DataTable dtGauge = queryGauge.Any() ? queryGauge.CopyToDataTable() : dt.Clone();

            //차트 툴팁 MIN/MAX 표시 설비 설정 체크. (E20241126-000434)
            if (chartConfigurationType == ChartConfigurationType.Input && CommonVerify.HasTableRow(dtGauge))
            {
                var queryEqptid = dtGauge.AsEnumerable().Where(o => o.Field<string>("EQPTID").IsEmpty() == false).FirstOrDefault();

                if (queryEqptid != null)
                    SetEqptDetailInfoByCommoncode(queryEqptid["EQPTID"].ToString());
            }

            for (int i = 0; i < dtGauge.Rows.Count; i++)
            {
                #region 측정데이터에서 불량관련(PET,SCRAP,RW_SCRAP,TAG_SECTION) 정보 제외한 데이터로 차트에 영역(AlarmZone) 표시
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dtGauge.Rows[i]["COLORMAP"]));

                AlarmZone alarmZone = new AlarmZone();

                #region alarmZone.Near(ADJ_STRT_PSTN)
                if (Util.IsNVC(dtGauge.Rows[i]["ADJ_STRT_PSTN"]))
                {
                    alarmZone.Near = null;
                }
                else
                {
                    alarmZone.Near = dtGauge.Rows[i]["ADJ_STRT_PSTN"].GetDouble();
                }
                #endregion

                #region alarmZone.Far(ADJ_END_PSTN)
                if (Util.IsNVC(dtGauge.Rows[i]["ADJ_END_PSTN"]))
                {
                    alarmZone.Far = null;
                }
                else
                {
                    alarmZone.Far = dtGauge.Rows[i]["ADJ_END_PSTN"].GetDouble();

                }
                #endregion

                #region alarmZone.LowerExtent(CHART_Y_END_CUM_RATE)
                if (Util.IsNVC(dtGauge.Rows[i]["CHART_Y_END_CUM_RATE"]))
                {
                    alarmZone.LowerExtent = null;
                }
                else
                {
                    alarmZone.LowerExtent = dtGauge.Rows[i]["CHART_Y_END_CUM_RATE"].GetDouble();
                }
                #endregion

                #region alarmZone.UpperExtent(CHART_Y_STRT_CUM_RATE)
                if (Util.IsNVC(dtGauge.Rows[i]["CHART_Y_STRT_CUM_RATE"]))
                {
                    alarmZone.UpperExtent = null;
                }
                else
                {
                    alarmZone.UpperExtent = dtGauge.Rows[i]["CHART_Y_STRT_CUM_RATE"].GetDouble();
                }
                #endregion

                alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;

                int x = i;

                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    if (pe is Lines)
                    {
                        //dtGauge.Rows[x]["SOURCE_ADJ_STRT_PSTN"] -> dtGauge.Rows[x]["ADJ_STRT_PSTN"] 으로 변경
                        //dtGauge.Rows[x]["SOURCE_ADJ_END_PSTN"] -> dtGauge.Rows[x]["ADJ_END_PSTN"] 으로 변경
                        double sourceStartPosition;
                        double sourceEndPosition;
                        double.TryParse(Util.NVC(dtGauge.Rows[x]["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);
                        double.TryParse(Util.NVC(dtGauge.Rows[x]["SOURCE_ADJ_END_PSTN"]), out sourceEndPosition);

                        //투입랏에 대한 차트 툴팁 MIN/MAX 표시 설비에 대해 수집된 MIN/MAX 값이 있을 경우 툴팁에 추가한다. (E20241126-000434)
                        string content = string.Empty;

                        if (_isMinMaxEqpt && chartConfigurationType == ChartConfigurationType.Input)
                        {
                            string scanMinValueContent = string.Empty;
                            string scaneMaxValueContent = string.Empty;

                            if (Util.NVC(dtGauge.Rows[x]["SCAN_MIN_VALUE"]).IsNullOrEmpty() == false && dtGauge.Rows[x]["SCAN_MIN_VALUE"].GetDouble() > 0)
                            {
                                scanMinValueContent = "Scan Min : " + Util.NVC($"{dtGauge.Rows[x]["SCAN_MIN_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine;
                            }
                            if (Util.NVC(dtGauge.Rows[x]["SCAN_MAX_VALUE"]).IsNullOrEmpty() == false && dtGauge.Rows[x]["SCAN_MAX_VALUE"].GetDouble() > 0)
                            {
                                scaneMaxValueContent = "Scan Max : " + Util.NVC($"{dtGauge.Rows[x]["SCAN_MAX_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine;
                            }

                            content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine
                                         + "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine
                                         + scanMinValueContent + scaneMaxValueContent
                                         + "ColorMap : " + Util.NVC(dtGauge.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine
                                         + "Offset : " + Util.NVC(dtGauge.Rows[x]["SCAN_OFFSET"]);
                        }
                        else
                        {
                            content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine
                                         + "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine
                                         + "ColorMap : " + Util.NVC(dtGauge.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine
                                         + "Offset : " + Util.NVC(dtGauge.Rows[x]["SCAN_OFFSET"]);
                        }

                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };


                chartOutput.Data.Children.Add(alarmZone);
                #endregion
            }

            #region SCRAP_SECTION(DrawScrapSectionOutlot)
            //조성근 2025.04.04 ScrapSection 적용 - start
            var queryScrapSetion = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("SCRAP_SECTION")).ToList();
            DataTable dtScrapSection = queryScrapSetion.Any() ? queryScrapSetion.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtScrapSection))
            {
                dtScrapSection.TableName = "SCRAP_SECTION";
                DrawScrapSectionOutlot(dtScrapSection, chartConfigurationType);
            }
            //조성근 2025.04.04 ScrapSection 적용 - end
            #endregion

            #region PET(DrawPetScrap)
            var queryPet = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("PET")).ToList();
            DataTable dtPet = queryPet.Any() ? queryPet.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtPet))
            {
                dtPet.TableName = "PET";
                DrawPetScrap(dtPet, chartConfigurationType);
            }
            #endregion

            #region SCRAP(DrawPetScrap)
            var queryScrap = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("SCRAP")).ToList();
            DataTable dtScrap = queryScrap.Any() ? queryScrap.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtScrap))
            {
                dtScrap.TableName = "SCRAP";
                DrawPetScrap(dtScrap, chartConfigurationType);
            }
            #endregion

            #region RW_SCRAP(DrawRewinderScrap) : RW 이후 수동 SCRAP
            var queryRewinderScrap = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("RW_SCRAP")).ToList();
            DataTable dtRwScrap = queryRewinderScrap.Any() ? queryRewinderScrap.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtRwScrap))
            {
                DrawRewinderScrap(dtRwScrap, chartConfigurationType);
            }
            #endregion

            #region TAG_SECTION(DrawDefectTagSection)
            var queryTagSection = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
            DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtTagSection))
            {
                DrawDefectTagSection(dtTagSection, chartConfigurationType);
            }
            #endregion

            #region TAG_SECTION_SINGLE(DrawDefectTagSection)
            var queryTagSectionSingle = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
            DataTable dtTagSectionSingle = queryTagSectionSingle.Any() ? queryTagSectionSingle.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtTagSectionSingle))
            {
                DrawDefectTagSectionSingle(dtTagSectionSingle);
            }
            #endregion     
        }
        #endregion

        #region DrawDefectVisionSurface
        /// <summary>
        /// TOP(VISION_SURF_NG_TOP), BACK(VISION_SURF_NG_BACK) 비전표면불량 표시
        /// </summary>
        /// <param name="dt">불량정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectVisionSurface(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            XYDataSeries ds = new XYDataSeries();
            ds.Tag = dt.TableName;
            ds.ItemsSource = DataTableConverter.Convert(dt);
            ds.ValueBinding = new Binding("Y_PSTN_RATE");
            ds.XValueBinding = new Binding("ADJ_X_PSTN");
            ds.Cursor = Cursors.Hand;
            ds.SymbolSize = new Size(7, 7);

            ds.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;

                if (pe is DotSymbol)
                {
                    DataPoint dp = pe.DataPoint;

                    var convertFromString = ColorConverter.ConvertFromString(DataTableConverter.GetValue(dp.DataObject, "COLORMAP").GetString());

                    pe.Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    pe.Stroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    double xposition;
                    double yposition;
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_X_PSTN")), out xposition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_Y_PSTN")), out yposition);
                    string defectName = DataTableConverter.GetValue(dp.DataObject, "ABBR_NAME").GetString();

                    string content = defectName
                                   + " [" + ObjectDic.Instance.GetObjectName("길이") + ":" + $"{xposition:###,###,###,##0.##}" + "m"
                                   + ", " + ObjectDic.Instance.GetObjectName("폭") + ":" + $"{yposition:###,###,###,##0.##}" + "mm" + "]";
                    ToolTipService.SetToolTip(pe, content);
                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                    ToolTipService.SetShowDuration(pe, 60000);
                }
            };
            c1Chart.Data.Children.Add(ds);
        }

        #endregion

        #region DrawDefectAlignVision
        /// <summary>
        /// TOP(VISION_NG_TOP/INS_ALIGN_VISION_NG_TOP),BACK(VISION_NG_BACK/INS_ALIGN_VISION_NG_BACK) 비전 불량, 절연코팅 Align 비전 불량 영역(AlarmZone)표시
        /// </summary>
        /// <param name="dt">불량정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectAlignVision(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            foreach (DataRow row in dt.Rows)
            {
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                AlarmZone alarmZone = new AlarmZone
                {
                    Near = row["ADJ_STRT_PSTN"].GetDouble(),
                    Far = row["ADJ_END_PSTN"].GetDouble(),
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["CHART_Y_STRT_CUM_RATE"].GetDouble(),
                    UpperExtent = row["CHART_Y_END_CUM_RATE"].GetDouble(),
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    Cursor = Cursors.Hand,
                    //Name = dt.TableName
                    Tag = dt.TableName
                };

                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    if (pe is Lines)
                    {
                        double startPosition;
                        double endPosition;
                        double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out startPosition);
                        double.TryParse(Util.NVC(row["SOURCE_ADJ_END_PSTN"]), out endPosition);

                        string content = row["ABBR_NAME"]
                                       + "[" + $"{startPosition:###,###,###,##0.##}" + "m" + "~" + $"{endPosition:###,###,###,##0.##}" + "m" + "]";

                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };
                c1Chart.Data.Children.Add(alarmZone);
            }
        }
        #endregion

        #region DrawDefectOverLayVision
        /// <summary>
        /// 절연 코팅 비전 불량(INS_OVERLAY_VISION_NG) 영역(AlarmZone) 표시
        /// </summary>
        /// <param name="dt">불량정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectOverLayVision(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            foreach (DataRow row in dt.Rows)
            {
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                AlarmZone alarmZone = new AlarmZone
                {
                    Near = row["ADJ_STRT_PSTN"].GetDouble(),
                    Far = row["ADJ_END_PSTN"].GetDouble(),
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["CHART_Y_STRT_CUM_RATE"].GetDouble(),
                    UpperExtent = row["CHART_Y_END_CUM_RATE"].GetDouble(),
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    Cursor = Cursors.Hand,
                    Tag = dt.TableName
                    //Name = dt.TableName
                };

                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;

                    if (pe is Lines)
                    {
                        double startPosition;
                        double endPosition;
                        double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out startPosition);
                        double.TryParse(Util.NVC(row["SOURCE_ADJ_END_PSTN"]), out endPosition);

                        string content = row["ABBR_NAME"]
                                       + "[" + $"{startPosition:###,###,###,##0.##}" + "m" + "~" + $"{endPosition:###,###,###,##0.##}" + "m" + "]";

                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                c1Chart.Data.Children.Add(alarmZone);
            }
        }
        #endregion

        #region DrawDefectTagSpot
        /// <summary>
        /// INPUT, OUTPUT 차트에 TAG_SPOT 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectTagSpot(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                dt = MakeTableForDisplay(dt, ChartDisplayType.TagSpot, chartConfigurationType);

                C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dt);
                ds.XValueBinding = new Binding("ADJ_STRT_PSTN");
                ds.ValueBinding = new Binding("SOURCE_ADJ_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Tag = dt.TableName;
                //ds.Name = dt.TableName;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                c1Chart.Data.Children.Add(ds);
            }
        }
        #endregion

        #region DrawDefectMark
        /// <summary>
        /// MARK(기준점) 라인 및 MARK 번호 표시 (불량정보 아님)
        /// </summary>
        /// <param name="dt">불량정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectMark(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = string.Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            // 종축 라인 설정
            DataRow[] drMark = dt.Select();

            if (drMark.Length > 0)
            {
                foreach (DataRow row in drMark)
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                    //adjustment
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    string content = row["EQPT_MEASR_PSTN_NAME"]
                                   + "[" + Util.NVC(row["ROLLMAP_CLCT_TYPE"]) + "],"
                                   + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_STRT_PSTN"].GetDouble(), row["ADJ_STRT_PSTN"].GetDouble() },
                        ValuesSource = new double[] { row["CHART_Y_STRT_CUM_RATE"].GetDouble(), row["CHART_Y_END_CUM_RATE"].GetDouble() },
                        ConnectionStroke = new SolidColorBrush(Colors.Black),
                        ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                        ToolTip = content,
                        Tag = dt.TableName
                        //Name = dt.TableName
                    });
                }
            }

            //Mark 라벨 설정(마스터 기준점에 대하여 라벨 표시)
            var queryLabel = dt.AsEnumerable().Where(x => x.GetValue("CHART_Y_STRT_CUM_RATE") != null
                                                       && x.GetValue("CHART_Y_END_CUM_RATE") != null
                                                       && Math.Abs(x.GetValue("CHART_Y_STRT_CUM_RATE").GetDecimal() - x.GetValue("CHART_Y_END_CUM_RATE").GetDecimal()) >= 100).ToList();

            DataTable dtLabel = queryLabel.Any() ? MakeTableForDisplay(queryLabel.CopyToDataTable(), ChartDisplayType.MarkLabel, chartConfigurationType)
                                                 : MakeTableForDisplay(dt.Clone(), ChartDisplayType.MarkLabel, chartConfigurationType);

            var dsMarkLabel = new XYDataSeries();
            dsMarkLabel.Name = dt.TableName;
            dsMarkLabel.ItemsSource = DataTableConverter.Convert(dtLabel);
            dsMarkLabel.XValueBinding = new Binding("ADJ_STRT_PSTN");
            dsMarkLabel.ValueBinding = new Binding("SOURCE_ADJ_Y_PSTN");
            dsMarkLabel.ChartType = ChartType.XYPlot;
            dsMarkLabel.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsMarkLabel.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsMarkLabel.PointLabelTemplate = grdMain.Resources["chartLabel"] as DataTemplate;
            //dsMarkLabel.PointLabelTemplate = grdMain.Resources["chartMarkLabel"] as DataTemplate;

            dsMarkLabel.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };
            c1Chart.Data.Children.Add(dsMarkLabel);
        }
        #endregion

        #region DrawDefectRewinderScrap
        /// <summary>
        /// RW_SCRAP, RW 이후 수동 SCRAP 종료위치에 세로 라인과 가위 표시
        /// </summary>
        /// <param name="dtRwScrap">불량정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectRewinderScrap(DataTable dtRwScrap, ChartConfigurationType chartConfigurationType)
        {
            if (CommonVerify.HasTableRow(dtRwScrap))
            {
                C1Chart c1Chart = string.Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

                dtRwScrap.Columns.Add("TOOLTIP", typeof(string));
                dtRwScrap.Columns.Add("TAG", typeof(string));
                dtRwScrap.Columns.Add("Y_PSTN", typeof(double));

                double lowerExtent = 0;
                double upperExtent = 100;

                foreach (DataRow row in dtRwScrap.Rows)
                {
                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));

                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString()
                                   + ";" + row["SOURCE_ADJ_STRT_PSTN"].GetString()
                                   + ";" + row["SOURCE_ADJ_END_PSTN"].GetString()
                                   + ";" + row["ADJ_LOTID"].GetString()
                                   + ";" + row["ADJ_WIPSEQ"].GetString()
                                   + ";" + row["CLCT_SEQNO"].GetString()
                                   + ";" + row["ROLLMAP_CLCT_TYPE"].GetString()
                                   + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";

                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString()
                                   + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + " ]";

                    row["TOOLTIP"] = toolTip;
                    row["TAG"] = tag;
                    row["Y_PSTN"] = 100;

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["SOURCE_ADJ_END_PSTN"], row["SOURCE_ADJ_END_PSTN"] },
                        ValuesSource = new double[] { lowerExtent, upperExtent },
                        ConnectionStroke = convertFromString,
                        ConnectionStrokeThickness = 3,
                        ConnectionFill = convertFromString,
                        Tag = dtRwScrap.TableName
                        //Name = dtRwScrap.TableName
                    });
                }

                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dtRwScrap);
                dsPetScrap.XValueBinding = new Binding("SOURCE_ADJ_END_PSTN");
                dsPetScrap.ValueBinding = new Binding("Y_PSTN");
                dsPetScrap.ChartType = ChartType.XYPlot;
                dsPetScrap.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.SymbolFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.PointLabelTemplate = grdMain.Resources["chartPetScrap"] as DataTemplate;
                dsPetScrap.Margin = new Thickness(0, 0, 0, 0);
                dsPetScrap.Tag = dtRwScrap.TableName;
                //dsPetScrap.Name = dtRwScrap.TableName;

                dsPetScrap.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };

                c1Chart.Data.Children.Add(dsPetScrap);
            }
        }
        #endregion

        #region DrawDefect
        /// <summary>
        /// Input, Output 차트의 불량정보 표시(불량 항목별 메소드를 별도 호출하여 불량정보 표시)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType">Input/Output</param>
        /// <param name="isReDraw"></param>
        private void DrawDefect(DataTable dt, ChartConfigurationType chartConfigurationType, bool isReDraw = false)
        {
            #region 1.VISION_SURF_NG_TOP / VISION_SURF_NG_BACK => DrawDefectVisionSurface
            var queryTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")).ToList();
            DataTable dtTop = queryTop.Any() ? queryTop.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtTop))
            {
                dtTop.TableName = "VISION_SURF_NG_TOP";
                DrawDefectVisionSurface(dtTop, chartConfigurationType);
            }

            var queryBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")).ToList();
            DataTable dtBack = queryBack.Any() ? queryBack.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtBack))
            {
                dtBack.TableName = "VISION_SURF_NG_BACK";
                DrawDefectVisionSurface(dtBack, chartConfigurationType);
            }
            #endregion

            #region 주석처리
            //// TableName 은 chart DataSeries의 Tag 설정하여 범례 텍스트 클릭 시 선택영역 차트의 DataSeries Remove 역할을 하기 위함.
            //if (CommonVerify.HasTableRow(dtTop))
            //{
            //    dtTop.TableName = dtTop.Rows[0]["ABBR_NAME"].GetString();
            //    DrawDefectVisionSurface(dtTop, chartConfigurationType);
            //}
            #endregion

            #region 2.VISION_NG_TOP / VISION_NG_BACK / INS_ALIGN_VISION_NG_TOP / INS_ALIGN_VISION_NG_BACK => DrawDefectAlignVision
            var queryVisionTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")).ToList();
            DataTable dtVisionTop = queryVisionTop.Any() ? queryVisionTop.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtVisionTop))
            {
                dtVisionTop.TableName = "VISION_NG_TOP";
                DrawDefectAlignVision(dtVisionTop, chartConfigurationType);
            }

            var queryVisionBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")).ToList();
            DataTable dtVisionBack = queryVisionBack.Any() ? queryVisionBack.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtVisionBack))
            {
                dtVisionBack.TableName = "VISION_NG_BACK";
                DrawDefectAlignVision(dtVisionBack, chartConfigurationType);
            }

            var queryAlignVisionTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")).ToList();
            DataTable dtAlignVisionTop = queryAlignVisionTop.Any() ? queryAlignVisionTop.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtAlignVisionTop))
            {
                dtAlignVisionTop.TableName = "INS_ALIGN_VISION_NG_TOP";
                DrawDefectAlignVision(dtAlignVisionTop, chartConfigurationType);
            }

            var queryAlignVisionBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")).ToList();
            DataTable dtAlignVisionBack = queryAlignVisionBack.Any() ? queryAlignVisionBack.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtAlignVisionBack))
            {
                dtAlignVisionBack.TableName = "INS_ALIGN_VISION_NG_BACK";
                DrawDefectAlignVision(dtAlignVisionBack, chartConfigurationType);
            }
            #endregion

            #region 3.INS_OVERLAY_VISION_NG => DrawDefectOverLayVision
            var queryOverLayVision = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
            DataTable dtOverLayVision = queryOverLayVision.Any() ? queryOverLayVision.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtOverLayVision))
            {
                dtOverLayVision.TableName = "INS_OVERLAY_VISION_NG";
                DrawDefectOverLayVision(dtOverLayVision, chartConfigurationType);
            }
            #endregion

            if (!isReDraw)
            {
                #region 4.TAG_SECTION => DrawDefectTagSection
                var queryTagSection = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dt.Clone();

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    dtTagSection.TableName = "TAG_SECTION";
                    DrawDefectTagSection(dtTagSection, chartConfigurationType);
                }
                #endregion
                #region TAG_SECTION_SINGLE(DrawDefectTagSection)
                var queryTagSectionSingle = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSectionSingle = queryTagSectionSingle.Any() ? queryTagSectionSingle.CopyToDataTable() : dt.Clone();

                if (CommonVerify.HasTableRow(dtTagSectionSingle))
                {
                    DrawDefectTagSectionSingle(dtTagSectionSingle);
                }
                #endregion

                #region 5.TAG_SPOT => DrawDefectTagSpot
                var queryTagSpot = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dt.Clone();
                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    dtTagSpot.TableName = "TAG_SPOT";
                    DrawDefectTagSpot(dtTagSpot, chartConfigurationType);
                }
                #endregion

                #region 6.MARK => DrawDefectMark
                var queryMark = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("MARK")).ToList();
                DataTable dtMark = queryMark.Any() ? queryMark.CopyToDataTable() : dt.Clone();
                if (CommonVerify.HasTableRow(dtMark))
                {
                    dtMark.TableName = "MARK";
                    DrawDefectMark(dtMark, chartConfigurationType);
                }
                #endregion

                #region 7.RW_SCRAP => DrawDefectRewinderScrap
                var queryRewinderScrap = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("RW_SCRAP")).ToList();
                DataTable dtRwScrap = queryRewinderScrap.Any() ? queryRewinderScrap.CopyToDataTable() : dt.Clone();
                if (CommonVerify.HasTableRow(dtRwScrap))
                {
                    dtRwScrap.TableName = "RW_SCRAP";
                    DrawDefectRewinderScrap(dtRwScrap, chartConfigurationType);
                }
                #endregion
            }
        }
        #endregion

        #region DrawDefectLegendText
        /// <summary>
        /// 불량 범례 표시 및 DescriptionOnPreviewMouseUp 이벤트 생성
        /// </summary>
        /// <param name="dt">불량정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectLegendText(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            try
            {
                if (CommonVerify.HasTableRow(dt))
                {
                    Grid gridLegend = grdOutputDefectLegend;

                    if (gridLegend.ColumnDefinitions.Count > 0) gridLegend.ColumnDefinitions.Clear();
                    if (gridLegend.RowDefinitions.Count > 0) gridLegend.RowDefinitions.Clear();

                    for (int x = 0; x < 2; x++)
                    {
                        ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                        gridLegend.ColumnDefinitions.Add(gridColumn);
                    }
                    gridLegend.Children.Clear();

                    #region 주석처리
                    /*
                    // Top, Back 구분 없는 범례 표시를 위한 Group 처리
                    var queryRow = dt.AsEnumerable().GroupBy(g => new
                    {
                        DefectName = g.Field<string>("ABBR_NAME"),
                        ColorMap = g.Field<string>("COLORMAP"),
                        DefectShape = g.Field<string>("DEFECT_SHAPE")
                        //MeasurementCode = g.Field<string>("EQPT_MEASR_PSTN_ID")
                    }).Select(x => new
                    {
                        DefectName = x.Key.DefectName,
                        ColorMap = x.Key.ColorMap,
                        DefectShape = x.Key.DefectShape,
                        MeasurementCode = string.Empty
                    }).ToList();
                    */
                    #endregion

                    #region 불량정보에서 불량명/색상/모양(사각형,원형) 정보 갯수만큼, 모양과 불량정보를 행단위로 표시
                    // Top, Back 구분 없는 범례 표시를 위한 Group 처리
                    var queryRow = from row in dt.AsEnumerable()
                                   group row by new
                                   {
                                       DefectName = row.Field<string>("ABBR_NAME"),
                                       ColorMap = row.Field<string>("COLORMAP"),
                                       DefectShape = row.Field<string>("DEFECT_SHAPE"),
                                   } into grp
                                   select new
                                   {
                                       DefectName = grp.Key.DefectName,
                                       ColorMap = grp.Key.ColorMap,
                                       DefectShape = grp.Key.DefectShape,
                                       DefectCount = grp.Count(),
                                       MeasurementCode = string.Empty
                                   };


                    int y = 0;

                    if (queryRow.Any())
                    {
                        foreach (var item in queryRow)
                        {
                            //불량별로 행추가해서 모양과 불량정보를 추가한다.
                            RowDefinition gridRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };
                            gridLegend.RowDefinitions.Add(gridRow);

                            StackPanel stackPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(2, 2, 2, 2)
                            };

                            var convertFromString = ColorConverter.ConvertFromString(Util.NVC(item.ColorMap));

                            switch (item.DefectShape)
                            {
                                case "RECT":
                                    Rectangle rectangleLegend = Util.CreateRectangle(HorizontalAlignment.Center,
                                                                                     VerticalAlignment.Center,
                                                                                     12,
                                                                                     12,
                                                                                     convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                                     null,
                                                                                     1,
                                                                                     new Thickness(2, 2, 2, 2),
                                                                                     null,
                                                                                     null);

                                    TextBlock rectangleDescription = Util.CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                                          HorizontalAlignment.Center,
                                                                                          VerticalAlignment.Center,
                                                                                          11,
                                                                                          FontWeights.Bold,
                                                                                          Brushes.Black,
                                                                                          new Thickness(1, 1, 1, 1),
                                                                                          new Thickness(0, 0, 0, 0),
                                                                                          item.MeasurementCode,
                                                                                          Cursors.Hand,
                                                                                          chartConfigurationType.ToString() + "|" + item.ColorMap);
                                    stackPanel.Children.Add(rectangleLegend);
                                    stackPanel.Children.Add(rectangleDescription);

                                    rectangleDescription.PreviewMouseUp += DescriptionOnPreviewMouseUp;
                                    break;

                                case "ELLIPSE":
                                    Ellipse ellipseLegend = Util.CreateEllipse(HorizontalAlignment.Center,
                                                                               VerticalAlignment.Center,
                                                                               12,
                                                                               12,
                                                                               convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                               1,
                                                                               new Thickness(2, 2, 2, 2),
                                                                               null,
                                                                               null);

                                    TextBlock ellipseDescription = Util.CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                                        HorizontalAlignment.Center,
                                                                                        VerticalAlignment.Center,
                                                                                        11,
                                                                                        FontWeights.Bold,
                                                                                        Brushes.Black,
                                                                                        new Thickness(1, 1, 1, 1),
                                                                                        new Thickness(0, 0, 0, 0),
                                                                                        item.MeasurementCode,
                                                                                        Cursors.Hand,
                                                                                        chartConfigurationType.ToString() + "|" + item.ColorMap);
                                    stackPanel.Children.Add(ellipseLegend);
                                    stackPanel.Children.Add(ellipseDescription);

                                    ellipseDescription.PreviewMouseUp += DescriptionOnPreviewMouseUp;
                                    break;
                            }
                            Grid.SetColumn(stackPanel, 0);
                            Grid.SetRow(stackPanel, y);
                            gridLegend.Children.Add(stackPanel);

                            y++;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region DrawDefectLegend
        /// <summary>
        /// Input, Output 차트 영역의 불량 정보의 범례를 표시
        /// </summary>
        /// <param name="dt">불량정보</param>
        /// <param name="chartConfigurationType">Input/Output</param>
        private void DrawDefectLegend(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            if (!CommonVerify.HasTableRow(dt)) return;

            var queryDefectLegend = dt.AsEnumerable().Where(o =>
                   o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();

            DataTable dtDefectLegend = queryDefectLegend.Any() ? queryDefectLegend.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtDefectLegend))
            {
                dtDefectLegend.TableName = Equals(chartConfigurationType, ChartConfigurationType.Input) ? "DefectLegendInput" : "DefectLegendOutput";
                DrawDefectLegendText(dtDefectLegend, chartConfigurationType);
            }

        }
        #endregion

        #region DrawThicknessLegend
        /// <summary>
        /// OUTLOT 측정데이터 라인정보를 범례에 표시한다.
        /// </summary>
        /// <param name="dt">OUTLOT 축정데이터</param>
        /// <param name="dtLegend">로딩량/두께 스펙컬러</param>
        private void DrawThicknessLegend(DataTable dt, DataTable dtLegend)
        {
            try
            {
                //if (grdLineLegend.ColumnDefinitions.Count > 0) grdLineLegend.ColumnDefinitions.Clear();
                //if (grdLineLegend.RowDefinitions.Count > 0) grdLineLegend.RowDefinitions.Clear();

                //grdLineLegend.Children.Clear();

                DataTable copyTable = dtLegend.Clone();
                copyTable.Columns.Add(new DataColumn() { ColumnName = "VALUE_AVG", DataType = typeof(double) });

                foreach (DataRow row in dtLegend.Select())
                {
                    string strVal = row["VALUE"].GetString();
                    string valueText = "VALUE_" + strVal;

                    switch (strVal)
                    {
                        case "LL":
                        case "L":
                        case "SV":
                        case "H":
                        case "HH":
                            {
                                if (dt.AsEnumerable().Any(o => o.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(o.GetValue(valueText).ToString())))
                                {
                                    //TODO 두께 측정중에서 데이터 가져와야 하는데....
                                    var query = (from t in dt.AsEnumerable()
                                                 where t.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(t.GetValue(valueText).ToString())
                                                 select new
                                                 {
                                                     Valuecol = t.GetValue(valueText).GetDecimal() //t.Field<decimal>(valueText)
                                                 }).ToList();

                                    double agvValue = 0;

                                    if (query.Any())
                                    {
                                        agvValue = query.Max(r => r.Valuecol).GetDouble();//실제는 스펙 최대값
                                    }

                                    DataRow dr = copyTable.NewRow();
                                    dr["COLORMAP"] = row["COLORMAP"];
                                    dr["VALUE"] = row["VALUE"];
                                    dr["VALUE_AVG"] = agvValue;
                                    copyTable.Rows.Add(dr);
                                }
                                break;
                            }
                    }
                }

                for (int i = 0; i < copyTable.Rows.Count; i++)
                {
                    #region 스펙 갯수만큼 행으로 TextBlock으로 정보 표시
                    RowDefinition gridTopRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };
                    //grdLineLegend.RowDefinitions.Add(gridTopRow);

                    StackPanel stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(2, 2, 2, 2),
                    };

                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(copyTable.Rows[i]["COLORMAP"]));

                    string rowValue = copyTable.Rows[i]["VALUE"] + " : " + copyTable.Rows[i]["VALUE_AVG"].GetInt();

                    TextBlock lineLegendDescription = Util.CreateTextBlock(rowValue,
                                                                           HorizontalAlignment.Center,
                                                                           VerticalAlignment.Center,
                                                                           11,
                                                                           FontWeights.Bold,
                                                                           convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                           new Thickness(1, 1, 1, 1),
                                                                           new Thickness(0, 0, 0, 0),
                                                                           null,
                                                                           null,
                                                                           null);


                    stackPanel.Children.Add(lineLegendDescription);

                    Grid.SetRow(stackPanel, i);
                    Grid.SetColumn(stackPanel, 0);
                    //grdLineLegend.Children.Add(stackPanel);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region GetLineValuesSource
        /// <summary>
        /// OUTLOT 길이만큼 해당 스펙 최대값을 반환한다.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="value">로딩량/두께 스팩정보(LL/L/SV/H/HH)</param>
        /// <returns></returns>
        private System.Collections.Generic.IEnumerable<double> GetLineValuesSource(DataTable dt, string value)
        {
            try
            {
                var queryCount = xOutputMaxLength.GetInt() + 1;

                double[] xx = new double[queryCount];
                string valueText = "VALUE_" + value;

                if (dt.AsEnumerable().Any(p => p.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(p.GetValue(valueText).ToString())))
                {
                    double agvValue = 0;

                    var query = (from t in dt.AsEnumerable()
                                 where t.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(t.GetValue(valueText).ToString())
                                 select new
                                 {
                                     Valuecol = t.GetValue(valueText).GetDecimal() //t.Field<decimal>(valueText)
                                 }).ToList();

                    if (query.Any())
                    {
                        agvValue = query.Max(r => r.Valuecol).GetDouble();//해당 스펙 최대값
                    }

                    for (int j = 0; j < queryCount; j++)
                    {
                        xx[j] = agvValue;
                    }
                }

                return xx;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region DrawThickness
        /// <summary>
        /// chartThickness 차트(라인)의 두께 측정데이터 표시
        /// </summary>
        /// <param name="dt">OUTLOT 측정데이터</param>
        /// <param name="chartConfigurationType">Input(x)/Output(o)</param>
        private void DrawThickness(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "TAG_SECTION", "COT_WIDTH_LEN_BACK" };

            var queryGauge = dt.AsEnumerable().Where(o => !gaugeExceptions.Contains(o.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList();
            dt = queryGauge.Any() ? queryGauge.CopyToDataTable() : null;

            if (!CommonVerify.HasTableRow(dt)) return;

            chartThickness.View.AxisX.Min = 0;
            chartThickness.View.AxisX.Max = xOutputMaxLength;
            chartThickness.ChartType = ChartType.Line;

            #region ADJ_AVG_VALUE(두께 평균) 없으면 수동으로 추가
            if (!dt.Columns.Contains("ADJ_AVG_VALUE"))
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "ADJ_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
            }

            foreach (DataRow row in dt.Rows)
            {
                row["ADJ_AVG_VALUE"] = (row["ADJ_STRT_PSTN"].GetDouble() + row["ADJ_END_PSTN"].GetDouble()) / 2;
            }
            dt.AcceptChanges();
            #endregion

            if (CommonVerify.HasTableRow(dt))
            {
                DrawThicknessLegend(dt.Copy(), dtLineLegend.Copy());

                DataTable dtLegend = dtLineLegend.Select().AsEnumerable().OrderByDescending(o => o.GetValue("NO").GetInt()).CopyToDataTable();
                //DataRow[] newRows  = dtLegend.Select();

                #region 1.로딩량/두께 차트 스팩(LL/L/SV/H/HH)별 라인 그리기
                foreach (DataRow row in dtLegend.Select())
                {

                    string strVal = row["VALUE"].GetString();
                    string valueText = "VALUE_" + strVal;

                    switch (strVal)
                    {
                        case "LL":
                        case "L":
                        case "SV":
                        case "H":
                        case "HH":
                            {
                                if (dt.AsEnumerable().All(p => p.GetValue(valueText) == DBNull.Value || string.IsNullOrEmpty(p.GetValue(valueText).ToString()))) continue;

                                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                                DataSeries dsLegend = new DataSeries();
                                dsLegend.ItemsSource = new[] { "LL", "L", "SV", "H", "HH" };
                                dsLegend.ChartType = ChartType.Line;
                                dsLegend.ValuesSource = GetLineValuesSource(dt, row["VALUE"].GetString());
                                dsLegend.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                                //dsLegend.ConnectionStrokeDashes = new DoubleCollection { 3, 2 };

                                if (row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "L")
                                {
                                    dsLegend.ConnectionStrokeDashes = new DoubleCollection { 3, 4 };
                                    dsLegend.ConnectionStrokeThickness = 0.8;
                                }
                                else
                                {
                                    dsLegend.ConnectionStrokeThickness = 1.0;
                                }

                                chartThickness.Data.Children.Add(dsLegend);
                                break;
                            }
                    }
                }
                #endregion

                #region 2.레인별로 로딩량/두께 측정데이터를 점선차트로 표시한다.
                var queryLaneInfo = (from t in dtLane.AsEnumerable()
                                     join x in dtLaneLegend.AsEnumerable()
                                       on t.Field<string>("LANE_NO") equals x.Field<string>("LANE_NO")
                                     select new
                                     {
                                         LaneNo = t.Field<string>("LANE_NO"),
                                         LaneColor = x.Field<string>("COLORMAP")
                                     }
                    ).ToList();

                if (queryLaneInfo.Any())
                {
                    foreach (var item in queryLaneInfo)
                    {
                        if (!CommonVerify.HasTableRow(dtLane)) continue;

                        var queryLane = dtLane.AsEnumerable().Where(where => @where.Field<string>("LANE_NO").Equals(item.LaneNo)).ToList();

                        if (!queryLane.Any()) continue;

                        var query = dt.Select("ADJ_LANE_NO = '" + item.LaneNo + "'").AsEnumerable().ToList();
                        DataTable dtLine = query.Any() ? query.CopyToDataTable() : dt.Clone();

                        var laneConvertFromString = ColorConverter.ConvertFromString(Util.NVC(item.LaneColor));

                        XYDataSeries ds = new XYDataSeries();
                        ds.ItemsSource = DataTableConverter.Convert(dtLine);
                        ds.XValueBinding = new Binding("ADJ_AVG_VALUE");
                        ds.ValueBinding = new Binding("SCAN_AVG_VALUE");
                        ds.ChartType = ChartType.LineSymbols;
                        ds.ConnectionFill = new SolidColorBrush(Colors.Black);
                        ds.SymbolFill = laneConvertFromString != null ? new SolidColorBrush((Color)laneConvertFromString) : null;
                        ds.Cursor = Cursors.Hand;
                        ds.SymbolSize = new Size(6, 6);
                        ds.ConnectionStrokeThickness = 0.8;

                        chartThickness.Data.Children.Add(ds);

                        ds.PlotElementLoaded += (s, e) =>
                        {
                            PlotElement pe = (PlotElement)s;
                            if (pe is DotSymbol)
                            {
                                DataPoint dp = pe.DataPoint;

                                double scanAvgValue;
                                double sourceStart;
                                double sourceEnd;

                                double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_SCAN_AVG_VALUE")), out scanAvgValue);
                                double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_STRT_PSTN")), out sourceStart);
                                double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_END_PSTN")), out sourceEnd);

                                //string content = rdoThick.IsChecked != null && (bool)rdoThick.IsChecked ? "Thickness : " : "Load : ";
                                string content = "Thickness : ";
                                content = content + $"{scanAvgValue:###,###,###,##0.##}" + "[" + $"{sourceStart:###,###,###,##0.##}" + "~" + $"{sourceEnd:###,###,###,##0.##}" + "]";

                                ToolTipService.SetToolTip(pe, content);
                                ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                ToolTipService.SetShowDuration(pe, 60000);
                            }
                        };
                    }
                }
                #endregion

                //TODO 왜 다 그린 후 초기화하는 건지??
                foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdThickness))
                {
                    InitializeChartView(c1Chart);
                }
            }
        }
        #endregion

        #region DrawLaneLegend
        /// <summary>
        /// chartThickness 차트의 Lane 범례 표시
        /// </summary>
        private void DrawLaneLegend()
        {
            if (grdLaneLegend.ColumnDefinitions.Count > 0) grdLaneLegend.ColumnDefinitions.Clear();
            if (grdLaneLegend.RowDefinitions.Count > 0) grdLaneLegend.RowDefinitions.Clear();

            //for (int x = 0; x < 2; x++)
            //{
            //    ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
            //    grdLaneLegend.ColumnDefinitions.Add(gridColumn);
            //}

            grdLaneLegend.Children.Clear();

            var query = (from t in dtLane.AsEnumerable()
                         join u in dtLaneLegend.AsEnumerable()
                             on t.Field<string>("LANE_NO") equals u.Field<string>("LANE_NO")
                         select new
                         {
                             LaneNo = t.Field<string>("LANE_NO"),
                             LaneColor = u.Field<string>("COLORMAP")
                         }).ToList();

            int y = 0;

            if (query.Any())
            {
                foreach (var item in query)
                {
                    RowDefinition gridRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };
                    grdLaneLegend.RowDefinitions.Add(gridRow);

                    StackPanel stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(2, 2, 2, 2)
                    };

                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(item.LaneColor));

                    Ellipse ellipseLane = Util.CreateEllipse(HorizontalAlignment.Center,
                                                             VerticalAlignment.Center,
                                                             12,
                                                             12,
                                                             convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                             1,
                                                             new Thickness(2, 2, 2, 2),
                                                             null,
                                                             null);

                    TextBlock textBlockLane = Util.CreateTextBlock("Lane " + item.LaneNo,
                                                                   HorizontalAlignment.Center,
                                                                   VerticalAlignment.Center,
                                                                   11,
                                                                   FontWeights.Bold,
                                                                   Brushes.Black,
                                                                   new Thickness(1, 1, 1, 1),
                                                                   new Thickness(0, 0, 0, 0),
                                                                   null,
                                                                   Cursors.Hand,
                                                                   string.Empty);

                    stackPanel.Children.Add(ellipseLane);
                    stackPanel.Children.Add(textBlockLane);

                    textBlockLane.PreviewMouseUp += TextBlockLane_PreviewMouseUp;

                    Grid.SetColumn(stackPanel, 0);
                    Grid.SetRow(stackPanel, y);

                    grdLaneLegend.Children.Add(stackPanel);

                    y++;
                }
            }
        }
        #endregion


        #region SelectRollMap
        private void SelectRollMap()
        {
            string mPoint_top = null;//1(로딩량) 2(두께) 3(전체)
            string mPstn_top = null;//1(Wet) 2(Dry)
            string mPoint_back = null;//1(로딩량) 2(두께) 3(전체)
            string mPstn_back = null;//1(Wet) 2(Dry)

            if (GetCboUserConf(ref mPoint_top, ref mPstn_top, ref mPoint_back, ref mPstn_back) == false)
            {
                #region 코터 롤맵화면에서 마지막 조회 정보가 없는 경우, TOP/BACK Default 설정
                DataTable dtResult = GetAreaCommonCode("ROLLMAP_DEFAULT_GAUGE", "E2000");

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (string.Equals("W", dtResult.Rows[0]["ATTR1"].GetString()))
                    {
                        mPoint_top = "1";
                        mPstn_top = "1";
                        mPoint_back = "1";
                        mPstn_back = "1";
                    }
                    else if (string.Equals("T", dtResult.Rows[0]["ATTR1"].GetString()))
                    {
                        mPoint_top = "2";
                        mPstn_top = "1";
                        mPoint_back = "2";
                        mPstn_back = "1";
                    }
                    else
                    {
                        mPoint_top = dtResult.Rows[0]["ATTR1"].GetString();
                        mPstn_top = Convert.ToInt16(dtResult.Rows[0]["ATTR1"]) % 2 == 0 ? "1" : "2";
                        mPoint_back = dtResult.Rows[0]["ATTR2"].GetString();
                        mPstn_back = Convert.ToInt16(dtResult.Rows[0]["ATTR2"]) % 2 == 0 ? "1" : "2";
                    }
                }
                else
                {
                    mPoint_top = "1";
                    mPstn_top = "1";
                    mPoint_back = "1";
                    mPstn_back = "1";
                }
                #endregion
            }

            //BR_PRD_SEL_ROLLMAP_CHART => BR_PRD_SEL_RM_RPT_CHART(LANE_LIST 파라미터 추가해서 처리)
            const string bizRuleName = "BR_PRD_SEL_RM_RPT_CHART_SBL";
            DataSet inDataSet = new DataSet();

            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("MPOINT_TOP", typeof(string));
            inTable.Columns.Add("MPSTN_TOP", typeof(string));
            inTable.Columns.Add("MPOINT_BACK", typeof(string));
            inTable.Columns.Add("MPSTN_BACK", typeof(string));
            inTable.Columns.Add("LANE_LIST", typeof(string));//요약 LANE LIST(DELITER:콤마)

            int laneCnt = msbRollMapLane.SelectedItems.Count;
            string strLaneList = msbRollMapLane.SelectedItemsToString;//요약 LANE LIST(DELITER:콤마 1,3,5) 

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = txtLot.Text;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["ADJFLAG"] = (CoordinateType.RelativeCoordinates == coordinateType) ? "1" : "2";
            newRow["MPOINT_TOP"] = mPoint_top;
            newRow["MPSTN_TOP"] = mPstn_top;
            newRow["MPOINT_BACK"] = mPoint_back;
            newRow["MPSTN_BACK"] = mPstn_back;

            if (string.IsNullOrEmpty(strLaneList) == false && ((DataView)msbRollMapLane.ItemsSource).Count > laneCnt)
            {
                newRow["LANE_LIST"] = strLaneList;
            }

            inTable.Rows.Add(newRow);

            ShowLoadingIndicator();

            SetChartOutputContextMenu(false);

            GetRollMapDefectList();

            SetTestCut_NonArea();

            //new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_LANE,OUT_HEAD,OUT_DEFECT,OUT_GAUGE,OUT_IN_LANE,OUT_IN_HEAD,OUT_IN_DEFECT,OUT_IN_GAUGE", (bizResult, bizException) =>
            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_LANE,OUT_HEAD,OUT_DEFECT,OUT_GAUGE", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        HiddenLoadingIndicator();
                        return;
                    }

                    SetMenuUseCount();

                    dt2DBarcodeInfo = Get2DBarcodeInfo(txtLot.Text, wParams.equipmentCode);

                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        DataTable dtLane = bizResult.Tables["OUT_LANE"];          //완성 차트Lane, 무지부, Top Back표시 테이블
                        DataTable dtHead = bizResult.Tables["OUT_HEAD"];          //완성 차트 전체 길이 표시 테이블
                        DataTable dtDefect = bizResult.Tables["OUT_DEFECT"];        //완성 차트 불량정보 표시 테이블
                        DataTable dtGauge = bizResult.Tables["OUT_GAUGE"];         //완성 차트 두께 측정 데이터 표시 테이블

                        #region [완성 차트 영역]

                        #region 1.dtHead
                        if (CommonVerify.HasTableRow(dtHead))
                        {
                            GetInOutProcessMaxLength(dtHead, ChartConfigurationType.Output);
                            InitializeChart(chartOutput);
                            DrawStartEndYAxis(dtHead, ChartConfigurationType.Output);
                        }
                        #endregion

                        #region 2.dtLane
                        if (CommonVerify.HasTableRow(dtLane))
                        {
                            DrawLane(dtLane, ChartConfigurationType.Output);
                            DrawWinderDirection(dtLane); //완성차트쪽에서 하는 로직이라서 호출위치(투입차트) 변경하는게 맞을 듯
                            DisplayTabLocation(dtLane, ChartConfigurationType.Output);
                        }
                        #endregion

                        #region 3.dtGauge
                        if (CommonVerify.HasTableRow(dtGauge))
                        {
                            dtOutGauge = dtGauge;

                            DrawGauge(dtGauge, ChartConfigurationType.Output);
                            DrawThickness(dtGauge, ChartConfigurationType.Output);
                            DrawLaneLegend();
                        }
                        #endregion

                        #region 4.dtDefect
                        if (CommonVerify.HasTableRow(dtDefect))
                        {
                            DrawDefect(dtDefect, ChartConfigurationType.Output);
                            DrawDefectLegend(dtDefect, ChartConfigurationType.Output);
                        }

                        if (_isSearchMode || tiTagSection.Visibility == Visibility.Collapsed) SetChartOutputContextMenu(false);
                        else SetChartOutputContextMenu(true);

                        dtBaseDefectOutput = dtDefect.Copy();

                        tbOutputCore.Text = "CORE";




                        #endregion

                        #endregion

                        HiddenLoadingIndicator();
                    }
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        #endregion

        #region GetCommonCode
        /// <summary>
        /// COMMONCODE 조회
        /// </summary>
        /// <param name="cmcdType"></param>
        /// <param name="cmCode"></param>
        /// <returns></returns>
        private DataTable GetCommonCode(string cmcdType, string cmCode = null)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = cmcdType;
            dr["CMCODE"] = cmCode;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dt);
        }
        #endregion

        #region GetAreaCommonCode
        /// <summary>
        /// 동별 공통코드
        /// </summary>
        /// <param name="cmcdType"></param>
        /// <param name="cmCode"></param>
        /// <returns></returns>
        private DataTable GetAreaCommonCode(string cmcdType, string cmCode = null)
        {
            DataTable dtRst = null;

            string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE";//DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA => DA_BAS_SEL_TB_MMD_AREA_COM_CODE

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));//추가
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.LANGID;//추가
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = cmcdType;
                newRow["COM_CODE"] = cmCode;
                inTable.Rows.Add(newRow);

                dtRst = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRst;
        }
        #endregion

        #region GetLegend
        /// <summary>
        /// 스펙 컬러 정보로 상단 범례 구성(6개 제한)
        /// </summary>
        private void GetLegend()
        {
            #region 좌표게 설정
            if (rdoAbsolutecoordinates.IsChecked != null && rdoAbsolutecoordinates.IsChecked == true)
            {
                coordinateType = CoordinateType.Absolutecoordinates;
            }
            else
            {
                coordinateType = CoordinateType.RelativeCoordinates;
            }
            #endregion

            grdLegendUp.Children.Clear();
            grdLegendDown.Children.Clear();

            DataTable dt = dtLineLegend.Copy();

            #region 1.스펙 컬러 정보 구성

            #region previous code(주석처리)
            //DataTable dt = dtLineLegend.AsEnumerable()
            //                .Where(row => new string[] { "HH", "H", "SV", "L", "LL", "Non" }.Contains(row.Field<string>("VALUE")))
            //                .CopyToDataTable();

            //if(coordinateType == CoordinateType.Absolutecoordinates)
            //{
            //    dt.Rows.Add(1, "#FAED7D", regendRect[0], "LOAD", "RECT");
            //    dt.Rows.Add(1, "#00D8FF", regendRect[1], "LOAD", "RECT");
            //    dt.Rows.Add(1, "#FFA7A7", regendRect[2], "LOAD", "RECT");
            //}
            #endregion

            string[] exceptCode = new string[] { "OK", "NG", "Ch", "Err" };

            string[] exceptLegend = new string[] { "QA샘플", "자주검사", "최외각 폐기" };

            if (coordinateType == CoordinateType.RelativeCoordinates)
            {
                //exceptCode, exceptLegend 해당하는 모든 코드 삭제
                dt.AsEnumerable().Where(r => exceptCode.Contains(r.Field<string>("CMCODE"))
                                          || exceptLegend.Contains(r.Field<string>("CMCODE"))).ToList().ForEach(row => row.Delete());
                dt.AcceptChanges();
            }
            else
            {
                //exceptCode 해당하는 코드 삭제
                dt.AsEnumerable().Where(r => exceptCode.Contains(r.Field<string>("CMCODE"))).ToList().ForEach(row => row.Delete());
                dt.AcceptChanges();
            }

            #endregion

            #region 2.스펙 컬러 범례를 상(1Column)하(2Column)로 분리
            for (int x = 0; x < 3; x++)
            {
                ColumnDefinition gridCol1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };

                if (x < 1)
                {
                    grdLegendUp.ColumnDefinitions.Add(gridCol1);
                }
                else
                {
                    grdLegendDown.ColumnDefinitions.Add(gridCol1);
                }
            }
            #endregion

            if (CommonVerify.HasTableRow(dt))
            {
                #region 1.스펙 컬러 범례(사각형[QA샘플/자주검사/최외각 폐기] 제외) : 상단
                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(3, 0, 3, 0)
                };

                foreach (DataRow row in dt.AsEnumerable().Where(r => exceptLegend.Contains(r.Field<string>("VALUE")) == false))
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                    Rectangle rectangleLegend = new Rectangle()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 15,
                        Width = 15,
                        StrokeThickness = 1,
                        Margin = new Thickness(3, 0, 3, 0),
                        Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                    };
                    TextBlock textBlockDescription = new TextBlock()
                    {
                        Text = Util.NVC(row["VALUE"]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        Margin = new Thickness(2, 0, 2, 2)
                    };
                    sp.Children.Add(rectangleLegend);
                    sp.Children.Add(textBlockDescription);
                }

                Grid.SetColumn(sp, 0);
                Grid.SetRow(sp, 1);
                grdLegendUp.Children.Add(sp);
                #endregion

                #region 2.스펙 컬러 범례(사각형[QA샘플/자주검사/최외각 폐기] 추가) , 이음매,  SCRAP, RW_SCRAP Polygon 범례(삼각형) : 하단(좌)
                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 3, 0)
                };

                #region 스펙 컬러 범례(사각형[QA샘플/자주검사/최외각 폐기] 추가) 
                foreach (DataRow row in dt.AsEnumerable().Where(r => exceptLegend.Contains(r.Field<string>("VALUE")) == true))
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                    Rectangle rectangleLegend = new Rectangle()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 15,
                        Width = 15,
                        StrokeThickness = 1,
                        Margin = new Thickness(3, 0, 3, 0),
                        Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                    };
                    TextBlock textBlockDescription = new TextBlock()
                    {
                        Text = Util.NVC(row["VALUE"]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        Margin = new Thickness(2, 0, 2, 2)
                    };
                    stackPanel.Children.Add(rectangleLegend);
                    stackPanel.Children.Add(textBlockDescription);
                }
                #endregion

                string[] legendArray;

                if (coordinateType == CoordinateType.Absolutecoordinates)
                {
                    legendArray = new string[]
                    {
                        ObjectDic.Instance.GetObjectName("SCRAP")
                      , ObjectDic.Instance.GetObjectName("CONNECTION")
                      , ObjectDic.Instance.GetObjectName("PET")
                      , ObjectDic.Instance.GetObjectName("ETC")
                      , ObjectDic.Instance.GetObjectName("RW_SCRAP")
                    };
                }
                else
                {
                    legendArray = new string[]
                    {
                        ObjectDic.Instance.GetObjectName("SCRAP")
                      , ObjectDic.Instance.GetObjectName("CONNECTION")
                      , ObjectDic.Instance.GetObjectName("PET")
                      , ObjectDic.Instance.GetObjectName("ETC")
                    };
                }

                for (int i = 0; i < legendArray.Length; i++)
                {
                    PointCollection pointCollection = new PointCollection();
                    pointCollection.Add(new Point(10, 10));
                    pointCollection.Add(new Point(20, 10));
                    pointCollection.Add(new Point(15, 30));

                    SolidColorBrush convertFromString;

                    if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("SCRAP")))
                    {
                        convertFromString = new SolidColorBrush(Colors.Black);
                    }
                    else if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("CONNECTION")))
                    {
                        convertFromString = new SolidColorBrush(Colors.Red);
                    }
                    else if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("PET")))
                    {
                        convertFromString = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("ETC")))
                    {
                        convertFromString = new SolidColorBrush(Colors.Gray);
                    }
                    else if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("RW_SCRAP")))
                    {
                        convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE400"));
                    }
                    else
                    {
                        convertFromString = new SolidColorBrush(Colors.White);
                    }

                    Polygon polygon = new Polygon()
                    {
                        Points = pointCollection,
                        Width = 13,
                        Height = 14,
                        Stretch = Stretch.Fill,
                        StrokeThickness = 1,
                        Fill = convertFromString,
                        Stroke = convertFromString,
                        Margin = new Thickness(3, 0, 3, 0)
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = legendArray[i].GetString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        Margin = new Thickness(2, 0, 2, 3)
                    };

                    stackPanel.Children.Add(polygon);
                    stackPanel.Children.Add(textBlock);
                }

                Grid.SetColumn(stackPanel, 0);
                Grid.SetRow(stackPanel, 1);
                grdLegendDown.Children.Add(stackPanel);
                #endregion

                #region 3.CUT_SPLIT 범례(Path) : 하단(우)
                StackPanel spLotCut = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 3, 0)
                };

                Path pathLotCut = new Path { Fill = Brushes.Black, Stretch = Stretch.Uniform };
                pathLotCut.Width = 17;
                pathLotCut.Height = 17;

                //Lot Cut 표시 Path
                string pathData = "M20.84 22.73L12.86 14.75L12.17 15.44L6.66 13.08C6.71 12.65 6.89 12.24 7.22 11.91L8.62 10.5L1.11 3L2.39 1.73L22.11 21.46L20.84 22.73M15.41 12.21L11.16 7.96L16.41 2.71C17.2 1.93 18.46 1.93 19.24 2.71L20.66 4.13C21.44 4.91 21.44 6.17 20.66 6.96L15.41 12.21M17.12 6.25C17.5 6.64 18.15 6.64 18.54 6.25C18.93 5.86 18.93 5.23 18.54 4.83C18.15 4.44 17.5 4.44 17.12 4.83C16.73 5.23 16.73 5.86 17.12 6.25M5 16V21.75L10.81 16.53L5.81 14.53L5 16Z";
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Geometry));
                pathLotCut.Data = (Geometry)converter.ConvertFrom(pathData);

                TextBlock textBlockSample = new TextBlock
                {
                    Text = ObjectDic.Instance.GetObjectName("CUT_SPLIT"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12,
                    Margin = new Thickness(3, 0, 3, 3)
                };

                spLotCut.Children.Add(pathLotCut);
                spLotCut.Children.Add(textBlockSample);

                Grid.SetColumn(spLotCut, 1);
                Grid.SetRow(spLotCut, 1);
                grdLegendDown.Children.Add(spLotCut);
                #endregion
            }
        }
        #endregion

        #region ValidationSearch
        /// <summary>
        /// 조회시 Validation(LOTID)
        /// </summary>
        /// <returns></returns>
        private bool ValidationSearch()
        {
            if (string.IsNullOrEmpty(txtLot.Text))
            {
                Util.MessageValidation("SFU1366");
                return false;
            }

            //if (string.IsNullOrEmpty(msbRollMapLane.SelectedItemsToString))
            //{
            //    // %1(을)를 선택하세요.
            //    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE"));
            //    return false;
            //}

            return true;
        }
        #endregion

        #region SelectRollMapLot
        /// <summary>
        /// 롤맵 LOT 여부
        /// </summary>
        /// <param name="strLotId"></param>
        /// <param name="strWipSeq"></param>
        /// <returns></returns>
        private bool SelectRollMapLot(string strLotId, string strWipSeq)
        {
            bool bRst = false;

            try
            {
                const string bizRuleName = "BR_PRD_CHK_ROLLMAP_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = strLotId;
                dr["WIPSEQ"] = strWipSeq;
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["ROLLMAP_LOT_YN"].Equals("Y"))
                    {
                        bRst = true;
                    }
                    else
                    {
                        bRst = false;
                    }
                }
            }
            catch (Exception) { }

            return bRst;
        }
        #endregion

        #region IsRollMapResultApply
        /// <summary>
        /// 설비 롤맵 수불여부(ROLLMAP_SBL_APPLY_FLAG)
        /// </summary>
        /// <param name="strProc"></param>
        /// <param name="strEqsg"></param>
        /// <returns></returns>
        private bool IsRollMapResultApply(string strProc, string strEqsg)
        {
            bool bRst = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = strProc;
                dr["EQSGID"] = strEqsg;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "RQSTDT", "RSLTDT", RQSTDT);
                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                    {
                        bRst = true;
                    }
                }
            }
            catch (Exception) { }

            return bRst;
        }
        #endregion


        #region UpdateScrollbars
        /// <summary>
        /// chartInput, chartOutput X축 ScrollBar Visibility 설정
        /// </summary>
        /// <param name="chartConfigurationType"></param>
        private void UpdateScrollbars(ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;
            AxisScrollBar axisScrollBar = (AxisScrollBar)c1Chart.View.AxisX.ScrollBar;

            double sxTop = c1Chart.View.AxisX.Scale;
            axisScrollBar.Visibility = sxTop >= 1.0 ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region SetScale
        /// <summary>
        /// chartInput, chartOutput X축 범위 설정
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="chartConfigurationType"></param>
        private void SetScale(double scale, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart;
            Button btnRefresh, btnZoomIn, btnZoomOut;

            //if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            //{
            //    #region ChartConfigurationType.Input
            //    c1Chart = chartInput;
            //    btnRefresh = btnInputRefresh;
            //    btnZoomIn = btnInputZoomIn;
            //    btnZoomOut = btnInputZoomOut;
            //    #endregion
            //}
            //else
            {
                #region ChartConfigurationType.Output
                c1Chart = chartOutput;
                btnRefresh = btnOutputRefresh;
                btnZoomIn = btnOutputZoomIn;
                btnZoomOut = btnOutputZoomOut;
                #endregion
            }
            //적용은 동일하게 한다.
            c1Chart.View.AxisX.Scale = scale;
            btnRefresh.IsCancel = !scale.Equals(1);
            btnZoomIn.IsCancel = scale > 0.002;
            btnZoomOut.IsCancel = scale < 1;
            UpdateScrollbars(chartConfigurationType);
        }
        #endregion

        #region BeginUpdateChart
        /// <summary>
        /// 화면 모든 차트를 대상으로 BeginUpdate 메서드 호출 후, Default Setting 설정한다.
        /// </summary>
        private void BeginUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                c1Chart.Reset(true);
                c1Chart.BeginUpdate();

                c1Chart.View.AxisX.Reversed = false;

                if (c1Chart.Name == "chartOutput" || c1Chart.Name == "chartInput")
                {
                    SetScale(1.0, c1Chart.Name == "chartInput" ? ChartConfigurationType.Input : ChartConfigurationType.Output);

                    c1Chart.View.AxisX.MinScale = 0.05;
                    c1Chart.View.Margin = c1Chart.Name == "chartInput" ? new Thickness(0) : new Thickness(0, 0, 0, 0);
                    c1Chart.Padding = new Thickness(10, 0, 0, 25);
                }
                else if (c1Chart.Name == "chartThickness")
                {
                    c1Chart.View.Margin = new Thickness(0);
                    c1Chart.Padding = new Thickness(10, 5, 5, 5);
                }
            }
        }
        #endregion

        #region EndUpdateChart
        /// <summary>
        /// 화면 모든 차트를 대상으로 X축 반전 후 EndUpdate 메서드 호출해서 마무리한다.
        /// </summary>
        private void EndUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                if (c1Chart.Name == "chartInput" || c1Chart.Name == "chartOutput" || c1Chart.Name == "chartThickness")
                {
                    c1Chart.View.AxisX.Reversed = true;
                }

                c1Chart.EndUpdate();
            }
        }
        #endregion


        #region GetRollMap
        /// <summary>
        /// 조회 버튼 클릭시 
        /// </summary>
        private void GetRollMap()
        {
            try
            {
                //Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));

                if (!ValidationSearch()) return;

                BeginUpdateChart();
                InitializeControls();
                InitializeDataTables();
                SelectRollMap();
                EndUpdateChart();
            }
            catch (Exception ex)
            {
                EndUpdateChart();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region DrawScrapSection
        /// <summary>
        /// 투입LOT SCRAP 정보를 가져와서, AlarmZone으로 설정에서 차트에 추가한다.
        /// </summary>
        /// <param name="lotId"></param>
        /// <param name="wipseq"></param>
        /// <param name="startPosition"></param>
        private void DrawScrapSection(string lotId, string wipseq, string startPosition)
        {
            string bizRuleName = "DA_PRD_SEL_RM_RPT_INPUT_SCRAP_PSTN";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("STRT_PSTN", typeof(decimal));

            DataRow newRow = inTable.NewRow();
            newRow["LOTID"] = lotId;
            newRow["WIPSEQ"] = wipseq.GetDecimal();
            newRow["STRT_PSTN"] = startPosition.GetDouble();
            inTable.Rows.Add(newRow);

            ShowLoadingIndicator();

            if (dsscrapSection != null) chartInput.Data.Children.Remove(dsscrapSection);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
            {
                if (bizException != null)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(bizException);
                    return;
                }

                if (CommonVerify.HasTableRow(bizResult))
                {
                    //투입 LOT의 SCRAP 시작 위치
                    double scrapStartPosition = bizResult.Rows[0]["SCRAP_STRT_PSTN"].GetDouble();
                    //투입 LOT의 SCRAP 종료 위치
                    double scrapEndPosition = bizResult.Rows[0]["SCRAP_END_PSTN"].GetDouble();

                    dsscrapSection.Near = scrapStartPosition;
                    dsscrapSection.Far = scrapEndPosition;
                    dsscrapSection.LowerExtent = 0;
                    dsscrapSection.UpperExtent = 100;

                    dsscrapSection.ConnectionStroke = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
                    dsscrapSection.ConnectionFill = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
                    dsscrapSection.Cursor = Cursors.Hand;
                    dsscrapSection.Tag = "INPUT_SCRAP";

                    #region 주석
                    //AlarmZone alarmZone = new AlarmZone
                    //{
                    //    Near = scrapStartPosition,
                    //    Far = scrapEndPosition,
                    //    ConnectionStroke = new SolidColorBrush(Colors.Black) { Opacity = 0.5 },
                    //    LowerExtent = 0,
                    //    UpperExtent = 100,
                    //    ConnectionFill = new SolidColorBrush(Colors.Black) { Opacity = 0.5 },
                    //    Cursor = Cursors.Hand,
                    //    Tag = "INPUT_SCRAP"
                    //};
                    #endregion

                    chartInput.Data.Children.Add(dsscrapSection);
                }

                HiddenLoadingIndicator();
            });

        }
        #endregion

        #region SetTagSectionClear
        private bool ValidationTagSection()
        {
            if (chartOutput.Data.Children.Count < 1)
            {
                //데이터가 없습니다.
                Util.MessageValidation("SFU1498");
                return false;
            }

            return true;
        }
        /// <summary>
        /// OUTPUT 차트에서 불량정보(TAG_SECTION) 정보를 모두 제거한다.
        /// </summary>
        private void SetTagSectionClear()
        {
            if (!ValidationTagSection()) return;

            for (int i = chartOutput.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = chartOutput.Data.Children[i];

                if (dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
                {
                    chartOutput.Data.Children.Remove(dataSeries);
                }
            }
        }
        #endregion

        #region SetLotInfo
        /// <summary>
        /// [LOT정보조회]입력한 LOTID로 롤맵관련 공정과 AREATYPE 정보로 WinParam에 해당하는 정보를 셋팅한다.
        /// </summary>
        /// <param name="sLot"></param>
        /// <param name="sProcID"></param>
        /// <param name="sAreaType"></param>
        public void SetLotInfo(string sLot, string sProcID, string sAreaType)
        {
            try
            {
                string Stap = string.Empty;
                string sBizName = sAreaType.Equals("A") ? "DA_PRD_SEL_LOT_STATUS2_ASSY" : "DA_PRD_SEL_LOT_STATUS2";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = sProcID;

                RQSTDT.Rows.Add(dr);

                /////////////////////////////////////////////////////////////////////
                //ExecuteServiceSync_Multi : 동기식 메서드
                /////////////////////////////////////////////////////////////////////
                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                if (!sAreaType.Equals("A") && dtResult != null && dtResult.Rows.Count > 0)
                {
                    wParams = new WinParams();

                    wParams.lotId = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    wParams.wipSeq = Util.NVC(dtResult.Rows[0]["WIPSEQ"].GetString());
                    wParams.processCode = Util.NVC(dtResult.Rows[0]["PROCID"]);
                    wParams.laneQty = Util.NVC(dtResult.Rows[0]["LANE_QTY"]);
                    wParams.equipmentCode = Util.NVC(dtResult.Rows[0]["ROLLMAP_EQPTID"]);
                    wParams.equipmentName = Util.NVC(dtResult.Rows[0]["ROLLMAP_EQPTNAME"]);
                    wParams.equipmentSegmentCode = Util.NVC(dtResult.Rows[0]["EQSGID"]);
                    wParams.version = Util.NVC(dtResult.Rows[0]["PROD_VER_CODE"]);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region SetNewLotInfo
        /// <summary>
        /// [LOT정보조회]입력한 LOTID 기준으로 LOT 정보를 가져와서 WinParam 정보 설정한다.
        /// </summary>
        /// <param name="lotId"></param>
        private bool SetNewLotInfo(string lotId)
        {
            bool bRst = false;

            try
            {
                DataSet inData = new DataSet();

                DataTable dtRqst = inData.Tables.Add("INDATA");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("GUBUN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;
                dr["GUBUN"] = "F";//F/R

                dtRqst.Rows.Add(dr);

                // CSR : [E20231018-000975] - 전극 재와인딩 공정 추가 Biz 분리 처리 [동별 공통코드 : REWINDING_LOT_INFO_TREE]
                string sBizName = string.Empty;

                DataTable dtRW = GetAreaCommonCode("REWINDING_LOT_INFO_TREE", null);

                if (dtRW != null && dtRW.Rows.Count > 0)
                {
                    sBizName = "BR_PRD_SEL_LOT_INFO_END_ELEC_LV";
                }
                else
                {
                    sBizName = "BR_PRD_SEL_LOT_INFO_END";
                }

                /////////////////////////////////////////////////////////////////////
                //ExecuteServiceSync_Multi : 동기식 메서드
                /////////////////////////////////////////////////////////////////////
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "LOTSTATUS,TREEDATA", inData);

                if (dsRslt != null && dsRslt.Tables["LOTSTATUS"].Rows.Count > 0)
                {
                    string strAreaType = string.Empty;

                    if (dsRslt.Tables["TREEDATA"].Rows.Count > 0)
                    {
                        strAreaType = dsRslt.Tables["TREEDATA"].Rows[0]["AREATYPE"].ToString();
                    }

                    string strProc = dsRslt.Tables["LOTSTATUS"].Rows[0]["PROCID"].ToString();

                    //이전 조회시 공정과 같은 경우에만 진행
                    if (strProc == wParams.processCode)
                    {
                        SetLotInfo(lotId, strProc, strAreaType);
                        bRst = true;
                    }
                    else
                    {
                        //같은 공정이 아닙니다.
                        Util.MessageValidation("SFU1446");
                        txtLot.Text = wParams.lotId;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return bRst;
        }
        #endregion

        #region SetMenuUseCount
        /// <summary>
        /// 메뉴 사용이력
        /// </summary>
        private void SetMenuUseCount()
        {
            const string bizRuleName = "COR_INS_MENU_USE_COUNT";

            DataTable inTable = new DataTable();
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("MENUID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["SYSTEM_ID"] = Common.Common.APP_System;
            newRow["MENUID"] = "SFU010160122";                 // 메뉴ID 코터:SFU010160121, 롤프레스:SFU010160122, 슬리터:SFU010160123, 리와인딩:SFU010160124  
            newRow["USERID"] = LoginInfo.USERID;
            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["EQSGID"] = wParams.equipmentSegmentCode;   // LoginInfo.CFG_EQSG_ID;
            newRow["PROCID"] = wParams.processCode;            // LoginInfo.CFG_PROC_ID;
            newRow["EQPTID"] = wParams.equipmentCode;          // LoginInfo.CFG_EQPT_ID;
            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (menuHitReuslt, menuHitException) => { });

        }
        #endregion

        #region GetCboUserConf
        /// <summary>
        /// 롤맵(코터)화면 마지막 조회시 TOP/BACK 선택정보(COMBOBOX INDEX => VALUE[WL,DL,WT,DT,TD])
        /// </summary>
        /// <param name="mPoint_top"></param>
        /// <param name="mPstn_top"></param>
        /// <param name="mPoint_back"></param>
        /// <param name="mPstn_back"></param>
        /// <returns></returns>
        private bool GetCboUserConf(ref string mPoint_top, ref string mPstn_top, ref string mPoint_back, ref string mPstn_back)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("WRK_TYPE", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("CONF_TYPE", typeof(string));
            dtRqst.Columns.Add("CONF_KEY1", typeof(string));
            dtRqst.Columns.Add("CONF_KEY2", typeof(string));
            dtRqst.Columns.Add("CONF_KEY3", typeof(string));
            dtRqst.Columns.Add("USER_CONF01", typeof(string));
            dtRqst.Columns.Add("USER_CONF02", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["WRK_TYPE"] = "SELECT";
            dr["USERID"] = LoginInfo.USERID;
            dr["CONF_TYPE"] = "USER_CONFIG_COMBOX";                     // USER_CONFIG_CBO => USER_CONFIG_COMBOX(USER_CONFIG_RADIOBUTTON 사용안함)
            dr["CONF_KEY1"] = "LGC.GMES.MES.COM001.COM001_RM_CHART_CT"; // 코터 랏 신규화면 
            dr["CONF_KEY2"] = "cboMeasurementTop";                      // cboTop  => cboMeasurementTop
            dr["CONF_KEY3"] = "cboMeasurementBack";                     // cboBack => cboMeasurementBack
            dtRqst.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow drConf in dtResult.Rows)
                {
                    // 사용자가 롤맵 코터화면에서 마지막으로 조회시 설정한 계측기 콤보박스 설정값

                    #region 이전 코드(콤보박스 인덱스 값으로 설정되던 로직을 선택한 Value로 저장하는 것으로 변경해서 주석)
                    // mPoint_top, mPoint_back : 1(로딩량) 2(두께) 3(전체)
                    // mPstn_top, mPstn_back   : 1(Wet) 2(Dry)
                    //mPoint_top  = dtResult.Rows[0]["USER_CONF01"].ToString();
                    //mPoint_back = dtResult.Rows[0]["USER_CONF02"].ToString();

                    //mPstn_top  = Convert.ToInt16(dtResult.Rows[0]["USER_CONF01"]) % 2 == 0 ? "1" : "2";
                    //mPstn_back = Convert.ToInt16(dtResult.Rows[0]["USER_CONF02"]) % 2 == 0 ? "1" : "2";
                    #endregion

                    string strTopValue = dtResult.Rows[0]["USER_CONF01"].ToString(); //WL	WET(로딩량), DL	DRY(로딩량), WT	WET(두께), DT DRY(두께)

                    if (string.IsNullOrEmpty(strTopValue)) { return false; } //데이터 없는 경우, 판단하지 않는다.

                    string strTopFirst = strTopValue.Substring(0, 1); //W(WET), D(DRY)
                    string strTopSecond = strTopValue.Substring(1, 1); //L(로딩량), T(두께)

                    string strBackValue = dtResult.Rows[0]["USER_CONF02"].ToString(); //WL	WET(로딩량), DL	DRY(로딩량), WT	WET(두께), DT DRY(두께), TD	TOTAL DRY

                    if (string.IsNullOrEmpty(strBackValue)) { return false; } //데이터 없는 경우, 판단하지 않는다.

                    string strBackFirst = strBackValue.Substring(0, 1); //W(WET), D(DRY)
                    string strBackSecond = strBackValue.Substring(1, 1); //L(로딩량), T(두께)

                    #region A.mPoint_top, mPoint_back 셋팅(1(로딩량) 2(두께) 3(전체))    B.mPstn_top, mPstn_back 셋팅(1(Wet) 2(Dry))
                    mPstn_top = strTopFirst.Equals("W") ? "1" : "2"; //1(Wet) 2(Dry)
                    mPstn_back = strBackFirst.Equals("W") ? "1" : "2"; //1(Wet) 2(Dry)

                    mPoint_top = strTopSecond.Equals("L") ? "1" : "2"; //1(로딩량) 2(두께) 3(전체)
                    mPoint_back = strBackSecond.Equals("L") ? "1" : "2"; //1(로딩량) 2(두께) 3(전체)

                    if (strBackValue.Equals("TD")) //TOTAL DRY
                    {
                        mPoint_back = "3"; //1(로딩량) 2(두께) 3(전체)
                    }
                    #endregion
                }

                return true;
            }

            return false;
        }
        #endregion

        #region ShowLoadingIndicator / HiddenLoadingIndicator
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    loadingIndicator.Visibility = Visibility.Visible;
                }));
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }));
            }
        }
        #endregion

        private DataTable GetCommoncodeUse(string codeType, string code)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = codeType;
                dr["CMCODE"] = code;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion

        #region Events

        #region RollMap_Loaded
        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (null != e) e.Handled = true;
            //var source = PresentationSource.FromVisual(this);
            //if (null != source && null != source.CompositionTarget)
            //{
            //    var matrix = source.CompositionTarget.TransformToDevice;
            //    var dpiTransform = new ScaleTransform(1 / matrix.M11, 1 / matrix.M22);
            //    if (dpiTransform.CanFreeze) dpiTransform.Freeze();
            //    LayoutTransform = dpiTransform;
            //}

            InitSetting();
            InitializeCombo();
            GetUserConf();
            GetLegend();
            if (!_isSearchMode) GetErpClose();
            //GetRollMap();
            Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            this.Closing += CMM_RM_RP_RESULT_Closing;
        }

        private void CMM_RM_RP_RESULT_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
                {
                    if (DataTableConverter.GetValue(row.DataItem, "UPDATE_FLAG").GetString() == "Y")
                    {
                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString()))
                        {
                            e.Cancel = true;

                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU10027"), null, "Modify RollMap", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                foreach (C1.WPF.DataGrid.DataGridRow row in dgOutsideScrap.Rows)
                {
                    if (DataTableConverter.GetValue(row.DataItem, "UPDATE_FLAG").GetString() == "Y")
                    {
                        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString()))
                        {
                            e.Cancel = true;

                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU10027"), null, "Modify RollMap", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                SaveUserConf();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CMM_RM_RP_RESULT_Closed(object sender, EventArgs e)
        {
            try
            {
                SaveUserConf();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetErpClose()
        {
            const string bizRuleName = "DA_PRD_SEL_RM_RPT_ERP_CLOSE";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = wParams.wipSeq;
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(result))
            {
                if (result.Rows[0]["ERP_CLOSING_FLAG"].GetString() == "CLOSE")
                {
                    Util.MessageValidation("SFU10005");

                    //_isSearchMode = true;
                }
            }
        }

        private void SaveUserConf()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("WRK_TYPE", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("CONF_TYPE", typeof(string));
            dtRqst.Columns.Add("CONF_KEY1", typeof(string));
            dtRqst.Columns.Add("CONF_KEY2", typeof(string));
            dtRqst.Columns.Add("CONF_KEY3", typeof(string));
            dtRqst.Columns.Add("USER_CONF01", typeof(string));
            dtRqst.Columns.Add("USER_CONF02", typeof(string));
            dtRqst.Columns.Add("USER_CONF03", typeof(string));

            string rdoCoordinate = "1";

            if (rdoAbsolutecoordinates.IsChecked == true) rdoCoordinate = "2";

            DataRow drNew = dtRqst.NewRow();
            drNew["WRK_TYPE"] = "SAVE";
            drNew["USERID"] = LoginInfo.USERID;
            drNew["CONF_TYPE"] = "USER_CONFIG_COMBOBOX";
            drNew["CONF_KEY1"] = "LGC.GMES.MES.CMM001.CMM_RM_RP_RESULT";
            drNew["CONF_KEY2"] = "rdoCoordinate";
            drNew["CONF_KEY3"] = "1";
            drNew["USER_CONF01"] = rdoCoordinate;

            if (rdoTCApplyY_FPI.IsChecked == true || rdoTCApplyN_FPI.IsChecked == true) drNew["USER_CONF03"] = "Y";
            else drNew["USER_CONF03"] = "N";

            dtRqst.Rows.Add(drNew);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
        }



        #endregion

        #region DataTemplate

        #region chartPetScrap
        /// <summary>
        /// chartPetScrap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gdPetScrap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            //return;
            Grid grid = sender as Grid;

            string[] splitItem = grid.Tag.GetString().Split(';');

            //if (splitItem[6] != null && splitItem[6].GetString() == "4")
            //{
            //    return;
            //}

            string equipmentMeasurementCode = splitItem[0];
            string startPosition = splitItem[1];
            string endPosition = splitItem[2];
            string scrapQty = $"{splitItem[2].GetDouble() - splitItem[1].GetDouble():###,###,###,##0.##}";
            string lotId = splitItem[3];
            string wipSeq = splitItem[4];
            string collectSeq = splitItem[5];
            string rollMapCollectType = string.Empty;
            string winderLength = string.Empty;

            if (string.Equals(equipmentMeasurementCode, "SCRAP_SECTION"))
            {
                //rollMapCollectType = splitItem[6];
                //winderLength = splitItem[7];
                popScrapSection(collectSeq);


            }

            //if (string.Equals(equipmentMeasurementCode, "PET"))
            //{
            //    //CMM_ROLLMAP_SCRAP_LOSS => CMM_RM_SCRAP_LOSS
            //    CMM_RM_SCRAP_LOSS popRollMapPet = new CMM_RM_SCRAP_LOSS();
            //    popRollMapPet.FrameOperation = FrameOperation;

            //    object[] parameters = new object[4];
            //    parameters[0] = equipmentMeasurementCode;
            //    parameters[1] = startPosition;
            //    parameters[2] = endPosition;
            //    parameters[3] = scrapQty;

            //    C1WindowExtension.SetParameters(popRollMapPet, parameters);

            //    Dispatcher.BeginInvoke(new Action(() => popRollMapPet.ShowModal()));
            //}
            //else if (string.Equals(equipmentMeasurementCode, "RW_SCRAP"))
            //{
            //    if (isRollMapResultLink && isRollMapLot) return; //조성근 2024.08.28 롤프레스 롤맵 수불 적용
            //    //CMM_ROLLMAP_RW_SCRAP => CMM_RM_RW_SCRAP
            //    CMM_RM_RW_SCRAP popRollMapRewinderScrap = new CMM_RM_RW_SCRAP();
            //    popRollMapRewinderScrap.FrameOperation = FrameOperation;

            //    object[] parameters = new object[10];
            //    parameters[0] = lotId;
            //    parameters[1] = wipSeq;
            //    parameters[2] = wParams.processCode;
            //    parameters[3] = wParams.equipmentCode;
            //    parameters[4] = equipmentMeasurementCode;
            //    parameters[5] = startPosition;
            //    parameters[6] = endPosition;
            //    parameters[7] = winderLength;
            //    parameters[8] = collectSeq;
            //    parameters[9] = rollMapCollectType;

            //    C1WindowExtension.SetParameters(popRollMapRewinderScrap, parameters);

            //    popRollMapRewinderScrap.Closed += popRollMapRewinderScrap_Closed;

            //    Dispatcher.BeginInvoke(new Action(() => popRollMapRewinderScrap.ShowModal()));
            //}
            //else
            //{
            //    //SCRAP 선택 시 Input 영역에 스크랩한 구간을 표시 함.
            //    DrawScrapSection(lotId, wipSeq, startPosition);

            //    //CMM_ROLLMAP_SCRAP => CMM_RM_SCRAP
            //    CMM_RM_SCRAP popRollMapScrap = new CMM_RM_SCRAP();
            //    popRollMapScrap.FrameOperation = FrameOperation;

            //    object[] parameters = new object[7];
            //    parameters[0] = lotId;
            //    parameters[1] = wipSeq;
            //    parameters[2] = equipmentMeasurementCode;
            //    parameters[3] = collectSeq;
            //    parameters[4] = startPosition;
            //    parameters[5] = endPosition;
            //    parameters[6] = scrapQty;

            //    C1WindowExtension.SetParameters(popRollMapScrap, parameters);

            //    popRollMapScrap.Closed += popRollMapScrap_Closed;

            //    Dispatcher.BeginInvoke(new Action(() => popRollMapScrap.ShowModal()));
            //}
        }
        #endregion

        #endregion

        #region 상단조건

        #region LOTID(txtLot_KeyDown)
        /// <summary>
        /// LOTID
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string strLotId = txtLot.Text.Trim();

                if (string.IsNullOrEmpty(strLotId))
                {
                    return;
                }

                /////////////////////////////////////////////////////////////////////
                //현재 LOTID 입력시 WinParam 정보가 변경되지 않는 점이 있어서,
                //[LOT 정보 조회] 화면에서 입력한 LOTID 기준으로 정보 가져오는 로직 적용
                /////////////////////////////////////////////////////////////////////
                if (SetNewLotInfo(strLotId))
                {
                    Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
                    //GetRollMap();
                }
            }
        }
        #endregion

        #region 계측기(로딩량/웹게이지)
        /// <summary>
        /// 계측기(로딩량/웹게이지)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoWebgauge_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 계측기(두께)
        /// <summary>
        /// 계측기(두께)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoThickness_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 맵표현설정:요약 선택시 LANE 선택 체크
        private void cboRollMapExpressSummary_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetMsbRollMapLaneCheck();
        }

        #endregion

        #region 롤맵보정(롤맵보정이후)
        /// <summary>
        /// 롤맵보정(롤맵보정이후)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoRelativeCoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
        }
        #endregion

        #region 롤맵보정(롤맵보정이전)
        /// <summary>
        /// 롤맵보정(롤맵보정이전)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoAbsolutecoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
        }

        private bool GetUserConf()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("WRK_TYPE", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("CONF_TYPE", typeof(string));
            dtRqst.Columns.Add("CONF_KEY1", typeof(string));
            dtRqst.Columns.Add("CONF_KEY2", typeof(string));
            dtRqst.Columns.Add("CONF_KEY3", typeof(string));
            //dtRqst.Columns.Add("USER_CONF01", typeof(string));
            //dtRqst.Columns.Add("USER_CONF02", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["WRK_TYPE"] = "SELECT";
            dr["USERID"] = LoginInfo.USERID;
            dr["CONF_TYPE"] = "USER_CONFIG_COMBOBOX";
            dr["CONF_KEY1"] = "LGC.GMES.MES.CMM001.CMM_RM_RP_RESULT";  //KEY1
            dr["CONF_KEY2"] = "rdoCoordinate";   //KEY2
            dr["CONF_KEY3"] = "1";   //KEY3
            dtRqst.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
            _bUseFPI = false;
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow drConf in dtResult.Rows)
                {
                    // 사용자가 롤맵 코터화면에서 마지막으로 조회시 설정한 계측기 콤보박스 설정값

                    string rdoCoordinate = dtResult.Rows[0]["USER_CONF01"].ToString(); //WL	WET(로딩량), DL	DRY(로딩량), WT	WET(두께), DT DRY(두께)

                    if (string.IsNullOrEmpty(rdoCoordinate)) { return false; } //데이터 없는 경우, 판단하지 않는다.

                    if (rdoCoordinate == "2")
                    {
                        if (rdoRelativeCoordinates.IsChecked == true) rdoAbsolutecoordinates.IsChecked = true;
                    }
                    else
                    {
                        if (rdoAbsolutecoordinates.IsChecked == true) rdoRelativeCoordinates.IsChecked = true;
                    }

                    if (dtResult.Rows[0]["USER_CONF03"] != null && dtResult.Rows[0]["USER_CONF03"].ToString() == "Y") _bUseFPI = true;  //FPI 사용여부
                }

                return true;
            }

            return false;
        }

        #endregion


        #region 구간불량 등록
        private void popRollMapPositionUpdate_Closed(object sender, EventArgs e)
        {
            // CMM_ROLLMAP_PSTN_UPD => CMM_RM_TAG_SECTION
            CMM_RM_TAG_SECTION popup = sender as CMM_RM_TAG_SECTION;

            if (popup != null && popup.IsUpdated)
            {
                IsUpdated = true;
                SelectRollMapTagSection();
            }
            else
            {
                //SetTagSectionClear();//기존로직(미등록시 호출하는 이유(?))
            }
        }

        /// <summary>
        /// 구간불량(TAG_SECTION) 등록 (보정이후 상대좌표인 경우에만 등록 가능.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRollMapPositionEdit_Click(object sender, RoutedEventArgs e)
        {
            //if (coordinateType == CoordinateType.Absolutecoordinates)
            //{
            //    Util.MessageValidation("SFU8541", "구간불량 등록");
            //    return;
            //}

            // CMM_ROLLMAP_PSTN_UPD => CMM_RM_TAG_SECTION
            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[12];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = string.Empty;
            parameters[2] = string.Empty;
            parameters[3] = wParams.processCode;
            parameters[4] = wParams.equipmentCode;
            parameters[5] = wParams.wipSeq;
            parameters[6] = 0;
            parameters[7] = string.Empty;
            parameters[8] = true;
            parameters[9] = xOutputMaxLength;
            parameters[10] = Visibility.Visible;
            if (coordinateType == CoordinateType.Absolutecoordinates)
                parameters[11] = "N";
            else
                parameters[11] = "Y";

            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;

            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            if (textBlock?.Tag == null) return;

            string[] splitItem = textBlock.Tag.GetString().Split(';');

            string startPosition = splitItem[0];
            string endPosition = splitItem[1];
            string collectSeq = splitItem[2];
            string collectType = splitItem[3];

            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[12];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = startPosition;
            parameters[2] = endPosition;
            parameters[3] = wParams.processCode;
            parameters[4] = wParams.equipmentCode;
            parameters[5] = wParams.wipSeq;
            parameters[6] = collectSeq;
            parameters[7] = collectType;
            parameters[8] = false;
            parameters[9] = xOutputMaxLength;
            parameters[10] = _isSearchMode || tiTagSection.Visibility == Visibility.Collapsed ? Visibility.Collapsed : Visibility.Visible;
            parameters[11] = "Y";
            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }

        #endregion

        #region 불량
        private void popupRollMapDataCollect_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_DATACOLLECT popup = sender as CMM_ROLLMAP_DATACOLLECT;

            if (popup != null && popup.IsUpdated)
            {
                //btnSearch_Click(btnSearch, null);
                Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            }
        }

        /// <summary>
        /// 불량 등록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            CMM_RM_RP_DATACOLLECT popupRollMapDataCollect = new CMM_RM_RP_DATACOLLECT();
            popupRollMapDataCollect.FrameOperation = FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = wParams.processCode;
            parameters[1] = wParams.equipmentCode;
            parameters[2] = wParams.lotId;
            parameters[3] = wParams.wipSeq;
            parameters[4] = wParams.laneQty;

            C1WindowExtension.SetParameters(popupRollMapDataCollect, parameters);
            popupRollMapDataCollect.Closed += popupRollMapDataCollect_Closed;

            Dispatcher.BeginInvoke(new Action(() => popupRollMapDataCollect.ShowModal()));
        }
        #endregion

        #region (팝업)SCRAP 선택 시 Input 영역에 스크랩한 구간을 표시 함.

        private void popRollMapScrap_Closed(object sender, EventArgs e)
        {
            return;
            ////CMM_ROLLMAP_SCRAP => CMM_RM_SCRAP
            //CMM_RM_SCRAP popup = sender as CMM_RM_SCRAP;

            //if (popup != null && popup.IsUpdated)
            //{
            //    IsUpdated = true;
            //    //btnSearch_Click(btnSearch, null);
            //    Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            //}
        }
        #endregion

        #region 조회
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //GetRollMap();
            Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
        }
        #endregion

        #endregion

        #region IN ROLLMAP

        #region [IN]Original size
        /// <summary>
        /// [IN]Original size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0, ChartConfigurationType.Input);
        }
        #endregion

        #region [IN]확대
        /// <summary>
        /// [IN]확대
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (chartInput.View.AxisX.MinScale >= chartInput.View.AxisX.Scale * 0.75)
            {
                return;
            }

            SetScale(chartInput.View.AxisX.Scale * 0.75, ChartConfigurationType.Input);
        }
        #endregion

        #region [IN]축소
        /// <summary>
        /// [IN]축소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartInput.View.AxisX.Scale * 1.50, ChartConfigurationType.Input);
        }
        #endregion

        #region [IN]좌우반전
        /// <summary>
        /// [IN]좌우반전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartInput.BeginUpdate();
            chartInput.View.AxisX.Reversed = !chartInput.View.AxisX.Reversed;
            chartInput.EndUpdate();
        }
        #endregion

        #region [IN]상하반전
        /// <summary>
        /// [IN]상하반전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInputReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartInput.BeginUpdate();
            chartInput.View.AxisY.Reversed = !chartInput.View.AxisY.Reversed;
            chartInput.EndUpdate();
        }
        #endregion

        #endregion

        #region OUT ROLLMAP

        #region [OUT]Original size
        /// <summary>
        /// [OUT]Original size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutputRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0, ChartConfigurationType.Output);
        }
        #endregion

        #region [OUT]확대
        /// <summary>
        /// [OUT]확대
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutputZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (chartOutput.View.AxisX.MinScale >= chartOutput.View.AxisX.Scale * 0.75)
            {
                return;
            }

            SetScale(chartOutput.View.AxisX.Scale * 0.75, ChartConfigurationType.Output);
        }
        #endregion

        #region [OUT]축소
        /// <summary>
        /// [OUT]축소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutputZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartOutput.View.AxisX.Scale * 1.50, ChartConfigurationType.Output);
        }
        #endregion

        #region [OUT]좌우반전
        /// <summary>
        /// [OUT]좌우반전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutputReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartOutput.BeginUpdate();
            chartOutput.View.AxisX.Reversed = !chartOutput.View.AxisX.Reversed;
            chartOutput.EndUpdate();
        }
        #endregion

        #region [OUT]상하반전
        /// <summary>
        /// [OUT]상하반전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOutputReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartOutput.BeginUpdate();
            chartOutput.View.AxisY.Reversed = !chartOutput.View.AxisY.Reversed;
            chartOutput.EndUpdate();
        }

        #endregion

        #endregion


        #region DescriptionOnPreviewMouseUp
        /// <summary>
        /// 불량정보 범례 Description 마우스 오버시 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DescriptionOnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;

                if (textBlock == null || textBlock.Tag == null || textBlock.Text == null) return;

                ShowLoadingIndicator();

                C1Chart c1Chart;
                DataTable dtDefect, dtBaseDefect;

                string[] splitItem = textBlock.Tag.GetString().Split('|');
                string textBlockTag = splitItem[0].GetString();//Input/Output
                string colorMap = splitItem[1].GetString();//색상

                if (string.Equals(textBlockTag, ChartConfigurationType.Input.ToString()))
                {
                    c1Chart = chartInput;
                    dtDefect = dtDefectInput;
                    dtBaseDefect = dtBaseDefectInput;
                }
                else
                {
                    c1Chart = chartOutput;
                    dtDefect = dtDefectOutput;
                    dtBaseDefect = dtBaseDefectOutput;
                }

                string[] defectDataSeries = new string[]
                {
                    "VISION_SURF_NG_TOP"     , "VISION_SURF_NG_BACK"
                  , "VISION_NG_TOP"          , "VISION_NG_BACK"
                  , "INS_ALIGN_VISION_NG_TOP", "INS_ALIGN_VISION_NG_BACK"
                  , "INS_OVERLAY_VISION_NG"
                };

                for (int i = c1Chart.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = c1Chart.Data.Children[i];

                    if (defectDataSeries.Contains(dataSeries.Tag.GetString()))
                    {
                        c1Chart.Data.Children.Remove(dataSeries);
                    }
                }

                const string roundbrackets = "(";
                int lastIndex = textBlock.Text.LastIndexOf(roundbrackets);
                string textBlockText = textBlock.Text.Substring(0, lastIndex);

                if (textBlock.Foreground == Brushes.Black)
                {
                    textBlock.Foreground = Brushes.LightGray;
                    dtDefect.Rows.Add(textBlockText, textBlockText, colorMap);
                }
                else
                {
                    //dtDefect.Select("EQPT_MEASR_PSTN_ID = '" + textBlockText + "' And ABBR_NAME = '" + textBlockText + "'").ToList().ForEach(row => row.Delete());
                    dtDefect.Select("EQPT_MEASR_PSTN_ID = '" + textBlockText + "' And ABBR_NAME = '" + textBlockText + "' And COLORMAP = '" + colorMap + "' ")
                            .ToList()
                            .ForEach(row => row.Delete());
                    dtDefect.AcceptChanges();
                    textBlock.Foreground = Brushes.Black;
                }
                var queryDefect = dtBaseDefect.AsEnumerable()
                                              .Where(x => !dtDefect.AsEnumerable().Any(y => y.Field<string>("ABBR_NAME") == x.Field<string>("ABBR_NAME")
                                                                                         && y.Field<string>("COLORMAP") == x.Field<string>("COLORMAP")));

                if (queryDefect.Any())
                {
                    DrawDefect(queryDefect.CopyToDataTable(), (ChartConfigurationType)Enum.Parse(typeof(ChartConfigurationType), textBlockTag), true);
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

        #region TextBlockLane_PreviewMouseUp
        private void TextBlockLane_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null) return;

                ShowLoadingIndicator();

                //_isResearch = true;
                chartThickness.Data.Children.Clear();

                string laneNo = textBlock.Text.Replace("Lane", "").Trim();

                if (textBlock.Foreground == Brushes.Black)
                {
                    dtLane.Rows.Remove(dtLane.Select("LANE_NO = '" + laneNo + "'")[0]);
                    textBlock.Foreground = Brushes.LightGray;
                }

                else //if(textBlock.Foreground == Brushes.Gray)
                {
                    dtLane.Rows.Add(laneNo);
                    textBlock.Foreground = Brushes.Black;
                }

                DrawThickness(dtOutGauge, ChartConfigurationType.Output);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        private void rdoTCApplyY_FPI_Checked(object sender, RoutedEventArgs e)
        {
            if(_EnableChangeFPIQty == "Y") txtGoodQty.IsEnabled = true;
            if (txtGoodQty.Value == 0 && _defaltFPIQty > 0) txtGoodQty.Value = _defaltFPIQty;
        }

        private void rdoTCApplyN_FPI_Checked(object sender, RoutedEventArgs e)
        {
            if (_EnableChangeFPIQty == "Y") txtGoodQty.IsEnabled = true;
            if (txtGoodQty.Value == 0 && _defaltFPIQty > 0) txtGoodQty.Value = _defaltFPIQty;
        }

        private void rdoTCApplyY_TESTCUT_Checked(object sender, RoutedEventArgs e)
        {
            txtGoodQty.IsEnabled = false;
            txtGoodQty.Value = 0;
        }

        private void rdoTCApplyN_TESTCUT_Checked(object sender, RoutedEventArgs e)
        {
            txtGoodQty.IsEnabled = false;
            txtGoodQty.Value = 0;
        }

        #endregion Events
        #region
        private void tcDataCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.RemovedItems.Count > 0)
            {
                C1TabItem olditem = e.RemovedItems[0] as C1TabItem;
                if (olditem != null)
                {
                    if (string.Equals(olditem.Name, "tiTagSection"))
                    {
                        dgLotInfo.EndEdit(true);
                    }
                }
            }
        }
        private void SelectRollMapTagSpot()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                for (int i = chartOutput.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartOutput.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TAG_SPOT"))
                    {
                        chartOutput.Data.Children.Remove(dataSeries);
                    }
                }

                //전체 조회 시 데이터가 많은 경우 바인딩 하는 속도 문제로 TAG_SPOT 따로 처리
                SelectRollMapPointInfo();
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void InitializeTagSectionControl()
        {
            //SetScale(1.0);

            //_selectedStartSectionText = string.Empty;
            //_selectedStartSectionPosition = string.Empty;
            //_selectedEndSectionPosition = string.Empty;
            //_selectedCollectType = string.Empty;
            //_selectedStartSampleFlag = string.Empty;
            //_selectedEndSampleFlag = string.Empty;

            _isSelectedTagSection = false;
            _selectedStartSection = 0;
            _selectedEndSection = 0;
            _selectedchartPosition = 0;

            //if (!(bool)btnZoomOut.IsEnabled) btnZoomOut.IsEnabled = true;
            //if (!(bool)btnZoomIn.IsEnabled) btnZoomIn.IsEnabled = true;
            //if (!(bool)btnRefresh.IsEnabled) btnRefresh.IsEnabled = true;
        }
        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private static DataTable MakeTableForDisplay(DataTable dt, ChartDisplayType chartDisplayType)
        {
            var dtBinding = dt.Copy();

            if (!CommonVerify.HasTableRow(dt)) return dtBinding;

            if (chartDisplayType == ChartDisplayType.TagSectionStart || chartDisplayType == ChartDisplayType.TagSectionEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });


                var queryBarcodeInfoList = (from t in dtBinding.AsEnumerable()
                                            where t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty()
                                            && t.Field<string>("ADJ_LANE_NO").IsNotEmpty()
                                            select new
                                            {
                                                MarkBarcodeString = t.Field<string>("MRK_2D_BCD_STR"),
                                                StartPosition = t.GetValue("ADJ_STRT_PSTN").GetDecimal(),  //t.Field<decimal>("ADJ_STRT_PSTN"),
                                                EndPosition = t.GetValue("ADJ_END_PSTN").GetDecimal() // t.Field<decimal>("ADJ_END_PSTN")
                                            }).GroupBy(x => new
                                            {
                                                x.MarkBarcodeString,
                                                x.StartPosition,
                                                x.EndPosition,
                                            }).Select(g => new
                                            {
                                                Mark2DBarCode = g.Key.MarkBarcodeString,
                                                Mark2DBarCodeStartPosition = g.Key.StartPosition,
                                                Mark2DBarCodeEndPosition = g.Key.EndPosition,
                                                Mark2DBarCodeCount = g.Count()
                                            }).ToList();


                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_ADJ_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -19 : -27;
                    row["TAG"] = $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();

                    //L001: Normal, L002: NFF, L003: 소형(2170)
                    if (string.Equals(row["DFCT_2D_BCR_STD"].GetString(), "L002") && !string.IsNullOrEmpty(row["MRK_2D_BCD_STR"].GetString().Trim()))
                    {
                        if (queryBarcodeInfoList.Any())
                        {
                            foreach (var item in queryBarcodeInfoList)
                            {
                                if (row["LANE_QTY"].GetInt() == item.Mark2DBarCodeCount && row["ADJ_STRT_PSTN"].GetDecimal() == item.Mark2DBarCodeStartPosition && row["ADJ_END_PSTN"].GetDecimal() == item.Mark2DBarCodeEndPosition)
                                {
                                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                                }
                                else
                                {
                                    string laneInfo = row["MRK_2D_BCD_STR"].GetString().Trim().Substring(0, 2);
                                    row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                                }
                            }
                        }
                    }
                    else
                    {
                        //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                        row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                    }
                }
            }
            else if (chartDisplayType == ChartDisplayType.Mark)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = -13;
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[" + row["ROLLMAP_CLCT_TYPE"] + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";
                }
            }
            else if (chartDisplayType == ChartDisplayType.Material)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double), AllowDBNull = true });

                foreach (DataRow row in dtBinding.Rows)
                {
                    if (!string.IsNullOrEmpty(row["INPUT_LOTID"].GetString()))
                    {
                        row["SOURCE_AVG_VALUE"] = (row["SOURCE_END_PSTN"].GetDouble() + row["SOURCE_STRT_PSTN"].GetDouble()) / 2;
                    }
                    else
                    {
                        row["SOURCE_AVG_VALUE"] = DBNull.Value;
                    }

                    if (row["EQPT_MEASR_PSTN_ID"].GetString() == "UW" || row["EQPT_MEASR_PSTN_ID"].GetString() == "INS_SLURRY_TOP" || row["EQPT_MEASR_PSTN_ID"].GetString() == "INS_SLURRY_BACK")
                    {
                        //절연, Foil 인 경우 0
                        row["SOURCE_Y_PSTN"] = 0;
                    }
                    else
                    {
                        row["SOURCE_Y_PSTN"] = 30;
                    }
                }
            }
            else if (chartDisplayType == ChartDisplayType.Sample)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "STRT_PSTN", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "END_PSTN", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                for (int i = 0; i < dtBinding.Rows.Count; i++)
                {
                    double toolTipStartPosition;
                    double rowStartPosition;
                    double rowEndPosition;
                    double.TryParse(Util.NVC(dtBinding.Rows[i]["RAW_END_PSTN"]), out rowEndPosition);

                    if (i == dtBinding.Rows.Count - 1)
                    {
                        double.TryParse(Util.NVC(dtBinding.Rows[i]["RAW_STRT_PSTN"]), out rowStartPosition);
                    }
                    else
                    {
                        double.TryParse(Util.NVC(dtBinding.Rows[i + 1]["RAW_STRT_PSTN"]), out rowStartPosition);
                    }

                    double.TryParse(Util.NVC(dtBinding.Rows[i]["RAW_STRT_PSTN"]), out toolTipStartPosition);

                    dtBinding.Rows[i]["TOOLTIP"] = dtBinding.Rows[i]["ROW_NUM"] + " Cut" + "( " + $"{toolTipStartPosition:###,###,###,##0.##}" + "m" + " ~ " + $"{rowEndPosition:###,###,###,##0.##}" + "m" + ")";
                    dtBinding.Rows[i]["END_PSTN"] = $"{rowEndPosition:###,###,###,##0.##}";
                    dtBinding.Rows[i]["SOURCE_Y_PSTN"] = 98;
                    dtBinding.Rows[i]["STRT_PSTN"] = $"{rowStartPosition:###,###,###,##0.##}";

                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSpot)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = -36;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + " : " + row["ABBR_NAME"] + " [" + sourceStartPosition + "m]";
                    row["TAG"] = null;
                }
            }

            dtBinding.AcceptChanges();
            return dtBinding;
        }

        private void SetChartOutputContextMenu(bool isEnabled)
        {
            for (int row = 0; row < chartOutput.ContextMenu.Items.Count; row++)
            {
                MenuItem item = chartOutput.ContextMenu.Items[row] as MenuItem;

                switch (item.Name.ToString())
                {
                    case "tagSectionStart":
                        item.Click -= tagSectionStart_Click;
                        item.Click += tagSectionStart_Click;
                        item.IsEnabled = isEnabled;
                        break;

                    case "tagSectionEnd":
                        item.Click -= tagSectionEnd_Click;
                        item.Click += tagSectionEnd_Click;
                        item.IsEnabled = isEnabled;
                        break;

                    case "tagSectionClear":
                        item.Click -= tagSectionClear_Click;
                        item.Click += tagSectionClear_Click;
                        item.IsEnabled = isEnabled;
                        break;
                }
            }
        }

        private void chartOutput_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point curPoint = e.GetPosition(chartOutput.View);
            var chartpoint = chartOutput.View.PointToData(curPoint);

            if (chartpoint == null || chartpoint.X < 0)
            {
                Util.MessageInfo("SFU3625");    //선택한 영역이 잘못되었습니다.
                return;
            }

            _selectedchartPosition = chartpoint.X;
        }

        private void tagSectionStart_Click(object sender, RoutedEventArgs e)
        {
            //chartOutput
            if (!ValidationTagSection()) return;

            // No.1 구간불량 시작 버튼 클릭 시 기존에 구간불량 시작 위치 또는 구간불량 종료 위치 데이터가 있는 경우 해당 데이터 삭제 후 다시 시작위치 작성
            if (_isSelectedTagSection)
            {
                // 구간불량 DataSeries 가 chartCoater 상에 존재하는 경우 임.
                for (int i = chartOutput.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartOutput.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
                    {
                        chartOutput.Data.Children.Remove(dataSeries);
                        _selectedStartSection = 0;
                        _selectedEndSection = 0;
                    }
                }
            }

            _selectedStartSection = _selectedchartPosition;

            DataTable dtTagSectionStart = new DataTable();

            dtTagSectionStart.Columns.Add("COLORMAP", typeof(string));
            dtTagSectionStart.Columns.Add("TAGNAME", typeof(string));
            dtTagSectionStart.Columns.Add("TAG", typeof(string));
            dtTagSectionStart.Columns.Add("TOOLTIP", typeof(string));
            dtTagSectionStart.Columns.Add("AXISX_PSTN", typeof(double));
            dtTagSectionStart.Columns.Add("AXISY_PSTN", typeof(double));

            double tagsectionStart = _selectedStartSection;

            DataRow dr = dtTagSectionStart.NewRow();
            dr["COLORMAP"] = "#FFE400";
            dr["TAGNAME"] = "S";
            dr["TAG"] = _selectedStartSection.GetString();
            dr["TOOLTIP"] = ObjectDic.Instance.GetObjectName("시작위치") + " [" + ObjectDic.Instance.GetObjectName("길이") + $"{tagsectionStart:###,###,###,##0.##}" + "m" + " ]";
            dr["AXISX_PSTN"] = _selectedStartSection;
            dr["AXISY_PSTN"] = 95;

            dtTagSectionStart.Rows.Add(dr);

            XYDataSeries ds = new XYDataSeries();
            ds.ItemsSource = DataTableConverter.Convert(dtTagSectionStart);
            ds.XValueBinding = new Binding("AXISX_PSTN");
            ds.ValueBinding = new Binding("AXISY_PSTN");
            ds.ChartType = ChartType.XYPlot;
            ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
            ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
            ds.Margin = new Thickness(0);
            ds.Tag = "TagSectionStart";

            ds.PlotElementLoaded += (s, e1) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };
            chartOutput.Data.Children.Add(ds);

            _isSelectedTagSection = true;
        }

        private void tagSectionEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTagSection()) return;

            if (!_isSelectedTagSection)
            {
                //시작위치를 먼저 선택하세요.
                Util.MessageInfo("SFU8529");
                return;
            }

            if (_selectedStartSection > _selectedchartPosition)
            {
                Util.MessageInfo("SFU8530");
                return;
            }


            _selectedEndSection = _selectedchartPosition;

            DataTable dtTagSectionEnd = new DataTable();

            dtTagSectionEnd.Columns.Add("COLORMAP", typeof(string));
            dtTagSectionEnd.Columns.Add("TAGNAME", typeof(string));
            dtTagSectionEnd.Columns.Add("TAG", typeof(string));
            dtTagSectionEnd.Columns.Add("TOOLTIP", typeof(string));
            dtTagSectionEnd.Columns.Add("AXISX_PSTN", typeof(double));
            dtTagSectionEnd.Columns.Add("AXISY_PSTN", typeof(double));

            double tagsectionEnd = _selectedEndSection;

            DataRow dr = dtTagSectionEnd.NewRow();
            dr["COLORMAP"] = "#FFE400";
            dr["TAGNAME"] = "E";
            dr["TAG"] = _selectedEndSection.GetString();
            dr["TOOLTIP"] = ObjectDic.Instance.GetObjectName("종료위치") + " [" + ObjectDic.Instance.GetObjectName("길이") + $"{tagsectionEnd:###,###,###,##0.##}" + "m" + " ]";
            dr["AXISX_PSTN"] = _selectedEndSection;
            dr["AXISY_PSTN"] = 95;

            dtTagSectionEnd.Rows.Add(dr);

            XYDataSeries ds = new XYDataSeries();
            ds.ItemsSource = DataTableConverter.Convert(dtTagSectionEnd);
            ds.XValueBinding = new Binding("AXISX_PSTN");
            ds.ValueBinding = new Binding("AXISY_PSTN");
            ds.ChartType = ChartType.XYPlot;
            ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
            ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
            ds.Margin = new Thickness(0);
            ds.Tag = "TagSectionEnd";

            ds.PlotElementLoaded += (s, e1) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };
            chartOutput.Data.Children.Add(ds);

            _isSelectedTagSection = true;

            PopupTagSection();
        }

        private void PopupTagSection()
        {
            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            double startSection;
            double endSection;

            startSection = _selectedStartSection;
            endSection = _selectedEndSection;

            object[] parameters = new object[12];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = $"{startSection:###,###,###,##0.##}";
            parameters[2] = $"{endSection:###,###,###,##0.##}";
            parameters[3] = wParams.processCode;
            parameters[4] = wParams.equipmentCode;
            parameters[5] = wParams.wipSeq;
            parameters[6] = 0;
            parameters[7] = string.Empty;
            parameters[8] = true;
            parameters[9] = xOutputMaxLength;
            parameters[10] = Visibility.Visible;
            if (coordinateType == CoordinateType.Absolutecoordinates)
                parameters[11] = "N";
            else
                parameters[11] = "Y";


            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));

        }

        private void tagSectionClear_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTagSection()) return;

            for (int i = chartOutput.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = chartOutput.Data.Children[i];
                if (dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
                {
                    chartOutput.Data.Children.Remove(dataSeries);
                }
            }

            InitializeTagSectionControl();
        }

        #endregion

        private void GetRollMapDefectList()
        {
            const string bizRuleName = "BR_PRD_SEL_RM_RPT_DEFECT_LIST";

            DataSet InDataSet = new DataSet();
            DataTable IndataTable = InDataSet.Tables.Add("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(decimal));
            IndataTable.Columns.Add("PROCID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQPTID"] = wParams.equipmentCode;
            Indata["LOTID"] = txtLot.Text;
            Indata["WIPSEQ"] = wParams.wipSeq;
            Indata["PROCID"] = wParams.processCode;
            IndataTable.Rows.Add(Indata);

            Util.gridClear(dgLotInfo);
            Util.gridClear(dgTagSectionSingle);
            Util.gridClear(dgOutsideScrap);
            Util.gridClear(dgScrapSection);

            GetRollMapDefectSum();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "TAG_SECTION,TAG_SECTION_SINGLE,OUTSIDE,SCRAP_SECTION,TAG_DEFECT_MES", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        return;
                    }

                    DataTable dtTagSection = bizResult.Tables["TAG_SECTION"];
                    DataTable dtTagSectionSingle = bizResult.Tables["TAG_SECTION_SINGLE"];
                    DataTable dtOutSide = bizResult.Tables["OUTSIDE"];
                    DataTable dtScrapSection = bizResult.Tables["SCRAP_SECTION"];
                    DataTable dtTagDefectMes = bizResult.Tables["TAG_DEFECT_MES"];

                    dtTagSection.Columns.Add("CHK", typeof(Boolean));
                    dtTagSectionSingle.Columns.Add("CHK", typeof(Boolean));
                    dtOutSide.Columns.Add("CHK", typeof(Boolean));
                    dtScrapSection.Columns.Add("CHK", typeof(Boolean));
                    dtTagDefectMes.Columns.Add("CHK", typeof(Boolean));

                    if (CommonVerify.HasTableRow(dtTagSection))
                    {
                        dtTagSection.Columns.Add("OLD_ADJ_STRT_PSTN", typeof(decimal));
                        dtTagSection.Columns.Add("OLD_ADJ_END_PSTN", typeof(decimal));
                        dtTagSection.Columns.Add("OLD_RESNCODE", typeof(string));
                        dtTagSection.Columns.Add("OLD_ROLLMAP_CLCT_TYPE", typeof(string));
                        dtTagSection.Columns.Add("UPDATE_FLAG", typeof(string));
                        dtTagSection.Columns.Add("DEL_FLAG", typeof(string));
                        dtTagSection.Columns.Add("LEVEL", typeof(string));

                        for (int i = 0; i < dtTagSection.Rows.Count; i++)
                        {
                            dtTagSection.Rows[i]["CHK"] = false;
                            dtTagSection.Rows[i]["OLD_ADJ_STRT_PSTN"] = dtTagSection.Rows[i]["ADJ_STRT_PSTN"];
                            dtTagSection.Rows[i]["OLD_ADJ_END_PSTN"] = dtTagSection.Rows[i]["ADJ_END_PSTN"];
                            dtTagSection.Rows[i]["OLD_RESNCODE"] = dtTagSection.Rows[i]["RESNCODE"];
                            dtTagSection.Rows[i]["OLD_ROLLMAP_CLCT_TYPE"] = dtTagSection.Rows[i]["ROLLMAP_CLCT_TYPE"];
                            dtTagSection.Rows[i]["UPDATE_FLAG"] = "N";
                            if (dtTagSection.Rows[i]["ADJ_END_PSTN"] == null || dtTagSection.Rows[i]["ADJ_END_PSTN"].GetDecimal() == 0)
                            {
                                dtTagSection.Rows[i]["DEL_FLAG"] = "Y";
                            }
                            else
                            {
                                dtTagSection.Rows[i]["DEL_FLAG"] = "N";
                            }
                            dtTagSection.Rows[i]["LEVEL"] = _strSectionDefect;
                        }

                        dgLotInfo.ItemsSource = DataTableConverter.Convert(dtTagSection);
                    }

                    if (CommonVerify.HasTableRow(dtTagSectionSingle))
                    {
                        dtTagSectionSingle.Columns.Add("OLD_ADJ_STRT_PSTN", typeof(decimal));
                        dtTagSectionSingle.Columns.Add("OLD_ADJ_END_PSTN", typeof(decimal));
                        dtTagSectionSingle.Columns.Add("OLD_RESNCODE", typeof(string));
                        dtTagSectionSingle.Columns.Add("OLD_ROLLMAP_CLCT_TYPE", typeof(string));
                        dtTagSectionSingle.Columns.Add("METHOD", typeof(string));
                        dtTagSectionSingle.Columns.Add("DEL_FLAG", typeof(string));
                        dtTagSectionSingle.Columns.Add("LEVEL", typeof(string));

                        for (int i = 0; i < dtTagSectionSingle.Rows.Count; i++)
                        {
                            dtTagSectionSingle.Rows[i]["CHK"] = false;
                            dtTagSectionSingle.Rows[i]["OLD_ADJ_STRT_PSTN"] = dtTagSectionSingle.Rows[i]["ADJ_STRT_PSTN"];
                            dtTagSectionSingle.Rows[i]["OLD_ADJ_END_PSTN"] = dtTagSectionSingle.Rows[i]["ADJ_END_PSTN"];
                            dtTagSectionSingle.Rows[i]["OLD_RESNCODE"] = dtTagSectionSingle.Rows[i]["RESNCODE"];
                            dtTagSectionSingle.Rows[i]["OLD_ROLLMAP_CLCT_TYPE"] = dtTagSectionSingle.Rows[i]["ROLLMAP_CLCT_TYPE"];
                            dtTagSectionSingle.Rows[i]["METHOD"] = "";
                            if (dtTagSectionSingle.Rows[i]["ADJ_END_PSTN"] == null || dtTagSectionSingle.Rows[i]["ADJ_END_PSTN"].GetDecimal() == 0)
                            {
                                dtTagSectionSingle.Rows[i]["DEL_FLAG"] = "Y";
                            }
                            else
                            {
                                dtTagSectionSingle.Rows[i]["DEL_FLAG"] = "N";
                            }
                            dtTagSectionSingle.Rows[i]["LEVEL"] = _strSectionDefect;
                        }
                        dgTagSectionSingle.ItemsSource = DataTableConverter.Convert(dtTagSectionSingle);
                        //DrawDefectTagSectionSingle(dtTagSectionSingle);
                    }

                    if (CommonVerify.HasTableRow(dtOutSide))
                    {
                        dtOutSide.Columns.Add("OLD_DEFECT_LEN", typeof(string));
                        dtOutSide.Columns.Add("OLD_RESNCODE", typeof(string));
                        dtOutSide.Columns.Add("OLD_ROLLMAP_CLCT_TYPE", typeof(string));
                        dtOutSide.Columns.Add("METHOD", typeof(string));
                        dtOutSide.Columns.Add("UPDATE_FLAG", typeof(string));
                        dtOutSide.Columns.Add("LEVEL", typeof(string));

                        for (int i = 0; i < dtOutSide.Rows.Count; i++)
                        {
                            dtOutSide.Rows[i]["CHK"] = false;
                            dtOutSide.Rows[i]["OLD_DEFECT_LEN"] = dtOutSide.Rows[i]["DEFECT_LEN"];
                            dtOutSide.Rows[i]["OLD_RESNCODE"] = dtOutSide.Rows[i]["RESNCODE"];
                            dtOutSide.Rows[i]["OLD_ROLLMAP_CLCT_TYPE"] = dtOutSide.Rows[i]["ROLLMAP_CLCT_TYPE"];
                            dtOutSide.Rows[i]["METHOD"] = "";
                            dtOutSide.Rows[i]["UPDATE_FLAG"] = "N";

                            var query = _dtOutsideComCode.AsEnumerable().Where(o => o.Field<string>("COM_CODE").Equals(dtOutSide.Rows[i]["EQPT_MEASR_PSTN_ID"])).ToList();
                            DataTable dtLevel = query.Any() ? query.CopyToDataTable() : _dtOutsideComCode.Clone();
                            if (CommonVerify.HasTableRow(dtLevel) == true)
                            {
                                dtOutSide.Rows[i]["LEVEL"] = dtLevel.Rows[0]["ATTR3"];
                            }
                            else
                            {
                                var query2 = _dtEtcLossComCode.AsEnumerable().Where(o => o.Field<string>("COM_CODE").Equals(dtOutSide.Rows[i]["EQPT_MEASR_PSTN_ID"])).ToList();
                                DataTable dtLevel2 = query2.Any() ? query2.CopyToDataTable() : _dtEtcLossComCode.Clone();
                                if (CommonVerify.HasTableRow(dtLevel2) == true)
                                {
                                    dtOutSide.Rows[i]["LEVEL"] = dtLevel2.Rows[0]["ATTR3"];
                                }
                            }

                        }
                        dgOutsideScrap.ItemsSource = DataTableConverter.Convert(dtOutSide);
                    }

                    if (CommonVerify.HasTableRow(dtScrapSection))
                    {
                        dtScrapSection.Columns.Add("OLD_ADJ_STRT_PSTN", typeof(decimal));
                        dtScrapSection.Columns.Add("OLD_ADJ_END_PSTN", typeof(decimal));
                        dtScrapSection.Columns.Add("OLD_RESNCODE", typeof(string));
                        dtScrapSection.Columns.Add("OLD_ROLLMAP_CLCT_TYPE", typeof(string));
                        dtScrapSection.Columns.Add("METHOD", typeof(string));
                        dtScrapSection.Columns.Add("DEL_FLAG", typeof(string));
                        dtScrapSection.Columns.Add("LEVEL", typeof(string));

                        for (int i = 0; i < dtScrapSection.Rows.Count; i++)
                        {
                            dtScrapSection.Rows[i]["CHK"] = false;
                            dtScrapSection.Rows[i]["OLD_ADJ_STRT_PSTN"] = dtScrapSection.Rows[i]["ADJ_STRT_PSTN"];
                            dtScrapSection.Rows[i]["OLD_ADJ_END_PSTN"] = dtScrapSection.Rows[i]["ADJ_END_PSTN"];
                            dtScrapSection.Rows[i]["OLD_RESNCODE"] = dtScrapSection.Rows[i]["RESNCODE"];
                            dtScrapSection.Rows[i]["OLD_ROLLMAP_CLCT_TYPE"] = dtScrapSection.Rows[i]["ROLLMAP_CLCT_TYPE"];
                            dtScrapSection.Rows[i]["METHOD"] = "";
                            if (dtScrapSection.Rows[i]["ADJ_END_PSTN"] == null || dtScrapSection.Rows[i]["ADJ_END_PSTN"].GetDecimal() == 0)
                            {
                                dtScrapSection.Rows[i]["DEL_FLAG"] = "Y";
                            }
                            else
                            {
                                dtScrapSection.Rows[i]["DEL_FLAG"] = "N";
                            }
                            dtScrapSection.Rows[i]["LEVEL"] = _strScrapSection;
                        }
                        dgScrapSection.ItemsSource = DataTableConverter.Convert(dtScrapSection);
                    }

                    if (CommonVerify.HasTableRow(dtTagDefectMes))
                    {
                        dtTagDefectMes.Columns.Add("OLD_DEFECT_LEN", typeof(string));
                        dtTagDefectMes.Columns.Add("OLD_DEFECT_CNT", typeof(string));
                        dtTagDefectMes.Columns.Add("METHOD", typeof(string));

                        for (int i = 0; i < dtTagDefectMes.Rows.Count; i++)
                        {
                            dtTagDefectMes.Rows[i]["CHK"] = false;
                            dtTagDefectMes.Rows[i]["OLD_DEFECT_LEN"] = dtTagDefectMes.Rows[i]["DEFECT_LEN"];
                            dtTagDefectMes.Rows[i]["OLD_DEFECT_CNT"] = dtTagDefectMes.Rows[i]["DEFECT_CNT"];
                            dtTagDefectMes.Rows[i]["METHOD"] = "";
                        }
                        dgTagDefectMes.ItemsSource = DataTableConverter.Convert(dtTagDefectMes);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                }
            }, InDataSet);



        }

        private void GetRollMapDefectSum()
        {
            const string bizRuleName = "BR_PRD_SEL_RM_RPT_SBL_PRODQTY";

            DataTable dt = new DataTable("INDATA");
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WIPSEQ", typeof(string));

            DataRow dr = dt.NewRow();
            dr["EQPTID"] = wParams.equipmentCode;
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = wParams.wipSeq;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                tbOutputProdQty.Text = @"P" + " : " + $"{dtResult.Rows[0]["PRODQTY"].GetDouble():###,###,###,##0.##}";
                tbOutputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{dtResult.Rows[0]["PRODQTY"].GetDouble():###,###,###,##0.##}";
                tbOutputGoodQty.Text = @"G" + " : " + $"{dtResult.Rows[0]["GOODQTY"].GetDouble():###,###,###,##0.##}";
                tbOutputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{dtResult.Rows[0]["GOODQTY"].GetDouble():###,###,###,##0.##}";
                tbOutputBadQty.Text = @"B" + " : " + $"{dtResult.Rows[0]["BADQTY"].GetDouble():###,###,###,##0.##}";
                tbOutputBadQty.ToolTip = ObjectDic.Instance.GetObjectName("LOSS") + " + " + ObjectDic.Instance.GetObjectName("불량량") + " : " + $"{dtResult.Rows[0]["BADQTY"].GetDouble():###,###,###,##0.##}";
            }
        }

        #region TAG_SETION 영역
        private bool ValidationDelete()
        {
            if (!CommonVerify.HasDataGridRow(dgLotInfo))
            {
                Util.MessageInfo("SFU8501", ObjectDic.Instance.GetObjectName("삭제"));
                return false;
            }

            return true;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            dgLotInfo.EndEdit();
            dgLotInfo.EndEditRow(true);

            // 구간불량 데이터 저장
            if (!ValidationSave()) return;

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            //inTable.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
            {
                if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "UPDATE_FLAG").GetString() == "Y")
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    newRollRow["ADJ_END_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();
                    newRollRow["METHOD"] = "U";
                    newRollRow["RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString();
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            //string xml = inDataSet.GetXml();

            if (rowIndex <= 0) return;

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        SelectRollMapTagSection();
                        InitializeTagSectionControl();

                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    IsUpdated = true;
                    SelectRollMapTagSection();
                    InitializeTagSectionControl();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private bool ValidationSave()
        {
            int selectedRowIndex = 0;

            //중복체크 변수 BizRule 에서 중복 체크 처리 하기로 함.
            //string lotId = string.Empty;
            //decimal wipseq = 0;
            //string measurementPosition = string.Empty;
            //decimal collectSeq = 0;
            //decimal adjustmentStartPosition = 0;
            //decimal adjustmentEndPosition = 0;
            //int uniqueTagsections = 0;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "UPDATE_FLAG").GetString() == "Y")
                {
                    if (DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal() > DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal())
                    {
                        Util.MessageValidation("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return false;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString()))
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("활동사유"));
                        return false;
                    }

                    selectedRowIndex++;
                }
            }

            if (selectedRowIndex < 1)
            {
                // 변경된 데이터가 없습니다.
                Util.MessageValidation("SFU1566");
                return false;
            }

            return true;
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            if (!ValidationDelete()) return;

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    newRollRow["ADJ_END_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();
                    newRollRow["METHOD"] = "D";
                    newRollRow["RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString();
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }



            if (rowIndex < 1)
            {
                Util.MessageValidation("SFU1636");
                return;
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                    IsUpdated = true;
                    SelectRollMapTagSection();
                    InitializeTagSectionControl();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);

        }
        private void FindTagSection(int rowindex)
        {
            //if (rowindex < 0 || !_util.GetDataGridCheckValue(dgLotInfo, "CHK", rowindex)) return;

            string borderTag = DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "ADJ_LOTID").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "ADJ_WIPSEQ").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "EQPT_MEASR_PSTN_ID").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "CLCT_SEQNO").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "TAG_AUTO_FLAG").GetString();

            System.Windows.Media.Animation.DoubleAnimation doubleAnimation = new System.Windows.Media.Animation.DoubleAnimation();

            foreach (Grid grid in Util.FindVisualChildren<Grid>(chartOutput))
            {
                if (grid.Name == "gdSectionTag")
                {
                    if (grid.Tag.GetString() == borderTag)
                    {
                        doubleAnimation.From = grid.ActualWidth;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                        doubleAnimation.AutoReverse = true;
                        doubleAnimation.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
                        grid.BeginAnimation(WidthProperty, doubleAnimation);
                    }
                    else
                    {
                        grid.BeginAnimation(WidthProperty, null);
                    }
                }
            }
        }
        private void dgLotInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (drv == null) return;

                // 시작위치 종료위치 확인
                if (e.Cell.Column.Name == "ADJ_STRT_PSTN" || e.Cell.Column.Name == "ADJ_END_PSTN")
                {
                    if (DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() > DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble())
                    {
                        Util.MessageInfo("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return;
                    }

                    double DEFECT_LEN = DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble();
                    drv.SetValue("DEFECT_LEN", DEFECT_LEN);
                }

                if (e.Cell.Column.Name == "ADJ_STRT_PSTN" || e.Cell.Column.Name == "ADJ_END_PSTN" || e.Cell.Column.Name == "RESNCODE" || e.Cell.Column.Name == "ROLLMAP_CLCT_TYPE")
                {
                    if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_STRT_PSTN").GetDouble()) > 0)
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_END_PSTN").GetDouble()) > 0)
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "RESNCODE").GetString(), DataTableConverter.GetValue(drv, "OLD_RESNCODE").GetString()))
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "ROLLMAP_CLCT_TYPE").GetString(), DataTableConverter.GetValue(drv, "OLD_ROLLMAP_CLCT_TYPE").GetString()))
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                    }
                    else
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "N");
                    }
                }
                dgLotInfo.EndEdit();
                dgLotInfo.EndEditRow(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgLotInfo_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (dgLotInfo.CurrentRow == null || dgLotInfo.SelectedIndex == -1 || dgLotInfo.CurrentColumn == null)
                {
                    return;
                }

                int rowIndex = dgLotInfo.CurrentRow.Index;
                dgLotInfo.SelectedIndex = rowIndex;

                dgLotInfo.GetCell(rowIndex, dgLotInfo.Columns["CHK"].Index);
                if (dgLotInfo.GetCell(rowIndex, dgLotInfo.Columns["CHK"].Index).Presenter == null) return;

                RadioButton rb = dgLotInfo.GetCell(rowIndex, dgLotInfo.Columns["CHK"].Index).Presenter.Content as RadioButton;

                if (rb?.DataContext == null) return;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (rowIndex == i)
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                FindTagSection(rowIndex);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void dgLotInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (string.Equals(DataTableConverter.GetValue(drv, "DEL_FLAG"), "Y"))
            {
                e.Cancel = true;
            }
        }
        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb?.DataContext == null) return;

            if (rb.IsChecked != null)
            {
                DataRowView drv = rb.DataContext as DataRowView;
                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgLotInfo.SelectedIndex = idx;

                    // 차트에 바인딩 된 Tag Section 찾기
                    FindTagSection(idx);
                }
            }
        }
        private void SelectRollMapPointInfo()
        {
            const string bizRuleName = "DA_PRD_SEL_RM_RPT_OUTPUT_DEFECT";
            DataSet inDataSet = new DataSet();

            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("IN_OUT", typeof(string));
            //inTable.Columns.Add("LANE_LIST", typeof(string));


            int laneCnt = msbRollMapLane.SelectedItems.Count;
            string strLaneList = msbRollMapLane.SelectedItemsToString;//요약 LANE LIST(DELITER:콤마 1,3,5) 

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = txtLot.Text;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["ADJFLAG"] = (CoordinateType.RelativeCoordinates == coordinateType) ? "1" : "2";
            newRow["IN_OUT"] = "2";

            inTable.Rows.Add(newRow);

            ShowLoadingIndicator();

            GetRollMapDefectList();


            new ClientProxy().ExecuteService_Multi(bizRuleName, "RQSTDT", "RSLTDT", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        HiddenLoadingIndicator();
                        return;
                    }

                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        DataTable dtDefect = bizResult.Tables["RSLTDT"];        //완성 차트 불량정보 표시 테이블

                        var queryTagSection = dtDefect.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                        DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dtDefect.Clone();

                        if (CommonVerify.HasTableRow(dtTagSection))
                        {
                            dtTagSection.TableName = "TAG_SECTION";
                            DrawDefectTagSection(dtTagSection, ChartConfigurationType.Output);
                        }

                        #region TAG_SECTION_SINGLE(DrawDefectTagSection)
                        var queryTagSectionSingle = dtDefect.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                        DataTable dtTagSectionSingle = queryTagSectionSingle.Any() ? queryTagSectionSingle.CopyToDataTable() : dtDefect.Clone();

                        if (CommonVerify.HasTableRow(dtTagSectionSingle))
                        {
                            DrawDefectTagSectionSingle(dtTagSectionSingle);
                        }
                        #endregion

                        dtBaseDefectOutput = dtDefect.Copy();
                        HiddenLoadingIndicator();
                    }
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private void SelectRollMapTagSection()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                for (int i = chartOutput.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartOutput.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TAG_SECTION") || dataSeries.Tag.GetString().Equals("TAG_SECTION_SINGLE") || dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
                    {
                        chartOutput.Data.Children.Remove(dataSeries);
                    }
                }

                //전체 조회 시 데이터가 많은 경우 바인딩 하는 속도 문제로 TAG_SECTION 따로 처리            
                SelectRollMapPointInfo();
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void tagMerge_Click(object sender, RoutedEventArgs e)
        {
            return;
            //if (_isSearchMode) return;

            //MenuItem itemmenu = sender as MenuItem;
            //ContextMenu itemcontext = itemmenu.Parent as ContextMenu;
            //TextBlock textBlock = itemcontext.PlacementTarget as TextBlock;

            //if (textBlock == null) return;

            ////Border border = FindVisualParentByName<Border>(textBlock, "tbTagSection");
            //string[] splitItem = textBlock.Tag.GetString().Split(';');

            //// 최초 셀병합 시
            //if (string.IsNullOrEmpty(_selectedStartSectionText))
            //{
            //    if (string.Equals(textBlock.Text, "S"))
            //    {
            //        textBlock.Background = new SolidColorBrush(Colors.White);
            //    }
            //    else
            //    {
            //        Util.MessageInfo("SFU8529");    //시작위치를 먼저 선택하세요.
            //        return;
            //    }

            //    btnZoomOut.IsEnabled = false;
            //    btnZoomIn.IsEnabled = false;
            //    btnRefresh.IsEnabled = false;

            //    _selectedStartSectionText = "S";
            //    _selectedStartSectionPosition = splitItem[0];
            //    _selectedCollectType = splitItem[3];
            //    _selectedStartSampleFlag = splitItem[4];
            //}
            //else
            //{
            //    if (string.Equals(textBlock.Text, "E"))
            //    {
            //        _selectedEndSectionPosition = splitItem[1];
            //        _selectedEndSampleFlag = splitItem[4];

            //        if (_selectedEndSectionPosition.GetDouble() - _selectedStartSectionPosition.GetDouble() < 0)
            //        {
            //            Util.MessageInfo("SFU8530");    //시작위치와 종료위치를 확인하세요.
            //            return;
            //        }

            //        if (_selectedStartSampleFlag != _selectedEndSampleFlag)
            //        {
            //            Util.MessageValidation("SFU8538");    //Sample Cut 구간의 위차와 양산 구간의 위치는 병합 할수 없습니다.
            //            return;
            //        }


            //        textBlock.Background = new SolidColorBrush(Colors.White);

            //        // Tag Section Merge 팝업 호출 함.
            //        CMM_RM_TAGSECTION_MERGE popRollMapTagsectionMerge = new CMM_RM_TAGSECTION_MERGE();
            //        popRollMapTagsectionMerge.FrameOperation = FrameOperation;

            //        object[] parameters = new object[8];
            //        parameters[0] = txtLot.Text;
            //        parameters[1] = _wipSeq;
            //        parameters[2] = _selectedStartSectionPosition;
            //        parameters[3] = _selectedEndSectionPosition;
            //        parameters[4] = _processCode;
            //        parameters[5] = _equipmentCode;
            //        parameters[6] = _selectedCollectType;
            //        parameters[7] = _selectedEndSampleFlag;

            //        C1WindowExtension.SetParameters(popRollMapTagsectionMerge, parameters);
            //        popRollMapTagsectionMerge.Closed += popRollMapTagsectionMerge_Closed;
            //        Dispatcher.BeginInvoke(new Action(() => popRollMapTagsectionMerge.ShowModal()));
            //    }
            //    else
            //    {
            //        Util.MessageInfo("SFU8531");    //선택된 시작위치가 존재 합니다.
            //        DrawRollMapTagSection(_dtTagSection);

            //        _selectedStartSectionText = string.Empty;
            //        _selectedStartSectionPosition = string.Empty;
            //        _selectedEndSectionPosition = string.Empty;
            //        _selectedCollectType = string.Empty;
            //        _selectedStartSampleFlag = string.Empty;
            //        _selectedEndSampleFlag = string.Empty;

            //        btnZoomOut.IsEnabled = true;
            //        btnZoomIn.IsEnabled = true;
            //        btnRefresh.IsEnabled = true;
            //        return;
            //    }
            //}
        }
        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid == null) return;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.IsReadOnly == true)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }

                        if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DEL_FLAG"), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }

                    }
                }
            }));
        }
        #endregion TAG_SETION 영역
        #region TAG_SECTION_SINGLE 영역

        private void DrawDefectTagSectionSingle(DataTable dt)
        {
            C1Chart c1Chart = chartOutput;

            for (int i = c1Chart.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = c1Chart.Data.Children[i];
                if (dataSeries.Tag.GetString().Equals("TAG_SECTION_SINGLE"))
                {
                    c1Chart.Data.Children.Remove(dataSeries);
                }
            }

            dt.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });//데이터 원천이 코터는 불량데이터, 롤프레스는 측정 데이터라 컬럼 틀림
            dt.Columns.Add(new DataColumn() { ColumnName = "BORDERTAG", DataType = typeof(string) });

            for (int x = 0; x < 2; x++)// Start, End 이미지 두번의 표현으로 for문 사용
            {
                foreach (DataRow row in dt.Rows)//전체 데이터 대상중에서, NFF 2D 바코드 정보가 있는 경우에 추가적인 정보 변경작업
                {
                    if (x == 0)
                    {
                        row["SOURCE_Y_PSTN"] = 105;
                        row["TAGNAME"] = "S";
                    }
                    else
                    {
                        row["SOURCE_Y_PSTN"] = 100;
                        row["TAGNAME"] = "E";
                    }
                    row["BORDERTAG"] = row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["TAG_AUTO_FLAG"].GetString();

                    row["TAG"] = $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}"
                               + ";"
                               + $"{row["ADJ_END_PSTN"]:###,###,###,##0.##}"
                               + ";"
                               + row["CLCT_SEQNO"].GetString()
                               + ";"
                               + row["ROLLMAP_CLCT_TYPE"].GetString();

                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString()
                                    + "["
                                    + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m"
                                    + " ~ "
                                    + $"{row["ADJ_END_PSTN"]:###,###,###,##0.##}" + "m"
                                    + ", "
                                    + row["TAG_AUTO_FLAG_NAME"].GetString()
                                    + " ]";

                    if (row.Table.Columns.Contains("DFCT_NAME"))
                    {
                        row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString()
                                    + "["
                                    + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m"
                                    + " ~ "
                                    + $"{row["ADJ_END_PSTN"]:###,###,###,##0.##}" + "m"
                                    + ", "
                                    + row["TAG_AUTO_FLAG_NAME"].GetString()
                                    + ", "
                                    + row["DFCT_NAME"].GetString()
                                    + " ]";
                    }

                }

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dt);
                ds.XValueBinding = (x == 0) ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");

                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartSectionTagSingle"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Tag = "TAG_SECTION_SINGLE";

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                c1Chart.Data.Children.Add(ds);
            }
            EndUpdateChart();
        }
        private void btnAddSingle_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagSectionSingle;


            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
            if (dt == null || dt.Columns.Count == 0)
            {
                for (int col = 0; col < dg.Columns.Count; col++)
                {
                    dt.Columns.Add(dg.Columns[col].Name.ToString(), typeof(string));
                }
            }

            DataRow newrow = dt.NewRow();
            newrow["CHK"] = true;
            newrow["ADJ_LOTID"] = wParams.lotId;
            newrow["ADJ_WIPSEQ"] = wParams.wipSeq;
            newrow["EQPT_MEASR_PSTN_ID"] = "TAG_SECTION_SINGLE";
            newrow["ROLLMAP_CLCT_TYPE"] = "";   //콤보? 나중에 봐서..
            newrow["LOTID"] = wParams.lotId;
            newrow["WIPSEQ"] = wParams.wipSeq;
            newrow["OLD_ADJ_END_PSTN"] = 0;
            newrow["METHOD"] = "N";
            newrow["LEVEL"] = _strSectionDefect;
            //OLD_ADJ_END_PSTN
            dt.Rows.Add(newrow);

            Util.GridSetData(dg, dt, null);

            dg.EndEdit();
            dg.EndEditRow(true);

        }
        private void btnSaveSingle_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagSectionSingle;
            dg.EndEdit();
            dg.EndEditRow(true);

            if (ValidationSaveSingle() == false) return;

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            //inTable.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    newRollRow["ADJ_END_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();
                    newRollRow["METHOD"] = DataTableConverter.GetValue(row.DataItem, "METHOD").GetString();
                    newRollRow["RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString();
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    IsUpdated = true;
                    SelectRollMapTagSection();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private void btnDeleteSingle_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagSectionSingle;
            dg.EndEdit();
            dg.EndEditRow(true);

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["METHOD"] = "D";
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            if (rowIndex < 1)
            {
                Util.MessageValidation("SFU1636");
                return;
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                    IsUpdated = true;
                    SelectRollMapTagSection();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private void dgTagSectionSingle_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (drv == null) return;

                C1DataGrid dg = dgTagSectionSingle;

                // 시작위치 종료위치 확인
                if (e.Cell.Column.Name == "ADJ_STRT_PSTN" || e.Cell.Column.Name == "ADJ_END_PSTN")
                {
                    if (DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() > 0
                        && DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() > DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble())
                    {
                        Util.MessageInfo("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return;
                    }

                    double DEFECT_LEN = DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble();
                    drv.SetValue("DEFECT_LEN", DEFECT_LEN);
                }


                if (e.Cell.Column.Name == "ADJ_STRT_PSTN" || e.Cell.Column.Name == "ADJ_END_PSTN" || e.Cell.Column.Name == "RESNCODE" || e.Cell.Column.Name == "ROLLMAP_CLCT_TYPE")
                {
                    string METHOD = "";

                    if (DataTableConverter.GetValue(drv, "METHOD").GetString() == "N")
                    {
                        METHOD = "N";
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_STRT_PSTN").GetDouble()) > 0)
                    {
                        METHOD = "U";
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_END_PSTN").GetDouble()) > 0)
                    {
                        METHOD = "U";
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "RESNCODE").GetString(), DataTableConverter.GetValue(drv, "OLD_RESNCODE").GetString()))
                    {
                        METHOD = "U";
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "ROLLMAP_CLCT_TYPE").GetString(), DataTableConverter.GetValue(drv, "OLD_ROLLMAP_CLCT_TYPE").GetString()))
                    {
                        METHOD = "U";
                    }
                    else
                    {
                        METHOD = "";
                    }

                    DataTableConverter.SetValue(drv, "METHOD", METHOD);
                    if (METHOD != "")
                    {
                        DataTableConverter.SetValue(drv, "CHK", true);
                    }

                    if (e.Cell.Column.Name == "RESNCODE" && DataTableConverter.GetValue(drv, "METHOD").GetString() == "N")
                    {
                        DataTable dtResult = GetEqptDefectCode(DataTableConverter.GetValue(drv, "EQPT_MEASR_PSTN_ID").GetString(), DataTableConverter.GetValue(drv, "RESNCODE").GetString());
                        if (CommonVerify.HasTableRow(dtResult) == true)
                        {
                            drv.SetValue("ROLLMAP_CLCT_TYPE", dtResult.Rows[0]["EQPT_INSP_DFCT_CODE"].ToString());
                        }
                    }
                }
                dg.EndEdit();
                dg.EndEditRow(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void CHK_Checked(object sender, RoutedEventArgs e)
        {
            if (_isSearchMode) return;
            C1DataGrid dg = dgTagSectionSingle;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagSectionSingle;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private bool ValidationSaveSingle()
        {
            int selectedRowIndex = 0;

            C1DataGrid dg = dgTagSectionSingle;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;

                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    selectedRowIndex++;

                    if (DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal() > DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal())
                    {
                        Util.MessageValidation("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return false;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString()))
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("활동사유"));
                        return false;
                    }

                    //if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString()))
                    //{
                    //    Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("수집유형"));
                    //    return false;
                    //}
                }
            }

            if (selectedRowIndex < 1)
            {
                // 변경된 데이터가 없습니다.
                Util.MessageValidation("SFU1566");
                return false;
            }

            return true;
        }
        private void dgTagSectionSingle_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid == null) return;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.IsReadOnly == true)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }

                        if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DEL_FLAG"), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }
                    }
                }
            }));
        }
        private void dgTagSectionSingle_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (string.Equals(DataTableConverter.GetValue(drv, "DEL_FLAG"), "Y"))
            {
                e.Cancel = true;
            }
        }
        #endregion TAG_SECTION_SINGLE 영역

        #region OUT_SIDE 영역
        private void CHK_OUTSIDE_Checked(object sender, RoutedEventArgs e)
        {
            if (_isSearchMode) return;
            C1DataGrid dg = dgOutsideScrap;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void CHK_OUTSIDE_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgOutsideScrap;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void btnAddOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgOutsideScrap;


            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
            if (dt == null || dt.Columns.Count == 0)
            {
                for (int col = 0; col < dg.Columns.Count; col++)
                {
                    dt.Columns.Add(dg.Columns[col].Name.ToString(), typeof(string));
                }
            }

            DataRow newrow = dt.NewRow();
            newrow["CHK"] = true;
            newrow["ADJ_LOTID"] = wParams.lotId;
            newrow["ADJ_WIPSEQ"] = wParams.wipSeq;
            newrow["EQPT_CLCT_SEQNO"] = 0;
            //newrow["ROLLMAP_CLCT_TYPE"] = "";   //콤보? 나중에 봐서..
            newrow["LOTID"] = wParams.lotId;
            newrow["WIPSEQ"] = wParams.wipSeq;
            newrow["DEFECT_LEN"] = 0;
            newrow["METHOD"] = "N";
            //OLD_ADJ_END_PSTN
            dt.Rows.Add(newrow);

            Util.GridSetData(dg, dt, null);

            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void btnSaveOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgOutsideScrap;
            dg.EndEdit();
            dg.EndEditRow(true);

            if (ValidationSaveOutsideScrap() == false) return;

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("DEFECT_LEN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드
            inRollmapTable.Columns.Add("TAG_AUTO_FLAG", typeof(string));
            inRollmapTable.Columns.Add("EQPT_CLCT_SEQNO", typeof(int));

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            long MAX_SEQ = 0;
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                var query = _dtOutsideEqptMeasrPstnID.AsEnumerable().Where(o => o.Field<string>("CBO_CODE").Equals(DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString())).ToList();
                DataTable dtCheckOutSide = query.Any() ? query.CopyToDataTable() : _dtOutsideEqptMeasrPstnID.Clone();
                if (CommonVerify.HasTableRow(dtCheckOutSide))
                {
                    long EQPT_CLCT_SEQNO = DataTableConverter.GetValue(row.DataItem, "EQPT_CLCT_SEQNO").GetInt();
                    if (MAX_SEQ < EQPT_CLCT_SEQNO) MAX_SEQ = EQPT_CLCT_SEQNO;
                }
            }

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["DEFECT_LEN"] = DataTableConverter.GetValue(row.DataItem, "DEFECT_LEN").GetDecimal();
                    newRollRow["METHOD"] = DataTableConverter.GetValue(row.DataItem, "METHOD").GetString();
                    if (DataTableConverter.GetValue(row.DataItem, "DEFECT_LEN").GetDecimal() == 0)
                    {
                        newRollRow["METHOD"] = "D";
                    }
                    newRollRow["RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString();
                    newRollRow["TAG_AUTO_FLAG"] = "N";


                    var query = _dtOutsideClctType.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals(newRollRow["EQPT_MEASR_PSTN_ID"].GetString())).ToList();
                    DataTable dtClctType = query.Any() ? query.CopyToDataTable() : _dtOutsideClctType.Clone();
                    if (CommonVerify.HasTableRow(dtClctType) == true && DataTableConverter.GetValue(row.DataItem, "METHOD").GetString() == "N")
                    {
                        MAX_SEQ = MAX_SEQ + 1;
                        newRollRow["EQPT_CLCT_SEQNO"] = MAX_SEQ;
                    }

                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    IsUpdated = true;
                    GetRollMap();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private bool ValidationSaveOutsideScrap()
        {
            int selectedRowIndex = 0;

            C1DataGrid dg = dgOutsideScrap;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    selectedRowIndex++;

                    if (DataTableConverter.GetValue(row.DataItem, "DEFECT_LEN").GetDecimal() < 0)
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("폐기길이"));
                        return false;
                    }
                    else if (DataTableConverter.GetValue(row.DataItem, "METHOD").GetString() == "N" && DataTableConverter.GetValue(row.DataItem, "DEFECT_LEN").GetDouble() == 0)
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("폐기길이"));
                        return false;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString()))
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("활동사유"));
                        return false;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString()))
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("불량명"));
                        return false;
                    }
                }
            }

            if (selectedRowIndex < 1)
            {
                // 변경된 데이터가 없습니다.
                Util.MessageValidation("SFU1566");
                return false;
            }

            return true;
        }
        private void btnDelOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgOutsideScrap;
            dg.EndEdit();
            dg.EndEditRow(true);

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["METHOD"] = "D";
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            if (rowIndex < 1)
            {
                Util.MessageValidation("SFU1636");
                return;
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                    IsUpdated = true;
                    GetRollMap();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private void dgOutsideScrap_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (drv == null) return;

                C1DataGrid dg = dgOutsideScrap;

                // 시작위치 종료위치 확인
                if (e.Cell.Column.Name == "DEFECT_LEN")
                {
                    if (DataTableConverter.GetValue(drv, "DEFECT_LEN").GetDouble() < 0)
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("폐기길이"));
                        return;
                    }
                }


                if (e.Cell.Column.Name == "DEFECT_LEN" || e.Cell.Column.Name == "RESNCODE" || e.Cell.Column.Name == "ROLLMAP_CLCT_TYPE" || e.Cell.Column.Name == "LEVEL")
                {
                    string METHOD = "";

                    if (DataTableConverter.GetValue(drv, "METHOD").GetString() == "N")
                    {
                        METHOD = "N";
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "DEFECT_LEN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_DEFECT_LEN").GetDouble()) > 0)
                    {
                        METHOD = "U";
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "RESNCODE").GetString(), DataTableConverter.GetValue(drv, "OLD_RESNCODE").GetString()))
                    {
                        METHOD = "U";
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "ROLLMAP_CLCT_TYPE").GetString(), DataTableConverter.GetValue(drv, "OLD_ROLLMAP_CLCT_TYPE").GetString()))
                    {
                        METHOD = "U";
                    }
                    else
                    {
                        METHOD = "";
                    }

                    DataTableConverter.SetValue(drv, "METHOD", METHOD);
                    if (METHOD != "")
                    {
                        DataTableConverter.SetValue(drv, "CHK", true);
                    }

                    if (e.Cell.Column.Name == "RESNCODE" && DataTableConverter.GetValue(drv, "METHOD").GetString() == "N")
                    {
                        string strEqptMeasrPstnID = "";

                        var query = _dtOutsideComCode.AsEnumerable().Where(o => o.Field<string>("ATTR3").Equals(DataTableConverter.GetValue(drv, "LEVEL").GetString())).ToList();
                        DataTable dtData = query.Any() ? query.CopyToDataTable() : _dtOutsideComCode.Clone();
                        if (CommonVerify.HasTableRow(dtData) == true)
                        {
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                if (strEqptMeasrPstnID == "") strEqptMeasrPstnID = dtData.Rows[i]["COM_CODE"].ToString();
                                else strEqptMeasrPstnID = strEqptMeasrPstnID + "," + dtData.Rows[i]["COM_CODE"].ToString();
                            }
                        }


                        var query2 = _dtEtcLossComCode.AsEnumerable().Where(o => o.Field<string>("ATTR3").Equals(DataTableConverter.GetValue(drv, "LEVEL").GetString())).ToList();
                        DataTable dtData2 = query2.Any() ? query2.CopyToDataTable() : _dtEtcLossComCode.Clone();
                        if (CommonVerify.HasTableRow(dtData2) == true)
                        {
                            for (int i = 0; i < dtData2.Rows.Count; i++)
                            {
                                if (strEqptMeasrPstnID == "") strEqptMeasrPstnID = dtData2.Rows[i]["COM_CODE"].ToString();
                                else strEqptMeasrPstnID = strEqptMeasrPstnID + "," + dtData2.Rows[i]["COM_CODE"].ToString();
                            }
                        }


                        DataTable dtResult = GetEqptDefectCode(strEqptMeasrPstnID, DataTableConverter.GetValue(drv, "RESNCODE").GetString());
                        if (CommonVerify.HasTableRow(dtResult) == true)
                        {
                            drv.SetValue("EQPT_MEASR_PSTN_ID", dtResult.Rows[0]["EQPT_MEASR_PSTN_ID"].ToString());
                            drv.SetValue("ROLLMAP_CLCT_TYPE", dtResult.Rows[0]["EQPT_INSP_DFCT_CODE"].ToString());
                        }
                        else
                        {
                            var queryRS = _dtOutsideComCode.AsEnumerable().Where(o => o.Field<string>("ATTR1").Equals(DataTableConverter.GetValue(drv, "RESNCODE").GetString())).ToList();
                            DataTable dtRS = queryRS.Any() ? queryRS.CopyToDataTable() : _dtOutsideComCode.Clone();
                            if (CommonVerify.HasTableRow(dtRS) == true)
                            {
                                drv.SetValue("EQPT_MEASR_PSTN_ID", dtRS.Rows[0]["COM_CODE"].ToString());
                                drv.SetValue("ROLLMAP_CLCT_TYPE", "");
                            }
                        }
                    }
                }
                dg.EndEdit();
                dg.EndEditRow(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgOutsideScrap_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid == null) return;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.IsReadOnly == true || e.Cell.Column.Name == "EQPT_MEASR_PSTN_ID" || e.Cell.Column.Name == "LEVEL")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }
                    }
                }
            }));
        }
        private void dgOutsideScrap_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid == null) return;
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (e.Column.Name == "EQPT_MEASR_PSTN_ID" && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "METHOD"), "N")) e.Cancel = true;
            else if (e.Column.Name == "LEVEL" && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "METHOD"), "N")) e.Cancel = true;

            if (e.Column.Name == "RESNCODE")
            {
                if (string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "LEVEL").GetString()))
                {
                    e.Cancel = true;
                    Util.MessageInfo("SFU10009");
                }
            }
        }
        private void dgOutsideScrap_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (Convert.ToString(e.Column.Name) == "ROLLMAP_CLCT_TYPE")
            {
                C1ComboBox cbo = e.EditingElement as C1ComboBox;
                DataRowView drv = e.Row.DataItem as DataRowView;
                C1DataGrid dg = dgOutsideScrap;
                if (cbo != null)
                {
                    var query = _dtOutsideEtcClctType.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals(DataTableConverter.GetValue(drv, "EQPT_MEASR_PSTN_ID").GetString())).ToList();
                    DataTable dtClctType = query.Any() ? query.CopyToDataTable() : _dtOutsideEtcClctType.Clone();
                    if (CommonVerify.HasTableRow(dtClctType))
                    {
                        string selectedValue = drv[e.Column.Name].GetString();

                        cbo.ItemsSource = null;
                        cbo.DisplayMemberPath = dg.Columns["ROLLMAP_CLCT_TYPE"].DisplayMemberPath();
                        cbo.SelectedValuePath = dg.Columns["ROLLMAP_CLCT_TYPE"].SelectedValuePath();
                        cbo.ItemsSource = dtClctType.Copy().AsDataView();

                        cbo.SelectedValue = selectedValue;
                        if (cbo.SelectedIndex < 0)
                            cbo.SelectedIndex = 0;
                    }
                }
            }
            else if (Convert.ToString(e.Column.Name) == "EQPT_MEASR_PSTN_ID")
            {
                C1ComboBox cbo = e.EditingElement as C1ComboBox;
                DataRowView drv = e.Row.DataItem as DataRowView;
                C1DataGrid dg = dgOutsideScrap;
                if (cbo != null)
                {
                    var query = _dtOutsideComCode.AsEnumerable().Where(o => o.Field<string>("ATTR3").Equals(DataTableConverter.GetValue(drv, "LEVEL").GetString())).ToList();
                    DataTable dtData = query.Any() ? query.CopyToDataTable() : _dtOutsideComCode.Clone();
                    if (CommonVerify.HasTableRow(dtData) == true)
                    {
                        string selectedValue = drv[e.Column.Name].GetString();

                        cbo.ItemsSource = null;
                        cbo.DisplayMemberPath = dg.Columns["EQPT_MEASR_PSTN_ID"].DisplayMemberPath();
                        cbo.SelectedValuePath = dg.Columns["EQPT_MEASR_PSTN_ID"].SelectedValuePath();
                        cbo.ItemsSource = _dtOutsideEqptMeasrPstnID.Copy().AsDataView();

                        cbo.SelectedValue = selectedValue;
                        if (cbo.SelectedIndex < 0)
                            cbo.SelectedIndex = 0;
                    }
                    else
                    {
                        string selectedValue = drv[e.Column.Name].GetString();

                        cbo.ItemsSource = null;
                        cbo.DisplayMemberPath = dg.Columns["EQPT_MEASR_PSTN_ID"].DisplayMemberPath();
                        cbo.SelectedValuePath = dg.Columns["EQPT_MEASR_PSTN_ID"].SelectedValuePath();
                        cbo.ItemsSource = _dtEtcLossEqptMeasrPstnID.Copy().AsDataView();

                        cbo.SelectedValue = selectedValue;
                        if (cbo.SelectedIndex < 0)
                            cbo.SelectedIndex = 0;
                    }
                }
            }
            else if (Convert.ToString(e.Column.Name) == "RESNCODE")
            {
                C1ComboBox cbo = e.EditingElement as C1ComboBox;
                DataRowView drv = e.Row.DataItem as DataRowView;
                C1DataGrid dg = dgOutsideScrap;
                if (cbo != null)
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "LEVEL").GetString())) return;

                    string strLevel = DataTableConverter.GetValue(drv, "LEVEL").GetString();

                    DataTable dtLevelReason = _dsLevelReasonCode.Tables[strLevel].Copy();

                    if (CommonVerify.HasTableRow(dtLevelReason) == false) return;

                    List<string> strList = new List<string>();
                    for (int i = 0; i < dtLevelReason.Rows.Count; i++)
                    {
                        string strData = dtLevelReason.Rows[i]["RESNCODE"].ToString();
                        strList.Insert(i, strData);
                    }
                    var query = _dtReason.AsEnumerable().Where(o => strList.Contains(o.Field<string>("RESNCODE"))).ToList();

                    //var query = (from t in _dtReason.AsEnumerable()
                    //                     join x in dtLevelReason.AsEnumerable()
                    //                       on t.Field<string>("RESNCODE") equals x.Field<string>("RESNCODE")
                    //                     select t
                    //                     ).ToList();

                    DataTable dtResn = query.Any() ? query.CopyToDataTable() : _dtReason.Clone();
                    if (CommonVerify.HasTableRow(dtResn))
                    {
                        string selectedValue = drv[e.Column.Name].GetString();

                        cbo.ItemsSource = null;
                        cbo.DisplayMemberPath = dg.Columns["RESNCODE"].DisplayMemberPath();
                        cbo.SelectedValuePath = dg.Columns["RESNCODE"].SelectedValuePath();
                        cbo.ItemsSource = dtResn.Copy().AsDataView();

                        cbo.SelectedValue = selectedValue;
                        if (cbo.SelectedIndex < 0)
                            cbo.SelectedIndex = 0;
                    }
                }
            }

            //_dsLevelReasonCode
        }
        #endregion OUT_SIDE 영역

        #region MES TAG 불량영역
        private void CHK_MES_Checked(object sender, RoutedEventArgs e)
        {
            if (_isSearchMode) return;
            C1DataGrid dg = dgTagDefectMes;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void CHK_MES_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagDefectMes;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void btnAddMes_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagDefectMes;


            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
            if (dt == null || dt.Columns.Count == 0)
            {
                for (int col = 0; col < dg.Columns.Count; col++)
                {
                    dt.Columns.Add(dg.Columns[col].Name.ToString(), typeof(string));
                }
            }

            DataRow newrow = dt.NewRow();
            newrow["CHK"] = true;
            //newrow["ADJ_LOTID"] = wParams.lotId;
            //newrow["ADJ_WIPSEQ"] = wParams.wipSeq;
            newrow["LOTID"] = wParams.lotId;
            newrow["WIPSEQ"] = wParams.wipSeq;
            newrow["DEFECT_LEN"] = 0;
            newrow["METHOD"] = "N";
            //OLD_ADJ_END_PSTN
            dt.Rows.Add(newrow);

            Util.GridSetData(dg, dt, null);

            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void btnSaveMes_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagDefectMes;
            dg.EndEdit();
            dg.EndEditRow(true);

            if (ValidationSaveMes() == false) return;

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("DEFECT_CNT", typeof(decimal));
            inRollmapTable.Columns.Add("DEFECT_LEN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드
            inRollmapTable.Columns.Add("TAG_AUTO_FLAG", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = wParams.lotId;
                    newRollRow["WIPSEQ"] = wParams.wipSeq;
                    newRollRow["EQPT_MEASR_PSTN_ID"] = "TAG_DEFECT_MES";
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["DEFECT_CNT"] = DataTableConverter.GetValue(row.DataItem, "DEFECT_CNT").GetDecimal();
                    newRollRow["DEFECT_LEN"] = DataTableConverter.GetValue(row.DataItem, "DEFECT_LEN").GetDecimal();
                    newRollRow["METHOD"] = DataTableConverter.GetValue(row.DataItem, "METHOD").GetString();
                    newRollRow["RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString();
                    newRollRow["TAG_AUTO_FLAG"] = "N";

                    if (DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt() == 0)
                    {
                        newRollRow["METHOD"] = "N";
                    }

                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    IsUpdated = true;
                    GetRollMapDefectList();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private bool ValidationSaveMes()
        {
            //int selectedRowIndex = 0;

            //C1DataGrid dg = dgEtcLoss;

            //foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            //{
            //    if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
            //        || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
            //    {
            //        selectedRowIndex++;

            //        if (DataTableConverter.GetValue(row.DataItem, "DEFECT_LEN").GetDecimal() <= 0)
            //        {
            //            Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("폐기길이"));
            //            return false;
            //        }

            //        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString()))
            //        {
            //            Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("활동사유"));
            //            return false;
            //        }

            //        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString()))
            //        {
            //            Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("수집유형"));
            //            return false;
            //        }

            //        if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString()))
            //        {
            //            Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("불량명"));
            //            return false;
            //        }
            //    }
            //}

            //if (selectedRowIndex < 1)
            //{
            //    // 변경된 데이터가 없습니다.
            //    Util.MessageValidation("SFU1566");
            //    return false;
            //}

            return true;
        }
        private void btnDeleteMes_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgTagDefectMes;
            dg.EndEdit();
            dg.EndEditRow(true);

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["METHOD"] = "D";
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            if (rowIndex < 1)
            {
                Util.MessageValidation("SFU1636");
                return;
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                    IsUpdated = true;
                    GetRollMapDefectList();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private void dgTagDefectMes_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (drv == null) return;

                C1DataGrid dg = dgTagDefectMes;

                if (e.Cell.Column.Name == "DEFECT_CNT")
                {
                    //여기서 길이 계산
                    double nCnt = DataTableConverter.GetValue(drv, "DEFECT_CNT").GetDouble();
                    double nEachLen = DataTableConverter.GetValue(drv, "EACH_LEN").GetDouble();

                    DataTableConverter.SetValue(drv, "DEFECT_LEN", nCnt * nEachLen);
                }


                if (e.Cell.Column.Name == "DEFECT_LEN" || e.Cell.Column.Name == "DEFECT_CNT")
                {
                    string METHOD = "";

                    if (DataTableConverter.GetValue(drv, "METHOD").GetString() == "N")
                    {
                        METHOD = "N";
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "DEFECT_LEN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_DEFECT_LEN").GetDouble()) > 0)
                    {
                        METHOD = "U";
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "DEFECT_CNT").GetDouble() - DataTableConverter.GetValue(drv, "OLD_DEFECT_CNT").GetDouble()) > 0)
                    {
                        METHOD = "U";
                    }
                    else
                    {
                        METHOD = "";
                    }

                    DataTableConverter.SetValue(drv, "METHOD", METHOD);
                    if (METHOD != "")
                    {
                        DataTableConverter.SetValue(drv, "CHK", true);
                    }
                }
                dg.EndEdit();
                dg.EndEditRow(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgTagDefectMes_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid == null) return;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.IsReadOnly == true)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }
                    }
                }
            }));
        }
        private void dgTagDefectMes_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid == null) return;

            //if (e.Column.Name == "EQPT_MEASR_PSTN_ID" && !string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "METHOD"), "N")) e.Cancel = true;
        }
        private void dgTagDefectMes_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }


        #endregion MES TAG 불량 영역

        #region TEST CUT 영역

        private void btnRollMapTestCut_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageValidation("SUF9024", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    btnSaveTestCut_NonArea.IsEnabled = true;
                }
            }, txtLot.Text);
        }

        private void btnSaveTestCut_NonArea_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTestCut_NonArea.Rows.Count <= 0) return;

                C1DataGrid dg = dgTestCut_NonArea;

                DataSet ds = new DataSet();

                DataTable dtData = new DataTable("INDATA");
                dtData.Columns.Add("EQPTID", typeof(string));
                dtData.Columns.Add("TOP_BACK_REVS_FLAG", typeof(string));
                dtData.Columns.Add("WIPSEQ", typeof(string));
                dtData.Columns.Add("CLCT_SEQNO", typeof(string));
                dtData.Columns.Add("ROLLMAP_ADJ_INFO_TYPE_CODE", typeof(string));
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("END_PSTN1", typeof(double));
                dtData.Columns.Add("ADJ_QTY1", typeof(double));
                dtData.Columns.Add("ADJ_ATTR2_VALUE", typeof(string));
                dtData.Columns.Add("USERID", typeof(string));
                dtData.Columns.Add("DEL_FLAG", typeof(string));
                dtData.Columns.Add("INPUT_LOTID", typeof(string));

                string ROLLMAP_ADJ_INFO_TYPE_CODE = "TEST_CUT_NON_AREA";
                string INPUT_LOTID = "TEST_CUT";
                string TOP_BACK_REVS_FLAG = "N";

                if (rdoTCApplyY_FPI.IsChecked == true)
                {
                    INPUT_LOTID = "FPI";
                    TOP_BACK_REVS_FLAG = "Y";
                }
                else if (rdoTCApplyN_FPI.IsChecked == true)
                {
                    INPUT_LOTID = "FPI";
                    TOP_BACK_REVS_FLAG = "N";
                }
                else if (rdoTCApplyY_TESTCUT.IsChecked == true)
                {
                    TOP_BACK_REVS_FLAG = "Y";
                }
                else
                {
                    TOP_BACK_REVS_FLAG = "N";
                }

                if (TOP_BACK_REVS_FLAG == "Y")
                {
                    if (cbTestCutLoss.SelectedIndex < 0 || cbTestCutLoss.SelectedValue.ToString() == "SELECT")
                    {
                        Util.MessageValidation("SFU8393", "LOSS");
                        return;
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    DataRow dr = dtData.NewRow();
                    dr["EQPTID"] = wParams.equipmentCode;
                    dr["TOP_BACK_REVS_FLAG"] = TOP_BACK_REVS_FLAG;
                    dr["INPUT_LOTID"] = INPUT_LOTID;
                    dr["WIPSEQ"] = wParams.wipSeq;
                    dr["CLCT_SEQNO"] = 1;
                    dr["ROLLMAP_ADJ_INFO_TYPE_CODE"] = ROLLMAP_ADJ_INFO_TYPE_CODE;



                    double LOSS_QTY = DataTableConverter.GetValue(row.DataItem, "TOTAL_QTY").GetDouble() - txtGoodQty.Value;
                    if (LOSS_QTY < 0) LOSS_QTY = 0;

                    dr["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    dr["END_PSTN1"] = LOSS_QTY;

                    dr["ADJ_QTY1"] = txtGoodQty.Value;
                    dr["ADJ_ATTR2_VALUE"] = Util.GetCondition(cbTestCutLoss);
                    dr["USERID"] = LoginInfo.USERID;
                    dr["DEL_FLAG"] = "N";
                    dtData.Rows.Add(dr);
                }

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("BR_PRD_REG_RM_ADJ_TEST_CUT_NON_AREA", "INDATA", null, dtData, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다

                        IsUpdated = true;
                        SetTestCut_NonArea();
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

        private void SetTestCut_NonArea()
        {
            //기본값 세팅
            {
                txtGoodQty.IsEnabled = false;
                DataTable dtResult = GetCommoncodeUse("TEST_CUT_NON_AREA_FPI_RP", LoginInfo.CFG_AREA_ID);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow dr = dtResult.Rows[0];
                    cbTestCutLoss.SelectedValue = dr["ATTRIBUTE4"].ToString();
                    txtGoodQty.Value = dr["ATTRIBUTE1"].SafeToDouble();
                    _defaltFPIQty = txtGoodQty.Value;
                    if(dr["ATTRIBUTE2"].ToString() == "N")
                    {
                        _EnableChangeFPIQty = "N";
                    }
                }

                if (_bUseFPI == true)
                {
                    rdoTCApplyY_FPI.IsChecked = true;
                    if (_EnableChangeFPIQty == "Y") txtGoodQty.IsEnabled = true;
                }
                else
                {
                    rdoTCApplyY_TESTCUT.IsChecked = true;
                    txtGoodQty.Value = 0;
                }
            }
            // TEST CUT DATA
            {
                DataTable dtTotal = SelectTestCutData();
                if (dtTotal == null) return;
                if (dtTotal.Rows.Count == 0) return;

                foreach (DataRow row in dtTotal.Rows)
                {
                    if (row["LOSS_APPLY_YN"].ToString() == "Y")
                    {

                    }
                    else
                    {
                        row["ADJ_QTY1"] = 0;
                        row["END_PSTN1"] = 0;
                    }
                }

                Util.GridSetData(dgTestCut_NonArea, dtTotal, FrameOperation);

                //대표값
                DataRow dr = dtTotal.Rows[0];

                if (string.IsNullOrEmpty(dr["ADJ_ATTR2_VALUE"].ToString()) == false)
                {
                    cbTestCutLoss.SelectedValue = dr["ADJ_ATTR2_VALUE"].ToString();
                }

                string INPUT_LOTID = dr["INPUT_LOTID"].ToString();

                if (string.IsNullOrEmpty(INPUT_LOTID) == false)
                {
                    if (INPUT_LOTID == "FPI")   //FPI
                    {
                        if (dr["LOSS_APPLY_YN"].ToString() == "Y") rdoTCApplyY_FPI.IsChecked = true;
                        else rdoTCApplyN_FPI.IsChecked = true;

                        txtGoodQty.Value = dr["ADJ_QTY1"].SafeToDouble();
                    }
                    else
                    {
                        if (dr["LOSS_APPLY_YN"].ToString() == "Y") rdoTCApplyY_TESTCUT.IsChecked = true;
                        else rdoTCApplyN_TESTCUT.IsChecked = true;
                    }
                }
            }
        }

        private DataTable SelectTestCutData()
        {
            try
            {
                DataTable dt = new DataTable();
                DataRow dr = dt.NewRow();
                dt.Columns.Add("WIPSEQ", typeof(string));
                dr["WIPSEQ"] = wParams.wipSeq;
                dt.Columns.Add("LOTID", typeof(string));
                dr["LOTID"] = wParams.lotId;

                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_TEST_CUT_DATA", "RQSTDT", "RSLTDT", dt);

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion TEST CUT 영역

        #region SCRAP SECTION 영역

        //조성근 2025.04.04 ScrapSection 적용 - start
        private void DrawScrapSectionOutlot(DataTable dtScrap, ChartConfigurationType chartConfigurationType)
        {
            //DataRow[] drPetScrap = dt.Select("EQPT_MEASR_PSTN_ID = 'PET' OR EQPT_MEASR_PSTN_ID = 'SCRAP' ");
            // 롤프레스 공정에서만 PET, SCRAP 호출 함.
            if (chartConfigurationType == ChartConfigurationType.Input) return;

            if (rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked)    //상대좌표
            {
                double lowerExtent = 0;
                double upperExtent = 100;

                DataTable dt = new DataTable();
                dt.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                dt.Columns.Add("RAW_STRT_PSTN", typeof(double));
                dt.Columns.Add("RAW_END_PSTN", typeof(double));
                dt.Columns.Add("Y_PSTN", typeof(double));
                dt.Columns.Add("COLORMAP", typeof(string));
                dt.Columns.Add("TOOLTIP", typeof(string));
                dt.Columns.Add("TAG", typeof(string));

                foreach (DataRow row in dtScrap.Rows)
                {
                    //var convertFromString = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD"));
                    //string colorMap = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? "#000000" : "#BDBDBD";

                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));
                    string colorMap = row["COLORMAP"].GetString();

                    string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] " + ObjectDic.Instance.GetObjectName("위치") + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m " + ObjectDic.Instance.GetObjectName("길이") + $"{row["WND_LEN"]:###,###,###,##0.##}" + "m";
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["ADJ_STRT_PSTN"].GetString() + ";" + row["ADJ_END_PSTN"].GetString()
                          + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                    chartOutput.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_STRT_PSTN"], row["ADJ_STRT_PSTN"] },
                        ValuesSource = new double[] { lowerExtent, upperExtent },
                        ConnectionStroke = convertFromString,
                        ConnectionStrokeThickness = 3,
                        ConnectionFill = convertFromString,
                    });

                    dt.Rows.Add(row["EQPT_MEASR_PSTN_ID"], row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"], 97, colorMap, content, tag);
                }

                if (CommonVerify.HasTableRow(dt))
                {
                    XYDataSeries dsPetScrap = new XYDataSeries();
                    dsPetScrap.ItemsSource = DataTableConverter.Convert(dt);
                    dsPetScrap.XValueBinding = new Binding("RAW_STRT_PSTN");
                    dsPetScrap.ValueBinding = new Binding("Y_PSTN");
                    dsPetScrap.ChartType = ChartType.XYPlot;
                    dsPetScrap.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    dsPetScrap.SymbolFill = new SolidColorBrush(Colors.Transparent);
                    dsPetScrap.PointLabelTemplate = grdMain.Resources["chartPetScrap"] as DataTemplate;
                    dsPetScrap.Margin = new Thickness(0, 0, 0, 0);

                    dsPetScrap.PlotElementLoaded += (s, e) =>
                    {
                        PlotElement pe = (PlotElement)s;
                        pe.Stroke = new SolidColorBrush(Colors.Transparent);
                        pe.Fill = new SolidColorBrush(Colors.Transparent);
                    };

                    chartOutput.Data.Children.Add(dsPetScrap);
                }
            }
            else // 절대좌표
            {
                foreach (DataRow row in dtScrap.Rows)
                {
                    double SOURCE_ADJ_STRT_PSTN = row["SOURCE_ADJ_STRT_PSTN"] == null ? 0 : double.Parse(row["SOURCE_ADJ_STRT_PSTN"].ToString());
                    double SOURCE_ADJ_END_PSTN = row["SOURCE_ADJ_END_PSTN"] == null ? 0 : double.Parse(row["SOURCE_ADJ_END_PSTN"].ToString());
                    string colorMap = row["COLORMAP"].GetString();
                    string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] " + ObjectDic.Instance.GetObjectName("위치") + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m " + ObjectDic.Instance.GetObjectName("길이") + $"{row["WND_LEN"]:###,###,###,##0.##}" + "m";
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["ADJ_STRT_PSTN"].GetString() + ";" + row["ADJ_END_PSTN"].GetString()
                          + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));
                    if (convertFromString == null) convertFromString = Colors.Black;
                    chartOutput.Data.Children.Add(new AlarmZone()
                    {
                        Near = SOURCE_ADJ_STRT_PSTN,
                        Far = SOURCE_ADJ_END_PSTN,
                        LowerExtent = 0,
                        UpperExtent = 100,
                        ConnectionStroke = new SolidColorBrush((Color)convertFromString) { Opacity = 0.5 },
                        ConnectionFill = new SolidColorBrush((Color)convertFromString) { Opacity = 0.5 },
                        //PointLabelTemplate = grdMain.Resources["chartPetScrap"] as DataTemplate, 
                        ToolTip = content,
                        Cursor = Cursors.Hand,
                        Tag = tag
                    });
                }
            }
        }
        //조성근 2025.04.04 ScrapSection 적용 - end
        private void SetUseScrapSection()
        {
            try
            {
                DataTable dtResult = GetEqptClctTypeCombo("SCRAP_SECTION");
                btnScrapSection.Visibility = Visibility.Collapsed;

                if (CommonVerify.HasTableRow(dtResult) == true)
                {
                    tiScrapSection.Visibility = Visibility.Visible;
                }

                if (CommonVerify.HasTableRow(dtResult) == true && _isSearchMode == false)
                {
                    btnScrapSection.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        private void btnScrapSection_Click(object sender, RoutedEventArgs e)
        {
            popScrapSection("");
        }

        private void popScrapSection(string strClctSeqNo = "")
        {
            CMM_RM_SCRAP_SECTION pop = new CMM_RM_SCRAP_SECTION();
            pop.FrameOperation = FrameOperation;

            string strMode = "2";

            if (_isSearchMode == true) strMode = "1";

            object[] parameters = new object[7];
            parameters[0] = wParams.processCode;
            parameters[1] = wParams.equipmentCode;
            parameters[2] = wParams.lotId;
            parameters[3] = wParams.wipSeq;
            parameters[4] = "SCRAP_SECTION";
            parameters[5] = strClctSeqNo;
            parameters[6] = strMode;

            C1WindowExtension.SetParameters(pop, parameters);
            pop.Closed += popScrapSection_Closed;
            Dispatcher.BeginInvoke(new Action(() => pop.ShowModal()));
        }

        private void popScrapSection_Closed(object sender, EventArgs e)
        {
            //return;
            //CMM_ROLLMAP_RW_SCRAP => CMM_RM_RW_SCRAP
            CMM_RM_SCRAP_SECTION popup = sender as CMM_RM_SCRAP_SECTION;
            if (popup != null && popup.IsUpdated)
            {
                IsUpdated = true;
                Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            }
        }

        private void chartOutput_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point curPoint = e.GetPosition(chartOutput.View);
            var chartpoint = chartOutput.View.PointToData(curPoint);

            if (chartpoint == null || chartpoint.X < 0 || chartpoint.Y < 0 || chartpoint.Y > 100)
            {
                return;
            }

            for (int i = chartOutput.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = chartOutput.Data.Children[i];
                string strTag = dataSeries.Tag.GetString();
                string[] splitItem = strTag.Split(';');

                if (splitItem.Count() < 6) continue;

                if (splitItem[0].Equals("SCRAP_SECTION"))
                {
                    AlarmZone az = (AlarmZone)dataSeries;
                    if (chartpoint.X <= az.Far && chartpoint.X >= az.Near)
                    {
                        popScrapSection(splitItem[5]);
                        return;
                    }
                }
            }
        }

        private void chartOutput_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isSearchMode || tiTagSection.Visibility == Visibility.Collapsed) return;

            SetChartOutputContextMenu(true);

            Point curPoint = e.GetPosition(chartOutput.View);
            var chartpoint = chartOutput.View.PointToData(curPoint);

            for (int i = chartOutput.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = chartOutput.Data.Children[i];
                string strTag = dataSeries.Tag.GetString();
                string[] splitItem = strTag.Split(';');

                if (splitItem.Count() < 6) continue;

                if (splitItem[0].Equals("SCRAP_SECTION"))
                {
                    AlarmZone az = (AlarmZone)dataSeries;
                    if (chartpoint.X <= az.Far && chartpoint.X >= az.Near)
                    {
                        SetChartOutputContextMenu(false);
                        return;
                    }
                }
            }
        }

        private void CHK_SCRAPSECTIONChecked(object sender, RoutedEventArgs e)
        {
            if (_isSearchMode) return;
            C1DataGrid dg = dgScrapSection;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void CHK_SCRAPSECTIONUnchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgScrapSection;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void btnAddScrapSection_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgScrapSection;
            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
            if (dt == null || dt.Columns.Count == 0)
            {
                for (int col = 0; col < dg.Columns.Count; col++)
                {
                    dt.Columns.Add(dg.Columns[col].Name.ToString(), typeof(string));
                }
            }

            DataRow newrow = dt.NewRow();
            newrow["CHK"] = true;
            newrow["ADJ_LOTID"] = wParams.lotId;
            newrow["ADJ_WIPSEQ"] = wParams.wipSeq;
            newrow["EQPT_MEASR_PSTN_ID"] = "SCRAP_SECTION";
            newrow["ROLLMAP_CLCT_TYPE"] = "";   //콤보? 나중에 봐서..
            newrow["LOTID"] = wParams.lotId;
            newrow["WIPSEQ"] = wParams.wipSeq;
            newrow["OLD_ADJ_END_PSTN"] = 0;
            newrow["METHOD"] = "N";
            newrow["LEVEL"] = _strScrapSection;
            //OLD_ADJ_END_PSTN
            dt.Rows.Add(newrow);

            Util.GridSetData(dg, dt, null);

            dg.EndEdit();
            dg.EndEditRow(true);
        }
        private void btnSaveScrapSection_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgScrapSection;
            dg.EndEdit();
            dg.EndEditRow(true);

            if (ValidationSaveScrapSection() == false) return;

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            //inTable.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("ADJ_STRT_PSTN2", typeof(decimal));
            inRollmapTable.Columns.Add("ADJ_END_PSTN2", typeof(decimal));
            inRollmapTable.Columns.Add("DEFECT_LEN", typeof(decimal));
            inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["ADJ_STRT_PSTN2"] = DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    newRollRow["ADJ_END_PSTN2"] = DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();
                    newRollRow["DEFECT_LEN"] = DataTableConverter.GetValue(row.DataItem, "DEFECT_LEN").GetDecimal();
                    newRollRow["METHOD"] = DataTableConverter.GetValue(row.DataItem, "METHOD").GetString();
                    newRollRow["RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString();
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    IsUpdated = true;
                    GetRollMap();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private bool ValidationSaveScrapSection()
        {
            int selectedRowIndex = 0;

            C1DataGrid dg = dgScrapSection;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;

                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    selectedRowIndex++;

                    if (DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal() > DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal())
                    {
                        Util.MessageValidation("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return false;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "RESNCODE").GetString()))
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("활동사유"));
                        return false;
                    }

                    //if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString()))
                    //{
                    //    Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("수집유형"));
                    //    return false;
                    //}
                }
            }

            if (selectedRowIndex < 1)
            {
                // 변경된 데이터가 없습니다.
                Util.MessageValidation("SFU1566");
                return false;
            }

            return true;
        }
        private void btnDelScrapSection_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgScrapSection;
            dg.EndEdit();
            dg.EndEditRow(true);

            const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = wParams.equipmentCode;
            newRow["LOTID"] = wParams.lotId;
            newRow["WIPSEQ"] = wParams.wipSeq;
            newRow["USER"] = LoginInfo.USERID;
            inTable.Rows.Add(newRow);

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type != DataGridRowType.Item) continue;
                if (DataTableConverter.GetValue(row.DataItem, "CHK").Equals(true)
                    || DataTableConverter.GetValue(row.DataItem, "CHK").Equals("True"))
                {
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["METHOD"] = "D";
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            if (rowIndex < 1)
            {
                Util.MessageValidation("SFU1636");
                return;
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                    IsUpdated = true;
                    GetRollMap();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }
        private void dgScrapSection_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (drv == null) return;

                C1DataGrid dg = dgScrapSection;
                // 시작위치 종료위치 확인
                if (e.Cell.Column.Name == "ADJ_STRT_PSTN" || e.Cell.Column.Name == "ADJ_END_PSTN")
                {
                    if (DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() > 0
                        && DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() > DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble())
                    {
                        Util.MessageInfo("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return;
                    }

                    double DEFECT_LEN = DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble();
                    drv.SetValue("DEFECT_LEN", DEFECT_LEN);
                }


                if (e.Cell.Column.Name == "ADJ_STRT_PSTN" || e.Cell.Column.Name == "ADJ_END_PSTN" || e.Cell.Column.Name == "RESNCODE" || e.Cell.Column.Name == "ROLLMAP_CLCT_TYPE")
                {
                    string METHOD = "";

                    if (DataTableConverter.GetValue(drv, "METHOD").GetString() == "N")
                    {
                        METHOD = "N";
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_STRT_PSTN").GetDouble()) > 0)
                    {
                        METHOD = "U";
                    }
                    else if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_END_PSTN").GetDouble()) > 0)
                    {
                        METHOD = "U";
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "RESNCODE").GetString(), DataTableConverter.GetValue(drv, "OLD_RESNCODE").GetString()))
                    {
                        METHOD = "U";
                    }
                    else if (!string.Equals(DataTableConverter.GetValue(drv, "ROLLMAP_CLCT_TYPE").GetString(), DataTableConverter.GetValue(drv, "OLD_ROLLMAP_CLCT_TYPE").GetString()))
                    {
                        METHOD = "U";
                    }
                    else
                    {
                        METHOD = "";
                    }

                    DataTableConverter.SetValue(drv, "METHOD", METHOD);
                    if (METHOD != "")
                    {
                        DataTableConverter.SetValue(drv, "CHK", true);
                    }

                    if (e.Cell.Column.Name == "RESNCODE" && DataTableConverter.GetValue(drv, "METHOD").GetString() == "N")
                    {
                        DataTable dtResult = GetEqptDefectCode(DataTableConverter.GetValue(drv, "EQPT_MEASR_PSTN_ID").GetString(), DataTableConverter.GetValue(drv, "RESNCODE").GetString());
                        if (CommonVerify.HasTableRow(dtResult) == true)
                        {
                            drv.SetValue("ROLLMAP_CLCT_TYPE", dtResult.Rows[0]["EQPT_INSP_DFCT_CODE"].ToString());
                        }
                    }
                }
                dg.EndEdit();
                dg.EndEditRow(true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgScrapSection_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid == null) return;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell != null && e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.IsReadOnly == true)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }

                        if (string.Equals(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DEL_FLAG"), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(strReadOnlyColor));
                        }
                    }
                }
            }));
        }
        private void dgScrapSection_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;
            if (string.Equals(DataTableConverter.GetValue(drv, "DEL_FLAG"), "Y"))
            {
                e.Cancel = true;
            }
        }

        #endregion


    }
}

