using System;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.CMM001.Popup
{
    public partial class CMM_ELEC_QA_SMPL_QTY_CHK: C1Window, IWorkArea
    {
        #region Declaration & Constructor
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ELEC_QA_SMPL_QTY_CHK()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    //%1은 QA Sampling 대상이지만 수량 입력이 되지 않았습니다. (실적확정은 계속 진행됩니다.)
                    string message = MessageDic.Instance.GetMessage("SFU8363", tmps);
                    message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                    txtMessage.Text = message;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
