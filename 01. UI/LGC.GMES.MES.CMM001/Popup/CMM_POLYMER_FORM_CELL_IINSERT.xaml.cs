/*************************************************************************************
 Created Date : 2018.06.12
      Creator : 정문교
   Decription : DSF 포장모드시 Inbox별 Cell 등록
--------------------------------------------------------------------------------------
 [Change History]
 2024.05.03  이병윤 : E20240411-000952 : 저장시 셀에 있는 GRADE 혼입 여부 체크 해서 인터락처리
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CELL_IINSERT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CELL_IINSERT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private bool _isSublot = true;

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

        public CMM_POLYMER_FORM_CELL_IINSERT()
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
                if (string.IsNullOrWhiteSpace(txtInbox.Text))
                {
                    //txtInbox.Focus();
                    txtWorker.Focus();
                }
                else
                {
                    Getinbox(txtInbox.Text);
                }

                _load = false;
            }

        }

        private void InitializeUserControls(bool IsAll = true)
        {
            if (IsAll)
            {
                txtInbox.Text = string.Empty;
            }

            txtBoxID.Text = string.Empty;
            txtProdID.Text = string.Empty;
            txtProdName.Text = string.Empty;
            txtProject.Text = string.Empty;
            txtLotID.Text = string.Empty;
            txtCellID.Text = string.Empty;
            txtBoxCellQty.Text = "0";
            txtCellQty.Text = "0";

            Util.gridClear(dgInbox);
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            txtInbox.Text = tmps[1] as string;
        }
        #endregion

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtWorker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GetUser(txtWorker.Text.Trim());
            }
        }

        private void txtInbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            InitializeUserControls(false);
        }

        private void txtInbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Getinbox(txtInbox.Text.Trim());
            }
        }

        private void txtBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                
                Chkbox(txtBoxID.Text.Trim());
            }
        }

        #region Cell 
        private void txtCellID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        _isSublot = true;

                        txtCellID.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtCellID.Text))
                            txtCellID_KeyDown(txtCellID, null);

                        if (!_isSublot)
                            break;

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

        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    if (txtUserId.Text == string.Empty)
                    {
                        Util.WarningPlayer();

                        _isSublot = false;

                        // SFU3526 사용자 ID를 입력하세요.
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3526"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellID.Text = string.Empty;
                                txtWorker.Focus();
                            }
                        });

                        return;
                    }

                    string sCellID = txtCellID.Text.Trim();

                    if (dgInbox.GetRowCount() > 0)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
                        DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sCellID + "'");

                        if (drList.Length > 0)
                        {
                            Util.WarningPlayer();

                            _isSublot = false;

                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellID.Focus();
                                    txtCellID.Text = string.Empty;
                                }
                            });

                            txtCellID.Text = string.Empty;
                            return;
                        }
                    }

                    if (dgInbox.GetRowCount() >= Util.NVC_Int(txtBoxCellQty.Text))
                    {
                        Util.WarningPlayer();

                        _isSublot = false;

                        // SFU3306 입력오류 : BOX의 포장가능 수량 % 1을 넘었습니다. [포장수량 수정 후 LOT 입력]
                        Util.MessageInfo("SFU3306", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellID.Focus();
                                txtCellID.Text = string.Empty;
                            }
                        }, new object[] { txtBoxCellQty.Text });
                        return;
                    }

                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("USERID");
                    RQSTDT.Columns.Add("SUBLOTID");
                    RQSTDT.Columns.Add("LOTID");

                    DataRow dr = RQSTDT.NewRow();
                    dr["USERID"] = txtUserId.Text;
                    dr["SUBLOTID"] = sCellID;
                    dr["LOTID"] = txtInbox.Text;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_DSF_NJ", "INDATA", "OUTDATA", RQSTDT);

                    if (dtRslt != null)
                    {
                        if (dgInbox.GetRowCount() > 0)
                        {
                            DataTable dt = DataTableConverter.Convert(dgInbox.ItemsSource);
                            DataRow[] drList = dt.Select("SUBLOTID = '" + dtRslt.Rows[0]["SUBLOTID"] + "'");

                            if (drList.Length > 0)
                            {
                                Util.WarningPlayer();

                                // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                                ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        txtCellID.Focus();
                                        txtCellID.Text = string.Empty;
                                    }
                                });

                                txtCellID.Text = string.Empty;
                                return;
                            }
                        }

                        DataColumn dc = new DataColumn("NEW");
                        dc.DefaultValue = "Y";
                        dtRslt.Columns.Add(dc);

                        DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);

                        dtRslt.Merge(dtInfo);
                        Util.GridSetData(dgInbox, dtRslt, null);

                        txtCellQty.Text = Util.NVC(dtRslt.Rows.Count);
                    }

                    txtCellID.Text = string.Empty;
                    txtCellID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.WarningPlayer();

                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtCellID.Text = string.Empty;
                        txtCellID.Focus();
                    }
                });
            }
            finally
            {
            }
        }
        #endregion

        #region Cell 삭제
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtUserId.Text == string.Empty)
                {
                    // SFU3526 사용자 ID를 입력하세요.
                    Util.MessageValidation("SFU3526");
                    return;
                }

                Button btn = sender as Button;
                int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

                string sCell = Util.NVC(dgInbox.GetCell(iRow, dgInbox.Columns["SUBLOTID"].Index).Value);
                if (Util.NVC(dgInbox.GetCell(iRow, dgInbox.Columns["NEW"].Index).Value) == "Y")
                {
                    DataTable dtCELL = DataTableConverter.Convert(dgInbox.ItemsSource);
                    DataRow[] drInfo = dtCELL.Select("SUBLOTID <> '" + sCell + "'", "PACKDTTM DESC");
                    int cnt = drInfo.Length;

                    if (cnt > 0)
                    {
                        //dgInbox.ItemsSource = DataTableConverter.Convert(drInfo.CopyToDataTable());
                        Util.GridSetData(dgInbox,drInfo.CopyToDataTable(), null);
                    }
                    else
                        Util.gridClear(dgInbox);

                    txtCellQty.Text = Util.NVC(cnt);
                }
                else
                {
                    // 삭제제하시겠습니까?";
                    Util.MessageConfirm("SFU1230", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable inTable = new DataTable();
                            inTable.Columns.Add("SUBLOTID");
                            inTable.Columns.Add("USERID");

                            DataRow newRow = inTable.NewRow();
                            newRow["SUBLOTID"] = sCell;
                            newRow["USERID"] = txtUserId.Text;
                            inTable.Rows.Add(newRow);

                            ShowLoadingIndicator();

                            new ClientProxy().ExecuteService("BR_PRD_DEL_SUBLOT_DSF_NJ", "INDATA", null, inTable, (bizResult, bizException) =>
                            {
                                try
                                {
                                    HiddenLoadingIndicator();

                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    //Getinbox(txtInbox.Text);
                                    DataTable dtCELL = DataTableConverter.Convert(dgInbox.ItemsSource);
                                    dtCELL.Select("SUBLOTID = '" + sCell + "'").ToList<DataRow>().ForEach(row => row.Delete());
                                    dtCELL.AcceptChanges();

                                    Util.GridSetData(dgInbox, dtCELL, null);

                                    txtCellQty.Text = Util.NVC(dtCELL.Rows.Count);
                                }
                                catch (Exception ex)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(ex);
                                }
                            });
                        }
                    }, sCell);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgInbox.ItemsSource == null)
            {
                // SFU3552 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return;
            }

            DataRow[] drList = (DataTableConverter.Convert(dgInbox.ItemsSource)).Select("NEW = 'Y'");
            if (drList.Length < 1)
            {
                // SFU3552 저장 할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                return;
            }

            // Grade 혼입 여부 체크
            DataTable dt = DataTableConverter.Convert(dgInbox.ItemsSource);
            var dupGrade = dt.AsEnumerable().GroupBy(x => x["GRADE"]);

            if (dupGrade.Count() > 1 && LoginInfo.SYSID == "GMES-S-N4")
            {
                // 동일한 등급이 아닙니다.
                Util.MessageValidation("SFU4060");
                return;
            }

            if (txtUserId.Text == string.Empty)
            {
                // SFU3526 사용자 ID를 입력하세요.
                Util.MessageValidation("SFU3526");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBoxID.Text))
            {
                // BOX 정보가 없습니다.
                Util.MessageValidation("SFU1180");
                return;
            }

            //SFU1241 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("LOTID");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("BOXID");

                    DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                    inSublotTable.Columns.Add("SUBLOTID");
                    inSublotTable.Columns.Add("BOX_PSTN_NO");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["LOTID"] = txtInbox.Text;
                    newRow["USERID"] = txtUserId.Text;
                    newRow["BOXID"] = txtBoxID.Text;
                    inDataTable.Rows.Add(newRow);

                    foreach (DataRow dr in drList)
                    {
                        newRow = inSublotTable.NewRow();
                        newRow["SUBLOTID"] = dr["SUBLOTID"];
                        newRow["BOX_PSTN_NO"] = 0; // dr["BOX_PSTN_NO"]; 사용안함
                        inSublotTable.Rows.Add(newRow);
                    }

                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SUBLOT_DSF_NJ", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            HiddenLoadingIndicator();

                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            //Getinbox(txtInbox.Text);
                            //this.DialogResult = MessageBoxResult.OK;

                            InitializeUserControls(true);
                            txtInbox.Focus();
                        }
                        catch (Exception ex)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(ex);
                        }
                    }, indataSet);
                }
            });
        }
        #endregion

        #region 초기화
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] drList = DataTableConverter.Convert(dgInbox.ItemsSource).Select("NEW <> 'Y'");
            Util.gridClear(dgInbox);

            if (drList.Length > 0)
            {
                dgInbox.ItemsSource = DataTableConverter.Convert(drList.CopyToDataTable());
            }

            txtCellQty.Text = drList.Length.ToString();
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

        /// <summary>
        /// Inbox, Cell 정보 조회
        /// </summary>
        private void Getinbox(string inboxID)
        {
            try
            {
                InitializeUserControls();

                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = inboxID;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_DSF_NJ", "INDATA", "OUTDATA,OUTSUBLOT", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Tables["OUTDATA"] != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            txtInbox.Text = (string)bizResult.Tables["OUTDATA"].Rows[0]["LOTID"];
                            txtProdID.Text = (string)bizResult.Tables["OUTDATA"].Rows[0]["PRODID"];
                            txtLotID.Text = (string)bizResult.Tables["OUTDATA"].Rows[0]["LOTID_RT"];
                            txtProject.Text = (string)bizResult.Tables["OUTDATA"].Rows[0]["PRJT_NAME"];
                            txtProdName.Text = (string)bizResult.Tables["OUTDATA"].Rows[0]["PRODNAME"];
                            txtBoxCellQty.Text = Util.NVC_Int(bizResult.Tables["OUTDATA"].Rows[0]["WIPQTY"]).ToString("D");
                        }

                        Util.GridSetData(dgInbox, bizResult.Tables["OUTSUBLOT"], null);

                        txtCellQty.Text = bizResult.Tables["OUTSUBLOT"].Rows.Count.ToString("D");

                        if (string.IsNullOrWhiteSpace(txtInbox.Text))
                            txtInbox.Focus();
                        else if (string.IsNullOrWhiteSpace(txtWorker.Text))
                            txtWorker.Focus();
                        else if (string.IsNullOrWhiteSpace(txtBoxID.Text))
                            txtBoxID.Focus();
                        else
                            txtCellID.Focus();

                    }
                    catch (Exception ex)
                    {
                        txtInbox.Focus();

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

        private void Chkbox(string boxID)
        {
            try
            {

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("BOXID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["BOXID"] = boxID;
                inTable.Rows.Add(newRow);
                    
                new ClientProxy().ExecuteService_Multi("BR_ACT_CHK_BOX_INFO_DSF_PACKING_NJ", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        txtCellID.Focus();
                    }
                    catch (Exception ex)
                    {
                        txtBoxID.Focus();

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
                            //txtCellID.Focus();

                            if (string.IsNullOrWhiteSpace((txtInbox.Text)))
                                txtInbox.Focus();
                            else
                                txtBoxID.Focus();
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