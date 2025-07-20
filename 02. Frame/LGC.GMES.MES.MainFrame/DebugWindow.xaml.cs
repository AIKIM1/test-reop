using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Data;
using System.IO;
using Microsoft.Win32;
using System.Configuration;
using System.IO.Ports;
using System.IO.IsolatedStorage;

using C1.WPF;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.MainFrame.ConfigWindows;
using LGC.GMES.MES.MainFrame.Security;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MainFrame
{
    /// <summary>
    /// ConfigWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DebugWindow : C1Window
    {
        #region Declaration & Constructor 

        public DebugWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DebugWindow_Loaded);
        }

        void DebugWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(DebugWindow_Loaded);

            string[] drives = System.Environment.GetLogicalDrives();
            txtDebug.Text = string.Join(",", drives);

            //IsolatedStorageFile isolatedStorage_W = IsolatedStorageFile.GetUserStoreForAssembly();
            //StreamWriter srWriter = new StreamWriter(new IsolatedStorageFileStream("isotest", FileMode.Create, isolatedStorage_W));

            //srWriter.WriteLine(System.DateTime.Now.ToLongTimeString());

            //srWriter.Flush();
            //srWriter.Close();


            txtDebug.Text = string.Empty;

            IsolatedStorageFile isolatedStorage_R = IsolatedStorageFile.GetUserStoreForAssembly();
            StreamReader srReader = new StreamReader(new IsolatedStorageFileStream("isotest", FileMode.OpenOrCreate, isolatedStorage_R));
            if (srReader == null)
            {
            }
            else
            {
                while (!srReader.EndOfStream)
                {
                    string item = srReader.ReadLine();
                    txtDebug.Text += item;
                }
            }
            srReader.Close();

            //OpenFileDialog fd = new OpenFileDialog();
            //fd.Filter = "Excel Files (.*)|*.*";
            //if (fd.ShowDialog() == true)
            //{
            //    txtDebug.Text = fd.FileName;
            //}
            //\\Client\C$\config.sys
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion
    }
}
