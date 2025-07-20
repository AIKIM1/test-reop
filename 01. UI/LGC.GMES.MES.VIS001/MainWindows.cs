using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Collections;

using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.VIS001
{
    public partial class MainWindows : Form
    {
        #region Declaration & Constructor 

        FileSystemWatcher fsw = new FileSystemWatcher();

        public MainWindows()
        {
            try
            {
                InitializeComponent();
                LGC.GMES.MES.Common.LoginInfo.SYSID = "GMES-E-KR";
                this.Load += MainWindows_Load;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void MainWindows_Load(object sender, EventArgs e)
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            try
            {
                GetConfig();

                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Visible = false;
                this.nifIcon.Visible = true;

                SetFileWatcher();

                switch(LGC.GMES.MES.VIS001.Config.SeparationType)
                {
                    case 0:
                        Config.DivChr = ',';
                        break;
                    case 1:
                        Config.DivChr = ';';
                        break;
                    case 2:
                        Config.DivChr = (char)(32);
                        break;
                    case 3:
                        Config.DivChr = '\t';
                        break;
                }

                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Program Start");
                IconRunCheck("START");
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        #endregion

        #region Event
        private void mnuStart_Click(object sender, EventArgs e)
        {
            try
            {
                IconRunCheck("START");
                fsw.EnableRaisingEvents = true; //'파일생성 이벤트 시작
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void mnuStop_Click(object sender, EventArgs e)
        {
            try
            {
                IconRunCheck("STOP");
                fsw.EnableRaisingEvents = false; //'파일생성 이벤트 종료
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void mnuRun_Click(object sender, EventArgs e)
        {
            try
            {
                IconRunCheck("RUN");
                RunUpload();
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void mnuLog_Click(object sender, EventArgs e)
        {
            try
            {
                dlgLog dlgLog = new dlgLog();
                dlgLog.ShowDialog();
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void mnuConfig_Click(object sender, EventArgs e)
        {
            try
            {
                dlgConfig dlgConfig = new dlgConfig();
                dlgConfig.ShowDialog();
                SetConfig();
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void mnuEnd_Click(object sender, EventArgs e)
        {
            try
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Program End");
                this.nifIcon.Visible = false;
                Application.Exit();
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void WatchOnCreated(object sender, FileSystemEventArgs e)
        {
            fsw = sender as FileSystemWatcher;
            try
            {
                fsw.EnableRaisingEvents = false; //'파일생성 감시 이벤트 종료
                System.Threading.Thread.Sleep(1000 * Config.SleepWatchOn);
                if (this.mnuRun.Checked == true)
                {
                    //만약 DB에 데이터를 쓰고 있다면, 그냥 빠져나와라 (insert 중복방지)
                    return;
                }
                IconRunCheck("RUN");
                RunUpload();
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
            finally
            {
                fsw.EnableRaisingEvents = true; //'파일생성 감시 이벤트 시작
            }
        }

        #endregion

        #region Mehod

        private void GetConfig()
        {
            try
            {
                LGC.GMES.MES.VIS001.Config.ShopCode = ConfigurationManager.AppSettings["ShopCode"];
                LGC.GMES.MES.VIS001.Config.AreaCode = ConfigurationManager.AppSettings["AreaCode"];

                LGC.GMES.MES.VIS001.Config.FTPUsed = Convert.ToBoolean(ConfigurationManager.AppSettings["FTPUsed"]);
                LGC.GMES.MES.VIS001.Config.FTP = ConfigurationManager.AppSettings["FTP"];
                LGC.GMES.MES.VIS001.Config.FTPID = ConfigurationManager.AppSettings["FTPID"];
                LGC.GMES.MES.VIS001.Config.FTPPassword = ConfigurationManager.AppSettings["FTPPassword"];

                LGC.GMES.MES.VIS001.Config.BizActorIP = ConfigurationManager.AppSettings[LoginInfo.SYSID + "_" + "BizActorIP"];
                LGC.GMES.MES.VIS001.Config.BizActorServiceIndex = ConfigurationManager.AppSettings[LoginInfo.SYSID + "_" + "BizActorServiceIndex"];
                LGC.GMES.MES.VIS001.Config.BizActorPort = ConfigurationManager.AppSettings[LoginInfo.SYSID + "_" + "BizActorPort"];

                LGC.GMES.MES.VIS001.Config.Extension = ConfigurationManager.AppSettings["Extension"];
                LGC.GMES.MES.VIS001.Config.DataFileFolder = ConfigurationManager.AppSettings["DataFileFolder"];
                LGC.GMES.MES.VIS001.Config.SeparationType = Convert.ToInt32(ConfigurationManager.AppSettings["SeparationType"]);

                LGC.GMES.MES.VIS001.Config.DataBackupFolderUsed = Convert.ToBoolean(ConfigurationManager.AppSettings["DataBackupFolderUsed"]);
                LGC.GMES.MES.VIS001.Config.DataBackupFolder = ConfigurationManager.AppSettings["DataBackupFolder"];
                LGC.GMES.MES.VIS001.Config.ImageBackupFolderUsed = Convert.ToBoolean(ConfigurationManager.AppSettings["ImageBackupFolderUsed"]);
                LGC.GMES.MES.VIS001.Config.ImageBackupFolder = ConfigurationManager.AppSettings["ImageBackupFolder"];
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void SetConfig()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings["ShopCode"].Value = LGC.GMES.MES.VIS001.Config.ShopCode;
                config.AppSettings.Settings["AreaCode"].Value = LGC.GMES.MES.VIS001.Config.AreaCode;

                config.AppSettings.Settings["FTPUsed"].Value = Convert.ToString(LGC.GMES.MES.VIS001.Config.FTPUsed);
                config.AppSettings.Settings["FTP"].Value = LGC.GMES.MES.VIS001.Config.FTP;
                config.AppSettings.Settings["FTPID"].Value = LGC.GMES.MES.VIS001.Config.FTPID;
                config.AppSettings.Settings["FTPPassword"].Value = LGC.GMES.MES.VIS001.Config.FTPPassword;

                config.AppSettings.Settings[LoginInfo.SYSID + "_" + "BizActorIP"].Value = LGC.GMES.MES.VIS001.Config.BizActorIP;
                config.AppSettings.Settings[LoginInfo.SYSID + "_" + "BizActorServiceIndex"].Value = LGC.GMES.MES.VIS001.Config.BizActorServiceIndex;
                config.AppSettings.Settings[LoginInfo.SYSID + "_" + "BizActorPort"].Value = LGC.GMES.MES.VIS001.Config.BizActorPort;

                config.AppSettings.Settings["Extension"].Value = LGC.GMES.MES.VIS001.Config.Extension;
                config.AppSettings.Settings["DataFileFolder"].Value = LGC.GMES.MES.VIS001.Config.DataFileFolder;
                config.AppSettings.Settings["SeparationType"].Value = Convert.ToString(LGC.GMES.MES.VIS001.Config.SeparationType);

                config.AppSettings.Settings["DataBackupFolderUsed"].Value = Convert.ToString(LGC.GMES.MES.VIS001.Config.DataBackupFolderUsed);
                config.AppSettings.Settings["DataBackupFolder"].Value = LGC.GMES.MES.VIS001.Config.DataBackupFolder;
                config.AppSettings.Settings["ImageBackupFolderUsed"].Value = Convert.ToString(LGC.GMES.MES.VIS001.Config.ImageBackupFolderUsed);
                config.AppSettings.Settings["ImageBackupFolder"].Value = LGC.GMES.MES.VIS001.Config.ImageBackupFolder;

                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("appSettings");

                GetConfig();
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void IconRunCheck(string Mode)
        {
            try
            {
                switch (Mode)
                {
                    case "START": //시작
                        this.nifIcon.Icon = Icon.FromHandle(((Bitmap)lstImg.Images[0]).GetHicon());
                        mnuStart.Checked = true;
                        mnuStop.Checked = false;
                        mnuRun.Checked = false;
                        break;
                    case "STOP": //정지
                        this.nifIcon.Icon = Icon.FromHandle(((Bitmap)lstImg.Images[1]).GetHicon());
                        mnuStart.Checked = false;
                        mnuStop.Checked = true;
                        mnuRun.Checked = false;
                        break;
                    case "RUN": //실행중
                        this.nifIcon.Icon = Icon.FromHandle(((Bitmap)lstImg.Images[2]).GetHicon());
                        mnuStart.Checked = false;
                        mnuStop.Checked = false;
                        mnuRun.Checked = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void SetFileWatcher()
        {
            try
            {
                fsw = new FileSystemWatcher(LGC.GMES.MES.VIS001.Config.DataFileFolder);
                fsw.Filter = "*." + LGC.GMES.MES.VIS001.Config.Extension; //csv파일생성시에만 이벤트발생
                fsw.NotifyFilter = NotifyFilters.FileName;
                fsw.Created += new FileSystemEventHandler(WatchOnCreated);
                fsw.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        public static void SystemLoggerWriteLine(string strLog)
        {
            DirectoryInfo dir = new DirectoryInfo(LGC.GMES.MES.VIS001.Common.sysDir);
            if (dir.Exists == false)
            {
                dir.Create();
            }

            FileStream LogFs = new FileStream(LGC.GMES.MES.VIS001.Common.sysDir + System.DateTime.Now.ToString("yyyyMM_") + "SYS.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            LogFs.Seek(0, SeekOrigin.End);

            System.IO.StreamWriter file = new System.IO.StreamWriter(LogFs, System.Text.Encoding.Default);
            file.WriteLine("[" + System.DateTime.Now + "]  " + strLog);
            file.Close();
        }

        public static void DataLoggerWriteLine(string strLog)
        {
            DirectoryInfo dir = new DirectoryInfo(LGC.GMES.MES.VIS001.Common.dataDir);
            if (dir.Exists == false)
            {
                dir.Create();
            }

            FileStream LogFs = new FileStream(LGC.GMES.MES.VIS001.Common.dataDir + System.DateTime.Now.ToString("yyyyMM_") + "DATA.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            LogFs.Seek(0, SeekOrigin.End);

            System.IO.StreamWriter file = new System.IO.StreamWriter(LogFs, System.Text.Encoding.Default);
            file.WriteLine("[" + System.DateTime.Now + "]  " + strLog);
            file.Close();
        }

        private void DataReadDir()
        {
            //*********************************************************************************
            //**  Function Name    : DataReadDir
            //**  Description      : 파일을 읽어 나눈다.
            //**  Parameter        : 
            //**  Modification Log : 
            //*********************************************************************************

            if (LGC.GMES.MES.VIS001.Config.FTPUsed == true)
            {
                FTP_Util.FTP ftp = new FTP_Util.FTP(Config.FTP, Config.FTPID, Config.FTPPassword, "");
                if (ftp.CheckFTP() == false)
                {
                    SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "FTP 연결 실패!");
                }
            }

            DirectoryInfo getDir = new DirectoryInfo(LGC.GMES.MES.VIS001.Config.DataFileFolder);
            //Data File Folder 에 신규 생성된 파일 리스트
            //[0]: {20160817010701_5D26G0322_DEFACT.csv}
            //[1]: {20160817015026_5D26G033_DEFACT.csv}
            FileInfo[] gatherFileList = getDir.GetFiles(); 
            string jobFile = string.Empty;
            foreach (FileInfo fileinfo in gatherFileList)
            {
                //파일 리스트 개수 만큼 CSV 데이터 저장 및 FTP 전송 반복
                System.Threading.Thread.Sleep(2000);
                jobFile = fileinfo.FullName;
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Handling  Start");
                DataReadFile(jobFile);
                BackupCSVFile(jobFile);
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "Handling End");
            }
        }

        private void DataReadFile(string path)
        {
            //*********************************************************************************
            //**  Function Name    : DataReadFile
            //**  Description      : 파일을 읽어 나눈다.
            //**  Parameter        : path : 풀패스명
            //**  Modification Log : 
            //*********************************************************************************

            ArrayList aryLineNum = new ArrayList(); //파일 데이터 각 라인
            ArrayList data = new ArrayList();
            ArrayList commonData = new ArrayList();
            FileInfo chkFile = new FileInfo(path);
            string[] aryDateKind;
            string LogSeq = string.Empty;

            try
            {
                if(chkFile.Exists == true && chkFile.Length > 0)
                {
                    LogSeq = "1";                                                           //clsIn.getSeqNum 시퀀스 가져오기 SELECT FUN_SEQ_NO FROM DUAL
                    commonData.Add(LogSeq);                                                 //common0 : "1" 시퀸스
                    aryDateKind = GetCsvFileKind(chkFile.Name);
                    commonData.Add(aryDateKind[0]);                                         //common1 : "20160817010701" 검사시작시간
                    commonData.Add(aryDateKind[aryDateKind.Length - 1].Substring(0, 1));    //common2 : "D" 타입
                    commonData.Add(chkFile.Name);                                           //common3 : "20160817010701_5D26G0322_DEFACT.csv" 파일명
                    aryLineNum = GetFileData(chkFile.FullName);                             //'파일의 각 라인을 읽어온다.
                    //dataDealLog(commonData, GetDivType(aryLineNum(0).ToString()))         // 'LOG를 넣자!(INSERT INTO TB_IEB762)
                    DataDealHis(commonData, aryLineNum);                                    //'History를 넣자! (INTO TB_IEB760 TB, INSERT INTO TB_IEB763, INSERT INTO TB_IEB761 )
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

         private void BackupCSVFile(string file)
        {
            //*********************************************************************************
            //**  Function Name    : BackupCSVFile
            //**  Description      : 파일을 백업하거나 삭제
            //**  Parameter        : LGC.GMES.MES.VIS001.Config.DataBackupFolderUsed : 파일삭제
            //**                     BACK폴더로 파일 이동
            //**  Modification Log : 
            //*********************************************************************************

            try
            {
                if (LGC.GMES.MES.VIS001.Config.DataBackupFolderUsed == true)
                {
                    string strDate = System.DateTime.Now.ToString("yyyyMMdd");
                    FileInfo gf = new FileInfo(file);
                    string filename = gf.Name;
                    FileInfo df = new FileInfo(LGC.GMES.MES.VIS001.Config.DataBackupFolder + @"\" + strDate + @"\" + filename);
                    if (df.Directory.Exists == false ) //  '폴더가 없으면 맹글고
                    {
                        makeBackupDir(df.DirectoryName);
                    }
                    if (df.Exists == true) //  '파일이 이미 존재한다면 지우고
                    {
                        df.Delete();
                    }
                    if (gf.Exists == true)
                    {
                        gf.MoveTo(LGC.GMES.MES.VIS001.Config.DataBackupFolder + "\\" + strDate + "\\" + filename); //이동
                        SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + gf.FullName + " csv Backup");
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private string[] GetCsvFileKind(string strfile)
        {
            //*********************************************************************************
            //**  Function Name    : GetCsvFileKind
            //**  Description      : 파일명만 분리 ( 예 : .csv 제거 )
            //**  Parameter        : 
            //**  Modification Log : 
            //*********************************************************************************

            try
            {
                string[] ary = (strfile).Split('_');
                ary[ary.Length - 1] = ary[ary.Length - 1].Substring(0, ary[ary.Length - 1].IndexOf("."));
                return ary;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return null;
            }
        }

        private ArrayList GetFileData(string file)
        {
            //*********************************************************************************
            //**  Function Name    : GetFileDataInfo
            //**  Description      : 파일의 라인을 읽어서 배열에 저장 
            //**  Parameter        : 
            //**  Modification Log : 
            //*********************************************************************************

            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);
            try
            {
                ArrayList strStream = new ArrayList();
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                while(sr.Peek() > -1)                //다음 문자가 있을 동안~ 돌아라
                {
                    strStream.Add(sr.ReadLine());
                }
                return strStream;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return null;
            }
            finally
            {
                fs.Close();
                sr.Close();
            }
        }

        private void makeBackupDir(string fullpath)
        {
            try
            {
                string[] temp = fullpath.Split('\\');
                string dir = string.Empty;
                DirectoryInfo dirbase = new DirectoryInfo(temp[0] + @"\");
                for (int idx = 1; idx < temp.Length; idx ++)
                {
                    dir = dir + temp[idx] + "\\";
                    dirbase.CreateSubdirectory(dir);
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void DataDealHis(ArrayList common , ArrayList line)
        {
            //*********************************************************************************
            //**  Function Name    : DataDealHis
            //**  Description      : DB에 넣는다. 이미지도 처리한다.
            //**  Parameter        : 
            //**  Modification Log : 
            //*********************************************************************************
            try
            {
                //'common0 : 시퀸스
                //'common1 : 검사시작시간
                //'common2 : 타입
                //'common3 : 파일명
                switch(Convert.ToString(common[2]))
                {
                    case "B":
                        //insertMaster(line);
                        break;
                    case "P":
                        //insertPattern(common(0), line);
                        break;
                    case "D":
                        InsertDefect(Convert.ToString(common[0]), line);
                        break;
                }

            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void InsertDefect(string LogSeq, ArrayList line)
        {
            //*********************************************************************************
            //**  Function Name    : InsertDefect
            //**  Description      : DB에 insert한다.   IEBIS.TB_IEB761     (defect 실적)
            //**  Parameter        : 
            //**  Modification Log :               
            //*********************************************************************************

            string strSQL = string.Empty;
            ArrayList data;
            int cnt = 0;
            int seq = 1;
            string oldImagePath = string.Empty;
            try
            {
                //구분자를 이용한 데이터 Split
                data = GetDivType(Convert.ToString(line[0])); 
                //'ImagePathCk -> 한파일을 처리하다가 이미지 경로명이 바뀌면 새로 FTP서버에 폴더를 맹글어주기 위함이다.
                //FTP 경로명 만들기
                Config.ImagePathCk = ForUploadImagePath(Convert.ToString(data[0]), Convert.ToString(data[1]), Convert.ToString(data[data.Count - 1]));
                //ftp에 폴더가 있으면 패쓰, 없으믄 맹근다.
                FtpCreateFullDirectory(Config.ImagePathCk);   
                for (int idx = 0; idx < line.Count; idx ++ )
                {
                    data = GetDivType(Convert.ToString(line[idx]));
                    oldImagePath = Convert.ToString(data[data.Count - 1]);
                    //"D:\\ETI\\Result\\20160816-23\\359190B1-L01_0816235505_156.bmp" =>
                    //"/A010T93BL40L211/2016/11/20160816-23/359190B1-L01_0816235505_156.jpg"
                    data[data.Count - 1] = GetUploadImagePath(Convert.ToString(data[0]), Convert.ToString(data[1]), Convert.ToString(data[data.Count - 1]), "/");
                    //'이미지경로명이 바뀌면 또 한번 경로체크해주고 ftp에 폴더를 만든다.
                    if (Convert.ToString(data[data.Count - 1]).Substring(0, Convert.ToString(data[data.Count - 1]).LastIndexOf("/") + 1) != Config.ImagePathCk)
                    {
                        //FTP 경로명 만들기
                        Config.ImagePathCk = ForUploadImagePath(Convert.ToString(data[0]), Convert.ToString(data[1]), Convert.ToString(data[data.Count - 1]), '/');
                        //ftp에 폴더가 있으면 패쓰, 없으믄 맹근다.
                        FtpCreateFullDirectory(Config.ImagePathCk);
                    }
                    UploadImage(oldImagePath, Convert.ToString(data[12])); //'이미지를 jpg로 저장하고, jpg를 업로드한다.
                    BackupImageFile(oldImagePath); //'jpg를 백업하거나, 지운다.  BMP는 안 건드린다.
                }

                ////CSV File 내용 저장 =====================================================================================================
                //DataTable loginIndataTable = new DataTable();
                //loginIndataTable.Columns.Add("USERID", typeof(string));
                //loginIndataTable.Columns.Add("LANGID", typeof(string));
                //DataRow loginIndata = loginIndataTable.NewRow();
                //loginIndata["USERID"] = "cnsjungks";
                //loginIndata["LANGID"] = "ko-KR";
                //loginIndataTable.Rows.Add(loginIndata);

                //new LGC.GMES.MES.Common.ClientProxy().ExecuteService("DA_BAS_SEL_PERSON_TBL", "INDATA", "OUTDATA", loginIndataTable, (loginOutdata, loginException) =>
                //{
                //    if (loginException != null)
                //    {
                //        SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + loginException.Message);
                //        return;
                //    }
                //}, null, true);
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private ArrayList GetDivType(string dataLine)
        {
            //*********************************************************************************
            //**  Function Name    : GetDivType
            //**  Description      : 한 라인을 지정한 타입(',' ,';' ,'tab') 으로 나눈다.
            //**  Parameter        : 
            //**  Modification Log : 
            //*********************************************************************************

            try
            {
                string[] rtn;
                ArrayList ary = new ArrayList();
                //"BL40,L211, 5D26G0322,359190B1,    1643, 1,2,S005,                Line,    0.19,    1.09,20160816235505,D:\\ETI\\Result\\20160816-23\\359190B1-L01_0816235505_156.bmp"
                rtn = dataLine.Split(Config.DivChr);
                for ( int idx =0; idx < rtn.Length; idx++)
                {
                    ary.Add(rtn[idx]);
                }
                return ary;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return null;
            }
        }

        private string ForUploadImagePath(string wc, string eqp, string fullpath, char type  = '\\')
        {
            //*********************************************************************************
            //**  Function Name    : ForUploadImagePath()
            //**  Description      : Image 풀패스명을 읽어 업로드할 ftp패스명으로 바꾼다.
            //**                   : 예) /A010T93AL40L111/2009/01/20090101_01/
            //**  Parameter        : 
            //**  Modification Log : 
            //*********************************************************************************

            try
            {
                string[] before;
                string strDateName = string.Empty;
                string strYM = System.DateTime.Now.ToString("yyyy") + "/" + System.DateTime.Now.ToString("MM");
                before = fullpath.Split(type);
                strDateName = before[before.Length - 2];
                return "/" + Config.ShopCode + Config.AreaCode + wc + eqp + "/" + strYM + "/" + strDateName + "/";
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return null;
            }

        }

        private bool FtpCreateFullDirectory(string path)
        {
            //*********************************************************************************
            //**  Function Name    : FtpCreateFullDirectory
            //**  Description      : path명에 따라 ftp서버에 폴더를 만든다... 쭉~ 만든다. 물론 있으면 패스한다. 
            //**                     패스명이 와야한다. 만약 파일명을 포함한 풀패스명이 오면 파일명으로 폴더를 맹근다. 주의...
            //**  Parameter        : 
            //**  Modification Log : 2008-06-18 shin hee chul vision 불량데이터수집 Initial Coding
            //*********************************************************************************

            try
            {
                if (LGC.GMES.MES.VIS001.Config.FTPUsed == false)
                {
                    return true;
                }

                ArrayList temp = ftpMatchDir(path);
                string bikoDir = string.Empty;
                string makeDir = string.Empty;
                List<string> ftpDirList;
                bool dirCk = false;

                FTP_Util.FTP ftp = new FTP_Util.FTP(Config.FTP, Config.FTPID, Config.FTPPassword, "");

                for (int idx01 =0; idx01 < temp.Count -1; idx01++)
                {
                    bikoDir = makeDir;
                    makeDir = (makeDir + Convert.ToString(temp[idx01 + 1])).Replace("//", "/");
                    dirCk = false;
                    ftpDirList = ftp.GetDirDetailList(bikoDir);
                    for ( int idx02 =0; idx02 < ftpDirList.Count; idx02++)
                    {
                        if(ftpDirList[idx02] == (bikoDir + Convert.ToString(temp[idx02 + 1])).Replace("//", "/"))
                        {
                            dirCk = true;
                            break;
                        }
                    }
                    if (dirCk == false) // 없으면 만들자
                    {
                        ftp.MakeDir(makeDir);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return false;
            }
        }

        private ArrayList ftpMatchDir(string path)
        {
            try
            {
                string[] temp = DelSlash(path).Split('/');
                ArrayList ary = new ArrayList();
                ary.Add("/");
                for (int idx = 0; idx < temp.Length; idx++)
                {
                    ary.Add("/" + Convert.ToString(temp[idx]));
                }
                return ary;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return null;
            }
        }

        private bool FtpDeleteDirectory(string dirpath)
        {
            try
            {


                return true;
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return false;
            }
        }

        private string DelSlash(string str, String mode = "a", string type = "/")
        {
            //*********************************************************************************
            //**  Function Name    : DelSlash()
            //**  Description      : 파라미터에 따라 문자열의 앞 뒤의 / or \를 지운다.
            //**  Parameter        : 1. 문자열
            //**                   : 2. 앞 or 뒤 or 모두       l, r, a
            //**                   : 3. type 
            //**  Modification Log : 2008-06-18 shin hee chul vision 불량데이터수집 Initial Coding
            //*********************************************************************************

            try
            {
                str = str.Trim();

                switch(mode)
                {
                    case "l":
                    case "L":
                        if (str.StartsWith(type))
                        {
                            return str.Substring(1, str.Length - 1);
                        }
                        else
                        {
                            return str;
                        }
                    case "r":
                    case "R":
                        if (str.EndsWith(type))
                        {
                            return str.Substring(0, str.Length - 1);
                        }
                        else
                        {
                            return str;
                        }
                        break;
                    case "a":
                    case "A":
                        if (str.StartsWith(type))
                        {
                            str = str.Substring(1, str.Length - 1);
                        }

                        if (str.EndsWith(type))
                        {
                            return str.Substring(0, str.Length - 1);
                        }
                        else
                        {
                            return str;
                        }
                    default:
                        return str;
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return null;
            }
        }

        private void UploadImage(string Localpath, string ftpPath)
        {
            //*********************************************************************************
            //**  Function Name    : uploadImage
            //**  Description      : Image를 웹서버로 올린다. 그 외 부가적인 짓을 하는디!
            //**  Parameter        : localpath : 풀패스.     ftpPath : ftp에 올릴 풀패스 
            //**  Modification Log : 
            //*********************************************************************************
            try
            {
                if (LGC.GMES.MES.VIS001.Config.FTPUsed == false)
                {
                    return;
                }
                //'bmp -> jpg
                string jpgFile = ConvertImageType(Localpath, false);   
                if (jpgFile != string.Empty)
                {
                    LGC.GMES.MES.VIS001.FTP_Util.FTP ftp = new FTP_Util.FTP(Config.FTP, Config.FTPID, Config.FTPPassword, "");
                    SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + Localpath);
                    ftp.Upload(ftpPath, jpgFile);
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void BackupImageFile(string fullPath)
        {
            //*********************************************************************************
            //**  Function Name    : BackupImageFile
            //**  Description      : 파일을 백업하거나 삭제
            //**  Parameter        : VS.ImageBackupFolderUsed = FALSE : 파일삭제
            //**                   :                TRUE  : 파일이동  (완료(DB에 저장)한 파일은  BACK폴더로 이동)
            //**  Modification Log : 2008-06-18 shin hee chul vision 불량데이터수집 Initial Coding
            //*********************************************************************************

            try
            {
                //    '헷갈리지말자
                //    '  1. bmp를 jpg로 저장한다.  (같은 이미지의 bmp와 jpg가 있겠다.)
                //    '  2. jpg를 업로드한다.
                //    '  3. - jpg를 백업하거나 지운다.
                //    '  4. bmp는 이 프로그램에서 건드리지 않는다.  (설비가 알아서 지운댄다.)

                fullPath = fullPath.Substring(0, fullPath.LastIndexOf(".")) + ".jpg";
                string[] ps = fullPath.Split('\\');
                FileInfo gf = new FileInfo(fullPath);
                string backf = System.DateTime.Now.ToString("yyyy") + @"\" + System.DateTime.Now.ToString("MM");

                if (Config.ImageBackupFolderUsed == true)
                {
                    FileInfo df = new FileInfo(Config.ImageBackupFolder + @"\" + backf + @"\" + ps[ps.Length - 2] + @"\" + ps[ps.Length - 1]);
                    if (df.Directory.Exists == false)
                    {
                        MakeDir(df.DirectoryName);
                    }
                    if (df.Exists == true)
                    {
                        df.Delete();
                    }
                    if (gf.Exists == true)
                    {
                        gf.MoveTo(Config.ImageBackupFolder + "\\" + backf + "\\" + ps[ps.Length - 2] + "\\" + ps[ps.Length - 1]);
                        SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + gf.FullName + " Image Backup");
                    }
                }
                else
                {
                    if(gf.Exists == true)
                    {
                        gf.Delete();
                        SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + gf.FullName + " Image Delete");
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private void MakeDir(string fullpath)
        {
            try
            {
                string[] temp = fullpath.Split('\\');
                string dir = string.Empty;
                DirectoryInfo dirbase = new DirectoryInfo(temp[0] + "\\");
                for (int idx = 1; idx < temp.Length; idx++)
                {
                    dir = dir + temp[idx] + "\\";
                    dirbase.CreateSubdirectory(dir);
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
            }
        }

        private string ConvertImageType(string image, bool type = true)
        {
            //*********************************************************************************
            //**  Function Name    : ConvertImageType
            //**  Description      : 이미지 타입 변경(bmp -> jpg)    용량 줄일라궁..
            //**  Parameter        : 풀패스명,    true : 원본파일 지움
            //**  Modification Log : 
            //*********************************************************************************

            string jpgImg = image.Substring(0, image.LastIndexOf(".")) + ".jpg";

            FileInfo jpgFile = new FileInfo(jpgImg); //'jpg파일이 존재하면 걍 나와라
            if (jpgFile.Exists == true)
            {
                return jpgFile.FullName;
            }

            FileInfo bmpImg = new FileInfo(image); //'bmp 파일이 있다면 jpg로 변경하고
            if (bmpImg.Exists == true)
            {
                System.Drawing.Bitmap img = new Bitmap(image);

                img.Save(jpgImg, System.Drawing.Imaging.ImageFormat.Jpeg);
                img.Dispose();
                if (type == true)
                {
                    bmpImg.Delete();
                }
                return jpgImg;
            }
            return jpgImg;
        }

        private string GetUploadImagePath(string wc, string eqp, string fullpath, string type)
        {
            //*********************************************************************************
            //**  Function Name    : GetUploadImagePath()
            //**  Description      : 해당 Image 풀패스명을 읽어 업로드할 디비에 넣을 패스명으로 바꿔놓는다.
            //**                   : 예) /A010T93AL40L111/2009/01/20090101_01/파일명
            //**                   : 왜 저렇게 날짜가 난무하냐고? 파일이 한 폴더에 너무 많이 쌓이기때문에 월별로 나눠놨다.
            //**  Parameter        : type = \  혹은 /
            //**  Modification Log : 2008-06-18 shin hee chul vision 불량데이터수집 Initial Coding
            //*********************************************************************************
            try
            {
                string[] before;
                string strFileName;
                string strDateName;
                string strYM = System.DateTime.Now.ToString("yyyy") + type + System.DateTime.Now.ToString("MM");
                before = fullpath.Split('\\');

                strFileName = before[before.Length - 1].Substring(0, before[before.Length - 1].LastIndexOf(".")) + ".jpg";// '파일명
                strDateName = before[before.Length - 2];//  '패스명
                if (type == "/")
                {
                    return "/" + Config.ShopCode + Config.AreaCode + wc + eqp + "/" + strYM + "/" + strDateName + "/" + strFileName;
                }
                else
                {
                    return "\\" + Config.ShopCode + Config.AreaCode + wc + eqp + "\\" +  strYM +  "\\" +  strDateName +  "\\" +  strFileName;
                }
            }
            catch (Exception ex)
            {
                SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message);
                return null;
            }
        }

        private void RunUpload()
        {
            if (LGC.GMES.MES.VIS001.Config.FTPUsed == true)
            {
                LGC.GMES.MES.VIS001.FTP_Util.FTP ftp = new FTP_Util.FTP(Config.FTP, Config.FTPID, Config.FTPPassword, "");
                if (ftp.CheckFTP() == false)
                {
                    SystemLoggerWriteLine(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + "FTP 연결 실패!");
                }
            }
            DataReadDir();
            IconRunCheck("START");
        }

        #endregion
    }
}
