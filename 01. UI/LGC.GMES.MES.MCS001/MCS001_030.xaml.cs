/*************************************************************************************
 Created Date : 2019.06.14
      Creator : 신광희
   Decription : Conveyor 반송 현황
--------------------------------------------------------------------------------------
 [Change History]
  2019.06.14  신광희 차장 : Initial Created.    
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_030.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_030 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string _selectedEquipmentCode;
        private string _selectedElectrodeTypeCode;
        private string _selectedProjectName;
        private string _selectedWipHold;

        public MCS001_030()
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
            InitializeCombo();
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            SelectConveyorCapacitySummary();
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            cboConveyor.SelectedValueChanged -= cboConveyor_SelectedValueChanged;
            SetConveyorCombo(cboConveyor);
            cboConveyor.SelectedValueChanged += cboConveyor_SelectedValueChanged;
        }


        private void cboConveyor_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }

        private void dgCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "EQPTNAME") || string.Equals(e.Cell.Column.Name, "BBN_E_QTY"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
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

        private void dgCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                if (cell.Column.Name.Equals("EQPTNAME") || cell.Column.Name.Equals("BBN_E_QTY"))
                {

                    int rowIdx = cell.Row.Index;
                    DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                    if (drv == null) return;

                    _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();

                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "Total"))
                    {
                        _selectedEquipmentCode = null;
                    }

                    if (cell.Column.Name.Equals("EQPTNAME"))
                    {
                        txtSection.Text = DataTableConverter.GetValue(drv, "EQPTNAME").GetString();
                        txtRealCarrierCount.Text = DataTableConverter.GetValue(drv, "BBN_U_QTY").GetString();

                        Util.gridClear(dgProduct);
                        SelectEquipmentProductSummary();
                        tabProduct.IsSelected = true;
                    }
                    else
                    {
                        tabEmptyCarrier.IsSelected = true;
                        SelectEquipmentEmptyCarrier();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProductSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

                _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                _selectedElectrodeTypeCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString()) ? null : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();

                if (cell.Column.Name.Equals("LOT_QTY") || cell.Column.Name.Equals("WIP_QTY"))
                {
                    _selectedWipHold = "N";
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QTY") || cell.Column.Name.Equals("WIP_HOLD_QTY"))
                {
                    _selectedWipHold = "Y";
                }
                else if (cell.Column.Name.Equals("ELTR_TYPE_NAME"))
                {
                    _selectedWipHold = null;
                }
                else if (cell.Column.Name.Equals("PRJT_NAME"))
                {
                    _selectedElectrodeTypeCode = null;
                    _selectedWipHold = null;
                }

                tabProduct.IsSelected = true;
                SelectEquipmentProductList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEmptyCarrier_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgEmptyCarrier_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgEmptyCarrier_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        #region Method

        private void SelectConveyorCapacitySummary()
        {
            const string bizRuleName = "DA_MCS_SEL_CONVEYOR_CAPACITY_SUMMARY";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQPTID"] = cboConveyor.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    var querySum = bizResult.AsEnumerable().GroupBy(x => new
                        { }).Select(g => new
                    {
                        EquipmentCode = "Total",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        CapacityQtyCount = g.Sum(x => x.Field<Int32>("CAPACITY_QTY")),
                        RecommendQtyCount = g.Sum(x => x.Field<Int32>("RECOMMEND_QTY")),
                        RealCarrierCount = g.Sum(x => x.Field<Int32>("BBN_U_QTY")),
                        EmptyCarrierCount = g.Sum(x => x.Field<Int32>("BBN_E_QTY")),
                        CapacityQtyRate = GetCapacityRate(g.Sum(x => x.Field<Int32>("BBN_U_QTY")), g.Sum(x => x.Field<Int32>("CAPACITY_QTY"))),
                        Count = g.Count()
                    }).ToList();

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        if (querySum.Any())
                        {
                            foreach (var item in querySum)
                            {
                                DataRow newRow = bizResult.NewRow();
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["CAPACITY_QTY"] = item.CapacityQtyCount;
                                newRow["RECOMMEND_QTY"] = item.RecommendQtyCount;
                                newRow["BBN_U_QTY"] = item.RealCarrierCount;
                                newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                                newRow["CAPACITY_RATE"] = item.CapacityQtyRate;
                                bizResult.Rows.Add(newRow);
                            }
                        }
                    }

                    Util.GridSetData(dgCapacitySummary, bizResult, null, true);
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectEquipmentProductSummary()
        {
            const string bizRuleName = "DA_MCS_SEL_EQUIPMENT_PRODUCT_SUMMARY";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = "CNV";
                //dr["EQPTID"] = cboConveyor.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgProductSummary, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectEquipmentProductList()
        {
            const string bizRuleName = "DA_MCS_SEL_EQUIPMENT_PRODUCT_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = "CNV";
                dr["ELTR_TYPE_CODE"] = _selectedElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode; //cboConveyor.SelectedValue;
                dr["PRJT_NAME"] = _selectedProjectName;
                dr["WIPHOLD"] = _selectedWipHold;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgProduct, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectEquipmentEmptyCarrier()
        {
            const string bizRuleName = "DA_MCS_SEL_EQUIPMENT_EMPTY_CARRIER";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                //inTable.Columns.Add("UNITID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = "CNV";
                dr["EQPTID"] = _selectedEquipmentCode;
                //dr["UNITID"] = _selectedEquipmentCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEmptyCarrier, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InitializeCombo()
        {
            // Area 콤보박스
            SetAreaCombo(cboArea);

            // Conveyor 콤보박스
            SetConveyorCombo(cboConveyor);
        }

        private void ClearControl()
        {
            _selectedEquipmentCode = string.Empty;
            _selectedElectrodeTypeCode = string.Empty;
            _selectedProjectName = string.Empty;
            _selectedWipHold = string.Empty;
            txtSection.Text = string.Empty;
            txtRealCarrierCount.Text = string.Empty;

            Util.gridClear(dgCapacitySummary);
            Util.gridClear(dgProductSummary);
            Util.gridClear(dgProduct);
            Util.gridClear(dgEmptyCarrier);
        }

        private double GetCapacityRate(int x, int y)
        {
            double capacityRate = 0;
            if (y.Equals(0)) return capacityRate;
            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_BY_SHOP_CBO";
            string[] arrColumn = { "SYSTEM_ID", "LANGID", "SHOPID", "USERID", "USE_FLAG" };
            string[] arrCondition = { LoginInfo.SYSID, LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.USERID, "Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetConveyorCombo(C1ComboBox cbo)
        {
            string areaCode = cboArea.SelectedValue != null ? cboArea.SelectedValue.GetString() : null;

            const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, areaCode, "CNV", null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

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
