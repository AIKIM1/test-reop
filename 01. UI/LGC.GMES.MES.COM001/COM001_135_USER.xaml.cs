/*************************************************************************************
 Created Date : 2020.04.24
      Creator : INS 김동일K
   Decription : 수불정보 이상 Data - 담당자 일괄등록 팝업 [C20200406-000377]
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.24  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_135_USER.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_135_USER : C1Window, IWorkArea
    {
        public string USER_NAME()
        {
            return txtUserName.Text;
        }

        public string USER_ID()
        {
            return (string)txtUserName.Tag;
        }

        public string COMMENT()
        {
            return txtReqNote.Text;
        }

        public string STATE()
        {
            return Util.NVC(cboState1.SelectedValue);
        }

        public string USER_NAME_SYS()
        {
            return txtUserName2.Text;
        }

        public string USER_ID_SYS()
        {
            return (string)txtUserName2.Tag;
        }

        public string COMMENT_SYS()
        {
            return txtReqNote2.Text;
        }

        public string STATE_SYS()
        {
            return Util.NVC(cboState2.SelectedValue);
        }

        public COM001_135_USER()
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
                
                this.Loaded -= C1Window_Loaded;

                this.txtUserName.IsEnabled = false;
                this.txtReqNote.IsEnabled = false;
                this.txtUserName2.IsEnabled = false;
                this.txtReqNote2.IsEnabled = false;

                this.chkStat1.IsEnabled = false;
                this.chkStat1.IsChecked = false;
                this.cboState1.IsEnabled = false;

                this.chkStat2.IsEnabled = false;
                this.chkStat2.IsChecked = false;
                this.cboState2.IsEnabled = false;

                DataTable dt = GetResultCombo();

                cboState1.ItemsSource = dt.Copy().AsDataView();
                cboState2.ItemsSource = dt.Copy().AsDataView();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
                    popUser.FrameOperation = FrameOperation;

                    object[] Parameters = new object[1];
                    Parameters[0] = txtUserName.Text;
                    C1WindowExtension.SetParameters(popUser, Parameters);

                    popUser.Closed += new EventHandler(popUser_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
                popUser.FrameOperation = FrameOperation;

                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(popUser, Parameters);

                popUser.Closed += new EventHandler(popUser_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtUserName2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
                    popUser.FrameOperation = FrameOperation;

                    object[] Parameters = new object[1];
                    Parameters[0] = txtUserName2.Text;
                    C1WindowExtension.SetParameters(popUser, Parameters);

                    popUser.Closed += new EventHandler(popUser2_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReqUser2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM001.Popup.CMM_PERSON popUser = new CMM001.Popup.CMM_PERSON();
                popUser.FrameOperation = FrameOperation;

                object[] Parameters = new object[1];
                Parameters[0] = txtUserName2.Text;
                C1WindowExtension.SetParameters(popUser, Parameters);

                popUser.Closed += new EventHandler(popUser2_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popUser.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Tag.ToString()))
                //{
                //    // 요청자를 입력 하세요.
                //    Util.MessageValidation("SFU3451", (action) => { txtUserName.Focus(); });
                //    return;
                //}

                //if (string.IsNullOrWhiteSpace(txtReqNote.Text))
                //{
                //    // 사유를 입력하세요.
                //    Util.MessageValidation("SFU1594", (action) => { txtReqNote.Focus(); });
                //    return;
                //}

                if (!(bool)rdoProd.IsChecked && !(bool)rdoSys.IsChecked)
                {
                    Util.MessageValidation("SFU1651"); // 선택된 항목이 없습니다.
                    return;
                }

                if (!(bool)chkStat1.IsChecked)
                    cboState1.SelectedValue = 0;

                if (!(bool)chkStat2.IsChecked)
                    cboState2.SelectedValue = 0;

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void popUser_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = popup.USERNAME;
                txtUserName.Tag = popup.USERID;

                txtReqNote.Focus();
            }
        }

        private void popUser2_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_PERSON popup = sender as CMM001.Popup.CMM_PERSON;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtUserName2.Text = popup.USERNAME;
                txtUserName2.Tag = popup.USERID;

                txtReqNote2.Focus();
            }
        }

        private void rdoProd_Checked(object sender, RoutedEventArgs e)
        {
            this.txtUserName.IsEnabled = true;
            this.txtReqNote.IsEnabled = true;
            this.txtUserName2.IsEnabled = false;
            this.txtUserName2.Text = "";
            this.txtUserName2.Tag = "";
            this.txtReqNote2.IsEnabled = false;
            this.txtReqNote2.Text = "";

            this.chkStat1.IsEnabled = true;
            this.chkStat1.IsChecked = false;
            this.cboState1.IsEnabled = false;
            this.cboState1.SelectedIndex = 0;

            this.chkStat2.IsEnabled = false;
            this.chkStat2.IsChecked = false;
            this.cboState2.IsEnabled = false;
            this.cboState2.SelectedIndex = 0;
        }

        private void rdoSys_Checked(object sender, RoutedEventArgs e)
        {
            this.txtUserName.IsEnabled = false;
            this.txtUserName.Text = "";
            this.txtUserName.Tag = "";
            this.txtReqNote.IsEnabled = false;
            this.txtReqNote.Text = "";
            this.txtUserName2.IsEnabled = true;            
            this.txtReqNote2.IsEnabled = true;

            this.chkStat1.IsEnabled = false;
            this.chkStat1.IsChecked = false;
            this.cboState1.IsEnabled = false;
            this.cboState1.SelectedIndex = 0;

            this.chkStat2.IsEnabled = true;
            this.chkStat2.IsChecked = false;
            this.cboState2.IsEnabled = false;
            this.cboState2.SelectedIndex = 0;
        }

        private void chkStat1_Checked(object sender, RoutedEventArgs e)
        {
            cboState1.IsEnabled = true;
        }

        private void chkStat1_Unchecked(object sender, RoutedEventArgs e)
        {
            cboState1.IsEnabled = false;
        }

        private void chkStat2_Checked(object sender, RoutedEventArgs e)
        {
            cboState2.IsEnabled = true;
        }

        private void chkStat2_Unchecked(object sender, RoutedEventArgs e)
        {
            cboState2.IsEnabled = false;
        }

        private DataTable GetResultCombo()
        {
            try
            {
                DataTable inTable = null;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("CMCDTYPE", typeof(string));
                inDataTable.Columns.Add("ATTRIBUTE1", typeof(string));

                inTable = inDataTable;

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "SBL_VERIF_FINL_RSLT";
                //newRow["ATTRIBUTE1"] = "Y";

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "INDATA", "OUTDATA", inTable);

                if (dtRslt == null)
                {
                    dtRslt = new DataTable();
                    dtRslt.Columns.Add("CBO_NAME", typeof(string));
                    dtRslt.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr = dtRslt.NewRow();

                dr["CBO_NAME"] = "-SELECT-";
                dr["CBO_CODE"] = "SELECT";
                dtRslt.Rows.InsertAt(dr, 0);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
    }
}
