/*************************************************************************************
 Created Date : 2021.03.05
      Creator : 오광택
   Decription : 믹서 원자재 불출요청
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.05          : Initial Created.

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
using System.Collections.Generic;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// </summary>
    public partial class COM001_352 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation { get; set; }
        RfcDestination dest;
        DataTable _dt;

        public COM001_352()
        {
            InitializeComponent();
            SetComboBox(cboArea);
            SetComboBox(cboLine);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
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
            catch (Exception ex) { }
        }

        void SetComboBox(C1ComboBox cbo)
        {
            switch (cbo.Name)
            {
                case "cboArea":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_AUTH_AREA_CBO", cbo, new string[] { "LANGID", "SHOPID", "USERID", "SYSTEM_ID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.USERID, LoginInfo.SYSID }, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME");
                    break;
                case "cboLine":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", cbo, new string[] { "LANGID", "AREAID" }, new string[] { LoginInfo.LANGID, cboArea.SelectedValue as string }, CommonCombo.ComboStatus.SELECT, "CBO_CODE", "CBO_NAME");
                    break;
            }
        }

        private enum RoundType
        {
            Ceiling,
            Round,
            Truncate
        }

        void GetStocks()
        {
            #region Initialize Grid
            dgStockList.ItemsSource = null;
            #endregion Initialize Grid

            #region Get GMES Stock List
            //DataTable IndataTable = new DataTable("INDATA");
            //IndataTable.Columns.Add("SHOPID", typeof(string));
            //IndataTable.Columns.Add("AREAID", typeof(string));
            //IndataTable.Columns.Add("PLAN_DATE", typeof(string));
            
            //DataRow Indata = IndataTable.NewRow();

            //Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            //Indata["AREAID"] = cboArea.SelectedValue;
            //Indata["PLAN_DATE"] = DateTime.Today.ToString("yyyyMMdd");

            //IndataTable.Rows.Add(Indata);

            #endregion Get GMES Stock List

            try
            {

                RfcRepository repo = dest.Repository;

                IRfcFunction FnStockList = repo.CreateFunction("ZPPB_SEND_ERP_STOCK");
                FnStockList.SetValue("I_WERKS", LoginInfo.CFG_SHOP_ID);

                DataTable stockDt = new DataTable();
                stockDt.Columns.Add("LANGID", typeof(string));
                stockDt.Columns.Add("CMCDTYPE", typeof(string));
                stockDt.Columns.Add("CMCODE", typeof(string));

                DataRow stockRow = stockDt.NewRow();
                stockRow["LANGID"] = LoginInfo.LANGID;
                stockRow["CMCDTYPE"] = "MIX_RMTRL_ERP_SLOC_MOVE";
                stockRow["CMCODE"] = cboLine.SelectedValue;
                //stockRow["CMCODE"] = "E5D01";

                stockDt.Rows.Add(stockRow);
                DataTable returnstockDt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", stockDt);
                // 저장위치 
                FnStockList.SetValue("I_LGORT", returnstockDt.Rows[0]["ATTRIBUTE1"].ToString()); 


                FnStockList.Invoke(dest);

                var stockList = FnStockList.GetTable("IT_STOCK");
                var dtStockList = stockList.ToDataTable2("IT_STOCK");

                // 'BA08','NA2037','NA2033','311000','NA2036'  자재 리스트 삭제
                DataTable mtdelDt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MATLIST_DEL", "INDATA", "OUTDATA", null);

                for ( int j = 0; j < mtdelDt.Rows.Count; j++)
                    dtStockList.Select("MATNR = '" + mtdelDt.Rows[j]["MTRLID"].ToString() + "'").ToList<DataRow>().ForEach(row => row.Delete());

                DataTable rfcDt = new DataTable();
                rfcDt.Columns.Add("CHK", typeof(bool));
                rfcDt.Columns.Add("MTRLID", typeof(string));
                rfcDt.Columns.Add("MTRLDESC", typeof(string));
                rfcDt.Columns.Add("MTRLUNIT", typeof(string));
                rfcDt.Columns.Add("SUM_QTY2", typeof(string));
                rfcDt.Columns.Add("REQUIREMENT_QTY", typeof(decimal));
                rfcDt.Columns.Add("DIFFWEIGHT", typeof(string));
                rfcDt.Columns.Add("DIFFPALLET", typeof(string));
                rfcDt.Columns.Add("REQWEIGHT", typeof(string));
                rfcDt.Columns.Add("REQPALLET", typeof(string));
                rfcDt.Columns.Add("PLLT_LOAD_MTRL_WEIGHT", typeof(string));

                foreach (DataRow dr in dtStockList.Rows)
                {
                    DataView dv = new DataView();
                    //dv = dtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == LoginInfo.CFG_SHOP_ID).AsDataView();

                    DataRow newRow = rfcDt.NewRow();
                    newRow["CHK"] = false;
                    newRow["MTRLID"] = dr["MATNR"];
                    newRow["MTRLDESC"] = dr["MAKTX"];
                    newRow["MTRLUNIT"] = dr["MEINS"];
                    newRow["SUM_QTY2"] = dr["LABST"];
                    newRow["REQUIREMENT_QTY"] = 0.0;
                    newRow["DIFFWEIGHT"] = string.Empty;
                     newRow["DIFFPALLET"] = string.Empty;
                    newRow["REQWEIGHT"] = string.Empty;
                    newRow["REQPALLET"] = string.Empty;
                    newRow["PLLT_LOAD_MTRL_WEIGHT"] = string.Empty;
                    rfcDt.Rows.Add(newRow);
                }  

                DataTable groupbysumDt = GroupbySum(rfcDt, new string[] { "SUM_QTY2" }, new string[] { "MTRLID", "MTRLDESC", "MTRLUNIT" }, true);
                //// 중복값제거
                //DataView distinctView = rfcDt.DefaultView;
                //DataTable distinctDt = distinctView.ToTable(true,"MTRLID");

                #region 필요량

                //DataTable needqtyDt = Needqty();

                #endregion

                #region 요청량
                DataTable reqDt = Reqqty();
                #endregion

                for (int i = 0; i < groupbysumDt.Rows.Count; i++)
                {
                    groupbysumDt.Rows[i]["SUM_QTY2"] = CustomRound(RoundType.Ceiling, Util.StringToDouble(groupbysumDt.Rows[i]["SUM_QTY2"].ToString()), 2);

                    for (int j = 0; j < reqDt.Rows.Count; j++)
                    {
                        if (groupbysumDt.Rows[i]["MTRLID"].ToString() == reqDt.Rows[j]["MTRLID"].ToString())
                        {
                            groupbysumDt.Rows[i]["REQUIREMENT_QTY"] = reqDt.Rows[j]["MTRL_REQ_QTY"].ToString();
                        }
                        if (!groupbysumDt.Rows[i]["REQUIREMENT_QTY"].ToString().Equals("") && groupbysumDt.Rows[i]["MTRLID"].ToString() == reqDt.Rows[j]["MTRLID"].ToString())
                        {
                            groupbysumDt.Rows[i]["DIFFWEIGHT"] = Util.StringToDouble(groupbysumDt.Rows[i]["SUM_QTY2"].ToString()) - Util.StringToDouble(groupbysumDt.Rows[i]["REQUIREMENT_QTY"].ToString());
                            if (Util.StringToDouble(groupbysumDt.Rows[i]["DIFFWEIGHT"].ToString()) < 0)
                            {
                                groupbysumDt.Rows[i]["DIFFWEIGHT"] = Math.Ceiling(Util.StringToDouble(groupbysumDt.Rows[i]["DIFFWEIGHT"].ToString()) * -1);
                                groupbysumDt.Rows[i]["DIFFPALLET"] = CustomRound(RoundType.Ceiling, Util.StringToDouble(groupbysumDt.Rows[i]["DIFFWEIGHT"].ToString()) / Util.StringToDouble(reqDt.Rows[j]["PLLT_LOAD_MTRL_WEIGHT"].ToString()), 2);
                                groupbysumDt.Rows[i]["REQPALLET"] = Math.Ceiling(Util.StringToDouble(groupbysumDt.Rows[i]["DIFFPALLET"].ToString()));
                                groupbysumDt.Rows[i]["REQWEIGHT"] = Util.StringToDouble(reqDt.Rows[j]["PLLT_LOAD_MTRL_WEIGHT"].ToString()) * Util.StringToInt(groupbysumDt.Rows[i]["REQPALLET"].ToString());
                            }
                            else
                            {
                                groupbysumDt.Rows[i]["DIFFWEIGHT"] = Math.Ceiling(Util.StringToDouble(groupbysumDt.Rows[i]["DIFFWEIGHT"].ToString()));
                                groupbysumDt.Rows[i]["DIFFPALLET"] = CustomRound(RoundType.Ceiling, Util.StringToDouble(groupbysumDt.Rows[i]["DIFFWEIGHT"].ToString()) / Util.StringToDouble(reqDt.Rows[j]["PLLT_LOAD_MTRL_WEIGHT"].ToString()), 2);
                                groupbysumDt.Rows[i]["REQPALLET"] = string.Empty;
                                groupbysumDt.Rows[i]["REQWEIGHT"] = string.Empty;
                            }
                        }
                    }
                    if (Util.NVC_Int(groupbysumDt.Rows[i]["REQPALLET"].ToString()) > 0)
                    {
                        groupbysumDt.Rows[i]["CHK"] = true;
                    }
                    else
                    {
                        groupbysumDt.Rows[i]["CHK"] = false;
                    }
                }
                _dt = groupbysumDt;
                Util.GridSetData(dgStockList, groupbysumDt, FrameOperation);
            }
            catch (Exception ex) { }
        }

        static private double CustomRound(RoundType roundType, double value, int digit = 1)
        {
            double dReturn = 0;

            // 지정 자릿수의 올림,반올림, 버림을 계산하기 위한 중간 계산
            double digitCal = Math.Pow(10, digit) / 10;

            switch (roundType)
            {
                case RoundType.Ceiling:
                    dReturn = Math.Ceiling(value * digitCal) / digitCal;
                    break;
                case RoundType.Round:
                    dReturn = Math.Round(value * digitCal) / digitCal;
                    break;
                case RoundType.Truncate:
                    dReturn = Math.Truncate(value * digitCal) / digitCal;
                    break;
            }
            return dReturn;
        }

        public DataTable Needqty()
        {
            DataTable needdt = new DataTable();
            DateTime nowDatetime = DateTime.Now;

            needdt.Columns.Add("SHOPID", typeof(string));
            needdt.Columns.Add("PROCID", typeof(string));
            needdt.Columns.Add("AREAID", typeof(string));
            needdt.Columns.Add("EQSGID", typeof(string));
            needdt.Columns.Add("STRT_DATE", typeof(string));
            needdt.Columns.Add("END_DATE", typeof(string));

            DataRow searchCondition = needdt.NewRow();
            searchCondition["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            searchCondition["PROCID"] = "E1000";
            searchCondition["AREAID"] = cboArea.SelectedValue;
            searchCondition["EQSGID"] = cboLine.SelectedValue;
            searchCondition["STRT_DATE"] = DateTime.Now.ToString("yyyyMMdd");

            if (nowDatetime.DayOfWeek == DayOfWeek.Friday)
            {
                searchCondition["END_DATE"] = DateTime.Now.AddDays(3).ToString("yyyyMMdd");
            }
            else
            {
                searchCondition["END_DATE"] = DateTime.Now.AddDays(3).ToString("yyyyMMdd");
            }

            needdt.Rows.Add(searchCondition);
            DataTable qtyDT = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_WITH_MIXER_QTY", "INDATA", "OUTDATA", needdt);
            qtyDT = GroupbySum(qtyDT, new string[] { "REQUEST_QTY" }, new string[] { "MTRLID", "MTRLDESC", "MTRLUNIT", "PLLT_LOAD_MTRL_WEIGHT" }, true);
            return qtyDT;
        }

        public DataTable Reqqty()
        {
            DataTable reqdt = new DataTable();
            DataTable plltDt = new DataTable();
            reqdt.Columns.Add("EQSGID", typeof(string));

            DataRow searchCondition = reqdt.NewRow();
            searchCondition["EQSGID"] = cboLine.SelectedValue;
            reqdt.Rows.Add(searchCondition);

            DataTable qtyDT = new ClientProxy().ExecuteServiceSync("DA_PRD_ROLLMAP_MIX_MTRL_REQ", "INDATA", "OUTDATA", reqdt);
            if( qtyDT.Rows.Count > 0)
            {
                try
                {
                    qtyDT.Columns.Add("SHOPID", typeof(string));
                    for (int ii = 0; ii < qtyDT.Rows.Count; ii++)
                    {
                        qtyDT.Rows[ii]["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    }
                    qtyDT.Columns.Add("PLLT_LOAD_MTRL_WEIGHT", typeof(string));
                    plltDt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_PLLTQTY", "INDATA", "OUTDATA", qtyDT);
                    for (int pllt = 0; pllt < plltDt.Rows.Count; pllt++)
                    {
                        if (plltDt.Rows[pllt]["PLLT_LOAD_MTRL_WEIGHT"].ToString().Equals(null) || plltDt.Rows[pllt]["PLLT_LOAD_MTRL_WEIGHT"].ToString().Equals(""))
                        {
                            plltDt.Rows[pllt]["PLLT_LOAD_MTRL_WEIGHT"] = "100";
                        }
                    }
                    for (int i = 0; i < qtyDT.Rows.Count; i++)
                    {
                        for (int j = 0; j < plltDt.Rows.Count; j++)
                        {
                            if (qtyDT.Rows[i]["MTRLID"].ToString() == plltDt.Rows[j]["MTRLID"].ToString())
                            {
                                qtyDT.Rows[i]["PLLT_LOAD_MTRL_WEIGHT"] = plltDt.Rows[j]["PLLT_LOAD_MTRL_WEIGHT"].ToString();
                            }
                            else
                            {
                                qtyDT.Rows[i]["PLLT_LOAD_MTRL_WEIGHT"] = "100";
                            }
                        }
                    }
                }
                catch(Exception ex){}
            }
            return qtyDT;
        }

        public static DataTable GroupbySum(DataTable dt, string[] sumColumnNames, string[] groupByColumnNames, bool reAllColumn)
        {
            DataTable dt_Return = null;

            try
            {
                //Check datatable
                if (dt == null || dt.Rows.Count < 1) { return dt; }
                //Check sum columns
                if (sumColumnNames == null || sumColumnNames.Length < 1) { return dt; }
                //Check group columns
                if (groupByColumnNames == null || groupByColumnNames.Length < 1) { return dt; }

                //Create return datatable
                dt_Return = dt.DefaultView.ToTable(true, groupByColumnNames);

                //Set return Columns
                if (reAllColumn)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (!dt_Return.Columns.Contains(dt.Columns[i].ColumnName))
                        {
                            DataColumn dc = new DataColumn(dt.Columns[i].ColumnName, dt.Columns[i].DataType);
                            dt_Return.Columns.Add(dc);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sumColumnNames.Length; i++)
                    {
                        if (dt.Columns.Contains(sumColumnNames[i]))
                        {
                            DataColumn dc = new DataColumn(sumColumnNames[i], dt.Columns[sumColumnNames[i]].DataType);
                            dt_Return.Columns.Add(dc);
                        }
                    }
                }

                //Summary Rows
                for (int i = 0; i < dt_Return.Rows.Count; i++)
                {
                    var sQuery = " 1=1 ";

                    foreach (var col in groupByColumnNames)
                    {
                        sQuery += "AND " + col + " = '" + dt_Return.Rows[i][col].ToString() + "'";
                    }
                    DataRow[] drs = dt.Select("(" + sQuery + ")");

                    foreach (var dr in drs)
                    {
                        foreach (var col in sumColumnNames)
                        {
                            decimal sum, val = 0;
                            decimal.TryParse(dt_Return.Rows[i][col].ToString(), out sum);
                            decimal.TryParse(dr[col].ToString(), out val);
                            dt_Return.Rows[i][col] = sum + val;
                        }
                    }

                }
            }
            catch (Exception ex) { }

            return dt_Return;                                                                                                                    
        }

        public string makeBodyApp(string sTitle, string sContent, DataTable dtLot = null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                sb.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                sb.Append("<head>");
                sb.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
                sb.Append("<title>Untitled Document</title>");
                sb.Append("<style>");
                sb.Append("	* {margin:0;padding:0;}");
                sb.Append("	body {font-family:Malgun Gothic, Arial, Helvetica, sans-serif;font-size:14px;line-height:1.8;color:#333333;}");
                sb.Append("	table {border-collapse:collapse;width:100%;}");
                sb.Append("	table th {background:#f5f5f5;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table td {background:#fff;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;border-bottom:1px solid #e1e1e1;}");
                sb.Append("	table tbody th {border-left:1px solid #e1e1e1;text-align:right;padding:6px 8px;		}");
                sb.Append("	table tbody td {text-align:left;padding:6px 8px;}");
                sb.Append("	table thead th {text-align:center;padding:3px;border-right:1px solid #e1e1e1;border-left:1px solid #e1e1e1;	border-bottom:1px solid #d1d1d1;}");
                //sb.Append("	.hori-table table tbody th {text-align:left;padding:3px;}");
                //sb.Append("	.hori-table table tbody td {text-align:right;padding:3px;}");
                sb.Append("	.vertical-table, .hori-table {margin-bottom:20px;}");
                sb.Append("</style>");
                sb.Append("</head>");
                sb.Append("<body>");
                sb.Append("	<div class=\"wrap\">");
                sb.Append("    	<div class=\"vertical-table\">");
                sb.Append("            <table style=\"border-top:2px solid #c8294b; max-width:720px;\">");
                sb.Append("                <tbody>");
                sb.Append("                    <tr>");
                sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("요청구분") + "</th>");
                sb.Append("                        <td>" + sTitle + "</td>");
                sb.Append("                    </tr>");
                sb.Append("                </tbody>");
                sb.Append("            </table>");
                sb.Append("        </div>");
                if (dtLot != null && dtLot.Rows.Count > 0)
                {
                    sb.Append("    <div class=\"hori-table\">");
                    sb.Append("        	<table style=\"border-top:2px solid #c8294b; max-width:720px;\" >");
                    sb.Append("            	<colgroup>");
                    sb.Append("                	<col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                    <col width=\"\" />");
                    sb.Append("                </colgroup>");
                    sb.Append("                <thead>");
                    sb.Append("                	<tr>");
                    //sb.Append("                    	<th>Lot ID</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("자재") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("자재명") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("필요량") + "</th>");
                    sb.Append("                        <th>" + ObjectDic.Instance.GetObjectName("PALLET") + "</th>");
                    sb.Append("                    </tr>");
                    sb.Append("                </thead>");
                    sb.Append("                <tbody>");
                    foreach (DataRow dr in dtLot.Rows)
                    {
                        sb.Append("                	<tr>");
                        sb.Append("                  <td style=" + "text-align:center" + ">" + Util.NVC(dr["MTRLID"]) + "&nbsp;</td>");
                        sb.Append("                  <td style=" + "text-align:left" + ">" + Util.NVC(dr["MTRLDESC"]) + "&nbsp;</th>");
                        sb.Append("                  <td style=" + "text-align:right" + ">" + Util.NVC(dr["REQWEIGHT"]) + "&nbsp;</td>");
                        sb.Append("                  <td style=" + "text-align:right" + ">" + Util.NVC(dr["REQPALLET"]) + "&nbsp;</td>");
                        sb.Append("                 </tr>");
                    }
                    sb.Append("                </tbody>");
                    sb.Append("            </table>");
                    sb.Append("        </div>");
                }
                sb.Append("    </div>");
                sb.Append("</body>");
                sb.Append("</html>");
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.ToString());
            }
            return sb.ToString();
        }

        private void cboLine_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //SetComboBox(cboStockLocation);
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetComboBox(cboLine);
        }

        //private void CheckBox_Click(object sender, RoutedEventArgs e)
        //{
        //    //if (dgStockList.CurrentRow.DataItem == null)
        //    //    return;

        //    int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

        //    DataTable dt = ((DataView)dgStockList.ItemsSource).Table;

        //    if (DataTableConverter.GetValue(dgStockList.Rows[rowIndex].DataItem, "CHK").ToString().Equals("False"))
        //    {
        //        DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "CHK", false);
        //        DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQWEIGHT", string.Empty);
        //        DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQPALLET", string.Empty);
        //    }
        //    else
        //    {
        //        DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "CHK", true);
        //        DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQWEIGHT", _dt.Rows[rowIndex - 2]["REQWEIGHT"].ToString());
        //        DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQPALLET", _dt.Rows[rowIndex - 2]["REQPALLET"].ToString());
        //    }
        //}
        

        private static void MailSend(string sSenderID, string sSenderName, string sTo, string sCC, string sSub, string sBody)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("P_RECIPIENTS", typeof(string));
                dtRqst.Columns.Add("P_COPY_RECIPIENTS", typeof(string));
                dtRqst.Columns.Add("P_FROM_ADDRESS", typeof(string));
                dtRqst.Columns.Add("P_SUBJECT", typeof(string));
                dtRqst.Columns.Add("P_BODY", typeof(string));
                dtRqst.Columns.Add("P_BODY_FORMAT", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (sTo.Contains(";"))
                {
                    sTo = sTo.Replace(";", "@lgespartner.com;");
                }
                else
                {
                    sTo = sTo + "@lgespartner.com";
                }
                dr["P_RECIPIENTS"] = sTo;

                if (sCC.Contains(";"))
                {
                    sCC = sCC.Replace(";", "@lgespartner.com;");
                }
                else
                {
                    sCC = sCC + "@lgespartner.com";
                }
                dr["P_COPY_RECIPIENTS"] = sCC;
                dr["P_FROM_ADDRESS"] = sSenderID + "@lgespartner.com";
                dr["P_SUBJECT"] = sSub;
                dr["P_BODY"] = sBody;
                dr["P_BODY_FORMAT"] = "HTML";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_MAIL_SEND", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable Searchdt = new DataTable();

            Searchdt.Columns.Add("EQSGID", typeof(string));
            Searchdt.Columns.Add("STRT_DATE", typeof(string));
            Searchdt.Columns.Add("END_DATE", typeof(string));

            DataRow dtRow = Searchdt.NewRow();
            dtRow["EQSGID"] = cboLine.SelectedValue;
            Searchdt.Rows.Add(dtRow);

            // 불출요청서 조회 비즈
            DataTable returndt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_REQID_CHECK", "INDATA", "OUTDATA", Searchdt);

            DataTable reqDt = new DataTable();
            DateTime nowDatetime = DateTime.Now;

            reqDt.Columns.Add("EQSGID", typeof(string));
            reqDt.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));
            reqDt.Columns.Add("MTRLID", typeof(string));
            reqDt.Columns.Add("MTRL_SPLY_REQ_QTY", typeof(decimal));
            reqDt.Columns.Add("PLLT_REQ_QTY", typeof(string));
            reqDt.Columns.Add("MTRL_SPLY_REQ_DATE", typeof(string));
            reqDt.Columns.Add("REQ_STAT_CODE", typeof(string));
            reqDt.Columns.Add("IWMS_ISS_QTY", typeof(decimal));
            reqDt.Columns.Add("DEL_FLAG", typeof(string));
            reqDt.Columns.Add("INSUSER", typeof(string));
            reqDt.Columns.Add("UPDUSER", typeof(string));
          
            DataTable dt = ((DataView)dgStockList.ItemsSource).Table;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["CHK"].ToString().Equals("True"))
                {
                    DataRow reqRow = reqDt.NewRow();
                    reqRow["EQSGID"] = cboLine.SelectedValue;
                    reqRow["MTRL_SPLY_REQ_ID"] = returndt.Rows[0]["MTRL_SPLY_REQ_ID"].ToString();
                    reqRow["MTRL_SPLY_REQ_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                    reqRow["REQ_STAT_CODE"] = "R";
                    reqRow["IWMS_ISS_QTY"] = 0;
                    reqRow["DEL_FLAG"] = "N";
                    reqRow["INSUSER"] = LoginInfo.USERID;
                    reqRow["UPDUSER"] = LoginInfo.USERID;
                    reqRow["MTRLID"] = dt.Rows[i]["MTRLID"].ToString();
                    reqRow["MTRL_SPLY_REQ_QTY"] = Util.NVC_Decimal(dt.Rows[i]["REQWEIGHT"].ToString());
                    reqRow["PLLT_REQ_QTY"] = dt.Rows[i]["REQPALLET"].ToString();
                    reqDt.Rows.Add(reqRow);
                }
            }
            
            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (reqDt.Rows.Count > 0)
                        new ClientProxy().ExecuteServiceSync("DA_PRD_INS_MATERIAL_REQUEST", "INDATA", "OUTDATA", reqDt);
                    
                    //정상 처리되었습니다.
                    Util.MessageInfo("SFU1275");
                } 
            });

            //메일전송
            string[] User = new string[] { "sensszz" };
            string Test = string.Empty;
            DataTable maildt = ((DataView)dgStockList.ItemsSource).Table;
            DataTable gridDt = new DataTable();
            gridDt.Columns.Add("MTRLID", typeof(string));
            gridDt.Columns.Add("MTRLDESC", typeof(string));
            gridDt.Columns.Add("REQWEIGHT", typeof(string));
            gridDt.Columns.Add("REQPALLET", typeof(string));

            for (int comma = 0; comma < maildt.Rows.Count; comma++)
            {
                maildt.Rows[comma]["REQWEIGHT"] = string.Format("{0:#,###}", Util.StringToDouble(maildt.Rows[comma]["REQWEIGHT"].ToString()));
                maildt.Rows[comma]["REQPALLET"] = string.Format("{0:#,###}", Util.StringToDouble(maildt.Rows[comma]["REQPALLET"].ToString()));
            }



            for (int gridCount = 0; gridCount < maildt.Rows.Count; gridCount++)
            {
                if (!maildt.Rows[gridCount]["CHK"].ToString().Equals("False"))
                {
                    DataRow gridRow = gridDt.NewRow();
                    gridRow["MTRLID"] = maildt.Rows[gridCount]["MTRLID"].ToString();
                    gridRow["MTRLDESC"] = maildt.Rows[gridCount]["MTRLDESC"].ToString();
                    gridRow["REQWEIGHT"] = maildt.Rows[gridCount]["REQWEIGHT"].ToString();
                    gridRow["REQPALLET"] = maildt.Rows[gridCount]["REQPALLET"].ToString();

                    gridDt.Rows.Add(gridRow);
                }
            }

            for (int i = 0; i < User.Length; i++)
            {
                MailSend(LoginInfo.USERID, LoginInfo.USERNAME, User[i], "", "믹서 불출 수량", this.makeBodyApp("믹서 불출 수량", Util.GetCondition("TEST"), gridDt));
            }

        }

        private void dgStockList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null)
            {
                //if (e.Column.Index != grid.Columns["CHK"].Index && DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(0))
                if(e.Column.Index != grid.Columns["CHK"].Index && DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(false))
                {
                    e.Cancel = true;
                }
            }

            // cell 숨기기 
            //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            //DataTable dt = ((DataView)dgStockList.ItemsSource).Table;

            //if (DataTableConverter.GetValue(dgStockList.Rows[rowIndex].DataItem, "CHK").ToString().Equals("False"))
            //{
            //    DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "CHK", false);
            //    DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQWEIGHT", string.Empty);
            //    DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQPALLET", string.Empty);
            //}
            //else
            //{
            //    DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "CHK", true);
            //    DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQWEIGHT", _dt.Rows[rowIndex - 2]["REQWEIGHT"].ToString());
            //    DataTableConverter.SetValue(dgStockList.Rows[rowIndex].DataItem, "REQPALLET", _dt.Rows[rowIndex - 2]["REQPALLET"].ToString());
            //}

        }

        private void dgStockList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //C1DataGrid dg = sender as C1DataGrid;
                DataTable dt = ((DataView)dgStockList.ItemsSource).Table;
                DataTable newDt = new DataTable();
                newDt.Columns.Add("MTRLID", typeof(string));
                newDt.Columns.Add("SHOPID", typeof(string));

                //DataRow newRow = newDt.NewRow();
                //newRow["MTRLID"] = dt.Rows[e.Cell.Row.Index]["MTRLID"].ToString();
                //newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //newDt.Rows.Add(newRow);

                //DataTable returnDt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLLTLOAD_MATERIAL", "INDATA", "OUTDATA", newDt);
                //DataTableConverter.SetValue(dgStockList.Rows[e.Cell.Row.Index].DataItem, "REQWEIGHT", Util.StringToDouble(dt.Rows[e.Cell.Row.Index - 2]["REQPALLET"].ToString()) * Util.StringToDouble(returnDt.Rows[0]["PLLT_LOAD_MTRL_WEIGHT"].ToString()));

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null)
                {

                    switch (Convert.ToString(e.Cell.Column.Name))
                    {

                        case "CHK":
                            break;

                        case "REQPALLET":
                            DataRow newRow = newDt.NewRow();
                            newRow["MTRLID"] = dt.Rows[e.Cell.Row.Index - 2]["MTRLID"].ToString();
                            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                            newDt.Rows.Add(newRow);

                            DataTable returnDt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLLTLOAD_MATERIAL", "INDATA", "OUTDATA", newDt);

                            if( returnDt.Rows.Count == 0 )
                            {
                                DataRow dr = returnDt.NewRow();
                                dr["PLLT_LOAD_MTRL_WEIGHT"] = "100";
                                returnDt.Rows.Add(dr);
                            }



                            DataTableConverter.SetValue(dgStockList.Rows[e.Cell.Row.Index].DataItem, "REQWEIGHT", Util.StringToDouble(dt.Rows[e.Cell.Row.Index - 2]["REQPALLET"].ToString()) * Util.StringToDouble(returnDt.Rows[0]["PLLT_LOAD_MTRL_WEIGHT"].ToString()));
                            //dt.Rows[e.Cell.Row.Index - 2]["REQWEIGHT"] = Util.StringToDouble(dt.Rows[e.Cell.Row.Index - 2]["REQPALLET"].ToString()) * Util.StringToDouble(returnDt.Rows[0]["PLLT_LOAD_MTRL_WEIGHT"].ToString());
                            //Util.GridSetData(dgStockList, dt, FrameOperation);
                            break;

                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }

    public static class IRfcTableExtensions2
    {
        /// <summary>
        /// Converts SAP table to .NET DataTable
        /// </summary>
        /// <param name="sapTable">The SAP table to convert.</param>
        /// <returns></returns>
        public static DataTable ToDataTable2(this IRfcTable sapTable, string name)
        {
            DataTable adoTable = new DataTable(name);

            for (int liElement = 0; liElement < sapTable.ElementCount; liElement++)
            {
                RfcElementMetadata metadata = sapTable.GetElementMetadata(liElement);
                adoTable.Columns.Add(metadata.Name, GetDataType1(metadata.DataType));
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

        public enum DayOfWeek
        {
            Sunday = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6,
        }

        private static Type GetDataType1(RfcDataType rfcDataType)
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