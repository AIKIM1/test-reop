/*************************************************************************************
 Created Date : 2020.10.20
      Creator : Kang Dong Hee
   Decription : 수동 OFF
--------------------------------------------------------------------------------------
 [Change History]
  2020.10.07  NAME : Initial Created
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
    public partial class FCS002_026_MANUAL_OFF : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        private string _sTrayID = string.Empty;

        public FCS002_026_MANUAL_OFF()
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
                _sTrayID = Util.NVC(tmps[0]);
            }
            else
            {
                _sTrayID = "";
            }
        }

        #endregion

        #region [Method]

        #endregion

        #region [Event]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtRemark.Text)) { Util.Alert("FM_ME_0117"); return; } //내용을 입력해주세요.
            Util.MessageConfirm("FM_ME_0337", (result) =>//상태를 변경하시겠습니까?
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string[] s = _sTrayID.Split(',');

                        for (int i = 0; i < s.Length; i++)
                        {

                            if (string.IsNullOrEmpty(s[i])) break;
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "RQSTDT";
                            dtRqst.Columns.Add("SRCTYPE", typeof(string));
                            dtRqst.Columns.Add("IFMODE", typeof(string));
                            dtRqst.Columns.Add("CSTID", typeof(string));
                            dtRqst.Columns.Add("NOTE", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));

                            DataRow dr = dtRqst.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["IFMODE"] = "OFF";
                            dr["CSTID"] = s[i];
                            dr["NOTE"] = txtRemark.Text;
                            dr["USERID"] = LoginInfo.USERID;

                            dtRqst.Rows.Add(dr);
                            try
                            {
                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_LOCATION_MANUAL_MB", "RQSTDT", "RSLTDT", dtRqst);
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                        this.DialogResult = MessageBoxResult.Yes;
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

    }
}
