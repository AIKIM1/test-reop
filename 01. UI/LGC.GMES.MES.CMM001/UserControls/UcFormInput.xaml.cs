using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcFormInput.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFormInput
    {
        #region Declaration

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl UcParentControl;

        public C1DataGrid DgInputPallet { get; set; }

        public Button ButtonPalletRemainWait { get; set; }

        public string ProdLotId { get; set; }

        public string ProdWipSeq { get; set; }

        public string ProcessCode { get; set; }

        public string EquipmentCode { get; set; }
        public TextBox TextBoxPalletID { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        private readonly Util _util = new Util();

        public UcFormInput()
        {
            InitializeComponent();
            SetControl();
            SetButtons();
        }

        #endregion

        #region Initialize
        private void SetControl()
        {
            DgInputPallet = dgInputPallet;
            TextBoxPalletID = txtPalletID;
        }

        private void SetButtons()
        {
            ButtonPalletRemainWait = btnPalletRemainWait;
        }
        #endregion

        #region Event
        private void dgInputPallet_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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

        private void dgInputPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    /*
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#E6F5FB");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#E8F7C8");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    */
                }
            }));
        }

        private void dgInputPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
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

                // Pallet를 투입 하시겠습니까?
                //Util.MessageConfirm("SFU4016", (result) =>
                //{
                //    if (result == MessageBoxResult.OK)
                //    {
                // Pallet 투입
                //InputPallet();
                //    }
                //});

                if (string.Equals(ProcessCode, Process.CircularGrader) || string.Equals(ProcessCode, Process.SmallGrader) || string.Equals(ProcessCode, Process.CircularCharacteristicGrader))
                {
                    InputPalletrGrader();
                }
                else
                {
                    InputPallet();
                }

            }
        }

        private void btnPalletInput_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationPalletInput()) return;

            // Pallet를 투입 하시겠습니까?
            Util.MessageConfirm("SFU4016", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // Pallet 투입
                    if (string.Equals(ProcessCode, Process.CircularGrader) || string.Equals(ProcessCode, Process.SmallGrader) || string.Equals(ProcessCode, Process.CircularCharacteristicGrader))
                    {
                        InputPalletrGrader();
                    }
                    else
                    {
                        InputPallet();
                    }
                }
            });

        }

        private void btnPalletInputCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ValidationPalletCancel()) return;

            // Pallet를 투입 취소 하시겠습니까?
            Util.MessageConfirm("SFU4299", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // Pallet 투입 취소
                    CancelPallet();
                }
            });

        }

        #endregion

        #region Mehod

        public void ChangeEquipment(string equipmentCode)
        {
            try
            {
                EquipmentCode = equipmentCode;

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
            Util.gridClear(dgInputPallet);
            txtPalletID.Text = string.Empty;
        }

        public void SetControlHeader()
        {
            if (string.Equals(ProcessCode, Process.SmallOcv) || string.Equals(ProcessCode, Process.SmallLeak) || string.Equals(ProcessCode, Process.SmallDoubleTab))
            {
                // 초소형 OCV 검사, 초소형 누액검사, 초소형 더블탭
                tbTitle.Text = ObjectDic.Instance.GetObjectName("투입 대차");
                tbPalletID.Text = ObjectDic.Instance.GetObjectName("투입 대차 ID");
                dgInputPallet.Columns["PALLETID"].Header = ObjectDic.Instance.GetObjectName("대차 ID");
            }
            else
            {
                tbTitle.Text = ObjectDic.Instance.GetObjectName("투입 Pallet");
                tbPalletID.Text = ObjectDic.Instance.GetObjectName("투입 Pallet ID");
                dgInputPallet.Columns["PALLETID"].Header = ObjectDic.Instance.GetObjectName("Pallet ID");
            }
        }

        public void SetDataGridColumnVisibility()
        {
            // 전압 등급
            if (string.Equals(ProcessCode, Process.CircularGrader) || 
                string.Equals(ProcessCode, Process.CircularCharacteristic) ||
                string.Equals(ProcessCode, Process.CircularVoltage))
            {
                dgInputPallet.Columns["VLTG_GRD_CODE"].Visibility = Visibility.Collapsed;
            }

            // 저항 등급
            if (string.Equals(ProcessCode, Process.CircularCharacteristicGrader) || 
                string.Equals(ProcessCode, Process.CircularReTubing))
            {
                dgInputPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgInputPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
            }

        }

        public DataRow GetSelectInputPalletRow()
        {
            DataRow row;

            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgInputPallet.ItemsSource).Select("CHK = 1");

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

        public void SelectInputPalletList()
        {
            try
            {
                ////InitializeControls();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = ProdWipSeq;
                newRow["INPUT_LOT_TYPE_CODE"] = "PROD";
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INPUT_HISTORY_FO", "INDATA", "OUTDATA", inTable);

                dgInputPallet.CurrentCellChanged -= dgInputPallet_CurrentCellChanged;

                Util.GridSetData(dgInputPallet, dtResult, null, true);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dgInputPallet.CurrentCell = dgInputPallet.GetCell(0, 1);

                dgInputPallet.CurrentCellChanged += dgInputPallet_CurrentCellChanged;

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
            inLot.Columns.Add("PROD_LOTID", typeof(string));


            return indataSet;
        }

        /// <summary>
        /// Pallet 투입
        /// </summary>
        private void InputPalletrGrader()
        {
            try
            {
                DataSet inDataSet = GetBR_PRD_CHK_INPUT_LOT();

                string bizRuleName = string.Empty;

                if (string.Equals(ProcessCode, Process.CircularGrader))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_GD";
                else if (string.Equals(ProcessCode, Process.SmallGrader))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_GS";
                else if (string.Equals(ProcessCode, Process.CircularCharacteristicGrader))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_CG";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("INPUT_TYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EquipmentCode);
                newRow["INPUT_LOTID"] = txtPalletID.Text;
                newRow["INPUT_TYPE"] = "P";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = ProdLotId;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    //HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    ////Util.AlertInfo("정상 처리 되었습니다.");
                    //Util.MessageInfo("SFU1889");

                    InitializeControls();
                    SelectInputPalletList();
                    GetProductLot();

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Pallet 투입
        /// </summary>
        private void InputPallet()
        {
            try
            {
                DataSet inDataSet = GetBR_PRD_CHK_INPUT_LOT();

                string bizRuleName = string.Empty;

                if (string.Equals(ProcessCode, Process.CircularCharacteristic))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_CM";
                else if (string.Equals(ProcessCode, Process.CircularReTubing))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_RT";
                else if (string.Equals(ProcessCode, Process.CircularVoltage))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_VT";
                else if (string.Equals(ProcessCode, Process.SmallXray))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_XR";
                else if (string.Equals(ProcessCode, Process.SmallExternalTab))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_TW";
                else if (string.Equals(ProcessCode, Process.SmallOcv))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_OCV";
                else if (string.Equals(ProcessCode, Process.SmallCCD))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_CCD";
                else if (string.Equals(ProcessCode, Process.SmallLeak))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_LI";
                else if (string.Equals(ProcessCode, Process.SmallDoubleTab))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_TI";
                else if (string.Equals(ProcessCode, Process.SmallPacking))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_MP";
                else if (string.Equals(ProcessCode, Process.SmallAppearance))
                    bizRuleName = "BR_PRD_CHK_INPUT_LOT_EI";

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["IN_INPUT"];

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

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,IN_INPUT", null, (bizResult, bizException) =>
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

                        InitializeControls();
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
        /// Pallet 투입 취소
        /// </summary>
        private void CancelPallet()
        {
            try
            {
                ////string BizName = string.Equals(ProcessCode, Process.CircularReTubing) ? "BR_PRD_REG_CANCEL_INPUT_RT" : "BR_PRD_REG_CANCEL_TERMINATE_LOT";

                string bizRuleName = string.Empty;

                if (string.Equals(ProcessCode, Process.CircularReTubing))
                {
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_RT";
                }
                else if (string.Equals(ProcessCode, Process.CircularVoltage))
                {
                    bizRuleName = "BR_PRD_REG_CANCEL_INPUT_VT";
                }
                else
                {
                    //bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";
                    bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT_FO";
                }

                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(dgInputPallet, "CHK");

                DataSet inDataSet = GetBR_PRD_REG_CANCEL_TERMINATE_LOT();

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inInput = inDataSet.Tables["INLOT"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgInputPallet.Rows[iRow].DataItem, "INPUT_SEQNO").GetString());
                newRow["LOTID"] = DataTableConverter.GetValue(dgInputPallet.Rows[iRow].DataItem, "PALLETID").GetString();
                newRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputPallet.Rows[iRow].DataItem, "INPUT_QTY").GetString());
                newRow["WIPQTY2"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputPallet.Rows[iRow].DataItem, "INPUT_QTY2").GetString());
                newRow["PROD_LOTID"] = ProdLotId;
                inInput.Rows.Add(newRow);

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

                        InitializeControls();
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
                // 투입 Pallet를 입력 하세요.
                Util.MessageValidation("SFU4015");
                return false;
            }

            return true;
        }

        private bool ValidationPalletCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgInputPallet))
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgInputPallet, "CHK") < 0)
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

        #endregion

    }
}
