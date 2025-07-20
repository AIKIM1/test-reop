/*************************************************************************************
 Created Date : 2022.11.24
      Creator : 신광희
   Decription : LNS-PKG 순환 CNV 모니터링 (고공 컨베이어 모니터링 참조)
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.24  신광희 차장 : Initial Created..    
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
    /// MCS001_087.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_087 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        private string _selectedPortCode;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public MCS001_087()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
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
                    //if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSG_NAME")), ObjectDic.Instance.GetObjectName("합계")))
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
                _selectedPortCode = null;

                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);
                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;


                _selectedPortCode = DataTableConverter.GetValue(drv, "PORT_ID").GetString();

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
            const string bizRuleName = "DA_MHS_SEL_LNS_PKG_CNV_TRAY_SUM";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgConveyor, bizResult, null, true);

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

            const string bizRuleName = "DA_MHS_SEL_LNS_PKG_CNV_TRAY_LIST";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PORT_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PORT_ID"] = _selectedPortCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
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
            _selectedPortCode = string.Empty;
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
