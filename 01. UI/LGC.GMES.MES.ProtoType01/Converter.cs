using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace LGC.GMES.MES.ProtoType01
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType.Name == "Brush" && value is Color)
            {
                return new SolidColorBrush((Color)value);
            }
            if (targetType.Name == "Color" && value is SolidColorBrush)
            {
                return ((SolidColorBrush)value).Color;
            }
            return value;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

    }

    public class ConverterTrayCellIDBackColor : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                switch (value.ToString())
                {
                    case "SC":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                    case "NR":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                    case "DL":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D941C5"));
                    case "ID":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                    case "PD":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500"));
                    case "NI":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                    default:
                        return "Gray";
                }
            }
            else
            {
                return "Gray";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "Gray";
        }
    }

    public class ConverterTrayCellIDText : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                string[] split = System.Convert.ToString(value).Split(',');
                switch (System.Convert.ToString(parameter))
                {
                    case "0":
                        if (split.Length < 1)
                        {
                            return null;
                        }
                        else
                        {
                            return split[0];
                        }
                    case "1":
                        if (split.Length < 2)
                        {
                            return null;
                        }
                        else
                        {
                            return split[1];
                        }
                    case "2":
                        if (split.Length < 3)
                        {
                            return null;
                        }
                        else
                        {
                            return split[2];
                        }
                    case "3":
                        if (split.Length < 4)
                        {
                            return null;
                        }
                        else
                        {
                            return split[3];
                        }
                    case "4":
                        if (split.Length < 5)
                        {
                            return null;
                        }
                        else
                        {
                            return split[4];
                        }
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class ConverterTrayCellIDTextVisibility : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace(value.ToString()))
            {

                string[] split = System.Convert.ToString(value).Split(',');
                switch (System.Convert.ToString(parameter))
                {
                    case "0":
                        if (split.Length < 1)
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    case "1":
                        if (split.Length < 2)
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    case "2":
                        if (split.Length < 3)
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    case "3":
                        if (split.Length < 4)
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    case "4":
                        if (split.Length < 5)
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    default:
                        return "Collapsed";
                }
            }
            else
            {
                return "Collapsed";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "Collapsed";
        }
    }

    public class ConverterTrayCellIDForeColor : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                switch (value.ToString())
                {
                    case "SC":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        //return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                    case "NR":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                    case "DL":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        //return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D941C5"));
                    case "ID":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                    case "PD":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        //return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500"));
                    case "NI":
                        return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                    default:
                        return "Gray";
                }
            }
            else
            {
                return "Gray";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "Gray";
        }
    }

    public class ActualWidthConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            System.Windows.Controls.TextBlock txtb = value as System.Windows.Controls.TextBlock;

            C1.WPF.DataGrid.DataGridCellPresenter pre = txtb.Parent as C1.WPF.DataGrid.DataGridCellPresenter;

            if (pre != null)
            {
                if (pre.Cell.Column.ActualWidth != 0)
                {
                    return pre.Cell.Column.ActualWidth;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }

    }
}
