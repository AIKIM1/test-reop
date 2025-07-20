/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
**************************************************************************************/

using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_201_Reprint_Reason : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public string _RePrintUser = string.Empty;
        public string _RePrintcomment = string.Empty;

        public PGM_GUI_201_Reprint_Reason(string sLotid)
        {
            InitializeComponent();


            //txtReprintID.Text = C1WindowExtension.GetParameter(this);
            txtReprintID.Text = sLotid;
        }

        public string PRINTUSERID
        {
            get { return _RePrintUser; }
        }

        public string PRINTCOMMENT
        {
            get { return _RePrintcomment; }
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod

        #endregion

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserID.Text))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("사용자를 입력 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtReason.Text))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("재발행 사유를 입력 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            _RePrintUser = txtUserID.Text;
            _RePrintcomment = txtReason.Text;

            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _RePrintUser = "";
            _RePrintcomment = "";
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
    }
}