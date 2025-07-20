
/*************************************************************************************
 Created Date : 2024.02.15
      Creator : 조성근
   Decription : 이전공정 사전알람 모니터링화면
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.15  조성근 : Initial Created.
  2024.06.12  정기동 : 부분 LANE 불량 툴팁 표기 기능 (TAG_SECTION_LANE) 및 구간 불량.LANE 불량 정보 목록 조회되도록 수정
  2024.07.23  조성근 : 특이사항 안나오는 오류 수정
  2024.07.30  황석동 : SCRAP 대각선으로 표시 수정
  2024.08.13  김지호 : E20240724-000827 TAG_SECTION_SINGLE 추가에 따른 수정
  2025.07.11  조성근 : 2.0 버전 컨버팅 및 차트검색 BIZ 변경
**************************************************************************************/


using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Collections.Generic;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
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
using System.Windows.Interop;

using System.Runtime.InteropServices;
using C1.WPF.DataGrid;

using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.MNT001
{

    public partial class MNT001_022 : Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _wipSeq = string.Empty;
        private string _laneQty = string.Empty;
        private double _xInputMaxLength = 1000;
        private DataTable _dtLineLegend;
        private DataTable _dtEioAttr;
        private static DataTable _dt2DBarcodeInfo;
        private bool _isClosed = false;
        private double _MinusRate = -4;
        private bool _bAfterLoading = false;
        private bool _bReverseFlag = false;
        private bool _isMinMaxEqpt = false;  //2024-12-17 웹게이지 MIN/MAX 툴팁 표시여부(설비기준)



        private C1Chart chartOutput = null;
        private Grid grdOutputDefectLegend = null;

        private Util _Util = new Util();

        DispatcherTimer _dispatcherTimer = null;

        delegate void FHideWindow();

        #endregion




        public MNT001_022()
        {
            //MainWindow에서 MNT001 로 시작하는 메뉴는 ShowDialog() 로 띄우기로 되어있으므로
            //모달리스로 하기 위해선 해당 화면을 닫고 새로운 창을 연다.
            this.Closing += MNT001_022_Closing;
            //this.LocationChanged += MNT001_022_LocationChanged;
            InitializeComponent();
            this.Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _equipmentCode = LoginInfo.CFG_EQPT_ID; //사용자 설비정보
            _processCode = LoginInfo.CFG_PROC_ID;   //사용자 PROC 정보

            //AutoSearchText.Text = "2";  //TEST 용
            Initialize();
            GetLegend();

            InitializeChartView(chartInput);

            if (_dispatcherTimer == null)
            {
                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Tick += DispatcherTimer_Tick;  //이거 여러번 해주면 타이머에서 함수호출을 여러번 해줌.
            }

            doTimeStart();
            _bAfterLoading = true;
        }



        #region # Event

        //자동호출 이벤트
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            doTimeStart();

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            doTimeStart(true);
        }

        //좌표선택 (상대좌표, 절대좌표)
        private void rdoCoording_Click(object sender, RoutedEventArgs e)
        {
            if (_bAfterLoading == false) return;
            GetLegend();
            doTimeStart(true);
        }


        private void OriginalSize_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            // View.AxisX.Scale 값이 0.05 이하인 경우 AlarmZone 영역의 데이터가 나오지 않는 경우가 발생하여 강제 처리 함.
            if (chartInput.View.AxisX.MinScale >= chartInput.View.AxisX.Scale * 0.75)
                return;

            SetScale(chartInput.View.AxisX.Scale * 0.75);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartInput.View.AxisX.Scale * 1.50);
        }


        //좌우반전
        private void btnReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartInput.BeginUpdate();
            chartInput.View.AxisX.Reversed = !chartInput.View.AxisX.Reversed;
            chartInput.EndUpdate();
        }


        /// <summary>
        /// 자동조회 시간 변경 발생 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((int)AutoSearchText.Text.GetInt() < 5)
                {
                    //%1 초 이하는 입력할 수 없습니다. _ 여기는 5초 이하 입력 불가 
                    Util.MessageValidation("SFU9953", 4);
                    AutoSearchText.Text = "5";
                    return;
                }

                StopTimer();
                StartTimer();
            }
        }

        //해당화면 닫을 때 이벤트
        private void MNT001_022_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_bAfterLoading == true)
            {
                _isClosed = true;
                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();
                    _dispatcherTimer = null;
                }
            }
            e.Cancel = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FHideWindow(doHideThisWindow));
        }

        //닫기버튼 눌렀을 때 이벤트
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _isClosed = true;
            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer = null;
            }
            this.Close();
        }

        #endregion

        #region # Method
        private void Initialize()
        {
            try
            {
                InitializeControl();

                DataTable dt = SelectEquipmentPolarity(_equipmentCode);

                if (CommonVerify.HasTableRow(dt))
                {

                    if (CommonVerify.HasTableRow(dt))
                    {
                        txtEquipmentName.Text = dt.Rows[0]["EQPTNAME"].GetString() + " [" + _equipmentCode + "]";
                    }
                    else
                    {
                        txtEquipmentName.Text = _equipmentCode;
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessageToWipNote(GetExceptionMessage(ex));
            }
        }

        private void InitializeControl()
        {
            chartInput.View.AxisX.ScrollBar = new AxisScrollBar();

            _dtLineLegend = new DataTable();
            _dtLineLegend.Columns.Add("NO", typeof(int));
            _dtLineLegend.Columns.Add("COLORMAP", typeof(string));
            _dtLineLegend.Columns.Add("VALUE", typeof(string));
            _dtLineLegend.Columns.Add("LEG_DESC", typeof(string));
            _dtLineLegend.Columns.Add("GROUP", typeof(string));
            _dtLineLegend.Columns.Add("SHAPE", typeof(string));

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
                    _dtLineLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCODE"].GetString(), row["CMCDNAME"].GetString(), "LOAD", "RECT");
                }
            }

            tbCoreR.Text = string.Empty;
            tbCoreL.Text = string.Empty;    //Core 위치 표시
        }


        private void StartTimer()
        {
            if (_dispatcherTimer == null) return;

            long secTicks = (int)AutoSearchText.Text.GetInt() * 10000000;  //ticks 10000000 = 1초
            _dispatcherTimer.Interval = TimeSpan.FromTicks(secTicks);
            _dispatcherTimer.Start();
            Console.WriteLine("StartTimer " + DateTime.Now.ToLongTimeString());
        }

        private void StopTimer()
        {
            Console.WriteLine("StopTimer " + DateTime.Now.ToLongTimeString());
            if (_dispatcherTimer == null) return;
            _dispatcherTimer.Stop();
        }

        //30초마다 실행. 사용자 지정 시 지정 시간으로 실행.
        private void doTimeStart(bool bForceRedraw = false)
        {
            StopTimer();

            ValidationSearch(bForceRedraw);
            //GetRollMap(bForceRedraw);

            //StartTimer();           

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


            if (c1Chart.Name == "chartInput" || c1Chart.Name == "chartInputBottom")
            {
                double majorUnit;
                double length = _xInputMaxLength;

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

            BeginUpdateChart();
        }

        private void InitializeDataTable()
        {
            dgLotInfo?.ClearRows();
            tbInputProdQty.Text = "";
            tbInputProdQty.ToolTip = null;
            tbInputGoodQty.Text = "";
            tbInputGoodQty.ToolTip = null;
        }

        private void doHideThisWindow()
        {
            //Close로 닫을경우 해당 화면을 다시 열수 없으므로 (C#특징) Hide로 해당 화면을 닫는다.
            this.Hide();
            //해당화면을 닫은 후 다시 화면을 표현해 준다.
            if (!_isClosed)
            {
                this.ShowActivated = true;
                this.Show();
            }
        }

        //범례표현
        private void GetLegend()
        {
            grdLegend.Children.Clear();

            DataTable dt = _dtLineLegend.Copy();

            //절대좌표 
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

            //GroupBox gBox1 = new GroupBox() { FontWeight = FontWeights.Bold };
            StackPanel sp1 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 3)
            };


            //GroupBox gBox = new GroupBox() { FontWeight = FontWeights.Bold };
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
                        Text = Util.NVC(row["LEG_DESC"]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        //FontWeight = FontWeights.Bold,
                        Margin = new Thickness(5, 0, 5, 3)
                    };
                    sp.Children.Add(rectangleLegend);
                    sp.Children.Add(textBlockDescription);

                }


                Grid.SetColumn(sp, 0);
                Grid.SetRow(sp, 1);
                grdLegend.Children.Add(sp);

                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 5, 3)
                };

                Path samplePath = new Path { Fill = Brushes.Black, Stretch = Stretch.Uniform };

                //해당 내용이 디자인을 그려준다고 함.. 인터넷에서 찾아보면 나온다고 함.. (가위모양)
                string pathData = "M11,21H7V19H11V21M15.5,19H17V21H13V19H13.2L11.8,12.9L9.3,13.5C9.2,14 9,14.4 8.8,14.8C7.9,16.3 6,16.7 4.5,15.8C3,14.9 2.6,13 3.5,11.5C4.4,10 6.3,9.6 7.8,10.5C8.2,10.7 8.5,11.1 8.7,11.4L11.2,10.8L10.6,8.3C10.2,8.2 9.8,8 9.4,7.8C8,6.9 7.5,5 8.4,3.5C9.3,2 11.2,1.6 12.7,2.5C14.2,3.4 14.6,5.3 13.7,6.8C13.5,7.2 13.1,7.5 12.8,7.7L15.5,19M7,11.8C6.3,11.3 5.3,11.6 4.8,12.3C4.3,13 4.6,14 5.3,14.4C6,14.9 7,14.7 7.5,13.9C7.9,13.2 7.7,12.2 7,11.8M12.4,6C12.9,5.3 12.6,4.3 11.9,3.8C11.2,3.3 10.2,3.6 9.7,4.3C9.3,5 9.5,6 10.3,6.5C11,6.9 12,6.7 12.4,6M12.8,11.3C12.6,11.2 12.4,11.2 12.3,11.4C12.2,11.6 12.2,11.8 12.4,11.9C12.6,12 12.8,12 12.9,11.8C13.1,11.6 13,11.4 12.8,11.3M21,8.5L14.5,10L15,12.2L22.5,10.4L23,9.7L21,8.5M23,19H19V21H23V19M5,19H1V21H5V19Z";
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Geometry));
                samplePath.Data = (Geometry)converter.ConvertFrom(pathData);

                TextBlock textBlockSample = new TextBlock
                {
                    Text = "Cut",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12,
                    Margin = new Thickness(5, 0, 5, 3)
                };

                stackPanel.Children.Add(samplePath);
                stackPanel.Children.Add(textBlockSample);

                Grid.SetColumn(stackPanel, 1);
                Grid.SetRow(stackPanel, 1);
                grdLegend.Children.Add(stackPanel);

            }
        }

        private void GetRollMap()
        {
            try
            {
                txtNote.Text = "";
                ShowLoadingIndicator();

                DoEvents();

                InitializeDataTable();
                InitializeControls();

                string adjFlag = (bool)rdoP.IsChecked ? "1" : "2";

                const string bizRuleName = "BR_PRD_SEL_RM_RPT_CHART_SBL";
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRE_EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("ADJFLAG", typeof(string));
                //inTable.Columns.Add("IN_OUT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQPTID"] = _equipmentCode;
                newRow["EQPTID"] = _dtEioAttr.Rows[0]["EQPTID"].ToString();
                newRow["LOTID"] = txtLot.Text;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["ADJFLAG"] = adjFlag;
                //newRow["IN_OUT"] = "1";
                inTable.Rows.Add(newRow);

                SetMessageToWipNote("Rollmap Data Loading...");

                GetTagList();

                Console.WriteLine("GetRollMap - SELECT " + DateTime.Now.ToLongTimeString());

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_LANE,OUT_HEAD,OUT_DEFECT,OUT_GAUGE", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            SetMessageToWipNote(GetExceptionMessage(bizException));
                            HiddenLoadingIndicator();
                            EndUpdateChart();
                            StartTimer();
                            return;
                        }

                        SetMessageToWipNote("");

                        Console.WriteLine("GetRollMap - DRAW START " + DateTime.Now.ToLongTimeString());

                        //SetMenuUseCount();
                        _dt2DBarcodeInfo = Get2DBarcodeInfo(txtLot.Text, _equipmentCode);
                        if (CommonVerify.HasTableInDataSet(bizResult))
                        {
                            DataTable dtInLane = bizResult.Tables["OUT_LANE"];       //투입 차트Lane, 무지부, Top Back표시 테이블
                            DataTable dtInHead = bizResult.Tables["OUT_HEAD"];       //투입 차트 헤더 길이 표시 테이블
                            DataTable dtInDefect = bizResult.Tables["OUT_DEFECT"];   //투입 차트 불량정보 표시 테이블
                            DataTable dtInGauge = bizResult.Tables["OUT_GAUGE"];     //투입 차트 두께 측정 데이터 표시 테이블
                            //DataTable dtLotCut = bizResult.Tables["LOT_CUT"];     //투입 차트 두께 측정 데이터 표시 테이블

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
                                //DisplayTabLocation(dtInLane, ChartConfigurationType.Input);
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

                            if (CommonVerify.HasTableRow(_dtEioAttr))
                            {
                                string wipNoteText = GetWipNote(_dtEioAttr);
                                if (string.IsNullOrEmpty(wipNoteText) == false)
                                {
                                    txtNote.Text = ObjectDic.Instance.GetObjectName("EQPT_NOTE") + "  : " + wipNoteText;
                                }
                            }

                            GetLotCut();

                            Console.WriteLine("GetRollMap - DRAW END " + DateTime.Now.ToLongTimeString());

                            ProcessArrow(_dtEioAttr);
                        }
                    }
                    catch (Exception ex)
                    {
                        SetMessageToWipNote(GetExceptionMessage(ex));
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        EndUpdateChart();
                        StartTimer();
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                EndUpdateChart();
                HiddenLoadingIndicator();
                SetMessageToWipNote(GetExceptionMessage(ex));
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
                             EndPosition = t.GetValue("END_PSTN").GetDecimal(),   // t.Field<decimal>("END_PSTN"),
                             ProdQty = t.GetValue("INPUT_QTY").GetDecimal(),  //t.Field<decimal?>("INPUT_QTY"),
                             GoodQty = t.GetValue("WIPQTY_ED").GetDecimal() //t.Field<decimal?>("WIPQTY_ED")
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
            }
        }

        private void InitializeChart(C1Chart chart)
        {
            double maxLength = _xInputMaxLength;


            chart.View.AxisX.Min = 0;
            chart.View.AxisY.Min = _MinusRate * 4;
            chart.View.AxisX.Max = maxLength;
            chart.View.AxisY.Max = 100;
            InitializeChartView(chart);


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
        }

        private void DrawLotCut(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            if (dt.Rows.Count <= 0) return;

            C1Chart chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            DataTable dtLotCut = new DataTable();
            dtLotCut.Columns.Add(new DataColumn() { ColumnName = "RAW_END_PSTN", DataType = typeof(double) });
            dtLotCut.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });
            dtLotCut.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });


            foreach (DataRow row in dt.Rows)
            {
                string ROLLMAP_ADJ_INFO_TYPE_CODE = row["ROLLMAP_ADJ_INFO_TYPE_CODE"].ToString();

                if (ROLLMAP_ADJ_INFO_TYPE_CODE != "LOT_CUT") continue;

                double END_PSTN1 = double.Parse(row["END_PSTN1"].ToString());

                double CUT_PSTN = GetReverseEndPstn(END_PSTN1, END_PSTN1);

                DataRow newLotCut = dtLotCut.NewRow();
                newLotCut["RAW_END_PSTN"] = $"{CUT_PSTN: ###,###,###,##0.##}";
                newLotCut["SOURCE_Y_PSTN"] = 100;
                newLotCut["TOOLTIP"] = ObjectDic.Instance.GetObjectName("CUT_SPLIT") + " : " + $"{CUT_PSTN:###,###,###,##0.##}";
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
            double axisXfar = Equals(chartConfigurationType, ChartConfigurationType.Input) ? _xInputMaxLength : _xInputMaxLength;     //x축 종료점

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // 무지부 Lane 지정
                if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Plain")
                {
                    AlarmZone alarmZone = new AlarmZone();
                    alarmZone.Near = axisXnear;
                    alarmZone.Far = axisXfar;
                    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"] == null ? 0 : dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_END_RATE"] == null ? 0 : dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
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
                    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"] == null ? 0 : dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_END_RATE"] == null ? 0 : dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                    alarmZone.ConnectionStroke = new SolidColorBrush(Colors.Transparent);
                    alarmZone.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    chart.Data.Children.Add(alarmZone);
                }
            }

            var queryLane = (from t in dt.AsEnumerable() where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" select new { LaneNo = t.Field<string>("LANE_NO_CUR") }).Distinct().ToList();
            int laneCount = queryLane.Count;


            var queryTopBack = (from t in dt.AsEnumerable() select new { TopBackFlag = t.Field<string>("T_B_FLAG") }).Distinct().ToList();
            int topBackCount = queryTopBack.Count;

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


            c1Chart = chartInput;
            startPosition = _xInputMaxLength - (_xInputMaxLength * 0.01);



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

        }


        private double GetReverseStartPstn(object strPstn, object endPstn)
        {
            double str = 0;
            double end = 0;

            if (strPstn != null) str = strPstn.GetDouble();
            if (endPstn != null) end = endPstn.GetDouble();

            if (_bReverseFlag == false) return str;

            if (_xInputMaxLength <= end) return 0;

            return _xInputMaxLength - end;
        }

        private double GetReverseEndPstn(object strPstn, object endPstn)
        {
            double str = 0;
            double end = 0;

            if (strPstn != null) str = strPstn.GetDouble();
            if (endPstn != null) end = endPstn.GetDouble();

            if (_bReverseFlag == false) return end;

            if (_xInputMaxLength <= str) return 0;

            return _xInputMaxLength - str;
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

                double ADJ_STRT_PSTN_R = GetReverseStartPstn(dtGauge.Rows[i]["ADJ_STRT_PSTN"], dtGauge.Rows[i]["ADJ_END_PSTN"]);
                double ADJ_END_PSTN_R = GetReverseEndPstn(dtGauge.Rows[i]["ADJ_STRT_PSTN"], dtGauge.Rows[i]["ADJ_END_PSTN"]);

                alarmZone.Near = ADJ_STRT_PSTN_R;
                alarmZone.Far = ADJ_END_PSTN_R;
                alarmZone.LowerExtent = 0;
                alarmZone.UpperExtent = 0;
                if (Util.NVC(dtGauge.Rows[i]["CHART_Y_END_CUM_RATE"]) != "") alarmZone.LowerExtent = dtGauge.Rows[i]["CHART_Y_END_CUM_RATE"].GetDouble();
                if (Util.NVC(dtGauge.Rows[i]["CHART_Y_STRT_CUM_RATE"]) != "") alarmZone.UpperExtent = dtGauge.Rows[i]["CHART_Y_STRT_CUM_RATE"].GetDouble();

                alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;

                int x = i;
                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    if (pe is Lines)
                    {
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

                            content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "~" + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                         "Scan AVG : " + Util.NVC($"{dtGauge.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                                         scanMinValueContent + scaneMaxValueContent +
                                         "ColorMap : " + Util.NVC(dtGauge.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                         "Offset : " + Util.NVC(dtGauge.Rows[x]["SCAN_OFFSET"]);
                        }
                        else
                        {
                            content = dtGauge.Rows[x]["EQPT_MEASR_PSTN_NAME"] + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "~" + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "] " + Environment.NewLine +
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

            // 부분 Lane 구간 불량
            var queryTagSectionLane = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
            DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtTagSectionLane))
            {
                DrawDefectTagSectionLane(dtTagSectionLane, chartConfigurationType);
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
            //if (chartConfigurationType == ChartConfigurationType.Input) return;

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
                //double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                //double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                //PET, SCRAP 의 경우 END 좌표가 의미가 없어 END 좌표를 반전하면 안됨.

                double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_STRT_PSTN"]);
                double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_STRT_PSTN"]);

                var convertFromString = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD"));
                string colorMap = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? "#000000" : "#BDBDBD";
                string content = "[" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "] " + ObjectDic.Instance.GetObjectName("위치") + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m " + ObjectDic.Instance.GetObjectName("길이") + $"{row["WND_LEN"]:###,###,###,##0.##}" + "m";
                string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + ADJ_STRT_PSTN_R.GetString() + ";" + ADJ_END_PSTN_R.GetString()
                      + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                chartInput.Data.Children.Add(new XYDataSeries()
                {
                    ChartType = ChartType.Line,
                    XValuesSource = new[] { ADJ_STRT_PSTN_R, ADJ_END_PSTN_R },
                    ValuesSource = new double[] { lowerExtent, upperExtent },
                    ConnectionStroke = convertFromString,
                    ConnectionStrokeThickness = 3,
                    ConnectionFill = convertFromString,
                });

                dt.Rows.Add(row["EQPT_MEASR_PSTN_ID"], ADJ_STRT_PSTN_R, ADJ_END_PSTN_R, 97, colorMap, content, tag);
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

                chartInput.Data.Children.Add(dsPetScrap);
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
            //if (chartConfigurationType == ChartConfigurationType.Input) return;

            if (CommonVerify.HasTableRow(dtRewinderScrap))
            {
                dtRewinderScrap.Columns.Add("TOOLTIP", typeof(string));
                dtRewinderScrap.Columns.Add("TAG", typeof(string));
                dtRewinderScrap.Columns.Add("Y_PSTN", typeof(double));
                dtRewinderScrap.Columns.Add("R_ADJ_END_PSTN", typeof(double));

                double lowerExtent = 0;
                double upperExtent = 100;

                foreach (DataRow row in dtRewinderScrap.Rows)
                {
                    double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                    double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);

                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + ADJ_STRT_PSTN_R.GetString() + ";" + ADJ_END_PSTN_R.GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + " ~ " + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + " ]";

                    row["TOOLTIP"] = toolTip;
                    row["TAG"] = tag;
                    row["Y_PSTN"] = 97;

                    chartInput.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { ADJ_END_PSTN_R, ADJ_END_PSTN_R },
                        ValuesSource = new double[] { lowerExtent, upperExtent },
                        ConnectionStroke = convertFromString,
                        ConnectionStrokeThickness = 3,
                        ConnectionFill = convertFromString,
                    });
                }

                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dtRewinderScrap);
                dsPetScrap.XValueBinding = new Binding("R_ADJ_END_PSTN");
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

                chartInput.Data.Children.Add(dsPetScrap);
            }
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

                // 부분 Lane 구간 불량
                var queryTagSectionLane = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
                DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dt.Clone();

                if (CommonVerify.HasTableRow(dtTagSectionLane))
                {
                    dtTagSectionLane.TableName = "TAG_SECTION_LANE";
                    DrawDefectTagSection(dtTagSectionLane, chartConfigurationType);
                }

                // Tag SectionSingle  NG 마킹
                var queryTagSectionSingle = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSectionSingle = queryTagSectionSingle.Any() ? queryTagSectionSingle.CopyToDataTable() : dt.Clone();
                if (CommonVerify.HasTableRow(dtTagSectionSingle))
                {
                    dtTagSectionSingle.TableName = "TAG_SECTION_SINGLE";
                    DrawDefectTagSection(dtTagSectionSingle, chartConfigurationType);
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
        /// TOP, BACK 최종 비전 표면불량 표시
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawDefectVisionSurface(DataTable dt, ChartConfigurationType chartConfigurationType)
        {
            C1Chart c1Chart = Equals(chartConfigurationType, ChartConfigurationType.Input) ? chartInput : chartOutput;

            dt.Columns.Add(new DataColumn() { ColumnName = "ADJ_X_PSTN_R", DataType = typeof(double) });

            foreach (DataRow row in dt.Rows)
            {
                double ADJ_X_PSTN_R = GetReverseStartPstn(row["ADJ_X_PSTN"], row["ADJ_X_PSTN"]);
                row["ADJ_X_PSTN_R"] = ADJ_X_PSTN_R;
            }

            XYDataSeries ds = new XYDataSeries();
            ds.Tag = dt.TableName;
            ds.ItemsSource = DataTableConverter.Convert(dt);
            //ds.ValueBinding = new Binding("Y_PSTN_CUM_RATE");
            ds.ValueBinding = new Binding("Y_PSTN_RATE");
            ds.XValueBinding = new Binding("ADJ_X_PSTN_R");
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
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "ADJ_X_PSTN_R")), out xposition);
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

                double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);

                AlarmZone alarmZone = new AlarmZone
                {
                    Near = ADJ_STRT_PSTN_R,
                    Far = ADJ_END_PSTN_R,
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["CHART_Y_STRT_CUM_RATE"] == null ? 0 : row["CHART_Y_STRT_CUM_RATE"].GetDouble(),
                    UpperExtent = row["CHART_Y_END_CUM_RATE"] == null ? 0 : row["CHART_Y_END_CUM_RATE"].GetDouble(),
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
                        string content = row["ABBR_NAME"] + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + "~" + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + "]";
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
                double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));
                AlarmZone alarmZone = new AlarmZone
                {
                    Near = ADJ_STRT_PSTN_R,
                    Far = ADJ_END_PSTN_R,
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["CHART_Y_STRT_CUM_RATE"] == null ? 0 : row["CHART_Y_STRT_CUM_RATE"].GetDouble(),
                    UpperExtent = row["CHART_Y_END_CUM_RATE"] == null ? 0 : row["CHART_Y_END_CUM_RATE"].GetDouble(),
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
                        string content = row["ABBR_NAME"] + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + "~" + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + "]";
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
                ds.XValueBinding = x == 0 ? new Binding("R_ADJ_STRT_PSTN") : new Binding("R_ADJ_END_PSTN");
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
            // Start, End 이미지 두번의 표현으로 for문 사용
            for (int x = 0; x < 2; x++)
            {
                DataTable dtTag = MakeTableForDisplay(dt, x == 0 ? ChartDisplayType.TagSectionLaneStart : ChartDisplayType.TagSectionLaneEnd, chartConfigurationType);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTag);
                ds.XValueBinding = x == 0 ? new Binding("R_ADJ_STRT_PSTN") : new Binding("R_ADJ_END_PSTN");
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
                ds.XValueBinding = new Binding("R_ADJ_STRT_PSTN");
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

            dt.Columns.Add(new DataColumn() { ColumnName = "R_ADJ_STRT_PSTN", DataType = typeof(double) });
            // 종축 라인 설정
            DataRow[] drMark = dt.Select();
            if (drMark.Length > 0)
            {
                foreach (DataRow row in drMark)
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                    double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_STRT_PSTN"]);
                    row["R_ADJ_STRT_PSTN"] = ADJ_STRT_PSTN_R;
                    string content = row["EQPT_MEASR_PSTN_NAME"] + "[" + Util.NVC(row["ROLLMAP_CLCT_TYPE"]) + "]," + " (POS : " + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + ")";

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { ADJ_STRT_PSTN_R, ADJ_STRT_PSTN_R },
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
            var queryLabel = dt.AsEnumerable().Where(x => x.GetValue("CHART_Y_STRT_CUM_RATE") != null
                                                       && x.GetValue("CHART_Y_END_CUM_RATE") != null
                                                       && Math.Abs(x.GetValue("CHART_Y_STRT_CUM_RATE").GetDecimal() - x.GetValue("CHART_Y_END_CUM_RATE").GetDecimal()) >= 100).ToList();

            DataTable dtLabel = queryLabel.Any() ? MakeTableForDisplay(queryLabel.CopyToDataTable(), ChartDisplayType.MarkLabel, chartConfigurationType)
                                                 : MakeTableForDisplay(dt.Clone(), ChartDisplayType.MarkLabel, chartConfigurationType);

            var dsMarkLabel = new XYDataSeries();
            dsMarkLabel.Name = dt.TableName;
            dsMarkLabel.ItemsSource = DataTableConverter.Convert(dtLabel);
            dsMarkLabel.XValueBinding = new Binding("R_ADJ_STRT_PSTN");
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
                dtRwScrap.Columns.Add("R_ADJ_END_PSTN", typeof(double));

                double lowerExtent = 0;
                double upperExtent = 100;

                foreach (DataRow row in dtRwScrap.Rows)
                {
                    double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                    double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                    row["R_ADJ_END_PSTN"] = ADJ_END_PSTN_R;

                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + ADJ_STRT_PSTN_R.GetString() + ";" + ADJ_END_PSTN_R.GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                    string toolTip = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + " ~ " + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + " ]";

                    row["TOOLTIP"] = toolTip;
                    row["TAG"] = tag;
                    row["Y_PSTN"] = 100;

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { ADJ_END_PSTN_R, ADJ_END_PSTN_R },
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
                dsPetScrap.XValueBinding = new Binding("R_ADJ_END_PSTN");
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

                                    //rectangleDescription.PreviewMouseUp += DescriptionOnPreviewMouseUp;
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

                                    //ellipseDescription.PreviewMouseUp += DescriptionOnPreviewMouseUp;
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
                SetMessageToWipNote(GetExceptionMessage(ex));
            }
        }


        private static DataTable SelectEquipmentPolarity(string equipmentCode)
        {
            const string bizRuleName = "DA_BAS_SEL_VWEQUIPMENT";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = equipmentCode;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private static double GetYValue(double maxValue, double minValue, double targetValue)
        {
            int value = (maxValue > 600) ? 600 : 0;
            return ((targetValue - value) * 100) / 600;
        }

        private void SetScale(double scale)
        {
            chartInput.View.AxisX.Scale = scale;
            btnRefresh.IsCancel = !scale.Equals(1);
            btnZoomIn.IsCancel = scale > 0.002;
            btnZoomOut.IsCancel = scale < 1;


            UpdateScrollbars();
        }

        private void UpdateScrollbars()
        {
            double sxTop = chartInput.View.AxisX.Scale;
            AxisScrollBar axisScrollBarCoater = (AxisScrollBar)chartInput.View.AxisX.ScrollBar;
            axisScrollBarCoater.Visibility = sxTop >= 1.0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BeginUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                c1Chart.Reset(true);
                c1Chart.BeginUpdate();

                //DataSeriesCollection sc = c1Chart.Data.Children;
                //sc.Clear();

                c1Chart.View.AxisX.Reversed = false;

                if (c1Chart.Name == "chartInput" || c1Chart.Name == "chartInputBottom")
                {
                    if (!chartInput.View.AxisX.Scale.Equals(1))
                    {
                        SetScale(1.0);
                    }
                    else
                    {
                        UpdateScrollbars();
                    }
                    c1Chart.View.AxisX.MinScale = 0.05;
                    c1Chart.View.Margin = new Thickness(0, 0, 0, 20);
                }

                if (c1Chart.Name == "chartTopLine" || c1Chart.Name == "chartBackLine")
                {
                    c1Chart.View.Margin = new Thickness(0, 0, 0, 5);
                }
            }

        }

        private void EndUpdateChart()
        {

            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                if (c1Chart.Name != "chart")
                {
                    c1Chart.View.AxisX.Reversed = true;
                }
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
                    double ADJ_STRT_PSTN_R = row["R_ADJ_STRT_PSTN"].GetDouble();
                    row["SOURCE_ADJ_Y_PSTN"] = _MinusRate * 1;
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[" + row["ROLLMAP_CLCT_TYPE"] + "]," + " (POS : " + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + ")";
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSpot)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "R_ADJ_STRT_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_STRT_PSTN"]);
                    row["R_ADJ_STRT_PSTN"] = ADJ_STRT_PSTN_R;
                    row["SOURCE_ADJ_Y_PSTN"] = _MinusRate * 4;
                    row["TAGNAME"] = "T";
                    // Mark Seqno 표기 추가 (GM VOE ESHM 추가 사양) 
                    //  - 슬리터 공정에서 코터 TAG 라벨 인식된 것인지 구분을 위해 사용
                    string sToolTip = row["EQPT_MEASR_PSTN_NAME"] + " : " + row["ABBR_NAME"] + " [" + ADJ_STRT_PSTN_R + "m]";
                    if (Util.IsNVC(row["MARK_SEQNO"]) == false && row["MARK_SEQNO"] != null)
                        sToolTip = "(COT)" + sToolTip;
                    row["TOOLTIP"] = sToolTip;
                    //row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + " : " + row["ABBR_NAME"] + " [" + ADJ_STRT_PSTN_R + "m]";
                    row["TAG"] = null;
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionStart || chartDisplayType == ChartDisplayType.TagSectionEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "R_ADJ_STRT_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "R_ADJ_END_PSTN", DataType = typeof(double) });

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
                         AdjStrtPstn = t.GetValue("ADJ_STRT_PSTN").GetDecimal(),
                         AdjEndPstn = t.GetValue("ADJ_END_PSTN").GetDecimal()
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
                    //nathan 2024.03.08 Lane별 TAG_SECTION 표시
                    if (row["SOURCE_ADJ_LANE_NO"] == null || row["SOURCE_ADJ_LANE_NO"].ToString() == "" || row["CHART_Y_STRT_CUM_RATE"] == null)
                    {
                        //E20240724-000827 TAG_SECTION_SINGLE 추가에 따른 분기                        
                        if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"])) row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? 100 : -62;
                        else row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? _MinusRate * 2 : _MinusRate * 3;
                    }
                    else
                    {
                        double CHART_Y_STRT = row["CHART_Y_STRT_CUM_RATE"].GetDouble() - 5; //5는 마크 크기

                        row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? CHART_Y_STRT : CHART_Y_STRT + _MinusRate;
                        //row["SOURCE_Y_PSTN"] = row["CHART_Y_STRT"];
                    }

                    //row["TAG"] = null;
                    double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                    double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                    row["R_ADJ_STRT_PSTN"] = ADJ_STRT_PSTN_R;
                    row["R_ADJ_END_PSTN"] = ADJ_END_PSTN_R;

                    row["TAG"] = chartConfigurationType == ChartConfigurationType.Input ? null : $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + ";" + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

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
                                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + " ~ " + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
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
                        row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + " ~ " + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                    }
                    //E20240724-000827 TAG_SECTION_SINGLE 추가로 인하여 UI상에서 M 으로 표기
                    else if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"]))
                    {
                        if (chartDisplayType == ChartDisplayType.TagSectionStart)
                        {
                            row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + " ~ " + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                            row["TAGNAME"] = "M";
                        }
                        else
                        {
                            row.Delete();
                        }

                    }
                    else
                    {
                        row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + " ~ " + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                    }

                }
                ////NFF Lane 불량 정보 표기에 따른 변경 - End
            }

            else if (chartDisplayType == ChartDisplayType.TagSectionLaneStart || chartDisplayType == ChartDisplayType.TagSectionLaneEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "R_ADJ_STRT_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "R_ADJ_END_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    //nathan 2024.03.08 Lane별 TAG_SECTION 표시
                    if (row["SOURCE_ADJ_LANE_NO"] == null || row["SOURCE_ADJ_LANE_NO"].ToString() == string.Empty || row["CHART_Y_STRT_CUM_RATE"] == null)
                    {
                        //E20240724-000827 TAG_SECTION_SINGLE 추가에 따른 분기
                        if ("TAG_SECTION".Equals(row["EQPT_MEASR_PSTN_ID"]))
                            row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? _MinusRate * 2 : _MinusRate * 3;
                        else if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"]))
                            row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? 100 : -62;
                    }
                    else
                    {
                        double CHART_Y_STRT = row["CHART_Y_STRT_CUM_RATE"].GetDouble() - 5; //5는 마크 크기
                        row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? CHART_Y_STRT : CHART_Y_STRT + _MinusRate;
                    }

                    double ADJ_STRT_PSTN_R = GetReverseStartPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                    double ADJ_END_PSTN_R = GetReverseEndPstn(row["ADJ_STRT_PSTN"], row["ADJ_END_PSTN"]);
                    row["R_ADJ_STRT_PSTN"] = ADJ_STRT_PSTN_R;
                    row["R_ADJ_END_PSTN"] = ADJ_END_PSTN_R;
                    row["TAG"] = chartConfigurationType == ChartConfigurationType.Input ? null : $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + ";" + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                    string laneInfo = row["LANE_NO_LIST"].ToString();
                    row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{ADJ_STRT_PSTN_R:###,###,###,##0.##}" + "m" + " ~ " + $"{ADJ_END_PSTN_R:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                }
            }

            dtBinding.AcceptChanges();
            return dtBinding;
        }


        private void ValidationSearch(bool bForceRedraw = false)
        {
            const string bizRuleName = "BR_PRD_GET_CT_LOT_MNT_INFO";

            _dtEioAttr?.Clear();

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = _equipmentCode;
            dr["ADJFLAG"] = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (dtEioAttr, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        SetMessageToWipNote(GetExceptionMessage(bizException));
                        //HiddenLoadingIndicator();
                        return;
                    }

                    _dtEioAttr = dtEioAttr.Copy();

                    if (dtEioAttr.Rows.Count <= 0 || dtEioAttr.Rows[0]["PRE_LOTID"].ToString().IsEmpty())
                    {
                        txtLot.Text = "";
                        SetMessageToWipNote(MessageDic.Instance.GetMessage("90026"));
                        //chartInput.Reset(true);
                        InitializeDataTable();
                        InitializeControls();
                        EndUpdateChart();
                        StartTimer();
                        _isMinMaxEqpt = false;
                        return;
                    }

                    if (txtLot.Text.Equals(dtEioAttr.Rows[0]["PRE_LOTID"].ToString())
                    && _wipSeq.Equals(dtEioAttr.Rows[0]["WIPSEQ"].ToString())
                    && bForceRedraw == false)
                    {
                        ProcessArrow(dtEioAttr);
                        EndUpdateChart();
                        StartTimer();
                        return;
                    }

                    txtLot.Text = dtEioAttr.Rows[0]["PRE_LOTID"].ToString();
                    _wipSeq = dtEioAttr.Rows[0]["WIPSEQ"].ToString();
                    if (dtEioAttr.Rows[0]["LAR_REVS_FLAG"].ToString() == "Y")
                    {
                        _bReverseFlag = true;   //요거 왠만하면 항상 true가 되어야함.
                    }

                    if (Util.NVC(_dtEioAttr.Rows[0]["EQPTID"]).IsNullOrEmpty() == false)
                        SetEqptDetailInfoByCommoncode(Util.NVC(_dtEioAttr.Rows[0]["EQPTID"]));

                    GetRollMap();

                }
                catch (Exception ex)
                {
                    //HiddenLoadingIndicator();
                    SetMessageToWipNote(GetExceptionMessage(ex));
                    StartTimer();
                }
            });

        }

        private void ValidationRollMapLot()
        {
            try
            {
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow newRow = inDataTable.NewRow();
                newRow["LOTID"] = txtLot.Text;
                newRow["WIPSEQ"] = _wipSeq;
                inDataTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_ROLLMAP_CHART_DISPLAY", "INDATA", null, inDataTable);
            }
            catch (Exception ex)
            {
                SetMessageToWipNote(GetExceptionMessage(ex));
                System.Threading.Thread.Sleep(3000);
                btnClose_Click(btnClose, null);
            }
        }


        private enum ChartDisplayType
        {
            MarkLabel,
            TagSectionStart,
            TagSectionEnd,
            TagSectionLaneStart,
            TagSectionLaneEnd,
            TagToolTip,
            TagVisionTop,
            TagVisionBack,
            Material,
            Sample,
            TagSpot,
            SurfaceTop,
            SurfaceBack,
            OverLayVision,
            RewinderScrap
        }

        //private enum ChartDisplayType
        //{
        //    MarkLabel,
        //    TagSectionStart,
        //    TagSectionEnd,
        //    //TagToolTip,
        //    //TagVisionTop,
        //    //TagVisionBack,
        //    //Material,
        //    //Sample,
        //    TagSpot,
        //    //SurfaceTop,
        //    //SurfaceBack,
        //    //OverLayVision,
        //    RewinderScrap
        //}

        private enum ChartConfigurationType
        {
            Input,
            Output
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

        /**
         * TAG 정보 Get
         */
        private void GetTagList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("INPUT_LOTID", typeof(string));
                IndataTable.Columns.Add("INPUT_CSTID", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("VIEW_TYPE", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _equipmentCode;
                Indata["INPUT_LOTID"] = txtLot.Text;
                Indata["INPUT_CSTID"] = null;
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["VIEW_TYPE"] = "A";          // (A:전체 Lane 불량 + 부분 Lane 불량 모두 조회, F: 전체 Lane 불량 조회, L : 부분 Lane 불량 조회)


                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RM_RPT_INPUT_LOT_TAG_SECTION", "INDATA", "OUTDATA", IndataTable);


                // 20240527 한유연 추가
                //----------------------------------------------------------------------------------

                Util.GridSetData(dgLotInfo, dtMain, FrameOperation, true);
                Util.gridClear(dgLotInfo);

                if (dtMain.Rows.Count > 0)
                {

                    // 데이터 Merge
                    string[] sColumnName = new string[] { "LANE_NO", "TAG_NG_NAME" };
                    _Util.SetDataGridMergeExtensionCol(dgLotInfo, sColumnName, DataGridMergeMode.VERTICAL);

                    dgLotInfo.GroupBy(dgLotInfo.Columns["TAG_NG_NAME"], DataGridSortDirection.None);
                    dgLotInfo.GroupRowPosition = DataGridGroupRowPosition.AboveData;


                    //첫번쨰 줄 행추가 / SP에서 ALL 데이터를 만들어서 불러 오면 아래 항목은 추가 안해도됨
                    DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["LANE_NO"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    //DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["LANE_NO"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("ALL") } });
                    DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["TAG_NG_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["TAG_START_POSITION"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["TAG_END_POSITION"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["TAG_LENGTH"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    // Grid 데이터 바인딩
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
                }

                //----------------------------------------------------------------------------------

            }
            catch (Exception ex)
            {
                SetMessageToWipNote(GetExceptionMessage(ex));
            }
        }


        //자동 조회 시 마다 진행 상태를 화살표로 표시
        private void ProcessArrow(DataTable dt)
        {
            var chart = chartInput;

            for (int i = chart.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = chart.Data.Children[i];
                if (dataSeries.Tag.GetString().Equals("dsLine") || dataSeries.Tag.GetString().Equals("dsText"))
                {
                    chart.Data.Children.Remove(dataSeries);
                }
            }

            //데이터 미 존재시 하기 내용 미 수행
            if (dt.Rows.Count <= 0) return;

            if (_bReverseFlag == true)
            {
                tbCoreR.Text = "";
                tbCoreL.Text = "CORE";
            }
            else
            {
                tbCoreR.Text = "CORE";
                tbCoreL.Text = "";
            }

            dt.Columns.Add("Y_POSITION1", typeof(double));  //화살표 Y축 현 chart에 50point 위에서 표현
            dt.Columns.Add("Y_POSITION2", typeof(double));  //화살표 Y축 현 chart에 50point 위에서 표현
            dt.Columns.Add("START_DESC", typeof(string));    //툴팁 진행정보 표현
            dt.Columns.Add("COLORMAP", typeof(string));     //색상 정보 표현
            dt.Columns.Add("START_LINE", typeof(double));     //진행 정보 라인 표현
            dt.Columns.Add("Y_LINE", typeof(double));     //라인 시작부분 0 으로 고정

            if ("".Equals(dt.Rows[0]["CURR_PSTN"].ToString()))
            {
                dt.Rows[0]["Y_POSITION1"] = 100;
                dt.Rows[0]["Y_POSITION2"] = 0;
                dt.Rows[0]["START_DESC"] = MessageDic.Instance.GetMessage("90026"); // //90026, 설비에 진행중인 LOT 이(가) 존재하지 않습니다.
                dt.Rows[0]["CURR_PSTN"] = _xInputMaxLength - 100;
            }
            else
            {
                dt.Rows[0]["Y_POSITION1"] = 0;
                dt.Rows[0]["Y_POSITION2"] = 98.5;   //숫자 표시 높이
                dt.Rows[0]["START_DESC"] = $"{dt.Rows[0]["CURR_PSTN"].GetDouble():###,###,###,##0.##}" + "m";
                dt.Rows[0]["COLORMAP"] = "#EB0000";
                dt.Rows[0]["START_LINE"] = dt.Rows[0]["CURR_PSTN"].GetDouble();
                dt.Rows[0]["Y_LINE"] = -5;
            }

            //진행상태 표현
            XYDataSeries dsText = new XYDataSeries();
            dsText.ItemsSource = DataTableConverter.Convert(dt);
            dsText.Tag = "dsText";
            dsText.XValueBinding = new Binding("CURR_PSTN");
            dsText.ValueBinding = new Binding("Y_POSITION2");
            dsText.ChartType = ChartType.XYPlot;
            dsText.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsText.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsText.Cursor = Cursors.Hand;
            dsText.ToolTip = dt.Rows[0]["START_DESC"];
            dsText.PointLabelTemplate = grdMain.Resources["proc_desc"] as DataTemplate;
            dsText.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };
            chart.Data.Children.Add(dsText);

            //var dsLine = new XYDataSeries();
            //dsLine.ItemsSource = DataTableConverter.Convert(dt);
            //dsLine.Tag = "dsLine";
            //dsLine.XValueBinding = new Binding("START_LINE");
            //dsLine.ValueBinding = new Binding("Y_LINE");
            //dsLine.ChartType = ChartType.Column;
            //dsLine.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            //dsLine.SymbolFill = new SolidColorBrush(Colors.Transparent);
            //dsLine.PointLabelTemplate = grdMain.Resources["chartLine"] as DataTemplate;
            //dsLine.PlotElementLoaded += (s, e) =>
            //{
            //    PlotElement pe = (PlotElement)s;
            //    pe.Stroke = new SolidColorBrush(Colors.Transparent);
            //    pe.Fill = new SolidColorBrush(Colors.Transparent);
            //    // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
            //};
            //chart.Data.Children.Add(dsLine); //원재료 마지막 부분에서 그려지기
            chart.Data.Children.Add(new XYDataSeries()
            {
                ChartType = ChartType.Line,
                XValuesSource = new[] { dt.Rows[0]["CURR_PSTN"].GetDouble(), dt.Rows[0]["CURR_PSTN"].GetDouble() },
                ValuesSource = new double[] { -20, 100 },
                ConnectionStroke = new SolidColorBrush(Colors.Red),
                ConnectionStrokeThickness = 10,
                //ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                //ToolTip = content,
                //Name = dt.TableName
                Tag = "dsLine"
            });

        }

        private string GetWipNote(DataTable dt)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                if (string.IsNullOrEmpty(dt.Rows[0]["WIP_NOTE"].GetString()))
                {
                    return string.Empty;
                }
                else
                {
                    string[] wipNote = dt.Rows[0]["WIP_NOTE"].GetString().Split('|');
                    if (wipNote.Length == 0)
                    {
                        return dt.Rows[0]["WIP_NOTE"].GetString();
                    }
                    else
                    {
                        return wipNote[0];
                    }
                }
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// R/P 공정에서 LOT이 PROC 상태인 경우에는 WIPHISTORY 상의 장비 ID를 사용
        /// </summary>
        /// <param name="lotId"></param>
        /// <param name="orgEquipmentId"></param>  
        /// <returns></returns>
        /// 2023.06.06 JEONG KI TONG - For Rollmap Project
        private string GetEquipmentCode(string lotId, string orgEquipmentId)
        {
            string curWipStat = string.Empty;
            string curWipSeq = string.Empty;
            string curProcId = string.Empty;
            string equipmentId = string.Empty;

            equipmentId = orgEquipmentId;

            //  Lot의 현재 공정, WipStat 를 확인하기 위해 Lot 현재 기본 정보 조회
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;

            inTable.Rows.Add(dr);

            DataTable dtLotInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIP_BY_LOTID", "RQSTDT", "RSLTDT", inTable);
            if (CommonVerify.HasTableRow(dtLotInfo))
            {
                curProcId = dtLotInfo.Rows[0]["PROCID"].GetString();
                curWipSeq = dtLotInfo.Rows[0]["WIPSEQ"].GetString();
                curWipStat = dtLotInfo.Rows[0]["WIPSTAT"].GetString();
            }

            if (string.Equals(curProcId, Process.ROLL_PRESSING))
            {
                if (string.Equals(curWipStat, Wip_State.PROC))
                {
                    // WipHistory 상의 장비ID 조회
                    inTable.Clear();
                    inTable = new DataTable { TableName = "RQSTDT" };
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("WIPSEQ", typeof(decimal));

                    dr = inTable.NewRow();
                    dr["LOTID"] = lotId;
                    dr["WIPSEQ"] = "1";
                    inTable.Rows.Add(dr);

                    DataTable dtEquipment = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_WIPHISTORY", "RQSTDT", "RSLTDT", inTable);

                    equipmentId = CommonVerify.HasTableRow(dtEquipment) ? dtEquipment.Rows[0]["EQPTID"].GetString() : orgEquipmentId;
                }
            }

            return equipmentId;
        }


        /// <summary>
        /// Chart 좌우 반전 여부 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewDirectionChecked(object sender, RoutedEventArgs e)
        {
            if (_bAfterLoading == false) return;
            chartInput.BeginUpdate();
            chartInput.View.AxisX.Reversed = !chartInput.View.AxisX.Reversed;
            chartInput.EndUpdate();
        }

        private static DataTable GetLotAttribute(string lotId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LOTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotId;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTATTR", "RQSTDT", "RSLTDT", dt);
        }

        private static DataTable Get2DBarcodeInfo(string lotId, string EqptId)
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotId;
            dr["EQPTID"] = EqptId;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_DFCT_2D_BCR_STD_RM", "RQSTDT", "RSLTDT", dt);
        }

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

        private void SetEqptDetailInfoByCommoncode(string eqptid)
        {
            //공통코드 ROLLMAP_EQPT_DETAIL_INFO
            // - 속성1 : Y일 경우 코터 차트의 1Lane을 위쪽으로 표시
            // - 속성2 : Y일 경우 MIN/MAX/REP 값 툴팁 표시 사용설비
            try
            {
                DataTable dtResult = GetCommonCode("ROLLMAP_EQPT_DETAIL_INFO", eqptid);

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

        private string GetExceptionMessage(Exception ex)
        {
            try
            {
                if (ex == null) return "";

                if (ex.Data.Contains("TYPE"))
                {
                    string conversionLanguage;
                    string exceptionMessage = ex.Message;
                    string exceptionParameter = "";
                    if (ex.Data.Contains("PARA"))
                    {
                        exceptionParameter = ex.Data["PARA"].ToString();
                    }

                    // Code 로 다국어 처리..
                    if (ex.Data.Contains("DATA"))
                    {
                        if (exceptionParameter.Equals(""))
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]));
                        }
                        else
                        {
                            if (exceptionParameter.Contains(":"))
                            {
                                string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.None);

                                conversionLanguage = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]), parameterList);
                            }
                            else
                            {
                                conversionLanguage = MessageDic.Instance.GetMessage(Util.NVC(ex.Data["DATA"]), exceptionParameter);
                            }
                        }
                    }
                    else
                    {
                        if (exceptionParameter.Contains(":"))
                        {
                            string sOrg = exceptionMessage;
                            string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.None);

                            for (int i = parameterList.Length; i > 0; i--)
                            {
                                sOrg = sOrg.Replace(parameterList[i - 1], "%" + (i));
                            }

                            conversionLanguage = MessageDic.Instance.GetMessage(sOrg, parameterList);
                        }
                        else
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(exceptionMessage);
                        }
                    }


                    if (ex.Data["TYPE"].ToString().Equals("LOGIC"))
                    {
                        string detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
                        return detailMessage;
                    }
                    else if (ex.Data["TYPE"].ToString().Equals("SYSTEM"))
                    {
                        string detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
                        return detailMessage;
                    }

                    return conversionLanguage;

                }
                else
                {
                    return ex.Message;
                }
            }
            catch (Exception exception)
            {
                return exception.Message;
            }

        }

        private void SetMessageToWipNote(string strMsg)
        {
            double txtNoteFontSize = txtNote.FontSize;
            Brush BorderBrush = txtNote.BorderBrush;
            txtNote.FontSize = 24;
            txtNote.Foreground = Brushes.Red;
            txtNote.Text = strMsg;

        }

        private void GetLotCut()
        {
            try
            {
                DataTable dt = new DataTable("RQSTDT");
                dt.Columns.Add("INPUT_LOTID", typeof(string));
                dt.Columns.Add("INPUT_LOTID_WIPSEQ", typeof(decimal));
                dt.Columns.Add("DEL_FLAG", typeof(string));

                DataRow dr = dt.NewRow();
                dr["INPUT_LOTID"] = txtLot.Text;
                dr["INPUT_LOTID_WIPSEQ"] = _wipSeq;
                dr["DEL_FLAG"] = "N";
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_ADJ_INFO_ALL", "RQSTDT", "RSLTDT", dt);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    DrawLotCut(dtResult, ChartConfigurationType.Input);
                }
            }
            catch (Exception ex)
            {
                SetMessageToWipNote(GetExceptionMessage(ex));
            }
        }


        #endregion

        #region Window "X"버튼비활성화
        private const int SC_CLOSE = 0xF060;
        private const int MF_ENABLED = 0x0;
        private const int MF_GRAYED = 0x1;
        private const int MF_DISABLED = 0x2;
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);



        [DllImport("user32.dll")]
        private static extern int EnableMenuItem(IntPtr hMenu, int wIDEnableItem, int wEnable);
        #endregion

    }
}