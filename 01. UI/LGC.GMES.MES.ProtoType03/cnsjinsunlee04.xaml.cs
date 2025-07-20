using System.Windows.Controls;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType03
{
    /// <summary>
    /// cnsjinsunlee04.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnsjinsunlee04 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public cnsjinsunlee04()
        {
            InitializeComponent();
        }
    }
}
