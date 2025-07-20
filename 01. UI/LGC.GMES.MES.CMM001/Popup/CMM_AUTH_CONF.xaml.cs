/*************************************************************************************
 Created Date : 2023.05.09
      Creator : 조영대
   Decription : 텍스트 입력
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.09  조영대 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_AUTH_CONF : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string topParentName = string.Empty;
        private System.Windows.Controls.Button button = null;

        private DataTable dtAuth = null;
        private DataTable dtAuthYes = null;
        private DataTable dtAuthNo = null;
        
        public CMM_AUTH_CONF()
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
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                button = tmps[0] as System.Windows.Controls.Button;
                topParentName = Util.NVC(tmps[1]);
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("WORK_TYPE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["WORK_TYPE"] = "ALL";
            dtRqst.Rows.Add(dr);

            dgAuthList.ExecuteService("BR_GET_AUTHORITY_INFO", "RQSTDT", "RSLTDT", dtRqst, true);
        }

        private void dgAuthList_ExecuteDataCompleted(object sender, Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            dtAuth = e.ResultData as DataTable;
            dtAuthYes = dtAuth.Clone();
            dtAuthNo = dtAuth.Clone();

            DataRow drAuth = dtAuth.AsEnumerable().Where(x => x.Field<string>("AUTHID").Equals("MESADMIN")).FirstOrDefault();
            drAuth.Delete();
            drAuth.AcceptChanges();

            DataView dvAuth = dtAuth.DefaultView;
            dvAuth.Sort = "AUTHID";
            dgAuthList.ItemsSource = dvAuth;

            DataView dvAuthYes = dtAuthYes.DefaultView;
            dvAuthYes.Sort = "AUTHID";
            dgAuthYesList.ItemsSource = dvAuthYes;

            DataView dvAuthNo = dtAuthNo.DefaultView;
            dvAuthNo.Sort = "AUTHID";
            dgAuthNoList.ItemsSource = dvAuthNo;

            GetUserConfig();
        }

        private void btnAllowYesIn_Click(object sender, RoutedEventArgs e)
        {
            if (dgAuthList.SelectedIndex < 0) return;

            string authID = Util.NVC(dgAuthList.GetValue("AUTHID"));
            DataRow drAuth = dtAuth.AsEnumerable().Where(x => x.Field<string>("AUTHID").Equals(authID)).FirstOrDefault();
            if (drAuth != null)
            {
                // MES 2.0 ItemArray 위치 오류 Patch
                //dtAuthYes.Rows.Add(drAuth.ItemArray);
                dtAuthYes.AddDataRow(drAuth);
                dtAuthYes.AcceptChanges();

                drAuth.Delete();
                drAuth.AcceptChanges();
            }

            SetRadioButton();
        }

        private void btnAllowYesOut_Click(object sender, RoutedEventArgs e)
        {
            if (dgAuthYesList.SelectedIndex < 0) return;

            string authID = Util.NVC(dgAuthYesList.GetValue("AUTHID"));
            DataRow drAuthYes = dtAuthYes.AsEnumerable().Where(x => x.Field<string>("AUTHID").Equals(authID)).FirstOrDefault();
            if (drAuthYes != null)
            {
                // MES 2.0 ItemArray 위치 오류 Patch
                //dtAuth.Rows.Add(drAuthYes.ItemArray);
                dtAuth.AddDataRow(drAuthYes);
                dtAuth.AcceptChanges();

                drAuthYes.Delete();
                drAuthYes.AcceptChanges();
            }

            SetRadioButton();
        }

        private void btnAllowNoIn_Click(object sender, RoutedEventArgs e)
        {
            if (dgAuthList.SelectedIndex < 0) return;

            string authID = Util.NVC(dgAuthList.GetValue("AUTHID"));
            DataRow drAuth = dtAuth.AsEnumerable().Where(x => x.Field<string>("AUTHID").Equals(authID)).FirstOrDefault();
            if (drAuth != null)
            {
                // MES 2.0 ItemArray 위치 오류 Patch
                //dtAuthNo.Rows.Add(drAuth.ItemArray);
                dtAuthNo.AddDataRow(drAuth);
                dtAuthNo.AcceptChanges();

                drAuth.Delete();
                drAuth.AcceptChanges();
            }

            SetRadioButton();
        }

        private void btnAllowNoOut_Click(object sender, RoutedEventArgs e)
        {
            if (dgAuthNoList.SelectedIndex < 0) return;

            string authID = Util.NVC(dgAuthNoList.GetValue("AUTHID"));
            DataRow drAuthNo = dtAuthNo.AsEnumerable().Where(x => x.Field<string>("AUTHID").Equals(authID)).FirstOrDefault();
            if (drAuthNo != null)
            {
                // MES 2.0 ItemArray 위치 오류 Patch
                //dtAuth.Rows.Add(drAuthNo.ItemArray);
                dtAuth.AddDataRow(drAuthNo);
                dtAuth.AcceptChanges();

                drAuthNo.Delete();
                drAuthNo.AcceptChanges();
            }

            SetRadioButton();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtAuthYes.Rows.Count > 0 || dtAuthNo.Rows.Count > 0)
                {
                    SaveUserConfig();
                }
                else
                {
                    DeleteUserConfig();
                }
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void GetUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = "SELECT";
                dr["USERID"] = "MESADMIN";
                dr["CONF_TYPE"] = "BUTTON_AUTHRITY_CONFIG";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = button.Name;
                dr["CONF_KEY3"] = "NULL";
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    foreach (DataRow drConf in dtResult.Rows)
                    {
                        string confYesString = Util.NVC(drConf["USER_CONF01"]) + Util.NVC(drConf["USER_CONF02"]) + Util.NVC(drConf["USER_CONF03"]) + Util.NVC(drConf["USER_CONF04"]);

                        List<string> confYesColumn = confYesString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                        foreach (string authID in confYesColumn)
                        {
                            DataRow drAuth = dtAuth.AsEnumerable().Where(x => x.Field<string>("AUTHID").Equals(authID)).FirstOrDefault();
                            if (drAuth != null)
                            {
                                // MES 2.0 ItemArray 위치 오류 Patch
                                //dtAuthYes.Rows.Add(drAuth.ItemArray);
                                dtAuthYes.AddDataRow(drAuth);

                                drAuth.Delete();
                                drAuth.AcceptChanges();
                            }
                        }
                        dtAuthYes.AcceptChanges();

                        string confNoString = Util.NVC(drConf["USER_CONF05"]) + Util.NVC(drConf["USER_CONF06"]) + Util.NVC(drConf["USER_CONF07"]) + Util.NVC(drConf["USER_CONF08"]);

                        List<string> confNoColumn = confNoString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                        foreach (string authID in confNoColumn)
                        {
                            DataRow drAuth = dtAuth.AsEnumerable().Where(x => x.Field<string>("AUTHID").Equals(authID)).FirstOrDefault();
                            if (drAuth != null)
                            {
                                // MES 2.0 ItemArray 위치 오류 Patch
                                //dtAuthNo.Rows.Add(drAuth.ItemArray);
                                dtAuthNo.AddDataRow(drAuth);

                                drAuth.Delete();
                                drAuth.AcceptChanges();
                            }
                        }
                        dtAuthNo.AcceptChanges();

                        SetRadioButton();

                        if (Util.NVC(drConf["USER_CONF09"]).Equals("Y"))
                        {
                            rdoRegOutYes.IsChecked = true;
                        }
                        else
                        {
                            rdoRegOutNo.IsChecked = true;
                        }

                        if (Util.NVC(drConf["USER_CONF10"]).Equals("DISABLE"))
                        {
                            rdoProcessDisable.IsChecked = true;
                        }
                        else if (Util.NVC(drConf["USER_CONF10"]).Equals("VISIBLE"))
                        {
                            rdoProcessVisible.IsChecked = true;
                        }
                        else
                        {
                            rdoProcessMsg.IsChecked = true;
                        }

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SaveUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));
                dtRqst.Columns.Add("USER_CONF01", typeof(string));
                dtRqst.Columns.Add("USER_CONF02", typeof(string));
                dtRqst.Columns.Add("USER_CONF03", typeof(string));
                dtRqst.Columns.Add("USER_CONF04", typeof(string));
                dtRqst.Columns.Add("USER_CONF05", typeof(string));
                dtRqst.Columns.Add("USER_CONF06", typeof(string));
                dtRqst.Columns.Add("USER_CONF07", typeof(string));
                dtRqst.Columns.Add("USER_CONF08", typeof(string));
                dtRqst.Columns.Add("USER_CONF09", typeof(string));
                dtRqst.Columns.Add("USER_CONF10", typeof(string));

                StringBuilder saveAuthYes = new StringBuilder();
                foreach (DataRow drAuthYes in dtAuthYes.Rows)
                {
                    saveAuthYes.Append(Util.NVC(drAuthYes["AUTHID"]) + ",");
                }

                StringBuilder saveAuthNo = new StringBuilder();
                foreach (DataRow drAuthNo in dtAuthNo.Rows)
                {
                    saveAuthNo.Append(Util.NVC(drAuthNo["AUTHID"]) + ",");
                }

                DataRow drNew = dtRqst.NewRow();
                drNew["WRK_TYPE"] = "SAVE";
                drNew["USERID"] = "MESADMIN";
                drNew["CONF_TYPE"] = "BUTTON_AUTHRITY_CONFIG";
                drNew["CONF_KEY1"] = topParentName;
                drNew["CONF_KEY2"] = button.Name;
                drNew["CONF_KEY3"] = "NULL";
                drNew["USER_CONF01"] = GetConfString(saveAuthYes);
                drNew["USER_CONF02"] = GetConfString(saveAuthYes);
                drNew["USER_CONF03"] = GetConfString(saveAuthYes);
                drNew["USER_CONF04"] = GetConfString(saveAuthYes);
                drNew["USER_CONF05"] = GetConfString(saveAuthNo);
                drNew["USER_CONF06"] = GetConfString(saveAuthNo);
                drNew["USER_CONF07"] = GetConfString(saveAuthNo);
                drNew["USER_CONF08"] = GetConfString(saveAuthNo);
                drNew["USER_CONF09"] = rdoRegOutYes.IsChecked.Equals(true) ? "Y" : "N";
                
                string processMethod = "MESSAGE";
                if (rdoProcessDisable.IsChecked.Equals(true))
                {
                    processMethod = "DISABLE";
                }
                else if (rdoProcessVisible.IsChecked.Equals(true))
                {
                    processMethod = "VISIBLE";
                }
                    
                drNew["USER_CONF10"] = processMethod;
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null)
                {
                    Util.MessageInfo("SFU3532", result =>
                    {
                        if (result == MessageBoxResult.OK) { }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteUserConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(topParentName)) return;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("WRK_TYPE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("CONF_TYPE", typeof(string));
                dtRqst.Columns.Add("CONF_KEY1", typeof(string));
                dtRqst.Columns.Add("CONF_KEY2", typeof(string));
                dtRqst.Columns.Add("CONF_KEY3", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["WRK_TYPE"] = "DELETE";
                dr["USERID"] = "MESADMIN";
                dr["CONF_TYPE"] = "BUTTON_AUTHRITY_CONFIG";
                dr["CONF_KEY1"] = topParentName;
                dr["CONF_KEY2"] = button.Name;
                dr["CONF_KEY3"] = "NULL";
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_USER_CONF_INFO", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private string GetConfString(StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) return null;

            string returnString = string.Empty;

            if (sb.Length > 4000)
            {
                returnString = sb.ToString().Substring(0, 4000);
                sb.Remove(0, 4000);
            }
            else
            {
                returnString = sb.ToString();
                sb.Remove(0, sb.Length);
            }

            return returnString;
        }

        private void SetRadioButton()
        {
            rdoRegOutNo.IsEnabled = rdoRegOutYes.IsEnabled = false;
            rdoRegOutNo.IsChecked = rdoRegOutYes.IsChecked = false;

            if (dtAuthYes.Rows.Count > 0) rdoRegOutNo.IsChecked = rdoRegOutNo.IsEnabled = true;
            if (dtAuthNo.Rows.Count > 0) rdoRegOutYes.IsChecked = rdoRegOutYes.IsEnabled = true;
        }
        #endregion
    }
}
