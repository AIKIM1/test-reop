/*************************************************************************************
 Created Date : 2019.11.07
      Creator : 신광희
   Decription : 물류관리 고공 Conveyor 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.08  신광희 차장 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Diagnostics;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_032.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_032 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly Util _util = new Util();
        private DataTable _transferInfoTable;
        private string _selectedProjectName;
        private string _selectedEquipmentGroupCode;
        private string _selectedEquipmentSegmentCode;
        private string _selectedTrayType;

        private string _bizRuleIp;
        private string _bizRuleProtocol;
        private string _bizRulePort;
        private string _bizRuleServiceMode;
        private string _bizRuleServiceIndex;

        public MCS001_032()
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
            List<Button> listAuth = new List<Button> {btnRealShipping, btnRealSearch, btnEmptyShipping, btnEmptySearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeControl();
            InitializeCombo();
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            if (TabItemRealTray.IsSelected)
            {
                SetDataGridCheckHeaderInitialize(dgRealTrayDetail);
                SelectConveyorSummary();
            }
            else
            {
                SetDataGridCheckHeaderInitialize(dgEmptyTrayDetail);
                SelectConveyorEmptySummary();
                
            }
        }

        private void tabTray_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabItem = ((C1TabItem) ((ItemsControl) sender).Items.CurrentItem).Name;

            switch (tabItem)
            {
                case "TabItemRealTray":
                    
                    break;
                case "TabItemEmptyTray":
                    
                    break;
                default:

                    break;
            }
        }

        private void cboRealArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(cboRealProcess);
            SetLevelLineCombo(cboRealLine);
        }

        private void cboEmptyArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(cboEmptyProcess);
            SetLevelLineCombo(cboEmptyLine);
        }

        private void cboRealLevel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetLevelLineCombo(cboRealLine);
        }

        private void cboEmptyLevel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetLevelLineCombo(cboEmptyLine);
        }

        private void cboRealProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetShippingPortCombo(cboRealShippingPort);
        }

        private void cboEmptyProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetShippingPortCombo(cboEmptyShippingPort);
        }

        private void cboRealLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetShippingPortCombo(cboRealShippingPort);
        }

        private void cboEmptyLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetShippingPortCombo(cboEmptyShippingPort);
        }


        private void btnShipping_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationShipping()) return;
            MessageBox.Show("출고 작업 준비중 입니다.");
        }


        private void chkEmptyHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgEmptyTrayDetail;
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", 1);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void chkEmptyHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgEmptyTrayDetail;
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", 0);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void chkRealHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgRealTrayDetail;
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", 1);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void chkRealHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgRealTrayDetail;
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", 0);
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void dgModelQty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "LOT_CNT")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOT_CNT").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRJT_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);

                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgModelQty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgEmptyModelQty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "CST_CNT")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CNT").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTPROD_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgEmptyModelQty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgLineQty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "LOT_CNT")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOT_CNT").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);

                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgLineQty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgEmptyLineQty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "CST_CNT")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CST_CNT").GetInt() > 0)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQSGNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);

                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgEmptyLineQty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgRealModelQty_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedEquipmentSegmentCode = null;
                _selectedEquipmentGroupCode = null;

                if (ObjectDic.Instance.GetObjectName("합계") == DataTableConverter.GetValue(drv, "PRJT_NAME").GetString())
                {
                    _selectedProjectName = null;
                }
                else
                {
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                }

                SelectPackageConveryorList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEmptyModelQty_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedEquipmentSegmentCode = null;
                _selectedTrayType = null;

                if (ObjectDic.Instance.GetObjectName("합계") == DataTableConverter.GetValue(drv, "CSTPROD_NAME").GetString())
                {
                    _selectedTrayType = null;
                }
                else
                {
                    _selectedTrayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                }

                SelectPackageConveryorEmptyList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRealLineQty_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedEquipmentSegmentCode = null;
                _selectedEquipmentGroupCode = null;
                _selectedEquipmentGroupCode = rdoLineStackFolding.IsChecked == true ? "STK" : "PKG";

                if (ObjectDic.Instance.GetObjectName("합계") == DataTableConverter.GetValue(drv, "EQSGNAME").GetString())
                {
                    _selectedProjectName = null;
                    _selectedEquipmentSegmentCode = null;
                }
                else
                {
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                    _selectedEquipmentSegmentCode = DataTableConverter.GetValue(drv, "EQSGID").GetString();
                }

                SelectPackageConveryorList();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEmptyLineQty_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                if (ObjectDic.Instance.GetObjectName("합계") == DataTableConverter.GetValue(drv, "EQSGNAME").GetString())
                {
                    _selectedTrayType = null;
                    _selectedEquipmentSegmentCode = null;
                }
                else
                {
                    _selectedTrayType = DataTableConverter.GetValue(drv, "CSTPROD").GetString();
                    _selectedEquipmentSegmentCode = DataTableConverter.GetValue(drv, "EQSGID").GetString();
                }

                SelectPackageConveryorEmptyList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoLineStackFolding_Checked(object sender, RoutedEventArgs e)
        {
            if (dgRealTrayDetail != null)
            {
                dgRealTrayDetail.Columns["LINE_NAME"].Header = ObjectDic.Instance.GetObjectName("PKG라인");
                btnSearch_Click(null, null);
            }
        }

        private void rdoLinePKG_Checked(object sender, RoutedEventArgs e)
        {
            if (dgRealTrayDetail != null)
            {
                dgRealTrayDetail.Columns["LINE_NAME"].Header = ObjectDic.Instance.GetObjectName("L&S라인");
                btnSearch_Click(null, null);
            }
        }

        #endregion

        #region Method

        private void SelectTransferInfo()
        {
            const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MCS_TRF_INFO";

            string areaId = string.Empty;
            string levelId = string.Empty;

            if (TabItemRealTray.IsSelected)
            {
                areaId = cboRealArea.SelectedValue.GetString();
                levelId = cboRealLevel.SelectedValue.GetString();
            }
            else
            {
                areaId = cboEmptyArea.SelectedValue.GetString();
                levelId = cboEmptyLevel.SelectedValue.GetString();
            }

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["CMCDTYPE"] = areaId + "_FLOW";
                dr["CMCODE"] = levelId;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    _transferInfoTable = bizResult.Copy();

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        //SelectConveyorProjectSummary(bizResult);
                        //SelectConveyorLineSummary(bizResult);
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
        
        private void SelectTransferInfo(string areaCode, string levelCode, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {
                const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MCS_TRF_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["CMCDTYPE"] = areaCode + "_FLOW";
                dr["CMCODE"] = levelCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }

                    actionCompleted?.Invoke(result, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorSummary()
        {
            ShowLoadingIndicator();

            try
            {
                string areaCode = cboRealArea.SelectedValue.GetString();
                string levelCode = cboRealLevel.SelectedValue.GetString();

                SelectTransferInfo(areaCode, levelCode, (table, ex) =>
                {
                    _transferInfoTable = table.Copy();

                    if (CommonVerify.HasTableRow(table))
                    {
                        // 모델별 수량 조회
                        SelectConveyorProjectSummary(table, (result, exception) =>
                        {
                            if (CommonVerify.HasTableRow(result))
                            {
                                var query = result.AsEnumerable().GroupBy(x => new
                                {
                                    ProductName = x.Field<string>("PRJT_NAME"),
                                }).Select(g => new
                                {
                                    ProjectName = g.Key.ProductName,
                                    SumLotCount = g.Sum(x => x.Field<Int32>("LOT_CNT")),
                                    Count = g.Count()
                                }).ToList();

                                if (query.Any())
                                {
                                    C1DataGrid dg = dgRealModelQty;
                                    DataTable dt = Util.MakeDataTable(dg, false);

                                    foreach (var item in query)
                                    {
                                        DataRow newRow = dt.NewRow();
                                        newRow["PRJT_NAME"] = item.ProjectName;
                                        newRow["LOT_CNT"] = item.SumLotCount;
                                        dt.Rows.Add(newRow);
                                    }

                                    int querySum = query.AsQueryable().Sum(x => x.SumLotCount);
                                    DataRow dr = dt.NewRow();
                                    dr["PRJT_NAME"] = ObjectDic.Instance.GetObjectName("합계");
                                    dr["LOT_CNT"] = querySum;
                                    dt.Rows.Add(dr);

                                    Util.GridSetData(dg, dt, null, true);
                                }
                            }
                        });

                        // 라인별 수량 조회
                        SelectConveyorLineSummary(table, (result, exception) =>
                        {
                            if (CommonVerify.HasTableRow(result))
                            {

                                var query = result.AsEnumerable().GroupBy(x => new
                                {
                                    ProductName = x.Field<string>("PRJT_NAME"),
                                    EquipmentSegmentCode = x.Field<string>("EQSGID"),
                                    EquipmentSegmentName = x.Field<string>("EQSGNAME"),
                                }).Select(g => new
                                {
                                    EquipmentSegmentCode = g.Key.EquipmentSegmentCode,
                                    EquipmentSegmentName = g.Key.EquipmentSegmentName,
                                    ProjectName = g.Key.ProductName,
                                    SumLotCount = g.Sum(x => x.Field<Int32>("LOT_CNT")),
                                    Count = g.Count()
                                }).ToList();

                                if (query.Any())
                                {
                                    C1DataGrid dg = dgRealLineQty;
                                    DataTable dt = Util.MakeDataTable(dg, false);

                                    foreach (var item in query)
                                    {
                                        DataRow newRow = dt.NewRow();
                                        newRow["EQSGID"] = item.EquipmentSegmentCode;
                                        newRow["EQSGNAME"] = item.EquipmentSegmentName;
                                        newRow["PRJT_NAME"] = item.ProjectName;
                                        newRow["LOT_CNT"] = item.SumLotCount;
                                        dt.Rows.Add(newRow);
                                    }

                                    int querySum = query.AsQueryable().Sum(x => x.SumLotCount);
                                    DataRow dr = dt.NewRow();
                                    dr["EQSGNAME"] = ObjectDic.Instance.GetObjectName("합계");
                                    dr["LOT_CNT"] = querySum;
                                    dt.Rows.Add(dr);

                                    Util.GridSetData(dg, dt, null, true);
                                }
                            }
                        });
                        HiddenLoadingIndicator();
                    }
                    else
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorEmptySummary()
        {
            ShowLoadingIndicator();
            try
            {
                string areaCode = cboEmptyArea.SelectedValue.GetString();
                string levelCode = cboEmptyLevel.SelectedValue.GetString();

                SelectTransferInfo(areaCode, levelCode, (table, ex) =>
                {
                    _transferInfoTable = table.Copy();

                    if (CommonVerify.HasTableRow(table))
                    {
                        // 모델별 수량 조회
                        SelectConveyorProjectEmptySummary(table, (result, exception) =>
                        {
                            if (CommonVerify.HasTableRow(result))
                            {
                                var query = result.AsEnumerable().GroupBy(x => new
                                {
                                    TrayType = x.Field<string>("CSTPROD"),
                                    TrayTypeName = x.Field<string>("CSTPROD_NAME"),
                                }).Select(g => new
                                {
                                    TrayType = g.Key.TrayType,
                                    TrayTypeName = g.Key.TrayTypeName,
                                    SumTrayCount = g.Sum(x => x.Field<Int32>("CST_CNT")),
                                    Count = g.Count()
                                }).ToList();

                                if (query.Any())
                                {
                                    C1DataGrid dg = dgEmptyModelQty;
                                    DataTable dt = Util.MakeDataTable(dg, false);

                                    foreach (var item in query)
                                    {
                                        DataRow newRow = dt.NewRow();
                                        newRow["CSTPROD"] = item.TrayType;
                                        newRow["CSTPROD_NAME"] = item.TrayTypeName;
                                        newRow["CST_CNT"] = item.SumTrayCount;
                                        dt.Rows.Add(newRow);
                                    }

                                    int querySum = query.AsQueryable().Sum(x => x.SumTrayCount);
                                    DataRow dr = dt.NewRow();
                                    dr["CSTPROD_NAME"] = ObjectDic.Instance.GetObjectName("합계");
                                    dr["CST_CNT"] = querySum;
                                    dt.Rows.Add(dr);

                                    Util.GridSetData(dg, dt, null, true);
                                }
                            }
                        });

                        // 라인별 수량 조회
                        SelectConveyorLineEmptySummary(table, (result, exception) =>
                        {
                            if (CommonVerify.HasTableRow(result))
                            {

                                var query = result.AsEnumerable().GroupBy(x => new
                                {
                                    EquipmentSegmentCode = x.Field<string>("EQSGID"),
                                    EquipmentSegmentName = x.Field<string>("EQSGNAME"),
                                    TrayType = x.Field<string>("CSTPROD"),
                                    TrayTypeName = x.Field<string>("CSTPROD_NAME"),
                                }).Select(g => new
                                {
                                    EquipmentSegmentCode = g.Key.EquipmentSegmentCode,
                                    EquipmentSegmentName = g.Key.EquipmentSegmentName,
                                    TrayType = g.Key.TrayType,
                                    TrayTypeName = g.Key.TrayTypeName,
                                    SumTrayCount = g.Sum(x => x.Field<Int32>("CST_CNT")),
                                    Count = g.Count()
                                }).ToList();

                                if (query.Any())
                                {
                                    C1DataGrid dg = dgEmptyLineQty;
                                    DataTable dt = Util.MakeDataTable(dg, false);

                                    foreach (var item in query)
                                    {
                                        DataRow newRow = dt.NewRow();
                                        newRow["EQSGID"] = item.EquipmentSegmentCode;
                                        newRow["EQSGNAME"] = item.EquipmentSegmentName;
                                        newRow["CSTPROD"] = item.TrayType;
                                        newRow["CSTPROD_NAME"] = item.TrayTypeName;
                                        newRow["CST_CNT"] = item.SumTrayCount;
                                        dt.Rows.Add(newRow);
                                    }

                                    int querySum = query.AsQueryable().Sum(x => x.SumTrayCount);
                                    DataRow dr = dt.NewRow();
                                    dr["EQSGNAME"] = ObjectDic.Instance.GetObjectName("합계");
                                    dr["CST_CNT"] = querySum;
                                    dt.Rows.Add(dr);

                                    Util.GridSetData(dg, dt, null, true);
                                }
                            }
                        });
                        HiddenLoadingIndicator();
                    }
                    else
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectPackageConveryorList()
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            try
            {
                ShowLoadingIndicator();

                C1DataGrid dg = TabItemRealTray.IsSelected ? dgRealTrayDetail : dgEmptyTrayDetail;
                const string bizRuleName = "DA_MCS_SEL_STK_PKG_CNV_LIST_USING";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("SRC_PORT", typeof(string));
                inTable.Columns.Add("DST_PORT", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));

                foreach (DataRow row in _transferInfoTable.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = row["CARRIERID"].ToString();
                    dr["SRC_PORT"] = row["SRC_LOCID"].ToString();
                    dr["DST_PORT"] = row["DST_LOCID"].ToString();
                    dr["EQGRID"] = _selectedEquipmentGroupCode;
                    dr["EQSGID"] = _selectedEquipmentSegmentCode;
                    dr["PRJT_NAME"] = _selectedProjectName;
                    inTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    //sw.Stop();
                    //ControlsLibrary.MessageBox.Show(sw.Elapsed.ToString());

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    HiddenLoadingIndicator();
                    Util.GridSetData(dg, bizResult, null, true);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectPackageConveryorEmptyList()
        {
            try
            {
                ShowLoadingIndicator();

                C1DataGrid dg = dgEmptyTrayDetail;
                const string bizRuleName = "DA_MCS_SEL_STK_PKG_CNV_LIST_EMPTY";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("SRC_PORT", typeof(string));
                inTable.Columns.Add("DST_PORT", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));

                foreach (DataRow row in _transferInfoTable.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = row["CARRIERID"].ToString();
                    dr["SRC_PORT"] = row["SRC_LOCID"].ToString();
                    dr["DST_PORT"] = row["DST_LOCID"].ToString();
                    dr["EQSGID"] = _selectedEquipmentSegmentCode;
                    dr["CSTPROD"] = _selectedTrayType;
                    inTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    HiddenLoadingIndicator();
                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorProjectSummary(DataTable dt, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {

                const string bizRuleName = "DA_MCS_SEL_STK_PKG_CNV_PJT_SUMMARY_USING";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("SRC_PORT", typeof(string));
                inTable.Columns.Add("DST_PORT", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["CSTID"] = row["CARRIERID"].ToString();
                    dr["SRC_PORT"] = row["SRC_LOCID"].ToString();
                    dr["DST_PORT"] = row["DST_LOCID"].ToString();
                    if (!string.IsNullOrEmpty(txtRealProjectName.Text))
                    {
                        dr["PRJT_NAME"] = txtRealProjectName.Text;
                    }
                    inTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    actionCompleted?.Invoke(bizResult, null);

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorProjectEmptySummary(DataTable dt, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {

                const string bizRuleName = "DA_MCS_SEL_STK_PKG_CNV_PJT_SUMMARY_EMPTY";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("SRC_PORT", typeof(string));
                inTable.Columns.Add("DST_PORT", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = row["CARRIERID"].ToString();
                    dr["SRC_PORT"] = row["SRC_LOCID"].ToString();
                    dr["DST_PORT"] = row["DST_LOCID"].ToString();
                    if (!string.IsNullOrEmpty(txtTrayType.Text))
                    {
                        dr["CSTPROD"] = txtTrayType.Text;
                    }
                    inTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    actionCompleted?.Invoke(bizResult, null);

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorLineSummary(DataTable dt, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_STK_PKG_CNV_LINE_SUMMARY_USING";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("SRC_PORT", typeof(string));
                inTable.Columns.Add("DST_PORT", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));

                string equipmentGroupCode;

                if (rdoLineStackFolding.IsChecked != null && (bool)rdoLineStackFolding.IsChecked)
                {
                    equipmentGroupCode = "STK";
                }
                else
                {
                    equipmentGroupCode = "PKG";
                }

                foreach (DataRow row in dt.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = row["CARRIERID"].ToString();
                    dr["SRC_PORT"] = row["SRC_LOCID"].ToString();
                    dr["DST_PORT"] = row["DST_LOCID"].ToString();
                    dr["EQGRID"] = equipmentGroupCode;

                    if (!string.IsNullOrEmpty(txtRealProjectName.Text))
                    {
                        dr["PRJT_NAME"] = txtRealProjectName.Text;
                    }
                    inTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    actionCompleted?.Invoke(bizResult, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectConveyorLineEmptySummary(DataTable dt, Action<DataTable, Exception> actionCompleted = null)
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_STK_PKG_CNV_LINE_SUMMARY_EMPTY";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("SRC_PORT", typeof(string));
                inTable.Columns.Add("DST_PORT", typeof(string));
                inTable.Columns.Add("CSTPROD", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CSTID"] = row["CARRIERID"].ToString();
                    dr["SRC_PORT"] = row["SRC_LOCID"].ToString();
                    dr["DST_PORT"] = row["DST_LOCID"].ToString();

                    if (!string.IsNullOrEmpty(txtTrayType.Text))
                    {
                        dr["CSTPROD"] = txtTrayType.Text;
                    }
                    inTable.Rows.Add(dr);
                }

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    actionCompleted?.Invoke(bizResult, null);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeControl()
        {
            TabItemRealTray.IsSelected = true;

            _transferInfoTable = new DataTable();
            _transferInfoTable.Columns.Add("CARRIERID", typeof(string));
            _transferInfoTable.Columns.Add("SRC_LOCID", typeof(string));
            _transferInfoTable.Columns.Add("DST_LOCID", typeof(string));
        }

        private void InitializeCombo()
        {
            // 동 콤보박스
            SetAreaCombo(cboRealArea);
            SetAreaCombo(cboEmptyArea);

            // 층 콤보박스
            SetLineCombo(cboRealLevel);
            SetLineCombo(cboEmptyLevel);
        }

        private void ClearControl()
        {
            if (TabItemRealTray.IsSelected)
            {
                Util.gridClear(dgRealTrayDetail); 
                Util.gridClear(dgRealLineQty);
                Util.gridClear(dgRealModelQty);
            }
            else
            {
                Util.gridClear(dgEmptyTrayDetail);
                Util.gridClear(dgEmptyLineQty);
                Util.gridClear(dgEmptyModelQty);
            }

            _selectedProjectName = string.Empty;
            _selectedEquipmentGroupCode = string.Empty;
            _selectedEquipmentSegmentCode = string.Empty;
            _selectedTrayType = string.Empty;
            _transferInfoTable.Clear();
        }

        private bool ValidationShipping()
        {
            C1DataGrid dg = TabItemRealTray.IsSelected ? dgRealTrayDetail : dgEmptyTrayDetail;
            C1ComboBox cbo = TabItemRealTray.IsSelected ? cboRealShippingPort : cboEmptyShippingPort;

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

            if (cbo.SelectedIndex < 0 || string.IsNullOrEmpty(cbo.SelectedValue.GetString()))
            {
                Util.MessageValidation("SFU4528");
                return false;
            }

            return true;
        }

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "SYSTEM_ID", "USERID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.SYSID, LoginInfo.USERID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private static void SetLineCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_INF_MCS_DB_SEL_TB_MMD_MCS_COMMONCODE";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "A7_FLOW";
            dr["CMCODE"] = null;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;

            //DataTable dt = DataTableConverter.Convert(cboDestination.ItemsSource);
            //DataRow dr = dt.Rows[cboDestination.SelectedIndex];
            //dr["PORT_WRK_MODE"].GetString();

        }

        private static void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_AREA_STK_FOL_CNV_EQGRID_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, cbo.SelectedValue.GetString()};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetLevelLineCombo(C1ComboBox cbo)
        {
            string areaId;
            string flow;

            switch (cbo.Name)
            {
                case "cboRealLine":
                    areaId = cboRealArea.SelectedValue.GetString();
                    flow = cboRealLevel.SelectedValue.GetString();
                    break;
                case "cboEmptyLine":
                    areaId = cboEmptyArea.SelectedValue.GetString();
                    flow = cboEmptyLevel.SelectedValue.GetString();
                    break;
                default:
                    areaId = string.Empty;
                    flow = string.Empty;
                    break;
            }

            const string bizRuleName = "DA_MCS_SEL_AREA_STK_FOL_CNV_FLOW_EQSGID_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "FLOW" };
            string[] arrCondition = { LoginInfo.LANGID, areaId, flow };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetShippingPortCombo(C1ComboBox cbo)
        {
            string equipmentCode;
            string equipmentGroupCode;
            string equipmentSegmentCode;

            switch (cbo.Name)
            {
                case "cboRealShippingPort":

                    if (cboRealLevel.SelectedIndex < 0 || cboRealLevel?.SelectedValue == null)
                    {
                        equipmentCode = null;
                    }
                    else
                    {
                        DataTable dt = DataTableConverter.Convert(cboRealLevel.ItemsSource);
                        DataRow dr = dt.Rows[cboRealLevel.SelectedIndex];
                        equipmentCode = dr["VALUE"].GetString();
                    }

                    equipmentGroupCode = cboRealProcess.SelectedValue.GetString();
                    equipmentSegmentCode = cboRealLine.SelectedValue.GetString();
                    break;

                case "cboEmptyShippingPort":
                    if (cboEmptyLevel.SelectedIndex < 0 || cboEmptyLevel?.SelectedValue == null)
                    {
                        equipmentCode = null;
                    }
                    else
                    {
                        DataTable dt = DataTableConverter.Convert(cboEmptyLevel.ItemsSource);
                        DataRow dr = dt.Rows[cboEmptyLevel.SelectedIndex];
                        equipmentCode = dr["VALUE"].GetString();
                    }

                    equipmentGroupCode = cboEmptyProcess.SelectedValue.GetString();
                    equipmentSegmentCode = cboEmptyLine.SelectedValue.GetString();
                    break;
                default:
                    equipmentCode = string.Empty;
                    equipmentGroupCode = string.Empty;
                    equipmentSegmentCode = string.Empty;
                    break;
            }

            const string bizRuleName = "DA_MCS_SEL_PORT_OUT_CONVEYOR";
            string[] arrColumn = { "LANGID", "EQPTID", "EQGRID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, equipmentCode, equipmentGroupCode,equipmentSegmentCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                if (dg.Name == "dgRealTrayDetail")
                {
                    allCheck.Unchecked -= chkRealHeaderAll_Unchecked;
                    allCheck.IsChecked = false;
                    allCheck.Unchecked += chkRealHeaderAll_Unchecked;
                }
                else
                {
                    allCheck.Unchecked -= chkEmptyHeaderAll_Unchecked;
                    allCheck.IsChecked = false;
                    allCheck.Unchecked += chkEmptyHeaderAll_Unchecked;
                }
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
