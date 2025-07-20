/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : Pallet 생성, 수정
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using System.Windows.Input;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_012_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration
        public UcFormShift UcFormShift { get; set; }
        public UcFormInputConfirm UcFormInputConfirm { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        DataTable _dtResult;
        DataTable _dtPallet;
        public bool ConfirmSave { get; set; }

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }
        public C1DataGrid DgInputPallet { get; set; }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FORM001_012_CONFIRM()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetControlVisibility();
                SetGridProduct();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
            UcFormShift = grdShift.Children[0] as UcFormShift;
            UcFormInputConfirm = grdInput.Children[0] as UcFormInputConfirm;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;

            // SET COMMON
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            // SET 생산 Lot 정보
            DataRow prodLot = tmps[4] as DataRow;

            if (prodLot == null)
                return;

            DataTable prodLotBind = new DataTable();

            prodLotBind = prodLot.Table.Clone();
            prodLotBind.ImportRow(prodLot);

            Util.GridSetData(dgLot, prodLotBind, null, true);

            GetDefectInfo();

            // 작업자, 작업조
            UcFormShift.TextShift.Tag = ShiftID;
            UcFormShift.TextShift.Text = ShiftName;
            UcFormShift.TextWorker.Tag = WorkerID;
            UcFormShift.TextWorker.Text = WorkerName;
            UcFormShift.TextShiftDateTime.Text = ShiftDateTime;

            UcFormShift = grdShift.Children[0] as UcFormShift;
            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

            /////////////////////////////// 투입 Pallet UserControl 설정 및 조회
            UcFormInputConfirm.EquipmentCode = _eqptID;

            if (UcFormInputConfirm != null)
            {
                UcFormInputConfirm.UcParentControl = this;
                UcFormInputConfirm.FrameOperation = FrameOperation;
                UcFormInputConfirm.ProcessCode = _procID;
                UcFormInputConfirm.SetControlHeader();
                UcFormInputConfirm.SetDataGridColumnVisibility();
                UcFormInputConfirm.ButtonPalletRemainWait.Click += ButtonPalletRemainWait_Click;
            }

            DgInputPallet = UcFormInputConfirm.DgInputPallet;

            SetInputPallet();

            /////////////////////////////// Focus           
            dgDefect.Focus();
            dgDefect.LoadedCellPresenter -= dgDefect_LoadedCellPresenter;
            if (dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount > 0)
            {
                dgDefect.CurrentCell = dgDefect.GetCell(0, dgDefect.Columns["RESNQTY"].Index);
                dgDefect.Selection.Add(dgDefect.GetCell(0, dgDefect.Columns["RESNQTY"].Index));
            }
            dgDefect.LoadedCellPresenter += dgDefect_LoadedCellPresenter;
        }

        private void SetControlVisibility()
        {
        }

        #endregion

        #region [Pallet ID ALL NEW Check, UnChkeck]
        private void chkAllNew_Checked(object sender, RoutedEventArgs e)
        {
            _dtResult.Select().ToList<DataRow>().ForEach(r => r["LOTID"] = "NEW");
            _dtResult.AcceptChanges();
            Util.GridSetData(dgDefect, _dtResult, null);

            SetProductCalculator();
        }

        private void chkAllNew_Unchecked(object sender, RoutedEventArgs e)
        {
            GetDefectInfo();
            SetProductCalculator();
        }
        #endregion

        #region 차이수량 Red 색상 처리
        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DIFF_QTY"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));

        }
        #endregion

        #region [불량수량 입력 - dgDefect_BeganEdit, dgDefect_CommittedEdit, dgDefect_LoadedCellPresenter, dgDefect_PreviewKeyDown]
        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                if (e.Cell.Column.Name.Equals("PRINT"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(80);
                }
            }));

        }

        private void dgDefect_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;

            if (cbo != null)
            {
                if (e.Column.Name == "cboPallet")
                {
                    DataTable dt = _dtPallet.Copy();
                    dt.Select("WIP_DFCT_CODE <> '" + Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "RESNCODE")) + "'").ToList<DataRow>().ForEach(row => row.Delete());
                    dt.AcceptChanges();

                    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());

                    int Index = 0;

                    if (cbo.SelectedValue != null)
                    {
                        DataRow[] drIndex = dt.Select("LOTID ='" + cbo.SelectedValue.ToString() + "'");

                        if (drIndex.Length > 0)
                        {
                            Index = dt.Rows.IndexOf(drIndex[0]);
                        }
                    }

                    cbo.SelectedIndex = Index;
                }
            }

        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("RESNQTY"))
            {
                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }

        }

        private void dgDefect_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Cell.Column.Name.Equals("cboPallet"))
            {
                if (((C1.WPF.DataGrid.C1DataGrid)sender).Focused)
                    return;

                try
                {
                    DataRow[] dr = _dtPallet.Select("WIP_DFCT_CODE ='" + Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNCODE").ToString()) + "' AND LOTID ='" +
                                                                         Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID").ToString()) + "'");

                    if (dr.Length > 0)
                    {
                        DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);
                        dt.Rows[e.Cell.Row.Index]["RESNQTY"] = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY").ToString());
                        dt.Rows[e.Cell.Row.Index]["WIPQTY"] = dr[0]["WIPQTY"];
                        dt.Rows[e.Cell.Row.Index]["SUMQTY"] = dr[0]["WIPQTY"];
                        dt.AcceptChanges();

                        int Index = e.Cell.Row.Index;

                        Util.GridSetData(dgDefect, dt, null);
                        dgDefect.SelectedIndex = Index;

                        // 생산 Lot 실적 산출
                        SetProductCalculator();
                    }
                }
                catch
                {
                }
            }
            else if (e.Cell.Column.Name.Equals("RESNQTY"))
            {
                int resnQty = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RESNQTY").ToString());
                decimal wipQty = Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPQTY").ToString());

                DataTableConverter.SetValue(e.Cell.Row.DataItem, "SUMQTY", resnQty + wipQty);

                // 생산 Lot 실적 산출
                SetProductCalculator();
            }
        }

        private void dgDefect_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int rIdx = 0;
                int cIdx = 0;

                C1DataGrid grid = sender as C1DataGrid;

                rIdx = grid.CurrentCell.Row.Index;
                cIdx = grid.CurrentCell.Column.Index;

                if (grid.CurrentCell.Column.Name.Equals("RESNQTY"))
                {
                    if (grid.GetRowCount() > ++rIdx)
                    {
                        grid.Selection.Clear();
                        grid.CurrentCell = grid.GetCell(rIdx, cIdx);
                        grid.Selection.Add(grid.GetCell(rIdx, cIdx));

                        if (grid.GetRowCount() - 1 != rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }
                    }
                }
            }

        }
        #endregion

        #region [작업완료]
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            try
            {
                dgDefect.EndEditRow(true);
            }
            catch
            { }

            if (!ValidateConfirmRun())
                return;

            // 실적확정 하시겠습니까?
            Util.MessageConfirm("SFU1716", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [태그 발행]
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            if (Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID")).Equals("NEW"))
            {
                // 출력 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4025");
                return;
            }

            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            popupTagPrint.DefectPalletYN = "Y";

            object[] parameters = new object[8];
            parameters[0] = _procID;
            parameters[1] = _eqptID;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
            parameters[3] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ").GetString();
            parameters[4] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNQTY"));
            parameters[5] = "N";      // 디스패치 처리
            parameters[6] = "N";      // 출력여부
            parameters[7] = "N";      // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);

            ////popupTagPrint.Show();
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupTagPrint);
                    popupTagPrint.BringToFront();
                    break;
                }
            }
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }
        #endregion

        #region [잔량 대기]
        /// <summary>
        /// Palette 잔량대기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPalletRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletRemain()) return;

            FORM001_PALETTE_WAIT_REMAIN popupPalletRemain = new FORM001_PALETTE_WAIT_REMAIN { FrameOperation = this.FrameOperation };

            popupPalletRemain.ShiftID = Util.NVC(UcFormShift.TextShift.Tag);
            popupPalletRemain.ShiftName = Util.NVC(UcFormShift.TextShift.Text);
            popupPalletRemain.WorkerID = Util.NVC(UcFormShift.TextWorker.Tag);
            popupPalletRemain.WorkerName = Util.NVC(UcFormShift.TextWorker.Text);
            popupPalletRemain.ShiftDateTime = Util.NVC(UcFormShift.TextShiftDateTime.Text);
            popupPalletRemain.DifferenceQty = Util.NVC_Int(DataTableConverter.GetValue(dgProduct.Rows[0].DataItem, "DIFF_QTY").ToString());

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = txtProcess.Text;
            parameters[2] = _eqptID;
            parameters[3] = txtEquipment.Text;
            parameters[4] = UcFormInputConfirm.GetSelectInputPalletRow();
            C1WindowExtension.SetParameters(popupPalletRemain, parameters);

            popupPalletRemain.Closed += new EventHandler(popupPalletRemainWait_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupPalletRemain);
                    popupPalletRemain.BringToFront();
                    break;
                }
            }
        }
        private void popupPalletRemainWait_Closed(object sender, EventArgs e)
        {
            FORM001_PALETTE_WAIT_REMAIN popup = sender as FORM001_PALETTE_WAIT_REMAIN;
            if (popup.ConfirmSave)
            {
                SetGridProduct();
                UcFormInputConfirm.SelectInputPalletList();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

            this.Focus();
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]
        /// <summary>
        /// 생산 Lot 실적 조회
        /// </summary>
        public void SetGridProduct()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID").GetString();
                newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ").GetString();
                newRow["PROCID"] = _procID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_PRODUCT_SUM_FO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProduct, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 불량, 로스, 물청 정보 조회
        /// </summary>
        private void GetDefectInfo()
        {
            try
            {
                DataTable inTable = _bizRule.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _procID;
                newRow["EQPTID"] = _eqptID;
                newRow["ACTID"] = "DEFECT_LOT";
                newRow["LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ");

                inTable.Rows.Add(newRow);

                _dtResult = new DataTable();
                _dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_INFO_FORMATION", "INDATA", "OUTDATA", inTable);

                // Pallet ID, 재공수량 Setting
                ////_dtResult.Columns.Add("LOTID", typeof(string));
                _dtResult.Columns.Add("WIPQTY", typeof(double));
                _dtResult.Columns.Add("SUMQTY", typeof(double));

                SetPalletID();

                Util.GridSetData(dgDefect, _dtResult, null);

                if (_dtPallet != null && _dtPallet.Rows.Count > 0)
                {
                    (dgDefect.Columns["cboPallet"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(_dtPallet.DefaultView.ToTable(true, "LOTID").Copy());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ConfirmProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_END_PROD_LOT_XS";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("INPUTQTY", typeof(Decimal));
                inTable.Columns.Add("OUTPUTQTY", typeof(Decimal));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));

                DataTable inResn = inDataSet.Tables.Add("INRESN");
                inResn.Columns.Add("LOTID", typeof(string));
                inResn.Columns.Add("WIP_DFCT_CODE", typeof(string));
                inResn.Columns.Add("WIPQTY", typeof(Decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["USERID"] = LoginInfo.USERID;
                newRow["INPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "INPUT_QTY"));
                newRow["OUTPUTQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "PRODUCT_QTY"));
                newRow["SHIFT"] = UcFormShift.TextShift.Tag.ToString();
                newRow["WRK_USERID"] = UcFormShift.TextWorker.Tag.ToString();
                newRow["WRK_USER_NAME"] = UcFormShift.TextWorker.Text;

                inTable.Rows.Add(newRow);

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgDefect.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top) || dRow.Type.Equals(DataGridRowType.Bottom))
                        continue;

                    newRow = inResn.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LOTID")).Equals("NEW") ? null : Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LOTID"));
                    newRow["WIP_DFCT_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "RESNCODE"));
                    newRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "RESNQTY"));
                    inResn.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        ConfirmSave = true;

                        if (bizResult == null || bizResult.Tables["OUTDATA"].Rows.Count == 0)
                        {
                            this.DialogResult = MessageBoxResult.OK;
                        }
                        else
                        {
                            SetPalletIDNextSave(bizResult.Tables["OUTDATA"]);
                            btnConfirm.IsEnabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateConfirmRun()
        {
            if (dgLot.Rows.Count <= 0)
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }
            
            if (dgProduct.Rows.Count <= 0)
            {
                // 생산량이 없습니다.
                Util.MessageValidation("SFU1613");
                return false;
            }

            if (UcFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcFormShift.TextShift.Tag.ToString()))
            {
                // 작업조를 입력해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            return true;
        }
        private bool ValidationPalletRemain()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(DgInputPallet, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

        private void SetPalletID()
        {
            try
            {
                DataTable dtDefect = new DataTable();
                string ResnCode = string.Empty;

                foreach (DataRow dRow in _dtResult.Rows)
                {
                    ResnCode += dRow["RESNCODE"] + ",";
                }

                if (string.IsNullOrWhiteSpace(ResnCode))
                    return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIP_DFCT_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SOC_VALUE", typeof(string));
                inTable.Columns.Add("ASSY_PROC_LOTID", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["WIP_DFCT_CODE"] = ResnCode.Substring(0, ResnCode.Length - 1);
                newRow["PRODID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "PRODID").GetString();
                //newRow["SOC_VALUE"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "SOC_VALUE").GetString();
                newRow["ASSY_PROC_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "ASSY_LOTID").GetString();
                newRow["MKT_TYPE_CODE"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "MKT_TYPE_CODE").GetString();
                newRow["LOTTYPE"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTTYPE").GetString();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_PALLET_RESN_FO", "INDATA", "OUTDATA", inTable);

                //////////////////////////////////// 불량 코드에 LOT이 없는경우 NEW로 넣어 준다
                _dtPallet = dtResult.Copy();

                for (int row = 0; row < _dtResult.Rows.Count; row++)
                {
                    DataRow[] dr = _dtPallet.Select("WIP_DFCT_CODE ='" + _dtResult.Rows[row]["RESNCODE"].ToString() + "'");

                    DataRow drAdd = _dtPallet.NewRow();
                    drAdd["WIP_DFCT_CODE"] = _dtResult.Rows[row]["RESNCODE"];
                    drAdd["LOTID"] = "NEW";
                    drAdd["WIPQTY"] = 0;
                    _dtPallet.Rows.Add(drAdd);

                    if (dr.Length == 0)
                    {
                        _dtResult.Rows[row]["LOTID"] = "NEW";
                        _dtResult.Rows[row]["WIPQTY"] = 0;
                        _dtResult.Rows[row]["SUMQTY"] = Util.NVC_Decimal(_dtResult.Rows[row]["RESNQTY"].ToString());
                    }
                    else
                    {
                        _dtResult.Rows[row]["LOTID"] = dr[0]["LOTID"];
                        _dtResult.Rows[row]["WIPQTY"] = dr[0]["WIPQTY"];
                        _dtResult.Rows[row]["SUMQTY"] = Util.NVC_Decimal(_dtResult.Rows[row]["RESNQTY"].ToString()) + Util.NVC_Decimal(dr[0]["WIPQTY"].ToString());
                    }
                }

                _dtPallet.AcceptChanges();
                _dtResult.AcceptChanges();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPalletIDNextSave(DataTable dt)
        {
            try
            {

                _dtResult = DataTableConverter.Convert(dgDefect.ItemsSource);

                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    DataRow[] dr = _dtPallet.Select("WIP_DFCT_CODE ='" + dt.Rows[row]["WIP_DFCT_CODE"].ToString() + "' AND LOTID = '" + dt.Rows[row]["LOTID"].ToString() + "'");

                    if (dr.Length == 0)
                    {
                        dr = _dtPallet.Select("WIP_DFCT_CODE ='" + dt.Rows[row]["WIP_DFCT_CODE"].ToString() + "'");

                        if (dr.Length > 0)
                            dr[0]["LOTID"] = dt.Rows[row]["LOTID"];
                    }
                }


                for (int row = 0; row < _dtResult.Rows.Count; row++)
                {
                    DataRow[] dr = dt.Select("WIP_DFCT_CODE ='" + _dtResult.Rows[row]["RESNCODE"].ToString() + "'");

                    if (dr.Length > 0)
                    {
                        _dtResult.Rows[row]["LOTID"] = dr[0]["LOTID"];
                    }
                }

                _dtPallet.AcceptChanges();
                _dtResult.AcceptChanges();

                try
                {
                    (dgDefect.Columns["cboPallet"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
                }
                catch { }

                Util.GridSetData(dgDefect, _dtResult, null);

                if (_dtPallet != null && _dtPallet.Rows.Count > 0)
                {
                    (dgDefect.Columns["cboPallet"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(_dtPallet.DefaultView.ToTable(true, "LOTID").Copy());

                    dgDefect.Columns["PRINT"].Visibility = Visibility.Visible;
                }
                dgDefect.Columns["RESNQTY"].IsReadOnly = true;
                dgDefect.Columns["cboPallet"].IsReadOnly = true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 작업조, 작업자
        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _procID; ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcFormShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcFormShift.TextShiftEndTime.Text))
                                {
                                    UcFormShift.TextShiftDateTime.Text = UcFormShift.TextShiftStartTime.Text + " ~ " + UcFormShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcFormShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcFormShift.TextWorker.Text = string.Empty;
                                    UcFormShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcFormShift.TextShift.Tag = string.Empty;
                                    UcFormShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcFormShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShiftUser);
                    popupShiftUser.BringToFront();
                    break;
                }
            }
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.Focus();
        }

        private void SetProductCalculator()
        {
            int resnQtySum = DataTableConverter.Convert(dgDefect.ItemsSource).AsEnumerable().Sum(r => r.Field<int>("RESNQTY"));
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "INPUT_QTY").ToString());
            decimal goodQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "GOOD_QTY").ToString());

            DataTableConverter.SetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "PRODUCT_QTY", (goodQty + resnQtySum));
            DataTableConverter.SetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "DFCT_QTY", resnQtySum);
            DataTableConverter.SetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "DIFF_QTY", inputQty - (goodQty + resnQtySum));
        }

        private void SetInputPallet()
        {
            UcFormInputConfirm.ProdLotId = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID"));
            UcFormInputConfirm.ProdWipSeq = Util.NVC(DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ"));

            UcFormInputConfirm.InitializeControls();
            UcFormInputConfirm.SelectInputPalletList();
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

        #endregion



    }
}
