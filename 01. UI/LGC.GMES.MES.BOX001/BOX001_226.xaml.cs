/*************************************************************************************
 Created Date : 2023.08.10
      Creator : 
   Decription : 전극 자동창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
   2023.08.10  DEVELOPER : Initial Created.
   2023.10.11  정재홍    : [E20230925-000281]- N7 T-BOX automatic warehouse UI improvement

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

//using LGC.GMES.MES.MCS001;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_226.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_226 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private UcPancakeRackStair[][] _rackStairs1;
        private UcPancakeRackStair[][] _rackStairs2;
        private UcPancakeRackStair[][] _rackStairs3;
        private UcPancakeRackStair[][] _rackStairs4;
        private UcPancakeRackStair[][] _rackStairs5;
        private UcPancakeRackStair[][] _rackStairs6;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private readonly Util _util = new Util();

        private bool _isSetAutoSelectTime = false;
        private bool _isFirstLoading = true;
        private DataTable _dtRackInfo;
        private int _maxRowCount;
        private int _maxColumnCount;
        //private string _selectedRackId;

        private string _scboStockerList;    // cboStocker 선택 리스트

        private string _strRank_eltr;       // 극성
        private string _strRank_mkt;        // 시장 유형
        #endregion

        public BOX001_226()
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
            List<Button> listAuth = new List<Button> { btnSearch, btnManualIssue, btnPancakeWarehousingHistory };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (_isFirstLoading)
            {
                InitializeCombo();

                GetcboStockerList();

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
                SelectPancakeRank();
                SelectInoutReservationInfo();
            }

            _isFirstLoading = false;
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
            Loaded -= UserControl_Loaded;

            //NJ.1 STK-1
            if (LoginInfo.CFG_SHOP_ID.GetString().Equals("G183") && LoginInfo.CFG_AREA_ID.GetString().Equals("EH"))
            {
                //N1_SetStairsRowDefinitions();
                NJ1_STK1_Zero();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_monitorTimer.Stop();
        }

        private void btnPancakeWarehousingHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWareHousingHistory()) return;

            object[] parameters = new object[2];
            parameters[0] = string.Empty;
            parameters[1] = string.Empty;
            FrameOperation.OpenMenu("SFU110000102", true, parameters);//실전
            //FrameOperation.OpenMenu("", true, parameters); //운영
        }

        private void dgProjectStock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (dgProjectStock.SelectedItem == null) return;

        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            // 축소
            //ContentsRow.ColumnDefinitions[0].Width = new GridLength(2.4, GridUnitType.Star);
            //ContentsRow.ColumnDefinitions[2].Width = new GridLength(7.6, GridUnitType.Star);
            ContentsRow.ColumnDefinitions[0].Width = new GridLength(3.5, GridUnitType.Star);
            ContentsRow.ColumnDefinitions[2].Width = new GridLength(6.5, GridUnitType.Star);
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
                        //자동조회  %1초로 변경 되었습니다.
                        if (cboAutoSearch != null)
                            Util.MessageInfo("SFU5127", cboAutoSearch.SelectedValue.GetString());
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

            if (cb?.DataContext == null)
                return;

            if (cb.IsChecked != null)
            {
                ShowLoadingIndicator();
                //DoEvents();

                //DataRow dtRow = (cb.DataContext as DataRowView)?.Row;

                //int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;

                //for (int i = 0; i < ((DataGridCellPresenter)cb.Parent).DataGrid.Rows.Count; i++)
                //{
                //    DataTableConverter.SetValue(((DataGridCellPresenter)cb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                //}

                DataRowView drv = cb.DataContext as DataRowView;

                if (drv != null)
                {

                    //if (!isRackBoxIDRsltChk(drv["RACK_ID"].GetString()))
                    //{
                    //    Util.MessageValidation("SFU3316");  // 작업오류 : 선택한 BOX는 이미 포장해제된 BOX입니다.[BOX 정보 확인]
                    //    HiddenLoadingIndicator();
                    //    cb.IsChecked = false;
                    //    return;
                    //}

                    _strRank_eltr = drv["ELTR_TYPE_CODE"].GetString();
                    _strRank_mkt = drv["MKT_TYPE_NAME"].GetString();

                    SelectPancakeFromRankChecked(drv["RACK_ID"].GetString());
                }

                HiddenLoadingIndicator();
            }
        }

        private void rankCheck_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb?.DataContext == null)
                return;

            if (cb.IsChecked != null)
            {
                ShowLoadingIndicator();
                //DoEvents();

                DataRow dtRow = (cb.DataContext as DataRowView)?.Row;

                if (dtRow != null)
                {
                    _strRank_eltr = string.Empty;
                    _strRank_mkt = string.Empty;

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
            Search();

            //NJ.1 STK-1
            if (LoginInfo.CFG_SHOP_ID.GetString().Equals("G183") && LoginInfo.CFG_AREA_ID.GetString().Equals("EH"))
            {
                //N1_SetStairsRowDefinitions();
                NJ1_STK1_Zero();
            }
        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue())
                return;

            DataTable dtRack = ((DataView)dgRackInfo.ItemsSource).Table;
            DataTable dtRack_FIFO = dtRack.DefaultView.ToTable(true, new string[] { "MCS_CST_ID", "PRODID", "VLD_DATE" });

            DataSet indataSet = new DataSet();
            indataSet.Tables.Add("INDATA");

            indataSet.Tables["INDATA"].Columns.Add("BOXID", typeof(string));
            indataSet.Tables["INDATA"].Columns.Add("PRODID", typeof(string));
            indataSet.Tables["INDATA"].Columns.Add("EQSGID", typeof(string));
            indataSet.Tables["INDATA"].Columns.Add("EQPTID", typeof(string));
            indataSet.Tables["INDATA"].Columns.Add("VLD_DATE", typeof(string));

            foreach (DataRow dr in dtRack_FIFO.Rows)
            {
                DataRow row = indataSet.Tables["INDATA"].NewRow();
                row["BOXID"] = dr["MCS_CST_ID"].ToString();
                row["PRODID"] = dr["PRODID"].ToString();
                row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                row["EQPTID"] = _scboStockerList;
                row["VLD_DATE"] = dr["VLD_DATE"].ToString();

                indataSet.Tables["INDATA"].Rows.Add(row);
            }

            new ClientProxy().ExecuteService_Multi("BR_PRD_CHECK_TBOX_STK_FIFO_NJ", "INDATA", "OUTDATA,OUT_FIFO_CHECK", (bizResult, bizException) =>
            {
                HiddenLoadingIndicator();

                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtResult = bizResult.Tables["OUTDATA"];
                    DataTable dtChk = bizResult.Tables["OUT_FIFO_CHECK"];

                    for (int i = 0; i < dtChk.Rows.Count; i++)
                    {
                        foreach (DataRow r in dtResult.Rows)
                        {
                            string stmp = dtChk.Rows[i]["TBOXID"].ToString();

                            if (stmp.Contains(r["TBOXID"].ToString()))
                            {
                                dtResult.Rows.Remove(r);
                                break;
                            }
                        }
                    }

                    if (dtResult.Rows.Count > 0)
                    {
                        Util.MessageValidation("90069");  // 선입선출 위반입니다.
                        return;
                    }

                    ManualIssue();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }, indataSet);
        }

        private void dgProjectStock_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectMaxRankxyz();
            Search();
        }

        private void popupRackInfo_Closed(object sender, EventArgs e)
        {
            BOX001_226_RACK_INFO popup = sender as BOX001_226_RACK_INFO;

            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectPancakeMonitoring();
            }
        }

        private void UcPancakeRackStair1_Click(object sender, RoutedEventArgs e)
        {
            UcRackStair rackStair = sender as UcRackStair;

            if (rackStair != null)
            {
                ControlsLibrary.MessageBox.Show(rackStair.Name);
            }
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
                dr["EQPTID"] = ucPancakeRackStair.EquipmentCode;
                dtRackInfo.Rows.Add(dr);

                RackInfo(dtRackInfo);
            }
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
                dr["EQPTID"] = ucPancakeRackStair.EquipmentCode;

                dtRackInfo.Rows.Add(dr);
                RackInfo(dtRackInfo);
            }
        }

        private void UcPancakeRackStair3_DoubleClick(object sender, RoutedEventArgs e)
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
                dr["EQPTID"] = ucPancakeRackStair.EquipmentCode;

                dtRackInfo.Rows.Add(dr);
                RackInfo(dtRackInfo);
            }
        }

        private void UcPancakeRackStair4_DoubleClick(object sender, RoutedEventArgs e)
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
                dr["EQPTID"] = ucPancakeRackStair.EquipmentCode;

                dtRackInfo.Rows.Add(dr);
                RackInfo(dtRackInfo);
            }
        }

        private void UcPancakeRackStair5_DoubleClick(object sender, RoutedEventArgs e)
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
                dr["EQPTID"] = ucPancakeRackStair.EquipmentCode;

                dtRackInfo.Rows.Add(dr);
                RackInfo(dtRackInfo);
            }
        }

        private void UcPancakeRackStair6_DoubleClick(object sender, RoutedEventArgs e)
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
                dr["EQPTID"] = ucPancakeRackStair.EquipmentCode;

                dtRackInfo.Rows.Add(dr);
                RackInfo(dtRackInfo);
            }
        }

        private void UcPancakeRackStair2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcPancakeRackStair3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcPancakeRackStair4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcPancakeRackStair5_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UcPancakeRackStair6_Click(object sender, RoutedEventArgs e)
        {

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
            if (!ValidationInoutReservationInfo())
                return;

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
            dr["EQPTID"] = ucPancakeRackStair.EquipmentCode;
            dr["ELTR_TYPE_CODE"] = _strRank_eltr;
            dr["MKT_TYPE_NAME"] = _strRank_mkt;

            _dtRackInfo.Rows.Add(dr);

            Util.GridSetData(dgRackInfo, _dtRackInfo, null, false);
        }

        private void UnCheckUcPancakeRackStair(UcPancakeRackStair ucPancakeRackStair)
        {
            DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID='" + ucPancakeRackStair.RackId + "'");
            foreach (DataRow row in selectedRow)
            {
                _dtRackInfo.Rows.Remove(row);
            }
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, false);
        }

        private void SelectAllPancakeMonitoringControl()
        {
            try
            {
                _monitorTimer.Stop();
                InitializeRackUserControl();
                SelectPancakeMonitoring();
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

                //const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_MONITORING";
                const string bizRuleName = "DA_MCS_SEL_STK_PANCAKE_MONITORING_MULTI";
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
                //dr["EQPTID"] = cboStocker.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue ?? _scboStockerList;
                dr["Z_PSTN"] = null;
                dr["PROCID"] = null;
                dr["MODLID"] = null;
                dr["LOTID"] = null;
                dr["PRJT_NAME"] = null;
                dr["PROD_VER_CODE"] = null;
                dr["WIPHOLD"] = null;
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

                        //UcPancakeRackStair ucPancakeRackStair = item["X_PSTN"].ToString() == "1" ? _rackStairs1[x][y] : item["X_PSTN"].ToString() == "2" ? _rackStairs2[x][y] : _rackStairs3[x][y];

                        UcPancakeRackStair ucPancakeRackStair = _rackStairs1[x][y];

                        switch (item["X_PSTN"].ToString())
                        {
                            case "1":
                                ucPancakeRackStair = _rackStairs1[x][y];
                                break;
                            case "2":
                                ucPancakeRackStair = _rackStairs2[x][y];
                                break;
                            case "3":
                                ucPancakeRackStair = _rackStairs3[x][y];
                                break;
                            case "4":
                                ucPancakeRackStair = _rackStairs4[x][y];
                                break;
                            case "5":
                                ucPancakeRackStair = _rackStairs5[x][y];
                                break;
                            case "6":
                                ucPancakeRackStair = _rackStairs6[x][y];
                                break;
                            default:
                                break;
                        }

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
                        //ucPancakeRackStair.QMS = item["QMS"].GetString(); //Q_F 표시X

                        ucPancakeRackStair.Position = item["X_PSTN"].GetString() + "-" + item["Y_PSTN"].GetString() + "-" + item["Z_PSTN"].GetString();

                        ucPancakeRackStair.Visibility = Visibility.Visible;

                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");

                        if (string.Equals(item["RACK_STAT_CODE"].GetString(), "UNUSE")) //입고금지
                        {
                            convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFD3D3D3");
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else if (string.Equals(item["RACK_STAT_CODE"].GetString(), "USABLE")) //사용가능
                        {
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else if (string.Equals(item["RACK_STAT_CODE"].GetString(), "USING")) //사용중
                        {
                            convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFAFAD2");
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else if (string.Equals(item["RACK_STAT_CODE"].GetString(), "CHECK")) //상태확인
                        {
                            convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#9C9C9C");
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else if (string.Equals(item["RACK_STAT_CODE"].GetString(), "RCV_RESERVE")) //입고예약
                        {
                            convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF00");
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush((Color)convertFromString);
                        }
                        else if (string.Equals(item["RACK_STAT_CODE"].GetString(), "ISS_RESERVE")) //출고예약
                        {
                            convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFFA500");
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush((Color)convertFromString);
                        }

                        if (string.Equals(item["CSTSTAT"].GetString(), "E") && string.Equals(item["RACK_STAT_CODE"].GetString(), "USING"))
                        {
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightSkyBlue);
                        }

                        if (string.Equals(item["BOXID_FLAG"].GetString(), "Y") && string.Equals(item["RACK_STAT_CODE"].GetString(), "USING"))
                        {
                            ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.Gray);
                        }

                        //else if (string.Equals(item["RACK_STAT_CODE"].GetString(), "PORT")) //포트
                        //    ucPancakeRackStair.RootLayout.Background = new SolidColorBrush(Colors.LightSkyBlue);


                        ucPancakeRackStair.IsCheckEnabled = false;
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

        private void SelectPancakeRank()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_TBOX_STK_RANK";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("MCS_CST_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                //dr["EQPTID"] = cboStocker.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue ?? _scboStockerList;
                dr["MCS_CST_ID"] = string.IsNullOrEmpty(txtTBoxId.Text) ? null : txtTBoxId.Text;
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
                    //Util.GridSetData(dgInputRank, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                    Util.GridSetData(dgInputRank, Util.CheckBoxColumnAddTable(bizResult, false), FrameOperation, true);
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

                const string bizRuleName = "DA_MCS_SEL_TBOX_STK_RSV_NJ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("INOUT_TYPE", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["EQPTID"] = cboStocker.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue ?? _scboStockerList;
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

        private void SelectPancakeFromRankChecked(string rackId)
        {
            //DoubleAnimation doubleAnimation = new DoubleAnimation();

            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs1[rowIndex][colIndex];

                    //doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    //doubleAnimation.To = 0;
                    //doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    //doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer1, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            //ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
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

                    //doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    //doubleAnimation.To = 0;
                    //doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    //doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer2, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            //ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair3.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair3.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs3[rowIndex][colIndex];

                    //doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    //doubleAnimation.To = 0;
                    //doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    //doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer3, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            //ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair4.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair4.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs4[rowIndex][colIndex];

                    //doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    //doubleAnimation.To = 0;
                    //doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    //doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer4, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            //ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair5.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair5.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs5[rowIndex][colIndex];

                    //doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    //doubleAnimation.To = 0;
                    //doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    //doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer5, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            //ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
                        }
                        return;
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair6.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair6.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs6[rowIndex][colIndex];

                    //doubleAnimation.From = ucPancakeRackStair.ActualHeight;
                    //doubleAnimation.To = 0;
                    //doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                    //doubleAnimation.AutoReverse = true;

                    if (string.IsNullOrEmpty(ucPancakeRackStair?.RackId)) continue;

                    if (string.Equals(ucPancakeRackStair.RackId, rackId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!ucPancakeRackStair.IsChecked)
                        {
                            SetScrollToHorizontalOffset(scrollViewer6, colIndex);
                            ucPancakeRackStair.IsChecked = true;
                            CheckUcPancakeRackStair(ucPancakeRackStair);

                            //ucPancakeRackStair.BeginAnimation(HeightProperty, doubleAnimation);
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

            for (int rowIndex = 0; rowIndex < grdRackstair3.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair3.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs3[rowIndex][colIndex];

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

            for (int rowIndex = 0; rowIndex < grdRackstair4.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair4.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs4[rowIndex][colIndex];

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

            for (int rowIndex = 0; rowIndex < grdRackstair5.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair5.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs5[rowIndex][colIndex];

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

            for (int rowIndex = 0; rowIndex < grdRackstair6.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair6.ColumnDefinitions.Count; colIndex++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs6[rowIndex][colIndex];

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
                //if (string.IsNullOrEmpty(cboStocker?.SelectedValue?.GetString())) return;

                const string bizRuleName = "DA_MCS_SEL_RACK_MAX_XYZ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                //dr["EQPTID"] = cboStocker.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue ?? _scboStockerList;
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

        private void ManualIssue()
        {
            BOX001_226_MANUAL_ISSUE popupManualIssue = new BOX001_226_MANUAL_ISSUE { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = cboStocker.SelectedValue;
            parameters[1] = ((DataView)dgRackInfo.ItemsSource).Table;
            C1WindowExtension.SetParameters(popupManualIssue, parameters);

            popupManualIssue.Closed += popupManualIssue_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupManualIssue.ShowModal()));
        }

        private void popupManualIssue_Closed(object sender, EventArgs e)
        {
            BOX001_226_MANUAL_ISSUE popup = sender as BOX001_226_MANUAL_ISSUE;

            if (popup != null && popup.IsUpdated)
            {
                _dtRackInfo.Clear();
                SelectPancakeMonitoring();
                SelectPancakeRank();
                SelectInoutReservationInfo();
            }
        }

        private void RackInfo(DataTable dt)
        {
            BOX001_226_RACK_INFO popupRackInfo = new BOX001_226_RACK_INFO { FrameOperation = FrameOperation };
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
            _dtRackInfo.Columns.Add("EQPTID", typeof(string));
            _dtRackInfo.Columns.Add("ELTR_TYPE_CODE", typeof(string));  // 극성
            _dtRackInfo.Columns.Add("MKT_TYPE_NAME", typeof(string));   // 시장 유형
        }

        private void GetcboStockerList()
        {
            string comboBoxItemsText = string.Empty;

            foreach (object item in cboStocker.Items)
            {
                if (!string.IsNullOrEmpty(((DataRowView)item).Row.ItemArray[0].ToString()))
                {
                    comboBoxItemsText += ((DataRowView)item).Row.ItemArray[0] + ",";
                }
            }

            _scboStockerList = comboBoxItemsText;
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
            //SetHoldCombo(cboHold);

            // QMS 콤보박스
            SetQMScombo(cboQMS);

            //범례공통코드
            //CommonCombo _combo = new CommonCombo();
            //String[] sFilter1 = { "CSTPROD", "SKID" };
            //_combo.SetCombo(cboCmd, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTR_MCS");
        }

        private void ReSetElectrodPancakeControl()
        {
            _dtRackInfo.Clear();
            MakeColumnDefinition();
            PrepareRackStair();
            PrepareRackStairLayout();
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

            for (int row = 0; row < grdRackstair3.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair3.ColumnDefinitions.Count; col++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs3[row][col];
                    ucPancakeRackStair.IsEnabled = true;
                    ucPancakeRackStair.SetRackBackcolor();
                }
            }

            for (int row = 0; row < grdRackstair4.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair4.ColumnDefinitions.Count; col++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs4[row][col];
                    ucPancakeRackStair.IsEnabled = true;
                    ucPancakeRackStair.SetRackBackcolor();
                }
            }

            for (int row = 0; row < grdRackstair5.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair5.ColumnDefinitions.Count; col++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs5[row][col];
                    ucPancakeRackStair.IsEnabled = true;
                    ucPancakeRackStair.SetRackBackcolor();
                }
            }

            for (int row = 0; row < grdRackstair6.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair6.ColumnDefinitions.Count; col++)
                {
                    UcPancakeRackStair ucPancakeRackStair = _rackStairs6[row][col];
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

            for (int row = 0; row < grdRackstair3.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair3.ColumnDefinitions.Count; col++)
                {
                    _rackStairs3[row][col].Visibility = Visibility.Hidden;
                    _rackStairs3[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair4.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair4.ColumnDefinitions.Count; col++)
                {
                    _rackStairs4[row][col].Visibility = Visibility.Hidden;
                    _rackStairs4[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair5.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair5.ColumnDefinitions.Count; col++)
                {
                    _rackStairs5[row][col].Visibility = Visibility.Hidden;
                    _rackStairs5[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair6.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair6.ColumnDefinitions.Count; col++)
                {
                    _rackStairs6[row][col].Visibility = Visibility.Hidden;
                    _rackStairs6[row][col].Clear();
                }
            }
        }

        private void SetColumnDefinition(int maxcolIndex)
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();
            grdStair3.Children.Clear();
            grdStair4.Children.Clear();
            grdStair5.Children.Clear();
            grdStair6.Children.Clear();

            if (grdStair1.ColumnDefinitions.Count > 0) grdStair1.ColumnDefinitions.Clear();
            if (grdStair1.RowDefinitions.Count > 0) grdStair1.RowDefinitions.Clear();

            if (grdStair2.ColumnDefinitions.Count > 0) grdStair2.ColumnDefinitions.Clear();
            if (grdStair2.RowDefinitions.Count > 0) grdStair2.RowDefinitions.Clear();

            if (grdStair3.ColumnDefinitions.Count > 0) grdStair3.ColumnDefinitions.Clear();
            if (grdStair3.RowDefinitions.Count > 0) grdStair3.RowDefinitions.Clear();

            if (grdStair4.ColumnDefinitions.Count > 0) grdStair4.ColumnDefinitions.Clear();
            if (grdStair4.RowDefinitions.Count > 0) grdStair4.RowDefinitions.Clear();

            if (grdStair5.ColumnDefinitions.Count > 0) grdStair5.ColumnDefinitions.Clear();
            if (grdStair5.RowDefinitions.Count > 0) grdStair5.RowDefinitions.Clear();

            if (grdStair6.ColumnDefinitions.Count > 0) grdStair6.ColumnDefinitions.Clear();
            if (grdStair6.RowDefinitions.Count > 0) grdStair6.RowDefinitions.Clear();

            int colIndex = 0;

            for (int i = 0; i < maxcolIndex; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition5 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition6 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);
                grdStair3.ColumnDefinitions.Add(columnDefinition3);
                grdStair4.ColumnDefinitions.Add(columnDefinition4);
                grdStair5.ColumnDefinitions.Add(columnDefinition5);
                grdStair6.ColumnDefinitions.Add(columnDefinition6);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock3 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock4 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock5 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock6 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                string colName = i + 1 + ObjectDic.Instance.GetObjectName("연");

                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = colName;
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = colName;
                Grid.SetColumn(textBlock2, colIndex);
                grdStair2.Children.Add(textBlock2);

                textBlock3.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock3.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock3.Text = colName;
                Grid.SetColumn(textBlock3, colIndex);
                grdStair3.Children.Add(textBlock3);

                textBlock4.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock4.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock4.Text = colName;
                Grid.SetColumn(textBlock4, colIndex);
                grdStair4.Children.Add(textBlock4);

                textBlock5.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock5.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock5.Text = colName;
                Grid.SetColumn(textBlock5, colIndex);
                grdStair5.Children.Add(textBlock5);

                textBlock6.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock6.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock6.Text = colName;
                Grid.SetColumn(textBlock6, colIndex);
                grdStair6.Children.Add(textBlock6);

                colIndex++;
            }
        }

        private void MakeColumnDefinition()
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();
            grdStair3.Children.Clear();
            grdStair4.Children.Clear();
            grdStair5.Children.Clear();
            grdStair6.Children.Clear();

            int colIndex = 0;

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition5 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition6 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);
                grdStair3.ColumnDefinitions.Add(columnDefinition3);
                grdStair4.ColumnDefinitions.Add(columnDefinition4);
                grdStair5.ColumnDefinitions.Add(columnDefinition5);
                grdStair6.ColumnDefinitions.Add(columnDefinition6);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock3 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock4 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock5 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock6 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                string colName = i + 1 + ObjectDic.Instance.GetObjectName("연");

                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = colName;
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = colName;
                Grid.SetColumn(textBlock2, colIndex);
                grdStair2.Children.Add(textBlock2);

                textBlock3.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock3.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock3.Text = colName;
                Grid.SetColumn(textBlock3, colIndex);
                grdStair3.Children.Add(textBlock3);

                textBlock4.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock4.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock4.Text = colName;
                Grid.SetColumn(textBlock4, colIndex);
                grdStair4.Children.Add(textBlock4);

                textBlock5.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock5.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock5.Text = colName;
                Grid.SetColumn(textBlock5, colIndex);
                grdStair5.Children.Add(textBlock5);

                textBlock6.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock6.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock6.Text = colName;
                Grid.SetColumn(textBlock6, colIndex);
                grdStair6.Children.Add(textBlock6);

                colIndex++;
            }
        }

        private void MakeRowDefinition()
        {
            // 단 Row 생성
            ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition5 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition6 = new ColumnDefinition { Width = new GridLength(60) };

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);
            grdColumn3.ColumnDefinitions.Add(columnDefinition3);
            grdColumn4.ColumnDefinitions.Add(columnDefinition4);
            grdColumn5.ColumnDefinitions.Add(columnDefinition5);
            grdColumn6.ColumnDefinitions.Add(columnDefinition6);

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                grdColumn1.RowDefinitions.Add(rowDefinition1);

                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdColumn2.RowDefinitions.Add(rowDefinition2);

                RowDefinition rowDefinition3 = new RowDefinition { Height = new GridLength(60) };
                grdColumn3.RowDefinitions.Add(rowDefinition3);

                RowDefinition rowDefinition4 = new RowDefinition { Height = new GridLength(60) };
                grdColumn4.RowDefinitions.Add(rowDefinition4);

                RowDefinition rowDefinition5 = new RowDefinition { Height = new GridLength(60) };
                grdColumn5.RowDefinitions.Add(rowDefinition5);

                RowDefinition rowDefinition6 = new RowDefinition { Height = new GridLength(60) };
                grdColumn6.RowDefinitions.Add(rowDefinition6);
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
                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter() });
                textBlock2.SetValue(Grid.RowProperty, i);
                textBlock2.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
                grdColumn2.Children.Add(textBlock2);

                TextBlock textBlock3 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock3.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter() });
                textBlock3.SetValue(Grid.RowProperty, i);
                textBlock3.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                textBlock3.HorizontalAlignment = HorizontalAlignment.Center;
                grdColumn3.Children.Add(textBlock3);

                TextBlock textBlock4 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock4.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter() });
                textBlock4.SetValue(Grid.RowProperty, i);
                textBlock4.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                textBlock4.HorizontalAlignment = HorizontalAlignment.Center;
                grdColumn4.Children.Add(textBlock4);

                TextBlock textBlock5 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock5.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter() });
                textBlock5.SetValue(Grid.RowProperty, i);
                textBlock5.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                textBlock5.HorizontalAlignment = HorizontalAlignment.Center;
                grdColumn5.Children.Add(textBlock5);

                TextBlock textBlock6 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock6.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter() });
                textBlock6.SetValue(Grid.RowProperty, i);
                textBlock6.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                textBlock6.HorizontalAlignment = HorizontalAlignment.Center;
                grdColumn6.Children.Add(textBlock6);
            }
        }

        private void ReSetRackStairLayout(int maxRowIndex, int maxColumnIndex)
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();
            grdRackstair3.Children.Clear();
            grdRackstair4.Children.Clear();
            grdRackstair5.Children.Clear();
            grdRackstair6.Children.Clear();

            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();

            if (grdRackstair3.ColumnDefinitions.Count > 0) grdRackstair3.ColumnDefinitions.Clear();
            if (grdRackstair3.RowDefinitions.Count > 0) grdRackstair3.RowDefinitions.Clear();

            if (grdRackstair4.ColumnDefinitions.Count > 0) grdRackstair4.ColumnDefinitions.Clear();
            if (grdRackstair4.RowDefinitions.Count > 0) grdRackstair4.RowDefinitions.Clear();

            if (grdRackstair5.ColumnDefinitions.Count > 0) grdRackstair5.ColumnDefinitions.Clear();
            if (grdRackstair5.RowDefinitions.Count > 0) grdRackstair5.RowDefinitions.Clear();

            if (grdRackstair6.ColumnDefinitions.Count > 0) grdRackstair6.ColumnDefinitions.Clear();
            if (grdRackstair6.RowDefinitions.Count > 0) grdRackstair6.RowDefinitions.Clear();

            for (int i = 0; i < maxColumnIndex; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition5 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition6 = new ColumnDefinition { Width = new GridLength(60) };

                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
                grdRackstair3.ColumnDefinitions.Add(columnDefinition3);
                grdRackstair4.ColumnDefinitions.Add(columnDefinition4);
                grdRackstair5.ColumnDefinitions.Add(columnDefinition5);
                grdRackstair6.ColumnDefinitions.Add(columnDefinition6);
            }

            for (int i = 0; i < maxRowIndex; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition3 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition4 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition5 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition6 = new RowDefinition { Height = new GridLength(60) };

                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
                grdRackstair3.RowDefinitions.Add(rowDefinition3);
                grdRackstair4.RowDefinitions.Add(rowDefinition4);
                grdRackstair5.RowDefinitions.Add(rowDefinition5);
                grdRackstair6.RowDefinitions.Add(rowDefinition6);
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

                    Grid.SetRow(_rackStairs3[row][col], row);
                    Grid.SetColumn(_rackStairs3[row][col], col);
                    grdRackstair3.Children.Add(_rackStairs3[row][col]);

                    Grid.SetRow(_rackStairs4[row][col], row);
                    Grid.SetColumn(_rackStairs4[row][col], col);
                    grdRackstair4.Children.Add(_rackStairs4[row][col]);

                    Grid.SetRow(_rackStairs5[row][col], row);
                    Grid.SetColumn(_rackStairs5[row][col], col);
                    grdRackstair5.Children.Add(_rackStairs5[row][col]);

                    Grid.SetRow(_rackStairs6[row][col], row);
                    Grid.SetColumn(_rackStairs6[row][col], col);
                    grdRackstair6.Children.Add(_rackStairs6[row][col]);
                }
            }
        }

        private void PrepareRackStair()
        {
            _rackStairs1 = new UcPancakeRackStair[_maxRowCount][];
            _rackStairs2 = new UcPancakeRackStair[_maxRowCount][];
            _rackStairs3 = new UcPancakeRackStair[_maxRowCount][];
            _rackStairs4 = new UcPancakeRackStair[_maxRowCount][];
            _rackStairs5 = new UcPancakeRackStair[_maxRowCount][];
            _rackStairs6 = new UcPancakeRackStair[_maxRowCount][];

            for (int r = 0; r < _rackStairs1.Length; r++)
            {
                _rackStairs1[r] = new UcPancakeRackStair[_maxColumnCount];
                _rackStairs2[r] = new UcPancakeRackStair[_maxColumnCount];
                _rackStairs3[r] = new UcPancakeRackStair[_maxColumnCount];
                _rackStairs4[r] = new UcPancakeRackStair[_maxColumnCount];
                _rackStairs5[r] = new UcPancakeRackStair[_maxColumnCount];
                _rackStairs6[r] = new UcPancakeRackStair[_maxColumnCount];

                for (int c = 0; c < _rackStairs1[r].Length; c++)
                {
                    UcPancakeRackStair ucPancakeRackStair1 = new UcPancakeRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    //ucPancakeRackStair1.Checked += UcPancakeRackStair1_Checked;
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

                    //ucPancakeRackStair2.Checked += UcPancakeRackStair2_Checked;
                    ucPancakeRackStair2.Click += UcPancakeRackStair2_Click;
                    ucPancakeRackStair2.DoubleClick += UcPancakeRackStair2_DoubleClick;
                    _rackStairs2[r][c] = ucPancakeRackStair2;
                }

                for (int c = 0; c < _rackStairs3[r].Length; c++)
                {
                    UcPancakeRackStair ucPancakeRackStair3 = new UcPancakeRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    //ucPancakeRackStair3.Checked += UcPancakeRackStair3_Checked;
                    ucPancakeRackStair3.Click += UcPancakeRackStair3_Click;
                    ucPancakeRackStair3.DoubleClick += UcPancakeRackStair3_DoubleClick;
                    _rackStairs3[r][c] = ucPancakeRackStair3;
                }

                for (int c = 0; c < _rackStairs4[r].Length; c++)
                {
                    UcPancakeRackStair ucPancakeRackStair4 = new UcPancakeRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    //ucPancakeRackStair4.Checked += UcPancakeRackStair4_Checked;
                    ucPancakeRackStair4.Click += UcPancakeRackStair4_Click;
                    ucPancakeRackStair4.DoubleClick += UcPancakeRackStair4_DoubleClick;
                    _rackStairs4[r][c] = ucPancakeRackStair4;
                }

                for (int c = 0; c < _rackStairs5[r].Length; c++)
                {
                    UcPancakeRackStair ucPancakeRackStair5 = new UcPancakeRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    //ucPancakeRackStair5.Checked += UcPancakeRackStair5_Checked;
                    ucPancakeRackStair5.Click += UcPancakeRackStair5_Click;
                    ucPancakeRackStair5.DoubleClick += UcPancakeRackStair5_DoubleClick;
                    _rackStairs5[r][c] = ucPancakeRackStair5;
                }

                for (int c = 0; c < _rackStairs6[r].Length; c++)
                {
                    UcPancakeRackStair ucPancakeRackStair6 = new UcPancakeRackStair
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    //ucPancakeRackStair6.Checked += UcPancakeRackStair6_Checked;
                    ucPancakeRackStair6.Click += UcPancakeRackStair6_Click;
                    ucPancakeRackStair6.DoubleClick += UcPancakeRackStair6_DoubleClick;
                    _rackStairs6[r][c] = ucPancakeRackStair6;
                }
            }
        }

        private void PrepareRackStairLayout()
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();
            grdRackstair3.Children.Clear();
            grdRackstair4.Children.Clear();
            grdRackstair5.Children.Clear();
            grdRackstair6.Children.Clear();

            //행 / 열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();

            if (grdRackstair3.ColumnDefinitions.Count > 0) grdRackstair3.ColumnDefinitions.Clear();
            if (grdRackstair3.RowDefinitions.Count > 0) grdRackstair3.RowDefinitions.Clear();

            if (grdRackstair4.ColumnDefinitions.Count > 0) grdRackstair4.ColumnDefinitions.Clear();
            if (grdRackstair4.RowDefinitions.Count > 0) grdRackstair4.RowDefinitions.Clear();

            if (grdRackstair5.ColumnDefinitions.Count > 0) grdRackstair5.ColumnDefinitions.Clear();
            if (grdRackstair5.RowDefinitions.Count > 0) grdRackstair5.RowDefinitions.Clear();

            if (grdRackstair6.ColumnDefinitions.Count > 0) grdRackstair6.ColumnDefinitions.Clear();
            if (grdRackstair6.RowDefinitions.Count > 0) grdRackstair6.RowDefinitions.Clear();

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition5 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition6 = new ColumnDefinition { Width = new GridLength(60) };

                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
                grdRackstair3.ColumnDefinitions.Add(columnDefinition3);
                grdRackstair4.ColumnDefinitions.Add(columnDefinition4);
                grdRackstair5.ColumnDefinitions.Add(columnDefinition5);
                grdRackstair6.ColumnDefinitions.Add(columnDefinition6);
            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition3 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition4 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition5 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition6 = new RowDefinition { Height = new GridLength(60) };

                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
                grdRackstair3.RowDefinitions.Add(rowDefinition3);
                grdRackstair4.RowDefinitions.Add(rowDefinition4);
                grdRackstair5.RowDefinitions.Add(rowDefinition5);
                grdRackstair6.RowDefinitions.Add(rowDefinition6);
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

                    Grid.SetRow(_rackStairs3[row][col], row);
                    Grid.SetColumn(_rackStairs3[row][col], col);
                    grdRackstair3.Children.Add(_rackStairs3[row][col]);

                    Grid.SetRow(_rackStairs4[row][col], row);
                    Grid.SetColumn(_rackStairs4[row][col], col);
                    grdRackstair4.Children.Add(_rackStairs4[row][col]);

                    Grid.SetRow(_rackStairs5[row][col], row);
                    Grid.SetColumn(_rackStairs5[row][col], col);
                    grdRackstair5.Children.Add(_rackStairs5[row][col]);

                    Grid.SetRow(_rackStairs6[row][col], row);
                    Grid.SetColumn(_rackStairs6[row][col], col);
                    grdRackstair6.Children.Add(_rackStairs6[row][col]);
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
            if (string.IsNullOrEmpty(txtTBoxId.Text))
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
                Util.MessageValidation("SFU1636");  // 선택된 대상이 없습니다.
                return false;
            }

            var query = (from t in ((DataView)dgRackInfo.ItemsSource).Table.AsEnumerable()
                         where (!t.Field<string>("RACK_STAT_CODE").Equals("USING") && !t.Field<string>("RACK_STAT_CODE").Equals("NOREAD"))
                         select t).ToList();

            if (query.Any())
            {
                Util.MessageValidation("SFU1643");  // 선택된 자재 정보가 없습니다.
                return false;
            }

            DataTable dtRack = ((DataView)dgRackInfo.ItemsSource).Table;
            DataTable dtRack_ELTR = dtRack.DefaultView.ToTable(true, new string[] { "ELTR_TYPE_CODE" });

            if (dtRack_ELTR.Rows.Count > 1)
            {
                Util.MessageValidation("SFU2057");  // 극성 정보가 다릅니다.
                return false;
            }

            DataTable dtRack_MKT = dtRack.DefaultView.ToTable(true, new string[] { "MKT_TYPE_NAME" });

            if (dtRack_MKT.Rows.Count > 1)
            {
                Util.MessageValidation("SFU4271");  // 동일한 시장유형이 아닙니다.
                return false;
            }

            DataTable dtRack_PROD = dtRack.DefaultView.ToTable(true, new string[] { "PRODID" });

            if (dtRack_PROD.Rows.Count > 1)
            {
                Util.MessageValidation("SFU1502");  // 동일 제품이 아닙니다.
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
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            DataRow drColor = inTable.NewRow();
            drColor["LANGID"] = LoginInfo.LANGID;
            drColor["CMCDTYPE"] = "VWW_MNT_LEGEND";

            inTable.Rows.Add(drColor);
            DataTable dtColorResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", inTable);

            foreach (DataRow row in dtColorResult.Rows)
            {
                C1ComboBoxItem cbItem1 = new C1ComboBoxItem
                {
                    Content = row["CBO_NAME"].ToString(),
                    Background = new BrushConverter().ConvertFromString(row["CBO_CODE"].ToString()) as SolidColorBrush
                };
                cboColor.Items.Add(cbItem1);
            }
            cboColor.SelectedIndex = 0;
        }
        private static void SetStockerCombo(C1ComboBox cbo)
        {
            //const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO_NJ";
            //string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "VWW", LoginInfo.CFG_AREA_ID };
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_MACHINE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
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
            scrollViewer.ScrollToHorizontalOffset(Math.Abs(colIndex - _maxColumnCount) * averageScrollWidth);
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
            //BOX001_226_SKID_STATE popupSKIDSearch = new BOX001_226_SKID_STATE { FrameOperation = FrameOperation };
            ////object[] parameters = new object[2];
            ////parameters[0] = cboStocker.SelectedValue;
            ////parameters[1] = null;

            ////C1WindowExtension.SetParameters(popupSKIDSearch, parameters);

            //popupSKIDSearch.Closed += popupSKIDSearch_Closed;
            //Dispatcher.BeginInvoke(new Action(() => popupSKIDSearch.ShowModal()));
        }
        private void popupSKIDSearch_Closed(object sender, EventArgs e)
        {
            //BOX001_226_SKID_STATE popup = sender as BOX001_226_SKID_STATE;
            //if (popup != null)
            //{

            //}
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            bool adminflag = CheckManagerAuth();

            if (adminflag == true)
            {
                Util.MessageConfirm("SFU4616", result =>
                {
                    if (result == MessageBoxResult.OK)
                        SendCancelLogis();
                });
            }
            else
            {
                Util.MessageInfo("RTLS0003"); //취소권한이 없습니다.
            }
        }

        private void SendCancelLogis()
        {
            try
            {
                ShowLoadingIndicator();

                string bizRuleName;

                bizRuleName = "BR_MCS_REG_CANCEL_LOGIS_CMD_NJ";

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add("INDATA");
                indataSet.Tables.Add("INCMD");

                indataSet.Tables["INDATA"].Columns.Add("LOGIS_CMD_ID", typeof(string)); //물류 명령 ID
                indataSet.Tables["INDATA"].Columns.Add("EQPTID_TRSF", typeof(string)); //실제 이동 설비 ID
                indataSet.Tables["INDATA"].Columns.Add("LOGIS_EQPT_STAT_CODE", typeof(string)); //물류 설비 상태 코드
                indataSet.Tables["INDATA"].Columns.Add("USERID", typeof(string));

                indataSet.Tables["INCMD"].Columns.Add("LOGIS_CMD_ID", typeof(string));

                foreach (DataRow drSelect in dgInOutReservation.GetCheckedDataRow("CHK"))
                {
                    DataRow dr = indataSet.Tables["INDATA"].NewRow();
                    DataRow drcmd = indataSet.Tables["INCMD"].NewRow();

                    dr["LOGIS_CMD_ID"] = drSelect["LOGIS_CMD_ID"];
                    //dr["EQPTID_TRSF"] = cboStocker.SelectedValue;
                    dr["EQPTID_TRSF"] = drSelect["STK_EQPTID"];
                    dr["LOGIS_EQPT_STAT_CODE"] = null;
                    dr["USERID"] = LoginInfo.USERID;

                    drcmd["LOGIS_CMD_ID"] = drSelect["LOGIS_CMD_ID"];

                    indataSet.Tables["INDATA"].Rows.Add(dr);
                    indataSet.Tables["INCMD"].Rows.Add(drcmd);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCMD", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        SelectPancakeMonitoring();
                        SelectPancakeRank();
                        SelectInoutReservationInfo();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CheckManagerAuth()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "LOGIS_MANA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows?.Count <= 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void Search()
        {
            //if (!ValidationSelectJumboRollMonitoring()) return;

            ReSetElectrodPancakeControl();

            SelectPancakeMonitoring();
            SelectPancakeRank();
            SelectInoutReservationInfo();

            SetStairsRowDefinitions();
        }

        private void SetStairsRowDefinitions()
        {
            if (string.IsNullOrEmpty(cboStocker?.SelectedValue?.GetString()))
            {
                STK1_Auto();
                STK2_Auto();
                STK3_Auto();

                return;
            }

            if (cboStocker.SelectedValue.ToString().Equals("N1ESTO15301"))
            {
                STK2_Zero();
                STK3_Zero();

                STK1_Auto();
            }
            else if (cboStocker.SelectedValue.ToString().Equals("N1ESTO15302"))
            {
                STK1_Zero();
                STK3_Zero();

                STK2_Auto();
            }
            else if (cboStocker.SelectedValue.ToString().Equals("N1ESTO15303"))
            {
                STK1_Zero();
                STK2_Zero();

                STK3_Auto();
            }
        }

        private void STK1_Zero()
        {
            stairs.RowDefinitions[2].Height = new GridLength(0);
            stairs.RowDefinitions[3].Height = new GridLength(0);
            stairs.RowDefinitions[4].Height = new GridLength(0);
            stairs.RowDefinitions[5].Height = new GridLength(0);

            stairs.RowDefinitions[6].Height = new GridLength(0);
            stairs.RowDefinitions[7].Height = new GridLength(0);
            stairs.RowDefinitions[8].Height = new GridLength(0);
            stairs.RowDefinitions[9].Height = new GridLength(0);
        }

        private void STK2_Zero()
        {
            stairs.RowDefinitions[10].Height = new GridLength(0);
            stairs.RowDefinitions[11].Height = new GridLength(0);
            stairs.RowDefinitions[12].Height = new GridLength(0);
            stairs.RowDefinitions[14].Height = new GridLength(0);

            stairs.RowDefinitions[14].Height = new GridLength(0);
            stairs.RowDefinitions[15].Height = new GridLength(0);
            stairs.RowDefinitions[16].Height = new GridLength(0);
            stairs.RowDefinitions[17].Height = new GridLength(0);
        }

        private void STK3_Zero()
        {
            stairs.RowDefinitions[18].Height = new GridLength(0);
            stairs.RowDefinitions[19].Height = new GridLength(0);
            stairs.RowDefinitions[20].Height = new GridLength(0);
            stairs.RowDefinitions[21].Height = new GridLength(0);

            stairs.RowDefinitions[22].Height = new GridLength(0);
        }

        private void STK1_Auto()
        {
            stairs.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[3].Height = new GridLength(10);
            stairs.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[5].Height = new GridLength(10);

            stairs.RowDefinitions[6].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[7].Height = new GridLength(10);
            stairs.RowDefinitions[8].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[9].Height = new GridLength(10);
        }

        private void STK2_Auto()
        {
            stairs.RowDefinitions[10].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[11].Height = new GridLength(10);
            stairs.RowDefinitions[12].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[13].Height = new GridLength(10);

            stairs.RowDefinitions[14].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[15].Height = new GridLength(10);
            stairs.RowDefinitions[16].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[17].Height = new GridLength(10);
        }

        private void STK3_Auto()
        {
            stairs.RowDefinitions[18].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[19].Height = new GridLength(10);
            stairs.RowDefinitions[20].Height = new GridLength(1, GridUnitType.Star);
            stairs.RowDefinitions[21].Height = new GridLength(10);

            stairs.RowDefinitions[22].Height = new GridLength(1, GridUnitType.Star);
        }

        //NJ.1 STK-1
        private void NJ1_STK1_Zero()
        {
            stairs.RowDefinitions[18].Height = new GridLength(0);
            stairs.RowDefinitions[19].Height = new GridLength(0);
            stairs.RowDefinitions[20].Height = new GridLength(0);
            stairs.RowDefinitions[21].Height = new GridLength(0);
            stairs.RowDefinitions[22].Height = new GridLength(0);
        }

        private bool isRackBoxIDRsltChk(string sRackID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RACKID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["RACKID"] = sRackID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_TBOX_STK_RACK_BOXID_CHK", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult == null || dtResult.Rows.Count == 0)
                    return true;

                if (dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0]["BOX_FLAG"].ToString() == "Y")
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void dgInputRank_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgInputRank.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                {
                    return;
                }

                if (Util.NVC(e.Cell.Column.Name).IsNullOrEmpty())
                {
                    return;
                }

                if ((e.Cell.Column.Name.Equals("WIPHOLD") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")) == "Y"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }));
        }

        private void rankCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            if (dgInputRank.GetRowCount() == 0) return;

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            CheckBoxClickProcess(index);
        }

        private void CheckBoxClickProcess(int index)
        {
            //ShowLoadingIndicator();

            string sRackID = Util.NVC(DataTableConverter.GetValue(dgInputRank.Rows[index].DataItem, "RACK_ID"));

            if (Util.NVC(DataTableConverter.GetValue(dgInputRank.Rows[index].DataItem, "CHK")).Equals("True") ||
                Util.NVC(DataTableConverter.GetValue(dgInputRank.Rows[index].DataItem, "CHK")).Equals("1"))
            {
                _strRank_eltr = Util.NVC(DataTableConverter.GetValue(dgInputRank.Rows[index].DataItem, "ELTR_TYPE_CODE"));
                _strRank_mkt = Util.NVC(DataTableConverter.GetValue(dgInputRank.Rows[index].DataItem, "MKT_TYPE_NAME"));

                SelectPancakeFromRankChecked(sRackID);
            }
            else
            {
                _strRank_eltr = string.Empty;
                _strRank_mkt = string.Empty;

                SelectPancakeFromRankUnChecked(sRackID);
            }

            //HiddenLoadingIndicator();
        }

        /// <summary>
        /// 우선출고
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPriorityShipping_Click(object sender, RoutedEventArgs e)
        {
            // 우선출고 하시겠습니까?
            Util.MessageConfirm("SFU3826", result =>
            {
                if (result == MessageBoxResult.OK)
                    SetPriorityShipping();
            });
        }

        private void SetPriorityShipping()
        {
            try
            {
                ShowLoadingIndicator();

                if (dgInOutReservation.GetCheckedDataRow("CHK").Count != 1)
                {
                    Util.MessageInfo("SFU4468");    // 한개의 데이터만 선택하세요.
                    return;
                }

                const string bizRuleName = "BR_MCS_REG_PRIORITY_LOGIS_CMD_NJ";
                DataTable inDataTable = new DataTable("INDATA");
                DataRow dr = inDataTable.NewRow();

                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOGIS_CMD_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                foreach (DataRow drSelect in dgInOutReservation.GetCheckedDataRow("CHK"))
                {
                    if (drSelect["INOUT_TYPE_CODE"].ToString() != "RACK_TO_PORT")
                    {
                        Util.MessageInfo("SFU3825");    // 출고만 가능합니다.
                        return;
                    }

                    dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                    dr["EQPTID"] = drSelect["EQPTID"];
                    dr["LOGIS_CMD_ID"] = drSelect["LOGIS_CMD_ID"];
                    dr["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        SelectPancakeMonitoring();
                        SelectPancakeRank();
                        SelectInoutReservationInfo();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
    }
}

