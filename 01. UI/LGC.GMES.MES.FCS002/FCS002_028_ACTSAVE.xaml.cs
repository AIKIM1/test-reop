/*************************************************************************************
 Created Date : 2020.12.09
      Creator : PJG
   Decription : 장애조치내역 저장
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.09  PJG : Initial Created
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
    public partial class FCS002_028_ACTSAVE : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sTrayID = string.Empty;

        private string _EqpId = string.Empty;
        private string _EqpName = string.Empty;

        private string _OcrTime = string.Empty;
        private string _TranNo = string.Empty;
        private string _AlarmCode = string.Empty;

        public string EQPID
        {
            set { this._EqpId = value; }
        }

        public string OCRTIME
        {
            set { this._OcrTime = value; }
        }

        public FCS002_028_ACTSAVE()
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
                _EqpId = Util.NVC(tmps[0]);
                DateTime dOcrTime = Convert.ToDateTime(tmps[1]);
                _OcrTime = dOcrTime.ToString("yyyy-MM-dd HH:mm:ss");
                _EqpName = Util.NVC(tmps[2]);
                _TranNo = Util.NVC(tmps[3]);


            }
            else
            {
                _sTrayID = "";
            }

            txtActdttm.Text = _OcrTime;
            txtRackID.Text = _EqpName;

            //GetTroubleData();
        }

        #endregion

        #region [Method]

        #endregion

        #region [Event]

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //변경하시겠습니까?
            Util.MessageConfirm("FM_ME_0337", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("RACK_ID", typeof(string));
                        dtRqst.Columns.Add("REF_TRANSACTION_SERIAL_NO", typeof(string));
                        //dtRqst.Columns.Add("REF_ACTDTTM", typeof(string));
                        dtRqst.Columns.Add("FIRE_OCCR_ACTION_NOTE", typeof(string)); // 조치내역 컬럼 추가되면 수정
                        dtRqst.Columns.Add("USERID", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["RACK_ID"] = _EqpId;
                        dr["REF_TRANSACTION_SERIAL_NO"] =_TranNo;  // 2.0 변경에 따른 REF_ACTDTTM 변경
                        dr["FIRE_OCCR_ACTION_NOTE"] = Util.GetCondition(txtOperMaintContents, sMsg: "FM_ME_0206");  //장애조치내역을 입력해주세요.
                        if (string.IsNullOrEmpty(dr["FIRE_OCCR_ACTION_NOTE"].ToString())) return;
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_INS_TROUBLE_FIRE_HIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                        
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

        private void GetTroubleData()
        {
            //try
            //{
            //    DataTable dtRqst = new DataTable();
            //    dtRqst.TableName = "RQSTDT";
            //    dtRqst.Columns.Add("EQP_ID", typeof(string));
            //    dtRqst.Columns.Add("TROUBLE_OCCUR_TIME", typeof(string));

            //    DataRow dr = dtRqst.NewRow();
            //    dr["EQP_ID"] = _EqpId;
            //    dr["TROUBLE_OCCUR_TIME"] = _OcrTime; //"yyyy-MM-dd HH:mm:ss";
            //    dtRqst.Rows.Add(dr);

            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TROUBLE_HIST_INFO_MB", "RQSTDT", "RSLTDT", dtRqst);

            //    if (dtRslt.Rows.Count > 0)
            //    {
            //        txtTroubleCd.Text = dtRslt.Rows[0]["TROUBLE_CD"].ToString();
            //        txtTroubleGradeCd.Text = dtRslt.Rows[0]["TROUBLE_GRADE_CD"].ToString();
            //        txtTroubleName.Text = dtRslt.Rows[0]["TROUBLE_NAME"].ToString();
            //        txtTroubleOccurTime.Text = dtRslt.Rows[0]["TROUBLE_OCCUR_TIME"].ToString();
            //        txtTroubleRepairWay.Text = dtRslt.Rows[0]["TROUBLE_REPAIR_WAY"].ToString();
            //        txtOperMaintContents.Text = dtRslt.Rows[0]["OPER_MAINT_CONTENTS"].ToString();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        #endregion

    }
}
