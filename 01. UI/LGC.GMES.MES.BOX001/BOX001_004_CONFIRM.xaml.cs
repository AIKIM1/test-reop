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
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_004_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty;
        string tmmp02 = string.Empty;
        DataTable tmmp03;

        public string UserInfo = string.Empty;

        public BOX001_004_CONFIRM()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as string;
            tmmp03 = tmps[2] as DataTable;

            txtWorkorder.Text = tmmp01;
            txtQty.Text = tmmp02;

            Util.gridClear(dgConfrim);
            dgConfrim.ItemsSource = DataTableConverter.Convert(tmmp03);

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
                if (string.IsNullOrEmpty(txtUserID.Text.ToString()))
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업자란은 필수 입력사항입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.Alert("SFU1842"); //작업자를 선택 하세요.
                    return;
                }

                UserInfo = txtUserID.Text.ToString();

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
            UserInfo = "";
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER wndPopup = new CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                // 팝업 화면 숨겨지는 문제 수정.
                 this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
              //  grdMain.Children.Add(wndPopup); 팝업이 뒤로떠서 주석처리함
                wndPopup.BringToFront();
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                //txtWorker.Tag = window.USERID;
                txtUserID.Text = window.USERNAME;
            }
        }
        #endregion

        #region Mehod

        #endregion


    }
}
