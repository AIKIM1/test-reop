/*************************************************************************************
 Created Date : 2021.06.02
      Creator : 신광희
   Decription : 자동차 전극 Roll Map - 롤맵 차트(ROLLPRESS)
--------------------------------------------------------------------------------------
 [Change History]
  2021.06.02  신광희 : Initial Created
  코드 복사해서 재사용하지 말고 C1Chart 익힌 후 참조 하시고, 참조 시 출처를 표시 바람.
  2024.03.19  김지호 : [E20240319-000656] 범례에 Non 추가 및 차트 색 (회색) 추가, SCAN_COLRMAP에 Err, Non, Ch 시에 Tooltip에 데이터 오류, 데이터 미수신, 데이터 미측정 내용 표시
  2024.06.12  정기동 : 부분 LANE 불량 툴팁 표기 기능 추가 (TAG_SECTION_LANE) 
**************************************************************************************/


using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System.Data;
using System;
using System.Collections.Generic;
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

    public partial class COM001_ROLLMAP_ROLLPRESS : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _runLotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _laneQty = string.Empty;
        private string _version = string.Empty;
        private string _expFlag = string.Empty;
        private string _sideType = string.Empty;

        private string _coaterEquipmentCode = string.Empty;
        private string _coaterinputLotCode = string.Empty;
        private string _coaterExpFlag = string.Empty;
        private string _coaterWipSeq = string.Empty;

        private double _xMaxLength;
        private double _xCoaterMaxLength;
        private DataTable _dtLineLegend;
        private DataTable _dtCoaterDefect;
        private DataTable _dtRollPressDefect;
        private DataTable _dtCoaterPoint;
        private DataTable _dtRollPressPoint;
        private DataTable _dtCoaterGraph;
        private DataTable _dtRollPressGraph;
        //private DataTable _dtLaneInfo;

        private DataTable _dtCoaterLaneInfo;
        private DataTable _dtRollPressLaneInfo;

        private DataTable _dtLaneLegend;
        private DataTable _dtLane;

        private bool _isResearch;

        #endregion

        public COM001_ROLLMAP_ROLLPRESS()
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
            try
            {
                if (!ValidationSearch()) return;

                ShowLoadingIndicator();

                _isResearch = false;

                _dtCoaterDefect?.Clear();
                _dtRollPressDefect?.Clear();
                _dtCoaterPoint?.Clear();
                _dtRollPressPoint?.Clear();
                _dtCoaterGraph?.Clear();
                _dtRollPressGraph?.Clear();

                //_dtLaneInfo?.Clear();
                _dtCoaterLaneInfo?.Clear();
                _dtRollPressLaneInfo?.Clear();
                _dtLane?.Clear();

                DoEvents();

                // 조회 시 데이터 유무에 관계없이 차트 초기화 설정
                InitializeControls();

                double maxLength = 0;

                _coaterEquipmentCode = string.Empty;
                _coaterinputLotCode = string.Empty;
                _coaterExpFlag = string.Empty;
                _coaterWipSeq = string.Empty;

                DataTable dtLotTrace = SelectRollMapLotTrace(_equipmentCode, txtLot.Text, _wipSeq);
                if (CommonVerify.HasTableRow(dtLotTrace))
                {
                    _coaterEquipmentCode = dtLotTrace.Rows[0]["EQPTID"].GetString();
                    _coaterinputLotCode = dtLotTrace.Rows[0]["INPUT_LOTID"].GetString();
                    _coaterExpFlag = dtLotTrace.Rows[0]["EXP_FLAG"].GetString();
                    _coaterWipSeq = dtLotTrace.Rows[0]["WIPSEQ"].GetString();

                    _expFlag = _coaterExpFlag;

                    string adjFlag = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";

                    DataTable dt;

                    if (!string.Equals(_coaterEquipmentCode, _equipmentCode) && !string.Equals(_coaterinputLotCode, _runLotId))
                    {
                        dt = SelectRollMapCoaterLength(_coaterinputLotCode, _coaterWipSeq, adjFlag);
                    }
                    else
                    {
                        dt = SelectRollMapCoaterLength(txtLot.Text, _wipSeq, adjFlag);
                    }


                    string[] petScrap = new string[] { "PET", "SCRAP" };

                    var query = (from t in dt.AsEnumerable()
                                 where t.Field<string>("SMPL_FLAG") != "Y"
                                 && t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                                 && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                 select new { RawEndPosition = t.Field<decimal>("RAW_END_PSTN") }).FirstOrDefault();

                    if (query != null) _xCoaterMaxLength = query.RawEndPosition.GetDouble();
                    else _xCoaterMaxLength = 0;

                    //var query = (from t in dt.AsEnumerable()where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                    //    select new { sourceEndPosition = t.Field<decimal>("SOURCE_END_PSTN") }).FirstOrDefault();
                    //if (query != null) _xCoaterMaxLength = query.sourceEndPosition.GetDouble();
                    //else _xCoaterMaxLength = 0;
                }
                else
                {
                    Util.MessageValidation("SFU9999", "ROLL Length");
                    HiddenLoadingIndicator();
                    InitializeChartByNoChildren(chartCoater, Process.COATING, 10);
                    InitializeChartByNoChildren(chartRollPress, Process.ROLL_PRESSING, 10);
                    EndUpdateChart();
                    return;
                }

                // Max Length 조회
                DataTable dtLength = SelectRollMapLength(txtLot.Text, _wipSeq, rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2");
                if (CommonVerify.HasTableRow(dtLength))
                {
                    var query = (from t in dtLength.AsEnumerable()
                                 where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                                 select new { sourceEndPosition = t.Field<decimal>("SOURCE_END_PSTN") }).FirstOrDefault();
                    if (query != null) maxLength = query.sourceEndPosition.GetDouble();
                    else maxLength = 0;

                    _xMaxLength = maxLength;

                    // RW, UW 조회 및 Display
                    DrawCicleLengthChart(dtLength);
                }

                //chartCoater, chartRollPress 두개의 차트가 비슷한 형태이므로 데이터 바인딩 및 처리를 동시에 진행함.
                string[] processArray = new string[] { "E2000", "E3000" };
                for (int i = 0; i < processArray.Length; i++)
                {
                    string processCode = processArray[i].GetString();
                    string equipmentId = string.Equals(processCode, Process.COATING) ? _coaterEquipmentCode : _equipmentCode;
                    string lotId = string.Equals(processCode, Process.COATING) ? _coaterinputLotCode : txtLot.Text;
                    string wipSequence = string.Equals(processCode, Process.COATING) ? _coaterWipSeq : _wipSeq;
                    string mPosition = "2";                                                                 //측정위치(OVEN 전 1,후 2)	
                    string outLotId = txtLot.Text;                                                          //OUTLOT , SEQ 추가 됨
                    string outSeq = _wipSeq;
                    string sampleFlag;
                    string adjFlag;
                    string mPoint;

                    //string mPoint = rdoTHICK.IsChecked != null && (bool) rdoTHICK.IsChecked ? "1" : "2";    //1: 두께, 2: 웹게이지
                    //string adjFlag = rdoP.IsChecked != null && (bool) rdoP.IsChecked ? "1" : "2";         //1: 상대좌표, 2: 절대좌표
                    //string sampleFlag = "N";                                                              //코터 공정진척에서 호출 시 Y, 롤프레스 는 N 으로 호출

                    if (string.Equals(processCode, Process.COATING))
                    {
                        adjFlag = "1";
                        sampleFlag = "N";
                        mPoint = rdoTHICK.IsChecked != null && (bool)rdoTHICK.IsChecked ? "1" : "2";    //1: 두께, 2: 웹게이지
                    }
                    else
                    {
                        adjFlag = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";
                        sampleFlag = string.Empty;   //롤프레스의 경우 sampleFlag 값이 없으므로 값이 상관없음.
                        mPoint = "1";
                    }

                    DataTable dtGraph = SelectRollMapGraphInfo(lotId
                                                            , wipSequence
                                                            , mPoint
                                                            , mPosition
                                                            , adjFlag
                                                            , processCode
                                                            , equipmentId
                                                            , sampleFlag
                                                            , outLotId
                                                            , outSeq
                                                            );

                    UpdatetCoaterPosition(dtGraph, processCode);

                    if (string.Equals(processCode, Process.COATING))
                        _dtCoaterGraph = dtGraph.Copy();
                    else
                        _dtRollPressGraph = dtGraph.Copy();

                    if (CommonVerify.HasTableRow(dtGraph))
                    {
                        double length = string.Equals(processCode, Process.COATING) ? _xCoaterMaxLength : maxLength;

                        DrawChartBackGround(dtGraph, length, processCode);
                        DrawLineChart(dtGraph, length, processCode);
                        //DrawRollMapLane(dtGraph, processCode);
                        if (string.Equals(processCode, Process.ROLL_PRESSING)) DrawLaneLegend();
                        DrawRollMapLane(processCode);

                        if (string.Equals(processCode, Process.COATING)) DrawMergeLot(dtGraph.Copy());
                    }
                    else
                    {
                        double length = string.Equals(processCode, Process.COATING) ? _xCoaterMaxLength : _xMaxLength;
                        C1Chart c1Chart = string.Equals(processCode, Process.COATING) ? chartCoater : chartRollPress;
                        if (length.Equals(0)) length = 10;
                        InitializeChartByNoChildren(c1Chart, processCode, length);
                    }

                    DataTable dtPoint = SelectRollMapPointInfo(equipmentId
                                                            , lotId
                                                            , wipSequence
                                                            , adjFlag
                                                            , sampleFlag
                                                            , processCode
                                                            , outLotId
                                                            , outSeq);
                    if (CommonVerify.HasTableRow(dtPoint))
                    {
                        if (string.Equals(processCode, Process.COATING))
                            _dtCoaterPoint = dtPoint.Copy();
                        else
                            _dtRollPressPoint = dtPoint.Copy();

                        DrawPointChart(dtPoint, processCode);
                    }

                    // 롤프레스 공정에서의 롤맵 팝업 호출 시 샘플표시는 필요 없음.
                    //DrawRollMapSampleYAxisLine(lotId, wipSequence, adjFlag, processArray[i].GetString());
                    if (string.Equals(processCode, Process.COATING) && !string.Equals(_coaterEquipmentCode, _equipmentCode) && !string.Equals(_coaterinputLotCode, _runLotId))
                    {
                        DrawRollMapStartEndYAxis(_coaterinputLotCode, _coaterWipSeq, adjFlag, processCode);
                    }
                    else
                    {
                        DrawRollMapStartEndYAxis(txtLot.Text, _wipSeq, adjFlag, processCode);
                    }

                    //DrawRollMapScrapYAxisLine(processArray[i].GetString());
                }

                if (chartCoater.Data.Children.Count < 1)
                    InitializeChartByNoChildren(chartCoater, Process.COATING, _xCoaterMaxLength);

                if (chartRollPress.Data.Children.Count < 1)
                    InitializeChartByNoChildren(chartRollPress, Process.ROLL_PRESSING, _xMaxLength);

                // 라인차트 너비를 맞추기 위해 처리
                grdLineAuto.Width = grdRollPress.ColumnDefinitions[4].ActualWidth;
                EndUpdateChart();

            }
            catch (Exception ex)
            {
                EndUpdateChart();
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            CMM_ROLLMAP_DATACOLLECT popupRollMapDataCollect = new CMM_ROLLMAP_DATACOLLECT();
            //CMM_ROLLMAP_HOLD popupRollMapDataCollect = new CMM_ROLLMAP_HOLD();
            popupRollMapDataCollect.FrameOperation = FrameOperation;
            object[] parameters = new object[10];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = _runLotId;
            //parameters[2] = txtLot.Text;
            parameters[3] = _wipSeq;
            parameters[4] = _laneQty;
            //parameters[4] = _equipmentName;

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

        private void btnRollMapOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            CMM_ROLLMAP_OUTSIDE_SCRAP popRollMapOutsideScrap = new CMM_ROLLMAP_OUTSIDE_SCRAP();

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = txtLot.Text;
            parameters[3] = _wipSeq;
            parameters[4] = "OUTSIDE_SCRAP"; //최외각 폐기

            C1WindowExtension.SetParameters(popRollMapOutsideScrap, parameters);
            popRollMapOutsideScrap.Closed += popRollMapOutsideScrap_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapOutsideScrap.ShowModal()));
        }

        private void popRollMapOutsideScrap_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_OUTSIDE_SCRAP popup = sender as CMM_ROLLMAP_OUTSIDE_SCRAP;
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
            //txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
        }

        private void rdoTHICK_Click(object sender, RoutedEventArgs e)
        {
            //txtMeasurement.Text = ObjectDic.Instance.GetObjectName("두께");
        }

        private void btnCoaterRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0, Process.COATING);
        }

        private void btnCoaterZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (chartCoater.View.AxisX.MinScale >= chartCoater.View.AxisX.Scale * 0.75)
                return;

            SetScale(chartCoater.View.AxisX.Scale * 0.75, Process.COATING);
        }

        private void btnCoaterZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartCoater.View.AxisX.Scale * 1.50, Process.COATING);
        }

        private void btnCoaterReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartCoater.BeginUpdate();
            chartCoater.View.AxisX.Reversed = !chartCoater.View.AxisX.Reversed;
            chartCoater.EndUpdate();
        }

        private void btnCoaterReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartCoater.BeginUpdate();
            chartCoater.View.AxisY.Reversed = !chartCoater.View.AxisY.Reversed;
            chartCoater.EndUpdate();
        }

        private void btnRollPressRefresh_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0, Process.ROLL_PRESSING);
        }

        private void btnRollPressZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (chartRollPress.View.AxisX.MinScale >= chartRollPress.View.AxisX.Scale * 0.75)
                return;

            SetScale(chartRollPress.View.AxisX.Scale * 0.75, Process.ROLL_PRESSING);
        }

        private void btnRollPressZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartRollPress.View.AxisX.Scale * 1.50, Process.ROLL_PRESSING);
        }

        private void btnRollPressReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartRollPress.BeginUpdate();
            chartRollPress.View.AxisX.Reversed = !chartRollPress.View.AxisX.Reversed;
            chartRollPress.EndUpdate();
        }

        private void btnRollPressReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartRollPress.BeginUpdate();
            chartRollPress.View.AxisY.Reversed = !chartRollPress.View.AxisY.Reversed;
            chartRollPress.EndUpdate();
        }

        private void DescriptionOnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock?.Tag == null) return;

                ShowLoadingIndicator();
                DoEvents();

                _isResearch = true;
                C1Chart c1Chart;
                DataTable dtDefect, dtPoint, dtGraph;

                if (string.Equals(textBlock.Tag.ToString(), Process.COATING))
                {
                    c1Chart = chartCoater;
                    dtDefect = _dtCoaterDefect;
                    dtPoint = _dtCoaterPoint;
                    dtGraph = _dtCoaterGraph;
                }
                else
                {
                    c1Chart = chartRollPress;
                    dtDefect = _dtRollPressDefect;
                    dtPoint = _dtRollPressPoint;
                    dtGraph = _dtRollPressGraph;
                }

                c1Chart.Data.Children.Clear();

                if (textBlock.Foreground == Brushes.Black)
                {
                    dtDefect.Rows.Add(textBlock.Name, textBlock.Text);
                    textBlock.Foreground = Brushes.LightGray;
                }
                else
                {
                    dtDefect.Select("EQPT_MEASR_PSTN_ID = '" + textBlock.Name + "' And "
                                    + "ABBR_NAME = '" + textBlock.Text + "'").ToList().ForEach(row => row.Delete());
                    dtDefect.AcceptChanges();
                    textBlock.Foreground = Brushes.Black;
                }

                double length = string.Equals(textBlock.Tag.ToString(), Process.COATING) ? _xCoaterMaxLength : _xMaxLength;
                //DrawChartBackGround(dtGraph.Copy(), length, textBlock.Tag.ToString());

                if (CommonVerify.HasTableRow(dtGraph))
                {
                    DrawChartBackGround(dtGraph.Copy(), length, textBlock.Tag.ToString());
                }
                else
                {
                    if (length.Equals(0)) length = 10;
                    InitializeChartByNoChildren(c1Chart, textBlock.Tag.ToString(), length);
                }

                if (dtDefect.Rows.Count < 1)
                {
                    DrawPointChart(dtPoint.Copy(), textBlock.Tag.ToString());
                }
                else
                {
                    var queryPoint = dtPoint.AsEnumerable().Where(ra => !dtDefect.AsEnumerable()
                        .Any(rb => rb.Field<string>("EQPT_MEASR_PSTN_ID") == ra.Field<string>("EQPT_MEASR_PSTN_ID") && rb.Field<string>("ABBR_NAME") == ra.Field<string>("ABBR_NAME")));
                    if (queryPoint.Any())
                    {
                        DrawPointChart(queryPoint.CopyToDataTable(), textBlock.Tag.ToString());
                    }
                }

                string adjFlag = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";           //1: 상대좌표, 2: 절대좌표

                if (string.Equals(textBlock.Tag.ToString(), Process.COATING))
                {
                    if (!string.Equals(_coaterEquipmentCode, _equipmentCode) && !string.Equals(_coaterinputLotCode, _runLotId))
                        DrawRollMapStartEndYAxis(_coaterinputLotCode, _coaterWipSeq, adjFlag, textBlock.Tag.ToString());
                    else
                        DrawRollMapStartEndYAxis(txtLot.Text, _wipSeq, adjFlag, textBlock.Tag.ToString());

                }
                else
                {
                    DrawRollMapStartEndYAxis(txtLot.Text, _wipSeq, adjFlag, textBlock.Tag.ToString());
                }
                DrawRollMapLane(textBlock.Tag.ToString());
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*
            TextBlock textBlock = sender as TextBlock;
            if (textBlock?.Tag == null) return;

            string[] splitItem = textBlock.Tag.GetString().Split(';');
            string equipmentMeasurementCode = splitItem[0];
            string startPosition = splitItem[1];
            string endPosition = splitItem[2];
            string collectSeq = splitItem[3];
            string collectType = splitItem[4];
            string length = splitItem[5];

            CMM_ROLLMAP_RW_SCRAP popRollMapRewinderScrap = new CMM_ROLLMAP_RW_SCRAP();
            popRollMapRewinderScrap.FrameOperation = FrameOperation;

            object[] parameters = new object[11];
            parameters[0] = txtLot.Text;
            parameters[1] = _wipSeq;
            parameters[2] = _processCode;
            parameters[3] = _equipmentCode;
            parameters[4] = equipmentMeasurementCode;
            parameters[5] = startPosition;
            parameters[6] = endPosition;
            parameters[7] = length;
            parameters[8] = collectSeq;
            parameters[9] = collectType;
            parameters[10] = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";

            C1WindowExtension.SetParameters(popRollMapRewinderScrap, parameters);
            popRollMapRewinderScrap.Closed += popRollMapRewinderScrap_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapRewinderScrap.ShowModal()));
            */
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

            chartCoater.View.AxisX.ScrollBar = new AxisScrollBar();
            chartRollPress.View.AxisX.ScrollBar = new AxisScrollBar();

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

            _dtCoaterDefect = new DataTable();
            _dtCoaterDefect.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtCoaterDefect.Columns.Add("ABBR_NAME", typeof(string));
            _dtRollPressDefect = _dtCoaterDefect.Copy();

            _dtLaneLegend = new DataTable();
            _dtLaneLegend.Columns.Add("LANE_NO", typeof(string));
            _dtLaneLegend.Columns.Add("COLORMAP", typeof(string));
            _dtLaneLegend.Rows.Add("1", "#FFFFD700");   //Gold
            _dtLaneLegend.Rows.Add("2", "#FFFF4500");   //OrangeRed
            _dtLaneLegend.Rows.Add("3", "#FF87CEEB");   //SkyBlue   
            _dtLaneLegend.Rows.Add("4", "#FF00FF00");   //Lime

            _dtLane = new DataTable();
            _dtLane.Columns.Add("LANE_NO", typeof(string));
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


            if (c1Chart.Name == "chartRollPress" || c1Chart.Name == "chartCoater")
            {
                double majorUnit;
                double length = c1Chart.Name == "chartRollPress" ? _xMaxLength : _xCoaterMaxLength;

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

        /// <summary>
        /// 무지부 표현
        /// </summary>
        /// <param name="xLength"></param>
        /// <param name="processCode"></param>
        private void InitializePointChart(double xLength, string processCode)
        {
            C1Chart c1Chart = string.Equals(processCode, Process.COATING) ? chartCoater : chartRollPress;

            string equipmentCode, lotCode;

            if (string.Equals(processCode, Process.COATING))
            {
                equipmentCode = _coaterEquipmentCode;
                lotCode = _coaterinputLotCode;
            }
            else
            {
                equipmentCode = _equipmentCode;
                lotCode = _runLotId;
            }

            DataTable dt = SelectRollMapGplmWidth(processCode, equipmentCode, lotCode);

            if (CommonVerify.HasTableRow(dt))
            {
                DataRow[] dr = dt.Select();
                int drLength = dr.Length;
                decimal sumLength = dt.AsEnumerable().Sum(s => s.Field<decimal>("LOV_VALUE"));
                var convertFromString = ColorConverter.ConvertFromString("#D5D5D5");

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
                                c1Chart.Data.Children.Insert(0, alarmZone);
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
                                    c1Chart.Data.Children.Insert(0, alarmZone);
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
                                    c1Chart.Data.Children.Insert(0, alarmZone);
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
                                c1Chart.Data.Children.Insert(0, alarmZone);
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
                                    c1Chart.Data.Children.Insert(0, alarmZone);
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
                                    c1Chart.Data.Children.Insert(0, alarmZone);
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
            var convertFromString = ColorConverter.ConvertFromString("#D5D5D5");
            AlarmZone alarmZone1 = new AlarmZone
            {
                Near = 0,
                Far = xLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 220,
                UpperExtent = 223,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            c1Chart.Data.Children.Insert(0, alarmZone1);

            // Top 무지부 하단
            AlarmZone alarmZone2 = new AlarmZone
            {
                Near = 0,
                Far = xLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 120,
                UpperExtent = 120 - 2.5,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            c1Chart.Data.Children.Insert(0, alarmZone2);


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
            c1Chart.Data.Children.Insert(0, alarmZone3);

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
            c1Chart.Data.Children.Insert(0, alarmZone4);
            */
        }

        private void InitializeControls()
        {

            if (grdCoaterDefectTopLegend.ColumnDefinitions.Count > 0) grdCoaterDefectTopLegend.ColumnDefinitions.Clear();
            if (grdCoaterDefectTopLegend.RowDefinitions.Count > 0) grdCoaterDefectTopLegend.RowDefinitions.Clear();
            grdCoaterDefectTopLegend.Children.Clear();

            if (grdCoaterDefectBackLegend.ColumnDefinitions.Count > 0) grdCoaterDefectBackLegend.ColumnDefinitions.Clear();
            if (grdCoaterDefectBackLegend.RowDefinitions.Count > 0) grdCoaterDefectBackLegend.RowDefinitions.Clear();
            grdCoaterDefectBackLegend.Children.Clear();

            if (grdRollPressDefectTopLegend.ColumnDefinitions.Count > 0) grdRollPressDefectTopLegend.ColumnDefinitions.Clear();
            if (grdRollPressDefectTopLegend.RowDefinitions.Count > 0) grdRollPressDefectTopLegend.RowDefinitions.Clear();
            grdRollPressDefectTopLegend.Children.Clear();

            if (grdRollPressDefectBackLegend.ColumnDefinitions.Count > 0) grdRollPressDefectBackLegend.ColumnDefinitions.Clear();
            if (grdRollPressDefectBackLegend.RowDefinitions.Count > 0) grdRollPressDefectBackLegend.RowDefinitions.Clear();
            grdRollPressDefectBackLegend.Children.Clear();

            if (grdLineLegend.ColumnDefinitions.Count > 0) grdLineLegend.ColumnDefinitions.Clear();
            if (grdLineLegend.RowDefinitions.Count > 0) grdLineLegend.RowDefinitions.Clear();
            grdLineLegend.Children.Clear();

            BeginUpdateChart();
        }

        private void InitializeChartByNoChildren(C1Chart c1Chart, string processCode, double maxLength)
        {
            c1Chart.View.AxisX.Min = 0;
            c1Chart.View.AxisY.Min = -80;   //Back 하단의 태그 표시로 인하여 AxisY Min 값을 설정 함.
            c1Chart.View.AxisX.Max = maxLength;
            c1Chart.View.AxisY.Max = 220 + 3;   //무지부 3
            InitializePointChart(maxLength, processCode);
            InitializeChartView(c1Chart);
        }

        private void GetLegend()
        {
            grdLegend.Children.Clear();

            DataTable dt = _dtLineLegend.Copy();

            if (rdoA.IsChecked == true)
            {
                dt.Rows.Add(1, "#FAED7D", ObjectDic.Instance.GetObjectName("QA샘플"), "LOAD", "RECT");
                dt.Rows.Add(1, "#00D8FF", ObjectDic.Instance.GetObjectName("자주검사"), "LOAD", "RECT");
                dt.Rows.Add(1, "#FFA7A7", ObjectDic.Instance.GetObjectName("최외각 폐기"), "LOAD", "RECT");
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
                        //FontWeight = FontWeights.Bold,
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

                //string[] legendArray = new string[] { ObjectDic.Instance.GetObjectName("이음매"), ObjectDic.Instance.GetObjectName("SCRAP") };

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

                    //var convertFromString = string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("이음매")) ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")) ;
                    Polygon polygon = new Polygon()
                    {
                        Points = pointCollection,
                        Width = 13,
                        Height = 15,
                        Stretch = Stretch.Fill,
                        StrokeThickness = 1,
                        //Fill = string.Equals(legendArray[i].GetString(), ObjectDic.Instance.GetObjectName("이음매")) ? Brushes.White : Brushes.Black,
                        //Stroke = Brushes.Black
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


        private static DataTable SelectRollMapLength(string lotId, string wipSeq, string adjFlag)
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_LENGTH_RP";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("SET_WIDTH", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["SET_WIDTH"] = 800;
            dr["ADJFLAG"] = adjFlag;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private static DataTable SelectRollMapLength(string lotId, string wipSeq, string adjFlag, string sampleFlag)
        {
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_LENGTH_CT";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;
            inTable.Rows.Add(dr);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inTable);
            //string xml = ds.GetXml();

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private static DataTable SelectRollMapCoaterLength(string lotId, string wipSeq, string adjFlag)
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_CT_HEAD";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["ADJFLAG"] = adjFlag;
            inTable.Rows.Add(dr);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inTable);
            //string xml = ds.GetXml();

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private DataTable SelectRollMapGplmWidth(string processCode, string equipmentCode, string lotCode)
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_GPLM_WIDTH";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["PROCID"] = processCode;
            dr["EQPTID"] = equipmentCode;
            dr["LOTID"] = lotCode;
            inTable.Rows.Add(dr);

            DataTable dtLaneInfo = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            /* BizRule(DA_PRD_SEL_ROLLMAP_GPLM_WIDTH ) 변경에 따른 데이터 강제 테스트 
            DataTable dtLaneInfo = new DataTable();
            dtLaneInfo.Columns.Add("LOC2", typeof(string));
            dtLaneInfo.Columns.Add("LOV_VALUE", typeof(decimal));
            dtLaneInfo.Columns.Add("LANE_NO_COT", typeof(string));
            dtLaneInfo.Columns.Add("LANE_NO_CUR", typeof(string));
            dtLaneInfo.Columns.Add("LANE_NO", typeof(string));
            dtLaneInfo.Columns.Add("RN", typeof(int));
            dtLaneInfo.Columns.Add("ANODE_CATHODE", typeof(string));
            dtLaneInfo.Columns.Add("LINE_BOTTOM", typeof(decimal));

            dtLaneInfo.Rows.Add("J", 18, null, null, null, 9, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("I", 285, "4", "2", "4", 8, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("H", 18, null, null, null, 7, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("G", 285, "3", "1", "3", 6, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("H", 18, null, null, null, 5, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("G", 285, "3", "1", "2", 4, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("F", 15, null, null, null, 3, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("G", 285, "3", "1", "1", 2, "CATHODE", 1227);
            dtLaneInfo.Rows.Add("F", 15, null, null, null, 1, "CATHODE", 1227);
            */

            dtLaneInfo.Columns.Add("LANE_LENGTH", typeof(double));
            dtLaneInfo.Columns.Add("Y_STRT_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("Y_END_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("Y_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("LANE_LENGTH_SUM", typeof(double));

            if (CommonVerify.HasTableRow(dtLaneInfo))
            {
                //Y 좌표 계산
                dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderByDescending(o => o.Field<int>("RN")).CopyToDataTable();

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

                        //if (i != 0)
                        //    yStartLength = yStartLength + x;

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

            if (CommonVerify.HasTableRow(dtLaneInfo)) dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderBy(o => o.Field<int>("RN")).CopyToDataTable();

            if (string.Equals(processCode, Process.COATING))
            {
                _dtCoaterLaneInfo = dtLaneInfo;
            }
            else
            {
                _dtRollPressLaneInfo = dtLaneInfo;
            }

            if (string.Equals(processCode, Process.ROLL_PRESSING))
            {
                _dtLane?.Clear();
                for (int i = 0; i < _dtRollPressLaneInfo.Rows.Count; i++)
                {
                    DataRow newRow = _dtLane.NewRow();

                    if (!string.IsNullOrEmpty(_dtRollPressLaneInfo.Rows[i]["LANE_NO"].GetString()))
                    {
                        newRow["LANE_NO"] = _dtRollPressLaneInfo.Rows[i]["LANE_NO"];
                        _dtLane.Rows.Add(newRow);
                    }
                }
            }

            return dtLaneInfo;
        }

        private static DataTable SelectRollMapLotTrace(string equipmentCode, string lotId, string wipSeq)
        {
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = equipmentCode;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_EXP_TRACE", "RQSTDT", "RSLTDT", inTable);
        }

        private static DataTable SelectRollMapGraphInfo(string lotId, string wipSeq, string mPoint, string position, string adjFlag, string processCode, string equipmentCode, string sampleFlag, string outLotId = null, string outSeq = null)
        {
            string bizRuleName = string.Equals(processCode, Process.COATING) ? "BR_PRD_SEL_ROLLMAP_CT_CHART" : "DA_PRD_SEL_ROLLMAP_RP_CHART";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("MPOINT", typeof(string));
            inTable.Columns.Add("MPSTN", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));

            if (string.Equals(processCode, Process.COATING))
            {
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("OUT_WIPSEQ", typeof(decimal));
            }

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["MPOINT"] = mPoint;
            dr["MPSTN"] = position;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;
            dr["EQPTID"] = equipmentCode;

            if (string.Equals(processCode, Process.COATING))
            {
                dr["OUT_LOTID"] = outLotId;
                dr["OUT_WIPSEQ"] = outSeq;
            }
            inTable.Rows.Add(dr);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inTable);
            //string xml = ds.GetXml();

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private static DataTable SelectRollMapPointInfo(string equipmentCode, string lotId, string wipSeq, string adjFlag, string sampleFlag, string processCode, string outLotId = null, string outSeq = null)
        {
            string bizRuleName = string.Equals(processCode, Process.COATING) ? "BR_PRD_SEL_ROLLMAP_CT_DEFECT_CHART" : "DA_PRD_SEL_ROLLMAP_RP_DEFECT_CHART";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));

            if (string.Equals(processCode, Process.COATING))
            {
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("OUT_WIPSEQ", typeof(string));
            }

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["EQPTID"] = equipmentCode;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;

            if (string.Equals(processCode, Process.COATING))
            {
                dr["OUT_LOTID"] = outLotId;
                dr["OUT_WIPSEQ"] = outSeq;
            }
            inTable.Rows.Add(dr);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inTable);
            //string xml = ds.GetXml();

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private void DrawCicleLengthChart(DataTable dtLength)
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
            chart.View.AxisX.Min = dtLength.AsEnumerable().ToList().Min(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
            chart.View.AxisX.Max = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();

            DataTable dt = new DataTable();
            dt.Columns.Add("SOURCE_END_PSTN", typeof(double));
            dt.Columns.Add("ARROW_PSTN", typeof(double));
            dt.Columns.Add("SOURCE_Y_PSTN", typeof(double));
            dt.Columns.Add("ARROW_Y_PSTN", typeof(double));
            dt.Columns.Add("COLORMAP", typeof(string));
            dt.Columns.Add("CIRCLENAME", typeof(string));
            dt.Columns.Add("WND_DIRCTN", typeof(string));


            double arrowValue = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble() * 0.025;

            for (int i = 0; i < dtLength.Rows.Count; i++)
            {
                double arrowYPosition = dtLength.Rows[i]["WND_DIRCTN"].GetInt() == 1 ? 60 : 0;
                double arrowPosition;

                if (dtLength.Rows[i]["EQPT_MEASR_PSTN_ID"].GetString() == "RW")
                {
                    arrowPosition = dtLength.Rows[i]["SOURCE_END_PSTN"].GetDouble() - arrowValue;
                }
                else
                {
                    arrowPosition = dtLength.Rows[i]["SOURCE_END_PSTN"].GetDouble() + arrowValue;
                }

                DataRow dr = dt.NewRow();
                dr["SOURCE_END_PSTN"] = dtLength.Rows[i]["SOURCE_END_PSTN"];
                dr["ARROW_PSTN"] = arrowPosition;
                dr["SOURCE_Y_PSTN"] = 0;
                dr["ARROW_Y_PSTN"] = arrowYPosition;
                dr["CIRCLENAME"] = dtLength.Rows[i]["EQPT_MEASR_PSTN_ID"];
                dr["COLORMAP"] = "#000000";
                dt.Rows.Add(dr);
            }

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

        private void DrawPointChart(DataTable dtPoint, string processCode)
        {
            if (CommonVerify.HasTableRow(dtPoint))
            {
                //var queryTop = dtPoint.AsEnumerable().Where(o => o.Field<Int32>("SEQ").Equals(topIndex) && o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")).ToList();
                var queryTop = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")).ToList();
                DataTable dtTop = queryTop.Any() ? MakeTableForDisplay(queryTop.CopyToDataTable(), ChartDisplayType.SurfaceTop, processCode) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.SurfaceTop, processCode);

                var queryBack = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")).ToList();
                DataTable dtBack = queryBack.Any() ? MakeTableForDisplay(queryBack.CopyToDataTable(), ChartDisplayType.SurfaceBack, processCode) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.SurfaceBack, processCode);

                if (CommonVerify.HasTableRow(dtTop))
                {
                    dtTop.TableName = "dtTop";
                    DrawRollMapDefect(dtTop, processCode);
                }
                if (CommonVerify.HasTableRow(dtBack))
                {
                    dtBack.TableName = "dtBack";
                    DrawRollMapDefect(dtBack, processCode);
                }

                var queryVisionTop = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP") ||
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")).ToList();
                //DataTable dtVisionTop = queryVisionTop.Any() ? queryVisionTop.CopyToDataTable() : dtPoint.Clone();
                DataTable dtVisionTop = queryVisionTop.Any() ? MakeTableForDisplay(queryVisionTop.CopyToDataTable(), ChartDisplayType.TagVisionTop, processCode) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.TagVisionTop, processCode);

                var queryVisionBack = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK") ||
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")).ToList();
                DataTable dtVisionBack = queryVisionBack.Any() ? MakeTableForDisplay(queryVisionBack.CopyToDataTable(), ChartDisplayType.TagVisionBack, processCode) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.TagVisionBack, processCode);

                if (CommonVerify.HasTableRow(dtVisionTop))
                {
                    dtVisionTop.TableName = "dtTop";
                    DrawRollMapVision(dtVisionTop, processCode);
                }

                if (CommonVerify.HasTableRow(dtVisionBack))
                {
                    dtVisionBack.TableName = "dtBack";
                    DrawRollMapVision(dtVisionBack, processCode);
                }

                var queryMark = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("MARK")).ToList();
                DataTable dtMark = queryMark.Any() ? queryMark.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtMark))
                {
                    DrawRollMapTagYAxisLineLabel(dtMark, processCode);
                }

                var queryTagSection = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawRollMapTagSection(dtTagSection, processCode);
                }

                // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
                var queryTagSectionLane = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
                DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dtPoint.Clone();
                dtTagSectionLane.TableName = "TAG_SECTION_LANE";
                //_dtTagSection = dtTagSectionLane;

                if (CommonVerify.HasTableRow(dtTagSectionLane))
                {
                    DrawRollMapTagSection(dtTagSectionLane, processCode);
                }

                var queryTagSpot = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    DrawRollMapTagSpot(dtTagSpot, processCode);
                }

                var queryOverLayVision = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
                DataTable dtOverLayVision = queryOverLayVision.Any() ? MakeTableForDisplay(queryOverLayVision.CopyToDataTable(), ChartDisplayType.OverLayVision) : MakeTableForDisplay(dtPoint.Clone(), ChartDisplayType.OverLayVision);

                if (CommonVerify.HasTableRow(dtOverLayVision) && string.Equals(processCode, Process.ROLL_PRESSING))
                {
                    DrawRollMapVision(dtOverLayVision, processCode);
                }

                var queryRewinderScrap = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("RW_SCRAP")).ToList();
                DataTable dtRwScrap = queryRewinderScrap.Any() ? queryRewinderScrap.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtRwScrap))
                {
                    DrawRollMapRewinderScrap(dtRwScrap, processCode);
                }


                var queryPointTopLegend = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")).ToList();
                DataTable dtPointTopLegend = queryPointTopLegend.Any() ? queryPointTopLegend.CopyToDataTable() : dtPoint.Clone();

                var queryPointBackLegend = dtPoint.AsEnumerable().Where(o =>
                    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
                DataTable dtPointBackLegend = queryPointBackLegend.Any() ? queryPointBackLegend.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtPointTopLegend))
                {
                    dtPointTopLegend.TableName = "TopLegend";
                    DrawPointLegend(dtPointTopLegend, processCode);
                }
                if (CommonVerify.HasTableRow(dtPointBackLegend))
                {
                    dtPointBackLegend.TableName = "BackLegend";
                    DrawPointLegend(dtPointBackLegend, processCode);
                }
            }
        }

        private void DrawRollMapDefect(DataTable dt, string processCode)
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

                    var convertFromString = ColorConverter.ConvertFromString(DataTableConverter.GetValue(dp.DataObject, "COLORMAP").GetString());
                    pe.Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    pe.Stroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    double xposition;
                    double yposition;
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_X_PSTN")), out xposition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_Y_PSTN")), out yposition);
                    string defectName = DataTableConverter.GetValue(dp.DataObject, "ABBR_NAME").GetString();

                    //string content = defectName + " [X:" + $"{xposition:###,###,###,##0.##}" + "m" + ", Y:" + $"{yposition:###,###,###,##0.##}" + "mm" + "]";
                    string content = defectName + " [" + ObjectDic.Instance.GetObjectName("길이") + ":" + $"{xposition:###,###,###,##0.##}" + "m" + ", " + ObjectDic.Instance.GetObjectName("폭") + ":" + $"{yposition:###,###,###,##0.##}" + "mm" + "]";
                    ToolTipService.SetToolTip(pe, content);
                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                    ToolTipService.SetShowDuration(pe, 60000);
                }
            };

            if (string.Equals(processCode, Process.COATING))
                chartCoater.Data.Children.Add(ds);
            else
                chartRollPress.Data.Children.Add(ds);

        }

        private void DrawRollMapVision(DataTable dt, string processCode)
        {
            if (!CommonVerify.HasTableRow(dt)) return;

            foreach (DataRow row in dt.Rows)
            {
                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

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


                if (string.Equals(processCode, Process.COATING))
                    chartCoater.Data.Children.Add(alarmZone);
                else
                    chartRollPress.Data.Children.Add(alarmZone);

            }
        }

        private void DrawRollMapRewinderScrap(DataTable dtRwScrap, string processCode)
        {
            if (string.Equals(processCode, Process.COATING)) return;

            if (CommonVerify.HasTableRow(dtRwScrap))
            {
                dtRwScrap.Columns.Add("TOOLTIP", typeof(string));
                dtRwScrap.Columns.Add("TAG", typeof(string));
                dtRwScrap.Columns.Add("Y_PSTN", typeof(double));

                double lowerExtent = 0;
                double upperExtent = 220;

                foreach (DataRow row in dtRwScrap.Rows)
                {
                    var convertFromString = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row["COLORMAP"].GetString()));
                    string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["SOURCE_STRT_PSTN"].GetString() + ";" + row["SOURCE_END_PSTN"].GetString() + ";" + row["ADJ_LOTID"].GetString() + ";" + row["ADJ_WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
                    string toolTip = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + " ]";

                    row["TOOLTIP"] = toolTip;
                    row["TAG"] = tag;
                    row["Y_PSTN"] = 213;

                    chartRollPress.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["SOURCE_END_PSTN"], row["SOURCE_END_PSTN"] },
                        ValuesSource = new double[] { lowerExtent, upperExtent },
                        ConnectionStroke = convertFromString,
                        ConnectionStrokeThickness = 3,
                        ConnectionFill = convertFromString,
                    });
                }

                XYDataSeries dsPetScrap = new XYDataSeries();
                dsPetScrap.ItemsSource = DataTableConverter.Convert(dtRwScrap);
                dsPetScrap.XValueBinding = new Binding("SOURCE_END_PSTN");
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

                chartRollPress.Data.Children.Add(dsPetScrap);

            }
        }

        private void DrawRollMapTagYAxisLineLabel(DataTable dt, string processCode)
        {
            C1Chart c1Chart = string.Equals(processCode, Process.COATING) ? chartCoater : chartRollPress;
            // 종축 라인 설정 TOP, BACK 동일 함.
            DataRow[] drMark = dt.Select();
            if (drMark.Length > 0)
            {
                foreach (DataRow row in drMark)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_STRT_PSTN"]), out sourceStartPosition);
                    string content = row["CMCDNAME"] + "[" + Util.NVC(row["ROLLMAP_CLCT_TYPE"]) + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";

                    c1Chart.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["ADJ_STRT_PSTN"].GetDouble(), row["ADJ_STRT_PSTN"].GetDouble() },
                        ValuesSource = new double[] { -20, 220 },
                        ConnectionStroke = new SolidColorBrush(Colors.Black),
                        ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                        ToolTip = content,
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
                // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
            };
            c1Chart.Data.Children.Add(dsMarkLabel);
        }

        private void DrawRollMapTagSection(DataTable dtTagPosition, string processCode)
        {
            C1Chart c1Chart = string.Equals(processCode, Process.COATING) ? chartCoater : chartRollPress;

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
                    c1Chart.Data.Children.Add(ds);
                }
            }
        }

        private void DrawRollMapTagSpot(DataTable dtTagSpot, string processCode)
        {
            // 롤프레스공정에만 해당
            if (CommonVerify.HasTableRow(dtTagSpot))
            {
                dtTagSpot = MakeTableForDisplay(dtTagSpot, ChartDisplayType.TagSpot);

                C1Chart c1Chart = string.Equals(processCode, Process.COATING) ? chartCoater : chartRollPress;

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
                c1Chart.Data.Children.Add(ds);
            }
        }

        private void DrawRollMapStartEndYAxis(string lotId, string wipSeq, string adjFlag, string processCode)
        {
            string bizRuleName;

            if (string.Equals(processCode, Process.COATING))
                bizRuleName = "DA_PRD_SEL_ROLLMAP_CT_HEAD";
            else
                bizRuleName = "DA_PRD_SEL_ROLLMAP_RP_HEAD";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["ADJFLAG"] = adjFlag;
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
                if (queryLength.Any())
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
                    ds.PointLabelTemplate = grdMain.Resources["chartLength"] as DataTemplate;
                    ds.Margin = new Thickness(0, 0, 0, 0);
                    ds.Cursor = Cursors.Hand;

                    ds.PlotElementLoaded += (s, e) =>
                    {
                        PlotElement pe = (PlotElement)s;
                        pe.Stroke = new SolidColorBrush(Colors.Transparent);
                        pe.Fill = new SolidColorBrush(Colors.Transparent);
                    };

                    if (string.Equals(processCode, Process.COATING))
                        chartCoater.Data.Children.Add(ds);
                    else
                        chartRollPress.Data.Children.Add(ds);
                }
            }

            // 롤프레스 공정에서만 호출 함. 이음매(PET), SCRAP 표현 
            var queryPetScrap = (from t in dtSample.AsEnumerable() where petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
            if (queryPetScrap.Any())
            {
                DrawRollMapScrapPetYAxisLine(processCode, dtSample);
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
                var convertFromString = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDBDBD"));
                string colorMap = string.Equals(row["EQPT_MEASR_PSTN_ID"].GetString(), "SCRAP") ? "#000000" : "#BDBDBD";
                string content = "[" + row["DFCT_NAME"].GetString() + "] " + $"{row["RAW_STRT_PSTN"]:###,###,###,##0.##}" + "m";
                string tag = row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["RAW_STRT_PSTN"].GetString() + ";" + row["RAW_END_PSTN"].GetString()
                      + ";" + row["LOTID"].GetString() + ";" + row["WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                chartRollPress.Data.Children.Add(new XYDataSeries()
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

                chartRollPress.Data.Children.Add(dsPetScrap);
            }
        }

        private void DrawChartBackGround(DataTable dt, double xLength, string processCode)
        {
            try
            {
                if (CommonVerify.HasTableRow(dt))
                {
                    C1Chart c1Chart;
                    DataTable dtLaneInfo;

                    var maxLength = dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble() > xLength
                        ? dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble()
                        : xLength;

                    InitializePointChart(maxLength, processCode);

                    if (string.Equals(processCode, Process.COATING))
                    {
                        c1Chart = chartCoater;
                        dtLaneInfo = _dtCoaterLaneInfo;
                        _xCoaterMaxLength = maxLength;
                    }
                    else
                    {
                        c1Chart = chartRollPress;
                        dtLaneInfo = _dtRollPressLaneInfo;
                        _xMaxLength = maxLength;
                    }

                    c1Chart.View.AxisX.Min = 0;
                    c1Chart.View.AxisY.Min = -80;
                    c1Chart.View.AxisX.Max = maxLength;
                    c1Chart.View.AxisY.Max = 220 + 3;   //무지부 3

                    int rowIndex = string.Equals(_processCode, Process.COATING) ? 1 : 2;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        double yStartPosition = 0, yEndPosition = 0;

                        string adjLaneNo = dt.Rows[i]["ADJ_LANE_NO"].GetString();

                        var query = (from t in dtLaneInfo.AsEnumerable()
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

                            if (string.Equals(processCode, Process.COATING))
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

                            var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dt.Rows[i]["COLORMAP"]));
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
                                                     "Scan AVG : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                                                     "ColorMap : " + Util.NVC(dt.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                                     "Offset : " + Util.NVC(dt.Rows[x]["SCAN_OFFSET"]);

                                    ToolTipService.SetToolTip(pe, content);
                                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                    ToolTipService.SetShowDuration(pe, 60000);
                                }
                            };
                            c1Chart.Data.Children.Add(alarmZone);
                        }
                    }

                    InitializeChartView(c1Chart);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DrawRollMapLane(string processCode)
        {

            DataTable dtLaneInfo = string.Equals(processCode, Process.COATING) ? _dtCoaterLaneInfo : _dtRollPressLaneInfo;

            if (CommonVerify.HasTableRow(dtLaneInfo))
            {
                DataTable dtLane = new DataTable();
                dtLane.Columns.Add("SOURCE_STRT_PSTN", typeof(string));
                dtLane.Columns.Add("SOURCE_Y_PSTN", typeof(string));
                dtLane.Columns.Add("LANEINFO", typeof(string));

                double startPosition;

                if (string.Equals(processCode, Process.COATING))
                    startPosition = _xCoaterMaxLength - (_xCoaterMaxLength * 0.01);
                else
                    startPosition = _xMaxLength - (_xMaxLength * 0.01);

                string[] typeArray = { "Top", "Back" };

                for (int i = 0; i < typeArray.Length; i++)
                {
                    string typeCode = typeArray[i].GetString();

                    for (int j = 0; j < dtLaneInfo.Rows.Count; j++)
                    {
                        if (string.IsNullOrEmpty(dtLaneInfo.Rows[j]["LANE_NO"].GetString()))
                            continue;

                        DataRow drLane = dtLane.NewRow();
                        drLane["SOURCE_STRT_PSTN"] = startPosition;
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
                if (string.Equals(processCode, Process.COATING))
                    chartCoater.Data.Children.Add(dsLane);
                else
                    chartRollPress.Data.Children.Add(dsLane);
            }
        }

        private void DrawLineChart(DataTable dt, double xLength, string processCode)
        {
            if (string.Equals(processCode, Process.COATING)) return;

            try
            {
                double maxLength = dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble() > xLength
                    ? dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble()
                    : xLength;

                chartLine.View.AxisX.Min = 0;
                chartLine.View.AxisX.Max = maxLength;

                chartLine.ChartType = ChartType.Line;

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
                    DrawLineLegend(dt.Copy(), _dtLineLegend.Copy());

                    DataTable dtLineLegend = _dtLineLegend.Select().AsEnumerable().OrderByDescending(o => o.Field<int>("NO")).CopyToDataTable();
                    DataRow[] newRows = dtLineLegend.Select();

                    var queryLaneInfo = (from t in _dtRollPressLaneInfo.AsEnumerable()
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
                        //if (!dt.AsEnumerable().Any(p => p.Field<string>("SCAN_COLRMAP") != null && p.Field<string>("SCAN_COLRMAP").Equals(row["VALUE"].GetString()))) continue;
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

                            chartLine.Data.Children.Add(dsLegend);
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
                            //ds.SymbolStroke = laneConvertFromString != null ? new SolidColorBrush((Color)laneConvertFromString) : null;
                            ds.Cursor = Cursors.Hand;
                            ds.SymbolSize = new Size(7, 7);

                            chartLine.Data.Children.Add(ds);

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
                                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_STRT_PSTN")), out sourceStart);
                                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_END_PSTN")), out sourceEnd);

                                    string content = rdoTHICK.IsChecked != null && (bool)rdoTHICK.IsChecked ? "Thickness : " : "Load : ";
                                    content = content + $"{scanAvgValue:###,###,###,##0.##}" + "[" + $"{sourceStart:###,###,###,##0.##}" + "~" + $"{sourceEnd:###,###,###,##0.##}" + "]";

                                    ToolTipService.SetToolTip(pe, content);
                                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                    ToolTipService.SetShowDuration(pe, 60000);
                                }
                            };
                        }
                    }
                }

                foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdLine))
                {
                    InitializeChartView(c1Chart);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DrawPointLegend(DataTable dt, string processCode)
        {
            if (_isResearch) return;

            try
            {
                Grid gridLegend;

                if (string.Equals(processCode, Process.COATING))
                {
                    if (_sideType == "B")
                    {
                        gridLegend = dt.TableName == "BackLegend" ? grdCoaterDefectTopLegend : grdCoaterDefectBackLegend;
                    }
                    else
                    {
                        gridLegend = dt.TableName == "TopLegend" ? grdCoaterDefectTopLegend : grdCoaterDefectBackLegend;
                    }
                }
                else
                {
                    gridLegend = dt.TableName == "TopLegend" ? grdRollPressDefectTopLegend : grdRollPressDefectBackLegend;
                }

                if (CommonVerify.HasTableRow(dt))
                {
                    if (gridLegend.ColumnDefinitions.Count > 0) gridLegend.ColumnDefinitions.Clear();
                    if (gridLegend.RowDefinitions.Count > 0) gridLegend.RowDefinitions.Clear();

                    for (int x = 0; x < 2; x++)
                    {
                        ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                        gridLegend.ColumnDefinitions.Add(gridColumn);
                    }

                    gridLegend.Children.Clear();

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
                                                                                      processCode);
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
                                                                                     processCode);
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

        private void DrawLineLegend(DataTable dt, DataTable dtLegend)
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
                        //double agvValue = dt.AsEnumerable().ToList().Max(r => (decimal?)r[valueText]).GetDouble();
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

            var query = (from t in _dtRollPressLaneInfo.AsEnumerable()
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
                         select new
                         {
                             AdjustmentLotId = t.Field<string>("ADJ_LOTID")
                         }).Distinct().ToList();

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
                    chartCoater.Data.Children.Add(ds);
                }
            }
        }

        private void DrawScrap(DataTable dt)
        {

        }

        private void TextBlockLane_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null) return;

                ShowLoadingIndicator();
                DoEvents();

                _isResearch = true;
                chartLine.Data.Children.Clear();

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

                DrawLineChart(_dtRollPressGraph.Copy(), _xMaxLength, Process.ROLL_PRESSING);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private IEnumerable<double> GetLineValuesSource(DataTable dt, string value)
        {
            try
            {
                var queryCount = _xMaxLength.GetInt() + 1;
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

        private void UpdatetCoaterPosition(DataTable dt, string processCode)
        {
            if (!string.Equals(processCode, Process.COATING)) return;

            try
            {
                var queryGraph = dt.AsEnumerable().Where(o => o.Field<short>("SEQ").Equals(1) && o.Field<string>("SIDE").Equals("B")).ToList();
                if (queryGraph.Any())
                {
                    txtCoaterPosition1.Text = "B";
                    txtCoaterPosition2.Text = "T";
                    _sideType = "B";

                }
                else
                {
                    txtCoaterPosition1.Text = "T";
                    txtCoaterPosition2.Text = "B";
                    _sideType = "T";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static double GetYValue(double maxValue, double minValue, double targetValue)
        {
            int value = (maxValue > 600) ? 600 : 0;
            return ((targetValue - value) * 100) / 600;
            /*
            //유지부 폭 정보 567
            if (maxValue < 567) maxValue = 567;
            //((input - min) * 100) / (max - min)
            return ((targetValue - minValue) * 100) / (maxValue - minValue);
            */
        }

        private void SetScale(double scale, string processCode)
        {
            C1Chart c1Chart;
            Button btnRefresh, btnZoomIn, btnZoomOut;

            if (string.Equals(processCode, Process.COATING))
            {
                c1Chart = chartCoater;
                btnRefresh = btnCoaterRefresh;
                btnZoomIn = btnCoaterZoomIn;
                btnZoomOut = btnCoaterZoomOut;
            }
            else
            {
                c1Chart = chartRollPress;
                btnRefresh = btnRollPressRefresh;
                btnZoomIn = btnRollPressZoomIn;
                btnZoomOut = btnRollPressZoomOut;
            }

            c1Chart.View.AxisX.Scale = scale;
            //btnRefresh.IsEnabled = scale != 1;
            btnRefresh.IsCancel = !scale.Equals(1);
            btnZoomIn.IsCancel = scale > 0.002;
            btnZoomOut.IsCancel = scale < 1;
            UpdateScrollbars(processCode);
        }

        private void UpdateScrollbars(string processCode)
        {
            C1Chart c1Chart = string.Equals(processCode, Process.COATING) ? chartCoater : chartRollPress;

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

                if (c1Chart.Name == "chartCoater" || c1Chart.Name == "chartRollPress")
                {
                    SetScale(1.0, c1Chart.Name == "chartCoater" ? Process.COATING : Process.ROLL_PRESSING);
                    c1Chart.View.AxisX.MinScale = 0.05;
                    c1Chart.View.Margin = new Thickness(5, 0, 5, 20);
                }

                if (c1Chart.Name == "chartLine")
                {
                    c1Chart.View.Margin = new Thickness(5, 0, 5, 5);
                }
            }
        }

        private void EndUpdateChart()
        {
            //변경 작업 필요 함.
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                if (c1Chart.Name == "chartRollPress" || c1Chart.Name == "chartLine" || c1Chart.Name == "chartCoater")
                    c1Chart.View.AxisX.Reversed = true;

                c1Chart.EndUpdate();
            }
        }

        private DataTable MakeTableForDisplay(DataTable dt, ChartDisplayType chartDisplayType, string processCode = null)
        {
            var dtBinding = dt.Copy();

            if (!CommonVerify.HasTableRow(dt)) return dtBinding;

            if (chartDisplayType == ChartDisplayType.TagStart || chartDisplayType == ChartDisplayType.TagEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    //row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -50 : -70;
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    //row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();
                    row["TAG"] = null;

                    // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
                    if ("TAG_SECTION_LANE".Equals(row["EQPT_MEASR_PSTN_ID"]))
                    {
                        String laneInfo = row["LANE_NO_LIST"].ToString();
                        row["TOOLTIP"] = "(" + laneInfo + ")" + row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
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

                    row["SOURCE_Y_PSTN"] = -33;
                    row["TOOLTIP"] = row["CMCDNAME"] + "[" + row["ROLLMAP_CLCT_TYPE"] + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagVisionTop || chartDisplayType == ChartDisplayType.TagVisionBack)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_STRT_PSTN", DataType = typeof(double) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_END_PSTN", DataType = typeof(double) });

                int maxYvalue;

                if (processCode == Process.COATING)
                {
                    if (_sideType == "B")
                        maxYvalue = chartDisplayType == ChartDisplayType.TagVisionTop ? 100 : 220;
                    else
                        maxYvalue = chartDisplayType == ChartDisplayType.TagVisionBack ? 100 : 220;
                }
                else
                {
                    maxYvalue = chartDisplayType == ChartDisplayType.TagVisionBack ? 100 : 220;
                }

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_STRT_PSTN"] = maxYvalue - 10;
                    row["SOURCE_Y_END_PSTN"] = maxYvalue;
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

                foreach (DataRow row in dtBinding.Rows)
                {
                    double rowStartPosition;
                    double rowEndPosition;
                    double.TryParse(Util.NVC(row["RAW_STRT_PSTN"]), out rowStartPosition);
                    double.TryParse(Util.NVC(row["RAW_END_PSTN"]), out rowEndPosition);


                    row["TOOLTIP"] = row["ROW_NUM"] + " Cut" + "( " + $"{rowStartPosition:###,###,###,##0.##}" + "m" + " ~ " + $"{rowEndPosition:###,###,###,##0.##}" + "m" + ")";
                    row["END_PSTN"] = $"{rowEndPosition:###,###,###,##0.##}";
                    row["SOURCE_Y_PSTN"] = 220;
                    row["STRT_PSTN"] = $"{rowStartPosition:###,###,###,##0.##}";
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

                    row["SOURCE_Y_PSTN"] = -80; //-50;
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
                    if (processCode == Process.COATING)
                    {
                        if (_sideType == "B")
                        {
                            row["Y_PSTN"] = chartDisplayType == ChartDisplayType.SurfaceBack ? row["Y_POSITION"].GetDouble() + 120 : row["Y_POSITION"].GetDouble();
                        }
                        else
                        {
                            row["Y_PSTN"] = chartDisplayType == ChartDisplayType.SurfaceTop ? row["Y_POSITION"].GetDouble() + 120 : row["Y_POSITION"].GetDouble();
                        }
                    }
                    else
                    {
                        row["Y_PSTN"] = chartDisplayType == ChartDisplayType.SurfaceTop ? row["Y_POSITION"].GetDouble() + 120 : row["Y_POSITION"].GetDouble();
                    }
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
                    row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + " ]";
                    row["TAG"] = row["EQPT_MEASR_PSTN_ID"] + ";" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + $"{row["WND_LEN"]:###,###,###,##0.##}";
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
        #endregion
    }
}