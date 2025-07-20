using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Windows;
using ServerAgent;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;

namespace LGC.GMES.MES.Common
{
    public class ClientProxy2007
    {
        private static string clientId = Guid.NewGuid().ToString("N");
        private RemoteCall2007 client = null;
        public static TimeSpan APServerTimeZoneDiff = TimeZoneInfo.Local.BaseUtcOffset;


        public ClientProxy2007(string sTarget)
        {//2007 biz actor 로 접속이 필요할 경우 추가해서 사용
            if (sTarget.Equals("AF1")) //자동차 1동 활성화
            {
                if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "SBC" && LGC.GMES.MES.Common.Common.APP_MODE == "RELEASE") // 운영 환경 FA
                {
                    client = new RemoteCall2007("192.168.247.105", "tcp", "7865", "", "0");
                }
                else if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "LOCAL" && LGC.GMES.MES.Common.Common.APP_MODE == "RELEASE") // 운영 환경 OA
                {
                    client = new RemoteCall2007("165.244.114.235", "tcp", "7865", "", "0");
                }
                else if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "SBC" && LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG") // 개발 환경 OA
                {
                    client = new RemoteCall2007("192.168.245.78", "tcp", "7866", "", "1"); // 개발
                }
                else if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "LOCAL" && LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG") // 개발 환경 OA
                {
                    client = new RemoteCall2007("165.244.114.45", "tcp", "7866", "", "1"); // 개발
                }
                else
                {
                    client = new RemoteCall2007("165.244.114.45", "tcp", "7866", "", "1"); // 개발
                }
            }
            else if (sTarget.Equals("AF2"))
            {
                if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "SBC" && LGC.GMES.MES.Common.Common.APP_MODE == "RELEASE") // 운영 환경 FA
                {
                    client = new RemoteCall2007("192.168.247.182", "tcp", "7865", "", "0");
                }
                else if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "LOCAL" && LGC.GMES.MES.Common.Common.APP_MODE == "RELEASE") // 운영 환경 OA
                {
                    client = new RemoteCall2007("165.244.114.220", "tcp", "7865", "", "0");
                }
                else if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "SBC" && LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG") // 개발 환경 OA
                {
                    client = new RemoteCall2007("192.168.245.78", "tcp", "7867", "", "2"); // 개발
                }
                else if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "LOCAL" && LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG") // 개발 환경 OA
                {
                    client = new RemoteCall2007("165.244.114.45", "tcp", "7867", "", "2"); // 개발
                }
                else
                {
                    client = new RemoteCall2007("165.244.114.45", "tcp", "7867", "", "2"); // 개발
                }
            }
            else
            {
                client = null;
            }
        }

        private static void convertToAPServerDateTime(DataTable table)
        {
            if (table == null)
                return;

            foreach (DataColumn column in table.Columns)
            {
                if (column.DataType.Equals(typeof(DateTime)))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (row[column] != null && !DBNull.Value.Equals(row[column]))
                        {
                            row[column] = ((DateTime)row[column]).AddMinutes(-(APServerTimeZoneDiff.Subtract(TimeZoneInfo.Local.BaseUtcOffset).TotalMinutes));
                        }
                    }
                }
            }
        }

        private static void convertToAPServerDateTime(DataSet set)
        {
            if (set == null)
                return;

            foreach (DataTable table in set.Tables)
            {
                convertToAPServerDateTime(table);
            }
        }

        private static void convertToClientDateTime(DataTable table)
        {
            if (table == null)
                return;

            foreach (DataColumn column in table.Columns)
            {
                if (column.DataType.Equals(typeof(DateTime)))
                {
                    foreach (DataRow row in table.Rows)
                    {
                        if (row[column] != null && !DBNull.Value.Equals(row[column]))
                        {
                            row[column] = ((DateTime)row[column]).AddMinutes(APServerTimeZoneDiff.Subtract(TimeZoneInfo.Local.BaseUtcOffset).TotalMinutes);
                        }
                    }
                }
            }
        }

        private static void convertToClientDateTime(DataSet set)
        {
            if (set == null)
                return;

            foreach (DataTable table in set.Tables)
            {
                convertToClientDateTime(table);
            }
        }

        public void ExecuteService(string bizRuleID, string indata, string outdata, DataTable indataTable, ExecuteServiceComplete delegater, string menuid = null, bool noLogInputData = false, bool nologOutputData = false)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;
            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;
            string formID = declaringType.Name;

            Thread worker = new Thread(new ThreadStart(() =>
            {
                DataTable result = null;
                Exception ex = null;

                DataSet INDATASET = new DataSet();
                if (indataTable != null && !string.IsNullOrEmpty(indata))
                {
                    DataTable INDATA = indataTable.Copy();
                    INDATA.TableName = indata;
                    INDATASET.Tables.Add(INDATA);
                }

                DateTime RCV_DATE = DateTime.Now;
                DateTime TRSM_DATE = DateTime.Now;
                try
                {
                    if (!noLogInputData)
                        Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);

                    convertToAPServerDateTime(INDATASET);
                    DataSet OUTDATASET = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                    TRSM_DATE = DateTime.Now;

                    if (!Common.BIZLOGLEVEL.Equals("-1"))
                    {
                        if (noLogInputData)
                        {
                            DataSet emptySet = new DataSet();
                            DataTable emptyTable = emptySet.Tables.Add("INDATA");
                            emptyTable.Columns.Add("MSG", typeof(string));
                            DataRow emptyRow = emptyTable.NewRow();
                            emptyRow["MSG"] = "INDATA set is hidden";
                            emptyTable.Rows.Add(emptyRow);
                            executeBizLog(formID, bizRuleID, emptySet, RCV_DATE, TRSM_DATE, false, null, menuid);
                        }
                        else
                        {
                            executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, false, null, menuid);
                        }
                    }

                    if (!nologOutputData)
                        Logger.Instance.WriteLine("[BIZRULE] Return BizRule", LogCategory.BIZ, bizRuleID, OUTDATASET);

                    if (OUTDATASET.Tables.Contains(outdata))
                    {
                        result = OUTDATASET.Tables[outdata];
                        convertToClientDateTime(result);
                    }
                }
                catch (Exception bizException)
                {
                    Logger.Instance.WriteLine("[BIZRULE] BizRule Exception", bizException, LogCategory.BIZ);

                    executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, true, bizException, menuid);

                    ex = bizException;
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        if (result != null)
                            result.AcceptChanges();
                        delegater(result, ex);
                    }));
                }
            }
            ));
            worker.IsBackground = true;
            worker.Start();
        }

        //public void ExecuteServiceSync(string bizRuleID, string indata, string outdata, DataTable indataTable, ExecuteServiceComplete delegater, string menuid = null, bool noLogInputData = false, bool nologOutputData = false)
        public DataTable ExecuteServiceSync(string bizRuleID, string indata, string outdata, DataTable indataTable, string menuid = null, bool noLogInputData = false, bool nologOutputData = false)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;
            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;
            string formID = declaringType.Name;


            DataTable result = null;
            Exception ex = null;

            DataSet INDATASET = new DataSet();
            if (indataTable != null && !string.IsNullOrEmpty(indata))
            {
                DataTable INDATA = indataTable.Copy();
                INDATA.TableName = indata;
                INDATASET.Tables.Add(INDATA);
            }

            DateTime RCV_DATE = DateTime.Now;
            DateTime TRSM_DATE = DateTime.Now;
            try
            {
                if (!noLogInputData)
                    Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);

                convertToAPServerDateTime(INDATASET);
                DataSet OUTDATASET = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                TRSM_DATE = DateTime.Now;

                if (!Common.BIZLOGLEVEL.Equals("-1"))
                {
                    if (noLogInputData)
                    {
                        DataSet emptySet = new DataSet();
                        DataTable emptyTable = emptySet.Tables.Add("INDATA");
                        emptyTable.Columns.Add("MSG", typeof(string));
                        DataRow emptyRow = emptyTable.NewRow();
                        emptyRow["MSG"] = "INDATA set is hidden";
                        emptyTable.Rows.Add(emptyRow);
                        executeBizLog(formID, bizRuleID, emptySet, RCV_DATE, TRSM_DATE, false, null, menuid);
                    }
                    else
                    {
                        executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, false, null, menuid);
                    }
                }

                if (!nologOutputData)
                    Logger.Instance.WriteLine("[BIZRULE] Return BizRule", LogCategory.BIZ, bizRuleID, OUTDATASET);

                if (OUTDATASET.Tables.Contains(outdata))
                {
                    result = OUTDATASET.Tables[outdata];
                    convertToClientDateTime(result);
                }
            }
            catch (Exception bizException)
            {
                Logger.Instance.WriteLine("[BIZRULE] BizRule Exception", bizException, LogCategory.BIZ);

                executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, true, bizException, menuid);

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

        private void executeBizLog(string formID, string bizRuleID, DataSet INDATASET, DateTime RCV_DATE, DateTime TRSM_DATE, bool hasError, Exception bizException, string menuid)
        {
            //Thread worker = new Thread(new ThreadStart(() =>
            //{
            //    try
            //    {
            //        DataSet bizLogSet = new DataSet();
            //        DataTable bizLogTable = bizLogSet.Tables.Add("INDATA");
            //        bizLogTable.Columns.Add("CLNT_ID", typeof(string));
            //        bizLogTable.Columns.Add("TXN_ID", typeof(string));
            //        bizLogTable.Columns.Add("CALL_FORM_ID", typeof(string));
            //        bizLogTable.Columns.Add("BIZLOGLEVEL", typeof(string));
            //        bizLogTable.Columns.Add("BIZ_SVC_ID", typeof(string));
            //        bizLogTable.Columns.Add("RCV_DATE", typeof(DateTime));
            //        bizLogTable.Columns.Add("TRSM_DATE", typeof(DateTime));
            //        bizLogTable.Columns.Add("MSG_CNTT", typeof(string));
            //        bizLogTable.Columns.Add("ERROR_FLAG", typeof(string));
            //        bizLogTable.Columns.Add("ERROR_MSG_CNTT", typeof(string));
            //        bizLogTable.Columns.Add("INSUSER", typeof(string));
            //        bizLogTable.Columns.Add("MENUID", typeof(string));

            //        DataRow bizLogData = bizLogTable.NewRow();
            //        bizLogData["CLNT_ID"] = clientId;
            //        bizLogData["TXN_ID"] = Guid.NewGuid().ToString("N");
            //        bizLogData["CALL_FORM_ID"] = formID;
            //        bizLogData["BIZLOGLEVEL"] = Common.BIZLOGLEVEL;
            //        bizLogData["BIZ_SVC_ID"] = bizRuleID;
            //        bizLogData["RCV_DATE"] = RCV_DATE;
            //        bizLogData["TRSM_DATE"] = TRSM_DATE;
            //        TextWriter tw = new StringWriter();
            //        INDATASET.WriteXml(tw, XmlWriteMode.IgnoreSchema);
            //        bizLogData["MSG_CNTT"] = tw.ToString();
            //        bizLogData["ERROR_FLAG"] = hasError ? "Y" : "N";
            //        bizLogData["ERROR_MSG_CNTT"] = hasError ? bizException.Message : null;
            //        bizLogData["INSUSER"] = LoginInfo.USERID;
            //        bizLogData["MENUID"] = menuid;
            //        bizLogTable.Rows.Add(bizLogData);

            //        RemoteCall logClient = new RemoteCall(Common.BizActorIP, Common.BizActorProtocol, Common.BizActorPort, Common.BizActorServiceMode, Common.BizActorServiceIndex);
            //        logClient.ExecuteService("BR_COR_REG_SFU_BIZRULE_TXN_HIST_2", bizLogSet, "INDATA", null);
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}));
            //worker.IsBackground = true;
            //worker.Start();
        }

        public void ExecuteService_Multi(string bizRuleID, string indata, string outdata, ExecuteServiceMultiComplete delegater, DataSet indataSet, string menuid = null)
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

                DataSet INDATASET = new DataSet();
                string[] indataNames = new string[0];
                if (indata != null)
                    indataNames = indata.Split(',');
                for (int inx = 0; inx < indataSet.Tables.Count; inx++)
                {
                    DataTable INDATATABLE = indataSet.Tables[inx].Copy();
                    if (indataNames.Length > inx)
                    {
                        INDATATABLE.TableName = indataNames[inx];
                    }
                    INDATASET.Tables.Add(INDATATABLE);
                }

                DateTime RCV_DATE = DateTime.Now;
                DateTime TRSM_DATE = DateTime.Now;

                try
                {
                    Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);
                    convertToAPServerDateTime(INDATASET);
                    result = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                    convertToClientDateTime(result);
                    TRSM_DATE = DateTime.Now;

                    if (!Common.BIZLOGLEVEL.Equals("-1"))
                    {
                        executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, false, null, menuid);
                    }

                    Logger.Instance.WriteLine("[BIZRULE] Return BizRule", LogCategory.BIZ, bizRuleID, result);
                }
                catch (Exception bizException)
                {
                    Logger.Instance.WriteLine("[BIZRULE] BizRule Exception", LogCategory.BIZ, bizException);

                    executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, true, bizException, menuid);

                    ex = bizException;
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        if (result != null)
                            result.AcceptChanges();
                        delegater(result, ex);
                    }));
                }
            }
            ));
            worker.IsBackground = true;
            worker.Start();
        }

        public DataSet ExecuteServiceSync_Multi(string bizRuleID, string indata, string outdata, DataSet indataSet, string menuid = null)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;
            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;
            string formID = declaringType.Name;


            DataSet result = null;
            Exception ex = null;

            DataSet INDATASET = new DataSet();
            string[] indataNames = new string[0];
            if (indata != null)
                indataNames = indata.Split(',');
            for (int inx = 0; inx < indataSet.Tables.Count; inx++)
            {
                DataTable INDATATABLE = indataSet.Tables[inx].Copy();
                if (indataNames.Length > inx)
                {
                    INDATATABLE.TableName = indataNames[inx];
                }
                INDATASET.Tables.Add(INDATATABLE);
            }

            DateTime RCV_DATE = DateTime.Now;
            DateTime TRSM_DATE = DateTime.Now;

            try
            {
                Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);
                convertToAPServerDateTime(INDATASET);
                result = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                convertToClientDateTime(result);
                TRSM_DATE = DateTime.Now;

                if (!Common.BIZLOGLEVEL.Equals("-1"))
                {
                    executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, false, null, menuid);
                }

                Logger.Instance.WriteLine("[BIZRULE] Return BizRule", LogCategory.BIZ, bizRuleID, result);
            }
            catch (Exception bizException)
            {
                Logger.Instance.WriteLine("[BIZRULE] BizRule Exception", LogCategory.BIZ, bizException);

                executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, true, bizException, menuid);

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
    }


}
