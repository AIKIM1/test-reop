/*************************************************************************************
 Created Date : 2024.09.10
      Creator : 오화백
   Decription : 자재재고현황조회
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.10  오화백 과장 : Initial Created.    
  2025.02.07  이민형 차장 : GridCell Merge 시 컬럼명 오류 수정.
  2025.02.07  이민형 차장 : GridCell Merge 제거
  2025.03.12  박승민 사원 : 자재 창고 현황 그룹별 집계 및 합계 추가
  2025.03.31  조범모 이사 : 대표carrier id 기준으로 수량 집계하여 [팔레트 수량] 항목으로 추가
  2025.04.28  이민형 책임 : 금지단 컬럼 추가 및 탭 추가
  2025.04.28  이민형 책임 : IQC_STATE -> IQC_STATE_NM 컬럼변경
  2025.06.12  오화백 책임 : 소스최신화
  2025.07.11  이민형 차장 : [MI2_OSS_0409] 자재창고 - 자재STO 재고현황 창고Combo 동별코드ATTR4->ATTR5 변경
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



namespace LGC.GMES.MES.MTRL001
{
    /// <summary>
    /// MTRL001_210.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MTRL001_210 : UserControl, IWorkArea
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
        private UcRackLayout[][] _ucRackLayout3;
        private UcRackLayout[][] _ucRackLayout4;

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
        public MTRL001_210()
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
            dgProduct.Columns["MATERIAL_QTY"].Visibility = Visibility.Visible;
            // Foil 창고일 경우 빈 Carrier 탭 보여줌
            if (cboStockerType.SelectedValue.ToString() == "MWW")
            {
                tabEmptyCarrier.Visibility = Visibility.Collapsed;

                dgCapacitySummary.Columns["EMPTY_QTY"].Visibility = Visibility.Collapsed;
                dgProduct.Columns["MATERIAL_QTY"].Visibility = Visibility.Visible;
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



        }
        #endregion

        #region 엑셀 다운로드 이벤트 : btnExcel_Click()

        /// <summary>
        /// 엑셀 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationExcel())
                    return;

                new ExcelExporter().Export(dgProduct);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                    if (string.Equals(e.Cell.Column.Name, "EQPT_NM") || string.Equals(e.Cell.Column.Name, "EMPTY_QTY") || string.Equals(e.Cell.Column.Name, "AB_RACK_QTY")
                    || string.Equals(e.Cell.Column.Name, "AB_CST_QTY") || string.Equals(e.Cell.Column.Name, "PROHIBIT_QTY")
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

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_NM")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_ID")), "XXXXXXXXXX") && !string.Equals(e.Cell.Column.Name, "ELTR_TYPE_NM"))
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

                if (cell.Column.Name.Equals("EQPT_NM") || (cell.Column.Name.Equals("ELTR_TYPE_NM") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계")))) // 창고정보 클릭시
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
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

                    if (cell.Column.Name.Equals("EQPT_NM") || (cell.Column.Name.Equals("ELTR_TYPE_NM") && cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
                    {
                        _selectedWipHold = null;
                        _selectedProjectName = null;

                        //레이아웃 탭 선택 및  창고정보 클릭시
                        if (tabLayout.IsSelected && (cell.Column.Name.Equals("EQPT_NM") || cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
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
                //공 Carrier 수량 클릭시
                else if (cell.Column.Name.Equals("EMPTY_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
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
                else if (cell.Column.Name.Equals("AB_RACK_QTY")) //
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NM").GetString(), ObjectDic.Instance.GetObjectName("합계")))
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
                else if (cell.Column.Name.Equals("AB_CST_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NM").GetString(), ObjectDic.Instance.GetObjectName("합계")))
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
                else if (cell.Column.Name.Equals("PROHIBIT_QTY"))
                {
                    if (string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "XXXXXXXXXX") ||
                        string.Equals(DataTableConverter.GetValue(drv, "EQPT_ID").GetString(), "ZZZZZZZZZZ"))
                    {
                        _selectedEquipmentCode = null;
                    }
                    else
                    {
                        _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPT_ID").GetString();
                    }

                    if (string.Equals(DataTableConverter.GetValue(drv, "ELTR_TYPE_NM").GetString(), ObjectDic.Instance.GetObjectName("합계")))
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
                //if (dg.GetRowCount() > 0)
                //{
                //    DataTable dt = ((DataView)dg.ItemsSource).Table;
                //    var query = dt.AsEnumerable().GroupBy(x => new
                //    {
                //        electrodeTypeCode = x.Field<string>("ELTR_TYPE_CODE")
                //    }).Select(g => new
                //    {
                //        ElectrodeTypeCode = g.Key.electrodeTypeCode,
                //        Count = g.Count()
                //    }).ToList();

                //    string previewElectrodeTypeCode = string.Empty;

                //    for (int i = 0; i < dg.Rows.Count; i++)
                //    {
                //        if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_NM").GetString() == ObjectDic.Instance.GetObjectName("합계"))
                //        {
                //            e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPT_NM"].Index), dg.GetCell(i, dg.Columns["EQPT_NM"].Index + 1)));
                //        }
                //        else
                //        {
                //            foreach (var item in query)
                //            {
                //                int rowIndex = i;
                //                if (DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString() == item.ElectrodeTypeCode && previewElectrodeTypeCode != DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString())
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["EQPT_NM"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["EQPT_NM"].Index)));
                //                }
                //            }
                //        }
                //        previewElectrodeTypeCode = DataTableConverter.GetValue(dg.Rows[i].DataItem, "ELTR_TYPE_CODE").GetString();
                //    }
                //}
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

            // 합계 행 추가로 색깔 지정 if문 추가
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTGRID")), ObjectDic.Instance.GetObjectName("합계")))
                {

                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else if (string.Equals(e.Cell.Column.Name, "AVAIL_QTY_1")
                    || string.Equals(e.Cell.Column.Name, "HOLD_QTY_1")
                    || string.Equals(e.Cell.Column.Name, "LOAD_REP_CSTID_QTY")
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

                if (string.Equals(e.Cell.Column.Name, "AVAIL_QTY_2")
                    || string.Equals(e.Cell.Column.Name, "HOLD_QTY_2")
                   )
                {
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
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
                    || cell.Column.Name.Equals("AVAIL_QTY_2")
                    || cell.Column.Name.Equals("HOLD_QTY_2"))
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

                _selectMtGrID = string.IsNullOrEmpty(DataTableConverter.GetValue(drv, "MATERIAL_CD").GetString()) ? null : string.Equals(DataTableConverter.GetValue(drv, "MATERIAL_CD").GetString(), "합계") ? null : DataTableConverter.GetValue(drv, "MATERIAL_CD").GetString();
                if (cell.Column.Name.Equals("AVAIL_QTY_1") || cell.Column.Name.Equals("AVAIL_QTY_2"))
                {
                    _selectedWipHold = "N";
                    _selectedQmsHold = "N";
                }
                else if (cell.Column.Name.Equals("HOLD_QTY_1") || cell.Column.Name.Equals("HOLD_QTY_2"))
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

                    if (Convert.ToString(e.Cell.Column.Name) == "HOLD_YN")
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

                for (int row = 0; row < grdRackstair3.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair3.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout3[row][col];

                        if (ucRackLayout.IsChecked)
                            ucRackLayout.IsChecked = false;
                    }
                }

                for (int row = 0; row < grdRackstair4.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair4.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout4[row][col];

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
                dr["MTRL_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["MATERIAL_CD"] = rackLayout.LotId;
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

                for (int row = 0; row < grdRackstair3.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair3.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout3[row][col];

                        if (ucRackLayout.IsChecked)
                            ucRackLayout.IsChecked = false;
                    }
                }

                for (int row = 0; row < grdRackstair4.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair4.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout4[row][col];

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
                dr["MTRL_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["MATERIAL_CD"] = rackLayout.LotId;
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


        /// <summary>
        /// 3열 LAYOUT RACK 체크 : 선택된 RACK에 대한 상세 정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout3_Checked(object sender, RoutedEventArgs e)
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
                for (int row = 0; row < grdRackstair3.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair3.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout3[row][col];

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
                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (ucRackLayout.IsChecked)
                            ucRackLayout.IsChecked = false;
                    }
                }

                for (int row = 0; row < grdRackstair4.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair4.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout4[row][col];

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
                dr["MTRL_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["MATERIAL_CD"] = rackLayout.LotId;
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



        /// <summary>
        /// 4열 LAYOUT RACK 체크 : 선택된 RACK에 대한 상세 정보 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcRackLayout4_Checked(object sender, RoutedEventArgs e)
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
                for (int row = 0; row < grdRackstair4.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair4.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout4[row][col];

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
                for (int row = 0; row < grdRackstair2.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair2.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[row][col];

                        if (ucRackLayout.IsChecked)
                            ucRackLayout.IsChecked = false;
                    }
                }

                for (int row = 0; row < grdRackstair3.RowDefinitions.Count; row++)
                {
                    for (int col = 0; col < grdRackstair3.ColumnDefinitions.Count; col++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout3[row][col];

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
                dr["MTRL_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["STATUS"] = rackLayout.RackStateCode;
                dr["MATERIAL_CD"] = rackLayout.LotId;
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

                        string targetControl = ucRackLayout.LotId;

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

                        string targetControl = ucRackLayout.LotId;

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
                for (int r = 0; r < grdRackstair3.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair3.ColumnDefinitions.Count; c++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[r][c];

                        doubleAnimation.From = ucRackLayout.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        doubleAnimation.AutoReverse = true;

                        string targetControl = ucRackLayout.LotId;

                        if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            SetScrollToHorizontalOffset(scrollViewer3, c);
                            ucRackLayout.IsChecked = true;
                            CheckUcRackLayout(ucRackLayout);
                            ucRackLayout.BeginAnimation(HeightProperty, doubleAnimation);

                            return;
                        }
                    }
                }
                for (int r = 0; r < grdRackstair4.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grdRackstair4.ColumnDefinitions.Count; c++)
                    {
                        UcRackLayout ucRackLayout = _ucRackLayout2[r][c];

                        doubleAnimation.From = ucRackLayout.ActualHeight;
                        doubleAnimation.To = 0;
                        doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
                        doubleAnimation.AutoReverse = true;

                        string targetControl = ucRackLayout.LotId;

                        if (string.Equals(targetControl.ToUpper(), textBox.Text.ToUpper(), StringComparison.Ordinal))
                        {
                            SetScrollToHorizontalOffset(scrollViewer4, c);
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

            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "ATTR3", "ATTR5", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null, null, null, "Y", "AREA_EQUIPMENT_MTRL_GROUP" };
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

            const string bizRuleName = "DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.GetString(), stockerType };
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

        #region 창고적재현황 DATATABLE : MakeWareHouseCapacityTable()

        /// <summary>
        ///  합계 및 창고 적재율 계산위하여 생성
        /// </summary>
        private void MakeWareHouseCapacityTable()
        {
            _dtWareHouseCapacity = new DataTable();
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            _dtWareHouseCapacity.Columns.Add("ELTR_TYPE_NM", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPT_ID", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPT_NM", typeof(string));
            _dtWareHouseCapacity.Columns.Add("MAX_CAPA", typeof(decimal));      // 용량
            _dtWareHouseCapacity.Columns.Add("FULL_QTY", typeof(decimal));     // 실Carrier수
            _dtWareHouseCapacity.Columns.Add("EMPTY_QTY", typeof(decimal));     // 공Carrier수
            _dtWareHouseCapacity.Columns.Add("AB_RACK_QTY", typeof(decimal));     // 비정상RACK
            _dtWareHouseCapacity.Columns.Add("AB_CST_QTY", typeof(decimal));    // 정보불일치수
            _dtWareHouseCapacity.Columns.Add("LOAD_RATE", typeof(double));      // 적재율

        }


        #endregion

        #region 창고 적재 현황 조회 : SelectWareHouseCapacitySummary()

        /// <summary>
        /// 창고 적재 현황 조회
        /// </summary>
        private void SelectWareHouseCapacitySummary()
        {
            //const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LOAD_SUMMARY_MTRL";
            const string bizRuleName = "DA_INV_SEL_SUMMARY_WAREHOUSE_MTRL";
            try
            {
                DataTable dtResult = new DataTable();

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("WH_TYPE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = cboArea.SelectedValue;
                dr["WH_TYPE"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = cboElectrodeType.SelectedValue;
                dr["STK_ID"] = cboStocker.SelectedValue;

                inTable.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (bizResult.Rows.Count > 0)
                {
                    decimal RackMaxqty = 0;
                    decimal RealCarrierqty = 0;
                    decimal Emptyqty = 0;
                    decimal Errorqty = 0;
                    decimal Abnormalqty = 0;
                    decimal RackCount;
                    decimal RepCstidQty = 0;
                    decimal ProhibitQty = 0;

                    for (int i = 0; i < bizResult.Rows.Count; i++)
                    {
                        RackCount = 0;
                        RackCount = Convert.ToDecimal(bizResult.Rows[i]["FULL_QTY"].ToString()) + Convert.ToDecimal(bizResult.Rows[i]["EMPTY_QTY"].ToString()) + Convert.ToDecimal(bizResult.Rows[i]["AB_CST_QTY"].ToString()) + Convert.ToDecimal(bizResult.Rows[i]["AB_RACK_QTY"].ToString());
                        RackMaxqty = RackMaxqty + Convert.ToDecimal(bizResult.Rows[i]["MAX_CAPA"].ToString());
                        RealCarrierqty = RealCarrierqty + Convert.ToDecimal(bizResult.Rows[i]["FULL_QTY"].ToString());
                        Emptyqty = Emptyqty + Convert.ToDecimal(bizResult.Rows[i]["EMPTY_QTY"].ToString());
                        Errorqty = Errorqty + Convert.ToDecimal(bizResult.Rows[i]["AB_CST_QTY"].ToString());
                        Abnormalqty = Abnormalqty + Convert.ToDecimal(bizResult.Rows[i]["AB_RACK_QTY"].ToString());
                        ProhibitQty = ProhibitQty + Convert.ToDecimal(bizResult.Rows[i]["PROHIBIT_QTY"].ToString());
                        if (string.IsNullOrEmpty(bizResult.Rows[i]["LOAD_REP_CSTID_QTY"].ToString()))
                            bizResult.Rows[i]["LOAD_REP_CSTID_QTY"] = 0;

                        RepCstidQty += string.IsNullOrEmpty(bizResult.Rows[i]["LOAD_REP_CSTID_QTY"].ToString()) ? 0 : Convert.ToDecimal(bizResult.Rows[i]["LOAD_REP_CSTID_QTY"].ToString());

                        bizResult.Rows[i]["LOAD_RATE"] = GetRackRate(RackCount, Convert.ToDecimal(bizResult.Rows[i]["MAX_CAPA"].ToString())).ToString();
                    }
                    DataRow newRow = bizResult.NewRow();
                    newRow["EQPT_ID"] = "ZZZZZZZZZZ";
                    newRow["EQPT_NM"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["MAX_CAPA"] = RackMaxqty;
                    newRow["FULL_QTY"] = RealCarrierqty;
                    newRow["EMPTY_QTY"] = Emptyqty;
                    newRow["AB_RACK_QTY"] = Abnormalqty;
                    newRow["AB_CST_QTY"] = Errorqty;
                    newRow["LOAD_RATE"] = GetRackRate(RealCarrierqty + Emptyqty + Abnormalqty + Errorqty + ProhibitQty, RackMaxqty).ToString();
                    newRow["LOAD_REP_CSTID_QTY"] = RepCstidQty;
                    newRow["PROHIBIT_QTY"] = ProhibitQty;
                    bizResult.Rows.Add(newRow);

                    Util.GridSetData(dgCapacitySummary, bizResult, null, true);
                    HiddenLoadingIndicator();
                }
                HiddenLoadingIndicator();
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
            //bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_PER_PRODUCT_MTRL";
            bizRuleName = "DA_INV_SEL_SUMMARY_WAREHOUSE_GROUP_MTRL_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["FACILITY_CODE"] = cboArea.SelectedValue;
                dr["STK_ID"] = _selectedEquipmentCode;

                inTable.Rows.Add(dr);


                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        HiddenLoadingIndicator();
                        return;
                    }

                    decimal SumAvailQty = 0;
                    decimal SumHoldQty1 = 0;
                    decimal SumAvailQty2 = 0;
                    decimal SumHoldQty2 = 0;
                    decimal SumPltQty = 0;

                    for (int i = 0; i < bizResult.Rows.Count; i++)
                    {
                        SumAvailQty = SumAvailQty + Convert.ToDecimal(bizResult.Rows[i]["AVAIL_QTY_1"].ToString());
                        SumHoldQty1 = SumHoldQty1 + Convert.ToDecimal(bizResult.Rows[i]["HOLD_QTY_1"].ToString());
                        SumAvailQty2 = SumAvailQty2 + Convert.ToDecimal(bizResult.Rows[i]["AVAIL_QTY_2"].ToString());
                        SumHoldQty2 = SumHoldQty2 + Convert.ToDecimal(bizResult.Rows[i]["HOLD_QTY_2"].ToString());
                        SumPltQty = SumPltQty + Convert.ToDecimal(bizResult.Rows[i]["LOAD_REP_CSTID_QTY"].ToString());
                    }

                    //합계 행
                    DataRow newRow = bizResult.NewRow();
                    newRow["MTGRID"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["MATERIAL_CD"] = ObjectDic.Instance.GetObjectName("합계");
                    //newRow["GRADE"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["MATERIAL_NAME"] = ObjectDic.Instance.GetObjectName("합계");
                    newRow["AVAIL_QTY_1"] = SumAvailQty;
                    newRow["HOLD_QTY_1"] = SumHoldQty1;
                    newRow["AVAIL_QTY_2"] = SumAvailQty2;
                    newRow["HOLD_QTY_2"] = SumHoldQty2;
                    newRow["LOAD_REP_CSTID_QTY"] = SumPltQty;

                    bizResult.Rows.Add(newRow);

                    Util.GridSetData(dgProductSummary, bizResult, null, true);

                    _util.SetDataGridMergeExtensionCol(dgProductSummary, new string[] { "MTGRID", "MATERIAL_CD", "MATERIAL_NAME" }, DataGridMergeMode.HORIZONTAL);

                    HiddenLoadingIndicator();
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

            //bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST_MTRL";
            bizRuleName = "DA_INV_SEL_NORM_MTRL_LIST";

            try
            {
                ShowLoadingIndicator();

                if (dg.Name == "dgRackInfo")
                {

                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inTable.Columns.Add("WH_TYPE", typeof(string));
                    inTable.Columns.Add("STK_ID", typeof(string));
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("RACK_ID", typeof(string));
                    DataRow dr = inTable.NewRow();

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FACILITY_CODE"] = cboArea.SelectedValue;
                    dr["WH_TYPE"] = cboStockerType.SelectedValue;
                    dr["STK_ID"] = _selectedEquipmentCode;
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
                        if (bizResult.Rows.Count > 0)
                        {
                            for (int i = 0; i < bizResult.Rows.Count; i++)
                            {
                                bizResult.Rows[i]["ROWNUMBER"] = i + 1;
                            }
                        }
                        Util.GridSetData(dg, bizResult, null, true);
                    });
                }
                else
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("FACILITY_CODE", typeof(string));
                    inTable.Columns.Add("WH_TYPE", typeof(string));
                    inTable.Columns.Add("STK_ID", typeof(string));
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("MTRLID", typeof(string));
                    inTable.Columns.Add("HOLD_PLT_YN", typeof(string));

                    DataRow dr = inTable.NewRow();

                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FACILITY_CODE"] = cboArea.SelectedValue;
                    dr["WH_TYPE"] = cboStockerType.SelectedValue;
                    dr["STK_ID"] = _selectedEquipmentCode;
                    dr["ELTR_TYPE_CODE"] = _selectedStkElectrodeTypeCode;
                    dr["MTRLID"] = _selectMtGrID;
                    dr["HOLD_PLT_YN"] = _selectedWipHold;

                    inTable.Rows.Add(dr);

                    new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                    {
                        HiddenLoadingIndicator();
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        if (bizResult.Rows.Count > 0)
                        {
                            for (int i = 0; i < bizResult.Rows.Count; i++)
                            {
                                bizResult.Rows[i]["ROWNUMBER"] = i + 1;
                            }
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
            //bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST_MTRL";
            bizRuleName = "DA_INV_SEL_NORM_MTRL_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("WH_TYPE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = cboArea.SelectedValue;
                dr["WH_TYPE"] = cboStockerType.SelectedValue;
                dr["STK_ID"] = _selectedEquipmentCode;
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
                    if (bizResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            bizResult.Rows[i]["ROWNUMBER"] = i + 1;
                        }
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
            //bizRuleName = "DA_MHS_SEL_STO_INVENT_SUMMARY_EMPTY_CST_MTRL";
            bizRuleName = "DA_INV_SEL_SUMMARY_EMPTY_CST_MTRL_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));



                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = cboArea.SelectedValue;
                dr["STK_ID"] = _selectedEquipmentCode;

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
            //bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_EMPTY_CST_MTRL";
            bizRuleName = "DA_INV_SEL_EMPTY_CST_MTRL_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("WH_TYPE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = cboArea.SelectedValue;
                dr["WH_TYPE"] = cboStockerType.SelectedValue;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["STK_ID"] = _selectedEquipmentCode;
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

            bizRuleName = "DA_INV_SEL_ABNORM_RACK_LIST";
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_NOREAD_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = cboArea.SelectedValue;
                dr["STK_ID"] = _selectedEquipmentCode;
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
            //bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_ABNORM_CST_MTRL";
            bizRuleName = "DA_INV_SEL_ABNORM_CST_MTRL_LIST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FACILITY_CODE", typeof(string));
                inTable.Columns.Add("STK_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                dr["STK_ID"] = _selectedEquipmentCode;
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
            const string bizRuleName = "DA_INV_SEL_DISABLE_RACK_MTRL_LIST";

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

        #region 엑셀다운로드 Validation : ValidationExcel()

        private bool ValidationExcel()
        {
            if (dgProduct.Rows.Count == 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
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
        private double GetRackRate(decimal x, decimal y)
        {
            double rackRate = 0;
            if (y.Equals(0)) return rackRate;

            try
            {
                return Math.Round(x.GetDouble() / y.GetDouble() * 100, 3);
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
            _dtRackInfo.Columns.Add("MTRL_CSTID", typeof(string));
            _dtRackInfo.Columns.Add("MATERIAL_CD", typeof(string));
            _dtRackInfo.Columns.Add("PLLT_ID", typeof(string));
            _dtRackInfo.Columns.Add("STATUS", typeof(string));
            _dtRackInfo.Columns.Add("MTGRID", typeof(string));
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
            grdColumn3.Children.Clear();
            grdColumn4.Children.Clear();

            grdStair1.Children.Clear();
            grdStair2.Children.Clear();
            grdStair3.Children.Clear();
            grdStair4.Children.Clear();

            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();
            grdRackstair3.Children.Clear();
            grdRackstair4.Children.Clear();

            if (grdColumn1.ColumnDefinitions.Count > 0) grdColumn1.ColumnDefinitions.Clear();
            if (grdColumn1.RowDefinitions.Count > 0) grdColumn1.RowDefinitions.Clear();

            if (grdColumn2.ColumnDefinitions.Count > 0) grdColumn2.ColumnDefinitions.Clear();
            if (grdColumn2.RowDefinitions.Count > 0) grdColumn2.RowDefinitions.Clear();

            if (grdColumn3.ColumnDefinitions.Count > 0) grdColumn3.ColumnDefinitions.Clear();
            if (grdColumn3.RowDefinitions.Count > 0) grdColumn3.RowDefinitions.Clear();

            if (grdColumn4.ColumnDefinitions.Count > 0) grdColumn4.ColumnDefinitions.Clear();
            if (grdColumn4.RowDefinitions.Count > 0) grdColumn4.RowDefinitions.Clear();

            if (grdStair1.ColumnDefinitions.Count > 0) grdStair1.ColumnDefinitions.Clear();
            if (grdStair1.RowDefinitions.Count > 0) grdStair1.RowDefinitions.Clear();

            if (grdStair2.ColumnDefinitions.Count > 0) grdStair2.ColumnDefinitions.Clear();
            if (grdStair2.RowDefinitions.Count > 0) grdStair2.RowDefinitions.Clear();

            if (grdStair3.ColumnDefinitions.Count > 0) grdStair3.ColumnDefinitions.Clear();
            if (grdStair3.RowDefinitions.Count > 0) grdStair3.RowDefinitions.Clear();

            if (grdStair4.ColumnDefinitions.Count > 0) grdStair4.ColumnDefinitions.Clear();
            if (grdStair4.RowDefinitions.Count > 0) grdStair4.RowDefinitions.Clear();

            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();

            if (grdRackstair3.ColumnDefinitions.Count > 0) grdRackstair3.ColumnDefinitions.Clear();
            if (grdRackstair3.RowDefinitions.Count > 0) grdRackstair3.RowDefinitions.Clear();

            if (grdRackstair4.ColumnDefinitions.Count > 0) grdRackstair4.ColumnDefinitions.Clear();
            if (grdRackstair4.RowDefinitions.Count > 0) grdRackstair4.RowDefinitions.Clear();
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
                const string bizRuleName = "DA_INV_SEL_RACK_MAX_XYZ";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("STK_ID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["STK_ID"] = _selectedEquipmentCode;
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

                    if (Convert.ToInt32(searchResult.Rows[0][0].GetString()) == 1)
                    {
                        scrollViewer2.Visibility = Visibility.Collapsed;
                        scrollViewer3.Visibility = Visibility.Collapsed;
                        scrollViewer4.Visibility = Visibility.Collapsed;
                    }
                    else if (Convert.ToInt32(searchResult.Rows[0][0].GetString()) == 2)
                    {
                        scrollViewer2.Visibility = Visibility.Visible;
                        scrollViewer3.Visibility = Visibility.Collapsed;
                        scrollViewer4.Visibility = Visibility.Collapsed;
                    }
                    else if (Convert.ToInt32(searchResult.Rows[0][0].GetString()) == 3)
                    {
                        scrollViewer2.Visibility = Visibility.Visible;
                        scrollViewer3.Visibility = Visibility.Visible;
                        scrollViewer4.Visibility = Visibility.Collapsed;
                    }
                    else if (Convert.ToInt32(searchResult.Rows[0][0].GetString()) == 4)
                    {
                        scrollViewer2.Visibility = Visibility.Visible;
                        scrollViewer3.Visibility = Visibility.Visible;
                        scrollViewer4.Visibility = Visibility.Visible;
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

                const string bizRuleName = "DA_INV_SEL_WAREHOUSE_RACK_LAYOUT_MTRL";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("STK_ID", typeof(string));


                DataRow dr = inDataTable.NewRow();
                dr["STK_ID"] = _selectedEquipmentCode;

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

                        UcRackLayout ucRackLayout = null;
                        if (item["X_PSTN"].ToString() == "1")
                        {
                            ucRackLayout = _ucRackLayout1[x][y];
                        }
                        else if (item["X_PSTN"].ToString() == "2")
                        {
                            ucRackLayout = _ucRackLayout2[x][y];
                        }
                        else if (item["X_PSTN"].ToString() == "3")
                        {
                            ucRackLayout = _ucRackLayout3[x][y];
                        }
                        else if (item["X_PSTN"].ToString() == "4")
                        {
                            ucRackLayout = _ucRackLayout4[x][y];
                        }
                        if (ucRackLayout == null) continue;

                        ucRackLayout.RackId = item["RACK_ID"].GetString();
                        ucRackLayout.Row = int.Parse(item["Z_PSTN"].GetString());
                        ucRackLayout.Col = int.Parse(item["Y_PSTN"].GetString());
                        ucRackLayout.Stair = int.Parse(item["X_PSTN"].GetString());
                        ucRackLayout.RackStateCode = item["STATUS"].GetString();
                        ucRackLayout.LotId = item["MATERIAL_CD"].GetString();
                        ucRackLayout.BobbinCarrierCode = item["MTRL_CSTID"].GetString();
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
            grdStair3.Children.Clear();
            grdStair4.Children.Clear();

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

                ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
                grdStair3.ColumnDefinitions.Add(columnDefinition3);
                TextBlock textBlock3 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                textBlock3.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock3.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock3.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock3, colIndex);
                grdStair3.Children.Add(textBlock3);



                ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };
                grdStair4.ColumnDefinitions.Add(columnDefinition4);
                TextBlock textBlock4 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                textBlock4.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock4.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock4.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock4, colIndex);
                grdStair4.Children.Add(textBlock4);


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
            ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
            ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);
            grdColumn3.ColumnDefinitions.Add(columnDefinition3);
            grdColumn4.ColumnDefinitions.Add(columnDefinition4);

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(55) };
                grdColumn1.RowDefinitions.Add(rowDefinition1);

                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(55) };
                grdColumn2.RowDefinitions.Add(rowDefinition2);

                RowDefinition rowDefinition3 = new RowDefinition { Height = new GridLength(55) };
                grdColumn3.RowDefinitions.Add(rowDefinition3);

                RowDefinition rowDefinition4 = new RowDefinition { Height = new GridLength(55) };
                grdColumn4.RowDefinitions.Add(rowDefinition4);

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

                TextBlock textBlock3 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock3.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock3.SetValue(Grid.RowProperty, i);
                textBlock3.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock3.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn3.Children.Add(textBlock3);

                TextBlock textBlock4 = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                textBlock4.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock4.SetValue(Grid.RowProperty, i);
                textBlock4.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock4.Text = _maxRowCount - i + ObjectDic.Instance.GetObjectName("단");
                grdColumn4.Children.Add(textBlock4);


            }
        }

        /// <summary>
        /// RACK 디자인 LAYOUT 셋팅
        /// </summary>
        private void PrepareRackStairLayout()
        {
            grdRackstair1.Children.Clear();
            grdRackstair2.Children.Clear();
            grdRackstair3.Children.Clear();
            grdRackstair4.Children.Clear();
            BrushConverter converter = new BrushConverter();
            // 행/열 전체 삭제
            if (grdRackstair1.ColumnDefinitions.Count > 0) grdRackstair1.ColumnDefinitions.Clear();
            if (grdRackstair1.RowDefinitions.Count > 0) grdRackstair1.RowDefinitions.Clear();

            if (grdRackstair2.ColumnDefinitions.Count > 0) grdRackstair2.ColumnDefinitions.Clear();
            if (grdRackstair2.RowDefinitions.Count > 0) grdRackstair2.RowDefinitions.Clear();

            if (grdRackstair3.ColumnDefinitions.Count > 0) grdRackstair3.ColumnDefinitions.Clear();
            if (grdRackstair3.RowDefinitions.Count > 0) grdRackstair3.RowDefinitions.Clear();

            if (grdRackstair4.ColumnDefinitions.Count > 0) grdRackstair4.ColumnDefinitions.Clear();
            if (grdRackstair4.RowDefinitions.Count > 0) grdRackstair4.RowDefinitions.Clear();

            for (int i = 0; i < _maxColumnCount; i++)
            {
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition3 = new ColumnDefinition { Width = new GridLength(60) };
                ColumnDefinition columnDefinition4 = new ColumnDefinition { Width = new GridLength(60) };
                grdRackstair1.ColumnDefinitions.Add(columnDefinition1);
                grdRackstair2.ColumnDefinitions.Add(columnDefinition2);
                grdRackstair3.ColumnDefinitions.Add(columnDefinition3);
                grdRackstair4.ColumnDefinitions.Add(columnDefinition4);

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


                Border border2 = new Border();
                if (i == _maxColumnCount - 1)
                {
                    border2.SetValue(Grid.RowProperty, 0);
                    border2.SetValue(Grid.ColumnProperty, i);
                    border2.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border2.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border2.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border2.SetValue(Grid.RowProperty, 0);
                    border2.SetValue(Grid.ColumnProperty, i);
                    border2.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border2.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border2.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }

                grdRackstair3.Children.Add(border2);



                Border border3 = new Border();
                if (i == _maxColumnCount - 1)
                {
                    border3.SetValue(Grid.RowProperty, 0);
                    border3.SetValue(Grid.ColumnProperty, i);
                    border3.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border3.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 0));
                    border3.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border3.SetValue(Grid.RowProperty, 0);
                    border3.SetValue(Grid.ColumnProperty, i);
                    border3.SetValue(Grid.RowSpanProperty, _maxRowCount);
                    border3.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 0, 0));
                    border3.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }

                grdRackstair4.Children.Add(border3);


            }

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition3 = new RowDefinition { Height = new GridLength(60) };
                RowDefinition rowDefinition4 = new RowDefinition { Height = new GridLength(60) };
                grdRackstair1.RowDefinitions.Add(rowDefinition1);
                grdRackstair2.RowDefinitions.Add(rowDefinition2);
                grdRackstair3.RowDefinitions.Add(rowDefinition3);
                grdRackstair4.RowDefinitions.Add(rowDefinition4);

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


                Border border2 = new Border();
                if (i == _maxRowCount - 1)
                {
                    border2.SetValue(Grid.RowProperty, i);
                    border2.SetValue(Grid.ColumnProperty, 0);
                    border2.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border2.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border2.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border2.SetValue(Grid.RowProperty, i);
                    border2.SetValue(Grid.ColumnProperty, 0);
                    border2.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border2.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border2.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair3.Children.Add(border2);

                Border border3 = new Border();
                if (i == _maxRowCount - 1)
                {
                    border3.SetValue(Grid.RowProperty, i);
                    border3.SetValue(Grid.ColumnProperty, 0);
                    border3.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border3.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 1));
                    border3.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                else
                {
                    border3.SetValue(Grid.RowProperty, i);
                    border3.SetValue(Grid.ColumnProperty, 0);
                    border3.SetValue(Grid.ColumnSpanProperty, _maxColumnCount);
                    border3.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
                    border3.BorderBrush = converter.ConvertFromString("#d8d6d7") as Brush;
                }
                grdRackstair4.Children.Add(border3);


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

                    Grid.SetRow(_ucRackLayout3[row][col], row);
                    Grid.SetColumn(_ucRackLayout3[row][col], col);
                    grdRackstair3.Children.Add(_ucRackLayout3[row][col]);

                    Grid.SetRow(_ucRackLayout4[row][col], row);
                    Grid.SetColumn(_ucRackLayout4[row][col], col);
                    grdRackstair4.Children.Add(_ucRackLayout4[row][col]);


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
            _ucRackLayout3 = new UcRackLayout[_maxRowCount][];
            _ucRackLayout4 = new UcRackLayout[_maxRowCount][];

            for (int r = 0; r < _ucRackLayout1.Length; r++)
            {
                _ucRackLayout1[r] = new UcRackLayout[_maxColumnCount];
                _ucRackLayout2[r] = new UcRackLayout[_maxColumnCount];
                _ucRackLayout3[r] = new UcRackLayout[_maxColumnCount];
                _ucRackLayout4[r] = new UcRackLayout[_maxColumnCount];

                for (int c = 0; c < _ucRackLayout1[r].Length; c++)
                {
                    UcRackLayout ucRackLayout1 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout1.Checked += UcRackLayout1_Checked;
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
                    _ucRackLayout2[r][c] = ucRackLayout2;
                }

                for (int c = 0; c < _ucRackLayout3[r].Length; c++)
                {
                    UcRackLayout ucRackLayout3 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout3.Checked += UcRackLayout3_Checked;
                    _ucRackLayout3[r][c] = ucRackLayout3;
                }

                for (int c = 0; c < _ucRackLayout4[r].Length; c++)
                {
                    UcRackLayout ucRackLayout4 = new UcRackLayout
                    {
                        Name = $"r{r:0}c{c:00}",
                        ProjectName = string.Empty,
                    };

                    ucRackLayout4.Checked += UcRackLayout4_Checked;
                    _ucRackLayout4[r][c] = ucRackLayout4;
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
                        Name = "ROWNUMBER",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROWNUMBER"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_NM",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NM",
                        Header = ObjectDic.Instance.GetObjectName("PLC_ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RCK_DATE",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("RCK_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MLOT_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("MLOT_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ABNORM_STAT_NM",
                        Header = ObjectDic.Instance.GetObjectName("오류유형"),
                        Binding = new Binding() { Path = new PropertyPath("ABNORM_STAT_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.IsReadOnly = true;
                    break;


                case "3":

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ROWNUMBER",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROWNUMBER"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_NM",
                        Header = ObjectDic.Instance.GetObjectName("창고"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NM",
                        Header = ObjectDic.Instance.GetObjectName("PLC_ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });


                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CST_TYPE_NAME",
                        Header = ObjectDic.Instance.GetObjectName("오류유형"),
                        Binding = new Binding() { Path = new PropertyPath("ABNORM_STAT_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.IsReadOnly = true;
                    break;

                case "4":
                case "5":

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ROWNUMBER",
                        Header = ObjectDic.Instance.GetObjectName("NO"),
                        Binding = new Binding() { Path = new PropertyPath("ROWNUMBER"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_NM",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NM",
                        Header = ObjectDic.Instance.GetObjectName("PLC_ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RCK_DATE",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("RCK_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "CST_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CST_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    //_util.SetDataGridMergeExtensionCol(dgRackInfo, new string[] { "ELTR_TYPE_NAME"}, DataGridMergeMode.VERTICALHIERARCHI);
                    dgRackInfo.IsReadOnly = true;
                    break;


                case "2":
                    dgRackInfo.Columns.Add(new DataGridNumericColumn()
                    {
                        Name = "ROWNUMBER",
                        Header = ObjectDic.Instance.GetObjectName("순위"),
                        Binding = new Binding() { Path = new PropertyPath("ROWNUMBER"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQPT_NM",
                        Header = ObjectDic.Instance.GetObjectName("STOCKER"),
                        Binding = new Binding() { Path = new PropertyPath("EQPT_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RACK_NM",
                        Header = ObjectDic.Instance.GetObjectName("PLC_ID"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "RCK_DATE",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("RCK_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PAST_DAY",
                        Header = ObjectDic.Instance.GetObjectName("PAST_DAY"),
                        Binding = new Binding() { Path = new PropertyPath("PAST_DAY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MLOT_ID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("PLLT_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PLLT_ID",
                        Header = ObjectDic.Instance.GetObjectName("PLT_ID"),
                        Binding = new Binding() { Path = new PropertyPath("PLLT_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PALLET_SERIAL_NO",
                        Header = ObjectDic.Instance.GetObjectName("PLT_SEQ"),
                        Binding = new Binding() { Path = new PropertyPath("PALLET_SERIAL_NO"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SUPPLIER_LOTID",
                        Header = ObjectDic.Instance.GetObjectName("자재LOTID"),
                        Binding = new Binding() { Path = new PropertyPath("SUPPLIER_LOTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MATERIAL_GROUP_NAME",
                        Header = ObjectDic.Instance.GetObjectName("자재그룹"),
                        Binding = new Binding() { Path = new PropertyPath("MATERIAL_GROUP_NAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MATERIAL_NM",
                        Header = ObjectDic.Instance.GetObjectName("자재코드"),
                        Binding = new Binding() { Path = new PropertyPath("MATERIAL_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MATERIAL_QTY",
                        Header = ObjectDic.Instance.GetObjectName("수량"),
                        Binding = new Binding() { Path = new PropertyPath("MATERIAL_QTY"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap,
                        Format = "#,###.000",
                        HorizontalAlignment = HorizontalAlignment.Right
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MATERIAL_UNIT",
                        Header = ObjectDic.Instance.GetObjectName("단위"),
                        Binding = new Binding() { Path = new PropertyPath("MATERIAL_UNIT"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "IQC_STATE_NM",
                        Header = ObjectDic.Instance.GetObjectName("IQC 상태"),
                        Binding = new Binding() { Path = new PropertyPath("IQC_STATE_NM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "IQC_NOTE",
                        Header = ObjectDic.Instance.GetObjectName("IQC 비고"),
                        Binding = new Binding() { Path = new PropertyPath("IQC_NOTE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "HOLD_YN",
                        Header = ObjectDic.Instance.GetObjectName("HOLD 여부"),
                        Binding = new Binding() { Path = new PropertyPath("HOLD_YN"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "NOTE",
                        Header = ObjectDic.Instance.GetObjectName("HOLD 사유"),
                        Binding = new Binding() { Path = new PropertyPath("NOTE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });
                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EXPIRY_DATE",
                        Header = ObjectDic.Instance.GetObjectName("유효일자"),
                        Binding = new Binding() { Path = new PropertyPath("EXPIRY_DATE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });



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
                case "4":   // 금지단
                    SelectProHibitList(dgRackInfo);
                    break;
                case "5":
                    SelectWareHouseEmptyCarrierList(dgRackInfo);
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

            for (int row = 0; row < grdRackstair3.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair3.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout3[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout3[row][col].Clear();
                }
            }

            for (int row = 0; row < grdRackstair4.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < grdRackstair4.ColumnDefinitions.Count; col++)
                {
                    _ucRackLayout4[row][col].Visibility = Visibility.Hidden;
                    _ucRackLayout4[row][col].Clear();
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

            for (int rowIndex = 0; rowIndex < grdRackstair3.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair3.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout2[rowIndex][colIndex];

                    if (ucRackLayout.IsChecked)
                    {
                        ucRackLayout.IsChecked = false;
                        UnCheckUcRackLayout(ucRackLayout);
                    }
                }
            }

            for (int rowIndex = 0; rowIndex < grdRackstair4.RowDefinitions.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < grdRackstair4.ColumnDefinitions.Count; colIndex++)
                {
                    UcRackLayout ucRackLayout = _ucRackLayout4[rowIndex][colIndex];

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
            dr["MATERIAL_CD"] = rackLayout.LotId;
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
