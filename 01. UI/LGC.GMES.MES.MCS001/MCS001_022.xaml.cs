/*************************************************************************************
 Created Date : 2019.01.21
      Creator : 오화백 과장
   Decription : 노칭대기창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.21  오화백과장 : Initial Created.  
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_022.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_022 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        private UcPancakeRackStair[][] _rackStairs1;
        private UcPancakeRackStair[][] _rackStairs2;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private readonly Util _util = new Util();

        private bool _isSetAutoSelectTime = false;
        private bool _isFirstLoading = true;
        private DataTable _dtRackInfo;
        private int _maxRowCount;
        private int _maxColumnCount;
        private string _selectedRackId;


        #endregion

        public MCS001_022()
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
            List<Button> listAuth = new List<Button> { btnSelect, btnSearch, btnManualIssue, btnPancakeWarehousingHistory };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_isFirstLoading)
            {
                InitializeCombo();

                if (grdRackstair1.Children.Count < 1 && grdRackstair2.Children.Count < 1)
                {
                    SelectMaxRankxyz();
                    MakeRowDefinition();
                    MakeColumnDefinition();
                    PrepareRackStair();
                    PrepareRackStairLayout();
                }

                SelectAllPancakeMonitoringControl();
            }
            else
            {
                SelectPancakeMonitoring();
                SelectPancakeInventory();
                SelectPancakeInventoryByProject();
                SelectPancakeRank();
                SelectInoutReservationInfo();
            }

            _isFirstLoading = false;
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_monitorTimer.Stop();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelect()) return;

            _selectedRackId = GetRackIdByLotId();

            if (string.IsNullOrEmpty(_selectedRackId)) return;

            try
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();

                // 1열 영역에서 LOT ID 또는 MODEL 코드로 대상을 조회
                for (int r = 0; r < grdRackstair1.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair1.ColumnDefinitions.Count; c++)
                    {

                        UcPancakeRackStair ucPancakeRackStair = _rackStairs1[r][c];

                        doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                        doubleAnimation.AutoReverse = true;

                        if (!string.IsNullOrEmpty(_selectedRackId) && string.Equals(ucPancakeRackStair.RackId.ToUpper(), _selectedRackId.ToUpper(), StringComparison.Ordinal))
                        {
                            if (!ucPancakeRackStair.IsChecked)
                            {
                                //UnCheckedAllPancakeRackStair();
                                SetScrollToHorizontalOffset(scrollViewer1, c);

                                ucPancakeRackStair.IsChecked = true;
                                CheckUcPancakeRackStair(ucPancakeRackStair);
                                ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);

                                //if (CommonVerify.HasDataGridRow(dgInputRank)) Util.DataGridCheckAllUnChecked(dgInputRank);
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
                        UcPancakeRackStair ucPancakeRackStair = _rackStairs2[r][c];

                        doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        doubleAnimation.AutoReverse = true;

                        if (!string.IsNullOrEmpty(_selectedRackId) && string.Equals(ucPancakeRackStair.RackId.ToUpper(), _selectedRackId.ToUpper(), StringComparison.Ordinal))
                        {
                            if (!ucPancakeRackStair.IsChecked)
                            {
                                //UnCheckedAllPancakeRackStair();
                                SetScrollToHorizontalOffset(scrollViewer2, c);

                                ucPancakeRackStair.IsChecked = true;
                                CheckUcPancakeRackStair(ucPancakeRackStair);
                                ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);

                                //if (CommonVerify.HasDataGridRow(dgInputRank)) Util.DataGridCheckAllUnChecked(dgInputRank);
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

        private void btnPancakeWarehousingHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWareHousingHistory()) return;

            object[] parameters = new object[2];
            parameters[0] = string.Empty;
            parameters[1] = string.Empty;
           // FrameOperation.OpenMenu("SFU010180300", true, parameters);//실전
            FrameOperation.OpenMenu("SFU010180220", true, parameters); //운영
        }

        private void btnInputLotSearch_Click(object sender, RoutedEventArgs e)
        {
            MCS001_022_INPUT_LOT popupInputLot = new MCS001_022_INPUT_LOT { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = null;

            C1WindowExtension.SetParameters(popupInputLot, parameters);

            popupInputLot.Closed += popupInputLot_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupInputLot.ShowModal()));
        }

        private void popupInputLot_Closed(object sender, EventArgs e)
        {
            MCS001_022_INPUT_LOT popup = sender as MCS001_022_INPUT_LOT;
            if (popup != null)
            {

            }
        }

        private void dgProjectStock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (dgProjectStock.SelectedItem == null) return;

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
            RightArea.RowDefinitions[4].Height = new GridLength(30);
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
                ShowLoadingIndicator();
                //DoEvents();

                int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;
                for (int i = 0; i < ((DataGridCellPresenter)cb.Parent).DataGrid.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(((DataGridCellPresenter)cb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                }

                DataRowView drv = cb.DataContext as DataRowView;
                if (drv != null)
                {
                    //UnCheckedAllRackStair();
                    SelectPancakeFromRankChecked(drv["RACK_ID"].GetString());
                }
                HiddenLoadingIndicator();
            }
        }

        private void rankCheck_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            if (cb.IsChecked != null)
            {
                ShowLoadingIndicator();
                //DoEvents();

                DataRow dtRow = (cb.DataContext as DataRowView)?.Row;
                if (dtRow != null)
                {
                    SelectPancakeFromRankUnChecked(dtRow["RACK_ID"].GetString());
                }

                HiddenLoadingIndicator();
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

                    ReSetElectrodPancakeControl();
                    SelectAllPancakeMonitoringControl();
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

            ReSetElectrodPancakeControl();

            SelectPancakeMonitoring();
            SelectPancakeInventory();
            SelectPancakeInventoryByProject();
            SelectPancakeRank();
            SelectInoutReservationInfo();
        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;

            var query = (from t in ((DataView)dgRackInfo.ItemsSource).Table.AsEnumerable()
                where t.Field<string>("WIPHOLD") == "Y"
                         select t).ToList();
            if (query.Any())
            {
                Util.MessageConfirm("SFU6003", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ManualIssue();
                    }
                });
            }
            else
            {
                ManualIssue();
            }
        }

        private void dgProjectStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectMaxRankxyz();
        }

        private void chkHoldException_Checked(object sender, RoutedEventArgs e)
        {
            SelectPancakeInventoryByProject();
        }

        private void chkHoldException_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectPancakeInventoryByProject();
        }

        private void UcPancakeRackStair1_DoubleClick(object sender, RoutedEventArgs e)
        {
            UcPancakeRackStair ucPancakeRackStair = sender as UcPancakeRackStair;

            if (ucPancakeRackStair != null)
            {
                DataTable dtRackInfo = null;
                dtRackInfo = _dtRackInfo.Clone();

                DataRow dr = dtRackInfo.NewRow();
                dr["RACK_ID"] = ucPancakeRackStair.RackId;
                dr["PRJT_NAME"] = ucPancakeRackStair.ProjectName;
                dr["MODLID"] = ucPancakeRackStair.ModelCode;
                dr["PRODID"] = ucPancakeRackStair.ProductCode;
                dr["PRODNAME"] = ucPancakeRackStair.ProductName;
                dr["WH_RCV_DTTM"] = ucPancakeRackStair.WarehouseReceiveDate;
                dr["LOT_CNT"] = ucPancakeRackStair.LotCount;
                dr["WIPQTY"] = ucPancakeRackStair.WipQty;
                dr["VLD_DATE"] = ucPancakeRackStair.ValidDate;
                dr["PAST_DAY"] = ucPancakeRackStair.ElapseDay;
                dr["WIPHOLD"] = ucPancakeRackStair.WipHold;
                dr["MCS_CST_ID"] = ucPancakeRackStair.DistributionCarrierId;
                dr["RACK_STAT_CODE"] = ucPancakeRackStair.RackStateCode;
                dr["POSITION"] = ucPancakeRackStair.Position;
                dtRackInfo.Rows.Add(dr);

                RackInfo(dtRackInfo);
            }

        }

        private void popupRackInfo_Closed(object sender, EventArgs e)
        {
            MCS001_022_RACK_INFO popup = sender as MCS001_022_RACK_INFO;
            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectPancakeMonitoring();
            }
        }

        private void UcPancakeRackStair1_Click(object sender, RoutedEventArgs e)
        {
            //UcRackStair rackStair = sender as UcRackStair;
            //if (rackStair != null)
            //{
            //    ControlsLibrary.MessageBox.Show(rackStair.Name);
            //}
        }

        private void UcPancakeRackStair1_Checked(object sender, RoutedEventArgs e)
        {
            UcPancakeRackStair rackStair = sender as UcPancakeRackStair;
            if (rackStair == null) return;

            ShowLoadingIndicator();
            //DoEvents();

            if (rackStair.IsChecked)
            {
                int maxSeq;

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    maxSeq = Convert.ToInt32(_dtRackInfo.Compute("max([SEQ])", string.Empty)) + 1;
                }
                else
                {
                    maxSeq = 1;
                }

                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackStair.RackId;
                dr["SEQ"] = maxSeq;
                dr["PRJT_NAME"] = rackStair.ProjectName;
                dr["MODLID"] = rackStair.ModelCode;
                dr["PRODID"] = rackStair.ProductCode;
                dr["PRODNAME"] = rackStair.ProductName;
                dr["WH_RCV_DTTM"] = rackStair.WarehouseReceiveDate;
                dr["LOT_CNT"] = rackStair.LotCount;
                dr["WIPQTY"] = rackStair.WipQty;
                dr["VLD_DATE"] = rackStair.ValidDate;
                dr["PAST_DAY"] = rackStair.ElapseDay;
                dr["WIPHOLD"] = rackStair.WipHold;
                dr["MCS_CST_ID"] = rackStair.DistributionCarrierId;
                dr["RACK_STAT_CODE"] = rackStair.RackStateCode;
                dr["POSITION"] = rackStair.Position;

                _dtRackInfo.Rows.Add(dr);

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, false);

            }
            else
            {
                DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + rackStair.RackId + "'");
                int seqno = 0;

                foreach (DataRow row in selectedRow)
                {
                    seqno = Convert.ToInt16(row["SEQ"]);
                    _dtRackInfo.Rows.Remove(row);
                }
                //seq 다시 처리
                foreach (DataRow row in _dtRackInfo.Rows)
                {
                    if (Convert.ToInt16(row["SEQ"]) > seqno)
                    {
                        row["SEQ"] = Convert.ToInt16(row["SEQ"]) - 1;
                    }
                }
                Util.GridSetData(dgRackInfo, _dtRackInfo, null);
            }
            HiddenLoadingIndicator();
        }

        private void UcPancakeRackStair2_DoubleClick(object sender, RoutedEventArgs e)
        {
            UcPancakeRackStair ucPancakeRackStair = sender as UcPancakeRackStair;

            if (ucPancakeRackStair != null)
            {
                DataTable dtRackInfo = null;
                dtRackInfo = _dtRackInfo.Clone();

                DataRow dr = dtRackInfo.NewRow();
                dr["RACK_ID"] = ucPancakeRackStair.RackId;
                dr["PRJT_NAME"] = ucPancakeRackStair.ProjectName;
                dr["MODLID"] = ucPancakeRackStair.ModelCode;
                dr["PRODID"] = ucPancakeRackStair.ProductCode;
                dr["PRODNAME"] = ucPancakeRackStair.ProductName;
                dr["WH_RCV_DTTM"] = ucPancakeRackStair.WarehouseReceiveDate;
                dr["LOT_CNT"] = ucPancakeRackStair.LotCount;
                dr["WIPQTY"] = ucPancakeRackStair.WipQty;
                dr["VLD_DATE"] = ucPancakeRackStair.ValidDate;
                dr["PAST_DAY"] = ucPancakeRackStair.ElapseDay;
                dr["WIPHOLD"] = ucPancakeRackStair.WipHold;
                dr["MCS_CST_ID"] = ucPancakeRackStair.DistributionCarrierId;
                dr["RACK_STAT_CODE"] = ucPancakeRackStair.RackStateCode;
                dr["POSITION"] = ucPancakeRackStair.Position;
                dtRackInfo.Rows.Add(dr);
                RackInfo(dtRackInfo);
            }
        }

        private void UcPancakeRackStair2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcPancakeRackStair2_Checked(object sender, RoutedEventArgs e)
        {
            UcPancakeRackStair rackStair = sender as UcPancakeRackStair;
            if (rackStair == null) return;

            ShowLoadingIndicator();
            //DoEvents();

            if (rackStair.IsChecked)
            {
                int maxSeq;

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    maxSeq = Convert.ToInt32(_dtRackInfo.Compute("max([SEQ])", string.Empty)) + 1;
                }
                else
                {
                    maxSeq = 1;
                }

                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackStair.RackId;
                dr["SEQ"] = maxSeq;
                dr["PRJT_NAME"] = rackStair.ProjectName;
                dr["MODLID"] = rackStair.ModelCode;
                dr["PRODID"] = rackStair.ProductCode;
                dr["PRODNAME"] = rackStair.ProductName;
                dr["WH_RCV_DTTM"] = rackStair.WarehouseReceiveDate;
                dr["LOT_CNT"] = rackStair.LotCount;
                dr["WIPQTY"] = rackStair.WipQty;
                dr["VLD_DATE"] = rackStair.ValidDate;
                dr["PAST_DAY"] = rackStair.ElapseDay;
                dr["WIPHOLD"] = rackStair.WipHold;
                dr["MCS_CST_ID"] = rackStair.DistributionCarrierId;
                dr["RACK_STAT_CODE"] = rackStair.RackStateCode;
                dr["POSITION"] = rackStair.Position;

                _dtRackInfo.Rows.Add(dr);

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);

            }
            else
            {
                DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + rackStair.RackId + "'");
                int seqno = 0;

                foreach (DataRow row in selectedRow)
                {
                    seqno = Convert.ToInt16(row["SEQ"]);
                    _dtRackInfo.Rows.Remove(row);
                }
                //seq 다시 처리
                foreach (DataRow row in _dtRackInfo.Rows)
                {
                    if (Convert.ToInt16(row["SEQ"]) > seqno)
                    {
                        row["SEQ"] = Convert.ToInt16(row["SEQ"]) - 1;
                    }
                }
                Util.GridSetData(dgRackInfo, _dtRackInfo, null);
            }
            HiddenLoadingIndicator();
        }

        private void dgInputRank_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

        }


        private void btnExpandFrame2_Checked(object sender, RoutedEventArgs e)
        {
            RightArea.RowDefinitions[4].Height = new GridLength(150);
        }

        private void btnExpandFrame2_Unchecked(object sender, RoutedEventArgs e)
        {
            RightArea.RowDefinitions[4].Height = new GridLength(30);
        }


        private void btnInoutSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInoutReservationInfo()) return;
            SelectInoutReservationInfo();
        }
        #endregion

        #region Method

        private void CheckUcPancakeRackStair(UcPancakeRackStair ucPancakeRackStair)
        {

            //if (CommonVerify.HasTableRow(_dtRackInfo))
            //{
            //    for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
            //    {
            //        _dtRackInfo.Rows.RemoveAt(i);
            //    }
            //}

            int maxSeq;
            if (CommonVerify.HasTableRow(_dtRackInfo))
            {
                maxSeq = Convert.ToInt32(_dtRackInfo.Compute("max([SEQ])", string.Empty)) + 1;
            }
            else
            {
                maxSeq = 1;
            }

            DataRow dr = _dtRackInfo.NewRow();
            dr["RACK_ID"] = ucPancakeRackStair.RackId;
            dr["SEQ"] = maxSeq;
            dr["PRJT_NAME"] = ucPancakeRackStair.ProjectName;
            dr["MODLID"] = ucPancakeRackStair.ModelCode;
            dr["PRODID"] = ucPancakeRackStair.ProductCode;
            dr["PRODNAME"] = ucPancakeRackStair.ProductName;
            dr["WH_RCV_DTTM"] = ucPancakeRackStair.WarehouseReceiveDate;
            dr["LOT_CNT"] = ucPancakeRackStair.LotCount;
            dr["WIPQTY"] = ucPancakeRackStair.WipQty;
            dr["VLD_DATE"] = ucPancakeRackStair.ValidDate;
            dr["PAST_DAY"] = ucPancakeRackStair.ElapseDay;
            dr["WIPHOLD"] = ucPancakeRackStair.WipHold;
            dr["MCS_CST_ID"] = ucPancakeRackStair.DistributionCarrierId;
            dr["RACK_STAT_CODE"] = ucPancakeRackStair.RackStateCode;
            dr["POSITION"] = ucPancakeRackStair.Position;

            _dtRackInfo.Rows.Add(dr);

            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }

        private void UnCheckUcPancakeRackStair(UcPancakeRackStair ucPancakeRackStair)
        {
            DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + ucPancakeRackStair.RackId + "'");
            foreach (DataRow row in selectedRow)
            {
                _dtRackInfo.Rows.Remove(row);
            }
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }

        private void SelectAllPancakeMonitoringControl()
        {
            try
            {
                _monitorTimer.Stop();
                InitializeRackUserControl();
                SelectPancakeMonitoring();
                SelectPancakeInventory();
                SelectPancakeInventoryByProject();
                SelectPancakeRank();
                SelectInoutReservationInfo();

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
            }
        }

        private void SelectPancakeMonitoring()
        {
            try
            {
                ShowLoadingIndicator();
                //DoEvents();
                
                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_MONITORING";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("Z_PSTN", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("WIPHOLD", typeof(string));
                inDataTable.Columns.Add("QMS", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["Z_PSTN"] = null;
                dr["PROCID"] = null;
                dr["MODLID"] = null;
                dr["LOTID"] = null;
                dr["PRJT_NAME"] = string.IsNullOrEmpty(txtProjectName.Text.Trim()) ? null : txtProjectName.Text.Trim();
                dr["PROD_VER_CODE"] = string.IsNullOrEmpty(txtProductVersion.Text.Trim()) ? null : txtProductVersion.Text.Trim();
                dr["WIPHOLD"] = cboHold.SelectedValue;
                dr["QMS"] = cboQMS.SelectedValue;
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

                        UcPancakeRackStair ucPancakeRackStair = item["X_PSTN"].ToString() == "1" ? _rackStairs1[x][y] : _rackStairs2[x][y];

                        if (ucPancakeRackStair == null) continue;

                        ucPancakeRackStair.LotCount = item["LOT_CNT"].GetString();
                        ucPancakeRackStair.RackId = item["RACK_ID"].GetString();
                        ucPancakeRackStair.RackStateCode = item["RACK_STAT_CODE"].GetString();
                        ucPancakeRackStair.LinqEquipmentSegmentCode = item["LINK_EQSGID"].GetString();
                        ucPancakeRackStair.ReceivePriority = Util.NVC_Int(item["RCV_PRIORITY"]);
                        ucPancakeRackStair.ZoneId = item["ZONE_ID"].GetString();
                        ucPancakeRackStair.DistributionCarrierId = item["MCS_CST_ID"].GetString();
                        ucPancakeRackStair.UseFlag = item["USE_FLAG"].GetString();
                        ucPancakeRackStair.EquipmentCode = item["EQPTID"].GetString();
                        ucPancakeRackStair.ValidDate = item["VLD_DATE"].GetString();
                        ucPancakeRackStair.CalculationDate = item["CALDATE"].GetString();
                        ucPancakeRackStair.WarehouseReceiveDate = item["WH_RCV_DTTM"].GetString();
                        ucPancakeRackStair.ProductCode = item["PRODID"].GetString();
                        ucPancakeRackStair.ProcessSegmentCode = item["PCSGID"].GetString();
                        ucPancakeRackStair.ProcessCode = item["PROCID"].GetString();
                        ucPancakeRackStair.WipState = item["WIPSTAT"].GetString();
                        ucPancakeRackStair.WipStartDateTime = item["WIPSDTTM"].GetString();
                        ucPancakeRackStair.WipQty = Util.NVC_Decimal(item["WIPQTY"]);
                        ucPancakeRackStair.WipQty2 = Util.NVC_Decimal(item["WIPQTY2"]);
                        ucPancakeRackStair.EquipmentSegmentCode = item["EQSGID"].GetString();
                        ucPancakeRackStair.WipHold = item["WIPHOLD"].GetString();
                        ucPancakeRackStair.ProductName = item["PRODNAME"].GetString();
                        ucPancakeRackStair.ModelCode = item["MODLID"].GetString();
                        ucPancakeRackStair.UnitCode = item["UNIT_CODE"].GetString();
                        ucPancakeRackStair.ProjectName = item["PRJT_NAME"].GetString();
                        ucPancakeRackStair.ProcessName = item["PROCNAME"].GetString();
                        ucPancakeRackStair.EquipmentSegmentName = item["EQSGNAME"].GetString();
                        ucPancakeRackStair.ElapseDay = Util.NVC_Int(item["PAST_DAY"]);
                        ucPancakeRackStair.Row = int.Parse(item["Z_PSTN"].GetString());
                        ucPancakeRackStair.Col = int.Parse(item["Y_PSTN"].GetString());
                        ucPancakeRackStair.Stair = int.Parse(item["X_PSTN"].GetString());

                        ucPancakeRackStair.LotNormalCount = item["NORM_LOT_CNT"].GetString();
                        ucPancakeRackStair.LotHoldCount = item["HOLD_LOT_CNT"].GetString();
                        ucPancakeRackStair.ProductClassCode = item["PRDT_CLSS_CODE"].GetString();
                        ucPancakeRackStair.SpecialFlag = item["SPCL_FLAG"].GetString();
                        ucPancakeRackStair.LegendColor = item["BG_COLOR"].GetString();
                        ucPancakeRackStair.QMS = item["QMS"].GetString();

                        ucPancakeRackStair.Position = item["X_PSTN"].GetString() + "-" + item["Y_PSTN"].GetString() + "-" + item["Z_PSTN"].GetString();

                        ucPancakeRackStair.Visibility = Visibility.Visible;

                        if (string.Equals(item["RACK_STAT_CODE"].GetString(), "USING"))
                        {
                            if (!string.IsNullOrEmpty(txtProjectName.Text.Trim()))
                            {
                                if (item["PRJT_NAME"] != null && !item["PRJT_NAME"].GetString().Contains(txtProjectName.Text))
                                    ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if (!string.IsNullOrEmpty(txtProductVersion.Text.Trim()))
                            {
                                if (!item["PROD_VER_CODE"].GetString().Contains(txtProductVersion.Text))
                                    ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }

                            if (cboHold.SelectedValue != null)
                            {
                                if (cboHold.SelectedValue.GetString() != item["WIPHOLD"].GetString())
                                    ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                            }
                        }

                        //if (!string.IsNullOrEmpty(txtProjectName.Text.Trim()))
                        //{
                        //    if (string.Equals(item["RACK_STAT_CODE"].GetString(), "USING"))
                        //    {
                        //        if (item["PRJT_NAME"] != null && item["PRJT_NAME"].GetString().Contains(txtProjectName.Text))
                        //        {
                        //            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.White);
                        //        }
                        //        else
                        //        {
                        //            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightGoldenrodYellow);
                        //        }
                        //    }
                        //    if (!string.IsNullOrEmpty(txtProductVersion.Text.Trim()))
                        //    {
                        //        ucPancakeRackStair.RootLayout.Background = item["PROD_VER_CODE"].GetString().Contains(txtProductVersion.Text) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                        //    }

                        //    if (cboHold.SelectedValue != null)
                        //    {
                        //        if (cboHold.SelectedValue.GetString() == "Y")
                        //        {
                        //            ucPancakeRackStair.RootLayout.Background = item["WIPHOLD"].GetString() == "Y" ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                        //        }
                        //        else if (cboHold.SelectedValue.GetString() == "N")
                        //        {
                        //            ucPancakeRackStair.RootLayout.Background = item["WIPHOLD"].GetString() == "N" ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGoldenrodYellow);
                        //        }
                        //    }
                        //}
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

        private void SelectPancakeInventory()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_INVENTORY";
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

        private void SelectPancakeInventoryByProject()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_INVENTORY_BY_PJT";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("WIPHOLD", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
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
                    Util.GridSetData(dgProjectStock, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectPancakeRank()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_OUT_RANK";
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

                    //Util.GridSetData(dgInputRank, bizResult, null, true);
                    Util.GridSetData(dgInputRank, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectInoutReservationInfo()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_RSV_CMD";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("INOUT_TYPE", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["INOUT_TYPE"] = cboInoutType.SelectedValue;
                dr["PRJT_NAME"] = !string.IsNullOrEmpty(txtInoutProjectName.Text.Trim()) ? txtInoutProjectName.Text : null;
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
                    Util.GridSetData(dgInOutReservation, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UnCheckedAllPancakeRackStair()
        {
            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs1[rowIndex][colIndex];

                    if (ucPancakeRackStair.IsChecked)
                    {
                        ucPancakeRackStair.IsChecked = false;
                        UnCheckUcPancakeRackStair(ucPancakeRackStair);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs2[rowIndex][colIndex];

                    if (ucPancakeRackStair.IsChecked)
                    {
                        ucPancakeRackStair.IsChecked = false;
                        UnCheckUcPancakeRackStair(ucPancakeRackStair);
                    }
                }
            }
        }

        private void SelectPancakeFromRankChecked(string rackId)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();

            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs1[rowIndex][colIndex];

                    doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    doubleAnimation.To = 0;
                    doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer1, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs2[rowIndex][colIndex];

                    doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    doubleAnimation.To = 0;
                    doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer2, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }
            
        }

        private void SelectPancakeFromRankUnChecked(string rackId)
        {
            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs1[rowIndex][colIndex];

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (ucPancakeRackStair.IsChecked)
                        {
                            ucPancakeRackStair.IsChecked = false;
                            UnCheckUcPancakeRackStair(ucPancakeRackStair);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs2[rowIndex][colIndex];
                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (ucPancakeRackStair.IsChecked)
                        {
                            ucPancakeRackStair.IsChecked = false;
                            UnCheckUcPancakeRackStair(ucPancakeRackStair);
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
                if (string.IsNullOrEmpty(cboStocker?.SelectedValue?.GetString())) return;

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

        private string GetRackIdByLotId()
        {
            string returnRackId = string.Empty;

            try
            {
                const string bizRuleName = "BR_MCS_GET_RACK_BY_LOTID";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["LOTID"] = txtLotId.Text;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    returnRackId = searchResult.Rows[0]["RACK_ID"].GetString();
                }
                return returnRackId;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return returnRackId;
            }
        }

        private void ManualIssue()
        {

            MCS001_022_MANUAL_ISSUE popupManualIssue = new MCS001_022_MANUAL_ISSUE { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = ((DataView)dgRackInfo.ItemsSource).Table;
            C1WindowExtension.SetParameters(popupManualIssue, parameters);

            popupManualIssue.Closed += popupManualIssue_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupManualIssue.ShowModal()));

        }

        private void popupManualIssue_Closed(object sender, EventArgs e)
        {
            MCS001_022_MANUAL_ISSUE popup = sender as MCS001_022_MANUAL_ISSUE;
            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectPancakeMonitoring();
            }
        }

        private void RackInfo(DataTable dt)
        {
            MCS001_022_RACK_INFO popupRackInfo = new MCS001_022_RACK_INFO { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = dt;
            C1WindowExtension.SetParameters(popupRackInfo, parameters);

            popupRackInfo.Closed += popupRackInfo_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupRackInfo.ShowModal()));
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
            _dtRackInfo.Columns.Add("LOT_CNT", typeof(string));
            _dtRackInfo.Columns.Add("WIPQTY", typeof(decimal));
            _dtRackInfo.Columns.Add("VLD_DATE", typeof(string));
            _dtRackInfo.Columns.Add("PAST_DAY", typeof(int));
            _dtRackInfo.Columns.Add("WIPHOLD", typeof(string));
            _dtRackInfo.Columns.Add("MCS_CST_ID", typeof(string));
            _dtRackInfo.Columns.Add("RACK_STAT_CODE", typeof(string));
            _dtRackInfo.Columns.Add("SEQ", typeof(int));
            _dtRackInfo.Columns.Add("POSITION", typeof(string));
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
            //SelectMaxRankxyz();

            //입/출고 콤보박스
            SetInoutTypeCombo(cboInoutType);

            //WIP HOLD
            SetHoldCombo(cboHold);

            // QMS 콤보박스
            SetQMScombo(cboQMS);

            //범례공통코드
            CommonCombo _combo = new CommonCombo();
            String[] sFilter1 = { "CSTPROD","SKID" };
            _combo.SetCombo(cboCmd, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTR_MCS");
        }

        private void ReSetElectrodPancakeControl()
        {
            _dtRackInfo.Clear();
            SetColumnDefinition(_maxColumnCount);
            ReSetRackStairLayout(_maxRowCount, _maxColumnCount);
        }

        private void InitializeRackUserControl()
        {
            for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs1[row][col];
                    ucPancakeRackStair.IsEnabled = true;
                    ucPancakeRackStair.SetRackBackcolor();
                }
            }

            for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs2[row][col];
                    ucPancakeRackStair.IsEnabled = true;
                    ucPancakeRackStair.SetRackBackcolor();
                }
            }
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
            for (int i = 0; i < maxcolIndex; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                string colName = i + 1 + ObjectDic.Instance.GetObjectName("연");
                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = colName;
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() {  Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = colName;
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

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                string colName = i + 1 + ObjectDic.Instance.GetObjectName("연");
                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = colName;
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() {  Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = colName;
                Grid.SetColumn(textBlock2, colIndex);
                grdStair2.Children.Add(textBlock2);
                colIndex++;
            }
        }

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
                textBlock1.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
                grdColumn1.Children.Add(textBlock1);

                TextBlock textBlock2 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() {Converter = new ObjectDicConverter() });
                textBlock2.SetValue(Grid.RowProperty, i);
                textBlock2.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
                grdColumn2.Children.Add(textBlock2);
            }
        }

        private void ReSetRackStairLayout(int maxRowIndex, int maxColumnIndex)
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();

            for (int i = 0; i < maxColumnIndex; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
            }

            for (int i = 0; i < maxRowIndex; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
            }

            for (int row = 0; row < maxRowIndex; row++)
            {
                for (int col = 0; col < maxColumnIndex; col++)
                {
                    Grid.SetRow(_rackStairs1[row][col], row);
                    Grid.SetColumn(_rackStairs1[row][col], col);
                    grdRackstair1.Children.Add(_rackStairs1[row][col]);

                    Grid.SetRow(_rackStairs2[row][col], row);
                    Grid.SetColumn(_rackStairs2[row][col], col);
                    grdRackstair2.Children.Add(_rackStairs2[row][col]);
                }
            }
        }

        private void PrepareRackStair()
        {
            _rackStairs1 = new UcPancakeRackStair[_maxRowCount][];
            _rackStairs2 = new UcPancakeRackStair[_maxRowCount][];

            for (int r = 0; r < _rackStairs1.Length; r++)
            {
                _rackStairs1[r] = new UcPancakeRackStair[_maxColumnCount];
                _rackStairs2[r] = new UcPancakeRackStair[_maxColumnCount];

                for (int c = 0; c < _rackStairs1[r].Length; c++)
                {
                    UcPancakeRackStair ucPancakeRackStair1 = new UcPancakeRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucPancakeRackStair1.Checked += UcPancakeRackStair1_Checked;
                    ucPancakeRackStair1.Click += UcPancakeRackStair1_Click;
                    ucPancakeRackStair1.DoubleClick += UcPancakeRackStair1_DoubleClick;
                    _rackStairs1[r][c] = ucPancakeRackStair1;
                }

                for (int c = 0; c < _rackStairs2[r].Length; c++)
                {
                    UcPancakeRackStair ucPancakeRackStair2 = new UcPancakeRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucPancakeRackStair2.Checked += UcPancakeRackStair2_Checked;
                    ucPancakeRackStair2.Click += UcPancakeRackStair2_Click;
                    ucPancakeRackStair2.DoubleClick += UcPancakeRackStair2_DoubleClick;
                    _rackStairs2[r][c] = ucPancakeRackStair2;
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


            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60)};
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60)};
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60)};
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60)};
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
            }

            for (int row = 0; row < _maxRowCount; row++)
            {
                for (int col = 0; col < _maxColumnCount; col++)
                {
                    Grid.SetRow(_rackStairs1[row][col], row);
                    Grid.SetColumn(_rackStairs1[row][col], col);
                    grdRackstair1.Children.Add(_rackStairs1[row][col]);

                    Grid.SetRow(_rackStairs2[row][col], row);
                    Grid.SetColumn(_rackStairs2[row][col], col);
                    grdRackstair2.Children.Add(_rackStairs2[row][col]);
                }
            }
        }

        private int GetXPosition(string zPosition)
        {
            int xposition = _maxRowCount == Convert.ToInt16(zPosition) ? 0 : _maxRowCount - Convert.ToInt16(zPosition);
            return xposition;
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

            var query = (from t in ((DataView)dgRackInfo.ItemsSource).Table.AsEnumerable()
                         where (!t.Field<string>("RACK_STAT_CODE").Equals("USING") && !t.Field<string>("RACK_STAT_CODE").Equals("NOREAD"))
                         select t).ToList();

            if (query.Any())
            {
                Util.MessageValidation("SFU1643");
                return false;
            }


            return true;
        }

        private bool ValidationWareHousingHistory()
        {
            return true;
        }

        private bool ValidationInoutReservationInfo()
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
            drColor["KEYGROUPID"] = "PCW_MNT_LEGEND";
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
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "PCW", LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetInoutTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "MCS_STK_MOVE_TYPE_CODE" };
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

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        private void SetScrollToHorizontalOffset(ScrollViewer scrollViewer, int colIndex)
        {
            double averageScrollWidth = scrollViewer.ScrollableWidth / _maxColumnCount;
            scrollViewer.ScrollToHorizontalOffset( Math.Abs(colIndex - _maxColumnCount) * averageScrollWidth);            
        }

        private static void SetQMScombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "QMS_JUDG_CMPLT" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);

        }

        //private static void DoEvents()
        //{
        //    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        //}

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

        private void btnSKIDSearch_Click(object sender, RoutedEventArgs e)
        {
            MCS001_018_SKID_STATE popupSKIDSearch = new MCS001_018_SKID_STATE { FrameOperation = FrameOperation };
            //object[] parameters = new object[2];
            //parameters[0] = cboStocker.SelectedValue;
            //parameters[1] = null;

            //C1WindowExtension.SetParameters(popupSKIDSearch, parameters);

            popupSKIDSearch.Closed += popupSKIDSearch_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupSKIDSearch.ShowModal()));
        }
        private void popupSKIDSearch_Closed(object sender, EventArgs e)
        {
            MCS001_018_SKID_STATE popup = sender as MCS001_018_SKID_STATE;
            if (popup != null)
            {

            }
        }

        private void btnSkidInfo_Click(object sender, RoutedEventArgs e)
        {
            //CMM_MCS_SKID_INFO popupSkidInfo = new CMM_MCS_SKID_INFO { FrameOperation = FrameOperation };
            //popupSkidInfo.Closed += popupSkidInfo_Closed;
            //Dispatcher.BeginInvoke(new Action(() => popupSkidInfo.ShowModal()));
        }
        private void popupSkidInfo_Closed(object sender, EventArgs e)
        {//
            //CMM_MCS_SKID_INFO popup = sender as CMM_MCS_SKID_INFO;
            //if (popup != null && popup._Excute=="Y")
            //{
            //    _dtRackInfo.Clear();
            //    SelectPancakeMonitoring();
            //}
        }
    }
}
