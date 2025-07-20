using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Collections.Specialized;

namespace LGC.GMES.MES.Common
{
    public class WebFileInfo : INotifyPropertyChanged
    {
        private FileInfo _LocalFile;
        public FileInfo LocalFile
        {
            get
            {
                return _LocalFile;
            }
            set
            {
                _LocalFile = value;
                WebFileID = null;
                WebFileName = string.Empty;
                NotifyPropertyChanged("LocalFile");
            }
        }

        private string _WebFileID;
        public string WebFileID
        {
            get
            {
                return _WebFileID;
            }
            internal set
            {
                _WebFileID = value;
                NotifyPropertyChanged("WebFileID");
            }
        }

        private string _WebFileName = string.Empty;
        public string WebFileName
        {
            get
            {
                return _WebFileName;
            }
            internal set
            {
                _WebFileName = value;
                NotifyPropertyChanged("WebFileName");
            }
        }

        internal WebFileSet _WebFileSet = null;
        public WebFileSet WebFileSet
        {
            get
            {
                return _WebFileSet;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class WebFileSet : INotifyPropertyChanged
    {
        internal string _FileSetID;
        public string FileSetID
        {
            get
            {
                return _FileSetID;
            }
            internal set
            {
                _FileSetID = value;
                NotifyPropertyChanged("FileSetID");

                WebFileList.Clear();

                if (!string.IsNullOrEmpty(FileSetID))
                {
                    DataTable webFileSelectIndata = new DataTable();
                    webFileSelectIndata.Columns.Add("FILEUNIQUEID", typeof(string));
                    DataRow webFileSearchCond = webFileSelectIndata.NewRow();
                    webFileSearchCond["FILEUNIQUEID"] = FileSetID;
                    webFileSelectIndata.Rows.Add(webFileSearchCond);

                    new ClientProxy().ExecuteService("COR_SEL_ATTACHEDFILE_UNIQUEID", "INDATA", "OUTDATA", webFileSelectIndata, (result, ex) =>
                        {
                            if (ex == null)
                            {
                                WebFileList.Clear();
                                IEnumerable<DataRow> orderedResult = (from DataRow r in result.Rows orderby r["FILESEQ"] select r);
                                foreach (DataRow row in orderedResult)
                                {
                                    WebFileInfo fileInfo = new WebFileInfo() { WebFileID = row["FILEKEY"].ToString(), WebFileName = row["FILENAME"].ToString(), _WebFileSet = this };
                                    this.Add(fileInfo);
                                }
                            }
                            NotifyPropertyChanged("WebFileList");
                        }
                    );
                }
            }
        }

        private ObservableCollection<WebFileInfo> _WebFileList = new ObservableCollection<WebFileInfo>();
        public ObservableCollection<WebFileInfo> WebFileList
        {
            get
            {
                return _WebFileList;
            }
        }

        public WebFileSet()
        {
            WebFileList.CollectionChanged += (s, arg) => NotifyPropertyChanged("WebFileList");
        }

        public void Add(WebFileInfo fileInfo)
        {
            WebFileList.Add(fileInfo);
            fileInfo._WebFileSet = this;
        }

        public WebFileSet(string fileSetID)
        {
            FileSetID = fileSetID;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
