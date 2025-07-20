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
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PGM_GUI_006()
        {
            InitializeComponent();
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // System.DateTime.Now.ToString("yyyy-MM-dd");
            initMterialCombo();
        }

        #endregion

        #region Mehod
        private void initMterialCombo()
        {
            _combo.SetCombo(cboMaterialCode, CommonCombo.ComboStatus.ALL);
        }
        #endregion
    }
}