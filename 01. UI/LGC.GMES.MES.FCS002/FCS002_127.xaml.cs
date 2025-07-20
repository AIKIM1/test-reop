/*************************************************************************************
 Created Date : 2021.12.20
      Creator : 이정미
   Decription : 컨베이어 명령 조건 정보 관리
--------------------------------------------------------------------------------------
 [Change History]
  2021.12.20 NAME   : Initial Created.
  2022.06.27 이정미 : SetGridCboItem 함수 호출 Biz 변경, 디자인 변경 
  2022.06.28 이정미 : 저장함수 변경 - 저장 확인 메세지 추가
  2022.07.11 조영대 : BCR 위치 콤보 변경 시 조회
  2022.09.01 최도훈 : Column 개수가 0인 경우 예외처리 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_127 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        DataTable dtHeader = new DataTable();
        private string port_id ="";
        private string seqno = "";
        public FCS002_127()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            _combo.SetCombo(cboEqpGrp, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "CNVR_GRP");
            string[] sFilter1 = { "CNVR_LOGIC_TYPE_CODE" };
            _combo.SetCombo(cboStepType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter1);
            string[] sFilter2 = { "USE_FLAG" };
            _combo.SetCombo(cboUseFlag, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "CMN", sFilter: sFilter2);

            SetGridCboItem(dgCMD.Columns["CNVR_LOGIC_TYPE_CODE"], "CNVR_LOGIC_TYPE_CODE");
            SetGridCboItem(dgCMD.Columns["USE_FLAG"], "USE_FLAG");
            SetGridCboItem(dgCMDCOND.Columns["CNVR_LOGIC_COND_TYPE_CODE"], "CNVR_LOGIC_COND_TYPE_CODE");
            SetGridCboItem(dgCMDCOND.Columns["CNVR_LOGIC_COND_RLSHP_CODE"], "CNVR_LOGIC_COND_RLSHP_CODE");
        }
        #endregion

        #region Method
     
        private void GetList()
        {
            try
            {
                Util.gridClear(dgCMD);
                
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQP_GRP", typeof(string));
                dtRqst.Columns.Add("PORT_ID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
               

                DataRow dr = dtRqst.NewRow();
                dr["EQP_GRP"] = Util.GetCondition(cboEqpGrp, bAllNull: true);
                dr["PORT_ID"] = Util.GetCondition(cboPortId, bAllNull: true);
                dr["CNVR_LOGIC_TYPE_CODE"] = Util.GetCondition(cboStepType, bAllNull: true);
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CNVR_TRF_CMD_UI", "RQSTDT", "RSLTDT", dtRqst);

                dtRslt.Columns.Add("FLAG", typeof(string));

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    dtRslt.Rows[i]["FLAG"] = "N";
                }

                Util.GridSetData(dgCMD, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListData(string port_id, string seqno)
        {
            try
            {
               Util.gridClear(dgCMDCOND);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PORT_ID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_SEQNO", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["PORT_ID"] = port_id;
                dr["CNVR_LOGIC_SEQNO"] = seqno;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CNVR_TRF_CMD_COND_UI_MB", "RQSTDT", "RSLTDT", dtRqst);    

                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    dtRslt.Columns.Add("FLAG", typeof(string));

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["FLAG"] = "N";
                    }

                }
                Util.GridSetData(dgCMDCOND, dtRslt, FrameOperation, true);

            }
           catch(Exception ex)
            { 
                Util.MessageException(ex);
            }
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                string tag = Util.NVC(e.Cell.Column.Tag);

                if (!string.IsNullOrEmpty(tag))
                {
                    if (tag.Equals("A"))
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    else e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                if (e.Cell.Row.Index == dataGrid.Rows.Count() - 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));
        }

        private void CmdDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if(dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0 && !dt.Columns.Contains("FLAG"))
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["USE_FLAG"] = "Y";
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["EQP_GRP"] = Util.GetCondition(cboEqpGrp, bAllNull: true);
                        dt.Rows.Add(dr);
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
                        dr["USE_FLAG"] = "Y";
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["EQP_GRP"] = Util.GetCondition(cboEqpGrp, bAllNull: true);
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

        private void CmdCondDataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                if (port_id.Equals("") || seqno.Equals(""))
                {
                    return;
                }

                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.Rows.Count == 0)
                    dt.Columns.Add("FLAG", typeof(string));

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0 && !dt.Columns.Contains("FLAG"))
                            dt.Columns.Add("FLAG", typeof(string));

                        DataRow dr = dt.NewRow();
                        dr["PORT_ID"] = port_id;
                        dr["CNVR_LOGIC_SEQNO"] = seqno;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER"] = LoginInfo.USERID;
                        dt.Rows.Add(dr);
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
                        dr["PORT_ID"] = port_id;
                        dr["CNVR_LOGIC_SEQNO"] = seqno;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER"] = LoginInfo.USERID;
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

        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.GetRowCount() > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.Rows.Count - 1].Delete();
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

        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

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
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
            Util.gridClear(dgCMDCOND);
        }

        private void dgCMD_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgCMD.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgCMDCOND_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgCMDCOND.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void Cmd_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            CmdDataGridRowAdd(dgCMD, 1);
            SetGridCboItem(dgCMD.Columns["CNVR_LOGIC_TYPE_CODE"], "CNVR_LOGIC_TYPE_CODE");
            SetGridCboItem(dgCMD.Columns["USE_FLAG"], "USE_FLAG");

            if(dgCMD.Rows.Count > 0)
            {
                DataTableConverter.SetValue(dgCMD.Rows[dgCMD.Rows.Count - 1].DataItem, "FLAG", "Y");
            }
        }

        private void Cmd_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            if(dgCMD.Rows.Count > 0)
            {
                string flag = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[dgCMD.Rows.Count - 1].DataItem, "FLAG"));
                if (flag.Equals("Y")) DataGridRowRemove(dgCMD);
            }
        }

        private void CmdCond_btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            CmdCondDataGridRowAdd(dgCMDCOND, 1);
            SetGridCboItem(dgCMDCOND.Columns["CNVR_LOGIC_COND_TYPE_CODE"], "CNVR_LOGIC_COND_TYPE_CODE");
            SetGridCboItem(dgCMDCOND.Columns["CNVR_LOGIC_COND_RLSHP_CODE"], "CNVR_LOGIC_COND_RLSHP_CODE");

            if (dgCMDCOND.Rows.Count > 0)
            {
                DataTableConverter.SetValue(dgCMDCOND.Rows[dgCMDCOND.Rows.Count - 1].DataItem, "FLAG", "Y");
            }
        }

        private void CmdCond_btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            if (dgCMDCOND.Rows.Count > 0)
            {
                string flag = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[dgCMDCOND.Rows.Count - 1].DataItem, "FLAG"));
                if (flag.Equals("Y")) DataGridRowRemove(dgCMDCOND);
            }
        }

        private void Cmd_btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("PORT_ID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_SEQNO", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_RSLT_VALUE", typeof(string));
                dtRqst.Columns.Add("CNVR_TRF_CMD_CODE", typeof(string));
                dtRqst.Columns.Add("CNVR_TRF_CMD_DESC", typeof(string));
                dtRqst.Columns.Add("UNIT_PATH_END_PORT_ID", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));

                DataRow dr = null;
                for (int i = 0; i < dgCMD.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "FLAG")).Equals("Y"))
                    {
                        dr = dtRqst.NewRow();
                        dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "PORT_ID"));
                        dr["CNVR_LOGIC_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "CNVR_LOGIC_SEQNO"));
                        dr["CNVR_LOGIC_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "CNVR_LOGIC_TYPE_CODE"));
                        dr["CNVR_LOGIC_RSLT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "CNVR_LOGIC_RSLT_VALUE"));
                        dr["CNVR_TRF_CMD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "CNVR_TRF_CMD_CODE"));
                        dr["CNVR_TRF_CMD_DESC"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "CNVR_TRF_CMD_DESC"));
                        dr["UNIT_PATH_END_PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "UNIT_PATH_END_PORT_ID"));
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgCMD.Rows[i].DataItem, "USE_FLAG"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }

                }
                Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_TRF_CMD_UI", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
                    {
                        Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
                    }
                    GetList();
                    }   
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CmdCond_btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("PORT_ID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_SEQNO", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_COND_SEQNO", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_COND_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_COND_VALUE", typeof(string));
                dtRqst.Columns.Add("CNVR_LOGIC_COND_RLSHP_CODE", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));
                dtRqst.Columns.Add("UPDDTTM", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));

                DataRow dr = null;
                for (int i = 0; i < dgCMDCOND.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "FLAG")).Equals("Y"))
                    {
                        dr = dtRqst.NewRow();
                        dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "PORT_ID"));
                        dr["CNVR_LOGIC_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CNVR_LOGIC_SEQNO"));
                        dr["CNVR_LOGIC_COND_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CNVR_LOGIC_COND_SEQNO"));
                        dr["CNVR_LOGIC_COND_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CNVR_LOGIC_COND_TYPE_CODE"));
                        dr["CNVR_LOGIC_COND_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CNVR_LOGIC_COND_VALUE"));
                        dr["CNVR_LOGIC_COND_RLSHP_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CNVR_LOGIC_COND_RLSHP_CODE"));
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "UPDDTTM"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }

                }
                Util.MessageConfirm("FM_ME_0214", (result) => //저장하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_TRF_CMD_COND_UI", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
                    {
                        Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                    }
                    else
                    {
                        Util.MessageValidation("FM_ME_0213");  //저장 실패하였습니다.
                    }

                    GetListData(port_id, seqno);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "INDATA";
            dtRqst.Columns.Add("PORT_ID", typeof(string));
            dtRqst.Columns.Add("CNVR_LOGIC_SEQNO", typeof(string));
            dtRqst.Columns.Add("CNVR_LOGIC_COND_SEQNO", typeof(string));

            for (int i = 0; i < dgCMDCOND.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    DataRow dr = dtRqst.NewRow();
                    dr["PORT_ID"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "PORT_ID"));
                    dr["CNVR_LOGIC_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CNVR_LOGIC_SEQNO"));
                    dr["CNVR_LOGIC_COND_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgCMDCOND.Rows[i].DataItem, "CNVR_LOGIC_COND_SEQNO"));
                    dtRqst.Rows.Add(dr);
                }
            }

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CNVR_TRF_CMD_COND_USE_FLAG_UI", "INDATA", "OUTDATA", dtRqst);
            if (dtRslt.Rows[0]["RESULT"].ToString().Equals("0")) //0성공, -1실패
            {
                Util.MessageValidation("FM_ME_0154");  //삭제하였습니다.
            }
            else
            {
                Util.MessageValidation("FM_ME_0153");  //삭제 실패하였습니다.
            }

            GetListData(port_id, seqno);
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgCMDCOND);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgCMDCOND);
        }

        private void dgCMD_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }
            int row = cell.Row.Index;

            if (port_id.Equals(null) && seqno.Equals(null))
            {
                return;
            }

            port_id = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "PORT_ID"));
            seqno = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[row].DataItem, "CNVR_LOGIC_SEQNO")); //CNVR_LOGIC_SEQNO (순번)

            GetListData(port_id, seqno);
            txtEqp.Text = port_id + "-" + seqno;
        }

        private void cboEqpGrp_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            C1ComboBox[] cboCraneParent = { cboEqpGrp };
            _combo.SetComboObjParent(cboPortId, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "BCR_LOC", objParent: cboCraneParent);
        }

        private void dgCMD_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void dgCMDCOND_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
        }

        private void cboPortId_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch.PerformClick();
        }

        #endregion


    }
}
