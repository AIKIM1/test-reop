/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_076_LOTCHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }



        public PGM_GUI_076_LOTCHANGE()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void chkLotScrap_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void chkLotScrap_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion

        #region Mehod

        #endregion


    }
}
