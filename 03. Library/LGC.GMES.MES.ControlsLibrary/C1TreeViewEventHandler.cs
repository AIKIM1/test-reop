﻿using System;
using System.Collections.Specialized;
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

using C1.WPF;
using C1.WPF.C1Chart;

namespace LGC.GMES.MES.ControlsLibrary
{
	public class C1TreeViewItemLine : DependencyObject
	{
		public static DependencyProperty ApplyStyleProperty = DependencyProperty.RegisterAttached("ApplyStyle", typeof(bool), typeof(C1TreeViewItemLine), new PropertyMetadata(false, ApplyStylePropertyChanged));
		public static void SetApplyStyle(DependencyObject treeViewItem, bool value)
		{
			treeViewItem.SetValue(ApplyStyleProperty, value);
		}
		public static bool GetApplyStyle(DependencyObject treeViewItem)
		{
			return (bool)treeViewItem.GetValue(ApplyStyleProperty);
		}
		public static void ApplyStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//if (true.Equals(e.NewValue))
			//{
			//	try
			//	{
            //      C1TreeViewItem tvi = d as C1TreeViewItem;
			//		(tvi.Items as INotifyCollectionChanged).CollectionChanged += (sender, arg) =>
			//		{
			//			foreach (C1TreeViewItem item in tvi.Items)
			//			{
			//				if (item != null)
			//				{
			//					item.Style = null;
			//					item.Style = item.FindResource("C1TreeViewItemStyle") as Style;
			//				}
			//			}
			//		};
			//	}
			//	catch (Exception ex)
			//	{
			//		Console.WriteLine(ex.StackTrace);
			//	}
			//}
		}
	}

}
