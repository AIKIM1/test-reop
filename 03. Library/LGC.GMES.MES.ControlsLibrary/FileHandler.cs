using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class FileHandler : Control
    {
        private TextBlock tbFile;
        private Image imgDelete;
        private C1FilePicker fpFile;

        public static DependencyProperty HasWebFileIDProperty = DependencyProperty.Register("HasWebFileID", typeof(bool), typeof(FileHandler), new PropertyMetadata(false));
        public bool HasWebFileID
        {
            get
            {
                return (bool)GetValue(HasWebFileIDProperty);
            }
        }

        public static DependencyProperty WebFileInfoProperty = DependencyProperty.Register("WebFileInfo", typeof(WebFileInfo), typeof(FileHandler), new PropertyMetadata(new WebFileInfo(), WebFileInfoPropertyChangedCallback));
        public WebFileInfo WebFileInfo
        {
            get
            {
                return GetValue(WebFileInfoProperty) as WebFileInfo;
            }
            set
            {
                SetValue(WebFileInfoProperty, value);
            }
        }
        public static void WebFileInfoPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebFileInfo fileInfo = e.NewValue as WebFileInfo;
            if (fileInfo != null)
            {
                if (string.IsNullOrEmpty(fileInfo.WebFileID))
                    d.SetValue(FileHandler.HasWebFileIDProperty, false);
                else
                    d.SetValue(FileHandler.HasWebFileIDProperty, true);
            }
        }

        public static DependencyProperty WebFileSetIDProperty = DependencyProperty.Register("WebFileSetID", typeof(string), typeof(FileHandler), new PropertyMetadata(null));
        public string WebFileSetID
        {
            get
            {
                return (string)GetValue(WebFileSetIDProperty);
            }
            set
            {
                SetValue(WebFileSetIDProperty, value);
            }
        }

        public static DependencyProperty FileDeleteCommandProperty = DependencyProperty.Register("FileDeleteCommand", typeof(ICommand), typeof(FileHandler), new PropertyMetadata(null));
        public ICommand FileDeleteCommand
        {
            get
            {
                return GetValue(FileDeleteCommandProperty) as ICommand;
            }
            set
            {
                SetValue(FileDeleteCommandProperty, value);
            }
        }

        public static readonly DependencyProperty IsReadonlyProperty = DependencyProperty.Register("IsReadonly", typeof(bool), typeof(FileHandler), new PropertyMetadata(false));
        public bool IsReadonly
        {
            get
            {
                return (bool)GetValue(IsReadonlyProperty);
            }
            set
            {
                SetValue(IsReadonlyProperty, value);
            }
        }

        public static readonly DependencyProperty MaxFileSizeProperty = DependencyProperty.Register("MaxFileSize", typeof(int), typeof(FileHandler), new PropertyMetadata(100));
        public int MaxFileSize
        {
            get
            {
                return (int)GetValue(MaxFileSizeProperty);
            }
            set
            {
                SetValue(MaxFileSizeProperty, value);
            }
        }

        public FileHandler()
        {
            this.DefaultStyleKey = typeof(FileHandler);
            this.DataContextChanged += FileHandler_DataContextChanged;
        }

        void FileHandler_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is WebFileInfo)
            {
                WebFileInfo = e.NewValue as WebFileInfo;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            tbFile = GetTemplateChild("tbFile") as TextBlock;
            tbFile.MouseDown += tbFile_MouseDown;
            imgDelete = GetTemplateChild("imgDelete") as Image;
            imgDelete.MouseLeftButtonDown += imgDelete_MouseLeftButtonDown;
            fpFile = GetTemplateChild("fpFile") as C1FilePicker;
            fpFile.SelectedFilesChanged += fpFile_SelectedFilesChanged;
        }

        void fpFile_SelectedFilesChanged(object sender, EventArgs e)
        {
            if (fpFile.SelectedFile != null && fpFile.SelectedFile.Length > 1024 * 1024 * MaxFileSize)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("COM0006", MaxFileSize), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                fpFile.ClearSelection();
            }

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory.ToString();
            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (Environment.CurrentDirectory.Contains("\\bin"))
                    Environment.CurrentDirectory = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf("\\bin"));
            }
        }

        void imgDelete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FileDeleteCommand != null && FileDeleteCommand.CanExecute(this))
            {
                FileDeleteCommand.Execute(WebFileInfo);
            }
        }

        void tbFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(WebFileSetID) && WebFileInfo != null)
            {
                FileDownloader fd = new FileDownloader();
                fd.Download(WebFileSetID, WebFileInfo.WebFileID, WebFileInfo.WebFileName);
            }
        }
    }
}
