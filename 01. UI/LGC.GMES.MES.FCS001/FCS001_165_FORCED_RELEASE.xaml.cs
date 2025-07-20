/************************************************************************************* 
 Created Date : 2023.10.23
      Creator : Chi Woo
   Decription : 공 Tray 보관/공급 관리 - 강제 출고 예약
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.23  조영대 : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_165_FORCED_RELEASE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string EQPTID = string.Empty;

        public FCS001_165_FORCED_RELEASE()
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
        

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 설비군
            string[] arrColumn = { "LANGID", "AREAID", "COM_TYPE_CODE", "ATTR2" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "FORMLGS_EMPTY_CST_EQPT_GR_CODE_UI", "Y" };
            cboEqptType.SetDataComboItem("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR", arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT);

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                if (string.IsNullOrEmpty(Util.NVC(tmps[0])))
                {
                    cboEqptType.SelectedIndex = 0;
                }
                else
                {
                    cboEqptType.SelectedValue = Util.NVC(tmps[0]);
                }
                txtTrayType.Text = Util.NVC(tmps[1]);
                EQPTID = Util.NVC(tmps[2]);
            }

            this.Header = ObjectDic.Instance.GetObjectName("FORC_ISS_RSV") + " : " + Util.NVC(tmps[2]);
        }

        private void cboEqptType_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            string[] arrColumn = { "LANGID", "AREAID", "EQPT_GR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, Util.NVC(cboEqptType.SelectedValue) };
            cboTargetLoc.DisplayMemberPath = "CNVR_LOCATION_DESC";
            cboTargetLoc.SelectedValuePath = "CNVR_LOCATION_ID";
            cboTargetLoc.SetDataComboItem("DA_SEL_CNVR_LOCATION_MANUAL_OUT_LOC_CBO", arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT);
        }

        private void btnForceRelease_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            if (cboEqptType.GetBindValue() == null)
            {
                cboEqptType.SetValidation("SFU8275", tbEqptType.Text);
                return;
            }

            if (cboTargetLoc.GetBindValue() == null)
            {
                cboTargetLoc.SetValidation("SFU8275", tbTargetLoc.Text);
                return;
            }

            if (txtTrayType.GetBindValue() == null)
            {
                txtTrayType.SetValidation("SFU8275", tbTrayType.Text);
                return;
            }
                        
            if (txtReserveQty.GetBindValue() == null)
            {
                txtReserveQty.SetValidation("SFU8275", tbReserveQty.Text);
                return;
            }

            int tryValue = 0;
            if (int.TryParse(txtReserveQty.Text, out tryValue))
            {
                if (tryValue < 1)
                {
                    txtReserveQty.Text = string.Empty;
                    txtReserveQty.SetValidation("SFU1802");
                    return;
                }
                else if (tryValue > 30)
                {
                    txtReserveQty.Text = string.Empty;
                    txtReserveQty.SetValidation("SFU1802");
                    return;
                }
            }
            else
            {
                txtReserveQty.Text = string.Empty;
                txtReserveQty.SetValidation("SFU2877");
                return;
            }

            //강재출고요청을 하시겠습니까?
            Util.MessageConfirm("FM_ME_0094", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.Columns.Add("AREAID", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("CNVR_LOCATION_ID", typeof(string));
                        dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));
                        dtRqst.Columns.Add("RESERV_CNT", typeof(int));

                        DataRow drRqst = dtRqst.NewRow();
                        drRqst["AREAID"] = LoginInfo.CFG_AREA_ID;
                        drRqst["USERID"] = LoginInfo.USERID;
                        drRqst["EQPTID"] = EQPTID;
                        drRqst["CNVR_LOCATION_ID"] = cboTargetLoc.GetBindValue();
                        drRqst["TRAY_TYPE_CODE"] = txtTrayType.Text;
                        drRqst["RESERV_CNT"] = txtReserveQty.Text.NvcInt();
                        dtRqst.Rows.Add(drRqst);

                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_UPD_EMPTY_CST_FORCE_RESERV_BY_LOCATION_ID", "RQSTDT", "RSLTDT", dtRqst);
                        if (dtResult != null && dtResult.Rows.Count > 0)
                        {
                            if (Util.NVC(dtResult.Rows[0]["RETVAL"]).Equals("1"))
                            {
                                Util.AlertInfo("FM_ME_0092");  //강제출고 요청을 완료하였습니다.
                            }
                            else
                            {
                                string msg = "[*]" + MessageDic.Instance.GetMessage("FM_ME_0091") + " - " + Util.NVC(dtResult.Rows[0]["RSLT_MSG"]);
                                Util.AlertInfo(msg);  //강제출고 요청에 실패하였습니다.
                            }
                        }
                       
                        this.DialogResult = MessageBoxResult.OK;
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

     
    }
}
