using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SolaceSystems.Solclient.Messaging.SDT;
using SolaceSystems.Solclient.Messaging;
using com.lgcns.mom;
using System.Net;
using LGC.GMES.MES.Common.ConfigInfos;
using System.IO;
using System.Linq;

namespace LGC.GMES.MES.Common
{
    public delegate void ExecuteServiceCompleteMom(DataSet result, Exception ex);
    public class ClientProxyMom
    {
        private static string clientId = Guid.NewGuid().ToString("N");
        private static MWSolaceClient _MWSolaceClient = null;
        public static TimeSpan APServerTimeZoneDiff = TimeZoneInfo.Local.BaseUtcOffset;
        private static int _DefaultTimeOut = 60;
        private static int _DefaultCompressLevel = 0;
        private static JObject _ConnectionStr = null;
        private static JObject _ConnectionStrBiz = null;
        private static JObject _ConnectionStrSql = null;
        private static JObject _ConnectionStrTest = null;
        private static string _bizQueue = null;
        private static string _sqlQueue = null;
        private static string _testQueue = null;
        private static bool reuseSession = true;
        private static QueueNameType queueType = QueueNameType.Unique;
        private static EventHandler _ReceivedTopic;

        private static MWConfigInfo solConfigInfo = new MWConfigInfo();

        static ClientProxyMom()
        {
            _MWSolaceClient = new MWSolaceClient();
            initialize();
        }

        public static void initialize(EventHandler<SessionEventArgs> onSessionEvent = null)
        {
            try
            {
                
                solConfigInfo.HOST = LGC.GMES.MES.Common.Common.SOL_HOST;
                solConfigInfo.VPNNAME = LGC.GMES.MES.Common.Common.SOL_VPN;
                solConfigInfo.USERNAME = LGC.GMES.MES.Common.Common.SOL_ID;
                solConfigInfo.PASSWORD = LGC.GMES.MES.Common.Common.SOL_PW;
                solConfigInfo.PROPERTIES = LGC.GMES.MES.Common.Common.SOL_PROPERTIES;
                solConfigInfo.MRS_QUEUE = LGC.GMES.MES.Common.Common.SOL_QUEUE;

                //if (queueType == QueueNameType.Multiple)
                //{
                //    if (!string.IsNullOrEmpty(Variables.appConfigInfo.FACILITY_CODE))
                //    {
                //        _bizQueue = solConfigInfo.MRS_QUEUE + "/" + Variables.appConfigInfo.FACTORY_CODE + "/" + Variables.appConfigInfo.FACILITY_CODE;// "/E5"; // PROC_TYPE/ECM_MES/UIF/FACTORY/FACILITY 
                //        _sqlQueue = solConfigInfo.SQL_QUEUE + "/" + Variables.appConfigInfo.FACTORY_CODE + "/" + Variables.appConfigInfo.FACILITY_CODE;// PROC_TYPE/ECM_MES/SQL/FACTORY/FACILITY 
                //    }
                //    else if (!string.IsNullOrEmpty(Variables.appConfigInfo.FACTORY_CODE))
                //    {
                //        _bizQueue = solConfigInfo.MRS_QUEUE + "/" + Variables.appConfigInfo.FACTORY_CODE;
                //        _sqlQueue = solConfigInfo.SQL_QUEUE + "/" + Variables.appConfigInfo.FACTORY_CODE;
                //    }
                //    else
                //    {
                //        _bizQueue = solConfigInfo.MRS_QUEUE;
                //        _sqlQueue = solConfigInfo.SQL_QUEUE;
                //    }
                //}
                //else
                //{
                //    _bizQueue = solConfigInfo.MRS_QUEUE;
                //    _sqlQueue = solConfigInfo.SQL_QUEUE;
                //}

                //_testQueue = solConfigInfo.TEST_QUEUE;

                _bizQueue = solConfigInfo.MRS_QUEUE;

                _ConnectionStr = new JObject();
                _ConnectionStr.Add("HOST", solConfigInfo.HOST);
                _ConnectionStr.Add("VPN_NAME", solConfigInfo.VPNNAME);
                _ConnectionStr.Add("USERNAME", solConfigInfo.USERNAME);
                _ConnectionStr.Add("PASSWORD", solConfigInfo.PASSWORD);

                if (!_ConnectionStr.ContainsKey("CONNECTION_RETRY"))
                {
                    _ConnectionStr.Add("CONNECTION_RETRY", -1);  //재접속 무제한
                }

                if (!_ConnectionStr.ContainsKey("CONNECTION_RETRY_WAIT_MS"))
                {
                    _ConnectionStr.Add("CONNECTION_RETRY_WAIT_MS", 10000); //재접속 시도 주기 (10Sec)
                }

                if (!_ConnectionStr.ContainsKey("COMPRESSION_LEVEL"))
                {
                    _ConnectionStr.Add("COMPRESSION_LEVEL", _DefaultCompressLevel); //COMPRESSION_LEVEL 표준 정의 필요
                }

                if (!string.IsNullOrEmpty(solConfigInfo.PROPERTIES))
                {
                    foreach (var item in solConfigInfo.PROPERTIES.Split(';'))
                    {
                        _ConnectionStr.Add(item.Split(':')[0].ToString(), item.Split(':')[1].ToString());
                    }
                }

                _ConnectionStrBiz = new JObject();
                _ConnectionStrBiz.Add("QUEUE_NAME", _bizQueue);
                _ConnectionStrBiz.Add("REPLY_TO_QUEUE", "REPLY/" + _bizQueue);
                _ConnectionStrBiz.Add("DELIVERY_MODE", MessageDeliveryMode.Persistent.ToString());
                _ConnectionStrBiz.Add("TIMEOUT_MS", _DefaultTimeOut * 1000);
                _ConnectionStrBiz.Add("BINDINGTIMEOUT_MS", 5000); //BindingtimeoutInMsec 5초로 지정

                //_ConnectionStrSql = new JObject();
                //_ConnectionStrSql.Add("QUEUE_NAME", _sqlQueue);
                //_ConnectionStrSql.Add("REPLY_TO_QUEUE", "REPLY/" + _sqlQueue);
                //_ConnectionStrSql.Add("DELIVERY_MODE", MessageDeliveryMode.Direct.ToString());
                //_ConnectionStrSql.Add("TIMEOUT_MS", _DefaultTimeOut * 1000);
                //_ConnectionStrSql.Add("BINDINGTIMEOUT_MS", 5000); //BindingtimeoutInMsec 5초로 지정

                //_ConnectionStrTest = new JObject();
                //_ConnectionStrTest.Add("QUEUE_NAME", _testQueue);
                //_ConnectionStrTest.Add("REPLY_TO_QUEUE", "REPLY/" + _testQueue);
                //_ConnectionStrTest.Add("DELIVERY_MODE", MessageDeliveryMode.Direct.ToString());
                //_ConnectionStrTest.Add("TIMEOUT_MS", _DefaultTimeOut * 1000);
                //_ConnectionStrTest.Add("BINDINGTIMEOUT_MS", 5000); //BindingtimeoutInMsec 5초로 지정


                if (_MWSolaceClient.helper.current_Session != null)
                {
                    _MWSolaceClient.disConnectSession();
                    _MWSolaceClient.closeSession();
                }

                if (reuseSession)
                {
                    _MWSolaceClient.createSession(_ConnectionStr.ToString(), new EventHandler((s, e) =>
                    {
                        if (onSessionEvent != null)
                        {
                            onSessionEvent(s, (SessionEventArgs)e);
                        }
                    }));
                }
                else
                {
                    _MWSolaceClient.createSession(_ConnectionStr.ToString());
                }

                reuseSession = (solConfigInfo.CONNECTION_MODE == "CO" ? true : false);

                if (reuseSession)
                {
                    _MWSolaceClient.connectSession();
                }

                ContextFactoryProperties cfp = new ContextFactoryProperties()
                {
                    SolClientLogLevel = SolLogLevel.Warning
                };
                cfp.LogDelegate += SolaceLog;

                ContextFactory.Instance.Init(cfp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        private static void SolaceLog(SolLogInfo logInfo)
        {
            //Logger.GetInstance().WriteException(LogLevel.EXCEPTION, "Solace Event:" + logInfo.LogMessage, null);
            Logger.Instance.WriteLine("Solace Exception", "Event:" + logInfo.LogMessage, LogCategory.BIZ);
        }

        public static bool connectionCheck(MWConfigInfo mWConfigInfo)
        {
            bool checkResult = false;
            MWSolaceClient _tmpMWSolaceClient = new MWSolaceClient();
            try
            {
                string _tmpbizQueue = string.Empty;
                string _tmpsqlQueue = string.Empty;
                JObject _tmpConnectionStr;
                JObject _tmpConnectionStrBiz;
                JObject _tmpConnectionStrSql;

                _tmpConnectionStr = new JObject();
                _tmpConnectionStr.Add("HOST", mWConfigInfo.HOST);
                _tmpConnectionStr.Add("VPN_NAME", mWConfigInfo.VPNNAME);
                _tmpConnectionStr.Add("USERNAME", mWConfigInfo.USERNAME);
                _tmpConnectionStr.Add("PASSWORD", mWConfigInfo.PASSWORD);

                if (!string.IsNullOrEmpty(solConfigInfo.PROPERTIES))
                {
                    foreach (var item in solConfigInfo.PROPERTIES.Split(';'))
                    {
                        _tmpConnectionStr.Add(item.Split(':')[0].ToString(), item.Split(':')[1].ToString());
                    }
                }

                if (!_tmpConnectionStr.ContainsKey("CONNECTION_RETRY"))
                {
                    _tmpConnectionStr.Add("CONNECTION_RETRY", -1);  //재접속 무제한
                }

                if (!_tmpConnectionStr.ContainsKey("CONNECTION_RETRY_WAIT_MS"))
                {
                    _tmpConnectionStr.Add("CONNECTION_RETRY_WAIT_MS", 10000); //재접속 시도 주기 (10Sec)
                }

                if (!_tmpConnectionStr.ContainsKey("COMPRESSION_LEVEL"))
                {
                    _tmpConnectionStr.Add("COMPRESSION_LEVEL", _DefaultCompressLevel); //COMPRESSION_LEVEL 표준 정의 필요
                }

                _tmpConnectionStrBiz = new JObject();
                _tmpConnectionStrBiz.Add("QUEUE_NAME", _bizQueue);
                _tmpConnectionStrBiz.Add("REPLY_TO_QUEUE", "REPLY/" + _bizQueue);
                _tmpConnectionStrBiz.Add("DELIVERY_MODE", MessageDeliveryMode.Persistent.ToString());
                _tmpConnectionStrBiz.Add("TIMEOUT_MS", _DefaultTimeOut * 1000);

                //_tmpConnectionStrSql = new JObject();
                //_tmpConnectionStrSql.Add("QUEUE_NAME", _sqlQueue);
                //_tmpConnectionStrSql.Add("REPLY_TO_QUEUE", "REPLY/" + _sqlQueue);
                //_tmpConnectionStrSql.Add("DELIVERY_MODE", MessageDeliveryMode.Direct.ToString());
                //_tmpConnectionStrSql.Add("TIMEOUT_MS", _DefaultTimeOut * 1000);

                _tmpMWSolaceClient.createSession(_tmpConnectionStr.ToString());
                _tmpMWSolaceClient.connectSession();
                checkResult = true;
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if (checkResult)
                {
                    _tmpMWSolaceClient.disConnectSession();
                    _tmpMWSolaceClient.closeSession();
                }
            }

            return checkResult;
        }

        private static void convertToAPServerDateTime(DataTable table)
        {
            if (table == null)
                return;

            foreach (DataColumn column in table.Columns)
                if (column.DataType.Equals(typeof(DateTime)))
                    foreach (DataRow row in table.Rows)
                        if (row[column] != null && !DBNull.Value.Equals(row[column]))
                            row[column] = ((DateTime)row[column]).AddMinutes(-(APServerTimeZoneDiff.Subtract(TimeZoneInfo.Local.BaseUtcOffset).TotalMinutes));
        }

        private static void convertToAPServerDateTime(DataSet set)
        {
            if (set == null)
                return;

            foreach (DataTable table in set.Tables)
                convertToAPServerDateTime(table);
        }

        private static void convertToClientDateTime(DataTable table)
        {
            if (table == null)
                return;

            foreach (DataColumn column in table.Columns)
                if (column.DataType.Equals(typeof(DateTime)))
                    foreach (DataRow row in table.Rows)
                        if (row[column] != null && !DBNull.Value.Equals(row[column]))
                            row[column] = ((DateTime)row[column]).AddMinutes(APServerTimeZoneDiff.Subtract(TimeZoneInfo.Local.BaseUtcOffset).TotalMinutes);
        }

        private static void convertToClientDateTime(DataSet set)
        {
            if (set == null)
                return;

            foreach (DataTable table in set.Tables)
                convertToClientDateTime(table);
        }

        private static string GetLangCode()
        {
            string resultData = "ENG";
            switch ((LoginInfo.LANGID ?? "en-US").ToUpper())
            {
                case "KO-KR":
                    resultData = "KOR";
                    break;
                case "EN-US":
                    resultData = "ENG";
                    break;
                case "PL-PL":
                    resultData = "POL";
                    break;
                case "RU-RU":
                    resultData = "RUS";
                    break;
                case "UK-UA":
                    resultData = "URK";
                    break;
                case "ZH-CN":
                    resultData = "ZHO";
                    break;
                default:
                    resultData = "ENG";
                    break;
            }
            return resultData;
        }

        public static void ExecuteService(string bizRuleID, string indata, string outdata, DataTable indataTable, ExecuteServiceCompleteMom delegater, ProcQueueType procQueueType = ProcQueueType.BIZ, string menuid = null, bool noLogInputData = false, bool nologOutputData = true)
        {
            DataSet INDATASET = new DataSet();

            if (indataTable != null)
            {
                DataTable tmpDt = indataTable.Copy();
                INDATASET.Tables.Add(tmpDt);
            }

            ExecuteService(bizRuleID, indata, outdata, INDATASET, delegater, procQueueType, menuid, noLogInputData, nologOutputData);
        }

        public static void ExecuteService(string bizRuleID, string indata, string outdata, DataSet indataSet, ExecuteServiceCompleteMom delegater, ProcQueueType procQueueType = ProcQueueType.BIZ, string menuid = null, bool noLogInputData = false, bool nologOutputData = true)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;
            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;
            string formID = declaringType.Name;

            Thread worker = new Thread(new ThreadStart(() =>
            {
                DataSet result = null;
                Exception ex = null;

                DataSet INDATASET = indataSet;

                DateTime RCV_DATE = DateTime.Now;
                DateTime TRSM_DATE = DateTime.Now;

                try
                {
                    //convertToAPServerDateTime(INDATASET);
                    result = ExecuteServiceSync(bizRuleID, indata, outdata, INDATASET, procQueueType, menuid, noLogInputData, nologOutputData);
                    //convertToClientDateTime(result);
                }
                catch (Exception bizException)
                {
                    //Logger.GetInstance().WriteException(LogLevel.EXCEPTION, "ExecuteService():[" + bizRuleID + "] Exception", bizException);
                    Logger.Instance.WriteLine("Biz Exception", "ExecuteService():[" + bizRuleID + "]:" + bizException, LogCategory.BIZ);
                    ex = bizException;
                }
                finally
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            if (result != null)
                                result.AcceptChanges();
                            if (delegater != null)
                            {
                                delegater(result, ex);
                            }
                        }));
                    }
                    else
                    {
                        if (result != null)
                            result.AcceptChanges();

                        if (delegater != null)
                        {
                            delegater(result, ex);
                        }
                    }
                }
            }));

            worker.IsBackground = true;
            worker.Start();
        }

        public static DataSet ExecuteServiceSync(string bizRuleID, string indata, string outdata, DataTable indataTable, ProcQueueType procQueueType = ProcQueueType.BIZ, string menuid = null, bool noLogInputData = false, bool nologOutputData = true, bool RowSequenceNo = false)
        {
            DataSet INDATASET = new DataSet();

            if (indataTable != null)
            {
                DataTable tmpDt = indataTable.Copy();
                INDATASET.Tables.Add(tmpDt);
            }

            return ExecuteServiceSync(bizRuleID, indata, outdata, INDATASET, procQueueType, menuid, noLogInputData, nologOutputData);
        }

        private static string GetRequestQueu(ProcQueueType procQueueType)
        {
            if (_ConnectionStrTest != null)
            {
                return _ConnectionStrTest.ToString();
            }
            else
            {
                return (procQueueType == ProcQueueType.BIZ ? _ConnectionStrBiz.ToString() : _ConnectionStrSql.ToString()).ToString();
            }
        }

        public static DataSet ExecuteServiceSync(string bizRuleID, string indata, string outdata, DataSet indataSet, ProcQueueType procQueueType = ProcQueueType.BIZ, string menuid = null, bool noLogInputData = false, bool nologOutputData = true, bool RowSequenceNo = false)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;

            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;

            string formID = declaringType.Name;

            DataSet result = new DataSet();
            Exception ex = null;
            DataSet INDATASET = indataSet;

            DateTime RCV_DATE = DateTime.Now;

            try
            {
                byte[] reply = null;
                string replyMessageStr = string.Empty;
                string transactionID = string.Empty;

                if (ClientProxyMom.reuseSession)
                {
                    //Connection orientated 처리
                    reply = _MWSolaceClient.request(Encoding.UTF8.GetBytes(GetJsonStrValue(bizRuleID, indata, outdata, INDATASET, out transactionID)), GetRequestQueu(procQueueType));
                }
                else
                {
                    //Connectionless 처리
                    MWSolaceClient tmpMWSolaceClient = new MWSolaceClient();

                    tmpMWSolaceClient.createSession(_ConnectionStr.ToString());
                    tmpMWSolaceClient.connectSession();
                    reply = tmpMWSolaceClient.request(Encoding.UTF8.GetBytes(GetJsonStrValue(bizRuleID, indata, outdata, INDATASET, out transactionID)), GetRequestQueu(procQueueType));
                    tmpMWSolaceClient.disConnectSession();
                    tmpMWSolaceClient.closeSession();
                    tmpMWSolaceClient = null;
                }
                TimeSpan execTimeSpan = DateTime.Now - RCV_DATE;
                replyMessageStr = reply != null ? Encoding.UTF8.GetString(reply) : null;
                result = GetDataSetFromReplyMsg(replyMessageStr, outdata, bizRuleID);//2022.06.08 Oracle error 발생시에도 Biz명을 남기도록 수정

                if (!noLogInputData)
                {
                    if (!nologOutputData)
                    {
                        //Logger.GetInstance().WriteLogLine(LogLevel.DATA, string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID), INDATASET, result);
                        Logger.Instance.WriteLine("Solace Data", "Event:" + string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3} / INDATASET : {4} / result : {5}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID, INDATASET, result), LogCategory.BIZ);
                    }
                    else
                    {
                        //Logger.GetInstance().WriteLogLine(LogLevel.DATA, string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID), INDATASET);
                        Logger.Instance.WriteLine("Solace Data", "Event:" + string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3} / INDATASET : {4}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID, INDATASET), LogCategory.BIZ);
                    }
                }
            }
            catch (Exception bizException)
            {
                // 비즈룰 호출 에러 발생 시 In Parameter 표시
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("ExecuteServiceSync():[" + bizRuleID + "] Exception");
                using (StringWriter sw = new StringWriter())
                {
                    indataSet.WriteXml(sw);
                    sb.AppendLine("    Parameter : ");
                    sb.AppendLine(sw.ToString());
                }

                //Logger.Instance.WriteException(LogLevel.EXCEPTION, sb.ToString(), bizException);
                Logger.Instance.WriteLine("Biz Exception", "Event:" + sb.ToString() + "Exception Message:" + bizException, LogCategory.BIZ);
                ex = bizException;
                throw ex;
            }
            finally
            {
                if (result != null)
                    result.AcceptChanges();
            }

            return result;
        }

        public static void ExecuteService(string topic_Name, string bizRuleID, string indata, string outdata, DataSet indataSet, ExecuteServiceCompleteMom delegater, ProcQueueType procQueueType = ProcQueueType.BIZ, string menuid = null, bool noLogInputData = false, bool nologOutputData = true)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;
            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;
            string formID = declaringType.Name;

            Thread worker = new Thread(new ThreadStart(() =>
            {
                DataSet result = null;
                Exception ex = null;

                DataSet INDATASET = indataSet;

                DateTime RCV_DATE = DateTime.Now;
                DateTime TRSM_DATE = DateTime.Now;

                try
                {
                    //convertToAPServerDateTime(INDATASET);
                    result = ExecuteServiceSync(topic_Name, bizRuleID, indata, outdata, INDATASET, procQueueType, menuid, noLogInputData, nologOutputData);
                    //convertToClientDateTime(result);
                }
                catch (Exception bizException)
                {
                    //Logger.GetInstance().WriteException(LogLevel.EXCEPTION, "ExecuteService():[" + bizRuleID + "] Exception", bizException);
                    Logger.Instance.WriteLine("Biz Exception", "ExecuteService():[" + bizRuleID + "]:" + bizException, LogCategory.BIZ);
                    ex = bizException;
                }
                finally
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            if (result != null)
                                result.AcceptChanges();

                            delegater(result, ex);
                        }));
                    }
                    else
                    {
                        if (result != null)
                            result.AcceptChanges();

                        delegater(result, ex);
                    }
                }
            }));

            worker.IsBackground = true;
            worker.Start();
        }

        public static DataSet ExecuteServiceSync(string topic_Name, string bizRuleID, string indata, string outdata, DataSet indataSet, ProcQueueType procQueueType = ProcQueueType.BIZ, string menuid = null, bool noLogInputData = false, bool nologOutputData = true, bool RowSequenceNo = false)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;

            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;

            string formID = declaringType.Name;

            DataSet result = new DataSet();
            Exception ex = null;
            DataSet INDATASET = indataSet;

            DateTime RCV_DATE = DateTime.Now;

            try
            {
                byte[] reply = null;
                string replyMessageStr = string.Empty;
                string transactionID = string.Empty;

                if (ClientProxyMom.reuseSession)
                {

                    //Connection orientated 처리
                    reply = _MWSolaceClient.request(Encoding.UTF8.GetBytes(GetJsonStrValue(bizRuleID, indata, outdata, INDATASET, out transactionID)), GetTopicInfo(topic_Name, procQueueType));
                }
                else
                {
                    //Connectionless 처리
                    MWSolaceClient tmpMWSolaceClient = new MWSolaceClient();

                    tmpMWSolaceClient.createSession(_ConnectionStr.ToString());
                    tmpMWSolaceClient.connectSession();
                    reply = tmpMWSolaceClient.request(Encoding.UTF8.GetBytes(GetJsonStrValue(bizRuleID, indata, outdata, INDATASET, out transactionID)), GetTopicInfo(topic_Name, procQueueType));
                    tmpMWSolaceClient.disConnectSession();
                    tmpMWSolaceClient.closeSession();
                    tmpMWSolaceClient = null;
                }
                TimeSpan execTimeSpan = DateTime.Now - RCV_DATE;
                replyMessageStr = reply != null ? Encoding.UTF8.GetString(reply) : null;
                result = GetDataSetFromReplyMsg(replyMessageStr, outdata, bizRuleID);//2022.06.08 Oracle error 발생시에도 Biz명을 남기도록 수정

                if (!noLogInputData)
                {
                    if (!nologOutputData)
                    {
                        //Logger.GetInstance().WriteLogLine(LogLevel.DATA, string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID), INDATASET, result);
                        Logger.Instance.WriteLine("Solace Data", "Event:" + string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3} / INDATASET : {4} / result : {5}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID, INDATASET, result), LogCategory.BIZ);
                    }
                    else
                    {
                        //Logger.GetInstance().WriteLogLine(LogLevel.DATA, string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID), INDATASET);
                        Logger.Instance.WriteLine("Solace Data", "Event:" + string.Format(" {0}():[ {1} ] / exec.Time : {2} / TXN_ID : {3} / INDATASET : {4}", "ExecuteService", bizRuleID, execTimeSpan.ToString(), transactionID, INDATASET), LogCategory.BIZ);
                    }
                }
            }
            catch (Exception bizException)
            {
                //Logger.Instance.WriteException(LogLevel.EXCEPTION, "ExecuteService():[" + bizRuleID + "] Exception", bizException);
                Logger.Instance.WriteLine("Biz Exception", "ExecuteService():[" + bizRuleID + "]:" + bizException, LogCategory.BIZ);
                ex = bizException;
                throw ex;
            }
            finally
            {
                if (result != null)
                    result.AcceptChanges();
            }

            return result;
        }

        private static string GetTopicInfo(string topic_Name, ProcQueueType procQueueType = ProcQueueType.SQL)
        {
            JObject _TopicStrBiz = new JObject();
            _TopicStrBiz.Add("TOPIC_NAME", topic_Name);
            _TopicStrBiz.Add("REPLY_TO_QUEUE", "REPLY/" + (procQueueType == ProcQueueType.BIZ ? _bizQueue : _sqlQueue));
            _TopicStrBiz.Add("DELIVERY_MODE", MessageDeliveryMode.Persistent.ToString());
            _TopicStrBiz.Add("TIMEOUT_MS", _DefaultTimeOut * 1000);

            return _TopicStrBiz.ToString();
        }

        private static string GetJsonStrValue(string bizRuleID, string indata, string outdata, DataSet indataSet, out string transactionID)
        {
            JObject result = new JObject();
            transactionID = getTransactionID();

            result.Add("actID", bizRuleID);
            result.Add("refDS", DataSetToJson(indataSet, indata));
            result.Add("inDTName", (string.IsNullOrEmpty(indata) ? "" : indata).ToString());
            result.Add("outDTName", (string.IsNullOrEmpty(outdata) ? "" : outdata).ToString());
            result.Add("TXN_ID", transactionID);
            
            return result.ToString();
        }

        private static string GetStringFromMessage(IMessage replyMsg)
        {
            string strResult = null;

            IStreamContainer respStream = (IStreamContainer)MoMHelper.getContainer(replyMsg);
            if (respStream != null)
            {
                ISDTField status = respStream.GetNext();
                if (status.Type == SDTFieldType.BYTEARRAY)
                {
                    strResult = Encoding.UTF8.GetString((byte[])status.Value);
                }
            }

            return strResult;
        }

        private static DataTable GetDataTableFromReplyMsg(string strReplyMsg, string outdata)
        {
            DataTable dtResult = null;
            if (outdata == null || outdata.Equals(string.Empty))
            {
                if (JObject.Parse(strReplyMsg).ContainsKey("ERR_CD"))
                {
                    string ErrCode = JObject.Parse(strReplyMsg)["ERR_CD"].ToString();
                    string ErrMsg = JObject.Parse(strReplyMsg)["ERR_MSG"].ToString();

                    Exception exception = new Exception(ErrMsg);

                    if (!string.IsNullOrEmpty(ErrCode))
                    {
                        exception.Data.Add("CODE", ErrCode);
                    }

                    throw exception;
                }

                return null;
            }

            // DEFAULT OUT MSG 추가 정의 필요
            // "{\"__ERROR_REPLY__\":[{\"ERR_CD\":\"5\",\"ERR_MSG\":\"Test error message\"}]}"
            // ERROR STEP 이외의 존재하지 않는 BR 호출이나 매개변수 오류로 인한 B/A 솔루션 에러 발생 케이스도 있을것이라
            // ERR_CD 가 있을 경우 MESSAGEDIC 에서 값을 가져와야하고, 없을 경우 ERR_MSG 만 표시 필요
            if (JObject.Parse(strReplyMsg).ContainsKey("OUT_REPLY"))
            {
                var dtTemp = JObject.Parse(strReplyMsg)["OUT_REPLY"];
                if (dtTemp != null)
                {
                    dtResult = JsonToDataTable(dtTemp.ToString(), "OUT_REPLY");
                    string sExceptionMessage = dtResult.Rows[0]["Reply"].ToString();

                    throw new Exception(sExceptionMessage);
                }
            }
            else if (JObject.Parse(strReplyMsg).ContainsKey(outdata))
            {
                var dtTemp = JObject.Parse(strReplyMsg)[outdata];
                if (dtTemp != null)
                {
                    dtResult = JsonToDataTable(dtTemp.ToString(), outdata);
                }
            }
            else
            {
                if (JObject.Parse(strReplyMsg).ContainsKey("ERR_CD"))
                {
                    string ErrCode = JObject.Parse(strReplyMsg)["ERR_CD"].ToString();
                    string ErrMsg = JObject.Parse(strReplyMsg)["ERR_MSG"].ToString();

                    Exception exception = new Exception(ErrMsg);

                    if (!string.IsNullOrEmpty(ErrCode))
                    {
                        exception.Data.Add("CODE", ErrCode);
                    }

                    throw exception;
                }
            }

            return dtResult;
        }

        private static DataSet GetDataSetFromReplyMsg(string strReplyMsg, string outdata, string bizActorSvcID = null) ////2022.06.08 Oracle error 발생시에도 Biz명을 남기도록 수정
        {
            DataSet dsResult = new DataSet();

            if (outdata == null || outdata.Equals(string.Empty))
            {
                if (JObject.Parse(strReplyMsg).ContainsKey("ERR_CD"))
                {
                    string ErrCode = JObject.Parse(strReplyMsg)["ERR_CD"].ToString();
                    string ErrMsg = JObject.Parse(strReplyMsg)["ERR_MSG"].ToString();
                    string[] ErrParam = null;
                    string ErrLoc = string.Empty;
                    string ErrBiz = string.Empty;

                    if (JObject.Parse(strReplyMsg).ContainsKey("ERR_PARA"))
                    {
                        String items = JObject.Parse(strReplyMsg)["ERR_PARA"].ToString();
                        ErrParam = items.Split(new String[] { "||" }, StringSplitOptions.None);
                    }

                    if (JObject.Parse(strReplyMsg).ContainsKey("ERR_LOC"))
                    {
                        ErrLoc = JObject.Parse(strReplyMsg)["ERR_LOC"].ToString();
                    }

                    if (JObject.Parse(strReplyMsg).ContainsKey("ERR_BIZ"))
                    {
                        ErrBiz = JObject.Parse(strReplyMsg)["ERR_BIZ"].ToString();
                    }
                    //2022.06.08 Oracle error 발생시에도 Biz명을 남기도록 수정
                    if (ErrBiz == string.Empty)
                    {
                        ErrBiz = (bizActorSvcID ?? string.Empty).ToString();
                    }

                    Exception exception = new Exception(ErrMsg);

                    if (!string.IsNullOrEmpty(ErrCode))
                    {
                        exception.Data.Add("CODE", ErrCode);
                        exception.Data.Add("PARA", (ErrParam ?? new string[0]));
                        exception.Data.Add("LOC", ErrLoc);
                        exception.Data.Add("BIZ", ErrBiz);
                    }

                    throw exception;
                }

                return null;
            }

            string[] sTableNames = outdata.Split(',');

            // DEFAULT OUT MSG 추가 정의 필요
            // "{\"__ERROR_REPLY__\":[{\"ERR_CD\":\"5\",\"ERR_MSG\":\"Test error message\"}]}"
            // ERROR STEP 이외의 존재하지 않는 BR 호출이나 매개변수 오류로 인한 B/A 솔루션 에러 발생 케이스도 있을것이라
            // ERR_CD 가 있을 경우 MESSAGEDIC 에서 값을 가져와야하고, 없을 경우 ERR_MSG 만 표시 필요
            if (JObject.Parse(strReplyMsg).ContainsKey("OUT_REPLY"))
            {
                var dtTemp = JObject.Parse(strReplyMsg)["OUT_REPLY"];
                if (dtTemp != null)
                {
                    DataTable dtResult = JsonToDataTable(dtTemp.ToString(), "OUT_REPLY");
                    string sExceptionMessage = dtResult.Rows[0]["Reply"].ToString();

                    throw new Exception(sExceptionMessage);
                }
            }
            else if (JObject.Parse(strReplyMsg).ContainsKey("ERR_CD") & JObject.Parse(strReplyMsg).ContainsKey("ERR_MSG"))
            {
                string ErrCode = JObject.Parse(strReplyMsg)["ERR_CD"].ToString();
                string ErrMsg = JObject.Parse(strReplyMsg)["ERR_MSG"].ToString();
                string[] ErrParam = null;
                string ErrLoc = string.Empty;
                string ErrBiz = string.Empty;

                if (JObject.Parse(strReplyMsg).ContainsKey("ERR_PARA"))
                {
                    String items = JObject.Parse(strReplyMsg)["ERR_PARA"].ToString();
                    ErrParam = items.Split(new String[] { "||" }, StringSplitOptions.None);
                }

                if (JObject.Parse(strReplyMsg).ContainsKey("ERR_LOC"))
                {
                    ErrLoc = JObject.Parse(strReplyMsg)["ERR_LOC"].ToString();
                }

                if (JObject.Parse(strReplyMsg).ContainsKey("ERR_BIZ"))
                {
                    ErrBiz = JObject.Parse(strReplyMsg)["ERR_BIZ"].ToString();
                }

                Exception exception = new Exception(ErrMsg);

                if (!string.IsNullOrEmpty(ErrCode))
                {
                    exception.Data.Add("CODE", ErrCode);
                    exception.Data.Add("PARA", (ErrParam ?? new string[0]));
                    exception.Data.Add("LOC", ErrLoc);
                    exception.Data.Add("BIZ", ErrBiz);
                }

                throw exception;
            }
            else
            {
                JObject parseResult = JObject.Parse(strReplyMsg);

                foreach (string strTableName in sTableNames)
                {
                    if (parseResult.ContainsKey(strTableName))
                    {
                        var dtTemp = parseResult[strTableName];
                        if (dtTemp != null)
                        {
                            // 조회 결과 컬럼의 Data Type 체크
                            if (parseResult.ContainsKey("schema"))
                            {
                                CheckDataTypeColumn(dtTemp as JArray, parseResult["schema"][strTableName] as JArray);
                            }
                            //

                            DataTable dtResult = (JsonToDataTable(dtTemp.ToString(), outdata) ?? new DataTable(outdata));
                            dtResult.TableName = strTableName;
                            dsResult.Tables.Add(dtResult);

                            if (dtResult.Rows.Count == 0 && parseResult.ContainsKey("schema"))
                            {
                                foreach (var colItem in parseResult["schema"][strTableName])
                                {
                                    DataColumn dtCol;
                                    try
                                    {
                                        dtCol = new DataColumn(colItem["name"].ToString(), Type.GetType("System." + colItem["type"].ToString()));

                                    }
                                    catch
                                    {
                                        dtCol = new DataColumn(colItem["name"].ToString(), Type.GetType("System.String"));
                                    }

                                    dtResult.Columns.Add(dtCol);
                                }
                            }
                            else if (dtResult.Rows.Count > 0 && parseResult.ContainsKey("schema"))
                            {
                                if (((JObject)parseResult["schema"]).ContainsKey(strTableName))
                                {
                                    int idx = 0;
                                    foreach (var colItem in parseResult["schema"][strTableName])
                                    {
                                        if (dtResult.Columns.Contains(colItem["name"].ToString())
)                                        {
                                            dtResult.Columns[colItem["name"].ToString()].SetOrdinal(idx);
                                            idx++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return dsResult;
        }

        private static DataTable JsonToDataTable(string strJson, string outdata)
        {
            DataTable dtResult = null;

            if (strJson.Equals(string.Empty)) return null;

            DataTable dtConvert = JsonConvert.DeserializeObject<DataTable>(strJson, new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffzzz"
            });
            if (dtConvert != null && dtConvert.Rows.Count > 0)
            {
                dtConvert.TableName = outdata;

                dtResult = dtConvert.Copy();
            }

            return dtResult;
        }

        private static DataSet JsonToDataSet(string strJson, string outdata)
        {
            DataSet dsResult = null;

            DataSet dsConvert = JsonConvert.DeserializeObject<DataSet>(strJson, new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffzzz"
            });

            string[] tbNameList = outdata.Split(',');

            if (dsConvert.Tables.Count == tbNameList.Length)
            {
                for (int inx = 0; inx < tbNameList.Length; inx++)
                {
                    dsConvert.Tables[inx].TableName = tbNameList[inx];
                }

                dsResult = dsConvert.Copy();
            }

            return dsResult;
        }

        private static void CheckDataTypeColumn(JArray jTable, JArray jSchema)
        {
            if (jTable.Any())
            {
                // Decimal 칼럼 추출 : Decimal 컬럼으로 지정하였으나 첫번째 Row 의 값이 정수 인 경우 Integer 인식 함. 
                // 첫번째 Row 이후 소수점 이하 인식 오류로 인하여 강제적으로 Data Type 변경 함. 
                System.Collections.Generic.List<string> lstDecimalColumns = jSchema.AsEnumerable().Where(jobj => jobj["type"].ToString() == "Decimal").Select(jobj => jobj["name"].ToString()).ToList();

                if (lstDecimalColumns.Any())
                {
                    // Decimal 칼럼타입인데 값은 Integer 타입인 경우 형변환을 해준다.
                    foreach (var prop in ((JObject)jTable[0]).Properties().Where(p => lstDecimalColumns.Contains(p.Name)))
                    {
                        if (prop.Value.Type == JTokenType.Integer)
                        {
                            try
                            {
                                prop.Value = Convert.ToDecimal(prop.Value); //JTokenType Float
                            }
                            catch { }
                        }
                    }
                }

                
                // Boolean 칼럼 추출
                System.Collections.Generic.List<string> lstBooleanColumns = jSchema.AsEnumerable().Where(jobj => jobj["type"].ToString() == "Boolean").Select(jobj => jobj["name"].ToString()).ToList();
                if (lstBooleanColumns.Any())
                {
                    foreach (var prop in ((JObject)jTable[0]).Properties().Where(p => lstBooleanColumns.Contains(p.Name)))
                    {
                        try
                        {
                            if (prop.Value == null || string.IsNullOrWhiteSpace(prop.Value.ToString()) || (prop.Value.Type == JTokenType.Boolean))
                            {
                                prop.Value = true;
                            }
                        }
                        catch { }
                    }
                }
                
            }
        }

        private static string DataTableToJson(DataTable dt, string indata)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt.Copy());

            return DataSetToJson(ds, indata);
        }

        private static string getTransactionID()
        {
            try
            {
                long nano = 10000L * Stopwatch.GetTimestamp();
                nano /= TimeSpan.TicksPerMillisecond;
                nano *= 100L;

                return DateTime.Now.ToString("yyyyMMddHHmmssfff") + nano.ToString().Substring(9);
            }
            catch
            {
                return "";
            }
        }

        private static string DataSetToJson(DataSet ds, string indata)
        {
            if (indata != null)
            {
                string[] strIndataList = indata.Split(',');

                if (ds.Tables.Count == 0) return string.Empty; // 2024.110.02 김영국 - DataSet이 Null인 경우 로직 처리.

                int inx = 0;
                foreach (string tableName in strIndataList)
                {
                    ds.Tables[inx].TableName = tableName;
                    inx++;
                }
            }

            if (ds != null)
            {
                if (!ds.Tables.Contains("__BIZACTOR_INFO__"))
                {
                    DataTable dtBizInfo = new DataTable("__BIZACTOR_INFO__");
                    dtBizInfo.Columns.Add("__ERR_MSG_LANG__");
                    dtBizInfo.Rows.Add(GetLangCode());
                    ds.Tables.Add(dtBizInfo);
                }

                if (!ds.Tables.Contains("__TRACE_INFO__"))
                {
                    DataTable dtTrace = new DataTable("__TRACE_INFO__");
                    dtTrace.Columns.Add("CLIENT_ID");
                    dtTrace.Columns.Add("CLIENT_IP");
                    //2022.03.31 이승헌 : Get_LocaIP()로 변경
                    //dtTrace.Rows.Add(LogInInfo.UserID, IPAddress.Parse(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString()));
                    //dtTrace.Rows.Add(LogInInfo.UserID, IPAddress.Parse(CommonFnc.Get_LocalIP()));
                    dtTrace.Columns.Add("CLIENT_TIME");
                    dtTrace.Columns.Add("CLIENT_FORM");
                    dtTrace.Rows.Add(LoginInfo.USERID, IPAddress.Parse(CommonFnc.Get_LocalIP()), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), getFullFormName());
                    ds.Tables.Add(dtTrace);
                }
            }

            return JsonConvert.SerializeObject(ds, new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffzzz",
                Formatting = Formatting.Indented
            });
        }

        private static string getFullFormName()
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(5).GetMethod().DeclaringType;

            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;
            
            return declaringType.FullName;

        }
    }
}
