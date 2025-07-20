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

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_027_TROUBLE : C1Window, IWorkArea
    {

        #region [Declaration & Constructor]
        private DataRow drRslt;
        private DataTable dtParam;
        int rowidx = 0;
        public DataRow RST_ROW
        {
            get { return drRslt; }
            set { drRslt = value; }
        }
        public FCS001_027_TROUBLE()
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
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    dtParam = tmps[0] as DataTable;
                    SettingData();
                }
                //  if (LoginInfo.USERID.Equals("guest")) btnSetting.Enabled = false;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [Method]
        private void SettingData()
        {
            try
            {
                txtNum.Text = (rowidx + 1) + " / " + dtParam.Rows.Count;
                txtEqpName.Text = dtParam.Rows[rowidx]["EQPTNAME"].ToString();
                txtTroubleCd.Text = dtParam.Rows[rowidx]["EQPT_ALARM_CODE"].ToString();
                txtTroubleName.Text = dtParam.Rows[rowidx]["TROUBLE_NAME"].ToString();
                txtRepairType.Text = dtParam.Rows[rowidx]["EQPT_ALARM_ACTION_MTHD_CNTT"].ToString();
                txtOccurTime.Text = dtParam.Rows[rowidx]["INSDTTM"].ToString();

                if (rowidx == dtParam.Rows.Count - 1) btnNext.Visibility = Visibility.Collapsed;
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        #region [Event]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Util.MessageConfirm("FM_ME_0282", (result) => //설비 Trouble 내역을 정말 확인하셨습니까?
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("EQPTID", typeof(string));
                        dtRqst.Columns.Add("EQPT_ALARM_CODE", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        DataRow dr = dtRqst.NewRow();
                        dr["EQPTID"] = dtParam.Rows[rowidx]["EQPTID"].ToString();
                        dr["EQPT_ALARM_CODE"] = dtParam.Rows[rowidx]["EQPT_ALARM_CODE"].ToString();
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_TC_TROUBLE", "RQSTDT", "RSLDT", dtRqst);
                        if (rowidx != dtParam.Rows.Count - 1)
                        {
                            rowidx += 1;
                            SettingData();
                        }
                        else
                        {
                            DialogResult = MessageBoxResult.Yes;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });
        }
        #endregion

        private void btnNextClick(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            rowidx += 1;
            SettingData();
        }
    }
}
