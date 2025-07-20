/*************************************************************************************
 Created Date : 2018.03.19
      Creator : 장만철
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2024.06.04  김태균    E20240604-000649   : 버전 선택시 오류 수정  




 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_225 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable dtTemp = new DataTable();
        DataTable dtList = new DataTable();
        public COM001_225()
        {
            try
            {
                InitializeComponent();
                Initialize();                
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize
        private void Initialize()
        {
            try
            {
                InitCombo();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event

        #region LOADED EVENT
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }
        #endregion

        #region GRID Event

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "PRODID" || e.Cell.Column.Name == "PROD_VER_CODE" || e.Cell.Column.Name == "PRDT_ABBR_NAME" || e.Cell.Column.Name == "PROCID" || e.Cell.Column.Name == "HOLD_CODE")
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

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            int row_idx = e.Row.Index;
            int col_idx = e.Column.Index;

            string e_cell_value = string.Empty;
            string column_name = string.Empty;

            if (DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals("False") || DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals("0"))
            {
                e.Cancel = true; // Editing 불가능
            }
            else
            {
                if (DataTableConverter.GetValue(e.Row.DataItem, "SEARCH_YN").Equals("Y"))
                {
                    if (e.Column.Name.Equals("PRODID") || e.Column.Name.Equals("PROD_VER_CODE") || e.Column.Name.Equals("PROCID") || e.Column.Name.Equals("PRDT_ABBR_NAME") ||
                    e.Column.Name.Equals("INSUSER_NAME") || e.Column.Name.Equals("INSDTTM") || e.Column.Name.Equals("UPDUSER_NAME") || e.Column.Name.Equals("UPDDTTM"))
                    {
                        e.Cancel = true; // Editing 불가능
                    }
                    else
                    {
                        e.Cancel = false; // Editing 가능
                    }
                }
                else
                {
                    column_name = e.Column.Name;

                    if (column_name.Equals("PRDT_ABBR_NAME"))
                    {
                        e.Cancel = true; // Editing 불가능
                        return;
                    }

                    e.Cancel = false; // Editing 가능

                    if (column_name.Equals("PROD_VER_CODE") || column_name.Equals("PROCID") || column_name.Equals("HOLD_CODE"))
                    {
                        if (DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRODID") == null || DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRODID").ToString() == "")
                        {
                            e.Cancel = true; // Editing 불가능
                            return;
                        }

                        //그리드 콤보 초기화
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, column_name, "");
                        (dgList.Columns[column_name] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;

                        if (DataTableConverter.GetValue(e.Row.DataItem, "PRODID") != null && DataTableConverter.GetValue(e.Row.DataItem, "PRODID").ToString().Length != 0)
                        {
                            if (column_name.Equals("PROD_VER_CODE")) //버전
                            {
                                //SetGridCboItem_VER(dgList.Columns[column_name], LoginInfo.CFG_SHOP_ID, DataTableConverter.GetValue(e.Row.DataItem, "PRODID").ToString()); 
                                if(!SetGridCboItem_VER(dgList.Columns[column_name], LoginInfo.CFG_SHOP_ID, DataTableConverter.GetValue(e.Row.DataItem, "PRODID").ToString()))
                                {
                                    Util.AlertInfo("SFU4909"); //해당 제품은 버전 정보가 없습니다.     
                                }                              
                            }
                            else if (column_name.Equals("PROCID")) //HOLD 공정
                            {
                                if (!SetGridCboItem_HOLD_PROC(dgList.Columns[column_name], DataTableConverter.GetValue(e.Row.DataItem, "PRODID").ToString()))
                                {
                                    Util.Alert("SFU1456"); //공정 정보가 없습니다. 
                                }
                            }
                            else
                            {
                                SetGridCboItem_HOLD_RESN(dgList.Columns[column_name]); //HOLD 사유
                            }
                        }
                    }
                }
            }
        }

        private void dgList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (dgList.CurrentColumn.Name == "PRODID")
                {
                    if (dgList.GetRowCount() == 0)
                    {
                        return;
                    }

                    if (dgList.CurrentCell.Value.ToString().Length == 0)
                    {
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PROD_VER_CODE", "");  //버전 초기화
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRDT_ABBR_NAME", ""); //극성 초기화
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PROCID", "");         //공정 초기화
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "HOLD_CODE", "");      //사유 초기화
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "HOLD_NOTE", "");      //비고 초기화
                        (dgList.Columns["PROD_VER_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null; //버전 콤보 초기화
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
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRODID", "");
                        return;
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRDT_ABBR_NAME", dtResult.Rows[0]["PRDT_ABBR_NAME"].ToString());

                        //버전 초기화
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PROD_VER_CODE", "");
                        (dgList.Columns["PROD_VER_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = null;
                    }
                }
                else if(dgList.CurrentColumn.Name == "PROD_VER_CODE")
                {

                }
            }
        }

        private void dgList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
            old_value = (dtList.Rows[_row] as DataRow)[e.Cell.Column.Name].ToString();

            if (value.Length == 0)
            {
                return;
            }

            if (column_name == "PROD_VER_CODE" || column_name == "USE_FLAG" || column_name == "HOLD_NOTE" ||
                column_name == "PROCID" || column_name == "HOLD_CODE")
            {
                if (value != old_value)
                {
                    DataTableConverter.SetValue(dgList.Rows[_row + _top_cnt].DataItem, "MODI_YN", "Y");
                }
                else
                {
                    if ((dtList.Rows[_row] as DataRow)["PROD_VER_CODE"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE")) &&
                        (dtList.Rows[_row] as DataRow)["USE_FLAG"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "USE_FLAG")) &&
                        (dtList.Rows[_row] as DataRow)["PROCID"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROCID")) &&
                        (dtList.Rows[_row] as DataRow)["HOLD_CODE"].ToString().Equals(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "HOLD_CODE"))
                        )
                    {
                        DataTableConverter.SetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "MODI_YN", "N");
                    }
                }

                if (column_name == "PROD_VER_CODE" || column_name == "PROCID")
                {
                    //Grid 중복 체크
                    foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                    {
                        if (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PRODID").Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"))))
                        {
                            if (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE").Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROD_VER_CODE"))))
                            {
                                if(DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROCID") != null && (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROCID").Equals(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROCID")))))
                                {
                                    if (e.Cell.Row.Index != row.Index)
                                    {
                                        if (column_name == "PROCID")
                                        {
                                            DataTableConverter.SetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROCID", "");
                                        }
                                        else
                                        {
                                            DataTableConverter.SetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE", "");
                                        }
                                        
                                        Util.MessageInfo("SFU3471", " " +   ObjectDic.Instance.GetObjectName("반제품") + "(" + Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID")) + ") / " +
                                                                            ObjectDic.Instance.GetObjectName("버전")   + "(" + Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROD_VER_CODE")) + ") / " +
                                                                            ObjectDic.Instance.GetObjectName("공정")   + "(" + Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROCID")) + ") "
                                                        );//[%1]은 이미 등록되었습니다.                                                            
                                    }
                                }
                            }
                        }
                    }

                    //DB 중복 체크
                    DataTable dt = dtHistResult();
                    if (dt != null)     //버전 선택시 오류 수정
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PRODID").ToString() == dt.Rows[i]["PRODID"].ToString())
                            {
                                if (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE").ToString() == dt.Rows[i]["PROD_VER_CODE"].ToString())
                                {
                                    if (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROCID") != null && (DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROCID").ToString() == dt.Rows[i]["PROCID"].ToString()))
                                    {
                                        if (column_name == "PROCID")
                                        {
                                            DataTableConverter.SetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROCID", "");
                                        }
                                        else
                                        {
                                            DataTableConverter.SetValue(dgList.Rows[_row + _top_cnt].DataItem, "PROD_VER_CODE", "");
                                        }

                                        Util.MessageInfo("SFU3471", " " + ObjectDic.Instance.GetObjectName("반제품") + "(" + dt.Rows[i]["PRODID"].ToString() + ") / " +
                                                                          ObjectDic.Instance.GetObjectName("버전") + "(" + dt.Rows[i]["PROD_VER_CODE"].ToString() + ") / " +
                                                                          ObjectDic.Instance.GetObjectName("공정") + "(" + dt.Rows[i]["PROCID"].ToString() + ") "
                                                        );//[%1]은 이미 등록되었습니다.               
                                    }
                                }
                            }
                        }
                    } 

                    if(column_name == "PROCID")
                    {
                        SetGridCboItem_HOLD_PROC(dgList.Columns[column_name], DataTableConverter.GetValue(dgList.Rows[_row + _top_cnt].DataItem, "PRODID").ToString());
                    }
                    else
                    {
                        SetGridCboItem_VER(dgList.Columns["PROD_VER_CODE"], LoginInfo.CFG_SHOP_ID, null);                        
                    }
                }
            }
        }

        private DataTable dtHistResult()
        {
            try
            {
                DataTable dt = new DataTable();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                searchCondition["PROCID"] = null;
                searchCondition["PRODID"] = null;
                searchCondition["PROD_VER_CODE"] = null;
                searchCondition["USE_FLAG"] = null;

                inDataTable.Rows.Add(searchCondition);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WIP_HOLD_SET", "INDATA", "OUTDATA", inDataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dt = dtResult;
                }
                else
                {
                    dt = null;
                }

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        #endregion

        #region BUTTON Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetGridCboItem_VER(dgList.Columns["PROD_VER_CODE"], LoginInfo.CFG_SHOP_ID, null);
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
                bool change_yn = false;

                if (!CommonVerify.HasDataGridRow(dgList)) return;
                this.dgList.EndEdit();
                this.dgList.EndEditRow(true);
                if (!Validation()) return;

                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "SEARCH_YN")) == "N") // INSERT
                        {
                            change_yn = true;
                        }
                        else // UPDATE
                        {
                            if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "MODI_YN")) == "Y")
                            {
                                change_yn = true;
                            }
                        }
                    }
                }

                if (change_yn)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("9085"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            try
                            {
                                Save();
                                search();
                                return;
                            }
                            catch(Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                    });
                }
                else
                {
                    Util.AlertInfo("9019"); //변경된 내용이 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt;
                DataRow dr;

                if (dgList.GetRowCount() == 0)
                {
                    InitGrid();

                    //추가한 Row 자동 세팅
                    DataTable temp = DataTableConverter.Convert(dgList.ItemsSource);

                    temp.Rows[temp.Rows.Count - 1]["CHK"] = true;
                    temp.Rows[temp.Rows.Count - 1]["USE_FLAG"] = "Y";
                    temp.Rows[temp.Rows.Count - 1]["SEARCH_YN"] = "N";
                    temp.Rows[temp.Rows.Count - 1]["MODI_YN"] = "Y";

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
                    temp.Rows[temp.Rows.Count - 1]["SEARCH_YN"] = "N";
                    temp.Rows[temp.Rows.Count - 1]["MODI_YN"] = "Y";
                    temp.AcceptChanges();

                    dgList.ItemsSource = DataTableConverter.Convert(temp); ;

                    dtList.Rows.Add();
                    dtList.Rows[temp.Rows.Count - 1]["CHK"] = true;
                    dtList.Rows[temp.Rows.Count - 1]["USE_FLAG"] = "Y";
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

        #endregion

        #region 기타
        private void txtProdid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sProdid = txtProdid.Text.Trim();

                    if (sProdid.Length == 0)
                    {
                        return;
                    }

                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("PRODID", typeof(string));

                    DataRow searchCondition = inDataTable.NewRow();
                    searchCondition["LANGID"] = LoginInfo.LANGID;
                    searchCondition["PRODID"] = sProdid;

                    inDataTable.Rows.Add(searchCondition);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_CHK_PRODID", "INDATA", "OUTDATA", inDataTable);

                    if (dtResult.Rows.Count == 0)
                    {
                        Util.AlertInfo("SFU4211"); //등록된 제품id가 아닙니다.
                        txtProdid.Text = "";
                        txtProdid.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Mehod
        private void InitCombo()
        {
            try
            {
                setCombo();
                setGridCombo();
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
                setUseYN();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setGridCombo()
        {
            try
            {
                SetGridCboitem_USEYN(dgList.Columns["USE_FLAG"]);     //사용유무              
                SetGridCboItem_HOLD_RESN(dgList.Columns["HOLD_CODE"]);      //HOLD 사유
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setUseYN()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr_);

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

        private bool SetGridCboItem_VER(C1.WPF.DataGrid.DataGridColumn col, string sShopid, string sProdid)
        {
            try
            {
                bool rtn = false;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["SHOPID"] = sShopid;
                searchCondition["PRODID"] = sProdid;

                inDataTable.Rows.Add(searchCondition);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_VER_TB_TB_MMD_PRDT_CONV_RATE", "RQSTDT", "RSLTDT", inDataTable);

                if (dt == null || dt.Rows.Count == 0)
                {
                    rtn = false;              
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool SetGridCboItem_HOLD_PROC(C1.WPF.DataGrid.DataGridColumn col, string sProdid)
        {
            try
            {
                bool rtn = false;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PRODID"] = sProdid;

                inDataTable.Rows.Add(searchCondition);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_PROCESS", "RQSTDT", "RSLTDT", inDataTable);

                if (dt == null || dt.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetGridCboItem_HOLD_RESN(C1.WPF.DataGrid.DataGridColumn col)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "HOLD_LOT";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ACTIVITIREASON_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
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
            dtTable.Columns.Add("PROD_VER_CODE", typeof(string));
            dtTable.Columns.Add("PRDT_ABBR_NAME", typeof(string));

            dtTable.Columns.Add("PROCID", typeof(string));
            dtTable.Columns.Add("HOLD_CODE", typeof(string));
            dtTable.Columns.Add("HOLD_NOTE", typeof(string));

            dtTable.Columns.Add("INSUSER_NAME", typeof(string));
            dtTable.Columns.Add("INSDTTM", typeof(string));
            dtTable.Columns.Add("UPDUSER_NAME", typeof(string));
            dtTable.Columns.Add("UPDDTTM", typeof(string));
            dtTable.Columns.Add("SEARCH_YN", typeof(string));
            dtTable.Columns.Add("MODI_YN", typeof(string));

            DataRow dr = dtTable.NewRow();

            dtTable.Rows.Add(dr);

            dgList.ItemsSource = DataTableConverter.Convert(dtTable);
        }

        private void search()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SHOPID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow searchCondition = inDataTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                searchCondition["PROCID"] = null;
                searchCondition["PRODID"] = Util.GetCondition(txtProdid) == "" ? null : Util.GetCondition(txtProdid);
                searchCondition["PROD_VER_CODE"] = null;
                searchCondition["USE_FLAG"] = Util.GetCondition(cboUseFlag) == "ALL" ? null : Util.GetCondition(cboUseFlag);

                inDataTable.Rows.Add(searchCondition);

                Util.gridClear(dgList);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_WIP_HOLD_SET", "INDATA", "OUTDATA", inDataTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgList, dtResult, FrameOperation);
                }

                dtList = dtResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

                            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "PROCID"))))
                            {
                                Util.Alert("SFU2052"); //	입력된 항목이 없습니다.
                                return false;
                            }

                            if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(row.DataItem, "HOLD_CODE"))))
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
                inputTable.Columns.Add("PROCID", typeof(string));
                inputTable.Columns.Add("PRODID", typeof(string));
                inputTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inputTable.Columns.Add("HOLD_CODE", typeof(string));
                inputTable.Columns.Add("HOLD_NOTE", typeof(string));
                inputTable.Columns.Add("USE_FLAG", typeof(string));
                inputTable.Columns.Add("USERID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "True")
                    {
                        DataRow InputRow = inputTable.NewRow();

                        InputRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                        InputRow["PROCID"] = DataTableConverter.GetValue(row.DataItem, "PROCID");
                        InputRow["PRODID"] = DataTableConverter.GetValue(row.DataItem, "PRODID");
                        InputRow["PROD_VER_CODE"] = DataTableConverter.GetValue(row.DataItem, "PROD_VER_CODE");
                        InputRow["HOLD_CODE"] = DataTableConverter.GetValue(row.DataItem, "HOLD_CODE");
                        InputRow["HOLD_NOTE"] = DataTableConverter.GetValue(row.DataItem, "HOLD_NOTE");
                        InputRow["USE_FLAG"] = DataTableConverter.GetValue(row.DataItem, "USE_FLAG");
                        InputRow["USERID"] = LoginInfo.USERID;

                        inputTable.Rows.Add(InputRow);

                        if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "SEARCH_YN")) == "N") // INSERT
                        {
                            new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_WIP_HOLD_SET", "INDATA", null, inputTable);
                            inputTable.Rows.Clear();
                        }
                        else // UPDATE
                        {
                            if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "MODI_YN")) == "Y")
                            {
                                new ClientProxy().ExecuteServiceSync("DA_BAS_UPD_TB_SFC_WIP_HOLD_SET", "INDATA", null, inputTable);
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

        #endregion



    }
}
