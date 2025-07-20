using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Threading;

namespace LGC.GMES.MES.Common
{
    public enum LogCategory { UI, FRAME, BIZ };
    public class Logger
    {
        public const string OPERATION_C = "Create ";
        public const string OPERATION_R = "Retrieve ";
        public const string OPERATION_U = "Update ";
        public const string OPERATION_D = "Delete ";

        public const string MESSAGE_OPERATION_START = "operation start";
        public const string MESSAGE_OPERATION_END = "operation end";

        public static readonly Logger Instance = new Logger();
        public static readonly SemaphoreSlim _logSemaphore = new SemaphoreSlim(1, 1);

        private string log_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GMES\\SFU\\LOG";
        private string log_file_name = "SFU_CLIENT_LOG_";
        private string log_file_ext = ".log";

        private object SyncObject = new object();

        private Logger()
        {
            string[] directoryNames = log_path.Split('\\');
            string current = string.Empty;

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
            {
                directoryNames = log_path.Split('\\');

                for (int inx = 0; inx < directoryNames.Length; inx++)
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

                foreach (FileInfo file in new DirectoryInfo(log_path).GetFiles())
                    if (file.LastWriteTime.AddDays(7) < DateTime.Now)
                        file.Delete();
            }
            else
            {
                log_path = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"] + "LOG";
                directoryNames = log_path.Split('\\');

                for (int inx = 0; inx < directoryNames.Length; inx++)
                {
                    string directoryName = directoryNames[inx];

                    if (string.IsNullOrEmpty(current))
                        current = directoryName;
                    else
                        current += "\\" + directoryName;

                    current = current.Replace("C:", @"\\Client\C$");
                    DirectoryInfo directoryInfo = new DirectoryInfo(current);

                    if (!directoryInfo.Exists)
                        directoryInfo.Create();
                }

                log_path = log_path.Replace("C:", @"\\Client\C$");

                foreach (FileInfo file in new DirectoryInfo(log_path).GetFiles())
                    if (file.LastWriteTime.AddDays(7) < DateTime.Now)
                        file.Delete();
            }
        }

        public void WriteLine(FrameworkElement control, string eventName, LogCategory category = LogCategory.UI)
        {
            string log = (string.IsNullOrEmpty(control.Name) ? control.GetType().Name : control.Name) + " : " + eventName + " event is ocurred";
            writeLog(log, category);
        }

        public void WriteLine(string operationName, string message, LogCategory category = LogCategory.UI)
        {
            string log = "Operation : " + operationName + ", Message : " + message;
            writeLog(log, category);
        }

        public void WriteLine(string operationName, LogCategory category = LogCategory.UI, params object[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Operation : " + operationName);
            foreach(object parameter in parameters)
            {
                if (parameter == null)
                {
                    continue;
                } 
                else if (parameter is DataSet)
                {
                    DataSet ds = parameter as DataSet;
                    using (StringWriter sw = new StringWriter())
                    {
                        ds.WriteXml(sw);
                        sb.Append("    Parameter : ");
                        sb.AppendLine(sw.ToString());
                    }
                }
                else if (parameter is DataTable)
                {
                    DataTable dt = parameter as DataTable;
                    using (StringWriter sw = new StringWriter())
                    {
                        dt.WriteXml(sw);
                        sb.Append("    Parameter : ");
                        sb.AppendLine(sw.ToString());
                    }
                }
                else
                {
                    sb.AppendLine("    Parameter : " + parameter.ToString());
                }
            }
            writeLog(sb.ToString(), category);
        }

        public void WriteLine(string operationName, Exception ex, LogCategory category = LogCategory.UI)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Operation : " + operationName + ", Exception : " + ex.Message);
            sb.AppendLine("Stack Trace : " + ex.StackTrace);
            writeLog(sb.ToString(), category);
        }

        private async void writeLog(string msg, LogCategory category)
        {
            switch (category)
            {
                case LogCategory.UI:
                    if (!CustomConfig.Instance.CONFIG_LOGGING_UI)
                        return;
                    break;
                case LogCategory.FRAME:
                    if (!CustomConfig.Instance.CONFIG_LOGGING_FRAME)
                        return;
                    break;
                case LogCategory.BIZ:
                    if (!CustomConfig.Instance.CONFIG_LOGGING_BIZRULE)
                        return;
                    break;
                default:
                    return;
            }

            await _logSemaphore.WaitAsync();

            FileInfo file = new FileInfo(log_path + "\\" + log_file_name + DateTime.Now.ToString("yyyyMMdd") + log_file_ext);

            try
            {
                using (FileStream fs = file.Open(FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        await sw.WriteLineAsync("[" + DateTime.Now.ToString() + "] " + msg);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {
                _logSemaphore.Release();
            }
        }
    }
}
