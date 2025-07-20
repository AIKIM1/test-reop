/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 신광희C
   Decription : Washing 초소형 작업 화면
--------------------------------------------------------------------------------------
 [Change History]
   2017.05.29   신광희C   : Washing 공정진척 UserControl(UcAssyCommand,UcAssySearch,UcAssyProdLot,UcAssyResultDetail,UcAssyCollect) 내에서 발생하는 이벤트 및 메소드 분리 작업 
                          : 1. 다른 UserControl 의 영향이 가지 않는 경우 각각의 UserControl 내부에서 이벤트 및 메소드 처리를 원칙으로 함
                            2. UserControl 간의 참조가 필요한 경우 ASSY002_006.xaml.cs 파일에서 처리 함 
   2021.09.03   심찬보S   : 오창 소형조립UI 버전추가 및 활성화(PROD_VER_CODE) 
   2023.06.25   조영대    : 설비 Loss Level 2 Code 사용 체크 및 변환
   2024.01.30   차의진    : 실적확정 시 허용범위를 넘은 물품 요청자, 사유 등 정보 DB 저장 
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
    public partial class ASSY002_006 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        string _LOTID = string.Empty;
        string _PRODID = string.Empty;
        string _VERSION = string.Empty;

        public UcAssyCommand UcAssyCommand { get; set; }
        public UcAssySearch UcAssySearch { get; set; }
        public UcAssyProdLot UcAssyProdLot { get; set; }
        public UcAssyProduction UcAssyProduction { get; set; }
        public UcAssyShift UcAssyShift { get; set; }

        public C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1DataGrid DgDefect { get; set; }
        public C1DataGrid DgDefectDetail { get; set; }
        //추가
        public C1DataGrid PRODLOT_GRID { get; set; }

        private string _processCode;
        private string _testModeType = string.Empty;
        private bool _isSmallType;
        private bool _isLoaded = false;
        private bool _isTestMode = false;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        //2021-10-07 오화백 버튼셀
        private string _Product_Lev = string.Empty;

        //허용비율 초과 사유
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;
        bool _bInputErpRate = false;
        string _WipSeq = string.Empty;

        private struct PreviewValues
        {
            public string PreviewTray;

            public PreviewValues(string tray)
            {
                PreviewTray = tray;
            }
        }
        private PreviewValues _previewValues = new PreviewValues("");

        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        SolidColorBrush myAnimatedBrush = new SolidColorBrush(Colors.Fuchsia);

        private bool _isAutoSelectTime = false;

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

        public ASSY002_006()
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
        }

        private void InitializeUserControls()
        {
            UcAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            UcAssySearch = grdSearch.Children[0] as UcAssySearch;
            UcAssyProdLot = grdProductLot.Children[0] as UcAssyProdLot;
            UcAssyProduction = grdResult.Children[0] as UcAssyProduction;
            //UcAssyInput = grdInput.Children[0] as UcAssyInput;
            //UcAssyProduction = grdProduction.Children[0] as UcAssyProduction;
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

            if (UcAssyProduction != null)
            {
                UcAssyProduction.UcParentControl = this;
                UcAssyProduction.FrameOperation = FrameOperation;
                UcAssyProduction.ProcessCode = _processCode;
                UcAssyProduction.IsSmallType = _isSmallType;

                UcAssyProduction.SetTabVisibility();
                UcAssyProduction.SetInputHistButtonControls();
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
            DgDefect = UcAssyProduction.DgDefect;
            DgDefectDetail = UcAssyProduction.DgDefectDetail;
            //추가
            PRODLOT_GRID = (grdProductLot.Children[0] as UcAssyProdLot).DgProductLot;
        }

        private void SetEventInUserControls()
        {
            if (UcAssyCommand != null)
            {
                UcAssyCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                UcAssyCommand.ButtonEqptCond.Click += ButtonEqptCond_Click;
                UcAssyCommand.ButtonCancelTerm.Click += ButtonCancelTerm_Click;
                UcAssyCommand.ButtonWindingTrayLocation.Click += ButtonWindingTrayLocation_Click;
                UcAssyCommand.ButtonTestMode.Click += ButtonTestMode_Click;
                UcAssyCommand.ButtonScheduledShutdown.Click += ButtonScheduledShutdown_Click;
                UcAssyCommand.ButtonQualityInput.Click += ButtonQualityInput_Click;
                UcAssyCommand.ButtonSelfInspection.Click += ButtonSelfInspection_Click;
                UcAssyCommand.ButtonSelfInspectionNew.Click += ButtonSelfInspectionNew_Click;
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

            if (UcAssyProduction != null)
            {
                UcAssyProduction.ButtonSaveWipHistory.Click += ButtonSaveWipHistory_Click;
                UcAssyProduction.ButtonBoxCreate.Click += ButtonBoxCreate_Click;
                UcAssyProduction.ButtonVersion.Click += btnVersion_Click;
            }
        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            RegisterName("redBrush", redBrush);
            RegisterName("yellowBrush", yellowBrush);
            HideTestMode();
        }

        /// <summary>
        /// Description     :   ASSY002_006.xaml Loaded 이벤트(Process Code 및 초소형 여부 설정, 권한별 버튼 설정, UserControl 정의 및 이벤트 설정)
        /// Author          :   신 광희
        /// Create Date     :   2017-06-07
        /// Update date     :   
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.WASHING;
            _isSmallType = true;

            ApplyPermissions();
            //SetWorkOrderWindow();
            Initialize();

            if (_isLoaded == false)
            {
                if (!ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
                    ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
            }
            _isLoaded = true;
            RegisterName("myAnimatedBrush", myAnimatedBrush);
            Loaded -= UserControl_Loaded;
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

        private void ButtonWindingTrayLocation_Click(object sender, RoutedEventArgs e)
        {

            var btnTrayPosition = sender as Button;
            if (btnTrayPosition == null) return;
            if (!ValidationTrayLocation(btnTrayPosition.Name)) return;

            UcAssyProduction.DispatcherTimer?.Stop();

            CMM_WINDING_TRAY_LOCATION_ADJUSTMENT popupWindingTrayPosition = new CMM_WINDING_TRAY_LOCATION_ADJUSTMENT
            {
                FrameOperation = FrameOperation
            };

            string lotId = string.Empty;
            string outLotId = string.Empty;
            string trayId = string.Empty;
            string trayTag = "C";
            DataRow drWorkOrder = null;
            string prodId = string.Empty;
            string flag = string.Empty;

            if (btnTrayPosition.Name == "btnCellWindingLocation")
            {
                int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                if (idx >= 0)
                {
                    lotId = DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID").GetString();
                }

                C1DataGrid dg = UcAssyProduction.DgProductionCellWinding;

                int rowIdx = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (rowIdx < 0) return;
                outLotId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "OUT_LOTID")).GetString();
                trayId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "TRAYID")).Replace("\0", "");
                trayTag = "U";
                drWorkOrder = null;
                prodId = string.Empty;
                flag = string.Empty;
            }

            object[] parameters = new object[12];
            parameters[0] = _processCode;
            parameters[1] = ComboEquipmentSegment.SelectedValue.GetString();
            parameters[2] = ComboEquipment.SelectedValue.GetString();
            parameters[3] = ComboEquipment.Text;
            parameters[4] = lotId;
            parameters[5] = outLotId;
            parameters[6] = trayId;
            parameters[7] = trayTag;
            parameters[8] = drWorkOrder;
            parameters[9] = prodId;
            parameters[10] = flag;
            parameters[11] = _isSmallType;

            C1WindowExtension.SetParameters(popupWindingTrayPosition, parameters);
            popupWindingTrayPosition.Closed += new EventHandler(popupWindingTrayPosition_Closed);

            //this.Dispatcher.BeginInvoke(new Action(() => popupWindingTrayPosition.ShowModal()));
            grdMain.Children.Add(popupWindingTrayPosition);
            popupWindingTrayPosition.BringToFront();

        }

        private void popupWindingTrayPosition_Closed(object sender, EventArgs e)
        {
            CMM_WINDING_TRAY_LOCATION_ADJUSTMENT popup = sender as CMM_WINDING_TRAY_LOCATION_ADJUSTMENT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popup);

            if (UcAssyProduction.DispatcherTimer != null && UcAssyProduction.DispatcherTimer.Interval.TotalSeconds > 0)
                UcAssyProduction.DispatcherTimer.Start();
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

        private void ButtonSelfInspectionNew_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelfInspection()) return;

            CMM_COM_SELF_INSP popupSelfInspectionNew = new CMM_COM_SELF_INSP { FrameOperation = FrameOperation };
            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupSelfInspectionNew, parameters);
            popupSelfInspectionNew.Closed += popupSelfInspectionNew_Closed;

            Dispatcher.BeginInvoke(new Action(() => popupSelfInspectionNew.ShowModal()));
        }

        private void popupSelfInspectionNew_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP popup = sender as CMM_COM_SELF_INSP;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
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

            ASSY002_006_RUNSTART popupRunStart = new ASSY002_006_RUNSTART { FrameOperation = FrameOperation };

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = string.Empty;
            // Set Work Order Parameter
            //parameters[5] = Ucworkorder.GetSelectWorkOrderRow();
            parameters[5] = null;
            C1WindowExtension.SetParameters(popupRunStart, parameters);
            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            //grdMain.Children.Add(popupRunStart);
            //popupRunStart.BringToFront();
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            ASSY002_006_RUNSTART popup = sender as ASSY002_006_RUNSTART;
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

        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSYRECIPE wndPopup = new CMM_ASSYRECIPE();
            wndPopup.FrameOperation = this.FrameOperation;
            DataRowView rowview = sender as DataRowView;

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (iRow >= 0)
            {
                object[] Parameters = new object[5];

                Parameters[0] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "PRODID"));
                Parameters[1] = _processCode;
                Parameters[2] = LoginInfo.CFG_AREA_ID;
                Parameters[3] = UcAssySearch.ComboEquipment.SelectedValue;
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseVersion);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        protected virtual void OnCloseVersion(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.Popup.CMM_ASSYRECIPE window = sender as LGC.GMES.MES.CMM001.Popup.CMM_ASSYRECIPE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (window._ReturnRecipeNo == "선택취소")
                {
                    UcAssyProduction.TxtProdVerCode.Text = "";
                }
                else
                {
                    UcAssyProduction.TxtProdVerCode.Text = window._ReturnRecipeNo;
                }
            }
        }

        private void ButtonBoxCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationBoxCreate()) return;
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateBox();
                }
            });
            //CreateBox
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            //GetWorkOrder();
            GetTestMode();
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
                //GetTestMode();
                ClearControls();
                UcAssyShift.ClearShiftControl();

                string equipmentCode = string.Empty;
                string equipmentSegmentCode = string.Empty;

                if (ComboEquipment?.SelectedValue != null)
                    equipmentCode = ComboEquipment.SelectedValue.GetString();

                if (ComboEquipmentSegment?.SelectedValue != null)
                    equipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();

                UcAssyProduction?.ChangeEquipment(equipmentCode, equipmentSegmentCode);

                // 설비 선택 시 자동 조회 처리
                if (ComboEquipment != null && (ComboEquipment.SelectedIndex > 0 && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex))
                {
                    if (ComboEquipment.SelectedValue.GetString() != "SELECT")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcAssySearch.ButtonSearch, null)));
                    }
                }

                GetSpecialTrayInfo();
                //if (_processCode.Equals(Process.WASHING) && _isSmallType)
                //{
                //    UcAssyProduction.GetSpecialTrayInfo();
                //}

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

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Rate 관련 팝업
                //완료 처리 하기 전에 팝업 표시
                _bInputErpRate = false;
                _dtRet_Data.Clear();
                _sUserID = string.Empty;
                _sDepID = string.Empty;
                if (PERMIT_RATE_input(_LOTID, _WipSeq))
                {
                    return;
                }
                ///////////////////////////////////////////////////////////////////////////////
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_END_LOT_WS";
                //var dtTime = new DateTime(UcAssyProduction.DtpCaldate.SelectedDateTime.Year, UcAssyProduction.DtpCaldate.SelectedDateTime.Month, UcAssyProduction.DtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WS();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = UcAssyProduction.ProdLotId;
                //newRow["INPUT_QTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                //newRow["OUTPUT_QTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["INPUT_QTY"] = 0;
                newRow["OUTPUT_QTY"] = 0;
                newRow["RESNQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = new TextRange(UcAssyProduction.TextRemark.Document.ContentStart, UcAssyProduction.TextRemark.Document.ContentEnd).Text;
                //newRow["WRK_USERID"] = UcAssyShift.TextWorker.Tag;
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

        private void Confirm_Real()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_END_LOT_WS";
                //var dtTime = new DateTime(UcAssyProduction.DtpCaldate.SelectedDateTime.Year, UcAssyProduction.DtpCaldate.SelectedDateTime.Month, UcAssyProduction.DtpCaldate.SelectedDateTime.Day);

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WS();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = UcAssyProduction.ProdLotId;
                //newRow["INPUT_QTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                //newRow["OUTPUT_QTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["INPUT_QTY"] = 0;
                newRow["OUTPUT_QTY"] = 0;
                newRow["RESNQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = new TextRange(UcAssyProduction.TextRemark.Document.ContentStart, UcAssyProduction.TextRemark.Document.ContentEnd).Text;
                //newRow["WRK_USERID"] = UcAssyShift.TextWorker.Tag;
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

                        // ERP 불량 비율 Rate 저장
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
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

        private double GetLotINPUTQTY(string sLotID)
        {
            double dInputQty = 0;
            if (DgDefectDetail != null && DgDefectDetail.Rows.Count == 3)
            {
                if (DgDefectDetail.Rows.Count > 0)
                {
                    dInputQty = Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[2].DataItem, "OUTPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[2].DataItem, "OUTPUTQTY")));
                }
            }
            return dInputQty;
        }

        private bool PERMIT_RATE_input(string sLotID, string sWipSeq)
        {
            bool bFlag = false;
            try
            {
                DgDefect.EndEdit();
                //양품 수량을 가지고 온다.
                //double goodQty = GetLotGoodQty(sLotID);
                ////////생산수량을 가지고 온다
                double goodQty = GetLotINPUTQTY(sLotID);

                DataTable data = new DataTable();
                data.Columns.Add("LOTID", typeof(string));
                data.Columns.Add("WIPSEQ", typeof(string));
                data.Columns.Add("ACTID", typeof(string));
                data.Columns.Add("ACTNAME", typeof(string));
                data.Columns.Add("RESNCODE", typeof(string));
                data.Columns.Add("RESNNAME", typeof(string));
                data.Columns.Add("DFCT_CODE_DETL_NAME", typeof(string));
                data.Columns.Add("RESNQTY", typeof(string));
                data.Columns.Add("PERMIT_RATE", typeof(string));
                data.Columns.Add("OVER_QTY", typeof(string));
                data.Columns.Add("SPCL_RSNCODE", typeof(string));
                data.Columns.Add("SPCL_RSNCODE_NAME", typeof(string));
                data.Columns.Add("RESNNOTE", typeof(string));


                decimal dRate = 0;
                decimal dQty = 0;
                decimal dAllowQty = 0;
                decimal OverQty = 0;

                for (int j = 0; j < DgDefect.Rows.Count - DgDefect.BottomRows.Count; j++)
                {
                    // double dRate = 0;
                    dRate = Util.NVC_Decimal(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "PERMIT_RATE"));
                    //등록된 Rate가 0보다 큰것인 것만 적용
                    if (dRate > 0)
                    {
                        dQty = Util.NVC_Decimal(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "RESNQTY")); //여러개인 경우 어떻게?                        
                                                                                                                    //dAllowQty = Math.Truncate(goodQty * dRate / 100); //버림 
                        dAllowQty = Convert.ToDecimal(goodQty) * dRate / 100;
                        if (dAllowQty < dQty)
                        {
                            OverQty = dQty - dAllowQty;
                            OverQty = Math.Ceiling(OverQty); //소수점 첫자리 올림

                            DataRow newRow = data.NewRow();

                            newRow["LOTID"] = sLotID; //필수
                            newRow["WIPSEQ"] = sWipSeq; //필수
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "ACTID")); //필수
                            newRow["ACTNAME"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "ACTNAME")); //필수
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "RESNCODE")); //필수
                            newRow["RESNNAME"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "RESNNAME")); //필수
                            newRow["DFCT_CODE_DETL_NAME"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "DFCT_CODE_DETL_NAME"));


                            newRow["RESNQTY"] = dQty.ToString("G29"); //필수
                            newRow["PERMIT_RATE"] = dRate.ToString("0.00");  //필수 0제거 
                            newRow["OVER_QTY"] = OverQty.ToString("G29"); //(dQty - dAllowQty).ToString("0.000"); //소수점 3자리

                            newRow["SPCL_RSNCODE"] = "";
                            newRow["SPCL_RSNCODE_NAME"] = "";
                            newRow["RESNNOTE"] = "";
                            data.Rows.Add(newRow);
                        }
                    }
                }


                //등록 할 정보가 있으면 
                if (data.Rows.Count > 0)
                {

                    CMM_PERMIT_RATE popRermitRate = new CMM_PERMIT_RATE { FrameOperation = FrameOperation };
                    object[] parameters = new object[2];
                    parameters[0] = sLotID;
                    parameters[1] = data;
                    C1WindowExtension.SetParameters(popRermitRate, parameters);

                    popRermitRate.Closed += popupPermitRate_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popRermitRate.ShowModal()));

                    bFlag = true;
                }

                return bFlag;
                ///////////////////////////////////////////////                  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bFlag;
            }

        }

        private void popupPermitRate_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM_PERMIT_RATE popRermitRate = sender as CMM_PERMIT_RATE;
                if (popRermitRate != null && popRermitRate.DialogResult == MessageBoxResult.OK)
                {
                    _dtRet_Data.Clear();
                    _dtRet_Data = popRermitRate.PERMIT_RATE.Copy();
                    _sUserID = popRermitRate.UserID;
                    _sDepID = popRermitRate.DeptID;
                    _bInputErpRate = true;

                    //////////////////////////////
                    //확정 처리
                    Confirm_Real();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BR_PRD_REG_PERMIT_RATE_OVER_HIST()
        {
            try
            {
                DataTable lotTab = _dtRet_Data.DefaultView.ToTable(true, new string[] { "LOTID", "WIPSEQ" });
                string sLot = "";


                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataTable inRESN = indataSet.Tables.Add("IN_RESN");
                inRESN.Columns.Add("PERMIT_RATE", typeof(decimal));
                inRESN.Columns.Add("ACTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(decimal));
                inRESN.Columns.Add("OVER_QTY", typeof(decimal));
                inRESN.Columns.Add("REQ_USERID", typeof(string));
                inRESN.Columns.Add("REQ_DEPTID", typeof(string));
                inRESN.Columns.Add("DIFF_RSN_CODE", typeof(string));
                inRESN.Columns.Add("NOTE", typeof(string));

                for (int j = 0; j < lotTab.Rows.Count; j++)
                {
                    inTable.Rows.Clear();
                    inRESN.Rows.Clear();

                    sLot = lotTab.Rows[j]["LOTID"].ToString();

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = sLot;
                    newRow["WIPSEQ"] = lotTab.Rows[j]["WIPSEQ"].ToString();
                    inTable.Rows.Add(newRow);
                    newRow = null;


                    for (int i = 0; i < _dtRet_Data.Rows.Count; i++)
                    {
                        if (sLot.Equals(_dtRet_Data.Rows[i]["LOTID"].ToString()))
                        {
                            newRow = inRESN.NewRow();
                            newRow["PERMIT_RATE"] = Convert.ToDecimal(_dtRet_Data.Rows[i]["PERMIT_RATE"].ToString());
                            newRow["ACTID"] = _dtRet_Data.Rows[i]["ACTID"].ToString();
                            newRow["RESNCODE"] = _dtRet_Data.Rows[i]["RESNCODE"].ToString();
                            newRow["RESNQTY"] = _dtRet_Data.Rows[i]["RESNQTY"].ToString();
                            newRow["OVER_QTY"] = _dtRet_Data.Rows[i]["OVER_QTY"].ToString();
                            newRow["REQ_USERID"] = _sUserID;
                            newRow["REQ_DEPTID"] = _sDepID;
                            newRow["DIFF_RSN_CODE"] = _dtRet_Data.Rows[i]["SPCL_RSNCODE"].ToString();
                            newRow["NOTE"] = _dtRet_Data.Rows[i]["RESNNOTE"].ToString();
                            inRESN.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PERMIT_RATE_OVER_HIST", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            //Util.AlertInfo("정상 처리 되었습니다.");
                            //Util.MessageInfo("SFU1275");
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    }, indataSet);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        private void SaveDefectBeforeConfirm()
        {
            int selectedIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            string selectedLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            string selectedWipSeq = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();

            UcAssyProduction.SaveDefectBeforeConfirm();

            if (string.Equals(_processCode, Process.WINDING))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                if (_isSmallType)
                {
                    UcAssyProduction.GetProductCellList(UcAssyProduction.DgProductionCellWinding, false);
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
                GetOutTray();
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
            //추가
            dr["PROD_VER_CODE"] = Util.NVC(UcAssyProduction.TxtProdVerCode.Text);
            dr["SHIFT"] = UcAssyShift.TextShift.Tag;
            dr["WIPNOTE"] = new TextRange(UcAssyProduction.TextRemark.Document.ContentStart, UcAssyProduction.TextRemark.Document.ContentEnd).Text;
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

        private void CreateBox()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CREATE_BOX_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));
                inDataTable.Columns.Add("SHIFT", typeof(string));
                inDataTable.Columns.Add("WRK_USERID", typeof(string));
                inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inDataTable.Columns.Add("CELL_QTY", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                dr["USERID"] = LoginInfo.USERID;
                dr["WIPNOTE"] = UcAssyProduction.TextWipNote.Text;
                dr["SHIFT"] = UcAssyShift.TextShift.Tag;
                dr["WRK_USERID"] = UcAssyShift.TextWorker.Tag;
                dr["WRK_USER_NAME"] = UcAssyShift.TextWorker.Text;
                dr["CELL_QTY"] = UcAssyProduction.NumericBoxQty.Value;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                    UcAssyProduction.TextWipNote.Text = string.Empty;
                    UcAssyProduction.NumericBoxQty.Value = 0;
                    UcAssyProduction.GetBoxList();

                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();
                // 원각 초소형 공통사용 BizRule
                const string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_WS";
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
                // 원각 초소형 공통 BizRule
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
            //if (grdWorkOrder.Children.Count == 0)
            //{
            //    Ucworkorder.FrameOperation = FrameOperation;
            //    Ucworkorder._UCParent = this;
            //    grdWorkOrder.Children.Add(Ucworkorder);
            //}
        }

        public void GetWorkOrder()
        {
            //if (Ucworkorder == null)
            //    return;

            //Ucworkorder.EQPTSEGMENT = ComboEquipmentSegment.SelectedValue.GetString();
            //Ucworkorder.EQPTID = ComboEquipment.SelectedValue.GetString();
            //Ucworkorder.PROCID = _processCode;

            //Ucworkorder.GetWorkOrder();
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

                const string wipState = "PROC,EQPT_END";
                ClearControls();
                //UcAssyProduction.ClearProductionControl();

                ShowLoadingIndicator();

                const string bizRuleName = "DA_PRD_SEL_PRODUCTLOT_WS";
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
                        if (string.Equals(_processCode, Process.WASHING))
                        {
                            DgProductLot.ItemsSource = DataTableConverter.Convert(result);
                        }
                        else
                        {
                            Util.GridSetData(DgProductLot, result, FrameOperation, true);
                        }

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
                    //parameters[8] = Ucworkorder.GetSelectWorkOrderRow();
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
                //GetWorkOrder();
                GetProductLot();
            }
            grdMain.Children.Remove(popup);

            if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                _dispatcherTimer.Start();
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
                Util.gridClear(dgOut);
                txtTrayID.Text = string.Empty;
                UcAssyProduction.ClearProductionControl();

                //특별관리 화면 display 초기화 시작
                chkOutTraySpl.IsChecked = false;
                txtOutTrayReamrk.Text = string.Empty;

                if (cboOutTraySplReason.Items.Count > 0) {
                    cboOutTraySplReason.SelectedIndex = 0;
                }

                if ((bool)chkOutTraySpl.IsChecked)
                {
                    grdSpecialTrayMode.Visibility = Visibility.Visible;
                    ColorAnimationInSpecialTray();
                }
                else
                {
                    grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                }
                //특별관리 화면 display 초기화 끝

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
            UcAssyProduction.ClearProductionControl();
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

                UcAssyProduction?.ChangeEquipment(ComboEquipment.SelectedValue.GetString(), ComboEquipmentSegment.SelectedValue.GetString());

                string lotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                string wipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                string ProdVerCode = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "PROD_VER_CODE"));

                UcAssyProduction.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyProduction.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyProduction.ProcessCode = _processCode;
                UcAssyProduction.ProdWorkOrderId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WOID"));
                UcAssyProduction.ProdWorkOrderDetailId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WO_DETL_ID"));
                UcAssyProduction.ProdLotState = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSTAT"));
                UcAssyProduction.ProdLotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                UcAssyProduction.ProdWorkInProcessSequence = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                UcAssyProduction.ProdSelectedCheckRowIdx = iRow;

                // 투입영역 데이터 조회
                GetInputSelectInfo(iRow);

                // 생산영역의 데이터 조회
                SetDefectDetail();
                GetDefectInfo(lotId, wipSeq);

                UcAssyProduction.SetResultDetailControl(DgProductLot.Rows[iRow].DataItem);
                GetOutTraybyAsync();
                GetSpecialTrayInfo();

                _LOTID = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                _PRODID = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "PRODID"));

                //Permit Rate Added
                _WipSeq = wipSeq;

                _Product_Lev = GetProduct_Level();

                if (ProdVerCode == "" || ProdVerCode == null)
                {
                    DataTable versionDt = GetProcessVersion(_LOTID, _PRODID);
                    if (versionDt.Rows.Count > 0)
                    {
                        _VERSION = Util.NVC(versionDt.Rows[0]["PROD_VER_CODE"]);
                    }


                    UcAssyProduction.TxtProdVerCode.Text = _VERSION;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCurrInputLotList()
        {
            if (UcAssyProduction != null)
            {
                if (!CommonVerify.HasDataGridRow(DgProductLot))
                {
                    UcAssyProduction.ProdLotId = string.Empty;
                    UcAssyProduction.ProdWorkInProcessSequence = string.Empty;
                    UcAssyProduction.ProdWorkOrderId = string.Empty;
                    UcAssyProduction.ProdWorkOrderDetailId = string.Empty;
                    UcAssyProduction.ProdLotState = string.Empty;
                }
                UcAssyProduction.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyProduction.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyProduction.ProcessCode = _processCode;
                UcAssyProduction.GetCurrInList();
            }
        }

        private void GetInputSelectInfo(int row)
        {
            if (UcAssyProduction != null)
            {
                UcAssyProduction.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyProduction.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyProduction.ProdLotId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "LOTID").GetString();
                UcAssyProduction.ProdWorkInProcessSequence = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WIPSEQ").GetString();
                UcAssyProduction.ProdWorkOrderId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WOID").GetString();
                UcAssyProduction.ProdWorkOrderDetailId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WO_DETL_ID").GetString();
                UcAssyProduction.SearchAll();
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

        public void CalculateDefectQty()
        {
            Decimal totalCellQty = 0;
            if (CommonVerify.HasDataGridRow(dgOut))
            {
                DataTable dt = ((DataView)dgOut.ItemsSource).Table;
                totalCellQty = dt.AsEnumerable().Sum(s => s.Field<Decimal>("CELLQTY"));
            }

            SumDefectQty();
            if (CommonVerify.HasDataGridRow(DgDefectDetail))
            {
                if (DgDefectDetail.Rows.Count - DgDefectDetail.TopRows.Count > 0)
                {

                    double goodQty = (double)totalCellQty;
                    double defectQty = Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")));
                    //double outputQty = goodQty + defectQty;
                    double boxQty = GetBoxQty().GetDouble();
                    double outputQty = goodQty + defectQty + boxQty;

                    // 투입수량 계산이 필요 함. 투입수량 과 생산수량이 같은 경우 재투입(ALPHAQTY_P), 미기입불량(ALPHAQTY_M) 수량이 모두 0 임
                    //양품수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                    //생산수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                    //투입수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", outputQty);
                    //BOX수량
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "BOXQTY", boxQty);
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

        private Decimal GetBoxQty()
        {
            Decimal returnBoxQty = 0;

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
                return 0;

            const string bizRuleName = "DA_PRD_SEL_WAIT_BOX_LIST_WS";
            DataTable indataTable = new DataTable();
            indataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = indataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            indataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", indataTable);
            if (CommonVerify.HasTableRow(searchResult))
            {
                returnBoxQty = searchResult.AsEnumerable().Sum(s => s.Field<Decimal>("WIPQTY"));
            }
            else
            {
                returnBoxQty = 0;
            }
            return returnBoxQty;
        }

        public DataRow GetSelectWorkOrderRow()
        {
            //return Ucworkorder.GetSelectWorkOrderRow();
            return null;
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

        private bool ValidationTrayLocation(string buttonName)
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

                if (dtRslt != null && dtRslt.Rows.Count > 0)
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

                if (dtRslt != null && dtRslt.Rows.Count > 0)
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
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_P", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_M", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("GOODQTY", typeof(int));
            inDataTable.Columns.Add("DTL_DEFECT", typeof(int));
            inDataTable.Columns.Add("DTL_LOSS", typeof(int));
            inDataTable.Columns.Add("DTL_CHARGEPRD", typeof(int));
            inDataTable.Columns.Add("DEFECTQTY", typeof(int));
            inDataTable.Columns.Add("REINPUTQTY", typeof(int));
            inDataTable.Columns.Add("BOXQTY", typeof(int));

            DataRow dtRow = inDataTable.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["ALPHAQTY_P"] = 0;
            dtRow["ALPHAQTY_M"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;
            dtRow["REINPUTQTY"] = 0;
            dtRow["BOXQTY"] = 0;
            inDataTable.Rows.Add(dtRow);
            DgDefectDetail.ItemsSource = DataTableConverter.Convert(inDataTable);
        }

        private void SetComboBox()
        {
            CommonCombo combo = new CommonCombo();
            //원각 : CR , 초소형 : CS
            const string gubun = "CS";
            String[] sFilter = { LoginInfo.CFG_AREA_ID, gubun, _processCode };
            C1ComboBox[] cboLineChild = { ComboEquipment };
            combo.SetCombo(ComboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { _processCode };
            C1ComboBox[] cboEquipmentParent = { ComboEquipmentSegment };
            combo.SetCombo(ComboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);

            // 자동 조회 시간 Combo
            String[] sFilter4 = { "MOBILE_TRAY_INTERVAL" };
            combo.SetCombo(cboAutoSearchOut, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");

            if (cboAutoSearchOut?.Items != null && cboAutoSearchOut.Items.Count > 0)
                cboAutoSearchOut.SelectedIndex = 0;

            // 특별 TRAY  사유 Combo
            String[] sFilter3 = { LoginInfo.CFG_AREA_ID };
            combo.SetCombo(cboOutTraySplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SpecialResonCodebyAreaCode");

            if (cboOutTraySplReason?.Items != null && cboOutTraySplReason.Items.Count > 0)
                cboOutTraySplReason.SelectedIndex = 0;


            // 생산 반제품 조회 Timer
            if (_dispatcherTimer != null)
            {
                int iSec = 0;

                if (cboAutoSearchOut?.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                    iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                _dispatcherTimer.Tick += _dispatcherTimer_Tick;
                _dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
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

        private void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (grdTestFullTrayCreate != null && grdTestFullTrayCreate.Visibility == Visibility.Collapsed)
                {
                    grdTestFullTrayCreate.Visibility = Visibility.Visible;
                }
                else if (grdTestFullTrayCreate != null && grdTestFullTrayCreate.Visibility == Visibility.Visible)
                {
                    grdTestFullTrayCreate.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnTestFullTrayCreate_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정보생성 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1887", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (ComboEquipment?.SelectedValue == null)
                                return;

                            int iPrdRow = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK"); // 조립 lot row

                            if (iPrdRow < 0)
                                return;

                            if (txtTestFullTrayID.Text.Length != 10)
                            {
                                //Util.AlertInfo("생성할 Tray ID를 입력하세요.");
                                Util.MessageValidation("SFU1626");
                                return;
                            }

                            if (!Util.CheckDecimal(txtTestFullTrayCnt.Text, 0))
                            {
                                //Util.AlertInfo("생성 수량을 입력하세요.");
                                Util.MessageValidation("SFU1620");
                                return;
                            }

                            int iCnt = int.Parse(txtTestFullTrayCnt.Text); // 총 tray 생성 수

                            if (!Util.CheckDecimal(txtTestFullTrayID.Text.Substring(4, 6), 0))
                                return;

                            int iStartNum = int.Parse(txtTestFullTrayID.Text.Substring(4, 6)); // 시작 tray 넘버

                            int iTrayTotCnt = GetCstCellQtyInfo(txtTestFullTrayID.Text.Substring(0, 4));

                            if (iTrayTotCnt == 0) iTrayTotCnt = 50;   // tray 내 cell 수량.

                            string trayOutLot = "";    // tray id 에 대한 db key id
                            string sPkgLot = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iPrdRow].DataItem, "LOTID"));

                            ShowLoadingIndicator();

                            for (int i = 0; i < iCnt; i++)
                            {
                                var temptrayId = txtTestFullTrayID.Text.Substring(0, 4) + (iStartNum + i).ToString("000000"); // tray id

                                // Tray 생성.
                                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_CREATE_TRAY_CL();
                                DataTable inTable = indataSet.Tables["IN_EQP"];

                                DataRow newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                                newRow["PROD_LOTID"] = sPkgLot;
                                newRow["OUT_LOTID"] = ""; // TRAY MAPPING LOT
                                newRow["TRAYID"] = temptrayId;
                                newRow["WO_DETL_ID"] = null;
                                newRow["USERID"] = LoginInfo.USERID;

                                inTable.Rows.Add(newRow);

                                DataTable inSpcl = indataSet.Tables["IN_SPCL"];
                                newRow = inSpcl.NewRow();
                                newRow["SPCL_CST_GNRT_FLAG"] = "N";
                                newRow["SPCL_CST_NOTE"] = "";
                                newRow["SPCL_CST_RSNCODE"] = "";

                                inSpcl.Rows.Add(newRow);

                                try
                                {
                                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_OUT_LOT_CL", "IN_EQP,IN_INPUT,IN_SPCL", "OUT_LOT", indataSet);

                                    if (dsRslt != null && dsRslt.Tables.Contains("OUT_LOT") && dsRslt.Tables["OUT_LOT"].Rows.Count > 0)
                                    {
                                        trayOutLot = Util.NVC(dsRslt.Tables["OUT_LOT"].Rows[0]["OUT_LOTID"]);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(ex);
                                    return;
                                }


                                // Cell 생성.
                                string tempCellId = sPkgLot.Substring(3, 5) + sPkgLot.Substring(9, 1);
                                int iRow = 0;
                                int iLocation = 0;

                                // 해당 LOT의 MAX SEQ 조회.
                                DataTable inTmpTable = new DataTable();
                                inTmpTable.Columns.Add("LOTID", typeof(string));
                                inTmpTable.Columns.Add("OUT_LOTID", typeof(string));
                                inTmpTable.Columns.Add("TRAYID", typeof(string));
                                inTmpTable.Columns.Add("CELLID", typeof(string));

                                DataRow newTmpRow = inTmpTable.NewRow();
                                newTmpRow["LOTID"] = sPkgLot;

                                inTmpTable.Rows.Add(newTmpRow);

                                try
                                {

                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CELL_SEQ_IN_TRAY", "INDATA", "OUTDATA", inTmpTable);

                                    if (CommonVerify.HasTableRow(dtRslt))
                                        iRow = Util.NVC(dtRslt.Rows[0]["MAXSEQ"]).Equals("") ? 0 : Convert.ToInt32(Util.NVC(dtRslt.Rows[0]["MAXSEQ"]));
                                }
                                catch (Exception ex)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(ex);
                                    return;
                                }

                                indataSet = _bizDataSet.GetBR_PRD_REG_PUT_SUBLOT_IN_CST_CL();
                                inTable = indataSet.Tables["IN_EQP"];
                                newRow = inTable.NewRow();
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                                newRow["PROD_LOTID"] = sPkgLot;
                                newRow["OUT_LOTID"] = trayOutLot;
                                newRow["CSTID"] = temptrayId;
                                newRow["USERID"] = LoginInfo.USERID;

                                inTable.Rows.Add(newRow);

                                DataTable inSublotTable = indataSet.Tables["IN_CST"];

                                for (int j = 1; j <= iTrayTotCnt; j++)
                                {
                                    iLocation = iLocation + 1;

                                    newRow = inSublotTable.NewRow();
                                    newRow["SUBLOTID"] = tempCellId + (iRow + j).ToString("0000");
                                    newRow["CSTSLOT"] = iLocation.ToString();

                                    inSublotTable.Rows.Add(newRow);
                                }

                                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PUT_SUBLOT_IN_CST_CL", "IN_EQP,IN_CST", null, indataSet);
                            }

                            GetOutTraybyAsync();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void btnTestFullTrayAllConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("일괄확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1794", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            if (dgOut == null || dgOut.Rows.Count < 1)
                                return;

                            for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                                {
                                    try
                                    {
                                        DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_CONFIRM_CL();
                                        DataTable inTable = indataSet.Tables["IN_EQP"];
                                        DataRow newRow = inTable.NewRow();
                                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                                        newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                                        newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
                                        newRow["USERID"] = LoginInfo.USERID;
                                        inTable.Rows.Add(newRow);

                                        DataTable inCst = indataSet.Tables["IN_CST"];
                                        newRow = inCst.NewRow();
                                        newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "OUT_LOTID"));
                                        newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CELLQTY")));
                                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "TRAYID"));
                                        newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "SPECIALYN"));
                                        newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "SPECIALDESC"));
                                        newRow["SPCL_CST_RSNCODE"] = "";

                                        inCst.Rows.Add(newRow);

                                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_OUT_LOT_CL", "IN_EQP,IN_CST", null, indataSet);
                                    }
                                    catch (Exception ex)
                                    {
                                        Util.MessageException(ex);
                                    }
                                }
                            }

                            GetOutTraybyAsync();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetOutTraybyAsync();
        }

        private void btnTraySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTraySearch()) return;
            _dispatcherTimer?.Stop();
            CMM_ASSY_TRAY_INFO popTraySearch = new CMM_ASSY_TRAY_INFO { FrameOperation = FrameOperation };
            object[] parameters = new object[2];
            parameters[0] = _processCode;
            parameters[1] = ComboEquipmentSegment.SelectedValue;

            C1WindowExtension.SetParameters(popTraySearch, parameters);
            popTraySearch.Closed += new EventHandler(popTraySearch_Closed);

            grdMain.Children.Add(popTraySearch);
            popTraySearch.BringToFront();
        }

        private void popTraySearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_TRAY_INFO pop = sender as CMM_ASSY_TRAY_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
            if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                _dispatcherTimer.Start();
            grdMain.Children.Remove(pop);
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!ValidationCreateTray()) return;
                    CreateTray();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayCreate()) return;

            _dispatcherTimer?.Stop();
            CMM_ASSY_TRAY_CREATE popTrayCreate = new CMM_ASSY_TRAY_CREATE {FrameOperation = FrameOperation};

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot,"CHK");
            object[] parameters = new object[3];
            parameters[0] = ComboEquipment.SelectedValue.GetString();
            parameters[1] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[2] = string.Empty;
            C1WindowExtension.SetParameters(popTrayCreate, parameters);
            popTrayCreate.Closed += new EventHandler(popTrayCreate_Closed);

            //this.Dispatcher.BeginInvoke(new Action(() => popTrayCreate.ShowModal()));
            grdMain.Children.Add(popTrayCreate);
            popTrayCreate.BringToFront();
        }

        private void popTrayCreate_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_TRAY_CREATE pop = sender as CMM_ASSY_TRAY_CREATE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
            grdMain.Children.Remove(pop);

            if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                _dispatcherTimer.Start();
            
        }

        private void btnOutDel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayDelete()) return;
            try
            {
                _dispatcherTimer?.Stop();

                string messageCode = "SFU1230";
                double dCellQty = 0;

                string cellQty = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[_util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "CELLQTY"));

                if (!string.IsNullOrEmpty(cellQty))
                    double.TryParse(cellQty, out dCellQty);

                if (!string.IsNullOrEmpty(cellQty) && !dCellQty.Equals(0))
                {
                    //sMsg = "Cell 수량이 존재 합니다.\n그래도 삭제 하시겠습니까?";
                    messageCode = "SFU1320";
                }

                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DeleteTray();
                    }
                    else
                    {
                        if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                            _dispatcherTimer.Start();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirmCancel()) return;

            try
            {
                _dispatcherTimer?.Stop();

                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmCancelTray();
                    }
                    else
                    {
                        if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                            _dispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirm()) return;

            try
            {
                _dispatcherTimer?.Stop();

                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (_Product_Lev == "B")
                        {
                            ConfirmTray_BottonCell();
                        }
                        else
                        {
                            ConfirmTray();
                        }

                    }
                    else
                    {
                        if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                            _dispatcherTimer.Start();
                    }
                });
                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutCell_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellChange()) return;

            _dispatcherTimer?.Stop();
            CMM_WASHING_CSH_CELL_INFO popCellInfo = new CMM_WASHING_CSH_CELL_INFO { FrameOperation = FrameOperation };
            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

            object[] parameters = new object[7];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "TRAYID"));
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "OUT_LOTID"));
            parameters[3] = ComboEquipment.SelectedValue;
            //상태가 미확정일 경우에만 저장/삭제가 가능하다 - 그 외 나머지 상태는 조회만 가능
            if (Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")) != "WAIT")
            {
                //Read 모드
                parameters[4] = "R";
            }
            else
            {   //Write 모드
                parameters[4] = "W";
            }
            parameters[5] = "";
            parameters[6] = _Product_Lev;

            C1WindowExtension.SetParameters(popCellInfo, parameters);
            popCellInfo.Closed += new EventHandler(popCellInfo_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popCellInfo.ShowModal()));
            //grdMain.Children.Add(popCellInfo);
            //popCellInfo.BringToFront();
        }

        private void popCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_WASHING_CSH_CELL_INFO pop = sender as CMM_WASHING_CSH_CELL_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
            grdMain.Children.Remove(pop);

            if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                _dispatcherTimer.Start();

        }

        private void btnOutSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTraySave()) return;

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveTray();
                }
            });
        }

        private void dgOut_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgOut_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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
                            var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                            if (checkBox != null && ((dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                                      dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                                      checkBox.IsChecked.HasValue &&
                                                      !(bool)checkBox.IsChecked)))
                            {

                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk.IsChecked = true;

                                // 이전 값 저장.
                                _previewValues.PreviewTray = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));
                                SetOutTrayButtonEnable(e.Cell.Row);

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

                                //if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                //{
                                //    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                //    chk.IsChecked = true;

                                //    // 이전 값 저장.
                                //    _previewValues.PreviewTray = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));
                                //    SetOutTrayButtonEnable(e.Cell.Row);

                                //    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                //    {
                                //        if (e.Cell.Row.Index != idx)
                                //        {
                                //            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                //                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                //                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                //            {
                                //                var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                //                if (box !=null)
                                //                    box.IsChecked = false;
                                //            }
                                //            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                //        }
                                //    }
                                //}
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
                                    _previewValues.PreviewTray = string.Empty;

                                    // 확정 시 저장, 삭제 버튼 비활성화
                                    SetOutTrayButtonEnable(null);

                                    //if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    //{
                                    //    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    //    chk.IsChecked = false;
                                    //    _previewValues.PreviewTray = string.Empty;

                                    //    // 확정 시 저장, 삭제 버튼 비활성화
                                    //    SetOutTrayButtonEnable(null);
                                    //}
                                }
                            }
                            break;
                    }

                    if (dgOut.CurrentCell != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                    else if (dgOut.Rows.Count > 0)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);

                }
            }));
        }

        private void dgOut_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals(borderWait.Tag))
                    {
                        e.Cell.Presenter.Background = borderWait.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals(borderAssyOut.Tag))
                    {
                        e.Cell.Presenter.Background = borderAssyOut.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals(borderFormIn.Tag))
                    {
                        e.Cell.Presenter.Background = borderFormIn.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }

                    if (e.Cell.Column.Index == dgOut.Columns["TRAYID"].Index && string.Equals(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TRAYID").GetString(), "NOREAD"))
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Violet);
                    }
                }
            }));
        }

        private void dgOut_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgOut_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                DataRowView drv = e.Row.DataItem as DataRowView;
                if (e.Column.Name == "CHK") e.Cancel = true;

                //if (e?.Row?.DataItem == null || e.Column == null)
                //    return;

                //if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN") ||
                //    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                //{
                //    e.Cancel = true;
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOut_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgOut.GetCellFromPoint(pnt);

            if (cell != null)
            {
                int idx = cell.Row.Index;
                string moveStateCode = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE"));
                string checkFlag = DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "CHK").GetString();


                if (string.Equals(moveStateCode, "ASSY_OUT"))
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (checkFlag == "0")
                        {
                            DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", idx == i);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                        }
                    }
                    SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
                }

                else if (string.Equals(moveStateCode, "FORM_IN"))
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (checkFlag == "0")
                        {
                            DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", idx == i);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                        }
                    }
                    SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
                }
                else if (string.Equals(moveStateCode, "WAIT"))
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        string code = DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "FORM_MOVE_STAT_CODE").GetString();

                        if (checkFlag == "0")
                        {
                            if (code != "WAIT")
                            {
                                DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                            }
                            else
                            {
                                if (idx == i)
                                    DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", true);
                            }

                        }
                        else
                        {
                            if (idx == i)
                                DataTableConverter.SetValue(dgOut.Rows[i].DataItem, "CHK", false);
                        }
                    }
                    SetOutTrayButtonEnable(checkFlag == "0" ? cell.Row : null);
                }

            }

        }

        private void chkOutTraySpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutTrayReamrk != null)
            {
                txtOutTrayReamrk.Text = string.Empty;
            }
        }

        private void btnOutTraySplSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSpecialTraySave()) return;

            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSpecialTray();
                }
            });
        }

        private void rdoTraceUse_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOut == null)
                return;

            if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                dgOut.Columns["CELLQTY"].IsReadOnly = true;
            else
                dgOut.Columns["CELLQTY"].IsReadOnly = false;
        }

        private void rdoTraceNotUse_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOut == null)
                return;

            if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                dgOut.Columns["CELLQTY"].IsReadOnly = false;
            else
                dgOut.Columns["CELLQTY"].IsReadOnly = true;
        }

        private void cboAutoSearchOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOut?.SelectedValue != null && !cboAutoSearchOut.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOut.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherTimer.Start();

                    if (_isAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        if (cboAutoSearchOut != null)
                            Util.MessageInfo("SFU1605", cboAutoSearchOut.SelectedValue.GetString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1 || !CommonVerify.HasDataGridRow(DgProductLot)) return;
                    GetOutTraybyAsync();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private int GetCstCellQtyInfo(string trayType)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_CST_TYPE";
                int iCnt = 0;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CST_INFO_CL();
                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID.Equals("S2") ? "A2" : LoginInfo.CFG_AREA_ID; // LNS는 2동 정보로 처리.
                newRow["CST_TYPE_CODE"] = trayType;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    iCnt = int.Parse(Util.NVC(dtResult.Rows[0]["CST_CELL_QTY"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private void GetOutTray()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WS";
                // Tray 관련 버튼 처리.
                SetOutTrayButtonEnable(null);

                if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = ComboEquipmentSegment.SelectedValue.GetString();
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                //dgOut.ItemsSource = DataTableConverter.Convert(searchResult);
                Util.GridSetData(dgOut, searchResult, FrameOperation, true);
                CalculateDefectQty();

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");

                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                var dataGridComboBoxColumn = dgOut.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dt.Copy());


                if (!_previewValues.PreviewTray.Equals(""))
                {
                    int idx = _util.GetDataGridRowIndex(dgOut, "OUT_LOTID", _previewValues.PreviewTray);

                    if (idx >= 0)
                    {
                        DataTableConverter.SetValue(dgOut.Rows[idx].DataItem, "CHK", true);

                        dgOut.ScrollIntoView(idx, dgOut.Columns["CHK"].Index);

                        // Tray 관련 버튼 처리.
                        SetOutTrayButtonEnable(dgOut.Rows[idx]);

                        dgOut.CurrentCell = dgOut.GetCell(idx, dgOut.Columns.Count - 1);
                    }
                    else
                    {
                        if (dgOut.CurrentCell != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                        else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                    }
                }
                else
                {
                    if (dgOut.CurrentCell != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                    else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                        dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutTraybyAsync()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WS";
                // Tray 관련 버튼 처리.
                SetOutTrayButtonEnable(null);

                if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
                    return;

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem,"LOTID"));
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = ComboEquipmentSegment.SelectedValue.GetString();
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["TRAYID"] = null; // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    HiddenLoadingIndicator();
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    Util.GridSetData(dgOut, result, FrameOperation, true);
                    CalculateDefectQty();

                    //특별TRAY 콤보
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CODE");
                    dt.Columns.Add("NAME");

                    dt.Rows.Add("N", "N");
                    dt.Rows.Add("Y", "Y");

                    var dataGridComboBoxColumn = dgOut.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn;
                    if (dataGridComboBoxColumn != null)
                        dataGridComboBoxColumn.ItemsSource = DataTableConverter.Convert(dt.Copy());


                    if (!_previewValues.PreviewTray.Equals(""))
                    {
                        int idx = _util.GetDataGridRowIndex(dgOut, "OUT_LOTID", _previewValues.PreviewTray);

                        if (idx >= 0)
                        {
                            DataTableConverter.SetValue(dgOut.Rows[idx].DataItem, "CHK", true);
                            dgOut.ScrollIntoView(idx, dgOut.Columns["CHK"].Index);

                            // Tray 관련 버튼 처리.
                            SetOutTrayButtonEnable(dgOut.Rows[idx]);
                            dgOut.CurrentCell = dgOut.GetCell(idx, dgOut.Columns.Count - 1);
                        }
                        else
                        {
                            if (dgOut.CurrentCell != null)
                                dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                            else if (dgOut.Rows.Count > 0 &&
                                     dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                                dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                        }
                    }
                    else
                    {
                        if (dgOut.CurrentCell != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.CurrentCell.Row.Index, dgOut.Columns.Count - 1);
                        else if (dgOut.Rows.Count > 0 && dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1) != null)
                            dgOut.CurrentCell = dgOut.GetCell(dgOut.Rows.Count, dgOut.Columns.Count - 1);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteTray()
        {
            try
            {
                ShowLoadingIndicator();

                // 원각/ 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_DELETE_OUT_LOT_WS";

                DataTable indataTable = _bizDataSet.GetBR_PRD_REG_DELETE_OUT_LOT_WS();

                int row = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");
                DataRow selectedWorkOrderRow = GetSelectWorkOrderRow();

                DataRow newRow = indataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "LOTID").GetString();
                newRow["OUT_LOTID"] = DataTableConverter.GetValue(dgOut.Rows[_util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "OUT_LOTID").GetString();
                newRow["TRAYID"] = DataTableConverter.GetValue(dgOut.Rows[_util.GetDataGridCheckFirstRowIndex(dgOut, "CHK")].DataItem, "TRAYID").GetString(); 
                newRow["WO_DETL_ID"] = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WO_DETL_ID").GetString();
                newRow["USERID"] = LoginInfo.USERID;

                indataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "IN_EQP", null, indataTable, (searchResult, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(exception);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        //ClearCollectControl();
                        //GetWorkOrder();
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                            _dispatcherTimer.Start();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ConfirmCancelTray()
        {
            try
            {
                ShowLoadingIndicator();
                // 원각/ 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS();

                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = indataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                indataTable.Rows.Add(newRow);

                int iRow = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

                DataTable inCstTable = indataSet.Tables["IN_CST"];
                newRow = inCstTable.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[iRow].DataItem, "OUT_LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[iRow].DataItem, "TRAYID"));
                inCstTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", null, (searchResult, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }
                        //Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                            _dispatcherTimer.Start();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ConfirmTray()
        {
            try
            {
                string bizRuleName = string.Empty;
                bizRuleName = "BR_PRD_REG_END_OUT_LOT_WSS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_ALL_OUT_WS();
                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataTable inCstTable = indataSet.Tables["IN_CST"];
                int rowidx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;
                    DataRow dr = indataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = DataTableConverter.GetValue(DgProductLot.Rows[rowidx].DataItem, "LOTID").GetString();
                    dr["EQPT_LOTID"] = string.Empty;
                    indataTable.Rows.Add(dr);

                    DataRow newRow = inCstTable.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "TRAYID"));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CELLQTY")).Equals("") ? 0 : Util.NVC(DataTableConverter.GetValue(row.DataItem, "CELLQTY")).GetDecimal();
                    inCstTable.Rows.Add(newRow);
                    string xmlText = indataSet.GetXml();
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP,IN_CST", null, indataSet);
                    indataTable.Rows.Remove(dr);
                    inCstTable.Rows.Remove(newRow);
                }
                HiddenLoadingIndicator();
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));
                if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                    _dispatcherTimer.Start();

                GetProductLot();

                /*
                ShowLoadingIndicator();
                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");

                //원각 초소형 공통사용 BizRule
                const string bizRuleName = "BR_PRD_REG_END_OUT_LOT_WSS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_ALL_OUT_WS();

                DataTable indataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = indataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "LOTID");
                newRow["EQPT_LOTID"] = string.Empty;
                indataTable.Rows.Add(newRow);

                DataTable inCstTable = indataSet.Tables["IN_CST"];
                newRow = inCstTable.NewRow();

                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "OUT_LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "TRAYID"));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")).Equals("") ? 0 : Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")).GetDecimal();
                inCstTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_CST", null, (searchResult, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(exception);
                            return;
                        }
                        //ClearCollectControl();
                        //Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                            _dispatcherTimer.Start();
                    }
                }, indataSet);
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2021-10-07 오화백 버튼셀
        private void ConfirmTray_BottonCell()
        {
            try
            {
                string bizRuleName = string.Empty;
                bizRuleName = "BR_PRD_REG_EQPT_END_OUT_LOT_WSB";
                //DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_ALL_OUT_WS();
                //DataTable indataTable = indataSet.Tables["IN_EQP"];

                DataSet indataSet = new DataSet();
                DataTable indataTable = indataSet.Tables.Add("IN_EQP");
                indataTable.Columns.Add("SRCTYPE", typeof(string));
                indataTable.Columns.Add("IFMODE", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("USERID", typeof(string));
                indataTable.Columns.Add("PROD_LOTID", typeof(string));
                indataTable.Columns.Add("EQPT_LOTID", typeof(string));
                indataTable.Columns.Add("OUT_LOTID", typeof(string));
                indataTable.Columns.Add("CSTID", typeof(string));
                indataTable.Columns.Add("OUTPUT_QTY", typeof(int));
                //DataTable inCstTable = indataSet.Tables["IN_CST"];
                int rowidx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

                foreach (C1.WPF.DataGrid.DataGridRow row in dgOut.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) != "1") continue;
                    DataRow dr = indataTable.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                    dr["USERID"] = LoginInfo.USERID;
                    dr["PROD_LOTID"] = DataTableConverter.GetValue(DgProductLot.Rows[rowidx].DataItem, "LOTID").GetString();
                    dr["EQPT_LOTID"] = string.Empty;
                    dr["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "OUT_LOTID"));
                    dr["CSTID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "TRAYID"));
                    dr["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CELLQTY")).Equals("") ? 0 : Util.NVC(DataTableConverter.GetValue(row.DataItem, "CELLQTY")).GetDecimal();
                    indataTable.Rows.Add(dr);

                    string xmlText = indataSet.GetXml();
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "IN_EQP", null, indataSet);
                    indataTable.Rows.Remove(dr);
                   
                }
                HiddenLoadingIndicator();
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1275"));
                if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                    _dispatcherTimer.Start();

                GetProductLot();

            
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SaveTray()
        {
            try
            {
                _dispatcherTimer?.Stop();
                dgOut.EndEdit();
                //원각 / 초소형 공통 BizRule
                const string bizRuleName = "BR_PRD_REG_UPD_OUT_LOT_WS";
                int rowIndex = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
                string specialyn = null;
                string specialDescription = null;
                string specialReasonCode = null;

                if (!ProcessCode.Equals(Process.WINDING))
                {
                    specialyn = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "SPECIALYN"));
                    specialDescription = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "SPECIALDESC"));
                    specialReasonCode = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "SPCL_RSNCODE"));

                    if (specialyn.Equals("Y"))
                    {
                        if (string.IsNullOrWhiteSpace(specialDescription))
                        {
                            //Util.Alert("특별관리내역을 입력하세요.");
                            Util.MessageValidation("SFU1990");
                            if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                                _dispatcherTimer.Start();
                            return;
                        }
                    }
                    else if (specialyn.Equals("N"))
                    {
                        if (!string.IsNullOrWhiteSpace(specialDescription))
                        {
                            //Util.Alert("특별관리내역을 삭제하세요.");
                            Util.MessageValidation("SFU1989");
                            if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                                _dispatcherTimer.Start();
                            return;
                        }
                    }
                }

                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_UPD_OUT_LOT_WS();
                DataTable inDataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "LOTID").GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                DataTable inLot = indataSet.Tables["IN_LOT"];
                DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                newRow = null;

                for (int i = 0; i < dgOut.Rows.Count - dgOut.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOut, "CHK", i)) continue;

                    // Tray 정보 DataTable             
                    newRow = inLot.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "TRAYID"));
                    if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                        newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")));

                    //newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")));

                    inLot.Rows.Add(newRow);

                    // 특별 Tray DataTable                
                    newRow = inSpcl.NewRow();
                    newRow["SPCL_CST_GNRT_FLAG"] = specialyn;
                    newRow["SPCL_CST_NOTE"] = specialDescription;
                    newRow["SPCL_CST_RSNCODE"] = specialReasonCode;

                    inSpcl.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_LOT,IN_SPCL", null, (searchResult, exception) =>
                {
                    try
                    {
                        Util.MessageInfo("SFU1275");
                        GetProductLot();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                            _dispatcherTimer.Start();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveSpecialTray()
        {
            try
            {
                string productLot = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
                if (String.IsNullOrEmpty(productLot))
                {
                    Util.MessageInfo("SFU1364");    //LOT ID가 선택되지 않았습니다.
                    return;
                }

                txtSpecialLotGradeCode.Text = string.Empty;

                const string bizRuleName = "BR_PRD_REG_EIOATTR_SPCL_CST_WSH";
                string sRsnCode = cboOutTraySplReason.SelectedValue?.ToString() ?? "";

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = Process.PACKAGING;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.ToString();
                newRow["SPCL_LOT_GNRT_FLAG"] = chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = txtOutTrayReamrk.Text;
                //newRow["SPCL_PROD_LOTID"] = chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked ? Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID")) : "";
                newRow["SPCL_PROD_LOTID"] = productLot;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (CommonVerify.HasTableRow(bizResult))
                        {
                            txtSpecialLotGradeCode.Text = bizResult.Rows[0]["SPCL_LOT_GR_CODE"].GetString();
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        // 특별 Tray 정보 조회.
                        GetSpecialTrayInfo();

                        if (chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked)
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Visible;
                            ColorAnimationInSpecialTray();
                        }
                        else
                        {
                            grdSpecialTrayMode.Visibility = Visibility.Collapsed;
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
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetTrayInfo(out string returnMessage, out string messageCode)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_WS";

                int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgOut, "CHK");

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PR_LOTID"] = DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "LOTID").GetString();
                indata["PROCID"] = ProcessCode;
                indata["EQSGID"] = ComboEquipmentSegment.SelectedValue.GetString();
                indata["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                indata["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "TRAYID"));
                indataTable.Rows.Add(indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", indataTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    {
                        returnMessage = "OK";
                        messageCode = "";
                    }
                    else
                    {
                        returnMessage = "NG";
                        //sMsg = "TRAY가 미확정 상태가 아닙니다.";
                        messageCode = "SFU1431";
                    }
                }
                else
                {
                    returnMessage = "NG";
                    //sMsg = "존재하지 않습니다.";
                    messageCode = "SFU2881";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnMessage = "EXCEPTION";
                messageCode = ex.Message;
            }
        }

        private void CreateTray()
        {
            try
            {
                _dispatcherTimer?.Stop();

                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_START_OUT_LOT_WSS";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("EQPT_LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                //inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                //DataTable incst = indataSet.Tables.Add("IN_CST");
                //incst.Columns.Add("CSTSLOT", typeof(string));
                //incst.Columns.Add("CSTSLOT_F", typeof(string));
                //incst.Columns.Add("SUBLOTID", typeof(string));

                DataTable inInputLot = indataSet.Tables.Add("IN_INPUT");
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputLot.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputLot.Columns.Add("INPUT_LOTID", typeof(string));
                inInputLot.Columns.Add("MTRLID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = ComboEquipment.SelectedValue;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "LOTID").GetString();
                dr["EQPT_LOTID"] = string.Empty;
                dr["CSTID"] = txtTrayID.Text.Trim();
                //dr["OUTPUT_QTY"] = 0;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_INPUT", "OUT_LOT", (bizResult, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                        _dispatcherTimer.Start();

                    //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    GetProductLot();
                    txtTrayID.Text = string.Empty;
                    txtTrayID.Focus();
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                if (_dispatcherTimer != null && _dispatcherTimer.Interval.TotalSeconds > 0)
                    _dispatcherTimer.Start();
                Util.MessageException(ex);
            }
        }

        public DataTable GetSpecialTrayInfo()
        {
            try
            {
                if (ComboEquipment?.SelectedValue == null) return null;

                if (DgProductLot == null || DgProductLot.Rows.Count <= 2)
                {
                    return null;
                }

                string productLot = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
                if (String.IsNullOrEmpty(productLot))
                {
                    Util.MessageInfo("SFU1364");    //LOT ID가 선택되지 않았습니다.
                    return null;
                }

                txtSpecialLotGradeCode.Text = string.Empty;
                string selectedProductLotCode = string.Empty;

                //const string bizRuleName = "DA_BAS_SEL_EIOATTR_SPCL_LOT_WS"; //C20180718_42539 로 LOT 단위로 특별관리 저장하도록 변경
                const string bizRuleName = "DA_BAS_SEL_WIPATTR_SPCL_LOT_WS";

                //DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                //DataRow newRow = inTable.NewRow();
                //newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                //inTable.Rows.Add(newRow);
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = productLot;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                    if (rowIndex >= 0)
                    {
                        selectedProductLotCode = DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID").GetString();
                    }

                    if (!string.IsNullOrEmpty(selectedProductLotCode))
                    {
                        if (string.Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]), "Y") &&
                            string.Equals(Util.NVC(dtResult.Rows[0]["SPCL_PROD_LOTID"]), selectedProductLotCode))
                        {
                            
                            chkOutTraySpl.IsChecked = true;
                            txtOutTrayReamrk.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);
                            txtSpecialLotGradeCode.Text = dtResult.Rows[0]["SPCL_LOT_GR_CODE"].GetString();
                        }
                        else
                        {
                            cboOutTraySplReason.SelectedIndex = 0;
                            chkOutTraySpl.IsChecked = false;
                            txtOutTrayReamrk.Text = string.Empty;
                            txtSpecialLotGradeCode.Text = string.Empty;
                        }
                    }
                    else
                    {
                        cboOutTraySplReason.SelectedIndex = 0;
                        chkOutTraySpl.IsChecked = false;
                        txtOutTrayReamrk.Text = string.Empty;
                        txtSpecialLotGradeCode.Text = string.Empty;
                    }
                    //chkOutTraySpl.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                    //txtOutTrayReamrk.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                    if (cboOutTraySplReason?.Items != null && cboOutTraySplReason.Items.Count > 0 && cboOutTraySplReason.Items.CurrentItem != null)
                    {
                        var dataRowView = cboOutTraySplReason.Items.CurrentItem as DataRowView;
                        DataView dtview = dataRowView?.DataView;
                        if (dtview?.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])) && string.Equals(Util.NVC(dtResult.Rows[0]["SPCL_PROD_LOTID"]), selectedProductLotCode))
                                {
                                    cboOutTraySplReason.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboOutTraySplReason.Items.Count > 0)
                                cboOutTraySplReason.SelectedIndex = 0;
                        }
                    }

                    //cboOutTraySplReason.SelectedValue = Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"]);

                    if (chkOutTraySpl.IsChecked != null && (bool)chkOutTraySpl.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    chkOutTraySpl.IsChecked = false;
                    txtOutTrayReamrk.Text = string.Empty;

                    if (cboOutTraySplReason.Items.Count > 0)
                        cboOutTraySplReason.SelectedIndex = 0;

                    if ((bool)chkOutTraySpl.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Collapsed;
                    }
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            try
            {
                if (dgRow != null)
                {
                    // 확정 시 저장, 삭제 버튼 비활성화
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN")) // 활성화입고
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = false;
                        btnOutConfirmCancel.IsEnabled = false;
                        btnOutConfirm.IsEnabled = false;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = false;
                    }
                    else
                    {
                        btnOutCreate.IsEnabled = true;
                        btnOutDel.IsEnabled = true;
                        btnOutConfirmCancel.IsEnabled = true;
                        btnOutConfirm.IsEnabled = true;
                        btnOutCell.IsEnabled = true;
                        btnOutSave.IsEnabled = true;
                    }
                }
                else
                {
                    btnOutCreate.IsEnabled = true;
                    btnOutDel.IsEnabled = true;
                    btnOutConfirmCancel.IsEnabled = true;
                    btnOutConfirm.IsEnabled = true;
                    btnOutCell.IsEnabled = true;
                    btnOutSave.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void ColorAnimationInSpecialTray()
        {
            recSpcTray.Fill = myAnimatedBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "myAnimatedBrush");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        private bool ValidationTraySearch()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return false;
            }
            return true;
        }

        private bool ValidationTrayCreate()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationTrayDelete()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationTrayConfirmCancel()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationTrayConfirm()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }


            //int iRow = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            int rowIndex = _util.GetDataGridFirstRowIndexByCheck(dgOut, "CHK");
            if (rowIndex < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            // 확정 여부 확인(활성화 이동 상태 코드 WAIT 가 아닌 경우)
            if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[rowIndex].DataItem, "CELLQTY")), out dTmp))
            {
                if (dTmp.Equals(0))
                {
                    //Util.Alert("수량이 0인 Tray는 확정할 수 없습니다.");
                    Util.MessageValidation("SFU1685");
                    return false;
                }
            }
            else
            {
                //Util.Alert("수량이 잘못되어 확정할 수 없습니다.");
                Util.MessageValidation("SFU1687");
                return false;
            }

            string returnMessage;
            string messageCode;

            // Tray 현재 작업중인지 여부 확인.
            GetTrayInfo(out returnMessage, out messageCode);

            if (returnMessage.Equals("NG"))
            {
                Util.MessageValidation(messageCode);
                return false;
            }
            else if (returnMessage.Equals("EXCEPTION"))
                return false;

            return true;
        }

        private bool ValidationTraySave()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOut, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOut.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            return true;
        }

        private bool ValidationCellChange()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgOut, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationSpecialTraySave()
        {
            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (chkOutTraySpl.IsChecked.HasValue && (bool)chkOutTraySpl.IsChecked)
            {
                if (cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return false;
                }

                if (txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return false;
                }
            }
            else
            {
                if (!txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
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

        private bool ValidationBoxCreate()
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

            if (string.IsNullOrEmpty(UcAssyShift.TextShift.Text))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            return true;
        }

        private bool ValidationCreateTray()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(txtTrayID.Text.Trim()) || txtTrayID.Text.Length != 10)
            {
                Util.MessageValidation("SFU3675");
                txtTrayID.SelectAll();
                return false;
            }

            bool chk = System.Text.RegularExpressions.Regex.IsMatch(txtTrayID.Text.ToUpper(), @"^[a-zA-Z0-9]+$");
            if (!chk)
            {
                //Util.Alert("{0}의 TRAY_ID 특수문자가 있습니다. 생성할 수 없습니다", txtTrayId.Text.ToUpper());
                Util.MessageValidation("SFU1298", txtTrayID.Text.ToUpper());
                txtTrayID.SelectAll();
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

        private DataTable GetProcessVersion(string sLotID, string sProdID)
        {
            // VERSION을 룰에 따라 가져옴
            DataTable dt = new DataTable();
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCSTATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow indata = inTable.NewRow();
                indata["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                indata["PROCID"] = _processCode;
                indata["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                indata["LOTID"] = sLotID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["PRODID"] = sProdID;
                inTable.Rows.Add(indata);

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_DEFAULT_ASSY", "INDATA", "RSLTDT", inTable);
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return dt;
        }

        //2021-10-07 오화백  버튼셀
        private string GetProduct_Level()
        {
            string confirmDate;

            const string bizRuleName = "DA_BAS_SEL_VW_PRODUCT_MODEL_INFO";
        
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("PRODID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["PRODID"] = _PRODID;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if(searchResult.Rows.Count > 0)
            {
                confirmDate = searchResult.Rows[0]["PRODUCT_LEVEL3_CODE"].ToString();
            }
            else
            {
                confirmDate = string.Empty;
            }
        
            return confirmDate;
        }


    }

}
