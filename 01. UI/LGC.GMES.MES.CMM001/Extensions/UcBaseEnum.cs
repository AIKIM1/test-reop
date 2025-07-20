/*************************************************************************************
 Created Date : 2023.10.30
      Creator : 
   Decription : UcBaseEnum
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.30  조영대 : Initial Created. 
**************************************************************************************/

namespace LGC.GMES.MES.CMM001.Controls
{
    public enum CommonCodeColumn
    {
        CMCODE,
        CMCDNAME,
        ATTRIBUTE1,
        ATTRIBUTE2,
        ATTRIBUTE3,
        ATTRIBUTE4,
        ATTRIBUTE5
    }

    public enum AggregateColumnType
    {
        SUM_TEXT,
        SUM,
        AVG,
        RATIO,
        RATIO_TOTAL,
        RATIO_PART
    }

    public enum BorderLineStyle
    {
        Line,
        SmallDot,
        Dot,
        Dash,
        SinglePointDash
    }
}