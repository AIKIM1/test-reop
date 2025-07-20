/*************************************************************************************
 Created Date : 2021.03.30
      Creator : INS 김동일K
   Decription : 일별 재고 비교 (MES-ERP) 팝업 추가
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.30  INS 김동일K : Initial Created.
  2024.10.15      김영택  : RFC 3.1 작업 내역 제거 롤백 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using SAP.Middleware.Connector;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_077_CMP_DAY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_077_CMP_DAY : C1Window, IWorkArea
    {
        private string _SHOPID = "";
        private string _AREAID = "";
        private string _LOCATIONID = "";
        private string _TYPE = "";
        private string _YYMM = "";

        private DateTime _CalDate;

        RfcDestination dest;

        public COM001_077_CMP_DAY()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps?.Length > 10)
                {
                    _SHOPID = Util.NVC(tmps[0]);
                    txtShop.Text = Util.NVC(tmps[1]);

                    _AREAID = Util.NVC(tmps[2]);
                    txtArea.Text = Util.NVC(tmps[3]);

                    _LOCATIONID = Util.NVC(tmps[4]);
                    txtLocation.Text = Util.NVC(tmps[5]);

                    txtProdID.Text = Util.NVC(tmps[6]);

                    _TYPE = Util.NVC(tmps[7]);
                    _YYMM = Util.NVC(tmps[8]);

                    if (tmps[9].GetType() == typeof(bool))
                        chkMoving.IsChecked = (bool)tmps[9];

                    if (tmps[10].GetType() == typeof(bool))
                        chkFinlwip.IsChecked = (bool)tmps[10];

                    if (_TYPE.Equals("CURR"))
                    {
                        DateTime dtMonthFirst = DateTime.Now.AddDays(1 - DateTime.Now.Day);
                        DateTime dtMonthLast = dtMonthFirst.AddMonths(1).AddDays(-1);
                        dtpDateFrom.SelectedDateTime = dtMonthFirst;
                        dtpDateTo.SelectedDateTime = dtMonthLast;
                    }
                    else if (_TYPE.Equals("DAY"))
                    {
                        DateTime dtTmp;
                        if (_YYMM.Length == 8 && DateTime.TryParse(_YYMM.Substring(0, 4) + "-" + _YYMM.Substring(4, 2) + "-" + _YYMM.Substring(6, 2) + " 00:00:00", out dtTmp))
                        {
                            DateTime dtMonthFirst = dtTmp.AddDays(1 - dtTmp.Day);
                            DateTime dtMonthLast = dtMonthFirst.AddMonths(1).AddDays(-1);
                            dtpDateFrom.SelectedDateTime = dtMonthFirst;
                            dtpDateTo.SelectedDateTime = dtMonthLast;
                        }
                    }
                    else
                    {
                        DateTime dtTmp;
                        if (_YYMM.Length == 6 && DateTime.TryParse(_YYMM.Substring(0, 4) + "-" + _YYMM.Substring(4, 2) + "-01" + " 00:00:00", out dtTmp))
                        {
                            DateTime dtMonthFirst = dtTmp.AddMonths(-1).AddDays(1 - dtTmp.AddMonths(-1).Day);
                            DateTime dtMonthLast = dtMonthFirst.AddMonths(1).AddDays(-1);
                            dtpDateFrom.SelectedDateTime = dtMonthFirst;
                            dtpDateTo.SelectedDateTime = dtMonthLast;
                        }
                    }

                    if (_TYPE.Equals("CURR") || _TYPE.Equals("DAY"))
                    {
                        GetCaldate();
                    }

                    dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
                    dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

                    InitSapInfo();

                    this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                }
                else
                {
                    this.DialogResult = MessageBoxResult.Cancel;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1)
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_CalDate.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1 && dtpDateFrom.SelectedDateTime.Year > 1)
            {
                if ((_CalDate - dtpDateTo.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateTo.SelectedDateTime = _CalDate;
                    return;
                }
                else if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }

        private void InitSapInfo()
        {
            ERPDestinationConfig cfg = new ERPDestinationConfig();

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }

            dest = RfcDestinationManager.GetDestination(ConfigurationManager.AppSettings["APP_SERVER"].Contains("DEV") ? "QAS" : "PRD");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetStocks();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetStocks()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 60)
                {
                    //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "60");
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                #region Initialize Grid            
                Util.gridClear(dgList);
                #endregion

                #region Get GMES Stock List
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SUM_TYPE", typeof(string));
                IndataTable.Columns.Add("SUM_YM", typeof(string));
                IndataTable.Columns.Add("SUM_DATE", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("SLOC_ID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("FINL_WIP_FLAG", typeof(string));
                IndataTable.Columns.Add("SUM_INCLUDE_HOLD_FLAG", typeof(string));
                IndataTable.Columns.Add("SUM_DATE_STRT_END_FLAG", typeof(string));
                IndataTable.Columns.Add("SUM_STRT_DATE", typeof(string));
                IndataTable.Columns.Add("SUM_END_DATE", typeof(string));

                DataRow Indata = IndataTable.NewRow();

                Indata["SUM_TYPE"] = "DAY";
                //if (_TYPE.Equals("DAY"))
                Indata["SUM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                //if (_TYPE.Equals("MONTH"))
                //    Indata["SUM_YM"] = _YYMM;

                Indata["SHOPID"] = _SHOPID;
                Indata["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text.Trim()) ? null : txtProdID.Text.Trim();
                Indata["SLOC_ID"] = string.IsNullOrWhiteSpace(_LOCATIONID) ? null : _LOCATIONID;
                Indata["AREAID"] = string.IsNullOrWhiteSpace(_AREAID) ? null : _AREAID;

                if (chkFinlwip.IsChecked == true)
                    Indata["FINL_WIP_FLAG"] = "Y";
                else
                    Indata["FINL_WIP_FLAG"] = "N";

                Indata["SUM_INCLUDE_HOLD_FLAG"] = "Y";
                Indata["SUM_DATE_STRT_END_FLAG"] = "Y";
                Indata["SUM_STRT_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["SUM_END_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");


                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_SUMMARY2", "INDATA", "RSLTDT", IndataTable, (dt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        #region Column Add
                        if (!dt.Columns.Contains("ZBEGIN_STOCK"))
                            dt.Columns.Add("ZBEGIN_STOCK", typeof(decimal));
                        if (!dt.Columns.Contains("ZEND_STOCK"))
                            dt.Columns.Add("ZEND_STOCK", typeof(decimal));
                        if (!dt.Columns.Contains("ZBEGIN_INTRANSIT"))
                            dt.Columns.Add("ZBEGIN_INTRANSIT", typeof(decimal));
                        if (!dt.Columns.Contains("ZEND_INTRANSIT"))
                            dt.Columns.Add("ZEND_INTRANSIT", typeof(decimal));
                        if (!dt.Columns.Contains("ZPROD_GR"))
                            dt.Columns.Add("ZPROD_GR", typeof(decimal));
                        if (!dt.Columns.Contains("ZPROD_GI"))
                            dt.Columns.Add("ZPROD_GI", typeof(decimal));
                        if (!dt.Columns.Contains("ZMOVE_IN"))
                            dt.Columns.Add("ZMOVE_IN", typeof(decimal));
                        if (!dt.Columns.Contains("ZMOVE_OUT"))
                            dt.Columns.Add("ZMOVE_OUT", typeof(decimal));
                        if (!dt.Columns.Contains("ZMOVE_IN_BLOCK"))
                            dt.Columns.Add("ZMOVE_IN_BLOCK", typeof(decimal));
                        if (!dt.Columns.Contains("ZMOVE_OUT_BLOCK"))
                            dt.Columns.Add("ZMOVE_OUT_BLOCK", typeof(decimal));
                        if (!dt.Columns.Contains("ZPUR_GR"))
                            dt.Columns.Add("ZPUR_GR", typeof(decimal));
                        if (!dt.Columns.Contains("ZSALES_GI"))
                            dt.Columns.Add("ZSALES_GI", typeof(decimal));
                        if (!dt.Columns.Contains("ZINTRANSIT_GR"))
                            dt.Columns.Add("ZINTRANSIT_GR", typeof(decimal));
                        if (!dt.Columns.Contains("ZINTRANSIT_GI"))
                            dt.Columns.Add("ZINTRANSIT_GI", typeof(decimal));
                        if (!dt.Columns.Contains("ZETC_IN"))
                            dt.Columns.Add("ZETC_IN", typeof(decimal));
                        if (!dt.Columns.Contains("ZETC_OUT"))
                            dt.Columns.Add("ZETC_OUT", typeof(decimal));
                        if (!dt.Columns.Contains("ZUNIDENTIFIED_QTY"))
                            dt.Columns.Add("ZUNIDENTIFIED_QTY", typeof(decimal));
                        if (!dt.Columns.Contains("ERFME"))
                            dt.Columns.Add("ERFME", typeof(string));
                        if (!dt.Columns.Contains("SUM_QTY2_ERP"))
                            dt.Columns.Add("SUM_QTY2_ERP", typeof(decimal));
                        if (!dt.Columns.Contains("MOVING_LOT_QTY2_ERP"))
                            dt.Columns.Add("MOVING_LOT_QTY2_ERP", typeof(decimal));
                        #endregion

                        try
                        {
                            #region Get ERP Stock List

                            RfcRepository repo = dest.Repository;

                            IRfcFunction FnStockList = repo.CreateFunction("ZPPB_ERP_BEGIN_END_STOCK");

                            FnStockList.SetValue("I_DAY_FROM", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                            if (txtProdID.Text != "")
                                FnStockList.SetValue("I_DAY_TO", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                            FnStockList.SetValue("I_WERKS", _SHOPID);                                               //Plant         *필수
                            FnStockList.SetValue("I_LGORT", _LOCATIONID);                                           //저장위치      *필수

                            FnStockList.SetValue("I_MATNR", txtProdID.Text);                                        //제품ID
                            FnStockList.SetValue("I_FLAG", "X");                                                    //수불없는 날짜 포함 (X or Blank)

                            FnStockList.Invoke(dest);

                            var stockList = FnStockList.GetTable("IT_STOCK_GRGI");
                            var dtStockList = stockList.ToDataTable("IT_STOCK_GRGI");
                            #endregion

                            #region Merge Based on GMES
                            foreach (DataRow dr in dt.Rows)
                            {
                                DataView dv = dtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString() && x.Field<string>("ZDAY") == dr["SUM_DATE"].ToString()).AsDataView();

                                if (dv.Count.Equals(0))
                                {
                                    dr["ZBEGIN_STOCK"] = 0;
                                    dr["ZEND_STOCK"] = 0;
                                    dr["ZBEGIN_INTRANSIT"] = 0;
                                    dr["ZEND_INTRANSIT"] = 0;
                                    dr["ZPROD_GR"] = 0;
                                    dr["ZPROD_GI"] = 0;
                                    dr["ZMOVE_IN"] = 0;
                                    dr["ZMOVE_OUT"] = 0;
                                    dr["ZMOVE_IN_BLOCK"] = 0;
                                    dr["ZMOVE_OUT_BLOCK"] = 0;
                                    dr["ZPUR_GR"] = 0;
                                    dr["ZSALES_GI"] = 0;
                                    dr["ZINTRANSIT_GR"] = 0;
                                    dr["ZINTRANSIT_GI"] = 0;
                                    dr["ZETC_IN"] = 0;
                                    dr["ZETC_OUT"] = 0;
                                    dr["ZUNIDENTIFIED_QTY"] = 0;
                                    dr["ERFME"] = "";

                                    dr["SUM_QTY2_ERP"] = 0;
                                    dr["MOVING_LOT_QTY2_ERP"] = 0;

                                    if (chkMoving.IsChecked == true)
                                    {
                                        dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                        dr["MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                    }
                                    else
                                    {
                                        dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        dr["GAP_SUM_QTY2"] = dr["SUM_QTY2"];
                                    }
                                }
                                else if (dv.Count > 0)
                                {
                                    dr["ZBEGIN_STOCK"] = dv[0]["ZBEGIN_STOCK"]; //가용/보류
                                    dr["ZEND_STOCK"] = dv[0]["ZEND_STOCK"];
                                    dr["ZBEGIN_INTRANSIT"] = dv[0]["ZBEGIN_INTRANSIT"]; //이동중
                                    dr["ZEND_INTRANSIT"] = dv[0]["ZEND_INTRANSIT"];
                                    dr["ZPROD_GR"] = dv[0]["ZPROD_GR"];
                                    dr["ZPROD_GI"] = dv[0]["ZPROD_GI"];
                                    dr["ZMOVE_IN"] = dv[0]["ZMOVE_IN"];
                                    dr["ZMOVE_OUT"] = dv[0]["ZMOVE_OUT"];
                                    dr["ZMOVE_IN_BLOCK"] = dv[0]["ZMOVE_IN_BLOCK"];
                                    dr["ZMOVE_OUT_BLOCK"] = dv[0]["ZMOVE_OUT_BLOCK"];
                                    dr["ZPUR_GR"] = dv[0]["ZPUR_GR"];
                                    dr["ZSALES_GI"] = dv[0]["ZSALES_GI"];
                                    dr["ZINTRANSIT_GR"] = dv[0]["ZINTRANSIT_GR"];
                                    dr["ZINTRANSIT_GI"] = dv[0]["ZINTRANSIT_GI"];
                                    dr["ZETC_IN"] = dv[0]["ZETC_IN"];
                                    dr["ZETC_OUT"] = dv[0]["ZETC_OUT"];
                                    dr["ZUNIDENTIFIED_QTY"] = dv[0]["ZUNIDENTIFIED_QTY"];
                                    dr["ERFME"] = dv[0]["ERFME"];

                                    if (chkMoving.IsChecked == true)
                                    {
                                        dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                        dr["MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        dr["SUM_QTY2_ERP"] = dv[0]["ZBEGIN_STOCK"];
                                        dr["MOVING_LOT_QTY2_ERP"] = dv[0]["ZBEGIN_INTRANSIT"];
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dv[0]["ZBEGIN_STOCK"]);
                                        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dv[0]["ZBEGIN_INTRANSIT"]);
                                    }
                                    else
                                    {
                                        dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        dr["SUM_QTY2_ERP"] = Convert.ToDouble(dv[0]["ZBEGIN_STOCK"]) + Convert.ToDouble(dv[0]["ZBEGIN_INTRANSIT"]);
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                    }
                                }
                            }
                            #endregion

                            #region Add Rows from ERP
                            foreach (DataRow dr in (dtStockList as DataTable).Rows)
                            {
                                DataView dv = dt.AsEnumerable().Where(x => x.Field<string>("SHOPID") == dr["WERKS"].ToString() && x.Field<string>("SLOC_ID") == dr["LGORT"].ToString() && x.Field<string>("PRODID") == dr["MATNR"].ToString() && x.Field<string>("SUM_DATE") == dr["ZDAY"].ToString()).AsDataView();

                                if (dv.Count.Equals(0))
                                {
                                    DataRow newRow = dt.NewRow();
                                    newRow["SHOPID"] = dr["WERKS"];
                                    newRow["SLOC_ID"] = dr["LGORT"];
                                    newRow["PRODID"] = dr["MATNR"];
                                    newRow["SUM_DATE"] = dr["ZDAY"];

                                    newRow["SUM_QTY2"] = 0; //가용/보류
                                    newRow["MOVING_LOT_QTY2"] = 0; //이동 중
                                    newRow["ZBEGIN_STOCK"] = dr["ZBEGIN_STOCK"]; //가용/보류
                                    newRow["ZEND_STOCK"] = dr["ZEND_STOCK"];
                                    newRow["ZBEGIN_INTRANSIT"] = dr["ZBEGIN_INTRANSIT"]; //이동중
                                    newRow["ZEND_INTRANSIT"] = dr["ZEND_INTRANSIT"];
                                    newRow["ZPROD_GR"] = dr["ZPROD_GR"];
                                    newRow["ZPROD_GI"] = dr["ZPROD_GI"];
                                    newRow["ZMOVE_IN"] = dr["ZMOVE_IN"];
                                    newRow["ZMOVE_OUT"] = dr["ZMOVE_OUT"];
                                    newRow["ZMOVE_IN_BLOCK"] = dr["ZMOVE_IN_BLOCK"];
                                    newRow["ZMOVE_OUT_BLOCK"] = dr["ZMOVE_OUT_BLOCK"];
                                    newRow["ZPUR_GR"] = dr["ZPUR_GR"];
                                    newRow["ZSALES_GI"] = dr["ZSALES_GI"];
                                    newRow["ZINTRANSIT_GR"] = dr["ZINTRANSIT_GR"];
                                    newRow["ZINTRANSIT_GI"] = dr["ZINTRANSIT_GI"];
                                    newRow["ZETC_IN"] = dr["ZETC_IN"];
                                    newRow["ZETC_OUT"] = dr["ZETC_OUT"];
                                    newRow["ZUNIDENTIFIED_QTY"] = dr["ZUNIDENTIFIED_QTY"];
                                    newRow["ERFME"] = dr["ERFME"];

                                    if (chkMoving.IsChecked == true)
                                    {
                                        newRow["SUM_QTY2_ERP"] = dr["ZBEGIN_STOCK"];
                                        newRow["MOVING_LOT_QTY2_ERP"] = dr["ZBEGIN_INTRANSIT"];
                                        newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(newRow["SUM_QTY2_ERP"]);
                                        newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(newRow["MOVING_LOT_QTY2_ERP"]);
                                    }
                                    else
                                    {
                                        newRow["SUM_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                        newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(newRow["SUM_QTY2_ERP"]);
                                    }

                                    dt.Rows.Add(newRow);
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex) { }

                        #region Like 검색 데이터 삭제.
                        try
                        {
                            dt.Select("PRODID <> '" + Util.NVC(txtProdID.Text.Trim()) + "' ").ToList<DataRow>().ForEach(row => row.Delete());
                            dt.AcceptChanges();
                        }
                        catch (Exception ex7) { }
                        #endregion

                        #region OrderBy
                        dt.AsEnumerable().OrderBy(x => x.Field<string>("PRODID") + x.Field<string>("SLOC_ID") + x.Field<string>("SUM_DATE"));
                        #endregion OrderBy

                        #region ItemsSource Bind
                        Util.GridSetData(dgList, dt, FrameOperation);
                        dgList.SortBy(dgList.Columns["SUM_DATE"]);
                        #endregion 

                        #region 이동중 컬럼 View 처리
                        if (chkMoving.IsChecked == true)
                        {
                            if (dgList.Columns.Contains("MOVING_LOT_QTY2"))
                                dgList.Columns["MOVING_LOT_QTY2"].Visibility = Visibility.Visible;
                            if (dgList.Columns.Contains("MOVING_LOT_QTY2_ERP"))
                                dgList.Columns["MOVING_LOT_QTY2_ERP"].Visibility = Visibility.Visible;
                            if (dgList.Columns.Contains("GAP_MOVING_LOT_QTY2"))
                                dgList.Columns["GAP_MOVING_LOT_QTY2"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            if (dgList.Columns.Contains("MOVING_LOT_QTY2"))
                                dgList.Columns["MOVING_LOT_QTY2"].Visibility = Visibility.Collapsed;
                            if (dgList.Columns.Contains("MOVING_LOT_QTY2_ERP"))
                                dgList.Columns["MOVING_LOT_QTY2_ERP"].Visibility = Visibility.Collapsed;
                            if (dgList.Columns.Contains("GAP_MOVING_LOT_QTY2"))
                                dgList.Columns["GAP_MOVING_LOT_QTY2"].Visibility = Visibility.Collapsed;
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        public void GetCaldate()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = string.IsNullOrWhiteSpace(_AREAID) ? null : _AREAID;
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    _CalDate = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    //dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = _CalDate;
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
