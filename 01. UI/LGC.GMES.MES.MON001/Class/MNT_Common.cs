using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.MON001
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
    }
}
