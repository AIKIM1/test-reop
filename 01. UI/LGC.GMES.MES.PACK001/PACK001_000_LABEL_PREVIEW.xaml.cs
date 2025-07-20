using LGC.GMES.MES.CMM001.Class;
using System;
using System.IO;
using System.Net;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// ZplView.xaml에 대한 상호 작용 논리
    /// </summary >
    public partial class PACK001_000_LABEL_PREVIEW : Window
    {
        string strUrl = string.Empty;

        public PACK001_000_LABEL_PREVIEW()
        {
            InitializeComponent();
        }

        public PACK001_000_LABEL_PREVIEW(string url, string zplText)
        {

            try
            {
                strUrl = url;
                //textBox.Text = "";
               // textBox.TextChanged -= textBox_TextChanged;

                string zplReplace;
                InitializeComponent();

                textBox.Text = zplText;
                zplReplace = zplText;
                if (zplText.Contains("%") || zplText.Contains("#"))
                {
                    zplReplace = zplText.Replace("%", "%25");
                    zplReplace = zplReplace.Replace("#", "%23");
                }

                zplBrowser.Navigate(url + zplReplace);
                //textBox.TextChanged += textBox_TextChanged;
            }
            catch (WebException ex)
            {
                Util.MessageException(ex);
            }           
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                string zplReplace;
                string zplText = string.Empty;

                zplText = textBox.Text;

                if (zplText.Equals("TextBox") || zplText.Equals(null) || zplText.Equals("")) return;
                zplReplace = zplText;
                if (zplText.Contains("%") || zplText.Contains("#"))
                {
                    zplReplace = zplText.Replace("%", "%25");
                    zplReplace = zplReplace.Replace("#", "%23");
                }

                zplBrowser.Navigate(strUrl + zplReplace);

            }
            catch (WebException ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
