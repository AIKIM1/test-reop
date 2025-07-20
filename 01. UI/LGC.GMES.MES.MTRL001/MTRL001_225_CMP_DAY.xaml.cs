/*************************************************************************************
 Created Date : 2025-04-07
      Creator : 이홍주 
   Decription : 일별 재고 비교 (CIMS-ERP) 팝업 (COM001_046_CMP_DAY) 복사
--------------------------------------------------------------------------------------
 [Change History]
  2025.04.07  이홍주  : Initial Created.
  
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

namespace LGC.GMES.MES.MTRL001
{
    /// <summary>
    /// MTRL001_077_CMP_DAY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MTRL001_225_CMP_DAY : C1Window, IWorkArea
    {
        private string _SHOPID = "";
        private string _AREAID = "";
        private string _LOCATIONID = "";
        private string _TYPE = "";
        private string _YYMM = "";

        private DateTime _CalDate;

        RfcDestination dest;

        // 2024.05.13 NERP 적용 여부 FLAG 
        private bool IS_NERP_FLAG = false;

        public MTRL001_225_CMP_DAY()
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
                // NERP 적용 판단 
                if (IsNerpApplyFlag().Equals("Y")) { IS_NERP_FLAG = true; }
                

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
                else if((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }

        private void InitSapInfo()
        {
            NERPDestinationConfig cfg = new NERPDestinationConfig();

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }
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
                // 2024.09.05 검색시작일이 검색종료일 1달전 같은날 : 없으면 말일
                DateTime dtSameDayPrevMonth = dtpDateTo.SelectedDateTime.AddMonths(-1);

                int dtCompare = DateTime.Compare(dtSameDayPrevMonth, dtpDateFrom.SelectedDateTime);
                if (dtCompare > 0)
                {
                    //SFU5033 : 기간은 %1달 이내 입니다.
                    Util.MessageValidation("SFU5033", "1");
                    //dtpDateFrom.SelectedDateTime = dtSameDayPrevMonth;
                    return;
                }

                


                //if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 60)
                //{
                //    //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "60");
                //    return;
                //}

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

                // 2024.08.29: 제품코드 LIKE 검색 => 동일제품만 검색 수정 
                string bizName = "DA_PRD_SEL_STOCK_SUMMARY2";
                if (IS_NERP_FLAG == true)
                {
                    bizName = "DA_PRD_SEL_STOCK_SUMMARY3";
                    //IndataTable.Columns.Add("NERP_APPLY_FLAG", typeof(string));
                }


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

                //if (IS_NERP_FLAG == true)
                //{
                //    Indata["NERP_APPLY_FLAG"] = "Y";
                //}
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService(bizName, "INDATA", "RSLTDT", IndataTable, (dt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        #region Column Add // 2024.08.01 주석 처리 
                        //if (!dt.Columns.Contains("ZBEGIN_STOCK"))
                        //    dt.Columns.Add("ZBEGIN_STOCK", typeof(decimal));
                        //if (!dt.Columns.Contains("ZEND_STOCK"))
                        //    dt.Columns.Add("ZEND_STOCK", typeof(decimal));
                        //if (!dt.Columns.Contains("ZBEGIN_INTRANSIT"))
                        //    dt.Columns.Add("ZBEGIN_INTRANSIT", typeof(decimal));
                        //if (!dt.Columns.Contains("ZEND_INTRANSIT"))
                        //    dt.Columns.Add("ZEND_INTRANSIT", typeof(decimal));
                        //if (!dt.Columns.Contains("ZPROD_GR"))
                        //    dt.Columns.Add("ZPROD_GR", typeof(decimal));
                        //if (!dt.Columns.Contains("ZPROD_GI"))
                        //    dt.Columns.Add("ZPROD_GI", typeof(decimal));
                        //if (!dt.Columns.Contains("ZMOVE_IN"))
                        //    dt.Columns.Add("ZMOVE_IN", typeof(decimal));
                        //if (!dt.Columns.Contains("ZMOVE_OUT"))
                        //    dt.Columns.Add("ZMOVE_OUT", typeof(decimal));
                        //if (!dt.Columns.Contains("ZMOVE_IN_BLOCK"))
                        //    dt.Columns.Add("ZMOVE_IN_BLOCK", typeof(decimal));
                        //if (!dt.Columns.Contains("ZMOVE_OUT_BLOCK"))
                        //    dt.Columns.Add("ZMOVE_OUT_BLOCK", typeof(decimal));
                        //if (!dt.Columns.Contains("ZPUR_GR"))
                        //    dt.Columns.Add("ZPUR_GR", typeof(decimal));
                        //if (!dt.Columns.Contains("ZSALES_GI"))
                        //    dt.Columns.Add("ZSALES_GI", typeof(decimal));
                        //if (!dt.Columns.Contains("ZINTRANSIT_GR"))
                        //    dt.Columns.Add("ZINTRANSIT_GR", typeof(decimal));
                        //if (!dt.Columns.Contains("ZINTRANSIT_GI"))
                        //    dt.Columns.Add("ZINTRANSIT_GI", typeof(decimal));
                        //if (!dt.Columns.Contains("ZETC_IN"))
                        //    dt.Columns.Add("ZETC_IN", typeof(decimal));
                        //if (!dt.Columns.Contains("ZETC_OUT"))
                        //    dt.Columns.Add("ZETC_OUT", typeof(decimal));
                        //if (!dt.Columns.Contains("ZUNIDENTIFIED_QTY"))
                        //    dt.Columns.Add("ZUNIDENTIFIED_QTY", typeof(decimal));
                        //if (!dt.Columns.Contains("ERFME"))
                        //    dt.Columns.Add("ERFME", typeof(string));
                        //if (!dt.Columns.Contains("SUM_QTY2_ERP"))
                        //    dt.Columns.Add("SUM_QTY2_ERP", typeof(decimal));
                        //if (!dt.Columns.Contains("MOVING_LOT_QTY2_ERP"))
                        //    dt.Columns.Add("MOVING_LOT_QTY2_ERP", typeof(decimal));
                        #endregion

                        DataTable dtNerp = new DataTable(); // NERP 데이터를 일자별로 합산 
                        try
                        {
                            #region Get ERP Stock List
                            if (IS_NERP_FLAG == true ) // NERP 적용시, (운영, 해외 세팅 추가 필요함) 
                            {
                                dest = RfcDestinationManager.GetDestination("ESQ"); // ESD --> ESQ
                            }
                            else
                            {
                                // app.debug.config (APP_SERVER): 개발에만 존재하는 듯 
                                //dest = RfcDestinationManager.GetDestination(ConfigurationManager.AppSettings["APP_SERVER"].Contains("DEV") ? "QAS" : "PRD");
                                dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "PRD" : "QAS");
                            }
                            RfcRepository repo = dest.Repository;

                            // NERP 적용 여부 분기
                            string rfcName = "ZPPB_ERP_BEGIN_END_STOCK";
                            string sapTable = "IT_STOCK_GRGI";


                            if (IS_NERP_FLAG == true)
                            {
                                rfcName = "ZPPNV_RFC_ERP_BEGIN_END_STOCK";
                                sapTable = "OT_STOCK_GRGI";
                            }

                            IRfcFunction FnStockList = repo.CreateFunction(rfcName); // 일재고만 조회 (현, 기말 재고 x) 

                            if (IS_NERP_FLAG == true)
                            {
                                FnStockList.SetValue("IV_DAY_FROM", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                                if (txtProdID.Text != "")
                                    FnStockList.SetValue("IV_DAY_TO", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                                FnStockList.SetValue("IV_WERKS", _SHOPID);                                               //Plant         *필수
                                FnStockList.SetValue("IV_LGORT", _LOCATIONID);                                           //저장위치      *필수

                                FnStockList.SetValue("IV_MATNR", txtProdID.Text);                                        //제품ID
                                FnStockList.SetValue("IV_FLAG", "X");
                            }
                            else
                            {
                                FnStockList.SetValue("I_DAY_FROM", dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                                if (txtProdID.Text != "")
                                    FnStockList.SetValue("I_DAY_TO", dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                                FnStockList.SetValue("I_WERKS", _SHOPID);                                               //Plant         *필수
                                FnStockList.SetValue("I_LGORT", _LOCATIONID);                                           //저장위치      *필수

                                FnStockList.SetValue("I_MATNR", txtProdID.Text);                                        //제품ID
                                FnStockList.SetValue("I_FLAG", "X");                                                    //수불없는 날짜 포함 (X or Blank) zero 데이터 포함 
                            }

                            

                            FnStockList.Invoke(dest);

                            var stockList = FnStockList.GetTable(sapTable);
                            var dtStockList = stockList.ToDataTable(sapTable);
                            #endregion

                            #region ERP data to DataTable (일자별 그룹핑) 
                            // SHOP, 창고, 제품 , 일자 그룹핑 
                            var query = dtStockList.AsEnumerable()
                            .GroupBy(g => new { MATNR = g.Field<string>("MATNR"), LGORT = g.Field<string>("LGORT"), ZDAY = g.Field<string>("ZDAY") })
                            .Select(x => new
                            {
                                ZDAY = x.Key.ZDAY,
                                MATNR = x.Key.MATNR,
                                LGORT = x.Key.LGORT,
                                ZBEGIN_STOCK = x.Sum(y => y.Field<decimal>("ZBEGIN_STOCK")), // 가용 (기초 재고)
                                ZEND_STOCK = x.Sum(y => y.Field<decimal>("ZEND_STOCK")), // 기말 재고
                                ZBEGIN_INTRANSIT = x.Sum(y => y.Field<decimal>("ZBEGIN_INTRANSIT")),   //이동 중 (운송중 기초 재고)
                                ZEND_INTRANSIT = x.Sum(y => y.Field<decimal>("ZEND_INTRANSIT")), // 운송 중 기말 재고
                                ZPROD_GR = x.Sum(y => y.Field<decimal>("ZPROD_GR")), // 생산 입고 
                                ZPROD_GI = x.Sum(y => y.Field<decimal>("ZPROD_GI")), // 생산 출고 
                                ZMOVE_IN = x.Sum(y => y.Field<decimal>("ZMOVE_IN")), // 이전 입고
                                ZMOVE_OUT = x.Sum(y => y.Field<decimal>("ZMOVE_OUT")), // 이전 출고
                                ZMOVE_IN_BLOCK = x.Sum(y => y.Field<decimal>("ZMOVE_IN_BLOCK")), // 이전 입고 (불량)
                                ZMOVE_OUT_BLOCK = x.Sum(y => y.Field<decimal>("ZMOVE_OUT_BLOCK")), // 이전 출고 (불량)
                                ZPUR_GR = x.Sum(y => y.Field<decimal>("ZPUR_GR")), // 구매 입고
                                ZSALES_GI = x.Sum(y => y.Field<decimal>("ZSALES_GI")), // 판매 출고
                                ZINTRANSIT_GR = x.Sum(y => y.Field<decimal>("ZINTRANSIT_GR")), // 운송중 입고
                                ZINTRANSIT_GI = x.Sum(y => y.Field<decimal>("ZINTRANSIT_GI")), // 운송중 출고
                                ZETC_IN = x.Sum(y => y.Field<decimal>("ZETC_IN")), // 기타 입고
                                ZETC_OUT = x.Sum(y => y.Field<decimal>("ZETC_OUT")), // 기타 출고
                                ZUNIDENTIFIED_QTY = x.Sum(y => y.Field<decimal>("ZUNIDENTIFIED_QTY")), // 미확인 수량 
                                ERFME = x.Select(y => y.Field<string>("ERFME")).First() // Unit of entry
                            })
                            .OrderBy(o => o.ZDAY);

                            //DataTable dtNerp = new DataTable(); // NERP 데이터를 일자별로 합산 
                            // MES 데이터 컬럼 

                            dtNerp.Columns.Add("SHOPID", Type.GetType("System.String"));
                            dtNerp.Columns.Add("GAP_SUM_QTY2", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("GAP_MOVING_LOT_QTY2", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("SUM_QTY2", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("MOVING_LOT_QTY2", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("SUM_QTY2_ERP", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("MOVING_LOT_QTY2_ERP", Type.GetType("System.Decimal"));


                            // ERP 데이터 컬럼 
                            dtNerp.Columns.Add("SUM_DATE", Type.GetType("System.String"));  // ZDAY --> SUM_DATE 
                            dtNerp.Columns.Add("PRODID", Type.GetType("System.String")); // MATNR --> PRODID
                            dtNerp.Columns.Add("SLOC_ID", Type.GetType("System.String")); // LGORT --> SLOC_ID 

                            dtNerp.Columns.Add("ZBEGIN_STOCK", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZEND_STOCK", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZBEGIN_INTRANSIT", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZEND_INTRANSIT", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZPROD_GR", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZPROD_GI", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZMOVE_IN", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZMOVE_OUT", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZMOVE_IN_BLOCK", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZMOVE_OUT_BLOCK", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZPUR_GR", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZSALES_GI", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZINTRANSIT_GR", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZINTRANSIT_GI", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZETC_IN", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZETC_OUT", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ZUNIDENTIFIED_QTY", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ERFME", Type.GetType("System.String"));

                            foreach (var item in query)
                            {
                                dtNerp.Rows.Add(
                                    _SHOPID, 0, 0, 0, 0, 0, 0, // MES 컬럼 
                                    item.ZDAY, item.MATNR, item.LGORT, item.ZBEGIN_STOCK, item.ZEND_STOCK, // ERP 컬럼 
                                    item.ZBEGIN_INTRANSIT, item.ZEND_INTRANSIT, item.ZPROD_GR, item.ZPROD_GI,
                                    item.ZMOVE_IN, item.ZMOVE_OUT, item.ZMOVE_IN_BLOCK, item.ZMOVE_OUT_BLOCK,
                                    item.ZPUR_GR, item.ZSALES_GI, item.ZINTRANSIT_GR, item.ZINTRANSIT_GI,
                                    item.ZETC_IN, item.ZETC_OUT, item.ZUNIDENTIFIED_QTY, item.ERFME);
                            }

                            #endregion

                            #region Merge Based on ERP Data

                            // MES DATA --> GROUPING (일자 중복 대비 ) 
                            var groupQuery = dt.AsEnumerable()
                            .GroupBy(g => new { SHOPID = g.Field<string>("SHOPID"), PRODID = g.Field<string>("PRODID"), SLOC_ID = g.Field<string>("SLOC_ID"), SUM_DATE = g.Field<string>("SUM_DATE") })
                            .Select(x => new
                            {
                                SHOPID = x.Key.SHOPID,
                                PRODID = x.Key.PRODID,
                                SLOC_ID = x.Key.SLOC_ID,
                                SUM_DATE = x.Key.SUM_DATE,
                                SUM_QTY2 = x.Sum(y => y.Field<decimal>("SUM_QTY2")),
                                MOVING_LOT_QTY2 = x.Sum(y => y.Field<decimal>("MOVING_LOT_QTY2")),
                                SUM_QTY2_ERP = x.Sum(y => y.Field<decimal>("SUM_QTY2_ERP")),
                                MOVING_LOT_QTY2_ERP = x.Sum(y => y.Field<decimal>("MOVING_LOT_QTY2_ERP")),
                                GAP_SUM_QTY2 = x.Sum(y => y.Field<decimal>("GAP_SUM_QTY2")),
                                GAP_MOVING_LOT_QTY2 = x.Sum(y => y.Field<decimal>("GAP_MOVING_LOT_QTY2"))
                            });

                            foreach (var item in groupQuery) // item : MES 데이터 
                            {
                                // 일자에 맞는 ERP DataRow 값 변경 
                                DataRow dr = dtNerp.Select($"SUM_DATE = '{item.SUM_DATE}'").FirstOrDefault();
                                if (dr != null)
                                {
                                    if (chkMoving.IsChecked == true)
                                    {
                                        dr["SUM_QTY2"] = Convert.ToDouble(item.SUM_QTY2);
                                        dr["MOVING_LOT_QTY2"] = Convert.ToDouble(item.MOVING_LOT_QTY2);
                                        dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]);
                                        dr["MOVING_LOT_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(item.SUM_QTY2) - Convert.ToDouble(dr["ZBEGIN_STOCK"]);
                                        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(item.MOVING_LOT_QTY2) - Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                    }
                                    else
                                    {
                                        dr["SUM_QTY2"] = Convert.ToDouble(item.SUM_QTY2) + Convert.ToDouble(item.MOVING_LOT_QTY2);
                                        dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(item.SUM_QTY2) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                    }
                                }

                                
                            }
                            #endregion

                            #region ERP 데이터만 존재하는 경우 
                            string seperator = ",";
                            string existMesDates = String.Join(seperator, groupQuery.Select(g => "'" + g.SUM_DATE + "'").ToList()); // '20240724', '20240728' 

                            foreach (DataRow dr in dtNerp.Select($"SUM_DATE NOT IN ({existMesDates})"))
                            {
                                if (chkMoving.IsChecked == true)
                                {
                                    //dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                    //dr["MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                    dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["ZBEGIN_STOCK"]);
                                    dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                }
                                else
                                {
                                    //dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                    dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["ZBEGIN_STOCK"]);
                                }

                            }

                            #endregion


                            #region Merge Based on GMES // 2024.08.01 주석 처리 
                            //foreach (DataRow dr in dt.Rows)
                            //{
                            //    DataView dv = dtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString() && x.Field<string>("ZDAY") == dr["SUM_DATE"].ToString()).AsDataView();

                            //    if (dv.Count.Equals(0))
                            //    {                                                                
                            //        dr["ZBEGIN_STOCK"] = 0;
                            //        dr["ZEND_STOCK"] = 0;
                            //        dr["ZBEGIN_INTRANSIT"] = 0;
                            //        dr["ZEND_INTRANSIT"] = 0;
                            //        dr["ZPROD_GR"] = 0;
                            //        dr["ZPROD_GI"] = 0;
                            //        dr["ZMOVE_IN"] = 0;
                            //        dr["ZMOVE_OUT"] = 0;
                            //        dr["ZMOVE_IN_BLOCK"] = 0;
                            //        dr["ZMOVE_OUT_BLOCK"] = 0;
                            //        dr["ZPUR_GR"] = 0;
                            //        dr["ZSALES_GI"] = 0;
                            //        dr["ZINTRANSIT_GR"] = 0;
                            //        dr["ZINTRANSIT_GI"] = 0;
                            //        dr["ZETC_IN"] = 0;
                            //        dr["ZETC_OUT"] = 0;
                            //        dr["ZUNIDENTIFIED_QTY"] = 0;
                            //        dr["ERFME"] = "";

                            //        dr["SUM_QTY2_ERP"] = 0;
                            //        dr["MOVING_LOT_QTY2_ERP"] = 0;

                            //        if (chkMoving.IsChecked == true)
                            //        {
                            //            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                            //            dr["MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);                                        
                            //            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                            //            dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);


                            //        }
                            //        else
                            //        {
                            //            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                            //            dr["GAP_SUM_QTY2"] = dr["SUM_QTY2"];


                            //        }
                            //    }
                            //    else if (dv.Count > 0)
                            //    {
                            //        // 2024.05.13  LOT (PLOTNO) 별로 합산 
                            //        if (IS_NERP_FLAG == true )
                            //        {
                            //            var query = dtStockList.AsEnumerable()
                            //                .Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString()
                            //                && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString()
                            //                && x.Field<string>("MATNR") == dr["PRODID"].ToString()
                            //                && x.Field<string>("ZDAY") == dr["SUM_DATE"].ToString())
                            //                .GroupBy(g => new { MATNR = g.Field<string>("MATNR"), LGORT = g.Field<string>("LGORT") })
                            //                .Select(x => new
                            //                {
                            //                    MATNR = x.Key.MATNR,
                            //                    LGORT = x.Key.LGORT,
                            //                    ZBEGIN_STOCK = x.Sum(y => y.Field<decimal>("ZBEGIN_STOCK")), // 가용 (기초 재고)
                            //                    ZEND_STOCK = x.Sum(y => y.Field<decimal>("ZEND_STOCK")), // 기말 재고
                            //                    ZBEGIN_INTRANSIT = x.Sum(y => y.Field<decimal>("ZBEGIN_INTRANSIT")),   //이동 중 (운송중 기초 재고)
                            //                    ZEND_INTRANSIT = x.Sum(y => y.Field<decimal>("ZEND_INTRANSIT")), // 운송 중 기말 재고
                            //                    ZPROD_GR = x.Sum(y => y.Field<decimal>("ZPROD_GR")), // 생산 입고 
                            //                    ZPROD_GI = x.Sum(y => y.Field<decimal>("ZPROD_GI")), // 생산 출고 
                            //                    ZMOVE_IN = x.Sum(y => y.Field<decimal>("ZMOVE_IN")), // 이전 입고
                            //                    ZMOVE_OUT = x.Sum(y => y.Field<decimal>("ZMOVE_OUT")), // 이전 출고
                            //                    ZMOVE_IN_BLOCK = x.Sum(y => y.Field<decimal>("ZMOVE_IN_BLOCK")), // 이전 입고 (불량)
                            //                    ZMOVE_OUT_BLOCK = x.Sum(y => y.Field<decimal>("ZMOVE_OUT_BLOCK")), // 이전 출고 (불량)
                            //                    ZPUR_GR = x.Sum(y => y.Field<decimal>("ZPUR_GR")), // 구매 입고
                            //                    ZSALES_GI = x.Sum(y => y.Field<decimal>("ZSALES_GI")), // 판매 출고
                            //                    ZINTRANSIT_GR = x.Sum(y => y.Field<decimal>("ZINTRANSIT_GR")), // 운송중 입고
                            //                    ZINTRANSIT_GI = x.Sum(y => y.Field<decimal>("ZINTRANSIT_GI")), // 운송중 출고
                            //                    ZETC_IN = x.Sum(y => y.Field<decimal>("ZETC_IN")), // 기타 입고
                            //                    ZETC_OUT = x.Sum(y => y.Field<decimal>("ZETC_OUT")), // 기타 출고
                            //                    ZUNIDENTIFIED_QTY = x.Sum(y => y.Field<decimal>("ZUNIDENTIFIED_QTY")), // 미확인 수량 
                            //                    ERFME = x.Select(y => y.Field<string>("ERFME")).First() // Unit of entry
                            //                });

                            //            DataTable dtNerp = new DataTable();
                            //            dtNerp.Columns.Add("MATNR");
                            //            dtNerp.Columns.Add("LGORT");
                            //            dtNerp.Columns.Add("ZBEGIN_STOCK", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZEND_STOCK", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZBEGIN_INTRANSIT", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZEND_INTRANSIT", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZPROD_GR", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZPROD_GI", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZMOVE_IN", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZMOVE_OUT", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZMOVE_IN_BLOCK", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZMOVE_OUT_BLOCK", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZPUR_GR", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZSALES_GI", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZINTRANSIT_GR", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZINTRANSIT_GI", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZETC_IN", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZETC_OUT", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ZUNIDENTIFIED_QTY", Type.GetType("System.Decimal"));
                            //            dtNerp.Columns.Add("ERFME", Type.GetType("System.String"));

                            //            foreach (var item in query)
                            //            {
                            //                dtNerp.Rows.Add(item.MATNR, item.LGORT, item.ZBEGIN_STOCK, item.ZEND_STOCK,
                            //                    item.ZBEGIN_INTRANSIT, item.ZEND_INTRANSIT, item.ZPROD_GR, item.ZPROD_GI,
                            //                    item.ZMOVE_IN, item.ZMOVE_OUT, item.ZMOVE_IN_BLOCK, item.ZMOVE_OUT_BLOCK,
                            //                    item.ZPUR_GR, item.ZSALES_GI, item.ZINTRANSIT_GR, item.ZINTRANSIT_GI,
                            //                    item.ZETC_IN, item.ZETC_OUT, item.ZUNIDENTIFIED_QTY, item.ERFME);
                            //            }

                            //            dv = dtNerp.DefaultView;
                            //        }

                            //        dr["ZBEGIN_STOCK"] = dv[0]["ZBEGIN_STOCK"]; //가용/보류
                            //        dr["ZEND_STOCK"] = dv[0]["ZEND_STOCK"];
                            //        dr["ZBEGIN_INTRANSIT"] = dv[0]["ZBEGIN_INTRANSIT"]; //이동중
                            //        dr["ZEND_INTRANSIT"] = dv[0]["ZEND_INTRANSIT"];
                            //        dr["ZPROD_GR"] = dv[0]["ZPROD_GR"];
                            //        dr["ZPROD_GI"] = dv[0]["ZPROD_GI"];
                            //        dr["ZMOVE_IN"] = dv[0]["ZMOVE_IN"];
                            //        dr["ZMOVE_OUT"] = dv[0]["ZMOVE_OUT"];
                            //        dr["ZMOVE_IN_BLOCK"] = dv[0]["ZMOVE_IN_BLOCK"];
                            //        dr["ZMOVE_OUT_BLOCK"] = dv[0]["ZMOVE_OUT_BLOCK"];
                            //        dr["ZPUR_GR"] = dv[0]["ZPUR_GR"];
                            //        dr["ZSALES_GI"] = dv[0]["ZSALES_GI"];
                            //        dr["ZINTRANSIT_GR"] = dv[0]["ZINTRANSIT_GR"];
                            //        dr["ZINTRANSIT_GI"] = dv[0]["ZINTRANSIT_GI"];
                            //        dr["ZETC_IN"] = dv[0]["ZETC_IN"];
                            //        dr["ZETC_OUT"] = dv[0]["ZETC_OUT"];
                            //        dr["ZUNIDENTIFIED_QTY"] = dv[0]["ZUNIDENTIFIED_QTY"];
                            //        dr["ERFME"] = dv[0]["ERFME"];

                            //        if (chkMoving.IsChecked == true)
                            //        {
                            //            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                            //            dr["MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                            //            dr["SUM_QTY2_ERP"] = dv[0]["ZBEGIN_STOCK"];
                            //            dr["MOVING_LOT_QTY2_ERP"] = dv[0]["ZBEGIN_INTRANSIT"];
                            //            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dv[0]["ZBEGIN_STOCK"]);
                            //            dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dv[0]["ZBEGIN_INTRANSIT"]);
                            //        }
                            //        else
                            //        {
                            //            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                            //            dr["SUM_QTY2_ERP"] = Convert.ToDouble(dv[0]["ZBEGIN_STOCK"]) + Convert.ToDouble(dv[0]["ZBEGIN_INTRANSIT"]);
                            //            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                            //        }
                            //    }
                            //}
                            #endregion

                            #region Add Rows from ERP // 2024.08.01 주석 처리 
                            //foreach (DataRow dr in (dtStockList as DataTable).Rows)
                            //{
                            //    DataView dv = dt.AsEnumerable().Where(x => x.Field<string>("SHOPID") == dr["WERKS"].ToString() && x.Field<string>("SLOC_ID") == dr["LGORT"].ToString() && x.Field<string>("PRODID") == dr["MATNR"].ToString() && x.Field<string>("SUM_DATE") == dr["ZDAY"].ToString()).AsDataView();

                            //    if (dv.Count.Equals(0))
                            //    {
                            //        DataRow newRow = dt.NewRow();
                            //        newRow["SHOPID"] = dr["WERKS"];
                            //        newRow["SLOC_ID"] = dr["LGORT"];
                            //        newRow["PRODID"] = dr["MATNR"];
                            //        newRow["SUM_DATE"] = dr["ZDAY"];

                            //        newRow["SUM_QTY2"] = 0; //가용/보류
                            //        newRow["MOVING_LOT_QTY2"] = 0; //이동 중
                            //        newRow["ZBEGIN_STOCK"] = dr["ZBEGIN_STOCK"]; //가용/보류
                            //        newRow["ZEND_STOCK"] = dr["ZEND_STOCK"];
                            //        newRow["ZBEGIN_INTRANSIT"] = dr["ZBEGIN_INTRANSIT"]; //이동중
                            //        newRow["ZEND_INTRANSIT"] = dr["ZEND_INTRANSIT"];
                            //        newRow["ZPROD_GR"] = dr["ZPROD_GR"];
                            //        newRow["ZPROD_GI"] = dr["ZPROD_GI"];
                            //        newRow["ZMOVE_IN"] = dr["ZMOVE_IN"];
                            //        newRow["ZMOVE_OUT"] = dr["ZMOVE_OUT"];
                            //        newRow["ZMOVE_IN_BLOCK"] = dr["ZMOVE_IN_BLOCK"];
                            //        newRow["ZMOVE_OUT_BLOCK"] = dr["ZMOVE_OUT_BLOCK"];
                            //        newRow["ZPUR_GR"] = dr["ZPUR_GR"];
                            //        newRow["ZSALES_GI"] = dr["ZSALES_GI"];
                            //        newRow["ZINTRANSIT_GR"] = dr["ZINTRANSIT_GR"];
                            //        newRow["ZINTRANSIT_GI"] = dr["ZINTRANSIT_GI"];
                            //        newRow["ZETC_IN"] = dr["ZETC_IN"];
                            //        newRow["ZETC_OUT"] = dr["ZETC_OUT"];
                            //        newRow["ZUNIDENTIFIED_QTY"] = dr["ZUNIDENTIFIED_QTY"];
                            //        newRow["ERFME"] = dr["ERFME"];

                            //        if (chkMoving.IsChecked == true)
                            //        {
                            //            newRow["SUM_QTY2_ERP"] = dr["ZBEGIN_STOCK"];
                            //            newRow["MOVING_LOT_QTY2_ERP"] = dr["ZBEGIN_INTRANSIT"];
                            //            newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(newRow["SUM_QTY2_ERP"]);
                            //            newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(newRow["MOVING_LOT_QTY2_ERP"]);
                            //        }
                            //        else
                            //        {
                            //            newRow["SUM_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                            //            newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(newRow["SUM_QTY2_ERP"]);
                            //        }

                            //        dt.Rows.Add(newRow);
                            //    }
                            //}
                            #endregion
                        }
                        catch (Exception ex) { }

                        #region Like 검색 데이터 삭제. // 2024.08.01 주석 처리 
                        //try
                        //{
                        //    dt.Select("PRODID <> '" + Util.NVC(txtProdID.Text.Trim()) + "' ").ToList<DataRow>().ForEach(row => row.Delete());
                        //    dt.AcceptChanges();
                        //}
                        //catch (Exception ex7) { }
                        #endregion

                        #region OrderBy // 2024.08.01 주석 처리 
                        //dt.AsEnumerable().OrderBy(x => x.Field<string>("PRODID") + x.Field<string>("SLOC_ID") + x.Field<string>("SUM_DATE"));
                        #endregion OrderBy

                        #region ItemsSource Bind // 2024.08.01: dt --> nertDt 변경 
                        //Util.GridSetData(dgList, dt, FrameOperation);
                        Util.GridSetData(dgList, dtNerp, FrameOperation);
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
        #region NERP APPLY FLAG 조회 
        // NERP 적용 확인  (2024.05.09: ytkim29), 두개의 DA expose 되어야 함 
        // DA_PRD_SEL_TB_MMD_SYSTEM_AREA: AREAID --> SYSTEM_ID
        // DA_BAS_SEL_NERP_APPLY_FLAG
        private string IsNerpApplyFlag()
        {
            string flag = "N";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(newRow);

            try
            {
                // GET SYSTEM_ID 
                DataTable dtNerpFlag = new ClientProxy().ExecuteServiceSync("BR_SEL_MMD_NERP_APPLY_FLAG", "RQSTDT", "RSLTDT", inTable);
                if (dtNerpFlag != null && dtNerpFlag.Rows.Count > 0)
                {
                    flag = dtNerpFlag.Rows[0]["NERP_APPLY_FLAG"].ToString();
                }
            }
            catch (Exception ex)
            {
                flag = "N";
            }
            return flag;
        }

        #endregion
    }
}
