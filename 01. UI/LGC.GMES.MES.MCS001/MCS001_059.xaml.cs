/*************************************************************************************
 Created Date : 2021.04.15
      Creator : 조영대
   Decription : 창고 재공 현황 (w/시운전)
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.15  조영대 차장 : Initial Created. (Copy by MCS001_040)
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_059 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string _selectedEquipmentCode;
        private string _selectedParentsElectrodeTypeCode;
        private string _selectedElectrodeTypeCode;
        private string _selectedProjectName;
        private string _selectedWipHold;
        private DataTable _dtWareHouseCapacity;

        public MCS001_059()
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
            List<Button> listAuth = new List<Button> { };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeCombo();
            MakeWareHouseCapacityTable();
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            cboStockerType.SelectedValueChanged -= cboStockerType_SelectedValueChanged;
            SetStockerTypeCombo(cboStockerType);
            cboStockerType.SelectedValueChanged += cboStockerType_SelectedValueChanged;

            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;


            dgLamiCapacitySummary.Visibility = Visibility.Collapsed;
            dgCapacitySummary.Visibility = Visibility.Visible;
            dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;

            if (cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgLamiCapacitySummary.Visibility = Visibility.Visible;
                dgCapacitySummary.Visibility = Visibility.Collapsed;
            }
            else if (cboStockerType.SelectedValue.GetString() == "NWW")
            {
                dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
            }
            else if (cboStockerType.SelectedValue.GetString() == "JRW")
            {
                dgProduct.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                dgProduct.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
            }
            else if (cboStockerType.SelectedValue.GetString() == "PCW")
            {
                dgProduct.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Visible;
            }

            if (cboStockerType.SelectedValue.GetString() == "NPW" || cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
            }
            
        }

        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();

            if (cboStockerType.SelectedValue.GetString() == "LWW")
            {
                SelectWareHouseLamiSummary();
            }
            else
            {
                SelectWareHouseSummary();
            }

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
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")), "XXXXXXXXXX") && !string.Equals(e.Cell.Column.Name, "ELTR_TYPE_NAME"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Aqua");
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

        private void dgCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                if (cell.Column.Name.Equals("EMPTY_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    tabEmptyCarrier.IsSelected = true;
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
                }
                else if (cell.Column.Name.Equals("PILOT_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    tabTrialRunCarrier.IsSelected = true;

                    SelectWareHouseTrialRunCarrierList();
                }
                else if (cell.Column.Name.Equals("ERROR_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString(), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList();
                }
                else
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") || string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (cell.Column.Name.Equals("ELTR_TYPE_NAME"))
                    {
                        _selectedEquipmentCode = null;
                        _selectedProjectName = null;
                        _selectedWipHold = null;

                        if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                        {
                            _selectedElectrodeTypeCode = null;
                        }
                        else
                        {
                            _selectedElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                        }
                    }
                    else if (cell.Column.Name.Equals("EQPTNAME") || cell.Column.Name.Equals("RACK_MAX_QTY") || cell.Column.Name.Equals("RACK_RATE"))
                    {
                        //_selectedEquipmentCode = null;
                        _selectedProjectName = null;
                        _selectedWipHold = null;

                        if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                        {
                            _selectedElectrodeTypeCode = null;
                        }
                        else
                        {
                            _selectedElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                        }
                    }
                    else
                    {
                        if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                        {
                            _selectedElectrodeTypeCode = null;
                            _selectedProjectName = null;
                        }
                        else if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX"))
                        {
                            _selectedElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                            _selectedProjectName = null;
                        }
                        else
                        {
                            _selectedElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                            _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                        }

                        if (cell.Column.Name.Equals("HOLD_QTY"))
                        {
                            _selectedWipHold = "Y";
                        }
                        else if (cell.Column.Name.Equals("LOT_QTY"))
                        {
                            _selectedWipHold = "N";
                        }
                        else
                        {
                            _selectedWipHold = null;
                        }
                    }

                    Util.gridClear(dgProduct);
                    tabProduct.IsSelected = true;

                    SelectWareHouseProductList();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCapacitySummary_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE")
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        Count = g.Count()
                    }).ToList();

                    string previewElectrodeTypeCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index + 1)));
                        }
                        else
                        {
                            foreach (var item in query)
                            {
                                int rowIndex = i;
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString() == item.ElectrodeTypeCode && previewElectrodeTypeCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ELTR_TYPE_NAME"].Index)));
                                }
                            }
                        }
                        previewElectrodeTypeCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString();
                    }

                    DataTable dt1 = ((DataView)dg.ItemsSource).Table;
                    var query1 = dt.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE"),
                        equipmentCode = x.Field<string>("EQPTID")
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        EquipmentCode = g.Key.equipmentCode,
                        Count = g.Count()
                    }).OrderBy(o => o.ElectrodeTypeCode).ThenBy(t => t.EquipmentCode).ToList();

                    string previewEquipmentCode = string.Empty;

                    for (int j = 0; j < dg.Rows.Count; j++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString() == "XXXXXXXXXX" ||
                            DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString() == "ZZZZZZZZZZ")
                        {
                            continue;
                        }

                        foreach (var item in query1)
                        {
                            int rowIndex = j;

                            if (DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString() == item.EquipmentCode 
                                && previewEquipmentCode != DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString()
                                && DataTableConverter.GetValue(dg.Rows[j].DataItem, "ELTR_TYPE_CODE").GetString() == item.ElectrodeTypeCode
                                )
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["EQPTNAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EQPTNAME"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["RACK_MAX_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_MAX_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["EMPTY_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EMPTY_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["PILOT_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PILOT_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["ERROR_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ERROR_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dg.GetCell(j, dg.Columns["RACK_RATE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_RATE"].Index)));
                            }
                        }
                        previewEquipmentCode = DataTableConverter.GetValue(dg.Rows[j].DataItem, "EQPTID").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLamiCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
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

        private void dgLamiCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgLamiCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                if (cell.Column.Name.Equals("EMPTY_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    _selectedElectrodeTypeCode = null;

                    tabEmptyCarrier.IsSelected = true;
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList();
                }
                else if (cell.Column.Name.Equals("PILOT_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    _selectedElectrodeTypeCode = null;

                    tabTrialRunCarrier.IsSelected = true;

                    SelectWareHouseTrialRunCarrierList();
                }
                else if (cell.Column.Name.Equals("ERROR_QTY"))
                {
                    _selectedElectrodeTypeCode = null;

                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList();
                }
                else
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                        _selectedProjectName = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                        _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                    }

                    if (cell.Column.Name.Equals("EQPTNAME"))
                    {
                        _selectedElectrodeTypeCode = null;
                        _selectedWipHold = null;
                        _selectedProjectName = null;
                    }
                    else
                    {
                        if (cell.Column.Name.Equals("LOT_QTY_C"))
                        {
                            _selectedElectrodeTypeCode = "C";
                            _selectedWipHold = "N";
                        }
                        else if (cell.Column.Name.Equals("HOLD_QTY_C"))
                        {
                            _selectedElectrodeTypeCode = "C";
                            _selectedWipHold = "Y";
                        }
                        else if (cell.Column.Name.Equals("LOT_QTY_A"))
                        {
                            _selectedElectrodeTypeCode = "A";
                            _selectedWipHold = "N";
                        }
                        else if (cell.Column.Name.Equals("HOLD_QTY_A"))
                        {
                            _selectedElectrodeTypeCode = "A";
                            _selectedWipHold = "Y";
                        }
                        else
                        {
                            _selectedElectrodeTypeCode = null;
                            _selectedWipHold = null;
                        }
                    }

                    Util.gridClear(dgProduct);
                    tabProduct.IsSelected = true;

                    SelectWareHouseProductList();
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void dgLamiCapacitySummary_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        equipmentCode = x.Field<string>("EQPTID")
                    }).Select(g => new
                    {
                        EquipmentCode = g.Key.equipmentCode,
                        Count = g.Count()
                    }).ToList();

                    string previewEquipmentCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTNAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                        {
                            //e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPTNAME"].Index), dg.GetCell(i, dg.Columns["EQPTNAME"].Index + 1)));
                        }
                        else
                        {
                            foreach (var item in query)
                            {
                                int rowIndex = i;
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString() == item.EquipmentCode && previewEquipmentCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPTNAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_MAX_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_MAX_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EMPTY_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EMPTY_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["PILOT_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["PILOT_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ERROR_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ERROR_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_RATE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_RATE"].Index)));
                                }
                            }
                        }
                        previewEquipmentCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString();
                    }

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("LOTID"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID"));
                    this.FrameOperation.OpenMenu("SFU010160050", true, parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
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

        private void dgProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void SelectWareHouseSummary()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_SUMMARY_ADD_PILOT";
            try
            {
                DataTable dtResult = new DataTable();

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }
                    
                    var queryBase = bizResult.AsEnumerable()
                        .Select(row => new {
                            ElectrodeTypeCode = row.Field<string>("ELTR_TYPE_CODE"),
                            ElectrodeTypeName = row.Field<string>("ELTR_TYPE_NAME"),
                            EquipmentCode = row.Field<string>("EQPTID"),
                            EquipmentName = row.Field<string>("EQPTNAME"),
                            RackMaxQty = row.Field<Int32>("RACK_MAX_QTY"),
                            EmptyQty = row.Field<Int32>("EMPTY_QTY"),
                            PilotQty = row.Field<Int32>("PILOT_QTY"),
                            ErrorQty = row.Field<Int32>("ERROR_QTY"),
                            RackRate = row.Field<decimal>("RACK_RATE"),
                            SumCarrierCount = row.Field<Int32>("RACK_QTY"),
                        }).Distinct();


                    // 극성별 소계
                    var query = bizResult.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE"),
                        electrodeTypeName = x.Field<string>("ELTR_TYPE_NAME"),
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        ElectrodeTypeName = g.Key.electrodeTypeName,
                        RackMaxCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.RackMaxQty).Sum(),
                        EmptyCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.EmptyQty).Sum(),
                        PilotCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.PilotQty).Sum(),
                        ErrorCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.ErrorQty).Sum(),
                        SumCarrierCount = queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.SumCarrierCount).Sum(),
                        RackRate = GetRackRate(queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.SumCarrierCount).Sum(), queryBase.AsQueryable().Where(x => x.ElectrodeTypeCode == g.Key.electrodeTypeCode).Select(s => s.RackMaxQty).Sum()),
                        LotCount = g.Sum(x => x.Field<Int32>("LOT_QTY")),
                        HoldCount = g.Sum(x => x.Field<Int32>("HOLD_QTY")),
                        ProjectName = "",
                        EquipmentCode = "XXXXXXXXXX",
                        EquipmentName = g.Key.electrodeTypeName + "  " + ObjectDic.Instance.GetObjectName("소계"),
                        Count = g.Count() 
                    }).ToList();

                    //합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new
                    {}).Select(g => new
                    {
                        ElectrodeTypeCode = "ZZZZZZZZZZ",
                        ElectrodeTypeName = ObjectDic.Instance.GetObjectName("합계"),
                        RackMaxCount = queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum(),
                        LotCount = g.Sum(x => x.Field<Int32>("LOT_QTY")),
                        HoldCount = g.Sum(x => x.Field<Int32>("HOLD_QTY")),
                        EmptyCount = queryBase.AsQueryable().Select(s => s.EmptyQty).Sum(),
                        PilotCount = queryBase.AsQueryable().Select(s => s.PilotQty).Sum(),
                        ErrorCount = queryBase.AsQueryable().Select(s => s.ErrorQty).Sum(),
                        SumCarrierCount = queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum(),
                        RackRate = GetRackRate(queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum(), queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum()),
                        ProjectName = "",
                        EquipmentCode = "ZZZZZZZZZZ",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        Count = g.Count()
                    }).ToList();

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            DataRow newRow = _dtWareHouseCapacity.NewRow();
                            newRow["ELTR_TYPE_CODE"] = bizResult.Rows[i]["ELTR_TYPE_CODE"];
                            newRow["ELTR_TYPE_NAME"] = bizResult.Rows[i]["ELTR_TYPE_NAME"];
                            newRow["EQPTID"] = bizResult.Rows[i]["EQPTID"];
                            newRow["EQPTNAME"] = bizResult.Rows[i]["EQPTNAME"];
                            newRow["PRJT_NAME"] = bizResult.Rows[i]["PRJT_NAME"];
                            newRow["RACK_MAX_QTY"] = bizResult.Rows[i]["RACK_MAX_QTY"];
                            newRow["LOT_QTY"] = bizResult.Rows[i]["LOT_QTY"];
                            newRow["HOLD_QTY"] = bizResult.Rows[i]["HOLD_QTY"];
                            newRow["EMPTY_QTY"] = bizResult.Rows[i]["EMPTY_QTY"];
                            newRow["PILOT_QTY"] = bizResult.Rows[i]["PILOT_QTY"];
                            newRow["ERROR_QTY"] = bizResult.Rows[i]["ERROR_QTY"];
                            newRow["RACK_QTY"] = bizResult.Rows[i]["RACK_QTY"];
                            newRow["RACK_RATE"] = bizResult.Rows[i]["RACK_RATE"];
                            
                            _dtWareHouseCapacity.Rows.Add(newRow);
                        }

                        if (query.Any())
                        {
                            foreach (var item in query)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["PRJT_NAME"] = item.ProjectName;
                                newRow["RACK_MAX_QTY"] = item.RackMaxCount;
                                newRow["LOT_QTY"] = item.LotCount;
                                newRow["HOLD_QTY"] = item.HoldCount;
                                newRow["EMPTY_QTY"] = item.EmptyCount;
                                newRow["PILOT_QTY"] = item.PilotCount;
                                newRow["ERROR_QTY"] = item.ErrorCount;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }

                        if (querySum.Any())
                        {
                            foreach (var item in querySum)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["PRJT_NAME"] = item.ProjectName;
                                newRow["RACK_MAX_QTY"] = item.RackMaxCount;
                                newRow["LOT_QTY"] = item.LotCount;
                                newRow["HOLD_QTY"] = item.HoldCount;
                                newRow["EMPTY_QTY"] = item.EmptyCount;
                                newRow["PILOT_QTY"] = item.PilotCount;
                                newRow["ERROR_QTY"] = item.ErrorCount;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }
                    }

                    if (CommonVerify.HasTableRow(_dtWareHouseCapacity))
                    {
                        dtResult = (from t in _dtWareHouseCapacity.AsEnumerable()
                            orderby t.Field<string>("ELTR_TYPE_CODE") ascending, t.Field<string>("EQPTID")
                            select t).CopyToDataTable();
                    }
                    else
                    {
                        dtResult = bizResult;
                    }

                    Util.GridSetData(dgCapacitySummary, dtResult, null, true);
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetSubSum(IEnumerable<object> queryBase, string column)
        {



        }

        private void SelectWareHouseLamiSummary()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_SUMMARY_LWW_ADD_PILOT";
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
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    // 라미대기 창고 인 경우 합계를 구하기 위해서 용량, 공Carrier, 오류Carrier, 적재율을 Distinct 처리 해야 함.
                    var queryBase = bizResult.AsEnumerable()
                        .Select(row => new {
                            EquipmentCode = row.Field<string>("EQPTID"),
                            EquipmentName = row.Field<string>("EQPTNAME"),
                            RackMaxQty = row.Field<Int32>("RACK_MAX_QTY"),
                            EmptyQty = row.Field<Int32>("EMPTY_QTY"),
                            PilotQty = row.Field<Int32>("PILOT_QTY"),
                            ErrorQty = row.Field<Int32>("ERROR_QTY"),
                            RackRate = row.Field<decimal>("RACK_RATE"),
                            SumCarrierCount = row.Field<Int32>("RACK_QTY"),
                        }).Distinct();

                    //합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new{ }).Select(g => new
                    {
                        EquipmentCode = "ZZZZZZZZZZ",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        ProjectName = "",
                        LotCountCathode = g.Sum(x => x.Field<Int32>("LOT_QTY_C")),
                        HoldCountCathode = g.Sum(x => x.Field<Int32>("HOLD_QTY_C")),
                        LotCountAnode = g.Sum(x => x.Field<Int32>("LOT_QTY_A")),
                        HoldCountAnode = g.Sum(x => x.Field<Int32>("HOLD_QTY_A")),
                        EmptyCount = queryBase.AsQueryable().Select(s => s.EmptyQty).Sum(),
                        PilotCount = queryBase.AsQueryable().Select(s => s.PilotQty).Sum(),
                        ErrorCount = queryBase.AsQueryable().Select(s => s.ErrorQty).Sum(),
                        RackMaxCount = queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum(),
                        SumCarrierCount = g.Sum(x => x.Field<Int32>("RACK_QTY")),
                        RackRate = GetRackRate(queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum(), queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum()),
                        Count = g.Count()
                        }).FirstOrDefault();

                    if (querySum != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = querySum.EquipmentCode;
                        newRow["EQPTNAME"] = querySum.EquipmentName;
                        newRow["PRJT_NAME"] = querySum.ProjectName;
                        newRow["RACK_MAX_QTY"] = querySum.RackMaxCount;
                        newRow["LOT_QTY_C"] = querySum.LotCountCathode;
                        newRow["HOLD_QTY_C"] = querySum.HoldCountCathode;
                        newRow["LOT_QTY_A"] = querySum.LotCountAnode;
                        newRow["HOLD_QTY_A"] = querySum.HoldCountAnode;
                        newRow["EMPTY_QTY"] = querySum.EmptyCount;
                        newRow["PILOT_QTY"] = querySum.PilotCount;
                        newRow["ERROR_QTY"] = querySum.ErrorCount;
                        newRow["RACK_RATE"] = querySum.RackRate;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgLamiCapacitySummary, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void SelectWareHouseProductList()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
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
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;//cboStocker.SelectedValue;
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

        private void SelectWareHouseEmptyCarrier()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPTY_CARRIER";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedElectrodeTypeCode;
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

                    Util.GridSetData(dgEmptyCarrier, bizResult, null, true);
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
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedElectrodeTypeCode;
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

                    Util.GridSetData(dgCarrierList, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseTrialRunCarrierList()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_TRIAL_RUN_CARRIER_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedElectrodeTypeCode;
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

                    Util.GridSetData(dgTrialRunCarrier, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectErrorCarrierList()
        {
            const string bizRuleName = "DA_MCS_SEL_WAREHOUSE_NOREAD_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedElectrodeTypeCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgErrorCarrier, bizResult, null, true);
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

            // 창고유형 콤보박스
            SetStockerTypeCombo(cboStockerType);

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);
        }

        private void MakeWareHouseCapacityTable()
        {
            _dtWareHouseCapacity = new DataTable();
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_NAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTID", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTNAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("RACK_MAX_QTY", typeof(decimal));
            _dtWareHouseCapacity.Columns.Add("PRJT_NAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("LOT_QTY", typeof(decimal));       // 양품 Carrier수
            _dtWareHouseCapacity.Columns.Add("HOLD_QTY", typeof(decimal));      // HOLD Carrier수
            _dtWareHouseCapacity.Columns.Add("EMPTY_QTY", typeof(decimal));     // 공Carrier수
            _dtWareHouseCapacity.Columns.Add("PILOT_QTY", typeof(decimal)); // 시운전Carrier수
            _dtWareHouseCapacity.Columns.Add("ERROR_QTY", typeof(decimal));     // 오류Carrier수
            _dtWareHouseCapacity.Columns.Add("RACK_QTY", typeof(decimal));      // 총Carrier수(실+공)
            _dtWareHouseCapacity.Columns.Add("RACK_RATE", typeof(double));      // 적재율

        }

        private double GetRackRate(int x, int y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception )
            {
                return 0;
            }
        }

        

        private void ClearControl()
        {
            _selectedEquipmentCode = string.Empty;
            _selectedParentsElectrodeTypeCode = string.Empty;
            _selectedElectrodeTypeCode = string.Empty;
            _selectedProjectName = string.Empty;
            _selectedWipHold = string.Empty;

            _dtWareHouseCapacity?.Clear();

            Util.gridClear(dgCapacitySummary);
            Util.gridClear(dgProduct);
            Util.gridClear(dgEmptyCarrier);
            Util.gridClear(dgCarrierList);
            Util.gridClear(dgTrialRunCarrier);
            Util.gridClear(dgErrorCarrier);
        }

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_BY_SHOP_CBO";
            string[] arrColumn = { "SYSTEM_ID", "LANGID", "SHOPID", "USERID", "USE_FLAG" };
            string[] arrCondition = { LoginInfo.SYSID, LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.USERID,"Y" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_AREA_COM_CODE_CSTTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), "Y", null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
            string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

            const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), stockerType, electrodeType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
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
