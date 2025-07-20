/*************************************************************************************
 Created Date : 2021.05.19
      Creator : 오화백
   Decription : 창고별 적재율
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.19  오화백      : Initial Created.    
  2021.08.20  오화백      : 스크롤 제거 및 창고에 대한 상세 LOT 정보 팝업 추가
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
    public partial class MCS001_063 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //private readonly 
        private readonly Util _util = new Util();
        private bool _isLoaded = false;
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;


        public MCS001_063()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            TimerSetting();
            Loaded -= UserControl_Loaded;
         
            _isLoaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
         
        }


        #endregion

        #region Event
   
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
            SelectCapacityAll();
        }

        #endregion

        #region 스프레드 색깔 표시 이벤트 : dgElecWareHouse_LoadedCellPresenter(), dgElecWareHouse_UnloadedCellPresenter()
        private void dgElecWareHouse_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    //if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    //{
                    //    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                    //    if (convertFromString != null)
                    //        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    //}

                    //else
                    //{
                    //    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                    //    if (convertFromString != null)
                    //        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    //}

                    if (string.Equals(e.Cell.Column.Name, "RATE_TEXT") ) // 적재율 90%이상 노랑색, 95%이상 빨간색
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 90 && (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 95))
                        {

                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Cursor = Cursors.Arrow;
               
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 95 )
                        {

                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Cursor = Cursors.Arrow;

                        }
                        else
                        {
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.Cursor = Cursors.Arrow;
                        }
                    }
                    if (string.Equals(e.Cell.Column.Name, "U_CNT_TEXT")) // 실수량 클릭시
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                 }
            }));
        }

        private void dgElecWareHouse_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region Lami 대기창고 스프레드 머지: dgLamiWaitWareHouse_MergingCells()

        private void dgLamiWaitWareHouse_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        equipmentCode = x.Field<string>("EQPTID")
                    }).Select(g => new
                    {
                        EquipmentCode = g.Key.equipmentCode,
                        Count = g.Count()
                    }).ToList();

                    string previewEquipmentCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        foreach (var item in query)
                        {
                            int rowIndex = i;
                            if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString() == item.EquipmentCode && previewEquipmentCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString())
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPTNAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_CNT_TEXT"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_CNT_TEXT"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["E_CNT_TEXT"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["E_CNT_TEXT"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ER_CNT_TEXT"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ER_CNT_TEXT"].Index)));
                                //e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ABNORM_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ABNORM_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RATE_TEXT"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RATE_TEXT"].Index)));
                            }
                        }
                        previewEquipmentCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 전극 대기창고 상세 LOT 팝업 호출 :  dgElecWareHouse_MouseLeftButtonUp()
        /// <summary>
        /// 전극 대기창고 팝업 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgElecWareHouse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || !cell.Column.Name.Equals("U_CNT_TEXT")) return;
                if (cell.Text.Replace(",", "").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_063_LOTLIST popupLotlist = new MCS001_063_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[6];
                parameters[0] = "ED";     
                parameters[1] = "PCW";
                parameters[2] = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                parameters[3] = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                parameters[4] = string.Empty;
                parameters[5] = "N";
                C1WindowExtension.SetParameters(popupLotlist, parameters);

                popupLotlist.Closed += popupLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 노칭 대기창고 상세 LOT 팝업 호출 : dgNNDWaitWareHouse_MouseLeftButtonUp()
        /// <summary>
        /// 노칭대기창고 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgNNDWaitWareHouse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || !cell.Column.Name.Equals("U_CNT_TEXT")) return;
                if (cell.Text.Replace(",", "").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_063_LOTLIST popupLotlist = new MCS001_063_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[6];
                parameters[0] = "A7";


                if (DataTableConverter.GetValue(drv, "EQPTID").GetString() == "A7STK108" || DataTableConverter.GetValue(drv, "EQPTID").GetString() == "A7STK104")
                {
                    parameters[1] = "MNW";
                }
                else
                {
                    parameters[1] = "NWW";
                }
                parameters[2] = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                parameters[3] = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                parameters[4] = string.Empty;
                parameters[5] = "N";
                C1WindowExtension.SetParameters(popupLotlist, parameters);

                popupLotlist.Closed += popupLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 노칭 완공창고 상세 LOT 팝업 호출 : dgNNDWareHouse_MouseLeftButtonUp()

        /// <summary>
        /// 노칭완공창고 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgNNDWareHouse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || !cell.Column.Name.Equals("U_CNT_TEXT")) return;
                if (cell.Text.Replace(",", "").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_063_LOTLIST popupLotlist = new MCS001_063_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[6];
                parameters[0] = "A7";
                parameters[1] = "NPW";
                parameters[2] = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                parameters[3] = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                parameters[4] = string.Empty;
                parameters[5] = "N";
                C1WindowExtension.SetParameters(popupLotlist, parameters);

                popupLotlist.Closed += popupLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Tray 창고 상세 LOT 팝업 호출 : dgTrauStocker_MouseLeftButtonUp()
        /// <summary>
        /// Tray 창고  팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTrauStocker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || !cell.Column.Name.Equals("U_CNT_TEXT")) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_063_LOTLIST popupLotlist = new MCS001_063_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[6];
                parameters[0] = "A7";
                parameters[1] = "STO";
                parameters[2] = string.Empty;
                parameters[3] = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                parameters[4] = string.Empty;
                parameters[5] = "Y";
                C1WindowExtension.SetParameters(popupLotlist, parameters);

                popupLotlist.Closed += popupLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 라미대기창고 상세 LOT 팝업 호출 : dgLamiWaitWareHouse_MouseLeftButtonUp()
        /// <summary>
        /// 라미대기 창고 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLamiWaitWareHouse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || !cell.Column.Name.Equals("U_CNT_TEXT")) return;
                if (cell.Text.Replace(",", "").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_063_LOTLIST popupLotlist = new MCS001_063_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[6];
                parameters[0] = "A7";
                parameters[1] = "LWW";
                parameters[2] = string.Empty;
                parameters[3] = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                parameters[4] = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                parameters[5] = "N";
                C1WindowExtension.SetParameters(popupLotlist, parameters);

                popupLotlist.Closed += popupLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 팝업닫기 : popupLotlist_Closed()
        /// <summary>
        /// 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupLotlist_Closed(object sender, EventArgs e)
        {
            MCS001_063_LOTLIST popup = sender as MCS001_063_LOTLIST;
            if (popup != null)
            {

            }
        }
        #endregion
      
        #endregion

        #region Method

        #region 전체 적재율 조회 : SelectCapacityAll()
        /// <summary>
        /// 전극창고 조회
        /// </summary>
        private void SelectCapacityAll()
        {
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_CAPACITY_SUMMARY_ALL";
            try
            {
                ShowLoadingIndicator();
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
            

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
       
                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTDATA_ELEC,OUTDATA_NNDW,OUTDATA_NNDO,OUTDATA_LAMW,OUTDATA_TRAW", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //전극창고 적용
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTDATA_ELEC"] != null)
                        {
                            SelectElecWarehouse(bizResult.Tables["OUTDATA_ELEC"]);

                        }
                        // NND 대기창고 조회 
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTDATA_NNDW"] != null)
                        {
                            SelecNNDWaitWareHouse(bizResult.Tables["OUTDATA_NNDW"]);
                        }
                        //NND 완성창고
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTDATA_NNDO"] != null)
                        {
                            SelectNNDWareHouse(bizResult.Tables["OUTDATA_NNDO"]);
                        }
                        //Tray Stocker
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTDATA_TRAW"] != null)
                        {
                            SelectTrayStock(bizResult.Tables["OUTDATA_TRAW"]);
                        }
                        //Lami 대기창고
                        if (bizResult != null && bizResult.Tables.Count > 0 && bizResult.Tables["OUTDATA_LAMW"] != null)
                        {
                            SelectLamiWaitWareHouse(bizResult.Tables["OUTDATA_LAMW"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion
        
        #region 전극 창고 적용 : SelectElecWarehouse()
        private void SelectElecWarehouse(DataTable dt)
        {
            dt.Columns.Add("RACK_CNT_TEXT", typeof(string)); //용량
            dt.Columns.Add("U_CNT_TEXT", typeof(string)); //실Tray수
            dt.Columns.Add("E_CNT_TEXT", typeof(string)); //공Tray수
            dt.Columns.Add("ETC_CNT_TEXT", typeof(string)); //기타
            dt.Columns.Add("RATE_TEXT", typeof(string)); //적재율

            if (CommonVerify.HasTableRow(dt))
            {

                Int32 _uBundel = 0;
                Int32 _eBundel = 0;
                Int32 _rCnt = 0;


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["RACK_CNT_TEXT"] = dt.Rows[i]["RACK_MAX"].GetString(); //용량
                    dt.Rows[i]["U_CNT_TEXT"] = dt.Rows[i]["CARRIER_U_QTY"].GetString(); //실Tray수
                    dt.Rows[i]["E_CNT_TEXT"] = dt.Rows[i]["CARRIER_E_QTY"].GetString(); //공Tray수
                    dt.Rows[i]["ETC_CNT_TEXT"] = dt.Rows[i]["CARRIER_ETC"].GetString();  //기타
                    _uBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["CARRIER_U_QTY"].GetString()) ? 0 : dt.Rows[i]["CARRIER_U_QTY"]);
                    _eBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["CARRIER_E_QTY"].GetString()) ? 0 : dt.Rows[i]["CARRIER_E_QTY"]);
                    _rCnt = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["RACK_MAX"].GetString()) ? 0 : dt.Rows[i]["RACK_MAX"]);

                    //bizResult.Rows[i]["RATE_TEXT"] = Math.Round(Convert.ToDouble(string.IsNullOrEmpty(bizResult.Rows[i]["RATE"].ToString()) ? 0 : bizResult.Rows[i]["RATE"]), 1).ToString("0.##") + " %";
                    dt.Rows[i]["RATE_TEXT"] = Convert.ToDouble(dt.Rows[i]["RACK_RATE"].GetString()).ToString("0.##");  //dt.Rows[i]["RATE_TEXT"] = GetPercentage(_uBundel + _eBundel, _rCnt).ToString("0.##");
                }

                //var query = dt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                //{
                //    EquipmentSegmentCode = string.Empty,
                //    EquipmentSegmentName = ObjectDic.Instance.GetObjectName("합계"),
                //    RackCount = g.Sum(x => x.Field<decimal>("RACK_MAX")),            //용량
                //    U_BundleCount = g.Sum(x => x.Field<decimal>("CARRIER_U_QTY")),    //실 TRAY 수
                //    E_BundleCount = g.Sum(x => x.Field<decimal>("CARRIER_E_QTY")),    //공 TRAY 수         
                //    ETC_CNT_COUNT = g.Sum(x => x.Field<decimal>("CARRIER_ETC")),      //기타

                //    Count = g.Count()
                //}).FirstOrDefault();

                //if (query != null)
                //{
                //    DataRow newRow = dt.NewRow();
                //    newRow["EQPTID"] = query.EquipmentSegmentCode;
                //    newRow["EQPTNAME"] = query.EquipmentSegmentName;
                //    newRow["RACK_CNT_TEXT"] = query.RackCount.GetString();
                //    newRow["U_CNT_TEXT"] = query.U_BundleCount.GetString();
                //    newRow["E_CNT_TEXT"] = query.E_BundleCount.GetString();
                //    newRow["ETC_CNT_TEXT"] = query.ETC_CNT_COUNT.GetString();
                //    newRow["RATE_TEXT"] = GetPercentage(Convert.ToInt32(query.U_BundleCount) + Convert.ToInt32(query.E_BundleCount), Convert.ToInt32(query.RackCount)).ToString("0.##") + " %";
                //    dt.Rows.Add(newRow);
                //}

                Util.GridSetData(dgElecWareHouse, dt, null, true);
            }
           
        }
        #endregion

        #region NND 대기창고 적용 : SelecNNDWaitWareHouse()
        /// <summary>
        /// NND 대기창고 조회
        /// </summary>
        private void SelecNNDWaitWareHouse(DataTable dt)
        {
            dt.Columns.Add("RACK_CNT_TEXT", typeof(string)); //용량
            dt.Columns.Add("U_CNT_TEXT", typeof(string)); //실Tray수
            dt.Columns.Add("E_CNT_TEXT", typeof(string)); //공Tray수
            dt.Columns.Add("ETC_CNT_TEXT", typeof(string)); //기타
            dt.Columns.Add("RATE_TEXT", typeof(string)); //적재율

            if (CommonVerify.HasTableRow(dt))
            {

                Int32 _uBundel = 0;
                Int32 _eBundel = 0;
                Int32 _rCnt = 0;


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["RACK_CNT_TEXT"] = dt.Rows[i]["RACK_MAX"].GetString(); //용량
                    dt.Rows[i]["U_CNT_TEXT"] = dt.Rows[i]["CARRIER_U_QTY"].GetString(); //실Tray수
                    dt.Rows[i]["E_CNT_TEXT"] = dt.Rows[i]["CARRIER_E_QTY"].GetString(); //공Tray수
                    dt.Rows[i]["ETC_CNT_TEXT"] = dt.Rows[i]["CARRIER_ETC"].GetString();  //기타
                    _uBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["CARRIER_U_QTY"].GetString()) ? 0 : dt.Rows[i]["CARRIER_U_QTY"]);
                    _eBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["CARRIER_E_QTY"].GetString()) ? 0 : dt.Rows[i]["CARRIER_E_QTY"]);
                    _rCnt = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["RACK_MAX"].GetString()) ? 0 : dt.Rows[i]["RACK_MAX"]);

                    //bizResult.Rows[i]["RATE_TEXT"] = Math.Round(Convert.ToDouble(string.IsNullOrEmpty(bizResult.Rows[i]["RATE"].ToString()) ? 0 : bizResult.Rows[i]["RATE"]), 1).ToString("0.##") + " %";
                    dt.Rows[i]["RATE_TEXT"] = Convert.ToDouble(dt.Rows[i]["RACK_RATE"].GetString()).ToString("0.##");  //dt.Rows[i]["RATE_TEXT"] = GetPercentage(_uBundel + _eBundel, _rCnt).ToString("0.##");
                }

                //var query = dt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                //{
                //    EquipmentSegmentCode = string.Empty,
                //    EquipmentSegmentName = ObjectDic.Instance.GetObjectName("합계"),
                //    RackCount = g.Sum(x => x.Field<decimal>("RACK_MAX")),            //용량
                //    U_BundleCount = g.Sum(x => x.Field<decimal>("CARRIER_U_QTY")),    //실 TRAY 수
                //    E_BundleCount = g.Sum(x => x.Field<decimal>("CARRIER_E_QTY")),    //공 TRAY 수         
                //    ETC_CNT_COUNT = g.Sum(x => x.Field<decimal>("CARRIER_ETC")),      //기타

                //    Count = g.Count()
                //}).FirstOrDefault();

                //if (query != null)
                //{
                //    DataRow newRow = dt.NewRow();
                //    newRow["EQPTID"] = query.EquipmentSegmentCode;
                //    newRow["EQPTNAME"] = query.EquipmentSegmentName;
                //    newRow["RACK_CNT_TEXT"] = query.RackCount.GetString();
                //    newRow["U_CNT_TEXT"] = query.U_BundleCount.GetString();
                //    newRow["E_CNT_TEXT"] = query.E_BundleCount.GetString();
                //    newRow["ETC_CNT_TEXT"] = query.ETC_CNT_COUNT.GetString();
                //    newRow["RATE_TEXT"] = GetPercentage(Convert.ToInt32(query.U_BundleCount) + Convert.ToInt32(query.E_BundleCount), Convert.ToInt32(query.RackCount)).ToString("0.##") + " %";
                //    dt.Rows.Add(newRow);
                //}

                DataTable dtSortTable = null;
                dtSortTable = dt.Select("", "ELTR_TYPE_CODE, EQPTID").CopyToDataTable<DataRow>(); //라인으로 정렬     


                Util.GridSetData(dgNNDWaitWareHouse, dtSortTable, null, true);

                //_util.SetDataGridMergeExtensionCol(dgNNDWaitWareHouse, new string[] { "ELTR_TYPE_NAME", "EQPTNAME" }, ControlsLibrary.DataGridMergeMode.VERTICALHIERARCHI);
            }
        }

        #endregion
  
        #region NND 완성창고 적용 : SelectNNDWareHouse()

        /// <summary>
        /// NND 완성창고
        /// </summary>
        private void SelectNNDWareHouse(DataTable dt)
        {
            dt.Columns.Add("RACK_CNT_TEXT", typeof(string)); //용량
            dt.Columns.Add("U_CNT_TEXT", typeof(string)); //실Tray수
            dt.Columns.Add("E_CNT_TEXT", typeof(string)); //공Tray수
            dt.Columns.Add("ETC_CNT_TEXT", typeof(string)); //기타
            dt.Columns.Add("RATE_TEXT", typeof(string)); //적재율

            if (CommonVerify.HasTableRow(dt))
            {

                Int32 _uBundel = 0;
                Int32 _eBundel = 0;
                Int32 _rCnt = 0;


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["RACK_CNT_TEXT"] = dt.Rows[i]["RACK_MAX"].GetString(); //용량
                    dt.Rows[i]["U_CNT_TEXT"] = dt.Rows[i]["CARRIER_U_QTY"].GetString(); //실Tray수
                    dt.Rows[i]["E_CNT_TEXT"] = dt.Rows[i]["CARRIER_E_QTY"].GetString(); //공Tray수
                    dt.Rows[i]["ETC_CNT_TEXT"] = dt.Rows[i]["CARRIER_ETC"].GetString();  //기타
                    _uBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["CARRIER_U_QTY"].GetString()) ? 0 : dt.Rows[i]["CARRIER_U_QTY"]);
                    _eBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["CARRIER_E_QTY"].GetString()) ? 0 : dt.Rows[i]["CARRIER_E_QTY"]);
                    _rCnt = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["RACK_MAX"].GetString()) ? 0 : dt.Rows[i]["RACK_MAX"]);

                    //bizResult.Rows[i]["RATE_TEXT"] = Math.Round(Convert.ToDouble(string.IsNullOrEmpty(bizResult.Rows[i]["RATE"].ToString()) ? 0 : bizResult.Rows[i]["RATE"]), 1).ToString("0.##") + " %";
                    dt.Rows[i]["RATE_TEXT"] = Convert.ToDouble(dt.Rows[i]["RACK_RATE"].GetString()).ToString("0.##");  //dt.Rows[i]["RATE_TEXT"] = GetPercentage(_uBundel + _eBundel, _rCnt).ToString("0.##");
                }

                //var query = dt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                //{
                //    EquipmentSegmentCode = string.Empty,
                //    EquipmentSegmentName = ObjectDic.Instance.GetObjectName("합계"),
                //    RackCount = g.Sum(x => x.Field<decimal>("RACK_MAX")),            //용량
                //    U_BundleCount = g.Sum(x => x.Field<decimal>("CARRIER_U_QTY")),    //실 TRAY 수
                //    E_BundleCount = g.Sum(x => x.Field<decimal>("CARRIER_E_QTY")),    //공 TRAY 수         
                //    ETC_CNT_COUNT = g.Sum(x => x.Field<decimal>("CARRIER_ETC")),      //기타

                //    Count = g.Count()
                //}).FirstOrDefault();

                //if (query != null)
                //{
                //    DataRow newRow = dt.NewRow();
                //    newRow["EQPTID"] = query.EquipmentSegmentCode;
                //    newRow["EQPTNAME"] = query.EquipmentSegmentName;
                //    newRow["RACK_CNT_TEXT"] = query.RackCount.GetString();
                //    newRow["U_CNT_TEXT"] = query.U_BundleCount.GetString();
                //    newRow["E_CNT_TEXT"] = query.E_BundleCount.GetString();
                //    newRow["ETC_CNT_TEXT"] = query.ETC_CNT_COUNT.GetString();
                //    newRow["RATE_TEXT"] = GetPercentage(Convert.ToInt32(query.U_BundleCount) + Convert.ToInt32(query.E_BundleCount), Convert.ToInt32(query.RackCount)).ToString("0.##") + " %";
                //    dt.Rows.Add(newRow);
                //}

                Util.GridSetData(dgNNDWareHouse, dt, null, true);
            }


        }


        #endregion

        #region  Tray Stocker 적용 : SelectTrayStock()


        /// <summary>
        /// Tray Strocker
        /// </summary>
        private void SelectTrayStock(DataTable dt)
        {
            dt.Columns.Add("RACK_CNT_TEXT", typeof(string)); //용량
            dt.Columns.Add("U_CNT_TEXT", typeof(string)); //실Tray수
            dt.Columns.Add("E_CNT_TEXT", typeof(string)); //공Tray수
            dt.Columns.Add("ETC_CNT_TEXT", typeof(string)); //기타
            dt.Columns.Add("RATE_TEXT", typeof(string)); //적재율

            if (CommonVerify.HasTableRow(dt))
            {

                Int32 _uBundel = 0;
                Int32 _eBundel = 0;
                Int32 _rCnt = 0;


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["RACK_CNT_TEXT"] = dt.Rows[i]["TOTAL_RACK_CNT"].GetString(); //용량
                    dt.Rows[i]["U_CNT_TEXT"] = dt.Rows[i]["U_BUNDLE_CNT"].GetString() +" (" + dt.Rows[i]["USING_CNT"].GetString() + " )"; //실Tray수
                    dt.Rows[i]["E_CNT_TEXT"] = dt.Rows[i]["E_BUNDLE_CNT"].GetString() + " (" + dt.Rows[i]["EMPTIED_CNT"].GetString() + " )"; //공Tray수
                    dt.Rows[i]["ETC_CNT_TEXT"] = dt.Rows[i]["ETC_CNT"].GetString();  //기타
                    _uBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["USING_CNT"].GetString()) ? 0 : dt.Rows[i]["USING_CNT"]);
                    _eBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["EMPTIED_CNT"].GetString()) ? 0 : dt.Rows[i]["EMPTIED_CNT"]);
                    _rCnt = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["TOTAL_RACK_CNT"].GetString()) ? 0 : dt.Rows[i]["TOTAL_RACK_CNT"]);

                    dt.Rows[i]["RATE_TEXT"] = Convert.ToDouble(dt.Rows[i]["USING_RATE"].GetString()).ToString("0.##");  //dt.Rows[i]["RATE_TEXT"] = GetPercentage(_uBundel + _eBundel, _rCnt).ToString("0.##");
                }

                //var query = dt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                //{
                //    EquipmentSegmentCode = string.Empty,
                //    EquipmentSegmentName = ObjectDic.Instance.GetObjectName("합계"),
                //    RackCount = g.Sum(x => x.Field<decimal>("TOTAL_RACK_CNT")),    //용량
                //    U_BundleCount = g.Sum(x => x.Field<decimal>("USING_CNT")),     //실 TRAY 수
                //    E_BundleCount = g.Sum(x => x.Field<decimal>("EMPTIED_CNT")),   //공 TRAY 수         
                //    ETC_CNT_COUNT = g.Sum(x => x.Field<decimal>("ETC_CNT")),       //기타

                //    Count = g.Count()
                //}).FirstOrDefault();

                //if (query != null)
                //{
                //    DataRow newRow = dt.NewRow();
                //    newRow["EQPTID"] = query.EquipmentSegmentCode;
                //    newRow["EQPTNAME"] = query.EquipmentSegmentName;
                //    newRow["RACK_CNT_TEXT"] = query.RackCount.GetString();
                //    newRow["U_CNT_TEXT"] = query.U_BundleCount.GetString();
                //    newRow["E_CNT_TEXT"] = query.E_BundleCount.GetString();
                //    newRow["ETC_CNT_TEXT"] = query.ETC_CNT_COUNT.GetString();
                //    newRow["RATE_TEXT"] = GetPercentage(Convert.ToInt32(query.U_BundleCount) + Convert.ToInt32(query.E_BundleCount), Convert.ToInt32(query.RackCount)).ToString("0.##") + " %";
                //    dt.Rows.Add(newRow);
                //}

                Util.GridSetData(dgTrauStocker, dt, null, true);
            }
    
        }

        #endregion

        #region Lami 대기창고 : SelectLamiWaitWareHouse()
        /// <summary>
        /// 라미대기 창고 조회
        /// </summary>
        private void SelectLamiWaitWareHouse(DataTable dt)
        {
            dt.Columns.Add("RACK_CNT_TEXT", typeof(string)); //용량
            dt.Columns.Add("U_CNT_TEXT", typeof(string)); //실Tray수
            dt.Columns.Add("E_CNT_TEXT", typeof(string)); //공Tray수
            dt.Columns.Add("ER_CNT_TEXT", typeof(string)); //오류Tray수
            dt.Columns.Add("A_CNT_TEXT", typeof(string)); //정보불일치 Tray수
            dt.Columns.Add("RATE_TEXT", typeof(string)); //적재율

            if (CommonVerify.HasTableRow(dt))
            {

                Int32 _uBundel = 0; //실 
                Int32 _eBundel = 0; //공
                Int32 _erBundel = 0; //오류

                Int32 _rCnt = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["RACK_CNT_TEXT"] = dt.Rows[i]["RACK_MAX_QTY"].GetString(); //용량
                    dt.Rows[i]["U_CNT_TEXT"] = dt.Rows[i]["LOT_QTY"].GetString(); //실Tray수
                    dt.Rows[i]["E_CNT_TEXT"] = dt.Rows[i]["EMPTY_QTY"].GetString(); //공Tray수
                    dt.Rows[i]["ER_CNT_TEXT"] = dt.Rows[i]["ERROR_QTY"].GetString() + " / " + dt.Rows[i]["ABNORM_QTY"].GetString(); //오류Tray수 + 정보불일치

                    _uBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["LOT_QTY"].GetString()) ? 0 : dt.Rows[i]["LOT_QTY"]);
                    _eBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["EMPTY_QTY"].GetString()) ? 0 : dt.Rows[i]["EMPTY_QTY"]);
                    _erBundel = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["ERROR_QTY"].GetString()) ? 0 : dt.Rows[i]["ERROR_QTY"]);
                    _rCnt = Convert.ToInt32(string.IsNullOrEmpty(dt.Rows[i]["RACK_MAX_QTY"].GetString()) ? 0 : dt.Rows[i]["RACK_MAX_QTY"]);

                    dt.Rows[i]["RATE_TEXT"] = Convert.ToDouble(dt.Rows[i]["RACK_RATE"].GetString()).ToString("0.##");                  //GetPercentage(_uBundel + _eBundel, _rCnt).ToString("0.##");
                }

                //var query = dt.AsEnumerable().GroupBy(x => new { }).Select(g => new
                //{
                //    EquipmentSegmentCode = string.Empty,
                //    EquipmentSegmentName = ObjectDic.Instance.GetObjectName("합계"),
                //    RackCount = g.Sum(x => x.Field<decimal>("RACK_MAX_QTY")),     //용량
                //    U_BundleCount = g.Sum(x => x.Field<decimal>("LOT_QTY")),      //실 TRAY 수
                //    E_BundleCount = g.Sum(x => x.Field<decimal>("EMPTY_QTY")),    //공 TRAY 수         
                //    ER_BundleCount = g.Sum(x => x.Field<decimal>("ERROR_QTY")),   //오류 TRAY 수      
                //    A_CNT_COUNT = g.Sum(x => x.Field<decimal>("ABNORM_QTY")),      //정보불일치

                //    Count = g.Count()
                //}).FirstOrDefault();

                //if (query != null)
                //{
                //    DataRow newRow = dt.NewRow();
                //    newRow["EQPTID"] = query.EquipmentSegmentCode;
                //    newRow["EQPTNAME"] = query.EquipmentSegmentName;
                //    newRow["RACK_CNT_TEXT"] = query.RackCount.GetString();
                //    newRow["U_CNT_TEXT"] = query.U_BundleCount.GetString();
                //    newRow["E_CNT_TEXT"] = query.E_BundleCount.GetString();
                //    newRow["ER_CNT_TEXT"] = query.ER_BundleCount.GetString();
                //    newRow["A_CNT_TEXT"] = query.A_CNT_COUNT.GetString();
                //    newRow["RATE_TEXT"] = GetPercentage(Convert.ToInt32(query.U_BundleCount) + Convert.ToInt32(query.E_BundleCount) + Convert.ToInt32(query.ER_BundleCount), Convert.ToInt32(query.RackCount)).ToString("0.##") + " %";
                //    dt.Rows.Add(newRow);
                //}

                Util.GridSetData(dgLamiWaitWareHouse, dt, null, true);
            }

        }

        #endregion

        #region 컨트롤 초기화 : ClearControl()
        /// <summary>
        /// 컨트롤 초기화
        /// </summary>
        private void ClearControl()
        {
            
            Util.gridClear(dgElecWareHouse);
            Util.gridClear(dgNNDWaitWareHouse);
            Util.gridClear(dgNNDWareHouse);
            Util.gridClear(dgTrauStocker);
            Util.gridClear(dgLamiWaitWareHouse);
        
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
