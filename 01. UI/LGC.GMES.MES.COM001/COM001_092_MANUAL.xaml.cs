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
    /// COM001_092_NORMAL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_092_MANUAL : UserControl, IWorkArea
    {
        #region Initialize
        private System.Windows.Threading.DispatcherTimer timer;
        //private Dictionary<string, bool> prodCollect;
        private Dictionary<int, List<string>> prodInfoCollect;
        private List<C1DataGrid> prodGrids;
        private List<TextBlock> prodPanels;     // 각종 정보
        private List<TextBlock> prodMovePanels; // 가대/컨베이어 인수 대기품
        private string procID = string.Empty;

        public COM001_092_MANUAL()
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
            prodInfoCollect = new Dictionary<int, List<string>>();

            prodGrids = new List<C1DataGrid>();
            prodGrids.Add(dgSubPanel1);
            prodGrids.Add(dgSubPanel2);
            prodGrids.Add(dgSubPanel3);
            prodGrids.Add(dgSubPanel4);
            prodGrids.Add(dgSubPanel5);
            prodGrids.Add(dgSubPanel6);

            prodPanels = new List<TextBlock>();
            prodPanels.Add(txtPanel1);
            prodPanels.Add(txtPanel2);
            prodPanels.Add(txtPanel3);
            prodPanels.Add(txtPanel4);
            prodPanels.Add(txtPanel5);
            prodPanels.Add(txtPanel6);

            prodMovePanels = new List<TextBlock>();
            prodMovePanels.Add(txtMovePanel1);
            prodMovePanels.Add(txtMovePanel2);
            prodMovePanels.Add(txtMovePanel3);
            prodMovePanels.Add(txtMovePanel4);
            prodMovePanels.Add(txtMovePanel5);
            prodMovePanels.Add(txtMovePanel6);

            Util.GridSetData(dgProcess, GetCommonCode("PROC_STOCK_CODE"), FrameOperation, true);

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

                foreach (C1DataGrid grid in prodGrids.ToArray())
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

        private void GetStockLotFifoInfo(C1DataGrid dataGrid, string prodID, string laneQty, string isCutConfig, string isOrder)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("EQSGID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("PRODID", typeof(string));
            IndataTable.Columns.Add("LANEQTY", typeof(string));
            IndataTable.Columns.Add("RACK_FLAG", typeof(string));
            IndataTable.Columns.Add("ORDER_TYPE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            Indata["PROCID"] = procID;
            Indata["PRODID"] = prodID;

            if (!string.IsNullOrEmpty(laneQty))
                Indata["LANEQTY"] = laneQty;
            if (chkRack.IsChecked == false)
                Indata["RACK_FLAG"] = "Y";

            Indata["ORDER_TYPE"] = isOrder;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_OUT_FIFO_VLD_DATE_MANUAL", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
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

        private void InitProdCollect()
        {
            List<int> keys = new List<int>(prodInfoCollect.Keys);
            foreach (int key in keys)
                prodInfoCollect.Remove(key);
        }

        private void InitProdCollect(int key)
        {
            prodInfoCollect.Remove(key);
        }

        private void InitControls()
        {
            for (int i = 0; i < prodGrids.Count; i++)
            {
                prodPanels[i].Text = string.Empty;
                prodMovePanels[i].Text = string.Empty;
                Util.gridClear(prodGrids[i]);
            }
        }

        private void Refresh()
        {
            if (prodInfoCollect != null && prodInfoCollect.Count > 0)
            {
                for (int i = 0; i < prodGrids.Count; i++)
                {
                    if (prodInfoCollect.ContainsKey(i))
                    {
                        List<string> array = prodInfoCollect[i];
                        if (array != null && array.Count > 0)
                            GetStockLotFifoInfo(prodGrids[i], array[0], array[1], array[2], array[3]);
                    }
                }
            }
        }
        #endregion
        #region Action Event
        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            SearchGrid.Visibility = Visibility.Collapsed;

            foreach (C1DataGrid grid in prodGrids.ToArray())
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
                    // ELEC
                    string sAttribute = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "ATTRIBUTE1"));

                    prodInfoCollect.Clear();
                    procID = string.Empty;

                    DataTable dt = ((DataView)cellPresenter.DataGrid.ItemsSource).Table;
                    foreach (DataRow row in dt.Rows)
                        if (!string.Equals(row["ATTRIBUTE1"], sAttribute))
                            row["CHK"] = false;

                    procID = Util.NVC(DataTableConverter.GetValue(cellPresenter.Row.DataItem, "ATTRIBUTE2"));
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
                    prodInfoCollect.Clear();
                    InitControls();
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

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            if ( string.IsNullOrEmpty(procID))
            {
                Util.MessageValidation("SFU3207");  //공정을 선택해주세요
                return;
            }

            Button button = sender as Button;
            if (button != null)
            {
                LGC.GMES.MES.COM001.COM001_092_PROD_SELECT wndPopup = new LGC.GMES.MES.COM001.COM001_092_PROD_SELECT();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = button.Tag;     // 현재 Grid 번호
                    Parameters[1] = procID;         // 공정

                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(OnCloseSetting);
                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
        }

        private void OnCloseSetting(object sender, EventArgs e)
        {
            LGC.GMES.MES.COM001.COM001_092_PROD_SELECT window = sender as LGC.GMES.MES.COM001.COM001_092_PROD_SELECT;
            if ( window.DialogResult == MessageBoxResult.OK)
            {
                // Remove
                InitProdCollect(window._GetSettingGrid);

                // Add
                List<string> array = new List<string>();
                array.Add(window._GetProductName);
                array.Add(window._GetLaneQty);
                array.Add(window._GetSkidConfig);
                array.Add(window._GetHotOrder);

                prodInfoCollect.Add(window._GetSettingGrid, array);

                // Setup
                prodPanels[window._GetSettingGrid].Text = window._GetPrjtName + ", " + window._GetProductName + ", " + window._GetElecType + ", " + (string.IsNullOrEmpty(window._GetLaneQty) == true ? ObjectDic.Instance.GetObjectName("전체") : window._GetLaneQty + " LANE");
                prodMovePanels[window._GetSettingGrid].Text = window._GetConvInfo;

                GetStockLotFifoInfo(prodGrids[window._GetSettingGrid], window._GetProductName, window._GetLaneQty, window._GetSkidConfig, window._GetHotOrder);
            }
        }
        #endregion
        #region Timer
        private void timer_Start(object sender, EventArgs e)
        {
            if (prodInfoCollect != null && prodInfoCollect.Count > 0)
            { 
                for (int i = 0; i < prodGrids.Count; i++)
                {
                    if (prodInfoCollect.ContainsKey(i))
                    {
                        List<string> array = prodInfoCollect[i];
                        if (array != null && array.Count > 0)
                            GetStockLotFifoInfo(prodGrids[i], array[0], array[1], array[2], array[3]);
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
        }

        private void chkLayout_UnChecked(object sender, RoutedEventArgs e)
        {
            dgPanel5.Visibility = Visibility.Collapsed;
            dgPanel6.Visibility = Visibility.Collapsed;
            //ContentGrid.RowDefinitions[5].Height = new GridLength(0);
            ContentGrid.ColumnDefinitions[3].Width = new GridLength(0);
            ContentGrid.ColumnDefinitions[4].Width = new GridLength(0);
        }
    }
}
