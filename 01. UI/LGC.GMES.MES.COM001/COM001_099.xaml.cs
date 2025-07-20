/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
 2025.02.03  백상우 : [MES2.0] 제품변경 탭에서 대상목록 Grid에 LotID 중복Row 제거 로직 추가.
 2025.02.03  백상우 : [MES2.0] 평가감 재고 변경 탭 미사용으로 탭 비활성화
 2025.07.05  김선영 : ESHG - 제품변경 탭 : 삭제버튼을 클릭하였으나 그리드에서 삭제되지 않고 남아있음.
**************************************************************************************/

using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_099 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private string sEmpty_Lot = string.Empty;
        string sWipstat = "WAIT,PROC,END,EQPT_END";

        private DataTable isCreateTable = new DataTable();
        private DataTable isDeleteTable = new DataTable();
        
        public COM001_099()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();

            if (LoginInfo.CFG_AREA_ID.ToString().Equals("M1") || LoginInfo.CFG_AREA_ID.ToString().Equals("M2"))
            {
                CreateExcel.Visibility = Visibility.Visible;
                CreateTray.Visibility = Visibility.Visible;
                DeleteTray.Visibility = Visibility.Visible;

                chkOffGrade1.Visibility = Visibility.Visible;
                chkOffGrade2.Visibility = Visibility.Visible;
                chkOffGrade3.Visibility = Visibility.Visible;

                btnCancelOffGrade1.Visibility = Visibility.Visible;
                btnCancelOffGrade2.Visibility = Visibility.Visible;
                btnCancelOffGrade3.Visibility = Visibility.Visible;
            }
            else
            {
                CreateExcel.Visibility = Visibility.Collapsed;
                CreateTray.Visibility = Visibility.Collapsed;
                DeleteTray.Visibility = Visibility.Collapsed;

                chkOffGrade1.Visibility = Visibility.Collapsed;
                chkOffGrade2.Visibility = Visibility.Collapsed;
                chkOffGrade3.Visibility = Visibility.Collapsed;

                btnCancelOffGrade1.Visibility = Visibility.Collapsed;
                btnCancelOffGrade2.Visibility = Visibility.Collapsed;
                btnCancelOffGrade3.Visibility = Visibility.Collapsed;
            }

            //MES 2.0 - 전극일때 탭 비활성화 
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("E"))
            {
                //평가감 재고 변경 탭 미사용으로 탭 비활성화
                Create2.Visibility = Visibility.Collapsed;
            }
            else
            {
                Create2.Visibility = Visibility.Visible;
            }
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll1 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnSaveExcel);
            listAuth.Add(btnSaveTray);
            listAuth.Add(btnSaveDel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");
            _combo.SetCombo(cboAreaTray, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Today.AddDays(1 - DateTime.Today.Day);
            dtpDateTo.SelectedDateTime = DateTime.Now;

            dtpTrayDateFrom.SelectedDateTime = DateTime.Today.AddDays(1 - DateTime.Today.Day);
            dtpTrayDateTo.SelectedDateTime = DateTime.Now;
        }
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpTrayDateFrom.SelectedDataTimeChanged += dtpTrayDateFrom_SelectedDataTimeChanged;
            dtpTrayDateTo.SelectedDataTimeChanged += dtpTrayDateTo_SelectedDataTimeChanged;

        }


        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        private void dtpTrayDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpTrayDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpTrayDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpTrayDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpTrayDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpTrayDateFrom.SelectedDateTime;
                return;
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgListCreate.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgListCreate.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgListCreate.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgListCreate.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        void checkAll1_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll1.IsChecked)
            {
                for (int i = 0; i < dgListDelete.GetRowCount(); i++)
                {
                    //if (Util.NVC(DataTableConverter.GetValue(dgListDelete.Rows[i].DataItem, "ERP_ERR_CODE")).Equals("FAIL"))
                    //{
                    DataTableConverter.SetValue(dgListDelete.Rows[i].DataItem, "CHK", true);
                    //}
                }
            }
        }

        void checkAllTray_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll1.IsChecked)
            {
                for (int i = 0; i < dgListDeleteTray.GetRowCount(); i++)
                {
                    //if (Util.NVC(DataTableConverter.GetValue(dgListDelete.Rows[i].DataItem, "ERP_ERR_CODE")).Equals("FAIL"))
                    //{
                    DataTableConverter.SetValue(dgListDeleteTray.Rows[i].DataItem, "CHK", true);
                    //}
                }
            }
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgListCreate.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgListCreate.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void checkAll1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll1.IsChecked)
            {
                for (int i = 0; i < dgListDelete.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgListDelete.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void checkAllTray_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll1.IsChecked)
            {
                for (int i = 0; i < dgListDeleteTray.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgListDeleteTray.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void CHK_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    int row_index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            //    if (DataTableConverter.GetValue(dgListDelete.Rows[row_index].DataItem, "CHK").Equals("True"))
            //    {
            //        if (DataTableConverter.GetValue(dgListDelete.Rows[row_index].DataItem, "ERP_ERR_CODE") == null || !DataTableConverter.GetValue(dgListDelete.Rows[row_index].DataItem, "ERP_ERR_CODE").Equals("FAIL"))
            //        {
            //            DataTableConverter.SetValue(dgListDelete.Rows[row_index].DataItem, "CHK", "False");
            //            Util.AlertInfo("SFU4911"); //ERP I/F가 실패일 경우에만 재전송 가능합니다.
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.AlertInfo(ex.Message);
            //}
        }

        private void btnReSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string erp_trnf_seqno = string.Empty;
                string chk = string.Empty;
                int chk_cnt = 0;

                //DataTable DT = DataTableConverter.Convert(dgListDelete.ItemsSource);
                for (int i = 0; i < dgListDelete.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgListDelete.Rows[i].DataItem, "CHK").Equals("True"))
                    {
                        if (DataTableConverter.GetValue(dgListDelete.Rows[i].DataItem, "ERP_ERR_CODE") == null || !DataTableConverter.GetValue(dgListDelete.Rows[i].DataItem, "ERP_ERR_CODE").Equals("FAIL"))
                        {
                            DataTableConverter.SetValue(dgListDelete.Rows[i].DataItem, "CHK", "False");
                            Util.AlertInfo("SFU4911"); //ERP I/F가 실패일 경우에만 재전송 가능합니다.
                            return;
                        }
                    }
                }

                // 전송 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3609"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {

                        DataTable inDataTable = new DataTable("INDATA");
                        inDataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                        for (int nrow = 0; nrow < dgListDelete.GetRowCount(); nrow++)
                        {
                            chk = DataTableConverter.GetValue(dgListDelete.Rows[nrow].DataItem, "CHK") as string;
                            erp_trnf_seqno = DataTableConverter.GetValue(dgListDelete.Rows[nrow].DataItem, "ERP_TRNF_SEQNO") as string;

                            if (chk == "True")
                            {
                                DataRow inDataRow = inDataTable.NewRow();
                                inDataRow["ERP_TRNF_SEQNO"] = erp_trnf_seqno;

                                inDataTable.Rows.Add(inDataRow);

                                chk_cnt++;
                            }
                        }

                        if (chk_cnt > 0)
                        {
                            try
                            {
                                new ClientProxy().ExecuteServiceSync("BR_ACT_REG_RESEND_TRSF_POST", "INDATA", null, inDataTable);

                                Util.MessageInfo("SFU1880"); // 전송 완료 되었습니다.

                                GetHistList();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                        else
                        {
                            // 전송 완료 되었습니다.
                            Util.MessageInfo("SFU1636"); // 선택된 대상이 없습니다.
                        }
                    }
                }, false, false, string.Empty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
        }

        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            GetHistList();
        }

        private void btnSearchHistTray_Click(object sender, RoutedEventArgs e)
        {
            GetHistListTray();
        }

        #endregion

        #region [생성,삭제]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 변경 하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }
        #endregion

        #region [생성,삭제]
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            setClear();
        }

        private void setClear()
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
            {
                txtWipNoteCr.Text = string.Empty;
                dtpDate.SelectedDateTime = DateTime.Now;
                txtUserNameCr.Text = string.Empty;
                txtUserNameCr.Tag = string.Empty;
                txtFilePath.Text = string.Empty;
                txtNewProduct.Text = string.Empty;
                Util.gridClear(dgListCreate);
                sEmpty_Lot = "";
                //chkErpSendYn.IsChecked = false;
            } 
            else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create2"))
            {
                txtWipNoteCr2.Text = string.Empty;
                dtpDate2.SelectedDateTime = DateTime.Now;
                txtUserNameCr2.Text = string.Empty;
                txtUserNameCr2.Tag = string.Empty;
                txtFilePath2.Text = string.Empty;
                txtNewProduct2.Text = string.Empty;
                Util.gridClear(dgListCreate2);
                sEmpty_Lot = "";
                //chkErpSendYn.IsChecked = false;
            }
            else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateExcel"))
            {
                txtWipNoteCrExcel.Text = string.Empty;
                dtpDateExcel.SelectedDateTime = DateTime.Now;
                txtUserNameCrExcel.Text = string.Empty;
                txtUserNameCrExcel.Tag = string.Empty;
                txtFilePathExcel.Text = string.Empty;
                Util.gridClear(dgListCreateExcel);
                sEmpty_Lot = "";
                //chkErpSendYnExcel.IsChecked = false;
            }
            else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateTray"))
            {
                txtWipNoteCrTray.Text = string.Empty;
                dtpDateTray.SelectedDateTime = DateTime.Now;
                txtUserNameCrTray.Text = string.Empty;
                txtUserNameCrTray.Tag = string.Empty;
                txtFilePathTray.Text = string.Empty;
                Util.gridClear(dgListCreateTray);
                sEmpty_Lot = "";
                //chkErpSendYnExcel.IsChecked = false;
            }

            else
            {
                txtWipNoteDel.Text = string.Empty;
                txtUserNameDel.Text = string.Empty;
                txtUserNameDel.Tag = string.Empty;
                Util.gridClear(dgListDelete);
                sEmpty_Lot = "";
                //chkHistoryErpSendYn.IsChecked = false;
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

        private void txtLotIDHist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

            }
        }


        private void txtLotIDHistTray_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

            }
        }

        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

            }
        }


        #endregion

        #region [대차 ID]
        private void txtCtnrID_KeyDown(object sender, KeyEventArgs e)
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

        #region Mehod

        #region [대상목록 가져오기]
        public void GetLotList()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLotID.Text) && string.IsNullOrWhiteSpace(txtCtnrID.Text))
                {
                    // 조회할 LOT ID 또는 CART ID 를 입력하세요.
                    Util.MessageInfo("SFU4495");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;

                if (!string.IsNullOrEmpty(txtLotID.Text.Trim()))
                    dr["LOTID"] = txtLotID.Text;

                if (!string.IsNullOrEmpty(txtCtnrID.Text.Trim()))
                    dr["CTNR_ID"] = txtCtnrID.Text.Trim();

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);

                if (dgListCreate.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListCreate, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);

                    dtInfo.Merge(dtResult);
                    DataTable distinctDt = dtInfo.DefaultView.ToTable(true); //중복제거
                    Util.GridSetData(dgListCreate, distinctDt, FrameOperation);
                }

                isCreateTable = DataTableConverter.Convert(dgListCreate.GetCurrentItems());
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

        public void GetLotList2()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    // PALLETID를 입력해주세요
                    Util.MessageInfo("SFU1411");
                    return;
                }

                for (int i = 0; i < dgListCreate2.GetRowCount(); i++)
                {
                    DataRowView drvOld = dgListCreate2.Rows[i].DataItem as DataRowView;
                    if (Util.NVC(drvOld["BOXID"]).Equals(txtPalletID.Text))
                    {
                        // [% 1]은 이미 등록되었습니다.
                        Util.MessageValidation("SFU3471", txtPalletID.Text);
                        return;
                    }
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));

                DataRow dr = inTable.NewRow();

                if (!string.IsNullOrEmpty(txtPalletID.Text.Trim()))
                    dr["BOXID"] = txtPalletID.Text;

                //if (!string.IsNullOrEmpty(txtCtnrID2.Text.Trim()))
                //    dr["CTNR_ID"] = txtCtnrID2.Text.Trim();

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET", "INDATA", "OUTDATA", inTable);

                if (dgListCreate2.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListCreate2, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate2.ItemsSource);

                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgListCreate2, dtInfo, FrameOperation);
                }

                isCreateTable = DataTableConverter.Convert(dgListCreate2.GetCurrentItems());
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

        public void GetHistList()
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(txtLotIDHist.Text))
                //{
                //    // 조회할 LOT ID 를 입력하세요.
                //    Util.MessageInfo("SFU1190");
                //    return;
                //}

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("REQ_USERNAME", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtLotIDHist.Text))
                {
                    dr["LOTID"] = string.IsNullOrWhiteSpace(txtLotIDHist.Text) ? null : txtLotIDHist.Text;
                }
                if (!string.IsNullOrWhiteSpace(txtUser.Text))
                {
                    dr["REQ_USERNAME"] = string.IsNullOrWhiteSpace(txtUser.Text) ? null : txtUser.Text;
                }
                if (string.IsNullOrWhiteSpace(txtLotIDHist.Text) && string.IsNullOrWhiteSpace(txtUser.Text))
                {
                    dr["AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue)) ? null : cboArea.SelectedValue;
                    dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                }

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTOM_HISTORY", "INDATA", "OUTDATA", inTable);
                if (!dtResult.Columns.Contains("CHK"))
                {
                    dtResult.Columns.Add("CHK");
                }

                Util.GridSetData(dgListDelete, dtResult, FrameOperation, true);

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

        public void GetHistListTray()
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(txtLotIDHist.Text))
                //{
                //    // 조회할 LOT ID 를 입력하세요.
                //    Util.MessageInfo("SFU1190");
                //    return;
                //}

                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("REQ_USERNAME", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtLotIDHistTray.Text))
                {
                    dr["LOTID"] = string.IsNullOrWhiteSpace(txtLotIDHistTray.Text) ? null : txtLotIDHistTray.Text;
                }
                if (!string.IsNullOrWhiteSpace(txtUserTray.Text))
                {
                    dr["REQ_USERNAME"] = string.IsNullOrWhiteSpace(txtUserTray.Text) ? null : txtUserTray.Text;
                }
                if (!string.IsNullOrWhiteSpace(txtCSTID.Text))
                {
                    dr["CSTID"] = string.IsNullOrWhiteSpace(txtCSTID.Text) ? null : txtCSTID.Text;
                }
                if (string.IsNullOrWhiteSpace(txtLotIDHistTray.Text) && string.IsNullOrWhiteSpace(txtUserTray.Text) && string.IsNullOrWhiteSpace(txtCSTID.Text))
                {
                    dr["AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboAreaTray.SelectedValue)) ? null : cboAreaTray.SelectedValue;
                    dr["FROMDATE"] = dtpTrayDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TODATE"] = dtpTrayDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                }

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTOM_TRAY_HISTORY", "INDATA", "OUTDATA", inTable);
                if (!dtResult.Columns.Contains("CHK"))
                {
                    dtResult.Columns.Add("CHK");
                }
                
                Util.GridSetData(dgListDeleteTray, dtResult, FrameOperation, true);

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

                DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);

                string sBizName = "BR_ACT_REG_CREATE_MTOM";

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("CALDATE", typeof(DateTime)); // MES 2.0 String => DateTime 으로 변경 (김지환 책임 요청)
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));
                inTable.Columns.Add("ATTCH_FILE_NAME", typeof(string));
                inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
                inTable.Columns.Add("OFFGRADE_FLAG", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
                row["CALDATE"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00").ToDateTime();
                row["PRODID"] = txtNewProduct.Text.ToUpper();
                row["ATTCH_FILE_NAME"] = System.IO.Path.GetFileName(Util.GetCondition(txtFilePath));
                row["ATTCH_FILE_CNTT"] = File.ReadAllBytes(Util.GetCondition(txtFilePath));
                row["USERID"] = LoginInfo.USERID;
                row["REQ_USERID"] = txtUserNameCr.Tag.ToString();
                row["NOTE"] = txtWipNoteCr.Text;
                row["ERP_TRNF_FLAG"] = chkErpSendYn.IsChecked != null && (bool)chkErpSendYn.IsChecked ? "T" : "N";
                row["OFFGRADE_FLAG"] = chkOffGrade1.IsChecked != null && (bool)chkOffGrade1.IsChecked ? "Y" : "N";

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                row = null;

                for (int i = 0; i < dgListCreate.GetRowCount(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(dgListCreate.GetCell(i, dgListCreate.Columns["LOTID"].Index).Value);
                    inLot.Rows.Add(row);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", dgListCreate.GetRowCount());
                 
                btnClear_Click(null, null);
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

        private void Save2()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                int nCount = 0;
                int nTotCount = 0;
                string sTemp = "";
                Decimal dTemp = 0;

                for (int i = 0; i < dgListCreate2.GetRowCount(); i++)
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate2.ItemsSource);

                    string sBizName = "BR_ACT_REG_CREATE_MTOM2";

                    DataSet inData = new DataSet();

                    //마스터 정보
                    DataTable inTable = inData.Tables.Add("INDATA");
                    inTable.Columns.Add("CALDATE", typeof(string));
                    inTable.Columns.Add("PRODID", typeof(string));
                    inTable.Columns.Add("REQ_USERID", typeof(string));
                    inTable.Columns.Add("USERID", typeof(string));
                    inTable.Columns.Add("NOTE", typeof(string));
                    inTable.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));
                    inTable.Columns.Add("ATTCH_FILE_NAME", typeof(string));
                    inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
                    inTable.Columns.Add("BOXID", typeof(string));
                    inTable.Columns.Add("OLD_PRODID", typeof(string));
                    inTable.Columns.Add("TOT_COUNT", typeof(Int32));
                    inTable.Columns.Add("OFFGRADE_FLAG", typeof(string));

                    DataRow row = null;

                    sTemp = Util.NVC(dgListCreate2.GetCell(i, dgListCreate2.Columns["TOTQTY"].Index).Value);
                    Decimal.TryParse(sTemp, out dTemp);
                    nCount = Convert.ToInt32(dTemp);

                    row = inTable.NewRow();
                    row["CALDATE"] = dtpDate2.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    row["PRODID"] = txtNewProduct2.Text.ToUpper();
                    row["ATTCH_FILE_NAME"] = System.IO.Path.GetFileName(Util.GetCondition(txtFilePath2));
                    row["ATTCH_FILE_CNTT"] = File.ReadAllBytes(Util.GetCondition(txtFilePath2));
                    row["USERID"] = LoginInfo.USERID;
                    row["REQ_USERID"] = txtUserNameCr2.Tag.ToString();
                    row["NOTE"] = txtWipNoteCr2.Text;
                    row["ERP_TRNF_FLAG"] = chkErpSendYn2.IsChecked != null && (bool)chkErpSendYn2.IsChecked ? "T" : "N";
                    row["BOXID"] = Util.NVC(dgListCreate2.GetCell(i, dgListCreate2.Columns["BOXID"].Index).Value);
                    row["OLD_PRODID"] = Util.NVC(dgListCreate2.GetCell(i, dgListCreate2.Columns["PRODID"].Index).Value);
                    row["TOT_COUNT"] = nCount;
                    row["OFFGRADE_FLAG"] = chkOffGrade3.IsChecked != null && (bool)chkOffGrade3.IsChecked ? "Y" : "N";
                    inTable.Rows.Add(row);
                    
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INBOX", null, inData);

                    nTotCount = nTotCount + nCount;
                }

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", dgListCreate2.GetRowCount());

                btnClear_Click(null, null);
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

                // List<int> list = _Util.GetDataGridCheckRowIndex(dgListCreate, "CHK");
                if (dgListCreate.Rows.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtNewProduct.Text))
                {
                    // 변경 제품ID가 입력되지 않았습니다.
                    Util.MessageValidation("SFU4036");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtFilePath.Text))
                {
                    // 승인증빙자료가 첨부되지 않았습니다.
                    Util.MessageValidation("SFU4037");
                    return false;
                }

                //DataTable dt = DataTableConverter.Convert(dgListCreate.ItemsSource);
                //DataRow[] dr = dt.Select("CHK = 1");
                //double dWipqty = 0;
                //bool bResult = true;

                //bResult = double.TryParse(dr[0]["WIPQTY"].ToString(), out dWipqty);

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

                if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || string.IsNullOrWhiteSpace(txtUserNameCr.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
            }
            else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateExcel"))
            {
                if (dgListCreateExcel.Rows.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                btnValidationExcel_Click(null, null); //확인 버튼
                if(!getCreateMsgChk())
                {
                    Util.MessageValidation("SFU4974");
                    return false;
                }

                // 사용자 요청으로 제외 함
                //if (string.IsNullOrWhiteSpace(txtFilePath.Text))
                //{
                //    // 승인증빙자료가 첨부되지 않았습니다.
                //    Util.MessageValidation("SFU4037");
                //    return false;
                //}

                if (string.IsNullOrWhiteSpace(txtWipNoteCrExcel.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUserNameCrExcel.Text) || string.IsNullOrWhiteSpace(txtUserNameCrExcel.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }


            }
            else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateTray"))
            {
                if (dgListCreateTray.Rows.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                btnValidationTray_Click(null, null); //확인 버튼
                if (!getCreateTrayMsgChk())
                {
                    Util.MessageValidation("SFU4974");
                    return false;
                }

                // 사용자 요청으로 제외 함
                //if (string.IsNullOrWhiteSpace(txtFilePath.Text))
                //{
                //    // 승인증빙자료가 첨부되지 않았습니다.
                //    Util.MessageValidation("SFU4037");
                //    return false;
                //}

                if (string.IsNullOrWhiteSpace(txtWipNoteCrTray.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUserNameCrTray.Text) || string.IsNullOrWhiteSpace(txtUserNameCrTray.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
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

        private bool CanSave2()
        {
            if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create2"))
            {
                //if (dgListCreate.SelectedIndex <= 0)
                //{
                //    // 선택된 항목이 없습니다.
                //    Util.MessageValidation("SFU1651");
                //    return false;
                //}

                // List<int> list = _Util.GetDataGridCheckRowIndex(dgListCreate, "CHK");
                if (dgListCreate2.Rows.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtNewProduct2.Text))
                {
                    // 변경 제품ID가 입력되지 않았습니다.
                    Util.MessageValidation("SFU4036");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtFilePath2.Text))
                {
                    // 승인증빙자료가 첨부되지 않았습니다.
                    Util.MessageValidation("SFU4037");
                    return false;
                }

                //DataTable dt = DataTableConverter.Convert(dgListCreate.ItemsSource);
                //DataRow[] dr = dt.Select("CHK = 1");
                //double dWipqty = 0;
                //bool bResult = true;

                //bResult = double.TryParse(dr[0]["WIPQTY"].ToString(), out dWipqty);

                //if (!bResult || dWipqty == 0)
                //{
                //    // 수량을 입력하세요.
                //    Util.MessageValidation("SFU1684");
                //    return false;
                //}

                if (string.IsNullOrWhiteSpace(txtWipNoteCr2.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUserNameCr2.Text) || string.IsNullOrWhiteSpace(txtUserNameCr2.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }
            }
            else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateExcel"))
            {
                if (dgListCreateExcel.Rows.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                btnValidationExcel_Click(null, null); //확인 버튼
                if (!getCreateMsgChk())
                {
                    Util.MessageValidation("SFU4974");
                    return false;
                }

                // 사용자 요청으로 제외 함
                //if (string.IsNullOrWhiteSpace(txtFilePath.Text))
                //{
                //    // 승인증빙자료가 첨부되지 않았습니다.
                //    Util.MessageValidation("SFU4037");
                //    return false;
                //}

                if (string.IsNullOrWhiteSpace(txtWipNoteCrExcel.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUserNameCrExcel.Text) || string.IsNullOrWhiteSpace(txtUserNameCrExcel.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
                }


            }
            else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateTray"))
            {
                if (dgListCreateTray.Rows.Count <= 0)
                {
                    // 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                btnValidationTray_Click(null, null); //확인 버튼
                if (!getCreateTrayMsgChk())
                {
                    Util.MessageValidation("SFU4974");
                    return false;
                }

                // 사용자 요청으로 제외 함
                //if (string.IsNullOrWhiteSpace(txtFilePath.Text))
                //{
                //    // 승인증빙자료가 첨부되지 않았습니다.
                //    Util.MessageValidation("SFU4037");
                //    return false;
                //}

                if (string.IsNullOrWhiteSpace(txtWipNoteCrTray.Text))
                {
                    // 사유를 입력하세요.
                    Util.MessageValidation("SFU1594");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtUserNameCrTray.Text) || string.IsNullOrWhiteSpace(txtUserNameCrTray.Tag.ToString()))
                {
                    // 요청자를 입력 하세요.
                    Util.MessageValidation("SFU3451");
                    return false;
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
        #endregion

        #region [Func]
        #region [요청자]
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                string sUserName = "";

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                {
                    sUserName = txtUserNameCr.Text;
                } else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateExcel"))
                {
                    sUserName = txtUserNameCrExcel.Text;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateTray"))
                {
                    sUserName = txtUserNameCrTray.Text;
                } else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Delete"))
                {
                    sUserName = txtUserNameDel.Text;
                } else
                {
                    sUserName = "";
                }

                object[] Parameters = new object[1];
                Parameters[0] = sUserName;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void GetUserWindow2()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                string sUserName = "";

                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create2"))
                {
                    sUserName = txtUserNameCr2.Text;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateExcel"))
                {
                    sUserName = txtUserNameCrExcel.Text;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateTray"))
                {
                    sUserName = txtUserNameCrTray.Text;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Delete"))
                {
                    sUserName = txtUserNameDel.Text;
                }
                else
                {
                    sUserName = "";
                }

                object[] Parameters = new object[1];
                Parameters[0] = sUserName;
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
                if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create"))
                {
                    txtUserNameCr.Text = wndPerson.USERNAME;
                    txtUserNameCr.Tag = wndPerson.USERID;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Create2"))
                {
                    txtUserNameCr2.Text = wndPerson.USERNAME;
                    txtUserNameCr2.Tag = wndPerson.USERID;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateExcel"))
                {
                    txtUserNameCrExcel.Text = wndPerson.USERNAME;
                    txtUserNameCrExcel.Tag = wndPerson.USERID;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("CreateTray"))
                {
                    txtUserNameCrTray.Text = wndPerson.USERNAME;
                    txtUserNameCrTray.Tag = wndPerson.USERID;
                }
                else if (((System.Windows.FrameworkElement)tbcWip.SelectedItem).Name.Equals("Delete"))
                {
                    txtUserNameDel.Text = wndPerson.USERNAME;
                    txtUserNameDel.Tag = wndPerson.USERID;
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

        private void txtLotIDHist_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            //{
            //    try
            //    {
            //        ShowLoadingIndicator();

            //        string[] stringSeparators = new string[] { "\r\n" };
            //        string sPasteString = Clipboard.GetText();
            //        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

            //        if (sPasteStrings.Count() > 100)
            //        {
            //            Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
            //            return;
            //        }

            //        for (int i = 0; i < sPasteStrings.Length; i++)
            //        {
            //            if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Process(sPasteStrings[i]) == false)
            //                break;

            //            System.Windows.Forms.Application.DoEvents();
            //        }

            //        if (sEmpty_Lot != "")
            //        {
            //            Util.MessageValidation("SFU3588", sEmpty_Lot);  // 입력한 LOTID[% 1] 정보가 없습니다.
            //            sEmpty_Lot = "";
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //        return;
            //    }
            //    finally
            //    {
            //        HiddenLoadingIndicator();
            //    }

            //    e.Handled = true;
            //}
        }

        public bool Multi_Process(string sLotid)
        {
            try
            {
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);

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
                    DataTable distinctDt = dtInfo.DefaultView.ToTable(true); //중복제거
                    Util.GridSetData(dgListCreate, distinctDt, FrameOperation);
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public bool Multi_Process2(string sLotid)
        {
            try
            {
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;
                dr["LOTID"] = sLotid;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count == 0)
                {
                    if (sEmpty_Lot == "")
                        sEmpty_Lot += sLotid;
                    else
                        sEmpty_Lot = sEmpty_Lot + ", " + sLotid;
                }

                if (dgListCreate2.GetRowCount() == 0)
                {
                    Util.GridSetData(dgListCreate2, dtResult, FrameOperation);
                }
                else
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate2.ItemsSource);
                    dtInfo.Merge(dtResult);
                    Util.GridSetData(dgListCreate2, dtInfo, FrameOperation);
                }

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

            // 취소 하시겠습니까?
            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Cancel();
                }
            });
        }

        private void Cancel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                string sBizName = "BR_ACT_REG_CANCEL_MTOM";

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("CALDATE", typeof(DateTime)); // MES 2.0 String 에서 DateTime 으로 변경 (이재원 책임 요청)
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
                row["CALDATE"] = dtpDateHist.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00").ToDateTime();
                row["USERID"] = LoginInfo.USERID;
                row["REQ_USERID"] = txtUserNameDel.Tag.ToString();
                row["NOTE"] = txtWipNoteDel.Text;
                row["ERP_TRNF_FLAG"] = chkHistoryErpSendYn.IsChecked != null && (bool)chkHistoryErpSendYn.IsChecked ? "T" : "N";

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("WIPSEQ", typeof(string));
                row = null;

                string sPalletIDs = string.Empty;
                int iSelCnt = 0;
                for (int i = 0; i < dgListDelete.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgListDelete.Rows[i].DataItem, "CHK")) == bool.TrueString)
                    {
                        iSelCnt = iSelCnt + 1;
                        row = inLot.NewRow();

                        row["LOTID"] = Util.NVC(dgListDelete.GetCell(i, dgListDelete.Columns["LOTID"].Index).Value);
                        row["WIPSEQ"] = Util.NVC(dgListDelete.GetCell(i, dgListDelete.Columns["WIPSEQ"].Index).Value);
                        inLot.Rows.Add(row);
                    }
                }

                if (iSelCnt == 0)
                {
                    Util.MessageValidation("SFU1408"); //Pallet ID를 선택을 하신 후 버튼을 클릭해주십시오
                    return;
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", iSelCnt);


                btnClear_Click(null, null);
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

                    //2025.07.05  김선영 : ESHG - 제품변경 탭 : 삭제버튼을 클릭하였으나 그리드에서 삭제되지 않고 남아있음.
                    //dgListCreate.RemoveRow(index); 
                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);
                    dtInfo.Rows.RemoveAt(index);
                    Util.GridSetData(dgListCreate, dtInfo, FrameOperation);
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

                    DataTable dtInfo = DataTableConverter.Convert(dgListCreate2.ItemsSource);
                    dtInfo.Rows.RemoveAt(index);
                    Util.GridSetData(dgListCreate2, dtInfo, FrameOperation);

                    txtPalletID.Focus();
                }
            });
        }

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_INV", "INDATA", "OUTDATA", inTable);

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

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            attachFile(txtFilePath);
        }

        private void attachFile(TextBox txtBox)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "All files (*.*)|*.*";

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openFileDialog.InitialDirectory = @"\\Client\C$";
                }

                else
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        if (new System.IO.FileInfo(filename).Length > 5 * 1024 * 1024) //파일크기 체크
                        {
                            Util.AlertInfo("SFU1926");  //첨부파일 크기는 5M 이하입니다.

                            txtBox.Text = string.Empty;
                        }
                        else
                        {
                            txtBox.Text = filename;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void attachFile2(TextBox txtBox)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "All files (*.*)|*.*";

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    openFileDialog.InitialDirectory = @"\\Client\C$";
                }

                else
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        if (new System.IO.FileInfo(filename).Length > 5 * 1024 * 1024) //파일크기 체크
                        {
                            Util.AlertInfo("SFU1926");  //첨부파일 크기는 5M 이하입니다.

                            txtBox.Text = string.Empty;
                        }
                        else
                        {
                            txtBox.Text = filename;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgListDelete_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "ATTCH_FILE_NAME")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListDelete_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                if (datagrid.CurrentColumn.Name == "ATTCH_FILE_NAME")
                {

                    Point pnt = e.GetPosition(null);
                    C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                    if (cell == null || datagrid.CurrentRow == null)
                        return;

                    string sLotid = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "LOTID"));
                    string sWipSeq = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "WIPSEQ"));
                    string sActDTTM = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "ACTDTTM"));

                    Byte[] sByteAttcFile;

                    //GetAttch_File_Cntt_DW(sLotid, sWipSeq, sActDTTM);

                
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("WIPSEQ", typeof(string));
                    inTable.Columns.Add("ACTDTTM", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = string.IsNullOrWhiteSpace(sLotid) ? null : sLotid;
                    dr["WIPSEQ"] = string.IsNullOrWhiteSpace(sWipSeq) ? null : sWipSeq;
                    dr["ACTDTTM"] = string.IsNullOrWhiteSpace(sActDTTM) ? null : sActDTTM;

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTOM_HISTORY_ATTCH_FILE", "INDATA", "OUTDATA", inTable);

                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        if (dtResult.Columns.Contains("ATTCH_FILE_CNTT")) {

                            // MES 2.0 오류 수정
                            //sByteAttcFile = (Byte[])dtResult.Rows[0]["ATTCH_FILE_CNTT"];
                            string fileCntt = dtResult.Rows[0]["ATTCH_FILE_CNTT"].Nvc();
                            sByteAttcFile = Convert.FromBase64String(fileCntt);
                        }
                        else
                        {
                            sByteAttcFile = null;
                        }

                    } else
                    {
                        sByteAttcFile = null;
                    }

                    // Byte[] bytes = (Byte[])datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["ATTCH_FILE_CNTT"].Index).Value;
                    string sFileName = Util.NVC(datagrid.CurrentCell.Value);
                    string sExt = Util.NVC(sFileName.Split(new string[] { "." }, StringSplitOptions.None)[1]);

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = sFileName;
                    saveFileDialog.DefaultExt = sExt;
                    saveFileDialog.Filter = "All files (*.*)|*.*";
                    saveFileDialog.AddExtension = false;
                    saveFileDialog.CheckFileExists = true;


                    if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                    {
                        saveFileDialog.InitialDirectory = @"\\Client\C$";
                    }
                    else { 
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, sByteAttcFile);

                        //저장이 완료되었습니다.
                        Util.MessageValidation("10004");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListDelete_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre1.Content = chkAll1;
                            e.Column.HeaderPresenter.Content = pre1;
                            chkAll1.Checked -= new RoutedEventHandler(checkAll1_Checked);
                            chkAll1.Unchecked -= new RoutedEventHandler(checkAll1_Unchecked);
                            chkAll1.Checked += new RoutedEventHandler(checkAll1_Checked);
                            chkAll1.Unchecked += new RoutedEventHandler(checkAll1_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void dgListDeleteTray_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "ATTCH_FILE_NAME")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListDeleteTray_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                if (datagrid.CurrentColumn.Name == "ATTCH_FILE_NAME")
                {
                    Point pnt = e.GetPosition(null);
                    C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                    if (cell == null || datagrid.CurrentRow == null)
                    return;

                    string sLotid = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "LOTID"));
                    string sCstid = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "CSTID"));
                    string sActDTTM = Util.NVC(DataTableConverter.GetValue(datagrid.CurrentRow.DataItem, "ACTDTTM"));

                    Byte[] sByteAttcFile;

                    DataTable inTable = new DataTable();                    
                    inTable.Columns.Add("LOTID", typeof(string));
                    inTable.Columns.Add("CSTID", typeof(string));
                    inTable.Columns.Add("ACTDTTM", typeof(string));

                    DataRow dr = inTable.NewRow();                    
                    dr["LOTID"] = string.IsNullOrWhiteSpace(sLotid) ? null : sLotid;
                    dr["CSTID"] = string.IsNullOrWhiteSpace(sCstid) ? null : sCstid;
                    dr["ACTDTTM"] = string.IsNullOrWhiteSpace(sActDTTM) ? null : sActDTTM;

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTOM_TRAY_HISTORY_ATTCH_FILE", "INDATA", "OUTDATA", inTable);

                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        if (dtResult.Columns.Contains("ATTCH_FILE_CNTT"))
                        {

                            sByteAttcFile = (Byte[])dtResult.Rows[0]["ATTCH_FILE_CNTT"];
                        }
                        else
                        {
                            sByteAttcFile = null;
                        }

                    }
                    else
                    {
                        sByteAttcFile = null;
                    }

                   
                   // Byte[] bytes = (Byte[])datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["ATTCH_FILE_CNTT"].Index).Value;
                    string sFileName = Util.NVC(datagrid.CurrentCell.Value);
                    string sExt = Util.NVC(sFileName.Split(new string[] { "." }, StringSplitOptions.None)[1]);

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = sFileName;
                    saveFileDialog.DefaultExt = sExt;
                    saveFileDialog.Filter = "All files (*.*)|*.*";
                    saveFileDialog.AddExtension = false;
                    saveFileDialog.CheckFileExists = false;


                    if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                    {
                        saveFileDialog.InitialDirectory = @"\\Client\C$";
                    }
                    else
                    {
                        saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    }

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, sByteAttcFile);

                        //저장이 완료되었습니다.
                        Util.MessageValidation("10004");
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListDeleteTray_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre1.Content = chkAll1;
                            e.Column.HeaderPresenter.Content = pre1;
                            chkAll1.Checked -= new RoutedEventHandler(checkAllTray_Checked);
                            chkAll1.Unchecked -= new RoutedEventHandler(checkAllTray_Unchecked);
                            chkAll1.Checked += new RoutedEventHandler(checkAllTray_Checked);
                            chkAll1.Unchecked += new RoutedEventHandler(checkAllTray_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        /// <summary>
        /// 엑셀 파일 양식 받기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoadExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "MTOM_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "LOTID";
                    sheet[1, 0].Value = "LOTID";

                    sheet[0, 1].Value = "PRODID";
                    sheet[1, 1].Value = "변경대상제품코드";

                    sheet[1, 4].Value = "<-- 2라인 삭제 후 입력";

                    sheet[0, 0].Style = sheet[0, 1].Style = sheet[0, 2].Style = styel;
                    sheet.Columns[0].Width = sheet.Columns[1].Width = sheet.Columns[2].Width = 1500;

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 엑셀 파일 오픈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenExcel_Click(object sender, RoutedEventArgs e)
        {

            ExcelMng exl = new ExcelMng();

            try
            {

                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                       // DataTable dtResult = exl.GetSheetData(str[0]);

                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        if (sheet.GetCell(0, 0).Text != "LOTID" || sheet.GetCell(0, 1).Text != "PRODID")
                        {
                            Util.MessageValidation("SFU4424");
                            return;
                        }


                        DataTable dtINLOT = getExcelFileToDataTable();
                        DataRow newRow = null;


                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (string.IsNullOrEmpty(sheet.GetCell(rowInx, 0).Text))
                                break;

                            newRow = dtINLOT.NewRow();

                            newRow["LOTID"] = sheet.GetCell(rowInx, 0).Text;
                            newRow["MTOM_PRODID"] = sheet.GetCell(rowInx, 1).Text;

                            dtINLOT.Rows.Add(newRow);
                        }

                        setgrListCreateExcel_DataSet(dtINLOT);
                    }
                }
            }
            catch (Exception ex)
            {
                if (exl != null)
                {
                    //이전 연결 해제
                    exl.Conn_Close();
                }
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }

        }

        /// <summary>
        /// 엑셀 목록 일괄 데이터 조회 데이터 셋
        /// </summary>
        /// <returns></returns>
        private DataSet getExcelFileShechIndataSet()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inTable = inData.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("WIPSTAT", typeof(string));



            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("MTOM_PRODID", typeof(string));
            inLot.Columns.Add("ROWNUM", typeof(string));

            return inData;
        }

        /// <summary>
        /// 데이터 테이블 셋
        /// </summary>
        /// <returns></returns>
        private DataTable getExcelFileToDataTable()
        {
            DataTable inLot = new DataTable();
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("MTOM_PRODID", typeof(string));
            return inLot;
        }

        private void setgrListCreateExcel_DataSet(DataTable dtLOTList)
        {
            
            try
            {
                if (dtLOTList == null || dtLOTList.Rows.Count <= 0)
                    return;

                DataSet inDataSet = getExcelFileShechIndataSet();
                DataTable inTable = inDataSet.Tables["INDATA"];

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPSTAT"] = sWipstat;
                inTable.Rows.Add(dr);

                DataTable inLot = inDataSet.Tables["INLOT"];
                DataRow newRow = null;

                for (int rowInx = 0; rowInx < dtLOTList.Rows.Count ; rowInx++)
                {
                    //Util.MessageValidation("SFU4979"); //필수입력항목을 모두 입력하십시오.
                    if (string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["LOTID"].ToString()) ||
                       string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["MTOM_PRODID"].ToString()) )
                    {
                        Util.MessageValidation("SFU4979"); //필수입력항목을 모두 입력하십시오.
                        return;
                    }

                    newRow = inLot.NewRow();

                    newRow["LOTID"] = dtLOTList.Rows[rowInx]["LOTID"].ToString();
                    newRow["MTOM_PRODID"] = dtLOTList.Rows[rowInx]["MTOM_PRODID"].ToString();

                    inLot.Rows.Add(newRow);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_STOCK_INV", "INDATA,INLOT", "OUTDATA", inDataSet);
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    Util.GridSetData(dgListCreateExcel, dsResult.Tables["OUTDATA"], FrameOperation);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnValidationExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtINLOT = getExclList();
                setgrListCreateExcel_DataSet(dtINLOT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// 엑셀 파일 그리드 데이터 셋팅
        /// </summary>
        /// <returns></returns>
        private DataTable getExclList()
        {
            try
            {
                if (dgListCreateExcel.GetRowCount() == 0)
                return null;

                DataTable dtRtn = getExcelFileToDataTable();
                DataRow row = null;

                for (int i = 0; i < dgListCreateExcel.GetRowCount(); i++)
                {
                    row = dtRtn.NewRow();
                    row["LOTID"] = Util.NVC(dgListCreateExcel.GetCell(i, dgListCreateExcel.Columns["LOTID"].Index).Value);
                    row["MTOM_PRODID"] = Util.NVC(dgListCreateExcel.GetCell(i, dgListCreateExcel.Columns["CHGPRODID"].Index).Value);
                    dtRtn.Rows.Add(row);
                }
                return dtRtn;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private Boolean getCreateMsgChk()
        {
            Boolean bRtn = true;

            for (int i = 0; i < dgListCreateExcel.GetRowCount(); i++)
            {

                if(Util.NVC(dgListCreateExcel.GetCell(i, dgListCreateExcel.Columns["ERROR_CODE"].Index).Value).Length > 0 )
                {
                    bRtn = false;
                    break;
                }
            }

            return bRtn;
        }


        private void dgLotChoiceExcel_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteExcel_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    DataTable dtInfo = DataTableConverter.Convert(dgListCreateExcel.ItemsSource);
                    dtInfo.Rows.RemoveAt(index);
                    Util.GridSetData(dgListCreateExcel, dtInfo, FrameOperation);
                }
            });
        }


        private void btnFileExcel_Click(object sender, RoutedEventArgs e)
        {
            attachFile(txtFilePathExcel);
        }

        private void btnSaveExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 변경 하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveExcel();
                }
            });
        }

        private void SaveExcel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);

                string sBizName = "BR_ACT_REG_CREATE_MTOM_MULTI";

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("CALDATE", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));
                inTable.Columns.Add("ATTCH_FILE_NAME", typeof(string));
                inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
                inTable.Columns.Add("OFFGRADE_FLAG", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
                row["CALDATE"] = dtpDateExcel.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                row["REQ_USERID"] = txtUserNameCrExcel.Tag.ToString();
                row["USERID"] = LoginInfo.USERID;
                row["NOTE"] = txtWipNoteCrExcel.Text;
                if (txtFilePathExcel.Text.ToString().Length > 0)
                {
                    row["ATTCH_FILE_NAME"] = System.IO.Path.GetFileName(Util.GetCondition(txtFilePathExcel));
                    row["ATTCH_FILE_CNTT"] = File.ReadAllBytes(Util.GetCondition(txtFilePathExcel));
                }
                row["ERP_TRNF_FLAG"] = chkErpSendYnExcel.IsChecked != null && (bool)chkErpSendYnExcel.IsChecked ? "T" : "N";
                row["OFFGRADE_FLAG"] = chkOffGrade2.IsChecked != null && (bool)chkOffGrade2.IsChecked ? "Y" : "N";

                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("PRODID", typeof(string));

                row = null;

                for (int i = 0; i < dgListCreateExcel.GetRowCount(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(dgListCreateExcel.GetCell(i, dgListCreateExcel.Columns["LOTID"].Index).Value);
                    row["PRODID"] = Util.NVC(dgListCreateExcel.GetCell(i, dgListCreateExcel.Columns["CHGPRODID"].Index).Value);
                    inLot.Rows.Add(row);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", dgListCreateExcel.GetRowCount());
                
                btnClear_Click(null, null);
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

        /// <summary>
        /// Tray 엑셀파일 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoadTray_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "MTOM_Tray_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "LOTID";
                    sheet[1, 0].Value = "LOTID";

                    sheet[0, 1].Value = "PRE_PRODID";
                    sheet[1, 1].Value = "변경전제품코드";

                    sheet[0, 2].Value = "PRODID";
                    sheet[1, 2].Value = "변경후제품코드";

                    sheet[0, 3].Value = "WIPQTY";
                    sheet[1, 3].Value = "수량";

                    sheet[0, 4].Value = "AREAID";
                    sheet[1, 4].Value = "동 ID";

                    sheet[0, 5].Value = "EQSGID";
                    sheet[1, 5].Value = "라인 ID";

                    sheet[0, 6].Value = "SLOC_ID";
                    sheet[1, 6].Value = "창고ID";

                    sheet[0, 7].Value = "CSTID";
                    sheet[1, 7].Value = "Tray ID";

                    //참고 문구
                    sheet[1, 9].Value = "<-- 2라인 부터 입력";

                    sheet[0, 10].Value = "동 목록";
                    sheet[1, 10].Value = "AREAID";
                    sheet[1, 11].Value = "명칭";

                    sheet[2, 10].Value = "M1";
                    sheet[2, 11].Value = "오창 소형1동";

                    sheet[3, 10].Value = "M2";
                    sheet[3, 11].Value = "오창 소형2동";

                    sheet[0, 13].Value = "라인 목록";
                    sheet[1, 13].Value = "LINEID";
                    sheet[1, 14].Value = "명칭";
                    sheet[1, 15].Value = "창고ID(추가/변경 될 수 있슴)";

                    sheet[2, 13].Value = "M1CF1";
                    sheet[2, 14].Value = "오창 1동 원형 활성화 라인";
                    sheet[2, 15].Value = "310C";

                    sheet[3, 13].Value = "M1CF2";
                    sheet[3, 14].Value = "오창 1동 초소형 활성화 라인";
                    sheet[3, 15].Value = "310C";

                    sheet[4, 13].Value = "M2CF3";
                    sheet[4, 14].Value = "오창 2동 원형 활성화 라인";
                    sheet[4, 15].Value = "320C";

                    sheet[0, 0].Style = sheet[0, 1].Style = sheet[0, 2].Style = styel;
                    sheet.Columns[0].Width = sheet.Columns[1].Width = sheet.Columns[2].Width = 1500;

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOpenTray_Click(object sender, RoutedEventArgs e)
        {
            ExcelMng exl = new ExcelMng();

            try
            {

                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        // DataTable dtResult = exl.GetSheetData(str[0]);

                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        if (sheet.GetCell(0, 0).Text != "LOTID" ||
                            sheet.GetCell(0, 1).Text != "PRE_PRODID" ||
                            sheet.GetCell(0, 2).Text != "PRODID" ||
                            sheet.GetCell(0, 3).Text != "WIPQTY" ||
                            sheet.GetCell(0, 4).Text != "AREAID" ||
                            sheet.GetCell(0, 5).Text != "EQSGID" ||
                            sheet.GetCell(0, 6).Text != "SLOC_ID" ||
                            sheet.GetCell(0, 7).Text != "CSTID"
                            )
                        {
                            Util.MessageValidation("SFU4424");
                            return;
                        }


                        DataTable dtINLOT = getTrayFileToDataTable();
                        DataRow newRow = null;


                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (sheet.GetCell(rowInx, 0) == null || string.IsNullOrEmpty(sheet.GetCell(rowInx, 0).Text))
                                break;

                            newRow = dtINLOT.NewRow();

                            newRow["LOTID"] = sheet.GetCell(rowInx, 0).Text;
                            newRow["PRE_PRODID"] = sheet.GetCell(rowInx, 1).Text;
                            newRow["PRODID"] = sheet.GetCell(rowInx, 2).Text;
                            newRow["WIPQTY"] = sheet.GetCell(rowInx, 3).Text;
                            newRow["AREAID"] = sheet.GetCell(rowInx, 4).Text;
                            newRow["EQSGID"] = sheet.GetCell(rowInx, 5).Text;
                            newRow["SLOC_ID"] = sheet.GetCell(rowInx, 6).Text;
                            newRow["CSTID"] = sheet.GetCell(rowInx, 7).Text;

                            dtINLOT.Rows.Add(newRow);
                        }

                        setgrListCreateTray_DataSet(dtINLOT);
                    }
                }
            }
            catch (Exception ex)
            {
                if (exl != null)
                {
                    //이전 연결 해제
                    exl.Conn_Close();
                }
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        private void setgrListCreateTray_DataSet(DataTable dtLOTList)
        {

            try
            {
                if (dtLOTList == null || dtLOTList.Rows.Count <= 0)
                    return;

                DataSet inDataSet = getTrayFileShechIndataSet();
                DataTable inTable = inDataSet.Tables["INDATA"];

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                DataTable inLot = inDataSet.Tables["INLOT"];
                DataRow newRow = null;

                for (int rowInx = 0; rowInx < dtLOTList.Rows.Count; rowInx++)
                {
                    if (string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["LOTID"].ToString()) ||
                        string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["PRE_PRODID"].ToString()) ||
                        string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["PRODID"].ToString()) ||
                        string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["WIPQTY"].ToString()) ||
                        string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["AREAID"].ToString()) ||
                        string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["EQSGID"].ToString()) ||
                        string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["SLOC_ID"].ToString()) ||
                        string.IsNullOrEmpty(dtLOTList.Rows[rowInx]["CSTID"].ToString()) )
                    {
                        Util.MessageValidation("SFU4979"); //필수입력항목을 모두 입력하십시오.
                        return;
                    }
                    newRow = inLot.NewRow();

                    newRow["LOTID"] = dtLOTList.Rows[rowInx]["LOTID"].ToString();
                    newRow["PRE_PRODID"] = dtLOTList.Rows[rowInx]["PRE_PRODID"].ToString();
                    newRow["PRODID"] = dtLOTList.Rows[rowInx]["PRODID"].ToString();
                    newRow["WIPQTY"] = Util.NVC_Decimal(dtLOTList.Rows[rowInx]["WIPQTY"].ToString());
                    newRow["AREAID"] = dtLOTList.Rows[rowInx]["AREAID"].ToString();
                    newRow["EQSGID"] = dtLOTList.Rows[rowInx]["EQSGID"].ToString();
                    newRow["SLOC_ID"] = dtLOTList.Rows[rowInx]["SLOC_ID"].ToString();
                    newRow["CSTID"] = dtLOTList.Rows[rowInx]["CSTID"].ToString();


                    inLot.Rows.Add(newRow);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_MTOM_FCS_TRAY_MULTI", "INDATA,INLOT", "OUTDATA", inDataSet);
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    Util.GridSetData(dgListCreateTray, dsResult.Tables["OUTDATA"], FrameOperation);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 엑셀 목록 일괄 데이터 조회 데이터 셋
        /// </summary>
        /// <returns></returns>
        private DataSet getTrayFileShechIndataSet()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inTable = inData.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("PRE_PRODID", typeof(string));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("AREAID", typeof(string));
            inLot.Columns.Add("EQSGID", typeof(string));
            inLot.Columns.Add("SLOC_ID", typeof(string));
            inLot.Columns.Add("CSTID", typeof(string));

            return inData;
        }

        /// <summary>
        /// 데이터 테이블 셋
        /// </summary>
        /// <returns></returns>
        private DataTable getTrayFileToDataTable()
        {
            DataTable inLot = new DataTable();
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("PRE_PRODID", typeof(string));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("AREAID", typeof(string));
            inLot.Columns.Add("EQSGID", typeof(string));
            inLot.Columns.Add("SLOC_ID", typeof(string));
            inLot.Columns.Add("CSTID", typeof(string));
            return inLot;
        }

        private void btnValidationTray_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtINLOT = getTrayList();
                setgrListCreateTray_DataSet(dtINLOT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDeleteTray_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    DataTable dtInfo = DataTableConverter.Convert(dgListCreateTray.ItemsSource);
                    dtInfo.Rows.RemoveAt(index);
                    Util.GridSetData(dgListCreateTray, dtInfo, FrameOperation);
                }
            });
        }


        private void btnFileTray_Click(object sender, RoutedEventArgs e)
        {
            attachFile(txtFilePathTray);
        }

        private void btnSaveTray_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            // 변경 하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveTray();
                }
            });

        }

        private void dgLotChoiceTray_Checked(object sender, RoutedEventArgs e)
        {

        }

        private Boolean getCreateTrayMsgChk()
        {
            Boolean bRtn = true;

            for (int i = 0; i < dgListCreateTray.GetRowCount(); i++)
            {

                if (Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["ERROR_CODE"].Index).Value).Length > 0)
                {
                    bRtn = false;
                    break;
                }
            }

            return bRtn;
        }

        /// <summary>
        /// 엑셀 파일 그리드 데이터 셋팅
        /// </summary>
        /// <returns></returns>
        private DataTable getTrayList()
        {
            try
            {
                if (dgListCreateTray.GetRowCount() == 0)
                    return null;

                DataTable dtRtn = getTrayFileToDataTable();
                DataRow row = null;

                for (int i = 0; i < dgListCreateTray.GetRowCount(); i++)
                {
                    row = dtRtn.NewRow();
                    row["LOTID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["LOTID"].Index).Value);
                    row["PRE_PRODID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["PRE_PRODID"].Index).Value);
                    row["PRODID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["PRODID"].Index).Value);
                    string sWipqty = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["WIPQTY"].Index).Value);
                    row["WIPQTY"] = sWipqty == "" ? 0 : decimal.Parse(sWipqty);
                    row["AREAID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["AREAID"].Index).Value);
                    row["EQSGID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["EQSGID"].Index).Value);
                    row["SLOC_ID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["SLOC_ID"].Index).Value);
                    row["CSTID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["CSTID"].Index).Value);

                    dtRtn.Rows.Add(row);
                }
                return dtRtn;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SaveTray()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtInfo = DataTableConverter.Convert(dgListCreate.ItemsSource);

                string sBizName = "BR_PRD_REG_MTOM_FCS_TRAY";

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("CALDATE", typeof(string));
                inTable.Columns.Add("REQ_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REQ_RSN_NOTE", typeof(string));
                inTable.Columns.Add("ATTCH_FILE_CNTT", typeof(Byte[]));
                inTable.Columns.Add("ATTCH_FILE_NAME", typeof(string));
                //inTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));

                DataRow row = null;

                row = inTable.NewRow();
                row["CALDATE"] = dtpDateTray.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                row["REQ_USERID"] = txtUserNameCrTray.Tag.ToString();
                row["USERID"] = LoginInfo.USERID;
                row["REQ_RSN_NOTE"] = txtWipNoteCrTray.Text;
                if (txtFilePathTray.Text.ToString().Length > 0)
                {
                    row["ATTCH_FILE_NAME"] = System.IO.Path.GetFileName(Util.GetCondition(txtFilePathTray));
                    row["ATTCH_FILE_CNTT"] = File.ReadAllBytes(Util.GetCondition(txtFilePathTray));
                }
                //row["ERP_TRNF_FLAG"] = chkErpSendYnExcel.IsChecked != null && (bool)chkErpSendYnExcel.IsChecked ? "T" : "N";
                inTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("PRE_PRODID", typeof(string));
                inLot.Columns.Add("PRODID", typeof(string));
                inLot.Columns.Add("WIPQTY", typeof(decimal));
                inLot.Columns.Add("AREAID", typeof(string));
                inLot.Columns.Add("EQSGID", typeof(string));
                inLot.Columns.Add("SLOC_ID", typeof(string));
                inLot.Columns.Add("CSTID", typeof(string));

                row = null;

                for (int i = 0; i < dgListCreateTray.GetRowCount(); i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["LOTID"].Index).Value);
                    row["PRE_PRODID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["PRE_PRODID"].Index).Value);
                    row["PRODID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["PRODID"].Index).Value);
                    row["WIPQTY"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["WIPQTY"].Index).Value);
                    row["AREAID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["AREAID"].Index).Value);
                    row["EQSGID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["EQSGID"].Index).Value);
                    row["SLOC_ID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["SLOC_ID"].Index).Value);
                    row["CSTID"] = Util.NVC(dgListCreateTray.GetCell(i, dgListCreateTray.Columns["CSTID"].Index).Value);
                    inLot.Rows.Add(row);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INLOT", null, inData);

                //[%1] 개가 정상 처리 되었습니다.
                Util.MessageInfo("SFU2056", dgListCreateTray.GetRowCount());
                
                btnClear_Click(null, null);
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

        private void txtLotIDHistTray_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowAdd(dgListCreateTray);
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
             //   if (dg.ItemsSource == null || dg.Rows.Count < 0)
              //  {
                //    dt.Columns.Add("LANGID", typeof(string));

               //     return;
               // }
                
                //foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                //{
                //    dt.Columns.Add(Convert.ToString(col.Name));
                //}
                if (dg.ItemsSource != null)
                {
                    dt = DataTableConverter.Convert(dg.ItemsSource);
                }
                else
                {
                    dt.Columns.Add("LOTID", typeof(String));
                    dt.Columns.Add("PRODID", typeof(String));
                    dt.Columns.Add("PRODNAME", typeof(String));
                    dt.Columns.Add("MODLID", typeof(String));
                    dt.Columns.Add("UNIT_CODE", typeof(String));
                    dt.Columns.Add("PRE_PRODID", typeof(String));
                    dt.Columns.Add("PRE_PRODNAME", typeof(String));
                    dt.Columns.Add("PRE_MODLID", typeof(String));
                    dt.Columns.Add("PRE_UNIT_CODE", typeof(String));
                    dt.Columns.Add("WIPQTY", typeof(Decimal));
                    dt.Columns.Add("SHOPID", typeof(String));
                    dt.Columns.Add("AREAID", typeof(String));
                    dt.Columns.Add("AREANAME", typeof(String));
                    dt.Columns.Add("EQSGID", typeof(String));
                    dt.Columns.Add("EQSGNAME", typeof(String));
                    dt.Columns.Add("SLOC_ID", typeof(String));
                    dt.Columns.Add("CSTID", typeof(String));
                    dt.Columns.Add("ERROR_CODE", typeof(String));
                    dt.Columns.Add("ERROR_MSG", typeof(String));
                    dt.Columns.Add("CHK_RESULT", typeof(String));
                }

                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);
                dt.AcceptChanges();
                dg.ItemsSource = DataTableConverter.Convert(dt);

                // 스프레드 스크롤 하단으로 이동
                //dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgListCreateTray_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    
                    if (e.Cell.Column.Name.Equals("LOTID") 
                        || e.Cell.Column.Name.Equals("PRE_PRODID")
                        || e.Cell.Column.Name.Equals("PRODID")
                        || e.Cell.Column.Name.Equals("WIPQTY")
                        || e.Cell.Column.Name.Equals("AREAID")
                        || e.Cell.Column.Name.Equals("EQSGID")
                        || e.Cell.Column.Name.Equals("SLOC_ID")
                        || e.Cell.Column.Name.Equals("CSTID") )
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgListCreateExcel_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name.Equals("LOTID")
                        || e.Cell.Column.Name.Equals("CHGPRODID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnReSendTray_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string erp_trnf_seqno = string.Empty;
                string chk = string.Empty;
                int chk_cnt = 0;

                //DataTable DT = DataTableConverter.Convert(dgListDelete.ItemsSource);
                for (int i = 0; i < dgListDeleteTray.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgListDeleteTray.Rows[i].DataItem, "CHK").Equals("True"))
                    {
                        if (DataTableConverter.GetValue(dgListDeleteTray.Rows[i].DataItem, "ERP_ERR_CODE") == null || !DataTableConverter.GetValue(dgListDeleteTray.Rows[i].DataItem, "ERP_ERR_CODE").Equals("FAIL"))
                        {
                            DataTableConverter.SetValue(dgListDeleteTray.Rows[i].DataItem, "CHK", "False");
                            Util.AlertInfo("SFU4911"); //ERP I/F가 실패일 경우에만 재전송 가능합니다.
                            return;
                        }
                    }
                }

                // 전송 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3609"), null, "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {

                        DataTable inDataTable = new DataTable("INDATA");
                        inDataTable.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                        for (int nrow = 0; nrow < dgListDeleteTray.GetRowCount(); nrow++)
                        {
                            chk = DataTableConverter.GetValue(dgListDeleteTray.Rows[nrow].DataItem, "CHK") as string;
                            erp_trnf_seqno = DataTableConverter.GetValue(dgListDeleteTray.Rows[nrow].DataItem, "ERP_TRNF_SEQNO") as string;

                            if (chk == "True")
                            {
                                DataRow inDataRow = inDataTable.NewRow();
                                inDataRow["ERP_TRNF_SEQNO"] = erp_trnf_seqno;

                                inDataTable.Rows.Add(inDataRow);

                                chk_cnt++;
                            }
                        }

                        if (chk_cnt > 0)
                        {
                            try
                            {
                                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_MTOM_FCS_TRAY_RESEND_TRSF_POST", "INDATA", null, inDataTable);

                                Util.MessageInfo("SFU1880"); // 전송 완료 되었습니다.

                                GetHistListTray();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }
                        else
                        {
                            // 전송 완료 되었습니다.
                            Util.MessageInfo("SFU1636"); // 선택된 대상이 없습니다.
                        }
                    }
                }, false, false, string.Empty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList2();
            }
        }

        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            //{
            //    try
            //    {
            //        ShowLoadingIndicator();

            //        string[] stringSeparators = new string[] { "\r\n" };
            //        string sPasteString = Clipboard.GetText();
            //        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

            //        if (sPasteStrings.Count() > 100)
            //        {
            //            Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
            //            return;
            //        }

            //        for (int i = 0; i < sPasteStrings.Length; i++)
            //        {
            //            if (!string.IsNullOrEmpty(sPasteStrings[i]) && Multi_Process2(sPasteStrings[i]) == false)
            //                break;

            //            System.Windows.Forms.Application.DoEvents();
            //        }

            //        if (sEmpty_Lot != "")
            //        {
            //            Util.MessageValidation("SFU3588", sEmpty_Lot);  // 입력한 LOTID[% 1] 정보가 없습니다.
            //            sEmpty_Lot = "";
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        Util.MessageException(ex);
            //        return;
            //    }
            //    finally
            //    {
            //        HiddenLoadingIndicator();
            //    }

            //    e.Handled = true;
            //}
        }

        private void txtCtnrID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList2();
            }
        }

        private void btnSearchCr2_Click(object sender, RoutedEventArgs e)
        {
            GetLotList2();
        }

        private void txtUserNameCr2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow2();
            }
        }

        private void btnUserCr2_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow2();
        }

        private void btnFile2_Click(object sender, RoutedEventArgs e)
        {
            attachFile2(txtFilePath2);
        }

        private void btnSave2_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave2())
                return;

            // 변경 하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save2();
                }
            });
        }

        private void btnClear2_Click(object sender, RoutedEventArgs e)
        {
            setClear();
        }


        private void btnCancelOffGrade1_Click(object sender, RoutedEventArgs e)
        {

            COM001_099_CANCEL_OFFGRADE popup = new COM001_099_CANCEL_OFFGRADE { FrameOperation = FrameOperation };

            popup.FrameOperation = this.FrameOperation;

            object[] parameters = new object[1];

            parameters[0] = "N";

            C1WindowExtension.SetParameters(popup, parameters);

            grdMain.Children.Add(popup);
            popup.BringToFront();

        }

        private void btnCancelOffGrade2_Click(object sender, RoutedEventArgs e)
        {

            COM001_099_CANCEL_OFFGRADE popup = new COM001_099_CANCEL_OFFGRADE { FrameOperation = FrameOperation };

            popup.FrameOperation = this.FrameOperation;

            object[] parameters = new object[1];

            parameters[0] = "N";

            C1WindowExtension.SetParameters(popup, parameters);

            grdMain.Children.Add(popup);
            popup.BringToFront();

        }


        private void btnCancelOffGrade3_Click(object sender, RoutedEventArgs e)
        {

            COM001_099_CANCEL_OFFGRADE popup = new COM001_099_CANCEL_OFFGRADE { FrameOperation = FrameOperation };

            popup.FrameOperation = this.FrameOperation;

            object[] parameters = new object[1];

            parameters[0] = "Y";

            C1WindowExtension.SetParameters(popup, parameters);

            grdMain.Children.Add(popup);
            popup.BringToFront();
        }
    }
}
