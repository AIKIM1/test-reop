using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_092.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_092 : UserControl, IWorkArea
    {
        #region Initialize
        private System.Windows.Threading.DispatcherTimer timer;
        private Dictionary<string, bool> eqptCollect;
        private Dictionary<string, string> eqptNameCollect;
        private List<C1DataGrid> eqptGrids;
        private List<TextBlock> eqptPanels;



        public COM001_092()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {

            eqptCollect = new Dictionary<string, bool>();
            eqptNameCollect = new Dictionary<string, string>();
            eqptGrids = new List<C1DataGrid>();

            eqptGrids.Add(dgSubPanel1);
            eqptGrids.Add(dgSubPanel2);
            eqptGrids.Add(dgSubPanel3);
            eqptGrids.Add(dgSubPanel4);
            eqptGrids.Add(dgSubPanel5);
            eqptGrids.Add(dgSubPanel6);

            eqptPanels = new List<TextBlock>();

            eqptPanels.Add(txtPanel1);
            eqptPanels.Add(txtPanel2);
            eqptPanels.Add(txtPanel3);
            eqptPanels.Add(txtPanel4);
            eqptPanels.Add(txtPanel5);
            eqptPanels.Add(txtPanel6);

            Util.GridSetData(dgProcess, GetCommonCode("PROC_STOCK_CODE"), FrameOperation, true);
            InitCombo();
            
            //cboNotchingLine.ItemsSource = DataTableConverter.Convert(GetCommonCode("NTC_STOCK_AREA"));

            // Timer 설정
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(txtCycle.Value);
            timer.Tick += timer_Start;
            timer.Start();
        }
        #endregion

        #region Init Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSpecialLot);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer.Tick -= timer_Start;
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape && SearchGrid.Visibility != Visibility.Visible)
            {
                SearchGrid.Visibility = Visibility.Visible;

                foreach (C1DataGrid grid in eqptGrids.ToArray())
                    grid.Columns["ABNORM_FLAG"].Visibility = Visibility.Collapsed;

                this.Refresh();
            }
        }
        #endregion
        #region Biz Call
        private DataTable GetCommonCode(string sCodeType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FOR_STOCK_OUT_V01", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetStockList(string sGroupCode, string sProcCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQGRID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["EQGRID"] = sGroupCode;
                dr["PROCID"] = sProcCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_IN_EQPT_V01", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetNotchingStockList(string sElecType = "")
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ELEC_TYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["ELEC_TYPE"] = sElecType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_IN_NTC_EQPT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private void GetStockLotFifoInfo(C1DataGrid dataGrid, string procID, string eqptID, string eqgrID)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("EQGRID", typeof(string));
            IndataTable.Columns.Add("RACK_FLAG", typeof(string));
            IndataTable.Columns.Add("ORDER_TYPE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            Indata["PROCID"] = procID;
            Indata["EQPTID"] = eqptID;
            Indata["EQGRID"] = eqgrID;
            
            if (chkRack.IsChecked == false)
                Indata["RACK_FLAG"] = "Y";

            Indata["ORDER_TYPE"] = chkOrder.IsChecked == true ? "Y" : "N";

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_OUT_FIFO_VLD_DATE", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                        throw searchException;

                    Util.GridSetData(dataGrid, result, FrameOperation, false);
                    dataGrid.Refresh(false);
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }
        #endregion
        #region User Function
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilterData = { "ELEC_TYPE" };
            _combo.SetCombo(cboNotchingLine, CommonCombo.ComboStatus.ALL, sFilter: sFilterData, sCase: "COMMCODE");
            cboNotchingLine.SelectedIndex = 0;
        }

        private DataRow[] gridGetChecked(C1DataGrid dg, string sCheckColName)
        {
            DataRow[] dr = null;
            try
            {
                DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
                if (dtChk.Columns.Contains(sCheckColName))
                {
                    dr = dtChk.Select(sCheckColName + " = True");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        private string GetNextEqptInfo()
        {
            if ( eqptCollect != null && eqptCollect.Count > 0)
            {
                foreach (KeyValuePair<string, bool> eqptInfo in eqptCollect)
                    if (eqptInfo.Value == false)
                        return eqptInfo.Key;

                InitEqptCollect();
            }
            return "";
        }

        private void InitEqptCollect()
        {
            List<string> keys = new List<string>(eqptCollect.Keys);
            foreach (string key in keys)
                eqptCollect[key] = false;

            /*
            if (eqptCollect != null && eqptCollect.Count > 0)
                foreach (string key in eqptCollect.Keys)
                    eqptCollect[key] = false;
             */
        }

        private void InitEqptAddCollect()
        {
            List<string> keys = new List<string>(eqptCollect.Keys);
            if (eqptCollect[keys[0]] == false)
                foreach (string key in keys)
                    if (eqptCollect[key] == true)
                        eqptCollect[key] = false;
        }

        private void InitControls()
        {
            for (int i = 0; i < eqptGrids.Count; i++)
            {
                eqptPanels[i].Text = string.Empty;
                eqptPanels[i].Tag = string.Empty;
                Util.gridClear(eqptGrids[i]);
            }
        }

        private void Refresh()
        {
            if (eqptCollect != null && eqptCollect.Count > 0 )
            {
                for (int i = 0; i < eqptGrids.Count; i++)
                {
                    string sEqptID = Util.NVC(eqptPanels[i].Tag);

                    if (!string.IsNullOrEmpty(sEqptID))
                    {
                        string sGroupID = Util.NVC(DataTableConverter.GetValue(dgProcessDetail.Rows[0].DataItem, "EQGRID"));

                        if (string.Equals(sGroupID, "NTC"))
                        { 
                            GetStockLotFifoInfo(eqptGrids[i], Util.NVC(DataTableConverter.GetValue(dgProcessDetail.Rows[0].DataItem, "PROCID")), sEqptID, sGroupID);
                        }
                        else
                        {
                            DataRow[] rows = DataTableConverter.Convert(dgProcessDetail.ItemsSource).Select("EQPTID = '" + sEqptID + "'");
                            if (rows != null && rows.Length > 0)
                                GetStockLotFifoInfo(eqptGrids[i], Util.NVC(rows[0]["PROCID"]), sEqptID, Util.NVC(rows[0]["EQGRID"]));
                        }
                    }
                }
            }
        }
        #endregion
        #region Action Event
        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            SearchGrid.Visibility = Visibility.Collapsed;

            foreach (C1DataGrid grid in eqptGrids.ToArray())
                grid.Columns["ABNORM_FLAG"].Visibility = Visibility.Collapsed;

            this.Refresh();
        }

        private void btnAbnormal_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.COM001.COM001_092_HOLD wndPopup = new LGC.GMES.MES.COM001.COM001_092_HOLD();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void chkProcess_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = checkBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (cellPresenter != null)
                {
                    string sLineType = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "ATTRIBUTE1"));

                    if (string.Equals(sLineType, "NTC"))
                    {
                        // NOTCHING
                        cboNotchingLine.Visibility = Visibility.Visible;
                        cboNotchingLine.SelectedIndex = 0;
                        eqptCollect.Clear();
                        eqptNameCollect.Clear();
                        InitControls();

                        DataTable dt = ((DataView)cellPresenter.DataGrid.ItemsSource).Table;
                        foreach (DataRow row in dt.Rows)
                            if (!string.Equals(row["ATTRIBUTE1"], "NTC"))
                                row["CHK"] = false;

                        Util.GridSetData(dgProcessDetail, GetNotchingStockList(), FrameOperation, true);
                    }
                    else if (string.Equals(sLineType, "PNC"))
                    {
                        // PANCAKE
                        cboNotchingLine.Visibility = Visibility.Collapsed;
                        cboNotchingLine.SelectedIndex = 0;
                        eqptCollect.Clear();
                        eqptNameCollect.Clear();
                        InitControls();

                        DataTable dt = ((DataView)cellPresenter.DataGrid.ItemsSource).Table;
                        foreach (DataRow row in dt.Rows)
                            if (!string.Equals(row["ATTRIBUTE1"], "PNC"))
                                row["CHK"] = false;

                        DataTable pancakeDt = new DataTable();
                        pancakeDt.Columns.Add("CHK");
                        pancakeDt.Columns.Add("EQPTID");
                        pancakeDt.Columns.Add("EQPTNAME");
                        pancakeDt.Columns.Add("EQGRID");
                        pancakeDt.Columns.Add("PROCID");
                        pancakeDt.Columns.Add("SEQNO");
                        pancakeDt.Rows.Add("False", ObjectDic.Instance.GetObjectName("전극창고(Pancake)"), ObjectDic.Instance.GetObjectName("전극창고(Pancake)"), sLineType, "E7000", 0);

                        Util.GridSetData(dgProcessDetail, pancakeDt, FrameOperation, true);
                    }
                    else
                    {
                        // ELEC
                        cboNotchingLine.Visibility = Visibility.Collapsed;
                        cboNotchingLine.SelectedIndex = 0;
                        DataTable copyDt = DataTableConverter.Convert(dgProcessDetail.ItemsSource);
                        DataRow[] chkRows = gridGetChecked(cellPresenter.DataGrid, "CHK");
                        eqptCollect.Clear();
                        eqptNameCollect.Clear();

                        if (copyDt == null && copyDt.Rows.Count == 0)
                            InitControls();

                        string sGroupName = string.Empty;
                        string sProcCode = string.Empty;
                        DataTable dt = ((DataView)cellPresenter.DataGrid.ItemsSource).Table;
                        foreach (DataRow row in dt.Rows)
                            if (string.Equals(row["ATTRIBUTE1"], "NTC") || string.Equals(row["ATTRIBUTE1"], "PNC"))
                                row["CHK"] = false;

                        foreach (DataRow row in dt.Rows)
                        {
                            if (Convert.ToBoolean(row["CHK"]) == true)
                            {
                                sGroupName += Util.NVC(row["ATTRIBUTE1"]) + ",";
                                sProcCode += Util.NVC(row["ATTRIBUTE2"]) + ",";
                            }
                        }

                        if (string.IsNullOrEmpty(sGroupName))
                            sGroupName = sGroupName.Substring(0, sGroupName.Length - 1);
                        if (string.IsNullOrEmpty(sProcCode))
                            sProcCode = sProcCode.Substring(0, sProcCode.Length - 1);

                        Util.GridSetData(dgProcessDetail, GetStockList(sGroupName, sProcCode), FrameOperation, true);

                        if (copyDt != null && copyDt.Rows.Count > 0)
                        {
                            DataTable processDt = ((DataView)dgProcessDetail.ItemsSource).Table;
                            foreach (DataRow row in copyDt.Rows)
                            {
                                if (Convert.ToBoolean(row["CHK"]) == false)
                                    continue;

                                foreach (DataRow inRow in processDt.Rows)
                                {
                                    if (string.Equals(row["EQPTID"], inRow["EQPTID"]))
                                    {
                                        inRow["CHK"] = true;
                                        break;
                                    }
                                }
                            }
                            copyDt.Clear();
                        }
                    }
                }
            }

        }

        private void chkProcess_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = checkBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (cellPresenter != null)
                {
                    eqptCollect.Clear();
                    eqptNameCollect.Clear();
                    InitControls();
                    Util.gridClear(dgProcessDetail);
                    string sLineType = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "ATTRIBUTE1"));

                    if (string.Equals(sLineType, "NTC"))
                    {
                        // NOTCHING
                        cboNotchingLine.Visibility = Visibility.Collapsed;
                        cboNotchingLine.SelectedIndex = 0;
                    }
                    else if (string.Equals(sLineType, "PNC")) { }
                    else
                    {
                        // ELEC
                        DataTable dt = ((DataView)cellPresenter.DataGrid.ItemsSource).Table;
                        foreach (DataRow row in dt.Rows)
                            if (!(string.Equals(row["ATTRIBUTE1"], "NTC") || string.Equals(row["ATTRIBUTE1"], "PNC")) && Convert.ToBoolean(row["CHK"]) == true)
                                row["CHK"] = false;
                    }
                }
            }
        }

        private void cboNotchingLine_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            if (e != null && !string.IsNullOrEmpty(Util.NVC(e.NewValue)))
            {
                eqptCollect.Clear();
                eqptNameCollect.Clear();
                InitControls();
                Util.GridSetData(dgProcessDetail, GetNotchingStockList(Util.NVC(e.NewValue)), FrameOperation, true);
            }
        }

        private void chkProcessDetail_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = checkBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (cellPresenter != null)
                {
                    string sEqptID = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTID"));
                    string sGroupID = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQGRID"));

                    if ( string.Equals(sEqptID, "ALL"))
                    {
                        DataTable processDt = ((DataView)dgProcessDetail.ItemsSource).Table;
                        foreach (DataRow row in processDt.Rows)
                            if (!string.Equals(sEqptID, row["EQPTID"]))
                                if (Convert.ToBoolean(row["CHK"]) == false)
                                    row["CHK"] = true;

                        return;
                    }

                    if (string.Equals(sGroupID, "NTC"))
                    {
                        string[] eqptIds = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTIDS")).Split(',');
                        string[] eqptNames = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTNAMES")).Split(',');

                        for (int i = 0; i < eqptIds.Length; i++)
                        {
                            if (eqptCollect.Count < eqptGrids.Count)
                            {
                                eqptCollect.Add(eqptIds[i], true);
                                eqptNameCollect.Add(eqptIds[i], eqptNames[i]);
                                eqptPanels[eqptCollect.Count - 1].Text = eqptNames[i];
                                eqptPanels[eqptCollect.Count - 1].Tag = eqptIds[i];
                                GetStockLotFifoInfo(eqptGrids[eqptCollect.Count - 1], Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "PROCID")), eqptIds[i],
                                    Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQGRID")));
                            }
                            else
                            {
                                eqptCollect.Add(eqptIds[i], false);
                                eqptNameCollect.Add(eqptIds[i], eqptNames[i]);
                            }
                        }
                    }
                    else
                    {
                        if (eqptCollect.Count < eqptGrids.Count)
                        {
                            eqptCollect.Add(sEqptID, true);
                            eqptNameCollect.Add(sEqptID, Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTNAME")));
                            eqptPanels[eqptCollect.Count - 1].Text = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTNAME"));
                            eqptPanels[eqptCollect.Count - 1].Tag = sEqptID;
                            GetStockLotFifoInfo(eqptGrids[eqptCollect.Count - 1], Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "PROCID")), sEqptID,
                                Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQGRID")));
                        }
                        else
                        {
                            eqptCollect.Add(sEqptID, false);
                            eqptNameCollect.Add(sEqptID, Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTNAME")));
                        }
                    }
                }
            }
        }

        private void chkProcessDetail_UnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = checkBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (cellPresenter != null)
                {
                    string sEqptID = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTID"));
                    string sGroupID = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQGRID"));

                    if (string.Equals(sEqptID, "ALL"))
                    {
                        DataTable processDt = ((DataView)dgProcessDetail.ItemsSource).Table;
                        foreach (DataRow row in processDt.Rows)
                            if (!string.Equals(sEqptID, row["EQPTID"]))
                                if (Convert.ToBoolean(row["CHK"]) == true)
                                    row["CHK"] = false;

                        InitControls();
                        return;
                    }

                    if (string.Equals(sGroupID, "NTC"))
                    {
                        string[] eqptIds = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "EQPTIDS")).Split(',');

                        foreach(string eqptId in eqptIds)
                        {
                            eqptCollect.Remove(eqptId);
                            eqptNameCollect.Remove(eqptId);
                        }
                    }
                    else if(string.Equals(sGroupID, "PNC"))
                    {
                        eqptCollect.Remove(sEqptID);
                        eqptNameCollect.Remove(sEqptID);
                        InitControls();
                    }
                    else
                    {
                        eqptCollect.Remove(sEqptID);
                        eqptNameCollect.Remove(sEqptID);
                    }
                }
            }
        }

        private void chkAbnorm_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = checkBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (cellPresenter != null)
                {
                    string sLotID = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "LOTID"));
                    bool IsCheck = Convert.ToBoolean(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "ABNORM_FLAG"));

                    if (IsCheck == true)
                    {
                        LGC.GMES.MES.COM001.COM001_092_ABNORMAL wndPopup = new LGC.GMES.MES.COM001.COM001_092_ABNORMAL();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = sLotID;
                            Parameters[1] = cellPresenter;

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(OnCloseAbnormal);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                }
            }
        }

        private void OnCloseAbnormal(object sender, EventArgs e)
        {
            LGC.GMES.MES.COM001.COM001_092_ABNORMAL window = sender as LGC.GMES.MES.COM001.COM001_092_ABNORMAL;
            DataGridCellPresenter cellPresenter = window.GetCellPresenter;
            if (cellPresenter != null)
            {
                /*
                if (window.DialogResult == MessageBoxResult.OK)
                    DataTableConverter.SetValue(cellPresenter.Row.DataItem, "ABNORM_FLAG", true);
                else if (window.DialogResult == MessageBoxResult.Cancel)
                    DataTableConverter.SetValue(cellPresenter.Row.DataItem, "ABNORM_FLAG", false);
                else
                    DataTableConverter.SetValue(cellPresenter.Row.DataItem, "ABNORM_FLAG", !Convert.ToBoolean(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "ABNORM_FLAG")));
                */
                this.Refresh();
            }
        }

        private void chkAbnorm_Checked(object sender, RoutedEventArgs e)
        {
            /*
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = checkBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (cellPresenter != null)
                {
                    string sLotID = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "LOTID"));

                    LGC.GMES.MES.COM001.COM001_092_ABNORMAL wndPopup = new LGC.GMES.MES.COM001.COM001_092_ABNORMAL();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = sLotID;

                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
            }
            */
        }

        private void chkAbnorm_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = checkBox.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                if (cellPresenter != null)
                    DataTableConverter.SetValue(cellPresenter.Row.DataItem, "ABNORM_FLAG", true);
            }
        }

        private void dgSubPanel_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BACKCOLOR"), "ORANGE"))
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                            else if (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BACKCOLOR"), "GREENYELLOW"))
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGreen);
                            else if (string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BACKCOLOR"), "SKYBLUE"))
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightSkyBlue);
                            else
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightPink);    // 유효일자 초과 분
                        }
                    }
                }));
            }
        }

        private void btnCycle_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(txtCycle.Value);

            Util.MessageInfoAutoClosing("SFU1166");    // 변경 되었습니다.
        }

        private void chkRack_Checked(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }

        private void chkOrder_Checked(object sender, RoutedEventArgs e)
        {
            this.Refresh();
        }
        #endregion
        #region Timer
        private void timer_Start(object sender, EventArgs e)
        {
            InitControls();

            if (eqptCollect != null && eqptCollect.Count > 0)
            {
                InitEqptAddCollect();

                for (int i = 0; i < eqptGrids.Count; i++)
                {
                    string sEqptID = GetNextEqptInfo();

                    if (!string.IsNullOrEmpty(sEqptID))
                    {
                        eqptCollect[sEqptID] = true;   // 현재 설정처리

                        string sGroupID = Util.NVC(DataTableConverter.GetValue(dgProcessDetail.Rows[0].DataItem, "EQGRID"));

                        if (string.Equals(sGroupID, "NTC"))
                        {
                            eqptPanels[i].Text = eqptNameCollect[sEqptID];
                            eqptPanels[i].Tag = sEqptID;

                            GetStockLotFifoInfo(eqptGrids[i], Util.NVC(DataTableConverter.GetValue(dgProcessDetail.Rows[0].DataItem, "PROCID")), sEqptID, sGroupID);
                        }
                        else
                        {
                            DataRow[] rows = DataTableConverter.Convert(dgProcessDetail.ItemsSource).Select("EQPTID = '" + sEqptID + "'");
                            if (rows != null && rows.Length > 0)
                            {
                                eqptPanels[i].Text = eqptNameCollect[sEqptID];
                                eqptPanels[i].Tag = sEqptID;

                                GetStockLotFifoInfo(eqptGrids[i], Util.NVC(rows[0]["PROCID"]), sEqptID, Util.NVC(rows[0]["EQGRID"]));
                            }
                        }
                    }
                    else
                    {
                        if (eqptGrids.Count >= eqptCollect.Count && i > 0)
                        {
                            break;
                        }
                        else
                        //else if ((eqptGrids.Count - (i + 1)) < eqptCollect.Count)
                        {
                            i--;
                            continue;
                        }
                    }
                }
            }
        }
        #endregion

        private void chkLayout_Checked(object sender, RoutedEventArgs e)
        {

            dgPanel5.Visibility = Visibility.Visible;
            dgPanel6.Visibility = Visibility.Visible;
            //ContentGrid.RowDefinitions[5].Height = new GridLength(1, GridUnitType.Star);
            ContentGrid.ColumnDefinitions[3].Width = new GridLength(8);
            ContentGrid.ColumnDefinitions[4].Width = new GridLength(1, GridUnitType.Star);

            eqptGrids[2] = dgSubPanel5;
            eqptGrids[3] = dgSubPanel3;
            eqptGrids[4] = dgSubPanel4;

            eqptPanels[2] = txtPanel5;
            eqptPanels[3] = txtPanel3;
            eqptPanels[4] = txtPanel4;


        }

        private void chkLayout_UnChecked(object sender, RoutedEventArgs e)
        {


            dgPanel5.Visibility = Visibility.Collapsed;
            dgPanel6.Visibility = Visibility.Collapsed;
            //ContentGrid.RowDefinitions[5].Height = new GridLength(0);
            ContentGrid.ColumnDefinitions[3].Width = new GridLength(0);
            ContentGrid.ColumnDefinitions[4].Width = new GridLength(0);

            eqptGrids[2] = dgSubPanel3;
            eqptGrids[3] = dgSubPanel4;
            eqptGrids[4] = dgSubPanel5;

            eqptPanels[2] = txtPanel3;
            eqptPanels[3] = txtPanel4;
            eqptPanels[4] = txtPanel5;
        }
    }
}
