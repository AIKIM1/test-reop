using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LGC.GMES.MES.VIS001
{
    public class Common
    {
        public static string baseDir = Application.StartupPath + @"\";
        public static string dataDir = baseDir + "DATA_LOG" + @"\";
        public static string sysDir = baseDir + "SYS_LOG" + @"\";
        public static string mIniPath = baseDir + "vision.ini"; //ini설정파일 경로
    }

    public class Config
    {
        public static string ShopCode = string.Empty;
        public static string AreaCode = string.Empty;

        public static bool FTPUsed = false;
        public static string FTP = string.Empty;
        public static string FTPID = string.Empty;
        public static string FTPPassword = string.Empty;

        public static string BizActorIP = string.Empty;
        public static string BizActorServiceIndex = string.Empty;
        public static string BizActorPort = string.Empty;

        public static string Extension = string.Empty;
        public static string DataFileFolder = string.Empty;
        public static int SeparationType = 0;

        public static bool DataBackupFolderUsed = false;
        public static string DataBackupFolder = string.Empty;
        public static bool ImageBackupFolderUsed = false;
        public static string ImageBackupFolder = string.Empty;

        public static string ImagePathCk = string.Empty;
        public static char DivChr;

        public static int SleepWatchOn = 10;
    }
}
