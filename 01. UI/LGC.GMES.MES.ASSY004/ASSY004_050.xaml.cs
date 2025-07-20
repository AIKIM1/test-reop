using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_050.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_050 : UserControl, IWorkArea
    {

        public ASSY004_050()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ASSY004_050_PROC ucProc = new ASSY004_050_PROC(this);
            ASSY004_050_SEL ucSel = new ASSY004_050_SEL(this);
            ASSY004_050_RWK_BOX ucRwkBox = new ASSY004_050_RWK_BOX(this);
            ASSY004_050_BOARD ucBoard = new ASSY004_050_BOARD(this);

            ucProc.FrameOperation = this.FrameOperation;
            ucSel.FrameOperation = this.FrameOperation;
            ucRwkBox.FrameOperation = this.FrameOperation;
            ucBoard.FrameOperation = this.FrameOperation;

            grdProc.Children.Add(ucProc);
            grdSel.Children.Add(ucSel);
            grdRwkBox.Children.Add(ucRwkBox);
            grdBoard.Children.Add(ucBoard);
        }
    }
}
