/*************************************************************************************
 Created Date : 2022.08.29
      Creator : 조영대
   Decription : Column Visible Popup
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.29  조영대 : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using System.Configuration;
using System.IO;
using System.Drawing;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_COLUMN_VISIBLE : C1Window
    {
        #region Declaration & Constructor
        private UcBaseDataGrid dgGrid = null;
        List<UcBaseDataGrid.UserConfigInformation> restoreConfigInfos = null;

        public bool IsRowCountView { get; set; }
        #endregion

        #region Initialize 

        public CMM_COLUMN_VISIBLE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {                
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    dgGrid = tmps[0] as UcBaseDataGrid;
                    List<UcBaseDataGrid.UserConfigInformation> originalConfigInfos = tmps[1] as List<UcBaseDataGrid.UserConfigInformation>;
                    chkRowCountView.IsChecked = tmps[2].Equals(true);

                    if (restoreConfigInfos == null || restoreConfigInfos.Count == 0)
                    {
                        restoreConfigInfos = new List<UcBaseDataGrid.UserConfigInformation>();
                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgGrid.Columns)
                        {
                            restoreConfigInfos.Add(new UcBaseDataGrid.UserConfigInformation(dgc));
                        }
                    }

                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("INDICATOR", typeof(string));
                    dtData.Columns.Add("COLUMN_NAME0", typeof(string));
                    dtData.Columns.Add("COLUMN_NAME1", typeof(string));
                    dtData.Columns.Add("COLUMN_NAME2", typeof(string));
                    dtData.Columns.Add("COLUMN_NAME3", typeof(string));
                    dtData.Columns.Add("COLUMN_NAME4", typeof(string));
                    dtData.Columns.Add("CHK", typeof(bool));
                    dtData.Columns.Add("COLUMN", typeof(string));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgGrid.Columns.OrderBy(x => x.DisplayIndex))
                    {
                        if (dgc.Name == null) continue;
                        if (originalConfigInfos.Find(f => f.ColumnName.Equals(dgc.Name)) == null) continue;
                        if (!dgGrid.UserConfigExceptColumns.Contains(dgc.Name) &&
                            originalConfigInfos.Find(f => f.ColumnName.Equals(dgc.Name)).Visibility != Visibility.Visible) continue;

                        DataRow drNew = dtData.NewRow();
                        drNew["CHK"] = dgc.Visibility.Equals(Visibility.Visible) ? true : false;

                        if (dgc.Header is List<string>)
                        {
                            List<string> headers = dgc.Header as List<string>;
                            for (int inx = 0; inx < headers.Count; inx++)
                            {
                                drNew["COLUMN_NAME" + inx.ToString()] = headers[inx];

                                if (dgDataGrid.Columns["COLUMN_NAME" + inx.ToString()].Visibility != Visibility.Visible)
                                {
                                    dgDataGrid.Columns["COLUMN_NAME" + inx.ToString()].Visibility = Visibility.Visible;
                                }
                            }
                        }
                        else if (dgc.Header is string)
                        {
                            if (dgGrid.TopRows.Count > 0)
                            {
                                for (int row = 0; row < dgGrid.TopRows.Count; row++)
                                {
                                    drNew["COLUMN_NAME" + Util.NVC(row)] = dgc.Header.ToString();
                                }
                            }
                            else
                            {
                                drNew["COLUMN_NAME0"] = dgc.Header.ToString();
                            }                            
                        }
                        else
                        {
                            drNew["COLUMN_NAME0"] = "...";
                        }
                        drNew["COLUMN"] = dgc.Name;
                        dtData.Rows.Add(drNew);
                    }

                    dgDataGrid.SetItemsSource(dtData, null, true);

                    MergeRefresh();
                }
                dgDataGrid.SelectRow(0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void dgDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                double width2 = 0;
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgDataGrid.Columns)
                {
                    if (dgc.Visibility != Visibility.Visible) continue;

                    width2 += dgc.ActualWidth;
                }

                this.Width = width2 + 150;
            }));
        }

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != MessageBoxResult.OK)
            {
                if (restoreConfigInfos != null && restoreConfigInfos.Count > 0)
                {
                    foreach (UcBaseDataGrid.UserConfigInformation userConfig in restoreConfigInfos)
                    {
                        if (dgGrid.Columns.Contains(userConfig.ColumnName))
                        {
                            dgGrid.Columns[userConfig.ColumnName].Visibility = userConfig.Visibility;
                            dgGrid.Columns[userConfig.ColumnName].Width = new C1.WPF.DataGrid.DataGridLength(userConfig.Width, userConfig.UnitType);
                            dgGrid.Columns[userConfig.ColumnName].DisplayIndex = userConfig.DisplayIndex;
                        }
                    }
                }
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtData = dgDataGrid.GetDataTable();

                if (dtData.AsEnumerable().Where(r => r["CHK"].Equals(true)).Count() == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                int displayIndex = 0;
                foreach (DataRow dr in dtData.Rows)
                {
                    if (Convert.ToBoolean(dr["CHK"]))
                    {
                        dgGrid.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dgGrid.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                    }

                    dgGrid.Columns[dr["COLUMN"].ToString()].DisplayIndex = displayIndex;

                    displayIndex++;
                }

                IsRowCountView = chkRowCountView.IsChecked.Equals(true);

                dgGrid.Refresh();

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgDataGrid_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            try
            {
                double width = 0;
                foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgDataGrid.Columns)
                {
                    if (dgc.Visibility != Visibility.Visible) continue;

                    width += dgc.ActualWidth;
                }

                this.Width = width + 150;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDataGrid_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dg = sender as C1DataGrid;

                if (e.Cell.Column.Index < 6)
                {
                    e.Cell.Presenter.Style = Application.Current.Resources["UcBaseDataGridCellHeaderPresenterStyle"] as Style;
                    e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(60);
                    e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Center;
                }

                if (e.Cell.Column.Index == 0)
                {
                    if (dg.Selection != null && dg.Selection.SelectedRows.Where(x => x.Index == e.Cell.Row.Index).Count() > 0)
                    {
                        dgDataGrid.SetValue(e.Cell.Row.Index, "INDICATOR", "▶");
                        dgDataGrid.EndEditRow(true);
                    }
                    else
                    {
                        dgDataGrid.SetValue(e.Cell.Row.Index, "INDICATOR", string.Empty);
                    }
                }

                string columnName = Util.NVC(dgDataGrid.GetValue(e.Cell.Row.Index, "COLUMN"));
                if (dgGrid.UserConfigExceptColumns.Contains(columnName))
                {
                    e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                }

                if (e.Cell.Column.Name.Equals("CHK"))
                {
                    if (dgGrid.UserConfigExceptColumns.Contains(columnName))
                    {
                        e.Cell.Presenter.Visibility = Visibility.Collapsed;
                        //if (e.Cell.Presenter.Content is System.Windows.Controls.CheckBox)
                        //{ 
                        //    System.Windows.Controls.CheckBox chkBox = e.Cell.Presenter.Content as System.Windows.Controls.CheckBox;
                        //    chkBox.Visibility = Visibility.Collapsed;
                        
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDataGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgDataGrid.CurrentColumn.Name.Equals("CHK") && dgDataGrid.CurrentRow != null && dgDataGrid.CurrentColumn != null)
            {
                object value = dgDataGrid.GetValue(dgDataGrid.CurrentRow.Index, dgDataGrid.CurrentColumn.Name);
                if (value.Equals(true))
                {
                    dgDataGrid.SetValue(dgDataGrid.CurrentRow.Index, dgDataGrid.CurrentColumn.Name, false);
                }
                else
                {
                    dgDataGrid.SetValue(dgDataGrid.CurrentRow.Index, dgDataGrid.CurrentColumn.Name, true);
                }
            }
            dgDataGrid.Refresh();
        }

        private void dgDataGrid_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (e.Column.Name.Equals("CHK"))
            {
                string columnName = Util.NVC(dgDataGrid.GetValue(e.Row.Index, "COLUMN"));
                if (dgGrid.UserConfigExceptColumns.Contains(columnName))
                {
                    e.Cancel = false;
                    return;
                }
            }
        }

        private void dgDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("CHK"))
            {
                string columnName = Util.NVC(dgDataGrid.GetValue(e.Row.Index, "COLUMN"));
                if (dgGrid.UserConfigExceptColumns.Contains(columnName))
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnUp.IsEnabled = false;

                if (dgDataGrid.Selection == null || dgDataGrid.Selection.SelectedRows.Count == 0) return;

                dgDataGrid.ClearMergeCell();

                List<int> selectionRows = dgDataGrid.Selection.SelectedRows.Select(x => x.Index).ToList();

                foreach (int rowIndex in selectionRows)
                {
                    string columnName = Util.NVC(dgDataGrid.GetValue(rowIndex, "COLUMN"));
                    if (dgGrid.UserConfigExceptColumns.Contains(columnName))
                    {
                        //Util.MessageValidation("이동불가");
                        MergeRefresh();
                        return;
                    }
                }

                for (int inx = 0; inx < selectionRows.Count; inx++)
                {
                    int moveRow = selectionRows[inx] - 1;
                    if (moveRow < 0) continue;
                    if (selectionRows.Contains(moveRow)) continue;

                    //C1.WPF.DataGrid.DataGridColumn dc = dgGrid.Columns[Util.NVC(dgDataGrid.GetValue(selectionRows[inx], "COLUMN"))];
                    //dgGrid.MoveColumn(dc, moveRow);

                    DataRowView dv = dgDataGrid.Rows[selectionRows[inx]].DataItem as DataRowView;
                    DataTable dt = dv.Row.Table;

                    DataRow newRow = dt.NewRow();
                    newRow.ItemArray = dv.Row.ItemArray;
                    dt.Rows.InsertAt(newRow, moveRow);
                    dt.Rows.Remove(dv.Row);

                    selectionRows[inx] = moveRow;
                }

                MergeRefresh();

                dgDataGrid.Selection.Clear();
                for (int inx = 0; inx < selectionRows.Count; inx++)
                {
                    DataGridCellsRange range = new DataGridCellsRange(dgDataGrid.Rows[selectionRows[inx]]);
                    dgDataGrid.Selection.Add(range);
                }

                if (dgDataGrid.Selection != null && dgDataGrid.Selection.SelectedRows.Count > 0)
                {
                    int minRow = dgDataGrid.Selection.SelectedRows.Min(x => x.Index);
                    double offset = (minRow - 5) * 24;
                    if (offset < 0) offset = 0;
                    dgDataGrid.Viewport.ScrollToVerticalOffset(offset);

                }

                dgDataGrid.Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnUp.IsEnabled = true;
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDown.IsEnabled = false;

                if (dgDataGrid.Selection == null || dgDataGrid.Selection.SelectedRows.Count == 0) return;

                dgDataGrid.ClearMergeCell();

                List<int> selectionRows = dgDataGrid.Selection.SelectedRows.Select(x => x.Index).ToList();

                foreach (int rowIndex in selectionRows)
                {
                    string columnName = Util.NVC(dgDataGrid.GetValue(rowIndex, "COLUMN"));
                    if (dgGrid.UserConfigExceptColumns.Contains(columnName))
                    {
                        //Util.MessageValidation("이동불가");
                        MergeRefresh();
                        return;
                    }
                }

                for (int inx = selectionRows.Count - 1; inx >= 0; inx--)
                {
                    int moveRow = selectionRows[inx] + 1;
                    if (moveRow > dgGrid.Columns.Max(x => x.DisplayIndex)) continue;
                    if (moveRow >= dgDataGrid.Rows.Count) continue;
                    if (selectionRows.Contains(moveRow)) continue;

                    //C1.WPF.DataGrid.DataGridColumn dc = dgGrid.Columns[Util.NVC(dgDataGrid.GetValue(selectionRows[inx], "COLUMN"))];
                    //dgGrid.MoveColumn(dc, moveRow);

                    DataRowView dv = dgDataGrid.Rows[selectionRows[inx]].DataItem as DataRowView;
                    DataTable dt = dv.Row.Table;

                    DataRow newRow = dt.NewRow();
                    newRow.ItemArray = dv.Row.ItemArray;
                    dt.Rows.Remove(dv.Row);
                    dt.Rows.InsertAt(newRow, moveRow);

                    selectionRows[inx] = moveRow;
                }

                MergeRefresh();

                dgDataGrid.Selection.Clear();
                for (int inx = 0; inx < selectionRows.Count; inx++)
                {
                    DataGridCellsRange range = new DataGridCellsRange(dgDataGrid.Rows[selectionRows[inx]]);
                    dgDataGrid.Selection.Add(range);
                }

                if (dgDataGrid.Selection != null && dgDataGrid.Selection.SelectedRows.Count > 0)
                {
                    int maxRow = dgDataGrid.Selection.SelectedRows.Max(x => x.Index);
                    double offset = (maxRow - 5) * 24;
                    if (offset < 0) offset = 0;
                    dgDataGrid.Viewport.ScrollToVerticalOffset(offset);
                }

                dgDataGrid.Refresh();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                btnDown.IsEnabled = true;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0493", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                        dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                        dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                        dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["WRK_TYPE"] = "SELECT";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["CONF_TYPE"] = "USER_CONFIG_DATAGRID";
                        dtRqst.Rows.Add(dr);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                        if (dtResult != null && dtResult.Rows.Count > 0)
                        {
                            Microsoft.Win32.SaveFileDialog od = new Microsoft.Win32.SaveFileDialog();

                            if (System.Configuration.ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                            {
                                od.InitialDirectory = @"\\Client\C$";
                            }
                            od.Filter = ObjectDic.Instance.GetObjectName("USER_CONFIG") + " (.ucnf)|*.ucnf";
                            od.FileName = ObjectDic.Instance.GetObjectName("USER_CONFIG") + "(" + LoginInfo.USERID + ").ucnf";

                            if (od.ShowDialog() == true)
                            {
                                C1XLBook c1XLBook1 = new C1XLBook();
                                XLSheet sheet = c1XLBook1.Sheets[0];
                                XLStyle styel = new XLStyle(c1XLBook1);

                                int rowCount = 0;
                                sheet[rowCount++, 0].Value = "Ver 0.1";
                                foreach (DataRow drConf in dtResult.Rows)
                                {
                                    if (!drConf["CONF_KEY1"].ToString().Substring(0, 4).Equals("LGC.")) continue;

                                    sheet[rowCount, 0].Value = drConf["USERID"];
                                    sheet[rowCount, 1].Value = drConf["CONF_TYPE"];
                                    sheet[rowCount, 2].Value = drConf["CONF_KEY1"];
                                    sheet[rowCount, 3].Value = drConf["CONF_KEY2"];
                                    sheet[rowCount, 4].Value = drConf["CONF_KEY3"];
                                    sheet[rowCount, 5].Value = drConf["USER_CONF01"];
                                    sheet[rowCount, 6].Value = drConf["USER_CONF02"];
                                    sheet[rowCount, 7].Value = drConf["USER_CONF03"];
                                    sheet[rowCount, 8].Value = drConf["USER_CONF04"];
                                    sheet[rowCount, 9].Value = drConf["USER_CONF05"];
                                    sheet[rowCount, 10].Value = drConf["USER_CONF06"];
                                    sheet[rowCount, 11].Value = drConf["USER_CONF07"];
                                    sheet[rowCount, 12].Value = drConf["USER_CONF08"];
                                    sheet[rowCount, 13].Value = drConf["USER_CONF09"];
                                    sheet[rowCount, 14].Value = drConf["USER_CONF10"];

                                    rowCount++;
                                }

                                c1XLBook1.Save(od.FileName);

                                Util.MessageValidation("SFU3532");
                            }
                        }
                    }
                });


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0494", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                        if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                        {
                            fd.InitialDirectory = @"\\Client\C$";
                        }

                        fd.Filter = ObjectDic.Instance.GetObjectName("USER_CONFIG") + " (.ucnf)|*.ucnf";

                        if (fd.ShowDialog() == true)
                        {
                            using (Stream stream = fd.OpenFile())
                            {
                                C1XLBook book = new C1XLBook();
                                book.Load(stream, FileFormat.Biff8);
                                XLSheet sheet = book.Sheets[0];

                                if (!Util.NVC(sheet.GetCell(0, 0).Text).Equals("Ver 0.1"))
                                {
                                    //입력 형식이 맞지 않습니다.
                                    Util.MessageValidation("SFU3465");
                                    return;
                                }

                                Window mainWindow = this.FindTopParent<Window>();

                                this.DialogResult = MessageBoxResult.Cancel;

                                DataTable dtRqst = new DataTable();
                                dtRqst.TableName = "INDATA";
                                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                                dtRqst.Columns.Add("USERID", typeof(string));
                                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                                dtRqst.Columns.Add("CONF_KEY3", typeof(string));
                                dtRqst.Columns.Add("USER_CONF01", typeof(string));
                                dtRqst.Columns.Add("USER_CONF02", typeof(string));
                                dtRqst.Columns.Add("USER_CONF03", typeof(string));
                                dtRqst.Columns.Add("USER_CONF04", typeof(string));
                                dtRqst.Columns.Add("USER_CONF05", typeof(string));
                                dtRqst.Columns.Add("USER_CONF06", typeof(string));
                                dtRqst.Columns.Add("USER_CONF07", typeof(string));
                                dtRqst.Columns.Add("USER_CONF08", typeof(string));
                                dtRqst.Columns.Add("USER_CONF09", typeof(string));
                                dtRqst.Columns.Add("USER_CONF10", typeof(string));

                                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                                {
                                    if (sheet.GetCell(rowInx, 0) == null) break;

                                    DataRow drNew = dtRqst.NewRow();
                                    drNew["WRK_TYPE"] = "SAVE";
                                    drNew["USERID"] = LoginInfo.USERID;
                                    drNew["CONF_TYPE"] = "USER_CONFIG_DATAGRID";
                                    drNew["CONF_KEY1"] = Util.NVC(sheet.GetCell(rowInx, 2)?.Value);
                                    drNew["CONF_KEY2"] = Util.NVC(sheet.GetCell(rowInx, 3)?.Value);
                                    drNew["CONF_KEY3"] = Util.NVC(sheet.GetCell(rowInx, 4)?.Value);
                                    drNew["USER_CONF01"] = Util.NVC(sheet.GetCell(rowInx, 5)?.Value);
                                    drNew["USER_CONF02"] = Util.NVC(sheet.GetCell(rowInx, 6)?.Value);
                                    drNew["USER_CONF03"] = Util.NVC(sheet.GetCell(rowInx, 7)?.Value);
                                    drNew["USER_CONF04"] = Util.NVC(sheet.GetCell(rowInx, 8)?.Value);
                                    drNew["USER_CONF05"] = Util.NVC(sheet.GetCell(rowInx, 9)?.Value);
                                    drNew["USER_CONF06"] = Util.NVC(sheet.GetCell(rowInx, 10)?.Value);
                                    drNew["USER_CONF07"] = Util.NVC(sheet.GetCell(rowInx, 11)?.Value);
                                    drNew["USER_CONF08"] = Util.NVC(sheet.GetCell(rowInx, 12)?.Value);
                                    drNew["USER_CONF09"] = Util.NVC(sheet.GetCell(rowInx, 13)?.Value);
                                    drNew["USER_CONF10"] = Util.NVC(sheet.GetCell(rowInx, 14)?.Value);
                                    dtRqst.Rows.Add(drNew);
                                }

                                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                                if (dtResult != null)
                                {
                                    Util.MessageValidation("FM_ME_0495", result2 =>
                                    {
                                        if (mainWindow != null)
                                        {
                                            C1TabControl tabMain = mainWindow.FindChild<C1TabControl>("tcMainContentTabControl");
                                            if (tabMain != null) tabMain.Items.Clear();
                                        }
                                    });
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        private void MergeRefresh()
        {
            try
            {
                C1.WPF.DataGrid.DataGridCell startCell = null;
                C1.WPF.DataGrid.DataGridCell endCell = null;
                for (int row = 0; row < dgDataGrid.Rows.Count; row++)
                {
                    startCell = endCell = null;
                    for (int col = 1; col < dgDataGrid.Columns.Count - 1; col++)
                    {
                        if (dgDataGrid.Columns[col].Visibility != Visibility.Visible) continue;
                        if (dgDataGrid.Columns[col].Name.Equals("CHK")) continue;

                        if (Util.NVC(dgDataGrid.GetValue(row, dgDataGrid.Columns[col].Name)).Equals(Util.NVC(dgDataGrid.GetValue(row, dgDataGrid.Columns[col + 1].Name))))
                        {
                            if (startCell == null) startCell = dgDataGrid.GetCell(row, col);
                        }
                        else
                        {
                            if (startCell != null)
                            {
                                endCell = dgDataGrid.GetCell(row, col);
                                dgDataGrid.MergeCells(startCell, endCell);
                                startCell = endCell = null;
                            }
                        }
                    }

                    if (startCell != null)
                    {
                        int lastVisibleColumn = dgDataGrid.Columns.GetVisibleColumnsFromIndex(dgDataGrid.Columns["CHK"].Index, System.Windows.Controls.Primitives.GeneratorDirection.Forward).FirstOrDefault().Index;
                        endCell = dgDataGrid.GetCell(row, lastVisibleColumn);
                        dgDataGrid.MergeCells(startCell, endCell);
                        startCell = endCell = null;
                    }
                }

                startCell = endCell = null;
                for (int col = 1; col < dgDataGrid.Columns.Count; col++)
                {
                    if (dgDataGrid.Columns[col].Visibility != Visibility.Visible) continue;
                    if (dgDataGrid.Columns[col].Name.Equals("CHK")) continue;

                    startCell = endCell = null;
                    for (int row = 0; row < dgDataGrid.Rows.Count - 1; row++)
                    {
                        string compareBefore = Util.NVC(dgDataGrid.GetValue(row, dgDataGrid.Columns[col].Name));
                        string compareAfter = Util.NVC(dgDataGrid.GetValue(row + 1, dgDataGrid.Columns[col].Name));
                        if (col > 1)
                        {
                            compareBefore = Util.NVC(dgDataGrid.GetValue(row, dgDataGrid.Columns[col - 1].Name)) + Util.NVC(dgDataGrid.GetValue(row, dgDataGrid.Columns[col].Name));
                            compareAfter = Util.NVC(dgDataGrid.GetValue(row + 1, dgDataGrid.Columns[col - 1].Name)) + Util.NVC(dgDataGrid.GetValue(row + 1, dgDataGrid.Columns[col].Name));
                        }

                        if (compareBefore.Equals(compareAfter))
                        {
                            if (startCell == null) startCell = dgDataGrid.GetCell(row, col);
                        }
                        else
                        {
                            if (startCell != null)
                            {
                                endCell = dgDataGrid.GetCell(row, col);
                                dgDataGrid.MergeCells(startCell, endCell);
                                startCell = endCell = null;
                            }
                        }
                    }

                    if (startCell != null)
                    {
                        endCell = dgDataGrid.GetCell(dgDataGrid.Rows.Count - 1, col);
                        dgDataGrid.MergeCells(startCell, endCell);
                        startCell = endCell = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
