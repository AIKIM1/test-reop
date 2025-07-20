/*************************************************************************************
 Created Date : 2021.03.13
      Creator : 오화백K
   Decription : QA 검사여부 확인
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.13  오화백 : Initial Created.
  
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
    /// CMM_COM_QA_INSP_TAGET_YN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_QA_INSP_TAGET_YN : C1Window, IWorkArea
    {
        string _LotID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_COM_QA_INSP_TAGET_YN()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _LotID = tmps[0] as string;
                txtComant.Text = MessageDic.Instance.GetMessage("SFU8335", _LotID.ToString());

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
             this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
     
    }
}
