/*************************************************************************************
 Created Date : 2018.09.27
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 월 생산계획 비고 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.27  INS 김동일K : Initial Created.
  
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

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_001_NOTE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_001_NOTE : C1Window, IWorkArea
    {
        public COM001_001_NOTE()
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
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 5)
                {
                    txtProdName.Text = Util.NVC(tmps[1]);
                    txtRemark.AppendText(Util.NVC(tmps[3]));
                    //txtPouchProdChgNote.Text = Util.NVC(tmps[4]);
                }

                ApplyPermissions();
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
         
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
    }
}
