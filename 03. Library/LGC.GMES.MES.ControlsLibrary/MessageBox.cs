using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using System.Windows.Controls;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class MessageBox : DependencyObject
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageBox), new PropertyMetadata(null));
        public string Message
        {
            get
            {
                return GetValue(MessageProperty) as string;
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }

        public static readonly DependencyProperty TopMessageProperty = DependencyProperty.Register("TopMessage", typeof(string), typeof(MessageBox), new PropertyMetadata(null, TopMessagePropertyChanged));
        public string TopMessage
        {
            get
            {
                return GetValue(TopMessageProperty) as string;
            }
            set
            {
                SetValue(TopMessageProperty, value);
            }
        }
        public static void TopMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
        {
            d.SetValue(HasTopMessageProperty, (arg.NewValue != null && !string.IsNullOrEmpty(arg.NewValue.ToString())));
        }

        public static readonly DependencyProperty HasTopMessageProperty = DependencyProperty.Register("HasTopMessage", typeof(bool), typeof(MessageBox), new PropertyMetadata(false));
        public bool HasTopMessage
        {
            get
            {
                return (bool)GetValue(HasTopMessageProperty);
            }
            set
            {
                SetValue(HasTopMessageProperty, value);
            }
        }

        public static readonly DependencyProperty DetailMessageProperty = DependencyProperty.Register("DetailMessage", typeof(string), typeof(MessageBox), new PropertyMetadata(null, DetailMessagePropertyChanged));
        public string DetailMessage
        {
            get
            {
                return GetValue(DetailMessageProperty) as string;
            }
            set
            {
                SetValue(DetailMessageProperty, value);
            }
        }
        public static void DetailMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs arg)
        {
            d.SetValue(HasDetailMessageProperty, (arg.NewValue != null && !string.IsNullOrEmpty(arg.NewValue.ToString())));
        }

        public static readonly DependencyProperty HasDetailMessageProperty = DependencyProperty.Register("HasDetailMessage", typeof(bool), typeof(MessageBox), new PropertyMetadata(false));
        public bool HasDetailMessage
        {
            get
            {
                return (bool)GetValue(HasDetailMessageProperty);
            }
            set
            {
                SetValue(HasDetailMessageProperty, value);
            }
        }


        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(MessageBox), new PropertyMetadata(null));
        public string Caption
        {
            get
            {
                return GetValue(CaptionProperty) as string;
            }
            set
            {
                SetValue(CaptionProperty, value);
            }
        }

        public static readonly DependencyProperty DetailFontSizeProperty = DependencyProperty.Register("DetailFontSize", typeof(double), typeof(MessageBox), new PropertyMetadata(12D));
        public double DetailFontSize
        {
            get
            {
                return Convert.ToDouble(GetValue(DetailFontSizeProperty));
            }
            set
            {
                SetValue(DetailFontSizeProperty, value);
            }
        }

        public static readonly DependencyProperty DetailWidthProperty = DependencyProperty.Register("DetailWidth", typeof(double), typeof(MessageBox), new PropertyMetadata(Double.NaN));
        public double DetailWidth
        {
            get
            {
                return Convert.ToDouble(GetValue(DetailWidthProperty));
            }
            set
            {
                SetValue(DetailWidthProperty, value);
            }
        }

        public static readonly DependencyProperty DetailHeightProperty = DependencyProperty.Register("DetailHeight", typeof(double), typeof(MessageBox), new PropertyMetadata(Double.NaN));
        public double DetailHeight
        {
            get
            {
                return Convert.ToDouble(GetValue(DetailHeightProperty));
            }
            set
            {
                SetValue(DetailHeightProperty, value);
            }
        }

        public static readonly DependencyProperty HasCancelButtonProperty = DependencyProperty.Register("HasCancelButton", typeof(bool), typeof(MessageBox), new PropertyMetadata(false));
        public bool HasCancelButton
        {
            get
            {
                return (bool)GetValue(HasCancelButtonProperty);
            }
            set
            {
                SetValue(HasCancelButtonProperty, value);
            }
        }

        public static readonly DependencyProperty HasYesNoButtonProperty = DependencyProperty.Register("HasYesNoButton", typeof(bool), typeof(MessageBox), new PropertyMetadata(false));
        public bool HasYesNoButton
        {
            get
            {
                return (bool)GetValue(HasYesNoButtonProperty);
            }
            set
            {
                SetValue(HasYesNoButtonProperty, value);
            }
        }

        public static readonly DependencyProperty HasWarningIconProperty = DependencyProperty.Register("HasWarningIcon", typeof(bool), typeof(MessageBox), new PropertyMetadata(false));
        public bool HasWarningIcon
        {
            get
            {
                return (bool)GetValue(HasWarningIconProperty);
            }
            set
            {
                SetValue(HasWarningIconProperty, value);
            }
        }

        public static readonly DependencyProperty HasCheckActionProperty = DependencyProperty.Register("HasCheckAction", typeof(bool), typeof(MessageBox), new PropertyMetadata(false));
        public bool HasCheckAction
        {
            get
            {
                return (bool)GetValue(HasCheckActionProperty);
            }
            set
            {
                SetValue(HasCheckActionProperty, value);
            }
        }

        public static readonly DependencyProperty CheckActionProperty = DependencyProperty.Register("CheckAction", typeof(bool), typeof(MessageBox), new PropertyMetadata(false));
        public bool CheckAction
        {
            get
            {
                return (bool)GetValue(CheckActionProperty);
            }
            set
            {
                SetValue(CheckActionProperty, value);
            }
        }

        public static readonly DependencyProperty CheckActionContentProperty = DependencyProperty.Register("CheckActionContent", typeof(string), typeof(MessageBox), new PropertyMetadata(null));
        public string CheckActionContent
        {
            get
            {
                return GetValue(CheckActionContentProperty) as string;
            }
            set
            {
                SetValue(CheckActionContentProperty, value);
            }
        }

        private C1Window window = new C1Window();

        public static void Show(string message, string detail = "", string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxIcon icon = MessageBoxIcon.None, Action<MessageBoxResult> callback = null, bool? isAutoClosing = false, bool? isUpSizing = false, string topMessage = "")
        {
            MessageBox data = new MessageBox();
            data.Message = message;
            data.TopMessage = topMessage;
            data.DetailMessage = detail;
            data.Caption = caption;
            if (button == MessageBoxButton.OKCancel)
                data.HasCancelButton = true;
            if (icon == MessageBoxIcon.Warning)
                data.HasWarningIcon = true;
            if (button == MessageBoxButton.YesNo)    // OK NO로 셋팅            
                data.HasYesNoButton = true;
        

            C1Window window = data.window;
            window.Header = data.Caption;
            window.Style = Application.Current.Resources.MergedDictionaries[10]["MessageBoxStyle"] as Style;
            window.Content = data;

            if (isUpSizing == true)
            {
                data.DetailFontSize = 17;
                data.DetailWidth = 500;
                data.DetailHeight = 150;
            }

            window.Closed += (sender, arg) =>
            {
                if (callback != null)
                    callback(window.DialogResult);
            };

            // [E20240514-000648] 알림창에서 ctrl+c 에서 카피
            MessageCopyFunc(window, message);

                //ok cancel 일때 엔터:ok esc:cancel (방향키 <:ok선택 >:cancel 선택일때 엔터 설정)
            if (button == MessageBoxButton.OKCancel)
                window.KeyDown += (sender, arg) =>
                {
                    //FocusNavigationDirection focusDirection = new System.Windows.Input.FocusNavigationDirection();

                    if (arg.Key == Key.Enter)
                    {
                        window.DialogResult = MessageBoxResult.OK;
                    }
                    else if (arg.Key == Key.Escape)
                    {
                        window.DialogResult = MessageBoxResult.Cancel;
                    }
                    else if (arg.Key == Key.Left)
                    {
                        //focusDirection = System.Windows.Input.FocusNavigationDirection.Left;
                    }
                    else if (arg.Key == Key.Right)
                    {
                        //focusDirection = System.Windows.Input.FocusNavigationDirection.Right;
                    }
                    //TraversalRequest request = new TraversalRequest(focusDirection);

                    // 방향키 포지션 이동.
                    // if (window) // 위치 확인 적용 해야함 뒤 컨트롤 까지 영향있음
                    //window.MoveFocus(request);
                };
            //ok No 일때 엔터:ok esc:No (방향키 <:ok선택 >:No 선택일때 엔터 설정)
            if (button == MessageBoxButton.YesNo)
                window.KeyDown += (sender, arg) =>
                {
                    //FocusNavigationDirection focusDirection = new System.Windows.Input.FocusNavigationDirection();

                    if (arg.Key == Key.Enter)
                    {
                        window.DialogResult = MessageBoxResult.OK;
                    }
                    else if (arg.Key == Key.Escape)
                    {
                        window.DialogResult = MessageBoxResult.No;
                    }
                    else if (arg.Key == Key.Left)
                    {
                        //focusDirection = System.Windows.Input.FocusNavigationDirection.Left;
                    }
                    else if (arg.Key == Key.Right)
                    {
                        //focusDirection = System.Windows.Input.FocusNavigationDirection.Right;
                    }
                    //TraversalRequest request = new TraversalRequest(focusDirection);

                    // 방향키 포지션 이동.
                    // if (window) // 위치 확인 적용 해야함 뒤 컨트롤 까지 영향있음
                    //window.MoveFocus(request);
                };

            //OK시 엔터키나 ESC키 정상 닫기
            if (button == MessageBoxButton.OK)
                window.KeyDown += (sender, arg) =>
                {
                    if (arg.Key == Key.Enter)
                    {
                        window.DialogResult = MessageBoxResult.OK;
                    }
                    else if (arg.Key == Key.Escape)
                    {
                        window.DialogResult = MessageBoxResult.OK;
                    }
                };

            if (button == MessageBoxButton.OK && isAutoClosing == true)
            {
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2d);
                timer.Tick += (sender, arg) =>
                {
                    timer.Stop();
                    window.Close();
                };
                timer.Start();
            }
            //OK시 2초후 닫힘 설정 open후부터 해야되는데 어느이벤트인지.
            //if (button == MessageBoxButton.OK)
            //    window.MouseEnter += (sender, arg)  => 
            //    {
            //        while (true)
            //        {
            //            Thread.Sleep(2000);
            //            break;
            //        }
            //        window.DialogResult = MessageBoxResult.OK;
            //    };

            //if (button == MessageBoxButton.OK)
            //    window.MouseLeave += (sender, arg) =>
            //    {
            //        while (true)
            //        {
            //            Thread.Sleep(2000);
            //            break;
            //        }
            //        window.DialogResult = MessageBoxResult.OK;
            //    };

            window.ShowModal();
            window.CenterOnScreen();

            // 2초후 바로닫힘. 열림.이벤트후 실행해야됨.
            //if (button == MessageBoxButton.OK)
            //{
            //    Thread.Sleep(2000);
            //    window.Close();
            //}  
        }

        private static void Window_Initialized(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        // INS 염규범S : C20170902_75124 - 폴딩공정 라벨발행 개선 
        public static void showPrintFD(string message, MessageBoxButton button = MessageBoxButton.OK, MessageBoxIcon icon = MessageBoxIcon.None, Action < MessageBoxResult, bool> callback = null, bool? isAutoClosing = true, bool? isUpSizing = false)
        {
            MessageBox data = new MessageBox();

            data.Message = message;

            if (button == MessageBoxButton.OKCancel)
                data.HasCancelButton = true;
            if (icon == MessageBoxIcon.Warning)
                data.HasWarningIcon = true;

            C1Window window = data.window;
            window.Header = data.Caption;
            window.Style = Application.Current.Resources.MergedDictionaries[10]["MessageBoxStyle"] as Style;
            window.Content = data;

            if (isUpSizing == true)
            {
                data.DetailFontSize = 17;
                data.DetailWidth = 500;
                data.DetailHeight = 150;
            }

            window.Closed += (sender, arg) =>
            {
                if (callback != null)
                    callback(window.DialogResult, data.CheckAction);
            };
            window.KeyDown += (sender, arg) =>
            {
                if (arg.Key == Key.Enter)
                {
                    window.DialogResult = MessageBoxResult.OK;

                }
                else if (arg.Key == Key.Escape)
                {
                    window.DialogResult = MessageBoxResult.Cancel;

                }
            };
            if (button == MessageBoxButton.OK && isAutoClosing == true)
            {
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2d);
                timer.Tick += (sender, arg) =>
                {
                    timer.Stop();
                    window.Close();
                };
                timer.Start();
            }
            window.ShowModal();
            window.CenterOnScreen();
        }

        public static void Show(string message, bool checkBoxVisible, bool checkSelect = false, string checkContent = "", string detail = "", string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxIcon icon = MessageBoxIcon.None, Action<MessageBoxResult, bool> callback = null, bool? isAutoClosing = false, bool? isUpSizing = false, string topMessage = "")
        {
            MessageBox data = new MessageBox();
            data.Message = message;
            data.TopMessage = topMessage;
            data.DetailMessage = detail;
            data.Caption = caption;
            data.HasCheckAction = checkBoxVisible;
            data.CheckAction = checkSelect;
            data.CheckActionContent = checkContent;
            if (button == MessageBoxButton.OKCancel)
                data.HasCancelButton = true;
            if (icon == MessageBoxIcon.Warning)
                data.HasWarningIcon = true;

            C1Window window = data.window;
            window.Header = data.Caption;
            window.Style = Application.Current.Resources.MergedDictionaries[10]["MessageBoxStyle"] as Style;
            window.Content = data;

            if (isUpSizing == true)
            {
                data.DetailFontSize = 17;
                data.DetailWidth = 500;
                data.DetailHeight = 150;
            }

            window.Closed += (sender, arg) =>
            {
                if (callback != null)
                    callback(window.DialogResult, data.CheckAction);
            };

            // [E20240514-000648] 알림창에서 ctrl+c 에서 카피
            MessageCopyFunc(window, message);

            window.KeyDown += (sender, arg) =>
            {
                if (arg.Key == Key.Enter)
                {
                    window.DialogResult = MessageBoxResult.OK;

                }
                else if (arg.Key == Key.Escape)
                {
                    window.DialogResult = MessageBoxResult.Cancel;

                }
            };
            if (button == MessageBoxButton.OK && isAutoClosing == true)
            {
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2d);
                timer.Tick += (sender, arg) =>
                {
                    timer.Stop();
                    window.Close();
                };
                timer.Start();
            }
            window.ShowModal();
            window.CenterOnScreen();
        }

        public static void ShowKeyEnter(string message, string detail = "", string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxIcon icon = MessageBoxIcon.None, Action<MessageBoxResult> callback = null, bool? isAutoClosing = false)
        {
            MessageBox data = new MessageBox();
            data.Message = message;
            data.DetailMessage = detail;
            data.Caption = caption;
            if (button == MessageBoxButton.OKCancel)
                data.HasCancelButton = true;
            if (icon == MessageBoxIcon.Warning)
                data.HasWarningIcon = true;

            C1Window window = data.window;
            window.KeyDown += (sender, arg) =>
            {
                if (arg.Key == Key.Enter)
                {
                    window.DialogResult = MessageBoxResult.OK;

                }
                else if (arg.Key == Key.Escape)
                {
                    window.DialogResult = MessageBoxResult.Cancel;

                }
            };
            if (button == MessageBoxButton.OK && isAutoClosing == true)
            {
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2d);
                timer.Tick += (sender, arg) =>
                {
                    timer.Stop();
                    window.Close();
                };
                timer.Start();
            }

            window.Header = data.Caption;
            window.Style = Application.Current.Resources.MergedDictionaries[10]["MessageBoxStyle"] as Style;
            window.Content = data;
            window.Closed += (sender, arg) =>
            {
                if (callback != null)
                    callback(window.DialogResult);
            };
            window.ShowModal();
            window.CenterOnScreen();
        }
        public static void ShowNoEnter(string message, string detail = "", string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxIcon icon = MessageBoxIcon.None, Action<MessageBoxResult> callback = null, bool? isAutoClosing = false)
        {
            MessageBox data = new MessageBox();
            data.Message = message;
          //  data.TopMessage = topMessage;
            data.DetailMessage = detail;
            data.Caption = caption;
            if (button == MessageBoxButton.OKCancel)
                data.HasCancelButton = true;
            if (icon == MessageBoxIcon.Warning)
                data.HasWarningIcon = true;

            C1Window window = data.window;
            window.Header = data.Caption;
            window.Style = Application.Current.Resources.MergedDictionaries[10]["MessageBoxStyle"] as Style;
            window.Content = data;
            
            window.Closed += (sender, arg) =>
            {
                if (callback != null)
                    callback(window.DialogResult);
            };

            //ok cancel 일때 엔터:ok esc:cancel (방향키 <:ok선택 >:cancel 선택일때 엔터 설정)
            if (button == MessageBoxButton.OKCancel)
                window.KeyDown += (sender, arg) =>
                {
                    if (arg.Key == Key.Space)
                    {
                        window.DialogResult = MessageBoxResult.OK;
                    }
                    else if (arg.Key == Key.Escape)
                    {
                        window.DialogResult = MessageBoxResult.Cancel;
                    }
                    else if (arg.Key == Key.Left)
                    {
                        //focusDirection = System.Windows.Input.FocusNavigationDirection.Left;
                    }
                    else if (arg.Key == Key.Right)
                    {
                        //focusDirection = System.Windows.Input.FocusNavigationDirection.Right;
                    }
                    //TraversalRequest request = new TraversalRequest(focusDirection);

                    // 방향키 포지션 이동.
                    // if (window) // 위치 확인 적용 해야함 뒤 컨트롤 까지 영향있음
                    //window.MoveFocus(request);
                };

            //OK시 엔터키나 ESC키 정상 닫기
            if (button == MessageBoxButton.OK)
                window.KeyDown += (sender, arg) =>
                {
                    if (arg.Key == Key.Space)
                    {
                        window.DialogResult = MessageBoxResult.OK;
                    }
                    else if (arg.Key == Key.Enter)
                    {
                        arg.Handled = true;
                    }
                };
            
            window.ShowModal();
            window.CenterOnScreen();
            window.BringToFront();
        }

        // [E20240514-000648] 알림창에서 ctrl+c 에서 카피
        public static void MessageCopyFunc(C1Window window, string message)
        {
            window.KeyDown += (sender, arg) =>
            {
                try
                {
                    if (arg.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        Clipboard.SetText(message);
                    }
                }
                catch (Exception ex)
                {
                    //Util.MessageException(ex);
                }

            };
        }

        public class OKCommand : ICommand
        {
            private C1Window window;

            public OKCommand(C1Window window)
            {
                this.window = window;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                window.DialogResult = MessageBoxResult.OK;
            }
        }
        public OKCommand OKCommandInst
        {
            get
            {
                return new OKCommand(window);
            }
        }

        public class CancelCommand : ICommand
        {
            private C1Window window;

            public CancelCommand(C1Window window)
            {
                this.window = window;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                window.DialogResult = MessageBoxResult.Cancel;
            }
        }
        public CancelCommand CancelCommandInst
        {
            get
            {
                return new CancelCommand(window);
            }
        }

        public class YESCommand : ICommand
        {
            private C1Window window;

            public YESCommand(C1Window window)
            {
                this.window = window;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                window.DialogResult = MessageBoxResult.Yes;
            }
        }
        public YESCommand YESCommandInst
        {
            get
            {
                return new YESCommand(window);
            }
        }

        public class NOCommand : ICommand
        {
            private C1Window window;

            public NOCommand(C1Window window)
            {
                this.window = window;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                window.DialogResult = MessageBoxResult.No;
            }
        }
        public NOCommand NOCommandInst
        {
            get
            {
                return new NOCommand(window);
            }
        }
    }

    public enum MessageBoxIcon { None, Warning }
}
