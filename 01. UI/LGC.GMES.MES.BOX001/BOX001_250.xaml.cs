/*************************************************************************************
 Created Date : 2020.08.17
      Creator : 오화백
   Decription : 활성화 포장실 재공 현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.17  오화백 : Initial Created.    

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

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_250.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_250 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        private readonly Util _util = new Util();
        private string _selectedProdID = string.Empty;
        private string _selectedSearchCoed = string.Empty;

     
        private double _scrollToHorizontalOffset = 0;
        private bool _isscrollToHorizontalOffset = false;

        private DataTable _dtLocation = null;
    

        public BOX001_250()
        {
            InitializeComponent();
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button> { btnSearch };
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeGrid();   //그리드 초기화
            InitializeCombo();  //콤보박스 조회
            InitializeFormation(); //포장실 재공현황 조회

            Loaded -= UserControl_Loaded;

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _isscrollToHorizontalOffset = true;
            _scrollToHorizontalOffset = dgProduct.Viewport.HorizontalOffset;
        }

        /// <summary>
        /// 콤보박스 조회
        /// </summary>
        private void InitializeCombo()
        {
             CommonCombo _combo = new CMM001.Class.CommonCombo();
             _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            // 콤보박스
            SetAreaCombo();
        }
        
        /// <summary>
        /// 포장실 재공현황 조회
        /// </summary>
        private void InitializeFormation()
        {
           // SetSearchFormationWarehose();
        }

        #endregion

        #region Event
        
        #region 조회 버튼 클릭  :  btnSearch_Click()

        /// <summary>
        /// 조회 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            InitializeFormation();
            ClearControl();
            //SelectWareHouseProductList(dgProduct);
            //ClearControl();
            SetSearchFormationWarehose();
        }
        #endregion

        #region 팔렛 텍스트 박스 KeyDown : txtPalletID_KeyDown()

        /// <summary>
        /// Pallet ID  keydown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClearControl();
                SelectWareHouseProductList(dgProduct);
            }
        }

        #endregion

        #region Pallet ID  멀티 조회 : txtPalletID_PreviewKeyDown()

        /// <summary>
        /// Pallet 멀티 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);


                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    foreach (string item in sPasteStrings)
                    {
                        if (!string.IsNullOrEmpty(item) && Multi_Cell(item) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }



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

                e.Handled = true;
            }
            txtPalletID.Focus();
        }

        #endregion
        
        #region  포장실 재공 현황 스프레드 이벤트  : dgProductSummary_LoadedCellPresenter(), dgProductSummary_UnloadedCellPresenter(), dgProductSummary_MouseLeftButtonUp()

        /// <summary>
        /// 데이터 조회 링크 색 설정
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
                    || string.Equals(e.Cell.Column.Name, "SHIP_PLT_QTY")
                    || string.Equals(e.Cell.Column.Name, "HOLD_PLT_QTY")
                    || string.Equals(e.Cell.Column.Name, "INSP_WAIT_PLT_QTY")
                    || string.Equals(e.Cell.Column.Name, "NG_PLT_QTY")
                    || string.Equals(e.Cell.Column.Name, "LONG_TERM_PLT_QTY")
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
            }));
        }


        /// <summary>
        /// 스프레드 스크롤 내릴떄 색이 초기화되는 문제 해결하기 위해 추가
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
        /// 링크걸린 컬럼 클릭시 상세 조회
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
                    || cell.Column.Name.Equals("SHIP_CELL_QTY")
                    || cell.Column.Name.Equals("HOLD_CELL_QTY")
                    || cell.Column.Name.Equals("INSP_WAIT_CELL_QTY")
                    || cell.Column.Name.Equals("NG_CELL_QTY")
                    || cell.Column.Name.Equals("PRODID")
                    || cell.Column.Name.Equals("LONG_TERM_CELL_QTY")
                    )
                {
                    return;
                }

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                _selectedProdID = DataTableConverter.GetValue(drv, "PRODID").GetString();

                if (cell.Column.Name.Equals("PRJT_NAME"))
                {
                    _selectedSearchCoed  = "ALL";
              
                }
                else if (cell.Column.Name.Equals("SHIP_PLT_QTY"))
                {
                    _selectedSearchCoed = "OK";
                }
                else if (cell.Column.Name.Equals("HOLD_PLT_QTY"))
                {
                    _selectedSearchCoed = "HOLD";
                }
                else if (cell.Column.Name.Equals("INSP_WAIT_PLT_QTY"))
                {
                    _selectedSearchCoed = "INSP_WAIT";
                }
                else if (cell.Column.Name.Equals("NG_PLT_QTY"))
                {
                    _selectedSearchCoed = "NG";
                }
                else if (cell.Column.Name.Equals("LONG_TERM_PLT_QTY"))
                {
                    _selectedSearchCoed = "LONG_TERM";
                }

                SelectWareHouseProductList(dgProduct);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    
        #region  상세 내용 스프레드 이벤트  : dgProduct_LoadedCellPresenter(), dgProduct_UnloadedCellPresenter(), dgProduct_PreviewMouseLeftButtonDown()

        /// <summary>
        /// 출하가능여부 관련 색깔 표현
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
        /// 스프레드 스크롤 내릴떄 색이 초기화되는 문제 해결하기 위해 추가
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

        /// <summary>
        /// 스크롤 관련 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgProduct_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isscrollToHorizontalOffset = false;
        }

        #endregion

        #region DoEvent : DoEvents()
        /// <summary>
        /// DoEvent
        /// </summary>
        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        #endregion

        #region 창고 콤보 이벤트 : cboWarehose_SelectedValueChanged()
       
        /// <summary>
        /// 창고정보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboWarehose_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            ClearControl();
            //SetSearchFormationWarehose();
        }

        #endregion


        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            cboWarehose.IsEnabled = false;
        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            cboWarehose.IsEnabled = true;
        }

        private void chkAssemblyLot_Checked(object sender, RoutedEventArgs e)
        {
            dgProduct.Columns["LOTID"].Visibility = Visibility.Visible;
        }

        private void chkAssemblyLot_Unchecked(object sender, RoutedEventArgs e)
        {
            dgProduct.Columns["LOTID"].Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Method

        #region 포장창고 콤보 조회 : SetAreaCombo()
        /// <summary>
        /// 포장창고 콤보 조회
        /// </summary>
        private void SetAreaCombo()
        {
            DataTable dtRQSTDT = new DataTable();
            dtRQSTDT.TableName = "RQSTDT";
            dtRQSTDT.Columns.Add("AREAID", typeof(string));
            dtRQSTDT.Columns.Add("WH_TYPE_CODE", typeof(string));

            DataRow drnewrow = dtRQSTDT.NewRow();
            drnewrow["AREAID"] = LoginInfo.CFG_AREA_ID.ToString();
            drnewrow["WH_TYPE_CODE"] = "PG";
            dtRQSTDT.Rows.Add(drnewrow);

            new ClientProxy().ExecuteService("DA_PRD_SEL_FORMATION_WAREHOUSE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    Util.AlertByBiz("DA_PRD_SEL_FORMATION_WAREHOUSE_CBO", Exception.Message, Exception.ToString());
                    return;
                }
                _dtLocation = new DataTable();
                _dtLocation = result.Copy();
                cboWarehose.ItemsSource = DataTableConverter.Convert(_dtLocation);
                cboWarehose.SelectedIndex = 0;
            }
            );
        }

        #endregion

        #region 활성화 포장실 재공조회 : SetSearchFormationWarehose()
        /// <summary>
        /// 활성화 포장실 재공조회
        /// </summary>
        private void SetSearchFormationWarehose()
        {
            try
            {
                string wareHouseId;

                if (chkSelectAll.IsChecked != null && (bool) chkSelectAll.IsChecked)
                {
                    wareHouseId = null;
                }
                else
                {
                    if (cboWarehose.SelectedValue == null)
                    {
                        return;
                    }

                    wareHouseId = cboWarehose.SelectedValue.GetString();

                }
                    
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID");
                dt.Columns.Add("AREAID");
                dt.Columns.Add("PRODID");
                dt.Columns.Add("WH_ID");
                dt.Columns.Add("PJT");
                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] =  dr["AREAID"] = Util.GetCondition(cboArea, "SFU4238"); // 동정보를 선택하세요
                if (dr["AREAID"].Equals("")) return;
                dr["PJT"] = txtPjt.Text;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("BR_PRD_SEL_STOCK_SUMMARY_CP", "INDATA", "OUTDATA", dt, (result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if(result.Rows.Count > 0)
                    {
                        double Ship_cell_Qty = 0;
                        double Hold_cell_Qty = 0;
                        double Insp_wait_cell_Qty = 0;
                        double Ng_cell_Qty = 0;
                        double longTermCellQty = 0;

                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            Ship_cell_Qty = Ship_cell_Qty + Convert.ToDouble(result.Rows[i]["SHIP_CELL_QTY"].ToString());
                            Hold_cell_Qty = Hold_cell_Qty + Convert.ToDouble(result.Rows[i]["HOLD_CELL_QTY"].ToString());
                            Insp_wait_cell_Qty = Insp_wait_cell_Qty + Convert.ToDouble(result.Rows[i]["INSP_WAIT_CELL_QTY"].ToString());
                            Ng_cell_Qty = Ng_cell_Qty + Convert.ToDouble(result.Rows[i]["NG_CELL_QTY"].ToString());
                            longTermCellQty = longTermCellQty + Convert.ToDouble(result.Rows[i]["LONG_TERM_CELL_QTY"].ToString());

                            txtRealCarrierCount.Text = String.Format("{0:#,##0}", Ship_cell_Qty + Hold_cell_Qty + Insp_wait_cell_Qty + Ng_cell_Qty + longTermCellQty);
                        }

                    }
                    else

                    {
                        txtRealCarrierCount.Text = "0";
                    }

    
                    Util.GridSetData(dgProductSummary, result, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region  상세내용 조회 : SelectWareHouseProductList()

        /// <summary>
        /// 상세내용 조회
        /// </summary>
        /// <param name="dg"></param>
        private void SelectWareHouseProductList(C1DataGrid dg)
        {

            //const string bizRuleName = "BR_PRD_SEL_SHIP_AREA_STOCK_LIST_CP_L";
            const string bizRuleName = "DA_PRD_SEL_STOCK_LIST_CP_AS_ASSYLOT";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("PACK_WRK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("VIEW_PROC", typeof(string));
                inTable.Columns.Add("VIEW_EXP", typeof(string));
                inTable.Columns.Add("VIEW_HOLD", typeof(string));
                inTable.Columns.Add("VIEW_OWMS", typeof(string));
                inTable.Columns.Add("VIEW_INSP_WAIT", typeof(string));
                inTable.Columns.Add("VIEW_INSP_NG", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = null;
                dr["PRODID"] = string.IsNullOrEmpty(_selectedProdID) ? null : _selectedProdID;
                dr["PACK_WRK_TYPE_CODE"] = null;
                if (_selectedSearchCoed .Equals("OK"))
                {
                    dr["VIEW_PROC"] = _selectedSearchCoed;

                }
                else if (_selectedSearchCoed.Equals("HOLD"))
                {
                    dr["VIEW_HOLD"] = _selectedSearchCoed;
                }
                else if (_selectedSearchCoed.Equals("INSP_WAIT"))
                {
                    dr["VIEW_INSP_WAIT"] = _selectedSearchCoed;
                }
                else if (_selectedSearchCoed.Equals("NG"))
                {
                    dr["VIEW_INSP_NG"] = _selectedSearchCoed;
                }
                else if (_selectedSearchCoed.Equals("LONG_TERM"))
                {
                    dr["VIEW_EXP"] = _selectedSearchCoed;
                }
                else 
                {
                    dr["VIEW_PROC"] = null;
                    dr["VIEW_HOLD"] = null;
                    dr["VIEW_INSP_WAIT"] = null;
                    dr["VIEW_INSP_NG"] = null;
                    dr["VIEW_EXP"] = null;
                    dr["VIEW_OWMS"] = null;
                }

                
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dg, bizResult, null, false);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region PalletID 멀티 상세조회 : Multi_Cell()

        /// <summary>
        /// Pallet ID 멀티 상세조회
        /// </summary>
        /// <param name="pallet_ID"></param>
        /// <returns></returns>
        bool Multi_Cell(string pallet_ID)
        {
            try
            {
                DoEvents();

                const string bizRuleName = "BR_PRD_SEL_SHIP_AREA_STOCK_LIST_CP_L";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SEARCH_CODE", typeof(string));
                inTable.Columns.Add("WH_ID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));
                inTable.Columns.Add("BOXID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PRODID"] = string.IsNullOrEmpty(_selectedProdID) ? null : _selectedProdID;
                dr["SEARCH_CODE"] = string.IsNullOrEmpty(_selectedSearchCoed) ? "ALL" : _selectedSearchCoed;
                dr["BOXID"] = pallet_ID;

                string warehouseId;
                string rackId;

                if (chkSelectAll.IsChecked != null && (bool)chkSelectAll.IsChecked)
                {
                    warehouseId = null;
                    rackId = null;
                }
                else
                {
                    warehouseId = cboWarehose.SelectedValue.GetString();
                    if (txtLocation.Text != string.Empty)
                    {
                        string _Location = string.Empty;
                        DataRow[] drLocation = _dtLocation.Select("CBO_CODE ='" + cboWarehose.SelectedValue.ToString() + "'");
                        _Location = drLocation[0]["WH_PHYS_PSTN_CODE"].ToString() + txtLocation.Text;

                        rackId = _Location;
                    }
                    else
                    {
                        rackId = null;
                    }
                }

                dr["WH_ID"] = warehouseId;
                dr["RACK_ID"] = rackId;
                inTable.Rows.Add(dr);


                /*
                if (_selectedProdID != string.Empty)
                {
                    dr["PRODID"] = _selectedProdID;
                }
                if (_selectedSearchCoed != string.Empty)
                {
                    dr["SEARCH_CODE"] = _selectedSearchCoed;
                }
                else
                {
                    dr["SEARCH_CODE"] = "ALL";
                }
                dr["WH_ID"] = cboWarehose.SelectedValue.ToString();
                if (txtLocation.Text != string.Empty)
                {
                    string _Location = string.Empty;
                    DataRow[] drLocation = _dtLocation.Select("CBO_CODE ='" + cboWarehose.SelectedValue.ToString() + "'");
                    _Location = drLocation[0]["WH_PHYS_PSTN_CODE"].ToString() + txtLocation.Text;

                    dr["RACK_ID"] = txtLocation.Text;
                }
                dr["BOXID"] = pallet_ID;
                inTable.Rows.Add(dr);
                */

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (dgProduct.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgProduct, bizResult, FrameOperation, true);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgProduct.ItemsSource);

                        if (dtInfo.Rows.Count > 0)
                        {
                            if (dtInfo.Select("BOXID = '" + pallet_ID + "'").Length > 0)
                            {
                                bizResult.Rows.Remove(bizResult.Select("BOXID = '" + pallet_ID + "'")[0]);
                            }
                        }
                        dtInfo.Merge(bizResult);
                        Util.GridSetData(dgProduct, dtInfo, FrameOperation, true);
                    }

                });

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region 포장창고 컬럼높이 정의 : InitializeGrid()
        /// <summary>
        /// 포장창고 컬럼높이 정의
        /// </summary>
        private void InitializeGrid()
        {
            dgProductSummary.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgProductSummary.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(40);

        }

        #endregion

        #region 초기화 : ClearControl()
        /// <summary>
        /// 초기화 
        /// </summary>
        private void ClearControl()
        {

            _selectedProdID = string.Empty;
            _selectedSearchCoed = string.Empty;

            Util.gridClear(dgProduct);

        }

        #endregion
 
        #region loadingIndicator  : ShowLoadingIndicator(), HiddenLoadingIndicator()
        /// <summary>
        /// loadingIndicator 함수
        /// </summary>
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

        #endregion


    }
}
