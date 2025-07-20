/*************************************************************************************
 Created Date :
      Creator : PSM
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  PSM    : Initial Created
  2021.04.22  KDH    : RACK ID 조회 위치 변경
  2022.09.05  이정미 : Aging 입고 호출 BIZ 변경 및 btnSave_Click 수정 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_021_AGING_CARRY_IN : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sTrayNo = string.Empty;
        private string sTrayId = string.Empty;
        private string sEndTime = string.Empty;

        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        private string RACK_ID = string.Empty;

        public string TRAYNO
        {
            set { this.sTrayNo = value; }
        }

        public string TRAYID
        {
            set { this.sTrayId = value; }
        }

        public string ENDTIME
        {
            set { this.sEndTime = value; }
        }
        #endregion

        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_021_AGING_CARRY_IN()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, EventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null && parameters.Length >= 1)
            {
                sTrayNo = Util.NVC(parameters[0]);
                sTrayId = Util.NVC(parameters[1]);
                sEndTime = Util.NVC(parameters[2]);
            }

            txtTrayId.Text = sTrayId;

            dtpDate.SelectedDateTime = DateTime.Now;
            dtpTime.DateTime = DateTime.Now;

            //Combo Setting
            InitCombo();
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();
            string[] sFilter = { "FORM_AGING_TYPE_CODE" };
            ComCombo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", sFilter: sFilter);
        }
        #endregion

        #region [Method]
        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingType);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetRackID()
        {
            try
            {
                RACK_ID = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("X_PSTN", typeof(string));
                RQSTDT.Columns.Add("Y_PSTN", typeof(string));
                RQSTDT.Columns.Add("Z_PSTN", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = Util.GetCondition(cboSCLine);//cboSCLine.GetStringValue("CBO_CODE");
                dr["X_PSTN"] = Util.GetCondition(cboRow);
                dr["Y_PSTN"] = Util.GetCondition(cboCol);
                dr["Z_PSTN"] = Util.GetCondition(cboStg);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_RACK_INFO_PSTN", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    RACK_ID = row["RACK_ID"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Event]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //Aging 입고처리 하시겠습니까?
            Util.MessageConfirm("FM_ME_0326", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        GetRackID(); //2021.04.22 RACK ID 조회 위치 변경

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("IFMODE", typeof(string));
                        dtRqst.Columns.Add("AREAID", typeof(string));
                        dtRqst.Columns.Add("CSTID", typeof(string));
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("RACK_ID", typeof(string));
                        dtRqst.Columns.Add("MANUAL_IN_TIME", typeof(DateTime));

                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["CSTID"] = sTrayId;
                        dr["EQPTID"] = Util.GetCondition(cboSCLine); //cboSCLine.GetStringValue("CBO_CODE");
                        dr["USERID"] = LoginInfo.USERID;

                        dr["RACK_ID"] = RACK_ID;
                        dr["MANUAL_IN_TIME"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpTime.DateTime.Value.ToString(" HH:mm:00");
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_AGING_MANUAL_INPUT_UI_MB", "INDATA", "OUTDATA", dtRqst);


                        //this.DialogResult = MessageBoxResult.OK;
                        //Util.AlertInfo("FM_ME_0327");  //Aging 입고처리 완료하였습니다.

                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            this.DialogResult = MessageBoxResult.OK;
                            Util.AlertInfo("FM_ME_0327");  //Aging 입고처리 완료하였습니다.
                        }
                        else
                        {
                            this.DialogResult = MessageBoxResult.No;
                            Util.AlertInfo("FM_ME_0328");  //Aging 입고처리에 실패하였습니다.
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
             Close();
        }

        private void cboAgingType_SelectionCommitted(object sender, EventArgs e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            if (cboAgingType.SelectedIndex > -1)
            {
                GetCommonCode();

                string[] sFilter = { EQPT_GR_TYPE_CODE, LANE_ID };
                _combo.SetCombo(cboSCLine, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "SCLINE", sFilter: sFilter);
            }
        }

        private void cboSCLine_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            if (cboSCLine.SelectedIndex > -1)
            {
                //GetRackID(); //2021.04.22 RACK ID 조회 위치 변경
                object[] objParent = { EQPT_GR_TYPE_CODE, LANE_ID, Util.GetCondition(cboSCLine) };
                _combo.SetComboObjParent(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AGING_ROW", objParent: objParent);
            }
        }

        private void cboRow_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();
            if (cboRow.SelectedIndex > -1)
            {
                object[] objParent = { EQPT_GR_TYPE_CODE, LANE_ID, Util.GetCondition(cboSCLine), Util.GetCondition(cboRow) };
                ComCombo.SetComboObjParent(cboCol, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AGING_COL", objParent: objParent);
                if (cboCol.Items.Count > 0)
                {
                    cboCol.SelectedIndex = 0;
                }

                //object[] objParent1 = { EQPT_GR_TYPE_CODE, LANE_ID, cboSCLine, cboRow, cboCol };
                //ComCombo.SetComboObjParent(cboStg, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AGING_STG", objParent: objParent1);
                //if (cboStg.Items.Count > 0)
                //{
                //    cboStg.SelectedIndex = 0;
                //}
            }
        }

        /// <summary>
        /// 자동입력 선택 시 이전공정 종료시간으로 자동입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkNotJudgCell_CheckedChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(sEndTime))
            {
                DateTime dEndTime = DateTime.Parse(sEndTime);
                dtpDate.SelectedDateTime = dEndTime;
                dtpTime.DateTime = dEndTime;
            }
            else
            {
                dtpDate.SelectedDateTime = DateTime.Now;
                dtpTime.DateTime = DateTime.Now;
            }

            dtpDate.IsEnabled = false;
            dtpTime.IsEnabled = false;
        }

        private void chkNotJudgCell_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpDate.IsEnabled = true;
            dtpTime.IsEnabled = true;
        }

        #endregion

        private void cboCol_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();
            if (cboCol.SelectedIndex > -1)
            {
                object[] objParent1 = { EQPT_GR_TYPE_CODE, LANE_ID, cboSCLine, cboRow, cboCol };
                ComCombo.SetComboObjParent(cboStg, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AGING_STG", objParent: objParent1);
                if (cboStg.Items.Count > 0)
                {
                    cboStg.SelectedIndex = 0;
                }
            }
        }
    }
}
