/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using Microsoft.Win32;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_087 : UserControl
    {
        #region Declaration & Constructor 
        DataTable dtResult;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PGM_GUI_087()
        {
            InitializeComponent();

            GridReset(200, 200);

            dataGrid.AutoGenerateColumns = true;

            dataGrid.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.None;

            dataGrid.CanUserAddRows = false;

            dataGrid.GridLinesVisibility = C1.WPF.DataGrid.DataGridGridLinesVisibility.None;

            dataGrid.AlternatingRowBackground = null;

            //Dictionary<Point, Thickness> borderInfo = new Dictionary<Point, Thickness>();
            //Dictionary<Point, SolidColorBrush> colorInfo = new Dictionary<Point, SolidColorBrush>();

            Dictionary<Point, CellStyleInfo> getCellStyle = new Dictionary<Point, CellStyleInfo>();

            //dataGrid.Resources.Add("BorderInfo", borderInfo);
            //dataGrid.Resources.Add("ColorInfo", colorInfo);
            dataGrid.Resources.Add("CellStyleInfo", getCellStyle);

            dataGrid.LoadedCellPresenter -= DataGrid_LoadedCellPresenter;
            dataGrid.LoadedCellPresenter += DataGrid_LoadedCellPresenter;
        }


        #endregion

        #region Initialize
        private void Initialize()
        {
            testData();

            excelData();

            
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
        #endregion

        #region Mehod
        private void excelData()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Excel Files (.xlsx)|*.xlsx";
            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    //LoadExcel(dataGrid, stream, "인원투입계획_실적");

                    //LoadExcelHelper ee = new LoadExcelHelper();
                    LoadExcelHelper.LoadExcel((C1DataGrid)dataGrid, (Stream)stream, (int)0);

                    
                }
            }
        }

        private void LoadExcel(C1DataGrid dataGrid, Stream excelFileStream, string sheetName)
        {
            excelFileStream.Seek(0, SeekOrigin.Begin);
            C1XLBook book = new C1XLBook();
            book.Load(excelFileStream, FileFormat.OpenXml);
            //XLSheet sheet = book.Sheets[sheetNo];
            XLSheet sheet = null;

            for (int i = 0; i < book.Sheets.Count; i++)
            {
                if (sheetName.Equals(book.Sheets[i].Name))
                    sheet = book.Sheets[i];
            }
            ;

            if (sheet == null)
            {
                MessageBox.Show("sheet not exists!");
                return;
            }
            #region extract data
            DataTable dataTable = new DataTable();
            //Dictionary<Point, Thickness> borderInfo = new Dictionary<Point, Thickness>();
            //Dictionary<Point, SolidColorBrush> colorInfo = new Dictionary<Point, SolidColorBrush>();
            Dictionary<Point, CellStyleInfo> getCellStyle = new Dictionary<Point, CellStyleInfo>();
            Dictionary<String, Point> cellValuePoint = new Dictionary<String, Point>();

            int colCnt = 0;
            int rowCnt = 0;

            for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
            {
                //col width setting
                if (sheet.GetCell(0, colInx) != null && !sheet.GetCell(0, colInx).Text.Equals(""))
                {
                    dataTable.Columns.Add("C" + colInx, typeof(string));
                    colCnt++;
                }
            }




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
                            if (cell.Text != null && !cell.Text.Equals("") && cell.Text.Substring(0, 1).Equals("¿"))
                            {

                                if (!cellValuePoint.ContainsKey(cell.Text.Substring(1)))
                                {
                                    cellValuePoint.Add(cell.Text.Substring(1), cellPoint);
                                }
                            }
                            else
                            {
                                dataRow["C" + colInx] = cell.Text;
                            }

                            Thickness cellThickness = new Thickness((cell.Style.BorderLeft != XLLineStyleEnum.None) ? 1 : 0,
                                                                    (cell.Style.BorderTop != XLLineStyleEnum.None) ? 1 : 0,
                                                                    (cell.Style.BorderRight != XLLineStyleEnum.None) ? 1 : 0,
                                                                    (cell.Style.BorderBottom != XLLineStyleEnum.None) ? 1 : 0);

                            CellStyleInfo cellStyleInfo = new CellStyleInfo();
                            cellStyleInfo.cellThickness = cellThickness;
                            cellStyleInfo.cellBackColor = new SolidColorBrush(cell.Style.BackColor);
                            cellStyleInfo.cellFontSize = cell.Style.Font.FontSize;
                            cellStyleInfo.cellFontName = cell.Style.Font.FontName;

                            getCellStyle.Add(cellPoint, cellStyleInfo);

                            rowCnt++;
                        }

                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            //if (dataGrid.Resources.Contains("BorderInfo"))
            //    dataGrid.Resources.Remove("BorderInfo");

            //dataGrid.Resources.Add("BorderInfo", borderInfo);

            //if (dataGrid.Resources.Contains("ColorInfo"))
            //    dataGrid.Resources.Remove("ColorInfo");

            //dataGrid.Resources.Add("ColorInfo", colorInfo);




            DataSet ds = getTestData();


            foreach (DataTable dt in ds.Tables)
            {
                if (dt.Rows.Count == 1)
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (cellValuePoint.ContainsKey(dc.ColumnName))
                        {
                            //String[] sRowCol = hashVal[dc.ColumnName].ToString().Split('_');

                            //db 값 타입에 따라 셀 타입

                            if (dc.DataType.Equals(typeof(decimal)))
                            {
                                //spdWorkDiary.ActiveSheet.Cells[Convert.ToInt16(sRowCol[0]), Convert.ToInt16(sRowCol[1])].CellType = objNumCell;

                            }
                            else
                            {
                                //spdWorkDiary.ActiveSheet.Cells[Convert.ToInt16(sRowCol[0]), Convert.ToInt16(sRowCol[1])].CellType = objTextCell;
                            }

                            //dataGrid.GetCell((int)cellValuePoint[dc.ColumnName].X, (int)cellValuePoint[dc.ColumnName].Y).Value = dt.Rows[0][dc].ToString();
                            dataTable.Rows[(int)cellValuePoint[dc.ColumnName].X][(int)cellValuePoint[dc.ColumnName].Y] = dt.Rows[0][dc].ToString();

                            //spdWorkDiary.ActiveSheet.Cells[Convert.ToInt16(sRowCol[0]), Convert.ToInt16(sRowCol[1])].Text = dt.Rows[0][dc].ToString();
                        }
                    }
                }
                else if (dt.Rows.Count > 1)
                {
                    int iInc = 0;
                    //int nextRow = 0;

                    foreach (DataRow dr in dt.Rows)
                    {
                        foreach (DataColumn dc in dt.Columns)
                        {
                            if (cellValuePoint.ContainsKey(dc.ColumnName))
                            {


                                //db 값 타입에 따라 셀 타입
                                if (dc.DataType.Equals(typeof(decimal)))
                                {
                                    //spdWorkDiary.ActiveSheet.Cells[Convert.ToInt16(sRowCol[0]) + iInc, Convert.ToInt16(sRowCol[1])].CellType = objNumCell;

                                }
                                else
                                {
                                    //spdWorkDiary.ActiveSheet.Cells[Convert.ToInt16(sRowCol[0]) + iInc, Convert.ToInt16(sRowCol[1])].CellType = objTextCell;
                                }

                                //dataGrid.GetCell((int)cellValuePoint[dc.ColumnName].X + iInc, (int)cellValuePoint[dc.ColumnName].Y).Value = dr[dc].ToString();
                                //dataGrid.GetCell((int)cellValuePoint[dc.ColumnName].X + iInc, 0).Value = "#FIX_ROW"; //살리기

                                dataTable.Rows[(int)cellValuePoint[dc.ColumnName].X + iInc][(int)cellValuePoint[dc.ColumnName].Y] = dt.Rows[0][dc].ToString();
                                dataTable.Rows[(int)cellValuePoint[dc.ColumnName].X + iInc][0] = "#FIX_ROW"; //살리기

                                //spdWorkDiary.ActiveSheet.Cells[Convert.ToInt16(sRowCol[0]) + iInc, Convert.ToInt16(sRowCol[1])].Text = dr[dc].ToString(); //값써주기

                                //spdWorkDiary.ActiveSheet.Cells[Convert.ToInt16(sRowCol[0]) + iInc, 0].Text = "#FIX_ROW"; //살리기
                                //nextRow = Convert.ToInt16(sRowCol[0]) + iInc + 1;
                            }
                        }

                        iInc++;

                        //if (iInc < dt.Rows.Count && nextRow > 0)
                        //{ 
                        //    spdWorkDiary.ActiveSheet.Rows.Add(nextRow, 1);
                        //    spdWorkDiary.ActiveSheet.CopyRange(nextRow - 1, 0, nextRow, 0, 1, 10, false);
                        //}

                    }
                }
            }


            GridReset(colCnt, rowCnt);

            //width, height 셋팅
            for (int colInx = 0; colInx < colCnt; colInx++)
            {
                if (sheet.Columns[colInx].Width > 0)
                    dataGrid.Columns[colInx].Width = new C1.WPF.DataGrid.DataGridLength(C1XLBook.TwipsToPixels(sheet.Columns[colInx].Width));
                else
                    dataGrid.Columns[colInx].Width = C1.WPF.DataGrid.DataGridLength.Auto;
            }

            for (int rowInx = 0; rowInx < rowCnt; rowInx++)
            {
                if (sheet.Rows[rowInx].Height > 0)
                    dataGrid.Rows[rowInx].Height = new C1.WPF.DataGrid.DataGridLength(C1XLBook.TwipsToPixels(sheet.Rows[rowInx].Height));
                else
                    dataGrid.Rows[rowInx].Height = C1.WPF.DataGrid.DataGridLength.Auto;
            }



            for (int i = dataTable.Rows.Count - 1; i > 0; i--)
            {
                if (!dataTable.Rows[i][0].Equals("#FIX_ROW")) dataTable.Rows.RemoveAt(i);
            }

            if (dataTable.Columns.Count > 0)
            {
                dataTable.Columns.RemoveAt(0);
            }

            if (dataTable.Rows.Count > 0)
            {
                dataTable.Rows.RemoveAt(0);
            }

            if (dataGrid.Resources.Contains("CellStyleInfo"))
                dataGrid.Resources.Remove("CellStyleInfo");

            dataGrid.Resources.Add("CellStyleInfo", getCellStyle);

            dataTable.AcceptChanges();

            dataGrid.ItemsSource = dataTable.DefaultView;

            #endregion
        }

        private void DataGrid_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

            e.Cell.Presenter.BorderThickness = new Thickness(0);

            Dictionary<Point, Thickness> borderInfo = dataGrid.Resources["BorderInfo"] as Dictionary<Point, Thickness>;
            Dictionary<Point, SolidColorBrush> colorInfo = dataGrid.Resources["ColorInfo"] as Dictionary<Point, SolidColorBrush>;

            Dictionary<Point, CellStyleInfo> getCellStyle = dataGrid.Resources["CellStyleInfo"] as Dictionary<Point, CellStyleInfo>;


            Point currentPoint = new Point(e.Cell.Row.Index + 1, e.Cell.Column.Index + 1);

            //if (borderInfo.ContainsKey(currentPoint))
            //{
            //    Thickness thickness = borderInfo[currentPoint];

            //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BorderThickness = thickness;
            //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Margin = new Thickness(0, 0, -1, -1);
            //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
            //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
            //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
            //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);
            //}

            //if (colorInfo.ContainsKey(currentPoint))
            //{
            //    dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = colorInfo[currentPoint];
            //}

            if (getCellStyle.ContainsKey(currentPoint))
            {
                Thickness thickness = getCellStyle[currentPoint].cellThickness;

                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BorderThickness = thickness;
                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Margin = new Thickness(0, 0, -1, -1);
                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.LeftLineBrush = new SolidColorBrush(Colors.Black);
                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.TopLineBrush = new SolidColorBrush(Colors.Black);
                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.RightLineBrush = new SolidColorBrush(Colors.Black);
                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.BottomLineBrush = new SolidColorBrush(Colors.Black);

                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = getCellStyle[currentPoint].cellBackColor;

                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontSize = getCellStyle[currentPoint].cellFontSize;
                dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.FontFamily = new FontFamily(getCellStyle[currentPoint].cellFontName);
            }


        }

       
        private void GridReset(int colCnt, int rowCnt)
        {

            dataGrid.Columns.Clear();

            DataTable tempdt = new DataTable();


            for (int i = 0; i < colCnt; i++)
            {
                //C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                //column.Name = "A" + i.ToString();
                //dataGrid.Columns.Add(column);

                DataColumn aa = new DataColumn();
                aa.ColumnName = "C" + i.ToString();
                tempdt.Columns.Add(aa);
            }

            for (int i = 0; i < rowCnt; i++)
            {
                DataRow bb = tempdt.NewRow();
                tempdt.Rows.Add(bb);
            }

            dataGrid.ItemsSource = tempdt.DefaultView;
        }




        private DataSet getTestData()
        {
            DataSet ds = new DataSet();

            DataTable RSLTDT = new DataTable("RSLTDT");

            RSLTDT.Columns.Add("LOT", typeof(string));
            RSLTDT.Columns.Add("QTY", typeof(decimal));

            DateTime dJobDate = DateTime.ParseExact("20160704" + " 08:00:00", "yyyyMMdd HH:mm:ss", null);

            for (int i = 0; i < 10; i++)
            {
                RSLTDT.Rows.Add(dJobDate.AddSeconds(i * 20).ToString("yyyyMMddHHmmss"), i);
            }
            ds.Tables.Add(RSLTDT);


            DataTable RSLTDT1 = new DataTable("RSLTDT1");

            RSLTDT1.Columns.Add("DATE", typeof(string));
            RSLTDT1.Columns.Add("QTY_SUM", typeof(decimal));

            RSLTDT1.Rows.Add("2016-07-04", "10000");

            ds.Tables.Add(RSLTDT1);

            DataTable RSLTDT2 = new DataTable("RSLTDT2");

            RSLTDT2.Columns.Add("LOT1", typeof(string));
            RSLTDT2.Columns.Add("QTY1", typeof(decimal));

            for (int i = 0; i < 4; i++)
            {
                RSLTDT2.Rows.Add("INLOT" + i, i);
            }
            ds.Tables.Add(RSLTDT2);

            return ds;
        }

        private void testData()
        {
            testGridData();
        }
        private void testGridData()
        {
            dtResult = new DataTable();
            dtResult.Columns.Add("LOTID", typeof(string));
            dtResult.Columns.Add("CREATEDATE", typeof(string));
            dtResult.Columns.Add("PROCID", typeof(string));

            List<object[]> menulist = new List<object[]>();

            menulist.Add(new object[] { "LGC-KOR18.11.15C0010001", "2016-07-13 19:28:18", "완료" });
            menulist.Add(new object[] { "LGC-KOR18.11.15C0010002", "2016-07-13 19:28:18", "완료" });
            menulist.Add(new object[] { "LGC-KOR23.11.15C0010001", "2016-07-13 19:28:18", "수리/재작업" });
            menulist.Add(new object[] { "LGC-KOR23.11.15C0010002", "2016-07-13 19:28:18", "완료" });

            DataRow newRow;

            foreach (object[] item in menulist)
            {
                newRow = dtResult.NewRow();
                newRow.ItemArray = item;
                dtResult.Rows.Add(newRow);
            }

            SetBinding(dgResult, dtResult);
        }

        private void SetBinding(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            dg.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion

        private void btnSmall_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgResult_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int currentRow = dgResult.CurrentRow.Index;
            string selectLot = DataTableConverter.GetValue(dgResult.Rows[currentRow].DataItem, "LOTID").ToString();

            txtLotId.Text = selectLot;
        }

        private void btnLotSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    

    internal class CellStyleInfo
    {
        internal SolidColorBrush cellBackColor;
        internal string cellFontName;
        internal float cellFontSize;
        internal Thickness cellThickness;

    }
}
