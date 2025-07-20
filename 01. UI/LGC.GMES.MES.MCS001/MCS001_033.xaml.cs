/*************************************************************************************
 Created Date : 2019.11.08
      Creator : 신광희
   Decription : MEB CV 버퍼 모니터링 / 수동 출고
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.08  신광희 차장 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
using System.ComponentModel;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_033.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_033 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly DispatcherTimer _monitorTimerLami = new DispatcherTimer();
        private readonly DispatcherTimer _monitorTimerEmpty = new DispatcherTimer();
        private bool _isSelectedAutoTimeLami = false;
        private bool _isSelectedAutoTimeEmptySkid = false;
        private bool _isLoaded = false;

        private readonly Util _util = new Util();
        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        public MCS001_033()
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
            List<Button> listAuth = new List<Button> {btnSearchLami, btnSearchEmptySkid, btnSearchShippingHistory, btnShippingRequestLami, btnShippingRequestEmptySkid,btnCancel};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            InitializeCombo();
            InitializeControl();
            TimerSetting();

            Loaded -= UserControl_Loaded;
            _isLoaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            //ClearControl();

            if (TabItemLami.IsSelected)
            {
                SelectConveyorBufferMonitoring("MEB_LWB_DIVERT");
            }
            else if(TabItemEmptySkid.IsSelected)
            {
                SelectConveyorBufferMonitoring("MEB_ESB_DIVERT");
            }
            else if (TabItemShippingHistory.IsSelected)
            {
                SelectShippingHistoryList();
            }
        }

        private void btnShippingRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationShippingRequest()) return;

            if (TabItemLami.IsSelected)
            {
                SaveShippingRequestLamination();
            }

            if (TabItemEmptySkid.IsSelected)
            {
                SaveShippingRequestEmptySkid();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;
            SaveManualTransferCancel();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();


            switch (tabItem)
            {
                case "TabItemLami":
                {
                    _monitorTimerEmpty.Stop();
                    _monitorTimerLami.Start();
                    break;
                }
                case "TabItemEmptySkid":
                {
                    _monitorTimerLami.Stop();
                    _monitorTimerEmpty.Start();
                    break;
                }
                default:
                    _monitorTimerLami.Stop();
                    _monitorTimerEmpty.Stop();
                    break;
            }

        }

        private void dgLamiChoice_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void dgLamiList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "VD_QA_RESULT").GetString() == "NG")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgLamiList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgCathode_Checked(object sender, RoutedEventArgs e)
        {
            if (CommonVerify.HasDataGridRow(dgAnodeList))
            {
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgAnodeList, "CHK");

                if(rowIndex > -1)
                    DataTableConverter.SetValue(dgAnodeList.Rows[rowIndex].DataItem, "CHK", 0);
            }
        }

        private void dgCathodeList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "VD_QA_RESULT").GetString() == "NG" || DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD").GetString() == "Y")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgCathodeList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgAnode_Checked(object sender, RoutedEventArgs e)
        {
            if (CommonVerify.HasDataGridRow(dgCathodeList))
            {
                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgCathodeList, "CHK");

                if (rowIndex > -1)
                    DataTableConverter.SetValue(dgCathodeList.Rows[rowIndex].DataItem, "CHK", 0);
            }
        }

        private void dgAnodeList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "VD_QA_RESULT").GetString() == "NG" || DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD").GetString() == "Y")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgAnodeList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void ShippingHistory_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void dgShippingHistoryList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgShippingHistoryList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

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

        private void cboTimerLami_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimerLami != null)
                {
                    _monitorTimerLami.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimerLami?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimerLami.SelectedValue.ToString());
                        _isSelectedAutoTimeLami = true;
                    }
                    else
                    {
                        _isSelectedAutoTimeLami = false;
                    }

                    if (second == 0 && !_isSelectedAutoTimeLami)
                    {
                        Util.MessageValidation("SFU8127");
                        return;
                    }
                    _monitorTimerLami.Interval = new TimeSpan(0, 0, second);
                    _monitorTimerLami.Start();

                    if (_isSelectedAutoTimeLami)
                    {
                        //MEB CV 버퍼 모니터링 자동조회  %1초로 변경 되었습니다.
                        if (cboTimerLami != null)
                            Util.MessageInfo("SFU8126", cboTimerLami.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboTimerEmptySkid_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimerEmpty != null)
                {
                    _monitorTimerEmpty.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimerEmptySkid?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimerEmptySkid.SelectedValue.ToString());
                        _isSelectedAutoTimeEmptySkid = true;
                    }
                    else
                    {
                        _isSelectedAutoTimeEmptySkid = false;
                    }

                    if (second == 0 && !_isSelectedAutoTimeEmptySkid)
                    {
                        Util.MessageValidation("SFU8127");
                        return;
                    }
                    _monitorTimerEmpty.Interval = new TimeSpan(0, 0, second);
                    _monitorTimerEmpty.Start();

                    if (_isSelectedAutoTimeEmptySkid)
                    {
                        //MEB CV 버퍼 모니터링 자동조회  %1초로 변경 되었습니다.
                        if (cboTimerEmptySkid != null)
                            Util.MessageInfo("SFU8126", cboTimerEmptySkid.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Method

        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboTimerLami, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimerLami != null && cboTimerLami.Items.Count > 0)
                cboTimerLami.SelectedIndex = 0;

            if (_monitorTimerLami != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimerLami?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimerLami.SelectedValue.ToString());

                _monitorTimerLami.Tick += _dispatcherTimer_Tick;
                _monitorTimerLami.Interval = new TimeSpan(0, 0, second);
            }


            combo.SetCombo(cboTimerEmptySkid, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");
            if (cboTimerEmptySkid != null && cboTimerEmptySkid.Items.Count > 0)
                cboTimerEmptySkid.SelectedIndex = 0;

            if (_monitorTimerEmpty != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimerEmptySkid?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimerEmptySkid.SelectedValue.ToString());

                _monitorTimerEmpty.Tick += _dispatcherTimer_Tick;
                _monitorTimerEmpty.Interval = new TimeSpan(0, 0, second);
            }

        }

        private void SelectConveyorBufferMonitoring(string bufferType)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_MEB_CV_BUFFER_MONITORING";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("BUFFER", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BUFFER"] = bufferType;
                dr["ELTR_TYPE_CODE"] = null;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    if (TabItemLami.IsSelected)
                    {
                        result.Columns.Add(new DataColumn() { ColumnName = "RadioButtonVisibility", DataType = typeof(string) });

                        //int rowIndex = 0;
                        foreach (DataRow row in result.Rows)
                        {
                            //if (rowIndex == 0) row["VD_QA_RESULT"] = "NG";

                            // VD 결과 값이 NG인 경우에만 라디오버튼 Visible 처리
                            if (row["VD_QA_RESULT"].GetString() == "NG")
                                row["RadioButtonVisibility"] = "Visible";
                            else
                                row["RadioButtonVisibility"] = "Collapsed";

                            //rowIndex++;
                        }
                        result.AcceptChanges();
                        Util.GridSetData(dgLamiList, result, null, true);
                    }
                    else if (TabItemEmptySkid.IsSelected)
                    {
                        IEnumerable<DataRow> queryCathode = from t in result.AsEnumerable()
                            where t.Field<string>("PRDT_CLSS_CODE") == "C"
                            select t;

                        IEnumerable<DataRow> queryAnode = from t in result.AsEnumerable()
                            where t.Field<string>("PRDT_CLSS_CODE") == "A"
                            select t;

                        DataTable dtCathode = queryCathode.Any() ? queryCathode.CopyToDataTable() : result.Clone();
                        dtCathode.Columns.Add(new DataColumn() { ColumnName = "RadioButtonVisibility", DataType = typeof(string) });


                        foreach (DataRow row in dtCathode.Rows)
                        {
                            //VD 결과가 NG , Hold품만 선택 가능을 위해 라디오버튼 Visible 처리
                            if (row["WIPHOLD"].GetString() == "Y" || row["VD_QA_RESULT"].GetString() == "NG")
                            {
                                row["RadioButtonVisibility"] = "Visible";
                            }
                            else row["RadioButtonVisibility"] = "Collapsed";

                        }
                        dtCathode.AcceptChanges();
                        Util.GridSetData(dgCathodeList, dtCathode, null, true);

                        DataTable dtAnode = queryAnode.Any() ? queryAnode.CopyToDataTable() : result.Clone();
                        dtAnode.Columns.Add(new DataColumn() { ColumnName = "RadioButtonVisibility", DataType = typeof(string) });

                        foreach (DataRow row in dtAnode.Rows)
                        {
                            //VD 결과가 NG , Hold품만 선택 가능을 위해 라디오버튼 Visible 처리
                            if (row["WIPHOLD"].GetString() == "Y" || row["VD_QA_RESULT"].GetString() == "NG")
                            {
                                row["RadioButtonVisibility"] = "Visible";
                            }
                            else row["RadioButtonVisibility"] = "Collapsed";

                        }
                        dtAnode.AcceptChanges();
                        Util.GridSetData(dgAnodeList, dtAnode, null, true);
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

        private void SelectShippingHistoryList()
        {

            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MCS_REQ_TRF";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("MEB_CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["MEB_CMCODE"] = cboLocation.SelectedValue.GetString();
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    result.Columns.Add("SKID_ID", typeof(string));
                    result.Columns.Add("LOTID", typeof(string));
                    result.Columns.Add("WIPQTY", typeof(decimal));
                    result.Columns.Add("LOTYNAME", typeof(string));
                    result.Columns.Add("PRJT_NAME", typeof(string));
                    result.Columns.Add("PRODID", typeof(string));
                    result.Columns.Add("ELTR_TYPE_NAME", typeof(string));
                    result.Columns.Add(new DataColumn() { ColumnName = "RadioButtonVisibility", DataType = typeof(string) });

                    foreach (DataRow row in result.Rows)
                    {
                        // VD 결과 값이 NG인 경우에만 라디오버튼 Visible 처리
                        if (row["REQ_TRF_STAT"].GetString() == "REQUEST")
                            row["RadioButtonVisibility"] = "Visible";
                        else
                            row["RadioButtonVisibility"] = "Collapsed";
                    }
                    result.AcceptChanges();

                    IEnumerable<DataRow> query = from t in result.AsEnumerable()
                        where t.Field<string>("REQ_TRF_STAT") == "REQUEST" || t.Field<string>("REQ_TRF_STAT") == "ACCEPT" select t;

                    DataTable dtFilter = query.Any() ? query.CopyToDataTable() : result.Clone();

                    if (!CommonVerify.HasTableRow(dtFilter))
                    {
                        HiddenLoadingIndicator();
                        Util.GridSetData(dgShippingHistoryList, result, null, true);
                        return;
                    }

                    DataTable dtparameter = dtFilter.DefaultView.ToTable(true, new string[] { "CARRIERID"});
                    dtparameter.Columns.Add(new DataColumn() { ColumnName = "LANGID", DataType = typeof(string) });

                    DataTable inHistoryTable = new DataTable("RQSTDT");
                    inHistoryTable.Columns.Add("LANGID", typeof(string));
                    inHistoryTable.Columns.Add("CSTID", typeof(string));

                    foreach (DataRow dataRow in dtparameter.Rows)
                    {
                        DataRow newRow = inHistoryTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = dataRow["CARRIERID"];
                        inHistoryTable.Rows.Add(newRow);
                    }

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MEB_CV_BUFFER_MONITORING_HISTORY", "RQSTDT", "RSLTDT", inHistoryTable);

                    foreach (DataRow row in result.Rows)
                    {
                        var queryResult = (from t in dtResult.AsEnumerable()
                            where t.Field<string>("SKID_ID") == row["CARRIERID"].GetString()
                            select new
                            {
                                SkIdId = t.Field<string>("SKID_ID"),
                                LotId = t.Field<string>("LOTID"),
                                WipQty = t.Field<decimal>("WIPQTY"),
                                LotTypeName = t.Field<string>("LOTYNAME"),
                                ProjectName = t.Field<string>("PRJT_NAME"),
                                ProductId = t.Field<string>("PRODID"),
                                ElectrodeType = t.Field<string>("ELTR_TYPE_NAME")
                            }).FirstOrDefault();

                        if (queryResult != null)
                        {
                            if (row["REQ_TRF_STAT"].GetString() == "REQUEST" || row["REQ_TRF_STAT"].GetString() == "ACCEPT")
                            {
                                row["SKID_ID"] = queryResult.SkIdId;
                                row["LOTID"] = queryResult.LotId;
                                row["WIPQTY"] = queryResult.WipQty;
                                row["LOTYNAME"] = queryResult.LotTypeName;
                                row["PRJT_NAME"] = queryResult.ProjectName;
                                row["PRODID"] = queryResult.ProductId;
                                row["ELTR_TYPE_NAME"] = queryResult.ElectrodeType;
                            }
                        }
                    }
                    result.AcceptChanges();

                    HiddenLoadingIndicator();
                    Util.GridSetData(dgShippingHistoryList, result, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorBufferMonitoringHistory()
        {

        }

        private string SelectDestinationPort()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "MEB_CONVEYOR_BUFFER";
            dr["COM_CODE"] = "MEB_LWB_OUT_PORT";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);
            return dtResult.Rows[0]["ATTR1"].GetString();
        }

        private void SaveShippingRequestLamination()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_LOCID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("DTTM", typeof(DateTime));

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgLamiList, "CHK");

                DataRow dr = inTable.NewRow();
                dr["CARRIERID"] = DataTableConverter.GetValue(dgLamiList.Rows[rowIndex].DataItem, "SKID_ID");
                dr["SRC_LOCID"] = DataTableConverter.GetValue(dgLamiList.Rows[rowIndex].DataItem, "SRC_PORT");
                dr["DST_LOCID"] = SelectDestinationPort();
                dr["UPDUSER"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    //SelectConveyorBufferMonitoring("MEB_LWB_DIVERT");
                    btnSearch_Click(null,null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveShippingRequestEmptySkid()
        {
            try
            {
                ShowLoadingIndicator();

                C1DataGrid dg = null;

                if (_util.GetDataGridRowCountByCheck(dgCathodeList, "CHK") > 0)
                {
                    dg = dgCathodeList;
                }

                if (_util.GetDataGridRowCountByCheck(dgAnodeList, "CHK") > 0)
                {
                    dg = dgAnodeList;
                }

                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_LOCID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("DTTM", typeof(DateTime));

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dg, "CHK");

                DataRow dr = inTable.NewRow();
                dr["CARRIERID"] = DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "SKID_ID");
                dr["SRC_LOCID"] = DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "SRC_PORT");
                dr["DST_LOCID"] = SelectDestinationPort();
                dr["UPDUSER"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    btnSearch_Click(null, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveManualTransferCancel()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER_CHG_STAT";

                DataSet ds = new DataSet();
                DataTable requestTransferInfoTable = ds.Tables.Add("IN_REQ_TRF_INFO");
                requestTransferInfoTable.Columns.Add("CARRIERID", typeof(string));
                requestTransferInfoTable.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgShippingHistoryList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = requestTransferInfoTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "SKID_ID").GetString();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        requestTransferInfoTable.Rows.Add(newRow);
                    }
                }

                DataTable changeStateTable = ds.Tables.Add("IN_CHG_STAT");
                changeStateTable.Columns.Add("CHG_STAT", typeof(string));
                DataRow dr = changeStateTable.NewRow();
                dr["CHG_STAT"] = "CANCEL";
                changeStateTable.Rows.Add(dr);

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService_Multi(bizRuleName, "IN_REQ_TRF_INFO,IN_CHG_STAT", null, (bizResult, bizException) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                            btnSearch_Click(null, null);
                        }
                        catch (Exception ex)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageException(ex);
                        }
                    }, ds);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
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

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void InitializeControl()
        {
            TabItemLami.IsSelected = true;
            
        }

        private void InitializeCombo()
        {

            SetLocation(cboLocation);
        }

        private void ClearControl()
        {
            Util.gridClear(dgLamiList);
            Util.gridClear(dgCathodeList);
            Util.gridClear(dgAnodeList);
            Util.gridClear(dgShippingHistoryList);
        }

        private bool ValidationRelease()
        {
            //C1DataGrid dg = TabItemRealTray.IsSelected ? dgRealTrayDetail : dgEmptyTrayDetail;

            //if (!CommonVerify.HasDataGridRow(dg))
            //{
            //    Util.MessageValidation("SFU1636");
            //    return false;
            //}

            //if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            //{
            //    Util.MessageValidation("SFU1636");
            //    return false;
            //}

            return true;
        }

        private bool ValidationShippingRequest()
        {
            if (TabItemLami.IsSelected)
            {
                if (!CommonVerify.HasDataGridRow(dgLamiList))
                {
                    Util.MessageValidation("SFU1636");
                    return false;
                }

                if (_util.GetDataGridRowCountByCheck(dgLamiList, "CHK") < 1)
                {
                    Util.MessageValidation("SFU1636");
                    return false;
                }
            }
            else if (TabItemEmptySkid.IsSelected)
            {
                if (!CommonVerify.HasDataGridRow(dgCathodeList) && !CommonVerify.HasDataGridRow(dgAnodeList))
                {
                    Util.MessageValidation("SFU1636");
                    return false;
                }

                if (_util.GetDataGridRowCountByCheck(dgCathodeList, "CHK") < 1 && _util.GetDataGridRowCountByCheck(dgAnodeList, "CHK") < 1)
                {
                    Util.MessageValidation("SFU1636");
                    return false;
                }

            }

            return true;
        }

        private bool ValidationCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgShippingHistoryList))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgShippingHistoryList, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private static void SetLocation(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MMD_MCS_COMMONCODE";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "CMCODE" };
            string[] arrCondition = { LoginInfo.LANGID, "MEB_CONVEYOR_BUFFER", null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
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
