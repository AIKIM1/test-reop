/*************************************************************************************
 Created Date : 2024.03.13
      Creator : 신광희
   Decription : 코터 롤맵 팝업(Roll Map 2.0) - 추후 자동차 롤맵과 통합 작업 필요
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.13  신광희 : Initial Created.
  2024.10.25  신광희 : JSON 결과값 Deserialize 시 Data Type 비정상으로 Linq 사용 시 Data Type 부분 수정
  2025.06.24  김한   : TAG_SECTION_LANE 분기에 따른 NFF TAG_SECTION 특화 로직 제거
  2025.07.04  김한   : GRADE 블록 적용 시 로딩량 부 선그래프에서 점그래프로 변환
  2025.07.10  김한   : 코터 웹게이지 표현 시, SCAN_OFFSET값 정상 매핑
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
using System.Text;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_RM_CHART_CT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        private double _xMaxLength;
        private double _xMinLength;
        private double _sampleCutLength;
        private string _polarityCode = string.Empty;
        private string _firstLanePosition = string.Empty;
        private DataTable _dtColorMapLegend;
        private DataTable _dtRollMapGauge;
        private DataTable _dtRollMapDefect;
        private DataTable _dtRollMapLane;

        private DataTable _dtLaneLegend;
        private DataTable _dtLane;
        private DataTable _dtDefectLegend;
        private DataTable _dtTagSection;
        private DataTable _dtTagSectionLane;
        private DataTable _dtTagSectionSingle;

        private string _selectedStartSectionText = string.Empty;
        private string _selectedStartSectionPosition = string.Empty;
        private string _selectedStartSampleFlag = string.Empty;
        private string _selectedEndSectionPosition = string.Empty;
        private string _selectedEndSampleFlag = string.Empty;
        private string _selectedCollectType = string.Empty;

        private double _selectedStartSection = 0;
        private double _selectedEndSection = 0;
        private double _selectedchartPosition = 0;

        private bool _isSelectedTagSection;
        private bool _isRollMapResultLink = false;  // 동별 공정별 롤맵 실적 연계 여부
        private bool _isRollMapLot;
        private bool _isEquipmentTotalDry = false; // 롤맵 설비 Total Dry 사용 유무
        private bool _isMinMaxEqpt = false;  //2024-12-17 웹게이지 MIN/MAX 툴팁 표시여부(설비기준)

        // 웹게이지 데이터 속도 향상 목적?
        private List<AlarmZone> _listgauge;

        private CoordinateType _CoordinateType;
        private enum CoordinateType
        {
            RelativeCoordinates,    //상대좌표
            Absolutecoordinates     //절대좌표
        }

        private struct LotInfo
        {
            public string ProcessCode;
            public string EquipmentSegmentCode;
            public string EquipmentCode;
            public string EquipmentName;
            public string RunlotId;
            public string WipSeq;
            public string LaneQty;
            public string Version;

            public LotInfo(string processCode, string equipmentSegmentCode, string equipmentCode, string equipmentName, string lotId, string wipSeq, string laneQty, string version)
            {
                ProcessCode = processCode;
                EquipmentSegmentCode = equipmentSegmentCode;
                EquipmentCode = equipmentCode;
                EquipmentName = equipmentName;
                RunlotId = lotId;
                WipSeq = wipSeq;
                LaneQty = laneQty;
                Version = version;
            }
        }

        private LotInfo _lotInfo = new LotInfo();

        public COM001_RM_CHART_CT()
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
                _lotInfo.ProcessCode = Util.NVC(parameters[0]);
                _lotInfo.EquipmentSegmentCode = Util.NVC(parameters[1]);
                _lotInfo.EquipmentCode = Util.NVC(parameters[2]);
                _lotInfo.RunlotId = Util.NVC(parameters[3]);
                _lotInfo.WipSeq = Util.NVC(parameters[4]);
                _lotInfo.LaneQty = Util.NVC(parameters[4]);
                _lotInfo.EquipmentName = Util.NVC(parameters[6]);
                _lotInfo.Version = Util.NVC(parameters[7]);

                // Roll Map 2.0 시스템 테스트
                //_lotInfo.ProcessCode = "E2000";
                //_lotInfo.EquipmentSegmentCode = string.Empty;
                //_lotInfo.EquipmentCode = "J1ECOT101";
                //_lotInfo.RunlotId = "ZCDJ9161C1";
                //_lotInfo.WipSeq = "1";
                //_lotInfo.LaneQty = string.Empty;
                //_lotInfo.EquipmentName = string.Empty;
                //_lotInfo.Version = string.Empty;

            }
            _CoordinateType = CoordinateType.RelativeCoordinates;
            Initialize();
            GetColorMapSpecLegend();
            GetRollMap();
        }

        private void UserControl_Closed(object sender, EventArgs e)
        {
            SetUserConfigurationControl();
        }

        private void windowRollMap_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Util.MessageInfo("sdfgh");
            //e.Cancel = true;
        }

        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!string.IsNullOrEmpty(txtLot.Text.Trim()))
                    {
                        if (string.Equals(_lotInfo.RunlotId, txtLot.Text))
                        {
                            GetRollMap();
                        }
                        else
                        {
                            if (IsNewLotInfo(txtLot.Text))
                            {
                                GetRollMap();
                            }
                        }
                    }

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

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            CMM_RM_CT_DATACOLLECT popupRollMapDataCollect = new CMM_RM_CT_DATACOLLECT();
            popupRollMapDataCollect.FrameOperation = FrameOperation;
            object[] parameters = new object[6];
            parameters[0] = _lotInfo.ProcessCode;
            parameters[1] = _lotInfo.EquipmentCode;
            parameters[2] = txtLot.Text;
            parameters[3] = _lotInfo.WipSeq;
            parameters[4] = _lotInfo.LaneQty;

            C1WindowExtension.SetParameters(popupRollMapDataCollect, parameters);
            popupRollMapDataCollect.Closed += popupRollMapDataCollect_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupRollMapDataCollect.ShowModal()));
        }

        private void popupRollMapDataCollect_Closed(object sender, EventArgs e)
        {
            CMM_RM_CT_DATACOLLECT popup = sender as CMM_RM_CT_DATACOLLECT;
            if (popup != null && popup.IsUpdated)
            {
                //btnSearch_Click(btnSearch, null);
            }
        }

        private void btnRollMapOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            CMM_RM_OUTSIDE_SCRAP popRollMapOutsideScrap = new CMM_RM_OUTSIDE_SCRAP();

            object[] parameters = new object[6];
            parameters[0] = _lotInfo.ProcessCode;
            parameters[1] = _lotInfo.EquipmentCode;
            parameters[2] = txtLot.Text;
            parameters[3] = _lotInfo.WipSeq;
            parameters[4] = "OUTSIDE_SCRAP"; //최외각 폐기
            parameters[5] = true;

            C1WindowExtension.SetParameters(popRollMapOutsideScrap, parameters);
            popRollMapOutsideScrap.Closed += popRollMapOutsideScrap_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapOutsideScrap.ShowModal()));
        }

        private void popRollMapOutsideScrap_Closed(object sender, EventArgs e)
        {
            CMM_RM_OUTSIDE_SCRAP popup = sender as CMM_RM_OUTSIDE_SCRAP;
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
                Util.MessageValidation("SFU8541", ObjectDic.Instance.GetObjectName("구간불량 등록"));
                return;
            }

            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[12];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = string.Empty;
            parameters[2] = string.Empty;
            parameters[3] = _lotInfo.ProcessCode;
            parameters[4] = _lotInfo.EquipmentCode;
            parameters[5] = _lotInfo.WipSeq;
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
            CMM_RM_TAG_SECTION popup = sender as CMM_RM_TAG_SECTION;
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
            CMM_RM_TAGSECTION_MERGE popup = sender as CMM_RM_TAGSECTION_MERGE;
            if (popup != null && popup.IsUpdated)
            {
                ShowLoadingIndicator();
                DoEvents();

                for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartCoater.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TAG_SECTION") || dataSeries.Tag.GetString().Equals("TAG_SECTION_SINGLE") || dataSeries.Tag.GetString().Equals("TAG_SECTION_LANE"))
                    {
                        chartCoater.Data.Children.Remove(dataSeries);
                    }
                }

                //전체 조회 시 데이터가 많은 경우 바인딩 하는 속도 문제로 TAG_SECTION 따로 처리
                string adjFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
                string sampleFlag = "Y";
                DataTable dtDefect = SelectRollMapDefectInfo(_lotInfo.EquipmentCode, txtLot.Text, _lotInfo.WipSeq, adjFlag, sampleFlag);

                var queryTagSection = dtDefect.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dtDefect.Clone();
                dtTagSection.TableName = "TAG_SECTION";
                _dtTagSection = dtTagSection;

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawDefectTagSection(dtTagSection);
                }

                var queryTagSectionLane = dtDefect.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
                DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dtDefect.Clone();
                dtTagSectionLane.TableName = "TAG_SECTION_LANE";
                _dtTagSectionLane = dtTagSectionLane;

                if (CommonVerify.HasTableRow(dtTagSectionLane))
                {
                    DrawDefectTagSectionLane(dtTagSectionLane);
                }

                var queryTagSectionSingle = dtDefect.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSectionSingle = queryTagSectionSingle.Any() ? queryTagSectionSingle.CopyToDataTable() : dtDefect.Clone();
                dtTagSectionSingle.TableName = "TAG_SECTION_SINGLE";
                _dtTagSectionSingle = dtTagSectionSingle;

                if (CommonVerify.HasTableRow(dtTagSectionSingle))
                {
                    DrawDefectTagSectionSingle(dtTagSectionSingle);
                }
                HiddenLoadingIndicator();
            }
            else
            {
                DrawDefectTagSection(_dtTagSection);
                DrawDefectTagSectionLane(_dtTagSectionLane);
                DrawDefectTagSectionSingle(_dtTagSectionSingle);
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
            GetColorMapSpecLegend();
        }

        private void rdoRelativeCoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetColorMapSpecLegend();
        }

        private void rdoWebgauge_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void rdoThickness_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void cboRollMapExpressSummary_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (string.IsNullOrEmpty(cboRollMapExpressSummary.SelectedValue.GetString())) return;

            DataTable dt = DataTableConverter.Convert(cboRollMapExpressSummary.ItemsSource);
            DataRow dr = dt.Rows[cboRollMapExpressSummary.SelectedIndex];

            string[] laneArray = dr["ATTR1"].GetString().Split(',');

            if (laneArray != null && msbRollMapLane.ItemsSource != null)
            {
                msbRollMapLane.UncheckAll();
                for (int i = 0; i < laneArray.Length; i++)
                {
                    foreach (MultiSelectionBoxItem msbitem in msbRollMapLane.MultiSelectionBoxSource)
                    {
                        if (DataTableConverter.GetValue(msbitem.Item, "COM_CODE").GetString().Equals(laneArray[i].GetString()))
                            msbitem.IsChecked = true;
                    }
                }
            }
        }

        private void tbTagSection_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", ObjectDic.Instance.GetObjectName("구간불량"));
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

            object[] parameters = new object[12];
            parameters[0] = txtLot.Text;
            // 좌표정보
            parameters[1] = startPosition;
            parameters[2] = endPosition;
            parameters[3] = _lotInfo.ProcessCode;
            parameters[4] = _lotInfo.EquipmentCode;
            parameters[5] = _lotInfo.WipSeq;
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
                if (string.Equals(textBlock.Text, "S"))
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
                    CMM_RM_TAGSECTION_MERGE popRollMapTagsectionMerge = new CMM_RM_TAGSECTION_MERGE();
                    popRollMapTagsectionMerge.FrameOperation = FrameOperation;

                    object[] parameters = new object[9];
                    parameters[0] = txtLot.Text;
                    parameters[1] = _lotInfo.WipSeq;
                    parameters[2] = _selectedStartSectionPosition;
                    parameters[3] = _selectedEndSectionPosition;
                    parameters[4] = _lotInfo.ProcessCode;
                    parameters[5] = _lotInfo.EquipmentCode;
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
                    DrawDefectTagSection(_dtTagSection);

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
                Util.MessageValidation("SFU8541", ObjectDic.Instance.GetObjectName("구간불량 등록"));
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
                Util.MessageValidation("SFU8541", ObjectDic.Instance.GetObjectName("구간불량 등록"));
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
                InitializeGroupBox();
                InitializeComboBox();

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
            txtLot.Text = _lotInfo.RunlotId;
            tbMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
            chartCoater.View.AxisX.ScrollBar = new AxisScrollBar();

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

            // 조회 결과 Lane 정보
            _dtLane = new DataTable();
            _dtLane.Columns.Add("LANE_NO", typeof(string));

            // 불량 범례 테이블
            _dtDefectLegend = new DataTable();
            _dtDefectLegend.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefectLegend.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefectLegend.Columns.Add("COLORMAP", typeof(string));

            IsShowDefectButton();

            SetEqptDetailInfoByCommoncode(); //공통코드 ROLLMAP_EQPT_DETAIL_INFO 설정에 따라 웹게이지 MIN/MAX 툴팁반영 여부 Flag 초기화(E20241126-000434)
        }

        private void InitializeGroupBox()
        {
            //조회 영역 GroupBox Visibility 속성 설정 - 동별 공통코드 관리.
            gdMeasurementradio.Visibility = Visibility.Collapsed;
            gdMeasurementcombo.Visibility = Visibility.Collapsed;
            gdMapExpress.Visibility = Visibility.Collapsed;
            gdColorMap.Visibility = Visibility.Collapsed;

            SetRollMapSearchGroupBox();
        }

        private void InitializeComboBox()
        {
            // 계측기 콤보박스 그리드 Visibility 속성 Visible 인 경우
            if (gdMeasurementcombo.Visibility == Visibility.Visible)
            {
                SetMeasurementComboBox();
            }

            if (gdMapExpress.Visibility == Visibility.Visible)
            {
                //맵 표현 설정(요약, Lane 선택 ComboBox,MultiSelectionBox 데이터 조회)
                SetRollMapLaneMultiSelectionBox(msbRollMapLane);

                // 맵표현 요약 콤보박스
                SetRollMapExpressSummaryComboBox(cboRollMapExpressSummary);
            }

            GetUserConfigurationControl();

            // 2023.10.17 고지연 사원 요청으로 모델별 Lane수 정보에 따라 선택범위 설정될 수 있게 수정(변경된 BizRule 에 따른 UI 수정) 
            // Lane별 롤맵 선택 MultiSelectionBox
            //SetRollMapLaneMultiSelectionBox(msbRollMapLane);
        }

        /// <summary>
        /// 극성에 따른 원재료 영역 Visibility 속성 변경
        /// </summary>
        private void InitializeMaterialDvision()
        {
            DataTable dt = SelectEquipmentPolarity(_lotInfo.EquipmentCode);
            _isEquipmentTotalDry = IsEquipmentTotalDry(_lotInfo.EquipmentCode);

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

                if (string.IsNullOrEmpty(_lotInfo.EquipmentName))
                {
                    _lotInfo.EquipmentName = dt.Rows[0]["EQPTNAME"].GetString();
                    txtEquipmentName.Text = _lotInfo.EquipmentName + " [" + _lotInfo.EquipmentCode + "]";
                }
                else
                {
                    txtEquipmentName.Text = _lotInfo.EquipmentName;
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
            chartCoater.View.AxisY.Min = -40;
            chartCoater.View.AxisX.Max = maxLength;
            chartCoater.View.AxisY.Max = 100;

            InitializeChartView(chartCoater);
        }

        /// <summary>
        /// 원재료 차트 View Initialize
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

        /// <summary>
        /// 코터 차트 단위 간격 설정 및 Grid lines, 
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
            if (grdDefectTopLegend.ColumnDefinitions.Count > 0) grdDefectTopLegend.ColumnDefinitions.Clear();
            if (grdDefectTopLegend.RowDefinitions.Count > 0) grdDefectTopLegend.RowDefinitions.Clear();
            grdDefectTopLegend.Children.Clear();

            if (grdDefectBackLegend.ColumnDefinitions.Count > 0) grdDefectBackLegend.ColumnDefinitions.Clear();
            if (grdDefectBackLegend.RowDefinitions.Count > 0) grdDefectBackLegend.RowDefinitions.Clear();
            grdDefectBackLegend.Children.Clear();

            if (grdTopThicknessLoadingLegend.ColumnDefinitions.Count > 0) grdTopThicknessLoadingLegend.ColumnDefinitions.Clear();
            if (grdTopThicknessLoadingLegend.RowDefinitions.Count > 0) grdTopThicknessLoadingLegend.RowDefinitions.Clear();
            grdTopThicknessLoadingLegend.Children.Clear();

            if (grdBackThicknessLoadingLegend.ColumnDefinitions.Count > 0) grdBackThicknessLoadingLegend.ColumnDefinitions.Clear();
            if (grdBackThicknessLoadingLegend.RowDefinitions.Count > 0) grdBackThicknessLoadingLegend.RowDefinitions.Clear();
            grdBackThicknessLoadingLegend.Children.Clear();

            if (grdThicknessLoadingLegend.ColumnDefinitions.Count > 0) grdThicknessLoadingLegend.ColumnDefinitions.Clear();
            if (grdThicknessLoadingLegend.RowDefinitions.Count > 0) grdThicknessLoadingLegend.RowDefinitions.Clear();
            grdThicknessLoadingLegend.Children.Clear();


            btnRefresh.IsEnabled = true;
            btnZoomIn.IsEnabled = true;
            btnZoomOut.IsEnabled = true;

            tbTopupper.Text = string.Empty;
            tbToplower.Text = string.Empty;
            tbBackupper.Text = string.Empty;
            tbBacklower.Text = string.Empty;
            tbOutputProdQty.Text = string.Empty;
            tbOutputGoodQty.Text = string.Empty;

            _isSelectedTagSection = false;
            _selectedStartSection = 0;
            _selectedEndSection = 0;
            _selectedchartPosition = 0;
            _firstLanePosition = string.Empty;

            _isRollMapLot = SelectRollMapLot();
            _isRollMapResultLink = IsRollMapResultApply();
            if (_isRollMapResultLink && _isRollMapLot)
            {
                btnRollMapOutsideScrap.Visibility = Visibility.Collapsed;
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnRollMapOutsideScrap.Visibility = Visibility.Visible;
                btnRollMapPositionEdit.Visibility = Visibility.Visible;
            }

            BeginUpdateChart();
        }

        private void InitializeDataTables()
        {
            _dtLane?.Clear();
            _dtTagSection?.Clear();
            _dtTagSectionLane?.Clear();
            _dtTagSectionSingle?.Clear();
            _dtDefectLegend?.Clear();
            _dtRollMapGauge?.Clear();
            _dtRollMapDefect?.Clear();
            _dtRollMapLane?.Clear();

            _selectedStartSectionText = string.Empty;
            _selectedStartSectionPosition = string.Empty;
            _selectedEndSectionPosition = string.Empty;
            _selectedCollectType = string.Empty;
            _selectedStartSampleFlag = string.Empty;
            _selectedEndSampleFlag = string.Empty;
        }

        private void ResetControlBySearch()
        {
            _CoordinateType = (rdoRelativeCoordinates != null && (bool)rdoRelativeCoordinates.IsChecked) ? CoordinateType.RelativeCoordinates : CoordinateType.Absolutecoordinates;

            if (gdMeasurementradio.Visibility == Visibility.Visible)
            {
                if (rdoThickness.IsChecked != null && (bool)rdoThickness.IsChecked)
                {
                    tbMeasurement.Text = ObjectDic.Instance.GetObjectName("두께");
                }
                else
                {
                    tbMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
                }
            }

            if (gdMeasurementcombo.Visibility == Visibility.Collapsed) return;

            if (!string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), cboMeasurementBack.SelectedValue.GetString().Substring(1, 1)))
            {
                bdmeasurement.Visibility = Visibility.Collapsed;
                bdThicknessLoadingTop.Visibility = Visibility.Visible;
                bdThicknessLoadingBack.Visibility = Visibility.Visible;
            }
            else
            {
                bdmeasurement.Visibility = Visibility.Visible;
                bdThicknessLoadingTop.Visibility = Visibility.Collapsed;
                bdThicknessLoadingBack.Visibility = Visibility.Collapsed;
            }

            if (string.Equals(cboMeasurementBack.SelectedValue.GetString(), "TD"))
            {
                if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), "L"))
                {
                    tbThicknessLoadingTop.Text = ObjectDic.Instance.GetObjectName("로딩량");
                }
                else
                {
                    tbThicknessLoadingTop.Text = ObjectDic.Instance.GetObjectName("두께");
                }
                tbThicknessLoadingBack.Text = ObjectDic.Instance.GetObjectName("TOTAL");
            }

            if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), "L")
                && string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(1, 1), "L"))
            {
                tbMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
            }

            if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), "T")
                && string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(1, 1), "T"))
            {
                tbMeasurement.Text = ObjectDic.Instance.GetObjectName("두께");
            }

            if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), "L"))
            {
                if (string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(1, 1), "T"))
                {
                    tbThicknessLoadingTop.Text = ObjectDic.Instance.GetObjectName("로딩량");
                    tbThicknessLoadingBack.Text = ObjectDic.Instance.GetObjectName("두께");
                }
            }
            else if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), "T"))
            {
                if (string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(1, 1), "L"))
                {
                    tbThicknessLoadingTop.Text = ObjectDic.Instance.GetObjectName("두께");
                    tbThicknessLoadingBack.Text = ObjectDic.Instance.GetObjectName("로딩량");
                }
            }
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
                        Margin = new Thickness(3, 0, 3, 3),
                        Fill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                    };
                    TextBlock textBlockDescription = new TextBlock
                    {
                        Text = Util.NVC(row["VALUE"]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        //FontWeight = FontWeights.Bold,
                        Margin = new Thickness(3, 0, 3, 3)
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


                if ((bool)rdoAbsolutecoordinates?.IsChecked)
                {
                    StackPanel stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(3, 0, 3, 3)
                    };

                    Path samplePath = new Path { Fill = Brushes.Black, Stretch = Stretch.Uniform };
                    //Sample Path
                    string pathData = "M11,21H7V19H11V21M15.5,19H17V21H13V19H13.2L11.8,12.9L9.3,13.5C9.2,14 9,14.4 8.8,14.8C7.9,16.3 6,16.7 4.5,15.8C3,14.9 2.6,13 3.5,11.5C4.4,10 6.3,9.6 7.8,10.5C8.2,10.7 8.5,11.1 8.7,11.4L11.2,10.8L10.6,8.3C10.2,8.2 9.8,8 9.4,7.8C8,6.9 7.5,5 8.4,3.5C9.3,2 11.2,1.6 12.7,2.5C14.2,3.4 14.6,5.3 13.7,6.8C13.5,7.2 13.1,7.5 12.8,7.7L15.5,19M7,11.8C6.3,11.3 5.3,11.6 4.8,12.3C4.3,13 4.6,14 5.3,14.4C6,14.9 7,14.7 7.5,13.9C7.9,13.2 7.7,12.2 7,11.8M12.4,6C12.9,5.3 12.6,4.3 11.9,3.8C11.2,3.3 10.2,3.6 9.7,4.3C9.3,5 9.5,6 10.3,6.5C11,6.9 12,6.7 12.4,6M12.8,11.3C12.6,11.2 12.4,11.2 12.3,11.4C12.2,11.6 12.2,11.8 12.4,11.9C12.6,12 12.8,12 12.9,11.8C13.1,11.6 13,11.4 12.8,11.3M21,8.5L14.5,10L15,12.2L22.5,10.4L23,9.7L21,8.5M23,19H19V21H23V19M5,19H1V21H5V19Z";
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Geometry));
                    samplePath.Data = (Geometry)converter.ConvertFrom(pathData);

                    TextBlock textBlockSample = new TextBlock
                    {
                        Text = ObjectDic.Instance.GetObjectName("CUT"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        Margin = new Thickness(3, 0, 3, 3)
                    };

                    stackPanel.Children.Add(samplePath);
                    stackPanel.Children.Add(textBlockSample);

                    Grid.SetColumn(stackPanel, 1);
                    Grid.SetRow(stackPanel, 1);

                    grdLegendBack.Visibility = Visibility.Visible;
                    grdLegendBack.Children.Add(stackPanel);
                }
            }
        }

        private void GetMaxLength(DataTable dtLength)
        {
            _xMaxLength = 0;
            _xMinLength = 0;

            var query = (from t in dtLength.AsEnumerable()
                         where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                         select new
                         {
                             sourceEndPosition = t.GetValue("SOURCE_END_PSTN").GetDecimal(),
                             ProdQty = t.GetValue("INPUT_QTY").GetDecimal(),
                             GoodQty = t.GetValue("WIPQTY_ED").GetDecimal()
                         }).FirstOrDefault();

            if (query != null)
            {
                tbOutputProdQty.Text = @"P" + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                tbOutputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                tbOutputGoodQty.Text = @"G" + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                tbOutputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
            }
            _xMaxLength = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
            _xMinLength = dtLength.AsEnumerable().ToList().Min(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
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
            chart.View.AxisX.Max = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();

            DataTable dt = new DataTable();
            dt.Columns.Add("SOURCE_END_PSTN", typeof(double));
            dt.Columns.Add("ARROW_PSTN", typeof(double));
            dt.Columns.Add("SOURCE_Y_PSTN", typeof(double));
            dt.Columns.Add("ARROW_Y_PSTN", typeof(double));
            dt.Columns.Add("COLORMAP", typeof(string));
            dt.Columns.Add("CIRCLENAME", typeof(string));
            dt.Columns.Add("WND_DIRCTN", typeof(string));
            dt.Columns.Add("TOOLTIP", typeof(string));


            double arrowValue = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble() * 0.025;

            for (int i = 0; i < dtLength.Rows.Count; i++)
            {
                double arrowYPosition;
                double arrowPosition;

                if (dtLength.Rows[i]["EQPT_MEASR_PSTN_ID"].GetString() == "RW")
                {
                    arrowYPosition = dtLength.Rows[i]["WND_DIRCTN"].GetInt() == 1 ? 0 : 60;
                    arrowPosition = dtLength.Rows[i]["SOURCE_END_PSTN"].GetDouble() - arrowValue;
                }
                else
                {
                    arrowYPosition = dtLength.Rows[i]["WND_DIRCTN"].GetInt() == 1 ? 60 : 0;
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
            const string bizRuleName = "BR_PRD_SEL_RM_RPT_CHART_CT";

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("OUT_LOTID", typeof(string));
            inTable.Columns.Add("OUT_WIPSEQ", typeof(decimal));
            inTable.Columns.Add("MPOINT_TOP", typeof(string));
            inTable.Columns.Add("MPOINT_BACK", typeof(string));
            inTable.Columns.Add("MPSTN_TOP", typeof(string));
            inTable.Columns.Add("MPSTN_BACK", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));
            inTable.Columns.Add("LANE_LIST", typeof(string));
            inTable.Columns.Add("SUM_FLAG", typeof(string));

            string pointTop = string.Empty;
            string pointBack = string.Empty;
            string positionTop = string.Empty;
            string positionBack = string.Empty;

            // pointTop, pointBack
            // 설치측정위치 1: 두께, 2: 로딩량 3:Total Dry
            // positionTop, positionBack
            // OVEN 전/후 

            if (gdMeasurementradio.Visibility == Visibility.Visible)
            {
                if ((bool)rdoThickness?.IsChecked)
                {
                    pointTop = "1";
                    pointBack = "1";
                    positionTop = "2";
                    positionBack = "2";

                }
                else if ((bool)rdoWebgaugeDry?.IsChecked)
                {
                    pointTop = "2";
                    pointBack = "2";
                    positionTop = "2";
                    positionBack = "2";
                }
                else if ((bool)rdoWebgaugeWet?.IsChecked)
                {
                    pointTop = "2";
                    pointBack = "2";
                    positionTop = "1";
                    positionBack = "1";
                }
                else
                {
                    //(bool)rdoWebgaugeTotalDry?.IsChecked
                    pointTop = "3";
                    pointBack = "3";
                    positionTop = "2";
                    positionBack = "2";
                }
            }
            else
            {
                //gdMeasurementcombo.Visibility == Visibility.Visible
                if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), "L"))
                    pointTop = "1";
                else
                    pointTop = "2";

                if (string.Equals(cboMeasurementBack.SelectedValue.GetString(), "TD"))
                {
                    pointBack = "3";
                }
                else
                {
                    if (string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(1, 1), "L"))
                        pointBack = "1";
                    else
                        pointBack = "2";
                }

                //DRY(로딩량), DRY(두께)
                if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(0, 1), "D"))
                    positionTop = "2";
                else
                    positionTop = "1";  //WET(로딩량), WET(두께), TOTAL_DRY


                if (string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(0, 1), "D"))
                    positionBack = "2";
                else
                    positionBack = "1";
            }


            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _lotInfo.EquipmentCode;
            newRow["LOTID"] = txtLot.Text;
            newRow["WIPSEQ"] = _lotInfo.WipSeq;
            newRow["MPOINT_TOP"] = pointTop;
            newRow["MPSTN_TOP"] = positionTop;
            newRow["MPOINT_BACK"] = pointBack;
            newRow["MPSTN_BACK"] = positionBack;
            newRow["ADJFLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
            newRow["SMPL_FLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "N" : "Y";
            newRow["LANE_LIST"] = gdMapExpress.Visibility == Visibility.Visible ? msbRollMapLane.SelectedItemsToString : null;
            newRow["SUM_FLAG"] = (gdColorMap.Visibility == Visibility.Visible && (bool)rdoGradeBlock?.IsChecked) ? "1" : "2";
            inTable.Rows.Add(newRow);

            //string xml = inDataSet.GetXml();

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUT_LENGTH,OUT_MATERIAL,OUT_HEAD,OUT_GAUGE,OUT_DEFECT,OUT_LANE", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        HiddenLoadingIndicator();
                        return;
                    }

                    ResetControlBySearch();
                    SetMenuUseCount();

                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        DataTable dtLength = bizResult.Tables["OUT_LENGTH"];        // RW, UW 길이 및 생산량, 양품량 정보 테이블
                        DataTable dtMaterial = bizResult.Tables["OUT_MATERIAL"];    // 투입자재(원재료) 영역 정보 표시 테이블
                        DataTable dtHead = bizResult.Tables["OUT_HEAD"];            // 코터 차트 HEADER(상단) RW 길이 표시 및 Sample Cut 정보 표시 테이블
                        DataTable dtGauge = bizResult.Tables["OUT_GAUGE"];          // 코터 차트 영역 Gauge, 로딩량 정보 표시 테이블
                        DataTable dtDefect = bizResult.Tables["OUT_DEFECT"];        // 코터 차트 영역 불량정보 표시 테이블
                        DataTable dtLane = bizResult.Tables["OUT_LANE"];            // 코터 차트 영역 무지부, 유지부, Edge Top Back 정보 테이블


                        _dtRollMapGauge = dtGauge.Copy();
                        _dtRollMapDefect = dtDefect.Copy();
                        _dtRollMapLane = dtLane.Copy();

                        if (CommonVerify.HasTableRow(dtLength))
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

                        if (CommonVerify.HasTableRow(dtMaterial))
                        {
                            DrawMaterialChart(dtMaterial);
                        }

                        InitializeCoaterChart();

                        if (CommonVerify.HasTableRow(dtLane))
                        {
                            //롤맵 무지부, 유지부, Lane 텍스트 표시
                            DrawRollMapLane(dtLane);
                            DisplayTabLocation(dtLane);
                        }

                        if (CommonVerify.HasTableRow(dtGauge))
                        {
                            DrawGauge(dtGauge);
                            DrawThicknessLoading(dtGauge);
                            DrawThicknessLoadingLaneLegend();
                        }

                        if (CommonVerify.HasTableRow(dtHead))
                        {
                            DrawStartEndYAxis(dtHead);
                        }

                        if (CommonVerify.HasTableRow(dtDefect))
                        {
                            DrawDefect(dtDefect);
                            DrawDefectLegend(dtDefect);
                        }

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

        private DataTable SelectRollMapDefectInfo(string equipmentCode, string lotId, string wipSeq, string adjFlag, string sampleFlag)
        {
            const string bizRuleName = "DA_PRD_SEL_RM_RPT_DEFECT_CT";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("IN_OUT", typeof(string));
            inTable.Columns.Add("LANE_LIST", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = equipmentCode;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["ADJFLAG"] = adjFlag;
            dr["IN_OUT"] = "2";
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
                                             StartPosition = t.GetValue("SOURCE_STRT_PSTN").GetDecimal(),
                                             EndPosition = t.GetValue("SOURCE_END_PSTN").GetDecimal(),
                                             InputLotId = t.Field<string>("INPUT_LOTID"),
                                             // [E20240115-000246] 코터 롤맵 Material CHART의 슬러리에 믹서 버퍼의 이전/이후 배치 ID를 툴팁으로 표기. (믹서-코터 배치연계 고도화) 2024.01.30 JEONG KI TONG
                                             PreInputLotId = t.Field<string>("PRE_INPUT_LOTID"),
                                             NextInputLotId = t.Field<string>("NEXT_INPUT_LOTID"),
                                             ColorMap = t.Field<string>("COLORMAP"),
                                             Side = t.Field<string>("SIDE"),
                                             AdjLotId = t.Field<string>("ADJ_LOTID"),
                                             MeasurementPosition = t.Field<string>("EQPT_MEASR_PSTN_ID"),
                                             MaterialDescription = t.Field<string>("MATERIAL_DESC"),
                                             AdjStartPosition = t.GetValue("ADJ_STRT_PSTN").GetDecimal(),
                                             AdjEndPosition = t.GetValue("ADJ_END_PSTN").GetDecimal(),
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

                                        StringBuilder sb = new StringBuilder();
                                        sb.Append(ObjectDic.Instance.GetObjectName("길이") + "  : " + $"{length:#,##0.#}" + "m (" + Util.NVC($"{row.AdjStartPosition:###,###,###,##0.##}") + " ~ " + Util.NVC($"{row.AdjEndPosition:###,###,###,##0.##}") + ")");
                                        //// [E20240115-000246] 코터 롤맵 Material CHART의 슬러리에 믹서 버퍼의 이전/이후 배치 ID를 툴팁으로 표기. (믹서-코터 배치연계 고도화) 2024.01.30 JEONG KI TONG
                                        sb.Append(Environment.NewLine);
                                        sb.Append(ObjectDic.Instance.GetObjectName("SLURRY") + "  : " + string.Format("Current : {0} / Pre : {1} / Next : {2}", row.InputLotId, row.PreInputLotId, row.NextInputLotId));
                                        ToolTipService.SetToolTip(pe, sb.ToString());
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

                var queryUnwinder = dt.AsEnumerable().Where(r => r.Field<string>("EQPT_MEASR_PSTN_ID").Equals("UW")).ToList();
                if (queryUnwinder.Any())
                {
                    DrawUnWinderInputLotLine(queryUnwinder.CopyToDataTable());
                }

            }
        }

        private void DrawUnWinderInputLotLine(DataTable dt)
        {
            var queryInputLot = (from t in dt.AsEnumerable().Where(r => r.Field<string>("INPUT_LOTID") != null && r.Field<string>("INPUT_LOTID") != string.Empty) select new { InputLotId = t.Field<string>("INPUT_LOTID") }).Distinct().ToList();

            if (queryInputLot.Count > 1)
            {
                C1Chart c1Chart = Equals(_polarityCode, "C") ? chartMaterialCathode4 : chartMaterialAnode3;

                //dt.AsEnumerable().OrderBy(x => x.GetValue("SOURCE_END_PSTN").GetDecimal());

                DataRow[] drInLots = dt.AsEnumerable().CopyToDataTable()
                    .DefaultView.ToTable(true, "INPUT_LOTID", "SOURCE_END_PSTN").AsEnumerable()
                    .OrderBy(r => Util.NVC_Decimal(r["SOURCE_END_PSTN"]))
                    .ToArray();

                for (int i = 0; i < drInLots.Length - 1; i++)
                {
                    double sourceEndPosition;
                    double.TryParse(Util.NVC(drInLots[i]["SOURCE_END_PSTN"]), out sourceEndPosition);

                    string content = string.Empty;
                    string inputLot = Util.NVC(drInLots[i]["INPUT_LOTID"]);
                    string nextLot = Util.NVC(drInLots[i + 1]["INPUT_LOTID"]);

                    if (inputLot.Equals(nextLot)) continue;

                    if (string.IsNullOrEmpty(inputLot))
                    {
                        content = "Change Lot " + " (" + $"{sourceEndPosition:###,###,###,##0.##}" + "m" + ")";
                    }
                    else
                    {
                        content = "Change Lot [" + Util.NVC(drInLots[i]["INPUT_LOTID"]) + "]" + " (" + $"{sourceEndPosition:###,###,###,##0.##}" + "m" + ")";
                    }

                    XYDataSeries ds = new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { drInLots[i]["SOURCE_END_PSTN"].GetDouble(), drInLots[i]["SOURCE_END_PSTN"].GetDouble() },
                        ValuesSource = new double[] { 0, 100 },
                        Cursor = Cursors.Hand,
                        ConnectionStroke = new SolidColorBrush(Colors.Red),
                        ConnectionStrokeThickness = 3d
                    };

                    ds.PlotElementLoaded += (s, e) =>
                    {
                        PlotElement pe = (PlotElement)s;

                        if (pe is Lines)
                        {
                            ToolTipService.SetToolTip(pe, content);
                            ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                            ToolTipService.SetShowDuration(pe, 60000);
                        }
                    };

                    c1Chart.Data.Children.Add(ds);
                }
            }
        }
        #endregion

        private void DrawRollMapLane(DataTable dt)
        {
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
            }


            // 차트 내 Lane 표시
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

            if (queryLane.Any() && queryTopBack.Any())
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
                                FirstTopBackLaneRate = s.Min(z => z.GetValue("Y_PSTN_STRT_RATE").GetDecimal()),
                                LastTopBackLaneRate = s.Max(z => z.GetValue("Y_PSTN_END_RATE").GetDecimal())
                            }).FirstOrDefault();

                        if (queryTopBackLaneRate != null)
                        {
                            DrawLaneText(queryTopBackLaneRate.FirstTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LastTopBackLaneRate.GetDouble(), queryTopBackLaneRate.LaneNo);
                        }
                    }
                }
            }
        }

        private void DrawThicknessLoading(DataTable dt)
        {
            try
            {
                chartTopThicknessLoading.View.AxisX.Min = 0;
                chartTopThicknessLoading.View.AxisX.Max = _xMaxLength;
                chartBackThicknessLoading.View.AxisX.Min = 0;
                chartBackThicknessLoading.View.AxisX.Max = _xMaxLength;

                chartTopThicknessLoading.ChartType = (gdColorMap.Visibility == Visibility.Visible && (bool)rdoGradeBlock?.IsChecked) ? ChartType.XYPlot : ChartType.Line;
                chartBackThicknessLoading.ChartType = (gdColorMap.Visibility == Visibility.Visible && (bool)rdoGradeBlock?.IsChecked) ? ChartType.XYPlot : ChartType.Line;

                dt.Columns.Add(new DataColumn { ColumnName = "ADJ_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
                foreach (DataRow row in dt.Rows)
                {
                    row["ADJ_AVG_VALUE"] = (row["ADJ_STRT_PSTN"].GetDouble() + row["ADJ_END_PSTN"].GetDouble()) / 2;
                }

                //MIN/MAX 반영 설비는 대표값을 라인차트에 표시한다. 대표 값이 없을 경우 평균값을 대입하여 처리(E20241126-000434)
                if (_isMinMaxEqpt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        double scanRepValue;
                        double scanAvgValue;

                        double.TryParse(Util.NVC(row["SCAN_REP_VALUE"]), out scanRepValue);
                        double.TryParse(Util.NVC(row["SCAN_AVG_VALUE"]), out scanAvgValue);

                        if (scanRepValue == scanAvgValue)
                            continue;

                        if (scanRepValue == 0)
                            row["SCAN_REP_VALUE"] = row["SCAN_AVG_VALUE"];
                    }
                }

                dt.AcceptChanges();

                if (CommonVerify.HasTableRow(dt))
                {
                    DrawThicknessLoadingLegend(dt.Copy(), _dtColorMapLegend.Copy());

                    DataTable dtColorMapLegend = _dtColorMapLegend.Select().AsEnumerable().OrderByDescending(o => o.GetValue("NO").GetInt()).CopyToDataTable();
                    DataRow[] newRows = dtColorMapLegend.Select();

                    //Lane 별 색상 정보 조회
                    var queryLaneInfo = (from t in _dtLane.AsEnumerable()
                                         join x in _dtLaneLegend.AsEnumerable()
                                         on t.Field<string>("LANE_NO") equals x.Field<string>("LANE_NO")
                                         select new
                                         {
                                             LaneNo = t.Field<string>("LANE_NO"),
                                             LaneColor = x.Field<string>("COLORMAP")
                                         }).ToList();

                    string[] scanColorSetValue = new string[] { "LL", "L", "SV", "H", "HH" };

                    // 두께,로딩량 영역 차트 Top, Back 구분으로 데이터 표현
                    for (int i = 1; i < 3; i++)
                    {
                        foreach (DataRow row in newRows)
                        {
                            if (!scanColorSetValue.Contains(row["VALUE"].GetString())) continue;

                            string valueText = "VALUE_" + row["VALUE"].GetString();
                            if (!dt.AsEnumerable().Any(p => p.GetValue("SEQ").GetInt() == i && p.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(p.GetValue(valueText).ToString()))) continue;

                            var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                            DataSeries dsLegend = new DataSeries();
                            dsLegend.ItemsSource = scanColorSetValue;
                            dsLegend.ChartType = ChartType.Line;
                            dsLegend.ValuesSource = GetLineValuesSource(dt, row["VALUE"].GetString(), i);
                            dsLegend.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;

                            if (row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "L")
                            {
                                dsLegend.ConnectionStrokeDashes = new DoubleCollection { 3, 4 };
                                dsLegend.ConnectionStrokeThickness = 0.8;
                            }
                            else
                            {
                                dsLegend.ConnectionStrokeThickness = 1.0;
                            }

                            if (i == 1)
                                chartTopThicknessLoading.Data.Children.Add(dsLegend);
                            else
                                chartBackThicknessLoading.Data.Children.Add(dsLegend);
                        }


                        if (queryLaneInfo.Any())
                        {
                            int rowIndex = 0;

                            foreach (var item in queryLaneInfo)
                            {
                                if (!CommonVerify.HasTableRow(_dtLane)) continue;

                                var queryLane = _dtLane.AsEnumerable().Where(where => @where.Field<string>("LANE_NO").Equals(item.LaneNo)).ToList();
                                if (!queryLane.Any()) continue;

                                //var query = dt.AsEnumerable().Where(x => x.Field<Int16>("SEQ") == i && item.LaneNo == x.Field<Int32?>("ADJ_LANE_NO").GetString()).ToList();
                                var query = dt.AsEnumerable().Where(x => x.GetValue("SEQ").GetInt() == i && item.LaneNo == x.GetValue("ADJ_LANE_NO").GetString()).ToList();
                                DataTable dtLine = query.Any() ? query.CopyToDataTable() : dt.Clone();

                                var laneConvertFromString = ColorConverter.ConvertFromString(Util.NVC(item.LaneColor));

                                XYDataSeries ds = new XYDataSeries();
                                ds.ItemsSource = DataTableConverter.Convert(dtLine);
                                ds.XValueBinding = new Binding("ADJ_AVG_VALUE");
                                //ds.ValueBinding = new Binding("SCAN_AVG_VALUE");
                                ds.ValueBinding = _isMinMaxEqpt ? new Binding("SCAN_REP_VALUE") : new Binding("SCAN_AVG_VALUE");
                                ds.ChartType = (gdColorMap.Visibility == Visibility.Visible && (bool)rdoGradeBlock?.IsChecked) ? ChartType.XYPlot : ChartType.LineSymbols;
                                ds.ConnectionFill = new SolidColorBrush(Colors.Black);
                                ds.SymbolFill = laneConvertFromString != null ? new SolidColorBrush((Color)laneConvertFromString) : null;
                                ds.Cursor = Cursors.Hand;
                                ds.SymbolSize = new Size(6, 6);
                                ds.ConnectionStrokeThickness = 0.8;

                                if (i == 1) chartTopThicknessLoading.Data.Children.Add(ds);
                                else chartBackThicknessLoading.Data.Children.Add(ds);

                                ds.PlotElementLoaded += (s, e) =>
                                {
                                    PlotElement pe = (PlotElement)s;
                                    if (pe is DotSymbol)
                                    {
                                        DataPoint dp = pe.DataPoint;

                                        string content = string.Empty;
                                        content = Util.NVC(DataTableConverter.GetValue(dp.DataObject, "CMCDNAME"));

                                        double scanAvgValue;
                                        double sourceStart;
                                        double sourceEnd;

                                        //double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_SCAN_AVG_VALUE")), out scanAvgValue);

                                        double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, _isMinMaxEqpt ? "SOURCE_SCAN_REP_VALUE" : "SOURCE_SCAN_AVG_VALUE")), out scanAvgValue);
                                        double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_STRT_PSTN")), out sourceStart);
                                        double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_END_PSTN")), out sourceEnd);

                                        content = content.Contains("두께") ? "Thickness : " : "Load : ";

                                        content = content + $"{scanAvgValue:###,###,###,##0.##}" + "[" + $"{sourceStart:###,###,###,##0.##}" + "~" + $"{sourceEnd:###,###,###,##0.##}" + "]";

                                        ToolTipService.SetToolTip(pe, content);
                                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                        ToolTipService.SetShowDuration(pe, 60000);
                                    }
                                };
                                rowIndex++;
                            }
                        }

                    }

                    foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdLine))
                    {
                        InitializeChartView(c1Chart);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DrawThicknessLoadingLegend(DataTable dt, DataTable dtLegend)
        {
            try
            {
                if (grdTopThicknessLoadingLegend.ColumnDefinitions.Count > 0) grdTopThicknessLoadingLegend.ColumnDefinitions.Clear();
                if (grdTopThicknessLoadingLegend.RowDefinitions.Count > 0) grdTopThicknessLoadingLegend.RowDefinitions.Clear();

                if (grdBackThicknessLoadingLegend.ColumnDefinitions.Count > 0) grdBackThicknessLoadingLegend.ColumnDefinitions.Clear();
                if (grdBackThicknessLoadingLegend.RowDefinitions.Count > 0) grdBackThicknessLoadingLegend.RowDefinitions.Clear();

                grdTopThicknessLoadingLegend.Children.Clear();
                grdBackThicknessLoadingLegend.Children.Clear();

                string[] scanColorSetValue = new string[] { "LL", "L", "SV", "H", "HH" };

                DataRow[] newRows = dtLegend.Select();
                DataTable copyTable = dtLegend.Clone();
                copyTable.Columns.Add(new DataColumn { ColumnName = "VALUE_AVG", DataType = typeof(double) });

                for (int y = 1; y < 3; y++)
                {
                    foreach (DataRow row in newRows)
                    {
                        if (!scanColorSetValue.Contains(row["VALUE"].GetString())) continue;

                        string valueText = "VALUE_" + row["VALUE"];
                        if (dt.AsEnumerable().Any(o => o.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(o.GetValue(valueText).ToString()) && o.GetValue("SEQ").GetInt() == y))
                        {
                            double agvValue = 0;
                            var query = (from t in dt.AsEnumerable() where t.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(t.GetValue(valueText).ToString()) && t.GetValue("SEQ").GetInt() == y select new { Valuecol = t.GetValue(valueText).GetDecimal() }).ToList();
                            if (query.Any())
                                agvValue = query.Max(r => r.Valuecol).GetDouble();

                            DataRow dr = copyTable.NewRow();
                            dr["NO"] = y;
                            dr["COLORMAP"] = row["COLORMAP"];
                            dr["VALUE"] = row["VALUE"];
                            dr["VALUE_AVG"] = agvValue;
                            copyTable.Rows.Add(dr);
                        }
                    }
                }


                var j = 0;
                var k = 0;

                for (int i = 0; i < copyTable.Rows.Count; i++)
                {
                    RowDefinition gridRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };

                    if (copyTable.Rows[i]["NO"].GetInt() == 1)
                    {
                        grdTopThicknessLoadingLegend.RowDefinitions.Add(gridRow);
                    }
                    else
                    {
                        grdBackThicknessLoadingLegend.RowDefinitions.Add(gridRow);
                    }

                    StackPanel stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(2),
                    };

                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(copyTable.Rows[i]["COLORMAP"]));
                    string rowValue = copyTable.Rows[i]["VALUE"] + " : " + copyTable.Rows[i]["VALUE_AVG"].GetInt();

                    TextBlock lineLegendDescription = Util.CreateTextBlock(rowValue,
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

                    stackPanel.Children.Add(lineLegendDescription);

                    Grid.SetRow(stackPanel, copyTable.Rows[i]["NO"].GetInt() == 1 ? j : k);
                    Grid.SetColumn(stackPanel, 0);

                    if (copyTable.Rows[i]["NO"].GetInt() == 1)
                    {
                        grdTopThicknessLoadingLegend.Children.Add(stackPanel);
                        j++;
                    }
                    else
                    {
                        grdBackThicknessLoadingLegend.Children.Add(stackPanel);
                        k++;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DrawThicknessLoadingLaneLegend()
        {
            if (grdThicknessLoadingLegend.ColumnDefinitions.Count > 0) grdThicknessLoadingLegend.ColumnDefinitions.Clear();
            if (grdThicknessLoadingLegend.RowDefinitions.Count > 0) grdThicknessLoadingLegend.RowDefinitions.Clear();

            for (int x = 0; x < 2; x++)
            {
                ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                grdThicknessLoadingLegend.ColumnDefinitions.Add(gridColumn);
            }

            grdThicknessLoadingLegend.Children.Clear();

            var query = (from t in _dtLane.AsEnumerable()
                         join u in _dtLaneLegend.AsEnumerable()
                             on t.Field<string>("LANE_NO") equals u.Field<string>("LANE_NO")
                         select new
                         {
                             LaneNo = t.Field<string>("LANE_NO"),
                             LaneColor = u.Field<string>("COLORMAP")
                         }).OrderByDescending(o => Convert.ToInt32(o.LaneNo)).ToList();

            int y = 0;
            if (query.Any())
            {
                foreach (var item in query)
                {
                    RowDefinition gridRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };
                    grdThicknessLoadingLegend.RowDefinitions.Add(gridRow);

                    StackPanel stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(2)
                    };

                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(item.LaneColor));

                    Ellipse ellipseLane = Util.CreateEllipse(HorizontalAlignment.Center,
                                                             VerticalAlignment.Center,
                                                             12,
                                                             12,
                                                             convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                             1,
                                                             new Thickness(2),
                                                             null,
                                                             null);


                    TextBlock textBlockLane = Util.CreateTextBlock("Lane " + item.LaneNo,
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
                    grdThicknessLoadingLegend.Children.Add(stackPanel);

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
            double startPosition = _xMaxLength - (_xMaxLength * 0.014);

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


                //속도 개선을 위해 인스턴스 생성 선행
                _listgauge = new List<AlarmZone>();
                foreach (var item in dt.Rows)
                {
                    _listgauge.Add(new AlarmZone());
                }


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dt.Rows[i]["COLORMAP"]));
                    _listgauge[i].Near = dt.Rows[i]["ADJ_STRT_PSTN"].GetDouble();
                    _listgauge[i].Far = dt.Rows[i]["ADJ_END_PSTN"].GetDouble();
                    _listgauge[i].LowerExtent = dt.Rows[i]["CHART_Y_PSTN_STRT_RATE"].GetDouble();
                    _listgauge[i].UpperExtent = dt.Rows[i]["CHART_Y_PSTN_END_RATE"].GetDouble();
                    _listgauge[i].ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    _listgauge[i].ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                    _listgauge[i].Cursor = Cursors.Arrow;

                    int x = i;
                    _listgauge[i].PlotElementLoaded += (s, e) =>
                    {
                        PlotElement pe = (PlotElement)s;
                        if (pe is Lines)
                        {
                            double sourceStartPosition;
                            double sourceEndPosition;
                            double.TryParse(Util.NVC(dt.Rows[x]["SOURCE_STRT_PSTN"]), out sourceStartPosition);
                            double.TryParse(Util.NVC(dt.Rows[x]["SOURCE_END_PSTN"]), out sourceEndPosition);

                            //string content = ObjectDic.Instance.GetObjectName("LANE") + " : #" + dt.Rows[x]["ADJ_LANE_NO"].GetString() + " " + dt.Rows[x]["CMCDNAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                            //                 "Scan AVG : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                            //                 "ColorMap : " + Util.NVC(dt.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                            //                 "Offset : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}");

                            //차트 툴팁 MIN/MAX 표시 설비에 대해 수집된 MIN/MAX 값이 있을 경우 툴팁에 추가한다. (E20241126-000434)
                            string content = string.Empty;
                            if (_isMinMaxEqpt)
                            {
                                string scanMinValueContent = string.Empty;
                                string scaneMaxValueContent = string.Empty;

                                if (Util.NVC(dt.Rows[x]["SCAN_MIN_VALUE"]).IsNullOrEmpty() == false && dt.Rows[x]["SCAN_MIN_VALUE"].GetDouble() > 0)
                                {
                                    scanMinValueContent = "Scan Min : " + Util.NVC($"{dt.Rows[x]["SCAN_MIN_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine;
                                }
                                if (Util.NVC(dt.Rows[x]["SCAN_MAX_VALUE"]).IsNullOrEmpty() == false && dt.Rows[x]["SCAN_MAX_VALUE"].GetDouble() > 0)
                                {
                                    scaneMaxValueContent = "Scan Max : " + Util.NVC($"{dt.Rows[x]["SCAN_MAX_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine;
                                }

                                content = ObjectDic.Instance.GetObjectName("LANE") + " : #" + dt.Rows[x]["ADJ_LANE_NO"].GetString() + " " + dt.Rows[x]["CMCDNAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                             "Scan AVG : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                                             scanMinValueContent + scaneMaxValueContent +
                                             "ColorMap : " + Util.NVC(dt.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                             "Offset : " + Util.NVC(dt.Rows[x]["SCAN_OFFSET"]);
                            }
                            else
                            {
                                content = ObjectDic.Instance.GetObjectName("LANE") + " : #" + dt.Rows[x]["ADJ_LANE_NO"].GetString() + " " + dt.Rows[x]["CMCDNAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "] " + Environment.NewLine +
                                             "Scan AVG : " + Util.NVC($"{dt.Rows[x]["SOURCE_SCAN_AVG_VALUE"].GetDouble():###,###,###,##0.##}") + Environment.NewLine +
                                             "ColorMap : " + Util.NVC(dt.Rows[x]["SCAN_COLRMAP"]) + Environment.NewLine +
                                             "Offset : " + Util.NVC(dt.Rows[x]["SCAN_OFFSET"]);
                            }

                            ToolTipService.SetToolTip(pe, content);
                            ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                            ToolTipService.SetShowDuration(pe, 60000);
                        }
                    };
                    chartCoater.Data.Children.Add(_listgauge[i]);

                    //if (dt.Rows.Count - 1 == i)
                    //{
                    //    stopwatch.Stop();
                    //    System.Diagnostics.Debug.WriteLine("DrawGauge PlotElementLoaded Time =======> : " + stopwatch.Elapsed.ToString());
                    //}
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
            if (query.Any())
            {
                DataTable dtMaxLength = new DataTable();
                dtMaxLength.Columns.Add("RAW_END_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_END_PSTN", typeof(string));
                dtMaxLength.Columns.Add("SOURCE_Y_PSTN", typeof(double));
                dtMaxLength.Columns.Add("TOOLTIP", typeof(string));
                dtMaxLength.Columns.Add("FONTSIZE", typeof(string));

                DataRow newRow = dtMaxLength.NewRow();
                newRow["RAW_END_PSTN"] = "0";
                newRow["SOURCE_END_PSTN"] = "0";
                newRow["SOURCE_Y_PSTN"] = 98;
                newRow["TOOLTIP"] = string.Empty;
                newRow["FONTSIZE"] = 11;
                dtMaxLength.Rows.Add(newRow);

                var queryLength = (from t in dt.AsEnumerable()
                                   where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                   select new
                                   {
                                       //RawEndPosition = Decimal.Parse(t.GetValue("RAW_END_PSTN").ToString()),//  t.Field<decimal?>("RAW_END_PSTN"),
                                       //RawStartPosition = t.Field<decimal?>("RAW_STRT_PSTN"),
                                       //EndPosition = t.Field<decimal?>("SOURCE_END_PSTN"),
                                       //RowNum = t.Field<Int16>("ROW_NUM"),
                                       //MeasurementCode = t.Field<string>("EQPT_MEASR_PSTN_ID")
                                       RawEndPosition = t.GetValue("RAW_END_PSTN").GetDecimal(),
                                       RawStartPosition = t.GetValue("RAW_STRT_PSTN").GetDecimal(),
                                       EndPosition = t.GetValue("SOURCE_END_PSTN").GetDecimal(),
                                       RowNum = t.GetValue("ROW_NUM").GetInt(),
                                       MeasurementCode = t.Field<string>("EQPT_MEASR_PSTN_ID")
                                   }).ToList();

                if (queryLength.Any())
                {
                    int rowIndex = 0;

                    foreach (var item in queryLength)
                    {
                        DataRow newLength = dtMaxLength.NewRow();

                        newLength["RAW_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_END_PSTN"] = (bool)rdoRelativeCoordinates.IsChecked ? $"{item.RawEndPosition:###,###,###,##0.##}" : $"{item.EndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_Y_PSTN"] = 98;
                        newLength["TOOLTIP"] = item.RowNum + " Cut" + "( " + $"{item.RawStartPosition:###,###,###,##0.##}" + "m" + " ~ " + $"{item.RawEndPosition:###,###,###,##0.##}" + "m" + ")";
                        newLength["FONTSIZE"] = queryLength.Count.Equals(rowIndex + 1) ? 20 : 11;
                        dtMaxLength.Rows.Add(newLength);

                        rowIndex++;
                    }

                    XYDataSeries ds = new XYDataSeries();
                    ds.ItemsSource = DataTableConverter.Convert(dtMaxLength);
                    ds.XValueBinding = new Binding("SOURCE_END_PSTN");
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
                if ((bool)rdoRelativeCoordinates.IsChecked) return;
                _sampleCutLength = dt.AsEnumerable().Where(x => x.Field<string>("SMPL_FLAG") == "Y").ToList().Sum(r => r["RAW_END_PSTN"].GetDouble()).GetDouble();

                foreach (var item in querySample)
                {
                    chartCoater.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { item["SOURCE_END_PSTN"].GetDouble(), item["SOURCE_END_PSTN"].GetDouble() },
                        ValuesSource = new double[] { 0, 98 },
                        ConnectionStroke = new SolidColorBrush(Colors.DarkRed),
                    });
                }

                DataTable dtSample = MakeTableForDisplay(querySample.CopyToDataTable().Copy(), ChartDisplayType.Sample);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtSample);
                ds.XValueBinding = new Binding("SOURCE_END_PSTN");
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
            }
            else
            {
                _sampleCutLength = 0;
            }
        }

        private void DrawDefect(DataTable dt, bool isReDraw = false)
        {
            if (!CommonVerify.HasTableRow(dt)) return;

            var queryVisionSurfaceNgTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")).ToList();
            DataTable dtVisionSurfaceNgTop = queryVisionSurfaceNgTop.Any() ? queryVisionSurfaceNgTop.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtVisionSurfaceNgTop))
            {
                dtVisionSurfaceNgTop.TableName = "VISION_SURF_NG_TOP";
                DrawDefectVisionSurface(dtVisionSurfaceNgTop);
            }

            var queryVisionSurfaceNgBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")).ToList();
            DataTable dtVisionSurfaceNgBack = queryVisionSurfaceNgBack.Any() ? queryVisionSurfaceNgBack.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtVisionSurfaceNgBack))
            {
                dtVisionSurfaceNgBack.TableName = "VISION_SURF_NG_TOP";
                DrawDefectVisionSurface(dtVisionSurfaceNgBack);
            }

            var queryVisionNgTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")).ToList();
            DataTable dtVisionNgTop = queryVisionNgTop.Any() ? queryVisionNgTop.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtVisionNgTop))
            {
                dtVisionNgTop.TableName = "VISION_NG_TOP";
                DrawDefectVision(dtVisionNgTop);
            }

            var queryVisionNgBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")).ToList();
            DataTable dtVisionNgBack = queryVisionNgBack.Any() ? queryVisionNgBack.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtVisionNgBack))
            {
                dtVisionNgBack.TableName = "VISION_NG_BACK";
                DrawDefectVision(dtVisionNgBack);
            }

            var queryAlignVisionNgTop = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")).ToList();
            DataTable dtAlignVisionNgTop = queryAlignVisionNgTop.Any() ? queryAlignVisionNgTop.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtAlignVisionNgTop))
            {
                dtAlignVisionNgTop.TableName = "INS_ALIGN_VISION_NG_TOP";
                DrawDefectVision(dtAlignVisionNgTop);
            }

            var queryAlignVisionNgBack = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")).ToList();
            DataTable dtAlignVisionNgBack = queryAlignVisionNgBack.Any() ? queryAlignVisionNgBack.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtAlignVisionNgBack))
            {
                dtAlignVisionNgBack.TableName = "INS_ALIGN_VISION_NG_BACK";
                DrawDefectVision(dtAlignVisionNgBack);
            }


            // Fat Edge, Sliding, OverLay UI 표현 추가
            // 1. Fat Edge
            var queryFatEdge = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_FAT_EDGE")).ToList();
            DataTable dtFatEdge = queryFatEdge.Any() ? queryFatEdge.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtFatEdge))
            {
                dtFatEdge.TableName = "THICK_NG_FAT_EDGE";
                DrawDefectFatEdge(dtFatEdge);
            }

            var queryTopSliding = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_SLIDING")).ToList();
            DataTable dtTopSliding = queryTopSliding.Any() ? queryTopSliding.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtTopSliding))
            {
                dtTopSliding.TableName = "THICK_NG_TOP_SLIDING";
                DrawDefectTopSliding(dtTopSliding);
            }

            var queryBackSliding = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_SLIDING")).ToList();
            DataTable dtBackSliding = queryBackSliding.Any() ? queryBackSliding.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtBackSliding))
            {
                dtBackSliding.TableName = "THICK_NG_BACK_SLIDING";
                DrawDefectBackSliding(dtBackSliding);
            }

            var queryTopOverLay = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_OVERLAY_WIDTH")).ToList();
            DataTable dtTopOverLay = queryTopOverLay.Any() ? queryTopOverLay.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtTopOverLay))
            {
                dtTopOverLay.TableName = "THICK_NG_TOP_OVERLAY_WIDTH";
                DrawDefectTopOverLay(dtTopOverLay);
            }

            var queryBackOverLay = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_OVERLAY_WIDTH")).ToList();
            DataTable dtBackOverLay = queryBackOverLay.Any() ? queryBackOverLay.CopyToDataTable() : dt.Clone();

            if (CommonVerify.HasTableRow(dtBackOverLay))
            {
                dtBackOverLay.TableName = "THICK_NG_BACK_OVERLAY_WIDTH";
                DrawDefectBackOverLay(dtBackOverLay);
            }

            if (!isReDraw)
            {
                var queryMark = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("MARK")).ToList();
                DataTable dtMark = queryMark.Any() ? queryMark.CopyToDataTable() : dt.Clone();

                if (CommonVerify.HasTableRow(dtMark))
                {
                    dtMark.TableName = "MARK";
                    DrawDefectMark(dtMark);
                }

                var queryTagSection = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dt.Clone();
                dtTagSection.TableName = "TAG_SECTION";
                _dtTagSection = dtTagSection;
                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawDefectTagSection(dtTagSection);
                }

                // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
                var queryTagSectionLane = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
                DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dt.Clone();
                dtTagSectionLane.TableName = "TAG_SECTION_LANE";
                _dtTagSectionLane = dtTagSectionLane;

                if (CommonVerify.HasTableRow(dtTagSectionLane))
                {
                    DrawDefectTagSectionLane(dtTagSectionLane);
                }

                var queryTagSectionSingle = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSectionSingle = queryTagSectionSingle.Any() ? queryTagSectionSingle.CopyToDataTable() : dt.Clone();
                dtTagSectionSingle.TableName = "TAG_SECTION_SINGLE";
                _dtTagSectionSingle = dtTagSectionSingle;
                if (CommonVerify.HasTableRow(dtTagSectionSingle))
                {
                    DrawDefectTagSectionSingle(dtTagSectionSingle);
                }

                var queryTagSpot = dt.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dt.Clone();

                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    DrawDefectTagSpot(dtTagSpot);
                }
            }



        }

        private void DrawDefectLegend(DataTable dt)
        {
            //var queryDefectLegend = dt.AsEnumerable().Where(o =>
            //    o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")
            //    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")
            //    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")
            //    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")
            //    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")
            //    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")
            //    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_OVERLAY_VISION_NG")).ToList();
            //DataTable dtDefectLegend = queryDefectLegend.Any() ? queryDefectLegend.CopyToDataTable() : dt.Clone();

            //if (CommonVerify.HasTableRow(dtDefectLegend))
            //{
            //    dtDefectLegend.TableName = "DefectLegend";
            //    DrawDefectLegendText(dtDefectLegend);
            //}

            var queryDefectTopLegend = dt.AsEnumerable().Where(o =>
            o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_SLIDING")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_OVERLAY_WIDTH")
            ).ToList();
            DataTable dtDefectTopLegend = queryDefectTopLegend.Any() ? queryDefectTopLegend.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtDefectTopLegend))
            {
                dtDefectTopLegend.TableName = "DefectTopLegend";
                DrawDefectLegendText(dtDefectTopLegend);
            }

            var queryDefectBackLegend = dt.AsEnumerable().Where(o =>
            o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_SLIDING")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_OVERLAY_WIDTH")
            || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_FAT_EDGE")
            ).ToList();
            DataTable dtDefectBackLegend = queryDefectBackLegend.Any() ? queryDefectBackLegend.CopyToDataTable() : dt.Clone();
            if (CommonVerify.HasTableRow(dtDefectBackLegend))
            {
                dtDefectBackLegend.TableName = "DefectBackLegend";
                DrawDefectLegendText(dtDefectBackLegend);
            }

        }

        private void DrawDefectLegendText(DataTable dt)
        {
            try
            {
                if (CommonVerify.HasTableRow(dt))
                {
                    Grid gridLegend = dt.TableName == "DefectTopLegend" ? grdDefectTopLegend : grdDefectBackLegend;

                    if (gridLegend.ColumnDefinitions.Count > 0) gridLegend.ColumnDefinitions.Clear();
                    if (gridLegend.RowDefinitions.Count > 0) gridLegend.RowDefinitions.Clear();

                    for (int x = 0; x < 2; x++)
                    {
                        ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                        gridLegend.ColumnDefinitions.Add(gridColumn);
                    }
                    gridLegend.Children.Clear();

                    // Top, Back 범례 표시를 위한 Group 처리
                    var queryRow = from row in dt.AsEnumerable()
                                   group row by new
                                   {
                                       DefectName = row.Field<string>("ABBR_NAME"),
                                       ColorMap = row.Field<string>("COLORMAP"),
                                       DefectShape = row.Field<string>("DEFECT_SHAPE"),
                                       MeasurementCode = row.Field<string>("EQPT_MEASR_PSTN_ID")
                                   } into grp
                                   select new
                                   {
                                       DefectName = grp.Key.DefectName,
                                       ColorMap = grp.Key.ColorMap,
                                       DefectShape = grp.Key.DefectShape,
                                       DefectCount = grp.Count(),
                                       MeasurementCode = grp.Key.MeasurementCode
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
                                    Rectangle rectangleLegend = Util.CreateRectangle(HorizontalAlignment.Center,
                                                                                     VerticalAlignment.Center,
                                                                                     12,
                                                                                     12,
                                                                                     convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                                     null,
                                                                                     1,
                                                                                     new Thickness(2),
                                                                                     null,
                                                                                     null);

                                    TextBlock rectangleDescription = Util.CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
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
                                    Ellipse ellipseLegend = Util.CreateEllipse(HorizontalAlignment.Center,
                                                                               VerticalAlignment.Center,
                                                                               12,
                                                                               12,
                                                                               convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                                                               1,
                                                                               new Thickness(2),
                                                                               null,
                                                                               null);

                                    TextBlock ellipseDescription = Util.CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
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
            ds.XValueBinding = new Binding("ADJ_X_PSTN");
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
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_X_PSTN")), out xPosition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_ADJ_Y_PSTN")), out yPosition);
                    string defectName = DataTableConverter.GetValue(dp.DataObject, "ABBR_NAME").GetString();

                    string content = defectName + " [" + ObjectDic.Instance.GetObjectName("길이") + ":" + $"{xPosition:###,###,###,##0.##}" + "m" + ", " + ObjectDic.Instance.GetObjectName("폭") + ":" + $"{yPosition:###,###,###,##0.##}" + "mm" + "]";
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
        private void DrawDefectVision(DataTable dt)
        {
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
                        string content = row["ABBR_NAME"] + "[" + $"{startPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + "~" + $"{endPosition:###,###,###,##0.##}" + ObjectDic.Instance.GetObjectName("M") + "]";
                        ToolTipService.SetToolTip(pe, content);
                        ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                        ToolTipService.SetShowDuration(pe, 60000);
                    }
                };

                chartCoater.Data.Children.Add(alarmZone);
            }
        }

        /// <summary>
        /// 기준점(MARK)
        /// </summary>
        /// <param name="dt"></param>
        private void DrawDefectMark(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                double sourceStartPosition;
                double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);
                string content = row["EQPT_MEASR_PSTN_NAME"] + "[" + Util.NVC(row["ROLLMAP_CLCT_TYPE"]) + "]," + " (POS : " + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + ")";

                chartCoater.Data.Children.Add(new XYDataSeries()
                {
                    ChartType = ChartType.Line,
                    XValuesSource = new[] { row["ADJ_STRT_PSTN"].GetDouble(), row["ADJ_STRT_PSTN"].GetDouble() },
                    ValuesSource = new double[] { row["CHART_Y_STRT_CUM_RATE"].GetDouble(), row["CHART_Y_END_CUM_RATE"].GetDouble() },
                    ConnectionStroke = new SolidColorBrush(Colors.Black),
                    ConnectionStrokeDashes = new DoubleCollection { 3, 2 },
                    ToolTip = content
                });
            }

            // 데이터가 없는 경우 발생 시 에러 방지를 위하여 체크
            var queryLabel = dt.AsEnumerable().ToList();
            DataTable dtLabel = queryLabel.Any() ? MakeTableForDisplay(queryLabel.CopyToDataTable(), ChartDisplayType.Mark) : MakeTableForDisplay(dt.Clone(), ChartDisplayType.Mark);

            var dsMarkLabel = new XYDataSeries();
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
            chartCoater.Data.Children.Add(dsMarkLabel);
        }

        /// <summary>
        /// NG 마킹 (TAG_SECTION)
        /// </summary>
        /// <param name="dtTagPosition"></param>
        private void DrawDefectTagSection(DataTable dtTagPosition)
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
                    ds.XValueBinding = x == 0 ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");
                    ds.ValueBinding = new Binding("SOURCE_ADJ_Y_PSTN");
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

        private void DrawDefectTagSectionLane(DataTable dtTagPositionLane)
        {
            if (CommonVerify.HasTableRow(dtTagPositionLane))
            {
                if (!dtTagPositionLane.Columns.Contains("IsEnabled"))
                {
                    dtTagPositionLane.Columns.Add(new DataColumn() { ColumnName = "IsEnabled", DataType = typeof(bool) });
                }

                foreach (DataRow row in dtTagPositionLane.Rows)
                {
                    row["IsEnabled"] = (_isRollMapResultLink && _isRollMapLot) ? false : true;
                }
                dtTagPositionLane.AcceptChanges();

                // Start, End 이미지 두번의 표현으로 for문 사용
                for (int x = 0; x < 2; x++)
                {
                    DataTable dtTag = MakeTableForDisplay(dtTagPositionLane, x == 0 ? ChartDisplayType.TagSectionLaneStart : ChartDisplayType.TagSectionLaneEnd);

                    XYDataSeries ds = new XYDataSeries();
                    ds.ItemsSource = DataTableConverter.Convert(dtTag);
                    ds.XValueBinding = x == 0 ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");
                    ds.ValueBinding = new Binding("SOURCE_ADJ_Y_PSTN");
                    ds.ChartType = ChartType.XYPlot;
                    ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                    ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                    ds.PointLabelTemplate = grdMain.Resources["chartSectionTag"] as DataTemplate;
                    ds.Margin = new Thickness(0);
                    ds.Tag = dtTagPositionLane.TableName;

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

        private void DrawDefectTagSectionSingle(DataTable dtTagPositionSingle)
        {
            if (CommonVerify.HasTableRow(dtTagPositionSingle))
            {
                if (!dtTagPositionSingle.Columns.Contains("IsEnabled"))
                {
                    dtTagPositionSingle.Columns.Add(new DataColumn() { ColumnName = "IsEnabled", DataType = typeof(bool) });
                }

                foreach (DataRow row in dtTagPositionSingle.Rows)
                {
                    row["IsEnabled"] = (_isRollMapResultLink && _isRollMapLot) ? false : true;
                }
                dtTagPositionSingle.AcceptChanges();


                DataTable dtTag = MakeTableForDisplay(dtTagPositionSingle, ChartDisplayType.TagSectionSingle);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTag);
                ds.XValueBinding = new Binding("ADJ_STRT_PSTN");
                ds.ValueBinding = new Binding("SOURCE_ADJ_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartSectionTag"] as DataTemplate;
                ds.Margin = new Thickness(0);
                ds.Tag = dtTagPositionSingle.TableName;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);

                };
                chartCoater.Data.Children.Add(ds);

            }
        }

        /// <summary>
        /// Tag(TAG SPOT)
        /// </summary>
        /// <param name="dtTagSpot"></param>
        private void DrawDefectTagSpot(DataTable dtTagSpot)
        {
            if (CommonVerify.HasTableRow(dtTagSpot))
            {
                dtTagSpot = MakeTableForDisplay(dtTagSpot, ChartDisplayType.TagSpot);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTagSpot);
                ds.XValueBinding = new Binding("ADJ_STRT_PSTN");
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

        private void DrawDefectFatEdge(DataTable dtFatEdge)
        {
            DrawRollMapThicknessDefect(dtFatEdge);
        }

        private void DrawDefectTopSliding(DataTable dtTopSliding)
        {
            DrawRollMapThicknessDefect(dtTopSliding);
        }

        private void DrawDefectBackSliding(DataTable dtBackSliding)
        {
            DrawRollMapThicknessDefect(dtBackSliding);
        }

        private void DrawDefectTopOverLay(DataTable dtTopOverLay)
        {
            DrawRollMapThicknessDefect(dtTopOverLay);
        }

        private void DrawDefectBackOverLay(DataTable dtBackOverLay)
        {
            DrawRollMapThicknessDefect(dtBackOverLay);
        }

        private void DrawRollMapThicknessDefect(DataTable dt)
        {
            _firstLanePosition = GetFirstLanePosition();
            if (string.IsNullOrEmpty(_firstLanePosition)) return;

            string topBackFlag = dt.TableName.IndexOf("TOP", StringComparison.OrdinalIgnoreCase) > -1 ? "T" : "B";

            if (CommonVerify.HasTableRow(_dtRollMapLane))
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "Y_STRT_PSTN", DataType = typeof(double) });
                dt.Columns.Add(new DataColumn() { ColumnName = "Y_END_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dt.Rows)
                {
                    if (_dtRollMapLane.AsEnumerable().Any(o => o.Field<string>("LANE_NO_CUR").Equals(row["ADJ_LANE_NO"].GetString()) && o.Field<string>("T_B_FLAG").Equals(row["TOP_BACK_GUBUN"].GetString())))
                    {
                        var query = (from t in _dtRollMapLane.AsEnumerable()
                                     where t.Field<string>("LANE_NO_CUR").Equals(row["ADJ_LANE_NO"].GetString())
                                     && t.Field<string>("T_B_FLAG").Equals(row["TOP_BACK_GUBUN"].GetString())
                                     && t.Field<string>("COATING_PATTERN") == "Coat"
                                     select new { LaneNo = t.Field<string>("LANE_NO_CUR"), Seq = t.GetValue("CNT").GetInt() }).FirstOrDefault();

                        if (query != null)
                        {
                            //2 안 소스
                            //설비의 첫번째 Lane 위치 값이 L 인 경우
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) L 인경우 Sliding Top -> Top 영역 무지부 상단
                            //                                        Sliding Back -> Back 영역 무지부 상단
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) R 인경우 Sliding Top -> Top 영역 무지부 하단
                            //                                        Sliding Back -> Back 영역 무지부 하단

                            //설비의 첫번째 Lane 위치 값이 R 인 경우
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) L 인경우 Sliding Top -> Top 영역 무지부 하단
                            //                                        Sliding Back -> Back 무지부 하단
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) R 인경우 Sliding Top -> Top 영역 무지부 상단
                            //                                        Sliding Back -> Back 무지부 상단
                            if (string.Equals(_firstLanePosition, "L"))
                            {
                                if (row["HALF_SLIT_SIDE"].GetString().Equals("L"))
                                {
                                    var queryInfo = (from t in _dtRollMapLane.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<string>("T_B_FLAG") == topBackFlag
                                                     && t.GetValue("CNT").GetInt() > query.Seq
                                                     select new
                                                     {
                                                         //yStartPosition = t.Field<decimal>("Y_PSTN_STRT_RATE").GetDouble(),
                                                         //yEndPosition = t.Field<decimal>("Y_PSTN_END_RATE").GetDouble()
                                                         yStartPosition = t.GetValue("Y_PSTN_STRT_RATE").GetDecimal().GetDouble(),
                                                         yEndPosition = t.GetValue("Y_PSTN_END_RATE").GetDecimal().GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = queryInfo.yEndPosition;
                                    }
                                }
                                else
                                {
                                    var queryInfo = (from t in _dtRollMapLane.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<string>("T_B_FLAG") == topBackFlag
                                                     && t.GetValue("CNT").GetInt() < query.Seq
                                                     select new
                                                     {
                                                         yStartPosition = t.GetValue("Y_PSTN_STRT_RATE").GetDecimal().GetDouble(),
                                                         yEndPosition = t.GetValue("Y_PSTN_END_RATE").GetDecimal().GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = queryInfo.yEndPosition;
                                    }
                                }
                            }
                            else
                            {
                                if (row["HALF_SLIT_SIDE"].GetString().Equals("L"))
                                {
                                    var queryInfo = (from t in _dtRollMapLane.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<string>("T_B_FLAG") == topBackFlag
                                                     && t.GetValue("CNT").GetInt() < query.Seq
                                                     select new
                                                     {
                                                         yStartPosition = t.GetValue("Y_PSTN_STRT_RATE").GetDecimal().GetDouble(),
                                                         yEndPosition = t.GetValue("Y_PSTN_END_RATE").GetDecimal().GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = queryInfo.yEndPosition;
                                    }

                                }
                                else
                                {
                                    var queryInfo = (from t in _dtRollMapLane.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<string>("T_B_FLAG") == topBackFlag
                                                     && t.GetValue("CNT").GetInt() > query.Seq
                                                     select new
                                                     {
                                                         yStartPosition = t.GetValue("Y_PSTN_STRT_RATE").GetDecimal().GetDouble(),
                                                         yEndPosition = t.GetValue("Y_PSTN_END_RATE").GetDecimal().GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = queryInfo.yEndPosition;
                                    }
                                }
                            }
                        }

                        AlarmZone alarmZone = new AlarmZone();
                        var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));
                        alarmZone.Near = row["ADJ_STRT_PSTN"].GetDouble();
                        alarmZone.Far = row["ADJ_END_PSTN"].GetDouble();
                        alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                        alarmZone.LowerExtent = row["Y_STRT_PSTN"].GetDouble();
                        alarmZone.UpperExtent = row["Y_END_PSTN"].GetDouble();
                        alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                        alarmZone.Tag = dt.TableName;

                        //int x = i;
                        alarmZone.PlotElementLoaded += (s, e) =>
                        {
                            PlotElement pe = (PlotElement)s;
                            if (pe is Lines)
                            {
                                double sourceStartPosition = Convert.ToDouble(row["SOURCE_ADJ_STRT_PSTN"]);
                                double sourceEndPosition = Convert.ToDouble(row["SOURCE_ADJ_END_PSTN"]);
                                string content = row["EQPT_MEASR_PSTN_NAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "m" + "]";

                                ToolTipService.SetToolTip(pe, content);
                                ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                ToolTipService.SetShowDuration(pe, 60000);
                            }
                        };

                        chartCoater.Data.Children.Add(alarmZone);

                    }
                }

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
                dtDefect = _dtDefectLegend;
                dtBaseDefect = _dtRollMapDefect;

                string[] defectDataSeries = new string[] { "VISION_SURF_NG_TOP", "VISION_SURF_NG_BACK", "VISION_NG_TOP", "VISION_NG_BACK", "INS_ALIGN_VISION_NG_TOP", "INS_ALIGN_VISION_NG_BACK", "THICK_NG_TOP_SLIDING", "THICK_NG_BACK_SLIDING", "THICK_NG_TOP_OVERLAY_WIDTH", "THICK_NG_BACK_OVERLAY_WIDTH", "THICK_NG_FAT_EDGE", "INS_OVERLAY_VISION_NG" };
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
                    dtDefect.Rows.Add(textBlock.Name, textBlockText, colorMap);
                    dtDefect.AcceptChanges();
                }
                else
                {
                    dtDefect.AsEnumerable().Where(r => r.Field<string>("EQPT_MEASR_PSTN_ID") == textBlock.Name && r.Field<string>("ABBR_NAME") == textBlockText && r.Field<string>("COLORMAP") == colorMap).ToList().ForEach(row => row.Delete());
                    dtDefect.AcceptChanges();
                    textBlock.Foreground = Brushes.Black;
                }

                var queryDefect = dtBaseDefect.AsEnumerable().Where(x => !dtDefect.AsEnumerable().Any(y => y.Field<string>("EQPT_MEASR_PSTN_ID") == x.Field<string>("EQPT_MEASR_PSTN_ID") && y.Field<string>("ABBR_NAME") == x.Field<string>("ABBR_NAME") && y.Field<string>("COLORMAP") == x.Field<string>("COLORMAP")));
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

                //_isResearch = true;

                chartTopThicknessLoading.Data.Children.Clear();
                chartBackThicknessLoading.Data.Children.Clear();

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

                DrawThicknessLoading(_dtRollMapGauge.Copy());
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
            if (CommonVerify.HasTableRow(dtTab))
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

                        if (queryTab.Any())
                        {
                            foreach (var item in queryTab)
                            {
                                if (string.Equals(item["T_B_FLAG"].GetString(), "T"))
                                {
                                    //if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.Field<Int64>("LANE_SEQ") < item["LANE_SEQ"].GetInt()))
                                    if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.GetValue("LANE_SEQ").GetInt() < item["LANE_SEQ"].GetInt()))
                                    {
                                        islower = true;
                                        tbToplower.Text = "Tab";
                                    }

                                    if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.GetValue("LANE_SEQ").GetInt() > item["LANE_SEQ"].GetInt()))
                                    {
                                        isupper = true;
                                        tbTopupper.Text = "Tab";
                                    }
                                }
                                else
                                {
                                    if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.GetValue("LANE_SEQ").GetInt() < item["LANE_SEQ"].GetInt()))
                                    {
                                        islower = true;
                                        tbBacklower.Text = "Tab";
                                    }

                                    if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<string>("T_B_FLAG") == item["T_B_FLAG"].GetString() && x.GetValue("LANE_SEQ").GetInt() > item["LANE_SEQ"].GetInt()))
                                    {
                                        isupper = true;
                                        tbBackupper.Text = "Tab";
                                    }
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
        /// 로딩량 두께 영역 차트의 HH,H,SV,L,LL 가로 선 표현
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private IEnumerable<double> GetLineValuesSource(DataTable dt, string value, int i)
        {
            try
            {
                var queryCount = _xMaxLength.GetInt() + 1;
                double[] xx = new double[queryCount];

                string valueText = "VALUE_" + value;

                //if (dt.AsEnumerable().Any(p => p.Field<Int16>("SEQ") == i && p.Field<decimal?>(valueText) != null))
                if (dt.AsEnumerable().Any(p => p.GetValue("SEQ").GetInt() == i && p.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(p.GetValue(valueText).ToString())))
                {
                    double avgValue = 0;
                    var query = (from t in dt.AsEnumerable() where t.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(t.GetValue(valueText).ToString()) && t.GetValue("SEQ").GetInt() == i select new { Valuecol = t.GetValue(valueText).GetDecimal() }).ToList();
                    if (query.Any())
                        avgValue = query.Max(r => r.Valuecol).GetDouble();

                    for (int j = 0; j < queryCount; j++)
                    {
                        xx[j] = avgValue;
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
                    c1Chart.View.Margin = new Thickness(0);
                    c1Chart.Padding = new Thickness(10, 0, 0, 15);
                }
                else if (c1Chart.Name == "chartTopThicknessLoading" || c1Chart.Name == "chartBackThicknessLoading")
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
                if (string.Equals(_lotInfo.ProcessCode, Process.COATING) && c1Chart.Name != "chart")
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

            if (chartDisplayType == ChartDisplayType.TagSectionStart || chartDisplayType == ChartDisplayType.TagSectionEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                // TAG_SECTION, TAG_SECTION_LANE 분리에 따른 NFF 특화로직 주석처리 2025.06.24
                /*var queryBarcodeInfoList = (from t in dtBinding.AsEnumerable()
                                            where !("TAG_SECTION".Equals(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                            && t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty()
                                            && t.Field<string>("ADJ_LANE_NO").IsNotEmpty())
                                            select new
                                            {
                                                MarkBarcodeString = t.Field<string>("MRK_2D_BCD_STR"),
                                                //StartPosition = t.Field<decimal>("ADJ_STRT_PSTN"),
                                                //EndPosition = t.Field<decimal>("ADJ_END_PSTN")
                                                StartPosition = t.GetValue("ADJ_STRT_PSTN").GetDecimal(),
                                                EndPosition = t.GetValue("ADJ_END_PSTN").GetDecimal()
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
                                            }).ToList();*/

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_ADJ_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionStart ? -19 : -27;
                    row["TAG"] = $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();

                    // TAG_SECTION, TAG_SECTION_LANE 분리에 따른 NFF 특화로직 주석처리 2025.06.24
                    //L001: Normal, L002: NFF, L003: 소형(2170)
                    //if (string.Equals(row["DFCT_2D_BCR_STD"].GetString(), "L002") && !string.IsNullOrEmpty(row["MRK_2D_BCD_STR"].GetString().Trim()))
                    //{
                    //    if (queryBarcodeInfoList.Any())
                    //    {
                    //        foreach (var item in queryBarcodeInfoList)
                    //        {
                    //           if (row["LANE_QTY"].GetInt() == item.Mark2DBarCodeCount && row["ADJ_STRT_PSTN"].GetDecimal() == item.Mark2DBarCodeStartPosition && row["ADJ_END_PSTN"].GetDecimal() == item.Mark2DBarCodeEndPosition)
                    //            {
                    //                row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    //                row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                    //            }
                    //            else
                    //           {
                    //                string laneInfo = row["MRK_2D_BCD_STR"].GetString().Trim().Substring(0, 2);
                    //                row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    //                row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                    //row["TOOLTIP"] = row["CMCDNAME"].GetString() + " : " + row["DFCT_NAME"] + "\n" + "[" + $"{Convert.ToDouble(row["SOURCE_END_PSTN"]) - Convert.ToDouble(row["SOURCE_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                    //row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + " : " + row["DFCT_NAME"] + "\n" + "[" + $"{Convert.ToInt32(row["SOURCE_ADJ_END_PSTN"]) - Convert.ToInt32(row["SOURCE_ADJ_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + " : " + row["DFCT_NAME"] + Environment.NewLine +
                        "[" + $"{ (row["SOURCE_ADJ_END_PSTN"].GetDecimal() - row["SOURCE_ADJ_STRT_PSTN"].GetDecimal()):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionStart ? "S" : "E";
                    //}
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionLaneStart || chartDisplayType == ChartDisplayType.TagSectionLaneEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_ADJ_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagSectionLaneStart ? -19 : -27;
                    row["TAG"] = $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();

                    string laneInfo = row["LANE_NO_LIST"].ToString();
                    row["TOOLTIP"] = "(" + laneInfo + ")" + row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagSectionLaneStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                }
            }

            else if (chartDisplayType == ChartDisplayType.TagSectionSingle)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    double sourceStartPosition;
                    double.TryParse(Util.NVC(row["SOURCE_ADJ_STRT_PSTN"]), out sourceStartPosition);

                    row["SOURCE_ADJ_Y_PSTN"] = 99;
                    row["TAGNAME"] = "M";
                    row["TOOLTIP"] = row["EQPT_MEASR_PSTN_NAME"].GetString() + "[" + $"{row["SOURCE_ADJ_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_ADJ_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAG"] = null;
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
            newRow["EQSGID"] = _lotInfo.EquipmentSegmentCode;   // LoginInfo.CFG_EQSG_ID;
            newRow["PROCID"] = _lotInfo.ProcessCode;            // LoginInfo.CFG_PROC_ID;
            newRow["EQPTID"] = _lotInfo.EquipmentCode;          // LoginInfo.CFG_EQPT_ID;
            inTable.Rows.Add(newRow);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (menuHitReuslt, menuHitException) => { });

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
            dr["CONF_KEY1"] = this.ToString();

            if (gdMeasurementcombo.Visibility == Visibility.Visible)
            {
                dr["CONF_TYPE"] = "USER_CONFIG_COMBOBOX";
                dr["CONF_KEY2"] = cboMeasurementTop.Name;
                dr["CONF_KEY3"] = cboMeasurementBack.Name;
                dr["USER_CONF01"] = cboMeasurementTop.SelectedValue;
                dr["USER_CONF02"] = cboMeasurementBack.SelectedValue;
            }
            else
            {
                dr["CONF_TYPE"] = "USER_CONFIG_RADIOBUTTON";
                var radio = gdMeasurementradio.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
                dr["CONF_KEY2"] = radio.Name;
                dr["USER_CONF01"] = radio.Name;
            }
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

            dr["CONF_KEY1"] = this.ToString();

            if (gdMeasurementcombo.Visibility == Visibility.Visible)
            {
                dr["CONF_TYPE"] = "USER_CONFIG_COMBOBOX";
                dr["CONF_KEY2"] = cboMeasurementTop.Name;
                dr["CONF_KEY3"] = cboMeasurementBack.Name;
            }
            else
            {
                //gdMeasurementradio.Visibility == Visibility.Visible
                dr["CONF_TYPE"] = "USER_CONFIG_RADIOBUTTON";
                var radio = gdMeasurementradio.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
                dr["CONF_KEY2"] = radio.Name;
            }

            inTable.Rows.Add(dr);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

            if (gdMeasurementcombo.Visibility == Visibility.Visible)
            {
                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (!string.IsNullOrEmpty(dtResult.Rows[0]["USER_CONF01"].GetString()))
                    {
                        cboMeasurementTop.SelectedValue = dtResult.Rows[0]["USER_CONF01"].GetString();
                    }
                    if (!string.IsNullOrEmpty(dtResult.Rows[0]["USER_CONF02"].GetString()))
                    {
                        cboMeasurementBack.SelectedValue = dtResult.Rows[0]["USER_CONF02"].GetString();
                    }
                    cboMeasurementTop.IsEnabled = true;
                    cboMeasurementBack.IsEnabled = true;
                }
                else
                {
                    cboMeasurementTop.SelectedIndex = 0;
                    cboMeasurementBack.SelectedIndex = 0;
                    SetRollMapDefaultGauge();
                }
            }
            else
            {
                //gdMeasurementradio.Visibility == Visibility.Visible
                if (CommonVerify.HasTableRow(dtResult))
                {
                    foreach (RadioButton rdo in gdMeasurementradio.Children.OfType<RadioButton>())
                    {
                        if (string.Equals(dtResult.Rows[0]["USER_CONF01"].GetString(), rdo.Name))
                        {
                            rdo.IsChecked = true;
                            break;
                        }
                    }
                }
                else
                {
                    SetRollMapDefaultGauge();
                }
            }
        }

        private void PopupTagSection()
        {
            #region [구간불량 팝업 호출]
            CMM_RM_TAG_SECTION popRollMapPositionUpdate = new CMM_RM_TAG_SECTION();
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
            parameters[3] = _lotInfo.ProcessCode;
            parameters[4] = _lotInfo.EquipmentCode;
            parameters[5] = _lotInfo.WipSeq;
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
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = type;
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(dtResult))
            {
                string[] exceptionCode = new string[] { "OK", "NG", "Ch", "Err" };

                return (from t in dtResult.AsEnumerable()
                        where !exceptionCode.Contains(t.Field<string>("CMCODE"))
                        //orderby t.Field<Int64>("CMCDSEQ") ascending //Oracle 버전의 CMCDSEQ 컬럼 DataType Int64, MS-SQL 버전은 CMCDSEQ 컬럼 DataType decimal
                        orderby t.GetValue("CMCDSEQ").GetInt() ascending //Oracle 버전의 CMCDSEQ 컬럼 DataType Int64, MS-SQL 버전은 CMCDSEQ 컬럼 DataType decimal
                        select t).CopyToDataTable();
            }
            else
            {
                return null;
            }
        }

        private DataTable GetLaneSymbolColor(string type)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = type;
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
        }

        private void SetEqptDetailInfoByCommoncode()
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
                dr["CMCODE"] = _lotInfo.EquipmentCode;
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

        private static DataTable Get2DBarcodeInfo(string lotId, string equipmentCode)
        {
            const string bizRuleName = "DA_BAS_SEL_DFCT_2D_BCR_STD_RM";
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = lotId;
            dr["EQPTID"] = equipmentCode;
            dt.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
        }

        private string GetFirstLanePosition()
        {
            const string bizRuleName = "DA_BAS_SEL_LOTATTR";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LOTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LOTID"] = txtLot.Text;
            inTable.Rows.Add(dr);

            DataTable dtFirstLane = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtFirstLane))
                return string.IsNullOrEmpty(dtFirstLane.Rows[0]["HALF_SLIT_SIDE"].GetString()) ? GetFirstLanePositionByEquipment() : dtFirstLane.Rows[0]["HALF_SLIT_SIDE"].GetString();
            else
                return GetFirstLanePositionByEquipment();

        }

        private string GetFirstLanePositionByEquipment()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQPTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = _lotInfo.EquipmentCode;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                return dtResult.Rows[0]["FRST_LANE_PSTN"].GetString();
            }
            else
            {
                return string.Empty;
            }
        }

        private static bool IsEquipmentTotalDry(string equipmentCode)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_EQPT_TOTALDRY_USE_FLAG";
            dr["CMCODE"] = equipmentCode;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (string.Equals(dtResult.Rows[0]["ATTRIBUTE1"].GetString(), "1"))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        private void SetMeasurementComboBox()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_MEASR_PSTN_CBO";
            dr["CMCODE"] = null;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
            //DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<Int64>("CMCDSEQ")).CopyToDataTable(); //Oracle 버전의 CMCDSEQ 컬럼 DataType Int64, MS-SQL 버전은 CMCDSEQ 컬럼 DataType decimal
            DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.GetValue("CMCDSEQ").GetInt()).CopyToDataTable(); //Oracle 버전의 CMCDSEQ 컬럼 DataType Int64, MS-SQL 버전은 CMCDSEQ 컬럼 DataType decimal

            if (_isEquipmentTotalDry)
            {
                cboMeasurementBack.ItemsSource = dtBinding.Copy().AsDataView();
                cboMeasurementBack.SelectedIndex = 0;

                dtBinding.AsEnumerable().Where(r => r.Field<string>("CMCODE") == "TD").ToList().ForEach(row => row.Delete());
                dtBinding.AcceptChanges();
                cboMeasurementTop.ItemsSource = dtBinding.Copy().AsDataView();
                cboMeasurementTop.SelectedIndex = 0;
            }
            else
            {
                dtBinding.AsEnumerable().Where(r => r.Field<string>("CMCODE") == "TD").ToList().ForEach(row => row.Delete());
                dtBinding.AcceptChanges();
                cboMeasurementTop.ItemsSource = dtBinding.Copy().AsDataView();
                cboMeasurementBack.ItemsSource = dtBinding.Copy().AsDataView();

                cboMeasurementTop.SelectedIndex = 0;
                cboMeasurementBack.SelectedIndex = 0;
            }
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
                const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT";
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _lotInfo.ProcessCode;
                dr["EQSGID"] = _lotInfo.EquipmentSegmentCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
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

        private void IsShowDefectButton()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("LANGID", typeof(string));
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CMCODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_DEF_COMP_CT";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
            btnDefect.Visibility = CommonVerify.HasTableRow(dtResult) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool IsNewLotInfo(string lotId)
        {
            bool bRst = false;

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("GUBUN", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;
                dr["GUBUN"] = "F";//F/R

                inTable.Rows.Add(dr);

                // CSR : [E20231018-000975] - 전극 재와인딩 공정 추가 Biz 분리 처리 [동별 공통코드 : REWINDING_LOT_INFO_TREE]
                string bizRuleName;

                DataTable dtRewinderLot = GetAreaCommonCode("REWINDING_LOT_INFO_TREE", null);

                if (CommonVerify.HasTableRow(dtRewinderLot))
                {
                    bizRuleName = "BR_PRD_SEL_LOT_INFO_END_ELEC_LV";
                }
                else
                {
                    bizRuleName = "BR_PRD_SEL_LOT_INFO_END";
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "LOTSTATUS,TREEDATA", inDataSet);

                if (CommonVerify.HasTableInDataSet(dsResult) && CommonVerify.HasTableRow(dsResult.Tables["LOTSTATUS"]))
                {
                    string areaType = string.Empty;

                    if (CommonVerify.HasTableRow(dsResult.Tables["TREEDATA"]))
                    {
                        areaType = dsResult.Tables["TREEDATA"].Rows[0]["AREATYPE"].ToString();
                    }

                    string processCode = dsResult.Tables["LOTSTATUS"].Rows[0]["PROCID"].ToString();

                    //이전 조회시 공정과 같은 경우에만 진행
                    if (string.Equals(processCode, _lotInfo.ProcessCode))
                    {
                        SetLotInfo(lotId, processCode, areaType);
                        bRst = true;
                    }
                    else
                    {
                        //같은 공정이 아닙니다.
                        Util.MessageValidation("SFU1446");
                        txtLot.Text = _lotInfo.RunlotId;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtLot.Text = _lotInfo.RunlotId;
            }

            return bRst;
        }

        private void SetRollMapDefaultGauge()
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
                newRow["COM_CODE"] = _lotInfo.ProcessCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);


                if (gdMeasurementradio.Visibility == Visibility.Visible)
                {
                    if (CommonVerify.HasTableRow(dtResult))
                    {
                        if (string.Equals("W", dtResult.Rows[0]["ATTR1"].GetString()))
                        {
                            rdoWebgaugeWet.IsChecked = true;
                        }
                        else if (string.Equals("T", dtResult.Rows[0]["ATTR1"].GetString()))
                        {
                            rdoThickness.IsChecked = true;
                        }
                        else
                        {
                            rdoWebgaugeDry.IsChecked = true;
                        }
                    }
                    else
                        rdoWebgaugeDry.IsChecked = true;
                }
                else
                {

                    foreach (C1ComboBox comboBox in Util.FindVisualChildren<C1ComboBox>(gdMeasurementcombo))
                    {
                        if (CommonVerify.HasTableRow(dtResult))
                        {
                            if (string.Equals("W", dtResult.Rows[0]["ATTR1"].GetString()))
                            {
                                comboBox.SelectedValue = "WL";
                            }
                            else if (string.Equals("T", dtResult.Rows[0]["ATTR1"].GetString()))
                            {
                                comboBox.SelectedValue = "WT";
                            }
                            else
                            {
                                comboBox.SelectedValue = "DL";
                            }
                        }
                        else
                        {
                            comboBox.SelectedValue = "DL";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
            newRow["COM_CODE"] = _lotInfo.ProcessCode;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                // 계측기 콤보박스 Visibility 속성
                if (string.Equals(dtResult.Rows[0]["ATTR1"].GetString(), "Y"))
                {
                    gdMeasurementcombo.Visibility = Visibility.Visible;
                    gdMeasurementradio.Visibility = Visibility.Collapsed;
                }
                else
                {
                    gdMeasurementcombo.Visibility = Visibility.Collapsed;
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
                gdMeasurementcombo.Visibility = Visibility.Visible;
                gdMapExpress.Visibility = Visibility.Collapsed;
                gdColorMap.Visibility = Visibility.Collapsed;
            }
        }

        private DataTable GetAreaCommonCode(string cmcdType, string cmCode = null)
        {
            string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = cmcdType;
                newRow["COM_CODE"] = cmCode;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetLotInfo(string lotId, string processCode, string areaType)
        {
            try
            {
                string bizRuleName = areaType.Equals("A") ? "DA_PRD_SEL_LOT_STATUS2_ASSY" : "DA_PRD_SEL_LOT_STATUS2";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = lotId;
                dr["PROCID"] = processCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (!areaType.Equals("A") && CommonVerify.HasTableRow(dtResult))
                {
                    _lotInfo.RunlotId = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    _lotInfo.WipSeq = Util.NVC(dtResult.Rows[0]["WIPSEQ"].GetString());
                    _lotInfo.ProcessCode = Util.NVC(dtResult.Rows[0]["PROCID"]);
                    _lotInfo.LaneQty = Util.NVC(dtResult.Rows[0]["LANE_QTY"]);

                    if (!Equals(_lotInfo.EquipmentCode, dtResult.Rows[0]["ROLLMAP_EQPTID"]))
                    {
                        ChangeEquipment(dtResult.Rows[0]["ROLLMAP_EQPTID"].GetString());
                    }

                    _lotInfo.EquipmentCode = Util.NVC(dtResult.Rows[0]["ROLLMAP_EQPTID"]);
                    _lotInfo.EquipmentName = Util.NVC(dtResult.Rows[0]["ROLLMAP_EQPTNAME"]);
                    _lotInfo.EquipmentSegmentCode = Util.NVC(dtResult.Rows[0]["EQSGID"]);
                    _lotInfo.Version = Util.NVC(dtResult.Rows[0]["PROD_VER_CODE"]);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeEquipment(string equipmentCode)
        {
            //설비 코드 변경 시 극성 및 Total Dry 설비 확인
            DataTable dt = SelectEquipmentPolarity(equipmentCode);

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
            }

            _isEquipmentTotalDry = IsEquipmentTotalDry(equipmentCode);

            if (gdMeasurementcombo.Visibility == Visibility.Visible)
            {
                string selectedMeasurementTop = cboMeasurementTop.SelectedValue.GetString();
                string selectedMeasurementBack = cboMeasurementBack.SelectedValue.GetString();

                const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "ROLLMAP_MEASR_PSTN_CBO";
                dr["CMCODE"] = null;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.GetValue("CMCDSEQ").GetInt()).CopyToDataTable(); //Oracle 버전의 CMCDSEQ 컬럼 DataType Int64, MS-SQL 버전은 CMCDSEQ 컬럼 DataType decimal

                if (_isEquipmentTotalDry)
                {
                    cboMeasurementBack.ItemsSource = dtBinding.Copy().AsDataView();

                    dtBinding.AsEnumerable().Where(r => r.Field<string>("CMCODE") == "TD").ToList().ForEach(row => row.Delete());
                    dtBinding.AcceptChanges();
                    cboMeasurementTop.ItemsSource = dtBinding.Copy().AsDataView();
                    cboMeasurementTop.SelectedValue = selectedMeasurementTop;
                }
                else
                {
                    dtBinding.AsEnumerable().Where(r => r.Field<string>("CMCODE") == "TD").ToList().ForEach(row => row.Delete());
                    dtBinding.AcceptChanges();
                    cboMeasurementTop.ItemsSource = dtBinding.Copy().AsDataView();
                    cboMeasurementBack.ItemsSource = dtBinding.Copy().AsDataView();

                    cboMeasurementTop.SelectedValue = selectedMeasurementTop;
                    cboMeasurementBack.SelectedValue = selectedMeasurementBack;
                }
            }
        }

        private bool SelectRollMapLot()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_RM_RPT_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = txtLot.Text;
                dr["WIPSEQ"] = _lotInfo.WipSeq;
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
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["COM_TYPE_CODE"] = "ROLLMAP_LANE";
            //newRow["COM_CODE"] = _processCode;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<string>("COM_CODE").GetDecimal()).CopyToDataTable();

            if (CommonVerify.HasTableRow(dtBinding))
            {
                msb.ItemsSource = DataTableConverter.Convert(dtBinding);
            }
        }

        private void SetRollMapExpressSummaryComboBox(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["COM_TYPE_CODE"] = "ROLLMAP_EXPRESS_SUMMARY";
            //newRow["COM_CODE"] = _processCode;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            dtResult = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<string>("COM_CODE").GetDecimal()).CopyToDataTable();

            if (CommonVerify.HasTableRow(dtResult))
            {
                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
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

        private bool ValidationTagSection()
        {
            if (chartCoater.Data.Children.Count < 1)
            {
                Util.MessageValidation("SFU1498");
                return false;
            }

            return true;
        }

        private enum ChartDisplayType
        {
            TagSectionStart,
            TagSectionEnd,
            TagSectionLaneStart,
            TagSectionLaneEnd,
            TagSectionSingle,
            Mark,
            Material,
            Sample,
            TagSpot
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

        #endregion

    }
}