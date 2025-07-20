using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class NumericBoxConverter : IValueConverter
    {

        //2019-10-01 오화백 러시아어 추가
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return double.NaN;

            if(LoginInfo.LANGID == "uk-UA" || LoginInfo.LANGID == "ru-RU")
            {
                value = value.ToString().Replace(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

}
