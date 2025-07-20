using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.MainFrame.Controls
{
    /// <summary>
    /// FlowMessage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FlowMessage : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FlowMessage), new PropertyMetadata(null));
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

        public static readonly DependencyProperty IsUrgentProperty = DependencyProperty.Register("IsUrgent", typeof(bool), typeof(FlowMessage), new PropertyMetadata(false, IsUrgentPropertyChanged));
        public bool IsUrgent
        {
            get
            {
                return (bool)GetValue(IsUrgentProperty);
            }
            set
            {
                SetValue(IsUrgentProperty, value);
            }
        }
        private static void IsUrgentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlowMessage msg = d as FlowMessage;
            if (e.NewValue.Equals(true))
            {
                msg.normal.Foreground = msg.grd.Resources["UrgentMessageBrush"] as SolidColorBrush;
                msg.tb.Foreground = msg.grd.Resources["UrgentMessageBrush"] as SolidColorBrush;
            }
            else
            {
                msg.normal.Foreground = msg.grd.Resources["NormalMessageBrush"] as SolidColorBrush;
                msg.tb.Foreground = msg.grd.Resources["NormalMessageBrush"] as SolidColorBrush;
            }
        }

        private Timer timer = null;
        private static readonly int maxAnimationEndDelayTick = 60;
        private int animationEndDelayTick;

        public FlowMessage()
        {
            InitializeComponent();

            normal.SetBinding(TextBlock.TextProperty, new Binding() { Source = this, Path = new PropertyPath("Text"), Mode = BindingMode.OneWay });
            tb.SetBinding(TextBlock.TextProperty, new Binding() { Source = this, Path = new PropertyPath("Text"), Mode = BindingMode.OneWay });
        }

        private void Grid_MouseEnter_1(object sender, MouseEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            normal.Visibility = Visibility.Collapsed;
            mouseEntered.Visibility = Visibility.Visible;
            mouseEntered.ScrollToHorizontalOffset(0);
            animationEndDelayTick = 0;
            timer = new Timer((state) =>
                {
                    mouseEntered.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (mouseEntered.HorizontalOffset + 1 > mouseEntered.ExtentWidth - mouseEntered.ActualWidth)
                            {
                                mouseEntered.ScrollToHorizontalOffset(mouseEntered.ExtentWidth - mouseEntered.ActualWidth);
                                animationEndDelayTick++;
                                if (animationEndDelayTick > maxAnimationEndDelayTick)
                                    mouseEntered.ScrollToHorizontalOffset(0);
                            }
                            else
                            {
                                mouseEntered.ScrollToHorizontalOffset(mouseEntered.HorizontalOffset + 1);
                            }
                        }));
                }, null, 50, 50);
        }

        private void Grid_MouseLeave_1(object sender, MouseEventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }

            normal.Visibility = Visibility.Visible;
            mouseEntered.Visibility = Visibility.Collapsed;
        }
    }
}
