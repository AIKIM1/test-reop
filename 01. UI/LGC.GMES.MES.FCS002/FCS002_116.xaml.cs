/*************************************************************************************
 Created Date : 2021.08.25
      Creator : 강동희
   Decription : Selector 현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.08.25  강동희 : Initial Created. (Copy by COM001_315)
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.FCS002.Controls;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_116 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private System.Windows.Threading.DispatcherTimer _timer = null;

        public FCS002_116()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            SetCombo();
        }

        private void SetCombo()
        {
            try
            {
                // Selector List
                SetSelectorEquipmentCombo(cboEqpID);

                // 자동조회
                CommonCombo_Form _combo = new CommonCombo_Form();
                String[] sFilter3 = { "SECOND_INTERVAL" };
                _combo.SetCombo(cboAutoSearch, CommonCombo_Form.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= UserControl_Loaded;
        }

        private void TimerSetting()
        {
            if (cboAutoSearch.SelectedValue == null || string.IsNullOrWhiteSpace(cboAutoSearch.SelectedValue.ToString()))
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            _timer = new System.Windows.Threading.DispatcherTimer();
            int interval = Convert.ToInt32(cboAutoSearch.SelectedValue);

            _timer.Interval = TimeSpan.FromSeconds(interval);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                btnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetSelectorEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_SEL_SELECTOR_EQP_CBO";
            string[] arrColumn = { "LANGID", "S70" };
            string[] arrCondition = { LoginInfo.LANGID, "6" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText);
        }

        #endregion

        #region Event

        private void cboEqpID_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(Util.NVC(cboEqpID.SelectedValue)))
            {
                btnSearch_Click(null, null);
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEqpID.GetBindValue() == null)
                {
                    //%1(을)를 선택하세요.
                    Util.MessageValidation("SFU4925", Util.NVC(lblEqpID.Text));
                    return;
                }

                GetSectionInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            TimerSetting();
        }

        private void UcEqpInfo_EqpClick(object sender, string Port_ID, string Lot_ID, string Tray_ID, string Wip_Qty)
        {
            for (int i = 0; i < dgPortList.Children.Count; i++)
            {
                (dgPortList.Children[i] as UcTrfSelectorMNTInfo).SetColor = "WHITE";
            }

            ClearCellInfo();

            UcTrfSelectorMNTInfo ucEqpInfo = sender as UcTrfSelectorMNTInfo;

            ucEqpInfo.SetColor = "GREEN";

            txtSelEqpID.Text = Util.NVC(cboEqpID.SelectedValue);
            txtSelPortID.Text = Util.NVC(Port_ID);
            txtSelTrayID.Text = Util.NVC(Tray_ID);
            txtSelLotID.Text = Util.NVC(Lot_ID);

            if (!Util.IsNVC(Lot_ID))
            {
                GetCellList(Lot_ID);
                //GetCellList("ABDS114001AATP04");
                expander1.IsExpanded = true;
            }
        }

        private void dgCellList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        ////////////////////////////////////////////  default 색상 및 Cursor
                        e.Cell.Presenter.Cursor = Cursors.Arrow;

                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontSize = 12;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        ///////////////////////////////////////////////////////////////////////////////////

                        if (e.Cell.Column.Name.ToString().Equals("SUBLOTID"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCellList.GetCellFromPoint(pnt);

                if (cell != null)
                {

                    if (!cell.Column.Name.Equals("SUBLOTID"))
                    {
                        return;
                    }

                    if (cell.Column.Name.Equals("SUBLOTID"))
                    {
                        GetCellInfo(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[cell.Row.Index].DataItem, "SUBLOTID")).ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void sv2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                //sv1.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }

        private void dgHist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgHist.GetCellFromPoint(pnt);

                if (cell != null)
                {

                    if (!cell.Column.Name.Equals("CSTID") && !cell.Column.Name.Equals("LOTID") && !cell.Column.Name.Equals("ROUTID"))
                    {
                        return;
                    }

                    if (cell.Column.Name.Equals("CSTID") || cell.Column.Name.Equals("LOTID"))
                    {
                        string sTrayId = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "CSTID")).ToString();
                        string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgHist.Rows[cell.Row.Index].DataItem, "LOTID")).ToString();

                        // 프로그램 ID 확인 후 수정
                        object[] parameters = new object[6];
                        parameters[0] = sTrayId;
                        parameters[1] = sTrayNo;
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = "Y";
                        this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회 연계
                    }
                    else if (cell.Column.Name.Equals("ROUTID"))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("LOTID") || e.Column.Name.Equals("CSTID"))
                    {
                    }
                }
            }));
        }

        private void dgHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.Equals("LOTID") || e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));
        }

        private void txtSelLotID_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtSelLotID.Text)))
                {
                    string sTrayId = Util.NVC(txtSelTrayID.Text);
                    string sTrayNo = Util.NVC(txtSelLotID.Text);

                    // 프로그램 ID 확인 후 수정
                    object[] parameters = new object[6];
                    parameters[0] = sTrayId;
                    parameters[1] = sTrayNo;
                    parameters[2] = string.Empty;
                    parameters[3] = string.Empty;
                    parameters[4] = string.Empty;
                    parameters[5] = "Y";
                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회 연계
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void txtSelTrayID_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtSelTrayID.Text)))
                {
                    string sTrayId = Util.NVC(txtSelTrayID.Text);
                    string sTrayNo = Util.NVC(txtSelLotID.Text);

                    // 프로그램 ID 확인 후 수정
                    object[] parameters = new object[6];
                    parameters[0] = sTrayId;
                    parameters[1] = sTrayNo;
                    parameters[2] = string.Empty;
                    parameters[3] = string.Empty;
                    parameters[4] = string.Empty;
                    parameters[5] = "Y";
                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회 연계
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Mehod
        private void ClearScreen()
        {
            try
            {
                dgPortList.Children.Clear();
                dgPortList.RowDefinitions.Clear();
                dgPortList.ColumnDefinitions.Clear();

                txtSelEqpID.Text = string.Empty;
                txtSelPortID.Text = string.Empty;
                txtSelTrayID.Text = string.Empty;
                txtSelLotID.Text = string.Empty;

                Util.gridClear(dgCellList);
                Util.gridClear(dgHist);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearCellInfo()
        {
            txtCellID.Text = string.Empty;
            txtCellNo.Text = string.Empty;
            txtRouteID.Text = string.Empty;
            txtTrayNo.Text = string.Empty;

            txtLotID.Text = string.Empty;
            txtOper.Text = string.Empty;
            txtCreateTime.Text = string.Empty;
            txtTrayID.Text = string.Empty;

            Util.gridClear(dgHist);
        }

        private void GetCellInfo(string sCellID)
        {
            try
            {
                ClearCellInfo();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = Util.NVC(sCellID);
                if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString()))
                {
                    return;
                }
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_PROCESS_RETRIEVE_INFO", "RQSTDT", "INFO1,INFO2", dsRqst);

                if (dsRslt.Tables["INFO1"].Rows.Count > 0)
                {
                    txtCellID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["SUBLOTID"].ToString());
                    txtCellNo.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CSTSLOT"].ToString());
                    txtRouteID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["ROUTID"].ToString());
                    txtTrayNo.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["LOTID"].ToString());

                    txtLotID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PROD_LOTID"].ToString());
                    txtOper.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["PROCID"].ToString());
                    txtCreateTime.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["LOTDTTM_CR"].ToString());
                    txtTrayID.Text = Util.NVC(dsRslt.Tables["INFO1"].Rows[0]["CSTID"].ToString());
                }

                if (dsRslt.Tables["INFO2"].Rows.Count > 0)
                {
                    Util.GridSetData(dgHist, dsRslt.Tables["INFO2"], FrameOperation, true);
                    expander.IsExpanded = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSectionInfo()
        {
            try
            {
                ClearScreen();

                const string bizRuleName = "DA_SEL_SELECTOR_SUMMARY";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboEqpID.GetBindValue();
                inDataTable.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {
                        SetGridList(bizResult);
                    }
                    else
                    {
                        GetTestData(ref bizResult);
                        SetGridList(bizResult);
                    }

                    HiddenLoadingIndicator();
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

        private void SetGridList(DataTable dt)
        {
            try
            {
                var eqpList = dt.AsEnumerable()
                .GroupBy(g => new
                {
                    ROW = g.Field<string>("ROW_VALUE"),
                    COLUMN = g.Field<string>("COL_VALUE")
                })
                .Select(f => new
                {
                    row = f.Key.ROW,
                    Columns = f.Key.COLUMN
                })
                .OrderBy(o => o.row).ToList();

                foreach (var eqpItem in eqpList)
                {
                    DataRow[] drrowinfo = dt.AsEnumerable().Where(f => f.Field<string>("ROW_VALUE").Equals(eqpItem.row)).ToArray();

                    int MaxHValue = 0;

                    foreach (DataRow dr in drrowinfo)
                    {
                        if (MaxHValue < int.Parse(Util.NVC(dr["H_VALUE"])))
                        {
                            MaxHValue = int.Parse(Util.NVC(dr["H_VALUE"]));
                        }
                    }

                    if (dgPortList.RowDefinitions.Count < int.Parse(eqpItem.row))
                    {
                        RowDefinition newRowRack = new RowDefinition { Height = new GridLength(MaxHValue) };
                        dgPortList.RowDefinitions.Add(newRowRack);
                    }
                }

                foreach (var eqpItem in eqpList)
                {
                    DataRow[] drcolInfo = dt.AsEnumerable().Where(f => f.Field<string>("COL_VALUE").Equals(eqpItem.Columns)).ToArray();

                    int MaxWValue = 0;

                    foreach (DataRow dr in drcolInfo)
                    {
                        if (MaxWValue < int.Parse(Util.NVC(dr["W_VALUE"])))
                        {
                            MaxWValue = int.Parse(Util.NVC(dr["W_VALUE"]));
                        }
                    }

                    if (dgPortList.ColumnDefinitions.Count < int.Parse(eqpItem.Columns))
                    {
                        if (MaxWValue > 500)
                        {
                            ColumnDefinition newColumn = new ColumnDefinition { Width = new GridLength(500) };
                            dgPortList.ColumnDefinitions.Add(newColumn);
                        }
                        else
                        {
                            ColumnDefinition newColumn = new ColumnDefinition { Width = new GridLength(MaxWValue) };
                            dgPortList.ColumnDefinitions.Add(newColumn);
                        }
                    }
                }

                DataRow[] eqpInfos = dt.AsEnumerable().ToArray();

                foreach (DataRow dr in eqpInfos)
                {
                    int rowValue = int.Parse(Util.NVC(dr["ROW_VALUE"]));
                    int colValue = int.Parse(Util.NVC(dr["COL_VALUE"]));
                    int HValue = int.Parse(Util.NVC(dr["H_VALUE"]));
                    int WValue = int.Parse(Util.NVC(dr["W_VALUE"]));

                    string sMargin = Util.NVC(dr["MARGIN"]);
                    string[] sMarginList = sMargin.Split(',');

                    double LValue = double.Parse(Util.NVC(sMarginList[0]));
                    double TValue = double.Parse(Util.NVC(sMarginList[1]));
                    double RValue = double.Parse(Util.NVC(sMarginList[2]));
                    double BValue = double.Parse(Util.NVC(sMarginList[3]));

                    UcTrfSelectorMNTInfo ucEqpInfo = new UcTrfSelectorMNTInfo();

                    ucEqpInfo.Width = WValue;
                    ucEqpInfo.Height = HValue;
                    ucEqpInfo.Margin = new Thickness(LValue, TValue, RValue, BValue);

                    ucEqpInfo.EqpClick += UcEqpInfo_EqpClick;

                    ucEqpInfo.EQP_ID = Util.NVC(dr["EQP_ID"]);
                    ucEqpInfo.PORT_ID = Util.NVC(dr["PORT_ID"]);
                    ucEqpInfo.TRAY_ID = Util.NVC(dr["TRAY_ID"]);
                    ucEqpInfo.LOT_ID = Util.NVC(dr["LOT_ID"]);
                    ucEqpInfo.WIP_QTY = Util.NVC(dr["WIP_QTY"]);
                    ucEqpInfo.SetColor = "WHITE";
                    ucEqpInfo.ROW = rowValue - 1;
                    ucEqpInfo.COL = colValue - 1;

                    ucEqpInfo.Info1 = ObjectDic.Instance.GetObjectName("Port 정보") + " : " + Util.NVC(dr["PORT_NAME"]) + "(" + Util.NVC(dr["PORT_ID"]) + ")";
                    ucEqpInfo.Info2 = ObjectDic.Instance.GetObjectName("TRAY_ID") + " : " + Util.NVC(dr["TRAY_ID"]);
                    ucEqpInfo.Info3 = ObjectDic.Instance.GetObjectName("TRAY_LOT_ID") + " : " + Util.NVC(dr["LOT_ID"]);
                    ucEqpInfo.Info4 = ObjectDic.Instance.GetObjectName("재공수량") + " : " + Util.NVC(dr["WIP_QTY"]);

                    Grid.SetRow(ucEqpInfo, rowValue - 1);
                    Grid.SetColumn(ucEqpInfo, colValue - 1);
                    if (Util.NVC(dr["EQP_ID"]).Equals("EQP"))
                    {
                        Grid.SetColumnSpan(ucEqpInfo, 99);
                    }

                    dgPortList.Children.Add(ucEqpInfo);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCellList(string LotID)
        {
            try
            {
                Util.gridClear(dgCellList);

                const string bizRuleName = "DA_BAS_SEL_SUBLOT_BY_LOTID";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LOTID"] = Util.NVC(LotID);
                inDataTable.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (bizResult.Rows.Count > 0)
                    {
                        Util.GridSetData(dgCellList, bizResult, FrameOperation, false);
                    }
                    else
                    {
                        Util.GridSetData(dgCellList, bizResult, FrameOperation, false);
                    }

                    HiddenLoadingIndicator();
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

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void GetTestData(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("EQP_ID", typeof(string));
            //dt.Columns.Add("PORT_ID", typeof(string));
            //dt.Columns.Add("PORT_NAME", typeof(string));
            //dt.Columns.Add("TRAY_ID", typeof(string));
            //dt.Columns.Add("LOT_ID", typeof(string));
            //dt.Columns.Add("WIP_QTY", typeof(string));
            //dt.Columns.Add("ROW_VALUE", typeof(string));
            //dt.Columns.Add("COL_VALUE", typeof(string));
            //dt.Columns.Add("H_VALUE", typeof(string));
            //dt.Columns.Add("W_VALUE", typeof(string));
            //dt.Columns.Add("MARGIN", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["EQP_ID"] = "EQPTID_1";
            row1["PORT_ID"] = "PORT ID 1";
            row1["PORT_NAME"] = "PORT DESC 1";
            row1["TRAY_ID"] = "TRAY ID 1";
            row1["LOT_ID"] = "LOT ID 1";
            row1["WIP_QTY"] = "5";
            row1["ROW_VALUE"] = "1";
            row1["COL_VALUE"] = "1";
            row1["H_VALUE"] = "300";
            row1["W_VALUE"] = "500";
            row1["MARGIN"] = "0,0,0,0";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["EQP_ID"] = "EQPTID_2";
            row2["PORT_ID"] = "PORT ID 2";
            row2["PORT_NAME"] = "PORT DESC 2";
            row2["TRAY_ID"] = "TRAY ID 2";
            row2["LOT_ID"] = "LOT ID 2";
            row2["WIP_QTY"] = "15";
            row2["ROW_VALUE"] = "1";
            row2["COL_VALUE"] = "2";
            row2["H_VALUE"] = "300";
            row2["W_VALUE"] = "500";
            row2["MARGIN"] = "0,0,0,0";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["EQP_ID"] = "EQPTID_3";
            row3["PORT_ID"] = "PORT ID 3";
            row3["PORT_NAME"] = "PORT DESC 3";
            row3["TRAY_ID"] = "TRAY ID 3";
            row3["LOT_ID"] = "LOT ID 3";
            row3["WIP_QTY"] = "20";
            row3["ROW_VALUE"] = "1";
            row3["COL_VALUE"] = "3";
            row3["H_VALUE"] = "300";
            row3["W_VALUE"] = "500";
            row3["MARGIN"] = "0,0,0,0";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["EQP_ID"] = "EQP";
            row4["PORT_ID"] = "";
            row4["PORT_NAME"] = "";
            row4["TRAY_ID"] = "";
            row4["LOT_ID"] = "LOT ID 4";
            row4["WIP_QTY"] = "25";
            row4["ROW_VALUE"] = "2";
            row4["COL_VALUE"] = "1";
            row4["H_VALUE"] = "100";
            row4["W_VALUE"] = "1500";
            row4["MARGIN"] = "0,0,0,0";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["EQP_ID"] = "EQPTID_4";
            row5["PORT_ID"] = "PORT ID 4";
            row5["PORT_NAME"] = "PORT DESC 4";
            row5["TRAY_ID"] = "TRAY ID 4";
            row5["LOT_ID"] = "LOT ID 5";
            row5["WIP_QTY"] = "30";
            row5["ROW_VALUE"] = "3";
            row5["COL_VALUE"] = "1";
            row5["H_VALUE"] = "300";
            row5["W_VALUE"] = "500";
            row5["MARGIN"] = "0,0,0,0";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["EQP_ID"] = "EQPTID_5";
            row6["PORT_ID"] = "PORT ID 5";
            row6["PORT_NAME"] = "PORT DESC 5";
            row6["TRAY_ID"] = "TRAY ID 5";
            row6["LOT_ID"] = "LOT ID 5";
            row6["WIP_QTY"] = "30";
            row6["ROW_VALUE"] = "3";
            row6["COL_VALUE"] = "2";
            row6["H_VALUE"] = "300";
            row6["W_VALUE"] = "500";
            row6["MARGIN"] = "0,0,0,0";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["EQP_ID"] = "EQPTID_6";
            row7["PORT_ID"] = "PORT ID 6";
            row7["PORT_NAME"] = "PORT DESC 6";
            row7["TRAY_ID"] = "TRAY ID 6";
            row7["LOT_ID"] = "LOT ID 6";
            row7["WIP_QTY"] = "35";
            row7["ROW_VALUE"] = "3";
            row7["COL_VALUE"] = "3";
            row7["H_VALUE"] = "300";
            row7["W_VALUE"] = "500";
            row7["MARGIN"] = "0,0,0,0";
            dt.Rows.Add(row7);

            #endregion

        }

    }
}