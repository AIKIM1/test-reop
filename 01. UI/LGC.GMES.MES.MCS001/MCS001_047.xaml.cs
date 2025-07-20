/*************************************************************************************
 Created Date : 2020.09.26
      Creator : 김상민 사원
   Decription : 점보롤 창고 모니터링 및 수동 출고
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.26  김상민 사원 : Initial Created.    
  2021.06.01  강동희      : HALF 슬리팅 면 컬럼 추가 대응.
  2021.06.28  강동희      : QMS HOLD 여부 컬럼 추가 대응.
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
    /// MCS001_047.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_047 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        private UcRackStair[][] _rackStairs1;
        private UcRackStair[][] _rackStairs2;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private const int MaxRow = 2;
        private const int MaxCol = 33;

        private bool _isSetAutoSelectTime = false;
        private bool _isFirstLoading = true;
        private DataTable _dtRackInfo;
        private int _maxRow;
        private int _maxCol;
        private string _userId;
        private string eqgrCboStockerSelectedValue;


        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;
        #endregion

        public MCS001_047()
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
            //GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
            TimerSetting();
            MakeRowDefinition();
            MakeColumnDefinition();
            PrepareRackStair();
            PrepareRackStairLayout();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSelect, btnSearch, btnManualIssue };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_isFirstLoading)
            {
                InitializeCombo();
                SelectAllJumboRollMonitoringControl();
                VisibleHideControl();
            }
            else
            {
                ReSetJumboRollControl();
                SelectJumboRollMonitoring();
                SelectJumboRollInventory();
                SelectOrderInfo();
                SelectDestPortInfo();
                SelectJumboRollRank();
                SelectJumboRollPortInfo();
                VisibleHideControl();
            }
            _isFirstLoading = false;


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
                UnCheckedAllRackStair();
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

                                SetScrollToHorizontalOffset(scrollViewer1, c);

                                ucRackStair.IsChecked = true;
                                CheckUcRackStair(ucRackStair);
                                ucRackStair.BeginAnimation(HeightProperty, doubleAnimation);

                                if (CommonVerify.HasDataGridRow(dgInputRank)) Util.DataGridCheckAllUnChecked(dgInputRank);

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
       
                                UnCheckedAllRackStair();
                                SetScrollToHorizontalOffset(scrollViewer2, c);

                                ucRackStair.IsChecked = true;
                                CheckUcRackStair(ucRackStair);
                                ucRackStair.BeginAnimation(HeightProperty, doubleAnimation);

                                if (CommonVerify.HasDataGridRow(dgInputRank)) Util.DataGridCheckAllUnChecked(dgInputRank);
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
                //int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;
                //for (int i = 0; i < ((DataGridCellPresenter)cb.Parent).DataGrid.Rows.Count; i++)
                //{
                //    DataTableConverter.SetValue(((DataGridCellPresenter)cb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                //}

                DataRowView drv = cb.DataContext as DataRowView;
                if (drv != null)
                {

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

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelectJumboRollMonitoring()) return;
            
            ReSetJumboRollControl();
            
            SelectJumboRollMonitoring();
            SelectJumboRollInventory();
            SelectOrderInfo();
            SelectDestPortInfo();
            SelectJumboRollRank();
            SelectJumboRollPortInfo();
            VisibleHideControl();
        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            List<int> checkRows = new List<int>();

            checkRows = getCheckGridRowsIndex();

            if (checkRows.IsNullOrEmpty())
            {
                Util.MessageValidation("MCS2000002");
                return;
            }
            insertManualOrder(checkRows);
            btnSearch_Click(null, null);

        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
   //         SelectMaxRankxyz();
        }

        private void chkHoldException_Checked(object sender, RoutedEventArgs e)
        {
            SelectOrderInfo();
        }

        private void chkHoldException_Unchecked(object sender, RoutedEventArgs e)
        {
            SelectOrderInfo();
        }

        private void UcRackStair1_DoubleClick(object sender, RoutedEventArgs e)
        {

        }

        private void UcRackStair1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcRackStair1_Checked(object sender, RoutedEventArgs e)
        {
            UcRackStair rackStair = sender as UcRackStair;
            if (rackStair == null) return;

            if (rackStair.IsChecked)
            {
                if (checkGridExistRack(rackStair)) return;

                int maxSeq = 1;

                DataRow dr = _dtRackInfo.NewRow();
                dr["CHKXX"] = false;
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
                dr["PROCIDNAME"] = rackStair.ProcessCode + " : " + rackStair.ProcessName;
                dr["WOID"] = rackStair.WorkOrderId;
                dr["RACK_STAT_CODE"] = rackStair.RackStateCode;
                dr["ELECTRODETYPE"] = rackStair.ProductClassCode;
                dr["PORTID"] = string.Empty;
                dr["HALF_SLIT_SIDE"] = rackStair.HaltSlitSide; //2021.06.01 HALF 슬리팅 면 컬럼 추가 대응
                dr["QMS_HOLD_FLAG"] = rackStair.QmsHoldFlag; //2021.06.28 QMS HOLD 여부 컬럼 추가 대응.

                _dtRackInfo.Rows.Add(dr);
               

                

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
            }
            else
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    if (rackStair.RackId.ToString() == _dtRackInfo.Rows[i]["RACK_ID"].ToString())
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
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
                if (checkGridExistRack(rackStair)) return;

                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["CHKXX"] = false;
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
                dr["PROCIDNAME"] = rackStair.ProcessCode + " : " + rackStair.ProcessName;
                dr["WOID"] = rackStair.WorkOrderId;
                dr["RACK_STAT_CODE"] = rackStair.RackStateCode;
                dr["ELECTRODETYPE"] = rackStair.ProductClassCode;
                dr["PORTID"] = string.Empty;
                dr["HALF_SLIT_SIDE"] = rackStair.HaltSlitSide; //2021.06.01 HALF 슬리팅 면 컬럼 추가 대응
                dr["QMS_HOLD_FLAG"] = rackStair.QmsHoldFlag; //2021.06.28 QMS HOLD 여부 컬럼 추가 대응.
                _dtRackInfo.Rows.Add(dr);

                Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
            }
            else
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    if (rackStair.RackId.ToString() == _dtRackInfo.Rows[i]["RACK_ID"].ToString())
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
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
        private void chkList_Checked(object sender, RoutedEventArgs e)
        {
            int rowIndexSave = -1;
            try
            {
                if (dgRackInfo.ItemsSource == null || dgRackInfo.Rows.Count == 0)
                    return;

                if (dgRackInfo.CurrentRow.DataItem == null)
                    return;
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
                rowIndexSave = rowIndex;
                DataTable dt = ((DataView)dgRackInfo.ItemsSource).Table;
                if ((bool)DataTableConverter.GetValue(dgRackInfo.Rows[rowIndex].DataItem, "CHKXX"))
                {
                    dgRackInfo.SelectedIndex = rowIndex;

                    C1.WPF.DataGrid.DataGridCell gridCell = dgRackInfo.GetCell(rowIndex, dgRackInfo.Columns["MCS_CST_ID"].Index) as C1.WPF.DataGrid.DataGridCell;
                    C1.WPF.DataGrid.DataGridCell gridCell2 = dgRackInfo.GetCell(rowIndex, dgRackInfo.Columns["PORTID"].Index) as C1.WPF.DataGrid.DataGridCell;
                    if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                    {
                        C1ComboBox combo = gridCell2.Presenter.Content as C1ComboBox;

                        if (combo != null)
                            SetGridCboItemDestPort(combo, cboStocker.SelectedValue.GetString(), gridCell.Value.ToString());
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                if(rowIndexSave >= 0)
                {
                    DataTableConverter.SetValue(dgRackInfo.Rows[rowIndexSave].DataItem, "CHKXX", false);
                }
                Util.MessageException(ex);
                return;
            }
        }
        private void btnOrderCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelManualOrder();
            SelectOrderInfo();

        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
           
        }
        #endregion

        #region Method

        private void CheckUcRackStair(UcRackStair ucRackStair)
        {

            if (checkGridExistRack(ucRackStair)) return; 
            int maxSeq = 1;
            DataRow dr = _dtRackInfo.NewRow();
            dr["CHKXX"] = false;
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
            dr["PROCIDNAME"] = ucRackStair.ProcessCode + " : " + ucRackStair.ProcessName;
            dr["WOID"] = ucRackStair.WorkOrderId;
            dr["RACK_STAT_CODE"] = ucRackStair.RackStateCode;
            dr["ELECTRODETYPE"] = ucRackStair.ProductClassCode;
            dr["PORTID"] = string.Empty;
            dr["HALF_SLIT_SIDE"] = ucRackStair.HaltSlitSide; //2021.06.01 HALF 슬리팅 면 컬럼 추가 대응
            dr["QMS_HOLD_FLAG"] = ucRackStair.QmsHoldFlag; //2021.06.28 QMS HOLD 여부 컬럼 추가 대응.
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

                _monitorTimer.Stop();
                InitializeRackUserControl();
                ReSetJumboRollControl();
                SelectJumboRollMonitoring();
                SelectJumboRollInventory();
                SelectOrderInfo();
                SelectDestPortInfo();
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
            }
        }
        

        private void SelectJumboRollMonitoring()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();
                if (eqgrCboStockerSelectedValue == "JRB") {
                    //점보롤 버퍼 부분
                    //MCS DB의 점보롤 버퍼에 존재하는 CARRIER ID로 MES쪽 정보 조회
                    const string bizRuleName = "BR_GUI_GET_JUMBOROLL_BUFFER_ELTR_INFO";
                    DataTable inDataTable = new DataTable("RQSTDT");
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));
                    inDataTable.Columns.Add("MODLID", typeof(string));
                    inDataTable.Columns.Add("LOTID", typeof(string));
                    inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                    inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                    inDataTable.Columns.Add("HOLDYN", typeof(string));
                    DataRow dr = inDataTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = cboStocker.SelectedValue;
                    dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue;
                    dr["MODLID"] = null;
                    dr["LOTID"] = null;
                    dr["PRJT_NAME"] = string.IsNullOrEmpty(txtProjectName.Text.Trim()) ? null : txtProjectName.Text.Trim();
                    dr["PROD_VER_CODE"] = string.IsNullOrEmpty(txtProductVersion.Text.Trim()) ? null : txtProductVersion.Text.Trim();
                    dr["HOLDYN"] = cboHold.SelectedValue;
                    inDataTable.Rows.Add(dr);
                    DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                    HideAndClearAllRack();
                    Util.gridClear(dgRackInfo);

                    if (CommonVerify.HasTableRow(dtResult))
                    {
                        for (int i =0; i< dtResult.Rows.Count; i++)
                        {
                            DataRow item = dtResult.Rows[i];
                            UcRackStair ucRackStair = _rackStairs1[0][i];

                            if (ucRackStair == null) continue;

                            // HOLD 검색조건에 따른 데이터 바인딩 처리 수정

                            ucRackStair.LotId = item["LOTID"].GetString();
                            ucRackStair.RackId = item["PORT_ID"].GetString();
                            ucRackStair.DistributionCarrierId = item["CSTID"].GetString();
                            ucRackStair.UseFlag = item["USE_FLAG"].GetString();
                            ucRackStair.EquipmentCode = item["EQPTID"].GetString();
                            ucRackStair.ValidDate = item["VLD_DATE"].GetString();
                            ucRackStair.CalculationDate = item["CALDATE"].GetString();
                            ucRackStair.WarehouseReceiveDate = item["CSTINDTTM"].GetString();
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
                            ucRackStair.Row = 0;
                            ucRackStair.Col = i;
                            ucRackStair.Stair = 1;
                            ucRackStair.ProductClassCode = item["PRDT_CLSS_CODE"].GetString();
                            ucRackStair.LegendColor = item["BG_COLOR"].GetString();

                            ucRackStair.HaltSlitSide = item["HALF_SLIT_SIDE"].GetString(); //2021.06.01 HALF 슬리팅 면 컬럼 추가 대응
                            ucRackStair.QmsHoldFlag = item["QMS_HOLD_FLAG"].GetString(); //2021.06.28 QMS HOLD 여부 컬럼 추가 대응

                            ucRackStair.Visibility = Visibility.Visible;
                            ucRackStair.BringIntoView();

                        }
                    }

                }
                else {
                    //점보롤 창고 부분
                    const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_MONITORING_BY_TB_CARRIER";
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


                    DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                    HideAndClearAllRack();
                    Util.gridClear(dgRackInfo);

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        foreach (DataRow item in bizResult.Rows)
                        {
                            int x = 0;
                            x = _maxRow - int.Parse(item["Z_PSTN"].ToString());

                            int y = _maxCol - int.Parse(item["Y_PSTN"].ToString());

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
                            ucRackStair.WarehouseReceiveDate = item["CSTINDTTM"].GetString();
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

                            ucRackStair.HaltSlitSide = item["HALF_SLIT_SIDE"].GetString(); //2021.06.01 HALF 슬리팅 면 컬럼 추가 대응
                            ucRackStair.QmsHoldFlag = item["QMS_HOLD_FLAG"].GetString(); //2021.06.28 QMS HOLD 여부 컬럼 추가 대응

                            if (item["USE_FLAG"].GetString() == "Y")
                            {
                                ucRackStair.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                ucRackStair.Visibility = Visibility.Hidden;

                            }
                            //ucRackStair.BringIntoView();

                        }
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

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_STK_RATE";
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

        private void SelectOrderInfo()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_GUI_GET_ORDER_INFO";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inDataTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException)=>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.GridSetData(dgOrderInfo, bizResult, null);
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
                if (eqgrCboStockerSelectedValue == "JRB")
                {
                    //점보롤 버퍼일 때
                    const string bizRuleName = "BR_GUI_GET_JUMBOROLL_BUFFER_ELTR_INFO_BY_CONDITION";
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
                    DataTable bizResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
                    bizResult.Columns.Add("PROCIDNAME", typeof(string));
                    for (int i = 0; i < bizResult.Rows.Count; i++)
                    {
                        bizResult.Rows[i]["PROCIDNAME"] = bizResult.Rows[i]["PROCID"].ToString() + " : " + bizResult.Rows[i]["PROCNAME"].ToString();

                    }
                    Util.GridSetData(dgInputRank, bizResult, null);
                    HiddenLoadingIndicator();
                }
                else
                {
                    const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_MONITORING_BY_CONDTION";
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

                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                    {
                        HiddenLoadingIndicator();
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                    //dgStore.ItemsSource = DataTableConverter.Convert(bizResult);
                    bizResult.Columns.Add("PROCIDNAME", typeof(string));
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            bizResult.Rows[i]["PROCIDNAME"] = bizResult.Rows[i]["PROCID"].ToString() + " : " + bizResult.Rows[i]["PROCNAME"].ToString();

                        }
                        Util.GridSetData(dgInputRank, bizResult, null);
                    });
                }
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

                //txtRollPressIn.Text = string.Empty;
                //txtRollPressOut.Text = string.Empty;
                //txtCoaterOut.Text = string.Empty;
                //txtCoaterIn.Text = string.Empty;

                var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB");
                //if (convertFromString != null)
                //{
                //    txtRollPressIn.Background = new SolidColorBrush((Color)convertFromString);
                //    txtRollPressOut.Background = new SolidColorBrush((Color)convertFromString);
                //    txtCoaterOut.Background = new SolidColorBrush((Color)convertFromString);
                //    txtCoaterIn.Background = new SolidColorBrush((Color)convertFromString);
                //}

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
                            ////txtRollPressIn.Text = queryRollPressInType.RollPressInType + " : " + queryRollPressInType.RollPressCurrInoutType;
                            //txtRollPressIn.Text = queryRollPressInType.RollPressCurrInoutType + " : " + queryRollPressInType.RollPressInType;
                            //txtRollPressIn.FontWeight = FontWeights.Bold;
                            //if(queryRollPressInType.MaterialExistFlag == "Y") txtRollPressIn.Background = new SolidColorBrush(Colors.LightPink);

                        }

                        var queryRollPressOutType = (from t in bizResult.AsEnumerable()
                                                     where t.Field<string>("PORT_TYPE_CODE") == "STK_JR_ROL_OUT"
                                                     select new { RollPressOutType = t.Field<string>("PORT_STAT_CODE"), RollPressCurrInoutType = t.Field<string>("CURR_INOUT_TYPE_CODE"), MaterialExistFlag = t.Field<string>("MTRL_EXIST_FLAG") }).FirstOrDefault();

                        if (queryRollPressOutType != null)
                        {
                            ////txtRollPressOut.Text = queryRollPressOutType.RollPressOutType + " : " + queryRollPressOutType.RollPressCurrInoutType;
                            //txtRollPressOut.Text = queryRollPressOutType.RollPressCurrInoutType + " : " + queryRollPressOutType.RollPressOutType;
                            //txtRollPressOut.FontWeight = FontWeights.Bold;
                            //if (queryRollPressOutType.MaterialExistFlag == "Y") txtRollPressOut.Background = new SolidColorBrush(Colors.LightPink);
                        }

                        var queryCoaterOutType = (from t in bizResult.AsEnumerable()
                                                  where t.Field<string>("PORT_TYPE_CODE") == "STK_JR_COT_OUT"
                                                  select new { CoaterOutType = t.Field<string>("PORT_STAT_CODE"), CoaterCurrInoutType = t.Field<string>("CURR_INOUT_TYPE_CODE"), MaterialExistFlag = t.Field<string>("MTRL_EXIST_FLAG") }).FirstOrDefault();

                        if (queryCoaterOutType != null)
                        {
                            ////txtCoaterOut.Text = queryCoaterOutType.CoaterOutType + " : " + queryCoaterOutType.CoaterCurrInoutType;
                            //txtCoaterOut.Text = queryCoaterOutType.CoaterCurrInoutType + " : " + queryCoaterOutType.CoaterOutType;
                            //txtCoaterOut.FontWeight = FontWeights.Bold;
                            //if (queryCoaterOutType.MaterialExistFlag == "Y") txtCoaterOut.Background = new SolidColorBrush(Colors.LightPink);
                        }

                        var queryCoaterInType = (from t in bizResult.AsEnumerable()
                                                 where t.Field<string>("PORT_TYPE_CODE") == "STK_JR_COT_IN"
                                                 select new { CoaterInType = t.Field<string>("PORT_STAT_CODE"), CoaterCurrInoutType = t.Field<string>("CURR_INOUT_TYPE_CODE"), MaterialExistFlag = t.Field<string>("MTRL_EXIST_FLAG") }).FirstOrDefault();

                        if (queryCoaterInType != null)
                        {
                            //txtCoaterIn.Text = queryCoaterInType.CoaterInType + " : " + queryCoaterInType.CoaterCurrInoutType;
                            //txtCoaterIn.Text = queryCoaterInType.CoaterCurrInoutType + " : " + queryCoaterInType.CoaterInType;
                            //txtCoaterIn.FontWeight = FontWeights.Bold;
                            //if (queryCoaterInType.MaterialExistFlag == "Y") txtCoaterIn.Background = new SolidColorBrush(Colors.LightPink);
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

        private void SelectRackFromJomboRollStChecked(string rackId)
        {
            DataTable dt = ((DataView)dgInputRank.ItemsSource).Table;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["RACK_ID"].ToString() == rackId)
                {

                    C1.WPF.DataGrid.DataGridCell gridCell = dgInputRank.GetCell(i, dgInputRank.Columns["CHK"].Index) as C1.WPF.DataGrid.DataGridCell;

                    if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                    {
                        CheckBox cb = gridCell.Presenter.Content as CheckBox;

                        cb.IsChecked = true;
                    }
                }
            }
        }

        private void SelectRackFromJomboRollStUnChecked(string rackId)
        {
            DataTable dt = ((DataView)dgInputRank.ItemsSource).Table;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["RACK_ID"].ToString() == rackId)
                {

                    C1.WPF.DataGrid.DataGridCell gridCell = dgRackInfo.GetCell(i, dgInputRank.Columns["CHK"].Index) as C1.WPF.DataGrid.DataGridCell;

                    if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
                    {
                        CheckBox cb = gridCell.Presenter.Content as CheckBox;

                        cb.IsChecked = false;
                    }
                }
            }
        }
        private void SelectMaxRankxyz()
        {
            try
            {
                if (cboStocker?.SelectedValue == null) return;
                if (eqgrCboStockerSelectedValue == "JRB")      {//점보롤 버퍼일 때
                    const string bizRuleName = "DA_SEL_MMD_LOC_NEW";
                    DataTable inDataTable = new DataTable("RQSTDT");
                    inDataTable.Columns.Add("USE_FLAG", typeof(string));
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                    DataRow dr = inDataTable.NewRow();
                    dr["USE_FLAG"] = "Y";
                    dr["EQPTID"] = cboStocker.SelectedValue;
                    inDataTable.Rows.Add(dr);
                    DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
                    if(dtResult.Rows.Count == 0)
                    {
                        _maxRow = 0;
                        _maxCol = 0;
                        return;
                    }
                    else
                    {
                        _maxRow = 1;
                        _maxCol = dtResult.Rows.Count;
                    }
                }

                else
                {//점보롤 창고 일 때
                    {
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

                        }
                        else
                        {
                            _maxRow = 0;
                            _maxCol = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void VisibleHideControl()
        {// 점보롤 버퍼와 창고일 때 보이는 화면이 다름
            if (eqgrCboStockerSelectedValue == "JRB")
            {//점보롤 버퍼일 때
                RollSltBufTable.Visibility = Visibility.Visible;
                RollSltPortTable.Visibility = Visibility.Visible;
                RolSltbuf.Visibility = Visibility.Visible;
                RolSltPort.Visibility = Visibility.Visible;

                RollPressPortInfo.Visibility = Visibility.Hidden;
                CoaterPortInfo.Visibility = Visibility.Hidden;
                RolSidePort.Visibility = Visibility.Hidden;
                CotSidePort.Visibility = Visibility.Hidden;

                btnManualIssue.Visibility = Visibility.Hidden;//수동 출고 버튼 사용 x

            }
            else
            {//점보롤 창고일 때
                RollSltBufTable.Visibility = Visibility.Hidden;
                RollSltPortTable.Visibility = Visibility.Hidden;
                RolSltbuf.Visibility = Visibility.Hidden;
                RolSltPort.Visibility = Visibility.Hidden;

                RollPressPortInfo.Visibility = Visibility.Visible;
                CoaterPortInfo.Visibility = Visibility.Visible;
                RolSidePort.Visibility = Visibility.Visible;
                CotSidePort.Visibility = Visibility.Visible;

                btnManualIssue.Visibility = Visibility.Visible;//수동 출고 버튼 사용 o
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
            _dtRackInfo.Columns.Add("CHKXX", typeof(bool));
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
            _dtRackInfo.Columns.Add("PROCIDNAME", typeof(string));
            _dtRackInfo.Columns.Add("RACK_STAT_CODE", typeof(string));
            _dtRackInfo.Columns.Add("WOID", typeof(string));
            _dtRackInfo.Columns.Add("SEQ", typeof(int));
            _dtRackInfo.Columns.Add("ELECTRODETYPE", typeof(string));
            _dtRackInfo.Columns.Add("PORTID", typeof(string));
            _dtRackInfo.Columns.Add("HALF_SLIT_SIDE", typeof(string)); //2021.06.01 HALF 슬리팅 면 컬럼 추가 대응
            _dtRackInfo.Columns.Add("QMS_HOLD_FLAG", typeof(string)); //2021.06.28 QMS HOLD 여부 컬럼 추가 대응
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

            //현재 선택 STK의 EQGRID 저장
            eqgrCboStockerSelectedValue = DataTableConverter.Convert(cboStocker.ItemsSource).Rows[cboStocker.SelectedIndex]["EQGRID"].ToString();

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
            eqgrCboStockerSelectedValue = DataTableConverter.Convert(cboStocker.ItemsSource).Rows[cboStocker.SelectedIndex]["EQGRID"].ToString();
            SelectMaxRankxyz();
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
                textBlock1.Text = _maxCol-i + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = _maxCol - i + ObjectDic.Instance.GetObjectName("연");
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
                textBlock1.Text = _maxCol-i + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = _maxCol-i + ObjectDic.Instance.GetObjectName("연");
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
                    ucRackStair1.Checked+= UcRackStair1_Checked;
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
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_JUMBOROLL_CBO", "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;

        }
        private static void SetDestPortCombo(C1ComboBox cbo)
        {
            var x = cbo.Parent;
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

        private void SetGridCboItemDestPort(C1ComboBox combo, string sEQPTID, string sCSTID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("CSTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEQPTID;
                Indata["CSTID"] = sCSTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("BR_GUI_GET_JUMBOROLL_STK_TO_DEST_PORT", "RQSTDT", "RSLTDT", IndataTable);

                combo.ItemsSource = dtResult.DefaultView;
            }
            catch (Exception ex)
            {
                throw;
            }
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
        private void insertManualOrder(List<int> rowIndex)
        {
            try
            {

                DataTable IndataTable = new DataTable();

                IndataTable.Columns.Add("SRC_LOCID", typeof(string));
                IndataTable.Columns.Add("DST_LOCID", typeof(string));
                IndataTable.Columns.Add("CSTID", typeof(string));
                IndataTable.Columns.Add("UPDUSER", typeof(string));

                for (int i = 0; i < rowIndex.Count; i++) {
                    DataRow Indata = IndataTable.NewRow();
                    C1.WPF.DataGrid.DataGridCell gridCell = dgRackInfo.GetCell(i, dgRackInfo.Columns["PORTID"].Index) as C1.WPF.DataGrid.DataGridCell;
                    C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;
                    Indata["SRC_LOCID"] = DataTableConverter.GetValue(dgRackInfo.Rows[rowIndex[i]].DataItem,"RACK_ID");
                    Indata["DST_LOCID"] = combo.SelectedValue;
                    Indata["CSTID"] = DataTableConverter.GetValue(dgRackInfo.Rows[rowIndex[i]].DataItem, "MCS_CST_ID");
                    Indata["UPDUSER"] = LoginInfo.USERID;
                    IndataTable.Rows.Add(Indata);

                }

                DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("BR_GUI_INS__MANUAL_ORD_JUMBOROLL_STK", "RQSTDT", "RSLTDT", IndataTable);
                Util.MessageValidation("SFU2871");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private List<int> getCheckGridRowsIndex()
        {
            List<int> checkRows = new List<int>();
            for (int i = 0; i < dgRackInfo.Rows.Count; i++)
            {
        
                if ((bool)DataTableConverter.GetValue(dgRackInfo.Rows[i].DataItem, "CHKXX"))
                {
                    C1.WPF.DataGrid.DataGridCell gridCell = dgRackInfo.GetCell(i, dgRackInfo.Columns["PORTID"].Index) as C1.WPF.DataGrid.DataGridCell;
                    C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgRackInfo.Rows[i].DataItem, "MCS_CST_ID").ToString()) || string.IsNullOrEmpty(combo.SelectedValue.ToString()))
                    {
                        return null;
                    }
                    checkRows.Add(i);
                }
            }
            return checkRows;
        }
        private bool checkGridExistRack(UcRackStair ucRackStair)
        {
            string rackId = ucRackStair.RackId;
            if(dgRackInfo.Rows.Count == 0) return false;
            DataTable dt = ((DataView)dgRackInfo.ItemsSource).Table;
            DataRow[] dr = dt.Select("RACK_ID = '" + rackId.ToString() + "'");
            if(dr.Length == 0)
            {
                return false;
            }
            else return true;

        }
        private void CancelManualOrder()
        {
            try
            {
                DataTable IndataTable = new DataTable();

                IndataTable.Columns.Add("MCS_CST_ID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("USER", typeof(string));
                for (int i = 0; i < dgOrderInfo.Rows.Count; i++)
                {
                    if ((bool)DataTableConverter.GetValue(dgOrderInfo.Rows[i].DataItem, "ORDERCHK"))
                    {
                        DataRow Indata = IndataTable.NewRow();
                        Indata["MCS_CST_ID"] = DataTableConverter.GetValue(dgOrderInfo.Rows[i].DataItem, "MCS_CST_ID");
                        Indata["USER"] = LoginInfo.USERID;
                        Indata["EQPTID"] = cboStocker.SelectedValue.ToString();
                        IndataTable.Rows.Add(Indata);
                    }
                }
                if(IndataTable.Rows.Count != 0)
                {
                    DataTable dtResult = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync("BR_GUI_CANCEL_MANUAL_ORDER", "IN_DATA", null, IndataTable);
                    Util.MessageValidation("SFU1736");
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SelectDestPortInfo()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_GUI_GET_JUMBOROLL_STK_TO_DEST_PORT_INFO";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inDataTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    SettDestPortInfo(bizResult);
                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SettDestPortInfo(DataTable bizResult)
        {
            DataTable RollPortTable = new DataTable();
            RollPortTable.Columns.Add("LOCID", typeof(string));
            RollPortTable.Columns.Add("AVAIL_FLAG", typeof(string));
            RollPortTable.Columns.Add("LOC_NAME", typeof(string));
            RollPortTable.Columns.Add("MCS_CST_ID", typeof(string));
            RollPortTable.Columns.Add("ACCESS_MODE_CODE", typeof(string));
            RollPortTable.Columns.Add("LOC_DETL_TP", typeof(string));
            RollPortTable.Columns.Add("TRF_STAT_CODE", typeof(string));

            DataTable CotPortTable = new DataTable();
            CotPortTable.Columns.Add("LOCID", typeof(string));
            CotPortTable.Columns.Add("AVAIL_FLAG", typeof(string));
            CotPortTable.Columns.Add("LOC_NAME", typeof(string));
            CotPortTable.Columns.Add("MCS_CST_ID", typeof(string));
            CotPortTable.Columns.Add("ACCESS_MODE_CODE", typeof(string));
            CotPortTable.Columns.Add("LOC_DETL_TP", typeof(string));
            CotPortTable.Columns.Add("TRF_STAT_CODE", typeof(string));

            DataTable RollSltBufTable = new DataTable();
            RollSltBufTable.Columns.Add("LOCID", typeof(string));
            RollSltBufTable.Columns.Add("AVAIL_FLAG", typeof(string));
            RollSltBufTable.Columns.Add("LOC_NAME", typeof(string));
            RollSltBufTable.Columns.Add("MCS_CST_ID", typeof(string));
            RollSltBufTable.Columns.Add("ACCESS_MODE_CODE", typeof(string));
            RollSltBufTable.Columns.Add("LOC_DETL_TP", typeof(string));
            RollSltBufTable.Columns.Add("TRF_STAT_CODE", typeof(string));

            DataTable RollSltPortTable = new DataTable();
            RollSltPortTable.Columns.Add("LOCID", typeof(string));
            RollSltPortTable.Columns.Add("AVAIL_FLAG", typeof(string));
            RollSltPortTable.Columns.Add("LOC_NAME", typeof(string));
            RollSltPortTable.Columns.Add("MCS_CST_ID", typeof(string));
            RollSltPortTable.Columns.Add("ACCESS_MODE_CODE", typeof(string));
            RollSltPortTable.Columns.Add("LOC_DETL_TP", typeof(string));
            RollSltPortTable.Columns.Add("TRF_STAT_CODE", typeof(string));

            if (eqgrCboStockerSelectedValue == "JRB")
            { //점보롤 버퍼일 때
                for (int i = 0; i < bizResult.Rows.Count; i++)
                {
                    if (bizResult.Rows[i]["SIDE_TP"].ToString() == "SLT_BUF")
                    {

                        DataRow dr = RollSltBufTable.NewRow();
                        dr["LOCID"] = bizResult.Rows[i]["LOCID"];
                        dr["LOC_NAME"] = bizResult.Rows[i]["LOC_NAME"];
                        dr["MCS_CST_ID"] = bizResult.Rows[i]["MCS_CST_ID"];
                        dr["ACCESS_MODE_CODE"] = bizResult.Rows[i]["ACCESS_MODE_CODE"];
                        dr["LOC_DETL_TP"] = bizResult.Rows[i]["LOC_TRF_TP"];
                        dr["TRF_STAT_CODE"] = bizResult.Rows[i]["TRF_STAT_CODE"];
                        RollSltBufTable.Rows.Add(dr);
                    }
                    else if(bizResult.Rows[i]["SIDE_TP"].ToString() == "ROL_OUT" || bizResult.Rows[i]["SIDE_TP"].ToString() == "SLT_IN")
                    {
                        DataRow dr = RollSltPortTable.NewRow();
                        dr["LOCID"] = bizResult.Rows[i]["LOCID"];
                        dr["LOC_NAME"] = bizResult.Rows[i]["LOC_NAME"];
                        dr["MCS_CST_ID"] = bizResult.Rows[i]["MCS_CST_ID"];
                        dr["ACCESS_MODE_CODE"] = bizResult.Rows[i]["ACCESS_MODE_CODE"];
                        dr["LOC_DETL_TP"] = bizResult.Rows[i]["LOC_TRF_TP"];
                        dr["TRF_STAT_CODE"] = bizResult.Rows[i]["TRF_STAT_CODE"];
                        RollSltPortTable.Rows.Add(dr);
                    }
                }
                
                Util.GridSetData(dgRollSltBufInfo, RollSltBufTable, null, true);
                Util.GridSetData(dgRollSltPortInfo, RollSltPortTable, null, true);
            }
            else
            {
                for (int i = 0; i < bizResult.Rows.Count; i++)
                {
                    if (bizResult.Rows[i]["SIDE_TP"].ToString() == "ROL_IN")
                    {

                        DataRow dr = RollPortTable.NewRow();
                        dr["LOCID"] = bizResult.Rows[i]["LOCID"];
                        dr["LOC_NAME"] = bizResult.Rows[i]["LOC_NAME"];
                        dr["MCS_CST_ID"] = bizResult.Rows[i]["MCS_CST_ID"];
                        dr["ACCESS_MODE_CODE"] = bizResult.Rows[i]["ACCESS_MODE_CODE"];
                        dr["LOC_DETL_TP"] = bizResult.Rows[i]["LOC_TRF_TP"];
                        dr["TRF_STAT_CODE"] = bizResult.Rows[i]["TRF_STAT_CODE"];
                        RollPortTable.Rows.Add(dr);
                    }
                    else if (bizResult.Rows[i]["SIDE_TP"].ToString() == "COT")
                    {


                        DataRow dr = CotPortTable.NewRow();
                        dr["LOCID"] = bizResult.Rows[i]["LOCID"];
                        dr["LOC_NAME"] = bizResult.Rows[i]["LOC_NAME"];
                        dr["MCS_CST_ID"] = bizResult.Rows[i]["MCS_CST_ID"];
                        dr["ACCESS_MODE_CODE"] = bizResult.Rows[i]["ACCESS_MODE_CODE"];
                        dr["LOC_DETL_TP"] = bizResult.Rows[i]["LOC_TRF_TP"];
                        dr["TRF_STAT_CODE"] = bizResult.Rows[i]["TRF_STAT_CODE"];
                        CotPortTable.Rows.Add(dr);
                    }
                }
                Util.GridSetData(dgCoaterSidePort, CotPortTable, null, true);
                Util.GridSetData(dgRollPressSidePort, RollPortTable, null, true);
            }

        }
        #endregion

    }
}
