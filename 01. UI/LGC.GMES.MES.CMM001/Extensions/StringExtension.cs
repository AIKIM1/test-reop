using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LGC.GMES.MES.CMM001.Extensions
{
    public static class StringExtension
    {



        /// <summary>
        /// var s = "aaaaaaaabbbbccccddddeeeeeeeeeeee".FormatWithMask("Hello ########-#A###-####-####-############ Oww");             
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static string FormatWithMask(this string input, string mask)
        {
            if (input.IsNullOrEmpty()) return input;
            var output = string.Empty;
            var index = 0;
            foreach (var m in mask)
            {
                if (m == '#')
                {
                    if (index < input.Length)
                    {
                        output += input[index];
                        index++;
                    }
                }
                else
                    output += m;
            }
            return output;
        }

        /// <summary>
        /// 문자열 포함 여부, 요소 포함 여부는 ContainsValue 로 사용한다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool Contains(this string value, params string[] values)
        {
            foreach (string one in values)
            {
                if (value.Contains(one))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsValue(this string value, params string[] values)
        {
            foreach (string one in values)
            {
                if (value == one)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 문자열 모두 포함 여부
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool ContainsAll(this string value, params string[] values)
        {
            foreach (string one in values)
            {
                if (!value.Contains(one))
                {
                    return false;
                }
            }
            return true;
        }

        public static string Right(this string s, int length)
        {
            length = Math.Max(length, 0);

            if (s.Length > length)
            {
                return s.Substring(s.Length - length, length);
            }
            else
            {
                return s;
            }
        }
        public static string Left(this string s, int length)
        {
            length = Math.Max(length, 0);

            if (s.Length > length)
            {
                return s.Substring(0, length);
            }
            else
            {
                return s;
            }
        }
        public static string Quote(this string value)
        {
            if (value == null) return "''";
            return "'" + value.Replace("'", "''") + "'";
        }

        public static string FirstCap(this string value)
        {
            if (value == null || value.Length == 0 || char.IsUpper(value[0])) return value;
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        public static string TrimSuffix(this string value, string suffix)
        {
            if (value == null || suffix == null || value.Length <= suffix.Length)
                return value;
            if (value.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
                return value.Substring(0, value.Length - suffix.Length);
            else
                return value;
        }


        public static string[] SafeSplit(this string value, params char[] separators)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[] { };
            if (separators == null || separators.Length == 0)
                separators = new char[] { ',', ';' };
            return value.Split(separators);
        }
        public static DateTime? ToDateTime(this string s)
        {
            DateTime dtr;
            var tryDtr = DateTime.TryParse(s, out dtr);
            return (tryDtr) ? dtr : new DateTime?();
        }
        public static DateTime? TryParseDateTime(this string value, string format = "yyyyMMdd")
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            DateTime dt;
            if (DateTime.TryParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt))
            {
                return dt;
            }
            //if (DateTime.TryParse(value, out dt))
            //    return dt;
            return null;
        }

        public static string IfEmpty(this string value, string defaultValue)
        {
            return (value.IsNotEmpty() ? value : defaultValue);
        }
        public static bool IsNotEmpty(this string value)
        {
            return (value.IsEmpty() == false);
        }
        public static string FormatWith(this string value, params object[] parameters)
        {
            return string.Format(value, parameters);
        }
        public static string EnsureStartsWith(this string value, string prefix)
        {
            return value.StartsWith(prefix) ? value : string.Concat(prefix, value);
        }
        public static string EnsureEndsWith(this string value, string suffix)
        {
            return value.EndsWith(suffix) ? value : string.Concat(value, suffix);
        }

        public static bool IsNumeric(this string value)
        {
            float output;
            return float.TryParse(value, out output);
        }
        public static string ConcatWith(this string value, params string[] values)
        {
            return string.Concat(value, string.Concat(values));
        }
        public static string PadBoth(this string value, int width, char padChar, bool truncate = false)
        {
            int diff = width - value.Length;
            if (diff == 0 || diff < 0 && !(truncate))
            {
                return value;
            }
            else if (diff < 0)
            {
                return value.Substring(0, width);
            }
            else
            {
                return value.PadLeft(width - diff / 2, padChar).PadRight(width, padChar);
            }
        }

        public static bool IsEmpty(this string value)
        {
            return ((value == null) || (value.Trim().Length == 0)); // 공백은 미취급
        }

        public static string GetString(object obj, string nullString)
        {
            try
            {
                if (obj == null || obj == DBNull.Value)
                {
                    return nullString;
                }

                var tempText = Convert.ToString(obj);
                return string.IsNullOrEmpty(tempText) ? nullString : tempText;
            }
            catch
            {
                return nullString;
            }
        }

        public static string GetString(this object obj)
        {
            return GetString(obj, string.Empty);
        }

        /// <summary>
        /// object to Interger 형변환 확장 메소드 추가
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Int64 GetInt(this object obj)
        {
            try
            {
                return Convert.ToInt64(obj);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// object to Decimal 형변환 확장 메소드 추가
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Decimal GetDecimal(this object obj)
        {
            try
            {
                return Convert.ToDecimal(obj, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static double GetDouble(this object obj)
        {
            try
            {
                return Convert.ToDouble(obj, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }


}
