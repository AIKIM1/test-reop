/*************************************************************************************
 Created Date : 2020.12.23
      Creator : 조영대
   Decription : DataGrid Extension
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.23  조영대 : Initial Created
  2021.02.26  조영대 : Method 추가 및 수정
  2022.07.14  조영대 : CheckBox 컬럼 Header Check All 추가 확장 메소드 추가
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace C1.WPF.DataGrid
{
    public static class DataGridExtensions
    {
        /// <summary>
        /// 그리드 데이터를 바인딩 합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="dt">Data</param>
        /// <param name="iFO">FrameOperation</param>
        /// <param name="isAutoWidth">너비 자동설정 유무</param>
        /// <param name="filter">필</param>
        public static void SetItemsSource(this C1DataGrid dataGrid, DataTable dt, LGC.GMES.MES.Common.IFrameOperation iFO, bool isAutoWidth = false, string filter = null)
        {
            dataGrid.ClearRows();

            DataView dv = dt.Copy().AsDataView();
            if (!string.IsNullOrEmpty(filter))
            {
                dv.RowFilter = filter;    
            }
            
            dataGrid.ItemsSource = dv;
            
            foreach (var col in dataGrid.FilteredColumns)
            {
                foreach (var colFilter in col.FilterState.FilterInfo)
                {
                    if (colFilter.FilterType == DataGridFilterType.Text)
                        colFilter.Value = string.Empty;
                }
            }

            if (dt.Rows.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + LGC.GMES.MES.Common.MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(dt.Rows.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && dt.Rows.Count > 0) dataGrid.AllColumnsWidthAuto();
            }
        }

        /// <summary>
        /// 그리드의 모든 컬럼 너비를 자동으로 설정합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        public static void AllColumnsWidthAuto(this C1DataGrid dataGrid)
        {
            dataGrid.Columns.Where(w => !w.Width.IsStar).ToList()
                .ForEach(x => x.Width = new DataGridLength(1, C1.WPF.DataGrid.DataGridUnitType.Auto));
        }

        /// <summary>
        /// 필터를 해제 합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        public static void ClearFilter(this C1DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            if (dataGrid.Columns.Count > 0)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dataGrid.FilterBy(dataGrid.Columns[0], null);
                }));                
            }
        }

        /// <summary>
        /// Grid 의 모든 Row 를 삭제합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        public static void ClearRows(this C1DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            DataTable dtClear = dataGrid.GetDataTable();
            if (dtClear != null && dtClear.Rows.Count > 0)
            {

                dtClear.Rows.Clear();
                dataGrid.ItemsSource = dtClear.AsDataView();

                dataGrid.Refresh();
            }
        }

        /// <summary>
        /// Grid 에 Row 를 추가합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowCount">Row 개수</param>
        public static void AddRows(this C1DataGrid dataGrid, int rowCount)
        {
            try
            {
                DataTable dt = null;

                if (dataGrid.ItemsSource != null)
                {
                    for (int i = 0; i < rowCount; i++)
                    {
                        dt = DataTableConverter.Convert(dataGrid.ItemsSource);
                        DataRow newRow = dt.NewRow();
                        dt.Rows.Add(newRow);
                    }
                }
                else
                {
                    dt = new DataTable();

                    foreach (DataGridColumn col in dataGrid.Columns)
                    {
                        if (!dt.Columns.Contains(col.Name)) dt.Columns.Add(col.Name);
                    }

                    for (int i = 0; i < rowCount; i++)
                    {
                        DataRow newRow = dt.NewRow();
                        dt.Rows.Add(newRow);
                    }
                }
                if (dt != null)
                {
                    dt.AcceptChanges();
                    dataGrid.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                  System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Cell 을 클릭한지 유무 확인   
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns></returns>
        public static bool IsClickedCell(this C1DataGrid dataGrid)
        {
            DataGridCell cell = dataGrid.GetCellFromPoint(System.Windows.Input.Mouse.GetPosition(null));
            if (cell == null || dataGrid.CurrentRow == null) return false;

            return true;
        }

        /// <summary>
        /// 선택된 로우가 있는지 판별합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">체크 컬럼명</param>
        /// <returns></returns>
        public static bool IsCheckedRow(this C1DataGrid dataGrid, string sColumnName)
        {
            if (dataGrid == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return false;

            if (!dataGrid.Columns.Contains(sColumnName))
                return false;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                int intResult = 0;
                bool boolResult = false;

                string value = DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)?.ToString();
                
                if (int.TryParse(value, out intResult))
                {
                    if (intResult > 0) return true;
                }
                else if (bool.TryParse(value, out boolResult))
                {
                    if (boolResult) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 해당행의 컬럼이 선택되었는지 확인한다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">체크 컬럼명</param>
        /// <param name="rowIndex">행</param>
        /// <returns></returns>
        public static bool IsCheckedRow(this C1DataGrid dataGrid, string sColumnName, int rowIndex)
        {
            if (dataGrid == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return false;

            if (!dataGrid.Columns.Contains(sColumnName))
                return false;

            DataRow drRow = GetDataRow(dataGrid, rowIndex);
            if (drRow == null) return false;

            if (drRow.RowState == DataRowState.Deleted || drRow.RowState == DataRowState.Detached) return false;

            if (drRow[sColumnName] == null) return false;
            if (drRow[sColumnName].ToString() == string.Empty) return false;

            if (drRow[sColumnName] != null)
            {
                int intResult = 0;
                bool boolResult = false;

                string value = drRow[sColumnName].ToString();

                if (int.TryParse(value, out intResult))
                {
                    if (intResult > 0) return true;
                }
                else if (bool.TryParse(value, out boolResult))
                {
                    if (boolResult) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 선택된 로우의 행번호를 List<int> 형식으로 반환 합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">Column Name</param>
        /// <returns></returns>
        public static List<int> GetCheckedRowIndex(this C1DataGrid dataGrid, string sColumnName)
        {
            List<int> valueList = new List<int>();

            if (dataGrid == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return valueList;

            if (!dataGrid.Columns.Contains(sColumnName))
                return valueList;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                int intResult = 0;
                bool boolResult = false;

                string value = DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)?.ToString();

                if (int.TryParse(value, out intResult))
                {
                    if (intResult > 0) valueList.Add(i);
                }
                else if (bool.TryParse(value, out boolResult))
                {
                    if (boolResult) valueList.Add(i);
                }
            }

            return valueList;
        }

        /// <summary>
        /// 선택된 로우의 DataRow 를 List<DataRow> 형식으로 반환 합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">Column Name</param>
        /// <returns></returns>
        public static List<DataRow> GetCheckedDataRow(this C1DataGrid dataGrid, string sColumnName)
        {
            List<DataRow> returnValue = new List<DataRow>();

            if (dataGrid == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return returnValue;

            if (!dataGrid.Columns.Contains(sColumnName))
                return returnValue;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                int intResult = 0;
                bool boolResult = false;

                string value = DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)?.ToString();

                DataRowView drv = dataGrid.Rows[i].DataItem as DataRowView;

                if (int.TryParse(value, out intResult))
                {
                    if (intResult > 0) returnValue.Add(drv.Row);
                }
                else if (bool.TryParse(value, out boolResult))
                {
                    if (boolResult) returnValue.Add(drv.Row);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// 선택된 첫번째 행에서 값을 가져온다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="chkeckColumnName">CheckBox 컬럼명</param>
        /// <param name="valueColumnName">값을 가져올 컬럼명</param>
        /// <returns>반환값</returns>
        public static object GetCheckedFirstValue(this C1DataGrid dataGrid, string chkeckColumnName, string valueColumnName)
        {
            object returnValue = null;

            if (dataGrid == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return returnValue;

            if (!dataGrid.Columns.Contains(chkeckColumnName) || !dataGrid.Columns.Contains(valueColumnName))
                return returnValue;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                int intResult = 0;
                bool boolResult = false;

                string value = DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, chkeckColumnName)?.ToString();

                DataRowView drv = dataGrid.Rows[i].DataItem as DataRowView;

                if (int.TryParse(value, out intResult))
                {
                    if (intResult > 0) return drv[valueColumnName];
                }
                else if (bool.TryParse(value, out boolResult))
                {
                    if (boolResult) return drv[valueColumnName];
                }

            }

            return returnValue;
        }


        /// <summary>
        /// DataTable 을 가져옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="isCopy">True 일때 복사본(Default), fals 일때 원본</param>
        /// <returns></returns>
        public static DataTable GetDataTable(this C1DataGrid dataGrid, bool isCopy = true)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return null;

            DataTable dtGrid = null;

            if (isCopy)
            {
                dtGrid = ((DataView)dataGrid.ItemsSource).ToTable();
            }
            else
            {
                DataView dvGrid = dataGrid.ItemsSource as DataView;
                dtGrid = dvGrid.Table;
            }

            return dtGrid;
        }

        public static EnumerableRowCollection<DataRow> AsEnumerable(this C1DataGrid dataGrid, bool isCopy = true)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return null;

            DataTable dtGrid = null;

            if (isCopy)
            {
                dtGrid = ((DataView)dataGrid.ItemsSource).ToTable();
            }
            else
            {
                DataView dvGrid = dataGrid.ItemsSource as DataView;
                dtGrid = dvGrid.Table;
            }

            return dtGrid.AsEnumerable();
        }

        /// <summary>
        /// 현재 행의 DataRow 를 가져옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns></returns>
        public static DataRow GetDataRow(this C1DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return null;

            if (dataGrid.CurrentRow == null) return null;

            DataTable dtGrid = ((DataView)dataGrid.ItemsSource).Table;
            if (dtGrid != null)
            {
                return dtGrid.Rows[dataGrid.CurrentRow.Index];
            }

            return null;
        }

        /// <summary>
        /// 해당 행의 DataRow 를 가져옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="index">Row Index</param>
        /// <returns></returns>
        public static DataRow GetDataRow(this C1DataGrid dataGrid, int index)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return null;

            DataTable dtGrid = ((DataView)dataGrid.ItemsSource).Table;

            if (dtGrid != null && dtGrid.Rows.Count > index)
            {
                return dtGrid.Rows[index];
            }

            return null;
        }

        public static void SelectRow(this C1DataGrid dataGrid, int index)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null || index < 0) return;
                        
            if (dataGrid.Selection != null &&
                dataGrid.Selection.SelectedRows.Count > 0)
            {
                dataGrid.CurrentRow = dataGrid.Rows[index];
            }
            dataGrid.SelectedIndex = index;
            dataGrid.ScrollIntoView(index, 0);
        }

        public static void SelectCell(this C1DataGrid dataGrid, int rowIndex, int colIndex)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            dataGrid.SelectedIndex = rowIndex;
            if (dataGrid.Selection != null &&
                dataGrid.Selection.SelectedRows.Count > 0 &&
                dataGrid.Selection.SelectedCells.Count > 0)
            {
                dataGrid.CurrentRow = dataGrid.Selection.SelectedRows[0];
                dataGrid.CurrentCell = dataGrid.Selection.SelectedCells[0];
            }
            dataGrid.ScrollIntoView(rowIndex, colIndex);
        }

        /// <summary>
        /// 해당 컬럼에서 데이터를 검색하여 있으면 DataRow 를 반환합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">검색 컬럼</param>
        /// <param name="value">찾을 데이터</param>
        /// <returns></returns>
        public static List<DataRow> FindDataRow(this C1DataGrid dataGrid, string sColumnName, object value)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return null;

            DataTable dtGrid = ((DataView)dataGrid.ItemsSource).ToTable();

            List<DataRow> returnValue = dtGrid.AsEnumerable().Where(row => row[sColumnName].Equals(value)).ToList();

            return returnValue;
        }

        /// <summary>
        /// 데이터 존재 유무 체크
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">검색 컬럼</param>
        /// <param name="value">검색 값</param>
        /// <returns></returns>
        public static int ExistsCheck(this C1DataGrid dataGrid, string sColumnName, object value)
        {
            int retCnt = 0;

            if (!dataGrid.Columns.Contains(sColumnName)) return retCnt;
            if (dataGrid.Rows.Count == 0) return retCnt;

            for (int row = 0; row < dataGrid.Rows.Count; row++)
            {
                object checkValue = dataGrid.GetValue(row, sColumnName);
                if (value == null && checkValue == null)
                {
                    retCnt++;
                    continue;
                }

                if (checkValue != null && checkValue.Equals(value))
                {
                    retCnt++;
                }
            }
            return retCnt;
        }

        /// <summary>
        /// 값을 설정합니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="index">Row Index</param>
        /// <param name="columnName">Column Name</param>
        /// <param name="value">Value</param>
        public static void SetValue(this C1DataGrid dataGrid, int index, string columnName, object value)
        {
            try
            {
                if (dataGrid == null || dataGrid.ItemsSource == null) return;
                if (dataGrid.Rows == null || dataGrid.Rows.Count == 0) return;
                if (index < 0 || index >= dataGrid.Rows.Count) return;

                object obj = dataGrid.Rows[index].DataItem;

                if (obj != null)
                {
                    if (obj is DataRowView)
                    {
                        DataRowView drv = obj as DataRowView;
                        if (drv.Row.Table.Columns.Contains(columnName))
                        {
                            drv[columnName] = value == null ? DBNull.Value : value;
                        }
                    }
                    else
                    {
                        System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty(columnName);
                        if (propertyInfo != null)
                        {
                            obj.GetType().InvokeMember(propertyInfo.Name, System.Reflection.BindingFlags.SetProperty, null, obj, new object[] { value });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        
        /// <summary>
        /// 현재 행의 컬럼값을 읽어옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="columnName">Column Name</param>
        /// <returns></returns>
        public static object GetValue(this C1DataGrid dataGrid, string columnName)
        {
            try
            {
                if (dataGrid == null || dataGrid.ItemsSource == null) return null;

                if (dataGrid.CurrentRow == null) return null;

                return GetValue(dataGrid, dataGrid.CurrentRow.Index, columnName);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
            return null;
        }

        /// <summary>
        /// 현재 행의 컬럼값을 String 으로 읽어옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="columnName">Column Name</param>
        /// <returns></returns>
        public static string GetStringValue(this C1DataGrid dataGrid, string columnName)
        {
            object returnValue = GetValue(dataGrid, columnName);
            if (returnValue == null) return string.Empty;

            return returnValue.ToString();
        }

        /// <summary>
        /// 값을 읽어옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="index">Row Index</param>
        /// <param name="columnName">Column Name</param>
        /// <returns></returns>
        public static object GetValue(this C1DataGrid dataGrid, int index, string columnName)
        {
            try
            {
                if (dataGrid == null || dataGrid.ItemsSource == null) return null;

                object obj = dataGrid.Rows[index].DataItem;

                if (obj != null)
                {
                    if (obj is DataRowView)
                    {
                        DataRowView drv = obj as DataRowView;
                        return drv[columnName] == DBNull.Value ? null : drv[columnName];
                    }
                    else
                    {
                        PropertyInfo propertyInfo = obj.GetType().GetProperty(columnName);
                        if (propertyInfo != null)
                        {
                            return obj.GetType().InvokeMember(propertyInfo.Name, BindingFlags.GetProperty, null, obj, new object[] { });
                        }
                    }
                }
                else
                {
                    DataTable dt = dataGrid.GetDataTable();
                    if (dt != null && dt.Columns.Contains(columnName) && dt.Rows.Count > index)
                    {
                        return dt.Rows[index][columnName];
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
            return null;
        }

        /// <summary>
        /// 값을 String 으로 읽어옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="index">Row Index</param>
        /// <param name="columnName">Column Name</param>
        /// <returns></returns>
        public static object GetStringValue(this C1DataGrid dataGrid, int index, string columnName)
        {
            object returnValue = GetValue(dataGrid, index, columnName);
            if (returnValue == null) return string.Empty;

            return returnValue.ToString();
        }

        /// <summary>
        /// 값을 읽어옵니다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="index">Row Index</param>
        /// <param name="columnName">Column Name</param>
        /// <returns></returns>
        public static string GetText(this C1DataGrid dataGrid, int index, string columnName)
        {
            try
            {
                if (dataGrid == null || dataGrid.ItemsSource == null) return null;

                C1.WPF.DataGrid.DataGridCell gridCell = dataGrid.GetCell(index, dataGrid.Columns[columnName].Index);
                if (gridCell != null)
                {
                    if (gridCell.Presenter != null && gridCell.Presenter.Content != null)
                    {
                        if (gridCell.Presenter.Content is C1ComboBox)
                        {
                            C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;
                            if (combo != null) return combo.Text;
                        }
                        else
                        {
                            return gridCell.Text;
                        }
                    }
                    else
                    {
                        return gridCell.Text;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
            return null;
        }

        /// <summary>
        /// 현재 로우의 행번호를 가져온다.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns></returns>
        public static int GetCurrentRowIndex(this C1DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return -1;

            if (dataGrid.CurrentRow == null) return -1;

            return dataGrid.CurrentRow.Index;
        }

        #region Grid Combo Cell 설정

        /// <summary>
        /// 현재 Row 의 Cell Combo 데이터 설정
        /// 컬럼단위 콤보설정이 아닌 셀단위 콤보설정시 DataGridTemplateColumn 으로 선언해야함.
        /// <c1:DataGridTemplateColumn    Header="콤보셀" EditOnSelection="True" Name="컬럼명"  MinWidth="80" CellContentStyle="{StaticResource C1ComboBoxStyle}">
        ///     <c1:DataGridTemplateColumn.CellTemplate>
        ///         <DataTemplate>
        ///             <c1:C1ComboBox SelectedValue = "{Binding 컬럼명, Mode=TwoWay}" MinWidth="40" DisplayMemberPath="CBO_NAME" SelectedValuePath="CBO_CODE" HorizontalAlignment="Stretch" />
        ///         </DataTemplate>
        ///     </c1:DataGridTemplateColumn.CellTemplate>
        /// </c1:DataGridTemplateColumn>
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex">Row Index</param>
        /// <param name="comboColumnName">Combo Column Name</param>
        /// <param name="dtCombo">DataTable</param>
        public static void SetGridCellCombo(this C1DataGrid dataGrid, string comboColumnName, DataTable dtCombo)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            if (dataGrid.CurrentRow == null) return;

            SetGridCellCombo(dataGrid, dataGrid.CurrentRow.Index, comboColumnName, dtCombo, false);
        }

        /// <summary>
        /// Cell Combo 데이터 설정
        /// 컬럼단위 콤보설정이 아닌 셀단위 콤보설정시 DataGridTemplateColumn 으로 선언해야함.
        /// <c1:DataGridTemplateColumn    Header="콤보셀" EditOnSelection="True" Name="컬럼명"  MinWidth="80" CellContentStyle="{StaticResource C1ComboBoxStyle}">
        ///     <c1:DataGridTemplateColumn.CellTemplate>
        ///         <DataTemplate>
        ///             <c1:C1ComboBox SelectedValue = "{Binding 컬럼명, Mode=TwoWay}" MinWidth="40" DisplayMemberPath="CBO_NAME" SelectedValuePath="CBO_CODE" HorizontalAlignment="Stretch" />
        ///         </DataTemplate>
        ///     </c1:DataGridTemplateColumn.CellTemplate>
        /// </c1:DataGridTemplateColumn>
        /// </summary>
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex">Row Index</param>
        /// <param name="comboColumnName">Combo Column Name</param>
        /// <param name="dtCombo">DataTable</param>
        public static void SetGridCellCombo(this C1DataGrid dataGrid, int rowIndex, string comboColumnName, DataTable dtCombo)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            SetGridCellCombo(dataGrid, rowIndex, comboColumnName, dtCombo, false);
        }

        /// <summary>
        /// Cell Combo 데이터 설정
        /// 컬럼단위 콤보설정이 아닌 셀단위 콤보설정시 DataGridTemplateColumn 으로 선언해야함.
        /// <c1:DataGridTemplateColumn    Header="콤보셀" EditOnSelection="True" Name="컬럼명"  MinWidth="80" CellContentStyle="{StaticResource C1ComboBoxStyle}">
        ///     <c1:DataGridTemplateColumn.CellTemplate>
        ///         <DataTemplate>
        ///             <c1:C1ComboBox SelectedValue = "{Binding 컬럼명, Mode=TwoWay}" MinWidth="40" DisplayMemberPath="CBO_NAME" SelectedValuePath="CBO_CODE" HorizontalAlignment="Stretch" />
        ///         </DataTemplate>
        ///     </c1:DataGridTemplateColumn.CellTemplate>
        /// </c1:DataGridTemplateColumn>
        /// </summary>
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex">Row Index</param>
        /// <param name="comboColumnName">Combo Column Name</param>
        /// <param name="dtCombo">DataTable</param>
        /// <param name="isSelectFirst">첫번째 선택 여부</param>
        public static void SetGridCellCombo(this C1DataGrid dataGrid, int rowIndex, string comboColumnName, DataTable dtCombo, bool isInBlank = true, bool isInCode = true)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            C1.WPF.DataGrid.DataGridCell gridCell = dataGrid.GetCell(rowIndex, dataGrid.Columns[comboColumnName].Index);
            if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
            {
                if (gridCell.Presenter.Content is C1ComboBox == false)
                {
                    gridCell.Presenter.Content = new C1ComboBox();
                }
                
                C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;
                
                if (combo != null)
                {
                    
                    if (dtCombo.Columns.Count == 1)
                    {
                        combo.DisplayMemberPath = dtCombo.Columns[0].ColumnName;
                        combo.SelectedValuePath = dtCombo.Columns[0].ColumnName;
                    }
                    else if (dtCombo.Columns.Count > 1)
                    {
                        combo.DisplayMemberPath = dtCombo.Columns[1].ColumnName;
                        combo.SelectedValuePath = dtCombo.Columns[0].ColumnName;
                    }
                    else
                    {
                        return;
                    }

                    DataTable dt = dataGrid.GetDataTable();
                    object val = dt.Rows[rowIndex][comboColumnName];
                    if (val != null || val.ToString() != string.Empty)
                    {
                        combo.SelectedValue = val;
                        combo.SetDataComboItem(dtCombo, isInBlank ? CommonCombo.ComboStatus.EMPTY : CommonCombo.ComboStatus.NONE, isInCode, ComboBoxExtension.InCodeType.Bracket, val.ToString());
                    }
                    else
                    {
                        combo.SetDataComboItem(dtCombo, isInBlank ? CommonCombo.ComboStatus.EMPTY : CommonCombo.ComboStatus.NONE, isInCode);
                    }
                }
            }
        }

        /// <summary>
        /// Cell Combo 데이터 설정
        /// 컬럼단위 콤보설정이 아닌 셀단위 콤보설정시 DataGridTemplateColumn 으로 선언해야함.
        /// <c1:DataGridTemplateColumn    Header="콤보셀" EditOnSelection="True" Name="컬럼명"  MinWidth="80" CellContentStyle="{StaticResource C1ComboBoxStyle}">
        ///     <c1:DataGridTemplateColumn.CellTemplate>
        ///         <DataTemplate>
        ///             <c1:C1ComboBox SelectedValue = "{Binding 컬럼명, Mode=TwoWay}" MinWidth="40" DisplayMemberPath="CBO_NAME" SelectedValuePath="CBO_CODE" HorizontalAlignment="Stretch" />
        ///         </DataTemplate>
        ///     </c1:DataGridTemplateColumn.CellTemplate>
        /// </c1:DataGridTemplateColumn>
        /// </summary>
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex">로우</param>
        /// <param name="comboColumnName">Combo Column Name</param>
        /// <param name="commonCode">공통코드</param>
        /// <param name="filter">필터</param>
        public static void SetGridCellCommonCombo(this C1DataGrid dataGrid, int rowIndex, string comboColumnName, string commonCode, string filter = "", bool isInBlank = true, bool isInCode = true)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            C1.WPF.DataGrid.DataGridCell gridCell = dataGrid.GetCell(rowIndex, dataGrid.Columns[comboColumnName].Index);
            if (gridCell != null && gridCell.Presenter != null && gridCell.Presenter.Content != null)
            {
                if (gridCell.Presenter.Content is C1ComboBox == false)
                {
                    gridCell.Presenter.Content = new C1ComboBox();
                }

                C1ComboBox combo = gridCell.Presenter.Content as C1ComboBox;
                if (combo != null)
                {
                    object val = dataGrid.GetValue(rowIndex, comboColumnName);
                    combo.SetCommonCode(commonCode, filter, isInBlank ? CommonCombo.ComboStatus.EMPTY : CommonCombo.ComboStatus.NONE, isInCode);
                    if (val != null) combo.SelectedValue = val;
                }
            }
        }

        /// <summary>
        /// Column Combo 데이터 설정
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex">Row Index</param>
        /// <param name="comboColumnName">Combo Column Name</param>
        /// <param name="dtCombo">DataTable</param>
        /// <param name="isSelectFirst">첫번째 선택 여부</param>
        public static void SetGridColumnCombo(this C1DataGrid dataGrid, string comboColumnName, DataTable dtCombo, string filter = "", bool isInBlank = true, bool isInCode = true)
        {
            try
            {
                if (dataGrid == null || dataGrid.ItemsSource == null) return;

                C1.WPF.DataGrid.DataGridColumn col = dataGrid.Columns[comboColumnName];
                C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;

                if (!filter.Equals(string.Empty))
                {
                    DataView dvResult = dtCombo.DefaultView;
                    dvResult.RowFilter = filter;
                    dtCombo = dvResult.ToTable();
                }

                if (dtCombo != null && dtCombo.Columns.Count >= 2)
                {
                    if (cboColumn.SelectedValuePath == null)
                    {
                        cboColumn.SelectedValuePath = dtCombo.Columns[0].ColumnName;
                    }

                    if (cboColumn.DisplayMemberPath == null)
                    {
                        cboColumn.DisplayMemberPath = dtCombo.Columns[1].ColumnName;
                    }
                }
                if (isInCode && dtCombo != null)
                {
                    dtCombo.AsEnumerable().ToList<DataRow>()
                        .ForEach(x => x[cboColumn.DisplayMemberPath] = "[" + LGC.GMES.MES.CMM001.Class.Util.NVC(x[cboColumn.SelectedValuePath]) + "] " + LGC.GMES.MES.CMM001.Class.Util.NVC(x[cboColumn.DisplayMemberPath]));
                }

                if (isInBlank && dtCombo != null)
                {
                    DataRow newRow = dtCombo.NewRow();
                    newRow[cboColumn.SelectedValuePath] = null;
                    newRow[cboColumn.DisplayMemberPath] = string.Empty;
                    dtCombo.Rows.InsertAt(newRow, 0);
                }

                cboColumn.ItemsSource = dtCombo.Copy().AsDataView();

                dtCombo.Dispose();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Column Combo 데이터 설정
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="comboColumnName">Combo Column Name</param>
        /// <param name="commonCode">공통코드</param>
        /// <param name="filter">필터</param>
        /// <param name="isInBlank">첫번째공백추가</param>
        /// <param name="isInCode">코드포함</param>
        public static void SetGridColumnCommonCombo(this C1DataGrid dataGrid, string comboColumnName, string commonCode, string filter = "", bool isInBlank = true, bool isInCode = true)
        {
            try
            {
                if (dataGrid == null || dataGrid.ItemsSource == null) return;

                C1.WPF.DataGrid.DataGridColumn col = dataGrid.Columns[comboColumnName];
                C1.WPF.DataGrid.DataGridComboBoxColumn cboColumn = col as C1.WPF.DataGrid.DataGridComboBoxColumn;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = commonCode;
                RQSTDT.Rows.Add(dr);

                string bizRuleName = "DA_BAS_SEL_COMMCODE_ALL_CBO";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);

                if (!filter.Equals(string.Empty))
                {
                    DataView dvResult = dtResult.DefaultView;
                    dvResult.RowFilter = filter;
                    dtResult = dvResult.ToTable();
                }

                if (dtResult != null && dtResult.Columns.Count >= 2)
                {
                    if (cboColumn.SelectedValuePath == null)
                    {
                        cboColumn.SelectedValuePath = dtResult.Columns[0].ColumnName;
                    }

                    if (cboColumn.DisplayMemberPath == null)
                    {
                        cboColumn.DisplayMemberPath = dtResult.Columns[1].ColumnName;
                    }
                }

                if (isInCode && dtResult != null)
                {
                    dtResult.AsEnumerable().ToList<DataRow>()
                        .ForEach(x => x[cboColumn.DisplayMemberPath] = "[" + LGC.GMES.MES.CMM001.Class.Util.NVC(x[cboColumn.SelectedValuePath]) + "] " + LGC.GMES.MES.CMM001.Class.Util.NVC(x[cboColumn.DisplayMemberPath]));
                }

                if (isInBlank && dtResult != null)
                {
                    DataRow newRow = dtResult.NewRow();
                    newRow[cboColumn.SelectedValuePath] = null;
                    newRow[cboColumn.DisplayMemberPath] = string.Empty;
                    dtResult.Rows.InsertAt(newRow, 0);
                }

                cboColumn.ItemsSource = dtResult.Copy().AsDataView();

                dtResult.Dispose();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Validation 설정

        /// <summary>
        /// Validation 설정
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex">로우</param>
        /// <param name="columnName">컬럼</param>
        public static void SetValidation(this C1DataGrid dataGrid, int rowIndex, string columnName)
        {
            try
            {
                if (dataGrid == null || dataGrid.ItemsSource == null) return;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
            }

        }

        #endregion

        #region Header Check All 추가
        private static C1.WPF.DataGrid.DataGridRowHeaderPresenter chkAllPresenter = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent),
            MouseOverBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent),
        };

        private static System.Windows.Controls.CheckBox chkDataGridAll = new System.Windows.Controls.CheckBox()
        {
            IsChecked = false,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public static void AddCheckAll(this C1DataGrid dataGrid)
        {
            if (!dataGrid.Columns.Contains("CHK"))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show("[CHK] Column not exists!", "", "Error");
                return;
            }

            dataGrid.Columns["CHK"].Header = "";

            dataGrid.LoadedColumnHeaderPresenter += DataGrid_LoadedColumnHeaderPresenter;
            chkDataGridAll.Checked += ChkDataGridAll_Checked;
            chkDataGridAll.Unchecked += ChkDataGridAll_Unchecked;
        }

        private static void DataGrid_LoadedColumnHeaderPresenter(object sender, DataGridColumnEventArgs e)
        {
            try
            {
                if (e.Column.Name.Equals("CHK"))
                {
                    chkAllPresenter.Content = chkDataGridAll;
                    e.Column.HeaderPresenter.Content = chkAllPresenter;
                    e.Column.HeaderPresenter.Margin = new System.Windows.Thickness(6, 0, 0, 0);
                }
            }
            catch { }
        }

        private static void ChkDataGridAll_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.CheckBox checkbox = (System.Windows.Controls.CheckBox)sender;

                C1DataGrid dataGrid = null;

                foreach (var item in checkbox.GetAllParents())
                {
                    if (item is C1DataGrid)
                    {
                        dataGrid = (C1DataGrid)item;
                        break;
                    }
                }
                if (dataGrid == null) return;

                for (int idx = 0; idx < dataGrid.Rows.Count; idx++)
                {
                    C1.WPF.DataGrid.DataGridRow row = dataGrid.Rows[idx];
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
                dataGrid.Refresh();
            }
            catch { }
        }

        private static void ChkDataGridAll_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Windows.Controls.CheckBox checkbox = (System.Windows.Controls.CheckBox)sender;

                C1DataGrid dataGrid = null;

                foreach (var item in checkbox.GetAllParents())
                {
                    if (item is C1DataGrid)
                    {
                        dataGrid = (C1DataGrid)item;
                        break;
                    }
                }
                if (dataGrid == null) return;

                for (int idx = 0; idx < dataGrid.Rows.Count; idx++)
                {
                    C1.WPF.DataGrid.DataGridRow row = dataGrid.Rows[idx];
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
                dataGrid.Refresh();
            }
            catch { }
        }
        #endregion
    }
}