/*************************************************************************************
 Created Date : 2017.11.20
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리
--------------------------------------------------------------------------------------
 [Change History]


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
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_245.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_245 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();

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
        public BOX001_245()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
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

            //if (LoginInfo.CFG_SHOP_ID.Equals("A010"))// ?? 남경 소형은???
            //{
            btnSublotHold.Visibility = Visibility.Collapsed;
            //}
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");

            string[] sFilter = { "HOLD_YN" };
            _combo.SetCombo(cboHoldYN2, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

            string[] sFilter1 = { "HOLD_TRGT_CODE" };
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType.SelectedIndex = 1;

            _combo.SetCombo(cboLotType2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE_WITHOUT_CODE");
            cboLotType2.SelectedIndex = 1;
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
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
        }

        //private void btnShift_Main_Click(object sender, RoutedEventArgs e)
        //{
        //    CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
        //    wndPopup.FrameOperation = this.FrameOperation;

        //    if (wndPopup != null)
        //    {
        //        object[] Parameters = new object[8];
        //        //Parameters[0] = LoginInfo.CFG_SHOP_ID;
        //        //Parameters[1] = LoginInfo.CFG_AREA_ID;
        //        //Parameters[2] = Util.NVC(cboLine.SelectedValue);
        //        //Parameters[3] = _PROCID;
        //        //Parameters[4] = Util.NVC(txtShift_Main.Tag);
        //        //Parameters[5] = Util.NVC(txtWorker_Main.Tag);
        //        //Parameters[6] = Util.NVC(cboEquipment_Search.SelectedValue);
        //        Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

        //        C1WindowExtension.SetParameters(wndPopup, Parameters);

        //        wndPopup.Closed += new EventHandler(wndShift_Main_Closed);

        //        // 팝업 화면 숨겨지는 문제 수정.
        //        //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
        //        grdMain.Children.Add(wndPopup);
        //        wndPopup.BringToFront();
        //    }
        //}

        //private void wndShift_Main_Closed(object sender, EventArgs e)
        //{
        //    CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

        //    if (wndPopup.DialogResult == MessageBoxResult.OK)
        //    {
        //        txtShift_Main.Text = Util.NVC(wndPopup.SHIFTNAME);
        //        txtShift_Main.Tag = Util.NVC(wndPopup.SHIFTCODE);
        //        txtWorker_Main.Text = Util.NVC(wndPopup.USERNAME);
        //        txtWorker_Main.Tag = Util.NVC(wndPopup.USERID);              
        //    }
        //    this.grdMain.Children.Remove(wndPopup);
        //}
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region Hold 등록 팝업
        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            registeHold("SUBLOT");
        }

        private void puHold_Closed(object sender, EventArgs e)
        {
            BOX001_245_HOLD window = sender as BOX001_245_HOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region Hold 해제 팝업
        private void btnHoldRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                if (dtInfo.Rows.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                if (drList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                BOX001_245_UNHOLD puHold = new BOX001_245_UNHOLD();
                puHold.FrameOperation = FrameOperation;

                object[] Parameters = new object[3];
                Parameters[0] = drList.CopyToDataTable();
                C1WindowExtension.SetParameters(puHold, Parameters);

                puHold.Closed += new EventHandler(puUnHold_Closed);

                grdMain.Children.Add(puHold);
                puHold.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void puUnHold_Closed(object sender, EventArgs e)
        {
            BOX001_245_UNHOLD window = sender as BOX001_245_UNHOLD;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion


        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_ASSY_HOLD_HIST     
        /// HOLD_FLAG = ‘Y’ , HOLD_TYPE_CODE = ‘SHIP_HOLD’ 고정
        /// </summary>
        private void Search()
        {
            try
            {
                //SERACH_GUBUN 통해서 특별관리 HOLD의 경우에는 GMES002만 조회되게하고 기존 비즈에서는 GMES002는 조회되지 않게 바꿔놓기
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("SEARCH_GUBUN");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["HOLD_FLAG"] = "Y";
                dr["SEARCH_GUBUN"] = "A";
                if (!string.IsNullOrEmpty(txtLotID.Text))
                {
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID.Text) ? null : txtLotID.Text;
                }
                else
                {
                    dr["FROM_HOLD_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    dr["AREAID"] = (string)cboArea.SelectedValue;
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType.SelectedValue;
                }

                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID.Text) ? null : txtCellID.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("SEARCH_GUBUN");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SEARCH_GUBUN"] = "A";
                if (!string.IsNullOrEmpty(txtLotID2.Text))
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotID2.Text) ? null : txtLotID2.Text;
                else
                {

                    dr["FROM_HOLD_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    dr["AREAID"] = (string)cboArea2.SelectedValue;
                    dr["HOLD_TRGT_CODE"] = (string)cboLotType2.SelectedValue;
                    dr["HOLD_FLAG"] = (string)cboHoldYN2.SelectedValue == "" ? null : (string)cboHoldYN2.SelectedValue;
                }
                dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellID2.Text) ? null : txtCellID2.Text;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                Util.GridSetData(dgHist, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSearchResult.ItemsSource);

                if (dtInfo.Rows.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<DataRow> drList = dtInfo.Select("CHK = 'True'")?.ToList();

                if (drList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                BOX001_245_UPDATE puUpdate = new BOX001_245_UPDATE();
                puUpdate.FrameOperation = FrameOperation;

                if (puUpdate != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = drList.CopyToDataTable();
                    C1WindowExtension.SetParameters(puUpdate, Parameters);

                    puUpdate.Closed += new EventHandler(puUpdate_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puUpdate);
                    puUpdate.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void puUpdate_Closed(object sender, EventArgs e)
        {
            BOX001_245_UPDATE window = sender as BOX001_245_UPDATE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(window);
        }

        #region  체크박스 선택 이벤트     
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearchResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearchResult.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        private void dgSearchResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
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

        private void dgSearchResult_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSearchResult_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgSearchResult.ItemsSource)
                    {
                        if (drv["HOLD_GR_ID"].ToString().Equals(item["HOLD_GR_ID"].ToString()))
                        {
                            item["CHK"] = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLotHold_Click(object sender, RoutedEventArgs e)
        {
            registeHold("LOT");
        }

        private void registeHold(string holdTrgtCode)
        {
            try
            {
                BOX001_245_HOLD puHold = new BOX001_245_HOLD();
                puHold.FrameOperation = FrameOperation;

                if (puHold != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = holdTrgtCode;
                    //Parameters[1] = cboEquipment.SelectedValue.ToString();
                    //Parameters[2] = cboElecType.SelectedValue.ToString();
                    C1WindowExtension.SetParameters(puHold, Parameters);

                    puHold.Closed += new EventHandler(puHold_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puHold);
                    puHold.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
