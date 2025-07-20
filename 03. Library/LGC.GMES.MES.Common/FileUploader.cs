using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Configuration;

using C1.C1Zip;
using Microsoft.Win32;

namespace LGC.GMES.MES.Common
{
    public class UploadCompletedEventArgs : EventArgs
    {
        public bool Success
        {
            get;
            internal set;
        }

        public bool Cancelled
        {
            get;
            internal set;
        }

        public Exception Error
        {
            get;
            internal set;
        }

        public string[] Files
        {
            get;
            internal set;
        }

        public object Response
        {
            get;
            internal set;
        }

        public object UserState
        {
            get;
            internal set;
        }

        internal UploadCompletedEventArgs(bool success, bool cancelled, Exception error, string[] files, object response, object userState)
        {
            Success = success;
            Cancelled = cancelled;
            Error = error;
            Files = files;
            Response = response;
            UserState = userState;
        }
    }

    internal class StreamUploader
    {
        internal void uploadStream(string fileuniqueid, int fileseq, string filekey, string filename, Stream stream, object userState, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler)
        {
            int blockSize = 1024 * 1024 * 3;
            int readedTotal = 0;
            int partNumber = 1;
            int uploadedNumber = 0;
            int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

            while (stream.Length > readedTotal)
            {
                byte[] buffer = new byte[blockSize];
                int readed = stream.Read(buffer, 0, blockSize);
                readedTotal += readed;

                FileInfo tempFile = new FileInfo(Directory.GetCurrentDirectory() + "\\" + filekey + "_" + partNumber.ToString());
                using (FileStream fs = tempFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    fs.Write(buffer, 0, readed);
                    fs.Flush();
                    fs.Close();
                }

                WebClient client = new WebClient();
                client.UploadFileCompleted += (s, arg) =>
                {
                    FileInfo uploadedFilePart = arg.UserState as FileInfo;
                    uploadedFilePart.Delete();
                    uploadedNumber++;

                    if (partCount == uploadedNumber)
                    {
                        if (uploadCompleteHandler != null)
                        {
                            uploadCompleteHandler(client, new UploadCompletedEventArgs(arg.Error == null ? true : false, arg.Cancelled, arg.Error, null, null, userState));
                        }
                    }
                };
                client.UploadFileAsync(new Uri(Common.DeploymentUrl + "FileUploadHandler.ashx?FILEUNIQUEID=" + HttpUtility.UrlEncode(fileuniqueid) +
                                                                                        "&FILESEQ=" + HttpUtility.UrlEncode(fileseq.ToString()) +
                                                                                        "&FILEKEY=" + HttpUtility.UrlEncode(filekey) +
                                                                                        "&FILENAME=" + HttpUtility.UrlEncode(filename) +
                                                                                        "&INSUSER=" + HttpUtility.UrlEncode(LoginInfo.USERID) +
                                                                                        "&partNumber=" + HttpUtility.UrlEncode(partNumber.ToString()) +
                                                                                        "&partCount=" + HttpUtility.UrlEncode(partCount.ToString())), "POST", tempFile.FullName, tempFile);
                partNumber++;
            }
        }

        internal void uploadTempStream(string filekey, Stream stream, object userState, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler, bool bOpen = false)
        {
            int blockSize = 1024 * 1024 * 100;
            //int blockSize = 1024 * 1024 * 3;
            int readedTotal = 0;
            int partNumber = 1;
            int uploadedNumber = 0;
            int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

            try
            {
                SaveFileDialog od = new SaveFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = filekey + "_" + partNumber.ToString() + ".xlsx";

                if (od.ShowDialog() == true)
                {
                    while (stream.Length > readedTotal)
                    {
                        byte[] buffer = new byte[blockSize];
                        int readed = stream.Read(buffer, 0, blockSize);
                        readedTotal += readed;

                        //FileInfo tempFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + filekey + "_" + partNumber.ToString() + ".xlsx");
                        FileInfo tempFile = new FileInfo(od.FileName);

                        using (FileStream fs = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            fs.Write(buffer, 0, readed);
                            fs.Flush();
                            fs.Close();
                        }

                        //using (FileStream fs = tempFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        //{
                        //    fs.Write(buffer, 0, readed);
                        //    fs.Flush();
                        //    fs.Close();
                        //}

                        //WebClient client = new WebClient();
                        //client.UploadFileCompleted += (s, arg) =>
                        //{
                        //    FileInfo uploadedFilePart = arg.UserState as FileInfo;
                        //    uploadedFilePart.Delete();
                        //    uploadedNumber++;

                        //    if (partCount == uploadedNumber)
                        //    {
                        //        if (uploadCompleteHandler != null)
                        //        {
                        //            uploadCompleteHandler(client, new UploadCompletedEventArgs(arg.Error == null ? true : false, arg.Cancelled, arg.Error, null, null, userState));
                        //        }
                        //    }
                        //};
                        //client.UploadFileAsync(new Uri(Common.DeploymentUrl + "TempFileUploadHandler.ashx?FILEID=" + HttpUtility.UrlEncode(filekey) +
                        //                                                                                "&partNumber=" + HttpUtility.UrlEncode(partNumber.ToString()) +
                        //                                                                                "&partCount=" + HttpUtility.UrlEncode(partCount.ToString())), "POST", tempFile.FullName, tempFile);

                        partNumber++;
                    }

                    WebClient client = new WebClient();
                    uploadCompleteHandler(client, new UploadCompletedEventArgs(true, false, null, null, null, userState));

                    if (bOpen)
                        System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch { }
        }

        internal void uploadTempStream(string filekey, Stream stream, object userState, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler, C1.WPF.DataGrid.Excel.ExcelFileFormat fileFormat, bool bOpen = false)
        {
            int blockSize = 1024 * 1024 * 100;
            //int blockSize = 1024 * 1024 * 3;
            int readedTotal = 0;
            int partNumber = 1;
            int uploadedNumber = 0;
            int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

            try
            {
                SaveFileDialog od = new SaveFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                if(fileFormat == C1.WPF.DataGrid.Excel.ExcelFileFormat.Xls)
                    od.Filter = "Excel File 97-2003 (*.xls)|*.xls";
                else
                    od.Filter = "Excel Files (.xlsx)|*.xlsx";

                od.FileName = filekey;// + "_" + partNumber.ToString() + ".xlsx";

                if (od.ShowDialog() == true)
                {
                    while (stream.Length > readedTotal)
                    {
                        byte[] buffer = new byte[blockSize];
                        int readed = stream.Read(buffer, 0, blockSize);
                        readedTotal += readed;

                        //FileInfo tempFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + filekey + "_" + partNumber.ToString() + ".xlsx");
                        FileInfo tempFile = new FileInfo(od.FileName);

                        using (FileStream fs = tempFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            fs.Write(buffer, 0, readed);
                            fs.Flush();
                            fs.Close();
                        }

                        //using (FileStream fs = tempFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        //{
                        //    fs.Write(buffer, 0, readed);
                        //    fs.Flush();
                        //    fs.Close();
                        //}

                        //WebClient client = new WebClient();
                        //client.UploadFileCompleted += (s, arg) =>
                        //{
                        //    FileInfo uploadedFilePart = arg.UserState as FileInfo;
                        //    uploadedFilePart.Delete();
                        //    uploadedNumber++;

                        //    if (partCount == uploadedNumber)
                        //    {
                        //        if (uploadCompleteHandler != null)
                        //        {
                        //            uploadCompleteHandler(client, new UploadCompletedEventArgs(arg.Error == null ? true : false, arg.Cancelled, arg.Error, null, null, userState));
                        //        }
                        //    }
                        //};
                        //client.UploadFileAsync(new Uri(Common.DeploymentUrl + "TempFileUploadHandler.ashx?FILEID=" + HttpUtility.UrlEncode(filekey) +
                        //                                                                                "&partNumber=" + HttpUtility.UrlEncode(partNumber.ToString()) +
                        //                                                                                "&partCount=" + HttpUtility.UrlEncode(partCount.ToString())), "POST", tempFile.FullName, tempFile);

                        partNumber++;
                    }

                    WebClient client = new WebClient();
                    uploadCompleteHandler(client, new UploadCompletedEventArgs(true, false, null, null, null, userState));

                    if (bOpen)
                        System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch { }
        }
        //internal void uploadTempStream(string filekey, Stream stream, object userState, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler)
        //{
        //    int blockSize = 1024 * 1024 * 3;
        //    int readedTotal = 0;
        //    int partNumber = 1;
        //    int uploadedNumber = 0;
        //    int partCount = (int)Math.Ceiling(((double)stream.Length / (double)blockSize));

        //    while (stream.Length > readedTotal)
        //    {
        //        byte[] buffer = new byte[blockSize];
        //        int readed = stream.Read(buffer, 0, blockSize);
        //        readedTotal += readed;

        //        FileInfo tempFile = new FileInfo(Directory.GetCurrentDirectory() + "\\" + filekey + "_" + partNumber.ToString());
        //        using (FileStream fs = tempFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None))
        //        {
        //            fs.Write(buffer, 0, readed);
        //            fs.Flush();
        //            fs.Close();
        //        }

        //        WebClient client = new WebClient();
        //        client.UploadFileCompleted += (s, arg) =>
        //        {
        //            FileInfo uploadedFilePart = arg.UserState as FileInfo;
        //            uploadedFilePart.Delete();
        //            uploadedNumber++;

        //            if (partCount == uploadedNumber)
        //            {
        //                if (uploadCompleteHandler != null)
        //                {
        //                    uploadCompleteHandler(client, new UploadCompletedEventArgs(arg.Error == null ? true : false, arg.Cancelled, arg.Error, null, null, userState));
        //                }
        //            }
        //        };
        //        client.UploadFileAsync(new Uri(Common.DeploymentUrl + "TempFileUploadHandler.ashx?FILEID=" + HttpUtility.UrlEncode(filekey) +
        //                                                                                        "&partNumber=" + HttpUtility.UrlEncode(partNumber.ToString()) +
        //                                                                                        "&partCount=" + HttpUtility.UrlEncode(partCount.ToString())), "POST", tempFile.FullName, tempFile);
        //        partNumber++;
        //    }
        //}
    }

    public class FileUploader
    {
        public event EventHandler<UploadCompletedEventArgs> UploadCompleted;
        private string newFileSetID;

        internal void uploadStream(string fileuniqueid, int fileseq, string filekey, string filename, Stream stream, object userState, EventHandler<UploadCompletedEventArgs> uploadCompleteHandler)
        {
            new StreamUploader().uploadStream(fileuniqueid, fileseq, filekey, filename, stream, userState, uploadCompleteHandler);
        }

        public void UploadAsync(WebFileSet fileSet)
        {
            if (fileSet != null)
            {
                newFileSetID = Guid.NewGuid().ToString("N");

                if (fileSet.WebFileList.Count > 0)
                {
                    uploadWebFileList(fileSet.WebFileList[0], new Action<WebFileSet>((uploadedFileSet) =>
                    {
                        uploadedFileSet._FileSetID = newFileSetID;
                        raiseComplete(new UploadCompletedEventArgs(true, false, null, null, null, uploadedFileSet));
                    }));
                }
                else
                {
                    raiseComplete(new UploadCompletedEventArgs(false, false, null, null, null, fileSet));
                }
            }
        }

        private void uploadWebFileList(WebFileInfo fileInfo, Action<WebFileSet> uploadWebFileListComplete)
        {
            if (fileInfo.LocalFile != null)
            {
                using (FileStream fs = fileInfo.LocalFile.OpenRead())
                {
                    string newFileID = Guid.NewGuid().ToString("N");
                    fileInfo.WebFileName = fileInfo.LocalFile.Name;
                    uploadStream(newFileSetID, fileInfo.WebFileSet.WebFileList.IndexOf(fileInfo) + 1, newFileID, fileInfo.WebFileName, fs, fileInfo, (sender, arg) =>
                    {
                        if (arg.Error != null)
                        {
                            raiseComplete(arg);
                        }
                        else
                        {
                            WebFileInfo uploadedFile = arg.UserState as WebFileInfo;
                            WebFileSet uploadingFileSet = uploadedFile.WebFileSet;

                            uploadedFile.WebFileID = newFileID;

                            if (uploadingFileSet.WebFileList.Count - 1 == uploadingFileSet.WebFileList.IndexOf(uploadedFile))
                            {
                                uploadWebFileListComplete(uploadingFileSet);
                            }
                            else
                            {
                                WebFileInfo nextUploadFile = uploadingFileSet.WebFileList[uploadingFileSet.WebFileList.IndexOf(uploadedFile) + 1];
                                uploadWebFileList(nextUploadFile, uploadWebFileListComplete);
                            }
                        }
                    }
                    );
                }
            }
            else if (!string.IsNullOrEmpty(fileInfo.WebFileID))
            {
                string newFileID = Guid.NewGuid().ToString("N");

                DataTable table = new DataTable();
                table.Columns.Add("OLDFILESETID", typeof(string));
                table.Columns.Add("NEWFILESETID", typeof(string));
                table.Columns.Add("OLDFILEID", typeof(string));
                table.Columns.Add("NEWFILEID", typeof(string));
                table.Columns.Add("NEWFILESEQ", typeof(decimal));
                table.Columns.Add("INSUSER", typeof(string));
                DataRow row = table.NewRow();
                row["OLDFILESETID"] = fileInfo.WebFileSet.FileSetID;
                row["NEWFILESETID"] = newFileSetID;
                row["OLDFILEID"] = fileInfo.WebFileID;
                row["NEWFILEID"] = newFileID;
                row["NEWFILESEQ"] = (decimal)(fileInfo.WebFileSet.WebFileList.IndexOf(fileInfo) + 1);
                row["INSUSER"] = LoginInfo.USERID;
                table.Rows.Add(row);

                new ClientProxy().ExecuteService("CUS_INS_ATTACHEDFILE_COPY_G", "INDATA", "OUTDATA", table, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            raiseComplete(new UploadCompletedEventArgs(false, false, ex, null, null, fileInfo.WebFileSet));
                            return;
                        }

                        fileInfo.WebFileID = newFileID;

                        if (fileInfo.WebFileSet.WebFileList.Count - 1 == fileInfo.WebFileSet.WebFileList.IndexOf(fileInfo))
                        {
                            uploadWebFileListComplete(fileInfo.WebFileSet);
                        }
                        else
                        {
                            WebFileInfo nextUploadFile = fileInfo.WebFileSet.WebFileList[fileInfo.WebFileSet.WebFileList.IndexOf(fileInfo) + 1];
                            uploadWebFileList(nextUploadFile, uploadWebFileListComplete);
                        }
                    }
                );
            }
            else
            {
                // process next file
                if (fileInfo.WebFileSet.WebFileList.Count - 1 == fileInfo.WebFileSet.WebFileList.IndexOf(fileInfo))
                {
                    uploadWebFileListComplete(fileInfo.WebFileSet);
                }
                else
                {
                    WebFileInfo nextUploadFile = fileInfo.WebFileSet.WebFileList[fileInfo.WebFileSet.WebFileList.IndexOf(fileInfo) + 1];
                    uploadWebFileList(nextUploadFile, uploadWebFileListComplete);
                }
            }
        }

        private void raiseComplete(UploadCompletedEventArgs arg)
        {
            if (UploadCompleted != null)
            {
                UploadCompleted(this, arg);
            }
        }
    }
}
