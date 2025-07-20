/*************************************************************************************
 Created Date : 2017.03.17
      Creator : 정문교
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.17  정문교 : Initial Created.
  2018.08.13  신광희 : 남경CNJ 요청으로 재공생성, 재공삭제 분류 하여 별도 메뉴 구성
  2020.04.14  고재영 : 기준정보에 따라 체크 변경 여부 설정
**************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
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
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_248 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private string _emptyLot = string.Empty;
        private DataTable _createTable = new DataTable();
        private string _UserID = string.Empty; //직접 실행하는 USerID
        public COM001_248()
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

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveCr);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            if (GetAreaCommoncode("CREATE_INVENTORY_CHECK", "ERP_CREATE_INVENTORY_DEFAULT_CHECK") == "Y")
            {
                chkERP.IsChecked = true;
                chkERP.IsEnabled = false;
            }
            else
                chkERP.IsChecked = false;

            Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearchHistory()) return;

                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_STOCK_INV_CANCEL_TERM_LOT";

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

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

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

        #endregion

        #region [생성,삭제]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;
        

            if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED") // CWA 조립3동 혹은 전극2동
            {
                if (LoginInfo.USERTYPE == "P") //공정PC
                {
                    LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM authConfirm = new LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM();
                    authConfirm.FrameOperation = FrameOperation;
                    if (authConfirm != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = LoginInfo.CFG_AREA_ID == "A7" ? "ASSYAU_MANA" : "ELEC_MANA";
                        Parameters[1] = "lgchem.com";
                        C1WindowExtension.SetParameters(authConfirm, Parameters);
                        authConfirm.Closed += new EventHandler(OnCloseAuthConfirm_Save);

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
                else  // 공정PC 아니면
                {
                    // 생성 하시겠습니까?
                    Util.MessageConfirm("SFU1621", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Save();
                        }
                    });


                }
            }
            else  //폴란드 조립3동, 전극2동이 아니면
            {
                // 생성 하시겠습니까?
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
        }

        // <summary>
        // LOT 생성 인증 팝업 닫기
        // </summary>
        // <param name = "sender" ></ param >
        // < param name="e"></param>
        private void OnCloseAuthConfirm_Save(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM window = sender as LGC.GMES.MES.CMM001.CMM_COM_AUTH_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                _UserID = window.UserID;
                Save();
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

        #endregion

        #region [생성,삭제]
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
            {
                txtWipNoteCr.Text = string.Empty;
                txtUserNameCr.Text = string.Empty;
                txtUserNameCr.Tag = string.Empty;
                chkERP.IsChecked = false;
                Util.gridClear(dgListCreate);
                _emptyLot = string.Empty;
                _createTable = new DataTable();
            }
            else
            {
                Util.gridClear(dgListHistory);
                _emptyLot = string.Empty;
            }
        }
        #endregion

        #region [대상 선택하기]
        private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            var dataGrid = dgListCreate;
            dataGrid.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            if (rb != null && rb.DataContext == null)
                return;

            if (rb?.IsChecked != null && ((bool)rb.IsChecked && ((DataRowView)rb.DataContext).Row["CHK"].ToString().Equals("0")))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                DataTable dtLot = DataTableConverter.Convert(dataGrid.ItemsSource);

                // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
                dtLot.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                dtLot.Rows[idx]["CHK"] = 1;
                dtLot.AcceptChanges();

                //Util.GridSetData(dataGrid, dtLot, null, false);
                dataGrid.ItemsSource = DataTableConverter.Convert(dtLot);

                //row 색 바꾸기
                dataGrid.SelectedIndex = idx;
            }

        }
        #endregion

        #region [LOT ID]
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region [요청자]
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


        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgListHistory);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgListHistory);
        }


        private void chModelChange_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (chk?.IsChecked != null && (bool)chk.IsChecked)
            {
                chkERP.IsChecked = false;
                chkERP.IsEnabled = false;
            }
        }

        private void chModelChange_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (chk?.IsChecked != null && !(bool)chk.IsChecked)
            {
                if (GetAreaCommoncode("CREATE_INVENTORY_CHECK", "ERP_CREATE_INVENTORY_DEFAULT_CHECK") == "Y")
                {
                    chkERP.IsChecked = true;
                    chkERP.IsEnabled = false;
                }
                else
                {
                    chkERP.IsEnabled = true;
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

        #endregion

        #endregion

        #region Mehod

        #region [대상목록 가져오기]
        public void GetLotList()
        {
            try
            {
                TextBox tb = txtLotIDCr;

                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageInfo("SFU1190");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_STOCK_INV";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = "TERM";
                dr["LOTID"] = txtLotIDCr.Text;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dgListCreate.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListCreate, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);
                    if (dtResult.Rows.Count != 0)
                    {
                        // 중복체크
                        if (dtInfo.Select("LOTID = '" + Convert.ToString(dtResult.Rows[0]["LOTID"]) + "'").Count() == 0)
                        {
                            dtInfo.Merge(dtResult);
                            Util.GridSetData(dgListCreate, dtInfo, FrameOperation);
                        }
                    }
                }

                _createTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());

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

        #endregion

        #region [생성,삭제]
        private void Save()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                const string bizRuleName = "BR_PRD_REG_STOCK_INV_CANCEL_TERM_LOT";

                _createTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
                inTable.Columns.Add("MODEL_CHANGE_YN", typeof(string));

                DataRow row = inTable.NewRow();
                row["SRCTYPE"] = "UI";
                if (LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED")
                {
                    if (LoginInfo.USERTYPE == "P")
                    {
                        row["USERID"] = _UserID;//LoginInfo.USERID;
                    }
                    else
                    {
                        row["USERID"] = LoginInfo.USERID;
                    }
                }
                else
                {
                    row["USERID"] = LoginInfo.USERID;
                }
                row["REQ_USERID"] = txtUserNameCr.Tag.ToString();
                row["WIPNOTE"] = txtWipNoteCr.Text;
                row["ERP_TRNF_FLAG"] = chkERP.IsChecked.Equals(true) ? "A" : "N";
                row["MODEL_CHANGE_YN"] = chModelChange.IsChecked.Equals(true) ? "Y" : "N";

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("LOTSTAT", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(decimal));
                inLot.Columns.Add("WIPQTY2", typeof(decimal));

                for (int i = 0; i < _createTable.Rows.Count; i++)
                {
                    if (Util.NVC(_createTable.Rows[i]["CHK"]) == "1")
                    {
                        row = inLot.NewRow();

                        row["LOTID"] = Util.NVC(_createTable.Rows[i]["LOTID"]);
                        row["LOTSTAT"] = "RELEASED";
                        row["WIPQTY"] = Util.NVC(_createTable.Rows[i]["WIPQTY"]);
                        row["WIPQTY2"] = Convert.ToDouble(Util.NVC(_createTable.Rows[i]["WIPQTY"])) * Convert.ToDouble(Util.NVC(_createTable.Rows[i]["LANE_QTY"]));

                        inLot.Rows.Add(row);
                    }
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", _createTable.Rows.Count);


                Util.gridClear(dgListCreate);
                _emptyLot = string.Empty;
                _createTable = new DataTable();

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

        #endregion

        #region [공통코드 조회]
        private string GetAreaCommoncode(string TypeCode, string ComCode)
        {
            //동별 공통코드
            string strResult = string.Empty;
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
            RQSTDT.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = TypeCode;
            dr["COM_CODE"] = ComCode;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
            {
                strResult = Util.NVC(dtResult.Rows[0]["ATTR1"]);
            }
            else
            {
                strResult = "N";
            }

            return strResult;
        }
        #endregion

        #region [Validation]
        private bool CanSave()
        {
            List<int> list = _util.GetDataGridCheckRowIndex(dgListCreate, "CHK");
            if (list.Count <= 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            DataTable dt = DataTableConverter.Convert(dgListCreate.ItemsSource);
            DataRow[] dr = dt.Select("CHK = 1");
            double dWipqty;
            bool bResult = double.TryParse(dr[0]["WIPQTY"].ToString(), out dWipqty);

            if (!bResult || dWipqty.Equals(0))
            {
                // 수량을 입력하세요.
                Util.MessageValidation("SFU1684");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWipNoteCr.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrEmpty(txtUserNameCr.Text) || string.IsNullOrEmpty(txtUserNameCr.Tag?.ToString()))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }


            return true;
        }

        #endregion

        #region [Func]
        #region [요청자]
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName = txtUserNameCr.Text;

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
                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;
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

        #endregion



        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((DataGridCellPresenter)((Button)sender).Parent).Row.Index;

                    dgListCreate.IsReadOnly = false;
                    dgListCreate.RemoveRow(index);
                    dgListCreate.IsReadOnly = true;
                    _createTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());
                    Util.GridSetData(dgListCreate, _createTable, FrameOperation);
                    txtLotIDCr.Focus();
                }
            });

        }

        private void txtLotIDCr_PreviewKeyDown(object sender, KeyEventArgs e)
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
                        if (!string.IsNullOrEmpty(item) && Multi_Create(item) == false)
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

        bool Multi_Create(string sLotid)
        {
            try
            {
                DoEvents();

                const string bizRuleName = "DA_PRD_SEL_STOCK_INV";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = "TERM";
                dr["LOTID"] = sLotid;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    if (string.IsNullOrEmpty(_emptyLot))
                        _emptyLot += sLotid;
                    else
                        _emptyLot = _emptyLot + ", " + sLotid;
                }

                if (dgListCreate.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListCreate, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);

                    if (dtResult.Rows.Count != 0)
                    {
                        // 중복체크
                        if (dtInfo.Select("LOTID = '" + Convert.ToString(dtResult.Rows[0]["LOTID"]) + "'").Count() == 0)
                        {
                            dtInfo.Merge(dtResult);
                            Util.GridSetData(dgListCreate, dtInfo, FrameOperation);
                        }
                    }
                }

                _createTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
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

    }
}
