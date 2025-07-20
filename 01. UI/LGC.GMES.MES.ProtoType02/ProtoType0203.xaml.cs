/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class ProtoType0203 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        Util _Util = new Util();

        private Storyboard sbExpand;
        private Storyboard sbCollapse;

        public ProtoType0203()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitControls();
        }

        #endregion

        #region Initialize

        private void InitControls()
        {
            sbExpand = (Storyboard)this.Resources["ExpandRightFrameStoryboard"];
            sbCollapse = (Storyboard)this.Resources["CollapseRightFrameStoryboard"];
        }

        #endregion

        #region Event

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

        //Sliding Control
        private void btnRightFrame_Checked(object sender, RoutedEventArgs e)
        {
            sbExpand.Begin();
        }

        private void btnRightFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            sbCollapse.Begin();
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        #endregion

        #region Mehod


        #endregion
    }
}
