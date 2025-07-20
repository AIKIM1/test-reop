/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
                      : Initial Created.
  2018.11.29  김동일  : 조회시 날짜 조건 버그 수정
  2021.04.01  조영대  : CNB2동 활성화일때 Lot ID 캡션 PKG Lot ID 로변경
  2021.04.13  김동일  : C20210329-000060 ERP 일재고 조회 RFC 개발에 따른 수정
  2022.04.06  김태균  : 제2산단 소형 전극/조립의 경우 SYSTEM ID가 'KR'이 아니고 'K2'임.'K2' 추가함.
                        조회대상 ERP서버정보 관련 수정
  2023.03.27  이홍주  : 소형활성화 MES 
**************************************************************************************/

using System;
using System.Configuration;
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

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    
    /// </summary>
    public partial class FCS002_320 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation { get; set; }
        RfcDestination dest;

        public FCS002_320()
        {
            InitializeComponent();
            SetComboBox(cboShop);
            SetComboBox(cboClass);

            //dtpEndStockYM.Text = System.DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            //dtpEndStockYM.SelectedDateTime = System.DateTime.Now.AddMonths(-1);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            ERPDestinationConfig cfg = new ERPDestinationConfig();

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }

            dest = RfcDestinationManager.GetDestination(ConfigurationManager.AppSettings["APP_SERVER"].Contains("DEV") ? "QAS" : "PRD");
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

        #region 기말재고 적용 여부 체크박스 처리 사용안함
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

        void GetStocks()
        {
            try
            {
                // 기말재고 월이 선택된 경우 이전 월인지 검사
                //if (chkApply.IsChecked == true)
                //{
                //    if (Convert.ToDecimal(System.DateTime.Now.AddMonths(-1).ToString("yyyyMM")) < Convert.ToDecimal(dtpEndStockYM.SelectedDateTime.ToString("yyyyMM")))
                //    {
                //        dtpEndStockYM.Text = System.DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
                //        dtpEndStockYM.SelectedDateTime = System.DateTime.Now.AddMonths(-1);
                //        Util.MessageValidation("SFU3448");  //이달 이후 날짜는 선택할 수 없습니다.
                //        return;
                //    }
                //}

                // 일별 조회 시 저장위치 필수.
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

                //DataTable dt;

                //if (chkApply.IsChecked == true)
                //{
                //    dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCK_SUMMARY_SUM_DATE", "INDATA", "RSLTDT", IndataTable);
                //}
                //else
                //{
                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_SUMMARY2", "INDATA", "RSLTDT", IndataTable, (dt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        try
                        {
                            if ((bool)rdoDay.IsChecked)
                            {
                                #region Get ERP Day Stock List

                                RfcRepository repo = dest.Repository;

                                IRfcFunction FnStockList = repo.CreateFunction("ZPPB_ERP_BEGIN_END_STOCK");
                                FnStockList.SetValue("I_DAY_FROM", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_From  *필수

                                if (txtProdId.Text != "")
                                    FnStockList.SetValue("I_DAY_TO", dtpDate.SelectedDateTime.ToString("yyyyMMdd"));  //일(Day)_To

                                FnStockList.SetValue("I_WERKS", cboShop.SelectedValue);                             //Plant         *필수
                                FnStockList.SetValue("I_LGORT", cboStockLocation.SelectedValue);                    //저장위치      *필수

                                FnStockList.SetValue("I_MATNR", txtProdId.Text);                                                //제품ID
                                FnStockList.SetValue("I_FLAG", "X");                                                //수불없는 날짜 포함 (X or Blank)

                                FnStockList.Invoke(dest);

                                var stockList = FnStockList.GetTable("IT_STOCK_GRGI");
                                var dtStockList = stockList.ToDataTable("IT_STOCK_GRGI");
                                #endregion Get ERP Stock List

                                #region Merge Based on GMES
                                foreach (DataRow dr in dt.Rows)
                                {
                                    DataView dv = dtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString()).AsDataView();

                                    if (dv.Count.Equals(0))
                                    {
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                        dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]);
                                        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        if (chkMoving.IsChecked == true)
                                        {
                                            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);                                         
                                        }
                                        else
                                        {
                                            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        }
                                        dr["SUM_QTY2_ERP"] = 0;
                                        dr["HOLD_LOT_QTY2_ERP"] = 0;
                                        dr["MOVING_LOT_QTY2_ERP"] = 0;
                                        dr["TOTAL_QTY2_ERP"] = 0;
                                    }
                                    else if (dv.Count > 0)
                                    {
                                        dr["SUM_QTY2_ERP"] = dv[0]["ZBEGIN_STOCK"]; //가용
                                        dr["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                        dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]) - Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                        dr["MOVING_LOT_QTY2_ERP"] = dv[0]["ZBEGIN_INTRANSIT"]; //이동 중
                                        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);

                                        if (chkMoving.IsChecked == true)
                                        {
                                            dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]); 
                                            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);

                                        }
                                        else
                                        {
                                            dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);

                                            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                            dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                        }
                                    }
                                }
                                #endregion Merge Base on GMES

                                #region Add Rows from ERP
                                foreach (DataRow dr in (dtStockList as DataTable).Rows)
                                {
                                    DataView dv = dt.AsEnumerable().Where(x => x.Field<string>("SHOPID") == dr["WERKS"].ToString() && x.Field<string>("SLOC_ID") == dr["LGORT"].ToString() && x.Field<string>("PRODID") == dr["MATNR"].ToString()).AsDataView();

                                    if (dv.Count.Equals(0))
                                    {
                                        DataRow newRow = dt.NewRow();
                                        newRow["SHOPID"] = dr["WERKS"];
                                        newRow["SLOC_ID"] = dr["LGORT"];
                                        newRow["PRODID"] = dr["MATNR"];
                                        newRow["SUM_QTY2"] = 0; //가용
                                        newRow["HOLD_LOT_QTY2"] = 0; //보류
                                        newRow["MOVING_LOT_QTY2"] = 0; //이동 중
                                        newRow["SUM_QTY2_ERP"] = dr["ZBEGIN_STOCK"]; //가용
                                        newRow["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                        newRow["MOVING_LOT_QTY2_ERP"] = dr["ZBEGIN_INTRANSIT"]; //이동 

                                        newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(dr["ZBEGIN_STOCK"]);
                                        newRow["GAP_HOLD_LOT_QTY2"] = 0 - 0;
                                        newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);

                                        if (chkMoving.IsChecked == true)
                                        {
                                            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + 0 + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                            newRow["TOTAL_QTY2"] = 0;
                                        }
                                        else
                                        {
                                            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + 0 + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                            newRow["SUM_QTY2_ERP"] = Convert.ToDouble(dr["ZBEGIN_STOCK"]) + Convert.ToDouble(dr["ZBEGIN_INTRANSIT"]);
                                            newRow["GAP_SUM_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                            newRow["TOTAL_QTY2"] = 0;

                                        }

                                        dt.Rows.Add(newRow);
                                    }
                                }
                                #endregion Add Rows from ERP
                            }
                            else
                            {
                                if (!dt.Columns.Contains("BKLAS"))
                                    dt.Columns.Add("BKLAS", typeof(string));

                                #region Get ERP Stock List

                                RfcRepository repo = dest.Repository;

                                IRfcFunction FnStockList = repo.CreateFunction("ZPPB_SEND_ERP_STOCK");
                                FnStockList.SetValue("I_WERKS", cboShop.SelectedValue);

                                //if (chkApply.IsChecked == true)
                                //    FnStockList.SetValue("I_YYYYMM", dtpEndStockYM.SelectedDateTime.ToString("yyyyMM"));

                                if ((bool)rdoMonth.IsChecked)
                                {
                                    FnStockList.SetValue("I_YYYYMM", dtpMonth.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"));
                                }

                                if (!string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                                    FnStockList.SetValue("I_LGORT", cboStockLocation.SelectedValue);

                                if (!string.IsNullOrWhiteSpace(txtProdId.Text.Trim()))
                                    FnStockList.SetValue("I_MATNR", txtProdId.Text.Trim());

                                FnStockList.Invoke(dest);

                                var stockList = FnStockList.GetTable("IT_STOCK");
                                var dtStockList = stockList.ToDataTable("IT_STOCK");
                                #endregion Get ERP Stock List

                                #region Merge Based on GMES
                                foreach (DataRow dr in dt.Rows)
                                {
                                    DataView dv = dtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString()).AsDataView();

                                    if (dv.Count.Equals(0))
                                    {
                                        dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]);
                                        dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]);
                                        dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        if (chkMoving.IsChecked == true)
                                        {
                                            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        }
                                        else
                                        {
                                            dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                            dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                            dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                            dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);

                                        }
                                    }
                                    else if (dv.Count > 0)
                                    {
                                        //if ((bool)rdoDay.IsChecked)
                                        //{
                                        //    dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]); //가용
                                        //    dr["HOLD_LOT_QTY2_ERP"] = Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]); //보류
                                        //    dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                        //    dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]) - Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                        //    dr["MOVING_LOT_QTY2_ERP"] = Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]); //이동 중
                                        //    dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);

                                        //    if (chkMoving.IsChecked == true)
                                        //    {
                                        //        dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                        //        dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]);
                                        //        dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]);

                                        //    }
                                        //    else
                                        //    {
                                        //        dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                        //        dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);
                                        //        dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        if (chkAvlPndChk.IsChecked == true)
                                        {
                                            dr["SUM_QTY2_ERP"] = dv[0]["LABST"]; //가용                        
                                            dr["HOLD_LOT_QTY2_ERP"] = dv[0]["SPEME"]; //보류                                                
                                            dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]) - Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                            dr["MOVING_LOT_QTY2_ERP"] = dv[0]["UMLME"]; //이동 중
                                            dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);

                                            if (chkMoving.IsChecked == true)
                                            {
                                                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);                                         
                                                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);

                                            }
                                            else
                                            {
                                                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                                dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                                dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);
                                                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]);
                                            }
                                        }
                                        else
                                        {
                                            dr["SUM_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]); //가용 + 보류                       
                                            dr["HOLD_LOT_QTY2_ERP"] = 0; //보류                                                                                   
                                            dr["GAP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["HOLD_LOT_QTY2"]) - Convert.ToDouble(dr["HOLD_LOT_QTY2_ERP"]);
                                            dr["MOVING_LOT_QTY2_ERP"] = dv[0]["UMLME"]; //이동 중
                                            dr["GAP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["MOVING_LOT_QTY2"]) - Convert.ToDouble(dr["MOVING_LOT_QTY2_ERP"]);

                                            if (chkMoving.IsChecked == true)
                                            {
                                                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                                dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]);
                                                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["GAP_MOVING_LOT_QTY2"]);

                                            }
                                            else
                                            {
                                                dr["TOTAL_QTY2_ERP"] = Convert.ToDouble(dv[0]["LABST"]) + Convert.ToDouble(dv[0]["SPEME"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                                dr["TOTAL_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                                dr["SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) + Convert.ToDouble(dr["HOLD_LOT_QTY2"]) + Convert.ToDouble(dr["MOVING_LOT_QTY2"]);
                                                dr["SUM_QTY2_ERP"] = Convert.ToDouble(dr["SUM_QTY2_ERP"]) + Convert.ToDouble(dv[0]["UMLME"]);
                                                dr["GAP_SUM_QTY2"] = Convert.ToDouble(dr["SUM_QTY2"]) - Convert.ToDouble(dr["SUM_QTY2_ERP"]);
                                                dr["GAP_TOTAL_QTY2"] = Convert.ToDouble(dr["GAP_SUM_QTY2"]) + Convert.ToDouble(dr["GAP_HOLD_LOT_QTY2"]);
                                            }
                                        }
                                        //}
                                    }
                                }
                                #endregion Merge Base on GMES

                                #region Add Rows from ERP
                                foreach (DataRow dr in (dtStockList as DataTable).Rows)
                                {
                                    DataView dv = dt.AsEnumerable().Where(x => x.Field<string>("SHOPID") == dr["WERKS"].ToString() && x.Field<string>("SLOC_ID") == dr["LGORT"].ToString() && x.Field<string>("PRODID") == dr["MATNR"].ToString()).AsDataView();

                                    if (dv.Count.Equals(0))
                                    {
                                        //int iBKLAS = 0;
                                        //int.TryParse(dr["BKLAS"].ToString(), out iBKLAS);

                                        //if (iBKLAS > 7900)
                                        //{
                                        DataRow newRow = dt.NewRow();
                                        newRow["SHOPID"] = dr["WERKS"];
                                        newRow["SLOC_ID"] = dr["LGORT"];
                                        newRow["PRODID"] = dr["MATNR"];
                                        newRow["SUM_QTY2"] = 0; //가용
                                        newRow["HOLD_LOT_QTY2"] = 0; //보류
                                        newRow["MOVING_LOT_QTY2"] = 0; //이동 중
                                        if (chkAvlPndChk.IsChecked == true)
                                        {
                                            newRow["SUM_QTY2_ERP"] = dr["LABST"]; //가용
                                            newRow["HOLD_LOT_QTY2_ERP"] = dr["SPEME"]; //보류
                                            newRow["MOVING_LOT_QTY2_ERP"] = dr["UMLME"]; //이동 

                                            newRow["GAP_SUM_QTY2"] = 0 - Convert.ToDouble(dr["LABST"]);
                                            newRow["GAP_HOLD_LOT_QTY2"] = 0 - Convert.ToDouble(dr["SPEME"]);
                                            newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(dr["UMLME"]);
                                        }
                                        else
                                        {
                                            newRow["SUM_QTY2_ERP"] = Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]); //가용 + 보류
                                            newRow["HOLD_LOT_QTY2_ERP"] = 0; //보류
                                            newRow["MOVING_LOT_QTY2_ERP"] = dr["UMLME"]; //이동 

                                            newRow["GAP_SUM_QTY2"] = 0 - (Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]));
                                            newRow["GAP_HOLD_LOT_QTY2"] = 0;
                                            newRow["GAP_MOVING_LOT_QTY2"] = 0 - Convert.ToDouble(dr["UMLME"]);
                                        }

                                        if (chkMoving.IsChecked == true)
                                        {                                           
                                            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]) + Convert.ToDouble(dr["UMLME"]);
                                            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                            newRow["TOTAL_QTY2"] = 0;
                                        }
                                        else
                                        {
                                            newRow["SUM_QTY2_ERP"] = Convert.ToDouble(newRow["SUM_QTY2_ERP"]) + Convert.ToDouble(dr["UMLME"]);
                                            newRow["TOTAL_QTY2_ERP"] = Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]) + Convert.ToDouble(dr["UMLME"]);
                                            newRow["GAP_TOTAL_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_HOLD_LOT_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                            newRow["GAP_SUM_QTY2"] = Convert.ToDouble(newRow["GAP_SUM_QTY2"]) + Convert.ToDouble(newRow["GAP_MOVING_LOT_QTY2"]);
                                            newRow["TOTAL_QTY2"] = 0;
                                        }

                                        newRow["BKLAS"] = dr["BKLAS"];

                                        dt.Rows.Add(newRow);
                                        //}
                                    }
                                }
                                #endregion Add Rows from ERP
                            }
                        }
                        catch (Exception ex) { }

                        #region Remove Rows by Condition
                        if (chkDiff.IsChecked == true)
                            for (int i = dt.Rows.Count - 1; i >= 0; i--)
                                if (Convert.ToDouble(dt.Rows[i]["SUM_QTY2"]).Equals(Convert.ToDouble(dt.Rows[i]["SUM_QTY2_ERP"])) && Convert.ToDouble(dt.Rows[i]["HOLD_LOT_QTY2"]).Equals(Convert.ToDouble(dt.Rows[i]["HOLD_LOT_QTY2_ERP"])) && Convert.ToDouble(dt.Rows[i]["MOVING_LOT_QTY2"]).Equals(Convert.ToDouble(dt.Rows[i]["MOVING_LOT_QTY2_ERP"])))
                                    dt.Rows.RemoveAt(i);
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
                        try
                        {
                            DataTable IndataTableMtrlClssCD = new DataTable("INDATA");
                            IndataTableMtrlClssCD.Columns.Add("CODE", typeof(string));

                            DataRow IndataRow = IndataTableMtrlClssCD.NewRow();
                            IndataRow["CODE"] = Util.NVC(cboClass.SelectedValue);
                            IndataTableMtrlClssCD.Rows.Add(IndataRow);

                            DataTable dtMtrlClss = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_CLSS_CODE", "INDATA", "RSLTDT", IndataTableMtrlClssCD);

                            if (dtMtrlClss != null && dtMtrlClss.Columns.Contains("CLSS_CODE"))
                            {
                                dt.AcceptChanges();

                                //for (int i = dt.Rows.Count; i > 0; i--)
                                //{
                                //    DataView dv = dtMtrlClss.AsEnumerable().Where(x => dt.Rows[i - 1]["PRODID"].ToString().Contains(x.Field<string>("CLSS_CODE"))).AsDataView();

                                //    if (dv.Count < 1)
                                //        dt.Rows.RemoveAt(i - 1);
                                //}
                                if (chkGmesChk.IsChecked == true)
                                {
                                    dt.AsEnumerable().Where(s => !dtMtrlClss.AsEnumerable().Where(es => s.Field<string>("PRODID").ToString().Contains(es.Field<string>("CLSS_CODE"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                                    dt.AcceptChanges();
                                }
                            }
                        }
                        catch (Exception ex3) { }
                        #endregion CLASS 조건 필터링

                        // ASL% 제품은 ROW 삭제
                        if (chkGmesChk.IsChecked == true)
                        {
                            dt.Select("PRODID Like 'ASL%' or PRODID Like 'APCSR%'").ToList<DataRow>().ForEach(row => row.Delete());
                            dt.Select("MTRLTYPE = 'PROD' and PRODID Not Like 'A%'").ToList<DataRow>().ForEach(row => row.Delete());

                            // MTRL 제외.
                            dt.Select("MTRLTYPE = 'MTRL' ").ToList<DataRow>().ForEach(row => row.Delete());
                            // null 이면 RFC 만 존재하는 데이터.. 자재 제외
                            if (dt.Columns.Contains("BKLAS"))
                                dt.Select("MTRLTYPE is null and BKLAS not like '7%' ").ToList<DataRow>().ForEach(row => row.Delete());

                            dt.AcceptChanges();
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

                                    dt.AsEnumerable().Where(s => !dtLocation.AsEnumerable().Where(c => s.Field<string>("SLOC_ID").ToString().Contains(c.Field<string>("CBO_CODE"))).Any()).ToList<DataRow>().ForEach(row => row.Delete());
                                    dt.AcceptChanges();
                                }
                            }
                            catch (Exception ex10) { }
                        }
                        #endregion

                        #region OrderBy
                        dt.AsEnumerable().OrderBy(x => x.Field<string>("PRODID") + x.Field<string>("SLOC_ID"));
                        #endregion OrderBy

                        #region ItemsSource Bind
                        Util.GridSetData(dgMaster, dt, FrameOperation);
                        #endregion ItemsSource Bind

                        #region Summaries
                        string[] columnNames = new string[] { "SHOPID", "PRODID", "SLOC_ID" };
                        new Util().SetDataGridMergeExtensionCol(dgMaster, columnNames, DataGridMergeMode.HORIZONTAL);

                        DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                        DataGridAggregateSum daSum = new DataGridAggregateSum();
                        daSum.ResultTemplate = DataGridAggregate.GetDataTemplateFromString("{0}");
                        dac.Add(daSum);

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
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
                //}
                #endregion Get GMES Stock List
            }
            catch (Exception ex99)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetStockDetail(C1.WPF.DataGrid.DataGridRow dataitem)
        {
            try
            {
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
                newRow["SUM_TYPE"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_TYPE"));
                newRow["SUM_YM"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_YM"));
                newRow["SUM_DATE"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_DATE"));
                newRow["SHOPID"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SHOPID"));
                newRow["AREAID"] = cboArea.SelectedValue;
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "PRODID"));
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

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_SUMMARY2_DETL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgDetail, searchResult, FrameOperation, true);
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

                    GetStockDetail(dg.CurrentCell.Row);

                    //if (e.LeftButton == MouseButtonState.Pressed &&
                    //    (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                    //    (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                    //    (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    //{
                    if (Util.NVC(currCell.Column.Name).Equals("PRODID"))
                    {
                        FCS002_320_CMP_DAY wndPopup = new FCS002_320_CMP_DAY();
                        wndPopup.FrameOperation = FrameOperation;

                        if (wndPopup != null)
                        {
                            object[] Parameters = new object[11];
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
                FCS002_320_CMP_DAY wndPopup = sender as FCS002_320_CMP_DAY;
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
    }

    public class ERPDestinationConfig : IDestinationConfiguration
    {
        public bool ChangeEventsSupported()
        {
            return false;
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters parms = new RfcConfigParameters();

            //if (destinationName.Equals("PRD"))
            //{
            //    //parms.Add(RfcConfigParameters.AppServerHost, LoginInfo.SYSID.Contains("KR") ? "165.244.235.102" : "165.244.235.119");
            //    parms.Add(RfcConfigParameters.MessageServerHost, LoginInfo.SYSID.Contains("KR") ? "165.244.235.102" : "165.244.235.119");
            //    //parms.Add(RfcConfigParameters.SystemNumber, "00");
            //    parms.Add(RfcConfigParameters.SystemID, LoginInfo.SYSID.Contains("KR") ? "PRD" : "GPD");
            //    parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
            //    parms.Add(RfcConfigParameters.Password, LoginInfo.SYSID.Contains("KR") ? "lgchem2016" : "RFCGMES01001");
            //    parms.Add(RfcConfigParameters.Client, "100");
            //    parms.Add(RfcConfigParameters.LogonGroup, LoginInfo.SYSID.Contains("KR") ? "PRDALL" : "LGGPD");
            //    parms.Add(RfcConfigParameters.Language, "KO");
            //    parms.Add(RfcConfigParameters.PoolSize, "5");
            //}
            //else if (destinationName.Equals("QAS"))
            //{
            //    //parms.Add(RfcConfigParameters.AppServerHost, "165.244.235.188");
            //    parms.Add(RfcConfigParameters.MessageServerHost, LoginInfo.SYSID.Contains("KR") ? "10.32.19.19" : "165.244.235.170");
            //    //parms.Add(RfcConfigParameters.SystemNumber, "00");
            //    parms.Add(RfcConfigParameters.SystemID, LoginInfo.SYSID.Contains("KR") ? "LBQ" : "GQS");
            //    parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
            //    parms.Add(RfcConfigParameters.Password, "lgchem2016");
            //    parms.Add(RfcConfigParameters.Client, "100");
            //    parms.Add(RfcConfigParameters.LogonGroup, LoginInfo.SYSID.Contains("KR") ? "LBQALL" : "LGGQS");
            //    parms.Add(RfcConfigParameters.Language, "KO");
            //    parms.Add(RfcConfigParameters.PoolSize, "5");
            //}

            if (destinationName.Equals("PRD"))
            {
                //parms.Add(RfcConfigParameters.AppServerHost, LoginInfo.SYSID.Contains("KR") ? "165.244.235.102" : "165.244.235.119");
                parms.Add(RfcConfigParameters.MessageServerHost, (LoginInfo.SYSID.Contains("KR")|| LoginInfo.SYSID.Contains("K2")) ? "10.94.36.35" : "165.244.235.119");
                //parms.Add(RfcConfigParameters.SystemNumber, "00");
                parms.Add(RfcConfigParameters.SystemID, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBP" : "GPD");
                parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
                parms.Add(RfcConfigParameters.Password, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "lgchem2016" : "RFCGMES01001");
                parms.Add(RfcConfigParameters.Client, "100");
                parms.Add(RfcConfigParameters.LogonGroup, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBPALL" : "LGGPD");
                parms.Add(RfcConfigParameters.Language, "KO");
                parms.Add(RfcConfigParameters.PoolSize, "5");
            }
            else if (destinationName.Equals("QAS"))
            {
                //parms.Add(RfcConfigParameters.AppServerHost, "165.244.235.188");
                parms.Add(RfcConfigParameters.MessageServerHost, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "10.94.36.231" : "165.244.235.170");
                //parms.Add(RfcConfigParameters.SystemNumber, "00");
                parms.Add(RfcConfigParameters.SystemID, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBQ" : "GQS");
                parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
                parms.Add(RfcConfigParameters.Password, "lgchem2016");
                parms.Add(RfcConfigParameters.Client, "100");
                parms.Add(RfcConfigParameters.LogonGroup, (LoginInfo.SYSID.Contains("KR") || LoginInfo.SYSID.Contains("K2")) ? "LBQALL" : "LGGQS");
                parms.Add(RfcConfigParameters.Language, "KO");
                parms.Add(RfcConfigParameters.PoolSize, "5");
            }

            return parms;
        }
    }

    public static class IRfcTableExtensions
    {
        /// <summary>
        /// Converts SAP table to .NET DataTable
        /// </summary>
        /// <param name="sapTable">The SAP table to convert.</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IRfcTable sapTable, string name)
        {
            DataTable adoTable = new DataTable(name);

            for (int liElement = 0; liElement < sapTable.ElementCount; liElement++)
            {
                RfcElementMetadata metadata = sapTable.GetElementMetadata(liElement);
                adoTable.Columns.Add(metadata.Name, GetDataType(metadata.DataType));
            }

            //SAP Table → .NET DataTable
            foreach (IRfcStructure row in sapTable)
            {
                DataRow ldr = adoTable.NewRow();

                for (int liElement = 0; liElement < sapTable.ElementCount; liElement++)
                {
                    RfcElementMetadata metadata = sapTable.GetElementMetadata(liElement);

                    switch (metadata.DataType)
                    {
                        case RfcDataType.DATE:
                            ldr[metadata.Name] = row.GetString(metadata.Name).Substring(0, 4) + row.GetString(metadata.Name).Substring(5, 2) + row.GetString(metadata.Name).Substring(8, 2);
                            break;

                        case RfcDataType.BCD:
                            ldr[metadata.Name] = row.GetDecimal(metadata.Name);
                            break;

                        case RfcDataType.CHAR:
                            ldr[metadata.Name] = row.GetString(metadata.Name);
                            break;

                        case RfcDataType.STRING:
                            ldr[metadata.Name] = row.GetString(metadata.Name);
                            break;

                        case RfcDataType.INT2:
                            ldr[metadata.Name] = row.GetInt(metadata.Name);
                            break;

                        case RfcDataType.INT4:
                            ldr[metadata.Name] = row.GetInt(metadata.Name);
                            break;

                        case RfcDataType.FLOAT:
                            ldr[metadata.Name] = row.GetDouble(metadata.Name);
                            break;

                        default:
                            ldr[metadata.Name] = row.GetString(metadata.Name);
                            break;
                    }
                }

                adoTable.Rows.Add(ldr);
            }

            return adoTable;
        }

        private static Type GetDataType(RfcDataType rfcDataType)
        {
            switch (rfcDataType)
            {
                case RfcDataType.DATE:
                    return typeof(string);

                case RfcDataType.CHAR:
                    return typeof(string);

                case RfcDataType.STRING:
                    return typeof(string);

                case RfcDataType.BCD:
                    return typeof(decimal);

                case RfcDataType.INT2:
                    return typeof(int);

                case RfcDataType.INT4:
                    return typeof(int);

                case RfcDataType.FLOAT:
                    return typeof(double);

                default:
                    return typeof(string);
            }
        }

    }
}