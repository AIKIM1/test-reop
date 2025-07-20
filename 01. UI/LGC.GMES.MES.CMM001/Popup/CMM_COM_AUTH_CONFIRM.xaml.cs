using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.DirectoryServices.AccountManagement;
using System.Windows;
using System.Data;
using System;
using System.Web.Hosting;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_AUTH_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_AUTH_CONFIRM : C1Window, IWorkArea
    {
        private string DOMAIN_NAME = "lgcgmes.net";
        //private const string DOMAIN_NAME = "lgchem.com";
        //private string DOMAIN_NAME = "lgcgmesd.net";
        private string _AUTHGROUP = string.Empty;

        public IFrameOperation FrameOperation { get; set; }
        //20181122 오화백 추가 : 사용자 ID를 리턴 받기 위해 추가
        public string UserID { get; set; }
        public string sContents { get; set; }

        public CMM_COM_AUTH_CONFIRM()
        {
            InitializeComponent();
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
                return;

            _AUTHGROUP = Util.NVC(tmps[0]);

            if (string.IsNullOrEmpty(_AUTHGROUP))
                return;

            // SGC AD, 화학 AD 구분을 위하여 추가
            if (tmps.Length == 2)
                DOMAIN_NAME = Util.NVC(tmps[1]);

            if (!string.IsNullOrWhiteSpace(sContents))
                txtContents.Text = sContents;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserID.Text.Trim()))
            {
                Util.MessageValidation("SFU1155");  //사용자 ID를 입력하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtPassWord.Password))
            {
                Util.MessageValidation("SFU1156"); //비밀번호를 입력하세요.
                return;
            }

            if (IsPersonByAuth(txtUserID.Text) == false)
            {
                Util.MessageValidation("SFU3520", txtUserID.Text, GetAuthName());  //해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                return;
            }

            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, DOMAIN_NAME))
                {
                    using (HostingEnvironment.Impersonate())
                    {
                        bool isValid = pc.ValidateCredentials(txtUserID.Text, txtPassWord.Password, ContextOptions.Negotiate | ContextOptions.Signing | ContextOptions.Sealing);

                        if (isValid == false)
                        {
                            Util.MessageValidation("SFU3188");  //비밀번호를 확인하세요.
                            return;
                        }

                        UserID = txtUserID.Text;
                        this.DialogResult = MessageBoxResult.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private bool IsPersonByAuth(string sUserID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = sUserID;
                dr["AUTHID"] = _AUTHGROUP;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) {}

            return false;
        }

        private string GetAuthName()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AUTHID"] =   _AUTHGROUP.Split(',')[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTHNAME", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AUTHNAME"]);
            }
            catch (Exception ex) { }

            return _AUTHGROUP;
        }
    }
}
