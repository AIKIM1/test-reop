using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    /// <summary>
    /// cnsjinsunlee05.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnsjinsunlee05 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        Util _Util = new Util();

        public cnsjinsunlee05()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitControls();
        }
        private void InitControls()
        {
        }



    }
}
