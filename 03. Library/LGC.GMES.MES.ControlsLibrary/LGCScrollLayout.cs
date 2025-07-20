using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace LGC.GMES.MES.ControlsLibrary
{
    [ContentProperty("Content")]
    public class LGCScrollLayout : Control
    {
        TextBlock txtTitle;
        private Button btnSearch;
        public event RoutedEventHandler SearchClick;

        public static readonly DependencyProperty SearchAreaProperty = DependencyProperty.Register("SearchArea", typeof(object), typeof(LGCScrollLayout), new PropertyMetadata(null));
        public object SearchArea
        {
            get
            {
                return GetValue(SearchAreaProperty);
            }
            set
            {
                SetValue(SearchAreaProperty, value);
            }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(LGCScrollLayout), new PropertyMetadata(null));
        public object Content
        {
            get
            {
                return GetValue(ContentProperty);
            }
            set
            {
                SetValue(ContentProperty, value);
            }
        }

        static LGCScrollLayout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LGCScrollLayout), new FrameworkPropertyMetadata(typeof(LGCScrollLayout)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            btnSearch = GetTemplateChild("btnSearch") as Button;

            btnSearch.Click += (sender, arg) =>
            {
                if (SearchClick != null)
                    SearchClick(sender, arg);
            };

            txtTitle = GetTemplateChild("txtTitlePanel") as TextBlock;
            txtTitle.Text = string.Empty;
            if (this.Tag == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(this.Tag.ToString()))
            {
                txtTitle.Text = "Program Description :" + Environment.NewLine + Environment.NewLine + this.Tag.ToString();
            }

        }
    }
}
