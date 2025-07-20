/*************************************************************************************
 Created Date : 2022.04.27
      Creator : 오화백
   Decription : 전극 원자재 STK 현황
--------------------------------------------------------------------------------------
 [Change History]
  2022.04.27  오화백 과장 : Initial Created.    
  2025.02.13  이민형 : 데이터가 없을 시 무한 로딩. 로딩 닫기 로직 추가
  2025.03.25  오화백 과장 : 창고 유형 콤보 DA 변경 : DA_MHS_SEL_AREA_COM_CODE_CBO ==> DA_MHS_SEL_AREA_COM_CODE_CBO_ATTR
  2025.03.26  이민형 : 믹서 STO 그룹별 적재 수량 Tab 안보이도록 처리
  2025.04.11  조범모 : [창고 적재 현황] 표에서 실Carrier수 더블클릭시 입고LOT탭 이동 후 정보 표시
  2025.04.28  이민형 과장 : 금지단 컬럼 추가 및 탭 추가
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
using C1.WPF.Excel;
using System.IO;
using System.Configuration;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid.Summaries;
using Microsoft.Win32;



namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_029.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_076 : UserControl, IWorkArea
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
        private string _selectedRackIdByRackInfo;
        private string _selectMtGrID;

        private DataTable _dtWareHouseCapacity;

        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;
        private int _maxRowCount;
        private int _maxColumnCount;

        private DataTable _dtRackInfo;
        private UcRackLayout[][] _ucRackLayout1;
        private UcRackLayout[][] _ucRackLayout2;

        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

        }
        /// <summary>
        /// 생성자
        /// </summary>
        public MCS001_076()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 화면 로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeGrid();  //초기화
            InitializeCombo(); //콤보박스 셋팅
            MakeRackInfoTable();
            MakeWareHouseCapacityTable();
            TimerSetting();
            Loaded -= UserControl_Loaded;
            C1TabControl.SelectionChanged += C1TabControl_SelectionChanged;
            _isLoaded = true;
        }
        /// <summary>
        /// 화면 언드르시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
        }
        #endregion

        #region Event

        #region  동정보 이벤트 : cboArea_SelectedValueChanged()
        /// <summary>
        /// 동정보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            SetStockerTypeCombo(cboStockerType); // 창고유형
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }
        #endregion

        #region 창고유형 콤보 이벤트 : cboStockerType_SelectedValueChanged()
        /// <summary>
        /// 창고 유형  콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStockerType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker);
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;

            LeftArea.Visibility = Visibility.Visible;
            dgProduct.Columns["QTY"].Visibility = Visibility.Visible;
            // Foil 창고일 경우 빈 Carrier 탭 보여줌
            if (cboStockerType.SelectedValue.ToString() == "FWW")
            {
                tabEmptyCarrier.Visibility = Visibility.Visible;
                tabLoadQty.Visibility = Visibility.Collapsed;
                dgCapacitySummary.Columns["BBN_E_QTY"].Visibility = Visibility.Visible;
                dgProduct.Columns["QTY"].Visibility = Visibility.Collapsed;
            }
            else
            {
                tabEmptyCarrier.Visibility = Visibility.Collapsed;
                //tabLoadQty.Visibility = Visibility.Visible;
                dgCapacitySummary.Columns["BBN_E_QTY"].Visibility = Visibility.Collapsed;
                dgProduct.Columns["QTY"].Visibility = Visibility.Visible;
            }

        }
        #endregion

        #region 극성 콤보 이벤트 : cboElectrodeType_SelectedValueChanged()
        /// <summary>
        /// 극성 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboElectrodeType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboStocker.SelectedValueChanged -= cboStocker_SelectedValueChanged;
            SetStockerCombo(cboStocker); // 극성에 따른 창고 콤보 조회
            cboStocker.SelectedValueChanged += cboStocker_SelectedValueChanged;
        }
        #endregion

        #region 창고 콤보 이벤트 : cboStocker_SelectedValueChanged()
        /// <summary>
        /// 창고 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboStocker_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl(); //초기화
        }
        #endregion

        #region 타이머 콤보박스 이벤트  : cboTimer_SelectedValueChanged()
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
        #endregion

        #region 조회버튼 이벤트 : btnSearch_Click()
        /// <summary>
        /// 조회버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            ClearControl();
            SelectWareHouseCapacitySummary();

            // STOCK별 적재수량 탭을 사용 창고일때 조회
            if (tabLoadQty.Visibility == Visibility.Visible)
            {
                SearchLoadQtyList();
            }

        }
        #endregion

        #region 창고적재현황 LIST 이벤트 : dgCapacitySummary_()

        /// <summary>
        ///  창고적재현황 로드시 데이터 색깔지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgCapacitySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(e.Cell.Column.Name, "EQPTNAME") || string.Equals(e.Cell.Column.Name, "BBN_U_QTY") || string.Equals(e.Cell.Column.Name, "BBN_E_QTY") || string.Equals(e.Cell.Column.Name, "ERROR_QTY")
                    || string.Equals(e.Cell.Column.Name, "ABNORM_QTY") || string.Equals(e.Cell.Column.Name, "PROHIBIT_QTY")
                    )
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
        /// <summary>
        /// 창고적재현황 언로드시 : 색깔이 지정된 리스트의 정보가 스크롤을 내릴시 초기화 되는 문제 해결
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 창고적재현황 리스트 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                _selectedRackIdByRackInfo = null;

                if (cell.Column.Name.Equals("EQPTNAME") || (cell.Column.Name.Equals("ELTR_TYPE_NAME") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계")))) // 창고정보 클릭시
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


                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedStkElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedStkElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    Util.gridClear(dgProduct);

                    if (cell.Column.Name.Equals("EQPTNAME") || (cell.Column.Name.Equals("ELTR_TYPE_NAME") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        _selectedWipHold = null;
                        _selectedProjectName = null;

                        //레이아웃 탭 선택 및  창고정보 클릭시
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
                            //입고LOT 탭 자동 선택 및  입고 LOT 정보 조회
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
                //실 Carrier 수량 클릭시
                else if (cell.Column.Name.Equals("BBN_U_QTY"))
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

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }

                    tabProduct.IsSelected = true;
                    //입고LOT 탭 자동 선택 및  입고 LOT 정보 조회
                    SelectWareHouseProductSummary(false);
                    SelectWareHouseProductList(dgProduct);

                }
                //공 Carrier 수량 클릭시
                else if (cell.Column.Name.Equals("BBN_E_QTY"))
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

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedLotElectrodeTypeCode = null;
                    }
                    else
                    {
                        _selectedLotElectrodeTypeCode = DataTableConverter.GetValue(drv, "ELTR_TYPE_CODE").GetString();
                    }


                    tabEmptyCarrier.IsSelected = true;
                    SelectWareHouseEmptyCarrier();
                    SelectWareHouseEmptyCarrierList(dgCarrierList);
                }
                //비정상RACK 수량 클릭시
                else if (cell.Column.Name.Equals("ERROR_QTY")) //
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


                    tabErrorCarrier.IsSelected = true;
                    SelectErrorCarrierList(dgErrorCarrier);
                }
                //정보불일치 수량 클릭
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


                    tabAbNormalCarrier.IsSelected = true;
                    SelectAbNormalCarrierList(dgAbNormalCarrier);
                }
                else if (cell.Column.Name.Equals("PROHIBIT_QTY"))  //금지단 수량 클릭
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

                    tabProHibit.IsSelected = true;
                    SelectProHibitList(dgProHibit);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 창고적재현황  데이터 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        #endregion

        #region 자재창고현황 LIST 이벤트 :  dgProductSummary_ () 

        /// <summary>
        /// 자재창고현황 로드시 색깔지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        //HOLD 비율이 20% 미만이면 빨간색 표시
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

        /// <summary>
        /// 자재창고현황 언로드시 : 색깔이 지정된 리스트의 정보가 스크롤을 내릴시 초기화 되는 문제 해결
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 자재창고현황 클릭시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                _selectedStkElectrodeTypeCode = null;
                _selectedRackIdByRackInfo = null;
                _selectedQmsHold = null;
                _selectMtGrID = null;

                _selectMtGrID = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "MTRLID").GetString()) ? null : DataTableConverter.GetValue(drv, "MTRLID").GetString();
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
                tabProduct.IsSelected = true;
                //입고LOT 정보 조회
                SelectWareHouseProductList(dgProduct);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 입고LOT LIST 이벤트 : dgProduct_()
        /// <summary>
        /// 입고LOT 로드시 색깔지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                //경과일수에 따라 색깔지정
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (Convert.ToString(e.Cell.Column.Name) == "PAST_DAY")
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
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F2CB61"));
                        }
                        else
                            e.Cell.Presenter.Background = null;
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "HOLD_YN")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetString() == "Y")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (Convert.ToString(e.Cell.Column.Name) == "VLD_DAY")
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() <= 7)
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
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


        /// <summary>
        /// 입고LOT 언로드시 : 지정된 데이터의 색깔이 스크롤에 따라 초기화 되는 문제 방지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        private void dgProduct_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isscrollToHorizontalOffset = false;
        }

        #endregion

        #region STO 그룹별 설정 적재수량  LIST 이벤트 : dgStocGrChoice_()

        /// <summary>
        /// 적재수량 리스트 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStocGr_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //STO 그룹에 따라 MAX CAPA 수량 및 설정 재고수량 머지             
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgStocGr.TopRows.Count; i < dgStocGr.Rows.Count; i++)
                {

                    if (dgStocGr.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgStocGr.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CHK"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CHK"].Index)));
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CAPA_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CAPA_QTY"].Index)));
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["SAFE_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["SAFE_QTY"].Index)));
                                    //e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["EQSGNAME"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["EQSGNAME"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CHK"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CHK"].Index)));
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["CAPA_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["CAPA_QTY"].Index)));
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["SAFE_QTY"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["SAFE_QTY"].Index)));
                                //e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["EQSGNAME"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["EQSGNAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }
                // STO그룹 및 라인에 따라 라인 컬럼 머지
                idxS = 0;
                idxE = 0;
                bStrt = false;
                sTmpLvCd = string.Empty;
                sTmpTOTALQTY = string.Empty;
                for (int i = dgStocGr.TopRows.Count; i < dgStocGr.Rows.Count; i++)
                {

                    if (dgStocGr.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")) + Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "EQSGID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if ((Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")) + Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "EQSGID"))).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgStocGr.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["EQSGNAME"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["EQSGNAME"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgStocGr.GetCell(idxS, dgStocGr.Columns["EQSGNAME"].Index), dgStocGr.GetCell(idxE, dgStocGr.Columns["EQSGNAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "STOC_STCK_GR_NAME")) + Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[i].DataItem, "EQSGID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
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
        /// STOCKER 그룹별 리스트 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgStocGrChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                dgStocGr.SelectedIndex = idx;
                txtStck_Gr.Text = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "STOC_STCK_GR_NAME"));
                txtStck_Gr.Tag = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "STOC_STCK_GR_ID"));
                txtSafeQty.Text = Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "SAFE_QTY"));
                GetLineLoadQty(Util.NVC(DataTableConverter.GetValue(dgStocGr.Rows[idx].DataItem, "STOC_STCK_GR_ID")));

            }
        }

        #endregion

        #region 적재수량 설정 LIST 이벤트 : dgLineLoadQty_
        /// <summary>
        /// 적재수량 데이터의 수정가능여부 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLineLoadQty_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    return;
                }

                // 기존 데이터 일경우 안전재고비율만 수정 가능
                else if (DataTableConverter.GetValue(e.Row.DataItem, "TYPE").ToString() == "Y")
                {
                    if (e.Column.Name.Equals("EQSGID") ||
                        e.Column.Name.Equals("MTRNAME") ||
                        e.Column.Name.Equals("LOAD_QTY")
                       )
                        e.Cancel = true;

                    return;
                }
                else
                    e.Cancel = false;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// 적재수량 데이터 수정시 : 등록된 적재수량 비율에 따른 수량 자동 계산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLineLoadQty_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {

                if (e.Cell.Column.Name.Equals("LOAD_RATE") || e.Cell.Column.Name.Equals("USE_FLAG") || e.Cell.Column.Name.Equals("EQSGID"))
                {
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "CHK", 1);
                }
                if (e.Cell.Column.Name.Equals("LOAD_RATE"))
                {
                    decimal _TOTLOADQTY = 0;

                    _TOTLOADQTY = Convert.ToDecimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOAD_RATE").GetString()) * Convert.ToDecimal(txtSafeQty.Text) / 100;
                    DataTableConverter.SetValue(e.Cell.Row.DataItem, "LOAD_QTY", _TOTLOADQTY);
                }
                dgLineLoadQty.EndEdit();
                dgLineLoadQty.Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        /// <summary>
        /// 적재수량설정 리스트 로드시 색깔지정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLineLoadQty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(e.Cell.Column.Name, "CHK"))
                {
                    return;
                }
                else if (string.Equals(e.Cell.Column.Name, "EQSGID") || string.Equals(e.Cell.Column.Name, "MTRNAME"))
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "TYPE").GetString() == "Y")
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
                else if (string.Equals(e.Cell.Column.Name, "LOAD_QTY"))
                {
                    if (!String.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TYPE").GetString()))
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }

                }
                else if (string.Equals(e.Cell.Column.Name, "LOAD_RATE") || string.Equals(e.Cell.Column.Name, "USE_FLAG"))
                {
                    if (!String.IsNullOrEmpty(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TYPE").GetString()))
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }

                }
                else
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                }
            }));
        }

        /// <summary>
        /// 적재수량설정 리스트 데이터 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgLineLoadQty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("MTRNAME"))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "TYPE")).Equals("N")) return;
                    //MIX 자재 정보를 가져오기 위해 팝업 
                    MCS001_076_WORKORDER_MX_MTRL wndMtrl = new MCS001_076_WORKORDER_MX_MTRL();
                    wndMtrl.FrameOperation = FrameOperation;

                    if (wndMtrl != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = dg.GetCurrentRowIndex();
                        Parameters[1] = Process.MIXING;
                        C1WindowExtension.SetParameters(wndMtrl, Parameters);

                        wndMtrl.Closed += new EventHandler(wndMtrl_Closed);
                        grdMain.Children.Add(wndMtrl);
                        wndMtrl.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndMtrl_Closed(object sender, EventArgs e)
        {
            MCS001_076_WORKORDER_MX_MTRL wndMtrl = sender as MCS001_076_WORKORDER_MX_MTRL;
            if (wndMtrl.DialogResult == MessageBoxResult.OK)
            {
                DataTable dt = new DataTable();


                dt = DataTableConverter.Convert(dgLineLoadQty.ItemsSource);

                dt.Rows[wndMtrl.ROW]["MTRLID"] = wndMtrl.MTRLID;
                dt.Rows[wndMtrl.ROW]["MTRNAME"] = wndMtrl.MTRLDESC;

                dgLineLoadQty.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                dgLineLoadQty.ScrollIntoView(wndMtrl.ROW, dgLineLoadQty.Columns["MTRNAME"].Index);
                DataTableConverter.SetValue(dgLineLoadQty.Rows[wndMtrl.ROW].DataItem, "CHK", 1);
                DataTableConverter.SetValue(dgLineLoadQty.Rows[wndMtrl.ROW].DataItem, "MTRLID", wndMtrl.MTRLID);
                DataTableConverter.SetValue(dgLineLoadQty.Rows[wndMtrl.ROW].DataItem, "MTRNAME", wndMtrl.MTRLDESC);
            }
        }

        /// <summary>
        /// 적재수량설정 전체선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgLineLoadQty);
        }
        /// <summary>
        /// 적재수량설정 전체선택 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgLineLoadQty);
        }


        #endregion

        #region 적재수량설정 LIST ROW 추가 : btnAdd_Click()
        /// <summary>
        /// 적재수량설정 리스트 ROW 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgLineLoadQty);
        }

        #endregion

        #region 적재수량설정 LIST ROW 삭제 : btnDelete_Click()
        /// <summary>
        /// 적재수량설정 LIST ROW 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgLineLoadQty);
        }

        #endregion

        #region 설정된 적재 수량 저장 : btnSave_Click()
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave()) return;
            SaveData();
        }


        #endregion

        #region  TAB 컨트롤 이벤트  : C1TabControl_SelectionChanged()
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
                //LAYOUT 탭 선택시
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
        #endregion

        #region LAYOUT RACK Check 이벤트 : UcRackLayout1_Checked(), UcRackLayout2_Checked()

        /// <summary>
        /// 1열 LAYOUT RACK 체크 : 선택된 RACK에 대한 상세 정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout1_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            _selectedRackIdByRackInfo = null;
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
                _selectedRackIdByRackInfo = string.IsNullOrEmpty(rackLayout.RackId) ? null : rackLayout.RackId;

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

        /// <summary>
        /// 2열 LAYOUT RACK 체크 : 선택된 RACK에 대한 상세 정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout2_Checked(object sender, RoutedEventArgs e)
        {
            UcRackLayout rackLayout = sender as UcRackLayout;
            if (rackLayout == null) return;

            Util.gridClear(dgRackInfo);
            _selectedLotIdByRackInfo = null;
            _selectedSkIdIdByRackInfo = null;
            _selectedBobbinIdByRackInfo = null;
            _selectedRackIdByRackInfo = null;
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
                _selectedRackIdByRackInfo = string.IsNullOrEmpty(rackLayout.RackId) ? null : rackLayout.RackId;
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

        #endregion

        #region LOT TEXT 박스 이벤트 : textBox_KeyDown()

        /// <summary>
        ///  LOT으로 조회된 LAYOUT RACK을 찾기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion

        #region Slitter 이벤트 : Splitter_DragStarted(), Splitter_DragCompleted()
        private void Splitter_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {

        }

        private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            GridSplitter splitter = (GridSplitter)sender;

            try
            {
                C1DataGrid dataGrid = dgCapacitySummary;
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

        #endregion

        #region Timer 실행 : _dispatcherTimer_Tick()
        /// <summary>
        /// 타이머 실행 : 조회 버튼 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    if (tabLoadQty.IsSelected == false)
                    {
                        btnSearch_Click(null, null);
                    }

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

        #endregion

        #endregion

        #region Method

        #region 전체 Control 초기화 : ClearControl()

        /// <summary>
        /// 초기화
        /// </summary>
        private void ClearControl()
        {
            _selectedEquipmentCode = string.Empty;
            _selectedLotElectrodeTypeCode = string.Empty;
            _selectedStkElectrodeTypeCode = string.Empty;
            _selectedRackIdByRackInfo = string.Empty;
            _selectedProjectName = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _selectedLotIdByRackInfo = string.Empty;
            _selectedSkIdIdByRackInfo = string.Empty;
            _selectedBobbinIdByRackInfo = string.Empty;

            txtLotId.Text = string.Empty;
            txtBobbinId.Text = string.Empty;

            _dtWareHouseCapacity?.Clear();

            Util.gridClear(dgCapacitySummary);
            Util.gridClear(dgProductSummary);
            Util.gridClear(dgProduct);
            Util.gridClear(dgEmptyCarrier);
            Util.gridClear(dgCarrierList);
            Util.gridClear(dgErrorCarrier);
            Util.gridClear(dgAbNormalCarrier);
            Util.gridClear(dgRackInfo);
            InitializeRackUserControl();
        }

        #endregion


        #region  콤보박스 셋팅 : InitializeCombo()
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

            //==========================================================================================

            //라인아이디 콤보
            SetDataGridEqsgidCombo(dgLineLoadQty.Columns["EQSGID"]);

            //사용 여부 Set
            SetDataGridUseFlagCombo(dgLineLoadQty.Columns["USE_FLAG"]);
        }
        #endregion

        #region 동정보 콤보박스 조회 : SetAreaCombo()

        /// <summary>
        /// 동정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_LOGIS_GROUP_MTRL_CBO";
            string[] arrColumn = { "LANGID", "AREAID" };

            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }




        #endregion

        #region 창고유형 콤보박스 조회 : SetStockerTypeCombo()

        /// <summary>
        /// 창고 유형 콤보박스 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {

            //const string bizRuleName = "DA_MHS_SEL_AREA_COM_CODE_CBO";
            //string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "COM_TYPE_CODE" };
            //string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), null, null, "AREA_EQUIPMENT_MTRL_GROUP" };

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "ATTR3", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "Y", "Y", "N", "AREA_EQUIPMENT_MTRL_GROUP" };

            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);


        }

        #endregion

        #region 창고 콤보박스 조회 : SetStockerCombo()

        /// <summary>
        /// 창고 정보 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerCombo(C1ComboBox cbo)
        {
            string stockerType = string.IsNullOrEmpty(cboStockerType.SelectedValue.GetString()) ? null : cboStockerType.SelectedValue.GetString();
            string electrodeType = string.IsNullOrEmpty(cboElectrodeType.SelectedValue.GetString()) ? null : cboElectrodeType.SelectedValue.GetString();

            const string bizRuleName = "DA_MHS_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", "ELTR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), stockerType, electrodeType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        #endregion

        #region 극성 콤보박스 조회 : SetElectrodeTypeCombo()
        private void SetElectrodeTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELTR_TYPE_CODE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        #endregion

        #region STO 그룹별 안전재고 설정 : 전극라인 콤보 조회

        /// <summary>
        /// 전극라인 조회
        /// </summary>
        /// <param name="dgcol"></param>
        private void SetDataGridEqsgidCombo(C1.WPF.DataGrid.DataGridColumn dgcol)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO_V2";
            string[] arrColumn = { "CMCDTYPE", "LANGID", "ATTRIBUTE2" };
            //string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE2" };
            string[] arrCondition = { "ELTR_LINE_ID", LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, dgcol, selectedValueText, displayMemberText);
        }

        #endregion

        #region STO 그룹별 안전재고 설정 : 리스트 내 콤보박스 셋팅(전극라인) : SetDataGridComboItem()
        /// <summary>
        /// 리스트 내 콤보박스 셋팅       
        /// </summary>
        /// <param name="bizRuleName"></param>
        /// <param name="arrColumn"></param>
        /// <param name="arrCondition"></param>
        /// <param name="dgcol"></param>
        /// <param name="selectedValueText"></param>
        /// <param name="displayMemberText"></param>
        public void SetDataGridComboItem(string bizRuleName, string[] arrColumn, string[] arrCondition, C1.WPF.DataGrid.DataGridColumn dgcol, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable { TableName = "RQSTDT" };

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                        inDataTable.Columns.Add(col, typeof(string));

                    DataRow dr = inDataTable.NewRow();

                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];

                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);


                DataRow dr2 = dtResult.NewRow();
                dr2 = dtResult.NewRow();

                dr2["CBO_NAME"] = "-ALL-";
                dr2["CBO_CODE"] = "ALL";

                dtResult.Rows.InsertAt(dr2, 0);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });
                C1.WPF.DataGrid.DataGridComboBoxColumn dataGridComboBoxColumn = dgcol as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (dataGridComboBoxColumn != null)
                    dataGridComboBoxColumn.ItemsSource = dtBinding.Copy().AsDataView();
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region STO 그룹별 안전재고 설정 : 사용여부 콤보 조회

        /// <summary>
        /// 사용여부 조회
        /// </summary>
        /// <param name="dgcol"></param>
        private static void SetDataGridUseFlagCombo(C1.WPF.DataGrid.DataGridColumn dgcol)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "CMCDTYPE", "LANGID" };
            string[] arrCondition = { "IUSE", LoginInfo.LANGID };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, dgcol, selectedValueText, displayMemberText);
        }

        #endregion


        #region 창고적재현황 DATATABLE : MakeWareHouseCapacityTable()

        /// <summary>
        ///  합계 및 창고 적재율 계산위하여 생성
        /// </summary>
        private void MakeWareHouseCapacityTable()
        {
            _dtWareHouseCapacity = new DataTable();
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_NAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTID", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTNAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("RACK_MAX", typeof(decimal));      // 용량
            _dtWareHouseCapacity.Columns.Add("BBN_U_QTY", typeof(decimal));     // 실Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_E_QTY", typeof(decimal));     // 공Carrier수
            _dtWareHouseCapacity.Columns.Add("ERROR_QTY", typeof(decimal));     // 비정상RACK
            _dtWareHouseCapacity.Columns.Add("ABNORM_QTY", typeof(decimal));    // 정보불일치수
            _dtWareHouseCapacity.Columns.Add("PROHIBIT_QTY", typeof(decimal));    // 정보불일치수
            _dtWareHouseCapacity.Columns.Add("RACK_RATE", typeof(double));      // 적재율
            _dtWareHouseCapacity.Columns.Add("RACK_QTY", typeof(decimal));      // 총Carrier수(실+공)
        }


        #endregion

        #region 창고 적재 현황 조회 : SelectWareHouseCapacitySummary()

        /// <summary>
        /// 창고 적재 현황 조회
        /// </summary>
        private void SelectWareHouseCapacitySummary()
        {
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LOAD_SUMMARY_MTRL";
            try
            {
                DataTable dtResult = new DataTable();

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["EQPTID"] = cboStocker.SelectedValue;

                inTable.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (bizResult.Rows.Count > 0)
                {
                    // 극성별 소계
                    var query = bizResult.AsEnumerable().GroupBy(x => new
                    {
                        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE"),
                        electrodeTypeName = x.Field<string>("ELTR_TYPE_NAME"),
                    }).Select(g => new
                    {
                        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                        ElectrodeTypeName = g.Key.electrodeTypeName,
                        EquipmentCode = "XXXXXXXXXX",
                        EquipmentName = g.Key.electrodeTypeName + "  " + ObjectDic.Instance.GetObjectName("소계"),
                        //RackMax = g.Sum(x => x.Field<Int32>("RACK_MAX")),  // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //RealCarrierCount = g.Sum(x => x.Field<Int32>("BBN_U_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //EmptyCarrierCount = g.Sum(x => x.Field<Int32>("BBN_E_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //AbnormalCount = g.Sum(x => x.Field<Int32>("ABNORM_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //ErrorCarrierCount = g.Sum(x => x.Field<Int32>("ERROR_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //SumCarrierCount = g.Sum(x => x.Field<Int32>("RACK_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //RackRate = GetRackRate(g.Sum(x => x.Field<Int32>("BBN_U_QTY")), g.Sum(x => x.Field<Int32>("RACK_MAX"))), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.

                        RackMax = g.Sum(x => x.Field<long>("RACK_MAX")),
                        RealCarrierCount = g.Sum(x => x.Field<long>("BBN_U_QTY")),
                        EmptyCarrierCount = g.Sum(x => x.Field<long>("BBN_E_QTY")),
                        AbnormalCount = g.Sum(x => x.Field<long>("ABNORM_QTY")),
                        ProhibitCount = g.Sum(x => x.Field<long>("PROHIBIT_QTY")),
                        ErrorCarrierCount = g.Sum(x => x.Field<long>("ERROR_QTY")),
                        SumCarrierCount = g.Sum(x => x.Field<long>("RACK_QTY")),
                        RackRate = GetRackRate(g.Sum(x => x.Field<long>("BBN_U_QTY")), g.Sum(x => x.Field<long>("RACK_MAX"))),

                        Count = g.Count()
                    }).ToList();
                    // 합계
                    var querySum = bizResult.AsEnumerable().GroupBy(x => new
                    { }).Select(g => new
                    {
                        ElectrodeTypeCode = "ZZZZZZZZZZ",
                        ElectrodeTypeName = ObjectDic.Instance.GetObjectName("합계"),
                        //RackMax = g.Sum(x => x.Field<Int32>("RACK_MAX")),  // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //RealCarrierCount = g.Sum(x => x.Field<Int32>("BBN_U_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //EmptyCarrierCount = g.Sum(x => x.Field<Int32>("BBN_E_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //ErrorCarrierCount = g.Sum(x => x.Field<Int32>("ERROR_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //AbnormalCount = g.Sum(x => x.Field<Int32>("ABNORM_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //SumCarrierCount = g.Sum(x => x.Field<Int32>("RACK_QTY")), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        //RackRate = GetRackRate(g.Sum(x => x.Field<Int32>("BBN_U_QTY")), g.Sum(x => x.Field<Int32>("RACK_MAX"))), // 2024.11.19 김영국 - DB Type에 따른 Type 변경.
                        RackMax = g.Sum(x => x.Field<long>("RACK_MAX")),
                        RealCarrierCount = g.Sum(x => x.Field<long>("BBN_U_QTY")),
                        EmptyCarrierCount = g.Sum(x => x.Field<long>("BBN_E_QTY")),
                        ErrorCarrierCount = g.Sum(x => x.Field<long>("ERROR_QTY")),
                        AbnormalCount = g.Sum(x => x.Field<long>("ABNORM_QTY")),
                        ProhibitCount = g.Sum(x => x.Field<long>("PROHIBIT_QTY")),
                        SumCarrierCount = g.Sum(x => x.Field<long>("RACK_QTY")),
                        RackRate = GetRackRate(g.Sum(x => x.Field<long>("BBN_U_QTY")), g.Sum(x => x.Field<long>("RACK_MAX"))),
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
                            newRow["ERROR_QTY"] = bizResult.Rows[i]["ERROR_QTY"];
                            newRow["ABNORM_QTY"] = bizResult.Rows[i]["ABNORM_QTY"];
                            newRow["PROHIBIT_QTY"] = bizResult.Rows[i]["PROHIBIT_QTY"];
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
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["PROHIBIT_QTY"] = item.ProhibitCount;
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
                                newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                                newRow["ABNORM_QTY"] = item.AbnormalCount;
                                newRow["PROHIBIT_QTY"] = item.ProhibitCount;
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
                }
                else
                {
                    HiddenLoadingIndicator();
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 자재창고 현황 리스트 초기화 : InitializeGrid()
        /// <summary>
        /// 자재창고현황 리스트 초기화
        /// </summary>
        private void InitializeGrid()
        {
            dgProductSummary.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgProductSummary.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

        }


        #endregion

        #region 자재 창고 현황 조회 : SelectWareHouseProductSummary()

        /// <summary>
        /// 자재 창고 현황
        /// </summary>
        /// <param name="isRefresh"></param>
        private void SelectWareHouseProductSummary(bool isRefresh = false)
        {

            string bizRuleName = string.Empty;
            bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_PER_PRODUCT_MTRL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
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


        #endregion

        #region 입고 LOT 조회 : SelectWareHouseProductList()

        /// <summary>
        /// 입고 LOT 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectWareHouseProductList(C1DataGrid dg)
        {
            string bizRuleName = string.Empty;

            bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST_MTRL";


            try
            {
                ShowLoadingIndicator();

                if (dg.Name == "dgRackInfo")
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));
                    inTable.Columns.Add("EQGRID", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("RACK_ID", typeof(string));
                    DataRow dr = inTable.NewRow();

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue;
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                    dr["EQPTID"] = _selectedEquipmentCode;
                    dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                    dr["RACK_ID"] = _selectedRackIdByRackInfo;

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
                else
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("AREAID", typeof(string));
                    inTable.Columns.Add("EQGRID", typeof(string));
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("MTRLID", typeof(string));
                    inTable.Columns.Add("MTRL_DFCT_FLAG", typeof(string));

                    DataRow dr = inTable.NewRow();

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = cboArea.SelectedValue;
                    dr["EQGRID"] = cboStockerType.SelectedValue;
                    dr["EQPTID"] = _selectedEquipmentCode;
                    dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                    dr["MTRLID"] = _selectMtGrID;
                    dr["MTRL_DFCT_FLAG"] = _selectedWipHold;

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


            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 입고 LOT 조회 : LAYOUT RACK 클릭시 호출 
        /// </summary>
        /// <param name="actionCompleted"></param>
        private void SelectWareHouseProductList(Action<DataTable, Exception> actionCompleted = null)
        {

            string bizRuleName = string.Empty;
            bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST_MTRL";
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                //inTable.Columns.Add("MTRLID", typeof(string));
                //inTable.Columns.Add("MLOT_ID", typeof(string));
                //inTable.Columns.Add("PAST_DAY", typeof(string));
                //inTable.Columns.Add("FIFO", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
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

        #endregion

        #region 공 Carrier 정보 조회 : SelectWareHouseEmptyCarrier(), SelectWareHouseEmptyCarrierList()
        /// <summary>
        /// 공 Carrier 정보 조회
        /// </summary>
        private void SelectWareHouseEmptyCarrier()
        {
            string bizRuleName = string.Empty;
            bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_EMPTY_CST_MTRL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));



                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;

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
        /// <summary>
        /// 공캐리어 정보 상세 리스트
        /// </summary>
        /// <param name="dg"></param>
        private void SelectWareHouseEmptyCarrierList(C1DataGrid dg)
        {
            string bizRuleName = string.Empty;
            bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_EMPTY_CST_MTRL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["RACK_ID"] = _selectedRackIdByRackInfo;
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

        #endregion

        #region 비정상 RACK 정보 조회 : SelectErrorCarrierList()

        /// <summary>
        ///비정상 Rack 정보 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectErrorCarrierList(C1DataGrid dg)
        {

            string bizRuleName = string.Empty;

            bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_ABNORM_RACK";
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
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CST_TYPE_CODE"] = null;
                dr["RACK_ID"] = _selectedRackIdByRackInfo;
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

        #endregion

        #region 정보 불일치 정보 조회 : SelectAbNormalCarrierList()

        /// <summary>
        /// 정보 불일치
        /// </summary>
        /// <param name="dg"></param>
        private void SelectAbNormalCarrierList(C1DataGrid dg)
        {
            string bizRuleName = string.Empty;
            bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_ABNORM_CST_MTRL";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = cboStockerType.SelectedValue;
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["RACK_ID"] = _selectedRackIdByRackInfo;
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
        #endregion

        #region 금지단 리스트 조회 : SelectProHibitList()
        /// <summary>
        /// 금지단 리스트 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectProHibitList(C1DataGrid dg)
        {
            const string bizRuleName = "DA_MHS_SEL_DISABLE_RACK_MTRL_LIST";

            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("WH_TYPE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["WH_TYPE"] = cboStockerType.SelectedValue;                
                dr["STK_ID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
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

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion


        #region STO 그룹별 셋팅 안전재고 조회 : GetLineLoadQty()


        /// <summary>
        /// Stocker 그룹별 라인에 대한 안전재고 리스트
        /// </summary>
        /// <param name="sReqID"></param>
        private void GetLineLoadQty(string sReqID)
        {
            try
            {
                Util.gridClear(dgLineLoadQty);


                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("STOC_STCK_GR_ID", typeof(string));


                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["STOC_STCK_GR_ID"] = sReqID;

                //Indata["REQ_DATE"] = null;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_MIX_STOCK_LINE_LOAD_QTY_LIST", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        dtMain.Rows[i]["LOAD_QTY"] = Convert.ToDecimal(dtMain.Rows[i]["LOAD_RATE"].ToString()) * Convert.ToDecimal(txtSafeQty.Text) / 100;

                    }

                }


                Util.GridSetData(dgLineLoadQty, dtMain, FrameOperation, true);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        #endregion

        #region STO 그룹별 설정된 안전재고 조회 : SearchLoadQtyList()
        /// <summary>
        ///  적재수량 조회
        /// </summary>
        private void SearchLoadQtyList()
        {
            Util.gridClear(dgStocGr);
            Util.gridClear(dgLineLoadQty);

            txtStck_Gr.Text = string.Empty;
            txtStck_Gr.Tag = null;
            txtSafeQty.Text = string.Empty;

            //const string bizRuleName = "DA_MHS_SEL_MIX_STOCK_LOAD_QTY_LIST";
            try
            {
                //ShowLoadingIndicator();
                //DataTable inTable = new DataTable("RQSTDT");
                //inTable.Columns.Add("LANGID", typeof(string));
                //inTable.Columns.Add("AREAID", typeof(string));

                //DataRow dr = inTable.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                //inTable.Rows.Add(dr);

                //new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                //{
                //    HiddenLoadingIndicator();
                //    if (bizException != null)
                //    {
                //        Util.MessageException(bizException);
                //        return;
                //    }

                //    if (bizResult.Rows.Count > 0)
                //    {
                //        for (int i = 0; i < bizResult.Rows.Count; i++)
                //        {
                //            if (bizResult.Rows[i]["EQSGID"].ToString() == "ALL")
                //            {
                //                bizResult.Rows[i]["EQSGNAME"] = ObjectDic.Instance.GetObjectName("공통");
                //            }

                //        }

                //    }


                //    Util.GridSetData(dgStocGr, bizResult, null, true);

                //    dgStocGr.MergingCells -= dgStocGr_MergingCells;
                //    dgStocGr.MergingCells += dgStocGr_MergingCells;

                //});
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STO 그룹별 셋팅 안전재고 저장 :  SaveData()

        /// <summary>
        /// 셋팅된 안전재고 저장
        /// </summary>
        private void SaveData()
        {
            try
            {
                const string bizRuleName = "DA_MHS_SEL_MIX_STOCK_LOAD_QTY_MNGT";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("STOC_STCK_GR_ID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("LOAD_QTY", typeof(decimal));
                inDataTable.Columns.Add("LOAD_RATE", typeof(decimal));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SYSDATE", typeof(DateTime));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgLineLoadQty.Rows)
                {
                    if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1"))
                    {
                        // 신규ROW에서 사용여부 N인것은 제외
                        if (!(Util.NVC(DataTableConverter.GetValue(row.DataItem, "TYPE")).GetString() == "N" && Util.NVC(DataTableConverter.GetValue(row.DataItem, "USE_FLAG")).GetString() == "N"))
                        {

                            DataRow dr = inDataTable.NewRow();
                            dr["STOC_STCK_GR_ID"] = txtStck_Gr.Tag.ToString();
                            dr["EQSGID"] = DataTableConverter.GetValue(row.DataItem, "EQSGID").GetString();
                            dr["EQPTID"] = "-";
                            dr["PRODID"] = DataTableConverter.GetValue(row.DataItem, "MTRLID").GetString();
                            dr["LOAD_QTY"] = 0;
                            dr["LOAD_RATE"] = DataTableConverter.GetValue(row.DataItem, "LOAD_RATE").GetString();
                            dr["USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "USE_FLAG").GetString();
                            dr["USERID"] = LoginInfo.USERID;
                            dr["SYSDATE"] = System.DateTime.Now;
                            inDataTable.Rows.Add(dr);
                        }
                    }
                }

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", null, inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상처리되었습니다.
                        SearchLoadQtyList();
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region STO 그룹별 안전재고설정 리스트 ROW 추가 : DataGridRowAdd()

        /// <summary>
        /// ROW 추가
        /// </summary>
        /// <param name="dg"></param>
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {

                if (txtStck_Gr.Text == string.Empty) return;

                DataTable dt = new DataTable();
                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr2 = dt.NewRow();
                dr2["CHK"] = true;
                dr2["USE_FLAG"] = "Y";
                dr2["EQSGID"] = "";
                dr2["LOAD_RATE"] = 0;
                dr2["LOAD_QTY"] = 0;
                dr2["TYPE"] = "N";
                dt.Rows.Add(dr2);
                dt.AcceptChanges();

                dg.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }



        #endregion

        #region STO 그룹별 안전재고설정 리스트 ROW 삭제 : DataGridRowRemove()

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                List<DataRow> drInfo = dt.Select("CHK = 1")?.ToList();
                foreach (DataRow dr in drInfo)
                {
                    if (dr["TYPE"].Equals("N"))
                    {
                        dt.Rows.Remove(dr);
                    }
                }
                Util.GridSetData(dg, dt, FrameOperation, true);
                dgLineLoadQty.EndEdit();
                dgLineLoadQty.Refresh();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region STO 그룹별 안전재고 수장 저장시 Validation : ValidationSave()
        private bool ValidationSave()
        {
            if (!CommonVerify.HasDataGridRow(dgLineLoadQty))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            DataRow[] drChk = Util.gridGetChecked(ref dgLineLoadQty, "CHK");

            if (drChk.Length <= 0)
            {
                Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                return false;
            }


            foreach (C1.WPF.DataGrid.DataGridRow row in dgLineLoadQty.Rows)
            {
                if (row.Type == DataGridRowType.Item && (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "True" || Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1"))
                {
                    //라인 체크
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "EQSGID").GetString()))
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("라인"));
                        return false;
                    }
                    // 자재 체크
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "MTRLID").GetString()))
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("자재"));
                        return false;
                    }


                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(row.DataItem, "LOAD_RATE").GetString()) || Convert.ToDecimal(DataTableConverter.GetValue(row.DataItem, "LOAD_RATE").GetString()) == 0)
                    {
                        //%1(을)를 선택하세요.
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("적재설정비율(%)"));
                        return false;
                    }

                }
            }
            DataTable dt = new DataTable();
            dt = DataTableConverter.Convert(dgLineLoadQty.ItemsSource);
            decimal _totloadqty = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                _totloadqty = Convert.ToDecimal(dt.Rows[i]["LOAD_RATE"].ToString()) + _totloadqty;
            }

            if (_totloadqty > 100)
            {
                //적재설정비율이 100%를 넘었습니다.
                Util.MessageValidation("SUF4968");
                return false;
            }


            return true;
        }

        #endregion


        #region 창고적재율 계산 : GetRackRate()

        /// <summary>
        /// 창고 적재율 계산
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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

        private double GetRackRate(long x, long y)
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

        #endregion

        #region 타이머 셋팅 : TimerSetting()

        /// <summary>
        /// 타이머 셋팅
        /// </summary>
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

        #endregion

        #region DoEvents : DoEvents()

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        #endregion

        #region  LoadingIndicator : ShowLoadingIndicator(), HiddenLoadingIndicator()

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


        #endregion

        #region LayOut TAB 셋팅 관련 

        /// <summary>
        /// RACK에 대한 상세정보를 위한 DATA TABLE 셋팅
        /// </summary>
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

        /// <summary>
        /// RACK UserControl 초기화 
        /// </summary>
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

        /// <summary>
        ///  UserControl 리셋
        /// </summary>
        private void ReSetLayoutUserControl()
        {
            _dtRackInfo.Clear();
            InitializeRackUserControl();

            MakeRowDefinition();
            MakeColumnDefinition();
            PrepareRackStair();
            PrepareRackStairLayout();
        }

        /// <summary>
        ///  XYZ 값 조회
        /// </summary>
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

                    if (Convert.ToInt32(searchResult.Rows[0][0].GetString()) > 1)
                    {
                        scrollViewer2.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        scrollViewer2.Visibility = Visibility.Collapsed;
                    }

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

        /// <summary>
        /// Rack 정보 조회 후 USER CONTROL에 셋팅
        /// </summary>
        private void SelectRackInfo()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_MHS_SEL_WAREHOUSE_RACK_LAYOUT_MTRL";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("EQPTID", typeof(string));


                DataRow dr = inDataTable.NewRow();
                dr["EQPTID"] = _selectedEquipmentCode;

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
                        //ucRackLayout.ProjectName = item["PRJT_NAME"].GetString();
                        ucRackLayout.LotId = item["MLOT_ID"].GetString();
                        //ucRackLayout.SkidCarrierProductCode = item["SD_CSTPROD"].GetString();
                        //ucRackLayout.SkidCarrierProductName = item["SD_CSTPROD_NAME"].GetString();
                        //ucRackLayout.SkidCarrierCode = item["SD_CSTID"].GetString();
                        //ucRackLayout.BobbinCarrierProductCode = item["BB_CSTPROD"].GetString();
                        //ucRackLayout.BobbinCarrierProductName = item["BB_CSTPROD_NAME"].GetString();
                        ucRackLayout.BobbinCarrierCode = item["MTRL_CSTID"].GetString();
                        //ucRackLayout.WipHold = item["WIPHOLD"].GetString();
                        //ucRackLayout.CarrierDefectFlag = item["CST_DFCT_FLAG"].GetString();
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

        /// <summary>
        /// X 포지션 정보 조회
        /// </summary>
        /// <param name="zPosition"></param>
        /// <returns></returns>
        private int GetXPosition(string zPosition)
        {
            int xposition = _maxRowCount == Convert.ToInt16(zPosition) ? 0 : _maxRowCount - Convert.ToInt16(zPosition);
            return xposition;
        }

        /// <summary>
        /// 열 컬럼 생성
        /// </summary>
        private void MakeColumnDefinition()
        {
            //열 컬럼 생성
            grdStair1.Children.Clear();
            grdStair2.Children.Clear();

            int colIndex = 0;

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                grdStair2.ColumnDefinitions.Add(columnDefinition2);
                TextBlock textBlock2 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                textBlock2.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock2.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock2.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock2, colIndex);
                grdStair2.Children.Add(textBlock2);
                colIndex++;
            }
        }

        /// <summary>
        /// 단 컬럼 생성
        /// </summary>
        private void MakeRowDefinition()
        {
            // 단 Row 생성
            ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(55) };
                grdColumn1.RowDefinitions.Add(rowDefinition1);

                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(55) };
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

        /// <summary>
        /// RACK 디자인 LAYOUT 셋팅
        /// </summary>
        private void PrepareRackStairLayout()
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();
            BrushConverter converter = new BrushConverter();
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

                Border border = new Border();
                if (i == _maxColumnCount - 1)
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, 0);
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair1.Children.Add(border);


                Border border1 = new Border();
                if (i == _maxColumnCount - 1)
                {
                    border1.SetValue(Grid.RowProperty, 0);
                    border1.SetValue(Grid.ColumnProperty, i);
                    border1.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border1.SetValue(Grid.RowProperty, 0);
                    border1.SetValue(Grid.ColumnProperty, i);
                    border1.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }

                grdRackstair2.Children.Add(border1);


            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);

                Border border = new Border();
                if (i == _maxRowCount - 1)
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border.SetValue(Grid.RowProperty, i);
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair1.Children.Add(border);




                Border border1 = new Border();
                if (i == _maxRowCount - 1)
                {
                    border1.SetValue(Grid.RowProperty, i);
                    border1.SetValue(Grid.ColumnProperty, 0);
                    border1.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border1.SetValue(Grid.RowProperty, i);
                    border1.SetValue(Grid.ColumnProperty, 0);
                    border1.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border1.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border1.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair2.Children.Add(border1);


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

        /// <summary>
        /// RACK USE Control 값 매핑
        /// </summary>
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


        /// <summary>
        /// Rack 상태에 따라 상단 상세정보 그리드 셋팅
        /// </summary>
        /// <param name="type"></param>
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
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ROW_NUM",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
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
                        Name = "CARRIERID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CARRIERID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ABNORM_TRF_RSN_NAME",
                        Header = ObjectDic.Instance.GetObjectName("오류유형"),
                        Binding = new Binding() { Path = new PropertyPath("ABNORM_TRF_RSN_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.IsReadOnly = true;
                    break;


                case "3":

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ROW_NUM",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROW_NUM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPTNAME",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
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
                        Header = ObjectDic.Instance.GetObjectName("Rack"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CST_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("오류유형"),
                        Binding = new Binding() { Path = new PropertyPath("CST_TYPE_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.IsReadOnly = true;
                    break;

                case "4":
                case "5":

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ROW_NUM",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
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
                        Name = "CARRIERID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CARRIERID"), Mode = BindingMode.OneWay },
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
                        Name = "CARRIERID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CARRIERID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "LOTID",
                        Header = ObjectDic.Instance.GetObjectName("자재LOTID"),
                        Binding = new Binding() { Path = new PropertyPath("LOTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MTGRID",
                        Header = ObjectDic.Instance.GetObjectName("자재그룹"),
                        Binding = new Binding() { Path = new PropertyPath("MTGRID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MTRLID",
                        Header = ObjectDic.Instance.GetObjectName("자재코드"),
                        Binding = new Binding() { Path = new PropertyPath("MTRLID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    if (cboStockerType.SelectedValue.ToString() == "MWW")
                    {
                        dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                        {
                            Name = "QTY",
                            Header = ObjectDic.Instance.GetObjectName("수량"),
                            Binding = new Binding() { Path = new PropertyPath("QTY"), Mode = BindingMode.OneWay },
                            TextWrapping = TextWrapping.Wrap,
                            Format = "#,###.000",
                            HorizontalAlignment = HorizontalAlignment.Right
                        });
                    }

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "GRADE",
                        Header = ObjectDic.Instance.GetObjectName("등급"),
                        Binding = new Binding() { Path = new PropertyPath("GRADE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MTRL_SPEC",
                        Header = ObjectDic.Instance.GetObjectName("자재 SPEC"),
                        Binding = new Binding() { Path = new PropertyPath("MTRL_SPEC"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "VLD_DATE",
                        Header = ObjectDic.Instance.GetObjectName("유효일자"),
                        Binding = new Binding() { Path = new PropertyPath("VLD_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.IsReadOnly = true;
                    break;

            }
        }

        /// <summary>
        /// RACK 상태에 따른 데이터 정보 조회
        /// </summary>
        /// <param name="type"></param>
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
                case "4":   // 공Carrier                    
                case "5":
                    SelectWareHouseEmptyCarrierList(dgRackInfo);
                    break;
                case "6":
                    SelectProHibitList(dgRackInfo);
                    break;

            }
        }

        /// <summary>
        /// RACK 숨김 및 초기화
        /// </summary>
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
        /// <summary>
        /// 선택된 RACK 선택해제
        /// </summary>
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
        /// <summary>
        /// RACK이 선택해제 됨에 따라 상세정보 그리드 초기화 
        /// </summary>
        /// <param name="ucRackLayout"></param>
        private void UnCheckUcRackLayout(UcRackLayout ucRackLayout)
        {
            DataRow[] selectedRow = _dtRackInfo.Select("RACK_ID = '" + ucRackLayout.RackId + "'");
            foreach (DataRow row in selectedRow)
            {
                _dtRackInfo.Rows.Remove(row);
            }
            Util.GridSetData(dgRackInfo, _dtRackInfo, null, true);
        }
        /// <summary>
        /// Rack 선택에 따라  상세 정보 조회 
        /// </summary>
        /// <param name="rackLayout"></param>
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
            _selectedRackIdByRackInfo = string.IsNullOrEmpty(rackLayout.RackId) ? null : rackLayout.RackId;
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

        /// <summary>
        /// Rack 그리드에 대한 스크롤 셋팅
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="colIndex"></param>
        private void SetScrollToHorizontalOffset(ScrollViewer scrollViewer, int colIndex)
        {
            double averageScrollWidth = scrollViewer.ActualWidth / _maxColumnCount;
            scrollViewer.ScrollToHorizontalOffset(Math.Abs(colIndex) * averageScrollWidth);
        }

        #endregion
        #endregion

    }
}
