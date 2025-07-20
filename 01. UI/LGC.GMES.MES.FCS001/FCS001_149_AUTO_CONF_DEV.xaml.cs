/*************************************************************************************
 Created Date : 2023.05.17
      Creator : 조영대
   Decription : 단적재 자동설정 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.17  조영대 : Initial Created.
  2024.02.22  조영대 : 입력 방식 수정
**************************************************************************************/


using System;
using System.Data;
using System.Windows;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.FCS001
{

    public partial class FCS001_149_AUTO_CONF_DEV : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string SHIP_LOC = string.Empty;

        private DataTable dtLaneCombo = null;
        private DataTable dtEquipmentCombo = null;
        

        private string saveEditValue = string.Empty;

        public FCS001_149_AUTO_CONF_DEV()
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
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters == null || parameters.Length < 1) return;

            SHIP_LOC = Util.NVC(parameters[0]);

            GetList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveStackShip();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgWorkLine.AddRowData(1);
                dgWorkLine.SetValue(dgWorkLine.Rows.Count - 1, "USE_FLAG", "Y");
                dgWorkLine.SetValue(dgWorkLine.Rows.Count - 1, "IS_STATE", "NEW");                
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
                int rowIndex = dgWorkLine.GetCurrentRowIndex();
                if (rowIndex < 0) return;

                if (dgWorkLine.GetValue(rowIndex, "IS_STATE").Nvc().Equals("NEW"))
                {
                    dgWorkLine.DeleteRowData(rowIndex);
                    if (rowIndex >= dgWorkLine.Rows.Count)
                    {
                        dgWorkLine.SelectRow(dgWorkLine.Rows.Count - 1);
                    }
                    else
                    {
                        dgWorkLine.SelectRow(rowIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkLine_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            try
            {
                int rowIndex = dgWorkLine.GetCurrentRowIndex();
                if (rowIndex < 0) return;

                btnDeleteRow.IsEnabled = dgWorkLine.GetValue(rowIndex, "IS_STATE").Nvc().Equals("NEW");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkLine_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                switch (e.Column.Name)
                {
                    case "LANE_ID":
                    case "EQPTID":
                        if (dgWorkLine.GetValue(e.Row.Index, "IS_STATE").Nvc().Equals(string.Empty))
                        {
                            e.Cancel = true;
                            return;
                        }
                        break;
                }

                saveEditValue = dgWorkLine.GetValue(e.Row.Index, e.Column.Name).Nvc();
                
                switch (e.Column.Name)
                {
                    case "EQPTID":
                        string filterRoute = "LANE_ID = '" + dgWorkLine.GetValue(e.Row.Index, "LANE_ID").Nvc() + "'";
                        dgWorkLine.SetDataGridComboBoxFilter("EQPTID", filterRoute);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkLine_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (dgWorkLine.GetValue(e.Cell.Row.Index, e.Cell.Column.Name).Nvc().Equals(saveEditValue)) return;

            switch (e.Cell.Column.Name)
            {
                case "LANE_ID":
                    dgWorkLine.SetValue(e.Cell.Row.Index, "EQPTID", "");
                    e.Cell.Row.Refresh();
                    break;
            }

            if (!dgWorkLine.GetValue(e.Cell.Row.Index, "IS_STATE").Nvc().Equals("NEW"))
            {
                dgWorkLine.SetValue(e.Cell.Row.Index, "IS_STATE", "EDIT");
            }
        }

        private void dgWorkLine_ExecuteDataDoWork(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;
            dtResult.Columns.Add("IS_STATE", typeof(string));

            if (dtLaneCombo == null) dtLaneCombo = GetLaneCombo();
            if (dtEquipmentCombo == null) dtEquipmentCombo = GetEquipmentCombo("");            
        }

        private void dgWorkLine_ExecuteDataCompleted(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            dgWorkLine.SetDataGridComboBoxColumn("LANE_ID", dtLaneCombo, isInBlank: false);
            dgWorkLine.SetDataGridComboBoxColumn("EQPTID", dtEquipmentCombo, isInBlank: false);
            

            DataTable dtYn = new DataTable();
            dtYn.Columns.Add("CBO_CODE");
            dtYn.Columns.Add("CBO_NAME");
            DataRow drY = dtYn.NewRow();
            drY["CBO_CODE"] = "Y"; drY["CBO_NAME"] = "Y"; dtYn.Rows.Add(drY);
            DataRow drN = dtYn.NewRow();
            drN["CBO_CODE"] = "N"; drN["CBO_NAME"] = "N"; dtYn.Rows.Add(drN);

            dgWorkLine.SetDataGridComboBoxColumn("USE_FLAG", dtYn, isInBlank: false, isInCode: false);
        }
        #endregion

        #region Method
        private DataTable GetLaneCombo()
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_AUTO_STACK_SHIP_LOC", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }

        private DataTable GetEquipmentCombo(string laneid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANE_ID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

             
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANE_ID"] = string.IsNullOrEmpty(laneid) ? null : laneid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_FORM_EQP_BY_LANE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }
                
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["LANE_ID"] = SHIP_LOC;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);
                                           
                dgWorkLine.ExecuteService("DA_BAS_SEL_TB_SFC_FORM_STACK_EQPT", "RQATDT", "RSLTDT", dtRqst, false, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveStackShip()
        {
            try
            {
                this.ClearValidation();

                int saveCount = 0;
                int validationCount = 0;
                for (int row = 0; row < dgWorkLine.Rows.Count; row++)
                {
                    if (dgWorkLine.GetValue(row, "IS_STATE").Nvc().Equals("NEW") ||
                        dgWorkLine.GetValue(row, "IS_STATE").Nvc().Equals("EDIT"))
                    {
                        saveCount++;

                        // 필수 입력 체크
                        if (string.IsNullOrEmpty(dgWorkLine.GetValue(row, "LANE_ID").Nvc()))
                        {
                            dgWorkLine.SetCellValidation(row, "LANE_ID", "SFU8275", ObjectDic.Instance.GetObjectName("LANE_ID"));
                            validationCount++;
                        }

                        if (string.IsNullOrEmpty(dgWorkLine.GetValue(row, "EQPTID").Nvc()))
                        {
                            dgWorkLine.SetCellValidation(row, "EQPTID", "SFU8275", ObjectDic.Instance.GetObjectName("EQPTID"));
                            validationCount++;
                        }

                        if (string.IsNullOrEmpty(dgWorkLine.GetValue(row, "BUF_QTY").Nvc()))
                        {
                            dgWorkLine.SetCellValidation(row, "BUF_QTY", "SFU8275", ObjectDic.Instance.GetObjectName("BUFFER_CNT"));
                            validationCount++;
                        }

                        if (string.IsNullOrEmpty(dgWorkLine.GetValue(row, "USE_FLAG").Nvc()))
                        {
                            dgWorkLine.SetCellValidation(row, "USE_FLAG", "SFU8275", ObjectDic.Instance.GetObjectName("사용유무"));
                            validationCount++;
                        }

                        // 중복 체크
                        for (int index = 0; index < dgWorkLine.Rows.Count; index++)
                        {
                            if (index == row) continue;

                            if (!string.IsNullOrEmpty(dgWorkLine.GetValue(row, "EQPTID").Nvc()) &&
                                dgWorkLine.GetValue(row, "EQPTID").Nvc() == dgWorkLine.GetValue(index, "EQPTID").Nvc())
                            {
                                dgWorkLine.SetCellValidation(row, "EQPTID", "SFU2051", "");
                                validationCount++;
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

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("BUF_QTY", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                
                for (int i = 0; i < dgWorkLine.Rows.Count; i++)
                {
                    if (dgWorkLine.Rows[i].DataItem == null) continue;

                    if (dgWorkLine.GetValue(i, "IS_STATE").Nvc().Equals("NEW") || dgWorkLine.GetValue(i, "IS_STATE").Nvc().Equals("EDIT"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["EQPTID"] = dgWorkLine.GetValue(i, "EQPTID").Nvc();
                        dr["LANE_ID"] = dgWorkLine.GetValue(i, "LANE_ID").Nvc();
                        dr["BUF_QTY"] = dgWorkLine.GetValue(i, "BUF_QTY").Nvc();
                        dr["USE_FLAG"] = dgWorkLine.GetValue(i, "USE_FLAG").Nvc();
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_MERGE_TB_SFC_FORM_STACK_EQPT", "RQSTDT", null, dtRqst);
                if (dtResult != null)
                {
                    this.DialogResult = MessageBoxResult.OK;
                    Util.MessageInfo("SFU1889");
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
