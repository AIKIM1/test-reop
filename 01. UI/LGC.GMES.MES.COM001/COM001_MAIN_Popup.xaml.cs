/*************************************************************************************
 Created Date : 2024.05.14
      Creator : kor21cman
   Decription : 배포 상세내용
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.14  조범모 : E20231011-000895 GMES Main 화면 배포이력 표기

**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using C1.WPF.DataGrid;
using System.Windows.Media;
using System.Reflection;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_MAIN_Popup : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataView _dvCMCD { get; set; }
        string _RELS_REQ_ID = string.Empty;
        object[] parameter = null;
        bool isMultiHeaderForSystem = true;

        public COM001_MAIN_Popup()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }


        #endregion

        #region Event
        private void UserControl_Loaded(object sender, EventArgs e)
        {
            parameter = C1WindowExtension.GetParameters(this);

            _dvCMCD = parameter[0] as DataView;
            _dvCMCD.RowFilter = "CMCDTYPE = 'RELS_FLAG' AND USE_FLAG = 'Y' ";
            CommonCombo.SetDtCommonCombo(_dvCMCD.ToTable(), this.cboRELS_FLAG, CommonCombo.ComboStatus.SELECT, null, null);

            _RELS_REQ_ID = parameter[1] as string;

            Get_DA_BAS_SEL_TB_SFC_GMES_RELS_REQ(_RELS_REQ_ID, (dt, ex) =>
            {
                if (ex != null)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }

                this.txtCSR_NO.Text        = dt.Rows[0]["CSR_NO"].ToString();
                this.cboRELS_FLAG.SelectedValue     = dt.Rows[0]["RELS_FLAG"].ToString();
                this.txtCSR_TITL.Text      = dt.Rows[0]["CSR_TITL"].ToString();
                this.txtREQ_USERNAME.Text  = dt.Rows[0]["REQ_USERNAME"].ToString();
                this.txtREQ_DEPT.Text      = dt.Rows[0]["DEPTID"].ToString() + " : " + dt.Rows[0]["REQ_DEPT"].ToString();

                var reqDate = DateTime.ParseExact(dt.Rows[0]["RELS_REQ_DATE"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                this.txtRELS_REQ_DATE.Text = reqDate.ToShortDateString();

                this.txtRELS_YEAR.Text     = dt.Rows[0]["RELS_YEAR"].ToString();
                this.txtRELS_MNTH.Text     = dt.Rows[0]["RELS_MNTH"].ToString();
                this.txtRELS_WEEK.Text     = dt.Rows[0]["RELS_WEEK"].ToString();

                this.txtDEV_CNTT.Text      = dt.Rows[0]["DEV_CNTT"].ToString();
                {
                    this.gdDEPLOY_CONTENTS.RowDefinitions[5].Height = new GridLength(10);

                    if (this.txtDEV_CNTT.LineCount > 7) this.txtDEV_CNTT.Height = 140;
                    else this.gdDEPLOY_CONTENTS.RowDefinitions[10].Height = new GridLength(0);
                }
                this.txtCSR_CNTT.Text      = dt.Rows[0]["CSR_CNTT"].ToString();
                {
                    if (this.txtCSR_CNTT.LineCount > 7) this.txtCSR_CNTT.Height = 140;
                    //else this.gdDEPLOY_CONTENTS.RowDefinitions[15].Height = new GridLength(0);
                }

                DataTable dtsystem = ConvertToTable(dt.Rows[0]["LOGIC_TRGT_SYSTEM_GR_LST"].ToString().Replace("O|", "●|").Replace("Z","").Split('|'), ':');
                {
                    //배포 대상 시스템 멀티헤더용
                    if (isMultiHeaderForSystem == true)
                    {
                        _dvCMCD.RowFilter = "CMCDTYPE = 'BIZ_DVSN' AND USE_FLAG = 'Y' ";
                        DataTable dtCMCD_System = _dvCMCD.ToTable();

                        DataTable dtblResult = JoinTwoDataTablesOnOneColumn(dtCMCD_System, dtsystem, "CMCODE", "Key", JoinType.Left);
                        var result_table = (from datarow in dtblResult.AsEnumerable()
                                            select new
                                            {
                                                Key = ((string)datarow["CMCODE"])
                                             ,
                                                Val = (string)datarow["Val"]
                                             ,
                                                Header = ((string)datarow["CMCDNAME"]).Replace("[", "").Replace("]", "").Replace("_", ",").Replace(" ", ",")
                                            }).ToList();

                        dtsystem.Clear();
                        dtsystem.Columns.Add("Header");
                        dtsystem = CreateDataTableFromAnyCollection(result_table);
                    }
                }

                DataTable dtblPivotResult = PivotTable(dtsystem, "Val", "Key");
                {
                    //Util.GridSetData(dgSystem, dtblPivotResult, this.FrameOperation, false);
                    dgSystem.Columns.Clear();
                    dgSystem.AutoGenerateColumns = true;
                    dgSystem.ItemsSource = DataTableConverter.Convert(dtblPivotResult);

                    //배포 대상 시스템 멀티헤더용
                    if (isMultiHeaderForSystem == true)
                    {
                        dgSystem.Columns.RemoveAt(0);

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgSystem.Columns)
                        {
                            //if (dgc.ActualWidth > 0)
                            {
                                dgc.Width = new C1.WPF.DataGrid.DataGridLength(80, C1.WPF.DataGrid.DataGridUnitType.Star);
                                dgc.HorizontalAlignment = HorizontalAlignment.Center;

                                //DataGridMergeExtension.SetMergeMode(dgc, DataGridMergeMode.VERTICAL);
                                dgc.Header = dtblPivotResult.Rows[1][dgc.Name].ToString().Split(',').ToList();
                            }
                        }

                        dgSystem.Rows[3].Visibility = Visibility.Collapsed;
                        this.gdDEPLOY_CONTENTS.RowDefinitions[19].Height = new GridLength(40);
                    }
                }


                DataTable dtCorp   = ConvertToTable(dt.Rows[0]["LOGIC_TRGT_CORP_LST"].ToString().Replace("O|", "●|").Replace("Z", "").Split('|'), ':');
                {
                    //((C1.WPF.DataGrid.DataGridTextColumn)dgSite.Columns["LOGIC_TRGT_CROP"]).Binding = new Binding("Key");
                    //((C1.WPF.DataGrid.DataGridTextColumn)dgSite.Columns["LOGIC_TRGT_CROP_FLAG"]).Binding = new Binding("Val");
                    //Util.GridSetData(dgSite, dtCorp, this.FrameOperation, false);
                }

                DataTable dtblPivotResult2 = PivotTable(dtCorp, "Val", "Key");
                {
                    //Util.GridSetData(dgSite, dtblPivotResult2, this.FrameOperation, false);

                    dgSite.Columns.Clear();
                    dgSite.AutoGenerateColumns = true;
                    dgSite.ItemsSource = DataTableConverter.Convert(dtblPivotResult2);

                    dgSite.Columns.RemoveAt(0);
                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgSite.Columns)
                    {
                        //if (dgc.ActualWidth > 0)
                        {
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(80, C1.WPF.DataGrid.DataGridUnitType.Star);
                            dgc.HorizontalAlignment = HorizontalAlignment.Center;
                        }
                    }
                }
            });
        }

        private void dgCommon_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    { 
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                }
            }));
        }

        private void dgCommon_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }
        #endregion

        #region Method
        private void Get_DA_BAS_SEL_TB_SFC_GMES_RELS_REQ(string sRELS_REQ_ID, Action<DataTable, Exception> ACTION_COMPLETED)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("RELS_REQ_ID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["RELS_REQ_ID"] = sRELS_REQ_ID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_GMES_RELS_REQ", "INDATA", "OUTDATA", IndataTable, (Result, Exception) =>
            {
                try
                {
                    if (ACTION_COMPLETED != null) ACTION_COMPLETED(Result, Exception);
                }
                catch (Exception ex)
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            );
        }

        private DataTable ConvertToTable(string[] txtArray, char separator)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Key", typeof(string));
            dt.Columns.Add("Val", typeof(string));

            foreach (string item in txtArray)
            {
                if (string.IsNullOrEmpty(item)) continue;

                string[] dic = item.Split(separator);

                DataRow dr = dt.NewRow();
                dr["Key"] = dic[0];
                dr["Val"] = dic[1];
                dt.Rows.Add(dr);
            }

            return dt;
        }

        enum JoinType
        {
            /// <summary>
            /// Same as regular join. Inner join produces only the set of records that match in both Table A and Table B.
            /// </summary>
            Inner = 0,
            /// <summary>
            /// Same as Left Outer join. Left outer join produces a complete set of records from Table A, with the matching records (where available) in Table B. If there is no match, the right side will contain null.
            /// </summary>
            Left = 1
        }

        private DataTable JoinTwoDataTablesOnOneColumn(DataTable dtblLeft, DataTable dtblRight, string leftColToJoinOn, string rightColToJoinOn, JoinType joinType)
        {
            //Change column name to a temp name so the LINQ for getting row data will work properly.
            string strTempColName = leftColToJoinOn + "_2";
            if (dtblRight.Columns.Contains(leftColToJoinOn))
                dtblRight.Columns[leftColToJoinOn].ColumnName = strTempColName;

            //Get columns from dtblLeft
            DataTable dtblResult = dtblLeft.Clone();

            //Get columns from dtblRight
            var dt2Columns = dtblRight.Columns.OfType<DataColumn>().Select(dc => new DataColumn(dc.ColumnName, dc.DataType, dc.Expression, dc.ColumnMapping));

            //Get columns from dtblRight that are not in dtblLeft
            var dt2FinalColumns = from dc in dt2Columns.AsEnumerable()
                                  where !dtblResult.Columns.Contains(dc.ColumnName)
                                  select dc;

            //Add the rest of the columns to dtblResult
            dtblResult.Columns.AddRange(dt2FinalColumns.ToArray());

            //No reason to continue if the colToJoinOn does not exist in both DataTables.
            if (!dtblLeft.Columns.Contains(leftColToJoinOn) || (!dtblRight.Columns.Contains(rightColToJoinOn) && !dtblRight.Columns.Contains(strTempColName)))
            {
                if (!dtblResult.Columns.Contains(rightColToJoinOn))
                    dtblResult.Columns.Add(rightColToJoinOn);
                return dtblResult;
            }

            switch (joinType)
            {

                default:
                case JoinType.Inner:
                    #region Inner
                    //get row data
                    //To use the DataTable.AsEnumerable() extension method you need to add a reference to the System.Data.DataSetExtension assembly in your project. 
                    var rowDataLeftInner = from rowLeft in dtblLeft.AsEnumerable()
                                           join rowRight in dtblRight.AsEnumerable() on rowLeft[leftColToJoinOn] equals rowRight[rightColToJoinOn]
                                           select rowLeft.ItemArray.Concat(rowRight.ItemArray).ToArray();


                    //Add row data to dtblResult
                    foreach (object[] values in rowDataLeftInner)
                        dtblResult.Rows.Add(values);

                    #endregion
                    break;
                case JoinType.Left:
                    #region Left
                    var rowDataLeftOuter = from rowLeft in dtblLeft.AsEnumerable()
                                           join rowRight in dtblRight.AsEnumerable() on rowLeft[leftColToJoinOn] equals rowRight[rightColToJoinOn] into gj
                                           from subRight in gj.DefaultIfEmpty()
                                           select rowLeft.ItemArray.Concat((subRight == null) ? (dtblRight.NewRow().ItemArray) : subRight.ItemArray).ToArray();


                    //Add row data to dtblResult
                    foreach (object[] values in rowDataLeftOuter)
                        dtblResult.Rows.Add(values);

                    #endregion
                    break;
            }

            //Change column name back to original
            if (dtblRight.Columns.Contains(strTempColName))
            {
                dtblRight.Columns[strTempColName].ColumnName = leftColToJoinOn;

                //Remove extra column from result
                dtblResult.Columns.Remove(strTempColName);
            }

            return dtblResult;
        }

        private DataTable PivotTable (DataTable dataTable, string firstColumnName, string pivotColumnName)
        {
            DataTable pivotedDataTable = new DataTable();

            pivotedDataTable.Columns.Add(firstColumnName);

            pivotedDataTable.Columns.AddRange(
                        dataTable.Rows.Cast<DataRow>().Select(x => new DataColumn(x[pivotColumnName].ToString())).ToArray());

            for (var index = 1; index < dataTable.Columns.Count; index++)
            {
                pivotedDataTable.Rows.Add(
                    new List<object> { dataTable.Columns[index].ColumnName }.Concat(
                        dataTable.Rows.Cast<DataRow>().Select(x => x[dataTable.Columns[index].ColumnName])).ToArray());
            }

            return pivotedDataTable;
        }

        private DataTable CreateDataTableFromAnyCollection<T>(IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity, null);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        #endregion

    }
}
 