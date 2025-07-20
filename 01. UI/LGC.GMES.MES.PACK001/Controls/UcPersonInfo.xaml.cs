/*************************************************************************************
 Created Date : 2020.11.10
      Creator : 김길용
   Decription : CWA 물류 - 창고모니터링 UserControl
--------------------------------------------------------------------------------------
 [Change History]
   2020.11.10   정용석  : Rack Stair UserControl 
**************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001.Controls
{
    public partial class UcPersonInfo : UserControl
    {
        #region Member Variable Lists...
        private string title;
        private string userID;
        private string userName;
        private string deptID;
        private string deptName;
        #endregion

        #region Constructor...
        public UcPersonInfo()
        {
            InitializeComponent();
        }

        public UcPersonInfo(string title)
        {
            InitializeComponent();
            this.txtTitle.Text = title;
        }
        #endregion

        #region Attributes...
        public IFrameOperation FrameOperation { get; set; }
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public string UserID
        {
            get
            {
                return this.userID;
            }
            set
            {
                this.userID = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
            set
            {
                this.userName = value;
            }
        }

        public string DeptID
        {
            get
            {
                return this.deptID;
            }
            set
            {
                this.deptID = value;
            }
        }

        public string DeptName
        {
            get
            {
                return this.deptName;
            }
            set
            {
                this.deptName = value;
            }
        }
        #endregion

        #region Member Function Lists...
        // 사용자 조회 입력 데이터 만들기
        private DataTable SetPersonData(string personName)
        {
            // Definition Schema of DataTable
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("USERNAME", typeof(string));
            dt.Columns.Add("LANGID", typeof(string));

            // Insert Data
            DataRow dr = dt.NewRow();
            dr["USERNAME"] = personName;
            dr["LANGID"] = LoginInfo.LANGID;
            dt.Rows.Add(dr);

            dt.AcceptChanges();     // Apply
            return dt;
        }
        // 사용자 조회
        private void SearchPersonData()
        {
            if (string.IsNullOrEmpty(this.txtUser.Text))
            {
                return;
            }

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "RQSTDT", "RLSTDT", this.SetPersonData(this.txtUser.Text));

            if (dt == null || dt.Rows.Count <= 0)
            {
                Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                return;
            }

            if (dt != null && dt.Rows.Count == 1)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    this.txtUser.Text = dr["USERNAME"].ToString();
                    //this.txtUserInfo.Text = "(" + dr["USERID"].ToString() + " / " + dr["DEPTNAME"].ToString() + ")";

                    this.userID = dr["USERID"].ToString();
                    this.userName = dr["USERNAME"].ToString();
                    this.deptID = dr["DEPTID"].ToString();
                    this.deptName = dr["DEPTNAME"].ToString();
                }
            }

            if (dt != null && dt.Rows.Count >= 2)
            {
                CMM_PERSON popup = new CMM_PERSON();
                popup.FrameOperation = FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = this.txtUser.Text;
                C1WindowExtension.SetParameters(popup, parameters);

                popup.Closed += new EventHandler(popup_Closed);
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }
        // Control Clear
        public void Clear()
        {
            this.txtUser.Text = string.Empty;
            this.UserID = string.Empty;
            this.userID = string.Empty;
            this.userName = string.Empty;
            this.UserName = string.Empty;
            this.deptID = string.Empty;
            this.deptName = string.Empty;
        }
        #endregion

        #region Event...
        // Search Button Click
        private void btnUserSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchPersonData();
        }

        // Popup Closed시
        private void popup_Closed(object sender, EventArgs e)
        {
            CMM_PERSON popup = sender as CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                this.txtUser.Text = popup.USERNAME;
                //this.txtUserInfo.Text = "(" + popup.USERID + " / " + popup.DEPTNAME + ")";

                this.userID = popup.USERID;
                this.userName = popup.USERNAME;
                this.deptID = popup.DEPTID;
                this.deptName = popup.DEPTNAME;
            }
            popup.Closed -= new EventHandler(popup_Closed);
        }

        // textBox Key Enter시
        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SearchPersonData();
            }
        }
        #endregion

        private void txtUser_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}