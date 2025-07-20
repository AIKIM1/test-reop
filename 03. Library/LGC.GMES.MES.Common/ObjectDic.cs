using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Interop;

namespace LGC.GMES.MES.Common
{
    public class ObjectDic
    {
        public static readonly ObjectDic Instance = new ObjectDic();

        private Dictionary<string, string> objectDic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private ObjectDic()
        {
        }

        public void SetObjectDicData(DataTable objectDic)
        {
            this.objectDic.Clear();
            foreach (DataRow row in objectDic.Rows)
            {
                try
                {
                    this.objectDic.Add(row["OBJECTID"].ToString(), row["OBJECTNAME"].ToString());
                }
                catch (Exception ex)
                {
                }
            }
        }

        public string GetObjectName(string objectID)
        {
            //2016.10.20 : MMD 요청 사항
            //필수 "(*)" 입력 시 "*" 로 변환하여 반환

            IList<string> list = new List<string>(objectID.Split(new string[] { "(*)" }, StringSplitOptions.None));

            if (list.Count == 1)
            {
                if (objectDic.ContainsKey(objectID))
                    return objectDic[objectID];

                if (objectID.Length > 3 && objectID.Substring(0, 3).Equals("[*]"))
                {
                    return objectID.Replace("[*]", "");
                }
                else
                {
                    return "[#] " + objectID;
                }
            }
            else
            {
                if (objectDic.ContainsKey(list[1]))
                    return "* " + objectDic[list[1]];

                if (list[1].Length > 3 && list[1].Substring(0, 3).Equals("[*]"))
                {
                    return objectID.Replace("[*]", "");
                }
                else
                {
                    return "[#] " + list[1];
                }
            }

            //if (objectDic.ContainsKey(objectID))
            //{
            //    return objectDic[objectID];
            //}
            //return "[#] " + objectID;
        }
    }
}