/*************************************************************************************
 Created Date : 2021.12.14
      Creator : 오화백
   Decription : STK Rework  
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using LGC.GMES.MES.Common;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_060.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_061 : UserControl, IWorkArea
    {
        public ASSY004_061()
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
            // 공정진척
            ASSY004_061_PROC ucProc = new ASSY004_061_PROC(this);
            // 재작업 BOX 출고
            ASSY004_061_RWK_BOX ucRwkBox = new ASSY004_061_RWK_BOX(this);

            ucProc.FrameOperation = this.FrameOperation;
            ucRwkBox.FrameOperation = this.FrameOperation;

            grdProc.Children.Add(ucProc);
            grdRwkBox.Children.Add(ucRwkBox);

            this.Loaded -= UserControl_Loaded;
        }
    }
}
