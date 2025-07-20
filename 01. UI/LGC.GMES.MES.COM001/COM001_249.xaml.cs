/*************************************************************************************
 Created Date : 2017.03.17
      Creator : 정문교
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.17  정문교 : Initial Created.
  2018.08.13  신광희 : 남경CNJ 요청으로 재공생성, 재공삭제 분류 하여 별도 메뉴 구성
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_249 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private string _emptyLot = string.Empty;
        private DataTable _dtDelete = new DataTable();
        private string _UserID = string.Empty; //직접 실행하는 USerID

        public COM001_249()
        {
            InitializeComponent();
            InitializeCombo();
            Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeCombo()
        {

            CommonCombo cbo = new CommonCombo();

            // 동 정보 조회
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            cbo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);
            // 라인 정보 조회
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            cbo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);
            // 공정 정보 조회
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            cbo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cboProcessParent);
            if (cboProcess.Items.Count < 1)
                SetProcess();

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
        }
        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveDel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }

        private void btnSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            dgListHistory.EndEdit();
            dgListHistory.EndEditRow(true);

            if (!ValidationSaveHistory()) return;
                       

            if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED") // CWA 조립3동 혹은 전극2동
            {
                if (LoginInfo.USERTYPE == "P") //공정PC만
                {
                    LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
                    authConfirm.FrameOperation = FrameOperation;
                    if (authConfirm != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = LoginInfo.CFG_AREA_ID == "A7" ? "ASSYAU_MANA" : "ELEC_MANA";
                        Parameters[1] = "lgchem.com";
                        C1WindowExtension.SetParameters(authConfirm, Parameters);
                        authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_SaveHist);

                        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (tmp.Name == "grdMain")
                            {
                                tmp.Children.Add(authConfirm);
                                authConfirm.BringToFront();
                                break;
                            }
                        }
                    }
                }
                else //공정 PC가 아니면
                {

                    Util.MessageConfirm("SFU1621", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            SaveHistory();
                        }
                    });

                }
            }
            else // 폴란드 조립3동, 전극2동을 제외한 나머지
            {
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveHistory();
                    }
                });
            }
        }

        // <summary>
        // LOT 이력저장 인증 팝업 닫기
        // </summary>
        // <param name = "sender" ></ param >
        // < param name="e"></param>
        private void OnCloseAuthConfirm_SaveHist(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _UserID = window.UserID;
                SaveHistory();
            }


            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if(((FrameworkElement)tbcWip.SelectedItem).Name.Equals("Delete"))
            {
                txtWipNoteDel.Text = string.Empty;
                txtUserNameDel.Text = string.Empty;
                txtUserNameDel.Tag = string.Empty;
                Util.gridClear(dgListDelete);
                _emptyLot = string.Empty;
                _dtDelete = new DataTable();
            }
            else
            {
                txtWipNoteHistory.Text = string.Empty;
                txtUserNameHistory.Text = string.Empty;
                txtUserNameHistory.Tag = string.Empty;
                Util.gridClear(dgListHistory);

                _emptyLot = string.Empty;
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        #endregion

        #region Mehod

        public void GetLotList()
        {
            try
            {
                TextBox tb = txtLotIDDel;

                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageInfo("SFU1190");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_STOCK_INV";
                const string wipstat = "WAIT,END,EQPT_END,PROC";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = wipstat;
                dr["LOTID"] = txtLotIDDel.Text;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dgListDelete.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListDelete, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListDelete.ItemsSource);
                    if (dtResult.Rows.Count != 0)
                    {
                        // 중복체크
                        if (dtInfo.Select("LOTID = '" + Convert.ToString(dtResult.Rows[0]["LOTID"]) + "'").Count() == 0)
                        {
                            dtInfo.Merge(dtResult);
                            Util.GridSetData(dgListDelete, dtInfo, FrameOperation);
                        }
                    }
                }

                _dtDelete = DataTableConverter.Convert(dgListDelete.GetCurrentItems());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        private void SaveHistory()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_PRD_REG_STOCK_INV_CANCEL_TERM_LOT";
                DataSet ds = new DataSet();

                //마스터 정보
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
                inTable.Columns.Add("MODEL_CHANGE_YN", typeof(string));

                DataRow row = inTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["REQ_USERID"] = txtUserNameHistory.Tag.ToString();
                row["WIPNOTE"] = txtWipNoteHistory.Text;
                row["ERP_TRNF_FLAG"] = chkERPHistory.IsChecked.Equals(true) ? "A" : "N";
                row["MODEL_CHANGE_YN"] = chModelChangeHistory.IsChecked.Equals(true) ? "Y" : "N";
                inTable.Rows.Add(row);

                DataTable inLot = ds.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("LOTSTAT", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(decimal));
                inLot.Columns.Add("WIPQTY2", typeof(decimal));

                int count = 0;

                foreach (C1.WPF.DataGrid.DataGridRow dataGridRow in dgListHistory.Rows)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHK")) == "True" ||
                        Util.NVC(DataTableConverter.GetValue(dataGridRow.DataItem, "CHK")) == "1")
                    {
                        DataRow param = inLot.NewRow();
                        param["LOTID"] = DataTableConverter.GetValue(dataGridRow.DataItem, "LOTID");
                        param["LOTSTAT"] = "RELEASED";
                        param["WIPQTY"] = DataTableConverter.GetValue(dataGridRow.DataItem, "WIPQTY").GetDecimal();
                        param["WIPQTY2"] = DataTableConverter.GetValue(dataGridRow.DataItem, "WIPQTY").GetDecimal() *
                                           DataTableConverter.GetValue(dataGridRow.DataItem, "LANE_QTY").GetDecimal();
                        inLot.Rows.Add(param);

                        count++;
                    }
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INLOT", null, ds);
                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", count);
                Util.gridClear(dgListHistory);
                _emptyLot = string.Empty;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        #region [Validation]
        private bool CanSave()
        {
            List<int> list = _util.GetDataGridCheckRowIndex(dgListDelete, "CHK");
            if (list.Count <= 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWipNoteDel.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrEmpty(txtUserNameDel.Text) || string.IsNullOrEmpty(txtUserNameDel.Tag?.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private bool ValidationSearchHistory()
        {
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        private bool ValidationSaveHistory()
        {
            List<int> list = _util.GetDataGridCheckRowIndex(dgListHistory, "CHK");
            if (list.Count <= 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            DataTable dtListHistory = ((DataView)dgListHistory.ItemsSource).Table;
            var query = (from t in dtListHistory.AsEnumerable()
                where t.Field<Int64>("CHK") == 1
                select t).ToList();

            if (query.Any())
            {
                foreach (var item in query)
                {
                    if (Util.NVC(item["WIPQTY"]).GetDecimal() == 0)
                    {
                        Util.MessageValidation("SFU3371");
                        return false;
                    }
                }
            }

            if (string.IsNullOrEmpty(txtWipNoteHistory.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrEmpty(txtUserNameHistory.Text) || string.IsNullOrEmpty(txtUserNameHistory.Tag?.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON {FrameOperation = FrameOperation};

            object[] parameters = new object[1];
            string userName = string.Equals(((FrameworkElement) tbcWip.SelectedItem).Name, "Delete") ? txtUserNameDel.Text : txtUserNameHistory.Text;
            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += wndUser_Closed;
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                if (((FrameworkElement) tbcWip.SelectedItem).Name.Equals("Delete"))
                {
                    txtUserNameDel.Text = wndPerson.USERNAME;
                    txtUserNameDel.Tag = wndPerson.USERID;
                }
                else
                {
                    txtUserNameHistory.Text = wndPerson.USERNAME;
                    txtUserNameHistory.Tag = wndPerson.USERID;
                }
            }
        }
        #endregion
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

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        private void txtLotIDDel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    foreach (string item in sPasteStrings)
                    {
                        if (!string.IsNullOrEmpty(item) && Multi_Process(item) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (!string.IsNullOrEmpty(_emptyLot))
                    {
                        Util.MessageValidation("SFU3588", _emptyLot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        _emptyLot = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        public bool Multi_Process(string sLotid)
        {
            try
            {                
                DoEvents();

                const string wipstat = "WAIT,END,EQPT_END,PROC";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string)); 
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = wipstat;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    if (_emptyLot == string.Empty)
                        _emptyLot += sLotid;
                    else
                        _emptyLot = _emptyLot + ", " + sLotid;
                }

                if (dgListDelete.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListDelete, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListDelete.ItemsSource);
                    if (dtResult.Rows.Count != 0)
                    {
                        // 중복체크
                        if (dtInfo.Select("LOTID = '" + Convert.ToString(dtResult.Rows[0]["LOTID"]) + "'").Count() == 0)
                        {
                            dtInfo.Merge(dtResult);
                            Util.GridSetData(dgListDelete, dtInfo, FrameOperation);
                        }
                    }
                }

                _dtDelete = DataTableConverter.Convert(dgListDelete.GetCurrentItems());

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void btnSaveDel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;
           if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED") // CWA 조립3동 혹은 전극2동
            {
                if (LoginInfo.USERTYPE == "P") //공정PC만
                {
                    LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
                    authConfirm.FrameOperation = FrameOperation;
                    if (authConfirm != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = LoginInfo.CFG_AREA_ID == "A7" ? "ASSYAU_MANA" : "ELEC_MANA";
                        Parameters[1] = "lgchem.com";
                        C1WindowExtension.SetParameters(authConfirm, Parameters);
                        authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Delete);

                        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (tmp.Name == "grdMain")
                            {
                                tmp.Children.Add(authConfirm);
                                authConfirm.BringToFront();
                                break;
                            }
                        }
                    }
                }
                else //공정 PC가 아니면
                {
                    // 삭제 하시겠습니까?
                    Util.MessageConfirm("SFU1230", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Term();
                        }
                    });

                }
            }
            else // 폴란드 조립3동, 전극2동을 제외한 나머지
            {
                // 삭제 하시겠습니까?
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Term();
                    }
                });
            }
        }
        // <summary>
        // LOT 삭제 인증 팝업 닫기
        // </summary>
        // <param name = "sender" ></ param >
        // < param name="e"></param>
        private void OnCloseAuthConfirm_Delete(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _UserID = window.UserID;
                Term();
            }


            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
        }

        private void Term()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_PRD_REG_STOCK_INV_TERM_LOT";
                
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));

                var row = inTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["REQ_USERID"] = txtUserNameDel.Tag.ToString();
                row["WIPNOTE"] = txtWipNoteDel.Text;

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("LOTSTAT", typeof(string));

                for (int i = 0; i < _dtDelete.Rows.Count; i++)
                {
                    if (Util.NVC(_dtDelete.Rows[i]["CHK"]) == "1")
                    {
                        row = inLot.NewRow();

                        row["LOTID"] = Util.NVC(_dtDelete.Rows[i]["LOTID"]);
                        row["LOTSTAT"] = "EMPTIED";

                        inLot.Rows.Add(row);

                    }
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", _dtDelete.Rows.Count);

                ////btnClear_Click(null, null);

                Util.gridClear(dgListDelete);
                _emptyLot = string.Empty;
                _dtDelete = new DataTable();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((DataGridCellPresenter)((Button) sender).Parent).Row.Index;

                    dgListDelete.IsReadOnly = false;
                    dgListDelete.RemoveRow(index);
                    dgListDelete.IsReadOnly = true;
                    _dtDelete = DataTableConverter.Convert(dgListDelete.GetCurrentItems());

                    Util.GridSetData(dgListDelete, _dtDelete, FrameOperation);
                    txtLotIDDel.Focus();
                }
            });
        }

        private void chModelChange_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (chk != null && chk.Name == "chModelChangeHistory")
            {
                if (chk.IsChecked != null && (bool) chk.IsChecked)
                {
                    chkERPHistory.IsChecked = false;
                    chkERPHistory.IsEnabled = false;
                }
            }
        }

        private void chModelChange_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (chk != null)
            {
                if (chk.Name == "chModelChangeHistory")
                {
                    if (chk.IsChecked != null && !(bool)chk.IsChecked)
                    {
                        chkERPHistory.IsEnabled = true;
                    }
                }
            }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && !string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue?.GetString()))
            {
                SetProcess();
            }
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                //SetEquipment();
            }
        }

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearchHistory()) return;

                ShowLoadingIndicator();
                DoEvents();

                SetDataGridCheckHeaderInitialize(dgListHistory);

                const string bizRuleName = "DA_PRD_SEL_STOCK_INV_TERM_LOT";

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                //dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                //dr["TODATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
                dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                Util.GridSetData(dgListHistory, dtResult, FrameOperation);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                }
            }
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgListHistory);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgListHistory);
        }

        private void GetCaldate()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; // 2024.10.02 김영국 - Oracle Query상 AREAID BINDING변수가 빠져있어 추가함.
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string area = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(area))
                    return;

                string equipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = area;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(equipmentSegment) ? null : equipmentSegment;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", inTable);


                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }


    }
}
