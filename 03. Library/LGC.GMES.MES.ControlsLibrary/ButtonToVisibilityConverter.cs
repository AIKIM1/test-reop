using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class ButtonToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                string[] split = System.Convert.ToString(value).Split('|');

                if (split.Length > 1)
                {
                    switch (System.Convert.ToString(parameter))
                    {
                        case "1":
                            if (!split[0].Trim().Equals("01"))
                            {
                                return "Collapsed";
                            }
                            else
                            {
                                return "Visible";
                            }
                        case "2":
                            if (!split[1].Trim().Equals("02"))
                            {
                                return "Collapsed";
                            }
                            else
                            {
                                return "Visible";
                            }
                        case "3":
                            if (!split[2].Trim().Equals("03"))
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
                    if (System.Convert.ToString(parameter).Equals("4"))
                        return "Visible";
                    else
                        return "Collapsed";
                }           
            }
            else
            {
                return "Collapsed";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible.Equals(value) ? true : false;
        }
    }
    public class ConverterButtonForeColor : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                string[] split = System.Convert.ToString(value).Split('|');
                string[] split1;
                if (split.Length > 1)
                {
                    switch (System.Convert.ToString(parameter))
                    {
                        case "1":
                            if (!split[0].Trim().Equals("00"))
                            {
                                split1 = split[0].Trim().Split('^');
                                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(split1[0].Trim()));
                            }
                            else
                            {
                                return "Black";
                            }
                        case "2":
                            if (!split[1].Trim().Equals("00"))
                            {
                                split1 = split[1].Trim().Split('^');
                                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(split1[0].Trim()));
                            }
                            else
                            {
                                return "Black";
                            }
                        case "3":
                            if (!split[2].Trim().Equals("00"))
                            {
                                split1 = split[2].Trim().Split('^');
                                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(split1[0].Trim()));
                            }
                            else
                            {
                                return "Black";
                            }
                        default:
                            return "Collapsed";
                    }
                }
                else
                {
                    return "Black";
                }
            }
            else
            {
                return "Black";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "Black";
        }
    }

    public class ConverterButtonBackColor : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                string[] split = System.Convert.ToString(value).Split('|');
                string[] split1;

                if (split.Length > 1)
                {
                    switch (System.Convert.ToString(parameter))
                    {
                        case "1":
                            if (!split[0].Trim().Equals("00"))
                            {
                                split1 = split[0].Trim().Split('^');
                                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(split1[1].Trim()));
                            }
                            else
                            {
                                return "Black";
                            }
                        case "2":
                            if (!split[1].Trim().Equals("00"))
                            {
                                split1 = split[1].Trim().Split('^');
                                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(split1[1].Trim()));
                            }
                            else
                            {
                                return "Black";
                            }
                        case "3":
                            if (!split[2].Trim().Equals("00"))
                            {
                                split1 = split[2].Trim().Split('^');
                                return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(split1[1].Trim()));
                            }
                            else
                            {
                                return "Black";
                            }
                        default:
                            return "Gray";
                    }
                }
                else
                {
                     return "Gray";
                }
            }
            else
            {
                if (System.Convert.ToString(parameter).Equals("4"))
                    return "LightGray";
                else
                    return "Gray";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return "LightGray";
        }
    }
}
