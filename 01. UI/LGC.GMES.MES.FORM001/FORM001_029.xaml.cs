/*************************************************************************************
 Created Date : 2017.12.04
      Creator : 신 광희
   Decription : 최종외관검사 공정진척(F8000 : DSF -> DSF 최종외관, F8100 : 특성 최종외관, F5600 : Offline 특성 측정 (파우치형))
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.04   신광희C : 최초 생성
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_029 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _processCode;
        private string _processName;
        private string _divisionCode;
        private string _inspectorCode;
        private bool _isLoaded = false;

        /// 2018-03-06 불량탭 추가
        private int _defectGradeCount;
        private bool _IsDefectSave;
        private DataTable _defectList;
        private bool _defectTabButtonClick;

        public UcPolymerFormCommand UcPolymerFormCommand { get; set; }
        public UcPolymerFormSearch UcPolymerFormSearch { get; set; }
        public UcPolymerFormProdLot UcPolymerFormProdLot { get; set; }
        public UcPolymerFormShift UcPolymerFormShift { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1ComboBox ComboProcess { get; set; }
        public C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        public C1ComboBox ComboInspector { get; set; }

        public string ProcessCode
        {
            get { return _processCode; }
            set {_processCode = value;}
        }
        public string ProcessName
        {
            get { return _processName;}
            set {_processName = value;}
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
        public FORM001_029()
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
        }

        private void InitializeUserControls()
        {
            UcPolymerFormCommand = grdCommand.Children[0] as UcPolymerFormCommand;
            UcPolymerFormSearch = grdSearch.Children[0] as UcPolymerFormSearch;
            UcPolymerFormProdLot = grdProductLot.Children[0] as UcPolymerFormProdLot;
            UcPolymerFormShift = grdShift.Children[0] as UcPolymerFormShift;
            
            if (UcPolymerFormCommand != null)
            {
                UcPolymerFormCommand.ProcessCode = _processCode;
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

            if (UcPolymerFormProdLot != null)
            {
                UcPolymerFormProdLot.UcParentControl = this;
                UcPolymerFormProdLot.ProcessCode = _processCode;
                UcPolymerFormProdLot.SetDataGridColumnVisibility();
            }

            _defectTabButtonClick = false;
        }

        private void InitializeUserControlsGrid()
        {
            DgProductLot = UcPolymerFormProdLot.DgProductLot;
        }

        private void SetEventInUserControls()
        {
            if (UcPolymerFormCommand != null)
            {
                UcPolymerFormCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                UcPolymerFormCommand.ButtonTakeOver.Click += ButtonTakeOver_Click;
                UcPolymerFormCommand.ButtonCartDefect.Click += ButtonCartDefect_Click;
                UcPolymerFormCommand.ButtonSublotDefect.Click += ButtonSublotDefect_Click;

                UcPolymerFormCommand.ButtonStart.Click += ButtonStart_Click;
                UcPolymerFormCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcPolymerFormCommand.ButtonConfirm.Click += ButtonConfirm_Click;

                UcPolymerFormCommand.ButtonCartMove.Click += ButtonCartMove_Click;
                UcPolymerFormCommand.ButtonCartStorage.Click += ButtonCartStorage_Click;
            }

            if (UcPolymerFormSearch != null)
            {
                UcPolymerFormSearch.ComboProcess.SelectedValueChanged += ComboProcess_SelectedValueChanged;
                UcPolymerFormSearch.ComboEquipmentSegment.SelectedValueChanged += ComboEquipmentSegment_SelectedValueChanged;
                UcPolymerFormSearch.ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;
                UcPolymerFormSearch.ComboInspector.SelectedValueChanged += ComboInspector_SelectedValueChanged;
                UcPolymerFormSearch.ButtonSearch.Click += ButtonSearch_Click;
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
                _processCode = Process.PolymerFinalExternal;
                _divisionCode = "Pallet";

                ApplyPermissions();
                Initialize();

                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT") && ComboEquipment.SelectedIndex > -1)
                    ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null);

                _isLoaded = true;
            }
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
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
            if (!ValidationRunStart()) return;

            //FORM001_029_RUNSTART popupRunStart = new FORM001_029_RUNSTART { FrameOperation = FrameOperation };
            FORM001_RUNSTART popupRunStart = new FORM001_RUNSTART { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupRunStart.Name) == false)
                return;

            popupRunStart.ShiftID = UcPolymerFormShift.TextShift.Tag.GetString();
            popupRunStart.ShiftName = UcPolymerFormShift.TextShift.Text;
            popupRunStart.WorkerID = UcPolymerFormShift.TextWorker.Tag.GetString();
            popupRunStart.WorkerName = UcPolymerFormShift.TextWorker.Text;
            popupRunStart.ShiftDateTime = UcPolymerFormShift.TextShiftDateTime.Text;
            popupRunStart.InspectorCode = ComboInspector.SelectedValue.ToString();

            object[] parameters = new object[6];
            parameters[0] = Util.NVC(ComboProcess.SelectedValue);
            parameters[1] = Util.NVC(ComboProcess.Text);
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
            //FORM001_029_RUNSTART popup = sender as FORM001_029_RUNSTART;
            FORM001_RUNSTART popup = sender as FORM001_RUNSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductCart(popup.ProdCartId);
            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;

            //Util.MessageConfirm("SFU1243", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        CancelRun();
            //    }
            //});

            FORM001_029_CANCEL popupCancel = new FORM001_029_CANCEL { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupCancel.Name) == false)
                return;

            object[] parameters = new object[6];
            parameters[0] = Util.NVC(ComboProcess.SelectedValue);
            parameters[1] = Util.NVC(ComboProcess.Text);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetDataGridFirstRowBycheck(dgProductCart, "CHK"); ;
            parameters[5] = DataTableConverter.Convert(DgProductLot.ItemsSource);
            C1WindowExtension.SetParameters(popupCancel, parameters);

            popupCancel.Closed += new EventHandler(popupCancel_Closed);
            grdMain.Children.Add(popupCancel);
            popupCancel.BringToFront();

        }

        private void popupCancel_Closed(object sender, EventArgs e)
        {
            FORM001_029_CANCEL popup = sender as FORM001_029_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductCart();
            }
            grdMain.Children.Remove(popup);
        }


        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfirm())
                return;

            FORM001_CONFIRM popupConfirm = new FORM001_CONFIRM { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupConfirm.Name) == false)
                return;

            popupConfirm.ShiftID = UcPolymerFormShift.TextShift.Tag.GetString();
            popupConfirm.ShiftName = UcPolymerFormShift.TextShift.Text;
            popupConfirm.WorkerID = UcPolymerFormShift.TextWorker.Tag.GetString();
            popupConfirm.WorkerName = UcPolymerFormShift.TextWorker.Text;
            popupConfirm.ShiftDateTime = UcPolymerFormShift.TextShiftDateTime.Text;

            // 작업대차에 양품량이 전부 0이고 마지막 실적 확정여부
            DataTable dt = DataTableConverter.Convert(DgProductLot.ItemsSource);
            DataRow[] drStat = dt.Select("WIPSTAT = 'PROC'");
            DataRow[] drZero = dt.Select("GOOD_QTY = 0");

            if (drStat.Length == 1)
            {
                popupConfirm.LastConfirm = dt.Rows.Count - drZero.Length == 0 ? true : false;
            }
            else
            {
                popupConfirm.LastConfirm = false;
            }

            object[] parameters = new object[8];
            parameters[0] = Util.NVC(ComboProcess.SelectedValue);
            parameters[1] = Util.NVC(ComboProcess.Text); 
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
                GetProductCart();
            }
            GetEquipmentWorkInfo();
            //SetInspectorCombo(ComboInspector);
            grdMain.Children.Remove(popup);
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            GetEquipmentWorkInfo();
            GetProductCart();
            //GetProductLot();
            //SetInspectorCombo(ComboInspector);

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
            parameters[3] = Util.NVC(ComboProcess.SelectedValue);
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
                GetEquipmentWorkInfo();
                //SetInspectorCombo(ComboInspector);
            }
            this.grdMain.Children.Remove(popup);
        }

        private void ComboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectProcessName();
            SetEquipmentSegmentCombo(ComboEquipmentSegment);
        }

        private void ComboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ComboEquipment.SelectedValueChanged -= ComboEquipment_SelectedValueChanged;
            SetEquipmentCombo(ComboEquipment);
            ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;

            // 뷸량 등록시 대상등급선택 콤보 2018-04-05
            cboGradeSelect.SelectionChanged -= cboGradeSelect_SelectionChanged;
            cboGradeSelect.ApplyTemplate();
            SetGradeCombo(cboGradeSelect);
            cboGradeSelect.SelectionChanged += cboGradeSelect_SelectionChanged;

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

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void cboGradeSelect_SelectionChanged(object sender, EventArgs e)
        {
            SetDefectGridGradeColumn();
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
            parameters[0] = Util.NVC(ComboProcess.SelectedValue);
            parameters[1] = Util.NVC(ComboProcess.Text);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetDataGridFirstRowBycheck(dgProductCart, "CHK").Field<string>("CTNR_ID").GetString();
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
            parameters[0] = Util.NVC(ComboProcess.SelectedValue);
            parameters[1] = Util.NVC(ComboProcess.Text);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetDataGridFirstRowBycheck(dgProductCart, "CHK").Field<string>("CTNR_ID").GetString();

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
                GetProductCart();
            }
            grdMain.Children.Remove(popup);
        }

        #region 대차
        private void dgProductCartChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgProductCart.SelectedIndex = idx;

                        GetProductLot(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region ### 완성 Inbox 탭 ###
        private void txtInBoxId_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtInBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (CommonVerify.HasDataGridRow(dgProductionInbox))
                    {
                        int rowIndex = _util.GetDataGridFirstRowIndexByColumnValue(dgProductionInbox, "INBOX_ID", txtInBoxId.Text);
                        if (rowIndex > -1)
                        {
                            DataTableConverter.SetValue(dgProductionInbox.Rows[rowIndex].DataItem, "CHK", true);
                            dgProductionInbox.SelectedIndex = rowIndex;
                        }
                        else
                        {
                            Util.MessageValidation("SFU2060");
                        }
                    }
                    else
                    {
                        Util.MessageValidation("SFU2060");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCartRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            DataTable dt = PrintCartData();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }

            // Page수 산출
            int PageCount = dt.Rows.Count % 40 != 0 ? (dt.Rows.Count / 40) + 1 : dt.Rows.Count / 40;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * 40) + 1;
                end = ((cnt + 1) * 40);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);
                CartRePrint(dr, cnt+1);
            }

        }

        private void btnInBoxSave_Click(object sender, RoutedEventArgs e)
        {
            dgProductionInbox.EndEdit();
            dgProductionInbox.EndEditRow(true);

            if (!ValidationInBoxSave()) return;

            // Inbox를 수정 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            Util.MessageConfirm("SFU4331", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ModifyInbox();
                }
            }, parameters);
        }

        private void btnInBoxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInBoxDelete()) return;

            // IInbox를 삭제 하시겠습니까?
            object[] parameters = new object[1];
            parameters[0] = ObjectDic.Instance.GetObjectName("INBOX");
            Util.MessageConfirm("SFU4332", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteInbox();
                }
            }, parameters);
        }

        private void btnInBoxDeleteCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInBoxDeleteCancel()) return;

            // 삭제 취소 하시겠습니까?
            Util.MessageConfirm("SFU4369", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelDeleteInbox();
                }
            });
        }

        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectPrint(dgProductionInbox, true)) return;

            PrintLabel("G", dgProductionInbox);
        }

        /// <summary>
        /// 2018-03-06 불량탭 관련 추가
        /// </summary>
        private void btnDefectInput_Click(object sender, RoutedEventArgs e)
        {
            btnDefectInput.Background = new SolidColorBrush(Colors.Red);
            btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Black);

            SetDefectGridInput(false);
        }

        private void btnDefectPrintSelect_Click(object sender, RoutedEventArgs e)
        {
            btnDefectInput.Background = new SolidColorBrush(Colors.Black);
            btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Red);

            SetDefectGridInput(true);
        }

        private void btnDefectPrint_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationDefectPrint(dgDefect, false)) return;

            PrintLabel("N", dgDefect);
        }

        private void dgProductionInbox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8");

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DELETE_YN")).Equals("Y"))
                    {
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgProductionInbox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgProductionInbox_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (sender == null || e.Cell == null)
                return;

            if (e.Cell.Row.Type == DataGridRowType.Item)
            {
                if (e.Cell.Column.Name.Equals("CELL_QTY") && e.Cell.IsEditable == true)
                {
                    DataRow dr = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
                    if (!Util.NVC(dr["WIPSTAT"]).Equals("PROC")) return;

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", true);
                }
            }

        }

        private void dgProductionInbox_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("CELL_QTY"))
            {
                DataRow dr = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");

                // 완성된 생산LOT Inbox는 수정 불가
                if (!Util.NVC(dr["WIPSTAT"]).Equals("PROC"))
                {
                    e.Cancel = true;
                }

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DELETE_YN")).Equals("Y"))
                {
                    e.Cancel = true;
                }

            }
        }

        private void chkAllInbox_Checked(object sender, RoutedEventArgs e)
        {
            DataRow dr = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            if (!Util.NVC(dr["WIPSTAT"]).Equals("PROC")) return;

            Util.DataGridCheckAllChecked(dgProductionInbox);
        }

        private void chkAllInbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgProductionInbox);
        }
        #endregion

        #region ### 불량 탭 ###
        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            dgDefect.EndEdit();
            dgDefect.EndEditRow(true);

            if (!ValidationDefectSave()) return;

            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveDefect();
                }
            });
        }

        ///
        /// 2018-03-06 불량탭 수정
        ///
        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Row.Type != DataGridRowType.Item)
                return;

            if (e.Cell.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
            {
                CheckBox cbo = e.Cell.Presenter.Content as CheckBox;
                if (cbo != null && cbo.IsChecked == true)
                {
                    cbo.Visibility = DataTableConverter.GetValue(e.Cell.Row.DataItem, "ACTID").GetString() != "CHARGE_PROD_LOT" ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            dgDefect?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // IsReadOnly=True 색상표시
                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible || e.Cell.Column.Name.Equals("CHK"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }

                // 출력 색상 표시
                if (e.Cell.Column.Name.IndexOf("GRADE") > -1 && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(45);

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN" + e.Cell.Column.Name.Substring(5, e.Cell.Column.Name.Length - 5))).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F7C8"));
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").Equals(1))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD0DA"));
                    }
                    else
                    {
                        if (e.Cell.Column.IsReadOnly == true)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEBEB"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }
            }));

        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
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

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Column != null)
                {
                    if (e.Column.Name == "RESNQTY")
                    {
                        DataRowView drv = e.Row.DataItem as DataRowView;
                        if (drv != null)
                        {
                            e.Cancel = drv["DFCT_QTY_CHG_BLOCK_FLAG"].GetString() == "Y";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 2018-03-06 불량탭 추가
        /// </summary>
        private void dgDefect_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                if (cell.Column.IsReadOnly == true)
                {
                    return;
                }

                SetDefectGridSelect(cell.Row.Index, cell.Column.Index);
            }

        }

        /// <summary>
        /// 2018-03-06 불량탭 추가
        /// </summary>
        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (e.Cell.Column.Name.IndexOf("GRADE") > -1)
                {
                    double ResnQtyColumnSum = 0;
                    int GradeColumn = _defectGradeCount + dgDefect.Columns["GRADE1"].Index;

                    for (int col = dgDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        ResnQtyColumnSum += DataTableConverter.GetValue(e.Cell.Row.DataItem, dgDefect.Columns[col].Name).GetDouble();
                    }
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "RESNQTY", ResnQtyColumnSum);
                }
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllChecked(dgDefect);

            // 2018-03-06 불량탭 추가
            SetDefectGridSelect();
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgDefect);

            // 2018-03-06 불량탭 추가
            // Load Event 호출
            DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);
            Util.GridSetData(dgDefect, dt, null, true);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn != null)
            {
                // 등록 팝업 ?
                string defectCode = DataTableConverter.GetValue(btn.DataContext, "DFCT_CODE").GetString();

            }
        }
        #endregion

        #endregion

        #region Mehod

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_START_PROD_FV";

                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
                newRow["CTNR_ID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("CTNR_ID").GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    GetProductCart();
                    Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetProductCart(string StartCartID = null)
        {
            try
            {
                string selectedLot = string.Empty;

                if (string.IsNullOrWhiteSpace(StartCartID))
                {
                    if (CommonVerify.HasDataGridRow(dgProductCart))
                    {
                        int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(dgProductCart, "CHK");
                        if (rowIdx >= 0)
                        {
                            selectedLot = DataTableConverter.GetValue(dgProductCart.Rows[rowIdx].DataItem, "CTNR_ID").GetString();
                        }
                    }
                }
                else
                {
                    // 작업 시작시 대차 ID
                    selectedLot = StartCartID;
                }

                ClearControls();
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_TB_SFC_CTNR_FINALEXTERNAL_PC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                dr["CTNR_STAT_CODE"] = "WORKING";
                dr["PROCID"] = Util.NVC(ComboProcess.SelectedValue);
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.GridSetData(dgProductCart, result, FrameOperation, true);

                        if (string.IsNullOrEmpty(selectedLot))
                        {
                            if (result.Rows.Count > 0)
                            {
                                int rowIndex = 0;
                                DataTableConverter.SetValue(dgProductCart.Rows[rowIndex].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgProductCart.SelectedIndex = rowIndex;
                                GetProductLot(true);
                            }
                        }
                        else
                        {
                            int idx = _util.GetDataGridRowIndex(dgProductCart, "CTNR_ID", selectedLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductCart.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                dgProductCart.SelectedIndex = idx;
                                GetProductLot(true);

                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                dgProductCart.CurrentCell = dgProductCart.GetCell(idx, dgProductCart.Columns.Count - 1);
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


        public void GetProductLot(bool IsCart = false, string SelectValue = "ALL")
        {
            try
            {
                int rowIdxCart = _util.GetDataGridFirstRowIndexWithTopRow(dgProductCart, "CHK");

                if (rowIdxCart < 0) return;

                string selectedLot = string.Empty;
                if (CommonVerify.HasDataGridRow(DgProductLot))
                {
                    if (IsCart == false)
                    {
                        int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                        if (rowIdx >= 0)
                        {
                            selectedLot = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                        }
                    }
                }

                //ClearDetailControls();
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_CART_PC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CTNR_ID"] = DataTableConverter.GetValue(dgProductCart.Rows[rowIdxCart].DataItem, "CTNR_ID").GetString();
                dr["PROCID"] = Util.NVC(ComboProcess.SelectedValue);
                inTable.Rows.Add(dr);

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

                        if (string.IsNullOrEmpty(selectedLot))
                        {
                            if (result.Rows.Count > 0)
                            {
                                int rowIndex = 0;
                                DataTableConverter.SetValue(DgProductLot.Rows[rowIndex].DataItem, "CHK", true);
                                //row 색 바꾸기
                                DgProductLot.SelectedIndex = rowIndex;
                                ProdListClickedProcess(rowIndex, SelectValue, true);
                            }
                        }
                        else
                        {
                            int idx = _util.GetDataGridRowIndex(DgProductLot, "LOTID", selectedLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(DgProductLot.Rows[idx].DataItem, "CHK", true);
                                //row 색 바꾸기
                                DgProductLot.SelectedIndex = idx;
                                ProdListClickedProcess(idx, SelectValue, true);

                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                DgProductLot.CurrentCell = DgProductLot.GetCell(idx, DgProductLot.Columns.Count - 1);
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

        public void ProdListClickedProcess(int rowIndex, string SelectValue = "ALL", bool FormCall = false)
        {
            try
            {
                if (rowIndex < 0 || !_util.GetDataGridCheckValue(DgProductLot, "CHK", rowIndex)) return;

                string Select = SelectValue ?? "ALL";

                if (!FormCall)
                    _defectTabButtonClick = false;

                switch (Select)
                {
                    case "ALL":
                        ClearDetailControls();
                        GetOutInboxList();                 // 완성 Inbox 조회
                        GetOutInboxGradeList();            // 등급별 Inbox 조회
                        GetDefectList();                   // 불량 정보 조회
                        break;
                    case "OUT":
                        ClearOutPalletControls();
                        GetOutInboxList();
                        GetOutInboxGradeList();
                        break;
                    case "DEF":
                        ClearDefectControls();
                        GetDefectList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutInboxList()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_INBOX_FINALEXTERNAL_PC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgProductionInbox, result, FrameOperation, true);
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
                Util.MessageException(ex);
            }
        }

        private void GetDefectList()
        {
            try
            {
                ShowLoadingIndicator();

                // 해더 정보 조회
                GetDefectHeader();

                const string bizRuleName = "DA_QCA_SEL_WIPRESONCOLLECT_INFO_PC";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 불량 저장시 비교용
                        _defectList = result;

                        Util.GridSetData(dgDefect, result, null, true);

                        if (CommonVerify.HasTableRow(result))
                        {
                            //Util.DataGridCheckAllChecked(dgDefect);
                            //C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                            //StackPanel allPanel = allColumn?.Header as StackPanel;
                            //CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                            //if (allCheck != null && allCheck.IsChecked == false) allCheck.IsChecked = true;

                            // 저장,조회시 현상태 유지
                            if (!_defectTabButtonClick)
                                SetDefectGridSelect(-1, -1, true);
                        }
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
                Util.MessageException(ex);
            }
        }

        private void GetDefectHeader()
        {
            try
            {
                _defectGradeCount = 0;
                _IsDefectSave = true;

                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_FORM_GRADE_TYPE_CODE_HEADER";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(result))
                        {
                            _defectGradeCount = result.Rows.Count;

                            for (int row = 0; row < result.Rows.Count; row++)
                            {
                                string ColumnName = "GRADE" + (row + 1).ToString();
                                dgDefect.Columns[ColumnName].Header = Util.NVC(result.Rows[row]["GRD_CODE"]);
                                dgDefect.Columns[ColumnName].Visibility = Visibility.Visible;
                            }

                            // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                            SetDefectGridGradeColumn();
                        }
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
                Util.MessageException(ex);
            }
        }

        private void SaveDefect()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_DEFECT_MCP";

                // 변경값 비교
                DataTable defect = DataTableConverter.Convert(dgDefect.ItemsSource);
                DataTable defectSave = new DataTable();
                defectSave.Columns.Add("NEW_ROW", typeof(int));
                defectSave.Columns.Add("NEW_COL", typeof(int));

                int ColStart = defect.Columns.IndexOf("GRADE1");
                int ColEnd = defect.Columns.IndexOf("GRADE1") + _defectGradeCount;

                for (int row = 0; row < defect.Rows.Count; row++)
                {
                    for (int col = ColStart; col < ColEnd; col++)
                    {
                        if (Util.NVC(defect.Rows[row][col]) != Util.NVC(_defectList.Rows[row][col]))
                        {
                            DataRow newrow = defectSave.NewRow();
                            newrow["NEW_ROW"] = row;
                            newrow["NEW_COL"] = col;
                            defectSave.Rows.Add(newrow);
                        }
                    }
                }

                // 변경자료가 없으면 Return
                if (defectSave.Rows.Count == 0)
                {
                    HiddenLoadingIndicator();

                    // 변경내용이 없습니다.
                    Util.MessageInfo("SFU1226");
                    return;
                }

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                // 불량 코드별 불량수량
                DataTable inDefectTable = ds.Tables.Add("INRESN");
                inDefectTable.Columns.Add("LOTID", typeof(string));
                inDefectTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectTable.Columns.Add("ACTID", typeof(string));
                inDefectTable.Columns.Add("RESNCODE", typeof(string));
                inDefectTable.Columns.Add("RESNQTY", typeof(double));
                inDefectTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDefectTable.Columns.Add("PROCID_CAUSE", typeof(string));
                inDefectTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectTable.Columns.Add("DFCT_TAG_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_PTN_QTY", typeof(int));
                inDefectTable.Columns.Add("COST_CNTR_ID", typeof(string));
                inDefectTable.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                inDefectTable.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                // 불량 코드, 등급별 수량
                DataTable inDefectGrdTable = ds.Tables.Add("INGRD");
                inDefectGrdTable.Columns.Add("LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectGrdTable.Columns.Add("ACTID", typeof(string));
                inDefectGrdTable.Columns.Add("RESNCODE", typeof(string));
                inDefectGrdTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inDefectGrdTable.Columns.Add("RESNQTY", typeof(double));
                inDefectGrdTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectGrdTable.Columns.Add("DFCT_GR_LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("LABEL_PRT_FLAG", typeof(string));

                foreach (DataRow row in defectSave.Rows)
                {
                    // `불량코드별 수량
                    string lotid = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                    string wipseq = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                    string actid = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["ACTID"]);
                    string resncode = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNCODE"]);
                    string resnqty = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["RESNQTY"]);
                    string costcntrid = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["COST_CNTR_ID"]);
                    string capagrdcode = Util.NVC(dgDefect.Columns[int.Parse(row["NEW_COL"].ToString())].Header);
                    string graderesnqty = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())][int.Parse(row["NEW_COL"].ToString())]);
                    string columnheadername = Util.NVC(defect.Columns[int.Parse(row["NEW_COL"].ToString())].ColumnName);

                    DataRow[] drSelect = inDefectTable.Select("ACTID = '" + actid + "' And RESNCODE = '" + resncode + "'");
                    if (drSelect.Length == 0)
                    {
                        DataRow newRow = inDefectTable.NewRow();
                        newRow["LOTID"] = lotid;
                        newRow["WIPSEQ"] = wipseq;
                        newRow["ACTID"] = actid;
                        newRow["RESNCODE"] = resncode;
                        newRow["RESNQTY"] = resnqty.Equals("") ? 0 : double.Parse(resnqty);
                        newRow["RESNCODE_CAUSE"] = string.Empty;
                        newRow["PROCID_CAUSE"] = string.Empty;
                        newRow["RESNNOTE"] = string.Empty;
                        newRow["DFCT_TAG_QTY"] = 0;
                        newRow["LANE_QTY"] = 1;
                        newRow["LANE_PTN_QTY"] = 1;

                        if (actid.Equals("CHARGE_PROD_LOT"))
                        {
                            newRow["COST_CNTR_ID"] = costcntrid;
                        }
                        else
                        {
                            newRow["COST_CNTR_ID"] = string.Empty;
                        }

                        newRow["A_TYPE_DFCT_QTY"] = 0;
                        newRow["C_TYPE_DFCT_QTY"] = 0;
                        inDefectTable.Rows.Add(newRow);
                    }

                    // 불량 등급별 수량
                    DataRow newRowGrd = inDefectGrdTable.NewRow();
                    newRowGrd["LOTID"] = lotid;
                    newRowGrd["WIPSEQ"] = wipseq;
                    newRowGrd["ACTID"] = actid;
                    newRowGrd["RESNCODE"] = resncode;
                    newRowGrd["CAPA_GRD_CODE"] = capagrdcode;
                    newRowGrd["RESNQTY"] = graderesnqty.Equals("") ? 0 : double.Parse(graderesnqty);
                    newRowGrd["RESNNOTE"] = string.Empty;
                    newRowGrd["DFCT_GR_LOTID"] = string.Empty;
                    newRowGrd["LABEL_PRT_FLAG"] = Util.NVC(defect.Rows[int.Parse(row["NEW_ROW"].ToString())]["PRINT_YN" + columnheadername.Substring(5, columnheadername.Length - 5)]);
                    inDefectGrdTable.Rows.Add(newRowGrd);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN,INGRD", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveDefectPrint()
        {
            try
            {
                _IsDefectSave = true;

                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_DEFECT_MCP";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                // 불량 코드별 불량수량
                DataTable inDefectTable = ds.Tables.Add("INRESN");
                inDefectTable.Columns.Add("LOTID", typeof(string));
                inDefectTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectTable.Columns.Add("ACTID", typeof(string));
                inDefectTable.Columns.Add("RESNCODE", typeof(string));
                inDefectTable.Columns.Add("RESNQTY", typeof(double));
                inDefectTable.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDefectTable.Columns.Add("PROCID_CAUSE", typeof(string));
                inDefectTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectTable.Columns.Add("DFCT_TAG_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_QTY", typeof(int));
                inDefectTable.Columns.Add("LANE_PTN_QTY", typeof(int));
                inDefectTable.Columns.Add("COST_CNTR_ID", typeof(string));
                inDefectTable.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                inDefectTable.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                // 불량 코드, 등급별 수량
                DataTable inDefectGrdTable = ds.Tables.Add("INGRD");
                inDefectGrdTable.Columns.Add("LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("WIPSEQ", typeof(string));
                inDefectGrdTable.Columns.Add("ACTID", typeof(string));
                inDefectGrdTable.Columns.Add("RESNCODE", typeof(string));
                inDefectGrdTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inDefectGrdTable.Columns.Add("RESNQTY", typeof(double));
                inDefectGrdTable.Columns.Add("RESNNOTE", typeof(string));
                inDefectGrdTable.Columns.Add("DFCT_GR_LOTID", typeof(string));
                inDefectGrdTable.Columns.Add("LABEL_PRT_FLAG", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridCell cell in dgDefect.Selection.SelectedCells)
                {
                    if (cell.Row.Index >= dgDefect.Rows.Count + dgDefect.FrozenBottomRowsCount)
                        continue;

                    if (cell.Column.Index < dgDefect.Columns["GRADE1"].Index)
                        continue;

                    if (cell.Column.Index > dgDefect.Columns["GRADE1"].Index + _defectGradeCount)
                        continue;

                    DataRow newRow = inDefectGrdTable.NewRow();
                    newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                    newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                    newRow["CAPA_GRD_CODE"] = cell.Column.Header.ToString();
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, dgDefect.Columns[cell.Column.Index].Name.ToString())).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, dgDefect.Columns[cell.Column.Index].Name.ToString())));
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_GR_LOTID"] = string.Empty;
                    newRow["LABEL_PRT_FLAG"] = "Y";
                    inDefectGrdTable.Rows.Add(newRow);

                    DataRow[] drSelect = inDefectTable.Select("ACTID ='" + Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "ACTID")) + "' And " +
                                                              "RESNCODE ='" + Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNCODE")) + "'");

                    if (drSelect.Length == 0)
                    {
                        newRow = inDefectTable.NewRow();
                        newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                        newRow["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                        newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "ACTID"));
                        newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                        newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNQTY")));
                        newRow["RESNCODE_CAUSE"] = string.Empty;
                        newRow["PROCID_CAUSE"] = string.Empty;
                        newRow["RESNNOTE"] = string.Empty;
                        newRow["DFCT_TAG_QTY"] = 0;
                        newRow["LANE_QTY"] = 1;
                        newRow["LANE_PTN_QTY"] = 1;

                        if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                        {
                            newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "COST_CNTR_ID"));
                        }
                        else
                        {
                            newRow["COST_CNTR_ID"] = string.Empty;
                        }

                        newRow["A_TYPE_DFCT_QTY"] = 0;
                        newRow["C_TYPE_DFCT_QTY"] = 0;
                        inDefectTable.Rows.Add(newRow);

                    }

                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN,INGRD", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            _IsDefectSave = false;
                            return;
                        }

                        //Util.MessageInfo("SFU1275");
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        _IsDefectSave = false;
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                _IsDefectSave = false;
            }
        }

        /// <summary>
        /// Inbox 수정
        /// </summary>
        private void ModifyInbox()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LOTID", typeof(string));
                inBox.Columns.Add("ACTQTY", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["VISL_INSP_USERID"] = ComboInspector.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inBox.NewRow();
                    newRow["LOTID"] = drDel["INBOX_ID"].ToString();
                    newRow["ACTQTY"] = Util.NVC_Int(drDel["CELL_QTY"]);
                    inBox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_INBOX", "INDATA,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        GetProductLot(false, "OUT");
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

        /// <summary>
        /// Inbox 삭제
        /// </summary>
        private void DeleteInbox()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LOTID", typeof(string));
                inBox.Columns.Add("ACTQTY", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["VISL_INSP_USERID"] = ComboInspector.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drDel in dr)
                {
                    newRow = inBox.NewRow();
                    newRow["LOTID"] = drDel["INBOX_ID"].ToString();
                    newRow["ACTQTY"] = 0;
                    inBox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_INBOX", "INDATA,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        GetProductLot(false, "OUT");
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

        /// <summary>
        /// Inbox 삭제취소
        /// </summary>
        private void CancelDeleteInbox()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("LOTID", typeof(string));
                inBox.Columns.Add("ACTQTY", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

                foreach (DataRow drCancel in dr)
                {
                    newRow = inBox.NewRow();
                    newRow["LOTID"] = drCancel["INBOX_ID"].ToString();
                    newRow["ACTQTY"] = Util.NVC_Int(drCancel["BEFORE_QTY"]);
                    inBox.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_TERMINATE_FV", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        GetProductLot(false, "OUT");
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

        #region 등급별 수량
        private void GetOutInboxGradeList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_GRADE_FINALEXTERNAL_PC", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProductionGrade, dtResult, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        /// <summary>
        /// 대차 출력 자료
        /// </summary>
        private DataTable PrintCartData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["CART_ID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("CTNR_ID").GetString();
                newRow["PROCID"] = Util.NVC(ComboProcess.SelectedValue);
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = ComboEquipment.SelectedValue;
            parameters[2] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("CTNR_ID").GetString();
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);
            grdMain.Children.Add(popupCartPrint);
            popupCartPrint.BringToFront();
        }

        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(popup);
        }

        private void PrintLabel(string IsQultType, C1DataGrid dg)
        {
            if (IsQultType.Equals("N"))
            {
                // 선택 추가
                SetDefectGridCheckSelect();
                if (!ValidationDefectPrint(dg, false)) return;

                // 선택 불량저장
                SaveDefectPrint();
                if (_IsDefectSave.Equals(false)) return;
            }

            DataRow[] drProdLot = DataTableConverter.Convert(DgProductLot.ItemsSource).Select("CHK = 1");

            string processName = ProcessName;
            string modelId = drProdLot[0]["MODLID"].GetString();
            string projectName = drProdLot[0]["PRJT_NAME"].GetString();
            string marketTypeName = drProdLot[0]["MKT_TYPE_NAME"].GetString();
            string assyLotId = drProdLot[0]["LOTID_RT"].GetString();
            string calDate = drProdLot[0]["CALDATE"].GetString();
            string shiftName = Util.NVC(UcPolymerFormShift.TextShift.Text);
            string equipmentShortName = drProdLot[0]["EQPTSHORTNAME"].GetString();

            string inspectorId = string.Empty;
            string[] inspectorItems = ComboInspector.Text.Split(' ');
            if (inspectorItems.Length > 0)
            {
                inspectorId = inspectorItems[0].Replace("(", "").Replace(")", "");
            }

            // 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            // 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            if (IsQultType.Equals("G"))
            {
                foreach (DataGridRow row in dg.Rows)
                {
                    if (row.Type == DataGridRowType.Item &&
                        (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
                         DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                    {
                        DataRow dr = dtLabelItem.NewRow();

                        dr["LABEL_CODE"] = "LBL0106";
                        dr["ITEM001"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CAPA_GRD_CODE"));
                        dr["ITEM002"] = modelId + "(" + projectName + ") ";
                        //dr["ITEM003"] = assyLotId;
                        dr["ITEM003"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "ASSY_LOT"));
                        dr["ITEM004"] = Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "CELL_QTY")).GetString();
                        dr["ITEM005"] = equipmentShortName;
                        //dr["ITEM006"] = calDate + "(" + shiftName + ")";
                        dr["ITEM006"] = DataTableConverter.GetValue(row.DataItem, "CALDATE").GetString() + "(" + DataTableConverter.GetValue(row.DataItem, "SHFT_NAME").GetString() + ")";
                        //dr["ITEM007"] = inspectorId;
                        dr["ITEM007"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INSPECTORID"));
                        dr["ITEM008"] = marketTypeName;
                        dr["ITEM009"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));
                        dr["ITEM010"] = null;
                        dr["ITEM011"] = null;

                        // 라벨 발행 이력 저장
                        DataRow newRow = inTable.NewRow();
                        newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                        newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "INBOX_ID"));    // Cell ID
                        newRow["PRT_ITEM02"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WIPSEQ"));
                        newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRINT_YN"));                                                // 재발행 여부
                        newRow["INSUSER"] = LoginInfo.USERID;
                        newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                        inTable.Rows.Add(newRow);

                        dtLabelItem.Rows.Add(dr);
                    }
                }
            }
            else
            {
                foreach (C1.WPF.DataGrid.DataGridCell cell in dgDefect.Selection.SelectedCells)
                {
                    if (cell.Row.Index >= dgDefect.Rows.Count + dgDefect.FrozenBottomRowsCount)
                        continue;

                    if (cell.Column.Index < dgDefect.Columns["GRADE1"].Index)
                        continue;

                    if (cell.Column.Index > dgDefect.Columns["GRADE1"].Index + _defectGradeCount)
                        continue;

                    DataRow dr = dtLabelItem.NewRow();
                    dr["LABEL_CODE"] = "LBL0107";
                    dr["ITEM001"] = modelId + "(" + projectName + ") ";
                    dr["ITEM002"] = assyLotId;
                    dr["ITEM003"] = marketTypeName;
                    dr["ITEM004"] = cell.Value.ToString().Equals("0") ? string.Empty : cell.Value.ToString();
                    dr["ITEM005"] = equipmentShortName;
                    dr["ITEM006"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "DFCT_CODE_DETL_NAME"));
                    dr["ITEM007"] = calDate + "(" + shiftName + ")";
                    dr["ITEM008"] = inspectorId;
                    dr["ITEM009"] = cell.Column.Header.ToString();
                    //dr["ITEM010"] = cell.Value.ToString();
                    dr["ITEM010"] = cell.Column.Header.ToString();     // 등급
                    dr["ITEM011"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString() + "+" + 
                                    Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNGR_ABBR_CODE")) + "+" +
                                    cell.Column.Header.ToString();
                    dtLabelItem.Rows.Add(dr);

                    // 라벨 발행 이력 저장
                    DataRow newRow = inTable.NewRow();
                    newRow["LABEL_PRT_COUNT"] = 1;                                                             // 발행 수량
                    newRow["PRT_ITEM01"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNGRID")); 
                    newRow["PRT_ITEM03"] = cell.Column.Header.ToString();
                    newRow["PRT_ITEM04"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[cell.Row.Index].DataItem, "RESNCODE"));
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                    inTable.Rows.Add(newRow);
                }
            }


            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {
                // 라벨 발행이력 저장
                string BizRuleName = string.Empty;
                if (IsQultType.Equals("G"))
                {
                    BizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST";
                }
                else
                {
                    BizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST_DEFECT";
                }

                new ClientProxy().ExecuteService(BizRuleName, "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (IsQultType.Equals("G"))
                            ProdListClickedProcess(_util.GetDataGridFirstRowIndexByCheck(DgProductLot, "CHK"), "OUT", true);
                        else
                            ProdListClickedProcess(_util.GetDataGridFirstRowIndexByCheck(DgProductLot, "CHK"), "DEF", true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });

                C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
                StackPanel allPanel = allColumn?.Header as StackPanel;
                CheckBox allCheck = allPanel?.Children[0] as CheckBox;
                if (allCheck != null && allCheck.IsChecked == true) allCheck.IsChecked = false;
            }
        }

        public void ClearDetailControls()
        {
            try
            {
                ClearOutPalletControls();
                ClearDefectControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool ClearControls()
        {
            try
            {
                Util.gridClear(dgProductCart);
                Util.gridClear(DgProductLot);
                ClearOutPalletControls();
                ClearDefectControls();

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

        private void ClearOutPalletControls()
        {
            Util.gridClear(dgProductionInbox);
            Util.gridClear(dgProductionGrade);
            txtInBoxId.Text = string.Empty;
        }

        private void ClearDefectControls()
        {
            Util.gridClear(dgDefect);
            SetDataGridCheckHeaderInitialize(dgDefect);
        }

        private void GetEquipmentWorkInfo()
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
                newRow["PROCID"] = Util.NVC(ComboProcess.SelectedValue);

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

        private bool ValidationSearch()
        {
            if (ComboProcess.SelectedIndex < 0 || ComboProcess.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return false;
            }

            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // "설비를 선택 하세요."
                Util.MessageValidation("SFU1673");
                return false;
            }

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
            if (ComboProcess.SelectedIndex < 0 || ComboProcess.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return false;
            }

            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 라인을 선택 하세요.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcPolymerFormShift.TextShift.Text)))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcPolymerFormShift.TextWorker.Text)))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            if (ComboInspector.SelectedValue == null || ComboInspector.SelectedValue.ToString().Equals("SELECT"))
            {
                //검사자를 입력해주세요.
                Util.MessageValidation("SFU1452");
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductCart, "CHK");

            if (rowIndex < 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(DgProductLot))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            return true;
        }

        private bool ValidationConfirm()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID").GetString()))
            {
                //실적확정 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1717");
                return false;
            }

            if (string.IsNullOrEmpty(UcPolymerFormShift.TextShift.Text))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(UcPolymerFormShift.TextWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

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

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductCart, "CHK");

            if (rowIndex < 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

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

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductCart, "CHK");

            if (rowIndex < 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        private bool ValidationDefectPrint(C1DataGrid dg, bool IsGoodTag)
        {
            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex < 0)
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcPolymerFormShift.TextShift.Text)))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(Util.NVC(UcPolymerFormShift.TextWorker.Text)))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            //if (ComboInspector.SelectedValue == null || ComboInspector.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    //검사자를 입력해주세요.
            //    Util.MessageValidation("SFU1452");
            //    return false;
            //}

            if (IsGoodTag)
            {
                // 양품 태그
                if (_util.GetDataGridCheckFirstRowIndex(dg, "CHK") < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");
                foreach (DataRow drrow in dr)
                {
                    if (Util.NVC_Int(drrow["CELL_QTY"]) != Util.NVC_Int(drrow["ORG_CELL_QTY"])
                        || Util.NVC(drrow["CAPA_GRD_CODE"]) != Util.NVC(drrow["ORG_CAPA_GRD_CODE"]))
                    {
                        // 변경된 데이터가 존재합니다.\r\n먼저 저장한 후 태그 발행하세요.
                        Util.MessageValidation("SFU4447");
                        return false;
                    }
                }
            }
            else
            {
                if (dg.Selection.SelectedCells.Count == 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (ComboInspector.SelectedValue == null || ComboInspector.SelectedValue.ToString().Equals("SELECT"))
                {
                    //검사자를 입력해주세요.
                    Util.MessageValidation("SFU1452");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationDefectSave()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                // 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return false;
            }

            if (dgDefect.ItemsSource == null || dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount == 0)
            {
                // 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return false;
            }

            //if (_util.GetDataGridCheckFirstRowIndex(dgDefect, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            return true;
        }

        private bool ValidationCartRePrint()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgProductCart, "CHK");

            if (rowIndex < 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        private bool ValidationInBoxSave()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (ComboInspector.SelectedIndex < 0 || ComboInspector.SelectedValue.GetString().Equals("SELECT"))
            {
                //검사원을 입력해 주세요.
                Util.MessageValidation("SFU4315");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 완성된 생산LOT Inbox는 수정 불가
            DataRow drStat = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            if (!Util.NVC(drStat["WIPSTAT"]).Equals("PROC"))
            {
                //완료된 생산LOT입니다. 수정 불가합니다.
                Util.MessageValidation("SFU4439");
                return false;
            }

            foreach (DataRow drChk in dr)
            {
                if (drChk["CELL_QTY"].Equals("") || drChk["CELL_QTY"].Equals("0"))
                {
                    // 수량은 0보다 큰 정수로 입력 하세요.
                    Util.MessageValidation("SFU3092");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationInBoxDelete()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (ComboInspector.SelectedIndex < 0 || ComboInspector.SelectedValue.GetString().Equals("SELECT"))
            {
                //검사원을 입력해 주세요.
                Util.MessageValidation("SFU4315");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 완성된 생산LOT Inbox는 삭제 불가
            DataRow drStat = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            if (!Util.NVC(drStat["WIPSTAT"]).Equals("PROC"))
            {
                // 완료된 생산LOT입니다. 삭제 불가합니다.
                Util.MessageValidation("SFU4440");
                return false;
            }

            return true;
        }

        private bool ValidationInBoxDeleteCancel()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //if (string.IsNullOrEmpty(Util.NVC(UcPolymerFormShift.TextShift.Text)))
            //{
            //    //작업조를 입력 해 주세요.
            //    Util.MessageValidation("SFU1845");
            //    return false;
            //}

            //if (string.IsNullOrEmpty(Util.NVC(UcPolymerFormShift.TextWorker.Text)))
            //{
            //    //작업자를 입력 해 주세요.
            //    Util.MessageValidation("SFU1842");
            //    return false;
            //}

            //if (ComboInspector.SelectedValue == null || ComboInspector.SelectedValue.ToString().Equals("SELECT"))
            //{
            //    //검사자를 입력해주세요.
            //    Util.MessageValidation("SFU1452");
            //    return false;
            //}

            DataRow[] dr = DataTableConverter.Convert(dgProductionInbox.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 완료된 생산LOT입니다. 삭제취소 불가합니다.
            DataRow drStat = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            if (!Util.NVC(drStat["WIPSTAT"]).Equals("PROC"))
            {
                // 완료된 생산LOT입니다. 삭제취소 불가합니다.
                Util.MessageValidation("SFU4441");
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

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

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

        private void SelectProcessName()
        {
            try
            {
                string procId = Util.NVC(ComboProcess.SelectedValue ?? _processCode);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = procId;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                _processCode = procId;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _processName = dtResult.Rows[0]["PROCNAME"].ToString();
                else
                    _processName = string.Empty;

                UcPolymerFormCommand.ProcessCode = _processCode;
                UcPolymerFormSearch.ProcessCode = _processCode;
                UcPolymerFormProdLot.ProcessCode = _processCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetComboBox()
        {
            SetProcessCombo(ComboProcess);
            SetEquipmentSegmentCombo(ComboEquipmentSegment);
            SetEquipmentCombo(ComboEquipment);
            //SetInspectorCombo(ComboInspector);
        }

        private static void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_PROCESS_PC";
            string[] arrColumn = { "LANGID", "AREAID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "FORM_INSP_PROCID" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);

        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            string processcode = string.Empty;
            if (!ComboProcess.SelectedValue.GetString().Equals("SELECT") && ComboProcess.SelectedIndex > 0)
                processcode = ComboProcess.SelectedValue.GetString();

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC";
            string[] arrColumn = { "LANGID", "AREAID" ,"PROCID"};
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

            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, equipmentSegmentCode, processcode, null };
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
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(ComboEquipment.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, _inspectorCode);
        }

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void SetGradeCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            mcb.Check(i);
                        }
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentSelectedValueChange()
        {
            _inspectorCode = string.Empty;

            ClearControls();
            UcPolymerFormShift.ClearShiftControl();

            if (ComboEquipment != null && ComboEquipment.SelectedIndex > -1 && ComboEquipment.SelectedValue.GetString() != "SELECT")
                this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null)));

        }

        private void GetUserControlInspectorCode()
        {
            string InspectorCode = ComboInspector.SelectedValue.GetString();

            _inspectorCode = InspectorCode;
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
                    UcPolymerFormCommand.ButtonEqptEnd
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }

        // 2018-03-06 불량탭 관련 추가
        private void SetDefectGridSelect(int CheckRow = -1, int CheckCol = -1, bool IsraedInly = false)
        {
            if (dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount == 0)
                return;

            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;

            int GradeColumn = _defectGradeCount + dgDefect.Columns["GRADE1"].Index;

            if (CheckRow.Equals(-1))
            {
                btnDefectInput.Background = new SolidColorBrush(Colors.Black);
                btnDefectPrintSelect.Background = new SolidColorBrush(Colors.Red);

                // 조회 후 전체 또는 헤더의 전체 선택시
                dgDefect.Selection.Clear();

                for (int row = 0; row < dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount; row++)
                {
                    for (int col = dgDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        if (row == 0 && IsraedInly)
                        {
                            dgDefect.Columns[col].IsReadOnly = true;
                        }

                        // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                        if (dgDefect.Columns[col].Visibility == Visibility.Collapsed)
                            continue;

                        if (!Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[row].DataItem, "PRINT_YN" + dgDefect.Columns[col].Name.Substring(5, dgDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                        {
                            C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCell(row, col);

                            if (!IsraedInly)
                            {
                                // 조회후 조회시는 제외, 전체선택 Click시만 적용
                                DataTableConverter.SetValue(dgDefect.Rows[row].DataItem, "CHK", true);
                                dgDefect.Selection.Add(cell);
                            }
                        }
                    }
                }

            }
            else
            {
                if (allCheck.IsChecked == true)
                    return;

                if (CheckCol == dgDefect.Columns["CHK"].Index)
                {
                    // Check 칼럼인 경우 출력을 제외한 Column 전체 선택
                    for (int col = dgDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                    {
                        // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                        if (dgDefect.Columns[col].Visibility == Visibility.Collapsed)
                            continue;

                        C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCell(CheckRow, col);

                        if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[CheckRow].DataItem, "PRINT_YN" + dgDefect.Columns[col].Name.Substring(5, dgDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                        {
                            cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F7C8"));
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgDefect.Rows[CheckRow].DataItem, "CHK").Equals(0))
                            {
                                dgDefect.Selection.Add(cell);

                                cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFD0DA"));
                            }
                            else
                            {
                                dgDefect.Selection.Remove(cell);

                                if (cell.Column.IsReadOnly == true)
                                {
                                    cell.Presenter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEBEB"));
                                }
                                else
                                {
                                    cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                }
                            }
                        }
                    }
                }

            }

            // header 체크
            int rowChkCount = DataTableConverter.Convert(dgDefect.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

            if (allCheck != null && allCheck.IsChecked == false && rowChkCount == dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount)
            {
                allCheck.Checked -= chkHeaderAll_Checked;
                allCheck.IsChecked = true;
                allCheck.Checked += chkHeaderAll_Checked;
            }

            dgDefect.EndEdit();
            dgDefect.EndEditRow(true);

        }

        private void SetDefectGridInput(bool breadonly)
        {
            if (dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount == 0)
            {
                return;
            }
            _defectTabButtonClick = true;

            dgDefect.Selection.Clear();

            DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);
            int GradeColumn = _defectGradeCount + dgDefect.Columns["GRADE1"].Index;

            for (int row = 0; row < dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount; row++)
            {
                dt.Rows[row]["CHK"] = 0;
            }

            dgDefect.Columns["CHK"].IsReadOnly = breadonly == true ? false : true;

            for (int col = dgDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
            {
                dgDefect.Columns[col].IsReadOnly = breadonly;
            }

            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dgDefect.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            allCheck.IsChecked = false;
            allCheck.IsEnabled = breadonly;

            Util.GridSetData(dgDefect, dt, null, true);

        }

        /// <summary>
        /// 2018-03-06 불량탭 추가
        /// </summary>
        private void SetDefectGridCheckSelect()
        {
            int GradeColumn = _defectGradeCount + dgDefect.Columns["GRADE1"].Index;

            for (int row = 0; row < dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount; row++)
            {
                for (int col = dgDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
                {
                    if (!DataTableConverter.GetValue(dgDefect.Rows[row].DataItem, "CHK").Equals(1))
                        continue;

                    // 뷸량 등록시 대상등급선택 콤보 2018-04-05
                    if (dgDefect.Columns[col].Visibility == Visibility.Collapsed)
                        continue;

                    if (!Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[row].DataItem, "PRINT_YN" + dgDefect.Columns[col].Name.Substring(5, dgDefect.Columns[col].Name.Length - 5))).Equals("Y"))
                    {
                        C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCell(row, col);
                        dgDefect.Selection.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void SetDefectGridGradeColumn()
        {
            string gradeSelect = Util.NVC(cboGradeSelect.SelectedItemsToString).Replace(",", "");

            if (string.IsNullOrWhiteSpace(gradeSelect)) return;

            dgDefect.Selection.Clear();
            int GradeColumn = _defectGradeCount + dgDefect.Columns["GRADE1"].Index;

            for (int col = dgDefect.Columns["GRADE1"].Index; col < GradeColumn; col++)
            {
                if (gradeSelect.IndexOf(Util.NVC(dgDefect.Columns[col].Header)) < 0)
                {
                    dgDefect.Columns[col].Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgDefect.Columns[col].Visibility = Visibility.Visible; ;
                }
            }
        }



        #endregion






    }
}
