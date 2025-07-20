/*************************************************************************************
 Created Date : 2022.04.30
      Creator : 신광희
   Decription : 자동차 전극 Roll Map - 범용차트(동일한 BizRule을 호출 하여 공정 구분 없이 범용으로 사용 목적)
--------------------------------------------------------------------------------------
 [Change History]
  2022.04.30  신광희 : Initial Created
  2022.10.19  신광희 : 롤맵 차트 팝업 조회 시 SOM 메뉴 사용 횟수(TB_SOM_MENU_USE_COUNT) 테이블 등록
  2022.10.21  신광희 : 롤맵 차트 팝업 조회 시 ROLLMAP COLORMAP SPEC, ROLLMAP LANE SYMBOL COLOR 데이터 검색 시 하드 코딩 제거
  2022.11.09  신광희 : CORE 표시, TAG_SECTION, TAG_SPOT 상하 간격 위치 조정
  2022.11.10  신광희 : SCRAP ToolTip 표시 형태 변경  Ex) [SCRAP]위치 200m, 길이 100.2 m 형태로 변경
  2022.11.29  신광희 : 불량 범례 우측 불량수량 표시
  2023.03.21  신광희 : 무지부 Tab 위치 표시
  2023.09.05  신광희 : LOT CUT 차트 표현 시 Cut 영역 Path 표시
  2024.01.02  김지호 : [E20231123-001387] TAG_SPOT TOOLTIP내 불량 약어명 표시
  2024.02.23  김  한 : ROLLMAP_DEFAULT_GAUGE에 따른 코터 롤맵 출력
  2024.02.28  신광희 : PET 항목 상세 구분 관리를 위한 롤맵 설비 검사 불량 구분코드(1~8) 바인딩 및 범례 표시 수정
  2024.03.13  김지호 : [E20240223-001703] CHART TOP/BACK 분리 및 투입랏 조회시 설정 값 가져와서 코터 롤맵 표현시 사용 
  2024.03.19  김지호 : [E20240319-000656] 범례에 Non 추가 및 차트 색 (회색) 추가, SCAN_COLRMAP에 Err, Non, Ch 시에 Tooltip에 데이터 오류, 데이터 미수신, 데이터 미측정 내용 표시
  2024.03.27  김지호 : [E20240223-001703] 롤프레스 롤맵에서는 코터 롤맵에서 사용자가 선택한 계측기 항목이 아닌 MMD에 저장된 항목이 우선이 되도록 수정
  2024.04.09  조성근 : OUT LOT의 Scrap 범위를 투입 LOT 에 표시 - 배포전까지 주석으로 막아둠.
  2024.06.12  정기동 : 부분 LANE 불량 툴팁 표기 기능 추가 (TAG_SECTION_LANE) 
  2024.06.18  조성근 : OUT LOT의 Scrap 범위를 투입 LOT 에 표시
  2024.08.09  조성근 : OUT LOT의 Scrap 범위 툴팁에 TAG_SECTION 좌표 표시
  2024.09.09  김지호 : [E20240731-000740] 롤맵 실행 시 사용자 환경 설정 정보를 불러 올때 cboTop -> cboMeasurementTop, cboBack -> cboMeasurementBack 로 변경
  2024.08.27  조성근 : [E20240827-000372] - 롤프레스 롤맵 실적 불량 비교 팝업 추가 및 구간불량 버튼제어
  2024.09.20  김지호 : [E20240905-001389] TAG_SECTION 툴팁내 불량 명칭 및 총 거리값 표시되도록 수정
  2024.09.25  정기동 : E20240911-000907 NFF 구간 불량 정보 중 Lane 구간 불량 수집 항목 ID 체계를 HM과 동일하게 관리하도록 수정 (TAG_SECTION -> TAG_SECTION_LANE)
  2024.12.17  이민영 : [E20241126-000434] 코터 웹게이지 MIN/MAX/대표값 추가로 설정된 설비별 툴팁 표시되도록 수정
  2025.03.26  신광희 : [E20250324-000244] 롤프레스 롤맵 시스템의 실적자동입력화 기능 개선을 위한 신규개발/기능변경 건
  2025.04.04  조성근 : [E20250325-000705] 롤프레스 롤맵 아웃피드 스크랩 추가
**************************************************************************************/


using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System.Data;
using System;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using C1.WPF;
using System.Windows.Media;
using C1.WPF.C1Chart;
using LGC.GMES.MES.CMM001.Extensions;
using Action = System.Action;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_ROLLMAP_ROLLPRESS_NEW : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        private DataTable _dtDefectOutput;
        private DataTable _dtDefectInput;
        private DataTable _dtBaseDefectOutput;
        private DataTable _dtBaseDefectInput;
        private DataTable _dtLineLegend;
        private DataTable _dtLaneLegend;
        private DataTable _dtLane;

        private DataTable _dtOutGauge;
        private DataTable _dt2DBarcodeInfo;

        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _runLotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _laneQty = string.Empty;
        private string _version = string.Empty;
        private double _xInputMaxLength;
        private double _xOutputMaxLength;

        private CoordinateType _CoordinateType;
        private bool _isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        private bool _isRollMapLot;
        private bool _isShowBtnDeft = false;	//조성근 2024.08.28 롤프레스 롤맵 수불 적용

        private bool _isMinMaxEqpt = false;  //2024-12-17 웹게이지 MIN/MAX 툴팁 표시여부(설비기준)

        private enum CoordinateType
        {
            RelativeCoordinates,    //상대좌표
            Absolutecoordinates     //절대좌표
        }

        AlarmZone dsscrapSection = new AlarmZone();

        #endregion

        public COM001_ROLLMAP_ROLLPRESS_NEW()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                _processCode = Util.NVC(parameters[0]);
                _equipmentSegmentCode = Util.NVC(parameters[1]);
                _equipmentCode = Util.NVC(parameters[2]);
                _runLotId = Util.NVC(parameters[3]);
                _wipSeq = Util.NVC(parameters[4]);
                _laneQty = Util.NVC(parameters[5]);
                _equipmentName = Util.NVC(parameters[6]);
                _version = Util.NVC(parameters[7]);
            }

            Initialize();
            GetLegend();
            GetRollMap();

            this.Closing += UserControl_Closing;
        }

        private void UserControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetUserConfigurationControl();
        }

        #region # Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            InitializeControls();
            InitializeDataTables();
            SelectRollMap();
            EndUpdateChart();
        }

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            CMM_RM_RP_DATACOLLECT popupRollMapDataCollect = new CMM_RM_RP_DATACOLLECT(); //조성근 2024.08.28 롤프레스 롤맵 수불 적용
            popupRollMapDataCollect.FrameOperation = FrameOperation;
            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = _runLotId;
            parameters[3] = _wipSeq;
            parameters[4] = _laneQty;

            C1WindowExtension.SetParameters(popupRollMapDataCollect, parameters);
            popupRollMapDataCollect.Closed += popupRollMapDataCollect_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupRollMapDataCollect.ShowModal()));
        }

        private void popupRollMapDataCollect_Closed(object sender, EventArgs e)
        {
            CMM_RM_RP_DATACOLLECT popup = sender as CMM_RM_RP_DATACOLLECT; //조성근 2024.08.28 롤프레스 롤맵 수불 적용
            if (popup != null && popup.IsUpdated)
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void rdoAbsolutecoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
        }

        private void rdoRelativeCoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
        }

        private void rdoWebgauge_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rdoThick_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnInputRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0, ChartConfigurationType.Input);
        }

        private void btnInputZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (chartInput.View.AxisX.MinScale >= chartInput.View.AxisX.Scale * 0.75)
                return;

            SetScale(chartInput.View.AxisX.Scale * 0.75, ChartConfigurationType.Input);
        }

        private void btnInputZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartInput.View.AxisX.Scale * 1.50, ChartConfigurationType.Input);
        }

        private void btnInputReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartInput.BeginUpdate();
            chartInput.View.AxisX.Reversed = !chartInput.View.AxisX.Reversed;
            chartInput.EndUpdate();
        }

        private void btnInputReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartInput.BeginUpdate();
            chartInput.View.AxisY.Reversed = !chartInput.View.AxisY.Reversed;
            chartInput.EndUpdate();
        }

        private void btnOutputRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0, ChartConfigurationType.Output);
        }

        private void btnOutputZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (chartOutput.View.AxisX.MinScale >= chartOutput.View.AxisX.Scale * 0.75)
                return;

            SetScale(chartOutput.View.AxisX.Scale * 0.75, ChartConfigurationType.Output);
        }

        private void btnOutputZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartOutput.View.AxisX.Scale * 1.50, ChartConfigurationType.Output);
        }

        private void btnOutputReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartOutput.BeginUpdate();
            chartOutput.View.AxisX.Reversed = !chartOutput.View.AxisX.Reversed;
            chartOutput.EndUpdate();
        }

        private void btnOutputReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartOutput.BeginUpdate();
            chartOutput.View.AxisY.Reversed = !chartOutput.View.AxisY.Reversed;
            chartOutput.EndUpdate();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            Grid grid = sender as Grid;

            string[] splitItem = grid.Tag.GetString().Split(';');
            if (splitItem[6] != null && splitItem[6].GetString() == "4") return;

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

                if (_isRollMapResultLink && _isRollMapLot) return; //조성근 2024.08.28 롤프레스 롤맵 수불 적용
            }

            if (string.Equals(equipmentMeasurementCode, "PET"))
            {
                CMM_ROLLMAP_SCRAP_LOSS popRollMapPet = new CMM_ROLLMAP_SCRAP_LOSS();
                popRollMapPet.FrameOperation = FrameOperation;
                object[] parameters = new object[4];
                parameters[0] = equipmentMeasurementCode;
                parameters[1] = startPosition;
                parameters[2] = endPosition;
                parameters[3] = scrapQty;
                C1WindowExtension.SetParameters(popRollMapPet, parameters);
                Dispatcher.BeginInvoke(new Action(() => popRollMapPet.ShowModal()));
            }
            else if (string.Equals(equipmentMeasurementCode, "SCRAP_SECTION"))
            {
                popScrapSection(collectSeq);
            }
            else if (string.Equals(equipmentMeasurementCode, "RW_SCRAP"))
            {
                CMM_ROLLMAP_RW_SCRAP popRollMapRewinderScrap = new CMM_ROLLMAP_RW_SCRAP();
                popRollMapRewinderScrap.FrameOperation = FrameOperation;
                object[] parameters = new object[10];
                parameters[0] = lotId;
                parameters[1] = wipSeq;
                parameters[2] = _processCode;
                parameters[3] = _equipmentCode;
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

                CMM_ROLLMAP_SCRAP popRollMapScrap = new CMM_ROLLMAP_SCRAP();
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

        private void popRollMapScrap_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_SCRAP popup = sender as CMM_ROLLMAP_SCRAP;
            if (popup != null && popup.IsUpdated)
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void popRollMapRewinderScrap_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_RW_SCRAP popup = sender as CMM_ROLLMAP_RW_SCRAP;
            if (popup != null && popup.IsUpdated)
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void cboLotId_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void DescriptionOnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null || textBlock.Tag == null || textBlock.Text == null) return;

                ShowLoadingIndicator();
                DoEvents();

                C1Chart c1Chart;
                DataTable dtDefect, dtBaseDefect;

                string[] splitItem = textBlock.Tag.GetString().Split('|');
                string textBlockTag = splitItem[0].GetString();
                string colorMap = splitItem[1].GetString();

                if (string.Equals(textBlockTag, ChartConfigurationType.Input.ToString()))
                {
                    c1Chart = chartInput;
                    dtDefect = _dtDefectInput;
                    dtBaseDefect = _dtBaseDefectInput;
                }
                else
                {
                    c1Chart = chartOutput;
                    dtDefect = _dtDefectOutput;
                    dtBaseDefect = _dtBaseDefectOutput;
                }

                string[] defectDataSeries = new string[] { "VISION_SURF_NG_TOP", "VISION_SURF_NG_BACK", "VISION_NG_TOP", "VISION_NG_BACK", "INS_ALIGN_VISION_NG_TOP", "INS_ALIGN_VISION_NG_BACK", "INS_OVERLAY_VISION_NG" };

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
                    dtDefect.Select("EQPT_MEASR_PSTN_ID = '" + textBlockText + "' And ABBR_NAME = '" + textBlockText + "' And COLORMAP = '" + colorMap + "' ").ToList().ForEach(row => row.Delete());
                    dtDefect.AcceptChanges();
                    textBlock.Foreground = Brushes.Black;
                }
                var queryDefect = dtBaseDefect.AsEnumerable()
                    .Where(x => !dtDefect.AsEnumerable().Any(y => y.Field<string>("ABBR_NAME") == x.Field<string>("ABBR_NAME") && y.Field<string>("COLORMAP") == x.Field<string>("COLORMAP")));
                if (queryDefect.Any()) DrawDefect(queryDefect.CopyToDataTable(), (ChartConfigurationType)Enum.Parse(typeof(ChartConfigurationType), textBlockTag), true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void TextBlockLane_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null) return;

                ShowLoadingIndicator();
                DoEvents();

                //_isResearch = true;
                chartThickness.Data.Children.Clear();

                string laneNo = textBlock.Text.Replace("Lane", "").Trim();

                if (textBlock.Foreground == Brushes.Black)
                {
                    _dtLane.Rows.Remove(_dtLane.Select("LANE_NO = '" + laneNo + "'")[0]);
                    textBlock.Foreground = Brushes.LightGray;
                }

                else //if(textBlock.Foreground == Brushes.Gray)
                {
                    _dtLane.Rows.Add(laneNo);
                    textBlock.Foreground = Brushes.Black;
                }

                DrawThickness(_dtOutGauge, ChartConfigurationType.Output);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
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
        }

        #endregion

        #region # Method
        private void Initialize()
        {
            try
            {
                ROLLMAP.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                ROLLMAP.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;

                InitializeControl();

                btnDefect.Visibility = Visibility.Collapsed;    //조성근 2024.08.28 롤프레스 롤맵 수불 적용
                bShowDeftButton(); //조성근 2024.08.28 롤프레스 롤맵 수불 적용
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeControl()
        {
            txtLot.Text = _runLotId;
            txtEquipmentName.Text = _equipmentName;

            if (rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked)
                _CoordinateType = CoordinateType.RelativeCoordinates;
            else
                _CoordinateType = CoordinateType.Absolutecoordinates;

            chartInput.View.AxisX.ScrollBar = new AxisScrollBar();
            chartOutput.View.AxisX.ScrollBar = new AxisScrollBar();

            _dtLineLegend = new DataTable();
            _dtLineLegend.Columns.Add("NO", typeof(int));
            _dtLineLegend.Columns.Add("COLORMAP", typeof(string));
            _dtLineLegend.Columns.Add("CMCODE", typeof(string));
            _dtLineLegend.Columns.Add("VALUE", typeof(string));
            _dtLineLegend.Columns.Add("GROUP", typeof(string));
            _dtLineLegend.Columns.Add("SHAPE", typeof(string));

            // 로딩량 평균
            DataTable dtColorMapSpec = GetColorMapSpec("ROLLMAP_COLORMAP_SPEC");
            if (CommonVerify.HasTableRow(dtColorMapSpec))
            {
                foreach (DataRow row in dtColorMapSpec.Rows)
                {
                    _dtLineLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCODE"].GetString(), row["CMCDNAME"].GetString(), "LOAD", "RECT");
                }
            }

            // Lane 범례 색상 
            _dtLaneLegend = new DataTable();
            _dtLaneLegend.Columns.Add("LANE_NO", typeof(string));
            _dtLaneLegend.Columns.Add("COLORMAP", typeof(string));

            DataTable dtLaneSymbolColor = GetLaneSymbolColor("ROLLMAP_LANE_SYMBOL_COLOR");
            if (CommonVerify.HasTableRow(dtLaneSymbolColor))
            {
                foreach (DataRow row in dtLaneSymbolColor.Rows)
                {
                    _dtLaneLegend.Rows.Add(row["CMCODE"].GetString(), row["ATTRIBUTE1"].GetString());
                }
            }

            // 두께 데이터 Lane 정보
            _dtLane = new DataTable();
            _dtLane.Columns.Add("LANE_NO", typeof(string));

            _dtDefectOutput = new DataTable();
            _dtDefectOutput.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefectOutput.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefectOutput.Columns.Add("COLORMAP", typeof(string));
            _dtDefectInput = _dtDefectOutput.Copy();

            GetUserConfigurationControl();
        }

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
                double length = c1Chart.Name == "chartInput" ? _xInputMaxLength : _xOutputMaxLength;

                if (length < 11)
                    majorUnit = 1;
                else if (length < 101)
                    majorUnit = 10;
                else
                    majorUnit = Math.Round(length / 200) * 10;

                c1Chart.View.AxisX.MinorTickOverlap = 20;
                c1Chart.View.AxisX.MajorGridStrokeDashes = new DoubleCollection { 3, 2 };
                c1Chart.View.AxisX.AutoMax = false;
                c1Chart.View.AxisX.AutoMin = false;
                c1Chart.View.AxisX.MajorUnit = majorUnit;
                c1Chart.View.AxisX.AnnoFormat = "#,##0";
            }
            else
            {
                c1Chart.View.AxisX.Foreground = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void InitializeControls()
        {
            if (grdInputDefectLegend.ColumnDefinitions.Count > 0) grdInputDefectLegend.ColumnDefinitions.Clear();
            if (grdInputDefectLegend.RowDefinitions.Count > 0) grdInputDefectLegend.RowDefinitions.Clear();
            grdInputDefectLegend.Children.Clear();

            if (grdOutputDefectLegend.ColumnDefinitions.Count > 0) grdOutputDefectLegend.ColumnDefinitions.Clear();
            if (grdOutputDefectLegend.RowDefinitions.Count > 0) grdOutputDefectLegend.RowDefinitions.Clear();
            grdOutputDefectLegend.Children.Clear();

            tbInputCore.Text = string.Empty;
            tbOutputCore.Text = string.Empty;

            tbInputProdQty.Text = string.Empty;
            tbInputGoodQty.Text = string.Empty;
            tbOutputProdQty.Text = string.Empty;
            tbOutputGoodQty.Text = string.Empty;

            tbInputTopupper.Text = string.Empty;
            tbInputToplower.Text = string.Empty;
            tbInputBackupper.Text = string.Empty;
            tbInputBacklower.Text = string.Empty;
            tbOutputTopupper.Text = string.Empty;
            tbOutputToplower.Text = string.Empty;
            tbOutputBackupper.Text = string.Empty;
            tbOutputBacklower.Text = string.Empty;

            _isRollMapLot = SelectRollMapLot();
            _isRollMapResultLink = IsRollMapResultApply();

            SetUseScrapSection();   //조성근 2025.04.10 Scrap Section 추가

            //조성근 2024.08.28 롤프레스 롤맵 수불 적용  start
            if (_isRollMapResultLink && _isRollMapLot)
            {
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnRollMapPositionEdit.Visibility = Visibility.Visible;
            }

            if (_isShowBtnDeft == true) btnDefect.Visibility = Visibility.Visible;
            else btnDefect.Visibility = Visibility.Collapsed;
            //조성근 2024.08.28 롤프레스 롤맵 수불 적용 - end

            BeginUpdateChart();

        }

        /// <summary>
        /// 검색된 내용이 없더라도 무지부 설정등 요청이 있는 경우 해당 메소드에서 처리 함.
        /// </summary>
        private void InitializeBaseChart()
        {

        }

        private void InitializeDataTables()
        {
            _dtDefectOutput?.Clear();
            _dtDefectInput?.Clear();
            _dtBaseDefectInput?.Clear();
            _dtBaseDefectOutput?.Clear();
            _dtOutGauge?.Clear();
            _dtLane?.Clear();
        }

        /// <summary>
        /// Input, Output 차트 축 초기 설정
        /// </summary>
        /// <param name="chart"></param>
        private void InitializeChart(C1Chart chart)
        {
            double maxLength = string.Equals(chart.Name, "chartInput") ? _xInputMaxLength.Equals(0) ? 10 : _xInputMaxLength : _xOutputMaxLength.Equals(0) ? 10 : _xOutputMaxLength;


            chart.View.AxisX.Min = 0;
            chart.View.AxisY.Min = -40;
            //chart.View.AxisY.Min = chart.Name == "chartInput" ? -10 : -30; // TODO 수정 필요 함. 차트 하단의 태그.. 등의 표시로  AxisY Min 값을 설정 함.
            chart.View.AxisX.Max = maxLength;
            chart.View.AxisY.Max = 100;
            InitializeChartView(chart);
        }

        private void GetLegend()
        {
            //grdLegend.Children.Clear();
            grdLegendUp.Children.Clear();
            grdLegendDown.Children.Clear();

            DataTable dt = _dtLineLegend.Copy();

            string[] exceptCode = new string[] { "OK", "NG", "Ch", "Err" };



            string[] exceptLegend = new string[] { "QA샘플", "자주검사", "최외각 폐기" };




            if (rdoRelativeCoordinates.IsChecked != null && rdoRelativeCoordinates.IsChecked == true)
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

                if (rdoAbsolutecoordinates.IsChecked != null && rdoAbsolutecoordinates.IsChecked == true)
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

            /*
            dt = _dtLineLegend.AsEnumerable()
                    .Where(row => new string[] { "HH", "H", "SV", "L", "LL", "Non" }.Contains(row.Field<string>("VALUE")))
                    .CopyToDataTable();

            if (rdoAbsolutecoordinates.IsChecked == true)
            {
                dt.Rows.Add(1, "#FAED7D", ObjectDic.Instance.GetObjectName("QA샘플"), "LOAD", "RECT");
                dt.Rows.Add(1, "#00D8FF", ObjectDic.Instance.GetObjectName("자주검사"), "LOAD", "RECT");
                dt.Rows.Add(1, "#FFA7A7", ObjectDic.Instance.GetObjectName("최외각 폐기"), "LOAD", "RECT");
            }


            for (int x = 0; x < 3; x++)
            {
                ColumnDefinition gridCol1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                grdLegend.ColumnDefinitions.Add(gridCol1);
            }

            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 0, 3, 0)
            };

            DataRow[] dr = dt.Select();
            if (dr.Length > 0)
            {
                foreach (DataRow row in dr)
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
                grdLegend.Children.Add(sp);

                // 이음매,  SCRAP, RW_SCRAP Polygon 범례 추가
                StackPanel stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 3, 0)
                };

                string[] legendArray;
                if (rdoAbsolutecoordinates.IsChecked != null && rdoAbsolutecoordinates.IsChecked == true)
                {
                    legendArray = new string[] { ObjectDic.Instance.GetObjectName("SCRAP"), ObjectDic.Instance.GetObjectName("CONNECTION"), ObjectDic.Instance.GetObjectName("PET"), ObjectDic.Instance.GetObjectName("ETC"), ObjectDic.Instance.GetObjectName("RW_SCRAP") };
                }
                else
                {
                    legendArray = new string[] { ObjectDic.Instance.GetObjectName("SCRAP"), ObjectDic.Instance.GetObjectName("CONNECTION"), ObjectDic.Instance.GetObjectName("PET"), ObjectDic.Instance.GetObjectName("ETC") };
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

                Grid.SetColumn(stackPanel, 1);
                Grid.SetRow(stackPanel, 1);
                grdLegend.Children.Add(stackPanel);


                // CUT_SPLIT
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

                Grid.SetColumn(spLotCut, 2);
                Grid.SetRow(spLotCut, 1);
                grdLegend.Children.Add(spLotCut);
            }
            */
        }

        private void GetRollMap()
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool SetInRollMapDefaultGauge(ref string mPoint_top, ref string mPstn_top, ref string mPoint_back, ref string mPstn_back)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = "ROLLMAP_DEFAULT_GAUGE";
                newRow["COM_CODE"] = "E2000";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (string.Equals("W", dtResult.Rows[0]["ATTR1"].GetString()))
                    {
                        mPoint_top = "1";
                        mPstn_top = "1";
                    }
                    else if (string.Equals("T", dtResult.Rows[0]["ATTR1"].GetString()))
                    {
                        mPoint_top = "2";
                        mPstn_top = "1";
                    }
                    else
                    {
                        mPoint_top = "1";
                        mPstn_top = "1";
                    }

                    if (string.Equals("W", dtResult.Rows[0]["ATTR2"].GetString()))
                    {
                        mPoint_back = "1";
                        mPstn_back = "1";
                    }
                    else if (string.Equals("T", dtResult.Rows[0]["ATTR2"].GetString()))
                    {
                        mPoint_back = "2";
                        mPstn_back = "1";
                    }
                    else if (string.Equals("TD", dtResult.Rows[0]["ATTR2"].GetString()))
                    {
                        mPoint_back = "3";
                        mPstn_back = "1";
                    }
                    else
                    {
                        mPoint_back = "1";
                        mPstn_back = "1";
                    }

                    return true;
                }
                else
                {
                    mPoint_top = "1";
                    mPstn_top = "1";
                    mPoint_back = "1";
                    mPstn_back = "1";
                }
                return false;
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        private void SelectRollMap()
        {
            string mPoint_top = null;
            string mPstn_top = null;
            string mPoint_back = null;
            string mPstn_back = null;

            if (!SetInRollMapDefaultGauge(ref mPoint_top, ref mPstn_top, ref mPoint_back, ref mPstn_back))
                GetCboUserConf(ref mPoint_top, ref mPstn_top, ref mPoint_back, ref mPstn_back);

            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CHART";
            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            //inTable.Columns.Add("GAUGEFLAG", typeof(string));
            inTable.Columns.Add("MPOINT_TOP", typeof(string));
            inTable.Columns.Add("MPSTN_TOP", typeof(string));
            inTable.Columns.Add("MPOINT_BACK", typeof(string));
            inTable.Columns.Add("MPSTN_BACK", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = txtLot.Text;
            newRow["WIPSEQ"] = _wipSeq;
            newRow["ADJFLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
            //newRow["GAUGEFLAG"] = SetInRollMapDefaultGauge();
            newRow["MPOINT_TOP"] = mPoint_top;
            newRow["MPSTN_TOP"] = mPstn_top;
            newRow["MPOINT_BACK"] = mPoint_back;
            newRow["MPSTN_BACK"] = mPstn_back;

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

                    SetMenuUseCount();
                    _dt2DBarcodeInfo = Get2DBarcodeInfo(txtLot.Text, _equipmentCode);
                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        DataTable dtInLane = bizResult.Tables["OUT_IN_LANE"];       //투입 차트Lane, 무지부, Top Back표시 테이블
                        DataTable dtInHead = bizResult.Tables["OUT_IN_HEAD"];       //투입 차트 헤더 길이 표시 테이블
                        DataTable dtInDefect = bizResult.Tables["OUT_IN_DEFECT"];   //투입 차트 불량정보 표시 테이블
                        DataTable dtInGauge = bizResult.Tables["OUT_IN_GAUGE"];     //투입 차트 두께 측정 데이터 표시 테이블

                        DataTable dtLane = bizResult.Tables["OUT_LANE"];            //완성 차트Lane, 무지부, Top Back표시 테이블
                        DataTable dtHead = bizResult.Tables["OUT_HEAD"];            //완성 차트 전체 길이 표시 테이블
                        DataTable dtDefect = bizResult.Tables["OUT_DEFECT"];        //완성 차트 불량정보 표시 테이블
                        DataTable dtGauge = bizResult.Tables["OUT_GAUGE"];          //완성 차트 두께 측정 데이터 표시 테이블

                        #region [투입 차트 영역]

                        if (CommonVerify.HasTableRow(dtInHead))
                        {
                            GetInOutProcessMaxLength(dtInHead, ChartConfigurationType.Input);
                            InitializeChart(chartInput);
                            DrawStartEndYAxis(dtInHead, ChartConfigurationType.Input);
                        }

                        if (CommonVerify.HasTableRow(dtInLane))
                        {
                            DrawInputTopBackDispaly(dtInLane);
                            DrawLane(dtInLane, ChartConfigurationType.Input);
                            DrawWinderDirection(dtInLane);
                            DisplayTabLocation(dtInLane, ChartConfigurationType.Input);
                        }

                        if (CommonVerify.HasTableRow(dtInGauge))
                        {
                            dtInGauge.AsEnumerable()
                            .Select(row =>
                            {
                                switch (row.Field<string>("SCAN_COLRMAP"))
                                {
                                    case "Ch":
                                        row.SetField("EQPT_MEASR_PSTN_NAME", "데이터 미측정");
                                        break;
                                    case "Non":
                                        row.SetField("EQPT_MEASR_PSTN_NAME", "데이터 미수신");
                                        break;
                                    case "Err":
                                        row.SetField("EQPT_MEASR_PSTN_NAME", "데이터 오류");
                                        break;
                                }
                                return row;
                            }).CopyToDataTable();

                            DrawGauge(dtInGauge, ChartConfigurationType.Input);
                            //if (string.Equals(_processCode, Process.COATING)) DrawMergeLot(dtInGauge);
                        }

                        if (CommonVerify.HasTableRow(dtInDefect))
                        {
                            DrawDefect(dtInDefect, ChartConfigurationType.Input);
                            DrawDefectLegend(dtInDefect, ChartConfigurationType.Input);
                        }
                        _dtBaseDefectInput = dtInDefect.Copy();

                        #endregion

                        #region [완성 차트 영역]
                        if (CommonVerify.HasTableRow(dtHead))
                        {
                            GetInOutProcessMaxLength(dtHead, ChartConfigurationType.Output);
                            InitializeChart(chartOutput);
                            DrawStartEndYAxis(dtHead, ChartConfigurationType.Output);
                        }

                        if (CommonVerify.HasTableRow(dtLane))
                        {
                            DrawLane(dtLane, ChartConfigurationType.Output);
                            DisplayTabLocation(dtLane, ChartConfigurationType.Output);
                        }

                        if (CommonVerify.HasTableRow(dtGauge))
                        {
                            DrawGauge(dtGauge, ChartConfigurationType.Output);
                            DrawThickness(dtGauge, ChartConfigurationType.Output);
                            _dtOutGauge = dtGauge;
                            DrawLaneLegend();
                        }


                        if (CommonVerify.HasTableRow(dtDefect))
                        {
                            DrawDefect(dtDefect, ChartConfigurationType.Output);
                            DrawDefectLegend(dtDefect, ChartConfigurationType.Output);
                        }

                        _dtBaseDefectOutput = dtDefect.Copy();

                        tbInputCore.Text = "CORE";
                        tbOutputCore.Text = "CORE";

                        DrawScrapForInLot();    //nathan 2024.04.09 OUT LOT의 Scrap 범위를 투입 LOT 에 표시

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

        private void GetCboUserConf(ref string mPoint_top, ref string mPstn_top, ref string mPoint_back, ref string mPstn_back)
        {
            try
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
                dr["CONF_TYPE"] = "USER_CONFIG_CBO";
                dr["CONF_KEY1"] = "LGC.GMES.MES.COM001.COM001_ROLLMAP_COATER"; // 코터 랏 화면 메뉴
                dr["CONF_KEY2"] = "cboMeasurementTop";
                dr["CONF_KEY3"] = "cboMeasurementBack";
                //dr["USER_CONF01"] = cboTop.SelectedIndex.ToString();
                //dr["USER_CONF02"] = cboBack.SelectedIndex.ToString();
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow drConf in dtResult.Rows)
                    {
                        // 사용자가 전에 설정한 콤보 박스대로 설정
                        mPoint_top = dtResult.Rows[0]["USER_CONF01"].ToString();
                        mPoint_back = dtResult.Rows[0]["USER_CONF02"].ToString();

                        mPstn_top = Convert.ToInt16(dtResult.Rows[0]["USER_CONF01"]) % 2 == 0 ? "1" : "2";
                        mPstn_back = Convert.ToInt16(dtResult.Rows[0]["USER_CONF02"]) % 2 == 0 ? "1" : "2";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetUserConfigurationControl()
        {
            const string bizRuleName = "BR_SET_USER_CONF_INFO";
            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("WRK_TYPE", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("CONF_TYPE", typeof(string));
            inTable.Columns.Add("CONF_KEY1", typeof(string));
            inTable.Columns.Add("CONF_KEY2", typeof(string));
            inTable.Columns.Add("CONF_KEY3", typeof(string));
            inTable.Columns.Add("USER_CONF01", typeof(string));
            inTable.Columns.Add("USER_CONF02", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["WRK_TYPE"] = "SAVE";
            dr["USERID"] = LoginInfo.USERID;
            dr["CONF_TYPE"] = "USER_CONFIG_CONTROL";
            dr["CONF_KEY1"] = this.ToString();
            dr["CONF_KEY2"] = rdoRelativeCoordinates.Name;
            dr["CONF_KEY3"] = rdoAbsolutecoordinates.Name;
            dr["USER_CONF01"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : string.Empty;
            dr["USER_CONF02"] = rdoAbsolutecoordinates.IsChecked != null && (bool)rdoAbsolutecoordinates.IsChecked ? "1" : string.Empty;

            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) => { });
        }

        private void GetUserConfigurationControl()
        {
            const string bizRuleName = "BR_SET_USER_CONF_INFO";

            DataTable inTable = new DataTable("INDATA");
            inTable.Columns.Add("WRK_TYPE", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("CONF_TYPE", typeof(string));
            inTable.Columns.Add("CONF_KEY1", typeof(string));
            inTable.Columns.Add("CONF_KEY2", typeof(string));
            inTable.Columns.Add("CONF_KEY3", typeof(string));


            DataRow dr = inTable.NewRow();
            dr["WRK_TYPE"] = "SELECT";
            dr["USERID"] = LoginInfo.USERID;
            dr["CONF_TYPE"] = "USER_CONFIG_CONTROL";
            dr["CONF_KEY1"] = this.ToString();
            dr["CONF_KEY2"] = rdoRelativeCoordinates.Name;
            dr["CONF_KEY3"] = rdoAbsolutecoordinates.Name;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (!string.IsNullOrEmpty(dtResult.Rows[0]["USER_CONF01"].GetString()))
                {
                    rdoRelativeCoordinates.IsChecked = true;
                }
                if (!string.IsNullOrEmpty(dtResult.Rows[0]["USER_CONF02"].GetString()))
                {
                    rdoAbsolutecoordinates.IsChecked = true;
                }
            }
        }


        /// <summary>
        /// 차트 상단 시작위치 종료위치 표시 및 chartInput 차트 인경우 Cut, MergeLot 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="processCode"></param>
        private void DrawStartEndYAxis(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            //chartInput, chartOutput 상단 Rewinder 길이 표시
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select
new
{
    StartPosition = t.Field<decimal?>("STRT_PSTN"),
    EndPosition = t.Field<decimal>("END_PSTN")
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

            /*
            // chartInput 차트 상단영역 Cut 표시
            var querycut = (from t in dt.AsEnumerable()
                            where t.Field<string>("EQPT_MEASR_PSTN_ID") == "CUT_PSTN"
                            select
                            new
                            {
                                StartPosition = t.Field<decimal?>("CUM_STRT_PSTN"),
                                EndPosition = t.Field<decimal>("CUM_END_PSTN")
                            }).ToList();

            if (querycut.Any())
            {
                DataTable dtLength = new DataTable();
                dtLength.Columns.Add("RAW_END_PSTN", typeof(string));
                dtLength.Columns.Add("SOURCE_Y_PSTN", typeof(double));
                dtLength.Columns.Add("FONTSIZE", typeof(double));

                foreach (var item in querycut)
                {
                    DataRow newLength = dtLength.NewRow();
                    newLength["RAW_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                    newLength["SOURCE_Y_PSTN"] = 100;
                    newLength["FONTSIZE"] = 11;
                    dtLength.Rows.Add(newLength);
                }

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtLength);
                ds.XValueBinding = new Binding("RAW_END_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartLength"] as DataTemplate;
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
            */

            // chartInput 차트 상단영역 LOT Cut 표시
            var querycut = (from t in dt.AsEnumerable()
                            where t.Field<string>("EQPT_MEASR_PSTN_ID") == "CUT_PSTN"
                            select
                            new
                            {
                                StartPosition = t.Field<decimal?>("CUM_STRT_PSTN"),
                                EndPosition = t.Field<decimal>("CUM_END_PSTN")
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



            // chartInput 차트 영역 상단 LOT MergeLot 영역 표시
            var queryMergeLot = (from t in dt.AsEnumerable()
                                 where t.Field<string>("EQPT_MEASR_PSTN_ID") == "MERGE_PSTN"
                                 select
new
{
    InputLotId = t.Field<string>("INPUT_LOTID"),
    StartPosition = t.Field<decimal>("CUM_STRT_PSTN"),
    EndPosition = t.Field<decimal>("CUM_END_PSTN")
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
                    drMergeLot["Y_PSTN"] = 100;
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
        }

        /// <summary>
        /// Input, Output 차트의 Lane 표시 (무지부 유지부 색상 표현 및 Top, Back의 Lane 표시) 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawLane(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            //무지부 색상
            var convertFromString = ColorConverter.ConvertFromString("#D5D5D5");
            const double axisXnear = 0;                 //x축 시작점
            double axisXfar = Equals(chartConfigurationType, ChartConfigurationType.Input) ? _xInputMaxLength : _xOutputMaxLength;     //x축 종료점

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // 무지부 Lane 지정
                if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Plain")
                {
                    AlarmZone alarmZone = new AlarmZone();
                    alarmZone.Near = axisXnear;
                    alarmZone.Far = axisXfar;
                    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                    alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    chart.Data.Children.Add(alarmZone);
                }

                // 유지부 BackGround 지정
                else if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Coat")
                {
                    // 1. 유지부 BackGround 지정
                    AlarmZone alarmZone = new AlarmZone();
                    alarmZone.Near = axisXnear;
                    alarmZone.Far = axisXfar;
                    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                    alarmZone.ConnectionStroke = new SolidColorBrush(Colors.Transparent);
                    alarmZone.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    chart.Data.Children.Add(alarmZone);
                }
            }

            var queryLane = (from t in dt.AsEnumerable() where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" select new { LaneNo = t.Field<string>("LANE_NO_CUR") }).Distinct().ToList();
            int laneCount = queryLane.Count;

            _dtLane?.Clear();
            if (queryLane.Any())
            {
                foreach (var item in queryLane)
                {
                    DataRow dr = _dtLane.NewRow();
                    dr["LANE_NO"] = item.LaneNo;
                    _dtLane.Rows.Add(dr);
                }
            }

            var queryTopBack = (from t in dt.AsEnumerable() select new { TopBackFlag = t.Field<string>("T_B_FLAG") }).Distinct().ToList();
            int topBackCount = queryTopBack.Count;

            //Top, Back 표현
            if (queryTopBack.Any())
            {
                foreach (var item in queryTopBack)
                {
                    /*
                    var queryTopBackRate = dt.AsEnumerable()
                        .Where(a => a.Field<string>("T_B_FLAG") == item.TopBackFlag)
                        .GroupBy(g => new { TopBackFlag = g.Field<string>("T_B_FLAG")})
                        .Select(s => new
                        {
                            TopBackFlag = s.Key.TopBackFlag,
                            FirstTopBackRate = s.Max(z => z.Field<decimal>("Y_PSTN_STRT_CUM_RATE")),
                            LastTopBackRate = s.Min(z => z.Field<decimal>("Y_PSTN_END_CUM_RATE"))
                        }).FirstOrDefault();

                    if (queryTopBackRate != null)
                    {
                        DrawTopBackText(item.TopBackFlag, queryTopBackRate.FirstTopBackRate.GetDouble(), queryTopBackRate.LastTopBackRate.GetDouble(), chartConfigurationType);
                    }
                    */
                    // Top, Back별 Lane 표시
                    if (queryLane.Any())
                    {
                        foreach (var itemLane in queryLane)
                        {
                            var queryTopBackLaneRate = dt.AsEnumerable()
                                .Where(x => x.Field<string>("T_B_FLAG") == item.TopBackFlag
                                && x.Field<string>("LANE_NO_CUR") == itemLane.LaneNo && x.Field<string>("LOC2") != "XX")
                                .GroupBy(g => new { TopBackFlag = g.Field<string>("T_B_FLAG"), LaneNo = g.Field<string>("LANE_NO_CUR") })
                                .Select(s => new
                                {
                                    TopBackFlag = s.Key.TopBackFlag,
                                    LaneNo = s.Key.LaneNo,
                                    FirstTopBackLaneRate = s.Min(z => z.Field<decimal>("Y_PSTN_STRT_RATE")),
                                    LastTopBackLaneRate = s.Max(z => z.Field<decimal>("Y_PSTN_END_RATE"))
                                }).FirstOrDefault();

                            if (queryTopBackLaneRate != null)
                            {
                                DrawLaneText(queryTopBackLaneRate.FirstTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LastTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LaneNo, chartConfigurationType);
                            }

                        }
                    }
                }
            }



            /*
            var query = (from t in dt.AsEnumerable() where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" select new { LaneNo = t.Field<string>("LANE_NO_CUR") }).Distinct().ToList();
            int laneCount = query.Count;

            #region [동적으로 Lane 및 Top,Back 표현을 위해 Lane별, Lane별 Top,Back의 Min, Max 값을 가져와 나눔처리]
            // Lane Text 표시
            if (query.Any())
            {
                foreach (var item in query)
                {
                    var queryLane = dt.AsEnumerable().Where(s => s.Field<string>("LANE_NO_CUR") == item.LaneNo && s.Field<string>("LOC2") != "XX").GroupBy(b => b.Field<string>("LANE_NO_CUR"))
                        .Select(c => new
                        {
                            LaneNo = c.Key,
                            FirstLaneRate = c.Max(z => z.Field<decimal>("Y_PSTN_END_CUM_RATE")),
                            LastLaneRate = c.Min(z => z.Field<decimal>("Y_PSTN_END_CUM_RATE")),
                        }).FirstOrDefault();

                    if (queryLane != null)
                    {
                        DrawLaneText(queryLane.FirstLaneRate.GetDouble(), queryLane.LastLaneRate.GetDouble(), queryLane.LaneNo, chartConfigurationType);
                    }
                }
            }

            //Lane별 Top, Back 그룹핑
            var queryTopBack = dt.AsEnumerable().Where(a => a.Field<string>("LANE_NO_CUR") != null && a.Field<string>("LOC2") != "XX").GroupBy(g => new{TopBackFlag = g.Field<string>("T_B_FLAG"),LaneNo = g.Field<string>("LANE_NO_CUR")})
                .Select(s => new{TopBackFlag = s.Key.TopBackFlag,LaneNo = s.Key.LaneNo}).ToList();

            // Top, Back 표시
            if (queryTopBack.Any())
            {
                foreach (var item in queryTopBack)
                {
                    var queryTopBackRate = dt.AsEnumerable()
                        .Where(a => a.Field<string>("LANE_NO_CUR") == item.LaneNo && a.Field<string>("T_B_FLAG") == item.TopBackFlag && a.Field<string>("LANE_NO_CUR") != null && a.Field<string>("LOC2") != "XX")
                        .GroupBy(g => new{TopBackFlag = g.Field<string>("T_B_FLAG"),LaneNo = g.Field<string>("LANE_NO_CUR")})
                        .Select(s => new
                        {
                            LaneNo = s.Key.LaneNo,
                            TopBackFlag = s.Key.TopBackFlag,
                            FirstTopBackRate = s.Max(z => z.Field<decimal>("Y_PSTN_END_CUM_RATE")),
                            LastTopBackRate = s.Min(z => z.Field<decimal>("Y_PSTN_END_CUM_RATE"))
                        }).FirstOrDefault();

                    if (queryTopBackRate != null)
                    {
                        DrawTopBackText(item.TopBackFlag, queryTopBackRate.FirstTopBackRate.GetDouble(), queryTopBackRate.LastTopBackRate.GetDouble(), chartConfigurationType);
                    }
                }
            }
            */

        }

        private void DisplayTabLocation(DataTable dtTab, ChartConfigurationType chartConfigurationType)
        {
            if (CommonVerify.HasTableRow(dtTab) && dtTab.Columns.Contains("TAB"))
            {
                bool isupper = false;
                bool islower = false;

                if (dtTab.AsEnumerable().Any(x => !string.IsNullOrEmpty(x.Field<string>("TAB")) && x.Field<string>("TAB") == "1" && x.Field<string>("COATING_PATTERN") == "Plain"))
                {
                    // 1. 유지부에 Tab 있는 경우 제외, 2. Lane 이 다수인 경우 제외?, 3. 유지부 상단 하단에 Tab 값이 1 인경우 upper, lower 모두 표시
                    var query = (from t in dtTab.AsEnumerable()
                                 where !string.IsNullOrEmpty(t.Field<string>("TAB"))
  && t.Field<string>("TAB") == "1"
  && t.Field<string>("COATING_PATTERN") == "Plain"
                                 select new { LaneNo = t.Field<string>("LANE_NO_CUR"), Tab = t.Field<string>("TAB") }).Distinct().ToList();
                    if (query.Count > 1)
                    {
                        // Lane 이 다수인 경우 Tab 표시하지 않음.
                        return;
                    }
                    else
                    {
                        var queryTab = (from t in dtTab.AsEnumerable()
                                        where !string.IsNullOrEmpty(t.Field<string>("TAB")) && t.Field<string>("TAB") == "1" && t.Field<string>("COATING_PATTERN") == "Plain"
                                        select t).ToList();

                        if (queryTab.Any())
                        {
                            foreach (var item in queryTab)
                            {
                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.Field<Int16>("CNT") < item["CNT"].GetInt()))
                                {
                                    islower = true;
                                }

                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.Field<Int16>("CNT") > item["CNT"].GetInt()))
                                {
                                    isupper = true;
                                }
                            }

                            if (chartConfigurationType == ChartConfigurationType.Input)
                            {
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

        /// <summary>
        /// 차트 최상단 UnWinder ReWinder 권취방향 표시
        /// </summary>
        /// <param name="dtLength"></param>
        private void DrawWinderDirection(DataTable dtLength)
        {
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

            chart.View.AxisY.Min = 0;
            chart.View.AxisY.Max = 100;
            chart.View.AxisX.Min = 0;
            chart.View.AxisX.Max = dtLength.AsEnumerable().ToList().Max(r => r["TOTAL_WIDTH2"].GetDouble()).GetDouble();

            DataTable dt = new DataTable();
            dt.Columns.Add("SOURCE_END_PSTN", typeof(double));
            dt.Columns.Add("ARROW_PSTN", typeof(double));
            dt.Columns.Add("SOURCE_Y_PSTN", typeof(double));
            dt.Columns.Add("ARROW_Y_PSTN", typeof(double));
            dt.Columns.Add("COLORMAP", typeof(string));
            dt.Columns.Add("CIRCLENAME", typeof(string));
            dt.Columns.Add("WND_DIRCTN", typeof(string));
            dt.Columns.Add("TOOLTIP", typeof(string));

            double arrowYPosition;
            double arrowPosition;
            double arrowValue = dtLength.AsEnumerable().ToList().Max(r => r["TOTAL_WIDTH2"].GetDouble()).GetDouble() * 0.025;

            //Rewinder 권취 방향 (RW_DIRCTN "1" 이면 하권취 "0" 이면 상권취)
            arrowYPosition = dtLength.Rows[0]["RW_DIRCTN"].GetInt() == 1 ? 0 : 60;
            arrowPosition = dtLength.Rows[0]["TOTAL_WIDTH2"].GetDouble() - arrowValue;

            DataRow dr = dt.NewRow();
            dr["SOURCE_END_PSTN"] = dtLength.Rows[0]["TOTAL_WIDTH2"];
            dr["ARROW_PSTN"] = arrowPosition;
            dr["SOURCE_Y_PSTN"] = 0;
            dr["ARROW_Y_PSTN"] = arrowYPosition;
            dr["CIRCLENAME"] = "RW";
            dr["COLORMAP"] = "#000000";
            dt.Rows.Add(dr);

            //UnWinder 권출 방향 (UW_DIRCTN "1" 이면 상권출 "0" 이면 하권출)
            arrowYPosition = dtLength.Rows[0]["UW_DIRCTN"].GetInt() == 1 ? 60 : 0;
            arrowPosition = arrowValue;

            DataRow newRow = dt.NewRow();
            newRow["SOURCE_END_PSTN"] = 0;
            newRow["ARROW_PSTN"] = arrowPosition;
            newRow["SOURCE_Y_PSTN"] = 0;
            newRow["ARROW_Y_PSTN"] = arrowYPosition;
            newRow["CIRCLENAME"] = "UW";
            newRow["COLORMAP"] = "#000000";
            dt.Rows.Add(newRow);

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
        }

        /// <summary>
        /// Input, Output 차트의 불량정보 표시(불량 항목별 메소드를 별도 호출하여 불량정보 표시)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawDefect(DataTable dt, ChartConfigurationType chartConfigurationType, bool isReDraw = false)
        {
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

            //// TableName 은 chart DataSeries의 Tag 설정하여 범례 텍스트 클릭 시 선택영역 차트의 DataSeries Remove 역할을 하기 위함.
            //if (CommonVerify.HasTableRow(dtTop))
            //{
            //    dtTop.TableName = dtTop.Rows[0]["ABBR_NAME"].GetString();
            //    DrawDefectVisionSurface(dtTop, chartConfigurationType);
            //}


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

            var queryOverLayVision = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
            DataTable dtOverLayVision = queryOverLayVision.Any() ? queryOverLayVision.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtOverLayVision))
            {
                dtOverLayVision.TableName = "INS_OVERLAY_VISION_NG";
                DrawDefectOverLayVision(dtOverLayVision, chartConfigurationType);
            }

            if (!isReDraw)
            {
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
                    DrawDefectTagSection(dtTagSectionLane, chartConfigurationType);
                }

                var queryTagSpot = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dt.Clone();
                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    dtTagSpot.TableName = "TAG_SPOT";
                    DrawDefectTagSpot(dtTagSpot, chartConfigurationType);
                }

                var queryMark = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("MARK")).ToList();
                DataTable dtMark = queryMark.Any() ? queryMark.CopyToDataTable() : dt.Clone();
                if (CommonVerify.HasTableRow(dtMark))
                {
                    dtMark.TableName = "MARK";
                    DrawDefectMark(dtMark, chartConfigurationType);
                }

                var queryRewinderScrap = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("RW_SCRAP")).ToList();
                DataTable dtRwScrap = queryRewinderScrap.Any() ? queryRewinderScrap.CopyToDataTable() : dt.Clone();
                if (CommonVerify.HasTableRow(dtRwScrap))
                {
                    dtRwScrap.TableName = "RW_SCRAP";
                    DrawDefectRewinderScrap(dtRwScrap, chartConfigurationType);
                }
            }
        }

        /// <summary>
        /// Input, Output 차트 영역의 불량 정보의 범례를 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
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

        /// <summary>
        /// Input, Output 차트 영역의 두께, 게이지 데이터 표시(PET, SCRAP, RW_SCRAP 도 해당 메소드에서 표시 함.)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawGauge(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            //두께 차트 영역에 Gauge 데이터 외의 설비 측정 위치 아이디는 제외 하여 표시 함.
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "TAG_SECTION", "SCRAP_SECTION" };
            //var queryGauge = dt.AsEnumerable().Where(x => !gaugeExceptions.Contains(x.Field<string>("EQPT_MEASR_PSTN_ID"))).Select(y => y);
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
                AlarmZone alarmZone = new AlarmZone();
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dtGauge.Rows[i]["COLORMAP"]));

                alarmZone.Near = dtGauge.Rows[i]["ADJ_STRT_PSTN"].GetDouble();
                alarmZone.Far = dtGauge.Rows[i]["ADJ_END_PSTN"].GetDouble();
                alarmZone.LowerExtent = dtGauge.Rows[i]["CHART_Y_END_CUM_RATE"].GetDouble();
                alarmZone.UpperExtent = dtGauge.Rows[i]["CHART_Y_STRT_CUM_RATE"].GetDouble();
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

                            content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                         "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                                         scanMinValueContent + scaneMaxValueContent +
                                         "ColorMap : " + Util.NVC(dtGauge.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                         "Offset : " + Util.NVC(dtGauge.Rows[x]["SCAN_OFFSET"]);

                        }
                        else
                        {
                            content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                         "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                                         "ColorMap : " + Util.NVC(dtGauge.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                         "Offset : " + Util.NVC(dtGauge.Rows[x]["SCAN_OFFSET"]);
                        }

                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                if (Equals(chartConfigurationType, ChartConfigurationType.Input))
                    chartInput.Data.Children.Add(alarmZone);
                else
                    chartOutput.Data.Children.Add(alarmZone);
            }
            //조성근 2025.04.04 ScrapSection 적용 - start
            var queryScrapSetion = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("SCRAP_SECTION")).ToList();
            DataTable dtScrapSection = queryScrapSetion.Any() ? queryScrapSetion.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtScrapSection))
            {
                dtScrapSection.TableName = "SCRAP_SECTION";
                DrawScrapSectionOutlot(dtScrapSection, chartConfigurationType);
            }
            //조성근 2025.04.04 ScrapSection 적용 - end
            // PET
            var queryPet = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("PET")).ToList();
            DataTable dtPet = queryPet.Any() ? queryPet.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtPet))
            {
                dtPet.TableName = "PET";
                DrawPetScrap(dtPet, chartConfigurationType);
            }
            // SCRAP
            var queryScrap = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("SCRAP")).ToList();
            DataTable dtScrap = queryScrap.Any() ? queryScrap.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtScrap))
            {
                dtScrap.TableName = "SCRAP";
                DrawPetScrap(dtScrap, chartConfigurationType);
            }
            // Rewinder SCRAP
            var queryRewinderScrap = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("RW_SCRAP")).ToList();
            DataTable dtRwScrap = queryRewinderScrap.Any() ? queryRewinderScrap.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtRwScrap))
            {
                DrawRewinderScrap(dtRwScrap, chartConfigurationType);
            }
            // Tag Section  NG 마킹
            var queryTagSection = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
            DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtTagSection))
            {
                DrawDefectTagSection(dtTagSection, chartConfigurationType);
            }

            // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
            var queryTagSectionLane = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
            DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dt.Clone();
            dtTagSectionLane.TableName = "TAG_SECTION_LANE";
            //_dtTagSection = dtTagSectionLane;

            if (CommonVerify.HasTableRow(dtTagSectionLane))
            {
                DrawDefectTagSection(dtTagSectionLane, chartConfigurationType);
            }

        }

        /// <summary>
        /// chartThickness 차트의 두께 측정데이터 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawThickness(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "TAG_SECTION", "COT_WIDTH_LEN_BACK", "SCRAP_SECTION" };
            var queryGauge = dt.AsEnumerable().Where(o => !gaugeExceptions.Contains(o.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList();
            dt = queryGauge.Any() ? queryGauge.CopyToDataTable() : null;

            if (!CommonVerify.HasTableRow(dt)) return;

            chartThickness.View.AxisX.Min = 0;
            chartThickness.View.AxisX.Max = _xOutputMaxLength;
            chartThickness.ChartType = ChartType.Line;

            if (!dt.Columns.Contains("ADJ_AVG_VALUE"))
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "ADJ_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
            }
            foreach (DataRow row in dt.Rows)
            {
                row["ADJ_AVG_VALUE"] = (row["ADJ_STRT_PSTN"].GetDouble() + row["ADJ_END_PSTN"].GetDouble()) / 2;
            }
            dt.AcceptChanges();

            if (CommonVerify.HasTableRow(dt))
            {
                DrawThicknessLegend(dt.Copy(), _dtLineLegend.Copy());

                DataTable dtLineLegend = _dtLineLegend.Select().AsEnumerable().OrderByDescending(o => o.Field<int>("NO")).CopyToDataTable();
                DataRow[] newRows = dtLineLegend.Select();


                var queryLaneInfo = (from t in _dtLane.AsEnumerable()
                                     join x in _dtLaneLegend.AsEnumerable()
                                         on t.Field<string>("LANE_NO") equals x.Field<string>("LANE_NO")
                                     select new
                                     {
                                         LaneNo = t.Field<string>("LANE_NO"),
                                         LaneColor = x.Field<string>("COLORMAP")
                                     }
                    ).ToList();

                foreach (DataRow row in newRows)
                {
                    string valueText = "VALUE_" + row["VALUE"].GetString();
                    if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                    {
                        if (dt.AsEnumerable().All(p => p.Field<decimal?>(valueText) == null)) continue;

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
                    }
                }


                if (queryLaneInfo.Any())
                {
                    foreach (var item in queryLaneInfo)
                    {
                        if (!CommonVerify.HasTableRow(_dtLane)) continue;

                        var queryLane = _dtLane.AsEnumerable().Where(where => @where.Field<string>("LANE_NO").Equals(item.LaneNo)).ToList();
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

                foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdThickness))
                {
                    InitializeChartView(c1Chart);
                }
            }
        }

        /// <summary>
        /// chartThickness 차트의 Lane 범례 표시
        /// </summary>
        private void DrawLaneLegend()
        {
            if (grdLaneLegend.ColumnDefinitions.Count > 0) grdLaneLegend.ColumnDefinitions.Clear();
            if (grdLaneLegend.RowDefinitions.Count > 0) grdLaneLegend.RowDefinitions.Clear();

            for (int x = 0; x < 2; x++)
            {
                ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                grdLaneLegend.ColumnDefinitions.Add(gridColumn);
            }

            grdLaneLegend.Children.Clear();

            var query = (from t in _dtLane.AsEnumerable()
                         join u in _dtLaneLegend.AsEnumerable()
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

                    Ellipse ellipseLane = CreateEllipse(HorizontalAlignment.Center,
                                                        VerticalAlignment.Center,
                                                        12,
                                                        12,
                                                        convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                        1,
                                                        new Thickness(2, 2, 2, 2),
                                                        null,
                                                        null);

                    TextBlock textBlockLane = CreateTextBlock("Lane " + item.LaneNo,
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

        private void DrawMergeLot(DataTable dt)
        {
            var query = (from t in dt.AsEnumerable()
                         select new { AdjustmentLotId = t.Field<string>("ADJ_LOTID") }).Distinct().ToList();

            if (query.Count() > 1)
            {
                DataTable dtMerge = new DataTable();
                dtMerge.Columns.Add(new DataColumn() { ColumnName = "ADJ_LOTID", DataType = typeof(string) });
                dtMerge.Columns.Add(new DataColumn() { ColumnName = "ADJ_STRT_PSTN", DataType = typeof(double) });
                dtMerge.Columns.Add(new DataColumn() { ColumnName = "ADJ_END_PSTN", DataType = typeof(double) });
                dtMerge.Columns.Add(new DataColumn() { ColumnName = "Y_PSTN", DataType = typeof(double) });
                dtMerge.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtMerge.Columns.Add(new DataColumn() { ColumnName = "ADJ_VISIVILITY", DataType = typeof(string) }); //Collapsed, Visible

                foreach (var item in query)
                {
                    var queryMerge = dt.AsEnumerable().Where(a => a.Field<string>("ADJ_LOTID").Equals(item.AdjustmentLotId))
                        .GroupBy(x => x.Field<string>("ADJ_LOTID"))
                        .Select(y => new
                        {
                            AdjustmentLotId = y.Key,
                            StartPosition = y.Min(z => z.Field<decimal>("ADJ_STRT_PSTN")),
                            EndPosition = y.Max(z => z.Field<decimal>("ADJ_END_PSTN")),
                        }).FirstOrDefault();

                    if (queryMerge != null)
                    {
                        dtMerge.Rows.Add(queryMerge.AdjustmentLotId
                            , queryMerge.StartPosition.GetDouble()
                            , queryMerge.EndPosition.GetDouble()
                            , 220
                            , "LOT ID : " + queryMerge.AdjustmentLotId + " ( " + $"{queryMerge.StartPosition.GetDouble():###,###,###,##0.##}" + " ~ " + $"{queryMerge.EndPosition.GetDouble():###,###,###,##0.##}" + ")"
                            , "Visible");
                    }
                }

                if (CommonVerify.HasTableRow(dtMerge))
                {

                    XYDataSeries ds = new XYDataSeries();
                    ds.ItemsSource = DataTableConverter.Convert(dtMerge);
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

                    chartInput.Data.Children.Add(ds);
                }
            }
        }

        /// <summary>
        /// Input 차트 코터인 경우 Top, Back 표시
        /// </summary>
        /// <param name="dt"></param>
        private void DrawInputTopBackDispaly(DataTable dt)
        {
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

        private void DrawScrapSection(string lotId, string wipseq, string startPosition)
        {
            const string bizRuleName = "DA_PRD_SEL_INPUT_SCRAP_PSTN_RM";
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
                    chartInput.Data.Children.Add(dsscrapSection);
                }
                HiddenLoadingIndicator();
            });

        }

        /// <summary>
        /// 차트 헤더 에 표기될 Rewinder 길이 표시 및 Input 영역 txtInputPosition 에 Top, Back 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void GetInOutProcessMaxLength(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
                         {
                             ProcessName = t.Field<string>("PROCNAME"),
                             EndPosition = t.Field<decimal>("END_PSTN"),
                             ProdQty = t.Field<decimal?>("INPUT_QTY"),
                             GoodQty = t.Field<decimal?>("WIPQTY_ED")
                         }).FirstOrDefault();

            if (query != null)
            {
                if (chartConfigurationType == ChartConfigurationType.Input)
                {
                    tbInput.Text = query.ProcessName;
                    _xInputMaxLength = query.EndPosition.GetDouble();

                    tbInputProdQty.Text = @"P" + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbInputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbInputGoodQty.Text = @"G" + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                    tbInputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                }
                else
                {
                    tbOutput.Text = query.ProcessName;
                    _xOutputMaxLength = query.EndPosition.GetDouble();

                    tbOutputProdQty.Text = @"P" + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbOutputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                    tbOutputGoodQty.Text = @"G" + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                    tbOutputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                }
            }
        }

        private System.Collections.Generic.IEnumerable<double> GetLineValuesSource(DataTable dt, string value)
        {
            try
            {
                var queryCount = _xOutputMaxLength.GetInt() + 1;
                double[] xx = new double[queryCount];
                string valueText = "VALUE_" + value;

                if (dt.AsEnumerable().Any(p => p.Field<decimal?>(valueText) != null))
                {
                    double agvValue = 0;
                    var query = (from t in dt.AsEnumerable() where t.Field<decimal?>(valueText) != null select new { Valuecol = t.Field<decimal>(valueText) }).ToList();
                    if (query.Any())
                        agvValue = query.Max(r => r.Valuecol).GetDouble();

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

        private void DrawThicknessLegend(DataTable dt, DataTable dtLegend)
        {
            try
            {
                if (grdLineLegend.ColumnDefinitions.Count > 0) grdLineLegend.ColumnDefinitions.Clear();
                if (grdLineLegend.RowDefinitions.Count > 0) grdLineLegend.RowDefinitions.Clear();

                grdLineLegend.Children.Clear();

                DataRow[] newRows = dtLegend.Select();
                DataTable copyTable = dtLegend.Clone();
                copyTable.Columns.Add(new DataColumn() { ColumnName = "VALUE_AVG", DataType = typeof(double) });

                foreach (DataRow row in newRows)
                {
                    string valueText = "VALUE_" + row["VALUE"];

                    if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                    {
                        if (dt.AsEnumerable().Any(o => o.Field<decimal?>(valueText) != null))
                        {
                            double agvValue = 0;
                            var query = (from t in dt.AsEnumerable() where t.Field<decimal?>(valueText) != null select new { Valuecol = t.Field<decimal>(valueText) }).ToList();
                            if (query.Any())
                                agvValue = query.Max(r => r.Valuecol).GetDouble();

                            DataRow dr = copyTable.NewRow();
                            dr["COLORMAP"] = row["COLORMAP"];
                            dr["VALUE"] = row["VALUE"];
                            dr["VALUE_AVG"] = agvValue;
                            copyTable.Rows.Add(dr);
                        }
                    }
                }

                for (int i = 0; i < copyTable.Rows.Count; i++)
                {
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

                    TextBlock lineLegendDescription = CreateTextBlock(rowValue,
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
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// chartInput, chartOutput 영역에 Top, Back 표시
        /// </summary>
        /// <param name="topBackFlag"></param>
        /// <param name="firstlaneRate"></param>
        /// <param name="lastRate"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawTopBackText(string topBackFlag, double firstlaneRate, double lastRate, ChartConfigurationType chartConfigurationType)
        {
            DataTable dtTopBack = new DataTable();
            dtTopBack.Columns.Add("SOURCE_STRT_PSTN", typeof(string));
            dtTopBack.Columns.Add("SOURCE_Y_PSTN", typeof(string));
            dtTopBack.Columns.Add("FLAG", typeof(string));

            C1Chart c1Chart;
            double startPosition;

            if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            {
                c1Chart = chartInput;
                startPosition = _xInputMaxLength - (_xInputMaxLength * 0.01);
            }
            else
            {
                c1Chart = chartOutput;
                startPosition = _xOutputMaxLength - (_xOutputMaxLength * 0.01);
            }

            DataRow dr = dtTopBack.NewRow();
            dr["SOURCE_STRT_PSTN"] = startPosition;
            dr["SOURCE_Y_PSTN"] = (firstlaneRate + lastRate) / 2 - 1;
            dr["FLAG"] = topBackFlag.Equals("T") ? "Top" : "Back";
            dtTopBack.Rows.Add(dr);

            XYDataSeries dsTopBack = new XYDataSeries();
            dsTopBack.ItemsSource = DataTableConverter.Convert(dtTopBack);
            dsTopBack.XValueBinding = new Binding("SOURCE_STRT_PSTN");
            dsTopBack.ValueBinding = new Binding("SOURCE_Y_PSTN");
            dsTopBack.ChartType = ChartType.XYPlot;
            dsTopBack.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsTopBack.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsTopBack.PointLabelTemplate = grdMain.Resources["chartTopBackFlag"] as DataTemplate;
            dsTopBack.Margin = new Thickness(0, 0, 0, 0);

            dsTopBack.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };

            c1Chart.Data.Children.Add(dsTopBack);
        }

        /// <summary>
        /// chartInput, chartOutput 영역에 Lane 표시
        /// </summary>
        /// <param name="firstlaneRate"></param>
        /// <param name="lastlaneRate"></param>
        /// <param name="laneNo"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawLaneText(double firstlaneRate, double lastlaneRate, string laneNo, ChartConfigurationType chartConfigurationType)
        {
            DataTable dtLane = new DataTable();
            dtLane.Columns.Add("SOURCE_STRT_PSTN", typeof(string));
            dtLane.Columns.Add("SOURCE_Y_PSTN", typeof(string));
            dtLane.Columns.Add("LANEINFO", typeof(string));

            C1Chart c1Chart;
            double startPosition;

            if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            {
                c1Chart = chartInput;
                startPosition = _xInputMaxLength - (_xInputMaxLength * 0.01);
            }
            else
            {
                c1Chart = chartOutput;
                startPosition = _xOutputMaxLength - (_xOutputMaxLength * 0.01);
            }


            DataRow dr = dtLane.NewRow();
            dr["SOURCE_STRT_PSTN"] = startPosition;
            dr["SOURCE_Y_PSTN"] = (firstlaneRate + lastlaneRate) / 2 - 1;
            dr["LANEINFO"] = "Lane " + laneNo;
            dtLane.Rows.Add(dr);

            XYDataSeries dsLane = new XYDataSeries();
            dsLane.ItemsSource = DataTableConverter.Convert(dtLane);
            dsLane.XValueBinding = new Binding("SOURCE_STRT_PSTN");
            dsLane.ValueBinding = new Binding("SOURCE_Y_PSTN");
            dsLane.ChartType = ChartType.XYPlot;
            dsLane.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsLane.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsLane.PointLabelTemplate = grdMain.Resources["chartLane"] as DataTemplate;
            dsLane.Margin = new Thickness(0, 0, 0, 0);

            dsLane.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };

            c1Chart.Data.Children.Add(dsLane);
        }

        /// <summary>
        /// TOP, BACK 최종 비전 표면불량 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawDefectVisionSurface(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            XYDataSeries ds = new XYDataSeries();
            ds.Tag = dt.TableName;
            ds.ItemsSource = DataTableConverter.Convert(dt);
            //ds.ValueBinding = new Binding("Y_PSTN_CUM_RATE");
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

                    string content = defectName + " [" + ObjectDic.Instance.GetObjectName("길이") + ":" + $"{xposition:###,###,###,##0.##}" + "m" + ", " + ObjectDic.Instance.GetObjectName("폭") + ":" + $"{yposition:###,###,###,##0.##}" + "mm" + "]";
                    ToolTipService.SetToolTip(pe, content);
                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                    ToolTipService.SetShowDuration(pe, 60000);
                }
            };
            c1Chart.Data.Children.Add(ds);
        }

        /// <summary>
        /// TOP,BACK 최종 비전 불량, TOP,BACK 절연코팅 Align 비전 불량 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
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
                        string content = row["ABBR_NAME"] + "[" + $"{startPosition:###,###,###,##0.##}" + "m" + "~" + $"{endPosition:###,###,###,##0.##}" + "m" + "]";
                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };
                c1Chart.Data.Children.Add(alarmZone);
            }
        }

        /// <summary>
        /// 절연 코팅 비전 불량 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
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
                        string content = row["ABBR_NAME"] + "[" + $"{startPosition:###,###,###,##0.##}" + "m" + "~" + $"{endPosition:###,###,###,##0.##}" + "m" + "]";
                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                c1Chart.Data.Children.Add(alarmZone);
            }
        }

        /// <summary>
        /// TAG_SECTION NG 마킹 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawDefectTagSection(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;
            // Start, End 이미지 두번의 표현으로 for문 사용
            for (int x = 0; x < 2; x++)
            {
                DataTable dtTag = MakeTableForDisplay(dt, x == 0 ? ChartDisplayType.TagSectionStart : ChartDisplayType.TagSectionEnd, chartConfigurationType);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTag);
                ds.XValueBinding = x == 0 ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");
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

        //구간불량등록 클릭 이벤트 추가
        private void tbTagSection_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", "구간불량");
                return;
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                return;
            }

            TextBlock textBlock = sender as TextBlock;
            if (textBlock?.Tag == null) return;

            string[] splitItem = textBlock.Tag.GetString().Split(';');

            string startPosition = splitItem[0];
            string endPosition = splitItem[1];
            string collectSeq = splitItem[2];
            string collectType = splitItem[3];

            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[11];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = startPosition;
            parameters[2] = endPosition;
            parameters[3] = _processCode;
            parameters[4] = _equipmentCode;
            parameters[5] = _wipSeq;
            parameters[6] = collectSeq;
            parameters[7] = collectType;
            parameters[8] = false;
            parameters[9] = _xOutputMaxLength;
            parameters[10] = (_isRollMapResultLink && _isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;
            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }

        /// <summary>
        /// TAG_SPOT, Tag 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
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
                //ds.Name = dt.TableName;
                ds.Tag = dt.TableName;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                c1Chart.Data.Children.Add(ds);
            }
        }

        /// <summary>
        /// MARK, 기준점 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
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
                    string content = row["EQPT_MEASR_PSTN_NAME"] + "[" + Util.NVC(row["ROLLMAP_CLCT_TYPE"]) + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_STRT_PSTN"].GetDouble(), row["ADJ_STRT_PSTN"].GetDouble() },
                        ValuesSource = new double[] { row["CHART_Y_STRT_CUM_RATE"].GetDouble(), row["CHART_Y_END_CUM_RATE"].GetDouble() },
                        ConnectionStroke = new SolidColorBrush(Colors.Black),
                        ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                        ToolTip = content,
                        //Name = dt.TableName
                        Tag = dt.TableName
                    });
                }
            }

            //Mark 라벨 설정(마스터 기준점에 대하여 라벨 표시)
            //var queryLabel = dt.AsEnumerable().Where(x => Math.Abs(x.Field<decimal>("CHART_Y_STRT_CUM_RATE") - x.Field<decimal>("CHART_Y_END_CUM_RATE")) >= 100).ToList();
            var queryLabel = dt.AsEnumerable().Where(x => x.Field<decimal?>("CHART_Y_STRT_CUM_RATE") != null && x.Field<decimal?>("CHART_Y_END_CUM_RATE") != null && Math.Abs(x.Field<decimal>("CHART_Y_STRT_CUM_RATE") - x.Field<decimal>("CHART_Y_END_CUM_RATE")) >= 100).ToList();
            DataTable dtLabel = queryLabel.Any() ? MakeTableForDisplay(queryLabel.CopyToDataTable(), ChartDisplayType.MarkLabel, chartConfigurationType) : MakeTableForDisplay(dt.Clone(), ChartDisplayType.MarkLabel, chartConfigurationType);

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

        /// <summary>
        /// RW_SCRAP, RW이후 수동 SCRAP 표시
        /// </summary>
        /// <param name="dtRwScrap"></param>
        /// <param name="chartConfigurationType"></param>
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
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["SOURCE_ADJ_STRT_PSTN"].GetString() + ";" + row["SOURCE_ADJ_END_PSTN"].GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + " ]";

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
                        //Name = dtRwScrap.TableName
                        Tag = dtRwScrap.TableName
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
                //dsPetScrap.Name = dtRwScrap.TableName;
                dsPetScrap.Tag = dtRwScrap.TableName;

                dsPetScrap.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };

                c1Chart.Data.Children.Add(dsPetScrap);
            }
        }

        /// <summary>
        /// PET, PET(이음매) 표시
        /// </summary>
        /// <param name="dtPetScrap"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawPetScrap(DataTable dtPetScrap, ChartConfigurationType chartConfigurationType)
        {
            //DataRow[] drPetScrap = dt.Select("EQPT_MEASR_PSTN_ID = 'PET' OR EQPT_MEASR_PSTN_ID = 'SCRAP' ");
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

        /// <summary>
        /// RW이후 수동 SCRAP
        /// </summary>
        /// <param name="dtRewinderScrap"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawRewinderScrap(DataTable dtRewinderScrap, ChartConfigurationType chartConfigurationType)
        {
            //if (string.Equals(processCode, Process.COATING)) return;
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
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["SOURCE_ADJ_STRT_PSTN"].GetString() + ";" + row["SOURCE_ADJ_END_PSTN"].GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + " ]";

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

        /// <summary>
        /// 불량 범례 표시 및 DescriptionOnPreviewMouseUp 이벤트 생성
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
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
                                    Rectangle rectangleLegend = CreateRectangle(HorizontalAlignment.Center,
                                                                                VerticalAlignment.Center,
                                                                                12,
                                                                                12,
                                                                                convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                                null,
                                                                                1,
                                                                                new Thickness(2, 2, 2, 2),
                                                                                null,
                                                                                null);

                                    TextBlock rectangleDescription = CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
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
                                    Ellipse ellipseLegend = CreateEllipse(HorizontalAlignment.Center,
                                                                          VerticalAlignment.Center,
                                                                          12,
                                                                          12,
                                                                          convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                          1,
                                                                          new Thickness(2, 2, 2, 2),
                                                                          null,
                                                                          null);

                                    TextBlock ellipseDescription = CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
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
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// chartInput, chartOutput x 축 범위 설정
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="chartConfigurationType"></param>
        private void SetScale(double scale, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart;
            Button btnRefresh, btnZoomIn, btnZoomOut;

            if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            {
                c1Chart = chartInput;
                btnRefresh = btnInputRefresh;
                btnZoomIn = btnInputZoomIn;
                btnZoomOut = btnInputZoomOut;

                c1Chart.View.AxisX.Scale = scale;
                btnRefresh.IsCancel = !scale.Equals(1);
                btnZoomIn.IsCancel = scale > 0.002;
                btnZoomOut.IsCancel = scale < 1;
                UpdateScrollbars(chartConfigurationType);
            }
            else
            {
                c1Chart = chartOutput;
                btnRefresh = btnOutputRefresh;
                btnZoomIn = btnOutputZoomIn;
                btnZoomOut = btnOutputZoomOut;

                c1Chart.View.AxisX.Scale = scale;
                btnRefresh.IsCancel = !scale.Equals(1);
                btnZoomIn.IsCancel = scale > 0.002;
                btnZoomOut.IsCancel = scale < 1;
                UpdateScrollbars(chartConfigurationType);
            }
        }

        /// <summary>
        /// chartInput, chartOutput x 축 ScrollBar 설정
        /// </summary>
        /// <param name="chartConfigurationType"></param>
        private void UpdateScrollbars(ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            double sxTop = c1Chart.View.AxisX.Scale;
            AxisScrollBar axisScrollBar = (AxisScrollBar)c1Chart.View.AxisX.ScrollBar;
            axisScrollBar.Visibility = sxTop >= 1.0 ? Visibility.Collapsed : Visibility.Visible;
        }

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

        private void EndUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                if (c1Chart.Name == "chartInput" || c1Chart.Name == "chartOutput" || c1Chart.Name == "chartThickness")
                    c1Chart.View.AxisX.Reversed = true;

                c1Chart.EndUpdate();
            }
        }

        /// <summary>
        /// chartInput, chartOutput에 바인딩 전 ChartDisplayType타입에 따른 Y좌표 생성 및 ToolTip 생성
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartDisplayType"></param>
        /// <param name="chartConfigurationType"></param>
        /// <returns></returns>
        private DataTable MakeTableForDisplay(DataTable dt, ChartDisplayType chartDisplayType, ChartConfigurationType chartConfigurationType)
        {
            var dtBinding = dt.Copy();

            if (!CommonVerify.HasTableRow(dt)) return dtBinding;

            if (chartDisplayType == ChartDisplayType.MarkLabel)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);
                    row["SOURCE_ADJ_Y_PSTN"] = -14;
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[" + row["ROLLMAP_CLCT_TYPE"] + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";
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

                    //row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -8 : -30;
                    row["SOURCE_ADJ_Y_PSTN"] = -40;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + " : " + row["ABBR_NAME"] + " [" + sourceStartPosition + "m]";
                    row["TAG"] = null;
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionStart || chartDisplayType == ChartDisplayType.TagSectionEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });

                ////NFF Lane 불량 정보 표기에 따른 변경 - Start

                //// 2D BCR 계산 제외
                var queryDtBinding = (from t in dtBinding.AsEnumerable()
                                      where (!("TAG_SECTION".Equals(t.Field<string>("EQPT_MEASR_PSTN_ID")) &&
                                      t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty() &&
                                      t.Field<string>("LANE_NO").IsNotEmpty())
                                      )
                                      select t).ToList();
                //// 2D BCR Count 추출
                var query2DBcrWithCountInfoToList =
                    (from t in dt.AsEnumerable()
                     where ("TAG_SECTION".Equals(t.Field<string>("EQPT_MEASR_PSTN_ID")) &&
                     t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty() &&
                     t.Field<string>("ADJ_LANE_NO").IsNotEmpty()
                     )
                     select new
                     {
                         Mark2DBarCodeStr = t.Field<string>("MRK_2D_BCD_STR"),
                         AdjStrtPstn = t.Field<decimal>("ADJ_STRT_PSTN"),
                         AdjEndPstn = t.Field<decimal>("ADJ_END_PSTN")
                     }).GroupBy(n => new { n.Mark2DBarCodeStr, n.AdjStrtPstn, n.AdjEndPstn })
                    .Select(n => new
                    {
                        Mark2DBarCode = n.Key.Mark2DBarCodeStr,
                        Mark2DBarCodeAdjStrtPstn = n.Key.AdjStrtPstn,
                        Mark2DBarCodeAdjEndPstn = n.Key.AdjEndPstn,
                        Mark2DBarCodeCnt = n.Count()
                    }).ToList();


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

                }
                );

                int laneQty = 0;
                string dfct2DBcrStd = "";
                // 완성 Lot의 총 Lane 갯수 및 설비의 구간 불량 2D BCR 규격 정보 조회
                if (CommonVerify.HasTableRow(_dt2DBarcodeInfo))
                {
                    laneQty = Convert.ToUInt16(_dt2DBarcodeInfo.Rows[0]["LANE_QTY"]);
                    dfct2DBcrStd = _dt2DBarcodeInfo.Rows[0]["DFCT_2D_BCR_STD"].GetString();
                }

                //foreach (DataRow row in dtBinding.Rows)
                foreach (DataRow row in queryDtBinding)
                {
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -22 : -31;
                    //row["TAG"] = null;
                    row["TAG"] = chartConfigurationType == ChartConfigurationType.Input ? null : $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                    string mark2DBarcode = string.Empty;
                    if (Util.IsNVC(row["MRK_2D_BCD_STR"]) == false)
                    {
                        mark2DBarcode = row["MRK_2D_BCD_STR"].ToString();
                    }

                    // NFF 설비 특화 로직                   
                    //  - 구 NFF 전체 Lane 불량 / Lane 별 불량 구분 표기 로직 
                    //  - 2D BCR  CODE 의 TAG_SECTION 정보가 있는 경우에만 표기
                    if ("TAG_SECTION".Equals(row["EQPT_MEASR_PSTN_ID"])
                        && "L002".Equals(dfct2DBcrStd.Trim()) //L001: Normal, L002: NFF, L003: 소형(2170)
                        && !"".Equals(row["MRK_2D_BCD_STR"].ToString().Trim())

                        && query2DBcrWithCountInfoToDict.Any()
                        && query2DBcrWithCountInfoToDict.ContainsKey(mark2DBarcode))
                    {
                        decimal adjStrtPstn = Convert.ToDecimal(row["ADJ_STRT_PSTN"]);
                        decimal adjEndPstn = Convert.ToDecimal(row["ADJ_END_PSTN"]);
                        List<Tuple<int, decimal, decimal>> mark2DBarcodeCountInfoList = query2DBcrWithCountInfoToDict[mark2DBarcode];

                        mark2DBarcodeCountInfoList.ForEach(
                            t =>
                            {
                                // 전체 Lane 구간 불량 표기
                                //  - Lot의 전체 Lane 갯수와 롤맵 상의 불량 Lane 갯수가 동일한 경우
                                if (laneQty == t.Item1 && adjStrtPstn == t.Item2 && adjEndPstn == t.Item3)
                                {




                                    //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";










                                }
                                // 부분 Lane 구간 불량 표기
                                //  - Lot의 전체 Lane 갯수와 롤맵 상의 불량 Lane 갯수가 동일하지 않은 경우
                                else
                                {
                                    //// MRK_2D_BCD_STR이 존재할 경우 그룹핑하여 LANE_QTY와 비교
                                    String laneInfo = row["MRK_2D_BCD_STR"].ToString().Trim().Substring(0, 2);
                                    row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                                }
                            }
                        );

                    }
                    // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
                    else if ("TAG_SECTION_LANE".Equals(row["EQPT_MEASR_PSTN_ID"]))
                    {
                        String laneInfo = row["LANE_NO_LIST"].ToString();
                        row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                    }
                    else
                    {

                        if (row.Table.Columns.Contains("DFCT_NAME"))
                        {
                            row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + " : " + row["DFCT_NAME"].GetString() + "\n" + "[" + $"{Convert.ToDouble(row["SOURCE_ADJ_END_PSTN"]) - Convert.ToDouble(row["SOURCE_ADJ_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                        }
                        else
                        {
                            row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        }

                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                    }

                }
                ////NFF Lane 불량 정보 표기에 따른 변경 - End
            }

            dtBinding.AcceptChanges();
            return dtBinding;
        }

        private DataTable GetLotAttribute(string lotId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LOTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotId;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);
        }

        private DataTable Get2DBarcodeInfo(string lotId, string eqptId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotId;
            dr["EQPTID"] = eqptId;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_DFCT_2D_BCR_STD_RM", "RQSTDT", "RSLTDT", dt);
        }

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
            newRow["MENUID"] = "SFU010160122";          // 메뉴ID 코터:SFU010160121, 롤프레스:SFU010160122, 슬리터:SFU010160123, 리와인딩:SFU010160124  
            newRow["USERID"] = LoginInfo.USERID;
            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["EQSGID"] = _equipmentSegmentCode;   // LoginInfo.CFG_EQSG_ID;
            newRow["PROCID"] = _processCode;            // LoginInfo.CFG_PROC_ID;
            newRow["EQPTID"] = _equipmentCode;          // LoginInfo.CFG_EQPT_ID;
            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (menuHitReuslt, menuHitException) => { });

        }

        private DataTable GetColorMapSpec(string type)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = type;
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                string[] exceptionCode = new string[] { "OK", "NG" };

                return (from t in dtResult.AsEnumerable()
                        where !exceptionCode.Contains(t.Field<string>("CMCODE"))
                        orderby t.Field<decimal>("CMCDSEQ") ascending
                        select t).CopyToDataTable();
            }
            else
            {
                return null;
            }
        }

        private DataTable GetLaneSymbolColor(string type)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = type;
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", dt);
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

        private bool ValidationSearch()
        {
            if (string.IsNullOrEmpty(txtLot.Text))
            {
                Util.MessageValidation("SFU1366");
                return false;
            }

            return true;
        }

        private static Rectangle CreateRectangle(HorizontalAlignment horizontal, VerticalAlignment vertical, double height, double width, SolidColorBrush brush, SolidColorBrush strokeBrush, double thickness, Thickness margine, string name, Cursor cursor)
        {
            Rectangle rectangle = new Rectangle
            {
                HorizontalAlignment = horizontal,
                VerticalAlignment = vertical,
                Height = height,
                Width = width,
                StrokeThickness = thickness,
                Stroke = strokeBrush,
                Margin = margine,
                Fill = brush,
                Name = "Rect" + name,
                Cursor = cursor
            };

            return rectangle;
        }

        private static TextBlock CreateTextBlock(string content, HorizontalAlignment horizontal, VerticalAlignment vertical, int fontSize, FontWeight fontWeights, SolidColorBrush brush, Thickness margine, Thickness padding, string name, Cursor cursor, string tag)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = content,
                HorizontalAlignment = horizontal,
                VerticalAlignment = vertical,
                FontSize = fontSize,
                FontWeight = fontWeights,
                Foreground = brush,
                Margin = margine,
                Padding = padding,
                Name = name,
                Cursor = cursor,
                Tag = tag
            };

            return textBlock;
        }

        private static Ellipse CreateEllipse(HorizontalAlignment horizontal, VerticalAlignment vertical, int height, int width, SolidColorBrush brush, double thickness, Thickness margine, string name, Cursor cursor)
        {
            Ellipse ellipse = new Ellipse
            {
                HorizontalAlignment = horizontal,
                VerticalAlignment = vertical,
                Height = height,
                Width = width,
                StrokeThickness = thickness,
                Margin = margine,
                Fill = brush,
                Name = "Ellipse" + name,
                Cursor = cursor
            };

            return ellipse;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private enum ChartDisplayType
        {
            MarkLabel,
            TagSectionStart,
            TagSectionEnd,
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

        private enum ChartConfigurationType
        {
            Input,
            Output
        }

        private void btnRollMapPositionEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", "구간불량 등록");
                return;
            }

            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[12];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = string.Empty;
            parameters[2] = string.Empty;
            parameters[3] = _processCode;
            parameters[4] = _equipmentCode;
            parameters[5] = _wipSeq;
            parameters[6] = 0;
            parameters[7] = string.Empty;
            parameters[8] = true;
            parameters[9] = _xOutputMaxLength;
            parameters[10] = (_isRollMapResultLink && _isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;

            if (rdoAbsolutecoordinates.IsChecked == true)
                parameters[11] = "N";
            else
                parameters[11] = "Y";

            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }
        private void popRollMapPositionUpdate_Closed(object sender, EventArgs e)
        {
            CMM_RM_TAG_SECTION popup = sender as CMM_RM_TAG_SECTION;
            if (popup != null && popup.IsUpdated)
            {
                _isRollMapLot = SelectRollMapLot();
                btnSearch_Click(btnSearch, null);
            }
            else
            {
                tagSectionClear_Click(null, null);
            }
        }


        private bool SelectRollMapLot()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_ROLLMAP_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = txtLot.Text;
                dr["WIPSEQ"] = _wipSeq;
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["ROLLMAP_LOT_YN"].Equals("Y"))
                        return true;
                    else
                        return false;
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
                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (Equals(dtResult.Rows[0]["ROLLMAP_SBL_APPLY_FLAG"], "Y"))
                        return true;
                    else return false;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ValidationTagSection()
        {
            if (chartOutput.Data.Children.Count < 1)
            {
                Util.MessageValidation("SFU1498");
                return false;
            }

            return true;
        }

        //nathan 2024.04.09 OUT LOT의 Scrap 범위를 투입 LOT 에 표시 - start
        private void DrawScrapForInLot()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_ROLLMAP_SCRAP_FOR_INLOT";

                for (int i = chartInput.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartInput.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("SCRAP_INPUT"))
                    {
                        chartInput.Data.Children.Remove(dataSeries);
                    }
                }

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ADJFLAG", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = txtLot.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSEQ"] = _wipSeq;
                dr["ADJFLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
                inTable.Rows.Add(dr);

                DataTable dtScrap = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtScrap) == false) return;

                foreach (DataRow row in dtScrap.Rows)
                {
                    string EQPT_MEASR_PSTN_ID = row["EQPT_MEASR_PSTN_ID"].ToString();

                    if (EQPT_MEASR_PSTN_ID != "SCRAP") continue;

                    double SOURCE_START_PSTN = row["SOURCE_START_PSTN"] == null ? 0 : double.Parse(row["SOURCE_START_PSTN"].ToString());
                    double SOURCE_END_PSTN = row["SOURCE_END_PSTN"] == null ? 0 : double.Parse(row["SOURCE_END_PSTN"].ToString());

                    string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] " + ObjectDic.Instance.GetObjectName("위치") + $"{SOURCE_START_PSTN:###,###,###,##0.##}" + "m ~ " + $"{SOURCE_END_PSTN:###,###,###,##0.##}" + "m";
                    content = content + " " + ObjectDic.Instance.GetObjectName("길이") + $"{SOURCE_END_PSTN - SOURCE_START_PSTN:###,###,###,##0.##}" + "m";

                    if (row["TAG_SECTION_STRT"] != DBNull.Value && row["TAG_SECTION_END"] != DBNull.Value)
                    {
                        content = content + "\r\n[" + row["TAG_SECTION_NAME"].GetString() + "] " + ObjectDic.Instance.GetObjectName("위치") + $"{row["TAG_SECTION_STRT"]:###,###,###,##0.##}" + "m ~ " + $"{row["TAG_SECTION_END"]:###,###,###,##0.##}" + "m";
                        content = content + " " + ObjectDic.Instance.GetObjectName("길이") + $"{row["TAG_SECTION_END"].GetDouble() - row["TAG_SECTION_STRT"].GetDouble():###,###,###,##0.##}" + "m";
                    }

                    chartInput.Data.Children.Add(new AlarmZone()
                    {
                        Near = SOURCE_START_PSTN,
                        Far = SOURCE_END_PSTN,
                        LowerExtent = 0,
                        UpperExtent = 100,
                        ConnectionStroke = new SolidColorBrush(Colors.Black) { Opacity = 0.5 },
                        ConnectionFill = new SolidColorBrush(Colors.Black) { Opacity = 0.5 },
                        ToolTip = content,
                        Tag = "SCRAP_INPUT"
                    });
                }
            }
            catch (Exception ex)
            {
                return;
            }


        }
        //nathan 2024.04.09 OUT LOT의 Scrap 범위를 투입 LOT 에 표시 - end

        private void bShowDeftButton() //조성근 2024.08.28 롤프레스 롤맵 수불 적용
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



        #endregion

        #region SCRAP SECTION 영역
        //조성근 2025.04.09 Scrap Section 추가 - START
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
                        ToolTip = content,
                        Tag = tag
                    });
                }
            }
        }
        private void SetUseScrapSection()
        {
            try
            {
                DataTable dtResult = GetEqptClctTypeCombo("SCRAP_SECTION");
                btnScrapSection.Visibility = Visibility.Collapsed;

                if (CommonVerify.HasTableRow(dtResult) == false) return;
                if (_isRollMapResultLink == true && _isRollMapLot == true) return;

                btnScrapSection.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
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
            dr["EQPTID"] = _equipmentCode;
            dr["PROCID"] = _processCode;
            dr["EQPT_MEASR_PSTN_ID"] = strPstnID;
            RQSTDT.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_DEFECT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
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

            //if (_isSearchMode == true) strMode = "1";

            object[] parameters = new object[7];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = txtLot.Text;
            parameters[3] = _wipSeq;
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
                //IsUpdated = true;
                Dispatcher.BeginInvoke(new Action(() => { GetRollMap(); }));
            }
        }
        private void chartOutput_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point curPoint = e.GetPosition(chartOutput.View);
            var chartpoint = chartOutput.View.PointToData(curPoint);

            if (chartpoint == null || chartpoint.X < 0 || chartpoint.Y < 0 || chartpoint.Y > 100) return;


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

        //조성근 2025.04.09 Scrap Section 추가 - end
        #endregion


    }
}