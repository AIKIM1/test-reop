using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using C1.WPF;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class LGCPopupWindowExtension : DependencyObject
    {
        public static readonly DependencyProperty ApplyStyleProperty = DependencyProperty.RegisterAttached("ApplyStyle", typeof(bool), typeof(LGCPopupWindowExtension), new PropertyMetadata(false, ApplyStylePropertyChanged));

        public static void SetApplyStyle(DependencyObject cWindow, bool Value)
        {
            cWindow.SetValue(ApplyStyleProperty, Value);
        }

        public static bool GetApplyStyle(DependencyObject cWindow)
        {
            return (bool)cWindow.GetValue(ApplyStyleProperty);
        }

        private static void ApplyStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (true.Equals(e.NewValue))
            {
                try
                {
                    C1Window window = d as C1Window;
                    window.Dispatcher.BeginInvoke(new Action(() => window.Style = window.Resources["C1WindowStyle"] as Style));
                }
                catch (Exception ex)
                {
					Console.WriteLine(ex.StackTrace);
                }
            }
            else
            {
                try
                {
                    C1Window window = d as C1Window;
                    window.Style = null;
                }
                catch (Exception ex)
                {
					Console.WriteLine(ex.StackTrace);
				}
            }

            try
            {
                C1Window window = d as C1Window;
                window.Loaded -= window_Loaded;
                window.Loaded += window_Loaded;
            }
            catch (Exception ex)
            {
				Console.WriteLine(ex.StackTrace);
			}
        }

        static void window_Loaded(object sender, RoutedEventArgs e)
        {
            C1Window window = sender as C1Window;
            window.Loaded -= window_Loaded;

            window.CenterOnScreen();
        }

    }


    public class C1WindowExtension
    {
        public static readonly DependencyProperty ParameterProperty =
               DependencyProperty.RegisterAttached("Parameter",
                                                   typeof(string),
                                                   typeof(C1WindowExtension),
                                                   new FrameworkPropertyMetadata(null));

        public static string GetParameter(DependencyObject d)
        {
            return (string)d.GetValue(ParameterProperty);
        }

        public static void SetParameter(DependencyObject d, string value)
        {
            d.SetValue(ParameterProperty, value);
        }

        public static readonly DependencyProperty ParametersProperty =
              DependencyProperty.RegisterAttached("Parameters",
                                                  typeof(object[]),
                                                  typeof(C1WindowExtension),
                                                  new FrameworkPropertyMetadata(null));

        public static object[] GetParameters(DependencyObject d)
        {
            return (object[])d.GetValue(ParametersProperty);
        }

        public static void SetParameters(DependencyObject d, object[] value)
        {
            d.SetValue(ParametersProperty, value);
        }
    }
}
