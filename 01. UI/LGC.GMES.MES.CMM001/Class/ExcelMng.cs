/*************************************************************************************
 Created Date : 2016.09.29
      Creator : hsjeong
   Description : Excel 관련 class
                Excel 을 DataTable로 하는 변환기능
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.29 / hsjeong : Initial Created.
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGC.GMES.MES.CMM001.Class
{
    public class ExcelMng
    {
        #region Member Variable

        private string sFileName = null;
        private OleDbConnection oleConn = null;
        private OleDbCommand oleCommand = null;
        private OleDbDataAdapter oleAdapter = null;

        #endregion

        #region Constructor

        public ExcelMng(string sArgFileName)
        { 
            sFileName = sArgFileName;
            oleConn = new OleDbConnection(GetConnectionString(sFileName));
            oleCommand = new OleDbCommand();
            oleCommand.Connection = oleConn;
            oleAdapter = new OleDbDataAdapter();
            //oleAdapter.SelectCommand = oleCommand;
        }

        public ExcelMng()
        {
            oleConn = new OleDbConnection();
            oleCommand = new OleDbCommand();
            oleCommand.Connection = oleConn;
            oleAdapter = new OleDbDataAdapter();
            //oleAdapter.SelectCommand = oleCommand;
            //oleAdapter.UpdateCommand = oleCommand;
        }

        #endregion

        #region Getter/Setter : ExcelFileName
        public string ExcelFileName
        {
            get
            {
                return sFileName;
            }
            set
            {
                this.sFileName = value;
                this.oleConn.ConnectionString = GetConnectionString(value);
            }
        }
        #endregion

        // **********************************************************
        // Method
        // **********************************************************

        #region GetConnectionString
        private static string GetConnectionString(string FileName)
        {
            string sConnStr = string.Empty;

            sConnStr =
                @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data Source=" + FileName + ";" +
                @"Extended Properties=" + Convert.ToChar(34).ToString() +
                @"Excel 8.0;" + "Imex=2;HDR=No;" + Convert.ToChar(34).ToString();

            //if (FileName.Substring(FileName.Length - 4, 4).ToUpper() == "XLSX")
            //{
            //    sConnStr =
            //    @"Provider=Microsoft.ACE.OLEDB.12.0;" +
            //    @"Data Source=" + FileName + ";" +
            //    @"Extended Properties=" + Convert.ToChar(34).ToString() +
            //    @"Excel 12.0;" + "HDR=NO;IMEX=1;" + Convert.ToChar(34).ToString();
            //}
            //else
            //{
            //    sConnStr =
            //    @"Provider=Microsoft.Jet.OLEDB.4.0;" +
            //    @"Data Source=" + FileName + ";" +
            //    @"Extended Properties=" + Convert.ToChar(34).ToString() +
            //    @"Excel 8.0;" + "Imex=2;HDR=No;" + Convert.ToChar(34).ToString();
            //}
            return sConnStr;
        }

        private static string GetConnectionString(string FileName , string sExelFlag)
        {
            string sConnStr = string.Empty;
            if (sExelFlag == "XLSX")
            {
                sConnStr =
                @"Provider=Microsoft.ACE.OLEDB.12.0;" +
                @"Data Source=" + FileName + ";" +
                @"Extended Properties=Excel 12.0;" + Convert.ToChar(34).ToString();
            }
            else
            {
                sConnStr =
                @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data Source=" + FileName + ";" +
                @"Extended Properties=" + Convert.ToChar(34).ToString() +
                @"Excel 8.0;" + "Imex=2;HDR=No;" + Convert.ToChar(34).ToString();
            }
            

            return sConnStr;
        }
        #endregion

        #region GetExcelSheets
        public string[] GetExcelSheets()
        {
            System.Data.DataTable dt = null;

            string[] sRows = null;
            try
            {
                oleConn.Open();
                dt = oleConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt != null)
                {
                    sRows = new string[dt.Rows.Count];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        sRows[i] = dt.Rows[i]["TABLE_NAME"].ToString();
                        sRows[i] = sRows[i].Substring(0, sRows[i].Length - 1);
                    }
                }
                else
                {
                    sRows = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sRows;
        }
        #endregion

        #region GetSheetData
        public DataTable GetSheetData(string sSheetName)
        {
            DataTable dt = null;
            try
            {
                oleAdapter.SelectCommand = oleCommand;

                dt = new DataTable(sSheetName);
                oleCommand.CommandText = "SELECT * FROM [" + sSheetName + "$" + "]";

                oleAdapter.FillSchema(dt, SchemaType.Source);
                oleAdapter.Fill(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dt;
        }
        public DataTable GetSheetData(string sSheetName, string sStartCell, string sEndCell)
        {
            DataTable dt = null;
            try
            {
                dt = new DataTable(sSheetName);
                oleCommand.CommandText = "SELECT * FROM [" + sSheetName + "$" + sStartCell + ":" + sEndCell + "]";

                oleAdapter.FillSchema(dt, SchemaType.Source);
                oleAdapter.Fill(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dt;
        }
        #endregion


        #region Conn_Close
        public void Conn_Close()
        {
            try
            {
                if (oleConn.State == System.Data.ConnectionState.Open)
                {
                    oleAdapter.Dispose();
                    oleConn.Close();
                    oleConn.Dispose();
                    //oleCommand.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
