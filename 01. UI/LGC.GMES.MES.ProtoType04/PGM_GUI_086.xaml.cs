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
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_086 : UserControl
    {
        #region Declaration & Constructor 
        public PGM_GUI_086()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {   
            //testData();
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

        }
        #endregion

        #region Mehod

        #endregion

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chkPrint_Click(object sender, RoutedEventArgs e)
        {
            //연속발행 checkbox 체크시에만 활성화시킴.
            if (chkPrint.IsChecked.Value)
            {
                nbPrintLastNo.IsReadOnly = true;
            }
            else
            {
                nbPrintLastNo.IsReadOnly = false;
            }
        }
    }
}
