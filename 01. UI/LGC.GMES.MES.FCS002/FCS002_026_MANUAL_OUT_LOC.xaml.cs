/*************************************************************************************
 Created Date : 2020.10.14
      Creator : Initial Created 
   Decription : [공Tray 수동배출]
                동별 공통코드 : CNVR_LOCATION_OUT_LOC_USE, USE_YN = "Y" 일때
                이 팝업 사용, 그외 일때 기존 팝업(FCS002_026_MANUAL_OUT) 사용.
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.06  조영대 : Initial Created  (Copy by FCS002_026_MANUAL_OUT)
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_026_MANUAL_OUT_LOC : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sTrayId = string.Empty;

        public string TrayId
        {
            get { return sTrayId; }
        }

        public FCS002_026_MANUAL_OUT_LOC()
        {
            InitializeComponent();
            Initialize();
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
            InitCombo();
        }

        private void InitCombo()
        {
            string bizRuleName = "DA_SEL_CNVR_LOCATION_MANUAL_OUT_EQPT_CBO";

            string[] arrColumn = { "LANGID", "AREAID", "EQPT_GR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "C" };
            cboToEqp.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, true);
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                sTrayId = Util.NVC(tmps[0]);
            }
        }

        private void cboToEqp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string bizRuleName = "DA_SEL_CNVR_LOCATION_MANUAL_OUT_LOC_CBO";

            string[] arrColumn = { "LANGID", "AREAID", "EQPT_GR_TYPE_CODE", "LANE_ID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "C", cboToEqp.GetStringValue() };
            cboOutLoc.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, true);
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboToEqp.GetStringValue()))
            {
                //설비를 선택해주세요.
                Util.MessageValidation("FM_ME_0171");
                return;
            }

            if (string.IsNullOrEmpty(cboOutLoc.GetStringValue()))
            {
                //배출할 위치를 선택해주세요.
                Util.MessageValidation("FM_ME_0340");
                return;
            }

            //상태를 변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0337", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("CSTID", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["CSTID"] = sTrayId;
                        RQSTDT.Rows.Add(dr);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_FROM_EQP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                        if (dtResult.Rows.Count > 0)
                        {
                            foreach (DataRow drTray in dtResult.Rows)
                            {
                                DataTable dtInData = new DataTable();
                                dtInData.TableName = "IN_REQ_TRF_INFO";
                                dtInData.Columns.Add("AREAID", typeof(string));
                                dtInData.Columns.Add("CARRIERID", typeof(string));
                                dtInData.Columns.Add("SRC_EQPTID", typeof(string));
                                dtInData.Columns.Add("DST_EQPTID", typeof(string));
                                dtInData.Columns.Add("DST_LOCID", typeof(string));
                                dtInData.Columns.Add("UPDUSER", typeof(string));

                                DataRow drIn = dtInData.NewRow();
                                drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                                drIn["CARRIERID"] = Util.NVC(drTray["CSTID"]);
                                //drIn["SRC_EQPTID"] = Util.NVC(drTray["SRC_EQPTID"]);
                                drIn["DST_EQPTID"] = Util.NVC(cboToEqp.GetBindValue());
                                drIn["DST_LOCID"] = Util.NVC(cboOutLoc.GetBindValue());
                                drIn["UPDUSER"] = LoginInfo.USERID;
                                dtInData.Rows.Add(drIn);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_MHS_REG_TRF_CMD_BY_UI", "IN_REQ_TRF_INFO", null, dtInData);
                            }
                        }

                        this.DialogResult = MessageBoxResult.OK;
                        Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }

        #endregion

        #region Mehod

        #endregion

    }
}
