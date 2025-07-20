/*************************************************************************************
 Created Date : 2021.03.05
      Creator : 오광택
   Decription : ROLLMAP HOLD 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.05          : Initial Created.
  2021.07.02 신광희   : 최초 생성 파일 전체 수정
**************************************************************************************/

using System;
using System.Linq;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.C1Chart;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Input;
using Action = System.Action;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_ROLLMAP_HOLD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }

        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _lotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _equipmentName = string.Empty;
        private string _version = string.Empty;

        private string _holdCheck = string.Empty;
        private double _xMaxLength;

        private DataTable _dtLineLegend;
        private DataTable _dtPoint;
        private DataTable _dtGraph;
        private DataTable _dtDefect;
        private DataTable _dtLaneInfo;
        private static DataTable _dt2DBarcodeInfo;
        private bool _isResearch = false;

        public string HoldCheck
        {
            get { return _holdCheck; }
        }

        public CMM_ROLLMAP_HOLD()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        #endregion

        #region Initialize

        private void Initialize()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                _processCode = Util.NVC(parameters[0]);
                _equipmentCode = Util.NVC(parameters[1]);
                _lotId = Util.NVC(parameters[2]);
                _wipSeq = Util.NVC(parameters[3]);
                _equipmentName = Util.NVC(parameters[4]);
                _version = Util.NVC(parameters[5]);
            }

            txtLot.Text = _lotId;
            txtEquipmentName.Text = _equipmentName;

            InitializeControl();

            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
        }

        private void InitializeControl()
        {
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

            _dtDefect = new DataTable();
            _dtDefect.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefect.Columns.Add("ABBR_NAME", typeof(string));

            chartRollMap.View.AxisX.ScrollBar = new AxisScrollBar();

            //rdoTHICK.Click += rdoTHICK_Click;
            //rdoWB.Click += rdoWB_Click;

            gbxMeasrPoint.Visibility = string.Equals(_processCode, Process.ROLL_PRESSING) ? Visibility.Collapsed : Visibility.Visible;
            GetLegend();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {      
            Initialize();
            GetRollMap();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearch()) return;
                ShowLoadingIndicator();
                DoEvents();

                _isResearch = false;

                _dtPoint?.Clear();
                _dtGraph?.Clear();
                _dtDefect?.Clear();

                InitializeControls();
                // Max Length 조회
                DataTable dtLength = SelectRollMapLength();

                if (CommonVerify.HasTableRow(dtLength))
                {
                    var query = (from t in dtLength.AsEnumerable() where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                        select new { sourceEndPosition = t.Field<decimal>("SOURCE_END_PSTN") }).FirstOrDefault();

                    double maxLength;
                    if (query != null) maxLength = query.sourceEndPosition.GetDouble();
                    else maxLength = 0;

                    _xMaxLength = maxLength;
                }

                // 측정기 맵, 그래프 조회 lotId, wipSeq, mPoint, position, adjFlag
                DataTable dtGraph = SelectRollMapGraphInfo();

                if (CommonVerify.HasTableRow(dtGraph))
                {
                    _dtGraph = dtGraph.Copy();
                    DrawBackGroundPointChart(dtGraph, _xMaxLength);
                    //DrawRollMapLane(dtGraph);
                    DrawRollMapLane();
                }
                else
                {
                    InitializeChart();
                }

                _dt2DBarcodeInfo = Get2DBarcodeInfo(txtLot.Text, _equipmentCode);
                DataTable dtPoint = SelectRollMapPointInfo();

                if (CommonVerify.HasTableRow(dtPoint))
                {
                    _dtPoint = dtPoint.Copy();
                    DrawPointChart(dtPoint);
                }

                // RollPress 공정은 샘플 라인이 없음.
                if (string.Equals(_processCode, Process.COATING))
                {
                    DrawRollMapSampleYAxisLine();
                }
                else
                {
                    DrawRollMapStartEndYAxis();
                }


                if (chartRollMap.Data.Children.Count < 1)
                {
                    InitializeChart();
                }

                EndUpdateChart();
                HiddenLoadingIndicator();

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void rdoTHICK_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void rdoWB_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Header.GetString().Contains("[#] "))
            {
                e.Column.HeaderPresenter.Content = e.Column.Header.GetString().Replace("[#] ", "");
                e.Column.HeaderPresenter.VerticalContentAlignment = VerticalAlignment.Center;
            }
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            _holdCheck = "Y";  // Hold
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            _holdCheck = "N";  // None
            this.DialogResult = MessageBoxResult.OK;
        }

        private void OriginalSize_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (chartRollMap.View.AxisX.MinScale >= chartRollMap.View.AxisX.Scale * 0.75) return;
            SetScale(chartRollMap.View.AxisX.Scale * 0.75);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartRollMap.View.AxisX.Scale * 1.50);
        }

        private void btnReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartRollMap.BeginUpdate();
            chartRollMap.View.AxisX.Reversed = !chartRollMap.View.AxisX.Reversed;
            chartRollMap.EndUpdate();
        }

        private void btnReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartRollMap.BeginUpdate();
            chartRollMap.View.AxisY.Reversed = !chartRollMap.View.AxisY.Reversed;
            chartRollMap.EndUpdate();
        }

        private void DescriptionOnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null) return;

                ShowLoadingIndicator();
                DoEvents();

                _isResearch = true;
                chartRollMap.Data.Children.Clear();

                if (textBlock.Foreground == Brushes.Black)
                {
                    _dtDefect.Rows.Add(textBlock.Name, textBlock.Text);
                    textBlock.Foreground = Brushes.LightGray;
                }

                else //if(textBlock.Foreground == Brushes.Gray)
                {
                    _dtDefect.Select("EQPT_MEASR_PSTN_ID = '" + textBlock.Name + "' And "
                                     + "ABBR_NAME = '" + textBlock.Text + "'").ToList().ForEach(row => row.Delete());
                    _dtDefect.AcceptChanges();

                    textBlock.Foreground = Brushes.Black;
                }

                DrawBackGroundPointChart(_dtGraph.Copy(), _xMaxLength);
                if (_dtDefect.Rows.Count < 1)
                {
                    DrawPointChart(_dtPoint.Copy());
                }
                else
                {
                    var queryPoint = _dtPoint.AsEnumerable().Where(ra => !_dtDefect.AsEnumerable()
                        .Any(rb => rb.Field<string>("EQPT_MEASR_PSTN_ID") == ra.Field<string>("EQPT_MEASR_PSTN_ID") && rb.Field<string>("ABBR_NAME") == ra.Field<string>("ABBR_NAME")));

                    if (queryPoint.Any())
                    {
                        DrawPointChart(queryPoint.CopyToDataTable());
                    }
                }

                DrawRollMapSampleYAxisLine();
                chartRollMap.View.AxisX.Reversed = true;

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void AxisScrollBarTopOnAxisRangeChanged(object sender, AxisRangeChangedEventArgs e)
        {

        }
        #endregion

        #region Mehod

        private void GetLegend()
        {
            grdLegend.Children.Clear();

            DataTable dt = _dtLineLegend.Copy();

            for (int x = 0; x < 2; x++)
            {
                ColumnDefinition gridCol1 = new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)};
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
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

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
                        //FontWeight = FontWeights.Bold,
                        Margin = new Thickness(5, 0, 5, 3)
                    };
                    sp.Children.Add(rectangleLegend);
                    sp.Children.Add(textBlockDescription);

                }
                Grid.SetColumn(sp, 0);
                Grid.SetRow(sp, 1);
                grdLegend.Children.Add(sp);

                if (string.Equals(_processCode, Process.COATING))
                {
                    // Sample Cut
                    StackPanel stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5, 0, 5, 3)
                    };

                    Path samplePath = new Path();
                    samplePath.Fill = Brushes.Black;
                    samplePath.Stretch = Stretch.Uniform;

                    string pathData = "M11,21H7V19H11V21M15.5,19H17V21H13V19H13.2L11.8,12.9L9.3,13.5C9.2,14 9,14.4 8.8,14.8C7.9,16.3 6,16.7 4.5,15.8C3,14.9 2.6,13 3.5,11.5C4.4,10 6.3,9.6 7.8,10.5C8.2,10.7 8.5,11.1 8.7,11.4L11.2,10.8L10.6,8.3C10.2,8.2 9.8,8 9.4,7.8C8,6.9 7.5,5 8.4,3.5C9.3,2 11.2,1.6 12.7,2.5C14.2,3.4 14.6,5.3 13.7,6.8C13.5,7.2 13.1,7.5 12.8,7.7L15.5,19M7,11.8C6.3,11.3 5.3,11.6 4.8,12.3C4.3,13 4.6,14 5.3,14.4C6,14.9 7,14.7 7.5,13.9C7.9,13.2 7.7,12.2 7,11.8M12.4,6C12.9,5.3 12.6,4.3 11.9,3.8C11.2,3.3 10.2,3.6 9.7,4.3C9.3,5 9.5,6 10.3,6.5C11,6.9 12,6.7 12.4,6M12.8,11.3C12.6,11.2 12.4,11.2 12.3,11.4C12.2,11.6 12.2,11.8 12.4,11.9C12.6,12 12.8,12 12.9,11.8C13.1,11.6 13,11.4 12.8,11.3M21,8.5L14.5,10L15,12.2L22.5,10.4L23,9.7L21,8.5M23,19H19V21H23V19M5,19H1V21H5V19Z";
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Geometry));
                    samplePath.Data = (Geometry)converter.ConvertFrom(pathData);

                    TextBlock textBlockSample = new TextBlock()
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
                else // 롤프레스
                {

                    // 이음매,  SCRAP Polygon 범례 추가
                    StackPanel stackPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5, 0, 5, 3)
                    };

                    string[] legendArray = new string[] { ObjectDic.Instance.GetObjectName("이음매"), ObjectDic.Instance.GetObjectName("SCRAP") };
                    for (int i = 0; i < legendArray.Length; i++)
                    {
                        PointCollection pointCollection = new PointCollection();
                        pointCollection.Add(new Point(10, 10));
                        pointCollection.Add(new Point(20, 10));
                        pointCollection.Add(new Point(15, 30));

                        Polygon polygon = new Polygon()
                        {
                            Points = pointCollection,
                            Width = 13,
                            Height = 15,
                            Stretch = Stretch.Fill,
                            StrokeThickness = 1,
                            Fill = string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("이음매")) ? Brushes.White : Brushes.Black,
                            Stroke = Brushes.Black
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

        private void SaveDefectForRollMap()
        {
            try
            {
                string bizRuleName = string.Equals(_processCode, Process.COATING) ? "BR_PRD_REG_DATACOLLECT_DEFECT_CT" : "BR_PRD_REG_DATACOLLECT_DEFECT_RP";

                DataSet inDataSet = new DataSet();

                DataTable dtInEquipment = inDataSet.Tables.Add("IN_EQP");
                dtInEquipment.Columns.Add("SRCTYPE", typeof(string));
                dtInEquipment.Columns.Add("IFMODE", typeof(string));
                dtInEquipment.Columns.Add("EQPTID", typeof(string));
                dtInEquipment.Columns.Add("USERID", typeof(string));
                DataRow dr = dtInEquipment.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = _equipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dtInEquipment.Rows.Add(dr);

                DataTable dtInLot = inDataSet.Tables.Add("IN_LOT");
                dtInLot.Columns.Add("LOTID", typeof(string));
                dtInLot.Columns.Add("WIPSEQ", typeof(Int32));
                DataRow newRow = dtInLot.NewRow();
                newRow["LOTID"] = _lotId;
                newRow["WIPSEQ"] = _wipSeq;
                dtInLot.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_LOT", null, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable SelectRollMapLength()
        {
            string bizRuleName = string.Equals(_processCode, Process.COATING) ? "BR_PRD_SEL_ROLLMAP_LENGTH_CT" : "DA_PRD_SEL_ROLLMAP_LENGTH_RP";

            DataTable inTable = new DataTable {TableName = "RQSTDT"};
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            if (string.Equals(_processCode, Process.COATING))
                inTable.Columns.Add("SMPL_FLAG", typeof(string));
            else if(string.Equals(_processCode, Process.ROLL_PRESSING))
                inTable.Columns.Add("SET_WIDTH", typeof(decimal));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dr["ADJFLAG"] = "1";

            if (string.Equals(_processCode, Process.COATING))
                dr["SMPL_FLAG"] = "Y";
            else if (string.Equals(_processCode, Process.ROLL_PRESSING))
                dr["SET_WIDTH"] = 800;

            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private DataTable SelectRollMapGplmWidth()
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_GPLM_WIDTH";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROCID"] = _processCode;
            dr["EQPTID"] = _equipmentCode;
            dr["LOTID"] = txtLot.Text;
            inTable.Rows.Add(dr);

            DataTable dtLaneInfo = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            dtLaneInfo.Columns.Add("LANE_LENGTH", typeof(double));
            dtLaneInfo.Columns.Add("Y_STRT_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("Y_END_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("Y_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("LANE_LENGTH_SUM", typeof(double));

            if (CommonVerify.HasTableRow(dtLaneInfo))
            {
                //Y 좌표 계산
                dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderByDescending(o => o.Field<string>("LANE_NO_CUR").GetDecimal()).ThenByDescending(x => x.Field<int>("RN")).CopyToDataTable();

                var query = (from t in dtLaneInfo.AsEnumerable() where t.Field<string>("LANE_NO") != null select new { LaneNo = t.Field<string>("LANE_NO") }).Distinct().ToList();
                decimal sumLength = dtLaneInfo.AsEnumerable().Sum(s => s.Field<decimal>("LOV_VALUE"));
                double yLength = 0;
                int laneSeq = 0;

                double yStartLength = 0;

                // Lane 이 하나인 경우
                if (query.Count < 2)
                {
                    for (int i = 0; i < dtLaneInfo.Rows.Count; i++)
                    {
                        double x = (dtLaneInfo.Rows[i]["LOV_VALUE"].GetDecimal() * 100 / sumLength).GetDouble();
                        dtLaneInfo.Rows[i]["LANE_LENGTH"] = x;

                        if (!string.IsNullOrEmpty(dtLaneInfo.Rows[i]["LANE_NO"].GetString()))
                        {
                            dtLaneInfo.Rows[i]["Y_STRT_PSTN"] = 0;
                            dtLaneInfo.Rows[i]["Y_END_PSTN"] = 100;
                            dtLaneInfo.Rows[i]["Y_PSTN"] = x / 2;

                            laneSeq++;
                        }
                        yLength = yLength + x;
                        dtLaneInfo.Rows[i]["LANE_LENGTH_SUM"] = yLength;
                    }
                }
                else
                {
                    for (int i = 0; i < dtLaneInfo.Rows.Count; i++)
                    {
                        double x = (dtLaneInfo.Rows[i]["LOV_VALUE"].GetDecimal() * 100 / sumLength).GetDouble();
                        dtLaneInfo.Rows[i]["LANE_LENGTH"] = x;

                        if (!string.IsNullOrEmpty(dtLaneInfo.Rows[i]["LANE_NO"].GetString()))
                        {
                            if (laneSeq == 0)
                            {
                                dtLaneInfo.Rows[i]["Y_STRT_PSTN"] = 0;
                                dtLaneInfo.Rows[i]["Y_END_PSTN"] = dtLaneInfo.Rows[i]["LANE_LENGTH"];
                                dtLaneInfo.Rows[i]["Y_PSTN"] = dtLaneInfo.Rows[i]["LANE_LENGTH"].GetDouble() / 3;
                            }
                            else
                            {
                                dtLaneInfo.Rows[i]["Y_STRT_PSTN"] = yStartLength;
                                dtLaneInfo.Rows[i]["Y_END_PSTN"] = laneSeq == (query.Count - 1) ? 100 : dtLaneInfo.Rows[i]["LANE_LENGTH"].GetDouble() + yStartLength;
                                dtLaneInfo.Rows[i]["Y_PSTN"] = dtLaneInfo.Rows[i]["Y_STRT_PSTN"].GetDouble() + dtLaneInfo.Rows[i]["LANE_LENGTH"].GetDouble() / 3;
                            }

                            laneSeq++;
                        }

                        if (i == 0)
                        {
                            if (!string.IsNullOrEmpty(dtLaneInfo.Rows[i]["LANE_NO"].GetString()))
                            {
                                yStartLength = yStartLength + x;
                            }
                        }
                        else
                        {
                            yStartLength = yStartLength + x;
                        }

                        yLength = yLength + x;
                        dtLaneInfo.Rows[i]["LANE_LENGTH_SUM"] = yLength;
                    }
                }
            }

            dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderBy(o => o.Field<int>("RN")).CopyToDataTable();
            _dtLaneInfo = dtLaneInfo;

            return dtLaneInfo;
        }

        private DataTable SelectRollMapGraphInfo()
        {
            string bizRuleName;
            string mPoint;          //두께 1, 웹게이지 2
            string adjFlag = "1";   //1: 상대좌표, 2: 절대좌표
            string position = "2";  //측정위치(OVEN 전 1,후 2)	
            string sampleFlag;      //Sample Cut 포함여부

            if (string.Equals(_processCode, Process.COATING))
            {
                bizRuleName = "BR_PRD_SEL_ROLLMAP_CT_CHART";
                mPoint = rdoTHICK.IsChecked != null && (bool)rdoTHICK.IsChecked ? "1" : "2";
                sampleFlag = "Y";
            }
            else //RollPress
            {
                bizRuleName = "DA_PRD_SEL_ROLLMAP_RP_CHART";
                mPoint = "1";
                sampleFlag = "N";
            }

            DataTable inTable = new DataTable {TableName = "RQSTDT"};
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("MPOINT", typeof(string));
            inTable.Columns.Add("MPSTN", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dr["MPOINT"] = mPoint;
            dr["MPSTN"] = position;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;
            dr["EQPTID"] = _equipmentCode;
            inTable.Rows.Add(dr);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inTable);
            //string xml = ds.GetXml();

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private DataTable SelectRollMapPointInfo()
        {
            string bizRuleName, sampleFlag, adjFlag;

            if (string.Equals(_processCode, Process.COATING))
            {
                bizRuleName = "BR_PRD_SEL_ROLLMAP_CT_DEFECT_CHART";
                sampleFlag = "Y";
            }
            else
            {
                bizRuleName = "DA_PRD_SEL_ROLLMAP_RP_DEFECT_CHART";
                sampleFlag = "N";
            }

            adjFlag = "1";
            //RW_SCRAP 의 경우 절대좌표 adjFlag = 2인 경우만 표현 됨.
            DataTable inTable = new DataTable {TableName = "RQSTDT"};
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dr["EQPTID"] = _equipmentCode;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private void DrawBackGroundPointChart(DataTable dt, double xLength)
        {

            if (CommonVerify.HasTableRow(dt))
            {

                _xMaxLength = dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble() > xLength
                            ? dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble()
                            : xLength;

                chartRollMap.View.AxisX.Min = 0;
                chartRollMap.View.AxisY.Min = -80;   //Back 하단의 태그 표시로 인하여 AxisY Min 값을 설정 함.
                chartRollMap.View.AxisX.Max = _xMaxLength;
                chartRollMap.View.AxisY.Max = 220 + 3;   //무지부 3

                InitializePointChart(_xMaxLength);

                int rowIndex = string.Equals(_processCode, Process.COATING) ? 1 : 2;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    double yStartPosition = 0, yEndPosition = 0;

                    string adjLaneNo = dt.Rows[i]["ADJ_LANE_NO"].GetString();

                    var query = (from t in _dtLaneInfo.AsEnumerable()
                        where t.Field<string>("LANE_NO") != null
                              && t.Field<string>("LANE_NO") == adjLaneNo
                        select new
                        {
                            YStartPosition = t.Field<decimal>("Y_STRT_PSTN"),
                            YEndPosition = t.Field<decimal>("Y_END_PSTN")

                        }).FirstOrDefault();

                    if (query != null)
                    {
                        yStartPosition = query.YStartPosition.GetDouble();
                        yEndPosition = query.YEndPosition.GetDouble();
                    }
                    else
                    {
                        yStartPosition = 0;
                        yEndPosition = 100;
                    }

                    for (int row = 0; row < rowIndex; row++)
                    {
                        double lowerExtent;
                        double upperExtent;

                        if (string.Equals(_processCode, Process.COATING))
                        {
                            //lowerExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 : 0;
                            //upperExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 220 : 100;
                            lowerExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yStartPosition : 0 + yStartPosition;
                            upperExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yEndPosition : yEndPosition;
                        }
                        else
                        {
                            //lowerExtent = row == 0 ? 0 : 120;
                            //upperExtent = row == 0 ? 100 : 220;
                            lowerExtent = row == 0 ? 0 + yStartPosition : 120 + yStartPosition;
                            upperExtent = row == 0 ? yEndPosition : 120 + yEndPosition;
                        }

                        AlarmZone alarmZone = new AlarmZone();

                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString(Util.NVC(dt.Rows[i]["COLORMAP"]));
                        alarmZone.Near = dt.Rows[i]["ADJ_STRT_PSTN"].GetDouble();
                        alarmZone.Far = dt.Rows[i]["ADJ_END_PSTN"].GetDouble();
                        alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                        alarmZone.LowerExtent = lowerExtent;
                        alarmZone.UpperExtent = upperExtent;
                        alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;

                        int x = i;
                        alarmZone.PlotElementLoaded += (s, e) =>
                        {
                            PlotElement pe = (PlotElement)s;
                            if (pe is Lines)
                            {
                                double sourceStartPosition = Convert.ToDouble(dt.Rows[x]["SOURCE_STRT_PSTN"]);
                                double sourceEndPosition = Convert.ToDouble(dt.Rows[x]["SOURCE_END_PSTN"]);

                                string content = dt.Rows[x]["CMCDNAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                                 "Scan AVG : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}" ) + Environment.NewLine +
                                                 "ColorMap : " + Util.NVC(dt.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                                 "Offset : " + Util.NVC(dt.Rows[x]["SCAN_OFFSET"]);

                                ToolTipService.SetToolTip(pe, content);
                                ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                ToolTipService.SetShowDuration(pe, 60000);
                            }
                        };

                        chartRollMap.Data.Children.Add(alarmZone);
                    }
                }

                InitializeChartView(chartRollMap);
            }
        }

        private void DrawPointChart(DataTable dtPoint)
        {
            if (CommonVerify.HasTableRow(dtPoint))
            {
                var queryTop = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")).ToList();
                DataTable dtTop = queryTop.Any() ? MakeTableForDisplay(queryTop.CopyToDataTable(), ChartDisplayType.SurfaceTop) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.SurfaceTop);

                var queryBack = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")).ToList();
                DataTable dtBack = queryBack.Any() ? MakeTableForDisplay(queryBack.CopyToDataTable(), ChartDisplayType.SurfaceBack) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.SurfaceBack);

                if (CommonVerify.HasTableRow(dtTop))
                {
                    dtTop.TableName = "dtTop";
                    DrawRollMapDefect(dtTop);
                }
                if (CommonVerify.HasTableRow(dtBack))
                {
                    dtBack.TableName = "dtBack";
                    DrawRollMapDefect(dtBack);
                }

                var queryVisionTop = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP") ||
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")).ToList();
                DataTable dtVisionTop = queryVisionTop.Any() ? MakeTableForDisplay(queryVisionTop.CopyToDataTable(), ChartDisplayType.TagVisionTop) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.TagVisionTop);

                var queryVisionBack = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK") ||
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")).ToList();
                DataTable dtVisionBack = queryVisionBack.Any() ? MakeTableForDisplay(queryVisionBack.CopyToDataTable(), ChartDisplayType.TagVisionBack) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.TagVisionBack);

                if (CommonVerify.HasTableRow(dtVisionTop))
                {
                    dtVisionTop.TableName = "dtTop";
                    DrawRollMapVision(dtVisionTop);
                }

                if (CommonVerify.HasTableRow(dtVisionBack))
                {
                    dtVisionBack.TableName = "dtBack";
                    DrawRollMapVision(dtVisionBack);
                }

                var queryMark = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("MARK")).ToList();
                DataTable dtMark = queryMark.Any() ? queryMark.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtMark))
                {
                    DrawRollMapTagYAxisLineLabel(dtMark);
                }

                var queryTagSection = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawRollMapTagSection(dtTagSection);
                }

                var queryTagSpot = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    DrawRollMapTagSpot(dtTagSpot);
                }

                if (string.Equals(_processCode, Process.ROLL_PRESSING))
                {
                    var queryOverLayVision = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
                    DataTable dtOverLayVision = queryOverLayVision.Any() ? MakeTableForDisplay(queryOverLayVision.CopyToDataTable(), ChartDisplayType.OverLayVision) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.OverLayVision);

                    if (CommonVerify.HasTableRow(dtOverLayVision))
                    {
                        DrawRollMapVision(dtOverLayVision);
                    }

                    var queryRewinderScrap = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("RW_SCRAP")).ToList();
                    DataTable dtRwScrap = queryRewinderScrap.Any() ? queryRewinderScrap.CopyToDataTable() : dtPoint.Clone();

                    if (CommonVerify.HasTableRow(dtRwScrap))
                    {
                        DrawRollMapRewinderScrap(dtRwScrap);
                    }
                }

                //if(string.Equals(_processCode, Process.COATING)) DrawRollMapLane(dtPoint);

                var queryPointTopLegend = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")).ToList();
                DataTable dtPointTopLegend = queryPointTopLegend.Any() ? queryPointTopLegend.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtPointTopLegend))
                {
                    dtPointTopLegend.TableName = "TopLegend";
                    DrawPointLegend(dtPointTopLegend);
                }

                var queryPointBackLegend = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
                DataTable dtPointBackLegend = queryPointBackLegend.Any() ? queryPointBackLegend.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtPointBackLegend))
                {
                    dtPointBackLegend.TableName = "BackLegend";
                    DrawPointLegend(dtPointBackLegend);
                }
            }
        }

        private void DrawRollMapDefect(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt)) return;

            XYDataSeries ds = new XYDataSeries();
            ds.Label = "points";
            ds.ItemsSource = DataTableConverter.Convert(dt);
            ds.ValueBinding = new Binding("Y_PSTN");
            ds.XValueBinding = new Binding("ADJ_X_PSTN");
            ds.Cursor = Cursors.Hand;
            ds.SymbolSize = new Size(7, 7);

            ds.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;

                if (pe is DotSymbol)
                {
                    DataPoint dp = pe.DataPoint;

                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString(DataTableConverter.GetValue(dp.DataObject, "COLORMAP").GetString());
                    pe.Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    pe.Stroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    double xPosition;
                    double yPosition;
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_X_PSTN")), out xPosition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_Y_PSTN")), out yPosition);
                    string defectName = DataTableConverter.GetValue(dp.DataObject, "ABBR_NAME").GetString();

                    string content = defectName + " [X:" + $"{xPosition:###,###,###,##0.##}" + "m" + ", Y:" + $"{yPosition:###,###,###,##0.##}" + "mm" + "]";
                    ToolTipService.SetToolTip(pe, content);
                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                    ToolTipService.SetShowDuration(pe, 60000);
                }
            };
            chartRollMap.Data.Children.Add(ds);
        }

        private void DrawRollMapVision(DataTable dt)
        {
            if (!CommonVerify.HasTableRow(dt)) return;

            foreach (DataRow row in dt.Rows)
            {
                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                AlarmZone alarmZone = new AlarmZone
                {
                    Near = row["ADJ_STRT_PSTN"].GetDouble(),
                    Far = row["ADJ_END_PSTN"].GetDouble(),
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = row["SOURCE_Y_STRT_PSTN"].GetDouble(),
                    UpperExtent = row["SOURCE_Y_END_PSTN"].GetDouble(),
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    Cursor = Cursors.Hand
                };

                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    if (pe is Lines)
                    {
                        double startPosition;
                        double endPosition;
                        double.TryParse(Util.NVC(row["SOURCE_STRT_PSTN"]), out startPosition);
                        double.TryParse(Util.NVC(row["SOURCE_END_PSTN"]), out endPosition);
                        string content = row["ABBR_NAME"] + "[" + $"{startPosition:###,###,###,##0.##}" + "m" + "~" + $"{endPosition:###,###,###,##0.##}" + "m" + "]";
                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                chartRollMap.Data.Children.Add(alarmZone);
            }
        }

        private void DrawRollMapRewinderScrap(DataTable dtRwScrap)
        {
            if (CommonVerify.HasTableRow(dtRwScrap))
            {
                dtRwScrap = MakeTableForDisplay(dtRwScrap, ChartDisplayType.RewinderScrap);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtRwScrap);
                ds.XValueBinding = new Binding("ADJ_END_PSTN");
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
                chartRollMap.Data.Children.Add(ds);
            }
        }

        private void DrawRollMapTagYAxisLineLabel(DataTable dt)
        {
            // 종축 라인 설정 TOP, BACK 동일 함.
            DataRow[] drMark = dt.Select();
            if (drMark.Length > 0)
            {
                foreach (DataRow row in drMark)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_STRT_PSTN"]), out sourceStartPosition);
                    string content = row["CMCDNAME"] + "[" + Util.NVC(row["ROLLMAP_CLCT_TYPE"]) + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";

                    chartRollMap.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_STRT_PSTN"].GetDouble(), row["ADJ_STRT_PSTN"].GetDouble() },
                        ValuesSource = new double[] { -20, 220 },
                        ConnectionStroke = new SolidColorBrush(Colors.Black),
                        ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                        ToolTip = content
                    });
                }
            }

            // 데이터가 없는 경우 발생 시 에러 방지를 위하여 체크
            var queryLabel = dt.AsEnumerable().ToList();
            DataTable dtLabel = queryLabel.Any() ? MakeTableForDisplay(queryLabel.CopyToDataTable(), ChartDisplayType.TagToolTip) : MakeTableForDisplay(dt.Clone(), ChartDisplayType.TagToolTip);

            var dsMarkLabel = new XYDataSeries();
            dsMarkLabel.ItemsSource = DataTableConverter.Convert(dtLabel);
            dsMarkLabel.XValueBinding = new Binding("ADJ_STRT_PSTN");
            dsMarkLabel.ValueBinding = new Binding("SOURCE_Y_PSTN");
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
            chartRollMap.Data.Children.Add(dsMarkLabel);
        }

        private void DrawRollMapTagSection(DataTable dtTagPosition)
        {
            // Start, End 라벨 삽입
            if (CommonVerify.HasTableRow(dtTagPosition))
            {
                // Start, End 이미지 두번의 표현으로 for문 사용
                for (int x = 0; x < 2; x++)
                {
                    DataTable dtTag = MakeTableForDisplay(dtTagPosition, x == 0 ? ChartDisplayType.TagStart : ChartDisplayType.TagEnd);

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
                    chartRollMap.Data.Children.Add(ds);
                }
            }
        }

        private void DrawRollMapTagSpot(DataTable dtTagSpot)
        {
            // 롤프레스공정에만 해당
            if (CommonVerify.HasTableRow(dtTagSpot))
            {
                dtTagSpot = MakeTableForDisplay(dtTagSpot, ChartDisplayType.TagSpot);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTagSpot);
                ds.XValueBinding = new Binding("ADJ_STRT_PSTN");
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
                    // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                };
                chartRollMap.Data.Children.Add(ds);
            }
        }

        private void DrawRollMapLane()
        {
            DataTable dtLaneInfo = _dtLaneInfo;

            if (CommonVerify.HasTableRow(dtLaneInfo))
            {
                DataTable dtLane = new DataTable();
                dtLane.Columns.Add("SOURCE_STRT_PSTN", typeof(string));
                dtLane.Columns.Add("SOURCE_Y_PSTN", typeof(string));
                dtLane.Columns.Add("LANEINFO", typeof(string));

                string[] typeArray = { "Top", "Back" };

                for (int i = 0; i < typeArray.Length; i++)
                {
                    string typeCode = typeArray[i].GetString();

                    for (int j = 0; j < dtLaneInfo.Rows.Count; j++)
                    {
                        if (string.IsNullOrEmpty(dtLaneInfo.Rows[j]["LANE_NO"].GetString()))
                            continue;

                        DataRow drLane = dtLane.NewRow();
                        drLane["SOURCE_STRT_PSTN"] = _xMaxLength - (_xMaxLength * 0.01);
                        drLane["SOURCE_Y_PSTN"] = typeCode == "Top" ? 120 + dtLaneInfo.Rows[j]["Y_PSTN"].GetDouble() : dtLaneInfo.Rows[j]["Y_PSTN"].GetDouble();
                        drLane["LANEINFO"] = "Lane " + dtLaneInfo.Rows[j]["LANE_NO"];
                        dtLane.Rows.Add(drLane);
                    }
                }

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
                    // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                };
                chartRollMap.Data.Children.Add(dsLane);
            }
        }

        private void DrawPointLegend(DataTable dt)
        {
            if (_isResearch) return;

            Grid grid = dt.TableName == "TopLegend" ? grdDefectTopLegend : grdDefectBackLegend;

            if (CommonVerify.HasTableRow(dt))
            {
                if (grid.ColumnDefinitions.Count > 0) grid.ColumnDefinitions.Clear();
                if (grid.RowDefinitions.Count > 0) grid.RowDefinitions.Clear();

                for (int x = 0; x < 2; x++)
                {
                    ColumnDefinition gridColumn = new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)};
                    grid.ColumnDefinitions.Add(gridColumn);
                }

                grid.Children.Clear();

                var queryRow = (from t in dt.AsEnumerable()
                                select new
                                {
                                    DefectName = t.Field<string>("ABBR_NAME"),
                                    ColorMap = t.Field<string>("COLORMAP"),
                                    DefectShape = t.Field<string>("DEFECT_SHAPE"),
                                    MeasurementCode = t.Field<string>("EQPT_MEASR_PSTN_ID")
                                }).Distinct().ToList();

                int y = 0;

                if (queryRow.Any())
                {
                    foreach (var item in queryRow)
                    {
                        RowDefinition gridRow = new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)};
                        grid.RowDefinitions.Add(gridRow);

                        StackPanel stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(2, 2, 2, 2)
                        };

                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString(Util.NVC(item.ColorMap));

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
                                                                             new Thickness(2, 2, 2, 2),  // new Thickness(5, 5, 5, 5),
                                                                             null,
                                                                             null);

                                TextBlock rectangleDescription = CreateTextBlock(item.DefectName,
                                                                                  HorizontalAlignment.Center,
                                                                                  VerticalAlignment.Center,
                                                                                  11,
                                                                                  FontWeights.Bold,
                                                                                  Brushes.Black,
                                                                                  new Thickness(1, 1, 1, 1), // new Thickness(5, 5, 5, 5),
                                                                                  new Thickness(0, 0, 0, 0),
                                                                                  item.MeasurementCode,
                                                                                  Cursors.Hand);
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
                                                                         new Thickness(2, 2, 2, 2),  // new Thickness(5, 5, 5, 5),
                                                                         null,
                                                                         null);

                                TextBlock ellipseDescription = CreateTextBlock(item.DefectName,
                                                                                 HorizontalAlignment.Center,
                                                                                 VerticalAlignment.Center,
                                                                                 11,
                                                                                 FontWeights.Bold,
                                                                                 Brushes.Black,
                                                                                 new Thickness(1, 1, 1, 1),  // new Thickness(5, 5, 5, 5),
                                                                                 new Thickness(0, 0, 0, 0),
                                                                                 item.MeasurementCode,
                                                                                 Cursors.Hand);
                                stackPanel.Children.Add(ellipseLegend);
                                stackPanel.Children.Add(ellipseDescription);

                                ellipseDescription.PreviewMouseUp += DescriptionOnPreviewMouseUp;
                                break;
                        }
                        Grid.SetColumn(stackPanel, 0);
                        Grid.SetRow(stackPanel, y);
                        grid.Children.Add(stackPanel);

                        y++;
                    }
                }
            }
        }

        private void DrawRollMapSampleYAxisLine()
        {
            //const string bizRuleName = "DA_PRD_SEL_ROLLMAP_HEAD_LENGTH";
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_CT_HEAD";

            DataTable inTable = new DataTable {TableName = "RQSTDT"};
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dr["ADJFLAG"] = "1";
            inTable.Rows.Add(dr);

            DataTable dtSample = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            string[] petScrap = new string[] { "PET", "SCRAP" };
            DataRow[] drSample = dtSample.Select("SMPL_FLAG = 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");

            if (drSample.Length > 0)
            {
                foreach (DataRow row in drSample)
                {
                    chartRollMap.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["SOURCE_END_PSTN"].GetDouble(), row["SOURCE_END_PSTN"].GetDouble() },
                        ValuesSource = new double[] { 0, 220 },
                        ConnectionStroke = new SolidColorBrush(Colors.DarkRed),
                    });
                }

                DataRow[] drAccumulateSample = dtSample.Select("EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP' ");
                DataTable dtAccumulateResult = MakeTableForDisplay(drAccumulateSample.CopyToDataTable().Copy(), ChartDisplayType.Sample);

                var result = from r in dtAccumulateResult.AsEnumerable()
                             where r.Field<string>("SMPL_FLAG") == "Y"
                             select r;
                DataTable dtResult = result.CopyToDataTable();

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtResult);
                ds.XValueBinding = new Binding("SOURCE_END_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartSample"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);
                ds.Cursor = Cursors.Hand;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                chartRollMap.Data.Children.Add(ds);
            }

            var query = (from t in dtSample.AsEnumerable() where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
            if (query.Any())
            {
                DataTable dtMaxLength = new DataTable();
                dtMaxLength.Columns.Add("RAW_END_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_END_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_Y_PSTN", typeof(double));
                dtMaxLength.Columns.Add("TOOLTIP", typeof(string));

                DataRow newRow = dtMaxLength.NewRow();
                newRow["RAW_END_PSTN"] = "0";
                newRow["SOURCE_END_PSTN"] = "0";
                newRow["SOURCE_Y_PSTN"] = 220;
                newRow["TOOLTIP"] = string.Empty;
                dtMaxLength.Rows.Add(newRow);

                var queryLength = (from t in dtSample.AsEnumerable()
                                   where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                   select new
                                   {
                                       RawEndPosition = t.Field<decimal?>("RAW_END_PSTN"),
                                       RawStartPosition = t.Field<decimal?>("RAW_STRT_PSTN"),
                                       EndPosition = t.Field<decimal?>("SOURCE_END_PSTN"),
                                       RowNum = t.Field<Int32>("ROW_NUM")
                                   }).ToList();

                if (queryLength.Any())
                {
                    foreach (var item in queryLength)
                    {
                        DataRow newLength = dtMaxLength.NewRow();
                        newLength["RAW_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_Y_PSTN"] = 220;
                        newLength["TOOLTIP"] = item.RowNum + " Cut" + "( " + $"{item.RawStartPosition:###,###,###,##0.##}" + "m" + " ~ " + $"{item.RawEndPosition:###,###,###,##0.##}" + "m" + ")";
                        dtMaxLength.Rows.Add(newLength);
                    }
                    XYDataSeries ds = new XYDataSeries();
                    ds.ItemsSource = DataTableConverter.Convert(dtMaxLength);
                    ds.XValueBinding = new Binding("SOURCE_END_PSTN");
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
                    chartRollMap.Data.Children.Add(ds);
                }
            } 
        }

        private void DrawRollMapStartEndYAxis()
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_RP_HEAD";

            DataTable inTable = new DataTable {TableName = "RQSTDT"};
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = _lotId;
            dr["WIPSEQ"] = _wipSeq;
            dr["ADJFLAG"] = "1";
            inTable.Rows.Add(dr);

            DataTable dtSample = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            string[] petScrap = new string[] { "PET", "SCRAP" };
            var query = (from t in dtSample.AsEnumerable() where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
            if (query.Any())
            {
                DataTable dtMaxLength = new DataTable();
                dtMaxLength.Columns.Add("RAW_END_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_END_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_Y_PSTN", typeof(double));

                DataRow newRow = dtMaxLength.NewRow();
                newRow["RAW_END_PSTN"] = "0";
                newRow["SOURCE_END_PSTN"] = "0";
                newRow["SOURCE_Y_PSTN"] = 220;
                dtMaxLength.Rows.Add(newRow);

                var queryLength = (from t in dtSample.AsEnumerable()
                                   where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                   select new
                                   {
                                       RawEndPosition = t.Field<decimal?>("RAW_END_PSTN"),
                                       RawStartPosition = t.Field<decimal?>("RAW_STRT_PSTN"),
                                       EndPosition = t.Field<decimal?>("SOURCE_END_PSTN"),
                                       RowNum = t.Field<Int32>("ROW_NUM")
                                   }).ToList();

                if(queryLength.Any())
                {
                    foreach (var item in queryLength)
                    {
                        DataRow newLength = dtMaxLength.NewRow();

                        newLength["RAW_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_Y_PSTN"] = 220;
                        dtMaxLength.Rows.Add(newLength);
                    }
                    XYDataSeries ds = new XYDataSeries();
                    ds.ItemsSource = DataTableConverter.Convert(dtMaxLength);
                    ds.XValueBinding = new Binding("RAW_END_PSTN");
                    ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                    ds.ChartType = ChartType.XYPlot;
                    ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                    ds.PointLabelTemplate = grdMain.Resources["chartRollPressLength"] as DataTemplate;
                    ds.Margin = new Thickness(0, 0, 0, 0);
                    ds.Cursor = Cursors.Hand;

                    ds.PlotElementLoaded += (s, e) =>
                    {
                        PlotElement pe = (PlotElement)s;
                        pe.Stroke = new SolidColorBrush(Colors.Transparent);
                        pe.Fill = new SolidColorBrush(Colors.Transparent);
                    };

                    chartRollMap.Data.Children.Add(ds);
                }
            }

            // 롤프레스 공정에서만 호출 함. 이음매(PET), SCRAP 표현 
            var queryPetScrap = (from t in dtSample.AsEnumerable() where petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
            if (queryPetScrap.Any())
            {
                DrawRollMapScrapPetYAxisLine(_processCode, dtSample);
            }
        }

        private void DrawRollMapScrapPetYAxisLine(string processCode, DataTable dt)
        {
            if (string.Equals(processCode, Process.COATING)) return;

            double lowerExtent = 0;
            double upperExtent = 220;

            DataRow[] drPetScrap = dt.Select("EQPT_MEASR_PSTN_ID = 'PET' OR EQPT_MEASR_PSTN_ID = 'SCRAP' ");

            if (drPetScrap.Length < 1) return;

            DataTable dtPetScrap = new DataTable();
            dtPetScrap.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            dtPetScrap.Columns.Add("RAW_STRT_PSTN", typeof(double));
            dtPetScrap.Columns.Add("RAW_END_PSTN", typeof(double));
            dtPetScrap.Columns.Add("Y_PSTN", typeof(double));
            dtPetScrap.Columns.Add("COLORMAP", typeof(string));
            dtPetScrap.Columns.Add("TOOLTIP", typeof(string));
            dtPetScrap.Columns.Add("TAG", typeof(string));

            foreach (DataRow row in drPetScrap)
            {
                var convertFromString = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000")) : new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#BDBDBD"));
                string colorMap = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? "#000000" : "#BDBDBD";
                string content = "[" + row["DFCT_NAME"].GetString() + "] " + $"{row["RAW_STRT_PSTN"]:###,###,###,##0.##}" + "m";
                string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["RAW_STRT_PSTN"].GetString() + ";" + row["RAW_END_PSTN"].GetString()
                      + ";" + row["LOTID"].GetString() + ";" + row["WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                chartRollMap.Data.Children.Add(new XYDataSeries()
                {
                    ChartType = ChartType.Line,
                    XValuesSource = new[] { row["RAW_STRT_PSTN"], row["RAW_STRT_PSTN"] },
                    ValuesSource = new double[] { lowerExtent, upperExtent },
                    ConnectionStroke = convertFromString,
                    ConnectionStrokeThickness = 3,
                    ConnectionFill = convertFromString,
                });

                dtPetScrap.Rows.Add(row["EQPT_MEASR_PSTN_ID"], row["RAW_STRT_PSTN"], row["RAW_END_PSTN"], 213, colorMap, content, tag);
            }

            if (CommonVerify.HasTableRow(dtPetScrap))
            {
                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dtPetScrap);
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

                chartRollMap.Data.Children.Add(dsPetScrap);
            }
        }

        private void InitializeControls()
        {
            if (grdDefectTopLegend.ColumnDefinitions.Count > 0) grdDefectTopLegend.ColumnDefinitions.Clear();
            if (grdDefectTopLegend.RowDefinitions.Count > 0) grdDefectTopLegend.RowDefinitions.Clear();
            grdDefectTopLegend.Children.Clear();

            if (grdDefectBackLegend.ColumnDefinitions.Count > 0) grdDefectBackLegend.ColumnDefinitions.Clear();
            if (grdDefectBackLegend.RowDefinitions.Count > 0) grdDefectBackLegend.RowDefinitions.Clear();
            grdDefectBackLegend.Children.Clear();

            BeginUpdateChart();
        }

        private void InitializeChart()
        {
            chartRollMap.View.AxisX.Min = 0;
            chartRollMap.View.AxisY.Min = -80;   //Back 하단의 태그 표시로 인하여 AxisY Min 값을 설정 함.
            chartRollMap.View.AxisX.Max = _xMaxLength.Equals(0) ? 10 : _xMaxLength;
            chartRollMap.View.AxisY.Max = 220 + 3;   //무지부 3
            InitializePointChart(_xMaxLength.Equals(0) ? 10 : _xMaxLength);
            InitializeChartView(chartRollMap);
        }

        private void InitializePointChart(double xLength)
        {

            DataTable dt = SelectRollMapGplmWidth();

            if (CommonVerify.HasTableRow(dt))
            {
                DataRow[] dr = dt.Select();
                int drLength = dr.Length;
                decimal sumLength = dt.AsEnumerable().Sum(s => s.Field<decimal>("LOV_VALUE"));
                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#D5D5D5");

                string[] typeArray = { "Top", "Back" };

                for (int i = 0; i < typeArray.Length; i++)
                {
                    string typeCode = typeArray[i].GetString();

                    double yLength = 0;
                    int rowIndex = 0;

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(dt.Rows[j]["LANE_NO"].GetString()))
                        {
                            yLength = yLength + dt.Rows[j]["LANE_LENGTH"].GetDouble();
                            rowIndex++;
                            continue;
                        }

                        if (typeCode == "Top")
                        {
                            if (rowIndex == 0)
                            {   // 무지부 상단
                                AlarmZone alarmZone = new AlarmZone
                                {
                                    Near = 0,
                                    Far = xLength,
                                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                    LowerExtent = 220,
                                    UpperExtent = 220 + dt.Rows[j]["LANE_LENGTH"].GetDouble(),
                                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                                };
                                chartRollMap.Data.Children.Insert(0, alarmZone);
                            }
                            else
                            {
                                // 무지부 하단
                                if (rowIndex == dt.Rows.Count - 1)
                                {
                                    //무지부 하단
                                    AlarmZone alarmZone = new AlarmZone
                                    {
                                        Near = 0,
                                        Far = xLength,
                                        ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                        LowerExtent = 120,
                                        UpperExtent = 120 - dt.Rows[j]["LANE_LENGTH"].GetDouble(),
                                        ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                                    };
                                    chartRollMap.Data.Children.Insert(0, alarmZone);
                                }
                                else
                                {
                                    //무지부 중단?
                                    AlarmZone alarmZone = new AlarmZone
                                    {
                                        Near = 0,
                                        Far = xLength,
                                        ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                        LowerExtent = 120 + yLength,
                                        UpperExtent = 120 + yLength + dt.Rows[j]["LANE_LENGTH"].GetDouble(),
                                        ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                                    };
                                    chartRollMap.Data.Children.Insert(0, alarmZone);
                                }

                            }
                        }
                        else
                        {
                            // BACK 영역 무지부
                            if (rowIndex == 0)
                            {
                                //무지부 상단                                    
                                AlarmZone alarmZone = new AlarmZone
                                {
                                    Near = 0,
                                    Far = xLength,
                                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                    LowerExtent = 100,
                                    UpperExtent = 100 - dt.Rows[j]["LANE_LENGTH"].GetDouble(),
                                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                                };
                                chartRollMap.Data.Children.Insert(0, alarmZone);
                            }
                            else
                            {
                                // 무지부 하단
                                if (rowIndex == dr.Length - 1)
                                {
                                    AlarmZone alarmZone = new AlarmZone
                                    {
                                        Near = 0,
                                        Far = xLength,
                                        ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                        LowerExtent = 0,
                                        UpperExtent = -dt.Rows[j]["LANE_LENGTH"].GetDouble(),
                                        ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                                    };
                                    chartRollMap.Data.Children.Insert(0, alarmZone);
                                }
                                else
                                {
                                    //무지부 중단?
                                    AlarmZone alarmZone = new AlarmZone
                                    {
                                        Near = 0,
                                        Far = xLength,
                                        ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                        LowerExtent = yLength,
                                        UpperExtent = yLength - dt.Rows[j]["LANE_LENGTH"].GetDouble(),
                                        ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                                    };
                                    chartRollMap.Data.Children.Insert(0, alarmZone);
                                }
                            }
                        }
                        rowIndex++;
                        yLength = yLength + dt.Rows[j]["LANE_LENGTH"].GetDouble();
                    }
                }
            }

            /*
            // Top 무지부 상단
            var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#D5D5D5");
            AlarmZone alarmZone1 = new AlarmZone
            {
                Near = 0,
                Far = xLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 220,
                UpperExtent = 223,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            chartRollMap.Data.Children.Insert(0, alarmZone1);

            AlarmZone alarmZone2 = new AlarmZone
            {
                Near = 0,
                Far = xLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 120,
                UpperExtent = 120 - 2.5,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            chartRollMap.Data.Children.Insert(0, alarmZone2);


            // Back 무지부 상단
            AlarmZone alarmZone3 = new AlarmZone
            {
                Near = 0,
                Far = xLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 100,
                UpperExtent = 100 - 2.5,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            chartRollMap.Data.Children.Insert(0, alarmZone3);

            // Back 무지부 하단
            AlarmZone alarmZone4 = new AlarmZone
            {
                Near = 0,
                Far = xLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 0,
                UpperExtent = -3,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            chartRollMap.Data.Children.Insert(0, alarmZone4);
            */
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


            if (c1Chart.Name == "chartRollMap")
            {
                double majorUnit;
                double length = _xMaxLength;

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
                c1Chart.View.AxisX.MajorUnit = majorUnit; //Math.Round(_xMaxLength / 200) * 10;
                c1Chart.View.AxisX.AnnoFormat = "#,##0";
            }
            else
            {
                c1Chart.View.AxisX.Foreground = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void SetScale(double scale)
        {
            chartRollMap.View.AxisX.Scale = scale;
            btnRefresh.IsCancel = !scale.Equals(1);
            btnZoomIn.IsCancel = scale > 0.002;
            btnZoomOut.IsCancel = scale < 1;

            UpdateScrollbars();
        }

        private void UpdateScrollbars()
        {
            double sxTop = chartRollMap.View.AxisX.Scale;
            AxisScrollBar axisScrollBarCoater = (AxisScrollBar)chartRollMap.View.AxisX.ScrollBar;
            axisScrollBarCoater.Visibility = sxTop >= 1.0 ? Visibility.Collapsed : Visibility.Visible;
            axisScrollBarCoater.AxisRangeChanged += AxisScrollBarTopOnAxisRangeChanged;
        }

        private void BeginUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                c1Chart.Reset(true);
                c1Chart.BeginUpdate();
                c1Chart.View.AxisX.Reversed = false;

                if (c1Chart.Name == "chartRollMap")
                {
                    SetScale(1.0);
                    c1Chart.View.AxisX.MinScale = 0.05;
                    //c1Chart.View.Margin = new Thickness(5);
                    c1Chart.View.Margin = new Thickness(5, 10, 5, 20);
                }
            }
        }

        private void EndUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                if (string.Equals(_processCode, Process.COATING) && c1Chart.Name != "chart")
                {
                    c1Chart.View.AxisX.Reversed = true;
                }

                if (string.Equals(_processCode, Process.ROLL_PRESSING))
                {
                    c1Chart.View.AxisX.Reversed = true;
                }

                c1Chart.EndUpdate();
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

        private static DataTable MakeTableForDisplay(DataTable dt, ChartDisplayType chartDisplayType)
        {
            var dtBinding = dt.Copy();

            if (!CommonVerify.HasTableRow(dt)) return dtBinding;

            if (chartDisplayType == ChartDisplayType.TagStart || chartDisplayType == ChartDisplayType.TagEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                ////NFF Lane 불량 정보 표기에 따른 변경 - Start

                //// 2D BCR 계산 제외
                var queryDtBinding = (from t in dtBinding.AsEnumerable()
                                      where (!("TAG_SECTION".Equals(t.Field<string>("EQPT_MEASR_PSTN_ID")) &&
                                      t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty() &&
                                      t.Field<string>("ADJ_LANE_NO").IsNotEmpty())
                                      )
                                      select t).ToList();

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
                if (CommonVerify.HasTableRow(_dt2DBarcodeInfo))
                {
                    laneQty = Convert.ToUInt16(_dt2DBarcodeInfo.Rows[0]["LANE_QTY"]);
                    dfct2DBcrStd = _dt2DBarcodeInfo.Rows[0]["DFCT_2D_BCR_STD"].GetString();
                }
                //foreach (DataRow row in dtBinding.Rows)
                foreach (DataRow row in queryDtBinding)
                {

                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString(); ;

                    if ("TAG_SECTION".Equals(row["EQPT_MEASR_PSTN_ID"])
                        && "L002".Equals(dfct2DBcrStd.Trim()) //L001: Normal, L002: NFF, L003: 소형(2170)
                        && !"".Equals(row["MRK_2D_BCD_STR"].ToString().Trim()))
                    {
                        if (query2DBcrWithCountInfoToDict.Any())
                        {
                            string mark2DBarcode = row["MRK_2D_BCD_STR"].ToString();
                            decimal adjStrtPstn = Convert.ToDecimal(row["ADJ_STRT_PSTN"]);
                            decimal adjEndPstn = Convert.ToDecimal(row["ADJ_END_PSTN"]);
                            List<Tuple<int, decimal, decimal>> mark2DBarcodeCountInfoList = query2DBcrWithCountInfoToDict[mark2DBarcode];

                            mark2DBarcodeCountInfoList.ForEach(
                                t =>
                                {
                                    if (laneQty == t.Item1 && adjStrtPstn == t.Item2 && adjEndPstn == t.Item3)
                                    {
                                        //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                                        row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                                    }
                                    else
                                    {
                                        //// MRK_2D_BCD_STR이 존재할 경우 그룹핑하여 LANE_QTY와 비교
                                        String laneInfo = row["MRK_2D_BCD_STR"].ToString().Trim().Substring(0, 2);
                                        row["TOOLTIP"] = "(" + laneInfo + ")" + row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                                    }
                                }
                                );
                            ////row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -50 : -70;
                            //row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                            //row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString(); ;
                            //row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                            //row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                        }
                    }
                    else
                    {
                        row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                    }
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagToolTip)
            {
                //dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_Y_PSTN"] = -30;
                    row["TOOLTIP"] = row["CMCDNAME"] + "[" + row["ROLLMAP_CLCT_TYPE"] + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagVisionTop || chartDisplayType == ChartDisplayType.TagVisionBack)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_STRT_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_END_PSTN", DataType = typeof(double) });

                int maxYValue = chartDisplayType == ChartDisplayType.TagVisionBack ? 100 : 220;

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_STRT_PSTN"] = maxYValue - 10;
                    row["SOURCE_Y_END_PSTN"] = maxYValue;
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
                        row["SOURCE_Y_PSTN"] = 10;
                    }
                    else
                    {
                        row["SOURCE_AVG_VALUE"] = DBNull.Value;
                        row["SOURCE_Y_PSTN"] = 10;
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
                    dtBinding.Rows[i]["SOURCE_Y_PSTN"] = 220;
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
                    double.TryParse(Util.NVC(row["SOURCE_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_Y_PSTN"] = -80; //- 60;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["CMCDNAME"] + "[" + sourceStartPosition + "m]";
                    row["TAG"] = null;
                }
            }
            else if (chartDisplayType == ChartDisplayType.SurfaceTop || chartDisplayType == ChartDisplayType.SurfaceBack)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "Y_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["Y_PSTN"] = chartDisplayType == ChartDisplayType.SurfaceTop ? row["Y_POSITION"].GetDouble() + 120 : row["Y_POSITION"].GetDouble();
                }
            }
            else if (chartDisplayType == ChartDisplayType.OverLayVision)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_STRT_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_END_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_STRT_PSTN"] = 100 - 10;
                    row["SOURCE_Y_END_PSTN"] = 100;
                }
            }
            else if (chartDisplayType == ChartDisplayType.RewinderScrap)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_PSTN"] = -70;
                    row["TAGNAME"] = "R";
                    row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + " ]";
                    row["TAG"] = row["EQPT_MEASR_PSTN_ID"] + ";" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                }
            }


            dtBinding.AcceptChanges();
            return dtBinding;
        }

        private static Rectangle CreateRectangle(HorizontalAlignment horizontal, VerticalAlignment vertical, double height, double width, SolidColorBrush brush, SolidColorBrush strokebrush, double thickness, Thickness margine, string name, Cursor cursor)
        {
            Rectangle rectangle = new Rectangle
            {
                HorizontalAlignment = horizontal,
                VerticalAlignment = vertical,
                Height = height,
                Width = width,
                StrokeThickness = thickness,
                Stroke = strokebrush,
                Margin = margine,
                Fill = brush,
                Name = "Rect" + name,
                Cursor = cursor
            };

            return rectangle;
        }

        private static TextBlock CreateTextBlock(string content, HorizontalAlignment horizontal, VerticalAlignment vertical, int fontsize, FontWeight fontweights, SolidColorBrush brush, Thickness margine, Thickness padding, string name, Cursor cursor)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = content,
                HorizontalAlignment = horizontal,
                VerticalAlignment = vertical,
                FontSize = fontsize,
                FontWeight = fontweights,
                Foreground = brush,
                Margin = margine,
                Padding = padding,
                Name = name,
                Cursor = cursor
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

        private enum ChartDisplayType
        {
            TagStart,
            TagEnd,
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private static DataTable Get2DBarcodeInfo(string lotId, string eqptId)
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
        #endregion


    }
}
