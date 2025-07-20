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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_030 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public BOX001_030()
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

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCell_Info_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUnpack_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReprint2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCellID_Info_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgPack_lotChoice_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
