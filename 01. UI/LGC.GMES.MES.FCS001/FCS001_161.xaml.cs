/*************************************************************************************
 Created Date : 2023.05.22
      Creator : LJE
   Decription : 샘플 포트 설정
----------------------------------------------------------------------------------------------------------------------
 [Change History]
  2023.05.22  NAME  : Initial Created.
 **************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_161 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_161()
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

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter0 = { "C" };
            _combo.SetCombo(cboDest, CommonCombo_Form.ComboStatus.ALL, sCase: "CNVR_LOCATION", sFilter : sFilter0); 
            string[] sFilter1 = { "USE_FLAG" };
            _combo.SetCombo(cboUseFlag, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter1);
            SetEqptGrTypeCode(cboProc);
            SetLaneCode(cboLane);

        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            if (!CheckButtonPermissionGroupByBtnGroupID("DELETE_INFORMATION_W", "FCS001_161"))
                btnDelete.Visibility = Visibility.Hidden;

            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnUnitPlus_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgList, 1);
            DataTableConverter.SetValue(dgList.Rows[dgList.Rows.Count - 1].DataItem, "FLAG", "Y");
            DataTableConverter.SetValue(dgList.Rows[dgList.Rows.Count - 1].DataItem, "NEW_FLAG", "Y"); //새로운 Row에 대해서만 ReadOnly 이벤트 적용하기 위함
            DataTableConverter.SetValue(dgList.Rows[dgList.Rows.Count - 1].DataItem, "CHK", true);
            LocationSetGridCbo(dgList.Columns["CNVR_LOCATION_ID"], "CNVR_LOCATION_ID");
            LaneSetGridCbo(dgList.Columns["LANE_ID"], "LANE_ID");
            EqptGrTypeSetGridCbo(dgList.Columns["EQPT_GR_TYPE_CODE"], "EQPT_GR_TYPE_CODE");

        }

        private void btnUnitMinus_Click(object sender, RoutedEventArgs e)
        {
            string flag = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.Rows.Count - 1].DataItem, "FLAG"));
            if (flag.Equals("Y")) DataGridRowRemove(dgList);
        }

    
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool checkOK = false;
                checkOK = GetRangeCheck();
                if(checkOK == false ) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("INSUSER", typeof(string));
                dtRqst.Columns.Add("INSDTTM", typeof(DateTime));
                dtRqst.Columns.Add("UPDUSER", typeof(string));
                dtRqst.Columns.Add("UPDDTTM", typeof(DateTime));

                DataRow dr = null;
                for (int i = 0; i < dgList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                    {
                        dr = dtRqst.NewRow();
                        dr["EQPT_GR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPT_GR_TYPE_CODE"));
                        dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LANE_ID"));
                        dr["CNVR_LOCATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                        dr["USE_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "USE_FLAG"));
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["INSDTTM"] = System.DateTime.Now;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["UPDDTTM"] = System.DateTime.Now;
                        dtRqst.Rows.Add(dr);
                    }
                }

                if (dtRqst.Rows.Count == 0) return;

                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_INS_SMPL_ISS_LOCATION_UI", "INDATA", "OUTDATA", dtRqst);

                        Util.MessageValidation("FM_ME_0215");  //저장하였습니다.
                        GetList();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dgTrayType_CommittedEdit(object sender, DataGridCellEventArgs e)
        {

            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            int row = e.Cell.Row.Index;

            if (e.Cell.Column.Name.Equals("CHK"))
            {
                return;
            }

            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "FLAG", "Y");
            DataTableConverter.SetValue(datagrid.Rows[row].DataItem, "CHK", true);

            datagrid.Refresh();
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

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                e.Cell.Presenter.BorderBrush = new SolidColorBrush(Colors.Black);

                e.Cell.Presenter.BorderBrush = Brushes.LightGray;
                e.Cell.Presenter.BorderThickness = new Thickness(0.5, 0, 0, 0.5);

            }));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //선택한 데이터를 삭제하시겠습니까?
                Util.MessageConfirm("FM_ME_0167", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));
                        dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                        dtRqst.Columns.Add("LANE_ID", typeof(string));
                        dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE")
                                && Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "NEW_FLAG")).ToUpper().Equals("N"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["PROCESS_TYPE"] = "D";
                                dr["EQPT_GR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPT_GR_TYPE_CODE"));
                                dr["LANE_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LANE_ID"));
                                dr["CNVR_LOCATION_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CNVR_LOCATION_ID"));
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }

                        //선택된 row가 0일경우 return
                        if (dtRqst.Rows.Count == 0)
                            return;

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_SMPL_ISS_LOCATION_UI", "INDATA", "OUTDATA", dtRqst);

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0")) //0성공, -1실패
                        {
                            Util.MessageValidation("FM_ME_0154");  //삭제하였습니다.
                        }
                        else
                        {
                            Util.MessageValidation("FM_ME_0153");  //삭제 실패하였습니다.
                        }

                        GetList();
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgList);
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgList);
        }

        private void dgOutStationList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgTrayList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            int _row = 0;
            int _col = 0;
            int _top_cnt = 0;
            string value = string.Empty;
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

            if (value.Length == 0)
            {
                return;
            }

            //수정

        }
        #endregion

        #region Method
        private void GetList() 
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USE_FLAG"] = Util.GetCondition(cboUseFlag, bAllNull: true);
                dr["CNVR_LOCATION_ID"] = Util.GetCondition(cboDest, bAllNull: true);
                dr["EQPT_GR_TYPE_CODE"] = Util.GetCondition(cboProc, bAllNull: true);
                dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SMPL_ISS_LOCATION_UI", "INDATA", "OUTDATA", dtRqst);

                dtRslt.Columns.Add("CHK", typeof(bool));
                //dtRslt.Columns.Add("FLAG", typeof(string));

                if (dtRslt.Rows.Count > 0)
                {
                    dtRslt.Columns.Add("FLAG", typeof(string));
                    dtRslt.Columns.Add("NEW_FLAG", typeof(string));

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        dtRslt.Rows[i]["FLAG"] = "N";
                        dtRslt.Rows[i]["NEW_FLAG"] = "N";
                    }

                    //Grid Combo Setting
                    SetGridCboItem(dgList.Columns["USE_FLAG"], "USE_FLAG");
                    LocationSetGridCbo(dgList.Columns["CNVR_LOCATION_ID"], "CNVR_LOCATION_ID");
                    LaneSetGridCbo(dgList.Columns["LANE_ID"], "LANE_ID");
                    EqptGrTypeSetGridCbo(dgList.Columns["EQPT_GR_TYPE_CODE"], "EQPT_GR_TYPE_CODE");

                }

                Util.GridSetData(dgList, dtRslt, this.FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                if (dg.Rows.Count == 0)
                {
                    dt.Columns.Add("FLAG", typeof(string));
                    dt.Columns.Add("NEW_FLAG", typeof(string));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);

                        if (dg.Rows.Count == 0)
                        {
                            dt.Columns.Add("FLAG", typeof(string));
                            dt.Columns.Add("NEW_FLAG", typeof(string));
                        }

                        DataRow dr = dt.NewRow();
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER_NAME"] = LoginInfo.USERNAME;
                        dr["UPDUSER_NAME"] = LoginInfo.USERNAME;
                        dr["UPDDTTM"] = System.DateTime.Now;
                        dr["INSDTTM"] = System.DateTime.Now;
                        dr["USE_FLAG"] = "Y";
                        dr["FLAG"] = "N";
                        dr["NEW_FLAG"] = "N";
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
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["INSUSER_NAME"] = LoginInfo.USERNAME;
                        dr["UPDUSER_NAME"] = LoginInfo.USERNAME;
                        dr["UPDDTTM"] = System.DateTime.Now;
                        dr["INSDTTM"] = System.DateTime.Now;
                        dr["USE_FLAG"] = "Y";
                        dr["FLAG"] = "N";
                        dr["NEW_FLAG"] = "N";
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

        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID, string sFormID)
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = sFormID;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_FORM", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    //if (sBtnGrpID == "SCRAP_PROC_W")
                    //    objectmessage = ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL");

                    //Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetTrayTypeCode(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CNVR_LOCATION_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CNVR_LOCATION_ID"] = Util.GetCondition(cboDest, bAllNull: true);
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_PORT_RCV_ENABLE_TRAY_TYPE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr[sDisplay] = "-ALL-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);
            return dt;
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private void SetEqptGrTypeCode(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPT_GR_TYPE_CODE_UI";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetLaneCode(C1ComboBox cbo, bool bCodeDisplay = true)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;


                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_SMPL_ISS_LOCATION_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(SetCodeDisplay(dtResult, bCodeDisplay), "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private bool LocationSetGridCbo(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPT_GR_TYPE_CODE"] = "C";
                dr["USE_FLAG"] = "Y";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CNVR_LOCATION", "RQSTDT", "RSLTDT", RQSTDT);

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

        private bool LaneSetGridCbo(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LANE_SMPL_ISS_LOCATION_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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

        private bool EqptGrTypeSetGridCbo(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "EQPT_GR_TYPE_CODE_UI";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

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


        private void dbList_BeginningRowEdit(object sender, DataGridEditingRowEventArgs e)
        {
            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[e.Row.Index].DataItem, "NEW_FLAG")).ToUpper().Equals("N")
                && (e.Row.DataGrid.CurrentCell.Column.Name.Equals("LANE_ID") || e.Row.DataGrid.CurrentCell.Column.Name.Equals("EQPT_GR_TYPE_CODE"))){ 

                e.Cancel = true;
            }
            
        }

        private bool GetRangeCheck() //Validation
        {
            bool rtn = false;

            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPT_GR_TYPE_CODE")).ToString().Equals("") ||
                       string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPT_GR_TYPE_CODE")).ToString()))
                    {
                        Util.Alert("FM_ME_0107"); //공정을 선택해주세요
                        return rtn;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LANE_ID")).ToString().Equals("") ||
                       string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LANE_ID")).ToString()))
                    {
                        Util.Alert("SFU4986"); //선택된 Lane이 없습니다
                        return rtn;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CNVR_LOCATION_ID")).ToString().Equals("") ||
                       string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "CNVR_LOCATION_ID")).ToString()))
                    {
                        Util.Alert("SFU7024"); //목적지 정보가 없습니다.
                        return rtn;
                    }


                }
            }


            return true;
        }
    }
}
