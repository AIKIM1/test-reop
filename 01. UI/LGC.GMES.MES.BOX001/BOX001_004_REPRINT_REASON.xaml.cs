/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_004_REPRINT_REASON : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public BOX001_004 BOX001_004;

        public string RePrintUser = string.Empty;
        public string RePrintcomment = string.Empty;

        public BOX001_004_REPRINT_REASON()
        {
            InitializeComponent();
                       
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string sLotid = string.Empty;

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 0)
            {
                sLotid = Util.NVC(tmps[0]);
            }

            this.Loaded -= Window_Loaded;

            txtReprintID.Text = sLotid;
            txtUserID.Text = LoginInfo.USERID;
        }

        public string PRINTCOMMENT
        {
            get { return RePrintcomment; }
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
            //if(string.IsNullOrEmpty(txtUserID.Text))
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("사용자를 입력 하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            if (string.IsNullOrEmpty(txtReason.Text))
            {
                Util.Alert("SFU3008");  //재발행 사유를 입력 하세요.
                return;
            }

            RePrintcomment = txtReason.Text.ToString();

            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            RePrintUser = "";
            RePrintcomment = "";
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        //private void btnWorker_Click(object sender, RoutedEventArgs e)
        //{
        //    CMM_SHIFT_USER wndPopup = new CMM_SHIFT_USER();
        //    wndPopup.FrameOperation = FrameOperation;

        //    if (wndPopup != null)
        //    {
        //        object[] Parameters = new object[5];
        //        Parameters[0] = LoginInfo.CFG_SHOP_ID;
        //        Parameters[1] = LoginInfo.CFG_AREA_ID;
        //        Parameters[2] = LoginInfo.CFG_EQPT_ID;
        //        Parameters[3] = LoginInfo.CFG_PROC_ID;
        //        Parameters[4] = "";
        //        C1WindowExtension.SetParameters(wndPopup, Parameters);

        //        wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
        //        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
        //    }
        //}

        //private void wndShiftUser_Closed(object sender, EventArgs e)
        //{
        //    CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
        //    if (window.DialogResult == MessageBoxResult.OK)
        //    {
        //        //txtWorker.Tag = window.USERID;
        //        //txtUserID.Text = window.USERNAME;
        //    }
        //}
    }
}
