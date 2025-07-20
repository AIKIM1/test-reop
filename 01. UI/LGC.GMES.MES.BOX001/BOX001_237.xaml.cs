/*************************************************************************************
 Created Date : 2020.02.23
      Creator : 이제섭
   Decription : IM향 데이터 조회 (물류)
--------------------------------------------------------------------------------------
 [Change History]
  2020.02.23  이제섭 : Initial Created. (BOX001_236 Copy 하여 물류 전용 화면 신규 생성)
  2020.02.29  이제섭 : Summary 정보를 팔레트 ID로 저장하는 기능추가 

**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using C1.WPF;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_237 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();

        private string _BOXID = string.Empty;

        private string sAREAID = string.Empty;
        private string sSHOPID = string.Empty;
        private string sAREAID2 = string.Empty;
        private string sSHOPID2 = string.Empty;

        public BOX001_237()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        DataGridRowHeaderPresenter pre = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center

        };

        #endregion

        #region Initialize
        private void Initialize()
        {
            //dtpDateFrom.SelectedDateTime = DateTime.Now;
            //dtpDateTo.SelectedDateTime = DateTime.Now;

            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sCase: "ALLAREA", cbChild: new C1ComboBox[] { cboLine2 });

            _combo.SetCombo(cboLine2, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea2 }, sFilter: new string[] { "B" }, sCase: "PROCESSSEGMENTLINE");

            dtpDateFrom2.SelectedDateTime = DateTime.Today;
            dtpDateTo2.SelectedDateTime = DateTime.Today;

            // 오창 소형 조립이면, dgSummary cell 수량 컬럼 보이게 처리, 포장출고 실적 Tab 숨김
            if (LoginInfo.CFG_SHOP_ID == "A010")
            {
                dgSummary.Columns["SUBLOT_QTY"].Visibility = Visibility.Visible;

                tbshiphist.Visibility = Visibility.Collapsed;
            }
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            //dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            //dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;

        }
        #endregion

        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo2.SelectedDateTime;
                return;
            }
        }
        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom2.SelectedDateTime;
                return;
            }
        }

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetEvent();

            if (LoginInfo.CFG_SHOP_ID == "A010")
            {
                btnExcelOC.Visibility = Visibility.Visible;
                btnCSVOC_All.Visibility = Visibility.Visible;
                btnCSVOC_All2.Visibility = Visibility.Visible;
            }
            else
            {
                btnExcelOC.Visibility = Visibility.Collapsed;
                btnCSVOC_All.Visibility = Visibility.Collapsed;
                btnCSVOC_All2.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [Shipment KeyDown]
        private void txtshipment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
        #endregion

        #region [Shipment KeyDown]
        private void txtpackinglist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
        #endregion

        #region [Pallet List 조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region [Excel Download]
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgHist.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "CSV Files|*.csv";
                saveFileDialog.Title = "Save an CSV File";
                saveFileDialog.FileName = DataTableConverter.GetValue(dgHist.Rows[0].DataItem, "SHIPPING_NO") as string; //"CSVExported_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                {
                    new ExcelExporter().Export_CSV(dgHist, saveFileDialog.FileName);

                    //저장이 완료되었습니다.
                    Util.MessageValidation("10004");
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

        private void btnCSV_All_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string SaveFolder = string.Empty;

                if (dgSummary.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "CSV Files|*.csv";
                saveFileDialog.Title = "Save an CSV File";
                saveFileDialog.FileName = "PALLET_NO";

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                    SaveFolder = saveFileDialog.FileName;
                else
                    return;

                MakeCsv(SaveFolder);

                //저장이 완료되었습니다.
                Util.MessageValidation("10004");

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


        private void btnCSV_All2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string SaveFolder = string.Empty;
                string SaveFileName = string.Empty;

                if (dgSummary.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }

                string ShipNo = DataTableConverter.GetValue(dgSummary.Rows[0].DataItem, "SHIP_NO") as string;

                loadingIndicator.Visibility = Visibility.Visible;

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "CSV Files|*.csv";
                saveFileDialog.Title = "Save an CSV File";
                saveFileDialog.FileName = "PALLET_NO";

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                {
                    SaveFileName = saveFileDialog.FileName;
                                          
                    System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(SaveFileName.Replace("PALLET_NO.csv", ShipNo));

                    if (directoryInfo.Exists != true)
                    {
                        directoryInfo.Create();
                    }

                    SaveFileName = Convert.ToString(directoryInfo) + "\\" + "PALLET_NO.csv";
                }
                else
                    return;

                MakeCsv(SaveFileName);

                //저장이 완료되었습니다.
                Util.MessageValidation("10004");

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

        private void MakeCsv(string sSaveDirectory)
        {
            try
            {
                for (int i = 0; i < dgSummary.Rows.Count - 1; i++)
                {
                    string _Pallet_ID = string.Empty;
                    string _SaveFileName = sSaveDirectory;

                    _Pallet_ID = DataTableConverter.GetValue(dgSummary.Rows[i].DataItem, "BOXID") as string;
                    
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PALLETID", typeof(string));
                    dtRqst.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["PALLETID"] = _Pallet_ID;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "RQSTDT", "RSLTDT", dtRqst);

                    new ExcelExporter().Export_CSV2(dtRslt, dgHist, _SaveFileName.Replace("PALLET_NO", _Pallet_ID));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MakeCsvOC(string sSaveDirectory)
        {
            try
            {
                for (int i = 0; i < dgSummary.Rows.Count - 1; i++)
                {
                    string _Pallet_ID = string.Empty;
                    string _SaveFileName = sSaveDirectory;

                    _Pallet_ID = DataTableConverter.GetValue(dgSummary.Rows[i].DataItem, "BOXID") as string;

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PALLETID", typeof(string));
                    dtRqst.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["PALLETID"] = _Pallet_ID;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "RQSTDT", "RSLTDT", dtRqst);

                    new ExcelExporter().Export_CSV2(dtRslt, dgHistCSVOC, _SaveFileName.Replace("PALLET_NO", _Pallet_ID));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                //if (e.Cell.Row.ParentGroup != null && e.Cell.Column.Name.Equals("PKG_LOTID"))
                if (e.Cell.Column.Name.Equals("BOXID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }
            }));
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (dg.CurrentColumn != null)
                {
                    if (dg.CurrentColumn.Name.Equals("BOXID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                    {
                        _BOXID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "BOXID"));

                        GetDetail(_BOXID);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.NVC(cboArea2.SelectedValue) == string.Empty || Util.NVC(cboArea2.SelectedValue) == "SELECT")
                {
                    // SFU1499 동을 선택해주십시오.
                    Util.MessageValidation("SFU1499");
                    return;
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_DTTM", typeof(String));
                RQSTDT.Columns.Add("PROJECT", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtPkgLotID2.Text))
                {
                    dr["LOTID"] = txtPkgLotID2.Text.Trim();
                }
                else
                {
                    dr["AREAID"] = cboArea2.SelectedValue;
                    dr["EQSGID"] = string.IsNullOrWhiteSpace(Util.NVC(cboLine2.SelectedValue)) ? null : cboLine2.SelectedValue;
                    dr["FROM_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["PROJECT"] = String.IsNullOrWhiteSpace(txtProject2.Text) ? null : txtProject2.Text;
                    dr["PRODID"] = String.IsNullOrWhiteSpace(txtProdId2.Text) ? null : txtProdId2.Text;
                    dr["PKG_LOTID"] = String.IsNullOrWhiteSpace(txtPkgLotID2.Text) ? null : txtPkgLotID2.Text;
                    dr["BOXID"] = String.IsNullOrWhiteSpace(txtBoxID2.Text) ? null : txtBoxID2.Text;


                }

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIP_ERP_TRSF_HIST_TESLA_NJ", "INDATA", "OUTDATA", RQSTDT);
                Util.GridSetData(dgSearhResult2, SearchResult, FrameOperation, true);

                if (dgSearhResult2.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSearhResult2.Columns["RCV_ISS_ID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                    DataGridAggregate.SetAggregateFunctions(dgSearhResult2.Columns["ISS_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSearhResult2.Columns["SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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


        #endregion

        #region Mehod

        #region [Biz]
        private void Search()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHIP_NO", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();

                dr["SHIP_NO"] = String.IsNullOrWhiteSpace(txtshipment.Text.ToString().Trim()) ? null : txtshipment.Text.ToString().Trim();
                dr["BOXID"] = String.IsNullOrWhiteSpace(txtpackinglist.Text.ToString().Trim()) ? null : txtpackinglist.Text.ToString().Trim();

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLT_LIST_BY_SHIPMENT_NO_NJ", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgHist);
                Util.gridClear(dgHistCSVOC);

                if (dtRslt.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["TOTAL_QTY"],  new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });

                    if (dgSummary.Columns["SUBLOT_QTY"].Visibility == Visibility.Visible)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                    }
                }

                DataGrid_Summary();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                txtshipment.Text = string.Empty;
                txtpackinglist.Text = string.Empty;
                txtshipment.Focus();
            }
        }

        private void GetDetail(string sPalletID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sPalletID))
                    return;

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("PALLETID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["PALLETID"] = sPalletID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgHist, dtRslt, FrameOperation, true);
                Util.GridSetData(dgHistCSVOC, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
        #endregion

        #region [Func]

        private void DataGrid_Summary()
        {
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            dagsum.ResultTemplate = dgSummary.Resources["ResultTemplate"] as DataTemplate;
            dac.Add(dagsum);
            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["TOTAL_QTY"], dac);
            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUBLOT_QTY"], dac);
        }

        private void dgSearhResult2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FFFF96");

                    string sqty = Convert.ToString(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SUBLOT_QTY"));

                    string iqty = Convert.ToString(Convert.ToInt32(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ISS_QTY")));

                    if (sqty != iqty)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);

                        if (e.Cell.Column.Name.Equals("SUBLOT_QTY") || e.Cell.Column.Name.Equals("ISS_QTY"))
                        {
                            //e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgSearhResult2_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
        #endregion

        private void btnExcelOC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgHistCSVOC.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "CSV Files|*.csv";
                saveFileDialog.Title = "Save an CSV File";
                saveFileDialog.FileName = DataTableConverter.GetValue(dgHistCSVOC.Rows[0].DataItem, "SHIPPING_NO") as string; //"CSVExported_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                {
                    new ExcelExporter().Export_CSV(dgHistCSVOC, saveFileDialog.FileName);

                    //저장이 완료되었습니다.
                    Util.MessageValidation("10004");
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

        private void btnCSVOC_All_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string SaveFolder = string.Empty;

                if (dgSummary.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "CSV Files|*.csv";
                saveFileDialog.Title = "Save an CSV File";
                saveFileDialog.FileName = "PALLET_NO";

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                    SaveFolder = saveFileDialog.FileName;
                else
                    return;

                MakeCsvOC(SaveFolder);

                //저장이 완료되었습니다.
                Util.MessageValidation("10004");

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

        private void btnCSVOC_All2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string SaveFolder = string.Empty;
                string SaveFileName = string.Empty;

                if (dgSummary.GetRowCount() == 0)
                {
                    //SFU1498	데이터가 없습니다.
                    Util.MessageValidation("SFU1498");
                    return;
                }

                string ShipNo = DataTableConverter.GetValue(dgSummary.Rows[0].DataItem, "SHIP_NO") as string;

                loadingIndicator.Visibility = Visibility.Visible;

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "CSV Files|*.csv";
                saveFileDialog.Title = "Save an CSV File";
                saveFileDialog.FileName = "PALLET_NO";

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                {
                    SaveFileName = saveFileDialog.FileName;

                    System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(SaveFileName.Replace("PALLET_NO.csv", ShipNo));

                    if (directoryInfo.Exists != true)
                    {
                        directoryInfo.Create();
                    }

                    SaveFileName = Convert.ToString(directoryInfo) + "\\" + "PALLET_NO.csv";
                }
                else
                    return;

                MakeCsvOC(SaveFileName);

                //저장이 완료되었습니다.
                Util.MessageValidation("10004");

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

        private void btnTransferSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtLotID.Text) && string.IsNullOrEmpty(txShipmentNo.Text) && string.IsNullOrEmpty(txPackingList.Text) && string.IsNullOrEmpty(txTeslaPalletID.Text))
                {
                    // 조회조건 입력 후 조회해야합니다.
                    Util.MessageValidation("SFU4494");
                    return;
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("PO_NO", typeof(String));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(String));
                RQSTDT.Columns.Add("FROM_LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = String.IsNullOrWhiteSpace(txPackingList.Text) ? null : txPackingList.Text;
                dr["PO_NO"] = String.IsNullOrWhiteSpace(txShipmentNo.Text) ? null : txShipmentNo.Text;
                dr["PKG_LOTID"] = String.IsNullOrWhiteSpace(txtLotID.Text) ? null : txtLotID.Text;
                dr["FROM_LOTID"] = String.IsNullOrWhiteSpace(txTeslaPalletID.Text) ? null : txTeslaPalletID.Text;

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FOX_TRSF_RESULT", "INDATA", "OUTDATA", RQSTDT);
                Util.GridSetData(dgTransferResult, SearchResult, FrameOperation, true);

                if (dgTransferResult.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgTransferResult.Columns["ROW_NUM"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                    DataGridAggregate.SetAggregateFunctions(dgTransferResult.Columns["FOX_SEND_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgTransferResult.Columns["PACKING_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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
