/*************************************************************************************
 Created Date : 2023.05.16
      Creator : 
   Decription : 단적재 출고
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.16  조영대 : Initial Created.
  2024.02.22  조영대 : Rout/공정 탭 추가
  2025.06.20  이해령 : PI : 박영규 책임 Route 등록시 직행 제외 조건 삭제 요청
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using C1.WPF;
using C1.WPF.DataGrid;

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_149_DEV : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private DataTable dtLineCombo = null;
        private DataTable dtRouteCombo = null;
        private DataTable dtProcessCombo = null;
        private DataTable dtYn = null;

        private string saveEditValue = string.Empty;

        public FCS001_149_DEV()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            // Tab1
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            string[] arrColum = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboShipLoc.SetDataComboItem("DA_SEL_AUTO_STACK_SHIP_LOC", arrColum, arrCondition);


            // Tab2
            _combo.SetCombo(cboLineRP, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");
        }

        private void InitControl()
        {
            dtpWorkDate.SelectedFromDateTime = DateTime.Now;
            dtpWorkDate.SelectedToDateTime = DateTime.Now;
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                dgLoadShipStatus.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SHIP_LOC", typeof(string));

                DataRow drStatus = dtRqst.NewRow();
                drStatus["LANGID"] = LoginInfo.LANGID;
                drStatus["AREAID"] = LoginInfo.CFG_AREA_ID;
                drStatus["EQSGID"] = cboLine.GetBindValue();
                drStatus["MDLLOT_ID"] = cboModel.GetBindValue();
                drStatus["SHIP_LOC"] = cboShipLoc.GetBindValue();
                dtRqst.Rows.Add(drStatus);

                dgLoadShipStatus.ExecuteService("BR_GET_AUTO_STACK_SHIP_LIST_MULT", "INDATA", "OUTDATA,STACK_INFO", dtRqst, bindTableName: "STACK_INFO");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListHist()
        {
            try
            {
                dgLoadShipHist.ClearRows();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHIP_LOC", typeof(string));

                DataRow drStatus = dtRqst.NewRow();
                drStatus["LANGID"] = LoginInfo.LANGID;
                drStatus["AREAID"] = LoginInfo.CFG_AREA_ID;
                drStatus["EQSGID"] = cboLine.GetBindValue();
                drStatus["MDLLOT_ID"] = cboModel.GetBindValue();
                drStatus["FROM_DATE"] = dtpWorkDate.SelectedFromDateTime.ToString("yyyyMMddHHmmss");
                drStatus["TO_DATE"] = dtpWorkDate.SelectedToDateTime.ToString("yyyyMMddHHmmss");
                drStatus["SHIP_LOC"] = cboShipLoc.GetBindValue();
                dtRqst.Rows.Add(drStatus);

                dgLoadShipHist.ExecuteService("DA_SEL_AUTO_STACK_SHIP_HIST", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListRouteProcess()
        {
            try
            {
                dgRouteProcess.ClearRows();
                dgRouteProcess.ClearValidation();

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));

                DataRow drStatus = dtRqst.NewRow();
                drStatus["LANGID"] = LoginInfo.LANGID;
                drStatus["EQSGID"] = cboLineRP.GetBindValue();
                drStatus["ROUTID"] = cboRouteRP.GetBindValue();
                dtRqst.Rows.Add(drStatus);

                dgRouteProcess.ExecuteService("DA_SEL_AUTO_STACK_ROUT", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetLineCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private DataTable GetRouteCombo(string eqsgId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ROUTE_TYPE_DG", typeof(string));
                RQSTDT.Columns.Add("ROUT_GR_CODE", typeof(string));
                RQSTDT.Columns.Add("EXCT_ROUT_RSLT_GR_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = string.IsNullOrEmpty(eqsgId) ? null : eqsgId;
                dr["ROUTE_TYPE_DG"] = "E"; // 양산
                dr["ROUT_GR_CODE"] = "E"; // Degas 후
                //dr["EXCT_ROUT_RSLT_GR_CODE"] = "0"; //  직행 제외 // 20250620 PI : 박영규 책임 Route 등록시 직행 제외 조건 삭제 요청
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_ROUTE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private DataTable GetProcessCombo(string routid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = string.IsNullOrEmpty(routid) ? null : routid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_STACK_ROUTE_OP", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private int GetRecipeTime(string routid, string procid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = string.IsNullOrEmpty(routid) ? null : routid;
                dr["PROCID"] = string.IsNullOrEmpty(procid) ? null : procid;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("MMD_SEL_FORM_ROUT_PROC_RECIPE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Columns.Contains("END_TIME"))
                {
                    return dtResult.Rows[0]["END_TIME"].NvcInt();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 0;
        }

        #endregion


        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetListHist();
        }

        private void btnAutoConf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_149_AUTO_CONF_DEV popAutoConfig = new FCS001_149_AUTO_CONF_DEV();

                if (popAutoConfig != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = cboShipLoc.GetBindValue();

                    ControlsLibrary.C1WindowExtension.SetParameters(popAutoConfig, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => popAutoConfig.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchRP_Click(object sender, RoutedEventArgs e)
        {
            GetListRouteProcess();
        }

        private void cboLineRP_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable dtRoutRPList = GetRouteCombo(cboLineRP.SelectedValue.Nvc());
            cboRouteRP.SetDataComboItem(dtRoutRPList, CommonCombo.ComboStatus.ALL);
        }

        private void dgRouteProcess_ExecuteDataDoWork(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (dtLineCombo == null) dtLineCombo = GetLineCombo();
            if (dtRouteCombo == null) dtRouteCombo = GetRouteCombo("");
            if (dtProcessCombo == null) dtProcessCombo = GetProcessCombo("");
            if (dtYn == null)
            {
                dtYn = new DataTable();
                dtYn.Columns.Add("CBO_CODE");
                dtYn.Columns.Add("CBO_NAME");
                DataRow drY = dtYn.NewRow();
                drY["CBO_CODE"] = "Y"; drY["CBO_NAME"] = "Y"; dtYn.Rows.Add(drY);
                DataRow drN = dtYn.NewRow();
                drN["CBO_CODE"] = "N"; drN["CBO_NAME"] = "N"; dtYn.Rows.Add(drN);
            }
        }

        private void dgRouteProcess_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            try
            {
                DataTable dtResult = e.ResultData as DataTable;
                dtResult.Columns.Add("IS_STATE", typeof(string));
                
                dgRouteProcess.SetDataGridComboBoxColumn("EQSGID", dtLineCombo, isInBlank: false, isInCode: true);
                dgRouteProcess.SetDataGridComboBoxColumn("ROUTID", dtRouteCombo, isInBlank: false, isInCode: false);
                dgRouteProcess.SetDataGridComboBoxColumn("PROCID", dtProcessCombo, isInBlank: false, isInCode: true);
                dgRouteProcess.SetDataGridComboBoxColumn("USE_FLAG", dtYn, isInBlank: false, isInCode: false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRouteProcess_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }

        private void dgRouteProcess_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                switch (e.Column.Name)
                {
                    case "EQSGID":
                    case "ROUTID":
                    case "PROCID":
                        if (dgRouteProcess.GetValue(e.Row.Index, "IS_STATE").Nvc().Equals(string.Empty))
                        {
                            e.Cancel = true;
                            return;
                        }
                        break;
                }

                saveEditValue = dgRouteProcess.GetValue(e.Row.Index, e.Column.Name).Nvc();

                switch (e.Column.Name)
                {
                    case "ROUTID":
                        string filterRoute = "EQSGID = '" + dgRouteProcess.GetValue(e.Row.Index, "EQSGID").Nvc() + "'";
                        dgRouteProcess.SetDataGridComboBoxFilter("ROUTID", filterRoute);
                        break;
                    case "PROCID":
                        string filterProc = "ROUTID = '" + dgRouteProcess.GetValue(e.Row.Index, "ROUTID").Nvc() + "'";
                        dgRouteProcess.SetDataGridComboBoxFilter("PROCID", filterProc);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRouteProcess_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (dgRouteProcess.GetValue(e.Cell.Row.Index, e.Cell.Column.Name).Nvc().Equals(saveEditValue)) return;

                switch (e.Cell.Column.Name)
                {
                    case "EQSGID":
                        dgRouteProcess.SetValue(e.Cell.Row.Index, "ROUTID", "");
                        dgRouteProcess.SetValue(e.Cell.Row.Index, "PROCID", "");
                        dgRouteProcess.SetValue(e.Cell.Row.Index, "END_TIME", null);
                        e.Cell.Row.Refresh();
                        break;
                    case "ROUTID":
                        dgRouteProcess.SetValue(e.Cell.Row.Index, "PROCID", "");
                        dgRouteProcess.SetValue(e.Cell.Row.Index, "END_TIME", null);
                        e.Cell.Row.Refresh();
                        break;
                    case "PROCID":
                        string routid = dgRouteProcess.GetValue(e.Cell.Row.Index, "ROUTID").Nvc();
                        string procid = dgRouteProcess.GetValue(e.Cell.Row.Index, "PROCID").Nvc();
                        int recipeTime = GetRecipeTime(routid, procid);
                        dgRouteProcess.SetValue(e.Cell.Row.Index, "END_TIME", recipeTime);
                        e.Cell.Row.Refresh();
                        break;
                        
                }

                if (!dgRouteProcess.GetValue(e.Cell.Row.Index, "IS_STATE").Nvc().Equals("NEW"))
                {
                    dgRouteProcess.SetValue(e.Cell.Row.Index, "IS_STATE", "EDIT");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgRouteProcess_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            try
            {
                int rowIndex = dgRouteProcess.GetCurrentRowIndex();
                if (rowIndex < 0) return;

                btnDeleteRow.IsEnabled = dgRouteProcess.GetValue(rowIndex, "IS_STATE").Nvc().Equals("NEW");
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
                dgRouteProcess.AddRowData(1);
                dgRouteProcess.SetValue(dgRouteProcess.Rows.Count - 1, "IS_STATE", "NEW");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rowIndex = dgRouteProcess.GetCurrentRowIndex();
                if (rowIndex < 0) return;

                if (dgRouteProcess.GetValue(rowIndex, "IS_STATE").Nvc().Equals("NEW"))
                {
                    dgRouteProcess.DeleteRowData(rowIndex - dgRouteProcess.TopRows.Count);
                    if (rowIndex >= dgRouteProcess.Rows.Count)
                    {
                        dgRouteProcess.SelectRow(dgRouteProcess.Rows.Count - 1);
                    }
                    else
                    {
                        dgRouteProcess.SelectRow(rowIndex);
                    }
                }
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
                dgRouteProcess.ClearValidation();

                int saveCount = 0;
                int validationCount = 0;
                for (int row = 0; row < dgRouteProcess.Rows.Count; row++)
                {
                    if (dgRouteProcess.GetValue(row, "IS_STATE").Nvc().Equals("NEW") ||
                        dgRouteProcess.GetValue(row, "IS_STATE").Nvc().Equals("EDIT"))
                    {
                        saveCount++; 

                        // 필수 입력 체크
                        if (string.IsNullOrEmpty(dgRouteProcess.GetValue(row, "EQSGID").Nvc()))
                        {
                            dgRouteProcess.SetCellValidation(row, "EQSGID", "SFU8275", ObjectDic.Instance.GetObjectName("라인"));
                            validationCount++;
                        }

                        if (string.IsNullOrEmpty(dgRouteProcess.GetValue(row, "ROUTID").Nvc()))
                        {
                            dgRouteProcess.SetCellValidation(row, "ROUTID", "SFU8275", ObjectDic.Instance.GetObjectName("공정경로"));
                            validationCount++;
                        }

                        if (string.IsNullOrEmpty(dgRouteProcess.GetValue(row, "PROCID").Nvc()))
                        {
                            dgRouteProcess.SetCellValidation(row, "PROCID", "SFU8275", ObjectDic.Instance.GetObjectName("공정"));
                            validationCount++;
                        }

                        if (string.IsNullOrEmpty(dgRouteProcess.GetValue(row, "MIN_PROG_TIME").Nvc()))
                        {
                            dgRouteProcess.SetCellValidation(row, "MIN_PROG_TIME", "SFU8275", ObjectDic.Instance.GetObjectName("STACK_POSSIBLE_TIME"));
                            validationCount++;
                        }

                        if (string.IsNullOrEmpty(dgRouteProcess.GetValue(row, "MAX_PROG_TIME").Nvc()))
                        {
                            dgRouteProcess.SetCellValidation(row, "MAX_PROG_TIME", "SFU8275", ObjectDic.Instance.GetObjectName("STACK_POSSIBLE_TIME"));
                            validationCount++;
                        }

                        if (!string.IsNullOrEmpty(dgRouteProcess.GetValue(row, "MIN_PROG_TIME").Nvc()) &&
                           !string.IsNullOrEmpty(dgRouteProcess.GetValue(row, "MAX_PROG_TIME").Nvc()))
                        {
                            decimal maxTime = dgRouteProcess.GetValue(row, "END_TIME").NvcDecimal();
                            //if (maxTime == 0)
                            //{
                            //    dgRouteProcess.SetCellValidation(row, "END_TIME", "SFU9999", ObjectDic.Instance.GetObjectName("END_TIME_MIN"));
                            //    validationCount++;
                            //}

                            if (maxTime > 0)
                            {
                                if (dgRouteProcess.GetValue(row, "MIN_PROG_TIME").NvcDecimal() <= 0)
                                {
                                    dgRouteProcess.SetCellValidation(row, "MIN_PROG_TIME", "SFU1203", "1", maxTime.Nvc());
                                    validationCount++;
                                }

                                if (dgRouteProcess.GetValue(row, "MIN_PROG_TIME").NvcDecimal() > maxTime)
                                {
                                    dgRouteProcess.SetCellValidation(row, "MIN_PROG_TIME", "SFU1203", "1", maxTime.Nvc());
                                    validationCount++;
                                }

                                if (dgRouteProcess.GetValue(row, "MAX_PROG_TIME").NvcDecimal() <= 0)
                                {
                                    dgRouteProcess.SetCellValidation(row, "MAX_PROG_TIME", "SFU1203", "1", maxTime.Nvc());
                                    validationCount++;
                                }

                                if (dgRouteProcess.GetValue(row, "MAX_PROG_TIME").NvcDecimal() > maxTime)
                                {
                                    dgRouteProcess.SetCellValidation(row, "MAX_PROG_TIME", "SFU1203", "1", maxTime.Nvc());
                                    validationCount++;
                                }

                                if (dgRouteProcess.GetValue(row, "MIN_PROG_TIME").NvcDecimal() > dgRouteProcess.GetValue(row, "MAX_PROG_TIME").NvcDecimal())
                                {
                                    dgRouteProcess.SetCellValidation(row, "MIN_PROG_TIME", "FM_ME_0252");
                                    dgRouteProcess.SetCellValidation(row, "MAX_PROG_TIME", "FM_ME_0252");
                                    validationCount++;
                                }
                            }
                        }
                    }
                }

                if (saveCount == 0)
                {
                    // 변경된 데이터가 없습니다.
                    Util.MessageInfo("SFU1566");
                    return;
                }

                if (validationCount > 0) return;

                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0214", (result) =>
                {
                    if (result != MessageBoxResult.OK) return;

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("ROUTID", typeof(string));
                    dtRqst.Columns.Add("PROCID", typeof(string));
                    dtRqst.Columns.Add("MIN_PROG_TIME", typeof(decimal));
                    dtRqst.Columns.Add("MAX_PROG_TIME", typeof(decimal));
                    dtRqst.Columns.Add("USE_FLAG", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));

                    for (int i = 0; i < dgRouteProcess.Rows.Count; i++)
                    {
                        if (dgRouteProcess.Rows[i].DataItem == null) continue;

                        if (dgRouteProcess.GetValue(i, "IS_STATE").Nvc().Equals("NEW") || dgRouteProcess.GetValue(i, "IS_STATE").Nvc().Equals("EDIT"))
                        {
                            DataRow dr = dtRqst.NewRow();
                            dr["ROUTID"] = dgRouteProcess.GetValue(i, "ROUTID").Nvc();
                            dr["PROCID"] = dgRouteProcess.GetValue(i, "PROCID").Nvc();
                            dr["MIN_PROG_TIME"] = dgRouteProcess.GetValue(i, "MIN_PROG_TIME").NvcInt();
                            dr["MAX_PROG_TIME"] = dgRouteProcess.GetValue(i, "MAX_PROG_TIME").NvcInt();                            
                            dr["USE_FLAG"] = dgRouteProcess.GetValue(i, "USE_FLAG").Nvc();
                            dr["USERID"] = LoginInfo.USERID;
                            dtRqst.Rows.Add(dr);
                        }
                    }

                    new ClientProxy().ExecuteService("DA_MERGE_AUTO_STACK_ROUT", "RQSTDT", null, dtRqst, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //저장하였습니다.
                            Util.MessageInfo("FM_ME_0215");
                            
                            GetListRouteProcess();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

     
    }

}
