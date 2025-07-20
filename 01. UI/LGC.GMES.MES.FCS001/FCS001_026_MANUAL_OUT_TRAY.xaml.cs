/*************************************************************************************
 Created Date : 2022.11.22
      Creator : Initial Created 
   Decription : [공Tray 수동배출]
--------------------------------------------------------------------------------------
 [Change History]
  2022.11.22  이정미 : Initial Created  (Copy by FCS001_026_MANUAL_OUT)
  2022.12.26  형준우 : ComboBox DA문 변경 및 수동배출 BR문 변경
  2023.11.17  김용식 : ComboBox 조회 조건 LangID 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_026_MANUAL_OUT_TRAY : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sTrayId = string.Empty;
        private string sTrayType = string.Empty;

        public string TrayId
        {
            get { return sTrayId; }
        }

        public string TrayType {
            get { return sTrayType; }
        }

        public FCS001_026_MANUAL_OUT_TRAY()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            string bizRuleName = "DA_SEL_CNVR_DESTINATION_OUT_LOC_CBO";

            //string[] arrColumn = { "AREAID", "TRAY_TYPE_CODE" };
            //string[] arrCondition = { LoginInfo.CFG_AREA_ID, TrayType };
            string[] arrColumn = { "AREAID", "TRAY_TYPE_CODE","LANGID" };
            string[] arrCondition = { LoginInfo.CFG_AREA_ID, TrayType, LoginInfo.LANGID }; // 2023.11.17 다국어를 위한 추가
            cboToDestination.SetDataComboItem(bizRuleName, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, true);
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                sTrayId = Util.NVC(tmps[0]);
                sTrayType = Util.NVC(tmps[1]);
                InitCombo();
            }
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboToDestination.GetStringValue()))
            {
                //설비를 선택해주세요.
                Util.MessageValidation("FM_ME_0171");
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
                                dtInData.TableName = "RQSTDT";
                                dtInData.Columns.Add("SRCTYPE", typeof(string));
                                dtInData.Columns.Add("IFMODE", typeof(string));
                                dtInData.Columns.Add("CSTID", typeof(string));
                                dtInData.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                                dtInData.Columns.Add("USERID", typeof(string));

                                DataRow drIn = dtInData.NewRow();
                                drIn["SRCTYPE"] = "UI";
                                drIn["IFMODE"] = "OFF";
                                drIn["CSTID"] = Util.NVC(drTray["CSTID"]);
                                drIn["CNVR_LOCATION_ID"] = Util.NVC(cboToDestination.GetBindValue());
                                drIn["USERID"] = LoginInfo.USERID;
                                dtInData.Rows.Add(drIn);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_EMPTY_CST_MANUAL_TO_FIXED_DESTINATION", "RQSTDT", "RSLTDT", dtInData);
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
