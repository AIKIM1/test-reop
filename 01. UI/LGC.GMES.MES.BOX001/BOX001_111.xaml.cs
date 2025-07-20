/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_111 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        public BOX001_111()
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

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
            
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
        
        private void btnShift_Main_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = this.FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                //Parameters[0] = LoginInfo.CFG_SHOP_ID;
                //Parameters[1] = LoginInfo.CFG_AREA_ID;
                //Parameters[2] = Util.NVC(cboLine.SelectedValue);
                //Parameters[3] = _PROCID;
                //Parameters[4] = Util.NVC(txtShift_Main.Tag);
                //Parameters[5] = Util.NVC(txtWorker_Main.Tag);
                //Parameters[6] = Util.NVC(cboEquipment_Search.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Main_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void wndShift_Main_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift_Main.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift_Main.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker_Main.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker_Main.Tag = Util.NVC(wndPopup.USERID);              
            }
            this.grdMain.Children.Remove(wndPopup);
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : DA_PRD_SEL_INPALLET_FM
        /// </summary>
        private void Search()
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("PROCID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("EQPTID", typeof(string));
                //RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                //RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                //RQSTDT.Columns.Add("PKG_LOTID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["PROCID"] = _PROCID;

                //if (!string.IsNullOrWhiteSpace(txtLotID_Search.Text))
                //{
                //    dr["PKG_LOTID"] = txtLotID_Search.Text;
                //}
                //else
                //{
                //    dr["EQSGID"] = (string)cboLine.SelectedValue;
                //    dr["EQPTID"] = (string)cboEquipment_Search.SelectedValue;
                //    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                //    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                //}
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPALLET_INFO_FM", "RQSTDT", "RSLTDT", RQSTDT);
                //if(!dtResult.Columns.Contains("CHK"))
                //    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                //Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);

                //if (dgSearchResult.Rows.Count > 0)
                //{
                //    DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                //    DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                //    DataGridAggregate.SetAggregateFunctions(dgSearchResult.Columns["DEFECT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                //}

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


        private void dgSearchResult_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void btnScrap_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRework_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDefect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgSearchResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
