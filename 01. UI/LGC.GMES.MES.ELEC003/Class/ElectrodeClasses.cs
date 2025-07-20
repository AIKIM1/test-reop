using System.ComponentModel;

namespace LGC.GMES.MES.ELEC003
{
    public static class ElectrodeShops
    {
        [Description("CNJ 남경 소형 전극")]
        public const string G183 = "G183";
    }

    public static class ElectrodeAreas
    {
        [Description("오창 자동차1동 전극")]
        public const string E5 = "E5";

        //2022-12-29 오화백  동 :EP 추가
        [Description("OC 자동차 전극 1동(소형)")]
        public const string EP = "EP";

        [Description("오창 자동차2동 전극")]
        public const string E6 = "E6";

        [Description("CSNB 자동차 2동 전극")]
        public const string EC = "EC";

        [Description("폴란드 자동차2동 전극")]
        public const string ED = "ED";

        [Description("남경 소형1동 전극")]
        public const string EH = "EH";
    }

    public static class ElectrodeProcesses
    {
        public const string SLITTING = "E4000";
        public const string TWO_SLITTING = "E4200"; //20211215 2차 Slitting 공정진척DRB 화면 개발
    }

}
