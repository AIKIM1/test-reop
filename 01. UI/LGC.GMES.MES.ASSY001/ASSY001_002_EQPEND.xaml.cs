/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_002_EQPEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string PRODID = string.Empty;
        private string WORKDATE = string.Empty;
        private string LOTID = string.Empty;
        private string STATUS = string.Empty;
        private string EQPTID = string.Empty;

        private string saveQty = string.Empty;
        private string endtime = string.Empty;
        
        
        private string SHIFT = string.Empty;
        private string USER = string.Empty;
        private string ISSUE = string.Empty;
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string USERID
        {
            get { return SHIFT; }
        }

        public string USERNAME
        {
            get { return USER; }
        }
        public string COMMENT
        {
            get { return ISSUE; }
        }


        public ASSY001_002_EQPEND()
        {
            InitializeComponent();


        }


        #endregion

        #region Event


        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            //CMM_SHIFT wndPopup = new CMM_SHIFT();
            //wndPopup.FrameOperation = FrameOperation;

            //if (wndPopup != null)
            //{
            //    object[] Parameters = new object[4];
            //    Parameters[0] = LoginInfo.CFG_SHOP_ID;
            //    Parameters[1] = LoginInfo.CFG_AREA_ID;
            //    Parameters[2] = LoginInfo.CFG_EQSG_ID;
            //    Parameters[3] = LoginInfo.CFG_PROC_ID;
            //    C1WindowExtension.SetParameters(wndPopup, Parameters);

            //    wndPopup.Closed += new EventHandler(wndShift_Closed);
            //    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            //}

            CMM001.Popup.CMM_SHIFT wndPopup = new CMM001.Popup.CMM_SHIFT();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = LoginInfo.CFG_PROC_ID;
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            //if (txtShift.Text.Trim().Equals(""))
            //{
            //    // 선택된 작업조가 없습니다.
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 작업조가 없습니다."), null, "Warning", MessageBoxButton.OK, MessageBoxIcon.None);
            //    return;
            //}

            //CMM_SHIFT_USER wndPopup = new CMM_SHIFT_USER();
            //wndPopup.FrameOperation = FrameOperation;

            //if (wndPopup != null)
            //{
            //    object[] Parameters = new object[5];
                
            //    Parameters[4] = txtShift.Tag;
            //    C1WindowExtension.SetParameters(wndPopup, Parameters);

            //    wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
            //    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            //}

           

            CMM001.Popup.CMM_SHIFT_USER wndPopup = new CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = this.FrameOperation;

            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = LoginInfo.CFG_PROC_ID;
                Parameters[4] = Util.NVC(txtShift.Text);
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                if (Util.NVC(txtShift.Text) == string.Empty)
                {
                    // 선택된 작업조가 없습니다.
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택된 작업조가 없습니다."), null, "Warning", MessageBoxButton.OK, MessageBoxIcon.None);
                    Util.MessageValidation("SFU1646");
                    return;
                }
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT window = sender as CMM_SHIFT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Tag = window.SHIFTCODE;
                txtShift.Text = window.SHIFTNAME;
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Tag = window.USERID;
                txtWorker.Text = window.USERNAME;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
          

            this.DialogResult = MessageBoxResult.Cancel;
        }


       private void btn_click(object sender, RoutedEventArgs e)
        {
            SHIFT = txtShift.Text;
            USER = txtWorker.Text;
            ISSUE = txtIssue.Text;
            this.DialogResult = MessageBoxResult.OK;
        }

            #endregion
        }
    }
