/*************************************************************************************
 Created Date : 2018.03.03
      Creator : 
   Decription : 양품화/C생산 공정진척
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Globalization;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Documents;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_032 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _processCode;
        private string _processName;
        private string _divisionCode;
        private string _selectCart;
        private string _inspectorCode;
        private bool _isLoaded = false;

        public UcPolymerFormCommand UcPolymerFormCommand { get; set; }
        public UcPolymerFormSearch UcPolymerFormSearch { get; set; }
        public UcPolymerFormProdLot UcPolymerFormProdLot { get; set; }
        public UcPolymerFormCart UcPolymerFormCart { get; set; }
        public UcPolymerFormInput UcPolymerFormInput { get; set; }
        public UcPolymerFormProductionResult UcPolymerFormProductionResult { get; set; }
        public UcPolymerFormShift UcPolymerFormShift { get; set; }
        public C1ComboBox ComboProcess { get; set; }
        private C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        private C1ComboBox ComboInspector { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1DataGrid DgProductionInbox { get; set; }
        public C1DataGrid DgInputPallet { get; set; }

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

        public string InspectorCode
        {
            get { return _inspectorCode; }
            set { _inspectorCode = value; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public FORM001_032()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            InitializeUserControls();
            InitializeUserControlsGrid();
            SetComboBox();
            SelectProcessName();
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

            if (UcPolymerFormCommand != null)
            {
                UcPolymerFormCommand.ProcessCode = _processCode;
                UcPolymerFormCommand.DivisionCode = _divisionCode;
                UcPolymerFormCommand.FrameOperation = FrameOperation;
                UcPolymerFormCommand.SetButtonVisibility();
            }

            if (UcPolymerFormSearch != null)
            {
                UcPolymerFormSearch.ProcessCode = _processCode;
                UcPolymerFormSearch.DivisionCode = _divisionCode;
                UcPolymerFormSearch.SetControlVisibility();

                ComboProcess = UcPolymerFormSearch.ComboProcess;
                ComboEquipmentSegment = UcPolymerFormSearch.ComboEquipmentSegment;
                ComboEquipment = UcPolymerFormSearch.ComboEquipment;
                ComboInspector = UcPolymerFormSearch.ComboInspector;
            }

            if (UcPolymerFormCart != null)
            {
                UcPolymerFormCart.ProcessCode = _processCode;
                UcPolymerFormCart.ProcessName = _processName;
                UcPolymerFormCart.FrameOperation = FrameOperation;
                UcPolymerFormCart.UcParentControl = this;
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
                UcPolymerFormInput.ProcessName = _processName;
                UcPolymerFormInput.SetControlHeader();
                UcPolymerFormInput.SetDataGridColumnVisibility();
                UcPolymerFormInput.SetTabVisibility();
            }

            if (UcPolymerFormProductionResult != null)
            {
                UcPolymerFormProductionResult.UcParentControl = this;
                UcPolymerFormProductionResult.FrameOperation = FrameOperation;
                UcPolymerFormProductionResult.ProcessCode = _processCode;
                UcPolymerFormProductionResult.ProcessName = _processName;
                UcPolymerFormProductionResult.AommGrdVisibility = true;
                UcPolymerFormProductionResult.SetButtonVisibility();
                UcPolymerFormProductionResult.SetControlHeader();
                UcPolymerFormProductionResult.SetControlVisibility();
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
                UcPolymerFormCommand.ButtonTakeOver.Click += ButtonTakeOver_Click;
                UcPolymerFormCommand.ButtonCartDefect.Click += ButtonCartDefect_Click;
                UcPolymerFormCommand.ButtonInspectionNew.Click += ButtonInspectionNew_Click;
                UcPolymerFormCommand.ButtonChangeRoute.Click += ButtonChangeRoute_Click;
                UcPolymerFormCommand.ButtonSublotDefect.Click += ButtonSublotDefect_Click;

                UcPolymerFormCommand.ButtonInspection.Click += ButtonInspection_Click;
                UcPolymerFormCommand.ButtonStart.Click += ButtonStart_Click;
                UcPolymerFormCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcPolymerFormCommand.ButtonConfirm.Click += ButtonConfirm_Click;
                UcPolymerFormCommand.ButtonInboxType.Click += ButtonInboxType_Click;
            }

            if (UcPolymerFormSearch != null)
            {
                UcPolymerFormSearch.ComboProcess.SelectedValueChanged += ComboProcess_SelectedValueChanged;
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

                UcPolymerFormCart.ButtonCartMove.Click += ButtonCartMove_Click;
                UcPolymerFormCart.ButtonCartStorage.Click += ButtonCartStorage_Click;
            }

            if (UcPolymerFormInput != null)
            {
                UcPolymerFormInput.ButtonPalletRemainWait.Click += ButtonPalletRemainWait_Click;
            }

            if (UcPolymerFormProductionResult != null)
            {
            }

            if (UcPolymerFormShift != null)
            {
                UcPolymerFormShift.ButtonShift.Click += ButtonShift_Click;
            }

        }


        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded == false)
            {
                _processCode = Process.PolymerFairQuality;
                _divisionCode = "Pallet";

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

        private void ButtonTakeOver_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTakeOver())
                return;

            CMM_POLYMER_FORM_CART_TAKEOVER popupTakeOver = new CMM_POLYMER_FORM_CART_TAKEOVER { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupTakeOver.Name) == false)
                return;

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            C1WindowExtension.SetParameters(popupTakeOver, parameters);

            popupTakeOver.Closed += popupTakeOver_Closed;
            grdMain.Children.Add(popupTakeOver);
            popupTakeOver.BringToFront();

        }

        private void popupTakeOver_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_TAKEOVER popup = sender as CMM_POLYMER_FORM_CART_TAKEOVER;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            GetProductCart();

            grdMain.Children.Remove(popup);
        }

        private void ButtonCartDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartDefect())
                return;

            CMM_POLYMER_FORM_CART_DEFECT popupCartDefect = new CMM_POLYMER_FORM_CART_DEFECT { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupCartDefect.Name) == false)
                return;

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[5] = Util.NVC(ComboEquipmentSegment.Text);
            C1WindowExtension.SetParameters(popupCartDefect, parameters);

            popupCartDefect.Closed += popupCartDefect_Closed;
            grdMain.Children.Add(popupCartDefect);
            popupCartDefect.BringToFront();

        }

        private void popupCartDefect_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DEFECT popup = sender as CMM_POLYMER_FORM_CART_DEFECT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonInspection_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInspection())
                return;

            CMM_FORM_QUALITY popupQualty = new CMM_FORM_QUALITY { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupQualty.Name) == false)
                return;

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            popupQualty.Update_YN = "N";
            popupQualty.ShiftID = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            popupQualty.ShiftName = Util.NVC(UcPolymerFormShift.TextShift.Text);
            popupQualty.WorkerID = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            popupQualty.WorkerName = Util.NVC(UcPolymerFormShift.TextWorker.Text);
            popupQualty.ShiftDateTime = Util.NVC(UcPolymerFormShift.TextShiftDateTime.Text);

            object[] parameters = new object[7];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[4] = Util.NVC(ComboEquipment.Text);
            parameters[5] = DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID").GetString();
            parameters[6] = DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ").GetString();
            C1WindowExtension.SetParameters(popupQualty, parameters);

            popupQualty.Closed += popupQualty_Closed;
            grdMain.Children.Add(popupQualty);
            popupQualty.BringToFront();
        }

        private void popupQualty_Closed(object sender, EventArgs e)
        {
            CMM_FORM_QUALITY popup = sender as CMM_FORM_QUALITY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot(false);
            }
            GetEqptWrkInfo();
            GetUserControlShift();

            grdMain.Children.Remove(popup);
        }

        private void ButtonInspectionNew_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInspection())
                return;

            CMM_COM_SELF_INSP popupQualty = new CMM_COM_SELF_INSP { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupQualty.Name) == false)
                return;

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID").GetString();
            parameters[5] = DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ").GetString();
            C1WindowExtension.SetParameters(popupQualty, parameters);

            popupQualty.Closed += popupQualtyNew_Closed;
            grdMain.Children.Add(popupQualty);
            popupQualty.BringToFront();
        }

        private void popupQualtyNew_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP popup = sender as CMM_COM_SELF_INSP;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot(false);
            }
            GetEqptWrkInfo();
            GetUserControlShift();

            grdMain.Children.Remove(popup);
        }

        private void ButtonChangeRoute_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInspection2())
                return;

            CMM_BOX_CART_CHANGE_ROUTE popupChangeRoute = new CMM_BOX_CART_CHANGE_ROUTE { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupChangeRoute.Name) == false)
                return;

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            //parameters[4] = DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID").GetString();
            parameters[5] = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            C1WindowExtension.SetParameters(popupChangeRoute, parameters);

            popupChangeRoute.Closed += popupChangeRoute_Closed;
            grdMain.Children.Add(popupChangeRoute);
            popupChangeRoute.BringToFront();
        }

        private void popupChangeRoute_Closed(object sender, EventArgs e)
        {
            CMM_BOX_CART_CHANGE_ROUTE popup = sender as CMM_BOX_CART_CHANGE_ROUTE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                //ClearControls();
                //GetProductLot(false);
            }
            GetEqptWrkInfo();
            GetUserControlShift();

            grdMain.Children.Remove(popup);
        }

        private void ButtonSublotDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSublotDefect())
                return;

            CMM_POLYMER_FORM_CELL_IINSERT_RSN_CLCT popupSublotDefect = new CMM_POLYMER_FORM_CELL_IINSERT_RSN_CLCT { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupSublotDefect.Name) == false)
                return;

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            C1WindowExtension.SetParameters(popupSublotDefect, parameters);

            popupSublotDefect.Closed += popupSublotDefect_Closed;
            grdMain.Children.Add(popupSublotDefect);
            popupSublotDefect.BringToFront();

        }

        private void popupSublotDefect_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CELL_IINSERT_RSN_CLCT popup = sender as CMM_POLYMER_FORM_CELL_IINSERT_RSN_CLCT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
                return;

            FORM001_RUNSTART popupRunStart = new FORM001_RUNSTART { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupRunStart.Name.ToString()) == false)
                return;

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _divisionCode;
            parameters[5] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            FORM001_RUNSTART popup = sender as FORM001_RUNSTART;
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

            FORM001_CONFIRM popupConfirm = new FORM001_CONFIRM { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupConfirm.Name.ToString()) == false)
                return;

            popupConfirm.ShiftID = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            popupConfirm.ShiftName = Util.NVC(UcPolymerFormShift.TextShift.Text);
            popupConfirm.WorkerID = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            popupConfirm.WorkerName = Util.NVC(UcPolymerFormShift.TextWorker.Text);
            popupConfirm.ShiftDateTime = Util.NVC(UcPolymerFormShift.TextShiftDateTime.Text);

            object[] parameters = new object[8];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            parameters[5] = _divisionCode;
            parameters[6] = null;
            parameters[7] = Util.NVC(ComboEquipmentSegment.SelectedValue);

            C1WindowExtension.SetParameters(popupConfirm, parameters);

            popupConfirm.Closed += new EventHandler(popupConfirm_Closed);
            grdMain.Children.Add(popupConfirm);
            popupConfirm.BringToFront();
        }

        private void popupConfirm_Closed(object sender, EventArgs e)
        {
            FORM001_CONFIRM popup = sender as FORM001_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot(false);
            }
            GetEqptWrkInfo();
            GetUserControlShift();

            grdMain.Children.Remove(popup);
        }

        private void ButtonInboxType_Click(object sender, RoutedEventArgs e)
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            FORM001_EQPT_INBOX_TYPE popupInboxType = new FORM001_EQPT_INBOX_TYPE { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupInboxType.Name.ToString()) == false)
                return;

            popupInboxType.EquipmentSegmentCode = Util.NVC(ComboEquipmentSegment.SelectedValue);

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            C1WindowExtension.SetParameters(popupInboxType, parameters);

            popupInboxType.Closed += new EventHandler(popupInboxTyp_Closed);
            grdMain.Children.Add(popupInboxType);
            popupInboxType.BringToFront();

        }

        private void popupInboxTyp_Closed(object sender, EventArgs e)
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            FORM001_EQPT_INBOX_TYPE popup = sender as FORM001_EQPT_INBOX_TYPE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcPolymerFormProductionResult.SetInboxType();
            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            GetEqptWrkInfo();
            GetProductLot(false);
        }

        private void ButtonCart_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Tag == null)
                return;

            GetProductCart(btn);
        }

        private void ButtonCartMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartMove()) return;

            // 대차 이동
            CartMove();
        }

        /// <summary>
        /// Cart Move 팝업
        /// </summary>
        private void CartMove()
        {
            CMM_POLYMER_FORM_CART_MOVE popupCartMove = new CMM_POLYMER_FORM_CART_MOVE();
            popupCartMove.FrameOperation = this.FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _selectCart;
            parameters[5] = Util.NVC(ComboEquipmentSegment.SelectedValue);

            C1WindowExtension.SetParameters(popupCartMove, parameters);

            popupCartMove.Closed += new EventHandler(popupCartMove_Closed);
            grdMain.Children.Add(popupCartMove);
            popupCartMove.BringToFront();
        }

        private void popupCartMove_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_MOVE popup = sender as CMM_POLYMER_FORM_CART_MOVE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true);
                UcPolymerFormProductionResult.SelectResultList();
                GetProductCart();
            }

            grdMain.Children.Remove(popup);
        }

        private void ButtonCartStorage_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartStorage()) return;

            // 대차 보관
            CartStorage();
        }

        /// <summary>
        /// Cart 보관 팝업
        /// </summary>
        private void CartStorage()
        {
            CMM_POLYMER_FORM_CART_STORAGE popupCartStorage = new CMM_POLYMER_FORM_CART_STORAGE();
            popupCartStorage.FrameOperation = this.FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _selectCart;

            C1WindowExtension.SetParameters(popupCartStorage, parameters);

            popupCartStorage.Closed += new EventHandler(popupCartStorage_Closed);
            grdMain.Children.Add(popupCartStorage);
            popupCartStorage.BringToFront();
        }

        private void popupCartStorage_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_STORAGE popup = sender as CMM_POLYMER_FORM_CART_STORAGE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true);
                UcPolymerFormProductionResult.SelectResultList();
                GetProductCart();
            }
            grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// Palette 잔량대기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPalletRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletRemain()) return;

            CMM_POLYMER_FORM_CART_INPUT_REMAIN popuInboxRemain = new CMM_POLYMER_FORM_CART_INPUT_REMAIN { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popuInboxRemain.Name.ToString()) == false)
                return;

            popuInboxRemain.ShiftID = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            popuInboxRemain.ShiftName = Util.NVC(UcPolymerFormShift.TextShift.Text);
            popuInboxRemain.WorkerID = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            popuInboxRemain.WorkerName = Util.NVC(UcPolymerFormShift.TextWorker.Text);
            popuInboxRemain.ShiftDateTime = Util.NVC(UcPolymerFormShift.TextShiftDateTime.Text);
            popuInboxRemain.EquipmentSegmentCode = Util.NVC(ComboEquipmentSegment.SelectedValue);

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            popuInboxRemain.DifferenceQty = Util.NVC_Int(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "DIFF_QTY").ToString());

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = UcPolymerFormInput.GetSelectInputPalletRow();
            parameters[5] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            C1WindowExtension.SetParameters(popuInboxRemain, parameters);

            popuInboxRemain.Closed += new EventHandler(popuInboxRemain_Closed);
            grdMain.Children.Add(popuInboxRemain);
            popuInboxRemain.BringToFront();
        }
        private void popuInboxRemain_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_INPUT_REMAIN popup = sender as CMM_POLYMER_FORM_CART_INPUT_REMAIN;
            if (popup.ConfirmSave)
            {
                GetProductLot(true);
                UcPolymerFormInput.SelectInputPalletList();
            }
            GetEqptWrkInfo();
            GetUserControlShift();

            this.grdMain.Children.Remove(popup);
        }

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            parameters[6] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);
            grdMain.Children.Add(popupShiftUser);
            popupShiftUser.BringToFront();
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(popup);

            GetUserControlShift();
        }


        private void ComboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectProcessName();
            UcPolymerFormProductionResult.ProcessCode = ComboProcess.SelectedValue.GetString();
            SetEquipmentSegmantCombo(ComboEquipmentSegment);
        }

        private void ComboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ComboEquipment.SelectedValueChanged -= ComboEquipment_SelectedValueChanged;
            SetEquipmentCombo(ComboEquipment);
            ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;

            SetEquipmentSelectedValueChange();
        }

        private void ComboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipmentSelectedValueChange();
        }

        private void ComboInspector_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 선택 검사자...
            GetUserControlInspectorCode();
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
                // 선택 공정
                string procId = Util.NVC(ComboProcess.SelectedValue ?? _processCode);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = procId;
                inTable.Rows.Add(newRow);

                _processCode = procId;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _processName = dtResult.Rows[0]["PROCNAME"].ToString();
                else
                    _processName = string.Empty;

                UcPolymerFormCommand.ProcessCode = _processCode;
                UcPolymerFormSearch.ProcessCode = _processCode;
                UcPolymerFormCart.ProcessCode = _processCode;
                UcPolymerFormCart.ProcessName = _processName;
                UcPolymerFormProdLot.ProcessCode = _processCode;
                UcPolymerFormInput.ProcessCode = _processCode;
                UcPolymerFormInput.ProcessName = _processName;
                UcPolymerFormProductionResult.ProcessCode = _processCode;
                UcPolymerFormProductionResult.ProcessName = _processName;

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

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_PC";
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
                //newRow["DIVISION"] = _divisionCode.Substring(0,1);

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
            inTable.Columns.Add("PROCID", typeof(string));
            inTable.Columns.Add("CTNR_ID", typeof(string));
            inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

            // INDATA SET
            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
            newRow["PROCID"] = _processCode;
            newRow["CTNR_STAT_CODE"] = "WORKING";
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
            SetCartButton(dtResult, isSelect);
            // 설정
            UcPolymerFormCart.CartCount = dtResult.Rows.Count;
        }

        /// <summary>
        /// 대차 상태 체크용
        /// </summary>
        private DataTable SelectCartInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["PROCID"] = _processCode;
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
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                newRow["PROCID"] = _processCode;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (UcPolymerFormShift != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        if (!dtResult.Rows[0].ItemArray[0].ToString().Equals(""))
                        {
                            UcPolymerFormShift.TextShiftStartTime.Text = Util.NVC(dtResult.Rows[0]["WRK_STRT_DTTM"]);
                        }
                        else
                        {
                            UcPolymerFormShift.TextShiftStartTime.Text = string.Empty;
                        }

                        if (!dtResult.Rows[0].ItemArray[1].ToString().Equals(""))
                        {
                            UcPolymerFormShift.TextShiftEndTime.Text = Util.NVC(dtResult.Rows[0]["WRK_END_DTTM"]);
                        }
                        else
                        {
                            UcPolymerFormShift.TextShiftEndTime.Text = string.Empty;
                        }

                        if (!string.IsNullOrEmpty(UcPolymerFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcPolymerFormShift.TextShiftEndTime.Text))
                        {
                            UcPolymerFormShift.TextShiftDateTime.Text = UcPolymerFormShift.TextShiftStartTime.Text + " ~ " + UcPolymerFormShift.TextShiftEndTime.Text;
                        }
                        else
                        {
                            UcPolymerFormShift.TextShiftDateTime.Text = string.Empty;
                        }

                        if (Util.NVC(dtResult.Rows[0]["WRK_USERID"]).Equals(""))
                        {
                            UcPolymerFormShift.TextWorker.Text = string.Empty;
                            UcPolymerFormShift.TextWorker.Tag = string.Empty;
                        }
                        else
                        {
                            UcPolymerFormShift.TextWorker.Text = Util.NVC(dtResult.Rows[0]["WRK_USERNAME"]);
                            UcPolymerFormShift.TextWorker.Tag = Util.NVC(dtResult.Rows[0]["WRK_USERID"]);
                        }

                        if (Util.NVC(dtResult.Rows[0]["SHFT_ID"]).Equals(""))
                        {
                            UcPolymerFormShift.TextShift.Tag = string.Empty;
                            UcPolymerFormShift.TextShift.Text = string.Empty;
                        }
                        else
                        {
                            UcPolymerFormShift.TextShift.Text = Util.NVC(dtResult.Rows[0]["SHFT_NAME"]);
                            UcPolymerFormShift.TextShift.Tag = Util.NVC(dtResult.Rows[0]["SHFT_ID"]);
                        }
                    }
                    else
                    {
                        UcPolymerFormShift.ClearShiftControl();
                    }

                    // 검사자 조회
                    SetInspectorCombo(ComboInspector);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
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

        private bool ValidationTakeOver()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationCartDefect()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

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

        private bool ValidationInspection2()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            //int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            //if (rowIndex < 0)
            //{
            //    // 생산 Lot 정보가 없습니다.
            //    Util.MessageValidation("SFU4014");
            //    return false;
            //}

            return true;
        }

        private bool ValidationSublotDefect()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationRunStart()
        {
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
                // 작업중인 LOT이 아닙니다.
                Util.MessageValidation("SFU1846");
                return false;
            }

            if (DgInputPallet.Rows.Count - DgInputPallet.FrozenBottomRowsCount > 0)
            {
                // 투입이력이 존재하여 취소할 수 없습니다.
                Util.MessageValidation("SFU3437");
                return false;
            }

            if (DgProductionInbox.Rows.Count - DgProductionInbox.FrozenBottomRowsCount > 0)
            {
                // 완성Inbox가 존재하여 취소할 수 없습니다.
                Util.MessageValidation("SFU4903");
                return false;
            }

            return true;
        }

        private bool ValidationCompletion()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
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

        private bool ValidationCartMove()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //DataRow[] dr = DataTableConverter.Convert(DgProductionInbox.ItemsSource).Select("CTNR_ID = '" + _selectCart + "' And PRINT_YN = 'N' ");

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

        private bool ValidationCartStorage()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //DataTable dt = SelectCartInfo();

            //if (dt == null || dt.Rows.Count == 0)
            //{
            //    // 대차 정보가 없습니다.
            //    Util.MessageValidation("SFU4365");
            //    return false;
            //}

            //if (dt.Rows[0]["CART_SHEET_PRT_FLAG"].ToString().Equals("N"))
            //{
            //    // 대차 발행후 보관 처리가 가능 합니다.
            //    Util.MessageValidation("SFU4408");
            //    return false;
            //}

            //if (Util.NVC_Int(dt.Rows[0]["CELL_QTY"]) == 0)
            //{
            //    // 대차에 Inbox 정보가 없습니다.
            //    Util.MessageValidation("SFU4375");
            //    return false;
            //}

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
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            int rowChkCount = DataTableConverter.Convert(DgInputPallet.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (rowChkCount == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (rowChkCount > 1)
            {
                // 한행만 선택 가능 합니다.
                Util.MessageValidation("SFU4023");
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

        public bool ClearControls()
        {
            try
            {
                _selectCart = string.Empty;
                UcPolymerFormCart.InitializeButtonControls();

                Util.gridClear(DgProductLot);
                UcPolymerFormInput.InitializeControls();
                UcPolymerFormProductionResult.InitializeControls();

                //ComboInspector.ItemsSource = null;
                //ComboInspector.Text = string.Empty;
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
                //string assyLot = Util.NVC(dr[0]["ASSY_LOTID"]);
                string assyLot = Util.NVC(dr[0]["LOTID_RT"]);
                string jobstart = Util.NVC(dr[0]["WIPDTTM_ST"]);

                UcPolymerFormCart.ProdLotId = lotId;
                UcPolymerFormCart.ProdWipSeq = wipSeq;

                UcPolymerFormInput.ProdLotId = lotId;
                UcPolymerFormInput.ProdWipSeq = wipSeq;
                UcPolymerFormInput.ProdAssyLotId = assyLot;
                UcPolymerFormInput.ProdJobStartDT = jobstart;

                UcPolymerFormProductionResult.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcPolymerFormProductionResult.EquipmentName = ComboEquipment.Text;
                UcPolymerFormProductionResult.ProdLotId = lotId;
                UcPolymerFormProductionResult.WipSeq = wipSeq;
                UcPolymerFormProductionResult.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcPolymerFormProductionResult.DataRowProductLot = dr[0];

                // Clear
                UcPolymerFormInput.InitializeControls();
                UcPolymerFormProductionResult.InitializeControls();
                UcPolymerFormProductionResult.SetControlVisibility();

                // 대차
                //UcPolymerFormCart.SetSelectCart();
                GetProductCart();

                // 투입 Tray, Pallet 데이터 조회
                UcPolymerFormInput.SelectInpuList();

                // 완성Palette 데이터 조회
                //UcPolymerFormProductionResult.SetGradeCombo();
                //UcPolymerFormProductionResult.SetInboxType();
                UcPolymerFormProductionResult.SelectResultList();

                // SHIFT, 작업자 
                GetUserControlShift();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void SetComboBox()
        {
            SetProcessCombo(ComboProcess);
            _processCode = ComboProcess.SelectedValue.GetString();
            SetEquipmentSegmantCombo(ComboEquipmentSegment);
            SetEquipmentCombo(ComboEquipment);
        }

        private static void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_PROCESS_PC";
            string[] arrColumn = { "LANGID", "AREAID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "FORM_REWORK_PROCID" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
        }

        private void SetEquipmentSegmantCombo(C1ComboBox cbo)
        {
            string processcode = string.Empty;
            if (!ComboProcess.SelectedValue.GetString().Equals("SELECT") && ComboProcess.SelectedIndex > 0)
                processcode = ComboProcess.SelectedValue.GetString();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, processcode };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            string processcode = string.Empty;
            if (ComboProcess.SelectedValue != null) processcode = ComboProcess.SelectedValue.GetString();

            string equipmentSegmentCode = string.Empty;
            if (ComboEquipmentSegment.SelectedValue != null) equipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();

            // 양품화, C생산 공정진척에서 포장설비는 EQPTTYPE = 'P'인 경우만
            string eqpttype = null;
            if (processcode.Equals(Process.CELL_BOXING) || processcode.Equals(Process.CELL_BOXING_RETURN)) eqpttype = "P";

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_PC";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "EQPTTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, equipmentSegmentCode, processcode, eqpttype };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

            if (cbo.Items.Count > 1 || cbo.Items.Count == 0)
            {
                DataTable dtEqpt = DataTableConverter.Convert(cbo.ItemsSource);
                DataRow drEqpt = dtEqpt.NewRow();
                drEqpt[selectedValueText] = "SELECT";
                drEqpt[displayMemberText] = "- SELECT -";
                dtEqpt.Rows.InsertAt(drEqpt, 0);

                cbo.ItemsSource = null;
                cbo.ItemsSource = dtEqpt.Copy().AsDataView();

                int index = 0;

                if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                {
                    DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + LoginInfo.CFG_EQPT_ID + "'");

                    if (drIndex.Length > 0)
                    {
                        index = dtEqpt.Rows.IndexOf(drIndex[0]);
                        cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                    }
                }
                cbo.SelectedIndex = index;
            }

        }

        private void SetInspectorCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_INSPECTOR_PC";
            string[] arrColumn = { "LANGID", "EQPTID"};
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(ComboEquipment.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, _inspectorCode);
        }

        private void SetEquipmentSelectedValueChange()
        {
            _inspectorCode = string.Empty;

            ClearControls();
            UcPolymerFormShift.ClearShiftControl();

            // 선택 설비로...
            GetUserControlEquipmentCode();

            if (ComboEquipment != null && ComboEquipment.SelectedIndex > -1 && ComboEquipment.SelectedValue.GetString() != "SELECT")
                this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null)));

        }

        private void GetUserControlShift()
        {
            UcPolymerFormProductionResult.ShiftID = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            UcPolymerFormProductionResult.WorkerID = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            UcPolymerFormProductionResult.WorkerName = Util.NVC(UcPolymerFormShift.TextWorker.Text);
            UcPolymerFormProductionResult.ShiftDateTime = Util.NVC(UcPolymerFormShift.TextShiftDateTime.Text);
            UcPolymerFormProductionResult.ShiftName = Util.NVC(UcPolymerFormShift.TextShift.Text);
        }

        private void GetUserControlEquipmentCode()
        {
            string equipmentCode = string.Empty;
            string equipmentSegment = string.Empty;

            equipmentCode = ComboEquipment.SelectedValue.GetString();
            equipmentSegment = ComboEquipmentSegment.SelectedValue.GetString();

            UcPolymerFormInput?.ChangeEquipment(equipmentCode, ComboEquipment.Text);
            UcPolymerFormCart?.ChangeEquipment(equipmentCode);
            UcPolymerFormCart.EquipmentName = ComboEquipment.Text;
            UcPolymerFormProductionResult?.ChangeEquipment(equipmentCode, equipmentSegment);
        }

        private void GetUserControlInspectorCode()
        {
            string InspectorCode = ComboInspector.SelectedValue.GetString();

            _inspectorCode = InspectorCode;
            UcPolymerFormProductionResult.InspectorCode = InspectorCode;

            string[] inspectorItems = ComboInspector.Text.Split(' ');
            if (inspectorItems.Length > 0)
            {
                UcPolymerFormProductionResult.InspectorId = inspectorItems[0].Replace("(", "").Replace(")", "");
            }
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
