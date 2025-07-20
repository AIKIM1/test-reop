/*************************************************************************************
 Created Date : 2024-05-02
      Creator : 김영택 (ytkim29)
   Decription : New ERP, GMES 간 연동을 통해 재고비교 화면 구현 
--------------------------------------------------------------------------------------
 [Change History]
   2024-05-02  김영택 : Initial Created.
   2024-08-28  김영택 : 월재고 (NERP) 적용 
   2024-09-02  김영택 : 메인 그리드 (summary3) 적용 (NERP)
   2024-09-19  김영택 : ESQ 연결 정보 변경 (310 --> 330)
   2024-10-02  김영택 : ESQ (310 적용), ESP 연결정보 수정, 제품ID 검색 수정 
   2024-10-16  김영택 : ESQ 연결 정보 변경 (330 open)
   2024.10.29  김영택 : MES 데이터 없는 경우 처리 추가 
   2024.11.04  김영택 : 세부항목조회시 MES 데이터 없음 예외처리 
   2024.11.20  김영택 : 실전 ESQ 330 --> ESP 110 연결 변경 
   2024.12.03  김영택 : 운영 ESP 연결 주소 설정 (ESP 100) 
   2024.12.18  김영택 : 실전 ESP 110 --> ESQ 330 변경 
   2024.12.28  김영택 : 운영 ESP 계정명 수정 
   2025.01.14  김영택 : TOTAL 클릭시 NERP 연결 추가 
   2025.02.05  김영택 : 저장위치 목록 수정 (MMD의 DB 유형 참고)
   2025.02.06  김영택 : 일재고 상세목록 재고 차이 수정, 재고목록 SUMMARY 적용  
   2025.02.18  김영택 : 상세목록 조회 성능 개선, ERP 수량 0 LOT 목록 제거 , DB TYPE 적용 (수정)
   2025.02.20  김영택 : 재고목록 조회시 ERP 만 존재하는 데이터 수정 (자재 제거)
                        DB_TYPE 참고 BIZ 이름 수정 _DBTYPE 
   2025.04.14  김영택 : RFC 테스트 (ESQ) CLIENT 330에서 820 변경 
**************************************************************************************/

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using SAP.Middleware.Connector;
using System.Linq;
using C1.WPF.DataGrid;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_403.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_403 : UserControl, IWorkArea
    {
        private string _DTL_SUM_TYPE = "";
        private string _DTL_SUM_YM = "";
        private string _DTL_SUM_DATE = "";
        private string _DTL_SHOPID = "";
        private string _DTL_AREAID = "";
        private string _DTL_PRODID = "";
        private string _DTL_SLOC_ID = "";
        private string _DTL_FINL_WIP_FLAG = "";
        private string _DTL_CBO_CLASS = "";
        private bool _DTL_CHK_GMES_CHK = false;

        // 2024.05.09 NERP 적용 여부 FLAG 
        private bool IS_NERP_FLAG = false;

        public IFrameOperation FrameOperation { get; set; }
        RfcDestination dest;

        public COM001_403()
        {
            InitializeComponent();

            SetComboBox(cboShop);
            SetComboBox(cboClass);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            // NERP 적용 판단 
            if (IsNerpApplyFlag().Equals("Y")) { IS_NERP_FLAG = true; }

            NERPDestinationConfig cfg = new NERPDestinationConfig();

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }

            if (IS_NERP_FLAG == true) // NERP 적용시, (운영, 해외 세팅 추가 필요함) 
            {
                chkNerpApply.Visibility = Visibility.Visible;
                chkNerpApply.IsChecked = true;
            }
            else
            {
                chkNerpApply.Visibility = Visibility.Hidden;
                chkNerpApply.IsChecked = false;
            }

            chkFinlwip.IsChecked = true;

            // 2021.04.01 CNB2동 활성화일때 Lot ID 캡션 PKG Lot ID 로변경
            if (LoginInfo.CFG_AREA_ID == "AA")
            {
                dgDetail.Columns["LOTID"].Header = ObjectDic.Instance.GetObjectName("PKG_LOT_ID");
            }

            chkAvlPndChk.Checked -= chkAvlPndChk_Checked;
            chkAvlPndChk.Checked += chkAvlPndChk_Checked;
            chkAvlPndChk.Unchecked -= chkAvlPndChk_Unchecked;
            chkAvlPndChk.Unchecked += chkAvlPndChk_Unchecked;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetStocks();
            }
            catch (Exception ex) { }
        }

        private void dgMaster_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv != null)
            {
                if (dgMaster.Columns["SUM_QTY2"].Visibility == Visibility.Visible && dgMaster.Columns["SUM_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["SUM_QTY2"]).Equals(Convert.ToDouble(drv["SUM_QTY2_ERP"])))
                    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                else if (dgMaster.Columns["HOLD_LOT_QTY2"].Visibility == Visibility.Visible && dgMaster.Columns["HOLD_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["HOLD_LOT_QTY2"]).Equals(Convert.ToDouble(drv["HOLD_LOT_QTY2_ERP"])))
                    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                else if (dgMaster.Columns["MOVING_LOT_QTY2"].Visibility == Visibility.Visible && dgMaster.Columns["MOVING_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["MOVING_LOT_QTY2"]).Equals(Convert.ToDouble(drv["MOVING_LOT_QTY2_ERP"])))
                    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                else
                    e.Row.Presenter.Background = new SolidColorBrush(Colors.White);
            }
        }

        void SetComboBox(C1ComboBox cbo)
        {
            switch (cbo.Name)
            {
                case "cboShop":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_AUTH_SHOP_CBO", cbo, new string[] { "SYSTEM_ID", "LANGID", "USERID", "USE_FLAG" }, new string[] { LoginInfo.SYSID, LoginInfo.LANGID, LoginInfo.USERID, "Y" }, CommonCombo.ComboStatus.NONE, "CBO_CODE", "CBO_NAME");
                    break;

                case "cboStockLocation":
                    if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("ALL"))
                        CommonCombo.CommonBaseCombo("DA_BAS_SEL_SLOC_BY_USER_PERMISSION", cbo, new string[] { "SHOPID", "AREAID", "SYSTEM_ID", "USERID" }, new string[] { cboShop.SelectedValue as string, null, LoginInfo.SYSID, LoginInfo.USERID }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    else
                        CommonCombo.CommonBaseCombo("DA_BAS_SEL_SLOC_BY_SHOP", cbo, new string[] { "SHOPID", "AREAID" }, new string[] { cboShop.SelectedValue as string, cboArea.SelectedValue as string }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;

                case "cboArea":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_AUTH_AREA_CBO", cbo, new string[] { "LANGID", "SHOPID", "USERID", "SYSTEM_ID" }, new string[] { LoginInfo.LANGID, cboShop.SelectedValue as string, LoginInfo.USERID, LoginInfo.SYSID }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;
                case "cboClass":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", cbo, new string[] { "LANGID", "CMCDTYPE" }, new string[] { LoginInfo.LANGID, "MTRL_CLSS_ID" }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;
            }
        }

        #region 기말재고 적용 여부 체크박스 처리
        //private void chkApply_Checked(object sender, RoutedEventArgs e)
        //{
        //    dtpEndStockYM_EnabledProcess();
        //}

        //private void chkApply_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    dtpEndStockYM_EnabledProcess();
        //}

        //private void dtpEndStockYM_EnabledProcess()
        //{
        //    dtpEndStockYM.IsEnabled = (chkApply.IsChecked == true) ? true : false;
        //}
        #endregion

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

            // GET SYSTEM_ID 
            try
            {
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

        // 2025.02.04 : ERP 재고목록 조회 함수 
        #region GetErpStocks
        private DataTable GetErpStocks()
        {
            DataTable result = new DataTable(); // return 

            // 2025.01.24 창고목록 콤보박스 필터링 추가 (ERP 데이터 한정), DB_TYPE 반영  
            List<string> slocListByDBType = new List<string>();
            cboStockLocation.Items.Cast<DataRowView>().ToList()
                .ForEach(c => slocListByDBType.Add(c.Row.ItemArray.GetValue(0).ToString()));
            slocListByDBType.RemoveAt(0); // All 제거   

            string slocks = string.Join(",", slocListByDBType.Select(x => $"'{x}'")); // '210A', '220A' 


            // destination, repository setting
            if (IS_NERP_FLAG == true) // NERP 적용시
            {
                dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "ESP" : "ESQ");
            }
            else
            {
                dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "PRD" : "QAS");
            }
            RfcRepository repo = dest.Repository;

            IRfcFunction FnStockList;

            string sapTable = string.Empty;
            #region 일재고 
            if ((bool)rdoDay.IsChecked) // 일 재고
            {
                // NERP 적용 여부 
                string rfcName = "ZPPB_ERP_BEGIN_END_STOCK";
                sapTable = IS_NERP_FLAG == true ? "OT_STOCK_GRGI" : "IT_STOCK_GRGI";

                FnStockList = repo.CreateFunction(rfcName);

                // 2025.02.04: 제품 id 입력없이 일재고 조회 가능하게 수정 필요함 (NERP 수정 예정)
                // ERP 수정 적용 후 반영 예정 
                // 제품코드 LIKE 검색 적용 : 일단 전제품 조회 후 필터링 
                if (IS_NERP_FLAG == true)
                {
                    FnStockList.SetValue("IV_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                    if (txtProdId.Text != "") // 일재고: 제품ID 필수,보류  
                        FnStockList.SetValue("IV_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                    FnStockList.SetValue("IV_WERKS", cboShop.SelectedValue);                             //Plant         *필수
                    FnStockList.SetValue("IV_LGORT", cboStockLocation.SelectedValue);                    //저장위치      *필수

                    FnStockList.SetValue("IV_MATNR", txtProdId.Text);                                                //제품ID
                    FnStockList.SetValue("IV_FLAG", "X");                                                //수불없는 날짜 포함 (X or Blank)
                }
                else // NERP 미적용시 제품코드 관련 제약 유지 
                {
                    FnStockList.SetValue("I_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                    if (txtProdId.Text != "")
                        FnStockList.SetValue("I_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                    FnStockList.SetValue("I_WERKS", cboShop.SelectedValue);                             //Plant         *필수
                    FnStockList.SetValue("I_LGORT", cboStockLocation.SelectedValue);                    //저장위치      *필수

                    FnStockList.SetValue("I_MATNR", txtProdId.Text);                                                //제품ID
                    FnStockList.SetValue("I_FLAG", "X");                                                //수불없는 날짜 포함 (X or Blank)
                }


            }
            #endregion
            #region 현재고, 월재고 
            else
            {
                #region 현재고, 월재고 파라미터 세팅 

                // NERP 적용 여부 분기
                string rfcName = "ZPPB_SEND_ERP_STOCK";
                sapTable = IS_NERP_FLAG == true ? "TT_STOCK" : "IT_STOCK";

                // 월재고:  
                if (IS_NERP_FLAG == true && (bool)rdoMonth.IsChecked && (bool)chkNerpApply.IsChecked)
                {
                    rfcName = "ZPPNV_RFC_SEND_ERP_STOCK";
                }

                FnStockList = repo.CreateFunction(rfcName);

                // 파라미터 세팅
                // 2025.02.04: 제품 id 입력없이 일재고 조회 가능 (NERP 수정 예정)
                // 제품코드 LIKE 검색 적용 : 일단 전제품 조회 후 필터링  
                if (IS_NERP_FLAG == true)
                {
                    FnStockList.SetValue("IV_WERKS", cboShop.SelectedValue);

                    if (!string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                        FnStockList.SetValue("IV_LGORT", cboStockLocation.SelectedValue);

                    //if (!string.IsNullOrWhiteSpace(txtProdId.Text.Trim()))
                    //    FnStockList.SetValue("IV_MATNR", txtProdId.Text.Trim());

                    if ((bool)rdoMonth.IsChecked) // 월재고 수정: 20240828
                    {
                        if ((bool)chkNerpApply.IsChecked)
                        {
                            FnStockList.SetValue("IV_YYYYMM", dtpMonth.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"));
                        }
                        else
                        {
                            FnStockList.SetValue("I_YYYYMM", dtpMonth.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"));
                        }
                    }
                }
                else
                {
                    FnStockList.SetValue("I_WERKS", cboShop.SelectedValue);

                    if ((bool)rdoMonth.IsChecked)
                    {
                        FnStockList.SetValue("I_YYYYMM", dtpMonth.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"));
                    }

                    if (!string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                        FnStockList.SetValue("I_LGORT", cboStockLocation.SelectedValue);

                    //if (!string.IsNullOrWhiteSpace(txtProdId.Text.Trim()))
                    //    FnStockList.SetValue("I_MATNR", txtProdId.Text.Trim());
                }
                #endregion 현재고, 월재고

            }
            #endregion

            FnStockList.Invoke(dest);

            var stockList = FnStockList.GetTable(sapTable);
            DataTable dtStockList = (IS_NERP_FLAG == true && (bool)rdoMonth.IsChecked && (bool)chkNerpApply.IsChecked) ? 
                GetErpSummaryStock(stockList.ToDataTable(sapTable)) : // NERP 월재고, LOT별 재고비교 체크 의 경우 제품별 summary data 반환 
                stockList.ToDataTable(sapTable);  

            if (dtStockList.Rows.Count > 0) // 데이터 건수 있는 경우만 필터링 
            {
                string filter = String.Empty;
                if (!String.IsNullOrEmpty(slocks)) { filter = $"LGORT IN ({slocks}) "; }
                if (!String.IsNullOrEmpty(txtProdId.Text)) // 필터링 
                {
                    dtStockList.DefaultView.RowFilter = $"{filter} AND MATNR LIKE '%{txtProdId.Text}%' ";
                    result = dtStockList.DefaultView.ToTable();
                }
                else
                {
                    if (!String.IsNullOrEmpty(filter)) { dtStockList.DefaultView.RowFilter = $"LGORT IN ({slocks})"; }
                    result = dtStockList.DefaultView.ToTable();
                }
            }

            return result;
        }
        #endregion

        #region GetErpSummaryStock
        private DataTable GetErpSummaryStock(DataTable input)
        {
            var query = input.AsEnumerable()                                          
                            .GroupBy(g => new { WERKS = g.Field<string>("WERKS"), MATNR = g.Field<string>("MATNR"), LGORT = g.Field<string>("LGORT") })
                            .Select(x => new
                            {
                                WERKS = x.Key.WERKS,
                                MATNR = x.Key.MATNR,
                                LGORT = x.Key.LGORT,
                                LABST = x.Sum(y => y.Field<decimal>("LABST")), // 가용 
                                SPEME = x.Sum(y => y.Field<decimal>("SPEME")), // 보류 
                                UMLME = x.Sum(y => y.Field<decimal>("UMLME")),   //이동 중
                                BKLAS = x.Max(y => y.Field<string>("BKLAS"))
                            });

            DataTable dt = new DataTable();
            dt.Columns.Add("WERKS");
            dt.Columns.Add("MATNR");
            dt.Columns.Add("LGORT");
            dt.Columns.Add("LABST", Type.GetType("System.Decimal"));
            dt.Columns.Add("SPEME", Type.GetType("System.Decimal"));
            dt.Columns.Add("UMLME", Type.GetType("System.Decimal"));
            dt.Columns.Add("BKLAS");

            foreach (var item in query)
            {
                dt.Rows.Add(item.WERKS, item.MATNR, item.LGORT, item.LABST, item.SPEME, item.UMLME, item.BKLAS);
            }

            return dt;
        }
        #endregion

        void GetStocks()
        {
            try
            {
             
                string bizName = IS_NERP_FLAG == true ? "DA_PRD_SEL_STOCK_SUMMARY3" : "DA_PRD_SEL_STOCK_SUMMARY2";

                if ((bool)rdoDay.IsChecked && string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                {
                    Util.MessageValidation("SFU4136"); // 저장위치를 선택해 주세요.
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                #region Initialize Grid
                dgMaster.ItemsSource = null;
                Util.gridClear(dgDetail);
                #endregion Initialize Grid

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

                if (chkFinlwip.IsChecked == true)
                {
                    Indata["FINL_WIP_FLAG"] = "Y";
                }
                else
                {
                    Indata["FINL_WIP_FLAG"] = "N";
                }

                if ((bool)rdoCurrent.IsChecked)
                    Indata["SUM_TYPE"] = Util.NVC(rdoCurrent.Tag);
                if ((bool)rdoDay.IsChecked)
                {
                    Indata["SUM_TYPE"] = Util.NVC(rdoDay.Tag);
                    Indata["SUM_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                }
                if ((bool)rdoMonth.IsChecked)
                {
                    Indata["SUM_TYPE"] = Util.NVC(rdoMonth.Tag);
                    Indata["SUM_YM"] = dtpMonth.SelectedDateTime.ToString("yyyyMM");
                }


                Indata["SHOPID"] = cboShop.SelectedValue;
                Indata["PRODID"] = string.IsNullOrWhiteSpace(txtProdId.Text.Trim()) ? null : txtProdId.Text.Trim();
                Indata["SLOC_ID"] = string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()) ? null : cboStockLocation.SelectedValue;
                Indata["AREAID"] = cboArea.SelectedValue;

                if ((bool)rdoDay.IsChecked)
                {
                    Indata["SUM_INCLUDE_HOLD_FLAG"] = "Y";
                }
                else
                {
                    if (chkAvlPndChk.IsChecked == true)
                    {
                        Indata["SUM_INCLUDE_HOLD_FLAG"] = "N";
                    }
                    else
                    {
                        Indata["SUM_INCLUDE_HOLD_FLAG"] = "Y";
                    }
                }

                Indata["SUM_DATE_STRT_END_FLAG"] = "N";
                Indata["SUM_STRT_DATE"] = null;
                Indata["SUM_END_DATE"] = null;

                IndataTable.Rows.Add(Indata);

                //-----------------------------------------------
                _DTL_SUM_TYPE = Util.NVC(Indata["SUM_TYPE"]);
                _DTL_SUM_YM = Util.NVC(Indata["SUM_YM"]);
                _DTL_SUM_DATE = Util.NVC(Indata["SUM_DATE"]);
                _DTL_SHOPID = Util.NVC(Indata["SHOPID"]);
                _DTL_AREAID = Util.NVC(Indata["AREAID"]);
                _DTL_PRODID = Util.NVC(Indata["PRODID"]);
                _DTL_SLOC_ID = Util.NVC(Indata["SLOC_ID"]);

                if (chkFinlwip.IsChecked == true)
                    _DTL_FINL_WIP_FLAG = "Y";
                else
                    _DTL_FINL_WIP_FLAG = "N";

                _DTL_CBO_CLASS = Util.NVC(cboClass.SelectedValue);

                if (chkGmesChk.IsChecked == true)
                    _DTL_CHK_GMES_CHK = true;
                else
                    _DTL_CHK_GMES_CHK = false;
                //-----------------------------------------------


                new ClientProxy().ExecuteService(bizName, "INDATA", "RSLTDT", IndataTable, (dt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // 2025.02.05 재고목록 : ERP SUMMARY 데이터 적용 
                        DataTable erpData = GetErpStocks();
                        DataTable displayDt = new DataTable(); // dgMaster (재고목록 바인딩 dt) 

                        #region displayDt 설정 
                        displayDt.Columns.Add("SHOPID", typeof(string));
                        displayDt.Columns.Add("PRODID", typeof(string));
                        displayDt.Columns.Add("SLOC_ID", typeof(string));

                        displayDt.Columns.Add("SUM_QTY2", typeof(decimal)); //MES 가용 
                        displayDt.Columns.Add("HOLD_LOT_QTY2", typeof(decimal)); // MES 보류
                        displayDt.Columns.Add("MOVING_LOT_QTY2", typeof(decimal)); // MES 이동 중
                        displayDt.Columns.Add("TOTAL_QTY2", typeof(decimal)); // MES 합계

                        displayDt.Columns.Add("SUM_QTY2_ERP", typeof(decimal)); // ERP 가용 
                        displayDt.Columns.Add("HOLD_LOT_QTY2_ERP", typeof(decimal)); // ERP 보류
                        displayDt.Columns.Add("MOVING_LOT_QTY2_ERP", typeof(decimal)); // ERP 이동 중 
                        displayDt.Columns.Add("TOTAL_QTY2_ERP", typeof(decimal)); // ERP 합계 

                        displayDt.Columns.Add("GAP_SUM_QTY2", typeof(decimal)); // 차이 가용 
                        displayDt.Columns.Add("GAP_HOLD_LOT_QTY2", typeof(decimal)); // 차이 보류
                        displayDt.Columns.Add("GAP_MOVING_LOT_QTY2", typeof(decimal)); // 차이 이동 중 
                        displayDt.Columns.Add("GAP_TOTAL_QTY2", typeof(decimal)); //  차이 합계

                        displayDt.Columns.Add("SUM_TYPE", typeof(string));
                        displayDt.Columns.Add("SUM_YM", typeof(string));
                        displayDt.Columns.Add("SUM_DATE", typeof(string));

                        displayDt.Columns.Add("MTRLTYPE", typeof(string));
                        displayDt.Columns.Add("PRJT_NAME", typeof(string));

                        
                        #endregion

                        try
                        {
                            

                            #region 일재고 
                            if ((bool)rdoDay.IsChecked) // 일 재고
                            {
                                #region 기존 로직 수정 (주석처리) 
                                #region Get ERP Day Stock List (주석)

                                ////RfcRepository repo = dest.Repository;

                                ////// NERP 적용 여부 분기, lot별 비교 체크 여부 
                                ////string rfcName = "ZPPB_ERP_BEGIN_END_STOCK";
                                ////string sapTable = "IT_STOCK_GRGI";

                                ////if (IS_NERP_FLAG == true)
                                ////{
                                ////    if (chkNerpApply.IsChecked == true)
                                ////    {
                                ////        rfcName = "ZPPNV_RFC_ERP_BEGIN_END_STOCK";
                                ////    }
                                ////    sapTable = "OT_STOCK_GRGI";
                                ////}

                                ////IRfcFunction FnStockList = repo.CreateFunction(rfcName);

                                ////if (IS_NERP_FLAG == true)
                                ////{
                                ////    FnStockList.SetValue("IV_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                                ////    if (txtProdId.Text != "") // 일재고: 제품ID 필수,보류  
                                ////        FnStockList.SetValue("IV_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                                ////    FnStockList.SetValue("IV_WERKS", cboShop.SelectedValue);                             //Plant         *필수
                                ////    FnStockList.SetValue("IV_LGORT", cboStockLocation.SelectedValue);                    //저장위치      *필수

                                ////    FnStockList.SetValue("IV_MATNR", txtProdId.Text);                                                //제품ID
                                ////    FnStockList.SetValue("IV_FLAG", "X");                                                //수불없는 날짜 포함 (X or Blank)
                                ////}
                                ////else
                                ////{
                                ////    FnStockList.SetValue("I_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                                ////    if (txtProdId.Text != "")
                                ////        FnStockList.SetValue("I_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                                ////    FnStockList.SetValue("I_WERKS", cboShop.SelectedValue);                             //Plant         *필수
                                ////    FnStockList.SetValue("I_LGORT", cboStockLocation.SelectedValue);                    //저장위치      *필수

                                ////    FnStockList.SetValue("I_MATNR", txtProdId.Text);                                                //제품ID
                                ////    FnStockList.SetValue("I_FLAG", "X");                                                //수불없는 날짜 포함 (X or Blank)
                                ////}

                                ////FnStockList.Invoke(dest);

                                ////var stockList = FnStockList.GetTable(sapTable);
                                ////var dtStockList = stockList.ToDataTable(sapTable);
                                #endregion Get ERP Stock List

                                #region Merge Based on GMES (주석)
                                //// NERP : PLOTNO (MES LOT NO) 추가 
                                //foreach (DataRow dr in dt.Rows)
                                //{
                                //    DataView dv = erpData.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString()).AsDataView();

                                //    if (dv.Count.Equals(0)) // 창고, 제품 매핑 데이터 X --> MES 데이터만 로딩 
                                //    {
                                //        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                //        dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]);
                                //        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //        if (chkMoving.IsChecked == true)
                                //        {
                                //            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //        }
                                //        else
                                //        {
                                //            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //        }
                                //        dr["SUM_QTY2_ERP"] = 0;
                                //        dr["HOLD_LOT_QTY2_ERP"] = 0;
                                //        dr["MOVING_LOT_QTY2_ERP"] = 0;
                                //        dr["TOTAL_QTY2_ERP"] = 0;
                                //    }
                                //    else if (dv.Count > 0) // MES, ERP 매핑 데이터 처리 
                                //    {
                                //        //2024.05.10: ytkim29-- > NERP 적용: 제품, 창고 Group by
                                //        //if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                                //        //{
                                //        //    //var query = dtStockList.AsEnumerable()
                                //        //    //.Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString()
                                //        //    //&& x.Field<string>("LGORT") == dr["SLOC_ID"].ToString()
                                //        //    //&& x.Field<string>("MATNR") == dr["PRODID"].ToString())
                                //        //    //.GroupBy(g => new { MATNR = g.Field<string>("MATNR"), LGORT = g.Field<string>("LGORT") })
                                //        //    //.Select(x => new
                                //        //    //{
                                //        //    //    MATNR = x.Key.MATNR,
                                //        //    //    LGORT = x.Key.LGORT,
                                //        //    //    ZBEGIN_STOCK = x.Sum(y => y.Field<decimal>("ZBEGIN_STOCK")), // 가용 (기초 재고)
                                //        //    //    //SPEME = x.Sum(y => y.Field<decimal>("SPEME")), // 보류 
                                //        //    //    ZBEGIN_INTRANSIT = x.Sum(y => y.Field<decimal>("ZBEGIN_INTRANSIT"))   //이동 중 (운송중 기초 재고)
                                //        //    //});

                                //        //    //DataTable dtNerp = new DataTable();
                                //        //    //dtNerp.Columns.Add("MATNR");
                                //        //    //dtNerp.Columns.Add("LGORT");
                                //        //    //dtNerp.Columns.Add("ZBEGIN_STOCK", Type.GetType("System.Decimal"));
                                //        //    ////dtNerp.Columns.Add("SPEME", Type.GetType("System.Decimal"));
                                //        //    //dtNerp.Columns.Add("ZBEGIN_INTRANSIT", Type.GetType("System.Decimal"));


                                //        //    //foreach (var item in query)
                                //        //    //{
                                //        //    //    dtNerp.Rows.Add(item.MATNR, item.LGORT, item.ZBEGIN_STOCK, item.ZBEGIN_INTRANSIT);
                                //        //    //}

                                //        //    dtNerp.Rows.Add(dv[0].MATNR, item.LGORT, item.ZBEGIN_STOCK, item.ZBEGIN_INTRANSIT);

                                //        //    dv = dtNerp.DefaultView;
                                //        //}// SUM  

                                //        dr["SUM_QTY2_ERP"] = dv[0]["ZBEGIN_STOCK"]; //가용, dv 데이터 1건씩 
                                //        dr["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                //        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                //        dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]) - Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                //        dr["MOVING_LOT_QTY2_ERP"] = dv[0]["ZBEGIN_INTRANSIT"]; //이동 중
                                //        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);

                                //        if (chkMoving.IsChecked == true)
                                //        {
                                //            dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                //            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);

                                //        }
                                //        else
                                //        {
                                //            dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                //            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);

                                //            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //            dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                //        }
                                //    }
                                //}
                                #endregion Merge Base on GMES

                                #region Add Rows from ERP (주석)

                                //// 2024.06.12 ytkim29: NERP => 제품, 창고별 합산 
                                //if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                                //{

                                //    DataTable dtNerp = new DataTable();
                                //    dtNerp.Columns.Add("WERKS");
                                //    dtNerp.Columns.Add("MATNR");
                                //    dtNerp.Columns.Add("LGORT");
                                //    dtNerp.Columns.Add("ZBEGIN_STOCK", Type.GetType("System.Decimal"));
                                //    //dtNerp.Columns.Add("SPEME", Type.GetType("System.Decimal"));
                                //    dtNerp.Columns.Add("ZBEGIN_INTRANSIT", Type.GetType("System.Decimal"));

                                //    var erpOnlyRows = erpData.AsEnumerable()
                                //        .Where(a => !dt.AsEnumerable()
                                //        .Any(b => b.Field<string>("SHOPID") == a.Field<string>("WERKS")
                                //        && b.Field<string>("SLOC_ID") == a.Field<string>("LGORT")
                                //        && b.Field<string>("PRODID") == a.Field<string>("MATNR")))
                                //        .Select(x => new
                                //        {
                                //            WERKS = x.Field<string>("WERKS"),
                                //            MATNR = x.Field<string>("MATNR"),
                                //            LGORT = x.Field<string>("LGORT"),
                                //            ZBEGIN_STOCK = x.Field<decimal>("ZBEGIN_STOCK"),
                                //            ZBEGIN_INTRANSIT = x.Field<decimal>("ZBEGIN_INTRANSIT")
                                //        });

                                //    foreach (var item in erpOnlyRows)
                                //    {
                                //        //var rows = dt.AsEnumerable()
                                //        //.Where(x => x.Field<string>("SHOPID") == item.WERKS
                                //        //    && x.Field<string>("SLOC_ID") == item.LGORT
                                //        //    && x.Field<string>("PRODID") == item.MATNR);

                                //        //if (!rows.Any())
                                //        //{
                                //        //    dtNerp.Rows.Add(item.WERKS, item.MATNR, item.LGORT, item.ZBEGIN_STOCK, item.ZBEGIN_INTRANSIT);
                                //        //}

                                //        dtNerp.Rows.Add(item.WERKS, item.MATNR, item.LGORT, item.ZBEGIN_STOCK, item.ZBEGIN_INTRANSIT);
                                //    }

                                //    foreach (DataRow nRow in dtNerp.Rows)
                                //    {
                                //        DataRow newRow = dt.NewRow();
                                //        newRow["SHOPID"] = nRow["WERKS"];
                                //        newRow["SLOC_ID"] = nRow["LGORT"];
                                //        newRow["PRODID"] = nRow["MATNR"];
                                //        newRow["SUM_QTY2"] = 0; //가용
                                //        newRow["HOLD_LOT_QTY2"] = 0; //보류
                                //        newRow["MOVING_LOT_QTY2"] = 0; //이동 중
                                //        newRow["SUM_QTY2_ERP"] = nRow["ZBEGIN_STOCK"]; //가용
                                //        newRow["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                //        newRow["MOVING_LOT_QTY2_ERP"] = nRow["ZBEGIN_INTRANSIT"]; //이동 

                                //        newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(nRow["ZBEGIN_STOCK"]);
                                //        newRow["GAP_HOLD_LOT_QTY2"] = 0 - 0;
                                //        newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(nRow["ZBEGIN_INTRANSIT"]);

                                //        if (chkMoving.IsChecked == true)
                                //        {
                                //            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(nRow["ZBEGIN_STOCK"]) + 0 + Convert.ToDouble(nRow["ZBEGIN_INTRANSIT"]);
                                //            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //            newRow["TOTAL_QTY2"] = 0;
                                //        }
                                //        else
                                //        {
                                //            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(nRow["ZBEGIN_STOCK"]) + 0 + Convert.ToDouble(nRow["ZBEGIN_INTRANSIT"]);
                                //            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //            newRow["SUM_QTY2_ERP"] = Convert.ToDouble(nRow["ZBEGIN_STOCK"]) + Convert.ToDouble(nRow["ZBEGIN_INTRANSIT"]);
                                //            newRow["GAP_SUM_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //            newRow["TOTAL_QTY2"] = 0;

                                //        }

                                //        dt.Rows.Add(newRow);
                                //    }
                                //} // end of if (IS_NERP_FLAG == true)
                                //else
                                //{
                                //    // 기존 ERP 데이터 로딩 : 변경 없이 그대로 
                                //    foreach (DataRow dr in erpData.Rows)
                                //    {
                                //        DataView dv = dt.AsEnumerable().Where(x => x.Field<string>("SHOPID") == dr["WERKS"].ToString() && x.Field<string>("SLOC_ID") == dr["LGORT"].ToString() && x.Field<string>("PRODID") == dr["MATNR"].ToString()).AsDataView();

                                //        if (dv.Count.Equals(0))
                                //        {
                                //            DataRow newRow = dt.NewRow();
                                //            newRow["SHOPID"] = dr["WERKS"];
                                //            newRow["SLOC_ID"] = dr["LGORT"];
                                //            newRow["PRODID"] = dr["MATNR"];
                                //            newRow["SUM_QTY2"] = 0; //가용
                                //            newRow["HOLD_LOT_QTY2"] = 0; //보류
                                //            newRow["MOVING_LOT_QTY2"] = 0; //이동 중
                                //            newRow["SUM_QTY2_ERP"] = dr["ZBEGIN_STOCK"]; //가용
                                //            newRow["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                //            newRow["MOVING_LOT_QTY2_ERP"] = dr["ZBEGIN_INTRANSIT"]; //이동 

                                //            newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(dr["ZBEGIN_STOCK"]);
                                //            newRow["GAP_HOLD_LOT_QTY2"] = 0 - 0;
                                //            newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);

                                //            if (chkMoving.IsChecked == true)
                                //            {
                                //                newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + 0 + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                //                newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //                newRow["TOTAL_QTY2"] = 0;
                                //            }
                                //            else
                                //            {
                                //                newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + 0 + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                //                newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //                newRow["SUM_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                //                newRow["GAP_SUM_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //                newRow["TOTAL_QTY2"] = 0;

                                //            }

                                //            dt.Rows.Add(newRow);
                                //        }
                                //    }

                                //}// end of if (IS_NERP_FLAG == true) else



                                #endregion Add Rows from ERP
                                #endregion 기존 로직 수정 (주석처리) 

                                // erp 데이터가 없는 경우 (empty datatabale) 처리 : 조인 오류 방지 
                                if (erpData.Rows.Count == 0 && erpData.Columns.Count == 0)
                                {
                                    erpData.Columns.Add("WERKS", typeof(string));
                                    erpData.Columns.Add("LGORT", typeof(string));
                                    erpData.Columns.Add("MATNR", typeof(string));
                                    erpData.Columns.Add("ZBEGIN_STOCK", typeof(decimal));
                                    erpData.Columns.Add("ZBEGIN_INTRANSIT", typeof(decimal));
                                }

                                #region 1: MES-ERP 공통 제품 처리 
                                var joinRows = from mes in dt.AsEnumerable()
                                               join erp in erpData.AsEnumerable()
                                               on new { SLOC_ID = mes.Field<string>("SLOC_ID"), PRODID = mes.Field<string>("PRODID") }
                                               equals new { SLOC_ID = erp.Field<string>("LGORT"), PRODID = erp.Field<string>("MATNR") }
                                               select new
                                               {
                                                   SHOPID = mes.Field<string>("SHOPID"),
                                                   SLOC_ID = mes.Field<string>("SLOC_ID"),
                                                   PRODID = mes.Field<string>("PRODID"),
                                                   SUM_QTY2 = Convert.ToDecimal(mes.Field<double>("SUM_QTY2")), // MES 가용 
                                                   HOLD_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("HOLD_LOT_QTY2")), // MES 보류
                                                   MOVING_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("MOVING_LOT_QTY2")), // MES 이동중 
                                                   TOTAL_QTY2 = Convert.ToDecimal(mes.Field<double>("TOTAL_QTY2")), // MES 합계 
                                                   SUM_QTY2_ERP = erp.Field<decimal>("ZBEGIN_STOCK"), // ERP 가용 
                                                   HOLD_LOT_QTY2_ERP = 0, // ERP 보류 
                                                   MOVING_LOT_QTY2_ERP = erp.Field<decimal>("ZBEGIN_INTRANSIT"), // ERP 이동 중 
                                                   SUM_TYPE = mes.Field<string>("SUM_TYPE"),
                                                   SUM_YM = mes.Field<string>("SUM_YM"),
                                                   SUM_DATE = mes.Field<string>("SUM_DATE"),
                                                   MTRLTYPE = mes.Field<string>("MTRLTYPE"),
                                                   PRJT_NAME = mes.Field<string>("PRJT_NAME")
                                               };

                                foreach (var item in joinRows)
                                {
                                    decimal gapSumQty2 = item.SUM_QTY2 - item.SUM_QTY2_ERP; // 차이 가용 
                                    decimal gapHoldLotQty2 = item.HOLD_LOT_QTY2 - item.HOLD_LOT_QTY2_ERP; // 차이 보류
                                    decimal gapMovingLotQty2 = item.MOVING_LOT_QTY2 - item.MOVING_LOT_QTY2_ERP; // 차이 이동 중 

                                    // (이동/보류 체크 X: 이동중 도 가용에 포함) 
                                    displayDt.Rows.Add(
                                        item.SHOPID,
                                        item.PRODID,
                                        item.SLOC_ID,
                                        chkMoving.IsChecked == true ? item.SUM_QTY2 : item.SUM_QTY2 + item.MOVING_LOT_QTY2,
                                        item.HOLD_LOT_QTY2,
                                        item.MOVING_LOT_QTY2,
                                        item.SUM_QTY2 + item.HOLD_LOT_QTY2 + item.MOVING_LOT_QTY2, // TOTAL_QTY2
                                        chkMoving.IsChecked == true ? item.SUM_QTY2_ERP : item.SUM_QTY2_ERP + item.MOVING_LOT_QTY2_ERP,
                                        item.HOLD_LOT_QTY2_ERP,
                                        item.MOVING_LOT_QTY2_ERP,
                                        item.SUM_QTY2_ERP + item.HOLD_LOT_QTY2_ERP + item.MOVING_LOT_QTY2_ERP, // TOTAL_QTY2_ERP
                                        chkMoving.IsChecked == true ? gapSumQty2 : gapSumQty2 + gapMovingLotQty2, // 차이 가용 
                                        gapHoldLotQty2, // 차이 보류 
                                        gapMovingLotQty2, // 차이 이동 중 
                                        gapSumQty2 + gapHoldLotQty2 + gapMovingLotQty2, // 차이 합계 
                                        item.SUM_TYPE,
                                        item.SUM_YM,
                                        item.SUM_DATE,
                                        item.MTRLTYPE,
                                        item.PRJT_NAME
                                        );
                                }

                                #endregion 1: MES-ERP 공통 제품 처리 

                                #region 2: MES only (ERP x) 
                                var mesOnlyRows = dt.AsEnumerable()
                                    .Where(a => !erpData.AsEnumerable()
                                    .Any(b => b.Field<string>("WERKS") == a.Field<string>("SHOPID")
                                    && b.Field<string>("LGORT") == a.Field<string>("SLOC_ID")
                                    && b.Field<string>("MATNR") == a.Field<string>("PRODID")))
                                    .Select(x => new
                                    {
                                        SHOPID = x.Field<string>("SHOPID"),
                                        PRODID = x.Field<string>("PRODID"),
                                        SLOC_ID = x.Field<string>("SLOC_ID"),
                                        SUM_QTY2 = Convert.ToDecimal(x.Field<double>("SUM_QTY2")),
                                        HOLD_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("HOLD_LOT_QTY2")),
                                        MOVING_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("MOVING_LOT_QTY2")),
                                        TOTAL_QTY2 = Convert.ToDecimal(x.Field<double>("TOTAL_QTY2")),
                                        SUM_TYPE = x.Field<string>("SUM_TYPE"),
                                        SUM_YM = x.Field<string>("SUM_YM"),
                                        SUM_DATE = x.Field<string>("SUM_DATE"),
                                        MTRLTYPE = x.Field<string>("MTRLTYPE"),
                                        PRJT_NAME = x.Field<string>("PRJT_NAME")
                                    });

                                foreach (var item in mesOnlyRows)
                                {
                                    // (이동/보류 체크 X: 이동중 도 가용에 포함) 
                                    displayDt.Rows.Add(
                                        item.SHOPID,
                                        item.PRODID,
                                        item.SLOC_ID,
                                        chkMoving.IsChecked == true ? item.SUM_QTY2 : item.SUM_QTY2 + item.MOVING_LOT_QTY2,
                                        item.HOLD_LOT_QTY2,
                                        item.MOVING_LOT_QTY2,
                                        item.SUM_QTY2 + item.HOLD_LOT_QTY2 + item.MOVING_LOT_QTY2, // TOTAL_QTY2
                                        0,
                                        0,
                                        0,
                                        0, // TOTAL_QTY2_ERP
                                        chkMoving.IsChecked == true ? item.SUM_QTY2 : item.SUM_QTY2 + item.MOVING_LOT_QTY2, // 차이 가용 
                                        item.HOLD_LOT_QTY2, // 차이 보류 
                                        item.MOVING_LOT_QTY2, // 차이 이동 중 
                                        item.SUM_QTY2 + item.HOLD_LOT_QTY2 + item.MOVING_LOT_QTY2, // 차이 합계 
                                        item.SUM_TYPE,
                                        item.SUM_YM,
                                        item.SUM_DATE,
                                        item.MTRLTYPE,
                                        item.PRJT_NAME
                                        );

                                }

                                #endregion 2: MES only (ERP x) 

                                #region 3: ERP only (MES x) 
                                var erpOnlyRows = erpData.AsEnumerable()
                                    .Where(a => !dt.AsEnumerable()
                                    .Any(b => b.Field<string>("SHOPID") == a.Field<string>("WERKS")
                                    && b.Field<string>("SLOC_ID") == a.Field<string>("LGORT")
                                    && b.Field<string>("PRODID") == a.Field<string>("MATNR")))
                                    .Select(x => new
                                    {
                                        ZDAY = x.Field<string>("ZDAY"),
                                        SHOPID = x.Field<string>("WERKS"),
                                        PRODID = x.Field<string>("MATNR"),
                                        SLOC_ID = x.Field<string>("LGORT"),
                                        ZBEGIN_STOCK = x.Field<decimal>("ZBEGIN_STOCK"),
                                        ZBEGIN_INTRANSIT = x.Field<decimal>("ZBEGIN_INTRANSIT")
                                    });

                                foreach (var item in erpOnlyRows)
                                {
                                    // (이동/보류 체크 X: 이동중 도 가용에 포함), ERP 일재고 => 가용만 존재하는 지 확인 필요 
                                    displayDt.Rows.Add(
                                        item.SHOPID,
                                        item.PRODID,
                                        item.SLOC_ID,
                                        0,
                                        0,
                                        0,
                                        0, // TOTAL_QTY2
                                        item.ZBEGIN_STOCK,
                                        0,
                                        0, // item.ZBEGIN_INTRANSIT,
                                        item.ZBEGIN_STOCK, // TOTAL_QTY2_ERP
                                        item.ZBEGIN_STOCK * -1, // 차이 가용 
                                        0, // 차이 보류 
                                        0, // 차이 이동 중 
                                        item.ZBEGIN_STOCK * -1, // 차이 합계 
                                        "",
                                        "",
                                        "",
                                        "",
                                        ""
                                        );
                                }

                                #endregion 3: ERP only (MES x)

                            }
                            #endregion
                            #region  현재고, 월재고
                            else // 현재고, 월재고
                            {
                                //if (!dt.Columns.Contains("BKLAS"))
                                //    dt.Columns.Add("BKLAS", typeof(string));

                                if (!displayDt.Columns.Contains("BKLAS"))
                                    displayDt.Columns.Add("BKLAS", typeof(string));

                                #region 기존 로직 (주석)
                                #region Get ERP Stock List (주석)

                                //RfcRepository repo = dest.Repository;

                                //// NERP 적용 여부 분기
                                //string rfcName = "ZPPB_SEND_ERP_STOCK";
                                //string sapTable = "IT_STOCK";

                                //if (IS_NERP_FLAG == true)
                                //{
                                //    if (chkNerpApply.IsChecked == true)
                                //    {
                                //        rfcName = "ZPPNV_RFC_SEND_ERP_STOCK";
                                //    }
                                //    sapTable = "TT_STOCK";
                                //}

                                //IRfcFunction FnStockList = repo.CreateFunction(rfcName);

                                //if (IS_NERP_FLAG == true)
                                //{
                                //    FnStockList.SetValue("IV_WERKS", cboShop.SelectedValue);

                                //    if (!string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                                //        FnStockList.SetValue("IV_LGORT", cboStockLocation.SelectedValue);

                                //    if (!string.IsNullOrWhiteSpace(txtProdId.Text.Trim()))
                                //        FnStockList.SetValue("IV_MATNR", txtProdId.Text.Trim());

                                //    if ((bool)rdoMonth.IsChecked) // 월재고 수정: 20240828
                                //    {
                                //        if ((bool)chkNerpApply.IsChecked)
                                //        {
                                //            FnStockList.SetValue("IV_YYYYMM", dtpMonth.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"));
                                //        }
                                //        else
                                //        {
                                //            FnStockList.SetValue("I_YYYYMM", dtpMonth.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"));
                                //        }

                                //        //string selectedMonth = dtpMonth.SelectedDateTime.AddMonths(0).ToString("yyyyMM");
                                //        //int lastDayOfMonth = DateTime.DaysInMonth(int.Parse(selectedMonth.Substring(0, 4)), int.Parse(selectedMonth.Substring(4, 2)));
                                //        //string lastDate = selectedMonth + lastDayOfMonth.ToString();

                                //        //FnStockList.SetValue("IV_DAY_FROM", lastDate);
                                //        //FnStockList.SetValue("IV_DAY_TO", lastDate);
                                //    }
                                //}
                                //else
                                //{
                                //    FnStockList.SetValue("I_WERKS", cboShop.SelectedValue);

                                //    //if (chkApply.IsChecked == true)
                                //    //    FnStockList.SetValue("I_YYYYMM", dtpEndStockYM.SelectedDateTime.ToString("yyyyMM"));

                                //    if ((bool)rdoMonth.IsChecked)
                                //    {
                                //        FnStockList.SetValue("I_YYYYMM", dtpMonth.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"));
                                //    }

                                //    if (!string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                                //        FnStockList.SetValue("I_LGORT", cboStockLocation.SelectedValue);

                                //    if (!string.IsNullOrWhiteSpace(txtProdId.Text.Trim()))
                                //        FnStockList.SetValue("I_MATNR", txtProdId.Text.Trim());
                                //}


                                //FnStockList.Invoke(dest);

                                //var stockList = FnStockList.GetTable(sapTable);
                                //var dtStockList = stockList.ToDataTable(sapTable);
                                #endregion Get ERP Stock List

                                #region Merge Based on GMES (주석)
                                //// NERP : PLOTNO (MES LOT NO) 추가 
                                //foreach (DataRow dr in dt.Rows)
                                //{
                                //    DataView dv = erpData.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString()).AsDataView();

                                //    if (dv.Count.Equals(0)) // MES 매핑 ERP 데이터 X, MES 데이터만 로딩 
                                //    {
                                //        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                //        dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]);
                                //        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //        if (chkMoving.IsChecked == true)
                                //        {
                                //            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //        }
                                //        else
                                //        {
                                //            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                //            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);

                                //        }
                                //    }
                                //    else if (dv.Count > 0)
                                //    {


                                //        // NERP 적용 (2024.05.09) : 제품, 창고 기준으로 SUM 
                                //        if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                                //        {
                                //            var query = dtStockList.AsEnumerable()
                                //            .Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString()
                                //            && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString()
                                //            && x.Field<string>("MATNR") == dr["PRODID"].ToString())
                                //            .GroupBy(g => new { MATNR = g.Field<string>("MATNR"), LGORT = g.Field<string>("LGORT") })
                                //            .Select(x => new
                                //            {
                                //                //WERKS = x.Key.WERKS,
                                //                MATNR = x.Key.MATNR,
                                //                LGORT = x.Key.LGORT,
                                //                LABST = x.Sum(y => y.Field<decimal>("LABST")), // 가용 
                                //                SPEME = x.Sum(y => y.Field<decimal>("SPEME")), // 보류 
                                //                UMLME = x.Sum(y => y.Field<decimal>("UMLME"))   //이동 중
                                //            });

                                //            DataTable dtNerp = new DataTable();
                                //            //dtNerp.Columns.Add("WERKS");
                                //            dtNerp.Columns.Add("MATNR");
                                //            dtNerp.Columns.Add("LGORT");
                                //            dtNerp.Columns.Add("LABST", Type.GetType("System.Decimal"));
                                //            dtNerp.Columns.Add("SPEME", Type.GetType("System.Decimal"));
                                //            dtNerp.Columns.Add("UMLME", Type.GetType("System.Decimal"));


                                //            foreach (var item in query)
                                //            {
                                //                dtNerp.Rows.Add(item.MATNR, item.LGORT, item.LABST, item.SPEME, item.UMLME);
                                //            }
                                //            dv = dtNerp.DefaultView;
                                //        }

                                //        if (chkAvlPndChk.IsChecked == true)
                                //        {
                                //            dr["SUM_QTY2_ERP"] = dv[0]["LABST"]; //가용                        
                                //            dr["HOLD_LOT_QTY2_ERP"] = dv[0]["SPEME"]; //보류                                                
                                //            dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]) - Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                //            dr["MOVING_LOT_QTY2_ERP"] = dv[0]["UMLME"]; //이동 중
                                //            dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);

                                //            if (chkMoving.IsChecked == true)
                                //            {
                                //                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                //                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                //                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);

                                //            }
                                //            else
                                //            {
                                //                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                //                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //                dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //                dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                //                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                //                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]);
                                //            }
                                //        }
                                //        else
                                //        {
                                //            dr["SUM_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]); //가용 + 보류                       
                                //            dr["HOLD_LOT_QTY2_ERP"] = 0; //보류                                                                                   
                                //            dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]) - Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                //            dr["MOVING_LOT_QTY2_ERP"] = dv[0]["UMLME"]; //이동 중
                                //            dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);

                                //            if (chkMoving.IsChecked == true)
                                //            {
                                //                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                //                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //                dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]);
                                //                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                //                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);

                                //            }
                                //            else
                                //            {
                                //                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                //                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //                dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                //                dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                //                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                //                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]);
                                //            }
                                //        }
                                //        //}
                                //    }
                                //}
                                #endregion Merge Base on GMES

                                #region Add Rows from ERP (주석)

                                //// 2024.06.12 ytkim29: NERP => 제품, 창고별 합산 
                                //if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                                //{
                                //    var query = dtStockList.AsEnumerable()
                                //            .GroupBy(g => new { WERKS = g.Field<string>("WERKS"), MATNR = g.Field<string>("MATNR"), LGORT = g.Field<string>("LGORT") })
                                //            .Select(x => new
                                //            {
                                //                WERKS = x.Key.WERKS,
                                //                MATNR = x.Key.MATNR,
                                //                LGORT = x.Key.LGORT,
                                //                LABST = x.Sum(y => y.Field<decimal>("LABST")), // 가용 
                                //                SPEME = x.Sum(y => y.Field<decimal>("SPEME")), // 보류 
                                //                UMLME = x.Sum(y => y.Field<decimal>("UMLME"))   //이동 중
                                //            });

                                //    DataTable dtNerp = new DataTable();
                                //    dtNerp.Columns.Add("WERKS");
                                //    dtNerp.Columns.Add("MATNR");
                                //    dtNerp.Columns.Add("LGORT");
                                //    dtNerp.Columns.Add("LABST", Type.GetType("System.Decimal"));
                                //    dtNerp.Columns.Add("SPEME", Type.GetType("System.Decimal"));
                                //    dtNerp.Columns.Add("UMLME", Type.GetType("System.Decimal"));


                                //    foreach (var item in query)
                                //    {
                                //        if (!slocListByDBType.Contains(item.LGORT)) { continue; } // DB_TYPE의 창고 해당 여부

                                //        // MES DATA에 있으면 제외 
                                //        var rows = dt.AsEnumerable()
                                //        .Where(x => x.Field<string>("SHOPID") == item.WERKS
                                //            && x.Field<string>("SLOC_ID") == item.LGORT
                                //            && x.Field<string>("PRODID") == item.MATNR);

                                //        if (!rows.Any())
                                //        {
                                //            dtNerp.Rows.Add(item.WERKS, item.MATNR, item.LGORT, item.LABST, item.SPEME, item.UMLME);
                                //        }
                                //    }

                                //    foreach (DataRow nRow in dtNerp.Rows)
                                //    {
                                //        DataRow newRow = dt.NewRow();
                                //        newRow["SHOPID"] = nRow["WERKS"];
                                //        newRow["SLOC_ID"] = nRow["LGORT"];
                                //        newRow["PRODID"] = nRow["MATNR"];
                                //        newRow["SUM_QTY2"] = 0; //가용
                                //        newRow["HOLD_LOT_QTY2"] = 0; //보류
                                //        newRow["MOVING_LOT_QTY2"] = 0; //이동 중

                                //        if (chkAvlPndChk.IsChecked == true)
                                //        {
                                //            newRow["SUM_QTY2_ERP"] = nRow["LABST"]; //가용
                                //            newRow["HOLD_LOT_QTY2_ERP"] = nRow["SPEME"]; //보류
                                //            newRow["MOVING_LOT_QTY2_ERP"] = nRow["UMLME"]; //이동 

                                //            newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(nRow["LABST"]);
                                //            newRow["GAP_HOLD_LOT_QTY2"] = 0 - Convert.ToDouble(nRow["SPEME"]);
                                //            newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(nRow["UMLME"]);
                                //        }
                                //        else
                                //        {
                                //            newRow["SUM_QTY2_ERP"] = Convert.ToDouble(nRow["LABST"]) + Convert.ToDouble(nRow["SPEME"]); //가용 + 보류
                                //            newRow["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                //            newRow["MOVING_LOT_QTY2_ERP"] = nRow["UMLME"]; //이동 

                                //            newRow["GAP_SUM_QTY2"] = 0 - (Convert.ToDouble(nRow["LABST"]) + Convert.ToDouble(nRow["SPEME"]));
                                //            newRow["GAP_HOLD_LOT_QTY2"] = 0;
                                //            newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(nRow["UMLME"]);
                                //        }

                                //        if (chkMoving.IsChecked == true)
                                //        {
                                //            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(nRow["LABST"]) + Convert.ToDouble(nRow["SPEME"]) + Convert.ToDouble(nRow["UMLME"]);
                                //            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //            newRow["TOTAL_QTY2"] = 0;
                                //        }
                                //        else
                                //        {
                                //            newRow["SUM_QTY2_ERP"] = Convert.ToDouble(newRow["SUM_QTY2_ERP"]) + Convert.ToDouble(nRow["UMLME"]);
                                //            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(nRow["LABST"]) + Convert.ToDouble(nRow["SPEME"]) + Convert.ToDouble(nRow["UMLME"]);
                                //            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //            newRow["GAP_SUM_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //            newRow["TOTAL_QTY2"] = 0;
                                //        }

                                //        //newRow["BKLAS"] = nRow["BKLAS"];

                                //        dt.Rows.Add(newRow);
                                //    }

                                //} // end of if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                                //else
                                //{
                                //    // 기존 ERP 데이터 로딩 그대로 
                                //    foreach (DataRow dr in (dtStockList as DataTable).Rows)
                                //    {
                                //        DataView dv = dt.AsEnumerable().Where(x => x.Field<string>("SHOPID") == dr["WERKS"].ToString() && x.Field<string>("SLOC_ID") == dr["LGORT"].ToString() && x.Field<string>("PRODID") == dr["MATNR"].ToString()).AsDataView();

                                //        if (dv.Count.Equals(0))
                                //        {
                                //            //int iBKLAS = 0;
                                //            //int.TryParse(dr["BKLAS"].ToString(), out iBKLAS);

                                //            //if (iBKLAS > 7900)
                                //            //{
                                //            DataRow newRow = dt.NewRow();
                                //            newRow["SHOPID"] = dr["WERKS"];
                                //            newRow["SLOC_ID"] = dr["LGORT"];
                                //            newRow["PRODID"] = dr["MATNR"];
                                //            newRow["SUM_QTY2"] = 0; //가용
                                //            newRow["HOLD_LOT_QTY2"] = 0; //보류
                                //            newRow["MOVING_LOT_QTY2"] = 0; //이동 중
                                //            if (chkAvlPndChk.IsChecked == true)
                                //            {
                                //                newRow["SUM_QTY2_ERP"] = dr["LABST"]; //가용
                                //                newRow["HOLD_LOT_QTY2_ERP"] = dr["SPEME"]; //보류
                                //                newRow["MOVING_LOT_QTY2_ERP"] = dr["UMLME"]; //이동 

                                //                newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(dr["LABST"]);
                                //                newRow["GAP_HOLD_LOT_QTY2"] = 0 - Convert.ToDouble(dr["SPEME"]);
                                //                newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(dr["UMLME"]);
                                //            }
                                //            else
                                //            {
                                //                newRow["SUM_QTY2_ERP"] = Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]); //가용 + 보류
                                //                newRow["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                //                newRow["MOVING_LOT_QTY2_ERP"] = dr["UMLME"]; //이동 

                                //                newRow["GAP_SUM_QTY2"] = 0 - (Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]));
                                //                newRow["GAP_HOLD_LOT_QTY2"] = 0;
                                //                newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(dr["UMLME"]);
                                //            }

                                //            if (chkMoving.IsChecked == true)
                                //            {
                                //                newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]) + Convert.ToDouble(dr["UMLME"]);
                                //                newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //                newRow["TOTAL_QTY2"] = 0;
                                //            }
                                //            else
                                //            {
                                //                newRow["SUM_QTY2_ERP"] = Convert.ToDouble(newRow["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["UMLME"]);
                                //                newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]) + Convert.ToDouble(dr["UMLME"]);
                                //                newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //                newRow["GAP_SUM_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                //                newRow["TOTAL_QTY2"] = 0;
                                //            }

                                //            newRow["BKLAS"] = dr["BKLAS"];

                                //            dt.Rows.Add(newRow);
                                //            //}
                                //        }
                                //    }
                                //}


                                #endregion Add Rows from ERP

                                #endregion

                                #region 1. MES, ERP 공통 데이터 처리 
                                var joinRows = from mes in dt.AsEnumerable()
                                               join erp in erpData.AsEnumerable()
                                               on new { SLOC_ID = mes.Field<string>("SLOC_ID"), PRODID = mes.Field<string>("PRODID") }
                                               equals new { SLOC_ID = erp.Field<string>("LGORT"), PRODID = erp.Field<string>("MATNR") }
                                               select new
                                               {
                                                   SHOPID = mes.Field<string>("SHOPID"),
                                                   SLOC_ID = mes.Field<string>("SLOC_ID"),
                                                   PRODID = mes.Field<string>("PRODID"),
                                                   SUM_QTY2 = Convert.ToDecimal(mes.Field<double>("SUM_QTY2")), // MES 가용 
                                                   HOLD_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("HOLD_LOT_QTY2")), // MES 보류
                                                   MOVING_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("MOVING_LOT_QTY2")), // MES 이동중 
                                                   TOTAL_QTY2 = Convert.ToDecimal(mes.Field<double>("TOTAL_QTY2")), // MES 합계 
                                                   SUM_QTY2_ERP = erp.Field<decimal>("LABST"), // ERP 가용 
                                                   HOLD_LOT_QTY2_ERP = erp.Field<decimal>("SPEME"), // ERP 보류 
                                                   MOVING_LOT_QTY2_ERP = erp.Field<decimal>("UMLME"), // ERP 이동 중 
                                                   SUM_TYPE = mes.Field<string>("SUM_TYPE"),
                                                   SUM_YM = mes.Field<string>("SUM_YM"),
                                                   SUM_DATE = mes.Field<string>("SUM_DATE"),
                                                   MTRLTYPE = mes.Field<string>("MTRLTYPE"),
                                                   PRJT_NAME = mes.Field<string>("PRJT_NAME")
                                               };
                                foreach (var item in joinRows)
                                {
                                    decimal sumQty2Erp = 0; // ERP 가용 
                                    decimal holdLotQty2Erp = 0; // ERP 보류 
                                    decimal gapHoldLotQty2 = 0; // 차이 보류 
                                    decimal movingLotQty2Erp = 0; // ERP 이동 중 
                                    decimal gapMovingLotQty2 = 0; // 차이 이동 중 

                                    decimal totalQty2Erp = 0; // ERP 합계
                                    decimal totalQty2 = 0; // MES 합계 
                                    decimal gapSumQty2 = 0; // 차이 가용 
                                    decimal gapTotalQty2 = 0; // 차이 합계 
                                    decimal sumQty2 = 0;
                                    if (chkAvlPndChk.IsChecked == true)
                                    {
                                        sumQty2Erp = item.SUM_QTY2_ERP; // ERP 가용 
                                        holdLotQty2Erp = item.HOLD_LOT_QTY2_ERP; // ERP 보류 
                                        gapHoldLotQty2 = item.HOLD_LOT_QTY2 - item.HOLD_LOT_QTY2_ERP; // 차이 보류 
                                        movingLotQty2Erp = item.MOVING_LOT_QTY2_ERP; // ERP 이동 중 
                                        gapMovingLotQty2 = item.MOVING_LOT_QTY2 - item.MOVING_LOT_QTY2_ERP; // 차이 이동 중 

                                        totalQty2Erp = item.SUM_QTY2_ERP + item.HOLD_LOT_QTY2_ERP + item.MOVING_LOT_QTY2_ERP; // ERP 합계
                                        totalQty2 = item.SUM_QTY2 + item.HOLD_LOT_QTY2 + item.MOVING_LOT_QTY2; // MES 합계 
                                        gapSumQty2 = 0; // 차이 가용 
                                        gapTotalQty2 = 0; // 차이 합계 
                                        sumQty2 = 0;

                                        if (chkMoving.IsChecked == true) // 이동 구분 
                                        {
                                            gapSumQty2 = item.SUM_QTY2 - item.SUM_QTY2_ERP;
                                            gapTotalQty2 = gapSumQty2 + gapHoldLotQty2 + gapMovingLotQty2;
                                        }
                                        else
                                        {
                                            sumQty2 = item.SUM_QTY2 + item.MOVING_LOT_QTY2;
                                            sumQty2Erp = item.SUM_QTY2_ERP + item.MOVING_LOT_QTY2_ERP;
                                            gapSumQty2 = item.SUM_QTY2 - item.SUM_QTY2_ERP;
                                            gapTotalQty2 = gapSumQty2 + gapHoldLotQty2;
                                        }
                                    }
                                    else
                                    {
                                        sumQty2Erp = item.SUM_QTY2_ERP + item.HOLD_LOT_QTY2_ERP; // ERP 가용 : 가용 + 보류 
                                        holdLotQty2Erp = 0; // 보류는 0으로 
                                        gapHoldLotQty2 = item.HOLD_LOT_QTY2; // ERP는 보류 0 
                                        movingLotQty2Erp = item.MOVING_LOT_QTY2_ERP; // ERP 이동 중 
                                        gapMovingLotQty2 = item.MOVING_LOT_QTY2 - item.MOVING_LOT_QTY2_ERP;

                                        totalQty2Erp = item.SUM_QTY2_ERP + item.HOLD_LOT_QTY2_ERP + item.MOVING_LOT_QTY2_ERP; // ERP 합계
                                        totalQty2 = item.SUM_QTY2 + item.HOLD_LOT_QTY2 + item.MOVING_LOT_QTY2; // MES 합계

                                        sumQty2 = 0; // MES 가용 
                                        gapSumQty2 = 0; // 차이 가용 
                                        gapTotalQty2 = 0; // 차이 합계 

                                        if (chkMoving.IsChecked == true)
                                        {
                                            sumQty2 = item.SUM_QTY2 + item.HOLD_LOT_QTY2;
                                            gapSumQty2 = sumQty2 - sumQty2Erp;
                                            gapTotalQty2 = gapSumQty2 + gapHoldLotQty2 + gapMovingLotQty2;
                                        }
                                        else
                                        {
                                            sumQty2 = item.SUM_QTY2 + item.HOLD_LOT_QTY2 + item.MOVING_LOT_QTY2;
                                            sumQty2Erp = sumQty2Erp + item.MOVING_LOT_QTY2_ERP;
                                            gapSumQty2 = sumQty2 - sumQty2Erp;
                                            gapTotalQty2 = gapSumQty2 + gapHoldLotQty2;
                                        }
                                    }

                                    displayDt.Rows.Add(
                                        item.SHOPID,
                                        item.PRODID,
                                        item.SLOC_ID,
                                        sumQty2,
                                        item.HOLD_LOT_QTY2,
                                        item.MOVING_LOT_QTY2,
                                        totalQty2, // TOTAL_QTY2
                                        sumQty2Erp,
                                        holdLotQty2Erp, // 0 
                                        movingLotQty2Erp,
                                        totalQty2Erp, // TOTAL_QTY2_ERP
                                        gapSumQty2, // 차이 가용 
                                        gapHoldLotQty2, // 차이 보류 
                                        gapMovingLotQty2, // 차이 이동 중 
                                        gapTotalQty2, // 차이 합계 
                                        item.SUM_TYPE,
                                        item.SUM_YM,
                                        item.SUM_DATE,
                                        item.MTRLTYPE,
                                        item.PRJT_NAME
                                        );

                                }// end of foreach (var item in joinRows)

                                #endregion

                                #region 2. MES only (ERP x)
                                var mesOnlyRows = dt.AsEnumerable()
                                    .Where(a => !erpData.AsEnumerable()
                                    .Any(b => b.Field<string>("WERKS") == a.Field<string>("SHOPID")
                                    && b.Field<string>("LGORT") == a.Field<string>("SLOC_ID")
                                    && b.Field<string>("MATNR") == a.Field<string>("PRODID")))
                                    .Select(x => new
                                    {
                                        SHOPID = x.Field<string>("SHOPID"),
                                        PRODID = x.Field<string>("PRODID"),
                                        SLOC_ID = x.Field<string>("SLOC_ID"),
                                        SUM_QTY2 = Convert.ToDecimal(x.Field<double>("SUM_QTY2")),
                                        HOLD_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("HOLD_LOT_QTY2")),
                                        MOVING_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("MOVING_LOT_QTY2")),
                                        TOTAL_QTY2 = Convert.ToDecimal(x.Field<double>("TOTAL_QTY2")),
                                        SUM_TYPE = x.Field<string>("SUM_TYPE"),
                                        SUM_YM = x.Field<string>("SUM_YM"),
                                        SUM_DATE = x.Field<string>("SUM_DATE"),
                                        MTRLTYPE = x.Field<string>("MTRLTYPE"),
                                        PRJT_NAME = x.Field<string>("PRJT_NAME")
                                    });

                                
                                foreach (var item in mesOnlyRows)
                                {
                                    decimal gapSumQty2 = item.SUM_QTY2; // 가용 
                                    decimal gapHoldLotQty2 = item.HOLD_LOT_QTY2;
                                    decimal gapMovingLotQty2 = item.MOVING_LOT_QTY2;

                                    decimal gapTotalQty2 = gapSumQty2 + gapHoldLotQty2 + gapMovingLotQty2;
                                    decimal totalQty2 = item.SUM_QTY2 + item.HOLD_LOT_QTY2 + item.MOVING_LOT_QTY2;
                                    decimal sumQty2 = item.SUM_QTY2;

                                    if (chkMoving.IsChecked == false)
                                    {
                                        gapSumQty2 = gapSumQty2 + gapMovingLotQty2; // 이동 중 포함 
                                        sumQty2 = sumQty2 + item.MOVING_LOT_QTY2;
                                    }

                                    displayDt.Rows.Add(
                                        item.SHOPID,
                                        item.PRODID,
                                        item.SLOC_ID,
                                        sumQty2,
                                        item.HOLD_LOT_QTY2,
                                        item.MOVING_LOT_QTY2,
                                        totalQty2, // TOTAL_QTY2
                                        0,
                                        0, // 0 
                                        0,
                                        0, // TOTAL_QTY2_ERP
                                        gapSumQty2, // 차이 가용 
                                        gapHoldLotQty2, // 차이 보류 
                                        gapMovingLotQty2, // 차이 이동 중 
                                        gapTotalQty2, // 차이 합계 
                                        item.SUM_TYPE,
                                        item.SUM_YM,
                                        item.SUM_DATE,
                                        item.MTRLTYPE,
                                        item.PRJT_NAME
                                        );
                                }

                                #endregion

                                #region 3. ERP only (MES x) 주석처리 : MES 관리 데이터 X
                                var erpOnlyRows = erpData.AsEnumerable()
                                   .Where(a => !dt.AsEnumerable()
                                   .Any(b => b.Field<string>("SHOPID") == a.Field<string>("WERKS")
                                   && b.Field<string>("SLOC_ID") == a.Field<string>("LGORT")
                                   && b.Field<string>("PRODID") == a.Field<string>("MATNR"))
                                   && a.Field<string>("BKLAS").StartsWith("7"))
                                   .Select(x => new
                                   {
                                       SHOPID = x.Field<string>("WERKS"),
                                       PRODID = x.Field<string>("MATNR"),
                                       SLOC_ID = x.Field<string>("LGORT"),
                                       LABST = x.Field<decimal>("LABST"),
                                       SPEME = x.Field<decimal>("SPEME"),
                                       UMLME = x.Field<decimal>("UMLME")
                                   });

                                foreach (var item in erpOnlyRows)
                                {
                                    decimal sumQty2Erp = 0;
                                    decimal holdLotQty2Erp = 0;
                                    decimal movingLotQty2Erp = item.UMLME;

                                    decimal gapSumQty2 = 0;
                                    decimal gapHoldLotQty2 = 0;
                                    decimal gapMovingLotQty2 = 0;

                                    decimal totalQty2Erp = item.LABST + item.SPEME + item.UMLME;
                                    decimal gapTotalQty2 = 0;

                                    if (chkAvlPndChk.IsChecked == true)
                                    {
                                        sumQty2Erp = item.LABST;
                                        holdLotQty2Erp = item.SPEME;
                                        gapSumQty2 = 0 - item.LABST;
                                        gapHoldLotQty2 = 0 - item.SPEME;
                                        gapMovingLotQty2 = 0 - item.UMLME;
                                    }
                                    else
                                    {
                                        sumQty2Erp = item.LABST + item.SPEME;
                                        holdLotQty2Erp = 0;
                                        gapSumQty2 = 0 - (item.LABST + item.SPEME);
                                        gapMovingLotQty2 = 0 - item.UMLME;
                                    }

                                    gapTotalQty2 = gapSumQty2 + gapHoldLotQty2 + gapMovingLotQty2;
                                    if (chkMoving.IsChecked == false)
                                    {
                                        sumQty2Erp = sumQty2Erp + item.UMLME;
                                        gapSumQty2 = gapSumQty2 + gapMovingLotQty2;
                                    }

                                    displayDt.Rows.Add(
                                        item.SHOPID,
                                        item.PRODID,
                                        item.SLOC_ID,
                                        0,
                                        0,
                                        0,
                                        0, // TOTAL_QTY2
                                        sumQty2Erp,
                                        holdLotQty2Erp, // 0 
                                        movingLotQty2Erp,
                                        totalQty2Erp, // TOTAL_QTY2_ERP
                                        gapSumQty2, // 차이 가용 
                                        gapHoldLotQty2, // 차이 보류 
                                        gapMovingLotQty2, // 차이 이동 중 
                                        gapTotalQty2, // 차이 합계 
                                        "",
                                        "",
                                        "",
                                        "",
                                        ""
                                        );

                                }// end of foreach (var item in erpOnlyRows)
                                #endregion

                            }
                            #endregion


                        }
                        catch (Exception ex) { }

                        #region Remove Rows by Condition
                        if (chkDiff.IsChecked == true)
                            for (int i = displayDt.Rows.Count - 1; i >= 0; i--)
                                if (Convert.ToDouble(displayDt.Rows[i]["SUM_QTY2"]).Equals(Convert.ToDouble(displayDt.Rows[i]["SUM_QTY2_ERP"])) && Convert.ToDouble(displayDt.Rows[i]["HOLD_LOT_QTY2"]).Equals(Convert.ToDouble(displayDt.Rows[i]["HOLD_LOT_QTY2_ERP"])) && Convert.ToDouble(displayDt.Rows[i]["MOVING_LOT_QTY2"]).Equals(Convert.ToDouble(displayDt.Rows[i]["MOVING_LOT_QTY2_ERP"])))
                                    displayDt.Rows.RemoveAt(i);
                        #endregion Remove Rows by Condition

                        #region SLOC GMES_USE_FLAG = 'Y'만 표시
                        DataTable IndataTableSLOC = new DataTable("INDATA");
                        IndataTableSLOC.Columns.Add("SHOPID", typeof(string));
                        IndataTableSLOC.Columns.Add("AREAID", typeof(string));

                        DataRow IndataSLOC = IndataTableSLOC.NewRow();
                        IndataSLOC["SHOPID"] = cboShop.SelectedValue;
                        IndataSLOC["AREAID"] = cboArea.SelectedValue;
                        IndataTableSLOC.Rows.Add(IndataSLOC);

                        DataTable dtSLOC = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_GMES_USE", "INDATA", "RSLTDT", IndataTableSLOC);

                        displayDt.Columns.Add("GMES_USE_FLAG");

                        // GMES_USE_FLAG Update후 GMES_USE_FLAG 없는경우 SLOC_ID Clear
                        displayDt.AsEnumerable().Join(dtSLOC.AsEnumerable(),
                                     dt1_Row => dt1_Row.Field<string>("SLOC_ID"),
                                     dt2_Row => dt2_Row.Field<string>("CBO_CODE"),
                                     (dt1_Row, dt2_Row) => new { dt1_Row, dt2_Row }).ToList()
                                     .ForEach(o => o.dt1_Row.SetField("GMES_USE_FLAG", o.dt2_Row.Field<string>("CBO_CODE")));


                        for (int i = 0; i < displayDt.Rows.Count; i++)
                        {
                            if (string.IsNullOrWhiteSpace(displayDt.Rows[i]["GMES_USE_FLAG"].ToString()))
                            {
                                displayDt.Rows.RemoveAt(i);
                            }
                        }
                        displayDt.Select("GMES_USE_FLAG is null or GMES_USE_FLAG = ''").ToList<DataRow>().ForEach(row => row.Delete());
                        #endregion SLOC GMES_USE_FLAG = 'Y'만 표시

                        #region CLASS 조건 필터링
                        try
                        {
                            DataTable IndataTableMtrlClssCD = new DataTable("INDATA");
                            IndataTableMtrlClssCD.Columns.Add("SHOPID", typeof(string));
                            IndataTableMtrlClssCD.Columns.Add("CODE", typeof(string));

                            DataRow IndataRow = IndataTableMtrlClssCD.NewRow();
                            IndataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            IndataRow["CODE"] = Util.NVC(cboClass.SelectedValue);
                            IndataTableMtrlClssCD.Rows.Add(IndataRow);

                            DataTable dtMtrlClss = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_MTRL_CLSS_CODE", "INDATA", "OUTDATA", IndataTableMtrlClssCD);

                            if (dtMtrlClss != null && dtMtrlClss.Columns.Contains("CLSS_CODE"))
                            {
                                displayDt.AcceptChanges();

                                //for (int i = displayDt.Rows.Count; i > 0; i--)
                                //{
                                //    DataView dv = dtMtrlClss.AsEnumerable().Where(x => displayDt.Rows[i - 1]["PRODID"].ToString().Contains(x.Field<string>("CLSS_CODE"))).AsDataView();

                                //    if (dv.Count < 1)
                                //        displayDt.Rows.RemoveAt(i - 1);
                                //}
                                if (chkGmesChk.IsChecked == true)
                                {
                                    displayDt.AsEnumerable().Where(s => !dtMtrlClss.AsEnumerable().Where(es => s.Field<string>("PRODID").ToString().Contains(es.Field<string>("CLSS_CODE"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                                    displayDt.AcceptChanges();
                                }
                            }
                        }
                        catch (Exception ex3) { }
                        #endregion CLASS 조건 필터링

                        // ASL% 제품은 ROW 삭제
                        if (chkGmesChk.IsChecked == true)
                        {
                            displayDt.Select("PRODID Like 'ASL%' or PRODID Like 'APCSR%'").ToList<DataRow>().ForEach(row => row.Delete());
                            displayDt.Select("MTRLTYPE = 'PROD' and PRODID Not Like 'A%'").ToList<DataRow>().ForEach(row => row.Delete());

                            // MTRL 제외.
                            displayDt.Select("MTRLTYPE = 'MTRL' ").ToList<DataRow>().ForEach(row => row.Delete());
                            // null 이면 RFC 만 존재하는 데이터.. 자재 제외, 위에서 ERP 전용 데이터 조건절 처리 
                            //if (displayDt.Columns.Contains("BKLAS"))
                            //    displayDt.Select("MTRLTYPE is null and BKLAS not like '7%' ").ToList<DataRow>().ForEach(row => row.Delete());

                            displayDt.AcceptChanges();
                        }

                        #region 저장위치 필터링
                        if (Util.NVC(cboStockLocation.SelectedValue).Equals(""))
                        {
                            try
                            {
                                DataTable dtLocation = DataTableConverter.Convert(cboStockLocation.ItemsSource);
                                if (dtLocation != null)
                                {
                                    dtLocation.Select("CBO_CODE = '' OR CBO_CODE IS NULL").ToList<DataRow>().ForEach(row => row.Delete());
                                    dtLocation.AcceptChanges();

                                    displayDt.AsEnumerable().Where(s => !dtLocation.AsEnumerable().Where(c => s.Field<string>("SLOC_ID").ToString().Contains(c.Field<string>("CBO_CODE"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                                    displayDt.AcceptChanges();
                                }
                            }
                            catch (Exception ex10) { }
                        }
                        #endregion

                        #region OrderBy
                        displayDt.AsEnumerable().OrderBy(x => x.Field<string>("PRODID") + x.Field<string>("SLOC_ID"));
                        #endregion OrderBy

                        #region ItemsSource Bind
                        Util.GridSetData(dgMaster, displayDt, FrameOperation);
                        #endregion ItemsSource Bind

                        #region Summaries
                        string[] columnNames = new string[] { "SHOPID", "PRODID", "SLOC_ID" };
                        new Util().SetDataGridMergeExtensionCol(dgMaster, columnNames, DataGridMergeMode.HORIZONTAL);

                        DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                        DataGridAggregateSum daSum = new DataGridAggregateSum();
                        daSum.ResultTemplate = DataGridAggregate.GetDataTemplateFromString("{0}");
                        dac.Add(daSum);

                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["SLOC_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("TOTAL") } });

                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["SUM_QTY2"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["HOLD_LOT_QTY2"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["MOVING_LOT_QTY2"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["TOTAL_QTY2"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["SUM_QTY2_ERP"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["HOLD_LOT_QTY2_ERP"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["MOVING_LOT_QTY2_ERP"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["TOTAL_QTY2_ERP"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["GAP_SUM_QTY2"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["GAP_HOLD_LOT_QTY2"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["GAP_MOVING_LOT_QTY2"], dac);
                        DataGridAggregate.SetAggregateFunctions(dgMaster.Columns["GAP_TOTAL_QTY2"], dac);
                        #endregion Summaries

                        #region 이동중 컬럼 View 처리
                        if (chkMoving.IsChecked == true)
                        {
                            if (dgMaster.Columns.Contains("MOVING_LOT_QTY2"))
                                dgMaster.Columns["MOVING_LOT_QTY2"].Visibility = Visibility.Visible;
                            if (dgMaster.Columns.Contains("MOVING_LOT_QTY2_ERP"))
                                dgMaster.Columns["MOVING_LOT_QTY2_ERP"].Visibility = Visibility.Visible;
                            if (dgMaster.Columns.Contains("GAP_MOVING_LOT_QTY2"))
                                dgMaster.Columns["GAP_MOVING_LOT_QTY2"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            if (dgMaster.Columns.Contains("MOVING_LOT_QTY2"))
                                dgMaster.Columns["MOVING_LOT_QTY2"].Visibility = Visibility.Collapsed;
                            if (dgMaster.Columns.Contains("MOVING_LOT_QTY2_ERP"))
                                dgMaster.Columns["MOVING_LOT_QTY2_ERP"].Visibility = Visibility.Collapsed;
                            if (dgMaster.Columns.Contains("GAP_MOVING_LOT_QTY2"))
                                dgMaster.Columns["GAP_MOVING_LOT_QTY2"].Visibility = Visibility.Collapsed;
                        }
                        #endregion

                        #region 가용/보류 구분 컬럼 View 처리
                        if ((bool)rdoDay.IsChecked)
                        {
                            if (dgMaster.Columns.Contains("HOLD_LOT_QTY2"))
                                dgMaster.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;
                            if (dgMaster.Columns.Contains("HOLD_LOT_QTY2_ERP"))
                                dgMaster.Columns["HOLD_LOT_QTY2_ERP"].Visibility = Visibility.Collapsed;
                            if (dgMaster.Columns.Contains("GAP_HOLD_LOT_QTY2"))
                                dgMaster.Columns["GAP_HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            if (chkAvlPndChk.IsChecked == true)
                            {
                                if (dgMaster.Columns.Contains("HOLD_LOT_QTY2"))
                                    dgMaster.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Visible;
                                if (dgMaster.Columns.Contains("HOLD_LOT_QTY2_ERP"))
                                    dgMaster.Columns["HOLD_LOT_QTY2_ERP"].Visibility = Visibility.Visible;
                                if (dgMaster.Columns.Contains("GAP_HOLD_LOT_QTY2"))
                                    dgMaster.Columns["GAP_HOLD_LOT_QTY2"].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                if (dgMaster.Columns.Contains("HOLD_LOT_QTY2"))
                                    dgMaster.Columns["HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;
                                if (dgMaster.Columns.Contains("HOLD_LOT_QTY2_ERP"))
                                    dgMaster.Columns["HOLD_LOT_QTY2_ERP"].Visibility = Visibility.Collapsed;
                                if (dgMaster.Columns.Contains("GAP_HOLD_LOT_QTY2"))
                                    dgMaster.Columns["GAP_HOLD_LOT_QTY2"].Visibility = Visibility.Collapsed;
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                });
                //}
                #endregion Get GMES Stock List
            }
            catch (Exception ex99)
            {
                loadingIndicator.Visibility = Visibility.Hidden;
            }
        }

        private void GetStockDetail(C1.WPF.DataGrid.DataGridRow dataitem)
        {
            try
            {
                if (IS_NERP_FLAG == true /* && chkNerpApply.IsChecked == true */) // NERP 적용시, (운영 세팅 추가 필요함) 
                {
                    //dest = RfcDestinationManager.GetDestination("ESQ");
                    dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "ESP" : "ESQ");
                }
                else
                {
                    dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "PRD" : "QAS");
                }

                loadingIndicator.Visibility = Visibility.Visible;

                string bizDA = "DA_PRD_SEL_STOCK_SUMMARY2_DETL";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUM_TYPE", typeof(string));
                inTable.Columns.Add("SUM_YM", typeof(string));
                inTable.Columns.Add("SUM_DATE", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SLOC_ID", typeof(string));
                inTable.Columns.Add("FINL_WIP_FLAG", typeof(string));
                inTable.Columns.Add("SUM_INCLUDE_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("SUM_DATE_STRT_END_FLAG", typeof(string));
                inTable.Columns.Add("SUM_STRT_DATE", typeof(string));
                inTable.Columns.Add("SUM_END_DATE", typeof(string));

                if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                {
                    //inTable.Columns.Add("NERP_APPLY_FLAG", typeof(string));
                    bizDA = "DA_PRD_SEL_STOCK_SUMMARY3_DETL";
                }

                DataRow newRow = inTable.NewRow();
                string prodID = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "PRODID"));
                newRow["SUM_TYPE"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_TYPE"));
                newRow["SUM_YM"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_YM"));
                newRow["SUM_DATE"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_DATE"));
                newRow["SHOPID"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SHOPID"));
                newRow["AREAID"] = cboArea.SelectedValue;
                newRow["PRODID"] = prodID;
                newRow["SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SLOC_ID"));
                if (chkFinlwip.IsChecked == true)
                {
                    newRow["FINL_WIP_FLAG"] = "Y";
                }
                else
                {
                    newRow["FINL_WIP_FLAG"] = "N";
                }
                newRow["SUM_INCLUDE_HOLD_FLAG"] = "N";
                newRow["SUM_DATE_STRT_END_FLAG"] = "N";
                newRow["SUM_STRT_DATE"] = null;
                newRow["SUM_END_DATE"] = null;


                //if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                //{
                //    newRow["NERP_APPLY_FLAG"] = "Y";
                //}
                //else
                //{
                //    newRow["NERP_APPLY_FLAG"] = "N";
                //}

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizDA, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // 2024.05.10 (ytkim29) LOT 별로 ERP 와의 재고 차이 표시 
                        // NERP 적용 여부에 따라 컬럼 추가, 제거 
                        if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                        {
                            //dgDetail.Columns["ERP_QTY"].Visibility = Visibility.Visible;
                            dgDetail.Columns["ERP_LABST"].Visibility = Visibility.Visible;
                            //dgDetail.Columns["ERP_SPEME"].Visibility = Visibility.Visible; // 24.07.30: erp에서 보류, 이동 수량 표시 필요없음 
                            //dgDetail.Columns["ERP_UMLME"].Visibility = Visibility.Visible;
                            dgDetail.Columns["MES_ERP_GAP"].Visibility = Visibility.Visible;

                            // 상세조회의 경우: equal 검색 (해당 제품만) 
                            //DataTable dtNerp = searchResult.Copy();
                            DataTable dtNerp;  
                            if (searchResult.Rows.Count == 0)
                            {
                                dtNerp = searchResult.Clone();
                            }
                            else
                            {
                                var collection = searchResult.Select($"PRODID = '{prodID}'");
                                dtNerp = collection.Any() ? collection.CopyToDataTable() : searchResult.Clone();
                            }

                            //dtNerp = searchResult.Select($"PRODID = '{prodID}'").CopyToDataTable();

                            dtNerp.Columns.Add("ERP_LABST", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ERP_SPEME", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ERP_UMLME", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("MES_ERP_GAP", Type.GetType("System.Decimal"));

                            #region GET ERP Data

                            RfcRepository repo = dest.Repository;

                            string rfcName = "ZPPNV_RFC_SEND_ERP_STOCK";
                            string sapTable = "TT_STOCK";


                            if ((bool)rdoDay.IsChecked) // 기일 재고 
                            {
                                rfcName = "ZPPNV_RFC_ERP_BEGIN_END_STOCK";
                                sapTable = "OT_STOCK_GRGI";
                            }

                            IRfcFunction FnStockList = repo.CreateFunction(rfcName);
                            FnStockList.SetValue("IV_WERKS", DataTableConverter.GetValue(dataitem.DataItem, "SHOPID"));
                            FnStockList.SetValue("IV_LGORT", DataTableConverter.GetValue(dataitem.DataItem, "SLOC_ID"));
                            FnStockList.SetValue("IV_MATNR", DataTableConverter.GetValue(dataitem.DataItem, "PRODID"));
                            if ((bool)rdoDay.IsChecked)
                            {
                                FnStockList.SetValue("IV_FLAG", "X");
                                FnStockList.SetValue("IV_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수
                                FnStockList.SetValue("IV_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To
                            }

                            FnStockList.Invoke(dest);

                            var stockList = FnStockList.GetTable(sapTable);
                            DataTable dtERP = stockList.ToDataTable(sapTable);

                            DataTable displayDt = dtNerp.Clone();

                            #region 1. MES, ERP 공통 데이터 처리 

                            if ((bool)rdoDay.IsChecked) // 일 재고, 월재고 
                            {
                                var joinRows = from mes in dtNerp.AsEnumerable()
                                               join erp in dtERP.AsEnumerable()
                                               on new { SLOC_ID = mes.Field<string>("SLOC_ID"), PRODID = mes.Field<string>("PRODID"), LOTID = mes.Field<string>("LOTID") }
                                               equals new { SLOC_ID = erp.Field<string>("LGORT"), PRODID = erp.Field<string>("MATNR"), LOTID = erp.Field<string>("PLOTNO") }
                                               select new
                                               {
                                                   SHOPID = mes.Field<string>("SHOPID"),
                                                   SLOC_ID = mes.Field<string>("SLOC_ID"),
                                                   PRODID = mes.Field<string>("PRODID"),
                                                   LOTID = mes.Field<string>("LOTID"),
                                                   WIPQTY2 = Convert.ToDecimal(mes.Field<double>("WIPQTY2")), // MES 수량 
                                                   SUM_QTY2 = Convert.ToDecimal(mes.Field<double>("SUM_QTY2")), // MES 가용 
                                                   HOLD_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("HOLD_LOT_QTY2")), // MES 보류
                                                   MOVING_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("MOVING_LOT_QTY2")), // MES 이동중 
                                                   TOTAL_QTY2 = Convert.ToDecimal(mes.Field<double>("TOTAL_QTY2")), // MES 합계 
                                                   ZBEGIN_STOCK = erp.Field<decimal>("ZBEGIN_STOCK") // ERP 가용 
                                               };

                                foreach (var item in joinRows)
                                {
                                    displayDt.Rows.Add(
                                                item.SHOPID,
                                                item.LOTID,
                                                item.PRODID,
                                                item.SLOC_ID,
                                                item.WIPQTY2,
                                                item.SUM_QTY2,
                                                item.HOLD_LOT_QTY2,
                                                item.MOVING_LOT_QTY2,
                                                item.TOTAL_QTY2,
                                                item.ZBEGIN_STOCK,
                                                0,
                                                0,
                                                item.TOTAL_QTY2 - item.ZBEGIN_STOCK
                                                );
                                }

                            }
                            else // 현재고
                            {
                                var joinRows = from mes in dtNerp.AsEnumerable()
                                               join erp in dtERP.AsEnumerable()
                                               on new { SLOC_ID = mes.Field<string>("SLOC_ID"), PRODID = mes.Field<string>("PRODID"), LOTID = mes.Field<string>("LOTID") }
                                               equals new { SLOC_ID = erp.Field<string>("LGORT"), PRODID = erp.Field<string>("MATNR"), LOTID = erp.Field<string>("PLOTNO") }
                                               select new
                                               {
                                                   SHOPID = mes.Field<string>("SHOPID"),
                                                   SLOC_ID = mes.Field<string>("SLOC_ID"),
                                                   PRODID = mes.Field<string>("PRODID"),
                                                   LOTID = mes.Field<string>("LOTID"),
                                                   WIPQTY2 = Convert.ToDecimal(mes.Field<double>("WIPQTY2")), // MES 수량 
                                                   SUM_QTY2 = Convert.ToDecimal(mes.Field<double>("SUM_QTY2")), // MES 가용 
                                                   HOLD_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("HOLD_LOT_QTY2")), // MES 보류
                                                   MOVING_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("MOVING_LOT_QTY2")), // MES 이동중 
                                                   TOTAL_QTY2 = Convert.ToDecimal(mes.Field<double>("TOTAL_QTY2")), // MES 합계 
                                                   ERP_LABST = erp.Field<decimal>("LABST"), // ERP 가용 
                                                   ERP_SPEME = erp.Field<decimal>("SPEME"),
                                                   ERP_UMLME = erp.Field<decimal>("UMLME")
                                               };

                                foreach (var item in joinRows)
                                {
                                    displayDt.Rows.Add(
                                                item.SHOPID,
                                                item.LOTID,
                                                item.PRODID,
                                                item.SLOC_ID,
                                                item.WIPQTY2,
                                                item.SUM_QTY2,
                                                item.HOLD_LOT_QTY2,
                                                item.MOVING_LOT_QTY2,
                                                item.TOTAL_QTY2,
                                                item.ERP_LABST,
                                                item.ERP_SPEME,
                                                item.ERP_UMLME,
                                                item.TOTAL_QTY2 - (item.ERP_LABST + item.ERP_SPEME + item.ERP_UMLME)
                                                );
                                }
                            }

                        #endregion

                            #region 2. MES Only (ERP x)

                            var mesOnlyRows = dtNerp.AsEnumerable()
                                    .Where(a => !dtERP.AsEnumerable()
                                    .Any(b => b.Field<string>("WERKS") == a.Field<string>("SHOPID")
                                    && b.Field<string>("LGORT") == a.Field<string>("SLOC_ID")
                                    && b.Field<string>("MATNR") == a.Field<string>("PRODID")
                                    && b.Field<string>("PLOTNO") == a.Field<string>("LOTID")))
                                    .Select(x => new
                                    {
                                        SHOPID = x.Field<string>("SHOPID"),
                                        PRODID = x.Field<string>("PRODID"),
                                        SLOC_ID = x.Field<string>("SLOC_ID"),
                                        LOTID = x.Field<string>("LOTID"),
                                        WIPQTY2 = Convert.ToDecimal(x.Field<double>("WIPQTY2")),
                                        SUM_QTY2 = Convert.ToDecimal(x.Field<double>("SUM_QTY2")),
                                        HOLD_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("HOLD_LOT_QTY2")),
                                        MOVING_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("MOVING_LOT_QTY2")),
                                        TOTAL_QTY2 = Convert.ToDecimal(x.Field<double>("TOTAL_QTY2"))
                                    });

                            foreach (var item in mesOnlyRows)
                            {
                                displayDt.Rows.Add(
                                            item.SHOPID,
                                            item.LOTID,
                                            item.PRODID,
                                            item.SLOC_ID,
                                            item.WIPQTY2,
                                            item.SUM_QTY2,
                                            item.HOLD_LOT_QTY2,
                                            item.MOVING_LOT_QTY2,
                                            item.TOTAL_QTY2,
                                            0,
                                            0,
                                            0,
                                            item.TOTAL_QTY2
                                            );
                            }

                            #endregion 2. MES Only (ERP x)

                            #region 3. ERP Only (MES x)

                            if ((bool)rdoDay.IsChecked) // 일재고, 월재고 
                            {
                                var erpOnlyRows = dtERP.AsEnumerable()
                                    .Where(a => !dtNerp.AsEnumerable()
                                    .Any(b => b.Field<string>("SHOPID") == a.Field<string>("WERKS")
                                    && b.Field<string>("SLOC_ID") == a.Field<string>("LGORT")
                                    && b.Field<string>("PRODID") == a.Field<string>("MATNR")
                                    && b.Field<string>("LOTID") == a.Field<string>("PLOTNO"))
                                    && a.Field<decimal>("ZBEGIN_STOCK") != 0) // 가용수량이 0 이상 
                                    .Select(x => new 
                                    {
                                        SHOPID = x.Field<string>("WERKS"),
                                        PRODID = x.Field<string>("MATNR"),
                                        SLOC_ID = x.Field<string>("LGORT"),
                                        LOTID = x.Field<string>("PLOTNO"),
                                        ZBEGIN_STOCK = x.Field<decimal>("ZBEGIN_STOCK"),
                                        ZBEGIN_INTRANSIT = x.Field<decimal>("ZBEGIN_INTRANSIT")
                                    });

                                foreach (var item in erpOnlyRows)
                                {

                                    displayDt.Rows.Add(
                                            item.SHOPID,
                                            item.LOTID,
                                            item.PRODID,
                                            item.SLOC_ID,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            item.ZBEGIN_STOCK,
                                            0,
                                            item.ZBEGIN_INTRANSIT,
                                            -(item.ZBEGIN_STOCK + item.ZBEGIN_INTRANSIT)
                                            );
                                }
                            }
                            else // 현재고 
                            {
                                var erpOnlyRows = dtERP.AsEnumerable()
                                    .Where(a => !dtNerp.AsEnumerable()
                                    .Any(b => b.Field<string>("SHOPID") == a.Field<string>("WERKS")
                                    && b.Field<string>("SLOC_ID") == a.Field<string>("LGORT")
                                    && b.Field<string>("PRODID") == a.Field<string>("MATNR")
                                    && b.Field<string>("LOTID") == a.Field<string>("PLOTNO"))
                                    && a.Field<decimal>("LABST") != 0)
                                    .Select(x => new
                                    {
                                        SHOPID = x.Field<string>("WERKS"),
                                        PRODID = x.Field<string>("MATNR"),
                                        SLOC_ID = x.Field<string>("LGORT"),
                                        LOTID = x.Field<string>("PLOTNO"),
                                        ERP_LABST = x.Field<decimal>("LABST"), // ERP 가용 
                                        ERP_SPEME = x.Field<decimal>("SPEME"),
                                        ERP_UMLME = x.Field<decimal>("UMLME")
                                    });

                                foreach (var item in erpOnlyRows)
                                {
                                    displayDt.Rows.Add(
                                            item.SHOPID,
                                            item.LOTID,
                                            item.PRODID,
                                            item.SLOC_ID,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            item.ERP_LABST,
                                            item.ERP_SPEME,
                                            item.ERP_UMLME,
                                            -(item.ERP_LABST + item.ERP_SPEME + item.ERP_UMLME)
                                            );
                                }
                            }

                            #endregion

                            #region 기존 로직 (주석 처리) 

                            //foreach (DataRow r in dtNerp.Rows)
                            //{
                            //    DataView dv = dtERP.AsEnumerable()
                            //    .Where(x => x.Field<string>("WERKS") == Util.NVC(r["SHOPID"])
                            //    && x.Field<string>("LGORT") == Util.NVC(r["SLOC_ID"])
                            //    && x.Field<string>("MATNR") == Util.NVC(r["PRODID"])
                            //    && x.Field<string>("PLOTNO") == Util.NVC(r["LOTID"]))
                            //    .AsDataView();

                            //    if (dv.Count == 0) // MES 데이터만 존재함 
                            //    {
                            //        displayDt.Rows.Add(
                            //                Util.NVC(r["SHOPID"]),
                            //                Util.NVC(r["LOTID"]),
                            //                Util.NVC(r["PRODID"]),
                            //                Util.NVC(r["SLOC_ID"]),
                            //                r["WIPQTY2"],
                            //                r["SUM_QTY2"],
                            //                r["HOLD_LOT_QTY2"],
                            //                r["MOVING_LOT_QTY2"],
                            //                r["TOTAL_QTY2"],
                            //                0,
                            //                0,
                            //                0,
                            //                r["TOTAL_QTY2"]
                            //                );
                            //    }
                            //    else // 매핑 ERP LOT 존재 처리 
                            //    {
                            //        foreach (DataRowView rowView in dv) // dv 1건일듯 하나 만일을 위해 loop 처리 
                            //        {
                            //            if (rowView["PLOTNO"].ToString().Equals(r["LOTID"]))
                            //            {
                            //                // 2025.02.05 ytkim29 재고 차이 : mes 합계 - ERP 가용 
                            //                if ((bool)rdoDay.IsChecked)
                            //                {
                            //                    r["ERP_LABST"] = Convert.ToDouble(rowView["ZBEGIN_STOCK"]);
                            //                    r["ERP_UMLME"] = 0; //Convert.ToDouble(rowView["ZBEGIN_INTRANSIT"]);
                            //                    r["ERP_SPEME"] = 0;
                            //                    r["MES_ERP_GAP"] = Convert.ToDouble(r["TOTAL_QTY2"]) - Convert.ToDouble(rowView["ZBEGIN_STOCK"]); 
                            //                    //(Convert.ToDouble(r["SUM_QTY2"]) + Convert.ToDouble(r["MOVING_LOT_QTY2"])) - (Convert.ToDouble(rowView["ZBEGIN_STOCK"]) + Convert.ToDouble(rowView["ZBEGIN_INTRANSIT"]));
                            //                }
                            //                else // 현재고 
                            //                {
                            //                    r["ERP_LABST"] = Convert.ToDouble(rowView["LABST"]);
                            //                    r["ERP_SPEME"] = Convert.ToDouble(rowView["SPEME"]);
                            //                    r["ERP_UMLME"] = Convert.ToDouble(rowView["UMLME"]);

                            //                    r["MES_ERP_GAP"] = (Convert.ToDouble(r["SUM_QTY2"]) + Convert.ToDouble(r["HOLD_LOT_QTY2"]) + Convert.ToDouble(r["MOVING_LOT_QTY2"])) -
                            //                    (Convert.ToDouble(rowView["LABST"]) + Convert.ToDouble(rowView["SPEME"]) + Convert.ToDouble(rowView["UMLME"]));
                            //                }
                            //                displayDt.Rows.Add(
                            //                    Util.NVC(r["SHOPID"]),
                            //                    Util.NVC(r["LOTID"]),
                            //                    Util.NVC(r["PRODID"]),
                            //                    Util.NVC(r["SLOC_ID"]),
                            //                    r["WIPQTY2"],
                            //                    r["SUM_QTY2"],
                            //                    r["HOLD_LOT_QTY2"],
                            //                    r["MOVING_LOT_QTY2"],
                            //                    r["TOTAL_QTY2"],
                            //                    r["ERP_LABST"],
                            //                    r["ERP_SPEME"],
                            //                    r["ERP_UMLME"],
                            //                    r["MES_ERP_GAP"]
                            //                    );
                            //            }


                            //        }// end of foreach (DataRowView rowView in dv), MES 기준 LOOP --> 해당 하는 ERP LOT 데이터 처리  
                            //    } // // 매핑 ERP LOT 존재 처리  끝 

                            //}// end of foreach (DataRow r in dtNerp.Rows)

                            //#region ERP only 재고
                            //string shopId = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SHOPID"));
                            //string prodId = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "PRODID"));
                            //string slocId = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SLOC_ID"));

                            //DataView dv2 = dtERP.AsEnumerable()
                            //    .Where(x => x.Field<string>("WERKS") == shopId
                            //    && x.Field<string>("LGORT") == slocId
                            //    && x.Field<string>("MATNR") == prodId)
                            //    //&& x.Field<string>("PLOTNO") == Util.NVC(r["LOTID"]))
                            //    .AsDataView();

                            //foreach (DataRowView rv in dv2)
                            //{
                            //    DataRow[] foundLotID3 = displayDt.Select("LOTID = '" + rv["PLOTNO"] + "'");
                            //    if (foundLotID3.Length == 0)
                            //    {
                            //        if ((bool)rdoDay.IsChecked && Convert.ToDouble(rv["ZBEGIN_STOCK"]) > 0 ) // ERP 전용이면서 수량 있는것만..
                            //        {
                            //            displayDt.Rows.Add(
                            //                Util.NVC(rv["WERKS"]),
                            //                Util.NVC(rv["PLOTNO"]),
                            //                Util.NVC(rv["MATNR"]),
                            //                Util.NVC(rv["LGORT"]),
                            //                0,
                            //                0,
                            //                0,
                            //                0,
                            //                0,
                            //                rv["ZBEGIN_STOCK"],
                            //                0,
                            //                rv["ZBEGIN_INTRANSIT"],
                            //                -(Convert.ToDouble(rv["ZBEGIN_STOCK"]) + Convert.ToDouble(rv["ZBEGIN_INTRANSIT"]))
                            //                );
                            //        }
                            //        else
                            //        {
                            //            displayDt.Rows.Add(
                            //                Util.NVC(rv["WERKS"]),
                            //                Util.NVC(rv["PLOTNO"]),
                            //                Util.NVC(rv["MATNR"]),
                            //                Util.NVC(rv["LGORT"]),
                            //                0,
                            //                0,
                            //                0,
                            //                0,
                            //                0,
                            //                rv["LABST"],
                            //                rv["SPEME"],
                            //                rv["UMLME"],
                            //                -(Convert.ToDouble(rv["LABST"]) + Convert.ToDouble(rv["SPEME"]) + Convert.ToDouble(rv["UMLME"]))
                            //                );
                            //        }

                            //    }
                            //}
                            //#endregion

                            #endregion

                            Util.GridSetData(dgDetail, displayDt, FrameOperation, true);

                            #endregion
                        } // end of if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                        else
                        {
                            if (searchResult.Rows.Count > 0)
                            {
                                // 상세조회: 해당 제품만 검색: searchResult.Select($"PRODID = '{prodID}'").CopyToDataTable();
                                Util.GridSetData(dgDetail, searchResult.Select($"PRODID = '{prodID}'").CopyToDataTable(), FrameOperation, true);

                                //dgDetail.Columns["ERP_QTY"].Visibility = Visibility.Collapsed;
                                dgDetail.Columns["ERP_LABST"].Visibility = Visibility.Collapsed; // 24.07.30: ERP 에서 보류, 이동 표시 필요없음 
                                                                                                 //dgDetail.Columns["ERP_SPEME"].Visibility = Visibility.Collapsed;
                                                                                                 //dgDetail.Columns["ERP_UMLME"].Visibility = Visibility.Collapsed;
                                dgDetail.Columns["MES_ERP_GAP"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                Util.GridSetData(dgDetail, searchResult, FrameOperation, true);
                            }
                            
                        }

                        //Util.GridSetData(dgDetail, searchResult, FrameOperation, true);


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Hidden;
            }
        }

        #region Total Click: Set MES, ERP Data on Detail
        private void GetDetailAllMesErpData()
        {
            DataTable dtMES, dtMain;
            DataTable dtERP = new DataTable();

            try
            {
                if (dgMaster.GetRowCount() < 1)
                {
                    Util.gridClear(dgDetail);
                    return;
                }

                loadingIndicator.Visibility = Visibility.Visible;

                // main grid list to datarows 
                dtMain = ((DataView)dgMaster.ItemsSource).ToTable();

                //if (IS_NERP_FLAG == true /* && chkNerpApply.IsChecked == true */) // NERP 적용시
                //{
                //    dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "ESP" : "ESQ");
                //}
                //else
                //{
                //    dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "PRD" : "QAS");
                //}

                

                string bizDA = "DA_PRD_SEL_STOCK_SUMMARY2_DETL";

                #region 1: MES INPUT 
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUM_TYPE", typeof(string));
                inTable.Columns.Add("SUM_YM", typeof(string));
                inTable.Columns.Add("SUM_DATE", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SLOC_ID", typeof(string));
                inTable.Columns.Add("FINL_WIP_FLAG", typeof(string));
                inTable.Columns.Add("SUM_INCLUDE_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("SUM_DATE_STRT_END_FLAG", typeof(string));
                inTable.Columns.Add("SUM_STRT_DATE", typeof(string));
                inTable.Columns.Add("SUM_END_DATE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SUM_TYPE"] = _DTL_SUM_TYPE;
                newRow["SUM_YM"] = _DTL_SUM_YM.Equals("") ? null : _DTL_SUM_YM;
                newRow["SUM_DATE"] = _DTL_SUM_DATE.Equals("") ? null : _DTL_SUM_DATE;
                newRow["SHOPID"] = _DTL_SHOPID.Equals("") ? null : _DTL_SHOPID;
                newRow["AREAID"] = _DTL_AREAID.Equals("") ? null : _DTL_AREAID;
                newRow["PRODID"] = _DTL_PRODID.Equals("") ? null : _DTL_PRODID;
                newRow["SLOC_ID"] = _DTL_SLOC_ID.Equals("") ? null : _DTL_SLOC_ID;
                newRow["FINL_WIP_FLAG"] = _DTL_FINL_WIP_FLAG;
                newRow["SUM_INCLUDE_HOLD_FLAG"] = "N";
                newRow["SUM_DATE_STRT_END_FLAG"] = "N";
                newRow["SUM_STRT_DATE"] = null;
                newRow["SUM_END_DATE"] = null;

                if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                {
                    //inTable.Columns.Add("NERP_APPLY_FLAG", typeof(string));
                    bizDA = "DA_PRD_SEL_STOCK_SUMMARY3_DETL";
                }
                inTable.Rows.Add(newRow);

                #endregion

                #region 2: Get MES Data & ERP Data 
                new ClientProxy().ExecuteService(bizDA, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // 1. copy datatable 
                        dtMES = searchResult.Rows.Count > 0 ? searchResult.Copy() : searchResult.Clone();

                        // 2. Get MES Data 
                        dtMES.AcceptChanges();
                        dtMES.AsEnumerable().Where(s => !dtMain.AsEnumerable().Where(es => s.Field<string>("PRODID").ToString().Contains(es.Field<string>("PRODID"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                        dtMES.AcceptChanges();

                        #region 3: Get ERP Data: NERP 적용 & LOT별 비교 체크시에만 필요함 
                        // RfcRepository repo = dest.Repository;

                        string rfcName = string.Empty;
                        string sapTable = string.Empty;

                        DataTable displayDt;
                        // MES DataTable : ERP 수량 표시 컬럼 추가 
                        dtMES.Columns.Add("ERP_LABST", Type.GetType("System.Decimal"));
                        dtMES.Columns.Add("ERP_SPEME", Type.GetType("System.Decimal"));
                        dtMES.Columns.Add("ERP_UMLME", Type.GetType("System.Decimal"));
                        dtMES.Columns.Add("MES_ERP_GAP", Type.GetType("System.Decimal"));
                        // 사전작업, displayDt 스키마 생성, 세부 목록 컬럼 visible 처리 
                        displayDt = dtMES.Clone();
                        dgDetail.Columns["ERP_LABST"].Visibility = Visibility.Visible;
                        dgDetail.Columns["MES_ERP_GAP"].Visibility = Visibility.Visible;

                        if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                        {
 
                            rfcName = "ZPPNV_RFC_SEND_ERP_STOCK";
                            sapTable = "TT_STOCK";

                            if ((bool)rdoDay.IsChecked) // 기일 재고 
                            {
                                rfcName = "ZPPNV_RFC_ERP_BEGIN_END_STOCK";
                                sapTable = "OT_STOCK_GRGI";
                            }

                            // Get ERP Data 
                            for (int i = 0; i < dtMain.Rows.Count; i++)
                            {
                                DataTable dt = new DataTable();
                                dt = AddErpData(rfcName, sapTable, dtMain.Rows[i]["SLOC_ID"].ToString(), dtMain.Rows[i]["PRODID"].ToString());
                                if (dtERP.Columns.Count == 0 && dt.Rows.Count > 0) { dtERP = dt.Clone(); }
                                dt.AsEnumerable().ToList().ForEach(row => dtERP.ImportRow(row));
                            }

                            // MAIN 목록 기준 처리 
                            foreach (DataRow mainRow in dtMain.Rows)
                            {
                                // MES, ERP 데이터 Filtering (창고, 제품) 
                                DataTable dtFilterMes = dtMES.Clone(); 
                                DataTable dtFilterErp = dtERP.Clone();

                                dtMES.AsEnumerable().Where(
                                    row => row.Field<string>("SHOPID") == mainRow["SHOPID"].ToString()
                                    && row.Field<string>("SLOC_ID") == mainRow["SLOC_ID"].ToString()
                                    && row.Field<string>("PRODID") == mainRow["PRODID"].ToString())
                                    .ToList().ForEach(e => dtFilterMes.ImportRow(e));

                                dtERP.AsEnumerable().Where(
                                    row => row.Field<string>("WERKS") == mainRow["SHOPID"].ToString()
                                    && row.Field<string>("LGORT") == mainRow["SLOC_ID"].ToString()
                                    && row.Field<string>("MATNR") == mainRow["PRODID"].ToString())
                                    .ToList().ForEach(e => dtFilterErp.ImportRow(e));


                               


                                // 1) MES, ERP 공통 LOT  --> 조인 
                                if ((bool)rdoDay.IsChecked) // 일재고
                                {
                                    var joinRows = from mes in dtFilterMes.AsEnumerable()
                                                    join erp in dtFilterErp.AsEnumerable()
                                                    on mes.Field<string>("LOTID") equals erp.Field<string>("PLOTNO")
                                                    select new {
                                                        SHOPID = mes.Field<string>("SHOPID"),
                                                        SLOC_ID = mes.Field<string>("SLOC_ID"),
                                                        PRODID = mes.Field<string>("PRODID"),
                                                        LOTID = mes.Field<string>("LOTID"),
                                                        WIPQTY2 = Convert.ToDecimal(mes.Field<double>("WIPQTY2")),
                                                        SUM_QTY2 = Convert.ToDecimal(mes.Field<double>("SUM_QTY2")),
                                                        HOLD_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("HOLD_LOT_QTY2")),
                                                        MOVING_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("MOVING_LOT_QTY2")),
                                                        TOTAL_QTY2 = Convert.ToDecimal(mes.Field<double>("TOTAL_QTY2")),
                                                        ZBEGIN_STOCK = erp.Field<decimal>("ZBEGIN_STOCK"),
                                                        ZBEGIN_INTRANSIT = erp.Field<decimal>("ZBEGIN_INTRANSIT")
                                                    };

                                    foreach (var row in joinRows)
                                    {
                                        displayDt.Rows.Add(
                                                row.SHOPID,
                                                row.LOTID,
                                                row.PRODID,
                                                row.SLOC_ID,
                                                row.WIPQTY2,
                                                row.SUM_QTY2,
                                                row.HOLD_LOT_QTY2,
                                                row.MOVING_LOT_QTY2,
                                                row.TOTAL_QTY2,
                                                row.ZBEGIN_STOCK,
                                                row.ZBEGIN_INTRANSIT,
                                                0,
                                                (row.SUM_QTY2 + row.MOVING_LOT_QTY2) -
                                                     (row.ZBEGIN_STOCK + row.ZBEGIN_INTRANSIT)
                                                );
                                    }

                                }
                                else // 현재고, 월재고 
                                {
                                    var joinRows = from mes in dtFilterMes.AsEnumerable()
                                                   join erp in dtFilterErp.AsEnumerable()
                                                   on mes.Field<string>("LOTID") equals erp.Field<string>("PLOTNO")
                                                   select new {
                                                        SHOPID = mes.Field<string>("SHOPID"),
                                                        SLOC_ID = mes.Field<string>("SLOC_ID"),
                                                        PRODID = mes.Field<string>("PRODID"),
                                                        LOTID = mes.Field<string>("LOTID"),
                                                        WIPQTY2 = Convert.ToDecimal(mes.Field<double>("WIPQTY2")),
                                                        SUM_QTY2 = Convert.ToDecimal(mes.Field<double>("SUM_QTY2")),
                                                        HOLD_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("HOLD_LOT_QTY2")),
                                                        MOVING_LOT_QTY2 = Convert.ToDecimal(mes.Field<double>("MOVING_LOT_QTY2")),
                                                        TOTAL_QTY2 = Convert.ToDecimal(mes.Field<double>("TOTAL_QTY2")),
                                                        LABST = erp.Field<decimal>("LABST"),
                                                        SPEME = erp.Field<decimal>("SPEME"),
                                                        UMLME = erp.Field<decimal>("UMLME")
                                                    };

                                    foreach (var row in joinRows)
                                    {
                                        displayDt.Rows.Add(
                                                row.SHOPID,
                                                row.LOTID,
                                                row.PRODID,
                                                row.SLOC_ID,
                                                row.WIPQTY2,
                                                row.SUM_QTY2,
                                                row.HOLD_LOT_QTY2,
                                                row.MOVING_LOT_QTY2,
                                                row.TOTAL_QTY2,
                                                row.LABST,
                                                row.SPEME,
                                                row.UMLME,
                                                (row.SUM_QTY2 + row.HOLD_LOT_QTY2 + row.MOVING_LOT_QTY2) -
                                                     (row.LABST + row.SPEME + row.UMLME)
                                                );
                                    }

                                }// if ((bool)rdoDay.IsChecked)


                                // 2) MES Only (ERP x)
                                var mesOnlyRows = dtFilterMes.AsEnumerable()
                                    .Where(a => !dtFilterErp.AsEnumerable()
                                    .Any(b => b.Field<string>("PLOTNO") == a.Field<string>("LOTID")))
                                    .Select(x => new
                                    {
                                        SHOPID = x.Field<string>("SHOPID"),
                                        LOTID = x.Field<string>("LOTID"),
                                        PRODID = x.Field<string>("PRODID"),
                                        SLOC_ID = x.Field<string>("SLOC_ID"),
                                        WIPQTY2 = Convert.ToDecimal(x.Field<double>("WIPQTY2")),
                                        SUM_QTY2 = Convert.ToDecimal(x.Field<double>("SUM_QTY2")),
                                        HOLD_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("HOLD_LOT_QTY2")),
                                        MOVING_LOT_QTY2 = Convert.ToDecimal(x.Field<double>("MOVING_LOT_QTY2")),
                                        TOTAL_QTY2 = Convert.ToDecimal(x.Field<double>("TOTAL_QTY2"))
                                    });


                                foreach (var row in mesOnlyRows)
                                {
                                    displayDt.Rows.Add(
                                                row.SHOPID,
                                                row.LOTID,
                                                row.PRODID,
                                                row.SLOC_ID,
                                                row.WIPQTY2,
                                                row.SUM_QTY2,
                                                row.HOLD_LOT_QTY2,
                                                row.MOVING_LOT_QTY2,
                                                row.TOTAL_QTY2,
                                                0,
                                                0,
                                                0,
                                                row.TOTAL_QTY2
                                                );
                                };

                                // 3) ERP Only (MES x) 
                             

                                if ((bool)rdoDay.IsChecked) // 일재고
                                {
                                    var erpOnlyRows = dtFilterErp.AsEnumerable()
                                    .Where(a => !dtFilterMes.AsEnumerable()
                                    .Any(b => b.Field<string>("LOTID") == a.Field<string>("PLOTNO")))
                                    .Select(x => new
                                    {
                                        WERKS = x.Field<string>("WERKS"),
                                        PLOTNO = x.Field<string>("PLOTNO"),
                                        MATNR = x.Field<string>("MATNR"),
                                        LGORT = x.Field<string>("LGORT"),
                                        ZBEGIN_STOCK = x.Field<decimal>("ZBEGIN_STOCK"),
                                        ZBEGIN_INTRANSIT = x.Field<decimal>("ZBEGIN_INTRANSIT")
                                    });
                                    foreach (var row in erpOnlyRows)
                                    {
                                        displayDt.Rows.Add(
                                            row.WERKS,
                                            row.PLOTNO,
                                            row.MATNR,
                                            row.LGORT,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            row.ZBEGIN_STOCK,
                                            0,
                                            row.ZBEGIN_INTRANSIT,
                                            -(row.ZBEGIN_STOCK + row.ZBEGIN_INTRANSIT)
                                            );
                                    }
                                }
                                else // 현재고, 월재고 
                                {
                                    var erpOnlyRows = dtFilterErp.AsEnumerable()
                                    .Where(a => !dtFilterMes.AsEnumerable()
                                    .Any(b => b.Field<string>("LOTID") == a.Field<string>("PLOTNO")))
                                    .Select(x => new
                                    {
                                        WERKS = x.Field<string>("WERKS"),
                                        PLOTNO = x.Field<string>("PLOTNO"),
                                        MATNR = x.Field<string>("MATNR"),
                                        LGORT = x.Field<string>("LGORT"),
                                        LABST = x.Field<decimal>("LABST"),
                                        SPEME = x.Field<decimal>("SPEME"),
                                        UMLME = x.Field<decimal>("UMLME")

                                    });

                                    foreach (var row in erpOnlyRows)
                                    {
                                        displayDt.Rows.Add(
                                            row.WERKS,
                                            row.PLOTNO,
                                            row.MATNR,
                                            row.LGORT,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            row.LABST,
                                            row.SPEME,
                                            row.UMLME,
                                            -(row.LABST + row.SPEME + row.UMLME)
                                            );
                                    }
                                }

                            }

                            Util.GridSetData(dgDetail, displayDt, FrameOperation, true);

                        }// end of if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                        else
                        {
                            if (searchResult.Rows.Count > 0)
                            {
                                Util.GridSetData(dgDetail, dtMES, FrameOperation, true);

                                dgDetail.Columns["ERP_LABST"].Visibility = Visibility.Collapsed; 
                                dgDetail.Columns["MES_ERP_GAP"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                Util.GridSetData(dgDetail, dtMES, FrameOperation, true);
                            }
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                }); // end of Get [MES Data] ClientProxy().ExecuteService
                #endregion

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Hidden;
            }
            //finally
            //{
            //    loadingIndicator.Visibility = Visibility.Hidden;
            //}

        }
        #endregion

        #region Add ERP Data (main grid loop)
        private DataTable AddErpData(string rfcName, string sapTable, string slocID, string prodID)
        {
            RfcRepository repo = dest.Repository;

            IRfcFunction FnStockList = repo.CreateFunction(rfcName);
            FnStockList.SetValue("IV_WERKS", _DTL_SHOPID);
            FnStockList.SetValue("IV_LGORT", slocID);
            FnStockList.SetValue("IV_MATNR", prodID);
            if ((bool)rdoDay.IsChecked)
            {
                FnStockList.SetValue("IV_FLAG", "X");
                FnStockList.SetValue("IV_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수
                FnStockList.SetValue("IV_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To
            }

            FnStockList.Invoke(dest);
            var stockList = FnStockList.GetTable(sapTable);
            return stockList.ToDataTable(sapTable);
        }

        #endregion



        private void GetStockDetailAll(string sumDate)
        {
            try
            {
                if (dgMaster.GetRowCount() < 1)
                {
                    Util.gridClear(dgDetail);
                    return;
                }

                if (IS_NERP_FLAG == true /* && chkNerpApply.IsChecked == true */) // NERP 적용시, (운영 세팅 추가 필요함) 
                {
                    //dest = RfcDestinationManager.GetDestination("ESQ");
                    dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "ESP" : "ESQ");
                }
                else
                {
                    dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "PRD" : "QAS");
                }

                loadingIndicator.Visibility = Visibility.Visible;

                string bizDA = "DA_PRD_SEL_STOCK_SUMMARY2_DETL";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUM_TYPE", typeof(string));
                inTable.Columns.Add("SUM_YM", typeof(string));
                inTable.Columns.Add("SUM_DATE", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("SLOC_ID", typeof(string));
                inTable.Columns.Add("FINL_WIP_FLAG", typeof(string));
                inTable.Columns.Add("SUM_INCLUDE_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("SUM_DATE_STRT_END_FLAG", typeof(string));
                inTable.Columns.Add("SUM_STRT_DATE", typeof(string));
                inTable.Columns.Add("SUM_END_DATE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SUM_TYPE"] = _DTL_SUM_TYPE;
                newRow["SUM_YM"] = _DTL_SUM_YM.Equals("") ? null : _DTL_SUM_YM;
                newRow["SUM_DATE"] = sumDate; //_DTL_SUM_DATE.Equals("") ? null : _DTL_SUM_DATE;
                newRow["SHOPID"] = _DTL_SHOPID.Equals("") ? null : _DTL_SHOPID;
                newRow["AREAID"] = _DTL_AREAID.Equals("") ? null : _DTL_AREAID;
                newRow["PRODID"] = _DTL_PRODID.Equals("") ? null : _DTL_PRODID;
                newRow["SLOC_ID"] = _DTL_SLOC_ID.Equals("") ? null : _DTL_SLOC_ID;
                newRow["FINL_WIP_FLAG"] = _DTL_FINL_WIP_FLAG;
                newRow["SUM_INCLUDE_HOLD_FLAG"] = "N";
                newRow["SUM_DATE_STRT_END_FLAG"] = "N";
                newRow["SUM_STRT_DATE"] = null;
                newRow["SUM_END_DATE"] = null;

                if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                {
                    //inTable.Columns.Add("NERP_APPLY_FLAG", typeof(string));
                    bizDA = "DA_PRD_SEL_STOCK_SUMMARY3_DETL";
                }

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizDA, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        string prodID = inTable.Rows[0]["PRODID"] == null ? string.Empty : inTable.Rows[0]["PRODID"].ToString();
                        string slocID = inTable.Rows[0]["SLOC_ID"] == null ? string.Empty : inTable.Rows[0]["SLOC_ID"].ToString();

                        if (IS_NERP_FLAG == true && chkNerpApply.IsChecked == true)
                        {
                            dgDetail.Columns["ERP_LABST"].Visibility = Visibility.Visible;
                            dgDetail.Columns["MES_ERP_GAP"].Visibility = Visibility.Visible;

                            DataTable dtNerp;
                            

                            if (searchResult.Rows.Count == 0)
                            {
                                dtNerp = searchResult.Clone();
                            }
                            else
                            {
                                var collection = prodID == string.Empty ? searchResult.Rows.Cast<DataRow>().ToArray() : searchResult.Select($"PRODID = '{prodID}'");
                                dtNerp = collection.Any() ? collection.CopyToDataTable() : searchResult.Clone();
                            }

                            dtNerp.Columns.Add("ERP_LABST", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ERP_SPEME", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("ERP_UMLME", Type.GetType("System.Decimal"));
                            dtNerp.Columns.Add("MES_ERP_GAP", Type.GetType("System.Decimal"));

                            #region Get ERP Data

                            RfcRepository repo = dest.Repository;

                            string rfcName = "ZPPNV_RFC_SEND_ERP_STOCK";
                            string sapTable = "TT_STOCK";


                            if ((bool)rdoDay.IsChecked) // 기일 재고 
                            {
                                rfcName = "ZPPNV_RFC_ERP_BEGIN_END_STOCK";
                                sapTable = "OT_STOCK_GRGI";
                            }

                            IRfcFunction FnStockList = repo.CreateFunction(rfcName);
                            FnStockList.SetValue("IV_WERKS", _DTL_SHOPID); // shopid는 항상 존재함 
                            if (!String.IsNullOrEmpty(slocID)) { FnStockList.SetValue("IV_LGORT", slocID); }
                            if (!String.IsNullOrEmpty(prodID)) { FnStockList.SetValue("IV_MATNR", prodID); } 

                            if ((bool)rdoDay.IsChecked)
                            {
                                FnStockList.SetValue("IV_FLAG", "X");
                                FnStockList.SetValue("IV_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수
                                FnStockList.SetValue("IV_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To
                            }

                            FnStockList.Invoke(dest);

                            var stockList = FnStockList.GetTable(sapTable);
                            DataTable dtERP = stockList.ToDataTable(sapTable);

                            DataTable displayDt = dtNerp.Clone();
                            foreach (DataRow r in dtNerp.Rows)
                            {
                                DataView dv = dtERP.AsEnumerable()
                                .Where(x => x.Field<string>("WERKS") == Util.NVC(r["SHOPID"])
                                && x.Field<string>("LGORT") == Util.NVC(r["SLOC_ID"])
                                && x.Field<string>("MATNR") == Util.NVC(r["PRODID"])
                                && x.Field<string>("PLOTNO") == Util.NVC(r["LOTID"]))
                                .AsDataView();

                                if (dv.Count == 0) // MES 데이터만 존재함 
                                {
                                    displayDt.Rows.Add(
                                            Util.NVC(r["SHOPID"]),
                                            Util.NVC(r["LOTID"]),
                                            Util.NVC(r["PRODID"]),
                                            Util.NVC(r["SLOC_ID"]),
                                            r["WIPQTY2"],
                                            r["SUM_QTY2"],
                                            r["HOLD_LOT_QTY2"],
                                            r["MOVING_LOT_QTY2"],
                                            r["TOTAL_QTY2"],
                                            0,
                                            0,
                                            0,
                                            r["TOTAL_QTY2"]
                                            );
                                }
                                else // 매핑 ERP LOT 존재 처리 
                                {
                                    foreach (DataRowView rowView in dv) // dv 1건일듯 하나 만일을 위해 loop 처리 
                                    {
                                        if (rowView["PLOTNO"].ToString().Equals(r["LOTID"]))
                                        {
                                            // 2025.02.05: 재고 차이: mes 합계 - ERP 가용 
                                            if ((bool)rdoDay.IsChecked)
                                            {
                                                r["ERP_LABST"] = Convert.ToDouble(rowView["ZBEGIN_STOCK"]);
                                                r["ERP_UMLME"] = 0; //Convert.ToDouble(rowView["ZBEGIN_INTRANSIT"]);
                                                r["ERP_SPEME"] = 0;
                                                r["MES_ERP_GAP"] = Convert.ToDouble(r["TOTAL_QTY2"]) - Convert.ToDouble(rowView["ZBEGIN_STOCK"]);
                                                //(Convert.ToDouble(r["SUM_QTY2"]) + Convert.ToDouble(r["MOVING_LOT_QTY2"])) - (Convert.ToDouble(rowView["ZBEGIN_STOCK"]) + Convert.ToDouble(rowView["ZBEGIN_INTRANSIT"]));
                                            }
                                            else // 현재고 
                                            {
                                                r["ERP_LABST"] = Convert.ToDouble(rowView["LABST"]);
                                                r["ERP_SPEME"] = Convert.ToDouble(rowView["SPEME"]);
                                                r["ERP_UMLME"] = Convert.ToDouble(rowView["UMLME"]);

                                                r["MES_ERP_GAP"] = (Convert.ToDouble(r["SUM_QTY2"]) + Convert.ToDouble(r["HOLD_LOT_QTY2"]) + Convert.ToDouble(r["MOVING_LOT_QTY2"])) -
                                                (Convert.ToDouble(rowView["LABST"]) + Convert.ToDouble(rowView["SPEME"]) + Convert.ToDouble(rowView["UMLME"]));
                                            }
                                            displayDt.Rows.Add(
                                                Util.NVC(r["SHOPID"]),
                                                Util.NVC(r["LOTID"]),
                                                Util.NVC(r["PRODID"]),
                                                Util.NVC(r["SLOC_ID"]),
                                                r["WIPQTY2"],
                                                r["SUM_QTY2"],
                                                r["HOLD_LOT_QTY2"],
                                                r["MOVING_LOT_QTY2"],
                                                r["TOTAL_QTY2"],
                                                r["ERP_LABST"],
                                                r["ERP_SPEME"],
                                                r["ERP_UMLME"],
                                                r["MES_ERP_GAP"]
                                                );
                                        }
                                    }// end of foreach (DataRowView rowView in dv), MES 기준 LOOP --> 해당 하는 ERP LOT 데이터 처리  
                                } // 매핑 ERP LOT 존재 처리  끝 

                            }// end of foreach (DataRow r in dtNerp.Rows)

                            #region ERP only 재고
                            

                            DataView dv2 = dtERP.AsEnumerable()
                                .Where(x => x.Field<string>("WERKS") == _DTL_SHOPID
                                && x.Field<string>("LGORT") == slocID
                                && x.Field<string>("MATNR") == prodID)
                                //&& x.Field<string>("PLOTNO") == Util.NVC(r["LOTID"]))
                                .AsDataView();

                            foreach (DataRowView rv in dv2)
                            {
                                DataRow[] foundLotID3 = displayDt.Select("LOTID = '" + rv["PLOTNO"] + "'");
                                if (foundLotID3.Length == 0)
                                {
                                    if ((bool)rdoDay.IsChecked)
                                    {
                                        displayDt.Rows.Add(
                                            Util.NVC(rv["WERKS"]),
                                            Util.NVC(rv["PLOTNO"]),
                                            Util.NVC(rv["MATNR"]),
                                            Util.NVC(rv["LGORT"]),
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            rv["ZBEGIN_STOCK"],
                                            0,
                                            rv["ZBEGIN_INTRANSIT"],
                                            -(Convert.ToDouble(rv["ZBEGIN_STOCK"]) + Convert.ToDouble(rv["ZBEGIN_INTRANSIT"]))
                                            );
                                    }
                                    else
                                    {
                                        displayDt.Rows.Add(
                                            Util.NVC(rv["WERKS"]),
                                            Util.NVC(rv["PLOTNO"]),
                                            Util.NVC(rv["MATNR"]),
                                            Util.NVC(rv["LGORT"]),
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            rv["LABST"],
                                            rv["SPEME"],
                                            rv["UMLME"],
                                            -(Convert.ToDouble(rv["LABST"]) + Convert.ToDouble(rv["SPEME"]) + Convert.ToDouble(rv["UMLME"]))
                                            );
                                    }

                                }
                            }
                            #endregion
                            Util.GridSetData(dgDetail, displayDt, FrameOperation, true);

                            #endregion
                        }
                        else // 기존 DA_PRD_SEL_STOCK_SUMMARY2_DETL 로직 
                        {
                            if (searchResult.Rows.Count > 0)
                            {
                                // 상세조회: 해당 제품만 검색: searchResult.Select($"PRODID = '{prodID}'").CopyToDataTable();
                                Util.GridSetData(dgDetail, searchResult.Select($"PRODID = '{prodID}'").CopyToDataTable(), FrameOperation, true);

                                //dgDetail.Columns["ERP_QTY"].Visibility = Visibility.Collapsed;
                                dgDetail.Columns["ERP_LABST"].Visibility = Visibility.Collapsed; // 24.07.30: ERP 에서 보류, 이동 표시 필요없음 
                                                                                                 //dgDetail.Columns["ERP_SPEME"].Visibility = Visibility.Collapsed;
                                                                                                 //dgDetail.Columns["ERP_UMLME"].Visibility = Visibility.Collapsed;
                                dgDetail.Columns["MES_ERP_GAP"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                Util.GridSetData(dgDetail, searchResult, FrameOperation, true);
                            }
                        }

                        #region NERP 적용전 (주석처리) 2025.01.14 (ytkim29)
                            /*
                            #region SLOC GMES_USE_FLAG = 'Y'만 표시
                            DataTable IndataTableSLOC = new DataTable("INDATA");
                            IndataTableSLOC.Columns.Add("SHOPID", typeof(string));
                            IndataTableSLOC.Columns.Add("AREAID", typeof(string));

                            DataRow IndataSLOC = IndataTableSLOC.NewRow();
                            IndataSLOC["SHOPID"] = _DTL_SHOPID.Equals("") ? null : _DTL_SHOPID;
                            IndataSLOC["AREAID"] = _DTL_AREAID.Equals("") ? null : _DTL_AREAID;
                            IndataTableSLOC.Rows.Add(IndataSLOC);

                            DataTable dtSLOC = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SLOC_BY_GMES_USE", "INDATA", "RSLTDT", IndataTableSLOC);

                            dt.Columns.Add("GMES_USE_FLAG");

                            // GMES_USE_FLAG Update후 GMES_USE_FLAG 없는경우 SLOC_ID Clear
                            dt.AsEnumerable().Join(dtSLOC.AsEnumerable(),
                                         dt1_Row => dt1_Row.Field<string>("SLOC_ID"),
                                         dt2_Row => dt2_Row.Field<string>("CBO_CODE"),
                                         (dt1_Row, dt2_Row) => new { dt1_Row, dt2_Row }).ToList()
                                         .ForEach(o => o.dt1_Row.SetField("GMES_USE_FLAG", o.dt2_Row.Field<string>("CBO_CODE")));


                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(dt.Rows[i]["GMES_USE_FLAG"].ToString()))
                                {
                                    dt.Rows.RemoveAt(i);
                                }
                            }
                            dt.Select("GMES_USE_FLAG is null or GMES_USE_FLAG = ''").ToList<DataRow>().ForEach(row => row.Delete());
                            #endregion SLOC GMES_USE_FLAG = 'Y'만 표시

                            #region CLASS 조건 필터링
                            if (_DTL_CHK_GMES_CHK)
                            {
                                try
                                {
                                    DataTable IndataTableMtrlClssCD = new DataTable("INDATA");
                                    IndataTableMtrlClssCD.Columns.Add("CODE", typeof(string));

                                    DataRow IndataRow = IndataTableMtrlClssCD.NewRow();
                                    IndataRow["CODE"] = _DTL_CBO_CLASS;
                                    IndataTableMtrlClssCD.Rows.Add(IndataRow);

                                    DataTable dtMtrlClss = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_CLSS_CODE", "INDATA", "RSLTDT", IndataTableMtrlClssCD);

                                    if (dtMtrlClss != null && dtMtrlClss.Columns.Contains("CLSS_CODE"))
                                    {
                                        dt.AcceptChanges();

                                        dt.AsEnumerable().Where(s => !dtMtrlClss.AsEnumerable().Where(es => s.Field<string>("PRODID").ToString().Contains(es.Field<string>("CLSS_CODE"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                                        dt.AcceptChanges();

                                    }
                                }
                                catch (Exception ex3) { }
                            }
                            #endregion CLASS 조건 필터링

                            #region 저장위치 필터링
                            if (Util.NVC(cboStockLocation.SelectedValue).Equals(""))
                            {
                                try
                                {
                                    DataTable dtLocation = DataTableConverter.Convert(cboStockLocation.ItemsSource);
                                    if (dtLocation != null)
                                    {
                                        dtLocation.Select("CBO_CODE = '' OR CBO_CODE IS NULL").ToList<DataRow>().ForEach(row => row.Delete());
                                        dtLocation.AcceptChanges();

                                        dt.AsEnumerable().Where(s => !dtLocation.AsEnumerable().Where(c => s.Field<string>("SLOC_ID").ToString().Contains(c.Field<string>("CBO_CODE"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                                        dt.AcceptChanges();
                                    }
                                }
                                catch (Exception ex10) { }
                            }
                            #endregion

                            #region 재고 목록의 제품ID 및 저장위치 필터링
                            try
                            {
                                DataTable dtMaster = DataTableConverter.Convert(dgMaster.ItemsSource);

                                if (dtMaster != null && dtMaster?.Rows?.Count > 0 && dtMaster.Columns.Contains("PRODID") && dtMaster.Columns.Contains("SLOC_ID"))
                                {
                                    dt.AcceptChanges();

                                    dt.AsEnumerable().Where(s => !dtMaster.AsEnumerable().Where(es => s.Field<string>("PRODID").ToString().Contains(es.Field<string>("PRODID")) && s.Field<string>("SLOC_ID").ToString().Contains(es.Field<string>("SLOC_ID"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                                    dt.AcceptChanges();
                                }
                            }
                            catch (Exception ex11) { }
                            #endregion
                            */
                            #endregion

                        //Util.GridSetData(dgDetail, dt, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Hidden;
            }
        }

        IRfcTable GetTableByRfcCall(string destName, int rowCount)
        {
            RfcDestination dest = RfcDestinationManager.GetDestination(destName);
            IRfcFunction func = dest.Repository.CreateFunction("ZPPB_SEND_ERP_STOCK");

            func.SetValue("I_WERKS", cboShop.SelectedValue);
            func.SetValue("I_LGORT", cboStockLocation.SelectedValue);
            func.SetValue("I_MATNR", txtProdId.Text.Trim());

            IRfcTable rfcTable = func.GetTable("IT_STOCK");

            return rfcTable;
        }

        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //SetComboBox(cboStockLocation);
            SetComboBox(cboArea);
        }

        private void chkDiff_Checked(object sender, RoutedEventArgs e)
        {
            GetStocks();
        }

        private void chkDiff_Unchecked(object sender, RoutedEventArgs e)
        {
            GetStocks();
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetComboBox(cboStockLocation);
        }

        private void rdoCurrent_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpDate != null)
                    dtpDate.IsEnabled = false;
                if (dtpMonth != null)
                    dtpMonth.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoDay_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpDate != null)
                    dtpDate.IsEnabled = true;
                if (dtpMonth != null)
                    dtpMonth.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoMonth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpDate != null)
                    dtpDate.IsEnabled = false;
                if (dtpMonth != null)
                    dtpMonth.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMaster_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Util.gridClear(dgDetail);

                    C1DataGrid dg = sender as C1DataGrid;

                    if (dg == null) return;

                    C1.WPF.DataGrid.DataGridCell currCell = dg.CurrentCell;

                    if (currCell == null || currCell.Presenter == null || currCell.Presenter.Content == null) return;

                    if (DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SHOPID") == null &&
                       DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "PRODID") == null &&
                       DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SLOC_ID") == null)
                    {
                        //string sumDate = dg.GetDataRow(dg.TopRows.Count)["SUM_DATE"].ToString();
                        //GetStockDetailAll(sumDate); // 실행조건 확인 필요함 

                        GetDetailAllMesErpData();
                    }
                    else
                    {
                        GetStockDetail(dg.CurrentCell.Row);
                    }

                    //if (e.LeftButton == MouseButtonState.Pressed &&
                    //    (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                    //    (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                    //    (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    //{
                    if (Util.NVC(currCell.Column.Name).Equals("PRODID"))
                    {
                        COM001_403_CMP_DAY wndPopup = new COM001_403_CMP_DAY();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[12];
                            Parameters[0] = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SHOPID"));
                            Parameters[1] = Util.NVC(cboShop.Text);
                            Parameters[2] = Util.NVC(cboArea.SelectedValue);
                            Parameters[3] = Util.NVC(cboArea.Text);
                            Parameters[4] = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SLOC_ID"));
                            Parameters[5] = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SLOC_ID"));
                            Parameters[6] = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "PRODID"));

                            string sSum_Type = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SUM_TYPE"));
                            if (sSum_Type.Equals("CURR"))
                            {
                                Parameters[7] = sSum_Type;
                                Parameters[8] = "";
                            }
                            else if (sSum_Type.Equals("DAY"))
                            {
                                Parameters[7] = sSum_Type;
                                Parameters[8] = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SUM_DATE"));
                            }
                            else if (sSum_Type.Equals("MONTH"))
                            {
                                Parameters[7] = sSum_Type;
                                Parameters[8] = Util.NVC(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, "SUM_YM"));
                            }
                            else
                            {
                                if ((bool)rdoCurrent.IsChecked)
                                {
                                    Parameters[7] = Util.NVC(rdoCurrent.Tag);
                                    Parameters[8] = "";
                                }
                                else if ((bool)rdoDay.IsChecked)
                                {
                                    Parameters[7] = Util.NVC(rdoDay.Tag);
                                    Parameters[8] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                                }
                                else if ((bool)rdoMonth.IsChecked)
                                {
                                    Parameters[7] = Util.NVC(rdoMonth.Tag);
                                    Parameters[8] = dtpMonth.SelectedDateTime.ToString("yyyyMM");
                                }
                            }

                            Parameters[9] = chkMoving.IsChecked;
                            Parameters[10] = chkFinlwip.IsChecked;
                            // 2024.05.13, NERP FLAG = Y: NERP 적용 여부 체크 
                            Parameters[11] = chkNerpApply.IsChecked;

                            C1WindowExtension.SetParameters(wndPopup, Parameters);

                            wndPopup.Closed += new EventHandler(wndPopup_Closed);
                            this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                        }
                    }
                    //}                    
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));

        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_403_CMP_DAY wndPopup = sender as COM001_403_CMP_DAY;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkMoving_Checked(object sender, RoutedEventArgs e)
        {
            GetStocks();
        }

        private void chkMoving_Unchecked(object sender, RoutedEventArgs e)
        {
            GetStocks();
        }

        private void chkLossScrap_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkLossScrap_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void chkGmesChk_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkGmesChk_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void chkAvlPndChk_Checked(object sender, RoutedEventArgs e)
        {
            GetStocks();
        }

        private void chkAvlPndChk_Unchecked(object sender, RoutedEventArgs e)
        {
            GetStocks();
        }

        private void dgDetail_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv != null)
            {
                if (dgDetail.Columns["TOTAL_QTY2"].Visibility == Visibility.Visible 
                    && dgDetail.Columns["ERP_LABST"].Visibility == Visibility.Visible 
                    && drv.DataView.Table.Columns.Contains("ERP_LABST")
                    && drv["ERP_LABST"] != DBNull.Value
                    && !Convert.ToDouble(drv["TOTAL_QTY2"]).Equals(Convert.ToDouble(drv["ERP_LABST"])))
                    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else if (dgDetail.Columns["HOLD_LOT_QTY2"].Visibility == Visibility.Visible && dgDetail.Columns["HOLD_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["HOLD_LOT_QTY2"]).Equals(Convert.ToDouble(drv["HOLD_LOT_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                //else if (dgDetail.Columns["MOVING_LOT_QTY2"].Visibility == Visibility.Visible && dgDetail.Columns["MOVING_LOT_QTY2_ERP"].Visibility == Visibility.Visible && !Convert.ToDouble(drv["MOVING_LOT_QTY2"]).Equals(Convert.ToDouble(drv["MOVING_LOT_QTY2_ERP"])))
                //    e.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                else
                    e.Row.Presenter.Background = new SolidColorBrush(Colors.White);
            }
        }

        //private void chkNerpApply_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (dgMaster.ItemsSource != null && dgMaster.SelectedIndex >= dgMaster.TopRows.Count)
        //    {
        //        //System.Windows.MessageBox.Show("Lot별 재고비교합니다!");
        //        GetStockDetail(dgMaster.CurrentCell.Row);
        //    }
        //    //GetStocks();
        //}

        //private void chkNerpApply_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (dgMaster.ItemsSource != null && dgMaster.SelectedIndex >= dgMaster.TopRows.Count)
        //    {
        //        //System.Windows.MessageBox.Show("Lot별 재고비교 안 합니다!");
        //        GetStockDetail(dgMaster.CurrentCell.Row);
        //    }
        //    //GetStocks();
        //}
    }

    public class NERPDestinationConfig : IDestinationConfiguration
    {
        public bool ChangeEventsSupported()
        {
            return false;
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters parms = new RfcConfigParameters();


            if (destinationName.Equals("PRD")) // 현 ERP 운영 
            {
                //parms.Add(RfcConfigParameters.AppServerHost, LoginInfo.SYSID.Contains("KR") ? "165.244.235.102" : "165.244.235.119");
                parms.Add(RfcConfigParameters.MessageServerHost, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "10.94.36.35" : "165.244.235.119"); // 국내 : 국외
                parms.Add(RfcConfigParameters.SystemID, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBP" : "GPD");
                parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
                parms.Add(RfcConfigParameters.Password, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "lgchem2016" : "RFCGMES01001");
                parms.Add(RfcConfigParameters.Client, "100");
                parms.Add(RfcConfigParameters.LogonGroup, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBPALL" : "LGGPD");
                parms.Add(RfcConfigParameters.Language, "KO");
                parms.Add(RfcConfigParameters.PoolSize, "5");
            }
            else if (destinationName.Equals("QAS")) // 현 ERP 개발 
            {
                //parms.Add(RfcConfigParameters.AppServerHost, "165.244.235.188");
                parms.Add(RfcConfigParameters.MessageServerHost, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "10.94.36.231" : "165.244.235.170"); // 국내 : 국외
                parms.Add(RfcConfigParameters.SystemID, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBQ" : "GQS");
                parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
                parms.Add(RfcConfigParameters.Password, "lgchem2016");
                parms.Add(RfcConfigParameters.Client, "100");
                parms.Add(RfcConfigParameters.LogonGroup, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBQALL" : "LGGQS");
                parms.Add(RfcConfigParameters.Language, "KO");
                parms.Add(RfcConfigParameters.PoolSize, "5");
            }
            else if (destinationName.Equals("ESD")) // New ERP 국내 개발
            {
                parms.Add(RfcConfigParameters.MessageServerHost, "10.31.134.32"); 
                parms.Add(RfcConfigParameters.SystemID, "ESD");
                parms.Add(RfcConfigParameters.User, "IF_GMES_01");
                parms.Add(RfcConfigParameters.Password, "Lgensolgmes2023#");
                parms.Add(RfcConfigParameters.Client, "600");
                parms.Add(RfcConfigParameters.LogonGroup, "ESDLGP");
                parms.Add(RfcConfigParameters.Language, "EN");
                parms.Add(RfcConfigParameters.MessageServerService, "3601");
            }
            else if (destinationName.Equals("ESQ")) // New ERP 실전 --> ESQ 330 
            {
                //< add key = "ESQ" value = "vhbzzesqcs.sap.erp.lgensol.com;ESQ;IF_GMES_01;Lgensolgmes2023#;330;ESQLGP;EN;3601" />
                //< add key = "ESP" value = "vhbzzespcs.sap.erp.lgensol.com;ESP;IF_GMES_01;Lgensolgmes2023#;110;ESPLGP;EN;3601" />
                parms.Add(RfcConfigParameters.MessageServerHost, "vhbzzesqcs.sap.erp.lgensol.com"); 
                parms.Add(RfcConfigParameters.SystemID, "ESQ");
                parms.Add(RfcConfigParameters.User, "IF_GMES_01");
                parms.Add(RfcConfigParameters.Password, "Lgensolgmes2023#");
                parms.Add(RfcConfigParameters.Client, "820");
                parms.Add(RfcConfigParameters.LogonGroup, "ESQLGP");
                parms.Add(RfcConfigParameters.Language, "EN");
                parms.Add(RfcConfigParameters.MessageServerService, "3601");
            }
            else if (destinationName.Equals("ESP")) // New ERP 운영 (추후 설정 필요함): 엔솔 NERP 유관시스템 대응 문서 확인 
            {
                parms.Add(RfcConfigParameters.MessageServerHost, "vhbzzespcs.sap.erp.lgensol.com");  
                parms.Add(RfcConfigParameters.SystemID, "ESP");
                parms.Add(RfcConfigParameters.User, "IF_GMES_01");
                parms.Add(RfcConfigParameters.Password, "Lgensolgmes2023#");
                parms.Add(RfcConfigParameters.Client, "100");
                parms.Add(RfcConfigParameters.LogonGroup, "ESPLGP");
                parms.Add(RfcConfigParameters.Language, "EN");
                parms.Add(RfcConfigParameters.MessageServerService, "3601");
            }

            return parms;
        }
    }
}
