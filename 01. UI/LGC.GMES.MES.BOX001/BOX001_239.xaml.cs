/*************************************************************************************
 Created Date : 2024.08.06
      Creator : 이병윤
   Decription : IM사 데이터 조회(CSV)_E20240717-000944
--------------------------------------------------------------------------------------
 [Change History]
  2024.08.06  이병윤 : Initial Created.
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
    public partial class BOX001_239 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        CheckBox _chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private string _PLLT = string.Empty;
        int CurrentTabPage;

        public BOX001_239()
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

        DataGridRowHeaderPresenter pre2 = new DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
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

        #region [Shipment KeyDown]
        private void txtShipment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
        #endregion

        #region [Shipment 조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region [텝 선택 ]
        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentTabPage = Tab.SelectedIndex;
            Util.gridClear(dgHist);
        }
        #endregion

        #region [확장텝 전체 선택 및 해제]

        private void dgSearchT01_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchT01.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgSearchT01.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchT01.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchT01.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #region [Shipped텝 전체 선택 및 해제]
        
        private void dgSearchT02_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK2"))
                        {
                            pre2.Content = _chkAll;
                            e.Column.HeaderPresenter.Content = pre2;
                            _chkAll.Checked -= new RoutedEventHandler(checkAll_Checked2);
                            _chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked2);
                            _chkAll.Checked += new RoutedEventHandler(checkAll_Checked2);
                            _chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked2);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll_Checked2(object sender, RoutedEventArgs e)
        {

            if ((bool)_chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchT02.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "CHK2")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "CHK2")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgSearchT02.Rows[i].DataItem, "CHK2", true);
                }
            }
        }
        private void checkAll_Unchecked2(object sender, RoutedEventArgs e)
        {
            if (!(bool)_chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchT02.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchT02.Rows[i].DataItem, "CHK2", false);
                }
            }
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

        private void dgSearchT01_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                C1DataGrid dg = sender as C1DataGrid;
                if (dg.CurrentColumn != null)
                {
                    if (!dg.CurrentColumn.Name.Equals("CHK") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                    {
                        _PLLT = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PLLT"));

                        GetDetail(_PLLT);
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

        private void dgSearchT02_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                C1DataGrid dg = sender as C1DataGrid;
                if (dg.CurrentColumn != null)
                {
                    if (!dg.CurrentColumn.Name.Equals("CHK2") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                    {
                        _PLLT = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PLLT"));

                        GetDetail(_PLLT);
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

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSearchT01_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                loadingIndicator.Visibility = Visibility.Visible;
        }

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

        #region [전체 CSV로 다운로더]
        private void btnCSV_All_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string SaveFolder = string.Empty;
                int rowCnt = 0;
                if(CurrentTabPage == 0)
                {
                    rowCnt = dgSearchT01.GetRowCount();
                }
                else
                {
                    rowCnt = dgSearchT02.GetRowCount();
                }
                if (rowCnt == 0)
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
        #endregion

        private void btnCsvUpload_Click(object sender, RoutedEventArgs e)
        {
            if((CurrentTabPage == 0 && dgSearchT01.GetRowCount() == 0)
                ||(CurrentTabPage == 1 && dgSearchT02.GetRowCount() == 0))
            {
                Util.MessageValidation("SFU1636");
                return;
            }
            if(CurrentTabPage > 1)
            {
                Util.MessageValidation("SFU1636");
                return;
            }

            // 업로더 처리
            UploadCsv();

        }

        #endregion [Event End]


        #region [Mehod Start]

        #region [Biz]
        private void Search()
        {
            try
            {
                string sStart_date = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("SHIPMENTNO", typeof(String));
                RQSTDT.Columns.Add("PALLETID", typeof(String));
                RQSTDT.Columns.Add("BOX_RCV_ISS_STAT_CODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["SHIPMENTNO"] = String.IsNullOrWhiteSpace(txtShipment.Text.ToString().Trim()) ? null : txtShipment.Text.ToString().Trim();
                dr["PALLETID"] = String.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()) ? null : txtPallet.Text.ToString().Trim();
                dr["BOX_RCV_ISS_STAT_CODE"] = CurrentTabPage == 0 ? "END_RECEIVE" : "SHIPPED";
                RQSTDT.Rows.Add(dr);

                Util.gridClear(dgHist);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIPMENTNO_6J_LABEL", "INDATA", "OUTDATA", RQSTDT);
                if(CurrentTabPage == 0 )
                {
                    // 확정탭에서 조회
                    Util.GridSetData(dgSearchT01, dtRslt, FrameOperation, true);

                    //txtShipmentT01.Text = string.Empty;

                    if (dtRslt.Rows.Count > 0)
                    {
                        //txtShipmentT01.Text = Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[0].DataItem, "SHIPMENTNO"));
                        DataGridAggregate.SetAggregateFunctions(dgSearchT01.Columns["LABEL_6J_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                    }
                }
                else
                {
                    // SHIPPED텝에서 조회
                    Util.GridSetData(dgSearchT02, dtRslt, FrameOperation, true);

                    //txtShipmentT02.Text = string.Empty;

                    if (dtRslt.Rows.Count > 0)
                    {
                        //txtShipmentT02.Text = Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[0].DataItem, "SHIPMENTNO"));
                        DataGridAggregate.SetAggregateFunctions(dgSearchT02.Columns["LABEL_6J_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
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
            }
        }

        private void GetDetail(string palletID)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

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
                dr["PALLETID"] = palletID;
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
            }
        }
        #endregion

        private void MakeCsv(string sSaveDirectory)
        {
            try
            {
                if (CurrentTabPage == 0)
                {
                    for (int i = 0; i < dgSearchT01.Rows.Count - 1; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "CHK")).Equals(bool.TrueString))
                        {
                            string _Pallet_ID = string.Empty;
                            string _SaveFileName = sSaveDirectory;

                            _Pallet_ID = DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "PLLT") as string;

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
                }
                else if(CurrentTabPage == 1)
                {
                    for (int i = 0; i < dgSearchT02.Rows.Count - 1; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "CHK2")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "CHK2")).Equals(bool.TrueString))
                        {
                            string _Pallet_ID = string.Empty;
                            string _SaveFileName = sSaveDirectory;

                            _Pallet_ID = DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "PLLT") as string;

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
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UploadCsv()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LABEL_6J", typeof(string));
                dtRqst.Columns.Add("LABEL_6J_QTY", typeof(decimal));
                dtRqst.Columns.Add("PLLT", typeof(string));
                dtRqst.Columns.Add("NO", typeof(string));
                int iNo = 1;
                if (CurrentTabPage == 0)
                {
                    for (int i = 0; i < dgSearchT01.Rows.Count - 1; i++)
                    {

                        if (Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "CHK")).Equals(bool.TrueString))
                        {

                            DataRow dr = dtRqst.NewRow();
                            dr["LABEL_6J"] = Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "LABEL_6J"));
                            dr["LABEL_6J_QTY"] = Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "LABEL_6J_QTY"));
                            dr["PLLT"] = Util.NVC(DataTableConverter.GetValue(dgSearchT01.Rows[i].DataItem, "PLLT"));
                            dr["NO"] = iNo;
                            dtRqst.Rows.Add(dr);
                            iNo++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dgSearchT02.Rows.Count - 1; i++)
                    {

                        if (Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "CHK2")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "CHK2")).Equals(bool.TrueString))
                        {

                            DataRow dr = dtRqst.NewRow();
                            dr["LABEL_6J"] = Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "LABEL_6J"));
                            dr["LABEL_6J_QTY"] = Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "LABEL_6J_QTY"));
                            dr["PLLT"] = Util.NVC(DataTableConverter.GetValue(dgSearchT02.Rows[i].DataItem, "PLLT"));
                            dr["NO"] = iNo;
                            dtRqst.Rows.Add(dr);
                            iNo++;
                        }
                    }
                }

                BOX001_239_CSV_UPLOAD puCsvUpload = new BOX001_239_CSV_UPLOAD();
                puCsvUpload.FrameOperation = FrameOperation;

                if (puCsvUpload != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = dtRqst;
                    C1WindowExtension.SetParameters(puCsvUpload, Parameters);

                    puCsvUpload.Closed += new EventHandler(puCsvUpload_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puCsvUpload);
                    puCsvUpload.BringToFront();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void puCsvUpload_Closed(object sender, EventArgs e)
        {
            BOX001_239_CSV_UPLOAD popup = sender as BOX001_239_CSV_UPLOAD;
            this.grdMain.Children.Remove(popup);
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
        }

        #endregion [Method]

        #region [Func]
        private void DataGrid_Summary()
        {
            DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
            DataGridAggregateSum dagsum = new DataGridAggregateSum();
            if(CurrentTabPage == 0)
            {
                dagsum.ResultTemplate = dgSearchT01.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgSearchT01.Columns["LABEL_6J_QTY"], dac);
            }
            else
            {
                dagsum.ResultTemplate = dgSearchT02.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgSearchT02.Columns["LABEL_6J_QTY"], dac);
            }
            
        }


        #endregion [Func]


    }
}
