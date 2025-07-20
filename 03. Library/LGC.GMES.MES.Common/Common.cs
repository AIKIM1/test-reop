//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Configuration;
//using System.Diagnostics;
//using System.Runtime.CompilerServices;

using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Data;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LGC.GMES.MES.MainFrame")]

namespace LGC.GMES.MES.Common
{
    public class Common
    {
        public static string BizActorIP = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "BizActorIP"];
        public static string BizActorProtocol = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "BizActorProtocol"];
        public static string BizActorPort = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "BizActorPort"];
        public static string BizActorServiceMode = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "BizActorServiceMode"];
        public static string BizActorServiceIndex = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "BizActorServiceIndex"];
        public static string GLABELAccess = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "GLABELAccess"];
        public static string DeploymentUrl = string.Empty;
        public static string APP_MODE = ConfigurationManager.AppSettings["APP_CONFIG_MODE"];
        public static string APP_System = ConfigurationManager.AppSettings["APP_CONFIG_SYSTEM"];
        public static string APP_SBC_MODE = ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"];

        //Solace Connection Info
        public static string APP_REMOTE_CALL_SYSTEM = ConfigurationManager.AppSettings["APP_CONFIG_REMOTE_CALL_SYSTEM"];
        public static string SOL_HOST = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "HOST"];
        public static string SOL_VPN = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "VPNNAME"];
        public static string SOL_ID = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "ID"];
        public static string SOL_PW = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "PW"];
        public static string SOL_PROPERTIES = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "PROPERTIES"];
        public static string SOL_QUEUE = ConfigurationManager.AppSettings[LoginInfo.SYSIDSUB + "_" + "QUEUE"];

        internal static string bizloglevel = "-1";
        public static string BIZLOGLEVEL
        {
            get
            {
                return bizloglevel;
            }
        }

        internal static void SetBizActorInfo(string bizActorIP, string bizActorProtocol, string bizActorPort, string bizActorServiceMode, string bizActorServiceIndex)
        {
            BizActorIP = bizActorIP;
            BizActorProtocol = bizActorProtocol;
            BizActorPort = bizActorPort;
            BizActorServiceMode = bizActorServiceMode;
            BizActorServiceIndex = bizActorServiceIndex;
        }

        public DataTable GetBizInfoListCallXML(string serviceType, string layer, string group, string component, string bizID)
        {
            try
            {
                // Get EndPoint
                string url = string.Format(@"net.tcp://{0}:4530/BizActorInfoServer", Common.BizActorIP);
                //string url = string.Format(@"net.tcp://{0}:4530/BizActorInfoServer", this.BizConnect.Bizservice.ServiceName);
                Uri uri = new Uri(url);
                EndpointAddress WcfEndPointAddress = new EndpointAddress(uri);

                // Get Binding
                Binding wcfBinding = CreateTcpBinding();

                // Get ServiceClient
                BizInfoService.BizActorInfoServerClient bizInfoSvc = new BizInfoService.BizActorInfoServerClient(wcfBinding, WcfEndPointAddress);

                DataSet dsDid = null;
                if (!string.IsNullOrEmpty(layer + group + component + bizID))
                {
                    dsDid = new DataSet();

                    DataTable dtDepId = new DataTable("DEPLOY_ID");
                    dtDepId.Columns.Add("LAYER");
                    dtDepId.Columns.Add("GRP_NAME");
                    dtDepId.Columns.Add("COM_ID");
                    dtDepId.Columns.Add("ID");
                    dsDid.Tables.Add(dtDepId);

                    DataRow dr = dtDepId.NewRow();
                    if (!string.IsNullOrEmpty(layer))
                        dr["LAYER"] = layer;
                    if (!string.IsNullOrEmpty(group))
                        dr["GRP_NAME"] = group;
                    if (!string.IsNullOrEmpty(component))
                        dr["COM_ID"] = component;
                    if (!string.IsNullOrEmpty(bizID))
                        dr["ID"] = bizID;

                    dtDepId.Rows.Add(dr);
                    dtDepId.AcceptChanges();
                }

                DataSet Result = bizInfoSvc.GetServiceInfo(serviceType, dsDid, Common.BizActorServiceIndex);

                //DataSet Result = bizInfoSvc.GetServiceInfo(serviceType, dsDid, this.BizConnect.Bizservice.ServiceIndex);
                //StringWriter strSW = new StringWriter();
                //XmlTextWriter xw = new XmlTextWriter(strSW);
                //Result.WriteXml(xw, XmlWriteMode.WriteSchema);
                //return strSW.ToString();

                string strselect = string.Format("LAYER = '{0}'", layer);
                //string strselect = string.Format("LAYER = '{0}' AND GRP_NAME = '{1}'", layer, group);
                DataTable dtFilter = Result.Tables[0].Select(strselect).CopyToDataTable();

                return dtFilter;
            }
            catch (Exception ex)
            {
                DataSet Result = new DataSet();

                DataTable dtException = new DataTable();
                //Ex.Data
                ObservableCollection<Dictionary<string, object>> obList = new ObservableCollection<Dictionary<string, object>>();
                Dictionary<string, object> exception = new Dictionary<string, object>();

                dtException.Columns.Add("_BizException_");
                dtException.Columns.Add("DATA");

                foreach (string ex_key in ex.Data.Keys)
                {
                    if (!dtException.Columns.Contains(ex_key))
                    {
                        dtException.Columns.Add(ex_key);
                    }
                }

                DataRow newrow = dtException.NewRow();

                newrow["_BizException_"] = true;
                newrow["DATA"] = ex.Message;

                foreach (string ex_key in ex.Data.Keys)
                {
                    newrow[ex_key] = ex.Data[ex_key];
                }

                dtException.Rows.Add(newrow);

                //Result.Tables.Add(dtException);
                //StringWriter TX = new StringWriter();
                //XmlTextWriter xw = new XmlTextWriter(TX);
                //Result.WriteXml(xw, XmlWriteMode.WriteSchema);
                //return TX.ToString();

                return dtException;
            }
        }

        public DataSet GetBizInfoCallXML(string bizRuleID)
        {
            try
            {
                DateTime STTime = DateTime.Now;
                string url = string.Format(@"net.tcp://{0}:4530/BizActorInfoServer", Common.BizActorIP);
                //string url = string.Format(@"net.tcp://{0}:4530/BizActorInfoServer", this.BizConnect.Bizservice.ServiceName);
                Uri uri = new Uri(url);
                EndpointAddress WcfEndPointAddress = new EndpointAddress(uri);
                Binding wcfBinding = CreateTcpBinding();
                BizInfoService.BizActorInfoServerClient bizInfoSvc = new BizInfoService.BizActorInfoServerClient(wcfBinding, WcfEndPointAddress);

                DataSet dsInput = new DataSet();
                DataTable dtInData = new DataTable("RQST");
                dtInData.Columns.Add("SVC_ID");

                if (bizRuleID != null)
                {
                    DataRow dataRow = dtInData.NewRow();
                    dataRow["SVC_ID"] = bizRuleID;
                    dtInData.Rows.Add(dataRow);
                }

                dsInput.Tables.Add(dtInData);

                DataSet Result = bizInfoSvc.GetServiceInfo("bizactor_svcinfo", dsInput, Common.BizActorServiceIndex);
                //DataSet Result = bizInfoSvc.GetServiceInfo("bizactor_svcinfo", dsInput, this.BizConnect.Bizservice.ServiceIndex);

                //StringWriter strSW = new StringWriter();
                //XmlTextWriter xw = new XmlTextWriter(strSW);
                //Result.WriteXml(xw, XmlWriteMode.WriteSchema);

                //return strSW.ToString();
                return Result;
            }
            catch (Exception ex)
            {
                DataSet Result = new DataSet();

                DataTable dtException = new DataTable();
                //Ex.Data
                ObservableCollection<Dictionary<string, object>> obList = new ObservableCollection<Dictionary<string, object>>();
                Dictionary<string, object> exception = new Dictionary<string, object>();

                dtException.Columns.Add("_BizException_");
                dtException.Columns.Add("DATA");

                foreach (string ex_key in ex.Data.Keys)
                {
                    if (!dtException.Columns.Contains(ex_key))
                    {
                        dtException.Columns.Add(ex_key);
                    }
                }

                DataRow newrow = dtException.NewRow();

                newrow["_BizException_"] = true;
                newrow["DATA"] = ex.Message;

                foreach (string ex_key in ex.Data.Keys)
                {
                    newrow[ex_key] = ex.Data[ex_key];
                }

                dtException.Rows.Add(newrow);

                Result.Tables.Add(dtException);

                //StringWriter TX = new StringWriter();
                //XmlTextWriter xw = new XmlTextWriter(TX);
                //Result.WriteXml(xw, XmlWriteMode.WriteSchema);

                //return TX.ToString();
                return Result;
            }
        }

        private Binding CreateTcpBinding()
        {
            List<BindingElement> elements = new List<BindingElement>();
            TcpTransportBindingElement tcpBinding = new TcpTransportBindingElement();
            tcpBinding.ManualAddressing = false;
            tcpBinding.MaxBufferSize = 2147483647;
            tcpBinding.MaxReceivedMessageSize = 2147483647;
            elements.Add(tcpBinding);
            CustomBinding binding = new CustomBinding(elements);
            return binding;
        }
    }
}
