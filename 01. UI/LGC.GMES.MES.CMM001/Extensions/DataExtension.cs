/*************************************************************************************
 Created Date : 2021.01.26 
      Creator : 조영대
   Decription : Controls Extension : ControlsExtension.cs
--------------------------------------------------------------------------------------
 [Change History]
  2021.11.02  조영대 : Initial Created
**************************************************************************************/

using System;
using System.Data;
using System.Text;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.ControlsLibrary
{
    public static class DataTableExtension
    {
        public static string GetDeclare(this DataTable dt)
        {
            if (dt == null) return string.Empty;
            if (dt.Rows.Count == 0) return string.Empty;

            string returnString = string.Empty;

            StringBuilder valueString = new StringBuilder();
            StringBuilder nullString = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                valueString.Clear();
                nullString.Clear();
                if (!dt.Columns.Contains("LANGID"))
                {
                    valueString.AppendLine("DECLARE @LANGID NVARCHAR(50) = '" + LoginInfo.LANGID + "'");
                }

                foreach (DataColumn col in dt.Columns)
                {
                    if (System.DBNull.Value.Equals(dr[col.ColumnName]))
                    {
                        nullString.AppendLine("DECLARE @" + col.ColumnName + " NVARCHAR(50) = NULL;");
                    }
                    else
                    {
                        switch (col.DataType.Name)
                        {
                            case "DateTime":
                                DateTime date = (DateTime)dr[col.ColumnName];
                                valueString.AppendLine("DECLARE @" + col.ColumnName + " DATETIME = '" + date.ToString("yyyy-MM-dd HH:mm:ss") + "';");
                                break;
                            default:
                                if (dr[col.ColumnName] != null && dr[col.ColumnName].ToString().Length > 50)
                                {
                                    int length = dr[col.ColumnName].ToString().Length + 5;
                                    valueString.AppendLine("DECLARE @" + col.ColumnName + " NVARCHAR(" + length.ToString() + ") = '" + dr[col.ColumnName]?.ToString() + "';");
                                }
                                else
                                {
                                    valueString.AppendLine("DECLARE @" + col.ColumnName + " NVARCHAR(50) = '" + dr[col.ColumnName]?.ToString() + "';");
                                }
                                break;
                        }
                    }                    
                }
                returnString += valueString.ToString() + nullString.ToString() + "\r\n";
            }

            return returnString;
        }

        public static string GetXaml(this DataTable dt)
        {
            if (dt.TableName.Equals(string.Empty)) dt.TableName = "INDATA";

            DataSet ds = new DataSet();
            ds.Tables.Add(dt.Copy());
            return ds.GetXml();
        }

        public static bool AddDataRow(this DataTable dt, DataRow dataRow)
        {
            if (dt == null) return false;
            if (dt.Columns.Count == 0) return false;

            DataRow newRow = null;

            foreach (DataColumn col in dt.Columns)
            {
                if (!dataRow.Table.Columns.Contains(col.ColumnName)) continue;

                if (newRow == null) newRow = dt.NewRow();

                newRow[col.ColumnName] = dataRow[col.ColumnName];
            }

            if (newRow != null)
            {
                dt.Rows.Add(newRow);
                return true;
            }

            return false;
        }
    }

    public static class DataSetExtension
    {
        public static string GetDeclare(this DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0) return string.Empty;

            string returnString = string.Empty;

            foreach (DataTable dt in ds.Tables)
            {
                if (dt.Rows.Count == 0) continue;

                returnString += "----- " + dt.TableName + " -----" + "\r\n";
                returnString += dt.GetDeclare();
            }
            return returnString;
        }
    }

    public static class ExecuteServiceExtension
    {
        public static DataTable ExecuteService(this ClientProxy cp, string query, DataTable indataTable)
        {
            try
            {
                string runQuery = query.ToUpper();

                DataRow drRqst = indataTable.Rows[0];
                foreach (DataColumn col in indataTable.Columns)
                {
                    if (!drRqst[col].IsNullOrEmpty())
                    {
                        runQuery = runQuery.Replace("@" + col.ColumnName.ToUpper(), "'" + drRqst[col].Nvc().ToUpper() + "'");
                    }
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SQL_QUERY", typeof(string));
                DataRow drNew = dtRqst.NewRow();
                drNew["SQL_QUERY"] = runQuery;
                dtRqst.Rows.Add(drNew);

                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QUERY_RUN_DRB", "RQSTDT", "RSLTDT", dtRqst);

                return dtReturn;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return null;
        }
    }
}