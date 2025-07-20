/*************************************************************************************
 Created Date : 2018.07.05
      Creator : 
   Decription : 기말재고 라우트별 실적 비교
--------------------------------------------------------------------------------------
 [Change History]
 2024-09-10   김영택   RFC 연결 문자열 변경 (NERP 적용)
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SAP.Middleware.Connector;
using System.Configuration;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_243 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        RfcDestination dest;

        // 2024.09.10 NERP 적용 여부 FLAG 
        private bool IS_NERP_FLAG = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_243()
        {
            InitializeComponent();            
        }

        #endregion Declaration & Constructor 


        #region Initialize

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            chkProd.IsChecked = true;
            chkMod.IsChecked = true;

            // NERP 적용 판단 
            if (IsNerpApplyFlag().Equals("Y")) { IS_NERP_FLAG = true; }

            // 2024.09.10 NERP 적용 
            NERPDestinationConfig cfg = new NERPDestinationConfig(); //ERPDestinationConfig cfg = new ERPDestinationConfig();

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }

            if (IS_NERP_FLAG == true ) // NERP 적용시
            {
                dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "ESP" : "ESQ");
            }
            else
            {
                // app.debug.config (APP_SERVER): 개발에만 존재하는 듯 
                //dest = RfcDestinationManager.GetDestination(ConfigurationManager.AppSettings["APP_SERVER"].Contains("DEV") ? "QAS" : "PRD");
                dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "PRD" : "QAS");
            }
        }

        #endregion Initialize


        #region Event

        private void dgLotDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgLotDetail.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // 차이가 0이 아닌 경우 Row색깔 변경
                if (!(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "GAP_QTY")).Equals("0")))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }
            }));
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            Util.gridClear(dgList);
            Util.gridClear(dgLotDetail);
            Util.gridClear(dgSummary);

            GetLotList();
            if (dgList.GetRowCount() > 0)
                GetSummary();
        }

        /// <summary>
        /// Lot 선택 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                RadioButton rb = sender as RadioButton;
                DataRow dr = (rb.DataContext as DataRowView).Row;
                string LotID = Util.ToString(dr["CT_LOTID"]);

                GetLotDetail(LotID);
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

        #endregion Event


        #region Method

        private bool Validation()
        {
            if(string.IsNullOrEmpty(txtProdID.Text))
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageValidation("SFU2949"); //제품ID를 입력하세요
                return false;
            }

            return true;
        }

        /// <summary>
        /// 실적처리이상LOT를 조회한다
        /// </summary>
        private void GetLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROD_MONTH", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PROD_LOT", typeof(string));
                dtRqst.Columns.Add("MOD_LOT", typeof(string));
                dtRqst.Columns.Add("SUM_FLAG", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROD_MONTH"] = Util.GetCondition(dtpDate);
                dr["PRODID"] = Util.GetCondition(txtProdID);
                dr["PROD_LOT"] = chkProd.IsChecked == true ? "Y" : "N";
                dr["MOD_LOT"] = chkMod.IsChecked == true ? "Y" : "N";
                dr["SUM_FLAG"] = "S";
                dr["LOTID"] = string.Empty;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GAP_LOT_LIST_ELTR", "INDATA", "OUTDATA", dtRqst);
                dtRslt.Columns.Add(new DataColumn("CHK", typeof(Int32)));

                if(dtRslt.Rows.Count > 0)
                {
                    dtRslt.Rows[0]["CHK"] = 1;
                }

                Util.GridSetData(dgList, dtRslt, FrameOperation, true);
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

        /// <summary>
        /// LOT 공정별 상세정보를 조회한다
        /// </summary>
        private void GetLotDetail(string _LotID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROD_MONTH", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PROD_LOT", typeof(string));
                dtRqst.Columns.Add("MOD_LOT", typeof(string));
                dtRqst.Columns.Add("SUM_FLAG", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROD_MONTH"] = Util.GetCondition(dtpDate);
                dr["PRODID"] = Util.GetCondition(txtProdID);
                dr["PROD_LOT"] = chkProd.IsChecked == true ? "Y" : "N";
                dr["MOD_LOT"] = chkMod.IsChecked == true ? "Y" : "N";
                dr["SUM_FLAG"] = "D";
                dr["LOTID"] = _LotID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GAP_LOT_LIST_ELTR", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgLotDetail, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {                
            }
        }

        /// <summary>
        /// Summary 그리드를 조회한다
        /// </summary>
        private void GetSummary()
        {
            try
            {
                #region Initialize Grid
                dgSummary.ItemsSource = null;
                #endregion Initialize Grid

                #region Get GMES Stock List
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROD_MONTH", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROD_MONTH"] = Util.GetCondition(dtpDate);
                Indata["PRODID"] = Util.GetCondition(txtProdID);
                dtRqst.Rows.Add(Indata);

                DataTable dt;

                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GAP_LOT_LIST_ELTR_SUM", "INDATA", "OUTDATA", dtRqst);

                #endregion Get GMES Stock List

                #region Get ERP Stock List
                

                RfcRepository repo = dest.Repository;

                string initDate = dtpDate.SelectedDateTime.AddMonths(-1).ToString("yyyyMM"); // 기초재고용 
                string finalDate = Util.GetCondition(dtpDate); // 기말재고용 
                string werks = LoginInfo.CFG_SHOP_ID;
                string matnr = Util.GetCondition(txtProdID);

                // 2024.09.10  Rfc name, input setting 
                string sapTable = IS_NERP_FLAG == true ? "TT_STOCK": "IT_STOCK";

                //기초재고 
                IRfcFunction initFnStockList = repo.CreateFunction("ZPPB_SEND_ERP_STOCK");

                if (IS_NERP_FLAG == true)
                {
                    initFnStockList.SetValue("IV_WERKS", werks);
                    initFnStockList.SetValue("IV_YYYYMM", initDate);
                    initFnStockList.SetValue("IV_MATNR", matnr);
                }
                else
                {
                    initFnStockList.SetValue("I_WERKS", werks);
                    initFnStockList.SetValue("I_YYYYMM", initDate);
                    initFnStockList.SetValue("I_MATNR", matnr);
                }
                initFnStockList.Invoke(dest);

                var initstockList = initFnStockList.GetTable(sapTable);
                var initdtStockList = initstockList.ToDataTable(sapTable);

                //기말재고
                IRfcFunction finalFnStockList = repo.CreateFunction("ZPPB_SEND_ERP_STOCK");

                if (IS_NERP_FLAG == true)
                {
                    finalFnStockList.SetValue("IV_WERKS", werks);
                    finalFnStockList.SetValue("IV_YYYYMM", finalDate);
                    finalFnStockList.SetValue("IV_MATNR", matnr);
                }
                else
                {
                    finalFnStockList.SetValue("I_WERKS", werks);
                    finalFnStockList.SetValue("I_YYYYMM", finalDate);
                    finalFnStockList.SetValue("I_MATNR", matnr);
                }
                finalFnStockList.Invoke(dest);

                var finalstockList = finalFnStockList.GetTable(sapTable);
                var finaldtStockList = finalstockList.ToDataTable(sapTable);

                #endregion Get ERP Stock List

                #region Merge Based on GMES
                foreach (DataRow dr in dt.Rows)
                {
                    // 기초재고
                    DataView initdv = initdtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString()).AsDataView();
                    if (initdv.Count.Equals(0))
                    {
                        dr["INIT_ERP_SUM_QTY2"] = Convert.ToDouble(dr["INIT_ERP_SUM_QTY2"]);
                        dr["INIT_ERP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["INIT_ERP_HOLD_LOT_QTY2"]);
                        dr["INIT_ERP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["INIT_ERP_MOVING_LOT_QTY2"]);
                    }
                    else if (initdv.Count > 0)
                    {
                        dr["INIT_ERP_SUM_QTY2"] = initdv[0]["LABST"];        //가용
                        dr["INIT_ERP_HOLD_LOT_QTY2"] = initdv[0]["SPEME"];   //보류
                        dr["INIT_ERP_MOVING_LOT_QTY2"] = initdv[0]["UMLME"]; //이동 중
                    }
                    // 기말재고
                    DataView finaldv = finaldtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == dr["SHOPID"].ToString() && x.Field<string>("LGORT") == dr["SLOC_ID"].ToString() && x.Field<string>("MATNR") == dr["PRODID"].ToString()).AsDataView();
                    if (finaldv.Count.Equals(0))
                    {
                        dr["FINAL_ERP_SUM_QTY2"] = Convert.ToDouble(dr["FINAL_ERP_SUM_QTY2"]);
                        dr["FINAL_ERP_HOLD_LOT_QTY2"] = Convert.ToDouble(dr["FINAL_ERP_HOLD_LOT_QTY2"]);
                        dr["FINAL_ERP_MOVING_LOT_QTY2"] = Convert.ToDouble(dr["FINAL_ERP_MOVING_LOT_QTY2"]);
                    }
                    else if (finaldv.Count > 0)
                    {
                        dr["FINAL_ERP_SUM_QTY2"] = finaldv[0]["LABST"];        //가용
                        dr["FINAL_ERP_HOLD_LOT_QTY2"] = finaldv[0]["SPEME"];   //보류
                        dr["FINAL_ERP_MOVING_LOT_QTY2"] = finaldv[0]["UMLME"]; //이동 중
                    }
                }
                #endregion Merge Base on GMES

                #region OrderBy
                dt.AsEnumerable().OrderBy(x => x.Field<string>("PRODID") + x.Field<string>("SLOC_ID"));
                #endregion OrderBy

                #region ItemsSource Bind
                Util.GridSetData(dgSummary, dt, FrameOperation);
                #endregion ItemsSource Bind

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {                
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

        #endregion Method
    }
}
