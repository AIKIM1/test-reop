using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows;

namespace LGC.GMES.MES.Updater
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(MainWindow_Loaded);

            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + ConfigurationManager.AppSettings["STARTAPP"];
            Process.Start(filename, Common.LoginInfo.PARAM);
            this.Close();

            //new ClientProxy().ExecuteService("COR_SEL_LANGUAGE", null, "RSLTDT", null, (result, ex) =>
            //{
            //    if (ex != null)
            //    {
            //        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    string filename = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + ConfigurationManager.AppSettings["STARTAPP"];
            //    Process.Start(filename, Common.LoginInfo.PARAM);
            //    this.Close();
            //});
        }
    }
}