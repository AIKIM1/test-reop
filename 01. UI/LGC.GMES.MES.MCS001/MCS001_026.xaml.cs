/*************************************************************************************
 Created Date : 2019.05.13
      Creator : 신광희 차장
   Decription : 물류 오류 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.13  신광희 차장 : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Data;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Threading;
using System.Windows.Media;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_026.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_026 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSetAutoSelectTime = false;
        private bool _isSearchResult = false;
        #endregion

        public MCS001_026()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            TimerSetting();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCombo();
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STAT_LOGIS_CMD";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                //inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["EQPTID"] = string.IsNullOrEmpty(msbWarehouseSection.SelectedItemsToString) ? msbWarehouseSection.SelectedItemsToString : msbWarehouseSection.SelectedItemsToString + "," + msbAgvSection.SelectedItemsToString;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, null, true);
                });

                
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboAutoSearch?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboAutoSearch.SelectedValue.ToString());
                    }

                    if (second == 0 && _isSetAutoSelectTime)
                    {
                        Util.MessageValidation("SFU6039");
                        return;
                    }

                    if (second == 0)
                    {
                        _isSetAutoSelectTime = true;
                        return;
                    }

                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSetAutoSelectTime)
                    {
                        //물류 오류 모니터링 자동조회  %1초로 변경 되었습니다.
                        if (cboAutoSearch != null)
                            Util.MessageInfo("SFU6036", cboAutoSearch.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _monitorTimer_Tick(object sender, EventArgs e)
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

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("TRBL_1"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRBL_1").GetInt() > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("TRBL_2"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRBL_2").GetInt() > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("TRBL_3"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRBL_3").GetInt() > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("TRBL_4"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRBL_4").GetInt() > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("TRBL_5"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRBL_5").GetInt() > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("TRBL_6"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRBL_6").GetInt() > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (e.Cell.Column.Name.Equals("TRBL_7"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRBL_7").GetInt() > 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        #endregion

        #region Mehod
        private void InitializeCombo()
        {
            SetWarehouseSection(msbWarehouseSection);
            SetAgvSection(msbAgvSection);
        }

        private void TimerSetting()
        {

            CommonCombo combo = new CommonCombo();
            // 자동 조회 시간 Combo
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboAutoSearch, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboAutoSearch != null && cboAutoSearch.Items.Count > 0)
                cboAutoSearch.SelectedIndex = 0;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboAutoSearch?.SelectedValue?.ToString()))
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _monitorTimer.Tick += _monitorTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

        private void SetWarehouseSection(MultiSelectionBox msb)
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_STAT_LOGIS_CMD_CBO";

                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                msb.ItemsSource = DataTableConverter.Convert(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    msb.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAgvSection(MultiSelectionBox msb)
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_STAT_LOGIS_CMD_AGV_CBO";

                DataTable inTable = new DataTable {TableName = "RQSTDT"};
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                msb.ItemsSource = DataTableConverter.Convert(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    msb.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
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
