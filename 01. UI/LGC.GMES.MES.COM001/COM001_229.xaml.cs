/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.12.04  신광희 차장 : 데이터 그리드 컬럼 Sorting 후 저장 로직 예외 케이스 수정
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using C1.WPF.DataGrid;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_229 : UserControl, IWorkArea
    {
        #region Declaration & Constructor  
        DataTable dtList = new DataTable();
        ControlTemplate saveCellTemplate = null;
        string saveProductCode = string.Empty;

        public COM001_229()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Initialize();
        }

        private void Initialize()
        {
            InitializeCombo();
        }

        #endregion

        #region Event
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    //DataGridCel
                    //if (e.Cell.Presenter)

                    if (e.Cell.Column.Name == "PRJT_NAME" || e.Cell.Column.Name == "PRDT_ABBR_NAME")
                    {
                        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns[e.Cell.Column.Name].Index).Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        dataGrid.GetCell(e.Cell.Row.Index, dgList.Columns[e.Cell.Column.Name].Index).Presenter.IsEnabled = true;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearComboBoxVer();

                DataTable dt;
                DataRow dr;

                if (dgList.GetRowCount() == 0)
                {
                    InitGrid();

                    //추가한 Row 자동 세팅
                    DataTable temp = DataTableConverter.Convert(dgList.ItemsSource);

                    temp.Rows[temp.Rows.Count - 1]["CHK"] = true;
                    temp.Rows[temp.Rows.Count - 1]["USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["COATER_USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["ROLLPRESS_USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["SLIT_USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["SEARCH_YN"] = "N";
                    temp.Rows[temp.Rows.Count - 1]["MODI_YN"] = "Y";

                    //temp.Rows[temp.Rows.Count - 1]["INSUSER_NAME"] =LoginInfo.USERNAME;
                    //temp.Rows[temp.Rows.Count - 1]["INSDTTM"] = "Y";
                    //temp.Rows[temp.Rows.Count - 1]["UPDUSER_NAME"] = "Y";
                    //temp.Rows[temp.Rows.Count - 1]["UPDDTTM"] = "Y";
                    temp.AcceptChanges();

                    dgList.ItemsSource = DataTableConverter.Convert(temp);

                    dtList = DataTableConverter.Convert(dgList.ItemsSource);
                }
                else
                {
                    dt = DataTableConverter.Convert(dgList.ItemsSource);
                    dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    dgList.ItemsSource = DataTableConverter.Convert(dt);

                    //추가한 Row 자동 세팅
                    DataTable temp = DataTableConverter.Convert(dgList.ItemsSource);

                    temp.Rows[temp.Rows.Count - 1]["CHK"] = true;
                    temp.Rows[temp.Rows.Count - 1]["USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["COATER_USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["ROLLPRESS_USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["SLIT_USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["SEARCH_YN"] = "N";
                    temp.Rows[temp.Rows.Count - 1]["MODI_YN"] = "Y";
                    temp.AcceptChanges();

                    dgList.ItemsSource = DataTableConverter.Convert(temp); ;

                    dtList.Rows.Add();
                    dtList.Rows[temp.Rows.Count - 1]["CHK"] = true;
                    dtList.Rows[temp.Rows.Count - 1]["USE_FLAG"] = "Y";
                    dtList.Rows[temp.Rows.Count - 1]["COATER_USE_FLAG"] = "Y";
                    dtList.Rows[temp.Rows.Count - 1]["ROLLPRESS_USE_FLAG"] = "Y";
                    dtList.Rows[temp.Rows.Count - 1]["SLIT_USE_FLAG"] = "Y";
                    dtList.Rows[temp.Rows.Count - 1]["SEARCH_YN"] = "N";
                    dtList.Rows[temp.Rows.Count - 1]["MODI_YN"] = "Y";
                    dtList.AcceptChanges();
                }

                // 스프레드 스크롤 하단으로 이동
                dgList.ScrollIntoView(dgList.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                //Util.MessageInfo(ex.Message);
                Util.MessageInfo(ex.Data["CODE"].ToString());

            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                search();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgList)) return;
                this.dgList.EndEdit();
                this.dgList.EndEditRow(true);
                if (!Validation()) return;
                Save();
                search();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (dgList.CurrentColumn.Name == "PRODID")
                {
                    if (dgList.CurrentCell.Value.ToString().Length == 0)
                    {
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRJT_NAME", "");
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRDT_ABBR_NAME", "");
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PROD_VER_CODE", "");
                        
                        dtList = DataTableConverter.Convert(dgList.ItemsSource);
                        return;
                    }

                    if (dgList.GetRowCount() == 0)
                    {
                        return;
                    }

                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("PRODID", typeof(string));

                    DataRow searchCondition = inDataTable.NewRow();
                    searchCondition["LANGID"] = LoginInfo.LANGID;
                    searchCondition["PRODID"] = dgList.CurrentCell.Value.ToString();

                    inDataTable.Rows.Add(searchCondition);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_CHK_PRODID", "INDATA", "OUTDATA", inDataTable);

                    if (dtResult.Rows.Count == 0)
                    {
                        Util.AlertInfo("SFU4211"); //등록된 제품id가 아닙니다.
                        return;
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRJT_NAME", dtResult.Rows[0]["PRJT_NAME"].ToString());
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRDT_ABBR_NAME", dtResult.Rows[0]["PRDT_ABBR_NAME"].ToString());

                        //버전 초기화
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PROD_VER_CODE", "");
                        
                        DataTable dt = ((DataView)dgList.ItemsSource).Table;
                        dt.AcceptChanges();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["SEARCH_YN"].GetString() == "N")
                            {
                                dt.Rows[i].SetAdded();
                            }
                            else
                            {
                                dt.Rows[i].SetModified();
                            }
                        }

                        dgList.ItemsSource = DataTableConverter.Convert(dt);
                        dtList = DataTableConverter.Convert(dgList.ItemsSource);
                    }
                }
            }
        }

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            int row_idx = e.Row.Index;
            int col_idx = e.Column.Index;

            string e_cell_value = string.Empty;

            if (e.Column.Name.Equals("USE_FLAG") || e.Column.Name.Equals("PRODID") || e.Column.Name.Equals("PROD_VER_CODE") ||
                e.Column.Name.Equals("COATER_USE_FLAG") || e.Column.Name.Equals("ROLLPRESS_USE_FLAG") || e.Column.Name.Equals("SLIT_USE_FLAG"))
            {
                if (DataTableConverter.GetValue(e.Row.DataItem, "CHK").Nvc().Equals("False") || DataTableConverter.GetValue(e.Row.DataItem, "CHK").Nvc().Equals("0"))
                {
                    e.Cancel = true; // Editing 불가능
                }
                else
                {
                    if (e.Column.Name.Equals("PROD_VER_CODE") && DataTableConverter.GetValue(e.Row.DataItem, "SEARCH_YN").Nvc().Equals("Y"))
                    {
                        e.Cancel = true; // Editing 불가능
                    }
                    else
                    {
                        e.Cancel = false; // Editing 가능

                        if (e.Column.Name.Equals("PROD_VER_CODE"))
                        {                            
                            C1.WPF.DataGrid.DataGridCell cell = dgList.GetCell(e.Row.Index, e.Column.Index);

                            if (cell.Presenter != null && cell.Presenter.Content is TextBlock)
                            {
                                string saveValue = DataTableConverter.GetValue(dgList.Rows[cell.Row.Index].DataItem, "PROD_VER_CODE").Nvc();
                                saveCellTemplate = cell.Presenter.Template;

                                cell.Presenter.Template = dgList.Resources["cellComboBox"] as ControlTemplate;
                                cell.Presenter.ApplyTemplate();

                                DataGridCellPresenter dgcp = cell.Presenter as DataGridCellPresenter;
                                C1ComboBox cbo = dgcp.FindChild<C1ComboBox>("cboCellComboBox");
                                if (cbo != null)
                                {
                                    string[] arrColum = { "SHOPID", "PRODID" };
                                    string[] arrCondition = { LoginInfo.CFG_SHOP_ID, DataTableConverter.GetValue(e.Row.DataItem, "PRODID").IsNvc() ? null : DataTableConverter.GetValue(e.Row.DataItem, "PRODID").Nvc() };
                                    cbo.SetDataComboItem("DA_BAS_VER_TB_TB_MMD_PRDT_CONV_RATE", arrColum, arrCondition);
                                    cbo.SelectedValue = saveValue;
                                    cbo.Focus();
                                }
                            }
                            else
                            {
                                e.Cancel = true;                                
                                return;
                            }
                        }

                        if (e.Column.Name.Equals("PRODID"))
                        {
                            saveProductCode = DataTableConverter.GetValue(dgList.Rows[row_idx].DataItem, "PRODID").Nvc();

                            ClearComboBoxVer();
                            e.Row.Refresh();

                            if (e.Column.Name.Equals("PRODID") && DataTableConverter.GetValue(e.Row.DataItem, "SEARCH_YN").Nvc().Equals("Y"))
                            {
                                e.Cancel = true; // Editing 불가능
                            }
                            else
                            {
                                e.Cancel = false; // Editing 가능
                            }

                            if (DataTableConverter.GetValue(dgList.Rows[row_idx].DataItem, "PRODID") == null)
                            {
                                return;
                            }                            
                        }
                    }
                }
            }
        }

        private void dgList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            int _row = 0;
            int _col = 0;
            int _top_cnt = 0;
            string value = string.Empty;
            string old_value = string.Empty;
            string column_name = string.Empty;

            if (e.Cell.Value == null)
            {
                return;
            }

            column_name = e.Cell.Column.Name;

            _top_cnt = dgList.Rows.TopRows.Count;
            _row = e.Cell.Row.Index - _top_cnt;
            _col = e.Cell.Column.Index;
            value = e.Cell.Value.ToString();

            if (_row >= dtList.Rows.Count) return;

            old_value = (dtList.Rows[_row] as DataRow)[e.Cell.Column.Name].Nvc();

            if (value.Length == 0)
            {
                return;

            }

            if (column_name == "PROD_VER_CODE" || column_name == "USE_FLAG" ||
                column_name == "COATER_USE_FLAG" || column_name == "ROLLPRESS_USE_FLAG" || column_name == "SLIT_USE_FLAG" || column_name == "PROD_VER_CODE")
            {
                if (value != old_value)
                {
                    DataTableConverter.SetValue(dgList.Rows[_row + _top_cnt].DataItem, "MODI_YN", "Y");
                }
                else
                {
                    if ((dtList.Rows[_row] as DataRow)["PROD_VER_CODE"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE")) &&
                        (dtList.Rows[_row] as DataRow)["COATER_USE_FLAG"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "COATER_USE_FLAG")) &&
                        (dtList.Rows[_row] as DataRow)["ROLLPRESS_USE_FLAG"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "ROLLPRESS_USE_FLAG")) &&
                        (dtList.Rows[_row] as DataRow)["SLIT_USE_FLAG"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "SLIT_USE_FLAG")) &&
                        (dtList.Rows[_row] as DataRow)["USE_FLAG"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "USE_FLAG"))
                        )
                    {
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "MODI_YN", "N");
                    }
                }

                if (column_name == "PROD_VER_CODE")
                {
                    //foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                    //{
                    //    if (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PRODID").Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"))))
                    //    {
                    //        if (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE").Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROD_VER_CODE"))))
                    //        {
                    //            if (e.Cell.Row.Index != row.Index)
                    //            {
                    //                DataTableConverter.SetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE", "");
                    //                Util.MessageInfo("SFU3471", Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROD_VER_CODE")));//[%1]은 이미 등록되었습니다.                                                            
                    //            }
                    //        }
                    //    }
                    //}

                    //SetGridCboItem_VER_ALL(dgList.Columns["PROD_VER_CODE"]);
                }
            }

            if (column_name.Equals("PRODID"))
            {
                string productCode = DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRODID").Nvc();
                if (!saveProductCode.Equals(productCode))
                {
                    DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PROD_VER_CODE", "");
                    e.Cell.Row.Refresh();
                }
            }
        }

        #endregion

        #region METHOD

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnAddRow);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitializeCombo()
        {
            try
            {
                #region 조회조건 combo
                setCombo();
                #endregion

                #region GRID combo         
                SetGridCboitem_USEYN(dgList.Columns["USE_FLAG"]);
                SetGridCboItem(dgList.Columns["COATER_USE_FLAG"]);
                SetGridCboItem(dgList.Columns["ROLLPRESS_USE_FLAG"]);
                SetGridCboItem(dgList.Columns["SLIT_USE_FLAG"]);
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitGrid()
        {
            DataTable dtTable = new DataTable();
            dtTable.Columns.Add("CHK", typeof(string));
            dtTable.Columns.Add("USE_FLAG", typeof(string));
            dtTable.Columns.Add("PRODID", typeof(string));
            dtTable.Columns.Add("PRJT_NAME", typeof(string));
            dtTable.Columns.Add("PRDT_ABBR_NAME", typeof(string));
            dtTable.Columns.Add("PRDT_ABBR_CODE", typeof(string));
            dtTable.Columns.Add("PROD_VER_CODE", typeof(string));
            dtTable.Columns.Add("COATER_USE_FLAG", typeof(string));
            dtTable.Columns.Add("ROLLPRESS_USE_FLAG", typeof(string));
            dtTable.Columns.Add("SLIT_USE_FLAG", typeof(string));
            dtTable.Columns.Add("SEARCH_YN", typeof(string));
            dtTable.Columns.Add("MODI_YN", typeof(string));

            DataRow dr = dtTable.NewRow();

            dtTable.Rows.Add(dr);

            dgList.ItemsSource = DataTableConverter.Convert(dtTable);
        }

        private bool Validation()
        {
            try
            {
                bool chk = false;

                if (CommonVerify.HasDataGridRow(dgList))
                {
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True")
                        {
                            chk = true;

                            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"))))
                            {
                                Util.Alert("SFU2052"); //	입력된 항목이 없습니다.
                                return false;
                            }

                            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROD_VER_CODE"))))
                            {
                                Util.Alert("SFU2052"); //	입력된 항목이 없습니다.
                                return false;
                            }
                        }
                    }

                    if (!chk)
                    {
                        Util.Alert("SFU1651"); //	선택된 항목이 없습니다.
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Save()
        {
            try
            {
                DataTable inputTable = new DataTable("INDATA");
                inputTable.Columns.Add("SHOPID", typeof(string));
                inputTable.Columns.Add("PRODID", typeof(string));
                inputTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inputTable.Columns.Add("ROLLPRESS_USE_FLAG", typeof(string));
                inputTable.Columns.Add("COATER_USE_FLAG", typeof(string));
                inputTable.Columns.Add("SLIT_USE_FLAG", typeof(string));
                inputTable.Columns.Add("USE_FLAG", typeof(string));
                inputTable.Columns.Add("USERID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (row.Type == DataGridRowType.Item &&
                        (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True" || DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "1"))
                    {
                        DataRow InputRow = inputTable.NewRow();

                        InputRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        InputRow["PRODID"] = DataTableConverter.GetValue(row.DataItem, "PRODID"); ;
                        InputRow["PROD_VER_CODE"] = DataTableConverter.GetValue(row.DataItem, "PROD_VER_CODE");
                        InputRow["ROLLPRESS_USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "ROLLPRESS_USE_FLAG");
                        InputRow["COATER_USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "COATER_USE_FLAG");
                        InputRow["SLIT_USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "SLIT_USE_FLAG");
                        InputRow["USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "USE_FLAG");
                        InputRow["USERID"] = LoginInfo.USERID;

                        inputTable.Rows.Add(InputRow);

                        //if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "SEARCH_YN")) == "N") // INSERT
                        if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added)
                        {
                            new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_WIP_HOLD_EXCT_SET", "INDATA", null, inputTable);
                            inputTable.Rows.Clear();
                        }
                        else // UPDATE
                        {
                            //if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "MODI_YN")) == "Y")
                            if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                            {
                                new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_TB_SFC_WIP_HOLD_EXCT_SET", "INDATA", null, inputTable);
                            }

                            inputTable.Rows.Clear();
                        }
                    }
                }

                Util.AlertInfo("SFU1889"); //정상 처리 되었습니다
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void search()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                searchCondition["USE_FLAG"] = Util.GetCondition(cboUseFlag);

                inDataTable.Rows.Add(searchCondition);

                Util.gridClear(dgList);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WIP_HOLD_EXCT_SET", "INDATA", "OUTDATA", inDataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation);
                    dtList = dtResult;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SetGridCboItem_VER_ALL(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SHOPID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(searchCondition);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_VER_TB_TB_MMD_PRDT_CONV_RATE", "RQSTDT", "RSLTDT", inDataTable);

                if (dt == null || dt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU4909"); //해당 제품은 버전 정보가 없습니다.                   
                }

                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setCombo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboUseFlag.ItemsSource = DataTableConverter.Convert(dt);
                cboUseFlag.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y";
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N";
                dr1["CBO_CODE"] = "N";

                dt.Rows.Add(dr1);
                dt.AcceptChanges();

                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetGridCboitem_USEYN(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("사용");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("사용 안함");
                dr1["CBO_CODE"] = "N";

                dt.Rows.Add(dr1);

                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        private void ClearComboBoxVer()
        {
            dgList.EndEdit();
            for (int row = 0; row < dgList.Rows.Count; row++)
            {
                C1.WPF.DataGrid.DataGridCell cell = dgList.GetCell(row, dgList.Columns["PROD_VER_CODE"].Index);
                DataGridCellPresenter dgcp = cell.Presenter as DataGridCellPresenter;

                if (dgcp != null && saveCellTemplate != null)
                {
                    dgcp.Template = saveCellTemplate;
                    dgcp.ApplyTemplate();
                }
            }            
        }

        private void cboCellComboBox_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox combo = sender as C1ComboBox;
            DataGridCellPresenter dgcp = combo.TemplatedParent as DataGridCellPresenter;
            if (dgcp != null && dgcp.Row != null)
            {
                dgList.EndEdit();
                DataTableConverter.SetValue(dgList.Rows[dgcp.Row.Index].DataItem, "PROD_VER_CODE", e.NewValue);
            }
        }

        private void cboCellComboBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(false))
            {
                ClearComboBoxVer();
            }
        }
    }
}
