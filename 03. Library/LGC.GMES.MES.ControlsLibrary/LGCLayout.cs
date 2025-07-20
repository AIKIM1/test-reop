using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.Win32.SafeHandles;

namespace LGC.GMES.MES.ControlsLibrary
{
    [ContentProperty("Content")]
    public class LGCLayout : Control, IDisposable
    {
        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        TextBlock txtTitle;

        ~LGCLayout()
        {
            Dispose(false);
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(LGCLayout), new PropertyMetadata(null));
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        static LGCLayout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LGCLayout), new FrameworkPropertyMetadata(typeof(LGCLayout)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            txtTitle = GetTemplateChild("txtTitlePanel") as TextBlock;
            txtTitle.Text = string.Empty;

            if (this.Tag == null)
                return;

            if (!string.IsNullOrEmpty(this.Tag.ToString()))
                txtTitle.Text = "Program Description :" + Environment.NewLine + Environment.NewLine + this.Tag.ToString();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                handle.Dispose();

            disposed = true;
        }
    }
}