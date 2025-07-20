using System;
using System.Collections;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ControlsLibrary
{
    public delegate void FileSetUploadCompleteHandler(FileSetHandler sender, bool success);
    public class FileSetHandler : Control
    {
        ListBox lbFileList;
        Button btnAdd;
        private Button btnDeleteAll;

        public event FileSetUploadCompleteHandler FileSetUploadComplete;

        public class FileRemoveCommand : ICommand
        {
            private FileSetHandler fileSetHandler;

            internal FileRemoveCommand(FileSetHandler fileSetHandler)
            {
                this.fileSetHandler = fileSetHandler;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                WebFileInfo fileInfo = parameter as WebFileInfo;
                fileSetHandler.WebFileSet.WebFileList.Remove(fileInfo);
            }
        }
        public static DependencyProperty FileRemoveCommandProperty = DependencyProperty.Register("FileRemoveCommnad", typeof(FileSetHandler.FileRemoveCommand), typeof(FileSetHandler), new PropertyMetadata(null));
        public FileRemoveCommand FileRemoveCommnad
        {
            get
            {
                return GetValue(FileRemoveCommandProperty) as FileRemoveCommand;
            }
        }

        public static DependencyProperty WebFileSetProperty = DependencyProperty.Register("WebFileSet", typeof(WebFileSet), typeof(FileSetHandler), new PropertyMetadata(new WebFileSet()));
        public WebFileSet WebFileSet
        {
            get
            {
                return GetValue(WebFileSetProperty) as WebFileSet;
            }
            set
            {
                SetValue(WebFileSetProperty, value);
            }
        }

        public static DependencyProperty IsReadonlyProperty = DependencyProperty.Register("IsReadonly", typeof(bool), typeof(FileSetHandler), new PropertyMetadata(false));
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

        public static readonly DependencyProperty MaxFileSizeProperty = DependencyProperty.Register("MaxFileSize", typeof(int), typeof(FileSetHandler), new PropertyMetadata(50));
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

        public FileSetHandler()
        {
            this.DefaultStyleKey = typeof(FileSetHandler);

            SetValue(FileRemoveCommandProperty, new FileRemoveCommand(this));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            lbFileList = this.GetTemplateChild("lbFileList") as ListBox;
            btnAdd = this.GetTemplateChild("btnAdd") as Button;
            btnDeleteAll = this.GetTemplateChild("btnDeleteAll") as Button;
            
            btnAdd.Click += btnAdd_Click;
            btnDeleteAll.Click += btnDeleteAll_Click;
        }

        void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            WebFileSet.WebFileList.Clear();
        }

        void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            WebFileSet.Add(new WebFileInfo());
        }

        public void Save()
        {
            FileUploader uploader = new FileUploader();
            uploader.UploadCompleted += (s, arg) =>
                {
                    if (FileSetUploadComplete != null)
                        FileSetUploadComplete(this, arg.Success);
                };
            uploader.UploadAsync(WebFileSet);
        }
    }
}
