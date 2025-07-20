using C1.WPF;
using LGC.GMES.MES.Common;
using System;

namespace LGC.GMES.MES.MNT001
{
    /// <summary>
    /// MNT001_004.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MNT001_004 : C1Window, IWorkArea
    {
        #region Initialize
        public MNT001_004()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            string url = "http://165.244.95.222:8100/install.jsp?str_gmes_id=GMES_00&str_gmes_pw=gmesview!";
            System.Diagnostics.Process.Start(url);

            this.Close();
        }
        #endregion
    }
}
