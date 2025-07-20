/*************************************************************************************
 Created Date : 2023.01.09
      Creator : 심찬보
   Decription : 오창 IT 3동 자동차 고전압 활성화 고온챔버 수동 완공
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.09  DEVELOPER : Initial Created.
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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_316_MANUAL_END : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sEqptid = string.Empty;
        private string sEqptname = string.Empty;
        private string sTray1 = string.Empty;
        private string sTray2 = string.Empty;
        private string sTray3 = string.Empty;
        private string sTray4 = string.Empty;


        #endregion

        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_316_MANUAL_END ()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, EventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters != null && parameters.Length >= 1)
            {

                sEqptid = Util.NVC(parameters[0]);
                sEqptname = Util.NVC(parameters[1]);
                sTray1 = Util.NVC(parameters[2]);
                sTray2 = Util.NVC(parameters[3]);
                sTray3 = Util.NVC(parameters[4]);
                sTray4 = Util.NVC(parameters[5]);
            }

            txtEqptName.Text = sEqptname;
            txt1stTrayId.Text = sTray1;
            txt2ndTrayId.Text = sTray2;
            txt3rdTrayId.Text = sTray3;
            txt4thTrayId.Text = sTray4;

            dtpDate.SelectedDateTime = DateTime.Now;
            dtpTime.DateTime = DateTime.Now;

            dtpDate.IsEnabled = false;
            dtpTime.IsEnabled = false;

        }

        #endregion

        #region [Method]

        #endregion

        #region [Event]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //출고처리 하시겠습니까?
            Util.MessageConfirm("FM_ME_0574", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    try
                    {

                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "INDATA";
                            dtRqst.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst.Columns.Add("IFMODE", typeof(string));
                            dtRqst.Columns.Add("AREAID", typeof(string));
                            dtRqst.Columns.Add("EQPTID", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));
                            dtRqst.Columns.Add("CSTID", typeof(string));
                            dtRqst.Columns.Add("CSTID_LOADER1", typeof(string));
                            dtRqst.Columns.Add("CSTID_LOADER2", typeof(string));
                            dtRqst.Columns.Add("CSTID_LOADER3", typeof(string));
                        //dtRqst.Columns.Add("MANUAL_IN_TIME", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "OFF";
                            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                            dr["EQPTID"] = sEqptid;
                            dr["USERID"] = LoginInfo.USERID;
                            dr["CSTID"] = sTray1;
                            dr["CSTID_LOADER1"] = sTray2;
                            dr["CSTID_LOADER2"] = sTray3;
                            dr["CSTID_LOADER3"] = sTray4;
                        //dr["MANUAL_IN_TIME"] = DateTime.Now.ToString("yyyy-MM-dd") + DateTime.Now.ToString(" HH:mm:00");
                        dtRqst.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_FORM_MANUAL_TRAY_END_UI", "INDATA", "OUTDATA", dtRqst);

                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                this.DialogResult = MessageBoxResult.OK;
                                Util.AlertInfo("FM_ME_0575");  //출고처리 완료하였습니다.
                            }
                            else
                            {
                                this.DialogResult = MessageBoxResult.No;
                                Util.AlertInfo("FM_ME_0576");  //출고처리에 실패하였습니다.
                            }
                        //}
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
            //Close();
        }

        ///// <summary>
        ///// 자동입력 선택 시 현재시간으로 자동입력
        ///// </summary>
        ///// <param name = "sender" ></ param >
        ///// < param name="e"></param>
        //private void chkNotJudgCell_CheckedChanged(object sender, EventArgs e)
        //{
        //    dtpDate.SelectedDateTime = DateTime.Now;
        //    dtpTime.DateTime = DateTime.Now;

        //    dtpDate.IsEnabled = false;
        //    dtpTime.IsEnabled = false;
        //}

        //private void chkNotJudgCell_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    dtpDate.IsEnabled = true;
        //    dtpTime.IsEnabled = true;
        //}

        #endregion

    }
}
