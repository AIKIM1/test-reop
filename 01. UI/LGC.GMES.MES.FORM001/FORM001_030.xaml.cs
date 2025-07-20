/*************************************************************************************
 Created Date : 2017.12.22
      Creator : 정문교
   Decription : F7500 : 양품화
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.22   정문교 : 최초 생성
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

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_030 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private string _processCode;
        private string _processName;
        private string _divisionCode;
        private string _inspectorCode;
        private string _selectCart;
        private string _selectProdLot;
        private bool _isLoaded = false;

        public UcPolymerFormCommand UcPolymerFormCommand { get; set; }
        public UcPolymerFormSearch UcPolymerFormSearch { get; set; }
        private UcPolymerFormCart UcPolymerFormCart { get; set; }
        public UcPolymerFormProductionResult UcPolymerFormProductionResult { get; set; }
        public UcPolymerFormShift UcPolymerFormShift { get; set; }
        public C1DataGrid DgProductionInbox { get; set; }
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
        public FORM001_030()
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
            UcPolymerFormShift = grdShift.Children[0] as UcPolymerFormShift;
            UcPolymerFormProductionResult = grdResult.Children[0] as UcPolymerFormProductionResult;


            if (UcPolymerFormCommand != null)
            {
                UcPolymerFormCommand.ProcessCode = _processCode;
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

            if (UcPolymerFormProductionResult != null)
            {
                UcPolymerFormProductionResult.UcParentControl = this;
                UcPolymerFormProductionResult.FrameOperation = FrameOperation;
                UcPolymerFormProductionResult.ProcessCode = _processCode;
                UcPolymerFormProductionResult.SetButtonVisibility();
                UcPolymerFormProductionResult.SetControlHeader();
                UcPolymerFormProductionResult.SetTabVisibility();
                //UcPolymerFormProductionResult.SetControlVisibility();
            }
        }

        private void InitializeUserControlsGrid()
        {
            DgProductionInbox = UcPolymerFormProductionResult.DgProductionInbox;
        }

        private void SetEventInUserControls()
        {
            if (UcPolymerFormCommand != null)
            {
                UcPolymerFormCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                UcPolymerFormCommand.ButtonTakeOver.Click += ButtonTakeOver_Click;
                UcPolymerFormCommand.ButtonCartDefect.Click += ButtonCartDefect_Click;
                UcPolymerFormCommand.ButtonSublotDefect.Click += ButtonSublotDefect_Click;

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
                _divisionCode = string.Empty;

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

        private void ButtonInboxType_Click(object sender, RoutedEventArgs e)
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return;
            }

            FORM001_EQPT_INBOX_TYPE popupInboxType = new FORM001_EQPT_INBOX_TYPE { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupInboxType.Name) == false)
                return;

            popupInboxType.EquipmentSegmentCode = Util.NVC(ComboEquipmentSegment.SelectedValue);

            object[] parameters = new object[4];
            parameters[0] = Util.NVC(ComboProcess.SelectedValue);
            parameters[1] = _processName;
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            C1WindowExtension.SetParameters(popupInboxType, parameters);

            popupInboxType.Closed += popupInboxType_Closed;
            grdMain.Children.Add(popupInboxType);
            popupInboxType.BringToFront();

        }

        private void popupInboxType_Closed(object sender, EventArgs e)
        {
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

            GetEquipmentWorkInfo();
            GetProductCart();
            //SetInspectorCombo(ComboInspector);
        }

        private void ButtonCart_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Tag == null)
                return;

            _selectProdLot = string.Empty;

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
                //GetProductLot(true);
                //UcPolymerFormProductionResult.SelectResultList();
                GetProductCart();
            }
            grdMain.Children.Remove(popup);
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
            this.grdMain.Children.Remove(popup);

            GetUserControlShift();
        }

        private void ComboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SelectProcessName();
            UcPolymerFormProductionResult.ProcessCode = ComboProcess.SelectedValue.GetString();
            SetEquipmentSegmentCombo(ComboEquipmentSegment);
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

        private void btnAssyCarry_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLoad()) return;

            FORM001_030_ASSYLOT_LOAD popupRunStart = new FORM001_030_ASSYLOT_LOAD { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popupRunStart.Name) == false)
                return;

            popupRunStart.ShiftID = UcPolymerFormShift.TextShift.Tag.GetString();
            popupRunStart.ShiftName = UcPolymerFormShift.TextShift.Text;
            popupRunStart.WorkerID = UcPolymerFormShift.TextWorker.Tag.GetString();
            popupRunStart.WorkerName = UcPolymerFormShift.TextWorker.Text;
            popupRunStart.ShiftDateTime = UcPolymerFormShift.TextShiftDateTime.Text;
            popupRunStart.InspectorCode = ComboInspector.SelectedValue.ToString();
            popupRunStart.DgAssyLot = dgAssyLot;
            popupRunStart.DgProductionInbox = DgProductionInbox;

            object[] parameters = new object[7];
            parameters[0] = Util.NVC(ComboProcess.SelectedValue);
            parameters[1] = Util.NVC(ComboProcess.Text);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _divisionCode;
            parameters[5] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[6] = _selectCart;
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            grdMain.Children.Add(popupRunStart);
            popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            FORM001_030_ASSYLOT_LOAD popup = sender as FORM001_030_ASSYLOT_LOAD;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                _selectProdLot = popup.ProductLot;
                GetProductCart();
            }
            grdMain.Children.Remove(popup);
        }

        private void dgAssyLot_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAssyLot.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgAssyLot.ItemsSource);

                if (dt.Rows[cell.Row.Index]["CHK"].Equals(1))
                {
                    dt.Rows[cell.Row.Index]["CHK"] = 0;
                }
                else
                {
                    dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                    dt.Rows[cell.Row.Index]["CHK"] = 1;
                }

                dt.AcceptChanges();

                if (dt.Rows[cell.Row.Index]["CHK"].Equals(1))
                {
                    UcPolymerFormCart.ProdLotId = Util.NVC(dt.Rows[cell.Row.Index]["PROD_LOTID"]);
                    UcPolymerFormCart.ProdWipSeq = Util.NVC(dt.Rows[cell.Row.Index]["WIPSEQ"]);

                    // 조회
                    GetUcPolymerFormProductionResult(dt.Rows[cell.Row.Index]);

                    dgAssyLot.ItemsSource = dt.AsDataView();
                    dgAssyLot.SelectedIndex = cell.Row.Index;
                }
                else
                {
                    UcPolymerFormProductionResult.InitializeControls();
                    dgAssyLot.ItemsSource = dt.AsDataView();
                }
            }

        }
        #endregion

        #region Mehod

        public void ProdListClickedProcess(string prodlot = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectCart)) return;

                // 조립 LOT 정보 조회
                GetAssyLotList();

                //// 완성 Inbox 조회
                //UcPolymerFormProductionResult.SelectResultList();

                // SHIFT, 작업자 
                GetUserControlShift();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// User Control에서 호출시 파라메터 수 맞춤
        /// </summary>
        /// <param name="isTemp"></param>
        /// <param name="isSelect"></param>
        public void GetProductLot(bool isTemp = false, bool isSelect = false)
        {
            // 조립 LOT 정보 조회
            GetAssyLotList();
        }

        public void GetProductCart(Button buttonClick = null, bool isSelect = false)
        {
            if (buttonClick != null)
            {
                SetCartButton(buttonClick);

                ProdListClickedProcess();
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

            ProdListClickedProcess();
        }

        public bool ClearControls()
        {
            try
            {
                _selectCart = string.Empty;
                UcPolymerFormCart.InitializeButtonControls();

                Util.gridClear(dgAssyLot);
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

        private void GetAssyLotList()
        {
            try
            {
                ShowLoadingIndicator();

                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["CTNR_ID"] = _selectCart;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSYLOT_CART_LOAD", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgAssyLot, dtResult, null, true);

                // 조립LOT 선택 체크
                if (string.IsNullOrWhiteSpace(_selectProdLot))
                {
                    if (dtResult == null || dtResult.Rows.Count == 0)
                        _selectProdLot = "";
                    else
                        _selectProdLot = dtResult.Rows[0]["PROD_LOTID"].ToString();
                }

                DataRow[] drSelect = dtResult.Select("PROD_LOTID = '" + _selectProdLot + "'");

                if (drSelect.Length == 0)
                {
                    UcPolymerFormProductionResult.InitializeControls();
                }
                else
                {
                    int row = dtResult.Rows.IndexOf(drSelect[0]);

                    DataTableConverter.SetValue(dgAssyLot.Rows[row].DataItem, "CHK", true);
                    dgAssyLot.SelectedIndex = row;

                    GetUcPolymerFormProductionResult(drSelect[0]);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

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

        #region 작업자 조회
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
        #endregion

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

        private bool ValidationLoad()
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

            if (string.IsNullOrEmpty(_selectCart))
            {
                //대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }
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
                UcPolymerFormCart.ProcessCode = _processCode;
                UcPolymerFormCart.ProcessName = _processName;
                UcPolymerFormProductionResult.ProcessCode = _processCode;
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
            SetEquipmentSegmentCombo(ComboEquipmentSegment);
            SetEquipmentCombo(ComboEquipment);
            //SetInspectorCombo(ComboInspector);
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

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
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

        private void GetUserControlEquipmentCode()
        {
            string equipmentCode = string.Empty;
            string equipmentSegment = string.Empty;

            equipmentCode = ComboEquipment.SelectedValue.GetString();
            equipmentSegment = ComboEquipmentSegment.SelectedValue.GetString();

            UcPolymerFormCart?.ChangeEquipment(equipmentCode);
            UcPolymerFormCart.EquipmentName = ComboEquipment.Text;
            UcPolymerFormProductionResult?.ChangeEquipment(equipmentCode, equipmentSegment);
        }

        private void GetUcPolymerFormProductionResult(DataRow drSelect)
        {
            _selectProdLot = Util.NVC(drSelect["PROD_LOTID"]);

            UcPolymerFormProductionResult.ShiftID = Util.NVC(UcPolymerFormShift.TextShift.Tag);
            UcPolymerFormProductionResult.WorkerID = Util.NVC(UcPolymerFormShift.TextWorker.Tag);
            UcPolymerFormProductionResult.WorkerName = Util.NVC(UcPolymerFormShift.TextWorker.Text);
            UcPolymerFormProductionResult.ShiftDateTime = Util.NVC(UcPolymerFormShift.TextShiftDateTime.Text);
            UcPolymerFormProductionResult.InspectorCode = ComboInspector.SelectedValue.GetString();

            UcPolymerFormProductionResult.EquipmentCode = ComboEquipment.SelectedValue.GetString();
            UcPolymerFormProductionResult.EquipmentName = ComboEquipment.Text;
            UcPolymerFormProductionResult.ProdLotId = Util.NVC(drSelect["PROD_LOTID"]);
            UcPolymerFormProductionResult.WipSeq = Util.NVC(drSelect["WIPSEQ"]);
            UcPolymerFormProductionResult.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
            UcPolymerFormProductionResult.DataRowProductLot = drSelect;
            UcPolymerFormProductionResult.SelectResultList();
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









        #endregion


    }
}
