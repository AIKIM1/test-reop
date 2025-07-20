using LGC.GMES.MES.Common.ConfigInfos;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Threading;
using SolaceSystems.Solclient.Messaging;

namespace LGC.GMES.MES.Common
{
    public class Variables
    {
        public static bool ISRunTime = false;

        public static string SystemType { get; set; }

        public static string SystemTitle { get; set; }

        /// <summary>
        /// Application Config Information control Object
        /// </summary>
        public static AppConfigInfo appConfigInfo;

        /// <summary>
        /// Middleware Config Information control Object
        /// </summary>
        public static MWConfigInfo mwConfigInfo;

        public static List<UIMessage> msgList = new List<UIMessage>();

        public static Dictionary<string, string> constDic = null;

        /// <summary>
        /// App.Config Local Path
        /// </summary>
        public static string ConfigFilePath { get; set; }

        public static DataTable dtCurretConfigInfo { get; set; }

        public static string CurrentProcID = string.Empty;
        public static string CurrentProcName = string.Empty;
        public static string CurrentEqptID = string.Empty;
        public static string CurrentEqptName = string.Empty;
        public static string CurrentEqptNameFull = string.Empty;
        public static string CurrentMenu = string.Empty; //Not Used
        public static string CurrentMenuID = string.Empty; //Not Used

        public static string CurrentProgramID = string.Empty; //Not Used
        public static string CurrentProgramName = string.Empty; //Not Used
        //public static UCBaseForm CurrentForm = null;
        public static string CurrentFuncMenuID = string.Empty;

        //아래 4개 private -> public 바꿈(20220602)
        public static string aesEncryptKey = "FACTOVA";
        public static string _ConfigFileName = "appSettings.json";
        public static string _ConfigFileSaveAsName = Application.ResourceAssembly.ManifestModule.Name + ".Setting.json";
        public static string _ConfigFileSaveAsName_Back = Application.ResourceAssembly.ManifestModule.Name + ".Setting_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";

        public static String SaveAsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings");
        public static String SaveAsPath_Back = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings\\BK");
        public static String SaveAsSoundFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings\\Sound\\");

        public static String LogPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Logs\\" + SystemTitle + "\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month);

        public static String OKSoundName = "OK_SOUND.wav";
        public static String NGSoundName = "NG_SOUND.wav";
        public static String ModelChangeSoundName = "ModelChange_SOUND.wav";
        public static String AlarmSoundName = "Alarm_SOUND.wav";

        public static String SaveAsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings", _ConfigFileName);
        public static String SaveAsFilePath_Back = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings\\BK", _ConfigFileSaveAsName_Back);
        //public static String SaveAsDevFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings", Application.ResourceAssembly.ManifestModule.Name + ".Dev.config");


        public static int mwConnectionStatus { get; set; }


        private static void onSessionEvent(object sender, SessionEventArgs e)
        {
            //Debug.WriteLine( "Solace Session Status : " + e.ResponseCode.ToString());

            if (Variables.appConfigInfo != null)
            {
                if (((SDKErrorSubcode)e.ResponseCode).ToString() != "Ok")
                {
                    //Logger.Instance.WriteLine(LogLevel.EVENT, "Solace Connection Status : ", ((SDKErrorSubcode)e.ResponseCode).ToString());
                    //Logger.GetInstance().WriteLog(LogLevel.EVENT, "Solace Connection Info : ", e.Info);
                    Logger.Instance.WriteLine("Event Log", "Solace Connection Status : "+ ((SDKErrorSubcode)e.ResponseCode).ToString(), LogCategory.FRAME );
                    Logger.Instance.WriteLine("Event Log", "Solace Connection Info : + e.Info ", LogCategory.FRAME);
                }
            }

            Variables.mwConnectionStatus = e.ResponseCode;
        }

        public static void getConstData()
        {
            try
            {
                constDic = new Dictionary<string, string>();

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("SYSTEM_ID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_COR_GET_CONST_BY_FACTORY_CODE", "RQSTDT", "RSLTDT", dt);

                if (dtResult.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if(dtResult.Rows[i]["USE_FLAG"].ToString() == "Y")
                            constDic.Add(dtResult.Rows[i]["CONSTANT_ID"].ToString(), dtResult.Rows[i]["CONSTANT_VALUE"].ToString());
                    }
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        public static void SetLogLocation()
        {
            try
            {
                SaveAsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings" + "\\" + SystemTitle);
                SaveAsSoundFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings\\Sound\\" + SystemTitle + "\\");
                SaveAsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings" + "\\" + SystemTitle, _ConfigFileSaveAsName);
                SaveAsFilePath_Back = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Settings\\BK\\" + _ConfigFileSaveAsName_Back);

                string DelLogPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FactovaMES\\SFC\\Logs" + "\\" + SystemTitle);

                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }

                //Delete logfiles that created before 15 days.
                if (Directory.Exists(DelLogPath))
                {
                    DirectoryInfo delDir = new DirectoryInfo(DelLogPath);

                    //Delete files
                    foreach (System.IO.FileInfo file in delDir.GetFiles("*.log", SearchOption.AllDirectories))
                    {
                        if (file.LastWriteTime < DateTime.Now.Date.AddDays(-15))
                        {
                            file.Delete();
                        }
                    }

                    //Directory Delete
                    foreach (DirectoryInfo directory in delDir.GetDirectories())
                    {
                        foreach (DirectoryInfo subDir in directory.GetDirectories())
                        {
                            if (subDir.GetFiles().Count() == 0 && subDir.FullName != LogPath)
                            {
                                subDir.Delete();
                            }
                        }
                    }
                }
            }
            catch
            {
            }

        }

        

        /// <summary>
        /// Get App.'s Configuration Info from App.config File.
        /// </summary>
        public static bool GetAppConfigInfos(bool isnewBizInfo = false, bool? isDevMode = null)
        {
            try
            {
                Variables.ConfigFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, _ConfigFileName);
                //FACTOVA.SFC.MainFrame.exe.Setting.json 파일 내용 조회
                
                if (File.Exists(SaveAsFilePath))
                {
                    string saveJsonconfigInfo = File.ReadAllText(SaveAsFilePath);
                    //FACTOVA.SFC.MainFrame.exe.Setting.json 파일 존재 + 안에 내용이 있는 경우만 appSetting.json으로 복사
                    if (!string.IsNullOrWhiteSpace(saveJsonconfigInfo))
                    {
                        File.Copy(SaveAsFilePath, System.IO.Path.Combine(Environment.CurrentDirectory, _ConfigFileName), true);
                        //Logger.GetInstance().WriteLog(LogLevel.EVENT, " JsonFile Text : ", "Use FACTOVA.SFC.MainFrame.exe.Setting.json");
                    }
                    //FACTOVA.SFC.MainFrame.exe.Setting.json 파일 존재 + 내용이 빈값인 경우
                    else
                    {
                        //Logger.GetInstance().WriteLog(LogLevel.EVENT, " JsonFile Text is empty : ", "Use default C:appSetting");
                    }
                }
              
                string jsonConfigInfo = File.ReadAllText(ConfigFilePath);
                JObject configObject = JObject.Parse(jsonConfigInfo);

                try 
                {
                    if (configObject["MWCONFIG_INFO"] != null)
                    {
                        mwConfigInfo = new MWConfigInfo();

                        Type mwConfigType = typeof(MWConfigInfo);

                        foreach (var propertyInfo in mwConfigType.GetProperties())
                        {
                            if (configObject["MWCONFIG_INFO"][propertyInfo.Name] != null)
                            {
                                if (propertyInfo.Name == "USERNAME" || propertyInfo.Name == "PASSWORD")
                                {
                                    string decryptData = CommonFnc.AESDecrypt(configObject["MWCONFIG_INFO"][propertyInfo.Name].Value<String>(), aesEncryptKey);
                                    propertyInfo.SetValue(mwConfigInfo, decryptData, null);
                                }
                                else
                                {
                                    propertyInfo.SetValue(mwConfigInfo, configObject["MWCONFIG_INFO"][propertyInfo.Name].Value<String>(), null);
                                }
                            }
                        }

                        ClientProxyMom.initialize(onSessionEvent);
                    }

                    if (configObject["APPCONFIG_INFO"] != null)
                    {
                        appConfigInfo = new AppConfigInfo();

                        Type appConfigType = typeof(AppConfigInfo);

                        foreach (var propertyInfo in appConfigType.GetProperties())
                        {
                            if (configObject["APPCONFIG_INFO"][propertyInfo.Name] != null)
                            {
                                if (propertyInfo.PropertyType == typeof(String))
                                {
                                    if (!String.IsNullOrEmpty(configObject["APPCONFIG_INFO"][propertyInfo.Name].Value<String>()))
                                    {
                                        propertyInfo.SetValue(appConfigInfo, configObject["APPCONFIG_INFO"][propertyInfo.Name].Value<String>(), null);
                                    }
                                }
                                else
                                {
                                    if (propertyInfo.Name == "SCANNER_INFOS")
                                    {
                                        Type scanConfigType = typeof(ScanConfigInfo);

                                        List<ScanConfigInfo> scanner_infos = new List<ScanConfigInfo>();
                                        foreach (var scannerItem in configObject["APPCONFIG_INFO"][propertyInfo.Name])
                                        {
                                            ScanConfigInfo scanConfigInfo = new ScanConfigInfo();

                                            foreach (var scannPropertyInfo in scanConfigType.GetProperties())
                                            {
                                                if (scannerItem[scannPropertyInfo.Name] != null)
                                                {
                                                    if (!String.IsNullOrEmpty(scannerItem[scannPropertyInfo.Name].Value<String>()))
                                                    {
                                                        scannPropertyInfo.SetValue(scanConfigInfo, scannerItem[scannPropertyInfo.Name].Value<String>(), null);
                                                    }
                                                }
                                            }

                                            scanner_infos.Add(scanConfigInfo);
                                        }

                                        appConfigInfo.SCANNER_INFOS = scanner_infos;
                                    }
                                    else if (propertyInfo.Name == "PRINTER_INFOS")
                                    {
                                        Type printerConfigType = typeof(PrinterConfigInfo);

                                        List<PrinterConfigInfo> printer_infos = new List<PrinterConfigInfo>();
                                        foreach (var printerItem in configObject["APPCONFIG_INFO"][propertyInfo.Name])
                                        {
                                            PrinterConfigInfo printerConfigInfo = new PrinterConfigInfo();

                                            foreach (var printerPropertyInfo in printerConfigType.GetProperties())
                                            {
                                                if (printerItem[printerPropertyInfo.Name] != null)
                                                {
                                                    if (!String.IsNullOrEmpty(printerItem[printerPropertyInfo.Name].Value<String>()))
                                                    {
                                                        printerPropertyInfo.SetValue(printerConfigInfo, printerItem[printerPropertyInfo.Name].Value<String>(), null);
                                                    }
                                                }
                                            }

                                            printer_infos.Add(printerConfigInfo);
                                        }

                                        appConfigInfo.PRINTER_INFOS = printer_infos;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                { 
                }

                



                //configuration = ConfigurationManager.OpenExeConfiguration(Variables.ConfigFilePath);

                //AppSection ApplicationSec = new AppSection();
                //AppSection bizActorInfoSec = new AppSection();
                //AppSection bizActorDevInfoSec = new AppSection();
                //AppSection bizActorInfoSec2 = new AppSection();
                //AppSection bizActorDevInfoSec2 = new AppSection();
                //AppSection SerialLstInfoSec = new AppSection();

                //if (configuration.Sections[AppConfigInfoSectionName] == null)
                //{
                //    ApplicationSec.AppInfoElement.DefaultApp = "";
                //    ApplicationSec.AppInfoElement.MessageYN = "Y";
                //    ApplicationSec.AppInfoElement.MsgAutoClearYN = "Y";
                //    ApplicationSec.AppInfoElement.MsgAutoClearInterval = "3000";

                //    //ACK Check interval information
                //    ApplicationSec.AppInfoElement.AckCheck = "N";
                //    ApplicationSec.AppInfoElement.AckCheckInterval = "3000";
                //    //Sound Use YN 
                //    ApplicationSec.AppInfoElement.SoundUseYN = "N";

                //    ApplicationSec.AppInfoElement.FuncAlignment = "Right";
                //    ApplicationSec.AppInfoElement.Organizations = "";
                //    ApplicationSec.AppInfoElement.Shop = "";
                //    ApplicationSec.AppInfoElement.Area = "";
                //    ApplicationSec.AppInfoElement.Line = "";
                //    ApplicationSec.AppInfoElement.LineDesc = "";
                //    ApplicationSec.AppInfoElement.IsAutoLogIn = "False";
                //    ApplicationSec.AppInfoElement.IsFLUse = "False";
                //    ApplicationSec.AppInfoElement.FLConnectionString = string.Empty;
                //    ApplicationSec.AppInfoElement.IsDevMode = (isDevMode ?? false).ToString();
                //    ApplicationSec.AppInfoElement.IsDataReceive = "False";
                //    ApplicationSec.AppInfoElement.DataReceiveIP = "";
                //    ApplicationSec.AppInfoElement.IsDataSend = "False";
                //    ApplicationSec.AppInfoElement.DataSendTarget = "127.0.0.1";
                //    ApplicationSec.AppInfoElement.IsGUITopPrority = "False";
                //    ApplicationSec.AppInfoElement.SpecialChars = "";

                //    ApplicationSec.AppInfoElement.IsEnableWorkGuide = "False";
                //    ApplicationSec.AppInfoElement.WorkGuideType = "Right";
                //    ApplicationSec.AppInfoElement.WorkGuideMargin = "0,0,0,0";

                //    configuration.Sections.Add(AppConfigInfoSectionName, ApplicationSec);
                //    configuration.Save(ConfigurationSaveMode.Modified);

                //    ConfigurationManager.RefreshSection(AppConfigInfoSectionName);
                //}
                //else
                //{
                //    ApplicationSec = (AppSection)configuration.GetSection(AppConfigInfoSectionName);

                //    if (isDevMode != null)
                //    {
                //        ApplicationSec.AppInfoElement.IsDevMode = isDevMode.ToString();
                //    }
                //    else
                //    {
                //        ApplicationSec.AppInfoElement.IsDevMode = "False";
                //    }

                //    appConfigInfo = ApplicationSec.AppInfoElement;
                //}

                //if (ApplicationSec.AppInfoElement.IsFLUse == "True")
                //{
                //    //Factory Lync Connection and Open;
                //    if (ApplicationSec.AppInfoElement.FLConnectionString != string.Empty)
                //    {
                //        Variables.fLConnector.ConnectionInfoString = ApplicationSec.AppInfoElement.FLConnectionString;

                //        if (!Variables.fLConnector.Connected)
                //        {
                //            Variables.fLConnector.Open();
                //        }
                //    }
                //}

                //labelPrinter.Initialze();

                //if (ApplicationSec.AppInfoElement.IsDevMode == "True")
                //{
                //    //BizActor Dev Connection Infod
                //    if (configuration.Sections[bizDevInfoSectionName] == null)
                //    {
                //        bizActorDevInfoSec.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorDevInfoSec.BizInfoElement.ServicePort = "7865";
                //        bizActorDevInfoSec.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorDevInfoSec.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorDevInfoSec.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizDevInfoSectionName, bizActorInfoSec);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizDevInfoSectionName);
                //    }
                //    else if (isnewBizInfo)
                //    {
                //        bizActorDevInfoSec.BizInfoElement.ServiceIP = newbizActorInfo.ServiceIP;
                //        bizActorDevInfoSec.BizInfoElement.ServicePort = newbizActorInfo.ServicePort;
                //        bizActorDevInfoSec.BizInfoElement.ServiceMode = newbizActorInfo.ServiceMode;
                //        bizActorDevInfoSec.BizInfoElement.ServiceProtocal = newbizActorInfo.ServiceProtocal;
                //        bizActorDevInfoSec.BizInfoElement.ServiceIndex = newbizActorInfo.ServiceIndex;

                //        configuration.Sections.Remove(bizDevInfoSectionName);
                //        configuration.Sections.Add(bizDevInfoSectionName, bizActorDevInfoSec);
                //        configuration.Save(ConfigurationSaveMode.Modified);
                //    }
                //    else
                //    {
                //        // stored in the configuration file.
                //        bizActorDevInfoSec = (AppSection)configuration.GetSection(bizDevInfoSectionName);
                //        CurbizActorInfo = bizActorDevInfoSec.BizInfoElement;
                //    }

                //    if (configuration.Sections[bizDevInfoSectionName2] == null)
                //    {
                //        bizActorDevInfoSec2.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorDevInfoSec2.BizInfoElement.ServicePort = "7865";
                //        bizActorDevInfoSec2.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorDevInfoSec2.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorDevInfoSec2.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizDevInfoSectionName, bizActorInfoSec2);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizDevInfoSectionName2);
                //    }
                //    else if (isnewBizInfo)
                //    {
                //        bizActorDevInfoSec2.BizInfoElement.ServiceIP = newbizActorInfo2.ServiceIP;
                //        bizActorDevInfoSec2.BizInfoElement.ServicePort = newbizActorInfo2.ServicePort;
                //        bizActorDevInfoSec2.BizInfoElement.ServiceMode = newbizActorInfo2.ServiceMode;
                //        bizActorDevInfoSec2.BizInfoElement.ServiceProtocal = newbizActorInfo2.ServiceProtocal;
                //        bizActorDevInfoSec2.BizInfoElement.ServiceIndex = newbizActorInfo2.ServiceIndex;

                //        configuration.Sections.Remove(bizDevInfoSectionName2);
                //        configuration.Sections.Add(bizDevInfoSectionName2, bizActorDevInfoSec2);
                //        configuration.Save(ConfigurationSaveMode.Modified);
                //    }


                //    //BizActor Connection Infod
                //    if (configuration.Sections[bizInfoSectionName] == null)
                //    {
                //        bizActorInfoSec.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorInfoSec.BizInfoElement.ServicePort = "7865";
                //        bizActorInfoSec.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorInfoSec.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorInfoSec.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizInfoSectionName, bizActorInfoSec);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizInfoSectionName);
                //    }

                //    if (configuration.Sections[bizInfoSectionName2] == null)
                //    {
                //        bizActorInfoSec2.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorInfoSec2.BizInfoElement.ServicePort = "7865";
                //        bizActorInfoSec2.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorInfoSec2.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorInfoSec2.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizInfoSectionName2, bizActorInfoSec2);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizInfoSectionName2);
                //    }
                //}
                //else
                //{
                //    //BizActor Connection Infod
                //    if (configuration.Sections[bizInfoSectionName] == null)
                //    {
                //        bizActorInfoSec.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorInfoSec.BizInfoElement.ServicePort = "7865";
                //        bizActorInfoSec.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorInfoSec.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorInfoSec.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizInfoSectionName, bizActorInfoSec);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizInfoSectionName);
                //    }
                //    else if (isnewBizInfo)
                //    {
                //        bizActorInfoSec.BizInfoElement.ServiceIP = newbizActorInfo.ServiceIP;
                //        bizActorInfoSec.BizInfoElement.ServicePort = newbizActorInfo.ServicePort;
                //        bizActorInfoSec.BizInfoElement.ServiceMode = newbizActorInfo.ServiceMode;
                //        bizActorInfoSec.BizInfoElement.ServiceProtocal = newbizActorInfo.ServiceProtocal;
                //        bizActorInfoSec.BizInfoElement.ServiceIndex = newbizActorInfo.ServiceIndex;

                //        configuration.Sections.Remove(bizInfoSectionName);
                //        configuration.Sections.Add(bizInfoSectionName, bizActorInfoSec);
                //        configuration.Save(ConfigurationSaveMode.Modified);
                //    }
                //    else
                //    {
                //        // stored in the configuration file.
                //        bizActorInfoSec = (AppSection)configuration.GetSection(bizInfoSectionName);
                //        CurbizActorInfo = bizActorInfoSec.BizInfoElement;
                //    }

                //    if (configuration.Sections[bizInfoSectionName2] == null)
                //    {
                //        bizActorInfoSec2.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorInfoSec2.BizInfoElement.ServicePort = "7865";
                //        bizActorInfoSec2.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorInfoSec2.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorInfoSec2.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizInfoSectionName2, bizActorInfoSec2);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizInfoSectionName2);
                //    }
                //    else if (isnewBizInfo)
                //    {
                //        bizActorInfoSec2.BizInfoElement.ServiceIP = newbizActorInfo2.ServiceIP;
                //        bizActorInfoSec2.BizInfoElement.ServicePort = newbizActorInfo2.ServicePort;
                //        bizActorInfoSec2.BizInfoElement.ServiceMode = newbizActorInfo2.ServiceMode;
                //        bizActorInfoSec2.BizInfoElement.ServiceProtocal = newbizActorInfo2.ServiceProtocal;
                //        bizActorInfoSec2.BizInfoElement.ServiceIndex = newbizActorInfo2.ServiceIndex;

                //        configuration.Sections.Remove(bizInfoSectionName2);
                //        configuration.Sections.Add(bizInfoSectionName2, bizActorInfoSec2);
                //        configuration.Save(ConfigurationSaveMode.Modified);
                //    }

                //    //BizActor Dev Connection Info
                //    if (configuration.Sections[bizDevInfoSectionName] == null)
                //    {
                //        bizActorDevInfoSec.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorDevInfoSec.BizInfoElement.ServicePort = "7865";
                //        bizActorDevInfoSec.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorDevInfoSec.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorDevInfoSec.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizDevInfoSectionName, bizActorDevInfoSec);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizDevInfoSectionName);
                //    }

                //    if (configuration.Sections[bizDevInfoSectionName2] == null)
                //    {
                //        bizActorDevInfoSec2.BizInfoElement.ServiceIP = "127.0.0.1";
                //        bizActorDevInfoSec2.BizInfoElement.ServicePort = "7865";
                //        bizActorDevInfoSec2.BizInfoElement.ServiceMode = "SERVICE";
                //        bizActorDevInfoSec2.BizInfoElement.ServiceProtocal = "tcp";
                //        bizActorDevInfoSec2.BizInfoElement.ServiceIndex = "1";

                //        configuration.Sections.Add(bizDevInfoSectionName2, bizActorDevInfoSec2);
                //        configuration.Save(ConfigurationSaveMode.Modified);

                //        ConfigurationManager.RefreshSection(bizDevInfoSectionName2);
                //    }
                //}

                ////SerialPort List Info
                //if (configuration.Sections[SerialPortSectionName] == null)
                //{
                //    SerialLstInfoSec.SerialListInfo.Add(new SerialPortInfo() { ComPortName = "COM1", ComPortIndex = "1", BaudRate = "9600" });

                //    configuration.Sections.Add(SerialPortSectionName, SerialLstInfoSec);
                //    configuration.Save(ConfigurationSaveMode.Modified);

                //    ConfigurationManager.RefreshSection(SerialPortSectionName);
                //}
                //else
                //{
                //    SerialLstInfoSec = (AppSection)configuration.GetSection(SerialPortSectionName);
                //    serialPortInfos = SerialLstInfoSec.SerialListInfo;
                //}

                //string strProtocol = CurbizActorInfo.ServiceProtocal;
                //string strServerName = CurbizActorInfo.ServiceIP;
                //string strServerPort = CurbizActorInfo.ServicePort;
                //string strMode = CurbizActorInfo.ServiceMode;
                //string strServiceIndex = CurbizActorInfo.ServiceIndex;

                //BizActorService = new BizService(strServerName, strProtocol, strServerPort, strMode, strServiceIndex);

                //if (!BizActorService.ConnectBizActor())
                //{
                //    MessageBox.Show("Server connection Error!\r\nPlease check the network!", MsgType.Warnning.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return false;
                //}

                //if (isnewBizInfo)
                //{
                //    SaveAs();
                //}

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        public static void SaveMWConfigInfo(MWConfigInfo mWConfigInfo)
        {
            try
            {
                string jsonString = File.ReadAllText(Variables.ConfigFilePath);
                JObject jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString) as JObject;
                JToken jHOST = jObject.SelectToken("MWCONFIG_INFO.HOST");
                jHOST.Replace(mWConfigInfo.HOST);
                JToken jVPNNAME = jObject.SelectToken("MWCONFIG_INFO.VPNNAME");
                jVPNNAME.Replace(mWConfigInfo.VPNNAME);
                JToken jUSERNAME = jObject.SelectToken("MWCONFIG_INFO.USERNAME");
                jUSERNAME.Replace(CommonFnc.AESEncrypt(mWConfigInfo.USERNAME,aesEncryptKey));
                JToken jPASSWORD = jObject.SelectToken("MWCONFIG_INFO.PASSWORD");
                jPASSWORD.Replace(CommonFnc.AESEncrypt(mWConfigInfo.PASSWORD, aesEncryptKey));
                JToken jPROPERTIES = jObject.SelectToken("MWCONFIG_INFO.PROPERTIES");
                jPROPERTIES.Replace(mWConfigInfo.PROPERTIES);
                JToken jSQL_QUEUE = jObject.SelectToken("MWCONFIG_INFO.SQL_QUEUE");
                jSQL_QUEUE.Replace(mWConfigInfo.SQL_QUEUE);
                JToken jMRS_QUEUE = jObject.SelectToken("MWCONFIG_INFO.MRS_QUEUE");
                jMRS_QUEUE.Replace(mWConfigInfo.MRS_QUEUE);
                JToken CONNECTION_MODE = jObject.SelectToken("MWCONFIG_INFO.CONNECTION_MODE");
                if (CONNECTION_MODE == null)
                {
                    (jObject["MWCONFIG_INFO"] as JObject).Add("CONNECTION_MODE", mWConfigInfo.CONNECTION_MODE);
                }
                else
                {
                    CONNECTION_MODE.Replace(mWConfigInfo.CONNECTION_MODE);
                }


                Type mwConfigType = typeof(MWConfigInfo);

                foreach (var propertyInfo in mwConfigType.GetProperties())
                {
                    JToken jItem = jObject["MWCONFIG_INFO"].SelectToken(propertyInfo.Name);

                    string value = (propertyInfo.GetValue(mWConfigInfo) ?? "").ToString();

                    if (propertyInfo.Name == "USERNAME" || propertyInfo.Name == "PASSWORD")
                    {
                        value = CommonFnc.AESEncrypt(value, aesEncryptKey);
                    }

                    if (jItem != null)
                    {
                        jItem.Replace(value);
                    }
                    else
                    {
                        (jObject["MWCONFIG_INFO"] as JObject).Add(propertyInfo.Name, value);
                    }
                }


                string updateConfig = jObject.ToString();
                File.WriteAllText(Variables.ConfigFilePath, updateConfig);

                SaveAs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void SaveAppConfigInfo(AppConfigInfo appConfigInfo = null)
        {
            try
            {
                if (appConfigInfo == null)
                {
                    appConfigInfo = Variables.appConfigInfo;
                }
                string jsonString = File.ReadAllText(Variables.ConfigFilePath);
                JObject jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString) as JObject;

                Type appConfigType = typeof(AppConfigInfo);

                foreach (var propertyInfo in appConfigType.GetProperties())
                {
                    JToken jItem = jObject["APPCONFIG_INFO"].SelectToken(propertyInfo.Name);

                    if (propertyInfo.PropertyType == typeof(String))
                    {
                        //APPCONFIG_INFO 안에 IS_TESTMODE의 경우 강제로 N으로 저장. 현재 실행중인 Variables.appConfigInfo.IS_TESTMODE 와 다름 주의.(2022.06.02)
                        if (jItem != null)
                        {
                            jItem.Replace((propertyInfo.Name == "IS_TESTMODE" ? "N" : (propertyInfo.GetValue(appConfigInfo) ?? "").ToString()).ToString());
                        }
                        else
                        {
                            (jObject["APPCONFIG_INFO"] as JObject).Add(propertyInfo.Name, (propertyInfo.Name == "IS_TESTMODE" ? "N" : (propertyInfo.GetValue(appConfigInfo) ?? "").ToString()).ToString());
                         
                        }
                    }
                    else
                    {
                        if (propertyInfo.Name == "SCANNER_INFOS")
                        {
                            Type scanConfigType = typeof(ScanConfigInfo);

                            JArray jArray = new JArray();
                            foreach (ScanConfigInfo scanConfigInfo in appConfigInfo.SCANNER_INFOS)
                            {
                                JObject item = new JObject();

                                foreach (var scannPropertyInfo in scanConfigType.GetProperties())
                                {
                                    item.Add(scannPropertyInfo.Name, (scannPropertyInfo.GetValue(scanConfigInfo) ?? "").ToString());
                                }

                                jArray.Add(item);
                            }

                            if (jItem == null)
                            {
                                (jObject["APPCONFIG_INFO"] as JObject).Add(propertyInfo.Name, jArray);
                            }
                            else
                            {
                                jItem.Replace(jArray);
                            }
                        }

                        if (propertyInfo.Name == "PRINTER_INFOS")
                        {
                            Type printerConfigType = typeof(PrinterConfigInfo);

                            JArray jArray = new JArray();
                            foreach (PrinterConfigInfo printerConfigInfo in appConfigInfo.PRINTER_INFOS)
                            {
                                JObject item = new JObject();

                                foreach (var printerPropertyInfo in printerConfigType.GetProperties())
                                {
                                    item.Add(printerPropertyInfo.Name, (printerPropertyInfo.GetValue(printerConfigInfo) ?? "").ToString());
                                }

                                jArray.Add(item);
                            }

                            if (jItem == null)
                            {
                               (jObject["APPCONFIG_INFO"] as JObject).Add(propertyInfo.Name, jArray);
                            }
                            else
                            {
                                jItem.Replace(jArray);
                            }
                        }
                    }
                }
                
                string updateConfig = jObject.ToString();
                File.WriteAllText(Variables.ConfigFilePath, updateConfig);

                SaveAs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MsgType.Error.ToMsgString(), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static bool SaveAs()
        {
            try
            {
                string configFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, _ConfigFileName);

                if (!Directory.Exists(SaveAsPath))
                {
                    Directory.CreateDirectory(SaveAsPath);
                }

                File.Copy(configFilePath, SaveAsFilePath, true);
                //File.Copy(configFilePath, SaveAsFilePath_Back, true);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SaveAsBack(bool isOnlyDBSave = false)
        {
            try
            {
                string configFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, _ConfigFileName);

                if (!Directory.Exists(SaveAsPath_Back))
                {
                    Directory.CreateDirectory(SaveAsPath_Back);
                }

                //File.Copy(configFilePath, SaveAsFilePath, true);
                if (isOnlyDBSave == false)
                {
                    //Save 저장버튼 누르는경우만 내문서쪽 백업 폴더에 컨피그 저장함(20220602).
                    File.Copy(configFilePath, SaveAsFilePath_Back, true);
                    //Logger.GetInstance().WriteLog(LogLevel.EVENT, " SavedJsonFile Info : ", File.ReadAllText(SaveAsFilePath_Back));
                    Logger.Instance.WriteLine("Evnet Log", " SavedJsonFile Info : "+ File.ReadAllText(SaveAsFilePath_Back),LogCategory.FRAME );
                }

                //SFC CLOSE &SAVE버튼 누를시 Bizactor 저장로직 구현(20220602).
                string ConfigFilePath_Main = System.IO.Path.Combine(Environment.CurrentDirectory, Variables._ConfigFileName);
                string jsonConfigInfo = File.ReadAllText(ConfigFilePath_Main);
                DataSet InDataMainframe = new DataSet();

                string sSFC_MODE = string.Empty;
                string sTRANSACTION_TYPE_CODE = string.Empty;

                if (!string.IsNullOrEmpty(Variables.appConfigInfo.IS_TESTMODE)
                    && Variables.appConfigInfo.IS_TESTMODE.Equals("Y")
                    )
                {
                    sSFC_MODE = "TEST"; // Y가 TEST 모드
                }
                else
                {
                    sSFC_MODE = "PROD"; // 빈값(그럴리 없지만) or N은 운영 모드
                }

                //isOnlyDBSave == false 면 SAVE 임. true는 Window CLOSE
                if (isOnlyDBSave == false)
                {
                    sTRANSACTION_TYPE_CODE = "SAVE";
                }
                else
                {
                    sTRANSACTION_TYPE_CODE = "LOGOUT";
                }

                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "PC_IP_ADDR", CommonFnc.Get_LocalIP());                  //PC아이피주소
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "SFC_MODE", sSFC_MODE);                                  //SFC모드 ( TEST / PROD )
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "TRANSACTION_TYPE_CODE", sTRANSACTION_TYPE_CODE);        //트랜잭션유형코드 ( LOGIN / LOGOUT / SAVE )
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "CONFIG_JSON", jsonConfigInfo.ToString());               //설정정보 JSON
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "CONFIG_REGISTER_YMD", null);                            //설정등록년월일
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "USE_FLAG", "Y");                                        //사용여부
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "CREATION_USER_ID", LoginInfo.USERID);                   //생성사용자아이디
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "CREATION_DATE", null);                                  //생성일자
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "LAST_UPDATE_USER_ID", LoginInfo.USERID);               //최종수정사용자아이디
                CommonFnc.MakeDataTable(ref InDataMainframe, "IN_DATA", "LAST_UPDATE_DATE", null);                               //최종수정일자
                InDataMainframe.AcceptChanges();

                ClientProxyMom.ExecuteServiceSync("BR_SFC_REG_MAINFRAME_CONFIG", "IN_DATA", null, InDataMainframe, ProcQueueType.BIZ);
                //SFC CLOSE &SAVE버튼 누를시 Bizactor 저장로직 구현(20220602). 끝


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SaveAs(string targetPath)
        {
            try
            {
                string configFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, Application.ResourceAssembly.ManifestModule.Name);

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                File.Copy(configFilePath + ".json", System.IO.Path.Combine(targetPath, Application.ResourceAssembly.ManifestModule.Name + ".Setting.json"), true);


                //Sound File Export
                String targetSoundPath = targetPath + "\\Sound";
                if (!Directory.Exists(targetSoundPath))
                {
                    Directory.CreateDirectory(targetSoundPath);
                }
                FileInfo _fiileOk = new FileInfo(Variables.SaveAsSoundFilePath + Variables.OKSoundName);
                if (_fiileOk.Exists)
                {
                    File.Copy(Variables.SaveAsSoundFilePath + Variables.OKSoundName, Path.Combine(targetSoundPath, Variables.OKSoundName), true);
                }
                FileInfo _fiileNg = new FileInfo(Variables.SaveAsSoundFilePath + Variables.NGSoundName);
                if (_fiileNg.Exists)
                {
                    File.Copy(Variables.SaveAsSoundFilePath + Variables.NGSoundName, Path.Combine(targetSoundPath, Variables.NGSoundName), true);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void AddUIMessage(UIMessage message)
        {
            Thread thr_msg = new Thread(saveUIMsg);
            thr_msg.Start(message);
        }

        private static void saveUIMsg(object message)
        {
            try
            {
                List<UIMessage> lstremoveMsg = msgList.Where(item => item.msgDateTime < DateTime.Now.AddDays(-1).Date).OrderByDescending(item => item.msgDateTime).ToList();

                if (lstremoveMsg.Count > 0)
                {
                    var idx = msgList.IndexOf(lstremoveMsg.Last());
                    msgList.RemoveRange(idx, lstremoveMsg.Count);
                }

                msgList.Add((UIMessage)message);
            }
            catch (System.Exception ex)
            {
                
            }
        }

    }

    public class BaudRateEnums : List<string>
    {
        public BaudRateEnums()
        {
            this.Add("115200");
            this.Add("57600");
            this.Add("38400");
            this.Add("19200");
            this.Add("9600");
            this.Add("4800");
            this.Add("2400");
        }
    }

    public class ScannerTypeEnums : List<string>
    {
        public ScannerTypeEnums()
        {
            this.Add("ComPort");
            this.Add("TCP");
            this.Add("Keyboard");
        }
    }

    public class ParityEnums : List<string>
    {
        public ParityEnums()
        {
            this.Add("Even");
            this.Add("Mark");
            this.Add("None");
            this.Add("Odd");
            this.Add("Space");
        }

        public static Parity ConvertToParity(string ParityName)
        {
            switch (ParityName)
            {
                case "Even":
                    return Parity.Even;
                case "Mark":
                    return Parity.Mark;
                case "None":
                    return Parity.None;
                case "Odd":
                    return Parity.Odd;
                case "Space":
                    return Parity.Space;
                default:
                    return Parity.None;
            }
        }
    }

    public class StopBitsEnums : List<string>
    {
        public StopBitsEnums()
        {
            this.Add("One");
            this.Add("OnePointFive");
            this.Add("Two");
            //this.Add("None");
        }

        public static StopBits ConvertToStopBits(string StopBitsName)
        {
            switch (StopBitsName)
            {
                case "One":
                    return StopBits.One;
                case "OnePointFive":
                    return StopBits.One;
                case "Two":
                    return StopBits.Two;
                //case "None":
                //    return StopBits.None;
                default:
                    return StopBits.One;
            }
        }
    }

    public class UIMessage
    {
        public DateTime msgDateTime { get; set; }

        public String Message { get; set; }

        public string ProgramID { get; set; }

        public string ProgramName { get; set; }

    }
}
