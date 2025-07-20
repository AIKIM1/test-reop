using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MNT001
{
    class MNT_Common
    {
        private static string customConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GMES\\SFU\\";

        public static DataSet GetConfigXML()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return ds;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_CONFIG");
                    dt.Columns.Add("SHOP", typeof(string));
                    dt.Columns.Add("AREA", typeof(string));
                    dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
                    dt.Columns.Add("PROCESS", typeof(string));
                    dt.Columns.Add("EQUIPMENT", typeof(string));
                    dt.Columns.Add("DISPLAYNAME", typeof(string));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["SHOP"] = "";
                    dr["AREA"] = "";
                    dr["EQUIPMENTSEGMENT"] = "";
                    dr["PROCESS"] = "";
                    dr["EQUIPMENT"] = "";
                    dr["DISPLAYNAME"] = "";
                    dr["DISPLAYTIME"] = 1;

                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        public static void SetConfigXML(DataSet ds)
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    customConfigFile.Delete();
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static DataSet GetConfigXML_PACK()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_PACK.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return ds;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_CONFIG");
                    dt.Columns.Add("SHOP", typeof(string));
                    dt.Columns.Add("AREA", typeof(string));
                    dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
                    dt.Columns.Add("PROCESS", typeof(string));
                    dt.Columns.Add("EQUIPMENT", typeof(string));
                    dt.Columns.Add("DISPLAYNAME", typeof(string));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["SHOP"] = "";
                    dr["AREA"] = "";
                    dr["EQUIPMENTSEGMENT"] = "";
                    dr["PROCESS"] = "";
                    dr["EQUIPMENT"] = "";
                    dr["DISPLAYNAME"] = "";
                    dr["DISPLAYTIME"] = 1;

                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        public static void SetConfigXML_PACK(DataSet ds)
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_PACK.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    customConfigFile.Delete();
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static DataSet GetConfigXML_EQPTLOSS()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_EQPTLOSS.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return ds;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_CONFIG_EQPTLOSS");
                    dt.Columns.Add("SHOP", typeof(string));
                    dt.Columns.Add("AREA", typeof(string));
                    dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
                    dt.Columns.Add("PROCESS", typeof(string));
                    dt.Columns.Add("VIEWROWCNT", typeof(string));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));
                    dt.Columns.Add("DISPLAYTIMESUB", typeof(int));
                    dt.Columns.Add("CHARTMAXVAL", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["SHOP"] = "";
                    dr["AREA"] = "";
                    dr["EQUIPMENTSEGMENT"] = "";
                    dr["PROCESS"] = "";
                    dr["VIEWROWCNT"] = 20;
                    dr["DISPLAYTIME"] = 5;
                    dr["DISPLAYTIMESUB"] = 30; 
                    dr["CHARTMAXVAL"] = 50;  

                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        public static void SetConfigXML_EQPTLOSS(DataSet ds)
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_EQPTLOSS.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    customConfigFile.Delete();
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static DataSet GetConfigXML_WIPSTAT()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_WIPSTAT.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return ds;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_CONFIG_WIPSTAT");
                    dt.Columns.Add("SHOP", typeof(string));
                    dt.Columns.Add("AREA", typeof(string)); 
                    dt.Columns.Add("PROCESS", typeof(string)); 
                    dt.Columns.Add("VIEWROWCNT", typeof(int));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));
                    dt.Columns.Add("DISPLAYTIMESUB", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["SHOP"] = "";
                    dr["AREA"] = ""; 
                    dr["PROCESS"] = "";
                    dr["VIEWROWCNT"] = 15;
                    dr["DISPLAYTIME"] = 10; 
                    dr["DISPLAYTIMESUB"] = 10;

                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        public static void SetConfigXML_WIPSTAT(DataSet ds)
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_WIPSTAT.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    customConfigFile.Delete();
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static DataSet GetConfigXML_WIPSTAT_V2()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_WIPSTAT_V2.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return ds;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_CONFIG_WIPSTAT_V2");
                    dt.Columns.Add("SHOP", typeof(string));
                    dt.Columns.Add("AREA", typeof(string));
                    dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
                    dt.Columns.Add("PLANFLAG", typeof(string));
                    dt.Columns.Add("VIEWROWCNT", typeof(string));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));
                    dt.Columns.Add("DISPLAYTIMESUB", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["SHOP"] = "";
                    dr["AREA"] = "";
                    dr["EQUIPMENTSEGMENT"] = "";
                    dr["PLANFLAG"] = "";
                    dr["VIEWROWCNT"] = 10;
                    dr["DISPLAYTIME"] = 30;
                    dr["DISPLAYTIMESUB"] = 20;

                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        public static void SetConfigXML_WIPSTAT_V2(DataSet ds)
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Config_WIPSTAT_V2.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    customConfigFile.Delete();
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static DataSet GetProcessConfigXML()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Process_Config.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return ds;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_CONFIG");
                    dt.Columns.Add("SHOP", typeof(string));
                    dt.Columns.Add("AREA", typeof(string));
                    dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
                    dt.Columns.Add("PROCESS", typeof(string));
                    dt.Columns.Add("EQUIPMENT", typeof(string));
                    dt.Columns.Add("DISPLAYNAME", typeof(string));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["SHOP"] = "";
                    dr["AREA"] = "";
                    dr["EQUIPMENTSEGMENT"] = "";
                    dr["PROCESS"] = "";
                    dr["EQUIPMENT"] = "";
                    dr["DISPLAYNAME"] = "";
                    dr["DISPLAYTIME"] = 1;

                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        public static void SetProcessConfigXML(DataSet ds)
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_Process_Config.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    customConfigFile.Delete();
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

		public static DataSet GetConfigXML_Formation_Stocker()
		{
			try
			{
				string settingFileName = "GMES.MES.MNT_Config_Formation_Stocker.config";

				FileInfo customConfigFile;
				if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
				{
					customConfigFile = new FileInfo(customConfigPath + settingFileName);
				}
				else
				{
					customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

					string[] directoryNames = customConfigPath.Split('\\');
					string current = string.Empty;
					for (int inx = 0; inx < directoryNames.Length - 1; inx++)
					{
						string directoryName = directoryNames[inx];
						if (string.IsNullOrEmpty(current))
						{
							current = directoryName;
						}
						else
						{
							current += @"\" + directoryName;
						}
						current = current.Replace("C:", @"\\Client\C$");
						DirectoryInfo directoryInfo = new DirectoryInfo(current);
						if (!directoryInfo.Exists)
						{
							directoryInfo.Create();
						}
					}
					customConfigFile = new FileInfo(current + @"\" + settingFileName);
				}

				if (customConfigFile.Exists)
				{
					DataSet ds = new DataSet();
					ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
					return ds;
				}
				else
				{
					DataSet ds = new DataSet();

					DataTable dt = new DataTable("MNT_CONFIG");
					dt.Columns.Add("SHOP", typeof(string));
					dt.Columns.Add("AREA", typeof(string));
					dt.Columns.Add("BLDG_CODE", typeof(string));
					dt.Columns.Add("LEVEL", typeof(string));
					dt.Columns.Add("TRAY_TYPE", typeof(string));
					dt.Columns.Add("DISPLAYVIEWROWCNT", typeof(int));
					dt.Columns.Add("DISPLAYREMARKROWCNT", typeof(int));
					dt.Columns.Add("DISPLAYTIME", typeof(int));
					dt.Columns.Add("DISPLAYTIMESUB", typeof(int));
					dt.Columns.Add("COLOR_DISP_FLAG", typeof(string));

					DataRow dr = dt.NewRow();
					dr["SHOP"] = "G481";
					dr["AREA"] = "A7";
					dr["BLDG_CODE"] = "";
					dr["LEVEL"] = "";
					dr["TRAY_TYPE"] = "";
					dr["DISPLAYVIEWROWCNT"] = 8;
					dr["DISPLAYREMARKROWCNT"] = 20;
					dr["DISPLAYTIME"] = 1;
					dr["DISPLAYTIMESUB"] = 10;
					dr["COLOR_DISP_FLAG"] = "Y";

					dt.Rows.Add(dr);
					ds.Tables.Add(dt);

					ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

					return ds;
				}
			}
			catch (Exception ex)
			{
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
				return null;
			}
		}

		public static void SetConfigXML_Formation_Stocker(DataSet ds)
		{
			try
			{
				string settingFileName = "GMES.MES.MNT_Config_Formation_Stocker.config";

				FileInfo customConfigFile;
				if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
				{
					customConfigFile = new FileInfo(customConfigPath + settingFileName);
				}
				else
				{
					customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

					string[] directoryNames = customConfigPath.Split('\\');
					string current = string.Empty;
					for (int inx = 0; inx < directoryNames.Length - 1; inx++)
					{
						string directoryName = directoryNames[inx];
						if (string.IsNullOrEmpty(current))
						{
							current = directoryName;
						}
						else
						{
							current += @"\" + directoryName;
						}
						current = current.Replace("C:", @"\\Client\C$");
						DirectoryInfo directoryInfo = new DirectoryInfo(current);
						if (!directoryInfo.Exists)
						{
							directoryInfo.Create();
						}
					}
					customConfigFile = new FileInfo(current + @"\" + settingFileName);
				}

				if (customConfigFile.Exists)
				{
					customConfigFile.Delete();
					ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
				}
				else
				{
					ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
				}
			}
			catch (Exception ex)
			{
				LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
			}
		}

               

        public static void SetConfigXML_LDR_UDR(DataSet ds)
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_UDR_LDR.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    customConfigFile.Delete();
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static DataSet GetConfigXML_LDR_UDR()
        {

            try
            {
                string settingFileName = "GMES.MES.MNT_UDR_LDR.config";

                FileInfo customConfigFile;
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];

                    string[] directoryNames = customConfigPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return ds;
                }
                else
                {
                    DataSet ds = new DataSet();

                    DataTable dt = new DataTable("MNT_SETTING");
                    dt.Columns.Add("EQUIPMENTSEGMENT", typeof(string));
                    dt.Columns.Add("EQUIPMENT_1", typeof(string));
                    dt.Columns.Add("EQUIPMENT_2", typeof(string));
                    dt.Columns.Add("EQUIPMENT_3", typeof(string));
                    dt.Columns.Add("DISPLAYTIME", typeof(int));

                    DataRow dr = dt.NewRow();
                    dr["EQUIPMENTSEGMENT"] = string.Empty;
                    dr["EQUIPMENT_1"] = string.Empty;
                    dr["EQUIPMENT_2"] = string.Empty;
                    dr["EQUIPMENT_3"] = string.Empty;
                    dr["DISPLAYTIME"] = 1;
                    dt.Rows.Add(dr);
                    ds.Tables.Add(dt);

                    ds.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);

                    return ds;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }


    }
}