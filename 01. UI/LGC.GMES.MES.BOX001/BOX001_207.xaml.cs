/*************************************************************************************
 Created Date : 2017.11.21
      Creator : 이영준
   Decription : oWMS 출하검사 데이터 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.21  이영준 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_100.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_207 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _util = new Util();
        DataTable _dtParam = new DataTable();
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_207()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboBoxType, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "BOXTYPE_NJ" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {

        }


        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            InitDataTable();
        }

        private void InitDataTable()
        {
            _dtParam.Columns.Add("BOXID");
            _dtParam.Columns.Add("OQC_INSP_REQ_ID");
        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }


        private void dgReturn_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                //if (e.Cell.Row.Type == DataGridRowType.Item)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipping.Tag))
                //    {
                //        e.Cell.Presenter.Background = lblShipping.Background;
                //    }
                //    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblFinish.Tag))
                //    {
                //        e.Cell.Presenter.Background = lblFinish.Background;
                //    }
                //    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblCancel.Tag))
                //    {
                //        e.Cell.Presenter.Background = lblCancel.Background;
                //    }
                //    else
                //    {
                //        e.Cell.Presenter.Background = null;
                //    }
                //}
            }));
        }

        private void dgReturnResultChoice_Checked(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region [Button]

        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_OQC_REQ_PALLET_LIST_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getPalletList();
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        private void getPalletList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("FROM_DTTM");
                dt.Columns.Add("TO_DTTM");
                dt.Columns.Add("BOXTYPE");
                dt.Columns.Add("BOXID");
                dt.Columns.Add("OQC_INSP_REQ_ID");
                dt.Columns.Add("LANGID");
                DataRow dr = dt.NewRow();
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                dr["BOXTYPE"] = string.IsNullOrWhiteSpace(Util.NVC(cboBoxType.SelectedValue)) ? null : cboBoxType.SelectedValue;
                dr["BOXID"] = string.IsNullOrWhiteSpace(txtPalletID.Text) ? null : txtPalletID.Text;
                dr["OQC_INSP_REQ_ID"] = string.IsNullOrWhiteSpace(txtReqID.Text) ? null : txtReqID.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);
                new ClientProxy().ExecuteService("BR_PRD_GET_OQC_REQ_PALLET_LIST_NJ", "INDATA", "OUTDATA", dt, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (!result.Columns.Contains("CHK"))
                    {
                        result = _util.gridCheckColumnAdd(result, "CHK");
                    }
                    Util.GridSetData(dgOQCList, result, FrameOperation);

                    if (dgOQCList.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgOQCList.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

        #endregion

        #region [PopUp Event]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = "";
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = "";
                Parameters[3] = Process.GRADING;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER wndPopup = sender as CMM_SHIFT_USER;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
            this.grdMain.Children.Remove(wndPopup);
        }

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #endregion

        private void dgOQCListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgOQCList.ItemsSource)
                    {
                        if (drv["OQC_INSP_REQ_ID"].ToString().Equals(item["OQC_INSP_REQ_ID"].ToString()))
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

        private void dgOQCListChoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkGroupSelect.IsChecked == true)
                {
                    CheckBox cb = sender as CheckBox;
                    if (cb?.DataContext == null) return;
                    if (cb.IsChecked == null) return;

                    DataRowView drv = cb.DataContext as DataRowView;

                    foreach (DataRowView item in dgOQCList.ItemsSource)
                    {
                        if (drv["OQC_INSP_REQ_ID"].ToString().Equals(item["OQC_INSP_REQ_ID"].ToString()))
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

        private void btnCellList_Click(object sender, RoutedEventArgs e)
        {
            List<int> chkList = _util.GetDataGridCheckRowIndex(dgOQCList, "CHK");
            string sParam = string.Empty;
            foreach (int chk in chkList)
            {
               sParam += dgOQCList.GetCell(chk, dgOQCList.Columns["BOXID"].Index).Value + ",";
            }
           // sParam = sParam.Substring(0, sParam.Length - 2);

            //object[] param = new object[1];
            //for (int i = 0; i < param.Length; i++)
            //{
            //    param[i] = dgOQCList.GetCell(chkList[i], dgOQCList.Columns["BOXID"].Index).Value;
            //}
            this.FrameOperation.OpenMenu("SFU010160570", true, new object[] {sParam });

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            List<int> chkList = _util.GetDataGridCheckRowIndex(dgOQCList, "CHK");

            if(chkList.Count<1)
            {
                Util.MessageValidation("SFU1651");
                return;
            }
            DataRow dr = null;
            _dtParam.Clear();
            foreach (int chk in chkList)
            {
                dr = _dtParam.NewRow();
                dr["BOXID"] = dgOQCList.GetCell(chk, dgOQCList.Columns["BOXID"].Index).Value.ToString();
                dr["OQC_INSP_REQ_ID"] = dgOQCList.GetCell(chk, dgOQCList.Columns["OQC_INSP_REQ_ID"].Index).Value.ToString();
                _dtParam.Rows.Add(dr);
            }
            var query = (from t in _dtParam.AsEnumerable()
                         where t.Field<string>("OQC_INSP_REQ_ID") != _dtParam.Rows[0]["OQC_INSP_REQ_ID"].ToString()
                         select t).Distinct();

            if(query.Any())
            {
                Util.MessageValidation("SFU4433");
                return;
            }

            BOX001_206_PLT_LABEL popup = new BOX001_206_PLT_LABEL();
            popup.FrameOperation = this.FrameOperation;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = _dtParam;
                Parameters[1] = txtWorker.Tag;

                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(printPopup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void printPopup_Closed(object sender, EventArgs e)
        {

        }

    }
}
