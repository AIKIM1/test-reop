/*************************************************************************************
 Created Date : 2020.05.26
      Creator : 황기근
   Decription : 비밀번호 입력
--------------------------------------------------------------------------------------
 [Change History]
  2020.05.26  황기근 : Initial Created.





 
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    public partial class CMM_ASSY_PASSWORD : C1Window, IWorkArea
    {
        string password = string.Empty;
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ASSY_PASSWORD()
        {
            InitializeComponent();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length > 0)
                {
                    password = Util.NVC(tmps[0]);
                }
                
                //ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            // 암호 일치하지 않으면 validation
            if (!pbPassword.Password.ToString().Equals(password))
            {
                Util.MessageValidation("SFU8212");
                return;
            }

            this.DialogResult = MessageBoxResult.OK;
        }
        
        //private void ApplyPermissions()
        //{
        //    List<Button> listAuth = new List<Button>();
        //    //listAuth.Add(btnCancel);

        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //}
    }
}
