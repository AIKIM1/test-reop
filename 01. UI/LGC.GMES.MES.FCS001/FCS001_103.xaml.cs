/*************************************************************************************
 Created Date : 2021.01.20
      Creator : 박준규
   Decription : 인수인계일지
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.20  DEVELOPER : 박준규





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// Lot별 Cell Data 조회
    /// </summary>
    public partial class FCS001_103 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string gsProcessType = string.Empty;
        private string gsEqpKind = string.Empty;

        private DataTable dtShift;

        public IFrameOperation FrameOperation {
            get;
            set;
        }

        Util _Util = new Util();

        public FCS001_103()
        {
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitUserGrid();

            
        }
     

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

     
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void GetList()
        {
            try
            {
                //SheetView fpSheet1 = fpsUser.ActiveSheet;
                //SheetView fpSheet3 = fpsTeam.ActiveSheet;

                //fpsLine.ActiveSheet.Rows.Clear();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_DATE", typeof(string));
                dtRqst.Columns.Add("WRKLOG_CLSS_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["WRKLOG_CLSS_CODE"] = "1"; //인수인계일지 : 1
                dtRqst.Rows.Add(dr);

                DataSet dsRqst = new DataSet();
                dsRqst.Tables.Add(dtRqst);

                // 비즈를 미완성 상태... 추가 확인 필요...
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_FORM_UI_GET_SHIFT_INSU", "INDATA", "OUTDATA_DIARY,OUTDATA_WORK", dsRqst);

                if (dsRslt.Tables["OUTDATA_DIARY"].Rows.Count < 1)
                {
                    for (int i = 0; i < dtShift.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(dgUser.Rows[0].DataItem, dgUser.Columns[i].Name, string.Empty);
                        dgUser.GetCell(0, i).Presenter.Tag = string.Empty;

                        DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[0].Name, string.Empty);
                        DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[1].Name, string.Empty);
                        DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[2].Name, string.Empty);
                        DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[3].Name, string.Empty);

                        //fpSheet1.Cells[0, i].Text = string.Empty;
                        //fpSheet1.Cells[0, i].Tag = string.Empty;
                        //fpSheet1.Cells[0, i].Text = string.Empty;
                        //fpSheet1.Cells[0, i].Tag = string.Empty;
                        //fpSheet1.Cells[0, i].Text = string.Empty;
                        //fpSheet1.Cells[0, i].Tag = string.Empty;

                        //fpSheet3.Cells[i, 0].Text = string.Empty;
                        //fpSheet3.Cells[i, 1].Text = string.Empty;
                        //fpSheet3.Cells[i, 2].Text = string.Empty;
                        //fpSheet3.Cells[i, 3].Text = string.Empty;
                    }
                }

                for (int i = 0; i < dsRslt.Tables["OUTDATA_DIARY"].Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgUser.Rows[0].DataItem, dgUser.Columns[i].Name, dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_USER_NAME"].ToString());
                    dgUser.GetCell(0, i).Presenter.Tag = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_ID"].ToString();

                    DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[0].Name, dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_AGING"].ToString());
                    DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[1].Name, dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_ETC"].ToString());
                    DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[2].Name, dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_FORAMTION_TEMP"].ToString());
                    DataTableConverter.SetValue(dgTeam.Rows[i].DataItem, dgTeam.Columns[3].Name, dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_HIGH_TEMP"].ToString());

                    //fpSheet1.Cells[0, i].Text = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_USER_NAME"].ToString();
                    //fpSheet1.Cells[0, i].Tag = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_ID"].ToString();
                    //fpSheet1.Cells[0, i].Text = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_USER_NAME"].ToString();
                    //fpSheet1.Cells[0, i].Tag = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_ID"].ToString();
                    //fpSheet1.Cells[0, i].Text = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_USER_NAME"].ToString();
                    //fpSheet1.Cells[0, i].Tag = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["SHIFT_ID"].ToString();

                    //fpSheet3.Cells[i, 0].Text = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_AGING"].ToString();
                    //fpSheet3.Cells[i, 1].Text = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_ETC"].ToString();
                    //fpSheet3.Cells[i, 2].Text = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_FORAMTION_TEMP"].ToString();
                    //fpSheet3.Cells[i, 3].Text = dsRslt.Tables["OUTDATA_DIARY"].Rows[i]["DIARY_HIGH_TEMP"].ToString();
                }

                Util.GridSetData(dgLine, dsRslt.Tables["OUTDATA_WORK"], FrameOperation, true);
                //fpsLine.SetDataSource(dsRslt.Tables["OUTDATA_WORK"], bDataFull: true, bMultiRowAutoFit: true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
       
        }

     

        private void InitCombo()
        {
         
        }

        private void InitControl()
        {
        
        }

        private void InitUserGrid()
        {
            //작업조 정보 조회
            DataTable dtRqst = new DataTable();
            DataRow dr = dtRqst.NewRow();
            dtRqst.Rows.Add(dr);
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TM_SHIFT_ID", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt.Rows.Count > 0)
            {
                dgUser.ClearRows();
                dgTeam.ClearRows();

                DataGridRowAdd(dgTeam, dtRslt.Rows.Count);

                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                column.Header = ".";
                column.Binding = new System.Windows.Data.Binding("INSU");                
                dgUser.Columns.Add(column);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    column = new C1.WPF.DataGrid.DataGridTextColumn();

                    column.Header = dtRslt.Rows[i]["SHIFT_NAME"].ToString();
                    column.Binding = new System.Windows.Data.Binding(dtRslt.Rows[i]["SHIFT_ID"].ToString());
                    dgUser.Columns.Add(column);

                    DataTableConverter.SetValue(dgTeam.Rows[i+1].DataItem, "TEAM", dtRslt.Rows[i]["SHIFT_NAME"].ToString());
                }

                dtShift = dtRslt.Copy();

                DataGridRowAdd(dgUser, 1);
                DataTableConverter.SetValue(dgUser.Rows[0].DataItem, "INSU", ObjectDic.Instance.GetObjectName("ACQUIRER_SENDER"));
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgLine, 1);
            SetGridCboItem_EQP(dgLine.Columns["LINE_ID"], "LINE");
        }

        private bool SetGridCboItem_EQP(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ONLY_X", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["ONLY_X"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);

                    //(col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = dtResult.Copy().AsDataView();
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowRemove(dgLine);
        }

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows[0].Index].Delete();
                    dt.AcceptChanges();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgLine.Rows.Count < 1)
                {
                    Util.MessageInfo("FM_ME_0278");  //LINE 정보는 1개이상 입력해주세요.
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("OP_DATE", typeof(string));
                dtRqst.Columns.Add("DIARY_CLASS_CD", typeof(string));
                dtRqst.Columns.Add("SHIFT_ID_1", typeof(string));
                dtRqst.Columns.Add("SHIFT_USER_NAME_1", typeof(string));
                dtRqst.Columns.Add("SHIFT_ID_2", typeof(string));
                dtRqst.Columns.Add("SHIFT_USER_NAME_2", typeof(string));
                dtRqst.Columns.Add("SHIFT_ID_3", typeof(string));
                dtRqst.Columns.Add("SHIFT_USER_NAME_3", typeof(string));
                dtRqst.Columns.Add("DIARY_PIN_CONTENTS_1", typeof(string));
                dtRqst.Columns.Add("DIARY_CONTENTS_1", typeof(string));
                dtRqst.Columns.Add("DIARY_FORMA_TEMPERATURE_1", typeof(string));
                dtRqst.Columns.Add("DIARY_AGING_TEMPERATURE_1", typeof(string));
                dtRqst.Columns.Add("DIARY_PIN_CONTENTS_2", typeof(string));
                dtRqst.Columns.Add("DIARY_CONTENTS_2", typeof(string));
                dtRqst.Columns.Add("DIARY_FORMA_TEMPERATURE_2", typeof(double));
                dtRqst.Columns.Add("DIARY_AGING_TEMPERATURE_2", typeof(double));
                dtRqst.Columns.Add("DIARY_PIN_CONTENTS_3", typeof(string));
                dtRqst.Columns.Add("DIARY_CONTENTS_3", typeof(string));
                dtRqst.Columns.Add("DIARY_FORMA_TEMPERATURE_3", typeof(double));
                dtRqst.Columns.Add("DIARY_AGING_TEMPERATURE_3", typeof(double));
                dtRqst.Columns.Add("LINE_ID", typeof(string));
                dtRqst.Columns.Add("ROUTE_ID", typeof(string));
                dtRqst.Columns.Add("LOT_INFO", typeof(string));
                dtRqst.Columns.Add("MDF_ID", typeof(string));
                dtRqst.Columns.Add("INPUT_CELL_CNT", typeof(Int16));
                dtRqst.Columns.Add("OCV_BAD_CNT", typeof(Int16));
                dtRqst.Columns.Add("LB_CNT_1", typeof(Int16));
                dtRqst.Columns.Add("CB_CNT_1", typeof(Int16));
                dtRqst.Columns.Add("MLB_CNT_1", typeof(Int16));
                dtRqst.Columns.Add("LB_CNT_2", typeof(Int16));
                dtRqst.Columns.Add("CB_CNT_2", typeof(Int16));
                dtRqst.Columns.Add("MLB_CNT_2", typeof(Int16));
                dtRqst.Columns.Add("LB_CNT_3", typeof(Int16));
                dtRqst.Columns.Add("CB_CNT_3", typeof(Int16));
                dtRqst.Columns.Add("MLB_CNT_3", typeof(Int16));
                DataRow dr = null;

                for (int i = 1; i < dgLine.Rows.Count; i++)
                {
                    dr = dtRqst.NewRow();

                    dr["OP_DATE"] = dtpWorkDate.SelectedDateTime.ToString("yyyyMMdd");
                    dr["DIARY_CLASS_CD"] = "1"; //인수인계일지 : 1



                    dr["SHIFT_ID_1"] = dgTeam.GetCell(0, 0).Presenter.Tag;
                    dr["SHIFT_USER_NAME_1"] = Util.NVC(DataTableConverter.GetValue(dgUser.Rows[0].DataItem, "INSU"));
                    dr["SHIFT_ID_2"] = dgTeam.Columns[0].Header; // fpsTeam.ActiveSheet.RowHeader.Cells[1, 0].Tag;
                    dr["SHIFT_USER_NAME_2"] = Util.NVC(dgUser.GetCell(0, 1).Text); // fpsUser.ActiveSheet.Cells[0, 1].Text;
                    dr["SHIFT_ID_3"] = dgTeam.Columns[0].Header; // fpsTeam.ActiveSheet.RowHeader.Cells[2, 0].Tag;
                    dr["SHIFT_USER_NAME_3"] = Util.NVC(dgUser.GetCell(0, 2).Text); // fpsUser.ActiveSheet.Cells[0, 2].Text;

                    dr["DIARY_PIN_CONTENTS_1"] = Util.NVC(dgTeam.GetCell(0, 0).Text); // fpsTeam.ActiveSheet.Cells[0, 0].Text;
                    dr["DIARY_CONTENTS_1"] = Util.NVC(dgTeam.GetCell(0, 1).Text); // fpsTeam.ActiveSheet.Cells[0, 1].Text;
                    dr["DIARY_FORMA_TEMPERATURE_1"] = double.Parse(string.IsNullOrEmpty(dgTeam.GetCell(0, 3).Text) ? "0" : dgTeam.GetCell(0, 3).Text); // double.Parse(string.IsNullOrEmpty(fpsTeam.ActiveSheet.Cells[0, 2].Text) ? "0" : fpsTeam.ActiveSheet.Cells[0, 2].Text);
                    dr["DIARY_AGING_TEMPERATURE_1"] = double.Parse(string.IsNullOrEmpty(dgTeam.GetCell(0, 4).Text) ? "0" : dgTeam.GetCell(0, 4).Text); // double.Parse(string.IsNullOrEmpty(fpsTeam.ActiveSheet.Cells[0, 3].Text) ? "0" : fpsTeam.ActiveSheet.Cells[0, 3].Text);

                    dr["DIARY_PIN_CONTENTS_2"] = dgTeam.GetCell(1, 0).Text; // fpsTeam.ActiveSheet.Cells[1, 0].Text;
                    dr["DIARY_CONTENTS_2"] = dgTeam.GetCell(1, 1).Text; // fpsTeam.ActiveSheet.Cells[1, 1].Text;
                    dr["DIARY_FORMA_TEMPERATURE_2"] = double.Parse(string.IsNullOrEmpty(dgTeam.GetCell(1, 3).Text) ? "0" : dgTeam.GetCell(1, 3).Text); // double.Parse(string.IsNullOrEmpty(fpsTeam.ActiveSheet.Cells[1, 2].Text) ? "0" : fpsTeam.ActiveSheet.Cells[1, 2].Text);
                    dr["DIARY_AGING_TEMPERATURE_2"] = double.Parse(string.IsNullOrEmpty(dgTeam.GetCell(1, 4).Text) ? "0" : dgTeam.GetCell(1, 4).Text); // double.Parse(string.IsNullOrEmpty(fpsTeam.ActiveSheet.Cells[1, 3].Text) ? "0" : fpsTeam.ActiveSheet.Cells[1, 3].Text);

                    dr["DIARY_PIN_CONTENTS_3"] = dgTeam.GetCell(2, 0).Text; // fpsTeam.ActiveSheet.Cells[2, 0].Text;
                    dr["DIARY_CONTENTS_3"] = dgTeam.GetCell(2, 1).Text; // fpsTeam.ActiveSheet.Cells[2, 1].Text;
                    dr["DIARY_FORMA_TEMPERATURE_3"] = double.Parse(string.IsNullOrEmpty(dgTeam.GetCell(2, 2).Text) ? "0" : dgTeam.GetCell(2, 2).Text); // double.Parse(string.IsNullOrEmpty(fpsTeam.ActiveSheet.Cells[2, 2].Text) ? "0" : fpsTeam.ActiveSheet.Cells[2, 2].Text);
                    dr["DIARY_AGING_TEMPERATURE_3"] = double.Parse(string.IsNullOrEmpty(dgTeam.GetCell(2, 3).Text) ? "0" : dgTeam.GetCell(2, 3).Text); // double.Parse(string.IsNullOrEmpty(fpsTeam.ActiveSheet.Cells[2, 3].Text) ? "0" : fpsTeam.ActiveSheet.Cells[2, 3].Text);
                    //Cell a = fpsLine.ActiveSheet.Cells[i, 0];
                    dr["LINE_ID"] = dgLine.GetCell(i, 0).Value; // fpsLine.ActiveSheet.Cells[i, 0].Value;
                    dr["ROUTE_ID"] = dgLine.GetCell(i, 1).Text; // fpsLine.ActiveSheet.Cells[i, 1].Text;
                    dr["LOT_INFO"] = dgLine.GetCell(i, 2).Text; // fpsLine.ActiveSheet.Cells[i, 2].Text;
                    dr["MDF_ID"] = LoginInfo.USERID;
                    dr["INPUT_CELL_CNT"] = 0;
                    dr["OCV_BAD_CNT"] = 0;
                    dr["LB_CNT_1"] = 0;
                    dr["CB_CNT_1"] = 0;
                    dr["MLB_CNT_1"] = 0;
                    dr["LB_CNT_2"] = 0;
                    dr["CB_CNT_2"] = 0;
                    dr["MLB_CNT_2"] = 0;
                    dr["LB_CNT_3"] = 0;
                    dr["CB_CNT_3"] = 0;
                    dr["MLB_CNT_3"] = 0;
                    dtRqst.Rows.Add(dr);
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SET_SHIFT_DIARY_INSU", "INDATA", "OUTDATA", dtRqst);
                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetINSUList();
        }

        private void GetINSUList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("DIARY_CLASS_CD", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["DIARY_CLASS_CD"] = "1"; //인수인계일지 : 1
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIFT_INSU_BY_PERIOD", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
