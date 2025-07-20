/*************************************************************************************
 Created Date : 2015.09.30
      Creator : J.H.Lim
   Description : UI에서 공용으로 사용되어지는 Class
--------------------------------------------------------------------------------------
 [Change History]
  2015.09.30 / J.H.Lim : Initial Created.
  2017.09.28 / INS 염규범S : C20170902_75124 - 폴딩공정 라벨발행 개선
  2017.10.27 손우석 CSR ID:3516073 오창 팩11호 BMW12V BMA 수작업 조립공정 작업성 개선을 위한 GMES 수정 요청 | [요청번호]C20171027_16073
  2017.12.14 손우석 CSR ID:3537853  팩 11호기 바코드 변경이력 추적기능 추가 요청의건 | [요청번호] C20171122_37853
  2018.10.17 손우석 Slow 쿼리 처리를 위한 파라미터 수정
  2018.11.09 손우석 CWA.PACK5호, 모듈5호, 모듈9호 증설 및 모듈2호 개조 GMES구축 - 작업지시 등록 시작 시간 및 종료 시간 추가
  2019.10.17 손우석 SM  공정별 W/O에서 종료 일자 기간 설정(다음 날 05:59)
  2019.12.05 이상준 SI  ZZS 공정코드 추가
  2020.01.16 정문교 SI  원자재 요청 상태 (Mtrl_Request_StatCode) 코드 RETURN, TAKING_OVER 추가
  2020.04.20 염규범 SI  DataTable 집계함수 추가
  2020.10.13 염규범 SI  PACK PALLET 유형 추가의 건
  2020.10.21 염규범 SI  PrintLabePackPLT 추가의 건
  2020.12.07 조영대 SI  IsNVC 메소드 추가
  2021.03.31 조영대 SI  PrintNoPreview 메소드 추가
  2021.10.21 염규범 SI  getZPL_Pack 매개변수 추가의 건
  2021.09.03 심찬보 SI  Gplm_Process_Type 소형조립 코드추가
  2021.10.14 강동희 SI  기능사용여부 체크(동별공통코드 )
  2021.11.04 권상민 SI  CT 검사 공정코드, 설비군 추가
  2021.12.15 강동희 SI  2차 Slitting 공정진척DRB 화면 개발
  2022.05.02 강동희 SI  IsAreaCommonCodeUse 오류 수정
  2022.05.17 정재홍 CSR C20220406-000241 - [자동차.전극생산 2팀] 자동차 1, 2동 전극 슬리터 바코드 불량수, Tag수 표시 공정 라벨 발행
  2022.07.16 조영대 SI  Case문 콤보컨트롤 추가.
  2022.11.04 강호운 : LASER_ABLATION 공정추가
  2023.01.03 정용석 CSR C20221212-000497 - Linq 결과 DataTable로 변환 함수 추가 - 팩 Project에서 쓰던거 Com Project에서도 사용하게 되어 갖다가 붙임.
  2023.01.28 정용석 CSR E20230125-000018 - C1ComboBox DataBinding - 팩 Project에서 쓰던거 Com Project에서도 사용하게 되어 갖다가 붙임. (기존 CommonCombo Class 버려버리는용도임.)
  2023.06.25 조영대 : 설비 Loss Level 2 Code 사용 체크 및 변환
  2023.07.06 이윤중 CSR ID E20230627-000461 - 설비 Loss 변경 제한 기준 날짜 공통코드 확인 메소드 추가 (LossDataUnalterable_BaseDate)
  2023.07.26 김태우 : NFF 신규 공정 DAM MIXER(E0430) 추가
  2023.07.31 주재홍 : Exception을 다국어 Text 로 받기 ( ExceptionMessageToString )
  2023.08.10 정재홍 CSR [E20230720-001540] - ESOC2 통합슬리터 라벨 변경 요청의건
  2023.08.31 김동훈 : gridFindDataRow 메소드 추가
  2023.10.12 김태우 : NFF 슬리팅 바코드 발행시 신규 라벨 (LBL0337) 사용
  2023.10.25 김용군 : 소형 조립 ZTZ(A5600) 설비 대응
  2023.11.29 조영대 : null 오류 수정
  2024.07.22 조범모 : [E20240715-000063] 동별공통코드 속성값 조회 메소드 추가
  2025.05.20 천진수 : ESHG 증설 조립공정진척 DNC공정추가 
 **************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LGC.GMES.MES.CMM001.Class
{
    public class CurrentLotInfo
    {
        public string LOTID = "";
        public string PR_LOTID = "";
        public string WIPSEQ = "";
        public string INPUTQTY = "";
        public string GOODQTY = "";
        public string LOSSQTY = "";
        public string WORKORDER = "";
        public string OPER_CODE = "";
        public string STATUS = "";

        public string WORKDATE = "";
        public string STARTTIME_CHAR = "";
        public string ENDTIME_CHAR = "";
        public string STATUSNAME = "";
        public string REMARK = "";
        public string PRODID = "";
        public string VERSION = "";
        public string CHANGERATE = "";
        public string MODELID = "";
        public string CONTROLQTY = "";
        public string CONFMUSER = "";
        public string EQPTID = "";

        public string CHILD_GR_SEQNO = "";
        public string WIP_TYPE_CODE = "";

        public string STARTTIME_CHAR_ORG = "";
        public string ENDTIME_CHAR_ORG = "";
        public string PR_PRODID = "";

        public void resetCurrentLotInfo()
        {
            LOTID = "";
            PR_LOTID = "";
            WIPSEQ = "";
            INPUTQTY = "";
            GOODQTY = "";
            LOSSQTY = "";
            WORKORDER = "";
            OPER_CODE = "";
            STATUS = "";
            STATUSNAME = "";
            WORKDATE = "";
            STARTTIME_CHAR = "";
            ENDTIME_CHAR = "";
            REMARK = "";
            PRODID = "";
            VERSION = "";
            CHANGERATE = "";
            MODELID = "";
            CONTROLQTY = "";
            CONFMUSER = "";
            EQPTID = "";
            CHILD_GR_SEQNO = "";
            WIP_TYPE_CODE = "";

            STARTTIME_CHAR_ORG = "";
            ENDTIME_CHAR_ORG = "";

            PR_PRODID = "";
        }
    }

    /// <summary>
    /// 출하 유형 코드
    /// </summary>
    public static class Ship_Type
    {
        /// <summary>
        /// CELL출하
        /// </summary>
        public static readonly string CELL = "CELL";
        /// <summary>
        /// SRS출하
        /// </summary>
        public static readonly string SRS = "SRS";
        /// <summary>
        /// 전극출하
        /// </summary>
        public static readonly string ELTR = "ELTR";
        /// <summary>
        /// 팩출하
        /// </summary>
        public static readonly string PACK = "PACK";
    }

    /// <summary>
    /// 저장위치유형코드
    /// </summary>
    public static class SLOC_TYPE_CODE
    {
        /// <summary>
        /// 구매원자재창고
        /// </summary>
        public static readonly string SLOC01 = "SLOC01";
        /// <summary>
        /// 원재료창고
        /// </summary>
        public static readonly string SLOC02 = "SLOC02";
        /// <summary>
        /// 반제품창고
        /// </summary>
        public static readonly string SLOC03 = "SLOC03";
        /// <summary>
        /// 물류창고
        /// </summary>
        public static readonly string SLOC05 = "SLOC05";
        /// <summary>
        /// 불량저장위치
        /// </summary>
        public static readonly string SLOC08 = "SLOC08";
    }
    public static class Area_Type
    {
        public static readonly string ELEC = "E";
        public static readonly string ASSY = "A";
        public static readonly string PACK = "P";
    }
    public static class IFMODE
    {
        /// <summary>
        /// InterFace Mode On
        /// </summary>
        public static readonly string IFMODE_ON = "ON";
        /// <summary>
        /// InterFace Mode Off
        /// </summary>
        public static readonly string IFMODE_OFF = "OFF";
    }
    public static class SRCTYPE
    {
        /// <summary>
        /// Input Type UI
        /// </summary>
        public static readonly string SRCTYPE_UI = "UI";
        /// <summary>
        /// Input Type Equipment
        /// </summary>
        public static readonly string SRCTYPE_EQ = "EQ";
    }
    public static class LABEL_TYPE_CODE
    {
        /// <summary>
        /// 팩 라벨타입 제품
        /// </summary>
        public static readonly string PACK = "PACK";
        /// <summary>
        /// 팩 라벨타입 BOX
        /// </summary>
        public static readonly string PACK_INBOX = "PACK_INBOX";
        /// <summary>
        /// 팩 팔렛트 라벨타입 PALLET
        /// </summary>
        public static readonly string PACK_OUTBOX = "PLT";
    }

    public static class Process_Type
    {
        /// <summary>
        /// 폐기공정
        /// </summary>
        public static readonly string SCRAP = "S";
        /// <summary>
        /// 측정공정
        /// </summary>
        public static readonly string MEASUREMENT = "M";
        /// <summary>
        /// 수리공정
        /// </summary>
        public static readonly string REPAIR = "R";
        /// <summary>
        /// 가상공정
        /// </summary>
        public static readonly string DUMMY = "D";
        /// <summary>
        /// 생산공정
        /// </summary>
        public static readonly string PRODUCTION = "P";
        /// <summary>
        /// 검사공정
        /// </summary>
        public static readonly string INSPECTION = "I";
        /// <summary>
        /// 출고처리공정
        /// </summary>
        public static readonly string SHIP = "SHIP";
        /// <summary>
        /// 인계가상공정
        /// </summary>
        public static readonly string SEND = "SEND";
    }

    public static class INOUT_TYPE
    {
        /// <summary>
        /// IN Type
        /// </summary>
        public static readonly string IN = "IN";

        /// <summary>
        /// OUT Type
        /// </summary>
        public static readonly string OUT = "OUT";

        /// <summary>
        /// PROD Type
        /// </summary>
        public static readonly string PROD = "PROD";

        /// <summary>
        /// INOUT Type
        /// </summary>
        public static readonly string INOUT = "INOUT";
    }

    public static class Process
    {
        //공정

        // 전극
        public static readonly string PRE_MIXING = "E0500";
        public static readonly string CMC = "E0410";
        public static readonly string BS = "E0400";
        public static readonly string MIXING = "E1000";
        public static readonly string COATING = "E2000";
        public static readonly string TOP_COATING = "E2000";
        public static readonly string BACK_COATING = "E2200";
        public static readonly string INS_COATING = "E2300";
        public static readonly string HALF_SLITTING = "E2500";
        public static readonly string ROLL_PRESSING = "E3000";
        public static readonly string TAPING = "E3500";
        public static readonly string REWINDER = "E3800";
        public static readonly string LASER_ABLATION = "E3300";         // 2022.11.04  강호운 : C20221107-000542 - LASER_ABLATION 공정추가
        public static readonly string BACK_WINDER = "E3900";
        public static readonly string SLITTING = "E4000";
        public static readonly string INS_SLIT_COATING = "E4500";
        public static readonly string HEAT_TREATMENT = "E4600";         // 열처리공정 신규 추가
        public static readonly string ELEC_STORAGE = "E7000";
        public static readonly string VD_ELEC = "E8000";
        public static readonly string ELEC_BOXING = "E9000";
        public static readonly string PRE_MIXING_PACK = "E0700";
        public static readonly string REWINDING = "E2100";
        public static readonly string SLIT_REWINDING = "E4100";
        public static readonly string InsulationMixing = "E0420";

        public static readonly string TWO_SLITTING = "E4200"; //20211215 2차 Slitting 공정진척DRB 화면 개발
        public static readonly string DAM_MIXING = "E0430"; //20230725 NFF DAM_MIXING (E0430) 추가

        // 폴리머 조립
        public static readonly string ASSY_REWINDER = "A4800";         // 조립 재와인더 공정 추가
        public static readonly string NOTCHING = "A5000";
        public static readonly string VD_LMN = "A6000";
        //public static readonly string VDQA = "";
        public static readonly string LAMINATION = "A7000";
        public static readonly string SRC = "A7100";
        public static readonly string SSC_BICELL = "A7200";
        public static readonly string STACKING_FOLDING = "A8000";
        public static readonly string STP = "A8100";
        //ZZS 공정코드 추가 2019.12.05
        public static readonly string ZZS = "A8200";
        public static readonly string SSC_FOLDED_BICELL = "A8300";

        public static readonly string PACKAGING = "A9000";
        public static readonly string CELL_BOXING = "B1000";
        public static readonly string CELL_BOXING_RETURN = "B9000";
        public static readonly string CELL_BOXING_RETURN_RMA = "B9100";
        public static readonly string AZS_STACKING = "A8400";
        public static readonly string AZS_ECUTTER = "A7400";

        //ZTZ 공정 추가
        public static readonly string ZTZ = "A5600";
        public static readonly string DNC = "A5700";    // 20250428 ESHG DNC공정신설

        public static readonly string CPROD = "AC001";
        public static readonly string WINDING_POUCH = "A7500";
        public static readonly string TAPING_POUCH = "A7600";
        public static readonly string TAPING_AFTER_FOLDING = "A8600";
        public static readonly string NOTCHING_REWINDING = "A5100";
        public static readonly string VD_LMN_AFTER_STP = "A8500";
        // CT검사 공정 코드 추가 2021.11.04
        public static readonly string CT_INSP = "A8800";
        //L&S 재작업 공정 코드 추가 2019.05.13
        public static readonly string RWK_LNS = "AC002";

        // 폴리머 후공정
        //public static readonly string BAKING = "";
        public static readonly string SELECTING = "F2000";
        public static readonly string DEGASING = "F4000";
        public static readonly string GRADING = "F5000";
        //public static readonly string DSF_STOCK = "";
        public static readonly string DSF = "F8000";
        public static readonly string TCO = "F7000";
        public static readonly string CHR = "F6000";
        //public static readonly string PACKING = "";
        //public static readonly string SHIPPING = "";

        // 활성화 후공정 폴리머
        public static readonly string PolymerDegas = "F4000";                   // Degas(Tray) 공정진척, Degas(Pallet) 공정진척
        public static readonly string PolymerGrading = "F5400";                 // Grading 공정진척       Grading -> 특성측성 online 특성
        public static readonly string PolymerOffLine = "F6000";                 // Off-Line 특성 공정진척 Grading -> 특성측성 offline 특성
        public static readonly string PolymerDSF = "F7000";                     // DSF 공정진척 TCO -> DSF
        public static readonly string PolymerTaping = "F7200";                  // Taping(2D) 공정진척
        public static readonly string PolymerTCO = "F7100";                     // TCO 공정진척
        public static readonly string PolymerSideTaping = "F7300";              // Side Taping 공정진척
        public static readonly string PolymerFinalExternalDSF = "F8000";        // DSF -> DSF 최종외관
        public static readonly string PolymerFinalExternal = "F8100";           // 특성 최종외관
        public static readonly string PolymerFairQuality = "F7500";             // 양품화 공정진척
        public static readonly string PolymerCharacteristicGrader = "F5500";    // 특성/Grading (파우치형)
        public static readonly string PolymerOffLineCharacteristic = "F5600";   // Offline 특성 측정 (파우치형)

        // 원/각 조립
        public static readonly string VD_CDE_PRS = "A1000";
        public static readonly string WINDING = "A2000";
        public static readonly string ASSEMBLY = "A3000";
        public static readonly string WASHING = "A4000";
        public static readonly string XRAY_REWORK = "A4100";
        public static readonly string ASSEMBLY_REWORK = "AC003";  // Assembly 재작업

        // SRS
        public static readonly string SRS_MIXING = "S1000";
        //public static readonly string SRS_BEADMILL = "";
        public static readonly string SRS_COATING = "S2000";
        public static readonly string SRS_SLITTING = "S4000";
        public static readonly string SRS_BOXING = "S9000";

        //PACK
        public static readonly string PACK_CMA_BOXING = "P5500";
        public static readonly string PACK_BMA_BOXING = "P9500";

        //활성화 후공정(원형 Grader, 원형 특성, 원형 재튜빙, 원형 특성/Grader, 초소형 Grader, 초소형 X-Ray검사, 초소형 외부탭, 초소형 OCV 검사, 초소형 누액검사, 초소형 더블탭)
        public static readonly string CircularGrader = "F5000";
        public static readonly string SmallGrader = "F5100";
        public static readonly string CircularReTubing = "F5200";
        public static readonly string CircularCharacteristicGrader = "F5300";
        public static readonly string CircularVoltage = "F5900";
        public static readonly string CircularCharacteristic = "F6000";

        public static readonly string SmallXray = "F6100";
        public static readonly string SmallExternalTab = "F6200";
        public static readonly string SmallOcv = "F6300";
        public static readonly string SmallLeak = "F6400";
        public static readonly string SmallDoubleTab = "F6500";
        public static readonly string SmallAppearance = "F6600";
        public static readonly string SmallCCD = "F6250";

        // 초소형 포장
        public static readonly string SmallPacking = "F9100";
    }


    public static class EquipmentGroup
    {
        public static readonly string FOLDING = "FOL";
        public static readonly string STACKING = "STK";
        public static readonly string SSC_FOLDEDCELL = "SSF";
        public static readonly string SRC = "SRC";
        public static readonly string STP = "STP";
        public static readonly string SSC_BICELL = "SSB";
        public static readonly string PACKAGING = "PKG";
        public static readonly string TAPING = "TAP";
        public static readonly string NND = "NND";
        public static readonly string CT_INSP = "OCI";
        public static readonly string INSPECTOR = "INS";
        public static readonly string AZSSTACKING = "AZS";
    }

    public static class Wip_State
    {
        /// <summary>
        /// 공정 대기
        /// </summary>
        public static readonly string WAIT = "WAIT";
        /// <summary>
        /// 공정 진행
        /// </summary>
        public static readonly string PROC = "PROC";
        /// <summary>
        /// 설비 완료
        /// </summary>
        public static readonly string EQPT_END = "EQPT_END";
        /// <summary>
        /// 공정 완료
        /// </summary>
        public static readonly string END = "END";
        /// <summary>
        /// 작업 완료
        /// </summary>
        public static readonly string TERMINATE = "TERMINATE";
        /// <summary>
        /// 재공 이동중
        /// </summary>
        public static readonly string MOVING = "MOVING";
        /// <summary>
        /// 예약
        /// </summary>
        public static readonly string RESERVE = "RESERVE";
        /// <summary>
        /// 작업 대기
        /// </summary>
        public static readonly string SUSPEND = "SUSPEND";
        /// <summary>
        /// 재공 종료
        /// </summary>
        public static readonly string TERM = "TERM";
    }

    /// <summary>
    /// 포장 Type : Box 테이블에 PACK_WRK_TYPE_CODE 컬럼
    /// </summary>
    public static class Pack_Wrk_Type
    {
        public static readonly string EQ = "EQ";      //자동포장
        public static readonly string UI = "UI";      //개별 LOT 포장
        public static readonly string MGZ = "MGZ";    //매거진 포장
    }

    public static class ProcessType
    {
        public static readonly string Notching = "Notching";
        public static readonly string Vacuumdry = "Vacuumdry";
        public static readonly string Lamination = "Lamination";
        public static readonly string Fold = "Fold";
        public static readonly string FoldedBiCell = "FoldedBiCell";
        public static readonly string Stacking = "Stacking";
        public static readonly string Packaging = "Packaging";
    }

    public static class Gplm_Process_Type
    {
        public static readonly string MIXING = "31";
        public static readonly string COATING = "32";
        public static readonly string RTS = "33";
        public static readonly string WINDING = "A2000";
        public static readonly string ASSEMBLY = "A3000";
        public static readonly string WASHING = "A4000";
    }

    /// <summary>
    /// 자재 공급 요청 상태 코드
    /// </summary>
    public static class Mtrl_Request_StatCode
    {
        public static readonly string Request = "REQUEST";
        public static readonly string Cancel = "CANCEL";
        public static readonly string Loaded = "LOADED";
        public static readonly string Transfer = "TRANSFER";
        public static readonly string Completed = "COMPLETED";
        public static readonly string Rejection = "REJECTION";
        public static readonly string Return = "RETURN";
        public static readonly string TakingOver = "TAKING_OVER";
    }
    public static class Mtrl_Request_TypeCode
    {
        public static readonly string Request = "REQ";
        public static readonly string Return = "RTN";
    }

    public class Util
    {
        static System.Media.SoundPlayer rpLazer = new System.Media.SoundPlayer(LGC.GMES.MES.CMM001.Properties.Resources.LAZER);
        static System.Media.SoundPlayer rpDing = new System.Media.SoundPlayer(LGC.GMES.MES.CMM001.Properties.Resources.DING);
        static System.Media.SoundPlayer rpDingDong = new System.Media.SoundPlayer(LGC.GMES.MES.CMM001.Properties.Resources.DINGDONG);
        static System.Media.SoundPlayer rpWarning = new System.Media.SoundPlayer(LGC.GMES.MES.CMM001.Properties.Resources.WARN);


        public static void LazerPlayer()
        {
            rpLazer.Play();
        }

        public static void DingPlayer()
        {
            rpDing.Play();
        }

        public static void DingDongPlayer()
        {
            rpDingDong.Play();
        }

        public static void WarningPlayer()
        {
            rpWarning.Play();
        }

        #region 생성자
        public Util()
        {
        }
        #endregion 생성자

        public static String[] szWeek = { "1st week", "2nd week", "3rd week", "4th week", "5th week", "6th week" };

        public static String[] szShortMonth = { "Jan", "Feb", "Mar", "Apr",
                                                  "May", "Jun", "Jul", "Aug",
                                                  "Sep", "Oct", "Nov", "Dec" };

        public static String[] szLongMonth = {"January", "February", "March", "April",
                                                "May", "June", "July", "August",
                                                "September", "October", "November", "December" };

        public static String[] szYear = { "2005", "2006", "2007", "2008", "2009", "2010" };

        /// <summary>
        /// 파일을 Bytes 로 변환하여 리턴
        /// </summary>
        /// <param name="pSourceFileName"></param>
        /// <returns></returns>
        public static byte[] FileToBytes(string pSourceFileName)
        {
            try
            {
                FileStream fileStream = new FileStream(pSourceFileName, FileMode.Open, FileAccess.Read);

                MemoryStream memoryStream = new MemoryStream();
                memoryStream.SetLength(fileStream.Length);
                fileStream.Read(memoryStream.GetBuffer(), 0, (int)fileStream.Length);
                fileStream.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new byte[0];
            }
        }

        /// <summary>
        ///  Clear Grid
        /// </summary>
        /// <param name="dataGrid"></param>
        public static void gridClear(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.ItemsSource == null) return;

            DataTable dtClear = DataTableConverter.Convert(dataGrid.ItemsSource);
            if (dtClear != null && dtClear.Rows.Count > 0)
            {

                dtClear.Rows.Clear();
                dataGrid.ItemsSource = DataTableConverter.Convert(dtClear);
                //20161015 add scpark
                //LoadedCellPresenter 셋팅된것들 초기화하도록
                dataGrid.Refresh();
            }
        }

        /// <summary>
        /// 해당 Row 전체 선택(row반전)
        /// </summary>
        /// <param name="svObj"></param>
        /// <returns></returns>
        public static int[] getSelectedRows(C1.WPF.DataGrid.C1DataGrid svObj)
        {
            int[] iRow;
            ArrayList ary = new ArrayList();

            if (svObj.SelectionMode == C1.WPF.DataGrid.DataGridSelectionMode.MultiRow)
            {
                for (int i = 0; i < svObj.Rows.Count - 1; i++)
                {
                    if (svObj.Rows[i].IsSelected && svObj.Rows[i].Visibility == Visibility.Visible)
                        ary.Add(i);
                }
            }
            //else
            //{
            //  //C1.Win.C1FlexGrid.CellRange r = svObj.Selection;
            //  C1.WPF.DataGrid.DataGridSelectedItemsCollection<C1.WPF.DataGrid.DataGridCellsRange> r = svObj.Selection.SelectedRanges;

            //  for (int i = r.TopLeftCell.Row.Index; i < r.BottomRightCell.Row.Index + 1; i++)
            //  {
            //      if (i != -1)
            //          ary.Add(i);
            //  }
            //}

            iRow = (int[])ary.ToArray(typeof(int));
            return iRow;
        }

        /// <summary>
        /// Grid에서 DataRow에 해당하는 Row Index 반환
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static int getGridDataRowIndex(C1DataGrid dg, DataRow row)
        {
            int iRow = -1;

            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i].ItemArray.SequenceEqual(row.ItemArray))
                {
                    iRow = i;
                    break;
                }
            }
            return iRow;
        }

        public static int gridFindDataRow(ref UcBaseDataGrid dg, string sColName, string sFindText, bool bMove)
        {
            int iRst = -1;
            try
            {
                C1DataGrid grid = dg as C1DataGrid;
                iRst = gridFindDataRow(ref grid, sColName, sFindText, bMove);                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iRst;
        }

        /// <summary>
        /// 찾고자 하는 값의 Columne 내에 데이터 인덱스를 반환 (없으면 -1)
        /// bMove : 찾은후 포커스지정여부 true , false
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="sColName"></param>
        /// <param name="sFindText"></param>
        /// <param name="bMove">찾은후 포커스지정여부 true , false</param>
        /// <returns></returns>
        public static int gridFindDataRow(ref C1DataGrid dg, string sColName, string sFindText, bool bMove)
        {
            int iRst = -1;
            try
            {
                if (dg != null && dg.Rows.Count > 0 && sColName.Length > 0)
                {
                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, sColName)) == sFindText)
                        {
                            iRst = i;
                            if (bMove)
                            {
                                //int iCol = dg.Columns[sColName].Index;
                                gridSetFocusRow(ref dg, iRst);
                            }
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iRst;
        }

        /// <summary>
        /// 해당 데이터가 같지 않는 내용 찾기 (내용이 없으면 -1, 있으면 최초 해당 index 반환 )
        /// bMove : 찾은후 포커스지정여부 true , false
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="sColName"></param>
        /// <param name="sFindText"></param>
        /// <param name="bMove">찾은후 포커스지정여부 true , false</param>
        /// <returns></returns>
        public static int gridFindIsDataRow(ref C1DataGrid dg, string sColName, string sFindText, bool bMove)
        {
            int iRst = -1;
            try
            {
                if (dg != null && dg.Rows.Count > 0 && sColName.Length > 0)
                {
                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, sColName)) != sFindText)
                        {
                            iRst = i;
                            if (bMove)
                            {
                                //int iCol = dg.Columns[sColName].Index;
                                gridSetFocusRow(ref dg, iRst);
                            }
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return iRst;
        }

        /// <summary>
        /// 그리드의 포커스 지정
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="iRow"></param>
        /// <returns></returns>
        public static bool gridSetFocusRow(ref C1DataGrid dg, int iRow)
        {
            bool bResult = false;
            try
            {
                if (dg != null && dg.Rows.Count > 0 && iRow > -1)
                {
                    if (dg.Rows.Count > iRow)
                    {
                        dg.SelectedItem = dg.Rows[iRow].DataItem;
                        dg.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bResult;
        }

        /// <summary>
        /// 찾고자 하는 Row의
        /// 컬럼명의 value return
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="sFindColName"></param>
        /// <param name="sFindText"></param>
        /// <param name="sGetColName"></param>
        /// <returns></returns>
        public static string gridFindDataRow_GetValue(ref C1DataGrid dg, string sFindColName, string sFindText, string sGetColName)
        {
            string sReturn = "";
            try
            {
                int iRow = gridFindDataRow(ref dg, sFindColName, sFindText, false);
                if (iRow > -1)
                {
                    sReturn = Util.NVC(DataTableConverter.GetValue(dg.Rows[iRow].DataItem, sGetColName));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sReturn;
        }

        /// <summary>
        /// 라디오/체크박스 선택된 ROW
        /// DataRow[] 리턴
        /// null 일경우 선택된게 없는것임 화면내에서 로직 처리할것
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="sCheckColName"></param>
        /// <returns></returns>
        public static DataRow[] gridGetChecked(ref C1DataGrid dg, string sCheckColName)
        {
            DataRow[] dr = null;
            try
            {
                DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
                if (dtChk.Columns.Contains(sCheckColName))
                {
                    dr = dtChk.Select(sCheckColName + " = 1");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        /// <summary>
        /// 라디오/체크박스 선택된 ROW
        /// DataRow[] 리턴
        /// null 일경우 선택된게 없는것임 화면내에서 로직 처리할것
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="sCheckColName"></param>
        /// <returns></returns>
        public static DataRow[] gridGetChecked(ref C1DataGrid dg, string sCheckColName, bool chkTF)
        {
            DataRow[] dr = null;
            try
            {
                DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
                if (dtChk.Columns.Contains(sCheckColName))
                {
                    if(chkTF)
                    {
                        dr = dtChk.Select(sCheckColName + " = True");
                    }
                    else
                        dr = dtChk.Select(sCheckColName + " = 1");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        public static DataRow[] gridGetChecked(ref UcBaseDataGrid dg, string sCheckColName)
        {
            DataRow[] dr = null;
            try
            {
                DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
                if (dtChk.Columns.Contains(sCheckColName))
                {
                    dr = dtChk.Select(sCheckColName + " = 1");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        public static DataRow[] gridGetCheckedTrue(ref UcBaseDataGrid dg, string sCheckColName)
        {
            DataRow[] dr = null;
            try
            {
                DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
                if (dtChk.Columns.Contains(sCheckColName))
                {
                    dr = dtChk.Select(sCheckColName + " = 'True'");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        ///
        /// <summary>
        /// 특정값을 문자열로 변환
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToString(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)
                return "";
            else
                return obj.ToString();
        }

        /// <summary>
        /// 문자열을 정수로 변환
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int StringToInt(String str)
        {
            int intReturn = 0;

            if (str.Equals(""))
                return intReturn;

            try
            {
                intReturn = int.Parse(str);
            }
            catch
            {
                return intReturn;
            }

            return intReturn;
        }

        /// <summary>
        /// 문자열을 정수로 변환
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long StringTolong(String str)
        {
            long lngReturn = 0;

            if (str.Equals(""))
                return lngReturn;

            try
            {
                lngReturn = long.Parse(str);
            }
            catch
            {
                return lngReturn;
            }

            return lngReturn;
        }

        /// <summary>
        /// 문자열을 Double로 변환
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double StringToDouble(String str)
        {
            double dblRet = 0.0;

            if (str.Equals(""))
                return dblRet;

            try
            {
                dblRet = double.Parse(str);
                return dblRet;
            }
            catch
            {
                return dblRet;
            }
        }

        /// <summary>
        /// 날짜를 문자열로 변환
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static string DatetoString(object objSender)
        {
            Type typSender = objSender.GetType();

            if ((objSender != null) && (objSender != System.DBNull.Value))
                return ((DateTime)(objSender)).ToString("MMM-dd-yyyy HH:mm:ss");

            return "";
        }

        /// <summary>
        /// 날짜를 Short형 문자열로 변환
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static string DatetoStringShort(object objSender)
        {
            Type typSender = objSender.GetType();

            if ((objSender != null) && (objSender != System.DBNull.Value))
                return ((DateTime)(objSender)).ToString("MMM-dd HH:mm");

            return "";
        }

        /// <summary>
        /// 날짜를 날짜만 표시되는 문자열로 변환
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static string DateOnlytoString(object objSender)
        {
            Type typSender = objSender.GetType();

            if ((objSender != null) && (objSender != System.DBNull.Value))
                return ((DateTime)(objSender)).ToString("MMM-dd");

            return "";
        }

        /// <summary>
        /// Null값 체크후 문자열로 리턴
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static String NVC(object objSender)
        {
            if (objSender == null || objSender == System.DBNull.Value)
                return "";

            return objSender.ToString();
        }

        /// <summary>
        /// Null값 체크후 문자열이 string.Empty 면 true
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static bool IsNVC(object objSender)
        {
            if (objSender == null || objSender == System.DBNull.Value)
                return true;

            return objSender.ToString().Equals(string.Empty);
        }

        /// <summary>
        /// Null값 체크후 숫자타입 문자열로 리턴
        /// 하태민
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static Decimal NVC_Decimal(object objSender)
        {
            if (objSender == null || objSender == System.DBNull.Value)
                return 0;

            return Convert.ToDecimal(objSender, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static int NVC_Int(object objSender)
        {
            if (objSender == null || objSender == "" || objSender == System.DBNull.Value)
                return 0;

            return int.Parse(System.Math.Round(Convert.ToDecimal(objSender)).ToString());
        }

        public static ulong NVC_ulong(object objSender)
        {
            if (objSender == null || objSender == "" || objSender == System.DBNull.Value)
                return 0;

            return ulong.Parse(System.Math.Round(Convert.ToDecimal(objSender)).ToString());
        }


        /// <summary>
        /// Null값 체크후 숫자타입 문자열로 리턴
        /// 하태민
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static String NVC_DecimalStr(object objSender)
        {
            if (objSender == null || objSender == System.DBNull.Value)
                return "0";

            return Convert.ToDecimal(objSender).ToString();
        }

        /// <summary>
        /// Null값 체크후 숫자타입 문자열로 리턴
        /// 하태민
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static String NVC_NUMBER(object objSender)
        {
            if (objSender == null || objSender == System.DBNull.Value)
                return "";

            return Convert.ToDecimal(objSender).ToString("#,##0.####");
        }

        /// <summary>
        /// Null값 체크후 문자열로 리턴
        /// </summary>
        /// <param name="objSender"></param>
        /// <param name="defaultValue"> Null 일때 리턴값</param>
        /// <returns></returns>
        public static string NVC(object objSender, string defaultValue)
        {
            if (objSender == null || objSender == System.DBNull.Value)
                return defaultValue;

            return objSender.ToString();
        }

        /// <summary>
        /// ComboBox Select 된 VALUE 값 체크후 리턴
        /// </summary>
        /// <param name="objSender"></param>
        /// <returns></returns>
        public static String COMBO_NVC(System.Windows.Controls.ComboBox cbo)
        {
            if (cbo.SelectedIndex.Equals(-1)) return "";
            if (cbo.SelectedValue == null) return "";

            return cbo.SelectedValue.ToString();
        }

        /// <summary>
        /// 월의 주차 가져오기
        /// </summary>
        /// <param name="nYear">년</param>
        /// <param name="nMonth">월</param>
        /// <returns></returns>
        public static int GetWeekOfMonth(int nYear, int nMonth)
        {
            int nDay = 0;
            int nQuotient = 0;
            int nRemainder = 0;
            DateTime dtStart = DateTime.MinValue;

            DateTime dtEnd = DateTime.MinValue;
            DateTime dtFirst = new DateTime(nYear, nMonth, 1);
            DateTime dtLast = GetLastDayOfMonth(dtFirst);

            nDay = (int)dtFirst.DayOfWeek;
            nQuotient = Math.DivRem(nDay + dtLast.Day, 7, out nRemainder);

            if (nRemainder > 0)
                nQuotient++;

            return nQuotient;
        }

        /// <summary>
        /// 월의 마지막 날짜 가져오기
        /// </summary>
        /// <param name="dtFirst"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(DateTime dtFirst)
        {
            DateTime dtTemp = dtFirst.AddMonths(1);
            DateTime dtTemp2 = new DateTime(dtTemp.Year, dtTemp.Month, 1);
            dtTemp2 = dtTemp2.AddDays(-1);

            return dtTemp2;
        }

        /// <summary>
        /// DataSet에서 Msg Code에 해당하는 Message 가져오기
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="strCode">Msg Code</param>
        /// <returns></returns>
        public static string GetMsg(DataSet ds, string strCode)
        {
            string strRet = "";
            DataRow[] drs = ds.Tables["MSGCODE"].Select("MSGCODE ='" + strCode + "'");

            if (drs.Length > 0)
                strRet = drs[0]["MESSAGE"].ToString() + "[" + drs[0]["MSGCODE"].ToString() + "]";

            return strRet;
        }

        /// <summary>
        /// 날짜 문자열을 DateTime형식으로 변환
        /// </summary>
        /// <param name="strDateTime"></param>
        /// <returns></returns>
        public static DateTime StringToDateTime(String strDateTime)
        {
            DateTime dt = DateTime.Now;

            try
            {
                dt = DateTime.Parse(strDateTime);
            }
            catch
            {

            }

            return dt;
        }

        /// <summary>
        /// 날짜 문자열을 DateTime형식으로 변환
        /// 문자열형식 param추가 ex) strDateTime:"20160825" , sFormat:"yyyyMMdd"
        /// </summary>
        /// <param name="strDateTime"></param>
        /// <param name="sFormat"></param>
        /// <returns></returns>
        public static DateTime StringToDateTime(String strDateTime, string sFormat)
        {
            DateTime dt = DateTime.Now;

            try
            {
                dt = DateTime.ParseExact(strDateTime, sFormat, null); ;
            }
            catch
            {

            }

            return dt;
        }

        /// <summary>
        /// 날짜차이 구하기
        /// </summary>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <returns></returns>
        public static int DateTimeGap(DateTime dtStart, DateTime dtEnd)
        {
            int iReturn = 0;
            try
            {
                DateTime datetimeEnd = new DateTime(dtEnd.Year, dtEnd.Month, dtEnd.Day);
                DateTime datetimeStart = new DateTime(dtStart.Year, dtStart.Month, dtStart.Day);
                TimeSpan ts = datetimeEnd - datetimeStart;
                iReturn = ts.Days;
            }
            catch
            {

            }

            return iReturn;
        }

        #region 라벨
        /// <summary>
        /// ELEC용 바코드 프린터 공통
        /// </summary>
        /// <param name="frameOperation"></param>
        /// <param name="loadingIndicator"></param>
        /// <param name="sLotID">LOT ID</param>
        /// <param name="procID">PROC ID</param>
        public static void PrintLabel_Elec(IFrameOperation frameOperation, LoadingIndicator loadingIndicator, string sLotID, string procID, string sSampleCompany = "", string userName = "")
        {
            try
            {
                //오창 자동차 전극 슬리터 목시관리 추가 ------------------------------------------------------------------------------------------
                string blCode = "";
                DataTable vleTable = new DataTable();
                vleTable.Columns.Add("LANGID", typeof(string));
                vleTable.Columns.Add("AREAID", typeof(string));
                vleTable.Columns.Add("LOTID", typeof(string));
                vleTable.Columns.Add("PROCID", typeof(string));

                DataRow vleindata = vleTable.NewRow();
                vleindata["LANGID"] = LoginInfo.LANGID;
                vleindata["AREAID"] = LoginInfo.CFG_AREA_ID;
                vleindata["LOTID"] = sLotID;
                vleindata["PROCID"] = procID;

                vleTable.Rows.Add(vleindata);

                DataTable vleresult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_ELTR_VISUAL_MNGT_AREA_BCOD", "RQSTDT", "OUTDATA", vleTable);

                if (vleresult.Rows.Count > 0)
                {
                    blCode = vleresult.Rows[0]["BLCODE"].ToString();
                }
                else
                {
                    // CSR : C20220406-000241 [2022-05-17]
                    //오창 전극 자동차 1~2동 슬리터 공정 바코드 발행 전용
                    //현장 PC 라벨 Setting 변경 없이 기준 정보로 사용 유무 변경 처리
                    //기존 라벨 신구 버전 체크로 'LBL0002'아니면 진행
                    if (!string.Equals(LoginInfo.CFG_LABEL_TYPE, "LBL0002"))
                    {
                        string[] sEqptAttr = { LoginInfo.CFG_AREA_ID, procID };
                        if (IsCommoncodeAttrUse("DEFECT_COLUMN_VISIBILITY", null, sEqptAttr))
                            blCode = "LBL0306";
                        else
                            blCode = LoginInfo.CFG_LABEL_TYPE;
                    }
                    else
                        blCode = LoginInfo.CFG_LABEL_TYPE;
                }
                //---------------------------------------------------------------------------------------------------------------------------------
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CUT_YN", typeof(string));   // CUT라벨 발행여부
                inTable.Columns.Add("I_LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("I_PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("I_RESO", typeof(string));   // 해상도
                inTable.Columns.Add("I_PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("I_MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("I_MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("I_DARKNESS", typeof(string));   // 농도
                inTable.Columns.Add("SAMPLE_COMPANY", typeof(string));   // 샘플링 출하거래처
                inTable.Columns.Add("USER_NAME", typeof(string));   // 라벨발행 사용자

                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LOTID"] = sLotID;
                        indata["PROCID"] = procID;

                        if (string.Equals(LoginInfo.CFG_CUT_LABEL, "Y"))
                            indata["CUT_YN"] = LoginInfo.CFG_CUT_LABEL;

                        indata["I_LBCD"] = blCode;
                        indata["I_PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["I_RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["I_PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["I_MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["I_MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        indata["I_DARKNESS"] = string.IsNullOrEmpty(Util.NVC(row["DARKNESS"])) ? "15" : Util.NVC(row["DARKNESS"]);
                        indata["SAMPLE_COMPANY"] = sSampleCompany;
                        indata["USER_NAME"] = string.IsNullOrEmpty(userName) ? null : userName;
                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                if (inTable.Rows.Count < 1)
                    throw new Exception(MessageDic.Instance.GetMessage("SFU3030"));

                // 구버전, 신버전용 공정라벨 분리 [2017-05-25]
                // 초소형 라벨일 경우 기존 BIZ, 나머지 라벨은 모두 V01 BIZ 호출 하도록 수정 [2018-11-13]
                // CSR : [E20230720-001540] - ESOC2 통합슬리터 라벨 변경 요청의건 [2023.08.11]
                string sBizName = string.Empty;

                if (string.Equals(LoginInfo.CFG_LABEL_TYPE, "LBL0002"))
                    sBizName = "BR_PRD_GET_PROCESS_LOT_LABEL_COM";
                else if (string.Equals(LoginInfo.CFG_LABEL_TYPE, "LBL0329"))
                    sBizName = "BR_PRD_GET_PROCESS_LOT_LABEL_COM_V02";
                else if (string.Equals(LoginInfo.CFG_LABEL_TYPE.Substring(0, 7), "LBL0337"))    //공통코드에 8자리로 나와서 어쩔수 없이 7자리만 비교함
                    sBizName = "BR_PRD_GET_PROCESS_LOT_LABEL_COM_V01_NFF";  // NFF 슬리팅 신규라벨
                else
                    sBizName = "BR_PRD_GET_PROCESS_LOT_LABEL_COM_V01";

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                    return;

                if (!string.Equals(Util.NVC(dtMain.Rows[0]["I_ATTVAL"]).Substring(0, 1), "0"))
                    throw new Exception(MessageDic.Instance.GetMessage("SFU1309"));

                // 동시에 출력시 순서 뒤바끼는 문제때문에 SLEEP 추가
                foreach (DataRow row in dtMain.Rows)
                {
                    System.Threading.Thread.Sleep(500);
                    PrintLabel(frameOperation, loadingIndicator, Util.NVC(row["I_ATTVAL"]));
                }
            }
            catch (Exception ex) { throw ex; }
        }

        public static void UpdatePrintExecCount(string sLotID, string sProcID)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));

            DataRow dr = IndataTable.NewRow();
            dr["LOTID"] = sLotID;
            dr["PROCID"] = sProcID;
            IndataTable.Rows.Add(dr);
            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_PROCESS_LOT_LABEL_PRINT_EXEC_CNT", "INDATA", "RSLTDT", IndataTable);

        }


        public static void PrintLabel_OtherElec(IFrameOperation frameOperation, LoadingIndicator loadingIndicator, string sLotID, string procID)
        {
            try
            {
                string blCode = string.Empty;
                blCode = LoginInfo.CFG_LABEL_TYPE;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CUT_YN", typeof(string));   // CUT라벨 발행여부
                inTable.Columns.Add("I_LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("I_PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("I_RESO", typeof(string));   // 해상도
                inTable.Columns.Add("I_PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("I_MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("I_MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("I_DARKNESS", typeof(string));   // 농도
                inTable.Columns.Add("SAMPLE_COMPANY", typeof(string));   // 샘플링 출하거래처

                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LOTID"] = sLotID;
                        indata["PROCID"] = procID;

                        if (string.Equals(LoginInfo.CFG_CUT_LABEL, "Y"))
                            indata["CUT_YN"] = LoginInfo.CFG_CUT_LABEL;

                        indata["I_LBCD"] = blCode;
                        indata["I_PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["I_RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["I_PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["I_MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["I_MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        indata["I_DARKNESS"] = string.IsNullOrEmpty(Util.NVC(row["DARKNESS"])) ? "15" : Util.NVC(row["DARKNESS"]);
                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                if (inTable.Rows.Count < 1)
                    throw new Exception(MessageDic.Instance.GetMessage("SFU3030")); //프린터 환경설정 정보가 없습니다.

                string sBizName = string.Empty;

                sBizName = "BR_PRD_GET_PROCESS_LOT_LABEL_COM_V01_OTHER_SITE";

                DataTable dtMain = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                    return;

                if (!string.Equals(Util.NVC(dtMain.Rows[0]["I_ATTVAL"]).Substring(0, 1), "0"))
                    throw new Exception(MessageDic.Instance.GetMessage("SFU1309"));

                // 동시에 출력시 순서 뒤바끼는 문제때문에 SLEEP 추가
                foreach (DataRow row in dtMain.Rows)
                {
                    System.Threading.Thread.Sleep(500);
                    PrintLabel(frameOperation, loadingIndicator, Util.NVC(row["I_ATTVAL"]));
                }
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// TEST용 바코드 프린터 공통 [프린터 헤드 점검]
        /// </summary>
        /// <param name="frameOperation"></param>
        /// <param name="loadingIndicator"></param>
        public static void PrintLabel_Test(IFrameOperation frameOperation, LoadingIndicator loadingIndicator)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("RESO", typeof(string));   // 해상도
                inTable.Columns.Add("PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("DARKNESS", typeof(string));   // 농도

                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LBCD"] = "LBLTEST";
                        indata["PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        indata["DARKNESS"] = string.IsNullOrEmpty(Util.NVC(row["DARKNESS"])) ? "15" : Util.NVC(row["DARKNESS"]);
                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                if (inTable.Rows.Count < 1)
                    throw new Exception(MessageDic.Instance.GetMessage("SFU3030"));

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM15", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                    return;

                if (!string.Equals(Util.NVC(dtMain.Rows[0]["LABELCD"]).Substring(0, 1), "0"))
                    throw new Exception(MessageDic.Instance.GetMessage("SFU1309"));

                foreach (DataRow row in dtMain.Rows)
                {
                    System.Threading.Thread.Sleep(500);
                    PrintLabel(frameOperation, loadingIndicator, Util.NVC(row["LABELCD"]));
                }
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// PACK 에서 사용
        /// LOTID 필수입력
        /// LABELCODE미입력시 등록된 LABELCODE수만큼 ZPL
        /// </summary>
        /// <param name="sLOTID"></param>
        /// <param name="sPROCID"></param>
        /// <param name="sEQPTID"></param>
        /// <param name="sEQSGID"></param>
        /// <param name="sLABEL_TYPE">PACK:공정용, PACK_INBOX:BOX포장</param>
        /// <param name="sLABEL_CODE"></param>
        /// <param name="sSAMPLE_FLAG">샘플발행여부Y N</param>
        /// <param name="sPRN_QTY">프린트수량</param>
        /// <param name="sPRODID">제품코드LOTID없는경우 필수</param>
        /// <param name="sDPI">DPI값,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sLEFT">X축,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sTOP">Y축,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sDARKNESS">DPI값,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sSHIPTO_ID">출하처, 기본 null</param>
        /// <param name="sSRCTYPE"> 기본이 UI 이지만, 라벨 Pool 방식일시 EQ 처리를 위해서 추가 2021.06.10 염규범S </param>
        /// <returns>DataTable LABEL_TYPE,ZPLSTRING</returns>
        public static DataTable getZPL_Pack(string sLOTID = null
                                          , string sPROCID = null
                                          , string sEQPTID = null
                                          , string sEQSGID = null
                                          , string sLABEL_TYPE = null
                                          , string sLABEL_CODE = null
                                          , string sSAMPLE_FLAG = null
                                          , string sPRN_QTY = null
                                          , string sPRODID = null
                                          , string sDPI = null
                                          , string sLEFT = null
                                          , string sTOP = null
                                          , string sDARKNESS = null
                                          , string sSHIPTO_ID = null
                                          , string sSRCTYPE = null
                                        )
        {
            DataSet dsResult = null;
            DataTable dtResult = null;
            try
            {
                if (sDPI == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sDPI = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                            break;
                        }
                    }
                }
                if (sTOP == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sTOP = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                            break;
                        }
                    }
                }
                if (sLEFT == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sLEFT = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                            break;
                        }
                    }
                }
                if (sDARKNESS == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sDARKNESS = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                            break;
                        }
                    }
                }

                DataSet dsIndata = new DataSet();
                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("EQPTID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("LABEL_TYPE", typeof(string));
                dtINDATA.Columns.Add("LABEL_CODE", typeof(string));
                dtINDATA.Columns.Add("SAMPLE_FLAG", typeof(string));
                dtINDATA.Columns.Add("PRN_QTY", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("DPI", typeof(string));
                dtINDATA.Columns.Add("SHIPTO_ID", typeof(string));
                //2017.12.14
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                //2021.06.10
                //염규범 선임
                drINDATA["SRCTYPE"] = sSRCTYPE == null ? SRCTYPE.SRCTYPE_UI : sSRCTYPE;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = sLOTID;
                drINDATA["PROCID"] = sPROCID;
                drINDATA["EQPTID"] = sEQPTID;
                drINDATA["EQSGID"] = sEQSGID;
                drINDATA["LABEL_TYPE"] = sLABEL_TYPE;
                drINDATA["LABEL_CODE"] = sLABEL_CODE;
                drINDATA["SAMPLE_FLAG"] = sSAMPLE_FLAG;
                drINDATA["PRN_QTY"] = sPRN_QTY;
                drINDATA["PRODID"] = sPRODID;
                drINDATA["DPI"] = sDPI;
                drINDATA["SHIPTO_ID"] = sSHIPTO_ID;
                //2017.12.14
                drINDATA["USERID"] = LoginInfo.USERID;

                dtINDATA.Rows.Add(drINDATA);

                DataTable dtIN_OPTION = new DataTable();
                dtIN_OPTION.TableName = "IN_OPTION";
                dtIN_OPTION.Columns.Add("LEFT", typeof(string));
                dtIN_OPTION.Columns.Add("TOP", typeof(string));
                dtIN_OPTION.Columns.Add("DARKNESS", typeof(string));

                DataRow drIN_OPTION = dtIN_OPTION.NewRow();
                drIN_OPTION["LEFT"] = sLEFT;
                drIN_OPTION["TOP"] = sTOP;
                drIN_OPTION["DARKNESS"] = sDARKNESS;
                dtIN_OPTION.Rows.Add(drIN_OPTION);

                dsIndata.Tables.Add(dtINDATA);
                dsIndata.Tables.Add(dtIN_OPTION);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ZPL", "INDATA,IN_OPTION", "OUTDATA", dsIndata);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                    {
                        dtResult = dsResult.Tables["OUTDATA"];
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        /// <summary>
        /// PACK 에서 사용
        /// LOTID 필수입력
        /// LABELCODE미입력시 등록된 LABELCODE수만큼 ZPL
        /// </summary>
        /// <param name="sLOTID"></param>
        /// <param name="sPROCID"></param>
        /// <param name="sEQPTID"></param>
        /// <param name="sEQSGID"></param>
        /// <param name="sLABEL_TYPE">PACK:공정용, PACK_INBOX:BOX포장</param>
        /// <param name="sLABEL_CODE"></param>
        /// <param name="sSAMPLE_FLAG">샘플발행여부Y N</param>
        /// <param name="sPRN_QTY">프린트수량</param>
        /// <param name="sPRODID">제품코드LOTID없는경우 필수</param>
        /// <param name="sDPI">DPI값,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sLEFT">X축,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sTOP">Y축,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sDARKNESS">DPI값,  null 값이면 환경설정값으로  사용함. 기본 null</param>
        /// <param name="sSHIPTO_ID">출하처, 기본 null</param>
        /// <param name="strUserId">출하처, 기본 null</param>
        /// <returns>DataTable LABEL_TYPE,ZPLSTRING</returns>
        public static DataTable getZPL_Pack_Temp(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator
                                          , string sLOTID = null
                                          , string sPROCID = null
                                          , string sEQPTID = null
                                          , string sEQSGID = null
                                          , string sLABEL_TYPE = null
                                          , string sLABEL_CODE = null
                                          , string sSAMPLE_FLAG = null
                                          , string sPRN_QTY = null
                                          , string sPRODID = null
                                          , string sDPI = null
                                          , string sLEFT = null
                                          , string sTOP = null
                                          , string sDARKNESS = null
                                          , string sSHIPTO_ID = null
                                          , string strUserId = null
                                        )
        {
            DataSet dsResult = null;
            DataTable dtResult = null;
            try
            {
                if (sDPI == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sDPI = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                            break;
                        }
                    }
                }
                if (sTOP == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sTOP = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                            break;
                        }
                    }
                }
                if (sLEFT == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sLEFT = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                            break;
                        }
                    }
                }
                if (sDARKNESS == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sDARKNESS = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                            break;
                        }
                    }
                }

                DataSet dsIndata = new DataSet();
                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("LOTID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("EQPTID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("LABEL_TYPE", typeof(string));
                dtINDATA.Columns.Add("LABEL_CODE", typeof(string));
                dtINDATA.Columns.Add("SAMPLE_FLAG", typeof(string));
                dtINDATA.Columns.Add("PRN_QTY", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("DPI", typeof(string));
                dtINDATA.Columns.Add("SHIPTO_ID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = sLOTID;
                drINDATA["PROCID"] = sPROCID;
                drINDATA["EQPTID"] = sEQPTID;
                drINDATA["EQSGID"] = sEQSGID;
                drINDATA["LABEL_TYPE"] = sLABEL_TYPE;
                drINDATA["LABEL_CODE"] = sLABEL_CODE;
                drINDATA["SAMPLE_FLAG"] = sSAMPLE_FLAG;
                drINDATA["PRN_QTY"] = sPRN_QTY;
                drINDATA["PRODID"] = sPRODID;
                drINDATA["DPI"] = sDPI;
                drINDATA["SHIPTO_ID"] = sSHIPTO_ID;
                drINDATA["USERID"] = strUserId;

                dtINDATA.Rows.Add(drINDATA);

                DataTable dtIN_OPTION = new DataTable();
                dtIN_OPTION.TableName = "IN_OPTION";
                dtIN_OPTION.Columns.Add("LEFT", typeof(string));
                dtIN_OPTION.Columns.Add("TOP", typeof(string));
                dtIN_OPTION.Columns.Add("DARKNESS", typeof(string));

                DataRow drIN_OPTION = dtIN_OPTION.NewRow();
                drIN_OPTION["LEFT"] = sLEFT;
                drIN_OPTION["TOP"] = sTOP;
                drIN_OPTION["DARKNESS"] = sDARKNESS;
                dtIN_OPTION.Rows.Add(drIN_OPTION);

                dsIndata.Tables.Add(dtINDATA);
                dsIndata.Tables.Add(dtIN_OPTION);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ZPL", "INDATA,IN_OPTION", "OUTDATA", dsIndata);

                if (dsResult != null && dsResult.Tables.Count > 0)
                {
                    for (int i = 0; i < dsResult.Tables["OUTDATA"].Rows.Count; i++)
                    {
                        string zpl = Util.NVC(dsResult.Tables["OUTDATA"].Rows[i]["ZPLSTRING"]);
                        PrintLabel(FrameOperation, loadingIndicator, zpl);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return dtResult;
        }

        /// <summary>
        /// PACK에서 사용
        /// zpl조회후 기본프린터기로 라벨발행
        /// </summary>
        /// <param name="FrameOperation"></param>
        /// <param name="loadingIndicator"></param>
        /// <param name="sLOTID"></param>
        /// <param name="sLABEL_TYPE">PACK:공정용, PACK_INBOX:BOX포장</param>
        /// <param name="sSAMPLE_FLAG">샘플출력Y/N</param>
        /// <param name="sPRN_QTY">프린트수량</param>
        /// <param name="sPRODID">제품코드LOTID없는경우 필수</param>
        public static void printLabel_Pack(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, string sLOTID, string sLABEL_TYPE, string sSAMPLE_FLAG, string sPRN_QTY, string sPRODID)
        {
            try
            {
                DataTable dtResult = getZPL_Pack(sLOTID: sLOTID
                                                , sLABEL_TYPE: sLABEL_TYPE
                                                , sSAMPLE_FLAG: sSAMPLE_FLAG
                                                , sPRN_QTY: sPRN_QTY
                                                , sPRODID: sPRODID
                                                );

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                    PrintLabel(FrameOperation, loadingIndicator, zpl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// PACK에서 사용
        /// 라벨 프린트 설정된값으로 프린트,
        /// zpl조회후 기본프린터기로 라벨발행
        /// </summary>
        /// <param name="FrameOperation"></param>
        /// <param name="loadingIndicator"></param>
        /// <param name="sLOTID"></param>
        /// <param name="sLABEL_TYPE">PACK:공정용, PACK_INBOX:BOX포장</param>
        /// <param name="sSAMPLE_FLAG">샘플출력Y/N</param>
        /// <param name="sPRN_QTY">프린트수량</param>
        /// <param name="sPRODID">제품코드LOTID없는경우 필수</param>
        public static void printLabel_Pack_New(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, string sLOTID, string sLABEL_TYPE, string sSAMPLE_FLAG, string sPRN_QTY, string sPRODID, string strEqptid, string strProcid,
            string strDpi, string strLeft, string strTop, string strDrakness)
        {
            try
            {
                DataTable dtResult = getZPL_Pack(sLOTID: sLOTID
                                                , sLABEL_TYPE: sLABEL_TYPE
                                                , sSAMPLE_FLAG: sSAMPLE_FLAG
                                                , sPRN_QTY: sPRN_QTY
                                                , sPRODID: sPRODID
                                                , sEQPTID: strEqptid
                                                , sPROCID: strProcid
                                                );

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                    PrintLabel(FrameOperation, loadingIndicator, zpl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// PACK에서 사용
        /// zpl조회후 기본프린터기로 라벨발행
        /// </summary>
        /// <param name="FrameOperation"></param>
        /// <param name="loadingIndicator"></param>
        /// <param name="sLOTID"></param>
        /// <param name="sLABEL_TYPE">PACK:공정용, PACK_INBOX:BOX포장</param>
        /// <param name="sLABEL_CODE">라벨양식코드</param>
        /// <param name="sSAMPLE_FLAG">샘플출력Y/N</param>
        /// <param name="sPRN_QTY">발행수</param>
        /// <param name="sPRODID">제품코드LOTID없는경우 필수</param>
        public static void printLabel_Pack(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, string sLOTID, string sLABEL_TYPE, string sLABEL_CODE, string sSAMPLE_FLAG, string sPRN_QTY, string sPRODID)
        {
            try
            {
                DataTable dtResult = getZPL_Pack(sLOTID: sLOTID
                                               , sLABEL_TYPE: sLABEL_TYPE
                                               , sLABEL_CODE: sLABEL_CODE
                                               , sSAMPLE_FLAG: sSAMPLE_FLAG
                                               , sPRN_QTY: sPRN_QTY
                                               , sPRODID: sPRODID
                                               );

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                    PrintLabel(FrameOperation, loadingIndicator, zpl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 기본프린터기로 라벨발행
        /// </summary>
        /// <param name="FrameOperation"></param>
        /// <param name="loadingIndicator"></param>
        /// <param name="sZPL"></param>
        public static void PrintLabel(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, string sZPL)
        {
            try
            {
                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                    {
                        FrameOperation.PrintFrameMessage(string.Empty);
                        //bool brtndefault = FrameOperation.Barcode_ZPL_Print(dr, sZPL);
                        bool brtndefault = PrintZpl_COM_LTP_USB(FrameOperation, dr, sZPL, NVC(dr[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME]));
                        if (brtndefault == false)
                        {
                            loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                            FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));        //Barcode Print 실패
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 기본프린터기로 라벨발행
        /// 라벨 ZPL 이 있는 프린트기로
        /// </summary>
        /// <param name="FrameOperation"></param>
        /// <param name="loadingIndicator"></param>
        /// <param name="sZPL"></param>
        public static Boolean PrintLabelPackPlt(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, DataRow drPrintSetting, string strPrintPortName, string sZPL)
        {
            try
            {
                FrameOperation.PrintFrameMessage(string.Empty);

                bool brtndefault = PrintZpl_COM_LTP_USB(FrameOperation, drPrintSetting, sZPL, strPrintPortName);

                if (brtndefault == false)
                {
                    loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                    FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));        //Barcode Print 실패
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        //2017.10.27
        public static bool PrintLabel_AutoPrint_PACK(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, DataTable dtZpl)
        {
            bool bReturn = true;
            try
            {
                foreach (DataRow drZpl in dtZpl.Rows)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    //bool brtndefault = FrameOperation.Barcode_ZPL_Print(dr, Util.NVC(drZpl["ZPL"]));
                    bool brtndefault = PrintZpl_COM_LTP_USB(FrameOperation, drZpl, Util.NVC(drZpl["ZPL"]), Util.NVC(drZpl["PORTNAME"]));
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));     //Barcode Print 실패
                        bReturn = false;
                        return bReturn;
                    }
                }

                return bReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool PrintLabel(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, DataTable dtZpl)
        {
            bool bReturn = true;
            try
            {
                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    foreach (DataRow drZpl in dtZpl.Rows)
                    {
                        if (dr[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME].ToString() == Util.NVC(drZpl["PORTNAME"]))
                        {
                            FrameOperation.PrintFrameMessage(string.Empty);
                            //bool brtndefault = FrameOperation.Barcode_ZPL_Print(dr, Util.NVC(drZpl["ZPL"]));
                            bool brtndefault = PrintZpl_COM_LTP_USB(FrameOperation, dr, Util.NVC(drZpl["ZPL"]), Util.NVC(drZpl["PORTNAME"]));
                            if (brtndefault == false)
                            {
                                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));     //Barcode Print 실패
                                bReturn = false;
                                return bReturn;
                            }
                        }
                    }
                }
                return bReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool PrintZpl_COM_LTP_USB(IFrameOperation FrameOperation, DataRow dr, string sZPL, string sPortName)
        {
            bool bPrintResult = false;

            if (sPortName.Contains("COM"))
            {
                bPrintResult = FrameOperation.Barcode_ZPL_Print(dr, sZPL);
            }
            else if (sPortName.Contains("LPT"))
            {
                bPrintResult = FrameOperation.Barcode_ZPL_LPT_Print(dr, sZPL);
            }
            else if (sPortName.Contains("USB"))
            {
                bPrintResult = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
            }
            else
            {
                bPrintResult = FrameOperation.Barcode_ZPL_Print(dr, sZPL);
            }


            return bPrintResult;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sLBCD">라벨코드</param>
        /// <param name="sPRMK">프린터(Z:Zebra)</param>
        /// <param name="sRESO">해상도(dpi)</param>
        /// <param name="sPRCN">프린트수량</param>
        /// <param name="sMARH">LEFT여백</param>
        /// <param name="sMARV">TOP여백</param>
        /// <param name="sDARK">Darkness</param>
        /// <param name="sATTVAL">ITEM STRING(ITEM001=값^ITEM002=값...)</param>
        /// <returns></returns>
        public static DataTable getDirectZpl(string sLBCD = ""
                                           , string sPRMK = "Z"
                                           , string sRESO = null
                                           , string sPRCN = "1"
                                           , string sMARH = null
                                           , string sMARV = null
                                           , string sDARK = null
                                           , string sATTVAL = ""
                                            )
        {
            DataTable dtResult = null;

            try
            {
                if (sRESO == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sRESO = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                            break;
                        }
                    }
                }
                if (sMARH == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sMARH = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                            break;
                        }
                    }
                }
                if (sMARV == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sMARV = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                            break;
                        }
                    }
                }

                if (sDARK == null)
                {
                    foreach (DataRow drConfig in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            sDARK = drConfig[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                            break;
                        }
                    }
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("I_LBCD", typeof(string));
                RQSTDT.Columns.Add("I_PRMK", typeof(string));
                RQSTDT.Columns.Add("I_RESO", typeof(string));
                RQSTDT.Columns.Add("I_PRCN", typeof(string));
                RQSTDT.Columns.Add("I_MARH", typeof(string));
                RQSTDT.Columns.Add("I_MARV", typeof(string));
                RQSTDT.Columns.Add("I_DARK", typeof(string));
                RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["I_LBCD"] = sLBCD;
                dr["I_PRMK"] = sPRMK;
                dr["I_RESO"] = sRESO;
                dr["I_PRCN"] = sPRCN;
                dr["I_MARH"] = sMARH;
                dr["I_MARV"] = sMARV;
                dr["I_DARK"] = sDARK;
                dr["I_ATTVAL"] = sATTVAL;

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_UI", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dtResult;
        }

        /// <summary>
        /// 2019.11.05 염규범S
        /// 소켓 데이터 처리
        /// Pack 다이렉트 프린트를 위한 UI
        /// </summary>
        /// <param name="sIP"></param>
        /// <param name="nPort"></param>
        /// <param name="msg"></param>
        /// <param name="timeOut"></param>
        /// <param name="return_yn"></param>
        /// <returns></returns>
        public string SendPrint(string sIP, Int16 nPort, string msg, Int32 timeOut, Boolean return_yn)
        {
            String responseData = String.Empty;
            try
            {
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                if (!client.Connected)
                {
                    client.Connect(sIP, nPort);
                }

                Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);

                System.Net.Sockets.NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);
                stream.Flush();
                data = new Byte[256];
                if (return_yn)
                {
                    stream.ReadTimeout = timeOut;
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                }
                stream.Close();
                stream = null;
                if (client.Connected)
                {
                    client.Close();
                    client = null;
                }
                return responseData;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return responseData = "ERRORS:         1";
            }
        }


        #endregion

        #region isNumber : 주어진 문자가 숫자형인지 검사한다.
        /// <summary>
        /// 주어진 문자가 숫자형인지 검사한다.
        /// </summary>
        /// <param name="sNumber">검사대상 문자열</param>
        /// <returns>숫자형이면 True,
        ///          문자형이면 false</returns>
        public static bool isNumber(string sNumber)
        {
            char[] sDT = sNumber.ToCharArray(0, sNumber.Length);

            try
            {
                for (int i = 0; i < sDT.Length; i++)
                    if (sDT[i] < '0' || sDT[i] > '9')
                        return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
        #endregion

        #region isNumber : 주어진 문자가 숫자형인지 검사한다.
        /// <summary>
        /// 주어진 문자가 숫자형인지 검사한다.
        /// </summary>
        /// <param name="sNumber">검사대상 문자열</param>
        /// <returns>숫자형이면 True,
        ///          문자형이면 false</returns>
        public static bool isNumber(string sNumber, char cExceptValue)
        {
            char[] sDT = sNumber.ToCharArray(0, sNumber.Length);

            try
            {
                for (int i = 0; i < sDT.Length; i++)
                    if ((sDT[i] < '0' || sDT[i] > '9') && sDT[i] != cExceptValue)
                        return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
        #endregion

        #region IS_NUMBER /// 주어진 문자가 숫자형인지 검사한다.
        /// <summary>
        /// 주어진 문자가 숫자형인지 검사한다.
        /// </summary>
        /// <param name="sSTRING">검사대상 문자열</param>
        /// <returns>숫자형이면 True,
        ///          문자형이면 false</returns>
        public static bool IS_NUMBER(string sSTRING)
        {
            char[] sDT = sSTRING.ToCharArray(0, sSTRING.Length);

            try
            {
                for (int i = 0; i < sDT.Length; i++)
                {
                    if (sDT[i] != '.' && //sDT[i] != '-' &&
                        (sDT[i] < '0' || sDT[i] > '9'))
                        return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }//end sub
        #endregion

        // 사사오입 반올림
        // <summary>
        //  사사오입 반올림
        // </summary>
        // <param name="value">Value</param>
        // <param name="iDigits">반올림 자리수</param>
        public static double myRound(double value, int iDigits)
        {
            int iSign = Math.Sign(value);

            double dScale = Math.Pow(10.0, iDigits);
            double dRound = Math.Floor(Math.Abs(value) * dScale + 0.5);

            return (iSign * dRound / dScale);
        }

        #region DB 시간을 지정한 형식으로 변환하여 반환
        /// <summary>
        /// DB 시간을 지정한 형식으로 변환하여 반환
        /// </summary>
        /// <returns></returns>
        public void GetDateTimeFormat(string strFormat)
        {
            string strRtnVal = "";
            DateTime dtVal = new DateTime();

            DataTable IndataTable = new DataTable();
            DataRow Indata = IndataTable.NewRow();

            IndataTable.Rows.Add(Indata);
            new ClientProxy().ExecuteService("SEL_DBTIME", "INDATA", "OUTDATA", IndataTable, (DataResult, DataException) =>
            {

                if (DataException != null)
                {
                    //System.Windows.MessageBox.Show(MessageDic.Instance.GetMessage(DataException), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //return;
                }

                dtVal = Convert.ToDateTime(DataResult.Rows[0]["DBTIME"].ToString());
                strRtnVal = dtVal.ToString(strFormat);
                return;
            }
            );

        }
        #endregion

        /// <summary>
        /// DATA를 DATATablq로 반환함
        /// </summary>
        /// <param name="strTableName"></param>
        /// <param name="colName"></param>
        /// <param name="colValue"></param>
        /// <returns></returns>
        public DataTable CreateDataTable(string strTableName, string[] colName, string[][] colValue)
        {
            DataTable dt = new DataTable();
            DataColumn col;
            DataRow row;
            dt.TableName = strTableName;

            for (int i = 0; i < colName.Length; ++i)
            {
                col = new DataColumn();
                col.ColumnName = colName[i].ToString();
                dt.Columns.Add(col);
            }
            for (int r = 0; r < colValue.Length; ++r)
            {
                row = dt.NewRow();
                for (int c = 0; c < colValue[r].Length; ++c)
                {
                    row[colName[c]] = colValue[r][c].ToString();
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        /// <summary>
        ///   Sends the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Send(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(e);

                    // Note: Based on your requirements you may also need to fire events for:
                    // RoutedEvent = Keyboard.PreviewKeyDownEvent
                    // RoutedEvent = Keyboard.KeyUpEvent
                    // RoutedEvent = Keyboard.PreviewKeyUpEvent
                }
            }
        }

        #region MultOrgCode InData 생성
        public static string GetMultOrgCode(string[] sMultOrgCode)
        {
            string sInOrgCode = null;

            for (int i = 0; i < sMultOrgCode.Length; i++)
            {
                sInOrgCode = sInOrgCode + "'" + sMultOrgCode[i] + "',";
            }

            sInOrgCode = sInOrgCode.Substring(0, sInOrgCode.Length - 1);

            return sInOrgCode;
        }
        #endregion

        #region Multi InData 생성(다중선택 콤보)
        public static string GetMultiInData(IList<object> sMultiCode)
        {
            string sInCode = null;

            for (int i = 0; i < sMultiCode.Count; i++)
            {
                sInCode = sInCode + "'" + sMultiCode[i] + "',";
            }

            if (sMultiCode.Count > 0)
            {
                sInCode = sInCode.Substring(0, sInCode.Length - 1);
            }

            return sInCode;
        }
        #endregion

        #region Config에 설정된 Multi Org 가져오기
        public static string getConfigMultiOrgCode()
        {

            string[] orgcode = CustomConfig.Instance.CONFIG_COMMON_ORG;
            return GetMultOrgCode(orgcode);

            //string strOrgCode = string.Empty;
            //if (CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_ORG].Rows.Count > 0)
            //{
            //  for (int i = 0; i < CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_ORG].Rows.Count; i++)
            //  {
            //      if (i == 0)
            //      {
            //          strOrgCode = CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_ORG].Rows[i][CustomConfig.CONFIGTABLE_ORG_CODE].ToString();
            //      }
            //      else
            //      {
            //          strOrgCode = strOrgCode + "," + CustomConfig.Instance.ConfigSet.Tables[CustomConfig.CONFIGTABLE_ORG].Rows[i][CustomConfig.CONFIGTABLE_ORG_CODE].ToString();
            //      }
            //  }
            //}

            //return GetMultOrgCode(strOrgCode.Split(','));
        }
        #endregion


        #region DataGrid LoadedRowHeaderPresenter Row Header 순번 넣기
        /// <summary>
        /// DataGrid 단일 선택/해제
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="sColumnName">ColumnName</param>
        public void gridLoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }
            C1DataGrid dg = sender as C1DataGrid;

            e.Row.HeaderPresenter.Content = null;

            if (e.Row.Type == DataGridRowType.Item)
            {
                TextBlock tb = new TextBlock();
                tb.Text = (e.Row.Index + 1 - dg.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region DataGrid 단일 선택/해제
        /// <summary>
        /// DataGrid 단일 선택/해제
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="sColumnName">ColumnName</param>
        public void gridSelectionSingle(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName, MouseButtonEventArgs e)
        {
            if (dataGrid.CurrentRow == null || dataGrid.SelectedIndex == -1) return;

            if (e.ChangedButton.ToString().Equals("Left") && dataGrid.CurrentColumn.Name == sColumnName)
            {
                string bcheck = dataGrid.GetCell(dataGrid.CurrentRow.Index, dataGrid.CurrentColumn.Index).Value.ToString();

                if (bcheck == "True" || bcheck == "1")
                {
                    DataTableConverter.SetValue(dataGrid.Rows[dataGrid.CurrentRow.Index].DataItem, sColumnName, false);
                }
                else
                {
                    DataTableConverter.SetValue(dataGrid.Rows[dataGrid.CurrentRow.Index].DataItem, sColumnName, true);
                }

                dataGrid.CurrentRow = null;
            }
        }
        #endregion

        #region DataGrid 전체 선택/해제
        /// <summary>
        /// DataGrid 전체 선택/해제
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="sColumnName">ColumnName</param>
        public void gridSelectionAll(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName)
        {
            DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (((System.Windows.Controls.CheckBox)dataGrid.Columns[sColumnName].Header).IsChecked == true)
                    {
                        DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, sColumnName, true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, sColumnName, false);
                    }
                }
            }
        }
        #endregion

        #region DataGrid 전체 선택/해제(TYPE 1)
        /// <summary>
        /// DataGrid 전체 선택/해제TYPE 1
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="sColumnName">ColumnName</param>
        public void gridSelectionAll_TP1(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName, bool bCheckTp)
        {
            DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (bCheckTp == true)
                    {
                        DataTableConverter.SetValue(dataGrid.Rows[dataGrid.CurrentRow.Index].DataItem, sColumnName, true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dataGrid.Rows[dataGrid.CurrentRow.Index].DataItem, sColumnName, false);
                    }
                }
            }
        }
        #endregion


        #region DataGrid Manual 선택
        /// <summary>
        /// DataGrid 전체 Manual 선택
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="sColumnName">ColumnName</param>
        public void gridSelectionManual(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName, bool bManual)
        {
            DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dataGrid.Rows[i].DataItem, sColumnName, bManual);
                }
            }
        }
        #endregion

        #region DataTable 체크컬럼 추가
        /// <summary>
        /// DataTable 체크컬럼 추가
        /// </summary>
        /// <param name="dataGrid">DataTable</param>
        /// <param name="sColumnName">ColumnName</param>
        public DataTable gridCheckColumnAdd(DataTable dt, string sColumnName)
        {
            DataTable NewDT = dt.Clone();
            NewDT.Columns.Add(new DataColumn() { ColumnName = sColumnName, DataType = typeof(bool) });

            foreach (DataRow dr in dt.Rows)
            {
                DataRow NewRow = NewDT.NewRow();
                foreach (DataColumn col in dt.Columns)
                {
                    NewRow[col.ColumnName] = dr[col.ColumnName];
                }

                NewRow[sColumnName] = false;
                NewDT.Rows.Add(NewRow);
            }

            return NewDT;
        }

        #endregion

        #region DataTable SUM컬럼 추가
        /// <summary>
        /// DataTable SUM컬럼 추가
        /// </summary>
        /// <param name="dataGrid">DataTable</param>
        /// <param name="sColumnName">ColumnName</param>
        public DataTable gridSumColumnAdd(DataTable dt, string sColumnName)
        {
            dt.Columns.Add(sColumnName, typeof(string));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i][sColumnName] = "SUM";
            }

            return dt;
        }

        #endregion

        #region DataGrid 선택 Row수 확인
        public int GetDataGridCheckCnt(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName)
        {
            int intReturn = 0;

            for (int i = 0; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)).Equals("1"))
                {
                    intReturn++;
                }
            }

            return intReturn;
        }
        #endregion

        #region DataGrid Columns Merge
        public void SetDataGridMergeExtensionCol(C1.WPF.DataGrid.C1DataGrid dataGrid, string[] sColumnName, DataGridMergeMode eMergeMode)
        {
            if (dataGrid.Rows.Count > 0)
            {
                for (int i = 0; i < sColumnName.Length; i++)
                {
                    DataGridMergeExtension.SetMergeMode(dataGrid.Columns[sColumnName[i].ToString()], eMergeMode); //DataGridMergeMode.VERTICALHIERARCHI);
                }
                dataGrid.ReMerge();
            }
        }
        #endregion

        #region [※ 숫자 ComboBox 넣기]
        public void SetNumberCombo(ComboBox cbo, int iMinVal, int iMaxVal)
        {
            try
            {
                DataSet dsTime = new DataSet();
                DataTable dtNumber = dsTime.Tables.Add("Time");//new DataTable();
                dtNumber.Columns.Add("CBO_NAME", typeof(string));
                dtNumber.Columns.Add("CBO_CODE", typeof(string));

                for (int iNum = iMaxVal; iNum >= iMinVal; iNum--)
                {
                    string sCobVal = iNum.ToString();

                    DataRow dr = dtNumber.NewRow();
                    dr["CBO_NAME"] = sCobVal;
                    dr["CBO_CODE"] = sCobVal;
                    dtNumber.Rows.InsertAt(dr, 0);
                }

                cbo.ItemsSource = DataTableConverter.Convert(dtNumber);

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion

        #region [※ 배열 ComboBox 넣기]
        public void SetValueCombo(ComboBox cbo, string sValue)
        {
            try
            {
                string[] sVal = sValue.Split(';');

                DataTable dtValue = new DataTable();
                dtValue.Columns.Add("CBO_NAME", typeof(string));
                dtValue.Columns.Add("CBO_CODE", typeof(string));

                for (int i = sVal.Length; i > 0; i--)
                {
                    string sCobVal = sVal[i - 1].ToString();

                    DataRow dr = dtValue.NewRow();
                    dr["CBO_NAME"] = sCobVal;
                    dr["CBO_CODE"] = sCobVal;
                    dtValue.Rows.InsertAt(dr, 0);
                }

                cbo.ItemsSource = DataTableConverter.Convert(dtValue);

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion

        #region [※ OK/NG ComboBox 넣기]
        public void SetOKNGCombo(ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();
                row["CBO_CODE"] = "";
                row["CBO_NAME"] = "-SELECT-";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "Y";
                row["CBO_NAME"] = "OK";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "N";
                row["CBO_NAME"] = "NG";
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }
        #endregion

        #region [※ 컨트롤 찾기]
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        #endregion

        #region [※ 그리드 칼럼 생성]

        /// <summary>
        /// 그리드 Text 칼럼 생성
        /// </summary>
        public static void SetGridColumnText(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, bool bUserResize, bool bUserSort,
                                       bool bUserFilter, bool bReadOnly, int nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                       Visibility ColumnVisibility, string sFormat = null)
        {
            C1.WPF.DataGrid.DataGridTextColumn col = new C1.WPF.DataGrid.DataGridTextColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            col.Width = new C1.WPF.DataGrid.DataGridLength(nHeaderWidth);
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            //col.AllowNull = true;

            if (!string.IsNullOrEmpty(sFormat))
            {
                col.Format = sFormat;
            }

            C1Grid.Columns.Add(col);
        }

        /// <summary>
        /// 그리드 Text 칼럼 생성 - HeaderWidth Auto
        /// </summary>
        public static void SetGridColumnText(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, bool bUserResize, bool bUserSort,
                                       bool bUserFilter, bool bReadOnly, C1.WPF.DataGrid.DataGridLength nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                       Visibility ColumnVisibility, string sFormat = null)
        {
            C1.WPF.DataGrid.DataGridTextColumn col = new C1.WPF.DataGrid.DataGridTextColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            col.Width = nHeaderWidth;
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            //col.AllowNull = true;

            if (!string.IsNullOrEmpty(sFormat))
            {
                col.Format = sFormat;
            }

            C1Grid.Columns.Add(col);
        }

        /// <summary>
        /// 그리드 Text 칼럼 생성 - HeaderWidth Auto, MinWidth
        /// </summary>
        public static void SetGridColumnText(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, bool bUserResize, bool bUserSort,
                                       bool bUserFilter, bool bReadOnly, C1.WPF.DataGrid.DataGridLength nHeaderWidth, int iMinWidth, HorizontalAlignment HorizontalAlignment,
                                       Visibility ColumnVisibility, string sFormat = null)
        {
            C1.WPF.DataGrid.DataGridTextColumn col = new C1.WPF.DataGrid.DataGridTextColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            col.Width = nHeaderWidth;
            col.MinWidth = iMinWidth;
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            //col.AllowNull = true;

            if (!string.IsNullOrEmpty(sFormat))
            {
                col.Format = sFormat;
            }

            C1Grid.Columns.Add(col);
        }

        /// <summary>
        /// 그리드 Text 칼럼 생성 - HeaderWidth Auto, MinWidth
        /// name
        /// </summary>
        public static void SetGridColumnTextName(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, string name, bool bUserResize, bool bUserSort,
                                       bool bUserFilter, bool bReadOnly, C1.WPF.DataGrid.DataGridLength nHeaderWidth, int iMinWidth, HorizontalAlignment HorizontalAlignment,
                                       Visibility ColumnVisibility, string sFormat = null)
        {
            C1.WPF.DataGrid.DataGridTextColumn col = new C1.WPF.DataGrid.DataGridTextColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            col.Width = nHeaderWidth;
            col.MinWidth = iMinWidth;
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            col.Name = name;

            if (!string.IsNullOrEmpty(sFormat))
            {
                col.Format = sFormat;
            }

            C1Grid.Columns.Add(col);
        }

        /// <summary>
        /// 그리드 Numeric 칼럼 생성
        /// </summary>
        public static void SetGridColumnNumeric(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, bool bUserResize, bool bUserSort,
                                          bool bUserFilter, bool bReadOnly, int nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                          Visibility ColumnVisibility, string sFormat = null, bool isShowButton = true, bool isHandleUpDown = true, bool isMinimum = true)
        {
            C1.WPF.DataGrid.DataGridNumericColumn col = new C1.WPF.DataGrid.DataGridNumericColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;

            if (nHeaderWidth > 0)
                col.Width = new C1.WPF.DataGrid.DataGridLength(nHeaderWidth);

            if (isMinimum == true)
                col.Minimum = 0;

            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            //col.AllowNull = true;

            if (!string.IsNullOrEmpty(sFormat))
            {
                col.Format = sFormat;
            }
            col.ShowButtons = isShowButton;
            col.HandleUpDownKeys = isHandleUpDown;

            C1Grid.Columns.Add(col);
        }

        /// <summary>
        /// 그리드 Numeric 칼럼 생성 (특정 위치에 insert)
        /// </summary>
        public static void SetGridColumnNumeric(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, bool bUserResize, bool bUserSort,
                                          bool bUserFilter, bool bReadOnly, int nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                          Visibility ColumnVisibility, int iInsertAt, string sFormat = null)
        {
            C1.WPF.DataGrid.DataGridNumericColumn col = new C1.WPF.DataGrid.DataGridNumericColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            if (nHeaderWidth > 0)
                col.Width = new C1.WPF.DataGrid.DataGridLength(nHeaderWidth);
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            //col.AllowNull = true;
            col.ShowButtons = false;

            if (!string.IsNullOrEmpty(sFormat))
            {
                col.Format = sFormat;
            }

            C1Grid.Columns.Insert(iInsertAt, col);
            col.DisplayIndex = iInsertAt;
        }

        public static void SetGridColumnNumeric(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, bool bUserResize, bool bUserSort,
                                          bool bUserFilter, bool bReadOnly, C1.WPF.DataGrid.DataGridLength nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                          Visibility ColumnVisibility, string sFormat = null, bool bEditOnSelection = false, bool bShowButton = true)
        {
            C1.WPF.DataGrid.DataGridNumericColumn col = new C1.WPF.DataGrid.DataGridNumericColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;
            col.Width = nHeaderWidth;
            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;
            //col.AllowNull = true;

            if (!string.IsNullOrEmpty(sFormat))
            {
                col.Format = sFormat;
            }

            if (bEditOnSelection && !bReadOnly)
            {
                col.EditOnSelection = true;
            }

            col.ShowButtons = bShowButton;

            C1Grid.Columns.Add(col);
        }

        public static void SetGridColumnCheckbox(C1DataGrid C1Grid, string sBinding, List<string> sHeadNames, string sHeadName, bool bUserResize, bool bUserSort,
                                          bool bUserFilter, bool bReadOnly, int nHeaderWidth, HorizontalAlignment HorizontalAlignment,
                                          Visibility ColumnVisibility)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn col = new C1.WPF.DataGrid.DataGridCheckBoxColumn();

            Binding databinding = new Binding(sBinding);
            databinding.Mode = BindingMode.TwoWay;
            //databinding.TargetNullValue = string.Empty;

            if (sHeadNames == null)
            {
                col.Header = sHeadName;
            }
            else
            {
                col.Header = sHeadNames;
            }

            col.Binding = databinding;
            col.CanUserResize = bUserResize;
            col.CanUserSort = bUserSort;
            col.CanUserFilter = bUserFilter;
            col.IsReadOnly = bReadOnly;

            if (nHeaderWidth > 0)
                col.Width = new C1.WPF.DataGrid.DataGridLength(nHeaderWidth);

            col.HorizontalAlignment = HorizontalAlignment;
            col.Visibility = ColumnVisibility;

            C1Grid.Columns.Add(col);
        }
        #endregion

        #region [※ 화면내 버튼 권한 처리]
        public static void pageAuth(List<Button> listAuth, string sAuth)
        {
            if (sAuth.Equals("R"))
            {
                foreach (Button a in listAuth)
                {
                    a.Visibility = Visibility.Collapsed;
                }
            }
        }

        public static bool pageAuthCheck(string sAuth)
        {
            bool bRet = false;

            if (sAuth.Equals("R"))
            {
                bRet = false;   // 권한 없음.
            }
            else
            {
                bRet = true;    // 권한 있음.
            }
            return bRet;
        }
        #endregion


        #region [※ Result DataSet Error Check]
        /// <summary>
        /// Result DataSet Error Check
        /// </summary>
        /// <param name="resultSet">Result DataSet</param>
        /// <param name="errorMsg">Error Msg</param>
        /// <returns>Error : false, No Error : true</returns>
        public static bool GetSvcErrorCheck(DataTable resultSet, out string errorMsg)
        {
            errorMsg = "";
            if (resultSet == null)
            {
                errorMsg += "Data Is Null";
                return false;
            }

            // Error Exist Check
            if (resultSet.TableName == "ERROR")
            {
                // "SVR_NAME", "FUNCTION", "DATA", "MESSAGE"
                foreach (DataRow dr in resultSet.Rows)
                {
                    errorMsg += dr["SVR_NAME"].ToString() + "/" + dr["FUNCTION"].ToString() + "/" + dr["DATA"].ToString() + "/" + dr["MESSAGE"].ToString() + "\n\r";
                }
                return false;
            }

            if (resultSet.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                errorMsg += "Data Empty";
                return false;
            }
        }

        /// <summary>
        /// Result DataSet Error Check
        /// </summary>
        /// <param name="resultSet">Result DataSet</param>
        /// <param name="errorMsg">Error Msg</param>
        /// <returns>Error : false, No Error : true</returns>
        public static bool GetSvcErrorCheck(string tableName, DataSet resultSet, out string errorMsg)
        {
            errorMsg = "";
            // Error Exist Check
            if (resultSet.Tables.Contains("ERROR"))
            {
                // "SVR_NAME", "FUNCTION", "DATA", "MESSAGE", "STACK_TRACE"
                foreach (DataRow dr in resultSet.Tables["ERROR"].Rows)
                {
                    errorMsg += dr["SVR_NAME"].ToString() + "/" + dr["FUNCTION"].ToString() + "/" + dr["DATA"].ToString() + "/" + dr["MESSAGE"].ToString() + "/" + dr["STACK_TRACE"].ToString() + "\n\r";
                }
                return false;
            }

            if (!resultSet.Tables.Contains(tableName))
            {
                errorMsg += "Table Name Not Found";
                return false;
            }

            if (resultSet.Tables.Count > 0)
            {
                if (resultSet.Tables[tableName].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    errorMsg += "Data Empty";
                    return false;
                }
            }
            else
            {
                errorMsg += "Data Empty";
                return false;
            }
        }

        #endregion

        public static void SetTextBlockText_DataGridRowCount(TextBlock tbSearchCount, string sRowCount)
        {
            try
            {
                tbSearchCount.Text = "[ " + sRowCount + ObjectDic.Instance.GetObjectName("건") + " ]";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// DataGrid의 TopRow를 Count 포함 Check 된 첫번째 Row Index 조회
        /// 필터가 걸린 경우 포함하여 Linq 식 재 작성
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="columnName"></param>
        /// <param name="isBoolType"></param>
        /// <returns></returns>
        public int GetDataGridFirstRowIndexWithTopRow(C1DataGrid dg, string columnName, bool isBoolType = false)
        {
            int idx = -1;

            if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return idx;

            C1.WPF.DataGrid.DataGridRow dr;

            if (isBoolType)
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "True"
                      select rows).FirstOrDefault();
            }
            else
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && (DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1" ||
                                DataTableConverter.GetValue(rows.DataItem, "CHK").GetString().ToUpper() == "TRUE")
                      select rows).FirstOrDefault();
            }

            if (dr != null)
            {
                idx = dr.Index;
            }

            return idx;

        }

        /// <summary>
        /// DataGrid의 Check 된 첫번째 Row Index 조회
        /// 필터가 걸린 경우 포함하여 Linq 식 재 작성
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="columnName"></param>
        /// <param name="isBoolType"></param>
        /// <returns></returns>
        public int GetDataGridFirstRowIndexByCheck(C1DataGrid dg, string columnName, bool isBoolType = false)
        {
            int idx = -1;

            if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return idx;

            C1.WPF.DataGrid.DataGridRow dr;

            if (isBoolType)
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "True"
                      select rows).FirstOrDefault();
            }
            else
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                      select rows).FirstOrDefault();
            }

            if (dr != null)
            {
                idx = dr.Index;
            }

            return idx;

        }


        public int GetDataGridLastRowIndexByCheck(C1DataGrid dg, string columnName, bool isBoolType = false)
        {
            int idx = -1;

            if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return idx;

            C1.WPF.DataGrid.DataGridRow dr;

            if (isBoolType)
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "True"
                      select rows).LastOrDefault();
            }
            else
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                      select rows).LastOrDefault();
            }

            if (dr != null)
            {
                idx = dr.Index;
            }

            return idx;

        }

        public int GetDataGridFirstRowIndexByColumnValue(C1DataGrid dg, string columnName, string columnValue)
        {
            int idx = -1;

            if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return idx;

            C1.WPF.DataGrid.DataGridRow dr;

            dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                  where rows.DataItem != null
                        && rows.Visibility == Visibility.Visible
                        && rows.Type == DataGridRowType.Item
                        && DataTableConverter.GetValue(rows.DataItem, columnName).GetString() == columnValue
                  select rows).FirstOrDefault();

            if (dr != null)
            {
                idx = dr.Index;
            }

            return idx;
        }

        public int GetDataGridFirstRowIndexByEquiptmentEnd(C1DataGrid dg, string columnName, string compareValue)
        {
            int idx = -1;
            if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return idx;
            C1.WPF.DataGrid.DataGridRow dr;

            dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                  where rows.DataItem != null
                        && rows.Visibility == Visibility.Visible
                        && rows.Type == DataGridRowType.Item
                        //&& DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                        && DataTableConverter.GetValue(rows.DataItem, columnName).GetString() == compareValue
                  orderby DataTableConverter.GetValue(rows.DataItem, "WIPDTTM_ED") descending
                  select rows).FirstOrDefault();

            if (dr != null)
            {
                idx = dr.Index;
            }

            return idx;
        }

        public DataRow GetDataGridFirstRowBycheck(C1DataGrid dg, string columnName, bool isBoolType = false)
        {
            DataRow row = null;

            if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return null;

            C1.WPF.DataGrid.DataGridRow dr;

            if (isBoolType)
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "True"
                      select rows).FirstOrDefault();
            }
            else
            {
                dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                      where rows.DataItem != null
                            && rows.Visibility == Visibility.Visible
                            && rows.Type == DataGridRowType.Item
                            && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "1"
                      select rows).FirstOrDefault();
            }

            DataRowView drv = dr?.DataItem as DataRowView;
            if (drv != null) row = drv.Row;

            return row;

        }

        /// <summary>
        /// 데이터 그리드의 체크된 Row Count 조회 Linq 식
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="columnName"></param>
        /// <param name="isBoolType"></param>
        /// <returns>Row Count</returns>
        public int GetDataGridRowCountByCheck(C1DataGrid dg, string columnName, bool isBoolType = false)
        {
            int idx = -1;
            if (!CommonVerify.HasDataGridRow(dg) || !dg.Rows.Any(x => x.Visibility == Visibility.Visible && x.Type == DataGridRowType.Item)) return idx;

            int rowCount;
            if (isBoolType)
            {
                rowCount = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                            where rows.DataItem != null
                                  && rows.Visibility == Visibility.Visible
                                  && rows.Type == DataGridRowType.Item
                                  && (DataTableConverter.GetValue(rows.DataItem, columnName).GetString() == "True" ||
                                      DataTableConverter.GetValue(rows.DataItem, columnName).GetString() == "1")
                            select rows).Count();
            }
            else
            {
                rowCount = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                            where rows.DataItem != null
                                  && rows.Visibility == Visibility.Visible
                                  && rows.Type == DataGridRowType.Item
                                  && DataTableConverter.GetValue(rows.DataItem, columnName).GetString() == "1"
                            select rows).Count();
            }

            return rowCount;
        }

        /// <summary>
        /// DataGrid DataTable 로 변환
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="isCopy">true:구조 및 데이터 복사 false:구조만 복사</param>
        /// <returns></returns>
        public static DataTable MakeDataTable(C1DataGrid dg, bool isCopy)
        {
            DataTable dt = new DataTable();
            try
            {
                if (dg.ItemsSource == null)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn column in dg.Columns)
                    {
                        if (!string.IsNullOrEmpty(column.Name))
                            dt.Columns.Add(column.Name);
                    }
                    return dt;
                }
                else
                {
                    if (isCopy)
                    {
                        dt = ((DataView)dg.ItemsSource).Table.Copy();
                        return dt;
                    }
                    else
                    {
                        dt = ((DataView)dg.ItemsSource).Table.Clone();
                        return dt;
                    }
                }
            }
            catch (Exception)
            {
                return dt;
            }
        }

        /// <summary>
        /// _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y");
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="colunm"></param>
        /// <param name="colunmValue"></param>
        /// <returns> DataRow </returns>
        public DataRow GetWorkOrderGridSelectedRow(C1DataGrid dg, string colunm, string colunmValue)
        {
            if (!CommonVerify.HasDataGridRow(dg)) return null;

            DataRow row = null;
            C1.WPF.DataGrid.DataGridRow dr;

            dr = (from C1.WPF.DataGrid.DataGridRow rows in dg.Rows
                  where rows.DataItem != null
                        && rows.Visibility == Visibility.Visible
                        && rows.Type == DataGridRowType.Item
                        && DataTableConverter.GetValue(rows.DataItem, colunm).GetString() == colunmValue
                  select rows).FirstOrDefault();

            DataRowView drv = dr?.DataItem as DataRowView;
            if (drv != null) row = drv.Row;

            return row;
        }

        public string Right(string text, int textLength)
        {
            if (text.Length < textLength)
            {
                textLength = text.Length;
            }
            string convertText = text.Substring(text.Length - textLength, textLength);
            return convertText;
        }

        public string Left(string text, int textLength)
        {
            if (textLength > text.Length)
            {
                textLength = text.Length;
            }

            //문자열 추출
            string convertText = text.Substring(0, textLength);
            return convertText;
        }

        /// <summary>
        /// DataGrid의 Check 된 첫번째 Row Index 조회
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName"></param>
        /// <returns>체크된 첫번째 Row Index</returns>
        public int GetDataGridCheckFirstRowIndex(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName)
        {
            int intReturn = -1;

            if (dataGrid == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return intReturn;

            if (!dataGrid.Columns.Contains(sColumnName))
                return intReturn;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                bool bReturn = false;

                //if (bool.TryParse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)),out bReturn) && bReturn)
                //{
                //    intReturn = i;
                //    break;

                //}
                //if (dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter != null
                //            && dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter.Content != null
                //            && (dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter.Content as CheckBox).IsChecked.HasValue
                //            && (bool)(dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter.Content as CheckBox).IsChecked)
                ////if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)))

                #region 2024.10.09 김영국 - 기존 로직 주석 (DB에서 값이 1 또는 0으로 올라오는 문제 대응로직 아래 구성함.
                //if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName))) // 2024.10.08 김영국 - DB에서 Object값이 String으로 넘어와 Int로 형변환
                //{
                //    intReturn = i;
                //    break;
                //}
                #endregion

                #region 2024.10.09 김영국 - DB에서 값이 1 또는 0으로 올라오는 문제 대응로직.
                bool rts = false;
                switch (DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName).GetString().ToUpper())
                {
                    case "1":
                    case "TRUE":
                        rts = true;
                        break;
                    case "0":
                    case "FALSE":
                        break;
                }

                if (Convert.ToBoolean(rts)) // 2024.10.08 김영국 - DB에서 Object값이 String으로 넘어와 Int로 형변환
                {
                    intReturn = i;
                    break;
                }
                #endregion
            }

            return intReturn;
        }

        /// <summary>
        /// DataGrid의 Check 된 첫번째 Row Index 조회
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName"></param>
        /// <returns>체크된 첫번째 Row Index</returns>
        public List<int> GetDataGridCheckRowIndex(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName)
        {
            List<int> value = new List<int>();

            if (dataGrid == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return value;

            if (!dataGrid.Columns.Contains(sColumnName))
                return value;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                bool bReturn = false;

                if (Convert.ToBoolean(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)))
                {
                    value.Add(i);
                }
            }

            return value;
        }

        /// <summary>
        /// DataGrid 리스트 Uncheck 처리
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName"></param>
        /// <param name="chkIndex">Check 처리할 Index</param>
        public void SetDataGridUncheck(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName, int chkIndex)
        {
            if (dataGrid.ItemsSource == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                if (i != chkIndex)
                {
                    if (dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter != null
                            && dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter.Content != null
                            && (dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter.Content as CheckBox).IsChecked.HasValue)
                        (dataGrid.GetCell(i, dataGrid.Columns[sColumnName].Index).Presenter.Content as CheckBox).IsChecked = false;
                }
            }
        }

        /// <summary>
        /// DataGrid 리스트 sChkValue와 동일한 값이 존재하면 Check 처리
        /// </summary>
        /// <param name="dataGrid">그리드 컨트롤</param>
        /// <param name="sChkBoxColumnName">CheckBox 컬럼명</param>
        /// <param name="sCompareColumnName">값을 비교할 컬럼명</param>
        /// <param name="sChkValue">비교 값</param>
        public void SetDataGridCheck(C1.WPF.DataGrid.C1DataGrid dataGrid, string sChkBoxColumnName, string sCompareColumnName, string sChkValue)
        {
            if (dataGrid.ItemsSource == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                if (DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sCompareColumnName).ToString().Equals(sChkValue))
                {
                    if (dataGrid.GetCell(i, dataGrid.Columns[sChkBoxColumnName].Index).Presenter != null
                            && dataGrid.GetCell(i, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content != null)
                    {
                        if (dataGrid.GetCell(i, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content is CheckBox)
                            (dataGrid.GetCell(i, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content as CheckBox).IsChecked = true;

                        else if (dataGrid.GetCell(i, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content is RadioButton)
                            (dataGrid.GetCell(i, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content as RadioButton).IsChecked = true;
                    }
                }
            }
        }
        public int SetDataGridCheck(C1.WPF.DataGrid.C1DataGrid dataGrid, string sChkBoxColumnName, string sCompareColumnName, string sChkValue, bool bScroll = false)
        {
            if (dataGrid.ItemsSource == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return -1;

            DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);
            DataRow dr = dtInfo.Select(sCompareColumnName + " = '" + sChkValue + "'").FirstOrDefault();

            if (dr == null)
                return -1;

            int i = dtInfo.Rows.IndexOf(dr);

            dr[sChkBoxColumnName] = bool.TrueString;
            dataGrid.ItemsSource = DataTableConverter.Convert(dtInfo);

            if (bScroll) dataGrid.ScrollIntoView(i, 0);
            return i;
        }

        /// <summary>
        /// Grid 전체 Column Width 'Auto' 로 설정.
        /// </summary>
        /// <param name="dgTaget"></param>
        public static void GridAllColumnWidthAuto(ref C1DataGrid dgTaget)
        {
            for (int i = 0; dgTaget.Columns.Count > i; i++)
            {
                dgTaget.Columns[i].Width = C1.WPF.DataGrid.DataGridLength.Auto;
            }
        }

        /// <summary>
        /// 파라미티 값이 숫자형인지 판정
        /// </summary>
        /// <param name="str"></param>
        /// <param name="dMinValue">허용 최소값</param>
        /// <returns>값 체크 결과</returns>
        public static bool CheckDecimal(string str, decimal dMinValue)
        {
            bool bRet = false;

            if (str.Trim().Equals(""))
                return bRet;

            decimal value;
            if (!decimal.TryParse(str, out value))
            {
                //숫자필드에 부적절한 값이 입력 되었습니다.
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2914"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return bRet;
            }
            if (value < dMinValue)
            {
                //숫자필드에 허용되지 않는 값이 입력 되었습니다.
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2915"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        /// <summary>
        /// 조건에 값 빼오기
        /// select 일때는 메시지 띄우기
        /// date 는 - 뺀거 가져오기
        /// </summary>
        /// <param name="str"></param>
        /// <param name="dMinValue">허용 최소값</param>
        /// <returns>값 체크 결과</returns>
        public static string GetCondition(object oCondition, string sMsg = "", bool bAllNull = false)
        {
            string sRet = "";
            switch (oCondition.GetType().Name)
            {
                case "LGCDatePicker":
                    LGCDatePicker lgcDp = oCondition as LGCDatePicker;
                    if (lgcDp.DatepickerType.ToString().Equals("Month"))
                    {
                        sRet = lgcDp.SelectedDateTime.ToString("yyyyMM");
                    }
                    else
                    {
                        sRet = lgcDp.SelectedDateTime.ToString("yyyyMMdd");
                    }

                    break;
                case "C1ComboBox":
                case "UcBaseComboBox":
                    C1ComboBox cb = oCondition as C1ComboBox;

                    if (cb.SelectedIndex < 0)
                    {
                        if (!sMsg.Equals(""))
                        {
                            Util.Alert(sMsg);
                        }
                        break;
                    }
                    if (cb.SelectedValue != null) sRet = cb.SelectedValue.ToString();

                    if (sRet.Equals("SELECT"))
                    {
                        sRet = "";
                        if (!sMsg.Equals(""))
                        {
                            Util.Alert(sMsg);
                        }
                        break;
                    }
                    else if (sRet.Equals(""))
                    {
                        if (bAllNull)
                        {
                            sRet = null;
                        }
                    }

                    break;
                case "TextBox":
                    TextBox tb = oCondition as TextBox;
                    sRet = tb.Text;
                    if (sRet.Equals("") && !sMsg.Equals(""))
                    {
                        Util.Alert(sMsg);
                        break;
                    }
                    break;
                case "UcBaseTextBox":
                    UcBaseTextBox baseTb = oCondition as UcBaseTextBox;
                    sRet = baseTb.Text;
                    if (sRet.Equals("") && !sMsg.Equals(""))
                    {
                        Util.Alert(sMsg);
                        break;
                    }
                    break;
                case "CheckBox":
                case "UcBaseCheckBox":
                    CheckBox checkbox = oCondition as CheckBox;
                    sRet = checkbox.IsChecked == true ? "Y" : "N";
                    break;
            }
            return sRet;
        }

        /// <summary>
        /// 조건에 값 가져오기 다중 쓰레드 처리 할때
        /// select 일때는 메시지 띄우기
        /// date 는 - 뺀거 가져오기
        /// </summary>
        /// <param name="str"></param>
        /// <param name="dMinValue">허용 최소값</param>
        /// <returns>값 체크 결과</returns>
        public static string GetCondition_Thread(object oCondition, string sMsg = "", bool bAllNull = false)
        {
            string sRet = "";
            int idx = 0;
            string type = "";
            switch (oCondition.GetType().Name)
            {
                case "LGCDatePicker":
                    LGCDatePicker lgcDp = oCondition as LGCDatePicker;
                    lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        type = lgcDp.DatepickerType.ToString();
                    }));

                    if (type.Equals("Month"))
                    {
                        lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = lgcDp.SelectedDateTime.ToString("yyyyMM");
                        }));
                    }
                    else
                    {
                        lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = lgcDp.SelectedDateTime.ToString("yyyyMMdd");
                        }));
                    }
                    break;
                case "C1ComboBox":
                case "UcBaseComboBox":
                    C1ComboBox cb = oCondition as C1ComboBox;
                    cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        idx = cb.SelectedIndex;
                    }));

                    if (idx < 0)
                    {
                        if (!sMsg.Equals(""))
                        {
                            Util.Alert(sMsg);
                        }
                        break;
                    }

                    cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = cb.SelectedValue.ToString();
                    }));

                    if (sRet.Equals("SELECT"))
                    {
                        sRet = "";
                        if (!sMsg.Equals(""))
                        {
                            Util.Alert(sMsg);
                        }
                        break;
                    }
                    else if (sRet.Equals(""))
                    {
                        if (bAllNull)
                        {
                            sRet = null;
                        }
                    }
                    break;
                case "TextBox":
                case "UcBaseTextBox":
                    TextBox tb = oCondition as TextBox;
                    tb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = tb.Text;
                    }));

                    if (sRet.Equals("") && !sMsg.Equals(""))
                    {
                        Util.Alert(sMsg);
                        break;
                    }
                    break;
                case "C1NumericBox":
                    C1NumericBox nb = oCondition as C1NumericBox;
                    nb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = nb.Value.ToString();
                    }));
                    break;
                case "CheckBox":

                    CheckBox chk = oCondition as CheckBox;
                    chk.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = chk.IsChecked.ToString();
                    }));
                    break;
            }
            return sRet;
        }

        /// <summary>
        /// 조건에 값 넣기 다중 쓰레드 처리 할때
        /// </summary>
        /// <param name="oCondition">Control</param>
        /// <param name="sMsg">입력 값</param>
        /// <param name="bAllNull"></param>
        public static void SetCondition_Thread(object oCondition, string sMsg, bool bAllNull = false)
        {
            switch (oCondition.GetType().Name)
            {
                case "LGCDatePicker":
                    LGCDatePicker lgcDp = oCondition as LGCDatePicker;
                    lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        lgcDp.SelectedDateTime = Convert.ToDateTime(sMsg);
                    }));
                    break;
                case "C1ComboBox":
                case "UcBaseComboBox":
                    C1ComboBox cb = oCondition as C1ComboBox;
                    cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        cb.SelectedValue = sMsg;
                    }));
                    break;
                case "TextBox":
                case "UcBaseTextBox":
                    TextBox tb = oCondition as TextBox;
                    tb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        tb.Text = sMsg;
                    }));
                    break;
                case "TextBlock":

                    TextBlock tbk = oCondition as TextBlock;
                    tbk.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        tbk.Text = sMsg;
                    }));

                    break;
                case "C1NumericBox":
                    C1NumericBox nb = oCondition as C1NumericBox;
                    nb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        nb.Value = Convert.ToInt32(sMsg);
                    }));
                    break;
            }
        }

        /// <summary>
        /// 자주검사 계산 수식 (스칼라함수 FN_SFC_GET_SELF_INSP_VALUE와 동시 수정 필요)
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="func_exp">수식</param>
        /// <param name="func_cond">조건수식</param>
        /// <returns>값 체크 결과</returns>
        public static string GetSelfInspectionValue(DataRow[] rows, string func_exp, string func_cond)
        {
            if (rows.Length == 0)
                return "";

            decimal result = 0;

            try
            {
                // 수식처리
                switch (func_exp)
                {
                    case "SUM":
                        result = rows.Sum(row => (string.IsNullOrEmpty(row.Field<string>("CLCTVAL01")) || string.Equals(row.Field<string>("CLCTVAL01"), "NaN")) ? 0 : Convert.ToDecimal(row.Field<string>("CLCTVAL01")));
                        break;
                    case "SUBT":
                        result = rows.Max(row => (string.IsNullOrEmpty(row.Field<string>("CLCTVAL01")) || string.Equals(row.Field<string>("CLCTVAL01"), "NaN")) ? 0 : Convert.ToDecimal(row.Field<string>("CLCTVAL01"))) -
                                   rows.Min(row => (string.IsNullOrEmpty(row.Field<string>("CLCTVAL01")) || string.Equals(row.Field<string>("CLCTVAL01"), "NaN")) ? 0 : Convert.ToDecimal(row.Field<string>("CLCTVAL01")));
                        break;
                    case "AVG":
                        result = rows.Average(row => (string.IsNullOrEmpty(row.Field<string>("CLCTVAL01")) || string.Equals(row.Field<string>("CLCTVAL01"), "NaN")) ? 0 : Convert.ToDecimal(row.Field<string>("CLCTVAL01")));
                        break;
                    default:
                        result = (string.IsNullOrEmpty(Convert.ToString(rows[0]["CLCTVAL01"])) || string.Equals(rows[0]["CLCTVAL01"], "NaN")) ? 0 : Convert.ToDecimal(rows[0]["CLCTVAL01"]);
                        break;
                }

                // 조건처리
                if (string.Equals(func_cond, "ABS"))
                    result = Math.Abs(result);
                else if (string.Equals(func_cond, "ROUND"))
                    result = Math.Round(result, 0);
            }
            catch (Exception ex) { }
            return result == 0 ? "0" : Convert.ToString(result);
        }

        /// <summary>
        /// 메세지 박스 alert
        /// </summary>
        /// <param name="sMsg"></param>
        public static void Alert(string sMsg)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        }

        public static void Alert(string sMsg, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg, parameters), "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            else
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public static void AlertInfo(string sMsg)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None);
        }

        public static void AlertInfo(string sMsg, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg, parameters), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None);
            }
            else
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        public static void AlertByBiz(string sBizName, string sMsg, string sMsgDtl)
        {
            string sMsgTmp = "";

            sMsgTmp = MessageDic.Instance.GetMessage(Util.NVC(sMsg)) + System.Environment.NewLine + "( BIZ : " + Util.NVC(sBizName) + " )";

            //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(sMsgTmp, MessageDic.Instance.GetMessage(sMsgDtl), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(sMsgTmp, "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

            //if(bShowDtl)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(sMsgTmp, MessageDic.Instance.GetMessage(sMsgDtl), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
            //else
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(sMsgTmp, "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }

        /// <summary>
        /// Confirm 메세지 박스
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        public static void AlertConfirm(string messageId, Action<MessageBoxResult> callback)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            ControlsLibrary.MessageBox.Show(message, "", "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, callback);
        }

        public static void AlertError(Exception ex)
        {
            ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Exception 전체(Biz Return Exception 및 프로그램 Exception 에 대한 다국어 처리
        /// </summary>
        /// <param name="ex">Exception</param>
        public static void ShowExceptionMessages(Exception ex)
        {
            try
            {
                if (ex == null) return;

                if (ex.Data != null)
                {
                    if (ex.Data.Contains("TYPE"))
                    {
                        // Biz Exception.

                        string sCnvLng = "";
                        string sMsg = ex.Message;
                        string sParm = "";
                        if (ex.Data.Contains("PARA"))
                        {
                            sParm = ex.Data["PARA"].ToString();
                        }

                        if (sParm.Contains(":"))
                        {
                            string sOrg = sMsg;
                            string[] sParmList = sParm.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < sParmList.Length; i++)
                            {
                                sOrg = sOrg.Replace(sParmList[i], "%" + (i + 1).ToString());
                            }

                            sCnvLng = MessageDic.Instance.GetMessage(sOrg, sParmList);

                            //for (int i = 0; i < sParmList.Length; i++)
                            //{
                            //    sCnvLng = sCnvLng.Replace("%" + (i + 1).ToString(), sParmList[i]);
                            //}
                        }
                        else
                        {
                            sCnvLng = MessageDic.Instance.GetMessage(sMsg);
                        }

                        if (ex.Data["TYPE"].ToString().Equals("USER"))
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(sCnvLng, "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        else if (ex.Data["TYPE"].ToString().Equals("LOGIC"))
                        {
                            string sDtl = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
#if DEBUG
                            sDtl = sDtl + System.Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
#endif
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(sCnvLng, sDtl, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        else if (ex.Data["TYPE"].ToString().Equals("SYSTEM"))
                        {
                            string sDtl = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
#if DEBUG
                            sDtl = sDtl + System.Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
#endif
                            LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(sCnvLng, sDtl, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        // 일반 exceitpon.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    // 일반 exceitpon.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception excp)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(excp.Message, excp.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// DataGrid의 해당 CheckBox 체크 여부 확인
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sChkBoxColumnName">체크 박스 컬럼명</param>
        /// <param name="iRowIdx">Row Index</param>
        /// <returns>체크 여부</returns>
        public bool GetDataGridCheckValue(C1.WPF.DataGrid.C1DataGrid dataGrid, string sChkBoxColumnName, int iRowIdx)
        {
            bool bRet = false;

            if (dataGrid.ItemsSource == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return bRet;

            //if (dataGrid.GetCell(iRowIdx, dataGrid.Columns[sChkBoxColumnName].Index).Presenter != null
            //         && dataGrid.GetCell(iRowIdx, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content != null
            //         && (dataGrid.GetCell(iRowIdx, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content as CheckBox).IsChecked.HasValue
            //         && (bool)(dataGrid.GetCell(iRowIdx, dataGrid.Columns[sChkBoxColumnName].Index).Presenter.Content as CheckBox).IsChecked)

            // Top Row 추가에 따른 문제로 로직 추가.
            int iTmpRow = iRowIdx;
            if (dataGrid.TopRows.Count > 0) iTmpRow = iRowIdx - dataGrid.TopRows.Count;
            if (iTmpRow < 0) return bRet;

            DataTable dt = DataTableConverter.Convert(dataGrid.ItemsSource);

            if (dt.Columns.Contains(sChkBoxColumnName) && dt.Rows.Count > iTmpRow)
            {
                bRet = Convert.ToBoolean(dt.Rows[iTmpRow][sChkBoxColumnName]);
            }
            else
            {
                return bRet;
            }

            return bRet;
        }

        /// <summary>
        /// DataGrid에 해당 값과 동일한 값의 RowIndex 값 반환
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">찾을 컬럼</param>
        /// <param name="sCompareValue">찾을 값</param>
        /// <returns>RowIndex 값</returns>
        public int GetDataGridRowIndex(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName, string sCompareValue)
        {
            int iRet = -1;
            if (dataGrid.ItemsSource == null || dataGrid.Rows.Count - dataGrid.TopRows.Count - dataGrid.BottomRows.Count < 1)
                return iRet;

            for (int i = dataGrid.TopRows.Count; i < dataGrid.Rows.Count - dataGrid.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[i].DataItem, sColumnName)).Equals(sCompareValue))
                {
                    iRet = i;
                    break;
                }
            }

            return iRet;
        }

        /// <summary>
        /// DataGrid의 칼럼명(Name)의 ColIndex 값 반환
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sColumnName">찾을 컬럼</param>
        /// <returns>ColIndex 값</returns>
        public int GetDataGridColIndex(C1.WPF.DataGrid.C1DataGrid dataGrid, string sColumnName)
        {
            int iRet = -1;

            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                if (dataGrid.Columns[i].Name.ToString() == sColumnName)
                {
                    iRet = i;
                    break;
                }
            }

            return iRet;
        }

        /// <summary>
        /// 프린터 Config 정보 조회
        /// </summary>
        /// <param name="sPrt">프린터 타입</param>
        /// <param name="sRes">해상도</param>
        /// <param name="sCopy">발행수량</param>
        /// <param name="sXpos">Horizontal Start Position</param>
        /// <param name="sYpos">Vertical Start Position</param>
        /// <param name="drPrtInfo">프린터 처리할 Config Row 정보</param>
        /// <returns></returns>
        public bool GetConfigPrintInfo(out string sPrt, out string sRes, out string sCopy, out string sXpos, out string sYpos, out string sDark, out DataRow drPrtInfo)
        {
            try
            {
                sPrt = "";
                sRes = "";
                sCopy = "";
                sXpos = "";
                sYpos = "";
                sDark = "";
                drPrtInfo = null;

                if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                {
                    MessageValidation("SFU2003");
                    return false;
                }

                DataRow[] drTemp = LoginInfo.CFG_SERIAL_PRINT.Select("DEFAULT = 'True'");
                if (drTemp == null || drTemp.Length < 1)
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
                    {
                        sPrt = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["PRINTERTYPE"].ToString().Equals("") ? "Z" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["PRINTERTYPE"].ToString();
                        sRes = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DPI"].ToString().Equals("") ? "203" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DPI"].ToString();
                        sCopy = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["COPIES"].ToString().Equals("") ? "1" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["COPIES"].ToString();
                        sXpos = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["X"].ToString().Equals("") ? "0" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["X"].ToString();
                        sYpos = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["Y"].ToString().Equals("") ? "0" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["Y"].ToString();
                        sDark = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DARKNESS"].ToString().Equals("") ? "15" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DARKNESS"].ToString();
                        drPrtInfo = LoginInfo.CFG_SERIAL_PRINT.Rows[0];
                    }

                    else
                    {
                        MessageValidation("SFU2003");
                        return false;
                    }
                }
                else
                {
                    sPrt = drTemp[0]["PRINTERTYPE"].ToString().Equals("") ? "Z" : drTemp[0]["PRINTERTYPE"].ToString();
                    sRes = drTemp[0]["DPI"].ToString().Equals("") ? "203" : drTemp[0]["DPI"].ToString();
                    sCopy = drTemp[0]["COPIES"].ToString().Equals("") ? "1" : drTemp[0]["COPIES"].ToString();
                    sXpos = drTemp[0]["X"].ToString().Equals("") ? "0" : drTemp[0]["X"].ToString();
                    sYpos = drTemp[0]["Y"].ToString().Equals("") ? "0" : drTemp[0]["Y"].ToString();
                    sDark = drTemp[0]["DARKNESS"].ToString().Equals("") ? "15" : drTemp[0]["DARKNESS"].ToString();
                    drPrtInfo = drTemp[0];
                }
                return true;
            }
            catch (Exception ex)
            {
                sPrt = "";
                sRes = "";
                sCopy = "";
                sXpos = "";
                sYpos = "";
                sDark = "";
                drPrtInfo = null;

                Util.MessageException(ex);
                return false;
            }
        }



        public static void SetDataGridCurrentCell(C1.WPF.DataGrid.C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCell currentcell)
        {
            if (dataGrid.CurrentCell == null)
            {
                dataGrid.ScrollIntoView(currentcell.Row.Index, currentcell.Column.Index);
                currentcell.Presenter.IsSelected = true;
            }
            else
            {
                C1.WPF.DataGrid.DataGridCell beforecell = dataGrid.CurrentCell;
                if (beforecell.Presenter != null)
                {
                    beforecell.Presenter.IsSelected = false;
                }

                dataGrid.ScrollIntoView(currentcell.Row.Index, currentcell.Column.Index);
                if (currentcell.Presenter != null)
                {
                    currentcell.Presenter.IsSelected = true;
                }
                else
                {
                    dataGrid.CurrentCell = currentcell;
                }
            }
        }

        /// <summary>
        /// 팩 조회 카운트 콤보박스 set
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="sDisplay"></param>
        /// <param name="sValue"></param>
        /// <param name="iCountGap"></param>
        /// <param name="iMaxCount"></param>
        /// <param name="iMinCount"></param>
        public static void Set_Pack_cboListCoount(C1ComboBox cb, string sDisplay, string sValue, int iCountGap, int iMaxCount, int iMinCount)
        {
            DataTable dtCountList = new DataTable();
            dtCountList.Columns.Add(sDisplay);
            dtCountList.Columns.Add(sValue);

            DataRow dr = dtCountList.NewRow();
            dr[sDisplay] = "-ALL-";
            //2018.10.17
            //dr[sValue] = "9223372036854775807";
            dr[sValue] = "50000";
            dtCountList.Rows.Add(dr);

            for (int i = iMaxCount; i >= iMinCount; i = i - iCountGap)
            {
                dr = dtCountList.NewRow();
                dr[sDisplay] = (i).ToString();
                dr[sValue] = (i).ToString();
                dtCountList.Rows.Add(dr);
            }

            cb.ItemsSource = DataTableConverter.Convert(dtCountList);
            //cb.SelectedIndex = 1;
            cb.SelectedValue = iMinCount;
        }

        public static void Set_Pack_cboTimeList(C1ComboBox cb, string sDisplay, string sValue, string sDefaultSelected)
        {
            DataTable dtTimeList = new DataTable();
            dtTimeList.Columns.Add(sDisplay);
            dtTimeList.Columns.Add(sValue);

            DataRow dr = dtTimeList.NewRow();
            for (int i = 0; i < 25; i++)
            {
                dr = dtTimeList.NewRow();
                dr[sDisplay] = (i).ToString("D2");
                if (i == 24)
                {
                    dr[sValue] = "23:59:59";
                }
                else
                {
                    dr[sValue] = (i).ToString("D2") + ":00:00";
                }

                dtTimeList.Rows.Add(dr);
            }

            cb.ItemsSource = DataTableConverter.Convert(dtTimeList);

            if (sDefaultSelected.Length > 0)
            {
                cb.SelectedValue = sDefaultSelected;
            }
        }

        //2018.11.09
        public static void Set_Pack_cboTimeList2(C1ComboBox cb, string sDisplay, string sValue, string sDefaultSelected)
        {
            DataTable dtTimeList = new DataTable();
            dtTimeList.Columns.Add(sDisplay);
            dtTimeList.Columns.Add(sValue);

            DataRow dr = dtTimeList.NewRow();
            for (int i = 0; i < 60; i++)
            {
                dr = dtTimeList.NewRow();
                dr[sDisplay] = (i).ToString("D2");
                if (i == 59)
                {
                    //2019.10.17
                    //dr[sValue] = "23:59:00";
                    switch (LoginInfo.CFG_SHOP_ID.ToString())
                    {
                        case "A040":
                            dr[sValue] = "05:59:00";
                            break;

                        case "G481":
                            dr[sValue] = "05:59:00";
                            break;

                        case "G382":
                            dr[sValue] = "05:59:00";
                            break;

                        case "G451":
                            dr[sValue] = "05:59:00";
                            break;
                    }
                }
                else
                {
                    //2019.10.17
                    //dr[sValue] = "23:" + (i).ToString("D2") + ":00";
                    switch (LoginInfo.CFG_SHOP_ID.ToString())
                    {
                        case "A040":
                            dr[sValue] = "05:" + (i).ToString("D2") + ":00";
                            break;

                        case "G481":
                            dr[sValue] = "05:" + (i).ToString("D2") + ":00";
                            break;

                        case "G382":
                            dr[sValue] = "05:" + (i).ToString("D2") + ":00";
                            break;

                        case "G451":
                            dr[sValue] = "05:" + (i).ToString("D2") + ":00";
                            break;
                    }
                }

                dtTimeList.Rows.Add(dr);
            }

            cb.ItemsSource = DataTableConverter.Convert(dtTimeList);

            if (sDefaultSelected.Length > 0)
            {
                cb.SelectedValue = sDefaultSelected;
            }
        }

        public static string[] GetDirectoryList(string address, string userId, string pwd)
        {
            FtpWebRequest directoryListRequest = (FtpWebRequest)WebRequest.Create(address);
            directoryListRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            directoryListRequest.Credentials = new NetworkCredential(userId, pwd);

            using (FtpWebResponse directoryListResponse = (FtpWebResponse)directoryListRequest.GetResponse())
            {
                using (StreamReader directoryListResponseReader = new StreamReader(directoryListResponse.GetResponseStream()))
                {
                    string responseString = directoryListResponseReader.ReadToEnd();
                    string[] results = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    return results;
                }
            }
        }

        public static List<BitmapImage> GetRequestImageList(string address, string userId, string pwd)
        {
            List<BitmapImage> imageList = new List<BitmapImage>();
            List<string> fileNames = new List<string>();

            FtpWebRequest directoryListRequest = (FtpWebRequest)WebRequest.Create(address);
            directoryListRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            directoryListRequest.UsePassive = true;

            directoryListRequest.Credentials = new NetworkCredential(userId, pwd);

            using (FtpWebResponse directoryListResponse = (FtpWebResponse)directoryListRequest.GetResponse())
            {
                using (StreamReader directoryListResponseReader = new StreamReader(directoryListResponse.GetResponseStream()))
                {
                    string responseString = directoryListResponseReader.ReadToEnd();
                    fileNames = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                }
            }

            foreach (string fileName in fileNames)
            {
                if (!string.Equals(fileName.Substring(fileName.Length - 3, 3).ToUpper(), "BMP"))
                    continue;

                string filePath = address + fileName;

                WebClient result = new WebClient();
                result.Credentials = new NetworkCredential(userId, pwd);

                byte[] imageFile = result.DownloadData(filePath);

                MemoryStream stream = new MemoryStream(imageFile);
                stream.Seek(0, SeekOrigin.Begin);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();

                imageList.Add(image);
            }
            return imageList;
        }

        public static void MakeComboBoxSearchable(ComboBox targetComboBox)
        {
            targetComboBox.Loaded += TargetComboBox_Loaded;
        }

        private static void TargetComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var targetComboBox = sender as ComboBox;
            var targetTextBox = targetComboBox?.Template.FindName("PART_EditableTextBox", targetComboBox) as TextBox;

            if (targetTextBox == null)
                return;

            targetComboBox.Tag = "TextInput";
            targetComboBox.StaysOpenOnEdit = true;
            targetComboBox.IsEditable = true;
            targetComboBox.IsTextSearchEnabled = false;

            targetTextBox.TextChanged += (o, args) =>
            {
                var textBox = o as TextBox;

                var searchText = textBox.Text;

                if (targetComboBox.Tag.ToString() == "Selection")
                {
                    targetComboBox.Tag = "TextInput";
                    targetComboBox.IsDropDownOpen = true;
                }
                else
                {
                    if (targetComboBox.SelectionBoxItem != null)
                    {
                        targetComboBox.SelectedItem = null;
                        targetTextBox.Text = searchText;
                        //textBox.CaretIndex = MaxValue;
                    }

                    if (string.IsNullOrEmpty(searchText))
                    {
                        targetComboBox.Items.Filter = item => true;
                        targetComboBox.SelectedItem = default(object);
                    }
                    else
                    {
                        targetComboBox.Items.Filter = item =>
                                item.ToString().StartsWith(searchText, true, CultureInfo.InvariantCulture);
                    }

                    Keyboard.ClearFocus();
                    Keyboard.Focus(targetTextBox);
                    //targetTextBox.CaretIndex = MaxValue;
                    targetComboBox.IsDropDownOpen = true;
                }
            };

            targetComboBox.SelectionChanged += (o, args) =>
            {
                var comboBox = o as ComboBox;

                if (comboBox?.SelectedItem == null)
                    return;

                comboBox.Tag = "Selection";
            };
        }

        /// <summary>
        /// 핸드폰 자리수 Hypen(-) 반환
        /// </summary>
        /// <param name="phoneNo"></param>
        /// <returns></returns>
        public static string AutoHypenPhone(string phoneNo)
        {
            phoneNo = System.Text.RegularExpressions.Regex.Replace(phoneNo, @"\D", "");
            string returnText = string.Empty;
            if (phoneNo.Length < 4)
            {
                return phoneNo;
            }
            else if (phoneNo.Length < 7)
            {
                returnText += phoneNo.Substring(0, 3);
                returnText += "-";
                returnText += phoneNo.Substring(3, phoneNo.Length - 3);
                return returnText;
            }
            else if (phoneNo.Length < 11)
            {
                returnText += phoneNo.Substring(0, 3);
                returnText += "-";
                returnText += phoneNo.Substring(3, 3);
                returnText += "-";
                returnText += phoneNo.Substring(6, phoneNo.Length - 6);
                return returnText;
            }
            else
            {
                returnText += phoneNo.Substring(0, 3);
                returnText += "-";
                returnText += phoneNo.Substring(3, 4);
                returnText += "-";
                returnText += phoneNo.Substring(7, phoneNo.Length - 7);
                return returnText;
            }
        }

        /// <summary>
        /// DateTime 으로 요일 찾기
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        /// 2019-10-01 오화백 러시아,우크라이나어 추가
        public static string GetDayOfWeek(DateTime dateTime)
        {
            var d = dateTime.DayOfWeek;
            string returnText = string.Empty;

            switch (d)
            {
                case DayOfWeek.Friday:
                    if (LoginInfo.LANGID == "en-US") returnText = "Fri";
                    else if (LoginInfo.LANGID == "zh-CN") returnText = "金";
                    else if (LoginInfo.LANGID == "pl-PL") returnText = "Fri";
                    else if (LoginInfo.LANGID == "ru-RU") returnText = "Fri";
                    else if (LoginInfo.LANGID == "uk-UA") returnText = "Fri";
                    else returnText = "금";
                    break;

                case DayOfWeek.Monday:
                    if (LoginInfo.LANGID == "en-US") returnText = "Mon";
                    else if (LoginInfo.LANGID == "zh-CN") returnText = "月";
                    else if (LoginInfo.LANGID == "pl-PL") returnText = "Mon";
                    else if (LoginInfo.LANGID == "ru-RU") returnText = "Mon";
                    else if (LoginInfo.LANGID == "uk-UA") returnText = "Mon";
                    else returnText = "월";
                    break;

                case DayOfWeek.Saturday:
                    if (LoginInfo.LANGID == "en-US") returnText = "Sat";
                    else if (LoginInfo.LANGID == "zh-CN") returnText = "土";
                    else if (LoginInfo.LANGID == "pl-PL") returnText = "Sat";
                    else if (LoginInfo.LANGID == "ru-RU") returnText = "Sat";
                    else if (LoginInfo.LANGID == "uk-UA") returnText = "Sat";
                    else returnText = "토";
                    break;

                case DayOfWeek.Sunday:
                    if (LoginInfo.LANGID == "en-US") returnText = "Sun";
                    else if (LoginInfo.LANGID == "zh-CN") returnText = "日";
                    else if (LoginInfo.LANGID == "pl-PL") returnText = "Sun";
                    else if (LoginInfo.LANGID == "ru-RU") returnText = "Sun";
                    else if (LoginInfo.LANGID == "uk-UA") returnText = "Sun";
                    else returnText = "일";
                    break;

                case DayOfWeek.Thursday:
                    if (LoginInfo.LANGID == "en-US") returnText = "Thu";
                    else if (LoginInfo.LANGID == "zh-CN") returnText = "木";
                    else if (LoginInfo.LANGID == "pl-PL") returnText = "Thu";
                    else if (LoginInfo.LANGID == "ru-RU") returnText = "Thu";
                    else if (LoginInfo.LANGID == "uk-UA") returnText = "Thu";
                    else returnText = "목";
                    break;

                case DayOfWeek.Tuesday:
                    if (LoginInfo.LANGID == "en-US") returnText = "Tue";
                    else if (LoginInfo.LANGID == "zh-CN") returnText = "火";
                    else if (LoginInfo.LANGID == "pl-PL") returnText = "Tue";
                    else if (LoginInfo.LANGID == "ru-RU") returnText = "Tue";
                    else if (LoginInfo.LANGID == "uk-UA") returnText = "Tue";
                    else returnText = "화";
                    break;

                case DayOfWeek.Wednesday:
                    if (LoginInfo.LANGID == "en-US") returnText = "Wed";
                    else if (LoginInfo.LANGID == "zh-CN") returnText = "水";
                    else if (LoginInfo.LANGID == "pl-PL") returnText = "Wed";
                    else if (LoginInfo.LANGID == "ru-RU") returnText = "Wed";
                    else if (LoginInfo.LANGID == "uk-UA") returnText = "Wed";
                    else returnText = "수";
                    break;

                default:
                    break;
            }
            return returnText;
        }

        #region [※ 삭재 대기]

        ////GLABEL 서버 유형을 지정합니다
        //public static string LBL_ACCESS = LGC.GMES.MES.Common.Common.GLABELAccess;  // 개발:D , 운영:P

        ///// <summary>
        ///// PopUp 화면에 Lot 정보를 넘김.
        ///// </summary>
        //public struct LotInfo
        //{
        //    public static string LOTID = string.Empty;
        //    public static string WIPSEQ = string.Empty;
        //    public static string PROCID = string.Empty;
        //    public static string EQPTID = string.Empty;
        //    public static string ORGCODE = string.Empty;
        //    public static string REMARK = string.Empty;
        //    public static string STKR_TP_CODE = string.Empty;
        //    public static string STKR_ID = string.Empty;
        //    public static string SLT_NO = string.Empty;
        //    public static string PROCNAME = string.Empty;
        //    public static string EQPTNAME = string.Empty;
        //}

        //public struct ProcessEquipmentInfo
        //{
        //    public static string SITEID = string.Empty;
        //    public static string PRODGROUPID = string.Empty;
        //    public static string ORGCODE = string.Empty;
        //    public static string PRODID = string.Empty;
        //    public static string PCSGID = string.Empty;
        //    public static string PROCID = string.Empty;
        //    public static string PROCNAME = string.Empty;
        //    public static string EQSGID = string.Empty;
        //    public static string EQPTID = string.Empty;
        //    public static string EQPTNAME = string.Empty;
        //}

        ////TS 자재 투입용 정보
        //public struct MTLinputTS
        //{
        //    public static string LOTID = string.Empty;
        //    public static string PROCID = string.Empty;
        //    public static string PRODID = string.Empty;
        //    public static string EQPTID = string.Empty;
        //    public static string ORGCODE = string.Empty;
        //}

        ////TS PRODUCT 정보
        //public struct ProdInfoTS
        //{
        //    public static string AREAID = string.Empty;
        //    public static string SHOPID = string.Empty;
        //    public static string ORG_CODE = string.Empty;
        //    public static string PRODID = string.Empty;
        //    public static string CUSTOMERID = string.Empty;
        //}

        //// COMM PRODUCT 정보
        //public struct ProdInfoCMM
        //{
        //    public static string SELECT = string.Empty;
        //    public static string PRODTYPE = string.Empty;
        //    public static string MULTI_PRODTYPE = string.Empty;
        //    public static DataTable dtPRODUCT = null;
        //}

        ///// <summary>
        ///// 변경점관리를 위한 구조체-2015.12.07, by.KWR
        ///// </summary>
        //public struct ChangesInfo
        //{
        //    public static string AREAID = string.Empty;
        //    public static string INS_DATE = string.Empty;
        //    public static int INS_SEQ = 1;
        //}

        ////자재LOT 정보
        //public struct MtrlLotInfo
        //{
        //    public static string ORGCODE = string.Empty;
        //    public static string MTRLID = string.Empty;
        //    public static string MLOTID = string.Empty;
        //    public static int MLOTQTY = 0;
        //    public static string WORKER = string.Empty;
        //    public static string NOTE = string.Empty;
        //    public static string PARENTS = string.Empty;
        //    public static DataTable dtMLOTINFO = null;
        //}

        ////자재LOT 정보 DataSet
        //public struct DTMtrlLotInfo
        //{
        //    public static DataTable dtMtrlLotInfo = null;
        //    public static string WORKER = string.Empty;
        //}

        ////SetMtrlPellicle
        //public struct MtrlPellicle
        //{
        //    public static string ORGCODE = string.Empty;
        //    public static string LOTID = string.Empty;
        //    public static string GDNAME = string.Empty;
        //    public static string PRODLOTID = string.Empty;
        //    public static string ASSGNPROCID = string.Empty;
        //    public static string NOTE = string.Empty;
        //    public static string WORKER = string.Empty;
        //}

        //public struct NoworkInfo
        //{
        //    public static string PROCID = string.Empty;
        //    public static string EQPTNAME = string.Empty;
        //    public static string EQPTID = string.Empty;
        //    public static string SEARCH_DATE = string.Empty;
        //}

        //// 출하성적서 항목 복사용.
        //public struct ShipReportInfo
        //{
        //    public static string ORGCODE = string.Empty;
        //    public static string AREAID = string.Empty;
        //    public static string PROD_CLASS1 = string.Empty;
        //    public static string PROD_CLASS2 = string.Empty;
        //    public static string PROD_CLASS3 = string.Empty;
        //    public static string PRODID = string.Empty;
        //    public static string PRODNAME = string.Empty;
        //    public static string VENDORID = string.Empty;
        //    public static string VENDORNAME = string.Empty;
        //    public static string FILENAME = string.Empty;
        //    public static DataTable dtPRODUCT = null;
        //}

        //// 자재현황 - 자재정보변경
        //public struct MtrlInfoChange
        //{
        //    public static string ORGCODE = string.Empty;
        //    public static string MLOTID = string.Empty;
        //    public static string SUPPLIER_LOTID = string.Empty;
        //    public static string COA_LEVEL = string.Empty;
        //    public static string SPT_COND_CODE = string.Empty;
        //    public static string SIDE_TRANS_FLAG = string.Empty;
        //    public static string MLOT_ST_NAME = string.Empty;
        //    public static string MLOT_ST_CODE = string.Empty;
        //    public static string MLOT_TP_CODE = string.Empty;
        //}

        //// 출하정보(TS)
        //public struct PackListInfo
        //{
        //    public static string VENDCODE = string.Empty;
        //    public static string VENDNAME = string.Empty;
        //    public static long LOTQTY = 0;
        //    public static long GLOTQTY = 0;
        //    public static long BLOTQTY = 0;
        //    public static double LOTLENGTH = 0;
        //    public static double GLOTLENGTH = 0;
        //    public static double BLOTLENGTH = 0;
        //    public static string YIELD = string.Empty;
        //    public static string LOTNAME = string.Empty;
        //    public static string MODEL = string.Empty;  //PRODID
        //    public static string OQCRANK = string.Empty;
        //    public static string SLITPOSITION = string.Empty;
        //    public static string CUST_LOTID = string.Empty;
        //    public static string TS_TYPE = string.Empty;
        //    //추가 항목 정의
        //    public static string SHIPPING_LOTID = string.Empty;
        //}

        ///// <summary>
        ///// 생산계획 후 LOT생성(TS)
        ///// </summary>
        //public struct PoodPlanLotCreate
        //{
        //    public static string PRDTNYMD = string.Empty;
        //    public static string AREAID = string.Empty;
        //    public static string AREANM = string.Empty;
        //    public static string PRODCLASS2ID = string.Empty;
        //    public static string PRODCLASS2NM = string.Empty;
        //    public static string PARENTS = string.Empty;
        //}

        ///// <summary>
        ///// Label Print(TS)
        ///// </summary>
        //public struct TSLabelPrint
        //{
        //    public static DataTable PRINTINFO = null;
        //    public static string BARCODENO = string.Empty;
        //    public static string WORKER = string.Empty;
        //    public static string PARENTS = string.Empty;
        //    public static string ORGCODE = string.Empty;
        //}

        ////public static class GlobalConfigParam
        ////{
        ////    public static string SITEID = string.Empty;
        ////    public static string PRODGROUPID = string.Empty;
        ////    public static string MODELID = string.Empty;
        ////    public static string PRODID = string.Empty;
        ////    public static string PROCID = string.Empty;
        ////    public static string EQPTID = string.Empty;
        ////}

        ////Grobal DataSet
        //public struct DTGlobal
        //{
        //    public static DataTable DTGLOBAL = null;
        //    public static string WORKER = string.Empty;
        //}

        ///// <summary>
        ///// 부모창에서 PopUP창으로 선택된 Lot 정보 넘김.
        ///// 사용화면: 착공화면-비가동사유등록, LOT정보조회-외관측정조회
        ///// </summary>
        ///// <param name="lotID"></param>
        //public static void SetLotInfo(string lotID, string wipSeq, string procID, string eqptID, string orgCode
        //                            , string remark = "", string stkrType = "", string stkrID = "", string sltNo = ""
        //                            , string procName = "", string eqptName = "")
        //{
        //    LotInfo.LOTID = lotID;
        //    LotInfo.WIPSEQ = wipSeq;
        //    LotInfo.PROCID = procID;
        //    LotInfo.EQPTID = eqptID;
        //    LotInfo.ORGCODE = orgCode;
        //    LotInfo.REMARK = remark;
        //    LotInfo.STKR_TP_CODE = stkrType;
        //    LotInfo.STKR_ID = stkrID;
        //    LotInfo.SLT_NO = sltNo;
        //    LotInfo.PROCNAME = procName;
        //    LotInfo.EQPTNAME = eqptName;
        //}

        ///// <summary>
        ///// 메인프레임에서 작업자가 선택한 공정/설비 정보(PopUp용)
        ///// </summary>
        ///// <param name="currProcessEquipmentInfo"></param>
        ///// <param name="currentProcessEquipmentInfo"></param>
        //public static void SetProcessEquipmentinfo(string siteID, string pdgrID, string orgCode, string prodID, string pcsgID
        //                                         , string procID, string procName, string eqsgID, string eqptID, string eqptName)
        //{
        //    ProcessEquipmentInfo.SITEID = siteID;
        //    ProcessEquipmentInfo.PRODGROUPID = pdgrID;
        //    ProcessEquipmentInfo.ORGCODE = orgCode;
        //    ProcessEquipmentInfo.PRODID = prodID;
        //    ProcessEquipmentInfo.PCSGID = pcsgID;
        //    ProcessEquipmentInfo.PROCID = procID;
        //    ProcessEquipmentInfo.PROCNAME = procName;
        //    ProcessEquipmentInfo.EQSGID = eqsgID;
        //    ProcessEquipmentInfo.EQPTID = eqptID;
        //    ProcessEquipmentInfo.EQPTNAME = eqptName;
        //}

        ///// <summary>
        ///// TS 완공화면에서 자재투입시 정보(PopUp용)
        ///// </summary>
        ///// <param name="currProcessEquipmentInfo"></param>
        ///// <param name="currentProcessEquipmentInfo"></param>
        //public static void SetMTLinputTS(string orgCode, string procID, string prodID, string eqptID, string lotID)
        //{
        //    MTLinputTS.LOTID = lotID;
        //    MTLinputTS.PROCID = procID;
        //    MTLinputTS.PRODID = prodID;
        //    MTLinputTS.EQPTID = eqptID;
        //    MTLinputTS.ORGCODE = orgCode;
        //}

        ///// <summary>
        ///// TS 제품정보조회
        ///// </summary>
        ///// <param name="sAreaID"></param>
        ///// <param name="sShopID"></param>
        ///// <param name="sOrgCode"></param>
        ///// <param name="sProdID"></param>
        ///// <param name="sCustomerID"></param>
        //public static void SetProdInfoTS(string sAreaID, string sShopID, string sOrgCode, string sProdID, string sCustomerID)
        //{
        //    ProdInfoTS.AREAID = sAreaID;
        //    ProdInfoTS.SHOPID = sShopID;
        //    ProdInfoTS.ORG_CODE = sOrgCode;
        //    ProdInfoTS.PRODID = sProdID;
        //    ProdInfoTS.CUSTOMERID = sCustomerID;
        //}

        ////CMM 제품정보 조회
        //public static void SetProdInfoCMM(string sSelect, string sProdTp, string sProdMultiTp, DataTable dtPRODUCT)
        //{
        //    ProdInfoCMM.SELECT = sSelect;
        //    ProdInfoCMM.PRODTYPE = sProdTp;
        //    ProdInfoCMM.MULTI_PRODTYPE = sProdMultiTp;
        //    ProdInfoCMM.dtPRODUCT = dtPRODUCT;
        //}

        ///// <summary>
        ///// 메인프레임에서 작업자가 선택한 공정/설비 정보(PopUp용)
        ///// </summary>
        //public static void SetChangesInfo(string areaID, string ins_Date, int ins_Seq)
        //{
        //    ChangesInfo.AREAID = areaID;
        //    ChangesInfo.INS_DATE = ins_Date;
        //    ChangesInfo.INS_SEQ = ins_Seq;
        //}

        ///// <summary>
        ///// 자재 LOT정보
        ///// </summary>
        //public static void SetMtrlLotInfo(string sOrgCode, string sMtrlId, string sMLotId, int sMLotQty, string sWorker, string sNote, string sParents, DataTable dtMLotInfo)
        //{
        //    MtrlLotInfo.ORGCODE = sOrgCode;
        //    MtrlLotInfo.MTRLID = sMtrlId;
        //    MtrlLotInfo.MLOTID = sMLotId;
        //    MtrlLotInfo.MLOTQTY = sMLotQty;
        //    MtrlLotInfo.WORKER = sWorker;
        //    MtrlLotInfo.NOTE = sNote;
        //    MtrlLotInfo.PARENTS = sParents;
        //    MtrlLotInfo.dtMLOTINFO = dtMLotInfo;
        //}

        ////자재정보 DATA SET
        //public static void SetDTMtrlLotInfo(DataTable dt, string sWorker)
        //{
        //    DTMtrlLotInfo.dtMtrlLotInfo = dt;
        //    DTMtrlLotInfo.WORKER = sWorker;
        //}

        ///// <summary>
        ///// Pellicle지정(PopUp용)
        ///// </summary>
        //public static void SetMtrlPellicle(string sOrgCode, string sLotId, string sGdName, string sProdLotId, string sAssgnProcId, string sNote, string sWorker)
        //{
        //    MtrlPellicle.ORGCODE = sOrgCode;
        //    MtrlPellicle.LOTID = sLotId;
        //    MtrlPellicle.GDNAME = sGdName;
        //    MtrlPellicle.PRODLOTID = sProdLotId;
        //    MtrlPellicle.ASSGNPROCID = sAssgnProcId;
        //    MtrlPellicle.NOTE = sNote;
        //    MtrlPellicle.WORKER = sWorker;
        //}

        //public static void SetNoworkInfo(string procID, string eqptName, string eqptID, string search_Date)
        //{
        //    NoworkInfo.PROCID = procID;
        //    NoworkInfo.EQPTNAME = eqptName;
        //    NoworkInfo.EQPTID = eqptID;
        //    NoworkInfo.SEARCH_DATE = search_Date;
        //}

        ////출하성적서 항목 복사용
        //public static void SetShipReportInfo(string orgCode, string areaID, string prodClass1, string prodClass2, string prodClass3, string prodID
        //                                   , string prodName, string vendorID, string vendorName, string fileName, DataTable dtProd)
        //{
        //    ShipReportInfo.ORGCODE = orgCode;
        //    ShipReportInfo.AREAID = areaID;
        //    ShipReportInfo.PROD_CLASS1 = prodClass1;
        //    ShipReportInfo.PROD_CLASS2 = prodClass2;
        //    ShipReportInfo.PROD_CLASS3 = prodClass3;
        //    ShipReportInfo.PRODID = prodID;
        //    ShipReportInfo.PRODNAME = prodName;
        //    ShipReportInfo.VENDORID = vendorID;
        //    ShipReportInfo.VENDORNAME = vendorName;
        //    ShipReportInfo.FILENAME = fileName;
        //    ShipReportInfo.dtPRODUCT = dtProd;
        //}

        //// 자재현황 - 자재정보변경
        //public static void SetMtrlInfoChange(string orgCode, string mlotID, string supplierLot, string coaLevel, string sptCond
        //                                   , string sideTransFlag, string mlotStName, string mlotStCode, string mlotTpCode)
        //{
        //    MtrlInfoChange.ORGCODE = orgCode;
        //    MtrlInfoChange.MLOTID = mlotID;
        //    MtrlInfoChange.SUPPLIER_LOTID = supplierLot;
        //    MtrlInfoChange.COA_LEVEL = coaLevel;
        //    MtrlInfoChange.SPT_COND_CODE = sptCond;
        //    MtrlInfoChange.SIDE_TRANS_FLAG = sideTransFlag;
        //    MtrlInfoChange.MLOT_ST_NAME = mlotStName;
        //    MtrlInfoChange.MLOT_ST_CODE = mlotStCode;
        //    MtrlInfoChange.MLOT_TP_CODE = mlotTpCode;
        //}

        //// 출하정보(TS)
        //public static void SetPackListInfo(string vendCode, string vendName, long lotQty, long glotQty, long blotQty
        //        , double lotLenght, double glotLength, double blotLenght, string yield, string lotName, string model
        //        , string oqcRank, string slitPosition, string custLotid, string tsType, string shipLotID)
        //{
        //    PackListInfo.VENDCODE = vendCode;
        //    PackListInfo.VENDNAME = vendName;
        //    PackListInfo.LOTQTY = lotQty;
        //    PackListInfo.GLOTQTY = glotQty;
        //    PackListInfo.BLOTQTY = blotQty;
        //    PackListInfo.LOTLENGTH = lotLenght;
        //    PackListInfo.GLOTLENGTH = glotLength;
        //    PackListInfo.BLOTLENGTH = blotLenght;
        //    PackListInfo.YIELD = yield;
        //    PackListInfo.LOTNAME = lotName;
        //    PackListInfo.MODEL = model;
        //    PackListInfo.OQCRANK = oqcRank;
        //    PackListInfo.SLITPOSITION = slitPosition;
        //    PackListInfo.CUST_LOTID = custLotid;
        //    PackListInfo.TS_TYPE = tsType;

        //    PackListInfo.SHIPPING_LOTID = shipLotID;
        //}

        ///// <summary>
        ///// 생산계획 후 LOT생성(TS)
        ///// </summary>
        //public static void SetPoodPlanLotCreate(string sPrdtnYMD, string sAreaID, string sAreaNM, string sProdClass2ID, string sProdClass2NM, string sParents)
        //{
        //    PoodPlanLotCreate.PRDTNYMD = sPrdtnYMD;
        //    PoodPlanLotCreate.AREAID = sAreaID;
        //    PoodPlanLotCreate.AREANM = sAreaNM;
        //    PoodPlanLotCreate.PRODCLASS2ID = sProdClass2ID;
        //    PoodPlanLotCreate.PRODCLASS2NM = sProdClass2NM;
        //    PoodPlanLotCreate.PARENTS = sParents;
        //}

        ///// <summary>
        ///// Label Print(TS)
        ///// </summary>
        //public static void SetTSLabelPrint(DataTable dtPrintInfo, string sBarCodeNo, string sWorker, string sParents, string sOrgCode = "")
        //{
        //    TSLabelPrint.PRINTINFO = dtPrintInfo;
        //    TSLabelPrint.BARCODENO = sBarCodeNo;
        //    TSLabelPrint.WORKER = sWorker;
        //    TSLabelPrint.PARENTS = sParents;
        //    TSLabelPrint.ORGCODE = sOrgCode;
        //}

        ///// <summary>
        ///// DataTable Global
        ///// </summary>
        ///// <param name="dtGlobal"></param>
        ///// <param name="sWorker"></param>
        //public static void SetDtGlobla(DataTable dtGlobal, string sWorker)
        //{
        //    DTGlobal.DTGLOBAL = dtGlobal;
        //    DTGlobal.WORKER = sWorker;
        //}

        ////// <summary>
        ////// Combobox에 Process 정보 조회
        ////// </summary>
        ////// <param name="cmbObj">ComboBox Object</param>
        ////public static void setComboBoxOfProc(ComboBox cmbObj, DataSet dsRet, bool bAll, string sCon)
        ////{
        ////    if (bAll)
        ////    {
        ////        DataRow dr1 = dsRet.Tables["OUTDATA"].NewRow();
        ////        dr1["PROC_ID"] = 0;
        ////        dr1["PROC_CODE"] = System.DBNull.Value;
        ////        dr1["PROC_NAME"] = sCon;
        ////        dr1["PROC_DESC"] = System.DBNull.Value;
        ////        dr1["IUSE"] = System.DBNull.Value;
        ////        dr1["ORG_NAME"] = System.DBNull.Value;
        ////        dr1["ORG_ID"] = System.DBNull.Value;

        ////        dsRet.Tables["OUTDATA"].Rows.InsertAt(dr1, 0);
        ////    }

        ////    cmbObj.DataSource = dsRet.Tables["OUTDATA"];
        ////    cmbObj.DisplayMember = "PROC_NAME";
        ////    cmbObj.ValueMember = "PROC_ID";
        ////}

        ////// <summary>
        //////  comboBox에서 value에 해당되는 Display Name 자동 선택
        ////// </summary>
        ////// <param name="cmbxObj">ComboBox</param>
        ////// <param name="sValue">비교 value</param>
        ////public static void selectComboBoxValue(ComboBox cmbxObj, string sValue)
        ////{
        ////    int IndexNum = 0;

        ////    for (int i = 0; i < cmbxObj.Items.Count; i++) //이전 선택한 공정 정보와 비교
        ////    {
        ////        cmbxObj.SelectedIndex = i;

        ////        if (sValue == cmbxObj.SelectedValue.ToString())
        ////        {
        ////            IndexNum = i;
        ////            break;
        ////        }
        ////    }

        ////    cmbxObj.SelectedIndex = IndexNum;
        ////}

        ///// <summary>
        ///// 입력받은 lotid를 수정해서 반환함
        ///// </summary>
        ///// <param name="strTargetLotID"></param>
        ///// <returns></returns>
        //public string ConvertLotID(string strTargetLotID)
        //{
        //    strTargetLotID = strTargetLotID.Trim();

        //    return strTargetLotID;
        //}

        //#region MLOTATTR Grid View
        ///// <summary>
        ///// ORG 해당 MLOTATTR을 보여주자
        ///// </summary>
        ///// <param name="dataGrid"></param>
        ///// <param name="sOrgMlotAttr"></param>
        //public void SetOrgMlotAttrGrid(C1.WPF.DataGrid.C1DataGrid dataGrid, string[] sOrgMlotAttr)
        //{
        //    for (int iAttrCnt = 0; iAttrCnt < sOrgMlotAttr.Length; iAttrCnt++)
        //    {
        //        string sAttrNo = sOrgMlotAttr[iAttrCnt].ToString();
        //        dataGrid.Columns["" + sAttrNo + ""].Visibility = System.Windows.Visibility.Visible;
        //    }
        //}
        //#endregion

        //#region NetworkInformation 정보조회
        ///// <summary>
        ///// [0]-Domain: [1]-Host: [2]-IP
        ///// </summary>
        ///// <returns></returns>
        //public string GetNetworkInfo()
        //{
        //    string LocalIp = string.Empty;
        //    string Domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
        //    string Host = System.Net.Dns.GetHostName();

        //    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        //    {
        //        return null;
        //    }

        //    System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

        //    foreach (System.Net.IPAddress ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //        {
        //            LocalIp = ip.ToString();

        //            break;
        //        }
        //    }

        //    string IpWidHost = String.Format("{0}:{1}:{2}", Domain, Host, LocalIp);

        //    return IpWidHost;
        //}
        //#endregion

        //#region ListItem 정의
        //public class ComboItemList
        //{
        //    public String strValue = string.Empty;
        //    public String strDisplay = string.Empty;

        //    public ComboItemList()
        //    {
        //    }

        //    public ComboItemList(String strValue, String strDisplay)
        //    {
        //        this.strValue = strValue;
        //        this.strDisplay = strDisplay;
        //    }

        //    public override string ToString()
        //    {
        //        return strDisplay;
        //    }
        //}
        //#endregion

        //#region 월 마감상태
        //public enum MonthClogingStatus
        //{
        //    /// <summary>
        //    /// 마감 처리 시 이후 Data는 ERP로 전송되지 않습니다.
        //    /// </summary>
        //    CLOSE,
        //    /// <summary>
        //    /// 마감 취소는 기존 마감 월로 실적 처리됩니다.
        //    /// </summary>
        //    CLOSE_CANCEL,
        //    /// <summary>
        //    /// Open 시 누적된 실적이 신규 마감 월로 실적 처리됩니다.
        //    /// </summary>
        //    OPEN
        //}
        //#endregion

        //#region SELECT 구분(SINGLE, MULT)
        //public enum SelectGubun
        //{
        //    // 단일선택
        //    SINGLE,

        //    // 다중선택
        //    MULTI
        //}
        //#endregion

        //#region [※ CMM - Prod ComboBox 넣기(MULTI)]
        //public void SetProdMultValueCombo(MultiSelectionBox cbo, DataTable dtCboProduct)
        //{
        //    try
        //    {
        //        string sSelValue = string.Empty;
        //        DataTable dtProd = Util.ProdInfoCMM.dtPRODUCT;

        //        if (dtProd.Rows.Count > 0)
        //        {
        //            for (int idtCnt = 0; idtCnt < dtProd.Rows.Count; idtCnt++)
        //            {
        //                //기존에 존재하면 패스
        //                bool isNew = true;
        //                sSelValue = dtProd.Rows[idtCnt]["PRODID"].ToString();

        //                for (int i = 0; i < dtCboProduct.Rows.Count; i++)
        //                {
        //                    if (sSelValue == Util.NVC(dtCboProduct.Rows[i]["CBO_CODE"]))
        //                    {
        //                        isNew = false;
        //                    }
        //                }

        //                if (isNew)
        //                {
        //                    DataRow dr = dtCboProduct.NewRow();
        //                    dr["CBO_NAME"] = sSelValue;
        //                    dr["CBO_CODE"] = sSelValue;
        //                    dtCboProduct.Rows.InsertAt(dr, dtCboProduct.Rows.Count);
        //                }
        //            }

        //            cbo.ItemsSource = DataTableConverter.Convert(dtCboProduct);

        //            if (dtCboProduct.Rows.Count > 0)
        //            {
        //                cbo.CheckAll();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
        //    }
        //}
        //#endregion

        //#region [※ CMM - Prod ComboBox 넣기(SINGLE)]
        //public void SetProdSingleValueCombo(ComboBox cbo, DataTable dtCboProduct)
        //{
        //    try
        //    {
        //        string sSelValue = string.Empty;
        //        DataTable dtProd = Util.ProdInfoCMM.dtPRODUCT;

        //        if (dtProd.Rows.Count == 1)
        //        {
        //            //기존에 존재하면 패스
        //            bool isNew = true;
        //            sSelValue = dtProd.Rows[0]["PRODID"].ToString();

        //            for (int i = 0; i < dtCboProduct.Rows.Count; i++)
        //            {
        //                if (sSelValue == Util.NVC(dtCboProduct.Rows[i]["CBO_CODE"]))
        //                {
        //                    isNew = false;
        //                }
        //            }

        //            if (isNew)
        //            {
        //                DataRow dr = dtCboProduct.NewRow();
        //                dr["CBO_NAME"] = sSelValue;
        //                dr["CBO_CODE"] = sSelValue;
        //                dtCboProduct.Rows.InsertAt(dr, dtCboProduct.Rows.Count);

        //                cbo.ItemsSource = DataTableConverter.Convert(dtCboProduct);

        //                if (dtCboProduct.Rows.Count > 0)
        //                {
        //                    cbo.SelectedValue = sSelValue;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
        //    }
        //}
        //#endregion

        //#region [※ 소재 GRADE ComboBox 넣기]
        //public void SetGradeCombo(ComboBox cbo)
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
        //        dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

        //        DataRow row = dt.NewRow();
        //        row["CBO_CODE"] = "";
        //        row["CBO_NAME"] = "-SELECT-";
        //        dt.Rows.Add(row);

        //        row = dt.NewRow();
        //        row["CBO_CODE"] = "A";
        //        row["CBO_NAME"] = "A";
        //        dt.Rows.Add(row);

        //        row = dt.NewRow();
        //        row["CBO_CODE"] = "B";
        //        row["CBO_NAME"] = "B";
        //        dt.Rows.Add(row);

        //        row = dt.NewRow();
        //        row["CBO_CODE"] = "C";
        //        row["CBO_NAME"] = "C";
        //        dt.Rows.Add(row);

        //        cbo.ItemsSource = DataTableConverter.Convert(dt);
        //        cbo.SelectedIndex = 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
        //    }
        //}
        //#endregion

        #endregion

        #region Grid 값셋팅 + 조회결과 없을때 아래에 메세지
        public static void GridSetData(C1.WPF.DataGrid.C1DataGrid dataGrid, DataTable dt, IFrameOperation iFO, bool isAutoWidth = false)
        {
            gridClear(dataGrid);

            dataGrid.ItemsSource = DataTableConverter.Convert(dt);

            foreach (var col in dataGrid.FilteredColumns)
            {
                foreach (var filter in col.FilterState.FilterInfo)
                {
                    if (filter.FilterType == DataGridFilterType.Text)
                        filter.Value = string.Empty;
                }
            }

            if (dt.Rows.Count == 0)
            {
                if (iFO != null)
                    iFO.PrintFrameMessage("[" + System.DateTime.Now.ToString("hh:mm:ss") + "] " + MessageDic.Instance.GetMessage("SFU2816"));
            }
            else
            {
                if (iFO != null)
                    iFO.PrintFrameMessage(dt.Rows.Count + ObjectDic.Instance.GetObjectName("건"));

                if (isAutoWidth && dt.Rows.Count > 0)
                {
                    dataGrid.Loaded -= DataGridLoaded;

                    double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
                    double sumHeight = dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? dataGrid.MaxHeight : dataGrid.ActualHeight;
                    //double sumHeight = dataGrid.ActualHeight == 0 ? (dataGrid.Rows.Count * 25) : dataGrid.ActualHeight;

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

                    dataGrid.UpdateLayout();
                    dataGrid.Measure(new Size(sumWidth, sumHeight));

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                        if (dgc.ActualWidth > 0)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);

                    dataGrid.Loaded += DataGridLoaded;

                    /*
                    dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    dataGrid.UpdateLayout();

                    double gridWidth = dataGrid.Parent.
                    double sumColumnsWidth = dataGrid.Columns.Sum(x => x.ActualWidth);

                    if (gridWidth < sumColumnsWidth)
                    {
                        double weight = gridWidth / sumColumnsWidth;

                        foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                            dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.ActualWidth * weight , DataGridUnitType.Pixel);
                    }
                    else
                    {
                        dataGrid.ColumnWidth = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
                    }
                    */
                }
            }
        }

        private static void DataGridLoaded(object sender, RoutedEventArgs args)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            double sumWidth = dataGrid.Columns.Sum(x => x.Visibility == Visibility.Collapsed ? 0 : x.ActualWidth);
            double sumHeight = dataGrid.ActualHeight;

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                dgc.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);

            dataGrid.UpdateLayout();
            dataGrid.Measure(new Size(sumWidth, sumHeight));

            foreach (C1.WPF.DataGrid.DataGridColumn dgc in dataGrid.Columns)
                if (dgc.ActualWidth > 0)
                    dgc.Width = new C1.WPF.DataGrid.DataGridLength(dgc.IsReadOnly ? dgc.ActualWidth : (dgc.ActualWidth + 5), DataGridUnitType.Pixel);
        }

        public static void DataGridCheckAllChecked(C1DataGrid dg, bool ischeckType = true)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (ischeckType)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "Y");
                }
            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        public static void DataGridCheckAllUnChecked(C1DataGrid dg, bool ischeckType = true)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                if (ischeckType)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", "N");
                }

            }
            dg.EndEdit();
            dg.EndEditRow(true);
        }

        public static DataTable CheckBoxColumnAddTable(DataTable dt, bool isChecked)
        {
            try
            {
                var dtBinding = dt.Copy();
                dtBinding.Columns.Add(new DataColumn() { ColumnName = "CHK", DataType = typeof(bool) });

                foreach (DataRow row in dtBinding.Rows)
                {
                    row["CHK"] = isChecked;
                }
                dtBinding.AcceptChanges();

                return dtBinding;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }


        /// <summary>
        /// 2020-10-02 염규범S
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="dt"></param>
        /// <param name="bisChecked"></param>
        /// <param name="bAutoWidh"></param>
        /// <returns></returns>
        public static bool DataGridAddRow(C1DataGrid dg, DataTable dt, string strKeyOverLap, bool bAutoWidh)
        {
            try
            {
                DataTable dtDgTemp = DataTableConverter.Convert(dg.ItemsSource);

                if (dtDgTemp.Rows.Count > 0)
                {
                    string[] dgColNm = dtDgTemp.Columns.Cast<DataColumn>()
                        .Select(x => x.ColumnName)
                        .ToArray();

                    if (!dgColNm.Contains(strKeyOverLap))
                    {
                        Util.MessageInfo("");
                        return false;
                    }

                    string[] dtColNm = dt.Columns.Cast<DataColumn>()
                        .Select(x => x.ColumnName)
                        .ToArray();

                    if (!dgColNm.Contains(strKeyOverLap))
                    {
                        Util.MessageInfo("");
                        return false;
                    }

                    object[] arrDgOjArray = dtDgTemp.Select().Select(x => x[strKeyOverLap]).ToArray();
                    object[] arrDtOjArray = dt.Select().Select(x => x[strKeyOverLap]).ToArray();

                    var vList = new List<object>();
                    vList.Add(arrDgOjArray);
                    vList.Add(arrDtOjArray);




                    dtDgTemp.AsEnumerable().CopyToDataTable(dt, LoadOption.Upsert);
                }
                else
                {
                    Util.GridSetData(dg, dt, null, bAutoWidh);
                }

                if (!(dtDgTemp.Columns.Count == dt.Columns.Count))
                {
                    Util.MessageInfo("");
                    return false;
                }


                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #region [유형별 MessageBox]
        /// <summary>
        /// Message Id를 통하여 메세지 박스 호출(Validation 체크용 MessageBoxIcon 없이 호출 함.)
        /// </summary>
        /// <param name="messageId"></param>
        public static void MessageInfo(string messageId)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.Show(message, "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
        }

        /// C20170902_75124 - 폴딩공정 라벨발행 개선 : INS 염규범S
        /// <summary>
        /// Message Id를 통하여 메세지 박스 호출, AutoClosing 추가
        /// </summary>
        public static void MessageInfoAutoClosing(string messageId)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.showPrintFD(message, MessageBoxButton.OK, MessageBoxIcon.None, null);
        }

        /// <summary>
        /// Message Id와 파라메터를 통하여 메세지 박스 호출
        /// ex) Util.MessageInfo("10012", ObjectDic.Instance.GetObjectName("근무조"));
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="parameters"></param>
        public static void MessageInfo(string messageId, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
        }

        /// <summary>
        /// Message Id를 통하여 메세지 박스 호출(Validation 체크용 MessageBoxIcon 없이 호출 함.)
        /// </summary>
        /// <param name="messageId"></param>
        public static void MessageInfo(string messageId, Action<MessageBoxResult> callback)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.Show(message, "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }

        public static void MessageInfo(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }

        /// <summary>
        /// Validation MessageBox 호출 메소드
        /// MessageBoxIcon Warning 이미지외 다른 (Caution)이미지 대체용
        /// </summary>
        /// <param name="messageId"></param>
        public static void MessageValidation(string messageId)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
        }

        /// <summary>
        /// Validation MessageBox 호출 메소드
        /// MessageBoxIcon Warning 이미지외 다른 (Caution)이미지 대체용
        /// </summary>
        /// <param name="messageId"></param>
        public static void MessageValidation(string messageId, Action<MessageBoxResult> callback)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }

        /// <summary>
        /// Validation MessageBox 호출 메소드
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="parameters"></param>
        public static void MessageValidation(string messageId, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, null);
        }

        public static void MessageValidation(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            ControlsLibrary.MessageBox.Show(message, "", "Caution", MessageBoxButton.OK, MessageBoxIcon.None, callback);
        }

        /// <summary>
        /// Confirm 메세지 박스 호출
        /// ex) MessageConfirm("SFU1230",(result) => {if(result == MessageBoxResult.OK){실행 메소드}}); 삭제하시겠습니까?
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        public static void MessageConfirm(string messageId, Action<MessageBoxResult> callback)
        {
            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.Show(message, "", "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, callback);
        }

        public static void MessageConfirm(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId, parameters);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.Show(message, "", "Confirm", MessageBoxButton.OKCancel, MessageBoxIcon.None, callback);
        }

        public static void MessageConfirmByWarning(string messageId, Action<MessageBoxResult> callback, params object[] parameters)
        {
            string message = MessageDic.Instance.GetMessage(messageId, parameters);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            ControlsLibrary.MessageBox.Show(message, "", "Warning", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, callback);
        }

        /// <summary>
        /// Exception 전체(Biz Return Exception 및 프로그램 Exception 에 대한 다국어 처리)
        /// </summary>
        /// <param name="ex"></param>
        public static void MessageException(Exception ex, Action<MessageBoxResult> callback = null)
        {
            try
            {
                if (ex == null) return;

                //if (ex.Data.Contains("TYPE"))
                if (ex.Data.Contains("BIZ"))
                {
                    string conversionLanguage;
                    string exceptionMessage = ex.Message;
                    string exceptionParameter = "";
                    if (ex.Data.Contains("PARA"))
                    {
                        if(((string[])ex.Data["PARA"]).Length > 0)
                        {
                            foreach(string strPara in (string[])ex.Data["PARA"])
                            {
                                exceptionParameter += strPara + ":";
                            }
                            exceptionParameter = exceptionParameter.Substring(0, exceptionParameter.Length - 1);
                        }
                        else
                        {
                            exceptionParameter = ex.Data["PARA"].ToString();
                        } 
                    }

                    // Code 로 다국어 처리..
                    if (ex.Data.Contains("CODE"))
                    {
                        if (exceptionParameter.Equals(""))
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["CODE"]));
                        }
                        else
                        {
                            if (exceptionParameter.Contains(":"))
                            {
                                string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.None);

                                conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["CODE"]), parameterList);
                            }
                            else
                            {
                                conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["CODE"]), exceptionParameter);
                            }
                        }
                    }
                    else
                    {
                        if (exceptionParameter.Contains(":"))
                        {
                            string sOrg = exceptionMessage;
                            string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.None);

                            for (int i = parameterList.Length; i > 0; i--)
                            {
                                sOrg = sOrg.Replace(parameterList[i - 1], "%" + (i));
                            }

                            conversionLanguage = MessageDic.Instance.GetMessage(sOrg, parameterList);
                        }
                        else
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(exceptionMessage);
                        }
                    }

                    ControlsLibrary.MessageBox.Show(conversionLanguage, "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);

                    //                    if (ex.Data["TYPE"].ToString().Equals("USER"))
                    //                    {
                    //                        ControlsLibrary.MessageBox.Show(conversionLanguage, "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                    //                    }
                    //                    else if (ex.Data["TYPE"].ToString().Equals("LOGIC"))
                    //                    {
                    //                        string detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
                    //#if DEBUG
                    //                        detailMessage = detailMessage + Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
                    //#endif
                    //                        ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message.ToString()), detailMessage, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                    //                    }
                    //                    else if (ex.Data["TYPE"].ToString().Equals("SYSTEM"))
                    //                    {
                    //                        string detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
                    //#if DEBUG
                    //                        detailMessage = detailMessage + Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
                    //#endif
                    //                        ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message.ToString()), detailMessage, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                    //                    }
                }
                else
                {
                    //ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                    ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, "", "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                }
            }
            catch (Exception exception)
            {
                ControlsLibrary.MessageBox.ShowKeyEnter(exception.Message, exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
            }
        }

        public static void MessageExceptionNoEnter(Exception ex, Action<MessageBoxResult> callback = null)
        {
            try
            {
                if (ex == null) return;

                if (ex.Data.Contains("TYPE"))
                {
                    string conversionLanguage;
                    string exceptionMessage = ex.Message;
                    string exceptionParameter = "";
                    if (ex.Data.Contains("PARA"))
                    {
                        exceptionParameter = ex.Data["PARA"].ToString();
                    }

                    // Code 로 다국어 처리..
                    if (ex.Data.Contains("DATA"))
                    {
                        if (exceptionParameter.Equals(""))
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["DATA"]));
                        }
                        else
                        {
                            if (exceptionParameter.Contains(":"))
                            {
                                string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.None);

                                conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["DATA"]), parameterList);
                            }
                            else
                            {
                                conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["DATA"]), exceptionParameter);
                            }
                        }
                    }
                    else
                    {
                        if (exceptionParameter.Contains(":"))
                        {
                            string sOrg = exceptionMessage;
                            string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.None);

                            for (int i = parameterList.Length; i > 0; i--)
                            {
                                sOrg = sOrg.Replace(parameterList[i - 1], "%" + (i));
                            }

                            conversionLanguage = MessageDic.Instance.GetMessage(sOrg, parameterList);
                        }
                        else
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(exceptionMessage);
                        }
                    }


                    if (ex.Data["TYPE"].ToString().Equals("USER"))
                    {
                        ControlsLibrary.MessageBox.ShowNoEnter(conversionLanguage, "", "Info", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                    }
                    else if (ex.Data["TYPE"].ToString().Equals("LOGIC"))
                    {
                        string detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
#if DEBUG
                        detailMessage = detailMessage + Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
#endif
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage(ex.Message.ToString()), detailMessage, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                    }
                    else if (ex.Data["TYPE"].ToString().Equals("SYSTEM"))
                    {
                        string detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
#if DEBUG
                        detailMessage = detailMessage + Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
#endif
                        ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage(ex.Message.ToString()), detailMessage, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                    }
                }
                else
                {
                    ControlsLibrary.MessageBox.ShowNoEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
                }
            }
            catch (Exception exception)
            {
                ControlsLibrary.MessageBox.ShowNoEnter(exception.Message, exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, callback);
            }
        }

        public static string ConvertEmptyToNull(string stxtBoxValue)
        {
            string sReturnValue = stxtBoxValue == null ? String.Empty : stxtBoxValue.Trim();

            if (string.IsNullOrEmpty(sReturnValue))
                return null;
            else
                return sReturnValue;
        }
        #endregion

        #region

        public static DataTable Get_EqpLossCnt(string sEqptid)
        {
            DataTable dtResult = null;
            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                //inTable.Columns.Add("WRK_DATE_FROM", typeof(string));
                //inTable.Columns.Add("WRK_DATE_TO", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = sEqptid;
                //newRow["WRK_DATE_FROM"] = (DateTime.Now.AddDays(-2)).ToString("yyyyMMdd");
                //newRow["WRK_DATE_TO"] = DateTime.Now.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOSS_CNT", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        public static DataTable Get_EqpLossInfo(string sEqptid, string sProcid)
        {
            DataTable dtResult = null;
            try
            {
                /*
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WRK_DATE_FROM", typeof(string));
                inTable.Columns.Add("WRK_DATE_TO", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = sEqptid;
                newRow["WRK_DATE_FROM"] = (DateTime.Now.AddDays(-2)).ToString("yyyyMMdd");
                newRow["WRK_DATE_TO"] = DateTime.Now.ToString("yyyyMMdd");
                newRow["PROCID"] = sProcid;

                inTable.Rows.Add(newRow);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOSS_INPUT_INFO", "INDATA", "OUTDATA", inTable);
                */

                Get_EqpLossInfoFromToyyyyMMdd(sEqptid, sProcid, (DateTime.Now.AddDays(-2)).ToString("yyyyMMdd"), DateTime.Now.ToString("yyyyMMdd"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        public static DataTable Get_EqpLossInfoFromToyyyyMMdd(string sEqptid, string sProcid, string FromyyyyMMdd, string ToyyyyMMdd)
        {
            DataTable dtResult = null;
            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WRK_DATE_FROM", typeof(string));
                inTable.Columns.Add("WRK_DATE_TO", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = sEqptid;
                newRow["WRK_DATE_FROM"] = FromyyyyMMdd;
                newRow["WRK_DATE_TO"] = ToyyyyMMdd;
                newRow["PROCID"] = sProcid;

                inTable.Rows.Add(newRow);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOSS_INPUT_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }
        #endregion

        #region Notching ZPL Data Send
        public static bool SendZplBarcode(string sLotID, string sZplData)
        {

            try
            {
                string rootPath = string.Empty;
                // 김도형C님의 요청사항으로 TXT 파일로 변경
                string sFileName = sLotID + ".txt";
                FileInfo zplFile;

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "LOCAL")
                {
                    // xp 에는 user 라는 폴더가 없어서 테스트 불가로 인해서 임시 조치
                    // rootPath = Environment.GetFolderPath(Environment.SpecialFolder.) + "\\GMES\\SFU\\ZPL\\";
                    rootPath = "\\GMES\\SFU\\ZPL\\";

                    DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
                    if (!directoryInfo.Exists)
                        directoryInfo.Create();

                    zplFile = new FileInfo(rootPath + sFileName);
                }
                else
                {
                    rootPath = ConfigurationManager.AppSettings["GLOBAL_CONFIG_FILE_PATH"];
                    rootPath += "ZPL\\";

                    string[] directoryNames = rootPath.Split('\\');
                    string current = string.Empty;
                    for (int inx = 0; inx < directoryNames.Length - 1; inx++)
                    {
                        string directoryName = directoryNames[inx];
                        if (string.IsNullOrEmpty(current))
                        {
                            current = directoryName;
                        }
                        else
                        {
                            current += @"\" + directoryName;
                        }
                        current = current.Replace("C:", @"\\Client\C$");
                        DirectoryInfo directoryInfo = new DirectoryInfo(current);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                    }
                    zplFile = new FileInfo(current + @"\" + sFileName);
                    rootPath = current + @"\";
                }

                Logger.Instance.WriteLine("ZPL LOCAL PATH", rootPath);

                //DirectoryInfo di = new DirectoryInfo(rootPath);
                //if (di.Exists && di.GetFiles().Length > 0)
                //{
                //    Util.MessageValidation("ZPL파일이 존재합니다.");
                //    return false;
                //}

                //if (zplFile.Exists)
                //zplFile.Delete();

                // File Create
                using (FileStream fs = zplFile.Create())
                {
                    Byte[] info = new System.Text.UTF8Encoding(true).GetBytes(sZplData);
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name + " : " + ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return true;
        }
        #endregion


        #region 실적 확정 처리시 투입수량 > 양품수량 + 불량수량일 경우만 실적 처리 가능하도록 개선
        public static DataTable Get_ResultQty_Chk(string sLotid, string sProcid, string sEqptid, string sLineid, string sWipseq)
        {
            DataTable dtResult = null;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROD_LOTID", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("EQPTID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("PROD_WIPSEQ", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROD_LOTID"] = sLotid;
                dr["PROCID"] = sProcid;
                dr["EQPTID"] = sEqptid;
                dr["EQSGID"] = sLineid;
                dr["PROD_WIPSEQ"] = sWipseq;

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_QTY_CHK", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        public static DataTable Get_ConfirmQty(string sLotid, string sProcid, string sEqptid, string sLine)
        {
            DataTable dtResult = null;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("EQPTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["PROCID"] = sProcid;
                dr["EQSGID"] = sLine;
                dr["EQPTID"] = sEqptid;

                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_QTY", "INDATA", "OUTDATA", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }
        #endregion

        #region 조립 작업조건,품질정보 입력 확인
        public static bool EQPTCondition(string sProcid, string sEqsgid, string sEqptid, string sLotid)
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = sProcid;
                newRow["EQSGID"] = sEqsgid;
                newRow["EQPTID"] = sEqptid;
                newRow["LOTID"] = sLotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_EQPT_CONDITION_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CHK_FLAG"))
                    {
                        if (Util.NVC(dtRslt.Rows[0]["CHK_FLAG"]).Equals("Y"))
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }

        }
        public static bool EDCCondition(string sAreaid, string sProcid, string sEqsgid, string sEqptid, string sLotid)
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = sAreaid;
                newRow["PROCID"] = sProcid;
                newRow["EQSGID"] = sEqsgid;
                newRow["EQPTID"] = sEqptid;
                newRow["LOTID"] = sLotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_EDC_CONDITION_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CHK_FLAG"))
                    {
                        if (Util.NVC(dtRslt.Rows[0]["CHK_FLAG"]).Equals("Y"))
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region 활성화 후공정 라벨 발행
        public static bool PrintLabelPolymerForm(IFrameOperation frameOperation, LoadingIndicator loadingIndicator, DataTable dt, string printType, string resolution, string issueCount, string xposition, string yposition, string darkness, DataRow drPrintInfo)
        {
            try
            {
                const string bizRuleName = "DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON";


                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LBCD", typeof(string));
                inTable.Columns.Add("PRMK", typeof(string));
                inTable.Columns.Add("RESO", typeof(string));
                inTable.Columns.Add("PRCN", typeof(string));
                inTable.Columns.Add("MARH", typeof(string));
                inTable.Columns.Add("MARV", typeof(string));
                inTable.Columns.Add("DARK", typeof(string));
                inTable.Columns.Add("ATTVAL001", typeof(string));
                inTable.Columns.Add("ATTVAL002", typeof(string));
                inTable.Columns.Add("ATTVAL003", typeof(string));
                inTable.Columns.Add("ATTVAL004", typeof(string));
                inTable.Columns.Add("ATTVAL005", typeof(string));
                inTable.Columns.Add("ATTVAL006", typeof(string));
                inTable.Columns.Add("ATTVAL007", typeof(string));
                inTable.Columns.Add("ATTVAL008", typeof(string));
                inTable.Columns.Add("ATTVAL009", typeof(string));
                inTable.Columns.Add("ATTVAL010", typeof(string));
                inTable.Columns.Add("ATTVAL011", typeof(string));
                DataRow dr = inTable.NewRow();
                inTable.Rows.Add(dr);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        foreach (DataRow itemRow in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (string.Equals(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString(), dt.Rows[i]["LABEL_CODE"].GetString()))
                            {
                                printType = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                resolution = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                issueCount = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                xposition = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                yposition = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                darkness = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                drPrintInfo = itemRow;
                            }
                        }
                    }

                    inTable.Rows[0]["LBCD"] = dt.Rows[i]["LABEL_CODE"].GetString();
                    inTable.Rows[0]["PRMK"] = printType;
                    inTable.Rows[0]["RESO"] = resolution;
                    inTable.Rows[0]["PRCN"] = issueCount;
                    inTable.Rows[0]["MARH"] = xposition;
                    inTable.Rows[0]["MARV"] = yposition;
                    inTable.Rows[0]["DARK"] = darkness;
                    inTable.Rows[0]["ATTVAL001"] = dt.Rows[i]["ITEM001"].GetString();
                    inTable.Rows[0]["ATTVAL002"] = dt.Rows[i]["ITEM002"].GetString();
                    inTable.Rows[0]["ATTVAL003"] = dt.Rows[i]["ITEM003"].GetString();
                    inTable.Rows[0]["ATTVAL004"] = dt.Rows[i]["ITEM004"].GetString();
                    inTable.Rows[0]["ATTVAL005"] = dt.Rows[i]["ITEM005"].GetString();
                    inTable.Rows[0]["ATTVAL006"] = dt.Rows[i]["ITEM006"].GetString();
                    inTable.Rows[0]["ATTVAL007"] = dt.Rows[i]["ITEM007"].GetString();
                    inTable.Rows[0]["ATTVAL008"] = dt.Rows[i]["ITEM008"].GetString();
                    inTable.Rows[0]["ATTVAL009"] = dt.Rows[i]["ITEM009"].GetString();
                    inTable.Rows[0]["ATTVAL010"] = dt.Rows[i]["ITEM010"].GetString();
                    inTable.Rows[0]["ATTVAL011"] = dt.Rows[i]["ITEM011"].GetString();

                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(inTable);
                    //string xml = ds.GetXml();

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                    string zplCode = dtResult.Rows[0]["LABELCD"].ToString();

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return false;
                    }

                    System.Threading.Thread.Sleep(100);
                    if (LabelPrintzplPolymerForm(frameOperation, zplCode, dt.Rows[i]["LABEL_CODE"].GetString(), drPrintInfo))
                    {
                        // 양품 태그 라벨의 경우 이후 처리 로직 추가 필요 함.
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
            }
        }

        public static bool LabelPrintzplPolymerForm(IFrameOperation frameOperation, string zplText, string labelCode, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                frameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));
                return false;
            }

            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                bool isReturnPrint = true;
                if (drPrtInfo["PORTNAME"].GetString().Contains("USB"))
                {
                    DataRow[] drInfo = LoginInfo.CFG_SERIAL_PRINT.Select("LABELID = '" + labelCode.Trim() + "'");
                    isReturnPrint = drInfo.Length > 0 ? frameOperation.PrintUsbBarCodeLabel(zplText, labelCode) : frameOperation.PrintUsbBarCodeLabelByUniversalTransformationFormat8(zplText);
                }
                else if (drPrtInfo["PORTNAME"].GetString().Contains("LPT"))
                {
                    isReturnPrint = frameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zplText);
                }
                else if (drPrtInfo["PORTNAME"].GetString().Contains("COM"))
                {
                    isReturnPrint = frameOperation.Barcode_ZPL_Print(drPrtInfo, zplText);
                }
                else
                {
                    isReturnPrint = frameOperation.Barcode_ZPL_Print(drPrtInfo, zplText);
                }

                if (isReturnPrint == false)
                {
                    frameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                }

                return isReturnPrint;
            }

            return true;
        }

        #endregion
        #region 남경 원/각형 라벨 발행
        public static bool PrintLabelPacking(IFrameOperation frameOperation, LoadingIndicator loadingIndicator, DataTable dt, string printType, string resolution, string issueCount, string xposition, string yposition, string darkness, DataRow drPrintInfo)
        {
            try
            {
                const string bizRuleName = "DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON";


                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LBCD", typeof(string));
                inTable.Columns.Add("PRMK", typeof(string));
                inTable.Columns.Add("RESO", typeof(string));
                inTable.Columns.Add("PRCN", typeof(string));
                inTable.Columns.Add("MARH", typeof(string));
                inTable.Columns.Add("MARV", typeof(string));
                inTable.Columns.Add("DARK", typeof(string));
                inTable.Columns.Add("ATTVAL001", typeof(string));
                inTable.Columns.Add("ATTVAL002", typeof(string));
                inTable.Columns.Add("ATTVAL003", typeof(string));
                inTable.Columns.Add("ATTVAL004", typeof(string));
                inTable.Columns.Add("ATTVAL005", typeof(string));
                inTable.Columns.Add("ATTVAL006", typeof(string));
                inTable.Columns.Add("ATTVAL007", typeof(string));
                inTable.Columns.Add("ATTVAL008", typeof(string));
                inTable.Columns.Add("ATTVAL009", typeof(string));
                inTable.Columns.Add("ATTVAL010", typeof(string));
                inTable.Columns.Add("ATTVAL011", typeof(string));
                inTable.Columns.Add("ATTVAL012", typeof(string));
                inTable.Columns.Add("ATTVAL013", typeof(string));
                inTable.Columns.Add("ATTVAL014", typeof(string));
                inTable.Columns.Add("ATTVAL015", typeof(string));
                inTable.Columns.Add("ATTVAL016", typeof(string));
                inTable.Columns.Add("ATTVAL017", typeof(string));
                DataRow dr = inTable.NewRow();
                inTable.Rows.Add(dr);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        foreach (DataRow itemRow in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (string.Equals(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString(), dt.Rows[i]["LABEL_CODE"].GetString()))
                            {
                                printType = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                resolution = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                issueCount = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                xposition = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                yposition = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                darkness = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                drPrintInfo = itemRow;
                            }
                        }
                    }

                    inTable.Rows[0]["LBCD"] = dt.Rows[i]["LABEL_CODE"].GetString();
                    inTable.Rows[0]["PRMK"] = printType;
                    inTable.Rows[0]["RESO"] = resolution;
                    inTable.Rows[0]["PRCN"] = issueCount;
                    inTable.Rows[0]["MARH"] = xposition;
                    inTable.Rows[0]["MARV"] = yposition;
                    inTable.Rows[0]["DARK"] = darkness;
                    inTable.Rows[0]["ATTVAL001"] = dt.Rows[i]["ITEM001"].GetString();
                    inTable.Rows[0]["ATTVAL002"] = dt.Rows[i]["ITEM002"].GetString();
                    inTable.Rows[0]["ATTVAL003"] = dt.Rows[i]["ITEM003"].GetString();
                    inTable.Rows[0]["ATTVAL004"] = dt.Rows[i]["ITEM004"].GetString();
                    inTable.Rows[0]["ATTVAL005"] = dt.Rows[i]["ITEM005"].GetString();
                    inTable.Rows[0]["ATTVAL006"] = dt.Rows[i]["ITEM006"].GetString();
                    inTable.Rows[0]["ATTVAL007"] = dt.Rows[i]["ITEM007"].GetString();
                    inTable.Rows[0]["ATTVAL008"] = dt.Rows[i]["ITEM008"].GetString();
                    inTable.Rows[0]["ATTVAL009"] = dt.Rows[i]["ITEM009"].GetString();
                    inTable.Rows[0]["ATTVAL010"] = dt.Rows[i]["ITEM010"].GetString();
                    inTable.Rows[0]["ATTVAL011"] = dt.Rows[i]["ITEM011"].GetString();
                    inTable.Rows[0]["ATTVAL012"] = dt.Rows[i]["ITEM012"].GetString();
                    inTable.Rows[0]["ATTVAL013"] = dt.Rows[i]["ITEM013"].GetString();
                    inTable.Rows[0]["ATTVAL014"] = dt.Rows[i]["ITEM014"].GetString();
                    inTable.Rows[0]["ATTVAL015"] = dt.Rows[i]["ITEM015"].GetString();
                    inTable.Rows[0]["ATTVAL016"] = dt.Rows[i]["ITEM016"].GetString();
                    inTable.Rows[0]["ATTVAL017"] = dt.Rows[i]["ITEM017"].GetString();


                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(inTable);
                    //string xml = ds.GetXml();

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                    string zplCode = dtResult.Rows[0]["LABELCD"].ToString();

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return false;
                    }

                    System.Threading.Thread.Sleep(100);
                    if (LabelPrintzplPolymerForm(frameOperation, zplCode, dt.Rows[i]["LABEL_CODE"].GetString(), drPrintInfo))
                    {

                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
            }
        }
        #endregion

        #region 원자재 라벨 발행
        public static bool PrintLabelMLOT(IFrameOperation frameOperation, LoadingIndicator loadingIndicator, DataTable dt, string printType, string resolution, string issueCount, string xposition, string yposition, string darkness, DataRow drPrintInfo)
        {
            try
            {
                const string bizRuleName = "DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON";


                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LBCD", typeof(string));
                inTable.Columns.Add("PRMK", typeof(string));
                inTable.Columns.Add("RESO", typeof(string));
                inTable.Columns.Add("PRCN", typeof(string));
                inTable.Columns.Add("MARH", typeof(string));
                inTable.Columns.Add("MARV", typeof(string));
                inTable.Columns.Add("DARK", typeof(string));
                inTable.Columns.Add("ATTVAL001", typeof(string));
                inTable.Columns.Add("ATTVAL002", typeof(string));
                inTable.Columns.Add("ATTVAL003", typeof(string));
                inTable.Columns.Add("ATTVAL004", typeof(string));
                inTable.Columns.Add("ATTVAL005", typeof(string));
                inTable.Columns.Add("ATTVAL006", typeof(string));
                inTable.Columns.Add("ATTVAL007", typeof(string));
                inTable.Columns.Add("ATTVAL008", typeof(string));
                inTable.Columns.Add("ATTVAL009", typeof(string));
                inTable.Columns.Add("ATTVAL010", typeof(string));
                inTable.Columns.Add("ATTVAL011", typeof(string));
                DataRow dr = inTable.NewRow();
                inTable.Rows.Add(dr);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        foreach (DataRow itemRow in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (Convert.ToBoolean(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                            {
                                printType = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                resolution = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                issueCount = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                xposition = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                yposition = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                darkness = itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                drPrintInfo = itemRow;
                            }
                        }
                    }

                    inTable.Rows[0]["LBCD"] = dt.Rows[i]["LABEL_CODE"].GetString();
                    inTable.Rows[0]["PRMK"] = printType;
                    inTable.Rows[0]["RESO"] = resolution;
                    inTable.Rows[0]["PRCN"] = issueCount;
                    inTable.Rows[0]["MARH"] = xposition;
                    inTable.Rows[0]["MARV"] = yposition;
                    inTable.Rows[0]["DARK"] = darkness;
                    inTable.Rows[0]["ATTVAL001"] = dt.Rows[i]["ITEM001"].GetString();
                    inTable.Rows[0]["ATTVAL002"] = dt.Rows[i]["ITEM002"].GetString();
                    inTable.Rows[0]["ATTVAL003"] = dt.Rows[i]["ITEM003"].GetString();
                    inTable.Rows[0]["ATTVAL004"] = dt.Rows[i]["ITEM004"].GetString();
                    inTable.Rows[0]["ATTVAL005"] = dt.Rows[i]["ITEM005"].GetString();
                    inTable.Rows[0]["ATTVAL006"] = dt.Rows[i]["ITEM006"].GetString();
                    inTable.Rows[0]["ATTVAL007"] = dt.Rows[i]["ITEM007"].GetString();
                    inTable.Rows[0]["ATTVAL008"] = dt.Rows[i]["ITEM008"].GetString();
                    inTable.Rows[0]["ATTVAL009"] = dt.Rows[i]["ITEM009"].GetString();
                    inTable.Rows[0]["ATTVAL010"] = dt.Rows[i]["ITEM010"].GetString();
                    inTable.Rows[0]["ATTVAL011"] = dt.Rows[i]["ITEM011"].GetString();

                    //DataSet ds = new DataSet();
                    //ds.Tables.Add(inTable);
                    //string xml = ds.GetXml();

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                    string zplCode = dtResult.Rows[0]["LABELCD"].ToString();

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return false;
                    }

                    System.Threading.Thread.Sleep(100);
                    if (LabelPrintzplPolymerForm(frameOperation, zplCode, dt.Rows[i]["LABEL_CODE"].GetString(), drPrintInfo))
                    {
                        // 양품 태그 라벨의 경우 이후 처리 로직 추가 필요 함.
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
            }
        }
        #endregion


        #region PACK PLT 라벨 발행
        /// <summary>
        /// FrameOperation   : 프레임의 동기화를 위해서, 호출
        /// loadingIndicator : 작업중 표시 처리
        /// strLblCode       : null 로 선언시, 기본값은 PLT LBL 인 LBL0247
        /// </summary>
        /// <param name="FrameOperation"></param>
        /// <param name="loadingIndicator"></param>
        /// <param name="dtIndata"></param>
        /// <param name="strLblCode"></param>
        public static void labelPrint(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator, DataTable dtIndata)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                string strPrintType;
                string strResolution;
                string strIssueCount;
                string strXposition;
                string strYposition;
                string strDarkness;
                string strPortName;

                DataRow drPrintInfo;

                //체크된 라벨 정보 확인
                if (!GetConfigPrintInfoPack(out strPrintType, out strResolution, out strIssueCount, out strXposition, out strYposition, out strDarkness, out strPortName, out drPrintInfo))
                    return;

                //인쇄 요청
                Util.PrintLabePackPLT(FrameOperation, loadingIndicator, dtIndata, strPrintType, strResolution, strIssueCount, strXposition, strYposition, strDarkness, strPortName, drPrintInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        public static bool GetConfigPrintInfoPack(out string sPrt, out string sRes, out string sCopy, out string sXpos, out string sYpos, out string sDark, out string sPortName, out DataRow drPrtInfo)
        {
            try
            {
                sPrt = "";
                sRes = "";
                sCopy = "";
                sXpos = "";
                sYpos = "";
                sDark = "";
                sPortName = "";
                drPrtInfo = null;

                if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
                {
                    MessageValidation("SFU2003");
                    return false;
                }

                DataRow[] drTemp = LoginInfo.CFG_SERIAL_PRINT.Select("DEFAULT = 'True'");
                if (drTemp == null || drTemp.Length < 1)
                {
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
                    {
                        sPrt = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["PRINTERTYPE"].ToString().Equals("") ? "Z" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["PRINTERTYPE"].ToString();
                        sRes = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DPI"].ToString().Equals("") ? "203" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DPI"].ToString();
                        sCopy = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["COPIES"].ToString().Equals("") ? "1" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["COPIES"].ToString();
                        sXpos = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["X"].ToString().Equals("") ? "0" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["X"].ToString();
                        sYpos = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["Y"].ToString().Equals("") ? "0" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["Y"].ToString();
                        sDark = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DARKNESS"].ToString().Equals("") ? "15" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["DARKNESS"].ToString();
                        sPortName = LoginInfo.CFG_SERIAL_PRINT.Rows[0]["PORTNAME"].ToString().Equals("") ? "15" : LoginInfo.CFG_SERIAL_PRINT.Rows[0]["PORTNAME"].ToString();

                        drPrtInfo = LoginInfo.CFG_SERIAL_PRINT.Rows[0];
                    }

                    else
                    {
                        MessageValidation("SFU2003");
                        return false;
                    }
                }
                else
                {
                    sPrt = drTemp[0]["PRINTERTYPE"].ToString().Equals("") ? "Z" : drTemp[0]["PRINTERTYPE"].ToString();
                    sRes = drTemp[0]["DPI"].ToString().Equals("") ? "203" : drTemp[0]["DPI"].ToString();
                    sCopy = drTemp[0]["COPIES"].ToString().Equals("") ? "1" : drTemp[0]["COPIES"].ToString();
                    sXpos = drTemp[0]["X"].ToString().Equals("") ? "0" : drTemp[0]["X"].ToString();
                    sYpos = drTemp[0]["Y"].ToString().Equals("") ? "0" : drTemp[0]["Y"].ToString();
                    sDark = drTemp[0]["DARKNESS"].ToString().Equals("") ? "15" : drTemp[0]["DARKNESS"].ToString();
                    sPortName = drTemp[0]["PORTNAME"].ToString().Equals("") ? "15" : drTemp[0]["PORTNAME"].ToString();
                    drPrtInfo = drTemp[0];
                }
                return true;
            }
            catch (Exception ex)
            {
                sPrt = "";
                sRes = "";
                sCopy = "";
                sXpos = "";
                sYpos = "";
                sDark = "";
                sPortName = "";
                drPrtInfo = null;

                Util.MessageException(ex);
                return false;
            }
        }



        public static bool PrintLabePackPLT(IFrameOperation FrameOperation, LoadingIndicator loadingIndicator,
                        DataTable dt, string strPrintType, string strResolution, string strIssueCount, string strXposition, string strYposition, string strDarkness, string strPortName, DataRow drPrintInfo)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                const string bizRuleName = "DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON";

                //프린트 ITEM 갯수 구하기
                int iDtColCnt = dt.Columns.Cast<DataColumn>()
                               .Where(x => x.ColumnName.StartsWith("ITEM"))
                               .Select(x => x)
                               .ToArray().Length;

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LBCD", typeof(string));
                inTable.Columns.Add("PRMK", typeof(string));
                inTable.Columns.Add("RESO", typeof(string));
                inTable.Columns.Add("PRCN", typeof(string));
                inTable.Columns.Add("MARH", typeof(string));
                inTable.Columns.Add("MARV", typeof(string));
                inTable.Columns.Add("DARK", typeof(string));


                for (int i = 1; iDtColCnt >= i; i++)
                {
                    inTable.Columns.Add("ATTVAL" + string.Format("{0:D3}", i), typeof(string));
                }

                DataRow dr = inTable.NewRow();
                inTable.Rows.Add(dr);

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    inTable.Rows[i]["LBCD"] = dt.Rows[i]["LABEL_CODE"].GetString();
                    inTable.Rows[i]["PRMK"] = strPrintType;
                    inTable.Rows[i]["RESO"] = strResolution;
                    inTable.Rows[i]["PRCN"] = strIssueCount;
                    inTable.Rows[i]["MARH"] = strXposition;
                    inTable.Rows[i]["MARV"] = strYposition;
                    inTable.Rows[i]["DARK"] = strDarkness;

                    for (int j = 1; iDtColCnt >= j; j++)
                    {
                        inTable.Rows[i]["ATTVAL" + string.Format("{0:D3}", j)] = dt.Rows[i]["ITEM" + string.Format("{0:D3}", j)].GetString();
                    }

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                    string zplCode = dtResult.Rows[0]["LABELCD"].ToString();

                    if (zplCode.StartsWith("0,"))
                    {
                        zplCode = zplCode.Substring(2);

                        //인쇄 처리, false 일 경우 Message 보여주기만함
                        if (PrintLabelPackPlt(FrameOperation, loadingIndicator, drPrintInfo, strPortName, zplCode))
                        {
                            bool bRePrintFlag = false;

                            bRePrintFlag = IsRePirntYn(dt.Rows[i]["LOTID"].GetString(), dt.Rows[i]["LABEL_CODE"].GetString());

                            DataTable dtLbl = new DataTable();
                            dtLbl.TableName = "RQSTDT";
                            dtLbl.Columns.Add("LOTID", typeof(string));
                            dtLbl.Columns.Add("LABEL_CODE", typeof(string));
                            dtLbl.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                            dtLbl.Columns.Add("NOTE", typeof(string));
                            dtLbl.Columns.Add("REPRT_FLAG", typeof(string));
                            dtLbl.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                            dtLbl.Columns.Add("INSUSER", typeof(string));

                            for (int j = 1; iDtColCnt >= j; j++)
                            {
                                dtLbl.Columns.Add("PRT_ITEM" + string.Format("{0:D2}", j), typeof(string));
                            }

                            DataRow drHist = dtLbl.NewRow();
                            drHist["LOTID"] = dt.Rows[i]["LOTID"].GetString();
                            drHist["LABEL_CODE"] = dt.Rows[i]["LABEL_CODE"].GetString();
                            drHist["LABEL_ZPL_CNTT"] = zplCode;
                            drHist["NOTE"] = "";
                            drHist["REPRT_FLAG"] = bRePrintFlag == true ? "Y" : "N";
                            drHist["LABEL_PRT_COUNT"] = strIssueCount;
                            drHist["INSUSER"] = LoginInfo.USERID;

                            for (int j = 1; iDtColCnt >= j; j++)
                            {
                                drHist["PRT_ITEM" + string.Format("{0:D2}", j)] = dt.Rows[i]["ITEM" + string.Format("{0:D3}", j)].GetString();
                            }

                            dtLbl.Rows.Add(drHist);

                            new ClientProxy().ExecuteServiceSync("DA_PRD_INS_PACKLABEL_HIST", "RQSTDT", "RSLTDT", dtLbl);
                        }
                    }
                    else
                    {
                        Util.MessageInfo(zplCode.Substring(2));
                        return false;
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        public static bool IsRePirntYn(string strLotId, string strLblCode)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LOTID"] = strLotId;
                dr["LABEL_CODE"] = strLblCode;
                dt.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKLABEL_HIST", "RQSTDT", "RSLTDT", dt);

                if (dtResult.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion


        public static void DataGridRowAdd(C1DataGrid dg, int rowCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                dg.BeginNewRow();
                dg.EndNewRow(true);
            }
        }

        public static void DataGridRowDelete(C1DataGrid dg, int rowCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                DataRowView drv = dg.SelectedItem as DataRowView;
                if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                {
                    if (dg.SelectedIndex > -1)
                    {
                        dg.EndNewRow(true);
                        dg.RemoveRow(dg.SelectedIndex);
                    }
                }
            }
        }

        public static void DataGridRowEditByCheckBoxColumn(C1.WPF.DataGrid.DataGridBeginningEditEventArgs e, string[] exceptionColumns)
        {
            if (e?.Column != null)
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                {
                    e.Cancel = e.Column.Name != "CHK" && drv["CHK"].GetString() != "True";
                }
                else
                {
                    if (e.Column.Name == "CHK")
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        if (exceptionColumns != null)
                        {
                            e.Cancel = exceptionColumns.Contains(e.Column.Name) || drv != null && drv["CHK"].GetString() != "True";
                        }
                        else
                        {
                            e.Cancel = drv != null && drv["CHK"].GetString() != "True";
                        }
                    }
                }
            }

        }

        public bool IsCommonCodeUse(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }

        public bool IsCommonCodeUseAttr(string sCodeType, string sCodeName, string sAttr1 = null, string sAttr2 = null, string sAttr3 = null, string sAttr4 = null, string sAttr5 = null)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CBO_CODE"] = sCodeName;
                if (sAttr1 != null && !string.IsNullOrEmpty(sAttr1))
                    dr["ATTRIBUTE1"] = sAttr1;
                if (sAttr2 != null && !string.IsNullOrEmpty(sAttr2))
                    dr["ATTRIBUTE2"] = sAttr2;
                if (sAttr3 != null && !string.IsNullOrEmpty(sAttr3))
                    dr["ATTRIBUTE3"] = sAttr3;
                if (sAttr4 != null && !string.IsNullOrEmpty(sAttr4))
                    dr["ATTRIBUTE4"] = sAttr4;
                if (sAttr5 != null && !string.IsNullOrEmpty(sAttr5))
                    dr["ATTRIBUTE5"] = sAttr5;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex) { }

            return false;
        }

        /// <summary>
        /// 공통코드 Data Check
        /// </summary>
        /// <param name="sCodeType"></param>
        /// <param name="sCmCode"></param>
        /// <param name="sAttribute"></param>
        /// <returns></returns>
        /// CSR : C20220406-000241 [2022-05-17]
        public static Boolean IsCommoncodeAttrUse(string sCodeType, string sCmCode, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTRIBUTE1", "ATTRIBUTE2", "ATTRIBUTE3", "ATTRIBUTE4", "ATTRIBUTE5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                dr["CMCODE"] = (sCmCode == string.Empty ? null : sCmCode);
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL_BY_ATTRIBUTES", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // 2020.04.16
        // 염규범S
        public static DataTable GetGroupBySum(DataTable dt, string[] sumColumnNames, string[] groupByColumnNames, bool reAllColumn)
        {

            DataTable dt_Return = null;

            try
            {
                //Check datatable
                if (dt == null || dt.Rows.Count < 1) { return dt; }
                //Check sum columns
                if (sumColumnNames == null || sumColumnNames.Length < 1) { return dt; }
                //Check group columns
                if (groupByColumnNames == null || groupByColumnNames.Length < 1) { return dt; }

                //Create return datatable
                dt_Return = dt.DefaultView.ToTable(true, groupByColumnNames);

                //Set return Columns
                if (reAllColumn)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (!dt_Return.Columns.Contains(dt.Columns[i].ColumnName))
                        {
                            DataColumn dc = new DataColumn(dt.Columns[i].ColumnName, dt.Columns[i].DataType);
                            dt_Return.Columns.Add(dc);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < sumColumnNames.Length; i++)
                    {
                        if (dt.Columns.Contains(sumColumnNames[i]))
                        {
                            DataColumn dc = new DataColumn(sumColumnNames[i], dt.Columns[sumColumnNames[i]].DataType);
                            dt_Return.Columns.Add(dc);
                        }
                    }
                }

                //Summary Rows
                for (int i = 0; i < dt_Return.Rows.Count; i++)
                {
                    var sQuery = " 1=1 ";

                    foreach (var col in groupByColumnNames)
                    {
                        sQuery += "AND " + col + " = '" + dt_Return.Rows[i][col].ToString() + "'";
                    }

                    DataRow[] drs = dt.Select("(" + sQuery + ")");

                    foreach (var dr in drs)
                    {
                        foreach (var col in sumColumnNames)
                        {
                            int sum, val = 0;
                            int.TryParse(dt_Return.Rows[i][col].ToString().Replace(",", ""), out sum);
                            int.TryParse(dr[col].ToString().Replace(",", ""), out val);
                            dt_Return.Rows[i][col] = sum + val;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dt_Return;
        }

        public static DataTable getEquipmentAttr(string sEqptid)
        {
            DataTable dtResult = null;
            try
            {

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = sEqptid;

                inTable.Rows.Add(newRow);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }

        public static void PrintNoPreview(DataTable dtReport, string targetFile, string targetName)
        {
            try
            {
                C1.C1Report.C1Report reportDesign = null;

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[dtReport.Rows.Count];

                int index = 0;
                C1.C1Report.C1Report reportPage = null;
                foreach (DataRow drPrint in dtReport.Rows)
                {
                    reportPage = new C1.C1Report.C1Report();

                    using (Stream stream = assembly.GetManifestResourceStream(targetFile))
                    {
                        reportPage.Load(stream, targetName);

                        if (reportDesign == null)
                        {
                            reportDesign = reportPage;
                        }

                        // 다국어 처리
                        for (int cnt = 0; cnt < reportPage.Fields.Count; cnt++)
                        {
                            if (reportPage.Fields[cnt].Name == null) continue;
                            if (reportPage.Fields[cnt].Name.Substring(0, 4).Equals("Text"))
                            {
                                reportPage.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(reportPage.Fields[cnt].Text));
                            }
                        }

                        for (int col = 0; col < dtReport.Columns.Count; col++)
                        {
                            string strColName = dtReport.Columns[col].ColumnName;

                            if (reportPage.Fields.Contains(strColName))
                            {
                                reportPage.Fields[strColName].Text = Util.NVC(drPrint[strColName]);
                            }
                        }

                    }

                    reportPage.Render();
                    foreach (System.Drawing.Imaging.Metafile Page in reportPage.GetPageImages())
                    {
                        Pages.SetValue(Page, index);
                        index++;
                    }
                }

                reportDesign.C1Document.Body.Children.Clear();
                for (int i = 0; i <= (Pages.Length - 1); i++)
                {
                    C1.C1Preview.RenderImage PageImage = new C1.C1Preview.RenderImage(Pages[i]);
                    reportDesign.C1Document.Body.Children.Add(PageImage);
                    reportDesign.C1Document.Reflow();
                }

                var pm = new C1.C1Preview.C1PrintManager { Document = reportDesign };
                System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                    ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                pm.Print(ps);

                foreach (System.Drawing.Imaging.Metafile Page in reportPage.GetPageImages())
                {
                    Page.Dispose();
                }

                reportDesign.Dispose();
                GC.Collect();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //20211014 기능사용여부 체크(동별공통코드) START
        public bool IsAreaCommonCodeUse(string sComeCodeType, string sComeCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                dr["COM_CODE"] = sComeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals("Y", row["ATTR1"])) //20220502_오류수정
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }
        //20211014 기능사용여부 체크(동별공통코드) END

        /// <summary>
        /// 동별공통코드 조회
        /// </summary>
        /// <param name="sCodeType"></param>
        /// <param name="sCodeName"></param>
        /// <param name="sAttribute"></param>
        /// <returns></returns>
        public bool IsAreaCommoncodeAttrUse(string sCodeType, string sCodeName, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        /// <summary>
        /// 동별공통코드 속성값 조회
        /// </summary>
        /// <param name="sCodeType"></param>
        /// <param name="sCodeName"></param>
        /// <returns></returns>
        public DataTable GetAreaCommonCodeUse(string sComeCodeType, string sComeCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                dr["COM_CODE"] = sComeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception e)
            {
                Util.MessageException(e);
                return null;
            }
        }

        // Linq 결과 DataTable로 변환 - 팩 Project에서 쓰던거 Com Project에서도 사용하게 되어 갖다가 붙임.
        public static DataTable queryToDataTable(IEnumerable<dynamic> records)
        {
            DataTable dt = new DataTable();

            try
            {
                var firstRow = records.FirstOrDefault();
                if (firstRow == null)
                {
                    return null;
                }

                PropertyInfo[] propertyInfos = firstRow.GetType().GetProperties();
                foreach (var propertyinfo in propertyInfos)
                {
                    Type propertyType = propertyinfo.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        dt.Columns.Add(propertyinfo.Name, Nullable.GetUnderlyingType(propertyType));
                    }
                    else
                    {
                        dt.Columns.Add(propertyinfo.Name, propertyinfo.PropertyType);
                    }
                }

                foreach (var record in records)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dr[i] = propertyInfos[i].GetValue(record) != null ? propertyInfos[i].GetValue(record) : DBNull.Value;
                    }

                    dt.Rows.Add(dr);
                }

                dt.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dt;
        }

        // ComboBox Binding (CommonCombo Class 버려버리는용도임)
        public static void SetC1ComboBox(DataTable dt, C1ComboBox c1ComboBox, bool isEmptyDataBinding, string title = null)
        {
            List<string> lstValueMemberPathFilter = new List<string>() { "ID", "CODE", "CD", "USE_FLAG", "TYPE" };
            List<string> lstDisplayMemberPathFilter = new List<string>() { "NAME", "NM", "DESC" };

            if (!CommonVerify.HasTableRow(dt) && !isEmptyDataBinding)
            {
                return;
            }

            try
            {
                var selectedValuePath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstValueMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                var displayMemberPath = (from d1 in dt.Columns.OfType<DataColumn>()
                                         from d2 in lstDisplayMemberPathFilter
                                         select new
                                         {
                                             COLUMNNAME = d1.ColumnName,
                                             FILTER = d2
                                         }).Where(x => x.COLUMNNAME.Contains(x.FILTER) || x.COLUMNNAME.EndsWith(x.FILTER)).Select(x => x.COLUMNNAME).FirstOrDefault();

                if (selectedValuePath == null || displayMemberPath == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(title))
                {
                    DataRow dr = dt.NewRow();
                    switch (title.ToUpper())
                    {
                        case "ALL":
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = "-ALL-";
                            break;
                        case "SELECT":
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = "-SELECT-";
                            break;
                        case "N/A":
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = "-N/A-";
                            break;
                        default:
                            dr[selectedValuePath] = string.Empty;
                            dr[displayMemberPath] = title;
                            break;
                    }
                    dt.Rows.InsertAt(dr, 0);
                }

                c1ComboBox.SelectedValuePath = selectedValuePath;
                c1ComboBox.DisplayMemberPath = displayMemberPath;
                c1ComboBox.ItemsSource = dt.AsDataView();
                c1ComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 설비 Loss Level 2 Code 사용 체크 및 변환
        public static string ConvertEqptLossLevel2Change(string NewLossCode)
        {
            string returnCode = string.Empty;
            try
            {
                DataTable dtRqst1 = new DataTable();
                dtRqst1.TableName = "RQSTDT";
                dtRqst1.Columns.Add("LANGID", typeof(string));
                dtRqst1.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst1.Columns.Add("CMCODE", typeof(string));
                dtRqst1.Columns.Add("CMCDIUSE", typeof(string));

                DataRow drNew1 = dtRqst1.NewRow();
                drNew1["LANGID"] = LoginInfo.LANGID;
                drNew1["CMCDTYPE"] = "EQPT_LOSS_CODE_LEVEL2";
                drNew1["CMCODE"] = NewLossCode;
                drNew1["CMCDIUSE"] = "Y";
                dtRqst1.Rows.Add(drNew1);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", dtRqst1);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    DataRow drReturn = dtResult.Rows[0];
                    // 기본 이전코드 설정
                    returnCode = Util.NVC(drReturn["ATTRIBUTE3"]);

                    DataTable dtRqst2 = new DataTable();
                    dtRqst2.TableName = "RQSTDT";
                    dtRqst2.Columns.Add("LANGID", typeof(string));
                    dtRqst2.Columns.Add("CMCDTYPE", typeof(string));
                    dtRqst2.Columns.Add("CMCODE", typeof(string));
                    dtRqst2.Columns.Add("CMCDIUSE", typeof(string));

                    DataRow drNew2 = dtRqst2.NewRow();
                    drNew2["LANGID"] = LoginInfo.LANGID;
                    drNew2["CMCDTYPE"] = "EQPT_LOSS_CODE_APPLY_AREA";
                    drNew2["CMCODE"] = LoginInfo.CFG_AREA_ID;
                    drNew2["CMCDIUSE"] = "Y";
                    dtRqst2.Rows.Add(drNew2);

                    DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", dtRqst2);
                    if (dtResult2 != null && dtResult2.Rows.Count > 0 && !Util.NVC(dtResult2.Rows[0]["ATTRIBUTE1"]).Equals(string.Empty))
                    {
                        // 적용 동일때 새코드
                        returnCode = Util.NVC(drReturn["ATTRIBUTE1"]);
                    }
                    else
                    {
                        // 적용 동이 아닐때 이전코드
                        returnCode = Util.NVC(drReturn["ATTRIBUTE3"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnCode;
        }

        public static DateTime LossDataUnalterable_BaseDate(string AREAID)
        {
            DateTime dtBaseDate = new DateTime();

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("CMCODE", typeof(string));
                dtRqst.Columns.Add("CMCDIUSE", typeof(string));

                DataRow drNew = dtRqst.NewRow();
                drNew["LANGID"] = LoginInfo.LANGID;
                drNew["CMCDTYPE"] = "EQPT_LOSS_CODE_APPLY_AREA";
                drNew["CMCODE"] = Util.NVC(AREAID, LoginInfo.CFG_AREA_ID);
                drNew["CMCDIUSE"] = "Y";
                dtRqst.Rows.Add(drNew);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtResult.Rows.Count > 0)
                {
                    string baseDate = dtResult.Rows[0]["ATTRIBUTE5"].ToString();

                    if (!string.IsNullOrEmpty(baseDate))
                    {
                        dtBaseDate = Convert.ToDateTime(baseDate); // 2023-07-01 00:00:00.000
                    }
                    else
                    {
                        dtBaseDate = new DateTime();
                    }
                }
                else
                {
                    dtBaseDate = new DateTime();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtBaseDate;
        }

        public static string ExceptionMessageToString(Exception ex)
        {
            string rtnVal = string.Empty;

            if (ex == null) goto ExceptionToStringSkip;

            if (!ex.Data.Contains("TYPE"))
            {
                rtnVal = ex.Message;
                goto ExceptionToStringSkip;
            }

            try
            {
                string conversionLanguage;
                string exceptionMessage = ex.Message;
                string exceptionParameter = "";

                if (ex.Data.Contains("PARA")) exceptionParameter = ex.Data["PARA"].ToString();

                if (ex.Data.Contains("DATA"))
                {
                    if (exceptionParameter.Equals(""))
                    {
                        conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["DATA"]));
                    }
                    else
                    {
                        if (exceptionParameter.Contains(":"))
                        {
                            string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                            conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["DATA"]), parameterList);
                        }
                        else
                        {
                            conversionLanguage = MessageDic.Instance.GetMessage(NVC(ex.Data["DATA"]), exceptionParameter);
                        }
                    }
                }
                else
                {
                    if (exceptionParameter.Contains(":"))
                    {
                        string sOrg = exceptionMessage;
                        string[] parameterList = exceptionParameter.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);

                        for (int i = parameterList.Length; i > 0; i--) sOrg = sOrg.Replace(parameterList[i - 1], "%" + (i));

                        conversionLanguage = MessageDic.Instance.GetMessage(sOrg, parameterList);
                    }
                    else
                    {
                        conversionLanguage = MessageDic.Instance.GetMessage(exceptionMessage);
                    }
                }

                string _type = ex.Data["TYPE"].ToString();
                string detailMessage = string.Empty;

                switch (_type)
                {
                    case "USER":
                        rtnVal = conversionLanguage;
                        break;
                    case "LOGIC":
                        detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
                        rtnVal = detailMessage + Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
                        break;
                    case "SYSTEM":
                        detailMessage = ex.Data.Contains("LOC") ? ex.Data["LOC"].ToString() : "";
                        rtnVal = detailMessage + Environment.NewLine + (ex.Data.Contains("DATA") ? ex.Data["DATA"].ToString() : "");
                        break;
                    default:
                        rtnVal = conversionLanguage;
                        break;
                }

            }
            catch (Exception tryExpt)
            {
                rtnVal = tryExpt.Message;
            }

            // **************************************************************
            // ******    Exception Skip label
            // **************************************************************

            ExceptionToStringSkip:  

            return rtnVal;

        }

        // NFF용 Cell ID convert   
        public static string Convert_CellID(string sInputID)
        {

            string sCellID = string.Empty;

            if (string.IsNullOrEmpty(sInputID))
            {
                return string.Empty;
            }


            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("INPUTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["INPUTID"] = sInputID;
            dtRqst.Rows.Add(dr);
            
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CONV_SUBLOTID_MB", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt.Rows.Count == 0)
                return string.Empty;
            else
                sCellID = dtRslt.Rows[0]["SUBLOTID"].ToString();

            return sCellID;
        }

        #region RollMap UI 사용 Utility
        public static TextBlock CreateTextBlock(string textBlockText, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, int fontsize, FontWeight fontWeight, SolidColorBrush foreground, Thickness margine, Thickness padding, string textBlockName, Cursor cursor, string textBlockTag)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = textBlockText,
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment,
                FontSize = fontsize,
                FontWeight = fontWeight,
                Foreground = foreground,
                Margin = margine,
                Padding = padding,
                Name = textBlockName,
                Cursor = cursor,
                Tag = textBlockTag
            };

            return textBlock;
        }

        public static Rectangle CreateRectangle(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, double height, double width, SolidColorBrush fill, SolidColorBrush stroke, double strokeThickness, Thickness margine, string rectangleName, Cursor cursor)
        {
            Rectangle rectangle = new Rectangle
            {
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment,
                Height = height,
                Width = width,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness,
                Margin = margine,
                Name = "Rect" + rectangleName,
                Cursor = cursor
            };
            return rectangle;
        }

        public static Ellipse CreateEllipse(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, int height, int width, SolidColorBrush fill, double strokeThickness, Thickness margine, string ellipseName, Cursor cursor)
        {
            Ellipse ellipse = new Ellipse
            {
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment,
                Height = height,
                Width = width,
                Fill = fill,
                StrokeThickness = strokeThickness,
                Margin = margine,
                Name = "Ellipse" + ellipseName,
                Cursor = cursor
            };

            return ellipse;
        }

        #endregion



    }
}