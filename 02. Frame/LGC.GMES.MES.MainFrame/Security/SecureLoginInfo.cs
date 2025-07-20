using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace LGC.GMES.MES.MainFrame.Security
{
    class SecureLoginInfo
    {
        private AESCoder coder = null;

        private string tableName = "SecuredInfo";
        private string columnName_LANGID = null;
        private string columnName_USERID = null;
        private string columnName_USERPW = null;
        private string columnName_AUTOLOGIN = null;

        private string columnName_CONNECT = null;
        private string columnName_POSITION = null;
        private string columnName_SITE = null;
        private string columnName_LINE = null;
        private string columnName_PROCESS = null;

        private DataSet info = null;

        private string filePath;

        internal SecureLoginInfo()
        {
            coder = new AESCoder();

            columnName_LANGID = coder.Encode("LANGID");
            columnName_USERID = coder.Encode("USERID");
            columnName_USERPW = coder.Encode("USERPW");
            columnName_AUTOLOGIN = coder.Encode("AUTOLOGIN");

            columnName_CONNECT = coder.Encode("CONNECT");
            columnName_POSITION = coder.Encode("POSITION");
            columnName_SITE = coder.Encode("SITE");
            columnName_LINE = coder.Encode("LINE");
            columnName_PROCESS = coder.Encode("PROCESS");

            if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "SBC")
            {
                filePath = @"\\Client\C$\" + "info.config";
                //filePath = @"\\Client\C$\Program Files" + "\\info.config";
            }
            else
            {
                filePath = Environment.CurrentDirectory + "\\info.config";
            }

            info = new DataSet();
            DataTable dt = info.Tables.Add(tableName);
            dt.Columns.Add(columnName_LANGID, typeof(string));
            dt.Columns.Add(columnName_USERID, typeof(string));
            dt.Columns.Add(columnName_USERPW, typeof(string));
            dt.Columns.Add(columnName_AUTOLOGIN, typeof(string));

            dt.Columns.Add(columnName_CONNECT, typeof(string));
            dt.Columns.Add(columnName_POSITION, typeof(string));
            dt.Columns.Add(columnName_SITE, typeof(string));
            dt.Columns.Add(columnName_LINE, typeof(string));
            dt.Columns.Add(columnName_PROCESS, typeof(string));

            if (File.Exists(filePath))
            {
                info.ReadXml(filePath, XmlReadMode.IgnoreSchema);
            }
            else
            {
                DataRow dr = dt.NewRow();
                dr[columnName_LANGID] = coder.Encode(string.Empty);
                dr[columnName_USERID] = coder.Encode(string.Empty);
                dr[columnName_USERPW] = coder.Encode(string.Empty);
                dr[columnName_AUTOLOGIN] = coder.Encode(false.ToString());

                dr[columnName_CONNECT] = coder.Encode(false.ToString());
                dr[columnName_POSITION] = coder.Encode(false.ToString());
                dr[columnName_SITE] = coder.Encode(false.ToString());
                dr[columnName_LINE] = coder.Encode(false.ToString());
                dr[columnName_PROCESS] = coder.Encode(false.ToString());
                
                dt.Rows.Add(dr);
            }
        }

        private void Save()
        {
            if (LGC.GMES.MES.Common.Common.APP_SBC_MODE == "SBC")
            {
                info.WriteXml(filePath + "\\info.config", XmlWriteMode.IgnoreSchema);
            }
            else
            {
                info.WriteXml(filePath, XmlWriteMode.IgnoreSchema);
            }
        }

        internal string LANGID
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_LANGID].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_LANGID] = coder.Encode(value);
                Save();
            }
        }

        internal string ID
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_USERID].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_USERID] = coder.Encode(value);
                Save();
            }
        }

        internal string PW
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_USERPW].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_USERPW] = coder.Encode(value);
                Save();
            }
        }

        internal bool AUTOLOGIN
        {
            get
            {
                return bool.Parse(coder.Decode(info.Tables[tableName].Rows[0][columnName_AUTOLOGIN].ToString()));
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_AUTOLOGIN] = coder.Encode(value.ToString());
                Save();
            }
        }

        internal string CONNECT
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_CONNECT].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_CONNECT] = coder.Encode(value.ToString());
                Save();
            }
        }

        internal string POSITION
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_POSITION].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_POSITION] = coder.Encode(value.ToString());
                Save();
            }
        }

        internal string SITE
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_SITE].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_SITE] = coder.Encode(value.ToString());
                Save();
            }
        }

        internal string LINE
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_LINE].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_LINE] = coder.Encode(value.ToString());
                Save();
            }
        }

        internal string PROCESS
        {
            get
            {
                return coder.Decode(info.Tables[tableName].Rows[0][columnName_PROCESS].ToString());
            }
            set
            {
                info.Tables[tableName].Rows[0][columnName_PROCESS] = coder.Encode(value.ToString());
                Save();
            }
        }
    }

}
