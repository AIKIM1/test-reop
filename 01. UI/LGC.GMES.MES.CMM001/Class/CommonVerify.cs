/*************************************************************************************
 Created Date : 2017.01.13
      Creator : K.H.SHIN
  Description : DataTable, C1DataGrid 검증용 클래스
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.13 / K.H.SHIN : Initial Created.
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using C1.C1Preview.Export;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001.Class
{
    public static class CommonVerify
    {
        public static bool HasTableInDataSet(DataSet ds)
        {
            return ds != null && ds.Tables.Count > 0;
        }

        public static bool HasTableRow(DataTable dt)
        {
            return dt != null && dt.Rows.Count > 0;
        }

        public static bool HasDataGridRow(C1DataGrid dg)
        {
            return dg.ItemsSource != null && dg.GetRowCount() > 0;
        }

        /// <summary>
        /// string 문자열 값을 정규식을 통하여  integer 형 여부 반환 함.
        /// </summary>
        /// <param name="stringText">문자열</param>
        /// <returns>결과 값(bool)</returns>
        public static bool IsInt(string stringText)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(stringText, @"^[+-]?\d*$");
        }

        public static bool IsValidEmail(string email)
        {
            bool valid = Regex.IsMatch(email, "[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            return valid;
        }

        //Regex(@"\d{3}\-\d{3,4}\-\d{4}$");

        public static bool IsValidPhoneNumber(string phoneNo)
       {
            Regex regex = new Regex(@"\d{3}\-\d{3,4}\-\d{4}$");
            //Regex regex = new Regex(@"^ 01([0 | 1 | 6 | 7 | 8 | 9] ?)-? ([0 - 9]{ 3,4})-? ([0 - 9]{ 4})$"); 
            Match m = regex.Match(phoneNo);
            return m.Success;
        }


        public static bool IsValidDateTime(string dateText)
        {
            try
            {
                if (dateText.Length != 8)
                    return false;

                Regex regex = new Regex(@"^(19|20)\d{2}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[0-1])$");
                Match m = regex.Match(dateText);
                return m.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
