/*************************************************************************************
 Created Date : 2024.09.10
      Creator : 오화백
   Decription : 랙관리(OWMS)
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.10  오화백 과장 : Initial Created.    
  
**************************************************************************************/
using System;
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



namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_217 : UserControl, IWorkArea
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
        public MTRL001_217()
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
            Loaded -= UserControl_Loaded;

            _isLoaded = true;
        }
        /// <summary>
        /// 화면 언로드시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;

        }
        #endregion

        #region Event

        #region 창고유형 콤보 이벤트 : cboStockerType_SelectedValueChanged()
        /// <summary>
        /// 창고 유형  콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void cboStockerTypeT2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetStockerCombo(cboStockerT2);
        }

        private void cboStockerTypeT1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetStockerCombo(cboStockerT1);
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
                    if (string.Equals(e.Cell.Column.Name, "EQPTNAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")), "XXXXXXXXXX"))
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

                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTNAME")), ObjectDic.Instance.GetObjectName("합계")))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("Yellow");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
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

                _selectedLotIdByRackInfo = null;
                _selectedSkIdIdByRackInfo = null;
                _selectedBobbinIdByRackInfo = null;
                _selectedRackIdByRackInfo = null;

                if (cell.Column.Name.Equals("EQPTNAME")) // 창고정보 클릭시
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

                    if (cell.Column.Name.Equals("EQPTNAME"))
                    {
                        _selectedWipHold = null;
                        _selectedProjectName = null;

                        //레이아웃 탭 선택 및  창고정보 클릭시
                        if ((cell.Column.Name.Equals("EQPTNAME") || cell.Text.Equals(ObjectDic.Instance.GetObjectName("합계"))))
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


                    }


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
            txtMtrlLotIDT2.Text = string.Empty;
            txtPalletIdT2.Text = string.Empty;

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
            txtMtrlLotIDT2.Text = string.Empty;
            txtPalletIdT2.Text = string.Empty;

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
                txtPalletIdT2.Text = string.Empty;
            }
            else
            {
                txtMtrlLotIDT2.Text = string.Empty;
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
                //C1DataGrid dataGrid = dgCapacitySummary;
                //double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);

                //if (ContentsRow.ColumnDefinitions[0].Width.Value > sumWidth)
                //{
                //    ContentsRow.ColumnDefinitions[0].Width = new GridLength(sumWidth + splitter.ActualWidth);
                //}

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
            _selectedRackIdByRackInfo = string.Empty;
            _selectedProjectName = string.Empty;
            _selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _selectedLotIdByRackInfo = string.Empty;
            _selectedSkIdIdByRackInfo = string.Empty;
            _selectedBobbinIdByRackInfo = string.Empty;

            txtMtrlLotIDT2.Text = string.Empty;
            txtPalletIdT2.Text = string.Empty;

            _dtWareHouseCapacity?.Clear();

            Util.gridClear(dgCapacitySummary);


            Util.gridClear(dgRackInfo);
            InitializeRackUserControl();
        }

        #endregion

        #region  콤보박스 셋팅 : InitializeCombo()
        private void InitializeCombo()
        {
            // 창고유형 콤보박스
            SetStockerTypeCombo(cboStockerTypeT1);
            SetStockerTypeCombo(cboStockerTypeT2);

            // Stocker 콤보박스
            SetStockerCombo(cboStockerT1);
            SetStockerCombo(cboStockerT2);



        }
        #endregion

        #region 창고유형 콤보박스 조회 : SetStockerTypeCombo()

        /// <summary>
        /// 창고 유형 콤보박스 조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetStockerTypeCombo(C1ComboBox cbo)
        {

            const string bizRuleName = "DA_MHS_SEL_AREA_COM_CODE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "ATTR1", "ATTR2", "COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null, null, "AREA_EQUIPMENT_MTRL_GROUP" };
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
            string stockerType = string.Empty;

            if (tiList.IsSelected == true)
            {
                stockerType = string.IsNullOrEmpty(cboStockerTypeT1.SelectedValue.GetString()) ? null : cboStockerTypeT1.SelectedValue.GetString();
            }
            else
            {
                stockerType = string.IsNullOrEmpty(cboStockerTypeT2.SelectedValue.GetString()) ? null : cboStockerTypeT2.SelectedValue.GetString();
            }
            const string bizRuleName = "DA_INV_SEL_EQUIPMENT_ELTRTYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQGRID", };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, stockerType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        #endregion

        #region 창고적재현황 DATATABLE : MakeWareHouseCapacityTable()

        /// <summary>
        ///  합계 및 창고 적재율 계산위하여 생성
        /// </summary>
        private void MakeWareHouseCapacityTable()
        {
            _dtWareHouseCapacity = new DataTable();
            _dtWareHouseCapacity.Columns.Add("EQPTID", typeof(string));
            _dtWareHouseCapacity.Columns.Add("EQPTNAME", typeof(string));
            _dtWareHouseCapacity.Columns.Add("RACK_MAX", typeof(decimal));      // 용량
            _dtWareHouseCapacity.Columns.Add("BBN_U_QTY", typeof(decimal));     // 실Carrier수
            _dtWareHouseCapacity.Columns.Add("BBN_E_QTY", typeof(decimal));     // 공Carrier수
            _dtWareHouseCapacity.Columns.Add("ERROR_QTY", typeof(decimal));     // 비정상RACK
            _dtWareHouseCapacity.Columns.Add("ABNORM_QTY", typeof(decimal));    // 정보불일치수
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
            const string bizRuleName = "DA_INV_SEL_STO_INVENT_LOAD_SUMMARY_MTRL";
            try
            {
                DataTable dtResult = new DataTable();

                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboStockerT2.GetBindValue();
                dr["EQGRID"] = cboStockerTypeT2.GetBindValue();
                inTable.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (bizResult.Rows.Count > 0)
                {
                    //// 합계
                    //var querySum = bizResult.AsEnumerable().GroupBy(x => new
                    //{ }).Select(g => new
                    //{
                    //    RackMax = g.Sum(x => x.Field<Int64>("RACK_MAX")),
                    //    RealCarrierCount = g.Sum(x => x.Field<Int64>("BBN_U_QTY")),
                    //    EmptyCarrierCount = g.Sum(x => x.Field<Int64>("BBN_E_QTY")),
                    //    ErrorCarrierCount = g.Sum(x => x.Field<Int64>("ERROR_QTY")),
                    //    AbnormalCount = g.Sum(x => x.Field<Int64>("ABNORM_QTY")),
                    //    SumCarrierCount = g.Sum(x => x.Field<Int64>("RACK_QTY")),
                    //    EquipmentCode = "ZZZZZZZZZZ",
                    //    EquipmentName = ObjectDic.Instance.GetObjectName("합계"),
                    //    Count = g.Count()
                    //}).ToList();

                    //if (CommonVerify.HasTableRow(bizResult))
                    //{
                    //    for (int i = 0; i < bizResult.Rows.Count; i++)
                    //    {
                    //        DataRow newRow = _dtWareHouseCapacity.NewRow();
                    //        newRow["EQPTID"] = bizResult.Rows[i]["EQPTID"];
                    //        newRow["EQPTNAME"] = bizResult.Rows[i]["EQPTNAME"];
                    //        newRow["RACK_MAX"] = bizResult.Rows[i]["RACK_MAX"];
                    //        newRow["BBN_U_QTY"] = bizResult.Rows[i]["BBN_U_QTY"];
                    //        newRow["BBN_E_QTY"] = bizResult.Rows[i]["BBN_E_QTY"];
                    //        newRow["ERROR_QTY"] = bizResult.Rows[i]["ERROR_QTY"];
                    //        newRow["ABNORM_QTY"] = bizResult.Rows[i]["ABNORM_QTY"];
                    //        newRow["RACK_RATE"] = bizResult.Rows[i]["RACK_RATE"];
                    //        newRow["RACK_QTY"] = bizResult.Rows[i]["RACK_QTY"];
                    //        _dtWareHouseCapacity.Rows.Add(newRow);
                    //    }

                    //    if (querySum.Any())
                    //    {
                    //        foreach (var item in querySum)
                    //        {
                    //            DataRow newRow = _dtWareHouseCapacity.NewRow();
                    //            newRow["EQPTID"] = item.EquipmentCode;
                    //            newRow["EQPTNAME"] = item.EquipmentName;
                    //            newRow["RACK_MAX"] = item.RackMax;
                    //            newRow["BBN_U_QTY"] = item.RealCarrierCount;
                    //            newRow["BBN_E_QTY"] = item.EmptyCarrierCount;
                    //            newRow["ERROR_QTY"] = item.ErrorCarrierCount;
                    //            newRow["ABNORM_QTY"] = item.AbnormalCount;
                    //            newRow["RACK_QTY"] = item.SumCarrierCount;
                    //            _dtWareHouseCapacity.Rows.Add(newRow);
                    //        }
                    //    }
                    //}

                    //if (CommonVerify.HasTableRow(_dtWareHouseCapacity))
                    //{
                    //    dtResult = _dtWareHouseCapacity;
                    //}
                    //else
                    //{
                    //    dtResult = bizResult;
                    //}


                    //합계 추가 2025.02.14
                    ////
                    if (bizResult.Rows.Count > 0)
                    {
                        /* 
                        EQPTID, 
                        EQPTNAME, 
                        */
                        decimal RACK_MAX = 0;
                        decimal AVAILABLE_QTY = 0;  //출하가능RACK
                        decimal UNABLE_QTY = 0;      //출하불가능RACK                        
                        decimal RACK_QTY = 0;       //RACK 수량
                        decimal ERROR_QTY = 0;       //비정상RACK
                        decimal RACK_RATE = 0;      //가용율
                                                    // 1 * RACK_QTY / RACK_MAX * 100 AS RACK_RATE

                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {

                            AVAILABLE_QTY = AVAILABLE_QTY + Convert.ToDecimal(bizResult.Rows[i]["AVAILABLE_QTY"].ToString());
                            UNABLE_QTY = UNABLE_QTY + Convert.ToDecimal(bizResult.Rows[i]["UNABLE_QTY"].ToString());
                            RACK_MAX = RACK_MAX + Convert.ToDecimal(bizResult.Rows[i]["RACK_MAX"].ToString());
                            ERROR_QTY = ERROR_QTY + Convert.ToDecimal(bizResult.Rows[i]["ERROR_QTY"].ToString());


                        }

                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTNAME"] = ObjectDic.Instance.GetObjectName("합계");
                        newRow["AVAILABLE_QTY"] = AVAILABLE_QTY;
                        newRow["UNABLE_QTY"] = UNABLE_QTY;
                        newRow["RACK_MAX"] = RACK_MAX;
                        newRow["ERROR_QTY"] = ERROR_QTY;
                        newRow["RACK_RATE"] = GetRackRate(AVAILABLE_QTY + UNABLE_QTY, RACK_MAX).ToString();
                        bizResult.Rows.Add(newRow);

                        Util.GridSetData(dgCapacitySummary, bizResult, null, true);
                        HiddenLoadingIndicator();
                    }
                    ////

                    Util.GridSetData(dgCapacitySummary, bizResult, null, true);
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

        #region 자재창고 현황 리스트 초기화 : InitializeGrid()
        /// <summary>
        /// 자재창고현황 리스트 초기화
        /// </summary>
        private void InitializeGrid()
        {
            dgList.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
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
            finally
            {
                HiddenLoadingIndicator();
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

                const string bizRuleName = "DA_INV_SEL_WAREHOUSE_RACK_LAYOUT";
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

                        UcRackLayout ucRackLayout = item["X_PSTN"].ToString() == "1" ? _ucRackLayout1[x][y] : _ucRackLayout2[x][y];

                        if (ucRackLayout == null) continue;

                        ucRackLayout.RackId = item["RACK_ID"].GetString();
                        ucRackLayout.Row = int.Parse(item["Z_PSTN"].GetString());
                        ucRackLayout.Col = int.Parse(item["Y_PSTN"].GetString());
                        ucRackLayout.Stair = int.Parse(item["X_PSTN"].GetString());
                        //ucRackLayout.RackStateCode = item["STATUS"].GetString();
                        //ucRackLayout.ProjectName = item["PRJT_NAME"].GetString();
                        ucRackLayout.LotId = item["LOTID"].GetString();
                        //ucRackLayout.SkidCarrierProductCode = item["SD_CSTPROD"].GetString();
                        //ucRackLayout.SkidCarrierProductName = item["SD_CSTPROD_NAME"].GetString();
                        //ucRackLayout.SkidCarrierCode = item["SD_CSTID"].GetString();
                        //ucRackLayout.BobbinCarrierProductCode = item["BB_CSTPROD"].GetString();
                        //ucRackLayout.BobbinCarrierProductName = item["BB_CSTPROD_NAME"].GetString();
                        ucRackLayout.BobbinCarrierCode = item["DURABLE_ID"].GetString();
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
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(45.5) }; //60
                grdStair1.ColumnDefinitions.Add(columnDefinition1);
                TextBlock textBlock1 = new TextBlock() { FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                textBlock1.SetBinding(TextBlock.TextProperty, new Binding() { Converter = new ObjectDicConverter(), Mode = BindingMode.OneWay });
                textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock1.Text = i + 1 + ObjectDic.Instance.GetObjectName("연");
                Grid.SetColumn(textBlock1, colIndex);
                grdStair1.Children.Add(textBlock1);

                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(45.5) }; //60
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
            ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(45.5) }; //60
            ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(45.5) }; //60

            grdColumn1.ColumnDefinitions.Add(columnDefinition1);
            grdColumn2.ColumnDefinitions.Add(columnDefinition2);

            for (int i = 0; i < _maxRowCount; i++)
            {
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(25) }; //55
                grdColumn1.RowDefinitions.Add(rowDefinition1);

                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(25) }; //55
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
                ColumnDefinition columnDefinition1 = new ColumnDefinition { Width = new GridLength(45.5) }; //60
                ColumnDefinition columnDefinition2 = new ColumnDefinition { Width = new GridLength(45.5) }; //60
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
                RowDefinition rowDefinition1 = new RowDefinition { Height = new GridLength(25) }; //60
                RowDefinition rowDefinition2 = new RowDefinition { Height = new GridLength(25) }; //60
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
                default:
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
                        Header = ObjectDic.Instance.GetObjectName("RACK"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_ID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MODLID",
                        Header = ObjectDic.Instance.GetObjectName("모델"),
                        Binding = new Binding() { Path = new PropertyPath("MODLID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "MATERIAL_CD",
                        Header = ObjectDic.Instance.GetObjectName("자재코드"),
                        Binding = new Binding() { Path = new PropertyPath("MATERIAL_CD"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PALLETID",
                        Header = ObjectDic.Instance.GetObjectName("PALLETID"),
                        Binding = new Binding() { Path = new PropertyPath("PALLETID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "PROD_LOTID",
                        Header = ObjectDic.Instance.GetObjectName("PROD LOTID"),
                        Binding = new Binding() { Path = new PropertyPath("PROD_LOTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "STAT_CODE",
                        Header = ObjectDic.Instance.GetObjectName("재고상태"),
                        Binding = new Binding() { Path = new PropertyPath("STAT_CODE"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "ERP_LOC",
                        Header = ObjectDic.Instance.GetObjectName("ERP 저장위치"),
                        Binding = new Binding() { Path = new PropertyPath("ERP_LOC"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

                    dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "STK_RCV_DTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("STK_RCV_DTTM"), Mode = BindingMode.OneWay },
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
            SelectRackInfo(dgRackInfo);

            //switch (type)
            //{
            //    case "1":   // 정보불일치
            //        SelectAbNormalCarrierList(dgRackInfo);
            //        break;
            //    case "2":   // 실보빈(LOT존재)
            //        SelectWareHouseProductList(dgRackInfo);
            //        break;
            //    case "3":   // 오류Carrier
            //        SelectErrorCarrierList(dgRackInfo);
            //        break;
            //    case "4":   // 공 Carrier
            //    case "5":
            //        SelectWareHouseEmptyCarrierList(dgRackInfo);
            //        break;
            //}
        }

        private void SelectRackInfo(C1DataGrid dg)
        {
            string bizRuleName = string.Empty;
            bizRuleName = "DA_INV_SEL_RACK_INFO";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQUIPMENT_ID"] = _selectedEquipmentCode;
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

                    // 행번호
                    if (!bizResult.Columns.Contains("ROW_NUM")) bizResult.Columns.Add("ROW_NUM", typeof(Int64));
                    int rowNum = 0;
                    bizResult.AsEnumerable().ToList().ForEach(r => r["ROW_NUM"] = ++rowNum);

                    Util.GridSetData(dg, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
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

        private void btnSearchT1_Click(object sender, RoutedEventArgs e)
        {
            PM_MtrlList();
        }

        private void PM_MtrlList()
        {
            const string bizRuleName = "DA_INV_SEL_MANUAL_SHIP_RACK";
            try
            {
                dgList.ClearRows();

                DataTable dtResult = new DataTable();

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQUIPMENT_ID", typeof(string));
                inTable.Columns.Add("PALLET_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQUIPMENT_ID"] = cboStockerT1.GetBindValue();
                dr["PALLET_ID"] = txtPalletIDT1.GetBindValue();
                inTable.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                if (bizResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, bizResult, null, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
