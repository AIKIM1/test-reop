/*************************************************************************************
 Created Date : 2021.02.22
      Creator : 조영대
   Decription : 수동 반송 지시
--------------------------------------------------------------------------------------
 [Change History]
  2021.02.22  조영대 : Initial Created.  
  2021.03.16  조영대 : 창고 콤보박스 코드표기 추가
  2021.04.07  조영대 : LWW 일때 극성 비활성화
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_057 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public MCS001_057()
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
            ApplyPermissions();

            InitializeControls();
            InitializeCombo();
        }

        private void InitializeControls()
        {
            btnReturnOrder.IsEnabled = false;
            btnReturnDelete.IsEnabled = false;
        }

        private void InitializeCombo()
        {
            CommonCombo comboSet = new CommonCombo();
            String[] sFilter = { string.Empty };

            // 동
            comboSet.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            // 창고 유형
            SetWarehouseTypeCombo();

            // 창고 극성
            cboElecType.SetCommonCode("ELTR_TYPE_CODE", CommonCombo.ComboStatus.SELECT);

            // 창고
            SetWarehouseCombo();

            // 생산 Hold
            cboProdHold.SetCommonCode("YORN", CommonCombo.ComboStatus.ALL);

            // QA Hold
            cboQaHold.SetCommonCode("YORN", CommonCombo.ComboStatus.ALL);

            // 반송 목적지 구분 
            cboTargetType.SetCommonCode("MHS_UI_TRF_TARGT_TYPE", CommonCombo.ComboStatus.SELECT, false);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            Loaded -= UserControl_Loaded;
        }
        
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearDataGrid();

            SetWarehouseTypeCombo();
            
            SetWarehouseCombo();
        }

        private void cboWarehouseType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetWarehouseCombo();

            if (cboWarehouseType.GetStringValue().Equals("LWW"))
            {
                spElecType.IsEnabled = false;
                cboElecType.IsEnabled = false;
                cboElecType.SelectedIndex = 0;
            }
            else
            {
                spElecType.IsEnabled = true;
                cboElecType.IsEnabled = true;
            }
        }

        private void cboElecType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetWarehouseCombo();
        }

        private void cboWarehouse_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetTargetWarehouse();
            SetTargetEquipment();
            SetTargetPort(cboWarehouse.GetStringValue());
        }

        private void cboTargetType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            switch (cboTargetType.SelectedValue?.ToString())
            {
                case "M":
                    lblTargetPort.Visibility = cboTargetPort.Visibility = Visibility.Visible;
                    lblTargetWarehouse.Visibility = cboTargetWarehouse.Visibility = Visibility.Collapsed;
                    lblTargetEquipmdent.Visibility = cboTargetEquipmdent.Visibility = Visibility.Collapsed;

                    SetTargetPort(cboWarehouse.GetStringValue());
                    break;
                case "O":
                    lblTargetPort.Visibility = cboTargetPort.Visibility = Visibility.Collapsed;
                    lblTargetWarehouse.Visibility = cboTargetWarehouse.Visibility = Visibility.Visible;
                    lblTargetEquipmdent.Visibility = cboTargetEquipmdent.Visibility = Visibility.Collapsed;

                    SetTargetWarehouse();
                    break;
                case "E":
                    lblTargetPort.Visibility = cboTargetPort.Visibility = Visibility.Visible;
                    lblTargetWarehouse.Visibility = cboTargetWarehouse.Visibility = Visibility.Collapsed;
                    lblTargetEquipmdent.Visibility = cboTargetEquipmdent.Visibility = Visibility.Visible;

                    SetTargetEquipment();
                    SetTargetPort(cboTargetEquipmdent.GetStringValue());
                    break;
            }
        }

        private void cboTargetEquipmdent_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetTargetPort(cboTargetEquipmdent.GetStringValue());
        }

        private void btnReturnOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgSelect.GetRowCount() == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return;
            }

            if (cboTargetType.GetBindValue() == null)
            {
                //%1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", lblTargetType.Text);
                return;
            }

            switch (cboTargetType.SelectedValue?.ToString())
            {
                case "M":
                    if (cboTargetPort.GetBindValue() == null)
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", lblTargetPort.Text);
                        return;
                    }
                    break;
                case "O":
                    if (cboTargetWarehouse.GetBindValue() == null)
                    {   
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", lblTargetWarehouse.Text);
                        return;
                    }
                    break;
                case "E":
                    if (cboTargetEquipmdent.GetBindValue() == null)
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", lblTargetEquipmdent.Text);
                        return;
                    }

                    if (cboTargetPort.GetBindValue() == null)
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", lblTargetPort.Text);
                        return;
                    }
                    break;
            }

            ReturnOrder();
        }

        private void btnReturnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSelect.GetRowCount() == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return;
            }

            Util.MessageConfirm("MCS1005", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ReturnDelete();
                }
            });
          
        }
        
        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMain.SelectedIndex == 0)
            {
                dtpStart.IsEnabled = false;

                lblLotId.Visibility = txtLotId.Visibility = Visibility.Visible;
                lblCstId.Visibility = txtCstId.Visibility = Visibility.Visible;
                lblProdHold.Visibility = cboProdHold.Visibility = Visibility.Visible;
                lblQaHold.Visibility = cboQaHold.Visibility = Visibility.Visible;
            }
            else
            {
                dtpStart.IsEnabled = true;

                lblLotId.Visibility = txtLotId.Visibility = Visibility.Collapsed;
                lblCstId.Visibility = txtCstId.Visibility = Visibility.Collapsed;
                lblProdHold.Visibility = cboProdHold.Visibility = Visibility.Collapsed;
                lblQaHold.Visibility = cboQaHold.Visibility = Visibility.Collapsed;
            }
        }
        
        private void dgProduct_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (dgProduct.CurrentRow == null || dgProduct.SelectedIndex < 0) return;
                if (!dgProduct.IsClickedCell()) return;

                switch (dgProduct.CurrentColumn.Name)
                {
                    case "LOTID":
                        object[] parameters = new object[1];
                        parameters[0] = dgProduct.GetValue("LOTID");
                        this.FrameOperation.OpenMenu("SFU010160050", true, parameters);
                        break;
                    case "CHK":
                        break;
                    default:
                        tabMain.SelectedIndex = 1;

                        SelectHistoryList(true);
                        break;
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
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgProduct_CheckedChanged(object sender, RoutedEventArgs e)
        {
            int rowIndex = ((DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            DataRow drSelect = dgProduct.GetDataRow(rowIndex);
            if (drSelect == null) return;

            DataTable dtSelect = ((DataView)dgSelect.ItemsSource).Table;

            if (dgProduct.IsCheckedRow("CHK", rowIndex))
            {
                object[] rowArray = drSelect.ItemArray;
                rowArray[rowArray.Length - 1] = false;

                DataRow findRow = dgSelect.FindDataRow("ROW_NUM", drSelect["ROW_NUM"]).FirstOrDefault();
                if (findRow == null)
                {
                    dtSelect.Rows.Add(rowArray);
                }
            }
            else
            {
                DataRow findRow = dgSelect.FindDataRow("ROW_NUM", drSelect["ROW_NUM"]).FirstOrDefault();
                findRow?.Delete();
            }

            if (!dgSelect.IsCheckedRow("CHK"))
            {
                btnReturnOrder.IsEnabled = false;
                btnReturnDelete.IsEnabled = false;
            }
        }

        private void dgSelect_CheckedChanged(object sender, RoutedEventArgs e)
        {
            int rowIndex = ((DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;

            if (dgSelect.IsCheckedRow("CHK", rowIndex))
            {
                dgSelect.SelectRow(rowIndex);

                if (dgSelect.GetStringValue(rowIndex, "CMD_STAT_NAME").Equals(string.Empty))
                {
                    btnReturnOrder.IsEnabled = true;
                    btnReturnDelete.IsEnabled = false;
                }
                else
                {
                    btnReturnOrder.IsEnabled = false;
                    btnReturnDelete.IsEnabled = true;
                }
            }     
        }

        private void dgSelect_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {            
        }

        private void dgHistory_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid grid = sender as C1DataGrid;
                if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
                {
                    var _mergeList = new List<DataGridCellsRange>();

                    int saveCmdSeq = Util.StringToInt(grid.GetValue(0, "CMD_SEQNO").ToString());
                    int mergeStart = 0, mergeEnd = 0;
                    for (int row = 0; row < grid.Rows.Count; row++)
                    {
                        if (grid.Rows.Count > 1 && row.Equals(grid.Rows.Count - 1) &&
                            Util.NVC(grid.GetValue(row - 1, "CMD_SEQNO")) != Util.NVC(grid.GetValue(row, "CMD_SEQNO")))
                        {
                            break;
                        }

                        if (!saveCmdSeq.Equals(Util.StringToInt(grid.GetValue(row, "CMD_SEQNO").ToString())) ||
                            row.Equals(grid.Rows.Count - 1))
                        {
                            mergeEnd = row.Equals(grid.Rows.Count - 1) ? row : row - 1;

                            if (mergeStart < mergeEnd)
                            {
                                foreach (C1.WPF.DataGrid.DataGridColumn dgCol in grid.Columns)
                                {
                                    switch (dgCol.Name)
                                    {
                                        case "CST_LOAD_LOCATION_CODE":
                                        case "CSTID":
                                        case "LOTID":
                                            break;
                                        default:
                                            _mergeList.Add(new DataGridCellsRange(grid.GetCell(mergeStart, dgCol.Index), grid.GetCell(mergeEnd, dgCol.Index)));
                                            break;
                                    }

                                }
                            }
                            mergeStart = row;
                            saveCmdSeq = Util.StringToInt(grid.GetValue(row, "CMD_SEQNO").ToString());
                        }
                    }
                    foreach (var range in _mergeList)
                    {
                        e.Merge(range);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnReturnOrder,
                btnReturnDelete
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void ClearDataGrid()
        {
            dgProduct.ClearRows();
            dgHistory.ClearRows();
        }

        private void SetWarehouseTypeCombo()
        {
            const string bizRuleName = "DA_MCS_SEL_AREA_COM_CODE_CSTTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue(), "Y", null };

            cboWarehouseType.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT);
        }

        private void SetWarehouseCombo()
        {
            string stockerType = string.IsNullOrEmpty(cboWarehouseType.GetStringValue()) ? null : cboWarehouseType.GetStringValue();
            string electrodeType = string.IsNullOrEmpty(cboElecType.GetStringValue()) ? null : cboElecType.GetStringValue();

            const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue(), stockerType, electrodeType };

            cboWarehouse.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, true);
        }

        private void SetTargetEquipment()
        {
            string equipment = cboWarehouse.GetStringValue();

            const string bizRuleName = "DA_MHS_SEL_TRF_TARGET_EQPT_CBO";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, equipment.Equals(string.Empty) ? null : equipment };

            cboTargetEquipmdent.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT);
        }

        private void SetTargetPort(string equipment)
        {
            const string bizRuleName = "DA_MHS_SEL_TRF_TARGET_PORT_CBO";

            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, equipment.Equals(string.Empty) ? null : equipment };

            cboTargetPort.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT);
        }

        private void SetTargetWarehouse()
        {
            const string bizRuleName = "DA_MHS_SEL_TRF_TARGET_STK_CBO";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, cboWarehouse.GetStringValue() };

            cboTargetWarehouse.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT);
        }

        private void Refresh()
        {
            try
            {
                if (!ChkValidation())
                {                 
                    return;
                }

                if (tabMain.SelectedIndex == 0)
                {
                    SelectWareHouseProductList();
                }
                else
                {
                    SelectHistoryList(false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
              
        private bool ChkValidation()
        {
            if (cboArea.SelectedValue==null || cboArea.SelectedValue.Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                cboArea.Focus();
                return false;
            }

            if (cboWarehouseType.SelectedValue == null || cboWarehouseType.SelectedValue.Equals("SELECT"))
            {
                // 창고 유형을 선택하세요.
                Util.MessageValidation("SFU4925", lblWarehouseType.Text);
                cboWarehouseType.Focus();
                return false;
            }

            if (cboElecType.IsEnabled)
            {
                if (cboElecType.SelectedValue == null || cboElecType.SelectedValue.Equals("SELECT"))
                {
                    // 전극을 선택하세요.
                    Util.MessageValidation("SFU4925", lblElecType.Text);
                    cboElecType.Focus();
                    return false;
                }
            }

            if (cboWarehouse.SelectedValue == null || cboWarehouse.SelectedValue.Equals("SELECT"))
            {
                // 창고를 선택하세요.
                Util.MessageValidation("SFU4925", lblWarehouse.Text);
                cboWarehouse.Focus();
                return false;
            }

            if (!Util.IsNVC(txtLotId.Text) && txtLotId.Text.Length < 3)
            {
                // [%1] 자리수 이상 입력하세요.
                Util.MessageValidation("SFU4342", "3");
                txtLotId.Focus();
                txtLotId.SelectAll();
                return false;
            }

            if (!Util.IsNVC(txtCstId.Text) && txtCstId.Text.Length < 3)
            {
                // [%1] 자리수 이상 입력하세요.
                Util.MessageValidation("SFU4342", "3");
                txtCstId.Focus();
                txtCstId.SelectAll();
                return false;
            }

            return true;
        }

        private void SelectWareHouseProductList()
        {
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
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("QMSHOLD", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.GetBindValue();
                dr["EQGRID"] = cboWarehouseType.GetBindValue();
                dr["ELTR_TYPE_CODE"] = cboElecType.GetBindValue();
                dr["EQPTID"] = cboWarehouse.GetBindValue();
                dr["LOTID"] = txtLotId.GetBindValue();
                dr["WIPHOLD"] = cboProdHold.GetBindValue();
                dr["QMSHOLD"] = cboQaHold.GetBindValue();
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_MCS_SEL_WAREHOUSE_PRODUCT_LIST_MANUAL", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtResult = bizResult;
                    if (txtCstId.Text.Length > 0)
                    {
                        DataView dv = bizResult.DefaultView;
                        dv.RowFilter = "CSTID = '" + txtCstId.Text + "'";
                        dtResult = dv.ToTable();
                    }

                    if (!dtResult.Columns.Contains("CHK"))
                    {
                        dtResult.Columns.Add("CHK", typeof(bool));
                        dtResult.Select().ToList<DataRow>().ForEach(r => r["CHK"] = false);
                    }

                    dgProduct.SetItemsSource(dtResult, FrameOperation, true);

                    dgSelect.SetItemsSource(dtResult.Clone(), FrameOperation, true);

                    btnReturnOrder.IsEnabled = false;
                    btnReturnDelete.IsEnabled = false;

                    cboTargetType.SelectedIndex = 0;
                    cboTargetPort.SelectedIndex = 0;
                    cboTargetWarehouse.SelectedIndex = 0;
                    cboTargetEquipmdent.SelectedIndex = 0;

                    lblTargetPort.Visibility = cboTargetPort.Visibility = Visibility.Collapsed;
                    lblTargetWarehouse.Visibility = cboTargetWarehouse.Visibility = Visibility.Collapsed;
                    lblTargetEquipmdent.Visibility = cboTargetEquipmdent.Visibility = Visibility.Collapsed;
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
                INDATA.Columns.Add("TRF_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("TRF_CLSS_CODE", typeof(string));
                INDATA.Columns.Add("FROM_EQPTID", typeof(string));
                INDATA.Columns.Add("TERM_TYPE", typeof(string));
                INDATA.Columns.Add("FROM_DATE", typeof(string));
                INDATA.Columns.Add("TO_DATE", typeof(string));
                INDATA.Columns.Add("CMD_SEQNO", typeof(string));
                INDATA.Columns.Add("EQGRID", typeof(string));
                INDATA.Columns.Add("STK_ELTR_TYPE_CODE", typeof(string));

                DataRow inData = INDATA.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["AREAID"] = cboArea.SelectedValue;  
                inData["TRF_CLSS_CODE"] = "U";
                inData["FROM_EQPTID"] = cboWarehouse.GetBindValue();
                if (isDoubleClick && dgProduct.CurrentRow != null)
                {
                    inData["CMD_SEQNO"] = dgProduct.GetValue("CMD_SEQNO");
                }
                else
                {
                    inData["FROM_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                    inData["TO_DATE"] = dtpStart.SelectedDateTime.ToString("yyyyMMdd");
                }
                inData["EQGRID"] = cboWarehouseType.GetBindValue();
                inData["STK_ELTR_TYPE_CODE"] = cboElecType.GetBindValue();
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

        private void ReturnOrder()
        {
            try
            {
                if (dgSelect.CurrentRow == null || dgSelect.SelectedIndex < 0)
                {
                    return;
                }

                string destEqptId = string.Empty;
                string destLocId = string.Empty;
                switch (cboTargetType.SelectedValue?.ToString())
                {
                    case "M":
                        destEqptId = cboWarehouse.GetStringValue();
                        destLocId = cboTargetPort.GetStringValue("LOCID");
                        break;
                    case "O":
                        destEqptId = cboTargetWarehouse.GetStringValue("TRGT_EQPT_ID");
                        destLocId = cboTargetWarehouse.GetStringValue("TRGT_PORT_ID");
                        break;
                    case "E":
                        destEqptId = cboTargetEquipmdent.GetStringValue();
                        destLocId = cboTargetPort.GetStringValue("LOCID");
                        break;
                    default:
                        return;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("SRC_EQPTID", typeof(string));
                inTable.Columns.Add("DST_EQPTID", typeof(string));
                inTable.Columns.Add("DST_LOCID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                inTable.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));
                
                DataRow inRow = inTable.NewRow();
                inRow["CARRIERID"] = dgSelect.GetStringValue("SKID_ID");
                inRow["SRC_EQPTID"] = dgSelect.GetStringValue("EQPTID");
                inRow["DST_EQPTID"] = destEqptId;
                inRow["DST_LOCID"] = destLocId;
                inRow["UPDUSER"] = LoginInfo.USERID;
                inRow["TRF_CAUSE_CODE"] = null;
                inRow["MANL_TRF_CAUSE_CNTT"] = null;

                inTable.Rows.Add(inRow);
                
                new ClientProxy().ExecuteService("BR_MHS_REG_TRF_CMD_BY_UI", "IN_REQ_TRF_INFO", null, inTable, (result, ex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (ex != null)
                        {
                            Util.MessageException(ex);

                            Refresh();
                            return;
                        }

                        // 정상 처리 되었습니다
                        Util.MessageValidation("SFU1889");

                    }
                    catch (Exception ex2)
                    {
                        Util.MessageException(ex2);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private void ReturnDelete()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MHS_REG_TRF_CMD_CANCEL_BY_UI";

                DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                inTable.Columns.Add("CARRIERID", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CARRIERID"] = dgSelect.GetStringValue("SKID_ID");
                newRow["UPDUSER"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "IN_REQ_TRF_INFO", null, inTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
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



        #endregion
        
    }
}