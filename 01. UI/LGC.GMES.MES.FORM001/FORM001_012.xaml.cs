/*************************************************************************************
 Created Date : 2017.07.28
      Creator : 
   Decription : 초소형 외관 검사 공정진척 
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
    public partial class FORM001_012 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        public UcFormCommand UcFormCommand { get; set; }
        public UcFormSearch UcFormSearch { get; set; }
        public UcFormProdLot UcFormProdLot { get; set; }
        public UcFormInput UcFormInput { get; set; }

        public UcFormProductionPalette UcFormProductionPallet { get; set; }
        public UcFormShift UcFormShift { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1DataGrid DgProductionPallet { get; set; }

        public C1DataGrid DgInputPallet { get; set; }

        private string _processCode;
        private string _processName;
        private bool _isLoaded = false;
        private int _tagPrintCount;

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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public FORM001_012()
        {
            InitializeComponent();
        }
        private void Initialize()
        {
            InitializeUserControls();
            InitializeUserControlsGrid();
            SetComboBox();
            SetEventInUserControls();

            string equipmentCode = string.Empty;

            if (ComboEquipment?.SelectedValue != null)
            {
                equipmentCode = ComboEquipment.SelectedValue.GetString();
                UcFormInput.EquipmentCode = ComboEquipment.SelectedValue.GetString();
            }

            UcFormInput?.ChangeEquipment(equipmentCode);
        }

        private void InitializeUserControls()
        {
            UcFormCommand = grdCommand.Children[0] as UcFormCommand;
            UcFormSearch = grdSearch.Children[0] as UcFormSearch;
            UcFormProdLot = grdProductLot.Children[0] as UcFormProdLot;
            UcFormInput = grdInput.Children[0] as UcFormInput;
            UcFormProductionPallet = grdProduction.Children[0] as UcFormProductionPalette;
            UcFormShift = grdShift.Children[0] as UcFormShift;

            if (UcFormCommand != null)
            {
                UcFormCommand.ProcessCode = _processCode;
                UcFormCommand.SetButtonVisibility();
            }

            if (UcFormSearch != null)
            {
                UcFormSearch.ProcessCode = _processCode;
                UcFormSearch.SetCheckBoxVisibility();

                ComboEquipment = UcFormSearch.ComboEquipment;
            }

            if (UcFormProdLot != null)
            {
                UcFormProdLot.UcParentControl = this;
                UcFormProdLot.ProcessCode = _processCode;
                UcFormProdLot.SetDataGridColumnVisibility();
            }

            if (UcFormInput != null)
            {
                UcFormInput.UcParentControl = this;
                UcFormInput.FrameOperation = FrameOperation;
                UcFormInput.ProcessCode = _processCode;
                UcFormInput.SetControlHeader();
                UcFormInput.SetDataGridColumnVisibility();
            }

            if (UcFormProductionPallet != null)
            {
                UcFormProductionPallet.UcParentControl = this;
                UcFormProductionPallet.ProcessCode = _processCode;
                UcFormProductionPallet.SetButtonVisibility();
                UcFormProductionPallet.SetControlHeader();
            }

        }

        private void InitializeUserControlsGrid()
        {
            DgProductLot = UcFormProdLot.DgProductLot;
            DgProductionPallet = UcFormProductionPallet.DgProductionPallet;
            DgInputPallet = UcFormInput.DgInputPallet;

        }

        private void SetEventInUserControls()
        {
            if (UcFormCommand != null)
            {
                UcFormCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                ////UcFormCommand.ButtonInspection.Click += ButtonInspection_Click;
                UcFormCommand.ButtonDefect.Click += ButtonDefect_Click;
                UcFormCommand.ButtonStart.Click += ButtonStart_Click;
                UcFormCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcFormCommand.ButtonCompletion.Click += ButtonCompletion_Click;
                UcFormCommand.ButtonInboxType.Click += ButtonInboxType_Click;
            }

            if (UcFormSearch != null)
            {
                UcFormSearch.ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;
                UcFormSearch.ButtonSearch.Click += ButtonSearch_Click;
                UcFormSearch.CheckRun.Checked += CheckWait_Checked;
                UcFormSearch.CheckEqpEnd.Checked += CheckWait_Checked;
                UcFormSearch.CheckRun.Unchecked += CheckWait_Checked;
                UcFormSearch.CheckEqpEnd.Unchecked += CheckWait_Checked;
            }

            if (UcFormInput != null)
            {
                UcFormInput.ButtonPalletRemainWait.Click += ButtonPalletRemainWait_Click;
            }

            if (UcFormProductionPallet != null)
            {
                //DgProductionPalette.CommittedEdit += dgProductionPalette_CommittedEdit;
                UcFormProductionPallet.ButtonPalletHold.Click += ButtonPalletHold_Click;
                UcFormProductionPallet.ButtonGoodPalletCreate.Click += ButtonGoodPalletCreate_Click;
                //UcFormProductionPallet.ButtonDefectPalletCreate.Click += ButtonDefectPalletCreate_Click;
                UcFormProductionPallet.ButtonPalletEdit.Click += ButtonPalletEdit_Click;
                UcFormProductionPallet.ButtonPalletDelete.Click += ButtonPalletDelete_Click;
                UcFormProductionPallet.ButtonTagPrint.Click += ButtonTagPrint_Click;
            }

            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

        }


        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.SmallAppearance;

            if (_isLoaded == false)
            {
                ApplyPermissions();
                Initialize();

                // 공정명 검색
                SelectProcessName();

                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
                    ButtonSearch_Click(UcFormSearch.ButtonSearch, null);
            }
            _isLoaded = true;
            ////this.Loaded -= UserControl_Loaded;

            UcFormInput.TextBoxPalletID.Focus();
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //UcFormProductionPallet.DispatcherTimer?.Stop();
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

        private void ButtonDefect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefect())
                return;

            FORM001_DEFECT popupDefect = new FORM001_DEFECT { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupDefect.Name.ToString()) == false)
                return;

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = UcFormProdLot.GetSelectProductLotRow();
            C1WindowExtension.SetParameters(popupDefect, parameters);

            popupDefect.Closed += new EventHandler(popupDefect_Closed);
            grdMain.Children.Add(popupDefect);
            popupDefect.BringToFront();

        }

        private void popupDefect_Closed(object sender, EventArgs e)
        {
            FORM001_DEFECT popup = sender as FORM001_DEFECT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true);
            }
            grdMain.Children.Remove(popup);

            UcFormInput.TextBoxPalletID.Focus();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
                return;

            FORM001_012_RUNSTART popupRunStart = new FORM001_012_RUNSTART { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupRunStart.Name.ToString()) == false)
                return;

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();

        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            FORM001_012_RUNSTART popup = sender as FORM001_012_RUNSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot(false, popup.ProdLotId);
            }
            grdMain.Children.Remove(popup);

            UcFormInput.TextBoxPalletID.Focus();
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

        private void ButtonCompletion_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCompletion())
                return;

            FORM001_012_CONFIRM popupConfirm = new FORM001_012_CONFIRM { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupConfirm.Name.ToString()) == false)
                return;

            popupConfirm.ShiftID = Util.NVC(UcFormShift.TextShift.Tag);
            popupConfirm.ShiftName = Util.NVC(UcFormShift.TextShift.Text);
            popupConfirm.WorkerID = Util.NVC(UcFormShift.TextWorker.Tag);
            popupConfirm.WorkerName = Util.NVC(UcFormShift.TextWorker.Text);
            popupConfirm.ShiftDateTime = Util.NVC(UcFormShift.TextShiftDateTime.Text);

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = UcFormProdLot.GetSelectProductLotRow();
            C1WindowExtension.SetParameters(popupConfirm, parameters);

            popupConfirm.Closed += new EventHandler(popupConfirm_Closed);
            grdMain.Children.Add(popupConfirm);
            popupConfirm.BringToFront();

        }

        private void popupConfirm_Closed(object sender, EventArgs e)
        {
            FORM001_012_CONFIRM popup = sender as FORM001_012_CONFIRM;
            if (popup.ConfirmSave)
            {
                ClearControls();
                GetProductLot(false);
            }
            GetEqptWrkInfo();
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

            FORM001_EQPT_INBOX_TYPE popupInboxTyp = new FORM001_EQPT_INBOX_TYPE { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupInboxTyp.Name.ToString()) == false)
                return;

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
            FORM001_EQPT_INBOX_TYPE popup = sender as FORM001_EQPT_INBOX_TYPE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);

        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            GetProductLot(false);
            GetEqptWrkInfo();

            UcFormInput.TextBoxPalletID.Focus();
        }

        /// <summary>
        /// Palette 잔량대기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPalletRemainWait_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletRemain()) return;

            FORM001_PALETTE_WAIT_REMAIN popupPalletRemain = new FORM001_PALETTE_WAIT_REMAIN { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popupPalletRemain.Name.ToString()) == false)
                return;

            popupPalletRemain.ShiftID = Util.NVC(UcFormShift.TextShift.Tag);
            popupPalletRemain.ShiftName = Util.NVC(UcFormShift.TextShift.Text);
            popupPalletRemain.WorkerID = Util.NVC(UcFormShift.TextWorker.Tag);
            popupPalletRemain.WorkerName = Util.NVC(UcFormShift.TextWorker.Text);
            popupPalletRemain.ShiftDateTime = Util.NVC(UcFormShift.TextShiftDateTime.Text);

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            popupPalletRemain.DifferenceQty = Util.NVC_Int(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "DIFF_QTY").ToString());

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = UcFormInput.GetSelectInputPalletRow();
            C1WindowExtension.SetParameters(popupPalletRemain, parameters);

            popupPalletRemain.Closed += new EventHandler(popupPalletRemainWait_Closed);
            grdMain.Children.Add(popupPalletRemain);
            popupPalletRemain.BringToFront();
        }
        private void popupPalletRemainWait_Closed(object sender, EventArgs e)
        {
            FORM001_PALETTE_WAIT_REMAIN popup = sender as FORM001_PALETTE_WAIT_REMAIN;
            if (popup.ConfirmSave)
            {
                GetProductLot(true);
                UcFormInput.SelectInputPalletList();
            }
            GetEqptWrkInfo();
            this.grdMain.Children.Remove(popup);

            UcFormInput.TextBoxPalletID.Focus();
        }

        /// <summary>
        /// Palette Hold
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPalletHold_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletHold()) return;

            FORM001_PALETTE_HOLD popupPalletHold = new FORM001_PALETTE_HOLD { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popupPalletHold.Name.ToString()) == false)
                return;

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = UcFormProdLot.GetSelectProductLotRow();
            parameters[5] = UcFormProductionPallet.GetSelectPalletLotRow();
            C1WindowExtension.SetParameters(popupPalletHold, parameters);

            popupPalletHold.Closed += new EventHandler(popupPalletHold_Closed);
            grdMain.Children.Add(popupPalletHold);
            popupPalletHold.BringToFront();
        }
        private void popupPalletHold_Closed(object sender, EventArgs e)
        {
            FORM001_PALETTE_HOLD popup = sender as FORM001_PALETTE_HOLD;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcFormProductionPallet.SelectPalletList();
                //UcFormProductionPallet.SelectPalletSummary();
            }
            this.grdMain.Children.Remove(popup);

            UcFormInput.TextBoxPalletID.Focus();
        }

        /// <summary>
        /// 양품 Palette 생성
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonGoodPalletCreate_Click(object sender, RoutedEventArgs e)
        {
            SetPopupGoodPalletCreate();
        }

        ///// <summary>
        ///// 불량 Palette 생성
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ButtonDefectPalletCreate_Click(object sender, RoutedEventArgs e)
        //{
        //    SetPopupGoodPalletCreate();
        //}

        /// <summary>
        /// Palette 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPalletEdit_Click(object sender, RoutedEventArgs e)
        {
            SetPopupGoodPalletEdit();
        }

        private void SetPopupGoodPalletCreate()
        {
            if (!ValidationGoodPalletCreate()) return;

            FORM001_012_PALETTE_CREATE popupPalletCreate = new FORM001_012_PALETTE_CREATE { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popupPalletCreate.Name.ToString()) == false)
                return;

            popupPalletCreate.ShiftID = Util.NVC(UcFormShift.TextShift.Tag);
            popupPalletCreate.ShiftName = Util.NVC(UcFormShift.TextShift.Text);
            popupPalletCreate.WorkerID = Util.NVC(UcFormShift.TextWorker.Tag);
            popupPalletCreate.WorkerName = Util.NVC(UcFormShift.TextWorker.Text);
            popupPalletCreate.ShiftDateTime = Util.NVC(UcFormShift.TextShiftDateTime.Text);

            object[] parameters = new object[7];
            parameters[0] = "Y";                      // 생성, 수정 구분
            parameters[1] = _processCode;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = UcFormProdLot.GetSelectProductLotRow();
            parameters[4] = _processName;
            parameters[5] = Util.NVC(ComboEquipment.Text);
            parameters[6] = null;
            C1WindowExtension.SetParameters(popupPalletCreate, parameters);

            popupPalletCreate.Closed += new EventHandler(popupPalletCreate_Closed);
            grdMain.Children.Add(popupPalletCreate);
            popupPalletCreate.BringToFront();
        }

        private void SetPopupGoodPalletEdit()
        {
            if (!ValidationPalletEdit()) return;

            FORM001_012_PALETTE_CREATE popupPalletEdit = new FORM001_012_PALETTE_CREATE { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popupPalletEdit.Name.ToString()) == false)
                return;

            popupPalletEdit.ShiftID = Util.NVC(UcFormShift.TextShift.Tag);
            popupPalletEdit.ShiftName = Util.NVC(UcFormShift.TextShift.Text);
            popupPalletEdit.WorkerID = Util.NVC(UcFormShift.TextWorker.Tag);
            popupPalletEdit.WorkerName = Util.NVC(UcFormShift.TextWorker.Text);
            popupPalletEdit.ShiftDateTime = Util.NVC(UcFormShift.TextShiftDateTime.Text);

            object[] parameters = new object[7];
            parameters[0] = "N";                      // 생성, 수정 구분
            parameters[1] = _processCode;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = UcFormProdLot.GetSelectProductLotRow();
            parameters[4] = _processName;
            parameters[5] = Util.NVC(ComboEquipment.Text);
            parameters[6] = UcFormProductionPallet.GetSelectPalletLotRow();
            C1WindowExtension.SetParameters(popupPalletEdit, parameters);

            popupPalletEdit.Closed += new EventHandler(popupPalletCreate_Closed);
            grdMain.Children.Add(popupPalletEdit);
            popupPalletEdit.BringToFront();
        }

        private void popupPalletCreate_Closed(object sender, EventArgs e)
        {
            FORM001_012_PALETTE_CREATE popup = sender as FORM001_012_PALETTE_CREATE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true);
                UcFormProductionPallet.SelectPalletList();
                UcFormProductionPallet.SelectPalletSummary();
            }
            GetEqptWrkInfo();
            this.grdMain.Children.Remove(popup);

            UcFormInput.TextBoxPalletID.Focus();
        }

        /// <summary>
        /// Palette 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletDelete()) return;

            FORM001_PALETTE_DELETE popupPalletDelete = new FORM001_PALETTE_DELETE { FrameOperation = this.FrameOperation };

            if (ValidationGridAdd(popupPalletDelete.Name.ToString()) == false)
                return;

            DataRow[] dr = DataTableConverter.Convert(DgProductionPallet.ItemsSource).Select("CHK = 1");

            object[] parameters = new object[3];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipment.SelectedValue);
            ////parameters[2] = UcFormProductionPallet.GetSelectPalletLotRow();
            parameters[2] = dr;
            C1WindowExtension.SetParameters(popupPalletDelete, parameters);

            popupPalletDelete.Closed += new EventHandler(popupPalletDelete_Closed);
            grdMain.Children.Add(popupPalletDelete);
            popupPalletDelete.BringToFront();
        }

        private void popupPalletDelete_Closed(object sender, EventArgs e)
        {
            FORM001_PALETTE_DELETE popup = sender as FORM001_PALETTE_DELETE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true);
                UcFormProductionPallet.SelectPalletList();
                UcFormProductionPallet.SelectPalletSummary();
            }
            this.grdMain.Children.Remove(popup);

            UcFormInput.TextBoxPalletID.Focus();
        }

        /// <summary>
        /// 태그 발행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTagPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTagPrint()) return;

            //CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            //popupTagPrint.FrameOperation = this.FrameOperation;

            //// SET PARAMETER
            //int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductionPallet, "CHK");

            //if (DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "WIPHOLD").GetString().Equals("Y"))
            //    popupTagPrint.HoldPalletYN = "Y";

            //object[] parameters = new object[8];
            //parameters[0] = _processCode;
            //parameters[1] = Util.NVC(ComboEquipment.SelectedValue);
            //parameters[2] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "PALLETE_ID").GetString();
            //parameters[3] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "WIPSEQ").GetString();
            //parameters[4] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "CELL_QTY").GetString();
            ////parameters[5] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "DISPATCH_YN").GetString();
            //parameters[5] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "DISPATCH_YN").GetString().Equals("Y") ? "N" : "Y";      // 디스패치 처리
            //parameters[6] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "PRINT_YN").GetString();
            //parameters[7] = "N";      // Direct 출력 여부

            //C1WindowExtension.SetParameters(popupTagPrint, parameters);

            //popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            //grdMain.Children.Add(popupTagPrint);
            //popupTagPrint.BringToFront();

            DataRow[] drSelect = DataTableConverter.Convert(DgProductionPallet.ItemsSource).Select("CHK = 1");

            _tagPrintCount = drSelect.Length;

            foreach (DataRow drPrint in drSelect)
            {
                TagPrint(drPrint);
            }

        }

        private void TagPrint(DataRow dr)
        {
            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            if (dr["WIPHOLD"].GetString().Equals("Y"))
                popupTagPrint.HoldPalletYN = "Y";

            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[8];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = dr["PALLETE_ID"];
            parameters[3] = dr["WIPSEQ"].GetString();
            parameters[4] = dr["CELL_QTY"].GetString();
            //parameters[5] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "DISPATCH_YN").GetString();
            parameters[5] = dr["DISPATCH_YN"].GetString().Equals("Y") ? "N" : "Y";   // 디스패치 처리
            parameters[6] = dr["PRINT_YN"].GetString();
            parameters[7] = "N";      // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcFormProductionPallet.SelectPalletList();
                //UcFormProductionPallet.SelectPalletSummary();
            }
            this.grdMain.Children.Remove(popup);

            UcFormInput.TextBoxPalletID.Focus();
        }

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.ToString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
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

            UcFormInput.TextBoxPalletID.Focus();
        }

        private void CheckWait_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            if (ComboEquipment.SelectedValue.GetString() != "SELECT")
            {
                this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcFormSearch.ButtonSearch, null)));
            }
        }

        private void ComboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                ClearControls();
                UcFormShift.ClearShiftControl();
                UcFormInput?.ChangeEquipment(ComboEquipment.SelectedValue.ToString());

                // 설비 선택 시 자동 조회 처리
                if (ComboEquipment != null && (ComboEquipment.SelectedIndex > 0 && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex))
                {
                    if (ComboEquipment.SelectedValue.GetString() != "SELECT")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcFormSearch.ButtonSearch, null)));
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
                if (UcFormSearch.CheckRun.IsChecked == false && UcFormSearch.CheckEqpEnd.IsChecked == false)
                {
                    Util.MessageValidation("SFU1904");
                    return;
                }

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


                List<string> condition = new List<string>();

                if (UcFormSearch.CheckRun.IsChecked == true)
                    condition.Add(UcFormSearch.CheckRun.Tag.ToString());

                if (UcFormSearch.CheckEqpEnd.IsChecked == true)
                    condition.Add(UcFormSearch.CheckEqpEnd.Tag.ToString());

                var wipState = string.Join(",", condition);
                wipState = wipState + ",";

                if (string.IsNullOrEmpty(wipState.Trim()))
                {
                    //WIP 상태를 선택하세요.
                    Util.MessageValidation("SFU1438");
                    return;
                }

                ////UcFormInput.InitializeControls();
                ////UcFormProductionPallet.InitializeControls();

                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_FO";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPTYPECODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;

                if (UcFormSearch.CheckRun.IsChecked == true || UcFormSearch.CheckEqpEnd.IsChecked == true) // WAIT는 설비명이 없음
                    newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);

                newRow["WIPSTAT"] = wipState;
                newRow["PROCID"] = _processCode;
                newRow["WIPTYPECODE"] = "PROD";

                inTable.Rows.Add(newRow);
                ////DataSet ds = new DataSet();
                ////ds.Tables.Add(inTable);
                ////string xml = ds.GetXml();

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
                            UcFormInput.InitializeControls();
                            UcFormProductionPallet.InitializeControls();
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

        public DataRow GetSelectProductRow()
        {
            DataRow row = null;

            try
            {
                row = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
                return row;
            }
            catch (Exception)
            {
                return row;
            }
        }

        public bool GetSearchConditions(out string processCode, out string equipmentCode)
        {
            try
            {
                processCode = _processCode;
                equipmentCode = ComboEquipment.SelectedIndex >= 0 ? ComboEquipment.SelectedValue.ToString() : string.Empty;

                return true;
            }
            catch (Exception)
            {
                processCode = string.Empty;
                equipmentCode = string.Empty;
                return false;
            }
        }

        public bool ClearControls()
        {
            try
            {
                Util.gridClear(DgProductLot);
                UcFormInput.InitializeControls();
                UcFormProductionPallet.InitializeControls();
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
            UcFormProductionPallet.InitializeControls();
        }

        public void ProdListClickedProcess(int iRow)
        {
            try
            {
                if (iRow < 0 || !_util.GetDataGridCheckValue(DgProductLot, "CHK", iRow)) return;
                string lotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                string wipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));

                UcFormInput.ProdLotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                UcFormInput.ProdWipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));

                UcFormProductionPallet.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcFormProductionPallet.ProcessCode = _processCode;
                UcFormProductionPallet.ProdLotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                UcFormProductionPallet.WipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));

                // Clear
                UcFormInput.InitializeControls();
                UcFormProductionPallet.InitializeControls();

                // 투입 Pallet 데이터 조회
                UcFormInput.SelectInputPalletList();

                // 완성Palette 데이터 조회
                UcFormProductionPallet.SelectPalletList();
                UcFormProductionPallet.SelectPalletSummary();
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
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _processCode;

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

        private bool ValidationSearch()
        {

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // "설비를 선택 하세요."
                Util.MessageValidation("SFU1673");
                return false;
            }

            ////if (UcFormSearch.CheckEqpEnd.IsChecked != null && UcFormSearch.CheckRun.IsChecked != null && !(bool)UcFormSearch.CheckRun.IsChecked && !(bool)UcFormSearch.CheckEqpEnd.IsChecked)
            ////{
            ////    // LOT 상태 선택 조건을 하나 이상 선택하세요.
            ////    Util.MessageValidation("SFU1370");
            ////    return false;
            ////}

            return true;
        }

        private bool ValidationDefect()
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
            if (DgProductionPallet.Rows.Count - DgProductionPallet.FrozenBottomRowsCount > 0)
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

            //if (DgProductionPallet.Rows.Count - DgProductionPallet.BottomRows.Count == 0)
            //{
            //    // 실적 확정할 데이터가 없습니다. 아래쪽 List를 확인하세요.
            //    Util.MessageValidation("SFU3157");
            //    return false;
            //}

            ////DataRow[] drchk = DataTableConverter.Convert(DgProductionPallet.ItemsSource).Select("DISPATCH_YN <> 'Y'");

            ////if (drchk.Length > 0)
            ////{
            ////    // Pallet 태그를 전부 발행해야 작업 완료 처리가 가능 합니다.
            ////    Util.MessageValidation("SFU4013");
            ////    return false;
            ////}

            DataRow[] drchk = DataTableConverter.Convert(DgProductionPallet.ItemsSource).Select("PRINT_YN <> 'Y'");

            if (drchk.Length > 0)
            {
                // Pallet 태그를 전부 발행해야 작업 완료 처리가 가능 합니다.
                Util.MessageValidation("SFU4013");
                return false;
            }

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

        private bool ValidationPalletHold()
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

            int rowChkCount = DataTableConverter.Convert(DgProductionPallet.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

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

            rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductionPallet, "CHK");
            if (DataTableConverter.GetValue(DgProductionPallet.Rows[rowIndex].DataItem, "WIPHOLD").GetString().Equals("Y"))
            {
                // HOLD 된 LOT ID 입니다.
                Util.MessageValidation("SFU1340");
                return false;
            }


            return true;
        }

        private bool ValidationGoodPalletCreate()
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

            return true;
        }

        private bool ValidationPalletEdit()
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

            int rowChkCount = DataTableConverter.Convert(DgProductionPallet.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

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

            rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductionPallet, "CHK");

            if (DataTableConverter.GetValue(DgProductionPallet.Rows[rowIndex].DataItem, "WIPHOLD").GetString().Equals("Y"))
            {
                // Hold 처리된 팔레트는 팔레트 수정이 불가합니다.
                Util.MessageValidation("SFU4064");
                return false;
            }

            return true;
        }

        private bool ValidationPalletDelete()
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
                Util.MessageValidation("SFU1651");
                return false;
            }

            rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductionPallet, "CHK");

            if (rowIndex < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (DataTableConverter.GetValue(DgProductionPallet.Rows[rowIndex].DataItem, "WIPHOLD").GetString().Equals("Y"))
            {
                // Hold 처리된 팔레트는 팔레트 삭제가 불가합니다.
                Util.MessageValidation("SFU4065");
                return false;
            }

            return true;
        }

        private bool ValidationTagPrint()
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
                Util.MessageValidation("SFU1651");
                return false;
            }

            int rowChkCount = DataTableConverter.Convert(DgProductionPallet.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            ////// 수정인 경우 Pallet 체크
            ////rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductionPallet, "CHK");

            ////if (rowIndex < 0)
            ////{
            ////    // 선택된 항목이 없습니다.
            ////    Util.MessageValidation("SFU1651");
            ////    return false;
            ////}

            if (rowChkCount == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            ////if (rowChkCount > 1)
            ////{
            ////    // 한행만 선택 가능 합니다.
            ////    Util.MessageValidation("SFU4023");
            ////    return false;
            ////}

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

        /// <summary>
        /// System 시간
        /// </summary>
        /// <returns></returns>
        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void SetComboBox()
        {
            SetEquipmentCombo(ComboEquipment);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, _processCode, null };
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
            var ucFormCommand = grdCommand.Children[0] as UcFormCommand;
            if (ucFormCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                    ucFormCommand.ButtonInspection,
                    ucFormCommand.ButtonStart,
                    ucFormCommand.ButtonCancel,
                    ucFormCommand.ButtonCompletion
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }
        #endregion

    }
}
