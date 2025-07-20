/*************************************************************************************
 Created Date : 2023.05.08
      Creator : 신광희
   Decription : 소형 전극 Roll Map - 롤맵 차트 원통형(2170) 하프 슬리터 맵 팝업 
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.08  신광희 : Initial Created
                       단순 코드 복사 재사용하지 말고 C1Chart 익힌 후 참조 바람(참조 시 출처는 반드시 표시 바람.)
  2030.09.19  조성진 : 수정
                       코터 영역을 코터 롤맵 UI와 동일하게 TOP 모음 / BACK 모음 형태로 수정 및 tooltip 문구 수정 등 UI 전체적 수정
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

    public partial class COM001_ROLLMAP_MOBILE_HALFSLITTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        private DataTable _dtDefectOutput;
        private DataTable _dtDefectInput;
        private DataTable _dtBaseDefectOutput;
        private DataTable _dtBaseDefectInput;
        private DataTable _dtLineLegend;
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

        private double _xMaxLength;
        private double _xMinLength;
        private int _xMaxLane;
        private int _xMinLane;
        private double _sampleCutLength;
        private bool _isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        private bool _isRollMapLot;
        private string _polarityCode = string.Empty;
        private DataTable _dtRollMapLane;    // 20231017 조성진, 맵표현 요약의 binding 방식 변경


        private CoordinateType _CoordinateType;
        private enum CoordinateType
        {
            RelativeCoordinates,    //상대좌표
            Absolutecoordinates     //절대좌표
        }
        #endregion

        public COM001_ROLLMAP_MOBILE_HALFSLITTING()
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
            CMM_ROLLMAP_DATACOLLECT popupRollMapDataCollect = new CMM_ROLLMAP_DATACOLLECT();
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
            CMM_ROLLMAP_DATACOLLECT popup = sender as CMM_ROLLMAP_DATACOLLECT;
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
            //Util.MessageValidation("웹게이지 데이터가 존재하지 않습니다.");
            Util.MessageInfo("SFU8501", ObjectDic.Instance.GetObjectName("웹게이지"));
            //rdoNone.IsChecked = true;
        }

        private void rdoThick_Click(object sender, RoutedEventArgs e)
        {
            //Util.MessageValidation("두께 데이터가 존재하지 않습니다.");
            Util.MessageInfo("SFU8501", ObjectDic.Instance.GetObjectName("두께"));
            //rdoNone.IsChecked = true;
        }

        private void rdoNone_Click(object sender, RoutedEventArgs e)
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
            string scrapQty = $"{splitItem[2].GetDouble() - splitItem[1].GetDouble():###,###,###,##0.00}";
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
            else if (string.Equals(equipmentMeasurementCode, "RW_SCRAP"))
            {
                CMM_ROLLMAP_RW_SCRAP popRollMapRewinderScrap = new CMM_ROLLMAP_RW_SCRAP();
                popRollMapRewinderScrap.FrameOperation = FrameOperation;
                object[] parameters = new object[11];
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
                parameters[10] = true;    // 20230912 조성진, 팝업의 재사용을 위한 파라미터 추가
                C1WindowExtension.SetParameters(popRollMapRewinderScrap, parameters);
                popRollMapRewinderScrap.Closed += popRollMapRewinderScrap_Closed;
                Dispatcher.BeginInvoke(new Action(() => popRollMapRewinderScrap.ShowModal()));
            }
            else
            {
                CMM_ROLLMAP_SCRAP popRollMapScrap = new CMM_ROLLMAP_SCRAP();
                popRollMapScrap.FrameOperation = FrameOperation;

                object[] parameters = new object[8];
                parameters[0] = lotId;
                parameters[1] = wipSeq;
                parameters[2] = equipmentMeasurementCode;
                parameters[3] = collectSeq;
                parameters[4] = startPosition;
                parameters[5] = endPosition;
                parameters[6] = scrapQty;
                parameters[7] = true;    // 20230912 조성진, 팝업의 재사용을 위한 파라미터 추가
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

        private void btnRollMapPositionEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", "구간불량 등록");
                return;
            }

            CMM_ROLLMAP_PSTN_UPD popRollMapPositionUpdate = new CMM_ROLLMAP_PSTN_UPD();
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
            parameters[9] = _xMaxLength - _sampleCutLength;
            parameters[10] = (_isRollMapResultLink && _isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;
            parameters[11] = true;
            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }

        private void popRollMapPositionUpdate_Closed(object sender, EventArgs e)
        {
            //CMM_ROLLMAP_PSTN_UPD popup = sender as CMM_ROLLMAP_PSTN_UPD;
            //if (popup != null && popup.IsUpdated)
            //{
            //    _isRollMapLot = SelectRollMapLot();
            //    btnSearch_Click(btnSearch, null);
            //}
            //else
            //{
            //    tagSectionClear_Click(tagSectionClear, null);
            //}
        }

        private void tagSectionClear_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationTagSection()) return;

            //for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
            //{
            //    DataSeries dataSeries = chartCoater.Data.Children[i];
            //    if (dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
            //    {
            //        chartCoater.Data.Children.Remove(dataSeries);
            //    }
            //}
            //_selectedStartSection = 0;
            //_selectedEndSection = 0;
            //_selectedchartPosition = 0;
            //_isSelectedTagSection = false;
        }

        private void cboRollMapExpressSummary_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (string.IsNullOrEmpty(cboRollMapExpressSummary.SelectedValue.GetString())) return;

            DataTable dt = DataTableConverter.Convert(cboRollMapExpressSummary.ItemsSource);
            DataRow dr = dt.Rows[cboRollMapExpressSummary.SelectedIndex];

            // 20231017 조성진, 맵표현 요약의 binding 방식 변경
            string[] laneArray;
            laneArray = dr["LaneDescription"].GetString().Split(',');

            //C : 양극
            //A : 음극

            //string[] laneArray;

            //if (string.Equals("C", _polarityCode))
            //{
            //    laneArray = dr["ATTRIBUTE1"].GetString().Split(',');
            //}
            //else
            //{
            //    laneArray = dr["ATTRIBUTE2"].GetString().Split(',');
            //}

            if (laneArray != null && msbRollMapLane.ItemsSource != null)
            {
                msbRollMapLane.UncheckAll();

                for (int i = 0; i < laneArray.Length; i++)
                {
                    foreach (MultiSelectionBoxItem msbitem in msbRollMapLane.MultiSelectionBoxSource)
                    {
                        if (DataTableConverter.GetValue(msbitem.Item, "CMCODE").GetString().Equals(laneArray[i].GetString()))
                            msbitem.IsChecked = true;
                    }
                }
            }
        }


        private void rdoWebgaugeTop_Click(object sender, RoutedEventArgs e)
        {
            // 20230825 조성진, 계측기 조회조건 Collapsed 처리
            ////txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
            //e.Handled = true;

            //GetLegend();
        }

        private void rdoWebgaugeBack_Click(object sender, RoutedEventArgs e)
        {
            // // 20230825 조성진, 계측기 조회조건 Collapsed 처리
            ////txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
            //e.Handled = true;

            //GetLegend();
        }

        #endregion

        #region # Method
        private void Initialize()
        {
            try
            {
                //ROLLMAP.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                //ROLLMAP.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;
                ROLLMAP.Width = (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width > 1920 ? 1920 : System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width) - 100;
                ROLLMAP.Height = (System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height > 1080 ? 1080 : System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height) - 100;

                InitializeControl();
                InitializeCombo();
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

            chartInput.View.AxisX.ScrollBar = new AxisScrollBar();
            chartOutput.View.AxisX.ScrollBar = new AxisScrollBar();

            _dtLineLegend = new DataTable();
            _dtLineLegend.Columns.Add("NO", typeof(int));
            _dtLineLegend.Columns.Add("COLORMAP", typeof(string));
            _dtLineLegend.Columns.Add("VALUE", typeof(string));
            _dtLineLegend.Columns.Add("GROUP", typeof(string));
            _dtLineLegend.Columns.Add("SHAPE", typeof(string));

            // 로딩량 평균
            DataTable dtColorMapSpec = GetColorMapSpec("ROLLMAP_COLORMAP_SPEC");
            if (CommonVerify.HasTableRow(dtColorMapSpec))
            {
                foreach (DataRow row in dtColorMapSpec.Rows)
                {
                    _dtLineLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCDNAME"].GetString(), "LOAD", "RECT");
                }
            }

            _dtDefectOutput = new DataTable();
            _dtDefectOutput.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefectOutput.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefectOutput.Columns.Add("COLORMAP", typeof(string));
            _dtDefectInput = _dtDefectOutput.Copy();

        }
        
        private void InitializeCombo()
        {
            //// Lane별 롤맵 선택 MultiSelectionBox
            //SetRollMapLaneMultiSelectionBox(msbRollMapLane);

            //// 맵표현 요약 콤보박스
            //SetRollMapExpressSummaryCombo(cboRollMapExpressSummary);

            // 20231017 조성진, 맵표현 요약의 binding 방식 변경
            GetRollMapLaneRange();
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

            BeginUpdateChart();
        }

        private void InitializeDataTables()
        {
            _dtDefectOutput?.Clear();
            _dtDefectInput?.Clear();
            _dtBaseDefectInput?.Clear();
            _dtBaseDefectOutput?.Clear();
        }

        private void InitializeChart(C1Chart chart)
        {
            double maxLength = string.Equals(chart.Name, "chartInput") ? _xInputMaxLength.Equals(0) ? 10 : _xInputMaxLength : _xOutputMaxLength.Equals(0) ? 10 : _xOutputMaxLength;

            chart.View.AxisX.Min = 0;
            chart.View.AxisY.Min = chart.Name == "chartInput" ? -20 : -30;
            chart.View.AxisX.Max = maxLength;
            chart.View.AxisY.Max = 100;
            InitializeChartView(chart);

            /*
            // 슬리터 차트의 경우 웹게이지, 두께 측정데이터 가 없으므로 강제로 배경색 지정 함.
            if (chart.Name == "chartOutput")
            {
                var convertFromString = ColorConverter.ConvertFromString("#F6F6F6");
                AlarmZone alarmZone = new AlarmZone
                {
                    Near = 0,
                    Far = maxLength,
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = 0,
                    UpperExtent = 100,
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                };
                chart.Data.Children.Insert(0, alarmZone);
            }
            */
        }

        private void GetLegend()
        {
            grdLegend.Children.Clear();

            DataTable dt = _dtLineLegend.Copy();

            if ((bool)rdoRelativeCoordinates?.IsChecked)
            {
                string[] exceptLegend = new string[] { ObjectDic.Instance.GetObjectName("QA샘플")
                                                     , ObjectDic.Instance.GetObjectName("자주검사")
                                                     , ObjectDic.Instance.GetObjectName("최외각 폐기")};

                dt.AsEnumerable().Where(r => exceptLegend.Contains(r.Field<string>("VALUE"))).ToList().ForEach(row => row.Delete());
                dt.AcceptChanges();
            }

            for (int x = 0; x < 2; x++)
            {
                ColumnDefinition gridCol1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                grdLegend.ColumnDefinitions.Add(gridCol1);
            }

            StackPanel sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 3)
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
                        Margin = new Thickness(5, 0, 5, 3),
                        Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                    };
                    TextBlock textBlockDescription = new TextBlock()
                    {
                        Text = Util.NVC(row["VALUE"]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        Margin = new Thickness(5, 0, 5, 3)
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
                    Margin = new Thickness(5, 0, 5, 3)
                };

                string[] legendArray;
                if (rdoAbsolutecoordinates.IsChecked != null && rdoAbsolutecoordinates.IsChecked == true)
                {
                    legendArray = new string[] { ObjectDic.Instance.GetObjectName("이음매"), ObjectDic.Instance.GetObjectName("SCRAP"), ObjectDic.Instance.GetObjectName("RW_SCRAP") };
                }
                else
                {
                    legendArray = new string[] { ObjectDic.Instance.GetObjectName("이음매"), ObjectDic.Instance.GetObjectName("SCRAP") };
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
                        convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD"));
                    }
                    else if (string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("SCRAP")))
                    {
                        convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                    }
                    else
                    {
                        convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE400"));
                    }

                    Polygon polygon = new Polygon()
                    {
                        Points = pointCollection,
                        Width = 13,
                        Height = 15,
                        Stretch = Stretch.Fill,
                        StrokeThickness = 1,
                        Fill = convertFromString,
                        Stroke = convertFromString
                    };
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = legendArray[i].GetString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        Margin = new Thickness(5, 0, 5, 3)
                    };

                    stackPanel.Children.Add(polygon);
                    stackPanel.Children.Add(textBlock);
                }

                Grid.SetColumn(stackPanel, 1);
                Grid.SetRow(stackPanel, 1);
                grdLegend.Children.Add(stackPanel);

            }
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

        private void SelectRollMap()
        {
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CHART_CY";
            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("LANE_LIST", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = txtLot.Text;
            newRow["WIPSEQ"] = _wipSeq;
            newRow["ADJFLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
            newRow["LANE_LIST"] = msbRollMapLane.SelectedItemsToString; 
            
            inTable.Rows.Add(newRow);

            //string xml = inDataSet.GetXml();

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_LANE,OUT_HEAD,OUT_DEFECT,OUT_GAUGE,OUT_IN_LANE,OUT_IN_HEAD,OUT_IN_DEFECT,OUT_IN_GAUGE", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    SetMenuUseCount();
                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        DataTable dtInLane = bizResult.Tables["OUT_IN_LANE"];       //투입 코터 차트Lane, 무지부, Top Back표시 테이블
                        DataTable dtInHead = bizResult.Tables["OUT_IN_HEAD"];       //투입 코터 차트 헤더 길이 표시 테이블
                        DataTable dtInDefect = bizResult.Tables["OUT_IN_DEFECT"];   //투입 코터 차트 불량정보 표시 테이블
                        DataTable dtInGauge = bizResult.Tables["OUT_IN_GAUGE"];     //투입 코터 차트 두께 측정 데이터 표시 테이블

                        DataTable dtLane = bizResult.Tables["OUT_LANE"];            //완성 하프슬리터 차트Lane, 무지부, Top Back표시 테이블
                        DataTable dtHead = bizResult.Tables["OUT_HEAD"];            //완성 하프슬리터 차트 전체 길이 표시 테이블
                        DataTable dtDefect = bizResult.Tables["OUT_DEFECT"];        //완성 하프슬리터 차트 불량정보 표시 테이블
                        DataTable dtGauge = bizResult.Tables["OUT_GAUGE"];          //완성 하프슬리터 차트 두께 측정 데이터 표시 테이블

                        #region [투입 코터 차트 영역]

                        if (CommonVerify.HasTableRow(dtInHead))
                        {
                            GetInOutProcessMaxLength(dtInHead, ChartConfigurationType.Input);
                            InitializeChart(chartInput);
                            DrawStartEndYAxis(dtInHead, ChartConfigurationType.Input);
                        }

                        if (CommonVerify.HasTableRow(dtInLane))
                        {
                            DrawLane(dtInLane, ChartConfigurationType.Input);
                            DrawWinderDirection(dtInLane);
                        }

                        if (CommonVerify.HasTableRow(dtInGauge))
                            DrawGauge(dtInGauge, ChartConfigurationType.Input);

                        if (CommonVerify.HasTableRow(dtInDefect))
                        {
                            DrawDefect(dtInDefect, ChartConfigurationType.Input);
                            DrawDefectLegend(dtInDefect, ChartConfigurationType.Input);
                        }

                        _dtBaseDefectInput = dtInDefect.Copy();
                        #endregion

                        #region [완성 하프슬리터 차트 영역]
                        if (CommonVerify.HasTableRow(dtHead))
                        {
                            GetInOutProcessMaxLength(dtHead, ChartConfigurationType.Output);
                            InitializeChart(chartOutput);
                            DrawStartEndYAxis(dtHead, ChartConfigurationType.Output);
                        }

                        if (CommonVerify.HasTableRow(dtLane))
                            DrawLane(dtLane, ChartConfigurationType.Output);

                        if (CommonVerify.HasTableRow(dtGauge))
                            DrawGauge(dtGauge, ChartConfigurationType.Output);

                        if (CommonVerify.HasTableRow(dtDefect))
                        {
                            DrawDefect(dtDefect, ChartConfigurationType.Output);
                            DrawDefectLegend(dtDefect, ChartConfigurationType.Output);
                        }

                        _dtBaseDefectOutput = dtDefect.Copy();

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

        /// <summary>
        /// 차트 상단 시작위치 종료위치 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawStartEndYAxis(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            //chartInput, chartOutput 상단 Rewinder 길이 표시
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
                         {
                             StartPosition = t.Field<decimal?>("STRT_PTN_PSTN"),
                             EndPosition = t.Field<decimal>("END_PTN_PSTN")
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
                newLength["RAW_END_PSTN"] = $"{query.EndPosition:###,###,###,##0.00}";
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

            // chartInput 차트 상단영역 Cut 표시
            var querycut = (from t in dt.AsEnumerable()
                            where t.Field<string>("EQPT_MEASR_PSTN_ID") == "CUT_PSTN"
                            select new
                            {
                                StartPosition = t.Field<decimal?>("CUM_STRT_PTN_PSTN"),
                                EndPosition = t.Field<decimal>("CUM_END_PTN_PSTN")
                            }).ToList();

            if (querycut.Any())
            {
                DataTable dtLength = new DataTable();
                dtLength.Columns.Add("RAW_END_PTN_PSTN", typeof(string));
                dtLength.Columns.Add("SOURCE_Y_PSTN", typeof(double));
                dtLength.Columns.Add("FONTSIZE", typeof(double));

                foreach (var item in querycut)
                {
                    DataRow newLength = dtLength.NewRow();
                    newLength["RAW_END_PTN_PSTN"] = $"{item.EndPosition:###,###,###,##0.00}";
                    newLength["SOURCE_Y_PSTN"] = 100;
                    newLength["FONTSIZE"] = 11;
                    dtLength.Rows.Add(newLength);
                }

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtLength);
                ds.XValueBinding = new Binding("RAW_END_PTN_PSTN");
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

            // chartInput 차트 영역 상단 LOT MergeLot 영역 표시
            var queryMergeLot = (from t in dt.AsEnumerable()
                                 where t.Field<string>("EQPT_MEASR_PSTN_ID") == "MERGE_PSTN"
                                 select new
                                 {
                                     InputLotId = t.Field<string>("INPUT_LOTID"),
                                     StartPosition = t.Field<decimal>("CUM_STRT_PTN_PSTN"),
                                     EndPosition = t.Field<decimal>("CUM_END_PTN_PSTN")
                                 }).ToList();

            if (queryMergeLot.Any())
            {
                DataTable dtMergeLot = new DataTable();
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_LOTID", DataType = typeof(string) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_STRT_PTN_PSTN", DataType = typeof(double) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_END_PTN_PSTN", DataType = typeof(double) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "Y_PSTN", DataType = typeof(double) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtMergeLot.Columns.Add(new DataColumn() { ColumnName = "ADJ_VISIVILITY", DataType = typeof(string) }); //Collapsed, Visible

                foreach (var item in queryMergeLot)
                {
                    DataRow drMergeLot = dtMergeLot.NewRow();
                    drMergeLot["ADJ_LOTID"] = item.InputLotId;
                    drMergeLot["ADJ_STRT_PTN_PSTN"] = $"{item.StartPosition:###,###,###,##0.00}";
                    drMergeLot["ADJ_END_PTN_PSTN"] = $"{item.EndPosition:###,###,###,##0.00}";
                    drMergeLot["Y_PSTN"] = 100;
                    drMergeLot["TOOLTIP"] = "LOT ID : " + item.InputLotId + " ( " + $"{item.StartPosition.GetDouble():###,###,###,##0.00}" + " ~ " + $"{item.EndPosition.GetDouble():###,###,###,##0.00}" + ")";
                    drMergeLot["ADJ_VISIVILITY"] = "Visible";
                    dtMergeLot.Rows.Add(drMergeLot);
                }

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtMergeLot);
                ds.XValueBinding = new Binding("ADJ_END_PTN_PSTN");
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

                // 유지부 BackGround 지정, TODO : Top, Back 및 Lane 지정 필요
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

            if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            {
                //var query = (from t in dt.AsEnumerable() where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" select new { LaneNo = t.Field<string>("LANE_NO_CUR") }).Distinct().ToList();
                //int laneCount = query.Count;

                //// Lane Text 표시
                //if (query.Any())
                //{
                //    foreach (var item in query)
                //    {
                //        var queryLane = dt.AsEnumerable().Where(s => s.Field<string>("LANE_NO_CUR") == item.LaneNo && s.Field<string>("LOC2") != "XX").GroupBy(b => b.Field<string>("LANE_NO_CUR"))
                //            .Select(c => new
                //            {
                //                LaneNo = c.Key,
                //                FirstLaneRate = c.Min(z => z.Field<decimal>("Y_PSTN_STRT_RATE")),
                //                LastLaneRate = c.Max(z => z.Field<decimal>("Y_PSTN_END_RATE")),
                //            }).FirstOrDefault();

                //        if (queryLane != null)
                //        {
                //            DrawLaneText(queryLane.FirstLaneRate.GetDouble(), queryLane.LastLaneRate.GetDouble(), queryLane.LaneNo, chartConfigurationType);
                //        }
                //    }
                //}

                // 20230913 조성진, 코터 영역을 코터 롤맵 UI와 동일하게 TOP 모음 / BACK 모음 형태로 수정. 추후 T/B T/B T/B 형태로 복구할 경우를 위해 전체 LOGIC은 남겨둠.
                // Top Back 구분, Lane  Grouping
                var queryTopBackLane = dt.AsEnumerable().Where(a => a.Field<string>("LANE_NO_CUR") != null && a.Field<string>("LOC2") != "XX").GroupBy(g => new { TopBackFlag = g.Field<string>("T_B_FLAG"), LaneNo = g.Field<string>("LANE_NO_CUR") })
                    .Select(s => new { TopBackFlag = s.Key.TopBackFlag, LaneNo = s.Key.LaneNo }).ToList();

                //Lane 표시
                if (queryTopBackLane.Any())
                {
                    foreach (var item in queryTopBackLane)
                    {
                        var queryLaneRate = dt.AsEnumerable()
                            .Where(a => a.Field<string>("LANE_NO_CUR") == item.LaneNo && a.Field<string>("T_B_FLAG") == item.TopBackFlag && a.Field<string>("LANE_NO_CUR") != null && a.Field<string>("LOC2") != "XX")
                            .GroupBy(g => new { TopBackFlag = g.Field<string>("T_B_FLAG"), LaneNo = g.Field<string>("LANE_NO_CUR") })
                            .Select(s => new
                            {
                                LaneNo = s.Key.LaneNo,
                                TopBackFlag = s.Key.TopBackFlag,
                                FirstLaneRate = s.Min(z => z.Field<decimal>("Y_PSTN_STRT_RATE")),
                                LastLaneRate = s.Max(z => z.Field<decimal>("Y_PSTN_END_RATE"))
                            }).FirstOrDefault();

                        if (queryLaneRate != null)
                        {
                            //DrawTopBackText(item.TopBackFlag, queryTopBackRate.FirstTopBackRate.GetDouble(), queryTopBackRate.LastTopBackRate.GetDouble(), chartConfigurationType);
                            DrawLaneText(queryLaneRate.FirstLaneRate.GetDouble(), queryLaneRate.LastLaneRate.GetDouble(), queryLaneRate.LaneNo, chartConfigurationType);
                        }
                    }
                }
                DrawTopBackRow(dt, chartConfigurationType);
            }
            else
            {
                // Top Back 구분, Lane  Grouping
                var queryTopBackLane = dt.AsEnumerable().Where(a => a.Field<string>("LANE_NO_CUR") != null && a.Field<string>("LOC2") != "XX").GroupBy(g => new { TopBackFlag = g.Field<string>("T_B_FLAG"), LaneNo = g.Field<string>("LANE_NO_CUR") })
                    .Select(s => new { TopBackFlag = s.Key.TopBackFlag, LaneNo = s.Key.LaneNo }).ToList();

                //Lane 표시
                if (queryTopBackLane.Any())
                {
                    foreach (var item in queryTopBackLane)
                    {
                        var queryLaneRate = dt.AsEnumerable()
                            .Where(a => a.Field<string>("LANE_NO_CUR") == item.LaneNo && a.Field<string>("T_B_FLAG") == item.TopBackFlag && a.Field<string>("LANE_NO_CUR") != null && a.Field<string>("LOC2") != "XX")
                            .GroupBy(g => new { TopBackFlag = g.Field<string>("T_B_FLAG"), LaneNo = g.Field<string>("LANE_NO_CUR") })
                            .Select(s => new
                            {
                                LaneNo = s.Key.LaneNo,
                                TopBackFlag = s.Key.TopBackFlag,
                                FirstLaneRate = s.Min(z => z.Field<decimal>("Y_PSTN_STRT_RATE")),
                                LastLaneRate = s.Max(z => z.Field<decimal>("Y_PSTN_END_RATE"))
                            }).FirstOrDefault();

                        if (queryLaneRate != null)
                        {
                            //DrawTopBackText(item.TopBackFlag, queryTopBackRate.FirstTopBackRate.GetDouble(), queryTopBackRate.LastTopBackRate.GetDouble(), chartConfigurationType);
                            DrawLaneText(queryLaneRate.FirstLaneRate.GetDouble(), queryLaneRate.LastLaneRate.GetDouble(), queryLaneRate.LaneNo, chartConfigurationType);
                        }
                    }
                }
            }

        }

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
            dt.Columns.Add("SOURCE_END_PTN_PSTN", typeof(double));
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
            dr["SOURCE_END_PTN_PSTN"] = dtLength.Rows[0]["TOTAL_WIDTH2"];
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
            newRow["SOURCE_END_PTN_PSTN"] = 0;
            newRow["ARROW_PSTN"] = arrowPosition;
            newRow["SOURCE_Y_PSTN"] = 0;
            newRow["ARROW_Y_PSTN"] = arrowYPosition;
            newRow["CIRCLENAME"] = "UW";
            newRow["COLORMAP"] = "#000000";
            dt.Rows.Add(newRow);

            XYDataSeries ds = new XYDataSeries();
            ds.ItemsSource = DataTableConverter.Convert(dt);
            ds.XValueBinding = new Binding("SOURCE_END_PTN_PSTN");
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
            }
        }

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

        private void DrawGauge(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            // 슬리터 차트의 경우 웹게이지, 두께 측정데이터 가 없음 공통코드에 등록된 배경색 으로 지정 함.
            var queryBackGround = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("ROLLMAP_BACKGROUND")).ToList();
            DataTable dtBackGround = queryBackGround.Any() ? queryBackGround.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtBackGround))
            {
                dtBackGround.TableName = "ROLLMAP_BACKGROUND";
                DrawRollMapBackGround(dtBackGround, chartConfigurationType);
            }

            //두께 차트 영역에 Gauge 데이터 외의 설비 측정 위치 아이디는 제외 하여 표시 함.
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "TAG_SECTION", "ROLLMAP_BACKGROUND" };
            var queryGauge = dt.AsEnumerable().Where(o => !gaugeExceptions.Contains(o.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList();
            DataTable dtGauge = queryGauge.Any() ? queryGauge.CopyToDataTable() : dt.Clone();

            for (int i = 0; i < dtGauge.Rows.Count; i++)
            {
                AlarmZone alarmZone = new AlarmZone();
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dtGauge.Rows[i]["COLORMAP"]));

                alarmZone.Near = dtGauge.Rows[i]["ADJ_STRT_PTN_PSTN"].GetDouble();
                alarmZone.Far = dtGauge.Rows[i]["ADJ_END_PTN_PSTN"].GetDouble();
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
                        double adjStartPosition = Convert.ToDouble(dtGauge.Rows[x]["ADJ_STRT_PTN_PSTN"]);
                        double adjEndPosition = Convert.ToDouble(dtGauge.Rows[x]["ADJ_END_PTN_PSTN"]);

                        string content = ObjectDic.Instance.GetObjectName("LANE") + " : #" + dtGauge.Rows[x]["ADJ_LANE_NO"].GetString() + Environment.NewLine + 
                                         dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{adjStartPosition:###,###,###,##0.00}" + "~" + $"{adjEndPosition:###,###,###,##0.00}" + "] " + Environment.NewLine +
                                         "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.00}") + Environment.NewLine +
                                         "ColorMap : " + Util.NVC(dtGauge.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                         "Offset : " + Util.NVC(dtGauge.Rows[x]["SCAN_OFFSET"]);

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
        }

        private void DrawTopBackText(string topBackFlag, double firstlaneRate, double lastRate, ChartConfigurationType chartConfigurationType)
        {
            DataTable dtTopBack = new DataTable();
            dtTopBack.Columns.Add("SOURCE_STRT_PTN_PSTN", typeof(string));
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
            dr["SOURCE_STRT_PTN_PSTN"] = startPosition;
            dr["SOURCE_Y_PSTN"] = (firstlaneRate + lastRate) / 2 - 1;
            dr["FLAG"] = topBackFlag.Equals("T") ? "Top" : "Back";
            dtTopBack.Rows.Add(dr);

            XYDataSeries dsTopBack = new XYDataSeries();
            dsTopBack.ItemsSource = DataTableConverter.Convert(dtTopBack);
            dsTopBack.XValueBinding = new Binding("SOURCE_STRT_PTN_PSTN");
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

        private void DrawTopBackRow(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            if (chartConfigurationType == ChartConfigurationType.Output) return;
            Grid grid = grdInputDynamicTopBack;

            grid.Children.Clear();
            if (grid.ColumnDefinitions.Count > 0) grid.ColumnDefinitions.Clear();
            if (grid.RowDefinitions.Count > 0) grid.RowDefinitions.Clear();

            var query = (from t in dt.AsEnumerable() where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" select new { LaneNo = t.Field<string>("LANE_NO_CUR") }).Distinct().ToList();
            //int laneCount = query.Count;
            // 20230913 조성진, 코터 영역을 코터 롤맵 UI와 동일하게 TOP 모음 / BACK 모음 형태로 수정. 추후 T/B T/B T/B 형태로 복구할 경우를 위해 전체 LOGIC은 남겨둠.
            int laneCount = 1;


            string firstTopBack = "T";
            string currentTopBack = string.Empty;


            var queryTopBack = (from t in dt.AsEnumerable()
                                where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" && (t.Field<decimal>("Y_PSTN_END_RATE") == 100 || t.Field<Int16>("CNT") == 1)
                                select new
                                {
                                    firstTopBackFlag = t.Field<string>("T_B_FLAG")
                                }).FirstOrDefault();

            if (queryTopBack != null)
                firstTopBack = queryTopBack.firstTopBackFlag;

            ColumnDefinition columnDefinition = new ColumnDefinition { Width = new GridLength(30) };
            grid.ColumnDefinitions.Add(columnDefinition);

            for (int i = 0; i < laneCount; i++)
            {
                RowDefinition rowDefinitionTop = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                RowDefinition rowDefinitionBack = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
                grid.RowDefinitions.Add(rowDefinitionTop);
                grid.RowDefinitions.Add(rowDefinitionBack);
            }

            // Border 및 TextBlock Grid(grid) 삽입
            for (int i = 0; i < laneCount; i++)
            {
                Border borderTop = new Border();
                borderTop.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                borderTop.Background = new SolidColorBrush(Colors.LightGray);
                borderTop.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffbebebe"));
                borderTop.CornerRadius = new CornerRadius(3);

                TextBlock textBlockTop = new TextBlock();
                textBlockTop.Text = string.IsNullOrEmpty(currentTopBack) ? firstTopBack : currentTopBack.Equals("T") ? "B" : "T";
                currentTopBack = textBlockTop.Text;
                textBlockTop.HorizontalAlignment = HorizontalAlignment.Stretch;
                textBlockTop.TextAlignment = TextAlignment.Center;
                textBlockTop.Style = Application.Current.Resources["Content_InputForm_LabelStyle"] as Style;

                Grid.SetColumn(borderTop, 0);
                Grid.SetRow(borderTop, i * 2);
                Grid.SetColumn(textBlockTop, 0);
                Grid.SetRow(textBlockTop, i * 2);
                grid.Children.Add(borderTop);
                grid.Children.Add(textBlockTop);

                Border borderBack = new Border();
                borderBack.SetValue(Border.BorderThicknessProperty, new Thickness(1));
                borderBack.Background = new SolidColorBrush(Colors.LightGray);
                borderBack.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffbebebe"));
                borderBack.CornerRadius = new CornerRadius(3);

                TextBlock textBlockBack = new TextBlock();
                textBlockBack.Text = string.IsNullOrEmpty(currentTopBack) ? firstTopBack : currentTopBack.Equals("T") ? "B" : "T";
                currentTopBack = textBlockBack.Text;
                textBlockBack.HorizontalAlignment = HorizontalAlignment.Stretch;
                textBlockBack.TextAlignment = TextAlignment.Center;
                textBlockBack.Style = Application.Current.Resources["Content_InputForm_LabelStyle"] as Style;

                Grid.SetColumn(borderBack, 0);
                Grid.SetRow(borderBack, (i * 2) + 1);
                Grid.SetColumn(textBlockBack, 0);
                Grid.SetRow(textBlockBack, (i * 2) + 1);
                grid.Children.Add(borderBack);
                grid.Children.Add(textBlockBack);
            }
        }

        private void DrawLaneText(double firstlaneRate, double lastlaneRate, string laneNo, ChartConfigurationType chartConfigurationType)
        {
            DataTable dtLane = new DataTable();
            dtLane.Columns.Add("SOURCE_STRT_PTN_PSTN", typeof(string));
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
            dr["SOURCE_STRT_PTN_PSTN"] = startPosition;            
            dr["SOURCE_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? (firstlaneRate + lastlaneRate) / 2 - 4 : (firstlaneRate + lastlaneRate) / 2 - 7.5;    // 텍스트 표시 Y 위치    // 20230913 조성진, 코터 영역을 코터 롤맵 UI와 동일하게 TOP 모음 / BACK 모음 형태로 수정.
            dr["LANEINFO"] = "Lane " + laneNo;
            dtLane.Rows.Add(dr);

            XYDataSeries dsLane = new XYDataSeries();
            dsLane.ItemsSource = DataTableConverter.Convert(dtLane);
            dsLane.XValueBinding = new Binding("SOURCE_STRT_PTN_PSTN");
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
                    double xPosition;
                    double yPosition;
                    double topPattern;
                    double backPattern;
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_X_PSTN")), out xPosition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_Y_PSTN")), out yPosition);
                    string defectName = DataTableConverter.GetValue(dp.DataObject, "EQPT_MEASR_PSTN_NAME").GetString();
                    

                    double adjStartPtnPosition;
                    double adjStartPosition = 0;
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "ADJ_STRT_PTN_PSTN")), out adjStartPtnPosition);        
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "ADJ_STRT_PSTN")), out adjStartPosition);
        

                    // 20230905 조성진, 툴팁 표현 문구 수정
                    // Ex) 불량 항목 : 패턴 길이 불량
                    //     패턴 No. : 1300EA
                    //     패턴 비율 : Top 0.7 / Back 0.5
                    string content;                    
                    content = defectName + Environment.NewLine +
                                $"{adjStartPtnPosition:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("EA") +
                                "(" + $"{adjStartPosition:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("M") + ")"
                                ;

                    //+ Environment.NewLine +
                    //ObjectDic.Instance.GetObjectName("패턴 비율") + " : " + ObjectDic.Instance.GetObjectName("TOP") + " " + $"{topPattern:###,###,###,##0.00}" + " / " + ObjectDic.Instance.GetObjectName("BACK") + " " + $"{backPattern:###,###,###,##0.00}";

                    ToolTipService.SetToolTip(pe, content);
                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                    ToolTipService.SetShowDuration(pe, 60000);
                }
            };
            c1Chart.Data.Children.Add(ds);
        }

        private void DrawDefectAlignVision(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;
            foreach (DataRow row in dt.Rows)
            {
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                AlarmZone alarmZone = new AlarmZone
                {
                    Near = row["ADJ_STRT_PTN_PSTN"].GetDouble(),
                    Far = row["ADJ_END_PTN_PSTN"].GetDouble(),
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["CHART_Y_STRT_CUM_RATE"].GetDouble(),
                    UpperExtent = row["CHART_Y_END_CUM_RATE"].GetDouble(),
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    Cursor = Cursors.Hand,
                    Tag = dt.TableName,
                };

                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    if (pe is Lines)
                    {
                        double adjStartPtnPosition;
                        double adjEndPtnPosition;
                        double.TryParse(Util.NVC(row["ADJ_STRT_PTN_PSTN"]), out adjStartPtnPosition);
                        double.TryParse(Util.NVC(row["ADJ_END_PTN_PSTN"]), out adjEndPtnPosition);
                        string content = row["ABBR_NAME"] + "[" + $"{adjStartPtnPosition:###,###,###,##0.00}" + "m" + "~" + $"{adjEndPtnPosition:###,###,###,##0.00}" + "m" + "]";
                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };
                c1Chart.Data.Children.Add(alarmZone);
            }
        }

        private void DrawDefectOverLayVision(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;
            foreach (DataRow row in dt.Rows)
            {
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));
                AlarmZone alarmZone = new AlarmZone
                {
                    Near = row["ADJ_STRT_PTN_PSTN"].GetDouble(),
                    Far = row["ADJ_END_PTN_PSTN"].GetDouble(),
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["CHART_Y_STRT_CUM_RATE"].GetDouble(),
                    UpperExtent = row["CHART_Y_END_CUM_RATE"].GetDouble(),
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    Cursor = Cursors.Hand,
                    Tag = dt.TableName,
                };

                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    if (pe is Lines)
                    {
                        double adjStarPtntPosition;
                        double adjEndPtnPosition;
                        double.TryParse(Util.NVC(row["ADJ_STRT_PTN_PSTN"]), out adjStarPtntPosition);
                        double.TryParse(Util.NVC(row["ADJ_END_PTN_PSTN"]), out adjEndPtnPosition);
                        string content = row["ABBR_NAME"] + "[" + $"{adjStarPtntPosition:###,###,###,##0.00}" + "m" + "~" + $"{adjEndPtnPosition:###,###,###,##0.00}" + "m" + "]";
                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                c1Chart.Data.Children.Add(alarmZone);
            }
        }

        private void DrawDefectTagSection(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;
            // Start, End 이미지 두번의 표현으로 for문 사용
            for (int x = 0; x < 2; x++)
            {
                DataTable dtTag = MakeTableForDisplay(dt, x == 0 ? ChartDisplayType.TagSectionStart : ChartDisplayType.TagSectionEnd, chartConfigurationType);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTag);
                ds.XValueBinding = x == 0 ? new Binding("ADJ_STRT_PTN_PSTN") : new Binding("ADJ_END_PTN_PSTN");
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

        private void DrawDefectTagSpot(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                dt = MakeTableForDisplay(dt, ChartDisplayType.TagSpot, chartConfigurationType);

                C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dt);
                ds.XValueBinding = new Binding("ADJ_STRT_PTN_PSTN");
                ds.ValueBinding = new Binding("SOURCE_ADJ_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Name = dt.TableName;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                c1Chart.Data.Children.Add(ds);
            }
        }

        private void DrawDefectMark(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            // 종축 라인 설정
            DataRow[] drMark = dt.Select();
            if (drMark.Length > 0)
            {
                foreach (DataRow row in drMark)
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));
                                        
                    double adjStartPtnPosition, adjStartPosition = 0;
                    double.TryParse(Util.NVC(row["ADJ_STRT_PTN_PSTN"]), out adjStartPtnPosition);
                    double.TryParse(Util.NVC(row["ADJ_STRT_PSTN"]), out adjStartPosition);


                    //string content = row["EQPT_MEASR_PSTN_NAME"] + "[" + Util.NVC(row["ROLLMAP_CLCT_TYPE"]) + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.00}" + "m" + ")";
                    // Ex. Lane : #1
                    //     패턴No.945EA(785.3M)
                    //
                    // 20230905 조성진, 툴팁 표현 문구 수정
                    string content;
                    content = ObjectDic.Instance.GetObjectName("LANE") + " : #" + row["ADJ_LANE_NO"].GetString() + Environment.NewLine +
                                 $"{adjStartPtnPosition:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("EA") +
                                 "(" + $"{adjStartPosition:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("M") + ")"
                                 ;

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_STRT_PTN_PSTN"].GetDouble(), row["ADJ_STRT_PTN_PSTN"].GetDouble() },
                        ValuesSource = new double[] { row["CHART_Y_STRT_CUM_RATE"].GetDouble(), row["CHART_Y_END_CUM_RATE"].GetDouble() },
                        ConnectionStroke = new SolidColorBrush(Colors.Black),
                        ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                        ToolTip = content,
                        Name = dt.TableName,
                    });
                }
            }

            //Mark 라벨 설정(슬리텅 롤맵 차트 기준점 Label 표시는 Master Lane 이 아닌경우에도 표시 함.)
            List<DataRow> queryLabel;
            if (Equals(chartConfigurationType, ChartConfigurationType.Output))
            {
                queryLabel = dt.AsEnumerable().Where(x => x.Field<decimal?>("CHART_Y_STRT_CUM_RATE") != null && x.Field<decimal?>("CHART_Y_END_CUM_RATE") != null).ToList();
            }
            else
            {
                queryLabel = dt.AsEnumerable().Where(x => x.Field<decimal?>("CHART_Y_STRT_CUM_RATE") != null && x.Field<decimal?>("CHART_Y_END_CUM_RATE") != null && Math.Abs(x.Field<decimal>("CHART_Y_STRT_CUM_RATE") - x.Field<decimal>("CHART_Y_END_CUM_RATE")) >= 100).ToList();
            }

            DataTable dtLabel = queryLabel.Any() ? MakeTableForDisplay(queryLabel.CopyToDataTable(), ChartDisplayType.MarkLabel, chartConfigurationType) : MakeTableForDisplay(dt.Clone(), ChartDisplayType.MarkLabel, chartConfigurationType);

            if (!CommonVerify.HasTableRow(dtLabel)) return;

            var dsMarkLabel = new XYDataSeries();
            dsMarkLabel.Name = dt.TableName;
            dsMarkLabel.ItemsSource = DataTableConverter.Convert(dtLabel);
            dsMarkLabel.XValueBinding = new Binding("ADJ_STRT_PTN_PSTN");
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

        private void DrawDefectRewinderScrap(DataTable dtRwScrap, string processCode)
        {
            if (CommonVerify.HasTableRow(dtRwScrap))
            {
                C1Chart c1Chart = string.Equals(processCode, Process.ROLL_PRESSING) ? chartInput : chartOutput;

                dtRwScrap.Columns.Add("TOOLTIP", typeof(string));
                dtRwScrap.Columns.Add("TAG", typeof(string));
                dtRwScrap.Columns.Add("Y_PSTN", typeof(double));

                double lowerExtent = 0;
                double upperExtent = 100;

                foreach (DataRow row in dtRwScrap.Rows)
                {
                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["ADJ_STRT_PTN_PSTN"].GetString() + ";" + row["ADJ_END_PTN_PSTN"].GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.00}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["ADJ_STRT_PTN_PSTN"]:###,###,###,##0.00}" + " ~ " + $"{row["ADJ_END_PTN_PSTN"]:###,###,###,##0.00}" + " ]";

                    row["TOOLTIP"] = toolTip;
                    row["TAG"] = tag;
                    row["Y_PSTN"] = 100;

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_END_PTN_PSTN"], row["ADJ_END_PTN_PSTN"] },
                        ValuesSource = new double[] { lowerExtent, upperExtent },
                        ConnectionStroke = convertFromString,
                        ConnectionStrokeThickness = 3,
                        ConnectionFill = convertFromString,
                        Name = dtRwScrap.TableName
                    });
                }

                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dtRwScrap);
                dsPetScrap.XValueBinding = new Binding("ADJ_END_PTN_PSTN");
                dsPetScrap.ValueBinding = new Binding("Y_PSTN");
                dsPetScrap.ChartType = ChartType.XYPlot;
                dsPetScrap.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.SymbolFill = new SolidColorBrush(Colors.Transparent);
                dsPetScrap.PointLabelTemplate = grdMain.Resources["chartPetScrap"] as DataTemplate;
                dsPetScrap.Margin = new Thickness(0, 0, 0, 0);
                dsPetScrap.Name = dtRwScrap.TableName;

                dsPetScrap.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };

                c1Chart.Data.Children.Add(dsPetScrap);
            }
        }

        private void DrawDefectLegendText(DataTable dt, ChartConfigurationType chartConfigurationType)
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

                //List<DefectLegend> queryRow;

                // Top, Back 구분 없는 범례 표시를 위한 Group 처리
                //var queryRow = dt.AsEnumerable().GroupBy(g => new
                //{
                //    DefectName = g.Field<string>("ABBR_NAME"),
                //    ColorMap = g.Field<string>("COLORMAP"),
                //    DefectShape = g.Field<string>("DEFECT_SHAPE")
                //}).Select(x => new
                //{
                //    DefectName = x.Key.DefectName,
                //    ColorMap = x.Key.ColorMap,
                //    DefectShape = x.Key.DefectShape,
                //    MeasurementCode = string.Empty
                //}).ToList();

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

        private void DrawRollMapBackGround(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            var queryBackGround = (from t in dt.AsEnumerable()
                                   where t.Field<string>("EQPT_MEASR_PSTN_ID") == "ROLLMAP_BACKGROUND"
                                   select new
                                   {
                                       StartPosition = t.Field<decimal>("ADJ_STRT_PTN_PSTN"),
                                       EndPosition = t.Field<decimal>("ADJ_END_PTN_PSTN"),
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

        private void DrawPetScrap(DataTable dtPetScrap, ChartConfigurationType chartConfigurationType)
        {
            //DataRow[] drPetScrap = dt.Select("EQPT_MEASR_PSTN_ID = 'PET' OR EQPT_MEASR_PSTN_ID = 'SCRAP' ");
            // 롤프레스 공정에서만 PET, SCRAP 호출 함.
            //if (chartConfigurationType == ChartConfigurationType.Input) return;

            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            double lowerExtent = 0;
            double upperExtent = 100;

            DataTable dt = new DataTable();
            dt.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            dt.Columns.Add("RAW_STRT_PTN_PSTN", typeof(double));
            dt.Columns.Add("RAW_END_PTN_PSTN", typeof(double));
            dt.Columns.Add("Y_PSTN", typeof(double));
            dt.Columns.Add("COLORMAP", typeof(string));
            dt.Columns.Add("TOOLTIP", typeof(string));
            dt.Columns.Add("TAG", typeof(string));

            foreach (DataRow row in dtPetScrap.Rows)
            {
                var convertFromString = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD"));
                string colorMap = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? "#000000" : "#BDBDBD";
                string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] " + $"{row["ADJ_STRT_PTN_PSTN"]:###,###,###,##0.00}" + "m";
                string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["ADJ_STRT_PTN_PSTN"].GetString() + ";" + row["ADJ_END_PTN_PSTN"].GetString()
                      + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                c1Chart.Data.Children.Add(new XYDataSeries()
                {
                    ChartType = ChartType.Line,
                    XValuesSource = new[] { row["ADJ_STRT_PTN_PSTN"], row["ADJ_STRT_PTN_PSTN"] },
                    ValuesSource = new double[] { lowerExtent, upperExtent },
                    ConnectionStroke = convertFromString,
                    ConnectionStrokeThickness = 3,
                    ConnectionFill = convertFromString,
                });

                dt.Rows.Add(row["EQPT_MEASR_PSTN_ID"], row["ADJ_STRT_PTN_PSTN"], row["ADJ_END_PTN_PSTN"], 97, colorMap, content, tag);
            }

            if (CommonVerify.HasTableRow(dt))
            {
                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dt);
                dsPetScrap.XValueBinding = new Binding("RAW_STRT_PTN_PSTN");
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

        private void DrawRewinderScrap(DataTable dtRewinderScrap, ChartConfigurationType chartConfigurationType)
        {
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
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["ADJ_STRT_PTN_PSTN"].GetString() + ";" + row["ADJ_END_PTN_PSTN"].GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.00}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["ADJ_STRT_PTN_PSTN"]:###,###,###,##0.00}" + "m" + " ~ " + $"{row["ADJ_END_PTN_PSTN"]:###,###,###,##0.00}" + "m" + " ]";

                    row["TOOLTIP"] = toolTip;
                    row["TAG"] = tag;
                    row["Y_PSTN"] = 97;

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_END_PTN_PSTN"], row["ADJ_END_PTN_PSTN"] },
                        ValuesSource = new double[] { lowerExtent, upperExtent },
                        ConnectionStroke = convertFromString,
                        ConnectionStrokeThickness = 3,
                        ConnectionFill = convertFromString,
                    });
                }

                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dtRewinderScrap);
                dsPetScrap.XValueBinding = new Binding("ADJ_END_PTN_PSTN");
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

        private void GetInOutProcessMaxLength(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
                         {
                             ProcessName = t.Field<string>("PROCNAME"),
                             EndPosition = t.Field<decimal>("END_PTN_PSTN")
                         }).FirstOrDefault();

            if (query != null)
            {
                if (chartConfigurationType == ChartConfigurationType.Input)
                {
                    tbInput.Text = query.ProcessName;
                    _xInputMaxLength = query.EndPosition.GetDouble();
                }
                else
                {
                    tbOutput.Text = query.ProcessName;
                    _xOutputMaxLength = query.EndPosition.GetDouble();
                }
            }
        }

        private void SetScale(double scale, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart;
            Button btnRefresh, btnZoomIn, btnZoomOut;

            c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;
            btnRefresh = Equals(chartConfigurationType, ChartConfigurationType.Input) ? btnInputRefresh : btnOutputRefresh;
            btnZoomIn = Equals(chartConfigurationType, ChartConfigurationType.Input) ? btnInputZoomIn : btnOutputZoomIn;
            btnZoomOut = Equals(chartConfigurationType, ChartConfigurationType.Input) ? btnInputZoomOut : btnOutputZoomOut;

            c1Chart.View.AxisX.Scale = scale;
            btnRefresh.IsCancel = !scale.Equals(1);
            btnZoomIn.IsCancel = scale > 0.002;
            btnZoomOut.IsCancel = scale < 1;
            UpdateScrollbars(chartConfigurationType);
        }

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

                SetScale(1.0, c1Chart.Name == "chartInput" ? ChartConfigurationType.Input : ChartConfigurationType.Output);
                c1Chart.View.AxisX.MinScale = 0.05;
                //c1Chart.View.Margin = c1Chart.Name == "chartInput" ? new Thickness(5, 5, 5, 25) : new Thickness(0, 10, 0, 10);
                c1Chart.View.Margin = c1Chart.Name == "chartInput" ? new Thickness(5, 5, 5, 25) : new Thickness(0, 0, 0, 25);
            }
        }

        private void EndUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                if (c1Chart.Name == "chartInput" || c1Chart.Name == "chartOutput")
                    c1Chart.View.AxisX.Reversed = true;

                c1Chart.EndUpdate();
            }
        }

        private DataTable MakeTableForDisplay(DataTable dt, ChartDisplayType chartDisplayType, ChartConfigurationType chartConfigurationType)
        {
            var dtBinding = dt.Copy();

            if (!CommonVerify.HasTableRow(dt)) return dtBinding;

            if (chartDisplayType == ChartDisplayType.MarkLabel)
            {
                //dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                //foreach (DataRow row in dtBinding.Rows)
                //{
                //    double sourceStartPosition;
                //    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PTN_PSTN"]), out sourceStartPosition);

                //    row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -7 : -19;
                //    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[" + row["ROLLMAP_CLCT_TYPE"] + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.00}" + "m" + ")";
                //}
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double adjStartPtnPosition, adjStartPosition = 0;
                    double.TryParse(Util.NVC(row["ADJ_STRT_PTN_PSTN"]), out adjStartPtnPosition);
                    double.TryParse(Util.NVC(row["ADJ_STRT_PSTN"]), out adjStartPosition);
                    
                    row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -7 : -17;    // 라벨 표시 Y 위치
                    // 20230905 조성진, 툴팁 표현 문구 수정
                     row["TOOLTIP"] = ObjectDic.Instance.GetObjectName("LANE") + " : #" + row["ADJ_LANE_NO"].GetString() + Environment.NewLine +
                                        $"{adjStartPtnPosition:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("EA") +                                            
                                        "(" + $"{adjStartPosition:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("M") + ")"
                                        ;
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSpot)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double adjStartPtnPosition;
                    double.TryParse(Util.NVC(row["ADJ_STRT_PTN_PSTN"]), out adjStartPtnPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = Equals(chartConfigurationType, ChartConfigurationType.Input) ? -12 : -27;    // 라벨 표시 Y 위치
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[" + adjStartPtnPosition + "m]";
                    row["TAG"] = null;
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionStart || chartDisplayType == ChartDisplayType.TagSectionEnd)
            {
                // Ex) 불량 구분 : L
                //     불량 구간 시작 패턴No. (좌표) ~ 끝 패턴No. (좌표) 
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    // 20230918 조성진, 툴팁 표현 문구 수정      
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -10 : -15;
                    row["TAG"] = $"{row["ADJ_STRT_PTN_PSTN"]:###,###,###,##0.00}" + ";" + $"{row["ADJ_END_PTN_PSTN"]:###,###,###,##0.00}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();
                    row["TOOLTIP"] = row["ABBR_NAME"].GetString() + Environment.NewLine +
                                     $"{row["ADJ_STRT_PTN_PSTN"]:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("EA") +
                                     "(" + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("M") + ")" +
                                     " ~ " +
                                     $"{row["ADJ_END_PTN_PSTN"]:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("EA") +
                                    "(" + $"{row["ADJ_END_PSTN"]:###,###,###,##0.00}" + ObjectDic.Instance.GetObjectName("M") + ")"
                                     ;
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";

                }
            }

            dtBinding.AcceptChanges();
            return dtBinding;
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
            newRow["MENUID"] = "SFU010160123";          // 메뉴ID 코터:SFU010160121, 롤프레스:SFU010160122, 슬리터:SFU010160123, 리와인딩:SFU010160124  
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
                string[] exceptionCode = new string[] { "OK", "NG", "Ch", "Err" };

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

        private bool ValidationSearch()
        {
            if (string.IsNullOrEmpty(txtLot.Text))
            {
                Util.MessageValidation("SFU1366");
                return false;
            }

            if (string.IsNullOrEmpty(msbRollMapLane.SelectedItemsToString))
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE"));
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

        private class DefectLegend
        {
            public string DefectName { get; set; }
            public string ColorMap { get; set; }
            public string DefectShape { get; set; }
            public string MeasurementCode { get; set; }
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

        private void SetRollMapLaneMultiSelectionBox(MultiSelectionBox msb)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_LANE";
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

            // 20230912 조성진,  제품 기준의 LANE 목록으로 조회조건 Binding
            #region 제품 기준의 LANE 정보(MIN/MAX)
            const string bizRuleName2 = "DA_PRD_SEL_ROLLMAP_LANE_CY";

            DataTable dt2 = new DataTable("RQSTDT");
            dt2.Columns.Add("LANGID", typeof(string));
            dt2.Columns.Add("EQPTID", typeof(string));
            dt2.Columns.Add("LOTID", typeof(string));
            dt2.Columns.Add("WIPSEQ", typeof(decimal));
            dt2.Columns.Add("IN_OUT", typeof(string));

            DataRow dr2 = dt2.NewRow();
            dr2["LANGID"] = LoginInfo.LANGID;
            dr2["EQPTID"] = _equipmentCode;
            dr2["LOTID"] = txtLot.Text;
            dr2["WIPSEQ"] = _wipSeq;
            dr2["IN_OUT"] = "2";
            dt2.Rows.Add(dr2);

            DataTable dtResult2 = new ClientProxy().ExecuteServiceSync(bizRuleName2, "RQSTDT", "RSLTDT", dt2);

            #endregion 제품 기준의 LANE 정보(MIN/MAX)

            // 20230912 조성진,  제품 기준의 LANE 목록으로 조회조건 Binding
            //DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();

            DataTable dtBinding;

            if (CommonVerify.HasTableRow(dtResult2))
            {
                //int laneMin = 0;
                //int laneMax = 0;
                Int32.TryParse(dtResult2.Rows[0]["LANE_MIN"].ToString(), out _xMinLane);
                Int32.TryParse(dtResult2.Rows[0]["LANE_MAX"].ToString(), out _xMaxLane);

                dtBinding = (from lane in dtResult.AsEnumerable()                                 
                             where Convert.ToInt32(lane.Field<string>("CMCODE")) >= _xMinLane && Convert.ToInt32(lane.Field<string>("CMCODE")) <= _xMaxLane
                             select lane
                       ).OrderBy(o => o.Field<decimal>("CMCDSEQ"))
                       .CopyToDataTable();
            }
            else
            {
                dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();
            }
            
            msb.ItemsSource = DataTableConverter.Convert(dtBinding);

        }
        

        private void SetRollMapExpressSummaryCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_EXPRESS_SUMMARY";
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
            //DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();

            // 20230913 조성진,  제품 기준의 LANE 목록으로 조회조건 Binding
            string[] laneArr = dtResult.Rows[0]["ATTRIBUTE3"].ToString().GetString().Split(',');

            DataTable dtBinding;
            // ATTRIBUTE3는 코터 기준으로 LANE의 가장 마지막 LANE 정보가 있는 컬럼임. 
            // 현재는 음극만 하프슬리터가 있는데, 추후 양극도 하프슬리터가 있을 경우 ATTRIBUTE 컬럼의 조정이 필요함.
            // 예) 코터의 마지막 LANE : 19, ATTRIBUTE3의 마지막 LANE : 19
            if (laneArr.Contains(_xMaxLane.ToString()))
            {
                foreach (DataRow r in dtResult.Rows)
                {
                    r["ATTRIBUTE2"] = r["ATTRIBUTE3"];
                }
                dtResult.AcceptChanges();

                dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();
            }
            else
            {
                foreach (DataRow r in dtResult.Rows)
                {
                    r["CMCODE"] = r["ATTRIBUTE4"];
                    r["ATTRIBUTE2"] = r["ATTRIBUTE5"];
                }
                dtResult.AcceptChanges();
                dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();
            }

            cbo.ItemsSource = dtBinding.Copy().AsDataView();
            if (cbo.SelectedIndex < 0)
                cbo.SelectedIndex = 0;
        }


        private void GetRollMapLaneRange()
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_LANE_CY";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WIPSEQ", typeof(decimal));
            dt.Columns.Add("IN_OUT", typeof(string));
            dt.Columns.Add("LANE_LIST", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = _equipmentCode;
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dr["IN_OUT"] = "2";
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
            _dtRollMapLane = dtResult.Copy();

            if (CommonVerify.HasTableRow(dtResult))
            {
                #region 맵 표현 요약(C1ComboBox) 및 LANE별 맵선택(MultiSelectionBox) 에 데이터 바인딩
                // No.1 Lane 별 맵 선택 MultiSelectionBox 바인딩
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

                // No.2 맵 표현 요약 ComboBox 바인딩
                var queryExpressSummary = (from t in dtResult.AsEnumerable()
                                           where !t.Field<string>("LANE").Equals("TOT")
                                           select new { CMCODE = t.Field<string>("LANE"), CMCDNAME = t.Field<string>("LANE"), LaneDescription = t.Field<string>("LANE_DESC") }).ToList();

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
                    if (cboRollMapExpressSummary.SelectedIndex < 0) cboRollMapExpressSummary.SelectedIndex = 0;

                }

                #endregion 
            }
        }
        #endregion


    }
}