/*************************************************************************************
 Created Date : 2023.01.16
      Creator : 신광희
   Decription : 코터 롤맵 수정 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.16  신광희 : Initial Created.
  2023.01.30  신광희 : 절대좌표 선택 시 구간불량 병합 및 등록 제한 처리
  2023.02.20  신광희 : CSR(E20230216-000010) 코팅 유무 표시 (코터 롤맵 차트 원재료 영역 추가)
  2023.02.21  윤기업 : 실적 조회 만 되는 기능 분리
  2023.02.28  신광희 : ERP 생산실적이 마감 체크 로직 추가
  2023.03.02  신광희 : 롤맵 수집유형(ROLLMAP_CLCT_TYPE) 171, 172 코드인 경우는 LOSS_LOT 데이터만 선택되도록 조건 처리
  2023.03.02  윤기업 : [E20230216-000013] Coater Chart에 생산량 및 양품량 표시
  2023.03.06  신광희 : 구간불량 병합 정상 저장인 경우 IsUpdated true 처리
  2023.03.21  신광희 : 무지부 Tab 위치 표시
  2023.06.12  황석동 : TAG 삭제 기능 추가
  2023.08.22  신광희 : 
  2023.12.13  조성근 : 롤맵 코터 수불 실적 적용
  2024.01.08  신광희 : 무지부 표시 영역 산출식 개선 함. 
  2024.01.31  김지호 : 팝업창을 닫을시에 메시지박스 출력
  2024.02.13  정기동 : [E20240115-000246] 코터 롤맵 Material CHART의 슬러리에 믹서 버퍼의 이전/이후 배치 ID를 툴팁으로 표기. (믹서-코터 배치연계 고도화)
  2024.03.13  김지호 : [E20240223-001703] CHART TOP/BACK 분리 및 기존 계측기 라디오 버튼에서 콤보박스로 변경
  2024.03.13  김지호 : 생산실적 조회(LOT 기준)에서 팝업창을 열고 닫을시에만 메시지박스 출력
  2024.03.19  김지호 : [E20240319-000656] 범례에 Non 추가 및 차트 색 (회색) 추가, SCAN_COLRMAP에 Err, Non, Ch 시에 Tooltip에 데이터 오류, 데이터 미수신, 데이터 미측정 내용 표시
  2024.03.31  김지호 : [긴급배포] 이후 이전 버튼 상관 없이 예전처럼 sampleFlag 기존에 'Y'로 고정으로 변경
  2024.03.31  김지호 : 이후 버튼 선택시(상대좌표:샘플컷 제외) 구간불량 등록 가능하도록 수정 / 샘플 길이 보다 작은 위치 선택하여 구간불량 등록시에 메시지 뜨는 부분 삭제 / Foil 교체시 빨간선으로 표시
  2024.04.17  김지호 : 이후 이전 버튼 상관 없이 예전처럼 sampleFlag 기존에 'Y'로 고정으로 변경 / 공정별 생산실적 수정(실적관리자) 화면에서만 호출 되고 닫을때 팝업창 출력
  2024.08.10  김지호 : E20240724-000827 TAG_SECTION_SINGLE 추가에 따른 수정
  2024.08.18  김지호 : [E20240729-000141] BEFORE 선택 시 SEARCH 버튼 사라지는 현상 수정 및 기존 chkTotal 체크버튼 삭제로 인한 코드 정리
  2024.09.05  김지호 : 측정위치 콤보박스 열면 항목이 보이지 않는 오류 수정 (기존에 콤보박스 항목을 하드코딩으로 넣는 것을 공통코드에서 불러오도록 수정)
  2024.09.05  신광희 : [E20240905-001378]ERP 생산실적 마감 이후 불량/LOSS 등록없이 Defect Section 추가 기능하도록 수정, 소스코드 리팩토링(사용하지 않는 변수, 메소드,이벤트 제거.. 등)
  2024.09.09  김지호 : [E20240731-000740] 측정위치 콤보박스 열면 항목이 보이지 않는 오류 수정 (기존에 콤보박스 항목을 하드코딩으로 넣는 것을 공통코드에서 불러오도록 수정)
  2024.09.11  신광희 : [E20240905-001378] UserControl_Closed 시 메세지 주석처리
  2024.09.19  김지호 : [E20240905-001389] TAG_SECTION 툴팁내 불량 명칭 및 총 거리값 표시되도록 수정
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System.Data;
using System;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using C1.WPF;
using System.Windows.Media;
using C1.WPF.C1Chart;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using Action = System.Action;
using ColorConverter = System.Windows.Media.ColorConverter;
using System.Windows.Input;
using System.Diagnostics;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_ROLLMAP_COATER_DEFECT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation { get; set; }

        public bool IsUpdated { get; set; }
        private readonly Util _util = new Util();
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _runLotId = string.Empty;
        private string _wipSeq = string.Empty;
        private string _laneQty = string.Empty;
        private double _xMaxLength;
        private double _sampleCutLength;
        private string _polarityCode = string.Empty;
        private string _version = string.Empty;
        private bool _isTestCutVisible = false;
        private DataTable _dtPoint;
        private DataTable _dtGraph;
        private DataTable _dtDefect;
        private DataTable _dtLaneInfo;
        private DataTable _dtLaneLegend;
        private DataTable _dtLane;
        private DataTable _dtTagSection;
        private DataTable _dtTagSpot;
        private DataTable _dtBackReason;
        private DataTable _dtColorMapLegend;
        private bool _isShowBtnDeft = false;	//nathan 2023.12.13 롤맵 코터 수불 실적 적용
        private bool _bUseSampleCut = true;	//nathan 2024.01.17 롤맵용 테스트 컷 적용
        private DataRow _drADJInfo_NonArea; //nathan 2024.01.17 롤맵용 테스트 컷 적용

        private bool _isResearch;

        private string _selectedStartSectionText = string.Empty;
        private string _selectedStartSectionPosition = string.Empty;
        private string _selectedStartSampleFlag = string.Empty;
        private string _selectedEndSectionPosition = string.Empty;
        private string _selectedEndSampleFlag = string.Empty;
        private string _selectedCollectType = string.Empty;

        private bool _isSelectedTagSection;
        private double _selectedStartSection = 0;
        private double _selectedEndSection = 0;
        private double _selectedchartPosition = 0;

        private double dSumTopLoss = 0;
        private double dSumBackLoss = 0;

        private bool _isRollMapLot;
        private bool _isSearchMode; // Search Mode

        private bool _isEquipmentTotalDry = false; // 롤맵 설비 Total Dry 사용 유무

        private CoordinateType _CoordinateType;
        private enum CoordinateType
        {
            RelativeCoordinates,    //상대좌표
            Absolutecoordinates     //절대좌표
        }

        public CMM_ROLLMAP_COATER_DEFECT()
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
                _isTestCutVisible = (Util.NVC(parameters[8])).Equals("Y") ? true : false;
                _isSearchMode = (Util.NVC(parameters[9])).Equals("Y") ? true : false;
                _CoordinateType = CoordinateType.RelativeCoordinates;
            }

            Initialize();
            GetErpClose();
            GetLegend();
            GetRollMap();
            this.Closed += UserControl_Closed;
        }

        private void UserControl_Closed(object sender, EventArgs e)
        {
            try
            {
                SetUserConfigurationControl();

                //if (_isTestCutVisible && !_isSearchMode)
                //    Util.MessageValidation("SFU9025");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearch()) return;

                _isResearch = false;

                ShowLoadingIndicator();
                DoEvents();

                // 데이터 테이블 초기화
                InitializeDataTable();
                // TAG SECTION 관련 변수 초기화
                InitializeTagSectionControl();
                // 컨트롤 초기화
                InitializeControls();

                double min = 0;
                double max = 0;
                double maxLength = 0;
                string sampleFlag = "Y";
                //string sampleFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "N" : "Y";                
                string adjFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
                //string mPoint = rdoThick.IsChecked != null && (bool)rdoThick.IsChecked ? "1" : "2";

                string mPoint_top = string.Empty;
                string mPstn_top = string.Empty;
                string mPoint_back = string.Empty;
                string mPstn_back = string.Empty;

                if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(1, 1), "L"))
                    mPoint_top = "1";
                else
                    mPoint_top = "2";

                if (string.Equals(cboMeasurementBack.SelectedValue.GetString(), "TD"))
                {
                    mPoint_back = "3";
                }
                else
                {
                    if (string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(1, 1), "L"))
                        mPoint_back = "1";
                    else
                        mPoint_back = "2";
                }

                //DRY(로딩량), DRY(두께)
                if (string.Equals(cboMeasurementTop.SelectedValue.GetString().Substring(0, 1), "D"))
                    mPstn_top = "2";
                else
                    mPstn_top = "1";  //WET(로딩량), WET(두께), TOTAL_DRY


                if (string.Equals(cboMeasurementBack.SelectedValue.GetString().Substring(0, 1), "D"))
                    mPstn_back = "2";
                else
                    mPstn_back = "1";


                if (rdoRelativeCoordinates != null && (bool)rdoRelativeCoordinates.IsChecked)
                    _CoordinateType = CoordinateType.RelativeCoordinates;
                else
                    _CoordinateType = CoordinateType.Absolutecoordinates;


                // Max Length 조회
                DataTable dtLength = SelectRollMapLength(txtLot.Text, _wipSeq, adjFlag, sampleFlag, _equipmentCode);
                if (!CommonVerify.HasTableRow(dtLength))
                {
                    SetCoaterChartContextMenu(false);
                    return;
                }
                else
                {
                    var query = (from t in dtLength.AsEnumerable()
                                 where t.Field<string>("EQPT_MEASR_PSTN_ID") == "RW"
                                 select new
                                 {
                                     sourceEndPosition = t.Field<decimal>("SOURCE_END_PSTN"),
                                     ProdQty = t.Field<decimal?>("INPUT_QTY"),
                                     GoodQty = t.Field<decimal?>("WIPQTY_ED")
                                 }).FirstOrDefault();
                    if (query != null)
                    {
                        maxLength = query.sourceEndPosition.GetDouble();
                        tbOutputProdQty.Text = @"P" + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                        tbOutputProdQty.ToolTip = ObjectDic.Instance.GetObjectName("생산량") + " : " + $"{query.ProdQty.GetDouble():###,###,###,##0.##}";
                        tbOutputGoodQty.Text = @"G" + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                        tbOutputGoodQty.ToolTip = ObjectDic.Instance.GetObjectName("양품량") + " : " + $"{query.GoodQty.GetDouble():###,###,###,##0.##}";
                    }
                    else maxLength = 0;

                    _xMaxLength = maxLength;

                    // RW, UW 조회 및 Display
                    //DrawCicleLengthChart(dtLength);

                    min = dtLength.AsEnumerable().ToList().Min(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
                    max = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
                }

                if (!_isSearchMode)
                    SetCoaterChartContextMenu(true);

                // 원재료 데이터 조회
                DataTable dtMaterial = SelectRollMapMaterialInfo(txtLot.Text, _wipSeq, adjFlag, sampleFlag);
                if (CommonVerify.HasTableRow(dtMaterial))
                {
                    DrawMaterialChart(dtMaterial, min, max);
                }

                // 측정기 맵, 그래프 조회 lotId, wipSeq, mPoint, position, adjFlag
                DataTable dtGraph = SelectRollMapGraphInfo(txtLot.Text, _wipSeq, mPoint_top, mPstn_top, mPoint_back, mPstn_back, adjFlag, _equipmentCode, sampleFlag);
                if (CommonVerify.HasTableRow(dtGraph))
                {
                    //_dtGraph = dtGraph.Copy();

                    _dtGraph = dtGraph.AsEnumerable()
                        .Select(row =>
                        {
                            switch (row.Field<string>("SCAN_COLRMAP"))
                            {
                                case "Ch":
                                    row.SetField("CMCDNAME", "데이터 미측정");
                                    break;
                                case "Non":
                                    row.SetField("CMCDNAME", "데이터 미수신");
                                    break;
                                case "Err":
                                    row.SetField("CMCDNAME", "데이터 오류");
                                    break;
                            }
                            return row;
                        }).CopyToDataTable();

                    DrawChartBackGround(dtGraph, maxLength); // 코터 Chart 디자인
                }
                else
                {
                    InitializeCoaterChart();
                }

                DrawRollMapLane();


                // Defect, tag, Vision, Mark..
                DataTable dtPoint = SelectRollMapPointInfo(_equipmentCode, txtLot.Text, _wipSeq, adjFlag, sampleFlag);
                if (CommonVerify.HasTableRow(dtPoint))
                {
                    _dtPoint = dtPoint.Copy();
                    DrawPointChart(dtPoint);
                }

                // RollPress 공정은 샘플 라인이 없음.
                DrawRollMapSampleYAxisLine();

                // 데이터가 없는 경우 무지부를 표현 함.
                if (chartCoater.Data.Children.Count < 1)
                {
                    InitializeCoaterChart();
                }
                GetRollMapTagSectionList();
                GetRollMapOutsideScrapList();
                EndUpdateChart();

                setTestCut_NonArea();  //nathan 2024.01.17 롤맵용 테스트 컷 적용

            }
            catch (Exception ex)
            {
                EndUpdateChart();
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                SetMenuUseCount();
                HiddenLoadingIndicator();
            }
        }

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {
            CMM_ROLLMAP_COATER_DATACOLLECT popupRollMapDataCollect = new CMM_ROLLMAP_COATER_DATACOLLECT();
            popupRollMapDataCollect.FrameOperation = FrameOperation;
            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = _runLotId;
            //parameters[2] = txtLot.Text;
            parameters[3] = _wipSeq;
            parameters[4] = _laneQty;
            //parameters[4] = _equipmentName;
            //parameters[5] = string.Empty;

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
                IsUpdated = true;
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

            CMM_ROLLMAP_PSTN_UPD popRollMapPositionUpdate = new CMM_ROLLMAP_PSTN_UPD();
            popRollMapPositionUpdate.FrameOperation = FrameOperation;

            object[] parameters = new object[11];
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
            parameters[10] = Visibility.Visible;
            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }

        private void popRollMapPositionUpdate_Closed(object sender, EventArgs e)
        {
            CMM_ROLLMAP_PSTN_UPD popup = sender as CMM_ROLLMAP_PSTN_UPD;
            if (popup != null && popup.IsUpdated)
            {
                IsUpdated = true;
                //btnSearch_Click(btnSearch, null);
                SelectRollMapTagSection();
                InitializeTagSectionControl();
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
                IsUpdated = true;
                SelectRollMapTagSection();
            }
            else
            {
                DrawRollMapTagSection(_dtTagSection);
            }

            InitializeTagSectionControl();
        }

        private void rdoAbsolutecoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
        }

        private void rdoRelativeCoordinates_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
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
            parameters[9] = _xMaxLength - _sampleCutLength;
            parameters[10] = _isSearchMode ? Visibility.Collapsed : Visibility.Visible;
            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }

        private void tbTagSection_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", ObjectDic.Instance.GetObjectName("구간불량"));

                return;
            }
        }


        private void tagMerge_Click(object sender, RoutedEventArgs e)
        {
            if (_isSearchMode) return;

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
                    CMM_ROLLMAP_TAGSECTION_MERGE popRollMapTagsectionMerge = new CMM_ROLLMAP_TAGSECTION_MERGE();
                    popRollMapTagsectionMerge.FrameOperation = FrameOperation;

                    object[] parameters = new object[8];
                    parameters[0] = txtLot.Text;
                    parameters[1] = _wipSeq;
                    parameters[2] = _selectedStartSectionPosition;
                    parameters[3] = _selectedEndSectionPosition;
                    parameters[4] = _processCode;
                    parameters[5] = _equipmentCode;
                    parameters[6] = _selectedCollectType;
                    parameters[7] = _selectedEndSampleFlag;

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
        //ighkds77 tag 삭제 추가
        private void tagDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_isSearchMode) return;

            MenuItem itemmenu = sender as MenuItem;
            ContextMenu itemcontext = itemmenu.Parent as ContextMenu;
            TextBlock textBlock = itemcontext.PlacementTarget as TextBlock;

            if (textBlock == null) return;

            //Border border = FindVisualParentByName<Border>(textBlock, "tbTagSection");
            string[] splitItem = textBlock.Tag.GetString().Split(';');

            double sourceStartPosition;
            double.TryParse(Util.NVC(splitItem[4]), out sourceStartPosition);

            string sMsg = string.Empty;

            sMsg = "TAG : " + sourceStartPosition + "m";

            Util.MessageConfirm("SFU3475", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        //BizActor 호출 TAG 삭제
                        const string bizRuleName = "DA_PRD_UPD_ROLLMAP_TAGSPOT_DEL";

                        DataTable inTable = new DataTable();
                        inTable.TableName = "RQSTDT";
                        inTable.Columns.Add("LOTID", typeof(string));
                        inTable.Columns.Add("WIPSEQ", typeof(Decimal));
                        inTable.Columns.Add("CLCT_SEQNO", typeof(int));
                        inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                        inTable.Columns.Add("USERID", typeof(string));


                        DataRow newRow = inTable.NewRow();


                        newRow["LOTID"] = splitItem[0];
                        newRow["WIPSEQ"] = splitItem[1];
                        newRow["CLCT_SEQNO"] = splitItem[2];
                        newRow["EQPT_MEASR_PSTN_ID"] = splitItem[3];
                        newRow["USERID"] = LoginInfo.USERID;

                        inTable.Rows.Add(newRow);


                        DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                        //btnSearch_Click(btnSearch, null);

                        IsUpdated = true;
                        SelectRollMapTagSpot();


                        //Util.MessageInfo("SFU1273");      // 삭제되었습니다.

                        return;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;

                    }
                }
            }, sMsg);

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

        private void chartCoater_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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

            //if (_selectedchartPosition < _sampleCutLength)
            //{
            //    Util.MessageValidation("SFU8542");
            //    return;
            //}

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
            dr["TOOLTIP"] = ObjectDic.Instance.GetObjectName("시작위치") + " [" + ObjectDic.Instance.GetObjectName("길이") + $"{tagsectionStart:###,###,###,##0.##}" + "m" + " ]";
            dr["AXISX_PSTN"] = _selectedStartSection;
            dr["AXISY_PSTN"] = 95;

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

            //if (_sampleCutLength > _selectedStartSection  && _sampleCutLength < _selectedchartPosition)
            //{                    
            //    Util.MessageInfo("SFU8542");
            //    return;
            //}

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
            dr["TOOLTIP"] = ObjectDic.Instance.GetObjectName("종료위치") + " [" + ObjectDic.Instance.GetObjectName("길이") + $"{tagsectionEnd:###,###,###,##0.##}" + "m" + " ]";
            dr["AXISX_PSTN"] = _selectedEndSection;
            dr["AXISY_PSTN"] = 95;

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

            InitializeTagSectionControl();
        }

        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb?.DataContext == null) return;

            if (rb.IsChecked != null)
            {
                DataRowView drv = rb.DataContext as DataRowView;
                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgLotInfo.SelectedIndex = idx;

                    // 차트에 바인딩 된 Tag Section 찾기
                    FindTagSection(idx);
                }
            }
        }

        private void dgLotInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            string[] exceptionColumn = { "TOP_PROD_QTY", "BACK_PROD_QTY" };

            if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
            {
                e.Cancel = e.Column.Name != "CHK"; // && drv["CHK"].GetString() != "1";
            }
            else
            {
                if (e.Column.Name == "CHK")
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = exceptionColumn.Contains(e.Column.Name);// || drv != null && drv["CHK"].GetString() != "1";
                }
            }
        }

        private void dgLotInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (drv == null) return;

                // 시작위치 종료위치 확인
                if (e.Cell.Column.Name == "ADJ_STRT_PSTN" || e.Cell.Column.Name == "ADJ_END_PSTN")
                {
                    if (DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() > DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble())
                    {
                        Util.MessageInfo("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return;
                    }

                    //BizActor 호출 Top, Back 양품량 계산
                    const string bizRuleName = "DA_PRD_SEL_ROLLMAP_PROD_QTY";

                    DataTable inTable = new DataTable();
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("WIPSEQ", typeof(decimal));
                    inTable.Columns.Add("CLCT_SEQNO", typeof(int));
                    inTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
                    inTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));

                    DataRow newRow = inTable.NewRow();

                    newRow["EQPTID"] = _equipmentCode;
                    newRow["LOTID"] = DataTableConverter.GetValue(drv, "LOTID").GetString();
                    newRow["WIPSEQ"] = DataTableConverter.GetValue(drv, "WIPSEQ").GetDecimal();
                    newRow["CLCT_SEQNO"] = DataTableConverter.GetValue(drv, "CLCT_SEQNO").GetInt();
                    newRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDecimal();
                    newRow["ADJ_END_PSTN"] = DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDecimal();
                    inTable.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                    if (CommonVerify.HasTableRow(dtResult))
                    {
                        DataTableConverter.SetValue(drv, "TOP_PROD_QTY", dtResult.Rows[0]["TOP_PROD_QTY"].GetDecimal());
                        DataTableConverter.SetValue(drv, "BACK_PROD_QTY", dtResult.Rows[0]["BACK_PROD_QTY"].GetDecimal());
                        DataTableConverter.SetValue(drv, "FOIL_QTY", dtResult.Rows[0]["FOIL_QTY"].GetDecimal());
                    }



                    //BizActor 호출 설비보고 TAG SECTION 변경여부 체크 ighkds77
                    const string bizRuleName2 = "BR_PRD_CHK_ROLLMAP_TAG_SECTION_CHANGE";

                    DataTable inTable2 = new DataTable();
                    inTable2.TableName = "RQSTDT";

                    inTable2.Columns.Add("LOTID", typeof(string));
                    inTable2.Columns.Add("WIPSEQ", typeof(decimal));
                    inTable2.Columns.Add("ROLLMAP_ADJ_INFO_TYPE_CODE", typeof(string));
                    inTable2.Columns.Add("CLCT_SEQNO", typeof(int));
                    inTable2.Columns.Add("STRT_PSTN1", typeof(decimal));
                    inTable2.Columns.Add("END_PSTN1", typeof(decimal));

                    DataRow newRow2 = inTable2.NewRow();

                    newRow2["LOTID"] = DataTableConverter.GetValue(drv, "LOTID").GetString();
                    newRow2["WIPSEQ"] = DataTableConverter.GetValue(drv, "WIPSEQ").GetDecimal();
                    newRow2["ROLLMAP_ADJ_INFO_TYPE_CODE"] = "TAG_SECTION";
                    newRow2["CLCT_SEQNO"] = DataTableConverter.GetValue(drv, "CLCT_SEQNO").GetInt();
                    newRow2["STRT_PSTN1"] = DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDecimal();
                    newRow2["END_PSTN1"] = DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDecimal();
                    inTable2.Rows.Add(newRow2);

                    DataTable dtResult2 = new ClientProxy().ExecuteServiceSync(bizRuleName2, "RQSTDT", "RSLTDT", inTable2);


                    if (CommonVerify.HasTableRow(dtResult2))
                    {

                        if (dtResult2.Rows[0]["CHANGE_YN"].GetString() == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }

                    }


                }

                if (e.Cell.Column.Name == "ADJ_STRT_PSTN")
                {
                    if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_STRT_PSTN").GetDouble()) > 0)
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                    }
                    else
                    {
                        if (string.Equals(DataTableConverter.GetValue(drv, "BACK_RESNCODE").GetString(), DataTableConverter.GetValue(drv, "OLD_BACK_RESNCODE").GetString())
                            && DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() == DataTableConverter.GetValue(drv, "OLD_ADJ_END_PSTN").GetDouble())
                        {
                            DataTableConverter.SetValue(drv, "UPDATE_FLAG", "N");
                        }
                        else
                        {
                            DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                        }

                    }
                }
                else if (e.Cell.Column.Name == "ADJ_END_PSTN")
                {
                    if (Math.Abs(DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() - DataTableConverter.GetValue(drv, "OLD_ADJ_END_PSTN").GetDouble()) > 0)
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                    }
                    else
                    {
                        if (string.Equals(DataTableConverter.GetValue(drv, "BACK_RESNCODE").GetString(), DataTableConverter.GetValue(drv, "OLD_BACK_RESNCODE").GetString())
                            && DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() == DataTableConverter.GetValue(drv, "OLD_ADJ_STRT_PSTN").GetDouble())
                        {
                            DataTableConverter.SetValue(drv, "UPDATE_FLAG", "N");
                        }
                        else
                        {
                            DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                        }
                    }
                }

                else if (e.Cell.Column.Name == "BACK_RESNCODE")
                {
                    if (!string.Equals(DataTableConverter.GetValue(drv, "BACK_RESNCODE").GetString(), DataTableConverter.GetValue(drv, "OLD_BACK_RESNCODE").GetString()))
                    {
                        DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                    }
                    else
                    {
                        if (DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDouble() == DataTableConverter.GetValue(drv, "OLD_ADJ_STRT_PSTN").GetDouble()
                            && DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDouble() == DataTableConverter.GetValue(drv, "OLD_ADJ_END_PSTN").GetDouble())
                        {
                            DataTableConverter.SetValue(drv, "UPDATE_FLAG", "N");
                        }
                        else
                        {
                            DataTableConverter.SetValue(drv, "UPDATE_FLAG", "Y");
                        }
                    }
                }
                dgLotInfo.EndEdit();
                dgLotInfo.EndEditRow(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotInfo_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (dgLotInfo.CurrentRow == null || dgLotInfo.SelectedIndex == -1 || dgLotInfo.CurrentColumn == null)
                {
                    return;
                }

                int rowIndex = dgLotInfo.CurrentRow.Index;
                dgLotInfo.SelectedIndex = rowIndex;

                dgLotInfo.GetCell(rowIndex, dgLotInfo.Columns["CHK"].Index);
                if (dgLotInfo.GetCell(rowIndex, dgLotInfo.Columns["CHK"].Index).Presenter == null) return;

                RadioButton rb = dgLotInfo.GetCell(rowIndex, dgLotInfo.Columns["CHK"].Index).Presenter.Content as RadioButton;

                if (rb?.DataContext == null) return;

                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (rowIndex == i)
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                FindTagSection(rowIndex);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    if (sender == null)
        //        return;

        //    C1DataGrid dg = sender as C1DataGrid;

        //    dg?.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (e.Cell.Presenter == null)
        //        {
        //            return;
        //        }

        //        if (e.Cell.Row.Type == DataGridRowType.Item)
        //        {
        //            string adjchangeYn = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ADJ_CHANGE_YN"));


        //            if (e.Cell.Column.Name.ToString() == "ADJ_STRT_PSTN" || e.Cell.Column.Name.ToString() == "ADJ_END_PSTN")
        //            {
        //                if (string.Equals(adjchangeYn, "Y"))
        //                {
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
        //                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
        //                }
        //                else
        //                {
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
        //                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
        //                }
        //            }
        //        }
        //    }));
        //}

        //private void dgLotInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        //{
        //    C1DataGrid dataGrid = sender as C1DataGrid;

        //    if (dataGrid != null)
        //    {
        //        dataGrid.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            if (e != null && e.Cell != null && e.Cell.Presenter != null)
        //            {
        //                if (e.Cell.Row.Type == DataGridRowType.Item)
        //                {
        //                    e.Cell.Presenter.Background = null;
        //                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
        //                }
        //            }
        //        }));
        //    }
        //}

        private void dgLotInfo_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;
            DataRowView drv = e.Row.DataItem as DataRowView;

            //롤맵 수집유형(ROLLMAP_CLCT_TYPE) 171, 172 코드인 경우는 LOSS_LOT 만 선택하도록 조건 처리
            if (DataTableConverter.GetValue(drv, "ROLLMAP_CLCT_TYPE").GetString() == "171" || DataTableConverter.GetValue(drv, "ROLLMAP_CLCT_TYPE").GetString() == "172")
            {
                if (Convert.ToString(e.Column.Name) == "BACK_RESNCODE")
                {
                    if (cbo != null)
                    {
                        string selectedValue = drv[e.Column.Name].GetString();

                        if (CommonVerify.HasTableRow(_dtBackReason))
                        {
                            var query = _dtBackReason.AsEnumerable().Where(o => o.Field<string>("ACTID").Equals("LOSS_LOT")).ToList();
                            DataTable dtReason = query.Any() ? query.CopyToDataTable() : _dtBackReason.Clone();

                            cbo.ItemsSource = null;
                            cbo.DisplayMemberPath = "RESNNAME";
                            cbo.SelectedValuePath = "RESNCODE";
                            cbo.ItemsSource = dtReason.Copy().AsDataView();

                            cbo.SelectedValue = selectedValue;
                            if (cbo.SelectedIndex < 0)
                                cbo.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            dgLotInfo.EndEdit();
            dgLotInfo.EndEditRow(true);

            // 구간불량 데이터 저장
            if (!ValidationSave()) return;

            const string bizRuleName = "BR_PRD_REG_TAG_SECTION";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("TOP_RESNCODE", typeof(string));    //TOP 활동유형코드
            inRollmapTable.Columns.Add("BACK_RESNCODE", typeof(string));   //BACK 활동유형코드

            foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
            {
                //if (row.Type == DataGridRowType.Item && ((DataRowView)row.DataItem).Row.RowState == DataRowState.Added || ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "UPDATE_FLAG").GetString() == "Y")
                {
                    if (rowIndex == 0)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    newRollRow["ADJ_END_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();
                    newRollRow["STRT_PSTN"] = DataTableConverter.GetValue(row.DataItem, "STRT_PSTN").GetDecimal();
                    newRollRow["END_PSTN"] = DataTableConverter.GetValue(row.DataItem, "END_PSTN").GetDecimal();
                    newRollRow["METHOD"] = "U";
                    newRollRow["TOP_RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "TOP_RESNCODE").GetString();
                    newRollRow["BACK_RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "BACK_RESNCODE").GetString();
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            //string xml = inDataSet.GetXml();

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        SelectRollMapTagSection();
                        InitializeTagSectionControl();

                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    IsUpdated = true;
                    SelectRollMapTagSection();
                    InitializeTagSectionControl();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // 구간불량 데이터 삭제
            if (!ValidationDelete()) return;

            const string bizRuleName = "BR_PRD_REG_TAG_SECTION";
            int rowIndex = 0;

            DataSet inDataSet = new DataSet();
            DataTable inTable = inDataSet.Tables.Add("INDATA");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("USER", typeof(string));
            inTable.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

            DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
            inRollmapTable.Columns.Add("LOTID", typeof(string));
            inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
            inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
            inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inRollmapTable.Columns.Add("ACTID", typeof(string));
            inRollmapTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("STRT_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("END_PSTN", typeof(decimal));
            inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
            inRollmapTable.Columns.Add("TOP_RESNCODE", typeof(string));    //TOP 활동유형코드
            inRollmapTable.Columns.Add("BACK_RESNCODE", typeof(string));   //BACK 활동유형코드

            foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
            {
                if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" ||
                                                         Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                {
                    if (rowIndex == 0)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["EQPTID"] = _equipmentCode;
                        newRow["USER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                    rowIndex++;

                    DataRow newRollRow = inRollmapTable.NewRow();
                    newRollRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRollRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetString();
                    newRollRow["EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRollRow["ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRollRow["CLCT_SEQNO"] = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetInt();
                    newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                    newRollRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    newRollRow["ADJ_END_PSTN"] = DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();
                    newRollRow["STRT_PSTN"] = DataTableConverter.GetValue(row.DataItem, "STRT_PSTN").GetDecimal();
                    newRollRow["END_PSTN"] = DataTableConverter.GetValue(row.DataItem, "END_PSTN").GetDecimal();
                    newRollRow["METHOD"] = "D";
                    newRollRow["TOP_RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "TOP_RESNCODE").GetString();
                    newRollRow["BACK_RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "BACK_RESNCODE").GetString();
                    inRollmapTable.Rows.Add(newRollRow);
                }
            }

            ShowLoadingIndicator();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.MessageInfo("SFU1273"); // 삭제되었습니다.

                    IsUpdated = true;
                    SelectRollMapTagSection();
                    InitializeTagSectionControl();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            }, inDataSet);

        }

        private void dgOutsideScrapChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb?.DataContext == null) return;

            if (rb.IsChecked != null)
            {
                DataRowView drv = rb.DataContext as DataRowView;
                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgLotInfo.SelectedIndex = idx;
                }
            }
        }

        private void dgOutsideScrap_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            string[] exceptionColumn = { "ROLLMAP_CLCT_TYPE", "WND_LEN" };

            if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
            {
                e.Cancel = e.Column.Name != "CHK" && drv["CHK"].GetString() != "1";
            }
            else
            {
                if (e.Column.Name == "CHK")
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = drv != null && drv["CHK"].GetString() != "1";
                }
            }
        }

        private void dgOutsideScrap_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = e.Cell.Row.DataItem as DataRowView;
                if (drv == null) return;

                // 시작위치 종료위치 확인
                if (e.Cell.Column.Name == "WND_LEN")
                {
                    //BizActor 호출 Top, Back 양품량 계산
                    const string bizRuleName = "DA_PRD_SEL_ROLLMAP_PROD_QTY";

                    DataTable inTable = new DataTable();
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("WIPSEQ", typeof(decimal));
                    inTable.Columns.Add("CLCT_SEQNO", typeof(int));
                    inTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
                    inTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));

                    DataRow newRow = inTable.NewRow();

                    newRow["EQPTID"] = _equipmentCode;
                    newRow["LOTID"] = DataTableConverter.GetValue(drv, "LOTID").GetString();
                    newRow["WIPSEQ"] = DataTableConverter.GetValue(drv, "WIPSEQ").GetDecimal();
                    newRow["CLCT_SEQNO"] = DataTableConverter.GetValue(drv, "CLCT_SEQNO").GetInt();
                    //newRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(drv, "ADJ_STRT_PSTN").GetDecimal();
                    newRow["ADJ_STRT_PSTN"] = DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDecimal() - DataTableConverter.GetValue(drv, "WND_LEN").GetDecimal();
                    newRow["ADJ_END_PSTN"] = DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDecimal();
                    inTable.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                    if (CommonVerify.HasTableRow(dtResult))
                    {
                        DataTableConverter.SetValue(drv, "TOP_PROD_QTY", dtResult.Rows[0]["TOP_PROD_QTY"].GetDecimal());
                        DataTableConverter.SetValue(drv, "BACK_PROD_QTY", dtResult.Rows[0]["BACK_PROD_QTY"].GetDecimal());
                        DataTableConverter.SetValue(drv, "FOIL_QTY", dtResult.Rows[0]["FOIL_QTY"].GetDecimal());
                    }

                    DataTableConverter.SetValue(drv, "ADJ_STRT_PSTN", DataTableConverter.GetValue(drv, "ADJ_END_PSTN").GetDecimal() - DataTableConverter.GetValue(drv, "WND_LEN").GetDecimal());
                }

                dgOutsideScrap.EndEdit();
                dgOutsideScrap.EndEditRow(true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutsideScrap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgOutsideScrap.CurrentRow == null || dgOutsideScrap.SelectedIndex == -1 || dgOutsideScrap.CurrentColumn == null)
            {
                return;
            }

            int iRow = dgOutsideScrap.CurrentRow.Index;
            dgOutsideScrap.SelectedIndex = iRow;

            dgOutsideScrap.GetCell(iRow, dgOutsideScrap.Columns["CHK"].Index);
            if (dgOutsideScrap.GetCell(iRow, dgOutsideScrap.Columns["CHK"].Index).Presenter == null) return;

            RadioButton rb = dgOutsideScrap.GetCell(iRow, dgOutsideScrap.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;

            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "CHK", true);

        }

        private void btnSaveOutsideScrap_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveOutsideScrap()) return;

            const string bizRuleName = "BR_PRD_REG_DATACOLLECT_DEFECT_CT";

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
            dtInLot.Columns.Add("USER_EQPT_MEASR_PSTN_ID", typeof(string));
            dtInLot.Columns.Add("USER_SCRAP_LEN", typeof(decimal));
            dtInLot.Columns.Add("USER_ROLLMAP_CLCT_TYPE", typeof(string));
            dtInLot.Columns.Add("TAG_SECTION_YN", typeof(string));
            dtInLot.Columns.Add("FORCE_FLAG", typeof(string));
            dtInLot.Columns.Add("BACK_RESNCODE", typeof(string));

            foreach (C1.WPF.DataGrid.DataGridRow row in dgOutsideScrap.Rows)
            {
                if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" ||
                                                         Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                {
                    DataRow newRow = dtInLot.NewRow();

                    newRow["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    newRow["WIPSEQ"] = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetDecimal();
                    newRow["USER_EQPT_MEASR_PSTN_ID"] = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    newRow["USER_SCRAP_LEN"] = DataTableConverter.GetValue(row.DataItem, "WND_LEN").GetDecimal();
                    newRow["USER_ROLLMAP_CLCT_TYPE"] = DataTableConverter.GetValue(row.DataItem, "ROLLMAP_CLCT_TYPE").GetString();
                    newRow["BACK_RESNCODE"] = DataTableConverter.GetValue(row.DataItem, "BACK_RESNCODE").GetString();
                    dtInLot.Rows.Add(newRow);
                }
            }

            //string xml = inDataSet.GetXml();

            loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                try
                {
                    if (bizException != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(bizException);
                        return;
                    }
                    //정상 처리 되었습니다.
                    Util.MessageInfo("SFU1275");
                    IsUpdated = true;
                    GetRollMapOutsideScrapList();

                }
                catch (Exception ex)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    IsUpdated = false;
                    Util.MessageException(ex);
                }
            }, inDataSet);

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
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;
        }

        private void InitializeControl()
        {
            txtLot.Text = _runLotId;
            chartCoater.View.AxisX.ScrollBar = new AxisScrollBar();

            _isEquipmentTotalDry = IsEquipmentTotalDry(_equipmentCode);

            _dtDefect = new DataTable();
            _dtDefect.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefect.Columns.Add("ABBR_NAME", typeof(string));
            _dtDefect.Columns.Add("COLORMAP", typeof(string));

            _dtLane = new DataTable();
            _dtLane.Columns.Add("LANE_NO", typeof(string));

            SetLegend();

            //nathan 2024.01.17 롤맵용 테스트 컷 적용 - start
            _bUseSampleCut = IsCommoncodeUse("TEST_CUT_HIST_AREA", LoginInfo.CFG_AREA_ID);

            if (_bUseSampleCut == false)    //Sample Cut 사용하지 않는 공장을 위한 임시 테스트컷
            {
                SelectLossCombo(cboTCTopLoss_NonArea, "DEFECT_TOP");
                SelectLossCombo(cboTCBackLoss_NonArea, "DEFECT_BACK");
                _isTestCutVisible = false;  //Sample Cut의 테스트컷 탭은 대체탭 사용으로 보여주지 않음
            }
            //nathan 2024.01.17 롤맵용 테스트 컷 적용 - end

            if (!_isTestCutVisible)
            {
                tiTestcut.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetControlTestCut();
                SelectTestCut(dgTestCut);
            }

            btnDefect.Visibility = Visibility.Collapsed;    //nathan 2023.12.13 롤맵 코터 수불 실적 적용
            _isShowBtnDeft = IsCommoncodeUse("ROLLMAP_DEF_COMP_CT", LoginInfo.CFG_AREA_ID); //nathan 2023.12.13 롤맵 코터 수불 실적 적용

            if (_isSearchMode)
            {
                btnDefect.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Collapsed;
                btnSaveOutsideScrap.Visibility = Visibility.Collapsed;
                btnSaveTestCut.Visibility = Visibility.Collapsed;
                btnRollMapOutsideScrap.Visibility = Visibility.Collapsed;
                btnRollMapPositionEdit.Visibility = Visibility.Collapsed;
            }
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

        private void InitializeCombo()
        {
            SetMeasurementCombo();
            InitializeDataGridCombo();
        }

        private void InitializeCoaterChart()
        {
            chartCoater.View.AxisX.Min = 0;
            chartCoater.View.AxisY.Min = -80;   //Back 하단의 태그 표시로 인하여 AxisY Min 값을 설정 함.
            chartCoater.View.AxisX.Max = _xMaxLength.Equals(0) ? 10 : _xMaxLength;
            chartCoater.View.AxisY.Max = 220 + 3;   //무지부 3

            InitializePointChart(_xMaxLength.Equals(0) ? 10 : _xMaxLength);

            InitializeChartView(chartCoater);
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

        private void InitializePointChart(double xLength)
        {
            DataTable dt = SelectRollMapGplmWidth();
            if (CommonVerify.HasTableRow(dt))
            {
                var convertFromString = ColorConverter.ConvertFromString("#D5D5D5");
                string[] typeArray = { "Top", "Back" };

                for (int i = 0; i < typeArray.Length; i++)
                {
                    string typeCode = typeArray[i].GetString();
                    int yposition = 0;

                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        yposition = typeCode == "Top" ? 120 : 0;

                        if (dt.Rows[j]["COATING_PATTERN"].GetString() == "Plain")
                        {
                            AlarmZone alarmZone = new AlarmZone
                            {
                                Near = 0,
                                Far = xLength,
                                ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                                LowerExtent = yposition + dt.Rows[j]["Y_STRT_PSTN"].GetDouble(),
                                UpperExtent = yposition + dt.Rows[j]["Y_END_PSTN"].GetDouble(),
                                ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                            };
                            chartCoater.Data.Children.Insert(0, alarmZone);
                        }
                    }

                }
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

            btnRefresh.IsEnabled = true;
            btnZoomIn.IsEnabled = true;
            btnZoomOut.IsEnabled = true;

            tbTopupper.Text = string.Empty;
            tbToplower.Text = string.Empty;
            tbBackupper.Text = string.Empty;
            tbBacklower.Text = string.Empty;

            _isRollMapLot = SelectRollMapLot();
            //nathan 2023.12.13 롤맵 코터 수불 실적 적용
            //btnDefect.Visibility = _isRollMapLot ? Visibility.Collapsed : Visibility.Visible;
            if (_isShowBtnDeft == true) btnDefect.Visibility = Visibility.Visible;
            else btnDefect.Visibility = Visibility.Collapsed;

            //nathan 2024.01.17 롤맵용 테스트 컷 적용 - start
            if (_bUseSampleCut == false)   //TEST CUT 사용하지 않는 AREA
            {
                btnRollMapTestCut.Visibility = Visibility.Visible;
                tiTestcut_NonArea.Visibility = Visibility.Visible;
                btnSaveTestCut_NonArea.IsEnabled = false;
            }
            else
            {
                btnRollMapTestCut.Visibility = Visibility.Collapsed;
                tiTestcut_NonArea.Visibility = Visibility.Collapsed;
            }

            //nathan 2024.01.17 롤맵용 테스트 컷 적용 - end

            //// TOP, BACK 콤보박스에 항목 추가  (2024.03.13 김지호)
            //if (cboMeasurementTop.Items.Count == 0 && cboMeasurementBack.Items.Count == 0)
            //{
            //    cboMeasurementTop.Items.Add("Wet(로딩량)"); // SelectedIndex = 1, mPoint_top = 1, mPstn_top = 1
            //    cboMeasurementTop.Items.Add("Dry(로딩량)"); // SelectedIndex = 2, mPoint_top = 2, mPstn_top = 1
            //    cboMeasurementTop.Items.Add("Wet(두께)"); // SelectedIndex = 3, mPoint_top = 1, mPstn_top = 2
            //    cboMeasurementTop.Items.Add("Dry(두께)"); // SelectedIndex = 4, mPoint_top = 2, mPstn_top = 2
            //    //cboMeasurementTop.Items.Add("Total Dry_TOP");   // SelectedIndex = 5, mPoint_top = 3, mPstn_top = 2

            //    cboMeasurementBack.Items.Add("Wet(로딩량)"); // SelectedIndex = 1, mPoint_back = 1, mPstn_back = 1
            //    cboMeasurementBack.Items.Add("Dry(로딩량)"); // SelectedIndex = 2, mPoint_back = 2, mPstn_back = 1
            //    cboMeasurementBack.Items.Add("Wet(두께)"); // SelectedIndex = 3, mPoint_back = 1, mPstn_back = 2
            //    cboMeasurementBack.Items.Add("Dry(두께)"); // SelectedIndex = 4, mPoint_back = 2, mPstn_back = 2

            //    if (sTDFlag == '1'.ToString())
            //        cboMeasurementBack.Items.Add("Total Dry");   // SelectedIndex = 5, mPoint_back = 3, mPstn_back = 2               

            //    if (!GetCboUserConf())
            //        SetRollMapDefaultGauge();
            //}

            BeginUpdateChart();

        }

        private void InitializeDataTable()
        {
            _dtPoint?.Clear();
            _dtGraph?.Clear();
            _dtDefect?.Clear();
            _dtLaneInfo?.Clear();
            _dtLane?.Clear();
            _dtTagSection?.Clear();
            _dtTagSpot?.Clear();
            Util.gridClear(dgLotInfo);
        }

        private void InitializeTagSectionControl()
        {
            SetScale(1.0);

            _selectedStartSectionText = string.Empty;
            _selectedStartSectionPosition = string.Empty;
            _selectedEndSectionPosition = string.Empty;
            _selectedCollectType = string.Empty;
            _selectedStartSampleFlag = string.Empty;
            _selectedEndSampleFlag = string.Empty;

            _isSelectedTagSection = false;
            _selectedStartSection = 0;
            _selectedEndSection = 0;
            _selectedchartPosition = 0;

            if (!(bool)btnZoomOut.IsEnabled) btnZoomOut.IsEnabled = true;
            if (!(bool)btnZoomIn.IsEnabled) btnZoomIn.IsEnabled = true;
            if (!(bool)btnRefresh.IsEnabled) btnRefresh.IsEnabled = true;
        }

        private void InitializeDataGridCombo()
        {
            SetDataGridResonCombo(dgLotInfo.Columns["TOP_RESNCODE"], CommonCombo.ComboStatus.NONE, "DEFECT_TOP");
            //SetDataGridResonCombo(dgLotInfo.Columns["BACK_RESNCODE"], CommonCombo.ComboStatus.NONE, "DEFECT_BACK");
            SetDataGridBackResonCombo(dgLotInfo.Columns["BACK_RESNCODE"]);
            SetDataGridResonCombo(dgOutsideScrap.Columns["BACK_RESNCODE"], CommonCombo.ComboStatus.NONE, "DEFECT_BACK");
        }

        //nathan 2023.12.13 롤맵 코터 수불 실적 적용

        private bool IsCommoncodeUse(string sCodeType, string sCmCode)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = sCmCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
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

        private void SetMeasurementCombo()
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
            DataTable dtBinding = dtResult.Select().AsEnumerable().OrderBy(o => o.Field<decimal>("CMCDSEQ")).CopyToDataTable();

            if (_isEquipmentTotalDry)
            {
                cboMeasurementBack.ItemsSource = dtBinding.Copy().AsDataView();

                dtBinding.AsEnumerable().Where(r => r.Field<string>("CMCODE") == "TD").ToList().ForEach(row => row.Delete());
                dtBinding.AcceptChanges();
                cboMeasurementTop.ItemsSource = dtBinding.Copy().AsDataView();
                cboMeasurementTop.SelectedIndex = 0;
                cboMeasurementBack.SelectedIndex = 0;
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

            GetUserConfigurationControl();
        }

        private void SetLegend()
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
        }

        private void GetLegend()
        {
            grdLegendTop.Children.Clear();
            grdLegendBack.Children.Clear();
            DataTable dt = _dtColorMapLegend.Copy();

            if ((bool)rdoRelativeCoordinates?.IsChecked)
            {
                dt.AsEnumerable().Where(r => r.Field<string>("VALUE") == ObjectDic.Instance.GetObjectName("QA샘플") || r.Field<string>("VALUE") == ObjectDic.Instance.GetObjectName("자주검사") || r.Field<string>("VALUE") == ObjectDic.Instance.GetObjectName("최외각 폐기")).ToList().ForEach(row => row.Delete());
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


                if ((bool)rdoAbsolutecoordinates?.IsChecked)
                {
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

                    grdLegendBack.Visibility = Visibility.Visible;
                    grdLegendBack.Children.Add(stackPanel);
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

        private void GetErpClose()
        {
            DataTable dt = new DataTable("RQSTDT");
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROLLMAP_ERP_CLOSE", "RQSTDT", "RSLTDT", dt);

            if (CommonVerify.HasTableRow(result))
            {
                if (result.Rows[0]["ERP_CLOSING_FLAG"].GetString() == "CLOSE")
                {
                    // ERP 생산실적이 마감되었습니다. 불량/Loss 등록없이 Defect Section 추가 가능합니다.
                    Util.MessageValidation("SFU10005");
                    //_isSearchMode = true;
                    btnSaveTestCut.IsEnabled = false;
                    btnSaveTestCut_NonArea.IsEnabled = false;
                }
                else
                {
                    btnSaveTestCut.IsEnabled = true;
                    btnSaveTestCut_NonArea.IsEnabled = true;
                }
            }
            else
            {
                btnSaveTestCut.IsEnabled = true;
                btnSaveTestCut_NonArea.IsEnabled = true;
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

        private static DataTable SelectRollMapLength(string lotId, string wipSeq, string adjFlag, string sampleFlag, string eqptId)
        {
            try
            {
                const string bizRuleName = "BR_PRD_SEL_ROLLMAP_LENGTH_CT";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("ADJFLAG", typeof(string));
                inTable.Columns.Add("SMPL_FLAG", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = lotId;
                dr["WIPSEQ"] = wipSeq;
                dr["ADJFLAG"] = adjFlag;
                dr["SMPL_FLAG"] = sampleFlag;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = eqptId;
                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;

            }
        }

        private static DataTable SelectRollMapMaterialInfo(string lotId, string wipSeq, string adjFlag, string sampleFlag)
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_MATERIAL_CT_CHART";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;
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

            if (dtLaneInfo.Columns.Contains("TAB"))
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
                dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderByDescending(o => o.Field<string>("LANE_NO_CUR").GetDecimal()).ThenByDescending(x => x.Field<int>("RN")).CopyToDataTable();
                var query = (from t in dtLaneInfo.AsEnumerable() where t.Field<string>("LANE_NO") != null select new { LaneNo = t.Field<string>("LANE_NO") }).Distinct().ToList();
                decimal sumLength = dtLaneInfo.AsEnumerable().Sum(s => s.Field<decimal>("LOV_VALUE"));
                double yLength = 0;

                // Lane 이 하나 인 경우
                if (query.Count < 2)
                {
                    for (int i = 0; i < dtLaneInfo.Rows.Count; i++)
                    {
                        // 기준 높이
                        double referenceheight = 100;
                        double x = (dtLaneInfo.Rows[i]["LOV_VALUE"].GetDouble() * referenceheight / sumLength.GetDouble()).GetDouble();
                        dtLaneInfo.Rows[i]["LANE_LENGTH"] = x;

                        dtLaneInfo.Rows[i]["Y_STRT_PSTN"] = referenceheight - (yLength + x);
                        dtLaneInfo.Rows[i]["Y_END_PSTN"] = referenceheight - yLength;
                        // 유지부 영역
                        if (dtLaneInfo.Rows[i]["COATING_PATTERN"].GetString() == "Coat")
                        {
                            dtLaneInfo.Rows[i]["Y_PSTN"] = referenceheight / 2;
                        }

                        yLength = yLength + x;
                        dtLaneInfo.Rows[i]["LANE_LENGTH_SUM"] = yLength;
                    }
                }
                // 다수 Lane 인 경우
                else
                {
                    for (int i = 0; i < dtLaneInfo.Rows.Count; i++)
                    {
                        double x = (dtLaneInfo.Rows[i]["LOV_VALUE"].GetDecimal() * 100 / sumLength).GetDouble();
                        dtLaneInfo.Rows[i]["LANE_LENGTH"] = x;

                        dtLaneInfo.Rows[i]["Y_STRT_PSTN"] = 100 - (yLength + x);
                        dtLaneInfo.Rows[i]["Y_END_PSTN"] = 100 - yLength;

                        if (dtLaneInfo.Rows[i]["COATING_PATTERN"].GetString() == "Coat")
                        {
                            dtLaneInfo.Rows[i]["Y_PSTN"] = (dtLaneInfo.Rows[i]["Y_STRT_PSTN"].GetDouble() + dtLaneInfo.Rows[i]["Y_END_PSTN"].GetDouble()) / 2 - 2;
                        }


                        yLength = yLength + x;
                        dtLaneInfo.Rows[i]["LANE_LENGTH_SUM"] = yLength;
                    }
                }

                _dtLaneInfo = dtLaneInfo;
                _dtLane?.Clear();

                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        DataRow newRow = _dtLane.NewRow();
                        newRow["LANE_NO"] = item.LaneNo;
                        _dtLane.Rows.Add(newRow);
                    }
                }
            }

            return dtLaneInfo;
        }

        private static DataTable SelectRollMapGraphInfo(string lotId, string wipSeq, string mPoint_top, string mPstn_top, string mPoint_back, string mPstn_back, string adjFlag, string equipmentCode, string sampleFlag)
        {
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CT_CHART";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("MPOINT_TOP", typeof(string));
            inTable.Columns.Add("MPSTN_TOP", typeof(string));
            inTable.Columns.Add("MPOINT_BACK", typeof(string));
            inTable.Columns.Add("MPSTN_BACK", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["MPOINT_TOP"] = mPoint_top;
            dr["MPSTN_TOP"] = mPstn_top;
            dr["MPOINT_BACK"] = mPoint_back;
            dr["MPSTN_BACK"] = mPstn_back;
            dr["ADJFLAG"] = adjFlag;
            dr["EQPTID"] = equipmentCode;
            dr["SMPL_FLAG"] = sampleFlag;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private static DataTable SelectRollMapPointInfo(string equipmentCode, string lotId, string wipSeq, string adjFlag, string sampleFlag)
        {
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CT_DEFECT_CHART";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));
            inTable.Columns.Add("SMPL_FLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = lotId;
            dr["WIPSEQ"] = wipSeq;
            dr["EQPTID"] = equipmentCode;
            dr["ADJFLAG"] = adjFlag;
            dr["SMPL_FLAG"] = sampleFlag;
            inTable.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
        }

        private void SelectRollMapTagSection()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartCoater.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TAG_SECTION") || dataSeries.Tag.GetString().Equals("TagSectionStart") || dataSeries.Tag.GetString().Equals("TagSectionEnd"))
                    {
                        chartCoater.Data.Children.Remove(dataSeries);
                    }
                }

                //전체 조회 시 데이터가 많은 경우 바인딩 하는 속도 문제로 TAG_SECTION 따로 처리
                string adjFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
                string sampleFlag = "Y";
                DataTable dtPoint = SelectRollMapPointInfo(_equipmentCode, txtLot.Text, _wipSeq, adjFlag, sampleFlag);

                // E20240724-000827 TAG_SECTION_SINGLE 추가
                var queryTagSection = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION") || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dtPoint.Clone();
                dtTagSection.TableName = "TAG_SECTION";
                _dtTagSection = dtTagSection;

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawRollMapTagSection(dtTagSection);
                }
                GetRollMapTagSectionList();
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectRollMapTagSpot()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
                {
                    DataSeries dataSeries = chartCoater.Data.Children[i];
                    if (dataSeries.Tag.GetString().Equals("TAG_SPOT"))
                    {
                        chartCoater.Data.Children.Remove(dataSeries);
                    }
                }

                //전체 조회 시 데이터가 많은 경우 바인딩 하는 속도 문제로 TAG_SPOT 따로 처리
                string adjFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
                string sampleFlag = "Y";
                DataTable dtPoint = SelectRollMapPointInfo(_equipmentCode, txtLot.Text, _wipSeq, adjFlag, sampleFlag);

                var queryTagSpot = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dtPoint.Clone();
                dtTagSpot.TableName = "TAG_SPOT";
                _dtTagSpot = dtTagSpot;

                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    DrawRollMapTagSpot(dtTagSpot);
                }
                //GetRollMapTagSectionList();
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DrawMaterialChart(DataTable dt, double minLength, double maxLength)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                string[] cathodeMeasurementPosition = { "INS_SLURRY_TOP", "SLURRY_TOP_L2", "SLURRY_TOP_L1", "UW", "SLURRY_BACK_L1", "SLURRY_BACK_L2", "INS_SLURRY_BACK" };
                string[] anodeMeasurementPosition = { "SLURRY_TOP_L2", "SLURRY_TOP_L1", "UW", "SLURRY_BACK_L1", "SLURRY_BACK_L2" };

                // Material 에서 필요한 측정위치 외의 정보 삭제
                if (_polarityCode == "C")
                {
                    dt.AsEnumerable().Where(r => !cathodeMeasurementPosition.Contains(r.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList().ForEach(row => row.Delete());
                }
                else
                {
                    dt.AsEnumerable().Where(r => !anodeMeasurementPosition.Contains(r.Field<string>("EQPT_MEASR_PSTN_ID"))).ToList().ForEach(row => row.Delete());
                }
                dt.AcceptChanges();

                InitializeMaterialChart(minLength, maxLength);

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
                                                 StartPosition = t.Field<decimal?>("SOURCE_STRT_PSTN"),
                                                 EndPosition = t.Field<decimal?>("SOURCE_END_PSTN"),
                                                 InputLotId = t.Field<string>("INPUT_LOTID"),
                                                 // [E20240115-000246] 코터 롤맵 Material CHART의 슬러리에 믹서 버퍼의 이전/이후 배치 ID를 툴팁으로 표기. (믹서-코터 배치연계 고도화) 2024.01.30 JEONG KI TONG
                                                 PreInputLotId = t.Field<string>("PRE_INPUT_LOTID"),
                                                 NextInputLotId = t.Field<string>("NEXT_INPUT_LOTID"),
                                                 ColorMap = t.Field<string>("COLORMAP"),
                                                 Side = t.Field<string>("SIDE"),
                                                 AdjLotId = t.Field<string>("ADJ_LOTID"),
                                                 MeasurementPosition = t.Field<string>("EQPT_MEASR_PSTN_ID"),
                                                 MaterialDescription = t.Field<string>("MATERIAL_DESC"),
                                                 AdjStartPosition = t.Field<decimal?>("ADJ_STRT_PSTN"),
                                                 AdjEndPosition = t.Field<decimal?>("ADJ_END_PSTN"),
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

                                            string content = ObjectDic.Instance.GetObjectName("길이") + "  : " + $"{length:#,##0.#}" + "m (" + Util.NVC($"{row.AdjStartPosition:###,###,###,##0.##}") + " ~ " + Util.NVC($"{row.AdjEndPosition:###,###,###,##0.##}") + ")";
                                            // [E20240115-000246] 코터 롤맵 Material CHART의 슬러리에 믹서 버퍼의 이전/이후 배치 ID를 툴팁으로 표기. (믹서-코터 배치연계 고도화) 2024.01.30 JEONG KI TONG
                                            content = content + System.Environment.NewLine;
                                            content = content + ObjectDic.Instance.GetObjectName("SLURRY") + "  : " + string.Format("Current : {0} / Pre : {1} / Next : {2}", row.InputLotId, row.PreInputLotId, row.NextInputLotId);

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

                    var uw = dt.AsEnumerable().Where(r =>
                    {
                        string pstn = string.Format("{0}", r["EQPT_MEASR_PSTN_ID"]);

                        if (pstn.Equals("UW"))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });

                    if (uw.Any())
                    {
                        DrawUWInputLotLine(uw.CopyToDataTable());
                    }
                }
            }
        }

        private void DrawUWInputLotLine(DataTable dt)
        {
            var rows = dt.AsEnumerable().Where(r =>
            {
                string inLot = Util.NVC(r["INPUT_LOTID"]);

                if (string.IsNullOrEmpty(inLot))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            });
            int cnt = rows.Select(r => new { INPUT_LOTID = Util.NVC(r["INPUT_LOTID"]) }).Distinct().Count();

            if (rows.Any() && cnt > 1)
            {
                #region INPUT_LOTID 데이터 있으면서 하나 이상이고, 중복건 제거하고, SOURCE_END_PSTN 순으로 표시
                DataRow[] drInLots = rows.CopyToDataTable()
                .DefaultView.ToTable(true, "INPUT_LOTID", "SOURCE_END_PSTN").AsEnumerable()
                .OrderBy(r => Util.NVC_Decimal(r["SOURCE_END_PSTN"]))
                .ToArray();

                if (drInLots != null && drInLots.Length > 1)
                {
                    C1Chart c1Chart;

                    if (_polarityCode == "C")
                    {
                        c1Chart = chartMaterialCathode4;
                    }
                    else
                    {
                        c1Chart = chartMaterialAnode3;
                    }

                    for (int i = 0; i < drInLots.Length - 1; i++)
                    {
                        double srcEndPtsn;
                        double.TryParse(Util.NVC(drInLots[i]["SOURCE_END_PSTN"]), out srcEndPtsn);

                        string content = string.Empty;
                        string inputLot = Util.NVC(drInLots[i]["INPUT_LOTID"]);
                        string nextLot = Util.NVC(drInLots[i + 1]["INPUT_LOTID"]);

                        #region 연속적으로 같은 INPUT_LOTID가 있을 경우, 다음 정보로 표시할 수 있도록 Pass !!
                        if (inputLot.Equals(nextLot))
                        {
                            continue;
                        }
                        #endregion

                        if (string.IsNullOrEmpty(inputLot))
                        {
                            content = "Change Lot " + " (" + $"{srcEndPtsn:###,###,###,##0.##}" + "m" + ")";
                        }
                        else
                        {
                            content = "Change Lot [" + Util.NVC(drInLots[i]["INPUT_LOTID"]) + "]" + " (" + $"{srcEndPtsn:###,###,###,##0.##}" + "m" + ")";
                        }

                        XYDataSeries srDot = new XYDataSeries()
                        {
                            ChartType = ChartType.Line,
                            XValuesSource = new[] { drInLots[i]["SOURCE_END_PSTN"].GetDouble(), drInLots[i]["SOURCE_END_PSTN"].GetDouble() },
                            ValuesSource = new double[] { -1.5, 111.5 }, //White(20)
                            Cursor = Cursors.Hand,
                            ConnectionStroke = new SolidColorBrush(Colors.Red),
                            ConnectionStrokeThickness = 3d
                            //ConnectionStrokeDashes  = new DoubleCollection { 3, 2 } //Dash 표현시 상단 끝부분이 채워지지 않는 경우 발생(주석처리)
                            //ToolTip                 = content
                        };

                        srDot.PlotElementLoaded += (s, e) =>
                        {
                            PlotElement pe = (PlotElement)s;

                            if (pe is Lines)
                            {
                                ToolTipService.SetToolTip(pe, content);
                                ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                                ToolTipService.SetShowDuration(pe, 60000);
                            }
                        };

                        c1Chart.Data.Children.Add(srDot);
                    }
                }
                #endregion
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

                // E20240724-000827 TAG_SECTION_SINGLE 추가
                var queryTagSection = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION") || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSection = queryTagSection.Any() ? queryTagSection.CopyToDataTable() : dtPoint.Clone();
                dtTagSection.TableName = "TAG_SECTION";
                _dtTagSection = dtTagSection;

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawRollMapTagSection(dtTagSection);
                }

                var queryTagSpot = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dtPoint.Clone();
                dtTagSpot.TableName = "TAG_SPOT";
                _dtTagSpot = dtTagSpot;

                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    DrawRollMapTagSpot(dtTagSpot);
                }

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
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")).ToList();
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
            //ELLIPSE
            if (!CommonVerify.HasTableRow(dt)) return;

            XYDataSeries ds = new XYDataSeries();
            ds.Label = "points";
            ds.ItemsSource = DataTableConverter.Convert(dt);
            ds.ValueBinding = new Binding("Y_PSTN");
            //ds.XValueBinding = new Binding("SOURCE_X_PSTN");  //Sample Cut 이후 ToolTip 변경으로 인한 SOURCE_X_PSTN -> ADJ_X_PSTN 변경(2021-09-01)
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
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_X_PSTN")), out xPosition);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_Y_PSTN")), out yPosition);
                    string defectName = DataTableConverter.GetValue(dp.DataObject, "ABBR_NAME").GetString();

                    //string content = defectName + " [X:" + $"{xPosition:###,###,###,##0.##}" + "m" + ", Y:" + $"{yPosition:###,###,###,##0.##}" + "mm" + "]";
                    string content = defectName + " [" + ObjectDic.Instance.GetObjectName("길이") + ":" + $"{xPosition:###,###,###,##0.##}" + "m" + ", " + ObjectDic.Instance.GetObjectName("폭") + ":" + $"{yPosition:###,###,###,##0.##}" + "mm" + "]";
                    ToolTipService.SetToolTip(pe, content);
                    ToolTipService.SetPlacement(pe, PlacementMode.MousePoint);
                    ToolTipService.SetShowDuration(pe, 60000);
                }
            };
            chartCoater.Data.Children.Add(ds);
        }

        private void DrawRollMapVision(DataTable dt)
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

                chartCoater.Data.Children.Add(alarmZone);
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

                    chartCoater.Data.Children.Add(new XYDataSeries()
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
            //dsMarkLabel.XValueBinding = new Binding("SOURCE_STRT_PSTN");
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
            chartCoater.Data.Children.Add(dsMarkLabel);
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
                    //ds.XValueBinding = x == 0 ? new Binding("SOURCE_STRT_PSTN") : new Binding("SOURCE_END_PSTN");
                    ds.XValueBinding = x == 0 ? new Binding("ADJ_STRT_PSTN") : new Binding("ADJ_END_PSTN");
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
                        // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.

                    };
                    chartCoater.Data.Children.Add(ds);
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
                //ds.XValueBinding = new Binding("SOURCE_STRT_PSTN");
                ds.XValueBinding = new Binding("ADJ_STRT_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartSpotTag"] as DataTemplate;
                ds.Margin = new Thickness(0);
                ds.Tag = dtTagSpot.TableName;

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                chartCoater.Data.Children.Add(ds);
            }
        }

        private void DrawRollMapSampleYAxisLine()
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_CT_HEAD";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(decimal));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dr["ADJFLAG"] = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
            inTable.Rows.Add(dr);

            DataTable dtSample = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            string[] petScrap = new string[] { "PET", "SCRAP" };
            DataRow[] drSample = dtSample.Select("SMPL_FLAG = 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");
            //DataRow[] drSample = new DataRow[] { };

            //if ((bool)rdoRelativeCoordinates.IsChecked)
            //    drSample = dtSample.Select("SMPL_FLAG <> 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");
            //else
            //    drSample = dtSample.Select("SMPL_FLAG = 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");

            if (drSample.Length > 0)
            {
                foreach (DataRow row in drSample)
                {
                    chartCoater.Data.Children.Add(new XYDataSeries()
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

            var query = (from t in dtSample.AsEnumerable() where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
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
                newRow["SOURCE_Y_PSTN"] = 220;
                newRow["TOOLTIP"] = string.Empty;
                newRow["FONTSIZE"] = 11;
                dtMaxLength.Rows.Add(newRow);

                var queryLength = (from t in dtSample.AsEnumerable()
                                   where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID"))
                                   select new
                                   {
                                       RawEndPosition = t.Field<decimal?>("RAW_END_PSTN"),
                                       RawStartPosition = t.Field<decimal?>("RAW_STRT_PSTN"),
                                       EndPosition = t.Field<decimal?>("SOURCE_END_PSTN"),
                                       RowNum = t.Field<Int32>("ROW_NUM"),
                                       MeasurementCode = t.Field<string>("EQPT_MEASR_PSTN_ID")
                                   }).ToList();

                int rowIndex = 0;

                if (queryLength.Any())
                {
                    foreach (var item in queryLength)
                    {
                        DataRow newLength = dtMaxLength.NewRow();

                        newLength["RAW_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";

                        //if ((bool)rdoRelativeCoordinates.IsChecked)
                        //    newLength["SOURCE_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";
                        //else
                        newLength["SOURCE_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";

                        newLength["SOURCE_Y_PSTN"] = 220;
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

            if (CommonVerify.HasTableRow(dtSample))
            {
                _sampleCutLength = dtSample.AsEnumerable().Where(x => x.Field<string>("SMPL_FLAG") == "Y").ToList().Sum(r => r["RAW_END_PSTN"].GetDouble()).GetDouble();
            }
            else
            {
                _sampleCutLength = 0;
            }

        }

        private void DrawChartBackGround(DataTable dt, double xLength)
        {
            try
            {
                if (CommonVerify.HasTableRow(dt))
                {
                    _xMaxLength = dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble() > xLength
                                ? dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble()
                                : xLength;

                    chartCoater.View.AxisX.Min = 0;
                    chartCoater.View.AxisY.Min = -80;   //Back 하단의 태그 표시로 인하여 AxisY Min 값을 설정 함.
                    chartCoater.View.AxisX.Max = _xMaxLength;
                    chartCoater.View.AxisY.Max = 220 + 3;   //무지부 3

                    // 라인 그리기                   
                    InitializePointChart(_xMaxLength);

                    // Data 그리기
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
                                         YEndPosition = t.Field<decimal>("Y_END_PSTN"),
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

                        XYDataSeries xyDataSeries = new XYDataSeries();

                        xyDataSeries.ChartType = ChartType.PolygonFilled;
                        xyDataSeries.XValues = new DoubleCollection();
                        xyDataSeries.Values = new DoubleCollection();

                        double? near = dt.Rows[i]["ADJ_STRT_PSTN"].GetDouble();
                        double? far = dt.Rows[i]["ADJ_END_PSTN"].GetDouble();
                        double? lowerExtent;
                        double? upperExtent;


                        lowerExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yStartPosition : 0 + yStartPosition;
                        upperExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yEndPosition : yEndPosition;


                        if (near != null && far != null && lowerExtent != null && upperExtent != null)
                        {
                            xyDataSeries.XValues.Clear();
                            xyDataSeries.XValues.Add((double)near);
                            xyDataSeries.XValues.Add((double)far);
                            xyDataSeries.XValues.Add((double)far);
                            xyDataSeries.XValues.Add((double)near);

                            xyDataSeries.Values.Clear();
                            xyDataSeries.Values.Add((double)lowerExtent);
                            xyDataSeries.Values.Add((double)lowerExtent);
                            xyDataSeries.Values.Add((double)upperExtent);
                            xyDataSeries.Values.Add((double)upperExtent);

                            var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dt.Rows[i]["COLORMAP"]));

                            xyDataSeries.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                            xyDataSeries.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                            xyDataSeries.SymbolFill = new SolidColorBrush(Colors.Transparent);

                            chartCoater.Data.Children.Add(xyDataSeries);

                            int x = i;
                            xyDataSeries.PlotElementLoaded += (s, e) =>
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
                        }
                    }

                    foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdPoint))
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

        private void DrawRollMapLane()
        {
            //DataTable dtLaneInfo = SelectRollMapGPLMWidth();
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
                        if (dtLaneInfo.Rows[j]["COATING_PATTERN"].GetString() == "Coat")
                        {
                            DataRow drLane = dtLane.NewRow();
                            drLane["SOURCE_STRT_PSTN"] = _xMaxLength - (_xMaxLength * 0.01);
                            drLane["SOURCE_Y_PSTN"] = typeCode == "Top" ? 120 + dtLaneInfo.Rows[j]["Y_PSTN"].GetDouble() : dtLaneInfo.Rows[j]["Y_PSTN"].GetDouble();
                            drLane["LANEINFO"] = "Lane " + dtLaneInfo.Rows[j]["LANE_NO"];
                            dtLane.Rows.Add(drLane);
                        }
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
                dsLane.Margin = new Thickness(0);

                dsLane.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                    // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                };
                chartCoater.Data.Children.Add(dsLane);
            }
        }

        private void DrawRollMapLane_TD()
        {
            //DataTable dtLaneInfo = SelectRollMapGPLMWidth();
            DataTable dtLaneInfo = _dtLaneInfo;

            if (CommonVerify.HasTableRow(dtLaneInfo))
            {
                DataTable dtLane = new DataTable();
                dtLane.Columns.Add("SOURCE_STRT_PSTN", typeof(string));
                dtLane.Columns.Add("SOURCE_Y_PSTN", typeof(string));
                dtLane.Columns.Add("LANEINFO", typeof(string));

                for (int j = 0; j < dtLaneInfo.Rows.Count; j++)
                {
                    if (dtLaneInfo.Rows[j]["COATING_PATTERN"].GetString() == "Coat")
                    {
                        DataRow drLane = dtLane.NewRow();
                        drLane["SOURCE_STRT_PSTN"] = _xMaxLength - (_xMaxLength * 0.01);
                        drLane["SOURCE_Y_PSTN"] = dtLaneInfo.Rows[j]["Y_PSTN"].GetDouble();
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
                dsLane.Margin = new Thickness(0);

                dsLane.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                    // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                };
                chartCoater.Data.Children.Add(dsLane);
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
                    ColumnDefinition gridColumn = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) };
                    grid.ColumnDefinitions.Add(gridColumn);
                }

                grid.Children.Clear();

                //var queryRow = (from t in dt.AsEnumerable()
                //                select new
                //                {
                //                    DefectName = t.Field<string>("ABBR_NAME"),
                //                    ColorMap = t.Field<string>("COLORMAP"),
                //                    DefectShape = t.Field<string>("DEFECT_SHAPE"),
                //                    MeasurementCode = t.Field<string>("EQPT_MEASR_PSTN_ID")
                //                }).Distinct().ToList();

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
                        grid.RowDefinitions.Add(gridRow);

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
                                                                                 new Thickness(2, 2, 2, 2),  // new Thickness(5, 5, 5, 5),
                                                                                 null,
                                                                                 null);

                                TextBlock rectangleDescription = Util.CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                                      HorizontalAlignment.Center,
                                                                                      VerticalAlignment.Center,
                                                                                      11,
                                                                                      FontWeights.Bold,
                                                                                      Brushes.Black,
                                                                                      new Thickness(1, 1, 1, 1), // new Thickness(5, 5, 5, 5),
                                                                                      new Thickness(0),
                                                                                      item.MeasurementCode,
                                                                                      Cursors.Hand,
                                                                                      item.ColorMap);
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
                                                                           new Thickness(2, 2, 2, 2),  // new Thickness(5, 5, 5, 5),
                                                                           null,
                                                                           null);

                                TextBlock ellipseDescription = Util.CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                                     HorizontalAlignment.Center,
                                                                                     VerticalAlignment.Center,
                                                                                     11,
                                                                                     FontWeights.Bold,
                                                                                     Brushes.Black,
                                                                                     new Thickness(1, 1, 1, 1),  // new Thickness(5, 5, 5, 5),
                                                                                     new Thickness(0),
                                                                                     item.MeasurementCode,
                                                                                     Cursors.Hand,
                                                                                     item.ColorMap);
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

        private void DisplayTabLocation(DataTable dtTab)
        {
            if (CommonVerify.HasTableRow(dtTab))
            {
                bool isupper = false;
                bool islower = false;

                if (dtTab.AsEnumerable().Any(x => !string.IsNullOrEmpty(x.Field<string>("TAB")) && x.Field<string>("TAB") == "1" && x.Field<string>("COATING_PATTERN") == "Plain"))
                {
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
                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<Int32>("RN") < item["RN"].GetInt()))
                                {
                                    islower = true;
                                }

                                if (dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO_CUR") == item["LANE_NO_CUR"].GetString() && x.Field<string>("COATING_PATTERN") == "Coat" && x.Field<Int32>("RN") > item["RN"].GetInt()))
                                {
                                    isupper = true;
                                }
                            }

                            if (isupper)
                            {
                                tbTopupper.Text = "Tab";
                                tbBackupper.Text = "Tab";
                            }
                            if (islower)
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

        private void DescriptionOnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock?.Text == null) return;

                ShowLoadingIndicator();
                DoEvents();

                _isResearch = true;
                chartCoater.Data.Children.Clear();

                const string roundbrackets = "(";
                int lastIndex = textBlock.Text.LastIndexOf(roundbrackets);
                string textBlockText = textBlock.Text.Substring(0, lastIndex);
                string colorMap = textBlock.Tag.GetString();

                if (textBlock.Foreground == Brushes.Black)
                {
                    _dtDefect.Rows.Add(textBlock.Name, textBlockText, colorMap);
                    textBlock.Foreground = Brushes.LightGray;
                }

                else //if(textBlock.Foreground == Brushes.Gray)
                {
                    _dtDefect.Select("EQPT_MEASR_PSTN_ID = '" + textBlock.Name + "' AND "
                                     + "ABBR_NAME = '" + textBlockText + "' And COLORMAP = '" + colorMap + "'").ToList().ForEach(row => row.Delete());
                    _dtDefect.AcceptChanges();

                    textBlock.Foreground = Brushes.Black;
                }

                if (CommonVerify.HasTableRow(_dtGraph))
                {
                    DrawChartBackGround(_dtGraph.Copy(), _xMaxLength);
                }
                else
                {
                    InitializeCoaterChart();
                }

                if (_dtDefect.Rows.Count < 1)
                {
                    DrawPointChart(_dtPoint.Copy());
                }
                else
                {
                    var queryPoint = _dtPoint.AsEnumerable().Where(ra => !_dtDefect.AsEnumerable()
                        .Any(rb => rb.Field<string>("EQPT_MEASR_PSTN_ID") == ra.Field<string>("EQPT_MEASR_PSTN_ID") && rb.Field<string>("ABBR_NAME") == ra.Field<string>("ABBR_NAME") && rb.Field<string>("COLORMAP") == ra.Field<string>("COLORMAP")));

                    if (queryPoint.Any())
                    {
                        DrawPointChart(queryPoint.CopyToDataTable());
                    }
                }
                DrawRollMapSampleYAxisLine();

                DrawRollMapLane();

                chartCoater.View.AxisX.Reversed = true;

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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
                    c1Chart.View.Margin = new Thickness(0, 0, 0, 10);
                    //c1Chart.View.Margin = new Thickness(0);
                    c1Chart.Padding = new Thickness(10, 3, 3, 5);
                }
            }
        }

        private void EndUpdateChart()
        {
            foreach (C1Chart c1Chart in Util.FindVisualChildren<C1Chart>(grdMain))
            {
                if (string.Equals(_processCode, Class.Process.COATING) && c1Chart.Name != "chart")
                    c1Chart.View.AxisX.Reversed = true;

                c1Chart.EndUpdate();
            }
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
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "BORDERTAG", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    // E20240724-000827 TAG_SECTION_SINGLE 위치 조정
                    if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"]))
                        row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? 220 : -62;
                    else
                        row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;

                    row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();
                    row["BORDERTAG"] = row["LOTID"].GetString() + ";" + row["WIPSEQ"].GetString() + ";" + row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["TAG_AUTO_FLAG"].GetString();
                    
                    // E20240724-000827 TAG_SECTION_SINGLE Tooltip 설정
                    if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"]))
                    {
                        if (chartDisplayType == ChartDisplayType.TagStart)
                        {
                            row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                            row["TAGNAME"] = "M";
                        }
                        else
                        {
                            row.Delete();
                        }

                    }
                    else
                    {
                        //row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TOOLTIP"] = row["CMCDNAME"].GetString() + " : " + row["DFCT_NAME"].GetString() + "\n" + "[" + $"{Convert.ToDouble(row["SOURCE_END_PSTN"]) - Convert.ToDouble(row["SOURCE_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                    }

                }
            }
            else if (chartDisplayType == ChartDisplayType.TagToolTip)
            {
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

                int maxYvalue = chartDisplayType == ChartDisplayType.TagVisionBack ? 100 : 220;

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

                    row["SOURCE_Y_PSTN"] = -80;
                    row["TAGNAME"] = "T";
                    row["TOOLTIP"] = row["CMCDNAME"] + "[" + sourceStartPosition + "m]";
                    //TAG 값 추가 IGHKDS77
                    row["TAG"] = row["LOTID"].GetString() + ";" + row["WIPSEQ"].GetString() + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["EQPT_MEASR_PSTN_ID"].GetString() + ";" + row["ADJ_STRT_PSTN"].GetString();
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

        private void GetRollMapTagSectionList()
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_CT_TAG_SECTION";

            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(decimal));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQPTID"] = _equipmentCode;
            Indata["LOTID"] = txtLot.Text;
            Indata["WIPSEQ"] = _wipSeq;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", IndataTable);

            dtResult.Columns.Add("OLD_ADJ_STRT_PSTN", typeof(decimal));
            dtResult.Columns.Add("OLD_ADJ_END_PSTN", typeof(decimal));
            dtResult.Columns.Add("OLD_BACK_RESNCODE", typeof(string));
            dtResult.Columns.Add("UPDATE_FLAG", typeof(string));

            if (CommonVerify.HasTableRow(dtResult))
            {
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    dtResult.Rows[i]["OLD_ADJ_STRT_PSTN"] = dtResult.Rows[i]["ADJ_STRT_PSTN"];
                    dtResult.Rows[i]["OLD_ADJ_END_PSTN"] = dtResult.Rows[i]["ADJ_END_PSTN"];
                    dtResult.Rows[i]["OLD_BACK_RESNCODE"] = dtResult.Rows[i]["BACK_RESNCODE"];
                    dtResult.Rows[i]["UPDATE_FLAG"] = "N";
                }
            }

            dgLotInfo.ItemsSource = DataTableConverter.Convert(dtResult);
        }

        private void GetRollMapOutsideScrapList()
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_OUTSIDE_SCRAP";

            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("ADJ_LOTID", typeof(string));
            IndataTable.Columns.Add("ADJ_WIPSEQ", typeof(decimal));
            IndataTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["ADJ_LOTID"] = txtLot.Text;
            Indata["ADJ_WIPSEQ"] = _wipSeq;
            Indata["EQPT_MEASR_PSTN_ID"] = "OUTSIDE_SCRAP";
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", IndataTable);
            dgOutsideScrap.ItemsSource = DataTableConverter.Convert(dtResult);

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

            //startSection = _selectedStartSection;
            //endSection = _selectedEndSection;

            object[] parameters = new object[11];
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
            parameters[10] = Visibility.Visible;
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

        private void SetCoaterChartContextMenu(bool isEnabled)
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
            dr["CONF_TYPE"] = "USER_CONFIG_COMBOBOX";
            dr["CONF_KEY2"] = cboMeasurementTop.Name;
            dr["CONF_KEY3"] = cboMeasurementBack.Name;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (!string.IsNullOrEmpty(dtResult.Rows[0]["USER_CONF01"].GetString()) && !Util.isNumber(dtResult.Rows[0]["USER_CONF01"].GetString()))
                {
                    cboMeasurementTop.SelectedValue = dtResult.Rows[0]["USER_CONF01"].GetString();
                }
                if (!string.IsNullOrEmpty(dtResult.Rows[0]["USER_CONF02"].GetString()) && !Util.isNumber(dtResult.Rows[0]["USER_CONF02"].GetString()))
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
            dr["CONF_TYPE"] = "USER_CONFIG_COMBOBOX";
            dr["CONF_KEY2"] = cboMeasurementTop.Name;
            dr["CONF_KEY3"] = cboMeasurementBack.Name;
            dr["USER_CONF01"] = cboMeasurementTop.SelectedValue;
            dr["USER_CONF02"] = cboMeasurementBack.SelectedValue;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) => { });
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
                newRow["COM_CODE"] = _processCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tagSection_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_CoordinateType == CoordinateType.Absolutecoordinates)
            {
                Util.MessageValidation("SFU8541", ObjectDic.Instance.GetObjectName("구간불량 등록"));
                return;
            }
        }

        private void FindTagSection(int rowindex)
        {
            //if (rowindex < 0 || !_util.GetDataGridCheckValue(dgLotInfo, "CHK", rowindex)) return;

            string borderTag = DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "LOTID").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "WIPSEQ").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "EQPT_MEASR_PSTN_ID").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "CLCT_SEQNO").GetString()
                + ";" + DataTableConverter.GetValue(dgLotInfo.Rows[rowindex].DataItem, "TAG_AUTO_FLAG").GetString();

            System.Windows.Media.Animation.DoubleAnimation doubleAnimation = new System.Windows.Media.Animation.DoubleAnimation();

            foreach (Grid grid in Util.FindVisualChildren<Grid>(chartCoater))
            {
                if (grid.Name == "gdSectionTag")
                {
                    if (grid.Tag.GetString() == borderTag)
                    {
                        doubleAnimation.From = grid.ActualWidth;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                        doubleAnimation.AutoReverse = true;
                        doubleAnimation.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
                        grid.BeginAnimation(WidthProperty, doubleAnimation);
                    }
                    else
                    {
                        grid.BeginAnimation(WidthProperty, null);
                    }
                }
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

        #region //TEST CUT 
        private void tcDataCollect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.RemovedItems.Count > 0)
            {
                C1TabItem olditem = e.RemovedItems[0] as C1TabItem;
                if (olditem != null)
                {
                    if (string.Equals(olditem.Name, "tiTagSection"))
                    {
                        dgLotInfo.EndEdit(true);
                    }
                }
            }
        }

        private void dgTestCut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid == null) return;
        }

        private void dgTestCutChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    int iRow = idx;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        dgTestCut.SelectedIndex = idx;
                    }

                    dSumTopLoss = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "TOP_LOSS").GetDouble();
                    dSumBackLoss = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "BACK_LOSS").GetDouble();

                    if (dSumTopLoss == 0 || dSumBackLoss == 0)
                    {
                        dSumTopLoss =
                        DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "TOP_COATING_LEN_QTY").GetDouble()
                        - DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "TOP_BACK_COATING_LEN_QTY").GetDouble();

                        dSumBackLoss = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "TOP_BACK_COATING_LEN_QTY").GetDouble();
                    }

                    cboTCTopLoss.SelectedValue = (string.IsNullOrEmpty(DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "TOP_RESNCODE").GetString())) ? "SELECT" : DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "TOP_RESNCODE").GetString();
                    cboTCBackLoss.SelectedValue = (string.IsNullOrEmpty(DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "BACK_RESNCODE").GetString())) ? "SELECT" : DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "BACK_RESNCODE").GetString();

                    rdoTCApplyY.IsChecked = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "LOSS_APPLY_FLAG").GetString() == "Y" ? true : false;
                    rdoTCApplyN.IsChecked = DataTableConverter.GetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[iRow].DataItem, "LOSS_APPLY_FLAG").GetString() == "Y" ? false : true;

                }
                else
                {
                    dSumTopLoss = 0;
                    dSumBackLoss = 0;
                }

                if (rdoTCApplyY.IsChecked == true)
                {
                    txtTCTopLossQty.Text = dSumTopLoss.ToString();
                    txtTCBackLossQty.Text = dSumBackLoss.ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgTestCut_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgTestCut.CurrentRow == null || dgTestCut.SelectedIndex == -1 || dgTestCut.CurrentColumn == null)
            {
                return;
            }

            int iRow = dgTestCut.CurrentRow.Index;
            dgTestCut.SelectedIndex = iRow;

            dgTestCut.GetCell(iRow, dgTestCut.Columns["CHK"].Index);
            if (dgTestCut.GetCell(iRow, dgTestCut.Columns["CHK"].Index).Presenter == null) return;

            RadioButton rb = dgTestCut.GetCell(iRow, dgTestCut.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;

            for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                if (iRow == i)   // Mode = OneWay 이므로 Set 처리.
                    DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                else
                    DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
            }
        }

        private void txtTCTopLossQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtTCTopLossQty.Text, 0))
                {
                    txtTCTopLossQty.Text = "";
                    return;
                }

                double dNow = 0;
                double.TryParse(txtTCTopLossQty.Text, out dNow);

                if (dSumTopLoss >= 0 && dSumTopLoss < dNow)
                {
                    Util.MessageValidation("SFU3107");   // 수량이 이전 수량보다 많이 입력 되었습니다.
                    txtTCTopLossQty.Text = dSumTopLoss == 0 ? "0" : dSumTopLoss.ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTCBackLossQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtTCBackLossQty.Text, 0))
                {
                    txtTCBackLossQty.Text = "";
                    return;
                }

                double dNow = 0;
                double.TryParse(txtTCBackLossQty.Text, out dNow);

                if (dSumBackLoss >= 0 && dSumBackLoss < dNow)
                {
                    Util.MessageValidation("SFU3107");   // 수량이 이전 수량보다 많이 입력 되었습니다.
                    txtTCBackLossQty.Text = dSumBackLoss == 0 ? "0" : dSumBackLoss.ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoTCApplyY_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoTCApplyY.IsChecked != null)
            {
                if (rdoTCApplyY.IsChecked == true)
                {

                    txtTCTopLossQty.Text = dSumTopLoss.ToString();
                    txtTCBackLossQty.Text = dSumBackLoss.ToString();
                }
            }
        }

        private void rdoTCApplyN_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoTCApplyN.IsChecked != null)
            {
                if (rdoTCApplyN.IsChecked == true)
                {

                    txtTCTopLossQty.Text = "0";
                    txtTCBackLossQty.Text = "0";
                }
            }
        }

        private void btnSaveTestCut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTestCut(dgTestCut)) return;

            // 선택한 항목을 Loss로 반영하시겠습니까?
            if (rdoTCApplyY.IsChecked == true)
            {
                Util.MessageConfirm("SFU5173", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveTestCut(dgTestCut);
                    }
                });
            }
            else if (rdoTCApplyN.IsChecked == true)
            {
                //  항목을 Loss 미반영 하시겠습니까?
                Util.MessageConfirm("SFU5174", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveTestCut(dgTestCut);
                    }
                });
            }

        }

        private void SetControlTestCut()
        {
            SelectLossCombo(cboTCTopLoss, "DEFECT_TOP");
            SelectLossCombo(cboTCBackLoss, "DEFECT_BACK");
        }

        private void SelectLossCombo(C1ComboBox cb, string sResnposition)
        {
            try
            {
                cb.ItemsSource = null;

                const string bizRuleName = "DA_PRD_SEL_ACTIVITYREASON_ELEC_AFTER_NEXT_PROC";
                string[] arrColumn = { "LANGID", "AREAID", "LOTID", "WIPSEQ", "ACTID", "RESNPOSITION" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _runLotId, _wipSeq, "LOSS_LOT", sResnposition };
                string selectedValueText = "RESNCODE";
                string displayMemberText = "RESNNAME";

                CommonCombo.CommonBaseCombo(bizRuleName, cb, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, selectedValue: "SELECT");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectTestCut(C1DataGrid dg)
        {
            try
            {
                Util.gridClear(dg);

                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("ROLLMAP_ADJ_INFO_TYPE_CODE", typeof(string));

                DataRow newRow = dt.NewRow();
                newRow["LOTID"] = _runLotId;
                newRow["ROLLMAP_ADJ_INFO_TYPE_CODE"] = "TEST_CUT";
                dt.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ADJ_TEST_CUT", "RQSTDT", "RSLTDT", dt);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dg, dtResult, FrameOperation);
                    DataRow dr = dtResult.Rows[0];

                    DataTableConverter.SetValue(dgTestCut.Rows[0].DataItem, "CHK", true);

                    txtTCTopLossQty.Text = (string.IsNullOrEmpty(dr["TOP_LOSS"].ToString())) ? "0" : dr["TOP_LOSS"].ToString();
                    txtTCBackLossQty.Text = (string.IsNullOrEmpty(dr["BACK_LOSS"].ToString())) ? "0" : dr["BACK_LOSS"].ToString();

                    cboTCTopLoss.SelectedValue = (string.IsNullOrEmpty(dr["TOP_RESNCODE"]?.ToString())) ? "SELECT" : dr["TOP_RESNCODE"].ToString();
                    cboTCBackLoss.SelectedValue = (string.IsNullOrEmpty(dr["BACK_RESNCODE"]?.ToString())) ? "SELECT" : dr["BACK_RESNCODE"].ToString();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveTestCut(C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() <= 0) return;

                dg.EndEdit(true);

                DataSet ds = new DataSet();

                DataTable dtData = new DataTable("IN_DATA");
                dtData.Columns.Add("EQPTID", typeof(string));
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("WIPSEQ", typeof(string));
                dtData.Columns.Add("ROLLMAP_ADJ_INFO_TYPE_CODE", typeof(string));
                dtData.Columns.Add("INPUT_LOTID", typeof(string));
                dtData.Columns.Add("TOP_BACK_REVS_FLAG", typeof(string));
                dtData.Columns.Add("STRT_PSTN1", typeof(double));
                dtData.Columns.Add("END_PSTN1", typeof(double));
                dtData.Columns.Add("STRT_PSTN2", typeof(double));
                dtData.Columns.Add("END_PSTN2", typeof(double));
                dtData.Columns.Add("ADJ_ATTR1_VALUE", typeof(string));
                dtData.Columns.Add("ADJ_ATTR2_VALUE", typeof(string));
                dtData.Columns.Add("USERID", typeof(string));
                dtData.Columns.Add("FORCE_FLAG", typeof(string)); // Y 이면 다음 공정 투입 여부 체크 안함

                foreach (C1.WPF.DataGrid.DataGridRow row in dgTestCut.Rows)
                {
                    if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" ||
                                                             Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                    {
                        DataRow dr = dtData.NewRow();
                        dr["EQPTID"] = _equipmentCode;
                        dr["LOTID"] = _runLotId;
                        dr["WIPSEQ"] = _wipSeq;
                        dr["ROLLMAP_ADJ_INFO_TYPE_CODE"] = "TEST_CUT";
                        dr["INPUT_LOTID"] = DataTableConverter.GetValue(row.DataItem, "INPUT_LOTID").GetString();
                        dr["TOP_BACK_REVS_FLAG"] = rdoTCApplyY.IsChecked == true ? "Y" : "N";
                        dr["STRT_PSTN1"] = Util.NVC_Decimal(txtTCTopLossQty.Text);
                        dr["END_PSTN1"] = Util.NVC_Decimal(txtTCBackLossQty.Text);
                        dr["STRT_PSTN2"] = DataTableConverter.GetValue(row.DataItem, "TOP_LOSS").GetDecimal();
                        dr["END_PSTN2"] = DataTableConverter.GetValue(row.DataItem, "BACK_LOSS").GetDecimal();
                        if (!string.IsNullOrEmpty(txtTCTopLossQty.Text) && Convert.ToDecimal(txtTCTopLossQty.Text) != 0)
                            dr["ADJ_ATTR1_VALUE"] = Util.GetCondition(cboTCTopLoss);
                        else
                            dr["ADJ_ATTR1_VALUE"] = string.Empty;
                        if (!string.IsNullOrEmpty(txtTCBackLossQty.Text) && Convert.ToDecimal(txtTCBackLossQty.Text) != 0)
                            dr["ADJ_ATTR2_VALUE"] = Util.GetCondition(cboTCBackLoss);
                        else
                            dr["ADJ_ATTR2_VALUE"] = string.Empty;
                        dr["USERID"] = LoginInfo.USERID;

                        dtData.Rows.Add(dr);
                    }
                }


                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("BR_PRD_REG_TEST_CUT_RM", "IN_DATA", null, dtData, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다

                        IsUpdated = true;
                        //TEST CUT 조회
                        SelectTestCut(dg);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetDataGridResonCombo(C1.WPF.DataGrid.DataGridColumn dgcol, CommonCombo.ComboStatus status, string resonGroupId)
        {
            string actid = resonGroupId.Equals("DEFECT_TOP") ? "LOSS_LOT" : "LOSS_LOT,DEFECT_LOT";

            const string bizRuleName = "DA_PRD_SEL_ACTIVITYREASON_ELEC_AFTER_NEXT_PROC";
            string[] arrColumn = { "LANGID", "AREAID", "LOTID", "WIPSEQ", "ACTID", "RESNPOSITION" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, txtLot.Text, _wipSeq, actid, resonGroupId };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
        }

        private void SetDataGridBackResonCombo(C1.WPF.DataGrid.DataGridColumn dgcol)
        {
            const string bizRuleName = "DA_PRD_SEL_ACTIVITYREASON_ELEC_AFTER_NEXT_PROC";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("RESNPOSITION", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LANGID"] = LoginInfo.LANGID;
            inData["AREAID"] = LoginInfo.CFG_AREA_ID;
            inData["LOTID"] = txtLot.Text;
            inData["WIPSEQ"] = _wipSeq;
            inData["ACTID"] = "LOSS_LOT,DEFECT_LOT";
            inData["RESNPOSITION"] = "DEFECT_BACK";
            inDataTable.Rows.Add(inData);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            _dtBackReason = dtResult.Copy();

            DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { dgcol.SelectedValuePath(), dgcol.DisplayMemberPath() });
            C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;
            if (dataGridComboBoxColumn != null)
                dataGridComboBoxColumn.ItemsSource = dtBinding.Copy().AsDataView();

        }

        private void SetDataGridCollectTypeCombo(C1.WPF.DataGrid.DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            const string bizRuleName = "DA_BAS_SEL_DEFECT_CBO";
            string[] arrColumn = { "PROCID", "EQPTID", "EQPT_MEASR_PSTN_ID", "LANGID" };
            string[] arrCondition = { _processCode, _equipmentCode, "OUTSIDE_SCRAP", LoginInfo.LANGID };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
        }

        private bool ValidationTestCut(C1DataGrid dg)
        {
            // Loss 반영 – YES : 체크박스 선택유무 확인, Loss Code 선택 확인 후 체크한 항목을 Loss로 반영하시겠습니까? 메시지
            if (dg.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1886");     // 정보가 없습니다.
                return false;
            }

            // Loss 반영

            List<DataRow> drList = dg.GetCheckedDataRow("CHK");
            if (drList.Count == 0)
            {
                Util.MessageValidation("SFU1651");  // 선택된 항목이 없습니다.
                return false;
            }

            if (!string.IsNullOrEmpty(txtTCTopLossQty.Text) && Convert.ToDecimal(txtTCTopLossQty.Text) != 0 && cboTCTopLoss.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1639");  // 선택된 불량코드가 없습니다
                return false;
            }

            if (!string.IsNullOrEmpty(txtTCBackLossQty.Text) && Convert.ToDecimal(txtTCBackLossQty.Text) != 0 && cboTCBackLoss.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1639");  // 선택된 불량코드가 없습니다
                return false;
            }


            return true;
        }

        #endregion

        private bool ValidationSearch()
        {
            if (string.IsNullOrEmpty(txtLot.Text))
            {
                Util.MessageValidation("SFU1366");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            int selectedRowIndex = 0;

            //중복체크 변수 BizRule 에서 중복 체크 처리 하기로 함.
            //string lotId = string.Empty;
            //decimal wipseq = 0;
            //string measurementPosition = string.Empty;
            //decimal collectSeq = 0;
            //decimal adjustmentStartPosition = 0;
            //decimal adjustmentEndPosition = 0;
            //int uniqueTagsections = 0;

            foreach (C1.WPF.DataGrid.DataGridRow row in dgLotInfo.Rows)
            {
                if (DataTableConverter.GetValue(row.DataItem, "UPDATE_FLAG").GetString() == "Y")
                {
                    if (DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal() > DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal())
                    {
                        Util.MessageValidation("SFU8530");    //시작위치와 종료위치를 확인하세요.
                        return false;
                    }

                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "BACK_RESNCODE").GetString()))
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("활동사유"));
                        return false;
                    }

                    /*
                    lotId = DataTableConverter.GetValue(row.DataItem, "LOTID").GetString();
                    wipseq = DataTableConverter.GetValue(row.DataItem, "WIPSEQ").GetDecimal();
                    measurementPosition = DataTableConverter.GetValue(row.DataItem, "EQPT_MEASR_PSTN_ID").GetString();
                    collectSeq = DataTableConverter.GetValue(row.DataItem, "CLCT_SEQNO").GetDecimal();
                    adjustmentStartPosition = DataTableConverter.GetValue(row.DataItem, "ADJ_STRT_PSTN").GetDecimal();
                    adjustmentEndPosition = DataTableConverter.GetValue(row.DataItem, "ADJ_END_PSTN").GetDecimal();

                    DataTable dt = ((DataView)dgLotInfo.ItemsSource).Table;

                    if (CommonVerify.HasTableRow(dt))
                    {
                        uniqueTagsections = (from t in dt.AsEnumerable()
                                             where t.Field<string>("LOTID") == lotId
                                             && t.Field<decimal>("WIPSEQ") == wipseq
                                             && t.Field<string>("EQPT_MEASR_PSTN_ID") == measurementPosition
                                             //&& t.Field<decimal>("CLCT_SEQNO") == collectSeq
                                             && t.Field<decimal>("ADJ_STRT_PSTN") == adjustmentStartPosition
                                             && t.Field<decimal>("ADJ_END_PSTN") == adjustmentEndPosition
                                             select t).Distinct().Count();

                        if (uniqueTagsections > 1)
                        {
                            Util.MessageValidation("중복된 시작위치 종료위치가 존재 합니다.");
                            return false;
                        }
                    }
                    */

                    selectedRowIndex++;
                }
            }

            if (selectedRowIndex < 1)
            {
                // 변경된 데이터가 없습니다.
                Util.MessageValidation("SFU1566");
                return false;
            }

            return true;
        }

        private bool ValidationDelete()
        {
            if (!CommonVerify.HasDataGridRow(dgLotInfo))
            {
                Util.MessageInfo("SFU8501", ObjectDic.Instance.GetObjectName("삭제"));
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgLotInfo, "CHK",true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }
            return true;
        }

        private bool ValidationSaveOutsideScrap()
        {
            if (!CommonVerify.HasDataGridRow(dgOutsideScrap))
            {
                Util.MessageInfo("SFU8501", ObjectDic.Instance.GetObjectName("저장"));
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgOutsideScrap, "CHK",true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            foreach (C1.WPF.DataGrid.DataGridRow row in dgOutsideScrap.Rows)
            {
                if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" ||
                                                         Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "BACK_RESNCODE").GetString()))
                    {
                        Util.MessageInfo("SFU4925", ObjectDic.Instance.GetObjectName("활동사유"));
                        return false;
                    }
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
            TagStart,
            TagEnd,
            TagToolTip,
            TagVisionTop,
            TagVisionBack,
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


        #endregion

        //nathan 2024.01.17 롤맵용 테스트 컷 적용 - start

        private DataTable SelectTestCut_NonArea()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("ROLLMAP_ADJ_INFO_TYPE_CODE", typeof(string));

                DataRow newRow = dt.NewRow();
                newRow["LOTID"] = _runLotId;
                newRow["ROLLMAP_ADJ_INFO_TYPE_CODE"] = "TEST_CUT_NON_AREA";
                dt.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ADJ_TEST_CUT", "RQSTDT", "RSLTDT", dt);

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable SelectTestCutTotalQty_NonArea()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow newRow = dt.NewRow();
                newRow["LOTID"] = _runLotId;
                dt.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "RQSTDT", "RSLTDT", dt);

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void btnRollMapTestCut_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageValidation("SUF9024", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    btnSaveTestCut_NonArea.IsEnabled = true;
                }
            }, _runLotId);
        }

        private void setTestCut_NonArea()
        {
            if (_bUseSampleCut == true) return; //SampleCut 사용 공장은 사용하지 않음.

            DataTable dtTotal = SelectTestCutTotalQty_NonArea();
            if (dtTotal == null) return;
            if (dtTotal.Rows.Count == 0) return;

            DataRow drTotal = dtTotal.Rows[0];

            dtTotal.Columns.Add("TOP_LOSS");
            dtTotal.Columns.Add("BACK_LOSS");

            Util.GridSetData(dgTestCut_NonArea, dtTotal, FrameOperation);

            double TotalQty = Convert.ToDouble(drTotal["EQPT_INPUT_M_TOTL_QTY"]);
            double TopQty = Convert.ToDouble(drTotal["EQPT_INPUT_M_TOP_QTY"]);
            double TopBackQty = Convert.ToDouble(drTotal["EQPT_INPUT_M_TOP_BACK_QTY"]);
            double TopLoss = TopQty - TopBackQty;
            double BackLoss = TopBackQty;


            DataTableConverter.SetValue(dgTestCut_NonArea.Rows[0].DataItem, "TOP_LOSS", TopLoss.ToString());
            DataTableConverter.SetValue(dgTestCut_NonArea.Rows[0].DataItem, "BACK_LOSS", BackLoss.ToString());

            //사용자 입력값
            DataTable dtResult = SelectTestCut_NonArea();

            if (dtResult == null) return;
            if (dtResult.Rows.Count == 0) return;

            DataRow dr = dtResult.Rows[0];

            _drADJInfo_NonArea = dr;

            cboTCTopLoss_NonArea.SelectedValue = dr["TOP_RESNCODE"].ToString();
            cboTCBackLoss_NonArea.SelectedValue = dr["BACK_RESNCODE"].ToString();

            if (dr["LOSS_APPLY_FLAG"].ToString() == "Y")
            {
                rdoTCApplyY_NonArea.IsChecked = true;
                rdoTCApplyN_NonArea.IsChecked = false;
            }
            else
            {
                rdoTCApplyY_NonArea.IsChecked = false;
                rdoTCApplyN_NonArea.IsChecked = true;
            }
        }

        private void btnSaveTestCut_NonArea_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTestCut_NonArea.Rows.Count <= 0) return;

                DataSet ds = new DataSet();

                DataTable dtData = new DataTable("INDATA");
                dtData.Columns.Add("EQPTID", typeof(string));
                dtData.Columns.Add("LOTID", typeof(string));
                dtData.Columns.Add("WIPSEQ", typeof(string));
                dtData.Columns.Add("CLCT_SEQNO", typeof(string));
                dtData.Columns.Add("ROLLMAP_ADJ_INFO_TYPE_CODE", typeof(string));
                dtData.Columns.Add("INPUT_LOTID", typeof(string));
                dtData.Columns.Add("TOP_BACK_REVS_FLAG", typeof(string));
                dtData.Columns.Add("STRT_PSTN1", typeof(double));
                dtData.Columns.Add("END_PSTN1", typeof(double));
                dtData.Columns.Add("ADJ_ATTR1_VALUE", typeof(string));
                dtData.Columns.Add("ADJ_ATTR2_VALUE", typeof(string));
                dtData.Columns.Add("USERID", typeof(string));
                dtData.Columns.Add("DEL_FLAG", typeof(string));


                DataRow dr = dtData.NewRow();
                dr["EQPTID"] = _equipmentCode;
                dr["LOTID"] = _runLotId;
                dr["WIPSEQ"] = _wipSeq;
                dr["CLCT_SEQNO"] = 1;
                dr["ROLLMAP_ADJ_INFO_TYPE_CODE"] = "TEST_CUT_NON_AREA";

                dr["TOP_BACK_REVS_FLAG"] = rdoTCApplyY_NonArea.IsChecked == true ? "Y" : "N";

                double topLoss = Convert.ToDouble(DataTableConverter.GetValue(dgTestCut_NonArea.Rows[0].DataItem, "TOP_LOSS"));
                double backLoss = Convert.ToDouble(DataTableConverter.GetValue(dgTestCut_NonArea.Rows[0].DataItem, "BACK_LOSS"));

                dr["STRT_PSTN1"] = topLoss;
                dr["END_PSTN1"] = backLoss;

                if (topLoss != 0)
                {
                    if (cboTCTopLoss_NonArea.SelectedIndex < 0 || cboTCTopLoss_NonArea.SelectedValue.ToString() == "SELECT")
                    {
                        Util.MessageValidation("SFU8393", "TOP_LOSS");
                        return;
                    }
                    dr["ADJ_ATTR1_VALUE"] = Util.GetCondition(cboTCTopLoss_NonArea);
                }
                else
                    dr["ADJ_ATTR1_VALUE"] = string.Empty;

                if (backLoss != 0)
                {
                    if (cboTCBackLoss_NonArea.SelectedIndex < 0 || cboTCBackLoss_NonArea.SelectedValue.ToString() == "SELECT")
                    {
                        Util.MessageValidation("SFU8393", "BACK_LOSS");
                        return;
                    }
                    dr["ADJ_ATTR2_VALUE"] = Util.GetCondition(cboTCBackLoss_NonArea);
                }
                else
                    dr["ADJ_ATTR2_VALUE"] = string.Empty;

                dr["USERID"] = LoginInfo.USERID;
                dr["DEL_FLAG"] = "N";

                if (_drADJInfo_NonArea != null)
                {
                    dr["CLCT_SEQNO"] = _drADJInfo_NonArea["CLCT_SEQNO"].ToString();
                }


                dtData.Rows.Add(dr);


                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("BR_PRD_REG_TEST_CUT_NON_AREA_RM", "INDATA", null, dtData, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");     // 저장 되었습니다

                        IsUpdated = true;
                        setTestCut_NonArea();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        //nathan 2024.01.17 롤맵용 테스트 컷 적용 - end
    }
}
