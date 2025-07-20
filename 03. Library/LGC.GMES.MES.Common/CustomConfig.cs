/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.06.27   권용섭    : [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림
  2024.08.26   최성필    : FDS 발열셀, SAS 송수신 오류 알람 추가
**************************************************************************************/
#region Import Library
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
#endregion

namespace LGC.GMES.MES.Common
{
    public class CustomConfig
    {
        #region Common Tab
        public static readonly string CONFIGTABLE_COMMON = "COMMON";
        public static readonly string CONFIGTABLE_COMMON_SITE = "SITE";
        public static readonly string CONFIGTABLE_COMMON_SHOP = "SHOP";
        public static readonly string CONFIGTABLE_COMMON_AREA = "AREA";
        public static readonly string CONFIGTABLE_COMMON_PCSG = "PCSG";
        public static readonly string CONFIGTABLE_COMMON_EQSG = "EQSG";
        public static readonly string CONFIGTABLE_COMMON_PROCESS = "PROCESS";
        public static readonly string CONFIGTABLE_COMMON_DEFAULTMENU = "DEFAULTMENU";
        public static readonly string CONFIGTABLE_ORG = "ORG";
        public static readonly string CONFIGTABLE_ORG_CODE = "ORG_CODE";
        public const string CONFIGTABLE_APPLICATION = "APPLICATION";
        public const string CONFIGTABLE_APPLICATION_AUTOLOGIN = "AUTOLOGIN";
        public const string CONFIGTABLE_MONITORING = "MONITORING";
        public const string CONFIGTABLE_MONITORING_FULLMONITORING = "FULLMONITORING";
        public const string CONFIGTABLE_LOGGING = "LOGGING";
        public const string CONFIGTABLE_LOGGING_UI = "UI";
        public const string CONFIGTABLE_LOGGING_MONITORING = "MONITORING";
        public const string CONFIGTABLE_LOGGING_FRAME = "FRAME";
        public const string CONFIGTABLE_LOGGING_BIZRULE = "BIZRULE";
        public const string CONFIGTABLE_ETC = "ETC";
        public const string CONFIGTABLE_ETC_NOTICEUSE = "NOTICE";
        public const string CONFIGTABLE_ETC_SMALLTYPE = "SMALL";
        public const string CONFIGTABLE_ETC_WIPSTAT = "WIPSTAT";
        public const string CONFIGTABLE_ETC_SAMPLE_COPIES = "SAMPLECOPIES";
        public const string CONFIGTABLE_ETC_HISTCARD_SCALE = "HISTCARDSCALE";
        public const string CONFIGTABLE_ETC_PAPER_SIZE = "PAPERSIZE";
        #endregion

        #region Printer Tab
        public static readonly string CONFIGTABLE_SCANTYPE = "SCANTYPE";
        public static readonly string CONFIGTABLE_SCANTYPE_SHOPID = "SHOPID";
        public static readonly string CONFIGTABLE_SCANTYPE_LABELTYPE = "LABEL_TYPE";
        public static readonly string CONFIGTABLE_SERIALPRINTER = "SERIALPRINTER";
        public static readonly string CONFIGTABLE_SERIALPRINTER_DEFAULT = "DEFAULT";
        public static readonly string CONFIGTABLE_SERIALPRINTER_PRINTERNAME = "PRINTERNAME";
        public static readonly string CONFIGTABLE_SERIALPRINTER_PRINTERKEY = "PRINTERKEY";
        public static readonly string CONFIGTABLE_SERIALPRINTER_PRINTERTYPE = "PRINTERTYPE";
        public static readonly string CONFIGTABLE_SERIALPRINTER_ISACTIVE = "ISACTIVE";
        public static readonly string CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY = "PARENTPRINTERKEY";
        public static readonly string CONFIGTABLE_SERIALPRINTER_LABELTYPE = "LABEL_TYPE";
        public static readonly string CONFIGTABLE_SERIALPRINTER_PORTNAME = "PORTNAME";
        public static readonly string CONFIGTABLE_SERIALPRINTER_BAUDRATE = "BAUDRATE";
        public static readonly string CONFIGTABLE_SERIALPRINTER_PARITYBIT = "PARITYBIT";
        public static readonly string CONFIGTABLE_SERIALPRINTER_DATABIT = "DATABIT";
        public static readonly string CONFIGTABLE_SERIALPRINTER_STOPBIT = "STOPBIT";
        public static readonly string CONFIGTABLE_SERIALPRINTER_DPI = "DPI";
        public static readonly string CONFIGTABLE_SERIALPRINTER_DARKNESS = "DARKNESS";
        public static readonly string CONFIGTABLE_SERIALPRINTER_X = "X";
        public static readonly string CONFIGTABLE_SERIALPRINTER_Y = "Y";
        public static readonly string CONFIGTABLE_SERIALPRINTER_COPIES = "COPIES";
        public static readonly string CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS = "CONNECTIONLESS";
        public static readonly string CONFIGTABLE_SERIALPRINTER_EQUIPMENT = "EQUIPMENT";
        public static readonly string CONFIGTABLE_LABEL = "LABEL";
        public static readonly string CONFIG_LABEL_TYPE = "CFG_LABEL_TYPE";
        public static readonly string CONFIG_LABEL_COPIES = "CFG_LABEL_COPIES";
        public static readonly string CONFIG_CUT_LABEL = "CFG_CUT_LABEL";
        public static readonly string CONFIG_LABEL_AUTO = "CFG_LABEL_AUTO";
        public static readonly string CONFIG_CARD_COPIES = "CFG_CARD_COPIES";
        public static readonly string CONFIG_CARD_AUTO = "CFG_CARD_AUTO";
        public static readonly string CONFIG_CARD_POPUP = "CFG_CARD_POPUP";
        public static readonly string CONFIG_THERMAL_COPIES = "CFG_THERMAL_COPIES";
        public static readonly string CONFIGTABLE_THERMALPRINTER = "THERMALPRINTER";
        public static readonly string CONFIGTABLE_THERMALPRINTER_NAME = "THERMALPRINTERNAME";
        public static readonly string CONFIGTABLE_THERMALPRINTER_X = "X";
        public static readonly string CONFIGTABLE_THERMALPRINTER_Y = "Y";
        public static readonly string CONFIGTABLE_GENERALPRINTER = "GENERAL_PRINTER";
        public static readonly string CONFIGTABLE_GENERALPRINTER_NAME = "GENERAL_PRINTER_NAME";
        public static readonly string CONFIGTABLE_SERIALPRINTER_LABELID = "LABELID";
        public static readonly string CONFIG_LABEL_FIRST_CUT_COPIES = "CFG_LABEL_FIRST_CUT_COPIES";

        #endregion

        #region Scanner Tab
        public static readonly string CONFIGTABLE_SCANITEM = "SCANITEM";
        public static readonly string CONFIGTABLE_SCANITEM_PARTKEY = "PARTKEY";
        public static readonly string CONFIGTABLE_SCANITEM_PARTNAME = "PARTNAME";
        public static readonly string CONFIGTABLE_SCANITEM_VALIDATION = "VALIDATION";
        public static readonly string CONFIGTABLE_SCANITEM_USEYN = "USEYN";
        public static readonly string CONFIGTABLE_SOUND = "SOUND";
        public static readonly string CONFIGTABLE_SOUND_USEYN = "USEYN";
        public static readonly string CONFIGTABLE_SOUND_OK = "OKFILE";
        public static readonly string CONFIGTABLE_SOUND_NG = "NGFILE";
        public static readonly string CONFIGTABLE_SERIALPORT = "SERIALPORT";
        public static readonly string CONFIGTABLE_SERIALPORT_PORTNAME = "PORTNAME";
        public static readonly string CONFIGTABLE_SERIALPORT_BAUDRATE = "BAUDRATE";
        public static readonly string CONFIGTABLE_SERIALPORT_PARITYBIT = "PARITYBIT";
        public static readonly string CONFIGTABLE_SERIALPORT_DATABIT = "DATABIT";
        public static readonly string CONFIGTABLE_SERIALPORT_STOPBIT = "STOPBIT";
        public static readonly string CONFIGTABLE_SERIALPORT_EQUIPMENT_SEGMENT = "LINE";
        #endregion

        public string ACTIVE_GENERAL_PRINTER_NAME = string.Empty;
        public string ACTIVE_THERMAL_PRINTER_NAME = string.Empty;

        #region Validation Tab
        public static readonly string CONFIGTABLE_COMMONVALIDATION = "COMMONVALIDATION";
        public static readonly string CONFIGTABLE_COMMONVALIDATION_VALIDATIONKEY = "CMCODE";
        public static readonly string CONFIGTABLE_COMMONVALIDATION_VALIDATIONNAME = "CMCDNAME";
        public static readonly string CONFIGTABLE_COMMONVALIDATION_USEYN = "USEYN";
        public static readonly string CONFIGTABLE_PROCESSVALIDATIONTARGET = "PROCESSVALIDATIONTARGET";
        public static readonly string CONFIGTABLE_PROCESSVALIDATIONTARGET_PROCESS = "PROCESS";
        public static readonly string CONFIGTABLE_PROCESSVALIDATION = "PROCESSVALIDATION";
        public static readonly string CONFIGTABLE_PROCESSVALIDATION_VALIDATIONKEY = "VALIDATIONKEY";
        public static readonly string CONFIGTABLE_PROCESSVALIDATION_VALIDATIONNAME = "VALIDATIONNAME";
        public static readonly string CONFIGTABLE_PROCESSVALIDATION_USEYN = "USEYN";
        #endregion

        public static CustomConfig Instance = new CustomConfig();

        private string customConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GMES\\SFU\\";

        private DataSet _ConfigSet = new DataSet();
        public DataSet ConfigSet { get { return _ConfigSet.Copy(); } }

        private CustomConfig()
        {
            //Reload(ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"]);
        }

        public void Reload(string settingFileName)
        {
            try
            {
                FileInfo customConfigFile;
                string current = string.Empty;

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    customConfigFile = new FileInfo(customConfigPath + settingFileName);
                }
                else
                {
                    customConfigPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];
                    settingFileName = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_NAME"];

                    string[] directoryNames = customConfigPath.Split('\\');

                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];

                        if (string.IsNullOrEmpty(current))
                            current = directoryName;
                        else
                            current += @"\" + directoryName;

                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);

                        if (!directoryInfo.Exists)
                            directoryInfo.Create();
                    }

                    customConfigFile = new FileInfo(current + @"\" + settingFileName);
                }

                if (customConfigFile.Exists)
                {
                    DataSet configSet = new DataSet();
                    configSet.ReadXml(customConfigFile.FullName);
                    _ConfigSet = configSet;

                    var printerQuery = new ManagementObjectSearcher("Select * from Win32_Printer");

                    if (configSet.Tables[CONFIGTABLE_GENERALPRINTER] == null)
                    {
                        LoginInfo.CFG_GENERAL_PRINTER = CreateEmptyGeneralPrintTable();
                    }
                    else
                    {
                        foreach (DataRow dr in configSet.Tables[CONFIGTABLE_GENERALPRINTER].Rows)
                        {
                            string s = dr[CONFIGTABLE_GENERALPRINTER_NAME] as string ?? string.Empty;
                            string printerName = s.Contains('(') ? s.Substring(0, s.IndexOf('(')) : s;

                            foreach (var printer in printerQuery.Get())
                            {
                                string name = printer.GetPropertyValue("Name") as string;
                                string checkName = name.Contains('(') ? name.Substring(0, name.IndexOf('(')) : name;

                                if (printerName.Trim().Equals(checkName.Trim()))
                                {
                                    dr[CONFIGTABLE_GENERALPRINTER_NAME] = name;
                                    break;
                                }
                            }
                        }

                        configSet.Tables[CONFIGTABLE_GENERALPRINTER].AcceptChanges();
                        LoginInfo.CFG_GENERAL_PRINTER = configSet.Tables[CONFIGTABLE_GENERALPRINTER];
                    }

                    if (configSet.Tables[CONFIGTABLE_SERIALPRINTER] == null)
                    {
                        LoginInfo.CFG_SERIAL_PRINT = CreateEmptySerialPrinterTable();
                    }
                    else
                    {
                        foreach (DataRow dr in configSet.Tables[CONFIGTABLE_SERIALPRINTER].Rows)
                        {
                            if ((dr[CONFIGTABLE_SERIALPRINTER_PORTNAME] as string ?? string.Empty).Contains("USB"))
                            {
                                string s = dr[CONFIGTABLE_SERIALPRINTER_PRINTERNAME] as string ?? string.Empty;
                                string printerName = s.Contains('(') ? s.Substring(0, s.IndexOf('(')) : s;

                                foreach (var printer in printerQuery.Get())
                                {
                                    string name = printer.GetPropertyValue("Name") as string;
                                    string checkName = name.Contains('(') ? name.Substring(0, name.IndexOf('(')) : name;

                                    if (printerName.Trim().Equals(checkName.Trim()))
                                    {
                                        dr[CONFIGTABLE_SERIALPRINTER_PRINTERNAME] = name;
                                        break;
                                    }
                                }
                            }
                        }

                        configSet.Tables[CONFIGTABLE_SERIALPRINTER].AcceptChanges();
                        LoginInfo.CFG_SERIAL_PRINT = configSet.Tables[CONFIGTABLE_SERIALPRINTER];
                    }

                    if (configSet.Tables[CONFIGTABLE_THERMALPRINTER] == null)
                    {
                        LoginInfo.CFG_THERMAL_PRINT = CreateEmptyThermalPrintTable();
                    }
                    else
                    {
                        foreach (DataRow dr in configSet.Tables[CONFIGTABLE_THERMALPRINTER].Rows)
                        {
                            string s = dr[CONFIGTABLE_THERMALPRINTER_NAME] as string ?? string.Empty;
                            string printerName = s.Contains('(') ? s.Substring(0, s.IndexOf('(')) : s;

                            foreach (var printer in printerQuery.Get())
                            {
                                string name = printer.GetPropertyValue("Name") as string;
                                string checkName = name.Contains('(') ? name.Substring(0, name.IndexOf('(')) : name;

                                if (printerName.Trim().Equals(checkName.Trim()))
                                {
                                    dr[CONFIGTABLE_THERMALPRINTER_NAME] = name;
                                    break;
                                }
                            }
                        }

                        configSet.Tables[CONFIGTABLE_THERMALPRINTER].AcceptChanges();
                        LoginInfo.CFG_THERMAL_PRINT = configSet.Tables[CONFIGTABLE_THERMALPRINTER];
                    }

                    if (configSet.Tables[CONFIGTABLE_LOGGING] == null)
                        LoginInfo.CFG_LOGGING = CreateEmptyLoggingTable();
                    else
                        LoginInfo.CFG_LOGGING = configSet.Tables[CONFIGTABLE_LOGGING];

                    if (!LoginInfo.CFG_SERIAL_PRINT.Columns.Contains(CONFIGTABLE_SERIALPRINTER_DEFAULT))
                    {
                        LoginInfo.CFG_SERIAL_PRINT.Columns.Add(new DataColumn() { ColumnName = CONFIGTABLE_SERIALPRINTER_DEFAULT, DataType = typeof(bool) });
                        int cnt = 0;

                        foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            if (cnt == 0)
                                row[CONFIGTABLE_SERIALPRINTER_DEFAULT] = true;
                            else
                                row[CONFIGTABLE_SERIALPRINTER_DEFAULT] = false;
                    }

                    if (!LoginInfo.CFG_SERIAL_PRINT.Columns.Contains(CONFIGTABLE_SERIALPRINTER_PRINTERNAME))
                    {
                        LoginInfo.CFG_SERIAL_PRINT.Columns.Add(new DataColumn() { ColumnName = CONFIGTABLE_SERIALPRINTER_PRINTERNAME, DataType = typeof(string) });

                        foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            row[CONFIGTABLE_SERIALPRINTER_PRINTERNAME] = string.Empty;
                    }

                    if (!LoginInfo.CFG_SERIAL_PRINT.Columns.Contains(CONFIGTABLE_SERIALPRINTER_PRINTERTYPE))
                    {
                        LoginInfo.CFG_SERIAL_PRINT.Columns.Add(new DataColumn() { ColumnName = CONFIGTABLE_SERIALPRINTER_PRINTERTYPE, DataType = typeof(string) });

                        foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            row[CONFIGTABLE_SERIALPRINTER_PRINTERTYPE] = "Z";
                    }

                    if (!LoginInfo.CFG_SERIAL_PRINT.Columns.Contains(CONFIGTABLE_SERIALPRINTER_DPI))
                    {
                        LoginInfo.CFG_SERIAL_PRINT.Columns.Add(new DataColumn() { ColumnName = CONFIGTABLE_SERIALPRINTER_DPI, DataType = typeof(int) });

                        foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            row[CONFIGTABLE_SERIALPRINTER_DPI] = 300;
                    }

                    if (!LoginInfo.CFG_SERIAL_PRINT.Columns.Contains(CONFIGTABLE_SERIALPRINTER_DARKNESS))
                    {
                        LoginInfo.CFG_SERIAL_PRINT.Columns.Add(new DataColumn() { ColumnName = CONFIGTABLE_SERIALPRINTER_DARKNESS, DataType = typeof(int) });

                        foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            row[CONFIGTABLE_SERIALPRINTER_DARKNESS] = 15;
                    }

                    if (!LoginInfo.CFG_SERIAL_PRINT.Columns.Contains(CONFIGTABLE_SERIALPRINTER_EQUIPMENT))
                        LoginInfo.CFG_SERIAL_PRINT.Columns.Add(new DataColumn() { ColumnName = CONFIGTABLE_SERIALPRINTER_EQUIPMENT, DataType = typeof(string) });

                    if (!LoginInfo.CFG_SERIAL_PRINT.Columns.Contains(CONFIGTABLE_SERIALPRINTER_LABELID))
                        LoginInfo.CFG_SERIAL_PRINT.Columns.Add(new DataColumn() { ColumnName = CONFIGTABLE_SERIALPRINTER_LABELID, DataType = typeof(string) });

                    if (configSet.Tables[CONFIGTABLE_LABEL] != null)
                    {
                        LoginInfo.CFG_LABEL_TYPE = Convert.ToString(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_LABEL_TYPE]);
                        LoginInfo.CFG_LABEL_COPIES = Convert.ToInt32(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_LABEL_COPIES]);
                        LoginInfo.CFG_CUT_LABEL = configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_CUT_LABEL] as string ?? string.Empty;
                        LoginInfo.CFG_LABEL_AUTO = Convert.ToString(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_LABEL_AUTO]);
                        LoginInfo.CFG_CARD_COPIES = Convert.ToInt32(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_CARD_COPIES]);
                        LoginInfo.CFG_CARD_AUTO = Convert.ToString(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_CARD_AUTO]);
                        LoginInfo.CFG_CARD_POPUP = Convert.ToString(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_CARD_POPUP]);
                        LoginInfo.CFG_THERMAL_COPIES = Convert.ToInt32(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_THERMAL_COPIES]);
                        LoginInfo.CFG_LABEL_FIRST_CUT_COPIES = Convert.ToInt32(configSet.Tables[CONFIGTABLE_LABEL].Rows[0][CONFIG_LABEL_FIRST_CUT_COPIES]);
                    }

                    if (configSet.Tables[CONFIGTABLE_ETC] == null)
                        LoginInfo.CFG_ETC = CreateEmptyEtcTable();
                    else
                        LoginInfo.CFG_ETC = configSet.Tables[CONFIGTABLE_ETC];

                    #region 활성화 관련 Config
                    if (configSet.Tables.Contains("FORM"))
                    {
                        LoginInfo.CFG_FORM = configSet.Tables["FORM"];

                        if (configSet.Tables["FORM"] != null
                            && configSet.Tables["FORM"].Rows.Count > 0)
                        {
                            LoginInfo.CFG_EQP_STATUS = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["EQP_STATUS"]);
                            LoginInfo.CFG_W_LOT = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["W_LOT"]);
                            LoginInfo.CFG_FORM_PROC_WAIT_LIMIT_TIME_OVER = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["PROC_WAIT_LIMIT_TIME_OVER"]);
                            LoginInfo.CFG_FORM_AGING_LIMIT_TIME_OVER = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["AGING_LIMIT_TIME_OVER"]);
                            LoginInfo.CFG_FORM_AGING_OUTPUT_TIME_OVER = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["AGING_OUTPUT_TIME_OVER"]); // 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
                            //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
                            LoginInfo.CFG_FORM_FITTED_DOCV_TRNF_FAIL = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["FITTED_DOCV_TRNF_FAIL"]);
                            //2024.08.20 / 최성필(cso59463) / FDS 발열셀 알람 팝업 사용 여부
                            LoginInfo.CFG_FORM_FDS_ALARM = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["FORM_FDS_ALARM"]);
                            //2024.08.26 / 최성필(cso59463) / SAS 송수신 오류 알람 팝업 사용 여부
                            LoginInfo.CFG_FORM_SAS_ALARM = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["FORM_SAS_ALARM"]);
                            //2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부
                            LoginInfo.CFG_FORM_HIGH_AGING_ABNORM_TMPR_ALARM = Convert.ToBoolean(configSet.Tables["FORM"].Rows[0]["FORM_HIGH_AGING_ABNORM_TMPR_ALARM"]);

                        }
                    }
                    #endregion

                    FileInfo info = new FileInfo(customConfigFile.FullName);

                    if (info.Exists)
                        File.Delete(customConfigFile.FullName);

                    configSet.WriteXml(customConfigFile.FullName, XmlWriteMode.WriteSchema);
                }
                else
                {
                    LoginInfo.CFG_GENERAL_PRINTER = CreateEmptyGeneralPrintTable();
                    LoginInfo.CFG_SERIAL_PRINT = CreateEmptySerialPrinterTable();
                    LoginInfo.CFG_THERMAL_PRINT = CreateEmptyThermalPrintTable();
                    LoginInfo.CFG_LOGGING = CreateEmptyLoggingTable();
                    //LoginInfo.CFG_ETC = CreateEmptyLoggingTable();
                    LoginInfo.CFG_ETC = CreateEmptyEtcTable();
                    LoginInfo.CFG_FORM = CreateEmptyFormTable();
                }
            }
            catch (Exception e)
            {
                //System.Windows.MessageBox.Show(e.ToString());
            }
        }

        public void WriteConfigSet(DataSet configSet, string settingFileName)
        {
            string[] directoryNames = customConfigPath.Split('\\');
            string current = string.Empty;

            for (int inx = 0; inx < directoryNames.Length - 1; inx++)
            {
                string directoryName = directoryNames[inx];

                if (string.IsNullOrEmpty(current))
                    current = directoryName;
                else
                    current += "\\" + directoryName;

                DirectoryInfo directoryInfo = new DirectoryInfo(current);

                if (!directoryInfo.Exists)
                    directoryInfo.Create();
            }

            configSet.WriteXml(customConfigPath + settingFileName, XmlWriteMode.WriteSchema);
            _ConfigSet = configSet;
        }

        public DataSet ReadConfigSet(string settingFileName)
        {
            DataSet configSet = new DataSet();

            FileInfo customConfigFile = new FileInfo(customConfigPath + settingFileName);
            if (customConfigFile.Exists)
            {
                try
                {
                    configSet.ReadXml(customConfigFile.FullName, XmlReadMode.ReadSchema);
                    return configSet;
                }
                catch (Exception ex)
                {
                    return configSet;
                }
                finally
                {
                }
            }
            else
            {
                return configSet;
            }
        }

        public string CONFIG_COMMON_SITE
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_COMMON].Rows[0][CONFIGTABLE_COMMON_SITE].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string CONFIG_COMMON_SHOP
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_COMMON].Rows[0][CONFIGTABLE_COMMON_SHOP].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string CONFIG_COMMON_AREA
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_COMMON].Rows[0][CONFIGTABLE_COMMON_AREA].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string CONFIG_COMMON_PCSG
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_COMMON].Rows[0][CONFIGTABLE_COMMON_PCSG].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string CONFIG_COMMON_EQSG
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_COMMON].Rows[0][CONFIGTABLE_COMMON_EQSG].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string CONFIG_COMMON_PROCESS
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_COMMON].Rows[0][CONFIGTABLE_COMMON_PROCESS].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string CONFIG_COMMON_DEFAULTMENU
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_COMMON].Rows[0][CONFIGTABLE_COMMON_DEFAULTMENU].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string[] CONFIG_COMMON_ORG
        {
            get
            {
                try
                {
                    return (from DataRow r in ConfigSet.Tables[CONFIGTABLE_ORG].Rows select r[CONFIGTABLE_ORG_CODE].ToString()).ToArray<string>();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string CONFIG_SCANTYPE_LABELTYPE
        {
            get
            {
                try
                {
                    return ConfigSet.Tables[CONFIGTABLE_SCANTYPE].Rows[0][CONFIGTABLE_SCANTYPE_LABELTYPE].ToString();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string[] CONFIG_COMMONVALIDATION
        {
            get
            {
                try
                {
                    return (from DataRow r in ConfigSet.Tables[CONFIGTABLE_COMMONVALIDATION].Rows where true.Equals(r[CONFIGTABLE_COMMONVALIDATION_USEYN]) select r[CONFIGTABLE_COMMONVALIDATION_VALIDATIONKEY].ToString()).ToArray<string>();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public string[] CONFIG_PROCESSVALIDATION
        {
            get
            {
                try
                {
                    return (from DataRow r in ConfigSet.Tables[CONFIGTABLE_PROCESSVALIDATION].Rows where true.Equals(r[CONFIGTABLE_PROCESSVALIDATION_USEYN]) select r[CONFIGTABLE_PROCESSVALIDATION_VALIDATIONKEY].ToString()).ToArray<string>();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public bool CONFIG_MONITORING_FULLSCREEN
        {
            get
            {
                try
                {
                    return (bool)ConfigSet.Tables[CONFIGTABLE_MONITORING].Rows[0][CONFIGTABLE_MONITORING_FULLMONITORING];
                }
                catch (Exception ex)
                {
                    return true;
                }
            }
        }

        public bool CONFIG_LOGGING_UI
        {
            get
            {
                try
                {
                    return (bool)ConfigSet.Tables[CONFIGTABLE_LOGGING].Rows[0][CONFIGTABLE_LOGGING_UI];
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool CONFIG_LOGGING_MONITORING
        {
            get
            {
                try
                {
                    return (bool)ConfigSet.Tables[CONFIGTABLE_LOGGING].Rows[0][CONFIGTABLE_LOGGING_MONITORING];
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool CONFIG_LOGGING_FRAME
        {
            get
            {
                try
                {
                    return (bool)ConfigSet.Tables[CONFIGTABLE_LOGGING].Rows[0][CONFIGTABLE_LOGGING_FRAME];
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool CONFIG_LOGGING_BIZRULE
        {
            get
            {
                try
                {
                    return (bool)ConfigSet.Tables[CONFIGTABLE_LOGGING].Rows[0][CONFIGTABLE_LOGGING_BIZRULE];
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        private DataTable CreateEmptyGeneralPrintTable()
        {
            DataTable emptyGeneralPrintTable = new DataTable();
            emptyGeneralPrintTable.TableName = CONFIGTABLE_GENERALPRINTER;
            emptyGeneralPrintTable.Columns.Add(CONFIGTABLE_GENERALPRINTER_NAME, typeof(string));

            return emptyGeneralPrintTable;
        }

        private DataTable CreateEmptySerialPrinterTable()
        {
            DataTable emptySerialPrinterTable = new DataTable();
            emptySerialPrinterTable.TableName = CONFIGTABLE_SERIALPRINTER;
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_DEFAULT, typeof(bool));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_PRINTERNAME, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_PRINTERTYPE, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_PRINTERKEY, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_ISACTIVE, typeof(bool));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_PARENTPRINTERKEY, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_LABELTYPE, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_PORTNAME, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_BAUDRATE, typeof(int));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_PARITYBIT, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_DATABIT, typeof(int));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_STOPBIT, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_DPI, typeof(int));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_DARKNESS, typeof(int));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_X, typeof(int));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_Y, typeof(int));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_COPIES, typeof(int));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_CONNECTIONLESS, typeof(bool));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_EQUIPMENT, typeof(string));
            emptySerialPrinterTable.Columns.Add(CONFIGTABLE_SERIALPRINTER_LABELID, typeof(string));

            return emptySerialPrinterTable;
        }

        private DataTable CreateEmptyThermalPrintTable()
        {
            DataTable emptyThermalPrintTable = new DataTable();
            emptyThermalPrintTable.TableName = CONFIGTABLE_THERMALPRINTER;
            emptyThermalPrintTable.Columns.Add(CONFIGTABLE_THERMALPRINTER_X, typeof(int));
            emptyThermalPrintTable.Columns.Add(CONFIGTABLE_THERMALPRINTER_Y, typeof(int));

            return emptyThermalPrintTable;
        }

        private DataTable CreateEmptyLoggingTable()
        {
            DataTable emptyLoggingTable = new DataTable();
            emptyLoggingTable.TableName = CONFIGTABLE_LOGGING;
            emptyLoggingTable.Columns.Add(CONFIGTABLE_LOGGING_UI, typeof(bool));
            emptyLoggingTable.Columns.Add(CONFIGTABLE_LOGGING_MONITORING, typeof(bool));
            emptyLoggingTable.Columns.Add(CONFIGTABLE_LOGGING_FRAME, typeof(bool));
            emptyLoggingTable.Columns.Add(CONFIGTABLE_LOGGING_BIZRULE, typeof(bool));

            DataRow dr = emptyLoggingTable.NewRow();

            dr[CONFIGTABLE_LOGGING_UI] = false;
            dr[CONFIGTABLE_LOGGING_MONITORING] = false;
            dr[CONFIGTABLE_LOGGING_FRAME] = false;
            dr[CONFIGTABLE_LOGGING_BIZRULE] = false;

            emptyLoggingTable.Rows.Add(dr);

            return emptyLoggingTable;
        }

        private DataTable CreateEmptyEtcTable()
        {
            DataTable emptyEtcTable = new DataTable();
            emptyEtcTable.TableName = CONFIGTABLE_ETC;
            emptyEtcTable.Columns.Add(CONFIGTABLE_ETC_NOTICEUSE, typeof(bool));
            emptyEtcTable.Columns.Add(CONFIGTABLE_ETC_SMALLTYPE, typeof(bool));
            emptyEtcTable.Columns.Add(CONFIGTABLE_ETC_WIPSTAT, typeof(string));
            emptyEtcTable.Columns.Add(CONFIGTABLE_ETC_PAPER_SIZE, typeof(string));

            DataRow dr = emptyEtcTable.NewRow();

            dr[CONFIGTABLE_ETC_NOTICEUSE] = false;
            dr[CONFIGTABLE_ETC_SMALLTYPE] = false;

            emptyEtcTable.Rows.Add(dr);

            return emptyEtcTable;
        }

        private DataTable CreateEmptyFormTable()
        {
            DataTable emptyFormTable = new DataTable();
            emptyFormTable.TableName = "FORM";
            emptyFormTable.Columns.Add("EQP_STATUS", typeof(bool));
            emptyFormTable.Columns.Add("W_LOT", typeof(bool));
            emptyFormTable.Columns.Add("PROC_WAIT_LIMIT_TIME_OVER", typeof(bool));
            emptyFormTable.Columns.Add("AGING_LIMIT_TIME_OVER", typeof(bool));
            //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
            emptyFormTable.Columns.Add("AGING_OUTPUT_TIME_OVER", typeof(bool)); // 기존 처리 누락분 추가
            emptyFormTable.Columns.Add("FITTED_DOCV_TRNF_FAIL", typeof(bool));

            return emptyFormTable;
        }
    }
}