/******************************************************************************************************************
 Created Date : 2022.05.25
      Creator : 최도훈
   Decription : Crack 선감지 Tray 조회
------------------------------------------------------------------------------------------------------------------
 [Change History]
  2022.05.25  최도훈 : Initial Created
  2022.10.04  최도훈 : Crack_Cnt DA 추가
  2022.10.06  최도훈 : FCS001_137_CRACK_CNT_DETAIL 호출, 컬럼 색상 추가
  2023.09.28  이정미 : 상세 화면 조회 조건 수정 밒 디자인 변경
******************************************************************************************************************/

using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
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
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.Common.Mvvm;
using LGC.GMES.MES.CMM001.Extensions;



namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_137 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        public FCS001_137()
        {
            InitializeComponent();

            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion


        #region [Initialize]
        private void Initialize()
        {
            SetWorkResetTime();

            // 조회기간 초기화
            InitializeControls();

            // 특별관리여부, 특별관리코드 초기화
            InitializeCombo();


            // 생산라인 초기화
            GetLine(cboLine);

            // 모델 초기화
            GetModel(cboModel);
        }
        
        private void InitializeControls()
        {
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();

            dtpFromDate2.SelectedDateTime = GetJobDateFrom();
            dtpFromTime2.DateTime = GetJobDateFrom();
            dtpToDate2.SelectedDateTime = GetJobDateTo();
            dtpToTime2.DateTime = GetJobDateTo();
        }

        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();

            string[] sFilter1 = { "COMBO_FORM_SPCL_FLAG" };
            _combo.SetCombo(cboSpclFlag, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "SPCL_FLAG" };
            _combo.SetCombo(cboSpclCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            Initialize();
                        
            this.Loaded -= UserControl_Loaded;
        }
        #endregion


        #region [Method]
        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }              


        private void GetLine(C1ComboBox cbo)
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FORM";

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EXCEPT_GROUP"] = null;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp = result.Copy();

                        cbo.ItemsSource = AddStatus(dtTemp, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                        cbo.SelectedIndex = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetModel(C1ComboBox cbo)
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_FORM_MODEL_CBO";

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp = result.Copy();

                        cbo.ItemsSource = AddStatus(dtTemp, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();
                        cbo.SelectedIndex = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }
        

        private void GetList()
        {
            try
            {
                Util.gridClear(dgCrackLine);
                Util.gridClear(dgTrayLocHist);
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_TRAY_CNVBCR_TOTAL_CNT";

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("BCR_CNT", typeof(int));
                dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_DATE", typeof(DateTime));
                dtRqst.Columns.Add("SPCL_YN", typeof(string));
                dtRqst.Columns.Add("SPCL_CD", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["BCR_CNT"] = Convert.ToInt32(cboCount.Value);
                dr["FROM_DATE"] = DateTime.Parse(dtpFromDate.SelectedDateTime.ToString("yyyy/MM/dd") + ' ' + dtpFromTime.DateTime.Value.ToString("HH:mm:00"));
                dr["TO_DATE"] = DateTime.Parse(dtpToDate.SelectedDateTime.ToString("yyyy/MM/dd") + ' ' + dtpToTime.DateTime.Value.ToString("HH:mm:00"));
                dr["SPCL_YN"] = Util.GetCondition(cboSpclFlag, bAllNull: true);
                dr["SPCL_CD"] = Util.GetCondition(cboSpclCode, bAllNull: true);
                dr["CSTID"] = string.IsNullOrWhiteSpace(txtTrayId.Text.Trim()) ? null : txtTrayId.Text.Trim();
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        Util.GridSetData(dgCrackLine, result, FrameOperation, true);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                throw ex;
            }            
        }

        private void GetCrackList()
        {
            try
            {
                Util.gridClear(dgCrackCnt);
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_CRACK_CNT";

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_DATE", typeof(DateTime));
                dtRqst.Columns.Add("LINE_ID", typeof(string));
                dtRqst.Columns.Add("MODEL_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = DateTime.Parse(dtpFromDate2.SelectedDateTime.ToString("yyyy/MM/dd") + ' ' + dtpFromTime2.DateTime.Value.ToString("HH:mm:00"));
                dr["TO_DATE"] = DateTime.Parse(dtpToDate2.SelectedDateTime.ToString("yyyy/MM/dd") + ' ' + dtpToTime2.DateTime.Value.ToString("HH:mm:00"));
                dr["LINE_ID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MODEL_ID"] = Util.GetCondition(cboModel, bAllNull: true);

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        Util.GridSetData(dgCrackCnt, result, FrameOperation, true);
                    }
                });
                
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                throw ex;
            }
        }

        private void GetTrayLocHist()
        {
            try
            {
                Util.gridClear(dgTrayLocHist);
                ShowLoadingIndicator();
                
                // 추가 예정
                
                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                throw ex;
            }
        }

        #endregion

        #region [Event]

        #region [Crack 선감지 tray 조회]
        private void btnClSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtTrayId.Text)) && (e.Key == Key.Enter))
            {
                btnClSearch_Click(null, null);
            }
        }

        private void dgCrackLine_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // 검색기간 제한 사용보류
                //if ((dtpToDate.SelectedDateTime.Date - dtpFromDate.SelectedDateTime.Date).Days >= 7)
                //{
                //    Util.Alert("FM_ME_0231"); //조회기간은 7일을 초과할 수 없습니다.
                //    return;
                //}

                C1DataGrid dg = sender as C1DataGrid;
                if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCrackLine.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("TRAYID") || cell.Column.Name.Equals("TRAYNO"))
                    {
                        Util.gridClear(dgTrayLocHist);                        

                        int rowIdx = cell.Row.Index;
                        DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                        if (drv == null) return;


                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("LANGID", typeof(string));
                        dtRqst.Columns.Add("FROM_DATE", typeof(string));
                        dtRqst.Columns.Add("TO_DATE", typeof(string));
                        dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                        dtRqst.Columns.Add("CSTID", typeof(string));
                        dtRqst.Columns.Add("USE_FLAG", typeof(string));

                        DataRow dr = dtRqst.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["FROM_DATE"] = Convert.ToDateTime(DataTableConverter.GetValue(drv, "STRTDTTM")).ToString("yyyyMMddHHmmss");
                        dr["TO_DATE"] = Convert.ToDateTime(DataTableConverter.GetValue(drv, "ENDDTTM")).ToString("yyyyMMddHHmmss");
                        dr["CMCDTYPE"] = "CSTSTAT";
                        dr["CSTID"] = DataTableConverter.GetValue(drv, "TRAYID").GetString();
                        dr["USE_FLAG"] = "Y";
                        dtRqst.Rows.Add(dr);

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService("DA_SEL_CV_TRAY_POSITION_INFO", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                        {
                            try
                            {
                                if (Exception != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                if (result.Rows.Count > 0)
                                {
                                    Util.GridSetData(dgTrayLocHist, result, FrameOperation, true);

                                    dgTrayLocHist.Columns["NOTE"].VerticalAlignment = VerticalAlignment.Top;

                                    Util _Util = new Util();
                                    string[] sColumnName = new string[] { "CSTID" };
                                    _Util.SetDataGridMergeExtensionCol(dgTrayLocHist, sColumnName, DataGridMergeMode.VERTICAL);
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
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }        

        private void dgCrackLine_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //    try
            //    {
            //        C1DataGrid dg = sender as C1DataGrid;
            //        if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null) return;

            //        Point pnt = e.GetPosition(null);
            //        C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

            //        if (cell == null) return;

            //        // 선택한 셀의 위치
            //        int rowIdx = cell.Row.Index;
            //        DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
            //        if (drv == null) return;

            //        if (cell.Column.Name.Equals("TRAYID") || cell.Column.Name.Equals("TRAYNO"))
            //        {
            //            GetTrayLocHist();
            //        }
            //        else
            //        {
            //            return;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        HiddenLoadingIndicator();
            //        Util.MessageException(ex);
            //    }
        }

        private void dgCrackLine_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;
                
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.Equals("TRAYID") || e.Cell.Column.Name.Equals("TRAYNO"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));

        }

        private void btnExpend_Click(object sender, RoutedEventArgs e)
        {
            if (btnExpend.Content.Equals("↗"))
            {
                btnExpend.Content = "↙";

                Row0.Height = new GridLength(0);
                Row1.Height = new GridLength(0);
            }
            else
            {
                btnExpend.Content = "↗";

                Row0.Height = new GridLength(2, GridUnitType.Star);
                Row1.Height = new GridLength(8);
                Row2.Height = new GridLength(2, GridUnitType.Star);
                Row3.Height = new GridLength(8);
            }
        }
        #endregion

        #region [Crack  count 조회]
        private void dgCrackCnt_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "LIMIT_OVER")
                    {
                        if(e.Cell.Text != "0")
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                    }
                    else if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString().Contains("TWO_TO_THREE", "THREE_TO_FIVE"))
                    {
                        if (e.Cell.Text != "0")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                        }
                    }
                    else if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString().Contains("FIVE_TO_TEN", "TEN_OVER"))
                    {
                        if (e.Cell.Text != "0")
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }
                }
            }));
        }


        private void btnCcSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCrackList();
        }

        private void dgCrackCnt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgCrackCnt.CurrentCell == null) return;

                FCS001_137_CRACK_CNT_DETAIL fcs001_137_CrackCntDetail = new FCS001_137_CRACK_CNT_DETAIL();
                fcs001_137_CrackCntDetail.FrameOperation = FrameOperation;

                if (fcs001_137_CrackCntDetail != null)
                {
                    string sLineId = string.Empty;
                    string sModelId = string.Empty;
                    string sBldgCd = string.Empty;
                    string sMin = string.Empty;
                    string sMax = string.Empty;
                    DateTime dtFromDate = new DateTime();
                    DateTime dtToDate = new DateTime();

                    sLineId = ((DataRowView)dgCrackCnt.CurrentRow.DataItem).Row.ItemArray[1].ToString().Trim();
                    sModelId = ((DataRowView)dgCrackCnt.CurrentRow.DataItem).Row.ItemArray[2].ToString().Split()[0].Trim();
                    //sBldgCd = dgCrackCnt.CurrentCell.
                    dtFromDate = DateTime.Parse(dtpFromDate2.SelectedDateTime.ToString("yyyy/MM/dd") + ' ' + dtpFromTime2.DateTime.Value.ToString("HH:mm:00"));
                    dtToDate = DateTime.Parse(dtpToDate2.SelectedDateTime.ToString("yyyy/MM/dd") + ' ' + dtpToTime2.DateTime.Value.ToString("HH:mm:00"));

                    switch (dgCrackCnt.CurrentCell.Column.Index)
                    {
                        case 4:         // 0 ~ 100
                            sMin = "0";
                            sMax = "100";
                            break;
                        case 5:         // 100 ~ 200
                            sMin = "100";
                            sMax = "200";
                            break;
                        case 6:         // 200 ~ 300
                            sMin = "200";
                            sMax = "300";
                            break;
                        case 7:         // 300 ~ 500
                            sMin = "300";
                            sMax = "500";
                            break;
                        case 8:         // 500 ~ 1000
                            sMin = "500";
                            sMax = "1000";
                            break;
                        case 9:         // 1000 ~
                            sMin = "1000";
                            sMax = int.MaxValue.ToString();
                            break;
                        default:
                            return;
                    }
                        
                    object[] Parameters = new object[7];
                    Parameters[0] = sLineId;
                    Parameters[1] = sModelId;
                    Parameters[2] = sBldgCd;
                    Parameters[3] = sMin;
                    Parameters[4] = sMax;
                    Parameters[5] = dtFromDate;
                    Parameters[6] = dtToDate;

                    C1WindowExtension.SetParameters(fcs001_137_CrackCntDetail, Parameters);

                    fcs001_137_CrackCntDetail.Closed += new EventHandler(CrackCntDetail_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => fcs001_137_CrackCntDetail.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CrackCntDetail_Closed(object sender, EventArgs e)
        {
            FCS001_137_CRACK_CNT_DETAIL window = sender as FCS001_137_CRACK_CNT_DETAIL;
            if (window.DialogResult == MessageBoxResult.Cancel)
            {
                // 추가 로직 필요시에 작성.
            }
            this.grdMain.Children.Remove(window);
        }

        #endregion

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion



        private void dgCrackLine_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {

        }


    }
}


