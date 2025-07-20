/*************************************************************************************
 Created Date : 2018.10.22
      Creator : 신광희 차장
   Decription : CWA 전극 PORT 설정 변경 팝업
--------------------------------------------------------------------------------------
  
**************************************************************************************/
using System;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Windows;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_001_PANCAKE_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_011_PORT_SETUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }
        private string _equipmentCode;
        #endregion

        public MCS001_011_PORT_SETUP()
        {
            InitializeComponent();
        }

        #region Initialize 
        private void InitializeControls()
        {
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            // Stocker 콤보박스
            SetStockerCombo(cboStocker);

            // Port 출고 그리드(Roll-Press, Coater 방향) 셀의 상태 콤보
            SetDataGridPortStateCombo(dgPortRollPress.Columns["CURR_INOUT_TYPE_CODE"], CommonCombo.ComboStatus.NONE);
            SetDataGridPortStateCombo(dgPortCoater.Columns["CURR_INOUT_TYPE_CODE"], CommonCombo.ComboStatus.NONE);
            SetDataGridPortWorkModeCombo(dgPortRollPress.Columns["TO_PORT_WRK_MODE"], CommonCombo.ComboStatus.NONE);
            SetDataGridPortWorkModeCombo(dgPortCoater.Columns["FROM_PORT_WRK_MODE"], CommonCombo.ComboStatus.NONE);
            
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();

            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                DataRow drRackInfo = parameters[1] as DataRow;

                if (drRackInfo != null)
                    SetRackInfo(drRackInfo);

            }

            // Stocker 정보 Setting
            if (!string.IsNullOrEmpty(_equipmentCode))
            {
                for (int i = 0; i < cboStocker.Items.Count; i++)
                {
                    if (string.Equals(_equipmentCode, ((DataRowView)cboStocker.Items[i]).Row.ItemArray[0].ToString()))
                    {
                        cboStocker.SelectedIndex = i;
                        cboStocker.IsEnabled = false;
                        break;
                    }
                }
            }

            SelectJumboRollPort();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            dgPortRollPress.EndEdit();
            dgPortRollPress.EndEditRow(true);
            dgPortCoater.EndEdit();
            dgPortCoater.EndEditRow(true);

            if (!ValidationSavePortSetup()) return;

            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SavePortSetUp();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void dgPortRollPress_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgPortRollPress.CurrentRow.DataItem as DataRowView;
                if (drv == null) return;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPortCoater_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e?.Row?.DataItem == null || e.Column == null)
                    return;

                if (e.Column.Name.Equals("PORT_WRK_MODE"))
                {
                    e.Cancel = true;
                }
                

                if (e.Column.Name.Equals("FROM_PORT_WRK_MODE"))
                {
                    if (string.IsNullOrEmpty(DataTableConverter.GetValue(e.Row.DataItem, "FROM_PORT_ID").GetString()))
                    {
                        e.Cancel = true;
                    }
                }
                    //if (string.IsNullOrEmpty(DataTableConverter.GetValue(e.Row.DataItem, "FROM_PORT_ID").GetString()))
                    //{
                    //    e.Cancel = true;
                    //}

                    if (e.Column.Name.Equals("CURR_INOUT_TYPE_CODE"))
                {
                    if (DataTableConverter.GetValue(e.Row.DataItem, "INOUT_TYPE_CODE").Equals("IN"))
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPortCoater_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                DataRowView drv = dgPortCoater.CurrentRow.DataItem as DataRowView;
                if (drv == null) return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void SavePortSetUp()
        {
            try
            {

                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_REG_CURR_PORT_STAT";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("PORT_ID", typeof(string));
                inDataTable.Columns.Add("PORT_STAT_CODE", typeof(string));
                inDataTable.Columns.Add("MCS_CST_ID", typeof(string));
                inDataTable.Columns.Add("MTRL_EXIST_FLAG", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));
                inDataTable.Columns.Add("CURR_INOUT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("PORT_WRK_MODE", typeof(string));

                foreach (DataGridRow row in dgPortRollPress.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added || ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                        {
                            DataRow dr = inDataTable.NewRow();
                            dr["PORT_ID"] = DataTableConverter.GetValue(row.DataItem, "PORT_ID");
                            //dr["PORT_STAT_CODE"] = DataTableConverter.GetValue(row.DataItem, "PORT_STAT_CODE");
                            dr["PORT_STAT_CODE"] = null;
                            dr["MTRL_EXIST_FLAG"] = DataTableConverter.GetValue(row.DataItem, "MTRL_EXIST_FLAG");
                            dr["UPDUSER"] = LoginInfo.USERID;
                            dr["CURR_INOUT_TYPE_CODE"] = DataTableConverter.GetValue(row.DataItem, "CURR_INOUT_TYPE_CODE");
                            dr["PORT_WRK_MODE"] = DataTableConverter.GetValue(row.DataItem, "PORT_WRK_MODE");
                            inDataTable.Rows.Add(dr);
                        }
                    }
                }

                foreach (DataGridRow row in dgPortCoater.Rows)
                {
                    if (row.Type == DataGridRowType.Item)
                    {
                        if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added || ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                        {
                            DataRow dr = inDataTable.NewRow();
                            dr["PORT_ID"] = DataTableConverter.GetValue(row.DataItem, "PORT_ID");
                            //dr["PORT_STAT_CODE"] = DataTableConverter.GetValue(row.DataItem, "PORT_STAT_CODE");
                            dr["PORT_STAT_CODE"] = null;
                            dr["MTRL_EXIST_FLAG"] = DataTableConverter.GetValue(row.DataItem, "MTRL_EXIST_FLAG");
                            dr["UPDUSER"] = LoginInfo.USERID;
                            dr["CURR_INOUT_TYPE_CODE"] = DataTableConverter.GetValue(row.DataItem, "CURR_INOUT_TYPE_CODE");
                            dr["PORT_WRK_MODE"] = DataTableConverter.GetValue(row.DataItem, "PORT_WRK_MODE");
                            inDataTable.Rows.Add(dr);
                        }
                    }
                }


                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        // 포트작업모드 UPDATE 
                        DataTable inTable = new DataTable("INDATA");
                        inTable.Columns.Add("PORT_ID", typeof(string));
                        inTable.Columns.Add("PORT_STAT_CODE", typeof(string));
                        inTable.Columns.Add("MCS_CST_ID", typeof(string));
                        inTable.Columns.Add("MTRL_EXIST_FLAG", typeof(string));
                        inTable.Columns.Add("UPDUSER", typeof(string));
                        inTable.Columns.Add("CURR_INOUT_TYPE_CODE", typeof(string));
                        inTable.Columns.Add("PORT_WRK_MODE", typeof(string));


                        foreach (DataGridRow row in dgPortRollPress.Rows)
                        {
                            if (row.Type == DataGridRowType.Item)
                            {
                                if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added || ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                                {
                                    DataRow dr = inTable.NewRow();
                                    dr["PORT_ID"] = DataTableConverter.GetValue(row.DataItem, "TO_PORT_ID");
                                    //dr["PORT_STAT_CODE"] = DataTableConverter.GetValue(row.DataItem, "PORT_STAT_CODE");
                                    dr["PORT_STAT_CODE"] = null;
                                    dr["MTRL_EXIST_FLAG"] = DataTableConverter.GetValue(row.DataItem, "TO_MTRL_EXIST_FLAG");
                                    dr["UPDUSER"] = LoginInfo.USERID;
                                    dr["CURR_INOUT_TYPE_CODE"] = DataTableConverter.GetValue(row.DataItem, "CURR_INOUT_TYPE_CODE");
                                    dr["PORT_WRK_MODE"] = DataTableConverter.GetValue(row.DataItem, "TO_PORT_WRK_MODE");
                                    inTable.Rows.Add(dr);
                                }
                            }
                        }

                        foreach (DataGridRow row in dgPortCoater.Rows)
                        {
                            if (row.Type == DataGridRowType.Item)
                            {
                                if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added || ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                                {
                                    DataRow dr = inTable.NewRow();
                                    dr["PORT_ID"] = DataTableConverter.GetValue(row.DataItem, "FROM_PORT_ID");
                                    //dr["PORT_STAT_CODE"] = DataTableConverter.GetValue(row.DataItem, "PORT_STAT_CODE");
                                    dr["PORT_STAT_CODE"] = null;
                                    dr["MTRL_EXIST_FLAG"] = DataTableConverter.GetValue(row.DataItem, "FROM_MTRL_EXIST_FLAG");
                                    dr["UPDUSER"] = LoginInfo.USERID;
                                    dr["CURR_INOUT_TYPE_CODE"] = DataTableConverter.GetValue(row.DataItem, "CURR_INOUT_TYPE_CODE");
                                    dr["PORT_WRK_MODE"] = DataTableConverter.GetValue(row.DataItem, "FROM_PORT_WRK_MODE");
                                    inTable.Rows.Add(dr);
                                }
                            }
                        }

                        new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizEx) =>
                        {
                            HiddenLoadingIndicator();
                            if (bizEx != null)
                            {
                                HiddenLoadingIndicator();
                                Util.MessageException(bizEx);
                                return;
                            }

                            Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                            DialogResult = MessageBoxResult.OK;
                        });

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region Mehod


        private void SetRackInfo(DataRow drRackInfo)
        {
            //txtRackId.Text = drRackInfo["RACK_ID"].GetString();
            //txtProjectName.Text = drRackInfo["PRJT_NAME"].GetString();
            //txtProductId.Text = drRackInfo["PRODID"].GetString();
            //txtProductName.Text = drRackInfo["PRODNAME"].GetString();
            //txtStartTime.Text = drRackInfo["WH_RCV_DTTM"].GetString();
            //txtLotId.Text = drRackInfo["LOTID"].GetString();
            //txtQty.Text = drRackInfo["WIPQTY"].GetString();
            //txtvalidDt.Text = drRackInfo["VLD_DATE"].GetString();
            //txtPastDay.Text = drRackInfo["PAST_DAY"].GetString();
            //txtWipHold.Text = drRackInfo["WIPHOLD"].GetString();
        }

        private void SelectJumboRollPort()
        {
            SelectJumboRollPortRollPress();
            SelectJumboRollPortCoater();
        }

        private void SelectJumboRollPortRollPress()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_PORT_ROLLPRESS";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgPortRollPress, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SelectJumboRollPortCoater()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_PORT_COATER";
                DataTable inDataTable = new DataTable("RQSTDT");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    //Util.GridSetData(dgPortCoater, bizResult, null, true);
                    Util.GridSetData(dgPortCoater, Util.CheckBoxColumnAddTable(bizResult, false), null, true);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private static void SetStockerCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_MCS_WAREHOUSE_CBO";
            string[] arrColumn = { "LANGID", "SHOPID", "EQGRID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, "JRW", LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetDataGridPortStateCombo(DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "CMCODE"};
            string[] arrCondition = { LoginInfo.LANGID, "MCS_CURR_INOUT_TYPE_CODE", null };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
        }

        private void SetDataGridPortWorkModeCombo(DataGridColumn dgcol, CommonCombo.ComboStatus status)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMONCODE_TBL";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "CMCODE" };
            string[] arrCondition = { LoginInfo.LANGID, "PORT_WRK_MODE", null };
            string selectedValueText = dgcol.SelectedValuePath();
            string displayMemberText = dgcol.DisplayMemberPath();
            CommonCombo.SetDataGridComboItem(bizRuleName, arrColumn, arrCondition, status, dgcol, selectedValueText, displayMemberText);
        }

        private bool ValidationSavePortSetup()
        {
            int modifiedCount = 0;

            const string bizRuleName = "DA_MCS_SEL_LOGIS_CMD_CHK_PORT";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("PORT_ID", typeof(string));

            foreach (DataGridRow row in dgPortRollPress.Rows)
            {
                if (row.Type == DataGridRowType.Item)
                {
                    if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added ||
                        ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                    {
                        modifiedCount++;

                        DataRow dr = inDataTable.NewRow();
                        dr["PORT_ID"] = DataTableConverter.GetValue(row.DataItem, "PORT_ID");
                        inDataTable.Rows.Add(dr);
                    }
                }
            }

            foreach (DataGridRow row in dgPortCoater.Rows)
            {
                if (row.Type == DataGridRowType.Item)
                {
                    if (((DataRowView)row.DataItem).Row.RowState == DataRowState.Added ||
                        ((DataRowView)row.DataItem).Row.RowState == DataRowState.Modified)
                    {
                        modifiedCount++;

                        DataRow dr = inDataTable.NewRow();
                        dr["PORT_ID"] = DataTableConverter.GetValue(row.DataItem, "PORT_ID");
                        inDataTable.Rows.Add(dr);
                    }
                }
            }

            if (modifiedCount < 1)
            {
                Util.MessageValidation("SFU1566");
                return false;
            }

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            if (searchResult.Rows.Count > 0)
            {   //명령 목록에서 FROM PORT, TO  PORT 정보가 중복으로 존재 합니다.
                Util.MessageValidation("SFU5058");
                return false;
            }


            return true;
        }


        #endregion


    }
}
