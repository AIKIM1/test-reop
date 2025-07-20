/*************************************************************************************
 Created Date : 2018.05.11
      Creator : 강민석
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.11  강민석, 손우석   CSR ID 3700547 GMES 재공종료 기능 요청 건 요청번호 C20180530_00547
  2019.04.17  염규범           삭제 다중 처리 기능 개선 ( 100개 제한 )
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

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_032 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private string sEmpty_Lot = string.Empty;

        private DataTable isCreateTable = new DataTable();
        private DataTable isDeleteTable = new DataTable();

        private string _emptyLot = string.Empty;

        public PACK001_032()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        #endregion

        #region Event
        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveCr);
            //listAuth.Add(btnSaveDel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region Button
        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }
        #endregion [조회]

        #region [재공 종료]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 재공종료 진행하시겠습니까?
            Util.MessageConfirm("SFU4945", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }
        #endregion [재공 종료]

        #region [Clear]
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
            {
                txtWipNoteCr.Text = string.Empty;
                txtUserNameCr.Text = string.Empty;
                txtUserNameCr.Tag = string.Empty;
                //chkERP.IsChecked = false;
                Util.gridClear(dgListCreate);
                sEmpty_Lot = "";
                isCreateTable = new DataTable();
            }
            else
            {
                sEmpty_Lot = "";
                isDeleteTable = new DataTable();
            }
        }
        #endregion [Clear]
        #region [요청자 조회]
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }
        #endregion [요청자 조회]
        //private void btnDelete_Click(object sender, RoutedEventArgs e)
        //{
        //    //삭제하시겠습니까?
        //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
        //    {
        //        if (result == MessageBoxResult.OK)
        //        {
        //            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

        //            //dgListDelete.IsReadOnly = false;
        //            //dgListDelete.RemoveRow(index);
        //            //dgListDelete.IsReadOnly = true;
        //            //isDeleteTable = DataTableConverter.Convert(dgListDelete.GetCurrentItems());

        //            //txtLotIDDel.Focus();
        //        }
        //    });
        //}
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

                    txtLotID.Focus();
                }
            });
        }
        #endregion Button

        #region [대상 선택하기]
        //private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (sender == null)
        //        return;

        //    C1.WPF.DataGrid.C1DataGrid dataGrid;

        //    if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
        //    {
        //        dataGrid = dgListCreate;
        //    }
                
        //    //else
        //    //    dataGrid = dgListDelete;

        //    //dataGrid.Selection.Clear();

        //    RadioButton rb = sender as RadioButton;

        //    if (rb.DataContext == null)
        //        return;

        //    if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
        //    {
        //        int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

        //        //DataTable dtLot = DataTableConverter.Convert(dataGrid.ItemsSource);

        //        //// 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
        //        //dtLot.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
        //        //dtLot.Rows[idx]["CHK"] = 1;
        //        //dtLot.AcceptChanges();

        //        ////Util.GridSetData(dataGrid, dtLot, null, false);
        //        //dataGrid.ItemsSource = DataTableConverter.Convert(dtLot);

        //        ////row 색 바꾸기
        //        //dataGrid.SelectedIndex = idx;
        //    }

        //}
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
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        #endregion

        private void txtLotID_PreviewKeyDown(object sender, KeyEventArgs e)
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
                        // 입력한 LOTID[% 1] 정보가 없습니다.
                        Util.MessageValidation("SFU3588", sEmpty_Lot);  
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

        #endregion Event

        #region Mehod
        #region [대상목록 가져오기]
        public void GetLotList()
        {
            try
            {
                TextBox tb = new TextBox();

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
                {
                    tb = txtLotID;
                }

                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageInfo("SFU1190");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                string sWipstat = string.Empty;

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
                {
                    sWipstat = "WAIT,PROC,END";
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
                {
                    dr["LOTID"] = txtLotID.Text;
                }

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_PACK", "INDATA", "OUTDATA", inTable);

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
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

                txtLotID.Text = "";
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
        #endregion [대상목록 가져오기]

        bool Multi_Create(string sLotid)
        {
            try
            {
                DoEvents();

                string sWipstat = "WAIT,PROC,END";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_PACK", "INDATA", "OUTDATA", inTable);

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

        #region [생성,삭제]
        private void Save()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                string sBizName = "BR_PRD_REG_STOCK_PACK_CANCEL_TERM_LOT";

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
                row["REQ_USERID"] = txtUserID.Text;
                row["WIPNOTE"] = txtWipNoteCr.Text;

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
                    if (Util.NVC(isCreateTable.Rows[i]["CHK"]) == "1")
                    {
                        if (i == 0)
                        {
                            row = inLot.NewRow();

                            row["LOTID"] = Util.NVC(isCreateTable.Rows[i]["LOTID"]);
                            row["LOTSTAT"] = Util.NVC(isCreateTable.Rows[i]["LOTSTAT"]);
                            row["WIPQTY"] = Util.NVC(isCreateTable.Rows[i]["WIPQTY"]);
                            row["WIPQTY2"] = Util.NVC(isCreateTable.Rows[i]["WIPQTY"]);

                            inLot.Rows.Add(row);
                        }
                        else
                        {
                            inLot.Clear();

                            row = inLot.NewRow();

                            row["LOTID"] = Util.NVC(isCreateTable.Rows[i]["LOTID"]);
                            row["LOTSTAT"] = Util.NVC(isCreateTable.Rows[i]["LOTSTAT"]);
                            row["WIPQTY"] = Util.NVC(isCreateTable.Rows[i]["WIPQTY"]);
                            row["WIPQTY2"] = Util.NVC(isCreateTable.Rows[i]["WIPQTY"]);

                            inLot.Rows.Add(row);
                        }

                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);
                    }
                }

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

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
        #endregion

        #region [Validation]
        private bool CanSave()
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
            {
                List<int> list = _Util.GetDataGridCheckRowIndex(dgListCreate, "CHK");
                if (list.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataTable dt = DataTableConverter.Convert(dgListCreate.ItemsSource);
                DataRow[] dr = dt.Select("CHK = 1");

                if (string.IsNullOrWhiteSpace(txtWipNoteCr.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserID.Text))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
            }
            else
            {

            }

            return true;
        }
        #endregion

        #region [Func]
        #region [요청자]
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserNameCr.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }
        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("WipTerm"))
                {
                    txtUserNameCr.Text = wndPerson.USERNAME;
                    txtUserID.Text = wndPerson.USERID;
                }
            }
            else
            {
                txtUserNameCr.Text = string.Empty;
                txtUserID.Text = string.Empty;
            }
        }
        #endregion [요청자]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion [Func]

        #endregion Mehod        
    }
}
