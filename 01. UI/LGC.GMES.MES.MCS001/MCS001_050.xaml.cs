/*************************************************************************************
 Created Date : 2020.10.27
      Creator : 서동현
   Decription : 활성화 STK 재공현황 LOT
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.27  서동현 책임 : Initial Created.    
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
    /// MCS001_050.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_050 : UserControl, IWorkArea
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
		private string _selectedLotResonCode;
		private string _selectedStkElectrodeTypeCode;
        private string _selectedProjectName;
		private string _selectedProdId;
		private string _selectedWipHold;
        private string _selectedQmsHold;
        private string _selectedLotIdByRackInfo;
        private string _selectedSkIdIdByRackInfo;
        private string _selectedBobbinIdByRackInfo;
        private DataTable _dtWareHouseCapacity;
		private DataTable _requestMcsRackRate;

		private string _bizRuleIp;
		private string _bizRuleProtocol;
		private string _bizRulePort;
		private string _bizRuleServiceMode;
		private string _bizRuleServiceIndex;

		private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;
        private bool _isGradeJudgmentDisplay;
        private int _maxRowCount;
        private int _maxColumnCount;

        private DataTable _dtRackInfo;
        private UcRackLayout[][] _ucRackLayout1;
        private UcRackLayout[][] _ucRackLayout2;

        private enum SearchType
        {
            Tab,
            MultiSelectionBox
        }

        public MCS001_050()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
			//GetBizActorServerInfo(); // 2024.10.21. 김영국 - MSC에서 사용하는 DA_MCS_SEL_MCS_CONFIG_INFO 부분 주석 처리
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
            Loaded -= UserControl_Loaded;
            C1TabControl.SelectionChanged += C1TabControl_SelectionChanged;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgRealPallet.Viewport.HorizontalOffset;
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
			ClearControl();

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

			SelectWareHouseSummary();
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

                _selectedStkElectrodeTypeCode = null;
                _selectedLotElectrodeTypeCode = null;
				_selectedLotResonCode = null;
				_selectedEquipmentCode = null;
                _selectedProjectName = null;
				_selectedProdId = null;
				_selectedWipHold = null;
                _selectedQmsHold = null;

                _selectedLotIdByRackInfo = null;
                _selectedSkIdIdByRackInfo = null;
                _selectedBobbinIdByRackInfo = null;

                if (string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "XXXXXXXXXX") ||
                    string.Equals(DataTableConverter.GetValue(drv, "EQPTID").GetString(), "ZZZZZZZZZZ"))
                {
					//_selectedEquipmentCode = null;
					_selectedEquipmentCode = string.IsNullOrEmpty(cboStocker.SelectedValue.ToString()) ? null : cboStocker.SelectedValue.ToString();
					_selectedProjectName = null;
					_selectedProdId = null;

				}
                else
                {
                    _selectedEquipmentCode = DataTableConverter.GetValue(drv, "EQPTID").GetString();
                    _selectedProjectName = DataTableConverter.GetValue(drv, "PRJT_NAME").GetString();
					_selectedProdId = DataTableConverter.GetValue(drv, "PRODID").GetString();
				}

                if (cell.Column.Name.Equals("EMPTY_QTY"))
                {
					tabEmptyPallet.IsSelected = true;
                    SelectWareHouseEmptyPallet();
                    SelectWareHouseEmptyPalletList(dgEmptyPalletList);
                }
                else if (cell.Column.Name.Equals("ERROR_QTY"))
                {
					tabErrorPallet.IsSelected = true;
                    SelectErrorPalletList(dgErrorPallet);
                }
                else if (cell.Column.Name.Equals("ABNORM_QTY"))
                {
					tabAbNormalPallet.IsSelected = true;
                    SelectAbNormalPalletList(dgAbNormalPallet);
                }
                else
                {
                    if (cell.Column.Name.Equals("EQPTNAME") || cell.Column.Name.Equals("RACK_MAX_QTY") || cell.Column.Name.Equals("RACK_RATE3"))
                    {
                        _selectedProjectName = null;
						_selectedProdId = null;
					}
                    else if (cell.Column.Name.Equals("SHIP_PLT_QTY"))
                    {
						_selectedLotResonCode = "OK";
						//_selectedLotElectrodeTypeCode = "C";
						//_selectedWipHold = "N";
					}
					else if (cell.Column.Name.Equals("HOLD_PLT_QTY"))
                    {
						_selectedLotResonCode = "HOLD";
						//_selectedLotElectrodeTypeCode = "C";
						//_selectedWipHold = "Y";
					}
					else if (cell.Column.Name.Equals("INSP_WAIT_PLT_QTY"))
                    {
						_selectedLotResonCode = "INSP_WAIT";
						//_selectedLotElectrodeTypeCode = "A";
						//_selectedWipHold = "N";
					}
					else if (cell.Column.Name.Equals("NG_PLT_QTY"))
                    {
						_selectedLotResonCode = "NG";
						//_selectedLotElectrodeTypeCode = "A";
						//_selectedWipHold = "Y";
					}
					else if (cell.Column.Name.Equals("VLD_DATE_PLT_QTY"))
					{
						_selectedLotResonCode = "VLD_DATE";
						//_selectedLotElectrodeTypeCode = "A";
						//_selectedWipHold = "Y";
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
                        Util.gridClear(dgRealPallet);
                        tabProduct.IsSelected = true;
                        SelectWareHouseProductList(dgRealPallet);
                    }
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
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
									e.Merge(new DataGridCellsRange(dg.GetCell(i, dg.Columns["RACK_RATE3"].Index), dg.GetCell(item.Count - 1 + rowIndex, dg.Columns["RACK_RATE3"].Index)));
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

        private void dgRealPallet_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow != null && dg.CurrentColumn.Name.Equals("BOXID"))
                {
                    //object[] parameters = new object[1];
                    //parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "BOXID"));
                    //this.FrameOperation.OpenMenu("SFU010160050", true, parameters);

					string sAREAID = LoginInfo.CFG_AREA_ID;
					string sSHOPID = LoginInfo.CFG_SHOP_ID;
					string sPalletid = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "BOXID"));

					loadingIndicator.Visibility = Visibility.Visible;
					string[] sParam = { sAREAID, sSHOPID, sPalletid };
					// 기간별 Pallet 확정 이력 정보 조회
					this.FrameOperation.OpenMenu("SFU010060100", true, sParam);
					loadingIndicator.Visibility = Visibility.Collapsed;
				}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRealPallet_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
					if (Convert.ToString(e.Cell.Column.Name) == "SHIPPING_YN")
					{
						if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHIPPING_YN").ToString() == "N")
						{
							e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
						}
						else
							e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
					}
					else if (Convert.ToString(e.Cell.Column.Name) == "BOXID")
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

        private void dgRealPallet_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                if (CommonVerify.HasDataGridRow(dgRealPallet))
                {
                    _isscrollToHorizontalOffset = true;
                    _scrollToHorizontalOffset = dgRealPallet.Viewport.HorizontalOffset;
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

        private void dgRealPallet_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
			txtCstId.Text = string.Empty;

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
                dr["CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["CSTID"] = rackLayout.SkidCarrierCode;
                //dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                //dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                //dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
                dr["WIPHOLD"] = rackLayout.WipHold;
                dr["CST_DFCT_FLAG"] = rackLayout.CarrierDefectFlag;
                dr["SKID_GUBUN"] = rackLayout.SkidType;
                dr["COLOR_GUBUN"] = rackLayout.LegendColorType;
                dr["COLOR"] = rackLayout.LegendColor;
                dr["ABNORM_TRF_RSN_CODE"] = rackLayout.AbnormalTransferReasonCode;
                dr["HOLD_FLAG"] = rackLayout.HoldFlag;
                dr["SEQ"] = maxSeq;
                _dtRackInfo.Rows.Add(dr);

                //if (rackLayout.LegendColorType == "4")
                //{
                //    _selectedSkIdIdByRackInfo = null;
                //}
                //else if (rackLayout.LegendColorType == "5")
                //{
                //    _selectedBobbinIdByRackInfo = null;
                //}

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
			txtCstId.Text = string.Empty;

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
                dr["CSTPROD"] = rackLayout.SkidCarrierProductCode;
                dr["CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
                dr["CSTID"] = rackLayout.SkidCarrierCode;
                //dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
                //dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
                //dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
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
				txtCstId.Text = string.Empty;
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

                        string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.SkidCarrierCode;

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

                        string targetControl = textBox.Name == "txtLotId" ? ucRackLayout.LotId : ucRackLayout.SkidCarrierCode;

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
                else
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 1)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else if (DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() > 0 && DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name).GetDecimal() < 6)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
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

        #region Method
        private void SelectLotTypeMultiSelectionBox(MultiSelectionBox msb)
        {
            try
            {
                const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_CAPA_SUMMARY_LOTTYPE_CBO";

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
                dr["EQGRID"] = "FCW";
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
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
			const string bizRuleName = "BR_MCS_SEL_FORMATION_PRODUCT_LIST2";

			try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
				inTable.Columns.Add("BLDGCODE", typeof(string));
				inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("QMSHOLD", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
				inTable.Columns.Add("LOTID", typeof(string));
				inTable.Columns.Add("PRODID", typeof(string));
				inTable.Columns.Add("CSTID", typeof(string));
				inTable.Columns.Add("SHIPPING_RESN", typeof(string));

				DataRow dr = inTable.NewRow();

                if (dg.Name == "dgRackInfo")
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
					dr["BLDGCODE"] = cboArea.SelectedValue;
					dr["EQGRID"] = "FCW";
					dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTID"] = _selectedLotIdByRackInfo;
                    dr["EQPTID"] = _selectedEquipmentCode;
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
					dr["AREAID"] = LoginInfo.CFG_AREA_ID;
					dr["EQGRID"] = "FCW";
					dr["BLDGCODE"] = cboArea.SelectedValue;
					dr["EQPTID"] = _selectedEquipmentCode;
                    dr["PRJT_NAME"] = _selectedProjectName;
					dr["PRODID"] = _selectedProdId;
					//dr["WIPHOLD"] = _selectedWipHold;
     //               dr["QMSHOLD"] = _selectedQmsHold;
					dr["SHIPPING_RESN"] = _selectedLotResonCode;
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
            const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_PRODUCT_LIST";
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
                dr["EQGRID"] = "FCW";
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

        private void SelectWareHouseEmptyPallet()
        {
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER";
			const string bizRuleName = "BR_MCS_SEL_FORMATION_EMPTY_CARRIER";

			try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
				inTable.Columns.Add("AREAID", typeof(string));
				inTable.Columns.Add("BLDGCODE", typeof(string));
				inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["BLDGCODE"] = cboArea.SelectedValue;
				dr["EQGRID"] = "FCW";
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

                    Util.GridSetData(dgEmptyPallet, bizResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectWareHouseEmptyPalletList(C1DataGrid dg)
        {
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_EMPTY_CARRIER_LIST";
			const string bizRuleName = "BR_MCS_SEL_FORMATION_EMPTY_CARRIER_LIST";

			try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
				inTable.Columns.Add("BLDGCODE", typeof(string));
				inTable.Columns.Add("EQGRID", typeof(string));
				inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
				inTable.Columns.Add("CST_CLEAN_FLAG", typeof(string));
				inTable.Columns.Add("CSTID", typeof(string));

				DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["BLDGCODE"] = cboArea.SelectedValue;
				dr["EQGRID"] = "FCW";
                dr["EQPTID"] = _selectedEquipmentCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
				dr["CST_CLEAN_FLAG"] = null;
				dr["CSTID"] = _selectedSkIdIdByRackInfo;

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

        private void SelectErrorPalletList(C1DataGrid dg)
        {
            const string bizRuleName = "BR_MCS_SEL_FORMATION_NOREAD_LIST";
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
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQGRID"] = "FCW";
				dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["CSTID"] = _selectedSkIdIdByRackInfo;
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

        private void SelectAbNormalPalletList(C1DataGrid dg)
        {
            //const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_ABNORM_LIST";
			const string bizRuleName = "BR_MCS_SEL_FORMATION_ABNORM_LIST";

			try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
				inTable.Columns.Add("BLDGCODE", typeof(string));
				inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "FCW";
				dr["BLDGCODE"] = cboArea.SelectedValue.ToString();
				dr["EQPTID"] = _selectedEquipmentCode;
                dr["ELTR_TYPE_CODE"] = _selectedLotElectrodeTypeCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["CSTID"] = _selectedSkIdIdByRackInfo;
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

		private void GetBizActorServerInfo()
		{
			DataTable inTable = new DataTable("RQSTDT");
			inTable.Columns.Add("KEYGROUPID", typeof(string));
			DataRow dr = inTable.NewRow();
			dr["KEYGROUPID"] = "FP_MCS_AP_CONFIG";
			inTable.Rows.Add(dr);

			DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_MCS_CONFIG_INFO", "RQSTDT", "RSLTDT", inTable);

			if (CommonVerify.HasTableRow(dtResult))
			{
				foreach (DataRow newRow in dtResult.Rows)
				{
					if (newRow["KEYID"].ToString() == "BizActorIP")
						_bizRuleIp = newRow["KEYVALUE"].ToString();
					else if (newRow["KEYID"].ToString() == "BizActorPort")
						_bizRulePort = newRow["KEYVALUE"].ToString();
					else if (newRow["KEYID"].ToString() == "BizActorProtocol")
						_bizRuleProtocol = newRow["KEYVALUE"].ToString();
					else if (newRow["KEYID"].ToString() == "BizActorServiceIndex")
						_bizRuleServiceIndex = newRow["KEYVALUE"].ToString();
					else
						_bizRuleServiceMode = newRow["KEYVALUE"].ToString();
				}
			}
		}

		private void SelectMcsRackRate(string bldgCode, string eqptId)
		{
			const string bizRuleName = "DA_SEL_MCS_STK_RACK_RATE_GUI";
			DataTable inDataTable = new DataTable("RQSTDT");

			inDataTable.Columns.Add("BLDGOCDE", typeof(string));
			inDataTable.Columns.Add("EQPTID", typeof(string));

			DataRow dr = inDataTable.NewRow();
			dr["BLDGOCDE"] = bldgCode;
			dr["EQPTID"] = (string.IsNullOrEmpty(eqptId)) ? null : eqptId;

			inDataTable.Rows.Add(dr);

			_requestMcsRackRate = new ClientProxy(_bizRuleIp, _bizRuleProtocol, _bizRulePort, _bizRuleServiceMode, _bizRuleServiceIndex).ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
		}

		private void SelectWareHouseSummary()
        {
            const string bizRuleName = "BR_MCS_SEL_FORMATION_SUMMARY2";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
				inTable.Columns.Add("BLDGCODE", typeof(string));
				inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
				dr["AREAID"] = LoginInfo.CFG_AREA_ID;
				dr["BLDGCODE"] = cboArea.SelectedValue.ToString();
				dr["EQGRID"] = "FCW";
				dr["EQPTID"] = string.IsNullOrEmpty(cboStocker.SelectedValue.ToString()) ? null : cboStocker.SelectedValue.ToString();
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

					this.SelectMcsRackRate(cboArea.SelectedValue.ToString(), string.IsNullOrEmpty(cboStocker.SelectedValue.ToString()) ? null : cboStocker.SelectedValue.ToString());

					bizResult.Columns.Add("RACK_RATE2", typeof(decimal));
					bizResult.Columns.Add("RACK_RATE3", typeof(string));

					foreach (DataRow drData in bizResult.Rows)
					{
						string strEqptId = drData["EQPTID"].ToString();

						DataRow[] drs = _requestMcsRackRate.Select("EQPTID = '" + strEqptId + "'");

						if (drs.Length > 0)
						{
							drData["RACK_RATE2"] = drs[0]["CURR_LOAD_RATE"].ToString();
							drData["RACK_RATE3"] = Convert.ToDecimal(drData["RACK_RATE"]).ToString("###,###,##0.##") + "\r\n(" + drData["RACK_RATE2"] + ")";
						}
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
                        ShipPltQty = g.Sum(x => x.Field<decimal>("SHIP_PLT_QTY")),
                        HoldPltQty = g.Sum(x => x.Field<decimal>("HOLD_PLT_QTY")),
                        InspWaitPltQty = g.Sum(x => x.Field<decimal>("INSP_WAIT_PLT_QTY")),
                        NGPltQty = g.Sum(x => x.Field<decimal>("NG_PLT_QTY")),
						VldDatePltQty = g.Sum(x => x.Field<decimal>("VLD_DATE_PLT_QTY")),
						EmptyCount = queryBase.AsQueryable().Select(s => s.EmptyQty).Sum(),
                        ErrorCount = queryBase.AsQueryable().Select(s => s.ErrorQty).Sum(),
                        RackMaxCount = queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum(),
                        SumCarrierCount = g.Sum(x => x.Field<decimal>("RACK_QTY")),
                        //AbNormalQty = g.Sum(x => x.Field<decimal>("ABNORM_QTY")),
                        AbNormalQty = queryBase.AsQueryable().Select(s => s.AbNormalQty).Sum(),
                        RackRate = GetRackRate(queryBase.AsQueryable().Select(s => s.SumCarrierCount).Sum(), queryBase.AsQueryable().Select(s => s.RackMaxQty).Sum()),
						RackRate2 = g.Average(x => x.Field<decimal>("RACK_RATE2")),
						Count = g.Count()
                    }).FirstOrDefault();

                    if (querySum != null)
                    {
                        DataRow newRow = bizResult.NewRow();
                        newRow["EQPTID"] = querySum.EquipmentCode;
                        newRow["EQPTNAME"] = querySum.EquipmentName;
                        newRow["PRJT_NAME"] = querySum.ProjectName;
                        newRow["RACK_MAX_QTY"] = querySum.RackMaxCount;
                        newRow["SHIP_PLT_QTY"] = querySum.ShipPltQty;
                        newRow["HOLD_PLT_QTY"] = querySum.HoldPltQty;
                        newRow["INSP_WAIT_PLT_QTY"] = querySum.InspWaitPltQty;
                        newRow["NG_PLT_QTY"] = querySum.NGPltQty;
						newRow["VLD_DATE_PLT_QTY"] = querySum.VldDatePltQty;
						newRow["EMPTY_QTY"] = querySum.EmptyCount;
                        newRow["ERROR_QTY"] = querySum.ErrorCount;
                        newRow["ABNORM_QTY"] = querySum.AbNormalQty;
						newRow["RACK_RATE"] = querySum.RackRate;
						newRow["RACK_RATE3"] = Convert.ToDecimal(querySum.RackRate).ToString("###,###,##0.##") + "\r\n(" + Convert.ToDecimal(querySum.RackRate2).ToString("###,###,##0.##") +  ")";
						bizResult.Rows.Add(newRow);
					}

                    Util.GridSetData(dgCapacitySummary, bizResult, null, true);
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

                const string bizRuleName = "BR_MCS_SEL_FORMATION_RACK_LAYOUT";
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
                        //ucRackLayout.SkidCarrierProductCode = item["CSTPROD"].GetString();
						ucRackLayout.SkidCarrierProductCode = (item["CSTPROD_NAME"].GetString().Length > 4) ? item["CSTPROD_NAME"].GetString().Substring(0,4) + ".." : item["CSTPROD_NAME"].GetString();		
						ucRackLayout.SkidCarrierProductName = item["CSTPROD_NAME"].GetString();
                        ucRackLayout.SkidCarrierCode = item["CSTID"].GetString();
						//ucRackLayout.BobbinCarrierProductCode = item["BB_CSTPROD"].GetString();
						//ucRackLayout.BobbinCarrierProductName = item["BB_CSTPROD_NAME"].GetString();
						//ucRackLayout.BobbinCarrierCode = item["BB_CSTID"].GetString();
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

        private void InitializeGrid()
        {
            
        }

        private void InitializeCombo()
        {
			// Area 콤보박스
			CommonCombo _combo = new CommonCombo();
			string[] sFilter1 = { "BLDG_CODE", "", LoginInfo.CFG_AREA_ID };
			_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODEATTRS");

            // Stocker 콤보박스
            SetStockerCombo(cboStocker);
        }

        private void MakeRackInfoTable()
        {
            _dtRackInfo = new DataTable();
            _dtRackInfo.Columns.Add("RACK_ID", typeof(string));
            _dtRackInfo.Columns.Add("STATUS", typeof(string));
            _dtRackInfo.Columns.Add("PRJT_NAME", typeof(string));
            _dtRackInfo.Columns.Add("LOTID", typeof(string));
            _dtRackInfo.Columns.Add("CSTPROD", typeof(string));
            _dtRackInfo.Columns.Add("CSTPROD_NAME", typeof(string));
            _dtRackInfo.Columns.Add("CSTID", typeof(string));
            //_dtRackInfo.Columns.Add("BB_CSTPROD", typeof(string));
            //_dtRackInfo.Columns.Add("BB_CSTPROD_NAME", typeof(string));
            //_dtRackInfo.Columns.Add("BB_CSTID", typeof(string));
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
			_selectedProdId = string.Empty;
			_selectedWipHold = string.Empty;
            _selectedQmsHold = string.Empty;
            _selectedLotIdByRackInfo = string.Empty;
            _selectedSkIdIdByRackInfo = string.Empty;
            _selectedBobbinIdByRackInfo = string.Empty;
            txtLotId.Text = string.Empty;
			txtCstId.Text = string.Empty;

            _dtWareHouseCapacity?.Clear();

            Util.gridClear(dgCapacitySummary);
            Util.gridClear(dgRealPallet);
            Util.gridClear(dgEmptyPallet);
            Util.gridClear(dgEmptyPalletList);
            Util.gridClear(dgErrorPallet);
            Util.gridClear(dgAbNormalPallet);
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
                        Name = "CSTID",
                        Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
                        Binding = new Binding() { Path = new PropertyPath("CSTID"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "CSTSTAT_NAME",
						Header = ObjectDic.Instance.GetObjectName("Carrier상태"),
						Binding = new Binding() { Path = new PropertyPath("CSTSTAT_NAME"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "CSTPROD_NAME",
						Header = ObjectDic.Instance.GetObjectName("Tray Type"),
						Binding = new Binding() { Path = new PropertyPath("CSTPROD_NAME"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "ABNORM_TRF_RSN_NAME",
						Header = ObjectDic.Instance.GetObjectName("정보불일치유형"),
						Binding = new Binding() { Path = new PropertyPath("ABNORM_TRF_RSN_NAME"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap
					});

					dgRackInfo.IsReadOnly = true;
                    break;
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
						Name = "CSTID",
						Header = ObjectDic.Instance.GetObjectName("Carrier ID"),
						Binding = new Binding() { Path = new PropertyPath("CSTID"), Mode = BindingMode.OneWay },
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
				case "6":
					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "SEQ",
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
                        Header = ObjectDic.Instance.GetObjectName("RACK명"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
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
						Name = "CSTPROD_NAME",
						Header = ObjectDic.Instance.GetObjectName("Tray Type"),
						Binding = new Binding() { Path = new PropertyPath("CSTPROD_NAME"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "CSTSTAT_YN",
						Header = ObjectDic.Instance.GetObjectName("공Tray 유무"),
						Binding = new Binding() { Path = new PropertyPath("CSTSTAT_YN"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "CST_CLEAN_FLAG",
						Header = ObjectDic.Instance.GetObjectName("세정여부"),
						Binding = new Binding() { Path = new PropertyPath("CST_CLEAN_FLAG"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
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
						Name = "NOTE",
						Header = ObjectDic.Instance.GetObjectName("비고"),
						Binding = new Binding() { Path = new PropertyPath("NOTE"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap
					});

				  //_util.SetDataGridMergeExtensionCol(dgRackInfo, new string[] { "ELTR_TYPE_NAME"}, DataGridMergeMode.VERTICALHIERARCHI);
				  dgRackInfo.IsReadOnly = true;
                    break;

                case "2":
                    dgRackInfo.Columns.Add(new DataGridNumericColumn()
                    {
                        Name = "SEQ",
                        //Header = ObjectDic.Instance.GetObjectName("순위"),
						Header = ObjectDic.Instance.GetObjectName("NO"),
						Binding = new Binding() { Path = new PropertyPath("SEQ"), Mode = BindingMode.OneWay },
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
                        Name = "RACK_NAME",
                        Header = ObjectDic.Instance.GetObjectName("Rack"),
                        Binding = new Binding() { Path = new PropertyPath("RACK_NAME"), Mode = BindingMode.OneWay },
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
                        Name = "CSTINDTTM",
                        Header = ObjectDic.Instance.GetObjectName("입고일시"),
                        Binding = new Binding() { Path = new PropertyPath("CSTINDTTM"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "PAST_DAY",
						Header = ObjectDic.Instance.GetObjectName("경과일수"),
						Binding = new Binding() { Path = new PropertyPath("PAST_DAY"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "BOXID",
						Header = ObjectDic.Instance.GetObjectName("Pallet ID"),
						Binding = new Binding() { Path = new PropertyPath("BOXID"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
                    {
                        Name = "EQSGNAME",
                        Header = ObjectDic.Instance.GetObjectName("LINE"),
                        Binding = new Binding() { Path = new PropertyPath("EQSGNAME"), Mode = BindingMode.OneWay },
                        TextWrapping = TextWrapping.Wrap
                    });

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
					{
						Name = "TOTAL_QTY",
						Header = ObjectDic.Instance.GetObjectName("수량"),
						Binding = new Binding() { Path = new PropertyPath("TOTAL_QTY"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						Format = "#,##0",
						HorizontalAlignment = HorizontalAlignment.Right
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
						Name = "MDLLOT_ID",
						Header = ObjectDic.Instance.GetObjectName("모델LOT"),
						Binding = new Binding() { Path = new PropertyPath("MDLLOT_ID"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "PRODID",
						Header = ObjectDic.Instance.GetObjectName("제품"),
						Binding = new Binding() { Path = new PropertyPath("PRODID"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "SHIPPING_YN",
						Header = ObjectDic.Instance.GetObjectName("출하가능여부"),
						Binding = new Binding() { Path = new PropertyPath("SHIPPING_YN"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "MES_HOLD_YN",
						Header = ObjectDic.Instance.GetObjectName("MES") + " " + ObjectDic.Instance.GetObjectName("HOLD"),
						Binding = new Binding() { Path = new PropertyPath("MES_HOLD_YN"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "QMS_HOLD_YN",
						Header = ObjectDic.Instance.GetObjectName("QMS") + " " + ObjectDic.Instance.GetObjectName("HOLD"),
						Binding = new Binding() { Path = new PropertyPath("QMS_HOLD_YN"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "SUBLOT_HOLD_YN",
						Header = ObjectDic.Instance.GetObjectName("CELL") + " " + ObjectDic.Instance.GetObjectName("HOLD"),
						Binding = new Binding() { Path = new PropertyPath("SUBLOT_HOLD_YN"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "PACK_HOLD_YN",
						Header = ObjectDic.Instance.GetObjectName("PALLET") + " " + ObjectDic.Instance.GetObjectName("HOLD"),
						Binding = new Binding() { Path = new PropertyPath("PACK_HOLD_YN"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "PROD_INSP_RESULT",
						Header = ObjectDic.Instance.GetObjectName("성능검사"),
						Binding = new Binding() { Path = new PropertyPath("PROD_INSP_RESULT"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "MEASR_INSP_RESULT",
						Header = ObjectDic.Instance.GetObjectName("치수검사"),
						Binding = new Binding() { Path = new PropertyPath("MEASR_INSP_RESULT"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "LOW_VOLT_INSP_RESULT",
						Header = ObjectDic.Instance.GetObjectName("한계불량율 (저전압)"),
						Binding = new Binding() { Path = new PropertyPath("LOW_VOLT_INSP_RESULT"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

					dgRackInfo.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
					{
						Name = "OQC_INSP_YN",
						Header = ObjectDic.Instance.GetObjectName("출하검사"),
						Binding = new Binding() { Path = new PropertyPath("OQC_INSP_YN"), Mode = BindingMode.OneWay },
						TextWrapping = TextWrapping.Wrap,
						HorizontalAlignment = HorizontalAlignment.Center
					});

                    dgRackInfo.IsReadOnly = true;

                    //dgRackInfo.Columns["DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["HALF_SLIT_SIDE"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["QMS_JUDGEMENT_CT"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["QMS_JUDGEMENT_RP"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["QMS_JUDGEMENT_ST"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["VD_QA_RESULT"].Visibility = Visibility.Collapsed;
                    //dgRackInfo.Columns["IQC_JUDGEMENT"].Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void SelectLayoutGrid(string type)
        {
            switch (type)
            {
                case "1":   // 정보불일치
                    SelectAbNormalPalletList(dgRackInfo);
                    break;
                case "2":   // 실Pallet
                    SelectWareHouseProductList(dgRackInfo);
                    break;
                case "3":   // 오류Pallet
                    SelectErrorPalletList(dgRackInfo);
                    break;
                case "4":   // 공Pallet
					SelectWareHouseEmptyPalletList(dgRackInfo);
					break;
				case "5":	// 공Tray Pallet
				case "6":   // 공Tray Pallet(세정)
					SelectWareHouseEmptyPalletList(dgRackInfo);
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
            dr["CSTPROD"] = rackLayout.SkidCarrierProductCode;
            dr["CSTPROD_NAME"] = rackLayout.SkidCarrierProductName;
            dr["CSTID"] = rackLayout.SkidCarrierCode;
            //dr["BB_CSTPROD"] = rackLayout.BobbinCarrierProductCode;
            //dr["BB_CSTPROD_NAME"] = rackLayout.BobbinCarrierProductName;
            //dr["BB_CSTID"] = rackLayout.BobbinCarrierCode;
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

        private void SetStockerCombo(C1ComboBox cbo)
        {
			DataTable dtRQSTDT = new DataTable();
			dtRQSTDT.TableName = "RQSTDT";
			dtRQSTDT.Columns.Add("LANGID", typeof(string));
			dtRQSTDT.Columns.Add("BLDGCODE", typeof(string));

			DataRow drNewrow = dtRQSTDT.NewRow();
			drNewrow["LANGID"] = LoginInfo.LANGID;
			drNewrow["BLDGCODE"] = cboArea.SelectedValue.ToString();
			dtRQSTDT.Rows.Add(drNewrow);

			new ClientProxy().ExecuteService("DA_MCS_SEL_FORMATION_STK_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
			{
				if (Exception != null)
				{
					Util.AlertByBiz("DA_MCS_SEL_FORMATION_STK_CBO", Exception.Message, Exception.ToString());
					return;
				}

				DataTable dtTemp = new DataTable();
				dtTemp = result.Copy();
				//cbo.ItemsSource = DataTableConverter.Convert(dtTemp);

				//ComboStatus cs = ComboStatus
				cbo.ItemsSource = AddStatus(dtTemp, CommonCombo.ComboStatus.ALL, "EQPTID", "EQPTNAME").Copy().AsDataView();
				cbo.SelectedIndex = 0;
			}
			);
		}

		private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
		{
			DataRow dr = dt.NewRow();

			switch (cs)
			{
				case CommonCombo.ComboStatus.ALL:
					dr[sDisplay] = "-ALL-";
					dr[sValue] = "";
					dt.Rows.InsertAt(dr, 0);
					break;

				case CommonCombo.ComboStatus.SELECT:
					dr[sDisplay] = "-SELECT-";
					dr[sValue] = "SELECT";
					dt.Rows.InsertAt(dr, 0);
					break;

				case CommonCombo.ComboStatus.NA:
					dr[sDisplay] = "-N/A-";
					dr[sValue] = "";
					dt.Rows.InsertAt(dr, 0);
					break;
			}

			return dt;
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
        #endregion
    }
}
