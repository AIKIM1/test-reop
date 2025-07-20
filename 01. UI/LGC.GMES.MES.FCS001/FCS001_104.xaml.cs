/*************************************************************************************
 Created Date : 2021.02.25
      Creator : 조영대
   Decription : 공 PLT 수동 출고 예약
--------------------------------------------------------------------------------------
 [Change History]
  2021.02.25  조영대      : : Initial Created.(Edit after copying from MCS001_027)
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_104.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_104 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private double _maxcheckCount = 0;

        private string _selectedRadioButtonValue;
        private string _projectName;
        private string _productVersion;
        private string _halfSlitterSideCode;
        private string _productCode;
        private string _holdCode;
        private string _pastDay;
        private string _lotId;
        private string _faultyType;
        private string _equipmentCode;
        private string _cstTypeCode;
        private string _abNormalReasonCode;
        private string _selectedWipHold;
        private string _selectedQmsHold;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        private DataTable _requestTransferInfoTable;
        private bool _isGradeJudgmentDisplay;
        private bool _isAdminAuthority;
        

        public FCS001_104()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            GetBizActorServerInfo();
            InitializeRequestTransferTable();
            _isAdminAuthority = IsAdminAuthorityByUserId(LoginInfo.USERID);
            _isGradeJudgmentDisplay = IsGradeJudgmentDisplay();
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>
            { btnManualIssue, btnTransferCancel};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            _selectedRadioButtonValue = "EMPTYCARRIER";

            InitializeGrid();
            InitializeCombo();

            SelectReleaseCount();

            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();

            //SetIssueEqpt();

            SetIssuePort();
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (tabMain.SelectedIndex == 0)
            {
                if (rdoEmptyCarrier.IsChecked == true)
                {
                    SelectWareHouseEmptyCarrier();
                }
            }
            else
            {
                SelectHistoryList(false);
            }
           
        }

        private void btnManualIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationManualIssue()) return;

            if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA")
            {
                SaveManualIssueByEsnb();
            }
            else
            {
                SaveManualIssue();
            }
        }

        private void btnTransferCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTransferCancel()) return;

            C1DataGrid dg = null;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }

            DataTable inTable = new DataTable();
            inTable.Columns.Add("RequestTransferId", typeof(string));
            inTable.Columns.Add("CARRIERID", typeof(string));

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    DataRow newRow = inTable.NewRow();
                    newRow["RequestTransferId"] = DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString();
                    newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                    inTable.Rows.Add(newRow);
                }
            }

            CMM_MCS_TRANSFER_CANCEL popupTransferCancel = new CMM_MCS_TRANSFER_CANCEL { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = inTable;
            C1WindowExtension.SetParameters(popupTransferCancel, parameters);

            popupTransferCancel.Closed += popupTransferCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTransferCancel.ShowModal()));
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMain.SelectedIndex == 0)
            {
                dtpStart.IsEnabled = false;
            }
            else
            {
                dtpStart.IsEnabled = true;
            }
        }

        private void popupTransferCancel_Closed(object sender, EventArgs e)
        {
            CMM_MCS_TRANSFER_CANCEL popup = sender as CMM_MCS_TRANSFER_CANCEL;
            if (popup != null && popup.IsUpdated)
            {
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
                }
            }
        }
        
        private void rdoRelease_Checked(object sender, RoutedEventArgs e)
        {

            if (dgStoreByEmptyPLT == null) return;

            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null) return;


            dgStoreByEmptyPLT.Visibility = Visibility.Collapsed;
            dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Collapsed;

            switch (radioButton.Name)
            {
                case "rdoEmptyCarrier":
                    dgIssueTargetInfoByEmptyCarrier.Visibility = Visibility.Visible;
                    dgStoreByEmptyPLT.Visibility = Visibility.Visible;
                    _selectedRadioButtonValue = "EMPTYCARRIER";
                    break;
            }

            ClearControl();
            SetStockerTypeCombo(cboStockerType);
        }
                
        private void dgStoreByEmptyPLT_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Convert.ToString(e.Cell.Column.Name) == "PLT_QTY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PLT_QTY").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }                    
                }
            }));
        }

        private void dgStoreByEmptyPLT_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        
        private void dgStoreByEmptyPLT_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;
                C1DataGrid dataGrid = sender as C1DataGrid;

                if (dataGrid == null || dataGrid.CurrentCell == null || dataGrid.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dataGrid.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;
                
                txtEquipmentName.Text = string.Empty;
                SelectWareHouseEmptyCarrierList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgIssueTargetInfoByEmptyCarrier_CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                dgIssueTargetInfoByEmptyCarrier.SelectedIndex = ((DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                C1DataGrid dg = dgIssueTargetInfoByEmptyCarrier;
                if (dg.IsCheckedRow("CHK"))
                {
                        SelectPortInfo(dg.GetStringValue("EQPTID"));
                        txtEquipmentName.Text = dg.GetStringValue("EQPTID");
                }

                txtSelectCount.Text = ObjectDic.Instance.GetObjectName("선택수량") + " : " +
                    dgIssueTargetInfoByEmptyCarrier.GetCheckedDataRow("CHK").Count.ToString() +
                    ObjectDic.Instance.GetObjectName("건");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void cboIssueEqpt_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //SetIssuePort();
        }
        
        private void cboIssuePort_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {

        }

        private void dgPortInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRF_STAT_CODE").GetString() == "OUT_OF_SERVICE")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#BDBDBD");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }

                }
            }));
        }

        private void dgPortInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        #region Method

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

        private void SelectRequestTransferList()
        {
            if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA") return;

            const string bizRuleName = "DA_SEL_MCS_REQ_TRF_MES_GUI";
            DataTable inDataTable = new DataTable("INDATA");
            _requestTransferInfoTable = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, null, "RSLTDT", inDataTable);
        }

        private void SelectWareHouseEmptyCarrier()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPTY_PLT";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboStockerType.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var query = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        EmptyPltType = ObjectDic.Instance.GetObjectName("합계"),
                        PltCount = g.Sum(x => x.Field<Int32>("PLT_QTY")),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (query != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EMPT_PLT_TYPE"] = query.EmptyPltType;
                        newRow["PLT_QTY"] = query.PltCount;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgStoreByEmptyPLT, bizResult, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private void SelectWareHouseEmptyCarrierList()
        {
            txtSelectCount.Text = ObjectDic.Instance.GetObjectName("선택수량") + " : 0" + ObjectDic.Instance.GetObjectName("건");

            SelectRequestTransferList();
            Util.gridClear(dgPortInfo);

            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPTY_PLT_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["CSTTYPE"] = dgStoreByEmptyPLT.GetValue("CSTTYPE");
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA")
                    {
                        Util.GridSetData(dgIssueTargetInfoByEmptyCarrier, bizResult, null, true);
                        return;
                    }

                    if (!bizResult.Columns.Contains("REQ_TRF_STAT"))
                    {
                        bizResult.Columns.Add("REQ_TRF_STAT", typeof(string));
                        bizResult.Columns.Add("CARRIERID", typeof(string));
                        bizResult.Columns.Add("REQ_TRFID", typeof(string));
                    }

                    if (CommonVerify.HasTableRow(_requestTransferInfoTable))
                    {
                        foreach (DataRow row in bizResult.Rows)
                        {
                            //창고유형 = “JRW” 이면 BOBBIN_ID (MES) = CARRIERID (MCS)
                            if (cboStockerType?.SelectedValue.GetString() == "JWR")
                            {
                                var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                             where t.Field<string>("CARRIERID") == row["BOBBIN_ID"].GetString()
                                             select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                                if (query != null)
                                {
                                    row["REQ_TRF_STAT"] = query.RequestTransferState;
                                    row["CARRIERID"] = query.CarrierId;
                                    row["REQ_TRFID"] = query.RequestTransferId;
                                }
                            }
                            else //창고유형 != “JRW” 이면SKID_ID (MES) = CARRIERID (MCS) 
                            {
                                var query = (from t in _requestTransferInfoTable.AsEnumerable()
                                             where t.Field<string>("CARRIERID") == row["SKID_ID"].GetString()
                                             select new { RequestTransferState = t.Field<string>("REQ_TRF_STAT"), CarrierId = t.Field<string>("CARRIERID"), RequestTransferId = t.Field<string>("REQ_TRFID") }).FirstOrDefault();

                                if (query != null)
                                {
                                    row["REQ_TRF_STAT"] = query.RequestTransferState;
                                    row["CARRIERID"] = query.CarrierId;
                                    row["REQ_TRFID"] = query.RequestTransferId;
                                }
                            }
                        }
                        bizResult.AcceptChanges();
                    }

                    Util.GridSetData(dgIssueTargetInfoByEmptyCarrier, bizResult, null, true);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectHistoryList(bool isDoubleClick)
        {
            try
            {
                ShowLoadingIndicator();

                dgHistory.ClearRows();

                DataTable INDATA = new DataTable("RQSTDT");
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("TRF_CLSS_CODE", typeof(string));
                INDATA.Columns.Add("FROM_DATE", typeof(string));
                INDATA.Columns.Add("TO_DATE", typeof(string));
                INDATA.Columns.Add("EQGRID", typeof(string));

                DataRow inData = INDATA.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["TRF_CLSS_CODE"] = "U";
                inData["FROM_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                inData["TO_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                inData["EQGRID"] = cboStockerType.GetBindValue();
                INDATA.Rows.Add(inData);

                new ClientProxy().ExecuteService("DA_MHS_SEL_TRF_CMD_HISTORY_LIST_MANUAL", "RQSTDT", "RSLTDT", INDATA, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    dgHistory.SetItemsSource(result, FrameOperation, true);

                    HiddenLoadingIndicator();
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SelectReleaseCount()
        {
            const string bizRuleName = "DA_MCS_SEL_COMMCODE";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["CMCDTYPE"] = "CWA_JRW_FIFO_DEFAULT_CNT";
            dr["CMCODE"] = "DEFAULT_VAL";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["ATTRIBUTE1"].GetInt() > 3 || dtResult.Rows[0]["ATTRIBUTE1"].GetInt() < 1)
                {
                    _maxcheckCount = 1;
                }
                else
                {
                    _maxcheckCount = dtResult.Rows[0]["ATTRIBUTE1"].GetDouble();
                }
            }
        }
        
        private void SelectPortInfo(string equipmentCode)
        {
            try
            {
                const string bizRuleName = "DA_SEL_MCS_LOC_PORT_INFO_EQPT";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = equipmentCode;
                inTable.Rows.Add(dr);

                if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA")
                {
                    new ClientProxy().ExecuteService("DA_MCS_SEL_WAREHOUSE_EMPTY_PORT_INFO", "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.GridSetData(dgPortInfo, result, null, true);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    });
                }
                else
                {
                    new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.GridSetData(dgPortInfo, result, null, true);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveManualIssue()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_GUI_REG_TRF_JOB_BYUSER";

                DateTime dtSystem = GetSystemTime();

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_LOCID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("DTTM", typeof(DateTime));

                C1DataGrid dg = null;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dg = dgIssueTargetInfoByEmptyCarrier;
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        string carrierId;
                        if (string.Equals(dg.Name, "dgIssueTargetInfoByNoReadCarrier") || string.Equals(dg.Name, "dgIssueTargetInfoByAbNormalCarrier"))
                        {
                            carrierId = "MCS_CST_ID";
                        }
                        else
                        {
                            carrierId = "SKID_ID";
                        }

                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, carrierId).GetString();
                        newRow["SRC_LOCID"] = DataTableConverter.GetValue(row.DataItem, "RACK_ID").GetString();
                        newRow["DST_LOCID"] = cboIssuePort.GetBindValue();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["DTTM"] = dtSystem;
                        inTable.Rows.Add(newRow);
                    }
                }

                new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", "OUT_REQ_TRF_INFO", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                        if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                        {
                            SelectWareHouseEmptyCarrierList();
                        }
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

        }

        private void SaveManualIssueByEsnb()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MHS_REG_TRF_CMD_BY_UI";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));

                C1DataGrid dg = null;
                if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                {
                    dg = dgIssueTargetInfoByEmptyCarrier;
                }

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        string carrierId;
                        if (string.Equals(dg.Name, "dgIssueTargetInfoByNoReadCarrier") || string.Equals(dg.Name, "dgIssueTargetInfoByAbNormalCarrier"))
                        {
                            carrierId = "MCS_CST_ID";
                        }
                        else
                        {
                            carrierId = "SKID_ID";
                        }

                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, carrierId).GetString();
                        newRow["SRC_EQPTID"] = DataTableConverter.GetValue(row.DataItem, "EQPTID").GetString();
                        newRow["DST_EQPTID"] = cboIssuePort.GetBindValue("TRGT_EQPTID");
                        newRow["DST_LOCID"] = cboIssuePort.GetBindValue();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        newRow["TRF_CAUSE_CODE"] = null;
                        newRow["MANL_TRF_CAUSE_CNTT"] = null;
                        inTable.Rows.Add(newRow);
                    }
                }
                
                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU8111"); //이동명령이 예약되었습니다

                        if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
                        {

                            SelectWareHouseEmptyCarrierList();
                        }
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private void GetBizActorServerInfo()
        {
            if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA") return;

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

        private bool IsAdminAuthorityByUserId(string userId)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["USERID"] = userId;
                dr["AUTHID"] = "MESADMIN,MESDEV,LOGIS_MANA";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsGradeJudgmentDisplay()
        {
            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        
        private void InitializeRequestTransferTable()
        {
            _requestTransferInfoTable = new DataTable();
            _requestTransferInfoTable.Columns.Add("CARRIERID", typeof(string));
            _requestTransferInfoTable.Columns.Add("REQ_TRF_STAT", typeof(string));
            _requestTransferInfoTable.Columns.Add("REQ_TRFID", typeof(string));
            _requestTransferInfoTable.Columns.Add("SRC_LOCID", typeof(string));
            _requestTransferInfoTable.Columns.Add("DST_LOCID", typeof(string));
            _requestTransferInfoTable.Columns.Add("JOBID", typeof(string));
        }

        private void InitializeGrid()
        {
            txtSelectCount.Text = ObjectDic.Instance.GetObjectName("선택수량") + " : 0" + ObjectDic.Instance.GetObjectName("건");
        }

        private void InitializeCombo()
        {
            // 창고 유형 콤보박스
            SetStockerTypeCombo(cboStockerType);            
        }

        private void ClearControl()
        {
            _projectName = string.Empty;
            _productVersion = string.Empty;
            _halfSlitterSideCode = string.Empty;
            _productCode = string.Empty;
            _holdCode = string.Empty;
            _pastDay = string.Empty;
            _lotId = string.Empty;
            _faultyType = string.Empty;
            _equipmentCode = string.Empty;
            _cstTypeCode = string.Empty;
            _abNormalReasonCode = string.Empty;
            txtEquipmentName.Text = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;

       
            Util.gridClear(dgStoreByEmptyPLT);

            Util.gridClear(dgIssueTargetInfoByEmptyCarrier);

            Util.gridClear(dgPortInfo);

            dgHistory.ClearRows();

            _requestTransferInfoTable.Clear();

            //if (cboIssueEqpt.SelectedItem != null && cboIssueEqpt.Items.Count > 0)
            //{
            //    SetIssueEqpt();
            //}

            if (cboIssuePort.SelectedItem != null && cboIssuePort.Items.Count > 0)
            {
                SetIssuePort();
            }        
        }
        
        private bool ValidationManualIssue()
        {
            C1DataGrid dg = null;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }

            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboIssuePort.SelectedItem == null || string.IsNullOrEmpty(cboIssuePort.GetStringValue()))
            {
                Util.MessageValidation("MCS1004");
                return false;
            }

            //if ((cboIssuePort.SelectedItem as C1ComboBoxItem).Name == "OUT_OF_SERVICE")
            //{
            //    Util.MessageInfo("SFU8137");
            //    return false;
            //}

            return true;
        }

        private bool ValidationTransferCancel()
        {
            C1DataGrid dg = null;
            if (string.Equals(_selectedRadioButtonValue, "EMPTYCARRIER"))
            {
                dg = dgIssueTargetInfoByEmptyCarrier;
            }
            if (!CommonVerify.HasDataGridRow(dg))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dg, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            // CNB 2동의 경우 반송요청 관련 BizRule에서 처리 하므로 아래의 조건은 Pass
            if (LoginInfo.CFG_AREA_ID == "EF" || LoginInfo.CFG_AREA_ID == "AA")
            {
                if (!ValidationTransferCancelByEsnb(dg))
                {
                    return false;
                }

                return true;
            }


            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "REQ_TRFID").GetString()) || DataTableConverter.GetValue(row.DataItem, "REQ_TRF_STAT").GetString() != "REQUEST")
                    {
                        Util.MessageInfo("SFU8116", ObjectDic.Instance.GetObjectName("반송요청상태"));
                        return false;
                    }
                }
            }

            return true;

        }
        
        private bool ValidationTransferCancelByEsnb(C1DataGrid dg)
        {
            try
            {
                DataTable inTable = new DataTable("IN_DATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["CSTID"] = DataTableConverter.GetValue(row.DataItem, "CARRIERID").GetString();
                        inTable.Rows.Add(newRow);
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_MHS_CHK_SEL_TRF_CMD_CANCEL_BY_UI", "IN_DATA", "OUT_DATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Rows[0]["RETVAL"].GetString() != "0")
                    {
                        return false;
                    }

                    return true;
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

        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPT_PLT_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetIssueEqpt()
        {
            try
            {
                cboIssueEqpt.ClearItems();

                DataTable inTable = new DataTable("IN_EQPT_INFO");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboStockerType.GetBindValue();
                
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_WAREHOUSE_EMPT_PLT_TRGT_EQPT_CBO", "IN_EQPT_INFO", "OUT_DEST_INFO", inTable);

                cboIssueEqpt.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                {
                    cboIssueEqpt.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetIssuePort()
        {
            try
            {
                cboIssuePort.ClearItems();

                DataTable inTable = new DataTable("IN_EQPT_INFO");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboStockerType.GetBindValue();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_WAREHOUSE_EMPT_PLT_TRGT_PORT_CBO", "IN_EQPT_INFO", "OUT_DEST_INFO", inTable);

                cboIssuePort.ItemsSource = dtResult.Copy().AsDataView();

                if (dtResult.Rows.Count > 0)
                {
                    cboIssuePort.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion


    }
}
