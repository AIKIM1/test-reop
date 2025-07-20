using System;
using System.Windows;
using LGC.GMES.MES.Common;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.MainFrame
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory.ToString();

            if (System.Diagnostics.Debugger.IsAttached)
                if (Environment.CurrentDirectory.Contains("\\bin"))
                    Environment.CurrentDirectory = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf("\\bin"));

            this.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);
            //this.StartupUri = new System.Uri("Login.xaml", System.UriKind.Relative);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                //SBC 입력 파라미터 처리 ==============================================================
                LoginInfo.PARAM = e.Args[0];
                string[] split = Convert.ToString(e.Args[0]).Split(';');

                foreach (string item in split)
                {
                    string[] splititem = item.Split(':');

                    switch (splititem[0].ToUpper())
                    {
                        case "SYS":
                            //LoginInfo.SYSID = splititem[1];
                            LoginInfo.SYSID = splititem[1].EndsWith("K2") ? splititem[1] + "R" : splititem[1]; //오창2공장 NFF 는 SYSID+R 사용
                            break;

                        case "SYSSUB":
                            LoginInfo.SYSIDSUB = splititem[1];
                            break;

                        case "U":
                            //LoginInfo.USERID = splititem[1].Replace("g.", string.Empty);
                            LoginInfo.USERID = splititem[1].StartsWith("g.") == true ? new Regex("g.").Replace(splititem[1], string.Empty, 1) : splititem[1];
                            break;
                        case "MENU":
                            LoginInfo.CTX_MENUID = splititem[1];
                            break;
                    }
                }

                if (string.IsNullOrEmpty(LoginInfo.SYSIDSUB))
                {
                    LoginInfo.SYSIDSUB = Common.LoginInfo.SYSID;
                }
                else if (LoginInfo.SYSIDSUB.Equals("개발"))
                {
                    LoginInfo.SYSIDSUB = Common.LoginInfo.SYSID + "-개발";
                }
                else if (LoginInfo.SYSIDSUB.Equals("실전"))
                {
                    LoginInfo.SYSIDSUB = Common.LoginInfo.SYSID + "-실전";
                }
                else if (LoginInfo.SYSIDSUB.Equals("운영"))
                {
                    LoginInfo.SYSIDSUB = Common.LoginInfo.SYSID + "-운영";
                }

                //Common.Common.DeploymentUrl = e.Args[0];
            }
        }

        public bool DoHandle { get; set; }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (this.DoHandle)
            {
                //Handling the exception within the UnhandledException handler.
                MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            else
            {
                //If you do not set e.Handled to true, the application will close due to crash.
                System.Text.StringBuilder sbmsg = new System.Text.StringBuilder();
                string methodName = new System.Diagnostics.StackFrame(0, true).GetMethod().Name;
                string[] split = Convert.ToString(e.Exception.StackTrace).Split('\r');
                int cnt = 0;

                foreach (string item in split)
                {
                    sbmsg.Append(item);
                    cnt++;

                    if (cnt > 2)
                        break;
                }
            
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(sbmsg.ToString(), null, "Exception Caught", MessageBoxButton.OK, LGC.GMES.MES.ControlsLibrary.MessageBoxIcon.Warning);
                //MessageBox.Show(e.Exception.Message);
                e.Handled = true;
            }
        }
    }
}