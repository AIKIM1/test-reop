/*************************************************************************************
 Created Date : 2023.05.02
      Creator : 신광희
   Decription : 소형 전극 Roll Map - 롤맵 차트 원통형(2170) 코터 맵 팝업 (COM001_ROLLMAP_COATER 참조)
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.02  신광희 : Initial Created.
  2023.12.12  윤기업 : Map 표현 요약 변경 및 웾게이지 압축 추가
  2024.03.19  김지호 : [E20240319-000656] 범례에 Non 추가 및 차트 색 (회색) 추가, SCAN_COLRMAP에 Err, Non, Ch 시에 Tooltip에 데이터 오류, 데이터 미수신, 데이터 미측정 내용 표시
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

    public partial class COM001_ROLLMAP_MOBILE_COATER : C1Window, IWorkArea
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
        private double _xMaxLength;
        private double _xMinLength;
        private double _sampleCutLength;
        private string _polarityCode = string.Empty;
        private string _version = string.Empty;
        private DataTable _dtLineLegend;
        private DataTable _dtRollMapDefect;
        private DataTable _dtRollMapGauge;
        private DataTable _dtDefect;
        private DataTable _dtLaneInfo;
        private DataTable _dtLaneLegend;
        private DataTable _dtLane;
        private DataTable _dtDefectScription;
        private DataTable _dtTagSection;
        private DataTable _dtRollMapLane;

        private string _selectedStartSectionText = string.Empty;
        private string _selectedStartSectionPosition = string.Empty;
        private string _selectedStartSampleFlag = string.Empty;
        private string _selectedEndSectionPosition = string.Empty;
        private string _selectedEndSampleFlag = string.Empty;
        private string _selectedCollectType = string.Empty;

        private bool _isSelectedTagSection;
        private bool _isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        private double _selectedStartSection = 0;
        private double _selectedEndSection = 0;
        private double _selectedchartPosition = 0;
        private bool _isRollMapLot;
        private bool _isRollMapPress = true; // 웹게이지 압축 여부

        // 웹게이지 데이터 속도 향상 목적?
        private List<AlarmZone> _listgauge;

        private CoordinateType _CoordinateType;
        private enum CoordinateType
        {
            RelativeCoordinates,    //상대좌표
            Absolutecoordinates     //절대좌표
        }

        public COM001_ROLLMAP_MOBILE_COATER()
        {
            InitializeComponent();
        }
        #endregion

        #region # Event
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
            _CoordinateType = CoordinateType.RelativeCoordinates;
            Initialize();
            GetLegend();
            GetRollMap();
        }

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
            CMM_ROLLMAP_COATER_DATACOLLECT popupRollMapDataCollect = new CMM_ROLLMAP_COATER_DATACOLLECT();
            popupRollMapDataCollect.FrameOperation = FrameOperation;
            object[] parameters = new object[6];
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
            CMM_ROLLMAP_COATER_DATACOLLECT popup = sender as CMM_ROLLMAP_COATER_DATACOLLECT;
            if (popup != null && popup.IsUpdated)
            {
                //btnSearch_Click(btnSearch, null);
            }
        }

        private void btnRollMapOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            CMM_ROLLMAP_OUTSIDE_SCRAP popRollMapOutsideScrap = new CMM_ROLLMAP_OUTSIDE_SCRAP();

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = txtLot.Text;
            parameters[3] = _wipSeq;
            parameters[4] = "OUTSIDE_SCRAP"; //최외각 폐기
            parameters[5] = true;

            C1WindowExtension.SetParameters(popRollMapOutsideScrap, parameters);
            popRollMapOutsideScrap.Closed += popRollMapOutsideScrap_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapOutsideScrap.ShowModal()));
        }

        private void popRollMapOutsideScrap_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_OUTSIDE_SCRAP popup = sender as CMM_ROLLMAP_OUTSIDE_SCRAP;
            if (popup != null && popup.IsUpdated)
            {
                _isRollMapLot = SelectRollMapLot();
                btnSearch_Click(btnSearch, null);
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
            CMM_ROLLMAP_PSTN_UPD popup = sender as CMM_ROLLMAP_PSTN_UPD;
            if (popup != null && popup.IsUpdated)
            {
                _isRollMapLot = SelectRollMapLot();
                btnSearch_Click(btnSearch, null);
            }
            else
            {
                tagSectionClear_Click(tagSectionClear, null);
            }
        }

        private void popRollMapTagsectionMerge_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_TAGSECTION_MERGE popup = sender as CMM_ROLLMAP_TAGSECTION_MERGE;
            if (popup != null && popup.IsUpdated)
            {
                ShowLoadingIndicator();
                DoEvents();

                for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartCoater.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TAG_SECTION"))
                    {
                        chartCoater.Data.Children.Remove(dataSeries);
                    }
                }

                //전체 조회 시 데이터가 많은 경우 바인딩 하는 속도 문제로 TAG_SECTION 따로 처리
                string adjFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
                string sampleFlag = "Y";
                DataTable dtDefect = SelectRollMapDefectInfo(_equipmentCode, txtLot.Text, _wipSeq, adjFlag, sampleFlag);

                var queryTagSection = dtDefect.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dtDefect.Clone();
                dtTagSection.TableName = "TAG_SECTION";
                _dtTagSection = dtTagSection;

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawRollMapTagSection(dtTagSection);
                }
                HiddenLoadingIndicator();
            }
            else
            {
                DrawRollMapTagSection(_dtTagSection);
            }

            SetScale(1.0);

            _selectedStartSectionText = string.Empty;
            _selectedStartSectionPosition = string.Empty;
            _selectedEndSectionPosition = string.Empty;
            _selectedCollectType = string.Empty;
            _selectedStartSampleFlag = string.Empty;
            _selectedEndSampleFlag = string.Empty;

            btnZoomOut.IsEnabled = true;
            btnZoomIn.IsEnabled = true;
            btnRefresh.IsEnabled = true;
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
            e.Handled = true;
            GetLegend();
        }

        private void rdoThickness_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            GetLegend();
        }

        private void rdoGradeBlock_Click(object sender, RoutedEventArgs e)
        {
            _isRollMapPress = true;
        }

        private void rdoAvgDtl_Click(object sender, RoutedEventArgs e)
        {
            _isRollMapPress = false;
        }
        private void cboRollMapExpressSummary_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (string.IsNullOrEmpty(cboRollMapExpressSummary.SelectedValue.GetString())) return;

            DataTable dt = DataTableConverter.Convert(cboRollMapExpressSummary.ItemsSource);
            DataRow dr = dt.Rows[cboRollMapExpressSummary.SelectedIndex];

            string[] laneArray;
            laneArray = dr["LaneDescription"].GetString().Split(',');

            //C : 양극
            //A : 음극
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

            CMM_ROLLMAP_PSTN_UPD popRollMapPositionUpdate = new CMM_ROLLMAP_PSTN_UPD();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[12];
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
            parameters[9] = _xMaxLength - _sampleCutLength;
            parameters[10] = (_isRollMapResultLink && _isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;
            parameters[11] = true;
            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }

        private void tagMerge_Click(object sender, RoutedEventArgs e)
        {
            if (_isRollMapResultLink && _isRollMapLot) return; 

            MenuItem itemmenu = sender as MenuItem;
            ContextMenu itemcontext = itemmenu.Parent as ContextMenu;
            TextBlock textBlock = itemcontext.PlacementTarget as TextBlock;

            if (textBlock == null) return;

            //Border border = FindVisualParentByName<Border>(textBlock, "tbTagSection");
            string[] splitItem = textBlock.Tag.GetString().Split(';');

            // 최초 셀병합 시
            if (string.IsNullOrEmpty(_selectedStartSectionText))
            {
                if(string.Equals(textBlock.Text, "S"))
                {
                    textBlock.Background = new SolidColorBrush(Colors.White);
                }
                else
                {
                    Util.MessageInfo("SFU8529");    //시작위치를 먼저 선택하세요.
                    return;
                }

                btnZoomOut.IsEnabled = false;
                btnZoomIn.IsEnabled = false;
                btnRefresh.IsEnabled = false;

                _selectedStartSectionText = "S";
                _selectedStartSectionPosition = splitItem[0];
                _selectedCollectType = splitItem[3];
                _selectedStartSampleFlag = splitItem[4];
            }
            else
            {
                if (string.Equals(textBlock.Text, "E"))
                {
                    _selectedEndSectionPosition = splitItem[1];
                    _selectedEndSampleFlag = splitItem[4];

                    if (_selectedEndSectionPosition.GetDouble() - _selectedStartSectionPosition.GetDouble() < 0)
                    {
                        Util.MessageInfo("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return;
                    }

                    if (_selectedStartSampleFlag != _selectedEndSampleFlag)
                    {
                        Util.MessageValidation("SFU8538");    //Sample Cut 구간의 위차와 양산 구간의 위치는 병합 할수 없습니다.
                        return;
                    }

                    
                    textBlock.Background = new SolidColorBrush(Colors.White);

                    // Tag Section Merge 팝업 호출 함.
                    CMM_ROLLMAP_TAGSECTION_MERGE popRollMapTagsectionMerge = new CMM_ROLLMAP_TAGSECTION_MERGE();
                    popRollMapTagsectionMerge.FrameOperation = FrameOperation;

                    object[] parameters = new object[9];
                    parameters[0] = txtLot.Text;
                    parameters[1] = _wipSeq;
                    parameters[2] = _selectedStartSectionPosition;
                    parameters[3] = _selectedEndSectionPosition;
                    parameters[4] = _processCode;
                    parameters[5] = _equipmentCode;
                    parameters[6] = _selectedCollectType;
                    parameters[7] = _selectedEndSampleFlag;
                    parameters[8] = true;

                    C1WindowExtension.SetParameters(popRollMapTagsectionMerge, parameters);
                    popRollMapTagsectionMerge.Closed += popRollMapTagsectionMerge_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popRollMapTagsectionMerge.ShowModal()));
                }
                else
                {
                    Util.MessageInfo("SFU8531");    //선택된 시작위치가 존재 합니다.
                    DrawRollMapTagSection(_dtTagSection);

                    _selectedStartSectionText = string.Empty;
                    _selectedStartSectionPosition = string.Empty;
                    _selectedEndSectionPosition = string.Empty;
                    _selectedCollectType = string.Empty;
                    _selectedStartSampleFlag = string.Empty;
                    _selectedEndSampleFlag = string.Empty;

                    btnZoomOut.IsEnabled = true;
                    btnZoomIn.IsEnabled = true;
                    btnRefresh.IsEnabled = true;
                    return;
                }
            }
        }

        private void OriginalSize_Click(object sender, RoutedEventArgs e)
        {
            SetScale(1.0);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            // View.AxisX.Scale 값이 0.05 이하인 경우 AlarmZone 영역의 데이터가 나오지 않는 경우가 발생하여 강제 처리 함.
            if (chartCoater.View.AxisX.MinScale >= chartCoater.View.AxisX.Scale * 0.75)
                return;

            SetScale(chartCoater.View.AxisX.Scale * 0.75);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SetScale(chartCoater.View.AxisX.Scale * 1.50);
        }

        private void AxisScrollBarTopOnAxisRangeChanged(object sender, AxisRangeChangedEventArgs e)
        {
            //chartBack.View.AxisX.Value = chartTop.View.AxisX.Value;
        }

        private void btnReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartCoater.BeginUpdate();
            chartCoater.View.AxisX.Reversed = !chartCoater.View.AxisX.Reversed;
            chartCoater.EndUpdate();
        }

        private void btnReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartCoater.BeginUpdate();
            chartCoater.View.AxisY.Reversed = !chartCoater.View.AxisY.Reversed;
            chartCoater.EndUpdate();
        }

        private void chartCoater_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", "구간불량 등록");
                return;
            }
        }

        private void chartCoater_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point curPoint = e.GetPosition(chartCoater.View);
            var chartpoint = chartCoater.View.PointToData(curPoint);

            if (chartpoint == null || chartpoint.X < 0)
            {
                Util.MessageInfo("SFU3625");    //선택한 영역이 잘못되었습니다.
                return;
            }

            _selectedchartPosition = chartpoint.X;
        }

        private void tagSection_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", "구간불량 등록");
                return;
            }
        }

        private void tagSectionStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTagSection()) return;

            if (_selectedchartPosition < _sampleCutLength)
            {
                Util.MessageValidation("SFU8542");
                return;
            }

            // No.1 구간불량 시작 버튼 클릭 시 기존에 구간불량 시작 위치 또는 구간불량 종료 위치 데이터가 있는 경우 해당 데이터 삭제 후 다시 시작위치 작성
            if (_isSelectedTagSection)
            {
                // 구간불량 DataSeries 가 chartCoater 상에 존재하는 경우 임.
                for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartCoater.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
                    {
                        chartCoater.Data.Children.Remove(dataSeries);
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

            double tagsectionStart = (_sampleCutLength > _selectedStartSection) ? _selectedStartSection : _selectedStartSection - _sampleCutLength;

            DataRow dr = dtTagSectionStart.NewRow();
            dr["COLORMAP"] = "#FFE400";
            dr["TAGNAME"] = "S";
            dr["TAG"] = _selectedStartSection.GetString();
            dr["TOOLTIP"] = ObjectDic.Instance.GetObjectName("시작위치") + " [" + ObjectDic.Instance.GetObjectName("길이") + $"{tagsectionStart:###,###,###,##0.#}" + " " + ObjectDic.Instance.GetObjectName("pattern") + " ]";
            dr["AXISX_PSTN"] = _selectedStartSection;
            dr["AXISY_PSTN"] = 45;

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
            chartCoater.Data.Children.Add(ds);

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

            double tagsectionEnd = (_sampleCutLength > _selectedEndSection) ? _selectedEndSection : _selectedEndSection - _sampleCutLength;

            DataRow dr = dtTagSectionEnd.NewRow();
            dr["COLORMAP"] = "#FFE400";
            dr["TAGNAME"] = "E";
            dr["TAG"] = _selectedEndSection.GetString();
            dr["TOOLTIP"] = ObjectDic.Instance.GetObjectName("종료위치") + " [" + ObjectDic.Instance.GetObjectName("길이") + $"{tagsectionEnd:###,###,###,##0.#}" + " " + ObjectDic.Instance.GetObjectName("pattern") + " ]";
            dr["AXISX_PSTN"] = _selectedEndSection;
            dr["AXISY_PSTN"] = 45;

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
            chartCoater.Data.Children.Add(ds);

            _isSelectedTagSection = true;

            PopupTagSection();
        }

        private void tagSectionClear_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTagSection()) return;

            for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = chartCoater.Data.Children[i];
                if (dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
                {
                    chartCoater.Data.Children.Remove(dataSeries);
                }
            }
            _selectedStartSection = 0;
            _selectedEndSection = 0;
            _selectedchartPosition = 0;
            _isSelectedTagSection = false;
        }
        #endregion

        #region # Method
        private void Initialize()
        {
            try
            {
                InitializeWindowResize();
                InitializeControl();
                InitializeMaterialDvision();
                InitializeCombo();
                
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
            txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
            chartCoater.View.AxisX.ScrollBar = new AxisScrollBar();

            _dtLineLegend = new DataTable();

            _dtLineLegend.Columns.Add("NO", typeof(int));
            _dtLineLegend.Columns.Add("COLORMAP", typeof(string));
            _dtLineLegend.Columns.Add("VALUE", typeof(string));
            _dtLineLegend.Columns.Add("GROUP", typeof(string));
            _dtLineLegend.Columns.Add("SHAPE", typeof(string));

            DataTable dtColorMapSpec = GetColorMapSpec("ROLLMAP_COLORMAP_SPEC");
            if (CommonVerify.HasTableRow(dtColorMapSpec))
            {
                foreach (DataRow row in dtColorMapSpec.Rows)
                {
                    _dtLineLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCDNAME"].GetString(), "LOAD", "RECT");
                }
            }

            _dtDefect = new DataTable();
            _dtDefect.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefect.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefect.Columns.Add("COLORMAP", typeof(string));

            _dtLaneLegend = new DataTable();
            _dtLaneLegend.Columns.Add("LANE_NO", typeof(string));
            _dtLaneLegend.Columns.Add("COLORMAP", typeof(string));

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

            _dtLane = new DataTable();
            _dtLane.Columns.Add("LANE_NO", typeof(string));

            // 불량 범례 테이블
            _dtDefectScription = new DataTable();
            _dtDefectScription.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefectScription.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefectScription.Columns.Add("COLORMAP", typeof(string));

        }

        private void InitializeCombo()
        {
            // 2023.10.17 고지연 사원 요청으로 모델별 Lane수 정보에 따라 선택범위 설정될 수 있게 수정(변경된 BizRule 에 따른 UI 수정) 
            // Lane별 롤맵 선택 MultiSelectionBox
            //SetRollMapLaneMultiSelectionBox(msbRollMapLane);

            // 맵표현 요약 콤보박스
            //SetRollMapExpressSummaryCombo(cboRollMapExpressSummary);

            GetRollMapLaneRange();
        }

        private void InitializeMaterialDvision()
        {
            DataTable dt = SelectEquipmentPolarity(_equipmentCode);

            if (CommonVerify.HasTableRow(dt))
            {
                _polarityCode = dt.Rows[0]["ELTR_TYPE_CODE"].GetString();
                
                if (_polarityCode == "C")
                {
                    grdMaterialCathode.Visibility = Visibility.Visible;
                    grdMaterialAnode.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grdMaterialCathode.Visibility = Visibility.Collapsed;
                    grdMaterialAnode.Visibility = Visibility.Visible;
                }

                if (string.IsNullOrEmpty(_equipmentName))
                {
                    _equipmentName = dt.Rows[0]["EQPTNAME"].GetString();
                    txtEquipmentName.Text = _equipmentName + " [" + _equipmentCode + "]";
                }
                else
                {
                    txtEquipmentName.Text = _equipmentName;
                }
            }
        }

        /// <summary>
        /// 코터 차트 Initialize
        /// </summary>
        private void InitializeCoaterChart()
        {
            double maxLength = _xMaxLength.Equals(0) ? 10 : _xMaxLength;

            chartCoater.View.AxisX.Min = 0;
            //chartCoater.View.AxisY.Min = -40;
            chartCoater.View.AxisY.Min = -30;
            chartCoater.View.AxisX.Max = maxLength;
            chartCoater.View.AxisY.Max = 100;

            InitializeChartView(chartCoater);
        }

        /// <summary>
        /// 원재료 차트 Initialize
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void InitializeMaterialChart(double min, double max)
        {
            var grdMaterial = _polarityCode == "C" ? grdMaterialCathode : grdMaterialAnode;

            foreach (C1Chart materialChart in Util.FindVisualChildren<C1Chart>(grdMaterial))
            {
                materialChart.View.AxisX.MajorGridStrokeThickness = 0;
                materialChart.View.AxisX.MinorGridStrokeThickness = 0;
                materialChart.View.AxisY.MajorGridStrokeThickness = 0;
                materialChart.View.AxisY.MinorGridStrokeThickness = 0;
                //materialchart.View.AxisX.LogBase = 3;
                materialChart.View.AxisX.MinorTickOverlap = 30;
                materialChart.View.AxisX.MajorGridStrokeDashes = new DoubleCollection { 3, 2 };
                // hide all tick marks
                materialChart.View.AxisX.MajorTickThickness = 0;
                materialChart.View.AxisX.MinorTickThickness = 0;
                materialChart.View.AxisY.MajorTickThickness = 0;
                materialChart.View.AxisY.MinorTickThickness = 0;
                //
                materialChart.View.AxisX.AutoMax = false;
                materialChart.View.AxisX.AutoMin = false;

                // hide axis labels
                materialChart.View.AxisY.Foreground = new SolidColorBrush(Colors.Transparent);
                materialChart.View.AxisX.Foreground = new SolidColorBrush(Colors.Transparent);

                materialChart.View.AxisX.Min = min;
                materialChart.View.AxisX.Max = max;
                materialChart.View.AxisY.Min = 0;
                materialChart.View.AxisY.Max = 100;
                materialChart.View.Margin = new Thickness(2, 1.5, 2, 1.5);

                var convertFromString = ColorConverter.ConvertFromString("#F6F6F6");
                AlarmZone alarmZone = new AlarmZone
                {
                    Near = min,
                    Far = max,
                    ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                    LowerExtent = 0,
                    UpperExtent = 100,
                    ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                };
                materialChart.Data.Children.Insert(0, alarmZone);
            }
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


            if (c1Chart.Name == "chartCoater")
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
            if (grdDefectLegend.ColumnDefinitions.Count > 0) grdDefectLegend.ColumnDefinitions.Clear();
            if (grdDefectLegend.RowDefinitions.Count > 0) grdDefectLegend.RowDefinitions.Clear();
            grdDefectLegend.Children.Clear();

            if (grdLoadingLegend.ColumnDefinitions.Count > 0) grdLoadingLegend.ColumnDefinitions.Clear();
            if (grdLoadingLegend.RowDefinitions.Count > 0) grdLoadingLegend.RowDefinitions.Clear();
            grdLoadingLegend.Children.Clear();

            if (grdLaneLegend.ColumnDefinitions.Count > 0) grdLaneLegend.ColumnDefinitions.Clear();
            if (grdLaneLegend.RowDefinitions.Count > 0) grdLaneLegend.RowDefinitions.Clear();
            grdLaneLegend.Children.Clear();
            

            btnRefresh.IsEnabled = true;
            btnZoomIn.IsEnabled = true;
            btnZoomOut.IsEnabled = true;

            tbTopupper.Text = string.Empty;
            tbToplower.Text = string.Empty;
            tbBackupper.Text = string.Empty;
            tbBacklower.Text = string.Empty;

            _isSelectedTagSection = false;
            _selectedStartSection = 0;
            _selectedEndSection = 0;
            _selectedchartPosition = 0;

            _isRollMapLot = SelectRollMapLot();
            _isRollMapResultLink = IsRollMapResultApply();
            if(_isRollMapResultLink && _isRollMapLot)
            {
                btnRollMapOutsideScrap.Visibility = Visibility.Collapsed;
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnRollMapOutsideScrap.Visibility = Visibility.Visible;
                btnRollMapPositionEdit.Visibility = Visibility.Visible;
            }
            btnDefect.Visibility = _isRollMapLot ? Visibility.Collapsed : Visibility.Visible;

            //tbTopBack.Text = (bool)rdoWebgaugeBack?.IsChecked ? "B" : "T";

            BeginUpdateChart();
        }

        private void InitializeDataTables()
        {
            _selectedStartSectionText = string.Empty;
            _selectedStartSectionPosition = string.Empty;
            _selectedEndSectionPosition = string.Empty;
            _selectedCollectType = string.Empty;
            _selectedStartSampleFlag = string.Empty;
            _selectedEndSampleFlag = string.Empty;

            _dtRollMapGauge?.Clear();
            _dtDefect?.Clear();
            _dtLaneInfo?.Clear();
            _dtLane?.Clear();
            _dtTagSection?.Clear();
            _dtDefectScription?.Clear();
            _dtRollMapDefect?.Clear();
            _dtRollMapGauge?.Clear();
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

            StackPanel sp1 = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 3)
            };


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


                // Sample Cut
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 5, 3)
                };

                Path samplePath = new Path { Fill = Brushes.Black, Stretch = Stretch.Uniform };
                //Sample Path
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

        private void GetMaxLength(DataTable dtLength)
        {
            _xMaxLength = 0;
            _xMinLength = 0;

            //SetCoaterChartContextMenu(false);
            var query = (from t in dtLength.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
                         {
                             sourceEndPosition = t.Field<decimal>("SOURCE_END_PTN_PSTN"),
                             ProdQty = t.Field<decimal?>("INPUT_QTY"),
                             GoodQty = t.Field<decimal?>("WIPQTY_ED")
                         }).FirstOrDefault();

            if(query != null)
            {
                //tbOutputProdQty.Text = @"P" + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                //tbOutputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                //tbOutputGoodQty.Text = @"G" + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                //tbOutputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
            }
            else
            {

            }

            _xMaxLength = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PTN_PSTN"].GetDouble()).GetDouble();
            _xMinLength = dtLength.AsEnumerable().ToList().Min(r => r["SOURCE_END_PTN_PSTN"].GetDouble()).GetDouble();
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
            chart.View.AxisX.Max = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PTN_PSTN"].GetDouble()).GetDouble();

            DataTable dt = new DataTable();
            dt.Columns.Add("SOURCE_END_PTN_PSTN", typeof(double));
            dt.Columns.Add("ARROW_PSTN", typeof(double));
            dt.Columns.Add("SOURCE_Y_PSTN", typeof(double));
            dt.Columns.Add("ARROW_Y_PSTN", typeof(double));
            dt.Columns.Add("COLORMAP", typeof(string));
            dt.Columns.Add("CIRCLENAME", typeof(string));
            dt.Columns.Add("WND_DIRCTN", typeof(string));
            dt.Columns.Add("TOOLTIP", typeof(string));


            double arrowValue = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PTN_PSTN"].GetDouble()).GetDouble() * 0.025;

            for (int i = 0; i < dtLength.Rows.Count; i++)
            {
                double arrowYPosition;
                double arrowPosition;

                if (dtLength.Rows[i]["EQPT_MEASR_PSTN_ID"].GetString() == "RW")
                {
                    arrowYPosition = dtLength.Rows[i]["WND_DIRCTN"].GetInt() == 1 ? 0 : 60;
                    arrowPosition = dtLength.Rows[i]["SOURCE_END_PTN_PSTN"].GetDouble() - arrowValue;
                }
                else
                {
                    arrowYPosition = dtLength.Rows[i]["WND_DIRCTN"].GetInt() == 1 ? 60 : 0;
                    arrowPosition = dtLength.Rows[i]["SOURCE_END_PTN_PSTN"].GetDouble() + arrowValue;
                }

                DataRow dr = dt.NewRow();
                dr["SOURCE_END_PTN_PSTN"] = dtLength.Rows[i]["SOURCE_END_PTN_PSTN"];
                dr["ARROW_PSTN"] = arrowPosition;
                dr["SOURCE_Y_PSTN"] = 0;
                dr["ARROW_Y_PSTN"] = arrowYPosition;
                dr["CIRCLENAME"] = dtLength.Rows[i]["EQPT_MEASR_PSTN_ID"];
                dr["COLORMAP"] = "#000000";
                //dr["TOOLTIP"] = "SOURCE_END_PSTN :" + dtLength.Rows[i]["SOURCE_END_PSTN"].GetString();
                dt.Rows.Add(dr);
            }


            XYDataSeries ds = new XYDataSeries();
            ds.ItemsSource = DataTableConverter.Convert(dt);
            ds.XValueBinding = new Binding("SOURCE_END_PTN_PSTN");
            ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
            ds.ChartType = ChartType.XYPlot;
            ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
            ds.PointLabelTemplate = grdMain.Resources["chartCircle"] as DataTemplate;
            ds.Margin = new Thickness(0);

            ds.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
                pe.Size = new Size(50, 50);
                // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
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
                // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
            };
            chart.Data.Children.Add(dsArrowRight);
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
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CT_CHART_CY"; 

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("OUT_LOTID", typeof(string));
            inTable.Columns.Add("OUT_WIPSEQ", typeof(decimal));
            inTable.Columns.Add("MPOINT", typeof(string));
            inTable.Columns.Add("MPSTN", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));
            inTable.Columns.Add("LANE_LIST", typeof(string));
            inTable.Columns.Add("SUM_FLAG", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = txtLot.Text;
            newRow["WIPSEQ"] = _wipSeq;
            newRow["MPOINT"] = rdoThickness.IsChecked != null && (bool)rdoThickness.IsChecked ? "1" : "2";
            newRow["MPSTN"] = "2";
            newRow["ADJFLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
            newRow["SMPL_FLAG"] = "Y";
            newRow["LANE_LIST"] = msbRollMapLane.SelectedItemsToString;
            newRow["SUM_FLAG"] = _isRollMapPress ? "1" : "2";
            inTable.Rows.Add(newRow);

            //string xml = inDataSet.GetXml();

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_GUAGE,OUT_MATERIAL,OUT_DEFECT,OUT_HEAD,OUT_LENGTH,OUT_LANE", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        HiddenLoadingIndicator();
                        return;
                    }

                    if (rdoRelativeCoordinates != null && (bool)rdoRelativeCoordinates.IsChecked)
                        _CoordinateType = CoordinateType.RelativeCoordinates;
                    else
                        _CoordinateType = CoordinateType.Absolutecoordinates;

                    if (rdoThickness.IsChecked != null && (bool)rdoThickness.IsChecked)
                    {
                        txtMeasurement.Text = ObjectDic.Instance.GetObjectName("두께");
                    }
                    else
                    {
                        txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
                    }
                    SetMenuUseCount();

                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        DataTable dtGauge = bizResult.Tables["OUT_GUAGE"];          // 코터 차트 영역 Gauge 데이터 표시, 로딩량 차트 영역 데이터 표시 테이블
                        DataTable dtMaterial = bizResult.Tables["OUT_MATERIAL"];    // 투입자재 영역 데이터 표시 테이블
                        DataTable dtDefect = bizResult.Tables["OUT_DEFECT"];        // 코터 차트 영역불량정보 표시 테이블
                        DataTable dtHead = bizResult.Tables["OUT_HEAD"];            // 코터 차트 RW 길이 표시 및 Sample Cut 데이터 표시 테이블
                        DataTable dtLength = bizResult.Tables["OUT_LENGTH"];        // 코터 차트 RW, UW 길이 정보 테이블
                        DataTable dtLane = bizResult.Tables["OUT_LANE"];            // 코터 차트 영역 무지부, 유지부, Edge Top Back 정보 테이블

                        if(CommonVerify.HasTableRow(dtLength))
                        {
                            // Get Max, Min Length, 양품량, 생산량 
                            GetMaxLength(dtLength);
                            DrawWinderDirection(dtLength);
                            SetCoaterChartContextMenuEnable(true);
                        }
                        else
                        {
                            SetCoaterChartContextMenuEnable(false);
                        }

                        if(CommonVerify.HasTableRow(dtMaterial))
                        {
                            DrawMaterialChart(dtMaterial);
                        }

                        InitializeCoaterChart();

                        if (CommonVerify.HasTableRow(dtLane))
                        {
                            //롤맵 무지부, 유지부, Lane 텍스트 표시
                            DrawRollMapLane(dtLane);
                        }

                        if (CommonVerify.HasTableRow(dtGauge))
                        {
                            DrawGauge(dtGauge);
                            DrawLoading(dtGauge);
                            DrawLoadingLaneLegend();
                            _dtRollMapGauge = dtGauge.Copy();
                        }

                        if (CommonVerify.HasTableRow(dtHead))
                        {
                            DrawStartEndYAxis(dtHead);
                        }

                        if(CommonVerify.HasTableRow(dtDefect))
                        {
                            DrawDefect(dtDefect);
                            DrawDefectLegend(dtDefect);
                        }
                        _dtRollMapDefect = dtDefect.Copy();

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

            if(dtLaneInfo.Columns.Contains("TAB"))
            {
                // Tab 위치 표시
                DisplayTabLocation(dtLaneInfo);
            }

            dtLaneInfo.Columns.Add("LANE_LENGTH", typeof(double));
            dtLaneInfo.Columns.Add("Y_STRT_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("Y_END_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("Y_PSTN", typeof(decimal));
            dtLaneInfo.Columns.Add("LANE_LENGTH_SUM", typeof(double));

            if (CommonVerify.HasTableRow(dtLaneInfo))
            {
                //Y 좌표 계산 코터 롤맵 차트에선 Lane 이 하단부터 순차적으로 올라 감. 
                //dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderBy(o => o.Field<int>("RN")).CopyToDataTable();
                dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderBy(o => o.Field<string>("LANE_NO_CUR")).ThenBy(x => x.Field<int>("RN")).CopyToDataTable();

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
                                //dtLaneInfo.Rows[i]["Y_PSTN"] = dtLaneInfo.Rows[i]["LANE_LENGTH"].GetDouble() / 2;
                                dtLaneInfo.Rows[i]["Y_PSTN"] = dtLaneInfo.Rows[i]["LANE_LENGTH"].GetDouble() / 3;
                            }
                            else
                            {
                                dtLaneInfo.Rows[i]["Y_STRT_PSTN"] = yStartLength;
                                dtLaneInfo.Rows[i]["Y_END_PSTN"] = laneSeq == (query.Count - 1) ? 100 : dtLaneInfo.Rows[i]["LANE_LENGTH"].GetDouble() + yStartLength;
                                //dtLaneInfo.Rows[i]["Y_PSTN"] = dtLaneInfo.Rows[i]["Y_STRT_PSTN"].GetDouble() + dtLaneInfo.Rows[i]["LANE_LENGTH"].GetDouble() / 2;
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

            //if(CommonVerify.HasTableRow(dtLaneInfo)) dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderBy(o => o.Field<int>("RN")).CopyToDataTable();
            _dtLaneInfo = dtLaneInfo;

            _dtLane?.Clear();

            for (int i = 0; i < _dtLaneInfo.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(_dtLaneInfo.Rows[i]["LANE_NO"].GetString()))
                {
                    DataRow newRow = _dtLane.NewRow();
                    newRow["LANE_NO"] = _dtLaneInfo.Rows[i]["LANE_NO"];
                    _dtLane.Rows.Add(newRow);
                }
            }
            
            return dtLaneInfo;
        }

        private DataTable SelectRollMapDefectInfo(string equipmentCode, string lotId, string wipSeq, string adjFlag, string sampleFlag)
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_CT_DEFECT_CHART_PTN_CY";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));
            inTable.Columns.Add("LANE_LIST", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["EQPTID"] = equipmentCode;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;
            dr["LANE_LIST"] = msbRollMapLane.SelectedItemsToString;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        #region [투입자재 Chart 바인딩]
        private void DrawMaterialChart(DataTable dt)
        {
            // 양극, 음극 구분에 따른 불필요한 데이터 제외 처리
            string[] cathodeMeasurementPosition = { "INS_SLURRY_TOP", "SLURRY_TOP_L2", "SLURRY_TOP_L1", "UW", "SLURRY_BACK_L1", "SLURRY_BACK_L2", "INS_SLURRY_BACK" };
            string[] anodeMeasurementPosition = { "SLURRY_TOP_L2", "SLURRY_TOP_L1", "UW", "SLURRY_BACK_L1", "SLURRY_BACK_L2" };

            if (_polarityCode == "C")
                dt.AsEnumerable().Where(r => !cathodeMeasurementPosition.Contains(r.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList().ForEach(row => row.Delete());
            else
                dt.AsEnumerable().Where(r => !anodeMeasurementPosition.Contains(r.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList().ForEach(row => row.Delete());

            dt.AcceptChanges();

            InitializeMaterialChart(_xMinLength, _xMaxLength);
            
            var query = (from t in dt.AsEnumerable()
                         select new
                         {
                             MeasurementPosition = t.Field<string>("EQPT_MEASR_PSTN_ID"),
                             MaterialDescription = t.Field<string>("MATERIAL_DESC")
                         }).Distinct().ToList();

            if (query.Any())
            {
                foreach (var item in query)
                {
                    var queryMaterial = (from t in dt.AsEnumerable()
                                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == item.MeasurementPosition
                                         select new
                                         {
                                             StartPosition = t.Field<decimal?>("SOURCE_STRT_PTN_PSTN"),
                                             EndPosition = t.Field<decimal?>("SOURCE_END_PTN_PSTN"),
                                             InputLotId = t.Field<string>("INPUT_LOTID"),
                                             ColorMap = t.Field<string>("COLORMAP"),
                                             Side = t.Field<string>("SIDE"),
                                             AdjLotId = t.Field<string>("ADJ_LOTID"),
                                             MeasurementPosition = t.Field<string>("EQPT_MEASR_PSTN_ID"),
                                             MaterialDescription = t.Field<string>("MATERIAL_DESC"),
                                             AdjStartPosition = t.Field<decimal?>("ADJ_STRT_PTN_PSTN"),
                                             AdjEndPosition = t.Field<decimal?>("ADJ_END_PTN_PSTN"),
                                         }).ToList();

                    if (queryMaterial.Any())
                    {
                        foreach (var row in queryMaterial)
                        {
                            if (string.IsNullOrEmpty(row.StartPosition.GetString())) continue;
                            var convertFromString = ColorConverter.ConvertFromString(row.ColorMap);

                            AlarmZone alarmZone = new AlarmZone
                            {
                                Near = row.StartPosition.GetDouble(),
                                Far = row.EndPosition.GetDouble(),
                                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                LowerExtent = 0,
                                UpperExtent = 100,
                                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                Cursor = Cursors.Hand
                            };

                            alarmZone.PlotElementLoaded += (s, e) =>
                            {
                                PlotElement pe = (PlotElement)s;
                                if (pe is Lines)
                                {
                                    if (!string.IsNullOrEmpty(row.StartPosition.GetString()) && !string.IsNullOrEmpty(row.EndPosition.GetString()))
                                    {
                                        double length = row.EndPosition.GetDouble() - row.StartPosition.GetDouble() > 0 ? row.EndPosition.GetDouble() - row.StartPosition.GetDouble() : 0;

                                        string content = ObjectDic.Instance.GetObjectName("길이") + "  : " + $"{length:#,##0.#}" + ObjectDic.Instance.GetObjectName("EA") + " (" + Util.NVC($"{row.AdjStartPosition:###,###,###,##0.##}") + " ~ " + Util.NVC($"{row.AdjEndPosition:###,###,###,##0.##}") + ")";
                                        ToolTipService.SetToolTip(pe, content);
                                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                        ToolTipService.SetShowDuration(pe, 60000);
                                    }
                                }
                            };

                            if (_polarityCode == "C")
                            {
                                switch (item.MeasurementPosition)
                                {
                                    case "INS_SLURRY_TOP":
                                        chartMaterialCathode1.Data.Children.Add(alarmZone);
                                        break;
                                    case "SLURRY_TOP_L2":
                                        chartMaterialCathode2.Data.Children.Add(alarmZone);
                                        break;
                                    case "SLURRY_TOP_L1":
                                        chartMaterialCathode3.Data.Children.Add(alarmZone);
                                        break;
                                    case "UW":
                                        chartMaterialCathode4.Data.Children.Add(alarmZone);
                                        break;
                                    case "SLURRY_BACK_L1":
                                        chartMaterialCathode5.Data.Children.Add(alarmZone);
                                        break;
                                    case "SLURRY_BACK_L2":
                                        chartMaterialCathode6.Data.Children.Add(alarmZone);
                                        break;
                                    case "INS_SLURRY_BACK":
                                        chartMaterialCathode7.Data.Children.Add(alarmZone);
                                        break;
                                }
                            }
                            else//_polarityCode == "A"
                            {
                                switch (item.MeasurementPosition)
                                {
                                    case "SLURRY_TOP_L2":
                                        chartMaterialAnode1.Data.Children.Add(alarmZone);
                                        break;
                                    case "SLURRY_TOP_L1":
                                        chartMaterialAnode2.Data.Children.Add(alarmZone);
                                        break;
                                    case "UW":
                                        chartMaterialAnode3.Data.Children.Add(alarmZone);
                                        break;
                                    case "SLURRY_BACK_L1":
                                        chartMaterialAnode4.Data.Children.Add(alarmZone);
                                        break;
                                    case "SLURRY_BACK_L2":
                                        chartMaterialAnode5.Data.Children.Add(alarmZone);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            DataTable dtMaterial = MakeTableForDisplay(dt, ChartDisplayType.Material);
            DataTable copyTable = dtMaterial.Clone();

            if (CommonVerify.HasTableRow(dtMaterial))
            {
                foreach (DataRow row in dtMaterial.Rows)
                {
                    if (string.IsNullOrEmpty(row["INPUT_LOTID"].GetString())) continue;

                    C1Chart c1Chart;
                    if (_polarityCode == "C")
                    {
                        if (row["EQPT_MEASR_PSTN_ID"].GetString() == "INS_SLURRY_TOP")
                            c1Chart = chartMaterialCathode1;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_TOP_L2")
                            c1Chart = chartMaterialCathode2;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_TOP_L1")
                            c1Chart = chartMaterialCathode3;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "UW")
                            c1Chart = chartMaterialCathode4;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_BACK_L1")
                            c1Chart = chartMaterialCathode5;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_BACK_L2")
                            c1Chart = chartMaterialCathode6;
                        else //if (row["EQPT_MEASR_PSTN_ID"].GetString() == "INS_SLURRY_BACK")
                            c1Chart = chartMaterialCathode7;
                    }
                    else
                    {
                        if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_TOP_L2")
                            c1Chart = chartMaterialAnode1;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_TOP_L1")
                            c1Chart = chartMaterialAnode2;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "UW")
                            c1Chart = chartMaterialAnode3;
                        else if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_BACK_L1")
                            c1Chart = chartMaterialAnode4;
                        else //if (row["EQPT_MEASR_PSTN_ID"].GetString() == "SLURRY_BACK_L2")
                            c1Chart = chartMaterialAnode5;
                    }

                    copyTable.ImportRow(row);

                    XYDataSeries dsMaterial = new XYDataSeries();
                    dsMaterial.ItemsSource = DataTableConverter.Convert(copyTable);
                    dsMaterial.XValueBinding = new Binding("SOURCE_AVG_VALUE");
                    dsMaterial.ValueBinding = new Binding("SOURCE_Y_PSTN");
                    dsMaterial.ChartType = ChartType.XYPlot;
                    dsMaterial.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    dsMaterial.SymbolFill = new SolidColorBrush(Colors.Transparent);
                    dsMaterial.PointLabelTemplate = grdMain.Resources["chartMaterial"] as DataTemplate;
                    dsMaterial.Margin = new Thickness(0);

                    dsMaterial.PlotElementLoaded += (s, e) =>
                    {
                        PlotElement pe = (PlotElement)s;
                        pe.Stroke = new SolidColorBrush(Colors.Transparent);
                        pe.Fill = new SolidColorBrush(Colors.Transparent);
                        // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                    };
                    c1Chart.Data.Children.Add(dsMaterial);

                    copyTable.Rows.RemoveAt(0);
                }
            }


        }
        #endregion

        private void DrawRollMapLane(DataTable dt)
        {
            /*
            #region [테스트 목적 무지부 강제 생성 테스트 후 삭제 예정]

            var convertFromString = ColorConverter.ConvertFromString("#D5D5D5");

            AlarmZone alarmZone = new AlarmZone
            {
                Near = 0,
                Far = _xMaxLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 99,
                UpperExtent = 100,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            chartCoater.Data.Children.Insert(0, alarmZone);

            AlarmZone alarmZone1 = new AlarmZone
            {
                Near = 0,
                Far = _xMaxLength,
                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                LowerExtent = 0,
                UpperExtent = 1,
                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
            };
            chartCoater.Data.Children.Insert(0, alarmZone1);

            #endregion
            */

            
            //무지부 색상 
            var convertFromString = ColorConverter.ConvertFromString("#D5D5D5");
            const double axisXnear = 0;         //x축 시작점
            double axisXfar = _xMaxLength;      //x축 종료점

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Edge Lane 영역 표시
                if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Edge" || dt.Rows[i]["COATING_PATTERN"].GetString() == "Plain")
                {
                    AlarmZone alarmZone = new AlarmZone();
                    alarmZone.Near = axisXnear;
                    alarmZone.Far = axisXfar;
                    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                    alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    chartCoater.Data.Children.Add(alarmZone);
                }

                //if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Plain")
                //{
                //    AlarmZone alarmZone = new AlarmZone();
                //    alarmZone.Near = axisXnear;
                //    alarmZone.Far = axisXfar;
                //    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                //    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                //    alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                //    alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                //    chartCoater.Data.Children.Add(alarmZone);
                //}

                // 유지부 BackGround 지정
                //else if (dt.Rows[i]["COATING_PATTERN"].GetString() == "Coat")
                //{
                //    // 1. 유지부 BackGround 지정
                //    AlarmZone alarmZone = new AlarmZone();
                //    alarmZone.Near = axisXnear;
                //    alarmZone.Far = axisXfar;
                //    alarmZone.UpperExtent = dt.Rows[i]["Y_PSTN_STRT_RATE"].GetDouble();
                //    alarmZone.LowerExtent = dt.Rows[i]["Y_PSTN_END_RATE"].GetDouble();
                //    alarmZone.ConnectionStroke = new SolidColorBrush(Colors.Transparent);
                //    alarmZone.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                //    chartCoater.Data.Children.Add(alarmZone);
                //}
                //else
                //{
                //    //dt.Rows[i]["COATING_PATTERN"].GetString() == "Edge"
                //}
            }
            

            _dtLane?.Clear();
            var queryLane = (from t in dt.AsEnumerable() where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" select new { LaneNo = t.Field<string>("LANE_NO_CUR") }).Distinct().ToList();
            if (queryLane.Any())
            {
                foreach (var item in queryLane)
                {
                    DataRow dr = _dtLane.NewRow();
                    dr["LANE_NO"] = item.LaneNo;
                    _dtLane.Rows.Add(dr);
                }
            }

            //var queryLane = (from t in dt.AsEnumerable() where t.Field<string>("LANE_NO_CUR") != null && t.Field<string>("LOC2") != "XX" select new { LaneNo = t.Field<string>("LANE_NO_CUR") }).Distinct().ToList();
            int laneCount = queryLane.Count;

            var queryTopBack = (from t in dt.AsEnumerable() select new { TopBackFlag = t.Field<string>("T_B_FLAG") }).Distinct().ToList();
            int topBackCount = queryTopBack.Count;

            if(queryLane.Any() && queryTopBack.Any())
            {

                foreach (var item in queryTopBack)
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
                            DrawLaneText(queryTopBackLaneRate.FirstTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LastTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LaneNo);
                        }
                    }
                }
            }
            
        }


        private void DrawLoading(DataTable dt)
        {
            /*
            string[] topBackArray;
            if ((bool)rdoWebgauge?.IsChecked)
                topBackArray = new string[] { "THICK_AVG_TOP_2", "WB_AVG_TOP_2" };
            else
                topBackArray = new string[] { "THICK_AVG_BACK_2", "WB_AVG_BACK_2" };
            */

            // 로딩량, 두께 영역 표시는 표현하는 영역 분류가 없으므로, Back 영역 만 표현 처리 함. 추후 변경 요청 사항에 따라 수정 가능 함.
            string[] gaugeArray; 

            if(rdoThickness.IsChecked != null && (bool)rdoThickness.IsChecked)
            {
                //gaugeArray = new string[] { "THICK_AVG_TOP_1", "THICK_AVG_TOP_2", "THICK_AVG_BACK_1", "THICK_AVG_BACK_2" };
                gaugeArray = new string[] { "THICK_AVG_BACK_1", "THICK_AVG_BACK_2" };
            }
            else
            {
                //gaugeArray = new string[] { "WB_AVG_TOP_1", "WB_AVG_TOP_2", "WB_AVG_BACK_1", "WB_AVG_BACK_2" };
                gaugeArray = new string[] { "WB_AVG_BACK_1", "WB_AVG_BACK_2" };
            }

            var queryWebGauge = dt.AsEnumerable().Where(o => gaugeArray.Contains(o.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList();
            DataTable dtWebGauge = queryWebGauge.Any() ? queryWebGauge.CopyToDataTable() : dt.Clone();

            if (!CommonVerify.HasTableRow(dtWebGauge)) return;

            chartLoading.View.AxisX.Min = 0;
            chartLoading.View.AxisX.Max = _xMaxLength;
            chartLoading.ChartType = _isRollMapPress ? ChartType.XYPlot:ChartType.Line;

            dtWebGauge.Columns.Add(new DataColumn { ColumnName = "ADJ_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
            foreach (DataRow row in dtWebGauge.Rows)
            {
                row["ADJ_AVG_VALUE"] = (row["ADJ_STRT_PTN_PSTN"].GetDouble() + row["ADJ_END_PTN_PSTN"].GetDouble()) / 2;
            }
            dtWebGauge.AcceptChanges();
            DrawLoadingLegend(dtWebGauge.Copy(), _dtLineLegend.Copy());

            if (CommonVerify.HasTableRow(dtWebGauge))
            {
                DataTable dtLineLegend = _dtLineLegend.Select().AsEnumerable().OrderByDescending(o => o.Field<int>("NO")).CopyToDataTable();

                var queryLaneInfo = (from t in _dtLane.AsEnumerable()
                                     join x in _dtLaneLegend.AsEnumerable()
                                     on t.Field<string>("LANE_NO") equals x.Field<string>("LANE_NO")
                                     select new
                                     {
                                         LaneNo = t.Field<string>("LANE_NO"),
                                         LaneColor = x.Field<string>("COLORMAP")
                                     }
                    ).ToList();

                foreach (DataRow row in _dtLineLegend.Rows)
                {
                    string valueText = "VALUE_" + row["VALUE"].GetString();

                    if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                    {
                        if (dtWebGauge.AsEnumerable().All(p => p.Field<decimal?>(valueText) == null)) continue;

                        var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                        DataSeries dsLegend = new DataSeries();
                        dsLegend.ItemsSource = new[] { "LL", "L", "SV", "H", "HH" };
                        dsLegend.ChartType = ChartType.Line;  //_isRollMapPress? ChartType.XYPlot : ChartType.Line; 
                        dsLegend.ValuesSource = GetLineValuesSource(dtWebGauge, row["VALUE"].GetString());
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

                        chartLoading.Data.Children.Add(dsLegend);
                    }
                }

                if (queryLaneInfo.Any() && CommonVerify.HasTableRow(_dtLane))
                {
                    foreach (var item in queryLaneInfo)
                    {
                        var queryLane = _dtLane.AsEnumerable().Where(where => @where.Field<string>("LANE_NO").Equals(item.LaneNo)).ToList();
                        if (!queryLane.Any()) continue;

                        var query = dtWebGauge.Select("ADJ_LANE_NO = '" + item.LaneNo + "'").AsEnumerable().ToList();
                        DataTable dtLine = query.Any() ? query.CopyToDataTable() : dtWebGauge.Clone();

                        var laneConvertFromString = ColorConverter.ConvertFromString(Util.NVC(item.LaneColor));

                        XYDataSeries ds = new XYDataSeries();
                        ds.ItemsSource = DataTableConverter.Convert(dtLine);
                        ds.XValueBinding = new Binding("ADJ_AVG_VALUE");
                        ds.ValueBinding = new Binding("SCAN_AVG_VALUE");
                        ds.ChartType = _isRollMapPress ? ChartType.XYPlot : ChartType.LineSymbols; 
                        ds.ConnectionFill = new SolidColorBrush(Colors.Black);
                        ds.SymbolFill = laneConvertFromString != null ? new SolidColorBrush((Color)laneConvertFromString) : null;
                        ds.Cursor = Cursors.Hand;
                        ds.SymbolSize = new Size(6, 6);
                        ds.ConnectionStrokeThickness = 0.8;

                        chartLoading.Data.Children.Add(ds);

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
                                double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_STRT_PTN_PSTN")), out sourceStart);
                                double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_END_PTN_PSTN")), out sourceEnd);

                                //string content = "Load : ";
                                string content = rdoThickness.IsChecked != null && (bool)rdoThickness.IsChecked ? "Thickness : " : "Load : ";
                                content = content + $"{scanAvgValue:###,###,###,##0.##}" + "[" + $"{sourceStart:###,###,###,##0.##}" + "~" + $"{sourceEnd:###,###,###,##0.##}" + "]";

                                ToolTipService.SetToolTip(pe, content);
                                ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                ToolTipService.SetShowDuration(pe, 60000);
                            }
                        };
                    }
                }
                InitializeChartView(chartLoading);
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

        private void DrawLoadingLegend(DataTable dt, DataTable dtLegend)
        {
            if (grdLoadingLegend.ColumnDefinitions.Count > 0) grdLoadingLegend.ColumnDefinitions.Clear();
            if (grdLoadingLegend.RowDefinitions.Count > 0) grdLoadingLegend.RowDefinitions.Clear();

            grdLoadingLegend.Children.Clear();

            DataTable copyTable = dtLegend.Clone();
            copyTable.Columns.Add(new DataColumn { ColumnName = "VALUE_AVG", DataType = typeof(double) });

            int rowIndex = 0;

            foreach (DataRow row in dtLegend.Rows)
            {
                string valueText = "VALUE_" + row["VALUE"];

                if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                {
                    double agvValue = 0;
                    var query = (from t in dt.AsEnumerable() where t.Field<decimal?>(valueText) != null select new { Valuecol = t.Field<decimal>(valueText) }).ToList();
                    if (query.Any())
                        agvValue = query.Max(r => r.Valuecol).GetDouble();

                    DataRow dr = copyTable.NewRow();
                    dr["NO"] = rowIndex;
                    dr["COLORMAP"] = row["COLORMAP"];
                    dr["VALUE"] = row["VALUE"];
                    dr["VALUE_AVG"] = agvValue;
                    copyTable.Rows.Add(dr);

                    rowIndex++;
                }
            }

            var j = 0;

            for (int i = 0; i < copyTable.Rows.Count; i++)
            {
                RowDefinition gridRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };
                grdLoadingLegend.RowDefinitions.Add(gridRow);

                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2),
                };

                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(copyTable.Rows[i]["COLORMAP"]));
                string rowValue = copyTable.Rows[i]["VALUE"] + " : " + copyTable.Rows[i]["VALUE_AVG"].GetDecimal();

                TextBlock loadingLegendDescription = CreateTextBlock(rowValue,
                                                                    HorizontalAlignment.Center,
                                                                    VerticalAlignment.Center,
                                                                    11,
                                                                    FontWeights.Bold,
                                                                    convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                    new Thickness(1),
                                                                    new Thickness(0),
                                                                    null,
                                                                    null,
                                                                    null);

                stackPanel.Children.Add(loadingLegendDescription);

                Grid.SetRow(stackPanel, j);
                Grid.SetColumn(stackPanel, 0);

                grdLoadingLegend.Children.Add(stackPanel);
                j++;

            }
        }

        private void DrawLoadingLaneLegend()
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
                         }).OrderByDescending(o => Convert.ToInt32(o.LaneNo)).ToList();

            //sizes.OrderBy(x => Convert.ToInt32(x)).ToList<string>();

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
                        Margin = new Thickness(2)
                    };

                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(item.LaneColor));

                    Ellipse ellipseLane = CreateEllipse(HorizontalAlignment.Center,
                        VerticalAlignment.Center,
                        12,
                        12,
                        convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                        1,
                        new Thickness(2),
                        null,
                        null);

                    TextBlock textBlockLane = CreateTextBlock("Lane " + item.LaneNo,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Center,
                        11,
                        FontWeights.Bold,
                        Brushes.Black,
                        new Thickness(1),
                        new Thickness(0),
                        null,
                        Cursors.Hand,
                        null);

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

        /// <summary>
        /// chartInput, chartOutput 영역에 Lane 표시
        /// </summary>
        /// <param name="firstlaneRate"></param>
        /// <param name="lastlaneRate"></param>
        /// <param name="laneNo"></param>
        /// <param name="chartConfigurationType"></param>
        private void DrawLaneText(double firstlaneRate, double lastlaneRate, string laneNo)
        {
            DataTable dtLane = new DataTable();
            dtLane.Columns.Add("SOURCE_STRT_PSTN", typeof(string));
            dtLane.Columns.Add("SOURCE_Y_PSTN", typeof(string));
            dtLane.Columns.Add("LANEINFO", typeof(string));

            C1Chart c1Chart = chartCoater;
            double startPosition = _xMaxLength - (_xMaxLength * 0.01);

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
            dsLane.Margin = new Thickness(0);

            dsLane.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };

            c1Chart.Data.Children.Add(dsLane);
        }

        private void DrawGauge(DataTable dt)
        {
            try
            {
                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Start();
                //System.Diagnostics.Debug.WriteLine("DrawGauge Start =======> : ");
                //    stopwatch.Stop();
                //    System.Diagnostics.Debug.WriteLine("DrawChartBackGround1 PlotElementLoaded Time =======> : " + stopwatch.Elapsed.ToString());

                //속도 개선을 위해 인스턴스 생성 선행
                _listgauge = new List<AlarmZone>();
                foreach (var item in dt.Rows)
                {
                    _listgauge.Add(new AlarmZone());
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dt.Rows[i]["COLORMAP"]));
                    _listgauge[i].Near = dt.Rows[i]["ADJ_STRT_PTN_PSTN"].GetDouble();
                    _listgauge[i].Far = dt.Rows[i]["ADJ_END_PTN_PSTN"].GetDouble();
                    _listgauge[i].LowerExtent = dt.Rows[i]["CHART_Y_PSTN_STRT_RATE"].GetDouble();
                    _listgauge[i].UpperExtent = dt.Rows[i]["CHART_Y_PSTN_END_RATE"].GetDouble();
                    _listgauge[i].ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    _listgauge[i].ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    _listgauge[i].Cursor = Cursors.Hand;

                    int x = i;
                    _listgauge[i].PlotElementLoaded += (s, e) =>
                    {
                        PlotElement pe = (PlotElement)s;
                        if (pe is Lines)
                        {
                            double sourceStartPosition;
                            double sourceEndPosition;
                            double.TryParse(Util.NVC(dt.Rows[x]["SOURCE_STRT_PTN_PSTN"]), out sourceStartPosition);
                            double.TryParse(Util.NVC(dt.Rows[x]["SOURCE_END_PTN_PSTN"]), out sourceEndPosition);

                            string content = ObjectDic.Instance.GetObjectName("LANE") + " : #" + dt.Rows[x]["ADJ_LANE_NO"].GetString() + " " + dt.Rows[x]["CMCDNAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                             "Scan AVG : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                                             "ColorMap : " + Util.NVC(dt.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                             "Offset : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}");

                            ToolTipService.SetToolTip(pe, content);
                            ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                            ToolTipService.SetShowDuration(pe, 60000);
                        }
                    };
                    chartCoater.Data.Children.Add(_listgauge[i]);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DrawStartEndYAxis(DataTable dt)
        {
            string[] petScrap = new string[] { "PET", "SCRAP" };
            var query = (from t in dt.AsEnumerable() where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
            // CUT, Rewinder 길이 표시
            if(query.Any())
            {
                DataTable dtMaxLength = new DataTable();
                dtMaxLength.Columns.Add("RAW_END_PTN_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_END_PTN_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_Y_PSTN", typeof(double));
                dtMaxLength.Columns.Add("TOOLTIP", typeof(string));
                dtMaxLength.Columns.Add("FONTSIZE", typeof(string));

                DataRow newRow = dtMaxLength.NewRow();
                newRow["RAW_END_PTN_PSTN"] = "0";
                newRow["SOURCE_END_PTN_PSTN"] = "0";
                newRow["SOURCE_Y_PSTN"] = 100;
                newRow["TOOLTIP"] = string.Empty;
                newRow["FONTSIZE"] = 11;
                dtMaxLength.Rows.Add(newRow);

                var queryLength = (from t in dt.AsEnumerable()
                                   where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                   select new
                                   {
                                       RawEndPosition = t.Field<decimal?>("RAW_END_PTN_PSTN"),
                                       RawStartPosition = t.Field<decimal?>("RAW_STRT_PTN_PSTN"),
                                       EndPosition = t.Field<decimal?>("SOURCE_END_PTN_PSTN"),
                                       RowNum = t.Field<Int16>("ROW_NUM"),
                                       MeasurementCode = t.Field<string>("EQPT_MEASR_PSTN_ID")
                                   }).ToList();

                if (queryLength.Any())
                {
                    int rowIndex = 0;

                    foreach (var item in queryLength)
                    {
                        DataRow newLength = dtMaxLength.NewRow();

                        newLength["RAW_END_PTN_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_END_PTN_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_Y_PSTN"] = 100;
                        newLength["TOOLTIP"] = item.RowNum + " Cut" + "( " + $"{item.RawStartPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + " ~ " + $"{item.RawEndPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + ")";
                        newLength["FONTSIZE"] = queryLength.Count.Equals(rowIndex + 1) ? 20 : 11;
                        dtMaxLength.Rows.Add(newLength);

                        rowIndex++;
                    }

                    XYDataSeries ds = new XYDataSeries();
                    ds.ItemsSource = DataTableConverter.Convert(dtMaxLength);
                    ds.XValueBinding = new Binding("SOURCE_END_PTN_PSTN");
                    ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                    ds.ChartType = ChartType.XYPlot;
                    ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                    ds.PointLabelTemplate = grdMain.Resources["chartLength"] as DataTemplate;
                    ds.Margin = new Thickness(0);
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

            // Sample 길이 및 Patn 표시
            var querySample = (from t in dt.AsEnumerable() where t.Field<string>("SMPL_FLAG") == "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
            if (querySample.Any())
            {
                foreach (var item in querySample)
                {
                    chartCoater.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { item["SOURCE_END_PTN_PSTN"].GetDouble(), item["SOURCE_END_PTN_PSTN"].GetDouble() },
                        ValuesSource = new double[] { 0, 100 },
                        ConnectionStroke = new SolidColorBrush(Colors.DarkRed),
                    });
                }

                DataTable dtSample = MakeTableForDisplay(querySample.CopyToDataTable().Copy(), ChartDisplayType.Sample);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtSample);
                ds.XValueBinding = new Binding("SOURCE_END_PTN_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartSample"] as DataTemplate;
                ds.Margin = new Thickness(0);
                ds.Cursor = Cursors.Hand;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                chartCoater.Data.Children.Add(ds);

                _sampleCutLength = dtSample.AsEnumerable().Where(x => x.Field<string>("SMPL_FLAG") == "Y").ToList().Sum(r => r["RAW_END_PTN_PSTN"].GetDouble()).GetDouble();
            }
            else
            {
                _sampleCutLength = 0;
            }
        }

        private void DrawDefect(DataTable dt, bool isReDraw = false)
        {
            var queryVisionSurfaceTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")).ToList();
            DataTable dtVisionSurfaceTop = queryVisionSurfaceTop.Any() ? queryVisionSurfaceTop.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtVisionSurfaceTop))
            {
                dtVisionSurfaceTop.TableName = "VISION_SURF_NG_TOP";
                DrawDefectVisionSurface(dtVisionSurfaceTop);
            }

            var queryVisionSurfaceBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")).ToList();
            DataTable dtVisionSurfaceBack = queryVisionSurfaceBack.Any() ? queryVisionSurfaceBack.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtVisionSurfaceBack))
            {
                dtVisionSurfaceBack.TableName = "VISION_SURF_NG_BACK";
                DrawDefectVisionSurface(dtVisionSurfaceBack);
            }

            var queryVisionTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")).ToList();
            DataTable dtVisionTop = queryVisionTop.Any() ? queryVisionTop.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtVisionTop))
            {
                dtVisionTop.TableName = "VISION_NG_TOP";
                DrawDefectAlignVision(dtVisionTop);
            }

            var queryVisionBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")).ToList();
            DataTable dtVisionBack = queryVisionBack.Any() ? queryVisionBack.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtVisionBack))
            {
                dtVisionBack.TableName = "VISION_NG_BACK";
                DrawDefectAlignVision(dtVisionBack);
            }

            var queryAlignVisionTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")).ToList();
            DataTable dtAlignVisionTop = queryAlignVisionTop.Any() ? queryAlignVisionTop.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtAlignVisionTop))
            {
                dtAlignVisionTop.TableName = "INS_ALIGN_VISION_NG_TOP";
                DrawDefectAlignVision(dtAlignVisionTop);
            }

            var queryAlignVisionBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")).ToList();
            DataTable dtAlignVisionBack = queryAlignVisionBack.Any() ? queryAlignVisionBack.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtAlignVisionBack))
            {
                dtAlignVisionBack.TableName = "INS_ALIGN_VISION_NG_BACK";
                DrawDefectAlignVision(dtAlignVisionBack);
            }

            var queryOverLayVision = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
            DataTable dtOverLayVision = queryOverLayVision.Any() ? queryOverLayVision.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtOverLayVision))
            {
                dtOverLayVision.TableName = "INS_OVERLAY_VISION_NG";
                DrawDefectAlignVision(dtOverLayVision);
            }

            if (!isReDraw)
            {
                var queryMark = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("MARK")).ToList();
                DataTable dtMark = queryMark.Any() ? queryMark.CopyToDataTable() : dt.Clone();

                if (CommonVerify.HasTableRow(dtMark))
                {
                    dtMark.TableName = "dtMark";
                    DrawRollMapMark(dtMark);
                }

                var queryTagSection = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dt.Clone();
                dtTagSection.TableName = "TAG_SECTION";
                _dtTagSection = dtTagSection;
                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawRollMapTagSection(dtTagSection);
                }

                var queryTagSpot = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dt.Clone();

                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    DrawRollMapTagSpot(dtTagSpot);
                }
            }

        }

        private void DrawDefectLegend(DataTable dt)
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
                DrawDefectLegendText(dtDefectLegend);
            }
        }

        private void DrawDefectLegendText(DataTable dt)
        {
            try
            {
                if (CommonVerify.HasTableRow(dt))
                {
                    Grid gridLegend = grdDefectLegend;

                    if (gridLegend.ColumnDefinitions.Count > 0) gridLegend.ColumnDefinitions.Clear();
                    if (gridLegend.RowDefinitions.Count > 0) gridLegend.RowDefinitions.Clear();

                    for (int x = 0; x < 2; x++)
                    {
                        ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                        gridLegend.ColumnDefinitions.Add(gridColumn);
                    }
                    gridLegend.Children.Clear();

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
                                Margin = new Thickness(2)
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
                                                                                new Thickness(2),
                                                                                null,
                                                                                null);

                                    TextBlock rectangleDescription = CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                                     HorizontalAlignment.Center,
                                                                                     VerticalAlignment.Center,
                                                                                     11,
                                                                                     FontWeights.Bold,
                                                                                     Brushes.Black,
                                                                                     new Thickness(1),
                                                                                     new Thickness(0),
                                                                                     item.MeasurementCode,
                                                                                     Cursors.Hand,
                                                                                     //chartConfigurationType.ToString() + "|" + item.ColorMap);
                                                                                     item.ColorMap);
                                    stackPanel.Children.Add(rectangleLegend);
                                    stackPanel.Children.Add(rectangleDescription);

                                    rectangleDescription.PreviewMouseUp += DefectDescriptionOnPreviewMouseUp;
                                    break;

                                case "ELLIPSE":
                                    Ellipse ellipseLegend = CreateEllipse(HorizontalAlignment.Center,
                                                                          VerticalAlignment.Center,
                                                                          12,
                                                                          12,
                                                                          convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                          1,
                                                                          new Thickness(2),
                                                                          null,
                                                                          null);

                                    TextBlock ellipseDescription = CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                                   HorizontalAlignment.Center,
                                                                                   VerticalAlignment.Center,
                                                                                   11,
                                                                                   FontWeights.Bold,
                                                                                   Brushes.Black,
                                                                                   new Thickness(1),
                                                                                   new Thickness(0),
                                                                                   item.MeasurementCode,
                                                                                   Cursors.Hand,
                                                                                   item.ColorMap);
                                    stackPanel.Children.Add(ellipseLegend);
                                    stackPanel.Children.Add(ellipseDescription);

                                    ellipseDescription.PreviewMouseUp += DefectDescriptionOnPreviewMouseUp;
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
        /// 최종 비전 표면불량 Top, Back
        /// </summary>
        /// <param name="dt"></param>
        private void DrawDefectVisionSurface(DataTable dt)
        {
            XYDataSeries ds = new XYDataSeries();
            ds.ItemsSource = DataTableConverter.Convert(dt);
            ds.ValueBinding = new Binding("Y_PSTN_RATE");
            //ds.XValueBinding = new Binding("ADJ_X_PSTN");
            ds.XValueBinding = new Binding("ADJ_STRT_PTN_PSTN");
            ds.Cursor = Cursors.Hand;
            ds.SymbolSize = new Size(7, 7);
            ds.Tag = dt.TableName;

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
                    double sourceStartPosition;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_X_PSTN")), out xPosition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_Y_PSTN")), out yPosition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "TOP_COATING_PTN_LEN")), out topPattern);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "BACK_COATING_PTN_LEN")), out backPattern);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_STRT_PTN_PSTN")), out sourceStartPosition);

                    string defectName = DataTableConverter.GetValue(dp.DataObject, "EQPT_MEASR_PSTN_NAME").GetString();

                    string content = defectName + Environment.NewLine +
                                     $"{sourceStartPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + "(" + $"{xPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + ")"; ;

                    ToolTipService.SetToolTip(pe, content);
                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                    ToolTipService.SetShowDuration(pe, 60000);
                }
            };
            chartCoater.Data.Children.Add(ds);
        }

        /// <summary>
        /// 최종 비전 불량(Top, Back) , 절연코팅 Align 비전 불량(Top, Back)
        /// </summary>
        /// <param name="dt"></param>
        private void DrawDefectAlignVision(DataTable dt)
        {
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
                    Tag = dt.TableName
                };

                alarmZone.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    if (pe is Lines)
                    {
                        double startPosition;
                        double endPosition;
                        double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PTN_PSTN"]), out startPosition);
                        double.TryParse(Util.NVC(row["SOURCE_ADJ_END_PTN_PSTN"]), out endPosition);
                        string content = row["ABBR_NAME"] + "[" + $"{startPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + "~" + $"{endPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + "]";
                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                chartCoater.Data.Children.Add(alarmZone);
            }
        }

        private void DrawRollMapMark(DataTable dt)
        {
            // 기준점 종축 라인 설정
            foreach (DataRow row in dt.Rows)
            {
                double sourceStartPosition;
                double adjStartPosition;
                double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PTN_PSTN"]), out sourceStartPosition);
                double.TryParse(Util.NVC(row["ADJ_STRT_PSTN"]), out adjStartPosition);

                string content = ObjectDic.Instance.GetObjectName("LANE") + " : #" + row["ADJ_LANE_NO"].GetString() + Environment.NewLine +
                                 $"{sourceStartPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + "(" + $"{adjStartPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + ")";


                chartCoater.Data.Children.Add(new XYDataSeries()
                {
                    ChartType = ChartType.Line,
                    XValuesSource = new[] { row["ADJ_STRT_PTN_PSTN"].GetDouble(), row["ADJ_STRT_PTN_PSTN"].GetDouble() },
                    ValuesSource = new double[] { row["CHART_Y_STRT_CUM_RATE"].GetDouble(), row["CHART_Y_END_CUM_RATE"].GetDouble() },
                    ConnectionStroke = new SolidColorBrush(Colors.Black),
                    ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                    ToolTip = content,
                    Tag = dt.TableName
                });
            }

            // 기준점 라벨 설정
            DataTable dtLabel = MakeTableForDisplay(dt, ChartDisplayType.Mark);
            var dsMarkLabel = new XYDataSeries();
            dsMarkLabel.ItemsSource = DataTableConverter.Convert(dtLabel);
            dsMarkLabel.XValueBinding = new Binding("ADJ_STRT_PTN_PSTN");
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
            chartCoater.Data.Children.Add(dsMarkLabel);
        }

        private void DrawRollMapTagSection(DataTable dtTagPosition)
        {
            // Start, End 라벨 삽입
            if (CommonVerify.HasTableRow(dtTagPosition))
            {
                if (!dtTagPosition.Columns.Contains("IsEnabled"))
                {
                    dtTagPosition.Columns.Add(new DataColumn() { ColumnName = "IsEnabled", DataType = typeof(bool) });
                }

                foreach (DataRow row in dtTagPosition.Rows)
                {
                    row["IsEnabled"] = (_isRollMapResultLink && _isRollMapLot) ? false : true;
                }
                dtTagPosition.AcceptChanges();

                // Start, End 이미지 두번의 표현으로 for문 사용
                for (int x = 0; x < 2; x++)
                {
                    DataTable dtTag = MakeTableForDisplay(dtTagPosition, x == 0 ? ChartDisplayType.TagSectionStart : ChartDisplayType.TagSectionEnd);

                    XYDataSeries ds = new XYDataSeries();
                    ds.ItemsSource = DataTableConverter.Convert(dtTag);
                    ds.XValueBinding = x == 0 ? new Binding("ADJ_STRT_PTN_PSTN") : new Binding("ADJ_END_PTN_PSTN");
                    ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                    ds.ChartType = ChartType.XYPlot;
                    ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                    ds.PointLabelTemplate = grdMain.Resources["chartSectionTag"] as DataTemplate;
                    ds.Margin = new Thickness(0);
                    ds.Tag = dtTagPosition.TableName;

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

        private void DrawRollMapTagSpot(DataTable dtTagSpot)
        {
            if (CommonVerify.HasTableRow(dtTagSpot))
            {
                dtTagSpot = MakeTableForDisplay(dtTagSpot, ChartDisplayType.TagSpot);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTagSpot);
                ds.XValueBinding = new Binding("ADJ_STRT_PTN_PSTN");
                ds.ValueBinding = new Binding("SOURCE_ADJ_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartSpotTag"] as DataTemplate;
                ds.Margin = new Thickness(0);

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                chartCoater.Data.Children.Add(ds);
            }
        }

        private void DefectDescriptionOnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null || textBlock.Tag == null || textBlock.Text == null) return;

                ShowLoadingIndicator();
                DoEvents();

                C1Chart c1Chart = chartCoater;
                DataTable dtDefect, dtBaseDefect;

                string colorMap = textBlock.Tag.GetString();
                dtDefect = _dtDefectScription;
                dtBaseDefect = _dtRollMapDefect;

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
                    dtDefect.AcceptChanges();
                }
                else
                {
                    dtDefect.AsEnumerable().Where(r => r.Field<string>("EQPT_MEASR_PSTN_ID") == textBlockText && r.Field<string>("ABBR_NAME") == textBlockText && r.Field<string>("COLORMAP") == colorMap).ToList().ForEach(row => row.Delete());
                    dtDefect.AcceptChanges();
                    textBlock.Foreground = Brushes.Black;
                }

                var queryDefect = dtBaseDefect.AsEnumerable().Where(x => !dtDefect.AsEnumerable().Any(y => y.Field<string>("ABBR_NAME") == x.Field<string>("ABBR_NAME") && y.Field<string>("COLORMAP") == x.Field<string>("COLORMAP")));
                if (queryDefect.Any())
                {
                    DrawDefect(queryDefect.CopyToDataTable(), true);
                }
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

                chartLoading.Data.Children.Clear();

                string laneNo = textBlock.Text.Replace("Lane", "").Trim();

                if (textBlock.Foreground == Brushes.Black)
                {
                    _dtLane.Rows.Remove(_dtLane.Select("LANE_NO = '" + laneNo + "'")[0]);
                    textBlock.Foreground = Brushes.LightGray;
                }

                else
                {
                    _dtLane.Rows.Add(laneNo);
                    textBlock.Foreground = Brushes.Black;
                }

                DrawLoading(_dtRollMapGauge);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DisplayTabLocation(DataTable dtTab)
        {
            if(CommonVerify.HasTableRow(dtTab))
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
                        // 무지부 Tab 방향을 상단 하단 모두 입력하는 경우가 있을 수 있어 query > queryTab 으로 변경 함.
                        var queryTab = (from t in dtTab.AsEnumerable()
                                        where !string.IsNullOrEmpty(t.Field<string>("TAB")) && t.Field<string>("TAB") == "1" && t.Field<string>("COATING_PATTERN") == "Plain"
                                        select t).ToList();

                        if(queryTab.Any())
                        {
                            foreach (var item in queryTab)
                            {
                                //if(dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO") == item["LANE_NO_CUR"].GetString() && x.Field<int>("RN") == (item["RN"].GetInt() - 1)))
                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<Int32>("RN") < item["RN"].GetInt() ))
                                {
                                    islower = true;
                                }

                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<Int32>("RN") > item["RN"].GetInt() ))
                                {
                                    isupper = true;
                                }
                            }

                            if(isupper)
                            {
                                tbTopupper.Text = "Tab";
                                tbBackupper.Text = "Tab";
                            }
                            if(islower)
                            {
                                tbToplower.Text = "Tab";
                                tbBacklower.Text = "Tab";
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

        private IEnumerable<double> GetLineValuesSource(DataTable dt, string value, int i)
        {
            try
            {
                var queryCount = _xMaxLength.GetInt() + 1;
                double[] xx = new double[queryCount];

                string valueText = "VALUE_" + value;

                if (dt.AsEnumerable().Any(p => p.Field<Int16>("SEQ") == i && p.Field<decimal?>(valueText) != null))
                {
                    double agvValue = 0;
                    var query = (from t in dt.AsEnumerable() where t.Field<decimal?>(valueText) != null && t.Field<Int16>("SEQ") == i select new { Valuecol = t.Field<decimal>(valueText) }).ToList();
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

        private void SetScale(double scale)
        {
            chartCoater.View.AxisX.Scale = scale;
            btnRefresh.IsCancel = !scale.Equals(1);
            btnZoomIn.IsCancel = scale > 0.002;
            btnZoomOut.IsCancel = scale < 1;

            UpdateScrollbars();
        }

        private void UpdateScrollbars()
        {
            double sxTop = chartCoater.View.AxisX.Scale;
            AxisScrollBar axisScrollBarCoater = (AxisScrollBar)chartCoater.View.AxisX.ScrollBar;
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

                if (c1Chart.Name == "chartCoater")
                {
                    if (!chartCoater.View.AxisX.Scale.Equals(1))
                        SetScale(1.0);
                    else
                        UpdateScrollbars();

                    c1Chart.View.AxisX.MinScale = 0.05;
                    //c1Chart.View.Margin = new Thickness(0, 0, 0, 20);
                    c1Chart.View.Margin = new Thickness(0);
                    c1Chart.Padding = new Thickness(10, 3, 3, 5);
                }

                if (c1Chart.Name == "chartLoading")
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
                if (string.Equals(_processCode, Process.COATING) && c1Chart.Name != "chart")
                    c1Chart.View.AxisX.Reversed = true;

                c1Chart.EndUpdate();
            }
        }

        /// <summary>
        /// 툴팁, Y좌표 강제 생성 .. 등을 위한 테이블 데이터 수정 및 생성
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chartDisplayType"></param>
        /// <returns></returns>
        private static DataTable MakeTableForDisplay(DataTable dt, ChartDisplayType chartDisplayType)
        {
            var dtBinding = dt.Copy();
            if (!CommonVerify.HasTableRow(dt)) return dtBinding;

            if (chartDisplayType == ChartDisplayType.Material)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "SOURCE_Y_PSTN", DataType = typeof(double), AllowDBNull = true });

                foreach (DataRow row in dtBinding.Rows)
                {
                    if (!string.IsNullOrEmpty(row["INPUT_LOTID"].GetString()))
                    {
                        row["SOURCE_AVG_VALUE"] = (row["SOURCE_END_PTN_PSTN"].GetDouble() + row["SOURCE_STRT_PTN_PSTN"].GetDouble()) / 2;
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
            else if(chartDisplayType == ChartDisplayType.Sample)
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
                    double.TryParse(Util.NVC(dtBinding.Rows[i]["RAW_END_PTN_PSTN"]), out rowEndPosition);

                    if (i == dtBinding.Rows.Count - 1)
                    {
                        double.TryParse(Util.NVC(dtBinding.Rows[i]["RAW_STRT_PTN_PSTN"]), out rowStartPosition);
                    }
                    else
                    {
                        double.TryParse(Util.NVC(dtBinding.Rows[i + 1]["RAW_STRT_PTN_PSTN"]), out rowStartPosition);
                    }

                    double.TryParse(Util.NVC(dtBinding.Rows[i]["RAW_STRT_PTN_PSTN"]), out toolTipStartPosition);

                    dtBinding.Rows[i]["TOOLTIP"] = dtBinding.Rows[i]["ROW_NUM"] + " Cut" + "( " + $"{toolTipStartPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + " ~ " + $"{rowEndPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + ")";
                    dtBinding.Rows[i]["END_PSTN"] = $"{rowEndPosition:###,###,###,##0.##}";
                    dtBinding.Rows[i]["SOURCE_Y_PSTN"] = 100;
                    dtBinding.Rows[i]["STRT_PSTN"] = $"{rowStartPosition:###,###,###,##0.##}";
                }
            }
            else if (chartDisplayType == ChartDisplayType.SurfaceTop || chartDisplayType == ChartDisplayType.SurfaceBack)
            {

            }
            else if (chartDisplayType == ChartDisplayType.VisionTop || chartDisplayType == ChartDisplayType.VisionBack)
            {

            }
            else if(chartDisplayType == ChartDisplayType.Mark)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition, adjStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PTN_PSTN"]), out sourceStartPosition);
                    double.TryParse(Util.NVC(row["ADJ_STRT_PSTN"]), out adjStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = -10;
                    row["TOOLTIP"] = ObjectDic.Instance.GetObjectName("LANE") + " : #" + row["ADJ_LANE_NO"].GetString() + Environment.NewLine +
                        $"{sourceStartPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + "(" + $"{adjStartPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + ")";

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
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -15 : -21;
                    row["TAG"] = $"{row["SOURCE_ADJ_STRT_PTN_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_ADJ_END_PTN_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();
                    //row["TOOLTIP"] = ObjectDic.Instance.GetObjectName("불량구분") + ": " + row["ABBR_NAME"].GetString() + Environment.NewLine +
                    //                 ObjectDic.Instance.GetObjectName("불량구간") + ": " + ObjectDic.Instance.GetObjectName("START") + ObjectDic.Instance.GetObjectName("PTN_NO") + "(" + $"{row["SOURCE_ADJ_STRT_PTN_PSTN"]:###,###,###,##0.##}" + ")" + " ~ " + ObjectDic.Instance.GetObjectName("END") + ObjectDic.Instance.GetObjectName("PTN_NO") + "(" + $"{row["SOURCE_ADJ_END_PTN_PSTN"]:###,###,###,##0.##}" + ")";

                    row["TOOLTIP"] = row["ABBR_NAME"].GetString() + Environment.NewLine +
                                     $"{row["SOURCE_ADJ_STRT_PTN_PSTN"]:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + "(" + $"{row["ADJ_STRT_PSTN"]:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + ")" + " ~ "  + $"{row["SOURCE_ADJ_END_PTN_PSTN"]:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("EA") + "(" + $"{row["ADJ_END_PSTN"]:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + ")";

                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
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
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PTN_PSTN"]), out sourceStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = -27;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"] + "[ " + sourceStartPosition + ObjectDic.Instance.GetObjectName("EA") + " ]";
                    row["TAG"] = null;


                    

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
            newRow["MENUID"] = "SFU010160121";          // 메뉴ID 코터:SFU010160121, 롤프레스:SFU010160122, 슬리터:SFU010160123, 리와인딩:SFU010160124            
            newRow["USERID"] = LoginInfo.USERID;
            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["EQSGID"] = _equipmentSegmentCode;   // LoginInfo.CFG_EQSG_ID;
            newRow["PROCID"] = _processCode;            // LoginInfo.CFG_PROC_ID;
            newRow["EQPTID"] = _equipmentCode;          // LoginInfo.CFG_EQPT_ID;
            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (menuHitReuslt, menuHitException) => { });

        }

        private void PopupTagSection()
        {
            #region [구간불량 팝업 호출]
            CMM_ROLLMAP_PSTN_UPD popRollMapPositionUpdate = new CMM_ROLLMAP_PSTN_UPD();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            double startSection;
            double endSection;

            if (_sampleCutLength > _selectedStartSection)
            {
                startSection = _selectedStartSection;
                endSection = _selectedEndSection;
            }
            else
            {
                startSection = _selectedStartSection - _sampleCutLength;
                endSection = _selectedEndSection - _sampleCutLength;
            }

            object[] parameters = new object[12];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = $"{startSection:###,###,###,##0.##}";
            parameters[2] = $"{endSection:###,###,###,##0.##}";
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
            #endregion
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

        private void SetCoaterChartContextMenuEnable(bool isEnabled)
        {
            for (int row = 0; row < chartCoater.ContextMenu.Items.Count; row++)
            {
                MenuItem item = chartCoater.ContextMenu.Items[row] as MenuItem;

                switch (item.Name.ToString())
                {
                    case "tagSectionStart":
                        item.PreviewMouseDown -= tagSection_PreviewMouseDown;
                        item.PreviewMouseDown += tagSection_PreviewMouseDown;
                        item.Click -= tagSectionStart_Click;
                        item.Click += tagSectionStart_Click;
                        item.IsEnabled = isEnabled;
                        break;

                    case "tagSectionEnd":
                        item.PreviewMouseDown -= tagSection_PreviewMouseDown;
                        item.PreviewMouseDown += tagSection_PreviewMouseDown;
                        item.Click -= tagSectionEnd_Click;
                        item.Click += tagSectionEnd_Click;
                        item.IsEnabled = isEnabled;
                        break;

                    case "tagSectionClear":
                        item.PreviewMouseDown -= tagSection_PreviewMouseDown;
                        item.PreviewMouseDown += tagSection_PreviewMouseDown;
                        item.Click -= tagSectionClear_Click;
                        item.Click += tagSectionClear_Click;
                        item.IsEnabled = isEnabled;
                        break;
                }
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
                if(CommonVerify.HasTableRow(dtResult))
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
            DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();

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
            DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();
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

                if(query != null)
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

                if(queryExpressSummary.Any())
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

        private bool ValidationSearch()
        {
            if (string.IsNullOrEmpty(txtLot.Text))
            {
                Util.MessageValidation("SFU1366");
                return false;
            }

            if(string.IsNullOrEmpty(msbRollMapLane.SelectedItemsToString))
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("LANE"));
                return false;
            }

            return true;
        }

        private bool ValidationTagSection()
        {
            if (chartCoater.Data.Children.Count < 1)
            {
                Util.MessageValidation("SFU1498");
                return false;
            }

            return true;
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

        private static TextBlock CreateTextBlock(string content, HorizontalAlignment horizontal, VerticalAlignment vertical, int fontsize, FontWeight fontweights, SolidColorBrush brush, Thickness margine, Thickness padding, string name, Cursor cursor, string tag)
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

        private enum ChartDisplayType
        {
            TagSectionStart,
            TagSectionEnd,
            Mark,
            VisionTop,
            VisionBack,
            Material,
            Sample,
            TagSpot,
            SurfaceTop,
            SurfaceBack
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


        /// <summary>
        /// 자신의 컨트롤 한단계 위부터 사용자정의 이름으로 검색.
        /// </summary>
        /// <typeparam name="T">결과값의 형식</typeparam>
        /// <param name="control">검색을 시작할 컨트롤</param>
        /// <param name="findControlName">찾고자하는 컨트롤 이름</param>
        /// <returns></returns>
        private T FindVisualParentByName<T>(FrameworkElement control, string findControlName) where T : FrameworkElement
        {
            T t = null;
            DependencyObject obj = VisualTreeHelper.GetParent(control);

            for (int i = 0; i < 100; i++) //최대 100개의 컨트롤 까지 검색
            {
                string currentName = obj.GetValue(Control.NameProperty) as string;
                if (currentName == findControlName)
                {
                    t = obj as T;
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
                if (obj == null)
                {
                    break;
                }
            }
            return t;
        }

        /// <summary>
        /// 자신의 컨트롤 한단계 아래부터 사용자정의 이름으로 검색.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <param name="findControlName"></param>
        /// <returns></returns>
        private T FindVisualChildByName<T>(DependencyObject control, string findControlName) where T : DependencyObject
        {

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(control); i++)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if (controlName == findControlName)
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T>(child, findControlName);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }


        /*
        async System.Threading.Tasks.Task<bool> WaitCallback()
        {

            bool succeeded = false;
            while (!succeeded)
            {
                string[] defectDataSeries = new string[] { "TAG_SECTION" };

                int j = 0;

                for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartCoater.Data.Children[i];
                    if (defectDataSeries.Contains(dataSeries.Tag.GetString()))
                    {
                        j++;
                        if (_tagSectionSeq == j)
                        succeeded = true;
                    }
                }
                await System.Threading.Tasks.Task.Delay(500);
            }
            return true;
        }
        */


        #endregion


    }
}