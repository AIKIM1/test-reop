/*************************************************************************************
 Created Date : 2024.11.11
      Creator : PJG
   Decription : 고온 Aging 온도 이탈 알람 조치내역 저장
--------------------------------------------------------------------------------------
 [Change History]
  2024.11.11  KDH : Initial Created
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

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_229_ACTSAVE : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _EqpName = string.Empty;
        private string _OcrTime = string.Empty;
        private string _ActionCntt = string.Empty;

        private string _FLOOR_CODE = string.Empty;
        private string _EQPT_GR_TYPE_CODE = string.Empty;
        private string _EQPT_ROW_LOC = string.Empty;
        private string _EQPT_COL_LOC = string.Empty;
        private string _EQPT_STG_LOC = string.Empty;
        private string _EQPT_ID = string.Empty;
        private string _RACK_ID = string.Empty;

        public FCS002_229_ACTSAVE()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps != null && tmps.Length >= 1)
            {
                _RACK_ID = Util.NVC(tmps[0]);
                DateTime dOcrTime = Convert.ToDateTime(tmps[1]);
                _OcrTime = dOcrTime.ToString("yyyy-MM-dd HH:mm:ss");
                _EqpName = Util.NVC(tmps[2]);
                _ActionCntt = Util.NVC(tmps[3]);

                _FLOOR_CODE = Util.NVC(tmps[4]);
                _EQPT_GR_TYPE_CODE = Util.NVC(tmps[5]);
                _EQPT_ROW_LOC = Util.NVC(tmps[6]);
                _EQPT_COL_LOC = Util.NVC(tmps[7]);
                _EQPT_STG_LOC = Util.NVC(tmps[8]);
                _EQPT_ID = Util.NVC(tmps[9]);
            }

            txtActdttm.Text = _OcrTime;
            txtRackID.Text = _EqpName;
            txtOperMaintContents.Text = _ActionCntt;
        }

        #endregion

        #region [Method]

        #endregion

        #region [Event]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ChkConfirmUserTmpr())
            {
                return;
            }

            SetAlarmAction();
        }

        private bool ChkConfirmUserTmpr()
        {
            string strResult = string.Empty;
            string InputID = txtUserID.Text;

            if (string.IsNullOrEmpty(InputID))
            {
                Util.MessageValidation("SFU1155");  // 사용자 ID를 입력해주세요.
                return false;
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("COM_CODE", typeof(string));
                dtRqst.Columns.Add("ATTR1", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
                dr["COM_CODE"] = "AGING_ACTION_MGR";
                dr["ATTR1"] = InputID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR_MULT_MB", "RQSTDT", "RSLTDT", dtRqst);


                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0594");  // 실행 권한이 없는 사용자 입니다.
                    return false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return true;
        }

        private void SetAlarmAction()
        {
            if (string.IsNullOrEmpty(txtOperMaintContents.Text))
            {
                Util.MessageValidation("FM_ME_0206");  //장애조치내역을 입력해주세요.
                return;
            }

            //변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0337", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("FLOOR_CODE", typeof(string));
                        dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));
                        dtRqst.Columns.Add("EQPT_ROW_LOC", typeof(string));
                        dtRqst.Columns.Add("EQPT_COL_LOC", typeof(string));
                        dtRqst.Columns.Add("EQPT_STG_LOC", typeof(string));
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("ACTION_MGR", typeof(string));
                        dtRqst.Columns.Add("WRKR_ACTION_CNTT", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["FLOOR_CODE"] = _FLOOR_CODE;
                        dr["EQPT_GR_TYPE_CODE"] = _EQPT_GR_TYPE_CODE;
                        dr["EQPT_ROW_LOC"] = _EQPT_ROW_LOC;
                        dr["EQPT_COL_LOC"] = _EQPT_COL_LOC;
                        dr["EQPT_STG_LOC"] = _EQPT_STG_LOC;
                        dr["EQPTID"] = _EQPT_ID;
                        dr["ACTION_MGR"] = Util.NVC(txtUserID.Text);
                        dr["WRKR_ACTION_CNTT"] = Util.NVC(txtOperMaintContents.Text);
                        if (string.IsNullOrEmpty(dr["WRKR_ACTION_CNTT"].ToString())) return;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_REG_AGING_ABNORMAL_TMPR_ACTION_CNTT", "RQSTDT", "RSLTDT", dtRqst);

                        Util.AlertInfo("FM_ME_0136");  //변경완료하였습니다.

                        DialogResult = MessageBoxResult.Yes;

                        Close();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });

        }

        #endregion

    }
}
