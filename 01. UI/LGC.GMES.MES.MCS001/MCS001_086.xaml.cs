/*************************************************************************************
 Created Date : 2022.09.15
      Creator : 오화백
   Decription : 공정설비 반송요청 진행현황(조립)
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.15  오화백 : Initial Created.    
  2023.03.08  신광희 : 공정설비 반송요청 진행현황 Resize 시 조회 영역 컨트롤 보여지지 않는 부분 개선
  2023.06.07  오화백 : Carrier ID 마지막 위치, 마지막 위치명 컬럼 추가
  2023.09.11  주동석 : 와인더 탭 추가 
  2024.03.19  배현우 : Winder 및 Assy 설비 조회되도록 비즈 변경및 오류 수정
  2024.05.31  배현우 : 컬럼 사용자 설정 기능 추가 및 조회 컬럼 사이즈 고정 (스크롤시 컬럼 사이즈 변경되어 고정요청, 대표랏 사용동) 
  2024.12.19  오화백 : NFF WMS 전환으로 NFF 일경우 설비 컬럼 숨김처리, 원자재 포트 정보 나오도록 처리(BIZ)
  2025.04.19  오화백 : 라미공정일 경우 극성 정보를 조회조건에서 빠지도록 처리
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Threading;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_037.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_086 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        private readonly DispatcherTimer _monitorTimer_NND = new DispatcherTimer();
        private bool _isSelectedAutoTime_NND = false;
     
        private readonly DispatcherTimer _monitorTimer_STK = new DispatcherTimer();
        private bool _isSelectedAutoTime_STK = false;

        private readonly DispatcherTimer _monitorTimer_WND = new DispatcherTimer();
        private bool _isSelectedAutoTime_WND = false;

        private bool _isLoaded = false;
        private bool RepLotUseArea = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
    
        public MCS001_086()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
        private void InitializeControl()
        {
            Tab_NND_LAMI.IsSelected = true;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            InitializeControl();
            TimerSetting_NND();
            TimerSetting_STK();
            TimerSetting_WND();

            TabVisibility();
            Columns_Hide();

            Loaded -= UserControl_Loaded;
            _isLoaded = true;

            GetRepLotUseArea();
            if (RepLotUseArea)
                ColumnSizeInit();
        }

        /// <summary>
        /// 각 탭 보기
        /// </summary>
        private void TabVisibility()
        {
            Tab_STK_PKG.Visibility = Visibility.Collapsed;

            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "REP_LOT_USE_AREA";
            row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

            RQSTDT1.Rows.Add(row);

            DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

            if (dtCommon != null)
            {
                if (dtCommon.Rows.Count > 0)
                {
                    Tab_NND_LAMI.Visibility = Visibility.Collapsed;
                    Tab_WND.Visibility = Visibility.Visible;
                }
                else
                {
                    Tab_NND_LAMI.Visibility = Visibility.Visible;
                    Tab_WND.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();

            // 라인 콥보박스
            SetEquipmentSegmentCombo(cboEquipmentSegment);
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;

            // 극성 콤보박스
            SetElectrodeTypeCombo_NND(cboElectrodeType);

            // 극성 콤보박스
            SetElectrodeTypeCombo_WND(cboElectrodeTypeWA);

            //NND 공정 콤보
            SetProcess_NND(cboProcess_NND);
            cboProcess_NND.SelectedItemChanged += cboProcess_NND_SelectedItemChanged;

            //STK 공정 콤보
            SetProcess_STK(cboProcess_STK);
            cboProcess_STK.SelectedItemChanged += cboProcess_STK_SelectedItemChanged;

            //WND 공정 콤보
            SetProcess_WND(cboProcess_WND);
            cboProcess_WND.SelectedItemChanged += cboProcess_WND_SelectedItemChanged;

        }
        #endregion

        #region Event

        #region NND/LAMI 조회 버튼 : btnSearch_NND_Click()

        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_NND_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch_NND())
                return;

            GetDataList_NND();
        }
        #endregion

        #region  NND/LAMI 공정 콤보 이벤트  : cboProcess_NND_SelectedItemChanged()
        /// <summary>
        /// 공정 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboProcess_NND_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess_NND.Items.Count > 0 && cboProcess_NND.SelectedValue != null)
            {

                if(cboProcess_NND.SelectedValue.ToString() == "A7000")
                {
                    elecType.Visibility = Visibility.Collapsed;
                    grdSearchRow.ColumnDefinitions[3].Width = new GridLength(0);
                }
                else
                {
                    elecType.Visibility = Visibility.Visible;
                    grdSearchRow.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
                }

                SetEquipment_NND();
            }
        }


        #endregion

        #region  NND/LAMI 극성 콤보 이벤트 : cboElectrodeType_SelectedValueChanged()
        /// <summary>
        /// 극성 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl_NND();
            SetEquipment_NND();
        }

        #endregion

        #region NND/LAMI Timer 콤보 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// Timer 콤보 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_NND_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer_NND != null)
                {
                    _monitorTimer_NND.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer_NND?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer_NND.SelectedValue.ToString());
                        _isSelectedAutoTime_NND = true;
                    }
                    else
                    {
                        _isSelectedAutoTime_NND = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime_NND)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer_NND.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer_NND.Start();

                    if (_isSelectedAutoTime_NND)
                    {
                        if (cboTimer_NND != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer_NND.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region NND/LAMI Timer에 따라 조회  : _dispatcherTimer_Tick()
        /// <summary>
        /// Timer에 따라 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_NND_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            if (Tab_NND_LAMI.IsSelected == false) return;
            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_NND_Click(null, null);
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
        #endregion

        #region NND/LAMI 조회 내용 색표시 : dgList_NND_LoadedCellPresenter(), dgList_NND_UnloadedCellPresenter()

        private void dgList_NND_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "PORT_STAT_CODE")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "LR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "UR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "PL"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "-"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                        }
                    }

                    if (Convert.ToString(e.Cell.Column.Name) == "TIME_OF_REQ")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_DISP_COLOR")), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_DISP_COLOR")), "R"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (Convert.ToString(e.Cell.Column.Name) == "TIME_OF_TRF")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_DISP_COLOR")), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_DISP_COLOR")), "R"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                }
            }));
        }

        private void dgList_NND_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        #endregion

        #region NND/LAMI 모니터링 리스트 화면 머지  : dgList_NND_MergingCells()
        /// <summary>
        /// LIST 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_NND_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgList_NND.TopRows.Count; i < dgList_NND.Rows.Count; i++)
                {

                    if (dgList_NND.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList_NND.Rows[i].DataItem, "EQPTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList_NND.Rows[i].DataItem, "EQPTID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgList_NND.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList_NND.GetCell(idxS, dgList_NND.Columns["EQPTNAME"].Index), dgList_NND.GetCell(idxE, dgList_NND.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_NND.GetCell(idxS, dgList_NND.Columns["EIOIFMODE_NAME"].Index), dgList_NND.GetCell(idxE, dgList_NND.Columns["EIOIFMODE_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_NND.GetCell(idxS, dgList_NND.Columns["EIOSTNAME"].Index), dgList_NND.GetCell(idxE, dgList_NND.Columns["EIOSTNAME"].Index)));
                           


                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList_NND.GetCell(idxS, dgList_NND.Columns["EQPTNAME"].Index), dgList_NND.GetCell(idxE, dgList_NND.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_NND.GetCell(idxS, dgList_NND.Columns["EIOIFMODE_NAME"].Index), dgList_NND.GetCell(idxE, dgList_NND.Columns["EIOIFMODE_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_NND.GetCell(idxS, dgList_NND.Columns["EIOSTNAME"].Index), dgList_NND.GetCell(idxE, dgList_NND.Columns["EIOSTNAME"].Index)));
                             

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList_NND.Rows[i].DataItem, "EQPTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //STK/PKG TAB
        #region STK/PKG 조회 버튼 : btnSearch_STK_Click()

        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_STK_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch_STK())
                return;

            GetDataList_PKG();
        }
        #endregion
       
        #region  STK/PKG 공정 콤보 이벤트  : cboProcess_STK_SelectedItemChanged()
        /// <summary>
        /// 공정 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboProcess_STK_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess_STK.Items.Count > 0 && cboProcess_STK.SelectedValue != null)
            {
                SetEquipment_STK();
            }
        }


        #endregion

        #region STK/PKG Timer 콤보 이벤트 : cboTimer_STK_SelectedValueChanged()
        /// <summary>
        /// Timer 콤보 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_STK_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer_STK != null)
                {
                    _monitorTimer_STK.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer_STK?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer_STK.SelectedValue.ToString());
                        _isSelectedAutoTime_STK = true;
                    }
                    else
                    {
                        _isSelectedAutoTime_STK = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime_STK)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer_STK.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer_STK.Start();

                    if (_isSelectedAutoTime_STK)
                    {
                        if (cboTimer_STK != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer_STK.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region STK/PKG Timer에 따라 조회  : _dispatcherTimer_STK_Tick()
        /// <summary>
        /// Timer에 따라 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_STK_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            if (Tab_STK_PKG.IsSelected == false) return;
            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_STK_Click(null, null);
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
        #endregion

        #region STK/PKG 모니터링 리스트 화면 머지  : dgList_STK_MergingCells()
        /// <summary>
        /// LIST 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_STK_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgList_STK.TopRows.Count; i < dgList_STK.Rows.Count; i++)
                {

                    if (dgList_STK.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList_STK.Rows[i].DataItem, "EQPTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList_STK.Rows[i].DataItem, "EQPTID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgList_STK.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList_STK.GetCell(idxS, dgList_STK.Columns["EQPTNAME"].Index), dgList_STK.GetCell(idxE, dgList_STK.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_STK.GetCell(idxS, dgList_STK.Columns["EIOIFMODE_NAME"].Index), dgList_STK.GetCell(idxE, dgList_STK.Columns["EIOIFMODE_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_STK.GetCell(idxS, dgList_STK.Columns["EIOSTNAME"].Index), dgList_STK.GetCell(idxE, dgList_STK.Columns["EIOSTNAME"].Index)));



                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList_STK.GetCell(idxS, dgList_STK.Columns["EQPTNAME"].Index), dgList_STK.GetCell(idxE, dgList_STK.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_STK.GetCell(idxS, dgList_STK.Columns["EIOIFMODE_NAME"].Index), dgList_STK.GetCell(idxE, dgList_STK.Columns["EIOIFMODE_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_STK.GetCell(idxS, dgList_STK.Columns["EIOSTNAME"].Index), dgList_STK.GetCell(idxE, dgList_STK.Columns["EIOSTNAME"].Index)));


                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList_NND.Rows[i].DataItem, "EQPTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region NND/LAMI 조회 버튼 : btnSearch_NND_Click()

        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_WND_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch_WND())
                return;

            GetDataList_WND();
        }
        #endregion

        #region  WND 공정 콤보 이벤트  : cboProcess_WND_SelectedItemChanged()
        /// <summary>
        /// 공정 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboProcess_WND_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess_WND.Items.Count > 0 && cboProcess_WND.SelectedValue != null)
            {
                if (cboProcess_WND.SelectedValue.ToString() == "A2000")
                {
                    elecType.Visibility = Visibility.Collapsed;
                    grdSearchRow.ColumnDefinitions[3].Width = new GridLength(0);
                    dgList_WND.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("대표 LOTID");
                }
                else
                {
                    elecType.Visibility = Visibility.Collapsed;
                    grdSearchRow.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
                    dgList_WND.Columns["CSTID"].Header = ObjectDic.Instance.GetObjectName("Carrier ID");
                }

                SetEquipment_WND();
            }
        }

        #region 라인 조회 : SetEquipmentSegmentCombo()
        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            string sBizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string sSelectedValueText = "CBO_CODE";
            string sDisplayMemberText = "CBO_NAME";

            // 2023.07.21  강성묵 : 라인 ALL 조건 추가
            CommonCombo.CommonBaseCombo(sBizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, sSelectedValueText, sDisplayMemberText, LoginInfo.CFG_EQSG_ID);
        }
        #endregion

        #region cboEquipmentSegment_SelectedValueChanged()
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 2023.07.21  강성묵 : 라인 ALL 조건 추가
            //if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null)
            //{
            SetProcess_WND(cboProcess_WND);
            //}
        }
        #endregion


        #endregion

        #region  WND 극성 콤보 이벤트 : cboElectrodeTypeWA_SelectedValueChanged()
        /// <summary>
        /// 극성 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboElectrodeTypeWA_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl_WND();
            SetEquipment_WND();
        }

        #endregion

        #region WND Timer 콤보 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// Timer 콤보 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_WND_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer_WND != null)
                {
                    _monitorTimer_WND.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer_WND?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer_WND.SelectedValue.ToString());
                        _isSelectedAutoTime_WND = true;
                    }
                    else
                    {
                        _isSelectedAutoTime_WND = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime_WND)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer_WND.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer_WND.Start();

                    if (_isSelectedAutoTime_WND)
                    {
                        if (cboTimer_WND != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer_WND.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region WND Timer에 따라 조회  : _dispatcherTimer_Tick()
        /// <summary>
        /// Timer에 따라 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_WND_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            if (Tab_WND.IsSelected == false) return;
            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_WND_Click(null, null);
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
        #endregion

        #region WND 조회 내용 색표시 : dgList_WND_LoadedCellPresenter(), dgList_WND_UnloadedCellPresenter()

        private void dgList_WND_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "PORT_STAT_CODE")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "LR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "UR"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "PL"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PORT_STAT_CODE")), "-"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);

                        }
                    }

                    if (Convert.ToString(e.Cell.Column.Name) == "TIME_OF_REQ")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_DISP_COLOR")), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_DISP_COLOR")), "R"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (Convert.ToString(e.Cell.Column.Name) == "TIME_OF_TRF")
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_DISP_COLOR")), "Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;

                        }
                        else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_DISP_COLOR")), "R"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                }
            }));
        }

        private void dgList_WND_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        #endregion

        #region WND 모니터링 리스트 화면 머지  : dgList_WND_MergingCells()
        /// <summary>
        /// LIST 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_WND_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgList_WND.TopRows.Count; i < dgList_WND.Rows.Count; i++)
                {

                    if (dgList_WND.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList_WND.Rows[i].DataItem, "EQPTID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList_WND.Rows[i].DataItem, "EQPTID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgList_WND.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["EQPTNAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["EIOIFMODE_NAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["EIOIFMODE_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["EIOSTNAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["EIOSTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["PRJT_NAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["PRJT_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["DEMAND_NAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["DEMAND_NAME"].Index)));


                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["EQPTNAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["EIOIFMODE_NAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["EIOIFMODE_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["EIOSTNAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["EIOSTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["PRJT_NAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["PRJT_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList_WND.GetCell(idxS, dgList_WND.Columns["DEMAND_NAME"].Index), dgList_WND.GetCell(idxE, dgList_WND.Columns["DEMAND_NAME"].Index)));


                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList_WND.Rows[i].DataItem, "EQPTID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Method

        #region NND/LAMI 극성 조회 : SetElectrodeTypeCombo_NND()
        /// <summary>
        ///  NND/LAMI 극성 참조 조회
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetElectrodeTypeCombo_NND(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region NND/LAMI 공정 조회 : SetProcess_NND()
        /// <summary>
        /// NND/LAMI 공정
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetProcess_NND(C1ComboBox cbo)
        {

            CommonCombo _combo = new CommonCombo();
            string[] sFilter1 = { "DDA_PROCESS", LoginInfo.CFG_SYSTEM_TYPE_CODE, "NL" };
            _combo.SetCombo(cbo, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODEATTRS");

        }

        #endregion

        #region NND/LAMI 설비 조회 : SetEquipment_NND()
        /// <summary>
        /// NND/LAMI 설비
        /// </summary>
        private void SetEquipment_NND()
        {
            try
            {
                string processCode = Util.GetCondition(cboProcess_NND);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment_NND.ItemsSource = null;
                    return;
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if(cboProcess_NND.SelectedValue.ToString() == "A5000")
                {
                    dr["EQGRID"] = "NND";
                    dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue == null ? null : cboElectrodeType.SelectedValue.ToString();
                }
                else
                {
                    dr["EQGRID"] = "LAM";
                    dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue = null;
                }
               

              
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_NND_LAMI_LOADER_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count != 0)
                {
                    cboEquipment_NND.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cboEquipment_NND.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboEquipment_NND.Check(-1);
                    }
                    else
                    {
                        cboEquipment_NND.isAllUsed = true;
                        cboEquipment_NND.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            cboEquipment_NND.Check(i);
                        }
                    }
                }
                else
                {
                    cboEquipment_NND.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region NND/LAMI 모니터링 데이터 조회 : GetDataList()
        /// <summary>
        ///  모니터링 데이터 조회
        /// </summary>
        private void GetDataList_NND()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LIST_BY_NND_LAM";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment_NND.SelectedItemsToString;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {

                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList_NND, bizResult, null, false);

                    if (bizResult.Rows.Count < 11)
                    {
                        dgList_NND.RowHeight = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                        dgList_NND.FontSize = 14;
                    }
                    else if (bizResult.Rows.Count > 6 && bizResult.Rows.Count < 15)
                    {
                        dgList_NND.RowHeight = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                        dgList_NND.FontSize = 13;
                    }
                    else
                    {
                        dgList_NND.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                        dgList_NND.FontSize = 12;
                    }

                    dgList_NND.MergingCells -= dgList_NND_MergingCells;
                    dgList_NND.MergingCells += dgList_NND_MergingCells;

                });



            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region NND/LAMI 초기화  : ClearControl_NND()

        private void ClearControl_NND()
        {
            Util.gridClear(dgList_NND);
        }

        #endregion

        #region NND/LAM_Timer 셋팅 : TimerSetting_NND()
        private void TimerSetting_NND()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_DDA" };
            combo.SetCombo(cboTimer_NND, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer_NND != null && cboTimer_NND.Items.Count > 0)
                cboTimer_NND.SelectedIndex = 3;

            if (_monitorTimer_NND != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer_NND?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer_NND.SelectedValue.ToString());

                _monitorTimer_NND.Tick += _dispatcherTimer_NND_Tick;
                _monitorTimer_NND.Interval = new TimeSpan(0, 0, second);
            }
        }


        #endregion

        #region NND/LAMI 조회 Validation : ValidationSearch_NND()

        private bool ValidationSearch_NND()
        {
            if (cboProcess_NND.SelectedValue == null || cboProcess_NND.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }
            return true;
        }

        #endregion


        //STK/PKG TAB
        #region STK/PKG 공정 조회 : SetProcess_STK()
        /// <summary>
        /// NND/LAMI 공정
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetProcess_STK(C1ComboBox cbo)
        {

            CommonCombo _combo = new CommonCombo();
            string[] sFilter1 = { "DDA_PROCESS", LoginInfo.CFG_SYSTEM_TYPE_CODE, "SP" };
            _combo.SetCombo(cbo, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODEATTRS");

        }

        #endregion
        
        #region STK/PKG 설비 조회 : SetEquipment_STK()
        /// <summary>
        /// STK/PKG 설비
        /// </summary>
        private void SetEquipment_STK()
        {
            try
            {
                string processCode = Util.GetCondition(cboProcess_STK);
                if (string.IsNullOrWhiteSpace(processCode))
                {
                    cboEquipment_STK.ItemsSource = null;
                    return;
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = cboProcess_STK.SelectedValue.ToString();
                dr["ELTR_TYPE_CODE"] = null;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_ELEC_ASSY_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count != 0)
                {
                    cboEquipment_STK.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cboEquipment_STK.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboEquipment_STK.Check(-1);
                    }
                    else
                    {
                        cboEquipment_STK.isAllUsed = true;
                        cboEquipment_STK.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            cboEquipment_STK.Check(i);
                        }
                    }
                }
                else
                {
                    cboEquipment_STK.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STK/PKG_Timer 셋팅 : TimerSetting_STK()
        private void TimerSetting_STK()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_DDA" };
            combo.SetCombo(cboTimer_STK, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer_STK != null && cboTimer_STK.Items.Count > 0)
                cboTimer_STK.SelectedIndex = 3;

            if (_monitorTimer_STK != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer_STK?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer_STK.SelectedValue.ToString());

                _monitorTimer_STK.Tick += _dispatcherTimer_STK_Tick;
                _monitorTimer_STK.Interval = new TimeSpan(0, 0, second);
            }
        }


        #endregion

        #region STK/PKG 조회 Validation : ValidationSearch_STK()

        private bool ValidationSearch_STK()
        {
            if (cboProcess_STK.SelectedValue == null || cboProcess_STK.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }
            return true;
        }

        #endregion

        #region STK/PKG 모니터링 데이터 조회 : GetDataList_PKG()
        /// <summary>
        ///  모니터링 데이터 조회
        /// </summary>
        private void GetDataList_PKG()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LIST_BY_NND_LAM";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment_STK.SelectedItemsToString;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {

                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList_STK, bizResult, null, false);

                    if (bizResult.Rows.Count < 11)
                    {
                        dgList_STK.RowHeight = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                        dgList_STK.FontSize = 14;
                    }
                    else if (bizResult.Rows.Count > 6 && bizResult.Rows.Count < 15)
                    {
                        dgList_STK.RowHeight = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                        dgList_STK.FontSize = 13;
                    }
                    else
                    {
                        dgList_STK.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                        dgList_STK.FontSize = 12;
                    }

                    dgList_STK.MergingCells -= dgList_STK_MergingCells;
                    dgList_STK.MergingCells += dgList_STK_MergingCells;

                });



            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region WND 극성 조회 : SetElectrodeTypeCombo_WND()
        /// <summary>
        ///  WND 극성 참조 조회
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetElectrodeTypeCombo_WND(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region WND 공정 조회 : SetProcess_WND()
        /// <summary>
        /// WND 공정
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetProcess_WND(C1ComboBox cbo)
        {

            CommonCombo _combo = new CommonCombo();
            string[] sFilter1 = { "DDA_PROCESS", LoginInfo.CFG_SYSTEM_TYPE_CODE, "WA" };
            _combo.SetCombo(cbo, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODEATTRS");

        }

        #endregion

        #region WND 설비 조회 : SetEquipment_WND()
        /// <summary>
        /// WND 설비
        /// </summary>
        private void SetEquipment_WND()
        {
            try
            {
                DataTable dtRQST = new DataTable();
                dtRQST.TableName = "RQSTDT";
                dtRQST.Columns.Add("LANGID", typeof(string));
                dtRQST.Columns.Add("AREAID", typeof(string));
                dtRQST.Columns.Add("EQSGID", typeof(string));
                dtRQST.Columns.Add("PROCID", typeof(string));
                dtRQST.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drRQST = dtRQST.NewRow();
                drRQST["LANGID"] = LoginInfo.LANGID;
                drRQST["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (cboEquipmentSegment.SelectedValue != null)
                {
                    drRQST["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                }

                if (cboProcess_WND.SelectedValue != null)
                {
                    drRQST["PROCID"] = cboProcess_WND.SelectedValue.ToString();
                }

                if (cboElectrodeType.SelectedValue != null)
                {
                    drRQST["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue.ToString();
                }

                dtRQST.Rows.Add(drRQST);

                string sBizRuleName = "";

                // 조립동 Machine 조회
                sBizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_ASSY_CBO";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizRuleName, "RQSTDT", "RSLTDT", dtRQST);
                if (dtResult.Rows.Count != 0)
                {
                    cboEquipment_WND.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cboEquipment_WND.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboEquipment_WND.Check(-1);
                    }
                    else
                    {
                        cboEquipment_WND.isAllUsed = true;
                        cboEquipment_WND.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            cboEquipment_WND.Check(i);
                        }
                    }
                }
                else
                {
                    cboEquipment_WND.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region WND 모니터링 데이터 조회 : GetDataList()
        /// <summary>
        ///  모니터링 데이터 조회
        /// </summary>
        private void GetDataList_WND()
        {
            ShowLoadingIndicator();
            // string bizRuleName = cboProcess_WND.SelectedValue.GetString() == "A3000" ? "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LIST_BY_ELEC_ASSY" : "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LIST_BY_ELEC_WND";
            string bizRuleName = "DA_MHS_SEL_PROCEQPT_TRANSFER_REQ_LIST_BY_ELEC_ASSY";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment_WND.SelectedItemsToString;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {

                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList_WND, bizResult, null, false);

                    if (bizResult.Rows.Count < 11)
                    {
                        dgList_WND.RowHeight = new C1.WPF.DataGrid.DataGridLength(70, DataGridUnitType.Pixel);
                        dgList_WND.FontSize = 14;
                    }
                    else if (bizResult.Rows.Count > 6 && bizResult.Rows.Count < 15)
                    {
                        dgList_WND.RowHeight = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                        dgList_WND.FontSize = 13;
                    }
                    else
                    {
                        dgList_WND.RowHeight = new C1.WPF.DataGrid.DataGridLength(30, DataGridUnitType.Pixel);
                        dgList_WND.FontSize = 12;
                    }

                    dgList_WND.MergingCells -= dgList_WND_MergingCells;
                    dgList_WND.MergingCells += dgList_WND_MergingCells;

                });



            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region WND 초기화  : ClearControl_WND()

        private void ClearControl_WND()
        {
            Util.gridClear(dgList_WND);
        }

        #endregion

        #region WND_Timer 셋팅 : TimerSetting_WND()
        private void TimerSetting_WND()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_DDA" };
            combo.SetCombo(cboTimer_WND, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer_WND != null && cboTimer_WND.Items.Count > 0)
                cboTimer_WND.SelectedIndex = 3;

            if (_monitorTimer_WND != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer_WND?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer_WND.SelectedValue.ToString());

                _monitorTimer_WND.Tick += _dispatcherTimer_WND_Tick;
                _monitorTimer_WND.Interval = new TimeSpan(0, 0, second);
            }
        }


        #endregion

        #region WND 조회 Validation : ValidationSearch_WND()

        private bool ValidationSearch_WND()
        {
            if (cboProcess_WND.SelectedValue == null || cboProcess_WND.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }
            return true;
        }

        #endregion


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

        private void ColumnSizeInit()
        {
            //NFF 양산라인의 경우 컬럼 사이즈 고정 요청 (스크롤시 컬럼크기가 변경되어 사이즈 고정)
            dgList_WND.Columns["EQPTID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["EQPTNAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["PRJT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList_WND.Columns["DEMAND_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList_WND.Columns["EIOIFMODE_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList_WND.Columns["EIOSTNAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList_WND.Columns["EQP_DIRCTN"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["PORT_ID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["PORT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["PORT_STAT_CODE"].Width = new C1.WPF.DataGrid.DataGridLength(100);
            dgList_WND.Columns["TRF_PROC_STATUS"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["CSTID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["CSTSTAT"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["CURR_LOCID"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["CURR_LOC_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["CURR_LOTID"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["LOT_DIRCTN"].Width = new C1.WPF.DataGrid.DataGridLength(150);
            dgList_WND.Columns["EMPTY_RSLT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(300);
            dgList_WND.Columns["TIME_OF_TRF"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["TIME_OF_REQ"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["REQ_DTTM"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["CMD_CR_DTTM"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["STO_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["STO_EIOSTNAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);
            dgList_WND.Columns["STO_EIOIFMODE_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(200);


        }

        private void GetRepLotUseArea()
        {
            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT1.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = RQSTDT1.NewRow();
            row["LANGID"] = LoginInfo.USERID;
            row["CMCDTYPE"] = "REP_LOT_USE_AREA";
            row["CBO_CODE"] = LoginInfo.CFG_AREA_ID;

            RQSTDT1.Rows.Add(row);

            DataTable dtCommon = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT1);

            if (dtCommon != null)
            {
                if (dtCommon.Rows.Count > 0)
                {
                    RepLotUseArea = true;
                }
                else
                {
                    RepLotUseArea = false;
                }
            }
        }

        private void Columns_Hide()
        {
            try
            {
             

                if (LoginInfo.CFG_AREA_ID == "MC")
                {
                    dgList_WND.Columns["PRJT_NAME"].Visibility = Visibility.Collapsed;
                    dgList_WND.Columns["DEMAND_TYPE"].Visibility = Visibility.Collapsed;
                    dgList_WND.Columns["DEMAND_NAME"].Visibility = Visibility.Collapsed;
                    dgList_WND.Columns["EIOIFMODE"].Visibility = Visibility.Collapsed;
                    dgList_WND.Columns["EIOIFMODE_NAME"].Visibility = Visibility.Collapsed;
                    dgList_WND.Columns["EIOSTAT"].Visibility = Visibility.Collapsed;
                    dgList_WND.Columns["EIOSTNAME"].Visibility = Visibility.Collapsed;
                    dgList_WND.Columns["EQP_DIRCTN"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgList_WND.Columns["PRJT_NAME"].Visibility = Visibility.Visible;
                    dgList_WND.Columns["DEMAND_TYPE"].Visibility = Visibility.Visible;
                    dgList_WND.Columns["DEMAND_NAME"].Visibility = Visibility.Visible;
                    dgList_WND.Columns["EIOIFMODE"].Visibility = Visibility.Visible;
                    dgList_WND.Columns["EIOIFMODE_NAME"].Visibility = Visibility.Visible;
                    dgList_WND.Columns["EIOSTAT"].Visibility = Visibility.Visible;
                    dgList_WND.Columns["EIOSTNAME"].Visibility = Visibility.Visible;
                    dgList_WND.Columns["EQP_DIRCTN"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


    }
}
