/*************************************************************************************
 Created Date : 2020.01.18
      Creator : 이제섭
   Decription : IM사 데이터 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.01.18  이제섭 : Initial Created.


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
//using System.Windows.Forms;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_236 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();

        private string _PKG_LOTID = string.Empty;

        private string sAREAID = string.Empty;
        private string sSHOPID = string.Empty;
        private string sAREAID2 = string.Empty;
        private string sSHOPID2 = string.Empty;

        public BOX001_236()
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
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

        }
        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetEvent();
        }
        #endregion

        #region [Outbox KeyDown]
        private void txtOutbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    if (String.IsNullOrWhiteSpace(txtOutbox.Text.ToString().Trim()))
                    {
                        // OUTBOX를 입력하세요.
                        Util.MessageInfo("SFU5008");
                        return;
                    }

                    string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                    string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PALLETID", typeof(string));
                    dtRqst.Columns.Add("ASSY_LOTID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));
                    dtRqst.Columns.Add("INBOXID", typeof(string));
                    dtRqst.Columns.Add("OUTBOXID", typeof(string));
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["PALLETID"] = null;
                    dr["ASSY_LOTID"] = null;
                    dr["FROM_DATE"] = null;
                    dr["TO_DATE"] = null;
                    dr["INBOXID"] = null;
                    dr["OUTBOXID"] = String.IsNullOrWhiteSpace(txtOutbox.Text.ToString().Trim()) ? null : txtOutbox.Text.ToString().Trim();
                    dr["SUBLOTID"] = null;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                    dtRqst.Rows.Add(dr);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgHist, dtRslt, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    txtOutbox.Text = string.Empty;
                    txtOutbox.Focus();
                }
            }
        }
        #endregion

        #region [Inbox KeyDown]
        private void txtInbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    if (String.IsNullOrWhiteSpace(txtInbox.Text.ToString().Trim()))
                    {
                        // Inbox ID를 입력 하세요.
                        Util.MessageInfo("SFU4517");
                        return;
                    }

                    string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                    string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PALLETID", typeof(string));
                    dtRqst.Columns.Add("ASSY_LOTID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));
                    dtRqst.Columns.Add("INBOXID", typeof(string));
                    dtRqst.Columns.Add("OUTBOXID", typeof(string));
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["PALLETID"] = null;
                    dr["ASSY_LOTID"] = null;
                    dr["FROM_DATE"] = null;
                    dr["TO_DATE"] = null;
                    dr["INBOXID"] = String.IsNullOrWhiteSpace(txtInbox.Text.ToString().Trim()) ? null : txtInbox.Text.ToString().Trim();
                    dr["OUTBOXID"] = null;
                    dr["SUBLOTID"] = null;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                    dtRqst.Rows.Add(dr);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgHist, dtRslt, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    txtInbox.Text = string.Empty;
                    txtInbox.Focus();
                }
            }
        }
        #endregion

        #region [Pallet KeyDown]
        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    if (String.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()))
                    {
                        // PALLETID를 입력해주세요
                        Util.MessageInfo("SFU1411");
                        return;
                    }

                    string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                    string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PALLETID", typeof(string));
                    dtRqst.Columns.Add("ASSY_LOTID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));
                    dtRqst.Columns.Add("INBOXID", typeof(string));
                    dtRqst.Columns.Add("OUTBOXID", typeof(string));
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["PALLETID"] = String.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()) ? null : txtPallet.Text.ToString().Trim();
                    dr["ASSY_LOTID"] = null;
                    dr["FROM_DATE"] = null;
                    dr["TO_DATE"] = null;
                    dr["INBOXID"] = null;
                    dr["OUTBOXID"] = null;
                    dr["SUBLOTID"] = null;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                    dtRqst.Rows.Add(dr);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgHist, dtRslt, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    txtPallet.Text = string.Empty;
                    txtPallet.Focus();
                }
            }
        }
        #endregion

        #region [Sublot KeyDown]

        private void txtSublot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    if (String.IsNullOrWhiteSpace(txtSublot.Text.ToString().Trim()))
                    {
                        // CELLID를 스캔 또는 입력하세요.
                        Util.MessageInfo("SFU1323");
                        return;
                    }

                    string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                    string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("PALLETID", typeof(string));
                    dtRqst.Columns.Add("ASSY_LOTID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));
                    dtRqst.Columns.Add("INBOXID", typeof(string));
                    dtRqst.Columns.Add("OUTBOXID", typeof(string));
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["PALLETID"] = null;
                    dr["ASSY_LOTID"] = null;
                    dr["FROM_DATE"] = null;
                    dr["TO_DATE"] = null;
                    dr["INBOXID"] = null;
                    dr["OUTBOXID"] = null;
                    dr["SUBLOTID"] = String.IsNullOrWhiteSpace(txtSublot.Text.ToString().Trim()) ? null : txtSublot.Text.ToString().Trim();
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                    dtRqst.Rows.Add(dr);
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "INDATA", "OUTDATA", dtRqst);

                    Util.GridSetData(dgHist, dtRslt, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    txtSublot.Text = string.Empty;
                    txtSublot.Focus();
                }
            }
        }
        #endregion

        #region [AssyLot KeyDown]
        private void txtAssyLot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
        #endregion

        #region [Summary 조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region [Detail 조회]
        private void btnSearch1_Click(object sender, RoutedEventArgs e)
        {
            GetDetail(null);
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
                saveFileDialog.FileName = "CSVExported_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                {
                    //new ExcelExporter().Export(dgHist);
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
                if (e.Cell.Column.Name.Equals("PKG_LOTID"))
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
                    if (dg.CurrentColumn.Name.Equals("PKG_LOTID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                    {
                        _PKG_LOTID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PKG_LOTID"));

                        GetDetail(_PKG_LOTID);
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

        #endregion

        #region [날짜 변경 Event]
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        #endregion

        #region Mehod

        #region [Biz]
        private void Search()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("ASSY_LOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["ASSY_LOTID"] = String.IsNullOrWhiteSpace(txtAssyLot.Text.ToString().Trim()) ? null : txtAssyLot.Text.ToString().Trim();

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SUMMARY_FOR_TESLA_NJ", "INDATA", "OUTDATA", RQSTDT);

                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgHist);

                if (dtRslt.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
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

                txtAssyLot.Text = string.Empty;
                txtAssyLot.Focus();
            }
        }

        private void GetDetail(string sAssyLot)
        {
            try
            {
                //loadingIndicator.Visibility = Visibility.Visible;

                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PALLETID", typeof(string));
                dtRqst.Columns.Add("ASSY_LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("INBOXID", typeof(string));
                dtRqst.Columns.Add("OUTBOXID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PALLETID"] = String.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()) ? null : txtPallet.Text.ToString().Trim();
                dr["ASSY_LOTID"] = sAssyLot;
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["INBOXID"] = String.IsNullOrWhiteSpace(txtInbox.Text.ToString().Trim()) ? null : txtInbox.Text.ToString().Trim();
                dr["OUTBOXID"] = String.IsNullOrWhiteSpace(txtOutbox.Text.ToString().Trim()) ? null : txtOutbox.Text.ToString().Trim();
                dr["SUBLOTID"] = String.IsNullOrWhiteSpace(txtSublot.Text.ToString().Trim()) ? null : txtSublot.Text.ToString().Trim();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CUST_CELL_DATA_FOR_TESLA", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgHist, dtRslt, FrameOperation, true);

                //DataGrid_Summary2();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                //검색조건 초기화
                txtOutbox.Text = string.Empty;
                txtInbox.Text = string.Empty;
                txtSublot.Text = string.Empty;
                txtPallet.Text = string.Empty;

                txtOutbox.Focus();
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
            DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], dac);
        }
        #endregion

    }
}
