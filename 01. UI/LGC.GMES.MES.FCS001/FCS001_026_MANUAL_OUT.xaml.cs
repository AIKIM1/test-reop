/*************************************************************************************
 Created Date : 2020.10.14
      Creator : Initial Created 
   Decription : [공Tray 수동배출]
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  NAME : Initial Created 
  2021.04.07  KDH : To Eqp 이벤트 변경 (SelectedIndexChanged -> SelectedValueChanged)
  2021.04.22  KDH : 위치 정보 그룹화 대응
  2022.05.13  이정미 : EQP_LOC_GRP_CD 동별 공통코드로 변경
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
    public partial class FCS001_026_MANUAL_OUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string sTrayId = string.Empty;

        public string TrayId
        {
            get { return sTrayId; }
        }

        public FCS001_026_MANUAL_OUT()
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
            CommonCombo_Form _combo = new CommonCombo_Form();

            //2021.04.22 위치 정보 그룹화 대응 START
            //string[] sFilter = { "CNV,STO" };
            //_combo.SetCombo(cboToEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "FORMMAINEQP", sFilter: sFilter);
            string[] sFilter = { "EQP_LOC_GRP_CD" };
            //_combo.SetCombo(cboToEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilter);
            //2021.04.22 위치 정보 그룹화 대응 END
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

        //2021.04.07 To Eqp 이벤트 변경 (SelectedIndexChanged -> SelectedValueChanged) START
        private void cboToEqp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboMainEqpParent = { cboToEqp };
            _combo.SetCombo(cboOutLoc, CommonCombo_Form.ComboStatus.SELECT, sCase: "FORMTOOUTLOC", cbParent: cboMainEqpParent);

            if (cboOutLoc.Items.Count > 0)
            {
                cboOutLoc.SelectedIndex = 0;
            }
        }
        //2021.04.07 To Eqp 이벤트 변경 (SelectedIndexChanged -> SelectedValueChanged) END

        private void btnChange_Click(object sender, EventArgs e)
        {
            //상태를 변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0337", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //2021-05-14 설비 EQPTID 그룹으로 맵핑되어 있어 EQPTID로 맵핑 start
                        
                        DataTable dtEqp = new DataTable();
                        dtEqp.Columns.Add("PORT_ID", typeof(string));

                        DataRow drPort = dtEqp.NewRow();
                        drPort["PORT_ID"] = Util.GetCondition(cboOutLoc);

                        dtEqp.Rows.Add(drPort);

                        DataTable dtEqpR = new ClientProxy().ExecuteServiceSync("DA_SEL_EQPTID_BY_PORT_UI", "RQSTDT", "RSLTDT", dtEqp);
                        cboToEqp.Tag = Util.NVC(dtEqpR.Rows[0]["EQPTID"]);

                        //2021-05-14 end


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
                                dtInData.Columns.Add("DST_PORTID", typeof(string));
                                dtInData.Columns.Add("UPDUSER", typeof(string));
                                //dtRqst.Columns.Add("TRF_CAUSE_CODE", typeof(string));
                                //dtRqst.Columns.Add("MANL_TRF_CAUSE_CNTT", typeof(string));

                                DataRow drIn = dtInData.NewRow();
                                drIn["AREAID"] = LoginInfo.CFG_AREA_ID;
                                drIn["CARRIERID"] = Util.NVC(drTray["CSTID"]);
                                drIn["SRC_EQPTID"] = Util.NVC(drTray["SRC_EQPTID"]);
                                //  drIn["DST_EQPTID"] = Util.GetCondition(cboToEqp);
                                drIn["DST_EQPTID"] = Util.NVC(cboToEqp.Tag);
                                drIn["DST_PORTID"] = Util.GetCondition(cboOutLoc, sMsg: "FM_ME_0340");  //배출할 위치를 선택해주세요.
                                if (string.IsNullOrEmpty(drIn["DST_PORTID"].ToString())) return;
                                drIn["UPDUSER"] = LoginInfo.USERID;
                                //dr["TRF_CAUSE_CODE"] = null;
                                //dr["MANL_TRF_CAUSE_CNTT"] = null ;
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
