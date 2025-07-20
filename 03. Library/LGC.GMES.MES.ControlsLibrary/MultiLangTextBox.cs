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

namespace LGC.GMES.MES.ControlsLibrary
{
    public class MultiLangTextBox : Control
    {
        private Grid LayoutRoot;
        private MultiLangTextPresenter presenter;
        private MultiLangEditor editor;

        public event EventHandler TextChanged;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MultiLangTextBox), new PropertyMetadata(string.Empty));
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

        public MultiLangTextBox()
        {
            this.DefaultStyleKey = typeof(MultiLangTextBox);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            (GetTemplateChild("editor") as MultiLangEditor).TextChanged += (s, arg) =>
                {
                    if (TextChanged != null)
                        TextChanged(this, null);
                };


            //LayoutRoot = GetTemplateChild("LayoutRoot") as Grid;
            //presenter = GetTemplateChild("presenter") as MultiLangTextPresenter;
            //editor = GetTemplateChild("editor") as MultiLangEditor;

            //LayoutRoot.GotFocus += presenter_GotFocus;
            //LayoutRoot.LostFocus += LayoutRoot_LostFocus;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focus();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            VisualStateManager.GoToState(this, "Focused", false);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            object obj = FocusManager.GetFocusedElement(this);
            //object obj = FocusManager.GetFocusedElement();
            if (obj != null && obj is FrameworkElement)
            {
                if (!findDescendent(this, obj))
                {
                    VisualStateManager.GoToState(this, "Unfocused", false);
                }
            }
        }

        public bool findDescendent(DependencyObject root, object targetChild)
        {
            if (root == targetChild)
            {
                return true;
            }
            else
            {
                int count = VisualTreeHelper.GetChildrenCount(root);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(root, i);
                    if (findDescendent(child, targetChild))
                        return true;
                }
            }

            return false;
        }
    }
}
