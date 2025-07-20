using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    /// <summary>
    /// cnsjinsunlee03.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnsjinsunlee03 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        Util _Util = new Util();

        public cnsjinsunlee03()
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

        //추가 기능 버튼
        private void btnLot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBringOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBringOutFree_Click(object sender, RoutedEventArgs e)
        {

        }

   
    }
}
