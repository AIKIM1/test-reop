/*************************************************************************************
 Created Date : 2022.02.03
      Creator : Choi Seok Jun
   Decription : Formation S/C Report
--------------------------------------------------------------------------------------
 [Change History]
  2022.02.03  Choi Seok Jun / Initial Created
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_135 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;

        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;

        #endregion

        #region [Initialize]
        public FCS001_135()
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
            //Combo Setting
            InitCombo();

            SetWorkResetTime();
            //Control Setting
            InitControl();

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }
        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpFromTime.DateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();
            dtpToTime.DateTime = GetJobDateTo();
        }
        #endregion

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "1,J" };
            _combo.SetCombo(cboFormType, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterEqpType);
        }

        #region [Method]
        private void GetList()
        {
            try
            {
                if ((dtpToDate.SelectedDateTime.Date - dtpFromDate.SelectedDateTime.Date).Days >= 7)
                {
                    Util.Alert("FM_ME_0231"); //조회기간은 7일을 초과할 수 없습니다.
                    return;
                }

                Util.gridClear(FormSTKReport);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQPT_GR_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") + dtpFromTime.DateTime.Value.ToString("HHmm00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd") + dtpToTime.DateTime.Value.ToString("HHmm00");
                dr["EQPT_GR_TYPE_CODE"] = Util.GetCondition(cboFormType);

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("BR_GET_FORM_STK_REPORT_LIST", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
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
                            // Summary 추가
                            DataTable dtsum = new DataTable();

                            string[] grCode = new string[1] { "EQPTNAME" };
                            string[] colName = result.Columns.Cast<DataColumn>()
                                                                                .Where(x => !(x.ColumnName.Contains("EQPTNAME")) && !(x.ColumnName.Contains("WORK_DATE"))
                                                                                            && !(x.ColumnName.Contains("STACK_RATE")) && !(x.ColumnName.Contains("TACT_SEC_TIME"))
                                                                                            && !(x.ColumnName.Contains("EQP_LOAD_RATE")) && !(x.ColumnName.Contains("DOUBLE_FORK_RATE")))
                                                                                .Select(x => x.ColumnName)
                                                                                .ToArray();
                            dtsum = Util.GetGroupBySum(result, colName, grCode, true);

                            ChangeValueToTotal(dtsum, "WORK_DATE");

                            DataTable dtTarget = new DataTable();
                            dtTarget = MergeAndSortDataTable(result, dtsum, "EQPTNAME ASC, WORK_DATE ASC");
                            ChangeValueFromTotalToEmpty(dtTarget, "WORK_DATE", "EQPTNAME");
                            GetColumnValueAvg(dtTarget, "TACT_SEC_TIME");
                            // Summary 종료

                            Util.GridSetData(FormSTKReport, dtTarget, FrameOperation, true);

                            Util _Util = new Util();
                            string[] sColumnName = new string[] { "EQPTNAME" };
                            _Util.SetDataGridMergeExtensionCol(FormSTKReport, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 공통함수로 뺄지 확인 필요 START
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
        // 공통함수로 뺄지 확인 필요 END

        /// <summary>
        /// DataTable에서 특정 컬럼의 Value값을 Total로 리턴함.
        /// </summary>
        /// <param name="dt">Target DataTable</param>
        /// <param name="sColumnName">값을 바꾸고자 하는 Column명</param>
        /// <returns></returns>
        private DataTable ChangeValueToTotal(DataTable dt, string sColumnName)
        {
            if (dt == null || dt.Rows.Count < 1) { return dt; }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i][sColumnName] = ObjectDic.Instance.GetObjectName("합계");
            }

            return dt;
        }

        /// <summary>
        /// DataTable에서 컬럼의 Total 대상을 바꿈.
        /// </summary>
        /// <param name="dt">Target DataTable</param>
        /// <param name="sFromColumnName">값을 공백으로 바꿀 Column명</param>
        /// <param name="sToColumnName">값을 Total로 바꾸고자 하는 Column명</param>
        /// <returns></returns>
        private DataTable ChangeValueFromTotalToEmpty(DataTable dt, string sFromColumnName, string sToColumnName)
        {
            if (dt == null || dt.Rows.Count < 1) { return dt; }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][sFromColumnName].ToString() == ObjectDic.Instance.GetObjectName("합계"))
                {
                    dt.Rows[i][sToColumnName] = ObjectDic.Instance.GetObjectName("합계");
                    dt.Rows[i][sFromColumnName] = "";
                }
            }

            return dt;
        }

        /// <summary>
        /// DataTable 정렬.
        /// </summary>
        /// <param name="sSort">Sort 조건</param>
        /// <returns></returns>
        private DataTable MergeAndSortDataTable(DataTable dt1, DataTable dt2, string sSort)
        {
            DataTable dt_temp = new DataTable();
            dt_temp.Merge(dt1);
            dt_temp.Merge(dt2);

            DataView dv = new DataView(dt_temp);
            dv.Sort = sSort;
            dt_temp = dv.ToTable();

            return dt_temp;
        }

        /// <summary>
        /// 해당 Column의 평균값을 구함.
        /// </summary>
        /// <param name="sColumnName">대상 Column명</param>
        /// <returns></returns>
        private DataTable GetColumnValueAvg(DataTable dt, string sColumnName)
        {
            decimal tt = 0;
            decimal ttSum = 0;
            int ttCnt = 0;
            int ColumnIndex = dt.Columns.IndexOf(sColumnName);
            object obj = new object();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                obj = dt.Rows[i].ItemArray[ColumnIndex];

                if (decimal.TryParse(Util.NVC(obj), System.Globalization.NumberStyles.Any, null, out tt))
                {
                    ttCnt++;
                    ttSum += tt;
                    tt = 0;
                }
                else
                {
                    dt.Rows[i][ColumnIndex] = ttSum / ttCnt;
                    ttSum = 0;
                    ttCnt = 0;
                }
            }

            return dt;
        }

        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
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

        #endregion

        private void dgFormSTKReport_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                int TopRowCount = FormSTKReport.TopRows.Count;
                int SummaryNameColumnIndex = 1;
                string CurrentRowSummaryName = string.Empty;
                SolidColorBrush SummaryHighlightColor = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF7E9D5"));

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;
                    try
                    {
                        if (e.Cell.Row.Index >= TopRowCount)
                        {
                            CurrentRowSummaryName = e.Cell.DataGrid.GetDataTable().Rows[e.Cell.Row.Index - TopRowCount][SummaryNameColumnIndex].ToString();

                            if (CurrentRowSummaryName == ObjectDic.Instance.GetObjectName("합계"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.Background = SummaryHighlightColor;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
