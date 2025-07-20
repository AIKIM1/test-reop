using System;
using System.Windows;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.Updater
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

            //Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;
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
                            LoginInfo.SYSID = splititem[1];
                            break;

                        case "U":
                            LoginInfo.USERID = splititem[1];
                            break;
                    }
                }
            }
        }
    }
}