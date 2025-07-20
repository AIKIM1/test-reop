/************************************************************************************
  Created Date : 2021.04.06
       Creator : 정용석
   Description : OCV Daily Status
 ------------------------------------------------------------------------------------
  [Change History]
    2021.08.19  정용석 : Initial Created
    2021.08.25  정용석 : Detail 장표 추가
    2021.12.15  강호운 : 조회조건 미존재시 팝업처리 (에러메시지 처리)
    2022.01.25  김길용 : 날짜별 Summary 데이터 BR_PRD_SEL_LOGIS_OCV_DAILY_SUMMARY 로 수정(팩2,3동 공통화로인한 BR구성)
    2024.05.30  정용석 : E20240313-001105 - [ESWA 모듈 생산팀] GMES 내 STK 일간 데이터 수정 요청
 ************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_026 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private PACK003_026_DataHelper dataHelper = new PACK003_026_DataHelper();
        private int productIDMultiComboBindDataCount = 0;
        private string productID = string.Empty;    // 조회버튼 눌렀을 당시 조회조건 저장 (제품만)
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Constructor
        public PACK003_026()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        // Initialize
        private void Initialize()
        {
            PackCommon.SearchRowCount(ref this.txtRowCount1, 0);
            PackCommon.SearchRowCount(ref this.txtRowCount2, 0);
            this.SetDatePicker();
            PackCommon.SetMultiSelectionComboBox(this.dataHelper.GetProductInfo(), this.cboMultiProductID, ref this.productIDMultiComboBindDataCount);
            this.MakeGridColumn();
        }

        // 날짜 Setting
        private void SetDatePicker()
        {
            this.dtpFromDate.ApplyTemplate();
            this.dtpToDate.ApplyTemplate();
            this.dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-7);
            this.dtpToDate.SelectedDateTime = DateTime.Now;
        }

        // Grid Column Setting (Form Load시)
        private void MakeGridColumn()
        {
            DateTime fromDate = this.dtpFromDate.SelectedDateTime;
            DateTime toDate = this.dtpToDate.SelectedDateTime;
            TimeSpan timeSpan = toDate - fromDate;
            this.dgOCVDailySummary.Columns.Clear();

            // 날짜 데이터 만들어주고.
            DataTable dt = new DataTable();
            dt.Columns.Add("CALDATE", typeof(DateTime));
            dt.Columns.Add("CALDATE_MONTH", typeof(string));
            dt.Columns.Add("CALDATE_DAY", typeof(string));

            for (int i = 0; i < 2; i++)
            {
                DataRow dr = dt.NewRow();
                dr["CALDATE_MONTH"] = "Date";
                dr["CALDATE_DAY"] = "Date";
                dt.Rows.Add(dr);
            }

            for (int i = 0; i <= timeSpan.Days; i++)
            {
                DataRow dr = dt.NewRow();
                DateTime dateTime = fromDate.AddDays(i);
                dr["CALDATE"] = dateTime;
                dr["CALDATE_MONTH"] = Convert.ToInt32(string.Format("{0:MM}", dateTime)).ToString() + "월";
                dr["CALDATE_DAY"] = Convert.ToInt32(string.Format("{0:dd}", dateTime)).ToString();
                dt.Rows.Add(dr);
            }

            // Column Header Setting
            foreach (DataRowView drv in dt.AsDataView())
            {
                bool isNumericName = Regex.IsMatch(drv["CALDATE_DAY"].ToString(), @"^[0-9]+$") ? true : false;
                List<string> lstHeader = new List<string>();
                lstHeader.Add(drv["CALDATE_MONTH"].ToString());
                lstHeader.Add(drv["CALDATE_DAY"].ToString());

                C1.WPF.DataGrid.DataGridTextColumn dataGridTextColumn = new C1.WPF.DataGrid.DataGridTextColumn();
                Binding textBinding = new Binding(drv["CALDATE_DAY"].ToString());
                textBinding.Mode = BindingMode.TwoWay;
                dataGridTextColumn.Header = lstHeader;
                dataGridTextColumn.Binding = textBinding;
                dataGridTextColumn.HorizontalAlignment = isNumericName ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                dataGridTextColumn.VerticalAlignment = VerticalAlignment.Center;
                dataGridTextColumn.Visibility = Visibility.Visible;
                dataGridTextColumn.Width = isNumericName ? new C1.WPF.DataGrid.DataGridLength(60, DataGridUnitType.Pixel) : new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                dataGridTextColumn.IsReadOnly = true;
                this.dgOCVDailySummary.Columns.Add(dataGridTextColumn);
            }
        }

        private void SearchProcess()
        {
            DateTime fromDate = this.dtpFromDate.SelectedDateTime;
            DateTime toDate = this.dtpToDate.SelectedDateTime;
            fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0);
            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 0, 0, 0);

            if (fromDate > toDate)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                this.dtpFromDate.Focus();
                return;
            }

            if (Convert.ToInt32((toDate - fromDate).TotalDays) > 31)
            {
                Util.MessageValidation("SFU2042", "31");   //기간은 {0}일 이내 입니다.
                this.dtpToDate.Focus();
                return;
            }

            // Validation of Search Condition
            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiProductID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("PRODID")); // %1(을)를 선택하세요.
                return;
            }


            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            PackCommon.SearchRowCount(ref this.txtRowCount1, 0);
            PackCommon.SearchRowCount(ref this.txtRowCount2, 0);
            Util.gridClear(this.dgOCVDailySummary);
            Util.gridClear(this.dgOCVDailyDetail);

            try
            {
                // 제품 조회조건은 별도로 저장
                this.productID = Convert.ToString(this.cboMultiProductID.SelectedItemsToString);
                DataSet ds = this.dataHelper.GetOCVDailySummary(fromDate, toDate, this.productID);

                if (ds != null)
                {
                    if (CommonVerify.HasTableRow(ds.Tables["PIVOT"]))
                    {
                        // Column Header Setting
                        this.dgOCVDailySummary.FrozenTopRowsCount = 2;
                        this.dgOCVDailySummary.Columns.Clear();
                        foreach (DataColumn dc in ds.Tables["PIVOT"].Columns.OfType<DataColumn>())
                        {
                            bool? isNumericName = false;
                            if (Regex.IsMatch(dc.ColumnName, @"^[0-9]+$")) isNumericName = true;
                            if (dc.ColumnName.Equals("TOTAL")) isNumericName = null;

                            string calDate = string.Empty;

                            List<string> lstHeader = new List<string>();
                            if (isNumericName == true)
                            {
                                var query = ds.Tables["RESULT"].AsEnumerable().Where(x => x.Field<string>("CALDATE").Equals(dc.ColumnName)).Take(1);
                                foreach (var item in query)
                                {
                                    calDate = item.Field<string>("CALDATE");
                                    lstHeader.Add(item.Field<string>("CALDATE_MONTH"));
                                    lstHeader.Add(Convert.ToInt32(item.Field<string>("CALDATE_DAY")).ToString());
                                }
                            }
                            else if (isNumericName == null)
                            {
                                lstHeader.Add(dc.ColumnName);
                                lstHeader.Add(dc.ColumnName);
                            }
                            else
                            {
                                lstHeader.Add("Date");
                                lstHeader.Add("Date");
                            }

                            C1.WPF.DataGrid.DataGridTextColumn dataGridTextColumn = new C1.WPF.DataGrid.DataGridTextColumn();
                            Binding textBinding = new Binding(dc.ColumnName);
                            textBinding.Mode = BindingMode.TwoWay;
                            dataGridTextColumn.Header = lstHeader;
                            dataGridTextColumn.Binding = textBinding;
                            dataGridTextColumn.HorizontalAlignment = isNumericName != false ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                            dataGridTextColumn.VerticalAlignment = VerticalAlignment.Center;
                            dataGridTextColumn.Visibility = dc.ColumnName.Equals("CATE_CD") ? Visibility.Collapsed : Visibility.Visible;
                            dataGridTextColumn.Width = isNumericName != false ? new C1.WPF.DataGrid.DataGridLength(80, DataGridUnitType.Pixel) : new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                            dataGridTextColumn.IsReadOnly = true;
                            dataGridTextColumn.Tag = calDate;
                            this.dgOCVDailySummary.Columns.Add(dataGridTextColumn);
                        }
                        this.dgOCVDailySummary.Refresh();

                        PackCommon.SearchRowCount(ref this.txtRowCount1, ds.Tables["PIVOT"].Rows.Count);
                        Util.GridSetData(this.dgOCVDailySummary, ds.Tables["PIVOT"], FrameOperation);
                    }
                }
                else
                {
                    Util.MessageValidation("SFU1905"); //조회된 Data가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchDetailProcess(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            try
            {
                PackCommon.DoEvents();
                this.loadingIndicator.Visibility = Visibility.Visible;

                PackCommon.SearchRowCount(ref this.txtRowCount2, 0);
                Util.gridClear(this.dgOCVDailyDetail);

                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
                if (dataGridCell == null)
                {
                    return;
                }


                bool isNumericName = Regex.IsMatch(dataGridCell.Column.Name, @"^[0-9]+$") ? true : false;
                if (!isNumericName)
                {
                    return;
                }

                DateTime fromDate = DateTime.ParseExact(dataGridCell.Column.Tag.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime toDate = DateTime.ParseExact(dataGridCell.Column.Tag.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                string categoryCD = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "CATE_CD").ToString();

                // 합계 Cell은 Return
                if (string.IsNullOrEmpty(categoryCD))
                {
                    return;
                }

                if (categoryCD.Equals("GROUPING_CUBE"))
                {
                    return;
                }

                switch (categoryCD)
                {
                    case "PACKING_OK":
                        dgOCVDailyDetail.Columns["PALLETID"].Visibility = Visibility.Visible;
                        dgOCVDailyDetail.Columns["PALLETDTTM"].Visibility = Visibility.Visible;
                        dgOCVDailyDetail.Columns["SRCTYPE"].Visibility = Visibility.Visible;
                        dgOCVDailyDetail.Columns["LOTCALDATE"].Visibility = Visibility.Visible;
                        break;
                    default:
                        dgOCVDailyDetail.Columns["PALLETID"].Visibility = Visibility.Collapsed;
                        dgOCVDailyDetail.Columns["PALLETDTTM"].Visibility = Visibility.Collapsed;
                        dgOCVDailyDetail.Columns["SRCTYPE"].Visibility = Visibility.Collapsed;
                        dgOCVDailyDetail.Columns["LOTCALDATE"].Visibility = Visibility.Collapsed;
                        break;
                }

                Util.gridClear(this.dgOCVDailyDetail);
                //// 순서도 호출 - OCV Daily Detail (남조선에서 순서도 호출하는 경우 결과 리턴 Time이 너무 오래 걸리는 관계로 이 Scope 추가 (결론은 똑같음))
                //DataTable dt = this.dataHelper.GetOCVDailyDetailInfo(categoryCD, fromDate, toDate, this.productID);
                //if (CommonVerify.HasTableRow(dt))
                //{
                //    PackCommon.SearchRowCount(ref this.txtRowCount2, dt.Rows.Count);
                //    Util.GridSetData(this.dgOCVDailyDetail, dt, FrameOperation);
                //}

                // 순서도 호출 - OCV Daily Detail(Desired Function!!!)
                DataSet ds = this.dataHelper.GetOCVDailyDetailInfo(categoryCD, fromDate, toDate, this.productID);
                if (CommonVerify.HasTableRow(ds.Tables["OUTDATA"]))
                {
                    PackCommon.SearchRowCount(ref this.txtRowCount2, ds.Tables["OUTDATA"].Rows.Count);
                    Util.GridSetData(this.dgOCVDailyDetail, ds.Tables["OUTDATA"], FrameOperation);
                }

                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void dgOCVDailySummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string categoryCD = DataTableConverter.GetValue(e.Cell.Row.DataItem, "CATE_CD").ToString();
                    bool isNumericName = Regex.IsMatch(e.Cell.Column.Name, @"^[0-9]+$") ? true : false;
                    if (isNumericName && categoryCD.Equals("GROUPING_CUBE"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (isNumericName)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOCVDailySummary_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.SearchDetailProcess(sender, e);
        }
        #endregion
    }

    internal class PACK003_026_DataHelper
    {
        #region Member Variable Lists...
        private DataTable dtProductInfo = new DataTable();
        #endregion

        #region Constructor
        internal PACK003_026_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - 제품
        internal DataTable GetProductInfo()
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_SHOP_PRDT_ROUT_MODULE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    var query = dtRSLTDT.AsEnumerable().Select(x => new
                    {
                        PRODID = x.Field<string>("PRODID"),
                        PRODNAME = x.Field<string>("PRODID") + " : " + x.Field<string>("PRODNAME")
                    });

                    dtReturn = PackCommon.queryToDataTable(query.ToList());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - OCV Daily Summary
        internal DataTable GetOCVDailySummaryInfo(DateTime fromDate, DateTime toDate, string productID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_SUMMARY_V2";
                //string bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_SUMMARY_V2_PRODTEST";  // For Test
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("FROMDATE", typeof(string));
                dtRQSTDT.Columns.Add("TODATE", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["FROMDATE"] = fromDate.ToString("yyyy-MM-dd");
                drRQSTDT["TODATE"] = toDate.ToString("yyyy-MM-dd");
                drRQSTDT["PRODID"] = productID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        //// 순서도 호출 - OCV Daily Detail (남조선에서 순서도 호출하는 경우 결과 리턴 Time이 너무 오래 걸리는 관계로 이 Scope 추가 (결론은 똑같음))
        //internal DataTable GetOCVDailyDetailInfo(string categoryCD, DateTime fromDate, DateTime toDate, string productID)
        //{
        //    DataTable dtReturn = new DataTable();
        //    string bizRuleName = string.Empty;

        //    switch (categoryCD)
        //    {
        //        case "OCV1_WAIT_STK":
        //        case "OCV2_WAIT_STK":
        //        case "OCV_WAIT_STK":
        //        case "OCV_NG_STK":
        //            bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_STOCKER";
        //            //bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_STOCKER_PRODTEST";     // For Test
        //            break;
        //        case "P5300_OK":
        //        case "P5400_OK":
        //            bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_END_LOT";
        //            //bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_END_LOT_PRODTEST";     // For Test
        //            break;
        //        case "P5300_NG":
        //        case "P5400_NG":
        //            bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_NG";
        //            //bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_NG_PRODTEST";     // For Test
        //            break;
        //        case "PACKING_OK":
        //            bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_PACKING";
        //            //bizRuleName = "DA_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_PACKING_PRODTEST";     // For Test
        //            break;
        //        default:
        //            break;
        //    }

        //    try
        //    {
        //        DataTable dtRQSTDT = new DataTable("RQSTDT");
        //        DataTable dtRSLTDT = new DataTable("RSLTDT");

        //        dtRQSTDT.Columns.Add("LANGID", typeof(string));
        //        dtRQSTDT.Columns.Add("SHOPID", typeof(string));
        //        dtRQSTDT.Columns.Add("AREAID", typeof(string));
        //        dtRQSTDT.Columns.Add("FROMDATE", typeof(string));
        //        dtRQSTDT.Columns.Add("TODATE", typeof(string));
        //        dtRQSTDT.Columns.Add("CATE_CD", typeof(string));
        //        dtRQSTDT.Columns.Add("PRODID", typeof(string));

        //        DataRow drRQSTDT = dtRQSTDT.NewRow();
        //        drRQSTDT["LANGID"] = LoginInfo.LANGID;
        //        drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
        //        drRQSTDT["FROMDATE"] = fromDate.ToString("yyyy-MM-dd");
        //        drRQSTDT["TODATE"] = toDate.ToString("yyyy-MM-dd");
        //        drRQSTDT["CATE_CD"] = categoryCD;
        //        drRQSTDT["PRODID"] = productID;
        //        dtRQSTDT.Rows.Add(drRQSTDT);

        //        dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

        //        if (CommonVerify.HasTableRow(dtRSLTDT))
        //        {
        //            dtReturn = dtRSLTDT.Copy();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }


        //    return dtReturn;
        //}

        // 순서도 호출 - OCV Daily Detail(Desired Function!!!)
        internal DataSet GetOCVDailyDetailInfo(string categoryCD, DateTime fromDate, DateTime toDate, string productID)
        {
            string bizRuleName = "BR_PRD_SEL_LOGIS_OCV_DAILY_DETAIL";
            //string bizRuleName = "BR_PRD_SEL_LOGIS_OCV_DAILY_DETAIL_PRODTEST";  // For Test
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA";
            string lstProdID = string.Join(",", this.GetProductInfo().AsEnumerable().Select(x => x.Field<string>("PRODID")).ToList());
            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("FROMDATE", typeof(string));
                dtINDATA.Columns.Add("TODATE", typeof(string));
                dtINDATA.Columns.Add("CATE_CD", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                drINDATA["FROMDATE"] = fromDate.ToString("yyyy-MM-dd");
                drINDATA["TODATE"] = toDate.ToString("yyyy-MM-dd");
                drINDATA["CATE_CD"] = categoryCD;
                drINDATA["PRODID"] = string.IsNullOrEmpty(productID) ? lstProdID : productID;
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
                if (!CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

            return dsOUTDATA;
        }

        // 순서도 호출 - 실적 Summary
        internal DataSet GetOCVDailySummary(DateTime fromDate, DateTime toDate, string productID)
        {
            DataSet ds = new DataSet();
            DataTable dt = this.GetOCVDailySummaryInfo(fromDate, toDate, productID);
            dt.TableName = "RESULT";

            if (!CommonVerify.HasTableRow(dt))
            {
                return null;
            }

            // 원본 DataTable Add (For Column Name)
            ds.Tables.Add(dt.Copy());

            // Step 1 : 구분 합계 Row가 아닌것만 Pivotting
            var query = dt.AsEnumerable().Where(x => x.Field<int>("CALDATE_SUMMARY_FLAG").Equals(0)).Select(x => new
            {
                CALDATE = x.Field<string>("CALDATE"),
                CATE_CD = x.Field<string>("CATE_CD"),
                CATE_NAME = x.Field<string>("CATE_NAME"),
                INQTY = x.Field<int>("INQTY")
            });
            DataTable dtEditing = PackCommon.queryToDataTable(query.ToList());
            var pivotColumn = dtEditing.Columns.OfType<DataColumn>().Where(x => x.ColumnName.Equals("CALDATE")).FirstOrDefault();
            var pivotValue = dtEditing.Columns.OfType<DataColumn>().Where(x => x.ColumnName.Equals("INQTY")).FirstOrDefault();
            DataTable dtPivoting = this.Pivot(dtEditing, pivotColumn, pivotValue);

            // 구분별 합계 추가
            var gubunSummary = dt.AsEnumerable().Where(x => x.Field<int>("CALDATE_SUMMARY_FLAG").Equals(1)).Select(x => new
            {
                CATE_CD = x.Field<string>("CATE_CD"),
                INQTY = x.Field<int>("INQTY")
            });

            // Pivoting 결과와 gubunSummary Join
            DataTable dtResult = dtPivoting.Copy();
            dtResult.Columns.Add("TOTAL", typeof(int));
            foreach (DataRow dr in dtResult.Rows)
            {
                int gubunTotal = gubunSummary.Where(x => x.CATE_CD.Equals(dr["CATE_CD"])).Select(x => x.INQTY).FirstOrDefault();
                dr["TOTAL"] = gubunTotal;
            }
            dtResult.AcceptChanges();

            dtResult.TableName = "PIVOT";
            ds.Tables.Add(dtResult);

            return ds;
        }

        // Pivotting
        internal DataTable Pivot(DataTable dt, DataColumn pivotColumn, DataColumn pivotValue)
        {
            // find primary key columns
            DataTable dtTemp = dt.Copy();
            dtTemp.Columns.Remove(pivotColumn.ColumnName);
            dtTemp.Columns.Remove(pivotValue.ColumnName);
            string[] pkColumnNames = dtTemp.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            // prep results table
            DataTable dtResult = dtTemp.DefaultView.ToTable(true, pkColumnNames).Copy();
            dtResult.PrimaryKey = dtResult.Columns.Cast<DataColumn>().ToArray();
            dt.AsEnumerable().Select(r => r[pivotColumn.ColumnName].ToString()).Distinct().ToList()
            .ForEach(c => dtResult.Columns.Add(c, pivotColumn.DataType));

            // load it
            foreach (DataRow dr in dt.Rows)
            {
                // find row to update
                DataRow aggregateRow = dtResult.Rows.Find(pkColumnNames.Select(c => dr[c]).ToArray());
                aggregateRow[dr[pivotColumn.ColumnName].ToString()] = dr[pivotValue.ColumnName];
            }

            return dtResult;
        }
        #endregion
    }
}