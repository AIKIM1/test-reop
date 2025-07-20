/*************************************************************************************
 Created Date : 2021.04.11
      Creator : 이제섭
   Decription : Cell 반품확정 - 반품확정, 샘플입고 확정 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.11  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Documents;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_310_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        int iReturnCellQty = 0;

        public string sNOTE = string.Empty;
        public BOX001_310_CONFIRM(int returnCellQty)
        {
            iReturnCellQty = returnCellQty;
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
            object[] tmps = C1WindowExtension.GetParameters(this);

            DataTable returnlist = new DataTable();
            if (tmps != null && tmps.Length >= 0)
            {
                returnlist = tmps[0] as DataTable;
            }

            this.Loaded -= Window_Loaded;

            Search_Confrim(returnlist);
        }

        #endregion

        #region Initialize

        #endregion


        private void Search_Confrim(DataTable returnList)
        {
            try
            {
                Util.gridClear(dgReturn_Confrim);
                dgReturn_Confrim.ItemsSource = DataTableConverter.Convert(returnList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd);
                if (textRange.Text.Trim() == string.Empty)
                {
                    Util.MessageValidation("SFU1554"); //"반품사유를 입력하세요"
                    return;
                }

                sNOTE = textRange.Text;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
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
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtUserID.Text = window.USERNAME;
            }
        }
        #endregion

        #region Mehod

        #endregion


    }
}
