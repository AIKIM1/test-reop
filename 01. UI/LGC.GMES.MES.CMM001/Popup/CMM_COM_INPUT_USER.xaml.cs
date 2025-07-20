/*************************************************************************************
 Created Date : 2020.04.14
      Creator : INS 김동일K
   Decription : 작업자 실명관리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.14  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Windows;
using System;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_INPUT_USER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_INPUT_USER : C1Window, IWorkArea
    {
        public string USER_NAME { get; set; }
        public object PARAM_1 { get; set; }
        public object PARAM_2 { get; set; }
        public object PARAM_3 { get; set; }
        public object PARAM_4 { get; set; }
        public object PARAM_5 { get; set; }


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_COM_INPUT_USER()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length > 0) PARAM_1 = tmps[0];
                if (tmps != null && tmps.Length > 1) PARAM_2 = tmps[1];
                if (tmps != null && tmps.Length > 2) PARAM_3 = tmps[2];
                if (tmps != null && tmps.Length > 3) PARAM_4 = tmps[3];
                if (tmps != null && tmps.Length > 4) PARAM_5 = tmps[4];

                txtUserName.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.NVC(txtUserName.Text).Trim().Equals(""))
                {
                    //작업자를 입력하세요
                    Util.MessageValidation("SFU4591", (action) => 
                    {
                        txtUserName.Text = "";
                        txtUserName.Focus();
                    }); 
                    
                    return;
                }

                if (Util.NVC(txtUserName.Text).Length > 100)
                {
                    //입력값이 최대값을 초과 하였습니다
                    Util.MessageValidation("SFU1145", (action) =>
                    {
                        txtUserName.Text = "";
                        txtUserName.Focus();
                    });
                    
                    return;
                }

                USER_NAME = txtUserName.Text;

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                USER_NAME = "";
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void C1Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUserName != null)
                txtUserName.Focus();
        }
    }
}
