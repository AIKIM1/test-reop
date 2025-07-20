/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.03.16  LEEHJ       소형활성화MES(기존 FCS002_315_CELL_DETL) 파우치 전용
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_315_CELL_DETL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        //private string _UserId = string.Empty;
        private string _PalltID = string.Empty;
        private string _AommGrade = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_315_CELL_DETL()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void FCS002_315_CELL_DETL_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FCS002_315_CELL_DETL_Loaded;
            InitControl();

            if (!string.IsNullOrWhiteSpace(txtBoxID.Text))
            {
                Getinbox(txtBoxID.Text);
                txtWorker.Focus();
            }
            else
                txtBoxID.Focus();
        }

        private void InitControl()
        {
            this.Focus();
            object[] tmps = C1WindowExtension.GetParameters(this);
            txtBoxID.Text = tmps[0] as string;
            //_UserId = tmps[1] as string;
            _PalltID = tmps[2] as string;
            _AommGrade = tmps[3] as string;
        }

        #endregion

        #region Event
        private void txtInbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Getinbox(txtBoxID.Text.Trim());
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

                    //if (e == null)
                    //{
                    //    this.Dispatcher.Invoke((ThreadStart)(() => { }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    //    Thread.Sleep(1000);
                    //}               

                    string sCellID = txtCellID.Text.Trim();

                    if (dgInbox.GetRowCount() > 0)
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
                        DataRow[] drList = dtInfo.Select("SUBLOTID = '" + sCellID + "'");

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
                            //  txtCellID.Focus();
                            return;
                        }
                    }

                    if (dgInbox.GetRowCount() >= Util.NVC_Int(txtBoxCellQty.Text))
                    {
                        Util.WarningPlayer();

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
                    RQSTDT.Columns.Add("BOXID");
                    RQSTDT.Columns.Add("SUBLOTID");
                    //         RQSTDT.Columns.Add("BOX_PSTN_NO");
                    RQSTDT.Columns.Add("USERID");
                    RQSTDT.Columns.Add("AOMM_GRD_CODE");

                    DataRow dr = RQSTDT.NewRow();
                    dr["BOXID"] = txtBoxID.Text;
                    dr["SUBLOTID"] = sCellID;
                    //       dr["BOX_PSTN_NO"] = 1;
                    dr["USERID"] = txtUserId.Text;
                    dr["AOMM_GRD_CODE"] = _AommGrade;

                    RQSTDT.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_NJ", "INDATA", "OUTDATA", RQSTDT);

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
                                //  txtCellID.Focus();
                                return;
                            }
                        }

                        DataColumn dc = new DataColumn("NEW");
                        dc.DefaultValue = "Y";
                        dtRslt.Columns.Add(dc);

                        DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);
                        //int cnt = dtInfo.Rows.Count < 1 ? 0 : Util.NVC_Int(dtInfo.Rows[0]["BOX_PSTN_NO"]);

                        dtRslt.Merge(dtInfo);
                        //if (!dtRslt.Columns.Contains("BOX_PSTN_NO"))
                        //    dtRslt.Columns.Add("BOX_PSTN_NO");
                        //dtRslt.Rows[0]["BOX_PSTN_NO"] = cnt + 1;
                        Util.GridSetData(dgInbox, dtRslt, FrameOperation);
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

        #region Mehod        
        private void Getinbox(string sBoxID)
        {
            try
            {
                Clear();

                DataSet ds = new DataSet();
                DataTable indata = ds.Tables.Add("INDATA");
                //indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("BOXID", typeof(string));
                indata.Columns.Add("OUTER_BOXID", typeof(string));

                DataRow dr = indata.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBoxID;
                if (!string.IsNullOrWhiteSpace(_PalltID))
                    dr["OUTER_BOXID"] = _PalltID;
                if (!string.IsNullOrWhiteSpace(txtInbox.Text))
                    dr["FORM_INBOX"] = txtInbox.Text;
                indata.Rows.Add(dr);

                
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_MB", "INDATA", "OUTDATA,OUTSUBLOT", ds);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    // txtInbox.IsReadOnly = true;
                    txtInbox.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["INBOXID"];
                    txtBoxID.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["BOXID"];
                    txtProdID.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["PRODID"];
                    txtLotID.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["PKG_LOTID"];
                    txtProject.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["PROJECT"];
                    txtProdName.Text = (string)dsResult.Tables["OUTDATA"].Rows[0]["PRODNAME"];
                    txtBoxCellQty.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["TOTAL_QTY"]);
                    txtCellQty.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["SUBLOT_QTY"]);
                }

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTSUBLOT"] != null)
                {
                    DataTable dtInfo = dsResult.Tables["OUTSUBLOT"];
                    DataColumn dc = new DataColumn("NEW");
                    dc.DefaultValue = "N";
                    dtInfo.Columns.Add(dc);
                    Util.GridSetData(dgInbox, dtInfo, FrameOperation);

                    if (dgInbox.GetRowCount() > 0)
                    {
                        dgInbox.Columns["DELETE"].Visibility = Visibility.Collapsed;
                        btnChange.Visibility = Visibility.Visible;
                        btnSave.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgInbox.Columns["DELETE"].Visibility = Visibility.Visible;
                        btnChange.Visibility = Visibility.Collapsed;
                        btnSave.Visibility = Visibility.Visible;
                    }
                }
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
                Util.MessageException(ex);
            }
        }


        private void GetUser(string sUserId)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("USERID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["USERID"] = sUserId;

                dt.Rows.Add(row);

                txtWorker.Text = string.Empty;
                
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_ID", "RQSTDT", "RSLTDT", dt);

                if (result != null && result.Rows.Count > 0)
                {
                    // txtInbox.IsReadOnly = true;
                    txtUserName.Text = (string)result.Rows[0]["USERNAME"];
                    txtUserId.Text = (string)result.Rows[0]["USERID"];
                    txtCellID.Focus();
                }
                else
                {
                    Util.Alert("SFU1592");
                    txtUserName.Text = string.Empty;
                    txtUserId.Text = string.Empty;
                    txtWorker.Focus();
                }
                
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }
        #endregion

        /// <summary>
        ///  BR_PRD_DEL_SUBLOT_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        dgInbox.ItemsSource = DataTableConverter.Convert(drInfo.CopyToDataTable());
                    }
                    else
                        Util.gridClear(dgInbox);

                    txtCellQty.Text = Util.NVC(cnt);
                }

                else
                {
                    // 삭제하시겠습니까?";
                    Util.MessageConfirm("SFU1230", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            inDataTable.Columns.Add("BOXID");
                            inDataTable.Columns.Add("USERID");

                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                            inSublotTable.Columns.Add("SUBLOTID");

                            DataRow newRow = inDataTable.NewRow();
                            newRow["BOXID"] = txtBoxID.Text;
                            newRow["USERID"] = txtUserId.Text;
                            inDataTable.Rows.Add(newRow);

                            newRow = inSublotTable.NewRow();
                            newRow["SUBLOTID"] = sCell;
                            inSublotTable.Rows.Add(newRow);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_SUBLOT_NJ", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }
                                    Getinbox(txtBoxID.Text);
                                //  Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                                //   this.DialogResult = MessageBoxResult.OK;
                            }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                            }, indataSet);
                        }
                    }, sCell);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

            if (txtUserId.Text == string.Empty)
            {
                // SFU3526 사용자 ID를 입력하세요.
                Util.MessageValidation("SFU3526");
                return;
            }

            //SFU1241 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("BOXID");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("FORM_INBOX");

                    DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                    inSublotTable.Columns.Add("SUBLOTID");
                    inSublotTable.Columns.Add("BOX_PSTN_NO");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["BOXID"] = txtBoxID.Text;
                    newRow["FORM_INBOX"] = String.IsNullOrWhiteSpace(txtInbox.Text) ? null : txtInbox.Text;
                    newRow["USERID"] = txtUserId.Text;
                    inDataTable.Rows.Add(newRow);

                    foreach (DataRow dr in drList)
                    {
                        newRow = inSublotTable.NewRow();
                        newRow["SUBLOTID"] = dr["SUBLOTID"];
                        newRow["BOX_PSTN_NO"] = 0; // dr["BOX_PSTN_NO"]; 사용안함
                        inSublotTable.Rows.Add(newRow);
                    }
                    //for (int row = 1; row <= dtInfo.Rows.Count ; row++)
                    //{
                    //    newRow = inSublotTable.NewRow();
                    //    newRow["SUBLOTID"] = dtInfo.Rows[dtInfo.Rows.Count - row]["SUBLOTID"];
                    //    newRow["BOX_PSTN_NO"] = row;
                    //    inSublotTable.Rows.Add(newRow);
                    //}

                    loadingIndicator.Visibility = Visibility.Visible;
                    //BR_PRD_REG_SUBLOT_MB 로 바꿔야 되지만 파우치 전용이라 변경 안함
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SUBLOT_NJ", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            Getinbox(txtBoxID.Text);
                            this.DialogResult = MessageBoxResult.OK;
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, indataSet);
                }
            });
        }

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

        private void Clear()
        {
            txtProdID.Text = string.Empty;
            txtProdName.Text = string.Empty;
            txtProject.Text = string.Empty;
            txtInbox.Text = string.Empty;
            txtBoxID.Text = string.Empty;
            txtLotID.Text = string.Empty;
            txtCellID.Text = string.Empty;
            txtBoxCellQty.Text = "0";
            txtCellQty.Text = "0";

            Util.gridClear(dgInbox);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

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
                        txtCellID.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtCellID.Text))
                            txtCellID_KeyDown(txtCellID, null);
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (dgInbox.GetRowCount() <= 0)
            {
                //SFU1209		Cell 정보가 없습니다.	
                Util.MessageValidation("SFU1209");
                return;
            }

            DataRow[] drList = (DataTableConverter.Convert(dgInbox.ItemsSource)).Select("NEW = 'Y'");
            if (drList.Length > 0)
            {
                // SFU4038 변경된 데이터가 존재합니다.\r\n먼저 저장한 후 다시 시도하세요.
                Util.MessageValidation("SFU4038");
                return;
            }

            if (txtUserId.Text == string.Empty)
            {
                // SFU3526 사용자 ID를 입력하세요.
                Util.MessageValidation("SFU3526");
                return;
            }

            FCS002_315_CHANGE_CELL puChange = new FCS002_315_CHANGE_CELL();
            puChange.FrameOperation = FrameOperation;

            if (puChange != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = txtBoxID.Text;
                Parameters[1] = txtUserId.Text;
                Parameters[2] = txtInbox.Text;
                C1WindowExtension.SetParameters(puChange, Parameters);

                puChange.Closed += new EventHandler(puChange_Closed);
              
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(puChange);
                        puChange.BringToFront();
                        break;
                    }
                }
            }
        }

        private void puChange_Closed(object sender, EventArgs e)
        {
            FCS002_315_CHANGE_CELL popup = sender as FCS002_315_CHANGE_CELL;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Getinbox(txtBoxID.Text);
                dgInbox.ScrollIntoView(dgInbox.GetRowCount()-1, 0);
            }
            this.grdMain.Children.Remove(popup);
        }

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

        }

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
