using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.ControlsLibrary
{
	public class CalendarDayButton : DependencyObject
    {
		public static readonly DependencyProperty ApplyStyleProperty = DependencyProperty.RegisterAttached("ApplyStyle", typeof(bool), typeof(CalendarDayButton), new PropertyMetadata(false, ApplyStylePropertyChanged));
		public static void SetApplyStyle(DependencyObject calendarDayButton, bool value)
		{
			calendarDayButton.SetValue(ApplyStyleProperty, value);
		}
		public static bool GetApplyStyle(DependencyObject calendarDayButton)
		{
			return (bool)calendarDayButton.GetValue(ApplyStyleProperty);
		}
		public static void ApplyStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (true.Equals(e.NewValue))
			{
				try
				{
					System.Windows.Controls.Primitives.CalendarDayButton btn = d as System.Windows.Controls.Primitives.CalendarDayButton;
					btn.Loaded += (sender, arg) =>
					{
						if (Grid.GetColumn(btn) == 0) // Sunday #FFEC486B
						{
							btn.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0x48, 0x6B));
						}
						else if (Grid.GetColumn(btn) == 6) // Saturday #FF00C9BD
						{
							btn.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xC9, 0xBD));
						}
					};
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.StackTrace);
				}
			}
		}
    }

	public class CalendarDayTextBlock : DependencyObject
	{
		public static readonly DependencyProperty ApplyStyleProperty = DependencyProperty.RegisterAttached("ApplyStyle", typeof(bool), typeof(CalendarDayTextBlock), new PropertyMetadata(false, ApplyStylePropertyChanged));
		public static void SetApplyStyle(DependencyObject calendarDayTextBlock, bool value)
		{
			calendarDayTextBlock.SetValue(ApplyStyleProperty, value);
		}
		public static bool GetApplyStyle(DependencyObject calendarDayTextBlock)
		{
			return (bool)calendarDayTextBlock.GetValue(ApplyStyleProperty);
		}
		public static void ApplyStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (true.Equals(e.NewValue))
			{
				try
				{
					TextBlock tb = d as TextBlock;
					tb.Loaded += (sender, arg) =>
					{
						if (Grid.GetColumn(tb.Parent as FrameworkElement) == 0) // Sunday #FFEC486B
						{
							tb.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0x48, 0x6B));
						}
						else if (Grid.GetColumn(tb.Parent as FrameworkElement) == 6) // Saturday #FF00C9BD
						{
							tb.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xC9, 0xBD));
						}
					};
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.StackTrace);
				}
			}
		}
	}
}
