/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.06.27   권용섭    : [E20240620-001371] 보정 dOCV 송/수신 실패시 팝업 알림
  2024.08.26   최성필    : FDS 발열셀, SAS 송수신 오류 알람 추가
  2025.04.22   이지은    : 2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부 반영의 건
**************************************************************************************/
using System.Data;

namespace LGC.GMES.MES.Common
{
    public class LoginInfo
    {
        public static string LANGID = string.Empty;

        public static string[] SUPPORTEDLANGLIST = new string[0];
        public static object[] SUPPORTEDLANGINFOLIST = new object[0];

        public static string PARAM = string.Empty;
        public static string USGRID = string.Empty;
        public static string USERID = string.Empty;
        public static string USERNAME = string.Empty;
        public static string USERPSWD = string.Empty;
        public static string AUTHID = string.Empty;
        public static string SUPPLIERID = string.Empty;
        public static string SYSID = string.Empty;
        public static string SYSIDSUB = string.Empty;
        public static string USERMAIL = string.Empty;
        public static string USERTYPE = string.Empty;
        public static bool? LOGGEDBYSSO;
        public static string ACTIVE_GENERAL_PRINTER = string.Empty;
        public static string ACTIVE_THERMAL_PRINTER = string.Empty;
        public static string PNTR_WRKR_TYPE = string.Empty;

        //CONFIG COMMON ====================================================================
        public static string CFG_SHOP_ID = string.Empty;
        public static string CFG_AREA_ID = string.Empty;
        public static string CFG_EQSG_ID = string.Empty;
        public static string CFG_PROC_ID = string.Empty;
        public static string CFG_EQPT_ID = string.Empty;

        public static string CFG_SHOP_NAME = string.Empty;
        public static string CFG_AREA_NAME = string.Empty;
        public static string CFG_EQSG_NAME = string.Empty;
        public static string CFG_PROC_NAME = string.Empty;
        public static string CFG_EQPT_NAME = string.Empty;

        public static string CFG_INI_MENUID = string.Empty;
        public static DataTable CFG_GENERAL_PRINTER = new DataTable();
        public static DataTable CFG_SERIAL_PRINT = new DataTable();
        public static DataTable CFG_THERMAL_PRINT = new DataTable();
        public static DataTable CFG_LOGGING = new DataTable();
        public static DataTable CFG_NOTCHING_PRINT = new DataTable();
        public static DataTable CFG_ETC = new DataTable();

        public static DataTable CFG_LABEL = new DataTable();
        public static string CFG_LABEL_TYPE = string.Empty;
        public static int CFG_LABEL_COPIES = 1;
        public static string CFG_CUT_LABEL = "N";
        public static string CFG_LABEL_AUTO = "N";
        public static int CFG_CARD_COPIES = 1;
        public static string CFG_CARD_AUTO = "N";
        public static string CFG_CARD_POPUP = "N";
        public static int CFG_THERMAL_COPIES = 2;
        //C20210415-000402 대LOT별 첫번째 컷 바코드 라벨 수량
        public static int CFG_LABEL_FIRST_CUT_COPIES = 0;


        public static bool IS_CONNECTED_ERP = false;
        //활성화 관련 추가. 2020.11.13
        public static string CFG_SYSTEM_TYPE_CODE = string.Empty;
        public static string CFG_MENUID = string.Empty;

        //2023.01.17 : 활성화 관련 FORM Config 테이블 추가 - leeyj
        public static DataTable CFG_FORM = new DataTable();
        //활성화 알림 팝업 관련 추가 2021.03.08
        public static bool CFG_EQP_STATUS = false;
        public static bool CFG_W_LOT = false;
        //2023.01.17 : 활성화 공정대기시간초과, aging 시간 초과 팝업관련 Flag 추가 - leeyj
        public static bool CFG_FORM_PROC_WAIT_LIMIT_TIME_OVER = true; //기본값 true
        public static bool CFG_FORM_AGING_LIMIT_TIME_OVER = true; //기본값 true
        public static bool CFG_FORM_AGING_OUTPUT_TIME_OVER = false; // 2023.12.17 출하 Aging 후단출고 기준시간 초과 현황 추가
        //2024.06.27 / 권용섭(cnsyongsub) / [E20240620-001371] 보정 dOCV 전송실패시 팝업 알림 여부
        public static bool CFG_FORM_FITTED_DOCV_TRNF_FAIL = false;
        //2024.08.20 / 최성필(cso59463) / FDS 발열셀 알람 팝업 사용 여부
        public static bool CFG_FORM_FDS_ALARM = false;
        //2024.08.26 / 최성필(cso59463) / SAS 송수신 오류 알람 팝업 사용 여부
        public static bool CFG_FORM_SAS_ALARM = false;
        //2024.11.01 / 강동희(kdh7609) / 고온 Aging 온도 이탈 알람 팝업 사용 여부
        public static bool CFG_FORM_HIGH_AGING_ABNORM_TMPR_ALARM = false;

        //활성화 관련 추가. 2022.12.14
        public static string USER_IP = string.Empty;
        public static string PC_NAME = string.Empty;

        public static string CTX_MENUID = string.Empty; 
    }
}