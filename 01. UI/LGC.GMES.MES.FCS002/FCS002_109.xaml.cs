/*************************************************************************************
 Created Date : 2021.03.25
      Creator : Dooly
   Decription : 활성화 외부 보관 창고 현황 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.25  DEVELOPER : Initial Created.
  2021.06.02  조영대    : 추가 수정
**************************************************************************************/

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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_109 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        int detailOpenRowIndex = -1;

        public FCS002_109()
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

            SearchWearhouse();
        }
        
        private void SetCombo()
        {
            try
            {
                string[] arrColumn = { "AREAID" };
                string[] arrCondition = { LoginInfo.CFG_AREA_ID };
                cboWearhouse.SetDataComboItem("DA_BAS_SEL_FORM_WHID_CBO", arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, true);
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

        #endregion

        #region Event

        private void cboWearhouse_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            SearchWearhouse();
        }

        private void UcRackInfo_RackClick(object sender, string rackId)
        {
            if (!Util.IsNVC(rackId))
            {
                GetPalletList(rackId);
            }
        }

        private void dgPallet_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            try
            {
                GetBoxList(Util.NVC(dgPallet.GetValue("OUTER_WH_PLLT_ID")));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void sv1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                sv2.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }

        private void sv2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                sv1.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }
        
        private void dgPalletDetl_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;

                    if (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Column.Name.Equals("OUTER_WH_BOX_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletDetlSub_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;

                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }

                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletDetl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
              
                if (dgPalletDetl == null || dgPalletDetl.CurrentRow == null || !dgPalletDetl.Columns.Contains("OUTER_WH_BOX_ID"))
                    return;

                if (dgPalletDetl.CurrentColumn.Name.Equals("OUTER_WH_BOX_ID"))
                {
                    if (detailOpenRowIndex > -1)
                    {
                        dgPalletDetl.Rows[detailOpenRowIndex].DetailsVisibility = Visibility.Collapsed;
                        detailOpenRowIndex = -1;
                    }

                    DataTable dt = GetSublotList(Util.NVC(dgPalletDetl.GetValue(dgPalletDetl.CurrentRow.Index, "OUTER_WH_BOX_ID")));
                    if (dt.Rows.Count == 1)
                    {
                        if (Util.NVC(dt.Rows[0]["CSTID"]).Equals(string.Empty))
                        {
                            // Tray ID 가 없습니다.
                            Util.MessageValidation("SFU1430");
                            return;
                        }

                        FCS002_021 wndTRAY = new FCS002_021();
                        wndTRAY.FrameOperation = FrameOperation;

                        object[] Parameters = new object[10];
                        Parameters[0] = Util.NVC(dt.Rows[0]["CSTID"]);
                        Parameters[1] = Util.NVC(dt.Rows[0]["LOTID"]);

                        this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                    }
                    else if (dt.Rows.Count > 1)
                    {
                        dgPalletDetl.CurrentRow.DetailsVisibility = Visibility.Visible;
                        detailOpenRowIndex = dgPalletDetl.CurrentRow.Index;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletDetlSub_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var detailGrid = sender as C1DataGrid;

                if (detailGrid == null || detailGrid.CurrentRow == null)
                    return;

                if (detailGrid.CurrentColumn.Name.Equals("CSTID"))
                {
                    if (Util.NVC(detailGrid.GetValue("CSTID")).Equals(string.Empty))
                    {
                        // Tray ID 가 없습니다.
                        Util.MessageValidation("SFU1430");
                        return;
                    }

                    FCS002_021 wndTRAY = new FCS002_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(detailGrid.GetValue("CSTID"));
                    Parameters[1] = Util.NVC(detailGrid.GetValue("LOTID"));

                    this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletDetl_LoadedRowDetailsPresenter(object sender, C1.WPF.DataGrid.DataGridRowDetailsEventArgs e)
        {
            if (e.Row.DetailsVisibility == Visibility.Visible)
            {
                var detailGrid = e.DetailsElement as C1DataGrid;

                if (detailGrid != null)
                {
                    if (detailGrid.ItemsSource == null)
                    {
                        DataTable dt = GetSublotList(Util.NVC(dgPalletDetl.GetValue(e.Row.Index, "OUTER_WH_BOX_ID")));
                        detailGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
        }

        #endregion

        #region Mehod
        private void ClearGrid()
        {
            try
            {
                dgWarehouseList.Children.Clear();
                dgWarehouseList.RowDefinitions.Clear();

                dgRackList.Children.Clear();
                dgRackList.RowDefinitions.Clear();
                dgRackList.ColumnDefinitions.Clear();

                Util.gridClear(dgPallet);
                Util.gridClear(dgPalletDetl);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SearchWearhouse()
        {
            try
            {
                ClearGrid();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_ID"] = cboWearhouse.GetBindValue();
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WH_RACK_OUTER_PLLT_MB", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        throw ex;
                    }

                    if (result.Rows.Count > 0)
                    {
                        SetGridList(result);

                        SettingRackInfo();
                    }

                    HiddenLoadingIndicator();

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridList(DataTable dt)
        {
            try
            {
                var whList = dt.AsEnumerable()
                .GroupBy(g => new
                {
                    WHID = g.Field<string>("WH_ID"),
                    WHNAME = g.Field<string>("WH_NAME")
                })
                .Select(f => new
                {
                    WhId = f.Key.WHID,
                    WhName = f.Key.WHNAME
                })
                .OrderBy(o => o.WhId).ToList();

                int rowIndex = 0;

                foreach (var whItem in whList)
                {
                    RowDefinition newRow = new RowDefinition { Height = new GridLength(90) };
                    dgWarehouseList.RowDefinitions.Add(newRow);

                    RowDefinition newRowRack = new RowDefinition { Height = new GridLength(90) };
                    dgRackList.RowDefinitions.Add(newRowRack);

                    UcListHeader ucListHeader = new UcListHeader();
                    ucListHeader.Width = ucListHeader.Height = double.NaN;
                    ucListHeader.Margin = new Thickness(5, 0, 5, 0);

                    ucListHeader.HeaderId = whItem.WhId;
                    ucListHeader.HeaderName = whItem.WhName;

                    Grid.SetRow(ucListHeader, rowIndex);
                    Grid.SetColumn(ucListHeader, 0);

                    dgWarehouseList.Children.Add(ucListHeader);

                    DataRow[] rackInfos = dt.AsEnumerable()
                        .Where(f => Util.NVC(f.Field<string>("WH_ID")).Equals(whItem.WhId)).ToArray();

                    int colIndex = 0;

                    foreach (DataRow dr in rackInfos)
                    {
                        if (dgRackList.ColumnDefinitions.Count <= colIndex)
                        {
                            ColumnDefinition newColumn = new ColumnDefinition { Width = new GridLength(120) };
                            dgRackList.ColumnDefinitions.Add(newColumn);
                        }

                        UcWearhouseRackInfo ucRackInfo = new UcWearhouseRackInfo();
                        ucRackInfo.Width = ucRackInfo.Height = double.NaN;
                        ucRackInfo.Margin = new Thickness(0, 0, -3, 0);

                        ucRackInfo.RackClick += UcRackInfo_RackClick;

                        ucRackInfo.RackId = Util.NVC(dr["RACK_ID"]);
                        ucRackInfo.LoadRate = Convert.ToSingle(dr["LOAD_RATE"]);

                        ucRackInfo.Info1 = Util.NVC(dr["WH_RCV_DTTM"]);
                        ucRackInfo.Info2 = Util.NVC(dr["MDLLOT_ID"]);
                        ucRackInfo.Info3 = Util.NVC(dr["PALLET_CNT"]);
                        ucRackInfo.Info4 = Util.NVC(dr["CELL_CNT"]);

                        Grid.SetRow(ucRackInfo, rowIndex);
                        Grid.SetColumn(ucRackInfo, colIndex);

                        dgRackList.Children.Add(ucRackInfo);

                        colIndex++;
                    }

                    rowIndex++;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void SettingRackInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["WH_ID"] = cboWearhouse.GetBindValue();
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_FORM_WH_RACK_INFO_MB", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    if (ex != null) 
                    {
                        HiddenLoadingIndicator();
                        throw ex;
                    }
                    else
                    {
                        txtLoadRate.Text = result.Rows[0]["LOAD_RATE"].ToString();
                        txtPalletCnt.Text = result.Rows[0]["PLLT_CNT"].ToString();
                        txtBoxCnt.Text = result.Rows[0]["BOX_CNT"].ToString();
                        txtCellCnt.Text = result.Rows[0]["CELL_CNT"].ToString();
                        txtRackCnt.Text = result.Rows[0]["LOAD_CAN_CNT"].ToString();
                    }

                    return;
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

        private void GetPalletList(string RackId)
        {
            try
            {
                dgPallet.ClearRows();
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RACK_ID"] = RackId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_RACK_PLLT_MB", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        throw ex;
                    }
                    else
                    {
                        if (result.Rows.Count > 0)
                        {
                            if (dgPallet.Rows.Count > dgPallet.TopRows.Count + dgPallet.BottomRows.Count)
                            {
                                DataTable dtData = DataTableConverter.Convert(dgPallet.ItemsSource);

                                DataRow drow = dtData.NewRow();
                                drow["OUTER_WH_PLLT_ID"] = result.Rows[0]["OUTER_WH_PLLT_ID"].ToString();
                                drow["LOT_DETL_TYPE_NAME"] = result.Rows[0]["LOT_DETL_TYPE_NAME"].ToString();
                                drow["LOT_DETL_TYPE_CODE"] = result.Rows[0]["LOT_DETL_TYPE_CODE"].ToString();
                                drow["CELL_CNT"] = result.Rows[0]["CELL_CNT"].ToString();
                                dtData.Rows.InsertAt(drow, 0);

                                dgPallet.SetItemsSource(dtData, FrameOperation);
                            }
                            else
                            {
                                dgPallet.SetItemsSource(result, FrameOperation);
                            }
                        }
                    }

                    return;
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

        private void GetBoxList(string palletId)
        {
            try
            {
                detailOpenRowIndex = -1;

                Util.gridClear(dgPalletDetl);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("OUTER_WH_PLLT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_WH_PLLT_ID"] = palletId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                
                new ClientProxy().ExecuteService("DA_PRD_SEL_RACK_BOX_MB", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        throw ex;
                    }
                    else
                    {
                        dgPalletDetl.SetItemsSource(result, FrameOperation);
                    }

                    return;
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

        private DataTable GetSublotList(string boxId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("OUTER_WH_BOX_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_WH_BOX_ID"] = boxId;
                RQSTDT.Rows.Add(dr);
                
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RACK_BOX_SUBLOT_MB", "RQSTDT", "RSLTDT", RQSTDT);

                return dtRslt;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return null;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion 
    }
}
