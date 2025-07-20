/*************************************************************************************
 Created Date : 2024.09.04
      Creator : 
   Decription : 알람 팝업 이력
--------------------------------------------------------------------------------------
 [Change History]
  2024.09.04  DEVELOPER : Initial Created.
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using C1.WPF.Excel;
using System.Configuration;
using Microsoft.Win32;
using System.IO;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_229 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
        private string RACKID = string.Empty;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_229()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            dtpFromDate.SelectedDateTime = DateTime.Today.AddDays(-6);
            dtpToDate.SelectedDateTime = DateTime.Today.AddDays(1);

            dtpFromDateTmpr.SelectedDateTime = DateTime.Today.AddDays(-6);
            dtpToDateTmpr.SelectedDateTime = DateTime.Today.AddDays(1);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region Method

        #region [FDS tab]
        private void GetList()
        {
            try
            {
                //DA_SEL_FDS_ALARM_HIST_UI_MB
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FDS_ALARM_HIST_UI_MB", "RQSTDT", "RSLTDT", dtRqst);
                
                Util.GridSetData(dgFDS, dtRslt, FrameOperation,true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [SAS tab]
        private void GetListSAS()
        {
            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SAS_ALARM_CURR_UI_MB", null, "RSLTDT", null);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_SAS_ALARM_HIST_UI_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgSAScurr, dtRslt, FrameOperation, true);
                Util.GridSetData(dgSAShist, dtRslt2, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [고온 Aging 온도이탈 tab]
        private void GetListTmprCurr()
        {
            try
            {
                RACKID = string.Empty;
                Util.gridClear(dgTmprcurr);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = "4";
                dr["FROMDATE"] = dtpFromDateTmpr.SelectedDateTime.ToString("yyyyMMdd") + "000000";
                dr["TODATE"] = dtpToDateTmpr.SelectedDateTime.ToString("yyyyMMdd") + "235959";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_HIGH_AGING_ABNORM_TMPR_ALARM_CURR_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgTmprcurr, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetListTmprHist()
        {
            try
            {
                RACKID = string.Empty;
                Util.gridClear(dgTmprhist);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("S70", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["S70"] = "4";
                dr["FROMDATE"] = dtpFromDateTmpr.SelectedDateTime.ToString("yyyyMMdd") + "000000";
                dr["TODATE"] = dtpToDateTmpr.SelectedDateTime.ToString("yyyyMMdd") + "235959";
                dtRqst.Rows.Add(dr);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_SEL_HIGH_AGING_ABNORM_TMPR_ALARM_HIST_MB", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgTmprhist, dtRslt2, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #endregion

        #region Event
        #region [FDS tab]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan DiffDate = dtpToDate.SelectedDateTime - dtpFromDate.SelectedDateTime;

            if (DiffDate.Days > 30)
            {
                //조회기간은 30일을 초과할수 없습니다
                Util.MessageValidation("SFU4466");
                return;
            }
            GetList();
        }

        private void dgFDS_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgFDS.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }
        #endregion

        #region [SAS tab]
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSAScurr);
            Util.gridClear(dgSAShist);

            GetListSAS();
        }

        private void btnREL_Click(object sender, RoutedEventArgs e)
        {
            if (!ChkConfirmUser())
                return;

            SetAlarmRel();
        }

        // 해제 가능 작업자 여부 확인
        private bool ChkConfirmUser()
        {


            string InputID = txtUserID.Text;

            if (string.IsNullOrEmpty(InputID))
            {
                Util.MessageValidation("SFU1155");  // 사용자 ID를 입력해주세요.
                return false;
            }


            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("COM_CODE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("INPUTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
                dr["COM_CODE"] = "SAS_ML_FITTED_CAPA";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["INPUTID"] = InputID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SAS_ALARM_CONFIRM_USER_MB", "RQSTDT", "RSLTDT", dtRqst);


                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0593");  // 알람 해제 가능 사용자가 아닙니다.
                    return false;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return true;

        }

        private void SetAlarmRel()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("ROUTID", typeof(string));
            dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));
            dtRqst.Columns.Add("MES_CALC_CNT", typeof(string));
            dtRqst.Columns.Add("TOTL_CNT", typeof(string));
            dtRqst.Columns.Add("ALARM_PCT", typeof(string));
            dtRqst.Columns.Add("MMD_SET_PCT", typeof(string));
            dtRqst.Columns.Add("RELDTTM", typeof(string));
            dtRqst.Columns.Add("REL_FLAG", typeof(string));
            dtRqst.Columns.Add("REL_USER", typeof(string));
            dtRqst.Columns.Add("UPDUSER", typeof(string));
            dtRqst.Columns.Add("UPDDTTM", typeof(string));

            string RELDTTM = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            for (int i = 0; i < dgSAScurr.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    DataRow dr = dtRqst.NewRow();
                    dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "ROUTID"));
                    dr["DAY_GR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "DAY_GR_LOTID"));
                    dr["MES_CALC_CNT"] = Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "MES_CALC_CNT"));
                    dr["TOTL_CNT"] = Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "TOTL_CNT"));
                    dr["ALARM_PCT"] = Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "ALARM_PCT"));
                    dr["MMD_SET_PCT"] = Util.NVC(DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "MMD_SET_PCT"));
                    dr["RELDTTM"] = RELDTTM;
                    dr["REL_FLAG"] = "Y";
                    dr["REL_USER"] = txtUserID.Text;
                    dr["UPDUSER"] = txtUserID.Text;
                    dr["UPDDTTM"] = RELDTTM;
                    dtRqst.Rows.Add(dr);
                    // sTrayList += DataTableConverter.GetValue(dgSAScurr.Rows[i].DataItem, "CSTID") + "|";
                }
            }

            if (dtRqst.Rows.Count == 0)
            {

                Util.MessageValidation("FM_ME_0165");  // 선택된 데이터가 없습니다
                return;

            }

            //저장하시겠습니까?
            Util.MessageConfirm("FM_ME_0214", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    string InputID = txtUserID.Text;

                    if (string.IsNullOrEmpty(InputID))
                        return;


                    try
                    {
                        new ClientProxy().ExecuteService("DA_UPD_TB_SFC_FORM_SAS_ALARM_HIST_MB", "RQSTDT", "RSLTDT", dtRqst, (dtRslt, Exception) =>
                        {
                            if (Exception != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            Util.MessageValidation("FM_ME_0215");
                        });


                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }

                    finally
                    {
                        Util.gridClear(dgSAScurr);
                        Util.gridClear(dgSAShist);

                        GetListSAS();
                    }
                }
            });

        }
        #endregion

        #region [고온 Aging 온도이탈 tab]
        private void btnSearchTmpr_Click(object sender, RoutedEventArgs e)
        {
            GetListTmprCurr();
            GetListTmprHist();
        }

        private void dgTmprcurr_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                ///////////////////////////////////////////////////////////////////////////////////

                if (!string.IsNullOrEmpty(e.Cell.Column.Name))
                {
                    if (e.Cell.Column.Name.Equals("RACKID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgTmprhist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;

                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                ///////////////////////////////////////////////////////////////////////////////////

                string sRackID = Util.NVC(DataTableConverter.GetValue(dgTmprhist.Rows[e.Cell.Row.Index].DataItem, "RACKID"));

                if (!string.IsNullOrEmpty(RACKID) && RACKID.Equals(sRackID))
                {
                    e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                }
            }));
        }

        private void dgTmprcurr_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgTmprhist_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgTmprcurr_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgTmprcurr.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Row.Index < 2 || cell.Column.Index < 1)
                {
                    return;
                }

                if (!cell.Column.Name.Equals("RACKID"))
                {
                    return;
                }

                string sRackID = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "RACKID"));
                string sFindText = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "FIND_TEXT"));
                SearchRackHist(sRackID, sFindText);
            }
        }

        private void dgTmprcurr_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTmprcurr.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "ACTION_CNTT")
                    {
                        FCS002_229_ACTSAVE wndPopup = new FCS002_229_ACTSAVE();
                        wndPopup.FrameOperation = FrameOperation;
                        if (wndPopup != null)
                        {
                            int rowidx = cell.Row.Index;

                            object[] Parameters = new object[10];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "RACKID")); //EQP_ID
                            Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "ALARM_OCCUR_DTTM"));
                            Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "EQPTNAME"));
                            Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "ACTION_CNTT"));
                            Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "FLOOR_CODE"));
                            Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "EQPT_GR_TYPE_CODE"));
                            Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "EQPT_ROW_LOC"));
                            Parameters[7] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "EQPT_COL_LOC"));
                            Parameters[8] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "EQPT_STG_LOC"));
                            Parameters[9] = Util.NVC(DataTableConverter.GetValue(dgTmprcurr.Rows[cell.Row.Index].DataItem, "EQPT_ID"));

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopup_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            FCS002_229_ACTSAVE window = sender as FCS002_229_ACTSAVE;

            if (window.DialogResult == MessageBoxResult.Yes)
            {
                btnSearchTmpr_Click(null, null);
            }
            this.grdMain.Children.Remove(window);
        }

        private void SearchRackHist(string rackid, string find_text)
        {
            RACKID = string.Empty;
            if (!string.IsNullOrEmpty(rackid))
            {
                RACKID = rackid;
            }

            DataTable temp = DataTableConverter.Convert(dgTmprhist.ItemsSource);
            Util.GridSetData(dgTmprhist, temp, this.FrameOperation);

            int iRow = 0;
            for (int i = dgTmprhist.Rows.Count - 1 ; i > dgTmprhist.TopRows.Count - 1; i--)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgTmprhist.Rows[i].DataItem, "RACKID")).Equals(RACKID))
                {
                    iRow = i;
                    break;
                }
            }
            dgTmprhist.ScrollIntoView(iRow, 0);
        }
        #endregion

        #endregion

    }
}
