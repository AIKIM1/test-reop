using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using DataGridCell = C1.WPF.DataGrid.DataGridCell;
using DataGridColumn = C1.WPF.DataGrid.DataGridColumn;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using DataGridRowEventArgs = C1.WPF.DataGrid.DataGridRowEventArgs;

namespace LGC.GMES.MES.ControlsLibrary
{    
    partial class C1DataGridEventHandlers : ResourceDictionary
    {
        private void dg_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            if (e.NewRow.Presenter != null)
            {
                e.NewRow.Presenter.Visibility = System.Windows.Visibility.Visible;
                e.NewRow.Visibility = Visibility.Visible;
            }
            else
            {
                e.NewRow.Visibility = System.Windows.Visibility.Visible;
                e.NewRow.DataGrid.ScrollIntoView(e.NewRow.Index, 0);
            }
        }

        private void dg_CommittingNewRow(object sender, DataGridEndingNewRowEventArgs e)
        {
            if (e.NewRow.Presenter != null)
            {
                //e.NewRow.Presenter.Visibility = System.Windows.Visibility.Collapsed;
                e.NewRow.Visibility = Visibility.Collapsed;
            }
            else
            {
                e.NewRow.Visibility = System.Windows.Visibility.Collapsed;
                e.NewRow.DataGrid.ScrollIntoView(e.NewRow.Index, 0);
            }
        }

        private void dg_CancelingNewRow(object sender, DataGridEndingNewRowEventArgs e)
        {
            if (e.NewRow.Presenter != null)
            {
                //e.NewRow.Presenter.Visibility = System.Windows.Visibility.Collapsed;
                e.NewRow.Visibility = Visibility.Collapsed;
            }
            else
            {
                e.NewRow.Visibility = System.Windows.Visibility.Collapsed;
                e.NewRow.DataGrid.ScrollIntoView(e.NewRow.Index, 0);
            }
        }

        private static System.Runtime.Serialization.ObjectIDGenerator objectIDGenerator = new System.Runtime.Serialization.ObjectIDGenerator();

        private void dg_Loaded(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            
            for (int idx = 0; idx < dg.ContextMenu.Items.Count; idx++)
            {
                MenuItem item = dg.ContextMenu.Items[idx] as MenuItem;
                if (item == null) continue;

                switch (item.Header.ToString())
                {
                    case "EXCELSAVE":
                        item.Tag = dg;
                        item.Header = ObjectDic.Instance.GetObjectName(item.Header.ToString());
                        System.Windows.Controls.Image imgExcelSave = new System.Windows.Controls.Image();
                        imgExcelSave.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_table_excel.png", UriKind.Relative));
                        imgExcelSave.Stretch = System.Windows.Media.Stretch.Fill;
                        item.Icon = imgExcelSave;
                        item.Click -= cmnuExcel_Click;
                        item.Click += cmnuExcel_Click;
                        break;

                    case "EXCELSAVEANDOPEN":
                        item.Tag = dg;
                        item.Header = ObjectDic.Instance.GetObjectName(item.Header.ToString());
                        System.Windows.Controls.Image imgExcelSaveAndOpen = new System.Windows.Controls.Image();
                        imgExcelSaveAndOpen.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_table_excel.png", UriKind.Relative));
                        imgExcelSaveAndOpen.Stretch = System.Windows.Media.Stretch.Fill;
                        item.Icon = imgExcelSaveAndOpen;
                        item.Click -= cmnuOpenExcel_Click;
                        item.Click += cmnuOpenExcel_Click;
                        break;

                    case "COPYWITHHEADER":
                        item.Tag = dg;
                        item.Header = ObjectDic.Instance.GetObjectName(item.Header.ToString());
                        System.Windows.Controls.Image imgCopyWithHeader = new System.Windows.Controls.Image();
                        imgCopyWithHeader.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                        imgCopyWithHeader.Stretch = System.Windows.Media.Stretch.Fill;
                        item.Icon = imgCopyWithHeader;
                        item.Click -= cmnuCopyWithHeader_Click;
                        item.Click += cmnuCopyWithHeader_Click;
                        break;

                    case "CopyAllWithHeader":
                        item.Tag = dg;
                        item.Header = ObjectDic.Instance.GetObjectName(item.Header.ToString());
                        System.Windows.Controls.Image imgCopyAllWithHeader = new System.Windows.Controls.Image();
                        imgCopyAllWithHeader.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/LGC.GMES.MES.ControlsLibrary;Component/Images/LGC/btn_i_main_confirm_disable.png", UriKind.Relative));
                        imgCopyAllWithHeader.Stretch = System.Windows.Media.Stretch.Fill;
                        item.Icon = imgCopyAllWithHeader;
                        item.Click -= cmnuCopyAllWithHeader_Click;
                        item.Click += cmnuCopyAllWithHeader_Click;
                        break;

                    case "CopyColumnWithAllData":
                        item.Tag = dg;
                        item.Header = ObjectDic.Instance.GetObjectName(item.Header.ToString());
                        item.Click -= cmnuCopyColumnWithAllData_Click;
                        item.Click += cmnuCopyColumnWithAllData_Click;
                        break;
                }
            }

            bool isFirst = false;
            objectIDGenerator.GetId(dg, out isFirst);

            if (!isFirst)
                return;

            // 폴란드 숫자 구분 기호 때문에 정상적으로 숫자표기가 안되는 문제가 있어
            // 임시적으로 폴란드는 en-US 방식으로 강제 정의
            // 2019-10-01 오화백  폴란드, 러시아어 추가
            if (LoginInfo.LANGID == "pl-PL" || LoginInfo.LANGID == "ru-RU" || LoginInfo.LANGID == "uk-UA")
                dg.Language = System.Windows.Markup.XmlLanguage.GetLanguage("en-US");
            else
                dg.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LoginInfo.LANGID);

            applyObejctDicToDataGrid(dg);
            dg.Columns.CollectionChanged -= Columns_CollectionChanged;
            dg.Columns.CollectionChanged += Columns_CollectionChanged;

            dg.BeginningNewRow -= new EventHandler<DataGridBeginningNewRowEventArgs>(dg_BeginningNewRow);
            dg.CommittingNewRow -= new EventHandler<DataGridEndingNewRowEventArgs>(dg_CommittingNewRow);
            dg.CancelingNewRow -= new EventHandler<DataGridEndingNewRowEventArgs>(dg_CancelingNewRow);

            dg.BeginningNewRow += new EventHandler<DataGridBeginningNewRowEventArgs>(dg_BeginningNewRow);
            dg.CommittingNewRow += new EventHandler<DataGridEndingNewRowEventArgs>(dg_CommittingNewRow);
            dg.CancelingNewRow += new EventHandler<DataGridEndingNewRowEventArgs>(dg_CancelingNewRow);
                        
            dg.MergingCells -= dg_MergingCells;
            dg.MergingCells += dg_MergingCells;
            
            dg.LoadedRowPresenter -= dg_LoadedRowPresenter;
            dg.LoadedRowPresenter += dg_LoadedRowPresenter;

            dg.DeletingRows -= dg_DeletingRows;
            dg.DeletingRows += dg_DeletingRows;

            dg.LoadedCellPresenter -= dg_LoadedCellPresenter;
            dg.LoadedCellPresenter += dg_LoadedCellPresenter;

            dg.BeganEdit -= dg_BeganEdit;
            dg.BeganEdit += dg_BeganEdit;

            dg.PreviewKeyDown -= dg_PreviewKeyDown;
            dg.PreviewKeyDown += dg_PreviewKeyDown;

            if (dg.TopRows.Count > 0)
                dg.Refresh();

            dg.BeginNewRow();
            dg.EndNewRow(false);
        }

        void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (DataGridColumn column in e.NewItems)
                    applyObejctDicToColumn(column);
        }

        void dg_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
            {
                DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                CheckBox cb = cell.Presenter.Content as CheckBox;

                if (cell.DataGrid != null)
                    cb.Checked += (s, arg) => cell.DataGrid.EndEdit();

                if (cell.DataGrid != null)
                    cb.Unchecked += (s, arg) => cell.DataGrid.EndEdit();
            }

            if (e.Column is C1.WPF.DataGrid.DataGridComboBoxColumn)
            {
                DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                C1.WPF.C1ComboBox cbo = cell.Presenter.Content as C1.WPF.C1ComboBox;
                cbo.AutoComplete = false;
                cbo.HorizontalAlignment = HorizontalAlignment.Stretch;
                cbo.Margin = new Thickness(0, 0, 0, 0);
                cbo.IsDropDownOpen = true;
                cbo.SelectedItemChanged += (s, arg) => cell.DataGrid.EndEdit();
            }

            // 그리드 숫자 입력 시 입력기가 중문인 경우 정상 입력 안되는 문제로 인한 수정.
            if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn && LoginInfo.CFG_SHOP_ID != null && LoginInfo.CFG_SHOP_ID.Equals("G182"))  // 임시 남경 소형만 적용 처리.
            {
                if (InputMethod.Current != null)
                    InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
            }
        }

        void dg_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
            {
                if (e.Cell.Presenter.Content is CheckBox)
                {
                    CheckBox cb = e.Cell.Presenter.Content as CheckBox;

                    cb.Checked += (s, arg) =>
                    {
                        if (e.Cell.DataGrid != null)
                            e.Cell.DataGrid.EndEdit();
                    };

                    cb.Unchecked += (s, arg) =>
                    {
                        if (e.Cell.DataGrid != null)
                            e.Cell.DataGrid.EndEdit();
                    };
                }
            }

            C1DataGrid dg = (sender as C1DataGrid);

            if (DataGridExtension.GetIsAlternatingRow(dg) == true)
            {
                //System.Diagnostics.Debug.WriteLine(dg.Name.ToString() + " - " + Convert.ToString(DataGridExtension.GetIsAlternatingRow(dg)));
                if (e.Cell.Row.Index >= e.Cell.DataGrid.TopRows.Count)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item && e.Cell.Presenter != null)
                    {
                        e.Cell.Presenter.Background = (e.Cell.Row.Index % 2) == (e.Cell.DataGrid.TopRows.Count % 2) ? e.Cell.DataGrid.RowBackground : e.Cell.DataGrid.AlternatingRowBackground;
                        e.Cell.Presenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                        e.Cell.Presenter.VerticalAlignment = VerticalAlignment.Stretch;
                    }
                }
            }

            //DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, e.Cell.Column.Index);

            //if (cell == (sender as C1DataGrid).CurrentCell)
            //{
            //    cell.Presenter.IsSelected = true;
            //}
            //else
            //{
            //    cell.Presenter.IsSelected = false;
            //}
        }

        void dg_LoadedRowPresenter(object sender, DataGridRowEventArgs e)
        {
            if (e.Row is DataGridNewRow)
            {
                if (e.Row.DataItem == null)
                {
                    e.Row.Presenter.Visibility = Visibility.Collapsed;
                    e.Row.Visibility = Visibility.Visible;
                }
                else
                {
                    e.Row.Presenter.Visibility = e.Row.Visibility;
                }
            }
        }

        void dg_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
                
            if (dg.TopRows.Count > 0 && sender.ToString().IndexOf("UcBaseDataGrid") < 0) // UcBaseDataGrid 컨트롤 제외 추가
            {
                DataGridRow[] _headerColumnRows = dg.TopRows.Take(dg.TopRows.Count).ToArray();

                var nonHeadersViewportRows = dg.Viewport.Rows.Where(r => !_headerColumnRows.Contains(r)).ToArray();

                // merge column & rows headers
                foreach (var range in Merge(System.Windows.Controls.Orientation.Horizontal, _headerColumnRows, dg.Columns.ToArray(), true).CheckCell())
                    e.Merge(range);
            }

            dataCellMerge(dg, e);
        }

        //기존에 BINDING 된 형식과 같은 크기로 BINDING하는 방식(VERTICALBYROWVALUE) 추가
        //2021-06-29 김민석 사원
        private void dataCellMerge(C1DataGrid dataGrid, DataGridMergingCellsEventArgs e)
        {
            int leftCol = -1;
            DataGridMergeMode mode = DataGridMergeMode.NONE;

            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (leftCol == -1)
                {
                    if (!DataGridMergeExtension.GetMergeMode(column).Equals(DataGridMergeMode.NONE))
                    {
                        leftCol = column.Index;
                        mode = DataGridMergeExtension.GetMergeMode(column);
                    }
                }
                else if (mode == DataGridMergeMode.VERTICALBYROWVALUE)
                {
                    if (mode != DataGridMergeMode.NONE)
                    {
                        Orientation orientation = (mode == DataGridMergeMode.HORIZONTAL) || (mode == DataGridMergeMode.HORIZONTALHIERARCHI) ? Orientation.Horizontal : Orientation.Vertical;
                        bool hierarchi = (mode == DataGridMergeMode.HORIZONTALHIERARCHI) || (mode == DataGridMergeMode.VERTICALHIERARCHI) ? true : false;
                        DataGridRow[] rows = null;           

                        if (dataGrid.TopRows.Count > 0)
                        {
                            DataGridRow[] _headerColumnRows = dataGrid.TopRows.Take(dataGrid.TopRows.Count).ToArray();
                            rows = dataGrid.Viewport.Rows.Where(r => !_headerColumnRows.Contains(r) && r.Type != DataGridRowType.Bottom).ToArray();
                        }
                        else
                        {
                            rows = dataGrid.Viewport.Rows.Where(r => r.Type != DataGridRowType.Bottom).ToArray();
                        }

                        DataGridColumn[] columns = dataGrid.Columns.Where(col => col.Index >= leftCol && col.Index < column.Index).ToArray();
                        DataGridColumn[] mainColumns = dataGrid.Columns.Where(col => col.Tag != null && col.Tag.ToString() == "MAIN").ToArray();


                        List<DataGridCellsRange> rangeList = Merge(orientation, rows, columns, hierarchi, groupColumns: mainColumns);

                        foreach (DataGridCellsRange range in rangeList.CheckCell())
                            e.Merge(range);
                    }

                    if (!DataGridMergeExtension.GetMergeMode(column).Equals(DataGridMergeMode.NONE))
                    {
                        leftCol = column.Index;
                        mode = DataGridMergeExtension.GetMergeMode(column);
                    }
                    else
                    {
                        leftCol = -1;
                        mode = DataGridMergeMode.NONE;
                    }
                }
                else if (mode.Equals(DataGridMergeExtension.GetMergeMode(column)) && mode != DataGridMergeMode.VERTICAL)
                {
                    continue;
                }
                else
                {
                    if (mode != DataGridMergeMode.NONE)
                    {
                        Orientation orientation = (mode == DataGridMergeMode.HORIZONTAL) || (mode == DataGridMergeMode.HORIZONTALHIERARCHI) ? Orientation.Horizontal : Orientation.Vertical;
                        bool hierarchi = (mode == DataGridMergeMode.HORIZONTALHIERARCHI) || (mode == DataGridMergeMode.VERTICALHIERARCHI) ? true : false;
                        DataGridRow[] rows = null;

                        if (dataGrid.TopRows.Count > 0)
                        {
                            DataGridRow[] _headerColumnRows = dataGrid.TopRows.Take(dataGrid.TopRows.Count).ToArray();
                            rows = dataGrid.Viewport.Rows.Where(r => !_headerColumnRows.Contains(r) && r.Type != DataGridRowType.Bottom).ToArray();
                        }
                        else
                        {
                            rows = dataGrid.Viewport.Rows.Where(r => r.Type != DataGridRowType.Bottom).ToArray();
                        }

                        DataGridColumn[] columns = dataGrid.Columns.Where(col => col.Index >= leftCol && col.Index < column.Index).ToArray();

                        List<DataGridCellsRange> rangeList = Merge(orientation, rows, columns, hierarchi);

                        foreach (DataGridCellsRange range in rangeList.CheckCell())
                            e.Merge(range);
                    }

                    if (!DataGridMergeExtension.GetMergeMode(column).Equals(DataGridMergeMode.NONE))
                    {
                        leftCol = column.Index;
                        mode = DataGridMergeExtension.GetMergeMode(column);
                    }
                    else
                    {
                        leftCol = -1;
                        mode = DataGridMergeMode.NONE;
                    }
                }
            }

            if (leftCol != -1 && mode != DataGridMergeMode.NONE)
            {
                Orientation orientation = (mode == DataGridMergeMode.HORIZONTAL) || (mode == DataGridMergeMode.HORIZONTALHIERARCHI) ? Orientation.Horizontal : Orientation.Vertical;
                bool hierarchi = (mode == DataGridMergeMode.HORIZONTALHIERARCHI) || (mode == DataGridMergeMode.VERTICALHIERARCHI) ? true : false;
                DataGridRow[] rows = null;

                if (dataGrid.TopRows.Count > 0)
                {
                    DataGridRow[] _headerColumnRows = dataGrid.TopRows.Take(dataGrid.TopRows.Count).ToArray();
                    rows = dataGrid.Viewport.Rows.Where(r => !_headerColumnRows.Contains(r) && r.Type != DataGridRowType.Bottom).ToArray();
                }
                else
                {
                    rows = dataGrid.Viewport.Rows.Where(r => r.Type != DataGridRowType.Bottom).ToArray();
                }


                DataGridColumn[] columns = dataGrid.Columns.Where(col => col.Index >= leftCol).ToArray();
                List<DataGridCellsRange> rangeList = Merge(orientation, rows, columns, hierarchi);

                foreach (DataGridCellsRange range in rangeList.CheckCell())
                    e.Merge(range);
            }
        }

        private List<DataGridCellsRange> Merge(Orientation orientation, DataGridRow[] rows, DataGridColumn[] columns, bool hierarchical, DataGridColumn[] groupColumns = null)
        {
            var merges = new List<DataGridCellsRange>();

            if ((rows.Length == 0) || (columns.Length == 0))
                return merges;

            var datagrid = rows[0].DataGrid;
            DataGridCellsRange currentRange = null;
            DataGridCellsRange valRange = null;

            var iterationLength = (orientation == Orientation.Vertical) ? rows.Length : columns.Length;
            int i = 0;

            while (i < iterationLength)
            {
                // skip empty cells 
                DataGridCell nextCell = null;
                DataGridCell valCell = null;

                while (nextCell == null && i < iterationLength)
                {
                    if (groupColumns != null)
                    {
                        valCell = (orientation == Orientation.Vertical) ? datagrid[rows[i], groupColumns[0]] : datagrid[rows[0], groupColumns[i]];

                    }

                    nextCell = (orientation == Orientation.Vertical) ? datagrid[rows[i], columns[0]] : datagrid[rows[0], columns[i]];

                    i++;

                }

                // there are no more cell in this column, end iteration
                if (nextCell == null)
                    break;

                // can expand the merge?
                if (CanMerge(orientation, currentRange, nextCell, valCell:valCell, valRange: valRange))
                {
                    // expand the merged range
                    currentRange = ExpandRange(currentRange, nextCell);
                    if(valCell != null)
                    {
                        valRange = ExpandRange(valRange, valCell);
                    }
                    
                }
                else
                {
                    // cannot merge anymore, add the last range we have
                    merges.Add(currentRange);
                    
                    currentRange = ExpandRange(null, nextCell);
                    if(valCell != null)
                    {
                        valRange = ExpandRange(null, valCell);
                    }
                    
                }
            }


            // add last merge to the collection
            if (currentRange != null)
                merges.Add(currentRange);

            // recursion
            var pendingColumns = (orientation == Orientation.Vertical) ? columns.Skip(1).ToArray() : columns;
            var pendingRows = (orientation == Orientation.Vertical) ? rows : rows.Skip(1).ToArray();
            var innerMerges = new List<DataGridCellsRange>();

            if (!hierarchical)
            {
                // treat each row independently
                // and add inner merges to the results
                var tmp = Merge(orientation, pendingRows, pendingColumns, hierarchical);
                merges.AddRange(tmp);
            }
            else
            {
                // merge in the other direction, bounding to the parent range limits
                foreach (var range in new List<DataGridCellsRange>(merges))
                {
                    innerMerges = (orientation == Orientation.Vertical)
                                ? Merge(orientation, range.Rows.ToArray(), pendingColumns, hierarchical)
                                : Merge(orientation, pendingRows, range.Columns.ToArray(), hierarchical);

                    // look into the inner merged ranges, to check if possible to expand the current merge in the other direction
                    var continueMerging = true;
                    var expandedRange = range;

                    while (innerMerges.Count > 0 && continueMerging)
                    {
                        var tmp = innerMerges[0];

                        if (CanMerge(orientation.Opposite(), expandedRange, tmp))
                        {
                            expandedRange = ExpandRange(expandedRange, tmp.BottomRightCell);
                            innerMerges.Remove(tmp);
                        }
                        else
                        {
                            continueMerging = false;
                        }
                    }

                    // replace range for the expanded one
                    if (expandedRange != range)
                        merges[merges.IndexOf(range)] = expandedRange;

                    // and add inner merges to the results
                    merges.AddRange(innerMerges);
                }
            }

            return merges;
        }

        private bool CanMerge(Orientation orientation, DataGridCellsRange currentRange , DataGridCellsRange newRange)
        {
            if (currentRange == null)
                return true;

            var datagrid = newRange.TopLeftCell.DataGrid;

            // 좌측상단 좌표가 우측하단 좌표보다 크면 안되어 오류방지를 위해 추가
            if (currentRange.TopLeftCell.Row.Index > newRange.BottomRightCell.Row.Index ||
                currentRange.TopLeftCell.Column.DisplayIndex > newRange.BottomRightCell.Column.DisplayIndex) return false;

            if (orientation == Orientation.Vertical)
            {
                return (object.Equals(GetCellValue(currentRange.TopLeftCell), GetCellValue(newRange.TopLeftCell))
                        && (currentRange.TopLeftCell.Column == newRange.TopLeftCell.Column)
                        && (currentRange.BottomRightCell.Column == newRange.BottomRightCell.Column));
            }
            else
            {
                return (object.Equals(GetCellValue(currentRange.TopLeftCell), GetCellValue(newRange.TopLeftCell))
                        && (currentRange.TopLeftCell.Row == newRange.TopLeftCell.Row)
                        && (currentRange.BottomRightCell.Row == newRange.BottomRightCell.Row));
            }
        }

        private bool CanMerge(Orientation orientation, DataGridCellsRange currentRange, DataGridCell cell, DataGridCell valCell = null, DataGridCellsRange valRange = null)
        {
            if (currentRange == null)
                return true;

            var datagrid = cell.DataGrid;
            var last = currentRange.BottomRightCell;
            var first = currentRange.TopLeftCell;

            // 좌측상단 좌표가 우측하단 좌표보다 크면 안되어 오류방지를 위해 추가
            if (currentRange.TopLeftCell.Row.Index > currentRange.BottomRightCell.Row.Index ||
                currentRange.TopLeftCell.Column.DisplayIndex > currentRange.BottomRightCell.Column.DisplayIndex) return false;

            if (orientation == Orientation.Vertical)
            {
                if(valCell == null || valRange == null)
                {
                    return (object.Equals(GetCellValue(first), GetCellValue(cell)))
                    && (last.Row.Index == cell.Row.Index - 1)
                    && (last.Row.ParentGroup == cell.Row.ParentGroup)
                    && (datagrid.FreezingSection(cell) == datagrid.FreezingSection(last));
                }
                else
                {
                    var valFirst = valRange.TopLeftCell;

                    return (object.Equals(GetCellValue(first), GetCellValue(cell)))
                    && (last.Row.Index == cell.Row.Index - 1)
                    && (last.Row.ParentGroup == cell.Row.ParentGroup)
                    && (datagrid.FreezingSection(cell) == datagrid.FreezingSection(last))
                    && (object.Equals(GetCellValue(valFirst), GetCellValue(valCell)));
                }

            }
            else
            {
                return (object.Equals(GetCellValue(first), GetCellValue(cell)))
                       && (last.Column.DisplayIndex == cell.Column.DisplayIndex - 1)
                       && (last.Row.ParentGroup == cell.Row.ParentGroup)
                       && (datagrid.FreezingSection(cell) == datagrid.FreezingSection(last));
            }
        }

        public DataGridCellsRange ExpandRange(DataGridCellsRange currentRange, DataGridCell cell)
        {
            if (currentRange == null)
                return new DataGridCellsRange(cell);
            else
                return new DataGridCellsRange(currentRange.TopLeftCell, cell);
        }

        private object GetCellValue(DataGridCell cell)
        {
            // We used the binding here previously, but that doesn't work for column headers.
            if (cell.Row.Index < cell.DataGrid.TopRows.Count)
            {
                return (cell.Column.Header is List<string> && (cell.Column.Header as List<string>).Count > cell.Row.Index) ?
                    (cell.Column.Header as List<string>)[cell.Row.Index] : cell.Column.Header;
            }
            else
            {
                object content = cell.Presenter;

                while (true)
                {
                    if (content is ContentControl)
                        content = (content as ContentControl).Content;
                    else if (content is TextBlock)
                        return (content as TextBlock).Text;
                    else if (content is TextBox)
                        return (content as TextBox).Text;
                    else
                        return content;
                }
            }
        }

        void dg_DeletingRows(object sender, DataGridDeletingRowsEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg.CanUserRemoveRows != true)
            {
                e.Cancel = true;

                foreach (DataGridRow row in e.DeletedRows)
                    row.Visibility = Visibility.Collapsed;
            }
        }

        private void applyObejctDicToDataGrid(C1DataGrid dataGrid)
        {
            foreach (DataGridColumn column in dataGrid.Columns)
                applyObejctDicToColumn(column);
        }

        private void applyObejctDicToColumn(DataGridColumn column)
        {
            if (column.Header is List<string>)
            {
                string[] headers = (column.Header as List<string>).ToArray();

                for (int index = 0; index < headers.Length; index++)
                {
                    // 번역 스킵 옵션 : 앞 3개 문자가 "[*]" 일경우
                    if (headers[index].Length > 3 && headers[index].Substring(0, 3).Equals("[*]"))
                    {
                        headers[index] = headers[index].Replace("[*]", "");
                    }
                    else
                    {
                        headers[index] = ObjectDic.Instance.GetObjectName(headers[index]);
                    }
                }
                column.Header = new List<string>(headers);
            }
            else if (column.Header is string)
            {
                string header = column.Header as string;

                // 번역 스킵 옵션 : 앞 3개 문자가 "[*]" 일경우
                if (header.Length > 3 && header.Substring(0, 3).Equals("[*]"))
                {
                    column.Header = header.Replace("[*]", "");
                }
                else
                {
                    column.Header = ObjectDic.Instance.GetObjectName(column.Header.ToString());
                }
            }
        }

        void cmnuExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem itemmenu = sender as MenuItem;
                ContextMenu itemcontext = itemmenu.Parent as ContextMenu;
                C1DataGrid dg = itemcontext.PlacementTarget as C1DataGrid;
                new ExcelExporter().Export(dg);

                //C1DataGrid dg = item.Tag as C1DataGrid;
                //if (dg != null)
                //{
                //    new ExcelExporter().Export(dg);
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FRA0004"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        void cmnuOpenExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem itemmenu = sender as MenuItem;
                ContextMenu itemcontext = itemmenu.Parent as ContextMenu;
                C1DataGrid dg = itemcontext.PlacementTarget as C1DataGrid;
                new ExcelExporter().Export(dg, null, true);

                //C1DataGrid dg = item.Tag as C1DataGrid;
                //if (dg != null)
                //{
                //    new ExcelExporter().Export(dg);
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FRA0004"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        protected virtual void dg_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                C1DataGrid dg = sender as C1DataGrid;
                DataGridCell dgc = dg.CurrentCell;

                if (dg != null && dgc != null)
                {
                    if (dg.SelectionMode == C1.WPF.DataGrid.DataGridSelectionMode.MultiRange)
                    {
                        System.Windows.Clipboard.SetText(Clipboard_SetText(dg.Selection.SelectedCells, false));
                    }
                    else
                    {
                        DataGridCell cell = dg.GetCell(dgc.Row.Index, dgc.Column.Index);

                        if (cell != null && cell.Value != null)
                        {
                            if (string.Equals(cell.Row.DataItem.ToString(), cell.Value.ToString()))
                                Clipboard.SetText("");
                            else
                                Clipboard.SetText(cell.Value.ToString());
                        }
                    }
                }
            }
        }

        void cmnuCopyWithHeader_Click(object sender, RoutedEventArgs e)
        {
            MenuItem itemmenu = sender as MenuItem;

            if (itemmenu != null)
            {
                ContextMenu itemcontext = itemmenu.Parent as ContextMenu;

                if (itemcontext != null)
                {
                    C1DataGrid dg = itemcontext.PlacementTarget as C1DataGrid;

                    if (dg != null)
                        System.Windows.Clipboard.SetText(Clipboard_SetText(dg.Selection.SelectedCells, true));
                }
            }
        }

        void cmnuCopyAllWithHeader_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            string str = string.Empty;

            ContextMenu itemContext = menuItem?.Parent as ContextMenu;
            C1DataGrid dg = itemContext?.PlacementTarget as C1DataGrid;

            if (dg != null)
            {
                //Header Text 적출
                if (dg.TopRows.Count == 0 && dg.HeadersVisibility != C1.WPF.DataGrid.DataGridHeadersVisibility.None) //Header가 Single Row이고 HeaderVisibility가 None이 아닌 경우 별도의 Header Text 적출 로직 적용
                {
                    for (int i = 0; i < dg.Columns.Count; i++)
                        if (dg.Columns[i].Visibility == Visibility.Visible) //Visibility.Visible인 것만 추출 (화면에 보이는 그대로 적출)
                            str += dg.Columns[i].ActualGroupHeader?.ToString() + (i.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)

                    str += "\r\n"; //Row가 바뀌므로 줄 바꿈
                }
                else
                {
                    for (int i = 0; i < dg.TopRows.Count; i++) //Header 적출
                    {
                        for (int j = 0; j < dg.Columns.Count; j++)
                            if (dg.Columns[j].Visibility == Visibility.Visible) //Visibility.Visible인 것만 추출 (화면에 보이는 그대로 적출)
                                str += dg.GetCell(i, j)?.Value + (j.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)

                        str += "\r\n"; //Row가 바뀌므로 줄 바꿈
                    }
                }

                //본문 내용 적출
                for (int i = dg.TopRows.Count; i < dg.Rows.Count; i++)
                {
                    for (int j = 0; j < dg.Columns.Count; j++)
                    {
                        if (dg.Columns[j].Visibility == Visibility.Visible) //Visibility.Visible인 것만 추출 (화면에 보이는 그대로 적출)
                        {
                            if (dg.GetCell(i, j).Value != null && dg.GetCell(i, j).Value.ToString().IndexOf("\n") > -1)
                            {
                                // 한 셀에 여러라인이 있을때 처리
                                str += ("\"" + dg.GetCell(i, j).Value?.ToString() + "\"").Replace("\r\n", " ") + (j.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)
                            }
                            else
                            {
                                str += dg.GetCell(i, j).Value?.ToString().Replace("\r\n", " ") + (j.Equals(dg.Columns.Count - 1) ? string.Empty : "\t"); //Tab 구분자 삽입 (마지막 Cell인 경우 불필요)
                            }
                        }
                    }

                    if (!i.Equals(dg.Rows.Count - 1)) //마지막 Row가 아닐 경우
                        str += "\r\n"; //줄 바꿈
                }

                Clipboard.SetText(str.Replace("System.Data.DataRowView", string.Empty)); //ClipBoard에 적출 내용 복사
            }
        }

        void cmnuCopyColumnWithAllData_Click(object sender, RoutedEventArgs e)
        {
            MenuItem itemMenu = sender as MenuItem;

            if (itemMenu != null)
            {
                ContextMenu itemContext = itemMenu.Parent as ContextMenu;

                if (itemContext != null)
                {
                    C1DataGrid dg = itemContext.PlacementTarget as C1DataGrid;

                    if (dg != null)
                    {
                        var column = dg.Columns[dg.CurrentCell.Column.DisplayIndex];

                        //foreach(C1DataGrid)
                    }
                    //Clipboard.SetText(Clipboard_SetText(dg.Selection.SelectedCells, true));
                }
            }
        }

        private string Clipboard_SetText(DataGridSelectedItemsCollection<DataGridCell> selectedCells, bool bheader)
        {
            try
            {
                string sClipText = string.Empty;

                if (selectedCells.Count > 0)
                {
                    int nRowIndex = 0;

                    if (bheader)
                    {
                        nRowIndex = selectedCells[0].Row.Index;

                        for (int i = 0; i < selectedCells.Count; i++)
                        {
                            if (nRowIndex == selectedCells[i].Row.Index)
                            {
                                string sColumn = selectedCells[i].Column.GetColumnText();
                                string[] sList = sColumn.Split(new string[] { ", " }, StringSplitOptions.None);

                                bool bMerge = true;

                                foreach (string str in sList)
                                    if (str != sList[0])
                                        bMerge = false;

                                sClipText += bMerge ? sList[0] : sColumn;

                                if (selectedCells.Count - 1 == i)
                                    sClipText += "\r\n";
                                else
                                    sClipText += "\t";
                            }
                            else
                            {
                                sClipText = sClipText.Substring(0, sClipText.Length - 1);
                                sClipText += "\r\n";
                                break;
                            }

                            nRowIndex = selectedCells[i].Row.Index;
                        }
                    }

                    nRowIndex = selectedCells[0].Row.Index;

                    for (int i = 0; i < selectedCells.Count; i++)
                    {
                        if (nRowIndex == selectedCells[i].Row.Index)
                        {
                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += selectedCells[i].Text;

                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += "\t";
                        }
                        else
                        {
                            sClipText = sClipText.Substring(0, sClipText.Length - 1);
                            sClipText += "\r\n";

                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += selectedCells[i].Text;

                            if (selectedCells[i].Text.Contains("\n"))
                                sClipText += "\"";

                            sClipText += "\t";
                        }

                        nRowIndex = selectedCells[i].Row.Index;
                    }

                    sClipText = sClipText.Substring(0, sClipText.LastIndexOf('\t'));
                }

                //sClipText = "포장완료\t\t\"NG18 [24]\nNG21 [13]\nNG22 [90]\nNG23 [29]\nNH15 [36]\"\n입고 완료\t 출고취소 팔레트\t \"NG24 [32]\nNG25 [78]\nNG28 [78]\nNG29 [4]\"";
                return sClipText;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string Clipboard_SetText(C1DataGrid sourceGrid)
        {
            try
            {
                string sClipText = string.Empty;

                return sClipText;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }

    public static class DataGridHandler
    {
        public static IEnumerable GetUnchangedItems(this C1DataGrid dataGrid)
        {
            dataGrid.EndNewRow(true);
            dataGrid.EndEditRow(true);
            IEnumerable<object[]> itemArryList = (from DataGridRow row in dataGrid.Rows where row.Visibility != Visibility.Collapsed && row.DataItem != null && (row.DataItem as DataRowView).Row.RowState == DataRowState.Unchanged select (row.DataItem as DataRowView).Row.ItemArray);
            DataTable table = (dataGrid.ItemsSource as DataView).Table.Clone();

            foreach (object[] itemArray in itemArryList)
            {
                DataRow row = table.NewRow();
                row.ItemArray = itemArray;
                table.Rows.Add(row);
            }

            table.AcceptChanges();
            return DataTableConverter.Convert(table);
        }

        public static IEnumerable GetAddedItems(this C1DataGrid dataGrid)
        {
            dataGrid.EndNewRow(true);
            dataGrid.EndEditRow(true);
            IEnumerable<object[]> itemArryList = (from DataGridRow row in dataGrid.Rows where row.Visibility != Visibility.Collapsed && row.DataItem != null && (row.DataItem as DataRowView).Row.RowState == DataRowState.Added select (row.DataItem as DataRowView).Row.ItemArray);
            DataTable table = (dataGrid.ItemsSource as DataView).Table.Clone();

            foreach (object[] itemArray in itemArryList)
            {
                DataRow row = table.NewRow();
                row.ItemArray = itemArray;
                table.Rows.Add(row);
            }

            table.AcceptChanges();
            return DataTableConverter.Convert(table);
        }

        public static IEnumerable GetModifiedItems(this C1DataGrid dataGrid)
        {
            dataGrid.EndNewRow(true);
            dataGrid.EndEditRow(true);
            IEnumerable<object[]> itemArryList = (from DataGridRow row in dataGrid.Rows where row.Visibility != Visibility.Collapsed && row.DataItem != null && (row.DataItem as DataRowView).Row.RowState == DataRowState.Modified select (row.DataItem as DataRowView).Row.ItemArray);
            DataTable table = (dataGrid.ItemsSource as DataView).Table.Clone();

            foreach (object[] itemArray in itemArryList)
            {
                DataRow row = table.NewRow();
                row.ItemArray = itemArray;
                table.Rows.Add(row);
            }

            table.AcceptChanges();
            return DataTableConverter.Convert(table);
        }

        public static IEnumerable GetDeletedItems(this C1DataGrid dataGrid)
        {
            dataGrid.EndNewRow(true);
            dataGrid.EndEditRow(true);
            IEnumerable<object[]> itemArryList = (from DataGridRow row in dataGrid.Rows where row.Visibility == Visibility.Collapsed && row.DataItem != null && (row.DataItem as DataRowView).Row.RowState != DataRowState.Added && (row.DataItem as DataRowView).Row.RowState != DataRowState.Detached select (row.DataItem as DataRowView).Row.ItemArray);
            DataTable table = (dataGrid.ItemsSource as DataView).Table.Clone();

            foreach (object[] itemArray in itemArryList)
            {
                DataRow row = table.NewRow();
                row.ItemArray = itemArray;
                table.Rows.Add(row);
            }

            table.AcceptChanges();
            return DataTableConverter.Convert(table);
        }

        public static IEnumerable GetCurrentItems(this C1DataGrid dataGrid)
        {
            dataGrid.EndNewRow(true);
            dataGrid.EndEditRow(true);
            IEnumerable<object[]> itemArryList = (from DataGridRow row in dataGrid.Rows where row.Visibility != Visibility.Collapsed && row.DataItem != null && (row.DataItem as DataRowView).Row.RowState != DataRowState.Detached && (row.DataItem as DataRowView).Row.RowState != DataRowState.Deleted select (row.DataItem as DataRowView).Row.ItemArray);
            DataTable table = (dataGrid.ItemsSource as DataView).Table.Clone();

            foreach (object[] itemArray in itemArryList)
            {
                DataRow row = table.NewRow();
                row.ItemArray = itemArray;
                table.Rows.Add(row);
            }

            table.AcceptChanges();
            return DataTableConverter.Convert(table);
        }

        public static int GetRowCount(this C1DataGrid dataGrid)
        {
            dataGrid.EndNewRow(true);
            dataGrid.EndEditRow(true);
            int cnt = dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count;
            return cnt;
        }
    }

    public enum DataGridMergeMode { NONE, HORIZONTAL, VERTICAL, HORIZONTALHIERARCHI, VERTICALHIERARCHI, VERTICALBYROWVALUE };

    public class DataGridMergeExtension : DependencyObject
    {
        public static readonly DependencyProperty MergeModeProperty = DependencyProperty.RegisterAttached("MergeMode", typeof(DataGridMergeMode), typeof(DataGridMergeExtension), new PropertyMetadata(DataGridMergeMode.NONE));

        public static void SetMergeMode(DependencyObject column, DataGridMergeMode value)
        {
            column.SetValue(MergeModeProperty, value);
        }

        public static DataGridMergeMode GetMergeMode(DependencyObject column)
        {
            return (DataGridMergeMode)column.GetValue(MergeModeProperty);
        }
    }

    public static class MergeExtensions
    {
        public static void ReMerge(this C1DataGrid datagrid)
        {
            datagrid.Refresh();
        }

        /// <summary>
        /// Returns a value different value for each viewport section (having freezing into account).
        /// The matrix is defined as follows, to facilitate calculations.
        /// 
        ///     -5  |  -3  |  -1
        ///    -------------------
        ///     -2  |   0  |   2
        ///     -------------------
        ///      1  |   3  |   5
        /// </summary>
        /// <param name="datagrid"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static int FreezingSection(this C1DataGrid datagrid, DataGridCell cell)
        {
            int section = 0;

            // vertical freezing
            if (cell.Row.Index < datagrid.FrozenTopRowsCount)
                section -= 3;
            else if ((datagrid.Rows.Count - cell.Row.Index) < datagrid.FrozenBottomRowsCount)
                section += 3;

            // horizontal freezing (only supports left freezing by now)
            if (cell.Column.DisplayIndex < datagrid.FrozenColumnCount)
                section -= 2;
            //else if ((datagrid.Columns.Count - cell.Column.DisplayIndex) < datagrid.RightFrozenColumnCount)
            //    section += 2;

            return section;
        }

        public static Orientation Opposite(this Orientation orientation)
        {
            return (orientation == Orientation.Vertical)
                    ? Orientation.Horizontal
                    : Orientation.Vertical;
        }

        public static List<DataGridCellsRange> CheckCell(this List<DataGridCellsRange> cellsRange)
        {
            try
            {
                if (cellsRange == null) return new List<DataGridCellsRange>();

                for (int index = cellsRange.Count - 1; index >= 0; index--)
                {
                    DataGridCellsRange range = cellsRange[index];
                    if (range.IsSingleCell())
                    {
                        cellsRange.RemoveAt(index);
                    }
                }
                return cellsRange;
            }
            catch
            {
                return cellsRange;
            }
        }
    }

    public class DataGridExtension : DependencyObject
    {
        public static DependencyProperty IsAlternatingRowProperty = DependencyProperty.RegisterAttached("IsAlternatingRow", typeof(bool), typeof(UserControl), new PropertyMetadata(true, IsAlternatingRowPropertyChanged));

        public static void SetIsAlternatingRow(DependencyObject dg, bool value)
        {
            dg.SetValue(IsAlternatingRowProperty, value);
        }

        public static bool GetIsAlternatingRow(DependencyObject dg)
        {
            return (bool)dg.GetValue(IsAlternatingRowProperty);
        }

        public static void IsAlternatingRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}