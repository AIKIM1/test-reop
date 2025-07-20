/*************************************************************************************
 Created Date : 2019.08.07
      Creator : 염규범
  Description :
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.07  염규범    Initialize
  2021.11.17  정용석    재공상태가 BIZWF, MOVING인 경우 또는 재공삭제하려는 LOT이 ERP 창고가 있는 경우 재공삭제 LOT 리스트 조회 불가 기능 추가
  2021.11.17  정용석    보이지 않는 Logic 오류 수정 및 멤버변수, 멤버함수 Refactoring 처리
  2023.09.06  김선준    Term 조회 DA->BR로 수정
  2023.09.14  김선준    Term 조회시 Interlock발생하면 loading bar 숨김처리
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_044 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _Util = new Util();
        private string abnormalLOT = string.Empty;      // 재공종료, 재공삭제가 안되는 LOT 리스트
        private string sTabId = string.Empty;

        private DataTable isCreateTable = new DataTable();
        private DataTable isDeleteTable = new DataTable();

        public PACK001_044()
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

        // Initialize
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
            string strProcessCase = string.Empty;
            strProcessCase = "cboProcessPack";
            cbo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent:cboProcessParent, sCase: strProcessCase);

            if (cboProcess.Items.Count < 1)
                SetProcess();


            Set_Combo_COMMCODE(cboWipStatCode);
            //cbo.SetCombo(cboWipStatCode, CommonCombo.ComboStatus.ALL, null, "cboWipStatCode");

            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSaveCr);
            //listAuth.Add(btnSaveDel);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            GetCaldate();

            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

            this.Loaded -= UserControl_Loaded;
        }

        // 재공생성, 재공삭제 조회 버튼
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }

        // 재공생성 버튼
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 생성 하시겠습니까?
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (MathingId(txtUserNameCr.Tag.ToString()))
                    {
                        sTabId = "Create";
                        TerminateCancelLOT();
                    }
                    else
                    {
                        Util.MessageInfo("SFU4967", (result2) =>
                        {
                            if (result2 == MessageBoxResult.OK)
                            {
                                HiddenLoadingIndicator();
                                return;
                            }
                        });
                    }
                }
                else
                {
                    HiddenLoadingIndicator();
                    return;
                }
            });
        }

        private void btnSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            dgListHistory.EndEdit();
            dgListHistory.EndEditRow(true);

            if (!ValidationSaveHistory()) return;

            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveHistoryTab();
                }
            });
        }

        // 재공 생성, 재공 삭제 Clear 버튼
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
            {
                txtWipNoteCr.Text = string.Empty;
                txtUserNameCr.Text = string.Empty;
                txtUserNameCr.Tag = string.Empty;
                Util.gridClear(dgListCreate);
                abnormalLOT = "";
                isCreateTable = new DataTable();
            }
            else if (((FrameworkElement)tbcWip.SelectedItem).Name.Equals("Delete"))
            {
                txtWipNoteDel.Text = string.Empty;
                txtUserNameDel.Text = string.Empty;
                txtUserNameDel.Tag = string.Empty;
                Util.gridClear(dgListDelete);
                abnormalLOT = "";
                isDeleteTable = new DataTable();
            }
            else
            {
                txtWipNoteHistory.Text = string.Empty;
                txtUserNameHistory.Text = string.Empty;
                txtUserNameHistory.Tag = string.Empty;
                Util.gridClear(dgListHistory);

                abnormalLOT = "";
            }
        }

        // 대상 선택하기
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

                dataGrid.ItemsSource = DataTableConverter.Convert(dtLot);

                //row 색 바꾸기
                dataGrid.SelectedIndex = idx;
            }

        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.GetLotList();
            }
        }

        // 요청자
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

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            if (string.Equals(((FrameworkElement)tbcWip.SelectedItem).Name, "Create"))
            {
                userName = txtUserNameCr.Text;
            }
            else if (string.Equals(((FrameworkElement)tbcWip.SelectedItem).Name, "Delete"))
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
                else if (((FrameworkElement)tbcWip.SelectedItem).Name.Equals("Delete"))
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

        // LotInfo 조회
        public void GetLotList()
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
            {
                this.GetLOTListForWipCreate();
            }
            else
            {
                this.GetLOTListForWipDelete();
            }

            this.abnormalLOT = string.Empty;
        }

        // 순서도 호출 - 재공생성 LOT List
        private DataSet GetStockListV2(List<string> lstLOTID)
        {
            DataTable dtINDATA = new DataTable("INDATA");
            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("LOTID", typeof(string));

            DataRow drINDATA = dtINDATA.NewRow();
            drINDATA["LANGID"] = LoginInfo.LANGID;
            drINDATA["LOTID"] = string.Join(",", lstLOTID);

            dtINDATA.Rows.Add(drINDATA);
            DataSet dsINDATA = new DataSet();
            dsINDATA.Tables.Add(dtINDATA);

            return new ClientProxy().ExecuteServiceSync_Multi("DA_PRD_SEL_STOCK_PACK_V2", "INDATA", "OUTDATA", dsINDATA);
        }

        // 순서도 호출 - 재공삭제 LOT List
        private DataSet GetStockList(List<string> lstLOTID)
        {
            DataTable dtINDATA = new DataTable("INDATA");
            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("WIPSTAT", typeof(string));
            dtINDATA.Columns.Add("LOTID", typeof(string));
            dtINDATA.Columns.Add("PROCID", typeof(string));

            DataRow drINDATA = dtINDATA.NewRow();
            drINDATA["LANGID"] = LoginInfo.LANGID;
            drINDATA["WIPSTAT"] = "WAIT,END,EQPT_END,PROC";
            drINDATA["LOTID"] = string.Join(",", lstLOTID);
            drINDATA["PROCID"] = null;

            dtINDATA.Rows.Add(drINDATA);
            DataSet dsINDATA = new DataSet();
            dsINDATA.Tables.Add(dtINDATA);

            return new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_STOCK_PACK_TERM", "INDATA", "OUTDATA", dsINDATA);
        }

        // 재공생성 탭에서의 LotInfo 조회 (N빵 및 Text입력)
        private void GetLOTListForWipCreate(List<string> lstLOTID)
        {
            if (lstLOTID == null || lstLOTID.Count <= 0)
            {
                // 조회할 LOT ID 를 입력하세요.
                Util.MessageInfo("SFU1190");
                return;
            }

            // 로딩그림 보여주기
            this.ShowLoadingIndicator();
            PackCommon.DoEvents();

            // LOTID 중복체크
            DataTable dtExistsSoundLotList = DataTableConverter.Convert(this.dgListCreate.ItemsSource);
            var query = dtExistsSoundLotList.AsEnumerable().Where(x => lstLOTID.Where(y => y.Equals(x.Field<string>("LOTID"))).Any());
            if (query.Count() > 0)
            {
                Util.MessageValidation("SFU1384");
                HiddenLoadingIndicator();
                return;
            }

            // 순서도 호출
            DataSet dsResult = this.GetStockListV2(lstLOTID);

            // Data Binding
            if (!CommonVerify.HasTableInDataSet(dsResult) || !CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]))
            {
                Util.MessageValidation("SFU3536");
                this.HiddenLoadingIndicator();
                return;
            }

            // 건전 Lot List와 불건전 LOT List 분리
            // 불건전 LOT List
            var queryUnwholesomenessLOTList = dsResult.Tables["OUTDATA"].AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Field<string>("RESNCODE")));

            // 건전 LOT List
            var querySoundLOTList = dsResult.Tables["OUTDATA"].AsEnumerable().Except(queryUnwholesomenessLOTList);

            // 건전 LOT List Grid Data Binding
            if (querySoundLOTList.Count() > 0)
            {
                DataTable dtBinding = dtExistsSoundLotList.AsEnumerable().Union(querySoundLOTList).CopyToDataTable();
                Util.GridSetData(this.dgListCreate, dtBinding, FrameOperation, false);
                this.isCreateTable = DataTableConverter.Convert(this.dgListCreate.GetCurrentItems());
            }

            // 로딩그림 숨기기
            this.HiddenLoadingIndicator();

            // 불건전 LOT List가 존재하면 Popup 띄우기
            if (queryUnwholesomenessLOTList.Count() > 0)
            {
                PACK001_044_POPUP popup = new PACK001_044_POPUP();
                popup.FrameOperation = FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = queryUnwholesomenessLOTList.CopyToDataTable();
                    C1WindowExtension.SetParameters(popup, Parameters);
                    popup.Closed += new EventHandler(CreatePopupWindow_Closed);
                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
            }
        }

        // 재공생성 탭에서의 LotInfo 조회
        private void GetLOTListForWipCreate()
        {
            try
            {
                List<string> lstLOTID = new List<string>();
                if (!string.IsNullOrEmpty(this.txtLotIDCr.Text))
                {
                    lstLOTID.Add(this.txtLotIDCr.Text);
                }

                this.GetLOTListForWipCreate(lstLOTID);
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

        // 재공삭제 탭에서의 LotInfo 조회 (N빵 및 Text입력)
        private void GetLOTListForWipDelete(List<string> lstLOTID)
        {
            if (lstLOTID == null || lstLOTID.Count <= 0)
            {
                // 조회할 LOT ID 를 입력하세요.
                Util.MessageInfo("SFU1190");
                return;
            }

            // 로딩그림 보여주기
            this.ShowLoadingIndicator();
            PackCommon.DoEvents();

            // LOTID 중복체크
            DataTable dtExistsSoundLotList = DataTableConverter.Convert(this.dgListDelete.ItemsSource);
            var query = dtExistsSoundLotList.AsEnumerable().Where(x => lstLOTID.Where(y => y.Equals(x.Field<string>("LOTID"))).Any());
            if (query.Count() > 0)
            {
                Util.MessageValidation("SFU1384");
                HiddenLoadingIndicator();
                return;
            }

            // 순서도 호출
            DataSet dsResult;
            try
            {
                dsResult = this.GetStockList(lstLOTID);
            }
            catch (Exception ex)
            {
                this.HiddenLoadingIndicator();
                Util.MessageException(ex);
                return;
            }

            // Data Binding
            if (!CommonVerify.HasTableInDataSet(dsResult) || !CommonVerify.HasTableRow(dsResult.Tables["OUTDATA"]))
            {
                Util.MessageValidation("SFU3536");
                this.HiddenLoadingIndicator();
                return;
            }

            // 건전 Lot List와 불건전 LOT List 분리
            // As-Is : PR000 / PS000 공정 Lot 만 처리 가능
            // To-Be 2021-11-15 : 재공상태가 BIZWF인 Lot일 경우 재공 종료 처리 불가
            // To-Be 2021-11-15 : 재공상태가 MOVING인 Lot일 경우 재공 종료 처리 불가
            // To-Be 2021-11-15 : 공정이 PB000 (반품후 대기) LOT인 경우 재공 종료 처리 불가
            // To-Be 2021-11-15 : 공정이 PD000 (등외품 관리) LOT인 경우 재공 종료 처리 불가
            // To-Be 2021-11-15 : ERP 창고가 NULL 이 아닌 Lot일 경우
            // To-Be 2021-12-24 : 공정이 P1뭐시깽이인것들은 재공 종료 처리 불가 (ERP 창고가 NULL인 아닌 것들은 재공 종료 처리 불가 걷어냄)
            // To-Be 2021-12-24 : 공정이 P5뭐시깽이인것들은 재공 종료 처리 불가 (ERP 창고가 NULL인 아닌 것들은 재공 종료 처리 불가 걷어냄)
            // To-Be 2021-12-24 : 공정이 P9뭐시깽이인것들은 재공 종료 처리 불가 (ERP 창고가 NULL인 아닌 것들은 재공 종료 처리 불가 걷어냄)
            // To-Be 2021-12-24 : 반품이력이 있는 LOT에 대해서 재공 삭제 불가 INTERLOCK 적용
            // 불건전 LOT List
            var queryUnwholesomenessLOTList = dsResult.Tables["OUTDATA"].AsEnumerable().Where(x => x.Field<string>("WIPSTAT").Equals("BIZWF") ||
                                                                                                   x.Field<string>("WIPSTAT").Equals("MOVING") ||
                                                                                                   x.Field<string>("PROCID").Equals("PB000") ||
                                                                                                   x.Field<string>("PROCID").Equals("PD000") ||
                                                                                                   x.Field<string>("PROCID").StartsWith("P1") ||
                                                                                                   x.Field<string>("PROCID").StartsWith("P5") ||
                                                                                                   x.Field<string>("PROCID").StartsWith("P9"));
                                                                                                   //!string.IsNullOrEmpty(x.Field<string>("RCV_ISS_PRODID")));     // 2022-02-07 반픔이력 관련 재공종료 불가 INTERLOCK ROLLBACK
            // 건전 LOT List
            var querySoundLOTList = dsResult.Tables["OUTDATA"].AsEnumerable().Except(queryUnwholesomenessLOTList);

            // 건전 LOT List Grid Data Binding
            if (querySoundLOTList.Count() > 0)
            {
                DataTable dtBinding = dtExistsSoundLotList.AsEnumerable().Union(querySoundLOTList).CopyToDataTable();
                Util.GridSetData(this.dgListDelete, dtBinding, FrameOperation, false);
                this.isDeleteTable = DataTableConverter.Convert(this.dgListDelete.GetCurrentItems());
            }

            // 로딩그림 숨기기
            this.HiddenLoadingIndicator();

            // 불건전 LOT List가 존재하면 Popup 띄우기
            if (queryUnwholesomenessLOTList.Count() > 0)
            {
                PACK001_044_POPUP_TERM popup = new PACK001_044_POPUP_TERM();
                popup.FrameOperation = FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = queryUnwholesomenessLOTList.CopyToDataTable();
                    C1WindowExtension.SetParameters(popup, Parameters);
                    popup.Closed += new EventHandler(DeletePopupWindows_Closed);
                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
            }
        }

        // 재공삭제 탭에서의 LotInfo 조회
        private void GetLOTListForWipDelete()
        {
            List<string> lstLOTID = new List<string>();
            if (!string.IsNullOrEmpty(this.txtLotIDDel.Text))
            {
                lstLOTID.Add(this.txtLotIDDel.Text);
            }

            this.GetLOTListForWipDelete(lstLOTID);
        }

        // 재공생성 Transaction
        private void TerminateCancelLOT()
        {
            // 기존 재공종료 Transaction 순서도명 BR_PRD_REG_STOCK_PACK_TERM_WIP_CANCEL
            // 변경 재공종료 Transaction 순서도명 BR_PRD_REG_STOCK_PACK_TERM_WIP_CANCEL_V2
            try
            {
                ShowLoadingIndicator();
                PackCommon.DoEvents();

                isCreateTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());
                string bizRuleName = "BR_PRD_REG_STOCK_PACK_TERM_WIP_CANCEL_V2";

                DataSet ds = new DataSet();
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("REQ_USERID", typeof(string));
                dtINDATA.Columns.Add("WIPNOTE", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));

                var queryLOTList = this.isCreateTable.AsEnumerable().Where(x => x.Field<bool>("CHK").Equals(true)).Select(x => x.Field<string>("LOTID")).ToArray();// MES 2.0 CHK 컬럼 Bool 오류 Patch
                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["REQ_USERID"] = this.txtUserNameCr.Tag.ToString();
                drINDATA["WIPNOTE"] = this.txtWipNoteCr.Text;
                drINDATA["LOTID"] = string.Join(",", queryLOTList);
                dtINDATA.Rows.Add(drINDATA);
                ds.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", ds.Tables.OfType<DataTable>().Select(dt => dt.TableName).ToList());
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, ds);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", isCreateTable.Rows.Count);
                Util.gridClear(dgListCreate);
                abnormalLOT = "";
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

        // 재공종료 Transaction
        private void TerminateLOT()
        {
            // 기존 재공종료 Transaction 순서도명 BR_PRD_REG_STOCK_PACK_TERM_WIP
            // 변경 재공종료 Transaction 순서도명 BR_PRD_REG_STOCK_PACK_TERM_WIP_V2
            try
            {
                ShowLoadingIndicator();
                PackCommon.DoEvents();

                string bizRuleName = "BR_PRD_REG_STOCK_PACK_TERM_WIP_V2";
                DataSet ds = new DataSet();

                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("REQ_USERID", typeof(string));
                dtINDATA.Columns.Add("WIPNOTE", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));

                var queryLOTList = isDeleteTable.AsEnumerable().Where(x => x.Field<bool>("CHK") == true).Select(x => x.Field<string>("LOTID")).ToArray();
                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["REQ_USERID"] = txtUserNameDel.Tag.ToString();
                drINDATA["WIPNOTE"] = this.txtWipNoteDel.Text;
                drINDATA["LOTID"] = string.Join(",", queryLOTList);
                dtINDATA.Rows.Add(drINDATA);
                ds.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", ds.Tables.OfType<DataTable>().Select(dt => dt.TableName).ToList());
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, ds);

                //[% 1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", isDeleteTable.Rows.Count);
                Util.gridClear(dgListDelete);
                abnormalLOT = "";
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


        private void SaveHistoryTab()
        {
            try
            {
                ShowLoadingIndicator();
                PackCommon.DoEvents();

                string sBizName = "BR_PRD_REG_STOCK_PACK_TERM_WIP_CANCEL";

                isCreateTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["USERID"] = LoginInfo.USERID;
                row["REQ_USERID"] = txtUserNameCr.Tag.ToString();
                row["WIPNOTE"] = txtWipNoteCr.Text;

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(Decimal));
                inLot.Columns.Add("WIPQTY2", typeof(Decimal));

                row = null;

                for (int i = 0; i < isCreateTable.Rows.Count; i++)
                {
                    if (Util.NVC(isCreateTable.Rows[i]["CHK"]) == "1" || Util.NVC(isCreateTable.Rows[i]["CHK"]) == "True")
                    {
                        row = inLot.NewRow();

                        row["LOTID"] = Util.NVC(isCreateTable.Rows[i]["LOTID"]);
                        row["WIPQTY"] = Util.NVC(isCreateTable.Rows[i]["WIPQTY"]);
                        row["WIPQTY2"] = Convert.ToDouble(Util.NVC(isCreateTable.Rows[i]["WIPQTY"]));

                        inLot.Rows.Add(row);

                    }
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", isCreateTable.Rows.Count);

                Util.gridClear(dgListCreate);
                abnormalLOT = "";
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

        // Validation
        private bool CanSave()
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
            {

                List<int> list = _Util.GetDataGridCheckRowIndex(dgListCreate, "CHK");
                if (list.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataTable dt = DataTableConverter.Convert(dgListCreate.ItemsSource);
                DataRow[] dr = dt.Select("CHK = true");// MES 2.0 CHK 컬럼 Bool 오류 Patch
                double dWipqty = 0;
                bool bResult = true;

                bResult = double.TryParse(dr[0]["WIPQTY"].ToString(), out dWipqty);

                //if (!bResult || dWipqty == 0)
                //{
                //    // 수량을 입력하세요.
                //    Util.MessageValidation("SFU1684");
                //    return false;
                //}

                if (string.IsNullOrWhiteSpace(txtWipNoteCr.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || txtUserNameCr.Tag == null)
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
            }
            else
            {
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

                if (string.IsNullOrWhiteSpace(txtUserNameDel.Text) || txtUserNameDel.Tag == null)
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
            }

            return true;
        }

        // Validation
        private Boolean MathingId(String UserId)
        {
            ShowLoadingIndicator();
            PackCommon.DoEvents();

            try
            {
                string sBizName = "DA_BAS_SEL_PERSON_TBL";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = UserId;

                inTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "RQSTDT", "RSLTDT", ds);
                DataTable dtResult = dsResult.Tables["RSLTDT"];


                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                {
                    if (txtUserNameCr.Text.Equals(dtResult.Rows[0]["USERNAME"]))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (txtUserNameDel.Text.Equals(dtResult.Rows[0]["USERNAME"]))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Validation
        private bool ValidationSearchHistory()
        {

            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1499");
                return false;
            }

            return true;
        }

        // Validation
        private bool ValidationSaveHistory()
        {
            List<int> list = _Util.GetDataGridCheckRowIndex(dgListHistory, "CHK");
            if (list.Count <= 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

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

        // 재공삭제 LOT N빵 입력
        private void txtLotIDDel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) || e.Key != Key.V)
            {
                return;
            }

            try
            {
                string[] stringSeparators = new string[] { "\r\n" };
                var lstClipboardData = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None).ToList<string>();
                var lstLOTIDList = lstClipboardData.Where(x => !string.IsNullOrEmpty(x));       // 빈값으로 들어온것 제거

                // Validation Check...
                if (lstLOTIDList.Count() > 500)
                {
                    Util.MessageValidation("SFU8102");   // 최대 100개 까지 가능합니다.
                    return;
                }

                // 재공조회
                this.GetLOTListForWipDelete(lstLOTIDList.ToList<string>());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                this.abnormalLOT = string.Empty;
            }

            e.Handled = true;
        }

        // 재공종료 버튼
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (MathingId(txtUserNameDel.Tag.ToString()))
                    {
                        sTabId = "Deleted";
                        TerminateLOT();
                    }
                    else
                    {
                        Util.MessageInfo("SFU4967", (result2) =>
                        {
                            if (result2 == MessageBoxResult.OK)
                            {
                                HiddenLoadingIndicator();
                                return;
                            }
                        });
                    }
                }
                else
                {
                    HiddenLoadingIndicator();
                    return;
                }
            });
        }


        // 재공 종료 삭제 버튼 - 해당 재공 제외처리
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

        // 재공 생성 N빵 입력
        private void txtLotIDCr_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) || e.Key != Key.V)
                {
                    return;
                }

                string[] stringSeparators = new string[] { "\r\n" };
                var lstClipboardData = Clipboard.GetText().Split(stringSeparators, StringSplitOptions.None).ToList<string>();
                var lstLOTIDList = lstClipboardData.Where(x => !string.IsNullOrEmpty(x));       // 빈값으로 들어온것 제거

                // Validation Check...
                if (lstLOTIDList.Count() > 500)
                {
                    Util.MessageValidation("SFU8102");   // 최대 1000개 까지 가능합니다.
                    return;
                }

                // 재공조회
                this.GetLOTListForWipCreate(lstLOTIDList.ToList<string>());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                this.abnormalLOT = string.Empty;
            }

            e.Handled = true;
        }

        // 중복 확인 처리
        private Boolean overLapLot(string strLotId, DataTable dt)
        {
            try
            {
                if (dt.Rows.Count == 0)
                {
                    return true;
                }

                if (dt.Select("LOTID = '" + strLotId + "'").Count() > 0)
                {
                    return false;

                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
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

        // 변경 처리
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearchHistory()) return;

                ShowLoadingIndicator();

                Util.gridClear(dgListHistory);
                PackCommon.DoEvents();

                SetDataGridCheckHeaderInitialize(dgListHistory);

                const string bizRuleName = "DA_PRD_SEL_STOCK_PACK_TERM";

                string strAbnormFlag = string.Empty;

                DataTable inTable = new DataTable { TableName = "RQSTDT" };
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));


                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                dr["EQSGID"] = string.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.GetString()) ? null : cboEquipmentSegment.SelectedValue.GetString();
                dr["PROCID"] = string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()) ? null : cboProcess.SelectedValue.GetString();
                dr["WIPSTAT"] = string.IsNullOrEmpty(cboWipStatCode.SelectedValue.GetString()) ? null : cboWipStatCode.SelectedValue.GetString();

                if (cboWipStatCode.SelectedValue.GetString().Equals("TERM"))
                {
                    strAbnormFlag = "Y";
                    inTable.Columns.Add("ABNORM_FLAG", typeof(string));
                    dr["ABNORM_FLAG"] = strAbnormFlag;
                }
                else if (cboWipStatCode.SelectedValue.GetString().Equals("WAIT,PROC"))
                {
                    strAbnormFlag = "N";
                    inTable.Columns.Add("ABNORM_FLAG", typeof(string));
                    dr["ABNORM_FLAG"] = strAbnormFlag;
                }
                inTable.Rows.Add(dr);



                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "RQSTDT", "RSLTDT", ds);
                DataTable dtResult = dsResult.Tables["RSLTDT"];

                if (dsResult.Tables.Count != 0)
                {
                    if (dgListHistory.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgListHistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgListHistory.ItemsSource);

                        dtInfo.Merge(dtResult);
                        Util.GridSetData(dgListHistory, dtInfo, FrameOperation);
                    }
                }
                else
                {
                    Util.MessageInfo("SFU1486");
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO", "RQSTDT", "RSLTDT", inTable);


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

        private void PopupTerminateCancel()
        {
            try
            {
                PACK001_044_POPUP popup = new PACK001_044_POPUP();
                popup.FrameOperation = FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = abnormalLOT;
                    Parameters[1] = sTabId;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed += new EventHandler(CreatePopupWindow_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndEqpend.ShowModal()));
                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                abnormalLOT = "";
                sTabId = "";
            }
        }

        // 불건전 LOT Interlock Popup 1호 --
        private void PopupTerminate()
        {
            try
            {

                PACK001_044_POPUP_TERM popup = new PACK001_044_POPUP_TERM();
                popup.FrameOperation = FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = abnormalLOT;
                    Parameters[1] = sTabId;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed += new EventHandler(DeletePopupWindows_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndEqpend.ShowModal()));
                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                abnormalLOT = "";
                sTabId = "";
            }
        }

        private void CreatePopupWindow_Closed(object sender, EventArgs e)
        {
            PACK001_044_POPUP window = sender as PACK001_044_POPUP;
            this.grdMain.Children.Remove(window);
        }

        private void DeletePopupWindows_Closed(object sender, EventArgs e)
        {
            PACK001_044_POPUP_TERM window = sender as PACK001_044_POPUP_TERM;
            this.grdMain.Children.Remove(window);
        }

        private void Set_Combo_COMMCODE(C1ComboBox cbo)
        {

            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("CBO_NAME", typeof(String));
            dtResult.Columns.Add("CBO_CODE", typeof(String));

            DataRow dRow = dtResult.NewRow();
            dRow["CBO_NAME"] = "-ALL-";
            dRow["CBO_CODE"] = "TERM,WAIT,PROC";
            dtResult.Rows.InsertAt(dRow, 0);

            dRow = null;
            dRow = dtResult.NewRow();
            dRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("재공종료");
            dRow["CBO_CODE"] = "TERM";
            dtResult.Rows.InsertAt(dRow, 1);

            dRow = null;
            dRow = dtResult.NewRow();
            dRow["CBO_NAME"] = ObjectDic.Instance.GetObjectName("재공생성");
            dRow["CBO_CODE"] = "WAIT,PROC";
            dtResult.Rows.InsertAt(dRow, 2);

            cbo.DisplayMemberPath = cbo.DisplayMemberPath.ToString();
            cbo.SelectedValuePath = cbo.SelectedValuePath.ToString();
            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedIndex = 0;
        }
    }
}