using System;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.Common
{
    public class FileDownloader
    {
        public void Download(string fileSetID, string fileID, string fileName)
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(Common.DeploymentUrl + "FileDownloadHandler.ashx?FILESETID=" + HttpUtility.UrlEncode(fileSetID)
                                                                                                    + "&FILEID=" + HttpUtility.UrlEncode(fileID)
                                                                                                    + "&FILENAME=" + HttpUtility.UrlEncode(fileName));
        }
        
        internal void TempFileDownload(string fileID, string fileName)
        {
            System.Diagnostics.Process process = System.Diagnostics.Process.Start(Common.DeploymentUrl + "TempFileDownloadHandler.ashx?FILEID=" + HttpUtility.UrlEncode(fileID)
                                                                                                                                   + "&FILENAME=" + HttpUtility.UrlEncode(fileName));
        }

        public Uri GetDownloadUri(string fileSetID, string fileID, string fileName)
        {
            return new Uri(Common.DeploymentUrl + "FileDownloadHandler.ashx?FILESETID=" + HttpUtility.UrlEncode(fileSetID)
                                                + "&FILEID=" + HttpUtility.UrlEncode(fileID)
                                                + "&FILENAME=" + HttpUtility.UrlEncode(fileName), UriKind.Absolute);
        }
    }
}
