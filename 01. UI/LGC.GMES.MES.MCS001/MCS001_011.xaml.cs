/*************************************************************************************
 Created Date : 2018.09.11
      Creator : 신광희
   Decription : 점보롤 창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.11  신광희 차장 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;


namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_011.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_011 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        private UcRackStair[][] _rackStairs1;
        private UcRackStair[][] _rackStairs2;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private const int MaxRow = 5;
        private const int MaxCol = 39;

        private bool _isSetAutoSelectTime = false;
        private bool _isFirstLoading = true;
        private DataTable _dtRackInfo;
        private int _maxRow;
        private int _maxCol;
        private string _userId;

        #endregion

        public MCS001_011()
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
            MakeRowDefinition();
            MakeColumnDefinition();
            PrepareRackStair();
            PrepareRackStairLayout();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSelect, btnSearch, btnManualIssue, btnwarehousingHistory };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_isFirstLoading)
            {
                InitializeCombo();
                SelectAllJumboRollMonitoringControl();
                
            }
            else
            {
                ReSetJumboRollControl();
                SelectJumboRollMonitoring();
                SelectJumboRollInventory();
                SelectJumboRollInventoryByProcess();
                SelectJumboRollRank();
                SelectJumboRollPortInfo();
            }
            _isFirstLoading = false;


            // TODO 특정 문화권에 대한 Convert 예시
            //double value = 123.45;
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("uk-UA");
            //double previewValue = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
            //ControlsLibrary.MessageBox.Show("uk-UA >> 123.45 : " + Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture).GetString());

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("ko-KR");
            //ControlsLibrary.MessageBox.Show("ko-KR >> 123,45 : " + Convert.ToDouble(previewValue, System.Globalization.CultureInfo.InvariantCulture).GetString());

            //Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_monitorTimer.Stop();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelect()) return;

            try
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();

                // 1열 영역에서 LOT ID 또는 MODEL 코드로 대상을 조회
                for (int r = 0; r < grdRackstair1.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair1.ColumnDefinitions.Count; c++)
                    {

                        UcRackStair ucRackStair = _rackStairs1[r][c];

                        doubleAnimation.From = ucRackStair.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                        doubleAnimation.AutoReverse = true;

                        if (!string.IsNullOrEmpty(txtLotId.Text) && string.Equals(ucRackStair.LotId.ToUpper(), txtLotId.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            if (!ucRackStair.IsChecked)
                            {
                                UnCheckedAllRackStair();
                                SetScrollToHorizontalOffset(scrollViewer1, c);

                                ucRackStair.IsChecked = true;
                                CheckUcRackStair(ucRackStair);
                                ucRackStair.BeginAnimation(HeightProperty, doubleAnimation);

                                if (CommonVerify.HasDataGridRow(dgInputRank)) Util.DataGridCheckAllUnChecked(dgInputRank);
                            }
                            return;
                        }

                    }
                }

                // 2열 영역에서 LOT ID 또는 MODEL 코드로 대상을 조회
                for (int r = 0; r < grdRackstair2.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair2.ColumnDefinitions.Count; c++)
                    {

                        UcRackStair ucRackStair = _rackStairs2[r][c];

                        doubleAnimation.From = ucRackStair.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        doubleAnimation.AutoReverse = true;

                        if (!string.IsNullOrEmpty(txtLotId.Text) && string.Equals(ucRackStair.LotId.ToUpper(), txtLotId.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            if (!ucRackStair.IsChecked)
                            {
                                UnCheckedAllRackStair();
                                SetScrollToHorizontalOffset(scrollViewer2, c);

                                ucRackStair.IsChecked = true;
                                CheckUcRackStair(ucRackStair);
                                ucRackStair.BeginAnimation(HeightProperty, doubleAnimation);

                                if (CommonVerify.HasDataGridRow(dgInputRank)) Util.DataGridCheckAllUnChecked(dgInputRank);
                            }
                            return;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnwarehousingHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWareHousingHistory()) return;

            object[] parameters = new object[2];
            parameters[0] = string.Empty;
            parameters[1] = string.Empty;
            FrameOperation.OpenMenu("SFU010180120", true, parameters);
        }

        private void dgProcessStock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (dgProcessStock.SelectedItem == null) return;

        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            // 축소
            ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.4, GridUnitType.Star);
            ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.6, GridUnitType.Star);
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            // 확장
            ContentsRow.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            ContentsRow.ColumnDefinitions[2].Width = new GridLength(10.0, GridUnitType.Star);
            RightArea.RowDefinitions[3].Height = new GridLength(30);
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
                        _isSetAutoSelectTime = true;
                    }
                    else
                    {
                        _isSetAutoSelectTime = false;
                    }


                    if (second == 0 && _isSetAutoSelectTime)
                    {
                        Util.MessageValidation("SFU1606");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSetAutoSelectTime)
                    {
                        //점보롤 창고 모니터링 자동조회  %1초로 변경 되었습니다.
                        if (cboAutoSearch != null)
                            Util.MessageInfo("SFU5035", cboAutoSearch.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rankCheck_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            if (cb.IsChecked != null)
            {
                int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;
                for (int i = 0; i < ((DataGridCellPresenter)cb.Parent).DataGrid.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(((DataGridCellPresenter)cb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                }

                DataRowView drv = cb.DataContext as DataRowView;
                if (drv != null)
                {
                    UnCheckedAllRackStair();
                    SelectJumboRollFromRankChecked(drv["RACK_ID"].GetString());
                }
                    
            }
        }

        private void rankCheck_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            if (cb.IsChecked != null)
            {
                DataRow dtRow = (cb.DataContext as DataRowView)?.Row;
                if (dtRow != null)
                {

                    SelectJumboRollFromRankUnChecked(dtRow["RACK_ID"].GetString());
                }
            }
        }

        private void _monitorTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;
            if (CommonVerify.HasDataGridRow(dgRackInfo) || CommonVerify.HasTableRow(_dtRackInfo)) return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1 || string.IsNullOrEmpty(cboStocker.SelectedValue.GetString())) return;

                    ReSetJumboRollControl();
                    SelectAllJumboRollMonitoringControl();
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

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //ShowLoadingIndicator();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelectJumboRollMonitoring()) return;
            
            ReSetJumboRollControl();
            
            SelectJumboRollMonitoring();
            SelectJumboRollInventory();
            SelectJumboRollInventoryByProcess();
            SelectJumboRollRank();
            SelectJumboRollPortInfo();
        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;
            _monitorTimer?.Stop();

            if (string.Equals(DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "WIPHOLD").GetString(),"Y"))
            {
                Util.MessageConfirm("SFU6003", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // 수동출고예약
                        //ManualIssue();
                        ManualIssueAuthConfirm();
                    }
                    else
                    {
                        if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
                    }
                });
            }
            else
            {
                //ManualIssue();
                ManualIssueAuthConfirm();
            }
        }

        private void btnMappingClear_Click(object sender, RoutedEventArgs e)
        {
            _monitorTimer?.Stop();
            CMM_MCS_WAREHOUSE_MAPPING_CLEAR popupMappingClear = new CMM_MCS_WAREHOUSE_MAPPING_CLEAR { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = null;

            C1WindowExtension.SetParameters(popupMappingClear, parameters);

            popupMappingClear.Closed += popupMappingClear_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupMappingClear.ShowModal()));
        }

        private void popupMappingClear_Closed(object sender, EventArgs e)
        {
            CMM_MCS_WAREHOUSE_MAPPING_CLEAR popup = sender as CMM_MCS_WAREHOUSE_MAPPING_CLEAR;
            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectJumboRollMonitoring();
            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
        }

        private void btnPortChange_Click(object sender, RoutedEventArgs e)
        {
            _monitorTimer?.Stop();
            MCS001_011_PORT_SETUP popupPortSetup = new MCS001_011_PORT_SETUP { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = null;

            C1WindowExtension.SetParameters(popupPortSetup, parameters);

            popupPortSetup.Closed += popupPortSetup_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupPortSetup.ShowModal()));

        }

        private void popupPortSetup_Closed(object sender, EventArgs e)
        {
            MCS001_011_PORT_SETUP popup = sender as MCS001_011_PORT_SETUP;
            if (popup != null)
            {

            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
        }

        private void btnFiFoManualIssue_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (!ValidationFiFoManualIssue()) return;
            _monitorTimer?.Stop();
            MCS001_011_FIFO_MANUAL_ISSUE popupFiFoManualIssue = new MCS001_011_FIFO_MANUAL_ISSUE { FrameOperation = FrameOperation };


            DataTable dt = DataTableConverter.Convert(cboStocker.ItemsSource);
            DataRow dr = dt.Rows[cboStocker.SelectedIndex];

            object[] parameters = new object[2];
            parameters[0] = cboProcess.SelectedValue;
            parameters[1] = dr["ELTR_TYPE_CODE"].GetString();

            C1WindowExtension.SetParameters(popupFiFoManualIssue, parameters);

            popupFiFoManualIssue.Closed += popupFiFoManualIssue_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupFiFoManualIssue.ShowModal()));
            */

            if (!ValidationFiFoManualIssue()) return;
            _monitorTimer?.Stop();
            MCS001_011_FIFO_RELEASE popupFiFoManualIssue = new MCS001_011_FIFO_RELEASE { FrameOperation = FrameOperation };


            DataTable dt = DataTableConverter.Convert(cboStocker.ItemsSource);
            DataRow dr = dt.Rows[cboStocker.SelectedIndex];

            object[] parameters = new object[2];
            parameters[0] = cboProcess.SelectedValue;
            parameters[1] = dr["ELTR_TYPE_CODE"].GetString();

            C1WindowExtension.SetParameters(popupFiFoManualIssue, parameters);

            popupFiFoManualIssue.Closed += popupFiFoManualIssue_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupFiFoManualIssue.ShowModal()));

        }

        private void popupFiFoManualIssue_Closed(object sender, EventArgs e)
        {
            /*
            MCS001_011_FIFO_MANUAL_ISSUE popup = sender as MCS001_011_FIFO_MANUAL_ISSUE;
            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectJumboRollMonitoring();
            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
            */

            MCS001_011_FIFO_RELEASE popup = sender as MCS001_011_FIFO_RELEASE;
            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectJumboRollMonitoring();
            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();

        }

        private void dgProcessStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectMaxRankxyz();
        }

        private void chkHoldException_Checked(object sender, RoutedEventArgs e)
        {
            SelectJumboRollInventoryByProcess();
        }

        private void chkHoldException_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectJumboRollInventoryByProcess();
        }

        private void UcRackStair1_DoubleClick(object sender, RoutedEventArgs e)
        {

        }

        private void UcRackStair1_Click(object sender, RoutedEventArgs e)
        {
            //UcRackStair rackStair = sender as UcRackStair;
            //if (rackStair != null)
            //{
            //    ControlsLibrary.MessageBox.Show(rackStair.Name);
            //}
        }

        private void UcRackStair1_Checked(object sender, RoutedEventArgs e)
        {
            UcRackStair rackStair = sender as UcRackStair;
            if (rackStair == null) return;

            if (rackStair.IsChecked)
            {
                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackStair ucRackStair = _rackStairs1[row][col];

                        if (!Equals(rackStair, ucRackStair))
                        {
                            ucRackStair.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackStair ucRackStair = _rackStairs2[row][col];

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
                int maxSeq = 1;

                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackStair.RackId;
                dr["SEQ"] = maxSeq;
                dr["PRJT_NAME"] = rackStair.ProjectName;
                dr["MODLID"] = rackStair.ModelCode;
                dr["PRODID"] = rackStair.ProductCode;
                dr["PRODNAME"] = rackStair.ProductName;
                dr["WH_RCV_DTTM"] = rackStair.WarehouseReceiveDate;
                dr["LOTID"] = rackStair.LotId;
                dr["WIPQTY"] = rackStair.WipQty;
                dr["VLD_DATE"] = rackStair.ValidDate;
                dr["PAST_DAY"] = rackStair.ElapseDay;
                dr["WIPHOLD"] = rackStair.WipHold;
                dr["MCS_CST_ID"] = rackStair.DistributionCarrierId;
                dr["WOID"] = rackStair.WorkOrderId;
                dr["RACK_STAT_CODE"] = rackStair.RackStateCode;
                dr["ELECTRODETYPE"] = rackStair.ProductClassCode;
                _dtRackInfo.Rows.Add(dr);

                

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
            }
            else
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
            }

            if (CommonVerify.HasDataGridRow(dgInputRank))
                Util.DataGridCheckAllUnChecked(dgInputRank);
        }

        private void UcRackStair2_DoubleClick(object sender, RoutedEventArgs e)
        {

        }

        private void UcRackStair2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcRackStair2_Checked(object sender, RoutedEventArgs e)
        {
            UcRackStair rackStair = sender as UcRackStair;
            if (rackStair == null) return;

            if (rackStair.IsChecked)
            {
                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackStair ucRackStair = _rackStairs2[row][col];

                        if (!Equals(rackStair, ucRackStair))
                        {
                            ucRackStair.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackStair ucRackStair = _rackStairs1[row][col];

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
                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackStair.RackId;
                dr["SEQ"] = maxSeq;
                dr["PRJT_NAME"] = rackStair.ProjectName;
                dr["MODLID"] = rackStair.ModelCode;
                dr["PRODID"] = rackStair.ProductCode;
                dr["PRODNAME"] = rackStair.ProductName;
                dr["WH_RCV_DTTM"] = rackStair.WarehouseReceiveDate;
                dr["LOTID"] = rackStair.LotId;
                dr["WIPQTY"] = rackStair.WipQty;
                dr["VLD_DATE"] = rackStair.ValidDate;
                dr["PAST_DAY"] = rackStair.ElapseDay;
                dr["WIPHOLD"] = rackStair.WipHold;
                dr["MCS_CST_ID"] = rackStair.DistributionCarrierId;
                dr["WOID"] = rackStair.WorkOrderId;
                dr["RACK_STAT_CODE"] = rackStair.RackStateCode;
                dr["ELECTRODETYPE"] = rackStair.ProductClassCode;
                _dtRackInfo.Rows.Add(dr);

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
            }
            else
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
            }
            if(CommonVerify.HasDataGridRow(dgInputRank))
                Util.DataGridCheckAllUnChecked(dgInputRank);
        }

        private void dgInputRank_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

        }

        private void btnSaveRackInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveRackInfo()) return;
            _monitorTimer?.Stop();

            RackInfo();
        }
        #endregion

        #region Method

        private void CheckUcRackStair(UcRackStair ucRackStair)
        {

            if (CommonVerify.HasTableRow(_dtRackInfo))
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }
            int maxSeq = 1;
            DataRow dr = _dtRackInfo.NewRow();
            dr["RACK_ID"] = ucRackStair.RackId;
            dr["SEQ"] = maxSeq;
            dr["PRJT_NAME"] = ucRackStair.ProjectName;
            dr["MODLID"] = ucRackStair.ModelCode;
            dr["PRODID"] = ucRackStair.ProductCode;
            dr["PRODNAME"] = ucRackStair.ProductName;
            dr["WH_RCV_DTTM"] = ucRackStair.WarehouseReceiveDate;
            dr["LOTID"] = ucRackStair.LotId;
            dr["WIPQTY"] = ucRackStair.WipQty;
            dr["VLD_DATE"] = ucRackStair.ValidDate;
            dr["PAST_DAY"] = ucRackStair.ElapseDay;
            dr["WIPHOLD"] = ucRackStair.WipHold;
            dr["MCS_CST_ID"] = ucRackStair.DistributionCarrierId;
            dr["WOID"] = ucRackStair.WorkOrderId;
            dr["RACK_STAT_CODE"] = ucRackStair.RackStateCode;
            dr["ELECTRODETYPE"] = ucRackStair.ProductClassCode;
            _dtRackInfo.Rows.Add(dr);

            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }

        private void UnCheckUcRackStair(UcRackStair ucRackStair)
        {
            DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID = '" + ucRackStair.RackId + "'");
            foreach (DataRow row in selectedRow)
            {
                _dtRackInfo.Rows.Remove(row);
            }
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }

        private void SelectAllJumboRollMonitoringControl()
        {
            try
            {
                //ShowLoadingIndicator();

                _monitorTimer.Stop();
                InitializeRackUserControl();
                SelectJumboRollMonitoring();
                SelectJumboRollInventory();
                SelectJumboRollInventoryByProcess();
                SelectJumboRollRank();
                SelectJumboRollPortInfo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                if (_isSetAutoSelectTime)
                {
                    _monitorTimer.Start();
                }

                //HiddenLoadingIndicator();
            }
        }

        /*
        private void SelectJumboRollMonitoring()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_MONITORING";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("Z_PSTN", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["Z_PSTN"] = null; //string.IsNullOrEmpty(cboStair.SelectedValue.GetString()) ? null : cboStair.SelectedValue;
                dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue;
                dr["MODLID"] = null;
                //dr["LOTID"] = string.IsNullOrEmpty(txtLotId.Text.Trim()) ? null : txtLotId.Text.Trim();
                dr["LOTID"] = null;
                dr["PRJT_NAME"] = string.IsNullOrEmpty(txtProjectName.Text.Trim()) ? null : txtProjectName.Text.Trim();
                inDataTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inDataTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    HideAndClearAllRack();
                    Util.gridClear(dgRackInfo);

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        foreach (DataRow item in bizResult.Rows)
                        {
                            int x = 0;
                            switch (item["Z_PSTN"].ToString())
                            {
                                case "1":
                                    x = 4;
                                    break;
                                case "2":
                                    x = 3;
                                    break;
                                case "3":
                                    x = 2;
                                    break;
                                case "4":
                                    x = 1;
                                    break;
                                case "5":
                                    x = 0;
                                    break;
                            }

                            int y = int.Parse(item["Y_PSTN"].ToString()) - 1;

                            UcRackStair ucRackStair = item["X_PSTN"].ToString() == "1" ? _rackStairs1[x][y] : _rackStairs2[x][y];

                            if (ucRackStair == null) continue;

                            ucRackStair.LotId = item["LOTID"].GetString();
                            ucRackStair.RackId = item["RACK_ID"].GetString();
                            ucRackStair.RackStateCode = item["RACK_STAT_CODE"].GetString();
                            ucRackStair.LinqEquipmentSegmentCode = item["LINK_EQSGID"].GetString();
                            ucRackStair.ReceivePriority = Util.NVC_Int(item["RCV_PRIORITY"]);
                            ucRackStair.ZoneId = item["ZONE_ID"].GetString();
                            ucRackStair.DistributionCarrierId = item["MCS_CST_ID"].GetString();
                            ucRackStair.UseFlag = item["USE_FLAG"].GetString();
                            ucRackStair.EquipmentCode = item["EQPTID"].GetString();
                            ucRackStair.ValidDate = item["VLD_DATE"].GetString();
                            ucRackStair.CalculationDate = item["CALDATE"].GetString();
                            ucRackStair.WarehouseReceiveDate = item["WH_RCV_DTTM"].GetString();
                            ucRackStair.WipSeq = Util.NVC_Decimal(item["WIPSEQ"]);
                            ucRackStair.ProductCode = item["PRODID"].GetString();
                            ucRackStair.ProcessSegmentCode = item["PCSGID"].GetString();
                            ucRackStair.ProcessCode = item["PROCID"].GetString();
                            ucRackStair.WipState = item["WIPSTAT"].GetString();
                            ucRackStair.WipStartDateTime = item["WIPSDTTM"].GetString();
                            ucRackStair.WipQty = Util.NVC_Decimal(item["WIPQTY"]);
                            ucRackStair.WipQty2 = Util.NVC_Decimal(item["WIPQTY2"]);
                            ucRackStair.WorkOrderId = item["WOID"].GetString();
                            ucRackStair.EquipmentSegmentCode = item["EQSGID"].GetString();
                            ucRackStair.WipHold = item["WIPHOLD"].GetString();
                            ucRackStair.ProductName = item["PRODNAME"].GetString();
                            ucRackStair.ModelCode = item["MODLID"].GetString();
                            ucRackStair.UnitCode = item["UNIT_CODE"].GetString();
                            ucRackStair.ProjectName = item["PRJT_NAME"].GetString();
                            ucRackStair.ProcessName = item["PROCNAME"].GetString();
                            ucRackStair.EquipmentSegmentName = item["EQSGNAME"].GetString();
                            ucRackStair.ElapseDay = Util.NVC_Int(item["PAST_DAY"]);
                            ucRackStair.Row = int.Parse(item["Z_PSTN"].GetString());
                            ucRackStair.Col = int.Parse(item["Y_PSTN"].GetString());
                            ucRackStair.Stair = int.Parse(item["X_PSTN"].GetString());
                            
                            ucRackStair.Visibility = Visibility.Visible;

                            if (cboProcess.SelectedValue != null || !string.IsNullOrEmpty(txtProjectName.Text.Trim()))
                            {
                                if (string.Equals(item["RACK_STAT_CODE"].GetString(), "USING"))
                                {
                                    if (cboProcess.SelectedValue != null && cboProcess.SelectedValue?.GetString() == item["PROCID"].GetString()
                                        || item["PRJT_NAME"] != null && item["PRJT_NAME"].GetString().Contains(txtProjectName.Text))
                                    {
                                        ucRackStair.RootLayout.Background = new SolidColorBrush(Colors.White);
                                    }
                                    else
                                    {
                                        ucRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                                    }
                                }
                            }
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
        */


        private void SelectJumboRollMonitoring()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_MONITORING";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("Z_PSTN", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("HOLDYN", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["Z_PSTN"] = null;
                dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue;
                dr["MODLID"] = null;
                dr["LOTID"] = null;
                dr["PRJT_NAME"] = string.IsNullOrEmpty(txtProjectName.Text.Trim()) ? null : txtProjectName.Text.Trim();
                dr["PROD_VER_CODE"] = string.IsNullOrEmpty(txtProductVersion.Text.Trim()) ? null : txtProductVersion.Text.Trim();
                dr["HOLDYN"] = cboHold.SelectedValue;
                inDataTable.Rows.Add(dr);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(inDataTable);
                //string xml = ds.GetXml();

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                HideAndClearAllRack();
                Util.gridClear(dgRackInfo);

                if (CommonVerify.HasTableRow(bizResult))
                {
                    foreach (DataRow item in bizResult.Rows)
                    {
                        int x = 0;
                        switch (item["Z_PSTN"].ToString())
                        {
                            case "1":
                                x = 4;
                                break;
                            case "2":
                                x = 3;
                                break;
                            case "3":
                                x = 2;
                                break;
                            case "4":
                                x = 1;
                                break;
                            case "5":
                                x = 0;
                                break;
                        }

                        int y = int.Parse(item["Y_PSTN"].ToString()) - 1;

                        UcRackStair ucRackStair = item["X_PSTN"].ToString() == "1" ? _rackStairs1[x][y] : _rackStairs2[x][y];

                        if (ucRackStair == null) continue;

                        // HOLD 검색조건에 따른 데이터 바인딩 처리 수정

                        ucRackStair.LotId = item["LOTID"].GetString();
                        ucRackStair.RackId = item["RACK_ID"].GetString();
                        ucRackStair.RackStateCode = item["RACK_STAT_CODE"].GetString();
                        ucRackStair.LinqEquipmentSegmentCode = item["LINK_EQSGID"].GetString();
                        ucRackStair.ReceivePriority = Util.NVC_Int(item["RCV_PRIORITY"]);
                        ucRackStair.ZoneId = item["ZONE_ID"].GetString();
                        ucRackStair.DistributionCarrierId = item["MCS_CST_ID"].GetString();
                        ucRackStair.UseFlag = item["USE_FLAG"].GetString();
                        ucRackStair.EquipmentCode = item["EQPTID"].GetString();
                        ucRackStair.ValidDate = item["VLD_DATE"].GetString();
                        ucRackStair.CalculationDate = item["CALDATE"].GetString();
                        ucRackStair.WarehouseReceiveDate = item["WH_RCV_DTTM"].GetString();
                        ucRackStair.WipSeq = Util.NVC_Decimal(item["WIPSEQ"]);
                        ucRackStair.ProductCode = item["PRODID"].GetString();
                        ucRackStair.ProcessSegmentCode = item["PCSGID"].GetString();
                        ucRackStair.ProcessCode = item["PROCID"].GetString();
                        ucRackStair.WipState = item["WIPSTAT"].GetString();
                        ucRackStair.WipStartDateTime = item["WIPSDTTM"].GetString();
                        ucRackStair.WipQty = Util.NVC_Decimal(item["WIPQTY"]);
                        ucRackStair.WipQty2 = Util.NVC_Decimal(item["WIPQTY2"]);
                        ucRackStair.WorkOrderId = item["WOID"].GetString();
                        ucRackStair.EquipmentSegmentCode = item["EQSGID"].GetString();
                        ucRackStair.WipHold = item["WIPHOLD"].GetString();
                        ucRackStair.ProductName = item["PRODNAME"].GetString();
                        ucRackStair.ModelCode = item["MODLID"].GetString();
                        ucRackStair.UnitCode = item["UNIT_CODE"].GetString();
                        ucRackStair.ProjectName = item["PRJT_NAME"].GetString();
                        ucRackStair.ProcessName = item["PROCNAME"].GetString();
                        ucRackStair.EquipmentSegmentName = item["EQSGNAME"].GetString();
                        ucRackStair.ProductVersionCode = item["PROD_VER_CODE"].GetString();
                        ucRackStair.ElapseDay = Util.NVC_Int(item["PAST_DAY"]);
                        ucRackStair.Row = int.Parse(item["Z_PSTN"].GetString());
                        ucRackStair.Col = int.Parse(item["Y_PSTN"].GetString());
                        ucRackStair.Stair = int.Parse(item["X_PSTN"].GetString());

                        ucRackStair.ProductClassCode = item["PRDT_CLSS_CODE"].GetString();
                        ucRackStair.LegendColor = item["BG_COLOR"].GetString();

                        ucRackStair.Visibility = Visibility.Visible;

                        if (string.Equals(item["RACK_STAT_CODE"].GetString(), "USING"))
                        {
                            if (cboProcess.SelectedValue != null)
                            {
                                if (cboProcess.SelectedValue.GetString() != item["PROCID"].GetString())
                                    ucRackStair.RootLayout.Background =new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if (!string.IsNullOrEmpty(txtProjectName.Text.Trim()))
                            {
                                if(!item["PRJT_NAME"].GetString().Contains(txtProjectName.Text))
                                    ucRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if (!string.IsNullOrEmpty(txtProductVersion.Text.Trim()))
                            {
                                if(!item["PROD_VER_CODE"].GetString().Contains(txtProductVersion.Text))
                                    ucRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if (cboHold.SelectedValue != null)
                            {
                                if (cboHold.SelectedValue.GetString() != item["WIPHOLD"].GetString())
                                    ucRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }
                        }

                        /*
                        if (string.Equals(item["RACK_STAT_CODE"].GetString(), "USING"))
                        {
                            if (cboProcess.SelectedValue != null)
                            {
                                ucRackStair.RootLayout.Background = cboProcess.SelectedValue.GetString() == item["PROCID"].GetString() ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if(!string.IsNullOrEmpty(txtProjectName.Text.Trim()))
                            {
                                ucRackStair.RootLayout.Background = item["PRJT_NAME"].GetString().Contains(txtProjectName.Text) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if (!string.IsNullOrEmpty(txtProductVersion.Text.Trim()))
                            {
                                ucRackStair.RootLayout.Background = item["PROD_VER_CODE"].GetString().Contains(txtProductVersion.Text) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if (cboHold.SelectedValue != null)
                            {
                                if (cboHold.SelectedValue.GetString() == "Y")
                                {
                                    ucRackStair.RootLayout.Background = item["WIPHOLD"].GetString() == "Y" ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                                }
                                else if (cboHold.SelectedValue.GetString() == "N")
                                {
                                    ucRackStair.RootLayout.Background = item["WIPHOLD"].GetString() == "N" ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                                }
                            }
                        }
                        */
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

        private void SelectJumboRollInventory()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_INVENTORY";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //dgStore.ItemsSource = DataTableConverter.Convert(bizResult);
                    Util.GridSetData(dgStore, bizResult, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectJumboRollInventoryByProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_INVENTORY_BY_PROCESS";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("WIPHOLD", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["WIPHOLD"] = chkHoldException.IsChecked != null && (bool)chkHoldException.IsChecked ? "N" : null;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgProcessStock, bizResult, null);
                    //dgProcessStock.ItemsSource = DataTableConverter.Convert(bizResult);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectJumboRollRank()
        {
            try
            {
                ShowLoadingIndicator();

                //const string bizRuleName = "DA_MCS_SEL_LOGIS_CMD_CREATED_LIST_BY_EQPTID";
                const string bizRuleName = "DA_MCS_SEL_RACK_ISSUE_LIST_BY_EQPTID";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = cboStocker.SelectedValue;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //Util.GridSetData(dgInputRank, bizResult, null, true);
                    Util.GridSetData(dgInputRank, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectJumboRollPortInfo()
        {
            try
            {
                if (cboStocker?.SelectedValue == null) return;

                ShowLoadingIndicator();

                txtRollPressIn.Text = string.Empty;
                txtRollPressOut.Text = string.Empty;
                txtCoaterOut.Text = string.Empty;
                txtCoaterIn.Text = string.Empty;

                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB");
                if (convertFromString != null)
                {
                    txtRollPressIn.Background = new SolidColorBrush((Color)convertFromString);
                    txtRollPressOut.Background = new SolidColorBrush((Color)convertFromString);
                    txtCoaterOut.Background = new SolidColorBrush((Color)convertFromString);
                    txtCoaterIn.Background = new SolidColorBrush((Color)convertFromString);
                }

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_PORT_INFO";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    // : 점보롤 창고 현재 포트 정보 Display
                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        var queryRollPressInType = (from t in bizResult.AsEnumerable()
                                                    where t.Field<string>("PORT_TYPE_CODE") == "STK_JR_ROL_IN"
                                                    select new { RollPressInType = t.Field<string>("PORT_STAT_CODE"), RollPressCurrInoutType = t.Field<string>("CURR_INOUT_TYPE_CODE"), MaterialExistFlag = t.Field<string>("MTRL_EXIST_FLAG") }).FirstOrDefault();

                        if (queryRollPressInType != null)
                        {
                            //txtRollPressIn.Text = queryRollPressInType.RollPressInType + " : " + queryRollPressInType.RollPressCurrInoutType;
                            txtRollPressIn.Text = queryRollPressInType.RollPressCurrInoutType + " : " + queryRollPressInType.RollPressInType;
                            txtRollPressIn.FontWeight = FontWeights.Bold;
                            if(queryRollPressInType.MaterialExistFlag == "Y") txtRollPressIn.Background = new SolidColorBrush(Colors.LightPink);

                        }

                        var queryRollPressOutType = (from t in bizResult.AsEnumerable()
                                                     where t.Field<string>("PORT_TYPE_CODE") == "STK_JR_ROL_OUT"
                                                     select new { RollPressOutType = t.Field<string>("PORT_STAT_CODE"), RollPressCurrInoutType = t.Field<string>("CURR_INOUT_TYPE_CODE"), MaterialExistFlag = t.Field<string>("MTRL_EXIST_FLAG") }).FirstOrDefault();

                        if (queryRollPressOutType != null)
                        {
                            //txtRollPressOut.Text = queryRollPressOutType.RollPressOutType + " : " + queryRollPressOutType.RollPressCurrInoutType;
                            txtRollPressOut.Text = queryRollPressOutType.RollPressCurrInoutType + " : " + queryRollPressOutType.RollPressOutType;
                            txtRollPressOut.FontWeight = FontWeights.Bold;
                            if (queryRollPressOutType.MaterialExistFlag == "Y") txtRollPressOut.Background = new SolidColorBrush(Colors.LightPink);
                        }

                        var queryCoaterOutType = (from t in bizResult.AsEnumerable()
                                                  where t.Field<string>("PORT_TYPE_CODE") == "STK_JR_COT_OUT"
                                                  select new { CoaterOutType = t.Field<string>("PORT_STAT_CODE"), CoaterCurrInoutType = t.Field<string>("CURR_INOUT_TYPE_CODE"), MaterialExistFlag = t.Field<string>("MTRL_EXIST_FLAG") }).FirstOrDefault();

                        if (queryCoaterOutType != null)
                        {
                            //txtCoaterOut.Text = queryCoaterOutType.CoaterOutType + " : " + queryCoaterOutType.CoaterCurrInoutType;
                            txtCoaterOut.Text = queryCoaterOutType.CoaterCurrInoutType + " : " + queryCoaterOutType.CoaterOutType;
                            txtCoaterOut.FontWeight = FontWeights.Bold;
                            if (queryCoaterOutType.MaterialExistFlag == "Y") txtCoaterOut.Background = new SolidColorBrush(Colors.LightPink);
                        }

                        var queryCoaterInType = (from t in bizResult.AsEnumerable()
                                                 where t.Field<string>("PORT_TYPE_CODE") == "STK_JR_COT_IN"
                                                 select new { CoaterInType = t.Field<string>("PORT_STAT_CODE"), CoaterCurrInoutType = t.Field<string>("CURR_INOUT_TYPE_CODE"), MaterialExistFlag = t.Field<string>("MTRL_EXIST_FLAG") }).FirstOrDefault();

                        if (queryCoaterInType != null)
                        {
                            //txtCoaterIn.Text = queryCoaterInType.CoaterInType + " : " + queryCoaterInType.CoaterCurrInoutType;
                            txtCoaterIn.Text = queryCoaterInType.CoaterCurrInoutType + " : " + queryCoaterInType.CoaterInType;
                            txtCoaterIn.FontWeight = FontWeights.Bold;
                            if (queryCoaterInType.MaterialExistFlag == "Y") txtCoaterIn.Background = new SolidColorBrush(Colors.LightPink);
                        }

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UnCheckedAllRackStair()
        {
            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackStair ucRackStair = _rackStairs1[rowIndex][colIndex];

                    if (ucRackStair.IsChecked)
                    {
                        ucRackStair.IsChecked = false;
                        UnCheckUcRackStair(ucRackStair);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackStair ucRackStair = _rackStairs2[rowIndex][colIndex];

                    if (ucRackStair.IsChecked)
                    {
                        ucRackStair.IsChecked = false;
                        UnCheckUcRackStair(ucRackStair);
                    }
                }
            }
        }

        private void SelectJumboRollFromRankChecked(string rackId)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();

            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackStair ucRackStair = _rackStairs1[rowIndex][colIndex];

                    doubleAnimation.From = ucRackStair.ActualHeight;
                    doubleAnimation.To = 0;
                    doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucRackStair.RackId)) continue;

                    if (string.Equals(ucRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer1, colIndex);
                            ucRackStair.IsChecked = true;
                            CheckUcRackStair(ucRackStair);

                            ucRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackStair ucRackStair = _rackStairs2[rowIndex][colIndex];

                    doubleAnimation.From = ucRackStair.ActualHeight;
                    doubleAnimation.To = 0;
                    doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucRackStair.RackId)) continue;

                    if (string.Equals(ucRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer2, colIndex);
                            ucRackStair.IsChecked = true;
                            CheckUcRackStair(ucRackStair);

                            ucRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }
            
        }

        private void SelectJumboRollFromRankUnChecked(string rackId)
        {
            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackStair ucRackStair = _rackStairs1[rowIndex][colIndex];

                    if (string.Equals(ucRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (ucRackStair.IsChecked)
                        {
                            ucRackStair.IsChecked = false;
                            UnCheckUcRackStair(ucRackStair);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackStair ucRackStair = _rackStairs2[rowIndex][colIndex];
                    if (string.Equals(ucRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (ucRackStair.IsChecked)
                        {
                            ucRackStair.IsChecked = false;
                            UnCheckUcRackStair(ucRackStair);
                        }
                        return;
                    }
                }
            }
        }

        private void SelectMaxRankxyz()
        {
            try
            {
                if (cboStocker?.SelectedValue == null) return;

                const string bizRuleName = "DA_MCS_SEL_RACK_MAX_XYZ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = cboStocker.SelectedValue;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    if (string.IsNullOrEmpty(searchResult.Rows[0][2].GetString()) || string.IsNullOrEmpty(searchResult.Rows[0][1].GetString()))
                    {
                        _maxRow = 0;
                        _maxCol = 0;
                        return;
                    }

                    _maxRow = Convert.ToInt32(searchResult.Rows[0][2].GetString());
                    _maxCol = Convert.ToInt32(searchResult.Rows[0][1].GetString());

                    //cboStair.ItemsSource = null;

                    //DataTable dtStair = new DataTable();
                    //dtStair.Columns.Add("CBO_CODE", typeof(string));
                    //dtStair.Columns.Add("CBO_NAME", typeof(string));

                    //for (int i = 1; i <= _maxRow; i++)
                    //{
                    //    DataRow newRow = dtStair.NewRow();
                    //    newRow["CBO_CODE"] = i.GetString();
                    //    newRow["CBO_NAME"] = i.GetString();
                    //    dtStair.Rows.Add(newRow);
                    //}

                    //DataRow dataRow = dtStair.NewRow();
                    //dataRow["CBO_CODE"] = string.Empty;
                    //dataRow["CBO_NAME"] = "-ALL-";
                    //dtStair.Rows.InsertAt(dataRow, 0);

                    //cboStair.ItemsSource = dtStair.Copy().AsDataView();
                    //cboStair.SelectedIndex = 0;
                }
                else
                {
                    _maxRow = 0;
                    _maxCol = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void ManualIssueAuthConfirm()
        {
            CMM_COM_AUTH_CONFIRM popupAuthConfirm = new CMM_COM_AUTH_CONFIRM { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = "LOGIS_MANA";
            C1WindowExtension.SetParameters(popupAuthConfirm, parameters);

            popupAuthConfirm.Closed += popupAuthConfirm_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupAuthConfirm.ShowModal()));
        }

        private void popupAuthConfirm_Closed(object sender, EventArgs e)
        {
            CMM_COM_AUTH_CONFIRM popup = sender as CMM_COM_AUTH_CONFIRM;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _userId = popup.UserID;
                ManualIssue();
            }
        }


        private void ManualIssue()
        {
            MCS001_011_MANUAL_ISSUE popupManualIssue = new MCS001_011_MANUAL_ISSUE { FrameOperation = FrameOperation };
            object[] parameters = new object[3];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = ((DataView)dgRackInfo.ItemsSource).Table.Rows[0];
            parameters[2] = _userId;
            C1WindowExtension.SetParameters(popupManualIssue, parameters);

            popupManualIssue.Closed += popupManualIssue_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupManualIssue.ShowModal()));

        }

        private void popupManualIssue_Closed(object sender, EventArgs e)
        {
            MCS001_011_MANUAL_ISSUE popup = sender as MCS001_011_MANUAL_ISSUE;
            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectJumboRollMonitoring();
            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
        }

        private void RackInfo()
        {
            MCS001_011_RACK_INFO popupRackInfo = new MCS001_011_RACK_INFO { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = ((DataView)dgRackInfo.ItemsSource).Table.Rows[0];
            C1WindowExtension.SetParameters(popupRackInfo, parameters);

            popupRackInfo.Closed += popupRackInfo_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupRackInfo.ShowModal()));
        }

        private void popupRackInfo_Closed(object sender, EventArgs e)
        {
            MCS001_011_RACK_INFO popup = sender as MCS001_011_RACK_INFO;
            if (popup != null && popup.IsUpdated)
            {
                SelectJumboRollMonitoring();
            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
        }

        private void InitializeRackInfo()
        {
            _dtRackInfo = new DataTable();
            _dtRackInfo.Columns.Add("RACK_ID", typeof(string));
            _dtRackInfo.Columns.Add("PRJT_NAME", typeof(string));
            _dtRackInfo.Columns.Add("MODLID", typeof(string));
            _dtRackInfo.Columns.Add("PRODID", typeof(string));
            _dtRackInfo.Columns.Add("PRODNAME", typeof(string));
            _dtRackInfo.Columns.Add("WH_RCV_DTTM", typeof(string));
            _dtRackInfo.Columns.Add("LOTID", typeof(string));
            _dtRackInfo.Columns.Add("WIPQTY", typeof(decimal));
            _dtRackInfo.Columns.Add("VLD_DATE", typeof(string));
            _dtRackInfo.Columns.Add("PAST_DAY", typeof(int));
            _dtRackInfo.Columns.Add("WIPHOLD", typeof(string));
            _dtRackInfo.Columns.Add("MCS_CST_ID", typeof(string));
            _dtRackInfo.Columns.Add("RACK_STAT_CODE", typeof(string));
            _dtRackInfo.Columns.Add("WOID", typeof(string));
            _dtRackInfo.Columns.Add("SEQ", typeof(int));
            _dtRackInfo.Columns.Add("ELECTRODETYPE", typeof(string));
        }

        private void InitializeCombo()
        {
            InitializeRackInfo();

            // 자동조회 콤보박스
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0)
                _monitorTimer.Start();

            // 범례 Color콤보박스
            SetLegendColorCombo();

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // 단 콤보박스
            SelectMaxRankxyz();

            //공정 콤보박스
            SetProcessCombo(cboProcess);

            //WIP HOLD
            SetHoldCombo(cboHold);


        }

        private void InitializeRackUserControl()
        {
            for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                {
                    UcRackStair ucRackStair = _rackStairs1[row][col];
                    ucRackStair.IsEnabled = true;
                    ucRackStair.SetRackBackcolor();
                }
            }

            for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                {
                    UcRackStair ucRackStair = _rackStairs2[row][col];
                    ucRackStair.IsEnabled = true;
                    ucRackStair.SetRackBackcolor();
                }
            }
        }

        private void ReSetJumboRollControl()
        {
            _dtRackInfo.Clear();
            SetColumnDefinition(_maxCol);
            ReSetRackStairLayout(_maxRow, _maxCol);
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

        private void HideAndClearAllRack()
        {
            for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                {
                    _rackStairs1[row][col].Visibility = Visibility.Hidden;
                    _rackStairs1[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                {
                    _rackStairs2[row][col].Visibility = Visibility.Hidden;
                    _rackStairs2[row][col].Clear();
                }
            }
        }

        private void SetColumnDefinition(int maxcolIndex)
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            if (grdStair1.ColumnDefinitions.Count > 0) grdStair1.ColumnDefinitions.Clear();
            if (grdStair1.RowDefinitions.Count > 0) grdStair1.RowDefinitions.Clear();

            if (grdStair2.ColumnDefinitions.Count > 0) grdStair2.ColumnDefinitions.Clear();
            if (grdStair2.RowDefinitions.Count > 0) grdStair2.RowDefinitions.Clear();

            int colIndex = 0;
            for (int i = maxcolIndex - 1; i >= 0; i--)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() {Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
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

        private void MakeColumnDefinition()
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            int colIndex = 0;

            for (int i = MaxCol - 1; i >= 0; i--)
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

        private void MakeRowDefinition()
        {
            // 단 Row 생성
            ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength() };
            ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength() };

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);
            
            for (int i = 0; i < MaxRow; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(45) };
                grdColumn1.RowDefinitions.Add(rowDefinition1);
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(45) };
                grdColumn2.RowDefinitions.Add(rowDefinition2);
            }

            for (int i = 0; i < MaxRow; i++)
            {
                TextBlock textBlock1 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.SetValue(Grid.RowProperty, i);
                textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock1.Text = MaxRow - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn1.Children.Add(textBlock1);

                TextBlock textBlock2 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter() });
                textBlock2.SetValue(Grid.RowProperty, i);
                textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock2.Text = MaxRow - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn2.Children.Add(textBlock2);
            }
        }

        private void ReSetRackStairLayout(int maxRowIndex, int maxColumnIndex)
        {
            // 기 생성된 UserControl(UcRackStair) 을 삭제 처리 함. 연 이 Stocker에 의해 달라지며 시작점이 다르므로 기존생성한 UserControl 을 삭제 처리 함.
            //foreach (UcRackStair userControl in Util.FindVisualChildren<UcRackStair>(grdRackstair1))
            //{
            //    grdRackstair1.Children.Remove(userControl);
            //}
            //foreach (UcRackStair userControl in Util.FindVisualChildren<UcRackStair>(grdRackstair2))
            //{
            //    grdRackstair2.Children.Remove(userControl);
            //}

            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();

            /* 기 생성된 UserControl(UcRackStair) 을 삭제 후 다시 UserControl 생성 함.
            // UserControl(UcRackStair) 생성
            _rackStairs1 = new UcRackStair[maxRowIndex][];
            _rackStairs2 = new UcRackStair[maxRowIndex][];

            for (int r = 0; r < _rackStairs1.Length; r++)
            {
                _rackStairs1[r] = new UcRackStair[maxColumnIndex];
                _rackStairs2[r] = new UcRackStair[maxColumnIndex];

                for (int c = 0; c < _rackStairs1[r].Length; c++)
                {
                    UcRackStair ucRackStair1 = new UcRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackStair1.Checked += UcRackStair1_Checked;
                    ucRackStair1.Click += UcRackStair1_Click;
                    ucRackStair1.DoubleClick += UcRackStair1_DoubleClick;
                    _rackStairs1[r][c] = ucRackStair1;
                }

                for (int c = 0; c < _rackStairs2[r].Length; c++)
                {
                    UcRackStair ucRackStair2 = new UcRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackStair2.Checked += UcRackStair2_Checked;
                    ucRackStair2.Click += UcRackStair2_Click;
                    ucRackStair2.DoubleClick += UcRackStair2_DoubleClick;
                    _rackStairs2[r][c] = ucRackStair2;
                }
            }
            */

            for (int i = 0; i < maxColumnIndex; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
            }

            for (int i = 0; i < maxRowIndex; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(45) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(45) };
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
            }

            for (int row = 0; row < maxRowIndex; row++)
            {
                int colIndex = 0;
                for (int col = maxColumnIndex - 1; col >= 0; col--)
                {
                    Grid.SetRow(_rackStairs1[row][col], row);
                    Grid.SetColumn(_rackStairs1[row][col], colIndex);
                    grdRackstair1.Children.Add(_rackStairs1[row][col]);

                    Grid.SetRow(_rackStairs2[row][col], row);
                    Grid.SetColumn(_rackStairs2[row][col], colIndex);
                    grdRackstair2.Children.Add(_rackStairs2[row][col]);

                    colIndex++;
                }
            }
        }

        private void PrepareRackStair()
        {
            _rackStairs1 = new UcRackStair[MaxRow][];
            _rackStairs2 = new UcRackStair[MaxRow][];

            for (int r = 0; r < _rackStairs1.Length; r++)
            {
                _rackStairs1[r] = new UcRackStair[MaxCol];
                _rackStairs2[r] = new UcRackStair[MaxCol];

                for (int c = 0; c < _rackStairs1[r].Length; c++)
                {
                    UcRackStair ucRackStair1 = new UcRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackStair1.Checked += UcRackStair1_Checked;
                    ucRackStair1.Click += UcRackStair1_Click;
                    ucRackStair1.DoubleClick += UcRackStair1_DoubleClick;
                    _rackStairs1[r][c] = ucRackStair1;
                }

                for (int c = 0; c < _rackStairs2[r].Length; c++)
                {
                    UcRackStair ucRackStair2 = new UcRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackStair2.Checked += UcRackStair2_Checked;
                    ucRackStair2.Click += UcRackStair2_Click;
                    ucRackStair2.DoubleClick += UcRackStair2_DoubleClick;
                    _rackStairs2[r][c] = ucRackStair2;
                }
            }

        }

        private void PrepareRackStairLayout()
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();


            for (int i = 0; i < MaxCol; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60)};
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60)};
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
            }

            for (int i = 0; i < MaxRow; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(45)};
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(45)};
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
            }

            for (int row = 0; row < MaxRow; row++)
            {
                int colIndex = 0;
                for(int col = MaxCol-1; col >= 0; col--)
                {
                    Grid.SetRow(_rackStairs1[row][col], row);
                    Grid.SetColumn(_rackStairs1[row][col], colIndex);
                    grdRackstair1.Children.Add(_rackStairs1[row][col]);

                    Grid.SetRow(_rackStairs2[row][col], row);
                    Grid.SetColumn(_rackStairs2[row][col], colIndex);
                    grdRackstair2.Children.Add(_rackStairs2[row][col]);

                    colIndex++;
                }
            }
        }

        private bool ValidationSelect()
        {
            if (string.IsNullOrEmpty(txtLotId.Text))
            {
                Util.MessageValidation("SFU1366");
                return false;
            }


            return true;
        }

        private bool ValidationSelectJumboRollMonitoring()
        {
            if (string.IsNullOrEmpty(cboStocker?.SelectedValue?.GetString()))
            {
                HiddenLoadingIndicator();
                return false;
            }
            return true;
        }

        private bool ValidationManualIssue()
        {
            if (dgRackInfo.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            /*
            if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "LOTID").GetString()))
            {
                Util.MessageValidation("SFU1643");
                return false;
            }

            if (DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_STAT_CODE").GetString().Equals("USING") || DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_STAT_CODE").GetString().Equals("NOREAD"))
            {
                return true;
            }
            */

            if (DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_STAT_CODE").GetString().Equals("USING") || DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_STAT_CODE").GetString().Equals("NOREAD"))
            {
                if (DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "RACK_STAT_CODE").GetString().Equals("USING"))
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgRackInfo.Rows[0].DataItem, "LOTID").GetString()))
                    {
                        Util.MessageValidation("SFU1643");
                        return false;
                    }
                }
            }
            else
            {
                Util.MessageValidation("SFU1643");
                return false;
            }

            //Util.MessageValidation("SFU1643");
            return true;
        }

        private bool ValidationFiFoManualIssue()
        {
            if (cboStocker?.SelectedValue == null)
            {
                Util.MessageValidation("SFU2961");
                return false;
            }

            DataTable dt = DataTableConverter.Convert(cboStocker.ItemsSource);
            DataRow dr = dt.Rows[cboStocker.SelectedIndex];

            if (string.IsNullOrEmpty(dr["ELTR_TYPE_CODE"].GetString()))
            {
                Util.MessageValidation("SFU2998");
                return false;
            }

            return true;
        }

        private bool ValidationSaveRackInfo()
        {
            if (dgRackInfo.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private bool ValidationWareHousingHistory()
        {

            return true;
        }

        private void SetLegendColorCombo()
        {
            cboColor.Items.Clear();
            C1ComboBoxItem cbItemTitle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("범례") };
            cboColor.Items.Add(cbItemTitle);

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("KEYGROUPID", typeof(string));
            DataRow drColor = inTable.NewRow();
            drColor["LANGID"] = LoginInfo.LANGID;
            drColor["KEYGROUPID"] = "JRW_MNT_LEGEND";

            inTable.Rows.Add(drColor);
            DataTable dtColorResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_COLOR_LEGEND_CBO", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtColorResult.Rows)
            {
                C1ComboBoxItem cbItem1 = new C1ComboBoxItem
                {
                    Content = row["KEYNAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["KEYVALUE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem1);
            }
            cboColor.SelectedIndex = 0;
        }

        private static void SetStockerCombo(C1ComboBox cbo)
        {

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("EQGRID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["EQGRID"] = "JRW";
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_WAREHOUSE_CBO", "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;

        }

    private static void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetHoldCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "WIPHOLD" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, "N");
        }

        private void SetScrollToHorizontalOffset(ScrollViewer scrollViewer, int colIndex)
        {
            double averageScrollWidth = scrollViewer.ScrollableWidth / _maxCol;
            scrollViewer.ScrollToHorizontalOffset( Math.Abs(colIndex - _maxCol) * averageScrollWidth);            
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
