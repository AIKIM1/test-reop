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
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_027_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public PACK001_027_CONFIRM()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string sRCV_ISS_ID = string.Empty;

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length == 6)
            {
                txtShiptoName.Text = tmps[0] as string;
                txtToSlocName.Text = tmps[1] as string;
                txtModel.Text = tmps[2] as string;
                txtProdID.Text = tmps[3] as string;
                txtPalletQty.Value = int.Parse(tmps[4] as string);
                txtCellQty.Value = int.Parse(tmps[5] as string);
            }

            this.Loaded -= Window_Loaded;
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Mehod

        #endregion
    }
}