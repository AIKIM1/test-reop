/*************************************************************************************
 Created Date : 2020.11.13
      Creator : KANG DONG HEE
   Decription : 활성화 공정진척
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.01  KDH    : 공정그룹 추가 및 개인별 설정된 공정이 없을 경우 에러 발생에 대한 조치
  2021.04.06  KDH    : Line별 공정그룹 Setting.
  2021.04.07  KDH    : Group Lot Link 기능 추가(활성화 Lot 관리 화면으로).
  2021.04.20  KDH    : 공통 Line에 대해 공정 List Setting되게 로직 추가
  2022.05.26  이제섭 : Line 콤보 조회 시, AREAID 파라미터 제거. (DA에서 처리)
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
using LGC.GMES.MES.FCS001.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Documents;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_095 : IWorkArea
    {
        #region Declaration

        public UcFCSCommand UcFCSCommand { get; set; }
        public UcFCSEquipment UcFCSEquipment { get; set; }
        public UcFCSProductLot UcFCSProductLot { get; set; }
        public UcFCSNGGroupLot UcFCSNGGroupLot { get; set; }
        public UcFCSDefectDetail UcFCSDefectDetail { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private C1DataGrid DgEquipment { get; set; }
        private C1DataGrid DgProductLot { get; set; }
        private C1DataGrid DgNgGroupLot { get; set; }
        private C1DataGrid DeDefectDetail { get; set; }


        private string _equipmentSegmentCode;
        private string _equipmentSegmentName;
        private string _processCode;
        private string _processName;
        private string _equipmentCode;
        private string _equipmentName;
        private string _treeEquipmentCode;
        private string _treeEquipmentName;
        private string _productLot;

        private string _labelPassHoldFlag = string.Empty;

        DataRowView _dvProductLot;
        DataRowView _dvNgGroupLot;

        private System.Windows.Threading.DispatcherTimer _timer = null;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public FCS001_095()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            SetComboBox();

            InitializeUserControls();
            SetEventInUserControls();
            SeProcessInUserControls();
            TimerSetting();
        }

        private void SetComboBox()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            // 라인
            SetEquipmentSegmentCombo(cboEquipmentSegment);

            // 공정 그룹
            SetProcessGroupCombo(cboProcGrpCode); //2021.04.06  KDH: Line별 공정그룹 Setting.

           // 공정
           SetProcessCombo(cboProcess);

            // 설비
            SetEquipmentCombo(cboEquipment);

            // 자동조회
            String[] sFilter3 = { "SECOND_INTERVAL" };
            _combo.SetCombo(cboAutoSearch, CommonCombo_Form.ComboStatus.NA, sFilter: sFilter3, sCase: "COMMCODE");

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
        }

        private void InitializeUserControls()
        {
            UcFCSCommand = grdCommand.Children[0] as UcFCSCommand;
            UcFCSEquipment = grdEquipment.Children[0] as UcFCSEquipment;
            UcFCSProductLot = grdProduct.Children[0] as UcFCSProductLot;
            UcFCSNGGroupLot = grdNgGroupLot.Children[0] as UcFCSNGGroupLot;
            UcFCSDefectDetail = grdDefectDetail.Children[0] as UcFCSDefectDetail;

            if (UcFCSCommand != null)
            {
                UcFCSCommand.FrameOperation = FrameOperation;
                UcFCSCommand.SetButtonVisibility(true);
                UcFCSCommand.SetButtonExtraVisibility();
                UcFCSCommand.ApplyPermissions();
            }

            if (UcFCSEquipment != null)
            {
                UcFCSEquipment.UcParentControl = this;
                UcFCSEquipment.FrameOperation = FrameOperation;
                UcFCSEquipment.DgEquipment.MouseLeftButtonUp += DgEquipment_MouseLeftButtonUp;
                UcFCSEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
                UcFCSEquipment.ProcessCode = _processCode;
                UcFCSEquipment.EquipmentCode = _equipmentCode;

                DgEquipment = UcFCSEquipment.dgEquipment;
                SetEquipmentTree();
            }

            if (UcFCSProductLot != null)
            {
                UcFCSProductLot.UcParentControl = this;
                UcFCSProductLot.FrameOperation = FrameOperation;
                UcFCSProductLot.DgProductLot.MouseLeftButtonUp += DgProductLot_MouseLeftButtonUp;
                UcFCSProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
                UcFCSProductLot.ProcessCode = _processCode;
                UcFCSProductLot.EquipmentCode = _equipmentCode;

                DgProductLot = UcFCSProductLot.dgProductLot;
                SetProductLotList();

                if (cboEquipment.SelectedItems.Count() > 0)
                    UcFCSProductLot.txtSelectEquipment.Text = _equipmentCode;
            }

            if (UcFCSNGGroupLot != null)
            {
                UcFCSNGGroupLot.UcParentControl = this;
                UcFCSNGGroupLot.FrameOperation = FrameOperation;
                UcFCSNGGroupLot.DgNgGroupLot.PreviewMouseDoubleClick += DgNgGroupLot_PreviewMouseDoubleClick;
                UcFCSNGGroupLot.EquipmentSegmentCode = _equipmentSegmentCode;
                UcFCSNGGroupLot.ProcessCode = _processCode;
                UcFCSNGGroupLot.EquipmentCode = _equipmentCode;

                DgNgGroupLot = UcFCSNGGroupLot.dgNgGroupLot;
                SetNGGroupLotList();
            }

            if (UcFCSDefectDetail != null)
            {
                UcFCSDefectDetail.FrameOperation = FrameOperation;
                UcFCSDefectDetail.EquipmentSegmentCode = _equipmentSegmentCode;
                UcFCSDefectDetail.ProcessCode = _processCode;
                UcFCSDefectDetail.EquipmentCode = _equipmentCode;

                DeDefectDetail = UcFCSDefectDetail.dgDefectDetail;
            }
        }

        private void SetEventInUserControls()
        {
            if (UcFCSCommand != null)
            {
                UcFCSCommand.ButtonOperResult.Click += ButtonOperResult_Click;                               // 공정별 실적조회
                UcFCSCommand.ButtonCreateTrayInfo.Click += ButtonCreateTrayInfo_Click;                       // Tray정보 생성
                UcFCSCommand.ButtonProductList.Click += ButtonProductList_Click;                             // 공정 홈 화면전환
            }
        }

        private void SeProcessInUserControls()
        {
        }

        private void TimerSetting()
        {
            if (cboAutoSearch.SelectedValue == null || string.IsNullOrWhiteSpace(cboAutoSearch.SelectedValue.ToString()))
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            _timer = new System.Windows.Threading.DispatcherTimer();
            int interval = Convert.ToInt32(cboAutoSearch.SelectedValue);

            _timer.Interval = TimeSpan.FromSeconds(interval);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 라인
        /// </summary>
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlEquipmentSegment();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // 공정 그룹
            SetProcessGroupCombo(cboProcGrpCode); //2021.04.06  KDH: Line별 공정그룹 Setting.

            // Clear
            SetControlClear();

            // 메인화면으로
            ButtonProductList_Click(null, null);
        }

        /// <summary>
        /// 2021.03.31 공정그룹 추가
        /// 공정그룹
        /// </summary>
        private void cboProcGrpCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 공정 
            SetProcessCombo(cboProcess);
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlProcess();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // 설비 
            SetEquipmentCombo(cboEquipment);
        }

        /// <summary>
        /// 설비 
        /// </summary>
        private void cboEquipment_SelectionChanged(object sender, EventArgs e)
        {
            SetEquipment("C");

            // 메인화면으로
            ButtonProductList_Click(null, null);
        }

        /// <summary>
        /// 자동조회
        /// </summary>
        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            TimerSetting();
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SetEquipment("L");
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                UcFCSEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);

                if (grdProduct.Visibility == Visibility.Visible)
                {
                    string SelectLotID = _dvProductLot == null ? null : _dvProductLot["LOTID"].ToString();
                    SetProductLotList(SelectLotID);       // Product Lot 조회
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 설비 선택 
        /// </summary>
        private void DgEquipment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = DgEquipment.GetCellFromPoint(pnt);

            if (cell != null) return;

            if (DgEquipment.CurrentCell == null) return;

            string Equipment = DgEquipment.CurrentCell.Row.DataItem.ToString();

            if (string.IsNullOrWhiteSpace(Equipment) || Equipment.Split(':').Length < 2)
            {
                _treeEquipmentCode = string.Empty;
                _treeEquipmentName = string.Empty;
            }
            else
            {
                _treeEquipmentCode = Equipment.Split(':')[0].Trim();
                _treeEquipmentName = Equipment.Split(':')[1].Trim();

                SetEquipment("T");

                // Product Lot 정렬
                DataTable dt = DataTableConverter.Convert(DgProductLot.ItemsSource);

                if (dt == null || dt.Rows.Count == 0) return;

                // 해당 설비로 정력 되었거나 설비가 한개인 경우
                if (dt.Rows[0]["EQPTID"].ToString() == _treeEquipmentCode) return;

                DataRow[] drSelect = dt.Select("EQPTID = '" + _treeEquipmentCode + "'");

                // 해당 설비가 생산 Lot 정보가 없는 경우
                if (drSelect.Length == 0) return;

                DataRow[] drNoneSelect = dt.Select("EQPTID <> '" + _treeEquipmentCode + "'");
                DataTable dtSort = dt.Clone();

                for (int row = 0; row < drSelect.Length; row++)
                {
                    dtSort.ImportRow(drSelect[row]);
                }

                for (int row = 0; row < drNoneSelect.Length ; row++)
                {
                    dtSort.ImportRow(drNoneSelect[row]);
                }

                Util.GridSetData(DgProductLot, dtSort, null, true);
            }
        }

        /// <summary>
        /// 재공 Lot 선택 
        /// </summary>
        private void DgProductLot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null) return;

            SetProductLotSelect(dg.CurrentRow.Index);
        }

        private void DgNgGroupLot_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CurrentCell == null) return;

            ////////////////////////////////////////////////////////////////////// 화면 전환
            if (dg.CurrentCell.Column.Name.ToString() == "LOTID")
            {
                dg.CurrentCell.Presenter.Cursor = Cursors.Wait;

                //UcFCSCommand.SetButtonVisibility(false);

                //grdNgGroupLot.Visibility = Visibility.Collapsed;
                //grdDefectDetail.Visibility = Visibility.Visible;

                //SetUserControlDefectDetail();
                //UcFCSDefectDetail.SelectProductionResult();

                //dg.CurrentCell.Presenter.Cursor = Cursors.Hand;


                object[] parameters = new object[1];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, "LOTID"));

                this.FrameOperation.OpenMenu("SFU010705220", true, parameters); //활성화 Lot 관리 화면 연계
                dg.CurrentCell.Presenter.Cursor = Cursors.Hand;
            }

        }

        public void ButtonProductList_Click(object sender, RoutedEventArgs e)
        {
            UcFCSCommand.SetButtonVisibility(true);

            grdProduct.Visibility = Visibility.Visible;
            grdNgGroupLot.Visibility = Visibility.Visible;
            grdDefectDetail.Visibility = Visibility.Collapsed;

            if (UcFCSDefectDetail.bProductionUpdate)
            {
                string SelectLotID = _dvProductLot["LOTID"].ToString();

                UcFCSDefectDetail.bProductionUpdate = false;

                SelectProductLot(SelectLotID);
                SetProductLotList(SelectLotID);
            }
        }
        
        /// <summary>
        /// 공정별 실적조회
        /// </summary>
        private void ButtonOperResult_Click(object sender, RoutedEventArgs e)
        {
            PopupOperResult();
        }

        /// <summary>
        /// Tray정보 생성
        /// </summary>
        private void ButtonCreateTrayInfo_Click(object sender, RoutedEventArgs e)
        {
            PopupCreateTrayInfo();
        }
        
        #endregion

        #region Mehod

        #region [BizCall]
        #region =============================공통
        /// <summary>
        /// 라인
        /// </summary>
        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_LINE";
            string[] arrColumn = { "LANGID" /*, "AREAID"*/ };
            string[] arrCondition = { LoginInfo.LANGID /*, LoginInfo.CFG_AREA_ID*/ };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";
            
            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, null);
            //LoginInfo.CFG_EQSG_ID
            cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
            if (!string.IsNullOrEmpty(Util.NVC(cboEquipmentSegment.SelectedValue)))
            {
                _equipmentSegmentCode = cboEquipmentSegment.SelectedValue.ToString();
                _equipmentSegmentName = cboEquipmentSegment.Text;
            }
            else
            {
                _equipmentSegmentCode = string.Empty;
                _equipmentSegmentName = string.Empty;
            }
        }

        /// <summary>
        /// 공정그룹
        /// </summary>
        private void SetProcessGroupCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_PROCESS_GROUP_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString(), "PROC_GR_CODE" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// 공정
        /// </summary>
        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_PROC_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "S26" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString(), cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.NONE, selectedValueText, displayMemberText, null); //2021.03.31 기본 공정이 없을 경우 에러 발생에 대한 조치
            
            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 START
            DataTable dtcbo = DataTableConverter.Convert(cbo.ItemsSource);
            if (dtcbo == null || dtcbo.Rows.Count == 0)
            {
                const string bizRuleName1 = "DA_BAS_SEL_ALL_OP_CBO";
                string[] arrColumn1 = { "LANGID", "PROC_GR_CODE" };
                string[] arrCondition1 = { LoginInfo.LANGID, cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };
                string selectedValueText1 = "CBO_CODE";
                string displayMemberText1 = "CBO_NAME";

                CommonCombo_Form.CommonBaseCombo(bizRuleName1, cbo, arrColumn1, arrCondition1, CommonCombo_Form.ComboStatus.NONE, selectedValueText1, displayMemberText1, null);
            }
            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 END

        }

        /// <summary>
        /// 설비
        /// </summary>
        private void SetEquipmentCombo(MultiSelectionBox mcb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue == null ? null : cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_EQP_BY_PROC", "RQSTDT", "RSLTDT", RQSTDT);

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
                        for (int row = 0; row < dtResult.Rows.Count; row++)
                        {
                            if (dtResult.Rows[row]["CBO_CODE"].ToString() == LoginInfo.CFG_EQPT_ID)
                            {
                                mcb.Check(row);
                            }
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

        #endregion

        #endregion

        #region [Func]
        #region =============================공통
        private void ApplyPermissions()
        {
            if (UcFCSCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }

        private void SetControlClear()
        {
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            _productLot = string.Empty;
            _dvProductLot = null;

            Util.gridClear(UcFCSEquipment.DgEquipment);
            Util.gridClear(UcFCSProductLot.DgProductLot);
            Util.gridClear(UcFCSNGGroupLot.DgNgGroupLot);
            Util.gridClear(UcFCSDefectDetail.DgDefectDetail);

            UcFCSProductLot.txtSelectEquipment.Text = string.Empty;
            UcFCSProductLot.txtSelectLot.Text = string.Empty;
        }

        private void SetUserControlEquipmentSegment()
        {
            if (!string.IsNullOrEmpty(Util.NVC(cboEquipmentSegment.SelectedValue)))
            {
                _equipmentSegmentCode = cboEquipmentSegment.SelectedValue.ToString();
                _equipmentSegmentName = cboEquipmentSegment.Text;
            }
            else
            {
                _equipmentSegmentCode = string.Empty;
                _equipmentSegmentName = string.Empty;
            }

            SetUserControlEquipment();
            SetUserControlProductLot();
            SetUserControlGNGroupLot();
            SetUserControlDefectDetail();
        }

        private void SetUserControlProcess()
        {
            if (!string.IsNullOrEmpty(Util.NVC(cboProcess.SelectedValue)))
            {
                _processCode = cboProcess.SelectedValue.ToString();
                _processName = cboProcess.Text;
            }
            else
            { 
                _processCode = string.Empty;
                _processName = string.Empty;
            }

            SetUserControlEquipment();
            SetUserControlProductLot();
            SetUserControlGNGroupLot();
            SetUserControlDefectDetail();
        }

        private void SetUserControlEquipment()
        {
            UcFCSEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
            UcFCSEquipment.ProcessCode = _processCode;
            UcFCSEquipment.ProcessName = _processName;
            UcFCSEquipment.EquipmentCode = cboEquipment.SelectedItemsToString;
        }

        private void SetUserControlProductLot()
        {
            UcFCSProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
            UcFCSProductLot.ProcessCode = _processCode;
            UcFCSProductLot.ProcessName = _processName;
            UcFCSProductLot.EquipmentCode = cboEquipment.SelectedItemsToString;
        }

        private void SetUserControlGNGroupLot()
        {
            UcFCSNGGroupLot.EquipmentSegmentCode = _equipmentSegmentCode;
            UcFCSNGGroupLot.ProcessCode = _processCode;
            UcFCSNGGroupLot.ProcessName = _processName;
            UcFCSNGGroupLot.EquipmentCode = cboEquipment.SelectedItemsToString;
        }

        private void SetUserControlDefectDetail()
        {
            UcFCSDefectDetail.EquipmentSegmentCode = _equipmentSegmentCode;
            UcFCSDefectDetail.ProcessCode = _processCode;
            UcFCSDefectDetail.ProcessName = _processName;
            UcFCSDefectDetail.EquipmentCode = cboEquipment.SelectedItemsToString;
            UcFCSDefectDetail.EquipmentName = _equipmentName;
        }

        public void SetUserControlEquipmentDataTable()
        {
            // 선택 설비가 없다면 return
            if (string.IsNullOrWhiteSpace(_equipmentCode)) return;

            if (DgEquipment.ItemsSource == null || DgEquipment.GetRowCount() == 0) return;

            DataTable dt = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "'").CopyToDataTable();

            UcFCSProductLot.DtEquipment = dt;
            UcFCSNGGroupLot.DtEquipment = dt;
            UcFCSDefectDetail.DtEquipment = dt;
        }

        public void SetProductLotSelect(int iRow)
        {
            // row 색 바꾸기
            DgProductLot.SelectedIndex = iRow;

            ////////////////////////////////////////////////////////////////////////////////
            _dvProductLot = DgProductLot.Rows[iRow].DataItem as DataRowView;

            if (_dvProductLot == null) return;

            SelectEquipment(_dvProductLot["EQPTID"].ToString(), _dvProductLot["EQPTNAME"].ToString());
            SelectProductLot(_dvProductLot["LOTID"].ToString());
        }

        private void SetEquipment(string sSelect)
        {
            if (!string.IsNullOrEmpty(Util.NVC(cboEquipmentSegment.SelectedValue)))
            {
                _equipmentSegmentCode = cboEquipmentSegment.SelectedValue.ToString();
                _equipmentSegmentName = cboEquipmentSegment.Text;
            }
            else
            {
                _equipmentSegmentCode = string.Empty;
                _equipmentSegmentName = string.Empty;
            }

            if (!string.IsNullOrEmpty(Util.NVC(cboProcess.SelectedValue)))
            {
                _processCode = cboProcess.SelectedValue.ToString();
                _processName = cboProcess.Text;
            }
            else
            {
                _processCode = string.Empty;
                _processName = string.Empty;
            }

            switch (sSelect)
            {
                ////////////////////////////////// 조회, 설비 콤보 변경
                case "L":
                case "C":
                    if (string.IsNullOrWhiteSpace(cboEquipment.SelectedItemsToString))
                    {
                        SetControlClear();
                        break;
                    }

                    if (grdDefectDetail.Visibility.Equals(Visibility.Visible))
                    {
                        UcFCSCommand.SetButtonVisibility(true);

                        grdNgGroupLot.Visibility = Visibility.Visible;
                        grdDefectDetail.Visibility = Visibility.Collapsed;
                    }

                    // 설비 Tree 조회
                    SetEquipmentTree();

                    SelectEquipment(cboEquipment.SelectedItemsToString, null);

                    // Product Lot 조회
                    SetProductLotList();

                    //ReCheck / NG Group Lot 조회
                    SetNGGroupLotList();
                    break;
                ///////////////////////////////// 설비 Tree Click
                case "T":
                    _equipmentCode = _treeEquipmentCode;
                    _equipmentName = _treeEquipmentName;

                    SelectEquipment(_equipmentCode, _equipmentName);
                    SelectProductLot(string.Empty);
                    break;

                // Product Lot Click
                case "P":
                    break;
            }

        }

        private void SetEquipmentTree()
        {
            // Clear
            SetControlClear();

            if (cboEquipment.SelectedItems.Count() > 0)
            {
                UcFCSEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        private void SetProductLotList(string LotID = null)
        {
            //UcFCSProductLot.SetControlVisibility();

            if (cboEquipment.SelectedItems.Count() > 0)
            {
                SetUserControlProductLot();
                UcFCSProductLot.SelectProductList(LotID);
            }
        }
        private void SetNGGroupLotList(string LotID = null)
        {
            if (cboEquipment.SelectedItems.Count() > 0)
            {
                SetUserControlProductLot();
                UcFCSNGGroupLot.SelectNgGroupLotList(LotID);
            }
        }

        private void SelectEquipment(string EqptID, string EqptName)
        {
            if (EqptID.Split(',').Length == 1)
            {
                _equipmentCode = EqptID;
                _equipmentName = EqptName;
                UcFCSProductLot.txtSelectEquipment.Text = _equipmentCode;
            }
            else
            {
                _equipmentCode = string.Empty;
                _equipmentName = string.Empty;
                UcFCSProductLot.txtSelectEquipment.Text = string.Empty;
            }

            SetUserControlEquipment();
            SetUserControlProductLot();
            SetUserControlGNGroupLot();
            SetUserControlDefectDetail();
        }

        private void SelectProductLot(string LotID)
        {
            if (string.IsNullOrWhiteSpace(LotID))
            {
                _dvProductLot = null;
            }

            _productLot = LotID;

            UcFCSProductLot.txtSelectLot.Text = _productLot;
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

        #region[[Validation]
        #region =============================공통
        private bool ValidationLogisStat()
        {
            if (string.IsNullOrWhiteSpace(UcFCSProductLot.txtSelectEquipment.Text))
            {
                // 설비를 선택 하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_processCode))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            return true;
        }

        #endregion

        #endregion

       
        /// <summary>
        /// 공정별 실적조회
        /// </summary>
        private void PopupOperResult()
        {
            object[] Parameters = new object[3];
            Parameters[0] = _processCode;
            Parameters[1] = _equipmentCode;
            Parameters[2] = _equipmentName;
            this.FrameOperation.OpenMenu("SFU010715020", true, Parameters);
        }

        /// <summary>
        /// Tray정보 생성
        /// </summary>
        private void PopupCreateTrayInfo()
        {
            object[] Parameters = new object[0];
            this.FrameOperation.OpenMenu("SFU010710030", true, Parameters);
        }


        #endregion

    }
}
