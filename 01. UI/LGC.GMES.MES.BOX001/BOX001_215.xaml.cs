/*************************************************************************************
 Created Date : 2017.12.07
      Creator : 
   Decription : Degas(Pallet) 공정진척
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_215 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _processCode;
        private string _processName;
        private string _divisionCode;
        private string _selectCart;
        private bool _isLoaded = false;

        public UcPolymerFormCommand UcPolymerFormCommand { get; set; }
        public UcPolymerFormSearch UcPolymerFormSearch { get; set; }
        public UcPolymerFormProdLot UcPolymerFormProdLot { get; set; }
        public UcPolymerFormCart UcPolymerFormCart { get; set; }
        public UcPolymerFormInput UcPolymerFormInput { get; set; }
        public UcPolymerFormProductionResult UcPolymerFormProductionResult { get; set; }
        public UcPolymerFormShift UcPolymerFormShift { get; set; }
        private C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        private C1ComboBox ComboInspector { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1DataGrid DgProductionInbox { get; set; }
        public C1DataGrid DgInputPallet { get; set; }
        public UCBoxShift UcBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }
        public TextBox txtShiftTime_Main { get; set; }



        public string ProcessCode
        {
            get { return _processCode; }
            set
            {
                _processCode = value;
            }
        }
        public string ProcessName
        {
            get { return _processName; }
            set
            {
                _processName = value;
            }
        }

        public string DivisionCode
        {
            get { return _divisionCode; }
            set { _divisionCode = value; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public BOX001_215()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            InitializeUserControls();
            InitializeUserControlsGrid();
            SetComboBox();
            SetEventInUserControls();

            if (ComboEquipment?.SelectedValue != null)
            {
                GetUserControlEquipmentCode();
            }

        }

        private void InitializeUserControls()
        {
            UcPolymerFormCommand = grdCommand.Children[0] as UcPolymerFormCommand;
            UcPolymerFormSearch = grdSearch.Children[0] as UcPolymerFormSearch;
            UcPolymerFormCart = grdCart.Children[0] as UcPolymerFormCart;
            UcPolymerFormProdLot = grdProductLot.Children[0] as UcPolymerFormProdLot;
            UcPolymerFormInput = grdInput.Children[0] as UcPolymerFormInput;
            UcPolymerFormProductionResult = grdProduction.Children[0] as UcPolymerFormProductionResult;
            UcPolymerFormShift = grdShift.Children[0] as UcPolymerFormShift;
            UcBoxShift = grdShift.Children[0] as UCBoxShift;


            if (UcPolymerFormCommand != null)
            {
                UcPolymerFormCommand.ProcessCode = _processCode;
                UcPolymerFormCommand.DivisionCode = _divisionCode;
                UcPolymerFormCommand.SetButtonVisibility();
                UcPolymerFormCommand.FrameOperation = FrameOperation;
            }

            if (UcPolymerFormSearch != null)
            {
                UcPolymerFormSearch.ProcessCode = _processCode;
                UcPolymerFormSearch.DivisionCode = _divisionCode;
                UcPolymerFormSearch.SetControlVisibility();

                ComboEquipmentSegment = UcPolymerFormSearch.ComboEquipmentSegment;
                ComboEquipment = UcPolymerFormSearch.ComboEquipment;
                ComboInspector = UcPolymerFormSearch.ComboInspector;
            }

            if (UcPolymerFormCart != null)
            {
                UcPolymerFormCart.FrameOperation = this.FrameOperation;
                UcPolymerFormCart.ProcessCode = _processCode;
                UcPolymerFormCart.ProcessName = _processName;
                UcPolymerFormCart.UcParentControl = this;
                UcPolymerFormCart.SetButtonVisibility();
            }

            if (UcPolymerFormProdLot != null)
            {
                UcPolymerFormProdLot.UcParentControl = this;
                UcPolymerFormProdLot.ProcessCode = _processCode;
                UcPolymerFormProdLot.SetDataGridColumnVisibility();
            }

            if (UcPolymerFormInput != null)
            {
                UcPolymerFormInput.UcParentControl = this;
                UcPolymerFormInput.FrameOperation = FrameOperation;
                UcPolymerFormInput.ProcessCode = _processCode;
                UcPolymerFormInput.SetControlHeader();
                UcPolymerFormInput.SetDataGridColumnVisibility();
                UcPolymerFormInput.SetTabVisibility();
            }

            if (UcPolymerFormProductionResult != null)
            {
                UcPolymerFormProductionResult.UcParentControl = this;
                UcPolymerFormProductionResult.FrameOperation = FrameOperation;
                UcPolymerFormProductionResult.ProcessCode = _processCode;
                UcPolymerFormProductionResult.SetButtonVisibility();
                UcPolymerFormProductionResult.SetGridColumnVisibility();
                UcPolymerFormProductionResult.SetControlHeader();
            }

            if(UcBoxShift != null)
            {
                UcBoxShift.UcParentControl = this;

                txtWorker_Main = UcBoxShift.TextWorker;
                txtShift_Main = UcBoxShift.TextShift;
                txtShiftTime_Main = UcBoxShift.TextShiftDateTime;
                UcBoxShift.ProcessCode = _processCode; //작업조 팝업에 넘길 공정

            }
        }

        private void InitializeUserControlsGrid()
        {
            DgProductLot = UcPolymerFormProdLot.DgProductLot;
            DgProductionInbox = UcPolymerFormProductionResult.DgProductionInbox;
            DgInputPallet = UcPolymerFormInput.DgPallet;
                
        }

        private void SetEventInUserControls()
        {
            if (UcPolymerFormCommand != null)
            {
                UcPolymerFormCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                //UcPolymerFormCommand.ButtonInspection.Click += ButtonInspection_Click;
                UcPolymerFormCommand.ButtonStart.Click += ButtonStart_Click;
                UcPolymerFormCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcPolymerFormCommand.ButtonConfirm.Click += ButtonConfirm_Click;
                UcPolymerFormCommand.ButtonInboxType.Click += ButtonInboxType_Click;
            }

            if (UcPolymerFormSearch != null)
            {
                UcPolymerFormSearch.ComboEquipmentSegment.SelectedValueChanged += ComboEquipmentSegment_SelectedValueChanged;
                UcPolymerFormSearch.ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;
                UcPolymerFormSearch.ComboInspector.SelectedValueChanged += ComboInspector_SelectedValueChanged;

                UcPolymerFormSearch.ButtonSearch.Click += ButtonSearch_Click;
            }

            if (UcPolymerFormCart != null)
            {
                // 해당 User Control에서 코딩
                UcPolymerFormCart.ButtonCart1.Click += ButtonCart_Click;
                UcPolymerFormCart.ButtonCart2.Click += ButtonCart_Click;
                UcPolymerFormCart.ButtonCart3.Click += ButtonCart_Click;
                UcPolymerFormCart.ButtonCart4.Click += ButtonCart_Click;
                UcPolymerFormCart.ButtonCart5.Click += ButtonCart_Click;
                UcPolymerFormCart.ButtonCartSelect.Click += ButtonCartSelect_Click;
                UcPolymerFormCart.ButtonCartStorage.Click += ButtonCartStorage_Click;
                UcPolymerFormCart.SetButtonVisibility();
                //UcPolymerFormCart.ButtonCartDelete.Click += ButtonCartDelete_Click;
                //UcPolymerFormCart.ButtonCartMove.Click += ButtonCartMove_Click;
            }

            if (UcPolymerFormInput != null)
            {
                UcPolymerFormInput.ButtonPalletRemainWait.Click += ButtonPalletRemainWait_Click;
            }

            if (UcPolymerFormProductionResult != null)
            {
                // 해당 User Control에서 코딩
                //UcPolymerFormProductionResult.CommittedEdit += dgProductionPalette_CommittedEdit;
                //UcPolymerFormProductionResult.ButtonPalletHold.Click += ButtonPalletHold_Click;
                //UcPolymerFormProductionResult.ButtonGoodPalletCreate.Click += ButtonGoodPalletCreate_Click;
                //UcPolymerFormProductionResult.ButtonDefectPalletCreate.Click += ButtonDefectPalletCreate_Click;
                //UcPolymerFormProductionResult.ButtonPalletEdit.Click += ButtonPalletEdit_Click;
                //UcPolymerFormProductionResult.ButtonPalletDelete.Click += ButtonPalletDelete_Click;
                //UcPolymerFormProductionResult.ButtonTagPrint.Click += ButtonTagPrint_Click;
            }
            if(UcBoxShift != null)
            {
                
            }

        }




        #endregion

        #region Event


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string menuid = FrameOperation.MENUID;
            if(string.Equals(menuid,"SFU010060269")) // B1000공정: 포장 재작업
                _processCode = Process.CELL_BOXING;
            else                                     // B9000공정: 물류 반품
                _processCode = Process.CELL_BOXING_RETURN;
            _divisionCode = "Pallet";

            if (_isLoaded == false)
            {
                // 공정명 검색
                SelectProcessName();

                ApplyPermissions();
                Initialize();

                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
                    ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null);
            }

            _isLoaded = true;
            ////this.Loaded -= UserControl_Loaded;
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //UcPolymerFormProductionResult.DispatcherTimer?.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ButtonExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            C1DropDownButton btn = sender as C1DropDownButton;
            if (btn != null) btn.IsDropDownOpen = false;
        }


        private void popupQualty_Closed(object sender, EventArgs e)
        {
            BOX001_215_QUALITY popup = sender as BOX001_215_QUALITY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot(false);
            }
            //GetEqptWrkInfo();
            grdMain.Children.Remove(popup);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
                return;

            BOX001_215_RUNSTART popupRunStart = new BOX001_215_RUNSTART { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupRunStart.Name.ToString()) == false)
                return;

            object[] parameters = new object[7];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _divisionCode;
            parameters[5] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[6] = txtWorker_Main.Tag;
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            BOX001_215_RUNSTART popup = sender as BOX001_215_RUNSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot(false, popup.ProdLotId);
            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCompletion())
                return;

            BOX001_215_CONFIRM popupConfirm = new BOX001_215_CONFIRM { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupConfirm.Name.ToString()) == false)
                return;

            popupConfirm.ShiftID = Util.NVC(txtShift_Main.Tag);
            popupConfirm.ShiftName = Util.NVC(txtShift_Main.Text);
            popupConfirm.WorkerID = Util.NVC(txtWorker_Main.Tag);
            popupConfirm.WorkerName = Util.NVC(txtWorker_Main.Text);
            popupConfirm.ShiftDateTime = Util.NVC(txtShiftTime_Main.Text);

            object[] parameters = new object[7];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            parameters[5] = _divisionCode;
            parameters[6] = null;

            C1WindowExtension.SetParameters(popupConfirm, parameters);

            popupConfirm.Closed += new EventHandler(popupConfirm_Closed);
            grdMain.Children.Add(popupConfirm);
            popupConfirm.BringToFront();
        }

        private void popupConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_215_CONFIRM popup = sender as BOX001_215_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot(false);
            }
           // GetEqptWrkInfo();
            grdMain.Children.Remove(popup);
        }

        private void ButtonInboxType_Click(object sender, RoutedEventArgs e)
        {
            BOX001_215_EQPT_INBOX_TYPE popupInboxTyp = new BOX001_215_EQPT_INBOX_TYPE { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupInboxTyp.Name.ToString()) == false)
                return;

            popupInboxTyp.EquipmentSegmentCode = Util.NVC(ComboEquipmentSegment.SelectedValue);

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            C1WindowExtension.SetParameters(popupInboxTyp, parameters);

            popupInboxTyp.Closed += new EventHandler(popupInboxTyp_Closed);
            grdMain.Children.Add(popupInboxTyp);
            popupInboxTyp.BringToFront();

        }

        private void popupInboxTyp_Closed(object sender, EventArgs e)
        {
            BOX001_215_EQPT_INBOX_TYPE popup = sender as BOX001_215_EQPT_INBOX_TYPE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcPolymerFormProductionResult.SetInboxType();
            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            //GetEqptWrkInfo();
            GetProductLot(false);
        }

        private void ButtonCart_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Tag == null)
                return;

            GetProductCart(btn);
        }

        /// <summary>
        /// Palette 잔량대기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPalletRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletRemain()) return;

            BOX001_215_INBOX_WAIT_REMAIN popupPalletRemain = new BOX001_215_INBOX_WAIT_REMAIN { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popupPalletRemain.Name.ToString()) == false)
                return;

            popupPalletRemain.ShiftID = Util.NVC(txtShift_Main.Tag);
            popupPalletRemain.ShiftName = Util.NVC(txtShift_Main.Text);
            popupPalletRemain.WorkerID = Util.NVC(txtWorker_Main.Tag);
            popupPalletRemain.WorkerName = Util.NVC(txtWorker_Main.Text);
            popupPalletRemain.ShiftDateTime = Util.NVC(txtShiftTime_Main.Text);

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            popupPalletRemain.DifferenceQty = Util.NVC_Int(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "DIFF_QTY").ToString());

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = UcPolymerFormInput.GetSelectInputPalletRow();
            C1WindowExtension.SetParameters(popupPalletRemain, parameters);

            popupPalletRemain.Closed += new EventHandler(popupPalletRemainWait_Closed);
            grdMain.Children.Add(popupPalletRemain);
            popupPalletRemain.BringToFront();
        }
        private void popupPalletRemainWait_Closed(object sender, EventArgs e)
        {
            BOX001_215_INBOX_WAIT_REMAIN popup = sender as BOX001_215_INBOX_WAIT_REMAIN;
            if (popup.ConfirmSave)
            {
                GetProductLot(true);
                GetProductCart();
                UcPolymerFormInput.SelectInputPalletList();
            }
            //GetEqptWrkInfo();
            this.grdMain.Children.Remove(popup);
        }

        
        private void ComboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ComboEquipment.SelectedValueChanged -= ComboEquipment_SelectedValueChanged;
            SetEquipmentCombo(ComboEquipment);
            ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;

            // 선택 설비로...
            GetUserControlEquipmentCode();

            if (ComboEquipment != null && (ComboEquipment.SelectedValue.GetString() != "SELECT") && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex)
            {
                this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null)));
            }
            else
            {
                // Clear
                Util.gridClear(DgProductLot);
                GetProductCart();
                UcPolymerFormInput.InitializeControls();
                UcPolymerFormProductionResult.InitializeControls();
            }
        }

        private void ComboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                ClearControls();
                UcPolymerFormShift.ClearShiftControl();

                // 선택 설비로...
                GetUserControlEquipmentCode();

                if (ComboEquipment != null && (ComboEquipment.SelectedValue.GetString() != "SELECT") && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex)
                {
                    this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null)));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ComboInspector_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                // 선택 검사자...
                GetUserControlInspectorCode();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ButtonCartSelect_Click(object sender, RoutedEventArgs e)
        {
            BOX001_215_CART_LIST popupCartList = new BOX001_215_CART_LIST { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupCartList.Name.ToString()) == false)
                return;

            List<string> cartList = new List<string>();
            foreach (Button btn in Util.FindVisualChildren<Button>(UcPolymerFormCart.GridCart))
            {
                if (btn.Tag == null)
                    continue;
                cartList.Add(btn.Tag.ToString());
            }

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(txtWorker_Main.Tag);
            parameters[4] = cartList;
            C1WindowExtension.SetParameters(popupCartList, parameters);

            popupCartList.Closed += new EventHandler(popupCartList_Closed);
            grdMain.Children.Add(popupCartList);
            popupCartList.BringToFront();
        }

        private void popupCartList_Closed(object sender, EventArgs e)
        {
            BOX001_215_CART_LIST popup = sender as BOX001_215_CART_LIST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot(false);
                GetProductCart();
            }
            //GetEqptWrkInfo();
            grdMain.Children.Remove(popup);
        }
        private void ButtonCartStorage_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartStorage()) return;

            // 대차 보관
            CartStorage();
        }
        private void popupCartStorage_Closed(object sender, EventArgs e)
        {

            CMM_POLYMER_FORM_CART_STORAGE popup = sender as CMM_POLYMER_FORM_CART_STORAGE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true);
                UcPolymerFormProductionResult.SelectResultList();
            }

            GetProductCart(null, true);
            grdMain.Children.Remove(popup);
        }
        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// 작업 취소
        /// </summary>
        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_FO";

                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    GetProductLot(false);
                    Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 공정명 검색
        /// </summary>
        private void SelectProcessName()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _processName = dtResult.Rows[0]["PROCNAME"].ToString();
                else
                    _processName = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetProductLot(bool ProductOnly, string prodLot = null)
        {
            try
            {
                string selectedLot = string.Empty;

                if (string.IsNullOrWhiteSpace(prodLot))
                {
                    if (CommonVerify.HasDataGridRow(DgProductLot))
                    {
                        int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                        if (rowIdx >= 0)
                        {
                            selectedLot = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                        }
                    }
                }
                else
                {
                    // 작업시작시 생성된 생산 Lot
                    selectedLot = prodLot;
                }


                ////UcPolymerFormInput.InitializeControls();
                ////UcPolymerFormProductionResult.InitializeControls();

                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_NJ";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("DIVISION", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["DIVISION"] = string.Empty;

                inTable.Rows.Add(newRow);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.GridSetData(DgProductLot, result, FrameOperation, true);

                        if (result == null || result.Rows.Count == 0)
                        {
                            // Clear ALL
                            GetProductCart();
                            UcPolymerFormInput.InitializeControls();
                            UcPolymerFormProductionResult.InitializeControls();
                            HiddenLoadingIndicator();
                            return;
                        }

                        if (string.IsNullOrEmpty(selectedLot))
                        {
                            SetSelectProductRow(ProductOnly);
                        }
                        else
                        {
                            int idx = _util.GetDataGridRowIndex(DgProductLot, "LOTID", selectedLot);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(DgProductLot.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                DgProductLot.SelectedIndex = idx;

                                if (!ProductOnly)
                                    ProdListClickedProcess(idx);

                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                DgProductLot.CurrentCell = DgProductLot.GetCell(idx, DgProductLot.Columns.Count - 1);
                            }
                            else
                            {
                                SetSelectProductRow(ProductOnly);
                            }

                        }
                    }
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetProductCart(Button buttonClick = null, bool isSelect = false)
        {
            if (buttonClick != null)
            {
                SetCartButton(buttonClick);
                return;
            }

            UcPolymerFormCart.InitializeButtonControls();
            UcPolymerFormProductionResult.ProdCartId = null;
            UcPolymerFormCart.ProdCartId = null;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("CTNR_ID", typeof(string));
            inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

            // INDATA SET
            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
            newRow["CTNR_STAT_CODE"] = "CREATED";
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
            SetCartButton(dtResult, isSelect);
            // 설정
            UcPolymerFormCart.CartCount = dtResult.Rows.Count;



        }

        public void ChangeShiftByBox(string[] workerInfo)
        {
            UcPolymerFormProductionResult.ChangeWorkerInfoByBox(workerInfo);
        }

        

        #region 작업조, 작업자
        //private void GetEqptWrkInfo()
        //{
        //    try
        //    {
        //        const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

        //        DataTable inTable = new DataTable("RQSTDT");
        //        inTable.Columns.Add("LANGID", typeof(string));
        //        inTable.Columns.Add("EQPTID", typeof(string));
        //        inTable.Columns.Add("SHOPID", typeof(string));
        //        inTable.Columns.Add("AREAID", typeof(string));
        //        inTable.Columns.Add("EQSGID", typeof(string));
        //        inTable.Columns.Add("PROCID", typeof(string));

        //        DataRow newRow = inTable.NewRow();
        //        newRow["LANGID"] = LoginInfo.LANGID;
        //        newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
        //        newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
        //        newRow["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
        //        newRow["PROCID"] = _processCode;

        //        inTable.Rows.Add(newRow);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

        //        if (UcPolymerFormShift != null)
        //        {
        //            if (dtResult.Rows.Count > 0)
        //            {
        //                if (!dtResult.Rows[0].ItemArray[0].ToString().Equals(""))
        //                {
        //                    UcPolymerFormShift.TextShiftStartTime.Text = Util.NVC(dtResult.Rows[0]["WRK_STRT_DTTM"]);
        //                }
        //                else
        //                {
        //                    UcPolymerFormShift.TextShiftStartTime.Text = string.Empty;
        //                }

        //                if (!dtResult.Rows[0].ItemArray[1].ToString().Equals(""))
        //                {
        //                    UcPolymerFormShift.TextShiftEndTime.Text = Util.NVC(dtResult.Rows[0]["WRK_END_DTTM"]);
        //                }
        //                else
        //                {
        //                    UcPolymerFormShift.TextShiftEndTime.Text = string.Empty;
        //                }

        //                if (!string.IsNullOrEmpty(UcPolymerFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcPolymerFormShift.TextShiftEndTime.Text))
        //                {
        //                    UcPolymerFormShift.TextShiftDateTime.Text = UcPolymerFormShift.TextShiftStartTime.Text + " ~ " + UcPolymerFormShift.TextShiftEndTime.Text;
        //                }
        //                else
        //                {
        //                    UcPolymerFormShift.TextShiftDateTime.Text = string.Empty;
        //                }

        //                if (Util.NVC(dtResult.Rows[0]["WRK_USERID"]).Equals(""))
        //                {
        //                    UcPolymerFormShift.TextWorker.Text = string.Empty;
        //                    UcPolymerFormShift.TextWorker.Tag = string.Empty;
        //                }
        //                else
        //                {
        //                    UcPolymerFormShift.TextWorker.Text = Util.NVC(dtResult.Rows[0]["WRK_USERNAME"]);
        //                    UcPolymerFormShift.TextWorker.Tag = Util.NVC(dtResult.Rows[0]["WRK_USERID"]);
        //                }

        //                if (Util.NVC(dtResult.Rows[0]["SHFT_ID"]).Equals(""))
        //                {
        //                    UcPolymerFormShift.TextShift.Tag = string.Empty;
        //                    UcPolymerFormShift.TextShift.Text = string.Empty;
        //                }
        //                else
        //                {
        //                    UcPolymerFormShift.TextShift.Text = Util.NVC(dtResult.Rows[0]["SHFT_NAME"]);
        //                    UcPolymerFormShift.TextShift.Tag = Util.NVC(dtResult.Rows[0]["SHFT_ID"]);
        //                }
        //            }
        //            else
        //            {
        //                UcPolymerFormShift.ClearShiftControl();
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        #endregion

        #endregion

        #region[[Validation]
        private bool ValidationSearch()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // "설비를 선택 하세요."
                Util.MessageValidation("SFU1673");
                return false;
            }

            ////if (UcPolymerFormSearch.CheckEqpEnd.IsChecked != null && UcPolymerFormSearch.CheckRun.IsChecked != null && !(bool)UcPolymerFormSearch.CheckRun.IsChecked && !(bool)UcPolymerFormSearch.CheckEqpEnd.IsChecked)
            ////{
            ////    // LOT 상태 선택 조건을 하나 이상 선택하세요.
            ////    Util.MessageValidation("SFU1370");
            ////    return false;
            ////}

            return true;
        }

        private bool ValidationInspection()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex < 0)
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            return true;
        }

        private bool ValidationRunStart()
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            // 투입 Pallet 존재여부 확인
            if (DgInputPallet.Rows.Count - DgInputPallet.FrozenBottomRowsCount > 0)
            {
                // 투입이력이 존재하여 취소할 수 없습니다.
                Util.MessageValidation("SFU3437");
                return false;
            }

            // 완성 Pallet 존재여부 확인
            if (DgProductionInbox.Rows.Count - DgProductionInbox.FrozenBottomRowsCount > 0)
            {
                // 생산Pallet가 존재하여 취소할 수 없습니다.
                Util.MessageValidation("SFU4012");
                return false;
            }

            return true;
        }

        private bool ValidationCompletion()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex < 0)
            {
                // 실적확정 할 LOT이 선택되지 않았습니다
                Util.MessageValidation("SFU1717");
                return false;
            }

            //if (DgProductionInbox.Rows.Count - DgProductionInbox.BottomRows.Count == 0)
            //{
            //    // 실적 확정할 데이터가 없습니다. 아래쪽 List를 확인하세요.
            //    Util.MessageValidation("SFU3157");
            //    return false;
            //}

            ////DataRow[] drchk = DataTableConverter.Convert(DgProductionInbox.ItemsSource).Select("DISPATCH_YN <> 'Y'");

            ////if (drchk.Length > 0)
            ////{
            ////    // Pallet 태그를 전부 발행해야 작업 완료 처리가 가능 합니다.
            ////    Util.MessageValidation("SFU4013");
            ////    return false;
            ////}

            //DataRow[] drchk = DataTableConverter.Convert(DgProductionInbox.ItemsSource).Select("PRINT_YN <> 'Y'");

            //if (drchk.Length > 0)
            //{
            //    // Pallet 태그를 전부 발행해야 작업 완료 처리가 가능 합니다.
            //    Util.MessageValidation("SFU4013");
            //    return false;
            //}

            return true;
        }

        private bool ValidationPalletRemain()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgInputPallet, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region [Func]

        /// <summary>
        /// 생산 Lot 조회시 Select Row Setting
        /// </summary>
        private void SetSelectProductRow(bool ProductOnly)
        {
            int iRowRun = _util.GetDataGridRowIndex(DgProductLot, "WIPSTAT", "PROC");
            if (iRowRun < 0)
            {
                iRowRun = 0;
                if (DgProductLot.TopRows.Count > 0)
                    iRowRun = DgProductLot.TopRows.Count;

                DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                //row 색 바꾸기
                DgProductLot.SelectedIndex = iRowRun;

                if (!ProductOnly)
                    ProdListClickedProcess(iRowRun);
            }
            else
            {
                DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                //row 색 바꾸기
                DgProductLot.SelectedIndex = iRowRun;

                if (!ProductOnly)
                    ProdListClickedProcess(iRowRun);
            }

        }
        private void CartStorage()
        {
            CMM_POLYMER_FORM_CART_STORAGE popupCartStorage = new CMM_POLYMER_FORM_CART_STORAGE();
            popupCartStorage.FrameOperation = this.FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = ProcessCode;
            parameters[1] = ProcessName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _selectCart;

            C1WindowExtension.SetParameters(popupCartStorage, parameters);

            popupCartStorage.Closed += new EventHandler(popupCartStorage_Closed);
            grdMain.Children.Add(popupCartStorage);
            popupCartStorage.BringToFront();
        }

        public bool ClearControls()
        {
            try
            {
                Util.gridClear(DgProductLot);
                UcPolymerFormInput.InitializeControls();
                UcPolymerFormProductionResult.InitializeControls();
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public void ClearResultCollectControls()
        {
            UcPolymerFormProductionResult.InitializeControls();
        }

        public void ProdListClickedProcess(int iRow)
        {
            try
            {
                DataRow[] dr = DataTableConverter.Convert(DgProductLot.ItemsSource).Select("CHK = 1");

                if (dr.Length == 0) return;

                string lotId = Util.NVC(dr[0]["LOTID"]);
                string wipSeq = Util.NVC(dr[0]["WIPSEQ"]);
                string assyLot = Util.NVC(dr[0]["ASSY_LOTID"]);
                string jobstart = Util.NVC(dr[0]["WIPDTTM_ST"]);

                UcPolymerFormCart.ProdLotId = lotId;
                UcPolymerFormCart.ProdWipSeq = wipSeq;

                UcPolymerFormInput.ProdLotId = lotId;
                UcPolymerFormInput.ProdWipSeq = wipSeq;
                UcPolymerFormInput.ProdAssyLotId = assyLot;
                UcPolymerFormInput.ProdJobStartDT = jobstart;

                UcPolymerFormProductionResult.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcPolymerFormProductionResult.ProcessCode = _processCode;
                UcPolymerFormProductionResult.ProdLotId = lotId;
                UcPolymerFormProductionResult.WipSeq = wipSeq;
                UcPolymerFormProductionResult.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcPolymerFormProductionResult.DataRowProductLot = dr[0];

                // Clear
                UcPolymerFormInput.InitializeControls();
                UcPolymerFormProductionResult.InitializeControls();

                // 대차
                //UcPolymerFormCart.SetSelectCart();
                GetProductCart();

                // 투입 Tray, Pallet 데이터 조회
                UcPolymerFormInput.SelectInpuList();

                // 완성Palette 데이터 조회
                UcPolymerFormProductionResult.SetGradeCombo();
                UcPolymerFormProductionResult.SetInboxType();
                UcPolymerFormProductionResult.SelectResultList();

                // SHIFT, 작업자 
                GetUserControlShift();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool ValidationCartStorage()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            DataTable dt = SelectCartInfo();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (dt.Rows[0]["CART_SHEET_PRT_FLAG"].ToString().Equals("N"))
            {
                // 대차 발행후 보관 처리가 가능 합니다.
                Util.MessageValidation("SFU4408");
                return false;
            }

            if (Util.NVC_Int(dt.Rows[0]["CELL_QTY"]) == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return false;
            }

            //DataRow[] dr = DataTableConverter.Convert(DgProductionInbox.ItemsSource).Select("PRINT_YN = 'N'");

            //if (dr.Length > 0)
            //{
            //    // Inbox 태그를 전부 발행해야 이동 처리가 가능 합니다.
            //    Util.MessageValidation("SFU4368");
            //    return false;
            //}

            //if (string.IsNullOrWhiteSpace(_selectCart))
            //{
            //    // 이동할 대차를 선택하세요.
            //    Util.MessageValidation("SFU4358");
            //    return false;
            //}

            return true;
        }
        private DataTable SelectCartInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["CTNR_ID"] = _selectCart;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private void SetComboBox()
        {
            SetEquipmentSegmantCombo(ComboEquipmentSegment);
            SetEquipmentCombo(ComboEquipment);
            SetInspectorCombo(ComboInspector);
        }

        private void SetEquipmentSegmantCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQSG_BY_SHOP_CBO";
            string[] arrColumn = { "LANGID", "SHOPID","PROD_GROUP", "PROCID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID,"MCP", _processCode, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "EQGRID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(ComboEquipmentSegment.SelectedValue), _processCode, "CMM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

            //////////////////// 설비가 N건인 경우 Select 추가
            if (ComboEquipment.Items.Count > 1 || ComboEquipment.Items.Count == 0)
            {
                DataTable dtEqpt = DataTableConverter.Convert(cbo.ItemsSource);
                DataRow drEqpt = dtEqpt.NewRow();
                drEqpt[selectedValueText] = "SELECT";
                drEqpt[displayMemberText] = "- SELECT -";
                dtEqpt.Rows.InsertAt(drEqpt, 0);

                cbo.ItemsSource = null;
                cbo.ItemsSource = dtEqpt.Copy().AsDataView();

                int Index = 0;

                if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + LoginInfo.CFG_EQPT_ID + "'");

                    if (drIndex.Length > 0)
                    {
                        Index = dtEqpt.Rows.IndexOf(drIndex[0]);
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                }

                cbo.SelectedIndex = Index;
            }

        }

        private void SetInspectorCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_INSPECTOR_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _processCode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.USERID);
        }
        private void SetCartButton(Button button)
        {

            try
            {
                string cartCode = button.Name.Substring(button.Name.Length - 1, 1);

                foreach (Grid grid in Util.FindVisualChildren<Grid>(UcPolymerFormCart.GridCart))
                {
                    const string controlName = "grdCart";
                    if (grid.Name.Substring(0, grid.Name.Length - 1) == controlName)
                    {
                        grid.Background = grid.Name.Substring(grid.Name.Length - 1, 1) == cartCode ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
                    }
                }

                foreach (TextBlock textBlock in Util.FindVisualChildren<TextBlock>(UcPolymerFormCart.GridCart))
                {
                    if (textBlock.Name.Length > 0)
                    {
                        const string controlName = "tbCartInBoxCount";
                        if (textBlock.Name.Substring(0, textBlock.Name.Length - 1) == controlName)
                        {
                            textBlock.Foreground = textBlock.Name.Substring(textBlock.Name.Length - 1, 1) == cartCode ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                        }

                        const string controlName1 = "tbCart";
                        if (textBlock.Name.Substring(0, textBlock.Name.Length - 1) == controlName1)
                        {
                            textBlock.Foreground = textBlock.Name.Substring(textBlock.Name.Length - 1, 1) == cartCode ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Black);
                        }
                    }
                }

                foreach (Button btn in Util.FindVisualChildren<Button>(UcPolymerFormCart.GridCart))
                {
                    const string controlName = "btnCart";
                    if (btn.Name.Substring(0, btn.Name.Length - 1) == controlName)
                    {
                        if (btn.Name == button.Name)
                        {
                            UcPolymerFormProductionResult.ProdCartId = button.Tag.ToString();
                            UcPolymerFormCart.ProdCartId = button.Tag.ToString();
                            _selectCart = button.Tag.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetCartButton(DataTable dt, bool isSelect)
        {
            if (!CommonVerify.HasTableRow(dt)) return;


            string selectedContainerCode = string.Empty;
            if (isSelect)
                selectedContainerCode = _selectCart;

            for (int row = 0; row < dt.Rows.Count; row++)
            {
                foreach (TextBlock textBlock in Util.FindVisualChildren<TextBlock>(UcPolymerFormCart.GridCart))
                {
                    if (textBlock.Name.Length > 0)
                    {
                        // InBox 수량 textBlock 설정
                        const string controlName = "tbCartInBoxCount";
                        if (textBlock.Name.Substring(0, textBlock.Name.Length - 1) == controlName)
                        {
                            if (dt.Rows[row]["ROWNUM"].GetString() == textBlock.Name.Substring(textBlock.Name.Length - 1, 1))
                            {
                                textBlock.Text = "▶ " + dt.Rows[row]["INBOX_COUNT"];

                                if (isSelect)
                                {
                                    textBlock.Foreground = selectedContainerCode == dt.Rows[row]["CTNR_ID"].ToString() ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                                }
                                else
                                {
                                    textBlock.Foreground = row + 1 == dt.Rows.Count ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
                                }
                            }

                        }

                        // 대차 ID textBlock 설정
                        const string controlName1 = "tbCart";
                        if (textBlock.Name.Substring(0, textBlock.Name.Length - 1) == controlName1)
                        {
                            if (dt.Rows[row]["ROWNUM"].GetString() == textBlock.Name.Substring(textBlock.Name.Length - 1, 1))
                            {
                                textBlock.Text = dt.Rows[row]["CTNR_ID"].ToString();
                                if (isSelect)
                                {
                                    if (selectedContainerCode == dt.Rows[row]["CTNR_ID"].ToString())
                                    {
                                        textBlock.Foreground = new SolidColorBrush(Colors.Yellow);
                                        UcPolymerFormProductionResult.ProdCartId = dt.Rows[row]["CTNR_ID"].ToString();
                                        UcPolymerFormCart.ProdCartId = dt.Rows[row]["CTNR_ID"].ToString();
                                        _selectCart = dt.Rows[row]["CTNR_ID"].ToString();
                                    }
                                    else
                                    {
                                        textBlock.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                }
                                else
                                {
                                    if (dt.Rows.Count == row + 1)
                                    {
                                        textBlock.Foreground = new SolidColorBrush(Colors.Yellow);
                                        UcPolymerFormProductionResult.ProdCartId = dt.Rows[row]["CTNR_ID"].ToString();
                                        UcPolymerFormCart.ProdCartId = dt.Rows[row]["CTNR_ID"].ToString();
                                        _selectCart = dt.Rows[row]["CTNR_ID"].ToString();
                                    }
                                    else
                                    {
                                        textBlock.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Grid grid in Util.FindVisualChildren<Grid>(UcPolymerFormCart.GridCart))
                {
                    if (grid.Name.Length > 0)
                    {
                        const string controlName = "grdCart";
                        if (grid.Name.Substring(0, grid.Name.Length - 1) == controlName)
                        {
                            if (dt.Rows[row]["ROWNUM"].GetString() == grid.Name.Substring(grid.Name.Length - 1, 1))
                            {
                                if (isSelect)
                                {
                                    grid.Background = selectedContainerCode == dt.Rows[row]["CTNR_ID"].ToString() ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
                                }
                                else
                                {
                                    grid.Background = row + 1 == dt.Rows.Count ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                    }
                }

                foreach (Button btn in Util.FindVisualChildren<Button>(UcPolymerFormCart.GridCart))
                {
                    if (btn.Name.Length > 0)
                    {
                        const string controlName = "btnCart";

                        if (btn.Name.Substring(0, btn.Name.Length - 1) == controlName)
                        {
                            if (dt.Rows[row]["ROWNUM"].GetString() == btn.Name.Substring(btn.Name.Length - 1, 1))
                            {
                                btn.Tag = dt.Rows[row]["CTNR_ID"].ToString();
                            }
                        }
                    }
                }


                foreach (Image image in Util.FindVisualChildren<Image>(UcPolymerFormCart.GridCart))
                {
                    if (image.Name.Length > 0)
                    {
                        const string controlName1 = "imgCartContainerSheet";
                        const string controlName2 = "imgCartContainerOkTag";
                        const string controlName3 = "imgCartContainerPartialTag";

                        if (image.Name.Substring(0, image.Name.Length - 1) == controlName1)
                        {
                            if (dt.Rows[row]["ROWNUM"].GetString() == image.Name.Substring(image.Name.Length - 1, 1))
                            {
                                image.Visibility = dt.Rows[row]["CART_SHEET_PRT_FLAG"].ToString() == "Y" ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }
                        if (image.Name.Substring(0, image.Name.Length - 1) == controlName2)
                        {
                            if (dt.Rows[row]["ROWNUM"].GetString() == image.Name.Substring(image.Name.Length - 1, 1))
                            {
                                image.Visibility = dt.Rows[row]["NO_PRINT_COUNT"].ToString() == "0" ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }
                        if (image.Name.Substring(0, image.Name.Length - 1) == controlName3)
                        {
                            if (dt.Rows[row]["ROWNUM"].GetString() == image.Name.Substring(image.Name.Length - 1, 1))
                            {
                                image.Visibility = dt.Rows[row]["PART_PRINT_YN"].ToString() == "Y" ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }
                    }
                }
            }

        }



        private void GetUserControlShift()
        {
            UcPolymerFormProductionResult.ShiftID = Util.NVC(txtShift_Main.Tag);
            UcPolymerFormProductionResult.WorkerID = Util.NVC(txtWorker_Main.Tag);
            UcPolymerFormProductionResult.WorkerName = Util.NVC(txtWorker_Main.Text);
            UcPolymerFormProductionResult.ShiftDateTime = Util.NVC(txtShiftTime_Main.Text);
        }

        private void GetUserControlEquipmentCode()
        {
            string equipmentCode = string.Empty;
            string equipmentSegment = string.Empty;

            equipmentCode = ComboEquipment.SelectedValue.GetString();
            equipmentSegment = ComboEquipmentSegment.SelectedValue.GetString();

            UcPolymerFormInput?.ChangeEquipment(equipmentCode);
            UcPolymerFormCart?.ChangeEquipment(equipmentCode);
            UcPolymerFormCart.EquipmentName = ComboEquipment.Text;
            UcPolymerFormProductionResult?.ChangeEquipment(equipmentCode, equipmentSegment);
        }

        private void GetUserControlInspectorCode()
        {
            string InspectorCode = string.Empty;

            InspectorCode = ComboInspector.SelectedValue.GetString();

            UcPolymerFormProductionResult.InspectorCode = InspectorCode;
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

        private void ApplyPermissions()
        {
            if (UcPolymerFormCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                    UcPolymerFormCommand.ButtonInspection,
                    UcPolymerFormCommand.ButtonStart,
                    UcPolymerFormCommand.ButtonCancel,
                    UcPolymerFormCommand.ButtonConfirm
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }
        #endregion

        #endregion

    }
}
