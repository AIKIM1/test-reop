/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ProtoType01
{
    public partial class ProtoType0104 : UserControl
    {
        #region Declaration & Constructor 

        private Storyboard sbExpandLeft;
        private Storyboard sbCollapseLeft;

        private Storyboard sbExpandRight;
        private Storyboard sbCollapseRight;


        public ProtoType0104()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            //Storyboard =======================================================================================
            sbExpandLeft = (Storyboard)this.Resources["ExpandLeftFrameStoryboard"];
            sbCollapseLeft = (Storyboard)this.Resources["CollapseLeftFrameStoryboard"];

            sbExpandRight = (Storyboard)this.Resources["ExpandRightFrameStoryboard"];
            sbCollapseRight = (Storyboard)this.Resources["CollapseRightFrameStoryboard"];

            TextBox txt = new TextBox();

            Grid.SetRow(txt, 1);
        }

        #endregion

        #region Event

        private void btnLeftFrame_Checked(object sender, RoutedEventArgs e)
        {
            sbExpandLeft.Begin();
        }

        private void btnLeftFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            sbCollapseLeft.Begin();
        }

        private void btnrightFrame_Checked(object sender, RoutedEventArgs e)
        {
            sbExpandRight.Begin();
        }

        private void btnrightFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            sbCollapseRight.Begin();
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            grAnimation.RowDefinitions[1].Height = new GridLength(0);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(1, GridUnitType.Star);
            gla.To = new GridLength(0, GridUnitType.Star); ;
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            grAnimation.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            grAnimation.RowDefinitions[1].Height = new GridLength(8);
            LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla.From = new GridLength(0);
            gla.To = new GridLength(1);
            gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
            grAnimation.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla);
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
