using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Controls;
using SAP.Middleware.Connector;

using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class RfcHandler : Control
    {

        public static DataTable GetErpStockList(string sShop, string sStockLocation, string sProdId, string sEndStockMonth)
        {
            RfcDestination dest;
            ERPDestinationConfig cfg = new ERPDestinationConfig();

            if (!LoginInfo.IS_CONNECTED_ERP)
            {
                RfcDestinationManager.RegisterDestinationConfiguration(cfg);
                LoginInfo.IS_CONNECTED_ERP = true;
            }

            dest = RfcDestinationManager.GetDestination("PRD");

            #region Get ERP Stock List
            RfcRepository repo = dest.Repository;

            IRfcFunction FnStockList = repo.CreateFunction("ZPPB_SEND_ERP_STOCK");
            FnStockList.SetValue("I_WERKS", sShop);

            if (!string.IsNullOrEmpty(sEndStockMonth))
                FnStockList.SetValue("I_YYYYMM", sEndStockMonth);

            if (!string.IsNullOrEmpty(sStockLocation))
                FnStockList.SetValue("I_LGORT", sStockLocation);

            if (!string.IsNullOrEmpty(sProdId))
                FnStockList.SetValue("I_MATNR", sProdId);

            FnStockList.Invoke(dest);

            var stockList = FnStockList.GetTable("IT_STOCK");
            var dtStockList = stockList.ToDataTable("IT_STOCK");
            #endregion Get ERP Stock List

            return dtStockList;
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

                // 20201204 LG ENSol 분사 처리 이전 erp 주소 
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
                //    parms.Add(RfcConfigParameters.MessageServerHost, LoginInfo.SYSID.Contains("KR") ? "165.244.235.84" : "165.244.235.170");
                //    //parms.Add(RfcConfigParameters.SystemNumber, "00");
                //    parms.Add(RfcConfigParameters.SystemID, LoginInfo.SYSID.Contains("KR") ? "QAS" : "GQS");
                //    parms.Add(RfcConfigParameters.User, "RFC_GMES_01");
                //    parms.Add(RfcConfigParameters.Password, "lgchem2016");
                //    parms.Add(RfcConfigParameters.Client, "100");
                //    parms.Add(RfcConfigParameters.LogonGroup, LoginInfo.SYSID.Contains("KR") ? "QASALL" : "LGGQS");
                //    parms.Add(RfcConfigParameters.Language, "KO");
                //    parms.Add(RfcConfigParameters.PoolSize, "5");
                //}

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
