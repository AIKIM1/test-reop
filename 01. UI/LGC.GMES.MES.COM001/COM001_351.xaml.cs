/*************************************************************************************
 Created Date : 2021.03.05
      Creator : 오광택
   Decription : 믹서 원자재 재고조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.05          : Initial Created.
  2024.09.09          : 김영택, RFC 연결 변경 (NERP 적용) 
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

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// </summary>
    public partial class COM001_351 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation { get; set; }
        RfcDestination dest;

        // 2024.09.10 NERP 적용 여부 FLAG 
        private bool IS_NERP_FLAG = false;

        public COM001_351()
        {
            InitializeComponent();
            SetComboBox(cboArea);
            SetComboBox(cboLine);
            SetComboBox(cboStockLocation);

            //dtpEndStockYM.Text = System.DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            //dtpEndStockYM.SelectedDateTime = System.DateTime.Now.AddMonths(-1);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 2024.09.10 NERP 연결 적용 
            //ERPDestinationConfig cfg = new ERPDestinationConfig();
            NERPDestinationConfig cfg = new NERPDestinationConfig();

            // NERP 적용 판단 
            if (IsNerpApplyFlag().Equals("Y")) { IS_NERP_FLAG = true; }

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }

            if (IS_NERP_FLAG == true) // NERP 적용시
            {
                dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "ESP" : "ESQ");
            }
            else
            {
                // app.debug.config (APP_SERVER): 개발에만 존재하는 듯 
                //dest = RfcDestinationManager.GetDestination(ConfigurationManager.AppSettings["APP_SERVER"].Contains("DEV") ? "QAS" : "PRD");
                dest = RfcDestinationManager.GetDestination(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["APP_SERVER"]) == true ? "PRD" : "QAS");
            }
            //chkFinlwip.IsChecked = true;
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
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_AUTH_AREA_CBO", cbo, new string[] { "LANGID", "SHOPID", "USERID", "SYSTEM_ID" }, new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.USERID, LoginInfo.SYSID }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;
                case "cboLine":
                    CommonCombo.CommonBaseCombo("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", cbo, new string[] { "LANGID", "AREAID" }, new string[] {  LoginInfo.LANGID, cboArea.SelectedValue as string}, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;

                case "cboStockLocation":
                    if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("ALL"))
                        CommonCombo.CommonBaseCombo("DA_BAS_SEL_SLOC_BY_SHOP", cbo, new string[] { "SHOPID" }, new string[] { LoginInfo.CFG_SHOP_ID }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    else
                        CommonCombo.CommonBaseCombo("DA_BAS_SEL_SLOC_BY_SHOP", cbo, new string[] { "SHOPID", "AREAID" }, new string[] { LoginInfo.CFG_SHOP_ID, cboArea.SelectedValue as string }, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME");
                    break;
            }
        }



        void GetStocks()
        {
            #region Initialize Grid
            dgStockList.ItemsSource = null;
            #endregion Initialize Grid

            #region Get GMES Stock List
            if ((bool)rdoDay.IsChecked)
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SUM_TYPE", typeof(string));
                IndataTable.Columns.Add("SUM_YM", typeof(string));
                IndataTable.Columns.Add("SUM_DATE", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("SLOC_ID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LINEID", typeof(string));
                IndataTable.Columns.Add("FINL_WIP_FLAG", typeof(string));

                DataRow Indata = IndataTable.NewRow();

                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["SLOC_ID"] = string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()) ? null : cboStockLocation.SelectedValue;
                Indata["AREAID"] = cboArea.SelectedValue;
                Indata["LINEID"] = cboLine.SelectedValue;
                Indata["FINL_WIP_FLAG"] = "N";
                //if ((bool)rdoCurrent.IsChecked)
                //{
                //    Indata["SUM_DATE"] = DateTime.Now.ToString("yyyyMMdd");
                //}
                if ((bool)rdoDay.IsChecked)
                {
                    Indata["SUM_TYPE"] = Util.NVC(rdoDay.Tag);
                    Indata["SUM_DATE"] = dtpDate.SelectedDateTime.ToString("yyyyMMdd");
                }
                if ((bool)rdoCurrent.IsChecked)
                    Indata["SUM_TYPE"] = Util.NVC(rdoCurrent.Tag);
                IndataTable.Rows.Add(Indata);

                DataTable dt;
                dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCKLIST_SUM", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgStockList, dt, FrameOperation);
            }

            #endregion Get GMES Stock List
            else
            {
                DataTable rfcDt = new DataTable();

                try
                {
                    #region Get ERP Stock List

                    string sapTable = IS_NERP_FLAG == true ? "TT_STOCK" : "IT_STOCK";

                    RfcRepository repo = dest.Repository;

                    IRfcFunction FnStockList = repo.CreateFunction("ZPPB_SEND_ERP_STOCK");
                    

                    if (IS_NERP_FLAG == true)
                    {
                        FnStockList.SetValue("IV_WERKS", LoginInfo.CFG_SHOP_ID);
                        if (!string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                            FnStockList.SetValue("IV_LGORT", cboStockLocation.SelectedValue);
                    }
                    else
                    {
                        FnStockList.SetValue("I_WERKS", LoginInfo.CFG_SHOP_ID);
                        if (!string.IsNullOrWhiteSpace(cboStockLocation.SelectedValue?.ToString()))
                            FnStockList.SetValue("I_LGORT", cboStockLocation.SelectedValue);
                    }

                    FnStockList.Invoke(dest);

                    var stockList = FnStockList.GetTable(sapTable);
                    var dtStockList = stockList.ToDataTable1(sapTable);
                    #endregion Get ERP Stock List

                    #region Merge Based on GMES

                    
                    rfcDt.Columns.Add("MTRLID", typeof(string));
                    rfcDt.Columns.Add("MTRLDESC", typeof(string));
                    rfcDt.Columns.Add("MTRLUNIT", typeof(string));
                    rfcDt.Columns.Add("SUM_QTY2", typeof(decimal));
                    rfcDt.Columns.Add("HOLD_LOT_QTY2", typeof(decimal));
                    rfcDt.Columns.Add("MOVING_LOT_QTY2", typeof(decimal));
                    rfcDt.Columns.Add("TOTAL_QTY2", typeof(decimal));

                    

                    foreach (DataRow dr in (dtStockList as DataTable).Rows)
                    {
                        DataView dv = new DataView();
                        if (cboStockLocation.SelectedValue == null)
                        {
                            dv = dtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == LoginInfo.CFG_SHOP_ID).AsDataView();
                        }
                        else
                        {
                            dv = dtStockList.AsEnumerable().Where(x => x.Field<string>("WERKS") == LoginInfo.CFG_SHOP_ID && x.Field<string>("LGORT") == cboStockLocation.SelectedValue.ToString()).AsDataView();
                        }

                        if( dv.Count > 0)
                        {
                            DataRow newRow = rfcDt.NewRow();

                            newRow["MTRLID"] = dr["MATNR"];
                            newRow["MTRLDESC"] = dr["MAKTX"];
                            newRow["MTRLUNIT"] = dr["MEINS"];
                            newRow["SUM_QTY2"] = dr["LABST"];
                            newRow["HOLD_LOT_QTY2"] = dr["SPEME"];
                            newRow["MOVING_LOT_QTY2"] = dr["UMLME"];
                            newRow["TOTAL_QTY2"] = Convert.ToDouble(dr["LABST"]) + Convert.ToDouble(dr["SPEME"]);
                            rfcDt.Rows.Add(newRow);
                        }
                    }
                    #endregion Merge Base on GMES
                }
                catch (Exception ex) { }

                Util.GridSetData(dgStockList, rfcDt, FrameOperation);
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

                DataRow newRow = inTable.NewRow();
                newRow["SUM_TYPE"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_TYPE"));
                newRow["SUM_YM"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_YM"));
                newRow["SUM_DATE"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SUM_DATE"));
                newRow["SHOPID"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SHOPID"));
                newRow["AREAID"] = cboArea.SelectedValue;
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "PRODID"));
                newRow["SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dataitem.DataItem, "SLOC_ID"));

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

        IRfcTable GetTableByRfcCall(string destName, int rowCount)
        {
            RfcDestination dest = RfcDestinationManager.GetDestination(destName);
            IRfcFunction func = dest.Repository.CreateFunction("ZPPB_SEND_ERP_STOCK");

            func.SetValue("I_WERKS", LoginInfo.CFG_SHOP_ID);
            func.SetValue("I_LGORT", cboStockLocation.SelectedValue);
            //func.SetValue("I_MATNR", txtProdId.Text.Trim());

            IRfcTable rfcTable = func.GetTable("IT_STOCK");

            return rfcTable;
        }

        private void cboLine_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //SetComboBox(cboStockLocation);
        }
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetComboBox(cboLine);
            SetComboBox(cboStockLocation);
        }

        private void rdoCurrent_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtpDate != null)
                    dtpDate.IsEnabled = false;
                //if (dtpMonth != null)
                //    dtpMonth.IsEnabled = false;
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
                //if (dtpMonth != null)
                //    dtpMonth.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

    }

    public class ERPDestinationConfig1 : IDestinationConfiguration
    {
        public bool ChangeEventsSupported()
        {
            return false;
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public RfcConfigParameters GetParameters(string destinationName)
        {
            RfcConfigParameters parms = new RfcConfigParameters();

            if (destinationName.Equals("PRD"))
            {
                //parms.Add(RfcConfigParameters.AppServerHost, LoginInfo.SYSID.Contains("KR") ? "165.244.235.102" : "165.244.235.119");
                parms.Add(RfcConfigParameters.MessageServerHost, LoginInfo.SYSID.Contains("KR") ? "10.94.36.35" : "165.244.235.119");
                //parms.Add(RfcConfigParameters.SystemNumber, "00");
                parms.Add(RfcConfigParameters.SystemID, LoginInfo.SYSID.Contains("KR") ? "LBP" : "GPD");
                parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
                parms.Add(RfcConfigParameters.Password, LoginInfo.SYSID.Contains("KR") ? "lgchem2016" : "RFCGMES01001");
                parms.Add(RfcConfigParameters.Client, "100");
                parms.Add(RfcConfigParameters.LogonGroup, LoginInfo.SYSID.Contains("KR") ? "LBPALL" : "LGGPD");
                parms.Add(RfcConfigParameters.Language, "KO");
                parms.Add(RfcConfigParameters.PoolSize, "5");
            }
            else if (destinationName.Equals("QAS"))
            {
                //parms.Add(RfcConfigParameters.AppServerHost, "165.244.235.188");
                parms.Add(RfcConfigParameters.MessageServerHost, LoginInfo.SYSID.Contains("KR") ? "10.94.36.231" : "165.244.235.170");
                //parms.Add(RfcConfigParameters.SystemNumber, "00");
                parms.Add(RfcConfigParameters.SystemID, LoginInfo.SYSID.Contains("KR") ? "LBQ" : "GQS");
                parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
                parms.Add(RfcConfigParameters.Password, "lgchem2016");
                parms.Add(RfcConfigParameters.Client, "100");
                parms.Add(RfcConfigParameters.LogonGroup, LoginInfo.SYSID.Contains("KR") ? "LBQALL" : "LGGQS");
                parms.Add(RfcConfigParameters.Language, "KO");
                parms.Add(RfcConfigParameters.PoolSize, "5");
            }

            return parms;
        }
    }

    public static class IRfcTableExtensions1
    {
        /// <summary>
        /// Converts SAP table to .NET DataTable
        /// </summary>
        /// <param name="sapTable">The SAP table to convert.</param>
        /// <returns></returns>
        public static DataTable ToDataTable1(this IRfcTable sapTable, string name)
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