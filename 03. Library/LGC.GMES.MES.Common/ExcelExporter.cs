using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Excel;
using C1.WPF.Excel;

namespace LGC.GMES.MES.Common
{
    public class ExcelExporter
    {
        public void Export(C1DataGrid dataGrid, string defaultFileName = null, bool bOpen = false)
        {
            MemoryStream ms = new MemoryStream();
            dataGrid.Save(ms, new ExcelSaveOptions() { FileFormat = ExcelFileFormat.Xlsx, KeepColumnWidths = true, KeepRowHeights = true });
            ms.Seek(0, SeekOrigin.Begin);
            C1XLBook book = new C1XLBook();
            book.Load(ms);
            List<int> deleteIndex = new List<int>();
            foreach (DataGridRow row in dataGrid.Rows)
            {
                if (row.Visibility == System.Windows.Visibility.Collapsed)
                {
                    deleteIndex.Add(row.Index + (dataGrid.TopRows.Count == 0 ? 1 : 0));
                }
            }

            foreach (int index in (from i in deleteIndex orderby i descending select i))
            {
                if (index < book.Sheets[0].Rows.Count)
                    book.Sheets[0].Rows.RemoveAt(index);
            }
            for (int rowinx = 0; rowinx < book.Sheets[0].Rows.Count; rowinx++)
            {
                for (int colinx = 0; colinx < book.Sheets[0].Columns.Count; colinx++)
                {
                    // 2017.03.30 이슬아
                    // DB에서 Header를 조회하여 다국어처리된 값을 업무테이블 동적으로 가져오나 
                    // 다국어테이블에는 등록이 안되서 [#]이 붙는 경우가 발생하여 replace 처리하고 싶었으나, 일단 주석처리함
                    if (rowinx == 0)
                    {
                        // book.Sheets[0].GetCell(rowinx, colinx).SetValue(book.Sheets[0].GetCell(rowinx, colinx).Text.Replace("[#]", string.Empty), book.Sheets[0].GetCell(rowinx, colinx).Style);
                        book.Sheets[0].GetCell(rowinx, colinx).SetValue(book.Sheets[0].GetCell(rowinx, colinx).Text, book.Sheets[0].GetCell(rowinx, colinx).Style);
                    }

                    if (book.Sheets[0].GetCell(rowinx, colinx).Style != null)
                        book.Sheets[0].GetCell(rowinx, colinx).Style.Font = new XLFont("Arial", 12);
                }
            }

            MemoryStream editedms = new MemoryStream();
            //===================================================================================================================
            if (dataGrid.Resources.Contains("ExportRemove"))
            {
                List<int> removecol = dataGrid.Resources["ExportRemove"] as List<int>;
                for (int idx = removecol.Count; idx > 0; idx--)
                {
                    book.Sheets[0].Columns.RemoveAt(removecol[idx - 1]);
                }
            }
            //===================================================================================================================

            AutoSizeColumns(book.Sheets[0]);

            book.Save(editedms, C1.WPF.Excel.FileFormat.OpenXml);
            editedms.Seek(0, SeekOrigin.Begin);
            string tempFilekey = "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            if (defaultFileName != null)
            {
                tempFilekey = defaultFileName;
            }
            //string tempFilekey = Guid.NewGuid().ToString("N");
            new StreamUploader().uploadTempStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
            {
                ms.Close();
                editedms.Close();
                if (arg.Success)
                {
                    //new FileDownloader().TempFileDownload(tempFilekey, string.IsNullOrEmpty(defaultFileName) ? "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : defaultFileName);
                }
            }, bOpen);
        }

        //public void Export(C1DataGrid dataGrid, string defaultFileName = null)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    dataGrid.Save(ms, new ExcelSaveOptions() { FileFormat = ExcelFileFormat.Xlsx, KeepColumnWidths = true, KeepRowHeights = true });
        //    ms.Seek(0, SeekOrigin.Begin);
        //    C1XLBook book = new C1XLBook();
        //    book.Load(ms);
        //    List<int> deleteIndex = new List<int>();
        //    foreach (DataGridRow row in dataGrid.Rows)
        //    {
        //        if (row.Visibility == System.Windows.Visibility.Collapsed)
        //        {
        //            deleteIndex.Add(row.Index + (dataGrid.TopRows.Count == 0 ? 1 : 0));
        //        }
        //    }
        //    foreach (int index in (from i in deleteIndex orderby i descending select i))
        //    {
        //        if (index < book.Sheets[0].Rows.Count)
        //            book.Sheets[0].Rows.RemoveAt(index);
        //    }
        //    for (int rowinx = 0; rowinx < book.Sheets[0].Rows.Count; rowinx++)
        //    {
        //        for (int colinx = 0; colinx < book.Sheets[0].Columns.Count; colinx++)
        //        {
        //            if (book.Sheets[0].GetCell(rowinx, colinx).Style != null)
        //                book.Sheets[0].GetCell(rowinx, colinx).Style.Font = new XLFont("Arial", 12);
        //        }
        //    }

        //    MemoryStream editedms = new MemoryStream();
        //    book.Save(editedms, C1.WPF.Excel.FileFormat.OpenXml);
        //    editedms.Seek(0, SeekOrigin.Begin);

        //    string tempFilekey = Guid.NewGuid().ToString("N");
        //    new StreamUploader().uploadTempStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
        //        {
        //            ms.Close();
        //            editedms.Close();
        //            if (arg.Success)
        //            {
        //                new FileDownloader().TempFileDownload(tempFilekey, string.IsNullOrEmpty(defaultFileName) ? "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : defaultFileName);
        //            }
        //        }
        //    );
        //}

        public void Export(C1DataGrid[] dataGridArray, string[] excelTabNameArray, string defaultFileName = null)
        {
            C1XLBook book = new C1XLBook();
            book.Sheets.Clear();
            int gridInx = 0;

            foreach (C1DataGrid dataGrid in dataGridArray)
            {
                MemoryStream ms = new MemoryStream();
                dataGrid.Save(ms, new ExcelSaveOptions() { FileFormat = ExcelFileFormat.Xlsx, KeepColumnWidths = true, KeepRowHeights = true });
                ms.Seek(0, SeekOrigin.Begin);

                C1XLBook tempBook = new C1XLBook();
                tempBook.Load(ms);

                List<int> deleteIndex = new List<int>();
                foreach (DataGridRow row in dataGrid.Rows)
                {
                    if (row.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        deleteIndex.Add(row.Index + (dataGrid.TopRows.Count == 0 ? 1 : 0));
                    }
                }
                foreach (int index in (from i in deleteIndex orderby i descending select i))
                {
                    if (index < tempBook.Sheets[0].Rows.Count)
                        tempBook.Sheets[0].Rows.RemoveAt(index);
                }
                for (int rowinx = 0; rowinx < tempBook.Sheets[0].Rows.Count; rowinx++)
                {
                    for (int colinx = 0; colinx < tempBook.Sheets[0].Columns.Count; colinx++)
                    {
                        if (tempBook.Sheets[0].GetCell(rowinx, colinx).Style != null)
                            tempBook.Sheets[0].GetCell(rowinx, colinx).Style.Font = new XLFont("Arial", 12);
                    }
                }

                if (excelTabNameArray != null && excelTabNameArray.Length > gridInx && !string.IsNullOrEmpty(excelTabNameArray[gridInx]))
                {
                    tempBook.Sheets[0].Name = excelTabNameArray[gridInx];
                }
                else
                {
                    tempBook.Sheets[0].Name = "Sheet" + (book.Sheets.Count + 1);
                }

                //===================================================================================================================
                if (dataGrid.Resources.Contains("ExportRemove"))
                {
                    List<int> removecol = dataGrid.Resources["ExportRemove"] as List<int>;
                    for (int idx = removecol.Count; idx > 0; idx--)
                    {
                        tempBook.Sheets[0].Columns.RemoveAt(removecol[idx - 1]);
                    }
                }
                //===================================================================================================================

                book.Sheets.Add(tempBook.Sheets[0].Clone());



                ms.Close();
                gridInx++;
            }
            MemoryStream editedms = new MemoryStream();
            book.Save(editedms, C1.WPF.Excel.FileFormat.OpenXml);
            editedms.Seek(0, SeekOrigin.Begin);

            string tempFilekey = "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            if (defaultFileName != null)
            {
                tempFilekey = defaultFileName;
            }
            //string tempFilekey = Guid.NewGuid().ToString("N");
            new StreamUploader().uploadTempStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
            {
                editedms.Close();
                if (arg.Success)
                {
                    //new FileDownloader().TempFileDownload(tempFilekey, string.IsNullOrEmpty(defaultFileName) ? "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : defaultFileName);
                }
            });


            //string tempFilekey = Guid.NewGuid().ToString("N");
            //new StreamUploader().uploadTempStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
            //    {
            //        editedms.Close();
            //        if (arg.Success)
            //        {
            //            new FileDownloader().TempFileDownload(tempFilekey, string.IsNullOrEmpty(defaultFileName) ? "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : defaultFileName);
            //        }
            //    }
            //);
        }

        public void Export_MergeHeader(C1DataGrid dataGrid, ExcelFileFormat fileFormat = ExcelFileFormat.Xlsx, C1DataGrid datagridCp = null)
        {
            MemoryStream ms = new MemoryStream();
            C1XLBook book = new C1XLBook();

            if (fileFormat == ExcelFileFormat.Xls)
            {
                List<C1DataGrid> gridList = new List<C1DataGrid>();

                int iFixRowCount = 60000;   // 엑셀 2003버전에서는 65536까지만 지원하기 때문에 Row수 고정
                List<DataRow> drList = DataTableConverter.Convert(dataGrid.ItemsSource).AsEnumerable().ToList();
                int iSheetCount = (drList.Count - 1) / iFixRowCount;

                book.Sheets.Clear();
                for (int cnt = 0; cnt <= iSheetCount; cnt++)
                {
                    DataTable dtInfo = drList.GetRange(0, cnt == iSheetCount ? drList.Count : iFixRowCount).CopyToDataTable();
                    datagridCp.ItemsSource = DataTableConverter.Convert(dtInfo);
                    drList.RemoveRange(0, cnt == iSheetCount ? drList.Count : iFixRowCount);
                    ms = new MemoryStream();
                    datagridCp.Save(ms, new ExcelSaveOptions() { FileFormat = fileFormat, KeepColumnWidths = true, KeepRowHeights = true });
                    ms.Seek(0, SeekOrigin.Begin);

                    C1XLBook tmp = new C1XLBook();
                    tmp.Load(ms, C1.WPF.Excel.FileFormat.Biff8);
                    XLSheet sheet = tmp.Sheets[0].Clone();
                    sheet.Name = "Sheet" + (book.Sheets.Count + 1).ToString();
                    book.Sheets.Add(sheet);
                }
            }
            else
            {
                dataGrid.Save(ms, new ExcelSaveOptions() { FileFormat = fileFormat, KeepColumnWidths = true, KeepRowHeights = true });
                ms.Seek(0, SeekOrigin.Begin);
                book.Load(ms, C1.WPF.Excel.FileFormat.OpenXml);
            }

            foreach (XLSheet sheet in book.Sheets)
            {
                XLCellRangeCollection ranges = sheet.MergedCells;

                for (int rowinx = 0; rowinx < sheet.Rows.Count; rowinx++)
                {
                    if (rowinx == 0)
                    {
                        int colFrom = 0;
                        int colTo = 0;
                        for (int colinx = 0; colinx < sheet.Columns.Count; colinx++)
                        {

                            if (colinx < sheet.Columns.Count - 1 && sheet.GetCell(0, colinx).Value.Equals(sheet.GetCell(0, colinx + 1).Value))
                            {
                                colTo = colinx + 1;
                            }
                            else
                            {
                                if (colTo < colFrom)
                                    colTo = colFrom;
                                XLCellRange range = new XLCellRange(0, 0, colFrom, colTo);
                                ranges.Add(range);
                                colFrom = colTo + 1;
                            }
                        }
                        //XLCellRange range = new XLCellRange(0, 0, 0, 3);
                        //XLCellRange range2 = new XLCellRange(0, 0, 4, 6);
                        //ranges.Insert(0, range);
                        //ranges.Insert(1, range2);
                        // }
                    }

                    for (int colinx = 0; colinx < sheet.Columns.Count; colinx++)
                    {
                        if (rowinx == 0)
                        {
                            // book.Sheets[0].GetCell(rowinx, colinx).SetValue(book.Sheets[0].GetCell(rowinx, colinx).Text.Replace("[#]", string.Empty), book.Sheets[0].GetCell(rowinx, colinx).Style);
                            sheet.GetCell(rowinx, colinx).SetValue(sheet.GetCell(rowinx, colinx).Text, sheet.GetCell(rowinx, colinx).Style);
                        }

                        if (sheet.GetCell(rowinx, colinx).Style != null)
                        {
                            sheet.GetCell(rowinx, colinx).Style.Font = new XLFont("Tahoma", 9);
                            sheet.GetCell(rowinx, colinx).Style.AlignHorz = XLAlignHorzEnum.Center;
                            sheet.GetCell(rowinx, colinx).Style.AlignVert = XLAlignVertEnum.Center;

                        }
                    }
                }
                AutoSizeColumns(sheet);
            }
            //  XLCellRangeCollection ranges = book.Sheets[0].MergedCells;/           

            MemoryStream editedms = new MemoryStream();

            book.Save(editedms, fileFormat == ExcelFileFormat.Xls ? C1.WPF.Excel.FileFormat.Biff8 : C1.WPF.Excel.FileFormat.OpenXml);
            editedms.Seek(0, SeekOrigin.Begin);
            string tempFilekey = "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            //if (defaultFileName != null)
            //{
            //     tempFilekey = defaultFileName;
            //}
            //string tempFilekey = Guid.NewGuid().ToString("N");
            new StreamUploader().uploadTempStream(tempFilekey, editedms, tempFilekey, (sender, arg) =>
            {
                ms.Close();
                editedms.Close();
                if (arg.Success)
                {
                        //new FileDownloader().TempFileDownload(tempFilekey, string.IsNullOrEmpty(defaultFileName) ? "ExcelExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx" : defaultFileName);
                    }
            }, fileFormat);
        }


        #region DataTable Excel download
        public void DtToExcel(DataTable dt, string sFileName, Dictionary<string, string> dicHeader)
        {

            C1DataGrid dg = new C1DataGrid();
            dg.AutoGenerateColumns = false;
            foreach (KeyValuePair<string, string> kv in dicHeader)
            {
                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                column.Header = kv.Value;
                column.Binding = new Binding(kv.Key);

                dg.Columns.Add(column);
            }

            dg.ItemsSource = DataTableConverter.Convert(dt);
            Export(dg, sFileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
        }
        #endregion

        private void AutoSizeColumns(XLSheet sheet)
        {
            /*No Graphics instance available because there's no Paint event*/
            /*Create a Graphics object using a handle to current window instead*/
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                /*Traverse rows and columns*/
                for (int c = 0; c < sheet.Columns.Count; c++)
                {
                    int colWidth = -1;
                    for (int r = 0; r < sheet.Rows.Count; r++)
                    {
                        /*Get cell value*/
                        object value = sheet[r, c].Value;
                        if (value != null)
                        {
                            string text = value.ToString();

                            /*Get Style for this cell*/
                            XLStyle s = sheet[r, c].Style;
                            if (s != null && s.Format.Length > 0 && value is IFormattable)
                            {
                                string fmt = XLStyle.FormatXLToDotNet(s.Format);
                                /*get formatted text*/
                                text = ((IFormattable)value).ToString(fmt, CultureInfo.CurrentCulture);
                            }

                            XLFont dFont = sheet.Book.DefaultFont;

                            //if (s != null && s.Font != null)
                            //{
                            //    font = sFont.Font;
                            //}

                            FontFamily fFamily = new FontFamily(dFont.FontName);
                            Font font = new Font(fFamily, dFont.FontSize);

                            /*Get size of drawn string according to its Font*/
                            Size sz = Size.Ceiling(g.MeasureString(text + "XX", font));

                            if (sz.Width > colWidth)
                                colWidth = sz.Width;
                        }
                    }
                    /*Set columns width*/
                    if (colWidth > -1)
                        sheet.Columns[c].Width = C1XLBook.PixelsToTwips(colWidth);
                }
            }
        }

        //CSV 파일 저장
        public void Export_CSV(C1DataGrid dataGrid, string defaultFileName = null, bool bHideHeader = false)
        {
            try
            {
                string tempFilekey = "CSVExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".CSV";
                if (defaultFileName != null)
                {
                    tempFilekey = defaultFileName;
                }

                string delimiter = ",";  // 구분자
                FileStream fs = new FileStream(tempFilekey, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                StreamWriter csvExport = new StreamWriter(fs, System.Text.Encoding.UTF8);

                if (dataGrid.Rows.Count == 0) return;

                // 헤더정보 출력
                if (!bHideHeader)
                {
                    for (int i = 0; i < dataGrid.Columns.Count; i++)
                    {
                        // 숨김 처리 된 컬럼은 제외.
                        if (dataGrid.Columns[i].Visibility != System.Windows.Visibility.Collapsed)
                        {
                            csvExport.Write(dataGrid.Columns[i].Header.ToString());

                            if (i != dataGrid.Columns.Count - 1)
                            {
                                csvExport.Write(delimiter);
                            }
                        }
                    }
                    csvExport.Write(csvExport.NewLine); // add new line
                }

                // 본문 출력
                for (int rowinx = 0; rowinx < dataGrid.Rows.Count - 1; rowinx++)
                {
                    for (int colinx = 0; colinx < dataGrid.Columns.Count; colinx++)
                    {
                        // 숨김 처리 된 컬럼은 제외.
                        if (dataGrid.Columns[colinx].Visibility != System.Windows.Visibility.Collapsed)
                        {
                            object value = dataGrid[rowinx, colinx].Value;
                            if (value != null)
                                csvExport.Write(value.ToString());

                            if (colinx != dataGrid.Columns.Count - 1)
                            {
                                csvExport.Write(delimiter);
                            }
                        }
                    }
                    csvExport.Write(csvExport.NewLine);
                }

                csvExport.Flush(); // flush from the buffers.
                csvExport.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        //CSV 파일 저장 From DataTable
        public void Export_CSV2(DataTable dtRslt, C1DataGrid dataGrid, string defaultFileName = null, bool bHideHeader = false)
        {
            try
            {
                string tempFilekey = "CSVExported_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".CSV";
                if (defaultFileName != null)
                {
                    tempFilekey = defaultFileName;
                }

                string delimiter = ",";  // 구분자
                FileStream fs = new FileStream(tempFilekey, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                StreamWriter csvExport = new StreamWriter(fs, System.Text.Encoding.UTF8);

                if (dtRslt.Rows.Count == 0) return;

                // 헤더정보 출력
                if (!bHideHeader)
                {
                    for (int i = 0; i < dataGrid.Columns.Count; i++)
                    {
                        // 숨김 처리 된 컬럼은 제외.
                        if (dataGrid.Columns[i].Visibility != System.Windows.Visibility.Collapsed)
                        {
                            csvExport.Write(dataGrid.Columns[i].Header.ToString());

                            if (i != dataGrid.Columns.Count - 1)
                            {
                                csvExport.Write(delimiter);
                            }
                        }
                    }
                    csvExport.Write(csvExport.NewLine); // add new line
                }

                DataRow[] rows = dtRslt.Select();

                // 본문 출력
                for (int rowinx = 0; rowinx < dtRslt.Rows.Count; rowinx++)
                {
                    for (int colinx = 0; colinx < dataGrid.Columns.Count; colinx++)
                    {
                        // 숨김 처리 된 컬럼은 제외.
                        if (dataGrid.Columns[colinx].Visibility != System.Windows.Visibility.Collapsed)
                        {
                            object value = rows[rowinx][dataGrid.Columns[colinx].Name.ToString()];
                            if (value != null)
                                csvExport.Write(value.ToString());

                            if (colinx != dataGrid.Columns.Count - 1)
                            {
                                csvExport.Write(delimiter);
                            }
                        }

                    }
                    csvExport.Write(csvExport.NewLine);
                }

                csvExport.Flush(); // flush from the buffers.
                csvExport.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }
    }
}
