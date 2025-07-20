/*************************************************************************************
 Created Date : 2020.11.19
      Creator : 박준규
   Decription : 충방전기 가동현황
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.18  DEVELOPER : 박준규
  2022.02.23  KDH : AREA 조건 추가
  2023.01.25  홍석원 : 라인별 JF와 충방전기의 가동률/비가동률/부동률을 확인할 수 있도록 UI 수정
  2023.08.15  손동혁 : NA 1동 요청 날짜 지정해서 이력조회 할 수 있도록 TAP 추가 
  2023.10.10  손동혁 : 이력조회 조건 라인아이디 -> 레인아이디 수정 
    

**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_004 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        bool bUseFlag = false; //2023.08.15 NA1동 이력조회 탭 추가

        public FCS001_004()
        {
            InitializeComponent();
        }
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

        #endregion

        #region Event 

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();


            /// 2023.08.15
            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_004"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.
            if (bUseFlag)
            {
                TabSearch.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                TabSearch.Visibility = Visibility.Collapsed;

            }


            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
           
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");

           
        }

        private void InitControl()
        {
            try
            {
                dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
                dtpFromTime.DateTime = DateTime.Now.AddDays(-1).AddMinutes(-1);
                dtpToDate.SelectedDateTime = DateTime.Now; ;
                dtpToTime.DateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnHistorySearch_Click(object sender, RoutedEventArgs e)
        {
            GetHistoyList();
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgFomtOperStatus_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            DataRowView drv = dgFomtOperStatus.SelectedItem as DataRowView;
            if (drv == null) return;

            string laneId = Util.NVC(drv["LANE_ID"]);
            string eqpKind = Util.NVC(drv["EQP_KIND"]);

            getDetailList(laneId, eqpKind);
        }
        /// <summary>
        ///2023.08.15 NA 1동 특화 날짜 지정해서 이력조회 할 수 있도록 TAP 추가
        /// 이력조회 dgFomtOperHistory 클릭
        /// </summary>

        private void dgFomtOperHistory_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            DataRowView drv = dgFomtOperHistory.SelectedItem as DataRowView;
            if (drv == null) return;
            string FromDate = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:ss");
            string ToDate = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:ss");

            string LaneID = Util.NVC(drv["LANE_ID"]);
            if (!Util.NVC(drv["JOB_DATE"]).Equals(ObjectDic.Instance.GetObjectName("TOTAL")))
            {
                FromDate = Convert.ToDateTime(Util.NVC(drv["MAKE_FROM_TIME"])).ToString("yyyy-MM-dd HH:mm:ss");
                ToDate = Convert.ToDateTime(Util.NVC(drv["MAKE_TO_TIME"])).ToString("yyyy-MM-dd HH:mm:ss");
               
            }
            
            getDetailHistoryList(LaneID, FromDate,ToDate);
        }

        /// <summary>
        /// 2023.08.15
        /// </summary>
        private void dgFomtOperHistoryDetail_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            DataRowView drv = dgFomtOperHistoryDetail.SelectedItem as DataRowView;
            if (drv == null) return;
            string Eqptid = Util.NVC(drv["EQPTID"]);
            string FromDate = Convert.ToDateTime(Util.NVC(drv["MAKE_FROM_TIME"])).ToString("yyyy-MM-dd HH:mm:ss");
            string ToDate = Convert.ToDateTime(Util.NVC(drv["MAKE_TO_TIME"])).ToString("yyyy-MM-dd HH:mm:ss");

            getDetailEndHistoryList(Eqptid, FromDate, ToDate);
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

        #region Method

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2022.02.23_AREA 조건 추가

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; //2022.02.23_AREA 조건 추가
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_FORMATION_RUN_STATUS_BY_LANE", "RQSTDT", "RSLTDT", dtRqst, (result, exception) =>
                {
                    try
                    {
                     
                            if (exception != null)
                            throw exception;

                        Util.gridClear(dgFomtOperStatus);
                        Util.gridClear(dgFomtOperDetail);
                        Util.GridSetData(dgFomtOperStatus, result, this.FrameOperation);
                  
                        string[] mergeColumnName = new string[] { "LANE_NAME" };
                      //  _Util.gridSumColumnAdd(dtRqst, "LANE_NAME");
                        _Util.SetDataGridMergeExtensionCol(dgFomtOperStatus, mergeColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                        

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

        private void getDetailList(string laneId, string eqpKind)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("EQP_KIND", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LANE_ID"] = laneId;
                dr["EQP_KIND"] = eqpKind;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_FORMATION_RUN_STATUS_BY_LANE_DETAIL", "RQSTDT", "RSLTDT", dtRqst, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgFomtOperDetail);
                        Util.GridSetData(dgFomtOperDetail, result, this.FrameOperation);
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

        ///2023.08.15 
        private void getDetailHistoryList(string LaneID, string FormDate , string Todate)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LANEID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = FormDate;
                dr["TO_DATE"] = Todate;

                dr["LANEID"] = LaneID;
        
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_FORMATION_RUN_STATUS_BY_LANE_HISTORY_DETAIL", "RQSTDT", "RSLTDT", dtRqst, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgFomtOperHistoryDetail);
                        Util.gridClear(dgFomtOperHistoryEndDetail);

                        Util.GridSetData(dgFomtOperHistoryDetail, result, this.FrameOperation);
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
        ///2023.08.15 
        private void getDetailEndHistoryList(string Eqptid , string FormDate , string Todate)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
              

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Eqptid;
                dr["FROM_DATE"] = FormDate;
                dr["TO_DATE"] = Todate;
 
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_FORMATION_RUN_STATUS_BY_LANE_HISTORY_END_DETAIL", "RQSTDT", "RSLTDT", dtRqst, (result, exception) =>
             
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgFomtOperHistoryEndDetail);
                        Util.GridSetData(dgFomtOperHistoryEndDetail, result, this.FrameOperation);
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

        ///2023.08.15 
        private void GetHistoyList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string)); //2022.02.23_AREA 조건 추가
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:00");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:59");
             

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_FORMATION_RUN_STATUS_BY_LANE_HISTORY", "RQSTDT", "RSLTDT", dtRqst, (result, exception) =>
               
                {
                    try
                    {

                        if (exception != null)
                            throw exception;

                        // Summary 추가
                        DataTable dtsum = new DataTable();

                        string[] grCode = new string[] { "LANE_NAME", "LANE_ID" , "EQSGID" };
                        string[] colName = result.Columns.Cast<DataColumn>()
                                                                            .Where(x => !(x.ColumnName.Contains("LANE_NAME")) && !(x.ColumnName.Contains("JOB_DATE"))
                                                                            && !(x.ColumnName.Contains("LANE_ID")) && !(x.ColumnName.Contains("RATE_RUN"))
                                                                            && !(x.ColumnName.Contains("RATE_SUM")) && !(x.ColumnName.Contains("RATE_TROUBLE"))
                                                                            && !(x.ColumnName.Contains("EQSGID")) && !(x.ColumnName.Contains("MAKE_FROM_TIME"))
                                                                            && !(x.ColumnName.Contains("MAKE_TO_TIME")))
                                                                            .Select(x => x.ColumnName)
                                                                            .ToArray();
                        dtsum = Util.GetGroupBySum(result, colName, grCode, true);

                        ChangeValueToTotal(dtsum, "JOB_DATE");

                        DataTable dtTarget = new DataTable();
                        dtTarget = MergeAndSortDataTable(result, dtsum, "LANE_NAME ASC");
                        GetColumnValueAvg(dtTarget, "RATE_RUN");
                        GetColumnValueAvg(dtTarget, "RATE_SUM");
                        GetColumnValueAvg(dtTarget, "RATE_TROUBLE");
                        // Summary 종료

                        Util.gridClear(dgFomtOperHistory);
                        Util.gridClear(dgFomtOperHistoryDetail);
                        Util.gridClear(dgFomtOperHistoryEndDetail);

                        Util.GridSetData(dgFomtOperHistory, dtTarget, this.FrameOperation);

                        string[] mergeColumnName = new string[] { "LANE_NAME", "LANE_ID", "EQSGID" };
                        //  _Util.gridSumColumnAdd(dtRqst, "LANE_NAME");
                        _Util.SetDataGridMergeExtensionCol(dgFomtOperHistory, mergeColumnName, DataGridMergeMode.VERTICALHIERARCHI);



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

        // 라인ID 다중선택 콤보박스 초기화        

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
                dt.Rows[i][sColumnName] = ObjectDic.Instance.GetObjectName("TOTAL");
            }

            return dt;
        }

        ///2023.08.15 
        /// <summary>
        /// DataTable 정렬.
        /// </summary>
       
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

        ///2023.08.15 
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



        #endregion

       
    }
}
