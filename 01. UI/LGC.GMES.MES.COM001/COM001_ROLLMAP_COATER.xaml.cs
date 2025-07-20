/*************************************************************************************
 Created Date : 2021.05.28
      Creator : 신광희
   Decription : 자동차 전극 Roll Map - 롤맵 차트(COATER) c1Chart 로 구성
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.28  신광희 : Initial Created.
  코드 복사해서 재사용하지 말고 C1Chart 익힌 후 참조 하시고, 참조 시 출처를 표시 바람.
  2022.07.07  이춘우 : 웹게이지(#5) TotalDry가 추가됨에 따라 기존 웹게이지와 TotalDry를 분기 처리하여 Chart 표시
  2022.10.19  신광희 : 롤맵 차트 팝업 조회 시 SOM 메뉴 사용 횟수(TB_SOM_MENU_USE_COUNT) 테이블 등록
  2022.10.21  신광희 : 롤맵 차트 팝업 조회 시 ROLLMAP COLORMAP SPEC, ROLLMAP LANE SYMBOL COLOR 데이터 검색 시 하드 코딩 제거
  2022.10.24  윤기업 : RDOWBWET 추가 및 mpstn = 1 (건조전 웹게이지) 반영 그외 mpstn = 2
  2022.11.16  신광희 : TAG_SECTION 병합 기능 추가 함
  2022.11.24  신광희 : 불량 팝업 신규 개발
  2022.11.29  신광희 : 불량 범례 우측 불량수량 표시
  2022.12.13  신광희 : 코터 맵 차트 영역 MouseRightButtonUp 이벤트 ContextMenu 기능 추가(구간불량 등록 위치 설정)
  2023.01.20  신광희 : 동별 공정별 롤맵 실적 연계 여부 값으로 구간불량, 최외곽 폐기 기능 개선(_isRollMapResultLink Y 인 경우 버튼(구간불량,최외곽 폐기 Visibility = Visibility.Collapsed 병합기능 IsEnabled = false))
  2023.01.30  신광희 : 절대좌표 선택 시 구간불량 병합 및 등록 제한 처리
  2023.02.01  신광희 : 롤맵LOT 여부에 따른 구간불량, 최외곽 폐기 컨트롤 기능 개선
  2023.03.02  윤기업 : [E20230216-000013] Coater Chart에 생산량 및 양품량 표시
  2023.03.21  신광희 : 무지부 Tab 위치 표시
  2023.12.06  신광희 : 두께측정기 Overlay 폭, Sliding, Fat Edge 불량 맵 표현 기능 추가
  2023.12.19  김지호 : [E20230922-001506] 롤맵 보정 이후(상대좌표)를 선택시 Sample Cut 미표시 / 롤맵 보정 이전(절대좌표)를 선택시 Sample Cut 표시
  2023.12.28  김지호 : [E20231123-001387] TAG_SPOT TOOLTIP내 불량 약어명 표시
  2024.01.08  신광희 : Overlay 폭, Sliding, Fat Edge 불량 표현 시 무지부와 유지부 영역에 사이에 표시. 무지부 표시 영역 개선 함. 
  2024.01.13  김지호 : [E20230922-001506] 롤맵 보정 이후(상대좌표)를 선택시 Sample Cut 미표시 / 롤맵 보정 이전(절대좌표)를 선택시 Sample Cut 표시 (오류 수정)  
  2024.02.13  정기동 : [E20240115-000246] 코터 롤맵 Material CHART의 슬러리에 믹서 버퍼의 이전/이후 배치 ID를 툴팁으로 표기. (믹서-코터 배치연계 고도화)
  2024.02.14  이재우 : 원자재 INPUT_LOTID 멀티인 경우, 구분 라인 표시 및 툴팁 표기
  2024.01.13  김지호 : [E20240223-001703] CHART TOP/BACK 분리
  2024.03.05  신광희 : Overlay 폭, Sliding, Fat Edge 불량 표현 시 LOTATTR.HALF_SLIT_SIDE 컬럼과 TB_SFC_ROLLMAP_CLCT_INFO.HALF_SLIT_SIDE 컬럼으로 Y 좌표 산술식 수정 GetFirstLanePosition() 
  2024.03.19  김지호 : [E20240319-000656] 범례에 Non 추가 및 차트 색 (회색) 추가, SCAN_COLRMAP에 Err, Non, Ch 시에 Tooltip에 데이터 오류, 데이터 미수신, 데이터 미측정 내용 표시
  2024.05.10  정기동 : Sliding 불량 표현 시 Tab 부착 방향이 아닌 곳에 표현되는 현상 수정 
  2024.06.18  박학철 : 롤맵 차트 보정 이후 선택 후 구간불량 수동등록시 등록 가능 좌표 설정을 위한 샘플컷 좌표 초기화 하도록 추가
  2024.06.20  정기동 : 부분 LANE 불량 툴팁 표기 기능 추가 (TAG_SECTION_LANE) 
  2024.07.18  이민영 : NFF 요청사항 대응, MMD 동별공통코드 ROLLMAP_EQPT_DETAIL_INFO에 설정된 설비기준 1LANE을 위로 표시
  2024.07.30  황석동 : Overlay 폭, Sliding, Fat Edge 불량 표현 시 LOTATTR.HALF_SLIT_SIDE 값이 없는 경우 EQUIPMENTATTR.S43 첫번째 LANE 위치[FRST_LANE_PSTN] 가져오도록 수정 함.
  2024.08.10  김지호 : E20240724-000827 TAG_SECTION_SINGLE 추가에 따른 수정
  2024.09.09  김지호 : [E20240731-000740] 측정위치 콤보박스 열면 항목이 보이지 않는 오류 수정 (기존에 콤보박스 항목을 하드코딩으로 넣는 것을 공통코드에서 불러오도록 수정)
  2024.09.19  김지호 : [E20240905-001389] TAG_SECTION 툴팁내 불량 명칭 및 총 거리값 표시되도록 수정
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

    public partial class COM001_ROLLMAP_COATER : C1Window, IWorkArea
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
        private string _productCode = string.Empty;
        private double _xMaxLength;
        private double _sampleCutLength;
        private string _polarityCode = string.Empty;
        private string _firstLanePosition = string.Empty;
        private string _version = string.Empty;
        private DataTable _dtColorMapLegend;
        private DataTable _dtPoint;
        private DataTable _dtGraph;
        private DataTable _dtDefect;
        private DataTable _dtLaneInfo;
        private DataTable _dtLaneLegend;
        private DataTable _dtLane;
        private DataTable _dtTagSection;
        private static DataTable _dt2DBarcodeInfo;
        private bool _isResearch;
        private string _selectedStartSectionText = string.Empty;
        private string _selectedStartSectionPosition = string.Empty;
        private string _selectedStartSampleFlag = string.Empty;
        private string _selectedEndSectionPosition = string.Empty;
        private string _selectedEndSampleFlag = string.Empty;
        private string _selectedCollectType = string.Empty;
        private string sTDFlag = string.Empty;

        private bool _isSelectedTagSection;
        private bool _isRollMapResultLink = false;   // 동별 공정별 롤맵 실적 연계 여부
        private double _selectedStartSection = 0;
        private double _selectedEndSection = 0;
        private double _selectedchartPosition = 0;
        private bool _isRollMapLot;
        private bool _isShowBtnDeft = false;	//2023-10-05 롤맵 불량 비교버튼
        private static bool _isLaneUADReverse = false; //2024-07-18 LANE 상하반전표시 대상 설비 여부
        private bool _isEquipmentTotalDry = false; // 롤맵 설비 Total Dry 사용 유무

        private CoordinateType _CoordinateType;
        private enum CoordinateType
        {
            RelativeCoordinates,    //상대좌표
            Absolutecoordinates     //절대좌표
        }

        public COM001_ROLLMAP_COATER()
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
            this.Closed += COM001_ROLLMAP_COATER_Closed;
        }

        private void COM001_ROLLMAP_COATER_Closed(object sender, EventArgs e)
        {
            try
            {
                SetCboUserConf();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /*
        private void btnSlurryInputSummary_Click(object sender, RoutedEventArgs e)
        {
            CMM_ROLLMAP_INPUTQTY popupRollmapInputQty = new CMM_ROLLMAP_INPUTQTY();
            popupRollmapInputQty.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = _runLotId;
            parameters[3] = _wipSeq;
            parameters[4] = _laneQty;
            C1WindowExtension.SetParameters(popupRollmapInputQty, parameters);
            Dispatcher.BeginInvoke(new Action(() => popupRollmapInputQty.ShowModal()));
        }

        private void btnFoil2DBarCodeInput_Click(object sender, RoutedEventArgs e)
        {
            CMM_ROLLMAP_RESIDUALQTY popupRollmapResidualQty = new CMM_ROLLMAP_RESIDUALQTY();
            popupRollmapResidualQty.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = Util.NVC(txtLot.Text);
            parameters[3] = _wipSeq;
            parameters[4] = _laneQty;
            C1WindowExtension.SetParameters(popupRollmapResidualQty, parameters);
            Dispatcher.BeginInvoke(new Action(() => popupRollmapResidualQty.ShowModal()));
        }

        private void btnInputQty_Click(object sender, RoutedEventArgs e)
        {
            CMM_ROLLMAP_INPUTQTY popupRollmapInputQty = new CMM_ROLLMAP_INPUTQTY();
            popupRollmapInputQty.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _processCode;
            parameters[1] = _equipmentCode;
            parameters[2] = _runLotId;
            parameters[3] = _wipSeq;
            parameters[4] = _laneQty;
            C1WindowExtension.SetParameters(popupRollmapInputQty, parameters);
            Dispatcher.BeginInvoke(new Action(() => popupRollmapInputQty.ShowModal()));
        }
        */

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
                // 컨트롤 초기화
                InitializeControls();

                double min = 0;
                double max = 0;
                double maxLength = 0;
                string sampleFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "N" : "Y";
                string adjFlag = rdoRelativeCoordinates.IsChecked != null && (bool)rdoRelativeCoordinates.IsChecked ? "1" : "2";
                //string mPoint_back = rdoThickness.IsChecked != null && (bool)rdoThickness.IsChecked ? "1" : "2";

                string mPoint_top = string.Empty;
                string mPstn_top = string.Empty;
                string mPoint_back = string.Empty;
                string mPstn_back = string.Empty;

                if (cboMeasurementTop.SelectedIndex == 4) mPoint_top = "3";
                else mPoint_top = (cboMeasurementTop.SelectedIndex < 2) ? "1" : "2";

                if (cboMeasurementBack.SelectedIndex == 4) mPoint_back = "3";
                else mPoint_back = (cboMeasurementBack.SelectedIndex < 2) ? "1" : "2";

                mPstn_top = (cboMeasurementTop.SelectedIndex % 2 == 0) ? "1" : "2";
                mPstn_back = (cboMeasurementBack.SelectedIndex % 2 == 0) ? "1" : "2";

                if ((mPoint_top == "3" && mPoint_back == "3") || (mPoint_top != mPoint_back))
                {
                    bdTop4.Visibility = Visibility.Visible;
                    bdBack4.Visibility = Visibility.Visible;

                    bdTotal2.Visibility = Visibility.Collapsed;
                    measurement.Visibility = Visibility.Collapsed;
                }
                else
                {
                    bdTotal1.Visibility = Visibility.Collapsed;
                    bdTop1.Visibility = Visibility.Visible;
                    bdBack1.Visibility = Visibility.Visible;

                    bdTotal2.Visibility = Visibility.Collapsed;
                    bdTop2.Visibility = Visibility.Visible;
                    bdBack2.Visibility = Visibility.Visible;

                    grdTotalLineLegend.Visibility = Visibility.Collapsed;
                    grdTopLineLegend.Visibility = Visibility.Visible;
                    grdBackLineLegend.Visibility = Visibility.Visible;

                    chartTotalLine.Visibility = Visibility.Collapsed;
                    chartTopLine.Visibility = Visibility.Visible;
                    chartBackLine.Visibility = Visibility.Visible;

                    bdTop4.Visibility = Visibility.Collapsed;
                    bdBack4.Visibility = Visibility.Collapsed;
                    measurement.Visibility = Visibility.Visible;
                }


                if (rdoRelativeCoordinates != null && (bool)rdoRelativeCoordinates.IsChecked)
                    _CoordinateType = CoordinateType.RelativeCoordinates;
                else
                    _CoordinateType = CoordinateType.Absolutecoordinates;


                // TOP : Total Dry_TOP && Back : Total Dry_BACK 
                if (cboMeasurementTop.SelectedIndex == 4 && cboMeasurementBack.SelectedIndex == 4)
                {
                    bdTop3.Text = ObjectDic.Instance.GetObjectName("TOTAL");
                    bdBack3.Text = ObjectDic.Instance.GetObjectName("TOTAL");
                }
                // TOP : Total Dry_TOP && Back : Wet(로딩량) || Dry(로딩량)           
                else if (cboMeasurementTop.SelectedIndex == 4 && cboMeasurementBack.SelectedIndex < 2)
                {
                    bdTop3.Text = ObjectDic.Instance.GetObjectName("TOTAL");
                    bdBack3.Text = ObjectDic.Instance.GetObjectName("로딩량");
                }
                // TOP : Total Dry_TOP && Back : Wet(두께) || Dry(두께)                    
                else if (cboMeasurementTop.SelectedIndex == 4 && cboMeasurementBack.SelectedIndex >= 2)
                {
                    bdTop3.Text = ObjectDic.Instance.GetObjectName("TOTAL_TOP");
                    bdBack3.Text = ObjectDic.Instance.GetObjectName("두께");
                }
                // TOP : Wet(로딩량) || Dry(로딩량) && Back : Total Dry_BACK                     
                else if (cboMeasurementTop.SelectedIndex < 2 && cboMeasurementBack.SelectedIndex == 4)
                {
                    bdTop3.Text = ObjectDic.Instance.GetObjectName("로딩량");
                    bdBack3.Text = ObjectDic.Instance.GetObjectName("TOTAL");
                }
                // TOP : Wet(두께) || Dry(두께) && Back : Total Dry_BACK               
                else if (cboMeasurementTop.SelectedIndex >= 2 && cboMeasurementBack.SelectedIndex == 4)
                {
                    bdTop3.Text = ObjectDic.Instance.GetObjectName("두께");
                    bdBack3.Text = ObjectDic.Instance.GetObjectName("TOTAL");
                }
                // TOP : Wet(로딩량) || Dry(로딩량) && Back : Wet(로딩량) || Dry(로딩량)  
                else if (cboMeasurementTop.SelectedIndex < 2 && cboMeasurementBack.SelectedIndex < 2)
                    txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");

                // TOP : Wet(두께) || Dry(두께) && Back : Wet(두께) || Dry(두께)  
                else if (cboMeasurementTop.SelectedIndex >= 2 && cboMeasurementBack.SelectedIndex >= 2)
                    txtMeasurement.Text = ObjectDic.Instance.GetObjectName("두께");

                // TOP : Wet(로딩량) || Dry(로딩량) && Back : Wet(두께) || Dry(두께)  
                else if (cboMeasurementTop.SelectedIndex < 2 && cboMeasurementBack.SelectedIndex >= 2)
                {
                    bdTop3.Text = ObjectDic.Instance.GetObjectName("로딩량");
                    bdBack3.Text = ObjectDic.Instance.GetObjectName("두께");
                }

                // TOP : Wet(두께) || Dry(두께) && Back : Wet(로딩량) || Dry(로딩량)               
                else if (cboMeasurementTop.SelectedIndex >= 2 && cboMeasurementBack.SelectedIndex < 2)
                {
                    bdTop3.Text = ObjectDic.Instance.GetObjectName("두께");
                    bdBack3.Text = ObjectDic.Instance.GetObjectName("로딩량");
                }
                else
                    txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");


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
                    DrawCicleLengthChart(dtLength);

                    min = dtLength.AsEnumerable().ToList().Min(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
                    max = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
                }

                SetCoaterChartContextMenu(true);

                // 원재료 데이터 조회
                DataTable dtMaterial = SelectRollMapMaterialInfo(txtLot.Text, _wipSeq, adjFlag, sampleFlag);
                if (CommonVerify.HasTableRow(dtMaterial))
                {
                    DrawMaterialChart(dtMaterial, min, max);
                }

                // 측정기 맵, 그래프 조회 lotId, wipSeq, mPoint_back, position, adjFlag
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

                    DrawLineChart(dtGraph, maxLength);

                    DrawLaneLegend(); // Lane 표시
                }
                else
                {
                    InitializeCoaterChart();
                }

                DrawRollMapLane();


                // Defect, tag, Vision, Mark..
                _dt2DBarcodeInfo = Get2DBarcodeInfo(txtLot.Text, _equipmentCode);
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
            parameters[10] = (_isRollMapResultLink && _isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;
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

        //private void rdoWebgaugeDry_Click(object sender, RoutedEventArgs e)
        //{
        //    e.Handled = true;
        //    GetLegend();
        //}

        //private void rdoLoading_Click(object sender, RoutedEventArgs e)
        //{
        //    e.Handled = true;
        //    GetLegend();
        //}

        private void chkTotal_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            GetLegend();
        }

        //private void rdoThickness_Click(object sender, RoutedEventArgs e)
        //{
        //    e.Handled = true;
        //    GetLegend();
        //}

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

            ////NFF Lane 불량 정보 표기에 따른 변경 - Start
            //// 부분 불량인 경우 리턴
            System.Text.RegularExpressions.Match matches = System.Text.RegularExpressions.Regex.Match(textBlock.Text, @"\([^{2})]*\)");
            if (matches.Success && matches.Length > 0)
            {
                return;
            }
            ////NFF Lane 불량 정보 표기에 따른 변경 - End

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
            parameters[10] = (_isRollMapResultLink && _isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;
            C1WindowExtension.SetParameters(popRollMapPositionUpdate, parameters);
            popRollMapPositionUpdate.Closed += popRollMapPositionUpdate_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapPositionUpdate.ShowModal()));
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tagMerge_Click(object sender, RoutedEventArgs e)
        {
            if (_isRollMapResultLink && _isRollMapLot) return;

            MenuItem itemmenu = sender as MenuItem;
            ContextMenu itemcontext = itemmenu.Parent as ContextMenu;
            TextBlock textBlock = itemcontext.PlacementTarget as TextBlock;

            if (textBlock == null) return;

            ////NFF Lane 불량 정보 표기에 따른 변경 - Start
            //// 부분 불량인 경우 리턴
            System.Text.RegularExpressions.Match matches = System.Text.RegularExpressions.Regex.Match(textBlock.Text, @"\([^{2})]*\)");
            if (matches.Success && matches.Length > 0)
            {
                return;
            }
            ////NFF Lane 불량 정보 표기에 따른 변경 - End

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

                    //chartCoater.BeginUpdate();
                    //for (int i = chartCoater.Data.Children.Count - 1; i >= 0; i--)
                    //{
                    //    DataSeries dataSeries = chartCoater.Data.Children[i];
                    //    if (dataSeries.Tag.GetString().Equals("TAG_SECTION"))
                    //    {
                    //        chartCoater.Data.Children.Remove(dataSeries);
                    //    }
                    //}

                    //System.Threading.Tasks.Task<bool> task = WaitCallback();
                    //task.ContinueWith(_ =>
                    //{
                    //    System.Threading.Thread.Sleep(500);
                    //    DrawRollMapTagSection(_dtTagSection);
                    //    textBlock.Background = new SolidColorBrush(Colors.White);

                    //    if (border != null)
                    //        border.Background = new SolidColorBrush(Colors.White);

                    //    _selectedStartSectionText = "S";
                    //    _selectedStartSectionPosition = splitItem[0];

                    //}, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

                    //chartCoater.EndUpdate();
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
            //if (_isRollMapResultLink && _isRollMapLot) return;

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
                ROLLMAP.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                ROLLMAP.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;
                InitializeControl();
                InitializeCombo();

                //GetVersion();
                DataTable dt = SelectEquipmentPolarity(_equipmentCode);

                if (CommonVerify.HasTableRow(dt))
                {
                    _polarityCode = dt.Rows[0]["ELTR_TYPE_CODE"].GetString();
                    //_firstLanePosition = dt.Rows[0]["FRST_LANE_PSTN"].GetString();

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

                // TotalDry 적용 설비 구분 (2022.07.13 이춘우)
                DataTable dtTD = SelectEquipmentTotalDry();
                sTDFlag = "0";

                if (CommonVerify.HasTableRow(dtTD))
                {
                    sTDFlag = dtTD.Rows.Count == 0 ? "0" : dtTD.Rows[0]["ATTRIBUTE1"].GetString();
                }

                _isLaneUADReverse = IsLaneUADReverse(); //2024-07-18 NFF 요건대응, LANE 상하반전(1Lane을 위로표시) 대상설비 여부 조회

                btnDefect.Visibility = Visibility.Collapsed;    //2023-10-05 롤맵 불량 비교버튼
                bShowDeftButton(); //2023-10-05 롤맵 불량 비교버튼
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeControl()
        {
            txtLot.Text = _runLotId;
            txtMeasurement.Text = ObjectDic.Instance.GetObjectName("로딩량");
            chartCoater.View.AxisX.ScrollBar = new AxisScrollBar();

            _isEquipmentTotalDry = IsEquipmentTotalDry(_equipmentCode);

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
            //_dtLaneLegend.Rows.Add("1", "#FFFFD700");   //Gold
            //_dtLaneLegend.Rows.Add("2", "#FFFF4500");   //OrangeRed
            //_dtLaneLegend.Rows.Add("3", "#FF87CEEB");   //SkyBlue   
            //_dtLaneLegend.Rows.Add("4", "#FF00FF00");   //Lime

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
        }

        private void InitializeCombo()
        {
            SetMeasurementCombo();
            GetCboUserConf();         
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

                /*
                #region [chart MouseMove 이벤트에 따른 값 설정]
                
                var chartPanel = new ChartPanel();
                var chartPanelObject = new ChartPanelObject()
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom
                };

                var border = new Border()
                {
                    Background = new SolidColorBrush(Colors.Green) { Opacity = 0.4 },
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1, 1, 3, 3),
                    CornerRadius = new CornerRadius(6, 6, 0, 6),
                    Padding = new Thickness(3)
                };

                var sp = new StackPanel();

                var textBlock1 = new TextBlock();
                var binding1 = new Binding();
                binding1.Source = chartPanelObject;
                binding1.StringFormat = "x={0:#.##}";
                binding1.Path = new PropertyPath("DataPoint.X");
                textBlock1.SetBinding(TextBlock.TextProperty, binding1);

                var textBlock2 = new TextBlock();
                var binding2 = new Binding();
                binding2.Source = chartPanelObject;
                binding2.StringFormat = "y={0:#.##}";
                binding2.Path = new PropertyPath("DataPoint.Y");
                textBlock2.SetBinding(TextBlock.TextProperty, binding2);

                sp.Children.Add(textBlock1);
                sp.Children.Add(textBlock2);

                border.Child = sp;

                chartPanelObject.Content = border;
                chartPanelObject.DataPoint = new Point();
                chartPanelObject.Action = ChartPanelAction.MouseMove;

                chartPanel.Children.Add(chartPanelObject);

                c1Chart.View.Layers.Add(chartPanel);
                #endregion
                */
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

            /*
            DataTable dt = SelectRollMapGplmWidth();
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
                                chartCoater.Data.Children.Insert(0, alarmZone);
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
                                    chartCoater.Data.Children.Insert(0, alarmZone);
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
                                    chartCoater.Data.Children.Insert(0, alarmZone);
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
                                chartCoater.Data.Children.Insert(0, alarmZone);
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
                                    chartCoater.Data.Children.Insert(0, alarmZone);
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
                                    chartCoater.Data.Children.Insert(0, alarmZone);
                                }
                            }
                        }

                        rowIndex++;
                        yLength = yLength + dt.Rows[j]["LANE_LENGTH"].GetDouble();
                    }
                }
            }
            */
        }

        private void InitializePointChart_TD(double xLength)
        {

            DataTable dt = SelectRollMapGplmWidth();
            if (CommonVerify.HasTableRow(dt))
            {
                var convertFromString = ColorConverter.ConvertFromString("#D5D5D5");
                string[] typeArray = { "Top" };


                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    if (dt.Rows[j]["COATING_PATTERN"].GetString() == "Plain")
                    {
                        AlarmZone alarmZone = new AlarmZone
                        {
                            Near = 0,
                            Far = xLength,
                            ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null,
                            LowerExtent = dt.Rows[j]["Y_STRT_PSTN"].GetDouble(),
                            UpperExtent = dt.Rows[j]["Y_END_PSTN"].GetDouble(),
                            ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null
                        };
                        chartCoater.Data.Children.Insert(0, alarmZone);
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

            if (grdTopLineLegend.ColumnDefinitions.Count > 0) grdTopLineLegend.ColumnDefinitions.Clear();
            if (grdTopLineLegend.RowDefinitions.Count > 0) grdTopLineLegend.RowDefinitions.Clear();
            grdTopLineLegend.Children.Clear();

            if (grdBackLineLegend.ColumnDefinitions.Count > 0) grdBackLineLegend.ColumnDefinitions.Clear();
            if (grdBackLineLegend.RowDefinitions.Count > 0) grdBackLineLegend.RowDefinitions.Clear();
            grdBackLineLegend.Children.Clear();

            if (grdTotalLineLegend.ColumnDefinitions.Count > 0) grdTotalLineLegend.ColumnDefinitions.Clear();
            if (grdTotalLineLegend.RowDefinitions.Count > 0) grdTotalLineLegend.RowDefinitions.Clear();
            grdTotalLineLegend.Children.Clear();

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

            //2023-10-05 롤맵 불량 비교버튼
            //btnDefect.Visibility = _isRollMapLot ? Visibility.Collapsed : Visibility.Visible;
            if (_isShowBtnDeft == true) btnDefect.Visibility = Visibility.Visible;
            else btnDefect.Visibility = Visibility.Collapsed;

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
            _selectedStartSectionText = string.Empty;
            _selectedStartSectionPosition = string.Empty;
            _selectedEndSectionPosition = string.Empty;
            _selectedCollectType = string.Empty;
            _selectedStartSampleFlag = string.Empty;
            _selectedEndSampleFlag = string.Empty;
            _firstLanePosition = string.Empty;

            _dtPoint?.Clear();
            _dtGraph?.Clear();
            _dtDefect?.Clear();
            _dtLaneInfo?.Clear();
            _dtLane?.Clear();
            _dtTagSection?.Clear();
        }

        private void GetLegend()
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
            }
            else
            {
                dtBinding.AsEnumerable().Where(r => r.Field<string>("CMCODE") == "TD").ToList().ForEach(row => row.Delete());
                dtBinding.AcceptChanges();
                cboMeasurementTop.ItemsSource = dtBinding.Copy().AsDataView();
                cboMeasurementBack.ItemsSource = dtBinding.Copy().AsDataView();
            }
        }

        private void GetVersion()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("EQSGID", typeof(string));
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("MODLID", typeof(string));
            inTable.Columns.Add("PROCSTATE", typeof(string));
            inTable.Columns.Add("TOPLOTID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQSGID"] = _equipmentSegmentCode;
            dr["PROCID"] = _processCode;
            dr["EQPTID"] = _equipmentCode;
            dr["LOTID"] = txtLot.Text;
            dr["MODLID"] = _productCode;

            if (string.Equals(_productCode, Process.ROLL_PRESSING))
                dr["PROCSTATE"] = "Y";
            else
                dr["PROCSTATE"] = null;
            inTable.Rows.Add(dr);

            DataTable dtVersion = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT", "INDATA", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtVersion))
                _version = dtVersion.Rows[0]["PROD_VER_CODE"].GetString();
            else
                _version = string.Empty;
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
            newRow["EQPTID"] = _equipmentCode;
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

                DataRow newRow = dtInLot.NewRow();
                newRow["LOTID"] = _runLotId;
                newRow["WIPSEQ"] = _wipSeq;
                dtInLot.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_LOT", null, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                //2024-07-18 Lane 상하반전 대상 설비일 경우 조회된 Lane정보의 정렬을 오름차순으로 처리한다.(1Lane이 위로)
                if (_isLaneUADReverse == true)
                    dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderBy(o => o.Field<string>("LANE_NO_CUR").GetDecimal()).ThenBy(x => x.Field<int>("RN")).CopyToDataTable();
                else //기존로직, Default
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

        // Total Dry 설비 유뮤 확인 (2022.07.13 이춘우)
        private DataTable SelectEquipmentTotalDry()
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TOTALDRY_USE";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "ROLLMAP_EQPT_TOTALDRY_USE_FLAG";
            dr["CMCODE"] = _equipmentCode;
            inTable.Rows.Add(dr);

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
            chart.View.AxisX.Min = 0; //dtLength.AsEnumerable().ToList().Min(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
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
                //dr["TOOLTIP"] = "SOURCE_END_PSTN :" + dtLength.Rows[i]["SOURCE_END_PSTN"].GetString();
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

        #region DrawMaterialChart
        private void DrawMaterialChart(DataTable dt, double minLength, double maxLength)
        {
            if (CommonVerify.HasTableRow(dt))
            {
                #region Validation(데이터 존재여부)

                #region 1.음극/양극 Measure PSTN 기준으로 조회결과 Filtering
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
                #endregion

                #region 2.음극/양극 Measure PSTN별 차트 초기화(영역은 회색처리)
                InitializeMaterialChart(minLength, maxLength);
                #endregion

                #region 3.Filtering 결과에 존재하는 음극/양극 Measuer PSTN
                var query = (from t in dt.AsEnumerable()
                             select new
                             {
                                 MeasurementPosition = t.Field<string>("EQPT_MEASR_PSTN_ID"),
                                 MaterialDescription = t.Field<string>("MATERIAL_DESC")
                             }).Distinct().ToList();
                #endregion

                if (query.Any())
                {
                    #region 4.Filtering 결과에서 해당 극성으로 존재하는 Measure PSTN이 존재할 경우
                    foreach (var item in query)
                    {
                        #region 4-1.Measure PSTN별 수집정보중 필요한 정보만 다시 재구성한다.
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
                        #endregion

                        if (queryMaterial.Any())
                        {
                            #region 4-2.Measure PSTN별 재구성 데이터 Row별로 AlarmZone 처리
                            foreach (var row in queryMaterial)
                            {
                                //StartPosition이 없으면 처리불가
                                if (string.IsNullOrEmpty(row.StartPosition.GetString())) continue;

                                #region 4-2-1.AlarmZone(XYDataSeries)로 높이(100), 너비(Near~Far)의 사각형 영역을 생성한다.
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
                                #endregion

                                #region 4-2-2.AlarmZone 차트에 로드시 툴팁정보 구성
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
                                #endregion

                                #region 4-2-3.극성별 Measure PSTN별로 생성한 AlarmZone을 차트에 추가한다.
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
                                #endregion
                            }
                            #endregion
                        }
                    }
                    #endregion
                }

                #region 5.음극/양극별 Measure PSTN별 필터링된 데이터에서 신규컬럼(SOURCE_AVG_VALUE / SOURCE_Y_PSTN) 추가 후 INPUT_LOTID 표시
                //1.INPUT_LOTID 존재하는 ROW에 SOURCE_AVG_VALUE값 입력
                //2.Measure PSTN(INS_SLURRY_TOP, UW, INS_SLURRY_BACK)인 경우, SOURCE_Y_PSTN(0) 나머지 30
                DataTable dtMaterial = MakeTableForDisplay(dt, ChartDisplayType.Material);
                DataTable copyTable = dtMaterial.Clone();

                if (CommonVerify.HasTableRow(dtMaterial))
                {
                    foreach (DataRow row in dtMaterial.Rows)
                    {
                        #region 5-1.INPUT_LOTID 정보가 있는 경우만, 차트에 표시
                        if (string.IsNullOrEmpty(row["INPUT_LOTID"].GetString())) continue;
                        #endregion

                        #region 5-2.극성별 Measure PSTN별 해당 차트 지정
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
                        #endregion

                        copyTable.ImportRow(row);

                        #region 5-3.XYDataSeries로 Resources(chartMaterial)를 사용
                        XYDataSeries dsMaterial = new XYDataSeries();
                        dsMaterial.ItemsSource = DataTableConverter.Convert(copyTable);
                        dsMaterial.XValueBinding = new Binding("SOURCE_AVG_VALUE");
                        dsMaterial.ValueBinding = new Binding("SOURCE_Y_PSTN");
                        dsMaterial.ChartType = ChartType.XYPlot;
                        dsMaterial.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                        dsMaterial.SymbolFill = new SolidColorBrush(Colors.Transparent);
                        dsMaterial.PointLabelTemplate = grdMain.Resources["chartMaterial"] as DataTemplate;
                        dsMaterial.Margin = new Thickness(0);
                        #endregion

                        #region 5-4.XYDataSeries 차트에 로드시 표시할 ToolTip 설정
                        dsMaterial.PlotElementLoaded += (s, e) =>
                        {
                            PlotElement pe = (PlotElement)s;
                            pe.Stroke = new SolidColorBrush(Colors.Transparent);
                            pe.Fill = new SolidColorBrush(Colors.Transparent);
                            // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                        };
                        #endregion

                        #region 5-5.차트에 XYDataSeries 추가
                        c1Chart.Data.Children.Add(dsMaterial);
                        #endregion

                        //XYDataSeries당 DataRow 1행으로 처리하기 위해, 삭제
                        copyTable.Rows.RemoveAt(0);
                    }

                    #region 5-6.[2024-02-13:LJW]Foil(UW)에 INPUT_LOTID가 복수인 경우, 점선으로 표시 추가
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
                    #endregion
                }
                #endregion

                #endregion
            }
        }
        #endregion

        #region DrawUWInputLotLine
        /// <summary>
        /// Foil(UW)에 멀티 INPUT_LOTID인 경우, LINE으로 표시
        /// </summary>
        /// <param name="dt">원자재 UW DataTable</param>
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
        #endregion

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
                dtTagSection.TableName = "TAG_SECTION";
                _dtTagSection = dtTagSection;

                if (CommonVerify.HasTableRow(dtTagSection))
                {
                    DrawRollMapTagSection(dtTagSection);
                }

                var queryTagSectionSignle = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSectionSingle = queryTagSectionSignle.Any() ? queryTagSectionSignle.CopyToDataTable() : dtPoint.Clone();
                dtTagSectionSingle.TableName = "TAG_SECTION_SINGLE";

                if (CommonVerify.HasTableRow(dtTagSectionSingle))
                {
                    DrawRollMapTagSection(dtTagSectionSingle);
                }

                // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
                var queryTagSectionLane = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
                DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dtPoint.Clone();
                dtTagSectionLane.TableName = "TAG_SECTION_LANE";
                //_dtTagSection = dtTagSectionLane;

                if (CommonVerify.HasTableRow(dtTagSectionLane))
                {
                    DrawRollMapTagSection(dtTagSectionLane);
                }

                var queryTagSpot = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTagSpot))
                {
                    DrawRollMapTagSpot(dtTagSpot);
                }

                // Fat Edge, Sliding, OverLay UI 표현 추가
                // 1. Fat Edge
                var queryFatEdge = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_FAT_EDGE")).ToList();
                DataTable dtFatEdge = queryFatEdge.Any() ? queryFatEdge.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtFatEdge))
                {
                    dtFatEdge.TableName = "dtFatEdge";
                    DrawRollMapFatEdge(dtFatEdge);
                }

                var queryTopSliding = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_SLIDING")).ToList();
                DataTable dtTopSliding = queryTopSliding.Any() ? queryTopSliding.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTopSliding))
                {
                    dtTopSliding.TableName = "dtTopSliding";
                    DrawRollMapTopSliding(dtTopSliding);
                }

                var queryBackSliding = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_SLIDING")).ToList();
                DataTable dtBackSliding = queryBackSliding.Any() ? queryBackSliding.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtBackSliding))
                {
                    dtBackSliding.TableName = "dtBackSliding";
                    DrawRollMapBackSliding(dtBackSliding);
                }

                var queryTopOverLay = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_OVERLAY_WIDTH")).ToList();
                DataTable dtTopOverLay = queryTopOverLay.Any() ? queryTopOverLay.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTopOverLay))
                {
                    dtTopOverLay.TableName = "dtTopOverLay";
                    DrawRollMapTopOverLay(dtTopOverLay);
                }

                var queryBackOverLay = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_OVERLAY_WIDTH")).ToList();
                DataTable dtBackOverLay = queryBackOverLay.Any() ? queryBackOverLay.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtBackOverLay))
                {
                    dtBackOverLay.TableName = "dtBackOverLay";
                    DrawRollMapBackOverLay(dtBackOverLay);
                }


                List<DataRow> drListTopLegend;

                drListTopLegend = dtPoint.AsEnumerable().Where(o =>
                o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_SLIDING")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_OVERLAY_WIDTH")).ToList();

                DataTable dtPointTopLegend = drListTopLegend.Any() ? drListTopLegend.CopyToDataTable() : dtPoint.Clone();
                if (CommonVerify.HasTableRow(dtPointTopLegend))
                {
                    dtPointTopLegend.TableName = "TopLegend";
                    DrawPointLegend(dtPointTopLegend);
                }

                List<DataRow> drListBackLegend;

                drListBackLegend = dtPoint.AsEnumerable().Where(o =>
                o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_BACK")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_BACK")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_BACK")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_FAT_EDGE")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_SLIDING")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_OVERLAY_WIDTH")).ToList();

                DataTable dtPointBackLegend = drListBackLegend.Any() ? drListBackLegend.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtPointBackLegend))
                {
                    dtPointBackLegend.TableName = "BackLegend";
                    DrawPointLegend(dtPointBackLegend);
                }

                /*
                var queryPointTopLegend = dtPoint.AsEnumerable().Where(o =>
                o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_SURF_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("VISION_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("INS_ALIGN_VISION_NG_TOP")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_SLIDING")
                || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_TOP_OVERLAY_WIDTH")
                ).ToList();
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
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_FAT_EDGE")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_SLIDING")
                    || o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("THICK_NG_BACK_OVERLAY_WIDTH")
                    ).ToList();
                DataTable dtPointBackLegend = queryPointBackLegend.Any() ? queryPointBackLegend.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtPointBackLegend))
                {
                    dtPointBackLegend.TableName = "BackLegend";
                    DrawPointLegend(dtPointBackLegend);
                }
                */

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
                    //Near = row["SOURCE_STRT_PSTN"].GetDouble(),
                    //Far = row["SOURCE_END_PSTN"].GetDouble(),
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
                    //DataTable dtTag = MakeTableForDisplay(dtTagPosition, x == 0 ? ChartDisplayType.TagStart : ChartDisplayType.TagEnd);
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

                ds.PlotElementLoaded += (s, e) =>
                {
                    PlotElement pe = (PlotElement)s;
                    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                    pe.Fill = new SolidColorBrush(Colors.Transparent);
                };
                chartCoater.Data.Children.Add(ds);
            }
        }

        private void DrawRollMapFatEdge(DataTable dtFatEdge)
        {
            DrawRollMapThicknessDefect(dtFatEdge);
        }

        private void DrawRollMapTopSliding(DataTable dtTopSliding)
        {
            DrawRollMapThicknessDefect(dtTopSliding);
        }

        private void DrawRollMapBackSliding(DataTable dtBackSliding)
        {
            DrawRollMapThicknessDefect(dtBackSliding);
        }

        private void DrawRollMapTopOverLay(DataTable dtTopOverLay)
        {
            DrawRollMapThicknessDefect(dtTopOverLay);
        }

        private void DrawRollMapBackOverLay(DataTable dtBackOverLay)
        {
            DrawRollMapThicknessDefect(dtBackOverLay);
        }

        private void DrawRollMapThicknessDefect(DataTable dt)
        {
            _firstLanePosition = GetFirstLanePosition();
            if (string.IsNullOrEmpty(_firstLanePosition)) return;

            double axisYPosition;
            if (dt.TableName.IndexOf("Top", StringComparison.Ordinal) > -1)
            {
                axisYPosition = 120;
            }
            else
            {
                axisYPosition = 0;
            }

            if (CommonVerify.HasTableRow(_dtLaneInfo))
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "Y_STRT_PSTN", DataType = typeof(double) });
                dt.Columns.Add(new DataColumn() { ColumnName = "Y_END_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dt.Rows)
                {
                    if (_dtLaneInfo.AsEnumerable().Any(o => o.Field<string>("LANE_NO_CUR").Equals(row["ADJ_LANE_NO"].GetString())))
                    {

                        var query = (from t in _dtLaneInfo.AsEnumerable()
                                     where t.Field<string>("LANE_NO") == row["ADJ_LANE_NO"].GetString()
                                     select new { LaneNo = t.Field<string>("LANE_NO"), Rank = t.Field<Int32>("RN") }).FirstOrDefault();


                        if (query != null)
                        {

                            //설비의 첫번째 Lane 위치 값이 L 인 경우
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) L 인경우 Sliding Top -> Top 영역 무지부 하단
                            //                                        Sliding Back -> Back 영역 무지부 하단
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) R 인경우 Sliding Top -> Top 영역 무지부 상단
                            //                                        Sliding Back -> Back 영역 무지부 상단

                            //설비의 첫번째 Lane 위치 값이 R 인 경우
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) L 인경우 Sliding Top -> Top 영역 무지부 상단
                            //                                        Sliding Back -> Back 무지부 상단
                            //HALF 슬리팅 면(HALF_SLIT_SIDE) R 인경우 Sliding Top -> Top 영역 무지부 하단
                            //                                        Sliding Back -> Back 무지부 하단

                            //1 안 소스
                            /*
                            if(string.Equals(_firstLanePosition,"L"))
                            {
                                if(row["HALF_SLIT_SIDE"].GetString().Equals("L"))
                                {
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") > query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition - queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition + 1.5;
                                    }
                                }
                                else
                                {
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") < query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition + queryInfo.yStartPosition - 1.5;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition;
                                    }
                                }
                            }
                            else
                            {
                                if (row["HALF_SLIT_SIDE"].GetString().Equals("L"))
                                {
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") < query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition + queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition;
                                    }

                                }
                                else
                                {
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") > query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition + queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition;
                                    }
                                }
                            }
                            */

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
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") > query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition + queryInfo.yStartPosition - 1.0;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition;
                                    }
                                }
                                else
                                {
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") < query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition + queryInfo.yStartPosition;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition + 1.0;
                                    }
                                }
                            }
                            else
                            {
                                if (row["HALF_SLIT_SIDE"].GetString().Equals("L"))
                                {
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") < query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition + queryInfo.yStartPosition + 1.0;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition;
                                    }

                                }
                                else
                                {
                                    var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                     where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                     && t.Field<Int32>("RN") > query.Rank
                                                     select new
                                                     {
                                                         yPosition = t.Field<double>("LANE_LENGTH_SUM"),
                                                         yStartPosition = t.Field<decimal>("Y_STRT_PSTN").GetDouble(),
                                                         yEndPosition = t.Field<decimal>("Y_END_PSTN").GetDouble()
                                                     }
                                                     ).FirstOrDefault();

                                    if (queryInfo != null)
                                    {
                                        row["Y_STRT_PSTN"] = axisYPosition + queryInfo.yStartPosition - 1.0;
                                        row["Y_END_PSTN"] = axisYPosition + queryInfo.yEndPosition;
                                    }
                                }
                            }

                            /*
                            if (_firstLanePosition == "L")
                            {
                                var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                 where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                 && t.Field<Int32>("RN") > query.Rank
                                                 select new { yPosition = t.Field<double>("LANE_LENGTH_SUM") }
                                                 ).FirstOrDefault();

                                if (queryInfo != null)
                                {
                                    row["Y_STRT_PSTN"] = queryInfo.yPosition + axisYPosition;
                                    row["Y_END_PSTN"] = queryInfo.yPosition + 3 + axisYPosition;
                                }
                            }
                            else
                            {
                                var queryInfo = (from t in _dtLaneInfo.AsEnumerable()
                                                 where t.Field<string>("LANE_NO_CUR") == query.LaneNo
                                                 && t.Field<Int32>("RN") < query.Rank
                                                 select new { yPosition = t.Field<double>("LANE_LENGTH_SUM") }
                                                 ).FirstOrDefault();

                                if (queryInfo != null)
                                {
                                    row["Y_STRT_PSTN"] = queryInfo.yPosition - 3 + axisYPosition;
                                    row["Y_END_PSTN"] = queryInfo.yPosition + axisYPosition;
                                }
                            }
                            */
                        }

                    }
                }

                if (CommonVerify.HasTableRow(dt))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        AlarmZone alarmZone = new AlarmZone();
                        var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dt.Rows[i]["COLORMAP"]));
                        alarmZone.Near = dt.Rows[i]["ADJ_STRT_PSTN"].GetDouble();
                        alarmZone.Far = dt.Rows[i]["ADJ_END_PSTN"].GetDouble();
                        alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                        alarmZone.LowerExtent = dt.Rows[i]["Y_STRT_PSTN"].GetDouble();
                        alarmZone.UpperExtent = dt.Rows[i]["Y_END_PSTN"].GetDouble();
                        alarmZone.ConnectionFill = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;

                        int x = i;
                        alarmZone.PlotElementLoaded += (s, e) =>
                        {
                            PlotElement pe = (PlotElement)s;
                            if (pe is Lines)
                            {
                                double sourceStartPosition = Convert.ToDouble(dt.Rows[x]["SOURCE_STRT_PSTN"]);
                                double sourceEndPosition = Convert.ToDouble(dt.Rows[x]["SOURCE_END_PSTN"]);
                                string content = dt.Rows[x]["CMCDNAME"] + "[" + $"{sourceStartPosition:###,###,###,##0.##}" + "m" + "~" + $"{sourceEndPosition:###,###,###,##0.##}" + "m" + "]";

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
            DataRow[] drSample = new DataRow[] { };

            //if ((bool)rdoRelativeCoordinates.IsChecked)
            //    drSample = dtSample.Select("SMPL_FLAG <> 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");
            //else
            //    drSample = dtSample.Select("SMPL_FLAG = 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");

            if ((bool)rdoRelativeCoordinates.IsChecked)
                drSample = null;
            else
                drSample = dtSample.Select("SMPL_FLAG = 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");


            if (drSample != null && drSample.Length > 0)
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

                //int rewinderCount = queryLength.Count();
                int rowIndex = 0;

                if (queryLength.Any())
                {
                    foreach (var item in queryLength)
                    {
                        DataRow newLength = dtMaxLength.NewRow();

                        newLength["RAW_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";

                        if ((bool)rdoRelativeCoordinates.IsChecked)
                            newLength["SOURCE_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";
                        else
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
            if (CommonVerify.HasTableRow(dtSample) && drSample != null)
            {
                _sampleCutLength = dtSample.AsEnumerable().Where(x => x.Field<string>("SMPL_FLAG") == "Y").ToList().Sum(r => r["RAW_END_PSTN"].GetDouble()).GetDouble();
            }
            else
            {
                _sampleCutLength = 0;
            }
        }

        /*
        private void DrawChartBackGround1(DataTable dt, double xLength)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

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

                    InitializePointChart(_xMaxLength);


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

                        AlarmZone alarmZone = new AlarmZone();

                        var convertFromString = ColorConverter.ConvertFromString(Util.NVC(dt.Rows[i]["COLORMAP"]));
                        alarmZone.Near = dt.Rows[i]["ADJ_STRT_PSTN"].GetDouble();
                        alarmZone.Far = dt.Rows[i]["ADJ_END_PSTN"].GetDouble();
                        alarmZone.ConnectionStroke = convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
                        alarmZone.LowerExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yStartPosition : 0 + yStartPosition;
                        alarmZone.UpperExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yEndPosition : yEndPosition;
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

                                if (x == dt.Rows.Count - 1)
                                {
                                    stopwatch.Stop();
                                    System.Diagnostics.Debug.WriteLine("DrawChartBackGround1 PlotElementLoaded Time =======> : " + stopwatch.Elapsed.ToString());
                                }
                            }
                        };

                        chartCoater.Data.Children.Add(alarmZone);
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
        */

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

        private void DrawLineChart(DataTable dt, double xLength)
        {
            try
            {
                double maxLength = dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble() > xLength
                    ? dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble()
                    : xLength;

                chartTopLine.View.AxisX.Min = 0;
                chartTopLine.View.AxisX.Max = maxLength;

                chartBackLine.View.AxisX.Min = 0;
                chartBackLine.View.AxisX.Max = maxLength;

                chartTopLine.ChartType = ChartType.Line;
                chartBackLine.ChartType = ChartType.Line;

                dt.Columns.Add(new DataColumn { ColumnName = "ADJ_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
                foreach (DataRow row in dt.Rows)
                {
                    row["ADJ_AVG_VALUE"] = (row["ADJ_STRT_PSTN"].GetDouble() + row["ADJ_END_PSTN"].GetDouble()) / 2;
                }
                dt.AcceptChanges();

                if (CommonVerify.HasTableRow(dt))
                {
                    DrawLineLegend(dt.Copy(), _dtColorMapLegend.Copy());

                    DataTable dtLineLegend = _dtColorMapLegend.Select().AsEnumerable().OrderByDescending(o => o.Field<int>("NO")).CopyToDataTable();
                    DataRow[] newRows = dtLineLegend.Select();

                    var queryLaneInfo = (from t in _dtLaneInfo.AsEnumerable()
                                         join x in _dtLaneLegend.AsEnumerable()
                                             on t.Field<string>("LANE_NO") equals x.Field<string>("LANE_NO")
                                         select new
                                         {
                                             LaneNo = t.Field<string>("LANE_NO"),
                                             LaneColor = x.Field<string>("COLORMAP")
                                         }
                        ).ToList();

                    for (int i = 1; i < 3; i++)
                    {
                        foreach (DataRow row in newRows)
                        {
                            string valueText = "VALUE_" + row["VALUE"].GetString();

                            if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                            {
                                //if (!dt.AsEnumerable().Any(p => p.Field<Int16>("SEQ").Equals(i) && p.Field<decimal?>(valueText) != null)) continue;
                                if (!dt.AsEnumerable().Any(p => p.Field<Int16>("SEQ") == i && p.Field<decimal?>(valueText) != null)) continue;

                                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                                DataSeries dsLegend = new DataSeries();
                                dsLegend.ItemsSource = new[] { "LL", "L", "SV", "H", "HH" };
                                dsLegend.ChartType = ChartType.Line;
                                dsLegend.ValuesSource = GetLineValuesSource(dt, row["VALUE"].GetString(), i);
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

                                if (i == 1)
                                    chartTopLine.Data.Children.Add(dsLegend);
                                else
                                    chartBackLine.Data.Children.Add(dsLegend);
                            }
                        }


                        if (queryLaneInfo.Any())
                        {
                            int rowIndex = 0;

                            foreach (var item in queryLaneInfo)
                            {
                                if (!CommonVerify.HasTableRow(_dtLane)) continue;

                                var queryLane = _dtLane.AsEnumerable().Where(where => @where.Field<string>("LANE_NO").Equals(item.LaneNo)).ToList();
                                if (!queryLane.Any()) continue;

                                //var query = dt.AsEnumerable().Where(x => x.Field<Int16>("SEQ").Equals(i) && item.LaneNo == x.Field<string>("ADJ_LANE_NO")).ToList();
                                var query = dt.AsEnumerable().Where(x => x.Field<Int16>("SEQ") == i && item.LaneNo == x.Field<Int32?>("ADJ_LANE_NO").GetString()).ToList();
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
                                ds.SymbolSize = new Size(6, 6);
                                ds.ConnectionStrokeThickness = 0.8;

                                if (i == 1) chartTopLine.Data.Children.Add(ds);
                                else chartBackLine.Data.Children.Add(ds);

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

                                        double.TryParse(Util.NVC(DataTableConverter.GetValue(dp.DataObject, "SOURCE_SCAN_AVG_VALUE")), out scanAvgValue);
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

        private void DrawLineChart_TD(DataTable dt, double xLength)
        {
            try
            {
                double maxLength = dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble() > xLength
                    ? dt.AsEnumerable().ToList().Max(r => r["ADJ_END_PSTN"].GetDouble()).GetDouble()
                    : xLength;

                chartTotalLine.View.AxisX.Min = 0;
                chartTotalLine.View.AxisX.Max = maxLength;

                chartTotalLine.ChartType = ChartType.Line;

                dt.Columns.Add(new DataColumn { ColumnName = "ADJ_AVG_VALUE", DataType = typeof(double), AllowDBNull = true });
                foreach (DataRow row in dt.Rows)
                {
                    row["ADJ_AVG_VALUE"] = (row["ADJ_STRT_PSTN"].GetDouble() + row["ADJ_END_PSTN"].GetDouble()) / 2;
                }
                dt.AcceptChanges();

                if (CommonVerify.HasTableRow(dt))
                {
                    DrawLineLegend(dt.Copy(), _dtColorMapLegend.Copy());

                    DataTable dtColorMapLegend = _dtColorMapLegend.Select().AsEnumerable().OrderByDescending(o => o.Field<int>("NO")).CopyToDataTable();
                    DataRow[] newRows = dtColorMapLegend.Select();

                    var queryLaneInfo = (from t in _dtLaneInfo.AsEnumerable()
                                         join x in _dtLaneLegend.AsEnumerable()
                                             on t.Field<string>("LANE_NO") equals x.Field<string>("LANE_NO")
                                         select new
                                         {
                                             LaneNo = t.Field<string>("LANE_NO"),
                                             LaneColor = x.Field<string>("COLORMAP")
                                         }
                        ).ToList();

                    for (int i = 1; i < 3; i++)
                    {
                        foreach (DataRow row in newRows)
                        {
                            string valueText = "VALUE_" + row["VALUE"].GetString();

                            if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                            {
                                //if (!dt.AsEnumerable().Any(p => p.Field<Int16>("SEQ").Equals(i) && p.Field<decimal?>(valueText) != null)) continue;
                                if (!dt.AsEnumerable().Any(p => p.Field<Int16>("SEQ") == i && p.Field<decimal?>(valueText) != null)) continue;

                                var convertFromString = ColorConverter.ConvertFromString(Util.NVC(row["COLORMAP"]));

                                DataSeries dsLegend = new DataSeries();
                                dsLegend.ItemsSource = new[] { "LL", "L", "SV", "H", "HH" };
                                dsLegend.ChartType = ChartType.Line;
                                dsLegend.ValuesSource = GetLineValuesSource(dt, row["VALUE"].GetString(), i);
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

                                chartTotalLine.Data.Children.Add(dsLegend);
                            }
                        }


                        if (queryLaneInfo.Any())
                        {
                            int rowIndex = 0;

                            foreach (var item in queryLaneInfo)
                            {
                                if (!CommonVerify.HasTableRow(_dtLane)) continue;

                                var queryLane = _dtLane.AsEnumerable().Where(where => @where.Field<string>("LANE_NO").Equals(item.LaneNo)).ToList();
                                if (!queryLane.Any()) continue;

                                //var query = dt.AsEnumerable().Where(x => x.Field<Int16>("SEQ").Equals(i) && item.LaneNo == x.Field<string>("ADJ_LANE_NO")).ToList();
                                var query = dt.AsEnumerable().Where(x => x.Field<Int16>("SEQ") == i && item.LaneNo == x.Field<Int32?>("ADJ_LANE_NO").GetString()).ToList();
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

                                chartTotalLine.Data.Children.Add(ds);

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

                                        string content = "Load : ";
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
                                                                         new Thickness(2),  // new Thickness(5, 5, 5, 5),
                                                                         null,
                                                                         null);

                                TextBlock rectangleDescription = CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                          HorizontalAlignment.Center,
                                                                          VerticalAlignment.Center,
                                                                          11,
                                                                          FontWeights.Bold,
                                                                          Brushes.Black,
                                                                          new Thickness(1), // new Thickness(5, 5, 5, 5),
                                                                          new Thickness(0),
                                                                          item.MeasurementCode,
                                                                          Cursors.Hand,
                                                                          item.ColorMap);
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
                                                                         new Thickness(2),  // new Thickness(5, 5, 5, 5),
                                                                         null,
                                                                         null);

                                TextBlock ellipseDescription = CreateTextBlock(item.DefectName + "(" + item.DefectCount.GetString() + ")",
                                                                             HorizontalAlignment.Center,
                                                                             VerticalAlignment.Center,
                                                                             11,
                                                                             FontWeights.Bold,
                                                                             Brushes.Black,
                                                                             new Thickness(1),  // new Thickness(5, 5, 5, 5),
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

        private void DrawLineLegend(DataTable dt, DataTable dtLegend)
        {
            try
            {
                if (grdTopLineLegend.ColumnDefinitions.Count > 0) grdTopLineLegend.ColumnDefinitions.Clear();
                if (grdTopLineLegend.RowDefinitions.Count > 0) grdTopLineLegend.RowDefinitions.Clear();

                if (grdBackLineLegend.ColumnDefinitions.Count > 0) grdBackLineLegend.ColumnDefinitions.Clear();
                if (grdBackLineLegend.RowDefinitions.Count > 0) grdBackLineLegend.RowDefinitions.Clear();

                if (grdTotalLineLegend.ColumnDefinitions.Count > 0) grdTotalLineLegend.ColumnDefinitions.Clear();
                if (grdTotalLineLegend.RowDefinitions.Count > 0) grdTotalLineLegend.RowDefinitions.Clear();

                grdTopLineLegend.Children.Clear();
                grdBackLineLegend.Children.Clear();
                grdTotalLineLegend.Children.Clear();

                DataRow[] newRows = dtLegend.Select();

                DataTable copyTable = dtLegend.Clone();
                copyTable.Columns.Add(new DataColumn { ColumnName = "VALUE_AVG", DataType = typeof(double) });

                for (int y = 1; y < 3; y++)
                {
                    foreach (DataRow row in newRows)
                    {
                        string valueText = "VALUE_" + row["VALUE"];
                        var count = dt.AsEnumerable().Where(x => x.Field<Int16>("SEQ") == y).Count();

                        if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                        {
                            //if (dt.AsEnumerable().Any(o => o.Field<Int16>("SEQ").Equals(y)))
                            if (dt.AsEnumerable().Any(o => o.Field<decimal?>(valueText) != null && o.Field<Int16>("SEQ") == y))
                            {
                                double agvValue = 0;
                                var query = (from t in dt.AsEnumerable() where t.Field<decimal?>(valueText) != null && t.Field<Int16>("SEQ") == y select new { Valuecol = t.Field<decimal>(valueText) }).ToList();
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
                }


                var j = 0;
                var k = 0;

                for (int i = 0; i < copyTable.Rows.Count; i++)
                {
                    RowDefinition gridRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) };

                    if (copyTable.Rows[i]["NO"].GetInt() == 1)
                    {
                        grdTopLineLegend.RowDefinitions.Add(gridRow);
                    }
                    else
                    {
                        grdBackLineLegend.RowDefinitions.Add(gridRow);
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

                    TextBlock lineLegendDescription = CreateTextBlock(rowValue,
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
                        grdTopLineLegend.Children.Add(stackPanel);
                        j++;
                    }
                    else
                    {
                        grdBackLineLegend.Children.Add(stackPanel);
                        k++;
                    }

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

            //var query = (from t in _dtLaneInfo.AsEnumerable()
            //    join u in _dtLaneLegend.AsEnumerable() 
            //        on t.Field<string>("LANE_NO") equals u.Field<string>("LANE_NO")
            //    select new
            //    {
            //        LaneNo = t.Field<string>("LANE_NO"),
            //        LaneColor = u.Field<string>("COLORMAP")
            //    }).ToList();

            var query = (from t in _dtLaneInfo.AsEnumerable()
                         join u in _dtLaneLegend.AsEnumerable()
                             on t.Field<string>("LANE_NO") equals u.Field<string>("LANE_NO")
                         select new
                         {
                             LaneNo = t.Field<string>("LANE_NO"),
                             LaneColor = u.Field<string>("COLORMAP")
                         }).OrderByDescending(o => o.LaneNo.GetDecimal()).ToList();

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

        private void DisplayTabLocation(DataTable dtTab)
        {
            if (CommonVerify.HasTableRow(dtTab))
            {
                //DataTable dtTab = new DataTable();
                //dtTab.Columns.Add("LOC2", typeof(string));
                //dtTab.Columns.Add("LOV_VALUE", typeof(decimal));
                //dtTab.Columns.Add("LANE_NO_COT", typeof(string));
                //dtTab.Columns.Add("LANE_NO_CUR", typeof(string));
                //dtTab.Columns.Add("LANE_NO", typeof(string));
                //dtTab.Columns.Add("RN", typeof(int));
                //dtTab.Columns.Add("ANODE_CATHODE", typeof(string));
                //dtTab.Columns.Add("LINE_BOTTOM", typeof(decimal));
                //dtTab.Columns.Add("TAB", typeof(string));

                //dtTab.Rows.Add("B", 15.00000, null, "1", null, 1, "CATHODE", 1227, null);
                //dtTab.Rows.Add("C", 547.00000, null, "1", "1", 2, "CATHODE", 1227, null);
                //dtTab.Rows.Add("D", 6.00000, null, "1", null, 3, "CATHODE", 1227, "1");
                //dtTab.Rows.Add("C", 547.00000, null, "2", "2", 4, "CATHODE", 1227, null);
                //dtTab.Rows.Add("D", 6.00000, null, "2", null, 5, "CATHODE", 1227, null);
                //dtTab.Rows.Add("D", 15.00000, null, "2", null, 6, "CATHODE", 1227, "1");


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
                                //if(dtTab.AsEnumerable().Any(x => x.Field<string>("LANE_NO") == item["LANE_NO_CUR"].GetString() && x.Field<int>("RN") == (item["RN"].GetInt() - 1)))
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

        private void TextBlockLane_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock textBlock = sender as TextBlock;
                if (textBlock == null) return;

                ShowLoadingIndicator();
                DoEvents();

                _isResearch = true;

                chartTopLine.Data.Children.Clear();
                chartBackLine.Data.Children.Clear();


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

                DrawLineChart(_dtGraph.Copy(), _xMaxLength);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private IEnumerable<double> GetLineValuesSource(DataTable dt, string value, int i)
        {
            try
            {
                //var queryCount = dt.AsEnumerable().Count(x => x.Field<int>("SEQ").Equals(i));
                var queryCount = _xMaxLength.GetInt() + 1;
                double[] xx = new double[queryCount];

                string valueText = "VALUE_" + value;

                //if (dt.AsEnumerable().Any(p => p.Field<Int32>("SEQ").Equals(i) && p.Field<decimal?>(valueText) != null))
                if (dt.AsEnumerable().Any(p => p.Field<Int16>("SEQ") == i && p.Field<decimal?>(valueText) != null))
                {
                    double agvValue = 0;
                    //var query = (from t in dt.AsEnumerable() where t.Field<decimal?>(valueText) != null && t.Field<Int32>("SEQ").Equals(i) select new { Valuecol = t.Field<decimal>(valueText) }).ToList();
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

        private static double GetYValue(double maxValue, double minValue, double targetValue)
        {
            int value = (maxValue > 600) ? 600 : 0;
            return ((targetValue - value) * 100) / 600;

            /*
            if (maxValue.Equals(minValue))
            {
                return targetValue - 600;
            }
            else
            {
                //유지부 폭 정보 567
                if (maxValue < 600) maxValue = 600;
                //((input - min) * 100) / (max - min)
                return ((targetValue - minValue) * 100) / (maxValue - minValue);
            }
            */
        }

        private void SetScale(double scale)
        {
            chartCoater.View.AxisX.Scale = scale;
            //btnRefresh.IsEnabled = scale != 1;
            //btnRefresh.IsEnabled = !scale.Equals(1);
            //btnZoomIn.IsEnabled = scale > 0.002;
            //btnZoomOut.IsEnabled = scale < 1;

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
                    c1Chart.View.Margin = new Thickness(0, 0, 0, 8);
                    c1Chart.Padding = new Thickness(10, 3, 3, 5);

                }

                if (c1Chart.Name == "chartTopLine" || c1Chart.Name == "chartBackLine" || c1Chart.Name == "chartTotalLine")
                {
                    //c1Chart.View.Margin = new Thickness(0, 0, 0, 5);
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

                //// 2D BCR Count 추출
                //Dictionary<string, Tuple<int, decimal, decimal>> query2DBcrWithCountInfo = (from t in dt.AsEnumerable()
                //                                where ("TAG_SECTION".Equals(t.Field<string>("EQPT_MEASR_PSTN_ID")) &&
                //                                t.Field<string>("MRK_2D_BCD_STR").IsNotEmpty() &&
                //                                t.Field<string>("ADJ_LANE_NO").IsNotEmpty()
                //                                )
                //                                select new
                //                                {
                //                                    Mark2DBarCodeStr = t.Field<string>("MRK_2D_BCD_STR"),
                //                                    AdjStrtPstn = t.Field<decimal>("ADJ_STRT_PSTN"),
                //                                    AdjEndPstn = t.Field<decimal>("ADJ_END_PSTN")
                //                                }).GroupBy(n => new { n.Mark2DBarCodeStr, n.AdjStrtPstn, n.AdjEndPstn })
                //                              .Select(n => new
                //                              {
                //                                  Mark2DBarCode = n.Key.Mark2DBarCodeStr,
                //                                  Mark2DBarCodeAdjStrtPstn = n.Key.AdjStrtPstn,
                //                                  Mark2DBarCodeAdjEndPstn = n.Key.AdjEndPstn,
                //                                  Mark2DBarCodeCnt = n.Count()
                //                              }).ToDictionary(
                //                                (item => item.Mark2DBarCode),
                //                                (item => new Tuple<int, decimal, decimal>(item.Mark2DBarCodeCnt, item.Mark2DBarCodeAdjStrtPstn, item.Mark2DBarCodeAdjEndPstn))
                //                                );


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
                    if ("TAG_SECTION".Equals(row["EQPT_MEASR_PSTN_ID"])) row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;

                    // E20240724-000827 TAG_SECTION_SINGLE 위치 조정
                    else if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"])) row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? 220 : -62;

                    //row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();

                    // NFF 설비 특화 로직                    
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
                                t => {
                                    // 전체 Lane 구간 불량 표기
                                    //  - Lot의 전체 Lane 갯수와 롤맵 상의 불량 Lane 갯수가 동일한 경우
                                    if (laneQty == t.Item1 && adjStrtPstn == t.Item2 && adjEndPstn == t.Item3)
                                    {
                                        //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                                        row["TOOLTIP"] = row["CMCDNAME"].GetString() + " : " + row["DFCT_NAME"] + "\n" + "[" + $"{Convert.ToInt32(row["SOURCE_END_PSTN"]) - Convert.ToInt32(row["SOURCE_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                                    }
                                    // 부분 Lane 구간 불량 표기
                                    //  - Lot의 전체 Lane 갯수와 롤맵 상의 불량 Lane 갯수가 동일하지 않은 경우
                                    else
                                    {
                                        //// MRK_2D_BCD_STR이 존재할 경우 그룹핑하여 LANE_QTY와 비교
                                        String laneInfo = row["MRK_2D_BCD_STR"].ToString().Trim().Substring(0, 2);
                                        row["TOOLTIP"] = "(" + laneInfo + ")" + row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                                    }
                                }
                                );


                            //if (laneQty == mark2DBarcodeCountInfo.Item1 && adjStrtPstn == mark2DBarcodeCountInfo.Item2 && adjEndPstn == mark2DBarcodeCountInfo.Item3)
                            //{
                            //    //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                            //    row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                            //    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                            //}else
                            //{
                            //    //// MRK_2D_BCD_STR이 존재할 경우 그룹핑하여 LANE_QTY와 비교
                            //    String laneInfo = row["MRK_2D_BCD_STR"].ToString().Trim().Substring(0, 2);
                            //    row["TOOLTIP"] = "(" + laneInfo + ")" + row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                            //    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                            //}

                        }

                    }
                    // 부분 Lane 구간 불량 (서버에서 부분 Lane 구간 불량만 필터에서 조회 한다)
                    else if ("TAG_SECTION_LANE".Equals(row["EQPT_MEASR_PSTN_ID"]))
                    {
                        String laneInfo = row["LANE_NO_LIST"].ToString();
                        row["TOOLTIP"] = "(" + laneInfo + ")" + row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                    }
                    //E20240724-000827 TAG_SECTION_SINGLE 추가로 인하여 UI상에서 M 으로 표기
                    else if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"]))
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
                        //// MRK_2D_BCD_STR이 존재하지 않을 경우 기존과 같이 표기
                        //row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                        row["TOOLTIP"] = row["CMCDNAME"].GetString() + " : " + row["DFCT_NAME"].GetString() + "\n" + "[" + $"{Convert.ToDouble(row["SOURCE_END_PSTN"]) - Convert.ToDouble(row["SOURCE_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                    }

                    ////row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    ////row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString() + ";" + row["SMPL_FLAG"].GetString();
                    ////row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    ////row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                    ////NFF Lane 불량 정보 표기에 따른 변경 - End
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
                    row["TOOLTIP"] = row["CMCDNAME"] + " : " + row["ABBR_NAME"] + " [" + sourceStartPosition + "m]";
                    row["TAG"] = null;
                }
            }
            else if (chartDisplayType == ChartDisplayType.SurfaceTop || chartDisplayType == ChartDisplayType.SurfaceBack)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "Y_PSTN", DataType = typeof(double) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    if (_isLaneUADReverse == true) //2024-07-18 NFF 요건대응, 대상설비일 경우 LANE 상하반전(1Lane을 위로표시)
                        row["Y_PSTN"] = chartDisplayType == ChartDisplayType.SurfaceTop ? (row["S_WIDTH"].GetDouble() - row["Y_POSITION"].GetDouble()) + 120 : row["S_WIDTH"].GetDouble() - row["Y_POSITION"].GetDouble();
                    else
                        row["Y_PSTN"] = chartDisplayType == ChartDisplayType.SurfaceTop ? row["Y_POSITION"].GetDouble() + 120 : row["Y_POSITION"].GetDouble();
                }
            }

            dtBinding.AcceptChanges();
            return dtBinding;
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
            parameters[10] = (_isRollMapResultLink && _isRollMapLot) ? Visibility.Collapsed : Visibility.Visible;
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

        private void SetCoaterChartContextMenu(bool isEnabled)
        {
            //if (_isRollMapResultLink && _isRollMapLot)
            //{
            //    isEnabled = false;
            //}

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

        private bool GetCboUserConf()
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
            dr["CONF_KEY1"] = this.ToString(); // LGC.CMES.MES.COM001_ROLLMAP_COATER
            dr["CONF_KEY2"] = "cboMeasurementTop";
            dr["CONF_KEY3"] = "cboMeasurementBack";
            //dr["USER_CONF01"] = cboMeasurementTop.SelectedIndex.ToString();
            //dr["USER_CONF02"] = cboMeasurementBack.SelectedIndex.ToString();
            dtRqst.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);

            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                foreach (DataRow drConf in dtResult.Rows)
                {
                    // 사용자가 전에 설정한 콤보 박스대로 설정
                    cboMeasurementTop.SelectedIndex = Convert.ToInt16(dtResult.Rows[0]["USER_CONF01"]);
                    cboMeasurementBack.SelectedIndex = Convert.ToInt16(dtResult.Rows[0]["USER_CONF02"]);

                    cboMeasurementTop.IsEnabled = true;
                    cboMeasurementBack.IsEnabled = true;
                }

                return true;
            }
            else
            {
                SetRollMapDefaultGauge();
            }
            return false;
        }

        private void SetCboUserConf()
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
            dtRqst.Columns.Add("USER_CONF03", typeof(string));
            dtRqst.Columns.Add("USER_CONF04", typeof(string));
            dtRqst.Columns.Add("USER_CONF05", typeof(string));
            dtRqst.Columns.Add("USER_CONF06", typeof(string));
            dtRqst.Columns.Add("USER_CONF07", typeof(string));
            dtRqst.Columns.Add("USER_CONF08", typeof(string));
            dtRqst.Columns.Add("USER_CONF09", typeof(string));
            dtRqst.Columns.Add("USER_CONF10", typeof(string));

            DataRow drNew = dtRqst.NewRow();
            drNew["WRK_TYPE"] = "SAVE";
            drNew["USERID"] = LoginInfo.USERID;
            drNew["CONF_TYPE"] = "USER_CONFIG_CBO";
            drNew["CONF_KEY1"] = this.ToString(); // LGC.CMES.MES.ASSY001.ASSY001_001_CONFIRM
            drNew["CONF_KEY2"] = "cboMeasurementTop";
            drNew["CONF_KEY3"] = "cboMeasurementBack";
            drNew["USER_CONF01"] = cboMeasurementTop.SelectedIndex.ToString();
            drNew["USER_CONF02"] = cboMeasurementBack.SelectedIndex.ToString();
            drNew["USER_CONF03"] = string.Empty;
            drNew["USER_CONF04"] = string.Empty;
            drNew["USER_CONF05"] = string.Empty;
            drNew["USER_CONF06"] = string.Empty;
            drNew["USER_CONF07"] = string.Empty;
            drNew["USER_CONF08"] = string.Empty;
            drNew["USER_CONF09"] = string.Empty;
            drNew["USER_CONF10"] = string.Empty;
            dtRqst.Rows.Add(drNew);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
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

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (string.Equals("W", dtResult.Rows[0]["ATTR1"].GetString()))
                    {
                        cboMeasurementTop.SelectedIndex = 1; // Wet(로딩량)
                        cboMeasurementBack.SelectedIndex = 1; // Wet(로딩량)                        
                    }
                    else if (string.Equals("T", dtResult.Rows[0]["ATTR1"].GetString()))
                    {
                        cboMeasurementTop.SelectedIndex = 4; // Wet(두께)
                        cboMeasurementBack.SelectedIndex = 4; // Wet(두께)      
                    }
                    else
                    {
                        cboMeasurementTop.SelectedIndex = Convert.ToInt16(dtResult.Rows[0]["ATTR1"]);
                        cboMeasurementBack.SelectedIndex = Convert.ToInt16(dtResult.Rows[0]["ATTR2"]);
                    }
                }
                else
                {
                    cboMeasurementTop.SelectedIndex = 1; // Dry(로딩량)
                    cboMeasurementBack.SelectedIndex = 1; // Dry(로딩량)                        
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool IsLaneUADReverse()
        {
            //공통코드 ROLLMAP_EQPT_DETAIL_INFO - 속성1의 값 : Y일 경우
            //코터 차트의 1Lane을 위쪽으로 표시하기 위한 설정 조회
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
                dr["CMCODE"] = _equipmentCode;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (string.Equals("Y", dtResult.Rows[0]["ATTRIBUTE1"].GetString()))
                        return true; // 1Lane이 윗쪽
                    else
                        return false; // 1Lane이 아래쪽
                }
                else
                {
                    return false; //1Lane이 아래쪽, 기존로직
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

        private bool ValidationSearch()
        {
            if (string.IsNullOrEmpty(txtLot.Text))
            {
                Util.MessageValidation("SFU1366");
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

        private void bShowDeftButton() //2023-10-05 롤맵 불량 비교버튼
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

            if (CommonVerify.HasTableRow(dtResult))
            {
                _isShowBtnDeft = true;
            }
            else
            {
                _isShowBtnDeft = false;
            }
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