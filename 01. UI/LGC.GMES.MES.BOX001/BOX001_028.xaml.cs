/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_028 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public BOX001_028()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);           
        }

        #endregion

        #region Initialize
        private void Initialize()
        {          

        }





        #endregion

        private void btnBoxLabelPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnShift_Main_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
