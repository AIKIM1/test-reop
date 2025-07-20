/*************************************************************************************
 Created Date : 2018.10.03
      Creator : 신광희 차장
   Decription : Foil 재고 현황 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.03  신광희 차장 : Initial Created.    
**************************************************************************************/

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_015.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_015 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly System.Windows.Threading.DispatcherTimer _monitorTimer = new System.Windows.Threading.DispatcherTimer();
        private bool _isSetAutoSelectTime = false;
        private readonly Util _util = new Util();
        private DataTable _dtResultPortInfo;
        private DataTable _dtPortInfo;
        
        #endregion

        public MCS001_015()
        {
            InitializeComponent();
            InitializeCombo();
            InitializePortInfo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            TimerSetting();
            SelectFoilStockMonitoring();
            SelectWorkOrderListWithFactoryPlanCoater();
            btnMaterialSupplyRequest_Click(btnMaterialSupplyRequest, null);
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _monitorTimer.Stop();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializeCheckBoxFromFoilStockInfo();
                InitializeTextBlockFromFoilStockInfo();

                SelectFoilStockMonitoring();
                SelectWorkOrderListWithFactoryPlanCoater();
                btnMaterialSupplyRequest_Click(btnMaterialSupplyRequest, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;

                    if (!string.IsNullOrEmpty(cboAutoSearch?.SelectedValue?.ToString()))
                    {
                        second = int.Parse(cboAutoSearch.SelectedValue.ToString());
                        _isSetAutoSelectTime = true;
                    }
                    else
                    {
                        _isSetAutoSelectTime = false;
                    }


                    if (second == 0 && _isSetAutoSelectTime)
                    {   //Foil 재고 현황 모니터링 자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU5060");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSetAutoSelectTime)
                    {
                        // Foil 재고 현황 모니터링 자동조회  %1초로 변경 되었습니다.
                        if (cboAutoSearch != null)
                            Util.MessageInfo("SFU5034", cboAutoSearch.SelectedValue.GetString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnFoilSupplyChoice_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationFoilSupplyChoice()) return;

            _monitorTimer?.Stop();

            MCS001_015_FOIL_SUPPLY_CHOICE popupFoilSupplyChoice = new MCS001_015_FOIL_SUPPLY_CHOICE { FrameOperation = FrameOperation };
            object[] parameters = new object[4];
            parameters[0] = dtpDateFrom.SelectedDateTime.ToShortDateString();
            parameters[1] = dtpDateTo.SelectedDateTime.ToShortDateString();
            parameters[2] = _util.GetDataGridFirstRowBycheck(dgMaterialSupply, "CHK",true).Field<string>("MTRL_SPLY_REQ_ID").GetString();
            parameters[3] = rdoAnode.IsChecked != null && (bool)rdoAnode.IsChecked ? "C" : "A";
            C1WindowExtension.SetParameters(popupFoilSupplyChoice, parameters);

            popupFoilSupplyChoice.Closed += popupFoilSupplyChoice_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupFoilSupplyChoice.ShowModal()));
        }

        private void popupFoilSupplyChoice_Closed(object sender, EventArgs e)
        {
            MCS001_015_FOIL_SUPPLY_CHOICE popup = sender as MCS001_015_FOIL_SUPPLY_CHOICE;
            if (popup != null && popup.IsUpdated)
            {
                btnMaterialSupplyRequest_Click(btnMaterialSupplyRequest, null);
            }

            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0)
                _monitorTimer.Start();
        }

        private void btnMaterialSupplyRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_GET_MTRL_SPLY_REQ_LIST";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("MTRL_SPLY_REQ_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("MTRLID", typeof(string));
                inDataTable.Columns.Add("MLOTID", typeof(string));
                inDataTable.Columns.Add("QRY_TYPE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["MTRL_ELTR_TYPE_CODE"] = rdoAnode.IsChecked != null && (bool)rdoAnode.IsChecked ? "C" : "A";
                dr["MTRL_SPLY_REQ_STAT_CODE"] = null;
                dr["EQPTID"] = null;
                dr["MTRLID"] = null;
                dr["MLOTID"] = null;
                dr["QRY_TYPE"] = "REQ";
                inDataTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inDataTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        foreach (DataRow item in bizResult.Rows)
                        {
                            if (item["SPLY_TRGT_FLAG"].GetString() == "Y")
                                item["CHK"] = true;
                            else
                                item["CHK"] = false;
                        }
                    }

                    Util.GridSetData(dgMaterialSupply, bizResult, null,true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgMaterialSupplyChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }

                        //row 색 바꾸기
                        dgMaterialSupply.SelectedIndex = idx;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMaterialSupply_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMaterialSupply()) return;

            object[] parameters = new object[2];
            parameters[0] = string.Empty;
            parameters[1] = string.Empty;
            FrameOperation.OpenMenu("SFU010180140", true, parameters);
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(btnSearch, null);
                    SelectWorkOrderListWithFactoryPlanCoater();
                    btnMaterialSupplyRequest_Click(btnMaterialSupplyRequest, null);

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

        private void btnCarrierMoveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCarrierMoveOrder()) return;
            _monitorTimer?.Stop();

            MCS001_015_CARRIER_MOVE_ORDER popupCarrierMoveOrder = new MCS001_015_CARRIER_MOVE_ORDER { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = DataTableConverter.GetValue(dgPortInfo.Rows[0].DataItem, "PORT_ID").GetString();
            parameters[1] = ((DataView)dgPortInfo.ItemsSource).Table.Rows[0];
            C1WindowExtension.SetParameters(popupCarrierMoveOrder, parameters);

            C1WindowExtension.SetParameters(popupCarrierMoveOrder, parameters);

            popupCarrierMoveOrder.Closed += popupCarrierMoveOrder_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCarrierMoveOrder.ShowModal()));
        }

        private void popupCarrierMoveOrder_Closed(object sender, EventArgs e)
        {
            MCS001_015_CARRIER_MOVE_ORDER popup = sender as MCS001_015_CARRIER_MOVE_ORDER;
            if (popup != null && popup.IsUpdated)
            {
                btnSearch_Click(btnSearch, null);
            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
        }

        private void btnFoilSupplyRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationFoilSupplyRequest()) return;
            _monitorTimer?.Stop();

            MCS001_015_FOIL_SUPPLY_REQUEST popupSupplyRequest = new MCS001_015_FOIL_SUPPLY_REQUEST { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = string.Empty;
            parameters[1] = string.Empty;

            C1WindowExtension.SetParameters(popupSupplyRequest, parameters);

            popupSupplyRequest.Closed += popupSupplyRequest_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupSupplyRequest.ShowModal()));
        }

        private void popupSupplyRequest_Closed(object sender, EventArgs e)
        {
            MCS001_015_FOIL_SUPPLY_REQUEST popup = sender as MCS001_015_FOIL_SUPPLY_REQUEST;
            if (popup != null && popup.IsUpdated)
            {
                //
            }
            if (_monitorTimer != null && _monitorTimer.Interval.TotalSeconds > 0) _monitorTimer.Start();
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
                    if (Convert.ToString(e.Cell.Column.Name) == "생산잔량")
                    {
                        var convertFromString = ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
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

        private void checkFoilStock_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                foreach (CheckBox check in Util.FindVisualChildren<CheckBox>(gdFoilStock))
                {
                    if (!Equals(check, checkBox))
                    {
                        if (check.IsChecked != null && (bool)check.IsChecked)
                            check.IsChecked = false;
                    }
                }

                string checkBoxName = checkBox.Name.Remove(0,5);
                CheckFoilStockInfo(checkBoxName);
            }

        }

        private void checkFoilStock_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                //string checkBoxName = checkBox.Name.Remove(0, 5);
                UnCheckFoilStockInfo();
            }
        }

        #endregion

        #region Mehod
        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();

            // 자동 조회 시간 Combo
            string[] filter = { "SECOND_INTERVAL" };
            combo.SetCombo(cboAutoSearch, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboAutoSearch != null && cboAutoSearch.Items.Count > 0)
                cboAutoSearch.SelectedIndex = 0;
        }

        private void InitializePortInfo()
        {
            _dtResultPortInfo = new DataTable();
            _dtResultPortInfo.Columns.Add("PORT_ID", typeof(string));
            _dtResultPortInfo.Columns.Add("PORT_NAME", typeof(string));
            _dtResultPortInfo.Columns.Add("PORT_TYPE_CODE", typeof(string));
            _dtResultPortInfo.Columns.Add("INOUT_TYPE_CODE", typeof(string));
            _dtResultPortInfo.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            _dtResultPortInfo.Columns.Add("EQPTID", typeof(string));
            _dtResultPortInfo.Columns.Add("PORTSEQ", typeof(decimal));
            _dtResultPortInfo.Columns.Add("USE_FLAG", typeof(string));
            _dtResultPortInfo.Columns.Add("X_PSTN", typeof(Int32));
            _dtResultPortInfo.Columns.Add("Y_PSTN", typeof(Int32));
            _dtResultPortInfo.Columns.Add("Z_PSTN", typeof(Int32));
            _dtResultPortInfo.Columns.Add("SCRN_X_PSTN_VALUE", typeof(Int32));
            _dtResultPortInfo.Columns.Add("SCRN_Y_PSTN_VALUE", typeof(Int32));
            _dtResultPortInfo.Columns.Add("CALDATE", typeof(DateTime));
            _dtResultPortInfo.Columns.Add("WH_RCV_DTTM", typeof(DateTime));
            _dtResultPortInfo.Columns.Add("MCS_CST_ID", typeof(string));
            _dtResultPortInfo.Columns.Add("EMPTY_CST_FLAG", typeof(string));
            _dtResultPortInfo.Columns.Add("MCS_WIP_STAT", typeof(string));
            _dtResultPortInfo.Columns.Add("MTRLID", typeof(string));
            _dtResultPortInfo.Columns.Add("MLOTID", typeof(string));
            _dtResultPortInfo.Columns.Add("MLOTQTY_CUR", typeof(decimal));
            _dtResultPortInfo.Columns.Add("MLOTQTY_STOCKED", typeof(decimal));
            _dtResultPortInfo.Columns.Add("MLOTDTTM_STOCKED", typeof(DateTime));
            _dtResultPortInfo.Columns.Add("BOMREV", typeof(string));
            _dtResultPortInfo.Columns.Add("MTRLNAME", typeof(string));
            _dtResultPortInfo.Columns.Add("MTRLDESC", typeof(string));
            _dtResultPortInfo.Columns.Add("MTRLUNIT", typeof(string));
            _dtResultPortInfo.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
            _dtResultPortInfo.Columns.Add("MTRL_SPLY_REQ_DTTM", typeof(string));
            _dtResultPortInfo.Columns.Add("MTRL_ELTR_TYPE_CODE", typeof(string));
            _dtResultPortInfo.Columns.Add("SPLY_TRGT_FLAG", typeof(string));
            _dtResultPortInfo.Columns.Add("REQ_EQPTID", typeof(string));
            _dtResultPortInfo.Columns.Add("REQ_EQPTNAME", typeof(string));

            _dtPortInfo = _dtResultPortInfo.Clone();
        }

        private void TimerSetting()
        {
            if (_monitorTimer != null)
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !cboAutoSearch.SelectedValue.ToString().Equals(""))
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);
            }
        }

        private void SelectFoilStockMonitoring()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_MCS_SEL_BUF_FOIL_MONITORING";

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("MLOTID", typeof(string));
                inDataTable.Columns.Add("MCS_CST_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //dr["MLOTID"] = string.Empty;
                //dr["MCS_CST_ID"] = string.Empty;
                inDataTable.Rows.Add(dr);

                _dtResultPortInfo.Clear();
                _dtPortInfo.Clear();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    _dtResultPortInfo = bizResult.Copy(); //Util.CheckBoxColumnAddTable(bizResult, true).Copy();

                    foreach (TextBlock textBlock in Util.FindVisualChildren<TextBlock>(gdFoilStock))
                    {
                        if (textBlock.Name.StartsWith("txtCST"))
                        {
                            textBlock.Text = GetFoilStockCarrierIdByPortId(textBlock.Name.Remove(0, 6), _dtResultPortInfo);
                        }
                        else if (textBlock.Name.StartsWith("txtMLOT"))
                        {
                            textBlock.Text = GetFoilStockMLotIdByPortId(textBlock.Name.Remove(0, 7), _dtResultPortInfo);
                        }
                        else if(textBlock.Name.StartsWith("txtInoutType"))
                        {
                            textBlock.Text = GetFoilCurrentInoutTypeByPortId(textBlock.Name.Remove(0, 12), _dtResultPortInfo);
                        }
                        else if (textBlock.Name.StartsWith("txtPortStateCode"))
                        {
                            textBlock.Text = GetFoilPortStateCodeByPortId(textBlock.Name.Remove(0, 16), _dtResultPortInfo);
                        }
                    }

                    //txtCSTEBB017.Text = "MFL01142AA20190108033416";
                    //txtCSTEBB017.Text = "MFL01142AA201901" + Environment.NewLine + "08033416";

                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWorkOrderListWithFactoryPlanCoater()
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_WORKORDER_LIST_WITH_FP_COATER";
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["COAT_SIDE_TYPE"] = null;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, null, false);
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetFoilStockMLotIdByPortId(string portId, DataTable dt)
        {
            var query = (from t in dt.AsEnumerable()
                where t.Field<string>("PORT_ID") == portId
                select new { MLotId = t.Field<string>("MLOTID") }).FirstOrDefault();

            if (query != null)
                return GetTextValue(query.MLotId);

            return string.Empty;
        }

        private string GetFoilStockCarrierIdByPortId(string portId, DataTable dt)
        {
            var query = (from t in dt.AsEnumerable()
                where t.Field<string>("PORT_ID") == portId
                select new { CarrierId = t.Field<string>("MCS_CST_ID") }).FirstOrDefault();

            if (query != null)
                return GetTextValue(query.CarrierId);

            return string.Empty;
        }

        private static string GetFoilCurrentInoutTypeByPortId(string portId, DataTable dt)
        {
            var query = (from t in dt.AsEnumerable()
                where t.Field<string>("PORT_ID") == portId
                select new { CurrentInoutTypeCode = t.Field<string>("CURR_INOUT_TYPE_CODE") }).FirstOrDefault();

            if (query != null)
                return query.CurrentInoutTypeCode;

            return string.Empty;
        }

        private static string GetFoilPortStateCodeByPortId(string portId, DataTable dt)
        {
            var query = (from t in dt.AsEnumerable()
                where t.Field<string>("PORT_ID") == portId
                select new { PortStateCode = t.Field<string>("PORT_STAT_CODE") }).FirstOrDefault();

            if (query != null)
                return query.PortStateCode;

            return string.Empty;
        }

        private string GetTextValue(string item)
        {
            if (!string.IsNullOrEmpty(item) && item.Length > 15)
            {
                string firstId = item.Substring(0, 15);
                string lastId = item.Substring(15);
                return firstId + Environment.NewLine + lastId;
            }
            else
            {
                return item;
            }
        }

        private void CheckFoilStockInfo(string portId)
        {

            if (CommonVerify.HasTableRow(_dtPortInfo))
            {
                for (int i = 0; i < _dtPortInfo.Rows.Count; i++)
                {
                    _dtPortInfo.Rows.RemoveAt(i);
                }
            }

            DataRow row = (from t in _dtResultPortInfo.AsEnumerable()
                where t.Field<string>("PORT_ID") == portId
                select t).FirstOrDefault();

            if (row != null)
            {
                _dtPortInfo.ImportRow(row);
                Util.GridSetData(dgPortInfo, _dtPortInfo, null, true);
            }

            /*
            DataTable dtPortInfo = _dtPortInfo.Clone();

            var query = (from t in _dtPortInfo.Copy().AsEnumerable()
                where t.Field<string>("PORT_ID") == portId
                select new
                {
                    PortId = t.Field<string>("PORT_ID"),
                    PortName = t.Field<string>("PORT_NAME"),
                    PortTypeCode = t.Field<string>("PORT_TYPE_CODE"),
                    InoutTypecode = t.Field<string>("INOUT_TYPE_CODE"),
                    ElectrodeTypeCode = t.Field<string>("ELTR_TYPE_CODE"),
                    EquipmentCode = t.Field<string>("EQPTID"),
                    PortSeq = t.Field<decimal>("PORTSEQ"),
                    UseFlag = t.Field<string>("USE_FLAG"),
                    //xPosition = t.Field<Int32>("X_PSTN"),
                    //yPosition = t.Field<Int32>("Y_PSTN"),
                    //zPosition = t.Field<Int32>("Z_PSTN"),
                    //xPositionValue = t.Field<Int32>("SCRN_X_PSTN_VALUE"),
                    //yPositionValue = t.Field<Int32>("SCRN_Y_PSTN_VALUE"),
                    CalculateDate = t.Field<DateTime>("CALDATE"),
                    WarehouseReceiveDateTime = t.Field<DateTime>("WH_RCV_DTTM"),
                    CarrierId = t.Field<string>("MCS_CST_ID"),
                    EmptyCarrierFlag = t.Field<string>("EMPTY_CST_FLAG"),
                    WipState = t.Field<string>("MCS_WIP_STAT"),
                    MaterialId = t.Field<string>("MTRLID"),
                    MLotId = t.Field<string>("MLOTID"),
                    CurrentMLotQty = t.Field<decimal>("MLOTQTY_CUR"),
                    StockedMLotQty = t.Field<decimal>("MLOTQTY_STOCKED"),
                    StockedMlotQtyDateTime = t.Field<DateTime>("MLOTDTTM_STOCKED"),
                    BillOfMaterialRevision = t.Field<string>("BOMREV"),
                    MaterialName = t.Field<string>("MTRLNAME"),
                    MaterialUnit = t.Field<string>("MTRLUNIT")
                }).FirstOrDefault();

            if (query != null)
            {
                DataRow dr = dtPortInfo.NewRow();
                dr["PORT_ID"] = query.PortId;
                dr["PORT_NAME"] = query.PortName;
                dr["PORT_TYPE_CODE"] = query.PortTypeCode;
                dr["INOUT_TYPE_CODE"] = query.InoutTypecode;
                dr["ELTR_TYPE_CODE"] = query.ElectrodeTypeCode;
                dr["EQPTID"] = query.EquipmentCode;
                dr["PORTSEQ"] = query.PortSeq;
                dr["USE_FLAG"] = query.UseFlag;
                //dr["X_PSTN"] = query.xPosition;
                //dr["Y_PSTN"] = query.yPosition;
                //dr["Z_PSTN"] = query.zPosition;
                //dr["SCRN_X_PSTN_VALUE"] = query.xPositionValue;
                //dr["SCRN_Y_PSTN_VALUE"] = query.yPositionValue;
                dr["CALDATE"] = query.CalculateDate;
                dr["WH_RCV_DTTM"] = query.WarehouseReceiveDateTime;
                dr["MCS_CST_ID"] = query.CarrierId;
                dr["EMPTY_CST_FLAG"] = query.EmptyCarrierFlag;
                dr["MTRLID"] = query.MaterialId;
                dr["MLOTID"] = query.MLotId;
                dr["MLOTQTY_CUR"] = query.CurrentMLotQty;
                dr["MLOTQTY_STOCKED"] = query.StockedMLotQty;
                dr["MLOTDTTM_STOCKED"] = query.StockedMlotQtyDateTime;
                dr["BOMREV"] = query.BillOfMaterialRevision;
                dr["MTRLNAME"] = query.MaterialName;
                dr["MTRLUNIT"] = query.MaterialUnit;
                dtPortInfo.Rows.Add(dr);

                Util.GridSetData(dgPortInfo, dtPortInfo, null, true);
            }
            */
        }

        private void UnCheckFoilStockInfo()
        {
            for (int i = 0; i < _dtPortInfo.Rows.Count; i++)
            {
                _dtPortInfo.Rows.RemoveAt(i);
            }

            Util.GridSetData(dgPortInfo, _dtPortInfo, null, true);
            
        }

        private void InitializeCheckBoxFromFoilStockInfo()
        {
            try
            {
                foreach (CheckBox checkBox in Util.FindVisualChildren<CheckBox>(gdFoilStock))
                {
                    if (checkBox.IsChecked != null && (bool)checkBox.IsChecked)
                        checkBox.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void InitializeTextBlockFromFoilStockInfo()
        {
            foreach (TextBlock textBlock in Util.FindVisualChildren<TextBlock>(gdFoilStock))
            {
                if (textBlock != null)
                {
                    if (textBlock.Name.StartsWith("txtCST") || textBlock.Name.StartsWith("txtMLOT") || textBlock.Name.StartsWith("txtInoutType") || textBlock.Name.StartsWith("txtPortStateCode"))
                    {
                        if (!string.IsNullOrEmpty(textBlock.Text))
                            textBlock.Text = string.Empty;
                    }
                }
            }
        }

        private bool ValidationFoilSupplyChoice()
        {
            if (_util.GetDataGridFirstRowIndexByCheck(dgMaterialSupply, "CHK", true) < 0 || !CommonVerify.HasDataGridRow(dgMaterialSupply))
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            string materialSupplyRequestStateCode = _util.GetDataGridFirstRowBycheck(dgMaterialSupply, "CHK",true).Field<string>("MTRL_SPLY_REQ_STAT_CODE").GetString();

            if (!string.Equals("CREATED", materialSupplyRequestStateCode))
            {
                //공급요청 상태에서만 선택 가능합니다.
                Util.MessageValidation("SFU6015");
                return false;
            }

            return true;
        }

        private bool ValidationCarrierMoveOrder()
        {
            if (dgPortInfo.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (!DataTableConverter.GetValue(dgPortInfo.Rows[0].DataItem, "PORT_TYPE_CODE").GetString().Equals("BFR_FOIL_WH_OUT") &&
                !DataTableConverter.GetValue(dgPortInfo.Rows[0].DataItem, "PORT_TYPE_CODE").GetString().Equals("BFR_FOIL_WH_IN") &&
                !DataTableConverter.GetValue(dgPortInfo.Rows[0].DataItem, "PORT_TYPE_CODE").GetString().Equals("BFR_FOIL_COT_OUT") &&
                !DataTableConverter.GetValue(dgPortInfo.Rows[0].DataItem, "PORT_TYPE_CODE").GetString().Equals("BFR_FOIL_COT_IN") )
            {
                Util.MessageValidation("SFU6009");
                return false;
            }

            return true;
        }

        private bool ValidationFoilSupplyRequest()
        {
            return true;
        }

        private bool ValidationMaterialSupply()
        {
            return true;
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
