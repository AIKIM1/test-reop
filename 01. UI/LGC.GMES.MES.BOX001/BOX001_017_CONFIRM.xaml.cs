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
using System.Windows.Documents;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_017_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        int iReturnCellQty = 0;

        public string sNOTE = string.Empty;
        public string sRETURN_TYPE_CODE = string.Empty;

        public BOX001_017_CONFIRM(int returnCellQty)
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
            // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
            //string sRCV_ISS_ID = string.Empty;
            string[] sRCV_ISS_ID = null;

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length > 0)
            {
                // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
                // Cell 반품 확정 다수 처리 할 수 있도록 수정
                //sRCV_ISS_ID = Util.NVC(tmps[0]);

                sRETURN_TYPE_CODE = Util.NVC(tmps[0]);

                sRCV_ISS_ID = new string[tmps.Length - 1];

                for (int i = 1; i < tmps.Length; i++)
                {
                    sRCV_ISS_ID[i - 1] = Util.NVC(tmps[i]);
                }
            }

            this.Loaded -= Window_Loaded;

            Search_Confrim(sRCV_ISS_ID);
        }

        #endregion

        #region Initialize

        #endregion

        // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정
        //private void Search_Confrim(string sRCV_ISS_ID)
        private void Search_Confrim(string []sRCV_ISS_ID)
        {
            try
            {
                string sBizName = "DA_PRD_SEL_RETURN_CELL_LIST";

                // 2021.04.05. jonghyun.han C20210207-000007 Cell 반품 확정 Multi 처리 되도록 수정

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                if (sRETURN_TYPE_CODE == "RMA")
                {
                    RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(String));
                    RQSTDT.Columns.Add("IN", typeof(String));
                    
                    sBizName = "DA_PRD_SEL_RETURN_CELL_LIST_RMA";
                }
                else
                {
                    sBizName = "DA_PRD_SEL_RETURN_CELL_LIST";
                }

                foreach (string str in sRCV_ISS_ID)
                {
                    DataRow dr = RQSTDT.NewRow();
                    //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["RCV_ISS_ID"] = str;
                    dr["LANGID"] = LoginInfo.LANGID;

                    if (sRETURN_TYPE_CODE == "RMA")
                    {
                        dr["RETURN_TYPE_CODE"] = sRETURN_TYPE_CODE;
                        dr["IN"] = "Y";
                    }

                    RQSTDT.Rows.Add(dr);
                }

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                //if (SearchResult.Rows.Count > 0)
                //    SearchResult.Rows[0]["RCV_QTY"] = iReturnCellQty;                
                for (int i = 0; i < SearchResult.Rows.Count; i++)
                {
                    SearchResult.Rows[i]["RCV_QTY"] = SearchResult.Rows[i]["ISS_QTY"];
                }

                Util.gridClear(dgReturn_Confrim);
                dgReturn_Confrim.ItemsSource = DataTableConverter.Convert(SearchResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtUserID.Text.ToString()))
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업자란은 필수 입력사항입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    return;
                //}
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
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                //txtWorker.Tag = window.USERID;
                txtUserID.Text = window.USERNAME;
            }
        }
        #endregion

        #region Mehod

        #endregion


    }
}
