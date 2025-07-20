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
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    /// <summary>
    /// PGM_GUI_196_TEST_PRINT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PGM_GUI_196_TEST_PRINT : C1Window, IWorkArea
    {
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PGM_GUI_196_TEST_PRINT()
        {
            InitializeComponent();
        }
    }
}
