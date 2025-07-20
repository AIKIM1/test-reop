/*************************************************************************************
 Created Date : 2020.04.09
      Creator : 신광희
   Decription : 고공 컨베이어 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.09  신광희 차장 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_042.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_042 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        private string _selectedEquipmentSegmentCode;
        private string _selectedCstTypeCode;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        //private const string _bizIp = "10.32.169.224";
        //private const string _bizPort = "7865";
        //private const string _bizIndex = "0";
        //private string[] _bizInfo = { _bizIp, _bizPort, _bizIndex };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public MCS001_042()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            
            InitializeControl();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            _isLoaded = true;
            Util.GridAllColumnWidthAuto(ref dgConveyor);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            SelectConveyorSummary();
        }

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
                        Util.MessageValidation("SFU8170");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        //자동조회  %1초로 변경 되었습니다.
                        if (cboTimer != null)
                            Util.MessageInfo("SFU5127", cboTimer.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgConveyor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Convert.ToString(e.Cell.Column.Name) == "T_CNT_TEXT" || Convert.ToString(e.Cell.Column.Name) == "U_CNT_TEXT" || Convert.ToString(e.Cell.Column.Name) == "E_CNT_TEXT" || Convert.ToString(e.Cell.Column.Name) == "O_CNT_TEXT")
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    //}
                    //else
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    //    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    //}

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

                SelectConveyorDetail();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(btnSearch, null);
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

        #region Method

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

        private void InitializeControl()
        {
            SetLevelCombo(cboLevel);

            if (LoginInfo.LANGID == "ko-KR")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.4, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.6, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "zh-CN")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.4, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.6, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "en-US")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.8, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.2, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "uk-UA")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.8, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.2, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "pl-PL")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(4.25, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(5.75, GridUnitType.Star);
            }
            else if (LoginInfo.LANGID == "ru-RU")
            {
                ContentsRow.ColumnDefinitions[0].Width = new GridLength(3.85, GridUnitType.Star);
                ContentsRow.ColumnDefinitions[1].Width = new GridLength(8, GridUnitType.Pixel);
                ContentsRow.ColumnDefinitions[2].Width = new GridLength(6.15, GridUnitType.Star);
            }
        }

        private void ClearControl()
        {
            Util.gridClear(dgConveyor);
            Util.gridClear(dgConveyorDetail);
            _selectedEquipmentSegmentCode = string.Empty;
            _selectedCstTypeCode = string.Empty;
        }

        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 0;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

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

        private double GetPercentage(int x, int y)
        {
            double rate = 0;
            if (y.Equals(0)) return rate;

            try
            {
                return Math.Round(x.GetDouble() / y.GetDouble() * 100, 1) ;
            }
            catch (Exception)
            {
                return 0;
            }
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




        #endregion


    }
}
