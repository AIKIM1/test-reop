/*************************************************************************************
 Created Date : 2021.03.12
      Creator : 오화백
   Decription : 고공 CNV 현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.12  오화백      : Initial Created.    
  
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_060.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_060 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //private readonly 
        private readonly Util _util = new Util();
        private string _selectedEquipmentCode;
     
    
        private string _selectedProjectName;
        private string _selectedWipHold;
        private string _selectedQmsHold;
        private string _selectedLotIdByRackInfo;
        private string _selectedSkIdIdByRackInfo;
        private string _selectedBobbinIdByRackInfo;
        private DataTable _dtWareHouseCapacity;

        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;
  
        private int _maxRowCount;
        private int _maxColumnCount;

        private DataTable _dtRackInfo;
        private UcRackLayout[][] _ucRackLayout1;
        private UcRackLayout[][] _ucRackLayout2;


        //private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        private string _selectedEquipmentSegmentCode;
        private string _selectedCstTypeCode;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;


        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;


        private enum SearchType
        {
            Tab
        }

        public MCS001_060()
        {
            InitializeComponent();
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeGrid();
            InitializeCombo();

            #region LayOut 관련 : 사용안함
            MakeRackInfoTable();
            MakeWareHouseCapacityTable();
            #endregion
            TimerSetting();
            Loaded -= UserControl_Loaded;
            C1TabControl.SelectionChanged += C1TabControl_SelectionChanged;
            _isLoaded = true;
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_isscrollToHorizontalOffset = true;
            //_scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
        }


        #endregion

        #region Event

        #region  동정보 선택시 초기화  : cboArea_SelectedValueChanged()
        /// <summary>
        /// 동정보 선택시 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }



        #endregion

        #region 조회버튼 클릭 : btnSearch_Click()
        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();


            //CNV 현황
            SelectConveyorSummary();
        
        }

        #endregion

        #region 출고예약 버튼 클릭 : btnManualIssue_Click()
        /// <summary>
        /// 출고 예약
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            // 실 Tray  선택
            if (tabProduct.IsSelected == true)
            {
                MCS001_060_USING_TRAY_OUTPUT popupUsingTray = new MCS001_060_USING_TRAY_OUTPUT { FrameOperation = FrameOperation };
                object[] parameters = new object[1];
                parameters[0] = _selectedEquipmentCode;


                C1WindowExtension.SetParameters(popupUsingTray, parameters);

                popupUsingTray.Closed += ppopupUsingTray_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupUsingTray.ShowModal()));
            }
            //공 Tray 
            else if (tabEmptyCarrier.IsSelected == true)
            {
                if (!ValidationEmptyTray()) return;

                MCS001_060_EMPTY_ERROR_TRAY_OUTPUT popupEmptyTray = new MCS001_060_EMPTY_ERROR_TRAY_OUTPUT { FrameOperation = FrameOperation };

                if (popupEmptyTray != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("RACK", typeof(string));
                    dtData.Columns.Add("CSTID", typeof(string));


                    foreach (C1.WPF.DataGrid.DataGridRow row in dgEmptyCarrier.Rows)
                    {
                        if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                        {
                            DataRow newRow = dtData.NewRow();
                            newRow["RACK"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                            newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                            dtData.Rows.Add(newRow);
                        }
                    }
                    object[] parameters = new object[2];
                    parameters[0] = dtData;
                    parameters[1] = _selectedEquipmentCode;
                    C1WindowExtension.SetParameters(popupEmptyTray, parameters);

                    popupEmptyTray.Closed += popupEmptyTray_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupEmptyTray.ShowModal()));
                }



            }
            //오류 Tray 선택
            else if (tabErrorCarrier.IsSelected == true)
            {
                if (!ValidationErrorTray()) return;


                MCS001_060_EMPTY_ERROR_TRAY_OUTPUT popupErrorTray = new MCS001_060_EMPTY_ERROR_TRAY_OUTPUT { FrameOperation = FrameOperation };

                if (popupErrorTray != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("RACK", typeof(string));
                    dtData.Columns.Add("CSTID", typeof(string));

                    foreach (C1.WPF.DataGrid.DataGridRow row in dgErrorCarrier.Rows)
                    {
                        if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True"))
                        {
                            DataRow newRow = dtData.NewRow();
                            newRow["RACK"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                            newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CSTID").GetString();
                            dtData.Rows.Add(newRow);
                        }
                    }
                    object[] parameters = new object[2];
                    parameters[0] = dtData;
                    parameters[1] = _selectedEquipmentCode;
                    C1WindowExtension.SetParameters(popupErrorTray, parameters);

                    popupErrorTray.Closed += popupErrorTray_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupErrorTray.ShowModal()));
                }
            }
        }

        #endregion

        #region 자동조회 Timer 관련 이벤트 : cboTimer_SelectedValueChanged(), _dispatcherTimer_Tick()

        /// <summary>
        /// 타이머 콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(null, null);
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
    

        //=============================== CNV 현황 관련 ==================================================

        #region CNV 현황 색깔 처리 : dgConveyor_LoadedCellPresenter()
        /// <summary>
        /// CNV 현황 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void dgConveyor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGID_NM")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                    if (cboLevel.SelectedIndex == 2 && (Convert.ToString(e.Cell.Column.Name) == "T_CNT_TEXT" || Convert.ToString(e.Cell.Column.Name) == "E_CNT_TEXT"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);

                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOTAL_RATIO") != null && DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOTAL_RATIO").ToString().Replace("%", "").Trim().GetDouble() >= 95)
                        {
                            if (Convert.ToString(e.Cell.Column.Name) == "T_CNT_TEXT")
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }
                    }
                    else
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOTAL_RATIO") != null && DataTableConverter.GetValue(e.Cell.Row.DataItem, "TOTAL_RATIO").ToString().Replace("%", "").Trim().GetDouble() >= 95)
                        {
                            if (Convert.ToString(e.Cell.Column.Name) == "T_CNT_TEXT")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.Background = null;
                            }
                        }

                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "USE_RATIO") != null && DataTableConverter.GetValue(e.Cell.Row.DataItem, "USE_RATIO").ToString().Replace("%", "").Trim().GetDouble() >= 95)
                        {
                            if (Convert.ToString(e.Cell.Column.Name) == "U_CNT_TEXT")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.Background = null;
                            }
                        }
                    }

                }
            }));
        }

        #endregion

        #region CNV 현황에 대한 색깔 지정 - 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결 : dgConveyor_UnloadedCellPresenter()
        /// <summary>
        ///  CNV 현황에 대한 색깔 지정 - 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgConveyor_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region CNV 현황 상세현황 조회 : dgConveyor_MouseLeftButtonUp()

        /// <summary>
        ///  CNV 현황 상세현황 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void dgConveyor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                if (cell.Column.Name.Equals("EQSGID_NM"))
                {
                    _selectedCstTypeCode = null;
                    if (cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedEquipmentSegmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentSegmentCode = DataTableConverter.GetValue(drv, "EQSGID").GetString();
                    }
                }
                else
                {
                    _selectedEquipmentSegmentCode = !string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "EQSGID").GetString()) ? DataTableConverter.GetValue(drv, "EQSGID").GetString() : null;

                    _selectedCstTypeCode = !cell.Column.Name.Equals("T_CNT_TEXT") ? cell.Column.Name.Substring(0, 1) : null;
                }

                tabCNVLot.IsSelected = true;
                SelectConveyorDetail();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region CNV 컬럼 Merge  : dgConveyor_MergingCells()
        /// <summary>
        /// CNV 컬럼 Merge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgConveyor_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        //=============================== 창고 적재 현황 관련 ==================================================

        #region 창고 적재 현황 조회 색깔 처리 : dgCapacitySummary_LoadedCellPresenter ()
        /// <summary>
        /// 창고 적재 현황 조회 색깔 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void dgCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "EQPT_NAME") || string.Equals(e.Cell.Column.Name, "U_CNT_TEXT") || string.Equals(e.Cell.Column.Name, "E_CNT_TEXT") || string.Equals(e.Cell.Column.Name, "O_CNT_TEXT"))
                    {
                        if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }

                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }


        #endregion

        #region 창고 적재 현황에 대한 색깔 지정 - 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결 : dgCapacitySummary_UnloadedCellPresenter()
        /// <summary>
        /// 창고 적재 현황에 대한 색깔 지정 - 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region 창고 적재 현황 데이터 클릭시 상세 조회 및 창고 재공현황 조회 : dgCapacitySummary_MouseLeftButtonUp()

        /// <summary>
        /// 창고 적재 현황 데이터 클릭시 상세 조회 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedLotIdByRackInfo = null;
                _selectedSkIdIdByRackInfo = null;
                _selectedBobbinIdByRackInfo = null;

                // 설비 정보 및 합계 클릭시
                if (cell.Column.Name.Equals("EQPT_NAME"))
                {


                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), string.Empty))
                    {
                        return;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    //창고 재공 현황 조회
                    SelectWareHouseProductSummary(false);

                    #region Layout 관련  - 현재는 사용안함  
                    //Util.gridClear(dgProduct);

                    //if (cell.Column.Name.Equals("EQPTNAME") || (cell.Column.Name.Equals("ELTR_TYPE_NAME") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    //{
                    //    _selectedWipHold = null;
                    //    _selectedProjectName = null;

                    //    if (tabLayout.IsSelected && (cell.Column.Name.Equals("EQPTNAME") || cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    //    {

                    //        ShowLoadingIndicator();
                    //        DoEvents();
                    //        // 창고에 대한 랙의 X,Y, LAY의  최대수량 조회 - ( Layout 모니터링 화면에서 사용)
                    //        SelectMaxxyz();
                    //        // UserControl 리셋 - ( Layout 모니터링 화면에서 사용)
                    //        ReSetLayoutUserControl();

                    //        if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                    //        {
                    //            // UserControl에 Rack 정보 바인딩
                    //            SelectRackInfo();
                    //        }
                    //        else
                    //        {
                    //            HiddenLoadingIndicator();
                    //        }


                    //    }
                    //    else
                    //    {
                    //        //입고 LOT 조회
                    //        SelectWareHouseProductList((table, ex) =>
                    //        {
                    //            tabProduct.IsSelected = true;
                    //            //창고 재공 현황 조회
                    //            SelectWareHouseProductSummary(false);

                    //            Util.GridSetData(dgProduct, table, null, true);
                    //            HiddenLoadingIndicator();
                    //        });
                    //    }

                    //}
                    //else
                    //{
                    //    tabProduct.IsSelected = true;
                    //    //창고 재공 현황 조회
                    //    SelectWareHouseProductSummary(false);
                    //}

                    #endregion


                }
                // 실 Tray 수  클릭 시
                else if (cell.Column.Name.Equals("U_CNT_TEXT"))
                {

                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), string.Empty))
                    {
                        return;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }



                    tabProduct.IsSelected = true;
                    //실 Tray 조회 
                    SelectWareHouseProductList(dgProduct);
                }
                // 공 Carrier 수  클릭 시
                else if (cell.Column.Name.Equals("E_CNT_TEXT"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), string.Empty))
                    {
                        return;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }
                    tabEmptyCarrier.IsSelected = true;
                    // 공트레이 조회
                    SelectWareHouseEmptyCarrierList(dgEmptyCarrier);
                }
                // 오류 Tray 클릭시
                else if (cell.Column.Name.Equals("O_CNT_TEXT"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), string.Empty))
                    {
                        return;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList(dgErrorCarrier);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


        //=============================== 실 Tray Tab 관련 ==================================================

        #region 실 Tray 색깔 지정 - 경과일수에 대한 색깔과 LOT링크 색깔 지정 : dgProduct_LoadedCellPresenter()
        /// <summary>
        /// 실 Tray 색깔 지정 - 경과일수에 대한 색깔과 LOT링크 색깔 지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                    if (_isscrollToHorizontalOffset)
                    {
                        dataGrid.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                    }
                }
                else
                {

                }
            }));
        }

        #endregion

        #region  실 Tray 색깔 지정 - 경과일수에 대한 색깔과 LOT링크 색깔 지정 ( 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결) : dgProduct_UnloadedCellPresenter()

        /// <summary>
        /// 실 Tray 색깔 지정 - 경과일수에 대한 색깔과 LOT링크 색깔 지정 ( 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region 실 Tray  Scroll 설정 : dgProduct_PreviewMouseLeftButtonDown()

        /// <summary>
        ///  실 Tray  Scroll 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isscrollToHorizontalOffset = false;
        }

        #endregion

        #region 실 트레이 출고예약 팝업닫기 : ppopupUsingTray_Closed()
        /// <summary>
        /// 실 Tray 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ppopupUsingTray_Closed(object sender, EventArgs e)
        {
            MCS001_060_USING_TRAY_OUTPUT popup = sender as MCS001_060_USING_TRAY_OUTPUT;
            if (popup != null && popup.IsUpdated)
            {
                btnSearch_Click(btnSearch, null);
            }
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            //    btnSearch_Click(btnSearch, null);
            //}

        }
        #endregion

        #region 공 트레이 출고예약 팝업닫기 : popupEmptyTray_Closed()
        /// <summary>
        /// 공 Tray 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupEmptyTray_Closed(object sender, EventArgs e)
        {
            MCS001_060_EMPTY_ERROR_TRAY_OUTPUT popup = sender as MCS001_060_EMPTY_ERROR_TRAY_OUTPUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
            }

        }

        #endregion

        #region 오류 트레이 출고예약 팝업닫기 : popupErrorTray_Closed()
        /// <summary>
        /// 오류 Tray 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupErrorTray_Closed(object sender, EventArgs e)
        {
            MCS001_060_EMPTY_ERROR_TRAY_OUTPUT popup = sender as MCS001_060_EMPTY_ERROR_TRAY_OUTPUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Click(btnSearch, null);
            }

        }

        #endregion

        //=============================== 공 Tray Tab 관련 ==================================================

        #region 공 Tray 관련 체크박스  Cell Merge : dgEmptyCarrier_MergingCells()
        /// <summary>
        ///  공 Tray 관련 체크박스  Cell Merge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgEmptyCarrier_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgEmptyCarrier.TopRows.Count; i < dgEmptyCarrier.Rows.Count; i++)
                {

                    if (dgEmptyCarrier.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEmptyCarrier.Rows[i].DataItem, "RACK_NAME"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgEmptyCarrier.Rows[i].DataItem, "RACK_NAME")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgEmptyCarrier.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgEmptyCarrier.GetCell(idxS, dgEmptyCarrier.Columns["CHK"].Index), dgEmptyCarrier.GetCell(idxE, dgEmptyCarrier.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgEmptyCarrier.GetCell(idxS, dgEmptyCarrier.Columns["REQ_STAT"].Index), dgEmptyCarrier.GetCell(idxE, dgEmptyCarrier.Columns["REQ_STAT"].Index)));
                            
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgEmptyCarrier.GetCell(idxS, dgEmptyCarrier.Columns["CHK"].Index), dgEmptyCarrier.GetCell(idxE, dgEmptyCarrier.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgEmptyCarrier.GetCell(idxS, dgEmptyCarrier.Columns["REQ_STAT"].Index), dgEmptyCarrier.GetCell(idxE, dgEmptyCarrier.Columns["REQ_STAT"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEmptyCarrier.Rows[i].DataItem, "RACK_NAME"));
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
                //Util.MessageException(ex);
            }
        }
        #endregion


        //=============================== 오류 Tray Tab 관련 ==================================================

        #region 오류 Tray 관련 체크박스  Cell Merge  : dgErrorCarrier_MergingCells()
        /// <summary>
        ///  오류 Tray 관련 체크박스  Cell Merge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgErrorCarrier_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgErrorCarrier.TopRows.Count; i < dgErrorCarrier.Rows.Count; i++)
                {

                    if (dgErrorCarrier.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgErrorCarrier.Rows[i].DataItem, "RACK_NAME"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgErrorCarrier.Rows[i].DataItem, "RACK_NAME")).Equals(sTmpLvCd))
                            {
                                idxE = i;

                                //마지막 Row 일경우
                                if (i == dgErrorCarrier.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgErrorCarrier.GetCell(idxS, dgErrorCarrier.Columns["CHK"].Index), dgErrorCarrier.GetCell(idxE, dgErrorCarrier.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgErrorCarrier.GetCell(idxS, dgErrorCarrier.Columns["REQ_STAT"].Index), dgErrorCarrier.GetCell(idxE, dgErrorCarrier.Columns["REQ_STAT"].Index)));

                                }


                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgErrorCarrier.GetCell(idxS, dgErrorCarrier.Columns["CHK"].Index), dgErrorCarrier.GetCell(idxE, dgErrorCarrier.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgErrorCarrier.GetCell(idxS, dgErrorCarrier.Columns["REQ_STAT"].Index), dgErrorCarrier.GetCell(idxE, dgErrorCarrier.Columns["REQ_STAT"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgErrorCarrier.Rows[i].DataItem, "RACK_NAME"));
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
                //Util.MessageException(ex);
            }
        }

        #endregion


        //=============================== JOB 현황 Tab 관련 ==================================================

        #region JOB 현황 색깔 지정 : dgJobStatus_LoadedCellPresenter()
        /// <summary>
        ///  JOB 현황 색깔 지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgJobStatus_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LINE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                    if (cboLevel.SelectedIndex == 2 && (Convert.ToString(e.Cell.Column.Name) == "S_E_CNT_TEXT" || Convert.ToString(e.Cell.Column.Name) == "E_DIR_CNT_TEXT"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }

        #endregion

        #region JOB 현황 색깔 지정 ( 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결) : dgJobStatus_UnloadedCellPresenter()
        /// <summary>
        /// JOB 현황 색깔 지정 ( 스크롤을 내릴 시 색깔이 초기화 되는 문제 해결)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgJobStatus_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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


        //================================= 기타등등 ==================================

        #region Splitter 관련 이벤트 : Splitter_DragStarted(), Splitter_DragCompleted()

        private void Splitter_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            GridSplitter splitter = (GridSplitter)sender;

            try
            {
                C1DataGrid dataGrid = dgCapacitySummary;
                double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);

                if (ContentsRow.ColumnDefinitions[0].Width.Value > sumWidth)
                {
                    ContentsRow.ColumnDefinitions[0].Width = new GridLength(sumWidth + splitter.ActualWidth);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }




        #endregion


     
        //================================== Layout 탭 관련 ============================
        #region Layout 관련  - 현재는 사용안함  
        /// <summary>
        /// 탭 정보 선택(Layout) 시  UserControl 정보 바인딩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    if (CommonVerify.HasDataGridRow(dgProduct))
            //    {
            //        _isscrollToHorizontalOffset = true;
            //        _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
            //    }

            //    string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

            //    if (string.Equals(tabItem, "tabLayout"))
            //    {
            //        if (!string.IsNullOrEmpty(_selectedEquipmentCode))
            //        {
            //            ShowLoadingIndicator();
            //            DoEvents();
            //            // 창고에 대한 랙의 X,Y, LAY의  최대수량 조회 - ( Layout 모니터링 화면에서 사용)
            //            SelectMaxxyz();
            //            // UserControl 리셋 - ( Layout 모니터링 화면에서 사용)
            //            ReSetLayoutUserControl();
            //            // User Control 정보에  창고 정보 바인딩
            //            SelectRackInfo();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }


        /// <summary>
        /// 1열 UserControl 체크박스 클릭시  상세 정보 리스트 바인딩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout1_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            if (rackLayout.IsChecked)
            {
                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout1[row][col];

                        if (!Equals(rackLayout, ucRackLayout))
                        {
                            ucRackLayout.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (ucRackLayout.IsChecked)
                            ucRackLayout.IsChecked = false;
                    }
                }

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
                }

                _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
                _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
                _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;

                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackLayout.RackId;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["PRJT_NAME"] = rackLayout.ProjectName;
                dr["LOTID"] = rackLayout.LotId;
                dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
                dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["WIPHOLD"] = rackLayout.WipHold;
                dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
                dr["SKID_GUBUN"] = rackLayout.SkidType;
                dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
                dr["COLOR"] = rackLayout.LegendColor;
                dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
                dr["HOLD_FLAG"] = rackLayout.HoldFlag;
                dr["SEQ"] = maxSeq;
                _dtRackInfo.Rows.Add(dr);

                if (rackLayout.LegendColorType == "4")
                {
                    _selectedSkIdIdByRackInfo = null;
                }
                else if (rackLayout.LegendColorType == "5")
                {
                    _selectedBobbinIdByRackInfo = null;
                }

                GetLayoutGridColumns(rackLayout.LegendColorType);
                SelectLayoutGrid(rackLayout.LegendColorType);
            }
            else
            {

                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }
        }


        /// <summary>
        /// 2열 UserControl 체크박스 클릭시  상세 정보 리스트 바인딩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout2_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            if (rackLayout.IsChecked)
            {
                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (!Equals(rackLayout, ucRackLayout))
                        {
                            ucRackLayout.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackStair = _ucRackLayout1[row][col];

                        if (ucRackStair.IsChecked)
                            ucRackStair.IsChecked = false;
                    }
                }

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
                }

                _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
                _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
                _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;

                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackLayout.RackId;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["PRJT_NAME"] = rackLayout.ProjectName;
                dr["LOTID"] = rackLayout.LotId;
                dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
                dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["WIPHOLD"] = rackLayout.WipHold;
                dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
                dr["SKID_GUBUN"] = rackLayout.SkidType;
                dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
                dr["COLOR"] = rackLayout.LegendColor;
                dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
                dr["HOLD_FLAG"] = rackLayout.HoldFlag;
                dr["SEQ"] = maxSeq;
                _dtRackInfo.Rows.Add(dr);
                GetLayoutGridColumns(rackLayout.LegendColorType);
                SelectLayoutGrid(rackLayout.LegendColorType);
            }
            else
            {

                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///  Layout  탭에서 LOT 아이디 및  보빈ID 정보를 입력시 에니메이션 효과 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            //TextBox textBox = sender as TextBox;
            //if (sender == null) return;

            //if (string.IsNullOrEmpty(textBox.Text.Trim())) return;

            //if (textBox.Name == "txtLotId")
            //{
            //    txtBobbinId.Text = string.Empty;
            //}
            //else
            //{
            //    txtLotId.Text = string.Empty;
            //}


            //if (e.Key == Key.Enter)
            //{
            //    DoubleAnimation doubleAnimation = new DoubleAnimation();

            //    UnCheckedAllRackLayout();

            //    for (int r = 0; r < grdRackstair1.RowDefinitions.Count; r++)
            //    {
            //        for (int c = 0; c < grdRackstair1.ColumnDefinitions.Count; c++)
            //        {
            //            UcRackLayout ucRackLayout = _ucRackLayout1[r][c];

            //            doubleAnimation.From = ucRackLayout.ActualHeight;
            //            doubleAnimation.To = 0;
            //            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
            //            doubleAnimation.AutoReverse = true;

            //            string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.BobbinCarrierCode;

            //            if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
            //            {
            //                SetScrollToHorizontalOffset(scrollViewer1, c);
            //                ucRackLayout.IsChecked = true;
            //                CheckUcRackLayout(ucRackLayout);
            //                ucRackLayout.BeginAnimation(HeightProperty, doubleAnimation);

            //                return;
            //            }
            //        }
            //    }

            //    for (int r = 0; r < grdRackstair2.RowDefinitions.Count; r++)
            //    {
            //        for (int c = 0; c < grdRackstair2.ColumnDefinitions.Count; c++)
            //        {
            //            UcRackLayout ucRackLayout = _ucRackLayout2[r][c];

            //            doubleAnimation.From = ucRackLayout.ActualHeight;
            //            doubleAnimation.To = 0;
            //            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            //            doubleAnimation.AutoReverse = true;

            //            string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.BobbinCarrierCode;

            //            if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
            //            {
            //                SetScrollToHorizontalOffset(scrollViewer2, c);
            //                ucRackLayout.IsChecked = true;
            //                CheckUcRackLayout(ucRackLayout);
            //                ucRackLayout.BeginAnimation(HeightProperty, doubleAnimation);

            //                return;
            //            }
            //        }
            //    }
            //}
        }


        #endregion


        #endregion

        #region Method

        //=============================== CNV 현황 관련 ==================================================

        #region CNV 현황 조회 : SelectConveyorSummary()
        /// <summary>
        /// CNV 현황 조회
        /// </summary>
        private void SelectConveyorSummary()
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_MNT";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboLevel.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUT_BK_INFO", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("T_CNT_TEXT", typeof(string));    //총트레이 Bundle 수량
                    bizResult.Columns.Add("U_CNT_TEXT", typeof(string));    //실트레이 Bundle 수량
                    bizResult.Columns.Add("E_CNT_TEXT", typeof(string));    //공트레이 Bundle 수량
                    bizResult.Columns.Add("O_CNT_TEXT", typeof(string));    //기타 Bundle 수량

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            bizResult.Rows[i]["T_CNT_TEXT"] = bizResult.Rows[i]["T_CNT"].GetString() + " / " + bizResult.Rows[i]["T_MAX_CNT"].GetString() + " (" + bizResult.Rows[i]["TOTAL_RATIO"].GetString() + ")";
                            bizResult.Rows[i]["U_CNT_TEXT"] = bizResult.Rows[i]["U_CNT"].GetString() + " / " + bizResult.Rows[i]["U_MAX_CNT"].GetString() + " (" + bizResult.Rows[i]["USE_RATIO"].GetString() + ")";
                            bizResult.Rows[i]["E_CNT_TEXT"] = bizResult.Rows[i]["E_CNT"].GetString();
                            bizResult.Rows[i]["O_CNT_TEXT"] = bizResult.Rows[i]["O_CNT"].GetString();
                        }

                        var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                        {
                            EquipmentSegmentCode = string.Empty,
                            EquipmentSegmentName = ObjectDic.Instance.GetObjectName("합계"),
                            TotalCount = g.Sum(x => x.Field<Int32>("T_CNT")),
                            RealCount = g.Sum(x => x.Field<Int32>("U_CNT")),
                            EmptyCount = g.Sum(x => x.Field<Int32>("E_CNT")),
                            EtcCount = g.Sum(x => x.Field<Int32>("O_CNT")),
                            TotalMaxCount = g.Sum(x => x.Field<Int32>("T_MAX_CNT")),    //공/실 트레이 최대수량
                            RealMaxCount = g.Sum(x => x.Field<Int32>("U_MAX_CNT")),     //실 트레이 최대수량
                            Count = g.Count()
                        }).FirstOrDefault();

                        if (query != null)
                        {
                            DataRow newRow = bizResult.NewRow();
                            newRow["EQSGID"] = query.EquipmentSegmentCode;
                            newRow["EQSGID_NM"] = query.EquipmentSegmentName;

                            if (cboLevel.SelectedIndex == 2)
                            {
                                newRow["T_CNT_TEXT"] = bizResult.Rows[0]["T_CNT"].GetString() + " / " + bizResult.Rows[0]["T_MAX_CNT"].GetString() + " (" + bizResult.Rows[0]["TOTAL_RATIO"].GetString() + ")";
                                newRow["E_CNT_TEXT"] = bizResult.Rows[0]["E_CNT"].GetString();
                            }
                            else
                            {
                                newRow["T_CNT_TEXT"] = query.TotalCount.GetString() + " / " + query.TotalMaxCount.GetString() + " (" + GetPercentage(query.TotalCount, query.TotalMaxCount).ToString("0.##") + " %)";
                                newRow["E_CNT_TEXT"] = query.EmptyCount.GetString();
                            }

                            newRow["U_CNT_TEXT"] = query.RealCount.GetString() + " / " + query.RealMaxCount.GetString() + " (" + GetPercentage(query.RealCount, query.RealMaxCount).ToString("0.##") + " %)";
                            newRow["O_CNT_TEXT"] = query.EtcCount.GetString();
                            bizResult.Rows.Add(newRow);
                        }

                        Util.GridSetData(dgConveyor, bizResult, null, true);

                        if (cboLevel.SelectedIndex == 2)
                        {
                            _util.SetDataGridMergeExtensionCol(dgConveyor, new string[] { "T_CNT_TEXT", "E_CNT_TEXT" }, ControlsLibrary.DataGridMergeMode.VERTICALHIERARCHI);
                        }
                        //창고적재현황 조회
                        SelectWareHouseCapacitySummary();
                     
                    }

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region CNV LOT Tab 조회 : SelectConveyorDetail()
        /// <summary>
        /// CNV LOT Tab 조회
        /// </summary>
        private void SelectConveyorDetail()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_DETL_MNT";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CSTSTAT", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboLevel.SelectedValue;
                dr["CSTSTAT"] = _selectedCstTypeCode;
                dr["EQSGID"] = _selectedEquipmentSegmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUT_BK_DETL_INFO", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var dtBinding = bizResult.Copy();
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "SEQ", DataType = typeof(int) });
                    int rowIndex = 1;
                    foreach (DataRow row in dtBinding.Rows)
                    {
                        row["SEQ"] = rowIndex;
                        rowIndex++;
                    }
                    dtBinding.AcceptChanges();


                    Util.GridSetData(dgConveyorDetail, dtBinding, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion


        //=============================== 창고 적재 현황 관련 ==================================================

        #region 창고 적재 현황 조회 : SelectWareHouseCapacitySummary()

        /// <summary>
        /// 창고 적재 현황 조회
        /// </summary>
        private void SelectWareHouseCapacitySummary()
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_STK_MNT";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboLevel.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("RACK_CNT_TEXT", typeof(string)); //용량
                    bizResult.Columns.Add("U_CNT_TEXT", typeof(string));    //실Tray수
                    bizResult.Columns.Add("E_CNT_TEXT", typeof(string));    //공Tray수
                    bizResult.Columns.Add("O_CNT_TEXT", typeof(string));    //오류Tray수
                    bizResult.Columns.Add("RATE_TEXT", typeof(string));    //적재율

                    if (CommonVerify.HasTableRow(bizResult))
                    {

                        Int32 _uBundel = 0;
                        Int32 _eBundel = 0;
                        Int32 _oCnt = 0;
                        Int32 _rCnt = 0;


                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            bizResult.Rows[i]["RACK_CNT_TEXT"] = bizResult.Rows[i]["RACK_CNT"].GetString();
                            bizResult.Rows[i]["U_CNT_TEXT"] = bizResult.Rows[i]["U_BUNDLE_CNT"].GetString() + " (" + bizResult.Rows[i]["U_CNT"].GetString() + ")";
                            bizResult.Rows[i]["E_CNT_TEXT"] = bizResult.Rows[i]["E_BUNDLE_CNT"].GetString() + " (" + bizResult.Rows[i]["E_CNT"].GetString() + ")";
                            bizResult.Rows[i]["O_CNT_TEXT"] = bizResult.Rows[i]["O_CNT"].GetString();
                            _uBundel = Convert.ToInt32(string.IsNullOrEmpty( bizResult.Rows[i]["U_BUNDLE_CNT"].GetString()) ? 0 : bizResult.Rows[i]["U_BUNDLE_CNT"]);
                            _eBundel = Convert.ToInt32(string.IsNullOrEmpty(bizResult.Rows[i]["E_BUNDLE_CNT"].GetString()) ? 0 : bizResult.Rows[i]["E_BUNDLE_CNT"]);
                            _oCnt = Convert.ToInt32(string.IsNullOrEmpty(bizResult.Rows[i]["O_CNT"].GetString()) ? 0 : bizResult.Rows[i]["O_CNT"]);
                            _rCnt = Convert.ToInt32(string.IsNullOrEmpty(bizResult.Rows[i]["RACK_CNT"].GetString()) ? 0 : bizResult.Rows[i]["RACK_CNT"]);

                            //bizResult.Rows[i]["RATE_TEXT"] = Math.Round(Convert.ToDouble(string.IsNullOrEmpty(bizResult.Rows[i]["RATE"].ToString()) ? 0 : bizResult.Rows[i]["RATE"]), 1).ToString("0.##") + " %";
                            bizResult.Rows[i]["RATE_TEXT"] = GetPercentage(_uBundel + _eBundel + _oCnt, _rCnt).ToString("0.##") + " %";
                        }

                        var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                        {
                            EquipmentSegmentCode = string.Empty,
                            EquipmentSegmentName = ObjectDic.Instance.GetObjectName("합계"),
                            RackCount = g.Sum(x => x.Field<Int32>("RACK_CNT")),            //용량
                            U_BundleCount = g.Sum(x => x.Field<Int32>("U_BUNDLE_CNT")),    //실 TRAY 번들 수
                            U_AllBundleCount = g.Sum(x => x.Field<Int32>("U_CNT")),        //실 TARY 총 수
                            E_BundleCount = g.Sum(x => x.Field<Int32>("E_BUNDLE_CNT")),    //공 TRAY 번틀 수         
                            E_AllBundleCount = g.Sum(x => x.Field<Int32>("E_CNT")),        //공 TARY 총 수
                            O_CNT_COUNT = g.Sum(x => x.Field<Int32>("O_CNT")),             //오류 TRAY 수

                            Count = g.Count()
                        }).FirstOrDefault();

                        if (query != null)
                        {
                            DataRow newRow = bizResult.NewRow();
                            newRow["EQPTID"] = query.EquipmentSegmentCode;
                            newRow["EQPT_NAME"] = query.EquipmentSegmentName;
                            newRow["RACK_CNT_TEXT"] = query.RackCount.GetString();
                            newRow["U_CNT_TEXT"] = query.U_BundleCount.GetString() + " (" + query.U_AllBundleCount.GetString() + ")";
                            newRow["E_CNT_TEXT"] = query.E_BundleCount.GetString() + " (" + query.E_AllBundleCount.GetString() + ")";
                            newRow["O_CNT_TEXT"] = query.O_CNT_COUNT.GetString();
                            newRow["RATE_TEXT"] = GetPercentage(query.U_BundleCount + query.E_BundleCount + query.O_CNT_COUNT, query.RackCount).ToString("0.##") + " %";
                            bizResult.Rows.Add(newRow);
                        }

                        Util.GridSetData(dgCapacitySummary, bizResult, null, true);
                      
                        // JOB 현황
                        SelectJOB();
                    }

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #endregion

        #region  실 Tray 탭 조회 : SelectWareHouseProductList()


        /// <summary>
        /// 실 Tray 조회
        /// </summary>
        private void SelectWareHouseProductList(C1DataGrid dg)
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_U_TRAY_MNT_DETL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _selectedEquipmentCode;
                inTable.Rows.Add(dr);


                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgProduct, bizResult, null, true);

                    HiddenLoadingIndicator();
                });


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 공 Tray 탭 조회 : SelectWareHouseEmptyCarrierList()
        /// <summary>
        /// 데이터 그리드에 대한 공 Tray 정보 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectWareHouseEmptyCarrierList(C1DataGrid dg)
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_E_TRAY_MNT_DETL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _selectedEquipmentCode;

                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                   
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("RACK_ID", typeof(string));
                    dtData.Columns.Add("REQ_STAT", typeof(string));
                 

                    Util.GridSetData(dg, bizResult, null, true);

                    dgEmptyCarrier.MergingCells -= dgEmptyCarrier_MergingCells;
                    dgEmptyCarrier.MergingCells += dgEmptyCarrier_MergingCells;

                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 오류 Tray 탭 조회 : SelectErrorCarrierList()
        /// <summary>
        /// 데이터  그리드에 대한 오류 Tray 정보 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectErrorCarrierList(C1DataGrid dg)
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_O_TRAY_MNT_DETL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _selectedEquipmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                   
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("RACK_ID", typeof(string));
                    dtData.Columns.Add("REQ_STAT", typeof(string));

                    Util.GridSetData(dg, bizResult, null, true);

                    dgErrorCarrier.MergingCells -= dgErrorCarrier_MergingCells;
                    dgErrorCarrier.MergingCells += dgErrorCarrier_MergingCells;


                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }



        #endregion

        #region 창고 재공 현황 조회 : SelectWareHouseProductSummary()
        /// <summary>
        /// 창고 재공 현황 조회
        /// </summary>
        private void SelectWareHouseProductSummary(bool isRefresh = false)
        {

            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_STK_LOT_MNT";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _selectedEquipmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgProductSummary, bizResult, null, true);


                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 창고 재공 현황  헤드높이 지정 : InitializeGrid()

        /// <summary>
        ///  창고 재공 현황  헤드높이 지정
        /// </summary>
        private void InitializeGrid()
        {
            dgProductSummary.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgProductSummary.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

        }
        #endregion


        //=============================== JOB 현황 관련 ==================================================

        #region JOB 현황 탭 조회 : SelectJOB()
        /// <summary>
        /// JOB 현황 조회 
        /// </summary>
        private void SelectJOB()
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_TRF_INFO_DETL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboLevel.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("S_TOTAL_CNT_TEXT", typeof(string)); //Total (CNV 반송 기준 수량)
                    bizResult.Columns.Add("S_U_CNT_TEXT", typeof(string));     //실Total (CNV 반송 기준 수량)
                    bizResult.Columns.Add("S_DIR_CNT_TEXT", typeof(string));    //PKG Direct (CNV 반송 기준 수량)
                    bizResult.Columns.Add("S_E_CNT_TEXT", typeof(string));     //공 Total (CNV 반송 기준 수량)

                    bizResult.Columns.Add("U_DIR_CNT_TEXT", typeof(string));   //STK/FOL -> PKG (실 Tray 반송)
                    bizResult.Columns.Add("U_STK_CNT_TEXT", typeof(string));   //STK/FOL -> STK (실 Tray 반송)
                    bizResult.Columns.Add("U_PKG_CNT_TEXT", typeof(string));   //STK -> PKG (실 Tray 반송)
                    bizResult.Columns.Add("U_TOTAL_CNT_TEXT", typeof(string)); //TOTAL COUNT (실 Tray 반송)

                    bizResult.Columns.Add("E_DIR_CNT_TEXT", typeof(string));   //PKG -> STK/FOL (공 TRAY 반송)
                    bizResult.Columns.Add("E_STK_CNT_TEXT", typeof(string));   //PKG -> STK (공 TRAY 반송)
                    bizResult.Columns.Add("E_FOL_CNT_TEXT", typeof(string));   //STK -> STK/FOL (공 TRAY 반송)
                    bizResult.Columns.Add("E_TOTAL_CNT_TEXT", typeof(string));   //TOTAL COUNT (공 TRAY 반송)


                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            bizResult.Rows[i]["S_TOTAL_CNT_TEXT"] = bizResult.Rows[i]["S_TOTAL_CNT"].GetString();  //Total (CNV 반송 기준 수량)
                            bizResult.Rows[i]["S_U_CNT_TEXT"] = bizResult.Rows[i]["S_U_CNT"].GetString();      //실Total (CNV 반송 기준 수량)
                            bizResult.Rows[i]["S_DIR_CNT_TEXT"] = bizResult.Rows[i]["S_DIR_CNT"].GetString();   //PKG Direct (CNV 반송 기준 수량)
                            bizResult.Rows[i]["S_E_CNT_TEXT"] = bizResult.Rows[i]["S_E_CNT"].GetString();      //공 Total (CNV 반송 기준 수량)

                            bizResult.Rows[i]["U_DIR_CNT_TEXT"] = bizResult.Rows[i]["U_DIR_CNT"].GetString();      //STK/FOL -> PKG (실 Tray 반송)
                            bizResult.Rows[i]["U_STK_CNT_TEXT"] = bizResult.Rows[i]["U_STK_CNT"].GetString();      //STK/FOL -> STK (실 Tray 반송)
                            bizResult.Rows[i]["U_PKG_CNT_TEXT"] = bizResult.Rows[i]["U_PKG_CNT"].GetString();      //STK -> PKG (실 Tray 반송)
                            bizResult.Rows[i]["U_TOTAL_CNT_TEXT"] = Math.Round(Convert.ToDouble(bizResult.Rows[i]["U_TOTAL_RATE"]), 1).ToString("0.##") + "% ( " + bizResult.Rows[i]["U_TOTAL_CNT"].GetString() + "/" + bizResult.Rows[i]["S_U_CNT"].GetString() + " )";//TOTAL COUNT (실 Tray 반송)

                            bizResult.Rows[i]["E_DIR_CNT_TEXT"] = bizResult.Rows[i]["E_DIR_CNT"].GetString();      //PKG -> STK/FOL (공 TRAY 반송)
                            bizResult.Rows[i]["E_STK_CNT_TEXT"] = bizResult.Rows[i]["E_STK_CNT"].GetString();      //PKG -> STK (공 TRAY 반송)
                            bizResult.Rows[i]["E_FOL_CNT_TEXT"] = bizResult.Rows[i]["E_FOL_CNT"].GetString();      //STK -> STK/FOL (공 TRAY 반송)
                            bizResult.Rows[i]["E_TOTAL_CNT_TEXT"] = Math.Round(Convert.ToDouble(bizResult.Rows[i]["E_TOTAL_RATE"]), 1).ToString("0.##") + "% ( " + bizResult.Rows[i]["E_TOTAL_CNT"].GetString() + "/" + bizResult.Rows[i]["S_E_CNT"].GetString() + " )"; //TOTAL COUNT (공 TRAY 반송)
                        }
                        var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                        {
                            LineCode = string.Empty,
                            LineName = ObjectDic.Instance.GetObjectName("합계"),

                            S_TOTAL_CNT = g.Sum(x => x.Field<Int32>("S_TOTAL_CNT")),    //Total (CNV 반송 기준 수량)
                            S_U_CNT = g.Sum(x => x.Field<Int32>("S_U_CNT")),            //실Total (CNV 반송 기준 수량)
                            S_DIR_CNT = g.Sum(x => x.Field<Int32>("S_DIR_CNT")),        //PKG Direct (CNV 반송 기준 수량)
                            S_E_CNT = g.Sum(x => x.Field<Int32>("S_E_CNT")),            //공 Total (CNV 반송 기준 수량)        
                            U_DIR_CNT = g.Sum(x => x.Field<Int32>("U_DIR_CNT")),    //STK/FOL -> PKG (실 Tray 반송)
                            U_STK_CNT = g.Sum(x => x.Field<Int32>("U_STK_CNT")),    //STK/FOL -> STK (실 Tray 반송)
                            U_PKG_CNT = g.Sum(x => x.Field<Int32>("U_PKG_CNT")),    //STK -> PKG (실 Tray 반송)
                            E_DIR_CNT = g.Sum(x => x.Field<Int32>("E_DIR_CNT")),   //PKG -> STK/FOL (공 TRAY 반송)
                            E_STK_CNT = g.Sum(x => x.Field<Int32>("E_STK_CNT")),   //PKG -> STK (공 TRAY 반송)
                            E_FOL_CNT = g.Sum(x => x.Field<Int32>("E_FOL_CNT")),   //STK -> STK/FOL (공 TRAY 반송)
                            Count = g.Count()
                        }).FirstOrDefault();

                        if (query != null)
                        {
                            DataRow newRow = bizResult.NewRow();
                            newRow["LINEID"] = query.LineCode;
                            newRow["LINE_NAME"] = query.LineName;


                            if (cboLevel.SelectedIndex == 2)
                            {
                                newRow["S_E_CNT_TEXT"] = bizResult.Rows[0]["S_E_CNT"].GetString();     //공 Total (CNV 반송 기준 수량)  
                                newRow["E_DIR_CNT_TEXT"] = bizResult.Rows[0]["E_DIR_CNT"].GetString();
                            }
                            else
                            {
                                newRow["S_E_CNT_TEXT"] = query.S_E_CNT.GetString();     //공 Total (CNV 반송 기준 수량)  
                                newRow["E_DIR_CNT_TEXT"] = query.E_DIR_CNT.GetString(); //PKG -> STK/FOL (공 TRAY 반송)
                            }

                            newRow["S_TOTAL_CNT_TEXT"] = query.S_TOTAL_CNT.GetString(); //Total (CNV 반송 기준 수량)
                            newRow["S_U_CNT_TEXT"] = query.S_U_CNT.GetString();     //실Total (CNV 반송 기준 수량)
                            newRow["S_DIR_CNT_TEXT"] = query.S_DIR_CNT.GetString();   //PKG Direct (CNV 반송 기준 수량)
                            newRow["U_DIR_CNT_TEXT"] = query.U_DIR_CNT.GetString(); //STK/FOL -> PKG (실 Tray 반송)
                            newRow["U_STK_CNT_TEXT"] = query.U_STK_CNT.GetString();     //STK/FOL -> STK (실 Tray 반송)
                            newRow["U_PKG_CNT_TEXT"] = query.U_PKG_CNT.GetString();   //STK -> PKG (실 Tray 반송)
                            newRow["U_TOTAL_CNT_TEXT"] = GetPercentage(query.U_DIR_CNT + query.U_STK_CNT + query.U_PKG_CNT, query.S_U_CNT).ToString("0.##") + " % (" + Convert.ToString(query.U_DIR_CNT + query.U_STK_CNT + query.U_PKG_CNT) + "/" + Convert.ToString(query.S_U_CNT) + ")";
                            newRow["E_STK_CNT_TEXT"] = query.E_STK_CNT.GetString(); //PKG -> STK (공 TRAY 반송)
                            newRow["E_FOL_CNT_TEXT"] = query.E_FOL_CNT.GetString(); //STK -> STK/FOL (공 TRAY 반송)
                            newRow["E_TOTAL_CNT_TEXT"] = GetPercentage(query.E_DIR_CNT + query.E_STK_CNT + query.E_FOL_CNT, query.S_E_CNT).ToString("0.##") + " % (" + Convert.ToString(query.E_DIR_CNT + query.E_STK_CNT + query.E_FOL_CNT) + "/" + Convert.ToString(query.S_E_CNT) + ")";
                            bizResult.Rows.Add(newRow);
                        }

                        Util.GridSetData(dgJobStatus, bizResult, null, true);

                    }

                    if (cboLevel.SelectedIndex == 2)
                    {
                        _util.SetDataGridMergeExtensionCol(dgJobStatus, new string[] { "S_E_CNT_TEXT", "E_DIR_CNT_TEXT" }, ControlsLibrary.DataGridMergeMode.VERTICALHIERARCHI);

                        dgJobStatus.Columns["E_STK_CNT_TEXT"].Visibility = Visibility.Collapsed;
                        dgJobStatus.Columns["E_FOL_CNT_TEXT"].Visibility = Visibility.Collapsed;
                        dgJobStatus.Columns["E_TOTAL_CNT_TEXT"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        _util.SetDataGridMergeExtensionCol(dgJobStatus, new string[] { "S_E_CNT_TEXT", "E_DIR_CNT_TEXT" }, ControlsLibrary.DataGridMergeMode.NONE);

                        dgJobStatus.Columns["E_STK_CNT_TEXT"].Visibility = Visibility.Visible;
                        dgJobStatus.Columns["E_FOL_CNT_TEXT"].Visibility = Visibility.Visible;
                        dgJobStatus.Columns["E_TOTAL_CNT_TEXT"].Visibility = Visibility.Visible;
                    }


                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #endregion

        //=============================== 기타 등등 ==================================================

        #region 콤보박스 조회 : InitializeCombo(), SetAreaCombo(), SetLevelCombo()
      
        /// <summary>
        /// 콤보박스 조회
        /// </summary>
        private void InitializeCombo()
        {
            // Area 콤보박스
            SetAreaCombo(cboArea);

            // 층 콤보박스
            SetLevelCombo(cboLevel);
        }


        /// <summary>
        /// 동정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };

            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }


        /// <summary>
        /// 층정보
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetLevelCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MMD_MCS_COMMONCODE";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "A7_FLOW";
            dr["CMCODE"] = null;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }


        #endregion

        #region 컨트롤 초기화 : ClearControl()
        /// <summary>
        /// 컨트롤 초기화
        /// </summary>
        private void ClearControl()
        {
            _selectedEquipmentCode = string.Empty;
            _selectedProjectName = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _selectedLotIdByRackInfo = string.Empty;
            _selectedSkIdIdByRackInfo = string.Empty;
            _selectedBobbinIdByRackInfo = string.Empty;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;
            _selectedEquipmentSegmentCode = string.Empty;
            _selectedCstTypeCode = string.Empty;

            _dtWareHouseCapacity?.Clear();

            tabCNVLot.IsSelected = true;
            Util.gridClear(dgConveyor);
            Util.gridClear(dgCapacitySummary);
            Util.gridClear(dgProductSummary);
            Util.gridClear(dgConveyorDetail);
            Util.gridClear(dgProduct);
            Util.gridClear(dgEmptyCarrier);
            Util.gridClear(dgErrorCarrier);
            Util.gridClear(dgRackInfo);
            Util.gridClear(dgJobStatus);
            InitializeRackUserControl();
        }

        #endregion

        #region 프로그래스바 관련 : ShowLoadingIndicator(), HiddenLoadingIndicator()
        /// <summary>
        /// 프로그래스 바 보이기
        /// </summary>
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// 프로그래스 바 숨기기
        /// </summary>
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        #endregion

        #region % 계산 : GetPercentage()
        /// <summary>
        /// 합계 % 계산
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private double GetPercentage(int x, int y)
        {
            double rate = 0;
            if (y.Equals(0)) return rate;

            try
            {
                return Math.Round(x.GetDouble() / y.GetDouble() * 100, 1);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region MCS 비즈 접속 정보 : GetBizActorServerInfo()
        /// <summary>
        /// MCS 비즈 접속 정보
        /// </summary>
        private void GetBizActorServerInfo()
        {

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["KEYGROUPID"] = "MCS_AP_CONFIG";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                foreach (DataRow newRow in dtResult.Rows)
                {
                    if (newRow["KEYID"].ToString() == "BizActorIP")
                        _bizRuleIp = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorPort")
                        _bizRulePort = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorProtocol")
                        _bizRuleProtocol = newRow["KEYVALUE"].ToString();
                    else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
                        _bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
                    else
                        _bizRuleServiceMode = newRow["KEYVALUE"].ToString();
                }
            }

        }

        #endregion

        #region Layout 관련  - 현재는 사용안함  



        private void SetScrollToHorizontalOffset(ScrollViewer scrollViewer, int colIndex)
        {
            double averageScrollWidth = scrollViewer.ActualWidth / _maxColumnCount;
            scrollViewer.ScrollToHorizontalOffset(Math.Abs(colIndex) * averageScrollWidth);
        }

        /// <summary>
        /// User Control 정보에  창고 정보 바인딩
        /// </summary>
        private void SelectRackInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_RACK_LAYOUT";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(dr);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                HideAndClearAllRack();
                Util.gridClear(dgRackInfo);

                if (CommonVerify.HasTableRow(bizResult))
                {
                    foreach (DataRow item in bizResult.Rows)
                    {
                        int x = GetXPosition(item["Z_PSTN"].ToString());
                        int y = int.Parse(item["Y_PSTN"].ToString()) - 1;

                        UcRackLayout ucRackLayout = item["X_PSTN"].ToString() == "1" ? _ucRackLayout1[x][y] : _ucRackLayout2[x][y];

                        if (ucRackLayout == null) continue;

                        ucRackLayout.RackId = item["RACK_ID"].GetString();
                        ucRackLayout.Row = int.Parse(item["Z_PSTN"].GetString());
                        ucRackLayout.Col = int.Parse(item["Y_PSTN"].GetString());
                        ucRackLayout.Stair = int.Parse(item["X_PSTN"].GetString());
                        ucRackLayout.RackStateCode = item["STSTUS"].GetString();
                        ucRackLayout.ProjectName = item["PRJT_NAME"].GetString();
                        ucRackLayout.LotId = item["LOTID"].GetString();
                        ucRackLayout.SkidCarrierProductCode = item["SD_CSTPROD"].GetString();
                        ucRackLayout.SkidCarrierProductName = item["SD_CSTPROD_NAME"].GetString();
                        ucRackLayout.SkidCarrierCode = item["SD_CSTID"].GetString();
                        ucRackLayout.BobbinCarrierProductCode = item["BB_CSTPROD"].GetString();
                        ucRackLayout.BobbinCarrierProductName = item["BB_CSTPROD_NAME"].GetString();
                        ucRackLayout.BobbinCarrierCode = item["BB_CSTID"].GetString();
                        ucRackLayout.WipHold = item["WIPHOLD"].GetString();
                        ucRackLayout.CarrierDefectFlag = item["CST_DFCT_FLAG"].GetString();
                        ucRackLayout.LegendColor = item["COLOR"].GetString();
                        ucRackLayout.SkidType = item["SKID_GUBUN"].GetString();
                        ucRackLayout.AbnormalTransferReasonCode = item["ABNORM_TRF_RSN_CODE"].GetString();
                        ucRackLayout.LegendColorType = item["COLOR_GUBUN"].GetString();
                        ucRackLayout.HoldFlag = item["HOLD_FLAG"].GetString();
                        ucRackLayout.Visibility = Visibility.Visible;
                    }
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

      


        /// <summary>
        ///  Datable반환  실 Tray 조회
        /// </summary>
        /// <param name="actionCompleted"></param>
        private void SelectWareHouseProductList(Action<DataTable, Exception> actionCompleted = null)
        {
            const string bizRuleName = "BR_MCS_GET_HIGH_CNV_U_TRAY_MNT_DETL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _selectedEquipmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    actionCompleted?.Invoke(bizResult, bizException);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void MakeRackInfoTable()
        {
            _dtRackInfo = new DataTable();
            _dtRackInfo.Columns.Add("RACK_ID", typeof(string));
            _dtRackInfo.Columns.Add("STATUS", typeof(string));
            _dtRackInfo.Columns.Add("PRJT_NAME", typeof(string));
            _dtRackInfo.Columns.Add("LOTID", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTPROD", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTPROD_NAME", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTID", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTPROD", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTPROD_NAME", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTID", typeof(string));
            _dtRackInfo.Columns.Add("WIPHOLD", typeof(string));
            _dtRackInfo.Columns.Add("CST_DFCT_FLAG", typeof(string));
            _dtRackInfo.Columns.Add("SKID_GUBUN", typeof(string));
            _dtRackInfo.Columns.Add("COLOR_GUBUN", typeof(string));
            _dtRackInfo.Columns.Add("COLOR", typeof(string));
            _dtRackInfo.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
            _dtRackInfo.Columns.Add("HOLD_FLAG", typeof(string));
            _dtRackInfo.Columns.Add("SEQ", typeof(int));
        }

        private void MakeWareHouseCapacityTable()
        {
            _dtWareHouseCapacity = new DataTable();
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_NAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTID", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTNAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("RACK_MAX", typeof(decimal));      // 용량
            _dtWareHouseCapacity.Columns.Add("BBN_U_QTY", typeof(decimal));     // 실Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_UM_QTY", typeof(decimal));    // 반대극성Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_E_QTY", typeof(decimal));     // 공Carrier수
            _dtWareHouseCapacity.Columns.Add("ERROR_QTY", typeof(decimal));     // 오류Carrier수
            _dtWareHouseCapacity.Columns.Add("ABNORM_QTY", typeof(decimal));    // 정보불일치수
            _dtWareHouseCapacity.Columns.Add("RACK_RATE", typeof(double));      // 적재율
            _dtWareHouseCapacity.Columns.Add("RACK_QTY", typeof(decimal));      // 총Carrier수(실+공)
        }

        private double GetRackRate(decimal x, decimal y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private double GetRackRate(int x, int y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private int GetXPosition(string zPosition)
        {
            int xposition = _maxRowCount == Convert.ToInt16(zPosition) ? 0 : _maxRowCount - Convert.ToInt16(zPosition);
            return xposition;
        }

       /// <summary>
        /// 창고에 대한 랙의 X,Y, LAY의  최대수량 조회 - ( Layout 모니터링 화면에서 사용)
        /// </summary>
        private void SelectMaxxyz()
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_RACK_MAX_XYZ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _selectedEquipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    if (string.IsNullOrEmpty(searchResult.Rows[0][2].GetString()) || string.IsNullOrEmpty(searchResult.Rows[0][1].GetString()))
                    {
                        _maxRowCount = 0;
                        _maxColumnCount = 0;
                        return;
                    }

                    _maxRowCount = Convert.ToInt32(searchResult.Rows[0][2].GetString());
                    _maxColumnCount = Convert.ToInt32(searchResult.Rows[0][1].GetString());
                }
                else
                {
                    _maxRowCount = 0;
                    _maxColumnCount = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        /// <summary>
        /// User Control 초기화
        /// </summary>
        private void InitializeRackUserControl()
        {
            grdColumn1.Children.Clear();
            grdColumn2.Children.Clear();

            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            if (grdColumn1.ColumnDefinitions.Count > 0) grdColumn1.ColumnDefinitions.Clear();
            if (grdColumn1.RowDefinitions.Count > 0) grdColumn1.RowDefinitions.Clear();

            if (grdColumn2.ColumnDefinitions.Count > 0) grdColumn2.ColumnDefinitions.Clear();
            if (grdColumn2.RowDefinitions.Count > 0) grdColumn2.RowDefinitions.Clear();

            if (grdStair1.ColumnDefinitions.Count > 0) grdStair1.ColumnDefinitions.Clear();
            if (grdStair1.RowDefinitions.Count > 0) grdStair1.RowDefinitions.Clear();

            if (grdStair2.ColumnDefinitions.Count > 0) grdStair2.ColumnDefinitions.Clear();
            if (grdStair2.RowDefinitions.Count > 0) grdStair2.RowDefinitions.Clear();

            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();
        }

        /// <summary>
        ///  열 정보 정의 
        /// </summary>
        private void MakeColumnDefinition()
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            int colIndex = 0;

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock2, colIndex);
                grdStair2.Children.Add(textBlock2);
                colIndex++;
            }
        }

        /// <summary>
        /// 단 정보 정의 
        /// </summary>
        private void MakeRowDefinition()
        {
            // 단 Row 생성
            ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                grdColumn1.RowDefinitions.Add(rowDefinition1);
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdColumn2.RowDefinitions.Add(rowDefinition2);
            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                TextBlock textBlock1 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.SetValue(Grid.RowProperty, i);
                textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock1.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn1.Children.Add(textBlock1);

                TextBlock textBlock2 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.SetValue(Grid.RowProperty, i);
                textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock2.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn2.Children.Add(textBlock2);
            }
        }

        /// <summary>
        /// UserControl  셋팅
        /// </summary>
        private void PrepareRackStair()
        {
            _ucRackLayout1 = new UcRackLayout[_maxRowCount][];
            _ucRackLayout2 = new UcRackLayout[_maxRowCount][];

            for (int r = 0; r < _ucRackLayout1.Length; r++)
            {
                _ucRackLayout1[r] = new UcRackLayout[_maxColumnCount];
                _ucRackLayout2[r] = new UcRackLayout[_maxColumnCount];

                for (int c = 0; c < _ucRackLayout1[r].Length; c++)
                {
                    UcRackLayout ucRackLayout1 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout1.Checked += UcRackLayout1_Checked;
                    //ucPancakeRackStair1.Click += UcRackStair1_Click;
                    //ucPancakeRackStair1.DoubleClick += UcRackStair1_DoubleClick;
                    _ucRackLayout1[r][c] = ucRackLayout1;
                }

                for (int c = 0; c < _ucRackLayout2[r].Length; c++)
                {
                    UcRackLayout ucRackLayout2 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout2.Checked += UcRackLayout2_Checked;
                    //ucPancakeRackStair2.Click += UcRackStair2_Click;
                    //ucPancakeRackStair2.DoubleClick += UcRackStair2_DoubleClick;
                    _ucRackLayout2[r][c] = ucRackLayout2;
                }
            }

        }

        /// <summary>
        ///  UserControl 이 들어갈 Grid 셋팅
        /// </summary>
        private void PrepareRackStairLayout()
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();


            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
            }

            for (int row = 0; row < _maxRowCount; row++)
            {
                for (int col = 0; col < _maxColumnCount; col++)
                {
                    Grid.SetRow(_ucRackLayout1[row][col], row);
                    Grid.SetColumn(_ucRackLayout1[row][col], col);
                    grdRackstair1.Children.Add(_ucRackLayout1[row][col]);

                    Grid.SetRow(_ucRackLayout2[row][col], row);
                    Grid.SetColumn(_ucRackLayout2[row][col], col);
                    grdRackstair2.Children.Add(_ucRackLayout2[row][col]);
                }
            }
        }


        /// <summary>
        /// UserControl 리셋 - ( Layout 모니터링 화면에서 사용)
        /// </summary>
        private void ReSetLayoutUserControl()
        {
            _dtRackInfo.Clear();
            InitializeRackUserControl();

            MakeRowDefinition();
            MakeColumnDefinition();
            PrepareRackStair();
            PrepareRackStairLayout();
        }


        /// <summary>
        ///  Layout 모니터링 데이터 셋팅
        /// </summary>
        /// <param name="type"></param>
        private void GetLayoutGridColumns(string type)
        {
            for (int i = dgRackInfo.Columns.Count - 1; i >= 0; i--)
            {
                dgRackInfo.Columns.RemoveAt(i);
            }

            dgRackInfo.Refresh();

            switch (type)
            {
                case "1":
                case "3":
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "NO",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("SEQ"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_NAME",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_ID",
                        Header = ObjectDic.Instance.GetObjectName("Rack ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Rack"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "UPDDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("UPDDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MCS_CST_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("MCS_CST_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.IsReadOnly = true;
                    break;

                case "4":
                case "5":
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SEQ",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("SEQ"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_ID",
                        Header = ObjectDic.Instance.GetObjectName("Rack ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("RACK명"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTINDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("CSTINDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "BOBBIN_ID",
                        Header = ObjectDic.Instance.GetObjectName("보빈 ID"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SKID_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("SKID_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTPROD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("사용자재"),
                        Binding = new Binding() { Path = new PropertyPath("CSTPROD_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTPROD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Carrier유형"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_TYPE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    //_util.SetDataGridMergeExtensionCol(dgRackInfo, new string[] { "ELTR_TYPE_NAME"}, DataGridMergeMode.VERTICALHIERARCHI);
                    dgRackInfo.IsReadOnly = true;
                    break;


                case "2":
                    dgRackInfo.Columns.Add(new DataGridNumericColumn()
                    {
                        Name = "ROW_NUM",
                        Header = ObjectDic.Instance.GetObjectName("순위"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("RACK명"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTINDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("CSTINDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQSGNAME",
                        Header = ObjectDic.Instance.GetObjectName("LINE"),
                        Binding = new Binding() { Path = new PropertyPath("EQSGNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "BOBBIN_ID",
                        Header = ObjectDic.Instance.GetObjectName("보빈 ID"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CSTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "LOTID",
                        Header = ObjectDic.Instance.GetObjectName("LOT ID"),
                        Binding = new Binding() { Path = new PropertyPath("LOTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "LOTYNAME",
                        Header = ObjectDic.Instance.GetObjectName("LOT유형"),
                        Binding = new Binding() { Path = new PropertyPath("LOTYNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = "WIPQTY",
                        Header = ObjectDic.Instance.GetObjectName("수량"),
                        Binding = new Binding() { Path = new PropertyPath("WIPQTY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Format = "#,##0",
                        HorizontalAlignment = HorizontalAlignment.Right
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "UNIT_CODE",
                        Header = ObjectDic.Instance.GetObjectName("단위"),
                        Binding = new Binding() { Path = new PropertyPath("UNIT_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRJT_NAME",
                        Header = ObjectDic.Instance.GetObjectName("프로젝트명"),
                        Binding = new Binding() { Path = new PropertyPath("PRJT_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HALF_SLIT_SIDE",
                        Header = ObjectDic.Instance.GetObjectName("무지부"),
                        Binding = new Binding() { Path = new PropertyPath("HALF_SLIT_SIDE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ELTR_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("극성"),
                        Binding = new Binding() { Path = new PropertyPath("ELTR_TYPE_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRODID",
                        Header = ObjectDic.Instance.GetObjectName("제품"),
                        Binding = new Binding() { Path = new PropertyPath("PRODID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRODNAME",
                        Header = ObjectDic.Instance.GetObjectName("PRODNAME"),
                        Binding = new Binding() { Path = new PropertyPath("PRODNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "VLD_DATE",
                        Header = ObjectDic.Instance.GetObjectName("유효일자"),
                        Binding = new Binding() { Path = new PropertyPath("VLD_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PAST_DAY",
                        Header = ObjectDic.Instance.GetObjectName("경과일수"),
                        Binding = new Binding() { Path = new PropertyPath("PAST_DAY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PROD_VER_CODE",
                        Header = ObjectDic.Instance.GetObjectName("버전"),
                        Binding = new Binding() { Path = new PropertyPath("PROD_VER_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ELTR_GRD_CODE",
                        Header = ObjectDic.Instance.GetObjectName("ELTR_GRD_CODE"),
                        Binding = new Binding() { Path = new PropertyPath("ELTR_GRD_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "IQC_JUDGEMENT",
                        Header = ObjectDic.Instance.GetObjectName("IQC 검사결과"),
                        Binding = new Binding() { Path = new PropertyPath("IQC_JUDGEMENT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "QMS_JUDGEMENT_CT",
                        Header = ObjectDic.Instance.GetObjectName("QMS_JUDGEMENT_CT"),
                        Binding = new Binding() { Path = new PropertyPath("QMS_JUDGEMENT_CT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "QMS_JUDGEMENT_RP",
                        Header = ObjectDic.Instance.GetObjectName("QMS_JUDGEMENT_RP"),
                        Binding = new Binding() { Path = new PropertyPath("QMS_JUDGEMENT_RP"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "QMS_JUDGEMENT_ST",
                        Header = ObjectDic.Instance.GetObjectName("QMS_JUDGEMENT_ST"),
                        Binding = new Binding() { Path = new PropertyPath("QMS_JUDGEMENT_ST"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "VD_QA_RESULT",
                        Header = ObjectDic.Instance.GetObjectName("VD검사결과"),
                        Binding = new Binding() { Path = new PropertyPath("VD_QA_RESULT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SPCL_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("특별관리여부"),
                        Binding = new Binding() { Path = new PropertyPath("SPCL_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RSV_EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("목적지 설비명"),
                        Binding = new Binding() { Path = new PropertyPath("RSV_EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "WIPHOLD",
                        Header = ObjectDic.Instance.GetObjectName("HOLD 여부"),
                        Binding = new Binding() { Path = new PropertyPath("WIPHOLD"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD사유"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_NOTE",
                        Header = ObjectDic.Instance.GetObjectName("HOLD비고"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_NOTE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_DTTM",
                        Header = ObjectDic.Instance.GetObjectName("HOLD시간"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_DTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_USERNAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD등록자"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_USERNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ACTION_USERNAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD담당자"),
                        Binding = new Binding() { Path = new PropertyPath("ACTION_USERNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_HOLD_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("설비 보류 유형 코드"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_HOLD_TYPE_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_HOLD_CNFM_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("설비 보류 확인 여부"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_HOLD_CNFM_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = "DFCT_TAG_QTY",
                        Header = ObjectDic.Instance.GetObjectName("불량태그수"),
                        Binding = new Binding() { Path = new PropertyPath("DFCT_TAG_QTY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Format = "#,##0",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.IsReadOnly = true;

                    dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Collapsed;

                    //if (cboStockerType.SelectedValue.GetString() == "NWW")
                    //{
                    //    dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;

                    //    if (LoginInfo.CFG_AREA_ID == "A7")
                    //    {
                    //        dgRackInfo.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Visible;
                    //    }
                    //}
                    //else if (cboStockerType.SelectedValue.GetString() == "JRW")
                    //{
                    //    dgRackInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                    //    dgRackInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                    //    dgRackInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                    //}
                    //else if (cboStockerType.SelectedValue.GetString() == "PCW")
                    //{
                    //    dgRackInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                    //    dgRackInfo.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Visible;
                    //    dgRackInfo.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Visible;
                    //}
                    //if (cboStockerType.SelectedValue.GetString() == "NPW" || cboStockerType.SelectedValue.GetString() == "LWW")
                    //{
                    //    dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;

                    //    if (cboStockerType.SelectedValue.GetString() == "LWW")
                    //    {
                    //        if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "A8" || LoginInfo.CFG_AREA_ID == "S4")
                    //            dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                    //    }
                    //}


                    break;

            }
        }

        private void SelectLayoutGrid(string type)
        {
            switch (type)
            {
                case "1":
                case "2":   // 실보빈(LOT존재)
                    SelectWareHouseProductList(dgProduct);
                    break;
                case "3":   // 오류Carrier
                    SelectErrorCarrierList(dgRackInfo);
                    break;
                case "4":   // 공 Carrier
                case "5":
                    SelectWareHouseEmptyCarrierList(dgRackInfo);
                    break;
            }
        }


        private void HideAndClearAllRack()
        {
            for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout1[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout1[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout2[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout2[row][col].Clear();
                }
            }
        }

        private void UnCheckedAllRackLayout()
        {
            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout1[rowIndex][colIndex];

                    if (ucRackLayout.IsChecked)
                    {
                        ucRackLayout.IsChecked = false;
                        UnCheckUcRackLayout(ucRackLayout);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout2[rowIndex][colIndex];

                    if (ucRackLayout.IsChecked)
                    {
                        ucRackLayout.IsChecked = false;
                        UnCheckUcRackLayout(ucRackLayout);
                    }
                }
            }
        }

        private void UnCheckUcRackLayout(UcRackLayout ucRackLayout)
        {
            DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID = '" + ucRackLayout.RackId + "'");
            foreach (DataRow row in selectedRow)
            {
                _dtRackInfo.Rows.Remove(row);
            }
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }

        private void CheckUcRackLayout(UcRackLayout rackLayout)
        {

            if (CommonVerify.HasTableRow(_dtRackInfo))
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }

            _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
            _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
            _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;

            int maxSeq = 1;
            DataRow dr = _dtRackInfo.NewRow();
            dr["RACK_ID"] = rackLayout.RackId;
            dr["STATUS"] = rackLayout.RackStateCode;
            dr["PRJT_NAME"] = rackLayout.ProjectName;
            dr["LOTID"] = rackLayout.LotId;
            dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
            dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
            dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
            dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
            dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
            dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
            dr["WIPHOLD"] = rackLayout.WipHold;
            dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
            dr["SKID_GUBUN"] = rackLayout.SkidType;
            dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
            dr["COLOR"] = rackLayout.LegendColor;
            dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
            dr["HOLD_FLAG"] = rackLayout.HoldFlag;
            dr["SEQ"] = maxSeq;
            _dtRackInfo.Rows.Add(dr);

            if (rackLayout.LegendColorType == "4")
            {
                _selectedSkIdIdByRackInfo = null;
            }
            else if (rackLayout.LegendColorType == "5")
            {
                _selectedBobbinIdByRackInfo = null;
            }

            GetLayoutGridColumns(rackLayout.LegendColorType);
            SelectLayoutGrid(rackLayout.LegendColorType);

        }




        private bool IsTabStatusbyWorkorderVisibility(SearchType searchType, string stockerTypeCode)
        {

            if (string.IsNullOrEmpty(stockerTypeCode)) return false;

            const string bizRuleName = "BR_MCS_SEL_AREA_COM_CODE_FOR_UI";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["COM_TYPE_CODE"] = "AREA_EQUIPMENT_GROUP_UI";
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_CODE"] = stockerTypeCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (searchType == SearchType.Tab)
                    {
                        if (dtResult.Rows[0]["ATTR1"].GetString() == "Y")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (dtResult.Rows[0]["ATTR2"].GetString() == "Y")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }


        #endregion

        #region 공 트레이 팝업 Validation : ValidationEmptyTray()
        /// <summary>
        /// 공 Tray Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationEmptyTray()
        {

            if (dgEmptyCarrier.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다.
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgEmptyCarrier, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgEmptyCarrier))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }


        #endregion

        #region 오류 트레이 팝업 Validation : ValidationErrorTray()
        /// <summary>
        /// 오류 Tray Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationErrorTray()
        {

            if (dgErrorCarrier.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다.
                return false;
            }

            if (_util.GetDataGridFirstRowIndexByCheck(dgErrorCarrier, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgErrorCarrier))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }


        #endregion

        #region 타이머 셋팅  : TimerSetting()
        /// <summary>
        /// Timer
        /// </summary>
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();
            }
        }

        #endregion

        #endregion

    }
}
