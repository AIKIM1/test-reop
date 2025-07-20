/*************************************************************************************
 Created Date : 2022.03.03
      Creator : YSH
   Decription : 롤프레스 작업자가 코터 롤맵 확인할 수 있는 모니터링정보
--------------------------------------------------------------------------------------
 [Change History]
  2022.03.09  YSH : Initial Created.
  2022.04.27  YSH : 이미지 반전
  2023.01.10  SKH : 코터 생산실적 특이사항 Comment 추가, chartLine DataTemplate ROLLMAP_CLCT_TYPE 바인딩 제거 
  2023.05.15  YSH : 기존 LOT일 경우 INS_SLURRY_BACK, SLURRY_BACK 부분 사라지는 현상 수정
  2023.05.18  YSH : SAMPLE 컷은 미 표현으로 변경
  2023.06.13  JKT : 대칭 위치 설비 대응을 위하여 방향에 따라 정/역방향 선택 기능 추가 (전기영 책임 요청)
                    언어를 영문으로 설정 후 음극 장비 조회 시 영문이 아닌 한글로 표시되는 현상 수정
                    현재 진행 위치 두껍게, 숫자 크게 표시                    
                    생산량/양품량의 정보를 모니터링 화면에도 추가적으로 표시
  2023.06.15  JKT : 역방향 조회 상태에서 불량 Description 선택 시 정방향으로 조회되는 현상 수정
  2023.08.03  JKT : Rollmap 언와인더부 모니터링 화면의 SAMPLE CUT 조회되는 현상 수정
  2023.08.22 신광희 : Maximize Button, Minimize Button 속성 수정, 범례 적용 기준정보 데이터 조회 수정, 메뉴 오픈 시 롤맵 LOT 적용유무에 따른 Close 기능 추가  
  2024.01.16 김지호 : [E20231123-001387] 롤맵 기능 개선을 통한 작업자 편의성 개선 건 (WIP_NOTE 기입량이 많은 경우 언와인더 모니터랑 화면에서 글자가 겯치는 부분을 TEXTBLOCK에서 TEXTBOX로 변경하여 수정
  2024.02.13  정기동 : [E20240115-000246] 코터 롤맵 Material CHART의 슬러리에 믹서 버퍼의 이전/이후 배치 ID를 툴팁으로 표기. (믹서-코터 배치연계 고도화)
  2024.03.31 김지호 : Foli 교체시 빨간선으로 표시
  2024.08.13 김지호 : E20240724-000827 TAG_SECTION_SINGLE 추가에 따른 수정
  2024.09.19 김지호 : E20240905-001397 정방향/역방향 라디오 버튼 사용자별 최종 선택 항목 저장
  2024.09.20 김지호 : E20240905-001389 TAG_SECTION 툴팁내 불량 명칭 및 총 거리값 표시되도록 수정
  2024.09.25  정기동 : E20240911-000907 NFF 구간 불량 정보 중 Lane 구간 불량 수집 항목 ID 체계를 HM과 동일하게 관리하도록 수정 (TAG_SECTION -> TAG_SECTION_LANE)
  2025.07.11 조성근 : 구간불량 리스트 BIZ 변경
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

namespace LGC.GMES.MES.MNT001
{

    public partial class MNT001_021 : Window, IWorkArea
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
        private string _polarityCode = string.Empty;
        private string _version = string.Empty;
        private DataTable _dtLineLegend;
        private DataTable _dtPoint;
        private DataTable _dtGraph;
        private DataTable _dtDefect;
        private DataTable _dtLaneInfo;
        private DataTable _dtLaneLegend;
        private DataTable _dtLane;
        private DataTable _dtEioAttr;
        private DataTable _dtUserConf;
        private static DataTable _dt2DBarcodeInfo;
        private bool _isResearch;
        private bool _isFirstSearch = true;
        private bool _isClosed = false;

        private ViewDirection _selectedViewDirection;   // 선택된 조회 방향 정보
        private ViewDirection _oldViewDirection;        // 이전 조회 방향 정보

        private Util _Util = new Util();

        /*-- 진행상황만 표시할 때 필요함 */
        DataTable _dtMaterial;
        double _min, _max;
        /*-----------------------------*/

        DispatcherTimer _dispatcherTimer = null;

        delegate void FHideWindow();

        #endregion

        #region Enum        
        /// <summary>
        /// 조회 방향
        /// </summary>
        private enum ViewDirection
        {
            Forward,    // 정방향
            Reverse     // 역방향
        }
        #endregion

        #region Property
        /// <summary>
        /// 조회 방향 확인
        /// </summary>
        private ViewDirection ViewDirectionValue
        {
            get
            {
                // 양극
                if (rdoCathodeViewDirectionForward != null && rdoCathodeViewDirectionForward.IsVisible && rdoCathodeViewDirectionForward.IsChecked == true)
                {
                    return ViewDirection.Forward;
                }

                if (rdoCathodeViewDirectionRevert != null && rdoCathodeViewDirectionRevert.IsVisible && rdoCathodeViewDirectionRevert.IsChecked == true)
                {
                    return ViewDirection.Reverse;
                }

                // 음극
                if (rdoAnodeViewDirectionForward != null && rdoAnodeViewDirectionForward.IsVisible && rdoAnodeViewDirectionForward.IsChecked == true)
                {
                    return ViewDirection.Forward;
                }

                if (rdoAnodeViewDirectionRevert != null && rdoAnodeViewDirectionRevert.IsVisible && rdoAnodeViewDirectionRevert.IsChecked == true)
                {
                    return ViewDirection.Reverse;
                }


                return ViewDirection.Forward;
            }
        }
        #endregion

        public MNT001_021()
        {
            //MainWindow에서 MNT001 로 시작하는 메뉴는 ShowDialog() 로 띄우기로 되어있으므로
            //모달리스로 하기 위해선 해당 화면을 닫고 새로운 창을 연다.
            this.Closing += MNT001_021_Closing;
            this.LocationChanged += MNT001_021_LocationChanged;
            InitializeComponent();
            this.Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //"X"버튼 비활성화
            var hWnd = new WindowInteropHelper(this);
            EnableMenuItem(GetSystemMenu(hWnd.Handle, false), SC_CLOSE, MF_GRAYED);

            _equipmentCode = LoginInfo.CFG_EQPT_ID; //사용자 설비정보
            _processCode = LoginInfo.CFG_PROC_ID;   //사용자 PROC 정보
            _selectedViewDirection = ViewDirectionValue;    // 정방향/ 역방향 조회 여부

            if (_dispatcherTimer == null)
            {
                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Tick += DispatcherTimer_Tick;  //이거 여러번 해주면 타이머에서 함수호출을 여러번 해줌.
            }
            long secTicks = (int)AutoSearchText.Text.GetInt() * 10000000;  //ticks 10000000 = 1초
            _dispatcherTimer.Interval = TimeSpan.FromTicks(secTicks);
            _dispatcherTimer.Start();

            Initialize();
            GetLegend();
            //GetRollMap();
        }



        #region # Event

        //메뉴창을 최소화 후 다시 올리면 윈도우 사이즈가 최대화가 안되는 것을 최대화 시켜주는 이벤트
        private void MNT001_021_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        //자동호출 이벤트
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            //doTimeStart();
            if (sender == null) return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    doTimeStart();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }
        //좌표선택 (상대좌표, 절대좌표)
        private void rdoCoording_Click(object sender, RoutedEventArgs e)
        {
            GetLegend();
            //재조회 요청_정종원책임
            GetRollMap();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
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

        //좌우반전
        private void btnReverseX_Click(object sender, RoutedEventArgs e)
        {
            chartCoater.BeginUpdate();
            chartCoater.View.AxisX.Reversed = !chartCoater.View.AxisX.Reversed;
            chartCoater.EndUpdate();
        }

        //상하반전
        private void btnReverseY_Click(object sender, RoutedEventArgs e)
        {
            chartCoater.BeginUpdate();
            chartCoater.View.AxisY.Reversed = !chartCoater.View.AxisY.Reversed;
            chartCoater.EndUpdate();
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

                if (_dispatcherTimer == null)
                {
                    _dispatcherTimer = new DispatcherTimer();
                    _dispatcherTimer.Tick += DispatcherTimer_Tick;  //이거 여러번 해주면 타이머에서 함수호출을 여러번 해줌.
                }
                else _dispatcherTimer.Stop();

                long secTicks = (int)AutoSearchText.Text.GetInt() * 10000000;  //ticks 10000000 = 1초
                _dispatcherTimer.Interval = TimeSpan.FromTicks(secTicks);
                _dispatcherTimer.Start();

            }
        }

        //해당화면 닫을 때 이벤트
        private void MNT001_021_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FHideWindow(doHideThisWindow));
        }

        //닫기버튼 눌렀을 때 이벤트
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SetRdoUserConf();

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
                ROLLMAP.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - 100;
                ROLLMAP.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height - 100;

                chartCoater.View.AxisX.ScrollBar = new AxisScrollBar();
                InitializeControl();

                DataTable dt = SelectEquipmentPolarity(_equipmentCode);


                if (CommonVerify.HasTableRow(dt))
                {
                    _polarityCode = dt.Rows[0]["ELTR_TYPE_CODE"].GetString();

                    if (!GetRdouserConf())
                    {
                        if (_polarityCode == "C")
                        {
                            grdMaterialCathode.Visibility = Visibility.Visible;
                            grdMaterialAnode.Visibility = Visibility.Collapsed;

                            //양극/음극에 따른 조회 방향 라디오 버튼 표기(C 양극, A 음극)
                            rdoCathodeViewDirectionForward.IsChecked = true;
                        }
                        else
                        {
                            grdMaterialCathode.Visibility = Visibility.Collapsed;
                            grdMaterialAnode.Visibility = Visibility.Visible;
                            //양극/음극에 따른 조회 방향 라디오 버튼 표기(C 양극, A 음극)
                            rdoAnodeViewDirectionForward.IsChecked = true;
                        }
                    }
                    else
                    {
                        if (_dtUserConf != null && _dtUserConf.Rows.Count > 0)
                        {
                            if (_dtUserConf.Rows[0]["USER_CONF01"].Equals("Forward") && _polarityCode == "C")
                            {
                                grdMaterialCathode.Visibility = Visibility.Visible;
                                grdMaterialAnode.Visibility = Visibility.Collapsed;

                                rdoCathodeViewDirectionForward.IsChecked = true;
                            }
                            if (_dtUserConf.Rows[0]["USER_CONF01"].Equals("Forward") && _polarityCode != "C")
                            {
                                grdMaterialCathode.Visibility = Visibility.Collapsed;
                                grdMaterialAnode.Visibility = Visibility.Visible;

                                rdoAnodeViewDirectionForward.IsChecked = true;
                            }
                            if (_dtUserConf.Rows[0]["USER_CONF01"].Equals("Revert") && _polarityCode == "C")
                            {
                                grdMaterialCathode.Visibility = Visibility.Visible;
                                grdMaterialAnode.Visibility = Visibility.Collapsed;

                                rdoCathodeViewDirectionRevert.IsChecked = true;
                            }
                            if (_dtUserConf.Rows[0]["USER_CONF01"].Equals("Revert") && _polarityCode != "C")
                            {
                                grdMaterialCathode.Visibility = Visibility.Collapsed;
                                grdMaterialAnode.Visibility = Visibility.Visible;

                                rdoAnodeViewDirectionRevert.IsChecked = true;
                            }
                        }
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeControl()
        {
            txtLot.Text = _runLotId;
            //chartCoater.View.AxisX.ScrollBar = new AxisScrollBar();

            _dtLineLegend = new DataTable();
            _dtLineLegend.Columns.Add("NO", typeof(int));
            _dtLineLegend.Columns.Add("COLORMAP", typeof(string));
            _dtLineLegend.Columns.Add("VALUE", typeof(string));
            _dtLineLegend.Columns.Add("LEG_DESC", typeof(string));
            _dtLineLegend.Columns.Add("GROUP", typeof(string));
            _dtLineLegend.Columns.Add("SHAPE", typeof(string));

            // 로딩량 평균
            DataTable dtColorMapSpec = GetColorMapSpec("ROLLMAP_COLORMAP_SPEC");
            if (CommonVerify.HasTableRow(dtColorMapSpec))
            {
                foreach (DataRow row in dtColorMapSpec.Rows)
                {
                    _dtLineLegend.Rows.Add(row["CMCDSEQ"].GetInt(), row["ATTRIBUTE1"].GetString(), row["CMCODE"].GetString(), row["CMCODE"].GetString(), "LOAD", "RECT");
                }
            }


            _dtDefect = new DataTable();
            _dtDefect.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            _dtDefect.Columns.Add("ABBR_NAME", typeof(string));

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

            tbCoreR.Text = string.Empty;
            tbCoreL.Text = string.Empty;    //Core 위치 표시
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


        //30초마다 실행. 사용자 지정 시 지정 시간으로 실행.
        private void doTimeStart(bool bForceRedraw = false)
        {
            try
            {
                if (!ValidationSearch(bForceRedraw)) return;
                if (_isFirstSearch) ValidationRollMapLot();

                ShowLoadingIndicator();

                _isResearch = false;
                DoEvents();

                InitializeDataTable();
                // 컨트롤 초기화
                InitializeControls();

                double min = 0;
                double max = 0;
                double maxLength = 0;
                string adjFlag = (bool)rdoP.IsChecked ? "1" : "2";

                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("LANGID", LoginInfo.LANGID);
                param.Add("LOTID", txtLot.Text);
                param.Add("EQPTID", _equipmentCode);
                param.Add("WIPSEQ", _wipSeq);
                param.Add("ADJFLAG", adjFlag);
                param.Add("SMPL_FLAG", "N");
                param.Add("MPOINT", "2");
                param.Add("MPSTN", "2");

                // Max Length 조회

                // 요청내용 : Unwinder 모니터링 화면 개선사항 추가 개발 (전기영 책임) - (2023.06.08 JEONG KI TONG - For Rollmap Project)
                // 모니터링 화면에 생산량/양품량 표시를 위한 개선
                // R/P 공정 진행 중 상태를 위한 장비 ID 조회
                //  - R/P 공정이면서, PROC 상태인 경우에는 WIPHISTORY 상의 장비(COATER) 장비 ID로 조회 
                //param["EQPTID"] = GetEquipmentCode(txtLot.Text, _equipmentCode);
                param["EQPTID"] = _dtEioAttr.Rows[0]["EQPTID"].ToString();   //이전설비로 변경

                DataTable dtLength = SelectRollMapLength(param);
                if (CommonVerify.HasTableRow(dtLength))
                {
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

                    if (_selectedViewDirection.Equals(ViewDirection.Reverse) == true)
                    {
                        // 상단의 RW, UW, 권출/권취 위치 반전 처리
                        chart.View.AxisX.Reversed = true;
                    }


                    min = dtLength.AsEnumerable().ToList().Min(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
                    max = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble();
                }

                // 원래 장비로 원복
                param["EQPTID"] = _equipmentCode;

                // 원재료 데이터 조회
                DataTable dtMaterial = SelectRollMapMaterialInfo(param);
                if (CommonVerify.HasTableRow(dtMaterial))
                {
                    //R/P UW 시 코터RW를 홀수면 역방향 짝수면 정방향
                    //역방향일 경우 End좌표와 Start좌표 계산. 
                    if ("Y".Equals(_dtEioAttr.Rows[0]["LAR_REVS_FLAG"].ToString()))
                    {

                        DataRow[] rows = dtMaterial.Select();
                        foreach (DataRow row in rows)
                        {
                            double st = row["SOURCE_STRT_PSTN"].GetDouble();
                            double ed = row["SOURCE_END_PSTN"].GetDouble();

                            row["SOURCE_STRT_PSTN"] = "".Equals(row["SOURCE_STRT_PSTN"].ToString()) ? row["SOURCE_STRT_PSTN"] : max - ed;
                            row["SOURCE_END_PSTN"] = "".Equals(row["SOURCE_END_PSTN"].ToString()) ? row["SOURCE_END_PSTN"] : max - st;

                            //툴팁 좌표 반전 2023.10.24
                            double st1 = row["ADJ_STRT_PSTN"].IsNullOrEmpty() ? 0 : row["ADJ_STRT_PSTN"].GetDouble();
                            double ed1 = row["ADJ_END_PSTN"].IsNullOrEmpty() ? 0 : row["ADJ_END_PSTN"].GetDouble();

                            row["ADJ_STRT_PSTN"] = "".Equals(row["ADJ_STRT_PSTN"].ToString()) ? row["ADJ_STRT_PSTN"] : max - ed1;
                            row["ADJ_END_PSTN"] = "".Equals(row["ADJ_END_PSTN"].ToString()) ? row["ADJ_END_PSTN"] : max - st1;

                        }

                    }
                }


                // 측정기 맵, 그래프 조회 lotId, wipSeq, mPoint, position, adjFlag
                DataTable dtGraph = SelectRollMapGraphInfo(param);
                if (CommonVerify.HasTableRow(dtGraph))
                {

                    //R/P UW 시 코터RW를 홀수면 역방향 짝수면 정방향
                    //역방향일 경우 End좌표와 Start좌표 계산. 
                    if ("Y".Equals(_dtEioAttr.Rows[0]["LAR_REVS_FLAG"].ToString()))
                    {
                        DataRow[] rows = dtGraph.Select();
                        foreach (DataRow row in rows)
                        {
                            double st = row["ADJ_STRT_PSTN"].GetDouble();
                            double ed = row["ADJ_END_PSTN"].GetDouble();

                            row["ADJ_STRT_PSTN"] = "".Equals(row["ADJ_STRT_PSTN"].ToString()) ? row["ADJ_STRT_PSTN"] : max - ed;
                            row["ADJ_END_PSTN"] = "".Equals(row["ADJ_END_PSTN"].ToString()) ? row["ADJ_END_PSTN"] : max - st;

                            //툴팁 좌표 반전 2023.10.24
                            double st1 = row["SOURCE_STRT_PSTN"].IsNullOrEmpty() ? 0 : row["SOURCE_STRT_PSTN"].GetDouble();
                            double ed1 = row["SOURCE_END_PSTN"].IsNullOrEmpty() ? 0 : row["SOURCE_END_PSTN"].GetDouble();
                            //double ast1 = row["SOURCE_X_PSTN"].IsNullOrEmpty() ? 0 : row["SOURCE_X_PSTN"].GetDouble();

                            row["SOURCE_STRT_PSTN"] = "".Equals(row["SOURCE_STRT_PSTN"].ToString()) ? row["SOURCE_STRT_PSTN"] : max - ed1;
                            row["SOURCE_END_PSTN"] = "".Equals(row["SOURCE_END_PSTN"].ToString()) ? row["SOURCE_END_PSTN"] : max - st1;
                            //row["SOURCE_X_PSTN"] = "".Equals(row["SOURCE_X_PSTN"].ToString()) ? row["SOURCE_X_PSTN"] : max - ast1;
                        }

                    }
                    _dtGraph = dtGraph.Copy();
                    DrawChartBackGround(dtGraph, maxLength);
                    //데이터 범례처리
                    InitializeControl();  //_dtLineLegend 초기화
                    DrawLineLegend(dtGraph.Copy(), _dtLineLegend.Copy());
                    DrawRollMapLane();

                    //Core 위치 표시
                    if (_selectedViewDirection.Equals(ViewDirection.Reverse) == true)
                    {
                        if ("Y".Equals(_dtEioAttr.Rows[0]["LAR_REVS_FLAG"].ToString())) // 반전이며 Y 일때
                        {
                            tbCoreR.Text = "CORE";
                            //tbCoreL.Text = "CORE";
                        }
                        else
                        {
                            //tbCoreR.Text = "CORE";
                            tbCoreL.Text = "CORE";
                        }
                    }
                    else
                    {
                        if ("Y".Equals(_dtEioAttr.Rows[0]["LAR_REVS_FLAG"].ToString())) //정방향 Y 일때
                        {
                            //tbCoreR.Text = "CORE";
                            tbCoreL.Text = "CORE";
                        }
                        else
                        {
                            tbCoreR.Text = "CORE";
                            //tbCoreL.Text = "CORE";
                        }
                    }
                }
                else
                {
                    InitializeCoaterChart();
                }

                // Defect, tag, Vision, Mark..
                _dt2DBarcodeInfo = Get2DBarcodeInfo(txtLot.Text, _equipmentCode);
                DataTable dtPoint = SelectRollMapPointInfo(param);
                if (CommonVerify.HasTableRow(dtPoint))
                {
                    //R/P UW 시 코터RW를 홀수면 역방향 짝수면 정방향
                    //역방향일 경우 End좌표와 Start좌표 계산. 
                    if ("Y".Equals(_dtEioAttr.Rows[0]["LAR_REVS_FLAG"].ToString()))
                    {
                        //int rowindex = 0;
                        DataRow[] rows = dtPoint.Select();
                        foreach (DataRow row in rows)
                        {
                            double ast = row["ADJ_X_PSTN"].IsNullOrEmpty() ? 0 : row["ADJ_X_PSTN"].GetDouble();
                            double st = row["ADJ_STRT_PSTN"].IsNullOrEmpty() ? 0 : row["ADJ_STRT_PSTN"].GetDouble();
                            double ed = row["ADJ_END_PSTN"].IsNullOrEmpty() ? 0 : row["ADJ_END_PSTN"].GetDouble();

                            //툴팁 좌표 반전 2023.10.24
                            double st1 = row["SOURCE_STRT_PSTN"].IsNullOrEmpty() ? 0 : row["SOURCE_STRT_PSTN"].GetDouble();
                            double ed1 = row["SOURCE_END_PSTN"].IsNullOrEmpty() ? 0 : row["SOURCE_END_PSTN"].GetDouble();
                            double ast1 = row["SOURCE_X_PSTN"].IsNullOrEmpty() ? 0 : row["SOURCE_X_PSTN"].GetDouble();

                            row["SOURCE_X_PSTN"] = "".Equals(row["SOURCE_X_PSTN"].ToString()) ? row["SOURCE_X_PSTN"] : max - ast1;
                            row["SOURCE_X_PSTN"] = "".Equals(row["SOURCE_X_PSTN"].ToString()) ? row["SOURCE_X_PSTN"] : max - ast1;

                            if (row["ADJ_END_PSTN"].IsNullOrEmpty())
                            {
                                row["ADJ_STRT_PSTN"] = "".Equals(row["ADJ_STRT_PSTN"].ToString()) ? row["ADJ_STRT_PSTN"] : max - st;
                                row["ADJ_END_PSTN"] = "".Equals(row["ADJ_END_PSTN"].ToString()) ? row["ADJ_END_PSTN"] : max - ed;
                                row["ADJ_X_PSTN"] = "".Equals(row["ADJ_X_PSTN"].ToString()) ? row["ADJ_X_PSTN"] : max - ast;
                                row["SOURCE_STRT_PSTN"] = "".Equals(row["SOURCE_STRT_PSTN"].ToString()) ? row["SOURCE_STRT_PSTN"] : max - st1;
                                row["SOURCE_END_PSTN"] = "".Equals(row["SOURCE_END_PSTN"].ToString()) ? row["SOURCE_END_PSTN"] : max - ed1;

                            }
                            else
                            {
                                row["ADJ_STRT_PSTN"] = "".Equals(row["ADJ_STRT_PSTN"].ToString()) ? row["ADJ_STRT_PSTN"] : max - ed;
                                row["ADJ_END_PSTN"] = "".Equals(row["ADJ_END_PSTN"].ToString()) ? row["ADJ_END_PSTN"] : max - st;
                                row["ADJ_X_PSTN"] = "".Equals(row["ADJ_X_PSTN"].ToString()) ? row["ADJ_X_PSTN"] : max - ast;
                                row["SOURCE_STRT_PSTN"] = "".Equals(row["SOURCE_STRT_PSTN"].ToString()) ? row["SOURCE_STRT_PSTN"] : max - ed1;
                                row["SOURCE_END_PSTN"] = "".Equals(row["SOURCE_END_PSTN"].ToString()) ? row["SOURCE_END_PSTN"] : max - st1;
                            }
                            //Console.WriteLine("rowindex="+rowindex++);
                        }

                    }
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

                ProcessArrow(_dtEioAttr, true);
                _dtMaterial = dtMaterial.Copy();
                _max = max;
                _min = min;
                DrawMaterialChart(dtMaterial, min, max);
                GetTagList();

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
                _isFirstSearch = false;
            }

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

        private void InitializePointChart(double xLength)
        {
            DataTable dt = SelectRollMapGplmWidth();

            if (CommonVerify.HasTableRow(dt))
            {
                DataRow[] dr = dt.Select();
                int drLength = dr.Length;
                decimal sumLength = dt.AsEnumerable().Sum(s => s.GetValue("LOV_VALUE").GetDecimal());
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

        private void InitializeDataTable()
        {
            _dtPoint?.Clear();
            _dtGraph?.Clear();
            _dtDefect?.Clear();
            _dtLaneInfo?.Clear();
            _dtLane?.Clear();
            dgLotInfo?.ClearRows();
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
                //                Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));  //조회할 수 있도록 요청할 수도 있으므로 해당내용은 주석처리로 보관한다.
                Dispatcher.BeginInvoke(new Action(() => doTimeStart(true)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetRdouserConf()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("WRK_TYPE", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("CONF_TYPE", typeof(string));
            dtRqst.Columns.Add("CONF_KEY1", typeof(string));
            dtRqst.Columns.Add("CONF_KEY2", typeof(string));
            dtRqst.Columns.Add("CONF_KEY3", typeof(string));


            DataRow dr = dtRqst.NewRow();
            dr["WRK_TYPE"] = "SELECT";
            dr["USERID"] = LoginInfo.USERID;
            dr["CONF_TYPE"] = "USER_CONFIG_RDO";
            dr["CONF_KEY1"] = this.ToString(); // LGC.GMES.MES.MNT001.MNT001_021
            dr["CONF_KEY2"] = "rdoViewDirectionForward";
            dr["CONF_KEY3"] = "rdoViewDirectionRevert";
            //dr["USER_CONF01"] = cboMeasurementTop.SelectedIndex.ToString();
            //dr["USER_CONF02"] = cboMeasurementBack.SelectedIndex.ToString();
            dtRqst.Rows.Add(dr);

            _dtUserConf = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);

            if (_dtUserConf != null && _dtUserConf.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void SetRdoUserConf()
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
            drNew["CONF_TYPE"] = "USER_CONFIG_RDO";
            drNew["CONF_KEY1"] = this.ToString(); // LGC.GMES.MES.MNT001.MNT001_021
            drNew["CONF_KEY2"] = "rdoViewDirectionForward";
            drNew["CONF_KEY3"] = "rdoViewDirectionRevert";

            if (rdoCathodeViewDirectionForward != null && rdoCathodeViewDirectionForward.IsVisible && rdoCathodeViewDirectionForward.IsChecked == true)
            {
                drNew["USER_CONF01"] = "Forward";
            }

            if (rdoCathodeViewDirectionRevert != null && rdoCathodeViewDirectionRevert.IsVisible && rdoCathodeViewDirectionRevert.IsChecked == true)
            {
                drNew["USER_CONF01"] = "Revert";
            }

            // 음극
            if (rdoAnodeViewDirectionForward != null && rdoAnodeViewDirectionForward.IsVisible && rdoAnodeViewDirectionForward.IsChecked == true)
            {
                drNew["USER_CONF01"] = "Forward";
            }

            if (rdoAnodeViewDirectionRevert != null && rdoAnodeViewDirectionRevert.IsVisible && rdoAnodeViewDirectionRevert.IsChecked == true)
            {
                drNew["USER_CONF01"] = "Revert";
            }

            drNew["USER_CONF02"] = string.Empty;
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

        private DataTable SelectRollMapLength(Dictionary<string, string> param)
        {
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_LENGTH_CT";
            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RSLTDT", param);
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
                dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderByDescending(o => o.Field<string>("LANE_NO_CUR").GetDecimal()).ThenByDescending(x => x.GetValue("RN").GetInt()).CopyToDataTable();
                var query = (from t in dtLaneInfo.AsEnumerable() where t.Field<string>("LANE_NO") != null select new { LaneNo = t.Field<string>("LANE_NO") }).Distinct().ToList();
                decimal sumLength = dtLaneInfo.AsEnumerable().Sum(s => s.GetValue("LOV_VALUE").GetDecimal());
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

            if (CommonVerify.HasTableRow(dtLaneInfo)) dtLaneInfo = dtLaneInfo.Select().AsEnumerable().OrderBy(o => o.GetValue("RN").GetInt()).CopyToDataTable();
            _dtLaneInfo = dtLaneInfo;

            _dtLane?.Clear();

            for (int i = 0; i < _dtLaneInfo.Rows.Count; i++)
            {
                DataRow newRow = _dtLane.NewRow();

                if (!string.IsNullOrEmpty(_dtLaneInfo.Rows[i]["LANE_NO"].GetString()))
                {
                    newRow["LANE_NO"] = _dtLaneInfo.Rows[i]["LANE_NO"];
                    _dtLane.Rows.Add(newRow);
                }
            }

            return dtLaneInfo;
        }

        private DataTable SelectRollMapMaterialInfo(Dictionary<string, string> param)
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_MATERIAL_CT_CHART";
            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RSLTDT", param);
        }

        private static DataTable SelectRollMapGraphInfo(Dictionary<string, string> param)
        {
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CT_CHART";
            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RSLTDT", param);
        }

        private static DataTable SelectRollMapPointInfo(Dictionary<string, string> param)
        {
            const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CT_DEFECT_CHART";
            return new ClientProxy().ExecuteServiceSync(bizRuleName, "RSLTDT", param);
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
                //                dr["TOOLTIP"] = "SOURCE_END_PSTN :" + dtLength.Rows[i]["SOURCE_END_PSTN"].GetString();
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
            if (_selectedViewDirection.Equals(ViewDirection.Reverse))
            {
                dsArrowRight.PointLabelTemplate = grdMain.Resources["arrowLeft"] as DataTemplate;
            }
            else
            {
                dsArrowRight.PointLabelTemplate = grdMain.Resources["arrowRight"] as DataTemplate;
            }


            dsArrowRight.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
                // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
            };
            chart.Data.Children.Add(dsArrowRight);

            // 코터 생산실적 특이사항 Comment 추가 삽입
            if (CommonVerify.HasTableRow(_dtEioAttr))
            {
                string wipNoteText = GetWipNote(_dtEioAttr);
                if (string.IsNullOrEmpty(wipNoteText)) return;

                txtNote.Text = ObjectDic.Instance.GetObjectName("EQPT_NOTE") + "  : " + wipNoteText;

                //DataTable dtWipNote = new DataTable();
                //dtWipNote.Columns.Add("SOURCE_STRT_PSTN", typeof(double));
                //dtWipNote.Columns.Add("SOURCE_Y_PSTN", typeof(double));
                //dtWipNote.Columns.Add("WIP_NOTE", typeof(string));

                //DataRow dr = dtWipNote.NewRow();
                //dr["SOURCE_STRT_PSTN"] = dtLength.AsEnumerable().ToList().Max(r => r["SOURCE_END_PSTN"].GetDouble()).GetDouble() / 2;
                //dr["SOURCE_Y_PSTN"] = 0;
                ////dr["WIP_NOTE"] = ObjectDic.Instance.GetObjectName("EQPT_NOTE") + "  : " + wipNoteText;
                //dtWipNote.Rows.Add(dr);               

                //XYDataSeries dsWipNote = new XYDataSeries();
                //dsWipNote.ItemsSource = DataTableConverter.Convert(dtWipNote);
                //dsWipNote.XValueBinding = new Binding("SOURCE_STRT_PSTN");
                //dsWipNote.ValueBinding = new Binding("SOURCE_Y_PSTN");
                //dsWipNote.ChartType = ChartType.XYPlot;
                //dsWipNote.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                //dsWipNote.SymbolFill = new SolidColorBrush(Colors.Transparent);
                //dsWipNote.PointLabelTemplate = grdMain.Resources["chartWipNote"] as DataTemplate;
                //dsWipNote.Margin = new Thickness(0, 0, 0, 0);

                //dsWipNote.PlotElementLoaded += (s, e) =>
                //{
                //    PlotElement pe = (PlotElement)s;
                //    pe.Stroke = new SolidColorBrush(Colors.Transparent);
                //    pe.Fill = new SolidColorBrush(Colors.Transparent);
                //    // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                //};
                //chart.Data.Children.Add(dsWipNote);
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
                        dsMaterial.Margin = new Thickness(0, 0, 0, 0);

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

                var queryTagSectionLane = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_LANE")).ToList();
                DataTable dtTagSectionLane = queryTagSectionLane.Any() ? queryTagSectionLane.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTagSectionLane))
                {
                    DrawRollMapTagSectionLane(dtTagSectionLane);
                }

                var queryTagSectionSingle = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SECTION_SINGLE")).ToList();
                DataTable dtTagSectionSingle = queryTagSectionSingle.Any() ? queryTagSectionSingle.CopyToDataTable() : dtPoint.Clone();

                if (CommonVerify.HasTableRow(dtTagSectionSingle))
                {
                    DrawRollMapTagSectionSingle(dtTagSectionSingle);

                }

                var queryTagSpot = dtPoint.AsEnumerable().Where(o => o.Field<string>("EQPT_MEASR_PSTN_ID").Equals("TAG_SPOT")).ToList();
                DataTable dtTagSpot = queryTagSpot.Any() ? queryTagSpot.CopyToDataTable() : dtPoint.Clone();

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

                    string content = defectName + " [X:" + $"{xPosition:###,###,###,##0.##}" + "m" + ", Y:" + $"{yPosition:###,###,###,##0.##}" + "mm" + "]";
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
                        string content = row["ABBR_NAME"] + "[" + $"{startPosition:###,###,###,##0.##}" + "~" + $"{endPosition:###,###,###,##0.##}" + "]";
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
                        //XValuesSource = new[] { row["SOURCE_STRT_PSTN"].GetDouble(), row["SOURCE_STRT_PSTN"].GetDouble() },
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
                    ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
                    ds.Margin = new Thickness(0, 0, 0, 0);

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

        private void DrawRollMapTagSectionLane(DataTable dtTagPosition)
        {
            // Start, End 라벨 삽입
            if (CommonVerify.HasTableRow(dtTagPosition))
            {
                // Start, End 이미지 두번의 표현으로 for문 사용
                for (int x = 0; x < 2; x++)
                {
                    DataTable dtTag = MakeTableForDisplay(dtTagPosition, x == 0 ? ChartDisplayType.TagSectionLaneStart : ChartDisplayType.TagSectionLaneEnd);

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
                        // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
                    };
                    chartCoater.Data.Children.Add(ds);
                }
            }
        }

        private void DrawRollMapTagSectionSingle(DataTable dtTagPositionSingle)
        {
            if (CommonVerify.HasTableRow(dtTagPositionSingle))
            {
                DataTable dtTag = MakeTableForDisplay(dtTagPositionSingle, ChartDisplayType.TagSectionSingle);

                XYDataSeries ds = new XYDataSeries();
                ds.ItemsSource = DataTableConverter.Convert(dtTag);
                ds.XValueBinding = new Binding("ADJ_STRT_PSTN");
                ds.ValueBinding = new Binding("SOURCE_Y_PSTN");
                ds.ChartType = ChartType.XYPlot;
                ds.ConnectionFill = new SolidColorBrush(Colors.Transparent);
                ds.SymbolFill = new SolidColorBrush(Colors.Transparent);
                ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
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
                ds.PointLabelTemplate = grdMain.Resources["chartTag"] as DataTemplate;
                ds.Margin = new Thickness(0, 0, 0, 0);

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

            inTable.Columns.Add("CT_MNT_FLAG", typeof(string));  //현재화면 호출.
            inTable.Columns.Add("LAR_REVS_FLAG", typeof(string));  //반전 여부 확인.

            DataRow dr = inTable.NewRow();
            dr["LOTID"] = txtLot.Text;
            dr["WIPSEQ"] = _wipSeq;
            dr["ADJFLAG"] = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";
            inTable.Rows.Add(dr);

            DataTable dtSample = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            // SAMPLE CUT (TEST CUT) 된 정보는 보이지 않도록 하기 위해 길이 정보 보정
            //  - SAMPLE CUT 제외된 롤맵 정보에 SAMPLE CUT 길이 정보가 포함되어 조회되어 강제 보정 처리
            DataRow[] drRows = dtSample.Select();
            foreach (DataRow row in drRows)
            {
                if (row["SMPL_FLAG"] != null && !row["SMPL_FLAG"].ToString().Equals("Y"))
                {
                    row["ADJ_STRT_PSTN"] = row["RAW_STRT_PSTN"];
                    row["ADJ_END_PSTN"] = row["RAW_END_PSTN"];
                    row["SOURCE_STRT_PSTN"] = row["RAW_STRT_PSTN"];
                    row["SOURCE_END_PSTN"] = row["RAW_END_PSTN"];
                }
            }

            //R/P UW 시 코터RW를 홀수면 역방향 짝수면 정방향
            //역방향일 경우 End좌표와 Start좌표 계산. 
            if ("Y".Equals(_dtEioAttr.Rows[0]["LAR_REVS_FLAG"].ToString()))
            {
                DataRow[] rows = dtSample.Select();
                foreach (DataRow row in rows)
                {
                    double ast = row["ADJ_STRT_PSTN"].GetDouble();
                    double aed = row["RAW_END_PSTN"].GetDouble();
                    double sst = row["SOURCE_STRT_PSTN"].GetDouble();
                    double sed = row["SOURCE_END_PSTN"].GetDouble();

                    row["ADJ_STRT_PSTN"] = "".Equals(row["ADJ_STRT_PSTN"].ToString()) ? row["ADJ_STRT_PSTN"] : _xMaxLength - aed;
                    row["ADJ_END_PSTN"] = "".Equals(row["ADJ_END_PSTN"].ToString()) ? row["ADJ_END_PSTN"] : _xMaxLength - ast;
                    row["SOURCE_STRT_PSTN"] = "".Equals(row["SOURCE_STRT_PSTN"].ToString()) ? row["SOURCE_STRT_PSTN"] : _xMaxLength - sed;
                    row["SOURCE_END_PSTN"] = "".Equals(row["SOURCE_END_PSTN"].ToString()) ? row["SOURCE_END_PSTN"] : _xMaxLength - sst;
                }

            }

            string[] petScrap = new string[] { "PET", "SCRAP" };

            /*  --> SAMP 정보는 표현할 필요없음
            DataRow[] drSample = dtSample.Select("SMPL_FLAG = 'Y' AND (EQPT_MEASR_PSTN_ID <> 'PET' AND EQPT_MEASR_PSTN_ID <> 'SCRAP') ");

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
            */
            var query = (from t in dtSample.AsEnumerable() where t.Field<string>("SMPL_FLAG") != "Y" && !petScrap.Contains(t.Field<string>("EQPT_MEASR_PSTN_ID")) select t).ToList();
            if (query.Any())
            {
                //------------ 상하 라인 표시 -------------------------------
                foreach (DataRow row in query)
                {
                    chartCoater.Data.Children.Add(new XYDataSeries()
                    {
                        ChartType = ChartType.Line,
                        XValuesSource = new[] { row["SOURCE_END_PSTN"].GetDouble(), row["SOURCE_END_PSTN"].GetDouble() },
                        ValuesSource = new double[] { 0, 220 },
                        ConnectionStroke = new SolidColorBrush(Colors.DarkRed),
                    });
                }

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
                                       RawEndPosition = t.GetValue("RAW_END_PSTN").GetDecimal(),
                                       RawStartPosition = t.GetValue("RAW_STRT_PSTN").GetDecimal(),
                                       EndPosition = t.GetValue("SOURCE_END_PSTN").GetDecimal(),
                                       RowNum = t.GetValue("ROW_NUM").GetInt()
                                   }).ToList();

                if (queryLength.Any())
                {

                    foreach (var item in queryLength)
                    {
                        DataRow newLength = dtMaxLength.NewRow();

                        newLength["RAW_END_PSTN"] = $"{item.RawEndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_END_PSTN"] = $"{item.EndPosition:###,###,###,##0.##}";
                        newLength["SOURCE_Y_PSTN"] = 220;
                        // item.RowNum => queryLength.Count 로 변경 (Sample Cut 은 안보여 주기로 했으므로 2 이상 나오는 경우가 없다)
                        newLength["TOOLTIP"] = queryLength.Count + " Cut" + "( " + $"{item.RawStartPosition:###,###,###,##0.##}" + "m" + " ~ " + $"{item.RawEndPosition:###,###,###,##0.##}" + "m" + ")";
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
                    chartCoater.Data.Children.Add(ds);
                }

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
                                         YStartPosition = t.GetValue("Y_STRT_PSTN").GetDecimal(),
                                         YEndPosition = t.GetValue("Y_END_PSTN").GetDecimal()

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
                        double? lowerExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yStartPosition : 0 + yStartPosition;
                        double? upperExtent = dt.Rows[i]["SEQ"].GetInt() == 1 ? 120 + yEndPosition : yEndPosition;

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

                            /*
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
                            */
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

                                TextBlock rectangleDescription = Util.CreateTextBlock(item.DefectName,
                                                                                      HorizontalAlignment.Center,
                                                                                      VerticalAlignment.Center,
                                                                                      11,
                                                                                      FontWeights.Bold,
                                                                                      Brushes.Black,
                                                                                      new Thickness(1, 1, 1, 1), // new Thickness(5, 5, 5, 5),
                                                                                      new Thickness(0, 0, 0, 0),
                                                                                      item.MeasurementCode,
                                                                                      Cursors.Hand,
                                                                                      null);
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

                                TextBlock ellipseDescription = Util.CreateTextBlock(item.DefectName,
                                                                                    HorizontalAlignment.Center,
                                                                                    VerticalAlignment.Center,
                                                                                    11,
                                                                                    FontWeights.Bold,
                                                                                    Brushes.Black,
                                                                                    new Thickness(1, 1, 1, 1),  // new Thickness(5, 5, 5, 5),
                                                                                    new Thickness(0, 0, 0, 0),
                                                                                    item.MeasurementCode,
                                                                                    Cursors.Hand,
                                                                                    null);
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
                if (textBlock == null) return;

                ShowLoadingIndicator();
                DoEvents();

                _isResearch = true;
                chartCoater.Data.Children.Clear();

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
                        .Any(rb => rb.Field<string>("EQPT_MEASR_PSTN_ID") == ra.Field<string>("EQPT_MEASR_PSTN_ID") && rb.Field<string>("ABBR_NAME") == ra.Field<string>("ABBR_NAME")));

                    if (queryPoint.Any())
                    {
                        DrawPointChart(queryPoint.CopyToDataTable());
                    }
                }
                DrawRollMapSampleYAxisLine();
                DrawRollMapLane();
                // 역방향 선택 상태에서 Description 선택 시 정방향 조회로 원복되는 현상 수정                
                chartCoater.View.AxisX.Reversed = _selectedViewDirection.Equals(ViewDirection.Reverse) == true ? false : true;
                //chartCoater.View.AxisX.Reversed = true;

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
                DataRow[] newRows = dtLegend.Select();
                bool isTop = false, isBack = false;

                foreach (DataRow row in newRows)
                {
                    string valueText = "VALUE_" + row["VALUE"];

                    if (row["VALUE"].GetString() == "LL" || row["VALUE"].GetString() == "L" || row["VALUE"].GetString() == "SV" || row["VALUE"].GetString() == "H" || row["VALUE"].GetString() == "HH")
                    {
                        DataRow dr = row;
                        dr.ItemArray = row.ItemArray;
                        for (int y = 1; y < 3; y++)
                        {
                            if (dt.AsEnumerable().Any(o => o.GetValue(valueText) != null && o.GetValue(valueText).ToString() != null && o.GetValue("SEQ").GetInt() == y))
                            {
                                double agvValue = 0;
                                var query = (from t in dt.AsEnumerable()
                                             where t.GetValue(valueText) != null && t.GetValue(valueText).ToString() != null && t.GetValue("SEQ").GetInt() == y
                                             select new { Valuecol = t.GetValue(valueText).GetDecimal() }).ToList();
                                if (query.Any())
                                    agvValue = query.Max(r => r.Valuecol).GetDouble();

                                dr["COLORMAP"] = row["COLORMAP"];
                                if (y == 1)
                                {
                                    dr["LEG_DESC"] = dr["LEG_DESC"] + " (TOP : " + agvValue;
                                    isTop = true;
                                }
                                else
                                {
                                    //Back만 존재하고 Top은 없을경우 Top존재 후 Back 존재하는 경우를 구분하여 괄호 표시한다.
                                    dr["LEG_DESC"] = isTop ? dr["LEG_DESC"] + "/BACK : " + agvValue + ")" : dr["LEG_DESC"] + " (BACK : " + agvValue + ")";
                                    isBack = true;
                                }
                            }
                        }
                        //범례표시 탑만 존재하고 Back 은 없을 경우 마지막에 괄호표시한다.
                        if (isTop && !isBack)
                        {
                            dr["LEG_DESC"] = dr["LEG_DESC"] + ")";
                        }
                    }
                }
                _dtLineLegend = dtLegend.Copy();

                GetLegend();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private IEnumerable<double> GetLineValuesSource(DataTable dt, string value, int i)
        {
            try
            {
                var queryCount = _xMaxLength.GetInt() + 1;
                double[] xx = new double[queryCount];

                string valueText = "VALUE_" + value;

                //if (dt.AsEnumerable().Any(p => p.GetValue("SEQ").GetInt() == i && p.GetValue(valueText) != null && p.GetValue(valueText).ToString() != null))
                if (dt.AsEnumerable().Any(p => p.GetValue("SEQ").GetInt() == i && p.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(p.GetValue(valueText).ToString())))
                {
                    double agvValue = 0;
                    var query = (from t in dt.AsEnumerable() where t.GetValue(valueText) != DBNull.Value && !string.IsNullOrEmpty(t.GetValue(valueText).ToString()) && t.GetValue("SEQ").GetInt() == i select new { Valuecol = t.GetValue(valueText).GetDecimal() }).ToList();
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

                //DataSeriesCollection sc = c1Chart.Data.Children;
                //sc.Clear();

                c1Chart.View.AxisX.Reversed = false;

                if (c1Chart.Name == "chartCoater")
                {
                    if (!chartCoater.View.AxisX.Scale.Equals(1))
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
                    // 역방향 조회인 경우
                    if (_selectedViewDirection.Equals(ViewDirection.Reverse) == true)
                    {
                        c1Chart.View.AxisX.Reversed = false;
                    }
                    else
                    {
                        c1Chart.View.AxisX.Reversed = true;
                    }
                }

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

                });

                int laneQty = 0;
                string dfct2DBcrStd = "";
                if (CommonVerify.HasTableRow(_dt2DBarcodeInfo))
                {
                    laneQty = Convert.ToUInt16(_dt2DBarcodeInfo.Rows[0]["LANE_QTY"]);
                    dfct2DBcrStd = _dt2DBarcodeInfo.Rows[0]["DFCT_2D_BCR_STD"].ToString();
                }
                //foreach (DataRow row in dtBinding.Rows)
                foreach (DataRow row in queryDtBinding)
                {
                    // E20240724-000827 TAG_SECTION_SINGLE 위치 조정
                    //if ("TAG_SECTION_SINGLE".Equals(row["EQPT_MEASR_PSTN_ID"])) row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? 220 : -62;
                    //else row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

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
                        //string mark2DBarcode = row["MRK_2D_BCD_STR"].ToString();
                        decimal adjStrtPstn = Convert.ToDecimal(row["ADJ_STRT_PSTN"]);
                        decimal adjEndPstn = Convert.ToDecimal(row["ADJ_END_PSTN"]);
                        List<Tuple<int, decimal, decimal>> mark2DBarcodeCountInfoList = query2DBcrWithCountInfoToDict[mark2DBarcode];

                        mark2DBarcodeCountInfoList.ForEach(t =>
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

                    }
                    else
                    {
                        row["TOOLTIP"] = row["CMCDNAME"].GetString() + " : " + row["DFCT_NAME"].GetString() + "\n" + "[" + $"{Convert.ToDouble(row["SOURCE_END_PSTN"]) - Convert.ToDouble(row["SOURCE_STRT_PSTN"]):###,###,###,##0.##}" + "m" + ", " + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + "]";
                        row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S" : "E";
                    }
                }
            }
            else if (chartDisplayType == ChartDisplayType.TagSectionLaneStart || chartDisplayType == ChartDisplayType.TagSectionLaneEnd)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_PSTN"] = chartDisplayType == ChartDisplayType.TagStart ? -45 : -62;
                    row["TAG"] = $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + ";" + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + ";" + row["CLCT_SEQNO"].GetString() + ";" + row["ROLLMAP_CLCT_TYPE"].GetString();

                    string laneInfo = row["LANE_NO_LIST"].ToString();
                    row["TOOLTIP"] = "(" + laneInfo + ")" + row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAGNAME"] = chartDisplayType == ChartDisplayType.TagStart ? "S(" + laneInfo + ")" : "E(" + laneInfo + ")";
                }
            }

            else if (chartDisplayType == ChartDisplayType.TagSectionSingle)
            {
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAG", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TAGNAME", DataType = typeof(string) });
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "TOOLTIP", DataType = typeof(string) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["SOURCE_Y_PSTN"] = 220;
                    row["TOOLTIP"] = row["CMCDNAME"].GetString() + "[" + $"{row["SOURCE_STRT_PSTN"]:###,###,###,##0.##}" + "m" + " ~ " + $"{row["SOURCE_END_PSTN"]:###,###,###,##0.##}" + "m" + ", " + row["TAG_AUTO_FLAG_NAME"].GetString() + " ]";
                    row["TAGNAME"] = "M";
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

            dtBinding.AcceptChanges();
            return dtBinding;
        }


        private bool ValidationSearch(bool bForceRedraw = false)
        {
            bool isCheck = true;
            const string bizRuleName = "BR_PRD_GET_CT_LOT_MNT_INFO";

            _dtEioAttr?.Clear();

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("ADJFLAG", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = _equipmentCode;
            dr["ADJFLAG"] = rdoP.IsChecked != null && (bool)rdoP.IsChecked ? "1" : "2";
            inTable.Rows.Add(dr);

            DataTable dtEioAttr = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            _dtEioAttr = dtEioAttr.Copy();

            //데이터가 없거나, LOT정보가 없거나, 데이터가 변경이 안되었다면 다시 조회할 필요가 없음.

            // 요청내용 : Unwinder 모니터링 화면 개선사항 추가 개발 (전기영 책임) - (2023.06.08 JEONG KI TONG - For Rollmap Project)
            // UW / RW 표시 Refresh를 위해 새로 조회
            // 조회 방향이 변경되지 않았고 이전 Lot ID와 동일한 Lot을 조회할 경우 Refresh 하지 않음
            if (dtEioAttr.Rows.Count <= 0 || dtEioAttr.Rows[0]["PRE_LOTID"].ToString().IsEmpty()) return false;

            if (bForceRedraw == true)   //무조건 다시 조회일 경우(방향전환, 상대좌표)
            {
                _runLotId = dtEioAttr.Rows[0]["PRE_LOTID"].ToString();
                txtLot.Text = _runLotId;
                _wipSeq = dtEioAttr.Rows[0]["WIPSEQ"].ToString();
                return true;
            }


            if (txtLot.Text.Equals(dtEioAttr.Rows[0]["PRE_LOTID"].ToString())
                && _wipSeq.Equals(dtEioAttr.Rows[0]["WIPSEQ"].ToString()))
            {
                ProcessArrow(dtEioAttr, false);
                EndUpdateChart();
                return false;
            }

            _runLotId = dtEioAttr.Rows[0]["PRE_LOTID"].ToString();
            txtLot.Text = _runLotId;
            _wipSeq = dtEioAttr.Rows[0]["WIPSEQ"].ToString();  //EQPTID와 LOTID 정보를 이용하여 WIPHISTORYATTR에서 WIPSEQ번호를 불러온다.

            return isCheck;
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
                Util.MessageException(ex);
                System.Threading.Thread.Sleep(3000);
                btnClose_Click(btnClose, null);
            }
        }


        private enum ChartDisplayType
        {
            TagStart,
            TagEnd,
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
            TagSectionSingle
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
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("VIEW_TYPE", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = _equipmentCode;
                Indata["INPUT_LOTID"] = txtLot.Text;
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["VIEW_TYPE"] = "F";          // (A:전체 Lane 불량 + 부분 Lane 불량 모두 조회, F: 전체 Lane 불량 조회, L : 부분 Lane 불량 조회)


                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RM_RPT_INPUT_LOT_TAG_SECTION", "INDATA", "OUTDATA", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        //자동 조회 시 마다 진행 상태를 화살표로 표시
        private void ProcessArrow(DataTable dt, bool isNewLot)
        {
            //양극/음극에 따른 GRID내 CHART 표시 (C 양극, A 음극)
            var grdMaterial = _polarityCode == "C" ? chartProcessCathode : chartProcessAnode;
            var chrtMaterial = _polarityCode == "C" ? chartMaterialCathode7 : chartMaterialAnode5;

            //미터, 수직선 잔상제거
            grdMaterial.Data.Children.Clear();

            //chrtMaterial.Data.Children.Clear();
            //chrtMaterial.Data.Children.Clear();
            for (int i = chrtMaterial.Data.Children.Count - 1; i >= 0; i--)
            {
                DataSeries dataSeries = chrtMaterial.Data.Children[i];
                if (dataSeries.Tag.GetString().Equals("dsLine"))
                {
                    chrtMaterial.Data.Children.Remove(dataSeries);
                }
            }




            //신규 LOT이 아니면 chartMaterialCathode7, chartMaterialAnode5 부분 재 설정
            //if (!isNewLot)
            //    DrawMaterialChart(_dtMaterial, _min, _max);


            //데이터 미 존재시 하기 내용 미 수행
            if (dt.Rows.Count <= 0) return;

            dt.Columns.Add("Y_POSITION1", typeof(double));  //화살표 Y축 현 chart에 50point 위에서 표현
            dt.Columns.Add("Y_POSITION2", typeof(double));  //화살표 Y축 현 chart에 50point 위에서 표현
            dt.Columns.Add("START_DESC", typeof(string));    //툴팁 진행정보 표현
            dt.Columns.Add("COLORMAP", typeof(string));     //색상 정보 표현
            dt.Columns.Add("START_LINE", typeof(double));     //진행 정보 라인 표현
            dt.Columns.Add("Y_LINE", typeof(double));     //라인 시작부분 0 으로 고정

            if ("".Equals(dt.Rows[0]["CURR_PSTN"].ToString()))
            {
                dt.Rows[0]["Y_POSITION1"] = 300;
                dt.Rows[0]["Y_POSITION2"] = 0;
                dt.Rows[0]["START_DESC"] = MessageDic.Instance.GetMessage("90026"); // //90026, 설비에 진행중인 LOT 이(가) 존재하지 않습니다.
                dt.Rows[0]["CURR_PSTN"] = _xMaxLength - 100;
            }
            else
            {
                dt.Rows[0]["Y_POSITION1"] = 0;
                dt.Rows[0]["Y_POSITION2"] = 70;
                //dt.Rows[0]["START_DESC"] = dt.Rows[0]["CURR_PSTN"].GetDouble() + "m";
                dt.Rows[0]["START_DESC"] = $"{dt.Rows[0]["CURR_PSTN"].GetDouble():###,###,###,##0.##}" + "m";
                dt.Rows[0]["COLORMAP"] = "#EB0000";
                dt.Rows[0]["START_LINE"] = dt.Rows[0]["CURR_PSTN"].GetDouble();
                dt.Rows[0]["Y_LINE"] = 0;
            }

            /*
            XYDataSeries dsArrow = new XYDataSeries();
            dsArrow.ItemsSource = DataTableConverter.Convert(dt);
            dsArrow.XValueBinding = new Binding("CURR_PSTN");
            dsArrow.ValueBinding = new Binding("Y_POSITION1");
            dsArrow.ChartType = ChartType.XYPlot;
            dsArrow.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsArrow.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsArrow.Cursor = Cursors.Hand;
            dsArrow.PointLabelTemplate = grdMain.Resources["triangleBottom"] as DataTemplate;
            dsArrow.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
                // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
            };
            chartProcessCathode.Data.Children.Add(dsArrow);
            */

            //진행상태 표현
            XYDataSeries dsdesc = new XYDataSeries();
            dsdesc.ItemsSource = DataTableConverter.Convert(dt);
            dsdesc.XValueBinding = new Binding("CURR_PSTN");
            dsdesc.ValueBinding = new Binding("Y_POSITION2");
            dsdesc.ChartType = ChartType.XYPlot;
            dsdesc.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsdesc.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsdesc.Cursor = Cursors.Hand;
            dsdesc.ToolTip = dt.Rows[0]["START_DESC"];
            dsdesc.PointLabelTemplate = grdMain.Resources["proc_desc"] as DataTemplate;
            dsdesc.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
            };
            grdMaterial.Data.Children.Add(dsdesc);

            var dsMarkLabel = new XYDataSeries();
            dsMarkLabel.ItemsSource = DataTableConverter.Convert(dt);
            dsMarkLabel.Tag = "dsLine";
            //dsMarkLabel.XValueBinding = new Binding("SOURCE_STRT_PSTN");
            dsMarkLabel.XValueBinding = new Binding("START_LINE");
            dsMarkLabel.ValueBinding = new Binding("Y_LINE");
            dsMarkLabel.ChartType = ChartType.XYPlot;
            dsMarkLabel.ConnectionFill = new SolidColorBrush(Colors.Transparent);
            dsMarkLabel.SymbolFill = new SolidColorBrush(Colors.Transparent);
            dsMarkLabel.PointLabelTemplate = grdMain.Resources["chartLine"] as DataTemplate;
            dsMarkLabel.PlotElementLoaded += (s, e) =>
            {
                PlotElement pe = (PlotElement)s;
                pe.Stroke = new SolidColorBrush(Colors.Transparent);
                pe.Fill = new SolidColorBrush(Colors.Transparent);
                // PlotElement 컬러가 Transparent 인경우 ToolTip 이 보이지 않는 현상으로 DataTemplate을 사용하여 반영 처리 함.
            };
            chrtMaterial.Data.Children.Add(dsMarkLabel); //원재료 마지막 부분에서 그려지기

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
            // 정방향/역방향 선택 정보 저장
            _oldViewDirection = _selectedViewDirection;
            _selectedViewDirection = ViewDirectionValue;

            doTimeStart(true);
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