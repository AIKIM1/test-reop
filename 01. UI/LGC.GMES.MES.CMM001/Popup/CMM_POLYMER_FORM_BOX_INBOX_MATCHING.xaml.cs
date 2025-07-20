/*************************************************************************************
 Created Date : 2018.06.13
      Creator : 정문교
   Decription : 1차 포장시 INBOX-BOX MATCHING
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CELL_IINSERT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_BOX_INBOX_MATCHING : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비
        private string _palletID = string.Empty;      // PalletID

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private bool _load = true;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_BOX_INBOX_MATCHING()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                SetControl();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {
            txtInboxID.Text = string.Empty;
            txtBoxID.Text = string.Empty;
            //Util.gridClear(dgInbox);
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[1] as string;
            _palletID = tmps[2] as string;

            txtWorker.Focus();
        }
        #endregion

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GetUser(txtWorker.Text.Trim());
            }
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtInboxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtBoxID.Focus();
            }
        }

        #region Box ID 
        private void txtBoxID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Length > 0)
                    {
                        txtBoxID.Text = sPasteStrings[0].Trim();
                        txtBoxID_KeyDown(txtBoxID, null);

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    if (string.IsNullOrWhiteSpace(txtUserId.Text))
                    {
                        // 사용자 ID를 입력하세요.
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3526"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtWorker.Focus();
                            }
                        });

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtInboxID.Text))
                    {
                        // Inbox정보가 없습니다.
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU4467"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtInboxID.Focus();
                            }
                        });

                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtBoxID.Text))
                    {
                        // BoxID를 입력하세요.
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU4391"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtInboxID.Focus();
                            }
                        });

                        return;
                    }

                    // DATA Table
                    DataSet inDataSet = new DataSet();

                    DataTable inTable = inDataSet.Tables.Add("INDATA");
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("INSP_SKIP_YN", typeof(string));

                    DataTable inPallet = inDataSet.Tables.Add("INPALLET");
                    inPallet.Columns.Add("EQPTID", typeof(string));
                    inPallet.Columns.Add("BOXID", typeof(string));

                    DataTable inSet = inDataSet.Tables.Add("INSET");
                    inSet.Columns.Add("BOXID", typeof(string));
                    inSet.Columns.Add("FORM_INBOXID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["USERID"] = txtUserId.Text;
                    inTable.Rows.Add(dr);

                    dr = inPallet.NewRow();
                    dr["EQPTID"] = _eqptID;
                    dr["BOXID"] = _palletID;
                    inPallet.Rows.Add(dr);

                    dr = inSet.NewRow();
                    dr["BOXID"] = txtBoxID.Text;
                    dr["FORM_INBOXID"] = txtInboxID.Text;
                    inSet.Rows.Add(dr);

                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SUBLOT_INBOX_NJ", "INDATA,INPALLET,INSET", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            HiddenLoadingIndicator();

                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            // 재조회
                            GetinboMatching(txtInboxID.Text);
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    }, inDataSet);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        private void GetUser(string sUserId)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow row = inTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["USERID"] = sUserId;
                inTable.Rows.Add(row);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_BY_ID", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            txtUserName.Text = (string)bizResult.Rows[0]["USERNAME"];
                            txtUserId.Text = (string)bizResult.Rows[0]["USERID"];
                            txtInboxID.Focus();
                        }
                        else
                        {
                            // 사용자 정보가 없습니다.
                            Util.MessageValidation("SFU1592");

                            txtUserName.Text = string.Empty;
                            txtUserId.Text = string.Empty;
                            txtWorker.Focus();
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Inbox, Cell 정보 조회
        /// </summary>
        private void GetinboMatching(string inboxID)
        {
            try
            {
                InitializeUserControls();

                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = inboxID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("DA_PRD_SEL_BOX_INBOX_MATCHING_PC", "INDATA", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        DataTable dtBefore = DataTableConverter.Convert(dgInbox.ItemsSource);

                        if (dtBefore != null)
                            bizResult.Merge(dtBefore);

                        Util.GridSetData(dgInbox, bizResult.Tables["OUTDATA"], FrameOperation);

                        txtInboxID.Focus();
                    }
                    catch (Exception ex)
                    {
                        txtInboxID.Focus();

                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }








        #endregion

    }
}