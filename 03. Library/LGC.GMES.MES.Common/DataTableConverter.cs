using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace LGC.GMES.MES.Common
{
    public class DataTableConverter
    {
        static DataTableConverter()
        {
        }

        public static IEnumerable Convert(DataTable table)
        {
            return table.Copy().AsDataView();
        }

        public static DataTable Convert(IEnumerable list)
        {
            if (list is DataView)
            {
                DataView dv = list as DataView;
                DataTable dt = dv.ToTable();
                return dt;
                //return (list as DataView).Table.Copy();
            }
            else
            {
                return new DataTable();
            }
        }

        public static void AddDatas(IEnumerable list, DataTable table)
        {
            if (list != null && list is DataView)
            {
                DataView dv = list as DataView;
                foreach (DataRow row in table.Rows)
                {
                    dv.Table.Rows.Add(row.ItemArray).AcceptChanges();
                }
            }
        }

        public static object GetValue(object obj, string property)
        {
            if (obj != null)
            {
                if (obj is DataRowView)
                {
                    DataRowView drv = obj as DataRowView;
                    return drv[property] == DBNull.Value ? null : drv[property];
                }
                else
                {
                    PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
                    if (propertyInfo != null)
                    {
                        return obj.GetType().InvokeMember(propertyInfo.Name, BindingFlags.GetProperty, null, obj, new object[] { });
                    }
                }
            }

            return null;
        }

        public static void SetValue(object obj, string property, object value)
        {
            if (obj != null)
            {
                if (obj is DataRowView)
                {
                    DataRowView drv = obj as DataRowView;
                    drv[property] = value == null ? DBNull.Value : value;
                }
                else
                {
                    PropertyInfo propertyInfo = obj.GetType().GetProperty(property);
                    if (propertyInfo != null)
                    {
                        try
                        {
                            obj.GetType().InvokeMember(propertyInfo.Name, BindingFlags.SetProperty, null, obj, new object[] { value });
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        public static Hashtable ToHash(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (!hash_return.Contains(dt.Rows[i][0].ToString()))
                    {
                        hash_return.Add(dt.Rows[i][0].ToString(), dt.Rows[i][1].ToString());
                    }

                }

            }
            catch (Exception ex)
            {
                hash_return = null;
            }
            return hash_return;
        }

        public static Hashtable ToHashByColName(DataTable dt)
        {
            Hashtable hash_return = new Hashtable();
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!hash_return.Contains(dt.Columns[j].ColumnName.ToString()))
                        {
                            hash_return.Add(dt.Columns[j].ColumnName, dt.Rows[i][j].ToString());
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //commMessage.Show(ex.Message);
                hash_return = null;
            }
            return hash_return;
        }
    }
}
