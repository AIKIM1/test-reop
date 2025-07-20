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
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_008_CHANGE_EXL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_008_CHANGE_EXL()
        {
            InitializeComponent();
            Loaded += BOX001_008_CHANGE_EXL_Loaded;
        }

        private void BOX001_008_CHANGE_EXL_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_008_CHANGE_EXL_Loaded;

            ////작업자 Combo Set.
            //String[] sFilter2 = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, Process.CELL_BOXING };
            //combo.SetCombo(cboProcUser, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "PROC_USER");

        }


        #endregion

        #region Initialize

        #endregion

        #region Event


        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            //Open2ExcelFile();
            this.Cursor = System.Windows.Input.Cursors.Wait;

            // System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => OpenExcel()));
            OpenExcel();

            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }


        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            ///List에서 삭제하시겠습니까? >> 삭제하시겠습니까?
           // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, ControlsLibrary.MessageBoxIcon.None, (result) =>
           Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 선택된 행 삭제
                    dgCellList.IsReadOnly = false;
                    dgCellList.RemoveRow(iRow);
                    dgCellList.IsReadOnly = true;

                    int iCnt = 0;
                    for (int x = dgCellList.TopRows.Count; x < dgCellList.Rows.Count; x++)
                    {
                        iCnt = iCnt + 1;
                        DataTableConverter.SetValue(dgCellList.Rows[x].DataItem, "SEQ_NO", iCnt);
                    }

                    txtCell_Cnt.Text = dgCellList.GetRowCount().ToString("#,##0");
                }

            });
        }

        /// <summary>
        /// 교체불가 일괄삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBatchDel_Click(object sender, RoutedEventArgs e)
        {

            int i = dgCellList.TopRows.Count;
            while (i < dgCellList.Rows.Count)
            {
                if (Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["CHANGEABLE_YN"].Index).Value) == "불가")
                {
                    //GridUtil.RemoveRow(spdCellList, i);
                    // 선택된 행 삭제
                    dgCellList.IsReadOnly = false;
                    dgCellList.RemoveRow(i);
                    dgCellList.IsReadOnly = true;
                }
                else
                {
                    i++;
                }
            }

            int iCnt = 0;
            for (int x = dgCellList.TopRows.Count; x < dgCellList.Rows.Count; x++)
            {
                iCnt = iCnt + 1;
                DataTableConverter.SetValue(dgCellList.Rows[x].DataItem, "SEQ_NO", iCnt);
            }

            txtCell_Cnt.Text = dgCellList.GetRowCount().ToString("#,##0");
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            dgCellList.ItemsSource = null;
            txtCell_Cnt.Text = "0";
        }

        /// <summary>
        /// 교체 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {

            for (int iRow = dgCellList.TopRows.Count; iRow < dgCellList.Rows.Count; iRow++)
            {
                if (Util.NVC(dgCellList.GetCell(iRow, dgCellList.Columns["CHANGEABLE_YN"].Index).Value) == "불가")
                {
                    //{%1}행에 교체 불가 Cell이 있습니다. \r\n삭제 후 진행 하십시오
                    Util.MessageInfo("SFU3250", iRow + 1 - dgCellList.TopRows.Count);
                    dgCellList.ScrollIntoView(0, iRow);
                    return;
                }
            }

            if (txtReason.Text.Trim() == "")
            {
                Util.MessageValidation("SFU3251"); //"변경사유는 필수 입력항목입니다."
                txtReason.Focus();
                txtReason.SelectAll();
                return;
            }

            //string sUser = Util.NVC(cboProcUser.SelectedValue);
            //if (sUser == "" || sUser == "SELECT")
            //{
            //    Util.AlertInfo("작업자를 선택해주세요.");
            //    return;
            //}

            Save2ChangeData();

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion


        #region Mehod

        private void OpenExcel()
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
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcel(Stream excelFileStream, int sheetNo)
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
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                  //  Util.AlertInfo("sheet not exists!");
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

                GetCell_Info(dataTable);

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
                int iData_Cnt = dtResult.Rows.Count;
                if (iData_Cnt < 1)
                {
                    Util.AlertInfo("SFU1331"); //"Data가 없습니다" >>Data가 존재하지 않습니다.
                    return;
                }

                txtCell_Cnt.Text = iData_Cnt.ToString("#,##0");

                int iOldCell_Col = dgCellList.Columns["NG_CELLID"].Index;
                int iNewCell_Col = dgCellList.Columns["GOOD_CELLID"].Index;

                int FindRow = -1;
                int FindCol = -1;

                //lblMSG.Text = "Excel File에서 Cell 정보를 읽는 중 입니다.\r\n\r\n잠시만 기다려 주십시오.";

                for (int i = 0; i < iData_Cnt; i++)
                {
                    //Application.DoEvents();
                    //pgbCnt.Value = i + 1;
                    //lblCnt.Text = "[" + (i + 1) + " / " + iData_Cnt + "]";

                    string sOldCellID = dtResult.Columns.Count >= 1 ? Util.NVC(dtResult.Rows[i][0]) : string.Empty;
                    string sNewCellID = dtResult.Columns.Count >= 2 ? Util.NVC(dtResult.Rows[i][1]) : string.Empty;
                    string s2DBCR = dtResult.Columns.Count >= 3 ? Util.NVC(dtResult.Rows[i][2]) : string.Empty;

                    if (sOldCellID.Equals(""))
                    {
                        int iOldCnt = i + 1;
                        string sOldCnt = Convert.ToString(iOldCnt);
                        Util.MessageValidation("SFU1575", new object[] { sOldCnt }); //"불량 Cell 정보 중 {0} 행의 Cell ID가 공백 입니다. \r\n확인 후 다시 하십시오"
                        dgCellList.ItemsSource = null;
                        return;
                    }
                    else if (sNewCellID.Equals(""))
                    {
                        int iNewCnt = i + 1;
                        string sNewCnt = Convert.ToString(iNewCnt);
                        Util.MessageValidation("SFU1460", new object[] { sNewCnt }); //"교체 Cell 정보 중 {0} 행의 Cell ID가 공백 입니다. \r\n확인 후 다시 하십시오"
                        dgCellList.ItemsSource = null;
                        return;
                    }

                    for (int iRow = dgCellList.TopRows.Count; iRow < dgCellList.Rows.Count; iRow++)
                    {
                        int nRow = iRow + 1 - dgCellList.TopRows.Count;
                        int nCnt = i + 1;
                        string sRow = Convert.ToString(nRow);
                        string sCnt = Convert.ToString(nCnt);

                        if (sOldCellID == Util.NVC(dgCellList.GetCell(iRow, iOldCell_Col).Value))
                        {
                            Util.MessageValidation("SFU1576", new object[] { sRow, sCnt }); //"불량 Cell 정보 중 {0} 행의 Cell ID와 {0} 행의 Cell ID와 중복 입니다."
                            dgCellList.ItemsSource = null;
                            return;
                        }

                        if (sNewCellID == Util.NVC(dgCellList.GetCell(iRow, iOldCell_Col).Value))
                        {
                            Util.MessageValidation("SFU1461", new object[] { sRow, sCnt }); //"교체 Cell 정보 중 {0} 행의 Cell ID와 {0} 행의 Cell ID와 중복 입니다. "
                            dgCellList.ItemsSource = null;
                            return;
                        }
                    }

                    if (dgCellList.GetRowCount() == 0)
                    {
                        DataTable DT = new DataTable();
                        DT.Columns.Add("SEQ_NO", typeof(decimal));
                        DT.Columns.Add("NG_CELLID", typeof(string));
                        DT.Columns.Add("GOOD_CELLID", typeof(string));
                        DT.Columns.Add("2DBARCODE", typeof(string));

                        DT.Columns.Add("NG_PALLETID", typeof(string));
                        DT.Columns.Add("NG_TRAYID", typeof(string));
                        DT.Columns.Add("NG_PACKCELLSEQ", typeof(string));
                        DT.Columns.Add("NG_LOTID", typeof(string));
                        DT.Columns.Add("NG_RELSID", typeof(string));
                        DT.Columns.Add("G_PALLETID", typeof(string));
                        DT.Columns.Add("G_TRAYID", typeof(string));
                        DT.Columns.Add("G_PACKCELLSEQ", typeof(string));
                        DT.Columns.Add("G_LOTID", typeof(string));
                        DT.Columns.Add("G_RELSID", typeof(string));
                        DT.Columns.Add("INSPECT_SKIP", typeof(string));
                        DT.Columns.Add("CHANGEABLE_YN", typeof(string));
                        DataRow newDr = DT.NewRow();

                        newDr["SEQ_NO"] = i + 1;
                        newDr["NG_CELLID"] = sOldCellID;
                        newDr["GOOD_CELLID"] = sNewCellID;
                        newDr["2DBARCODE"] = s2DBCR;
                        DT.Rows.Add(newDr);

                        ////dgCellList.ItemsSource = DataTableConverter.Convert(DT);
                        Util.GridSetData(dgCellList, DT, FrameOperation);
                    }
                    else
                    {
                        //전송정보 로우 수 체크(테이블 결합 루프용)
                        DataTable DT = DataTableConverter.Convert(dgCellList.ItemsSource);

                        DataRow newDr = DT.NewRow();
                        newDr["SEQ_NO"] = i + 1;
                        newDr["NG_CELLID"] = sOldCellID;
                        newDr["GOOD_CELLID"] = sNewCellID;
                        newDr["2DBARCODE"] = s2DBCR;
                        DT.Rows.Add(newDr);

                        ////dgCellList.ItemsSource = DataTableConverter.Convert(DT);
                        Util.GridSetData(dgCellList, DT, FrameOperation);
                    }


                }

                //lblMSG.Text = "Cell의 정보를 DataBase로 부터 가져 오는 중 입니다.\r\n\r\n잠시만 기다려 주십시오.";


                // cell 정보 조회
                //DataTable dt = Search2Data();

                for (int i = dgCellList.TopRows.Count; i < dgCellList.Rows.Count; i++)
                {
                    //pgbCnt.Value = i + 1;
                    //lblCnt.Text = "[" + (i + 1) + " / " + iData_Cnt + "]";
                    //Application.DoEvents();

                    string sOldCellID = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["NG_CELLID"].Index).Value);
                    string sNewCellID = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["GOOD_CELLID"].Index).Value);

                    DataTable dt = Search2Data(sOldCellID, sNewCellID);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        //if (sOldCellID == Util.NVC(dt.Rows[0]["FROM_SUBLOTID"]))
                        //{
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_PALLETID", dt.Rows[0]["FROM_OUTER_BOXID"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_TRAYID", dt.Rows[0]["FROM_INNER_BOXID"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_PACKCELLSEQ", dt.Rows[0]["FROM_BOX_PSTN_NO"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_LOTID", dt.Rows[0]["FROM_LOTID"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_RELSID", dt.Rows[0]["FROM_RCV_ISS_ID"].ToString());

                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_PALLETID", dt.Rows[0]["TO_OUTER_BOXID"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_TRAYID", dt.Rows[0]["TO_INNER_BOXID"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_PACKCELLSEQ", dt.Rows[0]["TO_BOX_PSTN_NO"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_LOTID", dt.Rows[0]["TO_LOTID"].ToString());
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_RELSID", dt.Rows[0]["TO_RCV_ISS_ID"].ToString());

                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "INSPECT_SKIP", dt.Rows[0]["FROM_INSP_SKIP_FLAG"].ToString());

                        string sRESULT_YN = Util.NVC(dt.Rows[0]["FROM_OUTER_BOXID"]);

                        if (!Search2FormData(sNewCellID, dt.Rows[0]["FROM_INSP_SKIP_FLAG"].ToString(), dt.Rows[0]["TO_AREAID"].ToString()))  //교체할 Cell 활성화 특성측정값 및 투입 가능 여부 판단.
                        {
                            DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "불가");
                            //spdCellList.ActiveSheet.Cells[i, iNewCell_Col].BackColor = Color.Turquoise;
                            //spdCellList.ActiveSheet.Cells[i, GridUtil.GetColumnIndex(spdCellList, "CHANGEABLE_YN")].ForeColor = Color.Red;
                        }
                        else
                        {
                            if (sRESULT_YN == "Y")
                            {
                                DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "가능");
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "불가");
                            }

                        }
                        //}
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "불가");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void Open2ExcelFile()
        {

            ExcelMng exl = new ExcelMng();
            dgCellList.ItemsSource = null;

            try
            {

                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();
                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        string sFileName = fd.FileName.ToString();
                        if (exl != null)
                        {
                            //이전 연결 해제
                            exl.Conn_Close();
                        }
                        //파일명 Set 으로 연결
                        exl.ExcelFileName = sFileName;
                        string[] str = exl.GetExcelSheets();

                        //첫번째 시트 DataTable반환.
                        if (str.Length > 0)
                        {
                            DataTable dtResult = exl.GetSheetData(str[0]);

                            if (dtResult == null)
                            {
                                Util.MessageValidation("SFU1331"); //"Data가 없습니다" >>Data가 존재하지 않습니다.
                                return;
                            }

                            int iData_Cnt = dtResult.Rows.Count;
                            if (iData_Cnt < 1)
                            {
                                Util.MessageValidation("SFU1331"); //"Data가 없습니다" >>Data가 존재하지 않습니다.
                                return;
                            }

                            txtCell_Cnt.Text = iData_Cnt.ToString("#,##0");

                            int iOldCell_Col = dgCellList.Columns["NG_CELLID"].Index;
                            int iNewCell_Col = dgCellList.Columns["GOOD_CELLID"].Index;

                            int FindRow = -1;
                            int FindCol = -1;

                            //lblMSG.Text = "Excel File에서 Cell 정보를 읽는 중 입니다.\r\n\r\n잠시만 기다려 주십시오.";

                            for (int i = 0; i < iData_Cnt; i++)
                            {
                                //Application.DoEvents();
                                //pgbCnt.Value = i + 1;
                                //lblCnt.Text = "[" + (i + 1) + " / " + iData_Cnt + "]";

                                string sOldCellID = dtResult.Columns.Count >= 1 ? Util.NVC(dtResult.Rows[i][0]) : string.Empty;
                                string sNewCellID = dtResult.Columns.Count >= 2 ? Util.NVC(dtResult.Rows[i][1]) : string.Empty;
                                string s2DBCR = dtResult.Columns.Count >= 3 ?  Util.NVC(dtResult.Rows[i][2]) : string.Empty;

                                if (sOldCellID.Equals(""))
                                {
                                    int iOldCnt = i + 1;
                                    string sOldCnt = Convert.ToString(iOldCnt);
                                    Util.MessageValidation("SFU1575", new object[] { sOldCnt }); //"불량 Cell 정보 중 {0} 행의 Cell ID가 공백 입니다. \r\n확인 후 다시 하십시오"
                                    dgCellList.ItemsSource = null;
                                    return;
                                }
                                else if (sNewCellID.Equals(""))
                                {
                                    int iNewCnt = i + 1;
                                    string sNewCnt = Convert.ToString(iNewCnt);
                                    Util.MessageValidation("SFU1460", new object[] { sNewCnt }); //"교체 Cell 정보 중 {0} 행의 Cell ID가 공백 입니다. \r\n확인 후 다시 하십시오"
                                    dgCellList.ItemsSource = null;
                                    dgCellList.ItemsSource = null;
                                    return;
                                }

                                for (int iRow = dgCellList.TopRows.Count; iRow < dgCellList.Rows.Count; iRow++)
                                {
                                    int nRow = iRow + 1 - dgCellList.TopRows.Count;
                                    int nCnt = i + 1;
                                    string sRow = Convert.ToString(nRow);
                                    string sCnt = Convert.ToString(nCnt);

                                    if (sOldCellID == Util.NVC(dgCellList.GetCell(iRow, iOldCell_Col).Value))
                                    {
                                        Util.MessageValidation("SFU1576", new object[] { sRow, sCnt }); //"불량 Cell 정보 중 {0} 행의 Cell ID와 {0} 행의 Cell ID와 중복 입니다."
                                        dgCellList.ItemsSource = null;
                                        return;
                                    }

                                    if (sNewCellID == Util.NVC(dgCellList.GetCell(iRow, iOldCell_Col).Value))
                                    {
                                        Util.MessageValidation("SFU1461", new object[] { sRow, sCnt }); //"교체 Cell 정보 중 {0} 행의 Cell ID와 {0} 행의 Cell ID와 중복 입니다. "
                                        dgCellList.ItemsSource = null;
                                        return;
                                    }
                                }

                                if (dgCellList.GetRowCount() == 0)
                                {
                                    DataTable DT = new DataTable();
                                    DT.Columns.Add("SEQ_NO", typeof(decimal));
                                    DT.Columns.Add("NG_CELLID", typeof(string));
                                    DT.Columns.Add("GOOD_CELLID", typeof(string));
                                    DT.Columns.Add("2DBARCODE", typeof(string));

                                    DT.Columns.Add("NG_PALLETID", typeof(string));
                                    DT.Columns.Add("NG_TRAYID", typeof(string));
                                    DT.Columns.Add("NG_PACKCELLSEQ", typeof(string));
                                    DT.Columns.Add("NG_LOTID", typeof(string));
                                    DT.Columns.Add("NG_RELSID", typeof(string));
                                    DT.Columns.Add("G_PALLETID", typeof(string));
                                    DT.Columns.Add("G_TRAYID", typeof(string));
                                    DT.Columns.Add("G_PACKCELLSEQ", typeof(string));
                                    DT.Columns.Add("G_LOTID", typeof(string));
                                    DT.Columns.Add("G_RELSID", typeof(string));
                                    DT.Columns.Add("INSPECT_SKIP", typeof(string));
                                    DT.Columns.Add("CHANGEABLE_YN", typeof(string));
                                    DataRow newDr = DT.NewRow();

                                    newDr["SEQ_NO"] = i + 1;
                                    newDr["NG_CELLID"] = sOldCellID;
                                    newDr["GOOD_CELLID"] = sNewCellID;
                                    newDr["2DBARCODE"] = s2DBCR;
                                    DT.Rows.Add(newDr);

                                    ////dgCellList.ItemsSource = DataTableConverter.Convert(DT);
                                    Util.GridSetData(dgCellList, DT, FrameOperation);
                                }
                                else
                                {
                                    //전송정보 로우 수 체크(테이블 결합 루프용)
                                    DataTable DT = DataTableConverter.Convert(dgCellList.ItemsSource);

                                    DataRow newDr = DT.NewRow();
                                    newDr["SEQ_NO"] = i + 1;
                                    newDr["NG_CELLID"] = sOldCellID;
                                    newDr["GOOD_CELLID"] = sNewCellID;
                                    newDr["2DBARCODE"] = s2DBCR;
                                    DT.Rows.Add(newDr);

                                    ////dgCellList.ItemsSource = DataTableConverter.Convert(DT);
                                    Util.GridSetData(dgCellList, DT, FrameOperation);
                                }


                            }

                            //lblMSG.Text = "Cell의 정보를 DataBase로 부터 가져 오는 중 입니다.\r\n\r\n잠시만 기다려 주십시오.";

                            
                            // cell 정보 조회
                            //DataTable dt = Search2Data();

                            for (int i = dgCellList.TopRows.Count; i < dgCellList.Rows.Count; i++)
                            {
                                //pgbCnt.Value = i + 1;
                                //lblCnt.Text = "[" + (i + 1) + " / " + iData_Cnt + "]";
                                //Application.DoEvents();

                                string sOldCellID = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["NG_CELLID"].Index).Value);
                                string sNewCellID = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["GOOD_CELLID"].Index).Value);

                                DataTable dt = Search2Data(sOldCellID, sNewCellID);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    //if (sOldCellID == Util.NVC(dt.Rows[0]["FROM_SUBLOTID"]))
                                    //{
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_PALLETID", dt.Rows[0]["FROM_OUTER_BOXID"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_TRAYID", dt.Rows[0]["FROM_INNER_BOXID"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_PACKCELLSEQ", dt.Rows[0]["FROM_BOX_PSTN_NO"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_LOTID", dt.Rows[0]["FROM_LOTID"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "NG_RELSID", dt.Rows[0]["FROM_RCV_ISS_ID"].ToString());

                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_PALLETID", dt.Rows[0]["TO_OUTER_BOXID"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_TRAYID", dt.Rows[0]["TO_INNER_BOXID"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_PACKCELLSEQ", dt.Rows[0]["TO_BOX_PSTN_NO"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_LOTID", dt.Rows[0]["TO_LOTID"].ToString());
                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "G_RELSID", dt.Rows[0]["TO_RCV_ISS_ID"].ToString());

                                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "INSPECT_SKIP", dt.Rows[0]["FROM_INSP_SKIP_FLAG"].ToString());

                                        string sRESULT_YN = Util.NVC(dt.Rows[0]["FROM_OUTER_BOXID"]);

                                        if (!Search2FormData(sNewCellID, dt.Rows[0]["FROM_INSP_SKIP_FLAG"].ToString(), dt.Rows[0]["TO_AREAID"].ToString()))  //교체할 Cell 활성화 특성측정값 및 투입 가능 여부 판단.
                                        {
                                            DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "불가");
                                            //spdCellList.ActiveSheet.Cells[i, iNewCell_Col].BackColor = Color.Turquoise;
                                            //spdCellList.ActiveSheet.Cells[i, GridUtil.GetColumnIndex(spdCellList, "CHANGEABLE_YN")].ForeColor = Color.Red;
                                        }
                                        else
                                        {
                                            if (sRESULT_YN == "Y")
                                            {
                                                DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "가능");
                                            }
                                            else
                                            {
                                                DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "불가");
                                            }
                                            
                                        }
                                    //}
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "불가");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (exl != null)
                {
                    //이전 연결 해제
                    exl.Conn_Close();
                }
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }


           
        }


        //private static string FileOpen()
        //{
        //    OpenFileDialog fDialog = new OpenFileDialog();
        //    string sFileName = String.Empty;
        //    fDialog.Filter = "Excel 97 - 2003 통합문서 (*.xls)|*.xls|Excel 통합 문서(*.xlsx)|*.xlsx";
        //    fDialog.FilterIndex = 2;
        //    fDialog.ShowDialog();
        //    sFileName = fDialog.FileName;

        //    return sFileName;
        //}

        //// GridUtil에 Excel 파일 읽는 방법이 있으나 처리 속도가 좀 느린거 같아서 OLEDB를 사용하는 방법을 써봄.
        //// 이게 좋다면 후에 이 방법으로 변경하는 것도 나쁘지는 않을 듯 하지만 지금은 전체 적용을 하지 않음.
        //private DataSet Get2ExcelData(string FilePaht, string verChk)
        //{

        //    try
        //    {
        //        string oledbConnectionString = string.Empty;

        //        if (verChk.Equals("12.0"))
        //        {
        //            oledbConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FilePaht + ";Extended Properties=Excel 12.0";
        //        }
        //        else
        //        {
        //            oledbConnectionString = "Provider=Microsoft.ACE.OLEDB.4.0;Data Source=" + FilePaht + ";Extended Properties=Excel 8.0";
        //        }
        //        DataSet ds = null;
        //        OleDbDataAdapter adt = null;
        //        using (OleDbConnection con = new OleDbConnection(oledbConnectionString))
        //        {
        //            con.Open();
        //            DataTable dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
        //            string sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
        //            string sQuery = string.Format(" SELECT * FROM [{0}]", sheetName);

        //            ds = new DataSet();
        //            adt = new OleDbDataAdapter(sQuery, con);
        //            adt.Fill(ds);
        //        }
        //        return ds;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}


        /// 불량 Cell 정보와 교체 Cell 정보 조회
        private DataTable Search2Data()
        {
            DataSet ds = new DataSet();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROM_SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("TO_SUBLOTID", typeof(string));

                for (int i = dgCellList.TopRows.Count; i < dgCellList.Rows.Count; i++)
                {
                    string sNgCellID = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["NG_CELLID"].Index).Value);
                    string sGoodCellID = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["GOOD_CELLID"].Index).Value);

                    DataRow dr = RQSTDT.NewRow();
                    dr["FROM_SUBLOTID"] = sNgCellID;
                    dr["TO_SUBLOTID"] = sGoodCellID;
                    RQSTDT.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_INFO_FOR_REPLACE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1905"); //"조회된 Data가 없습니다."
                    return null;
                }
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private DataTable Search2Data(string sNgCellID, string sGoodCellID)
        {
            DataSet ds = new DataSet();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROM_SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("TO_SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_SUBLOTID"] = sNgCellID;
                dr["TO_SUBLOTID"] = sGoodCellID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_INFO_FOR_REPLACE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1905"); //"조회된 Data가 없습니다."
                    return null;
                }
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// 활성화 특성 측정값 조회(교체 Cell만 조회)
        private bool Search2FormData(string sCellID, string sSkip, string sAreaID)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("CELLID", typeof(string));
                //RQSTDT.Columns.Add("COND_SKIP", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["CELLID"] = sCellID;
                //dr["COND_SKIP"] = sSkip;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtRslt = null;
                //if (sAreaID.Equals("A1"))
                //{
                //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_GET", "INDATA", "OUTDATA", RQSTDT);
                //}
                //else if (sAreaID.Equals("A2") || sAreaID.Equals("S2"))
                //{
                //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_GET", "INDATA", "OUTDATA", RQSTDT);
                //}
                //else
                //{
                //    return false;
                //}

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellID;
                dr["SHOPID"] = string.Empty;
                dr["AREAID"] = string.Empty;
                dr["EQSGID"] = string.Empty;
                dr["MDLLOT_ID"] = string.Empty;
                dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                dr["INSP_SKIP_FLAG"] = sSkip;
                dr["2D_BCR_SKIP_FLAG"] = "Y";
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                // ClientProxy2007
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", indataSet);

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;

            }
        }


        private void Save2ChangeData()
        {
            try
            {
                int iErr_Cnt = 0;
                int iOK_Cnt = 0;
                int iData_Cnt = dgCellList.GetRowCount();

                //pnlShow.Visible = true;
                //pgbCnt.Minimum = 0;
                //pgbCnt.Maximum = iData_Cnt;

                //lblMSG.Text = "Cell의 정보를 변경하여 DataBase에 적용 중 입니다.\r\n\r\n잠시만 기다려 주십시오.";

                for (int i = dgCellList.TopRows.Count; i < dgCellList.Rows.Count; i++)
                {
                    string sOldCell = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["NG_CELLID"].Index).Value); 
                    string sNewCell = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["GOOD_CELLID"].Index).Value); 
                    string s2DBCR = Util.NVC(dgCellList.GetCell(i, dgCellList.Columns["2DBARCODE"].Index).Value);

                    if (!Save2Data(sOldCell, sNewCell, s2DBCR))
                    {
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "Error");
                        //spdCellList.ActiveSheet.Cells[i, GridUtil.GetColumnIndex(spdCellList, "GOOD_CELLID")].BackColor = Color.Turquoise;
                        //spdCellList.ActiveSheet.Cells[i, GridUtil.GetColumnIndex(spdCellList, "CHANGEABLE_YN")].ForeColor = Color.Red;
                        iErr_Cnt++;
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgCellList.Rows[i].DataItem, "CHANGEABLE_YN", "변경");
                        iOK_Cnt++;
                    }
                    
                    //pgbCnt.Value = i + 1;
                    //lblCnt.Text = "[" + (i + 1) + " / " + iData_Cnt + "]";
                }

                //pnlShow.Visible = false;                        
                string sData_Cnt = Convert.ToString(iData_Cnt);
                string sOK_Cnt = Convert.ToString(iOK_Cnt);
                string sErr_Cnt = Convert.ToString(iErr_Cnt);
                Util.MessageInfo("SFU1883", new object[] { sData_Cnt, sOK_Cnt, sErr_Cnt }); //"전체 Data {0}건 중 \r\n{0}건 변경되었습니다.\r\n{0}건 Error 발생 되었습니다. "
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtReason.Text = "";
            }
        }

        private bool Save2Data(string sOldCell, string sNewCell, string s2DBCR)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("FROM_SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("TO_SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("BCR2D", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["FROM_SUBLOTID"] = sOldCell;
                dr["TO_SUBLOTID"] = sNewCell;
                dr["BCR2D"] = s2DBCR;
                dr["USERID"] = LoginInfo.USERID;    // Util.NVC(cboProcUser.SelectedValue);
                dr["NOTE"] = txtReason.Text.Trim();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_REPLACE_CELL", "RQSTDT", null, RQSTDT);


                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }


        #endregion


    }
}
