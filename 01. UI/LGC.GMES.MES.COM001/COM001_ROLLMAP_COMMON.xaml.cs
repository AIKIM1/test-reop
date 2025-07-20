/*************************************************************************************
 Created Date : 2022.04.30
      Creator : 신광희
   Decription : 자동차 전극 Roll Map - 범용차트(동일한 BizRule을 호출 하여 공정 구분 없이 범용으로 사용 목적)
--------------------------------------------------------------------------------------
 [Change History]
  2022.04.30  신광희 : Initial Created
  2024.03.19  김지호 : [E20240319-000656] 범례에 Non 추가 및 차트 색 (회색) 추가, SCAN_COLRMAP에 Err, Non, Ch 시에 Tooltip에 데이터 오류, 데이터 미수신, 데이터 미측정 내용 표시
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

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_ROLLMAP_COMMON : C1Window, IWorkArea
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

        #endregion

        public COM001_ROLLMAP_COMMON()
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
            // Input, Output Text설정
            GetRollMap();
        }

        #region # Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            InitializeControls();
            InitializeDataTables();
            //InitializeRollMapGrid();
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

        private void rdoA_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
        }

        private void rdoP_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
        }

        private void rdoWB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rdoTHICK_Click(object sender, RoutedEventArgs e)
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
                if (textBlock?.Tag == null) return;

                ShowLoadingIndicator();
                DoEvents();

                C1Chart c1Chart;
                DataTable dtDefect, dtBaseDefect;

                if (string.Equals(textBlock.Tag.ToString(), ChartConfigurationType.Input.ToString()))
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

                if (textBlock.Foreground == Brushes.Black)
                {
                    textBlock.Foreground = Brushes.LightGray;
                    dtDefect.Rows.Add(textBlock.Text, textBlock.Text);
                }
                else
                {
                    dtDefect.Select("EQPT_MEASR_PSTN_ID = '" + textBlock.Text + "' And ABBR_NAME = '" + textBlock.Text + "'").ToList().ForEach(row => row.Delete());
                    dtDefect.AcceptChanges();
                    textBlock.Foreground = Brushes.Black;
                }
                var queryDefect = dtBaseDefect.AsEnumerable()
                    .Where(x => !dtDefect.AsEnumerable().Any(y => y.Field<string>("ABBR_NAME") == x.Field<string>("ABBR_NAME")));
                if (queryDefect.Any()) DrawDefect(queryDefect.CopyToDataTable(), (ChartConfigurationType)Enum.Parse(typeof(ChartConfigurationType), textBlock.Tag.ToString()), true);

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

        #endregion

        #region # Method
        private void Initialize()
        {
            try
            {
                ROLLMAP.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                ROLLMAP.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;

                InitializeControl();
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
            _dtLineLegend.Rows.Add(1, "#F2685E", "HH", "LOAD", "RECT");
            _dtLineLegend.Rows.Add(2, "#FFA648", "H", "LOAD", "RECT");
            _dtLineLegend.Rows.Add(3, "#8DC63F", "SV", "LOAD", "RECT");
            _dtLineLegend.Rows.Add(4, "#618FFC", "L", "LOAD", "RECT");
            _dtLineLegend.Rows.Add(5, "#003399", "LL", "LOAD", "RECT");

            // Lane 범례 색상 
            _dtLaneLegend = new DataTable();
            _dtLaneLegend.Columns.Add("LANE_NO", typeof(string));
            _dtLaneLegend.Columns.Add("COLORMAP", typeof(string));
            _dtLaneLegend.Rows.Add("1", "#FFFFD700");   //Gold
            _dtLaneLegend.Rows.Add("2", "#FFFF4500");   //OrangeRed
            _dtLaneLegend.Rows.Add("3", "#FF87CEEB");   //SkyBlue   
            _dtLaneLegend.Rows.Add("4", "#FF00FF00");   //Lime

            // 두께 데이터 Lane 정보
            _dtLane = new DataTable();
            _dtLane.Columns.Add("LANE_NO", typeof(string));

            _dtDefectOutput = new DataTable();
            _dtDefectOutput.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefectOutput.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefectInput = _dtDefectOutput.Copy();

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
            chart.View.AxisY.Min = -30;
            //chart.View.AxisY.Min = chart.Name == "chartInput" ? -10 : -30; // TODO 수정 필요 함. 차트 하단의 태그.. 등의 표시로  AxisY Min 값을 설정 함.
            chart.View.AxisX.Max = maxLength;
            chart.View.AxisY.Max = 100;
            InitializeChartView(chart);
        }

        private void InitializeRollMapGrid()
        {
            // One Source 로 차트 표현을 위하여 차트 표현하는 그리드 Height 변경

            grdRollMap.RowDefinitions[0].Height = new GridLength(0.8, GridUnitType.Star);
            grdRollMap.RowDefinitions[1].Height = new GridLength(5, GridUnitType.Pixel);
            grdRollMap.RowDefinitions[2].Height = new GridLength(5.0, GridUnitType.Star);
            grdRollMap.RowDefinitions[3].Height = new GridLength(10, GridUnitType.Pixel);
            grdRollMap.RowDefinitions[4].Height = new GridLength(5.0, GridUnitType.Star);
            grdRollMap.RowDefinitions[5].Height = new GridLength(0, GridUnitType.Auto);

            UIElement ul0 = grdRollMap.Children.Cast<UIElement>().FirstOrDefault(uie => Grid.GetRow(uie) == 0 && Grid.GetColumn(uie) == 0);
            UIElement ul1 = grdRollMap.Children.Cast<UIElement>().FirstOrDefault(uie => Grid.GetRow(uie) == 1 && Grid.GetColumn(uie) == 0);
            UIElement ul2 = grdRollMap.Children.Cast<UIElement>().FirstOrDefault(uie => Grid.GetRow(uie) == 2 && Grid.GetColumn(uie) == 0);


            //grdRollMap.RowDefinitions[0].Height = new GridLength(0.8, GridUnitType.Star);
            //grdRollMap.RowDefinitions[1].Height = new GridLength(500, GridUnitType.Pixel);
            //grdRollMap.RowDefinitions[2].Height = new GridLength(4.0, GridUnitType.Star);
            //grdRollMap.RowDefinitions[3].Height = new GridLength(10, GridUnitType.Pixel);
            //grdRollMap.RowDefinitions[4].Height = new GridLength(4.0, GridUnitType.Star);
            //grdRollMap.RowDefinitions[5].Height = new GridLength(0, GridUnitType.Auto);

            /*
                <Grid.RowDefinitions>
                    <RowDefinition Height="0.8*" />
                    <RowDefinition Height="5" />
                    <RowDefinition Height="5.0*" />
                    <RowDefinition Height="10" />
                    <RowDefinition Height="5.0*" />
                    <RowDefinition Height="Auto" />                    
                </Grid.RowDefinitions>
            */
        }

        private void GetLegend()
        {
            grdLegend.Children.Clear();

            DataTable dt = _dtLineLegend.Copy();
            
            if ((bool)rdoP?.IsChecked)
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
                if (rdoA.IsChecked != null && rdoA.IsChecked == true)
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
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CHART";
            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = _runLotId;
            newRow["WIPSEQ"] = _wipSeq;
            newRow["ADJFLAG"] = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";

            // 실전 테스트 데이터
            //newRow["LANGID"] = LoginInfo.LANGID;
            //newRow["EQPTID"] = "A1ESLT001";
            //newRow["LOTID"] = "FAACQ11Q11";
            //newRow["WIPSEQ"] = 1;
            //newRow["ADJFLAG"] = "1";
            inTable.Rows.Add(newRow);

            //string xml = inDataSet.GetXml();
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
                        }

                        if (CommonVerify.HasTableRow(dtInGauge))
                        {
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
        /// 차트 상단 시작위치 종료위치 표시 및 chartInput 차트 인경우 Cut, MergeLot 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="processCode"></param>
        private void DrawStartEndYAxis(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            //chartInput, chartOutput 상단 Rewinder 길이 표시
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW" select
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

                DataRow newRow = dtLength.NewRow();
                newRow["RAW_END_PSTN"] = "0";
                newRow["SOURCE_Y_PSTN"] = 100;
                dtLength.Rows.Add(newRow);

                DataRow newLength = dtLength.NewRow();
                newLength["RAW_END_PSTN"] = $"{query.EndPosition:###,###,###,##0.##}";
                newLength["SOURCE_Y_PSTN"] = 100;
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

                foreach (var item in querycut)
                {
                    DataRow newLength = dtLength.NewRow();
                    newLength["RAW_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                    newLength["SOURCE_Y_PSTN"] = 100;
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

            // chartInput 차트 영역 상단 LOT MergeLot 영역 표시
            var queryMergeLot = (from t in dt.AsEnumerable()
                                 where t.Field<string>("EQPT_MEASR_PSTN_ID") == "MERGE_PSTN" select
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
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "TAG_SECTION" };
            //var queryGauge = dt.AsEnumerable().Where(x => !gaugeExceptions.Contains(x.Field<string>("EQPT_MEASR_PSTN_ID"))).Select(y => y);
            var queryGauge = dt.AsEnumerable().Where(o => !gaugeExceptions.Contains(o.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList();
            DataTable dtGauge = queryGauge.Any() ? queryGauge.CopyToDataTable() : dt.Clone();
            
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
                        double.TryParse(Util.NVC(dtGauge.Rows[x]["ADJ_STRT_PSTN"]), out sourceStartPosition);
                        double.TryParse(Util.NVC(dtGauge.Rows[x]["ADJ_END_PSTN"]), out sourceEndPosition);

                        string content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                         "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
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

        /// <summary>
        /// chartThickness 차트의 두께 측정데이터 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawThickness(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            string[] gaugeExceptions = new string[] { "PET", "SCRAP", "RW_SCRAP", "TAG_SECTION", "COT_WIDTH_LEN_BACK" };
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
                        dsLegend.ConnectionStrokeDashes = new DoubleCollection { 3, 2 };

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

                        var query = dt.AsEnumerable().ToList();
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
                        ds.SymbolSize = new Size(7, 7);

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

                                string content = rdoTHICK.IsChecked != null && (bool)rdoTHICK.IsChecked ? "Thickness : " : "Load : ";
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
                txtInputPosition1.Text = "B";
                txtInputPosition2.Text = "T";
            }
            else
            {
                txtInputPosition1.Text = "T";
                txtInputPosition2.Text = "B";
            }
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
                             EndPosition = t.Field<decimal>("END_PSTN")
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
                var convertFromString = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD"));
                string colorMap = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? "#000000" : "#BDBDBD";
                string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] " + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m";
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

                                    TextBlock rectangleDescription = CreateTextBlock(item.DefectName,
                                                                                     HorizontalAlignment.Center,
                                                                                     VerticalAlignment.Center,
                                                                                     11,
                                                                                     FontWeights.Bold,
                                                                                     Brushes.Black,
                                                                                     new Thickness(1, 1, 1, 1),
                                                                                     new Thickness(0, 0, 0, 0),
                                                                                     item.MeasurementCode,
                                                                                     Cursors.Hand,
                                                                                     chartConfigurationType.ToString());
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

                                    TextBlock ellipseDescription = CreateTextBlock(item.DefectName,
                                                                                   HorizontalAlignment.Center,
                                                                                   VerticalAlignment.Center,
                                                                                   11,
                                                                                   FontWeights.Bold,
                                                                                   Brushes.Black,
                                                                                   new Thickness(1, 1, 1, 1),
                                                                                   new Thickness(0, 0, 0, 0),
                                                                                   item.MeasurementCode,
                                                                                   Cursors.Hand,
                                                                                   chartConfigurationType.ToString());
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
                    c1Chart.View.Margin = new Thickness(5, 0, 5, 20);
                    //c1Chart.View.Margin = c1Chart.Name == "chartInput" ? new Thickness(5, 5, 5, 25) : new Thickness(5, 5, 5, 25);
                }
                else if (c1Chart.Name == "chartThickness")
                {
                    c1Chart.View.Margin = new Thickness(5, 0, 5, 5);
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
                    row["SOURCE_ADJ_Y_PSTN"] = -13;
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
                    row["SOURCE_ADJ_Y_PSTN"] = -28;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[" + sourceStartPosition + "m]";
                    row["TAG"] = null;
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionStart || chartDisplayType == ChartDisplayType.TagSectionEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -20 : -27;
                    row["TAG"] = null;
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                }

            }

            dtBinding.AcceptChanges();
            return dtBinding;
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

        #endregion


    }
}