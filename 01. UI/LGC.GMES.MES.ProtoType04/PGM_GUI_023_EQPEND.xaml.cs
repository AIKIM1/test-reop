using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    /// <summary>
    /// PGM_GUI_023_EQPEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_023_EQPEND : C1Window, IWorkArea
    {
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_023_EQPEND()
        {
            InitializeComponent();
        }
    }
}
