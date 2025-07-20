using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace LGC.GMES.MES.ControlsLibrary
{
	public class TreeLineVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value != null)
			{
				TreeViewItem tvi = value as TreeViewItem;

				if (tvi != null && tvi.Parent is TreeView)
				{
					return System.Windows.Visibility.Collapsed;
				}
				else
				{
					return System.Windows.Visibility.Visible;
				}
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	public class TreeLineCheckLastItemConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			TreeViewItem item = (TreeViewItem)value;
			ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
			return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
