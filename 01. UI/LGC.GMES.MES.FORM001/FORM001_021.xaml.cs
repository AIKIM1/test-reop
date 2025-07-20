/*************************************************************************************
 Created Date : 2017.12.04
      Creator : 신 광희
   Decription : Degas(Tray) 공정진척
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
using System.Globalization;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Documents;
using System.Linq;
using ColorConverter = System.Windows.Media.ColorConverter;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid.Summaries;


namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_021 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _processCode;
        private string _processName;
        private string _divisionCode;
        private bool _isLoaded;
        private string _caldate = string.Empty;
        private DateTime _dtCaldate;
        private const string LabelCode = "LBL0107"; //LBL0106 : 양품 태그 라벨,  LBL0107 : 불량 태그 라벨
        private string _inspectorCode;

        /// 2018-03-06 불량탭 추가
        private int _defectGradeCount;
        private bool _IsDefectSave;
        private DataTable _defectList;
        private bool _defectTabButtonClick;

        private CheckBoxHeaderType _inBoxHeaderType;

        private UcPolymerFormCommand UcPolymerFormCommand { get; set; }
        private UcPolymerFormSearch UcPolymerFormSearch { get; set; }
        private UcPolymerFormProdLot UcPolymerFormProdLot { get; set; }
        private UcPolymerFormShift UcPolymerFormShift { get; set; }

        private C1DataGrid DgProductLot { get; set; }

        private C1ComboBox ComboEquipmentSegment { get; set; }
        private C1ComboBox ComboEquipment { get; set; }
        private C1ComboBox ComboInspector { get; set; }

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
        public FORM001_021()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            InitializeUserControls();
            InitializeUserControlsGrid();
            SetComboBox();
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
                UcPolymerFormCommand.DivisionCode = _divisionCode;
                UcPolymerFormCommand.FrameOperation = FrameOperation;
                UcPolymerFormCommand.SetButtonVisibility();
            }

            if (UcPolymerFormProdLot != null)
            {
                UcPolymerFormProdLot.UcParentControl = this;
                UcPolymerFormProdLot.ProcessCode = _processCode;
                UcPolymerFormProdLot.SetDataGridColumnVisibility();
            }

            if (UcPolymerFormSearch != null)
            {
                UcPolymerFormSearch.ProcessCode = _processCode;
                UcPolymerFormSearch.DivisionCode =_divisionCode;
                UcPolymerFormSearch.SetControlVisibility();
                ComboEquipmentSegment = UcPolymerFormSearch.ComboEquipmentSegment;
                ComboEquipment = UcPolymerFormSearch.ComboEquipment;
                ComboInspector = UcPolymerFormSearch.ComboInspector;
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
                UcPolymerFormCommand.ButtonInspectionNew.Click += ButtonInspectionNew_Click;
                UcPolymerFormCommand.ButtonSublotDefect.Click += ButtonSublotDefect_Click;

                UcPolymerFormCommand.ButtonInspection.Click += ButtonInspection_Click;
                UcPolymerFormCommand.ButtonStart.Click += ButtonStart_Click;
                UcPolymerFormCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcPolymerFormCommand.ButtonConfirm.Click += ButtonConfirm_Click;
            }

            if (UcPolymerFormSearch != null)
            {
                UcPolymerFormSearch.ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;
                UcPolymerFormSearch.ComboEquipmentSegment.SelectedValueChanged += ComboEquipmentSegment_SelectedValueChanged;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded == false)
            {
                _processCode = Process.PolymerDegas;
                _divisionCode = "Tray";

                ApplyPermissions();
                Initialize();

                // 공정명 검색
                SelectProcessName();

                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT") && ComboEquipment.SelectedIndex > -1)
                    ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null);

                _isLoaded = true;

                _inBoxHeaderType = CheckBoxHeaderType.Zero;
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

        #region [버튼 영역]
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
                //GetProductLot();
            }
            GetEquipmentWorkInfo();
            //SetInspectorCombo(ComboInspector);
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
                //GetProductLot();
            }
            GetEquipmentWorkInfo();
            //SetInspectorCombo(ComboInspector);
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

            FORM001_RUNSTART popupRunStart = new FORM001_RUNSTART { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupRunStart.Name) == false)
                return;

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = ComboEquipment.SelectedValue;
            parameters[3] = ComboEquipment.Text;
            parameters[4] = _divisionCode;
            parameters[5] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            popupRunStart.Closed += popupRunStart_Closed;
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {

            FORM001_RUNSTART popup = sender as FORM001_RUNSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
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

            object[] parameters = new object[8];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
            parameters[5] = _divisionCode;
            parameters[6] = new TextRange(txtRemark.Document.ContentStart, txtRemark.Document.ContentEnd).Text;
            parameters[7] = Util.NVC(ComboEquipmentSegment.SelectedValue);

            C1WindowExtension.SetParameters(popupConfirm, parameters);

            popupConfirm.Closed += popupConfirm_Closed;
            grdMain.Children.Add(popupConfirm);
            popupConfirm.BringToFront();

        }

        private void popupConfirm_Closed(object sender, EventArgs e)
        {
            FORM001_CONFIRM popup = sender as FORM001_CONFIRM;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
            }
            GetEquipmentWorkInfo();
            //SetInspectorCombo(ComboInspector);
            grdMain.Children.Remove(popup);
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            GetProductLot();
            //SetInspectorCombo(ComboInspector);
            GetEquipmentWorkInfo();
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

            popupShiftUser.Closed += popupShiftUser_Closed;
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
            grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 투입 대차 팝업
        /// </summary>
        private void InputCart()
        {
            CMM_POLYMER_FORM_CART_INPUT popupCartInput = new CMM_POLYMER_FORM_CART_INPUT();
            popupCartInput.FrameOperation = this.FrameOperation;

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID").GetString();
            parameters[5] = Util.NVC(txtInputLotId.Text);

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
            CMM_POLYMER_FORM_CART_INPUT popup = sender as CMM_POLYMER_FORM_CART_INPUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            GetInputPallet();
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
            CMM_POLYMER_FORM_CART_INPUT_CANCEL popupCartInputCancel = new CMM_POLYMER_FORM_CART_INPUT_CANCEL();
            popupCartInputCancel.FrameOperation = this.FrameOperation;

            DataRow[] dr = DataTableConverter.Convert(dgInputPallet.ItemsSource).Select("CHK = 1");

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
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
            CMM_POLYMER_FORM_CART_INPUT_CANCEL popup = sender as CMM_POLYMER_FORM_CART_INPUT_CANCEL;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            GetInputPallet();
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
        #endregion


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

        private void TbDefectDetail_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// 뷸량 등록시 대상등급선택 콤보 2018-04-05
        /// </summary>
        private void cboGradeSelect_SelectionChanged(object sender, EventArgs e)
        {
            SetDefectGridGradeColumn();
        }

        private void dgDefectDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Index == dataGrid.Columns["GOODQTY"].Index)
                    {
                        //var convertFromString = ColorConverter.ConvertFromString("#F8DAC0");
                        var convertFromString = ColorConverter.ConvertFromString("Yellow");
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

        private void dgDefectDetail_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgDefectDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e?.Cell?.Column != null)
            {
                if (e.Cell.Column.Name.Equals("GOODQTY"))
                {
                    double goodqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "GOODQTY").GetDouble();
                    double defectqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_DEFECT").GetDouble();
                    double lossqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_LOSS").GetDouble();
                    double chargeprdqty = DataTableConverter.GetValue(e.Cell.Row.DataItem, "DTL_CHARGEPRD").GetDouble();

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "OUTQTY", goodqty + defectqty + lossqty + chargeprdqty);
                }
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                if (string.IsNullOrEmpty(_caldate)) return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                //DateTime dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                if (dtPik != null && ((Convert.ToDecimal(_dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                                      (Convert.ToDecimal(_dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))))
                {
                    dtPik.Text = _dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = _dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    //return;
                }
            }));
        }

        private void rdoPallet_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rdoInBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void txtInputLotId_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtInputLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    btnInputPallet_Click(btnInputPallet,null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInputPallet_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInputPallet()) return;

            /*
            string positionId = Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[_util.GetDataGridCheckFirstRowIndex(dgInputPallet, "CHK")].DataItem, "EQPT_MOUNT_PSTN_ID"));
            string positionName = Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[_util.GetDataGridCheckFirstRowIndex(dgInputPallet, "CHK")].DataItem, "EQPT_MOUNT_PSTN_NAME"));

            object[] parameters = new object[2];
            parameters[0] = positionName;
            parameters[1] = txtInputLotId.Text.Trim();

            Util.MessageConfirm("SFU1291", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    InputPallet(txtInputLotId.Text, positionId);
                    txtInputLotId.Text = string.Empty;
                }
            }, parameters);
            */

            //Util.MessageConfirm("SFU1248", result =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        InputPallet();
            //        txtInputLotId.Text = string.Empty;
            //    }
            //});

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

        private void btnInputPalletCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationInputPalletCancel()) return;

            //Util.MessageConfirm("SFU4299", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        InputPalletCancel();
            //    }
            //});

            InputCancelCart();
        }

        private void btnInputPalletRemain_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletRemain()) return;

            CMM_POLYMER_FORM_CART_INPUT_REMAIN popuInboxRemain = new CMM_POLYMER_FORM_CART_INPUT_REMAIN { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popuInboxRemain.Name) == false)
                return;

            popuInboxRemain.ShiftID = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            popuInboxRemain.ShiftName = Util.NVC(UcPolymerFormShift.TextShift.Text);
            popuInboxRemain.WorkerID = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            popuInboxRemain.WorkerName = Util.NVC(UcPolymerFormShift.TextWorker.Text);
            popuInboxRemain.ShiftDateTime = Util.NVC(UcPolymerFormShift.TextShiftDateTime.Text);
            popuInboxRemain.EquipmentSegmentCode = Util.NVC(ComboEquipmentSegment.SelectedValue);

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            popuInboxRemain.DifferenceQty = (int) DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "DIFF_QTY").GetInt();

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetDataGridFirstRowBycheck(dgInputPallet, "CHK");
            parameters[5] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            C1WindowExtension.SetParameters(popuInboxRemain, parameters);

            popuInboxRemain.Closed += popuInboxRemain_Closed;
            grdMain.Children.Add(popuInboxRemain);
            popuInboxRemain.BringToFront();
        }

        private void popuInboxRemain_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_INPUT_REMAIN popup = sender as CMM_POLYMER_FORM_CART_INPUT_REMAIN;
            if (popup != null && popup.ConfirmSave)
            {
                GetProductLot();
                GetInputPallet();
            }
            GetEquipmentWorkInfo();
            //SetInspectorCombo(ComboInspector);
            grdMain.Children.Remove(popup);
            txtInputLotId.Focus();
        }

        private void btnSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            dgDefectDetail.EndEditRow(true);
            dgDefectDetail.EndEdit();

            if (!ValidationSaveWipHistory()) return;
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveWipHistory();
                }
            });
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

        /// <summary>
        /// 2018-03-06 불량탭 관련 수정
        /// </summary>
        private void btnDefectPrint_Click(object sender, RoutedEventArgs e)
        {
            // 선택 추가
            SetDefectGridCheckSelect();
            if (!ValidationDefectPrint()) return;

            // 선택 불량저장
            SaveDefectPrint();
            if (_IsDefectSave.Equals(false)) return;

            //string processName = _processName;
            string modelId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("MODLID").GetString();
            string projectName = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("PRJT_NAME").GetString();
            string marketTypeName = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("MKT_TYPE_NAME").GetString();
            string assyLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("ASSY_LOTID").GetString();
            string calDate = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("CALDATE").GetString();
            string shiftName = UcPolymerFormShift.TextShift.Text;
            string equipmentShortName = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("EQPTSHORTNAME").GetString();
            string inspectorId = string.Empty;
            string[] inspectorItems = ComboInspector.Text.Split(' ');
            if (inspectorItems.Length > 0)
            {
                inspectorId = inspectorItems[0].Replace("(", "").Replace(")", "");
            }

            if (string.IsNullOrEmpty(calDate))
            {
                calDate = dtpCaldate.SelectedDateTime.Year.GetString() + "." +
                          dtpCaldate.SelectedDateTime.Month.GetString() + "." +
                          dtpCaldate.SelectedDateTime.Day.GetString();
            }

            // 불량 태그(라벨) 항목
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //PKG LOT
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //MARKET
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //DDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     
            dtLabelItem.Columns.Add("ITEM011", typeof(string));

            // 2018-03-06 불량탭 관련 수정
            //foreach (DataGridRow row in dgDefect.Rows)
            //{
            //    if (row.Type == DataGridRowType.Item &&
            //        (DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True" ||
            //         DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
            //    {
            //        DataRow dr = dtLabelItem.NewRow();
            //        dr["LABEL_CODE"] = LabelCode;
            //        dr["ITEM001"] = modelId + "(" + projectName + ") ";
            //        dr["ITEM002"] = assyLotId;
            //        dr["ITEM003"] = marketTypeName;
            //        dr["ITEM004"] = string.Empty;
            //        dr["ITEM005"] = equipmentShortName;
            //        dr["ITEM006"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "DFCT_CODE_DETL_NAME"));
            //        dr["ITEM007"] = calDate + "(" + shiftName + ")";
            //        dr["ITEM008"] = inspectorId;
            //        dr["ITEM009"] = string.Empty;
            //        dr["ITEM010"] = string.Empty;
            //        dr["ITEM011"] = string.Empty;
            //        dtLabelItem.Rows.Add(dr);
            //    }
            //}

            // 양품 Tag인 경우 라벨이력 저장
            DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

            foreach (C1.WPF.DataGrid.DataGridCell cell in dgDefect.Selection.SelectedCells)
            {
                if (cell.Row.Index >= dgDefect.Rows.Count + dgDefect.FrozenBottomRowsCount)
                    continue;

                if (cell.Column.Index < dgDefect.Columns["GRADE1"].Index)
                    continue;

                if (cell.Column.Index > dgDefect.Columns["GRADE1"].Index + _defectGradeCount)
                    continue;

                DataRow dr = dtLabelItem.NewRow();
                dr["LABEL_CODE"] = LabelCode;
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
                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST_DEFECT", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // 불량/물품청구 데이터 조회
                        GetDefectList();
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
                        //e.Cell.Presenter.Background = null;
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
                if (e.Cell.Column.Name.IndexOf("GRADE") >-1)
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

        private void dgInputPallet_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            //try
            //{
            //    C1DataGrid dg = sender as C1DataGrid;
            //    if (dg != null)
            //    {
            //        DataRowView drv = dg.CurrentRow.DataItem as DataRowView;

            //        if (drv == null) return;

            //        if (e.Cell.Column.Name == "CHK")
            //        {
            //            int rowIndex = 0;
            //            foreach (var item in dg.Rows)
            //            {
            //                if (drv["CHK"].GetString() == "True" || drv["CHK"].GetString() == "1")
            //                {
            //                    DataTableConverter.SetValue(item.DataItem, "CHK", e.Cell.Row.Index == rowIndex);
            //                }
            //                else
            //                {
            //                    DataTableConverter.SetValue(item.DataItem, "CHK", false);
            //                }
            //                rowIndex++;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgInputPallet;
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

        private void btnInTraySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonVerify.HasDataGridRow(DgProductLot)) return;
            GetReferenceInOutTray("IN");
        }

        private void btnOutTraySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonVerify.HasDataGridRow(DgProductLot)) return;
            GetReferenceInOutTray("OUT");
        }
        #endregion

        #region Mehod

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

                    GetProductLot();
                    Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetProductLot()
        {
            try
            {
                string selectedLot = string.Empty;
                if (CommonVerify.HasDataGridRow(DgProductLot))
                {
                    int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                    if (rowIdx >= 0)
                    {
                        selectedLot = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                    }
                }

                ClearDetailControls();
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_PC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("DIVISION", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = ComboEquipmentSegment.SelectedValue;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["PROCID"] = _processCode;
                dr["DIVISION"] = "T";
                inTable.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
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
                        //DgProductLot.ItemsSource = DataTableConverter.Convert(result);
                        Util.GridSetData(DgProductLot, result, FrameOperation,true);

                        if (string.IsNullOrEmpty(selectedLot))
                        {
                            if (result.Rows.Count > 0)
                            {
                                int rowIndex = _util.GetDataGridRowIndex(DgProductLot, "WIPSTAT", "PROC");
                                if (rowIndex < 0)
                                {
                                    rowIndex = 0;
                                    if (DgProductLot.TopRows.Count > 0)
                                        rowIndex = DgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(DgProductLot.Rows[rowIndex].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    DgProductLot.SelectedIndex = rowIndex;
                                    ProdListClickedProcess(rowIndex,true);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(DgProductLot.Rows[rowIndex].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    DgProductLot.SelectedIndex = rowIndex;
                                    ProdListClickedProcess(rowIndex, true);
                                }
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
                                ProdListClickedProcess(idx, true);

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

        public void ClearDetailControls()
        {
            try
            {
                ClearDefectDetailControls();
                ClearInputPalletControls();
                ClearDefectControls();
                ClearInTrayControls();
                ClearOutTrayControls();
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
                Util.gridClear(DgProductLot);
                ClearDefectDetailControls();
                ClearInputPalletControls();
                ClearDefectControls();
                ClearInTrayControls();
                ClearOutTrayControls();

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ClearDefectDetailControls()
        {
            Util.gridClear(dgDefectDetail);
            txtWorkOrder.Text = string.Empty;
            txtLotId.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtWorkMinute.Text = string.Empty;
            txtRemark.Document.Blocks.Clear();
            txtProdId.Text = string.Empty;
            _caldate = string.Empty;
        }

        private void ClearInputPalletControls()
        {
            Util.gridClear(dgInputPallet);
            txtInputLotId.Text = string.Empty;
        }

        private void ClearDefectControls()
        {
            SetDataGridCheckHeaderInitialize(dgDefect);
            Util.gridClear(dgDefect);
        }

        private void ClearInTrayControls()
        {
            Util.gridClear(dgInTray);
        }

        private void ClearOutTrayControls()
        {
            Util.gridClear(dgOutTray);
        }

        public void ProdListClickedProcess(int rowIndex, bool FormCall = false)
        {
            try
            {
                if (rowIndex < 0 || !_util.GetDataGridCheckValue(DgProductLot, "CHK", rowIndex)) return;
                //string lotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
                //string wipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

                if (!FormCall)
                    _defectTabButtonClick = false;

                // 실적상세 Control 바인딩
                SetResultDetailControl(DgProductLot.Rows[rowIndex].DataItem);

                // 생산실적 Setting
                SetDefectDetail();

                // 불량/물품청구 데이터 조회
                GetDefectList();

                // 투입 Pallet
                GetInputPallet();

                // 투입Tray 완성Tray 참고용
                GetReferenceInOutTray("IN");
                GetReferenceInOutTray("OUT");

                // 비동기 호출 등으로 인하여 불량/물품청구 데이터 조회 후 생산실적 수량 계산
                //CalculateDefectQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetResultDetailControl(object selectedItem)
        {
            DataRowView rowview = selectedItem as DataRowView;
            if (rowview == null) return;

            txtWorkOrder.Text = rowview["WOID"].GetString();
            txtLotId.Text = rowview["LOTID"].GetString();
            txtProdId.Text = rowview["PRODID"].GetString();
            txtStartTime.Text = rowview["WIPDTTM_ST"].GetString();
            string endTime = rowview["WIPDTTM_ED"].GetString();
            DateTime dTmpStart;

            if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                if (DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                {
                    txtWorkMinute.Text = Math.Truncate(GetSystemTime().Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (!string.IsNullOrEmpty(rowview["WIPDTTM_ST"].GetString()) && !string.IsNullOrEmpty(rowview["WIPDTTM_ED"].GetString()))
            {
                DateTime dTmpEnd;
                if (DateTime.TryParse(endTime, out dTmpEnd) && DateTime.TryParse(txtStartTime.Text, out dTmpStart))
                    txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(rowview["CALDATE"].GetString()))
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(rowview["CALDATE"])).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(rowview["CALDATE"]));
            }
            else
            {
                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(DateTime.Now.ToShortDateString())).ToLongDateString();
                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(DateTime.Now.ToShortDateString()));
            }
            //_caldate = Util.NVC(rowview["CALDATE"]);
            //_dtCaldate = Convert.ToDateTime(Util.NVC(rowview["CALDATE"]));

            txtRemark.AppendText(rowview["WIP_NOTE"].GetString());
        }

        private void SetDefectDetail()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("OUTQTY", typeof(int));
            inDataTable.Columns.Add("GOODQTY", typeof(int));
            inDataTable.Columns.Add("DTL_DEFECT", typeof(int));
            inDataTable.Columns.Add("DTL_LOSS", typeof(int));
            inDataTable.Columns.Add("DTL_CHARGEPRD", typeof(int));
            inDataTable.Columns.Add("DEFECTQTY", typeof(int));

            DataRow dtRow = inDataTable.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["OUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;

            inDataTable.Rows.Add(dtRow);
            dgDefectDetail.ItemsSource = DataTableConverter.Convert(inDataTable);
        }

        private void SaveWipHistory()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_SAVE_PROD";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("WIP_NOTE", typeof(string));
                inDataTable.Columns.Add("EQPT_END_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["WIP_NOTE"] = new TextRange(txtRemark.Document.ContentStart, txtRemark.Document.ContentEnd).Text;
                dr["EQPT_END_QTY"] = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);
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
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            _IsDefectSave = false;
                            return;
                        }

                        //Util.MessageInfo("SFU1275");
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
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
                            CalculateDefectQty();
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

        private void CalculateDefectQty()
        {
            // 양품수량

            if (CommonVerify.HasDataGridRow(dgDefectDetail))
            {
                //SumDefectQty();
                /*  불량  (생산수량부족:LENGTH_LACK), (생산수량초과:LENGTH_EXCEED)
                 *  길이 부족이면 생산량에 더해주고, 길이 초과면 뺴서 차이수량 조정
                 */
                DataTable dtTmp = DataTableConverter.Convert(dgDefect.ItemsSource);

                double defect = 0;
                double goodQty = 0;
                double loss = 0;
                double chargeprd = 0;
                double lengthLack = 0;
                double lengthExceed = 0;

                if (CommonVerify.HasTableRow(dtTmp))
                {
                    defect = double.Parse(dtTmp.Select("ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' AND PRCS_ITEM_CODE Not IN('LENGTH_LACK','LENGTH_EXCEED')").AsEnumerable().Sum(r => r.Field<int>("RESNQTY")).GetString());
                    loss = double.Parse(dtTmp.Select("ACTID = 'LOSS_LOT' AND PRCS_ITEM_CODE Not IN('LENGTH_LACK','LENGTH_EXCEED')").AsEnumerable().Sum(r => r.Field<int>("RESNQTY")).GetString());
                    chargeprd = double.Parse(dtTmp.Select("ACTID = 'CHARGE_PROD_LOT'").AsEnumerable().Sum(r => r.Field<int>("RESNQTY")).GetString());
                    lengthLack = double.Parse(dtTmp.Select("PRCS_ITEM_CODE = 'LENGTH_LACK'").AsEnumerable().Sum(r => r.Field<int>("RESNQTY")).GetString());
                    lengthExceed = double.Parse(dtTmp.Select("PRCS_ITEM_CODE = 'LENGTH_EXCEED'").AsEnumerable().Sum(r => r.Field<int>("RESNQTY")).GetString());
                }

                DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", defect.GetString());
                DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", loss.GetString());
                DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", chargeprd.GetString());
                DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", (defect + loss + chargeprd).GetString());

                if (CommonVerify.HasDataGridRow(DgProductLot))
                {
                    int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                    goodQty = DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "GOOD_QTY").GetDouble();
                }

                ////double defect = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                ////double loss = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                ////double chargeprd = DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                ////double defectQty = defect + loss + chargeprd;

                //양품수량
                DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                //생산수량 = 양품수량 + (불량 + 물품청구) + 길이부족 - 길이초과
                DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "OUTQTY", goodQty + defect + loss + chargeprd + lengthLack - lengthExceed);
            }
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(dgDefect.ItemsSource);

                if (CommonVerify.HasTableRow(dtTmp))
                {
                    //DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT'").GetString()));
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString()));
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").GetString()));
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString()));
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString()));
                }
                else
                {
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", 0);
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                    DataTableConverter.SetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInputPallet()
        {
            try
            {
                _inBoxHeaderType = CheckBoxHeaderType.Zero;

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_PALLET_INPUT_HISTORY_PC";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INPUT_LOT_TYPE_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["WIPSEQ"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _processCode;
                dr["INPUT_LOT_TYPE_CODE"] = "PROD";
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

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

                        //dgInputPallet.ItemsSource = DataTableConverter.Convert(result);
                        Util.GridSetData(dgInputPallet, result, FrameOperation, true);
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

        private void GetReferenceInOutTray(string trayType)
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_INOUT_TRAY_PC";
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("STARTDTTM", typeof(string));
                inTable.Columns.Add("INOUT", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["PROCID"] = _processCode;
                dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID_RT").GetString();
                dr["STARTDTTM"] = txtStartTime.Text;
                dr["INOUT"] = trayType;
                inTable.Rows.Add(dr);

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

                        if (trayType == "IN")
                            dgInTray.ItemsSource = DataTableConverter.Convert(result);
                        else
                            dgOutTray.ItemsSource = DataTableConverter.Convert(result);

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

        private void InputPallet()
        {
            try
            {
                string bizRuleName = rdoInBox.IsChecked == true ? "BR_PRD_CHK_INPUT_LOT_INBOX" : "BR_PRD_CHK_INPUT_LOT_CTNR";

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                if (!(bool)rdoInBox.IsChecked)
                {
                    inTable.Columns.Add("PROCID", typeof(string));
                }

                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["USERID"] = LoginInfo.USERID;
                if (!(bool)rdoInBox.IsChecked)
                {
                    dr["PROCID"] = _processCode;
                }
                inTable.Rows.Add(dr);

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
                DataRow newRow = inInputTable.NewRow();
                newRow["INPUT_LOTID"] = txtInputLotId.Text;
                inInputTable.Rows.Add(newRow);

                //string xmlText = indataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", "OUTDATA", indataSet);
                GetProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InputPalletCancel()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_TERMINATE_LOT";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(dr);

                DataTable inLotTable = ds.Tables.Add("INLOT");
                inLotTable.Columns.Add("INPUT_SEQNO", typeof(string));
                inLotTable.Columns.Add("LOTID", typeof(string));
                inLotTable.Columns.Add("WIPQTY", typeof(decimal));
                inLotTable.Columns.Add("WIPQTY2", typeof(decimal));

                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(dgInputPallet, "CHK");

                DataRow newRow = inLotTable.NewRow();
                newRow["INPUT_SEQNO"] = DataTableConverter.GetValue(dgInputPallet.Rows[rowIndex].DataItem, "INPUT_SEQNO").GetString();
                newRow["LOTID"] = DataTableConverter.GetValue(dgInputPallet.Rows[rowIndex].DataItem, "CELLID").GetString();
                newRow["WIPQTY"] = DataTableConverter.GetValue(dgInputPallet.Rows[rowIndex].DataItem, "INPUT_QTY").GetDecimal();
                newRow["WIPQTY2"] = DataTableConverter.GetValue(dgInputPallet.Rows[rowIndex].DataItem, "INPUT_QTY2").GetDecimal();
                inLotTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");
                        GetProductLot();

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
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

        #region [Validation 영역]
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

        private bool ValidationSaveWipHistory()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(dgDefectDetail))
            {
                Util.MessageValidation("SFU3552");
                return false;
            }
            /*
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
            */
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

            // 불량 수량, 양품수량을 0으로 만들어야만 시작 취소 가능
            if (DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() != 0 || DataTableConverter.GetValue(dgDefectDetail.Rows[dgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDecimal() != 0)
            {
                //불량수량, 양품수량이 0인 경우 시작 취소 가능합니다.
                Util.MessageValidation("SFU4352");
                return false;
            }

            /*
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
            */
            return true;
        }

        private bool ValidationConfirm()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID").GetString()))
            {
                //실적확정 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1717");
                return false;
            }
            /*
            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return false;
            }
            */
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

        private bool ValidationInputPallet()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtInputLotId.Text))
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

            //if (_util.GetDataGridFirstRowIndexByCheck(dgInputPallet, "CHK") < 0)
            //{
            //    //Util.Alert("투입 위치를 선택하세요.");
            //    Util.MessageValidation("SFU1957");
            //    return false;
            //}

            //for (int i = 0; i < dgInputPallet.GetRowCount(); i++)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[i].DataItem, "PALLETID")).Equals(txtInputLotId.Text.Trim()))
            //    {
            //        Util.MessageValidation("SFU3278", Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));    // %1 에 이미 투입되었습니다.
            //        return false;
            //    }
            //}

            return true;
        }

        private bool ValidationInputPalletCancel()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgInputPallet, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }


            /*
            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputPallet.Rows[_util.GetDataGridCheckFirstRowIndex(dgInputPallet, "CHK")].DataItem, "PALLETID"))))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return false;
            }
            */
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

            //int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            //if (rowIndex < 0)
            //{
            //     선택된 항목이 없습니다.
            //    Util.MessageValidation("SFU1645");
            //    return false;
            //}

            //rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgInputPallet, "CHK");
            //if (rowIndex < 0)
            //{
            //     선택된 항목이 없습니다.
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            int rowChkCount = DataTableConverter.Convert(dgInputPallet.ItemsSource).AsEnumerable().Count(r => r.Field<int>("CHK") == 1);

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

        private bool ValidationDefectPrint()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            //if (_util.GetDataGridCheckFirstRowIndex(dgDefect, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            if (dgDefect.Selection.SelectedCells.Count == 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
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

            if (ComboInspector.SelectedIndex < 0 || ComboInspector.SelectedValue.GetString().Equals("SELECT"))
            {
                //검사원을 입력해 주세요.
                Util.MessageValidation("SFU4315");
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == LabelCode
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
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

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }
        #endregion


        private void SetComboBox()
        {
            SetEquipmentSegmantCombo(ComboEquipmentSegment);
            SetEquipmentCombo(ComboEquipment);
        }

        private void SetEquipmentSegmantCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _processCode};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(ComboEquipmentSegment.SelectedValue), _processCode, null };
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

        private void SetEquipmentSelectedValueChange()
        {
            _inspectorCode = string.Empty;

            ClearControls();
            UcPolymerFormShift.ClearShiftControl();

            if (ComboEquipment != null && ComboEquipment.SelectedIndex > -1 && ComboEquipment.SelectedValue.GetString() != "SELECT")
                Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcPolymerFormSearch.ButtonSearch, null)));

        }

        private void GetUserControlInspectorCode()
        {
            string InspectorCode = ComboInspector.SelectedValue.GetString();

            _inspectorCode = InspectorCode;
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
