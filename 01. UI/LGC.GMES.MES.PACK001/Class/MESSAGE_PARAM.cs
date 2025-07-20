/*************************************************************************************
 Created Date : 2015.09.30
      Creator : J.H.Lim
   Description : UI에서 공용으로 사용되어지는 Class 
--------------------------------------------------------------------------------------
 [Change History]
  2015.09.30 / J.H.Lim : Initial Created.
**************************************************************************************/

using System;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001.Class
{
    public class MESSAGE_PARAM
    {

        #region 생성자
        public MESSAGE_PARAM()
        {
        }
        #endregion 생성자

        public static String[] param_message = new String[5];

        #region MESSAGE INFO
        public void AlertInfo(string sMsgId)
        {
            Util.AlertInfo(sMsgId);
        }

        public void AlertInfo(string sMsgId, string param1 = "")
        {
            if (param1 != "")
            {
                param_message[0] = param1;

                //AlertInfo(sMsgId, param1, "");
                Util.AlertInfo(sMsgId, param_message);
            }
            else
            {
                Util.AlertInfo(sMsgId);
            }
        }

        public void AlertInfo(string sMsgId, string param1 = "", string param2 = "")
        {
            if (param1 == "" && param2 == "")
            {
                Util.AlertInfo(sMsgId);
            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;

                Util.AlertInfo(sMsgId, param_message);
            }
        }

        public void AlertInfo(string sMsgId, string param1 = "", string param2 = "", string param3 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "")
            {
                Util.AlertInfo(sMsgId);
            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;

                Util.AlertInfo(sMsgId, param_message);
            }
        }

        public void AlertInfo(string sMsgId, string param1 = "", string param2 = "", string param3 = "", string param4 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "" && param4 == "")
            {
                Util.AlertInfo(sMsgId);

            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;
                param_message[3] = param4;

                Util.AlertInfo(sMsgId, param_message);
            }
        }

        public void AlertInfo(string sMsgId, string param1 = "", string param2 = "", string param3 = "", string param4 = "", string param5 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "" && param4 == "" && param5 == "")
            {
                Util.AlertInfo(sMsgId);

            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;
                param_message[3] = param4;
                param_message[4] = param5;

                Util.AlertInfo(sMsgId, param_message);
            }
        }
        #endregion

        #region MESSAGE WARNING
        public void AlertWarning(string sMsgId)
        {
            Util.Alert(sMsgId);
        }

        public void AlertWarning(string sMsgId, string param1 = "")
        {
            if (param1 != "")
            {
                param_message[0] = param1;

                //AlertInfo(sMsgId, param1, "");
                Util.Alert(sMsgId, param_message);
            }
            else
            {
                Util.Alert(sMsgId);
            }
        }

        public void AlertWarning(string sMsgId, string param1 = "", string param2 = "")
        {
            if (param1 == "" && param2 == "")
            {
                Util.Alert(sMsgId);
            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;

                Util.Alert(sMsgId, param_message);
            }
        }

        public void AlertWarning(string sMsgId, string param1 = "", string param2 = "", string param3 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "")
            {
                Util.Alert(sMsgId);
            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;

                Util.Alert(sMsgId, param_message);
            }
        }

        public void AlertWarning(string sMsgId, string param1 = "", string param2 = "", string param3 = "", string param4 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "" && param4 == "")
            {
                Util.Alert(sMsgId);

            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;
                param_message[3] = param4;

                Util.Alert(sMsgId, param_message);
            }
        }

        public void AlertWarning(string sMsgId, string param1 = "", string param2 = "", string param3 = "", string param4 = "", string param5 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "" && param4 == "" && param5 == "")
            {
                Util.Alert(sMsgId);

            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;
                param_message[3] = param4;
                param_message[4] = param5;

                Util.Alert(sMsgId, param_message);
            }
        }
        #endregion

        #region RETURN MESSAGE
        public string AlertRetun(string sMsgId)
        {
            return MessageDic.Instance.GetMessage(sMsgId);
        }

        public string AlertRetun(string sMsgId, string param1 = "")
        {
            if (param1 != "")
            {
                param_message[0] = param1;

                //AlertInfo(sMsgId, param1, "");
                return MessageDic.Instance.GetMessage(sMsgId, param_message);
            }
            else
            {
                return MessageDic.Instance.GetMessage(sMsgId);
            }
        }

        public string AlertRetun(string sMsgId, string param1 = "", string param2 = "")
        {
            if (param1 == "" && param2 == "")
            {
                return MessageDic.Instance.GetMessage(sMsgId);
            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;

                return MessageDic.Instance.GetMessage(sMsgId, param_message);
            }
        }

        public string AlertRetun(string sMsgId, string param1 = "", string param2 = "", string param3 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "")
            {
                return MessageDic.Instance.GetMessage(sMsgId);
            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;

                return MessageDic.Instance.GetMessage(sMsgId, param_message);
            }
        }

        public string AlertRetun(string sMsgId, string param1 = "", string param2 = "", string param3 = "", string param4 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "" && param4 == "")
            {
                return MessageDic.Instance.GetMessage(sMsgId);

            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;
                param_message[3] = param4;

                return MessageDic.Instance.GetMessage(sMsgId, param_message);
            }
        }

        public string AlertRetun(string sMsgId, string param1 = "", string param2 = "", string param3 = "", string param4 = "", string param5 = "")
        {
            if (param1 == "" && param2 == "" && param3 == "" && param4 == "" && param5 == "")
            {
                return MessageDic.Instance.GetMessage(sMsgId);

            }
            else
            {
                param_message[0] = param1;
                param_message[1] = param2;
                param_message[2] = param3;
                param_message[3] = param4;
                param_message[4] = param5;

                return MessageDic.Instance.GetMessage(sMsgId, param_message);
            }
        }
        #endregion


    }
}