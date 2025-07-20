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
using C1.WPF.C1Chart;

namespace LGC.GMES.MES.ControlsLibrary
{
	public class ChartDataSeries : DependencyObject
	{
		public static DependencyProperty ApplyStyleProperty = DependencyProperty.RegisterAttached("ApplyStyle", typeof(bool), typeof(ChartDataSeries), new PropertyMetadata(false, ApplyStylePropertyChanged));
		public static void SetApplyStyle(DependencyObject dataSeries, bool value)
		{
			dataSeries.SetValue(ApplyStyleProperty, value);
		}
		public static bool GetApplyStyle(DependencyObject dataSeries)
		{
			return (bool)dataSeries.GetValue(ApplyStyleProperty);
		}
		public static void ApplyStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (true.Equals(e.NewValue))
			{
				try
				{
					DataSeries ds = d as DataSeries;

					ds.PlotElementLoaded += (sender, arg) =>
					{
						var pe = (PlotElement)sender;

						if (!(pe is Lines)) // Line이 아닐 경우만 테두리 제거
						{
							pe.StrokeThickness = 0;
							pe.StrokeDashOffset = 0;
							pe.Stroke = new SolidColorBrush(Colors.Transparent);
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

	public class ChartLegend : DependencyObject
	{
		public static DependencyProperty ApplyStyleProperty = DependencyProperty.RegisterAttached("ApplyStyle", typeof(bool), typeof(ChartLegend), new PropertyMetadata(false, ApplyStylePropertyChanged));
		public static void SetApplyStyle(DependencyObject c1Chart, bool value)
		{
			c1Chart.SetValue(ApplyStyleProperty, value);
		}
		public static bool GetApplyStyle(DependencyObject c1Chart)
		{
			return (bool)c1Chart.GetValue(ApplyStyleProperty);
		}
		public static void ApplyStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (true.Equals(e.NewValue))
			{
				try
				{
					C1Chart c = d as C1Chart;
					var cl = c.Children.Count > 0 ? c.Children[0] as C1ChartLegend : null;

					double clHeight = 0;

					if (cl != null)
					{
						cl.Loaded += (sender, arg) =>
						{
							clHeight = cl.ActualHeight + 30;
							cl.Margin = new Thickness(10, c.ActualHeight - clHeight, 0, 0);
						};
					}

					c.SizeChanged += (sender, arg) =>
					{
						cl = c.Children.Count > 0 ? c.Children[0] as C1ChartLegend : null;

						if (cl != null) cl.Margin = new Thickness(10, c.ActualHeight - clHeight, 0, 0);
					};

					c.LayoutUpdated += (sender, arg) =>
					{
						cl = c.Children.Count > 0 ? c.Children[0] as C1ChartLegend : null;

						if (cl != null)
						{
							clHeight = cl.ActualHeight + 30;
							cl.Margin = new Thickness(10, c.ActualHeight - clHeight, 0, 0);
						}
						for (int i = 0; i < c.Data.Children.Count; i++)
						{
							c.Data.Children[i].Display = SeriesDisplay.ShowNaNGap;
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
