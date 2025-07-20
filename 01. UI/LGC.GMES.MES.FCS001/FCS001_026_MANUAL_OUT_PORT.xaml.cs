/*************************************************************************************
 Created Date : 2022.07.20
      Creator : Initial Created 
   Decription : [공Tray 수동배출]
                동별 공통코드 : BOTH_PORT_ID_USE, USE_YN = "Y" 일때
                이 팝업 사용, 그외 일때 기존 팝업(FCS001_026_MANUAL_OUT) 사용.
                폴란드 RTD에서 수동배출 Input Parameter, 로직이 변경되어 기존과 분리.
--------------------------------------------------------------------------------------
 [Change History]
  2022.07.20  최도훈 : Initial Created  (Copy by FCS001_026_MANUAL_OUT)
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_026_MANUAL_OUT_PORT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sTrayId = string.Empty;

        public string TrayId
        {
            get { return sTrayId; }
        }

        public FCS001_026_MANUAL_OUT_PORT()
        {
            InitializeComponent();
            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public enum PRIORITY_NO
        {
            Default = 50
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitCombo();
        }
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilter = { "EQP_LOC_GRP_CD" };
            _combo.SetCombo(cboToEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", sFilter: sFilter);
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
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboMainEqpParent = { cboToEqp };
            _combo.SetCombo(cboOutPort, CommonCombo_Form.ComboStatus.SELECT, sCase: "FORMTOOUTLOC", cbParent: cboMainEqpParent);

            if (cboOutPort.Items.Count > 0)
            {
                cboOutPort.SelectedIndex = 0;
            }

        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboToEqp.GetStringValue()))
            {
                //설비를 선택해주세요.
                Util.MessageValidation("FM_ME_0171");
                return;
            }

            if (string.IsNullOrEmpty(cboOutPort.GetStringValue()))
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
                        DataTable dtEqp = new DataTable();
                        dtEqp.Columns.Add("PORT_ID", typeof(string));

                        DataRow drPort = dtEqp.NewRow();
                        drPort["PORT_ID"] = Util.GetCondition(cboOutPort);

                        dtEqp.Rows.Add(drPort);

                        DataTable dtEqpR = new ClientProxy().ExecuteServiceSync("DA_SEL_EQPTID_BY_PORT_UI", "RQSTDT", "RSLTDT", dtEqp);
                        cboToEqp.Tag = Util.NVC(dtEqpR.Rows[0]["EQPTID"]);

                        //2021-05-14 end


                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("CSTID", typeof(string));

                        string[] arrTrayId = sTrayId.Split('|');

                        foreach (string tray in arrTrayId)
                        {
                            if (string.IsNullOrWhiteSpace(tray)) continue;

                            DataRow newRow = RQSTDT.NewRow();
                            newRow["CSTID"] = tray;
                            RQSTDT.Rows.Add(newRow);

                        }

                        //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_FROM_EQP_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_F", "RQSTDT", "RSLTDT", RQSTDT);

                        if (dtResult.Rows.Count > 0)
                        {
                            foreach (DataRow drTray in dtResult.Rows)
                            {
                                DataTable dtInData = new DataTable();
                                dtInData.TableName = "IN_REQ_TRF_INFO";
                                dtInData.Columns.Add("CARRIERID", typeof(string));
                                dtInData.Columns.Add("SRC_EQPTID", typeof(string));
                                dtInData.Columns.Add("SRC_PORTID", typeof(string));
                                dtInData.Columns.Add("DST_EQPTID", typeof(string));
                                dtInData.Columns.Add("DST_PORTID", typeof(string));
                                dtInData.Columns.Add("PRIORITY_NO", typeof(decimal));
                                dtInData.Columns.Add("UPDUSER", typeof(string));

                                DataRow drIn = dtInData.NewRow();
                                drIn["CARRIERID"] = Util.NVC(drTray["CSTID"]).ToString();
                                drIn["SRC_EQPTID"] = Util.NVC(drTray["EQPT_CUR"]).ToString();
                                drIn["SRC_PORTID"] = Util.NVC(drTray["PORT_CUR"]).ToString();
                                drIn["DST_EQPTID"] = Util.NVC(cboToEqp.Tag.ToString());
                                drIn["DST_PORTID"] = Util.NVC(cboOutPort.GetBindValue()).ToString();
                                drIn["PRIORITY_NO"] = PRIORITY_NO.Default;
                                drIn["UPDUSER"] = LoginInfo.USERID;
                                dtInData.Rows.Add(drIn);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_MHS_REG_TRF_CMD_BY_UI_2", "IN_REQ_TRF_INFO", null, dtInData);
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
