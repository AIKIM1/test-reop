/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 이슬아
   Decription : 포장출고
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.06  이슬아 : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_105 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        public BOX001_105()
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
            // DA_BAS_SEL_AREA_ALL_CBO
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "PACK_RCV_ISS_STAT_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
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

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }


        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtWorker.Tag = txtWorker.Text = string.Empty;
        }

        private void dgSearhResult_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblReady.Tag))
                    {
                        e.Cell.Presenter.Background = lblReady.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipping.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipped.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipped.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblCancel.Tag))
                    {
                        e.Cell.Presenter.Background = lblCancel.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgSearhResultChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgSearhResult.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_INPALLET_FOR_SHIP_FM
        /// </summary>
        private void Search()
        {
            try
            {
                if (Util.NVC(cboArea.SelectedValue) == string.Empty || Util.NVC(cboArea.SelectedValue) == "SELECT")
                {
                    // SFU1499 동을 선택해주십시오.
                    Util.MessageValidation("SFU1499");
                    return;
                }

                string[] sArry = Util.NVC(cboArea.SelectedValue).Split('^');

                string sAreaID = sArry[0];
                string sShopID = sArry[1];

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    dr["PKG_LOTID"] = txtPalletID.Text;
                }
                else if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    dr["BOXID"] = txtLotID.Text;
                }
                else
                {
                    dr["AREAID"] = sAreaID;
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["RCV_ISS_STAT_CODE"] = string.IsNullOrWhiteSpace(Util.NVC(cboStat.SelectedValue)) ? null : cboStat.SelectedValue;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PACKING_HISTORY_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgSearhResult, dtResult, FrameOperation, true);
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

        #region [PopUp Event]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {

            if (Util.NVC(cboArea.SelectedValue) == string.Empty || Util.NVC(cboArea.SelectedValue) == "SELECT")
            {
                // SFU1499 동을 선택해주십시오.
                Util.MessageValidation("SFU1499");
                return;
            }

            string[] sArry = Util.NVC(cboArea.SelectedValue).Split('^');

            string sAreaID = sArry[0];
            string sShopID = sArry[1];

            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = sShopID;
                Parameters[1] = sAreaID;
                Parameters[2] = "";
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShift_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion      
    }
}
