/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_014_USERSELECT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string BUTTONNAME
        {
            get
            {
                return sButtonName;
            }

            set
            {
                sButtonName = value;
            }
        }

        public string USERID
        {
            get
            {
                return sUserId;
            }

            set
            {
                sUserId = value;
            }
        }
        
        public string USERNAME
        {
            get
            {
                return sUserName;
            }

            set
            {
                sUserName = value;
            }
        }

        public string DEPTNAME
        {
            get
            {
                return sDeptName;
            }

            set
            {
                sDeptName = value;
            }
        }

        public string POSITION
        {
            get
            {
                return sPosition;
            }

            set
            {
                sPosition = value;
            }
        }

        private DataView dvRootNodes;
        private string sButtonName = "";
        private string sUserId = "";
        private string sUserName = "";
        private string sDeptName = "";
        private string sPosition = "";

        public PACK001_014_USERSELECT()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {               
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        sButtonName = dtText.Rows[0]["BUTTONNAME"].ToString();
                        getUser(LoginInfo.USERID, LoginInfo.USERNAME);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgUserList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgUserList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgUserList.Rows[cell.Row.Index].DataItem, "CHK", "1");
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               
                if (!(sUserId.Length > 0))
                {
                    ms.AlertInfo("SFU1842"); //작업자를 선택 하세요.
                    return;
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgUserListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                int iwoUserListIndex = Util.gridFindDataRow(ref dgUserList, "CHK", "True", false);
                if (iwoUserListIndex == -1)
                {
                    return;
                }
                USERID = Util.NVC(DataTableConverter.GetValue(dgUserList.Rows[iwoUserListIndex].DataItem, "USERID"));                
                USERNAME = Util.NVC(DataTableConverter.GetValue(dgUserList.Rows[iwoUserListIndex].DataItem, "USERNAME"));
                DEPTNAME = Util.NVC(DataTableConverter.GetValue(dgUserList.Rows[iwoUserListIndex].DataItem, "DEPTNAME")); 
                POSITION = Util.NVC(DataTableConverter.GetValue(dgUserList.Rows[iwoUserListIndex].DataItem, "POSITION"));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
       
        #endregion

        #region Mehod

        private void getUser(string userId, string userName)
        {
            try
            {                         
                string USERNAME = sUserName;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("USERNAME", typeof(string));             
                    

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = userId;
                dr["USERNAME"] = userName;              
               
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USERINFOR", "RQSTDT", "RSLTDT", RQSTDT);

                dgUserList.ItemsSource = null;

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgUserList, dtResult, FrameOperation, true);
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //private void txtUser_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    try
        //    {               
        //        string userId = "";
        //        string userName = "";
                
        //        userId = Util.GetCondition(txtUserID);  
        //        userName = Util.GetCondition(txtUserName);               

        //        getUser(userId, userName);

        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        private void btnUserSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string userId = "";
                string userName = "";

                userId = Util.GetCondition(txtUserID);
                userName = Util.GetCondition(txtUserName);

                getUser(userId, userName);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void userSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    string userId = "";
                    string userName = "";

                    userId = Util.GetCondition(txtUserID);
                    userName = Util.GetCondition(txtUserName);

                    getUser(userId, userName);
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
