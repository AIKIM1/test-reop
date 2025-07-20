/*************************************************************************************
 Created Date : 2023.01.09
      Creator : 심찬보
   Decription : 오창 IT 3동 자동차 고전압 활성화 고온챔버 수동 착공
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
    public partial class FCS001_316_MANUAL_START : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]

        //private string sRow = string.Empty;
        //private string sCol = string.Empty;
        //private string sStg = string.Empty;
        private string sEqptid = string.Empty;
        private string sEqptname = string.Empty;


        #endregion

        #region [Initialize]
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS001_316_MANUAL_START()
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
                //sRow = Util.NVC(parameters[1]);
                //sCol = Util.NVC(parameters[2]);
                //sStg = Util.NVC(parameters[3]);
            }

            txtEqptName.Text = sEqptname;
            //HighChamberCol.Text = sCol;
            //HighChamberStg.Text = sStg;

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
            //입고처리 하시겠습니까?
            Util.MessageConfirm("SFU4589", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    try
                    {
                        if (string.IsNullOrEmpty(txt1stTrayId.Text.Trim()) && string.IsNullOrEmpty(txt2ndTrayId.Text.Trim()) && string.IsNullOrEmpty(txt3rdTrayId.Text.Trim()) && string.IsNullOrEmpty(txt4thTrayId.Text.Trim()))
                        {
                            Util.MessageValidation("FM_ME_0070"); //Tray ID를 입력해 주세요.
                            return;
                        }

                        else if (txt1stTrayId.Text.Trim() != string.Empty && txt1stTrayId.Text.Trim().Length != 10)
                        {
                            Util.MessageValidation("FM_ME_0580"); //1단 Tray ID를 정확히 입력해 주세요. 
                            return;
                        }
                        else if (txt2ndTrayId.Text.Trim() != string.Empty && txt2ndTrayId.Text.Trim().Length != 10)

                        {
                            Util.MessageValidation("FM_ME_0581"); //2단 Tray ID를 정확히 입력해 주세요.
                            return;
                        }

                        else if (txt3rdTrayId.Text.Trim() != string.Empty && txt3rdTrayId.Text.Trim().Length != 10)

                        {
                            Util.MessageValidation("FM_ME_0582"); //3단 Tray ID를 정확히 입력해 주세요.
                            return;
                        }

                        else if (txt4thTrayId.Text.Trim() != string.Empty && txt4thTrayId.Text.Trim().Length != 10)

                        {
                            Util.MessageValidation("FM_ME_0583"); //4단 Tray ID를 정확히 입력해 주세요.
                            return;
                        }

                        else
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
                            dr["CSTID"] = txt1stTrayId.Text.Trim();
                            dr["CSTID_LOADER1"] = txt2ndTrayId.Text.Trim();
                            dr["CSTID_LOADER2"] = txt3rdTrayId.Text.Trim();
                            dr["CSTID_LOADER3"] = txt4thTrayId.Text.Trim();
                            //dr["MANUAL_IN_TIME"] = DateTime.Now.ToString("yyyy-MM-dd") + DateTime.Now.ToString(" HH:mm:00");
                            dtRqst.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_FORM_MANUAL_TRAY_START_UI", "INDATA", "OUTDATA", dtRqst);

                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                this.DialogResult = MessageBoxResult.OK;
                                Util.AlertInfo("입고처리 완료하였습니다.");  //입고처리 완료하였습니다.
                            }
                            else
                            {
                                this.DialogResult = MessageBoxResult.No;
                                Util.AlertInfo("입고처리에 실패하였습니다.");  //입고처리에 실패하였습니다.
                            }
                        }
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
