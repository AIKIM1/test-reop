/*************************************************************************************
 Created Date : 2024.03.18
      Creator : 신광희
   Decription : 리와인더 롤맵 팝업(NT향 소형 파우치) - 자동차 리와인더 롤맵과 통합 작업 필요.
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.18  신광희 : Initial Created
  2024.10.25  신광희 : JSON 결과값 Deserialize 시 Data Type 비정상으로 Linq 사용 시 Data Type 부분 수정
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

    public partial class COM001_RM_CHART_RW : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        private DataTable _dtDefectOutput;
        private DataTable _dtDefectInput;
        private DataTable _dtBaseDefectOutput;
        private DataTable _dtBaseDefectInput;
        private DataTable _dtColorMapLegend;
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _runLotId = string.Empty;
        private string _wipSeq = string.Empty;
        private double _xInputMaxLength;
        private double _xOutputMaxLength;

        #endregion

        public COM001_RM_CHART_RW()
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
                //_laneQty = Util.NVC(parameters[5]);
                _equipmentName = Util.NVC(parameters[6]);
            }

            Initialize();
            GetRollMap();
        }

        #region # Event

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtLot.Text.Trim()))
                        btnSearch_Click(btnSearch, null);
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            InitializeControls();
            InitializeDataTables();
            SelectRollMap();
            EndUpdateChart();
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
            GetColorMapSpecLegend();
        }

        private void rdoRelativeCoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetColorMapSpecLegend();
        }

        private void rdoWebgauge_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rdoThickness_Click(object sender, RoutedEventArgs e)
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

        private void gdPetScrap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void popRollMapScrap_Closed(object sender, EventArgs e)
        {
            //CMM_ROLLMAP_SCRAP => CMM_RM_SCRAP
            CMM_RM_SCRAP popup = sender as CMM_RM_SCRAP;
            if (popup != null && popup.IsUpdated)
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void popRollMapRewinderScrap_Closed(object sender, EventArgs e)
        {
            //CMM_ROLLMAP_RW_SCRAP => CMM_RM_RW_SCRAP
            CMM_RM_RW_SCRAP popup = sender as CMM_RM_RW_SCRAP;
            if (popup != null && popup.IsUpdated)
            {
                btnSearch_Click(btnSearch, null);
            }
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

        #endregion

        #region # Method
        private void Initialize()
        {
            try
            {
                InitializeWindowResize();
                InitializeControl();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeWindowResize()
        {
            windowRollMap.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
            windowRollMap.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;
        }

        private void InitializeControl()
        {
            txtLot.Text = _runLotId;
            txtEquipmentName.Text = _equipmentName;

            InitializeGroupBox();
            InitializechartViewXScrollBar();
            SetColorMapSpecTable();
            SetDefectTable();
            GetColorMapSpecLegend();
            InitializeCombo();//추가
        }

        private void InitializeGroupBox()
        {
            //조회 영역 GroupBox Visibility 속성 설정 - 동별 공통코드 로 관리 필요 함.
            gdMeasurementradio.Visibility = Visibility.Collapsed;
            gdMapExpress.Visibility = Visibility.Collapsed;
            gdColorMap.Visibility = Visibility.Collapsed;

            SetRollMapSearchGroupBox();
        }

        private void InitializechartViewXScrollBar()
        {
            chartInput.View.AxisX.ScrollBar = new AxisScrollBar();
            chartOutput.View.AxisX.ScrollBar = new AxisScrollBar();
        }

        private void SetColorMapSpecTable()
        {
            _dtColorMapLegend = new DataTable();
            _dtColorMapLegend.Columns.Add("NO", typeof(int));
            _dtColorMapLegend.Columns.Add("COLORMAP", typeof(string));
            _dtColorMapLegend.Columns.Add("VALUE", typeof(string));
            _dtColorMapLegend.Columns.Add("GROUP", typeof(string));
            _dtColorMapLegend.Columns.Add("SHAPE", typeof(string));

            DataTable dtColorMapSpec = GetColorMapSpec("ROLLMAP_COLORMAP_SPEC");
            if (CommonVerify.HasTableRow(dtColorMapSpec))
            {
                foreach (DataRow row in dtColorMapSpec.Rows)
                {
                    _dtColorMapLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCDNAME"].GetString(), "LOAD", "RECT");
                }
            }
        }

        private void SetDefectTable()
        {
            _dtDefectOutput = new DataTable();
            _dtDefectOutput.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefectOutput.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefectOutput.Columns.Add("COLORMAP", typeof(string));
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

            tbInputTopupper.Text = string.Empty;
            tbInputToplower.Text = string.Empty;
            tbInputBackupper.Text = string.Empty;
            tbInputBacklower.Text = string.Empty;
            tbOutputTopupper.Text = string.Empty;
            tbOutputToplower.Text = string.Empty;
            tbOutputBackupper.Text = string.Empty;
            tbOutputBacklower.Text = string.Empty;

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
            chart.View.AxisY.Min = -30;
            //chart.View.AxisY.Min = chart.Name == "chartInput" ? -10 : -30; // TODO 수정 필요 함. 차트 하단의 태그.. 등의 표시로  AxisY Min 값을 설정 함.
            chart.View.AxisX.Max = maxLength;
            chart.View.AxisY.Max = 100;
            InitializeChartView(chart);
        }

        private void GetColorMapSpecLegend()
        {

            grdLegendTop.Children.Clear();
            grdLegendBack.Children.Clear();
            DataTable dt = _dtColorMapLegend.Copy();

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
                ColumnDefinition gridcolumnTop = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                grdLegendTop.ColumnDefinitions.Add(gridcolumnTop);

                if (rdoAbsolutecoordinates.IsChecked == true)
                {
                    ColumnDefinition gridcolumnBack = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                    grdLegendBack.ColumnDefinitions.Add(gridcolumnBack);
                }
            }

            StackPanel stackPanelTop = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 3)
            };

            StackPanel stackPanelBack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 3)
            };

            DataRow[] dr = dt.Select();
            int indexLegend = 0;

            if (dr.Length > 0)
            {
                foreach (DataRow row in dr)
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                    Rectangle rectangleLegend = new Rectangle
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Height = 15,
                        Width = 15,
                        StrokeThickness = 1,
                        Margin = new Thickness(5, 0, 5, 3),
                        Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                    };
                    TextBlock textBlockDescription = new TextBlock
                    {
                        Text = Util.NVC(row["VALUE"]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        //FontWeight = FontWeights.Bold,
                        Margin = new Thickness(5, 0, 5, 3)
                    };

                    if (indexLegend < 6)
                    {
                        stackPanelTop.Children.Add(rectangleLegend);
                        stackPanelTop.Children.Add(textBlockDescription);
                    }
                    else
                    {
                        stackPanelBack.Children.Add(rectangleLegend);
                        stackPanelBack.Children.Add(textBlockDescription);
                    }

                    indexLegend++;
                }

                Grid.SetColumn(stackPanelTop, 0);
                Grid.SetRow(stackPanelTop, 1);
                grdLegendTop.Children.Add(stackPanelTop);

                Grid.SetColumn(stackPanelBack, 0);
                Grid.SetRow(stackPanelBack, 1);
                grdLegendBack.Children.Add(stackPanelBack);


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
                //grdLegend.Children.Add(stackPanel);

                if((bool)rdoAbsolutecoordinates?.IsChecked)
                {
                    stackPanelBack.Children.Add(stackPanel);
                }
                else
                {
                    stackPanelTop.Children.Add(stackPanel);
                }

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
                //grdLegend.Children.Add(spLotCut);

                if ((bool)rdoAbsolutecoordinates?.IsChecked)
                {
                    grdLegendBack.Visibility = Visibility.Visible;
                    stackPanelBack.Children.Add(spLotCut);
                }
                else
                {
                    grdLegendBack.Visibility = Visibility.Collapsed;
                    stackPanelTop.Children.Add(spLotCut);
                }

            }

            /*
            grdLegend.Children.Clear();

            DataTable dt = _dtColorMapLegend.Copy();

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
            inTable.Columns.Add("LANE_LIST", typeof(string));//요약 LANE LIST(DELITER:콤마)
            
            int laneCnt        = msbRollMapLane.SelectedItems.Count;
            string strLaneList = msbRollMapLane.SelectedItemsToString;//요약 LANE LIST(DELITER:콤마 1,3,5) 

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = txtLot.Text;
            newRow["WIPSEQ"] = _wipSeq;
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

                    SetMenuUseCount();
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
                            DrawLane(dtInLane, ChartConfigurationType.Input);
                            DrawWinderDirection(dtInLane);
                            DisplayTabLocation(dtInLane, ChartConfigurationType.Input);
                        }

                        if (CommonVerify.HasTableRow(dtInGauge))
                        {
                            DrawGauge(dtInGauge, ChartConfigurationType.Input);
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
                            DrawGauge(dtGauge, ChartConfigurationType.Output);

                        if (CommonVerify.HasTableRow(dtDefect))
                        {
                            DrawDefect(dtDefect, ChartConfigurationType.Output);
                            DrawDefectLegend(dtDefect, ChartConfigurationType.Output);
                        }

                        DrawScrapForInLot();    //nathan 2024.04.09 OUT LOT의 Scrap 범위를 투입 LOT 에 표시

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
        /// <param name="processCode"></param>
        private void DrawStartEndYAxis(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            //chartInput, chartOutput 상단 Rewinder 길이 표시
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
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

            // chartInput 차트 영역 상단 LOT MergeLot 영역 표시
            var queryMergeLot = (from t in dt.AsEnumerable()
                                 where t.Field<string>("EQPT_MEASR_PSTN_ID") == "MERGE_PSTN"
                                 select new
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

            var queryTopBack = (from t in dt.AsEnumerable() select new { TopBackFlag = t.Field<string>("T_B_FLAG") }).Distinct().ToList();
            int topBackCount = queryTopBack.Count;

            if (queryTopBack.Any())
            {
                foreach (var item in queryTopBack)
                {
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
                                    FirstTopBackLaneRate = s.Min(z => z.GetValue("Y_PSTN_STRT_RATE").GetDecimal()),
                                    LastTopBackLaneRate = s.Max(z => z.GetValue("Y_PSTN_END_RATE").GetDecimal())
                                }).FirstOrDefault();

                            if (queryTopBackLaneRate != null)
                            {
                                DrawLaneText(queryTopBackLaneRate.FirstTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LastTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LaneNo, chartConfigurationType);
                            }

                        }
                    }
                }
            }


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
                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.GetValue("CNT").GetInt() < item["CNT"].GetInt()))
                                {
                                    islower = true;
                                }

                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.GetValue("CNT").GetInt() > item["CNT"].GetInt()))
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

            // 불량 범례
            if (Equals(chartConfigurationType, ChartConfigurationType.Input))
            {
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
                    dtDefectLegend.TableName = "DefectLegend";
                    DrawDefectLegendText(dtDefectLegend, chartConfigurationType);
                }
            }
            else
            {
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
                    dtDefectLegend.TableName = "DefectLegendOut";
                    DrawDefectLegendText(dtDefectLegend, chartConfigurationType);
                }
            }
        }

        private void DrawGauge(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            //웹게이지, 두께 측정데이터가 없는 경우 롤맵 차트 배경색은 공통코드에 등록된 배경색 으로 지정 함.
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
                        double sourceStartPosition = Convert.ToDouble(dtGauge.Rows[x]["SOURCE_ADJ_STRT_PSTN"]);
                        double sourceEndPosition = Convert.ToDouble(dtGauge.Rows[x]["SOURCE_ADJ_END_PSTN"]);

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

                    string content = defectName + " [" + ObjectDic.Instance.GetObjectName("길이") + ":" + $"{xposition:###,###,###,##0.##}" + "m" + ", " + ObjectDic.Instance.GetObjectName("폭") + ":" + $"{yposition:###,###,###,##0.##}" + "mm" + "]";
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
                    Near = row["ADJ_STRT_PSTN"].GetDouble(),
                    Far = row["ADJ_END_PSTN"].GetDouble(),
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["CHART_Y_STRT_CUM_RATE"].GetDouble(),
                    UpperExtent = row["CHART_Y_END_CUM_RATE"].GetDouble(),
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    Cursor = Cursors.Hand,
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
                        Name = dt.TableName
                    });
                }
            }

            //Mark 라벨 설정(마스터 기준점에 대하여 라벨 표시)
            var queryLabel = dt.AsEnumerable().Where(x => x.GetValue("CHART_Y_STRT_CUM_RATE") != null && x.GetValue("CHART_Y_STRT_CUM_RATE").ToString() != null
            && x.GetValue("CHART_Y_END_CUM_RATE") != null && x.GetValue("CHART_Y_END_CUM_RATE").ToString() != null
            && Math.Abs(x.GetValue("CHART_Y_STRT_CUM_RATE").GetDecimal() - x.GetValue("CHART_Y_END_CUM_RATE").GetDecimal()) >= 100).ToList();

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

            dsMarkLabel.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };
            c1Chart.Data.Children.Add(dsMarkLabel);
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

                //// Top, Back 구분 없는 범례 표시를 위한 Group 처리
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
                string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] " + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m";
                string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["ADJ_STRT_PSTN"].GetString() + ";" + row["ADJ_END_PSTN"].GetString()
                      + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

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
                double upperExtent = 220;

                foreach (DataRow row in dtRewinderScrap.Rows)
                {
                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["SOURCE_ADJ_STRT_PSTN"].GetString() + ";" + row["SOURCE_ADJ_END_PSTN"].GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + " ]";

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

                    double SOURCE_START_PSTN = (row["SOURCE_START_PSTN"] == null || row["SOURCE_START_PSTN"] == DBNull.Value) ? 0 : double.Parse(row["SOURCE_START_PSTN"].ToString());
                    double SOURCE_END_PSTN = (row["SOURCE_END_PSTN"] == null || row["SOURCE_END_PSTN"] == DBNull.Value) ? 0 : double.Parse(row["SOURCE_END_PSTN"].ToString());

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
                Util.MessageException(ex);
            }
        }

        private void GetInOutProcessMaxLength(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
                         {
                             ProcessName = t.Field<string>("PROCNAME"),
                             EndPosition = t.GetValue("END_PSTN").GetDecimal()
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

                if (c1Chart.Name == "chartOutput" || c1Chart.Name == "chartInput")
                {
                    SetScale(1.0, c1Chart.Name == "chartInput" ? ChartConfigurationType.Input : ChartConfigurationType.Output);
                    c1Chart.View.AxisX.MinScale = 0.05;
                    c1Chart.View.Margin = c1Chart.Name == "chartInput" ? new Thickness(0) : new Thickness(0,0,0,30);
                    c1Chart.Padding = new Thickness(10, 2, 5, 5);
                }
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
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = -10;
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
                    row["SOURCE_ADJ_Y_PSTN"] = -29;
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
                    if(chartConfigurationType == ChartConfigurationType.Input)
                    {
                        row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -16 : -22;
                    }
                    else
                    {
                        row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -10 : -16;
                    }
                    row["TAG"] = null;
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + " : " + row["DFCT_NAME"].GetString() + "\n" + "[" + $"{Convert.ToDouble(row["SOURCE_ADJ_END_PSTN"]) - Convert.ToDouble(row["SOURCE_ADJ_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
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
            newRow["MENUID"] = "SFU010160124";          // 메뉴ID 코터:SFU010160121, 롤프레스:SFU010160122, 슬리터:SFU010160123, 리와인딩:SFU010160124  
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
                        orderby t.GetValue("CMCDSEQ").GetDecimal() ascending
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

            if (gdMapExpress.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(msbRollMapLane.SelectedItemsToString))
                {
                    // %1(을)를 선택하세요.
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE"));
                    return false;
                }
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

        #region 맵표현설정 관련
        
        #region InitializeCombo
        /// <summary>
        /// 로드시 상단조건 Combo 일괄설정 (1.맵표현설정:요약/Lane선택) 
        /// </summary>
        private void InitializeCombo()
        {
            if (gdMapExpress.Visibility == Visibility.Visible) //맵표현설정 사용시에만 레인정보 가져온다.
            {
                SetRollMapExpressionAndLines();
            }
        }
        #endregion

        private void SetRollMapSearchGroupBox()
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID; //;
            newRow["COM_TYPE_CODE"] = "ROLLMAP_HEADER_COND_VISIBILITY";
            newRow["COM_CODE"] = _processCode;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                // 계측기 콤보박스 Visibility 속성
                if (string.Equals(dtResult.Rows[0]["ATTR1"].GetString(), "Y"))
                {
                    gdMeasurementradio.Visibility = Visibility.Collapsed;
                }
                else
                {
                    gdMeasurementradio.Visibility = Visibility.Visible;
                }
                // 맵표현 설정 그룹박스 영역 Visibility 속성
                if (string.Equals(dtResult.Rows[0]["ATTR2"].GetString(), "Y"))
                {
                    gdMapExpress.Visibility = Visibility.Visible;
                }
                else
                {
                    gdMapExpress.Visibility = Visibility.Collapsed;
                }
                if (string.Equals(dtResult.Rows[0]["ATTR3"].GetString(), "Y"))
                {
                    gdColorMap.Visibility = Visibility.Visible;
                }
                else
                {
                    gdColorMap.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                gdMeasurementradio.Visibility = Visibility.Collapsed;
                gdMapExpress.Visibility = Visibility.Collapsed;
                gdColorMap.Visibility = Visibility.Collapsed;
            }
        }

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
                inTable.Columns.Add("LANGID"       , typeof(string));//추가
                inTable.Columns.Add("AREAID"       , typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE"     , typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"]        = LoginInfo.LANGID;//추가
                newRow["AREAID"]        = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = cmcdType;
                newRow["COM_CODE"]      = cmCode;
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
            DataRow dr   = dt.Rows[cboRollMapExpressSummary.SelectedIndex];

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
                DataTable dtExp  = GetAreaCommonCode("ROLLMAP_EXPRESS_SUMMARY");
                DataTable dtLane = GetAreaCommonCode("ROLLMAP_LANE");

                if (CommonVerify.HasTableRow(dtExp) && CommonVerify.HasTableRow(dtLane))
                {
                    DataTable dtMulti = new DataTable();
                    dtMulti.Columns.Add("CMCDNAME"  , typeof(string));
                    dtMulti.Columns.Add("CMCODE"    , typeof(string));
                    dtMulti.Columns.Add("ATTRIBUTE1", typeof(string));

                    #region 1.맵표현설정(레인선택)
                    foreach (DataRow drLane in dtLane.Rows)
                    {
                        DataRow drNew = dtMulti.NewRow();
                        drNew["CMCODE"]   = drLane["COM_CODE"];
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
                        drNew["CMCODE"]     = drExp["COM_CODE"];
                        drNew["CMCDNAME"]   = drExp["COM_CODE_NAME"];
                        drNew["ATTRIBUTE1"] = drExp["ATTR1"];
                        dtCbo.Rows.Add(drNew);
                    }

                    cboRollMapExpressSummary.ItemsSource = DataTableConverter.Convert(dtCbo);
                    #endregion
                
                    cboRollMapExpressSummary.SelectedIndex = cboRollMapExpressSummary.Items.Count -1;//전체 Lane 
                    //SetMsbRollMapLaneCheck();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [이벤트]맵표현설정:요약 선택시 LANE 선택 체크
        private void cboRollMapExpressSummary_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetMsbRollMapLaneCheck();
        }
        #endregion

        #endregion

        #endregion

    }
}