using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.Common
{
    public class MessageDic
    {
        // TODO : MessageDic 테스트 -> BizActor 오류 시 잘 되는지...

        public static readonly MessageDic Instance = new MessageDic();

        private Dictionary<string, string> messageDic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        private MessageDic()
        {
        }

        public void SetMessageDicData(DataTable messageDic)
        {
            this.messageDic.Clear();
            try
            {
                foreach (DataRow row in messageDic.Rows)
                {
                    if (this.messageDic.ContainsKey(row["MSGID"].ToString()))
                        Console.WriteLine(string.Format("MessageDic 중복오류 ( {0} : {1} )", row["MSGID"].ToString(), row["MSGNAME"].ToString()));
                    else
                        this.messageDic.Add(row["MSGID"].ToString(), row["MSGNAME"].ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public string GetMessage(string MSGID)
        {
            if (messageDic.ContainsKey(MSGID))
                return messageDic[MSGID].Replace("\\r\\n", "\r\n");
            else
            {
                // 예외 처리
                if (MSGID.Length > 3 && MSGID.Substring(0, 3).Equals("[*]"))
                {
                    return MSGID.Replace("[*]", "");
                }
                else
                {
                    return "[#] " + MSGID;
                }                
            }
        }

        //public string GetMessage(string MSGID, params object[] parameters)
        //{
        //    if (parameters.Length > 0)
        //    {
        //        return string.Format(GetMessage(MSGID), parameters);
        //    }
        //    else
        //    {
        //        return GetMessage(MSGID);
        //    }
        //}

        public string GetMessage(string MSGID, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                Regex rgx = new Regex(@"%\d+");

                string tgt2 = rgx.Replace(GetMessage(MSGID), (mtcInput) =>
                {
                    string sVal = mtcInput.Value.Trim(new char[] { '%' });
                    int iVal = Convert.ToInt32(sVal) - 1;

                    return "{" + iVal + "}";
                });

                return string.Format(tgt2, parameters).Replace("\\r\\n", "\r\n");
            }
            else
            { 
                return GetMessage(MSGID).Replace("\\r\\n", "\r\n");
            }
        }

        public string GetMessage(Exception ex, params object[] parameters)
        {
            if (isBizException(ex))
                return GetMessage(getBizExceptionCode(ex), parameters);
            else
                return GetMessage(ex.Message, parameters);
        }

        private bool isBizException(Exception ex)
        {
            if (ex.Data.Contains("CODE"))
                return true;
            else
                return false;
        }

        private string getBizExceptionCode(Exception ex)
        {
            return ex.Data.Contains("CODE") ? ex.Data["CODE"].ToString() : string.Empty;
        }
    }
}