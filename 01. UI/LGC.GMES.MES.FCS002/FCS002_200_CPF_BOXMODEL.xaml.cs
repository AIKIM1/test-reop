/*************************************************************************************
 Created Date : 2023.01.16
      Creator : Dooly
   Decription : CPF BOX MODEL
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.16  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_200_CPF_BOXMODEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public FCS002_200_CPF_BOXMODEL()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);


        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            //string[] sFilter = { "FORMEQPT_MAINT_TYPE_CODE" };
            //_combo.SetCombo(cboMaintCd, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN");

            //cboMaintCd.SelectedValueChanged += CboMaindCd_SelectedValueChanged;
        }
        #endregion

        #region Event

        

        #endregion

        #region Mehod
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

    }
}
