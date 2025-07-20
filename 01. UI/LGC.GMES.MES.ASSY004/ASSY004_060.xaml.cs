/*************************************************************************************
 Created Date : 2019.10.28
      Creator : 정문교
   Decription : FOL,STK Rework 
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.28  정문교 : Initial Created.   폴란드3동 & 빈강용 조립 공정에서 사용
                                          ASSY004_050 Copy ASSY004_060

**************************************************************************************/

using LGC.GMES.MES.Common;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_060.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_060 : UserControl, IWorkArea
    {
        public ASSY004_060()
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
            ASSY004_060_PROC ucProc = new ASSY004_060_PROC(this);
            // 재작업 BOX 출고
            ASSY004_060_RWK_BOX ucRwkBox = new ASSY004_060_RWK_BOX(this);

            ucProc.FrameOperation = this.FrameOperation;
            ucRwkBox.FrameOperation = this.FrameOperation;

            grdProc.Children.Add(ucProc);
            grdRwkBox.Children.Add(ucRwkBox);

            this.Loaded -= UserControl_Loaded;
        }
    }
}
