/*************************************************************************************
 Created Date : 2020.06.24
      Creator : 손우석
   Decription : ZPL Preview
--------------------------------------------------------------------------------------
 [Change History]
 2020.06.24 손우석 SM용 ZPL Preview
**************************************************************************************/

using System;
using System.Text;
using System.Windows;
using System.Net;
using System.IO;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;


namespace LGC.GMES.MES.PACK001
{
    public partial class Preview : Window
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        private string strZPL = "";

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Preview()
        {
            InitializeComponent();
        }

        public Preview(string sLabel, string sZPL)
        {
            InitializeComponent();

            txtLabelCode.Text = sLabel;

            txtZPL.Text = sZPL;

            strZPL = sZPL;
        }

        #endregion

        #region Event

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                zplBrowser.Navigate("http://api.labelary.com/v1/printers/8dpmm/labels/10x10/0/" + strZPL);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
