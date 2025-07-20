/*************************************************************************************
 Created Date : 2016.11.20
      Creator : JEONG JONGWON
   Decription : 원각형 조립 BaseForm
--------------------------------------------------------------------------------------
 [Change History]
   2016.11.20   JEONG JONGWON : Initial Created.
   2017.02.21   INS 정문교C   : Tray 관련 팝업 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcBaseAssy.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcBaseAssy : UcBase
    {
        #region Set Field

        Util _util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        // Get, Set
        string _procId;
        string _lotId;
        string _woId;
        string _lotState;
        bool _micro;

        // Tab 저장 여부 체크
        bool isChangeDefect = false;
        bool isChangeMaterial = false;
        bool isChangeProduct = false;

        // 생산반제품 Washing, 초소형 Winding, 초소형 Washing 이벤트 공유
        C1DataGrid _dgProdCellChange;
        TextBox _txOutTrayReamrkChange;
        CheckBox _ckOutTraySplChange;
        C1ComboBox _ckOutTraySplReasonChange;
        C1ComboBox _cbAutoSearchChange;

        private bool bSetAutoSelTime = false;
        private int nLoopCount = 0;
        private System.DateTime dtCaldate;

        public System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public bool MICRO
        {
            get { return _micro; }
            set { _micro = value; }
        }

        public string PROCID
        {
            get { return _procId; }
            set { _procId = value;
                ////SetButtonsEnabled();
                SetGrids();
                SetButtons();
                SetInitCheckBoxes();
                SetInitGrids();
                SetInitChange();
                SetTabItems();
                SetComboBox();
            }
        }

        public string LOTID
        {
            get { return _lotId; }
            set { _lotId = value;}
        }

        public string WOID  // PROC, EQP_END시 사용
        {
            get { return _woId; }
            set { _woId = value; }
        }

        public string LOTSTATE
        {
            get { return _lotState; }
            set { _lotState = value; }
        }

        #endregion

        #region Variable

        #region [ComboBox Variable]
        public C1ComboBox EQUIPMENT_COMBO { get; set; }
        public C1ComboBox EQUIPMENTSEGMENT_COMBO { get; set; }
        public C1ComboBox OUTTRAYSPLREASON_COMBO { get; set; }
        public C1ComboBox OUTTRAYSPLREASONWSM_COMBO { get; set; }
        public C1ComboBox AUTOSEARCHOUT_COMBO { get; set; }
        public C1ComboBox AUTOSEARCHOUTWNM_COMBO { get; set; }
        public C1ComboBox AUTOSEARCHOUTWSM_COMBO { get; set; }
        #endregion

        #region [Button Variable]
        public Button STARTBUTTON { get; set; }
        public Button PROD_ADD_BUTTON { get; set; }
        public TextBox PROD_PRESS_TEXT { get; set; }

        public Button HISTORYCARD { get; set; }

        // TRAY CELL BUTTON
        public Button CELL_CREATE_BUTTON { get; set; }
        public Button CELL_CREATEWNM_BUTTON { get; set; }
        public Button CELL_CREATEWSM_BUTTON { get; set; }
        public Button TRAY_SEARCH_BUTTON { get; set; }
        public Button TRAY_MOVE_BUTTON { get; set; }

        // 특별 TRAY BUTTON
        public Button OUTTRAY_SPL_BUTTON { get; set; }
        public Button OUTTRAY_SPLWSM_BUTTON { get; set; }

        public RadioButton TRACE_USE_WSM_RADIOBUTTON;

        #endregion

        #region [LotState Variable]
        public CheckBox CHECK_WAIT { get; set; }
        public CheckBox CHECK_RUN { get; set; }
        public CheckBox CHECK_END { get; set; }
        #endregion
        public CheckBox CHECK_OUTTRAYSPL { get; set; }
        public CheckBox CHECK_OUTTRAYSPLWSM { get; set; }

        #region [DataGrid Variable]
        public UC_WORKORDER WORKORDER { get; set; }
        public C1DataGrid PRODLOT_GRID { get; set; }
        public C1DataGrid LOTINFO_GRID { get; set; }
        public C1DataGrid DEFECT_GRID { get; set; }
        public C1DataGrid LOSS_GRID { get; set; }
        public C1DataGrid CHARGE_GRID { get; set; }
        public C1DataGrid INPUTMTRL_GRID { get; set; }
        public C1DataGrid INPUTPRD_GRID { get; set; }
        public C1DataGrid PRDCELL_GRID { get; set; }
        public C1DataGrid PRDCELLWNM_GRID { get; set; }
        public C1DataGrid PRDCELLWSM_GRID { get; set; }

        #endregion

        #region [Inner Variable]
        private double iDeffectSumQty = 0;
        #endregion

        #endregion

        #region Initialize
        public UcBaseAssy()
        {
            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            ////SetGrids();
            ////SetButtons();
            ////SetInitCheckBoxes();
            ////SetInitGrids();
        }

        private void DataCollectClearControls()
        {
            Util.gridClear(DEFECT_GRID);
            Util.gridClear(LOSS_GRID);
            Util.gridClear(CHARGE_GRID);
            Util.gridClear(INPUTMTRL_GRID);
            Util.gridClear(INPUTPRD_GRID);
            Util.gridClear(PRDCELL_GRID);
            Util.gridClear(PRDCELLWNM_GRID);
            Util.gridClear(PRDCELLWSM_GRID);
        }

        private void ResultClearControls()
        {
            /*
            Util.gridClear(LOTINFO_GRID);

            (grdResult.Children[0] as UcAssyResult).txtWorkOrder.Text = "";
            (grdResult.Children[0] as UcAssyResult).txtLotState.Text = "";
            (grdResult.Children[0] as UcAssyResult).txtWipQty.Value = 0;
            (grdResult.Children[0] as UcAssyResult).txtGoodQty.Value = 0;
            (grdResult.Children[0] as UcAssyResult).txtDefectQty.Value = 0;
            (grdResult.Children[0] as UcAssyResult).txtStartTime.Text = "";
            (grdResult.Children[0] as UcAssyResult).txtEndTime.Text = "";
            (grdResult.Children[0] as UcAssyResult).txtRemark.Document.Blocks.Clear();

            iDeffectSumQty = 0;

            isChangeDefect = false;
            isChangeMaterial = false;
            isChangeProduct = false;

            (grdDataCollect.Children[0] as UcAssyDataCollect).rdoTraceUse.IsChecked = true;
            (grdDataCollect.Children[0] as UcAssyDataCollect).rdoTraceNotUse.IsEnabled = false;

            TRACE_USE_WSM_RADIOBUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).rdoTraceUse;
            */

        }

        #endregion

        #region Set Control
        private void SetGrids()
        {
            /*
            WORKORDER = grdWorkOrder.Children[0] as UC_WORKORDER;
            PRODLOT_GRID = (grdProductLot.Children[0] as UcAssyProdLot).dgProductLot;
            LOTINFO_GRID = (grdResult.Children[0] as UcAssyResult).dgLotInfo;
            DEFECT_GRID = (grdDataCollect.Children[0] as UcAssyDataCollect).dgDefect;
            INPUTMTRL_GRID = (grdDataCollect.Children[0] as UcAssyDataCollect).dgInputMaterial;
            INPUTPRD_GRID = (grdDataCollect.Children[0] as UcAssyDataCollect).dgInputProduct;

            PRDCELL_GRID = (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCell;
            PRDCELLWNM_GRID = (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWNM;
            PRDCELLWSM_GRID = (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWSM;

            // Grid에서 필요 컬럼 Visibility
            (grdCommand.Children[0] as UcCommand).btnFinalCut.Visibility = Visibility.Collapsed;
            (grdCommand.Children[0] as UcCommand).btnBarcodeLabel.Visibility = Visibility.Collapsed;
            (grdCommand.Children[0] as UcCommand).btnCleanLot.Visibility = Visibility.Collapsed;

            (grdCommand.Children[0] as UcCommand).txtEndLotId.Visibility = Visibility.Collapsed;

            //(grdSearch.Children[0] as UcAssySearch).spSingleCoater.Visibility = Visibility.Collapsed;
            //(grdSearch.Children[0] as UcAssySearch).cboCoatType.Visibility = Visibility.Collapsed;

            // 공정진행 선 체크
            WORKORDER.chkProc.IsChecked = true;
            */
        }

        private void SetTabItems()
        {
            /*
            C1TabItem tabDefect = (grdDataCollect.Children[0] as UcAssyDataCollect).tiDefect;
            C1TabItem tabInputMaterial = (grdDataCollect.Children[0] as UcAssyDataCollect).tiInputMaterial;
            C1TabItem tabInputProduct = (grdDataCollect.Children[0] as UcAssyDataCollect).tiInputProduct;
            C1TabItem tabProdCell = (grdDataCollect.Children[0] as UcAssyDataCollect).tiProdCell;
            C1TabItem tabProdCellWNM = (grdDataCollect.Children[0] as UcAssyDataCollect).tiProdCellWNM;
            C1TabItem tabProdCellWSM = (grdDataCollect.Children[0] as UcAssyDataCollect).tiProdCellWSM;

            // 필요요소 Visibility
            tabProdCell.Visibility = Visibility.Collapsed;
            tabProdCellWNM.Visibility = Visibility.Collapsed;
            tabProdCellWSM.Visibility = Visibility.Collapsed;

            if (string.Equals(_procId, Process.WINDING))
            {
                if (_micro)
                    tabProdCellWNM.Visibility = Visibility.Visible;

                INPUTPRD_GRID.Columns[3].Visibility = Visibility.Collapsed; // RUNCARDID
            }
            else if (string.Equals(_procId, Process.WASHING))
            {
                if (_micro)
                    tabProdCellWSM.Visibility = Visibility.Visible;
                else
                    tabProdCell.Visibility = Visibility.Visible;
            }
            */
        }

        private void SetButtons()
        {
            /*
            STARTBUTTON = (grdCommand.Children[0] as UcCommand).btnStart;
            HISTORYCARD = (grdCommand.Children[0] as UcCommand).btnHistoryCard;

            PROD_ADD_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnPrdAdd;
            PROD_PRESS_TEXT = (grdDataCollect.Children[0] as UcAssyDataCollect).txtProductLotId;

            CELL_CREATE_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCreate;
            CELL_CREATEWNM_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCreateWNM;
            CELL_CREATEWSM_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCreateWSM;

            OUTTRAY_SPL_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutTraySplSave;
            OUTTRAY_SPLWSM_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutTraySplSaveWSM;

            TRAY_SEARCH_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnTraySearch;
            TRAY_MOVE_BUTTON = (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayMove;
            */
        }

        private void SetInitCheckBoxes()
        {
            /*
            //CHECK_WAIT = (grdSearch.Children[0] as UcAssySearch).chkWait;
            //CHECK_RUN = (grdSearch.Children[0] as UcAssySearch).chkRun;
            //CHECK_END = (grdSearch.Children[0] as UcAssySearch).chkEqpEnd;

            CHECK_WAIT.Visibility = Visibility.Collapsed;
            CHECK_RUN.IsChecked = true;
            CHECK_END.IsChecked = false;

            CHECK_OUTTRAYSPL = (grdDataCollect.Children[0] as UcAssyDataCollect).chkOutTraySpl;
            CHECK_OUTTRAYSPLWSM = (grdDataCollect.Children[0] as UcAssyDataCollect).chkOutTraySplWSM;

            // Set Event
            CHECK_WAIT.Checked += OnLotStateCheckBoxChecked;
            CHECK_RUN.Checked += OnLotStateCheckBoxChecked;
            CHECK_END.Checked += OnLotStateCheckBoxChecked;

            CHECK_OUTTRAYSPL.Click += OnOutTraySplCheckBoxClick;
            */
        }

        private void SetInitGrids()
        {
            // Search
            (grdSearch.Children[0] as UcAssySearch).btnSearch.Click += OnClickSearch;

            // Command
            (grdCommand.Children[0] as UcCommand).btnStart.Click += OnClickStart;
            (grdCommand.Children[0] as UcCommand).btnCancel.Click += OnClickStartCancel;
            (grdCommand.Children[0] as UcCommand).btnEqptEnd.Click += OnClickEqptEnd;
            (grdCommand.Children[0] as UcCommand).btnConfirm.Click += OnClickConfirm;
            (grdCommand.Children[0] as UcCommand).btnBarcodeLabel.Click += OnClickBarcode;
            (grdCommand.Children[0] as UcCommand).btnHistoryCard.Click += OnClickHistoryCard;

            // PRODUCT
            (grdProductLot.Children[0] as UcAssyProdLot).dgProductLot.CommittedEdit += OnProdLotGridCommittedEdit;

            // RESULT
            (grdResult.Children[0] as UcAssyResult).txtGoodQty.KeyDown += OnKeyPress;

            // SHIFT USER, WORKER 정보를 사용하기 위하여 BUTTON 이벤트 추가 [2017-02-24]
            (grdShift.Children[0] as UcShift).btnShift.Click += OnClickShift;

            /*
            // COLLECT
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgDefect.LoadedCellPresenter += OnLoadedGridCellPresenter;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgInputMaterial.BeginningEdit += OnDataGridBeginningEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgInputProduct.BeginningEdit += OnDataGridBeginningEdit;

            (grdDataCollect.Children[0] as UcAssyDataCollect).dgDefect.CommittedEdit += OnDataCollectGridCommittedEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgInputMaterial.CommittedEdit += OnDataCollectGridCommittedEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgInputProduct.CommittedEdit += OnDataCollectGridCommittedEdit;

            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCell.BeginningEdit += OnDataCollectProdGridBeginningEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCell.CommittedEdit += OnDataCollectProdGridCommittedEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCell.LoadedCellPresenter += OnDataCollectGridLoadedCellPresenter;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCell.UnloadedCellPresenter += OnDataCollectGridUnloadedCellPresenter;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWNM.BeginningEdit += OnDataCollectProdGridBeginningEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWNM.CommittedEdit += OnDataCollectProdGridCommittedEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWNM.LoadedCellPresenter += OnDataCollectGridLoadedCellPresenter;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWNM.UnloadedCellPresenter += OnDataCollectGridUnloadedCellPresenter;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWSM.BeginningEdit += OnDataCollectProdGridBeginningEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWSM.CommittedEdit += OnDataCollectProdGridCommittedEdit;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWSM.LoadedCellPresenter += OnDataCollectGridLoadedCellPresenter;
            (grdDataCollect.Children[0] as UcAssyDataCollect).dgProductionCellWSM.UnloadedCellPresenter += OnDataCollectGridUnloadedCellPresenter;

            (grdDataCollect.Children[0] as UcAssyDataCollect).btnDefectRefresh.Click += OnDefectSearch;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnDefectSave.Click += OnDefectSave;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnMatRefresh.Click += OnMaterialSearch;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnMatSave.Click += OnMaterialSave;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnMatDelete.Click += OnMaterialRemove;
            (grdDataCollect.Children[0] as UcAssyDataCollect).txtMatLotId.PreviewKeyDown += OnMaterialKeyDown;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnPrdRefresh.Click += OnHalfProdSearch;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnPrdSave.Click += OnHalfProdSave;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnPrdDelete.Click += OnHalfProdRemove;

            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemove.Click += OnTrayRemove;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWNM.Click += OnTrayRemove;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWSM.Click += OnTrayRemove;

            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancel.Click += OnTrayCancel;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancelWSM.Click += OnTrayCancel;

            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirm.Click += OnTrayConfirm;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWNM.Click += OnTrayConfirm;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWSM.Click += OnTrayConfirm;

            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSave.Click += OnOutSave;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWNM.Click += OnOutSave;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWSM.Click += OnOutSave;

            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutTraySplSave.Click += OnOutTraySplSave;
            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutTraySplSaveWSM.Click += OnOutTraySplSave;
            */

        }

        private void SetInitChange()
        {
            /*
            EQUIPMENT_COMBO = (grdSearch.Children[0] as UcAssySearch).cboEquipmentAssy;
            EQUIPMENTSEGMENT_COMBO = (grdSearch.Children[0] as UcAssySearch).cboEquipmentSegmentAssy;
            OUTTRAYSPLREASON_COMBO = (grdDataCollect.Children[0] as UcAssyDataCollect).cboOutTraySplReason;
            OUTTRAYSPLREASONWSM_COMBO = (grdDataCollect.Children[0] as UcAssyDataCollect).cboOutTraySplReasonWSM;

            AUTOSEARCHOUT_COMBO = (grdDataCollect.Children[0] as UcAssyDataCollect).cboAutoSearchOut;
            AUTOSEARCHOUTWNM_COMBO = (grdDataCollect.Children[0] as UcAssyDataCollect).cboAutoSearchOutWNM;
            AUTOSEARCHOUTWSM_COMBO = (grdDataCollect.Children[0] as UcAssyDataCollect).cboAutoSearchOutWSM;

            if (_procId.Equals(Process.WINDING))
            {
                _dgProdCellChange = PRDCELLWNM_GRID;                     // 초소형 Winding
                _cbAutoSearchChange = AUTOSEARCHOUTWNM_COMBO;
            }
            else if (_procId.Equals(Process.WASHING) && _micro)
            {
                _dgProdCellChange = PRDCELLWSM_GRID;                     // 초소형 Washing
                _txOutTrayReamrkChange = (grdDataCollect.Children[0] as UcAssyDataCollect).txtOutTrayReamrkWSM;
                _ckOutTraySplChange = CHECK_OUTTRAYSPLWSM;
                _ckOutTraySplReasonChange = OUTTRAYSPLREASONWSM_COMBO;
                _cbAutoSearchChange = AUTOSEARCHOUTWSM_COMBO;
            }
            else
            {
                _dgProdCellChange = PRDCELL_GRID;                        // Washing
                _txOutTrayReamrkChange = (grdDataCollect.Children[0] as UcAssyDataCollect).txtOutTrayReamrk;
                _ckOutTraySplChange = CHECK_OUTTRAYSPL;
                _ckOutTraySplReasonChange = OUTTRAYSPLREASON_COMBO;
                _cbAutoSearchChange = AUTOSEARCHOUT_COMBO;
            }
            */
        }

        private void SetComboBox()
        {
            /*
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboLineChild = { EQUIPMENT_COMBO };
            _combo.SetCombo(EQUIPMENTSEGMENT_COMBO, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { PROCID };
            C1ComboBox[] cboEquipmentParent = { EQUIPMENTSEGMENT_COMBO };
            _combo.SetCombo(EQUIPMENT_COMBO, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);

            // Winding인 경우 제외
            if (!_procId.Equals(Process.WINDING))
            {
                String[] sFilter3 = { "SPCL_RSNCODE" };
                _combo.SetCombo(_ckOutTraySplReasonChange, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");

                if (_ckOutTraySplReasonChange != null && _ckOutTraySplReasonChange.Items != null && _ckOutTraySplReasonChange.Items.Count > 0)
                    _ckOutTraySplReasonChange.SelectedIndex = 0;
            }

            // 자동 조회 시간 Combo
            String[] sFilter4 = { "SECOND_INTERVAL" };
            _combo.SetCombo(_cbAutoSearchChange, CommonCombo.ComboStatus.NA, sFilter: sFilter4, sCase: "COMMCODE");
            if (_cbAutoSearchChange != null && _cbAutoSearchChange.Items != null && _cbAutoSearchChange.Items.Count > 0)
                _cbAutoSearchChange.SelectedIndex = 0;

            (grdSearch.Children[0] as UcAssySearch).cboEquipmentAssy.SelectedValueChanged += OnoEquipmentSelectedValueChanged;
            (grdDataCollect.Children[0] as UcAssyDataCollect).cboAutoSearchOut.SelectedValueChanged += OnoAutoSearchOutSelectedValueChanged;
            (grdDataCollect.Children[0] as UcAssyDataCollect).cboAutoSearchOutWNM.SelectedValueChanged += OnoAutoSearchOutSelectedValueChanged;
            (grdDataCollect.Children[0] as UcAssyDataCollect).cboAutoSearchOutWSM.SelectedValueChanged += OnoAutoSearchOutSelectedValueChanged;
            */

        }

        private void SetButtonsEnabled()
        {
            // 이력카드 출력은 WINDING에서만 출력 한다.
            if (string.Equals(_procId, Process.WINDING))
                HISTORYCARD.Visibility = Visibility.Visible;

        }
        #endregion

        #region Event

        #region [폼 Load]
        private void UcBase_Loaded(object sender, RoutedEventArgs e)
        {
            //// FORM LOAD
            ////SetTabItems();
            ////SetComboBox();
            SetButtonsEnabled();

            if (PRODLOT_GRID.Rows.Count < 1)
            {
                if (EQUIPMENT_COMBO.SelectedValue != null && !EQUIPMENT_COMBO.SelectedValue.ToString().Equals("SELECT"))
                    OnoEquipmentSelectedValueChanged(null, null);
            }

            // 생산 반제품 조회 Timer Start.
            if ((_procId.Equals(Process.WINDING) && _micro == true) || _procId.Equals(Process.WASHING))
            {
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Start();

                    int iSec = 0;

                    if (_cbAutoSearchChange != null && _cbAutoSearchChange.SelectedValue != null && !_cbAutoSearchChange.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(_cbAutoSearchChange.SelectedValue.ToString());

                    dispatcherTimer.Tick += dispatcherTimer_Tick;
                    dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                }
            }
        }
        #endregion

        #region [설비 콤보]
        private void OnoEquipmentSelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (EQUIPMENTSEGMENT_COMBO.IsMouseOver && (EQUIPMENT_COMBO.SelectedValue == null || EQUIPMENT_COMBO.SelectedValue.ToString().Equals("SELECT")))
                    return;

                if (nLoopCount > 0)
                {
                    nLoopCount = 0;
                    return;
                }

                if (EQUIPMENT_COMBO.SelectedValue == null || EQUIPMENT_COMBO.SelectedValue.ToString().Equals("SELECT"))
                    return;

                // 설비 선택 시 자동 조회 처리
                this.Dispatcher.BeginInvoke(new Action(() => OnClickSearch(null, null)));

                // 작업조, 작업자 조회
                GetEqptWrkInfo();

                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();

                nLoopCount++;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [대기, 진행, 설비완공 체크 박스]
        protected virtual void OnLotStateCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            // SEARCH WIPSTATE 순서
            CheckBox checkBox = sender as CheckBox;

            if (checkBox.IsChecked.HasValue)
            {
                ////if (string.Equals(checkBox.Name, CHECK_WAIT.Name) && checkBox.IsChecked == true)
                ////{
                ////    CHECK_RUN.IsChecked = false;
                ////    CHECK_END.IsChecked = false;
                ////}
                ////else
                ////{
                ////    CHECK_WAIT.IsChecked = false;
                ////}

            }
        }
        #endregion

        #region [조회]
        protected virtual void OnClickSearch(object sender, RoutedEventArgs e)
        {
            try
            {
                (grdProductLot.Children[0] as UcAssyProdLot).dgProductLot.CommittedEdit -= OnProdLotGridCommittedEdit;

                // SEARCH BUTTON CLICK
                if (EQUIPMENTSEGMENT_COMBO.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1223");      //라인을 선택하세요.
                    EQUIPMENTSEGMENT_COMBO.Focus();
                    return;
                }

                if (EQUIPMENT_COMBO.SelectedIndex < 1)
                {
                    Util.MessageValidation("SFU1673");     //설비를 선택하세요.
                    EQUIPMENT_COMBO.Focus();
                    return;
                }

                // WORKORDER, 작업대상 조회
                this.Refresh();

                // CELL CLICK ( 이렇게 안해주면 CHECKBOX 바로 SELECT 안됨???)
                if (PRODLOT_GRID.GetRowCount() > 0)
                    PRODLOT_GRID.CurrentCell = PRODLOT_GRID.GetCell(0, 1);
            }
            finally
            {
                (grdProductLot.Children[0] as UcAssyProdLot).dgProductLot.CommittedEdit += OnProdLotGridCommittedEdit;
            }
        }
        #endregion

        #region [작업시작] - 각 화면별 호출
        protected virtual void OnClickStart(object sender, RoutedEventArgs e)
        {
            // JOB START (각 화면별 호출)
        }
        #endregion

        #region [시작취소] - ???
        protected virtual void OnClickStartCancel(object sender, RoutedEventArgs e)
        {
            // JOB CANCEL
            this.StartCancelProcess();
        }
        #endregion

        #region [작업종료] - 설비완공
        protected virtual void OnClickEqptEnd(object sender, RoutedEventArgs e)
        {
            // JOB CANCEL
            this.EqpEndProcess();
        }
        #endregion

        #region [실적확정]
        protected virtual void OnClickConfirm(object sender, RoutedEventArgs e)
        {
            // JOB CONFIRM
            this.ConfirmProcess();
        }
        #endregion

        #region [바코드발행] - ???
        protected virtual void OnClickBarcode(object sender, RoutedEventArgs e)
        {
            Util.MessageValidation("SFU1720");      //아직 개발중인 기능입니다.
        }
        #endregion

        #region [이력카드] - Winding 화면에서 호출
        protected virtual void OnClickHistoryCard(object sender, RoutedEventArgs e)
        {
            // Winding 화면에서 호출
        }
        #endregion

        #region [작업대상]
        protected virtual void OnProdLotGridCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                (grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;

                C1DataGrid dg = sender as C1DataGrid;

                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                string sLotid = dt.Rows[e.Cell.Row.Index]["LOTID"].ToString();
                int nrow = e.Cell.Row.Index;

                foreach (DataRow row in dt.Rows)
                {
                    if (row["LOTID"].Equals(sLotid))
                        continue;
                    else
                        row["CHK"] = "0";
                }

                dt.AcceptChanges();
                Util.GridSetData(dg, dt, null, true);

                ResultClearControls();
                DataCollectClearControls();

                // SET GRID INFO
                if (GetLotInfo(PRODLOT_GRID.Rows[nrow].DataItem) == true)
                {
                    GetDefectList(DEFECT_GRID, "DEFECT_LOT", Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID")));
                    GetMaterialList();
                    GetHalfProductList();
                    GetProductCellList();

                    PRODLOT_GRID.SelectedIndex = nrow;
                }
                else
                {
                    WOID = "";
                    LOTID = "";
                    LOTSTATE = "";
                }
            }
            finally
            {
                (grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged += OndtpCaldate_SelectedDataTimeChanged;
            }

        }

        protected virtual void OnProdLotGridCurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            // 작업대상 CHECK CELL CLICK
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;

                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                // INIT DATA
                                WOID = "";
                                LOTID = "";
                                LOTSTATE = "";

                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int i = 0; i < dg.Rows.Count; i++)
                                    {
                                        if (i != e.Cell.Row.Index)
                                        {
                                            DataTableConverter.SetValue(dg.Rows[i].DataItem, "CHK", false);

                                            if (dg.GetCell(i, e.Cell.Column.Index).Presenter != null)
                                            {
                                                chk = dg.GetCell(i, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                chk.IsChecked = false;
                                            }
                                        }
                                    }

                                    // INIT
                                    ResultClearControls();
                                    DataCollectClearControls();

                                    // SET GRID INFO
                                    if (GetLotInfo(PRODLOT_GRID.Rows[e.Cell.Row.Index].DataItem) == true)
                                    {
                                        GetDefectList(DEFECT_GRID, "DEFECT_LOT", Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID")));
                                        GetMaterialList();
                                        GetHalfProductList();
                                        GetProductCellList();
                                    }

                                    PRODLOT_GRID.SelectedIndex = e.Cell.Row.Index;
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    // INIT 
                                    ResultClearControls();
                                    DataCollectClearControls();

                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;
                        }

                        if (PRODLOT_GRID.CurrentCell != null)
                            PRODLOT_GRID.CurrentCell = PRODLOT_GRID.GetCell(PRODLOT_GRID.CurrentCell.Row.Index, PRODLOT_GRID.Columns.Count - 1);
                        else if (PRODLOT_GRID.Rows.Count > 0)
                            PRODLOT_GRID.CurrentCell = PRODLOT_GRID.GetCell(PRODLOT_GRID.Rows.Count, PRODLOT_GRID.Columns.Count - 1);
                    }
                }
            }));
        }
        #endregion

        #region [실적상세]

        /// <summary>
        /// 작업조, 작업자 조회 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnClickShift(object sender, EventArgs e)
        {
            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1673"); 
                return;
            }

            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[3] = _procId;
                Parameters[4] = Util.NVC((grdShift.Children[0] as UcShift).txtShift.Tag);
                Parameters[5] = Util.NVC((grdShift.Children[0] as UcShift).txtWorker.Tag);
                Parameters[6] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(OnCloseShift);
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPopup);
                        wndPopup.BringToFront();
                        break;
                    }
                }
            }
        }

        protected virtual void OnCloseShift(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                (grdShift.Children[0] as UcShift).txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                (grdShift.Children[0] as UcShift).txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                (grdShift.Children[0] as UcShift).txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                (grdShift.Children[0] as UcShift).txtWorker.Tag = Util.NVC(wndPopup.USERID);
                (grdShift.Children[0] as UcShift).txtShiftDateTime.Text = Util.NVC(wndPopup.WRKSTRTTIME) + " ~ " + Util.NVC(wndPopup.WRKENDTTIME);
                (grdShift.Children[0] as UcShift).txtShiftStartTime.Text = Util.NVC(wndPopup.WRKSTRTTIME);
                (grdShift.Children[0] as UcShift).txtShiftEndTime.Text = Util.NVC(wndPopup.WRKENDTTIME);
            }
        }

        /// <summary>
        /// 양품수량
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                C1NumericBox txtBox = sender as C1NumericBox;

                if (txtBox != null)
                {
                    (grdResult.Children[0] as UcAssyResult).txtWipQty.Value = (grdResult.Children[0] as UcAssyResult).txtGoodQty.Value + 
                                                                              (grdResult.Children[0] as UcAssyResult).txtDefectQty.Value;
                }
            }
        }

        #endregion

        #region [작업일자]
        private void OndtpCaldate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
                else
                    dtPik.Focus();
            }));
        }
        #endregion

        #region [불량/Loss/물품청구, 투입자재, 투입반제품 공통]

        /// <summary>
        /// 투입자재, 투입반제품
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDataGridBeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            // INPUT VALUE CELL EDITING VALIDATION
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (!string.Equals(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "INPUT_SEQNO")), "0"))
            {
                if (!!string.Equals(e.Column.Name, "CHK") && string.Equals(e.Column.Name, "INPUTQTY"))
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// 불량/Loss/물품청구, 투입자재, 투입반제품
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDataCollectGridCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (LOTINFO_GRID.Rows.Count > LOTINFO_GRID.TopRows.Count)
            {
                C1DataGrid caller = sender as C1DataGrid;

                switch (caller.Name)
                {
                    case "dgDefect":
                        isChangeDefect = true;
                        break;

                    case "dgInputMaterial":
                        isChangeMaterial = true;

                        if (string.Equals(caller.Name, INPUTMTRL_GRID.Name))
                        {
                            if (e != null && string.Equals(e.Cell.Column.Name, "ITEMID") && !string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                            {
                                DataTable dt = GetMaterialList((grdResult.Children[0] as UcAssyResult).txtWorkOrder.Text, Util.NVC(e.Cell.Value));

                                if (dt.Rows.Count > 0)
                                {
                                    DataTableConverter.SetValue(caller.Rows[e.Cell.Row.Index].DataItem, "ITEMNAME", dt.Rows[0]["ITEMDESC"]);
                                    DataTableConverter.SetValue(caller.Rows[e.Cell.Row.Index].DataItem, "UNIT", dt.Rows[0]["UNIT"]);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(caller.Rows[e.Cell.Row.Index].DataItem, "ITEMNAME", "");
                                    DataTableConverter.SetValue(caller.Rows[e.Cell.Row.Index].DataItem, "UNIT", "");
                                }
                                caller.EndEditRow(true);
                            }
                        }
                        break;

                    case "dgInputProduct":
                        isChangeProduct = true;
                        break;
                }
            }
        }
        #endregion

        #region 불량/Loss/물품청구
        /// <summary>
        /// 불량 수량 합산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnLoadedGridCellPresenter(object sender, DataGridCellEventArgs e)
        {
            // COMMON DEFECT QTY SUMMARY
            if (e.Cell.Row.Type == DataGridRowType.Bottom)
            {
                StackPanel panel = e.Cell.Presenter.Content as StackPanel;
                ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                if (string.Equals(e.Cell.DataGrid.Name, DEFECT_GRID.Name))
                {
                    if (string.Equals(e.Cell.Column.Name, "RESNQTY") && Convert.ToDouble(presenter.Content) > 0)
                        iDeffectSumQty = Convert.ToDouble(Util.NVC(presenter.Content));

                }

                // Defect 수량 갱신
                if (e.Cell.Column.Index == 6)
                {
                    // 수량 Summary
                    (grdResult.Children[0] as UcAssyResult).txtDefectQty.Value = iDeffectSumQty;
                    (grdResult.Children[0] as UcAssyResult).txtWipQty.Value = (grdResult.Children[0] as UcAssyResult).txtGoodQty.Value +
                                                                              (grdResult.Children[0] as UcAssyResult).txtDefectQty.Value;
                }
            }
        }

        /// <summary>
        /// 조회 - Refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDefectSearch(object sender, EventArgs e)
        {
            // DEFECT REFRESH
            GetDefectList(DEFECT_GRID, "DEFECT_LOT", Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID")));
        }

        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDefectSave(object sender, EventArgs e)
        {
            if ((grdResult.Children[0] as UcAssyResult).txtGoodQty.Value == 0)
            {
                //양품수량이 0 이므로 확정할 수 없습니다.
                Util.MessageValidation("SFU1724");   
                return;
            }

            if ((iDeffectSumQty) > (grdResult.Children[0] as UcAssyResult).txtGoodQty.Value)
            {
                //전체 불량 수량이 양품 수량보다 클 수 없습니다.
                Util.MessageValidation("SFU1884");
                return;
            }

            // Set BizRule
            SetDefectList(DEFECT_GRID, "DEFECT_LOT", Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID")));
        }
        #endregion

        #region [투입자재]
        /// <summary>
        /// 텍스트 박스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMaterialKeyDown(object sender, KeyEventArgs e)
        {
            // ADD MATERIAL
            if (e.Key == Key.Enter)
            {
                TextBox txtBox = sender as TextBox;

                if (string.IsNullOrEmpty(txtBox.Text.Trim()))
                    return;

                for (int i = 0; i < INPUTMTRL_GRID.Rows.Count; i++)
                {
                    if (string.Equals(txtBox.Text, Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "LOTID"))))
                    {
                        //동일한 자재LOT ID가 이미 입력되어 있습니다.
                        Util.MessageValidation("SFU1507");
                        INPUTMTRL_GRID.SelectedIndex = i;
                        return;
                    }
                }

                List<string> bindSet = new List<string>(new string[] { "True", "0", "", "", "", txtBox.Text, "0", "", DateTime.Now.ToString() }); // SEQ는 현재 0으로 처리

                GridDataBinding(INPUTMTRL_GRID, bindSet, false);

                txtBox.Text = "";
            }
        }
        /// <summary>
        /// 조회 - Refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMaterialSearch(object sender, EventArgs e)
        {
            // INPUT MATERIAL REFRESH
            GetMaterialList();
        }

        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMaterialSave(object sender, EventArgs e)
        {
            // INPUT MATERIAL SAVE
            if (INPUTMTRL_GRID.Rows.Count == 0)
            {
                //입력된 투입 자재정보가 없습니다.
                Util.MessageValidation("SFU1809");
                return;
            }

            for (int i = 0; i < INPUTMTRL_GRID.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "ITEMID"))))
                {
                    //자재ID가 누락되었습니다.
                    Util.MessageValidation("SFU1821");
                    INPUTMTRL_GRID.SelectedIndex = i;
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "ITEMNAME"))))
                {
                    //자재명이 누락되었습니다.
                    Util.MessageValidation("SFU1830");
                    INPUTMTRL_GRID.SelectedIndex = i;
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "LOTID"))))
                {
                    //자재LOT이 누락되었습니다.
                    Util.MessageValidation("SFU1823");
                    INPUTMTRL_GRID.SelectedIndex = i;
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "INPUTQTY"))) ||
                    Util.NVC_Int(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "INPUTQTY")) < 0)
                {
                    //사용량이 누락되었거나 0보다 커야 합니다.
                    Util.MessageValidation("SFU1591");
                    INPUTMTRL_GRID.SelectedIndex = i;
                    return;
                }
            }

            // SET BIZRULE
            SetMaterialList();
        }

        /// <summary>
        /// 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMaterialRemove(object sender, EventArgs e)
        {
            // INPUT MATERIAL REMOVE
            int iChkCount = _util.GetDataGridCheckCnt(INPUTMTRL_GRID, "CHK");

            if (iChkCount == 0)
            {
                //선택된 자재 정보가 없습니다.
                Util.MessageValidation("SFU1643");
                return;
            }

            // SET BIZRULE
            RemoveMaterialList();
        }
        #endregion

        #region [투입반제품]
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHalfProdSearch(object sender, EventArgs e)
        {
            // INPUT HALF PRODUCT REFRESH
            GetHalfProductList();
        }

        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHalfProdSave(object sender, EventArgs e)
        {
            // INPUT HALF PRODUCT SAVE
            if (INPUTPRD_GRID.Rows.Count == 0)
            {
                //입력된 투입 반제품정보가 없습니다.
                Util.MessageValidation("SFU1808");
                return;
            }

            for (int i = 0; i < INPUTPRD_GRID.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "INPUTQTY"))) ||
                    Util.NVC_Int(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "INPUTQTY")) < 0)
                {
                    //투입량이 누락되었거나 0보다 커야 합니다.
                    Util.MessageValidation("SFU1970");
                    INPUTPRD_GRID.SelectedIndex = i;
                    return;
                }
            }

            // SET BIZRULE
            SetHalfProdList();
        }

        /// <summary>
        /// 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHalfProdRemove(object sender, EventArgs e)
        {
            // INPUT HALF PRODUCT REMOVE
            int iChkCount = _util.GetDataGridCheckCnt(INPUTPRD_GRID, "CHK");

            if (iChkCount == 0)
            {
                //선택된 반제품 정보가 없습니다.
                Util.MessageValidation("SFU1638");
                return;
            }

            // SET BIZRULE
            RemoveHalfProdList();
        }
        #endregion

        #region [생산반제품]
        /// <summary>
        /// 자동조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnoAutoSearchOutSelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();

                    int iSec = 0;

                    if (_cbAutoSearchChange != null && _cbAutoSearchChange.SelectedValue != null && !_cbAutoSearchChange.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(_cbAutoSearchChange.SelectedValue.ToString());

                    if (iSec == 0 && bSetAutoSelTime)
                    {
                        dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        bSetAutoSelTime = true;
                        return;
                    }

                    dispatcherTimer.Interval = new TimeSpan(0, 0, iSec);
                    dispatcherTimer.Start();

                    if (bSetAutoSelTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        Util.MessageValidation("SFU1605", _cbAutoSearchChange.SelectedValue.ToString());
                    }

                    bSetAutoSelTime = true;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void OnDataCollectProdGridBeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("SPCL_RSNCODE") || e.Column.Name.Equals("SPECIALDESC"))
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (!DataTableConverter.GetValue(e.Row.DataItem, "SPECIALYN").Equals("Y"))
                {
                    e.Cancel = true;
                    ////dg.BeginEdit(e.Row.Index, e.Column.Index);
                }

            }
         
        }

        private void OnDataCollectProdGridCommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            // 체크시
            if (e.Cell.Column.Name.Equals("CHK"))
            {
                if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK").Equals(1))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        SetOutTrayButtonEnable(e.Cell.Row);

                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                        {
                            if (e.Cell.Row.Index != idx)
                            {
                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                            }
                        }
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                        dg.SelectedIndex = -1;
                    }
                }
                else
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                    {
                        SetOutTrayButtonEnable(null);
                    }
                }
            }
            else if (e.Cell.Column.Name.Equals("CBO_SPCL"))
            {
                if (!DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPECIALYN").Equals("Y"))
                {
                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "SPCL_RSNCODE", "");
                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "SPECIALDESC", "");
                    dg.EndEditRow(true);
                }
            }

        }

        /// <summary>
        /// dgProductionCell.LoadedCellPresenter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridLoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!_procId.Equals(Process.WINDING))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }

                    if (e.Cell.Column.Name.Equals("btnModify"))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                        {
                            // Tray 수정가능
                            ((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("수정");
                            ((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content).Tag = "U";
                            ((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content).Foreground = new SolidColorBrush(Colors.Red);
                            ((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                            //((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content)
                        }
                        else
                        {
                            // Tray 조회만
                            ((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content).Content = ObjectDic.Instance.GetObjectName("조회");
                            ((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content).Tag = "X";
                            ((System.Windows.Controls.ContentControl)e.Cell.Presenter.Content).FontWeight = FontWeights.Bold;
                        }
                    }

                }
            }));
        }

        /// <summary>
        /// dgProductionCell.UnloadedCellPresenter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataCollectGridUnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        #region Tray 삭제
        /// <summary>
        /// Try Tray삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnTrayRemove(object sender, EventArgs e)
        {
            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            try
            {
                if (!CanTrayDelete())
                    return;

                //string sMsg = "삭제 하시겠습니까?";
                string messageCode = "SFU1230";
                double dCellQty = 0;

                string sCellQty = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[_util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK")].DataItem, "CELLQTY"));

                if (!sCellQty.Equals(""))
                    double.TryParse(sCellQty, out dCellQty);

                if (!sCellQty.Equals("") && dCellQty != 0)
                {
                    //sMsg = "Cell 수량이 존재 합니다.\n그래도 삭제 하시겠습니까?";
                    messageCode = "SFU1320";
                }

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        TrayDelete();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                // Timer Start.
                if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                    dispatcherTimer.Start();
            }

        }

        #endregion

        #region Tray확정취소 
        /// <summary>
        /// Tray확정취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnTrayCancel(object sender, EventArgs e)
        {
            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            try
            {
                if (!CanConfirmCancel())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetTrayConfirmCancel();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                // Timer Start.
                if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                    dispatcherTimer.Start();
            }
        }

        #endregion

        #region Tray 확정
        /// <summary>
        /// Tray확정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrayConfirm(object sender, RoutedEventArgs e)
        {
            // Timer Stop.
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            try
            {
                if (!CanTrayConfirm())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        TrayConfirm();
                    }
                });

                // 특별 Tray 정보 조회.
                GetSpecialTrayInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                // Timer Start.
                if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                    dispatcherTimer.Start();
            }

        }
        #endregion

        #region Tray 저장(특이사항)
        /// <summary>
        /// Tray 저장(특이사항)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOutSave(object sender, RoutedEventArgs e)
        {
            if (!CanSaveTray())
                return;

            SaveTray();
        }
        #endregion

        #region 특별 Tray 적용
        /// <summary>
        /// 특별 Tray 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnOutTraySplSave(object sender, EventArgs e)
        {
            if (!CanOutTraySplSave())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("적용 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetSpecialTray();
                }
            });
        }
        #endregion

        /// <summary>
        /// 특별 Tray 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnOutTraySplCheckBoxClick(object sender, RoutedEventArgs e)
        {
            // SEARCH WIPSTATE 순서
            CheckBox checkBox = sender as CheckBox;

            if ((bool)(checkBox.IsChecked))
            {
                _ckOutTraySplReasonChange.IsEnabled = true;
                _txOutTrayReamrkChange.IsEnabled = true;
            }
            else
            {
                _ckOutTraySplReasonChange.IsEnabled = false;
                _txOutTrayReamrkChange.IsEnabled = false;
            }
        }

        #endregion

        #endregion Events

        #region User Method

        #region [BizCall]

        #region [### 작업대상 조회 ###]
        public void GetProductLot()
        {
            // SELECT PRODUCT LOT
            try
            {
                if (CHECK_WAIT.IsChecked == false && CHECK_RUN.IsChecked == false && CHECK_END.IsChecked == false)
                {
                    //조회 상태가 선택되지 않았습니다.
                    Util.MessageValidation("SFU1904"); 
                    return;
                }

                string sWipState = string.Empty;
                List<string> sCondition = new List<string>();

                if (CHECK_RUN.IsChecked == true)
                    sCondition.Add(CHECK_RUN.Tag.ToString());

                if (CHECK_END.IsChecked == true)
                    sCondition.Add(CHECK_END.Tag.ToString());

                sWipState = string.Join(",", sCondition);
                sWipState = sWipState + ",";

                if (string.IsNullOrEmpty(sWipState.Trim()))
                {
                    //WIP 상태를 선택하세요.
                    Util.MessageValidation("SFU1438");
                    return;
                }

                // SET BIND
                ShowLoadingIndicator();

                Util.gridClear(PRODLOT_GRID);

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_PRODUCTLOT_ASSY();

                DataRow newRow = IndataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);

                if (CHECK_RUN.IsChecked == true || CHECK_END.IsChecked == true) // WAIT는 설비명이 없음
                    newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);

                newRow["WIPSTAT"] = sWipState;
                newRow["PROCID"] = PROCID;

                if (string.Equals(PROCID, Process.WINDING) && !_micro)
                    newRow["WIPTYPECODE"] = "OUT";
                else
                    newRow["WIPTYPECODE"] = "PROD";

                IndataTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTLOT_ASSY", "RQSTDT", "RSLTDT", IndataTable);

                ////PRODLOT_GRID.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(PRODLOT_GRID, dtResult, FrameOperation, true);

                // 대기 Lot 정보 조회후 조회전 선택 Lot 체크
                if (PRODLOT_GRID != null && PRODLOT_GRID.Rows.Count > 0)
                {
                    if (LOTINFO_GRID != null && LOTINFO_GRID.Rows.Count > 0)
                    {
                        int row = _util.GetDataGridRowIndex(PRODLOT_GRID, "LOTID", Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID")));

                        if (row < 0)
                            return;

                        DataTableConverter.SetValue(PRODLOT_GRID.Rows[row].DataItem, "CHK", true);

                        //row 색 바꾸기
                        PRODLOT_GRID.SelectedIndex = row;
                    }
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
        #endregion

        #region [### 불량/Loss/물청 조회,저장 ###]
        private void GetDefectList(C1DataGrid dataGrid, string sReasonType, string sLotId)
        {
            // SELECT DEFECT, LOSS, CHARGE INFO
            try
            {
                if (string.Equals(sReasonType, "DEFECT_LOT"))
                    iDeffectSumQty = 0;
                //else if (string.Equals(sReasonType, "LOSS_LOT"))
                //    iLossSumQty = 0;
                //else if (string.Equals(sReasonType, "CHARGE_PROD_LOT"))
                //    iChargeSumQty = 0;

                ShowLoadingIndicator();

                Util.gridClear(dataGrid);

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_ASSY_DEFECT_ALL();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = PROCID;
                //Indata["RESNTYPE"] = sReasonType;
                Indata["LOTID"] = sLotId;
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_DEFECT_ALL", "INDATA", "RSLTDT", IndataTable);

                ////dataGrid.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dataGrid, dt, null, true);
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
        private void SetDefectList(C1DataGrid datagrid, string sReasonType, string sLotId)
        {
            // SET DEFECT INFO
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보를 저장하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        DataSet inDataSet = _bizRule.GetBR_QCA_REG_WIPREASONCOLLECT_ALL();

                        DataTable inDataTable = inDataSet.Tables["INDATA"];
                        DataRow row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        row["USERID"] = LoginInfo.USERID;
                        inDataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inDefectLot = inDataSet.Tables["INRESN"];

                        for (int i = 0; i < datagrid.GetRowCount(); i++)
                        {
                            row = inDefectLot.NewRow();
                            row["LOTID"] = Util.NVC(sLotId);
                            row["ACTID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "ACTID"));
                            row["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNCODE"));
                            row["RESNQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RESNQTY")));
                            row["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "COSTCENTERID"));

                            inDataSet.Tables["INRESN"].Rows.Add(row);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (defectResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }
                            // 정상처리되었습니다.
                            Util.MessageInfo("SFU1275");

                            datagrid.EndEdit(true);
                            this.GetDefectList(datagrid, sReasonType, sLotId);

                            isChangeDefect = false;

                        }, inDataSet);
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
            });
        }
        #endregion

        #region [### 투입자재 조회, 저장, 삭제 ###]

        private void GetMaterialList()
        {
            // SELECT INPUT MATERIAL
            try
            {
                ShowLoadingIndicator();

                Util.gridClear(INPUTMTRL_GRID);

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_ASSY_INPUT_MTRL();
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                Indata["MTRLTYPE"] = "MTRL";
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_INPUT_MTRL", "INDATA", "RSLTDT", IndataTable);

                ////INPUTMTRL_GRID.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(INPUTMTRL_GRID, dt, null, true);
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
        private DataTable GetMaterialList(string sWorkWorder, string sItemName)
        {
            // SELECT MATERIAL ITEM
            try
            {
                ShowLoadingIndicator();

                //Util.gridClear(gdInputItem);

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_ITEM_LIST();
                DataRow Indata = IndataTable.NewRow();

                Indata["WOID"] = Util.NVC(sWorkWorder);

                if (!string.IsNullOrEmpty(sItemName))
                    Indata["ITEMCODE"] = sItemName;

                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ITEM_LIST", "INDATA", "RSLTDT", IndataTable);

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return new DataTable();
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        private void SetMaterialList()
        {
            // SET INPUT MATERIAL INFO
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입자재 정보를 저장하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("투입자재 정보를 저장하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        string sBizName = "";

                        if (string.Equals(_procId, Process.WINDING))
                            sBizName = "BR_PRD_REG_INPUT_MTRL_WN";
                        else if (string.Equals(_procId, Process.ASSEMBLY))
                            sBizName = "BR_PRD_REG_INPUT_MTRL_AS";
                        else if (string.Equals(_procId, Process.WASHING))
                            sBizName = "BR_PRD_REG_INPUT_MTRL_WS";

                        DataSet inDataSet = _bizRule.GetBR_PRD_REG_INPUT_MTRL_WN();

                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                        inDataTable.Rows.Add(row);

                        DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                        for (int i = 0; i < INPUTMTRL_GRID.GetRowCount(); i++)
                        {
                            row = inInputTable.NewRow();
                            row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "SLOTNO"));
                            row["EQPT_MOUNT_PSTN_STATE"] = "A";
                            row["MTRLID"] = Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "ITEMID"));
                            row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "LOTID"));
                            row["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "INPUTQTY"));

                            inInputTable.Rows.Add(row);
                        }

                        new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
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

                                this.GetMaterialList();

                                isChangeMaterial = false;
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, inDataSet);
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
            });
        }
        private void RemoveMaterialList()
        {
            // REMOVE INPUT MATERIAL INFO
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입자재 정보를 삭제하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("투입자재 정보를 삭제하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        string sBizName = "";
                        if (string.Equals(_procId, Process.WINDING))
                            sBizName = "BR_PRD_DEL_INPUT_MTRL_WN";
                        else if (string.Equals(_procId, Process.ASSEMBLY))
                            sBizName = "BR_PRD_DEL_INPUT_MTRL_AS";
                        else if (string.Equals(_procId, Process.WASHING))
                            sBizName = "BR_PRD_DEL_INPUT_MTRL_WS";

                        DataSet inDataSet = _bizRule.GetBR_PRD_DEL_INPUT_ITEM_WN();

                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                        inDataTable.Rows.Add(row);

                        DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                        for (int i = 0; i < INPUTMTRL_GRID.GetRowCount(); i++)
                        {
                            // 삭제 대상은 체크 되어 있고, INPUTSEQ_NO가 0이 아닌것만 삭제
                            if (_util.GetDataGridCheckValue(INPUTMTRL_GRID, "CHK", i) == true &&
                                 !string.Equals(Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "INPUT_SEQNO")), "0"))
                            {
                                row = inInputTable.NewRow();
                                row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "INPUT_SEQNO"));
                                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(INPUTMTRL_GRID.Rows[i].DataItem, "LOTID"));

                                inInputTable.Rows.Add(row);
                            }
                        }

                        if (inInputTable.Rows.Count > 0)
                        {
                            new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    RemoveGridRow(INPUTMTRL_GRID);

                                    //정상 처리 되었습니다.
                                    Util.MessageInfo("SFU1275");

                                    //this.GetMaterialList();
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                finally
                                {
                                    HiddenLoadingIndicator();
                                }
                            }, inDataSet);
                        }
                        else
                        {
                            RemoveGridRow(INPUTMTRL_GRID);
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
            });
        }
        #endregion

        #region [### 투입반제품 조회,저장,삭제 ###]
        private void GetHalfProductList()
        {
            // SELECT INPUT HALFPRODUCT
            try
            {
                ShowLoadingIndicator();

                string sBizName = "";
                if (string.Equals(_procId, Process.WINDING))
                    sBizName = "DA_PRD_SEL_INPUT_HALFPROD_WN";
                else if (string.Equals(_procId, Process.ASSEMBLY))
                    sBizName = "DA_PRD_SEL_INPUT_HALFPROD_AS";
                else
                    sBizName = "DA_PRD_SEL_OUT_LOT_LIST_WS";

                Util.gridClear(INPUTPRD_GRID);

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_INPUT_HALFPROD();

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                Indata["MTRLTYPE"] = "PROD";
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", IndataTable);

                ////INPUTPRD_GRID.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(INPUTPRD_GRID, dt, null, true);
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

        private void SetHalfProdList()
        {
            // SET INPUT HALF PRODUCT INFO (초반에 공통으로 쓰려다가 나중에 나눠짐, WASHING도 별개로 사용한다면 각 화면별로 나누는것도 고려해야함)
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 반제품 정보를 저장하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("투입 반제품 정보를 저장하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        string sBizName = "";
                        if (string.Equals(_procId, Process.WINDING))
                            sBizName = "BR_PRD_REG_INPUT_LOT_WN";
                        else if (string.Equals(_procId, Process.ASSEMBLY))
                            sBizName = "BR_PRD_REG_INPUT_LOT_AS";
                        else if (string.Equals(_procId, Process.WASHING))
                            sBizName = "BR_PRD_REG_INPUT_LOT_WS";

                        DataSet inDataSet = null;

                        if (string.Equals(PROCID, Process.WINDING))
                        {
                            inDataSet = _bizRule.GetBR_PRD_REG_INPUT_LOT_WN();

                            DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                            DataRow row = inDataTable.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["IFMODE"] = IFMODE.IFMODE_OFF;
                            row["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                            row["USERID"] = LoginInfo.USERID;
                            row["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                            inDataTable.Rows.Add(row);

                            DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                            for (int i = 0; i < INPUTPRD_GRID.GetRowCount(); i++)
                            {
                                row = inInputTable.NewRow();
                                row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "SLOTNO"));
                                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "PRODID"));
                                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "LOTID"));
                                row["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "INPUTQTY"));

                                inInputTable.Rows.Add(row);
                            }
                        }
                        else if (string.Equals(_procId, Process.ASSEMBLY))
                        {
                            inDataSet = _bizRule.GetBR_PRD_REG_INPUT_LOT_AS();

                            DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                            DataRow row = inDataTable.NewRow();
                            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                            row["IFMODE"] = IFMODE.IFMODE_OFF;
                            row["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                            row["USERID"] = LoginInfo.USERID;
                            row["PROD_LOTID"] =Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                            inDataTable.Rows.Add(row);

                            DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                            for (int i = 0; i < INPUTPRD_GRID.GetRowCount(); i++)
                            {
                                row = inInputTable.NewRow();
                                row["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "SLOTNO"));
                                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "PRODID"));
                                row["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "RUNCARDID"));
                                row["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem,"INPUTQTY"));

                                inInputTable.Rows.Add(row);
                            }
                        }
                        else
                        {
                            
                        }

                        new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
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

                                this.GetHalfProductList();

                                isChangeProduct = false;
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, inDataSet);
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
            });
        }

        private void RemoveHalfProdList()
        {
            // REMOVE INPUT HALF PRODUCT INFO
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 반제품 정보를 삭제하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("투입 반제품 정보를 삭제하시겠습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        string sBizName = "";
                        if (string.Equals(_procId, Process.WINDING))
                            sBizName = "BR_PRD_DEL_INPUT_LOT_WN";
                        else if (string.Equals(_procId, Process.ASSEMBLY))
                            sBizName = "BR_PRD_DEL_INPUT_LOT_AS";
                        else if (string.Equals(_procId, Process.WASHING))
                            sBizName = "BR_PRD_DEL_INPUT_LOT_WS";

                        DataSet inDataSet = _bizRule.GetBR_PRD_DEL_INPUT_ITEM_WN();

                        DataTable inDataTable = inDataSet.Tables["IN_EQP"];
                        DataRow row = inDataTable.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        row["USERID"] = LoginInfo.USERID;
                        row["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                        inDataTable.Rows.Add(row);

                        DataTable inInputTable = inDataSet.Tables["IN_INPUT"];

                        for (int i = 0; i < INPUTPRD_GRID.GetRowCount(); i++)
                        {
                            // 삭제 대상은 체크 되어 있고, INPUTSEQ_NO가 0이 아닌것만 삭제
                            if (_util.GetDataGridCheckValue(INPUTPRD_GRID, "CHK", i) == true &&
                                 !string.Equals(Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "INPUT_SEQNO")), "0"))
                            {
                                row = inInputTable.NewRow();
                                row["INPUT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "INPUT_SEQNO"));
                                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[i].DataItem, "LOTID"));

                                inInputTable.Rows.Add(row);
                            }
                        }

                        if (inInputTable.Rows.Count > 0)
                        {
                            new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    RemoveGridRow(INPUTPRD_GRID);

                                    //정상 처리 되었습니다.
                                    Util.MessageInfo("SFU1275");

                                    //this.GetHalfProductList();
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                finally
                                {
                                    HiddenLoadingIndicator();
                                }
                            }, inDataSet);
                        }
                        else
                        {
                            RemoveGridRow(INPUTPRD_GRID);
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
            });
        }
        #endregion

        #region [### 생산 반제품 조회 ###]
        public void GetProductCellList()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName;
                if (_procId.Equals(Process.WINDING))
                    sBizName = "DA_PRD_SEL_OUT_LOT_LIST_WNM";
                else
                    sBizName = "DA_PRD_SEL_OUT_LOT_LIST_WS";

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                Indata["PROCID"] = _procId;
                Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(_dgProdCellChange, dt, null, true);

                if (_dgProdCellChange.Rows.Count > 0)
                    _dgProdCellChange.GetCell(0, 0);

                if (_procId.Equals(Process.WINDING))
                    return;

                // 특별TRAY 콤보
                DataTable dtcbo = new DataTable();
                dtcbo.Columns.Add("CODE");
                dtcbo.Columns.Add("NAME");

                dtcbo.Rows.Add("N", "N");
                dtcbo.Rows.Add("Y", "Y");

                (_dgProdCellChange.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtcbo.Copy());

                // 사유 콤보
                DataTable dtReason = SetOutTraySplReasonCommonCode();
                (_dgProdCellChange.Columns["SPCL_RSNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtReason.Copy());

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

        public DataTable GetHalfProductList(string sElectrodeType, string sLotId)
        {
            // SELECT HALF PRODUCT
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_HALFPROD_LIST_WN();
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Indata["PROCID"] = PROCID;

                if (!string.IsNullOrEmpty(sElectrodeType))
                    Indata["ELECTRODETYPE"] = Util.NVC(sElectrodeType);

                Indata["LOTID"] = Util.NVC(sLotId);
                IndataTable.Rows.Add(Indata);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HALFPROD_LIST_WN", "INDATA", "RSLTDT", IndataTable);

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return new DataTable();
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 작업취소 ###]
        private void StartCancelProcess()
        {
            // LOT START CANCEL ( TODO : CANCEL BIZRULE 확정 나면 BIZRULE SET으로 뺄것 )
            if (!ValidateCancelRun())
                return;

            try
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ShowLoadingIndicator();

                        string sBizName = "BR_PRD_REG_END_LOT_HS_UI";

                        DataTable IndataTable = _bizRule.GetBR_PRD_REG_END_LOT_HS_UI();
                        DataRow Indata = IndataTable.NewRow();

                        Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        Indata["IFMODE"] = IFMODE.IFMODE_OFF;
                        Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        Indata["LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                        Indata["USERID"] = LoginInfo.USERID;
                        IndataTable.Rows.Add(Indata);

                        new ClientProxy().ExecuteService(sBizName, "INDATA", null, IndataTable, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");

                            this.Refresh();

                        });
                    }
                });
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
        #endregion

        #region [### 작업종료 ###]
        private void EqpEndProcess()
        {
            if (!ValidateEqpEnd())
                return;

            CMM_ASSY_EQPEND wndPop = new CMM_ASSY_EQPEND();
            wndPop.FrameOperation = FrameOperation;

            if (wndPop != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Parameters[1] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Parameters[2] = _procId;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[_util.GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK")].DataItem, "LOTID"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[_util.GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK")].DataItem, "WIPSEQ"));
                Parameters[5] = "N";    // Stacking.
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[_util.GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK")].DataItem, "WIPDTTM_ST_ORG"));
                Parameters[7] = Util.NVC(DataTableConverter.GetValue(PRODLOT_GRID.Rows[_util.GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK")].DataItem, "DTTM_NOW"));
                C1WindowExtension.SetParameters(wndPop, Parameters);
                
                wndPop.Closed += new EventHandler(wndEqpEnd_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
                grdMain.Children.Add(wndPop);
                wndPop.BringToFront();
            }
        }

        private void wndEqpEnd_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPEND window = sender as CMM_ASSY_EQPEND;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
            }
        }

        #endregion

        #region [### 실적확정 ###]
        private void ConfirmProcess()
        {
            // LOT CONFIRM
            if (!ValidationConfirmRun())
                return;

            // 불량, 투입자재, 투입 반제품 저장여부 체크
            if (!ValidDataCollect())
                return;

            try
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("실적 확정 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                Util.MessageConfirm("SFU1706", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ShowLoadingIndicator();

                        string sBizName = "";

                        if (string.Equals(_procId, Process.WINDING))
                            sBizName = "BR_PRD_REG_END_LOT_WN";
                        else if (string.Equals(_procId, Process.ASSEMBLY))
                            sBizName = "BR_PRD_REG_END_LOT_AS";
                        else if (string.Equals(_procId, Process.WASHING))
                            sBizName = "BR_PRD_REG_END_LOT_WS";

                        DateTime dtTime;
                        dtTime = new DateTime((grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDateTime.Year,
                                              (grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDateTime.Month,
                                              (grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDateTime.Day);

                        // 차후에 실적확정을 각 화면별로 결정 필요
                        DataSet indataSet = new DataSet();
                        if (string.Equals(_procId, Process.WINDING))
                            indataSet = _bizRule.GetBR_PRD_REG_END_LOT_WN();
                        else
                            indataSet = _bizRule.GetBR_PRD_REG_END_LOT_AS();

                        DataTable inTable = indataSet.Tables["INDATA"];
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                        newRow["SHIFT"] = (grdShift.Children[0] as UcShift).txtShift.Tag.ToString();
                        newRow["WIPDTTM_ED"] = dtTime;   
                        newRow["WIPNOTE"] = new TextRange((grdResult.Children[0] as UcAssyResult).txtRemark.Document.ContentStart, (grdResult.Children[0] as UcAssyResult).txtRemark.Document.ContentEnd).Text;
                        newRow["WRK_USER_NAME"] = (grdShift.Children[0] as UcShift).txtWorker.Text;
                        newRow["USERID"] = LoginInfo.USERID;

                        if (string.Equals(_procId, Process.WINDING))
                            newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                        else
                            newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));

                        newRow["INPUT_QTY"] = (grdResult.Children[0] as UcAssyResult).txtWipQty.Value;
                        newRow["OUTPUT_QTY"] = (grdResult.Children[0] as UcAssyResult).txtGoodQty.Value;
                        newRow["RESNQTY"] =(grdResult.Children[0] as UcAssyResult).txtDefectQty.Value;

                        inTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (string.Equals(_procId, Process.WINDING))
                                {
                                    // Winding 이력카드 출력
                                    HistoryCradPrint(Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID")));
                                }
                                else
                                {
                                    //정상 처리 되었습니다.
                                    Util.MessageInfo("SFU1275");
                                }

                                this.Refresh();
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
                });
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
        #endregion

        #region [### Winding 이력카드 출력 ###]
        private void HistoryCradPrint(string sLotId)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet ds = _bizRule.GetBR_PRD_SEL_WINDING_RUNCARD_WN();
                DataTable IndataTable = ds.Tables["IN_DATA"];
                DataRow Indata = IndataTable.NewRow();

                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotId;

                IndataTable.Rows.Add(Indata);
                ds.Tables.Add(IndataTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_WINDING_RUNCARD_WN", "IN_DATA", "OUT_DATA,OUT_ELEC,OUT_DFCT,OUT_SEPA", ds);

                if (dsResult.Tables["OUT_DATA"].Rows.Count == 0)
                {
                    //조회 가능한 Winding Lot 정보가 없습니다.
                    Util.MessageValidation("SFU1900");
                    return;
                }

                CMM_ASSY_WINDERCARD_PRINT _print = new CMM_ASSY_WINDERCARD_PRINT();
                _print.FrameOperation = this.FrameOperation;

                if (_print != null)
                {
                    // SET PARAMETER
                    object[] Parameters = new object[1];
                    Parameters[0] = dsResult;

                    C1WindowExtension.SetParameters(_print, Parameters);

                    //_print.ShowModal();
                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(_print);
                            _print.BringToFront();
                            break;
                        }
                    }
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
        #endregion

        #region [### Tray ###]
        private void GetTrayInfo(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");

                DataTable IndataTable = _bizRule.GetDA_PRD_SEL_OUT_LOT_LIST_WS();
                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                Indata["PROCID"] = _procId;
                Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Indata["TRAYID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "TRAYID"));
                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_WS", "INDATA", "OUTDATA", IndataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";
                        //sMsg = "TRAY가 미확정 상태가 아닙니다.";
                        sMsg = "SFU1431";
                    }
                }
                else
                {
                    sRet = "NG";
                    //sMsg = "존재하지 않습니다.";
                    sMsg = "SFU2881";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                sRet = "EXCEPTION";
                sMsg = ex.Message;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 생산 반제품 Tray 삭제
        /// </summary>
        private void TrayDelete()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName;

                if (_procId.Equals(Process.WINDING))
                    sBizName = "BR_PRD_REG_DELETE_OUT_LOT_WNM";
                else
                    sBizName = "BR_PRD_REG_DELETE_OUT_LOT_WS";

                DataTable IndataTable = _bizRule.GetBR_PRD_REG_DELETE_OUT_LOT_WS();

                int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");
                DataRow workDataRow = WORKORDER.GetSelectWorkOrderRow();

                DataRow newRow = IndataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "OUT_LOTID"));
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "TRAYID"));
                newRow["WO_DETL_ID"] = Util.NVC(workDataRow["WO_DETL_ID"]);
                newRow["USERID"] = LoginInfo.USERID;

                IndataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "IN_EQP", null, IndataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        GetProductLot();
                        GetProductCellList();

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
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
                );
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
        /// 생산 반제품 Tray 확정취소
        /// </summary>
        private void SetTrayConfirmCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizRule.GetBR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS();

                DataTable IndataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = IndataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(newRow);

                int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");

                DataTable InCSTTable = indataSet.Tables["IN_CST"];
                newRow = InCSTTable.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "OUT_LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "TRAYID"));
                InCSTTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CNFM_CANCEL_OUT_LOT_WS", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        GetProductCellList();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
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
        /// Tray 확정
        /// </summary>
        private void TrayConfirm()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName;
                int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");

                if (_procId.Equals(Process.WINDING))
                    sBizName = "BR_PRD_REG_END_OUT_LOT_WNM";
                else
                    sBizName = "BR_PRD_REG_END_OUT_LOT_WS";

                DataSet indataSet = _bizRule.GetBR_PRD_REG_END_OUT_LOT_WS();

                DataTable IndataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = IndataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(newRow);

                DataTable InCSTTable = indataSet.Tables["IN_CST"];
                newRow = InCSTTable.NewRow();

                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "OUT_LOTID"));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "CELLQTY")));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "TRAYID"));
                newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "SPECIALYN"));
                newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "SPECIALDESC"));
                newRow["SPCL_CST_RSNCODE"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "SPCL_RSNCODE"));
                InCSTTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        GetProductCellList();
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();

                    }
                }, indataSet
                );
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
        /// Tray 특이사항 저장
        /// </summary>
        private void SaveTray()
        {
            /*
            try
            {
                _dgProdCellChange.EndEdit();

                int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");
                string specYN = null;
                string SpecDesc = null;
                string SpecRsnCode = null;

                if (!_procId.Equals(Process.WINDING))
                {
                    specYN = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "SPECIALYN"));
                    SpecDesc = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "SPECIALDESC"));
                    SpecRsnCode = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "SPCL_RSNCODE"));

                    if (specYN.Equals("Y"))
                    {
                        if (string.IsNullOrWhiteSpace(SpecDesc))
                        {
                            //Util.Alert("특별관리내역을 입력하세요.");
                            Util.MessageValidation("SFU1990");
                            return;
                        }
                    }
                    else if (specYN.Equals("N"))
                    {
                        if (!string.IsNullOrWhiteSpace(SpecDesc))
                        {
                            //Util.Alert("특별관리내역을 삭제하세요.");
                            Util.MessageValidation("SFU1989");
                            return;
                        }
                    }
                }

                ShowLoadingIndicator();

                DataSet indataSet = _bizRule.GetBR_PRD_REG_UPD_OUT_LOT_WS();

                DataTable inDataTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(LOTINFO_GRID.Rows[0].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                DataTable inLot = indataSet.Tables["IN_LOT"];
                DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                newRow = null;

                for (int i = 0; i < _dgProdCellChange.Rows.Count - _dgProdCellChange.BottomRows.Count; i++)
                {
                    // Tray 정보 DataTable             
                    newRow = inLot.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "OUT_LOTID"));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "TRAYID"));

                    if ((grdDataCollect.Children[0] as UcAssyDataCollect).rdoTraceNotUse.IsChecked.HasValue &&
                        (bool)(grdDataCollect.Children[0] as UcAssyDataCollect).rdoTraceNotUse.IsChecked)
                        newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "CELLQTY")));

                    inLot.Rows.Add(newRow);

                    newRow = null;

                    // 특별 Tray DataTable                
                    newRow = inSpcl.NewRow();
                    newRow["SPCL_CST_GNRT_FLAG"] = specYN;
                    newRow["SPCL_CST_NOTE"] = SpecDesc;
                    newRow["SPCL_CST_RSNCODE"] = SpecRsnCode;

                    inSpcl.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_WS", "IN_EQP,IN_LOT,IN_SPCL", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();
                        GetProductCellList();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
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
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            */
        }

        /// <summary>
        /// 생산 반제품 특별 Tray 저장
        /// </summary>
        private void SetSpecialTray()
        {
            try
            {
                ShowLoadingIndicator();

                string sRsnCode = _ckOutTraySplReasonChange.SelectedValue == null ? "" : _ckOutTraySplReasonChange.SelectedValue.ToString();

                DataTable inTable = _bizRule.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = Process.PACKAGING;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                newRow["SPCL_LOT_GNRT_FLAG"] = (bool)_ckOutTraySplChange.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = (bool)_ckOutTraySplChange.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = _txOutTrayReamrkChange.Text;
                newRow["SPCL_PROD_LOTID"] = (bool)_ckOutTraySplChange.IsChecked ? LOTID : "";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_SPCL_CST", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        // 특별 Tray 정보 조회.
                        GetSpecialTrayInfo();
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
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 생산 반제품 특별 Tray 저장후 조회
        /// </summary>
        /// <returns></returns>
        public DataTable GetSpecialTrayInfo()
        {
            try
            {
                if (_procId.Equals(Process.WINDING))
                    return null;

                if (EQUIPMENT_COMBO == null || EQUIPMENT_COMBO.SelectedValue == null)
                    return null;

                ShowLoadingIndicator();

                DataTable inTable = _bizRule.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_LOT", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _ckOutTraySplChange.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                    _txOutTrayReamrkChange.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                    if (_ckOutTraySplReasonChange != null && _ckOutTraySplReasonChange.Items != null && _ckOutTraySplReasonChange.Items.Count > 0 && _ckOutTraySplReasonChange.Items.CurrentItem != null)
                    {
                        DataView dtview = (_ckOutTraySplReasonChange.Items.CurrentItem as DataRowView).DataView;

                        if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            _ckOutTraySplReasonChange.SelectedValue = Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"]);

                            if (_ckOutTraySplReasonChange.SelectedIndex < 0 && _ckOutTraySplReasonChange.Items.Count > 0)
                                _ckOutTraySplReasonChange.SelectedIndex = 0;
                        }
                    }
                }
                else
                {
                    _ckOutTraySplChange.IsChecked = false;
                    _txOutTrayReamrkChange.Text = "";

                    if (_ckOutTraySplReasonChange.Items.Count > 0)
                        _ckOutTraySplReasonChange.SelectedIndex = 0;
                }

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

         /// <summary>
        /// 생산반제품 - 사유 콤보 칼럼
        /// </summary>
        private DataTable SetOutTraySplReasonCommonCode()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SPCL_RSNCODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [### 작업조, 작업자 조회 ###]
        private void GetEqptWrkInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(EQUIPMENT_COMBO.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(EQUIPMENTSEGMENT_COMBO.SelectedValue);
                Indata["PROCID"] = _procId;

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            (grdShift.Children[0] as UcShift).txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                            (grdShift.Children[0] as UcShift).txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            (grdShift.Children[0] as UcShift).txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                            (grdShift.Children[0] as UcShift).txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            (grdShift.Children[0] as UcShift).txtShiftDateTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]) + " ~ " + Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            (grdShift.Children[0] as UcShift).txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            (grdShift.Children[0] as UcShift).txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                        }
                        else
                        {
                            (grdShift.Children[0] as UcShift).txtShift.Text = string.Empty;
                            (grdShift.Children[0] as UcShift).txtShift.Tag = string.Empty;
                            (grdShift.Children[0] as UcShift).txtWorker.Text = string.Empty;
                            (grdShift.Children[0] as UcShift).txtWorker.Tag = string.Empty;
                            (grdShift.Children[0] as UcShift).txtShiftDateTime.Text = string.Empty;
                            (grdShift.Children[0] as UcShift).txtShiftStartTime.Text = string.Empty;
                            (grdShift.Children[0] as UcShift).txtShiftEndTime.Text = string.Empty;
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
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region DB 시간 조회 - 실적확정시 화면 하단 시간체크용
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
        #endregion

        #endregion

        #region [Validation]

        #region [### 실적확정 체크 ###]
        private bool ValidDataCollect()
        {
            if (isChangeDefect)
            {
                //불량 정보를 저장하세요.     
                Util.MessageValidation("SFU1577");
                return false;
            }

            if (isChangeMaterial)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입자재 정보를 저장하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageValidation("투입자재 정보를 저장하세요.");
                return false;
            }

            if (isChangeProduct)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 반제품 정보를 저장하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageValidation("투입 반제품 정보를 저장하세요.");
                return false;
            }

            int iChkCount = _util.GetDataGridCheckCnt(INPUTPRD_GRID, "CHK");

            if (iChkCount != 0)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 반제품 정보를 저장하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageValidation("투입 반제품 정보를 저장하세요.");
                return false;
            }

            if (!string.IsNullOrEmpty((grdShift.Children[0] as UcShift).txtShiftEndTime.Text))
            {
                DateTime shiftEndDateTime = Convert.ToDateTime((grdShift.Children[0] as UcShift).txtShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();
                int result = DateTime.Compare(shiftEndDateTime, systemDateTime);

                if (result < 0)
                {
                    Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                    (grdShift.Children[0] as UcShift).txtWorker.Text = string.Empty;
                    (grdShift.Children[0] as UcShift).txtWorker.Tag = string.Empty;
                    (grdShift.Children[0] as UcShift).txtShift.Text = string.Empty;
                    (grdShift.Children[0] as UcShift).txtShift.Tag = string.Empty;
                    (grdShift.Children[0] as UcShift).txtShiftStartTime.Text = string.Empty;
                    (grdShift.Children[0] as UcShift).txtShiftEndTime.Text = string.Empty;
                    (grdShift.Children[0] as UcShift).txtShiftDateTime.Text = string.Empty;
                    return false;
                }
            }
            return true;
        }
        private bool ValidationConfirmRun()
        {
            // 실적확정 COMMON VALIDATION
            if (string.IsNullOrEmpty(LOTID))
            {
                //실적확정 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1717");
                return false;
            }

            if (string.IsNullOrEmpty((grdShift.Children[0] as UcShift).txtShift.Text))
            {
                //작업조를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty((grdShift.Children[0] as UcShift).txtWorker.Text))
            {
                //작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if ((grdResult.Children[0] as UcAssyResult).txtGoodQty.Value == 0)
            {
                //양품 수량을 확인하십시오.
                Util.MessageValidation("SFU1722");
                return false;
            }

            if (!string.Equals( LOTSTATE, Wip_State.EQPT_END))
            {
                //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU2045");
                return false;
            }

            return true;
        }

        #endregion

        #region [### 반제품 추가 체크 ###]
        public bool ValidateHalfProduct()
        {
            // HALF PRODUCT POPUP
            if (string.IsNullOrEmpty((grdResult.Children[0] as UcAssyResult).txtWorkOrder.Text))
            {
                Util.MessageValidation("SFU1441");      //WORK ORDER ID가 선택되지 않았습니다.
                return false;
            }
            return true;
        }
        #endregion

        #region [### 작업시작 체크 ###]
        public bool ValidateStartRun()
        {
            // START LOT COMMON VALIDATION
            DataRow workDataRow = WORKORDER.GetSelectWorkOrderRow();

            if (workDataRow == null)
            {
                //작업지시를 선택하세요.
                Util.MessageValidation("SFU1443");
                return false;
            }

            return true;
        }
        #endregion

        #region [### 작업취소 체크 ###]
        private bool ValidateCancelRun()
        {
            // START LOT CANCEL COMMON VALIDATION
            if (string.IsNullOrEmpty(LOTID))
            {
                //작업취소 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1852");
                return false;
            }

            int iRow = new Util().GetDataGridCheckFirstRowIndex(PRODLOT_GRID, "CHK");

            if (string.Equals(LOTSTATE, Wip_State.EQPT_END))
            {
                //설비 완공 LOT은 취소할 수 없습니다.
                Util.MessageValidation("SFU1671");
                return false;
            }

            return true;
        }
        #endregion

        #region [### 작업종료 체크 ###]
        private bool ValidateEqpEnd()
        {
            // START LOT CANCEL COMMON VALIDATION
            if (string.IsNullOrEmpty(LOTID))
            {
                //작업취소 할 LOT이 선택되지 않았습니다.
                Util.MessageValidation("SFU1852");
                return false;
            }

            if (EQUIPMENT_COMBO.SelectedIndex < 1)
            {
                //설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (!string.Equals(LOTSTATE, Wip_State.PROC))
            {
                //Util.Alert("장비완료 할 수 있는 LOT상태가 아닙니다.");
                Util.MessageValidation("SFU1866");
                return false;
            }
            return true;
        }
        #endregion

        #region [### Tray ###]
        /// <summary>
        /// 생산반제품 - Tray 생성 Form Load시 체크
        /// </summary>
        /// <returns></returns>
        public bool ValidateStartTrayCreate()
        {
            // START LOT COMMON VALIDATION
            DataRow workDataRow = WORKORDER.GetSelectWorkOrderRow();

            if (workDataRow == null)
            {
                //작업지시를 선택하세요.
                Util.MessageValidation("SFU1443");
                return false;
            }

            if (string.IsNullOrEmpty(LOTID))
            {
                //Lot을 선택하세요.
                Util.MessageValidation("SFU1105");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 생산반제품 - Tray 조회 Form Load시 체크
        /// </summary>
        /// <returns></returns>
        public bool ValidateStartTraySearch()
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                Util.MessageValidation("SFU1105");       //Lot을 선택하세요.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tray 이동
        /// </summary>
        /// <returns></returns>
        public bool ValidateMoveTraySearch()
        {
            ////// START LOT COMMON VALIDATION
            ////DataRow workDataRow = WORKORDER.GetSelectWorkOrderRow();

            ////if (workDataRow == null)
            ////{
            ////    //Util.Alert("작업지시가 선택되지 않았습니다.");
            ////    Util.Alert("SFU1443");       //작업지시를 선택하세요.
            ////    return false;
            ////}

            if (string.IsNullOrEmpty(LOTID))
            {
                Util.MessageValidation("SFU1105");       //Lot을 선택하세요.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tray 삭제 - Validation 
        /// </summary>
        /// <returns></returns>
        private bool CanTrayDelete()
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");

            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            DataRow workDataRow = WORKORDER.GetSelectWorkOrderRow();
            if (workDataRow == null)
            {
                //Util.Alert("선택된 W/O가 없습니다.");
                Util.MessageValidation("SFU1635");
                return false;
            }

            return true;
        }

        /// <summary>
        /// TRAY확정취소 - Validation 
        /// </summary>
        /// <returns></returns>
        private bool CanConfirmCancel()
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tray확정
        /// </summary>
        /// <returns></returns>
        private bool CanTrayConfirm()
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");

            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "CELLQTY")), out dTmp))
            {
                if (dTmp == 0)
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

            string sRet = string.Empty;
            string sMsg = string.Empty;

            // Tray 현재 작업중인지 여부 확인.
            GetTrayInfo(out sRet, out sMsg);

            if (sRet.Equals("NG"))
            {
                Util.MessageValidation(sMsg);
                return false;
            }
            else if (sRet.Equals("EXCEPTION"))
                return false;

            return true;
        }

        /// <summary>
        /// Tray 특이사항
        /// </summary>
        /// <returns></returns>
        private bool CanSaveTray()
        {
            if (string.IsNullOrEmpty(LOTID))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(_dgProdCellChange, "CHK");

            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(_dgProdCellChange.Rows[iRow].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            return true;
        }

        private bool CanOutTraySplSave()
        {
            bool bRet = false;

            if (string.IsNullOrEmpty(LOTID))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            if (_ckOutTraySplChange.IsChecked.HasValue && (bool)_ckOutTraySplChange.IsChecked)
            {
                if (_ckOutTraySplReasonChange.SelectedValue == null || _ckOutTraySplReasonChange.SelectedValue == null || _ckOutTraySplReasonChange.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return bRet;
                }

                if (_txOutTrayReamrkChange.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 입력 하세요.");
                    Util.MessageValidation("SFU1590");
                    return bRet;
                }
            }
            else
            {
                if (!_txOutTrayReamrkChange.Text.Trim().Equals(""))
                {
                    //Util.Alert("비고를 삭제 하세요.");
                    Util.MessageValidation("SFU1589");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #endregion

        #region [Func]

        /// <summary>
        /// 조회
        /// </summary>
        public void Refresh()
        {
            // BASEFROM REFRESH
            GetWorkOrder();
            GetProductLot();

            // CLEAR
            ResultClearControls();
            DataCollectClearControls();
        }

        /// <summary>
        /// 조회 - WORKORDER
        /// </summary>
        private void GetWorkOrder()
        {
            // SELECT WORKORDER
            if (WORKORDER == null)
                return;

            WORKORDER.EQPTSEGMENT = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
            WORKORDER.EQPTID = EQUIPMENT_COMBO.SelectedValue.ToString();
            WORKORDER.PROCID = _procId;

            WORKORDER.GetWorkOrder();
        }

        /// <summary>
        /// 선택된 W/O
        /// </summary>
        /// <returns></returns>
        public DataRow GetSelectWorkOrderInfo()
        {
            // SELECT WORKORDER DATAROW
            if (WORKORDER == null)
                return null;

            WORKORDER.EQPTSEGMENT = EQUIPMENTSEGMENT_COMBO.SelectedValue.ToString();
            WORKORDER.EQPTID = EQUIPMENT_COMBO.SelectedValue.ToString();
            WORKORDER.PROCID = _procId;

            return WORKORDER.GetSelectWorkOrderRow();
        }

        private void RemoveGridRow(C1DataGrid datagrid)
        {
            // REMOVE ROW
            for (int i = datagrid.GetRowCount(); i >= 0; i--)
            {
                DataTable dt = DataTableConverter.Convert(datagrid.ItemsSource);
                if (_util.GetDataGridCheckValue(datagrid, "CHK", i) == true)
                {
                    dt.Rows[i].Delete();
                }
                datagrid.ItemsSource = DataTableConverter.Convert(dt);
            }

        }

        private bool GetLotInfo(Object SelectedItem)
        {
            try
            {
                ////(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;

                // SET DETAIL INFO
                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return false;

                string sWOID = String.Empty;
                double dEqpQty = 0;
                double dDefectQty = 0;

                if (!string.IsNullOrEmpty(Convert.ToString(rowview["WOID"])))
                {
                    sWOID = Convert.ToString(rowview["WOID"]);
                }
                else
                {
                    DataRow workOrder = GetSelectWorkOrderInfo();
                    if (workOrder != null)
                        sWOID = Convert.ToString(workOrder["WOID"]);
                }

                if (string.IsNullOrEmpty(sWOID))
                {
                    //WORK ORDER ID가 지정되지 않거나 없습니다.
                    Util.MessageValidation("SFU1442");
                    return false;
                }

                // SET GRID
                List<string> workOrderBind = new List<string>();
                workOrderBind.Add(Convert.ToString(rowview["LOTID"]));
                workOrderBind.Add(Convert.ToString(rowview["PRJT_NAME"]));
                workOrderBind.Add(Convert.ToString(rowview["PRODID"]));
                workOrderBind.Add(Convert.ToString(rowview["PRODNAME"]));

                GridDataBinding(LOTINFO_GRID, workOrderBind, true);

                dEqpQty = string.IsNullOrWhiteSpace(rowview["EQPQTY"].ToString()) ? 0 : Convert.ToDouble(rowview["EQPQTY"].ToString());
                dDefectQty = string.IsNullOrWhiteSpace(rowview["DEFECTQTY"].ToString()) ? 0 : Convert.ToDouble(rowview["DEFECTQTY"].ToString());

                (grdResult.Children[0] as UcAssyResult).txtWorkOrder.Text = sWOID;                                                              // 현재 SELECT된 WOID
                (grdResult.Children[0] as UcAssyResult).txtLotState.Text = Convert.ToString(rowview["WIPSNAME"]);                               // WIPSTATE명
                (grdResult.Children[0] as UcAssyResult).txtWipQty.Value = dEqpQty + dDefectQty;                                                 // 생산량 (장비완료+장비불량) 
                (grdResult.Children[0] as UcAssyResult).txtGoodQty.Value = dEqpQty;                                                             // 현재 재공수량(GOODQTY)
                (grdResult.Children[0] as UcAssyResult).txtDefectQty.Value = dDefectQty;                                                        // 장비불량수량

                // 수정 필요 NOW_CALDATE, CALDATE_LOT
                (grdResult.Children[0] as UcAssyResult).dtpCaldate.Text = DateTime.Now.ToString("yyyy-MM-dd");                                  // 현재날자
                (grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDateTime = DateTime.Now;
                dtCaldate = DateTime.Now;

                (grdResult.Children[0] as UcAssyResult).txtStartTime.Text = Convert.ToString(rowview["WIPDTTM_ST"]);                            // 시작시간

                // 수정필요 : 장비완료시간 EQPT_END_DTTM
                (grdResult.Children[0] as UcAssyResult).txtEndTime.Text = Util.NVC(rowview["WIPDTTM_ED"]);  
                // 수정 필요
                (grdResult.Children[0] as UcAssyResult).txtRemark.AppendText(Convert.ToString(rowview["REMARK"]));                             // 특이사항

                // SET WORK ORDER ID && LOTSTATE
                WOID = sWOID;
                LOTID = Convert.ToString(rowview["LOTID"]);
                LOTSTATE = Convert.ToString(rowview["WIPSTAT"]);

                // SET COMBO BIND
                ((C1.WPF.DataGrid.DataGridComboBoxColumn)INPUTMTRL_GRID.Columns["ITEMID"]).ItemsSource = null;

                DataTable materialTable = GetMaterialList(Convert.ToString(sWOID), "");
                if (materialTable.Rows.Count > 0)
                    ((C1.WPF.DataGrid.DataGridComboBoxColumn)INPUTMTRL_GRID.Columns["ITEMID"]).ItemsSource = DataTableConverter.Convert(materialTable);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            ////finally
            ////{
            ////    (grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged += OndtpCaldate_SelectedDataTimeChanged;
            ////}
        }

        public void GridDataBinding(C1DataGrid dataGrid, List<String> bindValues, bool isNewFlag)
        {
            // SET DATAGRID DATA
            if (dataGrid.ItemsSource == null)
            {
                DataTable colDt = new DataTable();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                    colDt.Columns.Add(dataGrid.Columns[i].Name);

                dataGrid.ItemsSource = DataTableConverter.Convert(colDt);
            }

            DataTable inputDt = ((DataView)dataGrid.ItemsSource).Table;
            DataRow inputRow = inputDt.NewRow();

            for (int i = 0; i < inputDt.Columns.Count; i++)
                inputRow[inputDt.Columns[i].Caption] = bindValues[i];

            // ADD DATA
            inputDt.Rows.Add(inputRow);

            if (isNewFlag)
                dataGrid.ItemsSource = DataTableConverter.Convert(inputDt);
        }

        public void SetHalfProdAddGrid(DataTable dt)
        {
            // ADD GRID
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bool isDupplicate = false;
                for (int j = 0; j < INPUTPRD_GRID.Rows.Count; j++)
                {
                    if (string.Equals(Convert.ToString(dt.Rows[i]["LOTID"]), Util.NVC(DataTableConverter.GetValue(INPUTPRD_GRID.Rows[j].DataItem, "LOTID"))))
                    {
                        isDupplicate = true;
                        break;
                    }
                }

                if (isDupplicate == false)
                {
                    List<string> bindValue = new List<string>();
                    bindValue.Add("True");
                    bindValue.Add("0"); // SEQ지정해야 함 (현재는 0으로 초기화
                    bindValue.Add(Convert.ToString(dt.Rows[i]["LOTID"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["RUNCARDID"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["ELECTRODECODE"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["ELECTRODETYPE"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["PRJT_NAME"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["PRODID"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["PRODNAME"]));
                    bindValue.Add(Convert.ToString(dt.Rows[i]["WIPQTY"]));
                    bindValue.Add("0");
                    bindValue.Add("");
                    bindValue.Add(DateTime.Now.ToString());

                    GridDataBinding(INPUTPRD_GRID, bindValue, false);
                }
            }
            /*
            // INIT TEXTBOX
            if (!string.IsNullOrEmpty((grdDataCollect.Children[0] as UcAssyDataCollect).txtProductLotId.Text))
                (grdDataCollect.Children[0] as UcAssyDataCollect).txtProductLotId.Text = "";
                */
        }

        /// <summary>
        /// 생산 반제품 - 자동 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;

            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    //FrameOperation.PrintFrameMessage(DateTime.Now.ToLongTimeString() + ">>" + dpcTmr.Interval.TotalSeconds.ToString());

                    if (dpcTmr != null)
                        dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0)
                        return;

                    if (string.IsNullOrEmpty(LOTID))
                        return;

                    GetProductCellList();
                    GetSpecialTrayInfo();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr != null && dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));

        }

        /// <summary>
        /// Tray 버튼 활성/비활성
        /// </summary>
        /// <param name="dgRow"></param>
        private void SetOutTrayButtonEnable(C1.WPF.DataGrid.DataGridRow dgRow)
        {
            /*
            try
            {
                if (dgRow != null)
                {
                    // 확정 시 저장, 삭제 버튼 비활성화
                    if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
                    {
                        if (_procId.Equals(Process.WINDING))
                        {
                            CELL_CREATEWNM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWNM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWNM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWNM.IsEnabled = true;
                        }
                        else if (_procId.Equals(Process.WASHING) && _micro)
                        {
                            CELL_CREATEWSM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWSM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancelWSM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWSM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWSM.IsEnabled = true;
                        }
                        else
                        {
                            CELL_CREATE_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemove.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancel.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirm.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayMove.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSave.IsEnabled = true;
                        }
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        if (_procId.Equals(Process.WINDING))
                        {
                            CELL_CREATEWNM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWNM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWNM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWNM.IsEnabled = false;
                        }
                        else if (_procId.Equals(Process.WASHING) && _micro)
                        {
                            CELL_CREATEWSM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWSM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancelWSM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWSM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWSM.IsEnabled = false;
                        }
                        else
                        {
                            CELL_CREATE_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemove.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancel.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirm.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayMove.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSave.IsEnabled = false;
                        }
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN ")) // 활성화입고
                    {
                        if (_procId.Equals(Process.WINDING))
                        {
                            CELL_CREATEWNM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWNM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWNM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWNM.IsEnabled = false;
                        }
                        else if (_procId.Equals(Process.WASHING) && _micro)
                        {
                            CELL_CREATEWSM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWSM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancelWSM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWSM.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWSM.IsEnabled = false;
                        }
                        else
                        {
                            CELL_CREATE_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemove.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancel.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirm.IsEnabled = false;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayMove.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSave.IsEnabled = false;
                        }
                    }
                    else
                    {
                        if (_procId.Equals(Process.WINDING))
                        {
                            CELL_CREATEWNM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWNM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWNM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWNM.IsEnabled = true;
                        }
                        else if (_procId.Equals(Process.WASHING) && _micro)
                        {
                            CELL_CREATEWSM_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWSM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancelWSM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWSM.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWSM.IsEnabled = true;
                        }
                        else
                        {
                            CELL_CREATE_BUTTON.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemove.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancel.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirm.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayMove.IsEnabled = true;
                            (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSave.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    if (_procId.Equals(Process.WINDING))
                    {
                        CELL_CREATEWNM_BUTTON.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWNM.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWNM.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWNM.IsEnabled = true;
                    }
                    else if (_procId.Equals(Process.WASHING) && _micro)
                    {
                        CELL_CREATEWSM_BUTTON.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemoveWSM.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancelWSM.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirmWSM.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSaveWSM.IsEnabled = true;
                    }
                    else
                    {
                        CELL_CREATE_BUTTON.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayRemove.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayCancel.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayConfirm.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnTrayMove.IsEnabled = true;
                        (grdDataCollect.Children[0] as UcAssyDataCollect).btnOutSave.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            */
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
