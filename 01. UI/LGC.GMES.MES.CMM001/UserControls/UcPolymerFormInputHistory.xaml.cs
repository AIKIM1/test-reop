using System;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Reflection;
using System.Data;
using System.Windows;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcFormInput.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcPolymerFormInputHistory
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private CheckBoxHeaderType _inBoxHeaderType;

        public UserControl UcParentControl;

        public C1DataGrid DgTray { get; set; }
        public C1DataGrid DgPallet { get; set; }
        public C1DataGrid DgMaterial { get; set; }
        public C1DataGrid DgMaterialHistory { get; set; }

        public Button ButtonPalletRemainWait { get; set; }
        public TextBox TextMaterialID { get; set; }

        public string ProdLotId { get; set; }
        public string ProdWipSeq { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string ProdAssyLotId { get; set; }
        public string ProdJobStartDT { get; set; }

        public UcPolymerFormInputHistory()
        {
            InitializeComponent();
            SetControl();
            SetButtons();
            SetCombo();
        }

        #endregion

        #region Initialize
        private void SetControl()
        {
            DgTray = dgTray;
            DgPallet = dgPallet;
            DgMaterial = dgMaterial;
            DgMaterialHistory = dgMaterialHistory;

            TextMaterialID = txtMaterialID;

            _inBoxHeaderType = CheckBoxHeaderType.Zero;
        }

        private void SetButtons()
        {
            ButtonPalletRemainWait = btnPalletRemainWait;
        }

        public void SetCombo()
        {
        }
        #endregion

        #region Event

        #region 투입 Tray Tab
        private void btnInputTraySearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationInputTraySearch()) return;

            SelectInputTrayList();
        }

        #endregion

        #region 투입 Inbox Tab
        private void dgPallet_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            //try
            //{
            //    this.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        C1DataGrid dg = sender as C1DataGrid;
            //        CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
            //        if (chk != null)
            //        {
            //            switch (Convert.ToString(e.Cell.Column.Name))
            //            {
            //                case "CHK":
            //                    if (dg != null)
            //                    {
            //                        var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
            //                        if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                                                 dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                                                 checkBox.IsChecked.HasValue &&
            //                                                 !(bool)checkBox.IsChecked))
            //                        {
            //                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
            //                            chk.IsChecked = true;

            //                            for (int idx = 0; idx < dg.Rows.Count; idx++)
            //                            {
            //                                if (e.Cell.Row.Index != idx)
            //                                {
            //                                    if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
            //                                        dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
            //                                        (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
            //                                    {
            //                                        var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
            //                                        if (box != null)
            //                                            box.IsChecked = false;
            //                                    }
            //                                    DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
            //                                }
            //                            }
            //                        }
            //                        else
            //                        {
            //                            var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
            //                            if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                                              dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                                              o.IsChecked.HasValue &&
            //                                              (bool)o.IsChecked))
            //                            {
            //                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
            //                                chk.IsChecked = false;
            //                            }
            //                        }
            //                    }
            //                    break;
            //            }

            //            if (dg?.CurrentCell != null)
            //                dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
            //            else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
            //                dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
            //        }

            //    }));
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {
            if (txtPalletID == null) return;

            txtPalletID.SelectAll();
            txtPalletID.Focus();
        }

        private void rdoInbox_Checked(object sender, RoutedEventArgs e)
        {
            if (txtPalletID == null) return;

            txtPalletID.SelectAll();
            txtPalletID.Focus();
        }

        private void txtPalletID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationPalletInput()) return;

                // 투입
                if ((bool)rdoPallet.IsChecked)
                {
                    // 대차투입
                    InputCart();
                }
                else
                {
                    // Inbox 투입
                    InputPallet();
                }
            }
        }

        private void btnPalletInput_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationPalletInput()) return;

            // 투입
            if ((bool)rdoPallet.IsChecked)
            {
                // 대차투입
                InputCart();
            }
            else
            {
                // Inbox 투입
                InputPallet();
            }

        }

        private void btnPalletInputCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationPalletCancel()) return;

            //// 투입취소 하시겠습니까?
            //Util.MessageConfirm("SFU1988", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        // 대차 투입 취소
            //        CancelPallet();
            //    }
            //});

            InputCancelCart();
        }

        #endregion

        #region 투입 자재 Tab

        private void dgMaterial_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg != null)
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                             checkBox.IsChecked.HasValue &&
                                                             !(bool)checkBox.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (box != null)
                                                        box.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                        if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                          dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                          o.IsChecked.HasValue &&
                                                          (bool)o.IsChecked))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                        }
                                    }
                                }
                                break;
                        }

                        if (dg?.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtMaterialID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtMaterialID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationMaterialInput()) return;

                //// 투입처리 하시겠습니까?
                //Util.MessageConfirm("SFU1987", (result) =>
                //{
                //    if (result == MessageBoxResult.OK)
                //    {
                InputMaterial();
                //    }
                //});

            }

        }

        private void btnMaterialInput_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationMaterialInput()) return;

            // 투입처리 하시겠습니까?
            //Util.MessageConfirm("SFU1987", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            InputMaterial();
            //    }
            //});

        }

        private void btnMaterialInputEnd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationMaterialEnd()) return;

            // 투입완료 처리 하시겠습니까?
            Util.MessageConfirm("SFU1972", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EndMaterial();
                }
            });

        }

        private void btnMaterialInputCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationMaterialCancel()) return;

            // 투입을 취소 하시겠습니까?
            Util.MessageConfirm("SFU1982", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelMaterial();
                }
            });

        }

        #endregion

        #region 자재 투입 이력 Tab

        private void dgMaterialHistory_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg != null)
                                {
                                    var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                    if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                             checkBox.IsChecked.HasValue &&
                                                             !(bool)checkBox.IsChecked))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                    if (box != null)
                                                        box.IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var o = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                        if (o != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                          dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                          o.IsChecked.HasValue &&
                                                          (bool)o.IsChecked))
                                        {
                                            DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                            chk.IsChecked = false;
                                        }
                                    }
                                }
                                break;
                        }

                        if (dg?.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg != null && (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null))
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMaterialHistoryCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationMaterialHistoryCancel()) return;

            // 투입을 취소 하시겠습니까?
            Util.MessageConfirm("SFU1982", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelMaterialHistory();
                }
            });

        }

        #endregion

        #endregion

        #region Mehod

        public void ChangeEquipment(string equipmentCode, string equipmentName = null)
        {
            try
            {
                EquipmentCode = equipmentCode;
                EquipmentName = equipmentName;

                ProdLotId = string.Empty;
                InitializeControls();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void InitializeControls()
        {
            Util.gridClear(dgTray);
            Util.gridClear(dgMaterial);
            Util.gridClear(dgMaterialHistory);

            txtMaterialID.Text = string.Empty;
        }

        public void SetControlHeader()
        {
        }

        public void SetDataGridColumnVisibility()
        {
        }

        public DataRow GetSelectInputPalletRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgPallet.ItemsSource).Select("CHK = 1");

                if (dr == null || dr.Length < 1)
                    row = null;
                else
                    row = dr[0];

                return row;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public void SelectInpuList()
        {
            InitializeControls();

            SelectInputTrayList();
            SelectInputPalletList();
            SelectInputMaterialList();
            SelectInputMaterialHistoryList();
        }

        #region [Tray 투입]
        /// <summary>
        /// Tray 투입 이력 조회
        /// </summary>
        private void SelectInputTrayList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("STARTDTTM", typeof(string));
                inTable.Columns.Add("INOUT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = EquipmentCode;
                newRow["PROCID"] = ProcessCode;
                newRow["LOTID"] = ProdAssyLotId;
                newRow["STARTDTTM"] = ProdJobStartDT;
                newRow["INOUT"] = "IN";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INOUT_TRAY_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgTray, dtResult, null, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region [대차 투입]
        public void SelectInputPalletList()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Zero;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = ProdWipSeq;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["INPUT_LOT_TYPE_CODE"] = "PROD";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INPUT_HISTORY_PC", "INDATA", "OUTDATA", inTable);

                dgPallet.CurrentCellChanged -= dgPallet_CurrentCellChanged;

                Util.GridSetData(dgPallet, dtResult, null, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgPallet.CurrentCell = dgPallet.GetCell(0, 1);

                dgPallet.CurrentCellChanged += dgPallet_CurrentCellChanged;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Inbox 투입
        /// </summary>
        private void InputPallet()
        {
            try
            {
                string bizRuleName = string.Empty;

                //if ((bool)rdoPallet.IsChecked)
                //{
                //    bizRuleName = "BR_PRD_CHK_INPUT_LOT_CTNR";
                //}
                //else
                //{
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_INBOX";
                //}

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = txtPalletID.Text;
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        txtPalletID.Text = string.Empty;

                        SelectInputPalletList();
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 대차 투입 취소
        /// </summary>
        private void CancelPallet()
        {
            try
            {
                string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";

                //int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgPallet, "CHK");

                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("INPUT_SEQNO", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(Decimal));
                inLot.Columns.Add("WIPQTY2", typeof(Decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgPallet.ItemsSource).Select("CHK = 1");
                foreach (DataRow drRow in dr)
                {
                    newRow = inLot.NewRow();
                    newRow["INPUT_SEQNO"] = Util.NVC_Int(drRow["INPUT_SEQNO"]).GetString();
                    newRow["LOTID"] = Util.NVC(drRow["CELLID"]).GetString();
                    newRow["WIPQTY"] = Util.NVC_Decimal(drRow["INPUT_QTY"]).GetString();
                    newRow["WIPQTY2"] = Util.NVC_Decimal(drRow["INPUT_QTY2"]).GetString();
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        SelectInputPalletList();
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [설비 투입 자재] Tab
        private void SelectInputMaterialList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CURR_IN_LOT_LIST_FO", "INDATA", "OUTDATA", inTable);

                dgMaterial.CurrentCellChanged -= dgMaterial_CurrentCellChanged;

                Util.GridSetData(dgMaterial, dtResult, null);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgMaterial.CurrentCell = dgMaterial.GetCell(0, 1);

                dgMaterial.CurrentCellChanged += dgMaterial_CurrentCellChanged;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 설비 투입 자재 투입
        /// </summary>
        private void InputMaterial()
        {
            try
            {
                DataSet inDataSet = GetBR_PRD_CHK_INPUT_LOT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["IN_INPUT"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgMaterial, "CHK");

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = txtMaterialID.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_ID").GetString());

                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_TW", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        txtMaterialID.Text = string.Empty;

                        SelectInputMaterialList();
                        SelectInputMaterialHistoryList();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비 투입 자재 투입 완료
        /// </summary>
        private void EndMaterial()
        {
            try
            {
                DataSet inDataSet = GetBR_PRD_REG_END_INPUT_IN_LOT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["IN_INPUT"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProdLotId;
                inTable.Rows.Add(newRow);

                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgMaterial, "CHK");

                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_ID").GetString());
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "INPUT_LOTID").GetString());
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        SelectInputMaterialList();
                        SelectInputMaterialHistoryList();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비 투입 자재 투입 취소
        /// </summary>
        private void CancelMaterial()
        {
            try
            {
                DataSet inDataSet = GetBR_PRD_REG_CANCEL_INPUT_IN_LOT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["IN_INPUT"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProdLotId;
                inTable.Rows.Add(newRow);

                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgMaterial, "CHK");

                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_ID").GetString());
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "INPUT_LOTID").GetString());
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        SelectInputMaterialList();
                        SelectInputMaterialHistoryList();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [자재 투입 이력 Tab]
        private void SelectInputMaterialHistoryList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = ProdWipSeq;
                newRow["INPUT_LOT_TYPE_CODE"] = "MTRL";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INPUT_HISTORY_FO", "INDATA", "OUTDATA", inTable);

                dgMaterialHistory.CurrentCellChanged -= dgMaterialHistory_CurrentCellChanged;

                Util.GridSetData(dgMaterialHistory, dtResult, null);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgMaterialHistory.CurrentCell = dgMaterialHistory.GetCell(0, 1);

                dgMaterialHistory.CurrentCellChanged += dgMaterialHistory_CurrentCellChanged;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Material 투입 취소
        /// </summary>
        private void CancelMaterialHistory()
        {
            try
            {
                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgMaterialHistory, "CHK");

                DataSet inDataSet = GetBR_PRD_REG_CANCEL_TERMINATE_LOT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["INLOT"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgMaterialHistory.Rows[iRow].DataItem, "INPUT_SEQNO").GetString());
                newRow["LOTID"] = DataTableConverter.GetValue(dgMaterialHistory.Rows[iRow].DataItem, "PALLETID").GetString();
                newRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgMaterialHistory.Rows[iRow].DataItem, "INPUT_QTY").GetString());
                newRow["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgMaterialHistory.Rows[iRow].DataItem, "INPUT_QTY2").GetString());
                inInput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_TERMINATE_LOT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        SelectInputMaterialHistoryList();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private bool ValidationInputTraySearch()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            return true;
        }

        private bool ValidationPalletInput()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPalletID.Text))
            {
                if ((bool)rdoPallet.IsChecked)
                {
                    // 투입 대차를 입력 하세요.
                    Util.MessageValidation("SFU4553");
                }
                else
                {
                    // 투입 Inbox를 입력 하세요.
                    Util.MessageValidation("SFU4015");
                }

                return false;
            }

            return true;
        }

        private bool ValidationPalletCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgPallet))
            {
                // 투입 정보가 없습니다.
                Util.MessageValidation("SFU1960");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK") < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationMaterialInput()
        {
            if (string.IsNullOrWhiteSpace(ProdLotId))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMaterialID.Text))
            {
                // 투입자재 LOT ID를 입력하세요.
                Util.MessageValidation("SFU1984");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK") < 0)
            {
                // 자재 투입위치를 선택하세요.
                Util.MessageValidation("SFU1820");
                return false;
            }


            return true;
        }

        private bool ValidationMaterialEnd()
        {
            if (!CommonVerify.HasDataGridRow(dgMaterial))
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK") < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationMaterialCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgMaterial))
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMaterial, "CHK") < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationMaterialHistoryCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgMaterialHistory))
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMaterialHistory, "CHK") < 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        protected virtual void GetProductLot()
        {
            if (UcParentControl == null)
                return;

            try
            {
                Type type = UcParentControl.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                ////for (int i = 0; i < parameterArrys.Length; i++)
                ////{
                ////    parameterArrys[i] = true;
                ////}

                parameterArrys[0] = true;
                parameterArrys[1] = null;

                methodInfo.Invoke(UcParentControl, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataSet GetBR_PRD_CHK_INPUT_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inBox = indataSet.Tables.Add("IN_INPUT");
            inBox.Columns.Add("INPUT_LOTID", typeof(string));
            inBox.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

            return indataSet;
        }

        private DataSet GetBR_PRD_REG_CANCEL_TERMINATE_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataTable inLot = indataSet.Tables.Add("INLOT");
            inLot.Columns.Add("INPUT_SEQNO", typeof(string));
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(Decimal));
            inLot.Columns.Add("WIPQTY2", typeof(Decimal));

            return indataSet;
        }

        private DataSet GetBR_PRD_REG_CANCEL_INPUT_IN_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInput = indataSet.Tables.Add("IN_INPUT");
            inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInput.Columns.Add("INPUT_LOTID", typeof(string));
            inInput.Columns.Add("WIPNOTE", typeof(string));
            inInput.Columns.Add("INPUT_SEQNO", typeof(Int64));

            return indataSet;
        }

        private DataSet GetBR_PRD_REG_END_INPUT_IN_LOT()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataTable inInput = indataSet.Tables.Add("IN_INPUT");
            inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            inInput.Columns.Add("INPUT_LOTID", typeof(string));

            return indataSet;
        }

        /// <summary>
        /// 투입 대차 팝업
        /// </summary>
        private void InputCart()
        {
            Popup.CMM_POLYMER_FORM_CART_INPUT popupCartInput = new Popup.CMM_POLYMER_FORM_CART_INPUT();
            popupCartInput.FrameOperation = this.FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = ProcessCode;
            parameters[1] = ProcessName;
            parameters[2] = EquipmentCode;
            parameters[3] = EquipmentName;
            parameters[4] = ProdLotId;
            parameters[5] = Util.NVC(txtPalletID.Text);

            C1WindowExtension.SetParameters(popupCartInput, parameters);

            popupCartInput.Closed += new EventHandler(popupCartInput_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartInput);
                    popupCartInput.BringToFront();
                    break;
                }
            }
        }

        private void popupCartInput_Closed(object sender, EventArgs e)
        {
            Popup.CMM_POLYMER_FORM_CART_INPUT popup = sender as Popup.CMM_POLYMER_FORM_CART_INPUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            SelectInputPalletList();
            GetProductLot();

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        /// <summary>
        /// 투입 취소 팝업
        /// </summary>
        private void InputCancelCart()
        {
            Popup.CMM_POLYMER_FORM_CART_INPUT_CANCEL popupCartInputCancel = new Popup.CMM_POLYMER_FORM_CART_INPUT_CANCEL();
            popupCartInputCancel.FrameOperation = this.FrameOperation;

            DataRow[] dr = DataTableConverter.Convert(dgPallet.ItemsSource).Select("CHK = 1");

            object[] parameters = new object[5];
            parameters[0] = ProcessCode;
            parameters[1] = ProcessName;
            parameters[2] = EquipmentCode;
            parameters[3] = EquipmentName;
            parameters[4] = dr;

            C1WindowExtension.SetParameters(popupCartInputCancel, parameters);

            popupCartInputCancel.Closed += new EventHandler(popupCartInputCancel_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartInputCancel);
                    popupCartInputCancel.BringToFront();
                    break;
                }
            }
        }

        private void popupCartInputCancel_Closed(object sender, EventArgs e)
        {
            Popup.CMM_POLYMER_FORM_CART_INPUT_CANCEL popup = sender as Popup.CMM_POLYMER_FORM_CART_INPUT_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            SelectInputPalletList();
            GetProductLot();

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgPallet;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }





        #endregion

    }
}
