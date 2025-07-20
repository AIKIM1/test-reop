using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ZPL_VIEWER2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_FP_WEB : Window
    {
        public CMM_FP_WEB()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {

            AESEncrypter encrypter = new CMM001.Class.AESEncrypter();

            string userId = LoginInfo.USERID;
            string encrypterUserId = encrypter.Encrypt(userId);

            string postData = "UserID=" + encrypterUserId;
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            byte[] bytes = encoding.GetBytes(postData);
            string url = "http://gscm-battery.lgensol.com/ClickOnce/SSOMain.aspx";
            //string url = "http://165.244.95.220:8100/";


            webBrowser.Navigate(url, string.Empty, bytes, "Content-Type: application/x-www-form-urlencoded");
            
        }
    }
}