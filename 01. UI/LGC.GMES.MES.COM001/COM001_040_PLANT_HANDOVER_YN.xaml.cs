/*************************************************************************************
 Created Date : 2024.05.02
      Creator : 
   Decription : Plant 이동(인계) : 인계처리 여부
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.02  김도형    : Initial Created.
  2024.05.02  김도형    : [E20240320-001047] [소형_전극]전극 전산 VD 미진행시 와인더 투입 불가 현상 개선 요청
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// CMM_FCUT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_040_PLANT_HANDOVER_YN : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PLANT_HANDOVER_YN  = "B"; // 인계처리 중지
        private string _LOTID = string.Empty;

        public string PLANT_HANDOVER_YN
        {
            get { return _PLANT_HANDOVER_YN; }
        }
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

        public COM001_040_PLANT_HANDOVER_YN()
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
                    _LOTID = Util.NVC(tmps[0]);
                }
                SetMessage();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void btnHandover_Click(object sender, RoutedEventArgs e)
        {
            _PLANT_HANDOVER_YN = "Y";
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _PLANT_HANDOVER_YN = "C";
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void C1Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Dispatcher.BeginInvoke(new Action(() => btnHandover_Click(null, null)));
        }
        private void SetMessage()
        {
            txtMsg.Text = MessageInfo("SFU9217");
        }
        #endregion

        #region Mehod
        public string  MessageInfo(string messageId, params object[] parameters)
        {

            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }

            return message;
        }
        public string MessageInfo(string messageId)
        {

            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            
            return message;
        }
        #endregion

    }
}
