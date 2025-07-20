using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ServerAgent;
using System.Collections.Generic;

namespace LGC.GMES.MES.Common
{
    public class ClientProxy
    {
        private static string clientId = Guid.NewGuid().ToString("N");
        private RemoteCall client = null;
        public static TimeSpan APServerTimeZoneDiff = TimeZoneInfo.Local.BaseUtcOffset;

        public ClientProxy()
        {
            if (LGC.GMES.MES.Common.Common.APP_REMOTE_CALL_SYSTEM != "SOLACE")
                client = new RemoteCall(Common.BizActorIP, Common.BizActorProtocol, Common.BizActorPort, Common.BizActorServiceMode, Common.BizActorServiceIndex);
        }

        public ClientProxy(string bizActorIp, string bizActorProtocol, string bizActorPort, string bizActorServiceMode, string bizActorServiceIndex)
        {
            if (LGC.GMES.MES.Common.Common.APP_REMOTE_CALL_SYSTEM != "SOLACE")
                client = new RemoteCall(bizActorIp, bizActorProtocol, bizActorPort, bizActorServiceMode, bizActorServiceIndex);
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
                    if (!INDATASET.Tables.Contains("__TRACE_INFO__"))
                    {
                        DataTable dtTrace = new DataTable("__TRACE_INFO__");
                        dtTrace.Columns.Add("CLIENT_ID");
                        dtTrace.Columns.Add("CLIENT_IP");
                        dtTrace.Columns.Add("CLIENT_TIME");
                        dtTrace.Columns.Add("CLIENT_FORM");
                        dtTrace.Rows.Add(LoginInfo.USERID, System.Net.IPAddress.Parse(CommonFnc.Get_LocalIP()), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), declaringType.FullName);
                        INDATASET.Tables.Add(dtTrace);
                    }

                    if (!noLogInputData)
                        Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);

                    convertToAPServerDateTime(INDATASET);

                    DataSet OUTDATASET = null;
                    if (LGC.GMES.MES.Common.Common.APP_REMOTE_CALL_SYSTEM == "SOLACE")
                    {
                        OUTDATASET = ClientProxyMom.ExecuteServiceSync(bizRuleID, indata, outdata, INDATASET);
                    }
                    else
                    {
                        OUTDATASET = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                    }


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

                    if(!string.IsNullOrEmpty(outdata))
                    {
                        if (OUTDATASET.Tables.Contains(outdata))
                        {
                            result = OUTDATASET.Tables[outdata];
                            convertToClientDateTime(result);
                        }
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

        //public void ExecuteServiceSync(string bizRuleID, string indata, string outdata, DataTable indataTable, ExecuteServiceComplete delegater, string menuid = null, bool noLogInputData = false, bool nologOutputData = false)
        public DataTable ExecuteServiceSync(string bizRuleID, string indata, string outdata, DataTable indataTable, string menuid = null, bool noLogInputData = false, bool nologOutputData = false, bool RowSequenceNo = false)
        {
            StackTrace st = new StackTrace();

            Type declaringType = st.GetFrame(1).GetMethod().DeclaringType;

            while (declaringType.DeclaringType != null)
                declaringType = declaringType.DeclaringType;

            string formID = declaringType.Name;

            DataTable result = new DataTable();
            
            Exception ex = null;
            DataSet INDATASET = new DataSet();

            if (indataTable != null && !string.IsNullOrEmpty(indata))
            {
                DataTable INDATA = indataTable.Copy();
                INDATA.TableName = indata;
                INDATASET.Tables.Add(INDATA);
            }
            else // 2024.10.02 김영국 - DataSet이 Null인 경우 강제적으로 DataTable을 만들어 넣어준다.
            {
                INDATASET.Tables.Add(new DataTable(indata));
            }

            DateTime RCV_DATE = DateTime.Now;
            DateTime TRSM_DATE = DateTime.Now;

            try
            {
                if (!noLogInputData)
                    Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);

                convertToAPServerDateTime(INDATASET);
                DataSet OUTDATASET;

                if (LGC.GMES.MES.Common.Common.APP_REMOTE_CALL_SYSTEM == "SOLACE")
                {
                    OUTDATASET = ClientProxyMom.ExecuteServiceSync(bizRuleID, indata, outdata, INDATASET);
                }
                else
                {
                    OUTDATASET = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                }

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

                if(!string.IsNullOrEmpty(outdata))
                {
                    if (OUTDATASET.Tables.Contains(outdata))
                    {
                        result = OUTDATASET.Tables[outdata];
                        convertToClientDateTime(result);

                        /// 결과테이블에 순번을 넣는다 ///
                        if (RowSequenceNo)
                        {
                            result.Columns.Add("ROWSEQUENCENO", typeof(Int64));
                            result.Columns["ROWSEQUENCENO"].SetOrdinal(0);

                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                result.Rows[i]["ROWSEQUENCENO"] = i + 1;
                            }
                        }
                    }
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

        /// <summary>
        /// Dictionary 정보를 Table데이터에 입력 후 쿼리 수행
        /// 작업자 : YSH
        /// 일시 : 2022.03.10
        /// </summary>
        /// <param name="bizRuleID"></param>
        /// <param name="outdata"></param>
        /// <param name="param"></param>
        /// <param name="indata">default : RQSTDT</param>
        /// <param name="menuid"></param>
        /// <param name="noLogInputData"></param>
        /// <param name="nologOutputData"></param>
        /// <param name="RowSequenceNo"></param>
        /// <returns></returns>
        public DataTable ExecuteServiceSync(string bizRuleID, string outdata, Dictionary<string, string> param, string indata = "RQSTDT", string menuid = null, bool noLogInputData = false, bool nologOutputData = false, bool RowSequenceNo = false)
        {
            DataTable inTable = new DataTable { TableName = indata };

            DataRow dr = inTable.NewRow();
            foreach (KeyValuePair<string, string> kvp in param)
            {
                inTable.Columns.Add(kvp.Key);
                dr[kvp.Key] = kvp.Value;
            }

            inTable.Rows.Add(dr);

            return ExecuteServiceSync(bizRuleID, indata, outdata, inTable, menuid , noLogInputData, nologOutputData, RowSequenceNo);
        }

        private void executeBizLog(string formID, string bizRuleID, DataSet INDATASET, DateTime RCV_DATE, DateTime TRSM_DATE, bool hasError, Exception bizException, string menuid)
        {

            //임시
            if (bizRuleID == "BR_SYS_REG_BIZRULE_EXCEPTION") return; 

            if (hasError)
            {
                try
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("SRCTYPE", typeof(string));
                    IndataTable.Columns.Add("IFMODE", typeof(string));
                    IndataTable.Columns.Add("EQPTID", typeof(string));
                    IndataTable.Columns.Add("PROGRAMID", typeof(string));
                    IndataTable.Columns.Add("BIZRULEID", typeof(string));
                    IndataTable.Columns.Add("EXCEPTION_CODE", typeof(string));
                    IndataTable.Columns.Add("EXCEPTION_MESSAGE", typeof(string));
                    IndataTable.Columns.Add("EXCEPTION_TYPE", typeof(string));
                    IndataTable.Columns.Add("EXCEPTION_LOC", typeof(string));
                    IndataTable.Columns.Add("EXCEPTION_DATA", typeof(string));
                    IndataTable.Columns.Add("EXCEPTION_PARA", typeof(string));
                    IndataTable.Columns.Add("LOTID", typeof(string));
                    IndataTable.Columns.Add("DATASET", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["SRCTYPE"] = "UI";
                    Indata["IFMODE"] = "OFF";
                    Indata["EQPTID"] = LoginInfo.USERID;
                    Indata["PROGRAMID"] = formID;
                    Indata["BIZRULEID"] = bizRuleID;
                    Indata["EXCEPTION_MESSAGE"] = bizException.Message;

                    if (bizException.Data.Count > 0)
                    {
                        Indata["EXCEPTION_TYPE"] = bizException.Data["TYPE"].ToString();
                        Indata["EXCEPTION_LOC"] = bizException.Data["LOC"].ToString();

                        if (bizException.Data["TYPE"].ToString() == "USER")
                        {
                            Indata["EXCEPTION_CODE"] = bizException.Data["CODE"].ToString();
                            Indata["EXCEPTION_DATA"] = bizException.Data["DATA"].ToString();
                            Indata["EXCEPTION_PARA"] = bizException.Data["PARA"].ToString();
                        }
                    }

                    Indata["LOTID"] = "";
                    Indata["DATASET"] = INDATASET.GetXml();
                    IndataTable.Rows.Add(Indata);

                    new ClientProxy().ExecuteService("BR_SYS_REG_BIZRULE_EXCEPTION", "IN_EQP", null, IndataTable, (result, ex) => { });
                }
                catch { }
            }

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
                        INDATATABLE.TableName = indataNames[inx];

                    INDATASET.Tables.Add(INDATATABLE);
                }

                DateTime RCV_DATE = DateTime.Now;
                DateTime TRSM_DATE = DateTime.Now;

                try
                {
                    if (!INDATASET.Tables.Contains("__TRACE_INFO__"))
                    {
                        DataTable dtTrace = new DataTable("__TRACE_INFO__");
                        dtTrace.Columns.Add("CLIENT_ID");
                        dtTrace.Columns.Add("CLIENT_IP");
                        dtTrace.Columns.Add("CLIENT_TIME");
                        dtTrace.Columns.Add("CLIENT_FORM");
                        dtTrace.Rows.Add(LoginInfo.USERID, System.Net.IPAddress.Parse(CommonFnc.Get_LocalIP()), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), declaringType.FullName);
                        INDATASET.Tables.Add(dtTrace);
                    }

                    Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);
                    convertToAPServerDateTime(INDATASET);

                    if (LGC.GMES.MES.Common.Common.APP_REMOTE_CALL_SYSTEM == "SOLACE")
                    {
                        result = ClientProxyMom.ExecuteServiceSync(bizRuleID, indata, outdata, INDATASET);
                    }
                    else
                    {
                        result = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                    }
                    //result = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                    convertToClientDateTime(result);
                    TRSM_DATE = DateTime.Now;

                    if (!Common.BIZLOGLEVEL.Equals("-1"))
                        executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, false, null, menuid);

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
            }));

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

            // 2024.10.02 김영국 - INDATASET이 NULL인 경우는 강제적으로 DATATABLE을 넘겨주도록 한다.
            if(indataSet != null && indataSet.Tables.Count > 0) 
            {
                for (int inx = 0; inx < indataSet.Tables.Count; inx++)
                {
                    DataTable INDATATABLE = indataSet.Tables[inx].Copy();

                    if (indataNames.Length > inx)
                        INDATATABLE.TableName = indataNames[inx];

                    INDATASET.Tables.Add(INDATATABLE);
                }
            }
            else
            {
                INDATASET.Tables.Add(new DataTable(indataNames[0]));
            }

            #region 2024.10.02 김영국 - 기존 로직 주석 처리
            //for (int inx = 0; inx < indataSet.Tables.Count; inx++)
            //{
            //    DataTable INDATATABLE = indataSet.Tables[inx].Copy();

            //    if (indataNames.Length > inx)
            //        INDATATABLE.TableName = indataNames[inx];

            //    INDATASET.Tables.Add(INDATATABLE);
            //} 
            #endregion

            DateTime RCV_DATE = DateTime.Now;
            DateTime TRSM_DATE = DateTime.Now;

            try
            {
                Logger.Instance.WriteLine("[BIZRULE] Call BizRule", LogCategory.BIZ, bizRuleID, indata, outdata, INDATASET);
                convertToAPServerDateTime(INDATASET);

                if (LGC.GMES.MES.Common.Common.APP_REMOTE_CALL_SYSTEM == "SOLACE")
                {
                    result = ClientProxyMom.ExecuteServiceSync(bizRuleID, indata, outdata, INDATASET);
                }
                else
                {
                    result = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                }
                //result = client.ExecuteService(bizRuleID, INDATASET, indata, outdata);
                convertToClientDateTime(result);
                TRSM_DATE = DateTime.Now;

                if (!Common.BIZLOGLEVEL.Equals("-1"))
                    executeBizLog(formID, bizRuleID, INDATASET, RCV_DATE, TRSM_DATE, false, null, menuid);

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

    public delegate void ExecuteServiceComplete(DataTable result, Exception ex);
    public delegate void ExecuteServiceMultiComplete(DataSet result, Exception ex);
}