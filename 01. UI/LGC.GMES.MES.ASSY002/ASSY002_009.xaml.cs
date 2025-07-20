/*************************************************************************************
 Created Date : 2017.07.24
      Creator : 신광희C
   Decription : Washing 재작업 공정진척(PALLET) 작업 화면
--------------------------------------------------------------------------------------
 [Change History]
2017.11.01   신광희C   : Washing 재작업 공정진척(PALLET) 신규 화면 
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


namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_009 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        public UC_WORKORDER_LINE Ucworkorder = new UC_WORKORDER_LINE();
        public UcAssyCommand UcAssyCommand { get; set; }
        public UcAssySearch UcAssySearch { get; set; }
        public UcAssyProdLot UcAssyProdLot { get; set; }
        public UcAssyInput UcAssyInput { get; set; }
        public UcAssyShift UcAssyShift { get; set; }

        public C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1DataGrid DgDefect { get; set; }
        public C1DataGrid DgDefectDetail { get; set; }

        private string _processCode;
        private string _cellManagementTypeCode;
        private bool _isSmallType;
        private bool _isReWork;
        private bool _isLoaded = false;

        SolidColorBrush myAnimatedBrush = new SolidColorBrush(Colors.Fuchsia);

        private struct PreviewValues
        {
            public string PreviewTray;

            public PreviewValues(string tray)
            {
                PreviewTray = tray;
            }
        }
        private PreviewValues _previewValues = new PreviewValues("");

        public string ProcessCode
        {
            get { return _processCode; }
            set
            {
                _processCode = value;
            }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY002_009()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeUserControls();
            InitializeUserControlsGrid();
            SetComboBox();
            SetEventInUserControls();

            string equipmentCode = string.Empty;
            string equipmentSegmentCode = string.Empty;

            if (ComboEquipment?.SelectedValue != null)
            {
                equipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyInput.EquipmentId = ComboEquipment.SelectedValue.GetString();
            }

            if (ComboEquipmentSegment?.SelectedValue != null)
            {
                equipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyInput.EquipmentSegment = ComboEquipmentSegment.SelectedValue.GetString();
            }
            UcAssyInput?.ChangeEquipment(equipmentCode, equipmentSegmentCode);
        }

        private void InitializeUserControls()
        {
            UcAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            UcAssySearch = grdSearch.Children[0] as UcAssySearch;
            UcAssyProdLot = grdProductLot.Children[0] as UcAssyProdLot;
            UcAssyInput = grdInput.Children[0] as UcAssyInput;
            UcAssyShift = grdShift.Children[0] as UcAssyShift;

            if (UcAssyCommand != null)
            {
                UcAssyCommand.ProcessCode = _processCode;
                UcAssyCommand.IsSmallType = _isSmallType;
                UcAssyCommand.IsReWork = _isReWork;
                UcAssyCommand.SetButtonVisibility();
            }

            if (UcAssyProdLot != null)
            {
                UcAssyProdLot.UcParentControl = this;
                UcAssyProdLot.ProcessCode = _processCode;
                UcAssyProdLot.IsSmalltype = _isSmallType;
                UcAssyProdLot.SetDataGridColumnVisibility();
            }

            if (UcAssyInput != null)
            {
                UcAssyInput.UcParentControl = this;
                UcAssyInput.FrameOperation = FrameOperation;

                UcAssyInput.ProcessCode = _processCode;
                UcAssyInput.IsSmallType = _isSmallType;
                UcAssyInput.IsReWork = _isReWork;
                UcAssyInput.SetControlVisibility();
                UcAssyInput.GetProcMtrlInputRule();
                //UcAssyInput.InitializeControls();
            }


            if (UcAssySearch != null)
            {
                UcAssySearch.ProcessCode = _processCode;
                ComboEquipmentSegment = UcAssySearch.ComboEquipmentSegment;
                ComboEquipment = UcAssySearch.ComboEquipment;
            }
        }

        private void InitializeUserControlsGrid()
        {
            DgProductLot = UcAssyProdLot.DgProductLot;
            DgDefect = UcAssyInput.DgDefect;
            DgDefectDetail = UcAssyInput.DgDefectDetail;
        }

        private void SetEventInUserControls()
        {
            if (UcAssyCommand != null)
            {
                UcAssyCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                UcAssyCommand.ButtonEqptCond.Click += ButtonEqptCond_Click;
                UcAssyCommand.ButtonCancelTerm.Click += ButtonCancelTerm_Click;
                UcAssyCommand.ButtonTestMode.Click += ButtonTestMode_Click;
                UcAssyCommand.ButtonQualityInput.Click += ButtonQualityInput_Click;
                UcAssyCommand.ButtonSelfInspection.Click += ButtonSelfInspection_Click;
                UcAssyCommand.ButtonEqptIssue.Click += ButtonEqptIssue_Click;
                UcAssyCommand.ButtonStart.Click += ButtonStart_Click;
                UcAssyCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcAssyCommand.ButtonEqptEnd.Click += ButtonEqptEnd_Click;
                UcAssyCommand.ButtonEqptEndCancel.Click += ButtonEqptEndCancel_Click;
                UcAssyCommand.ButtonConfirm.Click += ButtonConfirm_Click;
                UcAssyCommand.ButtonHistoryCard.Click += ButtonHistoryCard_Click;
                UcAssyCommand.ButtonQualitySearch.Click += ButtonQualitySearch_Click;
            }

            if (UcAssySearch != null)
            {
                UcAssySearch.ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;
                UcAssySearch.ButtonSearch.Click += ButtonSearch_Click;
            }

            if (UcAssyShift != null)
            {
                UcAssyShift.ButtonShift.Click += ButtonShift_Click;
            }

            if (UcAssyInput != null)
            {
                UcAssyInput.ButtonSaveWipHistory.Click += ButtonSaveWipHistory_Click;
            }
        }



        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Description     :   ASSY002_009.xaml Loaded 이벤트(Process Code 및 초소형 여부 설정, 권한별 버튼 설정, UserControl 정의 및 이벤트 설정)
        /// Author          :   신 광희
        /// Create Date     :   2017-06-07
        /// Update date     :   
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.WASHING;
            _isSmallType = false;
            _isReWork = true;

            ApplyPermissions();
            SetWorkOrderWindow();
            Initialize();

            if (_isLoaded == false)
            {
                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
                    ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
            }
            _isLoaded = true;
            RegisterName("myAnimatedBrush", myAnimatedBrush);
            this.Loaded -= UserControl_Loaded;
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                _dispatcherTimer?.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            */
        }

        private void ButtonExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            C1DropDownButton btn = sender as C1DropDownButton;
            if (btn != null) btn.IsDropDownOpen = false;
        }

        private void ButtonEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEquipmentCondition())
                return;

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            CMM_ASSY_PU_EQPT_COND popupEqptCond = new CMM_ASSY_PU_EQPT_COND { FrameOperation = FrameOperation };
            object[] parameters = new object[6];
            parameters[0] = ComboEquipmentSegment.SelectedValue;
            parameters[1] = ComboEquipment.SelectedValue;
            parameters[2] = _processCode;
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));
            parameters[5] = ComboEquipment.Text;

            C1WindowExtension.SetParameters(popupEqptCond, parameters);

            popupEqptCond.Closed += new EventHandler(popupEqptCond_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            //this.Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
            grdMain.Children.Add(popupEqptCond);
            popupEqptCond.BringToFront();
        }

        private void popupEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PU_EQPT_COND popup = sender as CMM_ASSY_PU_EQPT_COND;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popup);
        }

        private void ButtonCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_CANCEL_TERM popupCanCelTerm = new CMM_ASSY_CANCEL_TERM { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            parameters[0] = _processCode;
            C1WindowExtension.SetParameters(popupCanCelTerm, parameters);
            popupCanCelTerm.Closed += new EventHandler(popupCanCelTerm_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupCanCelTerm.ShowModal()));
            //grdMain.Children.Add(popupCanCelTerm);
            //popupCanCelTerm.BringToFront();
        }

        private void popupCanCelTerm_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
            //this.grdMain.Children.Remove(window);
        }

        private void ButtonTestMode_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualityInfo())
                return;

            CMM_ASSY_QUALITY_INPUT_LOT_TIME popupQualityInput = new CMM_ASSY_QUALITY_INPUT_LOT_TIME
            {
                FrameOperation = FrameOperation
            };

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = ComboEquipmentSegment.SelectedValue;
            parameters[1] = ComboEquipment.SelectedValue;
            parameters[2] = _processCode;
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WO_DETL_ID"));

            C1WindowExtension.SetParameters(popupQualityInput, parameters);
            popupQualityInput.Closed += new EventHandler(popupQualityInput_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            this.Dispatcher.BeginInvoke(new Action(() => popupQualityInput.ShowModal()));

        }

        private void popupQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_INPUT_LOT_TIME popup = sender as CMM_ASSY_QUALITY_INPUT_LOT_TIME;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }

        private void ButtonSelfInspection_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelfInspection()) return;

            CMM_COM_QUALITY popupSelfInspection = new CMM_COM_QUALITY
            {
                FrameOperation = FrameOperation
            };

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupSelfInspection, parameters);
            popupSelfInspection.Closed += popupSelfInspection_Closed;

            // 팝업 화면 숨겨지는 문제 수정.
            Dispatcher.BeginInvoke(new Action(() => popupSelfInspection.ShowModal()));
        }

        private void popupSelfInspection_Closed(object sender, EventArgs e)
        {
            CMM_COM_QUALITY popup = sender as CMM_COM_QUALITY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }


        private void ButtonEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.Equals("") || ComboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            CMM_COM_EQPCOMMENT popupEqpComment = new CMM_COM_EQPCOMMENT { FrameOperation = FrameOperation };

            object[] parameters = new object[10];
            parameters[0] = ComboEquipmentSegment.SelectedValue.GetString();
            parameters[1] = ComboEquipment.SelectedValue.ToString();
            parameters[2] = _processCode;
            parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
            parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
            parameters[5] = ComboEquipment.Text;
            parameters[6] = UcAssyShift.TextShift.Text;     // 작업조명
            parameters[7] = UcAssyShift.TextShift.Tag;      // 작업조코드
            parameters[8] = UcAssyShift.TextWorker.Text;    // 작업자명
            parameters[9] = UcAssyShift.TextWorker.Tag;     // 작업자 ID

            C1WindowExtension.SetParameters(popupEqpComment, parameters);
            popupEqpComment.Closed += new EventHandler(popupEqpComment_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupEqpComment.ShowModal()));
            //grdMain.Children.Add(popupEqpComment);
            //popupEqpComment.BringToFront();
        }

        private void popupEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT popup = sender as CMM_COM_EQPCOMMENT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            //this.grdMain.Children.Remove(popup);
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
                return;

            ASSY002_009_RUNSTART popupRunStart = new ASSY002_009_RUNSTART { FrameOperation = FrameOperation };

            object[] parameters = new object[5];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y");
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            //grdMain.Children.Add(popupRunStart);
            //popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            ASSY002_009_RUNSTART popup = sender as ASSY002_009_RUNSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
            }
            //grdMain.Children.Remove(popup);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelRun()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        private void ButtonEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEquipmentEnd()) return;

            CMM_ASSY_EQUIPMENT_END popupEqpEnd = new CMM_ASSY_EQUIPMENT_END { FrameOperation = FrameOperation };
            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[7];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPDTTM_ST_ORG"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "DTTM_NOW"));
            parameters[5] = _isSmallType;
            parameters[6] = _isReWork;  // 재작업 여부
            C1WindowExtension.SetParameters(popupEqpEnd, parameters);
            popupEqpEnd.Closed += new EventHandler(popupEqpEnd_Closed);
            grdMain.Children.Add(popupEqpEnd);
            popupEqpEnd.BringToFront();

        }

        private void popupEqpEnd_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQUIPMENT_END popup = sender as CMM_ASSY_EQUIPMENT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void ButtonEqptEndCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEquipmentEndCancel()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EquipmentEndCancel();
                }
            });

        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            DgDefectDetail.EndEdit();
            DgDefectDetail.EndEditRow(true);

            if (!ValidationConfirm()) return;
            // 불량, 투입자재, 투입 반제품 저장여부 체크
            //if (!ValidationDataCollect()) return;

            Util.MessageConfirm("SFU1706", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }
            });
        }

        private void ButtonHistoryCard_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_HISTORYCARD popupHistoryCard = new CMM_ASSY_HISTORYCARD { FrameOperation = FrameOperation };

            object[] parameters = new object[6];
            parameters[0] = DataTableConverter.Convert(ComboEquipment.ItemsSource);
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedIndex);
            parameters[2] = DataTableConverter.Convert(ComboEquipmentSegment.ItemsSource);
            parameters[3] = Util.NVC(ComboEquipmentSegment.SelectedIndex);
            parameters[4] = _processCode;
            parameters[5] = _isSmallType;

            C1WindowExtension.SetParameters(popupHistoryCard, parameters);
            popupHistoryCard.Closed += new EventHandler(popupHistoryCard_Closed);
            grdMain.Children.Add(popupHistoryCard);
            popupHistoryCard.BringToFront();
        }

        private void popupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_HISTORYCARD popup = sender as CMM_ASSY_HISTORYCARD;
            // 이력 팝업 종료후 처리
            this.grdMain.Children.Remove(popup);
        }

        private void ButtonQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualitySearch()) return;

            CMM_ASSY_QUALITY_PKG popQuality = new CMM_ASSY_QUALITY_PKG { FrameOperation = FrameOperation };

            object[] parameters = new object[5];
            parameters[0] = ComboEquipmentSegment.SelectedValue;
            parameters[1] = _processCode;
            parameters[2] = ComboEquipment.SelectedValue;
            parameters[3] = ComboEquipmentSegment.Text;
            parameters[4] = ComboEquipment.Text;

            C1WindowExtension.SetParameters(popQuality, parameters);

            popQuality.Closed += new EventHandler(popQuality_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            this.Dispatcher.BeginInvoke(new Action(() => popQuality.ShowModal()));
        }

        private void popQuality_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG pop = sender as CMM_ASSY_QUALITY_PKG;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(pop);
        }

        private void ButtonSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveWipHistory()) return;
            SaveWipHistory();
            //Util.MessageConfirm("SFU1241", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        SaveWipHistory();
            //    }
            //});
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            GetWorkOrder();
            GetProductLot();
            GetEqptWrkInfo();
        }

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(UcAssyShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcAssyShift.TextWorker.Tag);
            parameters[6] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupShiftUser.ShowModal()));
            //grdMain.Children.Add(popupShiftUser);
            //popupShiftUser.BringToFront();
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            //this.grdMain.Children.Remove(popup);
        }

        private void ComboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                ClearControls();
                UcAssyShift.ClearShiftControl();

                if (Ucworkorder != null)
                {
                    Ucworkorder.EQPTSEGMENT = ComboEquipmentSegment.SelectedValue.GetString();
                    Ucworkorder.EQPTID = ComboEquipment.SelectedValue.GetString();
                    Ucworkorder.PROCID = _processCode;
                }

                string equipmentCode = string.Empty;
                string equipmentSegmentCode = string.Empty;

                if (ComboEquipment?.SelectedValue != null)
                    equipmentCode = ComboEquipment.SelectedValue.GetString();

                if (ComboEquipmentSegment?.SelectedValue != null)
                    equipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();

                UcAssyInput?.ChangeEquipment(equipmentCode, equipmentSegmentCode);

                // 설비 선택 시 자동 조회 처리
                if (ComboEquipment != null && (ComboEquipment.SelectedIndex > 0 && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex))
                {
                    if (ComboEquipment.SelectedValue.GetString() != "SELECT")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcAssySearch.ButtonSearch, null)));
                    }
                }

                //GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletQty_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(((Key.D0 <= e.Key) && (e.Key <= Key.D9))
                || ((Key.NumPad0 <= e.Key) && (e.Key <= Key.NumPad9))
                || e.Key == Key.Back))
            {
                e.Handled = true;
            }
        }

        private void btnPalletCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletCreate()) return;

            //Pallet를 생성 하시겠습니까?
            Util.MessageConfirm("SFU4006", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletCreate();
                }
            });
        }

        private void btnPalletDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletDelete()) return;

            //Pallet를 삭제 하시겠습니까?
            Util.MessageConfirm("SFU4008", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletDelete();
                }
            });
        }

        private void btnPalletPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletPrint()) return;

            //발행하시겠습니까?
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletPrint();
                }
            });
        }

        private void dgOutPallet_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgOutPallet.CurrentRow.DataItem as DataRowView;
                if (drv == null) return;

                if (e.Cell.Column.Name == "CHK")
                {
                    int rowIndex = 0;
                    foreach (var item in dgOutPallet.Rows)
                    {
                        if (drv["CHK"].GetString() == "True" || drv["CHK"].GetString() == "1")
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", e.Cell.Row.Index == rowIndex);
                        }
                        else
                        {
                            DataTableConverter.SetValue(item.DataItem, "CHK", false);
                        }
                        rowIndex++;
                    }

                    dgOutPallet.EndEdit();
                    dgOutPallet.EndEditRow(true);
                }

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        private void ConfirmProcess()
        {

            try
            {
                SaveDefectBeforeConfirm();
                ShowLoadingIndicator();
                /*
                Name	Type	Default	Description

                SRCTYPE	String		(*)Input Type(UI or EQ)
                IFMODE	String		(*)I/F Mode(ON/OFF)
                EQPTID	String		(*)Equipment ID
                SHIFT	String		(*)작업조
                WIPDTTM_ED	DateTime		(*)완공일시
                WIPNOTE	String		비고
                WRK_USER_NAME	String		확정시 작업자
                USERID	String		(*)User ID
                PROD_LOTID	String		(*)생산 LOT ID
                INPUT_QTY	Decimal		투입 수량
                OUTPUT_QTY	Decimal		완성 수량
                RESNQTY	Decimal		(*)Total Defect Quantity


                SRCTYPE	String		(*)Input Type(UI or EQ)
                IFMODE	String		(*)I/F Mode(ON/OFF)
                EQPTID	String		(*)Equipment ID
                SHIFT	String		(*)작업조
                WIPDTTM_ED	DateTime		(*)완공일시
                WIPNOTE	String		비고
                WRK_USER_NAME	String		확정시 작업자
                USERID	String		(*)User ID
                PROD_LOTID	String		(*)생산 LOT ID
                INPUT_QTY	Decimal		투입 수량
                OUTPUT_QTY	Decimal		완성 수량
                RESNQTY	Decimal		(*)Total Defect Quantity
                */

                const string bizRuleName = "BR_PRD_REG_END_LOT_ASSY_PL";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WS();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = UcAssyInput.ProdLotId;
                newRow["INPUT_QTY"] = 0;
                newRow["OUTPUT_QTY"] = 0;
                newRow["RESNQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = new TextRange(UcAssyInput.TextRemark.Document.ContentStart, UcAssyInput.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USER_NAME"] = UcAssyShift.TextWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        private void SaveDefectBeforeConfirm()
        {
            UcAssyInput.SaveDefectBeforeConfirm();
            //GetOutTray();
        }

        private void SaveWipHistory()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "BR_ACT_REG_SAVE_LOT";

            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            dr["PROD_VER_CODE"] = null;
            dr["SHIFT"] = UcAssyShift.TextShift.Tag;
            dr["WIPNOTE"] = new TextRange(UcAssyInput.TextRemark.Document.ContentStart, UcAssyInput.TextRemark.Document.ContentEnd).Text;
            dr["WRK_USERID"] = Util.NVC(UcAssyShift.TextWorker.Tag);
            dr["WRK_USER_NAME"] = Util.NVC(UcAssyShift.TextWorker.Text);
            dr["PROD_QTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
            dr["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(dr);

            DataSet ds = new DataSet();
            ds.Tables.Add(inDataTable);
            string xml = ds.GetXml();

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetProductLot();
                    //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            });

        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();
                // Assembly 시작 취소 원각 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_AS";
                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
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

        private void EquipmentEndCancel()
        {
            try
            {
                ShowLoadingIndicator();

                // Assembly 장비완료 취소 원각 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_WS";
                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                Ucworkorder.FrameOperation = FrameOperation;
                Ucworkorder._UCParent = this;
                Ucworkorder.PROCID = _processCode;
                grdWorkOrder.Children.Add(Ucworkorder);
            }
        }

        public void GetWorkOrder()
        {
            if (Ucworkorder == null)
                return;

            Ucworkorder.EQPTSEGMENT = ComboEquipmentSegment.SelectedValue.GetString();
            Ucworkorder.EQPTID = ComboEquipment.SelectedValue.GetString();
            Ucworkorder.PROCID = _processCode;

            Ucworkorder.GetWorkOrder();
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
                ClearControls();
                const string wipState = "PROC,EQPT_END";

                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_WS_FOR_RW";
                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_PRODUCTLOT_ASSY();
                DataRow newRow = indataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["WIPSTAT"] = wipState;
                newRow["PROCID"] = _processCode;
                newRow["WIPTYPECODE"] = "PROD";

                indataTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(indataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (result, ex) =>
                {
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
                                int iRowRun = _util.GetDataGridRowIndex(DgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)
                                {
                                    iRowRun = 0;
                                    if (DgProductLot.TopRows.Count > 0)
                                        iRowRun = DgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    DgProductLot.SelectedIndex = iRowRun;
                                    ProdListClickedProcess(iRowRun);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    DgProductLot.SelectedIndex = iRowRun;
                                    ProdListClickedProcess(iRowRun);
                                }
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInputLotList();
                                //GetWaitPancakeList();
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
                                ProdListClickedProcess(idx);

                                // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                                DgProductLot.CurrentCell = DgProductLot.GetCell(idx, DgProductLot.Columns.Count - 1);
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInputLotList();
                                //GetWaitPancakeList();
                            }
                        }
                    }
                    GetLossCount();
                    HiddenLoadingIndicator();
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

        private void GetInputSelectInfo(int row)
        {
            if (UcAssyInput != null)
            {
                UcAssyInput.EquipmentId = ComboEquipment.SelectedValue.GetString();
                UcAssyInput.EquipmentSegment = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyInput.ProdLotId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "LOTID").GetString();
                UcAssyInput.ProdWorkInProcessSequence = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WIPSEQ").GetString();
                UcAssyInput.ProdWorkOrderId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WOID").GetString();
                UcAssyInput.ProdWorkOrderDetailId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WO_DETL_ID").GetString();
                UcAssyInput.SearchAll();
            }
        }

        private void SumDefectQty()
        {
            if (!CommonVerify.HasDataGridRow(DgDefect)) return;
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(DgDefect.ItemsSource);

                if (CommonVerify.HasTableRow(dtTmp))
                {
                    //DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT'").ToString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString()));
                }
                else
                {
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", 0);
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public bool GetSearchConditions(out string processCode, out string equipmentSegmentCode, out string equipmentCode)
        {
            try
            {
                processCode = _processCode;
                equipmentSegmentCode = ComboEquipmentSegment.SelectedIndex >= 0 ? ComboEquipmentSegment.SelectedValue.ToString() : string.Empty;
                equipmentCode = ComboEquipment.SelectedIndex >= 0 ? ComboEquipment.SelectedValue.ToString() : string.Empty;

                return true;
            }
            catch (Exception)
            {
                processCode = string.Empty;
                equipmentSegmentCode = string.Empty;
                equipmentCode = string.Empty;
                return false;
            }
        }

        public bool ClearControls()
        {
            try
            {
                Util.gridClear(DgProductLot);
                Util.gridClear(dgOutPallet);
                txtPalletQty.Text = string.Empty;
                txtNote.Text = string.Empty;

                UcAssyInput.ClearDataGrid();
                _cellManagementTypeCode = string.Empty;
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

        }

        public void GetAllInfoFromChild()
        {
            GetProductLot();
        }

        public void ProdListClickedProcess(int iRow)
        {
            try
            {
                if (iRow < 0 || !_util.GetDataGridCheckValue(DgProductLot, "CHK", iRow)) return;
                string lotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                string wipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                _cellManagementTypeCode = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "CELL_MNGT_TYPE_CODE"));
                //ChangeCellManagementType(_cellManagementTypeCode);
                // 투입영역 데이터 조회
                GetInputSelectInfo(iRow);

                // 생산영역의 데이터 조회
                SetDefectDetail();
                GetDefectInfo(lotId, wipSeq);
                UcAssyInput.SetResultDetailControl(DgProductLot.Rows[iRow].DataItem);
                //완성Pallet 리스트        
                GetOutPallet();

                //GetOutTraybyAsync();
                //GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCurrInputLotList()
        {
            if (UcAssyInput != null)
            {
                if (!CommonVerify.HasDataGridRow(DgProductLot))
                {
                    UcAssyInput.ProdLotId = string.Empty;
                    UcAssyInput.ProdWorkInProcessSequence = string.Empty;
                    UcAssyInput.ProdWorkOrderId = string.Empty;
                    UcAssyInput.ProdWorkOrderDetailId = string.Empty;
                    UcAssyInput.ProdLotState = string.Empty;
                }
                //UcAssyInput.TabControl.SelectedIndex = 0;
                UcAssyInput.EquipmentSegment = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyInput.EquipmentId = ComboEquipment.SelectedValue.GetString();
                UcAssyInput.ProcessCode = _processCode;
                UcAssyInput.GetCurrInList();
            }
        }

        private void GetWaitPancakeList()
        {
            if (UcAssyInput != null)
            {
                if (!CommonVerify.HasDataGridRow(DgProductLot))
                {
                    UcAssyInput.ProdLotId = string.Empty;
                    UcAssyInput.ProdWorkInProcessSequence = string.Empty;
                    UcAssyInput.ProdWorkOrderId = string.Empty;
                    UcAssyInput.ProdWorkOrderDetailId = string.Empty;
                    UcAssyInput.ProdLotState = string.Empty;
                }
                UcAssyInput.EquipmentSegment = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyInput.EquipmentId = ComboEquipment.SelectedValue.GetString();
                UcAssyInput.ProcessCode = _processCode;
                UcAssyInput.GetWaitPancake();
            }
        }

        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable indataTable = new DataTable("RQSTDT");
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("SHOPID", typeof(string));
                indataTable.Columns.Add("AREAID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));

                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                indata["PROCID"] = _processCode;

                indataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcAssyShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcAssyShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcAssyShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcAssyShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcAssyShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcAssyShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcAssyShift.TextShiftEndTime.Text))
                                {
                                    UcAssyShift.TextShiftDateTime.Text = UcAssyShift.TextShiftStartTime.Text + " ~ " + UcAssyShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcAssyShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcAssyShift.TextWorker.Text = string.Empty;
                                    UcAssyShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcAssyShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcAssyShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcAssyShift.TextShift.Tag = string.Empty;
                                    UcAssyShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcAssyShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcAssyShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcAssyShift.ClearShiftControl();
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
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetDefectInfo(string lotId, string wipSeq)
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", ds, FrameOperation.MENUID);
                if (CommonVerify.HasTableInDataSet(dsResult))
                {
                    //'AP' 동별 / 공정별
                    //'LP' 라인 / 공정별
                    DgDefect.Columns["CLSS_NAME1"].Visibility = dsResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                    DgDefect.ItemsSource = DataTableConverter.Convert(dsResult.Tables["OUTDATA"]);
                }
                /*
                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                if (searchResult != null)
                {
                    //Util.GridSetData(DgDefect, searchResult, null, true);
                    DgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
                }
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutPallet()
        {
            try
            {

                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                if (rowIndex < 0)
                    return;

                ShowLoadingIndicator();
                const string bizRuleName = "DA_PRD_SEL_OUT_PALLET_LIST_PL";

                DataTable dtInTable = new DataTable("RQSTDT");
                dtInTable.Columns.Add("PROD_LOTID", typeof(string));
                DataRow dr = dtInTable.NewRow();
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                dtInTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtInTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    //dgOutPallet.ItemsSource = DataTableConverter.Convert(searchResult);
                    Util.GridSetData(dgOutPallet, searchResult, FrameOperation, true);
                    CalculateDefectQty();
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void CalculateDefectQty()
        {
            Decimal totalCellQty = 0;
            if (CommonVerify.HasDataGridRow(dgOutPallet))
            {
                DataTable dt = ((DataView)dgOutPallet.ItemsSource).Table;
                totalCellQty = dt.AsEnumerable().Sum(s => s.Field<Decimal>("WIPQTY"));
            }
            SumDefectQty();

            if (CommonVerify.HasDataGridRow(DgDefectDetail))
            {
                if (DgDefectDetail.Rows.Count - DgDefectDetail.TopRows.Count > 0)
                {
                    double goodQty = (double)totalCellQty;
                    double inputQty = GetInputQty();
                    double defectQty = Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")));
                    double outputQty = goodQty + defectQty;

                    //양품수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                    //생산수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                    //투입수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", inputQty);

                    //UcAssyInput.TextDifferenceQty.Text = goodQty.Equals(inputQty) ? "0" : (inputQty - outputQty).ToString("##,###");
                    if (goodQty.Equals(inputQty))
                    {
                        UcAssyInput.TextDifferenceQty.Text = "0";
                        UcAssyInput.TextDifferenceQty.FontWeight = FontWeights.Normal;
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                        if (convertFromString != null)
                            UcAssyInput.TextDifferenceQty.Foreground = new SolidColorBrush((Color) convertFromString);
                    }
                    else
                    {
                        UcAssyInput.TextDifferenceQty.Text = (inputQty - outputQty).ToString("##,###");
                        UcAssyInput.TextDifferenceQty.FontWeight = FontWeights.Bold;
                        UcAssyInput.TextDifferenceQty.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
            }
            
        }

        private string GetConfirmDate()
        {
            string confirmDate;

            const string bizRuleName = "DA_PRD_SEL_CONFIRM_LOT_INFO";
            string prodLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = prodLotId;
            dr["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).Trim()))
            {
                confirmDate = Util.NVC(searchResult.Rows[0]["NOW_CALDATE"]).GetString();
            }
            else
            {
                confirmDate = Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).GetString();
            }

            return confirmDate;
        }

        private Decimal GetGoodQty()
        {
            Decimal returnGoodQty = 0;

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            const string bizRuleName = "DA_PRD_SEL_TMP_PROD_QTY";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            returnGoodQty = CommonVerify.HasTableRow(searchResult) ? searchResult.Rows[0]["TMP_PROD_QTY"].GetDecimal() : 0;
            return returnGoodQty;
        }

        private double GetInputQty()
        {
            double returnInputQty = 0;

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            const string bizRuleName = "DA_PRD_SEL_INPUT_HALFPROD_WS";
            const string materialType = "PROD";

            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LANGID", typeof(string));
            indataTable.Columns.Add("LOTID", typeof(string));
            indataTable.Columns.Add("MTRLTYPE", typeof(string));
            indataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            indataTable.Columns.Add("EQPTID", typeof(string));
            indataTable.Columns.Add("PROD_LOTID", typeof(string));

            DataRow dr = indataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = null;
            dr["MTRLTYPE"] = materialType;
            dr["EQPT_MOUNT_PSTN_ID"] = null;
            dr["EQPTID"] = ComboEquipment.SelectedValue;
            dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            indataTable.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "RSLTDT", indataTable);
            if (CommonVerify.HasTableRow(dt))
            {
                returnInputQty = dt.Compute("SUM(INPUT_QTY)", "").GetDouble();
            }
            return returnInputQty;
        }

        public DataRow GetSelectWorkOrderRow()
        {
            return _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y");
        }

        private void PalletCreate()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CREATE_PALLET_ASSY_PL";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inDataTable.Columns.Add("CELL_QTY", typeof(decimal));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["USERID"] = LoginInfo.USERID;
                dr["WIPNOTE"] = txtNote.Text;
                dr["SHIFT"] = UcAssyShift.TextShift.Tag;// UcAssyShift.TextShift.Tag;
                dr["WRK_USERID"] = Util.NVC(UcAssyShift.TextWorker.Tag);// Util.NVC(UcAssyShift.TextWorker.Tag);
                dr["WRK_USER_NAME"] = Util.NVC(UcAssyShift.TextWorker.Text);//Util.NVC(UcAssyShift.TextWorker.Text);
                dr["CELL_QTY"] = txtPalletQty.Text.GetDecimal();
                inDataTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetProductLot();
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

        private void PalletDelete()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_DELETE_PALLET_ASSY_PL";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOutPallet.Rows)
                {
                    if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1")
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["IFMODE"] = IFMODE.IFMODE_OFF;
                        dr["EQPTID"] = ComboEquipment.SelectedValue;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["LOTID"] = DataTableConverter.GetValue(row.DataItem, "PALLETID").GetString();
                        inDataTable.Rows.Add(dr);
                    }
                }

                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        GetProductLot();
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

        private void PalletPrint()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_PALLET_RUNCARD_DATA_ASSY_PL";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PALLET_ID", typeof(string));
                DataRow indata = inDataTable.NewRow();
                indata["PALLET_ID"] = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                inDataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        CMM_ASSY_PALLET_PRINT poopupPallet = new CMM_ASSY_PALLET_PRINT { FrameOperation = this.FrameOperation };
                        object[] parameters = new object[3];
                        parameters[0] = result;
                        parameters[1] = _util.GetDataGridFirstRowBycheck(dgOutPallet, "CHK").Field<string>("PALLETID").GetString();
                        parameters[2] = _processCode;

                        C1WindowExtension.SetParameters(poopupPallet, parameters);
                        poopupPallet.Closed += new EventHandler(poopupPallet_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => poopupPallet.ShowModal()));
                    }
                    else
                    {
                        //데이터가 없습니다.
                        Util.MessageValidation("SFU1498");
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupPallet_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PALLET_PRINT popup = sender as CMM_ASSY_PALLET_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetOutPallet();
            }
        }

        private bool ValidationSearch()
        {

            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
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
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y") == null)
            {
                Util.MessageValidation("SFU1635");
                return false;
            }

            if (!CheckSelectWorkOrderInfo())
            {
                return false;
            }

            return true;
        }

        private bool ValidationEquipmentEnd()
        {
            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            string wipState = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "WIPSTAT"));

            if (!wipState.Equals("PROC"))
            {
                Util.MessageValidation("SFU1866");
                return false;
            }

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

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextShift.Text))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            if (DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetString() == "0")
            {
                //양품 수량을 확인하십시오.
                Util.MessageValidation("SFU1722");
                return false;
            }

            if (Math.Abs(UcAssyInput.TextDifferenceQty.Text.GetDouble()) > 0)
            {
                //차이수량이 존재하여 실적 확정이 불가 합니다.\r\n생산실적을 재 확인 해주세요.
                Util.MessageValidation("SFU3701");
                return false;
            }
            return true;
        }

        private bool ValidationCancelRun()
        {

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            string lotid = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            string wipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

            // 투입 이력 정보 존재여부 확인            
            if (CheckInputHistoryInfo(lotid, wipSeq))
            {
                Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
                return false;
            }

            // 완성 이력 정보 존재여부 확인
            if (CheckOutTrayInfo(lotid, wipSeq))
            {
                Util.MessageValidation("SFU3438");   // 생산Tray가 존재하여 취소할 수 없습니다.
                return false;
            }

            return true;
        }

        private bool ValidationEquipmentCondition()
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

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationQualityInfo()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationSelfInspection()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationEquipmentEndCancel()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                Util.MessageValidation("SFU1864");  // 장비완료 상태의 LOT이 아닙니다.
                return false;
            }
            return true;
        }

        private bool ValidationSaveDefect()
        {
            if (CommonVerify.HasDataGridRow(DgDefect))
            {
                //Util.Alert("불량 항목이 없습니다.");
                Util.MessageValidation("SFU1578");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }
            else
            {
                if (DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID").GetString().Length < 1)
                {
                    Util.MessageValidation("SFU1195");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationQualitySearch()
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
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(DgDefectDetail))
            {
                Util.MessageValidation("SFU3552");
                return false;
            }

            return true;
        }

        private bool ValidationPalletCreate()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(DgProductLot, "CHK");
            if (rowIndex < 0)
            {
                //선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            if (string.IsNullOrEmpty(txtPalletQty.Text) || txtPalletQty.Text.GetDecimal() < 1)
            {
                //양품 수량을 확인하십시오.
                Util.MessageValidation("SFU1722");
                return false;
            }

            return true;
        }

        private bool ValidationPalletDelete()
        {
            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOutPallet, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationPalletPrint()
        {

            if (_util.GetDataGridCheckFirstRowIndex(dgOutPallet, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            //int iRow = _util.GetDataGridCheckFirstRowIndex(dgOutPallet, "CHK");
            //if (iRow < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            return true;
        }

        private bool CheckInputHistoryInfo(string lotId, string wipSeq)
        {
            bool bRet = false;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_INPUT_MTRL_HIST_CNT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if(CommonVerify.HasTableRow(dtRslt))
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }

        private bool CheckOutTrayInfo(string lotid, string wipSeq)
        {
            bool bRet = false;
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_CNT_WS";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = ComboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = lotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if(CommonVerify.HasTableRow(dtRslt))
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckSelectWorkOrderInfo()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_SET_WORKORDER_INFO";
                bool bRet = false;
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SET_WORKORDER_INFO();
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals(""))
                    {
                        //Util.Alert("선택된 W/O가 없습니다.");
                        Util.MessageValidation("SFU1635");
                    }
                    else if (Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK"]).Equals("N"))
                    {
                        Util.MessageValidation("SFU1635");
                    }
                    else
                    {
                        bRet = true;
                    }
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private static DataTable GetOutTrayAddColumn(DataTable dt)
        {
            var dtBinding = dt.Copy();
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "CELLQTY_BASE", DataType = typeof(decimal) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "SPECIALYN_BASE", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "SPECIALDESC_BASE", DataType = typeof(string) });
            dtBinding.Columns.Add(new DataColumn() { ColumnName = "TransactionFlag", DataType = typeof(string) });

            foreach (DataRow row in dtBinding.Rows)
            {
                row["CELLQTY_BASE"] = row["CELLQTY"];
                row["SPECIALYN_BASE"] = row["SPECIALYN"];
                row["SPECIALDESC_BASE"] = row["SPECIALDESC"];
                row["TransactionFlag"] = "N";
            }
            dtBinding.AcceptChanges();
            return dtBinding;
        }

        private bool CheckInputEquipmentCondition()
        {
            try
            {
                bool bRet = false;
                const string bizRuleName = "DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_CNT";
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_EQPT_CLCT_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "LOTID"));
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetLossCount()
        {
            try
            {
                DataTable dtLossCount = Util.Get_EqpLossCnt(ComboEquipment.SelectedValue.GetString());
                if (CommonVerify.HasTableRow(dtLossCount))
                {
                    UcAssyShift.TextLossCnt.Text = Util.NVC(dtLossCount.Rows[0]["CNT"]);
                    UcAssyShift.TextLossCnt.Background = dtLossCount.Rows[0]["CNT"].GetInt() > 0 ? new SolidColorBrush(Colors.LightPink) : new SolidColorBrush(Colors.WhiteSmoke);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void SetDefectDetail()
        {
            DataTable dtTmp = _bizDataSet.GetDA_PRD_SEL_DEFECT_DTL();

            DataRow dtRow = dtTmp.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["ALPHAQTY_P"] = 0;
            dtRow["ALPHAQTY_M"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;

            dtTmp.Rows.Add(dtRow);

            DgDefectDetail.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        private void SetComboBox()
        {
            CommonCombo combo = new CommonCombo();

            //원각 : CR , 초소형 : CS
            const string gubun = "CR";
            String[] sFilter = { LoginInfo.CFG_AREA_ID , gubun, _processCode};
            C1ComboBox[] cboLineChild = { ComboEquipment };
            combo.SetCombo(ComboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { _processCode };
            C1ComboBox[] cboEquipmentParent = { ComboEquipmentSegment };
            combo.SetCombo(ComboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);

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
            var ucAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            if (ucAssyCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                    ucAssyCommand.ButtonStart,
                    ucAssyCommand.ButtonCancel,
                    ucAssyCommand.ButtonConfirm
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }


        #endregion


    }
}
