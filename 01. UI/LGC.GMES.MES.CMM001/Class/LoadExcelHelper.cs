using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;

namespace LGC.GMES.MES.CMM001.Class
{
    public class LoadExcelHelper : DependencyObject
    {
        private static readonly string[] ExcelColumnNames = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public static void LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid, Stream excelFileStream, int sheetNo)
        {
            excelFileStream.Seek(0, SeekOrigin.Begin);
            C1XLBook book = new C1XLBook();
            book.Load(excelFileStream, C1.WPF.Excel.FileFormat.OpenXml);
            XLSheet sheet = book.Sheets[sheetNo];

            #region extract Merge Information
            List<Rect> mergedRange = new List<Rect>();
            foreach (XLCellRange range in sheet.MergedCells)
            {
                mergedRange.Add(new Rect(new Point(range.ColumnFrom, range.RowFrom), new Size(range.ColumnCount, range.RowCount)));
            }
            if (dataGrid.Resources.Contains("MergedRange"))
            {
                dataGrid.Resources.Remove("MergedRange");
            }
            dataGrid.Resources.Add("MergedRange", mergedRange);

            dataGrid.MergingCells -= dataGrid_MergingCells;
            dataGrid.MergingCells += dataGrid_MergingCells;

            Dictionary<Point, Point> mergedParentInfo = new Dictionary<Point, Point>();
            foreach (XLCellRange range in sheet.MergedCells)
            {
                for (int colInx = range.ColumnFrom; colInx <= range.ColumnTo; colInx++)
                {
                    for (int rowInx = range.RowFrom; rowInx <= range.RowTo; rowInx++)
                    {
                        mergedParentInfo.Add(new Point(colInx, rowInx), new Point(range.ColumnFrom, range.RowFrom));
                    }
                }
            }
            #endregion

            #region extract Border Information
            Point currentPoint = new Point(0, 0);
            Thickness thickness = new Thickness(0, 0, 0, 0);
            Dictionary<Point, Thickness> borderInfo = new Dictionary<Point, Thickness>();
            for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
            {
                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    currentPoint = new Point(colInx, rowInx);
                    if (sheet.GetCell(rowInx, colInx) != null)
                    {
                        double leftThickness = 0;
                        double topThickness = 0;
                        double rightThickness = 0;
                        double bottomThickness = 0;

                        leftThickness = (int)sheet.GetCell(rowInx, colInx).Style.BorderLeft;
                        topThickness = (int)sheet.GetCell(rowInx, colInx).Style.BorderTop;
                        rightThickness = 0;
                        bottomThickness = 0;

                        if (colInx == sheet.Columns.Count - 1)
                        {
                            rightThickness = (int)sheet.GetCell(rowInx, colInx).Style.BorderRight;
                        }

                        if (rowInx == sheet.Rows.Count - 1)
                        {
                            bottomThickness= (int)sheet.GetCell(rowInx, colInx).Style.BorderBottom;
                        }

                        thickness = new Thickness(leftThickness, topThickness, rightThickness,bottomThickness);

                        //thickness = new Thickness((int)sheet.GetCell(rowInx, colInx).Style.BorderLeft,
                        //                          (int)sheet.GetCell(rowInx, colInx).Style.BorderTop,
                        //                          (int)sheet.GetCell(rowInx, colInx).Style.BorderRight,
                        //                          (int)sheet.GetCell(rowInx, colInx).Style.BorderBottom);
                        borderInfo.Add(currentPoint, thickness);
                    }
                }
            }
            if (dataGrid.Resources.Contains("BorderInfo"))
            {
                dataGrid.Resources.Remove("BorderInfo");
            }

            dataGrid.Resources.Add("BorderInfo", borderInfo);

            //Dictionary<Point, Thickness> borderInfo = new Dictionary<Point, Thickness>();
            //for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
            //{
            //    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
            //    {
            //        if (sheet.GetCell(rowInx, colInx) != null)
            //        {
            //            double rightThickness = 0;
            //            double bottomThickness = 0;
            //            if (sheet.GetCell(rowInx, colInx).Style.BorderLeft != XLLineStyleEnum.None)
            //            {
            //                int prevColInx = colInx - 1;
            //                int prevRowInx = rowInx;
            //                if (prevColInx >= 0 && prevRowInx >= 0)
            //                {
            //                    Point prevPoint = new Point(prevColInx, prevRowInx);
            //                    if (mergedParentInfo.ContainsKey(prevPoint))
            //                    {
            //                        prevPoint = mergedParentInfo[prevPoint];
            //                    }

            //                    if (!borderInfo.ContainsKey(prevPoint))
            //                    {
            //                        borderInfo.Add(prevPoint, new Thickness(0, 0, 1, 0));
            //                    }
            //                    else
            //                    {
            //                        borderInfo[prevPoint] = new Thickness(0, 0, 1, borderInfo[prevPoint].Bottom);
            //                    }
            //                }
            //            }
            //            if (sheet.GetCell(rowInx, colInx).Style.BorderTop != XLLineStyleEnum.None)
            //            {
            //                int prevColInx = colInx;
            //                int prevRowInx = rowInx - 1;
            //                if (prevColInx >= 0 && prevRowInx >= 0)
            //                {
            //                    Point prevPoint = new Point(prevColInx, prevRowInx);
            //                    if (mergedParentInfo.ContainsKey(prevPoint))
            //                    {
            //                        prevPoint = mergedParentInfo[prevPoint];
            //                    }

            //                    if (!borderInfo.ContainsKey(prevPoint))
            //                    {
            //                        borderInfo.Add(prevPoint, new Thickness(0, 0, 0, 1));
            //                    }
            //                    else
            //                    {
            //                        borderInfo[prevPoint] = new Thickness(0, 0, borderInfo[prevPoint].Right, 1);
            //                    }
            //                }
            //            }
            //            if (sheet.GetCell(rowInx, colInx).Style.BorderRight != XLLineStyleEnum.None)
            //            {
            //                rightThickness = 1;
            //            }
            //            if (sheet.GetCell(rowInx, colInx).Style.BorderBottom != XLLineStyleEnum.None)
            //            {
            //                bottomThickness = 1;
            //            }
            //            if (!(rightThickness == 0 && bottomThickness == 0))
            //            {
            //                Point currentPoint = new Point(colInx, rowInx);
            //                if (mergedParentInfo.ContainsKey(currentPoint))
            //                {
            //                    currentPoint = mergedParentInfo[currentPoint];
            //                }

            //                if (borderInfo.ContainsKey(currentPoint))
            //                {
            //                    rightThickness = borderInfo[currentPoint].Right == 1 ? 1 : rightThickness;
            //                    bottomThickness = borderInfo[currentPoint].Bottom == 1 ? 1 : bottomThickness;
            //                    borderInfo[currentPoint] = new Thickness(0, 0, rightThickness, bottomThickness);
            //                }
            //                else
            //                {
            //                    borderInfo.Add(currentPoint, new Thickness(0, 0, rightThickness, bottomThickness));
            //                }
            //            }
            //        }
            //    }
            //}
            //if (dataGrid.Resources.Contains("BorderInfo"))
            //{
            //    dataGrid.Resources.Remove("BorderInfo");
            //}
            //dataGrid.Resources.Add("BorderInfo", borderInfo);

            #endregion

            #region extract data
            DataTable dataTable = new DataTable();
            for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
            {
                dataTable.Columns.Add(getExcelColumnName(colInx), typeof(string));
            }
            for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                {
                    XLCell cell = sheet.GetCell(rowInx, colInx);
                    if (cell != null)
                    {
                        dataRow[getExcelColumnName(colInx)] = cell.Text;
                    }
                }
                dataTable.Rows.Add(dataRow);

                if (rowInx == sheet.Rows.Count - 1)
                {
                    dataRow = dataTable.NewRow();
                    for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                    {
                        XLCell cell = sheet.GetCell(rowInx, colInx);
                        if (cell != null)
                        {
                            dataRow[getExcelColumnName(colInx)] = " ";
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }
            dataTable.AcceptChanges();
            #endregion

            #region redefine datagrid column
            dataGrid.Columns.Clear();
            dataGrid.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.None;
            dataGrid.FrozenTopRowsCount = 0;
            dataGrid.TopRows.Clear();

            for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
            {
                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                string columnName = getExcelColumnName(colInx);
                column.Header = columnName;
                column.Binding = new Binding(columnName);
                if (sheet.Columns[colInx].Width > 0)
                {
                    column.Width = new C1.WPF.DataGrid.DataGridLength(C1XLBook.TwipsToPixels(sheet.Columns[colInx].Width));
                }
                else
                {
                    column.Width = C1.WPF.DataGrid.DataGridLength.Auto;
                }
                dataGrid.Columns.Add(column);
            }

            #endregion

            dataGrid.ItemsSource = DataTableConverter.Convert(dataTable);

            #region extract row height
            for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
            {
                if (sheet.Rows[rowInx].Height > 0)
                    dataGrid.Rows[rowInx].Height = new C1.WPF.DataGrid.DataGridLength(C1XLBook.TwipsToPixels(sheet.Rows[rowInx].Height));
                else
                    dataGrid.Rows[rowInx].Height = C1.WPF.DataGrid.DataGridLength.Auto;
            }
            #endregion

            #region extract column width
            for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
            {
                if (sheet.Columns[colInx].Width > 0)
                    dataGrid.Columns[colInx].Width = new C1.WPF.DataGrid.DataGridLength(C1XLBook.TwipsToPixels(sheet.Columns[colInx].Width));
                else
                    dataGrid.Columns[colInx].Width = C1.WPF.DataGrid.DataGridLength.Auto;
            }
            #endregion

            LGC.GMES.MES.CMM001.Class.Util.SetGridColumnText(dataGrid, null, null, "DUMY", false, false, false, true, 10, HorizontalAlignment.Center, Visibility.Visible);

            dataGrid.LoadedCellPresenter -= dataGrid_LoadedCellPresenter;
            dataGrid.LoadedCellPresenter += dataGrid_LoadedCellPresenter;
        }

        static void dataGrid_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                if (dataGrid.Resources.Contains("BorderInfo"))
                {
                    e.Cell.Presenter.BorderThickness = new Thickness(0);

                    List<Rect> mergedRange = dataGrid.Resources["MergedRange"] as List<Rect>;
                    Dictionary<Point, Thickness> borderInfo = dataGrid.Resources["BorderInfo"] as Dictionary<Point, Thickness>;
                    Point currentPoint = new Point(e.Cell.Column.Index, e.Cell.Row.Index);

                    bool bmerged = false;
                    Rect mergedrect = new Rect();
                    foreach (Rect rect in mergedRange)
                    {
                        if (rect.TopLeft == currentPoint)
                        {
                            mergedrect = rect;
                            bmerged = true;
                        }
                    }

                    //if (bmerged == true)
                    //{
                    //    if (borderInfo.ContainsKey(currentPoint))
                    //    {
                    //        //Thickness thickness = new Thickness(borderInfo[currentPoint].Left, borderInfo[currentPoint].Top, borderInfo[currentPoint].Left, borderInfo[currentPoint].Top);
                    //        ////Thickness thickness = borderInfo[currentPoint];

                    //        //e.Cell.Presenter.BorderThickness = thickness;

                    //        //e.Cell.Presenter.LeftLineBrush = null;
                    //        //e.Cell.Presenter.TopLineBrush = null;
                    //        //e.Cell.Presenter.RightLineBrush = null;
                    //        //e.Cell.Presenter.BottomLineBrush = null;

                    //        //if (thickness.Left != 0)
                    //        //{
                    //        //    e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                    //        //}
                    //        //if (thickness.Top != 0)
                    //        //{
                    //        //    e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                    //        //}
                    //        //if (thickness.Right != 0)
                    //        //{
                    //        //    e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                    //        //}
                    //        //if (thickness.Bottom != 0)
                    //        //{
                    //        //    e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                    //        //}
                    //    }
                    //}
                    //else
                    //{
                        if (borderInfo.ContainsKey(currentPoint))
                        {
                            Thickness thickness = borderInfo[currentPoint];

                            e.Cell.Presenter.BorderThickness = thickness;

                            e.Cell.Presenter.LeftLineBrush = null;
                            e.Cell.Presenter.TopLineBrush = null;
                            e.Cell.Presenter.RightLineBrush = null;
                            e.Cell.Presenter.BottomLineBrush = null;

                            if (thickness.Left != 0)
                            {
                                e.Cell.Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                            }
                            if (thickness.Top != 0)
                            {
                                e.Cell.Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                            }
                            if (thickness.Right != 0)
                            {
                                e.Cell.Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                            }
                            if (thickness.Bottom != 0)
                            {
                                e.Cell.Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
                            }
                        }
                //    }
                    
                }
            }));
        }

        static void dataGrid_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            if (dataGrid.Resources.Contains("MergedRange"))
            {
                List<Rect> mergedRange = dataGrid.Resources["MergedRange"] as List<Rect>;
                Dictionary<Point, Thickness> borderInfo = dataGrid.Resources["BorderInfo"] as Dictionary<Point, Thickness>;

                foreach (Rect rect in mergedRange)
                {
                    e.Merge(new DataGridCellsRange(dataGrid.GetCell((int)rect.Top, (int)rect.Left), dataGrid.GetCell((int)rect.Bottom - 1, (int)rect.Right - 1)));
                }
            }
        }

        private static string getExcelColumnName(int inx)
        {
            Stack<string> nameStack = new Stack<string>();
            while (inx >= 0)
            {
                int mod = inx % ExcelColumnNames.Length;
                nameStack.Push(ExcelColumnNames[mod]);
                inx = (inx - mod) / ExcelColumnNames.Length;

                if (inx == 0)
                    break;
            }

            StringBuilder sb = new StringBuilder();
            while (nameStack.Count > 0)
            {
                sb.Append(nameStack.Pop());
            }
            return sb.ToString();
        }

        public static void LoadExcelData(C1.WPF.DataGrid.C1DataGrid dataGrid, Stream excelFileStream, int sheetNo, int headercnt )
        {
            excelFileStream.Seek(0, SeekOrigin.Begin);
            C1XLBook book = new C1XLBook();
            book.Load(excelFileStream, C1.WPF.Excel.FileFormat.OpenXml);
            XLSheet sheet = book.Sheets[sheetNo];

            DataTable dataTable = new DataTable();
            for (int colInx = 0; colInx < dataGrid.Columns.Count; colInx++)
            {
                dataTable.Columns.Add(Convert.ToString(dataGrid.Columns[colInx].Name), typeof(string));
            }
            for (int rowInx = headercnt; rowInx < sheet.Rows.Count; rowInx++)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int colInx = 0; colInx < dataGrid.Columns.Count; colInx++)
                {
                    XLCell cell = sheet.GetCell(rowInx, colInx);
                    if (cell != null)
                    {
                        dataRow[Convert.ToString(dataGrid.Columns[colInx].Name)] = cell.Text;
                    }
                }
                dataTable.Rows.Add(dataRow);
            }
            dataTable.AcceptChanges();

            dataGrid.ItemsSource = DataTableConverter.Convert(dataTable);
        }

        public static DataTable LoadExcelData(Stream excelFileStream, int sheetNo, int headercnt, bool? IsTrim = false)
        {
            DataTable dtReturn = null;
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                try
                {
                    book.Load(excelFileStream, C1.WPF.Excel.FileFormat.OpenXml);
                }
                catch
                {
                    book.Load(excelFileStream, C1.WPF.Excel.FileFormat.Biff8);
                }

                XLSheet sheet = book.Sheets[sheetNo];
                DataTable dataTable = new DataTable();

                for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                {
                    if (IsTrim == true && sheet.GetCell(0, colInx) == null)
                        break;
                        
                    dataTable.Columns.Add();
                }
                for (int rowInx = headercnt; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (IsTrim == true && (sheet.GetCell(rowInx, 0) == null || string.IsNullOrWhiteSpace(sheet.GetCell(rowInx, 0).Text)))
                        break;

                    DataRow dataRow = dataTable.NewRow();
                    for (int colInx = 0; colInx < dataTable.Columns.Count; colInx++)
                    {
                        XLCell cell = sheet.GetCell(rowInx, colInx);
                        if (cell != null)
                        {
                            dataRow[colInx] = cell.Text;
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }
                dataTable.AcceptChanges();
                dtReturn = dataTable.Copy();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return dtReturn;
        }

        public static DataTable LoadExcelData(Stream excelFileStream, int headercnt, bool? IsTrim = false)
        {
            DataTable dtReturn = null;
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                try
                {
                    book.Load(excelFileStream, C1.WPF.Excel.FileFormat.OpenXml);
                }
                catch
                {
                    book.Load(excelFileStream, C1.WPF.Excel.FileFormat.Biff8);
                }

                XLSheet sheet = book.Sheets[book.Sheets.SelectedIndex];
                DataTable dataTable = new DataTable();

                for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                {
                    if (IsTrim == true && sheet.GetCell(0, colInx) == null)
                        break;

                    dataTable.Columns.Add();
                }
                for (int rowInx = headercnt; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (IsTrim == true && (sheet.GetCell(rowInx, 0) == null || string.IsNullOrWhiteSpace(sheet.GetCell(rowInx, 0).Text)))
                        break;

                    DataRow dataRow = dataTable.NewRow();
                    for (int colInx = 0; colInx < dataTable.Columns.Count; colInx++)
                    {
                        XLCell cell = sheet.GetCell(rowInx, colInx);
                        if (cell != null)
                        {
                            dataRow[colInx] = cell.Text;
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }
                dataTable.AcceptChanges();
                dtReturn = dataTable.Copy();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtReturn;
        }
    }
}
