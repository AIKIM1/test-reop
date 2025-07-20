using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ControlsLibrary.Extensions
{
    public static class ObjectExtension
    {
        
        public static bool IsNullOrEmpty(this object item)
        {
            bool flag;
            try
            {
                if (item != null)
                {
                    flag = (!(item.ToString().Trim() == "") ? false : true);
                }
                else
                {
                    flag = true;
                }
            }
            catch //(Exception exception)
            {
                flag = false;
            }
            return flag;
        }
        
        public static void Each<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var elem in list)
                action(elem);
        }

        public static bool ContainsAll<T>(this IList<T> list, IEnumerable<T> subList)
        {
            foreach (var t in subList)
                if (!list.Contains(t)) return false;
            return true;
        }

        public static T[] EmptyArray<T>()
        {
            return new T[] { };
        }
        
        public static T ChangeType<T>(this object value, IFormatProvider provider) where T : IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T), provider);
        }
        public static T ChangeType<T>(this object value) where T : IConvertible
        {
            return ChangeType<T>(value, System.Globalization.CultureInfo.CurrentCulture);
        }
        public static bool SafeToBoolean(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return false;
            }

            string s = obj.ToString().ToUpper();

            if (s.Equals("TRUE") || s.Equals("Y") || s.Equals("1"))
            {
                return true;
            }

            return false;
        }

        public static string NullToString(this object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return string.Empty;
            }
            else
            {
                return obj.ToString();
            }
        }

        public static double SafeToDouble(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return 0;
            }


            return Convert.ToDouble(obj, System.Globalization.CultureInfo.CurrentCulture);
        }

        public static int SafeToInt32(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return 0;
            }

            return Convert.ToInt32(obj, System.Globalization.CultureInfo.CurrentCulture);
        }

        public static decimal SafeToDecimal(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString().Trim().Length == 0)
            {
                return 0;
            }

            decimal val;
            if (decimal.TryParse(obj.ToString(), out val))
            {
                return val;
            }

            return 0;


            // return Convert.ToDecimal(obj, CultureInfo.CurrentCulture);
        }

        public static string SafeToString(this object obj, bool trim = false, bool removeSpecial = false)
        {
            string result = string.Empty;


            if (obj == null || obj == DBNull.Value)
            {
                return result;
            }

            result = obj.ToString();

            if (removeSpecial == true) result = System.Text.RegularExpressions.Regex.Replace(result, "[^a-zA-Z0-9%._]", string.Empty); // 공백은 제외 "[^a-zA-Z0-9%. _]"

            if (trim == true) result = result.Trim();

            return result;
        }

        public static string SafeFormat(string message, params object[] args)
        {
            if (args == null || args.Length == 0)
                return message;
            try
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, message, args);
            }
            catch (Exception ex)
            {
                return message + " (System error: failed to format message. " + ex.Message + ")";
            }
        }

        public static void SetValue(this object obj, string property, object value)
        {
            if (obj != null)
            {
                if (obj is System.Data.DataRowView)
                {
                    System.Data.DataRowView drv = obj as System.Data.DataRowView;
                    drv[property] = value == null ? DBNull.Value : value;
                }
                else
                {
                    System.Reflection.PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
                    if (propertyInfo != null)
                    {
                        try
                        {
                            obj.GetType().InvokeMember(propertyInfo.Name, System.Reflection.BindingFlags.SetProperty, null, obj, new object[] { value });
                        }
                        catch //(Exception ex)
                        {
                        }
                    }
                }
            }
        }

        public static object GetValue(Dictionary<string, object> data, string key)
        {
            return data[key];
        }

        public static object GetValue(this object value, string name)
        {
            object obj;
            try
            {
                if (value == null)
                {
                    obj = "";
                }
                else
                {
                    if (value.GetType() == typeof(System.Data.DataRow))
                    {
                        obj = ((System.Data.DataRow)value)[name];
                    
                    } 
                    else if(value.GetType() == typeof(System.Data.DataRowView))
                    {
                        obj = ((System.Data.DataRowView)value)[name];
                    } 
                    else
                    {
                        obj = value.GetType().GetProperty(name).GetValue(value, null);
                    }
                }

            }
            catch //(Exception exception)
            {
                obj = null;
            }
            return obj;
        }

        public static string SelectedValuePath(this DataGridColumn col)
        {
            return ((DataGridComboBoxColumn)col).SelectedValuePath;
        }

        public static string DisplayMemberPath(this DataGridColumn col)
        {
            return ((DataGridComboBoxColumn)col).DisplayMemberPath;
        }
    }
}
