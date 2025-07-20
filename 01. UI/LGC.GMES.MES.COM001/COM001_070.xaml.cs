/*************************************************************************************
 Created Date : 2017.03.17
      Creator : 정문교
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.17  정문교 : Initial Created.

 
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
    public partial class COM001_070 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private string sEmpty_Lot = string.Empty;

        private DataTable isCreateTable = new DataTable();
        private DataTable isDeleteTable = new DataTable();

        private string _UserID = string.Empty; //직접 실행하는 USerID

        DateTime dCalDate;

        public COM001_070()
        {
            InitializeComponent();
            InitializeCombo();
            this.Loaded += UserControl_Loaded;
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



            // 동 정보 조회
            C1ComboBox[] cboAreaChild2 = { cboEquipmentSegmentCreateHist };
            cbo.SetCombo(cboAreaCreateHist, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild2, sCase: "cboArea");
            // 라인 정보 조회
            C1ComboBox[] cboEquipmentSegmentParent2 = { cboAreaCreateHist };
            C1ComboBox[] cboEquipmentSegmentChild2 = { cboProcessCreateHist };
            cbo.SetCombo(cboEquipmentSegmentCreateHist, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent2, cbChild: cboEquipmentSegmentChild2, sCase: "cboEquipmentSegment");
            // 공정 정보 조회
            C1ComboBox[] cboProcessParent2 = { cboEquipmentSegmentCreateHist };
            cbo.SetCombo(cboProcessCreateHist, CommonCombo.ComboStatus.ALL, null, cboProcessParent2, sCase: "cboProcess");
        }
        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveCr);
            listAuth.Add(btnSaveDel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            dtpCreateHistDateFrom.SelectedDataTimeChanged += dtpCreateHistDate_SelectedDataTimeChanged;
            dtpCreateHistDateTo.SelectedDataTimeChanged += dtpCreateHistDate_SelectedDataTimeChanged;

            dCalDate = GetComSelCalDate();
            dtCalDateCr.SelectedDateTime = dCalDate;
            dtCalDateDel.SelectedDateTime = dCalDate;
            dtCalDateHist.SelectedDateTime = dCalDate;

            //20191028 오화백  ERP 재고생성 및 전기일 지정 가능여부 Visible 체크
            VisibleChk();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion
        #region [전극 조립 구분]
        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex) { }
            return "";
        }
        #endregion
        #region [CHK_WIP_TYPE_CODE]
        private string GetWipType_UseFlag()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CHK_WIP_TYPE_CODE";
                dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["ATTRIBUTE1"]);
            }
            catch (Exception ex) { }
            return "";
        }
        #endregion
        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }
        #endregion

        #region [생성,삭제]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;
           if(LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED") // CWA 조립3동 혹은 전극2동
            {
                if(LoginInfo.USERTYPE == "P") //공정PC
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
                sEmpty_Lot = "";
                isCreateTable = new DataTable();
            }
            else if(((FrameworkElement)tbcWip.SelectedItem).Name.Equals("Delete"))
            {
                txtWipNoteDel.Text = string.Empty;
                txtUserNameDel.Text = string.Empty;
                txtUserNameDel.Tag = string.Empty;
                Util.gridClear(dgListDelete);
                sEmpty_Lot = "";
                isDeleteTable = new DataTable();
            }
            else
            {
                txtWipNoteHistory.Text = string.Empty;
                txtUserNameHistory.Text = string.Empty;
                txtUserNameHistory.Tag = string.Empty;
                Util.gridClear(dgListHistory);

                sEmpty_Lot = "";
            }
        }
        #endregion

        #region [대상 선택하기]
        private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid;

            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                dataGrid = dgListCreate;
            else
                dataGrid = dgListDelete;

            dataGrid.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTable dtLot = DataTableConverter.Convert(dataGrid.ItemsSource);

                // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
                dtLot.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
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
        #endregion

        #endregion

        #region Mehod

        #region [대상목록 가져오기]
        public void GetLotList()
        {
            try
            {
                TextBox tb = new TextBox();

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                    tb = txtLotIDCr;
                else
                    tb = txtLotIDDel;

                //if (tb.Text.Trim().Length < 4)
                //{
                //    // Lot ID는 4자리 이상 넣어 주세요.
                //    Util.MessageInfo("SFU3450");
                //    return;
                //}

                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageInfo("SFU1190");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                string sWipstat = string.Empty;

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                    sWipstat = "TERM";
                else
                    sWipstat = "WAIT,END,EQPT_END,PROC";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                    dr["LOTID"] = txtLotIDCr.Text;
                else
                    dr["LOTID"] = txtLotIDDel.Text;

                inTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_STOCK_INV_BY_CSTORLOT", "INDATA", "OUTDATA", ds);
                DataTable dtResult = dsResult.Tables["OUTDATA"];

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                {
                    if (dgListCreate.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgListCreate, dtResult, FrameOperation);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgListCreate, dtInfo, FrameOperation);
                    }

                    isCreateTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());
                }
                else
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (dtResult.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                        {
                            Util.MessageInfo("SFU1761", dtResult.Rows[i]["LOTID"].ToString());
                            return;
                        }
                    }
                    if (dgListDelete.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgListDelete, dtResult, FrameOperation);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgListDelete.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgListDelete, dtInfo, FrameOperation);
                    }

                    isDeleteTable = DataTableConverter.Convert(dgListDelete.GetCurrentItems());
                }
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

                string sBizName = "BR_PRD_REG_STOCK_INV_CANCEL_TERM_LOT";

                isCreateTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
                inTable.Columns.Add("MODEL_CHANGE_YN", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
                row["SRCTYPE"] = "UI";

                if(LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED")
                {
                    if(LoginInfo.USERTYPE == "P")
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
                row["CALDATE"] = dtCalDateCr.SelectedDateTime.ToString("yyyy-MM-dd");

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("LOTSTAT", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(Decimal));
                inLot.Columns.Add("WIPQTY2", typeof(Decimal));

                row = null;

                for (int i = 0; i < isCreateTable.Rows.Count; i++)
                {
                    if (Util.NVC(isCreateTable.Rows[i]["CHK"]) == "1" || Util.NVC(isCreateTable.Rows[i]["CHK"]) == "True")
                    {
                        row = inLot.NewRow();

                        row["LOTID"] = Util.NVC(isCreateTable.Rows[i]["LOTID"]);
                        row["LOTSTAT"] = "RELEASED";
                        row["WIPQTY"] = Util.NVC(isCreateTable.Rows[i]["WIPQTY"]);
                        //row["WIPQTY2"] = Convert.ToDouble(dr[0]["WIPQTY"].ToString()) * Convert.ToDouble(dr[0]["LANE_QTY"].ToString());
                        row["WIPQTY2"] = Convert.ToDouble(Util.NVC(isCreateTable.Rows[i]["WIPQTY"])) * Convert.ToDouble(Util.NVC(isCreateTable.Rows[i]["LANE_QTY"]));

                        inLot.Rows.Add(row);

                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", isCreateTable.Rows.Count);

                ////btnClear_Click(null, null);

                Util.gridClear(dgListCreate);
                sEmpty_Lot = "";
                isCreateTable = new DataTable();

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
                inTable.Columns.Add("CALDATE", typeof(string));

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

                row["REQ_USERID"] = txtUserNameHistory.Tag.ToString();
                row["WIPNOTE"] = txtWipNoteHistory.Text;
                row["ERP_TRNF_FLAG"] = chkERPHistory.IsChecked.Equals(true) ? "A" : "N";
                row["MODEL_CHANGE_YN"] = chModelChangeHistory.IsChecked.Equals(true) ? "Y" : "N";
                row["CALDATE"] = dtCalDateHist.SelectedDateTime.ToString("yyyy-MM-dd");
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

                //string xml = ds.GetXml();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,INLOT", null, ds);
                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", count);
                Util.gridClear(dgListHistory);
                sEmpty_Lot = "";

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

        #region [Validation]
        private bool CanSave()
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
            {
                //if (dgListCreate.SelectedIndex <= 0)
                //{
                //    // 선택된 항목이 없습니다.
                //    Util.MessageValidation("SFU1651");
                //    return false;
                //}

                List<int> list = _Util.GetDataGridCheckRowIndex(dgListCreate, "CHK");
                if (list.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataTable dt = DataTableConverter.Convert(dgListCreate.ItemsSource);
                DataRow[] dr = dt.Select("CHK = 1");
                double dWipqty = 0;
                bool bResult = true;

                bResult = double.TryParse(dr[0]["WIPQTY"].ToString(), out dWipqty);

                if (!bResult || dWipqty == 0)
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

                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserNameCr.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }

                if (string.Equals(GetAreaType(), "E"))
                {
                    if(string.Equals(GetWipType_UseFlag(), "Y")) { 
                        for (int i = 0; i < dgListCreate.Rows.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgListCreate.Rows[i].DataItem, "WIP_TYPE_CODE").ToString()))
                            {
                                if (DataTableConverter.GetValue(dgListCreate.Rows[i].DataItem, "WIP_TYPE_CODE").ToString().Equals("OUT"))
                                {
                                    Util.MessageValidation("SFU8171");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //if (dgListDelete.SelectedIndex < 0)
                //{
                //    // 선택된 항목이 없습니다.
                //    Util.MessageValidation("SFU1651");
                //    return false;
                //}

                List<int> list = _Util.GetDataGridCheckRowIndex(dgListDelete, "CHK");
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

                if (string.IsNullOrWhiteSpace(txtUserNameDel.Text) || string.IsNullOrWhiteSpace(txtUserNameDel.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
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
            List<int> list = _Util.GetDataGridCheckRowIndex(dgListHistory, "CHK");
            if (list.Count <= 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            //DataTable dt = DataTableConverter.Convert(dgListHistory.ItemsSource);
            //DataRow[] dr = dt.Select("CHK = 1");

            DataTable dtListHistory = ((DataView)dgListHistory.ItemsSource).Table;
            var query = (from t in dtListHistory.AsEnumerable()
                where t.Field<bool>("CHK") == true
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

            if (string.IsNullOrEmpty(txtUserNameHistory.Text) || string.IsNullOrEmpty(txtUserNameHistory.Tag.ToString()))
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
            CMM_PERSON wndPerson = new CMM_PERSON {FrameOperation = FrameOperation};

            object[] parameters = new object[1];
            string userName;

            if (string.Equals(((FrameworkElement) tbcWip.SelectedItem).Name, "Create"))
            {
                userName = txtUserNameCr.Text;
            }
            else if (string.Equals(((FrameworkElement) tbcWip.SelectedItem).Name, "Delete"))
            {
                userName = txtUserNameDel.Text;
            }
            else
            {
                userName = txtUserNameHistory.Text;
            }

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                if (((FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                {
                    txtUserNameCr.Text = wndPerson.USERNAME;
                    txtUserNameCr.Tag = wndPerson.USERID;
                }
                else if (((FrameworkElement) tbcWip.SelectedItem).Name.Equals("Delete"))
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        #endregion

        #endregion

        private void txtLotIDDel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Process(sPasteStrings[i]) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (sEmpty_Lot != "")
                    {
                        Util.MessageValidation("SFU3588", sEmpty_Lot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        sEmpty_Lot = "";
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

                string sWipstat = "WAIT,END,EQPT_END,PROC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string)); 
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_STOCK_INV_BY_CSTORLOT", "INDATA", "OUTDATA", ds);
                DataTable dtResult = dsResult.Tables["OUTDATA"];

                if (dtResult.Rows.Count == 0)
                {
                    if (sEmpty_Lot == "")
                        sEmpty_Lot += sLotid;
                    else
                        sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
                }
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                    {
                        Util.MessageInfo("SFU1761", dtResult.Rows[i]["LOTID"].ToString());
                        return false;
                    }
                }

                if (dgListDelete.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListDelete, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListDelete.ItemsSource);

                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgListDelete, dtInfo, FrameOperation);
                }

                isDeleteTable = DataTableConverter.Convert(dgListDelete.GetCurrentItems());

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

           if(LoginInfo.CFG_AREA_ID == "A7" || LoginInfo.CFG_AREA_ID == "ED") // CWA 조립3동 혹은 전극2동
            {
                if(LoginInfo.USERTYPE == "P") //공정PC만
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

                string sBizName = "BR_PRD_REG_STOCK_INV_TERM_LOT";
                
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("CALDATE", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
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

                row["REQ_USERID"] = txtUserNameDel.Tag.ToString();
                row["WIPNOTE"] = txtWipNoteDel.Text;
                row["CALDATE"] = dtCalDateDel.SelectedDateTime.ToString("yyyy-MM-dd");

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("LOTSTAT", typeof(string));

                row = null;


                for (int i = 0; i < isDeleteTable.Rows.Count; i++)
                {
                    if (Util.NVC(isDeleteTable.Rows[i]["CHK"]) == "1" || Util.NVC(isDeleteTable.Rows[i]["CHK"]) == "True")
                    {
                        row = inLot.NewRow();

                        row["LOTID"] = Util.NVC(isDeleteTable.Rows[i]["LOTID"]);
                        row["LOTSTAT"] = "EMPTIED";

                        inLot.Rows.Add(row);

                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", isDeleteTable.Rows.Count);

                ////btnClear_Click(null, null);

                Util.gridClear(dgListDelete);
                sEmpty_Lot = "";
                isDeleteTable = new DataTable();
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
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgListDelete.IsReadOnly = false;
                    dgListDelete.RemoveRow(index);
                    dgListDelete.IsReadOnly = true;
                    isDeleteTable = DataTableConverter.Convert(dgListDelete.GetCurrentItems());

                    txtLotIDDel.Focus();
                }
            });
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgListCreate.IsReadOnly = false;
                    dgListCreate.RemoveRow(index);
                    dgListCreate.IsReadOnly = true;
                    isCreateTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());

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

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Create(sPasteStrings[i]) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }

                    if (sEmpty_Lot != "")
                    {
                        Util.MessageValidation("SFU3588", sEmpty_Lot);  // 입력한 LOTID[% 1] 정보가 없습니다.
                        sEmpty_Lot = "";
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

                string sWipstat = "TERM";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);
                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_STOCK_INV_BY_CSTORLOT", "INDATA", "OUTDATA", ds);
                DataTable dtResult = dsResult.Tables["OUTDATA"];

                if (dtResult.Rows.Count == 0)
                {
                    if (sEmpty_Lot == "")
                        sEmpty_Lot += sLotid;
                    else
                        sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
                }

                if (dgListCreate.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListCreate, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);

                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgListCreate, dtInfo, FrameOperation);
                }

                isCreateTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void chModelChange_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (chk.Name == "chModelChangeHistory")
            {
                if (chk.IsChecked != null && (bool) chk.IsChecked)
                {
                    chkERPHistory.IsChecked = false;
                    chkERPHistory.IsEnabled = false;
                }
            }
            else
            {
                if (chk.IsChecked != null && (bool)chk.IsChecked)
                {
                    chkERP.IsChecked = false;
                    chkERP.IsEnabled = false;
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
                else
                {
                    if (chk.IsChecked != null && !(bool)chk.IsChecked)
                    {
                        chkERP.IsEnabled = true;
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

                const string bizRuleName = "BR_PRD_SEL_STOCK_INV_TERM_LOT_BY_CSTORLOT";

                DataTable inTable = new DataTable { TableName = "INDATA" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                    //ToShortDateString();
                dr["LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
                dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA", ds);
                DataTable dtResult = dsResult.Tables["OUTDATA"];
                Util.GridSetData(dgListHistory, dtResult, FrameOperation);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

       

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

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

        private void dtpCreateHistDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpCreateHistDateFrom.SelectedDateTime.Year > 1 && dtpCreateHistDateTo.SelectedDateTime.Year > 1)
            {
                if ((dtpCreateHistDateTo.SelectedDateTime - dtpCreateHistDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpCreateHistDateFrom.SelectedDateTime = dtpCreateHistDateTo.SelectedDateTime;
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

                    dtpCreateHistDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpCreateHistDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString()); 
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

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        private void dtCalDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker oDatePicker = sender as LGCDatePicker;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DATE"] = oDatePicker.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ERP_CLOSING_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                oDatePicker.SelectedDateTime = dCalDate;
            }
        }

        /// <summary>
        /// 20191028 오화백  ERP 재고생성여부, 전기일 지정 가능 여부에 대한 동별 공통코드 정보 가져오기
        /// </summary>
        /// <returns></returns>
        private string AreaCommoncode(string AreaId, string TypeCode, string ComCode)
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
            dr["AREAID"] = AreaId;
            dr["COM_TYPE_CODE"] = TypeCode;
            dr["COM_CODE"] = ComCode;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);
            
            if(dtResult.Rows.Count > 0)
            {
                strResult = dtResult.Rows[0]["ATTR1"].ToString();
            }
           else
            {
                strResult = "N";
            }

            return strResult;
        }

        /// <summary>
        /// 20191028 오화백 재고생성여부, 전기일 지정 가능여부 Visible Chk
        /// </summary>
        /// <returns></returns>
        private void VisibleChk()
        {
            //재고생성여부 Chk
            if(AreaCommoncode(LoginInfo.CFG_AREA_ID, "CREATE_INVENTORY_CHECK", "ERP_CREATE_INVENTORY_DEFAULT_CHECK") == "Y")
            {
                chkERP.IsChecked = true;
                chkERPHistory.IsChecked = true;
            }
            else
            {
                chkERP.IsChecked = false;
                chkERPHistory.IsChecked = false;
            }

            //전기일지정가능여부
            if (AreaCommoncode(LoginInfo.CFG_AREA_ID, "CREATE_INVENTORY_CHECK", "POSTING_DATE_BLOCKED") == "Y")
            {
                //재공생성 - 전기일
                txtCalDateCr.Visibility = Visibility.Collapsed;
                dtCalDateCr.Visibility = Visibility.Collapsed;

                //재공삭제 - 전기일
                txtCalDateDel.Visibility = Visibility.Collapsed;
                dtCalDateDel.Visibility = Visibility.Collapsed;

                //재공삭제이력 - 전기일
                txtCalDateHist.Visibility = Visibility.Collapsed;
                dtCalDateHist.Visibility = Visibility.Collapsed;
            }
            else
            {
                //재공생성 - 전기일
                txtCalDateCr.Visibility = Visibility.Visible;
                dtCalDateCr.Visibility = Visibility.Visible;

                //재공삭제 - 전기일
                txtCalDateDel.Visibility = Visibility.Visible;
                dtCalDateDel.Visibility = Visibility.Visible;

                //재공삭제이력 - 전기일
                txtCalDateHist.Visibility = Visibility.Visible;
                dtCalDateHist.Visibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// 20191028 오화백 동변경시  재고생성여부, 전기일 지정 가능여부 Visible Chk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //재고생성여부 Chk
            if (AreaCommoncode(cboArea.SelectedValue.ToString(), "CREATE_INVENTORY_CHECK", "ERP_CREATE_INVENTORY_DEFAULT_CHECK") == "Y")
            {
                chkERPHistory.IsChecked = true;
            }
            else
            {
                chkERPHistory.IsChecked = false;
            }

            //전기일지정가능여부
            if (AreaCommoncode(cboArea.SelectedValue.ToString(), "CREATE_INVENTORY_CHECK", "POSTING_DATE_BLOCKED") == "Y")
            {
                //재공삭제이력 - 전기일
                txtCalDateHist.Visibility = Visibility.Collapsed;
                dtCalDateHist.Visibility = Visibility.Collapsed;
            }
            else
            {
                //재공삭제이력 - 전기일
                txtCalDateHist.Visibility = Visibility.Visible;
                dtCalDateHist.Visibility = Visibility.Visible;
            }
        }

        private void cboAreaCreateHist_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void btnSearchCreateHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboAreaCreateHist.SelectedIndex < 0 || cboAreaCreateHist.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    Util.MessageValidation("SFU1499");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();
                
                const string bizRuleName = "BR_PRD_SEL_STOCK_INV_CANCEL_TERM_LOT_BY_CSTORLOT";

                DataTable inTable = new DataTable { TableName = "INDATA" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = dtpCreateHistDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["TODATE"] = dtpCreateHistDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                //ToShortDateString();
                dr["LOTID"] = string.IsNullOrEmpty(txtCreateHistLotID.Text) ? null : txtCreateHistLotID.Text;
                dr["AREAID"] = cboAreaCreateHist.SelectedValue;
                dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegmentCreateHist.SelectedValue.GetString()) ? null : cboEquipmentSegmentCreateHist.SelectedValue.GetString();
                dr["PROCID"] = string.IsNullOrEmpty(cboProcessCreateHist.SelectedValue.GetString()) ? null : cboProcessCreateHist.SelectedValue.GetString();
                inTable.Rows.Add(dr);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUTDATA", ds);
                DataTable dtResult = dsResult.Tables["OUTDATA"];
                Util.GridSetData(dgListCreateHist, dtResult, FrameOperation);
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void chkHeaderAllCreateHist_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkHeaderAllCreateHist_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
