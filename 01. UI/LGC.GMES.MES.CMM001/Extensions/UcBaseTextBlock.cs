/*************************************************************************************
 Created Date : 2024.02.01
      Creator : 
   Decription : TextBlock Extension
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.01  조영대 : Initial Created. 
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Controls
{
    public class UcBaseTextBlock : TextBlock
    {
        #region EventHandler
        #endregion

        #region Property
        #endregion

        #region Declaration & Constructor 

        public UcBaseTextBlock()
        {
        }

        public void InitializeControls()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
        }

        #endregion

        #region Override

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InitializeControls();
        }        
        #endregion

        #region Event
        #endregion

        #region Public Method
        #endregion

        #region Method
        #endregion

    }
}
