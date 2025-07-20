/*************************************************************************************
 Created Date : 2020.02.13
      Creator : 신광희
   Decription : 물류설비 Trouble 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.02.13  신광희 차장 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_041.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_041 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        private DataTable _dtBaseTable;

        private string _selectedEquipmentCode;


        //private const string _bizIp = "10.32.169.224";
        //private const string _bizPort = "7865";
        //private const string _bizIndex = "0";
        //private string[] _bizInfo = { _bizIp, _bizPort, _bizIndex };

        public MCS001_041()
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
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;
            ClearControl();

            ShowLoadingIndicator();
            SelecTroubleEquipmentList((table, ex) =>
            {
                if (ex != null)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
                else
                {
                    SelectAlarmEquipmentList();

                    //SelectAlarmUnitEquipmentList((table1, ex1) =>
                    //{
                    //    Util.GridSetData(dgTroubleEquipment, table, null, true);

                    //    if (ex1 != null)
                    //    {
                    //        HiddenLoadingIndicator();
                    //        Util.MessageException(ex1);
                    //    }
                    //    else
                    //    {
                    //        Util.GridSetData(dgAlarmUnitEquipment, table1, null, true);
                    //        SelectAlarmEquipmentList();
                    //    }

                    //});
                }
            });
        }

        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;

            if (string.IsNullOrEmpty(msbEquipmentType.SelectedItemsToString))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("EQPT_TP"));
                return;
            }

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

        private void dgTroubleEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgTroubleEquipment_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgTroubleEquipment_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

                // 공Carrier 컬럼 선택 시
                if (cell.Column.Name.Equals("EQPTNAME"))
                {
                    _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    SelectAlarmUnitEquipmentList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAlarmEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Convert.ToBoolean(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DISPLAY")) == true)
                    //{
                    //    e.Cell.Row.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    e.Cell.Row.Visibility = Visibility.Collapsed;
                    //}

                    if (Convert.ToString(e.Cell.Column.Name) == "ALARM_COUNT")
                    {

                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "ALARM_COUNT").GetInt() > 1 && Convert.ToBoolean(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VISIBILITY")) == true)
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
            }
        }

        private void dgAlarmEquipment_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (Convert.ToBoolean(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DISPLAY")) == true)
                    //{
                    //    e.Cell.Row.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    e.Cell.Row.Visibility = Visibility.Collapsed;
                    //}

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
            
        }

        private void dgAlarmEquipment_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = dgAlarmEquipment;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);
                if (cell == null) return;

                // 선택한 셀의 Row 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                if (cell.Column.Name.Equals("ALARM_COUNT"))
                {
                    if (DataTableConverter.GetValue(drv, "ALARM_COUNT").GetInt() > 1 && Convert.ToBoolean(DataTableConverter.GetValue(drv, "VISIBILITY")) == true)
                    {
                        //string groupEquipmentCode = DataTableConverter.GetValue(drv, "GROUP_EQPTID").GetString();
                        string computerintegratedmanufacturingType = DataTableConverter.GetValue(drv, "TSC_TP").GetString();
                        string computerintegratedmanufacturingCode = DataTableConverter.GetValue(drv, "TSCID").GetString();

                        var query = (from t in _dtBaseTable.AsEnumerable()
                            where t.Field<string>("TSCID") == computerintegratedmanufacturingCode
                                  && t.Field<Int32>("VISIBILITY") != 1
                                  && t.Field<Boolean>("DISPLAY") == false
                            select t).ToList();

                        DataTable updateTable = ((DataView)dg.ItemsSource).Table;

                        if (query.Any())
                        {
                            foreach (var item in query)
                            {
                                DataRow newRow = updateTable.NewRow();
                                newRow["TSC_TP"] = item["TSC_TP"];
                                newRow["TSCID"] = item["TSCID"];
                                newRow["TSC_NAME"] = item["TSC_NAME"];
                                newRow["ALARM_COUNT"] = item["ALARM_COUNT"];
                                newRow["ALARMID"] = item["ALARMID"];
                                newRow["ALARM_NAME"] = item["ALARM_NAME"];
                                newRow["ALARM_TX"] = item["ALARM_TX"];
                                newRow["ALARM_SET_DTTM"] = item["ALARM_SET_DTTM"];
                                newRow["MACHINE_NAME"] = item["MACHINE_NAME"];
                                newRow["UNIT_NAME"] = item["UNIT_NAME"];
                                newRow["ASSY_NAME"] = item["ASSY_NAME"];
                                newRow["VISIBILITY"] = item["VISIBILITY"];
                                newRow["DISPLAY"] = true;
                                updateTable.Rows.Add(newRow);
                            }

                            updateTable = (from t in updateTable.AsEnumerable()
                                orderby t.Field<string>("TSC_TP") ascending, t.Field<string>("TSCID") ascending//, t.Field<Int64>("GROUP_SEQ") ascending 
                                select t).CopyToDataTable();
                            Util.GridSetData(dgAlarmEquipment, updateTable.Copy(), null, true);
                            
                            
                            _dtBaseTable.AsEnumerable().Where(r => r.Field<string>("TSCID") == computerintegratedmanufacturingCode
                                                                   && r.Field<Int32>("VISIBILITY") != 1
                                                                   && r.Field<Boolean>("DISPLAY") == false
                                                                   ).ToList<DataRow>().ForEach(y => y["DISPLAY"] = true);

                            //rowIdx = rowIdx + Convert.ToInt16(DataTableConverter.GetValue(drv, "ALARM_COUNT").GetInt()) - 1;
                        }
                        else
                        {

                            updateTable.AsEnumerable().Where(r => r.Field<string>("TSCID") == computerintegratedmanufacturingCode
                                                                   && r.Field<Int32>("VISIBILITY") != 1
                                                                   && r.Field<Boolean>("DISPLAY") == true
                            ).ToList<DataRow>().ForEach(row => row.Delete());
                            updateTable.AcceptChanges();
                            Util.GridSetData(dgAlarmEquipment, updateTable.Copy(), null, true);
                            

                            _dtBaseTable.AsEnumerable().Where(r => r.Field<string>("TSCID") == computerintegratedmanufacturingCode
                                                                   && r.Field<Int32>("VISIBILITY") != 1
                                                                   && r.Field<Boolean>("DISPLAY") == true
                                                                   ).ToList<DataRow>().ForEach(y => y["DISPLAY"] = false);
                        }

                        _dtBaseTable.AcceptChanges();

                        if (dg.GetCell(rowIdx, dg.Columns["ALARM_COUNT"].Index).Presenter != null)
                        {
                            dg.GetCell(rowIdx, dg.Columns["ALARM_COUNT"].Index).Presenter.IsSelected = true;
                        }

                        dg.ScrollIntoView(rowIdx, dg.Columns["ALARM_COUNT"].Index);
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

        private void SelecTroubleEquipmentList(Action<DataTable, Exception> actionCompleted = null)
        {
            const string bizRuleName = "DA_MCS_SEL_EIO_TROUBLE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("BLDG_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BLDG_CODE"] = cboArea.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    actionCompleted?.Invoke(bizResult, bizException);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectAlarmUnitEquipmentList()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MCS_ALARM";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("BLDG_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BLDG_CODE"] = cboArea.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgAlarmUnitEquipment, bizResult, null, true);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectAlarmEquipmentList()
        {
            _dtBaseTable.Clear();

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MCS_ALARM_TREE";
            const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MCS_ALARM_GROUPBY_EQPTID";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPT_TP", typeof(string));
                inTable.Columns.Add("BLDG_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BLDG_CODE"] = cboArea.SelectedValue;
                dr["EQPT_TP"] = msbEquipmentType.SelectedItemsToString;
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    bizResult.Columns.Add("DISPLAY", typeof(bool));

                    foreach (DataRow row in bizResult.Rows)
                    {
                        if (Convert.ToBoolean(row["VISIBILITY"]) == true)
                        {
                            row["DISPLAY"] = true;
                        }
                        else
                        {
                            row["DISPLAY"] = false;
                        }
                    }

                    _dtBaseTable = bizResult.Copy();

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        bizResult = bizResult.Select("VISIBILITY = '1'").CopyToDataTable();
                    }
                    Util.GridSetData(dgAlarmEquipment, bizResult, null, true);
                    
                    //sw.Stop();
                    //ControlsLibrary.MessageBox.Show(sw.Elapsed.ToString());
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
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

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            if (string.IsNullOrEmpty(msbEquipmentType.SelectedItemsToString))
            {
                //Util.MessageValidation("설비유형을 선택해 주세요.");
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("EQPT_TP"));
                return;
            }


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


        private void InitializeControl()
        {
            _dtBaseTable = Util.MakeDataTable(dgAlarmEquipment, true);
            InitializeCombo();
        }

        private void InitializeCombo()
        {
            // 동 콤보박스
            SetAreaCombo(cboArea);
            SetEquipmentType(msbEquipmentType);
        }

        private void ClearControl()
        {
            Util.gridClear(dgAlarmEquipment);
            Util.gridClear(dgAlarmUnitEquipment);
            Util.gridClear(dgTroubleEquipment);
        }

        private bool ValidationSearch()
        {
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue == null)
            {
                Util.MessageValidation("SFU3206");
                return false;
            }

            if (string.IsNullOrEmpty(msbEquipmentType.SelectedItemsToString))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("EQPT_TP"));
                return false;
            }


            return true;
        }

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_BLDG_CODE_CBO", "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (LoginInfo.CFG_AREA_ID == ((DataRowView)cbo.Items[i]).Row.ItemArray[3].ToString())
                {
                    cbo.SelectedIndex = i;
                    return;
                }
            }
        }



        private void SetEquipmentType(MultiSelectionBox msb)
        {
            try
            {
                const string bizRuleName = "DA_INF_MCS_DB_SEL_EQPT_TP_CBO";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
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
