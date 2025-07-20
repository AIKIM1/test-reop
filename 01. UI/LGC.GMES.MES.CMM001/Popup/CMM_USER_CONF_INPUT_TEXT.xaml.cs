/*************************************************************************************
 Created Date : 2023.05.09
      Creator : 조영대
   Decription : 텍스트 입력
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.09  조영대 : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_USER_CONF_INPUT_TEXT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string formTitle = string.Empty; //화면명
        private string inputTitle = string.Empty; //입력항목명

        public string ReturnText { get; set; }
        public bool ReturnCheck { get; set; }

        public CMM_USER_CONF_INPUT_TEXT()
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
    
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                formTitle = Util.NVC(tmps[0]);
                inputTitle = Util.NVC(tmps[1]);
                txtInput.Text = Util.NVC(tmps[2]);
                chkDefault.IsChecked = tmps[3].Equals(true);

                this.Header = formTitle;
                this.lblInput.Text = inputTitle;
            }

            txtInput.Focus();
            txtInput.SelectionStart = txtInput.Text.Length;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtInput.Text.Trim()))
                {
                    txtInput.SetValidation("SFU8275", lblInput.Text);
                    return;
                }
                ReturnText = txtInput.Text;
                ReturnCheck = chkDefault.IsChecked.Equals(true);

                this.ClearValidation();
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }

        }
        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void lblInput_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            txtInput.Text = ObjectDic.Instance.GetObjectName("FRMSETTING");
        }
        #endregion

        #region Mehod

        #endregion

    }
}
