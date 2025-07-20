using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ZPL_VIEWER2.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ZPL_VIEWER2 : Window
    {
        public CMM_ZPL_VIEWER2()
        {
            InitializeComponent();

            //zplBrowser.Navigate("http://www.naver.com");
        }

        public CMM_ZPL_VIEWER2(string sZPL)
        {
            InitializeComponent();
            textBox.Text = sZPL;

            zplBrowser.Navigate("http://api.labelary.com/v1/printers/8dpmm/labels/10x10/0/"+ sZPL);
        }
    }
}
