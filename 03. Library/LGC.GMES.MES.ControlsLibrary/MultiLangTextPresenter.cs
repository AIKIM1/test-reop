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
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class MultiLangTextPresenter : Control
    {
        private TextBlock tb;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MultiLangTextPresenter), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }
        private static void TextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string value = string.Empty;
            if (e.NewValue != null)
            {
                value = (string)e.NewValue;
            }
            else
            {
                value = string.Empty;
            }

            string[] partArray = value.Split('|');
            try
            {
                (d as MultiLangTextPresenter).DisplayText = string.Empty;
                foreach (string part in partArray)
                {
                    string[] partDetail = part.Split('\\');
                    if (LoginInfo.LANGID.Equals(partDetail[0]))
                    {
                        (d as MultiLangTextPresenter).DisplayText = partDetail[1];
                        break;
                    }
                }
            }
            catch (Exception parseException)
            {
            }
        }

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register("DisplayText", typeof(string), typeof(MultiLangTextPresenter), new PropertyMetadata(null));
        public string DisplayText
        {
            get
            {
                return (string)GetValue(DisplayTextProperty);
            }
            set
            {
                SetValue(DisplayTextProperty, value);
            }
        }

        public MultiLangTextPresenter()
        {
            this.DefaultStyleKey = typeof(MultiLangTextPresenter);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
