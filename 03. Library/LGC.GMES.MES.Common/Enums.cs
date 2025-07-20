using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGC.GMES.MES.Common
{
    public enum ScanData_Type
    {
        SCAN_Data,
        SCAN_Command,
        SP_SCAN_Data
    }
    /// <summary>
    /// Data Type
    /// </summary>
    public enum DataType
    {
        String,
        Int32,
        Int16,
        Double,
        Decimal,
        Guid,
        Single,
        DateTime,
        Boolean
    }

    /// <summary>
    /// Message Type
    /// </summary>
    public enum MsgType
    {
        Warnning,
        Error,
        Information,
        Confirm
    }

    public enum SoundType
    {
        OK,
        NG,
        ModelChange,
        Alarm,
        NO
    }

    public enum ProcQueueType
    {
        SQL, BIZ
    }

    public enum QueueNameType
    {
        Unique, Multiple
    }

    /// <summary>
    /// Extension Methods of Enums
    /// </summary>
    public static class EnumExtensions
    {
        public static string ToMsgString(this MsgType me)
        {
            switch (me)
            {
                case MsgType.Warnning:
                    return "Warnning";
                case MsgType.Information:
                    return "Information";
                case MsgType.Confirm:
                    return "Confirm";
                case MsgType.Error:
                    return "Error";
            }

            return string.Empty;
        }
    }
}
