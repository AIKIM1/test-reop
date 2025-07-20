/*************************************************************************************
 Created Date : 2020.10.16
      Creator : 신광희
   Decription : 조립 공정진척(CNB 2동) - 생산실적영역 UserControl 
--------------------------------------------------------------------------------------
 [Change History]
 2021.09.02  조영대 : 폴딩 추가
 2022/11/29  오화백 : 스태킹 완성LOT 자동 Hold 기증 개발
 2023.01.03  윤지해 : C20221212-000186 LOT완료 UI abnormal (공정 진행 화면)-folding
 2023.02.24  김용군 : ESHM 증설 - AZS-STAKING 대응(A84000공정에 대해서 Stack별 불량정보, Stack별 완성수량 Tab_Page추가)
 2023.03.21  김용군 : ESHM 증설 - 설비불량정보Tab과 동일하게 Stack Machine별 불량그룹, 설비불량명, 불량수량, 최근수집일시 정보로 변경
 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
 2023.11.22  안유수 : E20231006-001025 특별 TARY 적용 관련 사유 콤보박스 추가 및 GROUP ID 발번 기능 추가
 2024.01.08  남재현 : STK 특별 Tray 설정 추가. (특이,내역 컬럼 추가 / 동 코드 조회 추가 / 특별 STK TRAY 관리 동 조회, 특별 TRAY 적용 Grid 추가 / 특별 Stacked Cell Tray 조회 / 특별 TRAY 컬러 적용 / 특별 Tray 정보 조회 / 특별 Tray 적용 Grid Btn Event 추가 / 특별 TRAY Validation 추가 / 특별 TRAY 적용 BR 추가)
 2024.08.27  안유수 : E20240810-001560 dgOutStacking_MouseDoubleClick 이벤트 제거
 2024.03.21  오화백 : Lami(하프라미 포함), Stacking  Dispach가 N 일경우 수량 수정안되도록 수정
 2025.05.20  천진수 : ESHG 증설 조립공정진척 DNC공정추가 
 2025.05.26  권준서 : PKG 트레이 상태 추가(CELL_MISMATCH)
 2025-05-28 김선영 : AZS 생산반제품 탭 - ESHG 특별TRAY 설정 Visible = Collapsed 처리  
 2025-06-30 이주원:  CAT_UP_0618[E20240906-001741] -outlot_confirmation_cancel_error 수정
**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Popup;
using System.Collections.Generic;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using System.Linq;

namespace LGC.GMES.MES.ASSY005.Controls
{
    /// <summary>
    /// UcAssemblyProductionResult.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssemblyProductionResult : UserControl
    {
        #region Declaration


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public DataTable DtEquipment { get; set; }
        public DataRowView DvProductLot { get; set; }
        public string EquipmentSegmentCode { get; set; }
        public string EquipmentSegmentName { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public string EquipmentCode { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentGroupCode { get; set; }
        public string LdrLotIdentBasCode { get; set; }
        public string UnldrLotIdentBasCode { get; set; }
        public bool IsMagzinePrintVisible { get; set; } = false;

        public bool IsProductLotRefreshFlag = false;

        public bool IsEquipmentTreeRefreshFlag = false;


        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        private COM001_314_HIST _winInputHistTool = new COM001_314_HIST();

        private DateTime dtCaldate;
        private string sCaldate = string.Empty;
        private string _equipmentConfigType = string.Empty;
        private bool _isSearchAll = false;
        private bool _isEdcAutoSelectTime = false;
        private bool _isOutStackingAutoSelectTime = false;
        private bool _isOutLaminationAutoSelectTime = false;
        private bool _isOutPackagingAutoSelectTime = false;

        // 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
        private bool _isAssyStackCellInfoNoUse = false;

        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimerEdc = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimerOutLamination = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimerOutStacking = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherTimerOutPackaging = new System.Windows.Threading.DispatcherTimer();

        SolidColorBrush edcBrush = new SolidColorBrush(Colors.DarkOrange);
        SolidColorBrush myAnimatedBrush = new SolidColorBrush(Colors.Fuchsia);
        Storyboard sbBcrWarning = new Storyboard();
        SolidColorBrush greenBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDAFFF6"));

        private struct PreviousValues
        {
            public string PreviousTray;

            public PreviousValues(string tray)
            {
                PreviousTray = tray;
            }
        }
        private PreviousValues _previousValues = new PreviousValues("");

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        public UcAssemblyProductionResult()
        {
            InitializeComponent();

            InitializeControls();
            SetControl();
            SetButtons();
            //SetControlVisibility();
        }

        #endregion

        #region Initialize

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitializeCombo();

            this.RegisterName("BCR_WARNING", edcBrush);
            this.RegisterName("myAnimatedBrush", myAnimatedBrush);

            rdoTraceUse.IsChecked = true;
            rdoTraceNotUse.IsEnabled = false;   // 오창 자동차는 모두 trace 모드..

            // EDC 조회 Timer
            if (_dispatcherTimerEdc != null)
            {
                int second = 0;

                if (cboEdcAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboEdcAutoSearch.SelectedValue.GetString()))
                    second = int.Parse(cboEdcAutoSearch.SelectedValue.ToString());

                _dispatcherTimerEdc.Tick -= dispatcherTimerEdc_Tick;
                _dispatcherTimerEdc.Tick += dispatcherTimerEdc_Tick;
                _dispatcherTimerEdc.Interval = new TimeSpan(0, 0, second);
            }

            // 생산 반제품(라미공정) 조회 Timer
            if (_dispatcherTimerOutLamination != null)
            {
                int iSec = 0;

                if (cboAutoSearchOutLamination?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearchOutLamination.SelectedValue.GetString()))
                    iSec = int.Parse(cboAutoSearchOutLamination.SelectedValue.ToString());

                _dispatcherTimerOutLamination.Tick += dispatcherTimerOutLamination_Tick;
                _dispatcherTimerOutLamination.Interval = new TimeSpan(0, 0, iSec);
            }

            // 생산 반제품(스태킹공정) 조회 Timer
            if (_dispatcherTimerOutStacking != null)
            {
                int iSec = 0;

                if (cboAutoSearchOutStacking?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearchOutStacking.SelectedValue.GetString()))
                    iSec = int.Parse(cboAutoSearchOutStacking.SelectedValue.ToString());

                _dispatcherTimerOutStacking.Tick += dispatcherTimerOutStacking_Tick;
                _dispatcherTimerOutStacking.Interval = new TimeSpan(0, 0, iSec);
            }

            // 생산 반제품(패키징공정) 조회 Timer
            if (_dispatcherTimerOutPackaging != null)
            {
                int iSec = 0;

                if (cboAutoSearchOutPackaging?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearchOutPackaging.SelectedValue.GetString()))
                    iSec = int.Parse(cboAutoSearchOutPackaging.SelectedValue.ToString());

                _dispatcherTimerOutPackaging.Tick += dispatcherTimerOutPackaging_Tick;
                _dispatcherTimerOutPackaging.Interval = new TimeSpan(0, 0, iSec);
            }

            // 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
            GetAssyStackCellInfoNoUse();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (_dispatcherTimerEdc != null) _dispatcherTimerEdc.Stop();
                //if (_dispatcherTimerOutLamination != null) _dispatcherTimerOutLamination.Stop();
                //if (_dispatcherTimerOutStacking != null) _dispatcherTimerOutStacking.Stop();
                //if (_dispatcherTimerOutPackaging != null) _dispatcherTimerOutPackaging.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void InitializeControls()
        {
            this.RegisterName("greenBrush", greenBrush);

            if (_dispatcherTimerEdc != null)
            {
                _dispatcherTimerEdc.Stop();
                cboEdcAutoSearch.SelectedValueChanged -= cboEdcAutoSearch_SelectedValueChanged;
                cboEdcAutoSearch.SelectedIndex = 0;
                cboEdcAutoSearch.SelectedValueChanged += cboEdcAutoSearch_SelectedValueChanged;
            }

            if (_dispatcherTimerOutLamination != null)
            {
                _dispatcherTimerOutLamination.Stop();
                cboAutoSearchOutLamination.SelectedValueChanged -= cboAutoSearchOutLamination_SelectedValueChanged;
                cboAutoSearchOutLamination.SelectedIndex = 0;
                cboAutoSearchOutLamination.SelectedValueChanged += cboAutoSearchOutLamination_SelectedValueChanged;
            }

            if (_dispatcherTimerOutStacking != null)
            {
                _dispatcherTimerOutStacking.Stop();
                cboAutoSearchOutStacking.SelectedValueChanged -= cboAutoSearchOutStacking_SelectedValueChanged;
                cboAutoSearchOutStacking.SelectedIndex = 0;
                cboAutoSearchOutStacking.SelectedValueChanged += cboAutoSearchOutStacking_SelectedValueChanged;
            }

            if (_dispatcherTimerOutPackaging != null)
            {
                _dispatcherTimerOutPackaging.Stop();
                cboAutoSearchOutPackaging.SelectedValueChanged -= cboAutoSearchOutPackaging_SelectedValueChanged;
                cboAutoSearchOutPackaging.SelectedIndex = 0;
                cboAutoSearchOutPackaging.SelectedValueChanged += cboAutoSearchOutPackaging_SelectedValueChanged;
            }
        }

        private void InitializeCombo()
        {
            CommonCombo combo = new CommonCombo();

            // 자동 조회 시간 Combo
            string[] sFilter = { "EDC_BCD_RATE_INTERVAL" };
            combo.SetCombo(cboEdcAutoSearch, CommonCombo.ComboStatus.NA, sFilter: sFilter, sCase: "COMMCODE");

            if (cboEdcAutoSearch != null && cboEdcAutoSearch.Items != null && cboEdcAutoSearch.Items.Count > 0)
                cboEdcAutoSearch.SelectedIndex = cboEdcAutoSearch.Items.Count - 1;

            // 자동 조회 시간 Combo
            string[] sFilter1 = { "SECOND_INTERVAL" };
            combo.SetCombo(cboAutoSearchOutLamination, CommonCombo.ComboStatus.NA, sFilter: sFilter1, sCase: "COMMCODE");

            if (cboAutoSearchOutLamination != null && cboAutoSearchOutLamination.Items != null && cboAutoSearchOutLamination.Items.Count > 0)
                cboAutoSearchOutLamination.SelectedIndex = 0;

            // 자동 조회 시간 Combo
            string[] sFilter2 = { "SECOND_INTERVAL" };
            combo.SetCombo(cboAutoSearchOutStacking, CommonCombo.ComboStatus.NA, sFilter: sFilter2, sCase: "COMMCODE");

            if (cboAutoSearchOutStacking != null && cboAutoSearchOutStacking.Items != null && cboAutoSearchOutStacking.Items.Count > 0)
                cboAutoSearchOutStacking.SelectedIndex = 0;

            // 자동 조회 시간 Combo
            string[] sFilter3 = { "SECOND_INTERVAL" };
            combo.SetCombo(cboAutoSearchOutPackaging, CommonCombo.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            if (cboAutoSearchOutPackaging != null && cboAutoSearchOutPackaging.Items != null && cboAutoSearchOutPackaging.Items.Count > 0)
                cboAutoSearchOutPackaging.SelectedIndex = 0;

            // 특별 TRAY  사유 Combo
            String[] sFilter4 = { "SPCL_RSNCODE" };
            combo.SetCombo(cboOutTraySplReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilter4, sCase: "COMMCODE_WITHOUT_CODE");

            if (cboOutTraySplReason != null && cboOutTraySplReason.Items != null && cboOutTraySplReason.Items.Count > 0)
                cboOutTraySplReason.SelectedIndex = 0;

            //HOLD 사유
            String[] sFilter5 = { LoginInfo.CFG_AREA_ID, "HOLD_LOT" };
            combo.SetCombo(cboHoldCode, CommonCombo.ComboStatus.SELECT, sFilter: sFilter5, sCase: "CBO_AREA_ACTIVITIREASON");
        }

        private void SetButtons()
        {
            //ButtonProductionUpdate = btnProductionUpdate;
        }

        private void SetControl()
        {
            recEquipment.Fill = greenBrush;
            txtEquipment.Text = "[" + EquipmentCode + "] " + EquipmentName;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "greenBrush");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        private void SetInputHistToolWindow()
        {
            if (grdInputHistTool.Children.Count == 0)
            {
                _winInputHistTool.FrameOperation = FrameOperation;

                _winInputHistTool._UCParent = this;
                grdInputHistTool.Children.Add(_winInputHistTool);
            }
        }

        private void CheckEDCVisibity()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["PROCID"] = ProcessCode;
                dtRow["EQSGID"] = EquipmentSegmentCode;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("BCD_READ_RATE_DISP_FLAG"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["BCD_READ_RATE_DISP_FLAG"]).Equals("Y"))
                    {
                        tabEDCBCRInfo.Visibility = Visibility.Visible;

                        if (_dispatcherTimerEdc != null && _dispatcherTimerEdc.Interval.TotalSeconds > 0)
                            _dispatcherTimerEdc.Start();
                    }
                    else
                    {
                        tabEDCBCRInfo.Visibility = Visibility.Collapsed;
                        HideBcrWarning();
                        if (_dispatcherTimerEdc != null) _dispatcherTimerEdc.Stop();
                    }
                }
                else
                {
                    tabEDCBCRInfo.Visibility = Visibility.Collapsed;
                    HideBcrWarning();
                    if (_dispatcherTimerEdc != null) _dispatcherTimerEdc.Stop();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetControlClear()
        {
            _isSearchAll = false;
            // NND 생산실적 UI 컨트롤 
            txtLotId.Text = string.Empty;
            txtCarrierId.Text = string.Empty;
            txtProdId.Text = string.Empty;
            txtProjectName.Text = string.Empty;
            txtLotStatus.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            txtWorkMinute.Text = string.Empty;
            txtParent2_M.Text = string.Empty;
            txtParent2.Text = string.Empty;
            rtxRemark.Document.Blocks.Clear();
            txtInputQty.Value = 0;
            txtGoodQty.Value = 0;
            sCaldate = string.Empty;
            // Lamination, Stacking, Packaging 생산실적 UI 컨트롤 
            textLotId.Text = string.Empty;
            textWorkTime.Text = string.Empty;
            textProdId.Text = string.Empty;
            textProjectName.Text = string.Empty;
            textInputQty.Value = 0;
            textGoodQty.Value = 0;

            txtOutLaminationCnt.Text = string.Empty;
            txtOutLaminationCa.Text = string.Empty;
            txtOutStackingCa.Text = string.Empty;
            txtOutStackingBoxQty.Text = string.Empty;

            if (_winInputHistTool.TextToolID != null)
            {
                _winInputHistTool.TextToolID.Text = string.Empty;
            }

            txtOutTrayReamrk.Text = string.Empty;
            if ((bool) chkOutTraySpl.IsChecked)
                chkOutTraySpl.IsChecked = false;

            Util.gridClear(dgDetail);
            // Grid DataCollect Left C1DataGrid
            Util.gridClear(dgFaulty);
            Util.gridClear(dgDefect);
            Util.gridClear(dgEqpFaulty);
            // 김용군
            Util.gridClear(dgStackDefect);

            // Grid DataCollect Right C1DataGrid
            Util.gridClear(dgOutLamination);
            Util.gridClear(dgOutStacking);
            Util.gridClear(dgOutPackaging);
            Util.gridClear(dgInputLoss);
            Util.gridClear(dgEDCInfo);
            Util.gridClear(dgReInput);

            // 김용군
            Util.gridClear(dgStackOutQty);
        }

        private void ClearLotInfo()
        {

        }

        private void SetControlVisibility()
        {
            SetTabVisibility();
        }

        private void SetTabVisibility()
        {
            tabDefectNotching.Visibility = Visibility.Collapsed;
            tabDefect.Visibility = Visibility.Collapsed;
            tabEquipmentFaulty.Visibility = Visibility.Collapsed;
            tabOutLaminationProduct.Visibility = Visibility.Collapsed;
            tabOutStackingProduct.Visibility = Visibility.Collapsed;
            tabOutPackagingProduct.Visibility = Visibility.Collapsed;

            // 김용군
            tabStackDefect.Visibility = Visibility.Collapsed;
            tabStackOutQty.Visibility = Visibility.Collapsed;

            tabRemark.Visibility = Visibility.Collapsed;
            tabToolInputHist.Visibility = Visibility.Collapsed;
            tabEDCBCRInfo.Visibility = Visibility.Collapsed;
            tabInputLoss.Visibility = Visibility.Collapsed;
            tabReInput.Visibility = Visibility.Collapsed;
            tabPkgSupplyCarrier.Visibility = Visibility.Collapsed;

            grdBcrWarning.Visibility = Visibility.Collapsed;
            grdTrayLegend.Visibility = Visibility.Collapsed;
            dgReInput.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
            dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;

            if (ProcessCode == Process.NOTCHING)
            {
                grdLotInfo.Visibility = Visibility.Visible;
                grdLotInfoCommon.Visibility = Visibility.Collapsed;
                grdBcrWarning.Visibility = Visibility.Visible;

                tabDefectNotching.Visibility = Visibility.Visible;
                tabEquipmentFaulty.Visibility = Visibility.Visible;
                tabRemark.Visibility = Visibility.Visible;
                tabToolInputHist.Visibility = Visibility.Visible;
                tabEDCBCRInfo.Visibility = Visibility.Visible;
            }
            else
            {
                grdLotInfo.Visibility = Visibility.Collapsed;
                grdLotInfoCommon.Visibility = Visibility.Visible;

                tabDefect.Visibility = Visibility.Visible;
                tabEquipmentFaulty.Visibility = Visibility.Visible;

                // 김용군
                if (ProcessCode == Process.AZS_STACKING)
                {
                    tabStackDefect.Visibility = Visibility.Visible;
                    tabStackOutQty.Visibility = Visibility.Visible;
                    tabOutStackingProduct.Visibility = Visibility.Visible;
                    tabToolInputHist.Visibility = Visibility.Visible;

                    grdSpclStkTranBtn.Visibility = Visibility.Collapsed;        //2025-05-28 AZS 생산반제품 탭 - ESHG 특별TRAY 설정 제외 
                }

                if (ProcessCode == Process.LAMINATION || ProcessCode == Process.AZS_ECUTTER || ProcessCode == Process.DNC)  // 250428 ESHG DNC공정추가 
                {
                    tabOutLaminationProduct.Visibility = Visibility.Visible;
                    tabInputLoss.Visibility = Visibility.Visible;
                    tabToolInputHist.Visibility = Visibility.Visible;
                    
                }
                else if (ProcessCode == Process.STACKING_FOLDING)
                {
                    tabOutStackingProduct.Visibility = Visibility.Visible;
                    tabToolInputHist.Visibility = Visibility.Visible;
                }
                else if (ProcessCode == Process.PACKAGING)
                {
                    tabOutPackagingProduct.Visibility = Visibility.Visible;
                    grdTrayLegend.Visibility = Visibility.Visible;
                    tabReInput.Visibility = Visibility.Visible;
                    tabToolInputHist.Visibility = Visibility.Visible;

                    // 동별 공통코드 항목에 RE_INPUT_DFCT_CODE 등록된 동만 보이도록 처리
                    if (CheckReInputDefectCode())
                    {
                        dgReInput.Columns["RESNNAME"].Visibility = Visibility.Visible;
                        dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgReInput.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                        dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                    }
                }
            }

            // 2023.08.21  강성묵: ESHM 증설 -완성 LOT => Remarks 컬럼 추가
            SetOutLotRemarkUse(ProcessCode);
        }

        private void SetFaulty(bool isMessageShow = true)
        {
            try
            {
                ShowLoadingIndicator();

                dgFaulty.EndEdit();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable inDEFECT_LOT = indataSet.Tables.Add("INRESN");
                inDEFECT_LOT.Columns.Add("LOTID", typeof(string));
                inDEFECT_LOT.Columns.Add("WIPSEQ", typeof(string));
                inDEFECT_LOT.Columns.Add("ACTID", typeof(string));
                inDEFECT_LOT.Columns.Add("RESNCODE", typeof(string));
                inDEFECT_LOT.Columns.Add("RESNQTY", typeof(double));
                inDEFECT_LOT.Columns.Add("RESNCODE_CAUSE", typeof(string));
                inDEFECT_LOT.Columns.Add("PROCID_CAUSE", typeof(string));
                inDEFECT_LOT.Columns.Add("RESNNOTE", typeof(string));
                inDEFECT_LOT.Columns.Add("DFCT_TAG_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("LANE_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("LANE_PTN_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("COST_CNTR_ID", typeof(string));
                inDEFECT_LOT.Columns.Add("A_TYPE_DFCT_QTY", typeof(int));
                inDEFECT_LOT.Columns.Add("C_TYPE_DFCT_QTY", typeof(int));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(newRow);

                for (int i = 0; i < dgFaulty.Columns.Count; i++)
                {
                    if (Util.NVC(dgFaulty.Columns[i].Name).StartsWith("DEFECTQTY"))
                    {
                        string sLot = dgFaulty.Columns[i].Header.ToString().Replace("[#]", "").Trim();
                        string sWipSeq = DvProductLot["WIPSEQ"].GetString();
                        string sColName = dgFaulty.Columns[i].Name.ToString();

                        if (string.IsNullOrEmpty(sWipSeq))
                        {
                            return;
                        }

                        for (int j = 0; j < dgFaulty.Rows.Count - dgFaulty.BottomRows.Count; j++)
                        {
                            newRow = null;

                            newRow = inDEFECT_LOT.NewRow();
                            newRow["LOTID"] = sLot;
                            newRow["WIPSEQ"] = sWipSeq;
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTID"));
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "RESNCODE"));
                            newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, sColName)).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, sColName)));
                            newRow["RESNCODE_CAUSE"] = "";
                            newRow["PROCID_CAUSE"] = "";
                            newRow["RESNNOTE"] = "";
                            newRow["LANE_QTY"] = 1;
                            newRow["LANE_PTN_QTY"] = 1;

                            if (Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                                newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgFaulty.Rows[j].DataItem, "COST_CNTR_ID"));
                            else
                                newRow["COST_CNTR_ID"] = "";

                            newRow["A_TYPE_DFCT_QTY"] = 0;
                            newRow["C_TYPE_DFCT_QTY"] = 0;
                            inDEFECT_LOT.Rows.Add(newRow);
                        }
                    }
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL_NT", "INDATA,INRESN", null, indataSet);

                if (isMessageShow)
                {
                    Util.MessageInfo("SFU1275");
                    GetFaultyData();
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

        private void SetDefect()
        {
            try
            {
                dgDefect.EndEdit();

                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;
                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = string.Empty;
                    newRow["PROCID_CAUSE"] = string.Empty;
                    newRow["RESNNOTE"] = string.Empty;
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).GetString() == "CHARGE_PROD_LOT")
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = string.Empty;
                    }
                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;
                    inDEFECT_LOT.Rows.Add(newRow);
                }

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetDefectInfo(DvProductLot);
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        IsProductLotRefreshFlag = true;
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

                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region Event

        private void txtCarrierId_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            if (string.IsNullOrEmpty(txtCarrierId.Text.Trim())) return;

            PopupCarrierHistory();
        }

        private void cboEdcAutoSearch_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherTimerEdc != null)
                {
                    _dispatcherTimerEdc.Stop();

                    int second = 0;

                    if (cboEdcAutoSearch != null && cboEdcAutoSearch.SelectedValue != null && !cboEdcAutoSearch.SelectedValue.ToString().Equals(""))
                        second = int.Parse(cboEdcAutoSearch.SelectedValue.ToString());

                    if (second == 0 && _isEdcAutoSelectTime)
                    {
                        _dispatcherTimerEdc.Interval = new TimeSpan(0, 0, second);
                        return;
                    }

                    if (second == 0)
                    {
                        _isEdcAutoSelectTime = true;
                        return;
                    }

                    _dispatcherTimerEdc.Interval = new TimeSpan(0, 0, second);
                    _dispatcherTimerEdc.Start();

                    _isEdcAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dispatcherTimerEdc_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (tabEDCBCRInfo.Visibility != Visibility.Visible) return;

                    if (dpcTmr != null) dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0) return;

                    GetBcrReadingRate(false);
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

        private void dispatcherTimerOutLamination_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (dpcTmr != null) dpcTmr.Stop();
                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0) return;
                    if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))return;

                    GetOutMagazine(DvProductLot);
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

        private void dispatcherTimerOutStacking_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (dpcTmr != null) dpcTmr.Stop();
                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0) return;
                    if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                    GetOutStackingProduct(DvProductLot);
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

        private void dispatcherTimerOutPackaging_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (dpcTmr != null) dpcTmr.Stop();
                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds == 0) return;
                    if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                    GetOutPackagingTray(DvProductLot);
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

        private void dgFaulty_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (!e.Cell.Column.Name.StartsWith("DEFECTQTY")) return;
            if (!CommonVerify.HasDataGridRow(dgDetail)) return;

            double dfct, loss, charge_prd;

            double sum = SumDefectQty(dgFaulty, e.Cell.Column.Name, out dfct, out loss, out charge_prd);

            double totSum = sum;

            if (DvProductLot["WIPSTAT"].GetString() != "WAIT")
            {
                double inputqty = txtInputQty.Value.GetDouble();

                if (inputqty < totSum)
                {
                    //생산량보다 불량이 크게 입력 될 수 없습니다.
                    Util.MessageValidation("SFU1608");

                    DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);

                    sum = SumDefectQty(dgFaulty, e.Cell.Column.Name, out dfct, out loss, out charge_prd);
                    totSum = sum;

                    //string xx = DataTableConverter.GetValue(dgDetail.Rows[0].DataItem, "DFCT_SUM").GetString();

                    DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DFCT_SUM", totSum.Equals(0) ? "0" : totSum.ToString("#,##0"));
                    DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_DEFECT_LOT", dfct.Equals(0) ? "0" : dfct.ToString("#,##0"));
                    DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_LOSS_LOT", loss.Equals(0) ? "0" : loss.ToString("#,##0"));
                    DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_CHARGE_PROD_LOT", charge_prd.Equals(0) ? "0" : charge_prd.ToString("#,##0"));

                    // 불량율
                    if (inputqty != 0)
                    {
                        DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DFCT_SUM", (totSum / inputqty).ToString("#0.##%"));
                        DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_DEFECT_LOT", (dfct / inputqty).ToString("#0.##%"));
                        DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_LOSS_LOT", (loss / inputqty).ToString("#0.##%"));
                        DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_CHARGE_PROD_LOT", (charge_prd / inputqty).ToString("#0.##%"));
                    }

                    //SetLotInfoCalc();
                    return;
                }

                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DFCT_SUM", totSum.Equals(0) ? "0" : totSum.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_DEFECT_LOT", dfct.Equals(0) ? "0" : dfct.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_LOSS_LOT", loss.Equals(0) ? "0" : loss.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_CHARGE_PROD_LOT", charge_prd.Equals(0) ? "0" : charge_prd.ToString("#,##0"));
                // 불량율
                if (inputqty != 0)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DFCT_SUM", (totSum / inputqty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_DEFECT_LOT", (dfct / inputqty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_LOSS_LOT", (loss / inputqty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_CHARGE_PROD_LOT", (charge_prd / inputqty).ToString("#0.##%"));
                }

                //SetLotInfoCalc();
            }
        }

        private void dgFaulty_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                for (int i = dgDetail.TopRows.Count; i < dgDetail.Rows.Count - 1; i++)
                {
                    string sColName = "DEFECTQTY" + (i + 1).ToString();
                    if (e.Column.Name.Equals(sColName))
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (sFlag == "Y")
                        {
                            e.Cancel = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(e.Cell.Column.Name).Equals("ACTNAME"))
                    {
                        string sFlag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));

                        if (sFlag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
            }));
        }

        private void dgFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

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

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(e.Cell.Column.Name).Equals("ACTNAME"))
                    {
                        string flag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                        if (flag == "Y")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

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

        private void dgDefect_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            // TO-BE 에서는 생산실적 영역이 존재하여, 자동계산 처리 로직을 추가.
            if (!CommonVerify.HasDataGridRow(dgDetail)) return;

            if (e.Cell.Column.Name.Equals("RESNQTY"))
            {
                double dfct, loss, charge_prd, goodQty;
                double sum = SumDefectQty(dgDefect, e.Cell.Column.Name, out dfct, out loss, out charge_prd);
                double totalSum = sum;
                double inputqty = textInputQty.Value.GetDouble();

                /*
                if (inputqty < totalSum)
                {
                    //생산량보다 불량이 크게 입력 될 수 없습니다.
                    Util.MessageValidation("SFU1608");
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.Name, 0);
                }
                */
                totalSum = SumDefectQty(dgDefect, e.Cell.Column.Name, out dfct, out loss, out charge_prd);

                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DFCT_SUM", totalSum.Equals(0) ? "0" : totalSum.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_DEFECT_LOT", dfct.Equals(0) ? "0" : dfct.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_LOSS_LOT", loss.Equals(0) ? "0" : loss.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_CHARGE_PROD_LOT", charge_prd.Equals(0) ? "0" : charge_prd.ToString("#,##0"));

                // 불량율
                if (Math.Abs(inputqty) > 0)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DFCT_SUM", (totalSum / inputqty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_DEFECT_LOT", (dfct / inputqty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_LOSS_LOT", (loss / inputqty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_CHARGE_PROD_LOT", (charge_prd / inputqty).ToString("#0.##%"));
                }

                //goodQty = inputqty - dfct - loss - charge_prd;
                //textGoodQty.Value = goodQty;
            }

        }

        private void btnDfctCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgDetail.ItemsSource);

                if (!CommonVerify.HasTableRow(dtTmp)) return;

                Dictionary<string, string> dList = new Dictionary<string, string>();
                dList.Add(Util.NVC(DvProductLot["LOTID"]), Util.NVC(DvProductLot["WIPSEQ"]));

                //foreach (DataRow dr in dtTmp.Rows)
                //{
                //    dList.Add(Util.NVC(dr["LOTID"]), Util.NVC(dr["WIPSEQ"]));
                //}

                Button bt = sender as Button;

                if (bt?.DataContext == null) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "DFCT_QTY_CHG_BLOCK_FLAG")), "Y"))
                    return;

                CMM_ASSY_DFCT_CELL_REG popupDefectCellInfo = new CMM_ASSY_DFCT_CELL_REG();
                popupDefectCellInfo.FrameOperation = FrameOperation;

                object[] parameters = new object[7];
                parameters[0] = dList;
                parameters[1] = dList;
                parameters[2] = EquipmentSegmentCode;
                parameters[3] = EquipmentCode;
                parameters[4] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNCODE"));
                parameters[5] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "ACTID"));
                parameters[6] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "RESNNAME"));
                C1WindowExtension.SetParameters(popupDefectCellInfo, parameters);

                popupDefectCellInfo.Closed += new EventHandler(popupDefectCellInfo_Closed);
                Dispatcher.BeginInvoke(new Action(() => popupDefectCellInfo.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDefectCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button?.DataContext == null) return;

                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY)) return;
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(button.DataContext, "DFCT_QTY_CHG_BLOCK_FLAG")), "Y"))
                    return;

                CMM_ASSY_DFCT_CELL_REG popDefectCell = new CMM_ASSY_DFCT_CELL_REG();
                popDefectCell.FrameOperation = FrameOperation;

                object[] parameters = new object[7];
                parameters[0] = DvProductLot["LOTID"].GetString();
                parameters[1] = DvProductLot["WIPSEQ"].GetString();
                parameters[2] = EquipmentSegmentCode;
                parameters[3] = EquipmentCode;
                parameters[4] = Util.NVC(DataTableConverter.GetValue(button.DataContext, "RESNCODE"));
                parameters[5] = Util.NVC(DataTableConverter.GetValue(button.DataContext, "ACTID"));
                parameters[6] = Util.NVC(DataTableConverter.GetValue(button.DataContext, "RESNNAME"));
                C1WindowExtension.SetParameters(popDefectCell, parameters);

                //popDefectCell.Closed += new EventHandler(popDefectCell_Closed);
                Dispatcher.BeginInvoke(new Action(() => popDefectCell.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupDefectCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DFCT_CELL_REG window = sender as CMM_ASSY_DFCT_CELL_REG;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
                //GetEqpFaultyData();
            }
        }

        private void btnSearchEqpFaulty_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgEqpFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (sender == null) return;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null) return;

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(e.Cell.Column.Name).Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // 합계 컬럼 색 변경.
                        if (!Util.NVC(e.Cell.Column.Name).Equals("EQPT_DFCT_SUM_GR_NAME") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_DFCT_GR_SUM_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgEqpFaulty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);
                if (sender == null) return;

                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;
                
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_ASSY_EQPT_DFCT_CELL_INFO popupDefectCellInfo = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
                            popupDefectCellInfo.FrameOperation = FrameOperation;

                            object[] parameters = new object[3];
                            parameters[0] = DvProductLot["LOTID"].GetString();
                            parameters[1] = EquipmentCode;
                            parameters[2] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID"));
                            C1WindowExtension.SetParameters(popupDefectCellInfo, parameters);

                            //popupDefectCellInfo.Closed += new EventHandler(wndEqptDfctCell_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => popupDefectCellInfo.ShowModal()));

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqpFaulty_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgEqpFaulty.TopRows.Count; i < dgEqpFaulty.Rows.Count; i++)
                {
                    if (dgEqpFaulty.Rows[i].DataItem.GetType() == typeof(DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") || Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
                        {
                            if (bStrt)
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));
                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                                idxS = i;

                                if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                    bStrt = false;
                }
            }
            catch (Exception)
            {
                //Util.MessageException(ex);
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isSearchAll) return;

            if (sender == null) return;
            LGCDatePicker dtPik = (sender as LGCDatePicker);

            if (sCaldate.Equals("")) return;

            if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
            {
                dtPik.Text = dtCaldate.ToLongDateString();
                dtPik.SelectedDateTime = dtCaldate;

                // 선택할 수 없습니다.
                Util.MessageValidation("SFU1669");
                //e.Handled = false;
                return;
            }

        }

        private void btnSecCutOff_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSectionCutOff()) return;

            PopupSectionCutOff();
        }

        private void btnSaveFaulty_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveFaulty()) return;

            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetFaulty();
                }
            });
        }

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveDefect()) return;

            //불량정보를 저장하시겠습니까?            
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void btnSaveRemark_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //LOT정보가 없습니다.
                Util.MessageValidation("SFU1386");
                return;
            }

            //저장하시겠습니까
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetRemark();
                }
            });
        }

        private void txtOutLaminationCnt_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutLaminationCnt.Text, 0))
                {
                    txtOutLaminationCnt.Text = string.Empty;
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (txtOutLaminationCa.Visibility == Visibility.Visible)
                        txtOutLaminationCa.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutLaminationCa_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        // 접근권한이 없습니다.
                        Util.MessageValidation("10042", (action) =>
                        {
                            txtOutLaminationCa.Text = string.Empty;
                            txtOutLaminationCa.Focus();
                        });

                        return;
                    }

                    CreateOutProdWithOutMsg();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutLaminationCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutLaminationCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutLaminationCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutLaminationAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationAddOutMagazine()) return;

            //생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOutMagazine();
                }
            });
        }

        private void btnOutLaminationDel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDeleteOutMagazine()) return;
            _dispatcherTimerOutLamination?.Stop();

            //삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                try
                {
                    // Timer Stop.
                    _dispatcherTimerOutLamination?.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        OutDelete();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (_dispatcherTimerOutLamination != null && _dispatcherTimerOutLamination.Interval.TotalSeconds > 0)
                        _dispatcherTimerOutLamination.Start();
                }
            });
        }

        private void btnOutLaminationSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveOutMagazine()) return;
            _dispatcherTimerOutLamination?.Stop();

            //저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        OutSave();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (_dispatcherTimerOutLamination != null && _dispatcherTimerOutLamination.Interval.TotalSeconds > 0)
                        _dispatcherTimerOutLamination.Start();
                }
            });

        }

        private void btnOutLaminationPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrintOutMagazine()) return;

            _dispatcherTimerOutLamination?.Stop();
            Util.MessageConfirm("SFU1237", (result) =>
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                try
                {
                    // Timer Stop.
                    _dispatcherTimerOutLamination?.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        if (UnldrLotIdentBasCode.Equals("RF_ID"))
                        {
                            try
                            {
                                btnOutLaminationPrint.IsEnabled = false;

                                using (ThermalPrint thmrPrt = new ThermalPrint())
                                {
                                    thmrPrt.Print(sEqsgID: EquipmentSegmentCode,
                                        sEqptID: EquipmentCode,
                                        sProcID: ProcessCode,
                                        inData: GetGroupPrintInfo(),
                                        iType: THERMAL_PRT_TYPE.COM_OUT_RFID_GRP,
                                        iPrtCnt: 1,
                                        bSavePrtHist: true,
                                        bDispatch: true);

                                    GetOutMagazine(DvProductLot);

                                    if (chkAll?.IsChecked != null)
                                        chkAll.IsChecked = false;
                                }

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                btnOutLaminationPrint.IsEnabled = true;
                            }
                        }
                        else
                        {

                            btnOutLaminationPrint.IsEnabled = false;

                            List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                            for (int i = 0; i < dgOutLamination.Rows.Count - dgOutLamination.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgOutLamination, "CHK", i)) continue;

                                DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID")));

                                if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                                //라미
                                dicParam.Add("reportName", "Lami"); //dicParam.Add("reportName", "Fold");
                                dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                                dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                                dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                                dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                                dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                                dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                                dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                                dicParam.Add("TITLEX", "MAGAZINE ID");
                                dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                                dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                                dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.
                                dicList.Add(dicParam);
                            }

                            CMM_THERMAL_PRINT_LAMI print = new CMM_THERMAL_PRINT_LAMI();
                            print.FrameOperation = FrameOperation;

                            object[] parameters = new object[7];
                            parameters[0] = dicList;
                            parameters[1] = Process.LAMINATION;
                            parameters[2] = EquipmentSegmentCode;
                            parameters[3] = EquipmentCode;
                            parameters[4] = "Y";            // 완료 메시지 표시 여부.
                            parameters[5] = "Y";            // 디스패치 처리.
                            parameters[6] = "MAGAZINE";     // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                            C1WindowExtension.SetParameters(print, parameters);
                            print.Closed += new EventHandler(print_Closed);
                            print.Show();

                            btnOutLaminationPrint.IsEnabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (_dispatcherTimerOutLamination != null && _dispatcherTimerOutLamination.Interval.TotalSeconds > 0)
                        _dispatcherTimerOutLamination.Start();
                }
            });
        }

        private void BoxIDPrint(string sBoxID = "", decimal dQty = 0)
        {
            try
            {
                int iCopys = 2;

                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                btnOutStackingPrint.IsEnabled = false;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                if (!sBoxID.Equals(""))
                {
                    // 발행..
                    DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //폴딩
                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수
                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.
                    dicList.Add(dicParam);
                }
                else
                {
                    for (int i = 0; i < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; i++)
                    {
                        if (!_util.GetDataGridCheckValue(dgOutStacking, "CHK", i)) continue;

                        DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "LOTID")));

                        if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                        Dictionary<string, string> dicParam = new Dictionary<string, string>();

                        //폴딩
                        dicParam.Add("reportName", "Fold");
                        dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                        dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                        dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                        dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                        dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                        dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                        dicParam.Add("TITLEX", "BASKET ID");
                        dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수
                        dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                        dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.
                        dicList.Add(dicParam);
                    }
                }

                CMM_THERMAL_PRINT_FOLD print = new CMM_THERMAL_PRINT_FOLD();
                print.FrameOperation = FrameOperation;

                object[] parameters = new object[6];
                parameters[0] = dicList;
                parameters[1] = Process.STACKING_FOLDING;
                parameters[2] = EquipmentSegmentCode;
                parameters[3] = EquipmentCode;
                parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                parameters[5] = "Y";   // 디스패치 처리.

                C1WindowExtension.SetParameters(print, parameters);
                print.Closed += new EventHandler(print_Closed);
                print.Show();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnOutStackingPrint.IsEnabled = true;
            }
        }

        private void txtOutStackingBoxQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutStackingBoxQty.Text, 0))
                {
                    txtOutStackingBoxQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (txtOutStackingCa.Visibility == Visibility.Visible)
                        txtOutStackingCa.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutStackingCa_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    System.Threading.Thread.Sleep(300);

                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        // 접근권한이 없습니다.
                        Util.MessageValidation("10042", (action) =>
                        {
                            txtOutStackingCa.Text = "";
                            txtOutStackingCa.Focus();
                        });

                        return;
                    }

                    //2020-01-16 오화백 영문과 숫자만 들어가도록 로직 추가
                    bool outCa = System.Text.RegularExpressions.Regex.IsMatch(txtOutStackingCa.Text, @"^[a-zA-Z0-9]+$");

                    if (outCa == false)
                    {
                        Util.MessageValidation("SFU3674", (action) =>
                        {
                            txtOutStackingCa.Text = "";
                            txtOutStackingCa.Focus();
                        });

                        return;
                    }


                    CreateOutStackingProdWithOutMsg();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutStackingCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutStackingCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutStackingCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCreateBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreateBox()) return;

            //생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOutBox();
                }
            });
        }

        private void btnDeleteBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDeleteBox()) return;

            // Timer Stop.
            _dispatcherTimerOutStacking?.Stop();
            Util.MessageConfirm("SFU1230", (result) =>
            {
                try
                {
                    // Timer Stop.
                    _dispatcherTimerOutStacking?.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        DeleteOutBox();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (_dispatcherTimerOutStacking != null && _dispatcherTimerOutStacking.Interval.TotalSeconds > 0)
                        _dispatcherTimerOutStacking.Start();
                }
            });

        }

        private void btnOutStackingSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveOutBox()) return;

            _dispatcherTimerOutStacking?.Stop();

            // 저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    // Timer Stop.
                    _dispatcherTimerOutStacking?.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        SaveOutBox();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (_dispatcherTimerOutStacking != null && _dispatcherTimerOutStacking.Interval.TotalSeconds > 0)
                        _dispatcherTimerOutStacking.Start();
                }
            });
        }

        private void btnOutStackingPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationStackingPrint()) return;

            Util.MessageConfirm("SFU1237", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 20.10.22 3) 고정식BCR 사용 시 라벨 출력 항목 수정
                    if (UnldrLotIdentBasCode.Equals("RF_ID"))
                    {
                        try
                        {
                            btnOutStackingPrint.IsEnabled = false;

                            DataTable dtTmp = null;

                            for (int i = 0; i < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; i++)
                            {
                                if (!_util.GetDataGridCheckValue(dgOutStacking, "CHK", i)) continue;


                                DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "LOTID")));

                                if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


                                if (!dtRslt.Columns.Contains("DISPATCH_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("DISPATCH_YN", typeof(string));
                                    dcTmp.DefaultValue = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "DISPATCH_YN")).Equals("Y") ? "Y" : "N";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                if (!dtRslt.Columns.Contains("RE_PRT_YN"))
                                {
                                    DataColumn dcTmp = new DataColumn("RE_PRT_YN", typeof(string));
                                    dcTmp.DefaultValue = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N";
                                    dtRslt.Columns.Add(dcTmp);
                                }

                                if (dtTmp == null)
                                    dtTmp = dtRslt.Copy();
                                else
                                    dtTmp.Merge(dtRslt);
                            }

                            if (dtTmp == null) return;

                            using (ThermalPrint thmrPrt = new ThermalPrint())
                            {
                                THERMAL_PRT_TYPE type = THERMAL_PRT_TYPE.FOL_OUT_BASKET_NO_BCD;

                                thmrPrt.Print(sEqsgID: EquipmentSegmentCode,
                                              sEqptID: EquipmentCode,
                                              sProcID: Process.STACKING_FOLDING,
                                              inData: dtTmp,
                                              iType: type,
                                              iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                              bSavePrtHist: true,
                                              bDispatch: true);

                                GetOutStackingProduct(DvProductLot);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            btnOutStackingPrint.IsEnabled = true;
                        }
                    }
                    else
                        BoxIDPrint();
                }
            });
        }

        private void dgOutStacking_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (IsMagzinePrintVisible)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        }
                    }
                }
            }));
        }

        private void dgOutStacking_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void cboAutoSearchOutStacking_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherTimerOutStacking != null)
                {
                    _dispatcherTimerOutStacking.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOutStacking != null && cboAutoSearchOutStacking.SelectedValue != null && !cboAutoSearchOutStacking.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutStacking.SelectedValue.ToString());

                    if (iSec == 0 && _isOutStackingAutoSelectTime)
                    {
                        _dispatcherTimerOutStacking.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isOutStackingAutoSelectTime = true;
                        return;
                    }

                    _dispatcherTimerOutStacking.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherTimerOutStacking.Start();

                    if (_isOutStackingAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        Util.MessageValidation("SFU1605", cboAutoSearchOutStacking.SelectedValue.ToString());
                    }

                    _isOutStackingAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            if (ProcessCode == Process.LAMINATION)
            {
                CMM_THERMAL_PRINT_LAMI window = sender as CMM_THERMAL_PRINT_LAMI;

                GetOutMagazine(DvProductLot);

                if (chkAll?.IsChecked != null)
                    chkAll.IsChecked = false;
            }
            else if (ProcessCode == Process.STACKING_FOLDING)
            {
                CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;
                if (window != null && window.DialogResult == MessageBoxResult.OK)
                {
                    
                }
                GetOutStackingProduct(DvProductLot);
            }

            // 김용군 AZS_STACKING 대응
            else if (ProcessCode == Process.AZS_STACKING)
            {
                CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;
                if (window != null && window.DialogResult == MessageBoxResult.OK)
                {

                }
                GetOutStackingProduct(DvProductLot);
            }
            // 김용군 AZS_ECUTTER 대응
            else if (ProcessCode == Process.AZS_ECUTTER)
            {
                CMM_THERMAL_PRINT_LAMI window = sender as CMM_THERMAL_PRINT_LAMI;

                GetOutMagazine(DvProductLot);

                if (chkAll?.IsChecked != null)
                    chkAll.IsChecked = false;
            }
        }

        private void dgOutLamination_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;


                if ((Util.NVC((e.Row.DataItem as DataRowView).Row["CHK"]).Equals("0") && e.Column.Name.Equals("CSTID")) || (Util.NVC((e.Row.DataItem as DataRowView).Row["CSTID"]).Trim().IndexOf("NOREAD") < 0 && e.Column.Name.Equals("CSTID")))
                {
                    e.Cancel = true;
                    //dgOut.BeginEdit(e.Row.Index, 4);
                }
                if (DataTableConverter.GetValue(e.Row.DataItem, "DISPATCH_YN").Equals("N"))
                {
                    e.Cancel = true;
                }
                if (InputMethod.Current != null)
                    InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
            }
            catch (Exception)
            {
            }
        }

        private void dgOutLamination_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null) return;

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        //if (Util.NVC(e.Cell.Column.Name).Equals("LOTID"))
                        //{
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //}
                        //else
                        //{
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}
                        if (IsMagzinePrintVisible)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                            }
                            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgOutLamination_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgOutLamination_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Presenter?.Content != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":
                            break;
                        case "CSTID":
                            if (UnldrLotIdentBasCode.Equals("RF_ID"))
                            {
                                dgOutLamination.EndEdit();
                                SetCarrierMapping(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CSTID")));
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutLamination_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_ASSY_CELL_INFO popupCellInfo = new CMM_ASSY_CELL_INFO();
                            popupCellInfo.FrameOperation = FrameOperation;

                            object[] parameters = new object[2];
                            parameters[0] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name));
                            parameters[1] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "CSTID"));

                            C1WindowExtension.SetParameters(popupCellInfo, parameters);
                            popupCellInfo.Closed += new EventHandler(popupCellInfo_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => popupCellInfo.ShowModal()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CELL_INFO window = sender as CMM_ASSY_CELL_INFO;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void cboAutoSearchOutLamination_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherTimerOutLamination != null)
                {
                    _dispatcherTimerOutLamination.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOutLamination != null && cboAutoSearchOutLamination.SelectedValue != null && !cboAutoSearchOutLamination.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutLamination.SelectedValue.ToString());

                    if (iSec == 0 && _isOutLaminationAutoSelectTime)
                    {
                        _dispatcherTimerOutLamination.Interval = new TimeSpan(0, 0, iSec);
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    

                    if (iSec == 0)
                    {
                        _isOutLaminationAutoSelectTime = true;
                        return;
                    }

                    _dispatcherTimerOutLamination.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherTimerOutLamination.Start();

                    if (_isOutLaminationAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        Util.MessageValidation("SFU1605", cboAutoSearchOutLamination.SelectedValue.ToString());
                    }

                    _isOutLaminationAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOutPackagingCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreateTray())
                return;

            // 특별 Tray 정보 Check.
            string samePackagingLot = "Y";
            string messageCode = string.Empty;

            DataTable dtRslt = GetSpecialTrayInfo();
            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                string specialProdLot = Util.NVC(dtRslt.Rows[0]["SPCL_PROD_LOTID"]);

                if (!specialProdLot.Equals("") && DvProductLot != null)
                {
                    if (!Util.NVC(DvProductLot["LOTID"]).Equals(specialProdLot))
                    {
                        samePackagingLot = "N";

                        //sMsg = "선택한 조립 LOT과 특별 TRAY로 설정된 조립 LOT이 다릅니다.";
                        messageCode = "SFU1665";
                    }
                }
            }

            if (string.IsNullOrEmpty(messageCode))
            {
                ASSY005_007_TRAY_CREATE popupTrayCreate = new ASSY005_007_TRAY_CREATE();
                popupTrayCreate.FrameOperation = FrameOperation;

                object[] parameters = new object[7];
                parameters[0] = EquipmentSegmentCode;
                parameters[1] = EquipmentCode;
                parameters[2] = DvProductLot["LOTID"].GetString();
                parameters[3] = DvProductLot["WIPSEQ"].GetString();
                parameters[4] = rdoTraceUse.IsChecked != null && (bool)rdoTraceUse.IsChecked ? "Y" : "N";
                parameters[5] = string.Empty;//cboTrayType.SelectedValue.ToString();
                parameters[6] = samePackagingLot;

                C1WindowExtension.SetParameters(popupTrayCreate, parameters);

                popupTrayCreate.Closed += new EventHandler(popupTrayCreate_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupTrayCreate.ShowModal()));
            }
            else
            {
                Util.MessageConfirm(messageCode, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ASSY005_007_TRAY_CREATE popupTrayCreate = new ASSY005_007_TRAY_CREATE();
                        popupTrayCreate.FrameOperation = FrameOperation;

                        object[] parameters = new object[7];
                        parameters[0] = EquipmentSegmentCode;
                        parameters[1] = EquipmentCode;
                        parameters[2] = DvProductLot["LOTID"].GetString();
                        parameters[3] = DvProductLot["WIPSEQ"].GetString();
                        parameters[4] = rdoTraceUse.IsChecked != null && (bool)rdoTraceUse.IsChecked ? "Y" : "N";
                        parameters[5] = string.Empty;// cboTrayType.SelectedValue.ToString();
                        parameters[6] = samePackagingLot;

                        C1WindowExtension.SetParameters(popupTrayCreate, parameters);
                        popupTrayCreate.Closed += new EventHandler(popupTrayCreate_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => popupTrayCreate.ShowModal()));
                    }
                });
            }
        }

        private void popupTrayCreate_Closed(object sender, EventArgs e)
        {
            ASSY005_007_TRAY_CREATE pop = sender as ASSY005_007_TRAY_CREATE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                // tray 생성 후 trace 모드인 경우는 cell 팝업 호출.
                if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                {
                    ASSY005_007_CELL_LIST popupCellList = new ASSY005_007_CELL_LIST();
                    popupCellList.FrameOperation = FrameOperation;

                    object[] parameters = new object[8];
                    parameters[0] = EquipmentSegmentCode;
                    parameters[1] = EquipmentCode;
                    parameters[2] = DvProductLot["LOTID"].GetString();
                    parameters[3] = DvProductLot["WIPSEQ"].GetString();
                    parameters[4] = Util.NVC(pop.CREATE_TRAYID);
                    parameters[5] = Util.NVC(pop.CREATE_TRAY_QTY);
                    parameters[6] = Util.NVC(pop.CREATE_OUT_LOT);
                    parameters[7] = false;  // View Mode. (Read Only)

                    C1WindowExtension.SetParameters(popupCellList, parameters);
                    popupCellList.Closed += new EventHandler(popupCellList_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupCellList.ShowModal()));
                }

                GetOutPackagingTray(DvProductLot);
                // 생산량, 양품량 재조회
                GetInputQoodQty();
                IsProductLotRefreshFlag = true;
            }
        }

        private void popupCellList_Closed(object sender, EventArgs e)
        {
            ASSY005_007_CELL_LIST window = sender as ASSY005_007_CELL_LIST;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }

            //GetWorkOrder(); // 작지 생산수량 정보 재조회.
            //GetProductLot();
            GetOutPackagingTray(DvProductLot);
            // 생산량, 양품량 재조회
            GetInputQoodQty();

            IsProductLotRefreshFlag = true;
        }

        private void btnOutPackagingDel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayDelete())
                return;

            //string sMsg = "삭제 하시겠습니까?";
            string messageCode = "SFU1230";
            string sCellQty = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK")].DataItem, "CELLQTY"));
            double dCellQty = 0;

            if (!sCellQty.Equals(""))
                double.TryParse(sCellQty, out dCellQty);

            if (!sCellQty.Equals("") && dCellQty != 0)
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
            });
        }

        private void btnOutPackagingConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTrayConfirm())
                return;

            ConfirmTrayProcess();

            // 특별 Tray 정보 조회.
            GetSpecialTrayInfo();
        }

        private void btnOutPackagingConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfirmCancel())
                return;

            //취소 하시겠습니까?
            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmTrayCancel();
                }
            });
        }

        private void btnOutPackagingCell_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationChangeCell())
                return;

            ChangeCellInfo();
        }

        private void btnOutPackagingSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveTray())
                return;

            SaveTray();
        }

        private void dgOutPackaging_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgOutPackaging_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null && e.Cell.Presenter?.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        // 이전 값 저장.
                                        _previousValues.PreviousTray = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OUT_LOTID"));

                                        SetOutTrayButtonEnable(e.Cell.Row);

                                        for (int idx = 0; idx < dg.Rows.Count; idx++)
                                        {
                                            if (e.Cell.Row.Index != idx)
                                            {
                                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                                {
                                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                                }
                                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                            }
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN"))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                        chk.IsChecked = false;

                                        _previousValues.PreviousTray = string.Empty;

                                        // 확정 시 저장, 삭제 버튼 비활성화
                                        SetOutTrayButtonEnable(null);
                                    }
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }
            }));
        }

        private void dgOutPackaging_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
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
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("CELL_MISMATCH") && e.Cell.Column.Name == "FORM_MOVE_STAT_CODE_NAME")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgOutPackaging_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
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

        private void dgOutPackaging_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null) return;

                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN") ||
                    Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT"))
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutPackaging_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (특별 Tray 적용 Grid Btn Event 추가) 
        private void chkOutStkTraySpl_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("EQGRID", typeof(string));
            dt.Columns.Add("LANGID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["EQSGID"] = EquipmentSegmentCode;
            dr["EQGRID"] = "PKG";
            dr["LANGID"] = LoginInfo.LANGID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SPCL_EQPT_LIST", "RQSTDT", "RSLTDT", dt);


            if (dtResult != null)
            {
                dr = dtResult.NewRow();
                dr["EQPTNAME"] = "-SELECT-";
                dr["EQPTID"] = "SELECT";
                dtResult.Rows.InsertAt(dr, 0);

                cboSpclEqpt.DisplayMemberPath = "EQPTNAME";
                cboSpclEqpt.SelectedValuePath = "EQPTID";
                cboSpclEqpt.ItemsSource = dtResult.Copy().AsDataView();
                cboSpclEqpt.SelectedIndex = 0;
            }
        }

        private void btnOutStkTraySplSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationOutStkTraySplSave())
                return;

            //적용 하시겠습니까?
            Util.MessageConfirm("SFU1879", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveSpecialStkTray();
                }
            });
        }

        private void chkOutStkTraySpl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txtOutStkTrayReamrk != null)
            {
                txtOutStkTrayReamrk.Text = string.Empty;
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
            if (!ValidationOutTraySplSave())
                return;

            //적용 하시겠습니까?
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
            if (dgOutPackaging == null)
                return;

            if (rdoTraceUse.IsChecked.HasValue && (bool)rdoTraceUse.IsChecked)
                dgOutPackaging.Columns["CELLQTY"].IsReadOnly = true;
            else
                dgOutPackaging.Columns["CELLQTY"].IsReadOnly = false;
        }

        private void rdoTraceNotUse_Checked(object sender, RoutedEventArgs e)
        {
            if (dgOutPackaging == null) return;

            if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                dgOutPackaging.Columns["CELLQTY"].IsReadOnly = false;
            else
                dgOutPackaging.Columns["CELLQTY"].IsReadOnly = true;
        }

        private void cboAutoSearchOutPackaging_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherTimerOutPackaging != null)
                {
                    _dispatcherTimerOutPackaging.Stop();

                    int iSec = 0;

                    if (cboAutoSearchOutPackaging?.SelectedValue != null && !cboAutoSearchOutPackaging.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearchOutPackaging.SelectedValue.ToString());

                    if (iSec == 0 && _isOutPackagingAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회가 사용하지 않도록 변경 되었습니다.");
                        Util.MessageValidation("SFU1606");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isOutPackagingAutoSelectTime = true;
                        return;
                    }

                    _dispatcherTimerOutPackaging.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherTimerOutPackaging.Start();

                    if (_isOutPackagingAutoSelectTime)
                    {
                        //Util.AlertInfo("생산 반제품 자동조회 주기가 {0}초로 변경 되었습니다.", cboAutoSearchOut.SelectedValue.ToString());
                        Util.MessageInfo("SFU1605", cboAutoSearchOutPackaging.SelectedValue.ToString());
                    }

                    _isOutPackagingAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSaveInputLoss_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                string sLot = DvProductLot["LOTID"].GetString();

                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("SECTION_ID", typeof(string));
                inDataTable.Columns.Add("OCCR_COUNT", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));

                for (int i = dgInputLoss.TopRows.Count; i < dgInputLoss.Rows.Count - dgInputLoss.BottomRows.Count; i++)
                {
                    DataRow newRow = inDataTable.NewRow();

                    decimal dCnt = 0;
                    decimal.TryParse(Util.NVC(DataTableConverter.GetValue(dgInputLoss.Rows[i].DataItem, "OCCR_COUNT")), out dCnt);

                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = EquipmentCode;
                    newRow["LOTID"] = sLot;
                    newRow["SECTION_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputLoss.Rows[i].DataItem, "SECTION_ID"));
                    newRow["OCCR_COUNT"] = dCnt;
                    newRow["USERID"] = LoginInfo.USERID;

                    inDataTable.Rows.Add(newRow);
                }

                if (inDataTable.Rows.Count < 1)
                {
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_SECTION_CUTOFF", "INDATA", null, inDataTable, (bizResult, bizException) =>
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
                        GetEquipmentSectionCutOffList(DvProductLot);
                        IsProductLotRefreshFlag = true;
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
                HiddenLoadingIndicator();
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                int iTimes = 0;

                int.TryParse(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT")), out iTimes);

                DataTableConverter.SetValue(bt.DataContext, "OCCR_COUNT", ++iTimes);


                //파단수량 및 전체 수량을 다시 계산함
                DataTableConverter.SetValue(bt.DataContext, "CUT_EA", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SECTION_EA"))) * Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT"))));

                DataTable dtList = DataTableConverter.Convert(dgInputLoss.ItemsSource);
                Util.GridSetData(dgInputLoss, dtList, FrameOperation, false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                int iTimes = 0;

                int.TryParse(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT")), out iTimes);

                DataTableConverter.SetValue(bt.DataContext, "OCCR_COUNT", iTimes < 1 ? 0 : --iTimes);

                //파단수량 및 전체 수량을 다시 계산함
                DataTableConverter.SetValue(bt.DataContext, "CUT_EA", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SECTION_EA"))) * Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "OCCR_COUNT"))));

                DataTable dtList = DataTableConverter.Convert(dgInputLoss.ItemsSource);
                Util.GridSetData(dgInputLoss, dtList, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void tcDataCollectRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            C1TabControl c1TabControl = sender as C1TabControl;
            if (c1TabControl.IsLoaded)
            {
                string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

                if (string.Equals(tabItem, "tabOutPackagingProduct"))
                {
                    grdTrayLegend.Visibility = Visibility.Visible;
                }
                else
                    grdTrayLegend.Visibility = Visibility.Collapsed;
            }
        }


        #region PKG 공급 Carrier
        private void btnCreatePkgSupplyBOX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCreatePkgSupplyBox())
                    return;

                //"생성 하시겠습니까?"
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreatePkgSupplyBox();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDeletePkgSupplyBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDeletePkgSupplyBox())
                return;

            // 삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeletePkgSupplyBox();
                }
            });
        }

        private void btnOutPkgSupplySave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutPkgSupplyBox())
                return;

            //저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SavePkgSupplyBox();
                }
            });
        }

        private void txtOutPkgSupplyBoxQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutPkgSupplyBoxQty.Text, 0))
                {
                    txtOutPkgSupplyBoxQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    if (txtOutPkgSupplyCa.Visibility == Visibility.Visible)
                        txtOutPkgSupplyCa.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutPkgSupplyCa_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    {
                        // 접근권한이 없습니다.
                        Util.MessageValidation("10042", (action) =>
                        {
                            txtOutPkgSupplyCa.Text = "";
                            txtOutPkgSupplyCa.Focus();
                        });

                        return;
                    }

                    CreatePkgSupplyBoxWithOutMsg();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtOutPkgSupplyCa_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtOutPkgSupplyCa == null) return;
                InputMethod.SetPreferredImeConversionMode(txtOutPkgSupplyCa, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        /// <summary>
        /// AutoHold 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAutoHold_Click(object sender, RoutedEventArgs e)
        {
            if(cboHoldCode.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("HOLD코드"));
                chkAutoHold.IsChecked = false;
                return;
            }
            if(Util.NVC(txtRemark.Text) == string.Empty)
            {
                Util.MessageValidation("SFU1341");
                chkAutoHold.IsChecked = false;
                return;
            }
            if (chkAutoHold.IsChecked == true)
            {
                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU8533", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetAutoHold(true);
                        chkAutoHold.IsChecked = true;
                    }
                    else
                    {
                        chkAutoHold.IsChecked = false;
                    }

                });
            }
            else
            {

                // 변경 하시겠습니까?
                Util.MessageConfirm("SFU8534", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetAutoHold(false);
                        chkAutoHold.IsChecked = false;
                    }
                    else
                    {
                        chkAutoHold.IsChecked = true;
                    }


                });
            }
        }

        #endregion

        #region Mehod


        /// <summary>
        ///2022-11-29 오화백 AUTO HOLD 설정
        /// 스태킹 AUTO HOLD 자동 설정
        /// </summary>
        private void SetAutoHold(bool HoldFlag)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("AUTO_HOLD_FLAG", typeof(string));
                dtRqst.Columns.Add("AUTO_HOLD_CODE", typeof(string));
                dtRqst.Columns.Add("AUTO_HOLD_NOTE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtRqst.NewRow();
                dr["LOTID"] = DvProductLot["LOTID"].GetString();
                if (HoldFlag == true)
                {
                    dr["AUTO_HOLD_FLAG"] = "Y";
                    dr["AUTO_HOLD_CODE"] = Util.NVC(cboHoldCode.SelectedValue.ToString());
                    dr["AUTO_HOLD_NOTE"] = Util.NVC(txtRemark.Text.ToString());
                }
                else
                {
                    dr["AUTO_HOLD_FLAG"] = string.Empty;
                    dr["AUTO_HOLD_CODE"] = string.Empty;
                    dr["AUTO_HOLD_NOTE"] = string.Empty;
                }
              
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_WIPATTR_STK_AUTO_HOLD_TAGET", "INDATA", null, dtRqst);

                if (HoldFlag)
                {
                    Util.MessageInfo("SFU1518");
                    // AutoHold 적용여부
                    CheckAutoHoldLot();
                    cboHoldCode.IsEnabled = false;
                    txtRemark.IsEnabled = false;

                }
                else
                {
                    Util.MessageInfo("SFU1937");
                    // AutoHold 적용여부
                    CheckAutoHoldLot();
                    cboHoldCode.IsEnabled = true;
                    txtRemark.IsEnabled = true;
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        /// <summary>
        /// 스태킹 AUTO HOLD 적용여부 체크
        /// </summary>
        private void CheckAutoHoldLot()
        {
           
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = DvProductLot["LOTID"].GetString();
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["AUTO_HOLD_FLAG"].ToString() == "Y")
            {
                chkAutoHold.IsChecked = true;
                cboHoldCode.SelectedValue = dtResult.Rows[0]["AUTO_HOLD_CODE"].ToString();
                txtRemark.Text = dtResult.Rows[0]["AUTO_HOLD_NOTE"].ToString();
                cboHoldCode.IsEnabled = false;
                txtRemark.IsEnabled = false;
            }
            else
            {
                chkAutoHold.IsChecked = false;
                cboHoldCode.SelectedIndex = 0;
                txtRemark.Text = string.Empty;

                cboHoldCode.IsEnabled = true;
                txtRemark.IsEnabled = true;

            }

        }


        /// <summary>
        /// 스태킹 AUTOHOLD 적용 공장  VISIBLITY
        /// </summary>
        private bool StkAutoHoldVisibility()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "STACKING_OUTLOT_AUTO_HOLD_AREA";
            dr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
            dt.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", dt);

            if (dtResult.Rows.Count != 0 && dtResult.Rows[0]["ATTRIBUTE1"].ToString() == "Y")
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }



        public void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                //btnSecCutOff,
                //btnSaveFaulty,
                //btnSaveRemark
            };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void SelectProductionResult()
        {
            SetControl();
            SetControlClear();
            SetControlVisibility();

            if (ProcessCode == Process.NOTCHING)
            {
                //설비불량정보 탭 그리드의 셀 등록 버튼 컬럼
                //TemplateCell.Visibility = Visibility.Collapsed;

                // 생산실적
                SetProductionResult();

                // 불량/LOSS/물품청구
                GetFaultyData();

                // 설비불량정보
                GetEquipmentFaultyData();
                

                if (DvProductLot["WIPSTAT"].GetString() == "EQPT_END")
                {
                    SetParentQty();
                }

                SetInputHistToolWindow();

                GetInputHistTool();

                CheckEDCVisibity();

                if (tabEDCBCRInfo.Visibility == Visibility.Visible)
                {
                    GetBcrReadingRate(true);
                }

                dtpCaldate.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            }
            else if (ProcessCode == Process.LAMINATION || ProcessCode == Process.DNC)   // 250428 ESHG DNC공정추가 
            {
                //투입LOSS관리
                GetEquipmentSectionCutOffList(DvProductLot);

                // 생산실적
                SetProductionResult();

                //불량/Loss/물품청구
                GetDefectInfo(DvProductLot);

                // 설비불량정보
                GetEquipmentFaultyData();

                //설비 구성 유형 조회 생산반제품 생성 시 Validation 처리를 위한 용도
                GetEquipmentConfigType();

                // Tool사용이력 컨트롤 추가
                SetInputHistToolWindow();

                // Tool사용이력 조회
                GetInputHistTool();

                //생산반제품 
                GetOutMagazine(DvProductLot);
            }
            else if (ProcessCode == Process.STACKING_FOLDING)
            {
                SetControlByIdentBasCode();

                // 생산실적
                SetProductionResult();

                //불량/Loss/물품청구
                GetDefectInfo(DvProductLot);

                // 설비불량정보
                GetEquipmentFaultyData();

                // Tool사용이력 컨트롤 추가
                SetInputHistToolWindow();

                // Tool사용이력 조회
                GetInputHistTool();

                //생산반제품 
                GetOutStackingProduct(DvProductLot);

                //  2024.01.08  남재현: STK 특별 Tray 설정 추가. (특별 Tray 정보 조회)
                GetSpecialStkTrayInfo();

                // PKG 공급 Carrier
                if (EquipmentGroupCode.Equals(EquipmentGroup.FOLDING))
                {
                    // 2023.01.03 (C20221212-000186) LOT완료 UI abnormal (공정 진행 화면)-folding PKG Supply Carrier data load
                    if ( CheckUnldrShrFlag() == true )
                    {
                        // PKG 공급 Carrier 데이터 로드
                        GetPkgSupplyProduct();
                    }
                }
                else
                {
                    tabPkgSupplyCarrier.Visibility = Visibility.Collapsed;
                }
                // STK AutoHold 사용여부 
                if(StkAutoHoldVisibility() == true )
                {
                    dgOutStacking.Columns["WIPHOLD"].Visibility = Visibility.Visible;
                    gAutoHold.Visibility = Visibility.Visible;
                   
                    // AutoHold 적용여부
                    CheckAutoHoldLot();
                }
                else
                {
                    dgOutStacking.Columns["WIPHOLD"].Visibility = Visibility.Collapsed;
                    gAutoHold.Visibility = Visibility.Collapsed;
                }

                // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (특별 STK TRAY 관리 동 조회, 특별 TRAY 적용 Grid 추가) 
                if (IsCommonCodeUse("STK_BOX_SPCL_MNG_AREA", LoginInfo.CFG_AREA_ID))
                {
                    grdSpclStkTranBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    grdSpclStkTranBtn.Visibility = Visibility.Collapsed;
                }
            }
            // 김용군
            else if (ProcessCode == Process.AZS_ECUTTER)
            {
                //투입LOSS관리
                GetEquipmentSectionCutOffList(DvProductLot);

                // 생산실적
                SetProductionResult();

                //불량/Loss/물품청구
                GetDefectInfo(DvProductLot);

                // 설비불량정보
                GetEquipmentFaultyData();

                //설비 구성 유형 조회 생산반제품 생성 시 Validation 처리를 위한 용도
                GetEquipmentConfigType();

                // Tool사용이력 컨트롤 추가
                SetInputHistToolWindow();

                // Tool사용이력 조회
                GetInputHistTool();

                //생산반제품 
                GetOutMagazine(DvProductLot);
            }
            else if (ProcessCode == Process.AZS_STACKING)
            {
                SetControlByIdentBasCode();

                // 생산실적
                SetProductionResult();

                //불량/Loss/물품청구
                GetDefectInfo(DvProductLot);

                // 설비불량정보
                GetEquipmentFaultyData();

                // STK별 불량정보
                GetStkEquipmentFaultyData();

                // STK별 완성수량정보
                GetStkCompleteQtyData();

                // Tool사용이력 컨트롤 추가
                SetInputHistToolWindow();

                // Tool사용이력 조회
                GetInputHistTool();

                //생산반제품 
                GetOutStackingProduct(DvProductLot);

                // PKG 공급 Carrier
                if (EquipmentGroupCode.Equals(EquipmentGroup.FOLDING))
                {
                    CheckUnldrShrFlag();
                }
                else
                {
                    tabPkgSupplyCarrier.Visibility = Visibility.Collapsed;
                }
                // STK AutoHold 사용여부 
                if (StkAutoHoldVisibility() == true)
                {
                    dgOutStacking.Columns["WIPHOLD"].Visibility = Visibility.Visible;
                    gAutoHold.Visibility = Visibility.Visible;

                    // AutoHold 적용여부
                    CheckAutoHoldLot();
                }
                else
                {
                    dgOutStacking.Columns["WIPHOLD"].Visibility = Visibility.Collapsed;
                    gAutoHold.Visibility = Visibility.Collapsed;
                }

                tcDataCollectLeft.SelectedIndex = 1;
                tcDataCollectRight.SelectedIndex = 1;

            }
            else if (ProcessCode == Process.PACKAGING)
            {
                // 생산실적
                SetProductionResult();

                //불량/Loss/물품청구
                GetDefectInfo(DvProductLot);

                // 설비불량정보
                GetEquipmentFaultyData();

                //재투입 정보
                GetReInputInfo();

                // Tool사용이력 컨트롤 추가
                SetInputHistToolWindow();

                // Tool사용이력 조회
                GetInputHistTool();

                // 특별 Tray 정보 조회
                GetSpecialTrayInfo();

                //생산반제품 
                GetOutPackagingTray(DvProductLot);
            }
        }

        private void GetInputHistTool()
        {
            if (_winInputHistTool == null)
                return;

            _winInputHistTool.EQPTID = EquipmentCode;
            _winInputHistTool.PROD_LOTID = ProcessCode == Process.NOTCHING ? DvProductLot["PR_LOTID"].GetString() : DvProductLot["LOTID"].GetString();
            _winInputHistTool.GetInputHistTool();
        }

        private void GetBcrReadingRate(bool bChgEqptID)
        {
            try
            {

                if (string.IsNullOrEmpty(EquipmentCode))
                {
                    HideBcrWarning();
                    Util.gridClear(dgEDCInfo);
                    return;
                }

                Edc_LoadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EDC_EQPT_BCR_READ_RATE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult.Select("WARN_DISP = 'Y'").Length > 0)
                        {
                            ShowBcrWarning();

                            if (bChgEqptID)
                                tabEDCBCRInfo.IsSelected = true;
                        }
                        else
                            HideBcrWarning();

                        Util.GridSetData(dgEDCInfo, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        Edc_LoadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                Edc_LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool CheckConfirmLot()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0 && !string.Equals(ProcessCode, dtResult.Rows[0]["PROCID"]) && (string.Equals(INOUT_TYPE.IN, dtResult.Rows[0]["WIP_TYPE_CODE"]) || string.Equals(INOUT_TYPE.INOUT, dtResult.Rows[0]["WIP_TYPE_CODE"])))
                {
                    Util.MessageValidation("SFU5066");  // 이미 실적 확정 된 LOT입니다.
                    return false;
                }

            }
            catch (Exception ex) { Util.MessageException(ex); }

            return true;
        }
        // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (동 코드 조회 추가) 
        private bool IsCommonCodeUse(string sCmcdType, string sAreaid)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCmcdType;
                dr["CMCODE"] = sAreaid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsAreaCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = sCodeType;
                newRow["COM_CODE"] = sCodeName;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;

                else return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

            
        }

        private void CreateOutProdWithOutMsg()
        {
            if (!ValidationAddOutMagazine())
                return;

            CreateOutMagazine();
        }

        private void CreateOutMagazine()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("IFMODE", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQPTID"] = EquipmentCode;
                dr["USERID"] = LoginInfo.USERID;
                dr["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                RQSTDT.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(RQSTDT);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService("BR_PRD_CHK_LOT_LABEL_PRT_RESTRCT", "INDATA", null, RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ShowLoadingIndicator();

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("IFMODE", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                        inDataTable.Columns.Add("CSTID", typeof(string));
                        inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                        inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                        inInputTable.Columns.Add("INPUT_LOTID", typeof(string));

                        DataTable inTable = indataSet.Tables["INDATA"];
                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow["EQPTID"] = EquipmentCode;
                        newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                        newRow["CSTID"] = txtOutLaminationCa.Text;
                        newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutLaminationCnt.Text);
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                        newRow = null;

                        // 김용군 AZS_ECUTTER 별도 BIZ호출
                        string bizName = string.Empty;

                        if (ProcessCode == Process.AZS_ECUTTER)
                        {
                            bizName = "BR_PRD_REG_CREATE_OUT_LOT_AZC";
                        }
                        else
                        {
                            bizName = "BR_PRD_REG_CREATE_OUT_LOT_LM_L";
                        }
                        
                        //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_LM_L", "INDATA,IN_INPUT", "OUTDATA", (searchResult, searchException) =>
                        new ClientProxy().ExecuteService_Multi(bizName, "INDATA,IN_INPUT", "OUTDATA", (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {

                                    Util.MessageException(searchException, (msgResult) =>
                                    {
                                        if (txtOutLaminationCa.Visibility == Visibility.Visible)
                                        {
                                            txtOutLaminationCa.Text = string.Empty;
                                            txtOutLaminationCa.Focus();
                                        }
                                    });

                                    return;
                                }

                                GetOutMagazine(DvProductLot);
                                // 생산량, 양품량 재조회
                                GetInputQoodQty();

                                IsProductLotRefreshFlag = true;

                                txtOutLaminationCa.Text = string.Empty;
                                txtOutLaminationCnt.Text = string.Empty;

                                //정상 처리 되었습니다.
                                Util.MessageInfoAutoClosing("SFU1275");

                                if (txtOutLaminationCa.Visibility == Visibility.Visible)
                                    txtOutLaminationCa.Focus();
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
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void OutDelete()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DELETE_MAGAZINE_LM();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutLamination.Rows.Count - dgOutLamination.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutLamination, "CHK", i)) continue;

                    newRow = input_LOT.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID"));
                    input_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_LM", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetWorkOrder(); // 작지 생산수량 정보 재조회.
                        // TODO : Product Lot 리스트 제조회 필요 함.
                        //GetProductLot();

                        GetOutMagazine(DvProductLot);
                        // 생산량, 양품량 재조회
                        GetInputQoodQty();

                        IsProductLotRefreshFlag = true;
                        //정상 처리 되었습니다.
                        Util.MessageValidation("SFU1275");
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

        private void OutSave()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutLamination.EndEdit();

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable out_LOT = indataSet.Tables.Add("IN_OUTLOT");
                out_LOT.Columns.Add("OUT_LOTID", typeof(string));
                out_LOT.Columns.Add("OUT_LOT_WIPSEQ", typeof(decimal));
                out_LOT.Columns.Add("OUTPUT_QTY", typeof(decimal));
                out_LOT.Columns.Add("CSTID", typeof(string));
                out_LOT.Columns.Add("BONUSQTY", typeof(decimal));


                DataTable inTable = indataSet.Tables["IN_DATA"];
                DataRow newRow = inTable.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_OUTLOT"];

                for (int i = 0; i < dgOutLamination.Rows.Count - dgOutLamination.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutLamination, "CHK", i)) continue;

                    newRow = outLotTable.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID"));
                    newRow["OUT_LOT_WIPSEQ"] = DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "WIPSEQ");
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "WIPQTY")));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "CSTID"));
                    outLotTable.Rows.Add(newRow);
                }

                if (outLotTable.Rows.Count < 1)
                    return;

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_END_OUT_LOT_QTY_L", "IN_DATA,IN_OUTLOT", null, indataSet);


                //GetWorkOrder(); // 작지 생산수량 정보 재조회.
                //GetProductLot(); TODO : Product Lot 조회 필요함.

                GetOutMagazine(DvProductLot);
                // 생산량, 양품량 재조회
                GetInputQoodQty();

                IsProductLotRefreshFlag = true;
                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                GetOutMagazine(DvProductLot);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void CreateOutStackingProdWithOutMsg()
        {
            if (!ValidationCreateBox()) return;

            CreateOutBox();
        }

        private void CreateOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));

                inTable = indataSet.Tables.Add("IN_INPUT");
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["CSTID"] = Util.NVC(txtOutStackingCa.Text.Trim());
                newRow["OUTPUTQTY"] = Convert.ToDecimal(txtOutStackingBoxQty.Text);
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                // 김용군 AZS Staking별도 BIZ호출
                string bizName = string.Empty;

                if (ProcessCode == Process.AZS_STACKING)
                {
                    bizName = "BR_PRD_REG_CREATE_OUT_LOT_AZS_L";
                }
                else
                {
                    bizName = "BR_PRD_REG_CREATE_OUT_LOT_FD_L";
                }

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_FD_L", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                new ClientProxy().ExecuteService_Multi(bizName, "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException, (msgResult) =>
                            {
                                if (txtOutStackingCa.Visibility == Visibility.Visible)
                                {
                                    txtOutStackingCa.Text = "";
                                    txtOutStackingCa.Focus();
                                }
                            });
                            return;
                        }

                        GetOutStackingProduct(DvProductLot);
                        // 생산량, 양품량 재조회
                        GetInputQoodQty();

                        IsProductLotRefreshFlag = true;
                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfoAutoClosing("SFU1275");
                        txtOutStackingCa.Text = "";

                        if (txtOutStackingCa.Visibility == Visibility.Visible)
                            txtOutStackingCa.Focus();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DeleteOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DELETE_BOX_ST();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutStacking, "CHK", i)) continue;

                    newRow = input_LOT.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "LOTID"));
                    input_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_FD", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetOutStackingProduct(DvProductLot);
                        // 생산량, 양품량 재조회
                        GetInputQoodQty();

                        IsProductLotRefreshFlag = true; //작지 생산수량 정보 재조회.
                        //Util.AlertInfo("정상 처리 되었습니다.");
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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutStacking.EndEdit();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable out_LOT = indataSet.Tables.Add("IN_OUTLOT");
                out_LOT.Columns.Add("OUT_LOTID", typeof(string));
                out_LOT.Columns.Add("OUT_LOT_WIPSEQ", typeof(decimal));
                out_LOT.Columns.Add("OUTPUT_QTY", typeof(decimal));
                out_LOT.Columns.Add("CSTID", typeof(string));
                out_LOT.Columns.Add("BONUSQTY", typeof(decimal));

                DataTable inTable = indataSet.Tables["IN_DATA"];
                DataRow newRow = inTable.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable outLotTable = indataSet.Tables["IN_OUTLOT"];

                for (int i = 0; i < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutStacking, "CHK", i)) continue;

                    newRow = outLotTable.NewRow();

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "LOTID"));
                    newRow["OUT_LOT_WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "WIPSEQ")).Equals("") ? 1 : int.Parse(Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "WIPSEQ")));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "WIPQTY")));
                    newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "CSTID"));
                    newRow["BONUSQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "BONUS_QTY")));
                    outLotTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_QTY_L", "IN_DATA,IN_OUTLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(searchException);
                            GetOutStackingProduct(DvProductLot);
                            return;
                        }

                        GetOutStackingProduct(DvProductLot);
                        // 생산량, 양품량 재조회
                        GetInputQoodQty();

                        IsProductLotRefreshFlag = true; //작지 생산수량 정보 재조회.
                        //Util.AlertInfo("정상 처리 되었습니다.");
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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DeleteTray()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_TRAY_DEL_CL();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK")].DataItem, "OUT_LOTID"));
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK")].DataItem, "TRAYID"));
                newRow["WO_DETL_ID"] = null;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_DELETE_OUT_LOT_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetOutPackagingTray(DvProductLot);
                        // 생산량, 양품량 재조회
                        GetInputQoodQty();
                        IsProductLotRefreshFlag = true;

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
                }
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveTray()
        {
            try
            {
                dgOutPackaging.EndEdit();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK");
                string specYN = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "SPECIALYN"));
                string SpecDesc = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "SPECIALDESC"));
                string SpecRsnCode = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "SPECIALRSNCODE"));

                if (specYN.Equals("Y"))
                {
                    if (SpecDesc == "")
                    {
                        //Util.Alert("특별관리내역을 입력하세요.");
                        Util.MessageValidation("SFU1990");
                        return;
                    }

                    if (string.IsNullOrEmpty(SpecRsnCode))
                    {
                        //Util.Alert("사유를 선택하세요.");
                        Util.MessageValidation("SFU1593");
                        return;
                    }
                }
                else if (specYN.Equals("N"))
                {
                    if (SpecDesc != "")
                    {
                        //Util.Alert("특별관리내역을 삭제하세요.");
                        Util.MessageValidation("SFU1989");
                        return;
                    }

                    if (!string.IsNullOrEmpty(SpecRsnCode))
                    {
                        //Util.Alert("사유를 삭제하세요.");
                        Util.MessageValidation("SFU8674");
                        return;
                    }
                }

                /*******************************************************
                * 창을 2개 띄워놓고 동일 건에 대하여 1개의 창에서 Tray 삭제를 진행 후에는 
                * 다른창에서 Cell 관리를 실행 할 수 없도록 Validation 추가.
                *(기본 Validation에서 진행할 수 없음)
                ***********************************************************/
                DataTable paramTable = _bizDataSet.GetBR_PRD_REG_TRAY_DEL_CL();
                DataRow newParamRow = paramTable.NewRow();
                newParamRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newParamRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "OUT_LOTID"));
                //newParamRow["WIPSTAT"] = DvProductLot["WIPSTAT"].GetString();
                newParamRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "TRAYID")).Replace("\0", "");
                paramTable.Rows.Add(newParamRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_LOT_BY_TRAYID", "INDATA", "OUTDATA", paramTable);
                string sRet = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    ShowLoadingIndicator();

                    DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_SAVE_CL();

                    DataTable inTable = indataSet.Tables["IN_EQP"];
                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = EquipmentCode;
                    newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                    newRow["USERID"] = LoginInfo.USERID;
                    inTable.Rows.Add(newRow);
                    newRow = null;

                    DataTable inLot = indataSet.Tables["IN_LOT"];
                    DataTable inSpcl = indataSet.Tables["IN_SPCL"];

                    for (int i = 0; i < dgOutPackaging.Rows.Count - dgOutPackaging.BottomRows.Count; i++)
                    {
                        if (!_util.GetDataGridCheckValue(dgOutPackaging, "CHK", i)) continue;
                        // Tray 정보 DataTable             
                        newRow = inLot.NewRow();
                        newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "OUT_LOTID"));
                        newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "TRAYID"));
                        if (rdoTraceNotUse.IsChecked.HasValue && (bool)rdoTraceNotUse.IsChecked)
                            newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "CELLQTY")));

                        inLot.Rows.Add(newRow);
                        newRow = null;

                        // 특별 Tray DataTable                
                        newRow = inSpcl.NewRow();
                        newRow["SPCL_CST_GNRT_FLAG"] = specYN;
                        newRow["SPCL_CST_NOTE"] = SpecDesc;
                        newRow["SPCL_CST_RSNCODE"] = specYN.Equals("Y") ? SpecRsnCode : "";
                        inSpcl.Rows.Add(newRow);
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_CL", "IN_EQP,IN_LOT,IN_SPCL", null, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            GetOutPackagingTray(DvProductLot);
                            // 생산량, 양품량 재조회
                            GetInputQoodQty();

                            IsProductLotRefreshFlag = true;
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
                else
                {
                    Util.MessageValidation("ME_0078"); // "Tray 정보가 존재하지 않습니다.";                
                }
                /*******************************************************
                * 여기까지
                ***********************************************************/
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveSpecialTray()
        {
            try
            {
                string sRsnCode = cboOutTraySplReason.SelectedValue == null ? "" : cboOutTraySplReason.SelectedValue.ToString();

                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_SPCL_TRAY_SAVE_CL();
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["SPCL_LOT_GNRT_FLAG"] = (bool)chkOutTraySpl.IsChecked ? "Y" : "N";
                newRow["SPCL_LOT_RSNCODE"] = (bool)chkOutTraySpl.IsChecked ? sRsnCode : "";
                newRow["SPCL_LOT_NOTE"] = txtOutTrayReamrk.Text;
                newRow["SPCL_PROD_LOTID"] = (bool)chkOutTraySpl.IsChecked ? DvProductLot["LOTID"].GetString() : "";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_EIOATTR_SPCL_CST_PKG", "INDATA", null, inTable, (searchResult, searchException) =>
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

        // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (특별 TRAY 적용) 
        private void SaveSpecialStkTray()
        {
            try
            {
                string sSpclEqpt = cboSpclEqpt.SelectedValue == null ? "" : cboSpclEqpt.SelectedValue.ToString();

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SPCL_FLAG", typeof(string));
                inTable.Columns.Add("RSV_EQPTID", typeof(string));
                inTable.Columns.Add("SPCL_NOTE", typeof(string));
                inTable.Columns.Add("SPCL_PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["SPCL_FLAG"] = (bool)chkOutStkTraySpl.IsChecked ? "Y" : "N";
                newRow["RSV_EQPTID"] = (bool)chkOutStkTraySpl.IsChecked ? sSpclEqpt : "";
                newRow["SPCL_NOTE"] = txtOutStkTrayReamrk.Text;
                newRow["SPCL_PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_PROD_LOT_WIPATTR_SPCL_STK", "INDATA", null, inTable, (searchResult, searchException) =>
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
                        GetSpecialStkTrayInfo();

                        if ((bool)chkOutStkTraySpl.IsChecked)
                        {
                            grdSpecialStkTrayMode.Visibility = Visibility.Visible;
                            ColorAnimationInSpecialTray();
                        }
                        else
                        {
                            grdSpecialStkTrayMode.Visibility = Visibility.Collapsed;
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

        private void ConfirmTrayProcess()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK");

            if (idx < 0)
                return;

            //확정 하시겠습니까?
            Util.MessageConfirm("SFU2044", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmTray();
                }
            });

        }

        private void ConfirmTray()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_CONFIRM_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inCst = indataSet.Tables["IN_CST"];
                newRow = inCst.NewRow();

                int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK");

                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "OUT_LOTID"));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "CELLQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "CELLQTY")));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "TRAYID"));
                newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "SPECIALYN"));
                newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "SPECIALDESC"));
                newRow["SPCL_CST_RSNCODE"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "SPECIALRSNCODE"));
                inCst.Rows.Add(newRow);

                //string xml = indataSet.GetXml();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_OUT_LOT_CL", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetOutPackagingTray(DvProductLot);
                        // 생산량, 양품량 재조회
                        GetInputQoodQty();

                        IsProductLotRefreshFlag = true;
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ConfirmTrayCancel()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_TRAY_CONFIRM_CNL_CL();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inCst = indataSet.Tables["IN_CST"];
                newRow = inCst.NewRow();
                newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK")].DataItem, "OUT_LOTID"));
                newRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK")].DataItem, "TRAYID"));
                inCst.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CNFM_CANCEL_OUT_LOT_CL", "IN_EQP,IN_CST", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetOutPackagingTray(DvProductLot);
                        // 생산량, 양품량 재조회
                        GetInputQoodQty();

                        IsProductLotRefreshFlag = true;
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void ChangeCellInfo()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK");
            if (idx < 0)return;

            string sTrayQty = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "CST_CELL_QTY"));//cboTrayType.SelectedValue == null ? "25" : cboTrayType.SelectedValue.ToString();
            string trayID = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "TRAYID")).Replace("\0", "");
            string outLOTID = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "OUT_LOTID"));

            /*******************************************************
           * 창을 2개 띄워놓고 동일 건에 대하여 1개의 창에서 Tray 삭제를 진행 후에는 
           * 다른창에서 Cell 관리를 실행 할 수 없도록 Validation 추가.
           *(기본 Validation에서 진행할 수 없음)
           ***********************************************************/
            DataTable paramTable = _bizDataSet.GetBR_PRD_REG_TRAY_DEL_CL();
            DataRow newParamRow = paramTable.NewRow();
            newParamRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
            newParamRow["OUT_LOTID"] = outLOTID;
            //newParamRow["WIPSTAT"] = DvProductLot["WIPSTAT"].GetString();
            newParamRow["TRAYID"] = trayID;
            paramTable.Rows.Add(newParamRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROD_LOT_BY_TRAYID", "INDATA", "OUTDATA", paramTable);
            string sRet = string.Empty;
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                ASSY005_007_CELL_LIST popupCellList = new ASSY005_007_CELL_LIST();
                popupCellList.FrameOperation = FrameOperation;

                object[] parameters = new object[8];
                parameters[0] = EquipmentSegmentCode;
                parameters[1] = EquipmentCode;
                parameters[2] = DvProductLot["LOTID"].GetString();
                parameters[3] = DvProductLot["WIPSEQ"].GetString();
                parameters[4] = trayID;
                parameters[5] = sTrayQty;
                parameters[6] = outLOTID;
                parameters[7] = false;  // View Mode. (Read Only)

                C1WindowExtension.SetParameters(popupCellList, parameters);

                popupCellList.Closed += new EventHandler(popupCellList_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupCellList.ShowModal()));
            }
            else
            {
                Util.MessageValidation("ME_0078"); // "Tray 정보가 존재하지 않습니다.";                
            }
            /*******************************************************
            * 여기까지
            ***********************************************************/
        }

        private void SetControlByIdentBasCode()
        {
            if (ProcessCode.Equals(Process.STACKING_FOLDING))
            {
                if (UnldrLotIdentBasCode.Equals("CST_ID") || UnldrLotIdentBasCode.Equals("RF_ID"))
                {
                    if (dgOutStacking.Columns.Contains("CSTID"))
                        dgOutStacking.Columns["CSTID"].Visibility = Visibility.Visible;

                    lblCST.Visibility = Visibility.Visible;
                    txtOutStackingCa.Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgOutStacking.Columns.Contains("CSTID"))
                        dgOutStacking.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    lblCST.Visibility = Visibility.Collapsed;
                    txtOutStackingCa.Visibility = Visibility.Collapsed;
                }
            }

            // 김용군 AZS_STACKING 대응
            if (ProcessCode.Equals(Process.AZS_STACKING))
            {
                if (UnldrLotIdentBasCode.Equals("CST_ID") || UnldrLotIdentBasCode.Equals("RF_ID"))
                {
                    if (dgOutStacking.Columns.Contains("CSTID"))
                        dgOutStacking.Columns["CSTID"].Visibility = Visibility.Visible;

                    lblCST.Visibility = Visibility.Visible;
                    txtOutStackingCa.Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgOutStacking.Columns.Contains("CSTID"))
                        dgOutStacking.Columns["CSTID"].Visibility = Visibility.Collapsed;

                    lblCST.Visibility = Visibility.Collapsed;
                    txtOutStackingCa.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SetProductionResult()
        {
            if (DvProductLot == null) return;

            // 생산량, 양품량은 바인딩 이전에 재 조회하여 바인딩  함.
            GetInputQoodQty();

            SelectLotInfo();

            SetProductionResultDetailGrid();

        }

        private void SelectLotInfo()
        {
            if (string.Equals(ProcessCode, Process.NOTCHING))
            {
                // 1. Prodeuct Lot 선택된 Row의 데이터를 바인딩 처리
                txtLotId.Text = DvProductLot["LOTID"].GetString();
                txtCarrierId.Text = DvProductLot["CSTID"].GetString();
                txtProdId.Text = DvProductLot["CH_PRODID"].GetString();
                txtProjectName.Text = DvProductLot["PRJT_NAME"].GetString();
                txtLotStatus.Text = DvProductLot["WIPSNAME"].GetString();

                //txtStartTime.Text = DvProductLot["WIPDTTM_ST"].GetString();
                //txtEndTime.Text = DvProductLot["EQPT_END_DTTM"].GetString();
                //txtInputQty.Value = DvProductLot["EQPT_INPUT_QTY"].GetInt();
                //txtGoodQty.Value = DvProductLot["EQPT_END_QTY"].GetInt();

                //if (!string.IsNullOrEmpty(DvProductLot["WIPDTTM_ST"].GetString()) && !string.IsNullOrEmpty(DvProductLot["EQPT_END_DTTM"].GetString()))
                //{
                //    DateTime dTmpEnd;
                //    DateTime dTmpStart;

                //    if (DateTime.TryParse(DvProductLot["EQPT_END_DTTM"].GetString(), out dTmpEnd) && DateTime.TryParse(DvProductLot["WIPDTTM_ST"].GetString(), out dTmpStart))
                //        txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
                //}

                if (Util.NVC(DvProductLot["CALDATE_LOT"]).Trim().Equals(""))
                {
                    dtpCaldate.Text = Convert.ToDateTime(Util.NVC(DvProductLot["NOW_CALDATE"])).ToLongDateString();
                    dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(DvProductLot["NOW_CALDATE"]));

                    sCaldate = Util.NVC(DvProductLot["NOW_CALDATE_YMD"]);
                    dtCaldate = Convert.ToDateTime(Util.NVC(DvProductLot["NOW_CALDATE"]));
                }
                else
                {
                    dtpCaldate.Text = Convert.ToDateTime(Util.NVC(DvProductLot["CALDATE_LOT"])).ToLongDateString();
                    dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(DvProductLot["CALDATE_LOT"]));

                    sCaldate = Convert.ToDateTime(Util.NVC(DvProductLot["CALDATE_LOT"])).ToString("yyyyMMdd");
                    dtCaldate = Convert.ToDateTime(Util.NVC(DvProductLot["CALDATE_LOT"]));
                }
            }
            else
            {
                textLotId.Text = DvProductLot["LOTID"].GetString();
                textProdId.Text = DvProductLot["PRODID"].GetString();
                textProjectName.Text = DvProductLot["PRJT_NAME"].GetString();
                //textWorkTime.Text = DvProductLot["WIPDTTM_ST"].GetString() + " ~ " + DvProductLot["EQPT_END_DTTM"].GetString();
                //textInputQty.Value = DvProductLot["EQPT_INPUT_QTY"].GetInt();
                //textGoodQty.Value = DvProductLot["WIPQTY"].GetInt();
            }
        }

        private void GetInputQoodQty()
        {
            if (DvProductLot == null) return;

            if (ProcessCode == Process.NOTCHING)
            {
                txtInputQty.Value = 0;
                txtGoodQty.Value = 0;

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["WIPSTAT"] = "PROC,EQPT_END";
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_NT_L_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    var query = (from t in searchResult.AsEnumerable()
                                 where t.Field<string>("LOTID") == DvProductLot["LOTID"].GetString()
                                       && t.Field<string>("EQPTID") == DvProductLot["EQPTID"].GetString()
                                       && t.Field<string>("WIPSEQ") == DvProductLot["WIPSEQ"].GetString()   // 2024.11.06 천진수 decimal → string 
                                 select new
                                 {
                                     InputQty = t.Field<long>("EQPT_INPUT_QTY"),    // 2024.11.06 천진수 decimal → long 
                                     GoodQty = t.Field<long>("EQPT_END_QTY"),       // 2024.11.06 천진수 decimal → long  
                                     StartWorkTime = t.Field<string>("WIPDTTM_ST"),
                                     EndWorkTime = t.Field<string>("EQPT_END_DTTM"),
                                     CalDateLot = t.Field<string>("CALDATE_LOT"),
                                     NowCalDate = t.Field<string>("NOW_CALDATE"),
                                 }).FirstOrDefault();

                    if (query != null)
                    {
                        txtInputQty.Value = query.InputQty.GetInt();
                        txtGoodQty.Value = query.GoodQty.GetInt();
                        txtStartTime.Text = query.StartWorkTime;
                        txtEndTime.Text = query.EndWorkTime;

                        if (!string.IsNullOrEmpty(query.StartWorkTime) && !string.IsNullOrEmpty(query.EndWorkTime))
                        {
                            DateTime dTmpEnd;
                            DateTime dTmpStart;

                            if (DateTime.TryParse(query.EndWorkTime, out dTmpEnd) && DateTime.TryParse(query.StartWorkTime, out dTmpStart))
                                txtWorkMinute.Text = Math.Truncate(dTmpEnd.Subtract(dTmpStart).TotalMinutes).ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            else
            {
                textInputQty.Value = 0;
                textGoodQty.Value = 0;

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["EQSGID"] = EquipmentSegmentCode;
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_LM_L_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    var query = (from t in searchResult.AsEnumerable()
                        where t.Field<string>("LOTID") == DvProductLot["LOTID"].GetString()
                              && t.Field<string>("EQPTID") == DvProductLot["EQPTID"].GetString()
                              && t.Field<string>("WIPSEQ") == DvProductLot["WIPSEQ"].GetString()   // 2024.11.08 천진수 decimal .GetString() → string
                        select new
                        {
                            InputQty = t.Field<long>("EQPT_INPUT_QTY"),  // 2024.11.08 천진수 decimal → long
                            GoodQty = t.Field<long>("WIPQTY"),           // 2024.11.08 천진수 decimal → long
                            StartWorkTime = t.Field<string>("WIPDTTM_ST"),
                            EndWorkTime = t.Field<string>("EQPT_END_DTTM")
                        }).FirstOrDefault();

                    if (query != null)
                    {
                        textInputQty.Value = query.InputQty.GetInt();
                        textGoodQty.Value = query.GoodQty.GetInt();
                        textWorkTime.Text = query.StartWorkTime + " ~ " + query.EndWorkTime;
                    }
                }
            }
        }

        private void SetProductionResultDetailGrid()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ITEM", typeof(string));
            dt.Columns.Add("DFCT_SUM", typeof(string));
            dt.Columns.Add("DTL_DEFECT_LOT", typeof(string));
            dt.Columns.Add("DTL_LOSS_LOT", typeof(string));
            dt.Columns.Add("DTL_CHARGE_PROD_LOT", typeof(string));

            DataRow newRow = dt.NewRow();
            newRow["ITEM"] = ObjectDic.Instance.GetObjectName("발생수량");
            for (int col = 1; col < dt.Columns.Count; col++)
                newRow[col] = "0";

            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow["ITEM"] = ObjectDic.Instance.GetObjectName("비율");
            for (int col = 1; col < dt.Columns.Count; col++)
                newRow[col] = "%";

            dt.Rows.Add(newRow);
            Util.GridSetData(dgDetail, dt, null);
        }

        private void GetFaultyData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                newRow["EQPTID"] = EquipmentCode;

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROCACTRSN_CODE_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgFaulty, searchResult, FrameOperation, false);

                // Defect Column 생성..
                if (dgDetail.Rows.Count - dgDetail.TopRows.Count > 0)
                {
                    InitFaultyDataGrid();

                    // 선택된 LOT 동적으로 컬럼 생성(한건 만 처리)
                    string sColName = "DEFECTQTY" + (0 + 1).ToString();
                    int colIndex = 0;
                    colIndex = dgFaulty.Columns["COST_CNTR_NAME"].Index;
                    Util.SetGridColumnNumeric(dgFaulty, sColName, null, DvProductLot["LOTID"].GetString(), true, true, true, false, -1, HorizontalAlignment.Right, Visibility.Visible, colIndex, "#,##0");  // 부품 항목 앞으로 위치 이동.

                    if (dgFaulty.Columns.Contains(sColName))
                    {
                        (dgFaulty.Columns[sColName] as DataGridNumericColumn).Minimum = 0;
                        (dgFaulty.Columns[sColName] as DataGridNumericColumn).Maximum = 2147483647; // int max : 2147483647;
                        (dgFaulty.Columns[sColName] as DataGridNumericColumn).EditOnSelection = true;
                    }

                    if (dgFaulty.Rows.Count != 0)
                    {
                        DataTable dt = GetFaultyDataByLot(DvProductLot["LOTID"].GetString(), DvProductLot["WIPSEQ"].GetString());
                        BindingDataGrid(dgFaulty, dt, sColName);
                    }

                    //double defect, loss, chargeprod;
                    //SumDefectQty(dgFaulty, sColName, out defect, out loss, out chargeprod);

                    SumDefectQty(dgFaulty, sColName);

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetDefectInfo(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals("")) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(rowview["WIPSEQ"]);
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);
                //"10.61.7.62", "tcp", "7865", "SERVICE", "0"

                // 김용군 AZS Staking설비불량 별도 DA호출
                string bizName = string.Empty;

                if (ProcessCode == Process.AZS_STACKING)
                {
                    // 김용군 AZS Staking설비불량 Main설비 불량 집계하게 Biz수정하여 기존 DA호출하는거로 변경
                    //bizName = "DA_QCA_SEL_WIPRESONCOLLECT_AZS_L";
                    bizName = "DA_QCA_SEL_WIPRESONCOLLECT_L";
                }
                else
                {
                    bizName = "DA_QCA_SEL_WIPRESONCOLLECT_L";
                }

                //new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                new ClientProxy().ExecuteService(bizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDefect, searchResult, null, true);
                        SumDefectQty(dgDefect, "RESNQTY");
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
                HiddenLoadingIndicator();
            }
        }

        private void InitFaultyDataGrid(bool bClearAll = false)
        {
            if (bClearAll)
            {
                Util.gridClear(dgFaulty);

                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    if (Util.NVC(dgFaulty.Columns[i].Name).ToString().StartsWith("DEFECTQTY"))
                    {
                        dgFaulty.Columns.RemoveAt(i);
                    }
                }
            }
            else
            {
                // 기존 추가된 Col 삭제..                
                for (int i = dgFaulty.Columns.Count; i-- > 0;)
                {
                    if (Util.NVC(dgFaulty.Columns[i].Name).ToString().StartsWith("DEFECTQTY"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgFaulty.ItemsSource);
                        if (dt.Columns.Count > i)
                            if (dt.Columns[i].ColumnName.Equals(dgFaulty.Columns[i].Name))
                                dt.Columns.RemoveAt(i);

                        dgFaulty.Columns.RemoveAt(i);
                    }
                }
            }
        }

        private DataTable GetFaultyDataByLot(string sLotID, string sWipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("WIPSEQ", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow newRow = RQSTDT.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = ProcessCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipseq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                RQSTDT.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_L", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void BindingDataGrid(C1DataGrid dataGrid, DataTable dtRslt, string sColName)
        {
            DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);

            if (!dt.Columns.Contains(sColName))
            {
                dt.Columns.Add(sColName, typeof(int));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][sColName] = 0;
                }
            }

            if (dtRslt != null)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    for (int j = 0; j < dtRslt.Rows.Count; j++)
                    {
                        if (dtRslt.Rows[j]["ACTID"].Equals(dt.Rows[k]["ACTID"]) && dtRslt.Rows[j]["RESNCODE"].Equals(dt.Rows[k]["RESNCODE"]))
                        {
                            dt.Rows[k][sColName] = dtRslt.Rows[j]["RESNQTY"];

                            if (dt.Columns.Contains("RESNNOTE") && dtRslt.Columns.Contains("RESNNOTE"))
                            {
                                dt.Rows[k]["RESNNOTE"] = dtRslt.Rows[j]["RESNNOTE"];
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][sColName] = 0;
                }
            }

            dataGrid.BeginEdit();
            Util.GridSetData(dataGrid, dt, FrameOperation, false);

            C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dataGrid.Columns[sColName], new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });
            dataGrid.EndEdit();


        }

        private void GetEquipmentFaultyData()
        {
            try
            {
                if (string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode; //"N3ALAMM05";
                newRow["LOTID"] = DvProductLot["LOTID"].GetString(); //"UTKB31LCT3" 
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                inTable.Rows.Add(newRow);
                //"10.61.7.62", "tcp", "7865","SERVICE", "0"

                // 김용군 AZS Staking설비불량 별도 DA호출
                string bizName = string.Empty;

                if (ProcessCode == Process.AZS_STACKING)
                {
                    // 김용군 AZS Staking설비불량 Main설비 불량 집계하게 Biz수정하여 기존 DA호출하는거로 변경
                    //bizName = "DA_EQP_SEL_EQPTDFCTSTACKING_INFO_L";
                    bizName = "DA_EQP_SEL_EQPTDFCT_INFO_L";
                }
                else
                {
                    bizName = "DA_EQP_SEL_EQPTDFCT_INFO_L";
                }

                //new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                new ClientProxy().ExecuteService(bizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgEqpFaulty, searchResult, FrameOperation, true);

                        dgEqpFaulty.MergingCells -= dgEqpFaulty_MergingCells;
                        dgEqpFaulty.MergingCells += dgEqpFaulty_MergingCells;

                        if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                            dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                        else
                            dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //불량/LOSS/물품청구 메소드 호출 후 _isSearchAll true
                        if (!_isSearchAll) _isSearchAll = true;
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

        private void SetParentQty()
        {
            DataTable dt = GetChildEquipmentQty(DvProductLot["PR_LOTID"].GetString(), DvProductLot["LOTID"].GetString());

            if (CommonVerify.HasTableRow(dt))
            {
                if (Util.NVC(dt.Rows[0]["EQPT_UNMNT_TYPE_CODE"]).Equals("R"))
                {
                    tbParentQtyInfo.Visibility = Visibility.Visible;
                    grdParentQty.Visibility = Visibility.Visible;
                    txtParent2_M.Visibility = Visibility.Visible;
                    txtParent2.Visibility = Visibility.Visible;
                }
                else
                {
                    tbParentQtyInfo.Visibility = Visibility.Collapsed;
                    grdParentQty.Visibility = Visibility.Collapsed;
                    txtParent2_M.Visibility = Visibility.Collapsed;
                    txtParent2.Visibility = Visibility.Collapsed;

                    return;
                }

                string dTmp = "0";
                string dTmp2 = "0";

                dTmp = Util.NVC(dt.Rows[0]["WIPQTY_EA"]).Equals("") ? "0" : double.Parse(Util.NVC(dt.Rows[0]["WIPQTY_EA"])).ToString("#,##0");
                dTmp2 = Util.NVC(dt.Rows[0]["WIPQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(dt.Rows[0]["WIPQTY"])).ToString("#,##0");


                SetParentRemainQty(dTmp, dTmp2);
            }
        }

        private void SetParentRemainQty(string parentQtyEa, string parentQtyM)
        {
            try
            {
                if (dgDetail.GetRowCount() > 0)
                {
                    string inputQty = txtInputQty.Value.GetString();

                    if (double.Parse(parentQtyEa) - double.Parse(inputQty) < 0)
                    {
                        txtParent2.Text = "0";
                        txtParent2_M.Text = "0";
                    }
                    else
                    {
                        // 반올림 처리.
                        txtParent2.Text = Math.Round((double.Parse(parentQtyEa) - double.Parse(inputQty))).ToString("#,##0");
                        string sTmp = DvProductLot["PTN_LEN"].GetString();

                        if (string.IsNullOrEmpty(sTmp))
                        {
                            txtParent2_M.Text = txtParent2.Text;
                        }
                        else
                        {
                            double dTmp = 0;
                            double.TryParse(sTmp, out dTmp);
                            double dInputEa = double.Parse(inputQty);
                            // EA = 재공(M) / 패턴길이
                            // M = 재공(EA) * 패턴길이
                            txtParent2_M.Text = Math.Round((double.Parse(parentQtyM) - (dInputEa * dTmp))).ToString("#,##0");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetChildEquipmentQty(string parentLot, string lotId)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PR_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = parentLot;
                newRow["LOTID"] = lotId;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPTQTY_NT_L", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return dtResult;
                else
                    return null;
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

        private double SumDefectQty(C1DataGrid dg, string sColName, out double dfct, out double loss, out double charge_prd)
        {
            double sum = 0;
            dfct = 0;
            loss = 0;
            charge_prd = 0;

            if (!dg.Columns.Contains(sColName))
                return sum;

            for (int i = 0; i < dg.Rows.Count - dg.Rows.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")).Equals("N"))  // 실적 제외 여부 확인.
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("DEFECT_LOT"))
                        dfct += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                    else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("LOSS_LOT"))
                        loss += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                    else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                        charge_prd += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());

                    sum += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                }
            }
            return sum;
        }

        private void SumDefectQty(C1DataGrid dg, string sColName)
        {
            try
            {
                DataTable dtDefect = DataTableConverter.Convert(dg.ItemsSource);

                double inputQty;
                double goodQty;
                double defectSum = 0;
                double defect = 0;
                double loss = 0;
                double chargeProd = 0;

                C1NumericBox numericBox;

                if (ProcessCode == Process.NOTCHING)
                    numericBox = txtInputQty;
                else
                    numericBox = textInputQty;


                if (numericBox.Value.ToString() == double.NaN.ToString())
                    inputQty = 0;
                else
                    inputQty = numericBox.Value;

                for (int i = 0; i < dg.Rows.Count - dg.Rows.BottomRows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "RSLT_EXCL_FLAG")).Equals("N"))  // 실적 제외 여부 확인.
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("DEFECT_LOT"))
                            defect += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                        else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("LOSS_LOT"))
                            loss += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                        else if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                            chargeProd += double.Parse(dg.Columns[sColName].GetCellValue(dg.Rows[i]).ToString());
                        
                    }
                }

                defectSum = defect + loss + chargeProd;

                // 불량수
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DFCT_SUM", defectSum.Equals(0) ? "0" : defectSum.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_DEFECT_LOT", defect.Equals(0) ? "0" : defect.ToString("#,##0"));
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_LOSS_LOT", loss.Equals(0) ? "0" : loss.ToString("#,##0") );
                DataTableConverter.SetValue(dgDetail.Rows[0].DataItem, "DTL_CHARGE_PROD_LOT", chargeProd.Equals(0) ? "0" : chargeProd.ToString("#,##0"));

                // 불량율
                if (inputQty != 0)
                {
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DFCT_SUM", (defectSum / inputQty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_DEFECT_LOT", (defect / inputQty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_LOSS_LOT", (loss / inputQty).ToString("#0.##%"));
                    DataTableConverter.SetValue(dgDetail.Rows[1].DataItem, "DTL_CHARGE_PROD_LOT", (chargeProd / inputQty).ToString("#0.##%"));
                }

                goodQty = inputQty - defect - loss - chargeProd;

                //if (ProcessCode == Process.NOTCHING)
                //    numericBox = txtGoodQty;
                //else
                //    numericBox = textGoodQty;

                //if(!inputQty.Equals(0))
                //    numericBox.Value = goodQty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetRemark()
        {
            try
            {
                if (string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                rtxRemark.Document.Blocks.Clear();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_NOTE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            if(CommonVerify.HasTableRow(searchResult) && !string.IsNullOrEmpty(searchResult.Rows[0]["WIP_NOTE"].GetString()))
                                rtxRemark.AppendText(Util.NVC(searchResult.Rows[0]["WIP_NOTE"]));
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetRemark()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("WIP_NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                newRow["WIP_NOTE"] = new System.Windows.Documents.TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_WIPHISTORY_NOTE", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //저장 되었습니다.
                        Util.MessageInfo("SFU1270");
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

        private string GetWipStat()
        {
            try
            {
                string sRet = string.Empty;

                ShowLoadingIndicator();
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LOTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", inTable);

                HiddenLoadingIndicator();

                if (CommonVerify.HasTableRow(dtRslt))
                    if (dtRslt.Columns.Contains("WIPSTAT"))
                        sRet = Util.NVC(dtRslt.Rows[0]["WIPSTAT"]);

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
                return "";
            }
        }

        private void GetEquipmentConfigType()
        {
            try
            {
                if (string.IsNullOrEmpty(EquipmentCode))
                {
                    _equipmentConfigType = string.Empty;
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = EquipmentCode;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPT_CONF_TYPE", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("EQPT_CONF_TYPE"))
                {
                    _equipmentConfigType = Util.NVC(dtRslt.Rows[0]["EQPT_CONF_TYPE"]);
                }
            }
            catch (Exception ex)
            {
                _equipmentConfigType = string.Empty;
                Util.MessageException(ex);
            }
        }

        private void GetOutMagazine()
        {
            try
            {
                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_LM_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutLamination, searchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutMagazine(DataRowView rowview)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_LM_L";

                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals("")) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(rowview["LOTID"]);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgOutLamination, bizResult, FrameOperation, true);

                        // 생산반제품 생성,저장,삭제 후 양품수량(textGoodQty.Value) 항목 데이터 바인딩 
                        //decimal goodQty = bizResult.AsEnumerable().Sum(s => s.Field<decimal>("WIPQTY"));
                        //textGoodQty.Value = goodQty.GetDouble();
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

        private void GetOutStackingProduct(DataRowView rowview)
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_OUT_LOT_LIST_ST_L";

                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals("")) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(rowview["LOTID"]);
                inTable.Rows.Add(newRow);


                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");
                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (특이, 내역 컬럼 추가) 
                if (IsCommonCodeUse("STK_BOX_SPCL_MNG_AREA", LoginInfo.CFG_AREA_ID))
                {
                    dgOutStacking.Columns["CBO_SPCL"].Visibility = Visibility.Visible;
                    dgOutStacking.Columns["SPECIALDESC"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgOutStacking.Columns["CBO_SPCL"].Visibility = Visibility.Collapsed;
                    dgOutStacking.Columns["SPECIALDESC"].Visibility = Visibility.Collapsed;
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgOutStacking, bizResult, FrameOperation, true);
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

        private void GetOutPackagingTray(DataRowView rowview)
        {
            try
            {
                // Tray 관련 버튼 처리.
                SetOutTrayButtonEnable(null);

                if (DvProductLot == null) return;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LIST_CL();
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutPackaging, searchResult, FrameOperation, true);

                if (searchResult?.Copy()?.Select("PKG_VISUAL_EQPT_JUDG_VALUE <> ''")?.Length > 0)
                    dgOutPackaging.Columns["PKG_VISUAL_EQPT_JUDG_VALUE"].Visibility = Visibility.Visible;
                else
                    dgOutPackaging.Columns["PKG_VISUAL_EQPT_JUDG_VALUE"].Visibility = Visibility.Collapsed;

                if (searchResult?.Copy()?.Select("PKG_VISUAL_MANL_JUDG_VALUE <> ''")?.Length > 0)
                    dgOutPackaging.Columns["PKG_VISUAL_MANL_JUDG_VALUE"].Visibility = Visibility.Visible;
                else
                    dgOutPackaging.Columns["PKG_VISUAL_MANL_JUDG_VALUE"].Visibility = Visibility.Collapsed;

                //특별TRAY 콤보
                DataTable dt = new DataTable();
                dt.Columns.Add("CODE");
                dt.Columns.Add("NAME");
                dt.Rows.Add("N", "N");
                dt.Rows.Add("Y", "Y");

                (dgOutPackaging.Columns["CBO_SPCL"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt.Copy());

                GetOutTrayRSNCODE(); //사유 콤보박스 Setting

                if (!_previousValues.PreviousTray.Equals(""))
                {
                    int idx = _util.GetDataGridRowIndex(dgOutPackaging, "OUT_LOTID", _previousValues.PreviousTray);

                    if (idx >= 0)
                    {
                        DataTableConverter.SetValue(dgOutPackaging.Rows[idx].DataItem, "CHK", true);

                        dgOutPackaging.ScrollIntoView(idx, dgOutPackaging.Columns["CHK"].Index);

                        // Tray 관련 버튼 처리.
                        SetOutTrayButtonEnable(dgOutPackaging.Rows[idx]);
                        dgOutPackaging.CurrentCell = dgOutPackaging.GetCell(idx, dgOutPackaging.Columns.Count - 1);
                    }
                    else
                    {
                        if (dgOutPackaging.CurrentCell != null)
                            dgOutPackaging.CurrentCell = dgOutPackaging.GetCell(dgOutPackaging.CurrentCell.Row.Index, dgOutPackaging.Columns.Count - 1);
                        else if (dgOutPackaging.Rows.Count > 0 && dgOutPackaging.GetCell(dgOutPackaging.Rows.Count, dgOutPackaging.Columns.Count - 1) != null)
                            dgOutPackaging.CurrentCell = dgOutPackaging.GetCell(dgOutPackaging.Rows.Count, dgOutPackaging.Columns.Count - 1);
                    }
                }
                else
                {
                    if (dgOutPackaging.CurrentCell != null)
                        dgOutPackaging.CurrentCell = dgOutPackaging.GetCell(dgOutPackaging.CurrentCell.Row.Index, dgOutPackaging.Columns.Count - 1);
                    else if (dgOutPackaging.Rows.Count > 0 && dgOutPackaging.GetCell(dgOutPackaging.Rows.Count, dgOutPackaging.Columns.Count - 1) != null)
                        dgOutPackaging.CurrentCell = dgOutPackaging.GetCell(dgOutPackaging.Rows.Count, dgOutPackaging.Columns.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetOutTrayRSNCODE()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "SPCL_RSNCODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                DataRow dr2 = dtResult.NewRow();
                dr2["CBO_CODE"] = "";
                dr2["CBO_NAME"] = "";
                dtResult.Rows.InsertAt(dr2, 0);

                (dgOutPackaging.Columns["CBO_SPCL_RSNCODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetEquipmentSectionCutOffList(DataRowView rowview)
        {
            try
            {
                if (rowview == null || Util.NVC(rowview["LOTID"]).Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = Util.NVC(rowview["LOTID"]);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_EQPT_SECTION_CUTOFF", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //2020-01-25 OHB 파단수 계산
                        if (searchResult.Rows.Count > 0)
                        {
                            searchResult.Columns.Add(new DataColumn("CUT_EA", typeof(decimal)));

                            for (int i = 0; i < searchResult.Rows.Count; i++)
                            {
                                searchResult.Rows[i]["CUT_EA"] = Convert.ToDecimal(searchResult.Rows[i]["SECTION_EA"]) * Convert.ToDecimal(searchResult.Rows[i]["OCCR_COUNT"]);
                            }
                        }
                        HiddenLoadingIndicator();
                        Util.GridSetData(dgInputLoss, searchResult, FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetReInputInfo()
        {
            try
            {
                //int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                //if (idx < 0) return;

                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_LOT_EQPT_RE_INPUT_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            Util.GridSetData(dgReInput, searchResult, FrameOperation, false);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTrayInfo(out string sRet, out string sMsg)
        {
            try
            {
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_OUT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = EquipmentSegmentCode;
                newRow["EQPTID"] = EquipmentCode;
                newRow["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK")].DataItem, "TRAYID"));

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("WAIT") || Util.NVC(dtResult.Rows[0]["FORM_MOVE_STAT_CODE"]).Equals("CELL_MISMATCH"))
                    {
                        sRet = "OK";
                        sMsg = "";
                    }
                    else
                    {
                        sRet = "NG";
                        sMsg = "SFU3045";   // TRAY가 미확정 상태가 아닙니다.
                    }
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                sRet = "EXCEPTION";
                sMsg = ex.Message;
            }
        }

        private bool GetErpSendInfo(string lotId, string wipSeq)
        {
            try
            {
                bool bRet = false;
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_ERP_SEND_INFO();
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    // 'S' 가 아닌 경우는 삭제 가능.
                    if (!Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("S")) // P : ERP 전송 중 , Y : ERP 전송 완료
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

        private int GetNoreadCarrierCount(DataTable inTable)
        {
            try
            {
                int count = 0;

                if (inTable?.Rows?.Count < 1) return count;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOREAD_CST_CNT", "INDATA", "OUTDATA", inTable);
                if(CommonVerify.HasTableRow(dtRslt))
                {
                    count = Util.NVC(dtRslt.Rows[0]["NOREAD_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOREAD_CNT"]));
                }

                return count;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
        }

        private DataTable GetGroupPrintInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                string sTmp = string.Empty;
                for (int i = 0; i < dgOutLamination.Rows.Count - dgOutLamination.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutLamination, "CHK", i)) continue;

                    if (sTmp.Length < 1)
                        sTmp = Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID"));
                    else
                        sTmp = sTmp + "," + Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID"));
                }

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sTmp;
                newRow["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_GRP_PRT_INFO_FD", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();
                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetCarrierMapping(string sLotID, string sCstID)
        {
            try
            {
                if (sCstID.ToUpper().Equals("NOREAD")) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sLotID;
                newRow["CSTID"] = sCstID;
                newRow["PROCID"] = Process.LAMINATION;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CSTID_USING_RFID", "INDATA", null, inTable);

                // 변경되었습니다.   
                Util.MessageInfo("PSS9097", (msgResult) =>
                {
                    GetOutMagazine(DvProductLot);
                    //int idx = _util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    //if (idx >= 0)
                    //{
                    //    GetOutMagazine(dgProductLot.Rows[idx].DataItem as DataRowView);
                    //}
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (msgResult) =>
                {
                    GetOutMagazine(DvProductLot);
                });
            }
        }

        private DataTable GetSpecialTrayInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(EquipmentCode)) return null;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SPCL_TRAY_INFO_CL();
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = EquipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_LOT", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    chkOutTraySpl.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y") ? true : false;
                    txtOutTrayReamrk.Text = Util.NVC(dtResult.Rows[0]["SPCL_LOT_NOTE"]);

                    if (cboOutTraySplReason != null && cboOutTraySplReason.Items != null && cboOutTraySplReason.Items.Count > 0 && cboOutTraySplReason.Items.CurrentItem != null)
                    {
                        DataView dtview = (cboOutTraySplReason.Items.CurrentItem as DataRowView).DataView;
                        if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("CBO_CODE"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["CBO_CODE"]).Equals(Util.NVC(dtResult.Rows[0]["SPCL_LOT_RSNCODE"])))
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

                    if ((bool)chkOutTraySpl.IsChecked)
                    {
                        grdSpecialTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                        SpcLotID.Text = Util.NVC(dtResult.Rows[0]["SPCL_PROD_LOTID"]);
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

        // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (특별 Stacked Cell Tray 조회) 
        private DataTable GetSpecialStkTrayInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(EquipmentCode)) return null;
                if (string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return null;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIPATTR_RSV_EQPTID", "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    chkOutStkTraySpl.IsChecked = Util.NVC(dtResult.Rows[0]["SPCL_FLAG"]).Equals("Y") ? true : false;
                    txtOutStkTrayReamrk.Text = Util.NVC(dtResult.Rows[0]["SPCL_NOTE"]);

                    if (cboSpclEqpt != null && cboSpclEqpt.Items != null && cboSpclEqpt.Items.Count > 0 && cboSpclEqpt.Items.CurrentItem != null)
                    {
                        DataView dtview = (cboSpclEqpt.Items.CurrentItem as DataRowView).DataView;
                        if (dtview != null && dtview.Table != null && dtview.Table.Columns.Contains("EQPTID"))
                        {
                            bool bFnd = false;
                            for (int i = 0; i < dtview.Table.Rows.Count; i++)
                            {
                                if (Util.NVC(dtview.Table.Rows[i]["EQPTID"]).Equals(Util.NVC(dtResult.Rows[0]["RSV_EQPTID"])))
                                {
                                    cboSpclEqpt.SelectedIndex = i;
                                    bFnd = true;
                                    break;
                                }
                            }

                            if (!bFnd && cboSpclEqpt.Items.Count > 0)
                                cboSpclEqpt.SelectedIndex = 0;

                        }
                    }

                    if ((bool)chkOutStkTraySpl.IsChecked)
                    {
                        grdSpecialStkTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                        SpcStkLotID.Text = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    }
                    else
                    {
                        grdSpecialStkTrayMode.Visibility = Visibility.Collapsed;
                        cboSpclEqpt.SelectedIndex = 0;
                    }
                }

                else
                {
                    chkOutStkTraySpl.IsChecked = false;
                    txtOutStkTrayReamrk.Text = string.Empty;

                    if (cboSpclEqpt.Items.Count > 0)
                        cboSpclEqpt.SelectedIndex = 0;

                    if ((bool)chkOutStkTraySpl.IsChecked)
                    {
                        grdSpecialStkTrayMode.Visibility = Visibility.Visible;
                        ColorAnimationInSpecialTray();
                    }
                    else
                    {
                        grdSpecialStkTrayMode.Visibility = Visibility.Collapsed;
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
                        btnOutPackagingCreate.IsEnabled = true;
                        btnOutPackagingDel.IsEnabled = true;
                        btnOutPackagingConfirmCancel.IsEnabled = false;
                        btnOutPackagingConfirm.IsEnabled = true;
                        btnOutPackagingCell.IsEnabled = true;
                        btnOutPackagingSave.IsEnabled = true;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("ASSY_OUT")) // 조립출고확정
                    {
                        btnOutPackagingCreate.IsEnabled = true;
                        btnOutPackagingDel.IsEnabled = false;
                        btnOutPackagingConfirmCancel.IsEnabled = true;
                        btnOutPackagingConfirm.IsEnabled = false;
                        btnOutPackagingCell.IsEnabled = true;
                        btnOutPackagingSave.IsEnabled = false;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(dgRow.DataItem, "FORM_MOVE_STAT_CODE")).Equals("FORM_IN ")) // 활성화입고
                    {
                        btnOutPackagingCreate.IsEnabled = true;
                        btnOutPackagingDel.IsEnabled = false;
                        btnOutPackagingConfirmCancel.IsEnabled = false;
                        btnOutPackagingConfirm.IsEnabled = false;
                        btnOutPackagingCell.IsEnabled = true;
                        btnOutPackagingSave.IsEnabled = false;
                    }
                    else
                    {
                        btnOutPackagingCreate.IsEnabled = true;
                        btnOutPackagingDel.IsEnabled = true;
                        btnOutPackagingConfirmCancel.IsEnabled = true;
                        btnOutPackagingConfirm.IsEnabled = true;
                        btnOutPackagingCell.IsEnabled = true;
                        btnOutPackagingSave.IsEnabled = true;
                    }
                }
                else
                {
                    btnOutPackagingCreate.IsEnabled = true;
                    btnOutPackagingDel.IsEnabled = true;
                    btnOutPackagingConfirmCancel.IsEnabled = true;
                    btnOutPackagingConfirm.IsEnabled = true;
                    btnOutPackagingCell.IsEnabled = true;
                    btnOutPackagingSave.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowBcrWarning()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdBcrWarning.Visibility = Visibility.Visible;
                grdBcrWarning.RowDefinitions[0].Height = new GridLength(5);
                grdBcrWarning.RowDefinitions[2].Height = new GridLength(5);
                GridLengthAnimation gla = new GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 300);
                gla.Completed += ShowLengthAnimationCompleted;
                //gla.FillBehavior = FillBehavior.Stop;
                grdBcrWarning.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));
        }

        private void HideBcrWarning()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdBcrWarning.Visibility = Visibility.Collapsed;

                if (grdBcrWarning.RowDefinitions[1].Height.Value <= 0) return;
                grdBcrWarning.RowDefinitions[0].Height = new GridLength(0);
                grdBcrWarning.RowDefinitions[2].Height = new GridLength(0);
                GridLengthAnimation gla = new GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 300);
                gla.Completed += HideLengthAnimationCompleted;
                grdBcrWarning.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));
        }

        private void ShowLengthAnimationCompleted(object sender, EventArgs e)
        {
            grdBcrWarning.Visibility = Visibility.Visible;
            BcrWarnAnimationInRectangle(recBcrWarning);
        }

        private void HideLengthAnimationCompleted(object sender, EventArgs e)
        {
            grdBcrWarning.Visibility = Visibility.Collapsed;
            sbBcrWarning.Stop();
        }

        private void BcrWarnAnimationInRectangle(System.Windows.Shapes.Rectangle rec)
        {
            rec.Fill = edcBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(1.1);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "BCR_WARNING");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));

            sbBcrWarning.Children.Add(opacityAnimation);
            sbBcrWarning.Begin(this);
        }

        private void ColorAnimationInSpecialTray()
        {

            // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (특별 TRAY 컬러 적용) 
            if (ProcessCode == Process.STACKING_FOLDING)
            {
                recSpcStkTray.Fill = myAnimatedBrush;
                recSpcStkLot.Fill = myAnimatedBrush;
            }
            else
            {
                recSpcTray.Fill = myAnimatedBrush;
                recSpcLot.Fill = myAnimatedBrush;
            }

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

        #region[[Validation]

        private bool ValidationSectionCutOff()
        {
            if (string.IsNullOrEmpty(EquipmentSegmentCode))
            {
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (string.IsNullOrEmpty(EquipmentCode))
            {
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (DvProductLot == null)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (DvProductLot["WIPSTAT"].GetString() == "WAIT")
            {
                return false;
            }

            if (GetWipStat().Equals(Wip_State.END) || GetWipStat().Equals(""))
            {
                Util.MessageValidation("SFU2063"); // 재공상태를 확인해주세요.
                return false;
            }

            return true;
        }

        private bool ValidationSaveFaulty()
        {
            if (!CommonVerify.HasDataGridRow(dgFaulty))
            {
                //불량항목이 없습니다.
                Util.MessageValidation("SFU1588");
                return false;
            }

            // Lot 상태 체크
            if (GetWipStat().Equals(Wip_State.END) || GetWipStat().Equals(""))
            {
                Util.MessageValidation("SFU2063"); // 재공상태를 확인해주세요.
                return false;
            }

            return true;
        }

        private bool ValidationSaveDefect()
        {
            if (!CommonVerify.HasDataGridRow(dgDefect))
            {
                //불량항목이 없습니다.
                Util.MessageValidation("SFU1588");
                return false;
            }

            if(DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()) )
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationAddOutMagazine()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (txtOutLaminationCnt.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutLaminationCnt.Focus();
                return false;
            }

            if (Convert.ToDecimal(txtOutLaminationCnt.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutLaminationCnt.SelectAll();
                return false;
            }

            if ((Convert.ToDecimal(txtOutLaminationCnt.Text) % 1) > 0)
            {
                //Util.Alert("소수점 입력은 불가능 합니다. 수량을 확인해 주세요.");
                Util.MessageValidation("SFU2342");
                txtOutLaminationCnt.SelectAll();
                return false;
            }

            // LOT TYPE 에 따른 VALIDATION 처리            
            
            if (UnldrLotIdentBasCode.Equals("CST_ID"))
            {
                // MC/HC 인 경우만 체크. 
                if (!DvProductLot["PRODUCT_LEVEL2_CODE"].Equals("BC") && !DvProductLot["PRODUCT_LEVEL2_CODE"].Equals("MC"))
                {
                    if (txtOutLaminationCa.Text.Equals(""))
                    {
                        //Util.Alert("카세트ID를 입력 하세요.");
                        Util.MessageValidation("SFU6051");
                        txtOutLaminationCa.Focus();
                        return false;
                    }
                }
            }

            if (UnldrLotIdentBasCode.Equals("RF_ID"))
            {
                if (txtOutLaminationCa.Text.Trim().Equals(""))
                {
                    //Inline설비 && Mono Type일 경우 카세트ID 입력에 관한 조건 무시
                    if (!(DvProductLot["PRODUCT_LEVEL2_CODE"].Equals("MC") && _equipmentConfigType.Equals("INLINE")))
                    {
                        //Util.Alert("카세트ID를 입력 하세요.");
                        Util.MessageValidation("SFU6051");
                        txtOutLaminationCa.Focus();
                        return false;
                    }
                }
            }

            if (LoginInfo.CFG_AREA_ID == "A5" && !CheckedUseCassette())  //Cassette 중복여부 체크.
            {
                txtOutLaminationCa.SelectAll();
                return false;
            }
            return true;
        }

        private bool ValidationCreateBox()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (txtOutStackingBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutStackingBoxQty.Focus();
                return false;
            }

            if (Convert.ToDecimal(txtOutStackingBoxQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutStackingBoxQty.SelectAll();
                return false;
            }

            if (!CheckedUseCassette())  //Cassette 중복여부 체크.
            {
                txtOutStackingCa.SelectAll();
                txtOutStackingCa.Focus();
                return false;
            }
            return true;
        }

        private bool ValidationDeleteBox()
        {

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutStacking, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutStacking, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "LOTID")), Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "LOTID")));
                    return false;
                }
            }
            return true;
        }

        private bool ValidationSaveOutBox()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            for (int i = 0; i < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutStacking, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return false;
                }

                // 양품수량(전체수량 - 재작업수량) 이 음수 여부 체크
                double dBonusQty = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDouble(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "BONUS_QTY"));
                if ((dTmp - dBonusQty) < 0)
                {
                    Util.MessageValidation("SFU1721");  // 양품량은 음수가 될 수 없습니다.값을 맞게 변경하세요.
                    return false;
                }
            }
            return true;
        }

        private bool ValidationDeleteOutMagazine()
        {

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                Util.MessageValidation("SFU1645");
                return false;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutLamination, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgOutLamination.Rows.Count - dgOutLamination.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutLamination, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID")),Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID")));
                    return false;
                }
            }

            return true;
        }

        private bool ValidationSaveOutMagazine()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgOutLamination, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            for (int i = 0; i < dgOutLamination.Rows.Count - dgOutLamination.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutLamination, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationPrintOutMagazine()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("라미LOT이 선택되지 않았습니다.");
                Util.MessageValidation("SFU1519");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgOutLamination, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            for (int i = 0; i < dgOutLamination.Rows.Count - dgOutLamination.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutLamination, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
                {
                    //Util.Alert("진행중인 매거진은 발행할 수 없습니다.");
                    Util.MessageValidation("SFU1919");
                    return false;
                }

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return false;
                }

                if (UnldrLotIdentBasCode.Equals("RF_ID"))
                {
                    using (DataTable dtTmp = new DataTable())
                    {
                        dtTmp.Columns.Add("PR_LOTID", typeof(string));
                        dtTmp.Columns.Add("LOTID", typeof(string));

                        DataRow drTmp = dtTmp.NewRow();
                        drTmp["PR_LOTID"] = DvProductLot["LOTID"].GetString();
                        drTmp["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID"));
                        dtTmp.Rows.Add(drTmp);

                        if (GetNoreadCarrierCount(dtTmp) > 0)
                        {
                            // 캐리어가 맵핑되지 않은 Lot은 발행할 수 없습니다. (LOT : %1)
                            Util.MessageValidation("SFU4934", Util.NVC(DataTableConverter.GetValue(dgOutLamination.Rows[i].DataItem, "LOTID")));
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool ValidationStackingPrint()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgOutStacking, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            for (int i = 0; i < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutStacking, "CHK", i)) continue;

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOutStacking.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return false;
                }
            }
            return true;
        }

        private bool ValidationCreateTray()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationTrayDelete()
        {
            if (DvProductLot == null)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK");
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
            if (DvProductLot == null)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT") && !Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("CELL_MISMATCH"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }

            double dTmp = 0;

            if (double.TryParse(Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "CELLQTY")), out dTmp))
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

        private bool ValidationConfirmCancel()
        {
            if (DvProductLot == null)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationChangeCell()
        {
            if (_util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationSaveTray()
        {
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPackaging, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()) )
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            // 확정 여부 확인
            if (!Util.NVC(DataTableConverter.GetValue(dgOutPackaging.Rows[idx].DataItem, "FORM_MOVE_STAT_CODE")).Equals("WAIT"))
            {
                //Util.Alert("이미 확정 되었습니다.");
                Util.MessageValidation("SFU1235");
                return false;
            }
            
            return true;
        }

        private bool ValidationOutTraySplSave()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (chkOutTraySpl.IsChecked.HasValue && (bool)chkOutTraySpl.IsChecked)
            {
                if (cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue == null || cboOutTraySplReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("사유를 선택하세요.");
                    Util.MessageValidation("SFU1593");
                    return false;
                }

                if (txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    // 특별관리내역을 입력하세요.
                    Util.MessageValidation("SFU8322");
                    return false;
                }
            }
            else
            {
                if (!txtOutTrayReamrk.Text.Trim().Equals(""))
                {
                    //특별관리내역을 삭제하세요.
                    Util.MessageValidation("SFU8323");
                    return false;
                }
            }

            return true;
        }

        // 2024.01.08  남재현: STK 특별 Tray 설정 추가. (특별 TRAY Validation 추가) 
        private bool ValidationOutStkTraySplSave()
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return false;
            }

            if (chkOutStkTraySpl.IsChecked.HasValue && (bool)chkOutStkTraySpl.IsChecked)
            {
                if (cboSpclEqpt.SelectedValue == null || cboSpclEqpt.SelectedValue == null || cboSpclEqpt.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("목적 설비를 선택하세요.");
                    Util.MessageValidation("SFU10011");
                    return false;
                }

                if (txtOutStkTrayReamrk.Text.Trim().Equals(""))
                {
                    // 특별관리내역을 입력하세요.
                    Util.MessageValidation("SFU8322");
                    return false;
                }
            }
            else
            {
                if (!txtOutStkTrayReamrk.Text.Trim().Equals(""))
                {
                    //특별관리내역을 삭제하세요.
                    Util.MessageValidation("SFU8323");
                    return false;
                }
            }

            return true;
        }

        private bool CheckedUseCassette()
        {
            try
            {
                DataSet IndataSet = new DataSet();

                DataTable dtIN_EQP = IndataSet.Tables.Add("IN_EQP");
                dtIN_EQP.Columns.Add("SRCTYPE", typeof(string));
                dtIN_EQP.Columns.Add("IFMODE", typeof(string));
                dtIN_EQP.Columns.Add("CSTID", typeof(string));
                dtIN_EQP.Columns.Add("WIP_TYPE_CODE", typeof(string));

                DataRow dr = dtIN_EQP.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["CSTID"] = ProcessCode == Process.LAMINATION ? Util.NVC(txtOutLaminationCa.Text.Trim()) : Util.NVC(txtOutStackingCa.Text.Trim());
                dr["WIP_TYPE_CODE"] = "OUT";
                dtIN_EQP.Rows.Add(dr);


                DataTable dtIN_INPUT = IndataSet.Tables.Add("IN_INPUT");
                dtIN_INPUT.Columns.Add("LANGID", typeof(string));
                dtIN_INPUT.Columns.Add("PROCID", typeof(string));
                dtIN_INPUT.Columns.Add("EQSGID", typeof(string));

                dr = dtIN_INPUT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = ProcessCode;
                dr["EQSGID"] = EquipmentSegmentCode;
                dtIN_INPUT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_CST_MAPPING_DUP", "IN_EQP,IN_INPUT", null, IndataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckReInputDefectCode()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["COM_TYPE_CODE"] = "RE_INPUT_DFCT_CODE";
            inTable.Rows.Add(newRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion


        #region [팝업]

        private void PopupSectionCutOff()
        {
            ASSY005_001_EQPT_SECT_CUTOFF popupSectionCutOff = new ASSY005_001_EQPT_SECT_CUTOFF();
            popupSectionCutOff.FrameOperation = FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = EquipmentCode;
            parameters[1] = DvProductLot["LOTID"].GetString();
            parameters[2] = DvProductLot["WIPSEQ"].GetString();
            parameters[3] = EquipmentSegmentCode;;
            parameters[4] = EquipmentSegmentName;

            C1WindowExtension.SetParameters(popupSectionCutOff, parameters);

            popupSectionCutOff.Closed += new EventHandler(popupSectionCutOff_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupSectionCutOff.ShowModal()));
        }

        private void popupSectionCutOff_Closed(object sender, EventArgs e)
        {
            ASSY005_001_EQPT_SECT_CUTOFF popup = sender as ASSY005_001_EQPT_SECT_CUTOFF;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupCarrierHistory()
        {
            CMM_CST_HIST popHistory = new CMM_CST_HIST();
            popHistory.FrameOperation = FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = txtCarrierId.Text;
            C1WindowExtension.SetParameters(popHistory, parameters);

            popHistory.Closed += new EventHandler(popHistory_Closed);
            Dispatcher.BeginInvoke(new Action(() => popHistory.ShowModal()));
        }

        private void popHistory_Closed(object sender, EventArgs e)
        {
            CMM_CST_HIST popup = sender as CMM_CST_HIST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }



        #endregion


        #region PKG 공급 Carrier
        private bool CheckUnldrShrFlag()
        {
            try
            {
                bool bRet = false;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = IndataTable.NewRow();
                dr["EQPTID"] = EquipmentCode;

                IndataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", IndataTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["UNLDR_SHR_FLAG"]).Equals("Y"))
                    {
                        tabPkgSupplyCarrier.Visibility = Visibility.Visible;
                        bRet = true;
                    }
                    else
                        tabPkgSupplyCarrier.Visibility = Visibility.Collapsed;
                }
                else
                    tabPkgSupplyCarrier.Visibility = Visibility.Collapsed;

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
      
        private bool CanCreatePkgSupplyBox()
        {
            bool bRet = false;
            string returnMsg = string.Empty;
            
            if (txtOutPkgSupplyBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutPkgSupplyBoxQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutPkgSupplyBoxQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtOutPkgSupplyBoxQty.SelectAll();
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void CreatePkgSupplyBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUTPUT_QTY", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["PROD_LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["CSTID"] = Util.NVC(txtOutPkgSupplyCa.Text.Trim());
                newRow["OUTPUT_QTY"] = Convert.ToDecimal(txtOutPkgSupplyBoxQty.Text);
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_OUT_LOT_UNLOADER_FD_L", "IN_EQP", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException, (msgResult) =>
                            {
                                if (txtOutPkgSupplyCa.Visibility == Visibility.Visible)
                                {
                                    txtOutPkgSupplyCa.Text = "";
                                    txtOutPkgSupplyCa.Focus();
                                }
                            });

                            return;
                        }
                        
                        Util.MessageInfoAutoClosing("SFU1275");  //정상 처리 되었습니다.
                        txtOutPkgSupplyCa.Text = "";

                        if (txtOutPkgSupplyCa.Visibility == Visibility.Visible)
                            txtOutPkgSupplyCa.Focus();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CanDeletePkgSupplyBox()
        {
            bool bRet = false;
            
            int idx = _util.GetDataGridCheckFirstRowIndex(dgOutPkgSupply, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPSEQ")) != GetWipSeq(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID")))) //Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIP_WIPSEQ")))
                {
                    Util.MessageValidation("SFU5145", Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID")), ObjectDic.Instance.GetObjectName("삭제")); // 해당 Lot[%1]은 %2 처리할 수 없습니다.
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private string GetWipSeq(string sLotID)
        {
            try
            {
                string sRet = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LOTID"] = sLotID;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    sRet = Util.NVC(dtRslt.Rows[0]["WIPSEQ"]);
                }

                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void DeletePkgSupplyBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));


                inDataTable = indataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                inDataTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;

                    newRow = inDataTable.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID"));

                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_UNLOADER_FD_L", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        
                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CanSaveOutPkgSupplyBox()
        {
            bool bRet = false;

            if (_util.GetDataGridCheckFirstRowIndex(dgOutPkgSupply, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return bRet;
                }

                // 양품수량(전체수량 - 재작업수량) 이 음수 여부 체크
                double dBonusQty = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "BONUS_QTY")).Equals("") ? 0 : Convert.ToDouble(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "BONUS_QTY"));
                if ((dTmp - dBonusQty) < 0)
                {
                    Util.MessageValidation("SFU1721");  // 양품량은 음수가 될 수 없습니다.값을 맞게 변경하세요.
                    return bRet;
                }


                if (Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPSEQ")) != GetWipSeq(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID"))))  //Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIP_WIPSEQ")))
                {
                    Util.MessageValidation("SFU5145", Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID")), ObjectDic.Instance.GetObjectName("저장")); // 해당 Lot[%1]은 %2 처리할 수 없습니다.
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private void SavePkgSupplyBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                inDataTable = indataSet.Tables.Add("IN_INPUT");
                inDataTable.Columns.Add("OUT_LOTID", typeof(string));
                inDataTable.Columns.Add("OUTPUT_QTY", typeof(decimal));

                inDataTable = indataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = EquipmentCode;
                newRow["PROD_LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                inDataTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutPkgSupply.Rows.Count - dgOutPkgSupply.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgOutPkgSupply, "CHK", i)) continue;

                    newRow = inDataTable.NewRow();

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "LOTID"));
                    newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[i].DataItem, "WIPQTY")));

                    inDataTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_OUT_LOT_UNLOADER_FD_L", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            GetPkgSupplyProduct();
                            return;
                        }
                        
                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetPkgSupplyProduct()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(DvProductLot["LOTID"]);
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_UNLOADER_FD_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutPkgSupply, searchResult, FrameOperation, false);

                if (dgOutPkgSupply.GetRowCount() > 0)
                {
                    double dQty = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgOutPkgSupply.Rows[0].DataItem, "WIPQTY")));
                    txtOutPkgSupplyBoxQty.Text = Convert.ToString(dQty);
                }
                else
                {
                    txtOutPkgSupplyBoxQty.Text = "";
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

        private void CreatePkgSupplyBoxWithOutMsg()
        {
            try
            {
                if (!CanCreatePkgSupplyBox())
                    return;

                CreatePkgSupplyBox();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        // 김용군
        private void dgStackDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (sender == null) return;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        // 김용군 
                        //if (!Util.NVC(e.Cell.Column.Name).Equals("MACHINE"))
                        //{
                        //    string flag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "EQPT_DFCT_GR_SUM_YN"));
                        //    if (flag == "Y")
                        //    {
                        //        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //    }
                        //    else
                        //    {
                        //        e.Cell.Presenter.Background = null;
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //    }
                        //}
                        if (Util.NVC(e.Cell.Column.Name).Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // 합계 컬럼 색 변경.
                        if (!Util.NVC(e.Cell.Column.Name).Equals("MACHINE") && !Util.NVC(e.Cell.Column.Name).Equals("EQPT_DFCT_SUM_GR_NAME") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_DFCT_GR_SUM_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        // 김용군
        private void dgStackDefect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = (sender as C1DataGrid);
                if (sender == null) return;

                if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            CMM_ASSY_EQPT_DFCT_CELL_INFO popupDefectCellInfo = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
                            popupDefectCellInfo.FrameOperation = FrameOperation;

                            object[] parameters = new object[3];
                            parameters[0] = DvProductLot["LOTID"].GetString();
                            parameters[1] = EquipmentCode;
                            parameters[2] = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID"));
                            C1WindowExtension.SetParameters(popupDefectCellInfo, parameters);

                            //popupDefectCellInfo.Closed += new EventHandler(wndEqptDfctCell_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => popupDefectCellInfo.ShowModal()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 김용군
        private void dgStackDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        // 김용군 Stack별 불량정보 조회
        private void GetStkEquipmentFaultyData()
        {
            try
            {
                if (string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTSTKDFCT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgStackDefect, searchResult, FrameOperation, true);

                        //  김용군 Stack별 불량정보 
                        dgStackDefect.MergingCells -= dgStackDefect_MergingCells;
                        dgStackDefect.MergingCells += dgStackDefect_MergingCells;

                        if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                            dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                        else
                            dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        if (!_isSearchAll) _isSearchAll = true;
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

        // 김용군
        private void GetStkCompleteQtyData()
        {
            try
            {
                if (string.IsNullOrEmpty(DvProductLot["LOTID"].GetString())) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = EquipmentCode;
                newRow["LOTID"] = DvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = DvProductLot["WIPSEQ"].GetString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_STACK_INPUT_QTY_DRB", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgStackOutQty, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        if (!_isSearchAll) _isSearchAll = true;
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

        // 김용군
        private void dgStackDefect_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgStackDefect.TopRows.Count; i < dgStackDefect.Rows.Count; i++)
                {
                    if (dgStackDefect.Rows[i].DataItem.GetType() == typeof(DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") || Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
                        {
                            if (bStrt)
                            {
                                e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));
                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                                idxS = i;

                                if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                    bStrt = false;
                }
            }
            catch (Exception)
            {
                //Util.MessageException(ex);
            }
        }

        #endregion

        // 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
        private void btnOutStackingRemarkSave_Click(object sender, RoutedEventArgs e)
        {
            if (DvProductLot == null || string.IsNullOrEmpty(DvProductLot["LOTID"].GetString()))
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            _dispatcherTimerOutStacking?.Stop();

            // 저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    // Timer Stop.
                    _dispatcherTimerOutStacking?.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        SaveOutBoxRemark();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    // Timer Start.
                    if (_dispatcherTimerOutStacking != null && _dispatcherTimerOutStacking.Interval.TotalSeconds > 0)
                        _dispatcherTimerOutStacking.Start();
                }
            });
        }

        // 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
        private void SaveOutBoxRemark()
        {
            try
            {
                ShowLoadingIndicator();
                
                DataSet dtIndataSet = new DataSet();

                DataTable dtInDataTable = dtIndataSet.Tables.Add("IN_EQP");
                dtInDataTable.Columns.Add("SRCTYPE", typeof(string));
                dtInDataTable.Columns.Add("IFMODE", typeof(string));
                dtInDataTable.Columns.Add("EQPTID", typeof(string));
                dtInDataTable.Columns.Add("PROD_LOTID", typeof(string));
                dtInDataTable.Columns.Add("USERID", typeof(string));

                DataTable dtInLot = dtIndataSet.Tables.Add("IN_LOT");
                dtInLot.Columns.Add("OUT_LOTID", typeof(string));
                dtInLot.Columns.Add("CSTID", typeof(string));
                dtInLot.Columns.Add("WIPQTY", typeof(int));
                dtInLot.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
                dtInLot.Columns.Add("SPCL_CST_NOTE", typeof(string));
                dtInLot.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

                DataTable dtInTable = dtIndataSet.Tables["IN_EQP"];
                DataRow drNewRow = dtInTable.NewRow();
                drNewRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drNewRow["IFMODE"] = IFMODE.IFMODE_OFF;
                drNewRow["EQPTID"] = EquipmentCode;
                drNewRow["PROD_LOTID"] = DvProductLot["LOTID"].GetString();
                drNewRow["USERID"] = LoginInfo.USERID;
                dtInTable.Rows.Add(drNewRow);
                drNewRow = null;

                for (int iIdx = 0; iIdx < dgOutStacking.Rows.Count - dgOutStacking.BottomRows.Count; iIdx++)
                {
                    if (_util.GetDataGridCheckValue(dgOutStacking, "CHK", iIdx) == false)
                    {
                        continue;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[iIdx].DataItem, "PRE_WIP_NOTE")) == Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[iIdx].DataItem, "WIP_NOTE")))
                    {
                        continue;
                    }

                    // Box 정보 DataTable             
                    drNewRow = dtInLot.NewRow();
                    drNewRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[iIdx].DataItem, "LOTID"));
                    drNewRow["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[iIdx].DataItem, "CSTID"));
                    drNewRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOutStacking.Rows[iIdx].DataItem, "WIP_NOTE"));

                    dtInLot.Rows.Add(drNewRow);
                }

                if(dtInLot.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1566");
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_SPCL", "IN_EQP,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetOutStackingProduct(DvProductLot);

                        // 생산량, 양품량 재조회
                        GetInputQoodQty();

                        IsProductLotRefreshFlag = true;

                        //Util.AlertInfo("정상 처리 되었습니다.");
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
                }, dtIndataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        // 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
        private void SetOutLotRemarkUse(string sProcId)
        {
            try
            {
                btnOutStackingRemarkSave.Visibility = Visibility.Collapsed;
                dgOutStacking_Remark.Visibility = Visibility.Collapsed;

                DataTable dtInTable = new DataTable("RQSTDT");
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("COM_CODE", typeof(string));

                DataRow drNewRow = dtInTable.NewRow();
                drNewRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                drNewRow["COM_TYPE_CODE"] = "OUTLOT_REMARK_COL_USE";
                drNewRow["COM_CODE"] = sProcId;
                dtInTable.Rows.Add(drNewRow);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dtInTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    btnOutStackingRemarkSave.Visibility = Visibility.Visible;
                    dgOutStacking_Remark.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                // NoAction
            }
        }

        // 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
        private void GetAssyStackCellInfoNoUse()
        {
            try
            {
                DataTable dtInTable = new DataTable("RQSTDT");
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("COM_CODE", typeof(string));

                DataRow drNewRow = dtInTable.NewRow();
                drNewRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                drNewRow["COM_TYPE_CODE"] = "EXEC_YN_FOR_REMOVE_AREA_HARD_CODE";
                drNewRow["COM_CODE"] = "ASSY_STACK_CELL_INFO_NOUSE";
                dtInTable.Rows.Add(drNewRow);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dtInTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _isAssyStackCellInfoNoUse = true;
                }
            }
            catch
            {
                // NoAction
            }
        }

        private void dgOutStacking_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null)
            {
                if (DataTableConverter.GetValue(e.Row.DataItem, "DISPATCH_YN").Equals("N"))
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
