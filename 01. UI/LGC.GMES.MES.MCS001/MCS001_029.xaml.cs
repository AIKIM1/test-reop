/*************************************************************************************
 Created Date : 2019.06.12
      Creator : 신광희
   Decription : STK 재공현황 LOT
--------------------------------------------------------------------------------------
 [Change History]
  2019.06.12  신광희 차장 : Initial Created.    
  2019.10.18  신광희 차장 : 입고LOT 조회 시 EQPTID 파라메터 값을 창고 적재 현황의 선택된 EQPTID 값으로 변경
  2020.04.01  정문교      : CSR[C20200325-000359] > 특별관리여부, 목적지 설비 칼럼 추가
  2020.05.27  김대근 사원 : CSR[C20200511-000332] > 노칭의 경우, 창고극성을 기준으로 조회 / 라미의 경우, 제품극성을 기준으로 조회
  2020.09.04  오화백      : 정보불일치 탭 리스트에 최종투입 설비 및 투입 시간 컬럼 추가
  2020.09.07  김동일      : C20200904-000407 > 버전 컬럼 추가요청에 의한 Collapsed 로직 제거.
  2020.12.29  안인효      : 자동 리프레쉬 기능 추가
  2021.02.02  오화백      : NND W/O별 현황에 Afrer NND Stocker, Lami Stocker  추가
  2021.08.10  오화백      : 노칭대기 창고 + 노칭대기 창고 Long  합침
  2021.09.06  오화백      : FAST TRACK 컬럼추가
  2022.03.24  서용호      : 슬라이딩 측정값 컬럼 추가
  2022.05.30  오화백      : 소계, 합계 일때 적재율 구하는 로직에 문제가 있어 수정 
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_029.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_029 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        //private readonly 
        private readonly Util _util = new Util();
        private string _selectedEquipmentCode;
        private string _selectedLotElectrodeTypeCode;
        private string _selectedStkElectrodeTypeCode;
        private string _selectedProjectName;
        private string _selectedWipHold;
        private string _selectedQmsHold;
        private string _selectedLotIdByRackInfo;
        private string _selectedSkIdIdByRackInfo;
        private string _selectedBobbinIdByRackInfo;
        private DataTable _dtWareHouseCapacity;

        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;
        private bool _isGradeJudgmentDisplay;
        private int _maxRowCount;
        private int _maxColumnCount;

        private DataTable _dtRackInfo;
        private UcRackLayout[][] _ucRackLayout1;
        private UcRackLayout[][] _ucRackLayout2;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        private enum SearchType
        {
            Tab,
            MultiSelectionBox
        }

        public MCS001_029()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _isGradeJudgmentDisplay = IsGradeJudgmentDisplay();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch};
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            
            InitializeGrid();
            InitializeCombo();

            MakeRackInfoTable();
            MakeWareHouseCapacityTable();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            C1TabControl.SelectionChanged += C1TabControl_SelectionChanged;
            _isLoaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            //cboStockerType.SelectedValueChanged -= cboStockerType_SelectedValueChanged;
            SetStockerTypeCombo(cboStockerType);
            //cboStockerType.SelectedValueChanged += cboStockerType_SelectedValueChanged;

            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;

            dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
            //dgProduct.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Collapsed;
            dgProductSummary.Columns["QMS_NG_QTY"].Visibility = Visibility.Collapsed;
            dgProductSummary.Columns["IQC_NG_QTY"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["A_COATING_NAME"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["E_COATING_NAME"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Collapsed;
            dgProduct.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;

            LeftAreaLami.Visibility = Visibility.Collapsed;
            LeftArea.Visibility = Visibility.Visible;
            tabStatusbyWorkorder.Visibility = Visibility.Collapsed;
            tabHoldList.Visibility = Visibility.Collapsed; //2021-05-19 OHB
            grdLotType.Visibility = Visibility.Collapsed;

            if (IsTabStatusbyWorkorderVisibility(SearchType.Tab, cboStockerType.SelectedValue.GetString()))

            {
                tabStatusbyWorkorder.Visibility = Visibility.Visible;
                if(LoginInfo.CFG_AREA_ID == "A7")
                { 
                     tabHoldList.Visibility = Visibility.Visible;  //2021-05-19 OHB
                }


            }

            if (IsTabStatusbyWorkorderVisibility(SearchType.MultiSelectionBox, cboStockerType.SelectedValue.GetString()))
                grdLotType.Visibility = Visibility.Visible;

            // 노칭대기창고
            if (cboStockerType.SelectedValue.GetString() == "NWW")
            {
                dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;

                if (LoginInfo.CFG_AREA_ID == "A7")
                {
                    dgProductSummary.Columns["IQC_NG_QTY"].Visibility = Visibility.Visible;
                    dgProduct.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Visible;
                }
            }
            else if (cboStockerType.SelectedValue.GetString() == "JRW")
            {
                dgProduct.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                dgProduct.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                dgProduct.Columns["E_COATING_NAME"].Visibility = Visibility.Visible;
            }
            else if (cboStockerType.SelectedValue.GetString() == "PCW")
            {
                dgProduct.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Visible;
                dgProduct.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Visible;
                dgProduct.Columns["E_COATING_NAME"].Visibility = Visibility.Visible;

                if (LoginInfo.CFG_AREA_ID == "ED")
                {
                    dgProductSummary.Columns["QMS_NG_QTY"].Visibility = Visibility.Visible;
                }
            }


            if (cboStockerType.SelectedValue.GetString() == "NPW" || cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgProduct.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;
                dgProduct.Columns["INPUT_LOTID"].Visibility = Visibility.Visible;

                if (cboStockerType.SelectedValue.GetString() == "LWW")
                {
                    LeftAreaLami.Visibility = Visibility.Visible;
                    LeftArea.Visibility = Visibility.Collapsed;

                    dgProduct.Columns["QMS_HOLD_FLAG"].Visibility = Visibility.Visible;
                    if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "A8" || LoginInfo.CFG_AREA_ID == "S4")
                        dgProduct.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                }
            }

            if (cboStockerType.SelectedValue.GetString() == "NWW" || cboStockerType.SelectedValue.GetString() == "MNW" || cboStockerType.SelectedValue.GetString() == "NPW" || cboStockerType.SelectedValue.GetString() == "LWW")
            {
                dgProduct.Columns["A_COATING_NAME"].Visibility = Visibility.Visible;
            }

        }

        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }

        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();
            if (cboStockerType.SelectedValue.GetString() == "LWW")
            {
                SelectWareHouseLamiSummary();
            }
            else if(cboStockerType.SelectedValue.GetString() == "NWW" && cboArea.SelectedValue.GetString() == "A7")
            {
                SelectWareHouseCapacitySummary_NWW();
            }
            else
            {
                SelectWareHouseCapacitySummary();
            }

            if (tabStatusbyWorkorder.Visibility == Visibility.Visible)
            {
                SelectStatusbyWorkorder();
            }

            if (tabHoldList.Visibility == Visibility.Visible)
            {
                SelectHold();
            }

           
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonVerify.HasDataGridRow(dgCapacitySummary)) return;

            SelectWareHouseProductSummary(true);
        }

        private void dgProductSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;


                if (string.Equals(e.Cell.Column.Name, "PRJT_NAME") 
                    || string.Equals(e.Cell.Column.Name, "ELTR_TYPE_NAME") 
                    || string.Equals(e.Cell.Column.Name, "LOT_QTY") 
                    || string.Equals(e.Cell.Column.Name, "LOT_HOLD_QTY")
                    || string.Equals(e.Cell.Column.Name, "LOT_HOLD_QMS_QTY"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    if (string.Equals(e.Cell.Column.Name, "WIP_HOLD_RATE"))
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_HOLD_RATE").GetDecimal() >= 20)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        private void dgProductSummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgProductSummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null 
                    || cell.Column.Name.Equals("WIP_HOLD_RATE") 
                    || cell.Column.Name.Equals("WIP_QTY")
                    || cell.Column.Name.Equals("WIP_HOLD_QTY")
                    || cell.Column.Name.Equals("WIP_HOLD_QMS_QTY")
                    || cell.Column.Name.Equals("IQC_NG_QTY")
                    || cell.Column.Name.Equals("QMS_NG_QTY"))
                {
                    return;
                }


                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                _selectedLotElectrodeTypeCode = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString()) ? null : DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                _selectedStkElectrodeTypeCode = null;
                _selectedQmsHold = null;

                if (cell.Column.Name.Equals("LOT_QTY") || cell.Column.Name.Equals("WIP_QTY"))
                {
                    _selectedWipHold = "N";
                    _selectedQmsHold = "N";
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QTY") || cell.Column.Name.Equals("WIP_HOLD_QTY"))
                {
                    _selectedWipHold = "Y";
                    _selectedQmsHold = null;
                }
                else if (cell.Column.Name.Equals("LOT_HOLD_QMS_QTY") || cell.Column.Name.Equals("WIP_HOLD_QMS_QTY"))
                {
                    _selectedQmsHold = "Y";
                    _selectedWipHold = "N";
                }
                else if(cell.Column.Name.Equals("PRJT_NAME"))
                {
                    _selectedLotElectrodeTypeCode = null;
                    _selectedWipHold = null;
                }

                tabProduct.IsSelected = true;
                SelectWareHouseProductList(dgProduct);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "EQPTNAME") || string.Equals(e.Cell.Column.Name, "BBN_E_QTY") || string.Equals(e.Cell.Column.Name, "ERROR_QTY") || string.Equals(e.Cell.Column.Name, "ABNORM_QTY"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ELTR_TYPE_NAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")), "XXXXXXXXXX") && !string.Equals(e.Cell.Column.Name, "ELTR_TYPE_NAME"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Aqua");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedLotElectrodeTypeCode = null;

                _selectedLotIdByRackInfo = null;
                _selectedSkIdIdByRackInfo = null;
                _selectedBobbinIdByRackInfo = null;

                if (cell.Column.Name.Equals("EQPTNAME") || (cell.Column.Name.Equals("ELTR_TYPE_NAME") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                {
                    txtStocker.Text = string.Empty;
                    txtRealCarrierCount.Text = string.Empty;

                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }
                    

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ") )
                    {
                        _selectedStkElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedStkElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    txtStocker.Text = DataTableConverter.GetValue(drv, "EQPTNAME").GetString();
                    txtRealCarrierCount.Text = DataTableConverter.GetValue(drv, "BBN_U_QTY").GetString();

                    Util.gridClear(dgProduct);

                    if (cell.Column.Name.Equals("EQPTNAME") || (cell.Column.Name.Equals("ELTR_TYPE_NAME") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        _selectedWipHold = null;
                        _selectedProjectName = null;

                        if (tabLayout.IsSelected && (cell.Column.Name.Equals("EQPTNAME") || cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))) )
                        {

                            ShowLoadingIndicator();
                            DoEvents();

                            SelectMaxxyz();
                            ReSetLayoutUserControl();

                            if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                            {
                                SelectRackInfo();
                            }
                            else
                            {
                                HiddenLoadingIndicator();
                            }
                           
                            
                        }
                        else
                        {
                            SelectWareHouseProductList((table, ex) =>
                            {
                                tabProduct.IsSelected = true;
                                SelectWareHouseProductSummary(false);

                                Util.GridSetData(dgProduct, table, null, true);
                                HiddenLoadingIndicator();
                            });
                        }

                    }
                    else
                    {
                        tabProduct.IsSelected = true;
                        SelectWareHouseProductSummary(false);
                    }

                }
                else if (cell.Column.Name.Equals("BBN_E_QTY"))
                {
                    //_selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    txtStocker.Text = DataTableConverter.GetValue(drv, "EQPTNAME").GetString();
                    txtRealCarrierCount.Text = DataTableConverter.GetValue(drv, "BBN_E_QTY").GetString();

                    tabEmptyCarrier.IsSelected = true;
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList(dgCarrierList);
                }
                else if (cell.Column.Name.Equals("ERROR_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString(), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    txtStocker.Text = DataTableConverter.GetValue(drv, "EQPTNAME").GetString();
                    txtRealCarrierCount.Text = DataTableConverter.GetValue(drv, "ERROR_QTY").GetString();
                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList(dgErrorCarrier);
                }
                else if (cell.Column.Name.Equals("ABNORM_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NAME").GetString(), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    txtStocker.Text = DataTableConverter.GetValue(drv, "EQPTNAME").GetString();
                    txtRealCarrierCount.Text = DataTableConverter.GetValue(drv, "ABNORM_QTY").GetString();
                    tabAbNormalCarrier.IsSelected = true;
                    SelectAbNormalCarrierList(dgAbNormalCarrier);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCapacitySummary_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE")
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        Count = g.Count()
                    }).ToList();

                    string previewElectrodeTypeCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_NAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                        {
                            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index + 1)));
                        }
                        else
                        {
                            foreach (var item in query)
                            {
                                int rowIndex = i;
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString() == item.ElectrodeTypeCode && previewElectrodeTypeCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ELTR_TYPE_NAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ELTR_TYPE_NAME"].Index)));
                                }
                            }
                        }
                        previewElectrodeTypeCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLamiCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgLamiCapacitySummary_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgLamiCapacitySummary_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                // 선택한 셀의 위치
                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedStkElectrodeTypeCode = null;
                _selectedLotElectrodeTypeCode = null;
                _selectedEquipmentCode = null;
                _selectedProjectName = null;
                _selectedWipHold = null;
                _selectedQmsHold = null;

                _selectedLotIdByRackInfo = null;
                _selectedSkIdIdByRackInfo = null;
                _selectedBobbinIdByRackInfo = null;

                if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                    string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                {
                    _selectedEquipmentCode = null;
                    _selectedProjectName = null;
                }
                else
                {
                    _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                }

                if (cell.Column.Name.Equals("EMPTY_QTY"))
                {
                    tabEmptyCarrier.IsSelected = true;
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList(dgCarrierList);
                }
                else if (cell.Column.Name.Equals("ERROR_QTY"))
                {
                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList(dgErrorCarrier);
                }
                else if (cell.Column.Name.Equals("ABNORM_QTY"))
                {
                    tabAbNormalCarrier.IsSelected = true;
                    SelectAbNormalCarrierList(dgAbNormalCarrier);
                }
                else
                {
                    if (cell.Column.Name.Equals("EQPTNAME") || cell.Column.Name.Equals("RACK_MAX_QTY") || cell.Column.Name.Equals("RACK_RATE"))
                    {
                        _selectedProjectName = null;
                    }

                    else if (cell.Column.Name.Equals("LOT_QTY_C"))
                    {
                        _selectedLotElectrodeTypeCode = "C";
                        _selectedWipHold = "N";
                    }
                    else if (cell.Column.Name.Equals("HOLD_QTY_C"))
                    {
                        _selectedLotElectrodeTypeCode = "C";
                        _selectedWipHold = "Y";
                    }
                    else if (cell.Column.Name.Equals("LOT_QTY_A"))
                    {
                        _selectedLotElectrodeTypeCode = "A";
                        _selectedWipHold = "N";
                    }
                    else if (cell.Column.Name.Equals("HOLD_QTY_A"))
                    {
                        _selectedLotElectrodeTypeCode = "A";
                        _selectedWipHold = "Y";
                    }

                    if (tabLayout.IsSelected && (cell.Column.Name.Equals("EQPTNAME") || cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        ShowLoadingIndicator();
                        DoEvents();

                        SelectMaxxyz();
                        ReSetLayoutUserControl();

                        if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                        {
                            SelectRackInfo();
                        }
                        else
                        {
                            HiddenLoadingIndicator();
                        }
                            
                    }
                    else
                    {
                        Util.gridClear(dgProduct);
                        tabProduct.IsSelected = true;
                        SelectWareHouseProductList(dgProduct);
                    }
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void dgLamiCapacitySummary_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg?.ItemsSource == null) return;

            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = ((DataView)dg.ItemsSource).Table;
                    var query = dt.AsEnumerable().GroupBy(x => new
                    {
                        equipmentCode = x.Field<string>("EQPTID")
                    }).Select(g => new
                    {
                        EquipmentCode = g.Key.equipmentCode,
                        Count = g.Count()
                    }).ToList();

                    string previewEquipmentCode = string.Empty;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTNAME").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                        {
                            //e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPTNAME"].Index), dg.GetCell(i, dg.Columns["EQPTNAME"].Index + 1)));
                        }
                        else
                        {
                            foreach (var item in query)
                            {
                                int rowIndex = i;
                                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString() == item.EquipmentCode && previewEquipmentCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString())
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPTNAME"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EQPTNAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_MAX_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_MAX_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EMPTY_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EMPTY_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ERROR_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ERROR_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["ABNORM_QTY"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["ABNORM_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_RATE"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_RATE"].Index)));
                                }
                            }
                        }
                        previewEquipmentCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "EQPTID").GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("LOTID"))
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID"));
                    this.FrameOperation.OpenMenu("SFU010160050", true, parameters);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Convert.ToString(e.Cell.Column.Name) == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 30)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 15)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                        else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PAST_DAY").GetInt() >= 7)
                        {
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Black");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                    
                    if (_isscrollToHorizontalOffset)
                    {
                        dataGrid.Viewport.ScrollToHorizontalOffset(_scrollToHorizontalOffset);
                    }
                }
                else
                {

                }
            }));
        }

        private void dgProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CommonVerify.HasDataGridRow(dgProduct))
                {
                    _isscrollToHorizontalOffset = true;
                    _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
                }

                string tabItem = ((C1TabItem)((ItemsControl)sender).Items.CurrentItem).Name.GetString();

                if (string.Equals(tabItem, "tabLayout"))
                {
                    if (!string.IsNullOrEmpty(_selectedEquipmentCode))
                    {
                        ShowLoadingIndicator();
                        DoEvents();

                        SelectMaxxyz();
                        ReSetLayoutUserControl();
                        SelectRackInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgProduct_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isscrollToHorizontalOffset = false;
        }

        private void UcRackLayout1_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            if (rackLayout.IsChecked)
            {
                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout1[row][col];

                        if (!Equals(rackLayout, ucRackLayout))
                        {
                            ucRackLayout.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (ucRackLayout.IsChecked)
                            ucRackLayout.IsChecked = false;
                    }
                }

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
                }

                _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
                _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
                _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;

                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackLayout.RackId;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["PRJT_NAME"] = rackLayout.ProjectName;
                dr["LOTID"] = rackLayout.LotId;
                dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
                dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["WIPHOLD"] = rackLayout.WipHold;
                dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
                dr["SKID_GUBUN"] = rackLayout.SkidType;
                dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
                dr["COLOR"] = rackLayout.LegendColor;
                dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
                dr["HOLD_FLAG"] = rackLayout.HoldFlag;
                dr["SEQ"] = maxSeq;
                _dtRackInfo.Rows.Add(dr);

                if (rackLayout.LegendColorType == "4")
                {
                    _selectedSkIdIdByRackInfo = null;
                }
                else if (rackLayout.LegendColorType == "5")
                {
                    _selectedBobbinIdByRackInfo = null;
                }

                GetLayoutGridColumns(rackLayout.LegendColorType);
                SelectLayoutGrid(rackLayout.LegendColorType);
            }
            else
            {

                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }
        }

        private void UcRackLayout2_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            if (rackLayout.IsChecked)
            {
                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (!Equals(rackLayout, ucRackLayout))
                        {
                            ucRackLayout.IsChecked = false;
                        }
                    }
                }

                for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackStair = _ucRackLayout1[row][col];

                        if (ucRackStair.IsChecked)
                            ucRackStair.IsChecked = false;
                    }
                }

                if (CommonVerify.HasTableRow(_dtRackInfo))
                {
                    for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                    {
                        _dtRackInfo.Rows.RemoveAt(i);
                    }
                }

                _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
                _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
                _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;

                int maxSeq = 1;
                DataRow dr = _dtRackInfo.NewRow();
                dr["RACK_ID"] = rackLayout.RackId;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["PRJT_NAME"] = rackLayout.ProjectName;
                dr["LOTID"] = rackLayout.LotId;
                dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
                dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["WIPHOLD"] = rackLayout.WipHold;
                dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
                dr["SKID_GUBUN"] = rackLayout.SkidType;
                dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
                dr["COLOR"] = rackLayout.LegendColor;
                dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
                dr["HOLD_FLAG"] = rackLayout.HoldFlag;
                dr["SEQ"] = maxSeq;
                _dtRackInfo.Rows.Add(dr);
                GetLayoutGridColumns(rackLayout.LegendColorType);
                SelectLayoutGrid(rackLayout.LegendColorType);
            }
            else
            {

                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (sender == null) return;

            if (string.IsNullOrEmpty(textBox.Text.Trim())) return;

            if (textBox.Name == "txtLotId")
            {
                txtBobbinId.Text = string.Empty;
            }
            else
            {
                txtLotId.Text = string.Empty;
            }


            if (e.Key == Key.Enter)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();

                UnCheckedAllRackLayout();

                for (int r = 0; r < grdRackstair1.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair1.ColumnDefinitions.Count; c++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout1[r][c];

                        doubleAnimation.From = ucRackLayout.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300)); // 300ms == 0.3s
                        doubleAnimation.AutoReverse = true;

                        string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.BobbinCarrierCode;

                        if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            SetScrollToHorizontalOffset(scrollViewer1, c);
                            ucRackLayout.IsChecked = true;
                            CheckUcRackLayout(ucRackLayout);
                            ucRackLayout.BeginAnimation(HeightProperty, doubleAnimation);

                            return;
                        }
                    }
                }

                for (int r = 0; r < grdRackstair2.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair2.ColumnDefinitions.Count; c++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[r][c];

                        doubleAnimation.From = ucRackLayout.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        doubleAnimation.AutoReverse = true;

                        string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.BobbinCarrierCode;

                        if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            SetScrollToHorizontalOffset(scrollViewer2, c);
                            ucRackLayout.IsChecked = true;
                            CheckUcRackLayout(ucRackLayout);
                            ucRackLayout.BeginAnimation(HeightProperty, doubleAnimation);

                            return;
                        }
                    }
                }
            }
        }

        private void dgStatusbyWorkorder_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "EQSGNAME")|| string.Equals(e.Cell.Column.Name, "PRJT_NAME"))
                {
                    return;
                }
                else if(string.Equals(e.Cell.Column.Name, "FOL_EQSG_MODEL_QTY") || string.Equals(e.Cell.Column.Name, "FOL_MODEL_QTY")) // PKG Input Try
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1000)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 1000 && DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 2000)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
                else if (string.Equals(e.Cell.Column.Name, "AN_QTY_AF_NT_NPW") || string.Equals(e.Cell.Column.Name, "CA_QTY_AF_NT_NPW")) //After NND Stocker 
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() >= 5)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                }
                else
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() > 0 && DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 6)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgStatusbyWorkorder_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgStatusbyWorkorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || cell.Column.Name.Equals("EQSGNAME") || cell.Column.Name.Equals("PRJT_NAME") || cell.Column.Name.Equals("EQSGID")) return;
                if (cell.Text.Replace(",","").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_029_LOTLIST popupLotlist = new MCS001_029_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[4];
                parameters[0] = LoginInfo.CFG_AREA_ID;
                parameters[1] = DataTableConverter.GetValue(drv, "EQSGID").GetString();
                parameters[2] = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                parameters[3] = cell.Column.Name;
                C1WindowExtension.SetParameters(popupLotlist, parameters);

                popupLotlist.Closed += popupLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popupLotlist_Closed(object sender, EventArgs e)
        {
            MCS001_029_LOTLIST popup = sender as MCS001_029_LOTLIST;
            if (popup != null)
            {
                
            }
        }


        private void Splitter_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            GridSplitter splitter = (GridSplitter)sender;

            try
            {
                C1DataGrid dataGrid = LeftArea.Visibility == Visibility.Visible ? dgCapacitySummary : dgLamiCapacitySummary;
                double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);

                if (ContentsRow.ColumnDefinitions[0].Width.Value > sumWidth)
                {
                    ContentsRow.ColumnDefinitions[0].Width = new GridLength(sumWidth + splitter.ActualWidth);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 2021-05-19 Hold 재고 현황  by 오화백

        private void dgHold_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "EQSGNAME") || string.Equals(e.Cell.Column.Name, "PRJT_NAME"))
                {
                    return;
                }

                else
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() > 4)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgHold_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgHold_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null || cell.Column.Name.Equals("EQSGNAME") || cell.Column.Name.Equals("PRJT_NAME") || cell.Column.Name.Equals("EQSGID")) return;
                if (cell.Text.Replace(",", "").GetDouble() < 1) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 팝업 호출
                MCS001_029_HOLD_LOTLIST popupHoldLotlist = new MCS001_029_HOLD_LOTLIST { FrameOperation = FrameOperation };
                object[] parameters = new object[4];
                parameters[0] = LoginInfo.CFG_AREA_ID;
                parameters[1] = DataTableConverter.GetValue(drv, "EQSGID").GetString();
                parameters[2] = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
                parameters[3] = cell.Column.Name;
                C1WindowExtension.SetParameters(popupHoldLotlist, parameters);

                popupHoldLotlist.Closed += popupHoldLotlist_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupHoldLotlist.ShowModal()));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void popupHoldLotlist_Closed(object sender, EventArgs e)
        {
            MCS001_029_HOLD_LOTLIST popup = sender as MCS001_029_HOLD_LOTLIST;
            if (popup != null)
            {

            }
        }

        #endregion

        #endregion

        #region Method

        private void SelectWareHouseCapacitySummary()
        {
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_CAPACITY_SUMMARY";
            try
            {
                DataTable dtResult = new DataTable();

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    // 극성별 소계
                    var query = bizResult.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE"),
                        electrodeTypeName = x.Field<string>("ELTR_TYPE_NAME"),
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        ElectrodeTypeName = g.Key.electrodeTypeName,
                        RackMax = g.Sum(x => x.Field<decimal>("RACK_MAX")),
                        RealCarrierCount = g.Sum(x => x.Field<decimal>("BBN_U_QTY")),
                        EmptyCarrierCount = g.Sum(x => x.Field<decimal>("BBN_E_QTY")),
                        OppositeCarrierCount = g.Sum(x => x.Field<decimal>("BBN_UM_QTY")),
                        ErrorCarrierCount = g.Sum(x => x.Field<decimal>("ERROR_QTY")),
                        AbnormalCount = g.Sum(x => x.Field<decimal>("ABNORM_QTY")),
                        SumCarrierCount = g.Sum(x => x.Field<decimal>("RACK_QTY")),
                        RackRate = GetRackRate(g.Sum(x => x.Field<decimal>("RACK_QTY")), g.Sum(x => x.Field<decimal>("RACK_MAX"))),
                        EquipmentCode = "XXXXXXXXXX",
                        EquipmentName = g.Key.electrodeTypeName + "  " + ObjectDic.Instance.GetObjectName("소계"),
                        Count = g.Count() 
                    }).ToList();

                    // 합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new
                    {}).Select(g => new
                    {
                        ElectrodeTypeCode = "ZZZZZZZZZZ",
                        ElectrodeTypeName = ObjectDic.Instance.GetObjectName("합계"),
                        RackMax = g.Sum(x => x.Field<decimal>("RACK_MAX")),
                        RealCarrierCount = g.Sum(x => x.Field<decimal>("BBN_U_QTY")),
                        EmptyCarrierCount = g.Sum(x => x.Field<decimal>("BBN_E_QTY")),
                        OppositeCarrierCount = g.Sum(x => x.Field<decimal>("BBN_UM_QTY")),
                        ErrorCarrierCount = g.Sum(x => x.Field<decimal>("ERROR_QTY")),
                        AbnormalCount = g.Sum(x => x.Field<decimal>("ABNORM_QTY")),
                        SumCarrierCount = g.Sum(x => x.Field<decimal>("RACK_QTY")),
                        RackRate = GetRackRate(g.Sum(x => x.Field<decimal>("RACK_QTY")), g.Sum(x => x.Field<decimal>("RACK_MAX"))),
                        EquipmentCode = "ZZZZZZZZZZ",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        Count = g.Count()
                    }).ToList();


                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            DataRow newRow = _dtWareHouseCapacity.NewRow();
                            newRow["ELTR_TYPE_CODE"] = bizResult.Rows[i]["ELTR_TYPE_CODE"];
                            newRow["ELTR_TYPE_NAME"] = bizResult.Rows[i]["ELTR_TYPE_NAME"];
                            newRow["EQPTID"] = bizResult.Rows[i]["EQPTID"];
                            newRow["EQPTNAME"] = bizResult.Rows[i]["EQPTNAME"];
                            newRow["RACK_MAX"] = bizResult.Rows[i]["RACK_MAX"];
                            newRow["BBN_U_QTY"] = bizResult.Rows[i]["BBN_U_QTY"];
                            newRow["BBN_E_QTY"] = bizResult.Rows[i]["BBN_E_QTY"];
                            newRow["BBN_UM_QTY"] = bizResult.Rows[i]["BBN_UM_QTY"];
                            newRow["ERROR_QTY"] = bizResult.Rows[i]["ERROR_QTY"];
                            newRow["ABNORM_QTY"] = bizResult.Rows[i]["ABNORM_QTY"];
                            newRow["RACK_RATE"] = bizResult.Rows[i]["RACK_RATE"];
                            newRow["RACK_QTY"] = bizResult.Rows[i]["RACK_QTY"];
                            _dtWareHouseCapacity.Rows.Add(newRow);
                        }

                        if (query.Any())
                        {
                            foreach (var item in query)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["RACK_MAX"] = item.RackMax;
                                newRow["BBN_U_QTY"] = item.RealCarrierCount;
                                newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                                newRow["BBN_UM_QTY"] = item.OppositeCarrierCount;
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }

                        if (querySum.Any())
                        {
                            foreach (var item in querySum)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["RACK_MAX"] = item.RackMax;
                                newRow["BBN_U_QTY"] = item.RealCarrierCount;
                                newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                                newRow["BBN_UM_QTY"] = item.OppositeCarrierCount;
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }
                    }

                    if (CommonVerify.HasTableRow(_dtWareHouseCapacity))
                    {
                        dtResult = (from t in _dtWareHouseCapacity.AsEnumerable()
                            orderby t.Field<string>("ELTR_TYPE_CODE") ascending, t.Field<string>("EQPTID")
                            select t).CopyToDataTable();
                    }
                    else
                    {
                        dtResult = bizResult;
                    }

                    Util.GridSetData(dgCapacitySummary, dtResult, null, true);
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseCapacitySummary_NWW()
        {
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_CAPACITY_SUMMARY_NWW";
            try
            {
                DataTable dtResult = new DataTable();

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    // 극성별 소계
                    var query = bizResult.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE"),
                        electrodeTypeName = x.Field<string>("ELTR_TYPE_NAME"),
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        ElectrodeTypeName = g.Key.electrodeTypeName,
                        RackMax = g.Sum(x => x.Field<decimal>("RACK_MAX")),
                        RealCarrierCount = g.Sum(x => x.Field<decimal>("BBN_U_QTY")),
                        EmptyCarrierCount = g.Sum(x => x.Field<decimal>("BBN_E_QTY")),
                        OppositeCarrierCount = g.Sum(x => x.Field<decimal>("BBN_UM_QTY")),
                        ErrorCarrierCount = g.Sum(x => x.Field<decimal>("ERROR_QTY")),
                        AbnormalCount = g.Sum(x => x.Field<decimal>("ABNORM_QTY")),
                        SumCarrierCount = g.Sum(x => x.Field<decimal>("RACK_QTY")),
                        RackRate = GetRackRate(g.Sum(x => x.Field<decimal>("RACK_QTY")), g.Sum(x => x.Field<decimal>("RACK_MAX"))),
                        EquipmentCode = "XXXXXXXXXX",
                        EquipmentName = g.Key.electrodeTypeName + "  " + ObjectDic.Instance.GetObjectName("소계"),
                        Count = g.Count()
                    }).ToList();

                    // 합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new
                    { }).Select(g => new
                    {
                        ElectrodeTypeCode = "ZZZZZZZZZZ",
                        ElectrodeTypeName = ObjectDic.Instance.GetObjectName("합계"),
                        RackMax = g.Sum(x => x.Field<decimal>("RACK_MAX")),
                        RealCarrierCount = g.Sum(x => x.Field<decimal>("BBN_U_QTY")),
                        EmptyCarrierCount = g.Sum(x => x.Field<decimal>("BBN_E_QTY")),
                        OppositeCarrierCount = g.Sum(x => x.Field<decimal>("BBN_UM_QTY")),
                        ErrorCarrierCount = g.Sum(x => x.Field<decimal>("ERROR_QTY")),
                        AbnormalCount = g.Sum(x => x.Field<decimal>("ABNORM_QTY")),
                        SumCarrierCount = g.Sum(x => x.Field<decimal>("RACK_QTY")),
                        RackRate = GetRackRate(g.Sum(x => x.Field<decimal>("RACK_QTY")), g.Sum(x => x.Field<decimal>("RACK_MAX"))),
                        EquipmentCode = "ZZZZZZZZZZ",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        Count = g.Count()
                    }).ToList();


                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            DataRow newRow = _dtWareHouseCapacity.NewRow();
                            newRow["ELTR_TYPE_CODE"] = bizResult.Rows[i]["ELTR_TYPE_CODE"];
                            newRow["ELTR_TYPE_NAME"] = bizResult.Rows[i]["ELTR_TYPE_NAME"];
                            newRow["EQPTID"] = bizResult.Rows[i]["EQPTID"];
                            newRow["EQPTNAME"] = bizResult.Rows[i]["EQPTNAME"];
                            newRow["RACK_MAX"] = bizResult.Rows[i]["RACK_MAX"];
                            newRow["BBN_U_QTY"] = bizResult.Rows[i]["BBN_U_QTY"];
                            newRow["BBN_E_QTY"] = bizResult.Rows[i]["BBN_E_QTY"];
                            newRow["BBN_UM_QTY"] = bizResult.Rows[i]["BBN_UM_QTY"];
                            newRow["ERROR_QTY"] = bizResult.Rows[i]["ERROR_QTY"];
                            newRow["ABNORM_QTY"] = bizResult.Rows[i]["ABNORM_QTY"];
                            newRow["RACK_RATE"] = bizResult.Rows[i]["RACK_RATE"];
                            newRow["RACK_QTY"] = bizResult.Rows[i]["RACK_QTY"];
                            _dtWareHouseCapacity.Rows.Add(newRow);
                        }

                        if (query.Any())
                        {
                            foreach (var item in query)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["RACK_MAX"] = item.RackMax;
                                newRow["BBN_U_QTY"] = item.RealCarrierCount;
                                newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                                newRow["BBN_UM_QTY"] = item.OppositeCarrierCount;
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }

                        if (querySum.Any())
                        {
                            foreach (var item in querySum)
                            {
                                DataRow newRow = _dtWareHouseCapacity.NewRow();
                                newRow["ELTR_TYPE_CODE"] = item.ElectrodeTypeCode;
                                newRow["ELTR_TYPE_NAME"] = item.ElectrodeTypeName;
                                newRow["EQPTID"] = item.EquipmentCode;
                                newRow["EQPTNAME"] = item.EquipmentName;
                                newRow["RACK_MAX"] = item.RackMax;
                                newRow["BBN_U_QTY"] = item.RealCarrierCount;
                                newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                                newRow["BBN_UM_QTY"] = item.OppositeCarrierCount;
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["RACK_RATE"] = item.RackRate;
                                newRow["RACK_QTY"] = item.SumCarrierCount;
                                _dtWareHouseCapacity.Rows.Add(newRow);
                            }
                        }
                    }

                    if (CommonVerify.HasTableRow(_dtWareHouseCapacity))
                    {
                        dtResult = (from t in _dtWareHouseCapacity.AsEnumerable()
                                    orderby t.Field<string>("ELTR_TYPE_CODE") ascending, t.Field<string>("EQPTID")
                                    select t).CopyToDataTable();
                    }
                    else
                    {
                        dtResult = bizResult;
                    }

                    Util.GridSetData(dgCapacitySummary, dtResult, null, true);
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        private void SelectWareHouseProductSummary(bool isRefresh = false)
        {
            if (isRefresh == false)
            {
                if (grdLotType.Visibility == Visibility.Visible)
                    SelectLotTypeMultiSelectionBox(msbLotType);
            }

            string lotType;
            if (grdLotType.Visibility == Visibility.Visible)
            {
                lotType = string.IsNullOrEmpty(msbLotType.SelectedItemsToString) ? null : msbLotType.SelectedItemsToString;
            }
            else
            {
                lotType = null;
            }

            string bizRuleName = string.Empty;

            if (string.IsNullOrEmpty(_selectedEquipmentCode)&& cboStockerType.SelectedValue.ToString()== "NWW")
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_SUMMARY_NWW";
            }
            else
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_SUMMARY";
            }


            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_SUMMARY";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                {
                    dr["EQGRID"] = "MNW";
                }
                else
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
                dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["LOTTYPE"] = lotType;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgProductSummary, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectLotTypeMultiSelectionBox(MultiSelectionBox msb)
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW" && cboArea.SelectedValue.ToString() == "A7")
                {
                    bizRuleName = "BR_MCS_SEL_WAREHOUSE_CAPA_SUMMARY_LOTTYPE_CBO_NWW";
                }
                else
                {
                    bizRuleName = "BR_MCS_SEL_WAREHOUSE_CAPA_SUMMARY_LOTTYPE_CBO";
                }

                //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_CAPA_SUMMARY_LOTTYPE_CBO";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                {
                    dr["EQGRID"] = "MNW";
                }
                else
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
                dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                msb.ItemsSource = DataTableConverter.Convert(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    msb.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseProductList(C1DataGrid dg)
        {
            string bizRuleName = string.Empty;

            if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW" && cboArea.SelectedValue.ToString() == "A7")
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST_NWW";
            }
            else
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
            }

            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("LOT_ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("STK_ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("QMSHOLD", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();

                if (dg.Name == "dgRackInfo")
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue;
                    if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                    {
                        dr["EQGRID"] = "MNW";
                    }
                    else
                    {
                        dr["EQGRID"] = cboStockerType.SelectedValue;
                    }
                    dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTID"] = _selectedLotIdByRackInfo;
                    dr["EQPTID"] = _selectedEquipmentCode;
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue;
                    if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                    {
                        dr["EQGRID"] = "MNW";
                    }
                    else
                    {
                        dr["EQGRID"] = cboStockerType.SelectedValue;
                    }
                    dr["LOT_ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                    dr["STK_ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                    dr["EQPTID"] = _selectedEquipmentCode;
                    dr["PRJT_NAME"] = _selectedProjectName;
                    dr["WIPHOLD"] = _selectedWipHold;
                    dr["QMSHOLD"] = _selectedQmsHold;
                    dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTID"] = _selectedLotIdByRackInfo;
                }
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseProductList(Action<DataTable, Exception> actionCompleted = null)
        {

            string bizRuleName = string.Empty;

            if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW" && cboArea.SelectedValue.ToString() == "A7")
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST_NWW";
            }
            else
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
            }
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("LOT_ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("STK_ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if(_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                {
                    dr["EQGRID"] = "MNW";
                }
                else
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
               
                dr["LOT_ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["STK_ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["PRJT_NAME"] = _selectedProjectName;
                dr["WIPHOLD"] = _selectedWipHold;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    actionCompleted?.Invoke(bizResult, bizException);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseEmptyCarrier()
        {
            string bizRuleName = string.Empty;

            if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW" && cboArea.SelectedValue.ToString() == "A7")
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_NWW";
            }
            else
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER";
            }
                //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                {
                    dr["EQGRID"] = "MNW";
                }
                else
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgEmptyCarrier, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseEmptyCarrierList(C1DataGrid dg)
        {
            string bizRuleName = string.Empty;

            if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW" && cboArea.SelectedValue.ToString() == "A7")
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_LIST_NWW";
            }
            else
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_LIST";
            }
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("SKID_ID", typeof(string));
                inTable.Columns.Add("BOBBIN_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                {
                    dr["EQGRID"] = "MNW";
                }
                else
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["SKID_ID"] = _selectedSkIdIdByRackInfo;
                dr["BOBBIN_ID"] = _selectedBobbinIdByRackInfo;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectErrorCarrierList(C1DataGrid dg)
        {

            string bizRuleName = string.Empty;

            if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW" && cboArea.SelectedValue.ToString() == "A7")
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_NOREAD_LIST_NWW";
            }
            else
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_NOREAD_LIST";
            }
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_NOREAD_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("SKID_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                {
                    dr["EQGRID"] = "MNW";
                }
                else
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
                //dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["SKID_ID"] = _selectedSkIdIdByRackInfo;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectAbNormalCarrierList(C1DataGrid dg)
        {
            string bizRuleName = string.Empty;

            if (string.IsNullOrEmpty(_selectedEquipmentCode) && cboStockerType.SelectedValue.ToString() == "NWW"&& cboArea.SelectedValue.ToString() == "A7")
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_ABNORM_LIST_NWW";
            }
            else
            {
                bizRuleName = "BR_MCS_SEL_WAREHOUSE_ABNORM_LIST";
            }

            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_ABNORM_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("SKID_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (_selectedEquipmentCode == "A7STK108" || _selectedEquipmentCode == "A7STK104")
                {
                    dr["EQGRID"] = "MNW";
                }
                else
                {
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                }
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["SKID_ID"] = _selectedSkIdIdByRackInfo;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseLamiSummary()
        {
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SUMMARY_LWW";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    // 라미대기 창고 인 경우 합계를 구하기 위해서 용량, 공Carrier, 오류Carrier, 적재율을 Distinct 처리 해야 함.
                    var queryBase = bizResult.AsEnumerable()
                        .Select(row => new {
                            EquipmentCode = row.Field<string>("EQPTID"),
                            EquipmentName = row.Field<string>("EQPTNAME"),
                            RackMaxQty = row.Field<decimal>("RACK_MAX_QTY"),
                            EmptyQty = row.Field<decimal>("EMPTY_QTY"),
                            ErrorQty = row.Field<decimal>("ERROR_QTY"),
                            RackRate = row.Field<decimal>("RACK_RATE"),
                            SumCarrierCount = row.Field<decimal>("RACK_QTY"),
                            AbNormalQty = row.Field<decimal>("ABNORM_QTY")
                        }).Distinct();

                    //합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new { }).Select(g => new
                    {
                        EquipmentCode = "ZZZZZZZZZZ",
                        EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                        ProjectName = "",
                        LotCountCathode = g.Sum(x => x.Field<decimal>("LOT_QTY_C")),
                        HoldCountCathode = g.Sum(x => x.Field<decimal>("HOLD_QTY_C")),
                        LotCountAnode = g.Sum(x => x.Field<decimal>("LOT_QTY_A")),
                        HoldCountAnode = g.Sum(x => x.Field<decimal>("HOLD_QTY_A")),
                        EmptyCount = queryBase.AsQueryable().Select(s => s.EmptyQty).Sum(),
                        ErrorCount = queryBase.AsQueryable().Select(s => s.ErrorQty).Sum(),
                        RackMaxCount = queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum(),
                        SumCarrierCount = g.Sum(x => x.Field<decimal>("RACK_QTY")),
                        //AbNormalQty = g.Sum(x => x.Field<decimal>("ABNORM_QTY")),
                        AbNormalQty = queryBase.AsQueryable().Select(s => s.AbNormalQty).Sum(),
                        RackRate = GetRackRate(queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum(), queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum()),
                        Count = g.Count()
                    }).FirstOrDefault();

                    if (querySum != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = querySum.EquipmentCode;
                        newRow["EQPTNAME"] = querySum.EquipmentName;
                        newRow["PRJT_NAME"] = querySum.ProjectName;
                        newRow["RACK_MAX_QTY"] = querySum.RackMaxCount;
                        newRow["LOT_QTY_C"] = querySum.LotCountCathode;
                        newRow["HOLD_QTY_C"] = querySum.HoldCountCathode;
                        newRow["LOT_QTY_A"] = querySum.LotCountAnode;
                        newRow["HOLD_QTY_A"] = querySum.HoldCountAnode;
                        newRow["EMPTY_QTY"] = querySum.EmptyCount;
                        newRow["ERROR_QTY"] = querySum.ErrorCount;
                        newRow["ABNORM_QTY"] = querySum.AbNormalQty;
                        newRow["RACK_RATE"] = querySum.RackRate;
                        bizResult.Rows.Add(newRow);
                    }

                    Util.GridSetData(dgLamiCapacitySummary, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectRackInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_RACK_LAYOUT";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(dr);

                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                HideAndClearAllRack();
                Util.gridClear(dgRackInfo);

                if (CommonVerify.HasTableRow(bizResult))
                {
                    foreach (DataRow item in bizResult.Rows)
                    {
                        int x = GetXPosition(item["Z_PSTN"].ToString());
                        int y = int.Parse(item["Y_PSTN"].ToString()) - 1;

                        UcRackLayout ucRackLayout = item["X_PSTN"].ToString() == "1" ? _ucRackLayout1[x][y] : _ucRackLayout2[x][y];

                        if (ucRackLayout == null) continue;

                        ucRackLayout.RackId = item["RACK_ID"].GetString();
                        ucRackLayout.Row = int.Parse(item["Z_PSTN"].GetString());
                        ucRackLayout.Col = int.Parse(item["Y_PSTN"].GetString());
                        ucRackLayout.Stair = int.Parse(item["X_PSTN"].GetString());
                        ucRackLayout.RackStateCode = item["STSTUS"].GetString();
                        ucRackLayout.ProjectName = item["PRJT_NAME"].GetString();
                        ucRackLayout.LotId = item["LOTID"].GetString();
                        ucRackLayout.SkidCarrierProductCode = item["SD_CSTPROD"].GetString();
                        ucRackLayout.SkidCarrierProductName = item["SD_CSTPROD_NAME"].GetString();
                        ucRackLayout.SkidCarrierCode = item["SD_CSTID"].GetString();
                        ucRackLayout.BobbinCarrierProductCode = item["BB_CSTPROD"].GetString();
                        ucRackLayout.BobbinCarrierProductName = item["BB_CSTPROD_NAME"].GetString();
                        ucRackLayout.BobbinCarrierCode = item["BB_CSTID"].GetString();
                        ucRackLayout.WipHold = item["WIPHOLD"].GetString();
                        ucRackLayout.CarrierDefectFlag = item["CST_DFCT_FLAG"].GetString();
                        ucRackLayout.LegendColor = item["COLOR"].GetString();
                        ucRackLayout.SkidType = item["SKID_GUBUN"].GetString();
                        ucRackLayout.AbnormalTransferReasonCode = item["ABNORM_TRF_RSN_CODE"].GetString();
                        ucRackLayout.LegendColorType = item["COLOR_GUBUN"].GetString();
                        ucRackLayout.HoldFlag = item["HOLD_FLAG"].GetString();
                        ucRackLayout.Visibility = Visibility.Visible;
                    }
                }
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectStatusbyWorkorder()
        {
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SUMMARY_BY_ASSY_WO";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgStatusbyWorkorder, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InitializeGrid()
        {
            dgProductSummary.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgProductSummary.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

            if (_isGradeJudgmentDisplay)
            {
                dgProduct.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
            }
            //20210903 오화백 : FastTrack 적용여부 체크
            if (ChkFastTrackOWNER())
            {
                dgProduct.Columns["FAST_TRACK_FLAG"].Visibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// FastTrack 적용 공장 체크
        /// </summary>
        private bool ChkFastTrackOWNER()
        {

            bool bRet = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("CMCDTYPE", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CMCDTYPE"] = "FAST_TRACK_OWNER";
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

        private void InitializeCombo()
        {
            // Area 콤보박스
            SetAreaCombo(cboArea);

            // 창고유형 콤보박스
            SetStockerTypeCombo(cboStockerType);

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // 극성 콤보박스
            SetElectrodeTypeCombo(cboElectrodeType);
        }

        private void MakeRackInfoTable()
        {
            _dtRackInfo = new DataTable();
            _dtRackInfo.Columns.Add("RACK_ID", typeof(string));
            _dtRackInfo.Columns.Add("STATUS", typeof(string));
            _dtRackInfo.Columns.Add("PRJT_NAME", typeof(string));
            _dtRackInfo.Columns.Add("LOTID", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTPROD", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTPROD_NAME", typeof(string));
            _dtRackInfo.Columns.Add("SD_CSTID", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTPROD", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTPROD_NAME", typeof(string));
            _dtRackInfo.Columns.Add("BB_CSTID", typeof(string));
            _dtRackInfo.Columns.Add("WIPHOLD", typeof(string));
            _dtRackInfo.Columns.Add("CST_DFCT_FLAG", typeof(string));
            _dtRackInfo.Columns.Add("SKID_GUBUN", typeof(string));
            _dtRackInfo.Columns.Add("COLOR_GUBUN", typeof(string));
            _dtRackInfo.Columns.Add("COLOR", typeof(string));
            _dtRackInfo.Columns.Add("ABNORM_TRF_RSN_CODE", typeof(string));
            _dtRackInfo.Columns.Add("HOLD_FLAG", typeof(string));
            _dtRackInfo.Columns.Add("SEQ", typeof(int));
        }

        private void MakeWareHouseCapacityTable()
        {
            _dtWareHouseCapacity = new DataTable();
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_NAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTID", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTNAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("RACK_MAX", typeof(decimal));      // 용량
            _dtWareHouseCapacity.Columns.Add("BBN_U_QTY", typeof(decimal));     // 실Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_UM_QTY", typeof(decimal));    // 반대극성Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_E_QTY", typeof(decimal));     // 공Carrier수
            _dtWareHouseCapacity.Columns.Add("ERROR_QTY", typeof(decimal));     // 오류Carrier수
            _dtWareHouseCapacity.Columns.Add("ABNORM_QTY", typeof(decimal));    // 정보불일치수
            _dtWareHouseCapacity.Columns.Add("RACK_RATE", typeof(double));      // 적재율
            _dtWareHouseCapacity.Columns.Add("RACK_QTY", typeof(decimal));      // 총Carrier수(실+공)
        }

        private double GetRackRate(decimal x, decimal y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception )
            {
                return 0;
            }
        }

        private double GetRackRate(int x, int y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return x.GetDouble() / y.GetDouble() * 100;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private int GetXPosition(string zPosition)
        {
            int xposition = _maxRowCount == Convert.ToInt16(zPosition) ? 0 : _maxRowCount - Convert.ToInt16(zPosition);
            return xposition;
        }

        private void SelectMaxxyz()
        {
            try
            {
                const string bizRuleName = "DA_MCS_SEL_RACK_MAX_XYZ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _selectedEquipmentCode;
                inDataTable.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    if (string.IsNullOrEmpty(searchResult.Rows[0][2].GetString()) || string.IsNullOrEmpty(searchResult.Rows[0][1].GetString()))
                    {
                        _maxRowCount = 0;
                        _maxColumnCount = 0;
                        return;
                    }

                    _maxRowCount = Convert.ToInt32(searchResult.Rows[0][2].GetString());
                    _maxColumnCount = Convert.ToInt32(searchResult.Rows[0][1].GetString());
                }
                else
                {
                    _maxRowCount = 0;
                    _maxColumnCount = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearControl()
        {
            _selectedEquipmentCode = string.Empty;
            _selectedLotElectrodeTypeCode = string.Empty;
            _selectedStkElectrodeTypeCode = string.Empty;
            _selectedProjectName = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _selectedLotIdByRackInfo = string.Empty;
            _selectedSkIdIdByRackInfo = string.Empty;
            _selectedBobbinIdByRackInfo = string.Empty;
            txtStocker.Text = string.Empty;
            txtRealCarrierCount.Text = string.Empty;
            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            _dtWareHouseCapacity?.Clear();

            Util.gridClear(dgCapacitySummary);
            Util.gridClear(dgLamiCapacitySummary);
            Util.gridClear(dgProductSummary);
            Util.gridClear(dgProduct);
            Util.gridClear(dgEmptyCarrier);
            Util.gridClear(dgCarrierList);
            Util.gridClear(dgErrorCarrier);
            Util.gridClear(dgAbNormalCarrier);
            Util.gridClear(dgRackInfo);
            InitializeRackUserControl();
        }

        private void InitializeRackUserControl()
        {
            grdColumn1.Children.Clear();
            grdColumn2.Children.Clear();

            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            if (grdColumn1.ColumnDefinitions.Count > 0) grdColumn1.ColumnDefinitions.Clear();
            if (grdColumn1.RowDefinitions.Count > 0) grdColumn1.RowDefinitions.Clear();

            if (grdColumn2.ColumnDefinitions.Count > 0) grdColumn2.ColumnDefinitions.Clear();
            if (grdColumn2.RowDefinitions.Count > 0) grdColumn2.RowDefinitions.Clear();

            if (grdStair1.ColumnDefinitions.Count > 0) grdStair1.ColumnDefinitions.Clear();
            if (grdStair1.RowDefinitions.Count > 0) grdStair1.RowDefinitions.Clear();

            if (grdStair2.ColumnDefinitions.Count > 0) grdStair2.ColumnDefinitions.Clear();
            if (grdStair2.RowDefinitions.Count > 0) grdStair2.RowDefinitions.Clear();

            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();
        }

        private void MakeColumnDefinition()
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            int colIndex = 0;

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                grdStair2.ColumnDefinitions.Add(columnDefinition2);

                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock2, colIndex);
                grdStair2.Children.Add(textBlock2);
                colIndex++;
            }
        }

        private void MakeRowDefinition()
        {
            // 단 Row 생성
            ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                grdColumn1.RowDefinitions.Add(rowDefinition1);
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdColumn2.RowDefinitions.Add(rowDefinition2);
            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                TextBlock textBlock1 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.SetValue(Grid.RowProperty, i);
                textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock1.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn1.Children.Add(textBlock1);

                TextBlock textBlock2 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.SetValue(Grid.RowProperty, i);
                textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock2.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn2.Children.Add(textBlock2);
            }
        }

        private void PrepareRackStair()
        {
            _ucRackLayout1 = new UcRackLayout[_maxRowCount][];
            _ucRackLayout2 = new UcRackLayout[_maxRowCount][];

            for (int r = 0; r < _ucRackLayout1.Length; r++)
            {
                _ucRackLayout1[r] = new UcRackLayout[_maxColumnCount];
                _ucRackLayout2[r] = new UcRackLayout[_maxColumnCount];

                for (int c = 0; c < _ucRackLayout1[r].Length; c++)
                {
                    UcRackLayout ucRackLayout1 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout1.Checked += UcRackLayout1_Checked;
                    //ucPancakeRackStair1.Click += UcRackStair1_Click;
                    //ucPancakeRackStair1.DoubleClick += UcRackStair1_DoubleClick;
                    _ucRackLayout1[r][c] = ucRackLayout1;
                }

                for (int c = 0; c < _ucRackLayout2[r].Length; c++)
                {
                    UcRackLayout ucRackLayout2 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout2.Checked += UcRackLayout2_Checked;
                    //ucPancakeRackStair2.Click += UcRackStair2_Click;
                    //ucPancakeRackStair2.DoubleClick += UcRackStair2_DoubleClick;
                    _ucRackLayout2[r][c] = ucRackLayout2;
                }
            }

        }

        private void PrepareRackStairLayout()
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();

            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();


            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
            }

            for (int row = 0; row < _maxRowCount; row++)
            {
                for (int col = 0; col < _maxColumnCount; col++)
                {
                    Grid.SetRow(_ucRackLayout1[row][col], row);
                    Grid.SetColumn(_ucRackLayout1[row][col], col);
                    grdRackstair1.Children.Add(_ucRackLayout1[row][col]);

                    Grid.SetRow(_ucRackLayout2[row][col], row);
                    Grid.SetColumn(_ucRackLayout2[row][col], col);
                    grdRackstair2.Children.Add(_ucRackLayout2[row][col]);
                }
            }
        }

        private void ReSetLayoutUserControl()
        {
            _dtRackInfo.Clear();
            InitializeRackUserControl();

            MakeRowDefinition();
            MakeColumnDefinition();
            PrepareRackStair();
            PrepareRackStairLayout();
        }

        private void GetLayoutGridColumns(string type)
        {
            for (int i = dgRackInfo.Columns.Count - 1; i >= 0; i--)
            {
                dgRackInfo.Columns.RemoveAt(i);
            }

            dgRackInfo.Refresh();

            switch (type)
            {
                case "1":
                case "3":
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "NO",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("SEQ"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_NAME",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_ID",
                        Header = ObjectDic.Instance.GetObjectName("Rack ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Rack"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "UPDDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("UPDDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MCS_CST_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("MCS_CST_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.IsReadOnly = true;
                    break;

                case "4":
                case "5":
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SEQ",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("SEQ"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_ID",
                        Header = ObjectDic.Instance.GetObjectName("Rack ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("RACK명"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTINDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("CSTINDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "BOBBIN_ID",
                        Header = ObjectDic.Instance.GetObjectName("보빈 ID"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SKID_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("SKID_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTPROD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("사용자재"),
                        Binding = new Binding() { Path = new PropertyPath("CSTPROD_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTPROD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Carrier유형"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_TYPE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    //_util.SetDataGridMergeExtensionCol(dgRackInfo, new string[] { "ELTR_TYPE_NAME"}, DataGridMergeMode.VERTICALHIERARCHI);
                    dgRackInfo.IsReadOnly = true;
                    break;


                case "2":
                    dgRackInfo.Columns.Add(new DataGridNumericColumn()
                    {
                        Name = "ROW_NUM",
                        Header = ObjectDic.Instance.GetObjectName("순위"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("RACK명"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTINDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("CSTINDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQSGNAME",
                        Header = ObjectDic.Instance.GetObjectName("LINE"),
                        Binding = new Binding() { Path = new PropertyPath("EQSGNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "BOBBIN_ID",
                        Header = ObjectDic.Instance.GetObjectName("보빈 ID"),
                        Binding = new Binding() { Path = new PropertyPath("BOBBIN_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CSTID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CSTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "LOTID",
                        Header = ObjectDic.Instance.GetObjectName("LOT ID"),
                        Binding = new Binding() { Path = new PropertyPath("LOTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "LOTYNAME",
                        Header = ObjectDic.Instance.GetObjectName("LOT유형"),
                        Binding = new Binding() { Path = new PropertyPath("LOTYNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = "WIPQTY",
                        Header = ObjectDic.Instance.GetObjectName("수량"),
                        Binding = new Binding() { Path = new PropertyPath("WIPQTY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Format = "#,##0",
                        HorizontalAlignment = HorizontalAlignment.Right
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "UNIT_CODE",
                        Header = ObjectDic.Instance.GetObjectName("단위"),
                        Binding = new Binding() { Path = new PropertyPath("UNIT_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRJT_NAME",
                        Header = ObjectDic.Instance.GetObjectName("프로젝트명"),
                        Binding = new Binding() { Path = new PropertyPath("PRJT_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HALF_SLIT_SIDE",
                        Header = ObjectDic.Instance.GetObjectName("무지부"),
                        Binding = new Binding() { Path = new PropertyPath("HALF_SLIT_SIDE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ELTR_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("극성"),
                        Binding = new Binding() { Path = new PropertyPath("ELTR_TYPE_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRODID",
                        Header = ObjectDic.Instance.GetObjectName("제품"),
                        Binding = new Binding() { Path = new PropertyPath("PRODID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PRODNAME",
                        Header = ObjectDic.Instance.GetObjectName("PRODNAME"),
                        Binding = new Binding() { Path = new PropertyPath("PRODNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "VLD_DATE",
                        Header = ObjectDic.Instance.GetObjectName("유효일자"),
                        Binding = new Binding() { Path = new PropertyPath("VLD_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PAST_DAY",
                        Header = ObjectDic.Instance.GetObjectName("경과일수"),
                        Binding = new Binding() { Path = new PropertyPath("PAST_DAY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PROD_VER_CODE",
                        Header = ObjectDic.Instance.GetObjectName("버전"),
                        Binding = new Binding() { Path = new PropertyPath("PROD_VER_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ELTR_GRD_CODE",
                        Header = ObjectDic.Instance.GetObjectName("ELTR_GRD_CODE"),
                        Binding = new Binding() { Path = new PropertyPath("ELTR_GRD_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "IQC_JUDGEMENT",
                        Header = ObjectDic.Instance.GetObjectName("IQC 검사결과"),
                        Binding = new Binding() { Path = new PropertyPath("IQC_JUDGEMENT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "QMS_JUDGEMENT_CT",
                        Header = ObjectDic.Instance.GetObjectName("QMS_JUDGEMENT_CT"),
                        Binding = new Binding() { Path = new PropertyPath("QMS_JUDGEMENT_CT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "QMS_JUDGEMENT_RP",
                        Header = ObjectDic.Instance.GetObjectName("QMS_JUDGEMENT_RP"),
                        Binding = new Binding() { Path = new PropertyPath("QMS_JUDGEMENT_RP"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "QMS_JUDGEMENT_ST",
                        Header = ObjectDic.Instance.GetObjectName("QMS_JUDGEMENT_ST"),
                        Binding = new Binding() { Path = new PropertyPath("QMS_JUDGEMENT_ST"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "VD_QA_RESULT",
                        Header = ObjectDic.Instance.GetObjectName("VD검사결과"),
                        Binding = new Binding() { Path = new PropertyPath("VD_QA_RESULT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SPCL_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("특별관리여부"),
                        Binding = new Binding() { Path = new PropertyPath("SPCL_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RSV_EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("목적지 설비명"),
                        Binding = new Binding() { Path = new PropertyPath("RSV_EQPTNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "WIPHOLD",
                        Header = ObjectDic.Instance.GetObjectName("HOLD 여부"),
                        Binding = new Binding() { Path = new PropertyPath("WIPHOLD"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_NAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD사유"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_NOTE",
                        Header = ObjectDic.Instance.GetObjectName("HOLD비고"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_NOTE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_DTTM",
                        Header = ObjectDic.Instance.GetObjectName("HOLD시간"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_DTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_USERNAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD등록자"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_USERNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ACTION_USERNAME",
                        Header = ObjectDic.Instance.GetObjectName("HOLD담당자"),
                        Binding = new Binding() { Path = new PropertyPath("ACTION_USERNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_HOLD_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("설비 보류 유형 코드"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_HOLD_TYPE_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_HOLD_CNFM_FLAG",
                        Header = ObjectDic.Instance.GetObjectName("설비 보류 확인 여부"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_HOLD_CNFM_FLAG"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = "DFCT_TAG_QTY",
                        Header = ObjectDic.Instance.GetObjectName("불량태그수"),
                        Binding = new Binding() { Path = new PropertyPath("DFCT_TAG_QTY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Format = "#,##0",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Visibility = Visibility.Collapsed
                    });

                    dgRackInfo.IsReadOnly = true;

                    dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
                    dgRackInfo.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Collapsed;

                    if (cboStockerType.SelectedValue.GetString() == "NWW")
                    {
                        dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;

                        if (LoginInfo.CFG_AREA_ID == "A7")
                        {
                            dgRackInfo.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Visible;
                        }
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "JRW")
                    {
                        dgRackInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                    }
                    else if (cboStockerType.SelectedValue.GetString() == "PCW")
                    {
                        dgRackInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Visible;
                        dgRackInfo.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Visible;
                    }
                    if (cboStockerType.SelectedValue.GetString() == "NPW" || cboStockerType.SelectedValue.GetString() == "LWW")
                    {
                        dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Visible;

                        if (cboStockerType.SelectedValue.GetString() == "LWW")
                        {
                            if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "A8" || LoginInfo.CFG_AREA_ID == "S4")
                                dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Visible;
                        }
                    }

                    if (_isGradeJudgmentDisplay)
                    {
                        dgRackInfo.Columns["ELTR_GRD_CODE"].Visibility = Visibility.Visible;
                    }

                    break;

            }
        }

        private void SelectLayoutGrid(string type)
        {
            switch (type)
            {
                case "1":   // 정보불일치
                    SelectAbNormalCarrierList(dgRackInfo);
                    break;
                case "2":   // 실보빈(LOT존재)
                    SelectWareHouseProductList(dgRackInfo);
                    break;
                case "3":   // 오류Carrier
                    SelectErrorCarrierList(dgRackInfo);
                    break;
                case "4":   // 공 Carrier
                case "5":
                    SelectWareHouseEmptyCarrierList(dgRackInfo);
                    break;
            }
        }

        private void HideAndClearAllRack()
        {
            for (int row = 0; row < grdRackstair1.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair1.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout1[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout1[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout2[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout2[row][col].Clear();
                }
            }
        }

        private void UnCheckedAllRackLayout()
        {
            for (int rowIndex = 0; rowIndex < grdRackstair1.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair1.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout1[rowIndex][colIndex];

                    if (ucRackLayout.IsChecked)
                    {
                        ucRackLayout.IsChecked = false;
                        UnCheckUcRackLayout(ucRackLayout);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair2.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair2.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout2[rowIndex][colIndex];

                    if (ucRackLayout.IsChecked)
                    {
                        ucRackLayout.IsChecked = false;
                        UnCheckUcRackLayout(ucRackLayout);
                    }
                }
            }
        }

        private void UnCheckUcRackLayout(UcRackLayout ucRackLayout)
        {
            DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID = '" + ucRackLayout.RackId + "'");
            foreach (DataRow row in selectedRow)
            {
                _dtRackInfo.Rows.Remove(row);
            }
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }

        private void CheckUcRackLayout(UcRackLayout rackLayout)
        {

            if (CommonVerify.HasTableRow(_dtRackInfo))
            {
                for (int i = 0; i < _dtRackInfo.Rows.Count; i++)
                {
                    _dtRackInfo.Rows.RemoveAt(i);
                }
            }

            _selectedLotIdByRackInfo = string.IsNullOrEmpty(rackLayout.LotId) ? null : rackLayout.LotId;
            _selectedSkIdIdByRackInfo = string.IsNullOrEmpty(rackLayout.SkidCarrierCode) ? null : rackLayout.SkidCarrierCode;
            _selectedBobbinIdByRackInfo = string.IsNullOrEmpty(rackLayout.BobbinCarrierCode) ? null : rackLayout.BobbinCarrierCode;

            int maxSeq = 1;
            DataRow dr = _dtRackInfo.NewRow();
            dr["RACK_ID"] = rackLayout.RackId;
            dr["STATUS"] = rackLayout.RackStateCode;
            dr["PRJT_NAME"] = rackLayout.ProjectName;
            dr["LOTID"] = rackLayout.LotId;
            dr["SD_CSTPROD"] = rackLayout.SkidCarrierProductCode;
            dr["SD_CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
            dr["SD_CSTID"] = rackLayout.SkidCarrierCode;
            dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
            dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
            dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
            dr["WIPHOLD"] = rackLayout.WipHold;
            dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
            dr["SKID_GUBUN"] = rackLayout.SkidType;
            dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
            dr["COLOR"] = rackLayout.LegendColor;
            dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
            dr["HOLD_FLAG"] = rackLayout.HoldFlag;
            dr["SEQ"] = maxSeq;
            _dtRackInfo.Rows.Add(dr);

            if (rackLayout.LegendColorType == "4")
            {
                _selectedSkIdIdByRackInfo = null;
            }
            else if (rackLayout.LegendColorType == "5")
            {
                _selectedBobbinIdByRackInfo = null;
            }

            GetLayoutGridColumns(rackLayout.LegendColorType);
            SelectLayoutGrid(rackLayout.LegendColorType);

        }

        private void SetScrollToHorizontalOffset(ScrollViewer scrollViewer, int colIndex)
        {
            double averageScrollWidth = scrollViewer.ActualWidth / _maxColumnCount;
            scrollViewer.ScrollToHorizontalOffset(Math.Abs(colIndex) * averageScrollWidth);
        }

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };

            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetStockerTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "BR_MCS_SEL_AREA_COM_CODE_CSTTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "CFG_AREA_ID" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), "Y", null,LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);

            ///2021-08-10 오화백  노칭대기 창고 Long Cell Row 삭제 (호출 비즈는 공통으로 사용되고 있어서 데이터 테이블로 변환시켜 노칭대기 창고 Long Cell Row는 삭제하고 다시 바인딩함
            if (cboArea.SelectedValue.GetString() == "A7")
            {
                DataTable dtStockerType = DataTableConverter.Convert(cbo.ItemsSource);

                if(dtStockerType.Rows.Count > 0)
                {
                    for( int i =0; i<dtStockerType.Rows.Count; i++)
                    {
                        if(dtStockerType.Rows[i]["CBO_CODE"].ToString() == "MNW")
                        {
                            dtStockerType.Rows[i].Delete();
                            dtStockerType.AcceptChanges();
                        }
                    }
                }

                cbo.ItemsSource = DataTableConverter.Convert(dtStockerType);
                //dtStockerType.Select("CBO_CODE = MNW").ToList<DataRow>().ForEach(row => row.Delete());
                //dtStockerType.AcceptChanges();
                //cbo.ItemsSource = DataTableConverter.Convert(dtStockerType);
            }
            
        }

        private void SetStockerCombo(C1ComboBox cbo)
        {
            //2021 08 10 조립공정의 노칭대기창고 일 경우  LONGCELL 대기창고와 통합
            if (cboArea.SelectedValue.GetString() == "A7")
            {
                string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
                string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

                if(stockerType == "NWW")
                {
                    stockerType = "NWW,MNW";
                }


                const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO_NWW";
                string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
                string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), stockerType, electrodeType };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
            }
            else
            {
                string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
                string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

                const string bizRuleName = "DA_MCS_SEL_MCS_EQUIPMENT_ELTRTYPE_CBO";
                string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
                string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), stockerType, electrodeType };
                string selectedValueText = cbo.SelectedValuePath;
                string displayMemberText = cbo.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

            }

        }

        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private bool IsGradeJudgmentDisplay()
        {
            const string bizRuleName = "DA_PRD_SEL_TB_MMD_AREA_COM_CODE";

            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsTabStatusbyWorkorderVisibility(SearchType searchType, string stockerTypeCode)
        {

            if (string.IsNullOrEmpty(stockerTypeCode)) return false;

            const string bizRuleName = "BR_MCS_SEL_AREA_COM_CODE_FOR_UI";
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["COM_TYPE_CODE"] = "AREA_EQUIPMENT_GROUP_UI";
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_CODE"] = stockerTypeCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (searchType == SearchType.Tab)
                    {
                        if (dtResult.Rows[0]["ATTR1"].GetString() == "Y")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (dtResult.Rows[0]["ATTR2"].GetString() == "Y")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 3;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();
            }
        }


        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    btnSearch_Click(null, null);
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


        #region 2021-05-19 Hold 재고 현황  by 오화백

        /// <summary>
        ///  재고 현황 조회
        /// </summary>
        private void SelectHold()
        {
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SUMMARY_BY_HOLD";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgHold, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion






        #endregion

    }
}
