using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_092_FULLSCREEN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_092_FULLSCREEN : C1Window, IWorkArea
    {
        public COM001_092_FULLSCREEN()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public void AddElements(List<C1DataGrid> dataGrids, List<TextBlock> textPanels)
        {
        }
    }
}
