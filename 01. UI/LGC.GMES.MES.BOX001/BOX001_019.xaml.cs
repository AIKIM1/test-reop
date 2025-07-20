/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_019 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CommonCombo();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_019()
        {
            InitializeComponent();
            Loaded += BOX001_019_Loaded;
        }

        private void BOX001_019_Loaded(object sender, RoutedEventArgs e)
        {
           
            Initialize();

            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnHold);
            listAuth.Add(btnUnHold);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpDateTo.SelectedDateTime = DateTime.Now;

            string[] sFilter = { "HOLD_YN" };
            _combo.SetCombo(cboHoldYN, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

            string[] sFilter1 = { "CP_CELL_TYPE" };
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType.SelectedIndex = 1;
            
            _combo.SetCombo(cboLotType_Hold, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
          //  cboLotType_Hold.SelectedIndex = 1;

        }
        #endregion


        #region Event

        private void SetEvent()
        {
            Loaded -= BOX001_019_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

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

        private void btnExcelLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
                OpenExcel_Hold();
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dgCellId.ItemsSource == null || dgCellId.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                TextRange textRange = new TextRange(txtHoldNote.Document.ContentStart, txtHoldNote.Document.ContentEnd);

                if (textRange.Text.Trim() == string.Empty)
                {
                    Util.MessageValidation("SFU1341"); //"HOLD 비고를 입력하세요."
                    return;
                }
                //Hold 를 하시게 되면, '포장 구성'을 할 수 없게 됩니다. 계속 진행하시겠습니까?
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3178"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
               Util.MessageConfirm("SFU3178", (result) =>
               {
                   if (result == MessageBoxResult.OK)
                   {
                       setHold(textRange.Text.Substring(0,textRange.Text.LastIndexOf(System.Environment.NewLine)));
                       cboLotType_Hold.IsEnabled = true;
                       Clear();
                       getSearch();
                   }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnUnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellHistory.ItemsSource == null || dgCellHistory.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    Util.MessageValidation("SFU1843");
                    return;
                }

                if (txtNote.Text.Trim() == "")
                {
                    Util.MessageValidation("SFU1404"); //"NOTE를 입력 하세요"
                    return;
                }

                //해제 작업 시작
                int iSelCnt = 0;
                for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
                {
                    if (Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["CHK"].Index).Value) == "1")
                    {
                        iSelCnt = iSelCnt + 1;
                        break;
                    }
                }

                if (iSelCnt == 0)
                {
                    Util.MessageValidation("SFU1651"); //"선택된 항목이 없습니다."
                    return;
                }

                try
                {
                    //BOX001_CONFIRM wndConfirm = new BOX001_CONFIRM();
                    //wndConfirm.FrameOperation = FrameOperation;
                    //wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                    //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));

                    setUnHold(txtNote.Text.Trim());

                    getSearch();
                    txtNote.Text = "";
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // Util.NVC(ex.Message);
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_CONFIRM window = sender as BOX001_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                setUnHold(txtNote.Text.Trim());
            }
            grdMain.Children.Remove(window);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getSearch();
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

            System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            string sCell = Util.NVC(row.Row.ItemArray[dgCellId.Columns["CELLID"].Index]);
           // string sMsg = "[" + sCell + "] CellID를 List에서 삭제하시겠습니까?";
            //삭제하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 선택된 행 삭제
                    dgCellId.IsReadOnly = false;
                    dgCellId.RemoveRow(iRow);
                    dgCellId.IsReadOnly = true;
                }
            });
        }


        private void txtCellId_Hold_KeyDown(object sender, KeyEventArgs e)
        {    
            if (e.Key == Key.Enter)
            {
                if ((string)cboLotType_Hold.SelectedValue == "CELL_RANGE")
                {
                    txtCellId_To.Focus();
                    txtCellId_To.SelectAll();
                }

                else
                    Add_ScanCellid(txtCellId_Hold.Text.Trim());                
            }
        }
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtCellId.Text = string.Empty;

                if (txtLotid.Text.Trim() != "")
                {
                    getSearch();
                }
            }
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtLotid.Text = string.Empty;

                if (txtCellId.Text.Trim() != "")
                {
                    getSearch();
                }
            }
        }

        #endregion


        #region Mehod

        private bool Add_ScanCellid(string sCellId)
        {
            if (string.IsNullOrEmpty(sCellId))
            {
                return false;                
            }
         
            try
            {

                if (dgCellId.GetRowCount() > 0)
                {
                    // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                    for (int i = 0; i < dgCellId.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgCellId.GetCell(i, dgCellId.Columns["CELLID"].Index).Value) == sCellId)
                        {
                            //아래쪽 List에 이미 존재하는 CELL ID입니다.
                            //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3159"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            Util.MessageInfo("SFU3159", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellId_Hold.Focus();
                                    txtCellId_Hold.SelectAll();
                                }
                            });
                            return false;
                        }
                    }
                }
                if (!ChkHoldSubLot(sCellId))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message, (result) =>
                {
                    txtCellId_Hold.Focus();
                    txtCellId_Hold.SelectAll();
                });
                return false;
            }
        }

        private bool Add_ScanLotid(string sLotId)
        {
            if (string.IsNullOrEmpty(sLotId))
            {
                return false;
            }

            // 스프레드에 새로 Row를 추가해야 해서 필요한 변수
            DataTable isDataTable = new DataTable();

            try
            {

                if (dgCellId.GetRowCount() > 0)
                {
                    // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                    for (int i = 0; i < dgCellId.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgCellId.GetCell(i, dgCellId.Columns["LOTID"].Index).Value) == sLotId)
                        {
                            //아래쪽 List에 이미 존재하는 LOT ID입니다.
                            Util.MessageInfo("아래쪽 List에 이미 존재하는 LOT ID입니다", (result) =>
                          {
                              if (result == MessageBoxResult.OK)
                              {
                                  txtCellId_Hold.Focus();
                                  txtCellId_Hold.SelectAll();
                              }
                          });
                            return false;
                        }
                    }
                }
                //스캔CELLID VALIDATION
                if (!ChkHoldLot(sLotId))
                {
                    return false;
                }


                isDataTable = DataTableConverter.Convert(dgCellId.ItemsSource);

                if (isDataTable.Columns.Count <= 0)
                {
                    // 스프레드에 어떤 값도 입력이 되어 있지 않다면, new DataColumn을 사용해 DataTable 구조를 만듬.
                    isDataTable.Columns.Add("LOTID", typeof(string));
                    isDataTable.Columns.Add("CELLID", typeof(string));
                    isDataTable.Columns.Add("CELLID_TO", typeof(string));
                }

                DataRow dRow = isDataTable.NewRow();
                dRow["LOTID"] = sLotId;

                isDataTable.Rows.Add(dRow);
                Util.GridSetData(dgCellId, isDataTable, FrameOperation, true);

                txtLotId_Hold.Focus();
                txtLotId_Hold.SelectAll();

                cboLotType_Hold.IsEnabled = false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message, (result) =>
                {
                    txtLotId_Hold.Focus();
                    txtLotId_Hold.SelectAll();
                });
                return false;
            }
        }
        private void OpenExcel_Hold()
        {

            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel_Hold(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void OpenExcel_Release()
        {
            try
            {
                if (dgCellHistory.Rows.Count == 0)
                {
                    //SFU3347	작업오류 : 조회 후 버튼을 클릭해 주세요.
                    Util.MessageValidation("SFU3347");
                    return;
                }
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        System.Windows.Forms.Application.DoEvents();
                        DataTable dtInfo = DataTableConverter.Convert(dgCellHistory.ItemsSource);

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            //DataRow dataRow = dataTable.NewRow();
                            string sCellID = string.Empty;

                            XLCell cell = sheet.GetCell(rowInx, 0);
                            if (cell != null)
                            {
                                sCellID = Util.NVC(cell.Text);
                                DataRow drInfo = dtInfo.Select("SUBLOTID = '" + sCellID + "'").FirstOrDefault();
                                if (drInfo != null) drInfo["CHK"] = true;

                                System.Windows.Forms.Application.DoEvents();

                            }
                        }
                        Util.GridSetData(dgCellHistory, dtInfo, FrameOperation, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcel_Hold(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];
                //XLSheet sheet = null;

                //for (int i = 0; i < book.Sheets.Count; i++)
                //{
                //    if (sheetName.Equals(book.Sheets[i].Name))
                //        sheet = book.Sheets[i];
                //}

                if (sheet == null)
                {
                    Util.AlertInfo("sheet not exists!");
                    return;
                }

                DataTable dataTable = new DataTable();

                if ((string)cboLotType_Hold.SelectedValue == "SUBLOT")
                {
                    dataTable.Columns.Add("CELLID");
                }

                else if ((string)cboLotType_Hold.SelectedValue == "CELL_RANGE")
                {
                    dataTable.Columns.Add("CELLID");
                    dataTable.Columns.Add("CELLID_TO");
                }

                else
                {
                    return;
                }

                //// extract data
                //DataTable dataTable = new DataTable();
                //Int32 colCnt = 0;
                //for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                //{
                //    //col width setting
                //    if (sheet.GetCell(0, colInx) != null && !sheet.GetCell(0, colInx).Text.Equals(""))
                //    {
                //        dataTable.Columns.Add("C" + colInx, typeof(string));
                //        colCnt++;
                //    }
                //}

                Int32 rowCnt = 0;
                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (sheet.GetCell(rowInx, 0) != null && !sheet.GetCell(rowInx, 0).Text.Equals(""))
                    {
                        DataRow dataRow = dataTable.NewRow();
                        for (int colInx = 0; colInx < dataTable.Columns.Count; colInx++)
                        {
                            XLCell cell = sheet.GetCell(rowInx, colInx);
                            Point cellPoint = new Point(rowInx, colInx);

                            XLRow row = sheet.Rows[1];

                            if (cell != null)
                            {
                                if (string.IsNullOrWhiteSpace(cell.Text))
                                {
                                    Util.MessageInfo(rowInx.ToString());
                                    return;
                                }
                                dataRow[dataTable.Columns[colInx].ColumnName] = cell.Text;
                                rowCnt++;
                            }

                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }


                if (dataTable == null)
                {
                    Util.MessageValidation("SFU1331"); //"Data가 없습니다" >>Data가 존재하지 않습니다.
                    return;
                }

                GetCell_Info(dataTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void LoadExcel_Release(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];
                //XLSheet sheet = null;

                //for (int i = 0; i < book.Sheets.Count; i++)
                //{
                //    if (sheetName.Equals(book.Sheets[i].Name))
                //        sheet = book.Sheets[i];
                //}

                if (sheet == null)
                {
                    Util.AlertInfo("sheet not exists!");
                    return;
                }

                // extract data
                DataTable dataTable = new DataTable();
                Int32 colCnt = 0;
                for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                {
                    //col width setting
                    if (sheet.GetCell(0, colInx) != null && !sheet.GetCell(0, colInx).Text.Equals(""))
                    {
                        dataTable.Columns.Add("C" + colInx, typeof(string));
                        colCnt++;
                    }
                }

                Int32 rowCnt = 0;
                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (sheet.GetCell(rowInx, 0) != null && !sheet.GetCell(rowInx, 0).Text.Equals(""))
                    {
                        DataRow dataRow = dataTable.NewRow();
                        for (int colInx = 0; colInx < colCnt; colInx++)
                        {
                            XLCell cell = sheet.GetCell(rowInx, colInx);
                            Point cellPoint = new Point(rowInx, colInx);

                            XLRow row = sheet.Rows[1];

                            if (cell != null)
                            {
                                dataRow["C" + colInx] = cell.Text;
                                rowCnt++;
                            }

                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }


                if (dataTable == null)
                {
                    Util.MessageValidation("SFU1331"); //"Data가 없습니다" >>Data가 존재하지 않습니다.
                    return;
                }

               // GetCell_Info(dataTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void GetCell_Info(DataTable dtResult)
        {
            try
            {
                if ((string)cboLotType_Hold.SelectedValue == "SUBLOT")
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (!Add_ScanCellid(Util.NVC(dtResult.Rows[i]["CELLID"])))
                        {
                            return;
                        }
                    }
                }

                else if ((string)cboLotType_Hold.SelectedValue == "CELL_RANGE")
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (!ChkHoldSubLotRange(Util.NVC(dtResult.Rows[i]["CELLID"]), Util.NVC(dtResult.Rows[i]["CELLID_TO"])))
                        {
                            return;
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void getSearch()
        {
            try
            {
                if (Util.NVC(cboLotType.SelectedValue) == "SELECT")
                {
                    Util.MessageValidation("1126");
                    return;
                }
                //조회
                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //  RQSTDT.Columns.Add("HOLD_GR_ID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DATE", typeof(DateTime));
                RQSTDT.Columns.Add("HOLD_FLAG", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));
                RQSTDT.Columns.Add("LOT_TYPE_YN", typeof(string));
                RQSTDT.Columns.Add("CELL_TYPE_YN", typeof(string));
                RQSTDT.Columns.Add("CELL_RANGE_TYPE_YN", typeof(string));

                string sHoldYN = Util.NVC(cboHoldYN.SelectedValue);
                if (sHoldYN == string.Empty)
                {
                    sHoldYN = null;
                }
                string sUserName = Util.NVC(txtUserName.Text);
                if (sUserName == string.Empty)
                {
                    sUserName = null;
                }


                DataRow searchCondition = RQSTDT.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                // searchCondition["HOLD_GR_ID"] =  rdoGroupid.IsChecked == true ? sHoldid : null;
                searchCondition["LOTID"] = string.IsNullOrWhiteSpace(txtLotid.Text)? null :txtLotid.Text;
                searchCondition["SUBLOTID"] = string.IsNullOrWhiteSpace(txtCellId.Text)? null : txtCellId.Text;

                if (string.IsNullOrWhiteSpace(txtLotid.Text) && string.IsNullOrWhiteSpace(txtCellId.Text))
                {
                    searchCondition["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00"; //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
                    searchCondition["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59"; //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";
                    searchCondition["USERNAME"] = sUserName;
                    searchCondition["HOLD_FLAG"] = sHoldYN;

                    if ((string)cboLotType.SelectedValue == "LOT")
                        searchCondition["LOT_TYPE_YN"] = "Y";

                    else if ((string)cboLotType.SelectedValue == "SUBLOT")
                        searchCondition["CELL_TYPE_YN"] = "Y";

                    else if ((string)cboLotType.SelectedValue == "CELL_RANGE")
                        searchCondition["CELL_RANGE_TYPE_YN"] = "Y";  
                }                

                //searchCondition["HOLD_FLAG"] = sHoldid == null ? sHoldYN : null; 
                RQSTDT.Rows.Add(searchCondition);


                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_HOLD_LIST_CP", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.AlertInfo(ex.Message);
                        return;
                    }
                    Util.GridSetData(dgCellHistory, dtResult, FrameOperation,true);
                });
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Sublot HOLD Validation
        /// </summary>
        /// <param name="sSubLot"></param>
        /// <returns></returns>
        private bool ChkHoldSubLot(string sSubLot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSubLot;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_ACT_CHK_HOLD_SUBLOT", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows == null || dtResult.Rows.Count < 1)
                    return false;

                // 스프레드에 새로 Row를 추가해야 해서 필요한 변수
                DataTable isDataTable = DataTableConverter.Convert(dgCellId.ItemsSource);

                if (isDataTable.Columns.Count <= 0)
                {
                    // 스프레드에 어떤 값도 입력이 되어 있지 않다면, new DataColumn을 사용해 DataTable 구조를 만듬.
                    isDataTable.Columns.Add("LOTID", typeof(string));
                    isDataTable.Columns.Add("CELLID", typeof(string));
                    isDataTable.Columns.Add("CELLID_TO", typeof(string));
                }

                DataRow dRow = isDataTable.NewRow();
                dRow["CELLID"] = dtResult.Rows[0]["SUBLOTID"];
                dRow["LOTID"] = dtResult.Rows[0]["PROD_LOTID"];

                isDataTable.Rows.Add(dRow);
                Util.GridSetData(dgCellId, isDataTable, FrameOperation, true);

                txtCellId_Hold.Focus();
                txtCellId_Hold.SelectAll();

                cboLotType_Hold.IsEnabled = false;
                return true;
            }
            catch(Exception ex)
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                Util.MessageInfo(ex.Message, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellId_Hold.Focus();
                        txtCellId_Hold.SelectAll();
                    }
                });
                return false;
            }
        }

        private bool ChkHoldSubLotRange(string sSubLot, string sSubLot_To)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSubLot;
                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_ACT_CHK_HOLD_SUBLOT", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows == null || dtResult.Rows.Count < 1)
                    return false;
                
                string sCellId = Util.NVC(dtResult.Rows[0]["SUBLOTID"]);
                string sLotId = Util.NVC(dtResult.Rows[0]["PROD_LOTID"]);

                RQSTDT.Clear();
                dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sSubLot_To;
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("BR_ACT_CHK_HOLD_SUBLOT", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows == null || dtResult.Rows.Count < 1)
                    return false;

                string sCellId_To = Util.NVC(dtResult.Rows[0]["SUBLOTID"]);
                string sLotId_To = Util.NVC(dtResult.Rows[0]["PROD_LOTID"]);

                if (sLotId != sLotId_To)
                {
                    // 동일한 LOT을 대상으로 HOLD 가능합니다.
                    Util.MessageValidation("SFU3493");
                    return false; 
                }

                // 스프레드에 새로 Row를 추가해야 해서 필요한 변수
                DataTable isDataTable = DataTableConverter.Convert(dgCellId.ItemsSource);

                if (isDataTable.Columns.Count <= 0)
                {
                    // 스프레드에 어떤 값도 입력이 되어 있지 않다면, new DataColumn을 사용해 DataTable 구조를 만듬.
                    isDataTable.Columns.Add("LOTID", typeof(string));
                    isDataTable.Columns.Add("CELLID", typeof(string));
                    isDataTable.Columns.Add("CELLID_TO", typeof(string));
                }

                DataRow dRow = isDataTable.NewRow();
                dRow["CELLID"] = sCellId;
                dRow["CELLID_TO"] = sCellId_To;
                dRow["LOTID"] = sLotId;
                isDataTable.Rows.Add(dRow);                   
               
                Util.GridSetData(dgCellId, isDataTable, FrameOperation, true);

                txtCellId_Hold.Text = string.Empty;
                txtCellId_To.Text = string.Empty;

                txtCellId_Hold.Focus();
                txtCellId_Hold.SelectAll();

                cboLotType_Hold.IsEnabled = false;
                return true;
            }
            catch (Exception ex)
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                Util.MessageInfo(ex.Message, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellId_Hold.Focus();
                        txtCellId_Hold.SelectAll();
                    }
                });
                return false;
            }
        }

        private bool ChkHoldLot(string sLot)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLot;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_ACT_CHK_HOLD_LOT", "RQSTDT", "SEL_WIP", RQSTDT);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellId_Hold.Focus();
                        txtCellId_Hold.SelectAll();
                    }
                });
                return false;
            }
        }

        /// <summary>
        /// Hold 처리
        /// </summary>
        /// <param name="sNote"></param>
        private void setHold(string sNote)
        {
            try
            {

                if ((string)cboLotType_Hold.SelectedValue == "LOT")
                {
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("ACTION_USERID", typeof(string));

                    DataTable inLotTable = indataSet.Tables.Add("INLOT");
                    inLotTable.Columns.Add("LOTID", typeof(string));
                    inLotTable.Columns.Add("HOLD_NOTE", typeof(string));
                    inLotTable.Columns.Add("RESNCODE", typeof(string));
                    inLotTable.Columns.Add("HOLD_CODE", typeof(string));
                    inLotTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

                    DataRow inData = inDataTable.NewRow();
                    inData["SRCTYPE"] = "UI";
                    inData["LANGID"] = LoginInfo.LANGID;
                    inData["IFMODE"] = "OFF"; //설비에서 HOLD 처리하는지 여부 : UI-OFF
                    inData["USERID"] = txtWorker.Tag as string;
                    inData["ACTION_USERID"] = txtWorker.Tag as string;
                    inDataTable.Rows.Add(inData);


                    for (int i = 0; i < dgCellId.GetRowCount(); i++)
                    {
                        DataRow inLot = inLotTable.NewRow();
                        inLot["LOTID"] = Util.NVC(dgCellId.GetCell(i, dgCellId.Columns["LOTID"].Index).Value);
                        inLot["HOLD_NOTE"] = sNote;
                        inLot["RESNCODE"] = "PH99H99";
                        inLot["HOLD_CODE"] = string.Empty;
                        inLot["UNHOLD_SCHD_DATE"] = string.Empty;
                        inLotTable.Rows.Add(inLot);
                    }

                    loadingIndicator.Visibility = Visibility.Visible;
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_LOT_CP", "INDATA,INLOT", null, indataSet);
                }
                else
                {
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("HOLD_NOTE", typeof(string));
                    inDataTable.Columns.Add("HOLD_CODE", typeof(string));
                    inDataTable.Columns.Add("HOLD_TYPE", typeof(string));
                    inDataTable.Columns.Add("UNHOLD_SCHDDATE", typeof(string));

                    DataTable inSubLotTable = indataSet.Tables.Add("INSUBLOT");
                    inSubLotTable.Columns.Add("SUBLOTID", typeof(string));

                    DataTable inSubLotRangeTable = indataSet.Tables.Add("INSUBLOT_RANGE");
                    inSubLotRangeTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                    inSubLotRangeTable.Columns.Add("TO_SUBLOTID", typeof(string));
                    
                    DataRow inData = inDataTable.NewRow();
                    inData["SRCTYPE"] = "UI";
                    inData["USERID"] = txtWorker.Tag as string;
                    inData["IFMODE"] = "OFF"; //설비에서 HOLD 처리하는지 여부 : UI-OFF
                    inData["HOLD_NOTE"] = sNote;
                    inData["HOLD_CODE"] = "n/a";
                    inData["HOLD_TYPE"] = cboLotType_Hold.SelectedValue;
                    inData["UNHOLD_SCHDDATE"] = null;
                    inDataTable.Rows.Add(inData);

                    if ((string)cboLotType_Hold.SelectedValue == "CELL_RANGE")
                    {
                        for (int i = 0; i < dgCellId.GetRowCount(); i++)
                        {
                            DataRow inSubLotRange = inSubLotRangeTable.NewRow();
                            inSubLotRange["FROM_SUBLOTID"] = Util.NVC(dgCellId.GetCell(i, dgCellId.Columns["CELLID"].Index).Value);
                            inSubLotRange["TO_SUBLOTID"] = Util.NVC(dgCellId.GetCell(i, dgCellId.Columns["CELLID_TO"].Index).Value);
                            inSubLotRangeTable.Rows.Add(inSubLotRange);
                        }
                    }

                    else if((string)cboLotType_Hold.SelectedValue == "SUBLOT")
                    {
                        for (int i = 0; i < dgCellId.GetRowCount(); i++)
                        {
                            DataRow inSubLot = inSubLotTable.NewRow();
                            inSubLot["SUBLOTID"] = Util.NVC(dgCellId.GetCell(i, dgCellId.Columns["CELLID"].Index).Value);
                            inSubLotTable.Rows.Add(inSubLot);
                        }
                    }

                    loadingIndicator.Visibility = Visibility.Visible;
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_SUBLOT_BY_RANGE", "INDATA,INSUBLOT,INSUBLOT_RANGE", null, indataSet);
                }
               
                Util.MessageInfo("SFU1267"); //"정상적으로 HOLD 등록 되었습니다."               
               

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

        private void setUnHold(string sNote)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("HOLD_TYPE", typeof(string));
                inDataTable.Columns.Add("UNHOLD_NOTE", typeof(string));
                inDataTable.Columns.Add("UNHOLD_CODE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                
                DataSet indataSetLot = new DataSet();
                DataTable inDataLotTable = indataSetLot.Tables.Add("INDATA");
                inDataLotTable.Columns.Add("SRCTYPE", typeof(string));
                inDataLotTable.Columns.Add("USERID", typeof(string));
                inDataLotTable.Columns.Add("IFMODE", typeof(string));

                DataRow inDataLot = inDataLotTable.NewRow();
                inDataLot["SRCTYPE"] = "UI";//LoginInfo.USERID;
                inDataLot["USERID"] = txtWorker.Tag as string;
                inDataLot["IFMODE"] = "OFF";
                inDataLotTable.Rows.Add(inDataLot);

                DataTable inSubLotTable = indataSet.Tables.Add("INSUBLOT");
                inSubLotTable.Columns.Add("SUBLOTID", typeof(string));

                DataTable inSubLotRangeTable = indataSet.Tables.Add("INSUBLOT_RANGE");
                inSubLotRangeTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                inSubLotRangeTable.Columns.Add("TO_SUBLOTID", typeof(string));

                DataTable inLotTable = indataSetLot.Tables.Add("INLOT");
                inLotTable.Columns.Add("LOTID", typeof(string));
                inLotTable.Columns.Add("RESNCODE", typeof(string));
                inLotTable.Columns.Add("UNHOLD_NOTE", typeof(string));
                inLotTable.Columns.Add("UNHOLD_CODE", typeof(string));

                string sLotType = string.Empty;

                for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
                {
                    if (Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["CHK"].Index).Value) == "1"
                        && Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["LOTTYPE"].Index).Value) == "CELL")
                    {
                        DataRow inSubLot = inSubLotTable.NewRow();
                        inSubLot["SUBLOTID"] = Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["SUBLOTID"].Index).Value);
                        inSubLotTable.Rows.Add(inSubLot);

                        sLotType = "SUBLOT";
                    }
                    else if(Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["CHK"].Index).Value) == "1"
                        && Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["LOTTYPE"].Index).Value) == "CELL RANGE")
                    {
                        string[] strLotRange = Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["SUBLOTID"].Index).Value).Split(new string[] { "-" }, StringSplitOptions.None);
                        DataRow inSubRagneLot = inSubLotRangeTable.NewRow();
                        inSubRagneLot["FROM_SUBLOTID"] = strLotRange[0];
                        inSubRagneLot["TO_SUBLOTID"] = strLotRange[1];
                        inSubLotRangeTable.Rows.Add(inSubRagneLot);

                        sLotType = "CELL_RANGE";
                    }
                    else if (Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["CHK"].Index).Value) == "1"
                        && Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["LOTTYPE"].Index).Value) == "LOT")
                    {
                        DataRow inLot = inLotTable.NewRow();
                        inLot["LOTID"] = Util.NVC(dgCellHistory.GetCell(i, dgCellHistory.Columns["LOTID"].Index).Value);
                        inLot["RESNCODE"] = "R001";
                        inLot["UNHOLD_NOTE"] = sNote;
                        inLot["UNHOLD_CODE"] = string.Empty;
                        inLotTable.Rows.Add(inLot);

                        sLotType = "LOT";
                    }
                }

                DataRow inData = inDataTable.NewRow();
                inData["USERID"] = txtWorker.Tag as string;
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["HOLD_TYPE"] = sLotType;
                inData["UNHOLD_NOTE"] = sNote;
                inData["UNHOLD_CODE"] = "n/a";
                inDataTable.Rows.Add(inData);

                loadingIndicator.Visibility = Visibility.Visible;

                if (inSubLotTable.Rows.Count > 0 || inSubLotRangeTable.Rows.Count > 0)
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNHOLD_SUBLOT_BY_RANGE", "INDATA,INSUBLOT,INSUBLOT_RANGE", null, indataSet);

                if (inLotTable.Rows.Count > 0)
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNHOLD_LOT_CP", "INDATA,INLOT", null, indataSetLot);

                Util.MessageInfo("SFU1268"); //"정상적으로 HOLD 해제 되었습니다."
                //getSearch();
                //txtNote.Text = "";

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // throw ex;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        #endregion

        private void dgCellHistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (dgCellHistory.CurrentRow == null || dgCellHistory.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (dgCellHistory.CurrentColumn.Name == "CHK")
                {
                    if (Util.NVC(dgCellHistory.GetCell(dgCellHistory.CurrentRow.Index, dgCellHistory.Columns["CHK"].Index).Value) == "1")
                    {
                        DataTableConverter.SetValue(dgCellHistory.Rows[dgCellHistory.CurrentRow.Index].DataItem, "CHK", false);
                    }
                    else
                    {
                        if (Util.NVC(dgCellHistory.GetCell(dgCellHistory.CurrentRow.Index, dgCellHistory.Columns["HOLDTYPE"].Index).Value) == "GMES"
                            //&& (Util.NVC(dgCellHistory.GetCell(dgCellHistory.CurrentRow.Index, dgCellHistory.Columns["LOTTYPE"].Index).Value) == "SUBLOT" 
                            //  || Util.NVC(dgCellHistory.GetCell(dgCellHistory.CurrentRow.Index, dgCellHistory.Columns["LOTTYPE"].Index).Value) == "CELL_RANGE"
                            //  || Util.NVC(dgCellHistory.GetCell(dgCellHistory.CurrentRow.Index, dgCellHistory.Columns["LOTTYPE"].Index).Value) == "LOT")
                            && Util.NVC(dgCellHistory.GetCell(dgCellHistory.CurrentRow.Index, dgCellHistory.Columns["HOLD_FLAG"].Index).Value) == "Y")
                        {
                            DataTableConverter.SetValue(dgCellHistory.Rows[dgCellHistory.CurrentRow.Index].DataItem, "CHK", true);
                        }        
                    }
                } 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgCellHistory.CurrentRow = null;
            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void txtCellId_Hold_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((string)cboLotType_Hold.SelectedValue == "CELL_RANGE") return;

            else if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Add_ScanCellid(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void dgCellHistory_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgCellHistory.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgCellHistory.Rows[i].DataItem, "CHK", true);
                }
            }

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgCellHistory.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgCellHistory.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void btnExcelLoad_Release_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
                OpenExcel_Release();
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void cboLotType_Hold_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtLotId_Hold.Text = string.Empty;
            txtCellId_Hold.Text = string.Empty;
            txtCellId_To.Text = string.Empty;

            if ((string)cboLotType_Hold.SelectedValue == "LOT")
            {
                lblLotId.Visibility = Visibility.Visible;
                lblCell_From.Visibility = Visibility.Collapsed;
                lblCell_To.Visibility = Visibility.Collapsed;            

                dgCellId.Columns["LOTID"].Visibility = Visibility.Visible;
                dgCellId.Columns["CELLID"].Visibility = Visibility.Collapsed;
                dgCellId.Columns["CELLID_TO"].Visibility = Visibility.Collapsed;
            }

            else if ((string)cboLotType_Hold.SelectedValue == "SUBLOT")
            {
                lblLotId.Visibility = Visibility.Collapsed;
                lblCell_From.Visibility = Visibility.Visible;
                lblCell_To.Visibility = Visibility.Collapsed;
                
                dgCellId.Columns["LOTID"].Visibility = Visibility.Visible;
                dgCellId.Columns["CELLID"].Visibility = Visibility.Visible;
                dgCellId.Columns["CELLID_TO"].Visibility = Visibility.Collapsed;
            }

            else if ((string)cboLotType_Hold.SelectedValue == "CELL_RANGE")
            {
                lblLotId.Visibility = Visibility.Collapsed;
                lblCell_From.Visibility = Visibility.Visible;
                lblCell_To.Visibility = Visibility.Visible;
                
                dgCellId.Columns["LOTID"].Visibility = Visibility.Visible;
                dgCellId.Columns["CELLID"].Visibility = Visibility.Visible;
                dgCellId.Columns["CELLID_TO"].Visibility = Visibility.Visible;
            }

            else
            {
                lblLotId.Visibility = Visibility.Visible;
                lblCell_From.Visibility = Visibility.Visible;
                lblCell_To.Visibility = Visibility.Visible;

                dgCellId.Columns["LOTID"].Visibility = Visibility.Collapsed;
                dgCellId.Columns["CELLID"].Visibility = Visibility.Visible;
                dgCellId.Columns["CELLID_TO"].Visibility = Visibility.Collapsed;
            }
        }

        private void txtLotId_Hold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Add_ScanLotid(txtLotId_Hold.Text.Trim());
            }
        }

        private void txtLotId_Hold_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Add_ScanLotid(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            cboLotType_Hold.IsEnabled = true;

            txtHoldNote.Document.Blocks.Clear();

            txtLotId_Hold.Text = string.Empty;
            txtCellId_Hold.Text = string.Empty;
            txtCellId_To.Text = string.Empty;

            Util.gridClear(dgCellId);
        }

        private void txtCellId_To_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtCellId_Hold.Text))
                    {
                        // 시작할 CellID 먼저 입력/스캔하세요.
                        Util.MessageValidation("SFU3481", (result) =>                        
                        {
                            txtCellId_Hold.Focus();
                            txtCellId_Hold.SelectAll();
                        });
                        return;
                    }

                    string sHoldId_From = txtCellId_Hold.Text.Trim();
                    string sHoldId_To = txtCellId_To.Text.Trim();

                    int iIdx = 0;
                    int iIdx_To = 0;

                    for (int cnt = 0; cnt < sHoldId_From.Length; cnt++)
                    {
                        if (char.IsNumber(sHoldId_From[cnt]) != true)
                        {
                            iIdx = cnt+1;
                        }
                    }

                    for (int cnt = 0; cnt < sHoldId_To.Length; cnt++)
                    {
                        if (char.IsNumber(sHoldId_To[cnt]) != true)
                        {
                            iIdx_To = cnt+1;
                        }
                    }

                    if (iIdx != iIdx_To)
                    {
                        //CellID를 다시 확인해주세요. 연속된 Cell만 작업 가능합니다. -> SFU3482
                        Util.MessageValidation("SFU3482");
                        return;
                    }

                    string sValue = sHoldId_From.Substring(0, iIdx);

                    if (sValue != sHoldId_To.Substring(0, iIdx))
                    {
                        Util.MessageValidation("SFU3482");
                        return;
                    }

                    //string sStart = sHoldId_From.Substring(iIdx);
                    //string sEnd = sHoldId_To.Substring(iIdx);

                    int iStart, iEnd;
                    if (!int.TryParse(sHoldId_From.Substring(iIdx), out iStart) || !int.TryParse(sHoldId_To.Substring(iIdx), out iEnd))
                    {
                        Util.MessageValidation("SFU3482");
                        return;
                    }

                    if (iStart > iEnd)
                    {
                        Util.MessageValidation("SFU3482");
                        return;
                    }

                    ChkHoldSubLotRange(txtCellId_Hold.Text, txtCellId_To.Text);

                    //for (int i = int.Parse(sStart); i <= int.Parse(sEnd); i++)
                    //{
                    //    if(!Add_ScanCellid(sValue + i))
                    //        return;
                    //}
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnExcelSample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();
             
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Hold_Lot_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "STARTCELLID"; // ObjectDic.Instance.GetObjectName("CELLID");                 
                    sheet[0, 1].Value = "ENDCELLID";
                    sheet[0, 0].Style = sheet[0, 1].Style = styel;
                    sheet.Columns[0].Width = sheet.Columns[1].Width = 2000;
                 


                    c1XLBook1.Save(od.FileName);
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
