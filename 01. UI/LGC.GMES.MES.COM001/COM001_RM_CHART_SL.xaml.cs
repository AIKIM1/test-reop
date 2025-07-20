/*************************************************************************************
 Created Date : 2024.03.19
      Creator : 이재우
   Decription : 슬리터 롤맵 팝업(NT향 소형 파우치) - 자동차 리와인더 롤맵과 통합 작업 필요.
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.19  이재우 : Initial Created
  2024.10.25  신광희 : JSON 결과값 Deserialize 시 Data Type 비정상으로 Linq 사용 시 Data Type 부분 수정
  2025.05.29  김한   : Field 사용하여 값 다운로드 시 변수 타입 지정 에러 가능성있어, GetValue로 변수 다운로드 로직 수정
  2025.06.24  김한   : 롤맵 슬리터 차트 투입 랏 및 완성 랏 수집 불량 출력 위치 조정
**************************************************************************************/


using System.Windows;
using System.Windows.Controls;
using System.Data;
using System;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using Action = System.Action;
using System.Collections.Generic;
using ColorConverter = System.Windows.Media.ColorConverter;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Extensions;

using C1.WPF;
using C1.WPF.C1Chart;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// 슬리팅 롤맵 팝업
    /// </summary>
    public partial class COM001_RM_CHART_SL : C1Window, IWorkArea
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
            TagSectionLaneStart,
            TagSectionLaneEnd,
            //TagToolTip,
            //TagVisionTop,
            //TagVisionBack,
            //Material,
            //Sample,
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
        public COM001_RM_CHART_SL()
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

            if (parameters != null && parameters.Length > 0)//10개
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
            }
            #endregion

            #region 2.Windows Size
            ROLLMAP.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
            ROLLMAP.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;
            #endregion

            #region 3.WinParams(LOT,설비)
            txtLot.Text = wParams.lotId;
            txtEquipmentName.Text = wParams.equipmentName;
            #endregion

            #region 4.좌표계(절대/상대)
            coordinateType = CoordinateType.RelativeCoordinates;
            #endregion

            #region 5.IN/OUT Chart X ScrollBar
            chartInput.View.AxisX.ScrollBar = new AxisScrollBar();
            chartOutput.View.AxisX.ScrollBar = new AxisScrollBar();
            #endregion

            #region 6.GetCommonCode(ROLLMAP_COLORMAP_SPEC) : 로딩량/두께 등 스펙 컬러
            dtLineLegend = new DataTable();
            dtLineLegend.Columns.Add("NO", typeof(int));
            dtLineLegend.Columns.Add("COLORMAP", typeof(string));
            dtLineLegend.Columns.Add("VALUE", typeof(string));
            dtLineLegend.Columns.Add("GROUP", typeof(string));
            dtLineLegend.Columns.Add("SHAPE", typeof(string));

            // 로딩량 평균
            DataTable dtColorMapSpec = GetCommonCode("ROLLMAP_COLORMAP_SPEC");

            if (CommonVerify.HasTableRow(dtColorMapSpec))
            {
                string[] exceptionCode = new string[] { "OK", "NG", "Ch", "Err" };

                dtColorMapSpec = (from t in dtColorMapSpec.AsEnumerable()
                                  where !exceptionCode.Contains(t.Field<string>("CMCODE"))
                                  orderby t.GetValue("CMCDSEQ").GetDecimal() ascending
                                  select t).CopyToDataTable();

                foreach (DataRow row in dtColorMapSpec.Rows)
                {
                    dtLineLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCDNAME"].GetString(), "LOAD", "RECT");
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
            DataTable dtV = GetAreaCommonCode("ROLLMAP_HEADER_COND_VISIBILITY");

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

            if (wParams.processCode == Process.ROLL_PRESSING)
            {
                #region Process.ROLL_PRESSING(롤프레스)
                btnRollMapOutsideScrap.Visibility = Visibility.Collapsed;//최외각폐기
                btnRollMapPositionEdit.Visibility = Visibility.Visible;  //구간불량 등록
                btnDefect.Visibility = Visibility.Collapsed;//불량 등록
                #endregion
            }
            else if (wParams.processCode == Process.SLITTING)
            {
                #region Process.ROLL_PRESSING (슬리팅)
                btnRollMapOutsideScrap.Visibility = Visibility.Collapsed;//최외각폐기
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;//구간불량 등록
                btnDefect.Visibility = Visibility.Collapsed;//불량 등록
                #endregion
            }
            else if (wParams.processCode == Process.INS_COATING || wParams.processCode == Process.INS_SLIT_COATING)
            {
                #region Process.INS_COATING / Process.INS_SLIT_COATING (절연코팅 / 절연슬리팅코팅)
                btnRollMapOutsideScrap.Visibility = Visibility.Collapsed;//최외각폐기
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;//구간불량 등록
                btnDefect.Visibility = Visibility.Collapsed;//불량 등록
                #endregion
            }
            #endregion

        }
        #endregion

        #region SetMsbRollMapLaneCheck
        /// <summary>
        /// 맵표현설정(요약) 선택시, 하단 Lane 선택 정보 체크변경
        /// </summary>
        private void SetMsbRollMapLaneCheck()
        {
            // 자동차 맵표현 요약 없을 시 PASS
            if (gdMapExpress.Visibility == Visibility.Collapsed)
            {
                return;
            }

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
        }
        #endregion

        #region InitializeControls
        /// <summary>
        /// 조회시 컨트롤 초기화 및 DataTable 초기화
        /// </summary>
        private void InitializeControls()
        {
            if (grdInputDefectLegend.ColumnDefinitions.Count > 0) grdInputDefectLegend.ColumnDefinitions.Clear();
            if (grdInputDefectLegend.RowDefinitions.Count > 0) grdInputDefectLegend.RowDefinitions.Clear();
            grdInputDefectLegend.Children.Clear();

            if (grdOutputDefectLegend.ColumnDefinitions.Count > 0) grdOutputDefectLegend.ColumnDefinitions.Clear();
            if (grdOutputDefectLegend.RowDefinitions.Count > 0) grdOutputDefectLegend.RowDefinitions.Clear();
            grdOutputDefectLegend.Children.Clear();

            if (grdLineLegend.ColumnDefinitions.Count > 0) grdLineLegend.ColumnDefinitions.Clear();
            if (grdLineLegend.RowDefinitions.Count > 0) grdLineLegend.RowDefinitions.Clear();
            grdLineLegend.Children.Clear();

            if (grdLaneLegend.ColumnDefinitions.Count > 0) grdLaneLegend.ColumnDefinitions.Clear();
            if (grdLaneLegend.RowDefinitions.Count > 0) grdLaneLegend.RowDefinitions.Clear();
            grdLaneLegend.Children.Clear();

            tbInputCore.Text = string.Empty;
            tbOutputCore.Text = string.Empty;

            tbInputProdQty.Text = string.Empty;
            tbInputGoodQty.Text = string.Empty;
            tbOutputProdQty.Text = string.Empty;
            tbOutputGoodQty.Text = string.Empty;

            #region Slitting 롤맵 INPUT LOT에서 Top/Bottom 동적으로 처리 => 고정으로 변경
            tbInputTopupper.Text = string.Empty;
            tbInputToplower.Text = string.Empty;
            tbInputBackupper.Text = string.Empty;
            tbInputBacklower.Text = string.Empty;
            #endregion

            tbOutputTopupper.Text = string.Empty;
            tbOutputToplower.Text = string.Empty;
            tbOutputBackupper.Text = string.Empty;
            tbOutputBacklower.Text = string.Empty;

            isRollMapLot = SelectRollMapLot(wParams.lotId, wParams.wipSeq);
            isRollMapResultLink = IsRollMapResultApply(wParams.processCode, wParams.equipmentSegmentCode);

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
                             EndPosition = t.GetValue("END_PSTN").GetDecimal(),
                             ProdQty = t.GetValue("INPUT_QTY").GetDecimal(),
                             GoodQty = t.GetValue("WIPQTY_ED").GetDecimal()
                         }).FirstOrDefault();

            if (query != null)
            {
                if (chartConfigurationType == ChartConfigurationType.Input)
                {
                    tbInput.Text = query.ProcessName;
                    xInputMaxLength = query.EndPosition.GetDouble();

                    tbInputProdQty.Text = @"P" + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbInputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbInputGoodQty.Text = @"G" + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                    tbInputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                }
                else
                {
                    tbOutput.Text = query.ProcessName;
                    xOutputMaxLength = query.EndPosition.GetDouble();

                    tbOutputProdQty.Text = @"P" + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbOutputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbOutputGoodQty.Text = @"G" + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                    tbOutputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                }
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
            chart.View.AxisY.Min = chart.Name == "chartInput" ? -20 : -30;
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
                             StartPosition = t.GetValue("STRT_PSTN").GetDecimal(),
                             EndPosition = t.GetValue("END_PSTN").GetDecimal()
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

                DataRow newLength = dtLength.NewRow();
                newLength["RAW_END_PSTN"] = $"{query.EndPosition:###,###,###,##0.##}";
                newLength["SOURCE_Y_PSTN"] = 100;
                newLength["FONTSIZE"] = 20;
                dtLength.Rows.Add(newLength);

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
                                StartPosition = t.GetValue("CUM_STRT_PSTN").GetDecimal(),
                                EndPosition = t.GetValue("CUM_END_PSTN").GetDecimal()
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
                                     StartPosition = t.GetValue("CUM_STRT_PSTN").GetDecimal(),
                                     EndPosition = t.GetValue("CUM_END_PSTN").GetDecimal()
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

        #region DrawInputTopBackDisplay(기존 Slitting 롤맵 INPUT LOT에서 Top/Bottom을 동적으로 처리하던 것을 TOP_BACK_REVS_FLAG에 따라 표시)
        /// <summary>
        /// Input Chart T(Top) / B(Back) 표시(TOP_BACK_REVS_FLAG(Y)이면 반대로 표시)  Output Chart 반영 불필요.
        /// </summary>
        /// <param name="dt"></param>
        private void DrawInputTopBackDisplay(DataTable dt)
        {
            //기존 함수(FN_SFC_ROLLMAP_LANE_INFO)결과 => 레인별
            //신규 함수(FN_SFC_RM_GET_LANE_INFO) 결과 => T/B별 
            //기존 롤맵화면과 표시 방식이 달라짐. (OUTPUT CHART는 반영 불필요)
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("TOP_BACK_REVS_FLAG") == "Y"
                         select t).ToList();

            if (query.Any())
            {
                tbInputTopmiddle.Text = "B";
                tbInputBackmiddle.Text = "T";
            }
            else
            {
                tbInputTopmiddle.Text = "T";
                tbInputBackmiddle.Text = "B";
            }
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
                             where t.GetValue("LANE_NO_CUR") != DBNull.Value && !string.IsNullOrEmpty(t.GetValue("LANE_NO_CUR").ToString()) && t.GetValue("LOC2").GetString() != "XX"
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

                if (dtTab.AsEnumerable().Any(x => !string.IsNullOrEmpty(x.GetValue("TAB").ToString())
                                               && x.Field<string>("TAB") == "1"
                                               && x.Field<string>("COATING_PATTERN") == "Plain"))
                {
                    /********************************************************************
                     * 1. 유지부(Coat)에 Tab 있는 경우 제외. 무지부(Plain)에만 존재해야 함.
                     * 2. Lane 이 다수인 경우 제외? 
                     * 3. 유지부 상단 하단에 Tab 값이 1 인경우 upper, lower 모두 표시                     
                     *******************************************************************/
                    var query = (from t in dtTab.AsEnumerable()
                                 where !string.IsNullOrEmpty(t.GetValue("TAB").ToString())
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
                                        where !string.IsNullOrEmpty(t.GetValue("TAB").ToString())
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

                            if (chartConfigurationType == ChartConfigurationType.Input)
                            {
                                #region Slitting 롤맵 INPUT LOT에서 동적으로 Top/Bottom 부분 처리 => 고정으로 변경
                                if (isupper)
                                {
                                    tbInputTopupper.Text = "Tab";
                                    tbInputBackupper.Text = "Tab";
                                }
                                if (islower)
                                {
                                    tbInputToplower.Text = "Tab";
                                    tbInputBacklower.Text = "Tab";
                                }
                                #endregion
                            }
                            else
                            {
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
            // 롤프레스 공정에서만 PET, SCRAP 호출 함.(슬리팅 공정도 호출함)
            //if (chartConfigurationType == ChartConfigurationType.Input) return;
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

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

                c1Chart.Data.Children.Add(new XYDataSeries()
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

                c1Chart.Data.Children.Add(dsPetScrap);
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
            //롤프레스 이후로 제한(슬리팅 공정도 호출함)
            //if (chartConfigurationType == ChartConfigurationType.Input) return;
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

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

                    c1Chart.Data.Children.Add(new XYDataSeries()
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

                c1Chart.Data.Children.Add(dsPetScrap);
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
                #region ChartDisplayType.MarkLabel (SOURCE_ADJ_Y_PSTN = -14)
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -7 : -19;
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

                    string sToolTip = row["EQPT_MEASR_PSTN_NAME"] + "[" + sourceStartPosition + "m]";
                    if (Util.IsNVC(row["MARK_SEQNO"]) == false && row["MARK_SEQNO"] != null)
                        sToolTip = "(COT)" + sToolTip;

                    //row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -8 : -30;
                    row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -20 : -30;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = sToolTip;
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


                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -10 : -15;
                    row["TAG"] = null;

                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + " : " + row["DFCT_NAME"].GetString() + "\n" + "[" + $"{Convert.ToDouble(row["SOURCE_ADJ_END_PSTN"]) - Convert.ToDouble(row["SOURCE_ADJ_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                }

                #endregion
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionLaneStart || chartDisplayType == ChartDisplayType.TagSectionLaneEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });//데이터 원천이 코터는 불량데이터, 롤프레스는 측정 데이터라 컬럼 틀림

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionLaneStart ? -10 : -15;
                    row["TAG"] = null;

                    string laneInfo = row["LANE_NO_LIST"].ToString();
                    row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionLaneStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";

                }
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
                ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                c1Chart.Data.Children.Add(ds);
            }
        }

        private void DrawDefectTagSectionLane(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            for (int x = 0; x < 2; x++)// Start, End 이미지 두번의 표현으로 for문 사용
            {
                DataTable dtTag = MakeTableForDisplay(dt, x == 0 ? ChartDisplayType.TagSectionLaneStart : ChartDisplayType.TagSectionLaneEnd, chartConfigurationType);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTag);
                ds.XValueBinding = (x == 0) ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);

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

        #region DrawRollMapBackGround

        private void DrawRollMapBackGround(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            var queryBackGround = (from t in dt.AsEnumerable()
                                   where t.Field<string>("EQPT_MEASR_PSTN_ID") == "ROLLMAP_BACKGROUND"
                                   select new
                                   {
                                       StartPosition = t.GetValue("ADJ_STRT_PSTN").GetDecimal(),
                                       EndPosition = t.GetValue("ADJ_END_PSTN").GetDecimal(),
                                       ColorMap = t.Field<string>("COLORMAP")
                                   }).FirstOrDefault();

            if (queryBackGround != null)
            {
                var convertFromString = ColorConverter.ConvertFromString(queryBackGround.ColorMap);
                AlarmZone alarmZone = new AlarmZone
                {
                    Near = 0,
                    Far = queryBackGround.EndPosition.GetDouble(),
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = 0,
                    UpperExtent = 100,
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                };
                c1Chart.Data.Children.Insert(0, alarmZone);
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
            #region 슬리터 차트의 경우 웹게이지, 두께 측정데이터가 없음. 공통코드에 등록된 배경색 으로 지정 함.
            var queryBackGround = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("ROLLMAP_BACKGROUND")).ToList();

            DataTable dtBackGround = queryBackGround.Any() ? queryBackGround.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtBackGround))
            {
                dtBackGround.TableName = "ROLLMAP_BACKGROUND";
                DrawRollMapBackGround(dtBackGround, chartConfigurationType);
            }
            #endregion

            //두께 차트 영역에 Gauge 데이터 외의 설비 측정 위치 아이디는 제외하여 표시 함.
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "ROLLMAP_BACKGROUND", "COT_WIDTH_LEN_BACK" };

            var queryGauge = dt.AsEnumerable().Where(o => !gaugeExceptions.Contains(o.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList();

            DataTable dtGauge = queryGauge.Any() ? queryGauge.CopyToDataTable() : dt.Clone();

            for (int i = 0; i < dtGauge.Rows.Count; i++)
            {
                #region 측정데이터에서 불량관련(PET,SCRAP,RW_SCRAP) 정보 제외한 데이터로 차트에 영역(AlarmZone) 표시
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

                        string content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine
                                         + "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine
                                         + "ColorMap : " + Util.NVC(dtGauge.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine
                                         + "Offset : " + Util.NVC(dtGauge.Rows[x]["SCAN_OFFSET"]);

                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                if (Equals(chartConfigurationType, ChartConfigurationType.Input))
                {
                    chartInput.Data.Children.Add(alarmZone);
                }
                else
                {
                    chartOutput.Data.Children.Add(alarmZone);
                }
                #endregion
            }

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
            //var queryLabel = dt.AsEnumerable().Where(x => x.Field<decimal?>("CHART_Y_STRT_CUM_RATE") != null
            //                                           && x.Field<decimal?>("CHART_Y_END_CUM_RATE") != null
            //                                           && Math.Abs(x.Field<decimal>("CHART_Y_STRT_CUM_RATE") - x.Field<decimal>("CHART_Y_END_CUM_RATE")) >= 100).ToList();
            var queryLabel = dt.AsEnumerable().Where(x => x.GetValue("CHART_Y_STRT_CUM_RATE") != DBNull.Value && !string.IsNullOrEmpty(x.GetValue("CHART_Y_STRT_CUM_RATE").ToString())
                                                       && x.GetValue("CHART_Y_END_CUM_RATE") != DBNull.Value && !string.IsNullOrEmpty(x.GetValue("CHART_Y_END_CUM_RATE").ToString())
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

                // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
                var queryTagSectionLane = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
                DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dt.Clone();
                dtTagSectionLane.TableName = "TAG_SECTION_LANE";
                //_dtTagSection = dtTagSectionLane;

                if (CommonVerify.HasTableRow(dtTagSectionLane))
                {
                    DrawDefectTagSectionLane(dtTagSectionLane, chartConfigurationType);
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
                    Grid gridLegend = Equals(chartConfigurationType, ChartConfigurationType.Input) ? grdInputDefectLegend : grdOutputDefectLegend;

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
                if (grdLineLegend.ColumnDefinitions.Count > 0) grdLineLegend.ColumnDefinitions.Clear();
                if (grdLineLegend.RowDefinitions.Count > 0) grdLineLegend.RowDefinitions.Clear();

                grdLineLegend.Children.Clear();

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
                                                     Valuecol = t.GetValue(valueText).GetDecimal()
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
                    grdLineLegend.RowDefinitions.Add(gridTopRow);

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
                    grdLineLegend.Children.Add(stackPanel);
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
                                     Valuecol = t.GetValue(valueText).GetDecimal()
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
            //BR_PRD_SEL_ROLLMAP_CHART => BR_PRD_SEL_RM_RPT_CHART(LANE_LIST 파라미터 추가해서 처리)
            const string bizRuleName = "BR_PRD_SEL_RM_RPT_CHART";
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
            newRow["ADJFLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";

            if (string.IsNullOrEmpty(strLaneList) == false && ((DataView)msbRollMapLane.ItemsSource).Count > laneCnt)
            {
                newRow["LANE_LIST"] = strLaneList;
            }

            inTable.Rows.Add(newRow);

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_LANE,OUT_HEAD,OUT_DEFECT,OUT_GAUGE,OUT_IN_LANE,OUT_IN_HEAD,OUT_IN_DEFECT,OUT_IN_GAUGE", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        HiddenLoadingIndicator();
                        return;
                    }

                    coordinateType = (rdoRelativeCoordinates != null && (bool)rdoRelativeCoordinates.IsChecked) ? CoordinateType.RelativeCoordinates : CoordinateType.Absolutecoordinates;
                    SetMenuUseCount();

                    dt2DBarcodeInfo = Get2DBarcodeInfo(txtLot.Text, wParams.equipmentCode);

                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        DataTable dtInLane = bizResult.Tables["OUT_IN_LANE"];       //투입 차트Lane, 무지부, Top Back표시 테이블
                        DataTable dtInHead = bizResult.Tables["OUT_IN_HEAD"];       //투입 차트 헤더 길이 표시 테이블
                        DataTable dtInDefect = bizResult.Tables["OUT_IN_DEFECT"];     //투입 차트 불량정보 표시 테이블
                        DataTable dtInGauge = bizResult.Tables["OUT_IN_GAUGE"];      //투입 차트 두께 측정 데이터 표시 테이블

                        DataTable dtLane = bizResult.Tables["OUT_LANE"];          //완성 차트Lane, 무지부, Top Back표시 테이블
                        DataTable dtHead = bizResult.Tables["OUT_HEAD"];          //완성 차트 전체 길이 표시 테이블
                        DataTable dtDefect = bizResult.Tables["OUT_DEFECT"];        //완성 차트 불량정보 표시 테이블
                        DataTable dtGauge = bizResult.Tables["OUT_GAUGE"];         //완성 차트 두께 측정 데이터 표시 테이블

                        #region [투입 차트 영역]

                        #region 1.dtInHead
                        if (CommonVerify.HasTableRow(dtInHead))
                        {
                            GetInOutProcessMaxLength(dtInHead, ChartConfigurationType.Input);
                            InitializeChart(chartInput);
                            DrawStartEndYAxis(dtInHead, ChartConfigurationType.Input);
                        }
                        #endregion

                        #region 2.dtInLane
                        if (CommonVerify.HasTableRow(dtInLane))
                        {
                            DrawInputTopBackDisplay(dtInLane);
                            DrawLane(dtInLane, ChartConfigurationType.Input);
                            //DrawWinderDirection(dtInLane); //완성차트쪽에서 하는 로직이라서 호출위치(투입차트) 변경하는게 맞을 듯
                            DisplayTabLocation(dtInLane, ChartConfigurationType.Input);
                        }
                        #endregion

                        #region 3.dtInGauge
                        if (CommonVerify.HasTableRow(dtInGauge))
                        {
                            DrawGauge(dtInGauge, ChartConfigurationType.Input);
                            //if (string.Equals(_processCode, Process.COATING)) DrawMergeLot(dtInGauge);
                        }
                        #endregion

                        #region 4.dtInDefect
                        if (CommonVerify.HasTableRow(dtInDefect))
                        {
                            DrawDefect(dtInDefect, ChartConfigurationType.Input);
                            DrawDefectLegend(dtInDefect, ChartConfigurationType.Input);
                        }

                        dtBaseDefectInput = dtInDefect.Copy();
                        #endregion

                        #endregion

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
                            //DrawThickness(dtGauge, ChartConfigurationType.Output);
                            //DrawLaneLegend();
                        }
                        #endregion

                        #region 4.dtDefect
                        if (CommonVerify.HasTableRow(dtDefect))
                        {
                            DrawDefect(dtDefect, ChartConfigurationType.Output);
                            DrawDefectLegend(dtDefect, ChartConfigurationType.Output);
                        }

                        dtBaseDefectOutput = dtDefect.Copy();

                        tbInputCore.Text = "CORE";
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
        /// 스펙 컬러 정보로 상단 범례 구성(1.상대좌표시 QA샘플,자주검사,최외각 폐기 표시 안함. 2.OK/NG/Ch/Non/Err 표시)
        /// </summary>
        private void GetLegend()
        {
            grdLegendUp.Children.Clear();
            grdLegendDown.Children.Clear();

            DataTable dt = dtLineLegend.Copy();

            #region 1.스펙 컬러 정보 구성
            string[] regendRect = new string[] { ObjectDic.Instance.GetObjectName("QA샘플")
                                               , ObjectDic.Instance.GetObjectName("자주검사")
                                               , ObjectDic.Instance.GetObjectName("최외각 폐기") };

            if ((bool)rdoRelativeCoordinates?.IsChecked)
            {
                dt.AsEnumerable().Where(r => regendRect.Contains(r.Field<string>("VALUE"))).ToList().ForEach(row => row.Delete());
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

                foreach (DataRow row in dt.AsEnumerable().Where(r => regendRect.Contains(r.Field<string>("VALUE")) == false))
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
                foreach (DataRow row in dt.AsEnumerable().Where(r => regendRect.Contains(r.Field<string>("VALUE")) == true))
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

                if ((bool)rdoAbsolutecoordinates?.IsChecked)
                {
                    legendArray = new string[]
                    {
                        ObjectDic.Instance.GetObjectName("이음매")
                      , ObjectDic.Instance.GetObjectName("SCRAP")
                      , ObjectDic.Instance.GetObjectName("RW_SCRAP")
                    };
                }
                else
                {
                    legendArray = new string[]
                    {
                        ObjectDic.Instance.GetObjectName("이음매")
                      , ObjectDic.Instance.GetObjectName("SCRAP")
                    };
                }

                for (int i = 0; i < legendArray.Length; i++)
                {
                    PointCollection pointCollection = new PointCollection();
                    pointCollection.Add(new Point(10, 10));
                    pointCollection.Add(new Point(20, 10));
                    pointCollection.Add(new Point(15, 30));

                    SolidColorBrush convertFromString;

                    if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("이음매")))
                    {
                        convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD"));//Colors.Black
                    }
                    else if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("SCRAP")))
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

            if (gdMapExpress.Visibility == Visibility.Visible && string.IsNullOrEmpty(msbRollMapLane.SelectedItemsToString))
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE"));
                return false;
            }

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

            if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            {
                #region ChartConfigurationType.Input
                c1Chart = chartInput;
                btnRefresh = btnInputRefresh;
                btnZoomIn = btnInputZoomIn;
                btnZoomOut = btnInputZoomOut;
                #endregion
            }
            else
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
                    c1Chart.View.Margin = c1Chart.Name == "chartInput" ? new Thickness(0) : new Thickness(0, 0, 0, 5);
                    c1Chart.Padding = new Thickness(10, 2, 5, 10);
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
            newRow["MENUID"] = "SFU010160123";                 // 메뉴ID 코터:SFU010160121, 롤프레스:SFU010160122, 슬리터:SFU010160123, 리와인딩:SFU010160124  
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

        #endregion

        #region Events

        #region RollMap_Loaded
        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RollMap_Loaded(object sender, RoutedEventArgs e)
        {
            InitSetting();
            InitializeCombo();
            GetLegend();
            //GetRollMap();
            Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
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
            Grid grid = sender as Grid;

            string[] splitItem = grid.Tag.GetString().Split(';');

            if (splitItem[6] != null && splitItem[6].GetString() == "4")
            {
                return;
            }

            string equipmentMeasurementCode = splitItem[0];
            string startPosition = splitItem[1];
            string endPosition = splitItem[2];
            string scrapQty = $"{splitItem[2].GetDouble() - splitItem[1].GetDouble():###,###,###,##0.##}";
            string lotId = splitItem[3];
            string wipSeq = splitItem[4];
            string collectSeq = splitItem[5];
            string rollMapCollectType = string.Empty;
            string winderLength = string.Empty;

            if (string.Equals(equipmentMeasurementCode, "RW_SCRAP"))
            {
                rollMapCollectType = splitItem[6];
                winderLength = splitItem[7];
            }

            if (string.Equals(equipmentMeasurementCode, "PET"))
            {
                //CMM_ROLLMAP_SCRAP_LOSS => CMM_RM_SCRAP_LOSS
                CMM_RM_SCRAP_LOSS popRollMapPet = new CMM_RM_SCRAP_LOSS();
                popRollMapPet.FrameOperation = FrameOperation;

                object[] parameters = new object[4];
                parameters[0] = equipmentMeasurementCode;
                parameters[1] = startPosition;
                parameters[2] = endPosition;
                parameters[3] = scrapQty;

                C1WindowExtension.SetParameters(popRollMapPet, parameters);

                Dispatcher.BeginInvoke(new Action(() => popRollMapPet.ShowModal()));
            }
            else if (string.Equals(equipmentMeasurementCode, "RW_SCRAP"))
            {
                //CMM_ROLLMAP_RW_SCRAP => CMM_RM_RW_SCRAP
                CMM_RM_RW_SCRAP popRollMapRewinderScrap = new CMM_RM_RW_SCRAP();
                popRollMapRewinderScrap.FrameOperation = FrameOperation;

                object[] parameters = new object[10];
                parameters[0] = lotId;
                parameters[1] = wipSeq;
                parameters[2] = wParams.processCode;
                parameters[3] = wParams.equipmentCode;
                parameters[4] = equipmentMeasurementCode;
                parameters[5] = startPosition;
                parameters[6] = endPosition;
                parameters[7] = winderLength;
                parameters[8] = collectSeq;
                parameters[9] = rollMapCollectType;

                C1WindowExtension.SetParameters(popRollMapRewinderScrap, parameters);

                popRollMapRewinderScrap.Closed += popRollMapRewinderScrap_Closed;

                Dispatcher.BeginInvoke(new Action(() => popRollMapRewinderScrap.ShowModal()));
            }
            else
            {
                //SCRAP 선택 시 Input 영역에 스크랩한 구간을 표시 함.
                DrawScrapSection(lotId, wipSeq, startPosition);

                //CMM_ROLLMAP_SCRAP => CMM_RM_SCRAP
                CMM_RM_SCRAP popRollMapScrap = new CMM_RM_SCRAP();
                popRollMapScrap.FrameOperation = FrameOperation;

                object[] parameters = new object[7];
                parameters[0] = lotId;
                parameters[1] = wipSeq;
                parameters[2] = equipmentMeasurementCode;
                parameters[3] = collectSeq;
                parameters[4] = startPosition;
                parameters[5] = endPosition;
                parameters[6] = scrapQty;

                C1WindowExtension.SetParameters(popRollMapScrap, parameters);

                popRollMapScrap.Closed += popRollMapScrap_Closed;

                Dispatcher.BeginInvoke(new Action(() => popRollMapScrap.ShowModal()));
            }
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
            //Util.MessageValidation("웹게이지 데이터가 존재하지 않습니다.");
            Util.MessageInfo("SFU8501", ObjectDic.Instance.GetObjectName("웹게이지"));
            rdoNone.IsChecked = true;
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
            //Util.MessageValidation("두께 데이터가 존재하지 않습니다.");
            Util.MessageInfo("SFU8501", ObjectDic.Instance.GetObjectName("두께"));
            rdoNone.IsChecked = true;
        }
        #endregion

        #region 계측기(None)
        private void rdoNone_Click(object sender, RoutedEventArgs e)
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
        #endregion

        #region 최외각 폐기
        private void popRollMapOutsideScrap_Closed(object sender, EventArgs e)
        {
            //CMM_ROLLMAP_OUTSIDE_SCRAP => CMM_RM_OUTSIDE_SCRAP
            CMM_RM_OUTSIDE_SCRAP popup = sender as CMM_RM_OUTSIDE_SCRAP;

            if (popup != null && popup.IsUpdated)
            {
                isRollMapLot = SelectRollMapLot(wParams.lotId, wParams.wipSeq);
                //btnSearch_Click(btnSearch, null);
                Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            }
        }
        /// <summary>
        /// 최외각 폐기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRollMapOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            //CMM_ROLLMAP_OUTSIDE_SCRAP => CMM_RM_OUTSIDE_SCRAP
            CMM_RM_OUTSIDE_SCRAP popRollMapOutsideScrap = new CMM_RM_OUTSIDE_SCRAP();

            object[] parameters = new object[5];
            parameters[0] = wParams.processCode;
            parameters[1] = wParams.equipmentCode;
            parameters[2] = wParams.lotId;
            parameters[3] = wParams.wipSeq;
            parameters[4] = "OUTSIDE_SCRAP"; //최외각 폐기

            C1WindowExtension.SetParameters(popRollMapOutsideScrap, parameters);
            popRollMapOutsideScrap.Closed += popRollMapOutsideScrap_Closed;

            Dispatcher.BeginInvoke(new Action(() => popRollMapOutsideScrap.ShowModal()));
        }
        #endregion

        #region 구간불량 등록
        private void popRollMapPositionUpdate_Closed(object sender, EventArgs e)
        {
            // CMM_ROLLMAP_PSTN_UPD => CMM_RM_TAG_SECTION
            CMM_RM_TAG_SECTION popup = sender as CMM_RM_TAG_SECTION;

            if (popup != null && popup.IsUpdated)
            {
                isRollMapLot = SelectRollMapLot(wParams.lotId, wParams.wipSeq);
                //btnSearch_Click(btnSearch, null);
                Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            }
            else
            {
                SetTagSectionClear();
            }
        }

        /// <summary>
        /// 구간불량(TAG_SECTION) 등록 (보정이후 상대좌표인 경우에만 등록 가능.)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRollMapPositionEdit_Click(object sender, RoutedEventArgs e)
        {
            if (coordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", ObjectDic.Instance.GetObjectName("구간불량 등록"));
                return;
            }

            // CMM_ROLLMAP_PSTN_UPD => CMM_RM_TAG_SECTION
            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[11];
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
            parameters[10] = (isRollMapResultLink && isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;

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
            CMM_ROLLMAP_DATACOLLECT popupRollMapDataCollect = new CMM_ROLLMAP_DATACOLLECT();
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
        private void popRollMapRewinderScrap_Closed(object sender, EventArgs e)
        {
            //CMM_ROLLMAP_RW_SCRAP => CMM_RM_RW_SCRAP
            CMM_RM_RW_SCRAP popup = sender as CMM_RM_RW_SCRAP;
            if (popup != null && popup.IsUpdated)
            {
                //btnSearch_Click(btnSearch, null);
                Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            }
        }


        private void popRollMapScrap_Closed(object sender, EventArgs e)
        {
            //CMM_ROLLMAP_SCRAP => CMM_RM_SCRAP
            CMM_RM_SCRAP popup = sender as CMM_RM_SCRAP;

            if (popup != null && popup.IsUpdated)
            {
                //btnSearch_Click(btnSearch, null);
                Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            }
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

        #endregion

    }
}
