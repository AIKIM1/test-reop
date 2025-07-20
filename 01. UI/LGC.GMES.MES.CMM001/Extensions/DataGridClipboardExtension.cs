using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using C1.WPF.DataGrid;
using System.Windows.Input;
using System.Threading;
using System.Data;
namespace LGC.GMES.MES.CMM001.Extensions
{
    public class DataGridClipboardExtension : DependencyObject
    {
        public static DependencyProperty ClipboardActionProperty = DependencyProperty.RegisterAttached("ClipboardAction", typeof(bool), typeof(DataGridClipboardExtension), new PropertyMetadata(false, ClipboardActionPropertyChanged));
        public static DependencyProperty MarkingCellProperty = DependencyProperty.RegisterAttached("MarkingCell", typeof(object), typeof(DataGridClipboardExtension), new PropertyMetadata(null, null));


        public static void SetClipboardAction(DependencyObject c1DataGrid, bool value)
        {
            c1DataGrid.SetValue(ClipboardActionProperty, value);
        }

        public static bool GetClipboardAction(DependencyObject c1DataGrid)
        {
            return (bool)c1DataGrid.GetValue(ClipboardActionProperty);
        }

        public static void ClipboardActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (true.Equals(e.NewValue))
            {
                try
                {
                    C1DataGrid dg = d as C1DataGrid;
                    dg.KeyDown += dg_KeyDown;
                    dg.SelectionChanged += dg_SelectionChanged;
                    dg.SelectionDragStarted += dg_SelectionDragStarted;
                    dg.SelectionDragCompleted += dg_SelectionDragCompleted;

                    dg.LoadedCellPresenter += dg_LoadedCellPresenter;

                    dg.UnloadedCellPresenter += dg_UnloadedCellPresenter;
                }
                catch
                {
                }
            }
        }

        static void dg_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            object obj = dataGrid.GetValue(MarkingCellProperty);
            if (obj != null)
            {
                C1.WPF.DataGrid.DataGridCell cell = obj as C1.WPF.DataGrid.DataGridCell;
                if (cell.Presenter != null && cell == e.Cell)
                {
                    cell.Presenter.SelectedBackground = selectedBackground1;
                }
            }
            
        }

        static void dg_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            object obj = dataGrid.GetValue(MarkingCellProperty);
            if (obj != null)
            {
                C1.WPF.DataGrid.DataGridCell cell = obj as C1.WPF.DataGrid.DataGridCell;
                if (cell.Presenter != null && cell == e.Cell)
                {
                    cell.Presenter.SelectedBackground = selectedBackground2;
                }
            }
            
        }

        static void dg_SelectionDragCompleted(object sender, DataGridSelectionDragEventArgs e)
        {
                  
        }


        static void dg_SelectionDragStarted(object sender, DataGridSelectionDragStartedEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);
            if (cell != null)
            {
                // dataGrid.Tag = cell;
                dataGrid.SetValue(MarkingCellProperty, cell);
            }
        }

        public static System.Windows.Media.SolidColorBrush selectedBackground1 =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 241, 241));

        public static System.Windows.Media.SolidColorBrush selectedBackground2 =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(155, 155, 241, 241));
        static void dg_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
          

            C1DataGrid dataGrid = sender as C1DataGrid;

            foreach (var range in e.AddedRanges)
            {
                int rangeCols = range.Columns.Count();
                int rangeRows = range.Rows.Count();

                if (rangeCols > 1 || rangeRows > 1)
                {

                    foreach (C1.WPF.DataGrid.DataGridCell cell in dataGrid.Selection.SelectedCells)
                    {
                        if (cell.Presenter != null)
                        {
                            cell.Presenter.SelectedBackground = selectedBackground1;
                        }
                    }
                    if (dataGrid.Selection.SelectedCells.Count > 1 && dataGrid.Selection.SelectedRows.Count > 1)
                    {
                        object markingCell = dataGrid.GetValue(MarkingCellProperty);
                        //if (dataGrid.Tag != null && ((C1.WPF.DataGrid.DataGridCell)dataGrid.Tag).Presenter != null)
                        if (markingCell != null && ((C1.WPF.DataGrid.DataGridCell)markingCell).Presenter != null)
                        {
                            ((C1.WPF.DataGrid.DataGridCell)markingCell).Presenter.SelectedBackground = selectedBackground2;
                            
                        }
                    }

                    if (dataGrid.Selection.SelectedCells.Count == 1)
                    {
                        dataGrid.SetValue(MarkingCellProperty, null);
                    }


                }
                else
                {
                    foreach (C1.WPF.DataGrid.DataGridCell cell in dataGrid.Selection.SelectedCells)
                    {
                        cell.Presenter.SelectedBackground = selectedBackground1;
                        dataGrid.SetValue(MarkingCellProperty, cell);
                        //dataGrid.Tag = cell;
                    }
                }
              

                break;
               
            }
            
        }
        static void dg_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            C1DataGrid dg = sender as C1DataGrid;
            try
            {
                if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    DatagridtoClipBoard(dg);
                    if (dg.Selection.SelectedCells.Count > 1)
                    {

                        dg.Selection.Clear();
                        object markingCell = dg.GetValue(MarkingCellProperty);
                        if (markingCell != null)
                        {
                            C1.WPF.DataGrid.DataGridCell cell = markingCell as C1.WPF.DataGrid.DataGridCell;
                            dg.Selection.Add(cell);
                        }                        
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    var _grid = sender as C1.WPF.DataGrid.C1DataGrid;

                    //if (DataRowState.Added.Equals(DataTableConverter.GetValue(_grid.Rows[_grid.CurrentRow.Index].DataItem, "_DataState"))
                    //    || DataRowState.Detached.Equals(DataTableConverter.GetValue(_grid.Rows[_grid.CurrentRow.Index].DataItem, "_DataState"))
                    //       )
                    //{
                    //    ClipBoardtoDatagrid(sender);
                    //    e.Handled = true;
                    //}




                    e.Handled = true;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




        static int GetColumntoDisplayIndex(C1DataGrid datagrid, int columnIndex)
        {
            for (int i = 0; i < datagrid.Columns.Count; i++)
            {
                if (datagrid.Columns[i].DisplayIndex == columnIndex)
                {
                    return i;
                }
            }

            return 0;
        }
        static List<List<string>> GetValuesToList(string clipboard)
        {
            List<List<string>> result = new List<List<string>>();
            List<string> rows = new List<string>();
            List<string> columns = new List<string>();

            rows.AddRange(clipboard.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            //columns.AddRange(rows[0].Split(new string[1] { "\t" }, StringSplitOptions.RemoveEmptyEntries));

            //var rows = clipboard.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            //var columns = rows[0].Split(new string[1] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            //string[,] result = new string[rows.Length, columns.Length];

            for (int i = 0; i < rows.Count; i++)
            {
                columns = new List<string>();
                columns.AddRange(rows[i].Split(new string[1] { "\t" }, StringSplitOptions.None));

                //columns = rows[i].Split(new string[1] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                //for (int j = 0; j < columns.Count; j++)
                //{
                //    //result[i, j] = columns[j];
                //}

                result.Add(columns);
            }
            return result;
        }

        static int GetVisibleColumnCount(C1DataGrid datagrid)
        {
            int columnCount = 0;

            for (int i = 0; i < datagrid.Columns.Count; i++)
            {
                if (datagrid.Columns[i].Visibility == System.Windows.Visibility.Visible)
                {
                    //if (datagrid.Columns[i] is LGIT.GMES.MMD.UI1.PopupFindDataColumn)
                    //{
                    //    columnCount++; // 숨김필드 포함 
                    //}

                    columnCount++;
                }
            }


            return columnCount;
        }
        static bool isMultiLangData(object value)
        {
            if (value == null)
            {
                return false;
            }

            char[] chrArray = new char[] { '\\' };
            char[] chrArray2 = new char[] { '|' };

            string[] strArrays = value.ToString().Split(new char[] { '|' });
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str1 = strArrays[i];

                if (str1.Split(chrArray)[0] == Thread.CurrentThread.CurrentCulture.Name)
                {
                    return true;
                }
            }

            string str2 = value.ToString();
            if (str2.Split(chrArray2).Length == 1)
            {
                if (str2.Split(chrArray).Length == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            return false;
        }

        //static void ClipBoardtoDatagrid(object sender)
        //{
        //    var clipboard = System.Windows.Clipboard.GetText();

        //    if (string.IsNullOrWhiteSpace(clipboard))
        //    {
        //        return;
        //    }

        //    var values = GetValuesToList(clipboard);

        //    int firstColumnIndex = 0;
        //    int firstRowIndex = 0;

        //    if (sender is C1.WPF.DataGrid.C1DataGrid)
        //    {
        //        var datagrid = sender as C1.WPF.DataGrid.C1DataGrid;

        //        DataTable table = null;
        //        if (datagrid.ItemsSource is DataTable)
        //        {
        //            table = (DataTable)datagrid.ItemsSource;
        //        }
        //        else if (datagrid.ItemsSource is DataView)
        //        {
        //            table = ((DataView)datagrid.ItemsSource).Table;
        //        }
        //        //var table = DataTableConverter.Convert(datagrid.ItemsSource);

        //        C1.WPF.DataGrid.DataGridComboBoxColumn comboboxcolumn;
        //        C1.WPF.DataGrid.DataGridDateTimeColumn datetimecolumn;
        //        C1.WPF.DataGrid.DataGridTextColumn textcolumn;
        //        //DataGridMultiLangColumn multilangcolumn;
        //        //LGIT.GMES.MMD.UI1.PopupFindDataColumn popupfinddatacolumn;
        //        C1.WPF.DataGrid.DataGridNumericColumn numericcolumn;


        //        var firstSelectedCell = datagrid.Selection.SelectedRanges[datagrid.Selection.SelectedRanges.Count - 1].TopLeftCell;
        //        //firstColumnIndex = firstSelectedCell.Column.DisplayIndex;
        //        firstRowIndex = firstSelectedCell.Row.Index;

        //        int pasteRowCount = table.Rows.Count - firstRowIndex > values.Count ? values.Count : table.Rows.Count - firstRowIndex;
        //        int pastedRowCount = 0;

        //        int pasteColumnCount = 0;
        //        int pastedColumnCount = 0;
        //        int lastColumnIndex = 0;
        //        int lastRowIndex = 0;


        //        for (int i = firstRowIndex; i < datagrid.Rows.Count; i++)
        //        {
        //            firstColumnIndex = firstSelectedCell.Column.DisplayIndex;

        //            if (pastedRowCount >= pasteRowCount)
        //            {
        //                break;
        //            }

        //            if (datagrid.Rows[i].Visibility == System.Windows.Visibility.Collapsed)
        //            {
        //                continue;
        //            }

        //            pastedColumnCount = 0;
        //            pasteColumnCount = GetVisibleColumnCount(datagrid) - firstColumnIndex > values[pastedRowCount].Count ? values[pastedRowCount].Count : GetVisibleColumnCount(datagrid) - firstColumnIndex;

        //            int j = 0;
        //            for (; j < values[pastedRowCount].Count; j++)
        //            {
        //                if (pastedColumnCount >= pasteColumnCount)
        //                {
        //                    break;
        //                }

        //                if (pasteColumnCount > 1)
        //                {
        //                    if (datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Visibility == System.Windows.Visibility.Collapsed)
        //                    {
        //                        firstColumnIndex++;
        //                        j--;
        //                        continue;
        //                    }
        //                }

        //                if (datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] is C1.WPF.DataGrid.DataGridTextColumn)
        //                {
        //                    textcolumn = datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] as C1.WPF.DataGrid.DataGridTextColumn;

        //                    if (textcolumn.IsReadOnly)
        //                    {
        //                        continue;
        //                    }

        //                    table.Rows[i][datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Name] = values[pastedRowCount][j];
        //                }
        //                else if (datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] is C1.WPF.DataGrid.DataGridNumericColumn)
        //                {
        //                    numericcolumn = datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] as C1.WPF.DataGrid.DataGridNumericColumn;

        //                    if (numericcolumn.IsReadOnly)
        //                    {
        //                        continue;
        //                    }

        //                    //string numberText = values[pastedRowCount][j].ToString();

        //                    foreach (Char c in values[pastedRowCount][j].ToCharArray())
        //                    {
        //                        if (!Char.IsDigit(c))
        //                        {
        //                            values[pastedRowCount][j] = string.Empty;
        //                            break;
        //                        }
        //                    }

        //                    table.Rows[i][datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Name] = values[pastedRowCount][j];
        //                }

        //                else if (datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] is C1.WPF.DataGrid.DataGridDateTimeColumn)
        //                {
        //                    datetimecolumn = datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] as C1.WPF.DataGrid.DataGridDateTimeColumn;

        //                    if (datetimecolumn.IsReadOnly)
        //                    {
        //                        continue;
        //                    }

        //                    table.Rows[i][datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Name] = values[pastedRowCount][j];
        //                }
        //                //else if (datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] is DataGridMultiLangColumn)
        //                //{
        //                //    multilangcolumn = datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] as DataGridMultiLangColumn;

        //                //    if (isMultiLangData(values[pastedRowCount][j]))
        //                //    {
        //                //        table.Rows[i][datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Name] = values[pastedRowCount][j];
        //                //    }
        //                //    else
        //                //    {
        //                //        table.Rows[i][datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Name] = @"ko-KR\|en-US\|zh-CN\";
        //                //    }

        //                //}
        //                //else if (datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] is LGIT.GMES.MMD.UI1.PopupFindDataColumn)
        //                //{
        //                //    popupfinddatacolumn = datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)] as LGIT.GMES.MMD.UI1.PopupFindDataColumn;


        //                //    int columnIndex = datagrid.Columns[datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Name.Replace(Info._POPUP, string.Empty)].Index;
        //                //    table.Rows[i][datagrid.Columns[GetColumntoDisplayIndex(datagrid, columnIndex)].Name] = values[pastedRowCount][j];

        //                //    //firstColumnIndex++;
        //                //}
        //                else
        //                {
        //                    table.Rows[i][datagrid.Columns[GetColumntoDisplayIndex(datagrid, firstColumnIndex + j)].Name] = values[pastedRowCount][j];
        //                }

        //                pastedColumnCount++;

        //            }

        //            lastColumnIndex = firstColumnIndex + j;
        //            lastRowIndex = i;

        //            pastedRowCount++;
        //        }

        //        int indexT = 0;
        //        //datagrid.BottomRows[0].Index
        //        for (int i = 0; i < datagrid.Rows.Count - 1; i++)
        //        {
                    
        //            if (DataRowState.Deleted.Equals(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "_DataState")))
        //            {
        //                //table.Rows.Remove(table.Rows[i]);
        //                table.Rows.Remove(table.Rows[indexT]);
        //                //i--;
        //            }
        //            else
        //            {
        //                indexT++;
        //            }


        //        }

        //        double hscroll = datagrid.Viewport.HorizontalOffset;
        //        double vscroll = datagrid.Viewport.VerticalOffset;

        //        datagrid.ItemsSource = DataTableConverter.Convert(table);
        //        //datagrid.ScrollIntoView(lastRowIndex, lastColumnIndex);

        //        datagrid.Viewport.ScrollToHorizontalOffset(hscroll);
        //        datagrid.Viewport.ScrollToVerticalOffset(vscroll);
        //    }
        //}

        static void DatagridtoClipBoard(object sender)
        {
            if (sender is C1.WPF.DataGrid.C1DataGrid)
            {
                var datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
                DataTable table = null;

                if (datagrid.ItemsSource is DataTable)
                {
                    table = (DataTable)datagrid.ItemsSource;
                }
                else if (datagrid.ItemsSource is DataView)
                {
                    table = ((DataView)datagrid.ItemsSource).Table;
                }

                

                if (datagrid.Rows.Count < 1)
                    return;

                datagrid.ClipboardCopyMode = C1.WPF.DataGrid.DataGridClipboardMode.ExcludeHeader;
                int firstColumnIndex = 0, firstRowIndex = 0;
                int lastColumnIndex = 0, lastRowIndex = 0;
                StringBuilder sb = new StringBuilder();

                var firstSelectedCell = datagrid.Selection.SelectedRanges[0].TopLeftCell;
                firstColumnIndex = firstSelectedCell.Column.DisplayIndex;
                firstRowIndex = firstSelectedCell.Row.Index;


                var lastSelectedCell = datagrid.Selection.SelectedRanges[datagrid.Selection.SelectedRanges.Count - 1].BottomRightCell;
                lastColumnIndex = lastSelectedCell.Column.DisplayIndex;
                lastRowIndex = lastSelectedCell.Row.Index;


                for (int i = firstRowIndex; i <= lastRowIndex; i++)
                {
                    if (datagrid.Rows[i].Visibility == System.Windows.Visibility.Collapsed)
                    {
                        continue;
                    }

                    if (datagrid.BottomRows.Count > 0 && i == datagrid.BottomRows[0].Index)
                    {
                        continue;
                    }

                    for (int j = firstColumnIndex; j <= lastColumnIndex; j++)
                    {
                        if (datagrid.Columns[j].Visibility == System.Windows.Visibility.Collapsed)
                        {
                            continue;
                        }

                        if (datagrid[i, GetColumntoDisplayIndex(datagrid, j)].Value == null)
                        {
                            sb.Append(string.Empty + "\t");
                            continue;
                        }

                        //if (this.chkTextOnly.IsChecked == false)
                        //{

                        //if (datagrid.Columns[j] is LGC.GMES.MES.ControlsLibrary.DataGridMultiLangColumn)
                        //{
                        //    sb.Append(datagrid[i, GetColumntoDisplayIndex(datagrid, j)].Value.ToString() + "\t");
                        //}
                        
                        if (datagrid.Columns[j] is C1.WPF.DataGrid.DataGridComboBoxColumn)
                        {
                            sb.Append(datagrid[i, GetColumntoDisplayIndex(datagrid, j)].Value.ToString() + "\t");
                        }
                        //else if (datagrid.Columns[j] is LGIT.GMES.MMD.UI1.PopupFindDataColumn)
                        //{
                        //    int columnIndex = datagrid.Columns[datagrid.Columns[GetColumntoDisplayIndex(datagrid, j)].Name.Replace(Info._POPUP, string.Empty)].Index;


                        //    sb.Append(datagrid[i, GetColumntoDisplayIndex(datagrid, columnIndex)].Value.ToString() + "\t");
                        //}
                        else
                        {
                            sb.Append(datagrid[i, GetColumntoDisplayIndex(datagrid, j)].Value.ToString() + "\t");

                        }
                        //}
                        //else
                        //{
                        //    if (datagrid.Columns[j] is LGIT.GMES.SL.ControlsLibrary.DataGridMultiLangColumn)
                        //    {
                        //        sb.Append(
                        //            GetTextFromMultiLangText(datagrid[i, GetColumntoDisplayIndex(datagrid, j)].Value.ToString(), LGIT.GMES.SL.Common.LoginInfo.LANGID) + "\t"

                        //        );
                        //    }
                        //    else if (datagrid.Columns[j] is C1.WPF.DataGrid.DataGridComboBoxColumn)
                        //    {
                        //        sb.Append(datagrid[i, GetColumntoDisplayIndex(datagrid, j)].Text + "\t");
                        //    }
                        //    else if (datagrid.Columns[j] is LGIT.GMES.MMD.UI1.PopupFindDataColumn)
                        //    {
                        //        int columnIndex = datagrid.Columns[datagrid.Columns[GetColumntoDisplayIndex(datagrid, j)].Name.Replace(Info._POPUP, string.Empty)].Index;


                        //        sb.Append(datagrid[i, GetColumntoDisplayIndex(datagrid, columnIndex)].Text + "\t");
                        //    }
                        //    else
                        //    {
                        //        sb.Append(datagrid[i, GetColumntoDisplayIndex(datagrid, j)].Text + "\t");

                        //    }
                        //}

                    }

                    sb.Replace("\t", "", sb.Length - 1, 1);
                    sb.Append("\r\n");
                }

                sb.Replace("\r\n", "", sb.Length - 2, 1);

                System.Windows.Clipboard.SetText(sb.ToString());
            }
        }
    }
}
