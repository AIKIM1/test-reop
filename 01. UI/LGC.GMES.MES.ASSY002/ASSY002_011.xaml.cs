/*************************************************************************************
 Created Date : 2017.11.15
      Creator : 김동일K
   Decription : 와인더(파우치) 공정진척
--------------------------------------------------------------------------------------
 [Change History]
   2017.11.15   김동일K   : 와인더 공정진척 ASSY002_001 Source를 기본으로 개발
   2023.06.25   조영대    : 설비 Loss Level 2 Code 사용 체크 및 변환
   2024.01.24   오수현    : E20230901-001504 UcAssyResultDetail에 ChangeEquipment()함수 호출 부분 추가
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY002
{
    /// <summary>
    /// ASSY002_011.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY002_011 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        public UC_WORKORDER_LINE Ucworkorder = new UC_WORKORDER_LINE();
        public UcAssyCommand UcAssyCommand { get; set; }
        public UcAssySearch UcAssySearch { get; set; }
        public UcAssyProdLot UcAssyProdLot { get; set; }
        public UcAssyResultDetail UcAssyResultDetail { get; set; }
        public UcAssyDataCollect UcAssyDataCollect { get; set; }
        public UcAssyShift UcAssyShift { get; set; }

        public C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1DataGrid DgDefect { get; set; }
        public C1DataGrid DgDefectDetail { get; set; }

        private string _processCode;
        private string _testModeType = string.Empty;
        private bool _isSmallType;
        private bool _isLoaded = false;
        private bool _isTestMode = false;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        private CMM_ASSY_QUALITY_PKG _popQuality;

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

        private struct PreviewValues
        {
            public string PreviewProdLotId;
            public decimal? PreviewGoodQty;

            public PreviewValues(decimal? goodQty, string prodLotId)
            {
                PreviewGoodQty = goodQty;
                PreviewProdLotId = prodLotId;
            }
        }

        private PreviewValues _previewValues = new PreviewValues(null, string.Empty);

        public ASSY002_011()
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
            }

            if (ComboEquipmentSegment?.SelectedValue != null)
            {
                equipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
            }
            UcAssyResultDetail?.ChangeEquipment(equipmentCode, equipmentSegmentCode);// E20230901-001504 2024.01.24 추가
            UcAssyDataCollect?.ChangeEquipment(equipmentCode, equipmentSegmentCode);
        }

        private void InitializeUserControls()
        {
            UcAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            UcAssySearch = grdSearch.Children[0] as UcAssySearch;
            UcAssyProdLot = grdProductLot.Children[0] as UcAssyProdLot;
            UcAssyResultDetail = grdResult.Children[0] as UcAssyResultDetail;
            UcAssyDataCollect = grdDataCollect.Children[0] as UcAssyDataCollect;
            UcAssyShift = grdShift.Children[0] as UcAssyShift;

            if (UcAssyCommand != null)
            {
                UcAssyCommand.ProcessCode = _processCode;
                UcAssyCommand.IsSmallType = _isSmallType;
                UcAssyCommand.SetButtonVisibility();
            }

            if (UcAssyProdLot != null)
            {
                UcAssyProdLot.UcParentControl = this;
                UcAssyProdLot.ProcessCode = _processCode;
                UcAssyProdLot.IsSmalltype = _isSmallType;
                UcAssyProdLot.SetDataGridColumnVisibility();
            }

            if (UcAssyResultDetail != null)
            {
                UcAssyResultDetail.UcParentControl = this;
                UcAssyResultDetail.FrameOperation = FrameOperation;
                UcAssyResultDetail.ProcessCode = _processCode;
                UcAssyResultDetail.IsSmallType = _isSmallType;
                UcAssyResultDetail.SetControlProperties();
            }

            if (UcAssyDataCollect != null)
            {
                UcAssyDataCollect.UcParentControl = this;
                UcAssyDataCollect.ProcessCode = _processCode;
                UcAssyDataCollect.IsSmallType = _isSmallType;
                UcAssyDataCollect.FrameOperation = FrameOperation;
                UcAssyDataCollect.SetControlProperties();

                UcAssyDataCollect.SetInputHistButtonControls();
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
            DgDefect = UcAssyDataCollect.DgDefect;
            DgDefectDetail = UcAssyResultDetail.DgDefectDetail;
        }

        private void SetEventInUserControls()
        {
            if (UcAssyCommand != null)
            {
                UcAssyCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                UcAssyCommand.ButtonEqptCond.Click += ButtonEqptCond_Click;
                UcAssyCommand.ButtonWaitPancake.Click += ButtonWaitPancake_Click;
                UcAssyCommand.ButtonCancelTerm.Click += ButtonCancelTerm_Click;
                //UcAssyCommand.ButtonWindingTrayLocation.Click += ButtonWindingTrayLocation_Click;
                UcAssyCommand.ButtonTestMode.Click += ButtonTestMode_Click;
                UcAssyCommand.ButtonScheduledShutdown.Click += ButtonScheduledShutdown_Click;
                UcAssyCommand.ButtonQualityInput.Click += ButtonQualityInput_Click;
                UcAssyCommand.ButtonEqptIssue.Click += ButtonEqptIssue_Click;
                UcAssyCommand.ButtonStart.Click += ButtonStart_Click;
                UcAssyCommand.ButtonCancel.Click += ButtonCancel_Click;
                UcAssyCommand.ButtonEqptEnd.Click += ButtonEqptEnd_Click;
                UcAssyCommand.ButtonEqptEndCancel.Click += ButtonEqptEndCancel_Click;
                UcAssyCommand.ButtonConfirm.Click += ButtonConfirm_Click;
                UcAssyCommand.ButtonHistoryCard.Click += ButtonHistoryCard_Click;
                UcAssyCommand.ButtonRemarkHist.Click += ButtonRemarkHist_Click;
                UcAssyCommand.ButtonEditEqptQty.Click += ButtonEditEqptQty_Click;
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

            if (UcAssyResultDetail != null)
            {
                UcAssyResultDetail.ButtonSaveWipHistory.Click += ButtonSaveWipHistory_Click;
            }
        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);
            //HideTestMode();
        }

        /// <summary>
        /// Description     :   ASSY002_001.xaml Loaded 이벤트(Process Code 및 초소형 여부 설정, 권한별 버튼 설정, UserControl 정의 및 이벤트 설정)
        /// Author          :   신 광희
        /// Create Date     :   2017-06-07
        /// Update date     :   
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            _processCode = Process.WINDING_POUCH;
            _isSmallType = false;

            ApplyPermissions();
            SetWorkOrderWindow();
            Initialize();

            if (_isLoaded == false)
            {
                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
                    ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
            }
            _isLoaded = true;
            this.Loaded -= UserControl_Loaded;
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                UcAssyProduction.DispatcherTimer?.Stop();
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

        private void ButtonWaitPancake_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.Equals("") || ComboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            string workOrder;

            if (GetSelectWorkOrderRow() == null || _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y") == null)
            {
                workOrder = string.Empty;
            }
            else
            {
                workOrder = _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y").Field<string>("WOID").GetString();
            }

            // A음극, C양극
            string electrodeCode = string.Empty;
            string limitCount = string.Empty;
            string flag = string.Empty;
            string inputLotId = string.Empty;

            switch (btn.Name)
            {
                case "btnWaitPancake":
                    flag = "A";
                    break;
                case "btnProductWaitLot":
                    flag = "C";
                    break;
            }

            CMM_WAITING_PANCAKE_SEARCH popupWaitingPancake = new CMM_WAITING_PANCAKE_SEARCH { FrameOperation = FrameOperation };
            object[] parameters = new object[11];
            parameters[0] = workOrder;
            parameters[1] = ComboEquipmentSegment.SelectedValue.GetString();
            parameters[2] = _processCode;
            parameters[3] = electrodeCode;
            parameters[4] = limitCount;
            parameters[5] = inputLotId;
            parameters[6] = flag;
            parameters[7] = ComboEquipment.SelectedValue.GetString();
            parameters[8] = string.Empty;
            parameters[9] = _isSmallType;
            parameters[10] = "N";
            C1WindowExtension.SetParameters(popupWaitingPancake, parameters);
            popupWaitingPancake.Closed += new EventHandler(popupWaitingPancake_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupWaitingPancake.ShowModal()));
            //grdMain.Children.Add(popupWaitingPancake);
            //popupWaitingPancake.BringToFront();
        }

        private void popupWaitingPancake_Closed(object sender, EventArgs e)
        {
            CMM_WAITING_PANCAKE_SEARCH popup = sender as CMM_WAITING_PANCAKE_SEARCH;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            //this.grdMain.Children.Remove(popup);
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
            if (!ValidationTestMode()) return;

            if (_isTestMode)
            {
                SetTestMode(false);
                GetTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // 테스트 Run이 되면 실적처리가 되지 않습니다. 테스트 Run 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEqptMode.Text = ObjectDic.Instance.GetObjectName("테스트모드사용중");

                        SetTestMode(true);
                        GetTestMode();
                    }
                });
            }
        }

        private void ButtonScheduledShutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationScheduledShutdownMode()) return;

                if (_isTestMode)
                {
                    SetTestMode(false, bShutdownMode: true);
                    GetTestMode();
                }
                else
                {
                    Util.MessageConfirm("SFU4460", (result) => // 계획정지를 하시겠습니까?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

                            SetTestMode(true, bShutdownMode: true);
                            GetTestMode();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

            ASSY002_001_RUNSTART popupRunStart = new ASSY002_001_RUNSTART { FrameOperation = FrameOperation };

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = string.Empty;
            parameters[5] = _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y");
            C1WindowExtension.SetParameters(popupRunStart, parameters);
            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            //grdMain.Children.Add(popupRunStart);
            //popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            ASSY002_001_RUNSTART popup = sender as ASSY002_001_RUNSTART;
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
            parameters[6] = false;  // 재작업 여부
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

            if (!ValidationInspectionLot()) return;

            if (!string.IsNullOrEmpty(ValidationInspectionTime()))
            {
                object[] parameters = new object[1];
                parameters[0] = ValidationInspectionTime();

                Util.MessageConfirm("SFU3670", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmProcess();
                    }
                }, parameters);
            }
            else
            {
                Util.MessageConfirm("SFU1706", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmProcess();
                    }
                });
            }
        }

        private void ButtonHistoryCard_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationHistoryCard()) return;

            CMM_ASSY_HISTORYCARD popupHistoryCard = new CMM_ASSY_HISTORYCARD { FrameOperation = FrameOperation };

            object[] parameters = new object[6];
            parameters[0] = DataTableConverter.Convert(ComboEquipment.ItemsSource);
            parameters[1] = Util.NVC(ComboEquipment.SelectedIndex);
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

        private void ButtonRemarkHist_Click(object sender, RoutedEventArgs e)
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            CMM_ASSY_LOTCOMMENTHIST popupLotCommentHistory = new CMM_ASSY_LOTCOMMENTHIST { FrameOperation = FrameOperation };
            object[] parameters = new object[1];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
            C1WindowExtension.SetParameters(popupLotCommentHistory, parameters);

            popupLotCommentHistory.Closed += new EventHandler(popupLotCommentHistory_Closed);

            grdMain.Children.Add(popupLotCommentHistory);
            popupLotCommentHistory.BringToFront();

        }

        private void popupLotCommentHistory_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_LOTCOMMENTHIST popup = sender as CMM_ASSY_LOTCOMMENTHIST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popup);
        }

        private void ButtonEditEqptQty_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonVerify.HasDataGridRow(DgProductLot))
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            CMM_ASSY_EQPT_INPUT_QTY popupEqutInputQty = new CMM_ASSY_EQPT_INPUT_QTY { FrameOperation = this.FrameOperation };

            object[] parameters = new object[2];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[1] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "EQPT_END_QTY")).GetInt();
            C1WindowExtension.SetParameters(popupEqutInputQty, parameters);
            popupEqutInputQty.Closed += new EventHandler(popupEqutInputQty_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupEqutInputQty.ShowModal()));
            //grdMain.Children.Add(popupEqutInputQty);
            //popupEqutInputQty.BringToFront();

        }

        private void popupEqutInputQty_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_INPUT_QTY popup = sender as CMM_ASSY_EQPT_INPUT_QTY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
            //this.grdMain.Children.Remove(popup);
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            GetTestMode();
            GetWorkOrder();
            GetProductLot();
            GetEqptWrkInfo();
        }

        private void ButtonQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualitySearch()) return;

            //if (_popQuality != null) return;

            _popQuality = new CMM_ASSY_QUALITY_PKG { FrameOperation = FrameOperation };
            object[] parameters = new object[5];
            parameters[0] = ComboEquipmentSegment.SelectedValue;
            parameters[1] = _processCode;
            parameters[2] = ComboEquipment.SelectedValue;
            parameters[3] = ComboEquipmentSegment.Text;
            parameters[4] = ComboEquipment.Text;
            C1WindowExtension.SetParameters(_popQuality, parameters);
            _popQuality.Closed += new EventHandler(popQuality_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => _popQuality.ShowModal()));
        }

        private void popQuality_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG pop = sender as CMM_ASSY_QUALITY_PKG;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
            }
            _popQuality = null;
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
                //GetTestMode();
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

                UcAssyResultDetail?.ChangeEquipment(equipmentCode, equipmentSegmentCode);// E20230901-001504 2024.01.24 추가
                UcAssyDataCollect?.ChangeEquipment(equipmentCode, equipmentSegmentCode);

                // 설비 선택 시 자동 조회 처리
                if (ComboEquipment != null && (ComboEquipment.SelectedIndex > 0 && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex))
                {
                    if (ComboEquipment.SelectedValue.GetString() != "SELECT")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcAssySearch.ButtonSearch, null)));
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

        private void ConfirmProcess()
        {

            try
            {
                // 실적확정 전 불량/LOSS/물품청구 데이터 저장 로직 추가
                SaveDefectBeforeConfirm();
                ShowLoadingIndicator();

                string bizRuleName = _isSmallType ? "BR_PRD_REG_END_LOT_WNS" : "BR_PRD_REG_END_LOT_WN";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["INPUTQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                newRow["OUTPUTQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["RESNQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USERID"] = UcAssyShift.TextWorker.Tag;
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
                        System.Threading.Thread.Sleep(500);

                        if (string.Equals(_processCode, Process.WINDING_POUCH))
                        {
                            // Winding 이력카드 출력
                            PrintHistoryCard(_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString());
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        UcAssyResultDetail.ClearResultDetailControl();
                        ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
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
            int selectedIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            string selectedLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            string selectedWipSeq = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();

            if (_previewValues.PreviewProdLotId == selectedLotId)
            {
                if (CommonVerify.HasDataGridRow(DgDefectDetail))
                {
                    _previewValues.PreviewGoodQty = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
                }
            }
            else
            {
                _previewValues.PreviewProdLotId = selectedLotId;
                _previewValues.PreviewGoodQty = null;
            }

            UcAssyDataCollect.SaveDefectBeforeConfirm();

            if (string.Equals(_processCode, Process.WINDING_POUCH))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                if (_isSmallType)
                {
                    UcAssyDataCollect.GetProductCellList(UcAssyDataCollect.DgProdCellWinding, false);
                }
                else
                {
                    CalculateDefectQty();
                }
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                CalculateDefectQty();
            }
            else if (string.Equals(_processCode, Process.WASHING))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                //GetOutTray();
            }
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
            dr["WIPNOTE"] = new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
            dr["WRK_USERID"] = Util.NVC(UcAssyShift.TextWorker.Tag);
            dr["WRK_USER_NAME"] = Util.NVC(UcAssyShift.TextWorker.Text);
            //dr["LANE_PTN_QTY"] = 0;
            //dr["LANE_QTY"] = 0;
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
                const string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_WN";
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
                const string bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_WN";

                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["INPUT_LOTID"] = null; // Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "PR_LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;

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

                // W/O 타라인 콤보 설정을 위한 기본 파라미터.
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

                if (_previewValues.PreviewProdLotId == selectedLot)
                {
                    if (CommonVerify.HasDataGridRow(DgDefectDetail))
                    {
                        _previewValues.PreviewGoodQty = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
                    }
                }
                else
                {
                    _previewValues.PreviewProdLotId = selectedLot;
                    _previewValues.PreviewGoodQty = null;
                }

                const string wipState = "PROC,EQPT_END";
                UcAssyDataCollect.ClearDataCollectControl();
                ShowLoadingIndicator();

                // DA_PRD_SEL_PRODUCTLOT_WNS 초소형 , DA_PRD_SEL_PRODUCTLOT_WN 원각
                string bizRuleName = _isSmallType ? "DA_PRD_SEL_PRODUCTLOT_WNS" : "DA_PRD_SEL_PRODUCTLOT_WN";

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
                                //int iRowRun = _util.GetDataGridRowIndex(DgProductLot, "WIPSTAT", "PROC");
                                int iRowRun = _util.GetDataGridFirstRowIndexByEquiptmentEnd(DgProductLot, "WIPSTAT", "EQPT_END");
                                if (iRowRun < 0)
                                {
                                    iRowRun = 0;
                                    if (DgProductLot.TopRows.Count > 0)
                                        iRowRun = DgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    if (string.IsNullOrEmpty(_previewValues.PreviewProdLotId))
                                    {
                                        _previewValues.PreviewProdLotId = DataTableConverter.GetValue(DgProductLot.Rows[iRowRun].DataItem, "LOTID").GetString();
                                    }
                                    //row 색 바꾸기
                                    DgProductLot.SelectedIndex = iRowRun;
                                    DgProductLot.Selection.Clear();

                                    ProdListClickedProcess(iRowRun);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    if (string.IsNullOrEmpty(_previewValues.PreviewProdLotId))
                                    {
                                        _previewValues.PreviewProdLotId = DataTableConverter.GetValue(DgProductLot.Rows[iRowRun].DataItem, "LOTID").GetString();
                                    }
                                    //row 색 바꾸기
                                    DgProductLot.SelectedIndex = iRowRun;
                                    DgProductLot.Selection.Clear();

                                    ProdListClickedProcess(iRowRun);

                                }
                            }
                            else
                            {
                                // 현재 설비 투입 자재 조회 처리.
                                GetCurrInputLotList();
                                GetWaitPancakeList();
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
                                GetWaitPancakeList();
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
            if (UcAssyDataCollect != null)
            {
                UcAssyDataCollect.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollect.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollect.ProdLotId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "LOTID").GetString();
                UcAssyDataCollect.ProdWorkInProcessSequence = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WIPSEQ").GetString();
                UcAssyDataCollect.ProdWorkOrderId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WOID").GetString();
                UcAssyDataCollect.ProdWorkOrderDetailId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WO_DETL_ID").GetString();
                UcAssyDataCollect.SearchAllDataCollect();
            }
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(DgDefect.ItemsSource);

                if (CommonVerify.HasTableRow(dtTmp))
                {
                    //DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT'").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString()));
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

        public void GetTrayFormLoad(string trayId, string trayTag, string outLotId, string wipQty)
        {
            try
            {

                int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                if (rowIdx >= 0)
                {
                    string selectedLotId = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                    string workOrderDetailId = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "WO_DETL_ID").GetString();

                    CMM_TRAY_CELL_INFO popupTrayCellInfo = new CMM_TRAY_CELL_INFO
                    {
                        FrameOperation = FrameOperation
                    };

                    // SET PARAMETER
                    object[] parameters = new object[10];
                    parameters[0] = _processCode;
                    parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                    parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
                    parameters[3] = Util.NVC(ComboEquipment.Text);
                    parameters[4] = selectedLotId;
                    parameters[5] = outLotId;
                    parameters[6] = trayId;
                    parameters[7] = trayTag;
                    parameters[8] = workOrderDetailId;
                    parameters[9] = wipQty;

                    C1WindowExtension.SetParameters(popupTrayCellInfo, parameters);

                    popupTrayCellInfo.Closed += new EventHandler(TrayCellInfo_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupTrayCellInfo.ShowModal()));
                    //grdMain.Children.Add(popupTrayCellInfo);
                    //popupTrayCellInfo.BringToFront();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TrayCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_TRAY_CELL_INFO popup = sender as CMM_TRAY_CELL_INFO;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetWorkOrder();
                GetProductLot();
            }
            grdMain.Children.Remove(popup);
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
                //UcAssyResultDetail.ClearResultDetailControl();
                UcAssyDataCollect.ClearDataCollectControl();
                UcAssyResultDetail.ClearResultDetailControl();
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
            string selectedLot = string.Empty;

            if (CommonVerify.HasDataGridRow(DgProductLot))
            {
                int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                if (rowIdx >= 0)
                {
                    selectedLot = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                }
            }

            if (_previewValues.PreviewProdLotId == selectedLot)
            {
                if (CommonVerify.HasDataGridRow(DgDefectDetail))
                {
                    _previewValues.PreviewGoodQty = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
                }
            }
            else
            {
                _previewValues.PreviewProdLotId = selectedLot;
                _previewValues.PreviewGoodQty = null;
            }

            //UcAssyProduction.ClearProductionControl();
            UcAssyDataCollect.ClearDataCollectControl();
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

                UcAssyDataCollect.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollect.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollect.EquipmentCodeName = ComboEquipment.Text;
                UcAssyDataCollect.ProcessCode = _processCode;
                UcAssyDataCollect.ProdWorkOrderId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WOID"));
                UcAssyDataCollect.ProdWorkOrderDetailId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WO_DETL_ID"));
                UcAssyDataCollect.ProdLotState = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSTAT"));
                UcAssyDataCollect.ProdLotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                UcAssyDataCollect.ProdWorkInProcessSequence = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                //UcAssyDataCollect.CellManagementTypeCode = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "CELL_MNGT_TYPE_CODE"));
                UcAssyDataCollect.ProdSelectedCheckRowIdx = iRow;

                // UcAssyDataCollect 영역 조회
                GetInputSelectInfo(iRow);

                // 생산실적 UcAssyResultDetail 영역 컨트롤 바인딩
                UcAssyResultDetail.ClearResultDetailControl();
                UcAssyResultDetail.SetResultDetailControl(DgProductLot.Rows[iRow].DataItem);

                // 생산실적 UcAssyResultDetail 영역 그리드(DgDefectDetail) 초기화 
                SetDefectDetail();
                // UcAssyDataCollect 영역 불량/LOSS/물품청구 조회
                GetDefectInfo(lotId, wipSeq);
                // 투입 양품 불량 수량 계산
                CalculateDefectQty();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCurrInputLotList()
        {
            if (UcAssyDataCollect != null)
            {
                if (!CommonVerify.HasDataGridRow(DgProductLot))
                {
                    UcAssyDataCollect.ProdLotId = string.Empty;
                    UcAssyDataCollect.ProdWorkInProcessSequence = string.Empty;
                    UcAssyDataCollect.ProdWorkOrderId = string.Empty;
                    UcAssyDataCollect.ProdWorkOrderDetailId = string.Empty;
                    UcAssyDataCollect.ProdLotState = string.Empty;
                }
                UcAssyDataCollect.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollect.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollect.ProcessCode = _processCode;
                UcAssyDataCollect.GetMaterialInputList();
            }
        }

        private void GetWaitPancakeList()
        {
            if (UcAssyDataCollect != null)
            {
                if (!CommonVerify.HasDataGridRow(DgProductLot))
                {
                    UcAssyDataCollect.ProdLotId = string.Empty;
                    UcAssyDataCollect.ProdWorkInProcessSequence = string.Empty;
                    UcAssyDataCollect.ProdWorkOrderId = string.Empty;
                    UcAssyDataCollect.ProdWorkOrderDetailId = string.Empty;
                    UcAssyDataCollect.ProdLotState = string.Empty;
                }
                UcAssyDataCollect.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollect.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollect.ProcessCode = _processCode;
                UcAssyDataCollect.GetWaitPancake();
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
                    finally
                    {
                        //HiddenLoadingIndicator();
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
                Util.gridClear(DgDefect);
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

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void CalculateDefectQty()
        {
            // 양품수량
            Decimal totalCellQty = 0;

            if (_processCode.Equals(Process.WINDING_POUCH))
            {
                if (_isSmallType)
                {
                    if (CommonVerify.HasDataGridRow(UcAssyDataCollect.DgProdCellWinding))
                    {
                        DataTable dt = ((DataView)UcAssyDataCollect.DgProdCellWinding.ItemsSource).Table;
                        totalCellQty = dt.AsEnumerable().Sum(s => s.Field<Decimal>("CELLQTY"));
                    }
                }
                else
                {
                    totalCellQty = GetGoodQty();
                }
            }

            SumDefectQty();

            if (CommonVerify.HasDataGridRow(DgDefectDetail))
            {
                if (DgDefectDetail.Rows.Count - DgDefectDetail.TopRows.Count > 0)
                {
                    double goodQty = 0;
                    double eqptQty = 0;
                    double inputQtyByType = 0;
                    int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                    goodQty = (double)totalCellQty;

                    if (rowIndex > 0)
                    {
                        if (_previewValues.PreviewGoodQty != null && _previewValues.PreviewGoodQty > 0)
                        {
                            goodQty = _previewValues.PreviewGoodQty.GetDouble();
                        }
                        eqptQty = DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "EQPT_END_QTY").GetDouble();
                        inputQtyByType = GetInputQtyByApplyTypeCode().GetDouble();
                        DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "EQPTQTY", eqptQty + inputQtyByType);
                        DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                    }

                    //double defectQty = Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")));
                    double defect = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                    double loss = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                    double chargeprd = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                    double defectQty = defect + loss + chargeprd;
                    double outputQty = goodQty + defectQty;

                    //양품수량
                    //DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                    //생산수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                    //투입수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", outputQty);
                    //추가불량
                    //UcAssyResultDetail.TextAssyResultQty.Text = Math.Abs(eqptQty - (defectQty - GetEtcDefect().GetDouble()) - goodQty) < 0 ? (eqptQty - (defectQty - GetEtcDefect().GetDouble()) - goodQty).ToString("##,###") : "0";
                    if (Math.Abs((eqptQty + inputQtyByType) - (defectQty + goodQty)) > 0)
                    {
                        UcAssyResultDetail.TextAssyResultQty.Text = ((eqptQty + inputQtyByType) - (defectQty + goodQty)).ToString("##,###");
                        UcAssyResultDetail.TextAssyResultQty.FontWeight = FontWeights.Bold;
                        UcAssyResultDetail.TextAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        UcAssyResultDetail.TextAssyResultQty.Text = "0";
                        UcAssyResultDetail.TextAssyResultQty.FontWeight = FontWeights.Normal;
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                        if (convertFromString != null)
                            UcAssyResultDetail.TextAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }
        }

        public decimal GetEtcDefect()
        {
            decimal defectDecimal = 0;

            if (CommonVerify.HasDataGridRow(DgDefect))
            {
                //전극 기인 수량
                DataTable dt = ((DataView)DgDefect.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("RESNCODE") == "P138541APC"
                             select new
                             {
                                 ResnQty = t.Field<decimal>("RESNQTY")
                             }).FirstOrDefault();
                if (query != null)
                {
                    defectDecimal = query.ResnQty;
                }
            }
            return defectDecimal;
        }

        private decimal GetInputQtyByApplyTypeCode()
        {
            decimal qty = 0;

            if (CommonVerify.HasDataGridRow(DgDefect))
            {
                DataTable dt = ((DataView)DgDefect.ItemsSource).Table;

                decimal qtyPlus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "PLUS").Sum(s => s.Field<Decimal>("RESNQTY"));
                decimal qtyMinus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "MINUS").Sum(s => s.Field<Decimal>("RESNQTY"));
                return qtyPlus - qtyMinus;
            }
            return qty;
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

        public DataRow GetSelectWorkOrderRow()
        {
            return _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y");
        }

        private void PrintHistoryCard(string lotId)
        {
            try
            {
                //string bizRuleName = _isSmallType ? "BR_PRD_SEL_WINDING_RUNCARD_WNS" : "BR_PRD_SEL_WINDING_RUNCARD_WN";
                //string bizRuleName = _isSmallType ? "BR_PRD_SEL_WINDING_RUNCARD_WNS" : "BR_PRD_SEL_WINDING_RUNCARD_WN_V01";
                //string outPutData = _isSmallType ? "OUT_DATA,OUT_ELEC,OUT_DFCT,OUT_SEPA,OUT_TRAY" : "OUT_DATA,OUT_ELEC";
                string bizRuleName = "BR_PRD_SEL_WINDING_RUNCARD_WN_V01_WP";
                string outPutData = "OUT_DATA,OUT_ELEC";

                DataSet ds = _bizDataSet.GetBR_PRD_SEL_WINDING_RUNCARD_WN();
                DataTable indataTable = ds.Tables["IN_DATA"];
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = lotId;
                indataTable.Rows.Add(indata);
                //ds.Tables.Add(indataTable);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_DATA", outPutData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        CMM_ASSY_WINDERCARD_PRINT poopupHistoryCard = new CMM_ASSY_WINDERCARD_PRINT { FrameOperation = this.FrameOperation };
                        object[] parameters = new object[5];
                        parameters[0] = result;
                        parameters[1] = _isSmallType;
                        parameters[2] = lotId;
                        parameters[3] = _processCode;
                        parameters[4] = true;
                        C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                        poopupHistoryCard.Closed += new EventHandler(poopupHistoryCard_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => poopupHistoryCard.ShowModal()));
                        //grdMain.Children.Add(poopupHistoryCard);
                        //poopupHistoryCard.BringToFront();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WINDERCARD_PRINT popup = sender as CMM_ASSY_WINDERCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            //this.grdMain.Children.Remove(popup);
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
            /*
            if (_util.GetDataGridRowIndex(DgProductLot, "WIPSTAT", "PROC") > -1)
            {
                //Util.Alert("장비에 진행 중 인 LOT이 존재 합니다.");
                Util.MessageValidation("SFU1863");
                return false;
            }
            */

            if (_util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y") == null)
            {
                Util.MessageValidation("SFU1635");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
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

            if (DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() == 0)
            {
                //전극 기인 수량을 확인하세요.
                if (GetEtcDefect().Equals(0))
                {
                    Util.MessageValidation("SFU4116");
                    return false;
                }
            }

            if (Math.Abs(UcAssyResultDetail.TextAssyResultQty.Text.GetDecimal()) > 0)
            {
                Util.MessageValidation("SFU3665");
                return false;
            }


            return true;
        }

        private bool ValidationInspectionLot()
        {
            const string bizRuleName = "DA_QCA_CHK_MAND_INSP_ITEM_RESULT_LOT";

            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PROCID"] = _processCode;
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
            {
                Util.MessageInfo("SFU3669", dtResult.Rows[0]["CLCTNAME"].GetString());
                return false;
            }
            else
            {
                return true;
            }
        }

        private string ValidationInspectionTime()
        {

            const string bizRuleName = "BR_QCA_CHK_MAND_INSP_ITEM_RESULT_TIME";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PROCID"] = _processCode;
            dr["EQPTID"] = ComboEquipment.SelectedValue;
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
            {
                //Util.MessageInfo("SFU1605", dtResult.Rows[0]["CLCTNAME"].GetString());
                return dtResult.Rows[0]["CLCTNAME"].GetString();
            }
            else
            {
                return string.Empty;
            }
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
            //if (CheckInputHistoryInfo(lotid, wipSeq))
            //{
            //    Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
            //    return false;
            //}
            /*
            // 완성 이력 정보 존재여부 확인
            if (CheckOutTrayInfo(lotid, wipSeq))
            {
                Util.MessageValidation("SFU3438");   // 생산Tray가 존재하여 취소할 수 없습니다.
                return false;
            }
            */
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

        private bool ValidationHistoryCard()
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

        private bool ValidationTestMode()
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

        private bool ValidationScheduledShutdownMode()
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
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_P", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_M", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("GOODQTY", typeof(int));
            inDataTable.Columns.Add("EQPTQTY", typeof(int));
            inDataTable.Columns.Add("DTL_DEFECT", typeof(int));
            inDataTable.Columns.Add("DTL_LOSS", typeof(int));
            inDataTable.Columns.Add("DTL_CHARGEPRD", typeof(int));
            inDataTable.Columns.Add("DEFECTQTY", typeof(int));

            DataRow dtRow = inDataTable.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["ALPHAQTY_P"] = 0;
            dtRow["ALPHAQTY_M"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["EQPTQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;

            inDataTable.Rows.Add(dtRow);
            DgDefectDetail.ItemsSource = DataTableConverter.Convert(inDataTable);
        }

        private void SetComboBox()
        {
            CommonCombo combo = new CommonCombo();
            
            String[] sFilter = { LoginInfo.CFG_AREA_ID, null, _processCode };
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

        private void ColorAnimationInredRectangle()
        {
            recTestMode.Fill = redBrush;
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);
            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainContents.RowDefinitions[3].Height.Value <= 0) return;

                //MainContents.RowDefinitions[1].Height = new GridLength(0);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0.3, GridUnitType.Star),
                    To = new GridLength(0, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            _isTestMode = false;

        }

        private void ShowTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainContents.RowDefinitions[3].Height.Value > 0) return;

                //MainContents.RowDefinitions[1].Height = new GridLength(8);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(0.3, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += ShowTestAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            _isTestMode = true;
        }

        private void ShowTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInredRectangle();
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        private bool SetTestMode(bool bOn, bool bShutdownMode = false)
        {
            try
            {
                string bizRuleName;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_MODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (bShutdownMode)
                {
                    bizRuleName = "BR_EQP_REG_EQPT_OPMODE_LOSS";

                    newRow["IFMODE"] = "ON";
                    newRow["UI_LOSS_MODE"] = bOn ? "ON" : "OFF";
                    newRow["UI_LOSS_CODE"] = bOn ? Util.ConvertEqptLossLevel2Change("LC003") : ""; // 계획정지 loss 코드.
                }
                else
                {
                    bizRuleName = "BR_EQP_REG_EQPT_OPMODE";
                    newRow["IFMODE"] = bOn ? "TEST" : "ON";
                }

                newRow["EQPTID"] = ComboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "IN_EQP", null, inTable);

                Util.MessageInfo("PSS9097");    // 변경되었습니다.

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetTestMode()
        {
            try
            {
                if (ComboEquipment?.SelectedValue == null) return;
                if (Util.NVC(ComboEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                DataTable inTable = _bizDataSet.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = ComboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO_S", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE") && dtRslt.Columns.Contains("MODE_TYPE") && dtRslt.Columns.Contains("SCHEDULED_SHUTDOWN"))
                {
                    _testModeType = Util.NVC(dtRslt.Rows[0]["MODE_TYPE"]);

                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        //HideTestMode();

                        if (Util.NVC(dtRslt.Rows[0]["SCHEDULED_SHUTDOWN"]).Equals("Y"))
                        {
                            ShowScheduledShutdown();
                        }
                        else
                        {
                            HideScheduledShutdown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowScheduledShutdown()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("계획정지");

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bScheduledShutdown) return;
                if (MainContents.RowDefinitions[3].Height.Value > 0)
                {
                    ColorAnimationInRectangle(false);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(0.3, GridUnitType.Star),
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += ShowScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);

            }));

            _isTestMode = true;
        }

        private void HideScheduledShutdown()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainContents.RowDefinitions[3].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0.3, GridUnitType.Star),
                    To = new GridLength(0, GridUnitType.Star),
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += HideScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            _isTestMode = false;

        }

        private void ShowScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(false);
        }

        private void HideScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void ColorAnimationInRectangle(bool isTest)
        {
            try
            {
                string name;
                if (isTest)
                {
                    recTestMode.Fill = redBrush;
                    name = "redBrush";
                }
                else
                {
                    recTestMode.Fill = yellowBrush;
                    name = "yellowBrush";
                }

                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.8),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                Storyboard.SetTargetName(opacityAnimation, name);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

                mouseLeftButtonDownStoryboard.Begin(this);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
